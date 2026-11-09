import { Config, Effect, Layer, Option, Schema } from 'effect';
import { SecureRandom, Signature } from 'effect/crypto';
import { FetchHttpClient, Headers, HttpRouter, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { Jwks, Jwt } from 'effect/jose';
import type { accessBinding } from '#/cloudflare';
import { HostingError } from '#/error';
import { User } from '#/user';

// --- [MODELS] --------------------------------------------------------------------------

const Claims = User.pipe(Schema.encodeKeys({ id: 'sub' }));
const Returning = Schema.Struct({ target: Schema.String });

class CloudflareAccess extends Schema.Class<CloudflareAccess, { readonly brand: unique symbol }>('CloudflareAccess')({ issuer: Schema.String, audience: Schema.String, jwks: Schema.String, logout: Schema.String }) {
    static readonly binding = 'ACCESS' satisfies typeof accessBinding;
    static readonly #config = Config.schema(Schema.fromJsonString(CloudflareAccess), CloudflareAccess.binding);
    static readonly middleware = CloudflareAccess.#config.pipe(Config.option, Effect.map(Option.match({ onNone: () => Layer.empty, onSome: (access) => access.layer() })), Layer.unwrap);
    layer() {
        const { issuer, audience, jwks, logout } = this;
        const returning = Returning.pipe(Schema.encodeKeys({ target: User.sites.returnTo }));
        const signin = Effect.fn('signin')(function* (request: HttpServerRequest.HttpServerRequest) {
            const [url, returned] = yield* Effect.all([Effect.orDie(Effect.fromOption(HttpServerRequest.toURL(request))), Effect.option(HttpServerRequest.schemaSearchParams(returning))]);
            return HttpServerResponse.redirect(
                returned.pipe(
                    Option.flatMap(({ target }) => Option.fromNullOr(URL.parse(target, url.origin))),
                    Option.filter(({ origin }) => origin === url.origin),
                    Option.getOrElse(() => new URL('/', url.origin)),
                ),
            );
        });
        const identify = Effect.fn('identify')(function* <E, R>(app: Effect.Effect<HttpServerResponse.HttpServerResponse, E, R>) {
            const request = yield* HttpServerRequest.HttpServerRequest;
            const anonymous = Headers.removeMany(request.headers, Object.values(User.sites.headers));
            const headers = yield* Effect.fromOption(Headers.get(request.headers, 'cf-access-jwt-assertion')).pipe(
                Effect.flatMap((token) => Jwt.verifyWithKeySet(Claims, token, { issuer, audience, algorithms: ['RS256'] })),
                Effect.flatMap(({ claims }) => Effect.orDie(Schema.encodeEffect(User.headers)(claims))),
                Effect.map((identity) => Headers.setAll(anonymous, identity)),
                Effect.catchReason('JoseError', 'KeySetUnavailable', HostingError.unavailable, () => Effect.succeed(anonymous)),
                Effect.catchTag(['NoSuchElementError', 'SchemaError'], () => Effect.succeed(anonymous)),
            );
            return yield* Effect.provideService(app, HttpServerRequest.HttpServerRequest, request.modify({ headers }));
        });
        const keys = Layer.mergeAll(Jwks.layerRemote({ url: jwks }).pipe(Layer.provide(FetchHttpClient.layer), Layer.provide(Layer.succeed(FetchHttpClient.RequestInit, { redirect: 'manual' }))), Signature.layerWebCrypto.pipe(Layer.provide(SecureRandom.layerWebCrypto)));
        return Layer.mergeAll(
            HttpRouter.add('*', User.sites.paths.signin, signin),
            HttpRouter.add('*', User.sites.paths.signout, HttpServerResponse.redirect(logout)),
            HttpRouter.middleware(
                Effect.map(Layer.build(keys), (services) => (app) => Effect.provideContext(identify(app), services)),
                { global: true },
            ),
        );
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { CloudflareAccess };
