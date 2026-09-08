// Tool and server rows typed over the generated declarations for the tool.call and tool.describe events

// --- [IMPORTS] -------------------------------------------------------------------------

import type { BuiltinToolName, McpContentBlock, McpToolInputs, ToolCallInput } from 'claude-code';
import { answer, type Decision, deny, rewrite } from '../composition/decision.ts';
import { isRecord, isString, type Namespace } from '../host/store.ts';
import { basename } from '../text/path.ts';
import type { OnceLine } from './paths.ts';
import { SOLUTION } from './roslyn.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type ToolName = keyof McpToolInputs | BuiltinToolName;

// The event of one tool as the declarations narrow it on the tool field, its arguments typed
type Named<T extends ToolName> = Extract<ToolCallInput, { readonly tool: T }>;

// The event a rewrite row changed with the line naming the change
interface Rewritten {
    readonly e: ToolCallInput;
    readonly context: string;
}

// The session facts the rows read, the once keys injected, the recorded snapshots and DNS reads, the redacted prompt, and the working directory
interface Facts {
    readonly seen: ReadonlySet<string>;
    readonly snapshots: ReadonlySet<string>;
    readonly dns: ReadonlySet<string>;
    readonly prompt: string;
    readonly cwd: string;
}

// Rows over one tool's event as the declarations type it, an undefined deny, answer, or rewrite passes the call on, and records names the
// namespace a successful call stamps its target id under
interface Rules<T extends ToolName> {
    readonly deny?: (e: Named<T>) => string | undefined;
    readonly answer?: (e: Named<T>) => readonly McpContentBlock[] | undefined;
    readonly rewrite?: (e: Named<T>, facts: Facts) => Rewritten | undefined;
    readonly once?: OnceLine;
    readonly each?: string;
    readonly records?: Namespace;
}

// A row as the table holds it, its rules lifted over the event union through the tool's own refinement
interface ToolRow extends Rules<ToolName> {
    readonly tool: ToolName;
}

// A destructive hostinger tool family by name prefix, the fact the session must hold before a call and the reason without it
interface Family {
    readonly prefix: string;
    readonly holds: (facts: Facts, id: string) => boolean;
    readonly reason: string;
}

