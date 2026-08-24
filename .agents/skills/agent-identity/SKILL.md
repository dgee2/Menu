---
name: agent-identity
description: Ensure every PR review thread comment and every commit Codex makes clearly identifies Codex as the author, so reviewers and contributors can immediately tell who wrote it.
---

# Agent Identity

## Purpose

Use this skill whenever you write content into git or GitHub on this repository — a pull request review thread comment, or a commit message. Readers must never have to guess whether a human or an agent wrote something.

Always identify yourself as **Codex**. Never attribute your work to a human, and never attribute it to another agent.

## PR review thread comments

Every PR review thread comment must begin with this attribution block, on its own line, before any substantive content:

```
> 🤖 **This comment was written by Codex.**
```

Rules:

- Always prepend the attribution line as the very first line of the comment.
- Leave one blank line between the attribution line and the body of the comment.
- Do **not** add the attribution line to pull request titles or descriptions, issue body text, or issue comments.
- Do **not** remove or reword the attribution in a follow-up edit to the same comment.
- Do **not** substitute different phrasing such as "As an AI" or "I am an assistant" — use the canonical block above exactly.

## Commit messages

Every commit you create must carry a `Co-authored-by:` trailer as the last line(s) of the commit message, separated from the body by a blank line:

```
Co-authored-by: Codex <codex@openai.com>
```

Rules:

- The trailer is the identity marker for commits — do **not** put the `> 🤖` PR-comment attribution block in a commit message, and do not put an agent name in the subject line.
- Keep the subject line a normal, descriptive summary of the change.
- If a human and Codex genuinely co-authored the change, list both `Co-authored-by:` trailers.
- You may append your model name (e.g. `Co-authored-by: Codex GPT-5 <codex@openai.com>`); what matters is that "Codex" appears.

## Example commit message

```
Add ownership-aware recipe list endpoint

Adds a `scope` query parameter accepting `mine` or `authenticated`, so
callers can choose between owner-only and all visible recipes.

Co-authored-by: Codex <codex@openai.com>
```

## Example PR comment

````markdown
> 🤖 **This comment was written by Codex.**

The `RecipeService.CreateAsync` method currently calls `_repository.InsertAsync` without first
deduplicating the ingredient list. Per the `set-like-write-dedupe` pattern, duplicates should be
collapsed at the service layer before the repository is called.

Suggested fix:

```csharp
var ingredients = [.. ViewModelMapper.Map(newRecipe.Ingredients).Distinct()];
```
````
