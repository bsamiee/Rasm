import { Config, Effect, Layer, Option, Schema } from 'effect';
import { SecureRandom, Signature } from 'effect/crypto';
import { FetchHttpClient, Headers, HttpRouter, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { Jwks, Jwt } from 'effect/jose';
import { unavailable } from '#/unavailable';
import { User } from '#/user';

// --- [MODELS] --------------------------------------------------------------------------

const Claims = User.pipe(Schema.encodeKeys({ id: 'sub' }));

class Access extends Schema.Class<Access, { readonly brand: unique symbol }>('Access')({ issuer: Schema.String, audience: Schema.String, jwks: Schema.String, logout: Schema.String }) {
    static readonly binding = 'ACCESS';
    static readonly middleware = (sites: Parameters<typeof User.href>[0]) => Config.schema(Access, Access.binding).pipe(Config.option, Effect.map(Option.match({ onNone: () => Layer.empty, onSome: (access) => access.layer(sites) })), Layer.unwrap);
    layer(sites: Parameters<typeof User.href>[0]) {
        const { issuer, audience, jwks, logout } = this;
        const signin = Effect.fn('Access.signin')(function* (request: HttpServerRequest.HttpServerRequest) {
            const url = yield* Effect.orDie(Effect.fromOption(HttpServerRequest.toURL(request)));
            return HttpServerResponse.redirect(Option.fromNullOr(url.searchParams.get(sites.returnTo)).pipe(Option.flatMap((target) => Option.fromNullOr(URL.parse(target, url.origin))), Option.filter(({ origin }) => origin === url.origin), Option.getOrElse(() => new URL('/', url.origin))));
        });
        const identify = Effect.fn('Access.identify')(function* <E, R>(app: Effect.Effect<HttpServerResponse.HttpServerResponse, E, R>) {
            const request = yield* HttpServerRequest.HttpServerRequest;
            const anonymous = Headers.removeMany(request.headers, Object.values(sites.headers));
            const headers = yield* Effect.fromNullishOr(request.cookies.CF_Authorization).pipe(
                Effect.flatMap((token) => Jwt.verifyWithKeySet(Claims, token, { issuer, audience, algorithms: ['RS256'] })),
                Effect.flatMap(({ claims }) => Effect.orDie(Schema.encodeEffect(User.headers(sites))(claims))),
                Effect.map((identity) => Headers.setAll(anonymous, identity)),
                Effect.catchReason('JoseError', 'KeySetUnavailable', unavailable, () => Effect.succeed(anonymous)),
                Effect.catchTag(['NoSuchElementError', 'SchemaError'], () => Effect.succeed(anonymous)),
            );
            return yield* Effect.provideService(app, HttpServerRequest.HttpServerRequest, request.modify({ headers }));
        });
        const keys = Layer.mergeAll(Jwks.layerRemote({ url: jwks }).pipe(Layer.provide(FetchHttpClient.layer), Layer.provide(Layer.succeed(FetchHttpClient.RequestInit, { redirect: 'manual' }))), Signature.layerWebCrypto.pipe(Layer.provide(SecureRandom.layerWebCrypto)));
        return Layer.mergeAll(
            HttpRouter.add('*', sites.paths.signin, signin),
            HttpRouter.add('*', sites.paths.signout, HttpServerResponse.redirect(logout)),
            HttpRouter.middleware(Effect.map(Layer.build(keys), (services) => (app) => Effect.provideContext(identify(app), services)), { global: true }),
        );
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Access };
