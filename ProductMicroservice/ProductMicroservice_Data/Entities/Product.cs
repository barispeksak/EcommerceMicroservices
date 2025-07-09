namespace ProductMicroservice_Data.Entities;

public class Product
{
    public int    Id          { get; set; }
    public int    CategoryId  { get; set; }   // başka mikroservisin id’si
    public string Name        { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Image       { get; set; } = null!;
    public string Brand       { get; set; } = null!;
}
