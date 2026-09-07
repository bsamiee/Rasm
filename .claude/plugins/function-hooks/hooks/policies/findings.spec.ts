// --- [IMPORTS] -------------------------------------------------------------------------

import { describe, expect, it } from 'vitest';
import type { Option } from '../composition/option.ts';
import { type Cleaned, type Entry, type Finding, id, PART_NAMES } from '../host/store.ts';
import { batch, close, decodeClose, due, expiredFindings, finding, MS_PER_DAY, open, questions, status } from './findings.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Plain<A> = { readonly kind: 'some'; readonly value: A } | { readonly kind: 'none' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const _NONE: Plain<unknown> = { kind: 'none' };
const _NOW = Date.parse('2026-09-06T00:00:00.000Z');
const _RANDOM = 'random-uuid-tail';
const _THRESHOLD = 5;
const _ONE_DAY = 1;
const _OLD_DAYS = 31;
const _CONFIDENCE = 1;
const _FINDING: Finding = {
    session: 's',
    file: 'CLAUDE.md',
    section: '[01]',
    evidence: 'a:1 b:2',
    change: 'drop the line',
    kind: 'stale',
    confidence: _CONFIDENCE,
    status: 'open',
    proof: '',
    ts: _NOW,
};
const _QUESTION: Finding = { ..._FINDING, status: 'open-question', change: 'keep the row?' };

// Stamp the given number of days before now
const _stamp = (days: number): string => new Date(_NOW - days * MS_PER_DAY).toISOString();

// Every part stamped one day ago, nothing due
const _FRESH: Cleaned = Object.fromEntries(PART_NAMES.map((part) => [part, _stamp(_ONE_DAY)]));

// Open rows of one kind under findings/<n>, one over the threshold
const _ENTRIES: readonly Entry[] = Array.from({ length: _THRESHOLD + 1 }, (_row, index): Entry => ({ key: `findings/${index}`, value: _FINDING }));

// --- [OPERATIONS] ----------------------------------------------------------------------

// Reads an Option as plain data for value comparison in assertions
const _plain = <A>(option: Option<A>): Plain<A> => option.match<Plain<A>>({ some: (value) => ({ kind: 'some', value }), none: () => _NONE });

// --- [TESTS] ---------------------------------------------------------------------------

describe('due', () => {
    it('names an absent stamp, a null stamp, an unparsed stamp, and an old stamp, in the parts order', () => {
        expect(due({}, _NOW)).toStrictEqual([...PART_NAMES]);
        expect(due({ ..._FRESH, memory: null }, _NOW)).toStrictEqual(['memory']);
        expect(due({ ..._FRESH, memory: 'yesterday' }, _NOW)).toStrictEqual(['memory']);
        expect(due({ ..._FRESH, prose: _stamp(_OLD_DAYS) }, _NOW)).toStrictEqual(['prose']);
        expect(due({ ..._FRESH, prose: null, memory: null }, _NOW)).toStrictEqual(['memory', 'prose']);
    });

    it('leaves a stamp one day old alone', () => {
        expect(due(_FRESH, _NOW)).toStrictEqual([]);
    });

    it('names the landed and closed rows older than the window, an open row and a young closed row stay', () => {
        const old = _NOW - _OLD_DAYS * MS_PER_DAY;
        expect(
            expiredFindings(
                [
                    { key: 'findings/landed', value: { ..._FINDING, status: 'landed', ts: old } },
                    { key: 'findings/closed', value: { ..._FINDING, status: 'closed', ts: old } },
                    { key: 'findings/open', value: { ..._FINDING, ts: old } },
                    { key: 'findings/question', value: { ..._QUESTION, ts: old } },
                    { key: 'findings/young', value: { ..._FINDING, status: 'closed', ts: _NOW - _ONE_DAY * MS_PER_DAY } },
                    { key: 'findings/malformed', value: 'row' },
                ],
                _NOW,
            ),
        ).toStrictEqual(['findings/landed', 'findings/closed']);
    });
});

describe('open', () => {
    it('counts the open rows and not the open questions', () => {
        expect(open([_FINDING, _QUESTION, { ..._FINDING, status: 'landed' }])).toBe(1);
    });
});

describe('status', () => {
    it('draws nothing on a null surface and the count with the due parts on a terminal', () => {
        expect(_plain(status(null, 1, ['memory']))).toStrictEqual(_NONE);
        expect(_plain(status('terminal', 0, []))).toStrictEqual(_NONE);
        expect(_plain(status('terminal', 2, ['memory', 'prose']))).toStrictEqual({ kind: 'some', value: '2 open findings, due: memory prose' });
        expect(_plain(status('terminal', 2, []))).toStrictEqual({ kind: 'some', value: '2 open findings' });
    });
});

describe('finding', () => {
    it('builds an open row under findings/<id> and an open question from a change ending in a question mark', () => {
        const input = {
            session: 's',
            kind: 'stale' as const,
            fields: { file: 'CLAUDE.md', section: '[01]', evidence: 'a:1 b:2', change: 'drop the line' },
        };
        expect(finding({ ...input, confidence: _CONFIDENCE, now: _NOW, random: _RANDOM })).toStrictEqual({
            key: `findings/${id(_NOW, _RANDOM)}`,
            row: _FINDING,
        });
        expect(
            finding({ ...input, fields: { ...input.fields, change: 'keep the row?' }, confidence: _CONFIDENCE, now: _NOW, random: _RANDOM }).row
                .status,
        ).toBe('open-question');
    });
});

