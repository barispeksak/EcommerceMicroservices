# ASP.NET Core SDK imajını kullanarak uygulamayı derle
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Çözüm dosyasını ve tüm proje dosyalarını kopyala
COPY "ProductMicroservices.sln" .
COPY "ProductMicroservice_Api/ProductMicroservice_Api.csproj" "ProductMicroservice_Api/"
COPY "ProductMicroservice_Data/ProductMicroservice_Data.csproj" "ProductMicroservice_Data/"
COPY "ProductMicroservice_Service/ProductMicroservice_Service.csproj" "ProductMicroservice_Service/"

# NuGet paketlerini geri yükle
RUN dotnet restore "ProductMicroservices.sln"

# Geri kalan tüm kaynak kodunu kopyala
COPY . .

# Uygulamayı yayınla
RUN dotnet publish "ProductMicroservice_Api/ProductMicroservice_Api.csproj" -c Release -o /app/publish

# --- İkinci Aşama: Çalıştırma Ortamı ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ProductMicroservice_Api.dll"]