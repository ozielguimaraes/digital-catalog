import { Routes } from '@angular/router';
import { AuthLayoutComponent } from './pages/auth-layout.component';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { GuestGuard } from '../../core/guards/guest.guard';

export const authRoutes: Routes = [
  {
    path: '',
    component: AuthLayoutComponent,
    canActivate: [GuestGuard],
    children: [
      {
        path: '',
        redirectTo: 'login',
        pathMatch: 'full'
      },
      {
        path: 'login',
        component: LoginComponent
      },
      {
        path: 'register',
        component: RegisterComponent
      }
    ]
  }
];
