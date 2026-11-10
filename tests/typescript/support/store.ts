import type { S3ClientConfig } from '@aws-sdk/client-s3';
import type { DescMessage, DescMethodUnary, Message, MessageInitShape } from '@bufbuild/protobuf';
import type { GenMessage } from '@bufbuild/protobuf/codegenv2';
import { Code, ConnectError } from '@connectrpc/connect';
import { createAsyncIterable } from '@connectrpc/connect/protocol';
import { createGrpcTransport, Http2SessionManager } from '@connectrpc/connect-node';
import type { ClickhouseClient } from '@effect/sql-clickhouse';
import type { PgClient } from '@effect/sql-pg';
import { Config, Console, Context, DateTime, Effect, Exit, Formatter, Function, Layer, Option, Redacted, Ref, Result, Schema, Sink, Stream } from 'effect';
import { SecureRandom } from 'effect/crypto';
import { NetAddress } from 'effect/net';
import type { RedisClientOptions } from 'redis';
import { type BucketStore, type CacheStore, type ClickhouseStore, type PostgresStore, StoreService, type TemplateRequestSchema } from '#/.buf/rasm/store/store_pb';

// --- [ERRORS] --------------------------------------------------------------------------

class StoreError extends Schema.TaggedError<StoreError, { readonly brand: unique symbol }>()('StoreError', {
    reason: Schema.TaggedUnion({ Silent: {}, Leaked: {}, Refused: { code: Schema.Enum(Code) } }),
    subject: Schema.String,
    endpoint: Schema.String,
    cause: Schema.Defect(),
}) {
    static readonly of = (subject: string, endpoint: URL) => (cause: unknown) => StoreError.make({ reason: { _tag: 'Refused', code: ConnectError.from(cause).code }, subject, endpoint: endpoint.href, cause });

    static readonly answered =
        (code: Code) =>
        (error: unknown): error is StoreError =>
            Schema.is(StoreError)(error) && StoreError.fields.reason.matchOrElse(error.reason, { Refused: (refused) => refused.code === code }, () => false);

    override get message() {
        return `${this.subject} at ${this.endpoint}: ${StoreError.fields.reason.match(this.reason, { Silent: () => 'silent', Leaked: () => 'a connection stays open', Refused: ({ code }) => Code[code] })}: ${Formatter.format(this.cause)}`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const postgres = ({ host, port, database, username, password, connectionLimit }: PostgresStore) => ({ host, port, database, username, password: Redacted.make(password), maxConnections: connectionLimit }) satisfies PgClient.PgPoolConfig;

const clickhouse = ({ host, httpPort, database, username, password }: ClickhouseStore) => ({ url: `http://${host}:${httpPort}`, database, username, password }) satisfies ClickhouseClient.ClickhouseClientConfig;

const cache = ({ host, port, database, username, password, channelPrefix }: CacheStore) => ({ channelPrefix, client: { socket: { host, port }, database, username, password } satisfies RedisClientOptions });

// --- [SERVICES] ------------------------------------------------------------------------

class Store extends Context.Service<Store>()('@rasm/test-support/store', {
    make: Effect.fnUntraced(function* (port: Config.Config<number> = Config.Port('STORE_PORT'), failed: Effect.Effect<boolean> = Effect.succeed(false)) {
        const endpoint = yield* Effect.fromResult(NetAddress.inetAddress(NetAddress.ipv4Loopback, yield* port).pipe(Result.flatMap(NetAddress.toUrl)));
        const sessionManager = yield* Effect.acquireRelease(
            Effect.sync(() => new Http2SessionManager(endpoint)),
            (connections) => Effect.sync(() => connections.abort()),
        );
        const transport = createGrpcTransport({ baseUrl: endpoint.href, sessionManager });
        const [session, held] = yield* Effect.all([SecureRandom.SecureRandom.use(({ uuidV7 }) => uuidV7()), Effect.abortSignal]);
        const silent = (cause: unknown) => StoreError.make({ reason: { _tag: 'Silent' }, subject: `${StoreService.method.holdSession.name} ${session}`, endpoint: endpoint.href, cause });
        const call = <I extends DescMessage, O extends DescMessage>(method: DescMethodUnary<I, O>, input: MessageInitShape<I>) => Effect.tryPromise({ try: (signal) => transport.unary(method, signal, undefined, undefined, input), catch: StoreError.of(method.name, endpoint) }).pipe(Effect.map(({ message }) => message));
        const kept = <S extends Message & { readonly id: string }>(store: S) => Effect.andThen(call(StoreService.method.keepStore, { id: store.id }), Effect.provideService(Effect.logWarning(StoreService.method.keepStore.name, store), Console.Console, console));
        const dropped = (id: string) => Effect.mapError(call(StoreService.method.dropStore, { id }), (refusal) => (StoreError.answered(Code.FailedPrecondition)(refusal) ? StoreError.make({ reason: { _tag: 'Leaked' }, subject: refusal.subject, endpoint: refusal.endpoint, cause: refusal.cause }) : refusal));
        const issued = <S extends Message & { readonly id: string }, A>(created: Effect.Effect<S, StoreError>, ready: (store: S) => A) =>
            Effect.acquireRelease(created, (store, exit) => Effect.orDie(Effect.flatMap(failed, (lost) => (Exit.isSuccess(exit) && !lost ? Effect.asVoid(dropped(store.id)) : kept(store))))).pipe(Effect.map(ready));
        const template =
            <S extends Message & { readonly id: string }, A>(open: DescMethodUnary<typeof TemplateRequestSchema, GenMessage<S>>, clone: DescMethodUnary<typeof TemplateRequestSchema, GenMessage<S>>, ready: (store: S) => A) =>
            <E, R>(digest: string, build: (store: A) => Effect.Effect<unknown, E, R>) => {
                const cloned = issued(call(clone, { session, digest }), ready);
                const built = Effect.gen(function* () {
                    const store = yield* call(open, { session, digest });
                    yield* build(ready(store)).pipe(
                        Effect.andThen(call(StoreService.method.sealTemplate, { id: store.id })),
                        Effect.onError(() => Effect.orDie(call(StoreService.method.dropStore, { id: store.id }))),
                    );
                }).pipe(
                    Effect.catchIf(StoreError.answered(Code.AlreadyExists), () => Effect.void),
                    Effect.andThen(cloned),
                );
                return cloned.pipe(
                    Effect.catchIf(StoreError.answered(Code.NotFound), () => built),
                    Effect.retry({ while: StoreError.answered(Code.NotFound) }),
                );
            };
        const credentials = Effect.fnUntraced(function* (granted: BucketStore) {
            const first = yield* Ref.make(Option.some(granted));
            const current = Effect.flatMap(Ref.getAndSet(first, Option.none()), Option.match({ onNone: () => call(StoreService.method.renewBucket, { id: granted.id }), onSome: Effect.succeed }));
            const signing = Effect.map(Effect.all([current, DateTime.now]), ([{ accessKeyId, secretAccessKey, sessionToken, credentialsTtlSeconds }, now]) => ({ accessKeyId, secretAccessKey, sessionToken, expiration: DateTime.toDate(DateTime.add(now, { seconds: credentialsTtlSeconds })) }));
            return { bucket: granted.bucket, client: { endpoint: granted.endpoint, region: granted.region, forcePathStyle: true, credentials: () => Effect.runPromise(signing) } satisfies S3ClientConfig };
        });
        const leases = yield* Effect.tryPromise({ try: () => transport.stream(StoreService.method.holdSession, held, undefined, undefined, createAsyncIterable([{ session }])), catch: silent });
        const [lease, renewed] = yield* Stream.peel(Stream.fromAsyncIterable(leases.message, silent), Sink.head());
        yield* Effect.fromOption(lease, () => silent(undefined));
        yield* Effect.forkScoped(Effect.tapCause(Stream.runDrain(renewed), Effect.logError));
        return {
            postgres: issued(call(StoreService.method.createPostgres, { session }), postgres),
            bucket: Effect.flatMap(issued(call(StoreService.method.createBucket, { session }), Function.identity), credentials),
            clickhouse: issued(call(StoreService.method.createClickhouse, { session }), clickhouse),
            cache: issued(call(StoreService.method.createCache, { session }), cache),
            templates: {
                postgres: template(StoreService.method.openPostgresTemplate, StoreService.method.clonePostgresTemplate, postgres),
                clickhouse: template(StoreService.method.openClickhouseTemplate, StoreService.method.cloneClickhouseTemplate, clickhouse),
            },
        };
    }),
}) {
    static readonly layer = (port?: Config.Config<number>, failed?: Effect.Effect<boolean>) => Layer.effect(this, this.make(port, failed)).pipe(Layer.provide(SecureRandom.layerWebCrypto));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Store, StoreError };
