# run-all.ps1
# Her servisi ayrı pencerede (veya -NoNewWindow ile aynı pencerede) çalıştırır.
# ♥ Ctrl+C ile tüm pencereleri kapatırken sırasıyla durdur.

$root = $PSScriptRoot   # script dosyasının konumu

$services = @(
    @{ Name = "ProductService";  Path = "$root\ProductService\ProductService.csproj";  Port = 5000 },
    @{ Name = "UserMicroservice"; Path = "$root\User\trendyolApi.csproj"; Port = 5001 },
    @{ Name = "AddressMicroservice"; Path = "$root\AddressMicroservice\AddressMicroservice.csproj"; Port = 5002 }
    @{ Name = "ShopOrderMicroservice"; Path = "$root\ShopOrderMicroservice\ShopOrderMicroservice.csproj"; Port = 5003 }
)

foreach ($srv in $services) {
    Write-Host "⏩  Starting $($srv.Name) on port $($srv.Port)..."
    Start-Process -FilePath "dotnet" `
                  -ArgumentList "run --project `"$($srv.Path)`"" `
                  -WorkingDirectory $root `
                  -WindowStyle Normal
}

Write-Host "`n🚀  All services launched. Close individual windows or press Ctrl+C here to exit.`n"
