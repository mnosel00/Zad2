# ============================================
# Stage 1: Build
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first (layer caching optimization)
COPY Zad2.Core/*.csproj Zad2.Core/
COPY Zad2.Infrastructure/*.csproj Zad2.Infrastructure/
COPY Zad2.API/*.csproj Zad2.API/
COPY Zad2.Tests/*.csproj Zad2.Tests/

# Restore dependencies
RUN dotnet restore Zad2.API/Zad2.API.csproj

# Copy the rest of the source code
COPY . .

# Build and publish in Release configuration
WORKDIR /src/Zad2.API
RUN dotnet publish -c Release -o /app/publish --no-restore

# ============================================
# Stage 2: Runtime
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos "" --home /app appuser && \
    chown -R appuser:appuser /app
USER appuser

# Copy published output from build stage
COPY --from=build --chown=appuser:appuser /app/publish .

# Expose default .NET 8 port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:8080/swagger/index.html || exit 1

# Entry point
ENTRYPOINT ["dotnet", "Zad2.API.dll"]