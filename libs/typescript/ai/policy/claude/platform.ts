import { Shell } from '@rasm/shell';
import type { EngineInterface, FsEntry, FsStat, HookStream, ProcessSpawnChunk, ProcessSpawnRequest, ProcessSpawnResult } from 'claude-code';
import { Array, Bytes, ByteSize, Cache, type Cause, Clock, Context, DateTime, Deferred, Duration, Effect, Exit, Fiber, FileSystem, Hash, Layer, ManagedRuntime, Match, Option, Order, Path, PlatformError, Predicate, Queue, Record, Redacted, Scheduler, Sink, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner, CommandLine } from 'effect/process';
import 'sh-syntax/vendors/wasm_exec';
import { getProcessor } from 'sh-syntax/sync';

// --- [TYPES] ---------------------------------------------------------------------------

type Probe = ReturnType<Shell['Service']['dialect']>;
type Output = Queue.Queue<Uint8Array, PlatformError.PlatformError | Cause.Done>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const readLimit = 4 * 1024 ** 2;
const slice = 8;
const errno = / failed(?: to start)?: (?<code>E[A-Z]+)/u;
const errnoTags: Readonly<Record<string, PlatformError.SystemErrorTag>> = { ENOENT: 'NotFound', EACCES: 'PermissionDenied', EPERM: 'PermissionDenied', EEXIST: 'AlreadyExists', EISDIR: 'BadResource', ENOTDIR: 'BadResource', ELOOP: 'BadResource', EBUSY: 'Busy' };
const kinds = { file: 'File', dir: 'Directory', other: 'Unknown' } as const satisfies Record<FsStat['kind'], FileSystem.File.Type>;
const hookScript = /-hook-\d+\.sh$/u;

// --- [SERVICES] ------------------------------------------------------------------------

class TimeSliced extends Scheduler.MixedScheduler {
    sliceStart = 0;

    override shouldYield(fiber: Parameters<Scheduler.Scheduler['shouldYield']>[0]) {
        this.sliceStart = fiber.currentOpCount <= 2 ? fiber.getRef(Clock.Clock).currentTimeMillisUnsafe() : this.sliceStart;
        return super.shouldYield(fiber) || ((fiber.currentOpCount & 15) === 0 && fiber.getRef(Clock.Clock).currentTimeMillisUnsafe() - this.sliceStart >= slice);
    }
}

class Session extends Context.Service<
    Session,
    {
        readonly environment: Effect.Effect<{ readonly dialect: Effect.Success<Probe>; readonly cwd: string }, Effect.Error<Probe> | PlatformError.PlatformError | CommandLine.CommandLineError>;
        readonly resting: (agent: Option.Option<string>) => Effect.Effect<boolean>;
        readonly tell: (agent: Option.Option<string>, text: string) => Effect.Effect<void>;
    }
>()('@rasm/policy/claude/platform/Session') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const encoder = new TextEncoder();
const decoder = new TextDecoder('utf-8', { fatal: true });
const collator = new Intl.Collator('en', { numeric: true });
const byTime = Order.mapInput(Order.Number, ({ mtimeMs }: FsEntry) => mtimeMs);
const systemError = (module: string, method: string, subject: string) => (cause: unknown) =>
    PlatformError.systemError({
        _tag: Option.fromNullishOr(errno.exec(Predicate.isError(cause) ? cause.message : '')?.groups?.['code']).pipe(
            Option.flatMap((code) => Record.get(errnoTags, code)),
            Option.getOrElse((): PlatformError.SystemErrorTag => 'Unknown'),
        ),
        module,
        method,
        pathOrDescriptor: subject,
        cause,
    });
const badArgument = (module: string, method: string, description: string) => PlatformError.badArgument({ module, method, description });
const unredacted = (word: string | Redacted.Redacted<string>) => (Redacted.isRedacted(word) ? Redacted.value(word) : word);
const outputQueue = (option: ChildProcess.CommandOptions['stdout']) =>
    Match.value(Predicate.isString(option) || Sink.isSink(option) ? option : (option?.stream ?? 'pipe')).pipe(
        Match.when('pipe', () => Effect.asSome(Queue.unbounded<Uint8Array, PlatformError.PlatformError | Cause.Done>())),
        Match.when('ignore', () => Effect.succeedNone),
        Match.orElse(() => Effect.fail(badArgument('ChildProcess', 'spawn', 'an output the engine cannot hand on: inherit, overlapped, or a sink'))),
    );
