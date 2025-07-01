using System.Collections.Generic;

namespace ShoppingCartMicroservice_Service.DTOs
{
    public class UpdateShoppingCartItemDto
    {
        public int Id { get; set; }      // Sepet satırının ID'si
        public int Qty { get; set; }     // Yeni adet
    }

    public class UpdateShoppingCartDto
    {
        public List<UpdateShoppingCartItemDto> Items { get; set; }
    }
}
