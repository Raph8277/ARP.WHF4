# SQLite DDL Cheatsheet

## Base pragma
```sql
PRAGMA foreign_keys = ON;
```

## Table example
```sql
CREATE TABLE IF NOT EXISTS project (
    id INTEGER PRIMARY KEY,
    code TEXT NOT NULL UNIQUE,
    name TEXT NOT NULL
);
```

## Foreign key example
```sql
CREATE TABLE IF NOT EXISTS task (
    id INTEGER PRIMARY KEY,
    project_id INTEGER NOT NULL,
    title TEXT NOT NULL,
    status TEXT NOT NULL,
    FOREIGN KEY (project_id) REFERENCES project(id)
);
```

## Index example
```sql
CREATE INDEX IF NOT EXISTS idx_task_project_id ON task(project_id);
```

## Database creation
```bash
sqlite3 app.db < schema.sql
```