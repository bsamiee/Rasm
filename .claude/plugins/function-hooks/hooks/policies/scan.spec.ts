import type { FsEntry } from 'claude-code';
import { describe, expect, it } from 'vitest';
import { key } from '../host/store.ts';
import {
    expiredScans,
    factsBlock,
    packages,
    type Run,
    SCAN_WINDOW_MS,
    scanHits,
    scanRows,
    stem,
    TREE,
    telemetryBlock,
    utilLanguage,
} from './scan.ts';

const _NOW = 1_700_000_000_000;
const _RECENT_MS = 1000;
const _OK: Run = { exitCode: 0, stdout: '', stderr: '' };

const _hit = (ruleId: string, file: string, replacement?: string): string =>
    JSON.stringify([{ ruleId, file, message: 'm', note: 'n', range: { start: { line: 4 } }, replacement }]);

const _argvs = (path: string): readonly (readonly string[])[] => scanRows(path).map((row) => row.argv(path, ['no-a']));

const _entries = (kind: FsEntry['kind'], ...names: readonly string[]): readonly FsEntry[] => names.map((name) => ({ name, kind, size: 0 }));

// A rule tree of one language directory, one package directory, and two rule files
const _list = (path: string): Promise<readonly FsEntry[]> => {
    if (path === TREE.rules) {
        return Promise.resolve(_entries('dir', 'tsx'));
    }
    return Promise.resolve(path === `${TREE.rules}/tsx` ? _entries('dir', 'syntax') : _entries('file', 'no-a.yml', 'no-b.yml'));
};

describe('scanRows', () => {
    it('selects the family scan, the tree test with the pairing check, the util test, and the affected projects by path', () => {
        expect(_argvs('libs/x.ts')).toStrictEqual([['ast-grep', 'scan', '--json=compact', 'libs/x.ts']]);
        expect(_argvs(`${TREE.rules}/a/b/no-x.yml`).map((argv) => argv[0])).toStrictEqual([
            'ast-grep',
            '.claude/skills/ast-grep/scripts/rule-checks.sh',
        ]);
        expect(_argvs(`${TREE.tests}/a/b/no-x-test.yml`)[0]).toStrictEqual(['ast-grep', 'test', '--include-off', '--filter', '^no-x$']);
        expect(_argvs(`${TREE.utils}/tsx/u.yml`)[0]).toStrictEqual(['ast-grep', 'test', '--include-off', '--filter', '^(no-a)$']);
        expect(_argvs('tools/nx/x.ts').map((argv) => argv[0])).toStrictEqual(['ast-grep', 'pnpm']);
        expect(_argvs('/outside/nx.json').length).toBe(1);
        expect(utilLanguage(`${TREE.utils}/tsx/u.yml`)).toBe('tsx');
        expect(stem(`${TREE.rules}/a/b/no-x-test.yml`)).toBe('no-x-test');
    });

    it('reads hits with their fix line, no hit, and an aborted scan', () => {
        const [row] = scanRows('libs/x.ts');
        expect(row?.lines({ ..._OK, stdout: _hit('no-a', 'libs/x.ts', 'y') }, 'libs/x.ts')[0]).toContain(
            "libs/x.ts:5 no-a: n, fix: nx run rasm:rewrite -- --filter='^no-a$'",
        );
        expect(row?.lines({ ..._OK, exitCode: 1, stdout: '[]' }, 'libs/x.ts')).toStrictEqual(['ast-grep scan: no hit in libs/x.ts']);
        expect(row?.lines({ exitCode: 2, stdout: '', stderr: 'custom language library missing' }, 'libs/x.ts')[0]).toContain(
            'run pnpm exec nx run rasm:grammar',
        );
        expect(scanHits({ ..._OK, stdout: '{' })).toStrictEqual([]);
    });

    it('keeps the verdict lines of a test run and the exit of a silent pairing run', () => {
        const path = `${TREE.rules}/a/b/no-x.yml`;
        const [test, pairing] = scanRows(path);
        expect(test?.lines({ ..._OK, stdout: 'x\nFAIL no-x\ntest result: 1 failed' }, path)).toStrictEqual([
            'ast-grep test no-x: FAIL no-x',
            'ast-grep test no-x: test result: 1 failed',
        ]);
        expect(pairing?.lines({ ..._OK, exitCode: 3 }, '')).toStrictEqual(['rule-checks.sh pairing exited 3']);
    });
});

describe('telemetry', () => {
    it('counts live hits per rule, names silent rules, and expires keys past the window', () => {
        const live = { key: key('scan', `${new Date(_NOW - _RECENT_MS).toISOString()}-a`), value: { ruleId: 'no-a', file: 'x.ts' } };
        const past = { key: key('scan', `${new Date(_NOW - SCAN_WINDOW_MS - 1).toISOString()}-b`), value: { ruleId: 'no-b', file: 'y.ts' } };
        expect(telemetryBlock([live, past], ['no-a', 'no-b'], _NOW)).toBe(
            'Rule hits from edit-time scans in the last 30 days:\n- no-a: 1 hits, last x.ts\nRules with no hit in the window: no-b',
        );
        expect(telemetryBlock([live, past], [], _NOW)).toBeUndefined();
        expect(expiredScans([live.key, past.key, 'scan/x'], _NOW)).toStrictEqual([past.key, 'scan/x']);
    });

    it('lists the packages of a tree and states the facts block', async () => {
        const rules = await packages(TREE.rules, _list);
        expect(rules).toStrictEqual([{ language: 'tsx', package: 'syntax', ids: ['no-a', 'no-b'] }]);
        const block = factsBlock({ version: { ..._OK, stdout: 'ast-grep 0.40.0\n' }, grammar: false, rules, rewrites: [], utils: [] });
        expect(block.split('\n')).toStrictEqual([
            'ast-grep 0.40.0 under sgconfig.yml',
            'Custom grammar .cache/ast-grep/xml.so absent, every scan aborts until pnpm exec nx run rasm:grammar writes it',
            'Rules per family: tsx/syntax 2',
            'Utils per language: ',
            'Rewrites: none',
        ]);
    });
});
