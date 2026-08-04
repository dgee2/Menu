---
name: agent-identity
description: Ensure every PR review thread comment and every commit made by an AI agent (GitHub Copilot, Claude, or Codex) clearly identifies which agent produced it, so reviewers and contributors can immediately tell who wrote it.
---

# Agent Identity

## Purpose

Use this skill whenever an AI agent writes content into git or GitHub on this repository — a pull request review thread comment, or a commit message. Readers must never have to guess whether a human or an agent wrote something, or which agent it was.

This applies to **GitHub Copilot**, **Claude**, and **Codex** alike. Identify yourself as whichever one you actually are; never attribute your work to a different agent.

## PR review thread comments

Every PR review thread comment must begin with an attribution block, on its own line, before any substantive content. Use the line matching your identity:

```
> 🤖 **This comment was written by GitHub Copilot.**
```

```
> 🤖 **This comment was written by Claude.**
```

```
> 🤖 **This comment was written by Codex.**
```

Rules:

- Always prepend the attribution line as the very first line of the comment.
- Leave one blank line between the attribution line and the body of the comment.
- Do **not** add the attribution line to pull request titles or descriptions, issue body text, or issue comments.
- Do **not** remove or reword the attribution in a follow-up edit to the same comment.
- Do **not** substitute different phrasing such as "As an AI" or "I am Copilot" — use the canonical block above exactly.

## Commit messages

Every commit an agent creates must carry a `Co-authored-by:` trailer identifying the agent, as the last line(s) of the commit message, separated from the body by a blank line. Use the canonical trailer for your identity:

```
Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
```

```
Co-authored-by: Claude <noreply@anthropic.com>
```

```
Co-authored-by: Codex <codex@openai.com>
```

Rules:

- The trailer is the identity marker for commits — do **not** put the `> 🤖` PR-comment attribution block in a commit message, and do not put an agent name in the subject line.
- Keep the subject line a normal, descriptive summary of the change.
- If a human and an agent genuinely co-authored the change, list both `Co-authored-by:` trailers.
- Claude may append its model name (e.g. `Co-authored-by: Claude Sonnet 5 <noreply@anthropic.com>`); what matters is that "Claude" appears.

## Example commit message

```
Add ownership-aware recipe list endpoint

Adds a `scope` query parameter accepting `mine` or `authenticated`, so
callers can choose between owner-only and all visible recipes.

Co-authored-by: Claude <noreply@anthropic.com>
```

## Example PR comment

````markdown
> 🤖 **This comment was written by GitHub Copilot.**

The `RecipeService.CreateAsync` method currently calls `_repository.InsertAsync` without first
deduplicating the ingredient list. Per the `set-like-write-dedupe` pattern, duplicates should be
collapsed at the service layer before the repository is called.

Suggested fix:

```csharp
var ingredients = [.. ViewModelMapper.Map(newRecipe.Ingredients).Distinct()];
```
````
