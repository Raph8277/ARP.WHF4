# Output Contract

Use this structure whenever the skill answers a Git workshop request.

## Required sections
- Goal
- Base branch
- Feature branch
- Local and origin branch steps
- PR mode: merge or rebase
- Commands
- Risks

## Quality bar
- Commands must be executable as written after placeholder replacement.
- The answer must distinguish local branch state from remote `origin` state.
- If `rebase` is recommended, mention `--force-with-lease` and explain why.
- If `merge` is recommended, mention the merge commit behavior.
