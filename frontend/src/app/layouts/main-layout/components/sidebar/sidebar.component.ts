import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

export interface MenuItem {
  label: string;
  icon: string;
  route: string;
  children?: MenuItem[];
}

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent implements OnInit {
  isCollapsed = false;
  
  menuItems: MenuItem[] = [
    {
      label: 'Dashboard',
      icon: 'dashboard',
      route: '/dashboard'
    },
    {
      label: 'Produtos',
      icon: 'inventory',
      route: '/produtos'
    },
    {
      label: 'Pedidos',
      icon: 'shopping_cart',
      route: '/orders'
    },
    {
      label: 'Clientes',
      icon: 'people',
      route: '/customers'
    },
    {
      label: 'Relatórios',
      icon: 'analytics',
      route: '/reports'
    },
    {
      label: 'Configurações',
      icon: 'settings',
      route: '/settings'
    }
  ];

  activeRoute = '';
  expandedMenus: Set<string> = new Set();

  constructor(private router: Router) {}

  ngOnInit(): void {
    // Track current route
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event) => {
        this.activeRoute = (event as NavigationEnd).url;
      });
  }

  toggleMenu(menuLabel: string): void {
    if (this.expandedMenus.has(menuLabel)) {
      this.expandedMenus.delete(menuLabel);
    } else {
      this.expandedMenus.add(menuLabel);
    }
  }

  isMenuExpanded(menuLabel: string): boolean {
    return this.expandedMenus.has(menuLabel);
  }

  isActiveRoute(route: string): boolean {
    return this.activeRoute.startsWith(route);
  }

  navigateTo(route: string): void {
    this.router.navigate([route]);
  }

  toggleCollapse(): void {
    this.isCollapsed = !this.isCollapsed;
  }
}
