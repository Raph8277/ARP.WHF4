---
name: deploy-docker
description: Use when you need to deploy the WFRP4 app via Docker Compose — rebuild image, restart container, verify migration and startup, check logs for errors. Safe deployment that preserves existing database data.
tools: Read, Grep, Glob, Bash
---
Deploy the WFRP4 application by rebuilding and restarting the Docker Compose stack.

## Rules
- NEVER use `docker compose down -v` — the `pgdata` volume holds persistent data.
- Only rebuild and restart `wfrp4-app` unless explicitly told to restart other services.
- Always verify the database migration ran successfully after restart.
- Always check application logs for startup errors.
- Report clearly if something fails — do not silently ignore errors.

## Workflow
1. **Pre-flight checks**
   - Verify Docker daemon is running (`docker info`).
   - Verify `docker-compose.yml` exists and is valid (`docker compose config --quiet`).
   - Check that postgres container is healthy (`docker compose ps postgres`).

2. **Build and deploy**
   - Rebuild only the app image: `docker compose build wfrp4-app`
   - Restart only the app container: `docker compose up -d wfrp4-app`

3. **Post-deploy verification**
   - Wait a few seconds for startup, then check container status: `docker compose ps wfrp4-app`
   - Read app logs to confirm: `docker compose logs --tail=50 wfrp4-app`
     - EF Core migration applied successfully (look for `Applying migration`)
     - No unhandled exceptions at startup
     - App is listening on the expected port

4. **Report**
   - Container status (running/exited)
   - Migration result (applied / already up-to-date / failed)
   - Any errors or warnings from logs
   - URL where the app is accessible (typically http://localhost:5080)

## Output
Goal · Pre-flight result · Build result · Container status · Migration status · Errors (if any) · Access URL
