// --- [IMPORTS] -------------------------------------------------------------------------

import { describe, expect, it } from 'vitest';
import type { Option } from '../composition/option.ts';
import { isKind, KIND_NAMES } from '../host/store.ts';
import { CLASSIFIER_PROMPT, decodeFields, fieldsOf, hasEvidence, KINDS, LABELS, NONE_LABEL } from './kinds.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Plain<A> = { readonly kind: 'some'; readonly value: A } | { readonly kind: 'none' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const _NONE: Plain<unknown> = { kind: 'none' };
const _FIELDS = { file: 'CLAUDE.md', section: '[01]', evidence: 'a:1 b:2', change: 'drop the line' };

// Reply text with one field replaced, the JSON the fork answers
const _reply = (patch: Readonly<Record<string, unknown>>): string => JSON.stringify({ ..._FIELDS, ...patch });

// The fields without one of them
const _without = (name: keyof typeof _FIELDS): string => JSON.stringify(Object.fromEntries(Object.entries(_FIELDS).filter(([key]) => key !== name)));

const _COMMAND_LINE = '$ mise which claude\n/Users/x/.local/share/mise/installs/claude/2.1.263/claude';
const _BACKTICK_LINE = '`claude --version`\n2.1.263 (Claude Code)';
const _PAGE_LINE = 'docs/plugins.md The version field is optional per the plugins reference';
const _LOCATIONS = 'CLAUDE.md:12 and README.md:40 both state the routing row';
const _RECOLLECTION = 'the routing row used to say something else';
// The fork's usage as the declarations spell it
const _USAGE = { ['input_tokens']: 1, ['output_tokens']: 1, ['cache_read_input_tokens']: 0, ['cache_creation_input_tokens']: 0 };

// --- [OPERATIONS] ----------------------------------------------------------------------

// Reads an Option as plain data for value comparison in assertions
const _plain = <A>(option: Option<A>): Plain<A> => option.match<Plain<A>>({ some: (value) => ({ kind: 'some', value }), none: () => _NONE });

// --- [TESTS] ---------------------------------------------------------------------------

describe('decodeFields', () => {
    it('decodes one valid text', () => {
        expect(_plain(decodeFields(_reply({})))).toStrictEqual({ kind: 'some', value: _FIELDS });
    });

    it('refuses a missing field', () => {
        expect(_plain(decodeFields(_without('file')))).toStrictEqual(_NONE);
        expect(_plain(decodeFields(_without('evidence')))).toStrictEqual(_NONE);
        expect(_plain(decodeFields(_without('section')))).toStrictEqual(_NONE);
    });

    it('refuses a null file and a null evidence', () => {
        expect(_plain(decodeFields(_reply({ file: null })))).toStrictEqual(_NONE);
        expect(_plain(decodeFields(_reply({ evidence: null })))).toStrictEqual(_NONE);
    });

    it('reads a null section and a null change as the empty string', () => {
        expect(_plain(decodeFields(_reply({ section: null })))).toStrictEqual({ kind: 'some', value: { ..._FIELDS, section: '' } });
        expect(_plain(decodeFields(_reply({ change: null })))).toStrictEqual({ kind: 'some', value: { ..._FIELDS, change: '' } });
    });

    it('refuses a text that is not JSON and a JSON null', () => {
        expect(_plain(decodeFields('not json'))).toStrictEqual(_NONE);
        expect(_plain(decodeFields('null'))).toStrictEqual(_NONE);
    });
});

describe('fieldsOf', () => {
    it('reads none from a null reply', () => {
        expect(_plain(fieldsOf(null))).toStrictEqual(_NONE);
    });

    it('reads none from a reply with text that is not JSON', () => {
        expect(_plain(fieldsOf({ text: 'not json', usage: _USAGE }))).toStrictEqual(_NONE);
    });

    it('reads none from a reply with evidence that is a recollection', () => {
        expect(_plain(fieldsOf({ text: _reply({ evidence: _RECOLLECTION }), usage: _USAGE }))).toStrictEqual(_NONE);
    });

    it('reads the fields from a grounded reply', () => {
        expect(_plain(fieldsOf({ text: _reply({}), usage: _USAGE }))).toStrictEqual({ kind: 'some', value: _FIELDS });
    });
});

describe('hasEvidence', () => {
    it('accepts a command line with its output', () => {
        expect(hasEvidence(_COMMAND_LINE)).toBe(true);
        expect(hasEvidence(_BACKTICK_LINE)).toBe(true);
    });

    it('accepts a page path with its sentence', () => {
        expect(hasEvidence(_PAGE_LINE)).toBe(true);
    });

    it('accepts two file:line locations', () => {
        expect(hasEvidence(_LOCATIONS)).toBe(true);
    });

    it('refuses a recollection', () => {
        expect(hasEvidence(_RECOLLECTION)).toBe(false);
    });
});

describe('CLASSIFIER_PROMPT', () => {
    it("names the kind's criterion and ends with the answer after the --- line", () => {
        const prompt = CLASSIFIER_PROMPT('stale', 'the answer');
        expect(prompt).toContain(`"stale": ${KINDS.stale.criterion}`);
        expect(prompt.split('\n').slice(-2)).toStrictEqual(['---', 'the answer']);
    });
});

describe('labels', () => {
    it('holds every kind and the none label', () => {
        expect([...LABELS]).toStrictEqual([...KIND_NAMES, NONE_LABEL]);
    });

    it('reads the none label as no kind, the arm ends before the fork', () => {
        expect(isKind(NONE_LABEL)).toBe(false);
    });
});
