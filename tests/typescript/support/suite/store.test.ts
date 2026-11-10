import { once } from 'node:events';
import { createServer } from 'node:http2';
import { Code, ConnectError, type ConnectRouter } from '@connectrpc/connect';
import { connectNodeAdapter } from '@connectrpc/connect-node';
import { assert, it } from '@effect/vitest';
import { Array, Cause, Config, ConfigProvider, Context, Deferred, Effect, Exit, flow, HashMap, Layer, Option, Redacted, Ref, Result, Schema, type Scope, Stream } from 'effect';
import { StoreService } from '#/.buf/rasm/store/store_pb';
import { Store, StoreError } from '#/store';

// --- [TYPES] ---------------------------------------------------------------------------

interface Served {
    readonly calls: readonly string[];
    readonly refusals: readonly (readonly [string, Code])[];
    readonly issued: number;
    readonly builds: HashMap.HashMap<string, Option.Option<string>>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const silent = /HoldSession \S+ at http:\/\/127\.0\.0\.1:1\/: silent: /u;

// --- [ERRORS] --------------------------------------------------------------------------

class Migration extends Schema.TaggedError<Migration, { readonly brand: unique symbol }>()('Migration', {}) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const signatures = (credentials: () => Promise<{ readonly accessKeyId: string; readonly secretAccessKey: string; readonly sessionToken: string }>) =>
    Effect.map(
        Effect.promise(() => Promise.all([credentials(), credentials()])),
        Array.map(({ accessKeyId, secretAccessKey, sessionToken }) => [accessKeyId, secretAccessKey, sessionToken]),
    );

const postgresRecord = (id: string) => ({ id, host: '127.0.0.1', port: 7411, database: id, username: id, password: 'secret', connectionLimit: 3 });
const bucketRecord = (id: string) => ({ id, endpoint: 'http://127.0.0.1:7412', region: 'us-east-1', bucket: id, accessKeyId: 'key', secretAccessKey: 'secret', sessionToken: 'token', credentialsTtlSeconds: 0 });
const clickhouseRecord = (id: string) => ({ id, host: '127.0.0.1', httpPort: 7413, database: id, username: id, password: 'secret' });
const cacheRecord = (id: string) => ({ id, host: '127.0.0.1', port: 7415, database: 3, username: id, password: 'secret', channelPrefix: `${id}:` });

// --- [SERVICES] ------------------------------------------------------------------------

class Contract extends Context.Service<Contract>()('@rasm/test-support/suite/store.test/Contract', {
    make: Effect.gen(function* () {
        const [served, released, services] = yield* Effect.all([Ref.make<Served>({ calls: [], refusals: [], issued: 0, builds: HashMap.empty() }), Deferred.make<void>(), Effect.context<never>()]);
        const answered = <A>(rpc: string, respond: (held: Served, id: string) => readonly [Result.Result<readonly [string, A], Code>, Served]) =>
            Ref.modify(served, (held) => {
                const [reply, next] = Option.match(
                    Array.findFirst(held.refusals, ([name]) => name === rpc),
                    { onNone: () => respond(held, `store${held.issued + 1}`), onSome: (scripted) => [Result.fail(scripted[1]), { ...held, refusals: held.refusals.filter((row) => row !== scripted) }] as const },
                );
                const subject = Result.match(reply, { onFailure: (code) => Code[code], onSuccess: ([named]) => named });
                return [Result.mapBoth(reply, { onFailure: (code) => new ConnectError(rpc, code), onSuccess: ([, answer]) => answer }), { ...next, calls: [...next.calls, `${rpc} ${subject}`] }] as const;
            }).pipe(Effect.runSyncWith(services), Result.getOrThrow);
        const created =
            <A>(rpc: string, record: (id: string) => A) =>
            () =>
                answered(rpc, (held, id) => [Result.succeed([id, record(id)]), { ...held, issued: held.issued + 1 }]);
        const opened =
            <A>(rpc: string, record: (id: string) => A) =>
            ({ digest }: { readonly digest: string }) =>
                answered(rpc, (held, id) => (HashMap.has(held.builds, digest) ? [Result.fail(Code.AlreadyExists), held] : [Result.succeed([id, record(id)]), { ...held, issued: held.issued + 1, builds: HashMap.set(held.builds, digest, Option.some(id)) }]));
        const cloned =
            <A>(rpc: string, record: (id: string) => A) =>
            ({ digest }: { readonly digest: string }) =>
                answered(rpc, (held, id) => (Option.exists(HashMap.get(held.builds, digest), Option.isNone) ? [Result.succeed([id, record(id)]), { ...held, issued: held.issued + 1 }] : [Result.fail(Code.NotFound), held]));
        const sealed = ({ id }: { readonly id: string }) =>
            answered('SealTemplate', (held) =>
                Option.match(HashMap.findFirst(held.builds, Option.contains(id)), {
                    onNone: () => [Result.fail(Code.NotFound), held],
                    onSome: ([digest]) => [Result.succeed([id, {}]), { ...held, builds: HashMap.set(held.builds, digest, Option.none()) }],
                }),
            );
        const lease = Stream.sync(() => answered('HoldSession', (held) => [Result.succeed(['held', { ttlSeconds: 10 }]), held]));
        const aborted = (signal: AbortSignal) => Stream.fromEventListener(signal, 'abort').pipe(Stream.take(1), Stream.drain);
        const routes = (router: ConnectRouter) =>
            router.service(StoreService, {
                holdSession: (_, { signal }) => lease.pipe(Stream.concat(aborted(signal)), Stream.ensuring(Deferred.succeed(released, undefined)), Stream.toAsyncIterable),
                renewSession: () => answered('RenewSession', (held) => [Result.succeed(['renewed', { ttlSeconds: 43_200 }]), held]),
                endSession: () => answered('EndSession', (held) => [Result.succeed(['ended', {}]), held]),
                createPostgres: created('CreatePostgres', postgresRecord),
                createBucket: created('CreateBucket', bucketRecord),
                createClickhouse: created('CreateClickhouse', clickhouseRecord),
                createCache: created('CreateCache', cacheRecord),
                renewBucket: ({ id }) => answered('RenewBucket', (held) => [Result.succeed([id, bucketRecord(id)]), held]),
                keepStore: ({ id }) => answered('KeepStore', (held) => [Result.succeed([id, {}]), held]),
                dropStore: ({ id }) => answered('DropStore', (held) => [Result.succeed([id, {}]), { ...held, builds: HashMap.filter(held.builds, (build) => !Option.contains(build, id)) }]),
                openPostgresTemplate: opened('OpenPostgresTemplate', postgresRecord),
                openClickhouseTemplate: opened('OpenClickhouseTemplate', clickhouseRecord),
                sealTemplate: sealed,
                clonePostgresTemplate: cloned('ClonePostgresTemplate', postgresRecord),
                cloneClickhouseTemplate: cloned('CloneClickhouseTemplate', clickhouseRecord),
            });
        const server = yield* Effect.acquireRelease(
            Effect.sync(() => createServer(connectNodeAdapter({ routes })).listen(0, '127.0.0.1')),
            (listening) =>
                Effect.promise(() => {
                    const closed = once(listening, 'close');
                    listening.close();
                    return closed;
                }),
        );
        yield* Effect.promise(() => once(server, 'listening'));
        const address = server.address();
        return {
            port: typeof address === 'object' && address !== null ? address.port : 0,
            calls: Effect.map(Ref.get(served), ({ calls }) => calls),
            released: Deferred.await(released),
            refuse: (rpc: string, code: Code) => Ref.update(served, (held) => ({ ...held, refusals: [...held.refusals, [rpc, code] as const] })),
        };
    }),
}) {
    static readonly layer = Layer.effect(this, this.make);
    static readonly stores = Layer.unwrap(Contract.use(({ port }) => Effect.succeed(Store.layer(Config.succeed(port)))));
    static readonly calls = Contract.use(({ calls }) => calls);
    static readonly sessioned = <A, E>(body: Effect.Effect<A, E, Store | Scope.Scope>) => Effect.exit(Effect.provide(Effect.scoped(body), Contract.stores)).pipe(Effect.tap(Contract.use(({ released }) => released)));
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const postgres = Store.use((store) => store.postgres);
const bucket = Store.use((store) => store.bucket);
const cache = Store.use((store) => store.cache);
const kinds = Store.use((store) => Effect.all({ clickhouse: store.clickhouse, cache: store.cache }));
const template = <E>(build: (database: string) => Effect.Effect<unknown, E>) => Store.use((store) => store.templates.postgres('digest', ({ database }) => build(database)));
const analytics = <E>(build: Effect.Effect<unknown, E>) => Store.use((store) => store.templates.clickhouse('digest', () => build));

it.layer(Contract.layer, { buildPerTest: true })('store', (methods) => {
    methods.effect(
        'holds one session per layer, drops a store its scope leaves with success, and releases the session when the layer closes',
        Effect.fnUntraced(function* () {
            const exit = yield* Contract.sessioned(postgres);
            assert.deepStrictEqual(
                Exit.map(exit, ({ password, ...config }) => ({ ...config, password: Redacted.value(password) })),
                Exit.succeed({ host: '127.0.0.1', port: 7411, database: 'store1', username: 'store1', password: 'secret', maxConnections: 3 }),
            );
            assert.deepStrictEqual(yield* Contract.calls, ['HoldSession held', 'CreatePostgres store1', 'DropStore store1']);
        }),
    );
    methods.effect(
        'keeps a store its scope leaves with a failure',
        Effect.fnUntraced(function* () {
            const exit = yield* Contract.sessioned(Effect.andThen(bucket, Effect.die('assertion')));
            assert.deepStrictEqual([exit, yield* Contract.calls], [Exit.die('assertion'), ['HoldSession held', 'CreateBucket store1', 'KeepStore store1']]);
        }),
    );
    methods.effect(
        'fails a passing scope with the refusal when the service refuses the drop',
        Effect.fnUntraced(function* () {
            const { refuse, port } = yield* Contract;
            yield* refuse('DropStore', Code.FailedPrecondition);
            const exit = yield* Contract.sessioned(cache);
            const refusal = Option.filter(Exit.match(exit, { onSuccess: () => Option.none(), onFailure: flow(Cause.findDefect, Result.getSuccess) }), Schema.is(StoreError));
            assert.deepStrictEqual(
                Option.map(refusal, ({ reason, subject, endpoint }) => [reason, subject, endpoint]),
                Option.some([{ _tag: 'Leaked' }, 'DropStore', `http://127.0.0.1:${port}/`]),
            );
        }),
    );
    methods.effect(
        'returns the options each catalog driver takes for every kind',
        Effect.fnUntraced(function* () {
            assert.deepStrictEqual<Exit.Exit<unknown, unknown>>(
                yield* Contract.sessioned(kinds),
                Exit.succeed({
                    clickhouse: { url: 'http://127.0.0.1:7413', database: 'store1', username: 'store1', password: 'secret' },
                    cache: { channelPrefix: 'store2:', client: { socket: { host: '127.0.0.1', port: 7415 }, database: 3, username: 'store2', password: 'secret' } },
                }),
            );
        }),
    );
    methods.effect(
        'signs with the granted bucket credentials first and with renewed ones after',
        Effect.fnUntraced(function* () {
            const exit = yield* Contract.sessioned(
                Effect.gen(function* () {
                    const { bucket: name, client } = yield* bucket;
                    return [name, client.endpoint, client.region, yield* signatures(client.credentials)];
                }),
            );
            const signed = ['key', 'secret', 'token'];
            assert.deepStrictEqual<readonly unknown[]>([exit, yield* Contract.calls], [Exit.succeed(['store1', 'http://127.0.0.1:7412', 'us-east-1', [signed, signed]]), ['HoldSession held', 'CreateBucket store1', 'RenewBucket store1', 'DropStore store1']]);
        }),
    );
    methods.effect(
        'builds a template once per digest, seals it, and clones it for each caller',
        Effect.fnUntraced(function* () {
            const built = yield* Ref.make(Array.empty<string>());
            const cloning = template((database) => Ref.update(built, Array.append(database)));
            const exit = yield* Contract.sessioned(Effect.all([cloning, cloning]));
            assert.deepStrictEqual(
                [
                    Exit.map(
                        exit,
                        Array.map(({ database }) => database),
                    ),
                    yield* Ref.get(built),
                    yield* Contract.calls,
                ],
                [Exit.succeed(['store2', 'store3']), ['store1'], ['HoldSession held', 'ClonePostgresTemplate NotFound', 'OpenPostgresTemplate store1', 'SealTemplate store1', 'ClonePostgresTemplate store2', 'ClonePostgresTemplate store3', 'DropStore store3', 'DropStore store2']],
            );
        }),
    );
    methods.effect(
        'drops the build store of a failing builder, passes its failure on, and leaves the digest open to the next builder',
        Effect.fnUntraced(function* () {
            const failed = yield* Contract.sessioned(analytics(Migration.make({})));
            const exit = yield* Contract.sessioned(analytics(Effect.void));
            assert.deepStrictEqual(
                [failed, Exit.map(exit, ({ database }) => database), yield* Contract.calls],
                [
                    Exit.fail(Migration.make({})),
                    Exit.succeed('store3'),
                    ['HoldSession held', 'CloneClickhouseTemplate NotFound', 'OpenClickhouseTemplate store1', 'DropStore store1', 'HoldSession held', 'CloneClickhouseTemplate NotFound', 'OpenClickhouseTemplate store2', 'SealTemplate store2', 'CloneClickhouseTemplate store3', 'DropStore store3'],
                ],
            );
        }),
    );
    methods.effect(
        'opens its own build after the builder holding the digest abandons it',
        Effect.fnUntraced(function* () {
            const { refuse } = yield* Contract;
            yield* Effect.andThen(refuse('OpenPostgresTemplate', Code.AlreadyExists), refuse('ClonePostgresTemplate', Code.NotFound));
            const exit = yield* Contract.sessioned(template(() => Effect.void));
            assert.deepStrictEqual(
                [Exit.map(exit, ({ database }) => database), yield* Contract.calls],
                [Exit.succeed('store2'), ['HoldSession held', 'ClonePostgresTemplate NotFound', 'OpenPostgresTemplate AlreadyExists', 'ClonePostgresTemplate NotFound', 'ClonePostgresTemplate NotFound', 'OpenPostgresTemplate store1', 'SealTemplate store1', 'ClonePostgresTemplate store2', 'DropStore store2']],
            );
        }),
    );
});

it.effect('fails the layer naming the endpoint and the start command when no service answers', () =>
    Effect.exit(Effect.provide(Effect.void, Layer.provide(Store.layer(), ConfigProvider.layer(ConfigProvider.fromUnknown({ STORE_PORT: '1' }))))).pipe(
        Effect.map(Exit.match({ onSuccess: () => 'answered', onFailure: Cause.pretty })),
        Effect.map((reported) => assert.match(reported, silent)),
    ),
);
