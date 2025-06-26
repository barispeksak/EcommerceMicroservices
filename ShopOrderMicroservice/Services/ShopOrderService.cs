using Microsoft.EntityFrameworkCore;
using ShopOrderMicroservice.Data;
using ShopOrderMicroservice.DTOs;
using ShopOrderMicroservice.Models;

namespace ShopOrderMicroservice.Services
{
    public class ShopOrderService : IShopOrderService
    {
        private readonly ShopOrderDbContext _context;

        public ShopOrderService(ShopOrderDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ShopOrderDto>> GetShopOrders()
        {
            return await _context.ShopOrders
                .Select(s => new ShopOrderDto
                {
                    Id = s.Id,
                    UserId = s.UserId,
                    OrderDate = s.OrderDate,
                    PaymentId = s.PaymentId,
                    ShippingAddressId = s.ShippingAddressId,
                    ShippingTypeId = s.ShippingTypeId,
                    OrderTotal = s.OrderTotal,
                    OrderStatusId = s.OrderStatusId
                }).ToListAsync();
        }

        public async Task<ShopOrderDto> GetShopOrder(int id)
        {
            var shopOrder = await _context.ShopOrders.FindAsync(id);

            if (shopOrder == null)
            {
                return null;
            }

            return new ShopOrderDto
            {
                Id = shopOrder.Id,
                UserId = shopOrder.UserId,
                OrderDate = shopOrder.OrderDate,
                PaymentId = shopOrder.PaymentId,
                ShippingAddressId = shopOrder.ShippingAddressId,
                ShippingTypeId = shopOrder.ShippingTypeId,
                OrderTotal = shopOrder.OrderTotal,
                OrderStatusId = shopOrder.OrderStatusId
            };
        }

        public async Task<ShopOrderDto> CreateShopOrder(ShopOrderDto shopOrderDto)
        {
            var shopOrder = new ShopOrder
            {
                UserId = shopOrderDto.UserId,
                OrderDate = shopOrderDto.OrderDate,
                PaymentId = shopOrderDto.PaymentId,
                ShippingAddressId = shopOrderDto.ShippingAddressId,
                ShippingTypeId = shopOrderDto.ShippingTypeId,
                OrderTotal = shopOrderDto.OrderTotal,
                OrderStatusId = shopOrderDto.OrderStatusId
            };

            _context.ShopOrders.Add(shopOrder);
            await _context.SaveChangesAsync();

            shopOrderDto.Id = shopOrder.Id;
            return shopOrderDto;
        }

        public async Task<ShopOrderDto> UpdateShopOrder(ShopOrderDto shopOrderDto)
        {
            var shopOrder = await _context.ShopOrders.FindAsync(shopOrderDto.Id);

            if (shopOrder == null)
            {
                return null;
            }

            shopOrder.UserId = shopOrderDto.UserId;
            shopOrder.OrderDate = shopOrderDto.OrderDate;
            shopOrder.PaymentId = shopOrderDto.PaymentId;
            shopOrder.ShippingAddressId = shopOrderDto.ShippingAddressId;
            shopOrder.ShippingTypeId = shopOrderDto.ShippingTypeId;
            shopOrder.OrderTotal = shopOrderDto.OrderTotal;
            shopOrder.OrderStatusId = shopOrderDto.OrderStatusId;

            await _context.SaveChangesAsync();

            return shopOrderDto;
        }

        public async Task<bool> DeleteShopOrder(int id)
        {
            var shopOrder = await _context.ShopOrders.FindAsync(id);
            if (shopOrder == null)
            {
                return false;
            }

            _context.ShopOrders.Remove(shopOrder);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
