import { AfterViewInit, Component, ViewChild, inject, signal } from '@angular/core';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { RouterLink } from '@angular/router';
import { CustomerApiService } from '../../../core/api/customer-api.service';
import { CustomerResponse } from '../../../core/models/customer.model';
import { AuthService } from '../../../core/auth/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-customer-list',
  standalone: true,
  imports: [
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatIconModule,
    MatButtonModule,
    MatDialogModule,
    RouterLink
  ],
  templateUrl: './customer-list.component.html',
  styleUrl: './customer-list.component.scss'
})
export class CustomerListComponent implements AfterViewInit {
  private readonly customerApi = inject(CustomerApiService);
  private readonly dialog = inject(MatDialog);
  private readonly notification = inject(NotificationService);
  readonly authService = inject(AuthService);

  readonly displayedColumns = ['name', 'email', 'phone', 'address', 'actions'];
  readonly dataSource = new MatTableDataSource<CustomerResponse>([]);
  readonly loading = signal(true);

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.customerApi.getAll().subscribe({
      next: (customers) => {
        this.dataSource.data = customers;
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  remove(customer: CustomerResponse): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Excluir cliente', message: `Deseja excluir o cliente "${customer.name}"?` }
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;
      this.customerApi.delete(customer.id).subscribe({
        next: () => {
          this.notification.success('Cliente excluído com sucesso.');
          this.load();
        }
      });
    });
  }
}
