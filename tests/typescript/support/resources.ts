import type { HttpApp } from '@effect/platform';
import { HttpClient, HttpServer } from '@effect/platform';
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
    Option,
    type Order,
    type ParseResult,
    Ref,
    Schema,
    type Scope,
} from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

declare namespace TestDatabase {
    type Service = {
        readonly exec: (
            statements: string,
        ) => Effect.Effect<void, TestResourceError>;
        readonly rows: (
            statement: string,
            params?: ReadonlyArray<unknown>,
        ) => Effect.Effect<ReadonlyArray<unknown>, TestResourceError>;
        readonly decoded: <A, I>(
            schema: Schema.Schema<A, I, never>,
        ) => (
            statement: string,
            params?: ReadonlyArray<unknown>,
        ) => Effect.Effect<
            ReadonlyArray<A>,
            TestResourceError | ParseResult.ParseError
        >;
        readonly listen: (
            channel: string,
        ) => Effect.Effect<
            Mailbox.ReadonlyMailbox<string, TestResourceError>,
            TestResourceError,
            Scope.Scope
        >;
        readonly rollbackTransaction: <A, E, R>(
            work: Effect.Effect<A, E, R>,
        ) => Effect.Effect<A, E | TestResourceError, R>;
    };
    type PgliteOptions = {
        readonly seed?: string;
        readonly extensions?: Extensions;
    };
}

