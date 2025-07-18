FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj files first - CORRECTED PATHS
COPY ["ShoppingCartMicroservice/ShoppingCartMicroservice_Api/ShoppingCartMicroservice_Api.csproj", "ShoppingCartMicroservice/ShoppingCartMicroservice_Api/"]
COPY ["ShoppingCartMicroservice/ShoppingCartMicroservice_Service/ShoppingCartMicroservice_Service.csproj", "ShoppingCartMicroservice/ShoppingCartMicroservice_Service/"]
COPY ["Common.Contracts/Common.Contracts.csproj", "Common.Contracts/"]

# Restore dependencies with correct project path
RUN dotnet restore "ShoppingCartMicroservice/ShoppingCartMicroservice_Api/ShoppingCartMicroservice_Api.csproj"

# Copy full source code
COPY ["ShoppingCartMicroservice/ShoppingCartMicroservice_Api/", "ShoppingCartMicroservice/ShoppingCartMicroservice_Api/"]
COPY ["ShoppingCartMicroservice/ShoppingCartMicroservice_Service/", "ShoppingCartMicroservice/ShoppingCartMicroservice_Service/"]
COPY ["Common.Contracts/", "Common.Contracts/"]

# Build with correct project path
WORKDIR /src
RUN dotnet publish "ShoppingCartMicroservice/ShoppingCartMicroservice_Api/ShoppingCartMicroservice_Api.csproj" -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ShoppingCartMicroservice_Api.dll"]