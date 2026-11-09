import { layer } from '@effect/platform-node/NodeServices';
import type { ExecutorContext } from '@nx/devkit';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Option, Path, Record, Schema, Struct } from 'effect';
import { PACKAGES, TypeScriptError } from '#/plugins/adobe-packages';

// --- [TYPES] ---------------------------------------------------------------------------

type Packed = ReturnType<(typeof PACKAGES)[keyof typeof PACKAGES]['pack']>;

// --- [MODELS] --------------------------------------------------------------------------

const Options = Schema.Struct({ outputPath: Schema.optionalKey(Schema.String), timestampAuthority: Schema.optionalKey(Schema.String) }).annotate({ description: 'package executor options, schema.json defaults applied' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const written = Effect.fnUntraced(function* (folder: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const names = yield* fs.readDirectory(folder, { recursive: true });
    return yield* Effect.filter(names.toSorted(), (name) => Effect.map(fs.stat(path.join(folder, name)), ({ type }) => type === 'File'));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = (options: unknown, context: ExecutorContext) =>
    Effect.runPromise(
        Effect.gen(function* () {
            const [fs, path, { outputPath, timestampAuthority }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Schema.decodeUnknownEffect(Options)(options)]);
            const graph = yield* Effect.fromNullishOr(context.taskGraph);
            const task = yield* Effect.fromNullishOr(Record.values(graph.tasks).find(({ target }) => target.project === context.projectName && target.target === context.targetName));
            const root = task.projectRoot ?? '';
            const folders = Array.match(graph.dependencies[task.id] ?? [], {
                onEmpty: () => [
                    {
                        folder: path.join(context.root, root),
                        listed: Effect.map(
                            Effect.promise(() => globWithWorkspaceContext(context.root, [path.join(root, '**')])),
                            Array.map((file) => path.relative(root, file)),
                        ),
                    },
                ],
                onNonEmpty: Array.flatMap(({ id }) => (graph.tasks[id]?.outputs ?? []).map((output) => ({ folder: path.join(context.root, output), listed: written(path.join(context.root, output)) }))),
            });
            const packed = Effect.fnUntraced(function* (destination: string) {
                const manifests = Array.map(Array.cartesian(Record.values(PACKAGES), folders), ([row, { folder, listed }]) => ({ row, folder, listed, manifest: path.join(folder, row.manifest) }));
                const present = yield* Effect.filter(manifests, ({ manifest }) => fs.exists(manifest));
                const accepted = yield* Effect.filter(present, ({ row, manifest }) => Effect.flatMap(fs.readFileString(manifest), row.accepts));
                const packages = yield* Array.match(accepted, {
                    onEmpty: () => TypeScriptError.make({ message: `${task.id} found no ${Array.map(Record.values(PACKAGES), Struct.get('manifest')).join(' or ')} in ${folders.map(({ folder }) => folder).join(', ')}` }),
                    onNonEmpty: Effect.succeed,
                });
                const directory = path.join(context.root, destination);
                yield* fs.makeDirectory(directory, { recursive: true });
                const archives = new Set<string>(Array.map(packages, ({ row }) => row.archive));
                yield* Effect.forEach(
                    (yield* fs.readDirectory(directory)).filter((name) => archives.has(path.extname(name))),
                    (name) => fs.remove(path.join(directory, name)),
                    { discard: true },
                );
                return yield* Effect.forEach(
                    packages,
                    Effect.fnUntraced(function* ({ row, folder, listed }) {
                        const names = yield* listed;
                        const pack: Packed = row.pack(folder, names, path.join(directory, `${path.basename(task.target.project)}${row.archive}`), timestampAuthority);
                        return yield* pack;
                    }),
                );
            });
            return { success: (yield* Option.match(Option.fromUndefinedOr(outputPath), { onNone: () => Effect.succeed([]), onSome: packed })).every(Boolean) };
        }).pipe(Effect.provide(layer)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute };
