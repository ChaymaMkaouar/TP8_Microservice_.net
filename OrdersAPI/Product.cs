using System.ComponentModel.DataAnnotations.Schema;

namespace OrdersAPI
{
    // Copie locale (réduite) du produit, alimentée par RabbitMQ
    public class Product
    {
        // L'Id vient de ProductsAPI : il ne doit PAS être généré par la base d'OrdersAPI
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
