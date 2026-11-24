import { Array, Cause, Data, type Duration, Effect, Formatter, Function, Layer, Option, Struct, UndefinedOr } from 'effect';
import { SqlClient, type SqlError, type Statement } from 'effect/sql';
import { Engine } from '#/engine';
import { ReconcileError } from '#/error';
import { digest, refusals } from '#/reconcile';
import type { Relation } from '#/relation';

// --- [MODELS] --------------------------------------------------------------------------

class Catalog extends Data.Class<{
    readonly tables: Array.NonEmptyReadonlyArray<Relation>;
    readonly retired: readonly string[];
    readonly reader: Option.Option<string>;
    readonly timeouts: Option.Option<{ readonly lock: Duration.Input; readonly transaction: Duration.Input }>;
}> {
    static readonly make = ({ tables, retired = [], reader, timeouts }: Pick<Catalog, 'tables'> & Partial<Pick<Catalog, 'retired'>> & { readonly reader?: string; readonly timeouts?: Option.Option.Value<Catalog['timeouts']> }) =>
        new Catalog({ tables, retired, reader: Option.fromUndefinedOr(reader), timeouts: Option.fromUndefinedOr(timeouts) });
    static readonly digest = ({ tables, retired, reader }: Catalog) =>
        digest(
            Formatter.formatJson({
                tables: tables.map(({ columns, search, tenant, ...table }) => ({
                    ...Struct.omit(table, ['schema', 'options', 'queries', 'paths']),
                    columns: Object.values(columns).map(Struct.omit(['schema'])),
                    search: Option.map(search, ({ columns: searched, tokenizer }) => ({ columns: searched.map(Struct.get('name')), tokenizer })),
                    tenant: Option.map(tenant, ({ column, setting }) => ({ column: column.name, setting })),
                })),
                retired,
                reader,
            }),
        );
    static readonly plan = Effect.fn('Catalog.plan')(
        (catalog: Catalog, runtime: Option.Option<string> = Option.none()) => converge(catalog, runtime, () => Effect.void),
        Effect.map(({ preamble, statements, concurrent, undeclared }) => ({ preamble: text(preamble), statements: text(statements), concurrent: text(concurrent), undeclared })),
    );
    static readonly reconcile = Effect.fn('Catalog.reconcile')(
        (catalog: Catalog, runtime: Option.Option<string> = Option.none()) => converge(catalog, runtime, ({ statements }, engine) => Array.match(statements, { onEmpty: () => Effect.void, onNonEmpty: engine.execute })),
        Effect.tap(Struct.get('concurrently')),
        Effect.map(Struct.get('undeclared')),
    );
    static readonly layer = <A, E, R, M = never, N = never>(catalog: Catalog, engine: Layer.Layer<A | Engine | SqlClient.SqlClient, E, R>, migrator?: Layer.Layer<SqlClient.SqlClient, M, N>) => Layer.effectDiscard(start(catalog, Option.fromUndefinedOr(migrator))).pipe(Layer.provideMerge(engine));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const text = (written: readonly Statement.Statement<object>[]) => written.map((statement) => statement.compile(true)[0]);
const converge = Effect.fnUntraced(function* (catalog: Catalog, runtime: Option.Option<string>, after: (planned: Effect.Success<ReturnType<Engine['Service']['plan']>>, engine: Engine['Service']) => Effect.Effect<unknown, SqlError.SqlError, SqlClient.SqlClient>) {
    const [client, engine] = yield* Effect.all([SqlClient.SqlClient, Engine]);
    const stored = UndefinedOr.match(engine.names.transformQueryNames, {
        onUndefined: () => catalog,
        onDefined: (rename) => new Catalog({ ...catalog, tables: Array.map(catalog.tables, (table) => table.named(rename)), retired: catalog.retired.map(rename) }),
    });
    yield* ReconcileError.raise([...stored.tables.flatMap(refusals), ...stored.tables.flatMap(engine.refusals)]);
    const steps = engine.plan(client.withoutTransforms(), stored, runtime);
    const settled = (planned: Parameters<typeof after>[0]): Effect.Effect<Parameters<typeof after>[0], Effect.Error<typeof steps>, SqlClient.SqlClient> =>
        Effect.catchTag(
            Effect.as(after(planned, engine), planned),
            'SqlError',
            Effect.fnUntraced(function* (error) {
                const next = yield* steps;
                const [failed, left] = [new Set(text(planned.statements)), new Set(text(next.statements))];
                return yield* left.size < failed.size && left.isSubsetOf(failed) ? settled(next) : error;
            }),
        );
    return yield* engine.transactional ? client.withTransaction(Effect.tap(steps, (planned) => after(planned, engine))) : Effect.flatMap(steps, settled);
});
const start = Effect.fnUntraced(function* <M, N>(catalog: Catalog, migrator: Option.Option<Layer.Layer<SqlClient.SqlClient, M, N>>) {
    const [engine, client] = yield* Effect.all([Engine, SqlClient.SqlClient]);
    const undeclared = yield* Option.match(migrator, {
        onNone: () => Catalog.reconcile(catalog),
        onSome: (layer) =>
            Effect.fromNullishOr(engine.role).pipe(
                Effect.mapError(() => new Cause.IllegalArgumentError(`${client.driver} has no role to migrate as`)),
                Effect.flatMap(Function.apply(client)),
                Effect.flatMap((role) => Effect.provide(Catalog.reconcile(catalog, Option.some(role)), layer)),
            ),
    });
    yield* Object.values(undeclared).some(Array.isReadonlyArrayNonEmpty<unknown>) ? Effect.annotateLogs(Effect.logInfo('undeclared database objects'), undeclared) : Effect.void;
    yield* engine.open?.(client) ?? Effect.void;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Catalog };
