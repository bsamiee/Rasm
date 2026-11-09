import { createServer } from 'node:http';
import { join } from 'node:path';
import { layerConfig } from '@effect/platform-node/NodeHttpServer';
import { Config, ConfigProvider, Effect, Layer, Match, Predicate, Record, Result, Schema, Struct } from 'effect';
import { HttpEffect, HttpMiddleware, HttpRouter, HttpServerResponse, HttpStaticServer } from 'effect/http';
import type { App, Binding } from '#/app';
import { nodeBundle } from '#/node-bundle';
import { Template } from '#/template';

// --- [TYPES] ---------------------------------------------------------------------------

interface Read extends Struct.Lambda {
    (binding: Binding): (typeof read)[Binding['_tag']];
    readonly '~lambda.out': this['~lambda.in'] extends { readonly _tag: infer T extends Binding['_tag'] } ? (typeof read)[T] : never;
}

// --- [MODELS] --------------------------------------------------------------------------

const read = { sqlite: Schema.String, postgres: Schema.Redacted(Schema.String), bucket: Schema.String, secret: Schema.Redacted(Schema.String), text: Schema.String, json: Schema.Json } as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

const serveNode = Effect.fnUntraced(
    function* <const E extends Record.ReadonlyRecord<string, Binding>, A, Err, R>(app: App<E>, routes: (env: Schema.Struct<{ readonly [K in keyof E]: Struct.Apply<Read, E[K]> }>['Type']) => Layer.Layer<A, Err, R>) {
        const root = join(import.meta.dirname, nodeBundle.assets);
        const settings = ConfigProvider.fromUnknown(Record.filterMap(app.env, (binding) => (Predicate.hasProperty(binding, 'value') ? Result.succeed(binding.value) : Result.failVoid)));
        const [env, files] = yield* Effect.all([
            Config.schema(
                Schema.Struct(
                    Struct.map(
                        app.env,
                        Struct.lambda<Read>(({ _tag }) => read[_tag]),
                    ),
                ),
            ).pipe(Effect.provide(ConfigProvider.layerAdd(settings, { asPrimary: true }))),
            HttpStaticServer.make({ root }),
        ]);
        return HttpRouter.serve(
            Layer.mergeAll(
                routes(env),
                HttpRouter.add('GET', nodeBundle.readiness, HttpMiddleware.withLoggerDisabled(Effect.succeed(HttpServerResponse.empty()))),
                HttpRouter.add('GET', '/*', files),
                HttpRouter.middleware(
                    Effect.map((response: HttpServerResponse.HttpServerResponse) =>
                        Match.value(response.body).pipe(
                            Match.when({ _tag: 'Raw', body: Match.instanceOf(Response) }, () => HttpServerResponse.fromWeb(HttpServerResponse.toWeb(response))),
                            Match.orElse(() => response),
                        ),
                    ),
                    { global: true },
                ),
            ),
        ).pipe(Layer.provide([Template.layer({ fetch: HttpEffect.toWebHandler(files) }, true), HttpMiddleware.layerTracerDisabledForUrls([nodeBundle.readiness])]));
    },
    Layer.unwrap,
    Layer.provide(layerConfig(createServer, { port: Config.Port(nodeBundle.portVariable) })),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { serveNode };
