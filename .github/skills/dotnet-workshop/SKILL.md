---
name: dotnet-workshop
description: 'Dotnet workshop workflow for solution design, C# implementation, project setup, testing, documentation, and Mermaid architecture diagrams.'
argument-hint: 'Describe the application type, target framework, expected projects, quality requirements, and documentation outputs.'
user-invocable: true
---

# Dotnet Workshop

Use this skill when the request involves .NET solution design, C# implementation, project setup, testing, Markdown documentation, or Mermaid diagrams.

## What This Skill Covers
- Define a .NET solution or project structure.
- Generate SDK-style project files and C# source code.
- Produce build, run, test, and publish commands.
- Recommend a test strategy and representative tests.
- Produce workshop-grade Markdown documentation and Mermaid diagrams.

## Workflow
1. Confirm the application type, target framework, and expected deliverable.
2. Load the architecture guidance in [solution-patterns](./references/solution-patterns.md).
3. Load the output contract in [output-contract](./references/output-contract.md).
4. If CLI work is requested, use [dotnet-cli-cheatsheet](./references/dotnet-cli-cheatsheet.md).
5. If tests are requested, use [testing-patterns](./references/testing-patterns.md).
6. If documentation is requested, use [architecture](./references/architecture.md) and [review-checklist](./references/review-checklist.md).
7. If a diagram is requested, use [mermaid-cheatsheet](./references/mermaid-cheatsheet.md).

## Standard Commands
### Create a console app
```bash
dotnet new console -n SampleApp
dotnet build SampleApp
dotnet run --project SampleApp
```

### Create a web API with tests
```bash
dotnet new sln -n SampleSolution
dotnet new webapi -n Sample.Api
dotnet new xunit -n Sample.Api.Tests
dotnet sln add Sample.Api/Sample.Api.csproj
dotnet sln add Sample.Api.Tests/Sample.Api.Tests.csproj
dotnet add Sample.Api.Tests/Sample.Api.Tests.csproj reference Sample.Api/Sample.Api.csproj
dotnet test
```

## Expected Outputs
- A .NET architecture recommendation.
- The exact source and project artifacts to create or update.
- Build, run, and test commands.
- Optional Markdown documentation.
- Optional Mermaid architecture diagram.

## Resources
- [Architecture](./references/architecture.md)
- [Output contract](./references/output-contract.md)
- [Solution patterns](./references/solution-patterns.md)
- [Dotnet CLI cheatsheet](./references/dotnet-cli-cheatsheet.md)
- [Testing patterns](./references/testing-patterns.md)
- [Mermaid cheatsheet](./references/mermaid-cheatsheet.md)
- [Review checklist](./references/review-checklist.md)
- [Starter prompt](./references/starter-prompt.md)
- [Project template](./assets/project-template.csproj)
- [Program template](./assets/program-template.cs)
- [Test template](./assets/test-template.cs)
- [Documentation template](./assets/documentation-template.md)