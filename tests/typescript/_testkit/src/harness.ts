import { CreateBucketCommand, DeleteObjectCommand, GetObjectCommand, ListObjectsV2Command, PutObjectCommand, S3Client } from '@aws-sdk/client-s3';
import { getSignedUrl } from '@aws-sdk/s3-request-presigner';
import type { HttpApp } from '@effect/platform';
import { HttpClient, HttpServer } from '@effect/platform';
import { NodeHttpServer } from '@effect/platform-node';
import type { SqlError } from '@effect/sql/SqlError';
import { PgClient } from '@effect/sql-pg';
import { type Extensions, PGlite } from '@electric-sql/pglite';
import {
    Array,
    Context,
    Data,
    Deferred,
    Effect,
    HashMap,
    Layer,
    Mailbox,
    Option,
    Order,
    type ParseResult,
    pipe,
    Redacted,
    Ref,
    Schema,
    type Scope,
    Stream,
} from 'effect';
import { GenericContainer, Network, type StartedNetwork, type StartedTestContainer, Wait, type WaitStrategy } from 'testcontainers';

// --- [TYPES] -----------------------------------------------------------------------------

declare namespace Containers {
    type Exec = { readonly exitCode: number; readonly output: string; readonly stderr: string; readonly stdout: string };
    // A lane is the SET of row fields it declares on the one builder; a new capability is a field, never a second mechanism.
    type Row = {
        readonly image: string;
        readonly ports: Array.NonEmptyReadonlyArray<number>;
        readonly environment: Record<string, string>;
        readonly command?: ReadonlyArray<string>;
        readonly copy?: ReadonlyArray<{ readonly content: string; readonly target: string }>;
        readonly labels?: Record<string, string>;
        readonly net?: { readonly network: StartedNetwork; readonly aliases: Array.NonEmptyReadonlyArray<string> };
        readonly resources?: { readonly cpu?: number; readonly memory?: number };
        readonly ready: WaitStrategy;
        readonly startupMs: number;
    };
}

declare namespace PgLane {
    type Service = {
        readonly exec: (statements: string) => Effect.Effect<void, HarnessFault>;
        readonly rows: (statement: string, params?: ReadonlyArray<unknown>) => Effect.Effect<ReadonlyArray<unknown>, HarnessFault>;
        readonly decoded: <A, I>(
            schema: Schema.Schema<A, I, never>,
        ) => (statement: string, params?: ReadonlyArray<unknown>) => Effect.Effect<ReadonlyArray<A>, HarnessFault | ParseResult.ParseError>;
        readonly listen: (channel: string) => Effect.Effect<Mailbox.ReadonlyMailbox<string, HarnessFault>, HarnessFault, Scope.Scope>;
    };
    type ContainerOptions = {
        readonly image: string;
        readonly database?: string;
        readonly username?: string;
        readonly password?: string;
        readonly seed?: string;
    };
    type PgliteOptions = { readonly seed?: string; readonly extensions?: Extensions };
}

declare namespace ObjectStore {
    type Service = {
        readonly put: (key: string, bytes: Uint8Array) => Effect.Effect<void, HarnessFault>;
        readonly get: (key: string) => Effect.Effect<Option.Option<Uint8Array>, HarnessFault>;
        readonly list: (prefix: string) => Effect.Effect<ReadonlyArray<string>, HarnessFault>;
        readonly remove: (key: string) => Effect.Effect<void, HarnessFault>;
        readonly url: (key: string, ttlSeconds: number) => Effect.Effect<string, HarnessFault>;
    };
    type S3Options = {
        readonly endpoint: string;
        readonly bucket: string;
        readonly accessKeyId: string;
        readonly secretAccessKey: string;
        readonly region?: string;
    };
}

// --- [CONSTANTS] -------------------------------------------------------------------------

const _PG = { port: 5432, database: 'rasm', username: 'rasm', password: 'rasm', startupMs: 120_000 } as const;
const _STORE = { port: 9000, health: '/minio/health/live', region: 'us-east-1', startupMs: 120_000 } as const;

