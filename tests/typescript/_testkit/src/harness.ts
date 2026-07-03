import { CreateBucketCommand, DeleteObjectCommand, GetObjectCommand, ListObjectsV2Command, PutObjectCommand, S3Client } from '@aws-sdk/client-s3';
import { getSignedUrl } from '@aws-sdk/s3-request-presigner';
import type { HttpApp } from '@effect/platform';
import { HttpClient, HttpServer } from '@effect/platform';
import { NodeHttpServer } from '@effect/platform-node';
import { SqlClient } from '@effect/sql';
import { PgClient } from '@effect/sql-pg';
import { PGlite } from '@electric-sql/pglite';
import { Array, Context, Data, Effect, HashMap, Layer, Option, type ParseResult, Redacted, Ref, Schema, type Scope } from 'effect';
import { GenericContainer, type StartedTestContainer, Wait, type WaitStrategy } from 'testcontainers';

// --- [TYPES] -----------------------------------------------------------------------------

declare namespace Containers {
    type Row = {
        readonly image: string;
        readonly ports: Array.NonEmptyReadonlyArray<number>;
        readonly environment: Record<string, string>;
        readonly command?: ReadonlyArray<string>;
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
    };
    type ContainerOptions = {
        readonly image: string;
        readonly database?: string;
        readonly username?: string;
        readonly password?: string;
    };
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

// --- [ERRORS] ----------------------------------------------------------------------------

class HarnessFault extends Data.TaggedError('HarnessFault')<{
    readonly lane: 'container' | 'loopback' | 'pg' | 'store';
    readonly reason: 'engine' | 'unsupported';
    readonly detail: string;
}> {}

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
    Effect.tryPromise({ try: run, catch: (defect) => new HarnessFault({ lane, reason: 'engine', detail: String(defect) }) });

const _lane = (exec: PgLane.Service['exec'], rows: PgLane.Service['rows']): PgLane.Service => ({
    exec,
    rows,
    decoded: (schema) => {
        const decode = Schema.decodeUnknown(Schema.Array(schema));
        return (statement, params) => Effect.flatMap(rows(statement, params), decode);
    },
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
    start: (row: Containers.Row): Effect.Effect<StartedTestContainer, HarnessFault, Scope.Scope> =>
        Effect.acquireRelease(
            _guarded('container', () => {
                const built = new GenericContainer(row.image)
                    .withExposedPorts(...row.ports)
                    .withEnvironment(row.environment)
                    .withWaitStrategy(row.ready)
                    .withStartupTimeout(row.startupMs);
                return (row.command === undefined ? built : built.withCommand([...row.command])).start();
            }),
            (started) => Effect.asVoid(Effect.promise(() => started.stop())),
        ),
} as const;

const _pgFromSql: Layer.Layer<PgLane, never, SqlClient.SqlClient> = Layer.effect(
    PgLane,
    Effect.map(SqlClient.SqlClient, (sql) =>
        _lane(
            (statements) =>
                Effect.asVoid(
                    Effect.mapError(sql.unsafe(statements), (fault) => new HarnessFault({ lane: 'pg', reason: 'engine', detail: fault.message })),
                ),
            (statement, params) =>
                Effect.mapError(
                    sql.unsafe(statement, params === undefined ? undefined : [...params]),
                    (fault) => new HarnessFault({ lane: 'pg', reason: 'engine', detail: fault.message }),
                ),
        ),
    ),
);

const PgLanes = {
    // The fast unit lane: one in-process WASM postgres, seeded once at acquire, discarded with the scope.
    pglite: (seed?: string): Layer.Layer<PgLane> =>
        Layer.scoped(
            PgLane,
            Effect.gen(function* () {
                const db = yield* Effect.acquireRelease(
                    Effect.promise(() => PGlite.create({ relaxedDurability: true })),
                    (live) => Effect.promise(() => live.close()),
                );
                yield* seed === undefined ? Effect.void : Effect.orDie(_guarded('pg', () => db.exec(seed)));
                return _lane(
                    (statements) => Effect.asVoid(_guarded('pg', () => db.exec(statements))),
                    (statement, params) =>
                        Effect.map(
                            _guarded('pg', () => db.query(statement, params === undefined ? undefined : [...params])),
                            (result) => result.rows as ReadonlyArray<unknown>,
                        ),
                );
            }),
        ),
    // The real-server lane: the pg container row bound through the real driver; server-extension DDL seeds via raw execute.
    container: (options: PgLane.ContainerOptions): Layer.Layer<PgLane, HarnessFault> =>
        Layer.provide(
            _pgFromSql,
            Layer.unwrapScoped(
                Effect.map(Containers.start(Containers.pg(options)), (started) =>
                    Layer.orDie(
                        PgClient.layer({
                            host: started.getHost(),
                            port: started.getMappedPort(_PG.port),
                            database: options.database ?? _PG.database,
                            username: options.username ?? _PG.username,
                            password: Redacted.make(options.password ?? _PG.password),
                        }),
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
                Effect.map(Ref.get(cell), (held) => Array.filter(Array.fromIterable(HashMap.keys(held)), (key) => key.startsWith(prefix))),
            remove: (key) => Ref.update(cell, HashMap.remove(key)),
            url: () => Effect.fail(new HarnessFault({ lane: 'store', reason: 'unsupported', detail: 'presign requires the s3 lane' })),
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
                    (fault) => fault.detail.includes('BucketAlready'),
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
                                (fault) => fault.detail.includes('NoSuchKey'),
                                () => Effect.succeed(Option.none<Uint8Array>()),
                            ),
                        ),
                    list: (prefix) =>
                        Effect.map(
                            _guarded('store', () => client.send(new ListObjectsV2Command({ Bucket: options.bucket, Prefix: prefix }))),
                            (reply) => Array.filterMap(reply.Contents ?? [], (entry) => Option.fromNullable(entry.Key)),
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