const inputText = (option: ChildProcess.CommandOptions['stdin']) =>
    Match.value(Predicate.isString(option) || Stream.isStream(option) ? option : (option?.stream ?? 'pipe')).pipe(
        Match.when(Stream.isStream, (input) => Effect.asSome(Stream.mkString(Stream.decodeText(input)))),
        Match.when(Match.is('pipe', 'ignore'), () => Effect.succeedNone),
        Match.orElse(() => Effect.fail(badArgument('ChildProcess', 'spawn', 'an input the engine cannot hand on: inherit or overlapped'))),
    );
const outputStream = (queue: Option.Option<Output>) => Option.match(queue, { onNone: () => Stream.empty, onSome: Stream.fromQueue });
const spawnRequest = ({ command, args, options }: ChildProcess.StandardCommand) => {
    const given = Record.map(options.env ?? {}, Option.fromUndefinedOr);
    const set = Record.map(Record.getSomes(given), unredacted);
    const unset = Record.keys(Record.filter(given, Option.isNone));
    const words = [command, ...args.map(unredacted)];
    return options.extendEnv === false || Array.isReadonlyArrayNonEmpty(unset) ? { argv: ['/usr/bin/env', ...(options.extendEnv === false ? ['-i'] : unset.flatMap((name) => ['-u', name])), ...Record.collect(set, (name, value) => `${name}=${value}`), ...words] } : { argv: words, env: set };
};
const spawner = (start: (request: ProcessSpawnRequest) => HookStream<ProcessSpawnChunk, ProcessSpawnResult>) =>
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
            const spawned = start({
                ...spawnRequest(standard),
                ...Option.getOrElse(
                    Option.map(Option.fromUndefinedOr(standard.options.cwd), (cwd) => ({ cwd })),
                    () => ({}),
                ),
                ...Option.getOrElse(
                    Option.map(input, (text) => ({ input: text })),
                    () => ({}),
                ),
            });
            const queues = { stdout, stderr };
            const pump = yield* Stream.fromAsyncIterable(spawned, systemError('ChildProcess', 'spawn', standard.command)).pipe(
                Stream.runForEach((chunk) => Effect.forEach(Option.toArray(queues[chunk.stream]), (queue) => Queue.offer(queue, encoder.encode(chunk.text)), { discard: true })),
                Effect.onExit((exit) =>
                    Effect.andThen(
                        Effect.forEach([...Option.toArray(stdout), ...Option.toArray(stderr)], (queue) => (Exit.isSuccess(exit) ? Queue.end(queue) : Queue.failCause(queue, exit.cause)), { discard: true }),
                        Deferred.succeed(ended, undefined),
                    ),
                ),
                Effect.forkScoped,
            );
            return ChildProcessSpawner.makeHandle({
                pid: ChildProcessSpawner.ProcessId(0),
                exitCode: Effect.tryPromise({ try: () => spawned.result, catch: systemError('ChildProcess', 'exitCode', standard.command) }).pipe(
                    Effect.flatMap(({ code, signal }) => (code === null ? Effect.fail(systemError('ChildProcess', 'exitCode', standard.command)(new Error(`Process interrupted due to receipt of signal: '${String(signal)}'`))) : Effect.succeed(ChildProcessSpawner.ExitCode(code)))),
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

function runtime($: EngineInterface) {
    const files = FileSystem.make(
        FileSystem.makeNoop({
            access: (path, options) =>
                Effect.tryPromise({ try: () => $.fs.exists(path), catch: systemError('FileSystem', 'access', path) }).pipe(
                    Effect.filterOrFail(
                        () => options?.readable !== true && options?.writable !== true,
                        () => badArgument('FileSystem', 'access', 'a permission check, which the engine has no call for'),
                    ),
                    Effect.filterOrFail(Predicate.isTruthy, () => PlatformError.systemError({ _tag: 'NotFound', module: 'FileSystem', method: 'access', pathOrDescriptor: path })),
                    Effect.asVoid,
                ),
            readDirectory: (path, options) =>
                Effect.tryPromise({ try: () => $.fs.list(path), catch: systemError('FileSystem', 'readDirectory', path) }).pipe(
                    Effect.filterOrFail(
                        () => options?.recursive !== true,
                        () => badArgument('FileSystem', 'readDirectory', 'a listing of nested folders, as the engine lists one folder'),
                    ),
                    Effect.map((entries) => entries.map(({ name }) => name)),
                ),
            readFile: (path) =>
                Stream.paginate(0, (offset: number) =>
                    Effect.tryPromise({ try: () => $.fs.read(path, { as: 'bytes', offset, length: readLimit }), catch: systemError('FileSystem', 'readFile', path) }).pipe(
                        Effect.map(({ base64 }) => Uint8Array.fromBase64(base64)),
                        Effect.map((bytes) => [[bytes], Option.liftPredicate(offset + readLimit, () => bytes.length === readLimit)] as const),
                    ),
                ).pipe(Stream.runCollect, Effect.map(Bytes.concat)),
            writeFile: (path, data, options) =>
                Effect.try({ try: () => decoder.decode(data), catch: () => badArgument('FileSystem', 'writeFile', 'bytes that are no UTF-8 text, as the engine writes text alone') }).pipe(
                    Effect.filterOrFail(
                        () => (options?.flag ?? 'w') === 'w' && options?.mode === undefined,
                        () => badArgument('FileSystem', 'writeFile', 'a flag or mode, as the engine writes a whole file under its own mode'),
                    ),
                    Effect.flatMap((text) => Effect.tryPromise({ try: () => $.fs.write(path, text), catch: systemError('FileSystem', 'writeFile', path) })),
                ),
            stat: (path) =>
                Effect.tryPromise({ try: () => $.fs.stat(path), catch: systemError('FileSystem', 'stat', path) }).pipe(
                    Effect.map((found) => ({
                        type: kinds[found.kind],
                        mtime: Option.some(DateTime.toDateUtc(DateTime.makeUnsafe(found.mtimeMs))),
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
                ),
            realPath: (path) =>
                Effect.tryPromise({ try: () => $.fs.stat(path, { resolve: true }), catch: systemError('FileSystem', 'realPath', path) }).pipe(
                    Effect.flatMap(({ realPath }) => Effect.fromOption(Option.fromUndefinedOr(realPath))),
                    Effect.catchTag('NoSuchElementError', () => Effect.fail(PlatformError.systemError({ _tag: 'NotFound', module: 'FileSystem', method: 'realPath', pathOrDescriptor: path }))),
                ),
        }),
    );
    const platform = Layer.mergeAll(
        Layer.succeed(FileSystem.FileSystem, files),
        Layer.succeed(
            ChildProcessSpawner.ChildProcessSpawner,
            spawner((request) => $.process.spawn(request)),
        ),
        Path.layer,
        Layer.succeed(Scheduler.Scheduler, new TimeSliced()),
    );
    const shells = Layer.effect(
        Shell,
        Effect.flatMap(
            Effect.promise(() => $.wasm.compile('./main.wasm')),
            (module) => Shell.make({ processor: getProcessor(module) }),
        ),
    ).pipe(Layer.provideMerge(platform));
    const session = Layer.effect(
        Session,
        Effect.gen(function* () {
            const [shell, fs, processes, path] = yield* Effect.all([Shell, FileSystem.FileSystem, ChildProcessSpawner.ChildProcessSpawner, Path.Path]);
            const [environment, plugin] = yield* Effect.all([
                Effect.map(processes.string(ChildProcess.make('env', ['-0'])), (text) =>
                    Record.fromEntries(
                        text.split('\0').flatMap((line) => {
                            const at = line.indexOf('=');
                            return at > 0 ? [[line.slice(0, at), line.slice(at + 1)] as const] : [];
                        }),
                    ),
                ),
                Effect.tryPromise({ try: () => $.process.hookEnv(), catch: systemError('Process', 'hookEnv', 'CLAUDE_PLUGIN_DATA') }),
            ]);
            const { binary, folder, written } = yield* Effect.fromOption(
                Option.all({
                    binary: Option.orElse(Record.get(environment, 'CLAUDE_CODE_SHELL'), () => Record.get(environment, 'SHELL')),
                    folder: Option.orElse(Record.get(environment, 'CLAUDE_CONFIG_DIR'), () => Option.map(Record.get(environment, 'HOME'), (home) => path.join(home, '.claude'))),
                    written: Option.map(Option.all([Record.get(plugin, 'CLAUDE_PLUGIN_DATA'), Record.get(plugin, 'CLAUDE_PID')]), ([data, pid]) => path.join(data, 'dialect', pid)),
                }),
            );
            yield* Effect.addFinalizer(() => Effect.ignore(processes.exitCode(ChildProcess.make('rm', ['-rf', written]))));
            const probes = yield* Cache.make({
                capacity: 4,
                lookup: Effect.fnUntraced(
                    function* ({ sourced }: { readonly sourced: string; readonly stamps: readonly number[] }) {
                        const file = path.join(written, `${Hash.string(sourced) >>> 0}.sh`);
                        yield* Effect.when(fs.writeFileString(file, sourced), Effect.succeed(sourced !== ''));
                        return yield* shell.dialect({ shell: binary, environment, timeout: Duration.infinity, ...(sourced === '' ? {} : { source: file }) });
                    },
                    Effect.provideService(ChildProcessSpawner.ChildProcessSpawner, processes),
                ),
            });
            const list = (directory: string) => Effect.orElseSucceed(Effect.tryPromise({ try: () => $.fs.list(directory), catch: systemError('FileSystem', 'readDirectory', directory) }), () => []);
            return {
                resting: Option.match({
                    onNone: () => Effect.succeed(true),
                    onSome: (agent) =>
                        Effect.map(
                            Effect.promise(() => $.agent.list()),
                            (agents) => !agents.some(({ id, status }) => id === agent && status === 'running'),
                        ),
                }),
                tell: (agent, text) => Effect.asVoid(Effect.promise((): Promise<unknown> => Option.match(agent, { onNone: () => $.prompt.submit({ text }), onSome: (agentId) => $.session.send({ to: { agentId }, text }) }))),
                environment: Effect.gen(function* () {
                    const [id, cwd] = yield* Effect.promise(() => Promise.all([$.session.id(), $.session.cwd()]));
                    const [snapshots, scripts, launched] = yield* Effect.all([list(path.join(folder, 'shell-snapshots')), list(path.join(folder, 'session-env', id)), Effect.filter(Option.toArray(Record.get(environment, 'CLAUDE_ENV_FILE')), (file) => fs.exists(file))]);
                    const snapshot = Array.match(
                        snapshots.filter(({ kind, name }) => kind === 'file' && name.startsWith(`snapshot-${path.basename(binary)}-`)),
                        {
                            onEmpty: Option.none,
                            onNonEmpty: (found) => Option.some(Array.max(found, byTime)),
                        },
                    );
                    const hooks = scripts.filter(({ kind, name }) => kind === 'file' && hookScript.test(name)).toSorted((left, right) => collator.compare(left.name, right.name));
                    const sources = [...Option.toArray(Option.map(snapshot, ({ name }) => path.join(folder, 'shell-snapshots', name))), ...launched, ...hooks.map(({ name }) => path.join(folder, 'session-env', id, name))];
                    const quoted = yield* Effect.forEach(sources, (source) => Effect.fromResult(CommandLine.quote(source)));
                    return { dialect: yield* Cache.get(probes, { sourced: quoted.map((word) => `. ${word}`).join('\n'), stamps: hooks.map(({ mtimeMs }) => mtimeMs) }), cwd };
                }),
            };
        }),
    );
    return ManagedRuntime.make(session.pipe(Layer.provideMerge(shells)));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { runtime, Session };
