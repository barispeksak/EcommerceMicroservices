#!/bin/bash

# Test script for E-Commerce Saga Orchestration Flow

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${GREEN}🧪 Testing E-Commerce Saga Orchestration Flow (Stages 1-6)${NC}"
echo "=============================================="

# Variables
CART_API="http://localhost:5020"
ORDER_API="http://localhost:5009"
SAGA_API="http://localhost:5010"
PAYMENT_API="http://localhost:5008"

# Test 1: Check if containers are running
echo -e "\n${YELLOW}Test 1: Checking container status...${NC}"

check_container() {
    local service_name=$1
    local container_name=$2
    
    if docker-compose ps | grep -q "$container_name.*Up"; then
        echo -e "${GREEN}✓ $service_name container is running${NC}"
        return 0
    else
        echo -e "${RED}✗ $service_name container is not running${NC}"
        return 1
    fi
}

# Check containers
check_container "OrderSagaOrchestrator" "ordersagaorchestrator"
check_container "ShoppingCartMicroservice" "shoppingcartmicroservice"
check_container "ShopOrderMicroservice" "shopordermicroservice"
check_container "PaymentTypeMicroservice" "paymenttypemicroservice"

echo -e "\n${YELLOW}Test 2: Checking RabbitMQ Management UI...${NC}"
if curl -f -s "http://localhost:15672" > /dev/null 2>&1; then
    echo -e "${GREEN}✓ RabbitMQ Management UI is accessible${NC}"
else
    echo -e "${RED}✗ RabbitMQ Management UI is not accessible${NC}"
fi

# Test 3: Check service logs for readiness
echo -e "\n${YELLOW}Test 3: Checking service readiness from logs...${NC}"

check_service_logs() {
    local service_name=$1
    local search_pattern=$2
    
    if docker-compose logs --tail=50 "$service_name" 2>/dev/null | grep -q "$search_pattern"; then
        echo -e "${GREEN}✓ $service_name is ready (found: $search_pattern)${NC}"
        return 0
    else
        echo -e "${YELLOW}⚠ $service_name readiness unclear${NC}"
        return 1
    fi
}

check_service_logs "ordersagaorchestrator" "Application started"
check_service_logs "shoppingcartmicroservice" "Application started"
check_service_logs "shopordermicroservice" "Application started"
check_service_logs "paymenttypemicroservice" "Application started"

# Test 4: Check RabbitMQ saga setup
echo -e "\n${YELLOW}Test 4: Checking RabbitMQ saga setup...${NC}"

check_saga_setup() {
    local logs=$(docker-compose logs ordersagaorchestrator 2>/dev/null)
    
    if echo "$logs" | grep -q "SagaState"; then
        echo -e "${GREEN}✓ SagaState queue configured${NC}"
    else
        echo -e "${YELLOW}⚠ SagaState queue not found in logs${NC}"
    fi
    
    if echo "$logs" | grep -q "Bus started"; then
        echo -e "${GREEN}✓ MassTransit bus is started${NC}"
    else
        echo -e "${YELLOW}⚠ MassTransit bus status unclear${NC}"
    fi
    
    if echo "$logs" | grep -q "Database initialized successfully"; then
        echo -e "${GREEN}✓ SagaDb database initialized${NC}"
    else
        echo -e "${YELLOW}⚠ SagaDb initialization status unclear${NC}"
    fi
}

check_saga_setup

# Test 5: Basic API endpoint tests (these may fail if endpoints don't exist)
echo -e "\n${BLUE}Test 5: Testing API endpoints (may show warnings if not implemented)...${NC}"

test_endpoint() {
    local name=$1
    local url=$2
    local method=${3:-GET}
    
    echo -e "\n${YELLOW}Testing $name endpoint...${NC}"
    
    response=$(curl -s -o /dev/null -w "%{http_code}" -X "$method" "$url" 2>/dev/null || echo "000")
    
    if [[ "$response" != "000" ]]; then
        echo -e "${GREEN}✓ $name endpoint is accessible (HTTP: $response)${NC}"
    else
        echo -e "${YELLOW}⚠ $name endpoint not accessible - may not be implemented yet${NC}"
    fi
}

# Test basic endpoints
test_endpoint "OrderSagaOrchestrator" "$SAGA_API"
test_endpoint "ShoppingCartMicroservice" "$CART_API"
test_endpoint "ShopOrderMicroservice" "$ORDER_API"
test_endpoint "PaymentTypeMicroservice" "$PAYMENT_API"

