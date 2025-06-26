using Microsoft.EntityFrameworkCore;
using OrderSummaryMicroservice.Data;
using OrderSummaryMicroservice.DTOs;
using OrderSummaryMicroservice.Models;

namespace OrderSummaryMicroservice.Services
{
    public class OrderSummaryService : IOrderSummaryService
    {
        private readonly OrderSummaryDbContext _context;

        public OrderSummaryService(OrderSummaryDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderSummaryDto>> GetAllAsync()
        {
            var orderSummaries = await _context.OrderSummaries.ToListAsync();
            return orderSummaries.Select(MapToDto);
        }

        public async Task<OrderSummaryDto?> GetByIdAsync(int id)
        {
            var orderSummary = await _context.OrderSummaries.FindAsync(id);
            return orderSummary != null ? MapToDto(orderSummary) : null;
        }

        public async Task<OrderSummaryDto> CreateAsync(CreateOrderSummaryDto createOrderSummaryDto)
        {
            var orderSummary = new OrderSummary
            {
                OrderId = createOrderSummaryDto.OrderId,
                ProductItemId = createOrderSummaryDto.ProductItemId,
                Quantity = createOrderSummaryDto.Quantity,
                Price = createOrderSummaryDto.Price
            };

            _context.OrderSummaries.Add(orderSummary);
            await _context.SaveChangesAsync();

            return MapToDto(orderSummary);
        }

        public async Task<OrderSummaryDto?> UpdateAsync(int id, UpdateOrderSummaryDto updateOrderSummaryDto)
        {
            var orderSummary = await _context.OrderSummaries.FindAsync(id);
            if (orderSummary == null)
                return null;

            orderSummary.Quantity = updateOrderSummaryDto.Quantity;
            orderSummary.Price = updateOrderSummaryDto.Price;

            await _context.SaveChangesAsync();
            return MapToDto(orderSummary);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var orderSummary = await _context.OrderSummaries.FindAsync(id);
            if (orderSummary == null)
                return false;

            _context.OrderSummaries.Remove(orderSummary);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.OrderSummaries.AnyAsync(os => os.Id == id);
        }

        private static OrderSummaryDto MapToDto(OrderSummary orderSummary)
        {
            return new OrderSummaryDto
            {
                Id = orderSummary.Id,
                OrderId = orderSummary.OrderId,
                ProductItemId = orderSummary.ProductItemId,
                Quantity = orderSummary.Quantity,
                Price = orderSummary.Price
            };
        }
    }
}