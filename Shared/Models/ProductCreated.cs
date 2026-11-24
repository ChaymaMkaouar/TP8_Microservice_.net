namespace Shared.Models
{
    // Message échangé via RabbitMQ (ProductsAPI -> OrdersAPI)
    public class ProductCreated
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
