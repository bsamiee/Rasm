import { createRequire, findPackageJSON } from 'node:module';
import { pathToFileURL } from 'node:url';
import { runtime, ToolError } from '@rasm/nx-devkit/process';
import { served } from '@rasm/nx-devkit/reply';
import { Array, Effect, FileSystem, Option, Path, Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Job = Schema.Struct({ root: Schema.String, file: Schema.String }).annotate({ description: 'One Storybook main config read' });
const Report = Schema.Struct({ packages: Schema.Array(Schema.String) }).annotate({ description: 'npm packages the main config names as its framework and addons' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.fnUntraced(function* ({ root, file }: typeof Job.Type, workspaceRoot: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const [folder, main] = [path.join(workspaceRoot, root), path.join(workspaceRoot, file)];
    const { isInNodeModules, loadMainConfig, normalizeAddonName } = yield* Effect.tryPromise({
        try: (): Promise<typeof import('storybook/internal/common')> => import(pathToFileURL(createRequire(path.join(folder, 'package.json')).resolve('storybook/internal/common')).href),
        catch: ToolError.of(file, `resolves no storybook from ${root}`),
    });
    const { framework, addons = [] } = yield* Effect.tryPromise({ try: () => loadMainConfig({ configDir: path.dirname(main), cwd: folder, skipCache: true }), catch: ToolError.of(file, 'does not load as a Storybook main config') });
    const named = Array.getSomes([...Option.toArray(Option.fromUndefinedOr(framework)), ...addons].map((entry) => Option.filter(Option.fromUndefinedOr(normalizeAddonName(entry)), (name) => !path.isAbsolute(name))));
    const packages = yield* Effect.filter(Array.dedupe(named), (name) =>
        Option.match(Option.flatMap(Option.liftThrowable(findPackageJSON)(name, pathToFileURL(main)), Option.fromUndefinedOr), {
            onNone: () => Effect.succeed(true),
            onSome: (at) => Effect.map(Effect.option(fs.realPath(at)), Option.exists(isInNodeModules)),
        }),
    );
    return { report: Report.make({ packages }), sources: [] };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

if (import.meta.main) {
    runtime.runPromise(served(Job, Report, read));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Job, Report };
