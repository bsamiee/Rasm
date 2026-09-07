// Tool and server rows typed over the generated declarations for the tool.call and tool.describe events

// --- [IMPORTS] -------------------------------------------------------------------------

import type { BuiltinToolName, McpToolInputs, ToolCallInput, ToolDescribeInput, ToolDescribeResult } from 'claude-code';
import { absurd, answer, type Decision, deny, type Rule, rewrite, when } from '../composition/decision.ts';
import {
    flatMap,
    fromBoolean,
    fromNullable,
    fromPredicate,
    getOrElse,
    isRecord,
    liftPredicate,
    map,
    none,
    type Option,
    some,
    toArray,
} from '../composition/option.ts';
import { isString } from '../host/store.ts';
import { basename } from '../text/path.ts';
import type { Once } from './paths.ts';
import { SOLUTION } from './roslyn.ts';

// --- [TYPES] ---------------------------------------------------------------------------

// The server names the generated declarations hold, one per mcp__<server>__ prefix
type _ServerOf<K> = K extends `mcp__${infer S}__${string}` ? S : never;
type Server = _ServerOf<keyof McpToolInputs>;

type Requirement = 'vm-snapshot' | 'dns-read' | 'operator-named-target';
type Recording = 'vm-snapshot' | 'dns-read';

// The event as the id and once reads see it, any tool with its arguments unknown
interface ToolEvent {
    readonly tool: string;
    readonly [argument: string]: unknown;
}

type ToolName = keyof McpToolInputs | BuiltinToolName;

// The event of one tool as the declarations narrow it on the tool field, its arguments typed
type Named<T extends ToolName> = Extract<ToolCallInput, { readonly tool: T }>;

// The event a rewrite row changed with the line naming the change
interface Rewritten {
    readonly e: ToolCallInput;
    readonly context: string;
}

// The event after the rewrite rows of its tool with the line of each row that applied
interface Applied {
    readonly e: ToolCallInput;
    readonly context: readonly string[];
}

// Deny and rewrite rules run over the tool's own event, lifted by _refused and _rewritten, and read its typed fields
interface ToolRow {
    readonly deny?: Rule<ToolCallInput, never, string>;
    readonly rewrite?: (e: ToolCallInput, facts: Facts) => Option<Rewritten>;
    readonly once?: Once;
    readonly each?: string;
    readonly records?: Recording;
}

interface ToolEntry extends ToolRow {
    readonly tool: ToolName;
}

interface Family {
    readonly prefix: string;
    readonly requires: Requirement;
}

interface Route {
    readonly host: string;
    readonly route: string;
}

interface ServerRow {
    readonly skill: string;
    readonly line: string;
}

interface Facts {
    readonly seen: ReadonlySet<string>;
    readonly snapshots: ReadonlySet<string>;
    readonly dns: ReadonlySet<string>;
    readonly prompt: string;
    readonly cwd: string;
}

interface Recorded {
    readonly kind: Recording;
    readonly id: string;
}

