import { Pipe, PipeTransform } from '@angular/core';
import { DateUtils } from '../utils/date.utils';

@Pipe({
  name: 'dateFormat',
  standalone: true
})
export class DateFormatPipe implements PipeTransform {
  /**
   * Transforms a UTC date string to a formatted local date string
   * @param value - UTC date string
   * @param format - Format type ('local', 'relative', 'display', 'datePicker', 'dateTimePicker')
   * @param options - Additional formatting options
   * @returns Formatted date string
   */
  transform(
    value: string | null | undefined, 
    format: 'local' | 'relative' | 'display' | 'datePicker' | 'dateTimePicker' = 'local',
    options?: Intl.DateTimeFormatOptions
  ): string {
    if (!value) {
      return '';
    }

    switch (format) {
      case 'local':
        return DateUtils.formatLocal(value, options);
      case 'relative':
        return DateUtils.formatRelative(value);
      case 'display':
        return DateUtils.formatForDisplay(value);
      case 'datePicker':
        return DateUtils.toDatePickerValue(value);
      case 'dateTimePicker':
        return DateUtils.toDateTimePickerValue(value);
      default:
        return DateUtils.formatLocal(value, options);
    }
  }
}

@Pipe({
  name: 'utcToLocal',
  standalone: true
})
export class UtcToLocalPipe implements PipeTransform {
  /**
   * Converts a UTC date string to a local Date object
   * @param value - UTC date string
   * @returns Local Date object or null
   */
  transform(value: string | null | undefined): Date | null {
    return DateUtils.fromUtc(value);
  }
}

@Pipe({
  name: 'timeAgo',
  standalone: true
})
export class TimeAgoPipe implements PipeTransform {
  /**
   * Shows relative time (e.g., "2 hours ago")
   * @param value - UTC date string
   * @returns Relative time string
   */
  transform(value: string | null | undefined): string {
    return DateUtils.formatRelative(value);
  }
}
