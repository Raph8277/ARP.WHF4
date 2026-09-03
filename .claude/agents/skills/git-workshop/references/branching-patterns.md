# Branching Patterns

## Simple feature branch from main
- Base branch: `main`
- Feature branch: `feature/<topic>`
- Use when work is short-lived and isolated.

## Integration branch from dev
- Base branch: `dev`
- Feature branch: `feature/<topic>` or `fix/<topic>`
- Use when the team stabilizes changes before promoting them.

## Merge workflow
- Best when preserving the branch history matters.
- Keeps the context of the feature branch visible in history.
- Often paired with standard PR merge in hosting platforms.

## Rebase workflow
- Best when the team wants a linear history before integration.
- Requires extra care if the branch was already shared.
- Usually followed by a normal PR, but the feature branch history has been rewritten first.

## Publication model
For every branch pattern, the publication model is:
1. Create branch locally from the chosen base branch.
2. Push the branch to `origin` with upstream tracking.
3. Keep the branch synchronized before opening or updating the PR.
