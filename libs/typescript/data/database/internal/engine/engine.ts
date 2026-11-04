import { Array, Cause, Context, Effect, Layer, Option, type Schema, Struct } from 'effect';
import { SqlClient, type SqlConnection, type SqlError, type Statement } from 'effect/sql';
import type { Catalog } from '#/catalog';
import type { Column, Kind, Projection } from '#/column';
import type { Pg } from '#/internal/engine/pg';
import { Sqlite } from '#/internal/engine/sqlite';
import type { diff, Identity } from '#/internal/reconcile';
import type { ReconcileError } from '#/reconcile-error';
import type { Relation } from '#/relation';

// --- [TYPES] ---------------------------------------------------------------------------

type Method = Relation['indexes'][number]['method'];
interface Shape {
    readonly types: Readonly<Record<Kind, string>>;
    readonly member: (sql: SqlClient.SqlClient, field: string) => Statement.Fragment;
    readonly element: (sql: SqlClient.SqlClient, entry: Column, text: Statement.Fragment) => Statement.Fragment;
    readonly texts: (sql: SqlClient.SqlClient, body: string) => Statement.Fragment;
    readonly insertion: (sql: SqlClient.SqlClient, body: string, order: Statement.Fragment) => Statement.Fragment;
    readonly read: (sql: SqlClient.SqlClient, entry: Column, scope: Statement.Fragment) => Statement.Fragment;
    readonly upsert: (parts: ReturnType<typeof Engine.fragments>, body: string, set: readonly Column[]) => Effect.Effect<readonly SqlConnection.Row[], SqlError.SqlError, SqlClient.SqlClient>;
    readonly ensure: (parts: ReturnType<typeof Engine.fragments>, body: string) => Effect.Effect<readonly SqlConnection.Row[], SqlError.SqlError, SqlClient.SqlClient>;
    readonly search: (parts: ReturnType<typeof Engine.fragments>, searchable: Option.Option.Value<Relation['search']>, text: string, pattern: string, marks: Option.Option<{ readonly open: string; readonly close: string }>, count: number) => Statement.Statement<SqlConnection.Row>;
    readonly constant: (entry: Column, value: string | number) => string;
    readonly codePoints: (sql: SqlClient.SqlClient, subject: Statement.Fragment) => Statement.Fragment;
    readonly text: (sql: SqlClient.SqlClient, subject: Statement.Fragment, kind: Kind) => Statement.Fragment;
    readonly domain: (sql: SqlClient.SqlClient, subject: Statement.Fragment, kind: Kind) => readonly Statement.Fragment[];
    readonly projection: (sql: SqlClient.SqlClient, entry: Column, projected: Extract<Projection, { readonly _tag: 'Path' }>) => Statement.Fragment;
    readonly generated: (sql: SqlClient.SqlClient, expression: Statement.Fragment) => Statement.Fragment;
    readonly constraint: (sql: SqlClient.SqlClient, name: string, text: Statement.Fragment) => Statement.Fragment;
    readonly casefold: (sql: SqlClient.SqlClient, name: string) => Statement.Fragment;
    readonly opclass: (method: Method, kind: Kind) => Option.Option<Option.Option<{ readonly name: string; readonly extensions: readonly string[] }>>;
    readonly projected: (sql: SqlClient.SqlClient, expression: Statement.Fragment, name: string) => Statement.Fragment;
    readonly methods: readonly Method[];
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
            readonly configuration: Option.Option<typeof Pg.Configuration.Type>;
            readonly text: (prefix: string, on: Statement.Fragment) => Statement.Statement<object>;
        }[];
        readonly virtual: Option.Option<{ readonly name: string; readonly create: Statement.Statement<object>; readonly triggers: readonly { readonly name: string; readonly create: Statement.Statement<object> }[]; readonly rebuild: Statement.Statement<object> }>;
    };
    readonly policy: (sql: SqlClient.SqlClient, owner: string, name: string, type: string, setting: string) => Option.Option<{ readonly name: string; readonly rule: Statement.Fragment; readonly text: (on: Statement.Fragment, role: Statement.Fragment) => Statement.Statement<object> }>;
    readonly comment: (sql: SqlClient.SqlClient, name: string, identity: typeof Identity.Type) => { readonly inline: Statement.Fragment; readonly statements: (relation: Statement.Fragment, column: Option.Option<string>) => readonly Statement.Statement<object>[] };
    readonly refusals: (relation: Relation) => readonly (typeof ReconcileError.Finding.Type)[];
    readonly role: (sql: SqlClient.SqlClient) => Effect.Effect<string, SqlError.SqlError | Schema.SchemaError | Cause.NoSuchElementError | Cause.IllegalArgumentError>;
    readonly open: (sql: SqlClient.SqlClient) => Effect.Effect<unknown, SqlError.SqlError>;
    readonly plan: (sql: SqlClient.SqlClient, catalog: Catalog, runtime: Option.Option<string>) => Effect.Effect<Effect.Success<ReturnType<ReturnType<typeof diff>['plan']>>, SqlError.SqlError | Schema.SchemaError | Cause.NoSuchElementError | ReconcileError>;
}

