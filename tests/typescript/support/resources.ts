import { type HttpApp, HttpClient, HttpServer } from '@effect/platform';
import { NodeHttpServer } from '@effect/platform-node';
import { type Extensions, PGlite } from '@electric-sql/pglite';
import {
    Array,
    Context,
    Data,
    Effect,
    HashMap,
    Layer,
    Mailbox,
    Match,
    Option,
    Order,
    type ParseResult,
    Predicate,
    pipe,
    Ref,
    Schema,
    type Scope,
    Struct,
} from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface TestDatabaseService {
    readonly exec: (statements: string) => Effect.Effect<void, TestResourceError>;
    readonly rows: (statement: string, params?: readonly unknown[]) => Effect.Effect<readonly unknown[], TestResourceError>;
    readonly decoded: <A, I>(
        schema: Schema.Schema<A, I, never>,
    ) => (statement: string, params?: readonly unknown[]) => Effect.Effect<readonly A[], TestResourceError | ParseResult.ParseError>;
    readonly listen: (channel: string) => Effect.Effect<Mailbox.ReadonlyMailbox<string, TestResourceError>, TestResourceError, Scope.Scope>;
    readonly rollbackTransaction: <A, E, R>(work: Effect.Effect<A, E, R>) => Effect.Effect<A, E | TestResourceError, R>;
}

interface PgliteOptions {
    readonly seed?: string;
    readonly extensions?: Extensions;
}

interface ObjectStoreService {
    readonly put: (key: string, bytes: Uint8Array) => Effect.Effect<void, TestResourceError>;
    readonly get: (key: string) => Effect.Effect<Option.Option<Uint8Array>, TestResourceError>;
    readonly list: (prefix: string) => Effect.Effect<readonly string[], TestResourceError>;
    readonly remove: (key: string) => Effect.Effect<void, TestResourceError>;
    readonly url: (key: string, ttlSeconds: number) => Effect.Effect<string, TestResourceError>;
}

interface LoopbackService {
    readonly url: string;
    readonly client: HttpClient.HttpClient;
}

