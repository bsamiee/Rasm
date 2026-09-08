// --- [IMPORTS] -------------------------------------------------------------------------

import { describe, expect, it } from 'vitest';
import { getOrElse } from '../composition/option.ts';
import type { Entry } from '../host/store.ts';
import {
    abort,
    expiredScans,
    type Facts,
    factsBlock,
    needsRuleIds,
    type RuleFile,
    type Run,
    SCAN,
    type ScanFacts,
    type ScanRow,
    scanHits,
    scanRows,
    stale,
    telemetryBlock,
} from './scan.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _FAMILY = 0;
const _TREE = 1;
const _UTILS = 2;
const _PAIRING = 3;
const _GRAPH = 4;
const _GRAPH_PREFIX = ['pnpm', 'exec', 'nx', 'show', 'projects', '--affected', '--json'];
const _GRAMMAR_EXIT = 79;
const _UNKNOWN_EXIT = 3;
const _FAIL_EXIT = 4;
const _OTHER_EXIT = 2;
const _NOTE = 'Dispatch on the value';
const _FILE = 'tools/nx/workspace.ts';
const _BRANCH_LINE = 1;
const _LOOP_LINE = 7;

// --- [OPERATIONS] ----------------------------------------------------------------------

// One element in the compact shape ast-grep scan --json=compact prints, the fields beside the decoded ones kept as the binary writes them
const _compact = (line: number, ruleId: string, note: string, replacement?: string): Readonly<Record<string, unknown>> => ({
    text: 'x',
    range: { byteOffset: { start: 0, end: 1 }, start: { line, column: 0 }, end: { line, column: 1 } },
    file: _FILE,
    lines: 'x',
    language: 'Tsx',
    ruleId,
    severity: 'error',
    note,
    message: 'The shape',
    replacement,
});

// One hit with a fix and one without
const _HITS = JSON.stringify([
    _compact(_BRANCH_LINE, 'no-return-by-branch', _NOTE),
    _compact(_LOOP_LINE, 'no-fold-by-loop', 'Fold with reduce', 'xs.reduce(f, 0)'),
]);
const _GRAMMAR_STDERR =
    'Error: Cannot load custom language library\nHelp: The custom language library is not found or cannot be loaded.\n\n✖ Caused by\n╰▻ cannot get the absolute path of dynamic lib\n';
const _TEST_STDOUT =
    'Running 1 tests\n\n----------- Case Details -----------\nPASS no-catch-around-next  ....................\n\ntest result: ok. 1 passed; 0 failed;\n';
const _FAIL_STDOUT = 'Running 1 tests\nFAIL no-catch-around-next\n╰▻ Noisy: valid case reported\n\ntest result: FAILED. 0 passed; 1 failed;\n';
const _UNKNOWN_STDERR = 'Error: Rule not found: ^no-x$\nHelp: Rule with id ^no-x$ not found in project configuration.\n';

// The clock of the telemetry cases, the window edges measured from it
const _NOW = Date.parse('2026-09-06T12:00:00.000Z');
const _DAY = 86_400_000;
const _ONE_DAY = 1;
const _OLD_DAYS = 31;
const _YEAR_DAYS = 365;
const _rule = (id: string, ageDays: number): RuleFile => ({ id, language: 'typescript', package: 'syntax', mtimeMs: _NOW - ageDays * _DAY });
const _RULES: readonly RuleFile[] = [_rule('no-fold-by-loop', _YEAR_DAYS), _rule('no-return-by-branch', _YEAR_DAYS), _rule('no-silent', _OLD_DAYS)];
const _scanKey = (ageDays: number, tail: string): string => `scan/${new Date(_NOW - ageDays * _DAY).toISOString()}-${tail}`;
const _entry = (storeKey: string, ruleId: string, file: string): Entry => ({ key: storeKey, value: { ruleId, file } });
const _LIVE_A = _entry(_scanKey(_ONE_DAY, 'aaaaaaaa'), 'no-return-by-branch', 'a.ts');
const _LIVE_B = _entry(_scanKey(_ONE_DAY + _ONE_DAY, 'bbbbbbbb'), 'no-return-by-branch', 'b.ts');
const _LIVE_C = _entry(_scanKey(_ONE_DAY, 'cccccccc'), 'no-fold-by-loop', 'c.ts');
const _UNKNOWN = _entry(_scanKey(_ONE_DAY, 'dddddddd'), 'no-gone', 'd.ts');
const _OLD = _entry(_scanKey(_OLD_DAYS, 'eeeeeeee'), 'no-return-by-branch', 'e.ts');
const _MALFORMED: Entry = { key: 'scan/not-a-stamp', value: { ruleId: 'no-return-by-branch' } };
const _FACTS: Facts = {
    version: { exitCode: 0, stdout: 'ast-grep 0.45.3\n', stderr: '' },
    grammar: true,
    families: [
        { language: 'typescript', package: 'syntax', rules: 8 },
        { language: 'dotnet', package: 'msbuild', rules: 40 },
        { language: 'typescript', package: 'claude-code', rules: 17 },
    ],
    utils: [
        { language: 'typescript', count: 27 },
        { language: 'dotnet', count: 3 },
    ],
    rewrites: [
        { language: 'typescript', package: 'effect', ids: ['option-nullish-ternary-to-from-nullable'] },
        { language: 'dotnet', package: 'msbuild', ids: ['exec-to-task', 'absolute-to-relative-path'] },
    ],
};

