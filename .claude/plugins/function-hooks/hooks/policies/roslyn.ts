// Roslyn diagnostics over an edited C# file, the lines the model reads after the edit

// --- [IMPORTS] -------------------------------------------------------------------------

import type { McpToolInputs, ToolCallInput } from 'claude-code';
import { flatMap, fromNullable, fromPredicate, getOrElse, liftPredicate, map, type Option, struct, toArray } from '../composition/option.ts';
import { decodeJson, isNumber, isString } from '../host/store.ts';
import { first } from '../text/lines.ts';
import { extension } from '../text/path.ts';
import type { Named } from './tools.ts';

// --- [TYPES] ---------------------------------------------------------------------------

// The adapter's view of McpToolResult, the text of the first text block of content, '' without one
interface Reply {
    readonly isError: boolean;
    readonly text: string;
}

// One item of the get_diagnostics envelope, file absolute and source compiler or analyzer:<id>
interface Diagnostic {
    readonly id: string;
    readonly severity: string;
    readonly message: string;
    readonly file: string;
    readonly line: number;
    readonly project: string;
    readonly source: string;
}

// The counts per severity, unreliable present when the solution loaded degraded
interface Summary {
    readonly error: number;
    readonly warning: number;
    readonly info: number;
    readonly hidden: number;
    readonly unreliable?: unknown;
}

interface Envelope {
    readonly items: readonly Diagnostic[];
    readonly summary: Summary;
}

// One ancestor directory of the edited file with the names $.fs.listDir answered
interface Listing {
    readonly dir: string;
    readonly names: readonly string[];
}

// One diagnostic id the server reports wrongly with the reason, the plugin README states each row's retirement
interface WrongDiagnostic {
    readonly id: string;
    readonly reason: string;
}

// The recovery a first reply asks for, one call then one re-read, none for a read reply
type ReplyClass = 'untrusted' | 'unreliable' | 'read';

// The value each line class carries, the reply text for a failure or an unparsed text and the envelope for a read
interface _LineValues {
    readonly failed: string;
    readonly unparsed: string;
    readonly read: Envelope;
}

type _LineClass = keyof _LineValues;

// The class decision with its value, the mapped form keeps the class and the value paired under a generic key
type _Lined<K extends _LineClass = _LineClass> = { readonly [P in K]: { readonly class: P; readonly value: _LineValues[P] } }[K];

// The envelope as read, every field kept beside the items and the summary so a rebuild keeps totalCount, truncated, and limit
interface _Shape {
    readonly [field: string]: unknown;
    readonly items: readonly unknown[];
    readonly summary: Summary;
}

// The envelope text less the WRONG_DIAGNOSTICS items, with the id of each dropped item
interface Filtered {
    readonly text: string;
    readonly dropped: readonly string[];
}

type _Severity = keyof Omit<Summary, 'unreliable'>;

// The server's tool names as the declarations hold them, and the name after the server prefix
type _RoslynTool = Extract<keyof McpToolInputs, `mcp__${typeof SERVER}__${string}`>;
type _RoslynName = _RoslynTool extends `mcp__${typeof SERVER}__${infer N}` ? N : never;

