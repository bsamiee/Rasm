import type { EngineInterface, EventName, FsStat, FsStatOptions } from 'claude-code';
import { Array, Bytes, ByteSize, Cause, Clock, ConfigProvider, DateTime, Effect, Exit, Fiber, FileSystem, Formatter, Function, Layer, Logger, Match, Option, Path, PlatformError, Predicate, Queue, Record, Redacted, Result, Scheduler, Sink, Stream, String } from 'effect';
import { KeyValueStore } from 'effect/persistence';
import { ChildProcess, ChildProcessSpawner, type CommandLine } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const READ_LIMIT = 4 * 1024 ** 2;
const ERRNO_TAGS: Readonly<Record<string, PlatformError.SystemErrorTag>> = { ENOENT: 'NotFound', EACCES: 'PermissionDenied', EPERM: 'PermissionDenied', EEXIST: 'AlreadyExists', EISDIR: 'BadResource', ENOTDIR: 'BadResource', ELOOP: 'BadResource', EBUSY: 'Busy' };
const KINDS = { file: 'File', dir: 'Directory', other: 'Unknown' } as const satisfies Record<FsStat['kind'], FileSystem.File.Type>;

// --- [SERVICES] ------------------------------------------------------------------------

class TimeSliced extends Scheduler.MixedScheduler {
    #fiber: Fiber.Fiber<unknown, unknown> | undefined = undefined;
    #ops = 0;
    #start = 0;

