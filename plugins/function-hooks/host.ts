import { availableParallelism } from 'node:os';
import process from 'node:process';
import { Installation } from '@rasm/applications';
import { Completed } from '@rasm/process';
import { Array, Cache, Config, Context, Duration, Effect, Exit, FileSystem, Formatter, Function, Layer, Option, Path, Predicate, Record, Schema, Semaphore, Stream, String } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { WATCH_APPS } from '#/shell/programs';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ASCENDING = /^\.\.(?:[/\\]|$)/u;
const SPACES = /\s+/u;
const STEP_FILES = /\{\{\s*files\s*\}\}/u;
const KEEP_SUCCESSES = { timeToLive: (exit: Exit.Exit<unknown, unknown>) => (Exit.isSuccess(exit) ? Duration.infinity : Duration.zero) };

// --- [MODELS] --------------------------------------------------------------------------

const Run = Schema.Struct({ command: Schema.Union([Schema.String, Schema.Struct({ argv: Schema.Array(Schema.String) })]) });
const Steps = Schema.fromJsonString(Schema.Struct({ steps: Schema.Record(Schema.String, Schema.Record(Schema.String, Schema.Unknown)) }));
const Installations = Schema.fromJsonString(Schema.toCodecJson(Schema.Record(Schema.String, Schema.Array(Installation))));

// --- [ERRORS] --------------------------------------------------------------------------

class HostError extends Schema.TaggedError<HostError, { readonly brand: unique symbol }>()('HostError', { program: Schema.String, cause: Schema.Defect() }) {
    static readonly of =
        (program: string) =>
        (cause: unknown): HostError =>
            HostError.make({ program, cause });

    override get message() {
        return `${this.program}: ${String.trim(Predicate.isString(this.cause) ? this.cause : Formatter.format(this.cause))}`;
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

class Host extends Context.Service<Host>()('@rasm/function-hooks/host', {
    make: Effect.gen(function* () {
        const [fs, path, spawner, home] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner, Config.option(Config.String('HOME'))]);
        const root = path.resolve(import.meta.dirname, '../..');
        const hooks = path.join(root, '.config/hk.pkl');
        const nx = Effect.promise(() => import('@nx/devkit/internal'));
        const slots = yield* Semaphore.make(availableParallelism());
        const run = Effect.fnUntraced(
            function* ([program, ...args]: Array.NonEmptyReadonlyArray<string>, { env = {}, stdin }: { readonly env?: Readonly<Record<string, string>>; readonly stdin?: string } = {}) {
                return yield* Completed.run(ChildProcess.make(program, args, { cwd: root, env, extendEnv: true, stdin: stdin === undefined ? 'ignore' : Stream.encodeText(Stream.make(stdin)) }));
            },
            Effect.provideService(ChildProcessSpawner.ChildProcessSpawner, spawner),
            (effect) => slots.withPermit(effect),
            (effect, ...[argv]) => Effect.mapError(effect, HostError.of(argv[0])),
        );
        const output = (argv: Array.NonEmptyReadonlyArray<string>) =>
            run(argv).pipe(
                Effect.map(({ stdout }) => stdout),
                Effect.orElseSucceed(() => ''),
            );
        const modified = (file: string) =>
            fs.stat(file).pipe(
                Effect.map(({ mtime }) => Option.match(mtime, { onNone: () => 0, onSome: (date) => date.getTime() })),
                Effect.orElseSucceed(() => 0),
            );
        const inside = (file: string, folder: string) => !ASCENDING.test(path.relative(folder, file));
        const real = (file: string) => Effect.orElseSucceed(fs.realPath(file), () => file);
        const graphs = yield* Cache.make({
            capacity: 1,
            lookup: Effect.fnUntraced(function* (_modified: number) {
                const [{ readCachedProjectGraph }, { createProjectRootMappings, findProjectForPath }] = yield* Effect.all([Effect.promise(() => import('@nx/devkit')), nx]);
                return Option.map(Option.liftThrowable(readCachedProjectGraph)(), ({ nodes }) => {
                    const roots = createProjectRootMappings(nodes);
                    const owner = (file: string) => {
                        const name = findProjectForPath(path.relative(root, file) || '.', roots);
                        return Option.fromNullishOr(name === null ? undefined : nodes[name]);
                    };
                    return { nodes, owner };
                });
            }),
        });
        const steps = yield* Cache.make({
            capacity: 1,
            lookup: Function.constant(
                Effect.map(
                    output(['pkl', 'eval', '--format', 'json', hooks]),
                    Function.flow(
                        Schema.decodeOption(Steps),
                        Option.toArray,
                        Array.flatMap(({ steps: decoded }) => Record.toEntries(decoded)),
                        Array.flatMap(([step, modes]) => Record.collect(Record.filter(modes, Schema.is(Run)), (mode, { command }) => ({ step, mode, parts: (Predicate.isString(command) ? command : command.argv.join(' ')).split(STEP_FILES) }))),
                        Array.filter(({ parts }) => parts.length > 1),
                        Array.map(({ step, mode, parts }) => ({ step, mode, text: parts.join('') })),
                    ),
                ),
            ),
        });
        const onPath = yield* Cache.makeWith((program: string) => Effect.map(run(['which', program]), ({ code }) => code === 0), { capacity: 1024, ...KEEP_SUCCESSES });
        const ignored = yield* Cache.makeWith(
            Effect.fnUntraced(function* (files: readonly string[]) {
                const reals = yield* Effect.forEach(files, real, { concurrency: 'unbounded' });
                const owned = reals.filter((file) => inside(file, root));
                return owned.length > 0 && (yield* run(['git', 'check-ignore', '--no-index', '--stdin', '-z'], { stdin: owned.join('\0') })).code === 0;
            }),
            { capacity: 1024, ...KEEP_SUCCESSES },
        );
        return {
            root,
            home,
            fs,
            path,
            run,
            output,
            inside,
            parent: `${process.ppid}`,
            parents: Effect.map(output(['ps', '-A', '-o', 'pid=,ppid=']), (table): ReadonlyMap<string, string> => new Map(Array.chunksOf(table.trim().split(SPACES), 2).map(([pid, parent = '']) => [pid, parent]))),
            environment: (name: string) => Effect.orElseSucceed(Config.option(Config.String(name)), Option.none),
            exists: (file: string) => Effect.orElseSucceed(fs.exists(file), () => false),
            onPath: (program: string) => Cache.get(onPath, program),
            ignored: (files: readonly string[]) => Cache.get(ignored, files),
            installations: (names: readonly string[]) => Effect.map(output([WATCH_APPS, 'find', ...names]), (text) => Array.getSomes(text.split('\n').map((line) => Schema.decodeOption(Installations)(line))).flatMap((found) => Object.values(found).flat())),
            graph: nx.pipe(
                Effect.flatMap(({ workspaceDataDirectory }) => modified(path.join(workspaceDataDirectory, 'project-graph.json'))),
                Effect.flatMap((at) => Cache.get(graphs, at)),
            ),
            steps: Effect.flatMap(modified(hooks), (at) => Cache.get(steps, at)),
            resolve: (word: string, cwd: Option.Option<string>): Option.Option<string> => (path.isAbsolute(word) ? Option.some(path.resolve(word)) : Option.map(cwd, (folder) => path.resolve(folder, word))),
        };
    }),
}) {
    static readonly layer = Layer.effect(this, this.make);
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Host, HostError, KEEP_SUCCESSES };