// Every server tool that answers from the compilation, the reads the watcher window delays
type RoslynRead = Exclude<_RoslynTool, `mcp__${typeof SERVER}__${(typeof _CONTROL)[number]}`>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const SERVER = 'roslyn-codelens';
const SOLUTION = 'Workspace.slnx';
const _PREVIEW = 120;
const _CSPROJ = '.csproj';
const _DEGRADED = 'Roslyn: the solution loaded degraded, results can name errors no build reports';
// The error code the skill maps to one trust_solution call
const _UNTRUSTED = /SolutionNotTrusted/u;
// The server applies a .cs write to its compilation 200 ms after the watcher event (RoslynCodeLens FileChangeTracker.cs:251, a one-shot
// Timer with 200 ms) and every tool reads the stale compilation inside that window, the probe's first always-fresh read came at 206 ms
const WATCHER_SETTLE_MS = 206;
// The diagnostics the lines and the filtered reply drop, each reason the tail of the reply's context line
const WRONG_DIAGNOSTICS = [
    {
        id: 'IDE0055',
        reason: 'the server formats under Roslyn defaults until a release attaches project.AnalyzerOptions (README known issues row 02)',
    },
] as const satisfies readonly WrongDiagnostic[];
// The server tools that manage solutions, trust, and tasks in place of reading the compilation, a name the declarations lack fails tsc
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
] as const satisfies readonly _RoslynName[];
const _READ_ROWS = [
    'mcp__roslyn-codelens__analyze_change_impact',
    'mcp__roslyn-codelens__analyze_control_flow',
    'mcp__roslyn-codelens__analyze_data_flow',
    'mcp__roslyn-codelens__analyze_method',
    'mcp__roslyn-codelens__apply_code_action',
    'mcp__roslyn-codelens__change_signature',
    'mcp__roslyn-codelens__check_architecture',
    'mcp__roslyn-codelens__find_async_violations',
    'mcp__roslyn-codelens__find_attribute_usages',
    'mcp__roslyn-codelens__find_breaking_changes',
    'mcp__roslyn-codelens__find_callers',
    'mcp__roslyn-codelens__find_catch_blocks',
    'mcp__roslyn-codelens__find_circular_dependencies',
    'mcp__roslyn-codelens__find_disposable_misuse',
    'mcp__roslyn-codelens__find_event_subscribers',
    'mcp__roslyn-codelens__find_god_objects',
    'mcp__roslyn-codelens__find_implementations',
    'mcp__roslyn-codelens__find_large_classes',
    'mcp__roslyn-codelens__find_naming_violations',
    'mcp__roslyn-codelens__find_obsolete_usage',
    'mcp__roslyn-codelens__find_references',
    'mcp__roslyn-codelens__find_reflection_usage',
    'mcp__roslyn-codelens__find_tests_for_symbol',
    'mcp__roslyn-codelens__find_throw_sites',
    'mcp__roslyn-codelens__find_uncovered_symbols',
    'mcp__roslyn-codelens__find_unused_symbols',
    'mcp__roslyn-codelens__generate_test_skeleton',
    'mcp__roslyn-codelens__get_call_graph',
    'mcp__roslyn-codelens__get_code_actions',
    'mcp__roslyn-codelens__get_code_fixes',
    'mcp__roslyn-codelens__get_complexity_metrics',
    'mcp__roslyn-codelens__get_di_registrations',
    'mcp__roslyn-codelens__get_diagnostics',
    'mcp__roslyn-codelens__get_exception_flow',
    'mcp__roslyn-codelens__get_extension_methods',
    'mcp__roslyn-codelens__get_file_overview',
    'mcp__roslyn-codelens__get_generated_code',
    'mcp__roslyn-codelens__get_instantiation_options',
    'mcp__roslyn-codelens__get_method_source',
    'mcp__roslyn-codelens__get_nuget_dependencies',
    'mcp__roslyn-codelens__get_operators',
    'mcp__roslyn-codelens__get_overloads',
    'mcp__roslyn-codelens__get_project_dependencies',
    'mcp__roslyn-codelens__get_project_health',
    'mcp__roslyn-codelens__get_public_api_surface',
    'mcp__roslyn-codelens__get_source_generators',
    'mcp__roslyn-codelens__get_symbol_context',
    'mcp__roslyn-codelens__get_test_summary',
    'mcp__roslyn-codelens__get_type_hierarchy',
    'mcp__roslyn-codelens__get_type_overview',
    'mcp__roslyn-codelens__go_to_definition',
    'mcp__roslyn-codelens__inspect_external_assembly',
    'mcp__roslyn-codelens__peek_il',
    'mcp__roslyn-codelens__rename_symbol',
    'mcp__roslyn-codelens__resolve_stack_trace',
    'mcp__roslyn-codelens__search_symbols',
] as const satisfies readonly RoslynRead[];

// A read the declarations gain and the table lacks names itself in _Unlisted, and the satisfies on ROSLYN_READS then fails tsc
type _Unlisted = Exclude<RoslynRead, (typeof _READ_ROWS)[number]>;
const ROSLYN_READS: readonly RoslynRead[] = _READ_ROWS satisfies [_Unlisted] extends [never] ? readonly RoslynRead[] : never;

// --- [OPERATIONS] ----------------------------------------------------------------------

const _isDiagnostic: (value: unknown) => value is Diagnostic = struct({
    id: isString,
    severity: isString,
    message: isString,
    file: isString,
    line: isNumber,
    project: isString,
    source: isString,
});

