import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { forkJoin } from 'rxjs';
import { OrderApiService } from '../../../core/api/order-api.service';
import { CustomerApiService } from '../../../core/api/customer-api.service';
import { ProductApiService } from '../../../core/api/product-api.service';
import { OrderResponse, OrderStatus } from '../../../core/models/order.model';
import { AuthService } from '../../../core/auth/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

const NEXT_STATUSES: Record<OrderStatus, { status: OrderStatus; label: string; icon: string }[]> = {
  Pending: [
    { status: 'Paid', label: 'Marcar como Pago', icon: 'payments' },
    { status: 'Cancelled', label: 'Cancelar', icon: 'cancel' }
  ],
  Paid: [
    { status: 'Shipped', label: 'Marcar como Enviado', icon: 'local_shipping' },
    { status: 'Cancelled', label: 'Cancelar', icon: 'cancel' }
  ],
  Shipped: [],
  Cancelled: []
};

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    RouterLink
  ],
  templateUrl: './order-detail.component.html',
  styleUrl: './order-detail.component.scss'
})
export class OrderDetailComponent {
  private readonly orderApi = inject(OrderApiService);
  private readonly customerApi = inject(CustomerApiService);
  private readonly productApi = inject(ProductApiService);
  private readonly notification = inject(NotificationService);
  readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);

  readonly orderId = this.route.snapshot.paramMap.get('id')!;
  readonly order = signal<OrderResponse | null>(null);
  readonly customerName = signal<string>('');
  readonly productNames = signal<Map<string, string>>(new Map());
  readonly loading = signal(true);
  readonly updating = signal(false);

  readonly itemColumns = ['product', 'quantity', 'unitPrice', 'lineTotal'];

  constructor() {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.orderApi.getById(this.orderId).subscribe({
      next: (order) => {
        this.order.set(order);
        forkJoin({
          customer: this.customerApi.getById(order.customerId),
          products: this.productApi.getAll()
        }).subscribe({
          next: ({ customer, products }) => {
            this.customerName.set(customer.name);
            this.productNames.set(new Map(products.map((p) => [p.id, p.name])));
            this.loading.set(false);
          },
          error: () => this.loading.set(false)
        });
      },
      error: () => this.loading.set(false)
    });
  }

  productName(id: string): string {
    return this.productNames().get(id) ?? id.slice(0, 8);
  }

  availableTransitions(status: OrderStatus) {
    return NEXT_STATUSES[status];
  }

  transitionTo(status: OrderStatus): void {
    const current = this.order();
    if (!current) return;

    this.updating.set(true);
    this.orderApi.update(current.id, { customerId: current.customerId, status }).subscribe({
      next: (updated) => {
        this.order.set(updated);
        this.updating.set(false);
        this.notification.success(`Pedido atualizado para "${status}".`);
      },
      error: () => this.updating.set(false)
    });
  }

  statusColor(status: string): 'primary' | 'accent' | 'warn' | undefined {
    switch (status) {
      case 'Paid':
        return 'primary';
      case 'Shipped':
        return 'accent';
      case 'Cancelled':
        return 'warn';
      default:
        return undefined;
    }
  }
}
