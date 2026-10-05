# Data Insertion Patterns

## Parent first, child second
- Insert referenced rows before dependent rows.
- Keep foreign key values easy to trace in workshop examples.

## Small realistic datasets
- Prefer 3 to 10 rows per table in workshop mode.
- Use coherent values that make joins easy to inspect.

## Stable identifiers
- Keep identifiers deterministic when examples are documented.
- Prefer explicit `id` values in seed files used for learning.

## Safety notes
- Do not assume seed data is production-safe.
- Document uniqueness assumptions in the seed file comments when relevant.