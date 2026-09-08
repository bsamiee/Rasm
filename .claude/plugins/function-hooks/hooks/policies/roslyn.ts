// Roslyn diagnostics over an edited C# file, the lines the model reads after the edit

// --- [IMPORTS] -------------------------------------------------------------------------

import type { McpToolInputs } from 'claude-code';
import { decodeJson, isNumber, isRecord, isString, struct } from '../host/store.ts';
import { first } from '../text/lines.ts';
import { extension } from '../text/path.ts';

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
}

// The envelope of a get_diagnostics reply, unreliable present in the summary when the solution loaded degraded
interface Envelope {
    readonly items: readonly Diagnostic[];
    readonly unreliable: boolean;
}

// One ancestor directory of the edited file with the names $.fs.listDir answered
interface Listing {
    readonly dir: string;
    readonly names: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SERVER = 'roslyn-codelens';
const SOLUTION = 'Workspace.slnx';
const _PREFIX = `mcp__${SERVER}__`;
const _PREVIEW = 120;
const _CSPROJ = '.csproj';
const _DEGRADED = 'Roslyn: the solution loaded degraded, results can name errors no build reports';
// The error code the skill maps to one trust_solution call
const _UNTRUSTED = /SolutionNotTrusted/u;
// The server applies a .cs write to its compilation 200 ms after the watcher event (RoslynCodeLens FileChangeTracker.cs, a one-shot Timer),
// a read inside that window answers the previous compilation, and the margin covers the timer's own scheduling
const WATCHER_SETTLE_MS = 206;
// The diagnostic ids the server reports wrongly with the reason, the plugin README states each row's retirement
const WRONG_DIAGNOSTICS: Readonly<Record<string, string>> = {
    ['IDE0055']: 'the server formats under Roslyn defaults until a release attaches project.AnalyzerOptions (README known issues row 02)',
};
// The server tools that manage solutions, trust, and tasks in place of reading the compilation, every other tool reads it
const _CONTROL: readonly string[] = [
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
];

const _isDiagnostic = struct({ id: isString, severity: isString, message: isString, file: isString, line: isNumber });

const _isEnvelope = struct({ items: Array.isArray, summary: isRecord });

// --- [OPERATIONS] ----------------------------------------------------------------------

// The envelope of a get_diagnostics text, an item missing a field drops alone and a text without items or summary reads as undefined
const decodeEnvelope = (text: string): Envelope | undefined => {
    const value = decodeJson(text);
    if (!_isEnvelope(value)) {
        return undefined;
    }
    return { items: value.items.filter(_isDiagnostic), unreliable: value.summary.unreliable !== undefined };
};

// The wrong diagnostics a reply holds by id, one line per id the model reads beside the reply
const wrongLines = (envelope: Envelope): readonly string[] =>
    Object.entries(WRONG_DIAGNOSTICS).flatMap(([id, reason]) => {
        const count = envelope.items.filter((item) => item.id === id).length;
        return count > 0 ? [`Ignore the ${count} ${id} items, ${reason}`] : [];
    });

// The milliseconds left of the watcher window since the last .cs write, 0 with no stamp or once the window passed
const settleWait = (stampAt: number | undefined, now: number): number =>
    stampAt === undefined ? 0 : Math.max(0, WATCHER_SETTLE_MS - (now - stampAt));

const settleLine = (ms: number): string =>
    `Waited ${ms} ms for the server to apply the last .cs write, a read inside the 200 ms debounce answers the previous compilation`;

// Whether a tool name is a server tool that answers from the compilation
const isRoslynRead = (tool: string): boolean => tool.startsWith(_PREFIX) && !_CONTROL.includes(tool.slice(_PREFIX.length));

// The stem of the first .csproj in the listings, the file's own directory first
const projectOf = (listings: readonly Listing[]): string | undefined =>
    listings.flatMap((listing) => listing.names.filter((name) => extension(name) === _CSPROJ))[0]?.slice(0, -_CSPROJ.length);

// The directories from the file's own up to the working directory, nearest first, a path outside it answers its own directory alone
const ancestors = (cwd: string, path: string): readonly string[] => {
    const own = path.slice(0, Math.max(path.lastIndexOf('/'), 0));
    const chain = own
        .split('/')
        .map((_segment, index, segments) => segments.slice(0, index + 1).join('/'))
        .filter((directory) => directory === cwd || directory.startsWith(`${cwd}/`))
        .toReversed();
    return chain.length > 0 ? chain : [own];
};

// The server's limit default of 1000 keeps the edited file's items under a substring project filter
const diagnosticsRequest = (project: string): McpToolInputs['mcp__roslyn-codelens__get_diagnostics'] => ({
    project,
    severity: 'error',
    includeAnalyzers: true,
});

// The scope default is session, the trust lasts as long as the server
const trustRequest = (solutionPath: string): McpToolInputs['mcp__roslyn-codelens__trust_solution'] => ({ path: solutionPath });

// The control tool a first reply asks for before one re-read, trust_solution for the trust code and rebuild_solution for a degraded load
const recovery = (reply: Reply): 'trust_solution' | 'rebuild_solution' | undefined => {
    if (reply.isError && _UNTRUSTED.test(reply.text)) {
        return 'trust_solution';
    }
    return decodeEnvelope(reply.text)?.unreliable === true ? 'rebuild_solution' : undefined;
};

// The context lines of the final reply, the items under the edited file less the wrong ids by line, file is the edited path relative to the working directory
const diagnosticLines = (reply: Reply, file: string, project: string): readonly string[] => {
    if (reply.isError) {
        return [`Roslyn get_diagnostics failed: ${first(reply.text)}`];
    }
    const envelope = decodeEnvelope(reply.text);
    if (envelope === undefined) {
        return [`Roslyn get_diagnostics answered no envelope: ${reply.text.slice(0, _PREVIEW)}`];
    }
    const own = envelope.items
        .filter((item) => WRONG_DIAGNOSTICS[item.id] === undefined && (item.file === file || item.file.endsWith(`/${file}`)))
        .toSorted((left, right) => left.line - right.line)
        .map((item) => `${file}:${item.line} ${item.id}: ${item.message}`);
    return [...(own.length > 0 ? own : [`Roslyn: no error in ${file} (${project})`]), ...(envelope.unreliable ? [_DEGRADED] : [])];
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Diagnostic, Envelope, Listing, Reply };
export {
    ancestors,
    decodeEnvelope,
    diagnosticLines,
    diagnosticsRequest,
    isRoslynRead,
    projectOf,
    recovery,
    SERVER,
    SOLUTION,
    settleLine,
    settleWait,
    trustRequest,
    WATCHER_SETTLE_MS,
    WRONG_DIAGNOSTICS,
    wrongLines,
};
