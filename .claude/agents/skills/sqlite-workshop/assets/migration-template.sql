BEGIN TRANSACTION;

ALTER TABLE entity_a ADD COLUMN description TEXT;

UPDATE entity_a
SET description = 'Backfilled description'
WHERE description IS NULL;

COMMIT;