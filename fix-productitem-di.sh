#!/bin/bash

echo "🔧 Creating ProductApiClient mock to fix dependency injection..."

# Step 1: Create the mock ProductApiClient class
cat > ProductItemMicroservice/ProductItemMicroservice_Service/Services/ProductApiClient.cs << 'END_FILE'
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProductItemMicroservice_Service.Services
{
    public class ProductApiClient
    {
        private readonly HttpClient _httpClient;
        
        public ProductApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        // Add mock methods if needed
        public Task<object> GetProductAsync(int id)
        {
            // Just return a mock result
            return Task.FromResult<object>(new { Id = id, Name = $"Product {id}" });
        }
    }
}
END_FILE

# Step 2: Update Program.cs to register the ProductApiClient
sed -i.bak '/builder\.Services\.AddScoped<IProductItemService, ProductItemService>();/a \\n    // Add ProductApiClient for DI resolution\n    builder.Services.AddHttpClient<ProductApiClient>(client => {\n        client.BaseAddress = new Uri("http://productmicroservice:8080");\n    });' ProductItemMicroservice/ProductItemMicroservice_Api/Program.cs

# Step 3: Rebuild and restart
echo "🔄 Rebuilding ProductItemMicroservice container..."
docker-compose build productitemmicroservice
docker-compose up -d productitemmicroservice

# Step 4: Wait for service to start
echo "⏳ Waiting for service to start..."
sleep 3

# Step 5: Test API endpoint
echo "🧪 Testing API endpoint..."
curl -v http://localhost:5006/api/productitems/1
