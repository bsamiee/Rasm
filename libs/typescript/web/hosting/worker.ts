import { ConfigProvider, Context, Function, Layer, Logger, Scheduler } from 'effect';
import { HttpRouter, HttpServer } from 'effect/http';

// --- [COMPOSITION] ---------------------------------------------------------------------

const serveWorker = <A, E>(env: object, layer: Layer.Layer<A, E, HttpRouter.HttpRouter | Layer.Success<typeof HttpServer.layerServices> | HttpRouter.Request<'Error', unknown> | HttpRouter.Request<'GlobalError', unknown> | HttpRouter.Request<'Requires', A> | HttpRouter.Request<'GlobalRequires', A>>) => {
    const built = Function.memoize((bindings: object) =>
        HttpRouter.toWebHandler(layer.pipe(Layer.provide([HttpServer.layerServices, ConfigProvider.layer(ConfigProvider.fromUnknown(bindings))]), Layer.provideMerge([Logger.layer([Logger.withLeveledConsole(Logger.formatStructured)]), Layer.succeed(Scheduler.Scheduler, new Scheduler.MixedScheduler('sync'))])), {
            disableLogger: true,
        }),
    );
    return { fetch: (request: Request) => built(env).handler(request, Context.empty()) };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { serveWorker };
