using OrderManagement.Application.Common;
using OrderManagement.Communication.Responses;
using OrderManagement.Domain.Entities.Product;

namespace OrderManagement.Application.Interfaces;

public interface IProductService
{
    Result<ProductResponse> Create(Product product);
    Result<ProductResponse> Update(Guid id, Product product);
    Result<ProductResponse> GetProductById(Guid id);
    Result<IEnumerable<ProductResponse>> GetAll();
    Result<bool> Delete(Guid id);
}
