import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Array, Effect, FileSystem, Option, Path, Record, Schema } from 'effect';
import { PACKAGES } from '#/plugins/adobe-packages';
import { TypeScriptError } from '#/plugins/project-graph';

// --- [TYPES] ---------------------------------------------------------------------------

type Packed = ReturnType<(typeof PACKAGES)[keyof typeof PACKAGES]['pack']>;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String), timestampAuthority: Schema.optionalKey(Schema.String) }).annotate({ description: 'package executor options, schema.json defaults applied' });

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: unknown, context: ExecutorContext) =>
    Effect.runPromise(
        Effect.gen(function* () {
            const [fs, path, { outputPath, timestampAuthority }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Schema.decodeUnknownEffect(Options)(options)]);
            const graph = yield* Effect.fromNullishOr(context.taskGraph);
            const task = yield* Effect.fromNullishOr(Record.values(graph.tasks).find(({ target }) => target.project === context.projectName && target.target === context.targetName));
            const folders = (graph.dependencies[task.id] ?? []).flatMap(({ id }) => graph.tasks[id]?.outputs ?? []).map((output) => path.join(context.root, output));
            const packed = Effect.fnUntraced(function* (destination: string) {
                const found = yield* Effect.forEach(Array.cartesian(Record.values(PACKAGES), folders), ([row, folder]) =>
                    Effect.flatMap(fs.exists(path.join(folder, row.manifest)), (exists) =>
                        exists ? Effect.map(Effect.flatMap(fs.readFileString(path.join(folder, row.manifest)), row.named), Option.map((file) => ({ row, folder, file }))) : Effect.succeedNone,
                    ),
                );
                const packages = yield* Array.match(Array.getSomes(found), {
                    onEmpty: () => TypeScriptError.make({ message: `${task.id} found no ${Record.values(PACKAGES).map(({ manifest }) => manifest).join(' or ')} in the outputs ${folders.join(', ') || 'of the targets it depends on'}` }),
                    onNonEmpty: Effect.succeed,
                });
                const directory = path.join(context.root, destination);
                yield* fs.makeDirectory(directory, { recursive: true });
                return yield* Effect.forEach(packages, ({ row, folder, file }) =>
                    Effect.andThen(fs.remove(path.join(directory, file), { force: true }), (): Effect.Effect<boolean, Effect.Error<Packed>, Effect.Services<Packed>> => row.pack(folder, path.join(directory, file), timestampAuthority)),
                );
            });
            return { success: (yield* Option.match(Option.fromUndefinedOr(outputPath), { onNone: () => Effect.succeed([]), onSome: packed })).every(Boolean) };
        }).pipe(Effect.provide(layer)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
