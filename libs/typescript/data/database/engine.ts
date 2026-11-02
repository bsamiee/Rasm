import { Cause, Effect, Option, type Schema } from 'effect';
import { SqlClient, type SqlError, type Statement } from 'effect/sql';
import type { Catalog } from '#/catalog';
import type { Column, Fields, Kind } from '#/column';
import { Pg } from '#/engine/pg';
import { Sqlite } from '#/engine/sqlite';
import type { Finding, ReconcileError } from '#/finding';
import type { Identity } from '#/reconcile';
import type { Relation, TableOptions } from '#/relation';

// --- [TYPES] ---------------------------------------------------------------------------

interface Engine {
    readonly types: Readonly<Record<Kind, string>>;
    readonly member: (sql: SqlClient.SqlClient, field: string) => Statement.Fragment;
    readonly element: (sql: SqlClient.SqlClient, entry: Column, text: Statement.Fragment) => Statement.Fragment;
    readonly texts: (sql: SqlClient.SqlClient, body: string) => Statement.Fragment;
    readonly insertion: (sql: SqlClient.SqlClient, body: string, order: Statement.Fragment) => Statement.Fragment;
    readonly read: (sql: SqlClient.SqlClient, entry: Column, scope: Statement.Fragment) => Statement.Fragment;
    readonly upsert: (parts: Effect.Success<ReturnType<typeof statements>>, body: string, set: readonly Column[]) => Effect.Effect<unknown, SqlError.SqlError, SqlClient.SqlClient>;
    readonly ensure: (parts: Effect.Success<ReturnType<typeof statements>>, body: string) => Effect.Effect<unknown, SqlError.SqlError, SqlClient.SqlClient>;
    readonly latest: (parts: Effect.Success<ReturnType<typeof statements>>, body: string, ranked: (scope: Statement.Fragment) => Statement.Fragment, newest: (selected: Statement.Fragment) => Statement.Fragment) => Statement.Statement<object>;
    readonly search: (parts: Effect.Success<ReturnType<typeof statements>>, searchable: Option.Option.Value<Relation['search']>, text: string, pattern: string, marks: Option.Option<{ readonly open: string; readonly close: string }>, count: number) => Statement.Statement<object>;
    readonly constant: (entry: Column, value: string | number) => string;
    readonly codePoints: (sql: SqlClient.SqlClient, subject: Statement.Fragment) => Statement.Fragment;
    readonly domain: (sql: SqlClient.SqlClient, subject: Statement.Fragment, kind: Kind) => readonly Statement.Fragment[];
    readonly projection: (sql: SqlClient.SqlClient, entry: Column, from: string, path: readonly string[]) => Statement.Fragment;
    readonly generated: (sql: SqlClient.SqlClient, expression: Statement.Fragment) => Statement.Fragment;
    readonly constraint: (sql: SqlClient.SqlClient, name: string, text: Statement.Fragment) => Statement.Fragment;
    readonly casefold: (sql: SqlClient.SqlClient, name: string) => Statement.Fragment;
    readonly opclass: (method: string, json: boolean) => Option.Option<{ readonly name: string; readonly extensions: readonly string[] }>;
    readonly projected: (sql: SqlClient.SqlClient, expression: Statement.Fragment, name: string) => Statement.Fragment;
    readonly methods: readonly string[];
    readonly covering: (sql: SqlClient.SqlClient, unique: boolean, include: readonly string[]) => { readonly keys: readonly Statement.Fragment[]; readonly clause: readonly Statement.Fragment[] };
    readonly suffix: (withoutRowid: boolean) => string;
    readonly fulltext: (
        sql: SqlClient.SqlClient,
        owner: string,
        search: Option.Option.Value<Relation['search']>,
    ) => {
        readonly indexes: readonly {
            readonly name: string;
            readonly parts: readonly string[];
            readonly extensions: readonly string[];
            readonly configuration: Option.Option<{ readonly name: string; readonly configuration: string; readonly filters: readonly string[] }>;
            readonly text: (prefix: string, on: Statement.Fragment) => Statement.Statement<object>;
        }[];
        readonly fts: Option.Option<{ readonly name: string; readonly create: Statement.Statement<object>; readonly triggers: readonly { readonly name: string; readonly create: Statement.Statement<object> }[]; readonly rebuild: Statement.Statement<object> }>;
    };
    readonly policy: (sql: SqlClient.SqlClient, owner: string, name: string, type: string, setting: string) => Option.Option<{ readonly name: string; readonly rule: Statement.Fragment; readonly text: (on: Statement.Fragment, role: Statement.Fragment) => Statement.Statement<object> }>;
    readonly comment: (sql: SqlClient.SqlClient, name: string, identity: typeof Identity.Type) => { readonly inline: Statement.Fragment; readonly statements: (relation: Statement.Fragment, column: Option.Option<string>) => readonly Statement.Statement<object>[] };
    readonly refusals: (relation: Relation) => readonly (typeof Finding.Type)[];
    readonly open: (sql: SqlClient.SqlClient) => Effect.Effect<unknown, SqlError.SqlError>;
    readonly plan: (
        sql: SqlClient.SqlClient,
        catalog: Catalog,
        runtime: Option.Option<string>,
    ) => Effect.Effect<
        {
            readonly preamble: readonly Statement.Statement<object>[];
            readonly statements: readonly Statement.Statement<object>[];
            readonly concurrent: readonly Statement.Statement<object>[];
            readonly undeclared: {
                readonly tables: readonly string[];
                readonly columns: readonly { readonly table: string; readonly column: string }[];
                readonly indexes: readonly string[];
                readonly references: readonly { readonly table: string; readonly columns: readonly string[]; readonly target: string }[];
            };
        },
        SqlError.SqlError | Schema.SchemaError | ReconcileError
    >;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const engine = SqlClient.SqlClient.use((sql) =>
    sql.onDialect({
        sqlite: () => Effect.succeed<Engine>(Sqlite),
        pg: () => Effect.succeed<Engine>(Pg),
        mysql: () => Effect.fail(new Cause.IllegalArgumentError('mysql has no engine')),
        mssql: () => Effect.fail(new Cause.IllegalArgumentError('mssql has no engine')),
        clickhouse: () => Effect.fail(new Cause.IllegalArgumentError('clickhouse has no engine')),
    }),
);
const statements = <F extends Fields, O extends TableOptions<F>>(relation: Relation<F, O>) =>
    Effect.map(Effect.all([SqlClient.SqlClient, engine]), ([sql, picked]) => {
        const columns = Object.values(relation.columns);
        const written = columns.filter(({ projection }) => Option.isNone(projection));
        const [bare, owner] = [sql.literal(''), sql`${sql(relation.name)}`];
        const element = (entry: Column) => picked.element(sql, entry, picked.member(sql, entry.field));
        const keys = (scope: Statement.Fragment) => sql.csv(relation.key.map((part) => sql`${scope}${sql(part)}`));
        const reads = (scope: Statement.Fragment, ...extra: readonly string[]) => sql.csv([...columns.map((entry) => picked.read(sql, entry, scope)), ...extra]);
        const order = sql.csv(relation.key.flatMap((part) => written.filter(({ name }) => name === part).map(element)));
        return {
            sql,
            picked,
            name: relation.name,
            written,
            owner,
            prefix: sql`${owner}${sql.literal('.')}`,
            bare,
            order,
            ignore: sql`ON CONFLICT (${keys(bare)}) DO NOTHING`,
            element,
            keys,
            reads,
            inserting: (body: string, conflict: Statement.Fragment, returning: Statement.Fragment) => sql`INSERT INTO ${owner} (${sql.csv(written.map(({ name }) => sql`${sql(name)}`))}) SELECT ${sql.csv(written.map(element))} FROM ${picked.insertion(sql, body, order)} ${conflict} RETURNING ${returning}`,
            matching: (entry: Column, body: string) => sql`${sql(entry.name)} IN (SELECT ${picked.element(sql, entry, sql.literal('e.value'))} FROM ${picked.texts(sql, body)})`,
        };
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Engine, engine, statements };
