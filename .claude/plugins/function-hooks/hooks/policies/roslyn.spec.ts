// --- [IMPORTS] -------------------------------------------------------------------------

import { describe, expect, it } from 'vitest';
import { none, type Option, some } from '../composition/option.ts';
import {
    ancestors,
    type Diagnostic,
    decodeEnvelope,
    diagnosticLines,
    droppedLines,
    filterEnvelope,
    isRoslynRead,
    projectOf,
    ROSLYN_READS,
    replyClass,
    settleWait,
    WATCHER_SETTLE_MS,
    WRONG_DIAGNOSTICS,
} from './roslyn.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Plain<A> = { readonly kind: 'some'; readonly value: A } | { readonly kind: 'none' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const _NONE: Plain<unknown> = { kind: 'none' };
const _CWD = '/repo';
const _FILE = 'libs/dotnet/interop/Rasm.Interop/RuntimeInitialization.cs';
const _PROJECT = 'Rasm.Interop';
const _SUMMARY = { error: 2, warning: 0, info: 0, hidden: 0 };
// The severity capitalized as the server spells it on an item
const _OWN: Diagnostic = {
    id: 'CS0219',
    severity: 'Error',
    message: "The variable 'unusedProbe' is assigned but its value is never used",
    file: `${_CWD}/${_FILE}`,
    line: 27,
    project: _PROJECT,
    source: 'compiler',
};
const _EARLIER: Diagnostic = { ..._OWN, id: 'IDE0055', message: 'Fix formatting', line: 12, source: 'analyzer:IDE0055' };
const _OTHER: Diagnostic = { ..._OWN, file: `${_CWD}/libs/dotnet/interop/Rasm.Interop.Pdf/PdfInterop.cs`, project: 'Rasm.Interop.Pdf' };
const _ENVELOPE = { items: [_OWN, _OTHER, _EARLIER], totalCount: 3, truncated: false, limit: 1000, summary: _SUMMARY };
const _UNRELIABLE = { ..._ENVELOPE, summary: { ..._SUMMARY, unreliable: { reason: 'degraded' } } };
const _DROPPED_LINE =
    'Dropped 2 IDE0055 items, the server formats under Roslyn defaults until a release attaches project.AnalyzerOptions (README known issues row 02)';
const _NOW = Date.parse('2026-09-06T12:00:00.000Z');
const _INSIDE_MS = 6;
const _PAST_MS = 300;
const _CONTROL = [
    'get_task_status',
    'list_running_tasks',
    'list_solutions',
    'list_trusted_paths',
    'load_solution',
    'rebuild_solution',
    'revoke_trust',
    'set_active_solution',
    'start_background_task',
    'trust_solution',
    'unload_solution',
];
const _READ_COUNT = 56;

// --- [OPERATIONS] ----------------------------------------------------------------------

const _plain = <A>(option: Option<A>): Plain<A> => option.match<Plain<A>>({ some: (value) => ({ kind: 'some', value }), none: () => _NONE });

const _reply = (value: unknown): { readonly isError: false; readonly text: string } => ({ isError: false, text: JSON.stringify(value) });

// --- [TESTS] ---------------------------------------------------------------------------

describe('decodeEnvelope', () => {
    it('reads the items and the summary and drops an item missing a field', () => {
        expect(_plain(decodeEnvelope(JSON.stringify({ ..._ENVELOPE, items: [_OWN, { id: 'x' }] })))).toStrictEqual({
            kind: 'some',
            value: { items: [_OWN], summary: _SUMMARY },
        });
    });

    it('reads a malformed text and an envelope without a summary as none', () => {
        expect(_plain(decodeEnvelope('not json'))).toStrictEqual(_NONE);
        expect(_plain(decodeEnvelope(JSON.stringify({ items: [] })))).toStrictEqual(_NONE);
    });
});

describe('filterEnvelope', () => {
    it('drops the wrong items and reduces totalCount and the severity count, every other field as read', () => {
        expect(_plain(filterEnvelope(JSON.stringify({ ..._ENVELOPE, items: [_EARLIER, _OWN, _EARLIER] })))).toStrictEqual({
            kind: 'some',
            value: {
                text: JSON.stringify({ items: [_OWN], totalCount: 1, truncated: false, limit: 1000, summary: { ..._SUMMARY, error: 0 } }),
                dropped: ['IDE0055', 'IDE0055'],
            },
        });
    });

    it('keeps an item missing a field and leaves the summary alone under a severity it lacks', () => {
        const partial = { id: 'IDE0055' };
        const noted = { ..._EARLIER, severity: 'note' };
        expect(_plain(filterEnvelope(JSON.stringify({ ..._ENVELOPE, items: [partial, noted] })))).toStrictEqual({
            kind: 'some',
            value: { text: JSON.stringify({ ..._ENVELOPE, items: [partial], totalCount: 2 }), dropped: ['IDE0055'] },
        });
    });

    it('answers none when nothing drops, when the text is no envelope, and when totalCount is absent it stays absent', () => {
        expect(_plain(filterEnvelope(JSON.stringify({ ..._ENVELOPE, items: [_OWN, _OTHER] })))).toStrictEqual(_NONE);
        expect(_plain(filterEnvelope('plain'))).toStrictEqual(_NONE);
        expect(_plain(filterEnvelope(JSON.stringify({ items: [_EARLIER], summary: _SUMMARY })))).toStrictEqual({
            kind: 'some',
            value: { text: JSON.stringify({ items: [], summary: { ..._SUMMARY, error: 1 } }), dropped: ['IDE0055'] },
        });
    });

    it('names each dropped id once with its count and nothing for no drop', () => {
        expect(droppedLines(['IDE0055', 'IDE0055'])).toStrictEqual([_DROPPED_LINE]);
        expect(droppedLines([])).toStrictEqual([]);
    });
});