// The rule ids the util row filters by language, two of python and one each of typescript and dotnet
const _IDS: ScanFacts = {
    rules: [
        { id: 'no-x', language: 'typescript' },
        { id: 'no-x-property', language: 'dotnet' },
        { id: 'no-repeated-literal-in-function', language: 'python' },
        { id: 'no-fixed-sleep', language: 'python' },
    ],
};
const _NO_IDS: ScanFacts = { rules: [] };
const _PAIRING_SCRIPT = '.claude/skills/ast-grep/scripts/rule-checks.sh';
const _PYTHON_UTIL = 'tools/ast-grep/utils/python/python-parameter.yml';
const _BASH_UTIL = 'tools/ast-grep/utils/bash/bash-parameter.yml';

// The rows under their row type, the literal table types each argv by its own parameter list
const _ROWS: readonly ScanRow[] = SCAN;

const _run = (exitCode: number, stdout: string, stderr = ''): Run => ({ exitCode, stdout, stderr });

const _lines = (index: number, run: Run, path: string): readonly string[] => _ROWS[index]?.lines(run, path, _NO_IDS) ?? [];

// The replacement of a hit as a nullable value for comparison
const _orNull = getOrElse((): string | null => null);

// --- [TESTS] ---------------------------------------------------------------------------

describe('scanRows', () => {
    it('matches the family extensions and names, the rule tree, the utils, and the task graph', () => {
        expect(scanRows('libs/x/a.ts', _NO_IDS)).toStrictEqual([SCAN[_FAMILY]]);
        expect(scanRows('eng/scripts/a.py', _NO_IDS)).toStrictEqual([SCAN[_FAMILY]]);
        for (const path of ['script.sh', 'script.bash', 'view.tsx', '.github/workflows/test.yml', 'config.yaml']) {
            expect(scanRows(path, _NO_IDS)).toStrictEqual([SCAN[_FAMILY]]);
        }
        expect(scanRows('libs/dotnet/a/A.csproj', _NO_IDS)).toStrictEqual([SCAN[_FAMILY]]);
        expect(scanRows('NuGet.config', _NO_IDS)).toStrictEqual([SCAN[_FAMILY]]);
        expect(scanRows('tools/ast-grep/rules/typescript/syntax/no-x.yml', _NO_IDS)).toStrictEqual([SCAN[_TREE], SCAN[_PAIRING]]);
        expect(scanRows('tools/ast-grep/tests/__snapshots__/no-x-snapshot.yml', _IDS)).toStrictEqual([SCAN[_TREE], SCAN[_PAIRING]]);
        expect(scanRows(_PYTHON_UTIL, _IDS)).toStrictEqual([SCAN[_UTILS], SCAN[_PAIRING]]);
        expect(scanRows(_BASH_UTIL, _NO_IDS)).toStrictEqual([SCAN[_UTILS], SCAN[_PAIRING]]);
        expect(scanRows('nx.json', _NO_IDS)).toStrictEqual([SCAN[_FAMILY], SCAN[_GRAPH]]);
        expect(scanRows('package.json', _NO_IDS)).toStrictEqual([SCAN[_FAMILY], SCAN[_GRAPH]]);
        expect(scanRows('libs/typescript/item/package.json', _NO_IDS)).toStrictEqual([SCAN[_FAMILY], SCAN[_GRAPH]]);
        expect(scanRows('eng/project.json', _NO_IDS)).toStrictEqual([SCAN[_FAMILY], SCAN[_GRAPH]]);
        expect(scanRows('tools/nx/workspace.ts', _NO_IDS)).toStrictEqual([SCAN[_FAMILY], SCAN[_GRAPH]]);
        expect(scanRows('README.md', _NO_IDS)).toStrictEqual([]);
        expect(scanRows('tools/ast-grep/rules/README.md', _NO_IDS)).toStrictEqual([]);
    });

    it('skips the task graph for a path outside the working directory, the absolute form relative keeps', () => {
        expect(scanRows('/private/tmp/scratchpad/package.json', _NO_IDS)).toStrictEqual([SCAN[_FAMILY]]);
        expect(scanRows('/private/tmp/scratchpad/tools/nx/workspace.ts', _NO_IDS)).toStrictEqual([SCAN[_FAMILY]]);
    });

    it('runs the test and pairing rows for a snapshot whatever the facts list, and needs the rule ids for a util alone', () => {
        expect(scanRows('tools/ast-grep/tests/__snapshots__/no-gone-snapshot.yml', _IDS)).toStrictEqual([SCAN[_TREE], SCAN[_PAIRING]]);
        expect(scanRows('tools/ast-grep/tests/__snapshots__/no-x-snapshot.yml', _NO_IDS)).toStrictEqual([SCAN[_TREE], SCAN[_PAIRING]]);
        expect(needsRuleIds(_PYTHON_UTIL)).toBe(true);
        expect(needsRuleIds('tools/ast-grep/tests/__snapshots__/no-x-snapshot.yml')).toBe(false);
        expect(needsRuleIds('tools/ast-grep/rules/python/anyio/no-fixed-sleep.yml')).toBe(false);
        expect(needsRuleIds('tools/ast-grep/utils/README.md')).toBe(false);
    });

    it('builds the argv per row from the path', () => {
        expect(_ROWS[_FAMILY]?.argv('tools/nx/workspace.ts', _NO_IDS)).toStrictEqual(['ast-grep', 'scan', '--json=compact', 'tools/nx/workspace.ts']);
        expect(_ROWS[_TREE]?.argv('tools/ast-grep/tests/typescript/syntax/no-x-test.yml', _NO_IDS)).toStrictEqual([
            'ast-grep',
            'test',
            '--include-off',
            '--filter',
            '^no-x$',
        ]);
        expect(_ROWS[_TREE]?.argv('tools/ast-grep/tests/__snapshots__/no-x-snapshot.yml', _NO_IDS).at(-1)).toBe('^no-x$');
        expect(_ROWS[_UTILS]?.argv(_PYTHON_UTIL, _IDS)).toStrictEqual([
            'ast-grep',
            'test',
            '--include-off',
            '--filter',
            '^(no-repeated-literal-in-function|no-fixed-sleep)$',
        ]);
        expect(_ROWS[_PAIRING]?.argv('tools/ast-grep/rules/dotnet/msbuild/no-x.yml', _NO_IDS)).toStrictEqual([_PAIRING_SCRIPT, 'pairing']);
        expect(_ROWS[_PAIRING]?.argv(_PYTHON_UTIL, _IDS)).toStrictEqual([_PAIRING_SCRIPT, 'pairing']);
        expect(_ROWS[_PAIRING]?.timeoutMs).toBeUndefined();
        expect(_ROWS[_GRAPH]?.argv('nx.json', _NO_IDS)).toStrictEqual([..._GRAPH_PREFIX, '--files=nx.json']);
        expect(_ROWS[_GRAPH]?.argv('libs/typescript/item/package.json', _NO_IDS).at(-1)).toBe('--files=libs/typescript/item/package.json');
    });

    it('filters the util test run to the rule ids of its language, and to a regex no id matches without one', () => {
        expect(_ROWS[_UTILS]?.argv('tools/ast-grep/utils/dotnet/msbuild-property.yml', _IDS).at(-1)).toBe('^(no-x-property)$');
        expect(_ROWS[_UTILS]?.argv(_BASH_UTIL, _IDS).at(-1)).toBe('^$');
        expect(_ROWS[_UTILS]?.argv(_PYTHON_UTIL, _NO_IDS).at(-1)).toBe('^$');
    });
});

