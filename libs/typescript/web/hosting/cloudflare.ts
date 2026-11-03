import { env } from 'cloudflare:workers';
import { ConfigProvider, Context, Layer, Logger, Option, Record, Scheduler } from 'effect';
import { HttpRouter, HttpServer } from 'effect/http';
import { Page } from '#/page/render';

// --- [COMPOSITION] ---------------------------------------------------------------------

const worker = <A, E>(
    config: { readonly env: Record.ReadonlyRecord<string, { readonly type: string }> },
    app: (env: Cloudflare.Env) => Layer.Layer<A, E, HttpRouter.HttpRouter | Layer.Success<typeof HttpServer.layerServices> | HttpRouter.Request<'Error', unknown> | HttpRouter.Request<'GlobalError', unknown> | HttpRouter.Request<'Requires', A | Page> | HttpRouter.Request<'GlobalRequires', A | Page>>,
) => {
    const { handler } = HttpRouter.toWebHandler(
        Layer.mergeAll(
            app(env),
            Option.match(
                Record.findFirst(config.env, ({ type }) => type === 'assets'),
                { onNone: () => Layer.empty, onSome: ([binding]) => Page.layer(Reflect.get(env, binding)) },
            ),
        ).pipe(Layer.provide([HttpServer.layerServices, ConfigProvider.layer(ConfigProvider.fromUnknown(env))]), Layer.provideMerge([Logger.layer([Logger.withLeveledConsole(Logger.formatStructured)]), Layer.succeed(Scheduler.Scheduler, new Scheduler.MixedScheduler('sync'))])),
        { disableLogger: true },
    );
    return { fetch: (request: Request) => handler(request, Context.empty()) };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { worker };
