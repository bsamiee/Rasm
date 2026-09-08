// The tool.call adapter folds the policy rules over every call and maps the decision onto the result union

// --- [IMPORTS] -------------------------------------------------------------------------

import type { FsEntry, McpContentBlock, McpToolResult, On, ToolCallInput, ToolCallResult } from 'claude-code';
import { type Decision, fold, type Rule, rewrite, when } from '../composition/decision.ts';
import type { Options } from '../host/options.ts';
import { decode, id, ids, isNotice, isNumber, isStringRecord, key, type StringRecord } from '../host/store.ts';
import { gitGuard, gitPaths } from '../policies/git.ts';
import { type PathEvent, type PathTool, pathOnce, pathRule, recordResult, recordWrite } from '../policies/paths.ts';
import {
    ancestors,
    decodeEnvelope,
    diagnosticLines,
    diagnosticsRequest,
    isRoslynRead,
    projectOf,
    type Reply,
    recovery,
    SERVER,
    SOLUTION,
    settleLine,
    settleWait,
    trustRequest,
    wrongLines,
} from '../policies/roslyn.ts';
import { abort, packages, type Run, scanHits, scanRows, TREE, utilLanguage } from '../policies/scan.ts';
import { restore } from '../policies/secrets.ts';
import { commandCeiling, commandTimeout, packageManager, shellOnce, shellRule } from '../policies/shell.ts';
import { type Facts, isTool, type Named, toolOnce, toolRecords, toolRule } from '../policies/tools.ts';
import { extension, relative } from '../text/path.ts';

// --- [TYPES] ---------------------------------------------------------------------------

// The two tools that land text at a file_path, the paths the scan rows read after the call
type Written = Extract<ToolCallInput, { readonly tool: 'Edit' | 'Write' }>;

// The built-in tools with the command the guard reads, four MCP tools hold a command field the guard leaves alone
type Commanded = Extract<ToolCallInput, { readonly tool: 'Bash' | 'Monitor' }> & { readonly command: string };

type Answered = Exclude<ToolCallResult, { readonly deny: string }>;

type Succeeded = Extract<ToolCallResult, { readonly deny?: undefined; readonly isError?: undefined }>;

// One roslyn-codelens call through $.mcp.call, answered as the Reply the roslyn lines read
type Call = (tool: string, args?: Readonly<Record<string, unknown>>) => Promise<Reply>;

// The engine calls the arms after next make, each built in the hook body over $ with its own catch
interface Io {
    readonly run: (argv: readonly string[], stdin?: string) => Promise<Run>;
    readonly record: (ruleId: string, file: string) => Promise<void>;
    readonly list: (dir: string) => Promise<readonly FsEntry[]>;
    readonly call: Call;
    readonly set: (storeKey: string, value: unknown) => Promise<void>;
    readonly sleep: (ms: number) => Promise<void>;
    readonly now: () => number;
}

