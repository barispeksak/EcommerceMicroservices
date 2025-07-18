FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the solution file if you have one
COPY *.sln ./

# Copy both project files first (for efficient caching)
COPY Common.Contracts/*.csproj Common.Contracts/
COPY UserAddressMicroservice/*.csproj UserAddressMicroservice/

# Restore packages
RUN dotnet restore UserAddressMicroservice/UserAddressMicroservice.csproj

# Copy the entire source code
COPY Common.Contracts/. Common.Contracts/
COPY UserAddressMicroservice/. UserAddressMicroservice/

# Build and publish
RUN dotnet publish UserAddressMicroservice/UserAddressMicroservice.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "UserAddressMicroservice.dll"]