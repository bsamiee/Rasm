import { Context, Effect, Layer, type Scope } from 'effect';
import { HttpEffect, HttpRouter, type HttpServerRequest } from 'effect/http';
import { Execution, type Kind } from '#/runtime/execution';

// --- [SERVICES] ------------------------------------------------------------------------

class Served extends Context.Service<Served, (request: Request, invocation: Context.Context<Execution>) => Promise<Response>>()('@rasm/workers/runtime/fetch/Served') {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const fetch = <A, E>(routes: Layer.Layer<A, E, HttpRouter.HttpRouter | HttpRouter.Request<'Requires', Execution> | HttpRouter.Request<'GlobalRequires', Execution> | HttpRouter.Request<'Error', unknown> | HttpRouter.Request<'GlobalError', unknown>>): Kind<never, Served, HttpRouter.Request.Without<E>> => ({
    layer: Layer.effect(Served, Effect.map(HttpRouter.toHttpEffect(routes), HttpEffect.toWebHandlerWith<never, HttpServerRequest.HttpServerRequest | Scope.Scope | Execution>(Context.empty()))),
    handlers: (ready) => ({
        fetch: (request, _, execution) => ready().then((isolate) => Execution.run(isolate, execution)(Effect.flatMap(Effect.context<Execution>(), (invocation) => Effect.promise(() => Context.get(isolate, Served)(request, invocation))))),
    }),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { fetch };
