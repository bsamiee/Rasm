// Tool and server rows typed over the generated declarations for the tool.call and tool.describe events

// --- [IMPORTS] -------------------------------------------------------------------------

import type { BuiltinToolName, McpToolInputs, ToolCallInput } from 'claude-code';
import { type Decision, deny, rewrite } from '../composition/decision.ts';
import { isRecord, isString, type Namespace } from '../host/store.ts';
import type { OnceLine } from './paths.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type ToolName = keyof McpToolInputs | BuiltinToolName;

// The event of one tool as the declarations narrow it on the tool field, its arguments typed
type Named<T extends ToolName> = Extract<ToolCallInput, { readonly tool: T }>;

// The session facts the rows read, the once keys injected, the recorded snapshots and DNS reads, and the redacted prompt
interface Facts {
    readonly seen: ReadonlySet<string>;
    readonly snapshots: ReadonlySet<string>;
    readonly dns: ReadonlySet<string>;
    readonly prompt: string;
}

// Rows over one tool's event as the declarations type it, an undefined deny passes the call on, and records names the namespace a
// successful call stamps its target id under
interface Rules<T extends ToolName> {
    readonly deny?: (e: Named<T>) => string | undefined;
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

// The route WebFetch names per host, and the fetch tools for every other host
const FETCH: Readonly<Record<string, string>> = {
    'code.claude.com': 'mcp__claudeCodeDocs__search_claude_code_docs',
    'docs.anthropic.com': 'mcp__claudeCodeDocs__search_claude_code_docs',
    'platform.openai.com': 'mcp__openaiDeveloperDocs__search_openai_docs',
    'github.com': 'mcp__github__get_file_contents',
    'nuget.org': 'mcp__nuget__get_package_context',
};
const _FETCH_FALLBACK = 'search-web skill';
// The path of a repository wiki page, /<owner>/<repo>/wiki and anything under it
const _WIKI = /^\/[^/]+\/[^/]+\/wiki(?:\/|$)/u;

const FAMILIES: readonly Family[] = [
    ...['mcp__hostinger__VPS_recreate', 'mcp__hostinger__VPS_restore', 'mcp__hostinger__VPS_delete'].map(
        (prefix): Family => ({
            prefix,
            holds: (facts, id): boolean => facts.snapshots.has(id),
            reason: 'Snapshot the machine first with mcp__hostinger__VPS_createSnapshotV1',
        }),
    ),
    {
        prefix: 'mcp__hostinger__DNS_reset',
        holds: (facts, id): boolean => facts.dns.has(id),
        reason: 'Read the zone first with mcp__hostinger__DNS_getDNSRecordsV1',
    },
    ...['mcp__hostinger__domains_purchase', 'mcp__hostinger__billing_createPurchaseOrder', 'mcp__hostinger__VPS_purchase'].map(
        (prefix): Family => ({
            prefix,
            holds: (facts, id): boolean => id !== '' && facts.prompt.includes(id),
            reason: 'Prompt names no target for the purchase, ask the operator',
        }),
    ),
];

// One skill per server, its line injected once per session before the first call
const SERVERS: Readonly<Partial<Record<string, string>>> = {
    context7: 'search-code',
    deepwiki: 'search-code',
    nuget: 'search-code',
    'roslyn-codelens': 'dotnet-roslyn-codelens',
    hostinger: 'hostinger',
    binlog: 'dotnet-msbuild-diagnostics',
    'ast-grep': 'ast-grep',
};

// One line per tool that a fresh session calls wrong the first time, prepended to the tool's description
const DESCRIBE: Readonly<Partial<Record<ToolName, string>>> = {
    ['WebSearch']: 'Refused, use search-web skill',
    ['WebFetch']: 'Refused, use the docs and github tools for their hosts and search-web skill for other URLs',
};

// --- [OPERATIONS] ----------------------------------------------------------------------

// The declarations' own narrowing on the tool field
const isTool =
    <T extends ToolName>(tool: T): ((e: ToolCallInput) => e is Named<T>) =>
    (e: ToolCallInput): e is Named<T> =>
        e.tool === tool;

const _route = (url: string): string => {
    const parsed = URL.canParse(url) ? new URL(url) : undefined;
    const host = parsed?.hostname ?? '';
    // A GitHub wiki is its own repository, the contents API answers Not Found for it
    if (host.endsWith('github.com') && _WIKI.test(parsed?.pathname ?? '')) {
        return 'search-code skill';
    }
    return Object.entries(FETCH).find(([suffix]) => host.endsWith(suffix))?.[1] ?? _FETCH_FALLBACK;
};

// A row of one tool, each rule typed over that tool's event and lifted over the union, an event of another tool passes
const _row = <T extends ToolName>(tool: T, rules: Rules<T>): ToolRow => {
    const is = isTool(tool);
    return {
        ...rules,
        tool,
        deny: (e): string | undefined => (is(e) ? rules.deny?.(e) : undefined),
    };
};

// Rows over the tool names the declarations know
const TOOLS: readonly ToolRow[] = [
    _row('WebSearch', { deny: (): string => 'Use search-web skill' }),
    _row('WebFetch', { deny: (e): string => `Use ${_route(e.url)}` }),
    _row('mcp__playwright__browser_run_code_unsafe', { deny: (): string => 'Use the typed browser tools' }),
    _row('mcp__hostinger__VPS_createSnapshotV1', { records: 'snapshot' }),
    _row('mcp__hostinger__DNS_getDNSRecordsV1', { records: 'dns' }),
];

const _rows = (e: ToolCallInput): readonly ToolRow[] => TOOLS.filter((row) => row.tool === e.tool);

// The billing tool names its targets under items, the other purchases under item_id, the machine and domain tools by their id
const _id = (e: ToolCallInput): string => {
    const fields: Readonly<Record<string, unknown>> = e;
    const items = Array.isArray(fields['items']) ? fields['items'] : [];
    const first: unknown = items[0];
    return String(
        fields['virtualMachineId'] ??
            fields['domain'] ??
            fields['item_id'] ??
            (isRecord(first) && isString(first['item_id']) ? first['item_id'] : ''),
    );
};

const _serverSkill = (tool: string): string | undefined => SERVERS[_SERVER_NAME.exec(tool)?.groups?.['server'] ?? ''];

const _serverOnce = (tool: string): OnceLine | undefined => {
    const skill = _serverSkill(tool);
    return skill === undefined ? undefined : { key: skill, line: `Load the ${skill} skill` };
};

// The once lines the call raises, the server's skill line then the tool's own
const toolOnce = (e: ToolCallInput): readonly OnceLine[] => [_serverOnce(e.tool) ?? [], _rows(e).flatMap((row) => row.once ?? [])].flat();

const _familyReason = (e: ToolCallInput, facts: Facts): string | undefined => {
    const family = FAMILIES.find((candidate) => e.tool.startsWith(candidate.prefix));
    return family !== undefined && !family.holds(facts, _id(e)) ? family.reason : undefined;
};

// The table deny or the family requirement ends the rule, otherwise the call passes with its once and each lines
const toolRule =
    (facts: Facts): ((e: ToolCallInput) => Decision<ToolCallInput>) =>
    (e: ToolCallInput): Decision<ToolCallInput> => {
        const rows = _rows(e);
        const reason = rows.map((row) => row.deny?.(e)).find((found) => found !== undefined) ?? _familyReason(e, facts);
        if (reason !== undefined) {
            return deny(reason);
        }
        const once = toolOnce(e).filter((line) => line.key === undefined || !facts.seen.has(line.key));
        return rewrite(e, [...once.map((line) => line.line), ...rows.flatMap((row) => row.each ?? [])]);
    };

// The fact a successful call records, read by the hostinger families later in the session
const toolRecords = (e: ToolCallInput): Recorded | undefined => {
    const [namespace] = _rows(e).flatMap((row) => row.records ?? []);
    return namespace === undefined ? undefined : { namespace, id: _id(e) };
};

// The owning skill's line, then the tool's own row, one per line, undefined when the tool has neither
const describeLine = (tool: string): string | undefined => {
    const skill = _serverSkill(tool);
    const own = DESCRIBE[tool as ToolName];
    const text = [...(skill === undefined ? [] : [`Load the ${skill} skill first`]), ...(own === undefined ? [] : [own])].join('\n');
    return text === '' ? undefined : text;
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Facts, Family, Named, Recorded, ToolName, ToolRow };
export { describeLine, isTool, toolOnce, toolRecords, toolRule };
