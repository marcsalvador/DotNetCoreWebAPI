using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using ProductsApi.Controllers;
using ProductsApi.Data;
using ProductsApi.Models;

namespace ProductsApi.Tests
{
    public class ProductsControllerTests
    {
        private readonly Mock<ProductsContext> _mockContext;
        private readonly ProductsController _controller;
        private readonly List<Product> _products;

        public ProductsControllerTests()
        {
            _products = new List<Product>
            {
                new Product { Id = 1, Name = "Product 1", Price = 10.0m, StockQuantity = 100 },
                new Product { Id = 2, Name = "Product 2", Price = 20.0m, StockQuantity = 200 }
            };

            var mockSet = new Mock<DbSet<Product>>();
            mockSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(_products.AsQueryable().Provider);
            mockSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(_products.AsQueryable().Expression);
            mockSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(_products.AsQueryable().ElementType);
            mockSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(_products.AsQueryable().GetEnumerator());

            _mockContext = new Mock<ProductsContext>(new DbContextOptions<ProductsContext>());
            _mockContext.Setup(c => c.Products).Returns(mockSet.Object);

            _controller = new ProductsController(_mockContext.Object, null);
        }

        [Fact]
        public async Task GetProducts_ReturnsAllProducts()
        {
            var result = await _controller.GetProducts();

            var actionResult = Assert.IsType<ActionResult<IEnumerable<Product>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var products = Assert.IsType<List<Product>>(okResult.Value);

            Assert.Equal(2, products.Count);
        }

        [Fact]
        public async Task GetProduct_ReturnsProductById()
        {
            var result = await _controller.GetProduct(1);

            var actionResult = Assert.IsType<ActionResult<Product>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var product = Assert.IsType<Product>(okResult.Value);

            Assert.Equal(1, product.Id);
        }

        [Fact]
        public async Task PostProduct_CreatesNewProduct()
        {
            var newProduct = new Product { Id = 3, Name = "Product 3", Price = 30.0m, StockQuantity = 300 };

            var result = await _controller.PostProduct(newProduct);

            var actionResult = Assert.IsType<ActionResult<Product>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var product = Assert.IsType<Product>(createdAtActionResult.Value);

            Assert.Equal(3, product.Id);
            Assert.Equal("Product 3", product.Name);
        }

        [Fact]
        public async Task PutProduct_UpdatesExistingProduct()
        {
            var updatedProduct = new Product { Id = 1, Name = "Updated Product 1", Price = 15.0m, StockQuantity = 150 };

            var result = await _controller.PutProduct(1, updatedProduct);

            Assert.IsType<NoContentResult>(result);

            var product = _products.FirstOrDefault(p => p.Id == 1);
            Assert.Equal("Updated Product 1", product.Name);
        }

        [Fact]
        public async Task DeleteProduct_DeletesProductById()
        {
            var result = await _controller.DeleteProduct(1);

            Assert.IsType<NoContentResult>(result);
            Assert.Null(_products.FirstOrDefault(p => p.Id == 1));
        }
    }
}
