// The tool.call adapter folds the policy rules over every call and maps the decision onto the result union

// --- [IMPORTS] -------------------------------------------------------------------------

import type { FsEntry, McpContentBlock, McpToolResult, On, ToolCallInput, ToolCallResult } from 'claude-code';
import { bind, fold, when } from '../composition/decision.ts';
import {
    flatMap,
    forEach,
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
    traverse,
} from '../composition/option.ts';
import { type Options, whenEnabled } from '../host/options.ts';
import {
    cleanedOf,
    decodeJson,
    decodeKeyedFindings,
    decodeNotice,
    decodeSession,
    decodeStamp,
    type Environment,
    id,
    ids,
    key,
    keys,
    type Namespace,
    type Notice,
    type Scan,
    type Session,
    type Stamp,
    secretsOf,
    stamp,
} from '../host/store.ts';
import { close, FINDING_VIEWS } from '../policies/findings.ts';
import { gitGuard, gitPaths } from '../policies/git.ts';
import { type PathEvent, type PathTool, pathRule, pathSkills, recordSearches, type Search, type Searched } from '../policies/paths.ts';
import {
    ancestors,
    diagnosticLines,
    diagnosticsRequest,
    droppedLines,
    type Filtered,
    filterEnvelope,
    isRoslynRead,
    projectOf,
    type Reply,
    type ReplyClass,
    replyClass,
    SERVER,
    SOLUTION,
    settleLine,
    settleWait,
    trustRequest,
} from '../policies/roslyn.ts';
import {
    abort,
    type Hit,
    needsRuleIds,
    packages,
    type Run,
    ruleIds,
    type ScanFacts,
    type ScanRow,
    scanHits,
    scanRows,
    TREE,
} from '../policies/scan.ts';
import { pairs, restore } from '../policies/secrets.ts';
import { commandTimeout, type NxCaches, type NxTarget, nxTargets, packageManager, shellRule, shellSkills, skipNxCache } from '../policies/shell.ts';
import { isTool, type Named, type Recorded, toolRecords, toolRule, toolSkills } from '../policies/tools.ts';
import { extension, relative } from '../text/path.ts';

// --- [TYPES] ---------------------------------------------------------------------------

// The two tools that land text at a file_path, the paths the scan rows read after the call
type Written = Extract<ToolCallInput, { readonly tool: 'Edit' | 'Write' }>;

// The built-in tools with the command the guard reads, four MCP tools hold a command field the guard leaves alone
type Commanded = Extract<ToolCallInput, { readonly tool: 'Bash' | 'Monitor' }> & { readonly command: string };

type Answered = Exclude<ToolCallResult, { readonly deny: string }>;

type Succeeded = Extract<ToolCallResult, { readonly deny?: undefined; readonly isError?: undefined }>;

// The succeeded edit with the session row, its scan children run under the row's environment
interface Target {
    readonly facts: Session;
    readonly written: Written;
}

interface ScanRun {
    readonly row: ScanRow;
    readonly run: Run;
}

// The engine calls the scan arm makes, each built in the hook body over $ with its own catch
interface Runner {
    readonly run: (argv: readonly string[], timeoutMs?: number) => Promise<Run>;
    readonly record: (row: Scan) => Promise<void>;
    readonly list: (dir: string) => Promise<readonly FsEntry[]>;
}

// One roslyn-codelens call through $.mcp.call, answered as the Reply the roslyn lines read
type Call = (tool: string, args?: Readonly<Record<string, unknown>>) => Promise<Reply>;

// The engine calls the arms after next make, each built in the hook body over $, the run over the environment the scan target names
interface Io extends Omit<Runner, 'run'> {
    readonly run: (argv: readonly string[], env: Environment, timeoutMs?: number) => Promise<Run>;
    readonly call: Call;
    readonly set: (storeKey: string, value: unknown) => Promise<void>;
    readonly sleep: (ms: number) => Promise<void>;
    readonly now: () => number;
}

// The stamp sets of the session the rules read, the tool rule's facts less the prompt
interface Stamps {
    readonly seen: ReadonlySet<string>;
    readonly snapshots: ReadonlySet<string>;
    readonly dns: ReadonlySet<string>;
}

