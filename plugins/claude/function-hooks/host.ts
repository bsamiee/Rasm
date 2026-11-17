import { availableParallelism } from 'node:os';
import process from 'node:process';
import { Installation } from '@rasm/applications';
import { Array, Cache, Config, Context, Duration, Effect, Exit, FileSystem, Formatter, Function, Layer, Option, Path, Predicate, Record, Schema, Semaphore, Stream, String } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';
import { WATCH_APP } from '#/shell/programs';

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

class Machine extends Context.Service<Machine>()('@rasm/function-hooks/host/Machine', {
    make: Effect.gen(function* () {
        const [fs, path, spawner, home] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner, Config.option(Config.String('HOME'))]);
        const nx = Effect.promise(() => Promise.all([import('@nx/devkit'), import('@nx/devkit/internal')]));
        const slots = yield* Semaphore.make(availableParallelism());
        const run = Effect.fnUntraced(
            function* ([program, ...args]: Array.NonEmptyReadonlyArray<string>, { cwd, env = {}, stdin }: { readonly cwd: string; readonly env?: Readonly<Record<string, string>>; readonly stdin?: string }) {
                const handle = yield* spawner.spawn(ChildProcess.make(program, args, { cwd, env, extendEnv: true, stdin: stdin === undefined ? 'ignore' : Stream.encodeText(Stream.make(stdin)) }));
                return yield* Effect.all({ stdout: Stream.mkString(Stream.decodeText(handle.stdout)), stderr: Stream.mkString(Stream.decodeText(handle.stderr)), code: handle.exitCode }, { concurrency: 'unbounded' });
            },
            Effect.scoped,
            (effect) => slots.withPermit(effect),
            (effect, ...[argv]) => Effect.mapError(effect, HostError.of(argv[0])),
        );
        const modified = (file: string) =>
            fs.stat(file).pipe(
                Effect.map(({ mtime }) => Option.match(mtime, { onNone: () => 0, onSome: (date) => date.getTime() })),
                Effect.orElseSucceed(() => 0),
            );
        const inside = (file: string, folder: string) => !ASCENDING.test(path.relative(folder, file));
        const real = (file: string) => Effect.orElseSucceed(fs.realPath(file), () => file);
        const toplevels = yield* Cache.makeWith(
            Effect.fnUntraced(function* (cwd: string) {
                const ancestors = Array.reverse(Array.scan(cwd.split(path.sep).filter(String.isNonEmpty), path.sep, (folder, segment) => path.join(folder, segment)));
                return Option.getOrElse(yield* Effect.findFirst(ancestors, (folder) => Effect.orElseSucceed(fs.exists(path.join(folder, '.git')), Function.constFalse)), () => cwd);
            }),
            { capacity: 64 },
        );
        const graphs = yield* Cache.make({
            capacity: 1,
            lookup: Effect.fnUntraced(function* (_modified: number) {
                const [{ readCachedProjectGraph, workspaceRoot }, { createProjectRootMappings, findProjectForPath }] = yield* nx;
                return Option.map(Option.liftThrowable(readCachedProjectGraph)(), ({ nodes }) => {
                    const roots = createProjectRootMappings(nodes);
                    const owner = (file: string) => {
                        const name = findProjectForPath(path.relative(workspaceRoot, file) || '.', roots);
                        return Option.fromNullishOr(name === null ? undefined : nodes[name]);
                    };
                    return { nodes, owner };
                });
            }),
        });
        const steps = yield* Cache.makeWith(
            Effect.fnUntraced(function* ([hooks, at]: readonly [hooks: string, modified: number]) {
                const { stdout } = at === 0 ? { stdout: '' } : yield* run(['pkl', 'eval', '--format', 'json', hooks], { cwd: path.dirname(hooks) });
                return Option.toArray(Schema.decodeOption(Steps)(stdout)).flatMap(({ steps: decoded }) => Record.toEntries(decoded));
            }),
            { capacity: 8, ...KEEP_SUCCESSES },
        );
        const onPath = yield* Cache.makeWith((program: string) => Effect.map(run(['which', program], { cwd: process.cwd() }), ({ code }) => code === 0), {
            capacity: 1024,
            timeToLive: Exit.match({ onSuccess: (found: boolean) => (found ? Duration.infinity : Duration.minutes(1)), onFailure: () => Duration.zero }),
        });
        const ignored = yield* Cache.makeWith(
            Effect.fnUntraced(function* ([root, , files]: readonly [root: string, modified: number, files: readonly string[]]) {
                const reals = yield* Effect.forEach(files, real, { concurrency: 'unbounded' });
                const owned = reals.filter((file) => inside(file, root));
                return owned.length > 0 && (yield* run(['git', 'check-ignore', '--no-index', '--stdin', '-z'], { cwd: root, stdin: owned.join('\0') })).code === 0;
            }),
            { capacity: 1024, ...KEEP_SUCCESSES },
        );
        const tracked = yield* Cache.makeWith(([root, , file]: readonly [root: string, modified: number, file: string]) => Effect.map(run(['git', 'ls-files', '--error-unmatch', '--', file], { cwd: root }), ({ code }) => code === 0), { capacity: 1024, ...KEEP_SUCCESSES });
        return {
            fs,
            path,
            home,
            run,
            inside,
            toplevel: (cwd: string) => Cache.get(toplevels, cwd),
            graph: Effect.fnUntraced(function* (root: string) {
                const [{ workspaceRoot }, { workspaceDataDirectory }] = yield* nx;
                return root === workspaceRoot ? yield* Cache.get(graphs, yield* modified(path.join(workspaceDataDirectory, 'project-graph.json'))) : Option.none();
            }),
            steps: (root: string) => {
                const hooks = path.join(root, '.config/hk.pkl');
                return Effect.flatMap(modified(hooks), (at) => Cache.get(steps, [hooks, at]));
            },
            onPath: (program: string) => Cache.get(onPath, program),
            ignored: (root: string, files: readonly string[]) => Effect.flatMap(modified(path.join(root, '.gitignore')), (at) => Cache.get(ignored, [root, at, files])),
            tracked: (root: string, file: string) => Effect.flatMap(modified(path.join(root, '.git/index')), (at) => Cache.get(tracked, [root, at, file])),
        };
    }),
}) {
    static readonly layer = Layer.effect(this, this.make);
}

