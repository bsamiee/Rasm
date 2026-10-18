import { createServer } from 'node:http';
import { join } from 'node:path';
import { NodeHttpServer } from '@effect/platform-node';
import { Config, Effect, Layer, Record } from 'effect';
import { HttpEffect, HttpRouter, HttpStaticServer } from 'effect/http';

// --- [COMPOSITION] ---------------------------------------------------------------------

const serve = <K extends string, A, E, R, S, SE, SR>(
    worker: { readonly env: Record.ReadonlyRecord<K, { readonly type: string }> },
    app: (env: Config.Success<ReturnType<typeof Config.all<Record<K, Config.Config<string>>>>>, assets: { readonly fetch: (request: Request) => Promise<Response> }) => { readonly routes: Layer.Layer<A, E, R>; readonly services: Layer.Layer<S, SE, SR> },
) =>
    Layer.unwrap(
        Effect.gen(function* () {
            const root = join(import.meta.dirname, 'assets');
            const [env, files] = yield* Effect.all([Config.all(Record.map(worker.env, ({ type }, name) => (type === 'assets' ? Config.succeed(root) : Config.String(name)))), HttpStaticServer.make({ root })]);
            const { routes, services } = app(env, { fetch: HttpEffect.toWebHandler(files) });
            return HttpRouter.serve(Layer.merge(routes, HttpStaticServer.layer({ root }))).pipe(Layer.provide(services));
        }),
    ).pipe(Layer.provide(NodeHttpServer.layerConfig(createServer, { port: Config.Port('PORT') })));

// --- [EXPORTS] -------------------------------------------------------------------------

export { serve };
