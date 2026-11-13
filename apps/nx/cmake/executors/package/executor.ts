import { join, resolve } from 'node:path';
import type { ExecutorContext } from '@nx/devkit';
import { Array, Effect, FileSystem, Schema } from 'effect';
import { run, sequenced } from '#/executors/cmake/executor';
import { refused } from '#/workspace';

// --- [MODELS] --------------------------------------------------------------------------

const Invocation = Schema.NonEmptyArray(Schema.String);
const Package = Schema.Struct({
    cwd: Schema.String.annotate({ description: 'Folder holding CMakePresets.json, relative to the workspace root, every invocation runs in it' }),
    configure: Invocation.annotate({ description: 'The cmake invocation that configures the build tree of the packaging workflow' }),
    build: Invocation.annotate({ description: "The cmake --build invocation of the project's targets in that tree" }),
    pack: Invocation.annotate({ description: 'The cpack invocation naming the package preset, without a configuration file or a package directory' }),
    config: Schema.String.annotate({ description: "The project's CPack configuration file, relative to the workspace root" }),
    outputPath: Schema.String.annotate({ description: 'Folder, relative to the workspace root, receiving the package files alone' }),
    args: Schema.optionalKey(Schema.Array(Schema.String)).annotate({ description: 'Arguments appended to the cpack invocation' }),
}).annotate({
    title: 'Package',
    description: "Configures the packaging workflow's build tree, builds the project's targets from its manifest as it stands, and packs the project's install tree with CPack into a staging folder, then replaces the output folder with the package files CPack named from the project name, version, system, and processor",
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = Effect.fnUntraced(function* ({ cwd, configure, build, pack, config, outputPath, args = [] }: typeof Package.Type, context: ExecutorContext) {
    const fs = yield* FileSystem.FileSystem;
    const [staging, folder] = [yield* fs.makeTempDirectoryScoped(), resolve(context.root, outputPath)];
    yield* sequenced(resolve(context.root, cwd), [configure, build, Array.appendAll(pack, ['--config', resolve(context.root, config), '-B', staging, ...args])]);
    const packages = yield* Effect.filter(yield* fs.readDirectory(staging), (name) => Effect.map(fs.stat(join(staging, name)), ({ type }) => type === 'File'));
    yield* fs.remove(folder, { recursive: true, force: true });
    yield* fs.makeDirectory(folder, { recursive: true });
    yield* Effect.forEach(packages, (name) => fs.copyFile(join(staging, name), join(folder, name)), { discard: true });
    yield* Array.match(packages, { onEmpty: () => Effect.fail(refused(outputPath, 'receives no package file from cpack')()), onNonEmpty: () => Effect.void });
    return { success: true };
}, run);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Package };
