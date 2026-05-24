FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy .csproj files for layer caching
COPY src/IceBackend.Domain/IceBackend.Domain.csproj IceBackend.Domain/
COPY src/IceBackend.Application/IceBackend.Application.csproj IceBackend.Application/
COPY src/IceBackend.Infrastructure/IceBackend.Infrastructure.csproj IceBackend.Infrastructure/
COPY src/IceBackend.Api/IceBackend.Api.csproj IceBackend.Api/
RUN dotnet restore IceBackend.Api/IceBackend.Api.csproj

# Copy all source and publish
COPY src/ .
RUN dotnet publish IceBackend.Api/IceBackend.Api.csproj -c Release -o /app

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
ENTRYPOINT ["dotnet", "IceBackend.Api.dll"]