    override shouldYield(fiber: Fiber.Fiber<unknown, unknown>) {
        this.#start = fiber === this.#fiber && fiber.currentOpCount > this.#ops ? this.#start : fiber.getRef(Clock.Clock).currentTimeMillisUnsafe();
        const due = super.shouldYield(fiber) || ((fiber.currentOpCount & 15) === 0 && fiber.getRef(Clock.Clock).currentTimeMillisUnsafe() - this.#start >= 8);
        this.#fiber = due ? undefined : fiber;
        this.#ops = fiber.currentOpCount;
        return due;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const engineError = (event: EventName, subject: string) => (cause: unknown) => {
    const [module, ...method] = String.split(event, '.');
    const description = Predicate.isError(cause) ? cause.message : Formatter.format(cause);
    return PlatformError.systemError({
        _tag: Option.fromNullishOr(/ failed(?: to start)?: (?<code>E[A-Z]+)/u.exec(description)?.groups?.['code']).pipe(
            Option.flatMap((code) => Record.get(ERRNO_TAGS, code)),
            Option.getOrElse((): PlatformError.SystemErrorTag => 'Unknown'),
        ),
        module,
        method: method.join('.'),
        pathOrDescriptor: subject,
        description,
        cause,
    });
};
const call = Effect.fnUntraced(function* <A>(event: EventName, subject: string, run: (signal: AbortSignal) => PromiseLike<A>, refusal: (answer: A) => string | undefined = Function.constUndefined) {
    const fail = engineError(event, subject);
    const answer = yield* Effect.tryPromise({ try: run, catch: fail });
    const description = refusal(answer);
    return description === undefined ? answer : yield* fail(new Cause.UnknownError(answer, description));
});
const refuse = (module: string, method: string, option: string) => PlatformError.badArgument({ module, method, description: `unsupported ${option}` });
const reveal = (word: CommandLine.Word) => (Redacted.isRedacted(word) ? Redacted.value(word) : word);

// --- [COMPOSITION] ---------------------------------------------------------------------

function services($: EngineInterface) {
    const stat = (path: string, options?: FsStatOptions) => call('fs.stat', path, () => $.fs.stat(path, options));
    const list = (path: string) => call('fs.list', path, () => $.fs.list(path));
    const storeCall = <A>(method: keyof EngineInterface['store'], key: string | undefined, run: () => PromiseLike<A>) =>
        call(`store.${method}`, key ?? $.plugin.name, run).pipe(Effect.mapError((cause) => new KeyValueStore.KeyValueStoreError({ method, ...(key === undefined ? {} : { key }), message: cause.message, cause })));
    const keys = storeCall('keys', undefined, () => $.store.keys());
    const remove = (key: string) => storeCall('delete', key, () => $.store.delete(key));
    return Layer.mergeAll(
        Layer.succeed(FileSystem.FileSystem)(
            FileSystem.make(
                FileSystem.makeNoop({
                    access: (path, options) =>
                        options?.readable === true || options?.writable === true
                            ? refuse('FileSystem', 'access', 'readable or writable check')
                            : call('fs.exists', path, () => $.fs.exists(path)).pipe(
                                  Effect.filterOrFail(Predicate.isTruthy, () => PlatformError.systemError({ _tag: 'NotFound', module: 'FileSystem', method: 'access', pathOrDescriptor: path })),
                                  Effect.asVoid,
                              ),
                    readDirectory: (path, options) => (options?.recursive === true ? refuse('FileSystem', 'readDirectory', 'recursive listing') : Effect.map(list(path), (entries) => entries.map(({ name }) => name))),
                    readDirectoryEntries: (path) => Effect.map(list(path), (entries) => entries.map(({ name, kind, isLink }) => ({ name, type: isLink ? ('SymbolicLink' as const) : KINDS[kind] }))),
                    readFile: (path) =>
                        stat(path).pipe(
                            Effect.flatMap(({ kind, size }) => Effect.forEach(kind === 'file' ? Array.makeBy(Math.ceil(size / READ_LIMIT), (index) => ({ offset: index * READ_LIMIT, length: READ_LIMIT })) : [{}], (range) => call('fs.read', path, () => $.fs.read(path, { as: 'bytes', ...range })))),
                            Effect.map((chunks) => Bytes.concat(chunks.map(({ base64 }) => Uint8Array.fromBase64(base64)))),
                        ),
                    writeFile: (path, data, options) =>
                        (options?.flag ?? 'w') !== 'w' || options?.mode !== undefined
                            ? refuse('FileSystem', 'writeFile', 'flag or mode')
                            : Effect.try({ try: () => new TextDecoder('utf-8', { fatal: true }).decode(data), catch: () => refuse('FileSystem', 'writeFile', 'non-UTF-8 bytes') }).pipe(Effect.flatMap((text) => call('fs.write', path, () => $.fs.write(path, text)))),
                    remove: (path, options) =>
                        call(
                            'process.run',
                            path,
                            () => $.process.run(['rm', ...(options?.recursive === true ? ['-r'] : []), ...(options?.force === true ? ['-f'] : []), '--', path]),
                            ({ exitCode, stderr }) => (exitCode === 0 ? undefined : stderr),
                        ).pipe(Effect.asVoid),
                    stat: (path) =>
                        Effect.map(stat(path), ({ kind, mtimeMs, size }) => ({
                            type: KINDS[kind],
                            mtime: DateTime.makeUnsafe(mtimeMs).pipe(DateTime.toDateUtc, Option.some),
                            atime: Option.none(),
                            birthtime: Option.none(),
                            dev: 0n,
                            ino: Option.none(),
                            mode: 0,
                            nlink: Option.none(),
                            uid: Option.none(),
                            gid: Option.none(),
                            rdev: Option.none(),
                            size: ByteSize.bytes(size),
                            blksize: Option.none(),
                            blocks: Option.none(),
                        })),
                    realPath: (path) => Effect.flatMap(stat(path, { resolve: true }), ({ realPath }) => (realPath === undefined ? PlatformError.systemError({ _tag: 'NotFound', module: 'FileSystem', method: 'realPath', pathOrDescriptor: path }) : Effect.succeed(realPath))),
                }),
            ),
        ),
        Layer.succeed(ChildProcessSpawner.ChildProcessSpawner)(
            ChildProcessSpawner.make(
                Effect.fnUntraced(function* (command: ChildProcess.Command) {
                    const { command: program, args, options } = ChildProcess.isStandardCommand(command) ? command : yield* refuse('ChildProcess', 'spawn', 'pipeline');
                    yield* Effect.when(refuse('ChildProcess', 'spawn', 'shell'), Effect.succeed((options.shell ?? false) !== false));
                    yield* Effect.when(refuse('ChildProcess', 'spawn', 'descriptors past stderr'), Effect.succeed(options.additionalFds !== undefined));
                    const [input, queues] = yield* Effect.all([
                        Match.value(Predicate.isString(options.stdin) || Stream.isStream(options.stdin) ? options.stdin : (options.stdin?.stream ?? 'pipe')).pipe(
                            Match.when(Stream.isStream, (stream) => Effect.map(stream.pipe(Stream.decodeText, Stream.mkString), (text) => ({ input: text }))),
                            Match.when(Match.is('pipe', 'ignore'), () => Effect.succeed({})),
                            Match.orElse(() => refuse('ChildProcess', 'spawn', 'input inherit or overlapped')),
                        ),
                        Effect.all(
                            Record.map({ stdout: options.stdout, stderr: options.stderr }, (output) =>
                                Match.value(Predicate.isString(output) || Sink.isSink(output) ? output : (output?.stream ?? 'pipe')).pipe(
                                    Match.when('pipe', () => Effect.asSome(Queue.unbounded<string, PlatformError.PlatformError | Cause.Done>())),
                                    Match.when('ignore', () => Effect.succeedNone),
                                    Match.orElse(() => refuse('ChildProcess', 'spawn', 'output inherit, overlapped, or sink')),
                                ),
                            ),
                        ),
                    ]);
                    const [set, unset] = Record.partition(options.env ?? {}, (value) => (value === undefined ? Result.failVoid : Result.succeed(reveal(value))));
                    const flags = options.env !== undefined && options.extendEnv !== true ? ['-i'] : Record.keys(unset).flatMap((name) => ['-u', name]);
                    const words = [program, ...args.map(reveal)];
                    const fail = engineError('process.spawn', program);
                    const child = yield* Effect.sync(() => {
                        const started = $.process.spawn({
                            ...(Array.isArrayNonEmpty(flags) ? { argv: ['env', ...flags, '--', ...Record.collect(set, (name, value) => `${name}=${value}`), ...words] } : { argv: words, env: set }),
                            ...(options.cwd === undefined ? {} : { cwd: options.cwd }),
                            ...input,
                        });
                        started.result.catch(Function.constVoid);
                        return started;
                    });
                    const pump = yield* Stream.fromAsyncIterable(child, fail).pipe(
                        Stream.runForEach(({ stream, text }) => Option.match(queues[stream], { onNone: () => Effect.void, onSome: (queue) => Queue.offer(queue, text) })),
                        Effect.onExit((exit) => Effect.forEach(Array.getSomes(Record.values(queues)), (queue) => (Exit.isFailure(exit) && !Cause.hasInterruptsOnly(exit.cause) ? Queue.failCause(queue, exit.cause) : Queue.end(queue)), { discard: true })),
                        Effect.forkScoped({ startImmediately: true }),
                    );
                    const streams = Record.map(queues, (queue) => Option.match(queue, { onNone: () => Stream.empty, onSome: (held) => Stream.encodeText(Stream.fromQueue(held)) }));
                    return ChildProcessSpawner.makeHandle({
                        pid: ChildProcessSpawner.ProcessId(0),
                        exitCode: call('process.spawn', program, () => child.result).pipe(Effect.flatMap((ended) => (ended.code === null ? fail(new Cause.UnknownError(ended, `ended by ${ended.signal}`)) : Effect.succeed(ChildProcessSpawner.ExitCode(ended.code))))),
                        isRunning: Effect.sync(() => pump.pollUnsafe() === undefined),
                        kill: () => Fiber.interrupt(pump),
                        stdin: Sink.fail(refuse('ChildProcess', 'stdin', 'write after spawn')),
                        ...streams,
                        all: Stream.merge(streams.stdout, streams.stderr),
                        getInputFd: () => Sink.drain,
                        getOutputFd: () => Stream.empty,
                        unref: Effect.succeed(Effect.void),
                    });
                }),
            ),
        ),
        Layer.succeed(KeyValueStore.KeyValueStore)(
            KeyValueStore.makeStringOnly({
                get: (key) =>
                    Effect.map(
                        storeCall('get', key, () => $.store.get(key)),
                        (value) => (Predicate.isString(value) ? value : undefined),
                    ),
                set: (key, value) => storeCall('set', key, () => $.store.set(key, value)),
                remove,
                clear: Effect.flatMap(keys, Effect.forEach(remove, { discard: true })),
                size: Effect.map(keys, Array.length),
                keys,
            }),
        ),
        ConfigProvider.layer(call('process.hookEnv', $.plugin.name, () => $.process.hookEnv()).pipe(Effect.map(ConfigProvider.fromEnvRecord))),
        Path.layer,
        Layer.succeed(Scheduler.Scheduler)(new TimeSliced()),
        Logger.layer([Logger.map(Logger.formatSimple, (line) => $.ui.log(line, { to: 'debug' }))]),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { call, services };
