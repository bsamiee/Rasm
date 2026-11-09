import { Config, ConfigProvider, Context, Effect, Function, Layer, Option, Scheduler, Schema } from 'effect';
import { SecureRandom, Signature } from 'effect/crypto';
import { FetchHttpClient, Headers, HttpRouter, HttpServer, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { Jwks, Jwt } from 'effect/jose';
import { type R2Bucket, type R2UploadStoreOptions, r2Store, r2UploadStore } from 'partial-content/r2';
import { HostingError } from '#/error';
import { type Identity, User } from '#/identity';
import { ObjectStorage } from '#/object-storage';

// --- [MODELS] --------------------------------------------------------------------------

const Claims = User.pipe(Schema.encodeKeys({ id: 'sub' }));

class CloudflareAccess extends Schema.Class<CloudflareAccess, { readonly brand: unique symbol }>('CloudflareAccess')({ issuer: Schema.String, audience: Schema.String, jwks: Schema.String, logout: Schema.String }) {
    static readonly middleware = (identity: Identity) => Config.schema(Schema.fromJsonString(CloudflareAccess), 'ACCESS').pipe(Config.option, Effect.map(Option.match({ onNone: () => Layer.empty, onSome: (access) => access.layer(identity) })), Layer.unwrap);
    layer(identity: Identity) {
        const { issuer, audience, jwks, logout } = this;
        const returning = Schema.Struct({ target: Schema.String }).pipe(Schema.encodeKeys({ target: identity.returnTo }));
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
            const anonymous = Headers.removeMany(request.headers, Object.values(identity.headers));
            const headers = yield* Effect.fromOption(Headers.get(request.headers, 'cf-access-jwt-assertion')).pipe(
                Effect.flatMap((token) => Jwt.verifyWithKeySet(Claims, token, { issuer, audience, algorithms: ['RS256'] })),
                Effect.flatMap(({ claims }) => Effect.orDie(Schema.encodeEffect(identity.user)(claims))),
                Effect.map((carried) => Headers.setAll(anonymous, carried)),
                Effect.catchReason('JoseError', 'KeySetUnavailable', HostingError.unavailable, () => Effect.succeed(anonymous)),
                Effect.catchTag(['NoSuchElementError', 'SchemaError'], () => Effect.succeed(anonymous)),
            );
            return yield* Effect.provideService(app, HttpServerRequest.HttpServerRequest, request.modify({ headers }));
        });
        const keys = Layer.mergeAll(Jwks.layerRemote({ url: jwks }).pipe(Layer.provide(FetchHttpClient.layer), Layer.provide(Layer.succeed(FetchHttpClient.RequestInit, { redirect: 'manual' }))), Signature.layerWebCrypto.pipe(Layer.provide(SecureRandom.layerWebCrypto)));
        return Layer.mergeAll(
            HttpRouter.add('*', identity.paths.signin, signin),
            HttpRouter.add('*', identity.paths.signout, HttpServerResponse.redirect(logout)),
            HttpRouter.middleware(
                Effect.map(Layer.build(keys), (services) => (app) => Effect.provideContext(identify(app), services)),
                { global: true },
            ),
        );
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const r2Storage = (bucket: R2Bucket & R2UploadStoreOptions['bucket']) => {
    const uploads = r2UploadStore({ bucket });
    return Layer.succeed(ObjectStorage, { objects: r2Store({ bucket, reservedPrefix: uploads.uploadPrefix }), uploads, delete: (key) => Effect.tryPromise({ try: () => bucket.delete(key), catch: HostingError.storage('delete', key) }) });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const serveWorker = <A, E>(env: object, layer: Layer.Layer<A, E, HttpRouter.HttpRouter | Layer.Success<typeof HttpServer.layerServices> | HttpRouter.Request<'Error', unknown> | HttpRouter.Request<'GlobalError', unknown> | HttpRouter.Request<'Requires', A> | HttpRouter.Request<'GlobalRequires', A>>) => {
    const built = Function.memoize((bindings: object) => HttpRouter.toWebHandler(layer.pipe(Layer.provide([HttpServer.layerServices, ConfigProvider.layer(ConfigProvider.fromUnknown(bindings))]), Layer.provideMerge(Layer.succeed(Scheduler.Scheduler, new Scheduler.MixedScheduler('sync')))), { disableLogger: true }));
    return { fetch: (request: Request) => built(env).handler(request, Context.empty()) };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { CloudflareAccess, r2Storage, serveWorker };