// The container listen arm barrier: real setTimeout pacing (TestClock-immune), bounded turns before a typed engine fault.
// The prefix marks kit-internal control frames — every listener on a channel drops foreign arm traffic, never delivers it.
const _ARM = { pauseMs: 25, turns: 120, prefix: '<rasm-testkit-armed:' } as const;

// --- [ERRORS] ----------------------------------------------------------------------------

class HarnessFault extends Data.TaggedError('HarnessFault')<{
    readonly lane: 'container' | 'loopback' | 'pg' | 'store';
    readonly reason: 'engine' | 'unsupported';
    readonly code: Option.Option<string>;
    readonly detail: string;
}> {
    // Foreign engines route by their own error NAME, carried typed — never re-derived from message substrings.
    static readonly engine =
        (lane: HarnessFault['lane']) =>
        (defect: unknown): HarnessFault =>
            new HarnessFault({
                lane,
                reason: 'engine',
                code: defect instanceof Error ? Option.some(defect.name) : Option.none(),
                detail: String(defect),
            });
}

// --- [SERVICES] --------------------------------------------------------------------------

class Loopback extends Context.Tag('rasm-testkit/Loopback')<
    Loopback,
    {
        readonly url: string;
        readonly client: HttpClient.HttpClient;
    }
>() {}

class ObjectStore extends Context.Tag('rasm-testkit/ObjectStore')<ObjectStore, ObjectStore.Service>() {}

class PgLane extends Context.Tag('rasm-testkit/PgLane')<PgLane, PgLane.Service>() {}

// --- [OPERATIONS] ------------------------------------------------------------------------

const _guarded = <A>(lane: HarnessFault['lane'], run: () => Promise<A>): Effect.Effect<A, HarnessFault> =>
    Effect.tryPromise({ try: run, catch: HarnessFault.engine(lane) });

const _breath = Effect.promise<void>(() => new Promise((wake) => setTimeout(wake, _ARM.pauseMs)));

const _lane = (exec: PgLane.Service['exec'], rows: PgLane.Service['rows'], listen: PgLane.Service['listen']): PgLane.Service => ({
    exec,
    rows,
    decoded: (schema) => {
        const decode = Schema.decodeUnknown(Schema.Array(schema));
        return (statement, params) => Effect.flatMap(rows(statement, params), decode);
    },
    listen,
});

// Container rows are DATA on the one builder: a new lane is a row, never a new mechanism. Images are caller-owned — the polyglot pin lives in tests/containers.json, never in the kit.
const Containers = {
    pg: (options: PgLane.ContainerOptions): Containers.Row => ({
        image: options.image,
        ports: [_PG.port],
        environment: {
            POSTGRES_DB: options.database ?? _PG.database,
            POSTGRES_USER: options.username ?? _PG.username,
            POSTGRES_PASSWORD: options.password ?? _PG.password,
        },
        ready: Wait.forLogMessage(/database system is ready to accept connections/, 2),
        startupMs: _PG.startupMs,
    }),
    objectStore: (options: { readonly image: string; readonly rootUser: string; readonly rootPassword: string }): Containers.Row => ({
        image: options.image,
        ports: [_STORE.port],
        environment: { MINIO_ROOT_USER: options.rootUser, MINIO_ROOT_PASSWORD: options.rootPassword },
        command: ['server', '/data'],
        ready: Wait.forHttp(_STORE.health, _STORE.port).forStatusCode(200),
        startupMs: _STORE.startupMs,
    }),
    // In-container command receipt: exit code, streams, and interleaved output — the caller owns the verdict.
    exec: (started: StartedTestContainer, command: Array.NonEmptyReadonlyArray<string>): Effect.Effect<Containers.Exec, HarnessFault> =>
        Effect.map(
            _guarded('container', () => started.exec([...command])),
            (receipt) => ({ exitCode: receipt.exitCode, output: receipt.output, stderr: receipt.stderr, stdout: receipt.stdout }),
        ),
    // Scoped network acquisition: rows join it through their `net` field; teardown rides the scope.
    network: Effect.acquireRelease(
        _guarded('container', () => new Network().start()),
        (started) => Effect.ignore(Effect.promise(() => started.stop())),
    ),
    start: (row: Containers.Row): Effect.Effect<StartedTestContainer, HarnessFault, Scope.Scope> =>
        Effect.acquireRelease(
            _guarded('container', () =>
                pipe(
                    new GenericContainer(row.image)
                        .withExposedPorts(...row.ports)
                        .withEnvironment(row.environment)
                        .withWaitStrategy(row.ready)
                        .withStartupTimeout(row.startupMs),
                    (built) => (row.command === undefined ? built : built.withCommand([...row.command])),
                    (built) => (row.copy === undefined ? built : built.withCopyContentToContainer([...row.copy])),
                    (built) => (row.labels === undefined ? built : built.withLabels(row.labels)),
                    (built) => (row.net === undefined ? built : built.withNetwork(row.net.network).withNetworkAliases(...row.net.aliases)),
                    (built) => (row.resources === undefined ? built : built.withResourcesQuota(row.resources)),
                ).start(),
            ),
            // Release never masks the test verdict; Ryuk reaps anything a failed stop strands.
            (started) => Effect.ignore(Effect.promise(() => started.stop())),
        ),
} as const;

