# ASP.NET Core SDK imajını kullanarak uygulamayı derle
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# NuGet paketlerini geri yüklemek için yalnızca .csproj dosyasını kopyala
COPY "PaymentTypeMicroservice.csproj" .
RUN dotnet restore "./PaymentTypeMicroservice.csproj"

# Geri kalan tüm kaynak kodunu kopyala
COPY . .

# Uygulamayı yayınla
RUN dotnet publish "PaymentTypeMicroservice.csproj" -c Release -o /app/publish

# --- İkinci Aşama: Çalıştırma Ortamı ---
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "PaymentTypeMicroservice.dll"]