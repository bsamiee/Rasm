import { createHmac, generateKeyPairSync, type KeyObject, sign } from 'node:crypto';
import { createServer } from 'node:https';
import { getCACertificates, setDefaultCACertificates } from 'node:tls';
import { layer, layerHttpServices, layerTest } from '@effect/platform-node/NodeHttpServer';
import { assert, it } from '@effect/vitest';
import { Access, Identity, User } from '@rasm/hosting';
import { Array, Cause, Context, DateTime, Duration, Effect, FileSystem, Function, Layer, Match, Option, Path, type Record, Schema } from 'effect';
import { Base64Url } from 'effect/encoding';
import { HttpClient, HttpClientRequest, HttpRouter, HttpServer, HttpServerResponse } from 'effect/http';
import { ChildProcess, ChildProcessSpawner } from 'effect/process';

// --- [CONSTANTS] -----------------------------------------------------------------------

const identity = new Identity({ headers: { id: 'x-user-id', email: 'x-user-email' }, paths: { signin: '/signin', signout: '/signout' }, returnTo: 'return_to' });
const audience = '32eafc7626e974616deaf0dc3ce63d7bcbed58a2731e84d06bc3cdf1b53c4228';
const proxied = { id: 'x-forwarded-user', email: 'x-forwarded-email' };
const subject = { sub: '7335d417-61da-459d-899c-0a01c76a2f94', email: 'user@example.com' };
const signedIn = [{ id: subject.sub, email: subject.email }];
const spoofed = { [identity.headers.id]: 'spoofed', [identity.headers.email]: 'spoofed@example.com' };
const modulusLength = 2048;
const [trusted, stranger] = [generateKeyPairSync('rsa', { modulusLength }), generateKeyPairSync('rsa', { modulusLength })];
const kid = 'f1e0a2c6';
const answers = { served: 200, unauthorized: 401, forbidden: 403, unavailable: 503 } as const;
const unsigned: readonly number[] = [answers.served, answers.unauthorized, answers.forbidden];
const quiet = { disableLogger: true, disableListenLog: true };
const loopback = '127.0.0.1';
const claimed = /\.[^.]+\./u;

// --- [MODELS] --------------------------------------------------------------------------

const Segment = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Json));
const Seen = Schema.Array(Schema.Struct({ id: Schema.String, email: Schema.optionalKey(Schema.String) }));
const Users = Schema.Array(User);

// --- [SERVICES] ------------------------------------------------------------------------

