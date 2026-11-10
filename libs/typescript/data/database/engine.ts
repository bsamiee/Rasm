import { Array, type Cause, Context, Effect, Option, type Schema, String } from 'effect';
import { SqlClient, type SqlConnection, type SqlError, Statement } from 'effect/sql';
import type { Catalog } from '#/catalog';
import type { Column, Kind, Projection } from '#/column';
import type { ReconcileError } from '#/error';
import type { Pg } from '#/pg';
import type { diff, Identity } from '#/reconcile';
import type { Relation } from '#/relation';

// --- [TYPES] ---------------------------------------------------------------------------

type Method = Relation['indexes'][number]['method'];
interface Dialect {
    readonly names: { readonly transformQueryNames?: (name: string) => string; readonly transformResultNames?: (name: string) => string };
    readonly type: (entry: Column) => string;
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
    readonly suffix: (sql: SqlClient.SqlClient, relation: Relation) => Statement.Fragment;
    readonly rename: (sql: SqlClient.SqlClient, from: string, to: Statement.Fragment) => Statement.Statement<object>;
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
            readonly text: (modifiers: readonly string[], on: Statement.Fragment) => Statement.Statement<object>;
        }[];
        readonly virtual: Option.Option<{ readonly name: string; readonly create: Statement.Statement<object>; readonly triggers: readonly { readonly name: string; readonly create: Statement.Statement<object> }[]; readonly rebuild: Statement.Statement<object> }>;
    };
    readonly policy: (sql: SqlClient.SqlClient, owner: string, name: string, type: string, setting: string) => Option.Option<{ readonly name: string; readonly rule: Statement.Fragment; readonly text: (on: Statement.Fragment, role: Statement.Fragment) => Statement.Statement<object> }>;
    readonly comment: (sql: SqlClient.SqlClient, name: string, identity: typeof Identity.Type) => { readonly inline: Statement.Fragment; readonly statements: (relation: Statement.Fragment, column: Option.Option<string>) => readonly Statement.Statement<object>[] };
    readonly refusals: (relation: Relation) => readonly ReconcileError['findings'][number][];
    readonly role: (sql: SqlClient.SqlClient) => Effect.Effect<string, SqlError.SqlError | Schema.SchemaError | Cause.NoSuchElementError | Cause.IllegalArgumentError>;
    readonly open: (sql: SqlClient.SqlClient) => Effect.Effect<unknown, SqlError.SqlError>;
    readonly plan: (sql: SqlClient.SqlClient, catalog: Catalog, runtime: Option.Option<string>) => Effect.Effect<Effect.Success<ReturnType<ReturnType<typeof diff>['plan']>>, SqlError.SqlError | Schema.SchemaError | Cause.NoSuchElementError | ReconcileError>;
    readonly execute: (statements: Array.NonEmptyReadonlyArray<Statement.Statement<object>>) => Effect.Effect<unknown, SqlError.SqlError, SqlClient.SqlClient>;
    readonly transactional: boolean;
}
interface RowDialect {
    readonly member: (sql: SqlClient.SqlClient, field: string) => Statement.Fragment;
    readonly element: (sql: SqlClient.SqlClient, entry: Column, text: Statement.Fragment) => Statement.Fragment;
    readonly texts: (sql: SqlClient.SqlClient, body: string) => Statement.Fragment;
    readonly insertion: (sql: SqlClient.SqlClient, body: string, order: Statement.Fragment) => Statement.Fragment;
    readonly read: (sql: SqlClient.SqlClient, entry: Column, scope: Statement.Fragment) => Statement.Fragment;
    readonly upsert: (parts: ReturnType<typeof Rows.fragments>, body: string, set: readonly Column[]) => Effect.Effect<readonly SqlConnection.Row[], SqlError.SqlError, SqlClient.SqlClient>;
    readonly ensure: (parts: ReturnType<typeof Rows.fragments>, body: string) => Effect.Effect<readonly SqlConnection.Row[], SqlError.SqlError, SqlClient.SqlClient>;
    readonly search: (
        parts: ReturnType<typeof Rows.fragments>,
        searchable: Option.Option.Value<Relation['search']>,
        text: string,
        pattern: string,
        marks: Option.Option<{ readonly open: string; readonly close: string }>,
        count: number,
    ) => Effect.Effect<readonly SqlConnection.Row[], SqlError.SqlError | Schema.SchemaError | Cause.NoSuchElementError>;
    readonly atomic: <A extends object>(statements: Array.NonEmptyReadonlyArray<Statement.Statement<A>>) => Effect.Effect<Array.NonEmptyReadonlyArray<readonly A[]>, SqlError.SqlError, SqlClient.SqlClient>;
}

