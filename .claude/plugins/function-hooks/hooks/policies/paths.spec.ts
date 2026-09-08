// --- [IMPORTS] -------------------------------------------------------------------------

import { describe, expect, it } from 'vitest';
import type { Decision } from '../composition/decision.ts';
import { toArray } from '../composition/option.ts';
import { landed, landing, type PathEvent, pathRule, pathSkills, recordSearches, type Searched } from './paths.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Plain<R, D> =
    | { readonly kind: 'rewrite'; readonly context: readonly string[] }
    | { readonly kind: 'deny'; readonly reason: D }
    | { readonly kind: 'answer'; readonly result: R };

// --- [CONSTANTS] -----------------------------------------------------------------------

const _NONE: ReadonlySet<string> = new Set();
const _FRESH = { seen: _NONE, guidance: ['/repo/docs'], file: '', searches: [] };
const _PACKAGING: ReadonlySet<string> = new Set(['dotnet-msbuild-packaging', 'dotnet-msbuild-evaluation', 'dotnet-msbuild-antipatterns']);
const _CATALOG_ROW = 'catalog:\n    ssh2: 1.17.0\n';
const _MANIFEST_ROW = '        "ssh2": "catalog:",\n';
const _VERSION_ROW = '<PackageVersion Include="Thinktecture.Runtime.Extensions" Version="9.0.0" />\n';
const _KEEP = 'keep the dropped row and add the missing dependency record, or remove it there too';
const _OVER = 160;
const _LONG = `- ${'x'.repeat(_OVER)}`;
// File with a third line an Edit replaces
const _FILE = '# Title\n\n- third line\n- fourth line\n';
const _SCRATCHPAD = '/private/tmp/claude-501/slug/session/scratchpad';
const _SCRATCH = '/repo/.claude/scratch/agent-context-gathering/probe';

// --- [OPERATIONS] ----------------------------------------------------------------------

const _plain = <E, R, D>(decision: Decision<E, R, D>): Plain<R, D> =>
    decision.match<Plain<R, D>>({
        rewrite: (_e, context) => ({ kind: 'rewrite', context }),
        deny: (reason) => ({ kind: 'deny', reason }),
        answer: (result) => ({ kind: 'answer', result }),
    });

// The API's own field names as computed keys, the naming rule then reads them as the declarations spell them
const _read = (path: string): PathEvent => ({ tool: 'Read', ['tool_use_id']: 'call', ['file_path']: path });

const _write = (path: string, content: string): PathEvent => ({ tool: 'Write', ['tool_use_id']: 'call', ['file_path']: path, content });

const _edit = (path: string, oldString: string, newString: string): PathEvent => ({
    tool: 'Edit',
    ['tool_use_id']: 'call',
    ['file_path']: path,
    ['old_string']: oldString,
    ['new_string']: newString,
});

// A finished search as rg answers it, exit 0 with the holder paths on stdout and exit 1 with none
const _search = (name: string, exitCode: number, stdout: string, stderr = ''): Searched => ({ name, argv: [], run: { exitCode, stdout, stderr } });

const _cell = (path: string, source: string): PathEvent => ({
    tool: 'NotebookEdit',
    ['tool_use_id']: 'call',
    ['notebook_path']: path,
    ['new_source']: source,
    ['edit_mode']: 'insert',
    ['cell_type']: 'markdown',
});

// --- [TESTS] ---------------------------------------------------------------------------