// The session facts the arms after next read, gathered once in the hook body
interface Scope {
    readonly cwd: string;
    readonly session: string;
    readonly facts: Option<Session>;
}

// A result after the read hook's rewrite with the lines the rewrite adds, the result as next resolved it with none
interface Rewritten {
    readonly result: ToolCallResult;
    readonly lines: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _PATH_TOOLS: readonly PathTool[] = ['Read', 'Edit', 'Write', 'NotebookEdit'];

// The namespace a successful recording tool stamps, read by the hostinger families
const _RECORD: Readonly<Record<Recorded['kind'], Namespace>> = { 'vm-snapshot': 'snapshot', 'dns-read': 'dns' };

const _CS = '.cs';
const _NO_RULES: ScanFacts = { rules: [] };

// The recovery call per reply class before the one re-read, a read reply asks for none
const _RECOVERY: Readonly<Record<ReplyClass, (cwd: string, call: Call) => Option<Promise<Reply>>>> = {
    untrusted: (cwd, call) => some(call('trust_solution', trustRequest(`${cwd}/${SOLUTION}`))),
    unreliable: (_cwd, call) => some(call('rebuild_solution')),
    read: none,
};

// --- [REFINEMENTS] ---------------------------------------------------------------------

const _isBash = isTool('Bash');

const _isPath = (e: ToolCallInput): e is PathEvent => _PATH_TOOLS.some((tool) => tool === e.tool);

// The markdown lines of an Edit number from the file, read in the hook body before the call
const _isEdit = isTool('Edit');

const _isWritten = (e: ToolCallInput): e is Written => e.tool === 'Edit' || e.tool === 'Write';

// Bash always, Monitor when it watches a command, and a ws monitor never
const _hasCommand = (e: ToolCallInput): e is Commanded => (e.tool === 'Bash' || e.tool === 'Monitor') && typeof e.command === 'string';

const _answered = (result: ToolCallResult): result is Answered => result.deny === undefined;

const _succeeded = (result: ToolCallResult): result is Succeeded => result.deny === undefined && result.isError === undefined;

const _isCs = (target: Target): boolean => extension(target.written.file_path) === _CS;

// The by-hand read whose reply the hook filters, the arm's own call skips the hook and keeps its own filter
const _isDiagnosticsRead = isTool('mcp__roslyn-codelens__get_diagnostics');

// The result of an MCP call as next resolves it, the content blocks alone, the model's text is joined from them
const _isBlocks = (value: unknown): value is readonly McpContentBlock[] =>
    Array.isArray(value) && value.every((block) => isRecord(block) && typeof block.type === 'string');

// --- [OPERATIONS] ----------------------------------------------------------------------

// Every once key the call injects that the session has not seen
const _onceKeys = (input: ToolCallInput, sets: Stamps): readonly string[] =>
    [
        ...new Set([
            ...getOrElse((): readonly string[] => [])(map((bash: Commanded) => shellSkills(bash.command))(fromPredicate(_isBash)(input))),
            ...getOrElse((): readonly string[] => [])(map(pathSkills)(fromPredicate(_isPath)(input))),
            ...toolSkills(input),
        ]),
    ].filter((name) => !sets.seen.has(name));

// The guidance directories the markdown rows measure, from the session row session.start wrote, none without the row
const _guidance = (facts: Option<Session>): readonly string[] =>
    getOrElse((): readonly string[] => [])(map((session: Session) => [...session.claudeChain, ...toArray(fromNullable(session.memoryDir))])(facts));

// The session's stamp sets, the once keys injected or loaded, the recorded snapshots, and the read domains
const _stamps = (all: readonly string[], session: string): Stamps => ({
    seen: new Set([...ids('injected', session)(all), ...ids('loaded', session)(all)]),
    snapshots: new Set(ids('snapshot', session)(all)),
    dns: new Set(ids('dns', session)(all)),
});

// The path as a some when its file exists, through the existence check the hook body builds
const _present =
    (exists: (path: string) => Promise<boolean>): ((path: string) => Promise<Option<string>>) =>
    (path: string): Promise<Option<string>> =>
        exists(path).then((found) => liftPredicate<string>(() => found)(path));

// The paths of the command that exist, the facts the git refinements test, a call without a command names no path
const _existing = async (e: ToolCallInput, exists: (path: string) => Promise<boolean>): Promise<ReadonlySet<string>> =>
    new Set(
        await traverse(_present(exists))(
            getOrElse((): readonly string[] => [])(map((commanded: Commanded) => gitPaths(commanded.command))(fromPredicate(_hasCommand)(e))),
        ),
    );

// The edited file's text before an Edit, '' for another tool or an absent file, the fs noun rejects a path outside the working directory
// and a memory-directory file then reads as '' with its lines numbered from the text
const _fileText = async (e: ToolCallInput, exists: (path: string) => Promise<boolean>, read: (path: string) => Promise<string>): Promise<string> =>
    getOrElse(() => '')(
        await forEach(read)(
            getOrElse<Option<string>>(none)(await forEach(_present(exists))(map((edit: Named<'Edit'>) => edit.file_path)(fromPredicate(_isEdit)(e)))),
        ),
    );

const _isBoolean = (value: unknown): value is boolean => typeof value === 'boolean';

// The boolean under targets.<target>.cache of an nx show project --json answer, none when the answer, the field, or a boolean is absent
const _cacheOf = (run: Run, target: string): Option<boolean> =>
    flatMap((row: Readonly<Record<string, unknown>>) => fromPredicate(_isBoolean)(row.cache))(
        flatMap((targets: Readonly<Record<string, unknown>>) => fromPredicate(isRecord)(targets[target]))(
            flatMap((project: Readonly<Record<string, unknown>>) => fromPredicate(isRecord)(project.targets))(
                flatMap(fromPredicate(isRecord))(decodeJson(run.stdout)),
            ),
        ),
    );

// The cache flag per project:target of every nx run leaf carrying a skip-cache flag, read from nx show project under the session environment
const _nxCaches = async (
    e: ToolCallInput,
    facts: Option<Session>,
    run: (argv: readonly string[], env: Environment) => Promise<Run>,
): Promise<NxCaches> =>
    getOrElse((): NxCaches => ({}))(
        await forEach(async (found: { readonly env: Environment; readonly targets: readonly NxTarget[] }) =>
            Object.fromEntries(
                await traverse(async (named: NxTarget) =>
                    map((cache: boolean): readonly [string, boolean] => [`${named.project}:${named.target}`, cache])(
                        _cacheOf(await run(['pnpm', 'exec', 'nx', 'show', 'project', named.project, '--json'], found.env), named.target),
                    ),
                )(found.targets),
            ),
        )(
            flatMap((row: Session) => map((bash: Named<'Bash'>) => ({ env: row.env, targets: nxTargets(bash.command) }))(fromPredicate(_isBash)(e)))(
                facts,
            ),
        ),
    );

// The record searches of a path call's dropped dependency rows, each run under the session environment, none without a session row
const _searched = async (
    e: ToolCallInput,
    cwd: string,
    facts: Option<Session>,
    run: (argv: readonly string[], env: Environment) => Promise<Run>,
): Promise<readonly Searched[]> =>
    getOrElse((): readonly Searched[] => [])(
        await forEach((found: { readonly env: Environment; readonly searches: readonly Search[] }) =>
            Promise.all(found.searches.map(async (search: Search) => ({ ...search, run: await run(search.argv, found.env) }))),
        )(
            flatMap((row: Session) => map((path: PathEvent) => ({ env: row.env, searches: recordSearches(path, cwd) }))(fromPredicate(_isPath)(e)))(
                facts,
            ),
        ),
    );

// The edit the scan rows read after the call, a succeeded Edit or Write under a session row, none otherwise
const _scanTarget = (e: ToolCallInput, result: ToolCallResult, facts: Option<Session>): Option<Target> =>
    flatMap(() => flatMap((row: Session) => map((written: Written) => ({ facts: row, written }))(fromPredicate(_isWritten)(e)))(facts))(
        fromPredicate(_succeeded)(result),
    );

// The rule and rewrite ids with their language, listed for a util path alone, every other path lists nothing
const _scanFacts = async (path: string, list: (dir: string) => Promise<readonly FsEntry[]>): Promise<ScanFacts> =>
    getOrElse(() => _NO_RULES)(
        await forEach(
            async (): Promise<ScanFacts> => ({
                rules: ruleIds((await Promise.all([packages(TREE.rules, list), packages(TREE.rewrites, list)])).flat()),
            }),
        )(fromBoolean(needsRuleIds(path))),
    );

// One run per row the path matches, through the process runner the hook body builds over the session environment
const _scanRuns = (path: string, facts: ScanFacts, run: (argv: readonly string[], timeoutMs?: number) => Promise<Run>): Promise<readonly ScanRun[]> =>
    Promise.all(scanRows(path, facts).map(async (row) => ({ row, run: await run(row.argv(path, facts), row.timeoutMs) })));

// Every run's stdout is read for hits, the test rows print no JSON array and the nx row's names hold no compact fields, so both contribute none
const _hits = (runs: readonly ScanRun[]): readonly Hit[] => runs.flatMap((entry) => scanHits(entry.run));

const _scanLines = (runs: readonly ScanRun[], path: string, facts: ScanFacts): readonly string[] =>
    runs.flatMap((entry) => entry.row.lines(entry.run, path, facts));

// The rule families' verdict on the edit through the runner and the hit writer the hook body builds, a child that did not start reads as an abort line
const _scan = async (cwd: string, target: Target, runner: Runner): Promise<readonly string[]> => {
    const path = relative(cwd, target.written.file_path);
    const facts = await _scanFacts(path, runner.list);
    const runs = await _scanRuns(path, facts, runner.run);
    await Promise.all(_hits(runs).map((hit) => runner.record({ ruleId: hit.ruleId, file: hit.file })));
    return _scanLines(runs, path, facts);
};

// The text of the first text block and '' without one, the text the model reads of a reply
const _firstText = (blocks: readonly McpContentBlock[]): string =>
    getOrElse(() => '')(fromNullable(blocks.find((block) => block.type === 'text')?.text));

// The reply of a call, the text of its first text block
const _reply = (result: McpToolResult): Reply => ({ isError: result.isError, text: _firstText(result.content) });

// A call the engine rejects reads as an error reply whose text is the rejection, an Error prints as Error: <message>
const _failed = (error: unknown): McpToolResult => ({ content: [{ type: 'text', text: String(error) }], isError: true });

// The final reply over a project, the first read then the recovery its class asks for and one re-read
const _finalReply = async (cwd: string, project: string, call: Call): Promise<Reply> => {
    const read = (): Promise<Reply> => call('get_diagnostics', diagnosticsRequest(project));
    const reply = await read();
    return getOrElse(() => reply)(await forEach((recovery: Promise<Reply>) => recovery.then(read))(_RECOVERY[replyClass(reply)](cwd, call)));
};

// The model reads the analyzer errors under the C# file it just edited, the build's set less the WRONG_DIAGNOSTICS rows, the project from
// the ancestor listings and one line when none holds a .csproj
const _roslynLines = async (
    cwd: string,
    target: Target,
    list: (dir: string) => Promise<readonly FsEntry[]>,
    call: Call,
): Promise<readonly string[]> => {
    const path = relative(cwd, target.written.file_path);
    const listings = await Promise.all(
        ancestors(cwd, target.written.file_path).map(async (dir) => ({ dir, names: (await list(dir)).map((entry) => entry.name) })),
    );
    return getOrElse((): readonly string[] => [`Roslyn: no .csproj above ${path}`])(
        await forEach(async (project: string) => diagnosticLines(await _finalReply(cwd, project, call), path, project))(projectOf(listings)),
    );
};

// The rest of the watcher window since the last .cs write, waited before a read of the compilation, and the line naming the wait
const _settle = async (e: ToolCallInput, stampAt: Option<number>, now: number, sleep: (ms: number) => Promise<void>): Promise<readonly string[]> =>
    toArray(
        await forEach((ms: number) => sleep(ms).then(() => settleLine(ms)))(
            flatMap(() => liftPredicate<number>((ms) => ms > 0)(settleWait(stampAt, now)))(fromPredicate(isRoslynRead)(e)),
        ),
    );

// The blocks with the text of the first text block replaced, the block the envelope was read from
const _withText = (blocks: readonly McpContentBlock[], text: string): readonly McpContentBlock[] => {
    const index = blocks.findIndex((block) => block.type === 'text');
    return blocks.map((block, at) => fromBoolean(at === index).match<McpContentBlock>({ some: () => ({ ...block, text }), none: () => block }));
};

// The blocks less the wrong items as the hook's own result, without ref and text so core maps the blocks for the model afresh
const _filteredReply = (succeeded: Succeeded): Option<Rewritten> =>
    flatMap((blocks: readonly McpContentBlock[]) =>
        map((filtered: Filtered): Rewritten => ({ result: { result: _withText(blocks, filtered.text) }, lines: droppedLines(filtered.dropped) }))(
            filterEnvelope(_firstText(blocks)),
        ),
    )(fromPredicate(_isBlocks)(succeeded.result));

// A by-hand get_diagnostics result less the WRONG_DIAGNOSTICS items with its lines, every other result as next resolved it
const _diagnosticsResult = (e: ToolCallInput, result: ToolCallResult): Rewritten =>
    getOrElse((): Rewritten => ({ result, lines: [] }))(
        flatMap(_filteredReply)(flatMap(() => fromPredicate(_succeeded)(result))(fromPredicate(_isDiagnosticsRead)(e))),
    );

// The arms after next in one Promise.all: the recording stamp, the scan lines, and the roslyn lines after the write's stamp and its window
const _settled = async (scope: Scope, e: ToolCallInput, result: ToolCallResult, io: Io): Promise<readonly string[]> => {
    const target = _scanTarget(e, result, scope.facts);
    const [, scanLines, roslynLines] = await Promise.all([
        forEach((recorded: Recorded) => io.set(key(_RECORD[recorded.kind], scope.session, recorded.id), stamp(scope.session, io.now())))(
            flatMap(() => toolRecords(e))(fromPredicate(_succeeded)(result)),
        ),
        forEach((found: Target) =>
            _scan(scope.cwd, found, { run: (argv, timeoutMs) => io.run(argv, found.facts.env, timeoutMs), record: io.record, list: io.list }),
        )(target),
        forEach(async (found: Target) => {
            // The write's time under roslyn/<session>, the by-hand reads then wait the rest of the window
            const at = io.now();
            await io.set(key('roslyn', scope.session), stamp(scope.session, at));
            await io.sleep(settleWait(some(at), io.now()));
            return _roslynLines(scope.cwd, found, io.list, io.call);
        })(flatMap(liftPredicate(_isCs))(target)),
    ]);
    return [scanLines, roslynLines].flatMap(toArray).flat();
};

// Answered results take the lines, and an empty context leaves the result as next resolved it
const _withContext = (result: ToolCallResult, context: readonly string[]): ToolCallResult =>
    fromPredicate((candidate: ToolCallResult): candidate is Answered => _answered(candidate) && context.length > 0)(result).match<ToolCallResult>({
        some: (answered) => ({ ...answered, context: [...(answered.context ?? []), ...context] }),
        none: () => result,
    });

// --- [REGISTRATION] --------------------------------------------------------------------

// The spawned editor's one way back to the store, declared before the fold to keep the matched hook's narrowing within the instantiation budget
const _serve = (on: On): void => {
    on('tool.call', { tool: 'mcp__function-hooks__close' }, async ($, e, _next): Promise<ToolCallResult> => {
        const all = await $.store.keys();
        const rows = await Promise.all(keys('findings')(all).map(async (storeKey) => ({ key: storeKey, value: await $.store.get(storeKey) })));
        const closing = close(e, decodeKeyedFindings(rows), cleanedOf(await $.store.get(key('cleaned'))), $.clock.now());
        await Promise.all(closing.writes.map((write) => $.store.set(write.key, write.value)));
        // The settled batch lifts the tick's hold, as the timer's own settle does
        await $.store.delete(key('dispatch', e.batchId));
        FINDING_VIEWS.map((view) => $.ui.invalidate(view));
        return { result: closing.result };
    });
};

const _call = (on: On, options: Options): void => {
    on('tool.call', async ($, e, next) => {
        const [session, cwd, secrets, all] = await Promise.all([$.session.id(), $.session.cwd(), $.store.get(key('secrets')), $.store.keys()]);
        const [promptRow, sessionRow, roslynRow] = await Promise.all([
            $.store.get(key('prompt', session)),
            $.store.get(key('session', session)),
            $.store.get(key('roslyn', session)),
        ]);
        const prompt = getOrElse(() => '')(map((row: Notice) => row.text)(decodeNotice(promptRow)));
        const facts = decodeSession(sessionRow);
        const exists = (path: string): Promise<boolean> => $.fs.exists(path).catch((): false => false);
        const sets = _stamps(all, session);
        // The shell rewrites run first and the guard and the cache read the command they produce, the timeout pass repeats for a prefix a rewrite exposes
        const shelled = fold<ToolCallInput, unknown, string>([
            when(_isBash, restore(pairs(secretsOf(secrets)))),
            when(_isBash, commandTimeout),
            when(_isBash, shellRule(sets.seen)),
            when(_isBash, commandTimeout),
        ])(e);
        const rewritten = shelled.match<ToolCallInput>({ rewrite: (input) => input, deny: () => e, answer: () => e });
        const run = (argv: readonly string[], env: Environment): Promise<Run> => $.process.run(argv, { env }).catch(abort);
        const [existing, file, caches, searches] = await Promise.all([
            _existing(rewritten, exists),
            _fileText(e, exists, (path) => $.fs.readFile(path).catch(() => '')),
            _nxCaches(rewritten, facts, run),
            _searched(e, cwd, facts, run),
        ]);
        const sleep = (ms: number): Promise<void> => $.clock.sleep(ms, { signal: next.signal });
        const io: Io = {
            run: (argv, env, timeoutMs) => $.process.run(argv, { env, timeoutMs }).catch(abort),
            record: (row) => $.store.set(key('scan', id($.clock.now(), crypto.randomUUID())), row),
            list: (dir) => $.fs.listDir(dir).catch(() => []),
            call: async (tool, args) => _reply(await $.mcp.call(SERVER, tool, args).catch(_failed)),
            set: (storeKey, value) => $.store.set(storeKey, value),
            sleep,
            now: () => $.clock.now(),
        };
        const decision = bind(
            fold<ToolCallInput, unknown, string>([
                when(_isBash, skipNxCache(caches)),
                when(_hasCommand, gitGuard(existing)),
                when(_isPath, pathRule({ seen: sets.seen, guidance: _guidance(facts), file, searches })),
                toolRule({ ...sets, prompt, cwd }),
                when(_isBash, packageManager(options.packageManager)),
            ]),
        )(shelled);
        return decision.match<ToolCallResult | Promise<ToolCallResult>>({
            deny: (reason) => ({ deny: reason }),
            answer: (result) => ({ result }),
            rewrite: async (input, context) => {
                await Promise.all(_onceKeys(input, sets).map((name) => $.store.set(key('injected', session, name), stamp(session, $.clock.now()))));
                fromBoolean(context.length > 0).match<void>({ some: () => $.ui.notice(e.tool_use_id, context.join(' ')), none: () => undefined });
                const waited = await _settle(input, map((row: Stamp) => row.at)(decodeStamp(roslynRow)), $.clock.now(), sleep);
                const read = _diagnosticsResult(e, await next(input));
                const lines = await _settled({ cwd, session, facts }, e, read.result, io);
                return _withContext(read.result, [...context, ...waited, ...read.lines, ...lines]);
            },
        });
    });
};

// The plain hook wraps the served tool, and the tool is registered only while dispatch is on
const toolCall = (on: On, options: Options): void => {
    _call(on, options);
    whenEnabled(options.dispatch, () => _serve(on));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { toolCall };
