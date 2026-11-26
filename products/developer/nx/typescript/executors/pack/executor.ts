import { join } from 'node:path';
import { executor, spawned } from '@rasm/nx/inference';
import { Effect, FileSystem, Schema, UndefinedOr } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Pack = Schema.Struct({
    argv: Schema.NonEmptyArray(Schema.String).annotate({ description: "Pack command of the workspace's package manager with its arguments, run in the project folder" }),
    outputPath: Schema.optionalKey(Schema.String).annotate({ description: 'Folder, relative to the workspace root, the tarball goes to, made before the pack runs' }),
}).annotate({
    title: 'Pack',
    description: "Packs the project into one tarball with its package manager's pack command, making the outputPath folder first, as npm pack refuses a --pack-destination folder that does not exist",
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Pack, function* ({ argv, outputPath }, context) {
    const fs = yield* FileSystem.FileSystem;
    const { root } = (yield* Effect.flatMap(Effect.fromNullishOr(context.projectName), (project) => Effect.fromNullishOr(context.projectGraph.nodes[project]))).data;
    yield* UndefinedOr.match(outputPath, { onUndefined: () => Effect.void, onDefined: (at) => fs.makeDirectory(join(context.root, at), { recursive: true }) });
    yield* spawned(argv, { cwd: join(context.root, root), stdout: 'inherit', stderr: 'inherit' });
    return { success: true };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Pack };
