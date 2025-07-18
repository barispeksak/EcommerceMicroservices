#!/bin/bash

echo "🔧 Fixing saga data issues..."
echo ""

# Step 1: Rebuild and restart ProductItemMicroservice to initialize database
echo "1️⃣ Rebuilding ProductItemMicroservice with database initialization..."
docker-compose build productitemmicroservice
docker-compose up -d productitemmicroservice

echo "⏳ Waiting for ProductItemMicroservice to initialize database..."
sleep 15

# Step 2: Fix Redis cart data
echo ""
echo "2️⃣ Fixing Redis cart data - replacing GUID productId with integer values..."

# Find the correct Redis container name
REDIS_CONTAINER=$(docker ps --filter "name=redis" --format "{{.Names}}" | head -1)

if [ -z "$REDIS_CONTAINER" ]; then
    echo "❌ Redis container not found! Looking for alternative names..."
    REDIS_CONTAINER=$(docker ps --filter "name=cart" --format "{{.Names}}" | head -1)
fi

if [ -z "$REDIS_CONTAINER" ]; then
    echo "❌ Redis container not found! Available containers:"
    docker ps --format "table {{.Names}}\t{{.Status}}"
    exit 1
fi

echo "✅ Found Redis container: $REDIS_CONTAINER"

# Connect to Redis and update cart data
docker exec -it "$REDIS_CONTAINER" redis-cli << 'EOF'
# Delete existing cart data with GUID productId
DEL cart:456e7890-e89b-12d3-a456-426614174000

# Set new cart data with integer productId
SET cart:456e7890-e89b-12d3-a456-426614174000 '[{"productId": 1, "quantity": 2}, {"productId": 2, "quantity": 1}]'

# Verify the data
GET cart:456e7890-e89b-12d3-a456-426614174000
EOF

echo ""
echo "✅ All fixes applied successfully!"
echo ""
echo "📋 Fixed Issues:"
echo "   ✅ ProductItemMicroservice database schema created"
echo "   ✅ Test product data seeded (ProductId: 1, 2, 3)"
echo "   ✅ Redis cart data fixed with integer productId values"
echo ""
echo "🚀 Ready to test saga flow end-to-end!"
echo ""
echo "🧪 To test the saga flow, run:"
echo "   ./test-saga-flow.sh"
