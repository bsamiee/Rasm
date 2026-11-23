import { Format } from '@rasm/ui';
import { ByteSize } from 'effect';
import { expect, it } from 'vitest';

// --- [CONSTANTS] -----------------------------------------------------------------------

const english = new Format({ locales: 'en', byteDigits: 3 });
const forms = { one: (count: string) => `${count} file`, two: (count: string) => `${count}nd`, few: (count: string) => `${count}rd`, other: (count: string) => `${count} files` };

// --- [COMPOSITION] ---------------------------------------------------------------------

it.for([
    { format: english, size: '0 B', shown: '0B' },
    { format: english, size: '999 B', shown: '999B' },
    { format: english, size: '1000 B', shown: '1kB' },
    { format: english, size: '1024 B', shown: '1.02kB' },
    { format: english, size: '999499 B', shown: '999kB' },
    { format: english, size: '999500 B', shown: '1MB' },
    { format: english, size: '1499 TB', shown: '1.5PB' },
    { format: new Format({ locales: 'en', byteDigits: 2 }), size: '1050 B', shown: '1.1kB' },
])('formats $size as $shown in the largest decimal unit its value rounded to $format.byteDigits digits reaches', ({ format, size, shown }) => {
    expect(format.bytes(ByteSize.fromStringUnsafe(size))).toBe(shown);
});

it.for([
    { count: 1, type: 'cardinal', forms, shown: '1 file' },
    { count: 1200, type: 'cardinal', forms, shown: '1,200 files' },
    { count: 23, type: 'ordinal', forms, shown: '23rd' },
    { count: 1, type: 'cardinal', forms: { other: (count: string) => `${count} selected` }, shown: '1 selected' },
] as const)('writes $count by the $type plural category of its grouped numeral as $shown, falling back to the other form', ({ count, type, forms: written, shown }) => {
    expect(english.quantity(count, written, type)).toBe(shown);
});

it.for([
    { format: english, value: 7, total: 12, shown: '07' },
    { format: english, value: 7, total: 9, shown: '7' },
    { format: english, value: 1200, total: 1500, shown: '1200' },
    { format: new Format({ locales: 'ar-EG' }), value: 7, total: 12, shown: '٠٧' },
])('pads $value to the digit count of $total as $shown in locale digits', ({ format, value, total, shown }) => {
    expect(format.numeral(value, total)).toBe(shown);
});
