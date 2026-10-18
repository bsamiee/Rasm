import { D1Client } from '@effect/sql-d1';
import { SqliteClient } from '@effect/sql-sqlite-do';
import { Array, Effect, Layer, Option, Schema } from 'effect';
import { Hex } from 'effect/encoding';
import { SqlClient, SqlSchema, type Statement } from 'effect/sql';
import { type Catalog, type Column, dialects, type Fields, names, quote, type Table } from './catalog.ts';
import { reconcile } from './reconcile.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const patterns = { words: /\s+/u, like: /[\\%_]/gu };

// --- [OPERATIONS] ----------------------------------------------------------------------

const engine = Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient;
    const dialect = sql.onDialectOrElse({ pg: () => dialects.pg, orElse: () => dialects.sqlite });
    const json = Schema.encodeEffect(Schema.fromJsonString(Schema.Unknown, { replacer: (_, value) => (value instanceof Uint8Array ? Hex.encode(value) : value) }));
    const reads = (columns: readonly Column[], scope: string) => sql.csv(columns.map((column) => sql.literal(dialect.read(column, scope))));
    return { sql, dialect, json, reads };
});
const write = (merge: boolean) =>
    Effect.fnUntraced(function* <F extends Fields>(table: Table<F>, rows: Array.NonEmptyReadonlyArray<Table<F>['Type']>) {
        const { sql, dialect, json, reads } = yield* engine;
        const document = yield* Effect.flatMap(Schema.encodeUnknownEffect(Schema.Array(table))(rows), json);
        const columns = Object.values(table.table.columns);
        const written = columns.filter(({ projection }) => Option.isNone(projection));
        const [owner, keys] = [sql(table.table.name), sql.csv(table.table.key.map(quote))];
        const changed = written.filter(({ name }) => !table.table.key.includes(name)).map(({ name }) => sql(name));
        const conflict = merge
            ? Array.match(changed, {
                  onEmpty: () => sql`ON CONFLICT (${keys}) DO NOTHING`,
                  onNonEmpty: (set) => sql`ON CONFLICT (${keys}) DO UPDATE SET ${sql.csv(set.map((name) => sql`${name} = excluded.${name}`))} WHERE (${sql.csv(set.map((name) => sql`${owner}.${name}`))}) IS DISTINCT FROM (${sql.csv(set.map((name) => sql`excluded.${name}`))})`,
              })
            : sql.literal('');
        const element = (column: Column) => sql.literal(dialect.element(column, dialect.member('e.value', column.field)));
        const [targets, values, order] = [sql.csv(written.map(({ name }) => quote(name))), sql.csv(written.map(element)), sql.csv(table.table.key.flatMap((name) => written.filter((column) => column.name === name).map(element)))];
        return sql.onDialectOrElse({
            pg: () => sql`INSERT INTO ${owner} (${targets}) SELECT ${values} FROM jsonb_array_elements(${document}) e(value) ORDER BY ${order} ${conflict} RETURNING ${reads(columns, '')}`,
            orElse: () => sql`INSERT INTO ${owner} (${targets}) SELECT ${values} FROM json_each(${document}) e WHERE true ${conflict} RETURNING ${reads(columns, '')}`,
        });
    });
