import { describe, expect, it } from 'vitest';
import {
    ancestors,
    decodeEnvelope,
    diagnosticLines,
    isRoslynRead,
    projectOf,
    recovery,
    settleWait,
    WATCHER_SETTLE_MS,
    wrongLines,
} from './roslyn.ts';

const _NOW = 1000;
const _LEFT = 6;
const _LAST = 9;
const _FIRST = 1;
const _MIDDLE = 3;
const _LINES: readonly [number, number, number] = [_LAST, _FIRST, _MIDDLE];

const _item = (id: string, file: string, line: number): Readonly<Record<string, unknown>> => ({
    id,
    severity: 'Error',
    message: `m${line}`,
    file,
    line,
});

const _envelope = (items: readonly unknown[], unreliable = false): string =>
    JSON.stringify({ items, summary: unreliable ? { unreliable: true } : {} });

describe('diagnosticLines', () => {
    it('lists the edited file items by line less the wrong ids, one line when none, and the degraded line', () => {
        const text = _envelope(
            [
                _item('CS1', '/r/a.cs', _LINES[0]),
                _item('IDE0055', '/r/a.cs', _LINES[1]),
                _item('CS2', '/r/a.cs', _LINES[2]),
                _item('CS3', '/r/b.cs', _LINES[1]),
            ],
            true,
        );
        expect(diagnosticLines({ isError: false, text }, 'a.cs', 'P')).toStrictEqual([
            'a.cs:3 CS2: m3',
            'a.cs:9 CS1: m9',
            'Roslyn: the solution loaded degraded, results can name errors no build reports',
        ]);
        expect(diagnosticLines({ isError: false, text: _envelope([]) }, 'a.cs', 'P')).toStrictEqual(['Roslyn: no error in a.cs (P)']);
        expect(diagnosticLines({ isError: true, text: 'boom\nmore' }, 'a.cs', 'P')).toStrictEqual(['Roslyn get_diagnostics failed: boom']);
        expect(diagnosticLines({ isError: false, text: '{' }, 'a.cs', 'P')[0]).toContain('answered no envelope');
    });

    it('decodes an envelope dropping malformed items and names the wrong ids it holds', () => {
        const envelope = decodeEnvelope(_envelope([_item('IDE0055', '/r/a.cs', 1), { id: 'x' }]));
        expect(envelope?.items.length).toBe(1);
        expect(envelope === undefined ? [] : wrongLines(envelope)[0]).toContain('Ignore the 1 IDE0055 items');
        expect(decodeEnvelope('[]')).toBeUndefined();
    });

    it('asks for trust on the trust code, a rebuild on a degraded load, and nothing otherwise', () => {
        expect(recovery({ isError: true, text: 'SolutionNotTrusted: x' })).toBe('trust_solution');
        expect(recovery({ isError: false, text: _envelope([], true) })).toBe('rebuild_solution');
        expect(recovery({ isError: false, text: _envelope([]) })).toBeUndefined();
    });

    it('reads the compilation tools apart from the control tools and the rest of the watcher window', () => {
        expect(isRoslynRead('mcp__roslyn-codelens__get_diagnostics')).toBe(true);
        expect(isRoslynRead('mcp__roslyn-codelens__trust_solution')).toBe(false);
        expect(isRoslynRead('Bash')).toBe(false);
        expect(settleWait(_NOW, _NOW + WATCHER_SETTLE_MS - _LEFT)).toBe(_LEFT);
        expect(settleWait(undefined, _NOW)).toBe(0);
    });

    it('walks the ancestors to the working directory and reads the nearest project stem', () => {
        expect(ancestors('/r', '/r/a/b/c.cs')).toStrictEqual(['/r/a/b', '/r/a', '/r']);
        expect(ancestors('/r', '/o/c.cs')).toStrictEqual(['/o']);
        expect(
            projectOf([
                { dir: '/r/a/b', names: ['c.cs'] },
                { dir: '/r/a', names: ['P.csproj'] },
            ]),
        ).toBe('P');
        expect(projectOf([{ dir: '/r', names: [] }])).toBeUndefined();
    });
});
