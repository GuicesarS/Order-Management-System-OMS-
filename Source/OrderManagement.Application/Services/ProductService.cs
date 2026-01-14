using OrderManagement.Application.Common;
using OrderManagement.Application.Interfaces;
using OrderManagement.Application.Validators.Product;
using OrderManagement.Communication.Dtos.Product;
using OrderManagement.Communication.Responses;
using OrderManagement.Domain.Entities.Product;
using OrderManagement.Domain.Interfaces;

namespace OrderManagement.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }
    public Result<ProductResponse> Create(CreateProductDto createProductDto)
    {
       var validator = new CreateProductDtoValidator();
       var resultValidation = validator.Validate(createProductDto);

        if (!resultValidation.IsValid)
            return Result<ProductResponse>.Failure(resultValidation.Errors.First().ErrorMessage);
        
       var product = new Product(
           createProductDto.Name, 
           createProductDto.Sku, 
           createProductDto.Price, 
           createProductDto.StockQuantity, 
           createProductDto.IsActive);

       _repository.Add(product);

        return Result<ProductResponse>.Ok(MapToProductResponse(product));

    }
    public Result<ProductResponse> Update(Guid id, UpdateProductDto updateProductDto)
    {
        var validator = new UpdateProductDtoValidator();
        var resultValidation = validator.Validate(updateProductDto);

        if (!resultValidation.IsValid)
            return Result<ProductResponse>.Failure(resultValidation.Errors.First().ErrorMessage);

        var product = _repository.GetProductById(id);
        if (product is null)
            return Result<ProductResponse>.Failure("Product not found.");

        product.ApplyChanges(
            updateProductDto.Name, 
            updateProductDto.Sku, 
            updateProductDto.Price, 
            updateProductDto.StockQuantity);

        _repository.Update(product);

        return Result<ProductResponse>.Ok(MapToProductResponse(product));

    }
    public Result<ProductResponse> GetById(Guid id)
    {
        var product = _repository.GetProductById(id);
        if (product is null)
            return Result<ProductResponse>.Failure("Product not found.");

        return Result<ProductResponse>.Ok(MapToProductResponse(product));
    }
    public Result<IEnumerable<ProductResponse>> GetAll()
    {
        var products = _repository.GetAll();
        var productResponses = products.Select(MapToProductResponse);

        return Result<IEnumerable<ProductResponse>>.Ok(productResponses);
    }

    public Result<bool> Delete(Guid id)
    {
        var product = _repository.GetProductById(id);

        if (product is null)
            return Result<bool>.Failure("Product not found.");

        _repository.Delete(id);

        return Result<bool>.Ok(true);
    }

    private static ProductResponse MapToProductResponse(Product product) =>
        new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.Sku,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt
        };
}
