import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';

// Angular Material Modules
import { MatSnackBarModule } from '@angular/material/snack-bar';

// Core Module
import { CoreModule } from './core/core.module';

// Feature Modules
import { AuthModule } from './features/auth/auth.module';
import { DashboardModule } from './features/dashboard/dashboard.module';

// Layout Module
import { MainLayoutModule } from './layouts/main-layout/main-layout.module';

// App Component
import { AppComponent } from './app.component';

// Routes
import { AppRoutingModule } from './app-routing.module';

// Interceptors
import { AuthInterceptor } from './core/interceptors/auth.interceptor';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    HttpClientModule,
    AppRoutingModule,
    CoreModule,
    AuthModule,
    DashboardModule,
    MainLayoutModule,
    MatSnackBarModule
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