// The session facts the arms read, gathered once in the hook body
interface Scope {
    readonly cwd: string;
    readonly session: string;
    readonly seen: ReadonlySet<string>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _PATH_TOOLS: readonly PathTool[] = ['Read', 'Edit', 'Write', 'NotebookEdit'];

// --- [REFINEMENTS] ---------------------------------------------------------------------

const _isBash = isTool('Bash');

const _isPath = (e: ToolCallInput): e is PathEvent => _PATH_TOOLS.some((tool) => tool === e.tool);

const _isWritten = (e: ToolCallInput): e is Written => e.tool === 'Edit' || e.tool === 'Write';

// Bash always, Monitor when it watches a command, and a ws monitor never
const _hasCommand = (e: ToolCallInput): e is Commanded => (e.tool === 'Bash' || e.tool === 'Monitor') && typeof e.command === 'string';

const _answered = (result: ToolCallResult): result is Answered => result.deny === undefined;

const _succeeded = (result: ToolCallResult): result is Succeeded => result.deny === undefined && result.isError === undefined;

// The result of an MCP call as next resolves it, the content blocks alone, the model's text is joined from them
const _isBlocks = (value: unknown): value is readonly McpContentBlock[] =>
    Array.isArray(value) && value.every((block) => typeof block === 'object' && block !== null && typeof block.type === 'string');

// --- [OPERATIONS] ----------------------------------------------------------------------

// Every once key the call injects that the session has not seen
const _onceKeys = (e: ToolCallInput, seen: ReadonlySet<string>): readonly string[] =>
    [...(_isBash(e) ? shellOnce(e.command) : []), ...(_isPath(e) ? pathOnce(e) : []), ...toolOnce(e)]
        .flatMap((line) => line.key ?? [])
        .filter((name) => !seen.has(name));

// The text of the first text block and '' without one, the text the model reads of a reply
const _firstText = (blocks: readonly McpContentBlock[]): string => blocks.find((block) => block.type === 'text')?.text ?? '';

// A call the engine rejects reads as an error reply whose text is the rejection, an Error prints as Error: <message>
const _failed = (error: unknown): McpToolResult => ({ content: [{ type: 'text', text: String(error) }], isError: true });

// The rule families' verdict on the edit, one child per row the path matches, each hit recorded for the telemetry
const _scan = async (cwd: string, written: Written, io: Io): Promise<readonly string[]> => {
    const path = relative(cwd, written.file_path);
    const language = utilLanguage(path);
    const ruleIds = language === undefined ? [] : (await packages(TREE.rules, io.list, language)).flatMap((group) => group.ids);
    const runs = await Promise.all(scanRows(path).map(async (row) => ({ row, run: await io.run(row.argv(path, ruleIds)) })));
    await Promise.all(runs.flatMap((entry) => scanHits(entry.run)).map((hit) => io.record(hit.ruleId, hit.file)));
    return runs.flatMap((entry) => entry.row.lines(entry.run, path));
};

// The final reply over a project, the first read then the recovery its class asks for and one re-read
const _finalReply = async (cwd: string, project: string, call: Call): Promise<Reply> => {
    const read = (): Promise<Reply> => call('get_diagnostics', diagnosticsRequest(project));
    const reply = await read();
    const control = recovery(reply);
    if (control === undefined) {
        return reply;
    }
    await call(control, control === 'trust_solution' ? trustRequest(`${cwd}/${SOLUTION}`) : undefined);
    return read();
};

// The analyzer errors under the C# file the model just edited, after the watcher window, the project from the ancestor listings
const _roslynLines = async (scope: Scope, written: Written, io: Io): Promise<readonly string[]> => {
    const at = io.now();
    await io.set(key('roslyn', scope.session), at);
    await io.sleep(settleWait(at, io.now()));
    const path = relative(scope.cwd, written.file_path);
    const listings = await Promise.all(
        ancestors(scope.cwd, written.file_path).map(async (dir) => ({ dir, names: (await io.list(dir)).map((entry) => entry.name) })),
    );
    const project = projectOf(listings);
    return project === undefined
        ? [`Roslyn: no .csproj above ${path}`]
        : diagnosticLines(await _finalReply(scope.cwd, project, io.call), path, project);
};

// The wrong ids a by-hand get_diagnostics reply holds, one line per id beside the reply
const _wrongLines = (e: ToolCallInput, result: ToolCallResult): readonly string[] => {
    if (!(isTool('mcp__roslyn-codelens__get_diagnostics')(e) && _succeeded(result) && _isBlocks(result.result))) {
        return [];
    }
    const envelope = decodeEnvelope(_firstText(result.result));
    return envelope === undefined ? [] : wrongLines(envelope);
};

// The arms after next in one Promise.all: the recording stamp, the scan lines, and the roslyn lines after the write's stamp and its window
const _afterNext = async (scope: Scope, e: ToolCallInput, result: ToolCallResult, io: Io): Promise<readonly string[]> => {
    const written = _succeeded(result) && _isWritten(e) ? e : undefined;
    const recorded = _succeeded(result) ? toolRecords(e) : undefined;
    const [, scanLines, roslynLines] = await Promise.all([
        recorded === undefined ? undefined : io.set(key(recorded.namespace, scope.session, recorded.id), io.now()),
        written === undefined ? [] : _scan(scope.cwd, written, io),
        written !== undefined && extension(written.file_path) === '.cs' ? _roslynLines(scope, written, io) : [],
    ]);
    return [...scanLines, ...roslynLines];
};

// Answered results take the lines, and an empty context leaves the result as next resolved it
const _withContext = (result: ToolCallResult, context: readonly string[]): ToolCallResult =>
    _answered(result) && context.length > 0 ? { ...result, context: [...(result.context ?? []), ...context] } : result;

// The shell rules over a Bash command with the secret values restored for the guards to read
const _shelled = (e: ToolCallInput, seen: ReadonlySet<string>, secrets: StringRecord): Decision<ToolCallInput> =>
    fold<ToolCallInput>([
        when(_isBash, (bash): Decision<Named<'Bash'>> => rewrite({ ...bash, command: restore(bash.command, secrets) })),
        when(_isBash, shellRule(seen)),
        when(_isBash, commandTimeout),
        when(_isBash, commandCeiling),
    ])(e);

// The guard and the rows over the command the shell rules produced, the package manager rewrite last for the model to read
const _decider =
    (manager: Rule<Named<'Bash'>>) =>
    (e: ToolCallInput, shelled: Decision<ToolCallInput>, facts: Facts, existing: ReadonlySet<string>): Decision<ToolCallInput> =>
        fold<ToolCallInput>([
            (): Decision<ToolCallInput> => shelled,
            when(_hasCommand, gitGuard(existing)),
            when(_isPath, pathRule(facts.seen)),
            toolRule(facts),
            when(_isBash, manager),
        ])(e);

// The answered Write of a record runs through its child, every other answer as the rule decided it
const _answer = async (e: ToolCallInput, result: unknown, run: Io['run']): Promise<ToolCallResult> => {
    const record = _isPath(e) ? recordWrite(e) : undefined;
    if (record === undefined) {
        return { result };
    }
    const child = await run(record.argv, record.stdin);
    return child.exitCode === 0 ? { result: recordResult(record.result, child.stdout) } : { isError: true, result: child.stderr, text: child.stderr };
};

// The session facts the tool rows read, from the key list and the redacted prompt row
const _facts = (session: string, cwd: string, all: readonly string[], promptRow: unknown): Facts => ({
    seen: new Set([...ids('injected', session)(all), ...ids('loaded', session)(all)]),
    snapshots: new Set(ids('snapshot', session)(all)),
    dns: new Set(ids('dns', session)(all)),
    prompt: decode(isNotice)(promptRow)?.text ?? '',
    cwd,
});

const _record = (row: unknown): StringRecord => decode(isStringRecord)(row) ?? {};

// The command the guard reads, the one the shell rules produced
const _command = (shelled: Decision<ToolCallInput>): string => (shelled.kind === 'rewrite' && _hasCommand(shelled.e) ? shelled.e.command : '');

// --- [REGISTRATION] --------------------------------------------------------------------

const toolCall = (on: On, options: Options): void => {
    const decide = _decider(packageManager(options.packageManager));
    on('tool.call', async ($, e, next) => {
        const [session, cwd, secrets, all] = await Promise.all([$.session.id(), $.session.cwd(), $.store.get(key('secrets')), $.store.keys()]);
        const [promptRow, sessionRow, roslynRow] = await Promise.all([
            $.store.get(key('prompt', session)),
            $.store.get(key('session', session)),
            $.store.get(key('roslyn', session)),
        ]);
        const facts = _facts(session, cwd, all, promptRow);
        const env = _record(sessionRow);
        const io: Io = {
            run: (argv, stdin) => $.process.run(argv, stdin === undefined ? { env } : { env, stdin }).catch(abort),
            record: (ruleId, file) => $.store.set(key('scan', id($.clock.now(), crypto.randomUUID())), { ruleId, file }),
            list: (dir) => $.fs.listDir(dir).catch((): readonly FsEntry[] => []),
            call: async (tool, args) => {
                const result = await $.mcp.call(SERVER, tool, args).catch(_failed);
                return { isError: result.isError, text: _firstText(result.content) };
            },
            set: (storeKey, value) => $.store.set(storeKey, value),
            sleep: (ms) => (ms > 0 ? $.clock.sleep(ms, { signal: next.signal }) : Promise.resolve()),
            now: () => $.clock.now(),
        };
        // The guard's refinements read which paths exist, over the command the shell rules produce
        const shelled = _shelled(e, facts.seen, _record(secrets));
        const existing: ReadonlySet<string> = new Set(
            (
                await Promise.all(
                    gitPaths(_command(shelled)).map(async (path) => ((await $.fs.exists(path).catch((): false => false)) ? [path] : [])),
                )
            ).flat(),
        );
        const decision = decide(e, shelled, facts, existing);
        if (decision.kind === 'deny') {
            return { deny: decision.reason };
        }
        if (decision.kind === 'answer') {
            return _answer(e, decision.result, io.run);
        }
        await Promise.all(_onceKeys(decision.e, facts.seen).map((name) => $.store.set(key('injected', session, name), $.clock.now())));
        $.ui.notice(e.tool_use_id, decision.context.length > 0 ? decision.context.join(' ') : undefined);
        // A by-hand read of the compilation waits the rest of the watcher window since the last .cs write
        const wait = isRoslynRead(e.tool) ? settleWait(decode(isNumber)(roslynRow), $.clock.now()) : 0;
        await io.sleep(wait);
        const result = await next(decision.e);
        const lines = await _afterNext({ cwd, session, seen: facts.seen }, e, result, io);
        return _withContext(result, [...decision.context, ...(wait > 0 ? [settleLine(wait)] : []), ..._wrongLines(e, result), ...lines]);
    });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { toolCall };
