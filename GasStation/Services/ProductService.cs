using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GasStation.Data; 
using GasStation.DTO;
using GasStation.Models;


namespace GasStation.Services
{
	public class ProductService
	{
		private readonly GasStationDbContext _context;
        public ProductService(GasStationDbContext context)
        {
            _context = context;
        }

        // Get all products
        public List<ProductDTO> GetAllProducts()
        {
            var products = _context.Products.ToList();
            return products.Select(p=> new ProductDTO
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Price = p.Price,
            }).ToList();
        }

        public ProductDTO AddProduct(ProductDTO productDTO)
        {

            var product = new Product
            {
                Name = productDTO.Name,
                Price = productDTO.Price,
            };
            _context.Products.Add(product);
            _context.SaveChanges();
            productDTO.ProductId = product.ProductId; // Set the ID of the newly created product
            return productDTO;
        }

        public ProductDTO UpdateProductPrice(ProductDTO productDTO, decimal newPrice)
        {
            var product = _context.Products.Find(productDTO.ProductId);
            if (product == null)
            {
                return null; // Product not found
            }
            if(newPrice <= 0)
            {
                throw new ArgumentException("Price must be positive.");
            }
            product.Price = newPrice;
            _context.SaveChanges();
            return productDTO;
        }

        public void RemoveProduct(ProductDTO productDTO)
        {
            var product = _context.Products.Find(productDTO.ProductId);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
        }

        public ProductDTO GetProductByName(string name)
        {
            var product = _context.Products.FirstOrDefault(p => p.Name == name);
            if (product == null)
            {
                return null; // Product not found
            }
            return new ProductDTO
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
            };
        }
    }
}