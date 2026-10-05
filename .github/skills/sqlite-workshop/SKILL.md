---
name: sqlite-workshop
description: 'SQLite workshop workflow for schema design, SQL generation, database creation, seed data insertion, migration planning, Markdown documentation, and Mermaid ER diagrams.'
argument-hint: 'Describe the domain, entities, expected SQLite artifacts, seed data needs, migration needs, and required documentation outputs.'
user-invocable: true
---

# SQLite Workshop

Use this skill when the request involves SQLite schema design, SQL generation, `.db` creation, seed data, migrations, Markdown documentation, or Mermaid diagrams.

## What This Skill Covers
- Define a relational model from a domain description.
- Generate SQLite DDL.
- Produce a `.sql` schema and optional seed data.
- Explain or execute the creation of a `.db` file.
- Produce insertion scripts for realistic sample data.
- Produce migration scripts for schema evolution and data backfills.
- Produce workshop-grade Markdown documentation and Mermaid ER diagrams.

## Workflow
1. Confirm the domain, entities, relationships, and required constraints.
2. Load the modeling guidance in [data-model-patterns](./references/data-model-patterns.md).
3. Load the output contract in [output-contract](./references/output-contract.md).
4. If SQL generation is requested, use [sqlite-ddl-cheatsheet](./references/sqlite-ddl-cheatsheet.md).
5. If seed data is requested, use [data-insertion-patterns](./references/data-insertion-patterns.md).
6. If migrations are requested, use [migration-patterns](./references/migration-patterns.md).
7. If documentation is requested, use [architecture](./references/architecture.md) and [review-checklist](./references/review-checklist.md).
8. If a diagram is requested, use [mermaid-cheatsheet](./references/mermaid-cheatsheet.md).

## Standard Artifacts
### Minimal SQLite schema file
```sql
PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS example_parent (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS example_child (
    id INTEGER PRIMARY KEY,
    parent_id INTEGER NOT NULL,
    label TEXT NOT NULL,
    FOREIGN KEY (parent_id) REFERENCES example_parent(id)
);
```

### Create a database from the schema
```bash
sqlite3 demo.db < schema.sql
```

### Seed the database
```bash
sqlite3 demo.db < seed.sql
```

### Apply a migration
```bash
sqlite3 demo.db < migrations/002_add_example_change.sql
```

## Expected Outputs
- A relational model recommendation.
- The exact SQL artifacts to create the schema.
- A command or procedure to build the SQLite database file.
- Optional seed data for insertion.
- Optional migration scripts and backfill steps.
- Optional Markdown documentation.
- Optional Mermaid `erDiagram` output.

## Resources
- [Architecture](./references/architecture.md)
- [Output contract](./references/output-contract.md)
- [Data model patterns](./references/data-model-patterns.md)
- [SQLite DDL cheatsheet](./references/sqlite-ddl-cheatsheet.md)
- [Data insertion patterns](./references/data-insertion-patterns.md)
- [Migration patterns](./references/migration-patterns.md)
- [Mermaid cheatsheet](./references/mermaid-cheatsheet.md)
- [Review checklist](./references/review-checklist.md)
- [Starter prompt](./references/starter-prompt.md)
- [Schema template](./assets/schema-template.sql)
- [Documentation template](./assets/documentation-template.md)
- [Seed template](./assets/seed-template.sql)
- [Migration template](./assets/migration-template.sql)