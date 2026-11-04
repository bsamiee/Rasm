import { layer } from '@effect/platform-node/NodeServices';
import { type ExecutorContext, runExecutor, type Target, targetToTargetString } from '@nx/devkit';
import { Array, Cause, Effect, FileSystem, Filter, Function, flow, Option, Path, Record, Schema, Stream, Struct } from 'effect';
import { CommandLine } from 'effect/process';
import { accumulate } from '#/eng/typescript/accumulate';
import { runCommand } from '#/eng/typescript/run-command';

// --- [MODELS] --------------------------------------------------------------------------

const Writer = Schema.Struct({ extensions: Schema.NonEmptyArray(Schema.String) });

// --- [COMPOSITION] ---------------------------------------------------------------------

const format = flow(
    Effect.fn('format')(
        function* (options: { readonly files: readonly string[] }, context: ExecutorContext) {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const project = yield* Effect.fromOption(
                Option.flatMap(Option.fromUndefinedOr(context.projectName), (name) => Record.get(context.projectGraph.nodes, name)),
                () => new Cause.NoSuchElementError(`Project graph holds no project ${context.projectName}`),
            );
            const named = yield* runCommand
                .records('\0', 'git', ['--literal-pathspecs', 'ls-files', '--cached', '--others', '--exclude-standard', '--deduplicate', '-z', '--', ...options.files], { cwd: context.root })
                .pipe(
                    Effect.flatMap(Effect.filter((entry) => fs.exists(path.resolve(context.root, entry)), { concurrency: 'unbounded' })),
                    Effect.map(Array.map((entry) => (entry.startsWith('-') || entry.startsWith('@') ? `./${entry}` : entry))),
                    Effect.when(Effect.succeed(Array.isReadonlyArrayNonEmpty(options.files))),
                );
            const run = (target: Target, args: { readonly args?: string }) =>
                Effect.tryPromise(() => runExecutor<{ success: boolean }>(target, args, context)).pipe(
                    Effect.map((results) => Stream.fromAsyncIterable(results, Function.identity)),
                    Stream.unwrap,
                    Stream.runForEach(({ success }) => (success ? Effect.void : Effect.fail(new Cause.UnknownError(target, 'Executor reported no success')))),
                    Effect.mapError((cause) => new Cause.UnknownError(cause, `${targetToTargetString(target)} failed`)),
                );
            const writers = Array.getSomes(Record.collect(project.data.targets ?? {}, (target, { metadata }) => Option.map(Schema.decodeUnknownOption(Writer)(metadata), Struct.assign({ target }))));
            const write = ({ target, extensions }: (typeof writers)[number]) =>
                Option.match(named, {
                    onNone: () => run({ project: project.name, target }, {}),
                    onSome: flow(
                        Array.filter((entry: string) => extensions.includes(path.extname(entry))),
                        Array.match({
                            onEmpty: () => Effect.void,
                            onNonEmpty: flow(
                                CommandLine.join,
                                Effect.fromResult,
                                Effect.flatMap((args) => run({ project: project.name, target, configuration: 'files' }, { args })),
                            ),
                        }),
                    ),
                });
            const serial = writers.reduce<readonly (readonly (typeof writers)[number][])[]>((groups, writer) => {
                const [overlapping, separate] = Array.partition(groups, Filter.fromPredicate(Array.some<(typeof writers)[number]>(flow(Struct.get('extensions'), Array.intersection(writer.extensions), Array.isReadonlyArrayNonEmpty))));
                return [...separate, [...overlapping.flat(), writer]];
            }, []);
            return yield* accumulate(serial, Effect.forEach(write, { discard: true }), { concurrency: 'unbounded' });
        },
        Effect.as({ success: true }),
        Effect.tapCause(Effect.logError),
        Effect.catchCause(() => Effect.succeed({ success: false })),
        Effect.provide(layer),
    ),
    Effect.runPromise,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { format };
