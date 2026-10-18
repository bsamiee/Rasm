import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { type ExecutorContext, runExecutor, type Target } from '@nx/devkit';
import { Array, Cause, Effect, Exit, FileSystem, Option, Path, Schema, type Scope, Stream, String } from 'effect';
import { quote } from 'shell-quote';
import { stdout } from '../process.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const execute = (target: Target, args: { readonly args?: string }, context: ExecutorContext): Effect.Effect<void, Cause.UnknownError> =>
    Effect.tryPromise(() => runExecutor<{ success: boolean }>(target, args, context)).pipe(
        Effect.map((results) => Stream.fromAsyncIterable(results, (cause) => new Cause.UnknownError(cause, `${target.target} did not finish`))),
        Stream.unwrap,
        Stream.runForEach(({ success }) => (success ? Effect.void : Effect.fail(new Cause.UnknownError(target, `${target.target} failed`)))),
    );
const write = Effect.fnUntraced(function* (context: ExecutorContext, project: string, { target, extensions }: { readonly target: string; readonly extensions: Option.Option<readonly string[]> }, named: Option.Option<readonly string[]>) {
    const path = yield* Path.Path;
    const owns = (entry: string) => Option.match(extensions, { onNone: () => true, onSome: Array.contains(path.extname(entry)) });
    return yield* Option.match(named, {
        onNone: () => execute({ project, target }, {}, context),
        onSome: (files) => Array.match(files.filter(owns), { onEmpty: () => Effect.void, onNonEmpty: (owned) => execute({ project, target, configuration: 'files' }, { args: quote(owned) }, context) }),
    });
});
const complete = <E>(program: Effect.Effect<void, E, NodeServices | Scope.Scope>): Promise<{ success: boolean }> =>
    program.pipe(
        Effect.as({ success: true }),
        Effect.catchCause((cause) => Effect.as(Effect.logError(cause), { success: false })),
        Effect.scoped,
        Effect.provide(layer),
        Effect.runPromise,
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

export const format = (options: { readonly project: string; readonly files: readonly string[] }, context: ExecutorContext): Promise<{ success: boolean }> =>
    complete(
        Effect.gen(function* () {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const named = Array.isReadonlyArrayNonEmpty(options.files)
                ? Option.some(
                      yield* stdout('git', ['--literal-pathspecs', 'ls-files', '--cached', '--others', '--exclude-standard', '--deduplicate', '-z', '--', ...options.files], { cwd: context.root, stderr: 'inherit' }).pipe(
                          Effect.flatMap((listed) => Effect.filter(listed.split('\0').filter(String.isNonEmpty), (entry) => fs.exists(path.resolve(context.root, entry)), { concurrency: 'unbounded' })),
                          Effect.map(Array.map((entry) => (entry.startsWith('-') || entry.startsWith('@') ? `./${entry}` : entry))),
                      ),
                  )
                : Option.none();
            const writers = Object.entries(context.projectsConfigurations.projects[options.project]?.targets ?? {}).flatMap(([target, { metadata }]) =>
                Option.toArray(Option.map(Schema.decodeUnknownOption(Schema.Struct({ extensions: Schema.OptionFromNullOr(Schema.Array(Schema.String)) }))(metadata), ({ extensions }) => ({ target, extensions }))),
            );
            return yield* Effect.forEach(Object.values(Array.groupBy(writers, ({ extensions }) => Option.match(extensions, { onNone: () => '', onSome: Array.join(',') }))), (group) => Effect.exit(Effect.forEach(group, (writer) => write(context, options.project, writer, named), { discard: true })), {
                concurrency: 'unbounded',
            }).pipe(Effect.flatMap(Exit.asVoidAll));
        }),
    );
