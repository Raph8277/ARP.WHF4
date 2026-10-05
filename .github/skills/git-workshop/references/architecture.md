# Architecture

The workshop is organized around one orchestrator and four specialists.

## Roles
- `git-workshop-orchestrator`: routes the request and consolidates the output.
- `git-branching-designer`: chooses the branching model and PR strategy.
- `git-command-coach`: generates the Git commands.
- `git-doc-writer`: produces the documentation.
- `git-mermaid-diagrammer`: produces the branching diagrams.

## Handoff model
1. Interpret the user intent.
2. Choose `merge` or `rebase`.
3. Create the local branch and publish it to `origin`.
4. Document the workflow.
5. Draw the diagram.

## Core workshop promise
Every workflow must explain:
- where the branch starts;
- how the branch is published to `origin`;
- how the PR is prepared;
- whether the final integration follows merge or rebase semantics.
