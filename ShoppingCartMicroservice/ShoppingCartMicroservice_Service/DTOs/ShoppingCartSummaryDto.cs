using System.Collections.Generic;

namespace ShoppingCartMicroservice_Service.DTOs
{
    public class ShoppingCartSummaryDto
    {
        public List<ShoppingCartDto> Items { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
