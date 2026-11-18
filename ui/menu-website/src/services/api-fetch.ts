import type { OperationContract } from '@/generated/open-api/menu-api.contract';
import { ApiContractError, ApiError } from '@/services/api-error';

interface RequestInput {
  body?: unknown;
  path?: Record<string, string>;
  query?: Record<string, string | number | boolean | null | undefined>;
}
export type RequestOptions = Pick<RequestInit, 'signal'>;

const prepareRequest = (contract: OperationContract, input: RequestInput) => {
  const operation = contract.name;
  const { request } = contract;
  for (const part of ['body', 'path', 'query'] as const) {
    const schema = request[part];
    if (schema && !schema.safeParse(input[part]).success) {
      throw new ApiContractError(operation, `request ${part}`);
    }
  }
  const path = contract.path.replace(
    /\{([^{}]+)\}/g,
    (_match, name: string) => input.path?.[name] ?? '',
  );
  const search = new URLSearchParams();
  for (const [name, value] of Object.entries(input.query ?? {})) {
    if (value !== undefined) search.append(name, String(value));
  }

  let body: string | undefined;
  if (request.body) {
    try {
      body = JSON.stringify(input.body);
    } catch {
      throw new ApiContractError(operation, 'request body');
    }
    // Validate the wire representation as JSON serialization can change values.
    if (body === undefined || !request.body.safeParse(JSON.parse(body)).success) {
      throw new ApiContractError(operation, 'request body');
    }
  }

  const query = search.toString();
  const url = query ? `${path}?${query}` : path;
  return { url, body };
};

export const createApiRequest =
  (getAccessToken: () => Promise<string | undefined>) =>
  async <T>(
    contract: OperationContract,
    input: RequestInput,
    options?: RequestOptions,
  ): Promise<T> => {
    const { url, body } = prepareRequest(contract, input);
    const accessToken = await getAccessToken();
    if (!accessToken) throw new Error('API authentication did not return an access token.');
    const headers = new Headers({ Authorization: `Bearer ${accessToken}` });
    if (contract.request.body) headers.set('Content-Type', 'application/json');
    const baseUrl = (import.meta.env.VITE_MENU_API_URL ?? '').replace(/\/$/, '');
    const response = await fetch(`${baseUrl}${url}`, {
      signal: options?.signal,
      method: contract.method,
      headers,
      body,
      redirect: 'manual',
    });
    return (await validateResponse(contract, response)) as T;
  };
const validateResponse = async (
  contract: OperationContract,
  response: Response,
): Promise<unknown> => {
  const operation = contract.name;
  const { status } = response;
  const expected = contract.responses[status];
  if (!expected) {
    throw new ApiContractError(operation, 'status', status);
  }

  const mediaType = response.headers.get('Content-Type')?.split(';', 1)[0]?.trim().toLowerCase();
  const content = expected.content;
  const schema =
    content && mediaType && Object.hasOwn(content, mediaType) ? content[mediaType] : undefined;
  if (!schema && (mediaType || !expected.bodyless)) {
    throw new ApiContractError(operation, 'media type', status);
  }

  const text = await response.text();
  let data: unknown;
  if (text.length || !expected.bodyless) {
    if (!schema) throw new ApiContractError(operation, 'response body', status);
    try {
      data = JSON.parse(text);
    } catch {
      throw new ApiContractError(operation, 'response body', status);
    }
    const parsed = schema.safeParse(data);
    if (!parsed.success) {
      throw new ApiContractError(operation, 'response body', status);
    }
    data = parsed.data;
  }

  if (!response.ok) {
    throw ApiError.from(operation, data, response);
  }

  return data;
};
