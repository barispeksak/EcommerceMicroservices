#!/bin/bash

echo "🔧 Fixing Redis cart data - replacing GUID productId with integer values..."

# Connect to Redis and update cart data
docker exec -it redis-cart redis-cli << 'EOF'
# Delete existing cart data with GUID productId
DEL cart:456e7890-e89b-12d3-a456-426614174000

# Set new cart data with integer productId
SET cart:456e7890-e89b-12d3-a456-426614174000 '[{"productId": 1, "quantity": 2}, {"productId": 2, "quantity": 1}]'

# Verify the data
GET cart:456e7890-e89b-12d3-a456-426614174000
EOF

echo "✅ Redis cart data updated successfully!"
echo "📋 Cart data now contains:"
echo "   - Product ID 1: quantity 2"
echo "   - Product ID 2: quantity 1"
echo ""
echo "🚀 Ready to test saga flow with correct integer productId values"