describe('family scan lines', () => {
    it('reports no hit, each hit with its fix command, and an abort naming the grammar target', () => {
        expect(_lines(_FAMILY, _run(0, '[]'), 'a.ts')).toStrictEqual(['ast-grep scan: no hit in a.ts']);
        expect(_lines(_FAMILY, _run(1, _HITS), 'tools/nx/workspace.ts')).toStrictEqual([
            `tools/nx/workspace.ts:2 no-return-by-branch: ${_NOTE}`,
            "tools/nx/workspace.ts:8 no-fold-by-loop: Fold with reduce, fix: nx run rasm:rewrite -- --filter='^no-fold-by-loop$' --error='no-fold-by-loop' 'tools/nx/workspace.ts'",
        ]);
        expect(_lines(_FAMILY, _run(_GRAMMAR_EXIT, '', _GRAMMAR_STDERR), 'a.csproj')).toStrictEqual([
            'ast-grep scan exited 79: Error: Cannot load custom language library, run pnpm exec nx run rasm:grammar',
        ]);
        expect(_lines(_FAMILY, _run(_OTHER_EXIT, '', 'Error: something else\n'), 'a.ts')).toStrictEqual([
            'ast-grep scan exited 2: Error: something else',
        ]);
    });

    it('reports warnings from a successful scan', () => {
        expect(_lines(_FAMILY, _run(0, _HITS), _FILE)).toStrictEqual(_lines(_FAMILY, _run(1, _HITS), _FILE));
    });

    it.each([undefined, null])('keeps a diagnostic with note %s', (note) => {
        const hit = { ..._compact(0, 'no-x', _NOTE), note };
        expect(_lines(_FAMILY, _run(1, JSON.stringify([hit])), _FILE)).toStrictEqual([`${_FILE}:1 no-x: The shape`]);
    });

    it('quotes fix arguments without evaluating file names', () => {
        const hit = { ..._compact(0, 'no-x', _NOTE, 'value'), file: "dir/a' b$(echo x).ts" };
        expect(_lines(_FAMILY, _run(1, JSON.stringify([hit])), _FILE)[0]).toContain(` 'dir/a'"'"' b$(echo x).ts'`);
    });

    it('decodes the hits and reads a malformed stdout as none', () => {
        const hits = scanHits(_run(1, _HITS));
        expect(hits.map((hit) => [hit.ruleId, hit.file, hit.line, _orNull(hit.replacement)])).toStrictEqual([
            ['no-return-by-branch', _FILE, _BRANCH_LINE + 1, null],
            ['no-fold-by-loop', _FILE, _LOOP_LINE + 1, 'xs.reduce(f, 0)'],
        ]);
        expect(scanHits(_run(1, 'not json'))).toStrictEqual([]);
        expect(scanHits(_run(1, '{"ruleId":"x"}'))).toStrictEqual([]);
        expect(scanHits(_run(1, '[{"ruleId":"x"}]'))).toStrictEqual([]);
    });
});

