import { readFile, writeFile } from 'node:fs/promises';
import { generate } from 'orval';
import { format, resolveConfig } from 'prettier';

const input = new URL('../../../open-api/menu-api.json', import.meta.url);
const output = new URL('../src/generated/open-api/', import.meta.url);
const document = JSON.parse(await readFile(input, 'utf8'));
const operations = new Map();
const methods = ['get', 'put', 'post', 'delete', 'options', 'head', 'patch', 'trace'];

for (const [path, item] of Object.entries(document.paths)) {
  for (const method of methods) {
    const operation = item[method];
    if (!operation) continue;
    const { operationId } = operation;
    if (
      typeof operationId !== 'string' ||
      !/^[a-z][A-Za-z0-9]*$/.test(operationId) ||
      operations.has(operationId)
    ) {
      throw new Error(
        `Missing, invalid or duplicate operationId for ${method.toUpperCase()} ${path}`,
      );
    }
    operations.set(operationId, {
      method: method.toUpperCase(),
      path,
      operation,
      parameters: [...(item.parameters ?? []), ...(operation.parameters ?? [])],
    });
  }
}

await generate('./orval.config.mjs', undefined, { throwOnError: true });

const validatorSource = await readFile(new URL('menu-api.zod.ts', output), 'utf8');
const validators = new Set(
  [...validatorSource.matchAll(/^export const (\w+)\s*=/gm)].map(([, name]) => name),
);

function useValidator(name) {
  if (!validators.has(name))
    throw new Error(`Orval did not generate the required validator ${name}`);
  return `schemas.${name}`;
}

function resolveReference(object) {
  if (!object.$ref) return object;
  if (!object.$ref.startsWith('#/')) {
    throw new Error(`Expected a reference within the backend OpenAPI document: ${object.$ref}`);
  }
  const target = object.$ref
    .slice(2)
    .split('/')
    .map((segment) => segment.replaceAll('~1', '/').replaceAll('~0', '~'))
    .reduce((node, segment) => node?.[segment], document);
  if (!target || target.$ref) throw new Error(`Expected a concrete target for ${object.$ref}`);
  return target;
}

function jsonMediaTypes(content, name) {
  const entries = Object.entries(content ?? {});
  for (const [mediaType, entry] of entries) {
    if (!/^application\/(?:[\w.-]+\+)?json$/.test(mediaType) || !entry.schema) {
      throw new Error(`Unsupported body media type for ${name}: ${mediaType}`);
    }
  }
  if (new Set(entries.map(([, entry]) => JSON.stringify(entry.schema))).size > 1) {
    throw new Error(`Orval cannot generate distinct validators per media type for ${name}`);
  }
  return entries.map(([mediaType]) => mediaType);
}

function describeOperation(operationId, { method, path, operation, parameters }) {
  const prefix = operationId[0].toUpperCase() + operationId.slice(1);
  const request = [];
  const responses = [];
  const parameterLocations = new Set(parameters.map((parameter) => resolveReference(parameter).in));

  if (parameterLocations.has('path')) request.push(`path: ${useValidator(prefix + 'Params')}`);
  if (parameterLocations.has('query')) {
    request.push(`query: ${useValidator(prefix + 'QueryParams')}`);
  }
  if (operation.requestBody) {
    const body = resolveReference(operation.requestBody);
    const mediaTypes = jsonMediaTypes(body.content, operationId);
    if (mediaTypes.length !== 1 || mediaTypes[0] !== 'application/json') {
      throw new Error(`Expected an application/json request body for ${operationId}`);
    }
    request.push(`body: ${useValidator(prefix + 'Body')}`);
  }

  for (const [status, entry] of Object.entries(operation.responses)) {
    if (!/^[1-5]\d{2}$/.test(status)) {
      throw new Error(`An explicit status code is required for ${operationId}: ${status}`);
    }
    const response = resolveReference(entry);
    const mediaTypes = jsonMediaTypes(response.content, `${operationId} ${status}`);
    const fields = [];
    if (mediaTypes.length === 0 || response['x-allow-empty-body'] === true) {
      fields.push('bodyless: true');
    }
    if (mediaTypes.length !== 0) {
      const validator = useValidator(`${prefix}${status}Response`);
      const media = mediaTypes.map((mediaType) => `${JSON.stringify(mediaType)}: ${validator}`);
      fields.push(`content: { ${media.join(', ')} }`);
    }
    responses.push(`${status}: { ${fields.join(', ')} }`);
  }

  return `
    ${JSON.stringify(operationId)}: {
      method: ${JSON.stringify(method)},
      path: ${JSON.stringify(path)},
      request: { ${request.join(', ')} },
      responses: { ${responses.join(', ')} },
    },
  `;
}

const contract = `
// Generated from the Menu API OpenAPI document. Do not edit.
import type { ZodMiniType } from 'zod/mini';
import * as schemas from './menu-api.zod';

export interface OperationContract {
  readonly method: string;
  readonly path: string;
  readonly request: {
    readonly body?: ZodMiniType;
    readonly path?: ZodMiniType;
    readonly query?: ZodMiniType;
  };
  readonly responses: Readonly<Record<number, {
    readonly bodyless?: true;
    readonly content?: Readonly<Record<string, ZodMiniType>>;
  }>>;
}

export const operationContracts = {
  ${[...operations].map(([operationId, operation]) => describeOperation(operationId, operation)).join('')}
} as const satisfies Record<string, OperationContract>;
`;

const contractFile = new URL('menu-api.contract.ts', output);
const prettierConfig = await resolveConfig(contractFile);
await writeFile(contractFile, await format(contract, { ...prettierConfig, parser: 'typescript' }));
