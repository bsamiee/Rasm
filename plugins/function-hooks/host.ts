import { availableParallelism } from 'node:os';
import process from 'node:process';
import { Completed } from '@rasm/process';
import { Array, Cache, Config, Context, Effect, FileSystem, Formatter, Layer, Option, Path, Predicate, Record, Schema, Semaphore, Stream, String } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ASCENDING = /^\.\.(?:[/\\]|$)/u;
const SPACES = /\s+/u;
const STEP_FILES = /\{\{\s*files\s*\}\}/u;

// --- [MODELS] --------------------------------------------------------------------------

const Run = Schema.Struct({ command: Schema.Union([Schema.String, Schema.Struct({ argv: Schema.Array(Schema.String) })]) });
const Steps = Schema.fromJsonString(Schema.Struct({ steps: Schema.Record(Schema.String, Schema.Record(Schema.String, Schema.Unknown)) }));

// --- [ERRORS] --------------------------------------------------------------------------

class HookError extends Schema.TaggedError<HookError, { readonly brand: unique symbol }>()('HookError', { program: Schema.String, cause: Schema.Defect() }) {
    static readonly of =
        (program: string) =>
        (cause: unknown): HookError =>
            HookError.make({ program, cause });

    override get message() {
        return `${this.program}: ${String.trim(Predicate.isString(this.cause) ? this.cause : Formatter.format(this.cause))}`;
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

class Host extends Context.Service<Host>()('@rasm/function-hooks/host', {
    make: Effect.fnUntraced(function* (root: string) {
        const [fs, path, spawner, home] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner, Config.option(Config.String('HOME'))]);
        const environment = (name: string) => Effect.orElseSucceed(Config.option(Config.String(name)), Option.none);
        const graphs = yield* Cache.make({
            capacity: 1,
            lookup: (_written: number) =>
                Effect.map(
                    Effect.promise(() => import('@nx/devkit')),
                    ({ readCachedProjectGraph }) => Option.liftThrowable(readCachedProjectGraph)(),
                ),
        });
        const slots = yield* Semaphore.make(availableParallelism());
        const run = Effect.fnUntraced(
            function* ([program, ...args]: Array.NonEmptyReadonlyArray<string>, { env = {}, stdin }: { readonly env?: Readonly<Record<string, string>>; readonly stdin?: string } = {}) {
                return yield* Completed.run(ChildProcess.make(program, args, { cwd: root, env, extendEnv: true, stdin: stdin === undefined ? 'ignore' : Stream.encodeText(Stream.make(stdin)) }));
            },
            Effect.provideService(ChildProcessSpawner.ChildProcessSpawner, spawner),
            (effect) => slots.withPermit(effect),
            (effect, ...[argv]) => Effect.mapError(effect, HookError.of(argv[0])),
        );
        const output = (argv: Array.NonEmptyReadonlyArray<string>) =>
            run(argv).pipe(
                Effect.map(({ stdout }) => stdout),
                Effect.orElseSucceed(() => ''),
            );
        const written = (file: string) =>
            fs.stat(file).pipe(
                Effect.map(({ mtime }) => Option.match(mtime, { onNone: () => 0, onSome: (date) => date.getTime() })),
                Effect.orElseSucceed(() => 0),
            );
        const hooks = path.join(root, '.config/hk.pkl');
        const stepped = yield* Cache.make({ capacity: 1, lookup: (_written: number) => Effect.map(output(['pkl', 'eval', '--format', 'json', hooks]), runsOf) });
        return {
            root,
            home,
            fs,
            path,
            run,
            environment,
            output,
            parent: `${process.ppid}`,
            parents: Effect.map(output(['ps', '-A', '-o', 'pid=,ppid=']), parentsOf),
            exists: (file: string) => Effect.orElseSucceed(fs.exists(file), () => false),
            real: (file: string) => Effect.orElseSucceed(fs.realPath(file), () => file),
            installed: (program: string) => Effect.map(run(['which', program]), ({ code }) => code === 0),
            graph: Effect.promise(() => import('@nx/devkit/internal')).pipe(
                Effect.flatMap(({ workspaceDataDirectory }) => written(path.join(workspaceDataDirectory, 'project-graph.json'))),
                Effect.flatMap((at) => Cache.get(graphs, at)),
            ),
            steps: Effect.flatMap(written(hooks), (at) => Cache.get(stepped, at)),
            place: (word: string, cwd: Option.Option<string>): Option.Option<string> => (path.isAbsolute(word) ? Option.some(path.resolve(word)) : Option.map(cwd, (folder) => path.resolve(folder, word))),
            inside: (file: string, folder: string) => !ASCENDING.test(path.relative(folder, file)),
        };
    }),
}) {
    static readonly layer = (root: string) => Layer.effect(this, this.make(root));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const parentsOf = (table: string): ReadonlyMap<string, string> => new Map(Array.chunksOf(table.trim().split(SPACES), 2).map(([pid, parent = '']) => [pid, parent]));

const runsOf = (text: string) =>
    Option.toArray(Schema.decodeOption(Steps)(text))
        .flatMap(({ steps }) => Record.toEntries(steps))
        .flatMap(([step, modes]) => Record.collect(Record.filter(modes, Schema.is(Run)), (mode, { command }) => ({ step, mode, parts: (Predicate.isString(command) ? command : command.argv.join(' ')).split(STEP_FILES) })))
        .filter(({ parts }) => parts.length > 1)
        .map(({ step, mode, parts }) => ({ step, mode, text: parts.join('') }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { HookError, Host };
