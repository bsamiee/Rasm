import type { InferEnv } from 'cf/config';
import { ConfigProvider, Context, Function, Layer, Logger, Option, Record, Scheduler } from 'effect';
import { HttpRouter, HttpServer } from 'effect/http';
import { Page } from '#/page/render';

// --- [COMPOSITION] ---------------------------------------------------------------------

const worker = <C extends { readonly env: Record.ReadonlyRecord<string, { readonly type: string }> }, A, E>(
    config: C,
    app: (
        env: InferEnv<C>,
    ) => Layer.Layer<A, E, HttpRouter.HttpRouter | Layer.Success<typeof HttpServer.layerServices> | HttpRouter.Request<'Error', unknown> | HttpRouter.Request<'GlobalError', unknown> | HttpRouter.Request<'Requires', A | Page> | HttpRouter.Request<'GlobalRequires', A | Page>>,
) => {
    const assets = Record.findFirst(config.env, ({ type }) => type === 'assets');
    const handler = Function.memoize(
        (env: InferEnv<C>) =>
            HttpRouter.toWebHandler(
                Layer.mergeAll(app(env), Option.match(assets, { onNone: () => Layer.empty, onSome: ([variable]) => Layer.succeed(Page, Reflect.get(env, variable)) })).pipe(
                    Layer.provide([HttpServer.layerServices, ConfigProvider.layer(ConfigProvider.fromUnknown(env))]),
                    Layer.provideMerge([Logger.layer([Logger.withLeveledConsole(Logger.formatStructured)]), Layer.succeed(Scheduler.Scheduler, new Scheduler.MixedScheduler('sync'))]),
                ),
                { disableLogger: true },
            ).handler,
    );
    return { fetch: (request: Request, env: InferEnv<C>) => handler(env)(request, Context.empty()) };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { worker };
