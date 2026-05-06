import { DateUtils } from './date.utils';

describe('DateUtils', () => {
  describe('fromUtc', () => {
    it('parses valid UTC ISO string into Date', () => {
      const result = DateUtils.fromUtc('2026-04-26T12:00:00Z');
      expect(result).not.toBeNull();
      expect(result instanceof Date).toBeTrue();
      expect(result!.getUTCFullYear()).toBe(2026);
    });

    it('returns null for null/undefined/empty', () => {
      expect(DateUtils.fromUtc(null)).toBeNull();
      expect(DateUtils.fromUtc(undefined)).toBeNull();
      expect(DateUtils.fromUtc('')).toBeNull();
    });

    it('returns null for unparseable string', () => {
      expect(DateUtils.fromUtc('not-a-date')).toBeNull();
    });
  });

  describe('toUtc', () => {
    it('serializes Date to ISO string ending in Z', () => {
      const date = new Date(Date.UTC(2026, 3, 26, 10, 30, 0));
      expect(DateUtils.toUtc(date)).toBe('2026-04-26T10:30:00.000Z');
    });
  });

  describe('isUtcFormat', () => {
    it('returns true for Z-suffixed strings', () => {
      expect(DateUtils.isUtcFormat('2026-04-26T12:00:00Z')).toBeTrue();
    });

    it('returns true for +00:00 offset strings', () => {
      expect(DateUtils.isUtcFormat('2026-04-26T12:00:00+00:00')).toBeTrue();
    });

    it('returns false for naive datetime', () => {
      expect(DateUtils.isUtcFormat('2026-04-26T12:00:00')).toBeFalse();
    });
  });

  describe('formatRelative', () => {
    // Calcula relativo a Date.now() real porque mock de Date global é frágil
    // (V8 lê o relógio interno, ignorando spyOn(Date, 'now')).
    function isoSecondsAgo(seconds: number): string {
      return new Date(Date.now() - seconds * 1000).toISOString();
    }

    it('returns empty string for null input', () => {
      expect(DateUtils.formatRelative(null)).toBe('');
    });

    it('returns "agora mesmo" for less than 60s ago', () => {
      expect(DateUtils.formatRelative(isoSecondsAgo(30))).toBe('agora mesmo');
    });

    it('returns minutes correctly (singular vs plural)', () => {
      expect(DateUtils.formatRelative(isoSecondsAgo(60))).toBe('há 1 minuto');
      expect(DateUtils.formatRelative(isoSecondsAgo(60 * 5))).toBe('há 5 minutos');
    });

    it('returns hours and days', () => {
      expect(DateUtils.formatRelative(isoSecondsAgo(60 * 60 * 2))).toBe('há 2 horas');
      expect(DateUtils.formatRelative(isoSecondsAgo(60 * 60 * 24 * 3))).toBe('há 3 dias');
    });

    it('returns months and years (plural)', () => {
      expect(DateUtils.formatRelative(isoSecondsAgo(60 * 60 * 24 * 120))).toBe('há 4 meses');
      expect(DateUtils.formatRelative(isoSecondsAgo(60 * 60 * 24 * 365 * 2))).toBe('há 2 anos');
    });
  });

  describe('toDatePickerValue', () => {
    it('returns YYYY-MM-DD format', () => {
      expect(DateUtils.toDatePickerValue('2026-04-26T12:00:00Z')).toBe('2026-04-26');
    });

    it('returns empty for null', () => {
      expect(DateUtils.toDatePickerValue(null)).toBe('');
    });
  });

  describe('formatForDisplay', () => {
    it('returns "-" for null/undefined', () => {
      expect(DateUtils.formatForDisplay(null)).toBe('-');
      expect(DateUtils.formatForDisplay(undefined)).toBe('-');
    });

    it('returns "-" for invalid date', () => {
      expect(DateUtils.formatForDisplay('garbage')).toBe('-');
    });
  });
});
