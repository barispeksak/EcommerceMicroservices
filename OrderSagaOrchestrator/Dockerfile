# ---------- build aşaması ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Tüm repo kopyalansın ki Common.Contracts referansı çözülsün
COPY . .

# Restore & Publish
RUN dotnet restore
RUN dotnet publish OrderSagaOrchestrator/OrderSagaOrchestrator.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# ---------- runtime aşaması ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OrderSagaOrchestrator.dll"]