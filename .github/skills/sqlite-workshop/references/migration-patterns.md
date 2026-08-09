# Migration Patterns

## Forward-only numbering
- Name migrations with a numeric prefix such as `001_`, `002_`, `003_`.
- Apply them in order and document dependencies.

## Additive change first
- Prefer adding new tables, columns, and indexes over destructive changes.
- Separate schema changes from optional data backfills.

## Rebuild when SQLite requires it
- For complex changes, create a replacement table.
- Copy data into the new table.
- Drop or rename the old table only after validation.

## Documentation notes
- Explain the intent of each migration.
- Call out rollback limitations explicitly.