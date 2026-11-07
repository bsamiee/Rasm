import { availableParallelism } from 'node:os';
import process from 'node:process';
import { Array, Cache, Config, Context, Effect, FileSystem, Formatter, Layer, Option, Path, Predicate, Schema, Semaphore, Stream, String } from 'effect';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ASCENDING = /^\.\.(?:[/\\]|$)/u;
const SPACES = /\s+/u;

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
    make: Effect.gen(function* () {
        const [fs, path, spawner, home] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner, Config.option(Config.String('HOME'))]);
        const root = path.resolve(import.meta.dirname, '../..');
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
                const child = yield* spawner.spawn(ChildProcess.make(program, args, { cwd: root, env, extendEnv: true, stdin: stdin === undefined ? 'ignore' : Stream.encodeText(Stream.make(stdin)) }));
                const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(child.stdout)), Stream.mkString(Stream.decodeText(child.stderr)), child.exitCode], { concurrency: 'unbounded' });
                return { code, stdout, stderr };
            },
            Effect.scoped,
            (effect) => slots.withPermit(effect),
            (effect, ...[argv]) => Effect.mapError(effect, HookError.of(argv[0])),
        );
        const output = (argv: Array.NonEmptyReadonlyArray<string>) =>
            run(argv).pipe(
                Effect.map(({ stdout }) => stdout),
                Effect.orElseSucceed(() => ''),
            );
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
                Effect.flatMap(({ workspaceDataDirectory }) => fs.stat(path.join(workspaceDataDirectory, 'project-graph.json'))),
                Effect.map(({ mtime }) => Option.match(mtime, { onNone: () => 0, onSome: (written) => written.getTime() })),
                Effect.orElseSucceed(() => 0),
                Effect.flatMap((written) => Cache.get(graphs, written)),
            ),
            place: (word: string, cwd: Option.Option<string>): Option.Option<string> => (path.isAbsolute(word) ? Option.some(path.resolve(word)) : Option.map(cwd, (folder) => path.resolve(folder, word))),
            inside: (file: string, folder: string) => !ASCENDING.test(path.relative(folder, file)),
        };
    }),
}) {
    static readonly layer = Layer.effect(this, this.make);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const parentsOf = (table: string): ReadonlyMap<string, string> => new Map(Array.chunksOf(table.trim().split(SPACES), 2).map(([pid, parent = '']) => [pid, parent]));

// --- [EXPORTS] -------------------------------------------------------------------------

export { HookError, Host };