describe('pathRule', () => {
    it('denies a binlog read naming the overview tool', () => {
        expect(_plain(pathRule(_FRESH)(_read('/repo/x.binlog')))).toStrictEqual({
            kind: 'deny',
            reason: '.binlog files are binary, call mcp__binlog__binlog_overview on the file',
        });
    });

    it('denies an op reference outside the skills directory and passes one inside it', () => {
        expect(_plain(pathRule(_FRESH)(_write('/repo/tmp-proof.md', 'op://Tokens/x/credential')))).toStrictEqual({
            kind: 'deny',
            reason: 'Secrets come from Doppler alone, an op:// reference never lands in a file, use doppler secrets',
        });
        expect(_plain(pathRule(_FRESH)(_write('/repo/.claude/skills/secrets/SKILL.md', 'op read op://x')))).toStrictEqual({
            kind: 'rewrite',
            context: ['Search the docs with mcp__claudeCodeDocs__search_claude_code_docs'],
        });
    });

    it.each(['report.md', 'summary.md', 'FINDINGS.md'])('answers a scratch Write of %s as a created file the adapter lands', (name) => {
        const path = `${_SCRATCH}/${name}`;
        const e = _write(path, 'probe line');
        expect(_plain(pathRule(_FRESH)(e))).toStrictEqual({
            kind: 'answer',
            result: { type: 'create', filePath: path, content: 'probe line', structuredPatch: [], originalFile: null },
        });
        expect(toArray(landing(e))).toStrictEqual([
            {
                argv: [
                    'sh',
                    '-c',
                    'if [ -e "$1" ]; then echo update; cat -- "$1"; else echo create; fi && mkdir -p "$(dirname "$1")" && cat > "$1"',
                    'sh',
                    path,
                ],
                stdin: 'probe line',
                result: { type: 'create', filePath: path, content: 'probe line', structuredPatch: [], originalFile: null },
            },
        ]);
    });

    it('answers an update over the previous text the child printed and the created record otherwise', () => {
        const created = { type: 'create' as const, filePath: `${_SCRATCH}/report.md`, content: 'two', structuredPatch: [], originalFile: null };
        expect(landed(created, 'update\none')).toStrictEqual({ ...created, type: 'update', originalFile: 'one' });
        expect(landed(created, 'update\n')).toStrictEqual({ ...created, type: 'update', originalFile: '' });
        expect(landed(created, 'create\n')).toStrictEqual(created);
    });

    it('passes a scratch Write of another name, a report outside scratch, and a report Edit to the engine', () => {
        expect(_plain(pathRule(_FRESH)(_write(`${_SCRATCH}/record.md`, 'probe line')))).toStrictEqual({
            kind: 'rewrite',
            context: ['Search the docs with mcp__claudeCodeDocs__search_claude_code_docs'],
        });
        expect(_plain(pathRule(_FRESH)(_write('/repo/docs/report.md', 'probe line')))).toStrictEqual({ kind: 'rewrite', context: [] });
        expect(_plain(pathRule(_FRESH)(_edit(`${_SCRATCH}/report.md`, 'probe line', 'probe line edited')))).toStrictEqual({
            kind: 'rewrite',
            context: ['Search the docs with mcp__claudeCodeDocs__search_claude_code_docs'],
        });
        expect(toArray(landing(_write(`${_SCRATCH}/record.md`, 'probe line')))).toStrictEqual([]);
    });

    it('denies an op reference in a notebook cell and passes markdown source outside the guidance directories', () => {
        expect(_plain(pathRule(_FRESH)(_cell('/repo/tmp-proof.ipynb', 'op://Tokens/x/credential')))).toStrictEqual({
            kind: 'deny',
            reason: 'Secrets come from Doppler alone, an op:// reference never lands in a file, use doppler secrets',
        });
        expect(_plain(pathRule(_FRESH)(_cell('/repo/scratch/a.ipynb', `# Title\n${_LONG}`)))).toStrictEqual({ kind: 'rewrite', context: [] });
    });

    it('measures a markdown entry and a self-reference', () => {
        expect(_plain(pathRule(_FRESH)(_write('/repo/docs/a.md', `# Title\n${_LONG}\nRead this file first.`)))).toStrictEqual({
            kind: 'rewrite',
            context: [`Entry at line 2 is ${_LONG.length} columns, the limit is 150`, 'Self-reference at line 3: this file'],
        });
    });

    it('leaves a markdown file outside the guidance directories unmeasured', () => {
        expect(_plain(pathRule(_FRESH)(_write('/repo/scratch/a.md', `# Title\n${_LONG}`)))).toStrictEqual({ kind: 'rewrite', context: [] });
    });

    it('routes a C# file, an infra file, and an env file to their skills', () => {
        expect(_plain(pathRule(_FRESH)(_read('/repo/libs/dotnet/A.cs')))).toStrictEqual({
            kind: 'rewrite',
            context: ['Load the dotnet-roslyn-codelens skill', 'Load the dotnet-coding skill'],
        });
        expect(_plain(pathRule(_FRESH)(_read('/repo/infra/index.ts')))).toStrictEqual({
            kind: 'rewrite',
            context: ['Load the manage-repo skill', 'Load the pulumi skill'],
        });
        expect(_plain(pathRule(_FRESH)(_read('/repo/.env.local')))).toStrictEqual({ kind: 'rewrite', context: ['Load the secrets skill'] });
    });

    it.each(['mise.toml', 'mise.unix.toml', '.miserc.toml', 'nx.json'])('routes the toolchain file %s to the manage-repo skill', (name) => {
        expect(_plain(pathRule(_FRESH)(_read(`/repo/${name}`)))).toStrictEqual({ kind: 'rewrite', context: ['Load the manage-repo skill'] });
    });

    it('injects each skill line once and stamps the keys', () => {
        const e = _edit('/repo/eng/native/Directory.Build.props', '<Target Name="a" />', '<Target Name="b" />');
        expect(_plain(pathRule(_FRESH)(e))).toStrictEqual({
            kind: 'rewrite',
            context: [
                'Load the dotnet-msbuild-evaluation skill',
                'Load the dotnet-msbuild-antipatterns skill',
                'Load the dotnet-msbuild-execution skill',
                'Load the manage-repo skill',
            ],
        });
        expect(pathSkills(e)).toStrictEqual(['dotnet-msbuild-evaluation', 'dotnet-msbuild-antipatterns', 'dotnet-msbuild-execution', 'manage-repo']);
        expect(_plain(pathRule({ ..._FRESH, seen: new Set(pathSkills(e)), guidance: [] })(e))).toStrictEqual({ kind: 'rewrite', context: [] });
    });
});

