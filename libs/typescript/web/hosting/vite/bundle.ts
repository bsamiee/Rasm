import { join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import type { CloudflareConfig } from '@cloudflare/vite-plugin';
import { layer } from '@effect/platform-node/NodeServices';
import { bindings } from 'cf/config';
import { Array, Effect, FileSystem, Function, Option, Record, Result } from 'effect';
import { CorePaths } from 'miniflare';
import type { Plugin } from 'vite';
import { App, type Binding } from '#/app/app.ts';
import { HostingError } from '#/app/error.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const START = `\0@rasm/hosting/bundle/${App.bundle.entry}`;
const STARTED = { id: new RegExp(`^${RegExp.escape(START)}$`, 'u') };

// --- [COMPOSITION] ---------------------------------------------------------------------

const nodeBundle = (entry: string): Plugin => ({
    name: '@rasm/hosting/bundle',
    config: ({ build, environments }) => ({
        builder: {},
        environments: {
            [App.bundle.environment]: {
                consumer: 'server',
                resolve: { noExternal: true },
                build: {
                    outDir: environments?.[App.bundle.environment]?.build?.outDir ?? join(build?.outDir ?? 'dist', App.bundle.environment),
                    assetsDir: 'chunks',
                    emptyOutDir: true,
                    copyPublicDir: false,
                    sourcemap: true,
                    target: 'esnext',
                    rolldownOptions: { input: { [App.bundle.entry]: START }, platform: 'node' },
                },
            },
        },
    }),
    resolveId: { filter: STARTED, handler: Function.identity },
    load: {
        filter: STARTED,
        handler: () =>
            `import { setSourceMapsSupport } from 'node:module';\nsetSourceMapsSupport(true);\nconst [{ runMain }, { default: served }] = await Promise.all([import(${JSON.stringify(fileURLToPath(import.meta.resolve('@effect/platform-node/NodeRuntime')))}), import(${JSON.stringify(entry)})]);\nrunMain(served);`,
    },
    buildApp: {
        order: 'post',
        handler: (builder) =>
            Option.fromNullishOr(builder.environments[App.bundle.environment]).pipe(
                Effect.fromOption,
                Effect.flatMap((target) => Effect.tryPromise({ try: () => builder.build(target), catch: HostingError.of('Build', `${App.bundle.environment} environment of ${entry}`) })),
                Effect.asVoid,
                Effect.runPromise,
            ),
    },
    closeBundle: {
        order: 'post',
        handler() {
            const { config } = this.environment;
            const target = this.environment.getTopLevelConfig().environments[App.bundle.environment];
            return config.consumer === 'client' && config.command === 'build' && target !== undefined
                ? FileSystem.FileSystem.use((fs) => fs.copy(resolve(config.root, config.build.outDir), join(resolve(config.root, target.build.outDir), App.bundle.assets), { overwrite: true })).pipe(Effect.provide(layer), Effect.runPromise)
                : undefined;
        },
    },
});
const worker =
    <const E extends Record.ReadonlyRecord<string, Binding>>(app: App<E>, stated: Omit<CloudflareConfig['worker'], 'name' | 'env'>) =>
    (loaded: CloudflareConfig): CloudflareConfig => {
        const local = { accessKeyId: app.name, secretAccessKey: App.resource(app, 'production') };
        return {
            ...loaded,
            worker: {
                ...stated,
                name: App.resource(app, 'production'),
                compatibilityFlags: Array.dedupe([...(stated.compatibilityFlags ?? []), 'enable_request_signal']),
                env: {
                    [App.bundle.fetcher]: bindings.assets(),
                    [App.bundle.mode]: bindings.text('production' satisfies keyof typeof App.modes),
                    ...Record.filterMap(app.env, (binding, variable) =>
                        App.Binding.$match(binding, {
                            sqlite: () => Result.succeed(bindings.d1({ name: App.resource(app, 'production', variable) })),
                            postgres: () => Result.failVoid,
                            bucket: () => Result.succeed(bindings.r2({ name: App.resource(app, 'production', variable), dev: { experimentalS3Credentials: local } })),
                            secret: () => Result.succeed(bindings.secret()),
                            text: ({ value }) => Result.succeed(bindings.text(value)),
                            json: ({ value }) => Result.succeed(bindings.json(value)),
                        }),
                    ),
                    ...(Record.some(app.env, App.Binding.$is('bucket')) && {
                        [App.bundle.signing.endpoint]: bindings.text(CorePaths.R2_S3),
                        [App.bundle.signing.region]: bindings.text('auto'),
                        [App.bundle.signing.key]: bindings.text(local.accessKeyId),
                        [App.bundle.signing.secret]: bindings.text(local.secretAccessKey),
                    }),
                },
            },
        };
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export { nodeBundle, worker };