// The line prepended to one tool's description, a row over a name the declarations lack fails tsc
interface DescribeRow {
    readonly tool: ToolName;
    readonly line: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _FETCH_FALLBACK = 'tvly extract or mcp__exa__web_fetch_exa';
const _SERVER_NAME = /^mcp__(?<server>.+?)__/u;

const FETCH: readonly Route[] = [
    { host: 'code.claude.com', route: 'mcp__claudeCodeDocs__search_claude_code_docs' },
    { host: 'docs.anthropic.com', route: 'mcp__claudeCodeDocs__search_claude_code_docs' },
    { host: 'platform.openai.com', route: 'mcp__openaiDeveloperDocs__search_openai_docs' },
    { host: 'github.com', route: 'mcp__github__get_file_contents' },
    { host: 'nuget.org', route: 'mcp__nuget__get_package_context' },
];

const _FAMILY_ROWS = [
    { prefix: 'mcp__hostinger__VPS_recreate', requires: 'vm-snapshot' },
    { prefix: 'mcp__hostinger__VPS_restore', requires: 'vm-snapshot' },
    { prefix: 'mcp__hostinger__VPS_delete', requires: 'vm-snapshot' },
    { prefix: 'mcp__hostinger__DNS_reset', requires: 'dns-read' },
    { prefix: 'mcp__hostinger__domains_purchase', requires: 'operator-named-target' },
    { prefix: 'mcp__hostinger__billing_createPurchaseOrder', requires: 'operator-named-target' },
    { prefix: 'mcp__hostinger__VPS_purchase', requires: 'operator-named-target' },
] as const satisfies readonly Family[];

// Families that vanish from the generated declarations name themselves in _Missing, and the satisfies on FAMILIES then fails tsc
type _Prefix = (typeof _FAMILY_ROWS)[number]['prefix'];
type _Missing = { [P in _Prefix]: [Extract<keyof McpToolInputs, `${P}${string}`>] extends [never] ? P : never }[_Prefix];
const FAMILIES: readonly Family[] = _FAMILY_ROWS satisfies [_Missing] extends [never] ? readonly Family[] : never;

const _REQUIREMENT: Readonly<Record<Requirement, { readonly holds: (facts: Facts, id: string) => boolean; readonly reason: string }>> = {
    'vm-snapshot': {
        holds: (facts, id): boolean => facts.snapshots.has(id),
        reason: 'Call mcp__hostinger__VPS_createSnapshotV1 on the machine first, then retry',
    },
    'dns-read': {
        holds: (facts, id): boolean => facts.dns.has(id),
        reason: 'Call mcp__hostinger__DNS_getDNSRecordsV1 on the domain first, then retry',
    },
    'operator-named-target': {
        holds: (facts, id): boolean => id !== '' && facts.prompt.includes(id),
        reason: 'The current prompt names no target, the operator names it before a purchase',
    },
};

const _route = (url: string): string =>
    getOrElse(() => _FETCH_FALLBACK)(
        flatMap(() => fromNullable(FETCH.find((route) => new URL(url).hostname.endsWith(route.host))?.route))(fromBoolean(URL.canParse(url))),
    );

// The declarations' own narrowing on the tool field, the refinement when lifts a typed rule over, one tool name per refinement
const isTool =
    <T extends ToolName>(tool: T): ((e: ToolCallInput) => e is Named<T>) =>
    (e: ToolCallInput): e is Named<T> =>
        e.tool === tool;

// Deny rows whose reason holds under the fields the declarations give the tool, a none passes the call
const _refusedWhen = <T extends ToolName>(tool: T, reason: (e: Named<T>) => Option<string>): ToolEntry => ({
    tool,
    deny: when(
        isTool(tool),
        (e: Named<T>): Decision<Named<T>, never, string> =>
            reason(e).match<Decision<Named<T>, never, string>>({ some: deny, none: () => rewrite(e, []) }),
    ),
});

// Deny rows with a reason over every call of the tool, WebFetch's url a string
const _refused = <T extends ToolName>(tool: T, reason: (e: Named<T>) => string): ToolEntry => _refusedWhen(tool, (e) => some(reason(e)));

// Rewrite rows over the fields the declarations give the tool, a none leaves the event as the row before it produced
const _rewritten = <T extends ToolName>(tool: T, change: (e: Named<T>, facts: Facts) => Option<Rewritten>): ToolEntry => ({
    tool,
    rewrite: (e, facts) => flatMap((named: Named<T>) => change(named, facts))(fromPredicate(isTool(tool))(e)),
});

// The two search tools, find_code takes the language as a field and find_code_by_rule on the language line of its yaml
type Searched = Named<'mcp__ast-grep__find_code' | 'mcp__ast-grep__find_code_by_rule'>;

const _TSX_LINE = 'Ran with language tsx, sgconfig.yml languageGlobs maps every .ts file to tsx and typescript finds nothing';
const _FOLDER_LINE = 'Resolved project_folder against the working directory, the server reads an absolute path';
const _YAML_TYPESCRIPT = /^(?<key>\s*language:\s*)typescript\s*$/mu;

const _tsxField = (e: Named<'mcp__ast-grep__find_code'>): Option<Rewritten> =>
    liftPredicate<Rewritten>(() => e.language === 'typescript')({ e: { ...e, language: 'tsx' }, context: _TSX_LINE });

const _tsxYaml = (e: Named<'mcp__ast-grep__find_code_by_rule'>): Option<Rewritten> =>
    liftPredicate<Rewritten>(() => _YAML_TYPESCRIPT.test(e.yaml))({
        e: { ...e, yaml: e.yaml.replace(_YAML_TYPESCRIPT, '$<key>tsx') },
        context: _TSX_LINE,
    });

const _absoluteFolder = (e: Searched, facts: Facts): Option<Rewritten> =>
    liftPredicate<Rewritten>(() => !e.project_folder.startsWith('/'))({
        e: { ...e, ['project_folder']: `${facts.cwd}/${e.project_folder}` },
        context: _FOLDER_LINE,
    });

// The server trusts every solution on its command line for the session, and .mcp.json names the workspace solution there
const _TRUST_DENY = `trust_solution on ${SOLUTION} is a wasted call, the server trusts every solution on its command line for the session, call get_diagnostics with includeAnalyzers: true directly`;

// Rows over the tool names the declarations know, a tool's rewrite rows run in table order and each reads the event the one before produced
const TOOLS: readonly ToolEntry[] = [
    _refused('WebSearch', (): string => 'WebSearch is refused, search with mcp__exa__web_search_exa or tvly search'),
    _refused('WebFetch', (e): string => `WebFetch is refused, ${_route(e.url)}`),
    _refused(
        'mcp__playwright__browser_run_code_unsafe',
        (): string => 'browser_run_code_unsafe runs arbitrary code in the page, use the typed browser tools',
    ),
    _refusedWhen(
        'mcp__roslyn-codelens__trust_solution',
        (e): Option<string> => liftPredicate<string>(() => basename(e.path) === SOLUTION)(_TRUST_DENY),
    ),
    { tool: 'mcp__hostinger__VPS_createSnapshotV1', records: 'vm-snapshot' },
    { tool: 'mcp__hostinger__DNS_getDNSRecordsV1', records: 'dns-read' },
    _rewritten('mcp__ast-grep__find_code', _tsxField),
    _rewritten('mcp__ast-grep__find_code', _absoluteFolder),
    _rewritten('mcp__ast-grep__find_code_by_rule', _tsxYaml),
    _rewritten('mcp__ast-grep__find_code_by_rule', _absoluteFolder),
];

const SERVERS: Readonly<Partial<Record<Server, ServerRow>>> = {
    context7: { skill: 'search-context7', line: 'Load the search-context7 skill, it caps each query' },
    'roslyn-codelens': { skill: 'dotnet-roslyn-codelens', line: 'Load the dotnet-roslyn-codelens skill' },
    hostinger: { skill: 'hostinger', line: 'Load the hostinger skill' },
    binlog: { skill: 'dotnet-msbuild-diagnostics', line: 'Load the dotnet-msbuild-diagnostics skill for the binlog tools' },
    nuget: { skill: 'dotnet-msbuild-packaging', line: 'Load the dotnet-msbuild-packaging skill for the nuget tools' },
    'ast-grep': { skill: 'ast-grep', line: 'Load the ast-grep skill for the ast-grep tools' },
};

// One row per tool that a fresh session calls wrong the first time, the ast-grep rows from the facts the skill's search section holds
const DESCRIBE = [
    { tool: 'WebSearch', line: 'WebSearch is refused, mcp__exa__web_search_exa and tvly search replace it' },
    { tool: 'WebFetch', line: 'WebFetch is refused, the docs and github tools, tvly extract, and mcp__exa__web_fetch_exa replace it' },
    {
        tool: 'mcp__roslyn-codelens__trust_solution',
        line: `The command-line solution ${SOLUTION} is trusted for the session, trust_solution serves another solution alone`,
    },
    {
        tool: 'mcp__ast-grep__dump_syntax_tree',
        line: 'format=cst shows the node kinds and field names a rule reads, and format=pattern shows how a pattern parses',
    },
    {
        tool: 'mcp__ast-grep__find_code',
        line: 'The plugin rewrites language typescript to tsx and a relative project_folder to an absolute one',
    },
    {
        tool: 'mcp__ast-grep__find_code_by_rule',
        line: 'The plugin rewrites language typescript to tsx and a relative project_folder to an absolute one, inline rules bind local utils alone, and ast-grep scan -c <config> proves a global util',
    },
    {
        tool: 'mcp__ast-grep__test_match_code_rule',
        line: "No match, a rejected rule, and an error diagnostic read as one failure, and printf '<code>' | ast-grep scan --inline-rules '<yaml>' --json --stdin; echo $? separates them by exit code 0, 8, and 1",
    },
] as const satisfies readonly DescribeRow[];

// --- [OPERATIONS] ----------------------------------------------------------------------

const _isKnownServer = (name: string): name is Server => Object.hasOwn(SERVERS, name);

// The server of an MCP tool name when the server table holds it
const server = (tool: string): Option<Server> => flatMap(fromPredicate(_isKnownServer))(fromNullable(_SERVER_NAME.exec(tool)?.groups?.server));

// The billing tool names its targets under items, the other purchases under item_id, the machine and domain tools by their id
type _Items = McpToolInputs['mcp__hostinger__billing_createPurchaseOrderV1']['items'];

const _isItems = (value: unknown): value is _Items => Array.isArray(value) && value.every((item) => isRecord(item) && isString(item.item_id));

const _id = (e: ToolEvent): string =>
    String(
        e.virtualMachineId ??
            e.domain ??
            e.item_id ??
            getOrElse(() => '')(map((items: _Items): string => items[0]?.item_id ?? '')(fromPredicate(_isItems)(e.items))),
    );

const _family = (tool: string): Option<Family> => fromNullable(FAMILIES.find((family) => tool.startsWith(family.prefix)));

const _rows = (tool: string): readonly ToolEntry[] => TOOLS.filter((row) => row.tool === tool);

// The first row of the tool, the one its deny, once, each, and records fields sit on
const _row = (tool: string): Option<ToolEntry> => fromNullable(_rows(tool)[0]);

const _serverRow = (tool: string): Option<ServerRow> => flatMap((name: Server) => fromNullable(SERVERS[name]))(server(tool));

const _serverOnce = (tool: string): Option<Once> => map((row: ServerRow): Once => ({ key: row.skill, line: row.line }))(_serverRow(tool));

const _onces = (tool: string): readonly Once[] => [
    ...toArray(_serverOnce(tool)),
    ...toArray(flatMap((row: ToolRow) => fromNullable(row.once))(_row(tool))),
];

const _requirementReason = (e: ToolEvent, facts: Facts): Option<string> =>
    flatMap((family: Family) =>
        liftPredicate<string>(() => !_REQUIREMENT[family.requires].holds(facts, _id(e)))(_REQUIREMENT[family.requires].reason),
    )(_family(e.tool));

// The row's deny rule run over the event, its decision read as the reason or none
const _rowReason = (e: ToolCallInput): Option<string> =>
    flatMap((rule: Rule<ToolCallInput, never, string>) =>
        rule(e).match<Option<string>>({ deny: some, rewrite: () => none<string>(), answer: absurd }),
    )(flatMap((row: ToolRow) => fromNullable(row.deny))(_row(e.tool)));

const _denyReason = (e: ToolCallInput, facts: Facts): Option<string> =>
    _rowReason(e).match<Option<string>>({ some, none: () => _requirementReason(e, facts) });

const _context = (e: ToolEvent, facts: Facts): readonly string[] => [
    ..._onces(e.tool)
        .filter((once) => !facts.seen.has(once.key))
        .map((once) => once.line),
    ...toArray(flatMap((row: ToolRow) => fromNullable(row.each))(_row(e.tool))),
];

// The tool's rewrite rows folded in table order, a row that applies feeds the next its event and adds its line, a none keeps the state
const _applied = (e: ToolCallInput, facts: Facts): Applied =>
    _rows(e.tool).reduce<Applied>(
        (state, row) =>
            flatMap((change: NonNullable<ToolRow['rewrite']>) => change(state.e, facts))(fromNullable(row.rewrite)).match<Applied>({
                some: (next) => ({ e: next.e, context: [...state.context, next.context] }),
                none: () => state,
            }),
        { e, context: [] },
    );

// The rewritten event with the rewrite lines, then the once and each lines
const _pass = (applied: Applied, facts: Facts): Decision<ToolCallInput, unknown, string> =>
    rewrite(applied.e, [...applied.context, ..._context(applied.e, facts)]);

// The table deny or the family requirement ends the rule, otherwise the rewrite rows run and their lines join the once and each lines
const toolRule =
    (facts: Facts): Rule<ToolCallInput, unknown, string> =>
    (e: ToolCallInput): Decision<ToolCallInput, unknown, string> =>
        _denyReason(e, facts).match<Decision<ToolCallInput, unknown, string>>({
            some: deny,
            none: () => _pass(_applied(e, facts), facts),
        });

// The fact a successful call records, read by the hostinger families later in the session
const toolRecords = (e: ToolEvent): Option<Recorded> =>
    flatMap((row: ToolRow) => map((kind: Recording): Recorded => ({ kind, id: _id(e) }))(fromNullable(row.records)))(_row(e.tool));

// The keys the adapter stamps as injected once the call ran
const toolSkills = (e: ToolEvent): readonly string[] => _onces(e.tool).map((once) => once.key);

// The owning skill's line, then the tool's own row, one per line, none when the tool has neither
const _describeLine = (tool: string): Option<string> =>
    liftPredicate<string>((line) => line !== '')(
        [
            ...toArray(map((row: ServerRow) => `Load the ${row.skill} skill before the first call`)(_serverRow(tool))),
            ...toArray(map((row: DescribeRow) => row.line)(fromNullable(DESCRIBE.find((row) => row.tool === tool)))),
        ].join('\n'),
    );

// Reads the tables alone, the per-session description cache then holds with no invalidate
const describeRule = <E extends ToolDescribeInput>(e: E): Decision<E, ToolDescribeResult, never> =>
    _describeLine(e.tool).match<Decision<E, ToolDescribeResult, never>>({
        some: (line) => answer({ description: `${line}\n${e.description}` }),
        none: () => rewrite(e, []),
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export type { DescribeRow, Facts, Family, Named, Recorded, Route, Server, ServerRow, ToolEntry, ToolEvent, ToolName, ToolRow };
export { DESCRIBE, describeRule, FAMILIES, FETCH, isTool, SERVERS, TOOLS, toolRecords, toolRule, toolSkills };