const _pgliteListen =
    (db: PGlite): PgLane.Service['listen'] =>
    (channel) =>
        Effect.gen(function* () {
            const box = yield* Mailbox.make<string, HarnessFault>();
            // The acquire IS the registration barrier: once listen resolves, every later NOTIFY is delivered.
            // Arm-prefixed control frames drop here too — both lanes serve one listen algebra.
            yield* Effect.acquireRelease(
                _guarded('pg', () => db.listen(channel, (payload) => void (payload.startsWith(_ARM.prefix) || box.unsafeOffer(payload)))),
                (dispose) => Effect.ignore(Effect.promise(() => dispose())),
            );
            return box;
        });

const _pgClientListen =
    (pg: PgClient.PgClient): PgLane.Service['listen'] =>
    (channel) =>
        Effect.gen(function* () {
            const box = yield* Mailbox.make<string, HarnessFault>();
            const armed = yield* Deferred.make<void>();
            const sentinel = `${_ARM.prefix}${crypto.randomUUID()}>`;
            yield* pipe(
                pg.listen(channel),
                Stream.mapError(HarnessFault.engine('pg')),
                Stream.runForEach((payload) =>
                    payload === sentinel
                        ? Effect.asVoid(Deferred.succeed(armed, void 0))
                        : payload.startsWith(_ARM.prefix)
                          ? Effect.void
                          : Effect.asVoid(box.offer(payload)),
                ),
                Effect.catchAll((fault) => Effect.asVoid(box.fail(fault))),
                Effect.forkScoped,
            );
            // Arm barrier: the driver subscribes asynchronously, so a sentinel self-notify proves the subscription live before
            // returning. The notify rides pg_notify() — NOTIFY is a utility statement and rejects bind parameters outright.
            yield* Effect.iterate(0, {
                while: (turn) => turn >= 0 && turn < _ARM.turns,
                body: (turn) =>
                    pipe(
                        Effect.mapError(pg.unsafe('SELECT pg_notify($1, $2)', [channel, sentinel]), HarnessFault.engine('pg')),
                        Effect.zipRight(_breath),
                        Effect.zipRight(Deferred.isDone(armed)),
                        Effect.map((live) => (live ? -1 : turn + 1)),
                    ),
            }).pipe(
                Effect.filterOrFail(
                    (turn) => turn === -1,
                    () => new HarnessFault({ lane: 'pg', reason: 'engine', code: Option.none(), detail: `listen never armed on ${channel}` }),
                ),
            );
            return box;
        });

