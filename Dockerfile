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

#Setup verdaccio
RUN npm config set registry http://65.108.83.60:4873 \
    && npm config get registry


# Copy and restore .NET dependencies
COPY ["react.core.server/react.coreserver.csproj", "react.core.server/"]
COPY ["react.core.client/react.core.client.esproj", "react.core.client/"]
RUN dotnet restore "react.core.server/react.core.server.csproj"

# Copy everything
COPY . .

# Build React App
WORKDIR /src/react.core.Client
RUN npm cache clean --force
RUN rm -rf node_modules package-lock.json
RUN npm install
RUN npm run build

# Build .NET app
WORKDIR /src/react.core.server
RUN dotnet build -c Release -o /app/build

# Publish the .NET app
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final Image
FROM base AS final
WORKDIR /app

# Copy published .NET files
COPY --from=publish /app/publish .

# Copy built React files into wwwroot
COPY --from=build /src/react.core.client/dist ./wwwroot

# Start the .NET application
ENTRYPOINT ["dotnet", "react.core.server.dll"]