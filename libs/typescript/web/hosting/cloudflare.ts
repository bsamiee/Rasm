import { Config, ConfigProvider, Context, Effect, Function, flow, Layer, Logger, Match, Option, Scheduler, Schema } from 'effect';
import { SecureRandom, Signature } from 'effect/crypto';
import { FetchHttpClient, Headers, HttpRouter, HttpServer, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { Jwks, Jwt } from 'effect/jose';
import { User } from '#/user';

// --- [MODELS] --------------------------------------------------------------------------

class Access extends Schema.Class<Access, { readonly brand: unique symbol }>('Access')({ issuer: Schema.String, audience: Schema.String, keys: Schema.String, logout: Schema.String }) {
    static readonly binding = 'ACCESS';
}
const Claims = Schema.Struct({ [User.sites.headers.id]: Schema.String, [User.sites.headers.email]: Schema.String }).pipe(Schema.encodeKeys({ [User.sites.headers.id]: 'sub', [User.sites.headers.email]: 'email' }));
const ReturnTo = Schema.fromURLSearchParams(Schema.Struct({ [User.sites.returnTo]: Schema.String }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const returned = (url: URL) =>
    Schema.decodeOption(ReturnTo)(url.searchParams).pipe(
        Option.flatMap(({ [User.sites.returnTo]: target }) => Option.fromNullOr(URL.parse(target, url.origin))),
        Option.filter(({ origin }) => origin === url.origin),
        Option.getOrElse(() => new URL('/', url.origin)),
    );
const identify = (settings: Access, request: HttpServerRequest.HttpServerRequest) => {
    const anonymous = Headers.removeMany(request.headers, Object.values(User.sites.headers));
    return Effect.fromNullishOr(request.cookies.CF_Authorization).pipe(
        Effect.flatMap((token) => Jwt.verifyWithKeySet(Claims, token, { issuer: settings.issuer, audience: settings.audience, algorithms: ['RS256'] })),
        Effect.option,
        Effect.map(Option.match({ onNone: () => anonymous, onSome: ({ claims }) => Headers.setAll(anonymous, claims) })),
    );
};
const admit = Effect.fn('admit')(function* <E, R>(settings: Access, app: Effect.Effect<HttpServerResponse.HttpServerResponse, E, R>) {
    const request = yield* HttpServerRequest.HttpServerRequest;
    const url = yield* Effect.fromOption(HttpServerRequest.toURL(request)).pipe(Effect.orDie);
    return yield* Match.value(url.pathname).pipe(
        Match.when(User.sites.paths.signin, () => Effect.succeed(HttpServerResponse.redirect(returned(url)))),
        Match.when(User.sites.paths.signout, () => Effect.succeed(HttpServerResponse.redirect(new URL(settings.logout, url.origin)))),
        Match.orElse(() => Effect.flatMap(identify(settings, request), (headers) => Effect.provideService(app, HttpServerRequest.HttpServerRequest, request.modify({ headers })))),
    );
});
const gate = (settings: Access) =>
    HttpRouter.middleware(
        Effect.map(
            Effect.context<Jwks.Jwks | Signature.Signature>(),
            flow(Context.pick(Jwks.Jwks, Signature.Signature), (services) => (app) => Effect.provideContext(admit(settings, app), services)),
        ),
        { global: true },
    ).pipe(Layer.provide([Jwks.layerRemote({ url: settings.keys }).pipe(Layer.provide(FetchHttpClient.layer), Layer.provide(Layer.succeed(FetchHttpClient.RequestInit, { redirect: 'manual' }))), Signature.layerWebCrypto.pipe(Layer.provide(SecureRandom.layerWebCrypto))]));

// --- [COMPOSITION] ---------------------------------------------------------------------

const worker = <A, E>(layer: Layer.Layer<A, E, HttpRouter.HttpRouter | Layer.Success<typeof HttpServer.layerServices> | HttpRouter.Request<'Error', unknown> | HttpRouter.Request<'GlobalError', unknown> | HttpRouter.Request<'Requires', A> | HttpRouter.Request<'GlobalRequires', A>>) => {
    const handler = Function.memoize(
        (env: object) =>
            HttpRouter.toWebHandler(
                Layer.mergeAll(layer, Config.schema(Access, Access.binding).pipe(Config.option, Effect.map(Option.match({ onNone: () => Layer.empty, onSome: gate })), Layer.unwrap, Layer.provide(ConfigProvider.layer(ConfigProvider.fromUnknown(env))))).pipe(
                    Layer.provide([HttpServer.layerServices, Logger.layer([Logger.withLeveledConsole(Logger.formatStructured)])]),
                    Layer.provideMerge(Layer.succeed(Scheduler.Scheduler, new Scheduler.MixedScheduler('sync'))),
                ),
                { disableLogger: true },
            ).handler,
    );
    return { fetch: (request: Request, env: object) => handler(env)(request, Context.empty()) };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Access, worker };
