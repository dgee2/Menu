---
on:
  schedule:
    # Run weekly on Friday at 09:00 UTC so dependency PRs stay predictable and reviewable.
    - cron: '0 9 * * 5'
  workflow_dispatch:

permissions:
  contents: read
  issues: read
  pull-requests: read

engine:
  id: copilot
  env:
    # Fine-grained PAT with "Copilot Requests" permission — stored as COPILOT_TOKEN in this repository.
    COPILOT_GITHUB_TOKEN: ${{ secrets.COPILOT_TOKEN }}

checkout:
  - fetch-depth: 0

network:
  allowed:
    - defaults
    - github

tools:
  edit:
  bash: [":*"]
  github:
    toolsets: [default]

safe-outputs:
  create-pull-request:
    max: 10
    preserve-branch-name: true
    recreate-ref: true
    fallback-as-issue: false
  close-pull-request:
    max: 10
  add-labels:
    max: 30
---

Read `.github/prompts/dependency-update.prompt.md` and apply these repository skills:
- `dependency-update-github-actions`
- `dependency-update-pr-description`

Assume the current workspace is the target repository and the current working directory is the repository root.

Runtime parameters:
- Ecosystem: GitHub Actions
- Branch prefix override: copilot/dependency-update/github-actions
- Default branch override: ${{ github.event.repository.default_branch }}
- Interaction mode: unattended
- GitHub Actions discovery scope: actions and reusable workflows
- Prerelease policy: Consider prerelease versions only when the current version is prerelease; otherwise prefer stable releases.

Use those runtime parameters when following the prompt and skills.
If the prompt or skill says to ask clarifying questions, resolve them from the runtime parameters and the repository state instead of waiting for user input.
If any required validation command fails for a planned update, record the failure, do not create or finalise that pull request, continue attempting any remaining planned update groups or pull requests, and exit with a non-zero status code only after all planned work has been attempted.