interface ObjectStoreDoubles {
    readonly memory: Layer.Layer<ObjectStoreService>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _CONTROL_NOTIFICATION_PREFIX = '<test-support-control:';
const _utf8 = new TextEncoder();

// --- [ERRORS] --------------------------------------------------------------------------

class TestResourceError extends Data.Error<{
    readonly resource: 'loopback' | 'database' | 'object-store';
    readonly reason: 'operation' | 'unsupported';
    readonly code: Option.Option<string>;
    readonly detail: string;
}> {
    readonly _tag = 'TestResourceError' as const;
}

// --- [SERVICES] ------------------------------------------------------------------------

const Loopback: Context.Tag<LoopbackService, LoopbackService> = Context.GenericTag<LoopbackService>('test-support/Loopback');
const ObjectStore: Context.Tag<ObjectStoreService, ObjectStoreService> = Context.GenericTag<ObjectStoreService>('test-support/ObjectStore');
const TestDatabase: Context.Tag<TestDatabaseService, TestDatabaseService> = Context.GenericTag<TestDatabaseService>('test-support/TestDatabase');

// --- [OPERATIONS] ----------------------------------------------------------------------

const _database = <A>(run: () => Promise<A>): Effect.Effect<A, TestResourceError> =>
    Effect.tryPromise({
        try: run,
        catch: (cause) =>
            new TestResourceError({
                resource: 'database',
                reason: 'operation',
                code: Option.map(Option.liftPredicate(cause, Predicate.isError), (error) => error.name),
                detail: String(cause),
            }),
    });

// S3 lists keys in UTF-8 byte order, which differs from UTF-16 code unit order past the basic multilingual plane
const _byUtf8Bytes: Order.Order<string> = Order.mapInput(Order.array(Order.number), (key: string) => Array.fromIterable(_utf8.encode(key)));

const _pglite = (configuration: PgliteOptions): Effect.Effect<TestDatabaseService, TestResourceError, Scope.Scope> =>
    Effect.gen(function* () {
        const db = yield* Effect.acquireRelease(
            _database(() => PGlite.create({ relaxedDurability: true, ...Struct.pick(configuration, 'extensions') })),
            (live) => Effect.promise(() => live.close()),
        );
        yield* Effect.forEach(Array.fromNullable(configuration.seed), (seed) => _database(() => db.exec(seed)));
        const exec: TestDatabaseService['exec'] = (statements) => Effect.asVoid(_database(() => db.exec(statements)));
        const rows: TestDatabaseService['rows'] = (statement, params = []) =>
            Effect.map(
                _database(() => db.query(statement, Array.fromIterable(params))),
                (result) => result.rows,
            );
        return {
            exec,
            rows,
            decoded: (schema) => {
                const decode = Schema.decodeUnknown(Schema.Array(schema));
                return (statement, params) => Effect.flatMap(rows(statement, params), decode);
            },
            listen: (channel) =>
                Effect.gen(function* () {
                    const mailbox = yield* Mailbox.make<string, TestResourceError>();
                    yield* Effect.acquireRelease(
                        _database(() =>
                            db.listen(channel, (payload) => {
                                if (!payload.startsWith(_CONTROL_NOTIFICATION_PREFIX)) {
                                    mailbox.unsafeOffer(payload);
                                }
                            }),
                        ),
                        (dispose) => Effect.ignore(Effect.promise(() => dispose())),
                    );
                    return mailbox;
                }),
            rollbackTransaction: (work) =>
                Effect.acquireUseRelease(
                    exec('BEGIN'),
                    () => work,
                    () => Effect.orDie(exec('ROLLBACK')),
                ),
        };
    });

const TestDatabases = {
    pglite: (options?: string | PgliteOptions): Layer.Layer<TestDatabaseService, TestResourceError> =>
        Layer.scoped(
            TestDatabase,
            _pglite(
                Match.value(options).pipe(
                    Match.when(Match.string, (seed) => ({ seed })),
                    Match.orElse((configuration) => configuration ?? {}),
                ),
            ),
        ),
} as const;

const ObjectStoreDoubles: ObjectStoreDoubles = {
    memory: Layer.effect(
        ObjectStore,
        Effect.map(Ref.make(HashMap.empty<string, Uint8Array>()), (cell) => ({
            put: (key, bytes) => Ref.update(cell, HashMap.set(key, bytes)),
            get: (key) => Effect.map(Ref.get(cell), HashMap.get(key)),
            list: (prefix) =>
                Effect.map(Ref.get(cell), (held) =>
                    pipe(
                        HashMap.keys(held),
                        Array.filter((key) => key.startsWith(prefix)),
                        Array.sort(_byUtf8Bytes),
                    ),
                ),
            remove: (key) => Ref.update(cell, HashMap.remove(key)),
            url: () =>
                Effect.fail(
                    new TestResourceError({
                        resource: 'object-store',
                        reason: 'unsupported',
                        code: Option.none(),
                        detail: 'presign requires a real object store',
                    }),
                ),
        })),
    ),
};

const _loopback: Effect.Effect<LoopbackService, never, HttpClient.HttpClient | HttpServer.HttpServer> = Effect.all({
    url: HttpServer.addressFormattedWith(Effect.succeed),
    client: HttpClient.HttpClient,
});

const LoopbackServers = {
    serve: <E>(app: HttpApp.Default<E>): Layer.Layer<LoopbackService> =>
        Layer.mergeAll(Layer.effect(Loopback, _loopback), HttpServer.serve(app)).pipe(Layer.provide(NodeHttpServer.layerTest), Layer.orDie),
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export {
    Loopback,
    LoopbackServers,
    type LoopbackService,
    ObjectStore,
    ObjectStoreDoubles,
    type ObjectStoreService,
    type PgliteOptions,
    TestDatabase,
    type TestDatabaseService,
    TestDatabases,
    TestResourceError,
};
