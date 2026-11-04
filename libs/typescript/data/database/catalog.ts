import { D1Client } from '@effect/sql-d1';
import { Array, Data, type Duration, Effect, Layer, Option, String, Struct } from 'effect';
import { SqlClient, type Statement } from 'effect/sql';
import { atomic } from '#/atomic';
import { Column } from '#/column';
import { Engine } from '#/internal/engine/engine';
import { refusals } from '#/internal/reconcile';
import { ReconcileError } from '#/reconcile-error';
import type { Relation } from '#/relation';

// --- [MODELS] --------------------------------------------------------------------------

class Catalog extends Data.Class<{
    readonly tables: Array.NonEmptyReadonlyArray<Relation>;
    readonly retired: readonly string[];
    readonly reader: Option.Option<string>;
    readonly excludedPrefixes: readonly string[];
    readonly timeouts: Option.Option<{ readonly lock: Duration.Input; readonly transaction: Duration.Input }>;
}> {
    static readonly make = ({ tables, retired = [], reader, excludedPrefixes = [], timeouts }: Pick<Catalog, 'tables'> & Partial<Pick<Catalog, 'retired' | 'excludedPrefixes'>> & { readonly reader?: string; readonly timeouts?: Option.Option.Value<Catalog['timeouts']> }) =>
        new Catalog({ tables, retired: retired.map(String.camelToSnake), reader: Option.fromUndefinedOr(reader), excludedPrefixes, timeouts: Option.fromUndefinedOr(timeouts) });
    static readonly plan = Effect.fn('Catalog.plan')(
        (catalog: Catalog, runtime: Option.Option<string> = Option.none()) => converge(catalog, runtime, () => Effect.void),
        Effect.map(({ preamble, statements, concurrent, undeclared }) => {
            const text = (written: readonly Statement.Statement<object>[]) => written.map((statement) => statement.compile(true)[0]);
            return { preamble: text(preamble), statements: text(statements), concurrent: text(concurrent), undeclared };
        }),
    );
    static readonly reconcile = Effect.fn('Catalog.reconcile')(
        (catalog: Catalog, runtime: Option.Option<string> = Option.none()) => converge(catalog, runtime, ({ statements }) => Array.match(statements, { onEmpty: () => Effect.void, onNonEmpty: atomic })),
        Effect.tap(({ concurrent, session }) => Array.match(concurrent, { onEmpty: () => Effect.void, onNonEmpty: (statements) => concurrently(session, statements) })),
        Effect.map(Struct.get('undeclared')),
    );
    static readonly client = <A, E, R>(client: (options: typeof Column.transforms) => Layer.Layer<A | SqlClient.SqlClient, E, R>) => Engine.layer.pipe(Layer.provideMerge(client(Column.transforms)));
    static readonly layer = <A, E, R, M = never, N = never>(catalog: Catalog, client: (options: typeof Column.transforms) => Layer.Layer<A | SqlClient.SqlClient, E, R>, migrator?: Layer.Layer<SqlClient.SqlClient, M, N>) =>
        Layer.effectDiscard(start(catalog, Option.fromUndefinedOr(migrator))).pipe(Layer.provideMerge(Catalog.client(client)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const converge = Effect.fnUntraced(function* <E, R>(catalog: Catalog, runtime: Option.Option<string>, after: (planned: Effect.Success<ReturnType<Engine['Service']['plan']>>) => Effect.Effect<unknown, E, R>) {
    const [client, engine, d1] = yield* Effect.all([SqlClient.SqlClient, Engine, Effect.serviceOption(D1Client.D1Client)]);
    yield* ReconcileError.raise([...catalog.tables.flatMap(refusals), ...catalog.tables.flatMap(engine.refusals)]);
    const steps = engine.plan(client.withoutTransforms(), catalog, runtime);
    const applied = Effect.tap(steps, after);
    const converged = <X>(error: X) =>
        Effect.filterOrFail(
            steps,
            ({ statements }) => Array.isReadonlyArrayEmpty(statements),
            () => error,
        );
    return yield* Option.isNone(d1) ? client.withTransaction(applied) : Effect.catchTag(applied, 'SqlError', converged);
});
const concurrently = Effect.fnUntraced(function* (session: readonly { readonly name: string; readonly value: string }[], statements: Array.NonEmptyReadonlyArray<Statement.Statement<object>>) {
    const sql = yield* SqlClient.SqlClient;
    const names = session.map(Struct.get('name'));
    const connection = yield* Effect.acquireRelease(sql.reserve, (reserved) => Effect.orDie(reserved.executeUnprepared(...sql`SELECT set_config(name, reset_val, false) FROM pg_settings WHERE name = ANY(${names})`.compile(true), undefined)));
    const run = (statement: Statement.Statement<object>) => connection.executeUnprepared(...statement.compile(true), undefined);
    yield* Effect.forEach(session, ({ name, value }) => run(sql`SELECT set_config(${name}, ${value}, false)`), { discard: true });
    return yield* Effect.forEach(statements, run, { discard: true });
}, Effect.scoped);
const start = Effect.fnUntraced(function* <M, N>(catalog: Catalog, migrator: Option.Option<Layer.Layer<SqlClient.SqlClient, M, N>>) {
    const [engine, client] = yield* Effect.all([Engine, SqlClient.SqlClient]);
    const undeclared = yield* Option.match(migrator, { onNone: () => Catalog.reconcile(catalog), onSome: (layer) => Effect.flatMap(engine.role(client), (role) => Effect.provide(Catalog.reconcile(catalog, Option.some(role)), layer)) });
    yield* Object.values(undeclared).some(Array.isReadonlyArrayNonEmpty<unknown>) ? Effect.annotateLogs(Effect.logInfo('undeclared database objects'), undeclared) : Effect.void;
    yield* engine.open(client);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Catalog };
