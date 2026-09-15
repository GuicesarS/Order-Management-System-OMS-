import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateProductDto, ProductResponse, UpdateProductDto } from '../models/product.model';

@Injectable({ providedIn: 'root' })
export class ProductApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/Product`;

  getAll(): Observable<ProductResponse[]> {
    return this.http.get<ProductResponse[]>(this.baseUrl);
  }

  getById(id: string): Observable<ProductResponse> {
    return this.http.get<ProductResponse>(`${this.baseUrl}/${id}`);
  }

  create(dto: CreateProductDto): Observable<ProductResponse> {
    return this.http.post<ProductResponse>(this.baseUrl, dto);
  }

  update(id: string, dto: UpdateProductDto): Observable<ProductResponse> {
    return this.http.put<ProductResponse>(`${this.baseUrl}/${id}`, dto);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