// --- [SERVICES] ------------------------------------------------------------------------

class Engine extends Context.Service<Engine, Shape>()('@rasm/database/internal/engine/engine') {
    static readonly flags = { inserted: '#inserted', previous: '#previous', rank: '#rank', highlight: '#highlight', position: '#position' } as const;
    static readonly layer = Layer.effect(
        Engine,
        SqlClient.SqlClient.use((sql) =>
            sql.onDialectOrElse({
                sqlite: () => Effect.succeed<Shape>(Sqlite),
                pg: () =>
                    Effect.map(
                        Effect.promise(() => import('#/internal/engine/pg')),
                        Struct.get('Pg'),
                    ),
                orElse: () => Effect.fail(new Cause.IllegalArgumentError(`${sql.driver} has no engine`)),
            }),
        ),
    );
    static readonly fragments = (sql: SqlClient.SqlClient, engine: Shape, { name, columns, key }: Pick<Relation, 'name' | 'columns' | 'key'>) => {
        const all = Object.values(columns);
        const written = all.filter(({ projection }) => Option.isNone(projection));
        const [unqualified, owner] = [sql.literal(''), sql`${sql(name)}`];
        const element = (entry: Column) => engine.element(sql, entry, engine.member(sql, entry.field));
        const keys = (scope: Statement.Fragment) => sql.csv(key.map((part) => sql`${scope}${sql(part)}`));
        const order = sql.csv(key.flatMap((part) => written.filter((entry) => entry.name === part).map(element)));
        return {
            sql,
            engine,
            name,
            owner,
            prefix: sql`${owner}${sql.literal('.')}`,
            unqualified,
            order,
            ignore: sql`ON CONFLICT (${keys(unqualified)}) DO NOTHING`,
            flags: Engine.flags,
            element,
            keys,
            reads: (scope: Statement.Fragment, ...extra: readonly (string | Statement.Fragment)[]) => sql.csv([...all.map((entry) => engine.read(sql, entry, scope)), ...extra]),
            inserting: (body: string, conflict: Statement.Fragment, returning: Statement.Fragment) => sql`INSERT INTO ${owner} (${sql.csv(written.map((entry) => sql`${sql(entry.name)}`))}) SELECT ${sql.csv(written.map(element))} FROM ${engine.insertion(sql, body, order)} ${conflict} RETURNING ${returning}`,
            where: (rules: { readonly [field: string]: string | { readonly not: string } | undefined }) =>
                sql.and(
                    all.flatMap((entry) =>
                        Array.fromNullishOr(rules[entry.field]).map((rule) => {
                            const [column, body] = [sql`${sql(entry.name)}`, typeof rule === 'string' ? rule : rule.not];
                            const matched = sql`(${column} IN (SELECT ${engine.element(sql, entry, sql.literal('e.value'))} FROM ${engine.texts(sql, body)}) OR (${column} IS NULL AND EXISTS (SELECT 1 FROM ${engine.texts(sql, body)} WHERE e.value IS NULL)))`;
                            return typeof rule === 'string' ? matched : sql`(${matched}) IS NOT TRUE`;
                        }),
                    ),
                ),
        };
    };
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Engine };
