# syntax=docker/dockerfile:1

# Builds both the Api and the one-shot Migrations runner from a single multi-stage Dockerfile.
# Build context must be src/EnsyInc.Loom (see repo-root docker-compose.yml).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore EnsyInc.Loom.slnx

# dotnet dev-certs is SDK-only tooling; generate the HTTPS dev cert here so it can be copied into
# the runtime-only Api image below. The cert only needs to exist for Kestrel to serve HTTPS with -
# EnsyInc.Loom.ServiceTests already bypasses server cert validation on the client side, so
# trusting the cert on the host/container isn't required.
ARG DEV_CERT_PASSWORD=DevCertPass123!
RUN mkdir -p /https && dotnet dev-certs https -ep /https/aspnetcore.pfx -p "$DEV_CERT_PASSWORD"

FROM build AS publish-api
RUN dotnet publish EnsyInc.Loom.Api/EnsyInc.Loom.Api.csproj -c Release -o /app/publish --no-restore

FROM build AS publish-migrations
RUN dotnet publish EnsyInc.Loom.Migrations/EnsyInc.Loom.Migrations.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS api
WORKDIR /app
ARG DEV_CERT_PASSWORD=DevCertPass123!
COPY --from=publish-api /app/publish .
COPY --from=build /https/aspnetcore.pfx /https/aspnetcore.pfx
ENV ASPNETCORE_URLS="https://+:7088;http://+:5029" \
    ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetcore.pfx \
    ASPNETCORE_Kestrel__Certificates__Default__Password=$DEV_CERT_PASSWORD
EXPOSE 7088 5029
ENTRYPOINT ["dotnet", "EnsyInc.Loom.Api.dll"]

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS migrations
WORKDIR /app
COPY --from=publish-migrations /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "EnsyInc.Loom.Migrations.dll"]
