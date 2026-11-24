using MassTransit;
using Microsoft.EntityFrameworkCore;
using OrdersAPI.Data;
using Shared.Models;

namespace OrdersAPI.Consumer
{
    public class ProductCreatedConsumer : IConsumer<ProductCreated>
    {
        private readonly OrdersAPIContext _ordersAPIContext;

        public ProductCreatedConsumer(OrdersAPIContext ordersAPIContext)
        {
            _ordersAPIContext = ordersAPIContext;
        }

        // Exécutée à chaque nouveau message reçu dans la file d'attente
        public async Task Consume(ConsumeContext<ProductCreated> context)
        {
            // Idempotence : si le produit existe déjà (message rejoué), on ignore
            var exists = await _ordersAPIContext.Products.AnyAsync(p => p.Id == context.Message.Id);
            if (exists) return;

            var newProduct = new Product
            {
                Id = context.Message.Id,
                Name = context.Message.Name
            };
            _ordersAPIContext.Products.Add(newProduct);
            await _ordersAPIContext.SaveChangesAsync();
        }
    }
}
