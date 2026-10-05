PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS entity_a (
    id INTEGER PRIMARY KEY,
    name TEXT NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS entity_b (
    id INTEGER PRIMARY KEY,
    entity_a_id INTEGER NOT NULL,
    label TEXT NOT NULL,
    FOREIGN KEY (entity_a_id) REFERENCES entity_a(id)
);

CREATE INDEX IF NOT EXISTS idx_entity_b_entity_a_id ON entity_b(entity_a_id);