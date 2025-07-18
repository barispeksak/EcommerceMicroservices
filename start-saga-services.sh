#!/bin/bash

# Start script for E-Commerce Saga Orchestration Services

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}🚀 Starting E-Commerce Saga Orchestration Services${NC}"
echo "=================================================="

# Step 1: Start infrastructure services
echo -e "\n${YELLOW}Step 1: Starting infrastructure services...${NC}"
docker-compose up -d postgresql rabbitmq redis mongo

# Step 2: Start core microservices needed for saga
echo -e "\n${YELLOW}Step 2: Starting core microservices...${NC}"
docker-compose up -d \
  authservice \
  productitemmicroservice \
  paymenttypemicroservice \
  shopordermicroservice \
  shoppingcartmicroservice \
  ordersagaorchestrator

echo -e "\n${GREEN}✅ All saga orchestration services started!${NC}"
echo "=================================================="
echo ""
echo "🔗 Service URLs:"
echo "  - RabbitMQ Management: http://localhost:15672 (guest/guest)"
echo "  - OrderSagaOrchestrator: http://localhost:5010"
echo "  - ShoppingCartMicroservice: http://localhost:5020"
echo "  - ShopOrderMicroservice: http://localhost:5009"
echo "  - PaymentTypeMicroservice: http://localhost:5008"
echo ""
echo "📋 Check service logs:"
echo "  docker-compose logs -f ordersagaorchestrator"
echo "  docker-compose logs -f shoppingcartmicroservice"
echo ""
echo "🛑 To stop services: docker-compose down"
