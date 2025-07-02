# ASP.NET Core SDK imajını kullanarak uygulamayı derle
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Çözüm dosyasını ve tüm proje dosyalarını kopyala
COPY "ProductCategoryMicroservice.sln" .
COPY "ProductCategoryMicroservice_Api/ProductCategoryMicroservice_Api.csproj" "ProductCategoryMicroservice_Api/"
COPY "ProductCategoryMicroservice_Data/ProductCategoryMicroservice_Data.csproj" "ProductCategoryMicroservice_Data/"
COPY "ProductCategoryMicroservice_Service/ProductCategoryMicroservice_Service.csproj" "ProductCategoryMicroservice_Service/"

# NuGet paketlerini geri yükle
RUN dotnet restore "ProductCategoryMicroservice.sln"

# Geri kalan tüm kaynak kodunu kopyala
COPY . .

# Uygulamayı yayınla
RUN dotnet publish "ProductCategoryMicroservice_Api/ProductCategoryMicroservice_Api.csproj" -c Release -o /app/publish

# --- İkinci Aşama: Çalıştırma Ortamı ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ProductCategoryMicroservice_Api.dll"]