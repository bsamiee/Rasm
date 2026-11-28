import { Shell } from '@rasm/shell';
import { Array, Cache, Config, Context, Duration, Effect, Exit, type Fiber, FiberSet, FileSystem, Glob, Hash, Layer, Option, Order, Path, PlatformError, Predicate, Record, Redacted, Result, type Scope, Sink, Stream, String } from 'effect';
import { ChildProcess, ChildProcessSpawner, CommandLine } from 'effect/process';
import 'sh-syntax/vendors/wasm_exec';
import { getProcessor } from 'sh-syntax/sync';
import { Engine } from '#/engine/interface';

// --- [TYPES] ---------------------------------------------------------------------------

type Probe = ReturnType<Shell['Service']['dialect']>;
type Script = Effect.Success<ReturnType<Shell['Service']['read']>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const COLLATOR = new Intl.Collator(undefined, { numeric: true });

// --- [SERVICES] ------------------------------------------------------------------------

class Session extends Context.Service<
    Session,
    {
        readonly shell: (agent: Option.Option<string>) => Effect.Effect<
            {
                readonly environment: Effect.Effect<{ readonly dialect: Effect.Success<Probe>; readonly cwd: string }, Effect.Error<Probe> | PlatformError.PlatformError | CommandLine.CommandLineError>;
                readonly path: Path.Path;
                readonly exec: (script: Script) => Effect.Effect<ChildProcessSpawner.ChildProcessHandle, Effect.Error<Probe> | PlatformError.PlatformError | CommandLine.CommandLineError, Scope.Scope>;
            },
            PlatformError.PlatformError
        >;
        readonly fork: <A, E, R>(effect: Effect.Effect<A, E, R>) => Effect.Effect<Fiber.Fiber<A, E>, never, R>;
    }
