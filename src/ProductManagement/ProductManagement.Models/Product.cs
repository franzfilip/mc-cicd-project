namespace ProductManagement.Models
{
    public class Product
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        public ICollection<StoredProduct> StoredProducts { get; set; } = new List<StoredProduct>();
    }
}