// --- [SERVICES] ------------------------------------------------------------------------

class Engine extends Context.Service<Engine, Dialect>()('@rasm/database/engine') {
    static readonly names = { snake: { transformQueryNames: String.camelToSnake, transformResultNames: String.snakeToCamel }, verbatim: {} } as const satisfies { readonly [pair: string]: Dialect['names'] };
    static readonly transaction: RowDialect['atomic'] = (statements) => SqlClient.SqlClient.use((sql) => sql.withTransaction(Effect.all(statements)));
}
class Rows extends Context.Service<Rows, RowDialect>()('@rasm/database/engine/Rows') {
    static readonly flags = { inserted: '#inserted', previous: '#previous', rank: '#rank', highlight: '#highlight', position: '#position' } as const;
    static readonly atomic = <A extends object>(statements: Array.NonEmptyReadonlyArray<Statement.Statement<A>>) => Rows.use((rows) => rows.atomic(statements));
    static readonly fragments = (sql: SqlClient.SqlClient, rows: RowDialect, { name, columns, key }: Pick<Relation, 'name' | 'columns' | 'key'>) => {
        const all = Object.values(columns);
        const written = all.filter(({ projection }) => Option.isNone(projection));
        const [unqualified, owner] = [sql.literal(''), sql`${sql(name)}`];
        const element = (entry: Column) => rows.element(sql, entry, rows.member(sql, entry.name));
        const keys = (scope: Statement.Fragment) => sql.csv(key.map((part) => sql`${scope}${sql(part)}`));
        const order = sql.csv(key.flatMap((part) => written.filter((entry) => entry.name === part).map(element)));
        return {
            sql,
            rows,
            name,
            owner,
            prefix: sql`${owner}${sql.literal('.')}`,
            unqualified,
            order,
            ignore: sql`ON CONFLICT (${keys(unqualified)}) DO NOTHING`,
            flags: Rows.flags,
            element,
            keys,
            reads: (scope: Statement.Fragment, ...extra: readonly (string | Statement.Fragment)[]) => sql.csv([...all.map((entry) => rows.read(sql, entry, scope)), ...extra]),
            inserting: (body: string, conflict: Statement.Fragment, returning: Statement.Fragment) =>
                sql`INSERT INTO ${owner} (${sql.csv(written.map((entry) => Statement.fragment([sql(entry.name)])))}) SELECT ${sql.csv(written.map(element))} FROM ${rows.insertion(sql, body, order)} ${conflict} RETURNING ${returning}`,
            where: (rules: { readonly [field: string]: string | { readonly not: string } | undefined }) =>
                sql.and(
                    all.flatMap((entry) =>
                        Array.fromNullishOr(rules[entry.name]).map((rule) => {
                            const [column, body] = [sql`${sql(entry.name)}`, typeof rule === 'string' ? rule : rule.not];
                            const matched = sql`(${column} IN (SELECT ${rows.element(sql, entry, sql.literal('e.value'))} FROM ${rows.texts(sql, body)}) OR (${column} IS NULL AND EXISTS (SELECT 1 FROM ${rows.texts(sql, body)} WHERE e.value IS NULL)))`;
                            return typeof rule === 'string' ? matched : sql`(${matched}) IS NOT TRUE`;
                        }),
                    ),
                ),
        };
    };
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Dialect, Engine, Rows };
