---
name: push-worktree
description: "Merge a clean worktree branch into the main working tree's branch via fast-forward. Validates both trees are clean, checks commit delta, and performs a safe --ff-only merge."
---

# /push-worktree

Merge the current worktree's branch into the main working tree's local branch.

## Usage

```
/push-worktree              # merge into the main worktree's current branch
/push-worktree main         # merge into a specific target branch
```

## What You Must Do When Invoked

Follow these steps in order. Stop at any failure and report clearly.

### Step 1 — Confirm we are inside a worktree

```bash
MAIN_WT=$(git worktree list --porcelain | head -1 | sed 's/^worktree //')
CURRENT=$(pwd -W 2>/dev/null || pwd)
```

If `CURRENT` equals `MAIN_WT`, stop: **"You are in the main working tree, not a worktree. Run this from a worktree."**

### Step 2 — Collect branch names

```bash
WT_BRANCH=$(git rev-parse --abbrev-ref HEAD)
TARGET_BRANCH=<user arg, or the branch checked out in MAIN_WT>
```

To find the main worktree's branch when no argument is given:

```bash
TARGET_BRANCH=$(git -C "$MAIN_WT" rev-parse --abbrev-ref HEAD)
```

If `TARGET_BRANCH` is `HEAD` (detached), stop: **"The main working tree is in detached HEAD state. Check it out on a branch first."**

### Step 3 — Validate both trees are clean

Check the **current worktree**:

```bash
git status --porcelain
```

If output is non-empty, stop: **"Worktree has uncommitted changes. Commit or stash first."**

Check the **main working tree**:

```bash
git -C "$MAIN_WT" status --porcelain
```

If output is non-empty, stop: **"Main working tree has uncommitted changes. Commit or stash there first."**

### Step 4 — Check there are commits to push

```bash
AHEAD=$(git rev-list --count "$TARGET_BRANCH"..HEAD)
```

If `AHEAD` is 0, stop: **"No new commits. The worktree branch is already up to date with `TARGET_BRANCH`."**

Show the user what will be merged:

```bash
git log --oneline "$TARGET_BRANCH"..HEAD
```

Print a summary: **"N commit(s) to merge into `TARGET_BRANCH`:"** followed by the log.

### Step 5 — Verify fast-forward is possible

```bash
git merge-base --is-ancestor "$TARGET_BRANCH" HEAD
```

If this command exits non-zero, stop: **"Cannot fast-forward — `TARGET_BRANCH` has diverged. Rebase the worktree branch first: `git rebase TARGET_BRANCH`."**

### Step 6 — Perform the merge

Run the merge **from the main working tree** so both the branch ref and the working tree files update atomically:

```bash
git -C "$MAIN_WT" merge --ff-only "$WT_BRANCH"
```

### Step 7 — Verify and report

```bash
MERGED_SHA=$(git -C "$MAIN_WT" rev-parse --short HEAD)
```

Print:

```
Done. TARGET_BRANCH is now at MERGED_SHA.
  N commit(s) merged from WT_BRANCH.
  Main working tree at MAIN_WT is up to date.
```

## Error handling

- If `git merge --ff-only` fails, print the git error verbatim and suggest the user inspect the main working tree.
- Never use `--force`, `branch -f`, or `update-ref` — always go through `merge --ff-only` so the working tree stays in sync.
- Never push to origin — this skill is local-only.
