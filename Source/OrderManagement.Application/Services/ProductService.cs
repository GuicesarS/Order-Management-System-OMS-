using Microsoft.Extensions.Logging;
using OrderManagement.Application.Common;
using OrderManagement.Application.Common.CustomMapping;
using OrderManagement.Application.Common.Extensions;
using OrderManagement.Application.Exceptions;
using OrderManagement.Application.Interfaces;
using OrderManagement.Communication.Dtos.Product;
using OrderManagement.Communication.Responses;
using OrderManagement.Domain.Entities.Product;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly ICustomMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IProductRepository repository, ICustomMapper mapper, ILogger<ProductService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<ProductResponse>> Create(CreateProductDto productDto)
    {
        _logger.LogInformation("Creating a new product with Name: {ProductName} and SKU: {ProductSku}", productDto.Name, productDto.Sku);

        var product = new Product(
            productDto.Name,
            productDto.Sku,
            productDto.Price,
            productDto.StockQuantity,
            productDto.IsActive);

        await _repository.AddAsync(product);

        _logger.LogInformation("Product created successfully with Id: {ProductId}", product.Id);

        var response = _mapper.Map<Product, ProductResponse>(product);
        return Result<ProductResponse>.Ok(response);
    }
    public async Task<Result<ProductResponse>> Update(Guid id, UpdateProductDto updateProductDto)
    {
        _logger.LogInformation("Updating product with ID: {ProductId}", id);

        var existingProduct = await _repository.GetProductById(id);

        if (existingProduct is null)
        {
            _logger.LogWarning("Product with ID: {ProductId} not found", id);
            throw new NotFoundException($"Product with id: {id} was not found.");
        }

        var productName = updateProductDto.Name.GetValueForUpdate();
        var productSku = updateProductDto.Sku.GetValueForUpdate();

        existingProduct.ApplyChanges(
            productName,
            productSku,
            updateProductDto.Price,
            updateProductDto.StockQuantity);

        await _repository.UpdateAsync(existingProduct);

        _logger.LogInformation("Product with ID: {ProductId} updated successfully", id);

        var response = _mapper.Map<Product, ProductResponse>(existingProduct);

        return Result<ProductResponse>.Ok(response);
    }
    public async Task<Result<bool>> Delete(Guid id)
    {
        _logger.LogInformation("Deleting product with ID: {ProductId}", id);

        var product = await _repository.GetProductById(id);

        if (product is null)
        {
            _logger.LogWarning("Product with ID: {ProductId} not found", id);
            throw new NotFoundException($"Product with id: {id} was not found.");
        }

        await _repository.DeleteAsync(id);

        _logger.LogInformation("Product with ID: {ProductId} deleted successfully", id);

        return Result<bool>.Ok(true);
    }
    public async Task<Result<IEnumerable<ProductResponse>>> GetAll()
    {
        _logger.LogInformation("Retrieving all products");

        var products = await _repository.GetAllAsync();
        var response = _mapper.Map<IEnumerable<Product>, IEnumerable<ProductResponse>>(products);

        _logger.LogInformation("Retrieved {ProductCount} products", response.Count());

        return Result<IEnumerable<ProductResponse>>.Ok(response);
    }
    public async Task<Result<ProductResponse>> GetProductById(Guid id)
    {
        _logger.LogInformation("Retrieving product with ID: {ProductId}", id);

        var existingProduct = await _repository.GetProductById(id);

        if (existingProduct is null)
        {
            _logger.LogWarning("Product with ID: {ProductId} not found", id);
            return Result<ProductResponse>.Failure("Product not found.");
        }

        var response = _mapper.Map<Product, ProductResponse>(existingProduct);

        _logger.LogInformation("Product with ID: {ProductId} retrieved successfully", id);

        return Result<ProductResponse>.Ok(response);
    }

}
