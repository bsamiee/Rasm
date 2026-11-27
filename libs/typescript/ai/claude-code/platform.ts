import { Shell } from '@rasm/shell';
import type { AgentStatus, EngineInterface, FsEntry, FsStat, HookStream, ProcessSpawnChunk, ProcessSpawnRequest, ProcessSpawnResult } from 'claude-code';
import {
    Array,
    Bytes,
    ByteSize,
    Cache,
    Cause,
    Clock,
    Context,
    Data,
    DateTime,
    Deferred,
    Duration,
    Effect,
    Exit,
    Fiber,
    FiberSet,
    FileSystem,
    Filter,
    Formatter,
    Function,
    Hash,
    Layer,
    Logger,
    ManagedRuntime,
    Match,
    Option,
    Order,
    Path,
    PlatformError,
    Predicate,
    Queue,
    Record,
    Redacted,
    Scheduler,
    Sink,
    Stream,
} from 'effect';
import { ChildProcess, ChildProcessSpawner, CommandLine } from 'effect/process';
import 'sh-syntax/vendors/wasm_exec';
import { getProcessor } from 'sh-syntax/sync';

// --- [TYPES] ---------------------------------------------------------------------------

type Probe = ReturnType<Shell['Service']['dialect']>;
type Output = Queue.Queue<Uint8Array, PlatformError.PlatformError | Cause.Done>;
type Child = HookStream<ProcessSpawnChunk, ProcessSpawnResult>;
type Notice = Data.TaggedEnum<{
    Toast: { readonly text: string; readonly timeout?: Duration.Input };
    Notify: { readonly key: string; readonly text: Option.Option<string> };
    Model: { readonly text: string; readonly agent: Option.Option<string>; readonly idle: boolean };
}>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const readLimit = 4 * 1024 ** 2;
const slice = 8;
const errno = / failed(?: to start)?: (?<code>E[A-Z]+)/u;
const errnoTags: Readonly<Record<string, PlatformError.SystemErrorTag>> = { ENOENT: 'NotFound', EACCES: 'PermissionDenied', EPERM: 'PermissionDenied', EEXIST: 'AlreadyExists', EISDIR: 'BadResource', ENOTDIR: 'BadResource', ELOOP: 'BadResource', EBUSY: 'Busy' };
const kinds = { file: 'File', dir: 'Directory', other: 'Unknown' } as const satisfies Record<FsStat['kind'], FileSystem.File.Type>;
const resting = { pending: Option.some(false), running: Option.some(false), waiting: Option.some(true), idle: Option.some(true), completed: Option.none(), failed: Option.none(), killed: Option.none() } as const satisfies Record<AgentStatus, Option.Option<boolean>>;
const variable = /^(?<name>[^=]+)=(?<value>.*)$/su;
const hookScript = /-hook-\d+\.sh$/u;
const encoder = new TextEncoder();
const decoder = new TextDecoder('utf-8', { fatal: true });
const collator = new Intl.Collator('en', { numeric: true });
const byTime = Order.mapInput(Order.Number, ({ mtimeMs }: FsEntry) => mtimeMs);

// --- [MODELS] --------------------------------------------------------------------------

const Notice = Data.taggedEnum<Notice>();

// --- [SERVICES] ------------------------------------------------------------------------

class TimeSliced extends Scheduler.MixedScheduler {
    #fiber: object | undefined = undefined;
    #ops = 0;
    #start = 0;

