---
name: pr-comment-identity
description: Ensure all PR review thread comments posted by Claude Code are clearly attributed as automated so reviewers and contributors can tell immediately who wrote the comment.
---

# PR Comment Identity

## Purpose

Use this skill whenever Claude Code posts a comment on a pull request review thread — whether responding to a review request, addressing a specific line comment, or replying in a general PR conversation thread.

## Required attribution

Every PR review thread comment must begin with the following attribution block, on its own line, before any substantive content:

```
> 🤖 **This comment was written by Claude Code.**
```

## Rules

- Always prepend the attribution line as the very first line of the comment.
- Leave one blank line between the attribution line and the body of the comment.
- Do **not** add the attribution line to:
  - Pull request titles or descriptions
  - Commit messages
  - Issue body text or issue comments
  - Any content that is not a PR review thread comment
- Do **not** remove or reword the attribution in a follow-up edit to the same comment.
- Do **not** replace the attribution with a different phrasing such as "As an AI" or "I am Claude" — use the canonical block above exactly.

## Example

````markdown
> 🤖 **This comment was written by Claude Code.**

The `RecipeService.CreateAsync` method currently calls `_repository.InsertAsync` without first
deduplicating the ingredient list. Per the `set-like-write-dedupe` pattern, duplicates should be
collapsed at the service layer before the repository is called.

Suggested fix:

```csharp
var ingredients = [.. ViewModelMapper.Map(newRecipe.Ingredients).Distinct()];
```
````