>()('@rasm/claude-code/policy/session', {
    make: Effect.gen(function* () {
        const [engine, shell, fs, processes, path] = yield* Effect.all([Engine, Shell, FileSystem.FileSystem, ChildProcessSpawner.ChildProcessSpawner, Path.Path]);
        const [printed, plugin] = yield* Effect.all([processes.string(ChildProcess.make('env', ['-0'])), Config.all({ data: Config.String('CLAUDE_PLUGIN_DATA'), pid: Config.String('CLAUDE_PID'), id: Config.String('CLAUDE_CODE_SESSION_ID') })], { concurrency: 'unbounded' });
        const variables = Record.fromEntries(
            Array.filterMap(printed.split('\0'), (entry) =>
                Option.match(String.indexOf('=')(entry), {
                    onNone: () => Result.failVoid,
                    onSome: (at) => Result.succeed([entry.slice(0, at), entry.slice(at + 1)] as const),
                }),
            ),
        );
        const { binary, home } = yield* Effect.fromOption(
            Option.all({
                binary: Option.orElse(Record.get(variables, 'CLAUDE_CODE_SHELL'), () => Record.get(variables, 'SHELL')),
                home: Option.orElse(Record.get(variables, 'CLAUDE_CONFIG_DIR'), () => Option.map(Record.get(variables, 'HOME'), (folder) => path.join(folder, '.claude'))),
            }),
            () => PlatformError.badArgument({ module: 'Session', method: 'make', description: 'unset variable, names SHELL HOME' }),
        );
        const dialects = path.join(plugin.data, 'dialect', plugin.pid);
        yield* Effect.addFinalizer(() => Effect.ignore(fs.remove(dialects, { recursive: true, force: true })));
        const stamp = (file: string) =>
            fs.stat(file).pipe(
                Effect.map(({ type, mtime }) => Option.filter(Option.some({ file, mtime }), () => type === 'File')),
                Effect.catchReason('PlatformError', 'NotFound', () => Effect.succeedNone),
            );
        const listing = (folder: string, pattern: string) =>
            fs.glob(pattern, { root: folder }).pipe(
                Effect.flatMap((names) => Effect.forEach(names.toSorted(COLLATOR.compare), (name) => stamp(path.join(folder, name)), { concurrency: 'unbounded' })),
                Effect.map(Array.getSomes),
            );
        const probes = yield* Cache.makeWith(
            Effect.fnUntraced(function* (sources: readonly { readonly file: string }[]) {
                const text = (yield* Effect.forEach(sources, ({ file }) => Effect.fromResult(CommandLine.join(['.', file])))).join('\n');
                const source = path.join(dialects, `${Hash.string(text) >>> 0}.sh`);
                yield* fs.writeFileString(source, text);
                return { source, dialect: yield* shell.dialect({ shell: binary, environment: variables, source }) };
            }),
            { capacity: 1, timeToLive: (exit) => (Exit.isSuccess(exit) ? Duration.infinity : Duration.zero) },
        );
        const probed = Effect.gen(function* () {
            const [snapshots, launched, hooks] = yield* Effect.all([listing(path.join(home, 'shell-snapshots'), `snapshot-${Glob.escape(path.basename(binary))}-*`), Effect.forEach(Option.toArray(Record.get(variables, 'CLAUDE_ENV_FILE')), stamp), listing(path.join(home, 'session-env', plugin.id), '*-hook-[0-9]*.sh')], {
                concurrency: 'unbounded',
            });
            return yield* Cache.get(probes, [...[snapshots].filter(Array.isArrayNonEmpty).map((held) => Array.max(held, Order.Struct({ mtime: Option.makeOrder(Order.Date) }))), ...Array.getSomes(launched), ...hooks]);
        });
        const exec = (cwd: string) =>
            Effect.fnUntraced(function* (script: Script) {
                const top = (yield* script.invocations).filter(({ within }) => Array.isReadonlyArrayEmpty(within));
                yield* Effect.when(PlatformError.badArgument({ module: 'Session', method: 'exec', description: 'unsupported list, pipeline, or background command' }), Effect.succeed(!Predicate.isTupleOf(top, 1) || top.some(({ background }) => background)));
                const { source } = yield* probed;
                const text = ['echo $$ >&2', yield* Effect.fromResult(CommandLine.join(['.', source])), `exec ${script.source}`].join('\n');
                const child = yield* processes.spawn(ChildProcess.make(binary, ['-c', Array.isReadonlyArrayNonEmpty(script.text.redacted) ? Redacted.make(text) : text], { cwd, env: variables, extendEnv: false }));
                const [first, rest] = yield* Stream.peel(Stream.decodeText(child.stderr), Sink.head());
                const { pid, tail } = yield* Effect.fromOption(
                    first.pipe(
                        Option.flatMapNullishOr((piece) => /^(?<pid>\d+)\n(?<tail>[^]*)$/u.exec(piece)?.groups),
                        Option.flatMap((groups) => Option.all({ pid: Record.get(groups, 'pid'), tail: Record.get(groups, 'tail') })),
                    ),
                    () => PlatformError.systemError({ _tag: 'Unknown', module: 'Session', method: 'exec', pathOrDescriptor: binary, description: 'no pid, stream stderr' }),
                );
                const stderr = Stream.encodeText(Stream.concat(Stream.make(tail), rest));
                return ChildProcessSpawner.makeHandle({ ...child, pid: ChildProcessSpawner.ProcessId(Number(pid)), stderr, all: Stream.merge(child.stdout, stderr) });
            });
        return {
            shell: Effect.fnUntraced(function* (agent: Option.Option<string>) {
                const found = yield* Effect.transposeOption(Option.map(agent, (id) => Effect.map(engine.agents, Record.get(id))));
                const cwd = yield* Option.match(Option.flatten(found), { onNone: () => engine.cwd, onSome: (info) => Effect.succeed(info.cwd) });
                return {
                    environment: Effect.map(probed, ({ dialect }) => ({ dialect, cwd })),
                    path: Path.make({
                        ...path,
                        resolve: (...segments) => path.resolve(cwd, ...segments),
                        relative: (from, to) => path.relative(path.resolve(cwd, from), path.resolve(cwd, to)),
                        toFileUrl: (file) => path.toFileUrl(path.isAbsolute(file) ? file : path.join(cwd, file)),
                    }),
                    exec: exec(cwd),
                };
            }),
            fork: FiberSet.run(yield* FiberSet.make()),
        };
    }),
}) {
    static readonly layer = Layer.effect(this, this.make).pipe(
        Layer.provideMerge(
            Layer.effect(
                Shell,
                Effect.gen(function* () {
                    const [engine, fs, path, root] = yield* Effect.all([Engine, FileSystem.FileSystem, Path.Path, Config.String('CLAUDE_PLUGIN_ROOT')]);
                    const module = yield* Effect.flatMap(fs.readFile(path.join(root, 'hooks', 'main.wasm')), engine.compile);
                    return yield* Shell.make({ processor: getProcessor(module) });
                }),
            ),
        ),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Session };
