import { layer } from '@effect/platform-node/NodeServices';
import { type ExecutorContext, runExecutor, type Target } from '@nx/devkit';
import { Array, Cause, Effect, Exit, FileSystem, flow, Option, Path, Schema, Stream, String } from 'effect';
import { quote } from 'shell-quote';
import { stdout } from '#/eng/typescript/process';

// --- [COMPOSITION] ---------------------------------------------------------------------

const format = flow(
    Effect.fn('format')(
        function* (options: { readonly project: string; readonly files: readonly string[] }, context: ExecutorContext) {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const named = Array.isReadonlyArrayNonEmpty(options.files)
                ? Option.some(
                      yield* stdout('git', ['--literal-pathspecs', 'ls-files', '--cached', '--others', '--exclude-standard', '--deduplicate', '-z', '--', ...options.files], { cwd: context.root }).pipe(
                          Effect.flatMap((listed) => Effect.filter(listed.split('\0').filter(String.isNonEmpty), (entry) => fs.exists(path.resolve(context.root, entry)), { concurrency: 'unbounded' })),
                          Effect.map(Array.map((entry) => (entry.startsWith('-') || entry.startsWith('@') ? `./${entry}` : entry))),
                      ),
                  )
                : Option.none();
            const run = (target: Target, args: { readonly args?: string }) =>
                Effect.tryPromise(() => runExecutor<{ success: boolean }>(target, args, context)).pipe(
                    Effect.map((results) => Stream.fromAsyncIterable(results, (cause) => new Cause.UnknownError(cause, `${target.target} did not finish`))),
                    Stream.unwrap,
                    Stream.runForEach(({ success }) => (success ? Effect.void : Effect.fail(new Cause.UnknownError(target, `${target.target} failed`)))),
                );
            const writers = Object.entries(context.projectsConfigurations.projects[options.project]?.targets ?? {}).flatMap(([target, { metadata }]) =>
                Option.toArray(Schema.decodeUnknownOption(Schema.Struct({ extensions: Schema.OptionFromNullOr(Schema.Array(Schema.String)) }))(metadata)).map(({ extensions }) => ({ target, extensions })),
            );
            const write = ({ target, extensions }: (typeof writers)[number]) =>
                Option.match(named, {
                    onNone: () => run({ project: options.project, target }, {}),
                    onSome: (files) =>
                        Array.match(
                            files.filter((file) => Option.match(extensions, { onNone: () => true, onSome: Array.contains(path.extname(file)) })),
                            { onEmpty: () => Effect.void, onNonEmpty: (owned) => run({ project: options.project, target, configuration: 'files' }, { args: quote(owned) }) },
                        ),
                });
            return yield* Effect.forEach(Object.values(Array.groupBy(writers, ({ extensions }) => Option.match(extensions, { onNone: () => '', onSome: Array.join(',') }))), (group) => Effect.exit(Effect.forEach(group, write, { discard: true })), { concurrency: 'unbounded' }).pipe(Effect.flatMap(Exit.asVoidAll));
        },
        Effect.as({ success: true }),
        Effect.catchCause((cause) => Effect.as(Effect.logError(cause), { success: false })),
        Effect.provide(layer),
    ),
    Effect.runPromise,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { format };
