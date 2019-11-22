FROM mcr.microsoft.com/dotnet/core/runtime:3.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster AS build
WORKDIR /src
COPY ["ServiceSiteHost/ServiceSiteHost.csproj", "ServiceSiteHost/"]
RUN dotnet restore "ServiceSiteHost/ServiceSiteHost.csproj"
COPY . .
WORKDIR "/src/ServiceSiteHost"
RUN dotnet build "ServiceSiteHost.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ServiceSiteHost.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServiceSiteHost.dll"]