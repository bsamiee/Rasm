import { Effect, Function, Match, Option, Schema } from 'effect';
import { Headers, HttpServerRequest } from 'effect/http';
import { createRemoteJWKSet, jwtVerify } from 'jose';
import { contract } from './sites.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Access {
    readonly team: string;
    readonly aud: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const accessBinding = 'ACCESS' as const;

// --- [MODELS] --------------------------------------------------------------------------

const Identity = Schema.Struct({ sub: Schema.String, email: Schema.String });

// --- [OPERATIONS] ----------------------------------------------------------------------

const keys = Function.memoize(({ team }: Access) => createRemoteJWKSet(new URL('/cdn-cgi/access/certs', team)));
const authenticate = Effect.fn('authenticate')(function* <E, R>(request: Request, access: Access, forward: (request: Request) => Effect.Effect<Response, E, R>) {
    const url = new URL(request.url);
    const target = new URL(url.searchParams.get(contract.returnTo) ?? '/', url.origin);
    const headers = Headers.removeMany(Headers.fromInput(request.headers), Object.values(contract.headers));
    const admitted = Effect.fromNullishOr(HttpServerRequest.fromWeb(request).cookies.CF_Authorization).pipe(
        Effect.flatMap((token) => Effect.tryPromise(() => jwtVerify(token, keys(access), { issuer: access.team, audience: access.aud }))),
        Effect.flatMap(({ payload }) => Schema.decodeUnknownEffect(Identity)(payload)),
        Effect.option,
        Effect.map((identity) => new Request(request, { headers: Option.match(identity, { onNone: () => headers, onSome: ({ sub, email }) => Headers.setAll(headers, { [contract.headers.id]: sub, [contract.headers.email]: email }) }) })),
    );
    return yield* Match.value(url.pathname).pipe(
        Match.when(contract.paths.signin, () => Effect.succeed(Response.redirect(`${url.origin}${target.pathname}${target.search}${target.hash}`))),
        Match.when(contract.paths.signout, () => Effect.succeed(Response.redirect(`${url.origin}/cdn-cgi/access/logout`))),
        Match.orElse(() => Effect.flatMap(admitted, forward)),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Access, accessBinding, authenticate };
