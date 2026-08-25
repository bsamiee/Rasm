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

declare namespace PgLane {
    type Service = {
        readonly exec: (
            statements: string,
        ) => Effect.Effect<void, HarnessFault>;
        readonly rows: (
            statement: string,
            params?: ReadonlyArray<unknown>,
        ) => Effect.Effect<ReadonlyArray<unknown>, HarnessFault>;
        readonly decoded: <A, I>(
            schema: Schema.Schema<A, I, never>,
        ) => (
            statement: string,
            params?: ReadonlyArray<unknown>,
        ) => Effect.Effect<
            ReadonlyArray<A>,
            HarnessFault | ParseResult.ParseError
        >;
        readonly listen: (
            channel: string,
        ) => Effect.Effect<
            Mailbox.ReadonlyMailbox<string, HarnessFault>,
            HarnessFault,
            Scope.Scope
        >;
        readonly sandbox: <A, E, R>(
            work: Effect.Effect<A, E, R>,
        ) => Effect.Effect<A, E | HarnessFault, R>;
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
        ) => Effect.Effect<void, HarnessFault>;
        readonly get: (
            key: string,
        ) => Effect.Effect<Option.Option<Uint8Array>, HarnessFault>;
        readonly list: (
            prefix: string,
        ) => Effect.Effect<ReadonlyArray<string>, HarnessFault>;
        readonly remove: (key: string) => Effect.Effect<void, HarnessFault>;
        readonly url: (
            key: string,
            ttlSeconds: number,
        ) => Effect.Effect<string, HarnessFault>;
    };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ARM = { prefix: '<rasm-testkit-armed:' } as const;

// --- [ERRORS] --------------------------------------------------------------------------

class HarnessFault extends Data.TaggedError('HarnessFault')<{
    readonly lane: 'loopback' | 'pg' | 'store';
    readonly reason: 'engine' | 'unsupported';
    readonly code: Option.Option<string>;
    readonly detail: string;
}> {
    static readonly engine =
        (lane: HarnessFault['lane']) =>
        (defect: unknown): HarnessFault =>
            new HarnessFault({
                lane,
                reason: 'engine',
                code:
                    defect instanceof Error
                        ? Option.some(defect.name)
                        : Option.none(),
                detail: String(defect),
            });
}

// --- [SERVICES] ------------------------------------------------------------------------

class Loopback extends Context.Tag('rasm-testkit/Loopback')<
    Loopback,
    {
        readonly url: string;
        readonly client: HttpClient.HttpClient;
    }
>() {}

class ObjectStore extends Context.Tag('rasm-testkit/ObjectStore')<
    ObjectStore,
    ObjectStore.Service
>() {}

class PgLane extends Context.Tag('rasm-testkit/PgLane')<
    PgLane,
    PgLane.Service
>() {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const _guarded = <A>(
    lane: HarnessFault['lane'],
    run: () => Promise<A>,
): Effect.Effect<A, HarnessFault> =>
    Effect.tryPromise({ try: run, catch: HarnessFault.engine(lane) });

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

const _lane = (
    exec: PgLane.Service['exec'],
    rows: PgLane.Service['rows'],
    listen: PgLane.Service['listen'],
    sandbox: PgLane.Service['sandbox'],
): PgLane.Service => ({
    exec,
    rows,
    decoded: (schema) => {
        const decode = Schema.decodeUnknown(Schema.Array(schema));
        return (statement, params) =>
            Effect.flatMap(rows(statement, params), decode);
    },
    listen,
    sandbox,
});

const _bracketSandbox =
    (exec: PgLane.Service['exec']): PgLane.Service['sandbox'] =>
    (work) =>
        Effect.acquireUseRelease(
            exec('BEGIN'),
            () => work,
            () => Effect.orDie(exec('ROLLBACK')),
        );

const _pgliteListen =
    (db: PGlite): PgLane.Service['listen'] =>
    (channel) =>
        Effect.gen(function* () {
            const box = yield* Mailbox.make<string, HarnessFault>();
            yield* Effect.acquireRelease(
                _guarded('pg', () =>
                    db.listen(
                        channel,
                        (payload) =>
                            void (
                                payload.startsWith(_ARM.prefix) ||
                                box.unsafeOffer(payload)
                            ),
                    ),
                ),
                (dispose) => Effect.ignore(Effect.promise(() => dispose())),
            );
            return box;
        });

const PgLanes = {
    pglite: (
        options?: string | PgLane.PgliteOptions,
    ): Layer.Layer<PgLane, HarnessFault> => {
        const lane: PgLane.PgliteOptions =
            typeof options === 'string' ? { seed: options } : (options ?? {});
        return Layer.scoped(
            PgLane,
            Effect.gen(function* () {
                const db = yield* Effect.acquireRelease(
                    _guarded('pg', () =>
                        PGlite.create({
                            relaxedDurability: true,
                            ...(lane.extensions === undefined
                                ? {}
                                : { extensions: lane.extensions }),
                        }),
                    ),
                    (live) => Effect.promise(() => live.close()),
                );
                const seed = lane.seed;
                yield* seed === undefined
                    ? Effect.void
                    : _guarded('pg', () => db.exec(seed));
                const exec: PgLane.Service['exec'] = (statements) =>
                    Effect.asVoid(_guarded('pg', () => db.exec(statements)));
                return _lane(
                    exec,
                    (statement, params) =>
                        Effect.map(
                            _guarded('pg', () =>
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
                    _bracketSandbox(exec),
                );
            }),
        );
    },
} as const;

const ObjectStores = {
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
                    new HarnessFault({
                        lane: 'store',
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

const Loopbacks = {
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
    HarnessFault,
    Loopback,
    Loopbacks,
    ObjectStore,
    ObjectStores,
    PgLane,
    PgLanes,
};