interface Recorded {
    readonly namespace: Namespace;
    readonly id: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _SERVER_NAME = /^mcp__(?<server>.+?)__/u;
const _TSX_LINE = 'Ran with language tsx, sgconfig.yml languageGlobs maps every .ts file to tsx and typescript finds nothing';
const _FOLDER_LINE = 'Resolved project_folder against the working directory, the server reads an absolute path';
const _YAML_TYPESCRIPT = /^(?<key>\s*language:\s*)typescript\s*$/mu;

// The route WebFetch names per host, and the fetch tools for every other host
const FETCH: Readonly<Record<string, string>> = {
    'code.claude.com': 'mcp__claudeCodeDocs__search_claude_code_docs',
    'docs.anthropic.com': 'mcp__claudeCodeDocs__search_claude_code_docs',
    'platform.openai.com': 'mcp__openaiDeveloperDocs__search_openai_docs',
    'github.com': 'mcp__github__get_file_contents',
    'nuget.org': 'mcp__nuget__get_package_context',
};
const _FETCH_FALLBACK = 'tvly extract or mcp__exa__web_fetch_exa';

const FAMILIES: readonly Family[] = [
    ...['mcp__hostinger__VPS_recreate', 'mcp__hostinger__VPS_restore', 'mcp__hostinger__VPS_delete'].map(
        (prefix): Family => ({
            prefix,
            holds: (facts, id): boolean => facts.snapshots.has(id),
            reason: 'Call mcp__hostinger__VPS_createSnapshotV1 on the machine first, then retry',
        }),
    ),
    {
        prefix: 'mcp__hostinger__DNS_reset',
        holds: (facts, id): boolean => facts.dns.has(id),
        reason: 'Call mcp__hostinger__DNS_getDNSRecordsV1 on the domain first, then retry',
    },
    ...['mcp__hostinger__domains_purchase', 'mcp__hostinger__billing_createPurchaseOrder', 'mcp__hostinger__VPS_purchase'].map(
        (prefix): Family => ({
            prefix,
            holds: (facts, id): boolean => id !== '' && facts.prompt.includes(id),
            reason: 'The current prompt names no target, the operator names it before a purchase',
        }),
    ),
];

// The server trusts every solution on its command line for the session, and .mcp.json names the workspace solution there
const _TRUST_ANSWER: readonly McpContentBlock[] = [
    {
        type: 'text',
        text: `${SOLUTION} is trusted for the session, the server trusts every solution on its command line, call get_diagnostics with includeAnalyzers: true`,
    },
];

// One skill per server, its line injected once per session before the first call
const SERVERS: Readonly<Partial<Record<string, OnceLine>>> = {
    context7: { key: 'search-context7', line: 'Load the search-context7 skill, it caps each query' },
    'roslyn-codelens': { key: 'dotnet-roslyn-codelens', line: 'Load the dotnet-roslyn-codelens skill' },
    hostinger: { key: 'hostinger', line: 'Load the hostinger skill' },
    binlog: { key: 'dotnet-msbuild-diagnostics', line: 'Load the dotnet-msbuild-diagnostics skill for the binlog tools' },
    nuget: { key: 'dotnet-msbuild-packaging', line: 'Load the dotnet-msbuild-packaging skill for the nuget tools' },
    'ast-grep': { key: 'ast-grep', line: 'Load the ast-grep skill for the ast-grep tools' },
};

// One line per tool that a fresh session calls wrong the first time, prepended to the tool's description
const DESCRIBE: Readonly<Partial<Record<ToolName, string>>> = {
    ['WebSearch']: 'WebSearch is refused, mcp__exa__web_search_exa and tvly search replace it',
    ['WebFetch']: 'WebFetch is refused, the docs and github tools, tvly extract, and mcp__exa__web_fetch_exa replace it',
    'mcp__roslyn-codelens__trust_solution': `The command-line solution ${SOLUTION} is trusted for the session, the plugin answers a call on it without the server, and trust_solution serves another solution alone`,
    'mcp__ast-grep__dump_syntax_tree': 'format=cst shows the node kinds and field names a rule reads, and format=pattern shows how a pattern parses',
    'mcp__ast-grep__find_code': 'The plugin rewrites language typescript to tsx and a relative project_folder to an absolute one',
    'mcp__ast-grep__find_code_by_rule':
        'The plugin rewrites language typescript to tsx and a relative project_folder to an absolute one, inline rules bind local utils alone, and ast-grep scan -c <config> proves a global util',
    'mcp__ast-grep__test_match_code_rule':
        "No match, a rejected rule, and an error diagnostic read as one failure, and printf '<code>' | ast-grep scan --inline-rules '<yaml>' --json --stdin; echo $? separates them by exit code 0, 8, and 1",
};

// --- [OPERATIONS] ----------------------------------------------------------------------

// The declarations' own narrowing on the tool field
const isTool =
    <T extends ToolName>(tool: T): ((e: ToolCallInput) => e is Named<T>) =>
    (e: ToolCallInput): e is Named<T> =>
        e.tool === tool;

const _route = (url: string): string => {
    const host = URL.canParse(url) ? new URL(url).hostname : '';
    return Object.entries(FETCH).find(([suffix]) => host.endsWith(suffix))?.[1] ?? _FETCH_FALLBACK;
};

// A row of one tool, each rule typed over that tool's event and lifted over the union, an event of another tool passes
const _row = <T extends ToolName>(tool: T, rules: Rules<T>): ToolRow => {
    const is = isTool(tool);
    return {
        ...rules,
        tool,
        deny: (e): string | undefined => (is(e) ? rules.deny?.(e) : undefined),
        answer: (e): readonly McpContentBlock[] | undefined => (is(e) ? rules.answer?.(e) : undefined),
        rewrite: (e, facts): Rewritten | undefined => (is(e) ? rules.rewrite?.(e, facts) : undefined),
    };
};

// The folder of the two ast-grep search tools resolved against the working directory
const _absoluteFolder = (e: Named<'mcp__ast-grep__find_code' | 'mcp__ast-grep__find_code_by_rule'>, facts: Facts): Rewritten | undefined =>
    e.project_folder.startsWith('/') ? undefined : { e: { ...e, ['project_folder']: `${facts.cwd}/${e.project_folder}` }, context: _FOLDER_LINE };

// Rows over the tool names the declarations know, a tool's rewrite rows run in table order and each reads the event the one before produced
const TOOLS: readonly ToolRow[] = [
    _row('WebSearch', { deny: (): string => 'WebSearch is refused, search with mcp__exa__web_search_exa or tvly search' }),
    _row('WebFetch', { deny: (e): string => `WebFetch is refused, ${_route(e.url)}` }),
    _row('mcp__playwright__browser_run_code_unsafe', {
        deny: (): string => 'browser_run_code_unsafe runs arbitrary code in the page, use the typed browser tools',
    }),
    _row('mcp__roslyn-codelens__trust_solution', {
        answer: (e): readonly McpContentBlock[] | undefined => (basename(e.path) === SOLUTION ? _TRUST_ANSWER : undefined),
    }),
    _row('mcp__hostinger__VPS_createSnapshotV1', { records: 'snapshot' }),
    _row('mcp__hostinger__DNS_getDNSRecordsV1', { records: 'dns' }),
    _row('mcp__ast-grep__find_code', {
        rewrite: (e): Rewritten | undefined => (e.language === 'typescript' ? { e: { ...e, language: 'tsx' }, context: _TSX_LINE } : undefined),
    }),
    _row('mcp__ast-grep__find_code', { rewrite: _absoluteFolder }),
    _row('mcp__ast-grep__find_code_by_rule', {
        rewrite: (e): Rewritten | undefined =>
            _YAML_TYPESCRIPT.test(e.yaml) ? { e: { ...e, yaml: e.yaml.replace(_YAML_TYPESCRIPT, '$<key>tsx') }, context: _TSX_LINE } : undefined,
    }),
    _row('mcp__ast-grep__find_code_by_rule', { rewrite: _absoluteFolder }),
];

const _rows = (e: ToolCallInput): readonly ToolRow[] => TOOLS.filter((row) => row.tool === e.tool);

// The billing tool names its targets under items, the other purchases under item_id, the machine and domain tools by their id
const _id = (e: ToolCallInput): string => {
    const fields: Readonly<Record<string, unknown>> = e;
    const items = Array.isArray(fields.items) ? fields.items : [];
    const first: unknown = items[0];
    return String(fields.virtualMachineId ?? fields.domain ?? fields.item_id ?? (isRecord(first) && isString(first.item_id) ? first.item_id : ''));
};

const _serverOnce = (tool: string): OnceLine | undefined => SERVERS[_SERVER_NAME.exec(tool)?.groups?.server ?? ''];

// The once lines the call raises, the server's skill line then the tool's own
const toolOnce = (e: ToolCallInput): readonly OnceLine[] => [_serverOnce(e.tool) ?? [], _rows(e).flatMap((row) => row.once ?? [])].flat();

const _familyReason = (e: ToolCallInput, facts: Facts): string | undefined => {
    const family = FAMILIES.find((candidate) => e.tool.startsWith(candidate.prefix));
    return family !== undefined && !family.holds(facts, _id(e)) ? family.reason : undefined;
};

// The table answer ends the rule, then the table deny or the family requirement, otherwise the rewrite rows run and their lines join the once and each lines
const toolRule =
    (facts: Facts): ((e: ToolCallInput) => Decision<ToolCallInput, readonly McpContentBlock[]>) =>
    (e: ToolCallInput): Decision<ToolCallInput, readonly McpContentBlock[]> => {
        const rows = _rows(e);
        const answered = rows.map((row) => row.answer?.(e)).find((blocks) => blocks !== undefined);
        if (answered !== undefined) {
            return answer(answered);
        }
        const reason = rows.map((row) => row.deny?.(e)).find((found) => found !== undefined) ?? _familyReason(e, facts);
        if (reason !== undefined) {
            return deny(reason);
        }
        const applied = rows.reduce<{ readonly e: ToolCallInput; readonly context: readonly string[] }>(
            (state, row) => {
                const next = row.rewrite?.(state.e, facts);
                return next === undefined ? state : { e: next.e, context: [...state.context, next.context] };
            },
            { e, context: [] },
        );
        const once = toolOnce(e).filter((line) => line.key === undefined || !facts.seen.has(line.key));
        return rewrite(applied.e, [...applied.context, ...once.map((line) => line.line), ...rows.flatMap((row) => row.each ?? [])]);
    };

// The fact a successful call records, read by the hostinger families later in the session
const toolRecords = (e: ToolCallInput): Recorded | undefined => {
    const [namespace] = _rows(e).flatMap((row) => row.records ?? []);
    return namespace === undefined ? undefined : { namespace, id: _id(e) };
};

// The owning skill's line, then the tool's own row, one per line, undefined when the tool has neither
const describeLine = (tool: string): string | undefined => {
    const server = _serverOnce(tool);
    const own = DESCRIBE[tool as ToolName];
    const text = [...(server === undefined ? [] : [`Load the ${server.key} skill before the first call`]), ...(own === undefined ? [] : [own])].join(
        '\n',
    );
    return text === '' ? undefined : text;
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Facts, Family, Named, Recorded, ToolName, ToolRow };
export { describeLine, isTool, toolOnce, toolRecords, toolRule };
