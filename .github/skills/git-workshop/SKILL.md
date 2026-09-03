---
name: git-workshop
description: 'Git workshop workflow for branching strategy, local and origin branch creation, pull request preparation, merge vs rebase decisions, and Mermaid branching diagrams.'
argument-hint: 'Describe the branch objective, base branch, target PR mode, and expected documentation output.'
user-invocable: true
---

# Git Workshop

Use this skill when the request involves Git workflow design, branch creation, PR preparation, merge vs rebase decisions, or Mermaid diagrams for branching.

## What This Skill Covers
- Define a branching strategy around `main`, `dev`, or another base branch.
- Create a local branch and publish it to `origin`.
- Prepare a feature branch for a PR.
- Choose between merge and rebase with clear tradeoffs.
- Keep `docs/git/branch-topology.md` synchronized after pushes that change the observed topology.
- Produce workshop-grade documentation and Mermaid diagrams.

## Workflow
1. Confirm the base branch, feature branch name, and whether the branch is shared.
2. Load the branching guidance in [branching-patterns](./references/branching-patterns.md).
3. Load the command contract in [output-contract](./references/output-contract.md).
4. After a successful push, update `docs/git/branch-topology.md` if branch heads, divergence points, or remote alignment changed.
5. If documentation is requested, use [architecture](./references/architecture.md) and [review-checklist](./references/review-checklist.md).
6. If a diagram is requested, use [mermaid-cheatsheet](./references/mermaid-cheatsheet.md).

## Standard Commands
### Create local and origin branch
```bash
git checkout <base-branch>
git pull origin <base-branch>
git checkout -b <feature-branch>
git push --set-upstream origin <feature-branch>
```

### Prepare a PR with merge
```bash
git checkout <feature-branch>
git fetch origin
git merge origin/<base-branch>
git push
```

### Prepare a PR with rebase
```bash
git checkout <feature-branch>
git fetch origin
git rebase origin/<base-branch>
git push --force-with-lease
```

## Expected Outputs
- A branching recommendation.
- The exact commands to create and publish the branch.
- A PR preparation flow in merge or rebase mode.
- A refreshed `docs/git/branch-topology.md` when the push changes the topology.
- Optional Markdown documentation.
- Optional Mermaid `gitGraph` diagram.

## Resources
- [Architecture](./references/architecture.md)
- [Output contract](./references/output-contract.md)
- [Branching patterns](./references/branching-patterns.md)
- [Mermaid cheatsheet](./references/mermaid-cheatsheet.md)
- [Review checklist](./references/review-checklist.md)
- [Starter prompt](./references/starter-prompt.md)
