# Stage 1: Base image for running the app
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Stage 2: Install Node.js and dependencies
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Install Node.js
RUN apt-get update && apt-get install -y curl
RUN curl -sL https://deb.nodesource.com/setup_20.x | bash
RUN apt-get install -y nodejs

# Copy and restore .NET dependencies
COPY ["react.core.Server/react.core.Server.csproj", "react.core.Server/"]
COPY ["react.core.Client/react.core.Client.esproj", "react.core.Client/"]
RUN dotnet restore "react.core.Server/react.core.Server.csproj"

# Copy everything
COPY . .

# Build React App
WORKDIR /src/react.core.Client
RUN npm cache clean --force
RUN rm -rf node_modules package-lock.json
RUN npm install
RUN npm run build

# Build .NET app
WORKDIR /src/react.core.Server
RUN dotnet build -c Release -o /app/build

# Publish the .NET app
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final Image
FROM base AS final
WORKDIR /app

# Install PostgreSQL client tools in the final image
RUN apt-get update && apt-get install -y wget gnupg2 lsb-release \
    && echo "deb [signed-by=/usr/share/keyrings/pgdg.gpg] http://apt.postgresql.org/pub/repos/apt $(lsb_release -cs)-pgdg main" | tee /etc/apt/sources.list.d/pgdg.list \
    && wget -qO - https://www.postgresql.org/media/keys/ACCC4CF8.asc | gpg --dearmor -o /usr/share/keyrings/pgdg.gpg \
    && apt-get update && apt-get install -y postgresql-client-17 \
    && rm -rf /var/lib/apt/lists/*

# Copy published .NET files
COPY --from=publish /app/publish .

# Copy built React files into wwwroot
COPY --from=build /src/react.core.Client/dist ./wwwroot

# Start the .NET application
ENTRYPOINT ["dotnet", "react.core.Server.dll"]