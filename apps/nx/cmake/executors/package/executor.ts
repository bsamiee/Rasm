import { executor } from '@rasm/nx-devkit/executor';
import { ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, FileSystem, Path, Schema } from 'effect';
import { sequenced } from '#/executors/cmake/executor';

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

const execute = executor(
    Package,
    Effect.fnUntraced(function* ({ cwd, configure, build, pack, config, outputPath, args = [] }, context, project) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const [staging, folder] = [yield* fs.makeTempDirectoryScoped(), path.resolve(context.root, outputPath)];
        yield* sequenced(path.resolve(context.root, cwd), [configure, build, Array.appendAll(pack, ['--config', path.resolve(context.root, config), '-B', staging, ...args])]);
        const packages = yield* Effect.filter(yield* fs.readDirectory(staging), (name) => Effect.map(fs.stat(path.join(staging, name)), ({ type }) => type === 'File'));
        yield* fs.remove(folder, { recursive: true, force: true });
        yield* fs.makeDirectory(folder, { recursive: true });
        yield* Effect.forEach(packages, (name) => fs.copyFile(path.join(staging, name), path.join(folder, name)), { discard: true });
        return yield* Array.match(packages, { onEmpty: () => ToolError.make({ subject: project, detail: 'cpack wrote no package file' }), onNonEmpty: () => Effect.succeed(true) });
    }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Package };