describe('settleWait', () => {
    it('answers the rest of the window since the stamp, the whole window on a fresh stamp, and 0 past it or with none', () => {
        expect(settleWait(some(_NOW - _INSIDE_MS), _NOW)).toBe(WATCHER_SETTLE_MS - _INSIDE_MS);
        expect(settleWait(some(_NOW), _NOW)).toBe(WATCHER_SETTLE_MS);
        expect(settleWait(some(_NOW - _PAST_MS), _NOW)).toBe(0);
        expect(settleWait(none(), _NOW)).toBe(0);
    });
});

describe('ROSLYN_READS', () => {
    it('lists every server tool but the control tools and refines a read event alone', () => {
        expect(ROSLYN_READS).toHaveLength(_READ_COUNT);
        expect(ROSLYN_READS.every((tool) => tool.startsWith('mcp__roslyn-codelens__'))).toBe(true);
        expect(ROSLYN_READS.filter((tool) => _CONTROL.some((name) => tool === `mcp__roslyn-codelens__${name}`))).toStrictEqual([]);
        expect(isRoslynRead({ tool: 'mcp__roslyn-codelens__get_diagnostics', ['tool_use_id']: 'call', project: _PROJECT })).toBe(true);
        expect(isRoslynRead({ tool: 'mcp__roslyn-codelens__trust_solution', ['tool_use_id']: 'call', path: '/x/Other.slnx' })).toBe(false);
        expect(isRoslynRead({ tool: 'Bash', ['tool_use_id']: 'call', command: 'ls' })).toBe(false);
    });
});

describe('projectOf', () => {
    it('answers the stem of the first csproj found walking upward', () => {
        expect(
            _plain(
                projectOf([
                    { dir: `${_CWD}/libs/dotnet/interop/Rasm.Interop/Native`, names: ['a.cs'] },
                    { dir: `${_CWD}/libs/dotnet/interop/Rasm.Interop`, names: ['Rasm.Interop.csproj', 'b.cs'] },
                    { dir: `${_CWD}/libs/dotnet/interop`, names: ['Directory.Build.props'] },
                ]),
            ),
        ).toStrictEqual({ kind: 'some', value: _PROJECT });
    });

    it('answers none without a csproj', () => {
        expect(_plain(projectOf([{ dir: _CWD, names: ['README.md'] }]))).toStrictEqual(_NONE);
    });
});

describe('ancestors', () => {
    it('lists the directories from the file up to the working directory', () => {
        expect(ancestors(_CWD, `${_CWD}/a/b/c/file.cs`)).toStrictEqual([`${_CWD}/a/b/c`, `${_CWD}/a/b`, `${_CWD}/a`, _CWD]);
    });

    it('answers the own directory alone outside the working directory', () => {
        expect(ancestors(_CWD, '/elsewhere/x/file.cs')).toStrictEqual(['/elsewhere/x']);
    });
});

describe('replyClass', () => {
    it('classes the trust error, the degraded summary, and a read', () => {
        expect(replyClass({ isError: true, text: '{"code":"SolutionNotTrusted","message":"call trust_solution"}' })).toBe('untrusted');
        expect(replyClass(_reply(_UNRELIABLE))).toBe('unreliable');
        expect(replyClass(_reply(_ENVELOPE))).toBe('read');
        expect(replyClass({ isError: true, text: '{"code":"Internal","message":"boom"}' })).toBe('read');
    });
});

describe('diagnosticLines', () => {
    it('lists the items under the edited file by line and drops the WRONG_DIAGNOSTICS rows', () => {
        expect(WRONG_DIAGNOSTICS.map((row) => row.id)).toContain(_EARLIER.id);
        expect(diagnosticLines(_reply(_ENVELOPE), _FILE, _PROJECT)).toStrictEqual([
            `${_FILE}:27 CS0219: The variable 'unusedProbe' is assigned but its value is never used`,
        ]);
    });

    it('answers the no-error line when the wrong rows alone sit under the edited file', () => {
        expect(diagnosticLines(_reply({ ..._ENVELOPE, items: [_EARLIER, _OTHER] }), _FILE, _PROJECT)).toStrictEqual([
            `Roslyn: no error in ${_FILE} (${_PROJECT})`,
        ]);
    });

    it('answers one line when the items sit under other files alone', () => {
        expect(diagnosticLines(_reply({ ..._ENVELOPE, items: [_OTHER] }), _FILE, _PROJECT)).toStrictEqual([
            `Roslyn: no error in ${_FILE} (${_PROJECT})`,
        ]);
    });

    it('names the failure of an error reply and an unparsed text', () => {
        expect(diagnosticLines({ isError: true, text: 'Internal: boom\nmore' }, _FILE, _PROJECT)).toStrictEqual([
            'Roslyn get_diagnostics failed: Internal: boom',
        ]);
        expect(diagnosticLines({ isError: false, text: 'plain' }, _FILE, _PROJECT)).toStrictEqual([
            'Roslyn get_diagnostics answered no envelope: plain',
        ]);
    });

    it('appends the degraded line under an unreliable summary', () => {
        expect(diagnosticLines(_reply({ ..._UNRELIABLE, items: [] }), _FILE, _PROJECT)).toStrictEqual([
            `Roslyn: no error in ${_FILE} (${_PROJECT})`,
            'Roslyn: the solution loaded degraded, results can name errors no build reports',
        ]);
    });
});
