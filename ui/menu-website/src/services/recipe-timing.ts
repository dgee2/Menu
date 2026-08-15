/**
 * Mirrors the server's `RecipeTiming.Effective`.
 *
 * Read-only pages must NOT call this — they render the server-computed `effectiveTotalTimeMinutes`
 * the API already sends them, which is the whole reason that field exists. This exists solely for
 * the editor, which needs the figure to update live as prep and cook are typed, and which in create
 * mode has no server response to read it from.
 */
export const effectiveTotalTimeMinutes = (
  totalTimeMinutes: number | null,
  prepTimeMinutes: number | null,
  cookTimeMinutes: number | null,
): number | null => {
  if (totalTimeMinutes != null) return totalTimeMinutes;
  if (prepTimeMinutes == null && cookTimeMinutes == null) return null;

  return (prepTimeMinutes ?? 0) + (cookTimeMinutes ?? 0);
};
