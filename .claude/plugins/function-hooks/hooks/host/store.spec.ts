// --- [IMPORTS] -------------------------------------------------------------------------

import { describe, expect, it } from 'vitest';
import type { Option } from '../composition/option.ts';
import {
    cleanedOf,
    decodeCleaned,
    decodeDispatch,
    decodeEnvironment,
    decodeFinding,
    decodeFindings,
    decodeJson,
    decodeKeyedFindings,
    decodeNotice,
    decodeScan,
    decodeSecrets,
    decodeSession,
    decodeSkill,
    decodeStamp,
    decodeSummary,
    id,
    ids,
    isNumber,
    KIND_NAMES,
    key,
    keys,
    STATUS,
    secretsOf,
    stamp,
    stampOf,
    suffix,
    summaryOf,
} from './store.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Plain<A> = { readonly kind: 'some'; readonly value: A } | { readonly kind: 'none' };

interface DecoderCase {
    readonly name: string;
    readonly decode: (value: unknown) => Option<unknown>;
    readonly valid: Readonly<Record<string, unknown>>;
    readonly wrongType: Readonly<Record<string, unknown>>;
    readonly missing: Readonly<Record<string, unknown>>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _EPOCH = 0;
const _RANDOM = 'random-uuid-tail';
const _CONFIDENCE = 0.5;
const _TS = 1000;
const _NONE: Plain<unknown> = { kind: 'none' };
const _KINDS = ['stale', 'wrong', 'narrowing', 'anchoring', 'scattered', 'restated', 'coined', 'missing', 'unread', 'narrative'];
const _STATUSES = ['open', 'open-question', 'landed', 'closed'];

const _SECRETS = { tokenA: 'value-a' };
const _ENVIRONMENT = { path: '/repo/node_modules/.bin:/usr/bin' };
const _SESSION = { startedAt: _TS, claudeChain: ['/repo'], memoryDir: null, remoteOwner: 'owner', env: _ENVIRONMENT };
const _NOTICE = { session: 's', text: 'a line' };
const _FINDING = {
    session: 's',
    file: 'CLAUDE.md',
    section: '[01]',
    evidence: 'a:1 b:2',
    change: 'drop the line',
    kind: 'stale',
    confidence: _CONFIDENCE,
    status: 'open',
    proof: '',
    ts: _TS,
};
const _DISPATCH = { rows: ['findings/a'], spawnedAt: _TS, kind: 'stale' };
const _SUMMARY = { open: 1, questions: ['- CLAUDE.md [01]: keep the row?'] };
const _PART_DISPATCH = { rows: [], spawnedAt: _TS, part: 'memory' };
const _SKILL = { loadedAt: _TS, session: 's' };
const _SCAN = { ruleId: 'no-return-by-branch', file: 'tools/nx/workspace.ts' };
const _STAMP = { session: 's', at: _TS };

// Each decoder with its valid row, the row with one field of the wrong type, and the row missing one field
const _DECODERS: readonly DecoderCase[] = [
    { name: 'decodeSecrets', decode: decodeSecrets, valid: _SECRETS, wrongType: { tokenA: 1 }, missing: { tokenA: null } },
    { name: 'decodeEnvironment', decode: decodeEnvironment, valid: _ENVIRONMENT, wrongType: { path: 1 }, missing: { path: null } },
    { name: 'decodeSession', decode: decodeSession, valid: _SESSION, wrongType: { ..._SESSION, env: { path: 1 } }, missing: { startedAt: _TS } },
    { name: 'decodeNotice', decode: decodeNotice, valid: _NOTICE, wrongType: { ..._NOTICE, text: 1 }, missing: { session: 's' } },
    { name: 'decodeFinding', decode: decodeFinding, valid: _FINDING, wrongType: { ..._FINDING, ts: 'x' }, missing: { session: 's' } },
    { name: 'decodeDispatch', decode: decodeDispatch, valid: _DISPATCH, wrongType: { ..._DISPATCH, rows: [1] }, missing: { rows: [] } },
    { name: 'decodeSkill', decode: decodeSkill, valid: _SKILL, wrongType: { ..._SKILL, loadedAt: 'x' }, missing: { session: 's' } },
    { name: 'decodeScan', decode: decodeScan, valid: _SCAN, wrongType: { ..._SCAN, file: 1 }, missing: { ruleId: 'no-x' } },
    { name: 'decodeStamp', decode: decodeStamp, valid: _STAMP, wrongType: { ..._STAMP, at: 'x' }, missing: { session: 's' } },
];

// --- [OPERATIONS] ----------------------------------------------------------------------

// Reads an Option as plain data for value comparison in assertions
const _plain = <A>(option: Option<A>): Plain<A> => option.match<Plain<A>>({ some: (value) => ({ kind: 'some', value }), none: () => _NONE });

// --- [TESTS] ---------------------------------------------------------------------------

describe('keys', () => {
    it('joins a namespace and its parts', () => {
        expect(key('findings', 'a', 'b')).toBe('findings/a/b');
    });

    it('filters a key list by the namespace prefix', () => {
        expect(keys('injected')(['injected/s/x', 'loaded/s/x', 'injectedx'])).toStrictEqual(['injected/s/x']);
    });

    it('lists the ids under a namespace and its parts and reads one id from a key', () => {
        expect(ids('injected', 's')(['injected/s/x', 'injected/t/y', 'loaded/s/x'])).toStrictEqual(['x']);
        expect(suffix('findings')('findings/a')).toBe('a');
    });

    it('builds an id from the clock and the random value', () => {
        expect(id(_EPOCH, _RANDOM)).toBe('1970-01-01T00:00:00.000Z-random-u');
    });

    it('builds a stamp from the session and the clock', () => {
        expect(stamp('s', _TS)).toStrictEqual({ session: 's', at: _TS });
    });

    it('reads the clock value back from an id and none from a text with no stamp', () => {
        expect(_plain(stampOf(id(_TS, _RANDOM)))).toStrictEqual({ kind: 'some', value: _TS });
        expect(_plain(stampOf('x'))).toStrictEqual(_NONE);
    });
});

describe('refinements', () => {
    // typeof NaN === 'number' holds, the refinement answers true for it and the decoders accept it
    it('accepts a number and NaN and rejects a string', () => {
        expect(isNumber(_TS)).toBe(true);
        expect(isNumber(Number.NaN)).toBe(true);
        expect(isNumber('1')).toBe(false);
    });
});

describe('tables', () => {
    it('matches the kind and status tables', () => {
        expect([...KIND_NAMES]).toStrictEqual(_KINDS);
        expect([...STATUS]).toStrictEqual(_STATUSES);
    });
});

describe('decoders', () => {
    it.each(_DECODERS)('$name accepts its row', ({ decode, valid }) => {
        expect(_plain(decode(valid))).toStrictEqual({ kind: 'some', value: valid });
    });

    it.each(_DECODERS)('$name rejects null, a string, and an array', ({ decode }) => {
        expect(_plain(decode(null))).toStrictEqual(_NONE);
        expect(_plain(decode('row'))).toStrictEqual(_NONE);
        expect(_plain(decode([]))).toStrictEqual(_NONE);
    });

    it.each(_DECODERS)('$name rejects a wrong field type and a missing field', ({ decode, wrongType, missing }) => {
        expect(_plain(decode(wrongType))).toStrictEqual(_NONE);
        expect(_plain(decode(missing))).toStrictEqual(_NONE);
    });

    it('reads JSON as a value and a parse failure as none, and a malformed record as the empty one', () => {
        expect(_plain(decodeJson('{"a":1}'))).toStrictEqual({ kind: 'some', value: { a: 1 } });
        expect(_plain(decodeJson('{'))).toStrictEqual(_NONE);
        expect(secretsOf(_SECRETS)).toStrictEqual(_SECRETS);
        expect(secretsOf('row')).toStrictEqual({});
        expect(cleanedOf({ memory: null })).toStrictEqual({ memory: null });
        expect(cleanedOf({ stale: null })).toStrictEqual({});
    });

    it('reads a summary row, and a wrong or missing one as zero open rows with no question', () => {
        expect(_plain(decodeSummary(_SUMMARY))).toStrictEqual({ kind: 'some', value: _SUMMARY });
        expect(_plain(decodeSummary({ ..._SUMMARY, questions: [1] }))).toStrictEqual(_NONE);
        expect(summaryOf(_SUMMARY)).toStrictEqual(_SUMMARY);
        expect(summaryOf({ open: '1' })).toStrictEqual({ open: 0, questions: [] });
        expect(summaryOf(undefined)).toStrictEqual({ open: 0, questions: [] });
    });

    it('keeps the rows of a list that decode and pairs each with its key', () => {
        expect(decodeFindings([_FINDING, 'row', { ..._FINDING, ts: 'x' }])).toStrictEqual([_FINDING]);
        expect(
            decodeKeyedFindings([
                { key: 'findings/a', value: _FINDING },
                { key: 'findings/b', value: null },
            ]),
        ).toStrictEqual([{ key: 'findings/a', row: _FINDING }]);
    });

    it('rejects a finding with an unknown kind or status', () => {
        expect(_plain(decodeFinding({ ..._FINDING, kind: 'other' }))).toStrictEqual(_NONE);
        expect(_plain(decodeFinding({ ..._FINDING, status: 'other' }))).toStrictEqual(_NONE);
    });

    it('accepts an empty and a null cleaned stamp and rejects a name outside the parts', () => {
        expect(_plain(decodeCleaned({}))).toStrictEqual({ kind: 'some', value: {} });
        expect(_plain(decodeCleaned({ memory: null }))).toStrictEqual({ kind: 'some', value: { memory: null } });
        expect(_plain(decodeCleaned({ stale: null }))).toStrictEqual(_NONE);
        expect(_plain(decodeCleaned({ other: '2026-01-01' }))).toStrictEqual(_NONE);
    });

    it('accepts a part dispatch with no rows and rejects a dispatch with a kind and a part together, with neither, or with rows under a part', () => {
        expect(_plain(decodeDispatch(_PART_DISPATCH))).toStrictEqual({ kind: 'some', value: _PART_DISPATCH });
        expect(_plain(decodeDispatch({ ..._DISPATCH, part: 'memory' }))).toStrictEqual(_NONE);
        expect(_plain(decodeDispatch({ rows: [], spawnedAt: _TS }))).toStrictEqual(_NONE);
        expect(_plain(decodeDispatch({ ..._DISPATCH, kind: 'other' }))).toStrictEqual(_NONE);
        expect(_plain(decodeDispatch({ ..._PART_DISPATCH, rows: ['findings/a'] }))).toStrictEqual(_NONE);
    });
});
