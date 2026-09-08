import { describe, expect, it } from 'vitest';
import { decode, decodeJson, findings, id, ids, isCleaned, isDispatch, isFinding, isNotice, key, keys, stampOf, struct, suffix } from './store.ts';

const _TS = 1000;
const _FINDING = {
    session: 's',
    file: 'CLAUDE.md',
    section: '[01]',
    evidence: 'a:1 b:2',
    change: 'drop the line',
    kind: 'stale',
    status: 'open',
    proof: '',
    ts: _TS,
};

describe('keys', () => {
    it('builds, filters, and reads keys under a namespace and stamps ids from the clock', () => {
        expect(key('findings', 'a', 'b')).toBe('findings/a/b');
        expect(keys('injected')(['injected/s/x', 'loaded/s/x', 'injectedx'])).toStrictEqual(['injected/s/x']);
        expect(ids('injected', 's')(['injected/s/x', 'injected/t/y'])).toStrictEqual(['x']);
        expect(suffix('findings')('findings/a')).toBe('a');
        expect(id(0, 'random-uuid-tail')).toBe('1970-01-01T00:00:00.000Z-random-u');
        expect(stampOf(id(_TS, 'random'))).toBe(_TS);
        expect(stampOf('x')).toBeUndefined();
    });
});

describe('guards', () => {
    it('holds on a record whose every named field passes and refuses a wrong type, a missing field, and a non-record', () => {
        const guard = struct({ text: isNotice });
        expect(guard({ text: { text: 'a' } })).toBe(true);
        expect(guard({ text: { text: 1 } })).toBe(false);
        expect(guard({})).toBe(false);
        expect(guard(null)).toBe(false);
        expect(guard([])).toBe(false);
        expect(isFinding({ ..._FINDING, kind: 'other' })).toBe(false);
    });

    it('reads a dispatch of a kind with rows or of a part with none, and cleaned stamps of known parts alone', () => {
        expect(isDispatch({ spawnedAt: _TS, rows: ['findings/a'], kind: 'stale' })).toBe(true);
        expect(isDispatch({ spawnedAt: _TS, rows: [], part: 'memory' })).toBe(true);
        expect(isDispatch({ spawnedAt: _TS, rows: ['a'], part: 'memory' })).toBe(false);
        expect(isCleaned({ memory: null, rules: '2026-01-01' })).toBe(true);
        expect(isCleaned({ other: null })).toBe(false);
    });

    it('reads JSON as a value and a parse failure as undefined, and decodes through a guard', () => {
        expect(decodeJson('{"a":1}')).toStrictEqual({ a: 1 });
        expect(decodeJson('{')).toBeUndefined();
        expect(decode(isNotice)({ text: 'a' })).toStrictEqual({ text: 'a' });
        expect(decode(isNotice)('row')).toBeUndefined();
        expect(
            findings([
                { key: 'findings/a', value: _FINDING },
                { key: 'findings/b', value: null },
            ]),
        ).toStrictEqual([{ key: 'findings/a', value: _FINDING, row: _FINDING }]);
    });
});
