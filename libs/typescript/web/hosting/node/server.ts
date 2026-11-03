import { createServer } from 'node:http';
import { join } from 'node:path';
import { NodeHttpServer } from '@effect/platform-node';
import { Config, ConfigProvider, Effect, Function, Layer, Logger, Match, Record, Schema } from 'effect';
import { FetchHttpClient, HttpEffect, HttpRouter, HttpServerResponse, HttpStaticServer } from 'effect/http';
import { Otlp, OtlpSerialization } from 'effect/observability';
import { vite } from '#/node/vite';
import { Page } from '#/page/render';

// --- [COMPOSITION] ---------------------------------------------------------------------

const server = <K extends string, A, E, R>(config: { readonly name: string; readonly env: Record.ReadonlyRecord<K, { readonly type: string }> }, app: (env: Record.ReadonlyRecord<K, string>) => Layer.Layer<A, E, R>) => {
    const root = join(import.meta.dirname, vite.layout.assets);
    return Layer.unwrap(
        Effect.gen(function* () {
            const [env, files] = yield* Effect.all([
                Config.schema(Schema.Record(Schema.Literals(Record.keys(config.env)), Schema.String)).pipe(
                    Effect.provide(ConfigProvider.layerAdd(ConfigProvider.fromUnknown(Record.filterMap(config.env, Match.type<{ readonly type: string }>().pipe(Match.when({ type: 'assets' }, Function.constant(root)), Match.result))), { asPrimary: true })),
                ),
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
            ).pipe(Layer.provide(Layer.succeed(Page, { fetch: HttpEffect.toWebHandler(files) })));
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
