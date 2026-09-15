import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent)
  },
  {
    path: '',
    loadComponent: () => import('./layout/shell.component').then((m) => m.ShellComponent),
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'orders', pathMatch: 'full' },
      {
        path: 'orders',
        loadComponent: () =>
          import('./features/orders/order-list/order-list.component').then((m) => m.OrderListComponent)
      },
      {
        path: 'orders/new',
        loadComponent: () =>
          import('./features/orders/order-form/order-form.component').then((m) => m.OrderFormComponent),
        canActivate: [roleGuard],
        data: { role: 'Admin' }
      },
      {
        path: 'orders/:id',
        loadComponent: () =>
          import('./features/orders/order-detail/order-detail.component').then((m) => m.OrderDetailComponent)
      },
      {
        path: 'orders/:id/edit',
        loadComponent: () =>
          import('./features/orders/order-form/order-form.component').then((m) => m.OrderFormComponent),
        canActivate: [roleGuard],
        data: { role: 'Admin' }
      },
      {
        path: 'customers',
        loadComponent: () =>
          import('./features/customers/customer-list/customer-list.component').then(
            (m) => m.CustomerListComponent
          )
      },
      {
        path: 'customers/new',
        loadComponent: () =>
          import('./features/customers/customer-form/customer-form.component').then(
            (m) => m.CustomerFormComponent
          ),
        canActivate: [roleGuard],
        data: { role: 'Admin' }
      },
      {
        path: 'customers/:id/edit',
        loadComponent: () =>
          import('./features/customers/customer-form/customer-form.component').then(
            (m) => m.CustomerFormComponent
          ),
        canActivate: [roleGuard],
        data: { role: 'Admin' }
      },
      {
        path: 'products',
        loadComponent: () =>
          import('./features/products/product-list/product-list.component').then(
            (m) => m.ProductListComponent
          )
      },
      {
        path: 'products/new',
        loadComponent: () =>
          import('./features/products/product-form/product-form.component').then(
            (m) => m.ProductFormComponent
          ),
        canActivate: [roleGuard],
        data: { role: 'Admin' }
      },
      {
        path: 'products/:id/edit',
        loadComponent: () =>
          import('./features/products/product-form/product-form.component').then(
            (m) => m.ProductFormComponent
          ),
        canActivate: [roleGuard],
        data: { role: 'Admin' }
      },
      {
        path: 'users',
        loadComponent: () =>
          import('./features/users/user-list/user-list.component').then((m) => m.UserListComponent),
        canActivate: [roleGuard],
        data: { role: 'Admin' }
      },
      {
        path: 'users/new',
        loadComponent: () =>
          import('./features/users/user-form/user-form.component').then((m) => m.UserFormComponent),
        canActivate: [roleGuard],
        data: { role: 'Admin' }
      },
      {
        path: 'users/:id/edit',
        loadComponent: () =>
          import('./features/users/user-form/user-form.component').then((m) => m.UserFormComponent),
        canActivate: [roleGuard],
        data: { role: 'Admin' }
      }
    ]
  },
  { path: '**', redirectTo: 'orders' }
];
