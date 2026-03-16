# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
#USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["../src/hotelier-core-app.API/hotelier-core-app.API.csproj", "src/hotelier-core-app.API/"]
COPY ["../src/hotelier-core-app.Service/hotelier-core-app.Service.csproj", "src/hotelier-core-app.Service/"]
COPY ["../src/hotelier-core-app.Repository/hotelier-core-app.Repository.csproj", "src/hotelier-core-app.Repository/"]
COPY ["../src/hotelier-core-app.Migrations/hotelier-core-app.Migrations.csproj", "src/hotelier-core-app.Migrations/"]
COPY ["../src/hotelier-core-app.Model/hotelier-core-app.Model.csproj", "src/hotelier-core-app.Model/"]
COPY ["../src/hotelier-core-app.Core/hotelier-core-app.Core.csproj", "src/hotelier-core-app.Core/"]
RUN dotnet restore "./src/hotelier-core-app.API/hotelier-core-app.API.csproj"
COPY . .
WORKDIR "/src/src/hotelier-core-app.API"
RUN dotnet build "./hotelier-core-app.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./hotelier-core-app.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "hotelier-core-app.API.dll"]