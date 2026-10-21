import { createServer } from 'node:http';
import { join } from 'node:path';
import { NodeHttpServer } from '@effect/platform-node';
import { Config, Effect, Layer, Logger, Record } from 'effect';
import { FetchHttpClient, HttpEffect, HttpRouter, HttpStaticServer } from 'effect/http';
import { Otlp, OtlpSerialization } from 'effect/observability';
import { Assets } from './render.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const serve = <K extends string, A, E, R, S, SE, SR>(
    worker: { readonly name: string; readonly env: Record.ReadonlyRecord<K, { readonly type: string }> },
    app: (env: Config.Success<ReturnType<typeof Config.all<Record<K, Config.Config<string>>>>>) => { readonly routes: Layer.Layer<A, E, R>; readonly services: Layer.Layer<S, SE, SR> },
) =>
    Layer.unwrap(
        Effect.gen(function* () {
            const root = join(import.meta.dirname, 'assets');
            const [env, files] = yield* Effect.all([Config.all(Record.map(worker.env, ({ type }, name) => (type === 'assets' ? Config.succeed(root) : Config.String(name)))), HttpStaticServer.make({ root })]);
            const { routes, services } = app(env);
            return HttpRouter.serve(Layer.merge(routes, HttpRouter.add('GET', '/*', files))).pipe(Layer.provide([services, Layer.succeed(Assets, { fetch: HttpEffect.toWebHandler(files) })]));
        }),
    ).pipe(Layer.provide([NodeHttpServer.layerConfig(createServer, { port: Config.Port('PORT') }), Otlp.layerFromConfig({ resource: { serviceName: worker.name } }).pipe(Layer.provide([FetchHttpClient.layer, OtlpSerialization.layerProtobuf]), Layer.provideMerge(Logger.layer([Logger.consoleJson])))]));

// --- [EXPORTS] -------------------------------------------------------------------------

export { serve };
