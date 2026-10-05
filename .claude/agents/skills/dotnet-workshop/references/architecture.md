# Architecture

The .NET workshop is organized around one orchestrator and five specialists.

## Roles
- `dotnet-workshop-orchestrator`: routes the request and consolidates the output.
- `dotnet-solution-designer`: defines the solution shape and dependency flow.
- `dotnet-implementation-builder`: generates projects, code, and commands.
- `dotnet-test-and-quality`: defines validation and test structure.
- `dotnet-doc-writer`: produces the Markdown documentation.
- `dotnet-mermaid-diagrammer`: produces the Mermaid diagrams.

## Handoff model
1. Interpret the user intent.
2. Define the solution architecture.
3. Generate the code and project artifacts.
4. Add tests and validation.
5. Document the workflow.
6. Draw the architecture diagram.

## Core workshop promise
Every workflow must explain:
- what kind of .NET application is being built;
- how the solution and projects are organized;
- how the code is built, run, and tested;
- how the architecture is documented in Markdown and Mermaid.