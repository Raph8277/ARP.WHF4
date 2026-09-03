# Mermaid Cheatsheet

Prefer `erDiagram` for SQLite schemas.

## Example
```mermaid
erDiagram
    project ||--o{ task : contains

    project {
        INTEGER id PK
        TEXT code UK
        TEXT name
    }

    task {
        INTEGER id PK
        INTEGER project_id FK
        TEXT title
        TEXT status
    }
```

## Notes
- Keep attribute lists short enough to remain readable.
- Use `PK`, `FK`, and `UK` markers when they improve clarity.
- Explain extra constraints in prose if the diagram would become noisy.