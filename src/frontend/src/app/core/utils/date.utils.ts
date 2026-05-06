/**
 * Date utility functions for handling UTC to local time conversion
 * Following best practices for date handling in web applications
 */

export class DateUtils {
  /**
   * Converts a UTC date string to local Date object
   * @param utcDateString - Date string in UTC format (ISO 8601)
   * @returns Local Date object or null if invalid
   */
  static fromUtc(utcDateString: string | null | undefined): Date | null {
    if (!utcDateString) {
      return null;
    }

    try {
      // Parse the UTC date string and create a local Date object
      const date = new Date(utcDateString);
      
      // Check if the date is valid
      if (isNaN(date.getTime())) {
        console.warn('Invalid date string:', utcDateString);
        return null;
      }

      return date;
    } catch (error) {
      console.error('Error parsing UTC date:', error);
      return null;
    }
  }

  /**
   * Converts a local Date object to UTC string for API communication
   * @param localDate - Local Date object
   * @returns UTC string in ISO 8601 format
   */
  static toUtc(localDate: Date): string {
    return localDate.toISOString();
  }

  /**
   * Formats a UTC date string to local time string
   * @param utcDateString - Date string in UTC format
   * @param options - Intl.DateTimeFormatOptions for formatting
   * @returns Formatted local time string
   */
  static formatLocal(utcDateString: string | null | undefined, options?: Intl.DateTimeFormatOptions): string {
    const localDate = this.fromUtc(utcDateString);
    if (!localDate) {
      return '';
    }

    const defaultOptions: Intl.DateTimeFormatOptions = {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      timeZoneName: 'short'
    };

    const formatOptions = { ...defaultOptions, ...options };
    
    return new Intl.DateTimeFormat('pt-BR', formatOptions).format(localDate);
  }

  /**
   * Formats a UTC date string to a relative time string (e.g., "2 hours ago")
   * @param utcDateString - Date string in UTC format
   * @returns Relative time string
   */
  static formatRelative(utcDateString: string | null | undefined): string {
    const localDate = this.fromUtc(utcDateString);
    if (!localDate) {
      return '';
    }

    const now = new Date();
    const diffInSeconds = Math.floor((now.getTime() - localDate.getTime()) / 1000);

    if (diffInSeconds < 60) {
      return 'agora mesmo';
    }

    const diffInMinutes = Math.floor(diffInSeconds / 60);
    if (diffInMinutes < 60) {
      return `há ${diffInMinutes} minuto${diffInMinutes > 1 ? 's' : ''}`;
    }

    const diffInHours = Math.floor(diffInMinutes / 60);
    if (diffInHours < 24) {
      return `há ${diffInHours} hora${diffInHours > 1 ? 's' : ''}`;
    }

    const diffInDays = Math.floor(diffInHours / 24);
    if (diffInDays < 30) {
      return `há ${diffInDays} dia${diffInDays > 1 ? 's' : ''}`;
    }

    const diffInMonths = Math.floor(diffInDays / 30);
    if (diffInMonths < 12) {
      return `há ${diffInMonths} ${diffInMonths > 1 ? 'meses' : 'mês'}`;
    }

    const diffInYears = Math.floor(diffInMonths / 12);
    return `há ${diffInYears} ano${diffInYears > 1 ? 's' : ''}`;
  }

  /**
   * Gets the timezone offset in minutes for the current browser
   * @returns Timezone offset in minutes
   */
  static getTimezoneOffset(): number {
    return new Date().getTimezoneOffset();
  }

  /**
   * Gets the timezone name for the current browser
   * @returns Timezone name (e.g., "America/Sao_Paulo")
   */
  static getTimezoneName(): string {
    return Intl.DateTimeFormat().resolvedOptions().timeZone;
  }

  /**
   * Checks if a date string is in UTC format
   * @param dateString - Date string to check
   * @returns True if the date string is in UTC format
   */
  static isUtcFormat(dateString: string): boolean {
    return dateString.endsWith('Z') || dateString.includes('+00:00');
  }

  /**
   * Formats a date for display in a table or list
   * @param utcDateString - Date string in UTC format
   * @returns Formatted date string for display
   */
  static formatForDisplay(utcDateString: string | null | undefined): string {
    if (!utcDateString) {
      return '-';
    }

    const localDate = this.fromUtc(utcDateString);
    if (!localDate) {
      return '-';
    }

    const now = new Date();
    const diffInDays = Math.floor((now.getTime() - localDate.getTime()) / (1000 * 60 * 60 * 24));

    // If the date is today, show time only
    if (diffInDays === 0) {
      return localDate.toLocaleTimeString('pt-BR', {
        hour: '2-digit',
        minute: '2-digit'
      });
    }

    // If the date is within the last week, show relative time
    if (diffInDays < 7) {
      return this.formatRelative(utcDateString);
    }

    // Otherwise, show the full date
    return this.formatLocal(utcDateString, {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  /**
   * Creates a date picker value from a UTC string
   * @param utcDateString - Date string in UTC format
   * @returns Date string in YYYY-MM-DD format for date inputs
   */
  static toDatePickerValue(utcDateString: string | null | undefined): string {
    const localDate = this.fromUtc(utcDateString);
    if (!localDate) {
      return '';
    }

    return localDate.toISOString().split('T')[0];
  }

  /**
   * Creates a datetime picker value from a UTC string
   * @param utcDateString - Date string in UTC format
   * @returns Date string in YYYY-MM-DDTHH:mm format for datetime inputs
   */
  static toDateTimePickerValue(utcDateString: string | null | undefined): string {
    const localDate = this.fromUtc(utcDateString);
    if (!localDate) {
      return '';
    }

    const year = localDate.getFullYear();
    const month = String(localDate.getMonth() + 1).padStart(2, '0');
    const day = String(localDate.getDate()).padStart(2, '0');
    const hours = String(localDate.getHours()).padStart(2, '0');
    const minutes = String(localDate.getMinutes()).padStart(2, '0');

    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }

  /**
   * Converts a local date picker value to UTC string
   * @param localDateString - Date string in YYYY-MM-DD format
   * @returns UTC string in ISO 8601 format
   */
  static fromDatePickerValue(localDateString: string): string {
    if (!localDateString) {
      return '';
    }

    const localDate = new Date(localDateString + 'T00:00:00');
    return localDate.toISOString();
  }

  /**
   * Converts a local datetime picker value to UTC string
   * @param localDateTimeString - Date string in YYYY-MM-DDTHH:mm format
   * @returns UTC string in ISO 8601 format
   */
  static fromDateTimePickerValue(localDateTimeString: string): string {
    if (!localDateTimeString) {
      return '';
    }

    const localDate = new Date(localDateTimeString);
    return localDate.toISOString();
  }
}
