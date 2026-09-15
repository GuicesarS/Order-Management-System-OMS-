import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { ProductApiService } from '../../../core/api/product-api.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSlideToggleModule,
    RouterLink
  ],
  templateUrl: './product-form.component.html',
  styleUrl: './product-form.component.scss'
})
export class ProductFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly productApi = inject(ProductApiService);
  private readonly notification = inject(NotificationService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly productId = this.route.snapshot.paramMap.get('id');
  readonly isEdit = !!this.productId;
  readonly saving = signal(false);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required]],
    sku: ['', [Validators.required]],
    price: [0, [Validators.required, Validators.min(0.01)]],
    stockQuantity: [0, [Validators.required, Validators.min(0)]],
    isActive: [true]
  });

  constructor() {
    if (this.productId) {
      this.productApi.getById(this.productId).subscribe((product) => {
        this.form.patchValue(product);
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
      ? this.productApi.update(this.productId!, value)
      : this.productApi.create(value);

    request$.subscribe({
      next: () => {
        this.saving.set(false);
        this.notification.success(this.isEdit ? 'Produto atualizado com sucesso.' : 'Produto criado com sucesso.');
        this.router.navigate(['/products']);
      },
      error: () => this.saving.set(false)
    });
  }
}
