# Basher decision — issue #977

- Date: 2026-05-03T21:56:25.759+01:00
- Decision: Treat exact duplicate entries on set-like backend writes as idempotent in both service and repository layers.
- Scope: `POST /api/ingredient`, `POST /api/recipe`, and `PUT /api/recipe/{recipeId}`.
- Rationale: Normalizing duplicate-equivalent payload items before persistence keeps existing success responses unchanged and prevents duplicate insert attempts against composite-key child/link tables. Conflicting duplicates remain out of scope here and stay available for explicit validation work in issue #980.
