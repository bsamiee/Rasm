import { Array, Cause, Config, Context, Effect, Layer, Option, Record, Schema, SchemaGetter, SchemaTransformation, Struct } from 'effect';
import { SecureRandom, Signature } from 'effect/crypto';
import { FetchHttpClient, Headers, HttpMethod, HttpRouter, HttpServerRequest, HttpServerResponse, UrlParams } from 'effect/http';
import { HttpApiError } from 'effect/http-api';
import { Jwks, type JoseError, Jwt } from 'effect/jose';

// --- [CONSTANTS] -----------------------------------------------------------------------

const address = new Intl.Collator(undefined, { usage: 'search', sensitivity: 'accent' });
const development = { cookie: 'development-user', page: '<!doctype html><title>Sign in</title><form method="post"><label>Email <input autofocus name="email" required type="email"></label> <button>Sign in</button></form>' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Header = Schema.String.pipe(Schema.decode(SchemaTransformation.toLowerCase()));
const Addresses = Schema.fromJsonString(Schema.Array(Schema.String));

class User extends Schema.Class<User, { readonly brand: unique symbol }>('User')({ id: Schema.NonEmptyString, email: Schema.OptionFromOptionalKey(Schema.String) }) {}

const Claims = User.pipe(Schema.encodeKeys({ id: 'sub' }));
const Remembered = Schema.fromJsonString(User);
const SignedIn = Schema.Struct({ email: Schema.NonEmptyString });
const ReturnTo = Schema.Struct({ return_to: Schema.String });

class Access extends Schema.Class<Access, { readonly brand: unique symbol }>('Access')({
    source: Schema.TaggedUnion({
        Cloudflare: {
            teamDomain: Schema.String.pipe(
                Schema.decodeTo(Schema.URLFromString.check(Schema.makeFilter(({ href, origin }) => href === `${origin}/`, { expected: 'an HTTPS origin or its host' })), {
                    decode: SchemaGetter.transform((stated: string) => (URL.parse(stated)?.protocol === 'https:' ? stated : `https://${stated}`)),
                    encode: SchemaGetter.passthrough(),
                }),
            ),
            audience: Schema.String,
        },
        Forwarded: { id: Header, email: Header },
    }),
    logout: Schema.String,
}) {
    static readonly variable = 'ACCESS';
    static readonly json = Schema.fromJsonString(Access);
    static readonly cloudflare = { header: 'cf-access-jwt-assertion', cookie: 'CF_Authorization', algorithm: 'RS256', certs: '/cdn-cgi/access/certs', logout: '/cdn-cgi/access/logout' } as const;
}

// --- [SERVICES] ------------------------------------------------------------------------

class Identity extends Context.Service<Identity, Effect.Effect<Option.Option<User>, HttpApiError.ServiceUnavailable, HttpServerRequest.HttpServerRequest>>()('@rasm/hosting/app/identity') {
    static readonly paths = { signin: '/signin', signout: '/signout' } as const;
    static readonly href = (action: keyof typeof Identity.paths, target: string) => `${Identity.paths[action]}?${UrlParams.toString(UrlParams.fromInput(ReturnTo.make({ return_to: target })))}`;
    static readonly back = Effect.gen(function* () {
        const request = yield* HttpServerRequest.HttpServerRequest;
        const [url, returned] = yield* Effect.all([Effect.orDie(Effect.fromOption(HttpServerRequest.toURL(request))), Effect.option(HttpServerRequest.schemaSearchParams(ReturnTo))]);
        const target = returned.pipe(
            Option.flatMapNullishOr(({ return_to }) => URL.parse(return_to, url.origin)),
            Option.filter(({ origin }) => origin === url.origin),
            Option.getOrElse(() => new URL('/', url.origin)),
        );
        return HttpServerResponse.redirect(target);
    });
    static readonly readers = {
        Cloudflare: ({ teamDomain, audience }: typeof Access.fields.source.cases.Cloudflare.Type) =>
            Effect.map(
                Layer.build(
                    Layer.mergeAll(Jwks.layerRemote({ url: new URL(Access.cloudflare.certs, teamDomain).href }).pipe(Layer.provide(FetchHttpClient.layer), Layer.provide(Layer.succeed(FetchHttpClient.RequestInit, { redirect: 'manual' }))), Signature.layerWebCrypto.pipe(Layer.provide(SecureRandom.layerWebCrypto))),
                ),
                (keys) => (request: HttpServerRequest.HttpServerRequest) =>
                    Effect.fromOption(Option.orElse(Headers.get(request.headers, Access.cloudflare.header), () => Record.get(request.cookies, Access.cloudflare.cookie))).pipe(
                        Effect.flatMap((token) => Jwt.verifyWithKeySet(Claims, token, { issuer: teamDomain.origin, audience, algorithms: [Access.cloudflare.algorithm] })),
                        Effect.map(Struct.get('claims')),
                        Effect.provideContext(keys),
                    ),
            ),
        Forwarded: (names: typeof Access.fields.source.cases.Forwarded.Type) => Effect.succeed((request: HttpServerRequest.HttpServerRequest) => Schema.decodeEffect(User.pipe(Schema.encodeKeys(Struct.pick(names, ['id', 'email']))))(request.headers)),
    };
    static readonly source = Effect.map(
        Config.option(Config.schema(Access.json, Access.variable)),
        Option.match({
            onNone: () =>
                import.meta.env.DEV
                    ? {
                          reader: Effect.succeed((request: HttpServerRequest.HttpServerRequest) => Effect.flatMap(Effect.fromOption(Record.get(request.cookies, development.cookie)), Schema.decodeEffect(Remembered))),
                          routes: Layer.mergeAll(
                              HttpRouter.add('GET', Identity.paths.signin, HttpServerResponse.html(development.page)),
                              HttpRouter.add(
                                  'POST',
                                  Identity.paths.signin,
                                  Effect.gen(function* () {
                                      const { email } = yield* HttpServerRequest.schemaBodyUrlParams(SignedIn);
                                      const remembered = yield* Effect.orDie(Schema.encodeEffect(Remembered)(User.make({ id: email, email: Option.some(email) })));
                                      return yield* HttpServerResponse.setCookie(yield* Identity.back, development.cookie, remembered, { path: '/', httpOnly: true, sameSite: 'lax' });
                                  }),
                              ),
                              HttpRouter.add('*', Identity.paths.signout, Effect.flatMap(Identity.back, HttpServerResponse.expireCookie(development.cookie, { path: '/' }))),
                          ),
                      }
                    : { reader: Effect.succeed(() => Effect.fail(new Cause.NoSuchElementError(`${Access.variable} names no access source`))), routes: Layer.empty },
            onSome: ({ source, logout }) => ({
                reader: Access.fields.source.match(source, Identity.readers),
                routes: Layer.mergeAll(HttpRouter.add('*', Identity.paths.signin, Identity.back), HttpRouter.add('*', Identity.paths.signout, HttpServerResponse.redirect(logout))),
            }),
        }),
    );
    static readonly signed = (read: (request: HttpServerRequest.HttpServerRequest) => Effect.Effect<User, Cause.NoSuchElementError | Schema.SchemaError | JoseError.JoseError>) =>
        HttpServerRequest.HttpServerRequest.pipe(
            Effect.flatMap(read),
            Effect.asSome,
            Effect.catchTag(['NoSuchElementError', 'SchemaError'], () => Effect.succeedNone),
            Effect.catchReason('JoseError', 'KeySetUnavailable', Effect.fail, () => Effect.succeedNone),
            Effect.tapCause(Effect.logError),
            Effect.mapError(() => HttpApiError.ServiceUnavailable.make()),
        );
    static readonly layer = Layer.effect(Identity, Identity.source.pipe(Effect.flatMap(Struct.get('reader')), Effect.map(Identity.signed)));
    static readonly routes = Layer.unwrap(Effect.map(Identity.source, Struct.get('routes')));
}

class Enrollment extends Context.Service<Enrollment, readonly string[]>()('@rasm/hosting/app/identity/Enrollment') {
    static readonly variable = 'ALLOWED';
    static readonly Session = Schema.Literals(['signedOut', 'signedIn', 'allowed']);
    static readonly layer = (allowed: readonly string[]) => Layer.effect(Enrollment, Config.schema(Addresses, Enrollment.variable).pipe(Config.withDefault(allowed)));
    static readonly session = Effect.all([Enrollment, Effect.flatten(Identity)]).pipe(
        Effect.map(([allowed, signed]) =>
            Option.match(signed, {
                onNone: () => 'signedOut' as const,
                onSome: ({ email }) => (Option.exists(email, (signedIn) => Array.containsWith((self: string, that: string) => address.compare(self, that) === 0)(allowed, signedIn)) ? ('allowed' as const) : ('signedIn' as const)),
            }),
        ),
        Effect.withSpan('Enrollment.session'),
    );
    static readonly sameOrigin = Effect.filterOrFail(
        HttpServerRequest.HttpServerRequest,
        ({ method, headers }) => !HttpMethod.hasBody(method) || headers['sec-fetch-site'] === 'same-origin',
        () => HttpApiError.Forbidden.make(),
    );
    static readonly unshared = HttpServerResponse.setHeader('Cache-Control', 'private, no-store');
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Access, Enrollment, Identity, User };
