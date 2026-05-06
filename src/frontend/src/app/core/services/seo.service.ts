import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Meta, Title } from '@angular/platform-browser';

@Injectable({
  providedIn: 'root'
})
export class SeoService {
  constructor(
    @Inject(PLATFORM_ID) private platformId: Object,
    private meta: Meta,
    private title: Title
  ) {}

  updateTitle(title: string) {
    this.title.setTitle(`${title} | Sany & Z - Catálogo Digital`);
  }

  updateMetaTags(config: {
    title?: string;
    description?: string;
    image?: string;
    url?: string;
    type?: string;
    keywords?: string;
  }) {
    if (isPlatformBrowser(this.platformId)) {
      // Update title
      if (config.title) {
        this.updateTitle(config.title);
      }

      // Update meta description
      if (config.description) {
        this.meta.updateTag({ name: 'description', content: config.description });
        this.meta.updateTag({ property: 'og:description', content: config.description });
        this.meta.updateTag({ property: 'twitter:description', content: config.description });
      }

      // Update meta keywords
      if (config.keywords) {
        this.meta.updateTag({ name: 'keywords', content: config.keywords });
      }

      // Update Open Graph tags
      if (config.image) {
        this.meta.updateTag({ property: 'og:image', content: config.image });
        this.meta.updateTag({ property: 'twitter:image', content: config.image });
      }

      if (config.url) {
        this.meta.updateTag({ property: 'og:url', content: config.url });
        this.meta.updateTag({ property: 'twitter:url', content: config.url });
      }

      if (config.type) {
        this.meta.updateTag({ property: 'og:type', content: config.type });
      }
    }
  }

  // Google Analytics tracking
  trackPageView(url: string) {
    if (isPlatformBrowser(this.platformId)) {
      // Google Analytics 4
      if (typeof gtag !== 'undefined') {
        gtag('config', 'GA_MEASUREMENT_ID', {
          page_path: url,
        });
      }
    }
  }

  // Track custom events
  trackEvent(eventName: string, parameters?: any) {
    if (isPlatformBrowser(this.platformId)) {
      if (typeof gtag !== 'undefined') {
        gtag('event', eventName, parameters);
      }
    }
  }

  // Track product views
  trackProductView(productId: string, productName: string, category?: string) {
    this.trackEvent('view_item', {
      item_id: productId,
      item_name: productName,
      item_category: category,
      currency: 'BRL',
      value: 0
    });
  }

  // Track search events
  trackSearch(searchTerm: string, resultsCount?: number) {
    this.trackEvent('search', {
      search_term: searchTerm,
      results_count: resultsCount
    });
  }

  // Track add to cart events
  trackAddToCart(productId: string, productName: string, value: number) {
    this.trackEvent('add_to_cart', {
      item_id: productId,
      item_name: productName,
      currency: 'BRL',
      value: value
    });
  }
}
