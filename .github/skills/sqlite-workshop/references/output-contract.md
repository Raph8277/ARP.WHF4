# Output Contract

Use this structure whenever the skill answers a SQLite workshop request.

## Required sections
- Goal
- Domain
- Tables
- Relationships
- SQLite artifacts
- Seed artifacts
- Migration artifacts
- Build procedure
- Documentation outputs
- Risks

## Quality bar
- SQL must target SQLite syntax.
- The answer must distinguish between the schema source and the resulting `.db` file.
- Foreign keys and indexes must be explicit when relevant.
- Insert order and migration order must be explicit when relevant.
- Mermaid output must align with the documented relationships.