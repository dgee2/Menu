---
name: github-issues-status
description: Fetch all issues for a GitHub repo, parse Epic task lists, and compute a blocked/unblocked/in-progress/done status for every issue and epic transitively.
user-invokable: true
context: inline
---

# GitHub Issues Status

## Purpose

Use this skill whenever you need a structured view of issue states and blocking relationships — as input to a diagram, a sprint report, or any other artefact. It produces a data model (not a visual output) that other skills or prompts can consume.

## Inputs

| Name | Required | Description |
|---|---|---|
| `repo` | yes | `OWNER/REPO` slug, e.g. `dgee2/Menu` |
| `epic-pattern` | no | Title prefix that identifies epic issues. Default: `[Epic]` |

## Step 1 — Fetch all issues

```bash
gh issue list --repo <repo> --state all --limit 200
```

Capture: `number`, `title`, `state` (OPEN / CLOSED), `labels`.

## Step 2 — Identify epics and fetch their bodies

Any issue whose title contains `[Epic]` (or the configured `epic-pattern`) is an **Epic**.

For each epic, fetch the full body:

```bash
gh issue view <number> --repo <repo> --json title,body
```

Parse the body for GitHub task-list syntax to extract child issue references:

```
- [x] #NNNN   ← completed child task
- [ ] #NNNN   ← open child task
```

Build a map: `epic → [child issue numbers]`.

## Step 3 — Infer cross-epic dependencies

Read each epic body for explicit dependency language:

> "depends on", "requires", "after", "once X exists", "built on top of", "comes first", "all subsequent"

Also apply these domain ordering rules:
- DB schema / entity changes must precede API endpoint changes
- API endpoint changes must precede frontend changes
- User identity / auth foundation (MenuUser) must precede all feature epics
- Outbox / event infrastructure must precede projection consumers that read from it
- A "public route group" structural split must precede any public endpoints that use it

Record each inferred dependency with a short reason label (used for arrow labels in diagrams).

For cross-epic task-level wiring in child-issue diagrams, connect the last key output task of a prerequisite epic to the first task of the dependent epic. If multiple tasks must complete, wire each one explicitly.

## Step 4 — Compute status for every issue

Apply this algorithm **transitively**, starting from issues with no dependencies:

```
status(issue):
  if issue.state == CLOSED            → DONE
  if all direct dependencies are DONE → UNBLOCKED
  otherwise                           → BLOCKED
```

For **Epics**, derive status from their children and epic-level dependencies:

```
epicStatus(epic):
  if all children DONE and all epic deps DONE  → DONE
  if some children DONE and next open child is UNBLOCKED
    and all epic deps DONE                     → IN_PROGRESS
  otherwise                                    → BLOCKED
```

## Step 5 — Output the data model

Produce a structured summary with these fields per issue:

```
{
  number:       1097,
  title:        "[Epic] Identity & User Provisioning (MenuUser)",
  state:        "OPEN",
  status:       "IN_PROGRESS",      // DONE | UNBLOCKED | IN_PROGRESS | BLOCKED
  isEpic:       true,
  children:     [1109, 1110],        // epic only
  closedChildren: [1109],            // epic only
  deps:         [],                  // issue numbers this one depends on
  depLabels:    {},                  // dep number → reason string
  blockedBy:    [],                  // first-level open deps
}
```

This output is consumed by `mermaid-html-diagram` or other reporting skills.
