import { NodeServices } from '@effect/platform-node';
import { type ExecutorContext, runExecutor, type Target, targetToTargetString } from '@nx/devkit';
import { Array, Cause, Effect, FileSystem, flow, Option, Path, Record, Schema, Stream } from 'effect';
import { CommandLine } from 'effect/process';
import { accumulate } from '#/eng/typescript/accumulate';
import { records } from '#/eng/typescript/process';

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
            const named = yield* records('\0', 'git', ['--literal-pathspecs', 'ls-files', '--cached', '--others', '--exclude-standard', '--deduplicate', '-z', '--', ...options.files], { cwd: context.root }).pipe(
                Effect.flatMap(Effect.filter((entry) => fs.exists(path.resolve(context.root, entry)), { concurrency: 'unbounded' })),
                Effect.map(Array.map((entry) => (entry.startsWith('-') || entry.startsWith('@') ? `./${entry}` : entry))),
                Effect.when(Effect.succeed(Array.isReadonlyArrayNonEmpty(options.files))),
            );
            const run = (target: Target, args: { readonly args?: string }) =>
                Effect.tryPromise(() => runExecutor<{ success: boolean }>(target, args, context)).pipe(
                    Effect.flatMap((results) =>
                        Stream.runForEach(
                            Stream.fromAsyncIterable(results, (cause) => new Cause.UnknownError(cause)),
                            (report) => (report.success ? Effect.void : Effect.fail(new Cause.UnknownError(report, 'Executor reported no success'))),
                        ),
                    ),
                    Effect.mapError((cause) => new Cause.UnknownError(cause, `${targetToTargetString(target)} failed`)),
                );
            const writers = Array.getSomes(Record.collect(project.data.targets ?? {}, (target, { metadata }) => Option.map(Schema.decodeUnknownOption(Writer)(metadata), ({ extensions }) => ({ target, extensions }))));
            const write = ({ target, extensions }: (typeof writers)[number]) =>
                Option.match(named, {
                    onNone: () => run({ project: project.name, target }, {}),
                    onSome: (files) =>
                        Array.match(
                            files.filter((file) => extensions.includes(path.extname(file))),
                            { onEmpty: () => Effect.void, onNonEmpty: (owned) => Effect.flatMap(Effect.fromResult(CommandLine.join(owned)), (args) => run({ project: project.name, target, configuration: 'files' }, { args })) },
                        ),
                });
            const serial = writers.reduce<readonly (readonly (typeof writers)[number][])[]>((groups, writer) => {
                const overlaps = (group: readonly (typeof writers)[number][]) => group.some(({ extensions }) => Array.isReadonlyArrayNonEmpty(Array.intersection(extensions, writer.extensions)));
                return [...groups.filter((group) => !overlaps(group)), [...groups.filter(overlaps).flat(), writer]];
            }, []);
            return yield* accumulate(serial, (group) => Effect.forEach(group, write, { discard: true }), { concurrency: 'unbounded' });
        },
        Effect.as({ success: true }),
        Effect.tapCause(Effect.logError),
        Effect.catchCause(() => Effect.succeed({ success: false })),
        Effect.provide(NodeServices.layer),
    ),
    Effect.runPromise,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { format };