    override shouldYield(fiber: Parameters<Scheduler.Scheduler['shouldYield']>[0]) {
        this.#start = fiber !== this.#fiber || fiber.currentOpCount <= this.#ops ? fiber.getRef(Clock.Clock).currentTimeMillisUnsafe() : this.#start;
        const due = super.shouldYield(fiber) || ((fiber.currentOpCount & 15) === 0 && fiber.getRef(Clock.Clock).currentTimeMillisUnsafe() - this.#start >= slice);
        this.#fiber = fiber;
        this.#ops = due ? Number.POSITIVE_INFINITY : fiber.currentOpCount;
        return due;
    }
}

class Session extends Context.Service<
    Session,
    {
        readonly acting: (agent: Option.Option<string>) => Effect.Effect<
            {
                readonly environment: Effect.Effect<{ readonly dialect: Effect.Success<Probe>; readonly cwd: string }, Effect.Error<Probe> | PlatformError.PlatformError | CommandLine.CommandLineError>;
                readonly path: Path.Path;
            },
            PlatformError.PlatformError
        >;
        readonly resting: (agent: string) => Effect.Effect<Option.Option<boolean>, PlatformError.PlatformError>;
        readonly watch: <E, R>(watcher: Effect.Effect<unknown, E, R>) => Effect.Effect<void, never, R>;
    }
>()('@rasm/claude-code/platform/Session') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const systemError = (module: string, method: string, subject: string) => (cause: unknown) => {
    const said = Predicate.isError(cause) ? cause.message : Formatter.format(cause);
    return PlatformError.systemError({
        _tag: Option.fromNullishOr(errno.exec(said)?.groups?.['code']).pipe(
            Option.flatMap((code) => Record.get(errnoTags, code)),
            Option.getOrElse((): PlatformError.SystemErrorTag => 'Unknown'),
        ),
        module,
        method,
        pathOrDescriptor: subject,
        description: said,
        cause,
    });
};
const request = <A>(module: string, method: string, subject: string, run: () => PromiseLike<A>) => Effect.tryPromise({ try: run, catch: systemError(module, method, subject) });
const delivered =
    <A>(module: string, method: string, subject: string, run: () => PromiseLike<A>) =>
    (refusal: (answer: A) => string | undefined) =>
        Effect.flatMap(request(module, method, subject, run), (answer) =>
            Option.match(Option.fromUndefinedOr(refusal(answer)), {
                onNone: () => Effect.void,
                onSome: (description) => PlatformError.systemError({ _tag: 'Unknown', module, method, pathOrDescriptor: subject, description }),
            }),
        );
function say($: EngineInterface) {
    return Notice.$match({
        Toast: ({ text, timeout }) => Effect.try({ try: () => $.ui.toast(text, timeout === undefined ? undefined : { timeoutMs: Duration.toMillis(timeout) }), catch: systemError('ui', 'toast', text) }),
        Notify: ({ key, text }) =>
            Option.match(text, {
                onNone: () => Effect.asVoid(request('ui', 'cancelNotify', key, () => $.ui.cancelNotify(key))),
                onSome: (body) => delivered('ui', 'notify', key, () => $.ui.notify(body, { key }))((sent) => (sent.isSent ? undefined : sent.reason)),
            }),
        Model: ({ text, agent, idle }) =>
            Match.value({ idle, agentId: Option.getOrUndefined(agent) }).pipe(
                Match.when({ idle: false }, ({ agentId }) => delivered('session', 'append', agentId ?? 'main', () => $.session.append({ message: { type: 'user', content: [{ type: 'text', text }] }, ...(agentId === undefined ? {} : { agentId }) }))(({ deny }) => deny)),
                Match.when({ agentId: Match.string }, ({ agentId }) => delivered('session', 'send', agentId, () => $.session.send({ to: { agentId }, text }))(({ reason }) => reason)),
                Match.orElse(() => delivered('prompt', 'submit', 'main', () => $.prompt.submit({ text }))(({ drop }) => drop)),
            ),
    });
}
const badArgument = (module: string, method: string, description: string) => PlatformError.badArgument({ module, method, description });
const held = <A, E>(exit: Exit.Exit<A, E>) => (Exit.isSuccess(exit) ? Duration.infinity : Duration.zero);
const anchor = (base: Path.Path, cwd: string) =>
    Path.make({
        ...base,
        resolve: (...segments) => base.resolve(cwd, ...segments),
        relative: (from, to) => base.relative(base.resolve(cwd, from), base.resolve(cwd, to)),
        toFileUrl: (file) => base.toFileUrl(file.endsWith('/') ? `${base.resolve(cwd, file)}/` : base.resolve(cwd, file)),
    });
const environment = (text: string) =>
    Record.fromEntries(
        Array.filterMap(
            text.split('\0'),
            Filter.fromPredicateOption((line) => Option.map(Option.fromNullishOr(variable.exec(line)?.groups), ({ name = '', value = '' }) => [name, value] as const)),
        ),
    );
const located = (variables: Readonly<Record<string, string>>, plugin: Readonly<Record<string, string>>, join: Path.Path['join']) =>
    Option.all({
        binary: Option.orElse(Record.get(variables, 'CLAUDE_CODE_SHELL'), () => Record.get(variables, 'SHELL')),
        folder: Option.orElse(Record.get(variables, 'CLAUDE_CONFIG_DIR'), () => Option.map(Record.get(variables, 'HOME'), (home) => join(home, '.claude'))),
        written: Option.map(Option.all([Record.get(plugin, 'CLAUDE_PLUGIN_DATA'), Record.get(plugin, 'CLAUDE_PID')]), ([data, pid]) => join(data, 'dialect', pid)),
    });
const unredacted = (word: string | Redacted.Redacted<string>) => (Redacted.isRedacted(word) ? Redacted.value(word) : word);
const outputQueue = (option: ChildProcess.CommandOptions['stdout']) =>
    Match.value(Predicate.isString(option) || Sink.isSink(option) ? option : (option?.stream ?? 'pipe')).pipe(
        Match.when('pipe', () => Effect.asSome(Queue.unbounded<Uint8Array, PlatformError.PlatformError | Cause.Done>())),
        Match.when('ignore', () => Effect.succeedNone),
        Match.orElse(() => Effect.fail(badArgument('ChildProcess', 'spawn', 'an output the engine cannot hand on: inherit, overlapped, or a sink'))),
    );
const inputText = (option: ChildProcess.CommandOptions['stdin']) =>
    Match.value(Predicate.isString(option) || Stream.isStream(option) ? option : (option?.stream ?? 'pipe')).pipe(
        Match.when(Stream.isStream, (input) => input.pipe(Stream.decodeText, Stream.mkString, Effect.asSome)),
        Match.when(Match.is('pipe', 'ignore'), () => Effect.succeedNone),
        Match.orElse(() => Effect.fail(badArgument('ChildProcess', 'spawn', 'an input the engine cannot hand on: inherit or overlapped'))),
    );
const outputStream = (queue: Option.Option<Output>) => Option.match(queue, { onNone: () => Stream.empty, onSome: Stream.fromQueue });
const spawnRequest = ({ command, args, options }: ChildProcess.StandardCommand, input: Option.Option<string>): ProcessSpawnRequest => {
    const given = Record.map(options.env ?? {}, Option.fromUndefinedOr);
    const set = Record.map(Record.getSomes(given), unredacted);
    const unset = Record.keys(Record.filter(given, Option.isNone));
    const [words, alone] = [[command, ...args.map(unredacted)], options.env !== undefined && options.extendEnv !== true];
    return {
        ...(alone || Array.isReadonlyArrayNonEmpty(unset) ? { argv: ['/usr/bin/env', ...(alone ? ['-i'] : unset.flatMap((name) => ['-u', name])), '--', ...Record.collect(set, (name, value) => `${name}=${value}`), ...words] } : { argv: words, env: set }),
        ...(options.cwd === undefined ? {} : { cwd: options.cwd }),
        ...Option.match(input, { onNone: () => ({}), onSome: (text) => ({ input: text }) }),
    };
};
const spawner = (start: (request: ProcessSpawnRequest) => Child) =>
    ChildProcessSpawner.make(
        Effect.fnUntraced(function* (command: ChildProcess.Command) {
            const standard = yield* Effect.succeed(command).pipe(
                Effect.filterOrFail(ChildProcess.isStandardCommand, () => badArgument('ChildProcess', 'spawn', 'a pipeline, as the engine spawns one argument vector')),
                Effect.filterOrFail(
                    ({ options }) => (options.shell ?? false) === false,
                    () => badArgument('ChildProcess', 'spawn', 'a shell, as the engine starts an argument vector alone'),
                ),
                Effect.filterOrFail(
                    ({ options }) => options.additionalFds === undefined,
                    () => badArgument('ChildProcess', 'spawn', 'descriptors past standard error'),
                ),
            );
            const [input, stdout, stderr, ended] = yield* Effect.all([inputText(standard.options.stdin), outputQueue(standard.options.stdout), outputQueue(standard.options.stderr), Deferred.make<void>()]);
            const spawned = yield* Effect.acquireRelease(
                Effect.sync(() => {
                    const child = start(spawnRequest(standard, input));
                    child.result.catch(Function.constVoid);
                    return child;
                }),
                (child) =>
                    Effect.sync(() => {
                        const stream: AsyncIterator<ProcessSpawnChunk, ProcessSpawnResult> = child;
                        stream.return?.().catch(Function.constVoid);
                    }),
            );
            const queues = { stdout, stderr };
            const pump = yield* Stream.fromAsyncIterable(spawned, systemError('process', 'spawn', standard.command)).pipe(
                Stream.runForEach((chunk) => Option.match(queues[chunk.stream], { onNone: () => Effect.void, onSome: (queue) => Queue.offer(queue, encoder.encode(chunk.text)) })),
                Effect.onExit((exit) =>
                    Effect.andThen(
                        Effect.forEach(Array.getSomes([stdout, stderr]), (queue) => (Exit.isFailure(exit) && !Cause.hasInterruptsOnly(exit.cause) ? Queue.failCause(queue, exit.cause) : Queue.end(queue)), { discard: true }),
                        Deferred.succeed(ended, undefined),
                    ),
                ),
                Effect.forkScoped({ startImmediately: true }),
            );
            return ChildProcessSpawner.makeHandle({
                pid: ChildProcessSpawner.ProcessId(0),
                exitCode: Effect.flatMap(
                    request('process', 'spawn', standard.command, () => spawned.result),
                    ({ code, signal }) => (code === null ? Effect.fail(systemError('process', 'spawn', standard.command)(new Error(`Process interrupted due to receipt of signal: '${String(signal)}'`))) : Effect.succeed(ChildProcessSpawner.ExitCode(code))),
                ),
                isRunning: Effect.map(Deferred.isDone(ended), (done) => !done),
                kill: () => Fiber.interrupt(pump),
                stdin: Sink.fail(badArgument('ChildProcess', 'stdin', 'writes after the start, as the engine takes standard input once, at spawn')),
                stdout: outputStream(stdout),
                stderr: outputStream(stderr),
                all: Stream.merge(outputStream(stdout), outputStream(stderr)),
                getInputFd: () => Sink.drain,
                getOutputFd: () => Stream.empty,
                unref: Effect.succeed(Effect.void),
            });
        }),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

function files($: EngineInterface) {
    const stat = (path: string) => request('fs', 'stat', path, () => $.fs.stat(path));
    const read = (path: string, range: Partial<Record<'offset' | 'length', number>>) =>
        Effect.map(
            request('fs', 'read', path, () => $.fs.read(path, { as: 'bytes', ...range })),
            ({ base64 }) => Uint8Array.fromBase64(base64),
        );
    const paged = (path: string) => Stream.paginate(0, (offset: number) => Effect.map(read(path, { offset, length: readLimit }), (bytes) => [[bytes], Option.liftPredicate(offset + readLimit, () => bytes.length === readLimit)] as const)).pipe(Stream.runCollect, Effect.map(Bytes.concat));
    return FileSystem.make(
        FileSystem.makeNoop({
            access: (path, options) =>
                request('fs', 'exists', path, () => $.fs.exists(path)).pipe(
                    Effect.filterOrFail(
                        () => options?.readable !== true && options?.writable !== true,
                        () => badArgument('FileSystem', 'access', 'a permission check, which the engine has no call for'),
                    ),
                    Effect.filterOrFail(Predicate.isTruthy, () => PlatformError.systemError({ _tag: 'NotFound', module: 'FileSystem', method: 'access', pathOrDescriptor: path })),
                    Effect.asVoid,
                ),
            readDirectory: (path, options) =>
                request('fs', 'list', path, () => $.fs.list(path)).pipe(
                    Effect.filterOrFail(
                        () => options?.recursive !== true,
                        () => badArgument('FileSystem', 'readDirectory', 'a listing of nested folders, as the engine lists one folder'),
                    ),
                    Effect.map((entries) => entries.map(({ name }) => name)),
                ),
            readDirectoryEntries: (path) =>
                Effect.map(
                    request('fs', 'list', path, () => $.fs.list(path)),
                    (entries) => entries.map(({ name, kind, isLink }) => ({ name, type: isLink ? ('SymbolicLink' as const) : kinds[kind] })),
                ),
            readFile: (path) => Effect.flatMap(stat(path), ({ kind }) => (kind === 'file' ? paged(path) : read(path, {}))),
            writeFile: (path, data, options) =>
                Effect.try({ try: () => decoder.decode(data), catch: () => badArgument('FileSystem', 'writeFile', 'bytes that are no UTF-8 text, as the engine writes text alone') }).pipe(
                    Effect.filterOrFail(
                        () => (options?.flag ?? 'w') === 'w' && options?.mode === undefined,
                        () => badArgument('FileSystem', 'writeFile', 'a flag or mode, as the engine writes a whole file under its own mode'),
                    ),
                    Effect.flatMap((text) => request('fs', 'write', path, () => $.fs.write(path, text))),
                ),
            stat: (path) =>
                Effect.map(stat(path), (found) => ({
                    type: kinds[found.kind],
                    mtime: DateTime.makeUnsafe(found.mtimeMs).pipe(DateTime.toDateUtc, Option.some),
                    atime: Option.none(),
                    birthtime: Option.none(),
                    dev: 0n,
                    ino: Option.none(),
                    mode: 0,
                    nlink: Option.none(),
                    uid: Option.none(),
                    gid: Option.none(),
                    rdev: Option.none(),
                    size: ByteSize.bytes(found.size),
                    blksize: Option.none(),
                    blocks: Option.none(),
                })),
            realPath: (path) =>
                Effect.flatMap(
                    request('fs', 'stat', path, () => $.fs.stat(path, { resolve: true })),
                    ({ realPath }) => Effect.fromOption(Option.fromUndefinedOr(realPath), () => PlatformError.systemError({ _tag: 'NotFound', module: 'FileSystem', method: 'realPath', pathOrDescriptor: path })),
                ),
        }),
    );
}

function session($: EngineInterface) {
    const listed = (agent: string) =>
        Effect.map(
            request('agent', 'list', agent, () => $.agent.list()),
            Array.findFirst(({ id }) => id === agent),
        );
    const folder = request('session', 'cwd', 'session', () => $.session.cwd());
    const entries = (directory: string, join: Path.Path['join']) =>
        request('fs', 'list', directory, () => $.fs.list(directory)).pipe(
            Effect.flatMap(
                Effect.forEach((entry) =>
                    entry.isLink
                        ? Effect.orElseSucceed(
                              Effect.map(
                                  request('fs', 'stat', entry.name, () => $.fs.stat(join(directory, entry.name))),
                                  ({ kind, mtimeMs }) => ({ ...entry, kind, mtimeMs }),
                              ),
                              () => entry,
                          )
                        : Effect.succeed(entry),
                ),
            ),
            Effect.map(Array.filter(({ kind }) => kind === 'file')),
            Effect.orElseSucceed((): FsEntry[] => []),
        );
    const stamped = (file: string) =>
        Effect.option(
            Effect.map(
                request('fs', 'stat', file, () => $.fs.stat(file)),
                ({ mtimeMs }) => ({ file, mtimeMs }),
            ),
        );
    return Layer.effect(
        Session,
        Effect.gen(function* () {
            const [shell, fs, processes, path, watchers] = yield* Effect.all([Shell, FileSystem.FileSystem, ChildProcessSpawner.ChildProcessSpawner, Path.Path, FiberSet.make()]);
            const facts = yield* Cache.makeWith(
                Effect.fnUntraced(function* () {
                    const [text, plugin] = yield* Effect.all([processes.string(ChildProcess.make('env', ['-0'])), request('process', 'hookEnv', 'CLAUDE_PLUGIN_DATA', () => $.process.hookEnv())]);
                    const variables = environment(text);
                    const found = located(variables, plugin, path.join);
                    return { variables, ...(yield* Effect.fromOption(found, () => badArgument('Session', 'environment', 'a process that names no shell, no home, or no plugin data folder'))) };
                }),
                { capacity: 1, timeToLive: held },
            );
            const probes = yield* Cache.makeWith(
                Effect.fnUntraced(function* ({ sourced }: { readonly sourced: string; readonly stamps: readonly number[] }) {
                    const { variables, binary, written } = yield* Cache.get(facts, undefined);
                    const file = path.join(written, `${Hash.string(sourced) >>> 0}.sh`);
                    yield* Effect.when(fs.writeFileString(file, sourced), Effect.succeed(sourced !== ''));
                    return yield* shell.dialect({ shell: binary, environment: variables, timeout: Duration.infinity, ...(sourced === '' ? {} : { source: file }) });
                }),
                { capacity: 4, timeToLive: held },
            );
            const probed = Effect.fnUntraced(function* (cwd: string) {
                const [{ variables, binary, folder: home }, id] = yield* Effect.all([Cache.get(facts, undefined), request('session', 'id', 'session', () => $.session.id())]);
                const [snapshots, scripts, launched] = yield* Effect.all([entries(path.join(home, 'shell-snapshots'), path.join), entries(path.join(home, 'session-env', id), path.join), Effect.forEach(Option.toArray(Record.get(variables, 'CLAUDE_ENV_FILE')), stamped)]);
                const named = snapshots.filter(({ name }) => name.startsWith(`snapshot-${path.basename(binary)}-`));
                const sources = [
                    ...(Array.isReadonlyArrayNonEmpty(named) ? [Array.max(named, byTime)] : []).map(({ name, mtimeMs }) => ({ file: path.join(home, 'shell-snapshots', name), mtimeMs })),
                    ...Array.getSomes(launched),
                    ...scripts
                        .filter(({ name }) => hookScript.test(name))
                        .toSorted((left, right) => collator.compare(left.name, right.name))
                        .map(({ name, mtimeMs }) => ({ file: path.join(home, 'session-env', id, name), mtimeMs })),
                ];
                const quoted = yield* Effect.forEach(sources, ({ file }) => Effect.fromResult(CommandLine.quote(file)));
                return { dialect: yield* Cache.get(probes, { sourced: quoted.map((word) => `. ${word}`).join('\n'), stamps: sources.map(({ mtimeMs }) => mtimeMs) }), cwd };
            });
            yield* Effect.addFinalizer(() => Effect.andThen(FiberSet.clear(watchers), Effect.flatMap(Cache.getSuccess(facts, undefined), Option.match({ onNone: () => Effect.void, onSome: ({ written }) => Effect.ignore(processes.exitCode(ChildProcess.make('rm', ['-rf', written]))) }))));
            return {
                acting: (agent) => Effect.map(Option.match(agent, { onNone: () => folder, onSome: (id) => Effect.flatMap(listed(id), Option.match({ onNone: () => folder, onSome: ({ cwd }) => Effect.succeed(cwd) })) }), (cwd) => ({ environment: probed(cwd), path: anchor(path, cwd) })),
                resting: (agent) =>
                    Effect.map(
                        listed(agent),
                        Option.flatMap(({ status }) => resting[status]),
                    ),
                watch: (watcher) => Effect.asVoid(FiberSet.run(watchers, watcher)),
            };
        }),
    ).pipe(
        Layer.provideMerge(
            Layer.effect(
                Shell,
                Effect.flatMap(
                    request('wasm', 'compile', './main.wasm', () => $.wasm.compile('./main.wasm')),
                    (module) => Shell.make({ processor: getProcessor(module) }),
                ),
            ),
        ),
    );
}

function runtime<A, E>($: EngineInterface, layer: Layer.Layer<A, E, FileSystem.FileSystem | ChildProcessSpawner.ChildProcessSpawner | Path.Path>) {
    return ManagedRuntime.make(
        layer.pipe(
            Layer.provideMerge(
                Layer.mergeAll(
                    Layer.succeed(FileSystem.FileSystem, files($)),
                    Layer.succeed(
                        ChildProcessSpawner.ChildProcessSpawner,
                        spawner((asked) => $.process.spawn(asked)),
                    ),
                    Path.layer,
                    Layer.succeed(Scheduler.Scheduler, new TimeSliced()),
                    Logger.layer([Logger.map(Logger.formatSimple, (line) => $.ui.log(line, { to: 'debug' }))]),
                ),
            ),
        ),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Notice, request, runtime, say, Session, session };
