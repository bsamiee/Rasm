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

const limit = 4 * 1024 ** 2;
const slice = 8;
const errno = / failed(?: to start)?: (?<code>E[A-Z]+)/u;
const reasons: Readonly<Record<string, PlatformError.SystemErrorTag>> = { ENOENT: 'NotFound', EACCES: 'PermissionDenied', EPERM: 'PermissionDenied', EEXIST: 'AlreadyExists', EISDIR: 'BadResource', ENOTDIR: 'BadResource', ELOOP: 'BadResource', EBUSY: 'Busy' };
const kinds = { file: 'File', dir: 'Directory', other: 'Unknown' } as const satisfies Record<FsStat['kind'], FileSystem.File.Type>;
const hooked = /-hook-\d+\.sh$/u;

// --- [SERVICES] ------------------------------------------------------------------------

class Sliced extends Scheduler.MixedScheduler {
    begun = 0;

    override shouldYield(fiber: Parameters<Scheduler.Scheduler['shouldYield']>[0]) {
        this.begun = fiber.currentOpCount <= 2 ? fiber.getRef(Clock.Clock).currentTimeMillisUnsafe() : this.begun;
        return super.shouldYield(fiber) || ((fiber.currentOpCount & 15) === 0 && fiber.getRef(Clock.Clock).currentTimeMillisUnsafe() - this.begun >= slice);
    }
}

class Dialects extends Context.Service<Dialects, { readonly dialect: Effect.Effect<Effect.Success<Probe>, Effect.Error<Probe> | PlatformError.PlatformError | CommandLine.CommandLineError> }>()('@rasm/policy/claude/runtime/Dialects') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const utf8 = new TextEncoder();
const strict = new TextDecoder('utf-8', { fatal: true });
const named = new Intl.Collator('en', { numeric: true });
const byTime = Order.mapInput(Order.Number, ({ mtimeMs }: FsEntry) => mtimeMs);
const failed = (module: string, method: string, subject: string) => (cause: unknown) =>
    PlatformError.systemError({
        _tag: Option.fromNullishOr(errno.exec(Predicate.isError(cause) ? cause.message : '')?.groups?.['code']).pipe(
            Option.flatMap((code) => Record.get(reasons, code)),
            Option.getOrElse((): PlatformError.SystemErrorTag => 'Unknown'),
        ),
        module,
        method,
        pathOrDescriptor: subject,
        cause,
    });
const refused = (module: string, method: string, description: string) => PlatformError.badArgument({ module, method, description });
const unwrapped = (word: string | Redacted.Redacted<string>) => (Redacted.isRedacted(word) ? Redacted.value(word) : word);
const piped = (option: ChildProcess.CommandOptions['stdout']) =>
    Match.value(Predicate.isString(option) || Sink.isSink(option) ? option : (option?.stream ?? 'pipe')).pipe(
        Match.when('pipe', () => Effect.asSome(Queue.unbounded<Uint8Array, PlatformError.PlatformError | Cause.Done>())),
        Match.when('ignore', () => Effect.succeedNone),
        Match.orElse(() => Effect.fail(refused('ChildProcess', 'spawn', 'an output the engine cannot hand on: inherit, overlapped, or a sink'))),
    );
const fed = (option: ChildProcess.CommandOptions['stdin']) =>
    Match.value(Predicate.isString(option) || Stream.isStream(option) ? option : (option?.stream ?? 'pipe')).pipe(
        Match.when(Stream.isStream, (input) => Effect.asSome(Stream.mkString(Stream.decodeText(input)))),
        Match.when(Match.is('pipe', 'ignore'), () => Effect.succeedNone),
        Match.orElse(() => Effect.fail(refused('ChildProcess', 'spawn', 'an input the engine cannot hand on: inherit or overlapped'))),
    );
