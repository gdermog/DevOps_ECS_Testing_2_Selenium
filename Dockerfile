# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base

USER root
RUN apt-get update && DEBIAN_FRONTEND=noninteractive apt-get install -y --no-install-recommends \
    firefox-esr \
    libgtk-3-0 libdbus-glib-1-2 libasound2 libnss3 libx11-6 libx11-xcb1 \
    libxdamage1 libxfixes3 libxcomposite1 libxcursor1 libxi6 libxtst6 libxrandr2 \
    libatk1.0-0 libatk-bridge2.0-0 libcups2 libgdk-pixbuf2.0-0 fonts-liberation \
    ca-certificates && \
    rm -rf /var/lib/apt/lists/*

USER $APP_UID
WORKDIR /app

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ECS2Selenium.csproj", "."]
RUN dotnet restore "./ECS2Selenium.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./ECS2Selenium.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ECS2Selenium.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ECS2Selenium.dll"]