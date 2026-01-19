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

    public ProductService(IProductRepository repository, ICustomMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<ProductResponse>> Create(CreateProductDto productDto)
    {
        var product = new Product(
            productDto.Name, 
            productDto.Sku, 
            productDto.Price, 
            productDto.StockQuantity,
            productDto.IsActive);

        await _repository.AddAsync(product);

        var response = _mapper.Map<Product,ProductResponse>(product);
        return Result<ProductResponse>.Ok(response);
    }
    public async Task<Result<ProductResponse>> Update(Guid id, UpdateProductDto productDto)
    {
        var existingProduct = await _repository.GetProductById(id);

        if(existingProduct is null)
            throw new NotFoundException($"Product with id: {id} was not found.");

        var productName = productDto.Name.GetValueForUpdate();
        var productSku = productDto.Sku.GetValueForUpdate();

        existingProduct.ApplyChanges(
            productName,
            productSku,
            productDto.Price,
            productDto.StockQuantity);

        await _repository.UpdateAsync(existingProduct);

        var response = _mapper.Map<Product,ProductResponse>(existingProduct);

        return Result<ProductResponse>.Ok(response);
    }
    public async Task<Result<bool>> Delete(Guid id)
    {
        var product = await _repository.GetProductById(id);

        if (product is null)
            throw new NotFoundException($"Product with id: {id} was not found.");

        await  _repository.DeleteAsync(id);

        return Result<bool>.Ok(true);
    }
    public async Task<Result<IEnumerable<ProductResponse>>> GetAll()
    {
       var products = await _repository.GetAllAsync();
       var response = _mapper.Map<IEnumerable<Product>, IEnumerable<ProductResponse>>(products);
       return Result<IEnumerable<ProductResponse>>.Ok(response);
    }
    public async Task<Result<ProductResponse>> GetProductById(Guid id)
    {
       var existingProduct = await _repository.GetProductById(id);

       if (existingProduct is null)
            return Result<ProductResponse>.Failure("Product not found.");

       var response = _mapper.Map<Product,ProductResponse>(existingProduct);
       return Result<ProductResponse>.Ok(response);
    }

}
