# Mermaid Cheatsheet

Prefer `gitGraph` for branching workflows.

## Merge example
```mermaid
gitGraph
    commit id: "main-1"
    branch feature/login
    checkout feature/login
    commit id: "feat-1"
    commit id: "feat-2"
    checkout main
    merge feature/login
```

## Rebase example
```mermaid
gitGraph
    commit id: "main-1"
    branch feature/login
    checkout feature/login
    commit id: "feat-1"
    checkout main
    commit id: "main-2"
    checkout feature/login
    commit id: "rebased-feat-1"
```

## Notes
- Use concrete branch names.
- Mention in prose when the branch is published to `origin`.
- Explain rebase separately because `gitGraph` does not fully express force-push semantics.
