import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { UserApiService } from '../../../core/api/user-api.service';
import { NotificationService } from '../../../core/services/notification.service';
import { UserRole } from '../../../core/models/auth.model';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    RouterLink
  ],
  templateUrl: './user-form.component.html',
  styleUrl: './user-form.component.scss'
})
export class UserFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly userApi = inject(UserApiService);
  private readonly notification = inject(NotificationService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly roles: UserRole[] = ['Admin', 'Operator'];
  readonly userId = this.route.snapshot.paramMap.get('id');
  readonly isEdit = !!this.userId;
  readonly saving = signal(false);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', this.isEdit ? [] : [Validators.required, Validators.minLength(6)]],
    role: ['Operator' as UserRole, [Validators.required]]
  });

  constructor() {
    if (this.userId) {
      this.userApi.getById(this.userId).subscribe((user) => {
        this.form.patchValue({ name: user.name, email: user.email, role: user.role });
      });
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    const value = this.form.getRawValue();

    const request$ = this.isEdit
      ? this.userApi.update(this.userId!, {
          name: value.name,
          email: value.email,
          role: value.role,
          ...(value.password ? { password: value.password } : {})
        })
      : this.userApi.create(value);

    request$.subscribe({
      next: () => {
        this.saving.set(false);
        this.notification.success(this.isEdit ? 'Usuário atualizado com sucesso.' : 'Usuário criado com sucesso.');
        this.router.navigate(['/users']);
      },
      error: () => this.saving.set(false)
    });
  }
}
