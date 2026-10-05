# Testing Patterns

## Unit tests
- Prefer unit tests for domain logic and small services.
- Keep external dependencies mocked or isolated.

## Integration tests
- Use integration tests for data access, HTTP boundaries, or DI wiring.
- Keep setup explicit and repeatable.

## Coverage guidance
- Cover core behavior and failure paths first.
- Avoid duplicating implementation details in tests.

## Tooling notes
- xUnit is a pragmatic default for workshop examples.
- Use `dotnet test` as the baseline validation command.