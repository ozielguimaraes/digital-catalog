import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

// Angular Material Modules
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

// Components
import { DashboardComponent } from './dashboard.component';

// Routes
import { dashboardRoutes } from './dashboard.routes';

@NgModule({
  declarations: [
    DashboardComponent
  ],
  imports: [
    CommonModule,
    RouterModule.forChild(dashboardRoutes),
    MatCardModule,
    MatIconModule,
    MatButtonModule
  ]
})
export class DashboardModule { }
