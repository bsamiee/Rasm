import { Shell } from '@rasm/shell';
import type { AgentStatus, EngineInterface, FsStat, FsStatOptions } from 'claude-code';
import { Array, Bytes, ByteSize, Cache, Cause, Clock, Context, Data, DateTime, Duration, Effect, Exit, Fiber, FiberSet, FileSystem, Formatter, Function, Hash, Layer, Logger, ManagedRuntime, Match, Option, Order, Path, PlatformError, Predicate, Queue, Record, Redacted, Result, Scheduler, Sink, Stream } from 'effect';
import { ChildProcess, ChildProcessSpawner, CommandLine } from 'effect/process';
import 'sh-syntax/vendors/wasm_exec';
import { getProcessor } from 'sh-syntax/sync';

// --- [TYPES] ---------------------------------------------------------------------------

type Probe = ReturnType<Shell['Service']['dialect']>;
type Notice = Data.TaggedEnum<{
    Toast: { readonly text: string; readonly timeout?: Duration.Input };
    Notify: { readonly key: string; readonly text: Option.Option<string> };
    Model: { readonly text: string; readonly agent: Option.Option<string>; readonly idle: boolean };
}>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const readLimit = 4 * 1024 ** 2;
const errnoTags: Readonly<Record<string, PlatformError.SystemErrorTag>> = { ENOENT: 'NotFound', EACCES: 'PermissionDenied', EPERM: 'PermissionDenied', EEXIST: 'AlreadyExists', EISDIR: 'BadResource', ENOTDIR: 'BadResource', ELOOP: 'BadResource', EBUSY: 'Busy' };
const kinds = { file: 'File', dir: 'Directory', other: 'Unknown' } as const satisfies Record<FsStat['kind'], FileSystem.File.Type>;
const resting = { pending: Option.some(false), running: Option.some(false), waiting: Option.some(true), idle: Option.some(true), completed: Option.none(), failed: Option.none(), killed: Option.none() } as const satisfies Record<AgentStatus, Option.Option<boolean>>;
const collator = new Intl.Collator(undefined, { numeric: true });

// --- [MODELS] --------------------------------------------------------------------------