class Team extends Context.Service<Team, { readonly key: string; readonly cert: string }>()('@rasm/test-hosting/access.test/Team') {
    static readonly layer = Layer.effect(
        this,
        Effect.gen(function* () {
            const [fs, path, spawner] = yield* Effect.all([FileSystem.FileSystem, Path.Path, ChildProcessSpawner.ChildProcessSpawner]);
            const files = yield* Effect.map(fs.makeTempDirectoryScoped(), (folder) => ({ key: path.join(folder, 'key.pem'), cert: path.join(folder, 'cert.pem') }));
            const issued = ['req', '-x509', '-newkey', `rsa:${modulusLength}`, '-nodes', '-keyout', files.key, '-out', files.cert, '-days', '1', '-subj', `/CN=${loopback}`, '-addext', `subjectAltName=IP:${loopback}`];
            yield* Effect.filterOrFail(
                spawner.exitCode(ChildProcess.make('openssl', issued, { stdin: 'ignore', stderr: 'ignore' })),
                (code) => code === 0,
                (code) => new Cause.UnknownError(code, `openssl req exited ${code}`),
            );
            const pair = yield* Effect.all({ key: fs.readFileString(files.key), cert: fs.readFileString(files.cert) });
            yield* Effect.sync(() => setDefaultCACertificates([...getCACertificates('default'), pair.cert]));
            return pair;
        }),
    ).pipe(Layer.provideMerge(layerHttpServices));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const segment = (fields: typeof Segment.Type) => Effect.map(Schema.encodeEffect(Segment)(fields), Base64Url.encode);
const rs256 = (key: KeyObject) => (input: string) => Base64Url.encode(sign('RSA-SHA256', new TextEncoder().encode(input), key));
const token = Effect.fnUntraced(function* (teamDomain: string, now: DateTime.Utc, claims: typeof Segment.Type = {}, header: typeof Segment.Type = {}, signature: (input: string) => string = rs256(trusted.privateKey)) {
    const issued = DateTime.toEpochSeconds(now);
    const parts = yield* Effect.all([segment({ alg: Access.cloudflare.algorithm, kid, typ: 'JWT', ...header }), segment({ aud: [audience], ...subject, iss: `https://${teamDomain}`, iat: issued, nbf: issued, exp: issued + Duration.toSeconds('1 hour'), type: 'app', ...claims })]);
    return `${parts.join('.')}.${signature(parts.join('.'))}`;
});

const serving = Effect.gen(function* () {
    const { key, cert } = yield* Team;
    const certs = HttpServerResponse.jsonUnsafe({ keys: [{ ...trusted.publicKey.export({ format: 'jwk' }), kid, alg: Access.cloudflare.algorithm, use: 'sig' }] });
    const context = yield* Layer.build(
        Layer.provideMerge(
            HttpRouter.serve(HttpRouter.add('GET', Access.cloudflare.certs, certs), quiet),
            layer(() => createServer({ key, cert }), { port: 0, host: loopback }),
        ),
    );
    return yield* Match.valueTags(Context.get(context, HttpServer.HttpServer).address, { InetAddressV4: ({ port }) => Effect.succeed(`${loopback}:${port}`), InetAddressV6: Effect.die, UnixPathAddress: Effect.die });
});

const seen = Effect.fnUntraced(function* (source: (typeof Access.Encoded)['source'], headers: Record.ReadonlyRecord<string, string>) {
    const access = yield* Schema.decodeEffect(Access)({ source, logout: Access.cloudflare.logout });
    const who = HttpRouter.add(
        'GET',
        '/who',
        Effect.flatMap(identity.current, (user) => HttpServerResponse.schemaJson(Users)(Option.toArray(user))),
    );
    yield* Layer.build(HttpRouter.serve(Layer.merge(access.layer(identity), who), quiet));
    const response = yield* HttpClient.execute(HttpClientRequest.get('/who', { headers }));
    return { status: response.status, users: response.status === answers.served ? yield* Effect.flatMap(response.json, Schema.decodeUnknownEffect(Seen)) : [] };
}, Effect.provide(layerTest));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(Team.layer, { excludeTestServices: true })('access', (methods) => {
    methods.effect.each([
        { label: 'a token in the Access header', sent: (jwt: string) => ({ [Access.cloudflare.header]: jwt }), change: {}, users: signedIn },
        { label: 'a token in the Access cookie', sent: (jwt: string) => ({ cookie: `${Access.cloudflare.cookie}=${jwt}` }), change: {}, users: signedIn },
        { label: 'a token beside identity headers the client sent', sent: (jwt: string) => ({ ...spoofed, [Access.cloudflare.header]: jwt }), change: {}, users: signedIn },
        { label: 'a token for another application audience', sent: (jwt: string) => ({ [Access.cloudflare.header]: jwt }), change: { aud: ['f5d8c1b9d3f24c3e9a5d6f1d2b7c8e90a1b2c3d4e5f60718293a4b5c6d7e8f90'] }, users: [] },
        { label: 'a token another team issued', sent: (jwt: string) => ({ [Access.cloudflare.header]: jwt }), change: { iss: 'https://other.cloudflareaccess.com' }, users: [] },
        { label: 'identity headers the client sent and no token', sent: () => spoofed, change: {}, users: [] },
        { label: 'no token', sent: () => ({}), change: {}, users: [] },
    ])(
        'Cloudflare source: $label signs in $users',
        Effect.fnUntraced(function* ({ sent, change, users }) {
            const [teamDomain, now] = yield* Effect.all([serving, DateTime.now]);
            const { status, users: found } = yield* seen({ _tag: 'Cloudflare', teamDomain, audience }, sent(yield* token(teamDomain, now, change)));
            assert.include(Array.isReadonlyArrayNonEmpty(users) ? [answers.served] : unsigned, status);
            assert.deepStrictEqual(found, users);
        }),
    );

    methods.effect.each([
        { label: 'an expired token', forged: (teamDomain: string, now: DateTime.Utc) => token(teamDomain, DateTime.subtract(now, { hours: 2 })) },
        { label: 'a token a key outside the served set signed', forged: (teamDomain: string, now: DateTime.Utc) => token(teamDomain, now, {}, {}, rs256(stranger.privateKey)) },
        {
            label: 'a token with a changed claim under its old signature',
            forged: (teamDomain: string, now: DateTime.Utc) => Effect.zipWith(token(teamDomain, now), segment({ aud: [audience], sub: 'forged', iss: `https://${teamDomain}`, exp: DateTime.toEpochSeconds(now) + Duration.toSeconds('1 hour') }), (signed, claims) => signed.replace(claimed, `.${claims}.`)),
        },
        { label: 'an unsigned token naming algorithm none', forged: (teamDomain: string, now: DateTime.Utc) => token(teamDomain, now, {}, { alg: 'none' }, Function.constant('')) },
        {
            label: 'an HS256 token keyed by the served public key',
            forged: (teamDomain: string, now: DateTime.Utc) =>
                token(teamDomain, now, {}, { alg: 'HS256' }, (input) =>
                    createHmac('sha256', trusted.publicKey.export({ type: 'spki', format: 'pem' }))
                        .update(input)
                        .digest('base64url'),
                ),
        },
        { label: 'text that is no token', forged: () => Effect.succeed('not.a.token') },
    ])(
        'Cloudflare source: $label signs nobody in',
        Effect.fnUntraced(function* ({ forged }) {
            const [teamDomain, now] = yield* Effect.all([serving, DateTime.now]);
            const { status, users } = yield* seen({ _tag: 'Cloudflare', teamDomain, audience }, { ...spoofed, [Access.cloudflare.header]: yield* forged(teamDomain, now) });
            assert.include(unsigned, status);
            assert.deepStrictEqual(users, []);
        }),
    );

    methods.effect(
        'Cloudflare source: takes the team domain with the scheme Cloudflare states it with',
        Effect.fnUntraced(function* () {
            const [teamDomain, now] = yield* Effect.all([serving, DateTime.now]);
            const found = yield* seen({ _tag: 'Cloudflare', teamDomain: `https://${teamDomain}`, audience }, { [Access.cloudflare.header]: yield* token(teamDomain, now) });
            assert.deepStrictEqual(found, { status: answers.served, users: signedIn });
        }),
    );

    methods.effect(
        'Cloudflare source: answers a token 503 while the team serves no key set',
        Effect.fnUntraced(function* () {
            const [teamDomain, now] = yield* Effect.all([Effect.scoped(serving), DateTime.now]);
            const found = yield* seen({ _tag: 'Cloudflare', teamDomain, audience }, { [Access.cloudflare.header]: yield* token(teamDomain, now) });
            assert.deepStrictEqual(found, { status: answers.unavailable, users: [] });
        }),
    );

    methods.effect.each([
        { label: 'headers the proxy set', headers: { [proxied.id]: subject.sub, [proxied.email]: subject.email }, users: signedIn },
        { label: 'a proxy id header alone', headers: { [proxied.id]: subject.sub }, users: [{ id: subject.sub }] },
        { label: 'proxy headers beside identity headers the client sent', headers: { ...spoofed, [proxied.id]: subject.sub, [proxied.email]: subject.email }, users: signedIn },
        { label: 'identity headers the client sent and no proxy header', headers: spoofed, users: [] },
        { label: 'no header', headers: {}, users: [] },
    ])(
        'Forwarded source: $label signs in $users',
        Effect.fnUntraced(function* ({ headers, users }) {
            assert.deepStrictEqual(yield* seen({ _tag: 'Forwarded', ...proxied }, headers), { status: answers.served, users });
        }),
    );
});
