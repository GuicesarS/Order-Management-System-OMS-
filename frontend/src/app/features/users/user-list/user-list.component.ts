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
import { UserApiService } from '../../../core/api/user-api.service';
import { UserResponse } from '../../../core/models/user.model';
import { AuthService } from '../../../core/auth/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-user-list',
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
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.scss'
})
export class UserListComponent implements AfterViewInit {
  private readonly userApi = inject(UserApiService);
  private readonly dialog = inject(MatDialog);
  private readonly notification = inject(NotificationService);
  private readonly authService = inject(AuthService);

  readonly currentUserId = this.authService.getUserId();
  readonly displayedColumns = ['name', 'email', 'role', 'createdAt', 'actions'];
  readonly dataSource = new MatTableDataSource<UserResponse>([]);
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
    this.userApi.getAll().subscribe({
      next: (users) => {
        this.dataSource.data = users;
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  remove(user: UserResponse): void {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Excluir usuário', message: `Deseja excluir o usuário "${user.name}"?` }
    });

    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed) return;
      this.userApi.delete(user.id).subscribe({
        next: () => {
          this.notification.success('Usuário excluído com sucesso.');
          this.load();
        }
      });
    });
  }
}
