FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the solution file if you have one
COPY *.sln ./

# Copy both project files first (for efficient caching)
COPY Common.Contracts/*.csproj Common.Contracts/
COPY ShopOrderMicroservice/*.csproj ShopOrderMicroservice/

# Restore packages
RUN dotnet restore ShopOrderMicroservice/ShopOrderMicroservice.csproj

# Copy the entire source code
COPY Common.Contracts/. Common.Contracts/
COPY ShopOrderMicroservice/. ShopOrderMicroservice/

# Build and publish
RUN dotnet publish ShopOrderMicroservice/ShopOrderMicroservice.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_ENVIRONMENT=Development
ENTRYPOINT ["dotnet", "ShopOrderMicroservice.dll"]