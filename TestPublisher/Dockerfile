FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["TestPublisher/TestPublisher.csproj", "TestPublisher/"]
COPY ["Common.Contracts/Common.Contracts.csproj", "Common.Contracts/"]
RUN dotnet restore "TestPublisher/TestPublisher.csproj"
COPY . .
WORKDIR "/src/TestPublisher"
RUN dotnet build "TestPublisher.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TestPublisher.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestPublisher.dll"]