describe('pathRule line numbers', () => {
    it('numbers an Edit from the file line the replaced text starts at', () => {
        expect(_plain(pathRule({ ..._FRESH, file: _FILE })(_edit('/repo/docs/a.md', '- third line', _LONG)))).toStrictEqual({
            kind: 'rewrite',
            context: [`Entry at line 3 is ${_LONG.length} columns, the limit is 150`],
        });
    });

    it('numbers a Write from its content', () => {
        expect(_plain(pathRule({ ..._FRESH, file: _FILE })(_write('/repo/docs/a.md', `# Title\n${_LONG}`)))).toStrictEqual({
            kind: 'rewrite',
            context: [`Entry at line 2 is ${_LONG.length} columns, the limit is 150`],
        });
    });

    it('numbers an Edit from the replacement when the file lacks the replaced text', () => {
        expect(_plain(pathRule({ ..._FRESH, file: _FILE })(_edit('/repo/docs/a.md', '- absent line', `x\n${_LONG}`)))).toStrictEqual({
            kind: 'rewrite',
            context: [`Entry at line 2 is ${_LONG.length} columns, the limit is 150`],
        });
    });
});

describe('pathRule per edit', () => {
    it('adds the per-edit lines for a package version, a tool version, and a pin', () => {
        expect(
            _plain(
                pathRule({ ..._FRESH, seen: _PACKAGING, guidance: [] })(
                    _edit(
                        '/repo/Directory.Packages.props',
                        '<PackageVersion Include="A" Version="1" />',
                        '<PackageVersion Include="A" Version="2" />',
                    ),
                ),
            ),
        ).toStrictEqual({ kind: 'rewrite', context: ['Verify the row with mcp__nuget__get_package_context'] });
        expect(
            _plain(
                pathRule({ ..._FRESH, seen: new Set(['manage-repo']), guidance: [] })(_edit('/repo/mise.toml', 'buf = "latest"', 'buf = "1.2.3"')),
            ),
        ).toStrictEqual({
            kind: 'rewrite',
            context: ['Pinned tool versions take their reason in a comment on the row'],
        });
        expect(_plain(pathRule(_FRESH)(_edit('/repo/pyproject.toml', '"httpx",', '"httpx==0.28",')))).toStrictEqual({
            kind: 'rewrite',
            context: ['The lock file alone pins versions, spell the row unpinned'],
        });
    });

    it('tells an added dependency row from a dropped one', () => {
        expect(_plain(pathRule(_FRESH)(_edit('/repo/pnpm-workspace.yaml', 'catalog:\n', 'catalog:\n  effect: 3.0.0\n')))).toStrictEqual({
            kind: 'rewrite',
            context: ['Record the dependency in the owning README.md dependency list'],
        });
        expect(
            _plain(
                pathRule({ ..._FRESH, searches: [_search('effect', 0, 'package.json\n')] })(
                    _edit('/repo/pnpm-workspace.yaml', 'catalog:\n  effect: 3.0.0\n', 'catalog:\n'),
                ),
            ),
        ).toStrictEqual({ kind: 'rewrite', context: [`effect remains in package.json, ${_KEEP}`] });
    });
});

