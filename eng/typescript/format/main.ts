import { NodeServices } from '@effect/platform-node';
import { type ExecutorContext, normalizePath, runExecutor, type Target } from '@nx/devkit';
import { Array, Effect, FileSystem, Option, Path, Schema, type Scope, Stream } from 'effect';
import { ChildProcess, type ChildProcessSpawner } from 'effect/process';
import { quote } from 'shell-quote';

// --- [ERRORS] --------------------------------------------------------------------------

const Reason = Schema.TaggedUnion({
    Executor: { target: Schema.String, cause: Schema.Defect() },
    Writer: { target: Schema.String },
    Git: { code: Schema.Finite },
});
class FormatError extends Schema.TaggedError<FormatError, { readonly brand: unique symbol }>()('FormatError', { reason: Reason }) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const execute = (target: Target, args: { readonly args?: string }, context: ExecutorContext): Effect.Effect<void, FormatError> => {
    const failed = (cause: unknown): FormatError => FormatError.make({ reason: { _tag: 'Executor', target: target.target, cause } });
    return Stream.fromEffect(Effect.tryPromise({ try: () => runExecutor<{ success: boolean }>(target, args, context), catch: failed })).pipe(
        Stream.flatMap((results) => Stream.fromAsyncIterable(results, failed)),
        Stream.runForEach(({ success }) => (success ? Effect.void : FormatError.make({ reason: { _tag: 'Writer', target: target.target } }))),
    );
};
const compose = (invoke: (target: string, extensions: Option.Option<readonly string[]>) => Effect.Effect<void, FormatError>): Effect.Effect<void, Array.NonEmptyArray<FormatError>> =>
    Effect.validate(
        [
            { targets: ['format:biome'], extensions: Option.none() },
            { targets: ['format:ruff'], extensions: Option.some(['.py', '.pyi', '.pyw', '.ipynb']) },
            { targets: ['format:yamlfmt'], extensions: Option.some(['.yaml', '.yml']) },
            { targets: ['format:tombi'], extensions: Option.some(['.toml']) },
            { targets: ['format:google-java-format'], extensions: Option.some(['.java']) },
            { targets: ['format:swiftlint', 'format:swift-format'], extensions: Option.some(['.swift']) },
            { targets: ['format:rustfmt'], extensions: Option.some(['.rs']) },
            { targets: ['format:shfmt'], extensions: Option.some(['.sh']) },
        ],
        ({ targets, extensions }) => Effect.forEach(targets, (target) => invoke(target, extensions), { discard: true }),
        { concurrency: 'unbounded', discard: true },
    );
const complete = <E>(program: Effect.Effect<void, E, FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner | Scope.Scope>): Promise<{ success: boolean }> =>
    program.pipe(
        Effect.as({ success: true }),
        Effect.catchCause((cause) => Effect.as(Effect.logError(cause), { success: false })),
        Effect.scoped,
        Effect.provide(NodeServices.layer),
        Effect.runPromise,
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

export const formatWorkspace = (options: { readonly project: string }, context: ExecutorContext): Promise<{ success: boolean }> => complete(compose((target) => execute({ project: options.project, target }, {}, context)));

export const formatFiles = (options: { readonly project: string; readonly files: readonly string[] }, context: ExecutorContext): Promise<{ success: boolean }> =>
    complete(
        Effect.gen(function* () {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const handle = yield* ChildProcess.make('git', ['ls-files', '--cached', '--others', '--exclude-standard', '-z'], { cwd: context.root, stderr: 'inherit' });
            const [text, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), handle.exitCode], { concurrency: 'unbounded' });
            if (code !== 0) {
                return yield* FormatError.make({ reason: { _tag: 'Git', code } });
            }
            const argued = Array.dedupe(options.files.map((entry) => normalizePath(path.relative(context.root, path.resolve(context.root, entry)))));
            const eligible = Array.intersection(
                argued,
                text.split('\0').filter((entry) => entry !== ''),
            );
            const files = yield* Effect.filter(eligible, (entry) => fs.exists(path.resolve(context.root, entry)), { concurrency: 'unbounded' });
            return yield* compose((target, extensions) => {
                const named = Option.match(extensions, { onNone: () => files, onSome: (listed) => files.filter((entry) => listed.includes(path.extname(entry))) });
                return named.length === 0 ? Effect.void : execute({ project: options.project, target, configuration: 'files' }, { args: quote(named.map((entry) => (entry.startsWith('-') || entry.startsWith('@') ? `./${entry}` : entry))) }, context);
            });
        }),
    );
