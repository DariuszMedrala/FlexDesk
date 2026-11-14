# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props .editorconfig ./
COPY src/FlexDesk.Api/FlexDesk.Api.csproj src/FlexDesk.Api/
COPY src/FlexDesk.Application/FlexDesk.Application.csproj src/FlexDesk.Application/
COPY src/FlexDesk.Domain/FlexDesk.Domain.csproj src/FlexDesk.Domain/
COPY src/FlexDesk.Infrastructure/FlexDesk.Infrastructure.csproj src/FlexDesk.Infrastructure/
RUN dotnet restore src/FlexDesk.Api/FlexDesk.Api.csproj

COPY src/ src/
RUN dotnet publish src/FlexDesk.Api/FlexDesk.Api.csproj -c Release -o /app --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "FlexDesk.Api.dll"]
