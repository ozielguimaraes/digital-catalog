import { Routes } from '@angular/router';
import { ProductListComponent } from './components/product-list/product-list.component';
import { AuthGuard } from '../../core/guards/auth.guard';

export const productRoutes: Routes = [
  {
    path: '',
    component: ProductListComponent,
    canActivate: [AuthGuard]
  }
];
