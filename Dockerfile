FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine as builder

WORKDIR /src/configurable.web
COPY src/configurable.web/configurable.web.csproj .
RUN dotnet restore

COPY src /src
RUN dotnet publish -c Release -o /out configurable.web.csproj --no-restore

# app image
FROM  mcr.microsoft.com/dotnet/aspnet:5.0-alpine

EXPOSE 80
ENTRYPOINT ["dotnet", "/app/configurable.web.dll"]

WORKDIR /app
COPY --from=builder /out/ .