import type { EngineInterface, FsStat, FsStatOptions } from 'claude-code';
import { Array, Bytes, ByteSize, Cause, Clock, ConfigProvider, DateTime, Effect, Exit, Fiber, FileSystem, Formatter, Function, Layer, Logger, Match, Option, Path, PlatformError, Predicate, Queue, Record, Redacted, Result, Scheduler, Sink, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner, type CommandLine } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const readLimit = 4 * 1024 ** 2;
const errnoTags: Readonly<Record<string, PlatformError.SystemErrorTag>> = { ENOENT: 'NotFound', EACCES: 'PermissionDenied', EPERM: 'PermissionDenied', EEXIST: 'AlreadyExists', EISDIR: 'BadResource', ENOTDIR: 'BadResource', ELOOP: 'BadResource', EBUSY: 'Busy' };
const kinds = { file: 'File', dir: 'Directory', other: 'Unknown' } as const satisfies Record<FsStat['kind'], FileSystem.File.Type>;

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

const engineError = (module: string, method: string, subject: string) => (cause: unknown) => {
    const said = Predicate.isError(cause) ? cause.message : Formatter.format(cause);
    return PlatformError.systemError({
        _tag: Option.fromNullishOr(/ failed(?: to start)?: (?<code>E[A-Z]+)/u.exec(said)?.groups?.['code']).pipe(
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
const call = Effect.fnUntraced(function* <A>(module: string, method: string, subject: string, run: (signal: AbortSignal) => PromiseLike<A>, refusal: (answer: A) => string | undefined = Function.constUndefined) {
    const answer = yield* Effect.tryPromise({ try: run, catch: engineError(module, method, subject) });
    const description = refusal(answer);
    return description === undefined ? answer : yield* PlatformError.systemError({ _tag: 'Unknown', module, method, pathOrDescriptor: subject, description });
});
const reveal = (word: CommandLine.Word) => (Redacted.isRedacted(word) ? Redacted.value(word) : word);

// --- [COMPOSITION] ---------------------------------------------------------------------

function fileSystem($: EngineInterface) {
    const stat = (path: string, options?: FsStatOptions) => call('fs', 'stat', path, () => $.fs.stat(path, options));
    const list = (path: string) => call('fs', 'list', path, () => $.fs.list(path));
    const read = (path: string, range: Partial<Record<'offset' | 'length', number>>) =>
        Effect.map(
            call('fs', 'read', path, () => $.fs.read(path, { as: 'bytes', ...range })),
            ({ base64 }) => Uint8Array.fromBase64(base64),
        );
    return FileSystem.make(
        FileSystem.makeNoop({
            access: (path, options) =>
                options?.readable === true || options?.writable === true
                    ? PlatformError.badArgument({ module: 'FileSystem', method: 'access', description: 'a permission check, which the engine has no call for' })
                    : call('fs', 'exists', path, () => $.fs.exists(path)).pipe(
                          Effect.filterOrFail(Predicate.isTruthy, () => PlatformError.systemError({ _tag: 'NotFound', module: 'FileSystem', method: 'access', pathOrDescriptor: path })),
                          Effect.asVoid,
                      ),
            readDirectory: (path, options) => (options?.recursive === true ? PlatformError.badArgument({ module: 'FileSystem', method: 'readDirectory', description: 'a listing of nested folders, as the engine lists one folder' }) : Effect.map(list(path), (entries) => entries.map(({ name }) => name))),
            readDirectoryEntries: (path) => Effect.map(list(path), (entries) => entries.map(({ name, kind, isLink }) => ({ name, type: isLink ? ('SymbolicLink' as const) : kinds[kind] }))),
            readFile: (path) =>
                Effect.flatMap(stat(path), ({ kind, size }) =>
                    kind === 'file'
                        ? Effect.map(
                              Effect.forEach(
                                  Array.makeBy(Math.ceil(size / readLimit), (index) => index * readLimit),
                                  (offset) => read(path, { offset, length: readLimit }),
                              ),
                              Bytes.concat,
                          )
                        : read(path, {}),
                ),
            writeFile: (path, data, options) =>
                (options?.flag ?? 'w') !== 'w' || options?.mode !== undefined
                    ? PlatformError.badArgument({ module: 'FileSystem', method: 'writeFile', description: 'a flag or mode, as the engine writes a whole file under its own mode' })
                    : Effect.try({ try: () => new TextDecoder('utf-8', { fatal: true }).decode(data), catch: () => PlatformError.badArgument({ module: 'FileSystem', method: 'writeFile', description: 'bytes that are no UTF-8 text, as the engine writes text alone' }) }).pipe(
                          Effect.flatMap((text) => call('fs', 'write', path, () => $.fs.write(path, text))),
                      ),
            remove: (path, options) =>
                Effect.asVoid(
                    call(
                        'process',
                        'run',
                        path,
                        () => $.process.run(['rm', ...(options?.recursive === true ? ['-r'] : []), ...(options?.force === true ? ['-f'] : []), '--', path]),
                        ({ exitCode, stderr }) => (exitCode === 0 ? undefined : stderr),
                    ),
                ),
            stat: (path) =>
                Effect.map(stat(path), ({ kind, mtimeMs, size }) => ({
                    type: kinds[kind],
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
    );
}

function spawner($: EngineInterface) {
    return ChildProcessSpawner.make(
        Effect.fnUntraced(function* (command: ChildProcess.Command) {
            const {
                command: program,
                args,
                options,
            } = yield* Effect.succeed(command).pipe(
                Effect.filterOrFail(ChildProcess.isStandardCommand, () => PlatformError.badArgument({ module: 'ChildProcess', method: 'spawn', description: 'a pipeline, as the engine spawns one argument vector' })),
                Effect.filterOrFail(
                    (standard) => (standard.options.shell ?? false) === false,
                    () => PlatformError.badArgument({ module: 'ChildProcess', method: 'spawn', description: 'a shell, as the engine starts an argument vector alone' }),
                ),
                Effect.filterOrFail(
                    (standard) => standard.options.additionalFds === undefined,
                    () => PlatformError.badArgument({ module: 'ChildProcess', method: 'spawn', description: 'descriptors past standard error' }),
                ),
            );
            const [input, queues] = yield* Effect.all([
                Match.value(Predicate.isString(options.stdin) || Stream.isStream(options.stdin) ? options.stdin : (options.stdin?.stream ?? 'pipe')).pipe(
                    Match.when(Stream.isStream, (stream) =>
                        stream.pipe(
                            Stream.decodeText,
                            Stream.mkString,
                            Effect.map((text) => ({ input: text })),
                        ),
                    ),
                    Match.when(Match.is('pipe', 'ignore'), () => Effect.succeed({})),
                    Match.orElse(() => PlatformError.badArgument({ module: 'ChildProcess', method: 'spawn', description: 'an input the engine cannot hand on: inherit or overlapped' })),
                ),
                Effect.all(
                    Record.map({ stdout: options.stdout, stderr: options.stderr }, (output) =>
                        Match.value(Predicate.isString(output) || Sink.isSink(output) ? output : (output?.stream ?? 'pipe')).pipe(
                            Match.when('pipe', () => Effect.asSome(Queue.unbounded<string, PlatformError.PlatformError | Cause.Done>())),
                            Match.when('ignore', () => Effect.succeedNone),
                            Match.orElse(() => PlatformError.badArgument({ module: 'ChildProcess', method: 'spawn', description: 'an output the engine cannot hand on: inherit, overlapped, or a sink' })),
                        ),
                    ),
                ),
            ]);
            const [set, unset] = Record.partition(options.env ?? {}, (value) => (value === undefined ? Result.failVoid : Result.succeed(reveal(value))));
            const alone = options.env !== undefined && options.extendEnv !== true;
            const words = [program, ...args.map(reveal)];
            const child = yield* Effect.sync(() => {
                const started = $.process.spawn({
                    ...(alone || !Record.isEmptyRecord(unset) ? { argv: ['/usr/bin/env', ...(alone ? ['-i'] : Record.keys(unset).flatMap((name) => ['-u', name])), '--', ...Record.collect(set, (name, value) => `${name}=${value}`), ...words] } : { argv: words, env: set }),
                    ...(options.cwd === undefined ? {} : { cwd: options.cwd }),
                    ...input,
                });
                started.result.catch(Function.constVoid);
                return started;
            });
            const pump = yield* Stream.fromAsyncIterable(child, engineError('process', 'spawn', program)).pipe(
                Stream.runForEach(({ stream, text }) => Option.match(queues[stream], { onNone: () => Effect.void, onSome: (queue) => Queue.offer(queue, text) })),
                Effect.onExit((exit) => Effect.forEach(Array.getSomes(Record.values(queues)), (queue) => (Exit.isFailure(exit) && !Cause.hasInterruptsOnly(exit.cause) ? Queue.failCause(queue, exit.cause) : Queue.end(queue)), { discard: true })),
                Effect.forkScoped({ startImmediately: true }),
            );
            const streams = Record.map(queues, (queue) => Option.match(queue, { onNone: () => Stream.empty, onSome: (held) => Stream.encodeText(Stream.fromQueue(held)) }));
            return ChildProcessSpawner.makeHandle({
                pid: ChildProcessSpawner.ProcessId(0),
                exitCode: Effect.flatMap(
                    call('process', 'spawn', program, () => child.result),
                    ({ code, signal }) => (code === null ? PlatformError.systemError({ _tag: 'Unknown', module: 'process', method: 'spawn', pathOrDescriptor: program, description: `ended by ${String(signal)}` }) : Effect.succeed(ChildProcessSpawner.ExitCode(code))),
                ),
                isRunning: Effect.sync(() => pump.pollUnsafe() === undefined),
                kill: () => Fiber.interrupt(pump),
                stdin: Sink.fail(PlatformError.badArgument({ module: 'ChildProcess', method: 'stdin', description: 'writes after the start, as the engine takes standard input once, at spawn' })),
                ...streams,
                all: Stream.merge(streams.stdout, streams.stderr),
                getInputFd: () => Sink.drain,
                getOutputFd: () => Stream.empty,
                unref: Effect.succeed(Effect.void),
            });
        }),
    );
}

function services($: EngineInterface) {
    return Layer.mergeAll(
        Layer.succeed(FileSystem.FileSystem, fileSystem($)),
        Layer.succeed(ChildProcessSpawner.ChildProcessSpawner, spawner($)),
        ConfigProvider.layer(
            Effect.map(
                call('process', 'hookEnv', $.plugin.name, () => $.process.hookEnv()),
                (variables) => ConfigProvider.fromEnvRecord(variables),
            ),
        ),
        Path.layer,
        Layer.succeed(Scheduler.Scheduler, new TimeSliced()),
        Logger.layer([Logger.map(Logger.formatSimple, (line) => $.ui.log(line, { to: 'debug' }))]),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { call, services };