declare namespace ObjectStore {
    type Service = {
        readonly put: (
            key: string,
            bytes: Uint8Array,
        ) => Effect.Effect<void, TestResourceError>;
        readonly get: (
            key: string,
        ) => Effect.Effect<Option.Option<Uint8Array>, TestResourceError>;
        readonly list: (
            prefix: string,
        ) => Effect.Effect<ReadonlyArray<string>, TestResourceError>;
        readonly remove: (
            key: string,
        ) => Effect.Effect<void, TestResourceError>;
        readonly url: (
            key: string,
            ttlSeconds: number,
        ) => Effect.Effect<string, TestResourceError>;
    };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _CONTROL_NOTIFICATION_PREFIX = '<rasm-test-support-control:';

// --- [ERRORS] --------------------------------------------------------------------------

class TestResourceError extends Data.TaggedError('TestResourceError')<{
    readonly resource: 'loopback' | 'database' | 'object-store';
    readonly reason: 'operation' | 'unsupported';
    readonly code: Option.Option<string>;
    readonly detail: string;
}> {
    static readonly operation =
        (resource: TestResourceError['resource']) =>
        (cause: unknown): TestResourceError =>
            new TestResourceError({
                resource,
                reason: 'operation',
                code:
                    cause instanceof Error
                        ? Option.some(cause.name)
                        : Option.none(),
                detail: String(cause),
            });
}

// --- [SERVICES] ------------------------------------------------------------------------

class Loopback extends Context.Tag('rasm-test-support/Loopback')<
    Loopback,
    {
        readonly url: string;
        readonly client: HttpClient.HttpClient;
    }
>() {}

class ObjectStore extends Context.Tag('rasm-test-support/ObjectStore')<
    ObjectStore,
    ObjectStore.Service
>() {}

class TestDatabase extends Context.Tag('rasm-test-support/TestDatabase')<
    TestDatabase,
    TestDatabase.Service
>() {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const _fromPromise = <A>(
    resource: TestResourceError['resource'],
    run: () => Promise<A>,
): Effect.Effect<A, TestResourceError> =>
    Effect.tryPromise({
        try: run,
        catch: TestResourceError.operation(resource),
    });

const _utf8 = new TextEncoder();
const _byKeyBytes: Order.Order<string> = (self, that) => {
    const left = _utf8.encode(self);
    const right = _utf8.encode(that);
    for (const [at, own] of left
        .subarray(0, Math.min(left.length, right.length))
        .entries()) {
        const other = right[at];
        if (other !== undefined && own !== other) {
            return own < other ? -1 : 1;
        }
    }
    return left.length === right.length
        ? 0
        : left.length < right.length
          ? -1
          : 1;
};

const _database = (
    exec: TestDatabase.Service['exec'],
    rows: TestDatabase.Service['rows'],
    listen: TestDatabase.Service['listen'],
    rollbackTransaction: TestDatabase.Service['rollbackTransaction'],
): TestDatabase.Service => ({
    exec,
    rows,
    decoded: (schema) => {
        const decode = Schema.decodeUnknown(Schema.Array(schema));
        return (statement, params) =>
            Effect.flatMap(rows(statement, params), decode);
    },
    listen,
    rollbackTransaction,
});

const _rollbackTransaction =
    (
        exec: TestDatabase.Service['exec'],
    ): TestDatabase.Service['rollbackTransaction'] =>
    (work) =>
        Effect.acquireUseRelease(
            exec('BEGIN'),
            () => work,
            () => Effect.orDie(exec('ROLLBACK')),
        );

const _pgliteListen =
    (db: PGlite): TestDatabase.Service['listen'] =>
    (channel) =>
        Effect.gen(function* () {
            const mailbox = yield* Mailbox.make<string, TestResourceError>();
            yield* Effect.acquireRelease(
                _fromPromise('database', () =>
                    db.listen(
                        channel,
                        (payload) =>
                            void (
                                payload.startsWith(
                                    _CONTROL_NOTIFICATION_PREFIX,
                                ) || mailbox.unsafeOffer(payload)
                            ),
                    ),
                ),
                (dispose) => Effect.ignore(Effect.promise(() => dispose())),
            );
            return mailbox;
        });

const TestDatabases = {
    pglite: (
        options?: string | TestDatabase.PgliteOptions,
    ): Layer.Layer<TestDatabase, TestResourceError> => {
        const configuration: TestDatabase.PgliteOptions =
            typeof options === 'string' ? { seed: options } : (options ?? {});
        return Layer.scoped(
            TestDatabase,
            Effect.gen(function* () {
                const db = yield* Effect.acquireRelease(
                    _fromPromise('database', () =>
                        PGlite.create({
                            relaxedDurability: true,
                            ...(configuration.extensions === undefined
                                ? {}
                                : { extensions: configuration.extensions }),
                        }),
                    ),
                    (live) => Effect.promise(() => live.close()),
                );
                const seed = configuration.seed;
                yield* seed === undefined
                    ? Effect.void
                    : _fromPromise('database', () => db.exec(seed));
                const exec: TestDatabase.Service['exec'] = (statements) =>
                    Effect.asVoid(
                        _fromPromise('database', () => db.exec(statements)),
                    );
                return _database(
                    exec,
                    (statement, params) =>
                        Effect.map(
                            _fromPromise('database', () =>
                                db.query(
                                    statement,
                                    params === undefined
                                        ? undefined
                                        : [...params],
                                ),
                            ),
                            (result) => result.rows,
                        ),
                    _pgliteListen(db),
                    _rollbackTransaction(exec),
                );
            }),
        );
    },
} as const;

const ObjectStoreDoubles = {
    memory: Layer.effect(
        ObjectStore,
        Effect.map(Ref.make(HashMap.empty<string, Uint8Array>()), (cell) => ({
            put: (key, bytes) => Ref.update(cell, HashMap.set(key, bytes)),
            get: (key) => Effect.map(Ref.get(cell), HashMap.get(key)),
            list: (prefix) =>
                Effect.map(Ref.get(cell), (held) =>
                    Array.sort(
                        Array.filter(
                            Array.fromIterable(HashMap.keys(held)),
                            (key) => key.startsWith(prefix),
                        ),
                        _byKeyBytes,
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
} as const;

const _loopbackValue: Effect.Effect<
    Context.Tag.Service<Loopback>,
    never,
    HttpClient.HttpClient | HttpServer.HttpServer
> = Effect.gen(function* () {
    const client = yield* HttpClient.HttpClient;
    const url = yield* HttpServer.addressFormattedWith(Effect.succeed);
    return { url, client };
});

const LoopbackServers = {
    serve: <E>(app: HttpApp.Default<E>): Layer.Layer<Loopback> => {
        const base = NodeHttpServer.layerTest;
        return Layer.mergeAll(
            Layer.effect(Loopback, _loopbackValue),
            HttpServer.serve(app),
        ).pipe(Layer.provide(base), Layer.orDie);
    },
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export {
    Loopback,
    LoopbackServers,
    ObjectStore,
    ObjectStoreDoubles,
    TestDatabase,
    TestDatabases,
    TestResourceError,
};
