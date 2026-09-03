# Data Model Patterns

## Simple catalog
- Use one table per core entity.
- Add unique constraints to natural identifiers.
- Prefer explicit lookup tables over repeated free-text values.

## Parent-child relationship
- Use an integer foreign key on the child table.
- Index the foreign key when the relationship is queried often.

## Many-to-many relationship
- Use a junction table with two foreign keys.
- Add a composite unique constraint to prevent duplicates.

## Audit timestamps
- Add `created_at` and `updated_at` as TEXT in ISO-8601 format when lifecycle tracking matters.
- Document whether timestamps are application-managed.

## SQLite notes
- SQLite uses type affinity rather than rigid column typing.
- Foreign key enforcement requires `PRAGMA foreign_keys = ON;`.
- Over-normalization is often not worth it for small embedded databases.