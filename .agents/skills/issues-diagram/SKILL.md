---
name: issues-diagram
description: Generate or refresh a self-contained HTML file with three interactive Mermaid dependency diagrams for the dgee2/Menu repo — epics only, child issues only, and a full combined view — with status-based colour coding and pan/zoom.
---

# Issues Dependency Diagram

Generate (or refresh) the interactive HTML dependency diagram for this repository.

## Skills to apply

Apply both of these skills in order:

1. **`github-issues-status`** — fetch all issues, parse epic task lists, and compute blocked / unblocked / in-progress / done status for every issue and epic.
2. **`mermaid-html-diagram`** — render the computed data as a self-contained HTML file.

## Runtime context

- Repository: `dgee2/Menu`
- Output path: use the path supplied by the user, otherwise `docs/issues-diagram.html`
- Open the file after writing it

## Three diagrams to produce

### Diagram 1 — Epics only (`#epics-only`)

- `rankDir: TB`, height `72vh`
- One node per epic; apply the epic's computed status class
- Labelled arrows between epics showing the dependency reason
- Node label includes: epic number, title, status indicator, and a one-line blocker summary

### Diagram 2 — Child issues only (`#child-issues`)

- `rankDir: LR`, height `85vh`
- Task nodes grouped into epic subgraphs; subgraph background colour matches epic status
- Within-epic arrows follow the natural implementation order (DB entity → API → frontend)
- Cross-epic arrows connect specific tasks at the boundary between dependent epics
  (see the cross-epic wiring table below)
- Node label includes: issue number, short title, ✅ if closed

### Diagram 3 — Full diagram (`#full-diagram`)

- `rankDir: TB`, height `85vh`
- Epics as subgraph containers (styled by status) containing all child tasks
- Epic-level dependency arrows between subgraphs

## Cross-epic task-level wiring (child issues diagram)

These are the established dependencies inferred from epic bodies and domain ordering rules.
Re-verify closed state on each refresh — a closed issue promotes its dependents from BLOCKED to UNBLOCKED.

| From | To | Reason |
|---|---|---|
| #1110 | #1111, #1112, #1113 | MenuUser.Id referenced by Recipe ownership |
| #1111, #1112, #1113 | #1114 | All DB changes before DTO expansion |
| #1114 | #1115, #1116, #1117 | DTOs before API endpoints |
| #1115 | #1118, #1123 | List endpoint before detail page / search index |
| #1116 | #1119, #1124, #1129 | Create/update before form, outbox, sharing |
| #1133 | #1134 | Public route group structural split required |
| #1117 | #1139 | Recipe CRUD complete before architectural restructure |
| #1110 | #1129, #1143, #1144, #1151, #1154 | User identity before all user-owned features |
| #1124 | #1148 | Outbox infra before metrics snapshot |
| #1125 | #1149 | RecipeViewed event before metrics consumer |
| #1143, #1145 | #1149 | Favourite/diary events feed metrics consumer |
| #1116 | #1143, #1151 | Recipe CRUD before favourite / collection features |

## Known epic structure

The table below is pre-populated from the 2026-05-28 snapshot. On each run, re-check issue states via `github-issues-status` and update statuses — do not use this table as a substitute for live data.

| Epic  | Title                                      | Children        | Depends on        |
|-------|--------------------------------------------|-----------------|-------------------|
| #1097 | Identity & User Provisioning (MenuUser)    | #1109, #1110    | —                 |
| #1098 | Core Recipe Model (DB Schema)              | #1111–#1113     | #1097             |
| #1099 | Recipe Authoring API Expansion             | #1114–#1117     | #1098             |
| #1100 | Frontend: Recipe Editor & Detail Pages     | #1118–#1122     | #1099             |
| #1101 | Recipe Search                              | #1123–#1128     | #1099             |
| #1102 | Access Control & Sharing                   | #1129–#1133     | #1097, #1099      |
| #1103 | Recipe Publication                         | #1134–#1138     | #1102 (via #1133) |
| #1104 | Architectural Project Restructure          | #1139–#1142     | #1099             |
| #1105 | Favourites & Diary                         | #1143–#1147     | #1097, #1099      |
| #1106 | Metrics & Engagement                       | #1148–#1150     | #1101, #1105      |
| #1107 | Curated Recipe Collections                 | #1151–#1153     | #1099             |
| #1108 | Communication Preferences                  | #1154–#1156     | #1097             |

## Summary table

Append an **Epic Summary** HTML table below the diagrams with columns:
Epic #, Title, Child Issues, Status, Depends on.
