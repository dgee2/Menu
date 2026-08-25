/**
 * RFC 7807 problem details, as the API returns them. `errors` is present on the
 * `ValidationProblemDetails` shape produced by `TypedResults.ValidationProblem` and by the
 * FluentValidation endpoint filter.
 */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
}

const isProblemDetails = (value: unknown): value is ProblemDetails =>
  typeof value === 'object' && value !== null && !Array.isArray(value);

const firstValidationError = (problem: ProblemDetails | undefined): string | undefined =>
  Object.values(problem?.errors ?? {})
    .flat()
    .find((message) => !!message);

/**
 * The server's own explanation, when it gave one worth showing.
 *
 * A validation problem carries no `detail` and a fixed, useless `title` ("One or more validation
 * errors occurred."), so the first field message is preferred over it - that is the part the user
 * can actually act on until field-level mapping arrives.
 *
 * A free function rather than only a getter, so `ApiError.from` can compute the message before
 * constructing rather than building a throwaway instance to ask it.
 */
const detailOf = (problem: ProblemDetails | undefined): string | undefined =>
  problem?.detail ?? firstValidationError(problem) ?? problem?.title;

/**
 * An API call that came back with a non-2xx status, carrying the parsed problem details.
 *
 * The point of the class is that the body survives. Throwing a bare `Error` discards it, so a 409
 * duplicate-title is indistinguishable from a network outage, and the form can only offer "please
 * try again" for a failure that retrying can never fix.
 */
export class ApiError extends Error {
  readonly status: number;
  readonly problem: ProblemDetails | undefined;

  constructor(message: string, status: number, problem: ProblemDetails | undefined) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.problem = problem;
  }

  /** A duplicate recipe title. Callers surface this against the title field, not as a banner. */
  get isConflict(): boolean {
    return this.status === 409;
  }

  /**
   * Field-level validation messages keyed by property name, when the server sent them.
   *
   * Nothing maps these onto individual form rows yet — that is deliberately deferred. Exposing them
   * here means doing so later is an addition rather than a rewrite of the error path.
   */
  get validationErrors(): Record<string, string[]> | undefined {
    return this.problem?.errors;
  }

  /** The server's own explanation, when it gave one worth showing. See {@link detailOf}. */
  get detail(): string | undefined {
    return detailOf(this.problem);
  }

  /**
   * The message to put in front of a user, given a fallback for when the server has nothing useful
   * to say.
   *
   * A 4xx explains something about the request that the user can act on, so its detail is worth
   * showing. A 5xx explains something about the server: "Internal Server Error" tells the user
   * nothing they can do about it, so the caller's own wording wins.
   */
  userFacingMessage(fallback: string): string {
    const isClientError = this.status >= 400 && this.status < 500;

    return (isClientError && this.detail) || fallback;
  }

  static from(operation: string, error: unknown, response: Response): ApiError {
    const problem = isProblemDetails(error) ? error : undefined;
    const fallback = `${operation} failed (${response.status})`;

    return new ApiError(detailOf(problem) ?? fallback, response.status, problem);
  }
}

/**
 * Keep unexpected failures useful while developing without exposing implementation details in
 * production. This is especially valuable for authentication failures, which happen before the
 * API client can wrap the error in an ApiError.
 */
export const userFacingMessage = (error: unknown, fallback: string): string => {
  if (error instanceof ApiError) return error.userFacingMessage(fallback);

  return import.meta.env.DEV && error instanceof Error && error.message ? error.message : fallback;
};
