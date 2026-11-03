import { Config, Context, Effect, flow, Layer, Match, Option, Schema } from 'effect';
import { SecureRandom, Signature } from 'effect/crypto';
import { FetchHttpClient, Headers, HttpRouter, HttpServerRequest, HttpServerResponse, UrlParams } from 'effect/http';
import { Jwks, Jwt } from 'effect/jose';
import { unavailable } from '#/unavailable';
import { User } from '#/user';

// --- [TYPES] ---------------------------------------------------------------------------

type Sites = Parameters<typeof User.current>[0];

// --- [MODELS] --------------------------------------------------------------------------

const Claims = User.pipe(Schema.encodeKeys({ id: 'sub' }));

class Access extends Schema.Class<Access, { readonly brand: unique symbol }>('Access')({ issuer: Schema.String, audience: Schema.String, jwks: Schema.String, logout: Schema.String }) {
    static readonly binding = 'ACCESS';
    static readonly middleware = (sites: Sites) => Config.schema(Access, Access.binding).pipe(Config.option, Effect.map(Option.match({ onNone: () => Layer.empty, onSome: (access) => gate(access, sites) })), Layer.unwrap);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const returned = (sites: Sites, url: URL) =>
    UrlParams.getFirst(UrlParams.fromInput(url.searchParams), sites.returnTo).pipe(
        Option.flatMap((target) => Option.fromNullOr(URL.parse(target, url.origin))),
        Option.filter(({ origin }) => origin === url.origin),
        Option.getOrElse(() => new URL('/', url.origin)),
    );
const identify = (access: Access, sites: Sites, request: HttpServerRequest.HttpServerRequest) => {
    const anonymous = Headers.removeMany(request.headers, Object.values(sites.headers));
    return Effect.fromNullishOr(request.cookies.CF_Authorization).pipe(
        Effect.flatMap((token) => Jwt.verifyWithKeySet(Claims, token, { issuer: access.issuer, audience: access.audience, algorithms: ['RS256'] })),
        Effect.flatMap(({ claims }) => Schema.encodeEffect(User.headers(sites))(claims).pipe(Effect.orDie)),
        Effect.map((identity) => Headers.setAll(anonymous, identity)),
        Effect.catchReason('JoseError', 'KeySetUnavailable', unavailable, () => Effect.succeed(anonymous)),
        Effect.catchTag(['NoSuchElementError', 'SchemaError'], () => Effect.succeed(anonymous)),
    );
};
const admit = Effect.fn('admit')(function* <E, R>(access: Access, sites: Sites, app: Effect.Effect<HttpServerResponse.HttpServerResponse, E, R>) {
    const request = yield* HttpServerRequest.HttpServerRequest;
    const url = yield* Effect.fromOption(HttpServerRequest.toURL(request)).pipe(Effect.orDie);
    return yield* Match.value(url.pathname).pipe(
        Match.when(sites.paths.signin, () => Effect.succeed(HttpServerResponse.redirect(returned(sites, url)))),
        Match.when(sites.paths.signout, () => Effect.succeed(HttpServerResponse.redirect(new URL(access.logout, url.origin)))),
        Match.orElse(() => Effect.flatMap(identify(access, sites, request), (headers) => Effect.provideService(app, HttpServerRequest.HttpServerRequest, request.modify({ headers })))),
    );
});
const gate = (access: Access, sites: Sites) =>
    HttpRouter.middleware(
        Effect.map(
            Effect.context<Jwks.Jwks | Signature.Signature>(),
            flow(Context.pick(Jwks.Jwks, Signature.Signature), (services) => (app) => Effect.provideContext(admit(access, sites, app), services)),
        ),
        { global: true },
    ).pipe(Layer.provide([Jwks.layerRemote({ url: access.jwks }).pipe(Layer.provide(FetchHttpClient.layer), Layer.provide(Layer.succeed(FetchHttpClient.RequestInit, { redirect: 'manual' }))), Signature.layerWebCrypto.pipe(Layer.provide(SecureRandom.layerWebCrypto))]));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Access };
