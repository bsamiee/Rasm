import { ConfigProvider, Context, Effect, Function, Layer, Logger, Scheduler } from 'effect';
import { HttpRouter, HttpServer } from 'effect/http';
import { r2Store, r2UploadStore } from 'partial-content/r2';
import { HostingError } from '#/error';
import { ObjectStorage } from '#/object-storage';

// --- [OPERATIONS] ----------------------------------------------------------------------

const r2Storage = (bucket: R2Bucket) => {
    const uploads = r2UploadStore({ bucket });
    return Layer.succeed(ObjectStorage, { objects: r2Store({ bucket, reservedPrefix: uploads.uploadPrefix }), uploads, delete: (key) => Effect.tryPromise({ try: () => bucket.delete(key), catch: HostingError.storage('delete', key) }) });
};

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

export { r2Storage, serveWorker };
