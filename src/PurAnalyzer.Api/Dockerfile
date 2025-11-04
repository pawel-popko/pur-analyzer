# ===========================================================
#  PurAnalyzer.Api - Multi-stage Docker build
#  Stage 1: Build and publish the .NET application
#  Stage 2: Runtime container (lightweight)
# ===========================================================

# -------- Stage 1: Build --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file and project files separately to leverage Docker layer caching
COPY ./PurAnalyzer.sln ./
COPY ./src/PurAnalyzer.Api/PurAnalyzer.Api.csproj ./src/PurAnalyzer.Api/
COPY ./src/PurAnalyzer.Infrastructure/PurAnalyzer.Infrastructure.csproj ./src/PurAnalyzer.Infrastructure/
COPY ./src/PurAnalyzer.Application/PurAnalyzer.Application.csproj ./src/PurAnalyzer.Application/
COPY ./src/PurAnalyzer.Domain/PurAnalyzer.Domain.csproj ./src/PurAnalyzer.Domain/

# Restore NuGet packages (using only csproj files for caching efficiency)
RUN dotnet restore ./src/PurAnalyzer.Api/PurAnalyzer.Api.csproj

# Copy all remaining source files
COPY ./src ./src

# Publish the application in Release mode to a self-contained folder
RUN dotnet publish ./src/PurAnalyzer.Api/PurAnalyzer.Api.csproj -c Release -o /app/publish

# -------- Stage 2: Runtime --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Expose HTTP port (you can add 8443 if HTTPS is later configured)
EXPOSE 8080

# Set default URL binding for Kestrel (listen on all interfaces)
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Copy the published output from the build stage
COPY --from=build /app/publish ./

# Optional: set environment explicitly for development
ENV ASPNETCORE_ENVIRONMENT=Development

# Set the entry point (start the Web API)
ENTRYPOINT ["dotnet", "PurAnalyzer.Api.dll"]
