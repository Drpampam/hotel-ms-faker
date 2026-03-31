# ── Stage 1: Build ───────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution + project files first (better layer caching)
COPY hotel-ms.sln .
COPY src/hotel-ms.API/hotel-ms.API.csproj           src/hotel-ms.API/
COPY src/hotel-ms.Core/hotel-ms.Core.csproj         src/hotel-ms.Core/
COPY src/hotel-ms.Migrations/hotel-ms.Migrations.csproj src/hotel-ms.Migrations/
COPY src/hotel-ms.Model/hotel-ms.Model.csproj       src/hotel-ms.Model/
COPY src/hotel-ms.Repository/hotel-ms.Repository.csproj src/hotel-ms.Repository/
COPY src/hotel-ms.Service/hotel-ms.Service.csproj   src/hotel-ms.Service/
COPY src/hotel-ms.Test/hotel-ms.Test.csproj         src/hotel-ms.Test/

RUN dotnet restore

# Copy the rest of the source
COPY . .

RUN dotnet publish src/hotel-ms.API/hotel-ms.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Stage 2: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create a non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .

# Render injects PORT at runtime; fall back to 8080 for local Docker runs
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "hotel-ms.API.dll"]
