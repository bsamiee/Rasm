// --- [IMPORTS] -------------------------------------------------------------------------

import { describe, expect, it } from 'vitest';
import type { Decision } from '../composition/decision.ts';
import { type PathEvent, pathRule, pathSkills } from './paths.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Plain<D> =
    | { readonly kind: 'rewrite'; readonly context: readonly string[] }
    | { readonly kind: 'deny'; readonly reason: D }
    | { readonly kind: 'answer' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const _NONE: ReadonlySet<string> = new Set();
const _FRESH = { seen: _NONE, guidance: ['/repo/docs'], file: '' };
const _OVER = 160;
const _LONG = `- ${'x'.repeat(_OVER)}`;
// File with a third line an Edit replaces
const _FILE = '# Title\n\n- third line\n- fourth line\n';
const _SCRATCHPAD = '/private/tmp/claude-501/slug/session/scratchpad';

// --- [OPERATIONS] ----------------------------------------------------------------------

const _plain = <E, R, D>(decision: Decision<E, R, D>): Plain<D> =>
    decision.match<Plain<D>>({
        rewrite: (_e, context) => ({ kind: 'rewrite', context }),
        deny: (reason) => ({ kind: 'deny', reason }),
        answer: () => ({ kind: 'answer' }),
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
        expect(_plain(pathRule({ seen: new Set(pathSkills(e)), guidance: [], file: '' })(e))).toStrictEqual({ kind: 'rewrite', context: [] });
    });
});

describe('pathRule probe directory', () => {
    it('denies a write under the shared probe directory naming the labelled one, and passes the labelled write and a read', () => {
        expect(_plain(pathRule(_FRESH)(_write(`${_SCRATCHPAD}/probe/sgconfig.yml`, 'ruleDirs: []')))).toStrictEqual({
            kind: 'deny',
            reason: `${_SCRATCHPAD}/probe/sgconfig.yml sits in the probe/ directory every agent of the session shares, write under ${_SCRATCHPAD}/<label>-probe/sgconfig.yml, the label your brief states (main for the main agent)`,
        });
        expect(_plain(pathRule(_FRESH)(_write(`${_SCRATCHPAD}/x-probe/sgconfig.yml`, 'ruleDirs: []')))).toStrictEqual({
            kind: 'rewrite',
            context: [],
        });
        expect(_plain(pathRule(_FRESH)(_read(`${_SCRATCHPAD}/probe/sgconfig.yml`)))).toStrictEqual({ kind: 'rewrite', context: [] });
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
                pathRule({
                    seen: new Set(['dotnet-msbuild-packaging', 'dotnet-msbuild-evaluation', 'dotnet-msbuild-antipatterns']),
                    guidance: [],
                    file: '',
                })(
                    _edit(
                        '/repo/Directory.Packages.props',
                        '<PackageVersion Include="A" Version="1" />',
                        '<PackageVersion Include="A" Version="2" />',
                    ),
                ),
            ),
        ).toStrictEqual({ kind: 'rewrite', context: ['Verify the row with mcp__nuget__get_package_context'] });
        expect(
            _plain(pathRule({ seen: new Set(['manage-repo']), guidance: [], file: '' })(_edit('/repo/mise.toml', 'buf = "latest"', 'buf = "1.2.3"'))),
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
        expect(_plain(pathRule(_FRESH)(_edit('/repo/pnpm-workspace.yaml', 'catalog:\n  effect: 3.0.0\n', 'catalog:\n')))).toStrictEqual({
            kind: 'rewrite',
            context: ['Add the missing dependency record to its manifest or README.md dependency list and keep the dropped row'],
        });
    });
});
