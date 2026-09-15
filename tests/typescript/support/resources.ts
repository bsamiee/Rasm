// --- [IMPORTS] -------------------------------------------------------------------------

import { type HttpApp, HttpServer, type HttpServerError } from '@effect/platform';
import { NodeHttpServer } from '@effect/platform-node';
import { PGlite, type PGliteOptions } from '@electric-sql/pglite';
import { Array, Context, Data, Effect, flow, HashMap, Layer, type Option, Order, type ParseResult, Ref, Schema, Stream, String, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

interface TestDatabase {
    readonly exec: (statements: string) => Effect.Effect<void, DatabaseError>;
    readonly query: <A, I>(schema: Schema.Schema<A, I, never>, statement: string, params: readonly unknown[]) => Effect.Effect<readonly A[], DatabaseError | ParseResult.ParseError>;
    readonly listen: (channel: string) => Stream.Stream<string, DatabaseError>;
    readonly rollbackTransaction: <A, E, R>(effect: Effect.Effect<A, E, R>) => Effect.Effect<A, E | DatabaseError, R>;
}

interface ObjectStore {
    readonly put: (key: string, bytes: Uint8Array) => Effect.Effect<void>;
    readonly get: (key: string) => Effect.Effect<Option.Option<Uint8Array>>;
    readonly list: (prefix: string) => Effect.Effect<readonly string[]>;
    readonly remove: (key: string) => Effect.Effect<void>;
}

// --- [ERRORS] --------------------------------------------------------------------------

interface DatabaseError {
    readonly _tag: 'DatabaseError';
    readonly cause: unknown;
}

const DatabaseError: Data.Case.Constructor<DatabaseError, '_tag'> = Data.tagged<DatabaseError>('DatabaseError');

// --- [SERVICES] ------------------------------------------------------------------------

const TestDatabase: Context.Tag<TestDatabase, TestDatabase> = Context.GenericTag<TestDatabase>('test-support/TestDatabase');
const ObjectStore: Context.Tag<ObjectStore, ObjectStore> = Context.GenericTag<ObjectStore>('test-support/ObjectStore');

// --- [OPERATIONS] ----------------------------------------------------------------------

const _pglite = <A>(run: () => Promise<A>): Effect.Effect<A, DatabaseError> => Effect.tryPromise({ try: run, catch: (cause) => DatabaseError({ cause }) });

const _dispose = (dispose: () => Promise<void>): Effect.Effect<void> => Effect.promise(() => dispose());

// --- [LAYERS] --------------------------------------------------------------------------

const pglite: (options: PGliteOptions, ...seed: readonly string[]) => Layer.Layer<TestDatabase, DatabaseError> = Effect.fnUntraced(function* (options: PGliteOptions, ...seed: readonly string[]) {
    const db = yield* Effect.acquireRelease(
        _pglite(() => PGlite.create({ relaxedDurability: true, ...options })),
        (instance) => _dispose(() => instance.close()),
    );
    const exec = (statements: string): Effect.Effect<void, DatabaseError> => Effect.asVoid(_pglite(() => db.exec(statements)));
    yield* Effect.forEach(seed, exec, { discard: true });
    return {
        exec,
        query: (schema, statement, params) => _pglite(() => db.query(statement, [...params])).pipe(Effect.map(Struct.get('rows')), Effect.flatMap(Schema.decodeUnknown(Schema.Array(schema)))),
        listen: (channel) =>
            Stream.asyncPush((emit) =>
                Effect.acquireRelease(
                    _pglite(() => db.listen(channel, emit.single)),
                    _dispose,
                ),
            ),
        rollbackTransaction: (effect) =>
            Effect.acquireUseRelease(
                exec('BEGIN'),
                () => effect,
                () => Effect.orDie(exec('ROLLBACK')),
            ),
    } satisfies TestDatabase;
}, Layer.scoped(TestDatabase));

const memoryObjectStore: Layer.Layer<ObjectStore> = Layer.effect(
    ObjectStore,
    Effect.map(Ref.make(HashMap.empty<string, Uint8Array>()), (ref) => ({
        put: (key, bytes) => Ref.update(ref, HashMap.set(key, bytes)),
        get: (key) => Effect.map(Ref.get(ref), HashMap.get(key)),
        list: (prefix) =>
            Effect.map(
                Ref.get(ref),
                flow(HashMap.keys, Array.filter(String.startsWith(prefix)), Array.sort(Order.mapInput(Order.array(Order.number), (key: string) => Array.fromIterable(new TextEncoder().encode(key))))),
            ),
        remove: (key) => Ref.update(ref, HashMap.remove(key)),
    })),
);

const loopback = <E>(app: HttpApp.Default<E>): Layer.Layer<Layer.Layer.Success<typeof NodeHttpServer.layerTest>, HttpServerError.ServeError> =>
    Layer.provideMerge(HttpServer.serve(app), NodeHttpServer.layerTest);

// --- [EXPORTS] -------------------------------------------------------------------------

export { DatabaseError, loopback, memoryObjectStore, ObjectStore, pglite, TestDatabase };
