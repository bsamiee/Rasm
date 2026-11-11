import { runtime, ToolError } from '@rasm/nx-devkit/process';
import { loaded, outermost, served } from '@rasm/nx-devkit/reply';
import { Effect, FileSystem, MutableRef, Option, Path, Record, Schema } from 'effect';
import { NAME } from '#/ready-signal';

// --- [MODELS] --------------------------------------------------------------------------

const Job = Schema.Struct({ root: Schema.String, file: Schema.String, outDir: Schema.optionalKey(Schema.String) }).annotate({ description: 'One Vite config read as a production build, under the output folder its target passes' });
const Report = Schema.Struct({ folders: Schema.optionalKey(Schema.Array(Schema.String)), signals: Schema.Boolean, ports: Schema.optionalKey(Schema.Struct({ serve: Schema.Int, preview: Schema.Int })) }).annotate({
    description:
        'Workspace-relative folders the build writes, absent where the config builds nothing; whether its development and preview servers report their address to Nx; and the ports those servers resolve to, absent for a config that neither reports its address nor states a server, or does not resolve while serving',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.fnUntraced(function* ({ root, file, outDir }: typeof Job.Type, workspaceRoot: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folder = path.join(workspaceRoot, root);
    const { resolveConfig } = yield* loaded<typeof import('vite')>(folder, 'vite');
    const stated = MutableRef.make(false);
    const capture = { name: 'nx-vite-stated-server', enforce: 'post', config: { order: 'post', handler: (user: { readonly server?: unknown }) => MutableRef.update(stated, (held) => held || user.server !== undefined) } } as const;
    const resolved = (command: 'build' | 'serve', mode: string, build: { readonly outDir?: string }) =>
        Effect.tryPromise({ try: () => resolveConfig({ configFile: path.join(workspaceRoot, file), root: folder, logLevel: 'silent', build, plugins: [capture] }, command, mode, mode), catch: ToolError.of(file, 'does not resolve as a Vite config') });
    const config = yield* resolved('build', 'production', outDir === undefined ? {} : { outDir });
    const serving = yield* Effect.option(resolved('serve', 'development', {}));
    const builds = [config.build, ...Object.values(config.environments).map((environment) => environment.build)];
    const buildable = config.build.lib !== false || config.builder?.buildApp !== undefined || builds.some((build) => build.rolldownOptions.input !== undefined) || (yield* Effect.mapError(fs.exists(path.join(config.root, 'index.html')), ToolError.of(file, 'has an unreadable root')));
    const folders = outermost(
        path,
        workspaceRoot,
        builds.map((build) => path.resolve(config.root, build.outDir)),
    );
    const signals = config.plugins.some(({ name }) => name === NAME);
    const ports = Option.map(
        Option.filter(serving, () => signals || MutableRef.get(stated)),
        ({ server, preview }) => ({ serve: server.port, preview: preview.port }),
    );
    return { report: Report.make({ ...(buildable && { folders }), signals, ...Record.getSomes({ ports }) }), sources: config.configFileDependencies };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

if (import.meta.main) {
    runtime.runPromise(served(Job, Report, read));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Job, Report };
