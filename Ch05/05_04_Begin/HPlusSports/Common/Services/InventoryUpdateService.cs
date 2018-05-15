using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HPlusSports.Models;

namespace HPlusSports.Services
{
    public class InventoryUpdateService
    {
        private readonly HPlusSportsDbContext _context;

        public InventoryUpdateService(HPlusSportsDbContext context)
        {
            _context = context;
        }

        public void Update(UpdateProductRequest request)
        {
            var existing = _context.Products.Find(request.Id);

            if (existing == null)
            {
                throw new EntityNotFoundException<Product>(request.Id);
            }

            var hasPriceChanged = existing.Price != request.Price;

            existing.CategoryId = request.CategoryId;
            existing.Description = request.Description;
            existing.MSRP = request.MSRP;
            existing.Name = request.Name;
            existing.Price = request.Price;
            existing.SKU = request.SKU;
            existing.Summary = request.Summary;
            existing.LastUpdated = DateTime.UtcNow;
            existing.LastUpdatedUserId = request.LastUpdatedUserId;

            _context.SaveChanges();

            if (hasPriceChanged)
            {
                var cartsToUpdate =
                  _context.ShoppingCarts
                    .Include("Items")
                    .Where(cart => cart.Items.Any(x => x.SKU == request.SKU));

                foreach (var cart in cartsToUpdate)
                {
                    foreach (var cartItem in cart.Items.Where(x => x.SKU == request.SKU))
                    {
                        cartItem.Price = request.Price;
                    }

                    cart.Recalculate();
                }
            }

            _context.SaveChanges();
        }
    }
}
