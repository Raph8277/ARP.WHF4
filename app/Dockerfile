# ==== BUILD ====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copier les csproj et restaurer
COPY src/Wfrp4.Shared/Wfrp4.Shared.csproj src/Wfrp4.Shared/
COPY src/Wfrp4.Infrastructure/Wfrp4.Infrastructure.csproj src/Wfrp4.Infrastructure/
COPY src/Wfrp4.Client/Wfrp4.Client.csproj src/Wfrp4.Client/
COPY src/Wfrp4.Server/Wfrp4.Server.csproj src/Wfrp4.Server/
RUN dotnet restore src/Wfrp4.Server/Wfrp4.Server.csproj

# Copier tout le code et publier
COPY src/ src/
RUN dotnet publish src/Wfrp4.Server/Wfrp4.Server.csproj -c Release -o /app/publish --no-restore

# ==== RUNTIME ====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

EXPOSE 8081

ENV ASPNETCORE_URLS=http://+:8081
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Wfrp4.Server.dll"]