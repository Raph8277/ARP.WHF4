BEGIN TRANSACTION;

INSERT INTO entity_a (id, name) VALUES
    (1, 'Example A'),
    (2, 'Example B');

INSERT INTO entity_b (id, entity_a_id, label) VALUES
    (1, 1, 'Child 1'),
    (2, 2, 'Child 2');

COMMIT;