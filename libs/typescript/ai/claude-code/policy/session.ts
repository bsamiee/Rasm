import { Shell } from '@rasm/shell';
import { Array, Cache, Config, Context, Duration, Effect, Exit, FiberSet, FileSystem, Glob, Hash, Layer, Option, Order, Path, PlatformError, Record, type Scope } from 'effect';
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
                readonly exec: (script: Script) => Effect.Effect<ChildProcessSpawner.ChildProcessHandle, PlatformError.PlatformError | CommandLine.CommandLineError, Scope.Scope>;
            },
            PlatformError.PlatformError
        >;
        readonly watch: <E, R>(watcher: Effect.Effect<unknown, E, R>) => Effect.Effect<unknown, never, R>;
    }
>()('@rasm/claude-code/policy/session') {
    static readonly layer = Layer.effect(
        Session,
        Effect.gen(function* () {
            const [engine, shell, fs, processes, path] = yield* Effect.all([Engine, Shell, FileSystem.FileSystem, ChildProcessSpawner.ChildProcessSpawner, Path.Path]);
            const [printed, plugin] = yield* Effect.all([processes.string(ChildProcess.make('env', ['-0'])), Config.all({ data: Config.String('CLAUDE_PLUGIN_DATA'), pid: Config.String('CLAUDE_PID'), id: Config.String('CLAUDE_CODE_SESSION_ID') })], { concurrency: 'unbounded' });
            const variables = Record.fromIterableWith(printed.matchAll(/(?<name>[^\0=]+)=(?<value>[^\0]*)/gu), ({ groups }) => [groups?.['name'] ?? '', groups?.['value'] ?? '']);
            const { binary, home } = yield* Effect.fromOption(
                Option.all({
                    binary: Option.orElse(Record.get(variables, 'CLAUDE_CODE_SHELL'), () => Record.get(variables, 'SHELL')),
                    home: Option.orElse(Record.get(variables, 'CLAUDE_CONFIG_DIR'), () => Option.map(Record.get(variables, 'HOME'), (folder) => path.join(folder, '.claude'))),
                }),
                () => PlatformError.badArgument({ module: 'Session', method: 'shell', description: 'no shell or no home in the environment' }),
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
                    const script = path.join(dialects, `${Hash.string(text) >>> 0}.sh`);
                    yield* fs.writeFileString(script, text);
                    return yield* shell.dialect({ shell: binary, environment: variables, source: script });
                }),
                { capacity: 1, timeToLive: (exit) => (Exit.isSuccess(exit) ? Duration.infinity : Duration.zero) },
            );
            const sources = Effect.gen(function* () {
                const [snapshots, launched, hooks] = yield* Effect.all(
                    [listing(path.join(home, 'shell-snapshots'), `snapshot-${Glob.escape(path.basename(binary))}-*`), Effect.forEach(Option.toArray(Record.get(variables, 'CLAUDE_ENV_FILE')), stamp), listing(path.join(home, 'session-env', plugin.id), '*-hook-[0-9]*.sh')],
                    {
                        concurrency: 'unbounded',
                    },
                );
                return [...[snapshots].filter(Array.isArrayNonEmpty).map((held) => Array.max(held, Order.Struct({ mtime: Option.makeOrder(Order.Date) }))), ...Array.getSomes(launched), ...hooks];
            });
            const dialect = Effect.flatMap(sources, (held) => Cache.get(probes, held));
            return {
                shell: Effect.fnUntraced(function* (agent: Option.Option<string>) {
                    const listed = yield* Effect.when(engine.agents, Effect.succeed(Option.isSome(agent)));
                    const found = Option.flatMap(Option.all({ id: agent, live: listed }), ({ id, live }) => Record.get(live, id));
                    const cwd = yield* Option.match(found, { onNone: () => engine.cwd, onSome: (info) => Effect.succeed(info.cwd) });
                    const anchored = Path.make({
                        ...path,
                        resolve: (...segments) => path.resolve(cwd, ...segments),
                        relative: (from, to) => path.relative(path.resolve(cwd, from), path.resolve(cwd, to)),
                        toFileUrl: (file) => path.toFileUrl(path.isAbsolute(file) ? file : path.join(cwd, file)),
                    });
                    return {
                        environment: Effect.map(dialect, (probed) => ({ dialect: probed, cwd })),
                        path: anchored,
                        exec: (script: Script) =>
                            sources.pipe(
                                Effect.flatMap(Effect.forEach(({ file }) => Effect.fromResult(CommandLine.join(['.', file])))),
                                Effect.flatMap((loaded) => processes.spawn(ChildProcess.make(binary, ['-c', [...loaded, `exec ${script.source}`].join('\n')], { cwd, env: variables, extendEnv: false }))),
                            ),
                    };
                }),
                watch: FiberSet.run(yield* FiberSet.make()),
            };
        }),
    ).pipe(
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
