# ASP.NET Core SDK imajını kullanarak uygulamayı derle
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Çözüm dosyasını ve tüm proje dosyalarını kopyala
COPY "ProductConfigurationMicroservice.sln" .
COPY "ProductConfigurationMicroservice_Api/ProductConfigurationMicroservice_Api.csproj" "ProductConfigurationMicroservice_Api/"
COPY "ProductConfigurationMicroservice_Data/ProductConfigurationMicroservice_Data.csproj" "ProductConfigurationMicroservice_Data/"
COPY "ProductConfigurationMicroservice_Service/ProductConfigurationMicroservice_Service.csproj" "ProductConfigurationMicroservice_Service/"

# NuGet paketlerini geri yükle
RUN dotnet restore "ProductConfigurationMicroservice.sln"

# Geri kalan tüm kaynak kodunu kopyala
COPY . .

# Uygulamayı yayınla
RUN dotnet publish "ProductConfigurationMicroservice_Api/ProductConfigurationMicroservice_Api.csproj" -c Release -o /app/publish

# --- İkinci Aşama: Çalıştırma Ortamı ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ProductConfigurationMicroservice_Api.dll"]