# Mermaid Cheatsheet

Prefer `flowchart` for solution and dependency views.

## Example
```mermaid
flowchart LR
    Client[Client] --> Api[Sample.Api]
    Api --> Core[Sample.Core]
    Api --> Infra[Sample.Infrastructure]
    Tests[Sample.Tests] --> Api
```

## Notes
- Keep project names close to the actual `.csproj` names.
- Use left-to-right flow for dependency diagrams.
- Explain omitted runtime infrastructure in prose when necessary.