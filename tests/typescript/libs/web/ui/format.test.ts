import { Format } from '@rasm/ui';
import { ByteSize, DateTime, Duration } from 'effect';
import { expect, it } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const english = new Format({ locales: 'en', byteDigits: 3 });
const dates = new Intl.DateTimeFormat('en');

// --- [COMPOSITION] ---------------------------------------------------------------------

it('selects cardinal and ordinal forms by the plural category of the count', () => {
    const files = { one: (count: string) => `${count} file`, other: (count: string) => `${count} files` };
    const places = { one: (count: string) => `${count}st`, two: (count: string) => `${count}nd`, few: (count: string) => `${count}rd`, other: (count: string) => `${count}th` };
    const counts = [
        { count: 1, forms: files, type: 'cardinal' },
        { count: 1200, forms: files, type: 'cardinal' },
        { count: 2, forms: places, type: 'ordinal' },
        { count: 11, forms: places, type: 'ordinal' },
        { count: 23, forms: places, type: 'ordinal' },
    ] as const;
    expect(counts.map(({ count, forms, type }) => english.quantity(count, forms, type))).toEqual(['1 file', '1,200 files', '2nd', '11th', '23rd']);
});
it('pads numerals to the digit count of their total in locale digits', () => {
    const numerals = [
        { format: english, value: 7, total: 12 },
        { format: english, value: 7, total: 9 },
        { format: english, value: 12, total: undefined },
        { format: new Format({ locales: 'ar-EG' }), value: 7, total: 12 },
    ] as const;
    expect(numerals.map(({ format, value, total }) => format.numeral(value, total))).toEqual(['07', '7', '12', '٠٧']);
});
it('formats sizes in the largest unit their rounded value reaches', () => {
    expect(['512 B', '1500 B', '999999 B', '100 MB'].map((size) => english.bytes(ByteSize.fromStringUnsafe(size)))).toEqual(['512B', '1.5kB', '1MB', '100MB']);
});
it('joins lists by list type', () => {
    expect([english.list(['plan', 'section', 'detail']), english.list(['plan', 'section'], 'disjunction')]).toEqual(['plan, section, and detail', 'plan or section']);
});
it('formats instants, offsets, and durations through the locale formatters', () => {
    const instant = DateTime.makeUnsafe('2026-01-02T12:00:00Z');
    expect([english.date(instant), english.relative(Duration.fromInputUnsafe('3 hours')), english.duration(Duration.fromInputUnsafe('90 minutes'))]).toEqual([dates.format(DateTime.toDateUtc(instant)), 'in 3 hours', '1 hr, 30 min']);
});
