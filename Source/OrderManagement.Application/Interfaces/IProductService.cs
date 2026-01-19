using OrderManagement.Application.Common;
using OrderManagement.Communication.Dtos.Product;
using OrderManagement.Communication.Responses;
using OrderManagement.Domain.Entities.Product;

namespace OrderManagement.Application.Interfaces;

public interface IProductService
{
    Task<Result<ProductResponse>> Create(CreateProductDto productDto);
    Task<Result<ProductResponse>> Update(Guid id, UpdateProductDto updateProductDto);
    Task<Result<ProductResponse>> GetProductById(Guid id);
    Task<Result<IEnumerable<ProductResponse>>> GetAll();
    Task<Result<bool>> Delete(Guid id);
}
