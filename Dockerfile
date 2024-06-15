FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5260

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/CustomerLedger.Web/CustomerLedger.Web.csproj", "src/CustomerLedger.Web/"]
COPY ["src/CustomerLedger.Application/CustomerLedger.Application.csproj", "src/CustomerLedger.Application/"]
COPY ["src/CustomerLedger.Domain/CustomerLedger.Domain.csproj", "src/CustomerLedger.Domain/"]
COPY ["src/CustomerLedger.Infrastructure/CustomerLedger.Infrastructure.csproj", "src/CustomerLedger.Infrastructure/"]
RUN dotnet restore "src/CustomerLedger.Web/CustomerLedger.Web.csproj"
COPY . .
WORKDIR "/src/src/CustomerLedger.Web"
RUN dotnet build "CustomerLedger.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CustomerLedger.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CustomerLedger.Web.dll"]