const _isSummary: (value: unknown) => value is Summary = struct({ error: isNumber, warning: isNumber, info: isNumber, hidden: isNumber });

const _isShape: (value: unknown) => value is _Shape = struct({ items: Array.isArray, summary: _isSummary });

// The envelope of a get_diagnostics text, an item missing a field drops alone and a text without items or summary reads as none
const decodeEnvelope = (text: string): Option<Envelope> =>
    map(
        (shape: _Shape): Envelope => ({
            items: shape.items.flatMap((item) => toArray(fromPredicate(_isDiagnostic)(item))),
            summary: shape.summary,
        }),
    )(flatMap(fromPredicate(_isShape))(decodeJson(text)));

const _isWrong = (item: Diagnostic): boolean => WRONG_DIAGNOSTICS.some((row) => row.id === item.id);

const _isDropped = (item: unknown): boolean => _isDiagnostic(item) && _isWrong(item);

// The server spells an item's severity capitalized (Error) and the summary's keys lower (error), proof session e307c2b3
const _count = (dropped: readonly Diagnostic[], severity: _Severity): number =>
    dropped.filter((item) => item.severity.toLowerCase() === severity).length;

// The summary less the dropped items under each severity it counts, a severity outside the summary counts nothing
const _lessDropped = (summary: Summary, dropped: readonly Diagnostic[]): Summary => ({
    ...summary,
    error: summary.error - _count(dropped, 'error'),
    warning: summary.warning - _count(dropped, 'warning'),
    info: summary.info - _count(dropped, 'info'),
    hidden: summary.hidden - _count(dropped, 'hidden'),
});

// The envelope rebuilt without the wrong items, totalCount and the summary reduced by the drops and every other field as read
const _filtered = (shape: _Shape): Option<Filtered> => {
    const dropped = shape.items.flatMap((item) => toArray(fromPredicate(_isDiagnostic)(item))).filter(_isWrong);
    return liftPredicate<Filtered>((filtered) => filtered.dropped.length > 0)({
        text: JSON.stringify({
            ...shape,
            items: shape.items.filter((item) => !_isDropped(item)),
            totalCount: getOrElse(() => shape.totalCount)(map((count: number) => count - dropped.length)(fromPredicate(isNumber)(shape.totalCount))),
            summary: _lessDropped(shape.summary, dropped),
        }),
        dropped: dropped.map((item) => item.id),
    });
};

// The reply text of a by-hand get_diagnostics less the WRONG_DIAGNOSTICS items, none when no envelope decodes or nothing drops
const filterEnvelope = (text: string): Option<Filtered> => flatMap(_filtered)(flatMap(fromPredicate(_isShape))(decodeJson(text)));

// One context line per wrong id the filtered reply dropped, with the count and the row's reason
const droppedLines = (dropped: readonly string[]): readonly string[] =>
    WRONG_DIAGNOSTICS.flatMap((row) =>
        toArray(
            map((count: number) => `Dropped ${count} ${row.id} items, ${row.reason}`)(
                liftPredicate<number>((count) => count > 0)(dropped.filter((id) => id === row.id).length),
            ),
        ),
    );

// The milliseconds left of the watcher window since the last .cs write, 0 with no stamp or once the window passed
const settleWait = (stampAt: Option<number>, now: number): number =>
    getOrElse(() => 0)(map((at: number) => Math.max(0, WATCHER_SETTLE_MS - (now - at)))(stampAt));

// The context line of a by-hand read the hook delayed by the rest of the window
const settleLine = (ms: number): string =>
    `Waited ${ms} ms for the server to apply the last .cs write, a read inside the 200 ms debounce answers the previous compilation`;

// The declarations' narrowing to a server tool that answers from the compilation
const isRoslynRead = (e: ToolCallInput): e is Named<RoslynRead> => ROSLYN_READS.some((tool) => tool === e.tool);

// The stem of the first .csproj in the listings, the file's own directory first
const projectOf = (listings: readonly Listing[]): Option<string> =>
    map((name: string) => name.slice(0, -_CSPROJ.length))(
        fromNullable(listings.flatMap((listing) => listing.names.filter((name) => extension(name) === _CSPROJ))[0]),
    );

const _directory = (path: string): string => path.slice(0, Math.max(path.lastIndexOf('/'), 0));

