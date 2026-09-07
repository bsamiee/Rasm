// --- [IMPORTS] -------------------------------------------------------------------------

import type { ToolCallInput } from 'claude-code';
import { describe, expect, it } from 'vitest';
import type { Decision } from '../composition/decision.ts';
import { getOrElse } from '../composition/option.ts';
import { DESCRIBE, describeRule, type Facts, toolRecords, toolRule, toolSkills } from './tools.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Plain<R, D> =
    | { readonly kind: 'rewrite'; readonly context: readonly string[] }
    | { readonly kind: 'deny'; readonly reason: D }
    | { readonly kind: 'answer'; readonly result: R };

// --- [CONSTANTS] -----------------------------------------------------------------------

const _FACTS: Facts = { seen: new Set(), snapshots: new Set(), dns: new Set(), prompt: 'restore the machine', cwd: '/Users/x/Rasm' };
// The ast-grep skill seen, the search cases then read the rewrite rows' lines without the server's once line
const _AST_GREP_SEEN: Facts = { ..._FACTS, seen: new Set(['ast-grep']) };
const _TSX_LINE = 'Ran with language tsx, sgconfig.yml languageGlobs maps every .ts file to tsx and typescript finds nothing';
const _FOLDER_LINE = 'Resolved project_folder against the working directory, the server reads an absolute path';
// The tools the ast-grep server lists, each with one describe line the model reads before its first call
const _AST_GREP_TOOLS = 4;
const _LINE_LIMIT = 200;
// Events the declarations narrow on their tool field, the API's own field names as computed keys
const _SEARCH: ToolCallInput = { tool: 'WebSearch', ['tool_use_id']: 'call', query: 'effect retry schedule' };
const _RESTORE: ToolCallInput = { tool: 'mcp__hostinger__VPS_restoreSnapshotV1', ['tool_use_id']: 'call', virtualMachineId: 1 };
const _SNAPSHOT: ToolCallInput = { tool: 'mcp__hostinger__VPS_createSnapshotV1', ['tool_use_id']: 'call', virtualMachineId: 7 };
const _PURCHASE: ToolCallInput = {
    // biome-ignore lint/security/noSecrets: the tool name as the declarations spell it, the literal narrows the event
    tool: 'mcp__hostinger__domains_purchaseNewDomainV1',
    ['tool_use_id']: 'call',
    domain: 'example.com',
    ['item_id']: 'hostingercom-domain-com-usd-1y',
};
const _ORDER: ToolCallInput = {
    tool: 'mcp__hostinger__billing_createPurchaseOrderV1',
    ['tool_use_id']: 'call',
    items: [{ ['item_id']: 'hostingercom-vps-kvm2-usd-1m' }],
};
const _RESET: ToolCallInput = { tool: 'mcp__hostinger__DNS_resetDNSRecordsV1', ['tool_use_id']: 'call', domain: 'example.com' };
const _RECORDS: ToolCallInput = { tool: 'mcp__hostinger__DNS_getDNSRecordsV1', ['tool_use_id']: 'call', domain: 'example.com' };
const _RESOLVE: ToolCallInput = { tool: 'mcp__context7__resolve-library-id', ['tool_use_id']: 'call', query: 'effect', libraryName: 'effect' };
const _RUN_CODE: ToolCallInput = { tool: 'mcp__playwright__browser_run_code_unsafe', ['tool_use_id']: 'call', code: '1' };
const _TRUST_DENY =
    'trust_solution on Workspace.slnx is a wasted call, the server trusts every solution on its command line for the session, call get_diagnostics with includeAnalyzers: true directly';
// The roslyn skill seen, the trust cases then read the row's own decision without the server's once line
const _ROSLYN_SEEN: Facts = { ..._FACTS, seen: new Set(['dotnet-roslyn-codelens']) };

// --- [OPERATIONS] ----------------------------------------------------------------------

const _plain = <E, R, D>(decision: Decision<E, R, D>): Plain<R, D> =>
    decision.match<Plain<R, D>>({
        rewrite: (_e, context) => ({ kind: 'rewrite', context }),
        deny: (reason) => ({ kind: 'deny', reason }),
        answer: (result) => ({ kind: 'answer', result }),
    });

const _fetch = (url: string): ToolCallInput => ({ tool: 'WebFetch', ['tool_use_id']: 'call', url, prompt: 'read the page' });

const _trust = (path: string): ToolCallInput => ({ tool: 'mcp__roslyn-codelens__trust_solution', ['tool_use_id']: 'call', path });

const _find = (language: string, folder: string): ToolCallInput => ({
    tool: 'mcp__ast-grep__find_code',
    ['tool_use_id']: 'call',
    pattern: 'const $A = $B',
    language,
    ['project_folder']: folder,
});

