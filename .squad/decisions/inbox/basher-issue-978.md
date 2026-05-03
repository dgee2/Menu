# Basher decision — issue #978

- Date: 2026-05-03T21:56:25.759+01:00
- Decision: For canonical ingredient rows, normalize incoming `UnitIds`, reuse an existing same-name ingredient when the effective unit set matches, and block mismatched redefinitions before insert.
- Scope: `POST /api/ingredient`
- Rationale: Ingredient rows behave as reference data for recipe writes, so the backend must perform lookup-before-insert to keep name-based resolution unambiguous and to prepare the schema for uniqueness constraints in #979.
