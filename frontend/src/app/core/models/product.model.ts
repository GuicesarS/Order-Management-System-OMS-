export interface ProductResponse {
  id: string;
  name: string;
  sku: string;
  price: number;
  stockQuantity: number;
  isActive: boolean;
  createdAt: string;
}

export interface CreateProductDto {
  name: string;
  sku: string;
  price: number;
  stockQuantity: number;
  isActive: boolean;
}

export interface UpdateProductDto {
  name?: string;
  sku?: string;
  price?: number;
  stockQuantity?: number;
  isActive?: boolean;
}
