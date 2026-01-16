using OrderManagement.Application.Common;
using OrderManagement.Communication.Responses;
using OrderManagement.Domain.Entities.Product;

namespace OrderManagement.Application.Interfaces;

public interface IProductService
{
    Task<Result<ProductResponse>> Create(Product product);
    Task<Result<ProductResponse>> Update(Guid id, Product product);
    Task<Result<ProductResponse>> GetProductById(Guid id);
    Task<Result<IEnumerable<ProductResponse>>> GetAll();
    Task<Result<bool>> Delete(Guid id);
}
