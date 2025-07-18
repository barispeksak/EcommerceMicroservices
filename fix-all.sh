#!/bin/bash

# Fix ProductItemMicroservice database
echo "🔧 Creating schema and tables in trendyolDb..."
docker-compose exec postgresql psql -U postgres -d trendyolDb -c "CREATE SCHEMA IF NOT EXISTS item;"
docker-compose exec postgresql psql -U postgres -d trendyolDb -c "
CREATE TABLE IF NOT EXISTS item.product_item (
  id SERIAL PRIMARY KEY,
  sku VARCHAR(30) NOT NULL UNIQUE,
  quantity_in_stock INT NOT NULL,
  price NUMERIC(18,2) NOT NULL,
  currency VARCHAR(3) NOT NULL,
  product_id INT NOT NULL
);"

# Add test product data
echo "🌱 Adding test product data..."
docker-compose exec postgresql psql -U postgres -d trendyolDb -c "
INSERT INTO item.product_item (sku, quantity_in_stock, price, currency, product_id)
VALUES 
  ('1-RED-M', 100, 29.99, 'USD', 1),
  ('2-BLUE-L', 50, 39.99, 'USD', 2),
  ('3-GREEN-S', 75, 24.99, 'USD', 3)
ON CONFLICT (sku) DO NOTHING;"

# Restart services
echo "🔄 Restarting services..."
docker-compose restart productitemmicroservice
docker-compose restart shoppingcartmicroservice
