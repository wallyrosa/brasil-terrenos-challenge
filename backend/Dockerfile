FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Transfers.slnx ./
COPY src/Transfers.Domain/Transfers.Domain.csproj src/Transfers.Domain/
COPY src/Transfers.Application/Transfers.Application.csproj src/Transfers.Application/
COPY src/Transfers.Infrastructure/Transfers.Infrastructure.csproj src/Transfers.Infrastructure/
COPY src/Transfers.Api/Transfers.Api.csproj src/Transfers.Api/
RUN dotnet restore src/Transfers.Api/Transfers.Api.csproj

COPY src/ src/
RUN dotnet publish src/Transfers.Api/Transfers.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Transfers.Api.dll"]
