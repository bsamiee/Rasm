import { D1Client } from '@effect/sql-d1';
import { Array, Data, type Duration, Effect, Layer, Option, Schema, Struct } from 'effect';
import { SqlClient, type Statement } from 'effect/sql';
import { Column } from '#/column';
import { type Engine, engine } from '#/engine';
import { ReconcileError } from '#/finding';
import { refusals } from '#/reconcile';
import type { Relation } from '#/relation';
import { atomic } from '#/transaction';

// --- [MODELS] --------------------------------------------------------------------------

const Role = Schema.Tuple([Schema.Struct({ role: Schema.String })]);
class Catalog extends Data.Class<{
    readonly tables: Array.NonEmptyReadonlyArray<Relation>;
    readonly retired?: readonly string[];
    readonly reader?: string;
    readonly excluded?: readonly string[];
    readonly timeouts?: { readonly lock: Duration.Input; readonly transaction: Duration.Input };
}> {
    static readonly plan = Effect.fn('Catalog.plan')(
        (catalog: Catalog, runtime: Option.Option<string> = Option.none()) => converge(catalog, runtime, () => Effect.void),
        Effect.map(({ preamble, statements, concurrent, undeclared }) => {
            const text = (written: readonly Statement.Statement<object>[]) => written.map((statement) => statement.compile(true)[0]);
            return { preamble: text(preamble), statements: text(statements), concurrent: text(concurrent), undeclared };
        }),
    );
    static readonly reconcile = Effect.fn('Catalog.reconcile')(
        (catalog: Catalog, runtime: Option.Option<string> = Option.none()) => converge(catalog, runtime, ({ statements }) => Array.match(statements, { onEmpty: () => Effect.void, onNonEmpty: atomic })),
        Effect.tap(({ concurrent }) => Effect.all(concurrent, { discard: true })),
        Effect.map(Struct.get('undeclared')),
    );
    static readonly layer = <A, E, R, M = never, N = never>(catalog: Catalog, client: (options: typeof Column.transforms) => Layer.Layer<A | SqlClient.SqlClient, E, R>, migrator?: Layer.Layer<SqlClient.SqlClient, M, N>) =>
        Layer.effectDiscard(start(catalog, Option.fromUndefinedOr(migrator))).pipe(Layer.provideMerge(client(Column.transforms)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const converge = Effect.fnUntraced(function* <E, R>(catalog: Catalog, runtime: Option.Option<string>, after: (planned: Effect.Success<ReturnType<Engine['plan']>>) => Effect.Effect<unknown, E, R>) {
    const [client, picked, d1] = yield* Effect.all([SqlClient.SqlClient, engine, Effect.serviceOption(D1Client.D1Client)]);
    yield* Array.match(
        catalog.tables.flatMap((relation) => [...refusals(relation), ...picked.refusals(relation)]),
        { onEmpty: () => Effect.void, onNonEmpty: (findings) => Effect.fail(ReconcileError.make({ findings })) },
    );
    const steps = picked.plan(client.withoutTransforms(), catalog, runtime);
    const applied = Effect.tap(steps, after);
    return yield* Option.match(d1, {
        onNone: () => client.withTransaction(applied),
        onSome: () =>
            Effect.catchTag(applied, 'SqlError', (error) =>
                Effect.filterOrFail(
                    steps,
                    ({ statements }) => !Array.isReadonlyArrayNonEmpty(statements),
                    () => error,
                ),
            ),
    });
});
const start = Effect.fnUntraced(function* <M, N>(catalog: Catalog, migrator: Option.Option<Layer.Layer<SqlClient.SqlClient, M, N>>) {
    const undeclared = yield* Option.match(migrator, {
        onNone: () => Catalog.reconcile(catalog),
        onSome: (layer) =>
            SqlClient.SqlClient.use((sql) => sql`SELECT current_user AS role`).pipe(
                Effect.flatMap(Schema.decodeUnknownEffect(Role)),
                Effect.flatMap(([{ role }]) => Effect.provide(Catalog.reconcile(catalog, Option.some(role)), layer)),
            ),
    });
    yield* Object.values(undeclared).some((objects) => objects.length > 0) ? Effect.annotateLogs(Effect.logInfo('undeclared database objects'), undeclared) : Effect.void;
    const [picked, client] = yield* Effect.all([engine, SqlClient.SqlClient]);
    yield* picked.open(client);
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Catalog };