// Every prefix of a directory path, the root first
const _prefixes = (directory: string): readonly string[] =>
    directory.split('/').map((_segment, index, segments) => segments.slice(0, index + 1).join('/'));

// The directories from the file's own up to the working directory, nearest first, a path outside it answers its own directory alone
const ancestors = (cwd: string, path: string): readonly string[] => {
    const own = _directory(path);
    return getOrElse((): readonly string[] => [own])(
        liftPredicate<readonly string[]>((chain) => chain.length > 0)(
            _prefixes(own)
                .filter((directory) => directory === cwd || directory.startsWith(`${cwd}/`))
                .toReversed(),
        ),
    );
};

// The server's limit default of 1000 keeps the edited file's items under a substring project filter
const diagnosticsRequest = (project: string): McpToolInputs['mcp__roslyn-codelens__get_diagnostics'] => ({
    project,
    severity: 'error',
    includeAnalyzers: true,
});

// The scope default is session, the trust lasts as long as the server
const trustRequest = (solutionPath: string): McpToolInputs['mcp__roslyn-codelens__trust_solution'] => ({ path: solutionPath });

const _unreliable = (envelope: Envelope): boolean => envelope.summary.unreliable !== undefined;

// Error replies naming the trust code ask for trust_solution, a degraded summary for rebuild_solution, the rest is read
const replyClass = (reply: Reply): ReplyClass =>
    (reply.isError && _UNTRUSTED.test(reply.text) && 'untrusted') ||
    (getOrElse(() => false)(map(_unreliable)(decodeEnvelope(reply.text))) && 'unreliable') ||
    'read';

// An error reply is failed, a text with no envelope unparsed, and the rest read with the envelope
const _lined = (reply: Reply): _Lined =>
    (reply.isError && { class: 'failed', value: reply.text }) ||
    getOrElse((): _Lined => ({ class: 'unparsed', value: reply.text }))(
        map((envelope: Envelope): _Lined => ({ class: 'read', value: envelope }))(decodeEnvelope(reply.text)),
    );

const _underFile =
    (file: string): ((item: Diagnostic) => boolean) =>
    (item: Diagnostic): boolean =>
        item.file === file || item.file.endsWith(`/${file}`);

// The items under the edited file less the wrong rows by line, one line when none, and the degraded line when the summary says so
const _envelopeLines = (envelope: Envelope, file: string, project: string): readonly string[] => [
    ...getOrElse((): readonly string[] => [`Roslyn: no error in ${file} (${project})`])(
        liftPredicate<readonly string[]>((lines) => lines.length > 0)(
            envelope.items
                .filter((item) => !_isWrong(item))
                .filter(_underFile(file))
                .toSorted((left, right) => left.line - right.line)
                .map((item) => `${file}:${item.line} ${item.id}: ${item.message}`),
        ),
    ),
    ...toArray(liftPredicate<string>(() => _unreliable(envelope))(_DEGRADED)),
];

const _LINES: { readonly [K in _LineClass]: (value: _LineValues[K], file: string, project: string) => readonly string[] } = {
    failed: (text): readonly string[] => [`Roslyn get_diagnostics failed: ${first(text)}`],
    unparsed: (text): readonly string[] => [`Roslyn get_diagnostics answered no envelope: ${text.slice(0, _PREVIEW)}`],
    read: _envelopeLines,
};

// The lines of one class, the generic key correlates the record member with the value it receives
const _lines = <K extends _LineClass>(lined: _Lined<K>, file: string, project: string): readonly string[] =>
    _LINES[lined.class](lined.value, file, project);

// The context lines of the final reply, file is the edited path relative to the working directory
const diagnosticLines = (reply: Reply, file: string, project: string): readonly string[] => _lines(_lined(reply), file, project);

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Diagnostic, Envelope, Filtered, Listing, Reply, ReplyClass, RoslynRead, Summary, WrongDiagnostic };
export {
    ancestors,
    decodeEnvelope,
    diagnosticLines,
    diagnosticsRequest,
    droppedLines,
    filterEnvelope,
    isRoslynRead,
    projectOf,
    ROSLYN_READS,
    replyClass,
    SERVER,
    SOLUTION,
    settleLine,
    settleWait,
    trustRequest,
    WATCHER_SETTLE_MS,
    WRONG_DIAGNOSTICS,
};
