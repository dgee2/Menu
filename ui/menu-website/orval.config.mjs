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
      client: 'vue-query',
      httpClient: 'fetch',
      mock: {
        generators: [{ type: 'msw', delay: false }],
      },
      override: {
        mutator: {
          path: './src/services/api-fetch.ts',
          name: 'apiFetch',
        },
        fetch: {
          includeHttpResponseReturnType: false,
        },
        query: {
          signal: true,
          useInfinite: false,
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
