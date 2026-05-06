import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { MatMenuTrigger } from '@angular/material/menu';
import { AuthService } from '../../../../core/services/auth.service';
import { ThemeService } from '../../../../core/services/theme.service';
import { User } from '../../../../core/models/user.model';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit {
  @Input() isMobile = false;
  @Output() toggleSidenav = new EventEmitter<void>();

  user: User | null = null;
  isDarkTheme = false;

  constructor(
    private authService: AuthService,
    private themeService: ThemeService
  ) {}

  ngOnInit(): void {
    // Subscribe to auth state
    this.authService.authState$.subscribe(authState => {
      this.user = authState.user;
    });

    // Subscribe to theme state
    this.themeService.theme$.subscribe(theme => {
      this.isDarkTheme = theme === 'dark';
    });
  }

  onToggleSidenav(): void {
    this.toggleSidenav.emit();
  }

  toggleTheme(): void {
    this.themeService.toggleTheme();
  }

  logout(): void {
    this.authService.logout().subscribe({
      next: () => {
        // Navigation will be handled by the auth guard
      },
      error: (error) => {
        console.error('Logout error:', error);
        // Even if logout fails on server, user will be redirected
      }
    });
  }

  openProfile(): void {
    // TODO: Implement profile navigation
    console.log('Open profile');
  }

  openSettings(): void {
    // TODO: Implement settings navigation
    console.log('Open settings');
  }

  openUserMenu(menuTrigger: MatMenuTrigger): void {
    menuTrigger.openMenu();
  }
}
