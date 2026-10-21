import { Context, Effect, Function, Layer, Logger, Match, Option, Schema } from 'effect';
import { Headers, HttpRouter, HttpServerRequest } from 'effect/http';
import { createRemoteJWKSet, jwtVerify } from 'jose';
import { contract } from './sites.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Access {
    readonly team: string;
    readonly aud: string;
}

// --- [MODELS] --------------------------------------------------------------------------

const Identity = Schema.Struct({ sub: Schema.String, email: Schema.String });

// --- [COMPOSITION] ---------------------------------------------------------------------

const worker = <Env extends object, A, E>(routes: (env: Env) => Layer.Layer<A, E, HttpRouter.HttpRouter | HttpRouter.Request<'Error', unknown> | HttpRouter.Request<'GlobalError', unknown> | HttpRouter.Request<'Requires', A> | HttpRouter.Request<'GlobalRequires', A>>) => {
    const serve = Function.memoize((env: Env) => HttpRouter.toWebHandler(Layer.provideMerge(routes(env), Logger.layer([Logger.withLeveledConsole(Logger.formatStructured)])), { disableLogger: true }).handler);
    const keys = Function.memoize(({ team }: Access) => createRemoteJWKSet(new URL('/cdn-cgi/access/certs', team)));
    const admit = Effect.fn('admit')(function* (request: Request, access: Access) {
        const identity = yield* Effect.fromNullishOr(HttpServerRequest.fromWeb(request).cookies.CF_Authorization).pipe(
            Effect.flatMap((token) => Effect.tryPromise(() => jwtVerify(token, keys(access), { issuer: access.team, audience: access.aud }))),
            Effect.flatMap(({ payload }) => Schema.decodeUnknownEffect(Identity)(payload)),
            Effect.option,
        );
        const headers = Headers.removeMany(Headers.fromInput(request.headers), Object.values(contract.headers));
        return new Request(request, { headers: Option.match(identity, { onNone: () => headers, onSome: ({ sub, email }) => Headers.setAll(headers, { [contract.headers.id]: sub, [contract.headers.email]: email }) }) });
    });
    return {
        fetch: Effect.fn('fetch')(function* (request: Request, env: Env & { readonly ACCESS?: Access }) {
            const { ACCESS: access } = env;
            const url = new URL(request.url);
            const target = new URL(url.searchParams.get(contract.returnTo) ?? '/', url.origin);
            const forward = (admitted: Request) => Effect.promise(() => serve(env)(admitted, Context.empty()));
            const response = yield* access === undefined
                ? forward(request)
                : Match.value(url.pathname).pipe(
                      Match.when(contract.paths.signin, () => Effect.succeed(Response.redirect(`${url.origin}${target.pathname}${target.search}${target.hash}`))),
                      Match.when(contract.paths.signout, () => Effect.succeed(Response.redirect(`${url.origin}/cdn-cgi/access/logout`))),
                      Match.orElse(() => Effect.flatMap(admit(request, access), forward)),
                  );
            const length = response.headers.get('Content-Length');
            return response.body === null || length === null ? response : new Response(response.body.pipeThrough(new FixedLengthStream(Number(length))), response);
        }, Effect.runPromise),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { worker };