const _findByRule = (language: string, folder: string): ToolCallInput => ({
    tool: 'mcp__ast-grep__find_code_by_rule',
    ['tool_use_id']: 'call',
    yaml: `id: probe\nlanguage: ${language}\nrule: { pattern: 'const $A = $B' }`,
    ['project_folder']: folder,
});

// The event the rule passed beneath, its rewritten fields read beside the context
const _passed = <E, R, D>(decision: Decision<E, R, D>): E | null =>
    decision.match<E | null>({ rewrite: (e) => e, deny: () => null, answer: () => null });

// --- [TESTS] ---------------------------------------------------------------------------

describe('toolRule denies', () => {
    it('denies WebSearch and routes WebFetch by host', () => {
        expect(_plain(toolRule(_FACTS)(_SEARCH))).toStrictEqual({
            kind: 'deny',
            reason: 'WebSearch is refused, search with mcp__exa__web_search_exa or tvly search',
        });
        expect(_plain(toolRule(_FACTS)(_fetch('https://code.claude.com/docs/en/hooks')))).toStrictEqual({
            kind: 'deny',
            reason: 'WebFetch is refused, mcp__claudeCodeDocs__search_claude_code_docs',
        });
        expect(_plain(toolRule(_FACTS)(_fetch('https://github.com/anthropics/claude-code/blob/main/README.md')))).toStrictEqual({
            kind: 'deny',
            reason: 'WebFetch is refused, mcp__github__get_file_contents',
        });
        expect(_plain(toolRule(_FACTS)(_fetch('not a url')))).toStrictEqual({
            kind: 'deny',
            reason: 'WebFetch is refused, tvly extract or mcp__exa__web_fetch_exa',
        });
    });

    it('denies the unsafe browser tool', () => {
        expect(_plain(toolRule(_FACTS)(_RUN_CODE))).toStrictEqual({
            kind: 'deny',
            reason: 'browser_run_code_unsafe runs arbitrary code in the page, use the typed browser tools',
        });
    });

    it('denies trust_solution on the command-line solution and passes it on another solution', () => {
        expect(_plain(toolRule(_ROSLYN_SEEN)(_trust('/Users/x/Rasm/Workspace.slnx')))).toStrictEqual({ kind: 'deny', reason: _TRUST_DENY });
        expect(_plain(toolRule(_ROSLYN_SEEN)(_trust('/elsewhere/Other.slnx')))).toStrictEqual({ kind: 'rewrite', context: [] });
    });
});

describe('toolRule guards', () => {
    it('requires a recorded snapshot before a restore and accepts one', () => {
        expect(_plain(toolRule(_FACTS)(_RESTORE))).toStrictEqual({
            kind: 'deny',
            reason: 'Call mcp__hostinger__VPS_createSnapshotV1 on the machine first, then retry',
        });
        expect(_plain(toolRule({ ..._FACTS, snapshots: new Set(['1']), seen: new Set(['hostinger']) })(_RESTORE))).toStrictEqual({
            kind: 'rewrite',
            context: [],
        });
    });

    it('requires a recorded DNS read before a reset and records the read by its domain', () => {
        expect(_plain(toolRule({ ..._FACTS, seen: new Set(['hostinger']) })(_RESET))).toStrictEqual({
            kind: 'deny',
            reason: 'Call mcp__hostinger__DNS_getDNSRecordsV1 on the domain first, then retry',
        });
        expect(_plain(toolRule({ ..._FACTS, seen: new Set(['hostinger']), dns: new Set(['example.com']) })(_RESET))).toStrictEqual({
            kind: 'rewrite',
            context: [],
        });
        expect(getOrElse(() => null)(toolRecords(_RECORDS))).toStrictEqual({ kind: 'dns-read', id: 'example.com' });
    });

    it('requires the operator to name a purchase target in the prompt', () => {
        expect(_plain(toolRule({ ..._FACTS, seen: new Set(['hostinger']) })(_PURCHASE))).toStrictEqual({
            kind: 'deny',
            reason: 'The current prompt names no target, the operator names it before a purchase',
        });
        expect(_plain(toolRule({ ..._FACTS, seen: new Set(['hostinger']), prompt: 'buy example.com' })(_PURCHASE))).toStrictEqual({
            kind: 'rewrite',
            context: [],
        });
    });

    it('reads a billing order target from its first item', () => {
        expect(_plain(toolRule({ ..._FACTS, seen: new Set(['hostinger']) })(_ORDER))).toStrictEqual({
            kind: 'deny',
            reason: 'The current prompt names no target, the operator names it before a purchase',
        });
        expect(_plain(toolRule({ ..._FACTS, seen: new Set(['hostinger']), prompt: 'order hostingercom-vps-kvm2-usd-1m' })(_ORDER))).toStrictEqual({
            kind: 'rewrite',
            context: [],
        });
    });

    it('records a snapshot call by its machine id', () => {
        expect(getOrElse(() => null)(toolRecords(_SNAPSHOT))).toStrictEqual({ kind: 'vm-snapshot', id: '7' });
    });

    it('injects the server line once and stamps the skill', () => {
        expect(_plain(toolRule(_FACTS)(_RESOLVE))).toStrictEqual({
            kind: 'rewrite',
            context: ['Load the search-context7 skill, it caps each query'],
        });
        expect(toolSkills(_RESOLVE)).toStrictEqual(['search-context7']);
        expect(_plain(toolRule({ ..._FACTS, seen: new Set(['search-context7']) })(_RESOLVE))).toStrictEqual({ kind: 'rewrite', context: [] });
    });
});

