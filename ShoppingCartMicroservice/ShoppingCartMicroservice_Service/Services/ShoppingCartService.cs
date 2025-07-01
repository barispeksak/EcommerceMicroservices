using AutoMapper;
using ShoppingCartMicroservice_Data.Entities;
using ShoppingCartMicroservice_Service.DTOs;
using ShoppingCartMicroservice_Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShoppingCartMicroservice_Service.Services
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IShoppingCartRepository _repository;
        private readonly IMapper _mapper;
        private readonly ProductItemApiClient _productClient;

        public ShoppingCartService(
            IShoppingCartRepository repository,
            IMapper mapper,
            ProductItemApiClient productClient)
        {
            _repository = repository;
            _mapper = mapper;
            _productClient = productClient;
        }

        public async Task<ShoppingCartSummaryDto> GetAllItemsAsync(int cartId)
        {
            var items = await _repository.GetByCartIdAsync(cartId);
            var filteredItems = items.Where(x => !x.IsTotalRow).ToList();
            var totalRow = items.FirstOrDefault(x => x.IsTotalRow);

            var dtoItems = _mapper.Map<List<ShoppingCartDto>>(filteredItems);

            return new ShoppingCartSummaryDto
            {
                Items = dtoItems,
                TotalPrice = totalRow?.TotalPrice ?? 0m
            };
        }

        public async Task<int> GetItemQuantityAsync(int productItemId, int cartId)
        {
            var items = await _repository.GetByCartIdAsync(cartId);
            return items.Where(x => x.ProductItemId == productItemId && !x.IsTotalRow)
                        .Sum(x => x.Qty);
        }

        public async Task<ShoppingCartDto> AddItemAsync(CreateShoppingCartDto dto, int cartId)
        {
            if (cartId == 0)
            {
                var totalRow = new ShoppingCart
                {
                    IsTotalRow = true,
                    TotalPrice = 0m
                };
                await _repository.AddAsync(totalRow);
                cartId         = totalRow.Id;         // ① gerçek sepete id’si
                totalRow.CartId = cartId;             // ② FK bağla
                await _repository.UpdateAsync(totalRow);
            }

            decimal unitPrice = await GetUnitPriceFromProductService(dto.ProductItemId);

            var newItem = _mapper.Map<ShoppingCart>(dto);
            newItem.UnitPrice = unitPrice;
            newItem.LinePrice = unitPrice * dto.Qty;
            newItem.CartId   = cartId;  
            newItem.IsTotalRow = false;
            // newItem.CartId = cartId; // varsa FK

            await _repository.AddAsync(newItem);

            await UpdateTotalPrice(cartId);

            return _mapper.Map<ShoppingCartDto>(newItem);
        }

        public async Task UpdateCartAsync(UpdateShoppingCartDto dto, int cartId)
        {
            foreach (var itemDto in dto.Items)
            {
                var existingItem = await _repository.GetByIdAsync(itemDto.Id);
                if (existingItem == null) continue;

                decimal unitPrice = await GetUnitPriceFromProductService(existingItem.ProductItemId);
                existingItem.Qty = itemDto.Qty;
                existingItem.UnitPrice = unitPrice;
                existingItem.LinePrice = unitPrice * itemDto.Qty;

                await _repository.UpdateAsync(existingItem);
            }
            await UpdateTotalPrice(cartId);
        }

        public async Task DeleteItemAsync(int id)
        {
            await _repository.DeleteAsync(id);
        }

        private async Task UpdateTotalPrice(int cartId)
        {
            var items = await _repository.GetByCartIdAsync(cartId);
            decimal total = items.Where(x => !x.IsTotalRow).Sum(x => x.LinePrice);

            var totalRow = items.FirstOrDefault(x => x.IsTotalRow);
            if (totalRow == null)
            {
                totalRow = new ShoppingCart
                {
                    CartId    = cartId,
                    IsTotalRow = true,
                    TotalPrice = total
                };
                await _repository.AddAsync(totalRow);
            }
            else
            {
                totalRow.TotalPrice = total;
                await _repository.UpdateAsync(totalRow);
            }
        }

        private async Task<decimal> GetUnitPriceFromProductService(int productItemId)
        {
            var (ok, price) = await _productClient.TryGetAsync(productItemId);
            if (!ok)
            {
                throw new Exception($"Ürün {productItemId} bulunamadı veya fiyatı çekilemedi.");
            }
            return price;
        }
    }
}