const insert = write(false);
const upsert = write(true);
const member = Effect.fnUntraced(function* <F extends Fields, K extends keyof F & string>(table: Table<F>, field: K, values: Array.NonEmptyReadonlyArray<F[K]['Type']>) {
    const { sql, dialect, json, reads } = yield* engine;
    const column = table.table.columns[field];
    const document = yield* Effect.flatMap(Schema.encodeUnknownEffect(Schema.Array(column.schema))(values), json);
    const element = sql.literal(dialect.element(column, 'e.value'));
    return {
        sql,
        owner: sql(table.table.name),
        reads: reads(Object.values(table.table.columns), ''),
        keys: sql.csv(table.table.key.map(quote)),
        where: sql.onDialectOrElse({
            pg: () => sql`${sql(column.name)} IN (SELECT ${element} FROM jsonb_array_elements_text(${document}) e(value))`,
            orElse: () => sql`${sql(column.name)} IN (SELECT ${element} FROM json_each(${document}) e)`,
        }),
    };
});
const select = Effect.fnUntraced(function* <F extends Fields, K extends keyof F & string>(table: Table<F>, field: K, values: Array.NonEmptyReadonlyArray<F[K]['Type']>) {
    const { sql, owner, reads, keys, where } = yield* member(table, field, values);
    return sql`SELECT ${reads} FROM ${owner} WHERE ${where} ORDER BY ${keys}`;
});
const remove = Effect.fnUntraced(function* <F extends Fields, K extends keyof F & string>(table: Table<F>, field: K, values: Array.NonEmptyReadonlyArray<F[K]['Type']>) {
    const { sql, owner, reads, where } = yield* member(table, field, values);
    return sql`DELETE FROM ${owner} WHERE ${where} RETURNING ${reads}`;
});
const latest = Effect.fnUntraced(function* <F extends Fields, G extends keyof F & string>(table: Table<F>, group: G, order: keyof F & string, groups: Array.NonEmptyReadonlyArray<F[G]['Type']>) {
    const { sql, dialect, json, reads } = yield* engine;
    const [owner, by, columns] = [sql(table.table.name), table.table.columns[group], Object.values(table.table.columns)];
    const document = yield* Effect.flatMap(Schema.encodeUnknownEffect(Schema.Array(by.schema))(groups), json);
    const matched = sql`i.${sql(by.name)} = ${sql.literal(dialect.element(by, 'e.value'))}`;
    const descending = sql.csv([table.table.columns[order].name, ...table.table.key].map((name) => sql`i.${sql(name)} DESC`));
    const keys = (scope: string) => sql.csv(table.table.key.map((name) => sql`${sql.literal(scope)}${sql(name)}`));
    return sql.onDialectOrElse({
        pg: () => sql`SELECT ${reads(columns, 'l.')} FROM jsonb_array_elements_text(${document}) e(value) CROSS JOIN LATERAL (SELECT * FROM ${owner} i WHERE ${matched} ORDER BY ${descending} LIMIT 1) l`,
        orElse: () => sql`SELECT ${reads(columns, 't.')} FROM json_each(${document}) e JOIN ${owner} t ON (${keys('t.')}) = (SELECT ${keys('i.')} FROM ${owner} i WHERE ${matched} ORDER BY ${descending} LIMIT 1)`,
    });
});
const search = Effect.fnUntraced(function* <F extends Fields>(table: Table<F>, text: string) {
    const { sql, reads } = yield* engine;
    const { columns, tokenizer } = yield* Effect.fromOption(table.table.search);
    const [owner, all] = [sql(table.table.name), Object.values(table.table.columns)];
    const document = sql.literal(dialects.pg.document(columns));
    return sql.onDialectOrElse({
        pg: () =>
            tokenizer === 'trigram'
                ? sql`SELECT ${reads(all, '')} FROM ${owner} WHERE (${document}) ILIKE ${`%${text.replaceAll(patterns.like, (character) => `\\${character}`)}%`} ORDER BY ${sql.csv(table.table.key.map(quote))}`
                : sql`SELECT ${reads(all, '')} FROM ${owner} WHERE to_tsvector('simple', ${document}) @@ plainto_tsquery('simple', ${text}) ORDER BY ts_rank(to_tsvector('simple', ${document}), plainto_tsquery('simple', ${text})) DESC`,
        orElse: () => {
            const virtual = sql(`${table.table.name}_fts`);
            const terms = (tokenizer === 'trigram' ? [text] : text.split(patterns.words).filter((word) => word !== '')).map((term) => `"${term.replaceAll('"', '""')}"`).join(' ');
            return sql`SELECT ${reads(all, `${quote(table.table.name)}.`)} FROM ${virtual} JOIN ${owner} ON ${owner}."rowid" = ${virtual}."rowid" WHERE ${virtual} MATCH ${terms} ORDER BY ${virtual}."rank"`;
        },
    });
});
const atomic = (statements: Array.NonEmptyReadonlyArray<Statement.Statement<object>>) => Effect.flatMap(Effect.serviceOption(D1Client.D1Client), Option.match({ onSome: (d1) => d1.batch(statements), onNone: () => SqlClient.SqlClient.use((sql) => sql.withTransaction(Effect.all(statements))) }));
const tenant = Effect.fnUntraced(function* <A, E, R>(id: string, effect: Effect.Effect<A, E, R>) {
    const sql = yield* SqlClient.SqlClient;
    return yield* sql.withTransaction(Effect.andThen(sql`SELECT set_config('app.tenant', ${id}, true)`, effect));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const store = <A, E, R, M = never, N = never>(catalog: Catalog, client: (options: typeof names) => Layer.Layer<A | SqlClient.SqlClient, E, R>, migrator?: (options: typeof names) => Layer.Layer<SqlClient.SqlClient, M, N>) =>
    Layer.effectDiscard(
        Effect.gen(function* () {
            const sql = yield* SqlClient.SqlClient;
            const runtime = sql.onDialectOrElse({
                pg: () =>
                    Effect.map(
                        SqlSchema.findOneOption({ Request: Schema.Void, Result: Schema.Struct({ role: Schema.String }), execute: () => sql`SELECT current_user AS role` })(undefined),
                        Option.map(({ role }) => role),
                    ),
                orElse: () => Effect.succeedNone,
            });
            const undeclared = yield* Option.match(Option.fromUndefinedOr(migrator), {
                onNone: () => reconcile(catalog, Option.none()),
                onSome: (layer) => Effect.flatMap(runtime, (role) => Effect.provide(reconcile(catalog, role), layer(names))),
            });
            const node = sql.onDialectOrElse({ sqlite: () => true, orElse: () => false }) && Option.isNone(yield* Effect.serviceOption(D1Client.D1Client)) && Option.isNone(yield* Effect.serviceOption(SqliteClient.SqliteClient));
            yield* Object.values(undeclared).some((objects) => objects.length > 0) ? Effect.logInfo('undeclared', undeclared) : Effect.void;
            yield* node
                ? Effect.andThen(
                      sql`PRAGMA optimize = 0x10002`,
                      Effect.addFinalizer(() => Effect.orDie(sql`PRAGMA optimize`)),
                  )
                : Effect.void;
        }),
    ).pipe(Layer.provideMerge(client(names)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { atomic, insert, latest, remove, search, select, store, tenant, upsert };
