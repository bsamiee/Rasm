import { createServer } from 'node:http';
import { join } from 'node:path';
import { NodeHttpServer } from '@effect/platform-node';
import { Config, ConfigProvider, Context, Effect, Function, Layer, Logger, Match, Record, Schema } from 'effect';
import { FetchHttpClient, HttpEffect, HttpRouter, HttpServer, HttpStaticServer } from 'effect/http';
import { Otlp, OtlpSerialization } from 'effect/observability';
import { layout } from '#/node/vite';
import { Page } from '#/page/render';

// --- [COMPOSITION] ---------------------------------------------------------------------

const server = <K extends string, A, E>(
    worker: { readonly name: string; readonly env: Record.ReadonlyRecord<K, { readonly type: string }> },
    app: (
        env: Schema.Struct.Type<Record<K, typeof Schema.String>>,
    ) => Layer.Layer<A, E, HttpRouter.HttpRouter | Page | Layer.Success<typeof NodeHttpServer.layerHttpServices> | HttpRouter.Request<'Error', unknown> | HttpRouter.Request<'GlobalError', unknown> | HttpRouter.Request<'Requires', A | Page> | HttpRouter.Request<'GlobalRequires', A>>,
) => {
    const root = join(import.meta.dirname, layout.assets);
    const telemetry = Otlp.layerFromConfig({ resource: { serviceName: worker.name } }).pipe(Layer.provide([FetchHttpClient.layer, OtlpSerialization.layerProtobuf]), Layer.provideMerge(Logger.layer([Logger.consoleJson])));
    return Layer.unwrap(
        Effect.gen(function* () {
            const [env, files, memoMap] = yield* Effect.all([
                Config.schema(Schema.Struct(Record.map(worker.env, Function.constant(Schema.String)))).pipe(
                    Effect.provide(ConfigProvider.layerAdd(ConfigProvider.fromUnknown(Record.filterMap(worker.env, Match.type<{ readonly type: string }>().pipe(Match.when({ type: 'assets' }, Function.constant(root)), Match.result))), { asPrimary: true })),
                ),
                HttpStaticServer.make({ root }),
                Effect.map(Effect.context<never>(), Layer.CurrentMemoMap.forkOrCreate),
            ]);
            const { handler } = yield* Effect.acquireRelease(
                Effect.sync(() => HttpRouter.toWebHandler(Layer.mergeAll(app(env), HttpRouter.add('GET', '/*', files)).pipe(Layer.provideMerge([Layer.succeed(Page, { fetch: HttpEffect.toWebHandler(files) }), NodeHttpServer.layerHttpServices, telemetry])), { memoMap })),
                ({ dispose }) => Effect.promise(dispose),
            );
            return HttpServer.serve(HttpEffect.fromWebHandler((request) => handler(request, Context.empty())));
        }),
    ).pipe(HttpServer.withLogAddress, Layer.provide([NodeHttpServer.layerConfig(createServer, { port: Config.Port(layout.port) }), telemetry]));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { server };
