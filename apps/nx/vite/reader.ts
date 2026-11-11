import { runtime, ToolError } from '@rasm/nx-devkit/process';
import { loaded, outermost, served } from '@rasm/nx-devkit/reply';
import { Effect, FileSystem, Path, Schema } from 'effect';
import { NAME } from '#/ready-signal';

// --- [MODELS] --------------------------------------------------------------------------

const Job = Schema.Struct({ root: Schema.String, file: Schema.String, outDir: Schema.optionalKey(Schema.String) }).annotate({ description: 'One Vite config read as a production build, under the output folder its target passes' });
const Report = Schema.Struct({ folders: Schema.optionalKey(Schema.Array(Schema.String)), signals: Schema.Boolean }).annotate({
    description: 'Workspace-relative folders the build writes, absent where the config builds nothing, and whether its development and preview servers report their address to Nx',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.fnUntraced(function* ({ root, file, outDir }: typeof Job.Type, workspaceRoot: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folder = path.join(workspaceRoot, root);
    const { resolveConfig } = yield* loaded<typeof import('vite')>(folder, 'vite');
    const config = yield* Effect.tryPromise({
        try: () => resolveConfig({ configFile: path.join(workspaceRoot, file), root: folder, logLevel: 'silent', build: outDir === undefined ? {} : { outDir } }, 'build', 'production', 'production'),
        catch: ToolError.of(file, 'does not resolve as a Vite config'),
    });
    const builds = [config.build, ...Object.values(config.environments).map((environment) => environment.build)];
    const buildable = config.build.lib !== false || config.builder?.buildApp !== undefined || builds.some((build) => build.rolldownOptions.input !== undefined) || (yield* Effect.mapError(fs.exists(path.join(config.root, 'index.html')), ToolError.of(file, 'has an unreadable root')));
    const folders = outermost(
        path,
        workspaceRoot,
        builds.map((build) => path.resolve(config.root, build.outDir)),
    );
    return { report: Report.make({ ...(buildable && { folders }), signals: config.plugins.some(({ name }) => name === NAME) }), sources: config.configFileDependencies };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

if (import.meta.main) {
    runtime.runPromise(served(Job, Report, read));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Job, Report };
