# Squad Decisions

## Active Decisions

### Backlog Triage — 2026-05-03

**Decision Owner:** Danny (Lead)  
**Date:** 2026-05-03T21:56:25.759+01:00  
**Status:** ✅ Implemented

#### Summary

Triaged 8 open GitHub issues in dgee2/Menu. Routed 7 issues into Squad workflow with explicit member assignments. Kept 1 epic untagged pending sub-issue completion.

#### Routing Decisions

| Issue | Title | Assigned To | Rationale |
|-------|-------|-------------|-----------|
| #1012 | Dependency Updates | squad:copilot | Good fit — well-scoped boilerplate task with clear acceptance criteria |
| #977 | Prevent duplicate inserts (set-like) | squad:basher | Backend service/repository work; high-priority in epic sequence |
| #978 | Prevent duplicate inserts (canonical) | squad:basher | Backend lookup-before-insert logic; clear scope with existing patterns |
| #979 | Add uniqueness constraints | squad:basher | EF Core migrations and schema hardening; depends on #977/#978 |
| #980 | Expose explicit duplicate conflicts | squad:basher | API contract work with endpoint-specific validation logic |
| #884 | Review AutoData usage | squad:yen | Testing infrastructure cleanup; improves fixture maintainability |
| #885 | Review integration tests | squad:yen | Testing infrastructure validation; checks AppHost collection usage |

#### Epic Held in Backlog

| Issue | Title | Reason |
|-------|-------|--------|
| #887 | Review update/insert API operations | Epic coordination work. Sub-issues assigned individually (977–980). Untagged until sub-issues complete. |

#### Ordering (Basher Sequence)

1. **#977** (set-like dedupe) — Required before #978 and #979
2. **#978** (canonical dedupe) — Builds on #977 patterns
3. **#979** (schema constraints) — Depends on #977/#978 completing
4. **#980** (explicit conflicts) — Final step; API contracts

Yen can work on #884 and #885 in parallel.

#### Labels Created

- `squad` — Issue is in Squad backlog
- `squad:copilot` — Assigned to Copilot (coding agent)
- `squad:basher` — Assigned to Basher (Backend Dev)
- `squad:yen` — Assigned to Yen (Tester)

---

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
