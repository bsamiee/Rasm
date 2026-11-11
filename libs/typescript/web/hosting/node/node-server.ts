import { dirname, join } from 'node:path';
import process from 'node:process';
import { Config, ConfigProvider, Effect, Layer, Match, Record, Schema, Struct } from 'effect';
import { HttpEffect, HttpMiddleware, HttpRouter, HttpServerResponse, HttpStaticServer } from 'effect/http';
import { App, type Binding } from '#/app/app';
import { Access, type Identity } from '#/identity/identity';
import { Template } from '#/rendering/template';

// --- [TYPES] ---------------------------------------------------------------------------

interface Read extends Struct.Lambda {
    (binding: Binding): (typeof read)[Binding['_tag']];
    readonly '~lambda.out': this['~lambda.in'] extends { readonly _tag: infer T extends Binding['_tag'] } ? (typeof read)[T] : never;
}

// --- [MODELS] --------------------------------------------------------------------------

const read = { sqlite: Schema.String, postgres: Schema.Redacted(Schema.String), bucket: Schema.String, secret: Schema.Redacted(Schema.String), text: Schema.String, json: Schema.fromJsonString(Schema.Json) } as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

const serveNode = Effect.fnUntraced(function* <const E extends Record.ReadonlyRecord<string, Binding>, A, Err, R>(app: App<E>, identity: Identity, routes: (env: Schema.Struct<{ readonly [K in keyof E]: Struct.Apply<Read, E[K]> }>['Type']) => Layer.Layer<A, Err, R>) {
    const root = join(dirname(yield* Effect.fromNullishOr(process.argv[1])), App.bundle.assets);
    const settings = ConfigProvider.fromUnknown(
        Record.filterMap(
            app.env,
            Match.type<Binding>().pipe(
                Match.tag('text', ({ value }) => value),
                Match.tag('json', ({ value }) => JSON.stringify(value)),
                Match.result,
            ),
        ),
    );
    const [env, access, files] = yield* Effect.all([
        Config.schema(
            Schema.Struct(
                Struct.map(
                    app.env,
                    Struct.lambda<Read>(({ _tag }) => read[_tag]),
                ),
            ),
        ).pipe(Effect.provide(ConfigProvider.layerAdd(settings, { asPrimary: true }))),
        Access.config,
        HttpStaticServer.make({ root }),
    ]);
    return HttpRouter.serve(Layer.mergeAll(routes(env), access.layer(identity), HttpRouter.add('GET', App.bundle.readiness, HttpMiddleware.withLoggerDisabled(Effect.succeed(HttpServerResponse.empty()))), HttpRouter.add('GET', '/*', files))).pipe(
        Layer.provide([Template.layer({ fetch: HttpEffect.toWebHandler(files) }), HttpMiddleware.layerTracerDisabledForUrls([App.bundle.readiness])]),
    );
}, Layer.unwrap);

// --- [EXPORTS] -------------------------------------------------------------------------

export { serveNode };
