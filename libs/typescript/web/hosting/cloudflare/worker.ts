import { Context, Effect, Function, Layer, Logger } from 'effect';
import { HttpRouter } from 'effect/http';
import { type Access, accessBinding, authenticate } from '#/authentication/access';

// --- [COMPOSITION] ---------------------------------------------------------------------

const worker = <Env extends object, A, E>(routes: (env: Env) => Layer.Layer<A, E, HttpRouter.HttpRouter | HttpRouter.Request<'Error', unknown> | HttpRouter.Request<'GlobalError', unknown> | HttpRouter.Request<'Requires', A> | HttpRouter.Request<'GlobalRequires', A>>) => {
    const serve = Function.memoize((env: Env) => HttpRouter.toWebHandler(Layer.provideMerge(routes(env), Logger.layer([Logger.withLeveledConsole(Logger.formatStructured)])), { disableLogger: true }).handler);
    return {
        fetch: Effect.fn('fetch')(function* (request: Request, env: Env & Partial<Record<typeof accessBinding, Access>>) {
            const { [accessBinding]: access } = env;
            const forward = (admitted: Request) => Effect.promise(() => serve(env)(admitted, Context.empty()));
            const response = yield* access === undefined ? forward(request) : authenticate(request, access, forward);
            const length = response.headers.get('Content-Length');
            return response.body === null || length === null ? response : new Response(response.body.pipeThrough(new FixedLengthStream(Number(length))), response);
        }, Effect.runPromise),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { worker };
