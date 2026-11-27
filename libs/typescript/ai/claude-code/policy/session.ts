import { Shell } from '@rasm/shell';
import { Array, Cache, Config, Context, Duration, Effect, Exit, FiberSet, FileSystem, Hash, Layer, Option, Order, Path, PlatformError, Record } from 'effect';
import { ChildProcess, ChildProcessSpawner, CommandLine } from 'effect/process';
import 'sh-syntax/vendors/wasm_exec';
import { getProcessor } from 'sh-syntax/sync';
import { Engine } from '#/engine/interface';

// --- [TYPES] ---------------------------------------------------------------------------

type Probe = ReturnType<Shell['Service']['dialect']>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const collator = new Intl.Collator(undefined, { numeric: true });

// --- [SERVICES] ------------------------------------------------------------------------

class Session extends Context.Service<
    Session,
    {
        readonly shell: (agent: Option.Option<string>) => Effect.Effect<
            {
                readonly environment: Effect.Effect<{ readonly dialect: Effect.Success<Probe>; readonly cwd: string }, Effect.Error<Probe> | PlatformError.PlatformError | CommandLine.CommandLineError>;
                readonly path: Path.Path;
            },
            PlatformError.PlatformError
        >;
        readonly watch: <E, R>(watcher: Effect.Effect<unknown, E, R>) => Effect.Effect<void, never, R>;
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
                () => PlatformError.badArgument({ module: 'Session', method: 'shell', description: 'a process that names no shell or no home' }),
            );
            const dialects = path.join(plugin.data, 'dialect', plugin.pid);
            yield* Effect.addFinalizer(() => Effect.ignore(fs.remove(dialects, { recursive: true, force: true })));
            const stamp = (file: string) =>
                fs.stat(file).pipe(
                    Effect.map(({ type, mtime }) => ({ file, type, mtimeMs: Option.match(mtime, { onNone: () => 0, onSome: (at) => at.getTime() }) })),
                    Effect.orElseSucceed(() => ({ file, type: 'Unknown' as const, mtimeMs: 0 })),
                );
            const listing = (directory: string, kept: (name: string) => boolean) =>
                fs.readDirectory(directory).pipe(
                    Effect.flatMap((names) => Effect.forEach(names.filter(kept), (name) => stamp(path.join(directory, name)), { concurrency: 'unbounded' })),
                    Effect.orElseSucceed(() => []),
                );
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
                const [named, scripts, launched] = yield* Effect.all(
                    [listing(path.join(home, 'shell-snapshots'), (name) => name.startsWith(`snapshot-${path.basename(binary)}-`)), listing(path.join(home, 'session-env', plugin.id), (name) => /-hook-\d+\.sh$/u.test(name)), Effect.forEach(Option.toArray(Record.get(variables, 'CLAUDE_ENV_FILE')), stamp)],
                    { concurrency: 'unbounded' },
                );
                return yield* Cache.get(
                    probes,
                    [...[named].filter(Array.isArrayNonEmpty).map(Array.max(Order.mapInput(Order.Number, ({ mtimeMs }: (typeof named)[number]) => mtimeMs))), ...launched, ...scripts.toSorted((left, right) => collator.compare(left.file, right.file))].filter(({ type }) => type === 'File'),
                );
            });
            return {
                shell: Effect.fnUntraced(function* (agent: Option.Option<string>) {
                    const found = yield* Effect.transposeOption(Option.map(agent, engine.findAgent));
                    const cwd = yield* Option.match(Option.flatten(found), { onNone: () => engine.cwd, onSome: (info) => Effect.succeed(info.cwd) });
                    const anchored = Path.make({
                        ...path,
                        resolve: (...segments) => path.resolve(cwd, ...segments),
                        relative: (from, to) => path.relative(path.resolve(cwd, from), path.resolve(cwd, to)),
                        toFileUrl: (file) => path.toFileUrl(file.endsWith('/') ? `${path.resolve(cwd, file)}/` : path.resolve(cwd, file)),
                    });
                    return { environment: Effect.map(dialect, (probed) => ({ dialect: probed, cwd })), path: anchored };
                }),
                watch: (watcher) => Effect.asVoid(FiberSet.run(watchers, watcher)),
            };
        }),
    ).pipe(
        Layer.provideMerge(
            Layer.effect(
                Shell,
                Effect.flatMap(
                    Engine.use((engine) => engine.compile('./main.wasm')),
                    (module) => Shell.make({ processor: getProcessor(module) }),
                ),
            ),
        ),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Session };
