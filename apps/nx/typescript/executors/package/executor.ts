import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { Array, Effect, FileSystem, Option, Path, Record, Schema, Struct } from 'effect';
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
                const manifests = Array.map(Array.cartesian(Record.values(PACKAGES), folders), ([row, folder]) => ({ row, folder, manifest: path.join(folder, row.manifest) }));
                const present = yield* Effect.filter(manifests, ({ manifest }) => fs.exists(manifest));
                const accepted = yield* Effect.filter(present, ({ row, manifest }) => Effect.flatMap(fs.readFileString(manifest), row.accepts));
                const packages = yield* Array.match(accepted, {
                    onEmpty: () => TypeScriptError.make({ message: `${task.id} found no ${Array.map(Record.values(PACKAGES), Struct.get('manifest')).join(' or ')} in the outputs ${folders.join(', ') || 'of the targets it depends on'}` }),
                    onNonEmpty: Effect.succeed,
                });
                const directory = path.join(context.root, destination);
                yield* fs.makeDirectory(directory, { recursive: true });
                const archives = new Set(Array.map(packages, ({ row }) => row.archive));
                yield* Effect.forEach(
                    (yield* fs.readDirectory(directory)).filter((name) => archives.has(path.extname(name))),
                    (name) => fs.remove(path.join(directory, name)),
                    { discard: true },
                );
                return yield* Effect.forEach(packages, ({ row, folder }): Effect.Effect<boolean, Effect.Error<Packed>, Effect.Services<Packed>> => row.pack(folder, path.join(directory, `${path.basename(task.target.project)}${row.archive}`), timestampAuthority));
            });
            return { success: (yield* Option.match(Option.fromUndefinedOr(outputPath), { onNone: () => Effect.succeed([]), onSome: packed })).every(Boolean) };
        }).pipe(Effect.provide(layer)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