describe('rule tree lines', () => {
    it('keeps the verdict lines under the stem prefix, the failure lines, and the unknown-id error', () => {
        expect(_lines(_TREE, _run(0, _TEST_STDOUT), 'tools/ast-grep/rules/typescript/claude-code/no-catch-around-next.yml')).toStrictEqual([
            'ast-grep test no-catch-around-next: test result: ok. 1 passed; 0 failed;',
        ]);
        expect(
            _lines(_TREE, _run(_FAIL_EXIT, _FAIL_STDOUT), 'tools/ast-grep/tests/typescript/claude-code/no-catch-around-next-test.yml'),
        ).toStrictEqual([
            'ast-grep test no-catch-around-next: FAIL no-catch-around-next',
            'ast-grep test no-catch-around-next: ╰▻ Noisy: valid case reported',
            'ast-grep test no-catch-around-next: test result: FAILED. 0 passed; 1 failed;',
        ]);
        expect(_lines(_TREE, _run(_UNKNOWN_EXIT, '', _UNKNOWN_STDERR), 'tools/ast-grep/tests/typescript/syntax/no-x-test.yml')).toStrictEqual([
            'ast-grep test no-x: Error: Rule not found: ^no-x$',
        ]);
        expect(_lines(_UTILS, _run(0, _TEST_STDOUT), 'tools/ast-grep/utils/typescript/syntax-x.yml')).toStrictEqual([
            'ast-grep test: test result: ok. 1 passed; 0 failed;',
        ]);
    });

    it('reports each pairing finding, no finding on a silent success, and the exit code of a silent failure', () => {
        const path = 'tools/ast-grep/rules/typescript/syntax/no-x.yml';
        expect(_lines(_PAIRING, _run(0, ''), path)).toStrictEqual(['rule-checks.sh pairing: no finding']);
        expect(_lines(_PAIRING, _run(_OTHER_EXIT, ''), path)).toStrictEqual(['rule-checks.sh pairing exited 2']);
        expect(_lines(_PAIRING, _run(1, 'no test: no-x\nid differs from file stem: a.yml\n'), path)).toStrictEqual([
            'no test: no-x',
            'id differs from file stem: a.yml',
        ]);
        expect(_lines(_PAIRING, _run(1, '', 'sgconfig.yml not found in /x\n'), path)).toStrictEqual(['sgconfig.yml not found in /x']);
    });
});

