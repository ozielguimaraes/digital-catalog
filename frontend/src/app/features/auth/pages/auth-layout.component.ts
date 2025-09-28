import { Component } from '@angular/core';

@Component({
  selector: 'app-auth-layout',
  template: `
    <div class="auth-layout">
      <div class="auth-header">
        <h1 class="app-title">
          <mat-icon class="app-icon">inventory</mat-icon>
          Digital Catalog
        </h1>
        <p class="app-subtitle">Sistema de gerenciamento de catálogo digital</p>
      </div>
      <div class="auth-content">
        <router-outlet></router-outlet>
      </div>
    </div>
  `,
  styleUrls: ['./auth-layout.component.scss']
})
export class AuthLayoutComponent {
  constructor() {}
}
