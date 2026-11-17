import { operationContracts, type OperationContract } from '@/generated/open-api/menu-api.contract';
import { ApiContractError, ApiError } from '@/services/api-error';

type Operation = keyof typeof operationContracts;
type RequestInput = Partial<Record<'body' | 'path' | 'query', unknown>>;
const contractContext = Symbol('API contract context');

interface ApiFetchOptions extends RequestInit {
  [contractContext]?: {
    operation: Operation;
    getAccessToken: () => Promise<string>;
  };
}

const contracts: Record<Operation, OperationContract> = operationContracts;

export const contractRequest = (
  operation: Operation,
  getAccessToken: () => Promise<string>,
  input: RequestInput = {},
): ApiFetchOptions => {
  const { request } = contracts[operation];
  for (const part of ['body', 'path', 'query'] as const) {
    const schema = request[part];
    if (schema && !schema.safeParse(input[part]).success) {
      throw new ApiContractError(operation, `request ${part}`);
    }
  }
  return { [contractContext]: { operation, getAccessToken } };
};

export const apiFetch = async <T>(path: string, options: ApiFetchOptions): Promise<T> => {
  const { [contractContext]: context, ...init } = options;
  if (!context) {
    throw new ApiContractError('Fetch API', 'request');
  }

  const { operation, getAccessToken } = context;
  const contract = contracts[operation];
  const requestPath = path.split('?', 1)[0];
  const expectedSegments = contract.path.split('/');
  const requestSegments = requestPath?.split('/');
  if (
    init.method !== contract.method.toUpperCase() ||
    requestSegments?.length !== expectedSegments.length ||
    expectedSegments.some(
      (segment, index) => !segment.startsWith('{') && segment !== requestSegments[index],
    )
  ) {
    throw new ApiContractError(operation, 'request');
  }

  const accessToken = await getAccessToken();
  if (!accessToken) {
    throw new Error('API authentication did not return an access token.');
  }

  const headers = new Headers(init.headers);
  headers.set('Authorization', `Bearer ${accessToken}`);
  const baseUrl = (import.meta.env.VITE_MENU_API_URL ?? '').replace(/\/$/, '');
  const response = await fetch(`${baseUrl}${path}`, { ...init, headers, redirect: 'manual' });
  const { status } = response;
  const expected = contract.responses[status];
  if (!expected) {
    throw new ApiContractError(operation, 'status', status);
  }

  const mediaType = response.headers.get('Content-Type')?.split(';', 1)[0]?.trim().toLowerCase();
  const schema = mediaType ? expected.content?.[mediaType] : undefined;
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

  // The operation's generated response schema has already checked the parsed value.
  return data as T;
};