describe('telemetry', () => {
    it('names the keys to prune, a live hit stays', () => {
        expect(stale([_LIVE_A, _UNKNOWN, _OLD, _MALFORMED], _RULES, _NOW)).toStrictEqual([_UNKNOWN.key, _OLD.key, _MALFORMED.key]);
        expect(stale([_LIVE_A], [], _NOW)).toStrictEqual([_LIVE_A.key]);
    });

    it('names the scan keys past the window from the key list alone, an unknown rule inside it stays', () => {
        expect(expiredScans([_LIVE_A.key, _UNKNOWN.key, _OLD.key, _MALFORMED.key, 'session/s'], _NOW)).toStrictEqual([_OLD.key, _MALFORMED.key]);
    });

    it('counts hits per rule newest first and names the silent rules older than the window', () => {
        expect(getOrElse(() => null)(telemetryBlock([_LIVE_B, _LIVE_C, _LIVE_A, _UNKNOWN, _OLD], _RULES, _NOW))).toBe(
            [
                'Rule hits from edit-time scans in the last 30 days:',
                '- typescript/syntax/no-return-by-branch: 2 hits, last a.ts',
                '- typescript/syntax/no-fold-by-loop: 1 hits, last c.ts',
                'Rules with no hit in the window, landed before it: no-silent',
            ].join('\n'),
        );
        expect(getOrElse(() => null)(telemetryBlock([_LIVE_A], [_rule('no-return-by-branch', _YEAR_DAYS), _rule('no-young', _ONE_DAY)], _NOW))).toBe(
            [
                'Rule hits from edit-time scans in the last 30 days:',
                '- typescript/syntax/no-return-by-branch: 1 hits, last a.ts',
                'Every rule landed before the window fired at least once',
            ].join('\n'),
        );
        expect(getOrElse(() => null)(telemetryBlock([_LIVE_A], [], _NOW))).toBeNull();
    });
});

describe('facts', () => {
    it('states the version, the grammar, and the sorted families, utils, and rewrites', () => {
        expect(factsBlock(_FACTS)).toBe(
            [
                'ast-grep 0.45.3 under sgconfig.yml',
                'Custom grammar .cache/ast-grep/xml.so present',
                'Rules per family: dotnet/msbuild 40, typescript/claude-code 17, typescript/syntax 8',
                'Utils per language: dotnet 3, typescript 27',
                'Rewrites: dotnet/msbuild absolute-to-relative-path, dotnet/msbuild exec-to-task, typescript/effect option-nullish-ternary-to-from-nullable',
            ].join('\n'),
        );
    });

    it('reads a rejected run as an exit -1 with the error as stderr and names it in the version line', () => {
        expect(abort(new Error('spawn ENOENT'))).toStrictEqual({ exitCode: -1, stdout: '', stderr: 'Error: spawn ENOENT' });
        expect(factsBlock({ ..._FACTS, version: abort(new Error('spawn ENOENT')) }).split('\n')[0]).toBe(
            'ast-grep --version exited -1: Error: spawn ENOENT',
        );
    });

    it('names the grammar target when the grammar is absent and none without a rewrite', () => {
        expect(
            factsBlock({ ..._FACTS, grammar: false, rewrites: [] })
                .split('\n')
                .slice(1),
        ).toStrictEqual([
            'Custom grammar .cache/ast-grep/xml.so absent, every scan aborts until pnpm exec nx run rasm:grammar writes it',
            'Rules per family: dotnet/msbuild 40, typescript/claude-code 17, typescript/syntax 8',
            'Utils per language: dotnet 3, typescript 27',
            'Rewrites: none',
        ]);
    });
});

describe('task graph lines', () => {
    it('lists the affected projects and reports a failed query', () => {
        expect(_lines(_GRAPH, _run(0, '["rasm","eng"]\n'), 'nx.json')).toStrictEqual(['Affected projects (2): rasm, eng']);
        expect(_lines(_GRAPH, _run(0, '[]'), 'nx.json')).toStrictEqual(['Affected projects (0): ']);
        expect(_lines(_GRAPH, _run(1, '', ' NX   path should be a `path.relative()`d string\n\nPass --verbose\n'), '/r/nx.json')).toStrictEqual([
            'nx show projects exited 1:  NX   path should be a `path.relative()`d string',
        ]);
    });
});
