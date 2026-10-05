# Dotnet CLI Cheatsheet

## Create a solution
```bash
dotnet new sln -n SampleSolution
```

## Create projects
```bash
dotnet new console -n Sample.App
dotnet new classlib -n Sample.Core
dotnet new webapi -n Sample.Api
dotnet new xunit -n Sample.Tests
```

## Add projects and references
```bash
dotnet sln add Sample.App/Sample.App.csproj
dotnet add Sample.App/Sample.App.csproj reference Sample.Core/Sample.Core.csproj
```

## Restore, build, run, test
```bash
dotnet restore
dotnet build
dotnet run --project Sample.App
dotnet test
```

## Publish
```bash
dotnet publish Sample.App -c Release -o publish
```