describe('questions', () => {
    it('builds the block from the open-question rows and none without one', () => {
        expect(_plain(questions([_FINDING, _QUESTION]))).toStrictEqual({
            kind: 'some',
            value: { name: 'openQuestions', text: 'Open questions from earlier findings, answer the ones you can:\n- CLAUDE.md [01]: keep the row?' },
        });
        expect(_plain(questions([_FINDING]))).toStrictEqual(_NONE);
    });
});

describe('decodeClose', () => {
    it('reads a null stamp and refuses a stamp that is not a string', () => {
        expect(_plain(decodeClose({ batchId: 'b', cleaned: { memory: null } }))).toStrictEqual({
            kind: 'some',
            value: { batchId: 'b', cleaned: { memory: null } },
        });
        expect(_plain(decodeClose({ batchId: 'b', cleaned: { memory: _NOW } }))).toStrictEqual(_NONE);
    });
});

describe('close', () => {
    it('lists the open rows and the open questions on a bare batchId', () => {
        const rows = [
            { key: 'findings/a', row: _FINDING },
            { key: 'findings/b', row: _QUESTION },
            { key: 'findings/c', row: { ..._FINDING, status: 'landed' as const } },
        ];
        expect(close({ batchId: 'b' }, rows, _FRESH, _NOW)).toStrictEqual({
            writes: [],
            result: {
                rows: [
                    { id: 'a', ..._FINDING },
                    { id: 'b', ..._QUESTION },
                ],
            },
        });
    });

    it('lands or closes each named row with its proof and keeps the stored stamps', () => {
        const rows = [
            { key: 'findings/a', row: _FINDING },
            { key: 'findings/b', row: _FINDING },
        ];
        const updates = [
            { id: 'a', verdict: 'landed', landedFile: 'CLAUDE.md', lines: '12', proof: 'test' },
            { id: 'b', verdict: 'wrong', landedFile: '', lines: '', proof: 'none' },
        ];
        expect(close({ batchId: 'b', rows: updates }, rows, _FRESH, _NOW)).toStrictEqual({
            writes: [
                { key: 'findings/a', value: { ..._FINDING, status: 'landed', proof: 'test' } },
                { key: 'findings/b', value: { ..._FINDING, status: 'closed', proof: 'none' } },
                { key: 'cleaned', value: _FRESH },
            ],
            result: { closed: 2 },
        });
    });

    it('writes the stamps under the cleaned key over the stored ones', () => {
        expect(close({ batchId: 'b', cleaned: { memory: null } }, [], _FRESH, _NOW)).toStrictEqual({
            writes: [{ key: 'cleaned', value: { ..._FRESH, memory: null } }],
            result: { closed: 0 },
        });
    });
});

describe('batch', () => {
    it('makes a kind batch over the open rows at the threshold', () => {
        expect(_plain(batch(_ENTRIES, _FRESH, _NOW, _RANDOM))).toStrictEqual({
            kind: 'some',
            value: {
                batchId: id(_NOW, _RANDOM),
                dispatch: { rows: _ENTRIES.map((entry) => entry.key), spawnedAt: _NOW, kind: 'stale' },
            },
        });
    });

    it('names the most frequent open kind, and the first seen of a tie', () => {
        const rows = (kinds: readonly Finding['kind'][]): readonly Entry[] =>
            kinds.map((kind, index): Entry => ({ key: `findings/${index}`, value: { ..._FINDING, kind } }));
        const keys = _ENTRIES.map((entry) => entry.key);
        expect(_plain(batch(rows(['wrong', 'stale', 'wrong', 'stale', 'stale', 'wrong']), _FRESH, _NOW, _RANDOM))).toStrictEqual({
            kind: 'some',
            value: { batchId: id(_NOW, _RANDOM), dispatch: { rows: keys, spawnedAt: _NOW, kind: 'wrong' } },
        });
        expect(_plain(batch(rows(['wrong', 'stale', 'stale', 'wrong', 'stale', 'coined']), _FRESH, _NOW, _RANDOM))).toStrictEqual({
            kind: 'some',
            value: { batchId: id(_NOW, _RANDOM), dispatch: { rows: keys, spawnedAt: _NOW, kind: 'stale' } },
        });
    });

    it('makes none with no open rows and every part fresh', () => {
        expect(_plain(batch([], _FRESH, _NOW, _RANDOM))).toStrictEqual(_NONE);
    });

    it('makes a part batch for the first due part with no open rows', () => {
        const { memory: _memory, ...stamps } = _FRESH;
        expect(_plain(batch([], stamps, _NOW, _RANDOM))).toStrictEqual({
            kind: 'some',
            value: { batchId: id(_NOW, _RANDOM), dispatch: { rows: [], spawnedAt: _NOW, part: 'memory' } },
        });
    });
});
