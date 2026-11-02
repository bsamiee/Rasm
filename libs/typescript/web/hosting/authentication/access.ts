import { Effect, Function, Match, Option, Record, Schema } from 'effect';
import { Headers, HttpServerRequest } from 'effect/http';
import { createRemoteJWKSet, jwtVerify } from 'jose';
import { contract } from '#/authentication/sites';

// --- [TYPES] ---------------------------------------------------------------------------

interface Access {
    readonly team: string;
    readonly aud: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const accessBinding = 'ACCESS' as const;
const claims = { [contract.headers.id]: 'sub', [contract.headers.email]: 'email' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const ReturnTo = Schema.fromURLSearchParams(Schema.Struct({ [contract.returnTo]: Schema.String }));
const Identity = Schema.Struct(Record.map(claims, () => Schema.String)).pipe(Schema.encodeKeys(claims));

// --- [OPERATIONS] ----------------------------------------------------------------------

const keys = Function.memoize(({ team }: Access) => createRemoteJWKSet(new URL('/cdn-cgi/access/certs', team)));
const authenticate = Effect.fn('authenticate')(function* <E, R>(request: Request, access: Access, forward: (request: Request) => Effect.Effect<Response, E, R>) {
    const url = new URL(request.url);
    const headers = Headers.removeMany(Headers.fromInput(request.headers), Object.values(contract.headers));
    const identified = Effect.fromNullishOr(HttpServerRequest.fromWeb(request).cookies.CF_Authorization).pipe(
        Effect.flatMap((token) => Effect.tryPromise(() => jwtVerify(token, keys(access), { issuer: access.team, audience: access.aud }))),
        Effect.flatMap(({ payload }) => Schema.decodeUnknownEffect(Identity)(payload)),
        Effect.option,
        Effect.map((identity) => new Request(request, { headers: Option.match(identity, { onNone: () => headers, onSome: (fields) => Headers.setAll(headers, fields) }) })),
    );
    return yield* Match.value(url.pathname).pipe(
        Match.when(contract.paths.signin, () =>
            Schema.decodeOption(ReturnTo)(url.searchParams).pipe(
                Option.flatMap(({ [contract.returnTo]: returnTo }) => Option.fromNullOr(URL.parse(returnTo, url.origin))),
                Option.match({ onNone: () => '/', onSome: ({ pathname, search, hash }) => `${pathname}${search}${hash}` }),
                (target) => Effect.succeed(Response.redirect(`${url.origin}${target}`)),
            ),
        ),
        Match.when(contract.paths.signout, () => Effect.succeed(Response.redirect(`${url.origin}/cdn-cgi/access/logout`))),
        Match.orElse(() => Effect.flatMap(identified, forward)),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Access, accessBinding, authenticate };
