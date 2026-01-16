using OrderManagement.Application.Common;
using OrderManagement.Application.Common.CustomMapping;
using OrderManagement.Application.Interfaces;
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

    public async Task<Result<ProductResponse>> Create(Product product)
    {
        await _repository.AddAsync(product);

        var response = _mapper.Map<Product,ProductResponse>(product);
        return Result<ProductResponse>.Ok(response);
    }
    public async Task<Result<ProductResponse>> Update(Guid id, Product product)
    {
        var existingProduct = await _repository.GetProductById(id);

        if(existingProduct is null)
            return Result<ProductResponse>.Failure("Product not found.");

        existingProduct.ApplyChanges(
            product.Name, 
            product.Sku, 
            product.Price, 
            product.StockQuantity);

        await _repository.UpdateAsync(existingProduct);

        var response = _mapper.Map<Product,ProductResponse>(existingProduct);

        return Result<ProductResponse>.Ok(response);
    }
    public async Task<Result<bool>> Delete(Guid id)
    {
        var product = await _repository.GetProductById(id);

        if (product is null)
            return Result<bool>.Failure("Product not found.");

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
