import { defineConfig } from 'orval';

const spec = '../../open-api/menu-api.json';
const outputDirectory = './src/generated/open-api';

function stripUntypedExtensions(document) {
  const result = structuredClone(document);
  for (const schema of Object.values(result.components?.schemas ?? {})) {
    // Treat untyped extensions on known objects like other unknown response fields.
    // Typed dictionaries, including the validation problem's errors map, are preserved.
    if (
      schema.properties &&
      schema.additionalProperties &&
      Object.keys(schema.additionalProperties).length === 0
    ) {
      delete schema.additionalProperties;
    }
  }
  return result;
}

export default defineConfig({
  menu: {
    input: spec,
    output: {
      mode: 'single',
      target: `${outputDirectory}/menu-api.ts`,
      schemas: `${outputDirectory}/model`,
      client: (clients) => ({
        ...clients.fetch,
        client: async (operation, options) => {
          const native = await clients.fetch.client(operation, options);
          const { operationName, operationId, props, response } = operation;
          const inputs = [];
          for (const prop of props) {
            if (prop.type === 'body') inputs.push('body: ' + prop.name);
            else if (prop.type === 'queryParam') inputs.push('query: ' + prop.name);
            else if (prop.type !== 'param')
              throw new Error('Unsupported request input: ' + prop.type);
          }
          const pathParams = props.filter((prop) => prop.type === 'param').map((prop) => prop.name);
          if (pathParams.length) inputs.push('path: { ' + pathParams.join(', ') + ' }');
          const args = [...props.map((prop) => prop.definition), 'options?: RequestOptions'].join(
            ', ',
          );
          const result = response.definition.success;
          const implementation = `const ${operationName} = (${args}): Promise<${result}> =>
            request<${result}>(operationContracts.${operationId}, { ${inputs.join(', ')} }, options);`;
          const identifiers = new Set(implementation.match(/\b\w+\b/g));
          return {
            implementation,
            imports: native.imports.filter((entry) => identifiers.has(entry.name)),
          };
        },
        header:
          () => `import { createApiRequest, type RequestOptions } from '../../services/api-fetch';
          import { operationContracts } from './menu-api.contract';
          export const createMenuApi = (getAccessToken: () => Promise<string | undefined>) => {
            const request = createApiRequest(getAccessToken);`,
        footer: ({ operationNames }) => 'return { ' + operationNames.join(', ') + ' }; };',
        dependencies: () => [],
        extraFiles: () => [],
      }),
      httpClient: 'fetch',
      mock: {
        generators: [{ type: 'msw', delay: false }],
      },
      override: {
        fetch: {
          includeHttpResponseReturnType: false,
        },
      },
    },
  },
  validators: {
    input: { target: spec, override: { transformer: stripUntypedExtensions } },
    output: {
      mode: 'single',
      target: `${outputDirectory}/menu-api.zod.ts`,
      client: 'zod',
      override: {
        zod: {
          variant: 'mini',
          version: 4,
          // SQL materialization can leave DateTime.Kind unspecified.
          dateTimeOptions: { offset: true, local: true },
          generate: { body: true, param: true, query: true, response: true },
          generateEachHttpStatus: true,
          generateReusableSchemas: true,
        },
      },
    },
  },
});
