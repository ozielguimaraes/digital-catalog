// import { Injectable } from '@angular/core';
// import { BehaviorSubject } from 'rxjs';

// export type Theme = 'light' | 'dark';

// @Injectable({
//   providedIn: 'root'
// })
// export class ThemeService {
//   private readonly THEME_KEY = 'app_theme';
//   private themeSubject = new BehaviorSubject<Theme>('light');
  
//   public theme$ = this.themeSubject.asObservable();

//   constructor() {
//     this.initializeTheme();
//   }

//   /**
//    * Initialize theme from localStorage or system preference
//    */
//   private initializeTheme(): void {
//     const savedTheme = localStorage.getItem(this.THEME_KEY) as Theme;
    
//     if (savedTheme && (savedTheme === 'light' || savedTheme === 'dark')) {
//       this.setTheme(savedTheme);
//     } else {
//       // Check system preference
//       const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
//       this.setTheme(prefersDark ? 'dark' : 'light');
//     }
//   }

//   /**
//    * Get current theme
//    */
//   getCurrentTheme(): Theme {
//     return this.themeSubject.value;
//   }

//   /**
//    * Set theme
//    */
//   setTheme(theme: Theme): void {
//     this.themeSubject.next(theme);
//     localStorage.setItem(this.THEME_KEY, theme);
//     this.applyTheme(theme);
//   }

//   /**
//    * Toggle between light and dark theme
//    */
//   toggleTheme(): void {
//     const newTheme = this.getCurrentTheme() === 'light' ? 'dark' : 'light';
//     this.setTheme(newTheme);
//   }

//   /**
//    * Apply theme to document
//    */
//   private applyTheme(theme: Theme): void {
//     const body = document.body;
    
//     // Remove existing theme classes
//     body.classList.remove('light-theme', 'dark-theme');
    
//     // Add new theme class
//     body.classList.add(`${theme}-theme`);
//   }

//   /**
//    * Check if current theme is dark
//    */
//   isDarkTheme(): boolean {
//     return this.getCurrentTheme() === 'dark';
//   }

//   /**
//    * Check if current theme is light
//    */
//   isLightTheme(): boolean {
//     return this.getCurrentTheme() === 'light';
//   }
// }
