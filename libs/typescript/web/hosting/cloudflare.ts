import type { AssetsBinding } from 'cf/config';
import { Array, ConfigProvider, Context, Layer, Logger, Option, Record, Scheduler } from 'effect';
import { HttpRouter, HttpServer } from 'effect/http';
import { Template } from '#/server/template';

// --- [TYPES] ---------------------------------------------------------------------------

type AssetsKey<B> = { [K in keyof B & string]: B[K] extends AssetsBinding ? K : never }[keyof B & string];
type Rendering<B> = [AssetsKey<B>] extends [never] ? never : Template;

// --- [CONSTANTS] -----------------------------------------------------------------------

const compatibilityDate = '2026-10-01';

// --- [COMPOSITION] ---------------------------------------------------------------------

const serveWorker = <B extends Record.ReadonlyRecord<string, { readonly type: string }>, A, E>(
    env: Record.ReadonlyRecord<AssetsKey<B>, Parameters<typeof Template.layer>[0]>,
    config: { readonly env: B },
    layer: Layer.Layer<A, E, HttpRouter.HttpRouter | Layer.Success<typeof HttpServer.layerServices> | HttpRouter.Request<'Error', unknown> | HttpRouter.Request<'GlobalError', unknown> | HttpRouter.Request<'Requires', A | Rendering<B>> | HttpRouter.Request<'GlobalRequires', A | Rendering<B>>>,
) => {
    const { handler } = HttpRouter.toWebHandler(
        Layer.mergeAll(
            layer,
            Option.match(
                Array.findFirst(Record.keys(config.env), (key): key is AssetsKey<B> => config.env[key]?.type === ('assets' satisfies AssetsBinding['type'])),
                { onNone: () => Layer.empty, onSome: (binding) => Template.layer(env[binding]) },
            ),
        ).pipe(Layer.provide([HttpServer.layerServices, ConfigProvider.layer(ConfigProvider.fromUnknown(env))]), Layer.provideMerge([Logger.layer([Logger.withLeveledConsole(Logger.formatStructured)]), Layer.succeed(Scheduler.Scheduler, new Scheduler.MixedScheduler('sync'))])),
        { disableLogger: true },
    );
    return { fetch: (request: Request) => handler(request, Context.empty()) };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { compatibilityDate, serveWorker };
