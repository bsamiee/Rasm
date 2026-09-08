import { describe, expect, it } from 'vitest';
import { redact, restore } from './secrets.ts';

const _SECRETS = { tokenA: 'value-$a', empty: '' };

describe('secrets', () => {
    it('replaces each value by its id and back, keeps a $ in a value, and skips an empty value', () => {
        expect(redact('use value-$a and empty', _SECRETS)).toBe('use tokenA and empty');
        expect(restore('use tokenA', _SECRETS)).toBe('use value-$a');
    });
});
