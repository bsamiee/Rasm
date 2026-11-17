import { createServer } from 'node:http';
import { dirname, join } from 'node:path';
import process from 'node:process';
import { layerConfig } from '@effect/platform-node/NodeHttpServer';
import { layerNodeFromConfig } from '@rasm/telemetry/node';
import { Config, ConfigProvider, Effect, Layer, Match, Record, Schema, Struct } from 'effect';
import { HttpEffect, HttpMiddleware, HttpRouter, HttpServerResponse, HttpStaticServer } from 'effect/http';
import { App, type Binding } from '#/app/app';
import { Access, Enrollment, Identity } from '#/app/identity';
import { Template } from '#/rendering/template';

// --- [TYPES] ---------------------------------------------------------------------------

interface Read extends Struct.Lambda {
    (binding: Binding): ReturnType<(typeof read)[Binding['_tag']]>;
    readonly '~lambda.out': this['~lambda.in'] extends { readonly _tag: infer T extends Binding['_tag'] } ? ReturnType<(typeof read)[T]> : never;
}

// --- [MODELS] --------------------------------------------------------------------------

const JsonText = Schema.fromJsonString(Schema.Json);
const read = {
    sqlite: () => Schema.String,
    postgres: () => Schema.Redacted(Schema.String),
    bucket: () => Schema.String,
    secret: () => Schema.Redacted(Schema.String),
    text: () => Schema.String,
    json: () => JsonText,
} as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

const serveNode = Effect.fnUntraced(
    function* <const E extends Record.ReadonlyRecord<string, Binding>, A, Err, R>(app: App<E>, routes: (env: Schema.Struct<{ readonly [K in keyof E]: Struct.Apply<Read, E[K]> }>['Type']) => Layer.Layer<A, Err, R>) {
        const root = join(dirname(yield* Effect.fromNullishOr(process.argv[1])), App.bundle.assets);
        const declared = ConfigProvider.fromUnknown(
            Record.filterMap(
                app.env,
                Match.type<Binding>().pipe(
                    Match.tag('text', ({ value }) => value),
                    Match.tag('json', ({ value }) => JSON.stringify(value)),
                    Match.result,
                ),
            ),
        );
        const [env, access, files] = yield* Effect.all([Config.schema(Schema.Struct(Struct.map(app.env, Struct.lambda<Read>(App.Binding.$match(read))))).pipe(Effect.provide(ConfigProvider.layerAdd(declared, { asPrimary: true }))), Access.config, HttpStaticServer.make({ root })]);
        const services = Layer.merge(Template.layer({ fetch: HttpEffect.toWebHandler(files) }), Enrollment.layer({ identity: Identity.sites, allowed: app.allowed }));
        return HttpRouter.serve(
            Layer.mergeAll(access.layer(Identity.sites), HttpRouter.add('GET', App.bundle.readiness, HttpMiddleware.withLoggerDisabled(Effect.succeed(HttpServerResponse.empty()))), HttpRouter.add('GET', '/*', files).pipe(Layer.provideMerge(routes(env).pipe(HttpRouter.provideRequest(services))))),
        ).pipe(Layer.provide([services, HttpMiddleware.layerTracerDisabledForUrls([App.bundle.readiness])]));
    },
    (effect, app) => effect.pipe(Layer.unwrap, Layer.provide(layerConfig(createServer, { port: Config.Port(App.bundle.portVariable) })), Layer.provide(layerNodeFromConfig({ resource: { serviceName: app.name }, target: 'stderr' })), Layer.launch),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { serveNode };
