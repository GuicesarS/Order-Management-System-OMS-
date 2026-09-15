import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { forkJoin } from 'rxjs';
import { OrderApiService } from '../../../core/api/order-api.service';
import { CustomerApiService } from '../../../core/api/customer-api.service';
import { ProductApiService } from '../../../core/api/product-api.service';
import { NotificationService } from '../../../core/services/notification.service';
import { CustomerResponse } from '../../../core/models/customer.model';
import { ProductResponse } from '../../../core/models/product.model';

@Component({
  selector: 'app-order-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    RouterLink
  ],
  templateUrl: './order-form.component.html',
  styleUrl: './order-form.component.scss'
})
export class OrderFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly orderApi = inject(OrderApiService);
  private readonly customerApi = inject(CustomerApiService);
  private readonly productApi = inject(ProductApiService);
  private readonly notification = inject(NotificationService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly orderId = this.route.snapshot.paramMap.get('id');
  readonly isEdit = !!this.orderId;
  readonly saving = signal(false);
  readonly loading = signal(true);
  readonly canEditItems = signal(true);

  readonly customers = signal<CustomerResponse[]>([]);
  readonly products = signal<ProductResponse[]>([]);

  readonly form = this.fb.nonNullable.group({
    customerId: ['', [Validators.required]],
    items: this.fb.array([this.createItemGroup()])
  });

  get items(): FormArray {
    return this.form.get('items') as FormArray;
  }

  constructor() {
    forkJoin({
      customers: this.customerApi.getAll(),
      products: this.productApi.getAll()
    }).subscribe(({ customers, products }) => {
      this.customers.set(customers);
      this.products.set(products);

      if (this.orderId) {
        this.orderApi.getById(this.orderId).subscribe((order) => {
          this.canEditItems.set(order.status === 'Pending');
          this.items.clear();
          order.items.forEach((item) => {
            this.items.push(
              this.createItemGroup({
                productId: item.productId,
                quantity: item.quantity,
                unitPrice: item.unitPrice
              })
            );
          });
          this.form.patchValue({ customerId: order.customerId });
          this.loading.set(false);
        });
      } else {
        this.loading.set(false);
      }
    });
  }

  private createItemGroup(initial?: { productId: string; quantity: number; unitPrice: number }) {
    return this.fb.nonNullable.group({
      productId: [initial?.productId ?? '', [Validators.required]],
      quantity: [initial?.quantity ?? 1, [Validators.required, Validators.min(1)]],
      unitPrice: [initial?.unitPrice ?? 0, [Validators.required, Validators.min(0.01)]]
    });
  }

  addItem(): void {
    this.items.push(this.createItemGroup());
  }

  removeItem(index: number): void {
    if (this.items.length <= 1) return;
    this.items.removeAt(index);
  }

  onProductChange(index: number): void {
    const group = this.items.at(index);
    const productId = group.get('productId')?.value;
    const product = this.products().find((p) => p.id === productId);
    if (product) {
      group.get('unitPrice')?.setValue(product.price);
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
      ? this.orderApi.update(this.orderId!, {
          customerId: value.customerId,
          items: value.items
        })
      : this.orderApi.create({
          customerId: value.customerId,
          items: value.items
        });

    request$.subscribe({
      next: () => {
        this.saving.set(false);
        this.notification.success(this.isEdit ? 'Pedido atualizado com sucesso.' : 'Pedido criado com sucesso.');
        this.router.navigate(['/orders']);
      },
      error: () => this.saving.set(false)
    });
  }
}
