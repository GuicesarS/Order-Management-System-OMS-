using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrderManagement.Application.Common.CustomMapping;
using OrderManagement.Application.Exceptions;
using OrderManagement.Application.Services;
using OrderManagement.Communication.Responses;
using OrderManagement.Domain.Entities.Product;
using OrderManagement.Domain.Interfaces;
using OrderManagement.Tests.Application.Builders;
using OrderManagement.Tests.Application.ExtensionMethods;
using OrderManagement.Tests.Domain.Builders;

namespace OrderManagement.Tests.Application.Services;

public class ProductServiceTest
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<ICustomMapper> _mapperMock;
    private readonly Mock<ILogger<ProductService>> _loggerMock;

    private readonly ProductService _productService;

    public ProductServiceTest()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _mapperMock = new Mock<ICustomMapper>();
        _loggerMock = new Mock<ILogger<ProductService>>();

        _productService = new ProductService(
            _productRepositoryMock.Object,
            _mapperMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task CreateProduct_ShouldBeSuccessful()
    {
        var productDto = new CreateProductDtoBuilder().Build();

        _productRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(m => m.Map<Product, ProductResponse>(It.IsAny<Product>()))
            .Returns((Product product) => new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Sku = product.Sku,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive
            });

        var result = await _productService.Create(productDto);

        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(productDto.Name);
        result.Data.Sku.Should().Be(productDto.Sku);
        result.Data.Price.Should().Be(productDto.Price);
        result.Data.StockQuantity.Should().Be(productDto.StockQuantity);
        result.Data.IsActive.Should().Be(productDto.IsActive);
        result.Data.Id.Should().NotBe(Guid.Empty);

        _productRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
        _loggerMock.VerifyLog($"Product created successfully with Id: {result.Data.Id}");
    }

    [Fact]
    public async Task UpdateProduct_ShouldBeSuccessful()
    {
        var existingProduct = new ProductBuilder().Build();
        var updateDto = new UpdateProductDtoBuilder().Build();

        _productRepositoryMock
            .Setup(r => r.GetProductById(existingProduct.Id))
            .ReturnsAsync(existingProduct);

        _productRepositoryMock
            .Setup(r => r.UpdateAsync(existingProduct))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(m => m.Map<Product, ProductResponse>(existingProduct))
            .Returns(new ProductResponse
            {
                Id = existingProduct.Id,
                Name = existingProduct.Name,
                Sku = existingProduct.Sku,
                Price = existingProduct.Price,
                StockQuantity = existingProduct.StockQuantity,
                IsActive = existingProduct.IsActive
            });

        var result = await _productService.Update(existingProduct.Id, updateDto);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(existingProduct.Id);

        _productRepositoryMock.Verify(r => r.UpdateAsync(existingProduct), Times.Once);
        _loggerMock.VerifyLog($"Product with ID: {existingProduct.Id} updated successfully");
    }

    [Fact]
    public async Task UpdateProduct_ShouldThrowNotFoundException_WhenProductNotFound()
    {
        var invalidId = Guid.NewGuid();
        var updateDto = new UpdateProductDtoBuilder().Build();

        _productRepositoryMock
            .Setup(r => r.GetProductById(invalidId))
            .ReturnsAsync((Product?)null);

        Func<Task> act = async () => await _productService.Update(invalidId, updateDto);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Product with id: {invalidId} was not found.");
    }

    [Fact]
    public async Task DeleteProduct_ShouldBeSuccessful()
    {
        var existingProduct = new ProductBuilder().Build();

        _productRepositoryMock
            .Setup(r => r.GetProductById(existingProduct.Id))
            .ReturnsAsync(existingProduct);

        _productRepositoryMock
            .Setup(r => r.DeleteAsync(existingProduct.Id))
            .Returns(Task.CompletedTask);

        var result = await _productService.Delete(existingProduct.Id);

        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();

        _productRepositoryMock.Verify(r => r.DeleteAsync(existingProduct.Id), Times.Once);
        _loggerMock.VerifyLog($"Deleting product with ID: {existingProduct.Id}");
        _loggerMock.VerifyLog($"Product with ID: {existingProduct.Id} deleted successfully");
    }

    [Fact]
    public async Task DeleteProduct_ShouldThrowNotFoundException_WhenProductNotFound()
    {
        var invalidId = Guid.NewGuid();

        _productRepositoryMock
            .Setup(r => r.GetProductById(invalidId))
            .ReturnsAsync((Product?)null);

        Func<Task> act = async () => await _productService.Delete(invalidId);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Product with id: {invalidId} was not found.");
    }

    [Fact]
    public async Task GetAllProducts_ShouldBeSuccessful()
    {
        var products = new List<Product>
        {
            new ProductBuilder().Build(),
            new ProductBuilder().Build()
        };

        _productRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(products);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<Product>, IEnumerable<ProductResponse>>(It.IsAny<IEnumerable<Product>>()))
            .Returns((IEnumerable<Product> p) => p.Select(product => new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Sku = product.Sku,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive
            }));

        var result = await _productService.GetAll();

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNullOrEmpty();
        result.Data!.Count().Should().Be(2);

        _loggerMock.VerifyLog($"Retrieved {result.Data!.Count()} products");
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnEmptyList_WhenNoProductsExist()
    {
        _productRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Product>());

        _mapperMock
            .Setup(m => m.Map<IEnumerable<Product>, IEnumerable<ProductResponse>>(It.IsAny<IEnumerable<Product>>()))
            .Returns(Enumerable.Empty<ProductResponse>());

        var result = await _productService.GetAll();

        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetProductById_ShouldBeSuccessful()
    {
        var existingProduct = new ProductBuilder().Build();

        _productRepositoryMock
            .Setup(r => r.GetProductById(existingProduct.Id))
            .ReturnsAsync(existingProduct);

        _mapperMock
            .Setup(m => m.Map<Product, ProductResponse>(It.IsAny<Product>()))
            .Returns((Product product) => new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Sku = product.Sku,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive
            });

        var result = await _productService.GetProductById(existingProduct.Id);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(existingProduct.Id);
        result.Data.Name.Should().Be(existingProduct.Name);

        _loggerMock.VerifyLog($"Product with ID: {existingProduct.Id} retrieved successfully");
    }

    [Fact]
    public async Task GetProductById_ShouldReturnFailure_WhenProductNotFound()
    {
        var invalidId = Guid.NewGuid();

        _productRepositoryMock
            .Setup(r => r.GetProductById(invalidId))
            .ReturnsAsync((Product?)null);

        var result = await _productService.GetProductById(invalidId);

        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Be("Product not found.");
    }
}