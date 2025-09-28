import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { AuthService } from './core/services/auth.service';
import { ThemeService } from './core/services/theme.service';

@Component({
  selector: 'app-root',
  template: `
    <router-outlet></router-outlet>
  `,
  styles: []
})
export class AppComponent implements OnInit {
  title = 'Digital Catalog';

  constructor(
    private router: Router,
    private authService: AuthService,
    private themeService: ThemeService
  ) {}

  ngOnInit(): void {
    // Initialize theme
    this.themeService.theme$.subscribe(theme => {
      document.body.className = `${theme}-theme`;
    });

    // Track route changes
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event) => {
        // You can add analytics or other route tracking here
        console.log('Route changed to:', (event as NavigationEnd).url);
      });
  }
}
