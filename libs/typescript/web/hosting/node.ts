import { createServer } from 'node:http';
import { join } from 'node:path';
import { layerConfig } from '@effect/platform-node/NodeHttpServer';
import { Config, ConfigProvider, Effect, Layer, Logger, Record, Result, Schema, Struct } from 'effect';
import { FetchHttpClient, HttpEffect, HttpRouter, HttpServerResponse, HttpStaticServer } from 'effect/http';
import { Otlp, OtlpSerialization } from 'effect/observability';
import { Template } from '#/server/template';
import { vite } from '#/vite';

// --- [TYPES] ---------------------------------------------------------------------------

interface Field extends Struct.Lambda {
    (binding: { readonly type: string }): typeof Json | typeof Schema.String;
    readonly '~lambda.out': this['~lambda.in'] extends { readonly type: 'json' } ? typeof Json : typeof Schema.String;
}

// --- [MODELS] --------------------------------------------------------------------------

const Json = Schema.fromJsonString(Schema.Unknown);

// --- [COMPOSITION] ---------------------------------------------------------------------

const serveNode = Effect.fnUntraced(
    function* <B extends Record.ReadonlyRecord<string, { readonly type: string }>, A, E, R>(config: { readonly name: string; readonly env: B }, app: (env: Schema.Struct<{ [K in keyof B]: Struct.Apply<Field, B[K]> }>['Type']) => Layer.Layer<A, E, R>) {
        const root = join(import.meta.dirname, vite.layout.assets);
        const [env, files] = yield* Effect.all([
            Config.schema(
                Schema.Struct(
                    Struct.map(
                        config.env,
                        Struct.lambda<Field>(({ type }) => (type === 'json' ? Json : Schema.String)),
                    ),
                ),
            ).pipe(Effect.provide(ConfigProvider.layerAdd(ConfigProvider.fromUnknown(Record.filterMap(config.env, ({ type }) => (type === 'assets' ? Result.succeed(root) : Result.failVoid))), { asPrimary: true }))),
            HttpStaticServer.make({ root }),
        ]);
        return HttpRouter.serve(
            Layer.mergeAll(
                app(env),
                HttpRouter.add('GET', '/*', files),
                HttpRouter.middleware(
                    Effect.map((response: HttpServerResponse.HttpServerResponse) => (response.body._tag === 'Raw' && response.body.body instanceof Response ? HttpServerResponse.fromWeb(HttpServerResponse.toWeb(response)) : response)),
                    { global: true },
                ),
            ),
        ).pipe(Layer.provide(Template.layer({ fetch: HttpEffect.toWebHandler(files) })));
    },
    Layer.unwrap,
    (layer, config) =>
        layer.pipe(
            Layer.provide([
                layerConfig(createServer, { port: Config.Port(vite.layout.portVariable) }),
                Otlp.layerFromConfig({ resource: { serviceName: config.name } }).pipe(Layer.provide([FetchHttpClient.layer, OtlpSerialization.layerProtobuf]), Layer.provideMerge(Logger.layer([Logger.consoleJson, Logger.tracerLogger]))),
            ]),
        ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { serveNode };
