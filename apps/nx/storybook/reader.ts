import { findPackageJSON } from 'node:module';
import { pathToFileURL } from 'node:url';
import { runtime, ToolError } from '@rasm/nx-devkit/process';
import { loaded, served } from '@rasm/nx-devkit/reply';
import { Array, Effect, FileSystem, Option, Path, pipe, Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Job = Schema.Struct({ root: Schema.String, file: Schema.String }).annotate({ description: 'One Storybook main config read' });
const Report = Schema.Struct({ packages: Schema.Array(Schema.String), signals: Schema.Boolean }).annotate({ description: 'npm packages the main config names as its framework and addons, and whether its addons hold the preset reporting the development server address to Nx' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.fnUntraced(function* ({ root, file }: typeof Job.Type, workspaceRoot: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const [folder, main] = [path.join(workspaceRoot, root), path.join(workspaceRoot, file)];
    const { isInNodeModules, loadMainConfig, normalizeAddonName } = yield* loaded<typeof import('storybook/internal/common')>(folder, 'storybook/internal/common');
    const { framework, addons = [] } = yield* Effect.tryPromise({ try: () => loadMainConfig({ configDir: path.dirname(main), cwd: folder, skipCache: true }), catch: ToolError.of(file, 'does not load as a Storybook main config') });
    const named = Array.getSomes([...Option.toArray(Option.fromUndefinedOr(framework)), ...addons].map((entry) => Option.filter(Option.fromUndefinedOr(normalizeAddonName(entry)), (name) => !path.isAbsolute(name))));
    const located = yield* Effect.forEach(Array.dedupe(named), (name) =>
        pipe(
            Option.liftThrowable(findPackageJSON)(name, pathToFileURL(main)),
            Option.flatMap(Option.fromUndefinedOr),
            Option.map((at) => Effect.option(fs.realPath(at))),
            Effect.transposeOption,
            Effect.map((manifest) => ({ name, manifest })),
        ),
    );
    const packages = located.filter(({ manifest }) => Option.match(manifest, { onNone: () => true, onSome: Option.exists(isInNodeModules) })).map(({ name }) => name);
    const own = findPackageJSON(import.meta.url);
    const signals = located.some(({ manifest }) => Option.exists(Option.flatten(manifest), (at) => at === own));
    return { report: Report.make({ packages, signals }), sources: [] };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

if (import.meta.main) {
    runtime.runPromise(served(Job, Report, read));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Job, Report };
