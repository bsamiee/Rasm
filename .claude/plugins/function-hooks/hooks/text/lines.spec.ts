// --- [IMPORTS] -------------------------------------------------------------------------

import { describe, expect, it } from 'vitest';
import { first, lines } from './lines.ts';

// --- [TESTS] ---------------------------------------------------------------------------

describe('lines', () => {
    it('keeps the non-empty lines under either line ending', () => {
        expect(lines('a\r\n\nb\n')).toStrictEqual(['a', 'b']);
        expect(lines('')).toStrictEqual([]);
    });

    it('reads the first non-empty line', () => {
        expect(first('\nError: spawn ENOENT\nmore')).toBe('Error: spawn ENOENT');
        expect(first('\n')).toBe('');
    });
});
