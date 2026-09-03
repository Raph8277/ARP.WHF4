# Architecture

The SQLite workshop is organized around one orchestrator and six specialists.

## Roles
- `sqlite-workshop-orchestrator`: routes the request and consolidates the output.
- `sqlite-schema-designer`: defines the relational model.
- `sqlite-db-builder`: generates SQL and database creation steps.
- `sqlite-data-inserter`: produces seed datasets and insertion scripts.
- `sqlite-migration-manager`: produces schema evolution and backfill scripts.
- `sqlite-doc-writer`: produces the Markdown documentation.
- `sqlite-mermaid-diagrammer`: produces the Mermaid ER diagram.

## Handoff model
1. Interpret the user intent.
2. Define the relational model.
3. Generate SQLite artifacts.
4. Add seed data and migrations when needed.
5. Document the schema and build procedure.
6. Draw the ER diagram.

## Core workshop promise
Every workflow must explain:
- what entities and relationships exist;
- how the schema is expressed in SQLite DDL;
- how seed data and migrations evolve the database over time;
- how the `.db` file is created from the SQL artifacts;
- how the database is documented in Markdown and Mermaid.