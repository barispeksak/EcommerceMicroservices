# ASP.NET Core SDK imajını kullanarak uygulamayı derle
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Çözüm dosyasını ve tüm proje dosyalarını kopyala
COPY "ShoppingCartMicroservice.sln" .
COPY "ShoppingCartMicroservice_Api/ShoppingCartMicroservice_Api.csproj" "ShoppingCartMicroservice_Api/"
COPY "ShoppingCartMicroservice_Data/ShoppingCartMicroservice_Data.csproj" "ShoppingCartMicroservice_Data/"
COPY "ShoppingCartMicroservice_Service/ShoppingCartMicroservice_Service.csproj" "ShoppingCartMicroservice_Service/"

# NuGet paketlerini geri yükle
RUN dotnet restore "ShoppingCartMicroservice.sln"

# Geri kalan tüm kaynak kodunu kopyala
COPY . .

# Uygulamayı yayınla
RUN dotnet publish "ShoppingCartMicroservice_Api/ShoppingCartMicroservice_Api.csproj" -c Release -o /app/publish

# --- İkinci Aşama: Çalıştırma Ortamı ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ShoppingCartMicroservice_Api.dll"]