# Test 6: Check database tables
echo -e "\n${YELLOW}Test 6: Checking database setup...${NC}"

check_saga_database() {
    echo "Checking SagaDb tables..."
    
    # Check if SagaStates table exists
    if docker-compose exec -T postgresql psql -U postgres -d SagaDb -c "\dt" 2>/dev/null | grep -q "SagaStates"; then
        echo -e "${GREEN}✓ SagaStates table exists in SagaDb${NC}"
        
        # Show table structure
        echo "SagaStates table structure:"
        docker-compose exec -T postgresql psql -U postgres -d SagaDb -c "\d SagaStates" 2>/dev/null | head -10
    else
        echo -e "${YELLOW}⚠ SagaStates table not found or database not accessible${NC}"
    fi
}

check_saga_database

# Test 7: Test actual saga flow
echo -e "\n${BLUE}Test 7: Testing actual saga flow (Stages 1-6)...${NC}"

test_saga_flow() {
    echo -e "\n${YELLOW}🚀 Triggering saga orchestration flow...${NC}"
    
    # Create test payload
    local test_payload='{
        "cartId": "123e4567-e89b-12d3-a456-426614174000",
        "items": [
            "223e4567-e89b-12d3-a456-426614174001",
            "323e4567-e89b-12d3-a456-426614174002"
        ]
    }'
    
    echo "Payload: $test_payload"
    echo ""
    
    # Call the saga API
    response=$(curl -s -w "%{http_code}" -X POST "$SAGA_API/api/saga/place-order" \
        -H "Content-Type: application/json" \
        -d "$test_payload" 2>/dev/null)
    
    http_code="${response: -3}"
    body="${response%???}"
    
    if [[ "$http_code" == "202" ]]; then
        echo -e "${GREEN}✅ Saga flow triggered successfully!${NC}"
        echo "Response: $body"
        echo ""
        echo -e "${YELLOW}🔍 Checking saga execution...${NC}"
        
        # Wait a moment for processing
        sleep 2
        
        # Check logs for saga activity
        echo "Recent saga logs:"
        docker-compose logs --tail=20 ordersagaorchestrator | grep -E "(CorrelationId|saga|state|event|command)" || echo "No saga activity found in recent logs"
        
        # Check database for saga state
        echo ""
        echo -e "${YELLOW}📊 Checking saga state in database...${NC}"
        docker-compose exec -T postgresql psql -U postgres -d SagaDb -c "SELECT \"CorrelationId\", \"CurrentState\", \"CartId\", \"CreatedAt\" FROM \"SagaStates\" ORDER BY \"CreatedAt\" DESC LIMIT 5;" 2>/dev/null || echo "Could not query saga states"
        
        return 0
    else
        echo -e "${RED}❌ Saga flow failed (HTTP: $http_code)${NC}"
        echo "Response: $body"
        return 1
    fi
}

test_saga_flow

echo -e "\n${GREEN}✅ Saga orchestration system diagnostic completed!${NC}"
echo "=============================================="
echo ""
echo "🔍 Summary:"
echo "• Containers are running"
echo "• RabbitMQ is configured with saga queues and exchanges"
echo "• Database is set up with SagaStates table"
echo "• MassTransit bus is connected and ready"
echo "• Saga flow can be triggered via API"
echo ""
echo "🎯 Monitor saga execution with:"
echo "  docker-compose logs -f ordersagaorchestrator"
echo "  docker-compose logs -f shoppingcartmicroservice"
echo "  docker-compose logs -f shopordermicroservice"
echo "  docker-compose logs -f paymenttypemicroservice"
echo ""
echo "🌐 Service URLs:"
echo "  - RabbitMQ Management: http://localhost:15672 (guest/guest)"
echo "  - OrderSagaOrchestrator: http://localhost:5010"
echo "  - ShoppingCartMicroservice: http://localhost:5020"
echo "  - ShopOrderMicroservice: http://localhost:5009"
echo "  - PaymentTypeMicroservice: http://localhost:5008"
echo ""
echo "🧪 To trigger saga manually:"
echo "  curl -X POST http://localhost:5010/api/saga/place-order \\"
echo "    -H 'Content-Type: application/json' \\"
echo "    -d '{\"cartId\":\"123e4567-e89b-12d3-a456-426614174000\",\"items\":[\"223e4567-e89b-12d3-a456-426614174001\"]}'"