const Notice = Data.taggedEnum<Notice>();

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
const request = Effect.fnUntraced(function* <A>(module: string, method: string, subject: string, run: () => PromiseLike<A>, refusal: (answer: A) => string | undefined = Function.constUndefined) {
    const answer = yield* Effect.tryPromise({ try: run, catch: engineError(module, method, subject) });
    const description = refusal(answer);
    return description === undefined ? answer : yield* PlatformError.systemError({ _tag: 'Unknown', module, method, pathOrDescriptor: subject, description });
});
const reveal = (word: CommandLine.Word) => (Redacted.isRedacted(word) ? Redacted.value(word) : word);
const anchor = (base: Path.Path, cwd: string) =>
    Path.make({
        ...base,
        resolve: (...segments) => base.resolve(cwd, ...segments),
        relative: (from, to) => base.relative(base.resolve(cwd, from), base.resolve(cwd, to)),
        toFileUrl: (file) => base.toFileUrl(file.endsWith('/') ? `${base.resolve(cwd, file)}/` : base.resolve(cwd, file)),
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

function say($: EngineInterface) {
    return (notice: Notice) =>
        Effect.asVoid(
            Notice.$match(notice, {
                Toast: ({ text, timeout }) => Effect.try({ try: () => $.ui.toast(text, timeout === undefined ? undefined : { timeoutMs: Duration.toMillis(timeout) }), catch: engineError('ui', 'toast', text) }),
                Notify: ({ key, text }) =>
                    Option.match(text, {
                        onNone: () => request('ui', 'cancelNotify', key, () => $.ui.cancelNotify(key)),
                        onSome: (body) =>
                            request(
                                'ui',
                                'notify',
                                key,
                                () => $.ui.notify(body, { key }),
                                (sent) => (sent.isSent ? undefined : sent.reason),
                            ),
                    }),
                Model: ({ text, agent, idle }) =>
                    Match.value({ idle, agentId: Option.getOrUndefined(agent) }).pipe(
                        Match.when({ idle: false }, ({ agentId }) =>
                            request(
                                'session',
                                'append',
                                agentId ?? 'main',
                                () => $.session.append({ message: { type: 'user', content: [{ type: 'text', text }] }, ...(agentId === undefined ? {} : { agentId }) }),
                                ({ deny }) => deny,
                            ),
                        ),
                        Match.when({ agentId: Match.string }, ({ agentId }) =>
                            request(
                                'session',
                                'send',
                                agentId,
                                () => $.session.send({ to: { agentId }, text }),
                                ({ reason }) => reason,
                            ),
                        ),
                        Match.orElse(() =>
                            request(
                                'prompt',
                                'submit',
                                'main',
                                () => $.prompt.submit({ text }),
                                ({ drop }) => drop,
                            ),
                        ),
                    ),
            }),
        );
}

function fileSystem($: EngineInterface) {
    const stat = (path: string, options?: FsStatOptions) => request('fs', 'stat', path, () => $.fs.stat(path, options));
    const list = (path: string) => request('fs', 'list', path, () => $.fs.list(path));
    const read = (path: string, range: Partial<Record<'offset' | 'length', number>>) =>
        Effect.map(
            request('fs', 'read', path, () => $.fs.read(path, { as: 'bytes', ...range })),
            ({ base64 }) => Uint8Array.fromBase64(base64),
        );
    return FileSystem.make(
        FileSystem.makeNoop({
            access: (path, options) =>
                options?.readable === true || options?.writable === true
                    ? PlatformError.badArgument({ module: 'FileSystem', method: 'access', description: 'a permission check, which the engine has no call for' })
                    : request('fs', 'exists', path, () => $.fs.exists(path)).pipe(
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
                          Effect.flatMap((text) => request('fs', 'write', path, () => $.fs.write(path, text))),
                      ),
            remove: (path, options) =>
                Effect.asVoid(
                    request(
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
                    request('process', 'spawn', program, () => child.result),
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

function session($: EngineInterface) {
    const listed = (agent: string) =>
        Effect.map(
            request('agent', 'list', agent, () => $.agent.list()),
            Array.findFirst(({ id }) => id === agent),
        );
    const stamp = (file: string) =>
        request('fs', 'stat', file, () => $.fs.stat(file)).pipe(
            Effect.map(({ kind, mtimeMs }) => ({ file, kind, mtimeMs })),
            Effect.orElseSucceed(() => ({ file, kind: 'other' as const, mtimeMs: 0 })),
        );
    const listing = (directory: string, join: Path.Path['join']) =>
        request('fs', 'list', directory, () => $.fs.list(directory)).pipe(
            Effect.flatMap(Effect.forEach(({ name, kind, mtimeMs, isLink }) => (isLink ? stamp(join(directory, name)) : Effect.succeed({ file: join(directory, name), kind, mtimeMs })))),
            Effect.orElseSucceed(() => []),
        );
    return Layer.effect(
        Session,
        Effect.gen(function* () {
            const [shell, fs, processes, path] = yield* Effect.all([Shell, FileSystem.FileSystem, ChildProcessSpawner.ChildProcessSpawner, Path.Path]);
            const [printed, plugin, id] = yield* Effect.all([processes.string(ChildProcess.make('env', ['-0'])), request('process', 'hookEnv', 'plugin', () => $.process.hookEnv()), request('session', 'id', 'session', () => $.session.id())], { concurrency: 'unbounded' });
            const variables = Record.fromIterableWith(printed.matchAll(/(?<name>[^\0=]+)=(?<value>[^\0]*)/gu), ({ groups }) => [groups?.['name'] ?? '', groups?.['value'] ?? '']);
            const { binary, home, dialects } = yield* Effect.fromOption(
                Option.all({
                    binary: Option.orElse(Record.get(variables, 'CLAUDE_CODE_SHELL'), () => Record.get(variables, 'SHELL')),
                    home: Option.orElse(Record.get(variables, 'CLAUDE_CONFIG_DIR'), () => Option.map(Record.get(variables, 'HOME'), (folder) => path.join(folder, '.claude'))),
                    dialects: Option.map(Option.all([Record.get(plugin, 'CLAUDE_PLUGIN_DATA'), Record.get(plugin, 'CLAUDE_PID')]), ([data, pid]) => path.join(data, 'dialect', pid)),
                }),
                () => PlatformError.badArgument({ module: 'Session', method: 'environment', description: 'a process that names no shell, no home, or no plugin data folder' }),
            );
            yield* Effect.addFinalizer(() => Effect.ignore(fs.remove(dialects, { recursive: true, force: true })));
            const watchers = yield* FiberSet.make();
            const probes = yield* Cache.makeWith(
                Effect.fnUntraced(function* (sources: readonly { readonly file: string }[]) {
                    const lines = yield* Effect.forEach(sources, ({ file }) => Effect.fromResult(CommandLine.join(['.', file])));
                    const script = path.join(dialects, `${Hash.string(lines.join('\n')) >>> 0}.sh`);
                    yield* fs.writeFileString(script, lines.join('\n'));
                    return yield* shell.dialect({ shell: binary, environment: variables, source: script });
                }),
                { capacity: 4, timeToLive: (exit) => (Exit.isSuccess(exit) ? Duration.infinity : Duration.zero) },
            );
            const dialect = Effect.gen(function* () {
                const [snapshots, scripts, launched] = yield* Effect.all([listing(path.join(home, 'shell-snapshots'), path.join), listing(path.join(home, 'session-env', id), path.join), Effect.forEach(Option.toArray(Record.get(variables, 'CLAUDE_ENV_FILE')), stamp)], { concurrency: 'unbounded' });
                const named = snapshots.filter(({ file }) => path.basename(file).startsWith(`snapshot-${path.basename(binary)}-`));
                return yield* Cache.get(
                    probes,
                    [...[named].filter(Array.isArrayNonEmpty).map(Array.max(Order.mapInput(Order.Number, ({ mtimeMs }: (typeof named)[number]) => mtimeMs))), ...launched, ...scripts.filter(({ file }) => /-hook-\d+\.sh$/u.test(file)).toSorted((left, right) => collator.compare(left.file, right.file))].filter(
                        ({ kind }) => kind === 'file',
                    ),
                );
            });
            return {
                acting: Effect.fnUntraced(function* (agent: Option.Option<string>) {
                    const found = yield* Effect.transposeOption(Option.map(agent, listed));
                    const cwd = yield* Option.match(Option.flatten(found), { onNone: () => request('session', 'cwd', 'session', () => $.session.cwd()), onSome: (info) => Effect.succeed(info.cwd) });
                    return { environment: Effect.map(dialect, (probed) => ({ dialect: probed, cwd })), path: anchor(path, cwd) };
                }),
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
                Layer.mergeAll(Layer.succeed(FileSystem.FileSystem, fileSystem($)), Layer.succeed(ChildProcessSpawner.ChildProcessSpawner, spawner($)), Path.layer, Layer.succeed(Scheduler.Scheduler, new TimeSliced()), Logger.layer([Logger.map(Logger.formatSimple, (line) => $.ui.log(line, { to: 'debug' }))])),
            ),
        ),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Notice, request, runtime, say, Session, session };
