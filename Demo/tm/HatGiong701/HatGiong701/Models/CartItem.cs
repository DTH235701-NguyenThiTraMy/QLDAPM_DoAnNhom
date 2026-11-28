namespace HatGiong701.Models
{
    public class CartItem
    {
        public Seed Seed { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => Seed.Price * Quantity;
    }
}