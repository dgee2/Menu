import { afterEach, describe, expect, it, vi } from 'vitest';
import { ApiError, userFacingMessage } from './api-error';

const response = (status: number) => new Response(null, { status });

describe('ApiError', () => {
  afterEach(() => {
    vi.unstubAllEnvs();
  });

  it('prefers the server-supplied detail', () => {
    const error = ApiError.from('Create recipe', { detail: 'Already exists.' }, response(409));

    expect(error.detail).toBe('Already exists.');
    expect(error.message).toBe('Already exists.');
    expect(error.isConflict).toBe(true);
  });

  it('surfaces the first field message rather than the generic validation title', () => {
    // ValidationProblemDetails has no `detail` and a fixed `title`, so preferring `title` would
    // show "One or more validation errors occurred." and bury the one actionable message.
    const error = ApiError.from(
      'Create recipe',
      {
        title: 'One or more validation errors occurred.',
        status: 400,
        errors: { Servings: ["'Servings' must be between 0 and 1000."] },
      },
      response(400),
    );

    expect(error.detail).toBe("'Servings' must be between 0 and 1000.");
    expect(error.validationErrors).toEqual({
      Servings: ["'Servings' must be between 0 and 1000."],
    });
  });

  it('falls back to the title when there are no field messages', () => {
    const error = ApiError.from('Get recipe', { title: 'Not Found', status: 404 }, response(404));

    expect(error.detail).toBe('Not Found');
    expect(error.status).toBe(404);
  });

  it('falls back to the operation and status for a non-problem body', () => {
    const error = ApiError.from('Get recipe', 'plain text', response(500));

    expect(error.detail).toBeUndefined();
    expect(error.message).toBe('Get recipe failed (500)');
  });

  it('surfaces an unexpected error message during development', () => {
    vi.stubEnv('DEV', true);

    expect(userFacingMessage(new Error('consent_required'), 'Try again.')).toBe('consent_required');
  });

  it('keeps unexpected error details out of production messages', () => {
    vi.stubEnv('DEV', false);

    expect(userFacingMessage(new Error('consent_required'), 'Try again.')).toBe('Try again.');
  });

  it('keeps the fallback for non-Error failures', () => {
    vi.stubEnv('DEV', true);

    expect(userFacingMessage('consent_required', 'Try again.')).toBe('Try again.');
  });
});
