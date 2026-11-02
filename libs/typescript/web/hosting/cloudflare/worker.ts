import { Context, Effect, Function, Layer, Logger, Option, Schema } from 'effect';
import { HttpRouter, HttpServer } from 'effect/http';
import { type Access, accessBinding, authenticate } from '#/authentication/access';

// --- [MODELS] --------------------------------------------------------------------------

const ContentLength = Schema.FiniteFromString.check(Schema.isInt(), Schema.isGreaterThanOrEqualTo(0));

// --- [COMPOSITION] ---------------------------------------------------------------------

const worker = <Env extends object, A, E>(
    routes: (env: Env) => Layer.Layer<A, E, HttpRouter.HttpRouter | Layer.Success<typeof HttpServer.layerServices> | HttpRouter.Request<'Error', unknown> | HttpRouter.Request<'GlobalError', unknown> | HttpRouter.Request<'Requires', A> | HttpRouter.Request<'GlobalRequires', A>>,
) => {
    const serve = Function.memoize((env: Env) => HttpRouter.toWebHandler(Layer.provideMerge(routes(env), Layer.merge(HttpServer.layerServices, Logger.layer([Logger.withLeveledConsole(Logger.formatStructured)]))), { disableLogger: true }).handler);
    return {
        fetch: Effect.fn('fetch')(function* (request: Request, env: Env & Partial<Record<typeof accessBinding, Access>>) {
            const { [accessBinding]: access } = env;
            const forward = (inbound: Request) => Effect.promise(() => serve(env)(inbound, Context.empty()));
            const response = yield* access === undefined ? forward(request) : authenticate(request, access, forward);
            return Option.match(Option.all({ body: Option.fromNullOr(response.body), length: Schema.decodeUnknownOption(ContentLength)(response.headers.get('Content-Length')) }), {
                onNone: () => response,
                onSome: ({ body, length }) => new Response(body.pipeThrough(new FixedLengthStream(length)), response),
            });
        }, Effect.runPromise),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { worker };
