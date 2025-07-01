namespace ShopOrderMicroservice.Data.Dtos
{   
    public class ShoppingCartDto
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public int ProductItemId { get; set; }
        public int Qty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LinePrice { get; set; }
        public bool IsTotalRow { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
