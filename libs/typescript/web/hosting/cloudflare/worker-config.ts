import process from 'node:process';
import { bindings, type WorkerConfig } from 'cf/config';
import { Array, Cause, Effect, Match, Record, type Schema, Struct } from 'effect';
import { CorePaths } from 'miniflare';
import { defaultClientMainFields, type Plugin } from 'vite';
import { App, type Binding } from '#/app/app';

// --- [TYPES] ---------------------------------------------------------------------------

interface Projection extends Struct.Lambda {
    (binding: Binding): ReturnType<ReturnType<typeof projection>[Binding['_tag']]>;
    readonly '~lambda.in': Binding;
    readonly '~lambda.out': ReturnType<ReturnType<typeof projection>[this['~lambda.in']['_tag']]>;
}
interface Served extends Struct.Lambda {
    (binding: Binding): (name: string) => Pick<Binding, '_tag'>;
    readonly '~lambda.in': Binding;
    readonly '~lambda.out': (name: string) => this['~lambda.in'] extends { readonly _tag: 'bucket' } ? Pick<this['~lambda.in'], '_tag'> & { readonly name: string } : Pick<this['~lambda.in'], '_tag'>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const local = (app: App) => ({ accessKeyId: app.name, secretAccessKey: App.resource(app, 'production') });
const projection = (app: App) =>
    ({
        sqlite: () => (name: string) => bindings.d1({ name }),
        postgres: () => () => undefined,
        bucket: () => (name: string) => bindings.r2({ name, dev: { experimentalS3Credentials: local(app) } }),
        secret: () => () => bindings.secret(),
        text: (text: { readonly value: string }) => () => bindings.text(text.value),
        json: (json: { readonly value: Schema.MutableJson }) => () => bindings.json(json.value),
    }) as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

const worker = Object.assign(
    <const E extends Record.ReadonlyRecord<string, Binding>, const C extends Omit<WorkerConfig, 'name'>>(app: App<E>, config: C) =>
        ({
            ...config,
            name: App.resource(app, 'production'),
            compatibilityFlags: Array.dedupe([...(config.compatibilityFlags ?? []), 'enable_request_signal']),
            env: {
                ...Struct.get(config, 'env'),
                [App.bundle.fetcher]: bindings.assets(),
                OTEL_SERVICE_NAME: bindings.text(app.name),
                [App.bundle.hosting]: bindings.json({
                    assets: App.bundle.fetcher,
                    allowed: [...app.allowed],
                    env: Struct.evolve(
                        Record.map<keyof E & string, Binding, string>(app.env, (_, variable) => App.resource(app, 'production', variable)),
                        Struct.map(
                            app.env,
                            Struct.lambda<Served>(
                                Match.type<Binding>().pipe(
                                    Match.tag('bucket', ({ _tag }) => (name: string) => ({ _tag, name })),
                                    Match.orElse(({ _tag }) => () => ({ _tag })),
                                ),
                            ),
                        ),
                    ),
                }),
                ...Struct.evolve(
                    Record.map<keyof E & string, Binding, string>(app.env, (_, variable) => App.resource(app, 'production', variable)),
                    Struct.map(app.env, Struct.lambda<Projection>(App.Binding.$match(projection(app)))),
                ),
                ...(Record.some(app.env, App.Binding.$is('bucket')) && Record.fromIterableWith(Object.values(App.bundle.signing), (variable) => [variable, bindings.secret()])),
            },
        }) satisfies WorkerConfig,
    {
        plugin: (app: App): Plugin[] => [
            {
                name: '@rasm/hosting/cloudflare/local',
                apply: 'serve',
                config: (config, { isPreview = false }) =>
                    Effect.fromNullishOr(config[isPreview ? 'preview' : 'server']?.port).pipe(
                        Effect.mapError(() => new Cause.NoSuchElementError(`Vite config of ${app.name} states no ${isPreview ? 'preview' : 'server'}.port, the origin its local S3 endpoint signs for`)),
                        Effect.tap((port) =>
                            Effect.sync(() =>
                                Object.assign(process.env, {
                                    [App.bundle.signing.endpoint]: new URL(CorePaths.R2_S3, `http://localhost:${port}`).href,
                                    [App.bundle.signing.region]: 'auto',
                                    [App.bundle.signing.key]: local(app).accessKeyId,
                                    [App.bundle.signing.secret]: local(app).secretAccessKey,
                                }),
                            ),
                        ),
                        Effect.as({ server: { strictPort: true }, preview: { strictPort: true } }),
                        Effect.runSync,
                    ),
            },
            {
                name: '@rasm/hosting/cloudflare/resolve',
                enforce: 'post',
                configEnvironment: (_, { resolve }) => (resolve?.conditions?.includes('browser') === true ? { resolve: { mainFields: [...defaultClientMainFields] }, optimizeDeps: { rolldownOptions: { resolve: { mainFields: [...defaultClientMainFields], aliasFields: [['browser']] } } } } : null),
            },
        ],
    },
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { worker };
