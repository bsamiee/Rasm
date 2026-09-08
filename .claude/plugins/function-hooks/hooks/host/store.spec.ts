import { describe, expect, it } from 'vitest';
import { decode, ids, isPrompt, key, keys, struct, suffix } from './store.ts';

describe('keys', () => {
    it('builds, filters, and reads keys under a namespace', () => {
        expect(key('injected', 'a', 'b')).toBe('injected/a/b');
        expect(keys('injected')(['injected/s/x', 'loaded/s/x', 'injectedx'])).toStrictEqual(['injected/s/x']);
        expect(ids('injected', 's')(['injected/s/x', 'injected/t/y'])).toStrictEqual(['x']);
        expect(suffix('loaded')('loaded/a')).toBe('a');
    });
});

describe('guards', () => {
    it('holds on a record whose every named field passes and refuses a wrong type, a missing field, and a non-record', () => {
        const guard = struct({ text: isPrompt });
        expect(guard({ text: { text: 'a' } })).toBe(true);
        expect(guard({ text: { text: 1 } })).toBe(false);
        expect(guard({})).toBe(false);
        expect(guard(null)).toBe(false);
        expect(guard([])).toBe(false);
    });

    it('decodes a value through a guard', () => {
        expect(decode(isPrompt)({ text: 'a' })).toStrictEqual({ text: 'a' });
        expect(decode(isPrompt)('row')).toBeUndefined();
    });
});
