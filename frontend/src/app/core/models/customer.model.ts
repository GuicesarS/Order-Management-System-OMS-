export interface CustomerResponse {
  id: string;
  name: string;
  email: string;
  phone: string;
  address: string;
  createdAt: string;
}

export interface CreateCustomerDto {
  name: string;
  email: string;
  phone: string;
  address: string;
}

export interface UpdateCustomerDto {
  name?: string;
  email?: string;
  phone?: string;
  address?: string;
}
