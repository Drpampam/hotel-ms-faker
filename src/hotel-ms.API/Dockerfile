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
COPY ["../src/hotel-ms.API/hotel-ms.API.csproj", "src/hotel-ms.API/"]
COPY ["../src/hotel-ms.Service/hotel-ms.Service.csproj", "src/hotel-ms.Service/"]
COPY ["../src/hotel-ms.Repository/hotel-ms.Repository.csproj", "src/hotel-ms.Repository/"]
COPY ["../src/hotel-ms.Migrations/hotel-ms.Migrations.csproj", "src/hotel-ms.Migrations/"]
COPY ["../src/hotel-ms.Model/hotel-ms.Model.csproj", "src/hotel-ms.Model/"]
COPY ["../src/hotel-ms.Core/hotel-ms.Core.csproj", "src/hotel-ms.Core/"]
RUN dotnet restore "./src/hotel-ms.API/hotel-ms.API.csproj"
COPY . .
WORKDIR "/src/src/hotel-ms.API"
RUN dotnet build "./hotel-ms.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./hotel-ms.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "hotel-ms.API.dll"]