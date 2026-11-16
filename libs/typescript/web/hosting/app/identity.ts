import { Array, Config, Context, Data, Effect, Layer, Option, Record, Schema, SchemaGetter, SchemaTransformation, Struct } from 'effect';
import { SecureRandom, Signature } from 'effect/crypto';
import { FetchHttpClient, Headers, HttpMethod, HttpRouter, HttpServerRequest, HttpServerResponse, UrlParams } from 'effect/http';
import { HttpApiError } from 'effect/http-api';
import { Jwks, Jwt } from 'effect/jose';

// --- [CONSTANTS] -----------------------------------------------------------------------

const address = new Intl.Collator(undefined, { usage: 'search', sensitivity: 'accent' });
const sameAddress = (self: string, that: string) => address.compare(self, that) === 0;

// --- [MODELS] --------------------------------------------------------------------------

const Header = Schema.String.pipe(Schema.decode(SchemaTransformation.toLowerCase()));
const Addresses = Schema.fromJsonString(Schema.Array(Schema.String));

class User extends Schema.Class<User, { readonly brand: unique symbol }>('User')({ id: Schema.NonEmptyString, email: Schema.OptionFromOptionalKey(Schema.String) }) {}

const Claims = User.pipe(Schema.encodeKeys({ id: 'sub' }));
const ReturnTo = Schema.Struct({ target: Schema.String });

class Identity extends Data.Class<{
    readonly headers: { readonly id: string; readonly email: string };
    readonly paths: { readonly signin: HttpRouter.PathInput; readonly signout: HttpRouter.PathInput };
    readonly returnTo: string;
}> {
    static readonly sites = new Identity({ headers: { id: 'oai-authenticated-user-id', email: 'oai-authenticated-user-email' }, paths: { signin: '/signin-with-chatgpt', signout: '/signout-with-chatgpt' }, returnTo: 'return_to' });
    readonly user = User.pipe(Schema.encodeKeys(this.headers));
    readonly current = HttpServerRequest.schemaHeaders(this.user).pipe(Effect.option);
    href(action: keyof Identity['paths'], target: string) {
        return `${this.paths[action]}?${UrlParams.toString(UrlParams.fromInput({ [this.returnTo]: target }))}`;
    }
}

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
    static readonly config = Config.schema(Access.json, Access.variable);
    static readonly cloudflare = { header: 'cf-access-jwt-assertion', cookie: 'CF_Authorization', algorithm: 'RS256', certs: '/cdn-cgi/access/certs', logout: '/cdn-cgi/access/logout' } as const;
    layer(identity: Identity) {
        const returning = ReturnTo.pipe(Schema.encodeKeys({ target: identity.returnTo }));
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
        const verified = Access.fields.source.match(this.source, {
            Cloudflare: ({ teamDomain, audience }) =>
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
            Forwarded: (names) => Effect.succeed((request: HttpServerRequest.HttpServerRequest) => Schema.decodeEffect(User.pipe(Schema.encodeKeys(names)))(request.headers)),
        });
        const identify = Effect.fn('identify')(function* <E, R>(app: Effect.Effect<HttpServerResponse.HttpServerResponse, E, R>, read: Effect.Success<typeof verified>) {
            const request = yield* HttpServerRequest.HttpServerRequest;
            const anonymous = Headers.removeMany(request.headers, Object.values(identity.headers));
            const headers = yield* read(request).pipe(
                Effect.flatMap((user) => Effect.orDie(Schema.encodeEffect(identity.user)(user))),
                Effect.map((signed) => Headers.setAll(anonymous, signed)),
                Effect.catchTag(['NoSuchElementError', 'SchemaError'], () => Effect.succeed(anonymous)),
                Effect.catchReason('JoseError', 'KeySetUnavailable', Effect.fail, () => Effect.succeed(anonymous)),
                Effect.tapCause(Effect.logError),
                Effect.mapError(() => HttpApiError.ServiceUnavailable.make()),
            );
            return yield* Effect.provideService(app, HttpServerRequest.HttpServerRequest, request.modify({ headers }));
        });
        return Layer.mergeAll(
            HttpRouter.add('*', identity.paths.signin, signin),
            HttpRouter.add('*', identity.paths.signout, HttpServerResponse.redirect(this.logout)),
            HttpRouter.middleware(
                Effect.map(verified, (read) => (app) => identify(app, read)),
                { global: true },
            ),
        );
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

class Enrollment extends Context.Service<Enrollment, { readonly identity: Identity; readonly allowed: readonly string[] }>()('@rasm/hosting/app/identity/Enrollment') {
    static readonly variable = 'ALLOWED';
    static readonly Session = Schema.Literals(['signedOut', 'signedIn', 'allowed']);
    static readonly layer = ({ identity, allowed }: Enrollment['Service']) =>
        Layer.effect(
            Enrollment,
            Effect.map(Config.schema(Addresses, Enrollment.variable).pipe(Config.withDefault(allowed)), (enrolled) => ({ identity, allowed: enrolled })),
        );
    static readonly session = Effect.fn('Enrollment.session')(function* <E, R>(store: {
        readonly enrolled: (addresses: Array.NonEmptyReadonlyArray<string>) => Effect.Effect<readonly { readonly email: string; readonly userId: string }[], E, R>;
        readonly enroll: (entry: { readonly email: string; readonly userId: string }) => Effect.Effect<readonly { readonly userId: string }[], E, R>;
    }) {
        const { identity, allowed } = yield* Enrollment;
        return yield* Option.match(yield* identity.current, {
            onNone: () => Effect.succeed('signedOut' as const),
            onSome: Effect.fnUntraced(function* (user) {
                const enrolled = yield* Array.match(allowed, { onEmpty: () => Effect.succeed([]), onNonEmpty: store.enrolled });
                const open = Array.differenceWith(sameAddress)(
                    allowed,
                    enrolled.map(({ email }) => email),
                );
                const unclaimed = Array.head(Array.intersectionWith(sameAddress)(open, Option.toArray(user.email)));
                const claimed = yield* Option.match(unclaimed, { onNone: () => Effect.succeed(enrolled), onSome: (email) => store.enroll({ email, userId: user.id }) });
                return claimed.some((entry) => entry.userId === user.id) ? ('allowed' as const) : ('signedIn' as const);
            }),
        });
    });
    static readonly sameOrigin = Effect.filterOrFail(
        HttpServerRequest.HttpServerRequest,
        ({ method, headers }) => !HttpMethod.hasBody(method) || headers['sec-fetch-site'] === 'same-origin',
        () => HttpApiError.Forbidden.make(),
    );
    static readonly unshared = HttpServerResponse.setHeader('Cache-Control', 'private, no-store');
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Access, Enrollment, Identity, User };