const _pgFromClient = (seed?: string): Layer.Layer<PgLane, HarnessFault, PgClient.PgClient> =>
    Layer.effect(
        PgLane,
        Effect.gen(function* () {
            const pg = yield* PgClient.PgClient;
            const fault = (raw: SqlError): HarnessFault =>
                new HarnessFault({ lane: 'pg', reason: 'engine', code: Option.some(raw._tag), detail: raw.message });
            const lane = _lane(
                (statements) => Effect.asVoid(Effect.mapError(pg.unsafe(statements), fault)),
                (statement, params) => Effect.mapError(pg.unsafe(statement, params === undefined ? undefined : [...params]), fault),
                _pgClientListen(pg),
            );
            yield* seed === undefined ? Effect.void : lane.exec(seed);
            return lane;
        }),
    );

const PgLanes = {
    // The fast unit lane: one in-process WASM postgres, seeded once at acquire, discarded with the scope. One entry owns
    // both modalities: a bare string is the seed; the options row adds caller-owned extension modules — the pglite twin
    // of the caller-owned container image pin.
    pglite: (options?: string | PgLane.PgliteOptions): Layer.Layer<PgLane, HarnessFault> => {
        const lane: PgLane.PgliteOptions = typeof options === 'string' ? { seed: options } : (options ?? {});
        return Layer.scoped(
            PgLane,
            Effect.gen(function* () {
                const db = yield* Effect.acquireRelease(
                    _guarded('pg', () =>
                        PGlite.create({ relaxedDurability: true, ...(lane.extensions === undefined ? {} : { extensions: lane.extensions }) }),
                    ),
                    (live) => Effect.promise(() => live.close()),
                );
                const seed = lane.seed;
                yield* seed === undefined ? Effect.void : _guarded('pg', () => db.exec(seed));
                return _lane(
                    (statements) => Effect.asVoid(_guarded('pg', () => db.exec(statements))),
                    (statement, params) =>
                        Effect.map(
                            _guarded('pg', () => db.query(statement, params === undefined ? undefined : [...params])),
                            (result) => result.rows as ReadonlyArray<unknown>,
                        ),
                    _pgliteListen(db),
                );
            }),
        );
    },
    // The real-server lane: the pg container row bound through the real driver; server-extension DDL seeds via raw execute.
    container: (options: PgLane.ContainerOptions): Layer.Layer<PgLane, HarnessFault> =>
        Layer.provide(
            _pgFromClient(options.seed),
            Layer.unwrapScoped(
                Effect.map(Containers.start(Containers.pg(options)), (started) =>
                    Layer.mapError(
                        PgClient.layer({
                            host: started.getHost(),
                            port: started.getMappedPort(_PG.port),
                            database: options.database ?? _PG.database,
                            username: options.username ?? _PG.username,
                            password: Redacted.make(options.password ?? _PG.password),
                        }),
                        HarnessFault.engine('pg'),
                    ),
                ),
            ),
        ),
} as const;