const drained = (queue: Option.Option<Output>) => Option.match(queue, { onNone: () => Stream.empty, onSome: Stream.fromQueue });
const fenced = ({ command, args, options }: ChildProcess.StandardCommand) => {
    const given = Record.map(options.env ?? {}, Option.fromUndefinedOr);
    const set = Record.map(Record.getSomes(given), unwrapped);
    const unset = Record.keys(Record.filter(given, Option.isNone));
    const words = [command, ...args.map(unwrapped)];
    return options.extendEnv === false || Array.isReadonlyArrayNonEmpty(unset) ? { argv: ['/usr/bin/env', ...(options.extendEnv === false ? ['-i'] : unset.flatMap((name) => ['-u', name])), ...Record.collect(set, (name, value) => `${name}=${value}`), ...words] } : { argv: words, env: set };
};
const spawner = (start: (request: ProcessSpawnRequest) => HookStream<ProcessSpawnChunk, ProcessSpawnResult>) =>
    ChildProcessSpawner.make(
        Effect.fnUntraced(function* (command: ChildProcess.Command) {
            const standard = yield* Effect.succeed(command).pipe(
                Effect.filterOrFail(ChildProcess.isStandardCommand, () => refused('ChildProcess', 'spawn', 'a pipeline, as the engine spawns one argument vector')),
                Effect.filterOrFail(
                    ({ options }) => (options.shell ?? false) === false,
                    () => refused('ChildProcess', 'spawn', 'a shell, as the engine starts an argument vector alone'),
                ),
                Effect.filterOrFail(
                    ({ options }) => options.additionalFds === undefined,
                    () => refused('ChildProcess', 'spawn', 'descriptors past standard error'),
                ),
            );
            const [input, stdout, stderr, ended] = yield* Effect.all([fed(standard.options.stdin), piped(standard.options.stdout), piped(standard.options.stderr), Deferred.make<void>()]);
            const spawned = start({
                ...fenced(standard),
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
            const pump = yield* Stream.fromAsyncIterable(spawned, failed('ChildProcess', 'spawn', standard.command)).pipe(
                Stream.runForEach((chunk) => Effect.forEach(Option.toArray(queues[chunk.stream]), (queue) => Queue.offer(queue, utf8.encode(chunk.text)), { discard: true })),
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
                exitCode: Effect.tryPromise({ try: () => spawned.result, catch: failed('ChildProcess', 'exitCode', standard.command) }).pipe(
                    Effect.flatMap(({ code, signal }) => (code === null ? Effect.fail(failed('ChildProcess', 'exitCode', standard.command)(new Error(`Process interrupted due to receipt of signal: '${String(signal)}'`))) : Effect.succeed(ChildProcessSpawner.ExitCode(code)))),
                ),
                isRunning: Effect.map(Deferred.isDone(ended), (done) => !done),
                kill: () => Fiber.interrupt(pump),
                stdin: Sink.fail(refused('ChildProcess', 'stdin', 'writes after the start, as the engine takes standard input once, at spawn')),
                stdout: drained(stdout),
                stderr: drained(stderr),
                all: Stream.merge(drained(stdout), drained(stderr)),
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
                Effect.tryPromise({ try: () => $.fs.exists(path), catch: failed('FileSystem', 'access', path) }).pipe(
                    Effect.filterOrFail(
                        () => options?.readable !== true && options?.writable !== true,
                        () => refused('FileSystem', 'access', 'a permission check, which the engine has no call for'),
                    ),
                    Effect.filterOrFail(Predicate.isTruthy, () => PlatformError.systemError({ _tag: 'NotFound', module: 'FileSystem', method: 'access', pathOrDescriptor: path })),
                    Effect.asVoid,
                ),
            readDirectory: (path, options) =>
                Effect.tryPromise({ try: () => $.fs.list(path), catch: failed('FileSystem', 'readDirectory', path) }).pipe(
                    Effect.filterOrFail(
                        () => options?.recursive !== true,
                        () => refused('FileSystem', 'readDirectory', 'a listing of nested folders, as the engine lists one folder'),
                    ),
                    Effect.map((entries) => entries.map(({ name }) => name)),
                ),
            readFile: (path) =>
                Stream.paginate(0, (offset: number) =>
                    Effect.tryPromise({ try: () => $.fs.read(path, { as: 'bytes', offset, length: limit }), catch: failed('FileSystem', 'readFile', path) }).pipe(
                        Effect.map(({ base64 }) => Uint8Array.fromBase64(base64)),
                        Effect.map((bytes) => [[bytes], Option.liftPredicate(offset + limit, () => bytes.length === limit)] as const),
                    ),
                ).pipe(Stream.runCollect, Effect.map(Bytes.concat)),
            writeFile: (path, data, options) =>
                Effect.try({ try: () => strict.decode(data), catch: () => refused('FileSystem', 'writeFile', 'bytes that are no UTF-8 text, as the engine writes text alone') }).pipe(
                    Effect.filterOrFail(
                        () => (options?.flag ?? 'w') === 'w' && options?.mode === undefined,
                        () => refused('FileSystem', 'writeFile', 'a flag or mode, as the engine writes a whole file under its own mode'),
                    ),
                    Effect.flatMap((text) => Effect.tryPromise({ try: () => $.fs.write(path, text), catch: failed('FileSystem', 'writeFile', path) })),
                ),
            stat: (path) =>
                Effect.tryPromise({ try: () => $.fs.stat(path), catch: failed('FileSystem', 'stat', path) }).pipe(
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
                Effect.tryPromise({ try: () => $.fs.stat(path, { resolve: true }), catch: failed('FileSystem', 'realPath', path) }).pipe(
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
        Layer.succeed(Scheduler.Scheduler, new Sliced()),
    );
    const host = Layer.effect(
        Shell,
        Effect.flatMap(
            Effect.promise(() => $.wasm.compile('./main.wasm')),
            (module) => Shell.make({ processor: getProcessor(module) }),
        ),
    ).pipe(Layer.provideMerge(platform));
    const dialects = Layer.effect(
        Dialects,
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
                Effect.tryPromise({ try: () => $.process.hookEnv(), catch: failed('Process', 'hookEnv', 'CLAUDE_PLUGIN_DATA') }),
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
            const listed = (directory: string) => Effect.orElseSucceed(Effect.tryPromise({ try: () => $.fs.list(directory), catch: failed('FileSystem', 'readDirectory', directory) }), () => []);
            return {
                dialect: Effect.gen(function* () {
                    const session = yield* Effect.promise(() => $.session.id());
                    const [snapshots, scripts, launched] = yield* Effect.all([listed(path.join(folder, 'shell-snapshots')), listed(path.join(folder, 'session-env', session)), Effect.filter(Option.toArray(Record.get(environment, 'CLAUDE_ENV_FILE')), (file) => fs.exists(file))]);
                    const snapshot = Array.match(
                        snapshots.filter(({ kind, name }) => kind === 'file' && name.startsWith(`snapshot-${path.basename(binary)}-`)),
                        {
                            onEmpty: Option.none,
                            onNonEmpty: (found) => Option.some(Array.max(found, byTime)),
                        },
                    );
                    const hooks = scripts.filter(({ kind, name }) => kind === 'file' && hooked.test(name)).toSorted((left, right) => named.compare(left.name, right.name));
                    const sources = [...Option.toArray(Option.map(snapshot, ({ name }) => path.join(folder, 'shell-snapshots', name))), ...launched, ...hooks.map(({ name }) => path.join(folder, 'session-env', session, name))];
                    const quoted = yield* Effect.forEach(sources, (source) => Effect.fromResult(CommandLine.quote(source)));
                    return yield* Cache.get(probes, { sourced: quoted.map((word) => `. ${word}`).join('\n'), stamps: hooks.map(({ mtimeMs }) => mtimeMs) });
                }),
            };
        }),
    );
    return ManagedRuntime.make(dialects.pipe(Layer.provideMerge(host)));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Dialects, runtime };