describe('pathRule dependency records', () => {
    it('reports a catalog-only deletion while the manifest holds the package', () => {
        expect(
            _plain(
                pathRule({ ..._FRESH, searches: [_search('ssh2', 0, 'package.json\n')] })(
                    _edit('/repo/pnpm-workspace.yaml', _CATALOG_ROW, 'catalog:\n'),
                ),
            ),
        ).toStrictEqual({
            kind: 'rewrite',
            context: [`ssh2 remains in package.json, ${_KEEP}`],
        });
    });

    it('reports a manifest-only deletion while the catalog holds the package', () => {
        expect(
            _plain(pathRule({ ..._FRESH, searches: [_search('ssh2', 0, 'pnpm-workspace.yaml\n')] })(_edit('/repo/package.json', _MANIFEST_ROW, ''))),
        ).toStrictEqual({
            kind: 'rewrite',
            context: [`ssh2 remains in pnpm-workspace.yaml, ${_KEEP}`],
        });
    });

    it('passes the deletion from the last record in either order', () => {
        expect(_plain(pathRule({ ..._FRESH, searches: [_search('ssh2', 1, '')] })(_edit('/repo/package.json', _MANIFEST_ROW, '')))).toStrictEqual({
            kind: 'rewrite',
            context: [],
        });
        expect(
            _plain(pathRule({ ..._FRESH, searches: [_search('ssh2', 1, '')] })(_edit('/repo/pnpm-workspace.yaml', _CATALOG_ROW, 'catalog:\n'))),
        ).toStrictEqual({
            kind: 'rewrite',
            context: [],
        });
    });

    it('reports a package version deletion while a project references the package', () => {
        const holders = 'libs/dotnet/A/A.csproj\ntests/dotnet/B/B.csproj\n';
        expect(
            _plain(
                pathRule({ ..._FRESH, seen: _PACKAGING, searches: [_search('Thinktecture.Runtime.Extensions', 0, holders)] })(
                    _edit('/repo/Directory.Packages.props', _VERSION_ROW, ''),
                ),
            ),
        ).toStrictEqual({
            kind: 'rewrite',
            context: [`Thinktecture.Runtime.Extensions remains in libs/dotnet/A/A.csproj, tests/dotnet/B/B.csproj, ${_KEEP}`],
        });
    });

    it('reports a search child that failed', () => {
        expect(
            _plain(pathRule({ ..._FRESH, searches: [_search('ssh2', -1, '', 'spawn rg ENOENT')] })(_edit('/repo/package.json', _MANIFEST_ROW, ''))),
        ).toStrictEqual({
            kind: 'rewrite',
            context: ['The record search for ssh2 failed, rg exited -1: spawn rg ENOENT'],
        });
    });
});

describe('recordSearches', () => {
    it('builds one rg search per dropped row over the sibling records, the edited file excluded last', () => {
        expect(recordSearches(_edit('/repo/pnpm-workspace.yaml', _CATALOG_ROW, 'catalog:\n'), '/repo')).toStrictEqual([
            {
                name: 'ssh2',
                argv: [
                    'rg',
                    '--files-with-matches',
                    '--ignore-case',
                    '--hidden',
                    '--glob',
                    'package.json',
                    '--glob',
                    'pnpm-workspace.yaml',
                    '--glob',
                    'README.md',
                    '--glob',
                    '!/pnpm-workspace.yaml',
                    '--regexp',
                    '^\\s+\'?ssh2\'?:|^\\s*"ssh2":\\s*"|`ssh2`',
                ],
            },
        ]);
    });

    it('escapes a NuGet id and normalizes a Python name', () => {
        const dotnet = recordSearches(_edit('/repo/Directory.Packages.props', _VERSION_ROW, ''), '/repo');
        expect(dotnet.map((search) => search.argv.at(-1))).toStrictEqual([
            'PackageReference\\s+Include="Thinktecture\\.Runtime\\.Extensions"|`Thinktecture\\.Runtime\\.Extensions`',
        ]);
        expect(dotnet.map((search) => search.argv.includes('!/Directory.Packages.props'))).toStrictEqual([true]);
        expect(
            recordSearches(_edit('/repo/pyproject.toml', '    "typing-extensions>=4",\n', ''), '/repo').map((search) => search.argv.at(-1)),
        ).toStrictEqual(['^\\s*"typing[-_.]+extensions[^\\w.-]|^name = "typing[-_.]+extensions"|`typing-extensions`']);
    });

    it('builds no search for a read, a write, an added row, or a renamed file', () => {
        expect(recordSearches(_read('/repo/package.json'), '/repo')).toStrictEqual([]);
        expect(recordSearches(_write('/repo/package.json', _MANIFEST_ROW), '/repo')).toStrictEqual([]);
        expect(recordSearches(_edit('/repo/package.json', '', _MANIFEST_ROW), '/repo')).toStrictEqual([]);
        expect(recordSearches(_edit('/repo/deps.yaml', _CATALOG_ROW, 'catalog:\n'), '/repo')).toStrictEqual([]);
    });
});