class Host extends Context.Service<Host>()('@rasm/function-hooks/host', {
    make: Effect.fnUntraced(function* (cwd: string) {
        const machine = yield* Machine;
        const { fs, path, home, inside } = machine;
        const root = yield* machine.toplevel(cwd);
        const run = (argv: Array.NonEmptyReadonlyArray<string>, options: { readonly env?: Readonly<Record<string, string>>; readonly stdin?: string } = {}) => machine.run(argv, { ...options, cwd: root });
        const output = (argv: Array.NonEmptyReadonlyArray<string>) => Effect.map(run(argv), ({ stdout }) => stdout);
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
            onPath: machine.onPath,
            ignored: (files: readonly string[]) => machine.ignored(root, files),
            tracked: (file: string) => (inside(file, root) ? machine.tracked(root, file) : Effect.succeed(false)),
            installations: (names: readonly string[]) => Effect.map(output([WATCH_APP, 'find', ...names]), (text) => Array.getSomes(text.split('\n').map((line) => Schema.decodeOption(Installations)(line))).flatMap((found) => Object.values(found).flat())),
            graph: machine.graph(root),
            steps: Effect.map(
                machine.steps(root),
                Function.flow(
                    Array.flatMap(([step, modes]) => Record.collect(Record.filter(modes, Schema.is(Run)), (mode, { command }) => ({ step, mode, parts: (Predicate.isString(command) ? command : command.argv.join(' ')).split(STEP_FILES) }))),
                    Array.filter(({ parts }) => parts.length > 1),
                    Array.map(({ step, mode, parts }) => ({ step, mode, text: parts.join('') })),
                ),
            ),
            resolve: (word: string, folder: Option.Option<string>): Option.Option<string> => (path.isAbsolute(word) ? Option.some(path.resolve(word)) : Option.map(folder, (base) => path.resolve(base, word))),
        };
    }),
}) {
    static readonly layer = Layer.effect(this, this.make(process.cwd())).pipe(Layer.provideMerge(Machine.layer));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Host, HostError, KEEP_SUCCESSES, Machine };
