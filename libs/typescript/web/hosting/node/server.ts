import { createServer } from 'node:http';
import { join } from 'node:path';
import { NodeHttpServer } from '@effect/platform-node';
import { Config, ConfigProvider, Effect, Layer, Logger, Record, Result, Schema, Struct } from 'effect';
import { FetchHttpClient, HttpEffect, HttpRouter, HttpServerResponse, HttpStaticServer } from 'effect/http';
import { Otlp, OtlpSerialization } from 'effect/observability';
import { vite } from '#/node/vite';
import { Page } from '#/page/render';

// --- [TYPES] ---------------------------------------------------------------------------

interface Field extends Struct.Lambda {
    (binding: { readonly type: string }): typeof Json | typeof Schema.String;
    readonly '~lambda.out': this['~lambda.in'] extends { readonly type: 'json' } ? typeof Json : typeof Schema.String;
}

// --- [MODELS] --------------------------------------------------------------------------

const Json = Schema.fromJsonString(Schema.Unknown);

// --- [COMPOSITION] ---------------------------------------------------------------------

const server = <B extends Record.ReadonlyRecord<string, { readonly type: string }>, A, E, R>(config: { readonly name: string; readonly env: B }, app: (env: Schema.Struct<{ [K in keyof B]: Struct.Apply<Field, B[K]> }>['Type']) => Layer.Layer<A, E, R>) => {
    const root = join(import.meta.dirname, vite.layout.assets);
    return Layer.unwrap(
        Effect.gen(function* () {
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
            ).pipe(Layer.provide(Page.layer({ fetch: HttpEffect.toWebHandler(files) })));
        }),
    ).pipe(
        Layer.provide([
            NodeHttpServer.layerConfig(createServer, { port: Config.Port(vite.layout.portVariable) }),
            Otlp.layerFromConfig({ resource: { serviceName: config.name } }).pipe(Layer.provide([FetchHttpClient.layer, OtlpSerialization.layerProtobuf]), Layer.provideMerge(Logger.layer([Logger.consoleJson, Logger.tracerLogger]))),
        ]),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { server };
