import { Effect, Function, Match, Schema } from 'effect';
import { HttpServerRequest } from 'effect/http';
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

const worker = <Env extends object>(handler: (env: Env) => (request: Request) => Promise<Response>) => {
    const memoized = Function.memoize(handler);
    const keys = Function.memoize(({ team }: Access) => createRemoteJWKSet(new URL('/cdn-cgi/access/certs', team)));
    const hidden = new Set<string>(Object.values(contract.headers));
    const admit = (request: Request, access: Access, serve: (request: Request) => Promise<Response>) => {
        const url = new URL(request.url);
        const kept = [...request.headers].filter(([name]) => !hidden.has(name));
        return Match.value(url.pathname).pipe(
            Match.when(contract.paths.signin, () => Effect.succeed(Response.redirect(`${url.origin}${url.searchParams.get(contract.returnTo) ?? '/'}`))),
            Match.when(contract.paths.signout, () => Effect.succeed(Response.redirect(`${url.origin}/cdn-cgi/access/logout`))),
            Match.orElse(() =>
                Effect.fromNullishOr(HttpServerRequest.fromWeb(request).cookies.CF_Authorization).pipe(
                    Effect.flatMap((token) => Effect.tryPromise(() => jwtVerify(token, keys(access), { issuer: access.team, audience: access.aud }))),
                    Effect.flatMap(({ payload }) => Schema.decodeUnknownEffect(Identity)(payload)),
                    Effect.match({
                        onFailure: () => kept,
                        onSuccess: ({ sub, email }): typeof kept => [...kept, [contract.headers.id, sub], [contract.headers.email, email]],
                    }),
                    Effect.flatMap((headers) => Effect.promise(() => serve(new Request(request, { headers })))),
                ),
            ),
        );
    };
    return {
        fetch: (request: Request, env: Env & { readonly ACCESS?: Access }): Promise<Response> =>
            (env.ACCESS === undefined ? Effect.promise(() => memoized(env)(request)) : admit(request, env.ACCESS, memoized(env))).pipe(
                Effect.map((response) => {
                    const length = response.headers.get('Content-Length');
                    return response.body === null || length === null ? response : new Response(response.body.pipeThrough(new FixedLengthStream(Number(length))), response);
                }),
                Effect.runPromise,
            ),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { worker };
