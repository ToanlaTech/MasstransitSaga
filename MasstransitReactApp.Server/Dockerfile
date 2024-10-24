# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8083
ENV ASPNETCORE_URLS=http://+:8083
ENV ASPNETCORE_ENVIRONMENT=Production

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS with-node
RUN apt-get update
RUN apt-get install curl
RUN curl -sL https://deb.nodesource.com/setup_20.x | bash
RUN apt-get -y install nodejs

# This stage is used to build the service project
FROM with-node AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["MasstransitReactApp.Server/MasstransitReactApp.Server.csproj", "MasstransitReactApp.Server/"]
COPY ["masstransitreactapp.client/masstransitreactapp.client.esproj", "masstransitreactapp.client/"]
COPY ["MasstransitSaga.Core/MasstransitSaga.Core.csproj", "MasstransitSaga.Core/"]
RUN dotnet restore "./MasstransitReactApp.Server/MasstransitReactApp.Server.csproj"
COPY . .
WORKDIR "/src/MasstransitReactApp.Server"
RUN dotnet build "./MasstransitReactApp.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MasstransitReactApp.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MasstransitReactApp.Server.dll"]