describe('toolRule rewrites', () => {
    it('rewrites find_code from typescript to tsx and a relative folder to an absolute one with a line per row', () => {
        const decision = toolRule(_AST_GREP_SEEN)(_find('typescript', 'tools/nx'));
        expect(_plain(decision)).toStrictEqual({ kind: 'rewrite', context: [_TSX_LINE, _FOLDER_LINE] });
        expect(_passed(decision)).toStrictEqual(_find('tsx', '/Users/x/Rasm/tools/nx'));
    });

    it('rewrites the language line of a find_code_by_rule yaml and its relative folder', () => {
        const decision = toolRule(_AST_GREP_SEEN)(_findByRule('typescript', 'tools/nx'));
        expect(_plain(decision)).toStrictEqual({ kind: 'rewrite', context: [_TSX_LINE, _FOLDER_LINE] });
        expect(_passed(decision)).toStrictEqual(_findByRule('tsx', '/Users/x/Rasm/tools/nx'));
    });

    it('passes tsx with an absolute folder and another language untouched', () => {
        expect(_plain(toolRule(_AST_GREP_SEEN)(_find('tsx', '/Users/x/Rasm/tools/nx')))).toStrictEqual({ kind: 'rewrite', context: [] });
        expect(_plain(toolRule(_AST_GREP_SEEN)(_findByRule('tsx', '/Users/x/Rasm/tools/nx')))).toStrictEqual({ kind: 'rewrite', context: [] });
        expect(_plain(toolRule(_AST_GREP_SEEN)(_find('python', '/Users/x/Rasm/libs/python')))).toStrictEqual({ kind: 'rewrite', context: [] });
    });

    it('ends on a deny before any rewrite row', () => {
        expect(_plain(toolRule(_FACTS)(_SEARCH)).kind).toBe('deny');
    });
});

describe('describeRule', () => {
    it('prepends the owning skill for a server tool and passes an unknown tool', () => {
        expect(_plain(describeRule({ tool: 'mcp__context7__resolve-library-id', description: 'Resolves a library.' }))).toStrictEqual({
            kind: 'answer',
            result: { description: 'Load the search-context7 skill before the first call\nResolves a library.' },
        });
        expect(_plain(describeRule({ tool: 'Read', description: 'Reads a file.' }))).toStrictEqual({ kind: 'rewrite', context: [] });
    });

    it('prepends a built-in row alone', () => {
        expect(_plain(describeRule({ tool: 'WebSearch', description: 'Searches the web.' }))).toStrictEqual({
            kind: 'answer',
            result: { description: 'WebSearch is refused, mcp__exa__web_search_exa and tvly search replace it\nSearches the web.' },
        });
    });

    it('joins the roslyn skill line with the trust_solution row', () => {
        expect(_plain(describeRule({ tool: 'mcp__roslyn-codelens__trust_solution', description: 'Marks a solution trusted.' }))).toStrictEqual({
            kind: 'answer',
            result: {
                description: [
                    'Load the dotnet-roslyn-codelens skill before the first call',
                    'The command-line solution Workspace.slnx is trusted for the session, trust_solution serves another solution alone',
                    'Marks a solution trusted.',
                ].join('\n'),
            },
        });
    });

    it.each(DESCRIBE.filter((row) => row.tool.startsWith('mcp__ast-grep__')))('joins the skill line with the $tool row', ({ tool, line }) => {
        expect(_plain(describeRule({ tool, description: 'Runs ast-grep.' }))).toStrictEqual({
            kind: 'answer',
            result: { description: `Load the ast-grep skill before the first call\n${line}\nRuns ast-grep.` },
        });
    });

    it('holds one row per ast-grep tool, each one line under 200 characters with no trailing period', () => {
        const lines = DESCRIBE.filter((row) => row.tool.startsWith('mcp__ast-grep__')).map((row) => row.line);
        expect(lines).toHaveLength(_AST_GREP_TOOLS);
        expect(lines.every((line) => line.length < _LINE_LIMIT && !line.endsWith('.'))).toBe(true);
    });
});
