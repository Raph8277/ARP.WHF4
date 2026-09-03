# Solution Patterns

## Single console project
- Use for utilities, import/export jobs, and small automation tools.
- Prefer one SDK-style project plus one optional test project.

## Web API plus tests
- Use one API project and one test project.
- Add a solution file when more than one project exists.

## Layered application
- Use separate projects for domain, application, infrastructure, and host only when the complexity justifies it.
- Keep dependency flow inward and explicit.

## Library-first design
- Put reusable business logic in a class library.
- Keep application hosts thin.