const ObjectStores = {
    // The in-process double: the same filesystem algebra over one keyed cell; presign is a real-store capability and refuses typed here.
    memory: Layer.effect(
        ObjectStore,
        Effect.map(Ref.make(HashMap.empty<string, Uint8Array>()), (cell) => ({
            put: (key, bytes) => Ref.update(cell, HashMap.set(key, bytes)),
            get: (key) => Effect.map(Ref.get(cell), HashMap.get(key)),
            list: (prefix) =>
                Effect.map(Ref.get(cell), (held) =>
                    // Lexicographic order mirrors the real S3 listing contract, so the double and the live lane agree.
                    Array.sort(
                        Array.filter(Array.fromIterable(HashMap.keys(held)), (key) => key.startsWith(prefix)),
                        Order.string,
                    ),
                ),
            remove: (key) => Ref.update(cell, HashMap.remove(key)),
            url: () =>
                Effect.fail(new HarnessFault({ lane: 'store', reason: 'unsupported', code: Option.none(), detail: 'presign requires the s3 lane' })),
        })),
    ),
    // The real lane: an S3-compatible endpoint (container row or external), path-style, bucket ensured at acquire.
    s3: (options: ObjectStore.S3Options): Layer.Layer<ObjectStore, HarnessFault> =>
        Layer.scoped(
            ObjectStore,
            Effect.gen(function* () {
                const client = yield* Effect.acquireRelease(
                    Effect.sync(
                        () =>
                            new S3Client({
                                endpoint: options.endpoint,
                                region: options.region ?? _STORE.region,
                                forcePathStyle: true,
                                credentials: { accessKeyId: options.accessKeyId, secretAccessKey: options.secretAccessKey },
                            }),
                    ),
                    (live) => Effect.sync(() => live.destroy()),
                );
                yield* Effect.catchIf(
                    _guarded('store', () => client.send(new CreateBucketCommand({ Bucket: options.bucket }))),
                    (fault) => Option.exists(fault.code, (name) => name.startsWith('BucketAlready')),
                    () => Effect.void,
                );
                return {
                    put: (key, bytes) =>
                        Effect.asVoid(_guarded('store', () => client.send(new PutObjectCommand({ Bucket: options.bucket, Key: key, Body: bytes })))),
                    get: (key) =>
                        _guarded('store', () => client.send(new GetObjectCommand({ Bucket: options.bucket, Key: key }))).pipe(
                            Effect.flatMap((reply) =>
                                Option.match(Option.fromNullable(reply.Body), {
                                    onNone: () => Effect.succeed(Option.none<Uint8Array>()),
                                    onSome: (body) =>
                                        Effect.map(
                                            _guarded('store', () => body.transformToByteArray()),
                                            Option.some,
                                        ),
                                }),
                            ),
                            Effect.catchIf(
                                (fault) => Option.contains(fault.code, 'NoSuchKey'),
                                () => Effect.succeed(Option.none<Uint8Array>()),
                            ),
                        ),
                    // Continuation-token pagination: the listing is total over the prefix, never the first truncated page.
                    list: (prefix) =>
                        Effect.map(
                            Effect.iterate(
                                { keys: [] as ReadonlyArray<string>, token: Option.none<string>(), open: true },
                                {
                                    while: (state) => state.open,
                                    body: (state) =>
                                        Effect.map(
                                            _guarded('store', () =>
                                                client.send(
                                                    new ListObjectsV2Command({
                                                        Bucket: options.bucket,
                                                        Prefix: prefix,
                                                        ContinuationToken: Option.getOrUndefined(state.token),
                                                    }),
                                                ),
                                            ),
                                            (reply) => ({
                                                keys: Array.appendAll(
                                                    state.keys,
                                                    Array.filterMap(reply.Contents ?? [], (entry) => Option.fromNullable(entry.Key)),
                                                ),
                                                token: Option.fromNullable(reply.NextContinuationToken),
                                                open: reply.IsTruncated === true,
                                            }),
                                        ),
                                },
                            ),
                            (state) => state.keys,
                        ),
                    remove: (key) =>
                        Effect.asVoid(_guarded('store', () => client.send(new DeleteObjectCommand({ Bucket: options.bucket, Key: key })))),
                    url: (key, ttlSeconds) =>
                        _guarded('store', () =>
                            getSignedUrl(client, new GetObjectCommand({ Bucket: options.bucket, Key: key }), { expiresIn: ttlSeconds }),
                        ),
                };
            }),
        ),
} as const;

// The loopback capsule: one in-process socket server serving `app`, yielding its endpoint and a base-wired client (relative-path requests).
const _loopbackValue: Effect.Effect<Context.Tag.Service<Loopback>, never, HttpClient.HttpClient | HttpServer.HttpServer> = Effect.gen(function* () {
    const client = yield* HttpClient.HttpClient;
    const url = yield* HttpServer.addressFormattedWith(Effect.succeed);
    return { url, client };
});

const Loopbacks = {
    serve: <E>(app: HttpApp.Default<E>): Layer.Layer<Loopback> => {
        const base = NodeHttpServer.layerTest;
        return Layer.mergeAll(Layer.effect(Loopback, _loopbackValue), HttpServer.serve(app)).pipe(Layer.provide(base), Layer.orDie);
    },
} as const;

// --- [EXPORTS] ---------------------------------------------------------------------------

export { Containers, HarnessFault, Loopback, Loopbacks, ObjectStore, ObjectStores, PgLane, PgLanes };
