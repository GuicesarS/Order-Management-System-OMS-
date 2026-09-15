import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, ViewChild, inject, signal } from '@angular/core';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatChipsModule } from '@angular/material/chips';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { OrderApiService } from '../../../core/api/order-api.service';
import { CustomerApiService } from '../../../core/api/customer-api.service';
import { OrderResponse } from '../../../core/models/order.model';
import { AuthService } from '../../../core/auth/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatIconModule,
    MatButtonModule,
    MatDialogModule,
    MatChipsModule,
    RouterLink
  ],
  templateUrl: './order-list.component.html',
  styleUrl: './order-list.component.scss'
})
export class OrderListComponent implements AfterViewInit {
  private readonly orderApi = inject(OrderApiService);
  private readonly customerApi = inject(CustomerApiService);
  private readonly dialog = inject(MatDialog);
  private readonly notification = inject(NotificationService);
  readonly authService = inject(AuthService);

  readonly displayedColumns = ['id', 'customer', 'status', 'totalAmount', 'createdAt', 'actions'];
  readonly dataSource = new MatTableDataSource<OrderResponse>([]);
  readonly loading = signal(true);
  readonly customerNames = signal<Map<string, string>>(new Map());

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
    this.load();
  }

  load(): void {
    this.loading.set(true);
    forkJoin({
      orders: this.orderApi.getAll(),
      customers: this.customerApi.getAll()
    }).subscribe({
      next: ({ orders, customers }) => {
        this.customerNames.set(new Map(customers.map((c) => [c.id, c.name])));
        this.dataSource.data = orders;
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  customerName(id: string): string {
    return this.customerNames().get(id) ?? id.slice(0, 8);
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

  remove(order: OrderResponse): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Excluir pedido', message: `Deseja excluir o pedido "${order.id.slice(0, 8)}"?` }
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;
      this.orderApi.delete(order.id).subscribe({
        next: () => {
          this.notification.success('Pedido excluído com sucesso.');
          this.load();
        }
      });
    });
  }
}
