using OrderManagement.Application.Common;
using OrderManagement.Communication.Dtos.Product;
using OrderManagement.Communication.Responses;

namespace OrderManagement.Application.Interfaces;

public interface IProductService
{
    Result<ProductResponse> Create(CreateProductDto createProductDto);
    Result<ProductResponse> Update(UpdateProductDto updateProductDto);
    Result<ProductResponse> GetById(Guid id);
    Result<IEnumerable<ProductResponse>> GetAll();
    Result<bool> Delete(Guid id);
}
