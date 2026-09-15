import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, ViewChild, inject, signal } from '@angular/core';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { RouterLink } from '@angular/router';
import { ProductApiService } from '../../../core/api/product-api.service';
import { ProductResponse } from '../../../core/models/product.model';
import { AuthService } from '../../../core/auth/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatIconModule,
    MatButtonModule,
    MatDialogModule,
    RouterLink
  ],
  templateUrl: './product-list.component.html',
  styleUrl: './product-list.component.scss'
})
export class ProductListComponent implements AfterViewInit {
  private readonly productApi = inject(ProductApiService);
  private readonly dialog = inject(MatDialog);
  private readonly notification = inject(NotificationService);
  readonly authService = inject(AuthService);

  readonly displayedColumns = ['name', 'sku', 'price', 'stockQuantity', 'isActive', 'actions'];
  readonly dataSource = new MatTableDataSource<ProductResponse>([]);
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
    this.productApi.getAll().subscribe({
      next: (products) => {
        this.dataSource.data = products;
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  remove(product: ProductResponse): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Excluir produto', message: `Deseja excluir o produto "${product.name}"?` }
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;
      this.productApi.delete(product.id).subscribe({
        next: () => {
          this.notification.success('Produto excluído com sucesso.');
          this.load();
        }
      });
    });
  }
}
