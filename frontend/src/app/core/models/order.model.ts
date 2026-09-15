export type OrderStatus = 'Pending' | 'Paid' | 'Shipped' | 'Cancelled';

export interface OrderItemResponse {
  id: string;
  orderId: string;
  productId: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface OrderResponse {
  id: string;
  customerId: string;
  status: OrderStatus;
  totalAmount: number;
  createdAt: string;
  paidAt: string | null;
  items: OrderItemResponse[];
}

export interface CreateOrderItemDto {
  productId: string;
  quantity: number;
  unitPrice: number;
}

export interface CreateOrderDto {
  customerId: string;
  items: CreateOrderItemDto[];
}

export interface UpdateOrderItemDto {
  productId?: string;
  quantity?: number;
  unitPrice?: number;
}

export interface UpdateOrderDto {
  customerId?: string;
  status?: OrderStatus;
  items?: UpdateOrderItemDto[];
}
