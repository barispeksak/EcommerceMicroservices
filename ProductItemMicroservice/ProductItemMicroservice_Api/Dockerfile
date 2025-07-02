# ASP.NET Core SDK imajını kullanarak uygulamayı derle
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Çözüm dosyasını ve tüm proje dosyalarını kopyala
COPY "ProductItemMicroservice.sln" .
COPY "ProductItemMicroservice_Api/ProductItemMicroservice_Api.csproj" "ProductItemMicroservice_Api/"
COPY "ProductItemMicroservice_Data/ProductItemMicroservice_Data.csproj" "ProductItemMicroservice_Data/"
COPY "ProductItemMicroservice_Service/ProductItemMicroservice_Service.csproj" "ProductItemMicroservice_Service/"

# NuGet paketlerini geri yükle
RUN dotnet restore "ProductItemMicroservice.sln"

# Geri kalan tüm kaynak kodunu kopyala
COPY . .

# Uygulamayı yayınla
RUN dotnet publish "ProductItemMicroservice_Api/ProductItemMicroservice_Api.csproj" -c Release -o /app/publish

# --- İkinci Aşama: Çalıştırma Ortamı ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ProductItemMicroservice_Api.dll"]