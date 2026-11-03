import { Array, Duration, Effect, Equal, Function, Match, Option, Order, Record, Schema, Struct } from 'effect';
import { type SqlClient, SqlSchema, type Statement } from 'effect/sql';
import type { Catalog } from '#/catalog';
import type { Column, Kind } from '#/column';
import type { Engine } from '#/engine';
import { Reconcile } from '#/reconcile';

// --- [CONSTANTS] -----------------------------------------------------------------------

const trigram = { extension: 'pg_trgm', classes: { gin: 'gin_trgm_ops', gist: 'gist_trgm_ops' } } as const;
const identifiers = /"(?:[^"]|"")*"|[\p{L}_][\p{L}\p{N}_$]*/gu;
const placeholder = '$column';
const confdeltype = { 'NO ACTION': 'a', RESTRICT: 'r', CASCADE: 'c', 'SET NULL': 'n', 'SET DEFAULT': 'd' } as const satisfies { readonly [A in (typeof Reconcile.referentialActions)[number]]: string };

// --- [MODELS] --------------------------------------------------------------------------

const Names = Schema.Array(Schema.String);
const PgTable = Schema.Struct({
    name: Schema.String,
    identity: Schema.NullOr(Schema.String),
    rls: Schema.Boolean,
    force: Schema.Boolean,
    grants: Schema.Array(Schema.Tuple([Schema.String, Schema.String])),
    columns: Schema.Array(Schema.Struct({ name: Schema.String, identity: Schema.NullOr(Schema.String), type: Schema.String, expression: Schema.NullOr(Schema.String), generated: Schema.Literals(['', 's', 'v']), required: Schema.Boolean, sources: Names })),
    checks: Schema.Array(Schema.Struct({ name: Schema.String, definition: Schema.String, columns: Names })),
    references: Schema.Array(Schema.Struct({ name: Schema.String, columns: Schema.NonEmptyArray(Schema.String), target: Schema.String, onDelete: Schema.Literals(Object.values(confdeltype)).transform(Record.keys(confdeltype)) })),
    invalid: Names,
    indexes: Schema.Array(
        Schema.Struct({
            name: Schema.String,
            valid: Schema.Boolean,
            unique: Schema.Boolean,
            method: Schema.String,
            width: Schema.Int,
            keys: Schema.Array(Schema.Struct({ column: Schema.Boolean, text: Schema.String })),
            classes: Names,
            predicate: Schema.NullOr(Schema.String),
            definition: Schema.String,
            columns: Names,
        }),
    ),
    policies: Schema.Array(Schema.Struct({ name: Schema.String, using: Schema.NullOr(Schema.String), check: Schema.NullOr(Schema.String), roles: Names })),
});
class PgPrelude extends Schema.Class<PgPrelude, { readonly brand: unique symbol }>('PgPrelude')({
    migrator: Schema.String,
    absent: Names,
    configurations: Schema.fromJsonString(Schema.Array(Schema.Struct({ name: Schema.String, configuration: Schema.String, configured: Schema.Boolean, drift: Schema.Array(Schema.Struct({ tokens: Schema.NonEmptyArray(Schema.String), dictionaries: Schema.OptionFromNullOr(Schema.NonEmptyArray(Schema.String)) })) }))),
}) {
    static readonly settings = ({ timeouts }: Catalog) =>
        Option.toArray(timeouts).flatMap(({ lock, transaction }) => [
            { name: 'lock_timeout', value: `${Duration.toMillis(lock)}ms` },
            { name: 'transaction_timeout', value: `${Duration.toMillis(transaction)}ms` },
        ]);
    static readonly open = Effect.fnUntraced(function* (sql: SqlClient.SqlClient, preamble: readonly Statement.Statement<object>[], required: readonly string[], filters: readonly string[], configurations: typeof Pg.Configurations.Type) {
        yield* Effect.all(preamble, { discard: true });
        const { search_path: path } = yield* SqlSchema.findOne({
            Request: Schema.Struct({ required: Names, filters: Names }),
            Result: Schema.Struct({ search_path: Schema.String }),
            execute: (names) =>
                sql`SELECT pg_advisory_xact_lock(hashtextextended(current_schema(), 0))::text AS lock, set_config('search_path', concat_ws(', ', quote_ident(current_schema()),
                            (SELECT string_agg(quote_ident(n.nspname), ', ' ORDER BY n.nspname) FROM pg_namespace n WHERE n.oid NOT IN (current_schema()::regnamespace, 'pg_catalog'::regnamespace)
                                AND (EXISTS (SELECT 1 FROM pg_extension e WHERE e.extnamespace = n.oid AND e.extname = ANY(${names.required}::text[])) OR EXISTS (SELECT 1 FROM pg_ts_dict d WHERE d.dictnamespace = n.oid AND d.dictname = ANY(${names.filters}::text[])))), 'pg_temp'), true) AS search_path`,
        })({ required, filters });
        const prelude = yield* SqlSchema.findOne({
            Request: Schema.Struct({ required: Names, filters: Names, declared: Pg.Configurations }),
            Result: PgPrelude,
            execute: (names) =>
                sql`WITH maps AS (SELECT m.mapcfg, m.maptokentype, array_agg(m.mapdict::regdictionary::text ORDER BY m.mapseqno) AS dictionaries FROM pg_ts_config_map m GROUP BY m.mapcfg, m.maptokentype),
                            declared AS (SELECT d.name, d.configuration::regconfig::oid AS base, (SELECT c.oid FROM pg_ts_config c WHERE c.cfgnamespace = current_schema()::regnamespace AND c.cfgname = d.name) AS live, array(SELECT quote_ident(f) FROM unnest(d.filters) f) AS filters FROM json_to_recordset(${names.declared}::json) d(name text, configuration text, filters text[])),
                            tokens AS (SELECT d.name, t.tokid, t.alias, CASE WHEN b.dictionaries IS NOT NULL THEN d.filters || b.dictionaries END AS wanted, l.dictionaries AS now
                                FROM declared d JOIN pg_ts_config c ON c.oid = d.base CROSS JOIN ts_token_type(c.cfgparser) t
                                LEFT JOIN maps b ON (b.mapcfg, b.maptokentype) = (d.base, t.tokid) LEFT JOIN maps l ON (l.mapcfg, l.maptokentype) = (coalesce(d.live, d.base), t.tokid))
                            SELECT current_user::text AS migrator,
                                array(SELECT a.name FROM (SELECT x FROM unnest(${names.required}::text[]) x WHERE NOT EXISTS (SELECT 1 FROM pg_extension e WHERE e.extname = x)
                                    UNION SELECT f FROM unnest(${names.filters}::text[]) f WHERE NOT EXISTS (SELECT 1 FROM pg_ts_dict d WHERE d.dictname = f) AND EXISTS (SELECT 1 FROM pg_available_extensions v WHERE v.name = f)) a(name) ORDER BY a.name) AS absent, array_to_json(array(SELECT json_build_object('name', d.name, 'configuration', d.base::regconfig::text, 'configured', d.live IS NOT NULL,
                                'drift', array(SELECT json_build_object('tokens', array_agg(k.alias ORDER BY k.tokid), 'dictionaries', k.wanted) FROM tokens k WHERE k.name = d.name AND k.now IS DISTINCT FROM k.wanted GROUP BY k.wanted)) FROM declared d))::text AS configurations`,
        })({ required, filters, declared: configurations });
        return { prelude, path };
    });
}
class Pg extends Schema.Class<Pg, { readonly brand: unique symbol }>('Pg')({ tables: Schema.Array(PgTable), scratch: Schema.Array(PgTable) }) {
    static readonly Configuration = Schema.Struct({ name: Schema.String, configuration: Schema.String, filters: Names });
    static readonly Configurations = Schema.fromJsonString(Schema.Array(Pg.Configuration));

    // --- [ROWS]
    static readonly dialect = 'pg';
    static readonly types = { text: 'text', uuid: 'uuid', json: 'jsonb', instant: 'timestamptz(3)', integer: 'bigint', decimal: 'numeric', real: 'double precision', boolean: 'boolean', blob: 'bytea' } as const satisfies Engine['Service']['types'];
    static readonly member: Engine['Service']['member'] = (sql, field) => sql`e.value ->> ${sql.literal(Reconcile.literal(field))}`;
    static readonly element: Engine['Service']['element'] = (sql, { kind }, text) => (kind === 'blob' ? sql`decode(${text}, 'hex')` : sql`(${text})::${sql.literal(Pg.types[kind])}`);
    static readonly rows = (sql: SqlClient.SqlClient, body: string) => sql`jsonb_array_elements(${body}) e(value)`;
    static readonly texts: Engine['Service']['texts'] = (sql, body) => sql`jsonb_array_elements_text(${body}) WITH ORDINALITY e(value, position)`;
    static readonly insertion: Engine['Service']['insertion'] = (sql, body, order) => sql`${Pg.rows(sql, body)} ORDER BY ${order}`;
    static readonly read: Engine['Service']['read'] = (sql, { name, textual }, scope) => (textual ? sql`${scope}${sql(name)}::text AS ${sql(name)}` : sql`${scope}${sql(name)}`);
    static readonly upsert: Engine['Service']['upsert'] = ({ sql, owner, unqualified, ignore, keys, reads, inserting, flags }, body, set) => {
        const parts = set.map(({ name }) => sql(name));
        const update = sql`ON CONFLICT (${keys(unqualified)}) DO UPDATE SET ${sql.csv(parts.map((part) => sql`${part} = excluded.${part}`))} WHERE (${sql.csv(parts.map((part) => sql`${owner}.${part}`))}) IS DISTINCT FROM (${sql.csv(parts.map((part) => sql`excluded.${part}`))})`;
        return inserting(body, Array.isReadonlyArrayNonEmpty(parts) ? update : ignore, sql.csv(`WITH (OLD AS "${flags.previous}")`, [reads(unqualified), sql`${sql(flags.previous)} IS NULL AS ${sql(flags.inserted)}`]));
    };
    static readonly ensure: Engine['Service']['ensure'] = ({ sql, owner, unqualified, order, ignore, keys, reads, inserting }, body) => {
        const outer = sql.literal('t.');
        return sql`WITH inserted AS (${inserting(body, ignore, sql.literal('*'))}) SELECT ${reads(outer)} FROM (SELECT * FROM inserted UNION ALL SELECT * FROM ${owner} WHERE (${keys(unqualified)}) IN (SELECT ${order} FROM ${Pg.rows(sql, body)})) t ORDER BY ${keys(outer)}`;
    };
    static readonly configuration = ({ configuration, filters }: { readonly configuration: string; readonly filters: readonly string[] }) => Reconcile.identifier([configuration, ...filters]);
    static readonly document = (sql: SqlClient.SqlClient, { columns, tokenizer }: Parameters<Engine['Service']['fulltext']>[2]) => {
        const joined = sql.join(` || ' ' || `, false)(columns.map(({ name }) => sql`coalesce(${sql(name)}, '')`));
        return tokenizer === 'trigram' ? joined : sql`to_tsvector(${sql.literal(Reconcile.literal(Pg.configuration(tokenizer)))}, ${joined})`;
    };
    static readonly search: Engine['Service']['search'] = ({ sql, owner, prefix, unqualified, keys, reads, flags }, searchable, text, pattern, marks, count) => {
        const { columns: searched, tokenizer } = searchable;
        const body = Pg.document(sql, searchable);
        const [rank, highlight] = [sql(flags.rank), sql(flags.highlight)];
        const marked = (mark: (name: string, open: string, close: string) => Statement.Fragment) => Option.match(marks, { onNone: () => sql.literal('NULL'), onSome: ({ open, close }) => sql`jsonb_build_object(${sql.csv(searched.flatMap((entry) => [Reconcile.literal(entry.field), mark(entry.name, open, close)]))})` });
        return tokenizer === 'trigram'
            ? sql`SELECT ${reads(unqualified)}, word_similarity(${text}, ${body}) AS ${rank}, ${marked((part, open, close) => sql`regexp_replace(${sql(part)}, ${`***=${text}`}, ${`${open.replaceAll('\\', '\\\\')}\\&${close.replaceAll('\\', '\\\\')}`}, 'gi')`)} AS ${highlight} FROM ${owner} WHERE (${body}) ILIKE ${pattern} ORDER BY ${rank} DESC, ${keys(prefix)} LIMIT ${count}`
            : sql`SELECT ${reads(unqualified)}, ts_rank(${body}, q) AS ${rank}, ${marked((part, open, close) => sql`ts_headline(${sql.literal(Reconcile.literal(Pg.configuration(tokenizer)))}, ${sql(part)}, q, ${`StartSel="${open.replaceAll('"', '""')}", StopSel="${close.replaceAll('"', '""')}", HighlightAll=true`})`)} AS ${highlight} FROM ${owner} CROSS JOIN plainto_tsquery(${sql.literal(Reconcile.literal(Pg.configuration(tokenizer)))}, ${text}) q WHERE ${body} @@ q ORDER BY ${rank} DESC, ${keys(prefix)} LIMIT ${count}`;
    };

    // --- [DDL]
    static readonly constant: Engine['Service']['constant'] = (entry, value) => (entry.kind === 'boolean' ? `${value === 1}` : Reconcile.constant(entry, value));
    static readonly codePoints: Engine['Service']['codePoints'] = (sql, subject) => sql`char_length(${subject})`;
    static readonly text: Engine['Service']['text'] = (sql, subject, kind) => (kind === 'uuid' ? sql`${subject}::text` : subject);
    static readonly domain: Engine['Service']['domain'] = (sql, subject, kind) =>
        Match.value(kind).pipe(
            Match.whenOr('real', 'decimal', () => [sql`${subject} NOT IN ('NaN', 'Infinity', '-Infinity')`]),
            Match.when('instant', () => [sql`isfinite(${subject})`]),
            Match.orElse(() => []),
        );
    static readonly projection: Engine['Service']['projection'] = (sql, { kind }, { from, path }) => sql`(${sql(from)} ${sql.literal(kind === 'json' ? '#>' : '#>>')} ARRAY[${sql.literal(path.map(Reconcile.literal).join(', '))}])${sql.literal(['json', 'text'].includes(kind) ? '' : `::${Pg.types[kind]}`)}`;
    static readonly generated: Engine['Service']['generated'] = (sql, expression) => sql`GENERATED ALWAYS AS (${expression}) VIRTUAL`;
    static readonly constraint: Engine['Service']['constraint'] = (sql, name, text) => sql`CONSTRAINT ${sql(name)} ${text}`;
    static readonly casefold: Engine['Service']['casefold'] = (sql, name) => sql`casefold(${sql(name)} COLLATE pg_unicode_fast)`;
    static readonly opclass: Engine['Service']['opclass'] = (method, kind) =>
        Match.value({ method, kind }).pipe(
            Match.when({ method: 'btree' }, () => Option.none()),
            Match.when({ method: Match.is('gin', 'gist'), kind: 'text' }, ({ method: access }) => Option.some({ name: trigram.classes[access], extensions: [trigram.extension] })),
            Match.when({ method: 'gin', kind: 'json' }, () => Option.some({ name: 'jsonb_path_ops', extensions: [] })),
            Match.when({ method: 'brin', kind: (value: Kind) => value !== 'json' && value !== 'boolean' }, () => Option.none()),
            Match.option,
        );
    static readonly projected: Engine['Service']['projected'] = (sql, expression) => sql`(${expression})`;
    static readonly methods: Engine['Service']['methods'] = Reconcile.methods;
    static readonly covering: Engine['Service']['covering'] = (sql, _, include) => ({ keys: [], clause: Array.isReadonlyArrayNonEmpty(include) ? [sql`INCLUDE (${sql.csv(include.map((name) => sql`${sql(name)}`))})`] : [] });
    static readonly suffix: Engine['Service']['suffix'] = () => '';
    static readonly fulltext: Engine['Service']['fulltext'] = (sql, owner, search) => {
        const [name, body] = [Reconcile.identifier([owner, 'search']), Pg.document(sql, search)];
        const words = search.tokenizer === 'trigram' ? Option.none() : Option.some({ ...search.tokenizer, name: Pg.configuration(search.tokenizer) });
        return {
            indexes: [
                {
                    name,
                    parts: ['search'],
                    extensions: Option.match(words, { onNone: () => [trigram.extension], onSome: () => [] }),
                    configuration: Option.filter(words, ({ filters }) => Array.isReadonlyArrayNonEmpty(filters)),
                    text: (prefix, on) => sql`CREATE INDEX ${sql.literal(prefix)}${sql(name)} ON ${on} USING gin (${Option.match(words, { onNone: () => sql`(${body}) ${sql.literal(trigram.classes.gin)}`, onSome: () => body })})`,
                },
            ],
            virtual: Option.none(),
        };
    };
    static readonly policy: Engine['Service']['policy'] = (sql, owner, name, type, setting) => {
        const [policy, rule] = [Reconcile.identifier([owner, 'tenant']), sql`${sql(name)} = nullif(current_setting(${sql.literal(Reconcile.literal(setting))}, true), '')::${sql.literal(type)}`];
        return Option.some({ name: policy, rule, text: (on, role) => sql`CREATE POLICY ${sql(policy)} ON ${on} TO ${role} USING (${rule}) WITH CHECK (${rule})` });
    };
    static readonly commentTarget = (sql: SqlClient.SqlClient, relation: Statement.Fragment, column: Option.Option<string>) => Option.match(column, { onNone: () => sql`TABLE ${relation}`, onSome: (name) => sql`COLUMN ${relation}.${sql(name)}` });
    static readonly comment: Engine['Service']['comment'] = (sql, name, identity) => ({
        inline: sql.literal(''),
        statements: (relation, column) => (identity.origin === name && identity.generation === 0 && identity.digest === undefined ? [] : [sql`COMMENT ON ${Pg.commentTarget(sql, relation, column)} IS ${sql.literal(Reconcile.literal(JSON.stringify(identity)))}`]),
    });

    // --- [RECONCILE]
    static readonly refusals: Engine['Service']['refusals'] = ({ name: owner, columns, indexes }) => {
        const all = Object.values(columns);
        return Reconcile.unsupported(owner, 'opclass', Array.dedupe(indexes.flatMap(({ keys, method }) => keys.flatMap(({ column, casefold }) => all.filter((entry) => entry.name === column && Option.isNone(Pg.opclass(method, casefold ? 'text' : entry.kind)))))), Function.constTrue);
    };
    static readonly role: Engine['Service']['role'] = (sql) => Effect.map(SqlSchema.findOne({ Request: Schema.Void, Result: Schema.Struct({ role: Schema.String }), execute: () => sql`SELECT current_user AS role` })(undefined), Struct.get('role'));
    static readonly open: Engine['Service']['open'] = () => Effect.void;
    static readonly observe = (sql: SqlClient.SqlClient, excludedPrefixes: readonly string[]) =>
        Effect.map(
            SqlSchema.findOne({
                Request: Names,
                Result: Schema.Struct({ observed: Schema.fromJsonString(Pg) }),
                execute: (prefixes) =>
                    sql`WITH relations AS (SELECT c.oid, c.relname, c.relowner, c.relacl, c.relrowsecurity, c.relforcerowsecurity, c.relnamespace = pg_my_temp_schema() AS scratch FROM pg_class c WHERE c.relnamespace IN (current_schema()::regnamespace, pg_my_temp_schema()) AND c.relkind = 'r' AND NOT EXISTS (SELECT 1 FROM unnest(${prefixes}::text[]) p(prefix) WHERE starts_with(c.relname::text, p.prefix))),
                    constraints AS (SELECT n.oid, n.conrelid, n.conname, n.contype, n.convalidated, n.confrelid, n.confdeltype,
                        array(SELECT a.attname::text FROM unnest(n.conkey) WITH ORDINALITY k(number, position) JOIN pg_attribute a ON (a.attrelid, a.attnum) = (n.conrelid, k.number) ORDER BY k.position) AS columns
                        FROM pg_constraint n JOIN relations r ON r.oid = n.conrelid),
                    described AS (SELECT r.scratch, json_build_object('name', r.relname, 'identity', obj_description(r.oid, 'pg_class'), 'rls', r.relrowsecurity, 'force', r.relforcerowsecurity,
                        'grants', array(SELECT json_build_array(a.grantee::regrole::text, a.privilege_type) FROM aclexplode(r.relacl) a WHERE a.grantee <> r.relowner),
                        'columns', array(SELECT json_build_object('name', a.attname, 'identity', col_description(a.attrelid, a.attnum), 'type', format_type(a.atttypid, a.atttypmod), 'expression', pg_get_expr(d.adbin, d.adrelid), 'generated', a.attgenerated::text,
                            'required', EXISTS (SELECT 1 FROM pg_constraint n WHERE n.conrelid = a.attrelid AND n.contype = 'n' AND n.conkey = ARRAY[a.attnum] AND n.convalidated),
                            'sources', array(SELECT s.attname::text FROM pg_depend p JOIN pg_attribute s ON (s.attrelid, s.attnum) = (p.refobjid, p.refobjsubid) WHERE p.classid = 'pg_attrdef'::regclass AND p.objid = d.oid AND p.refobjsubid > 0 AND s.attnum <> a.attnum))
                            FROM pg_attribute a LEFT JOIN pg_attrdef d ON (d.adrelid, d.adnum) = (a.attrelid, a.attnum) WHERE a.attrelid = r.oid AND a.attnum > 0 AND NOT a.attisdropped ORDER BY a.attnum),
                        'checks', array(SELECT json_build_object('name', n.conname, 'definition', pg_get_constraintdef(n.oid, true), 'columns', n.columns) FROM constraints n WHERE n.conrelid = r.oid AND n.contype = 'c'),
                        'references', array(SELECT json_build_object('name', n.conname, 'columns', n.columns, 'target', t.relname, 'onDelete', n.confdeltype::text) FROM constraints n JOIN pg_class t ON t.oid = n.confrelid WHERE n.conrelid = r.oid AND n.contype = 'f'),
                        'invalid', array(SELECT n.conname::text FROM constraints n WHERE n.conrelid = r.oid AND NOT n.convalidated),
                        'indexes', array(SELECT json_build_object('name', i.relname, 'valid', x.indisvalid, 'unique', x.indisunique, 'method', am.amname, 'width', x.indnkeyatts,
                            'keys', array(SELECT json_build_object('column', x.indkey[k - 1] <> 0, 'text', coalesce((SELECT a.attname::text FROM pg_attribute a WHERE a.attrelid = x.indrelid AND a.attnum = x.indkey[k - 1]), pg_get_indexdef(x.indexrelid, k, true))) FROM generate_series(1, x.indnatts) k ORDER BY k),
                            'classes', array(SELECT o.opcname::text FROM unnest(x.indclass) WITH ORDINALITY u(oid, position) JOIN pg_opclass o ON o.oid = u.oid ORDER BY u.position),
                            'predicate', pg_get_expr(x.indpred, x.indrelid, true), 'definition', pg_get_indexdef(x.indexrelid),
                            'columns', array(SELECT a.attname::text FROM pg_depend p JOIN pg_attribute a ON (a.attrelid, a.attnum) = (p.refobjid, p.refobjsubid) WHERE p.objid = x.indexrelid AND p.classid = 'pg_class'::regclass AND p.refobjsubid > 0))
                            FROM pg_index x JOIN pg_class i ON i.oid = x.indexrelid JOIN pg_am am ON am.oid = i.relam WHERE x.indrelid = r.oid AND NOT EXISTS (SELECT 1 FROM pg_constraint k WHERE k.conindid = x.indexrelid AND k.conrelid = x.indrelid)),
                        'policies', array(SELECT json_build_object('name', p.polname, 'using', pg_get_expr(p.polqual, p.polrelid, true), 'check', pg_get_expr(p.polwithcheck, p.polrelid, true), 'roles', p.polroles::regrole[]::text[]) FROM pg_policy p WHERE p.polrelid = r.oid)) AS relation
                        FROM relations r)
                    SELECT json_build_object('tables', array(SELECT d.relation FROM described d WHERE NOT d.scratch), 'scratch', array(SELECT d.relation FROM described d WHERE d.scratch))::text AS observed`,
            })(excludedPrefixes),
            Struct.get('observed'),
        );
    static readonly plan: Engine['Service']['plan'] = Effect.fnUntraced(function* (sql: SqlClient.SqlClient, catalog: Catalog, runtime: Option.Option<string>) {
        const rendered = catalog.tables.map((declared) => ({ declared, rendered: Reconcile.render(sql, Pg, catalog, declared) }));
        const indexes = rendered.flatMap(({ rendered: table }) => table.indexes);
        const required = Array.dedupe(indexes.flatMap(Struct.get('extensions')));
        const owned = Array.dedupeWith(Array.getSomes(indexes.map(({ configuration }) => configuration)), (self, that) => self.name === that.name);
        const settings = PgPrelude.settings(catalog);
        const preamble = settings.map(({ name, value }) => sql`SET LOCAL ${sql.literal(name)} = ${sql.literal(Reconcile.literal(value))}`);
        const { prelude, path } = yield* PgPrelude.open(sql, preamble, required, Array.dedupe(owned.flatMap(Struct.get('filters'))), owned);
        const { migrator, absent, configurations } = prelude;
        const [existing, drifted] = [configurations.filter(Struct.get('configured')).map(Struct.get('name')), configurations.filter(({ drift }) => Array.isReadonlyArrayNonEmpty(drift)).map(Struct.get('name'))];
        const ready = ({ extensions: needed, configuration }: (typeof indexes)[number]) => !needed.some((name) => absent.includes(name)) && Option.toArray(configuration).every(({ name }) => existing.includes(name));
        const role = Option.match(runtime, { onNone: () => sql.literal('PUBLIC'), onSome: (name) => sql`${sql(name)}` });
        const block = (statements: readonly Statement.Statement<object>[]) => sql`DO ${sql.literal(Reconcile.literal(`BEGIN ${statements.map((statement) => `${statement.compile(true)[0]};`).join(' ')} END`))}`;
        const scratched = rendered.map(({ declared }) => Reconcile.render(sql, Pg, catalog, declared, { target: (name) => sql`pg_temp.${sql(name)}`, inline: Function.constFalse }));
        yield* block(scratched.flatMap((table) => [sql`${table.create} ON COMMIT DROP`, ...table.indexes.filter(ready).map(({ text }) => text('')), ...Option.toArray(table.policy).map(({ text }) => text(role))]));
        const live = yield* Pg.observe(sql, catalog.excludedPrefixes);
        const resolved = Reconcile.diff(sql, catalog, rendered, { tables: live.tables, columns: ({ columns }) => columns, references: ({ references }) => references });
        const populated = yield* resolved.populated;
        const privileges = [...Option.toArray(runtime).map((grantee) => ({ grantee, all: ['SELECT', 'INSERT', 'UPDATE', 'DELETE'] })), ...Option.toArray(catalog.reader).map((grantee) => ({ grantee, all: ['SELECT'] }))].filter(({ grantee }) => grantee !== migrator);
        const grants = (on: Statement.Fragment, held: readonly (readonly [string, string])[]) =>
            privileges.flatMap(({ grantee, all }) => {
                const missing = Array.difference(
                    all,
                    held.filter(([holder]) => holder === grantee).map(([, granted]) => granted),
                );
                return Array.isReadonlyArrayNonEmpty(missing) ? [sql`GRANT ${sql.literal(missing.join(', '))} ON ${on} TO ${sql(grantee)}`] : [];
            });
        const stamped = (relation: Statement.Fragment, column: Option.Option<string>, name: string, recorded: Option.Option<typeof Reconcile.Identity.Type>, identity: typeof Reconcile.Identity.Type) =>
            Array.match(Pg.comment(sql, name, identity).statements(relation, column), {
                onEmpty: () => (Option.isSome(recorded) ? [sql`COMMENT ON ${Pg.commentTarget(sql, relation, column)} IS NULL`] : []),
                onNonEmpty: (writes) => (Option.exists(recorded, ({ origin, generation, digest }) => origin === identity.origin && generation === identity.generation && digest === identity.digest) ? [] : writes),
            });
        const shape = (row: (typeof PgTable.Type)['indexes'][number], rename: (column: string) => string) => ({ unique: row.unique, method: row.method, width: row.width, classes: row.classes, predicate: row.predicate, keys: row.keys.map(({ column, text }) => (column ? rename(text) : text)) });
        const checks = ({ checks: rows }: typeof PgTable.Type, column: string) =>
            Array.sort(
                rows.filter(({ columns }) => Equal.equals(columns, [column])).map(({ definition }) => definition.replaceAll(identifiers, (token) => (token === column || token === `"${column.replaceAll('"', '""')}"` ? placeholder : token))),
                Order.String,
            );
        const drifts = (table: typeof PgTable.Type, desired: typeof PgTable.Type, now: (typeof PgTable.Type)['columns'][number], definition: { readonly name: string; readonly digest: number; readonly projection: Column['projection'] }, digested: Option.Option<number>) =>
            Option.exists(
                Array.findFirst(desired.columns, (row) => row.name === definition.name),
                (wants) =>
                    now.type !== wants.type ||
                    now.required !== wants.required ||
                    now.generated !== wants.generated ||
                    (wants.generated === '' && now.expression !== wants.expression) ||
                    Option.match(digested, {
                        onNone: () => !Equal.equals(checks(table, now.name), checks(desired, definition.name)),
                        onSome: (digest) => Option.isNone(definition.projection) && digest !== definition.digest,
                    }),
            );
        const analyze = (entry: (typeof resolved.matched)[number], desired: typeof PgTable.Type) => {
            const { name } = entry.declared;
            const owner = entry.rendered.relation;
            const prior = (parts: readonly string[]) => Reconcile.identifier([entry.live.name, ...parts.map(entry.original)]);
            const named = [...entry.live.checks, ...entry.live.references].map(({ name: constraint }) => constraint);
            const relabels = [...Array.getSomes(entry.rendered.definitions.map(({ check }) => check)), ...entry.rendered.links].filter(({ name: target, parts }) => named.includes(prior(parts)) && prior(parts) !== target && !named.includes(target)).map(({ name: target, parts }) => ({ from: prior(parts), to: target }));
            const relabeled = (constraint: string) => relabels.find(({ from }) => from === constraint)?.to ?? constraint;
            const links = entry.live.references.map((row) => ({ ...row, name: relabeled(row.name), columns: Array.map(row.columns, entry.renamed), target: resolved.referenced(row.target).name }));
            const stale = entry.rendered.links.filter((link) => !links.some((row) => row.name === link.name && Equal.equals(row.columns, link.columns) && row.target === link.target && row.onDelete === link.onDelete));
            const planned = (index: (typeof entry.rendered.indexes)[number]) => {
                const target = Array.findFirst(desired.indexes, (row) => row.name === index.name);
                const fits = (row: (typeof entry.live.indexes)[number]) => row.valid && Option.exists(target, (wanted) => Equal.equals(shape(row, entry.renamed), shape(wanted, Function.identity)));
                const present = Array.findFirst(entry.live.indexes, (row) => row.name === index.name);
                const before = Array.findFirst(entry.live.indexes, (row) => row.name === prior(index.parts) && row.name !== index.name);
                const [keep, rename] = [Option.exists(present, fits), Option.isNone(present) && Option.exists(before, fits)];
                return { index, keep, rename, dropped: [...Option.toArray(present).filter(() => !keep), ...Option.toArray(before).filter(() => !rename)].map((row) => row.name) };
            };
            const plans = entry.rendered.indexes.map(planned);
            const policy = (rule: Option.Option.Value<typeof entry.rendered.policy>) => {
                const current = Array.findFirst(entry.live.policies, (row) => row.name === rule.name);
                const before = Array.findFirst(entry.live.policies, (row) => row.name === prior(['tenant']) && row.name !== rule.name && Option.isNone(current));
                const [present, wanted] = [Option.orElse(current, () => before), Array.findFirst(desired.policies, (row) => row.name === rule.name)];
                return [
                    ...Option.toArray(before).map((row) => sql`ALTER POLICY ${sql(row.name)} ON ${owner} RENAME TO ${sql(rule.name)}`),
                    ...Option.match(present, { onNone: () => [rule.text(role)], onSome: (row) => (Option.contains(wanted, { ...row, name: rule.name }) ? [] : [sql`ALTER POLICY ${sql(rule.name)} ON ${owner} TO ${role} USING (${rule.rule}) WITH CHECK (${rule.rule})`]) }),
                ];
            };
            const pairs = (entry.owned ? entry.fields.filter(({ definition }) => definition.retype) : []).flatMap(({ definition: column, found }) => Option.toArray(Option.all({ wants: Array.findFirst(desired.columns, (row) => row.name === column.name), now: found })).map((pair) => ({ column, ...pair })));
            const moved = pairs.filter(({ wants, now }) => now.type !== wants.type);
            const constrained = (rows: readonly (typeof PgTable.Type)['checks'][number][]) => rows.filter((row) => pairs.some(({ column }) => Equal.equals(row.columns, [column.name])));
            const [had, needs] = [constrained(entry.live.checks.map((row) => ({ ...row, name: relabeled(row.name), columns: row.columns.map(entry.renamed) }))), constrained(desired.checks)];
            const unmatched = Array.differenceWith<(typeof had)[number]>((self, that) => self.definition === that.definition);
            const [obsolete, missing] = [unmatched(had, needs), unmatched(needs, had)];
            const dependents = entry.live.columns.filter((row) => row.generated !== '' && moved.some(({ now }) => row.sources.includes(now.name)));
            const tightened = pairs.filter(({ wants, now }) => wants.required && !now.required).map(({ column }) => ({ column: column.name, constraint: Reconcile.identifier([name, column.name, 'not', 'null']) }));
            const changes = pairs.flatMap(({ column, wants, now }) => [
                ...(now.type === wants.type ? [] : [sql`ALTER COLUMN ${sql(column.name)} TYPE ${sql.literal(wants.type)} USING ${sql(column.name)}::${sql.literal(wants.type)}`]),
                ...(wants.generated !== '' || now.expression === wants.expression ? [] : [Option.match(Option.fromNullOr(wants.expression), { onNone: () => sql`ALTER COLUMN ${sql(column.name)} DROP DEFAULT`, onSome: (expression) => sql`ALTER COLUMN ${sql(column.name)} SET DEFAULT ${sql.literal(expression)}` })]),
                ...(now.required && !wants.required ? [sql`ALTER COLUMN ${sql(column.name)} DROP NOT NULL`] : []),
            ]);
            const retyping = [
                ...obsolete.map((row) => sql`ALTER TABLE ${owner} DROP CONSTRAINT ${sql(row.name)}`),
                ...dependents.map((row) => sql`ALTER TABLE ${owner} DROP COLUMN ${sql(row.name)}`),
                ...moved
                    .filter(({ now }) => now.type !== 'text')
                    .map(({ column, wants, now }) =>
                        block([
                            sql`IF EXISTS (SELECT 1 FROM ${owner} WHERE (${sql(column.name)}::${sql.literal(wants.type)})::${sql.literal(now.type)} IS DISTINCT FROM ${sql(column.name)}) THEN RAISE EXCEPTION 'ALTER COLUMN %.% TYPE % changes stored values', ${sql.literal(Reconcile.literal(name))}, ${sql.literal(Reconcile.literal(column.name))}, ${sql.literal(Reconcile.literal(wants.type))} USING ERRCODE = 'check_violation'; END IF`,
                        ]),
                    ),
                ...Array.match(changes, { onEmpty: () => [], onNonEmpty: (all) => [sql`ALTER TABLE ${owner} ${sql.csv(all)}`] }),
                ...tightened.map(({ column, constraint }) => sql`ALTER TABLE ${owner} ADD CONSTRAINT ${sql(constraint)} NOT NULL ${sql(column)} NOT VALID`),
                ...missing.map((row) => sql`ALTER TABLE ${owner} ADD CONSTRAINT ${sql(row.name)} ${sql.literal(row.definition)} NOT VALID`),
                ...dependents.flatMap((row) => [
                    sql`ALTER TABLE ${owner} ADD COLUMN ${sql(row.name)} ${sql.literal(row.type)} GENERATED ALWAYS AS (${sql.literal(`${row.expression}`)}) ${sql.literal(row.generated === 's' ? 'STORED' : 'VIRTUAL')}`,
                    ...Array.fromNullishOr(row.identity).map((comment) => sql`COMMENT ON COLUMN ${owner}.${sql(row.name)} IS ${sql.literal(Reconcile.literal(comment))}`),
                ]),
                ...entry.live.indexes.filter((index) => dependents.some((row) => index.columns.includes(row.name))).map(({ definition }) => sql`${sql.literal(definition)}`),
            ];
            const added = entry.added.flatMap((definition) => desired.columns.filter((row) => row.name === definition.name).map((row) => ({ definition, row })));
            const adding = added.map(({ definition }) => definition.name);
            const additions = desired.checks.filter((row) => row.columns.every((part) => adding.includes(part)));
            const tenant = Option.isSome(entry.declared.tenant);
            return {
                ...entry,
                relabels: [...relabels.map(({ from, to }) => sql`ALTER TABLE ${owner} RENAME CONSTRAINT ${sql(from)} TO ${sql(to)}`), ...plans.filter(({ rename }) => rename).map(({ index }) => sql`ALTER INDEX ${sql(prior(index.parts))} RENAME TO ${sql(index.name)}`)],
                statements: [
                    ...entry.dropping.map((row) => sql`ALTER TABLE ${owner} DROP COLUMN ${sql(row.name)}`),
                    ...added.map(
                        ({ definition, row }) =>
                            sql`ALTER TABLE ${owner} ADD COLUMN ${sql(definition.name)} ${sql.literal(row.type)}${sql.literal(row.generated === 'v' ? ` GENERATED ALWAYS AS (${row.expression}) VIRTUAL` : '')}${sql.literal(definition.nullable ? '' : ' NOT NULL')}${sql.literal(row.generated === '' && row.expression !== null ? ` DEFAULT ${row.expression}` : '')}`,
                    ),
                    ...additions.map((row) => sql`ALTER TABLE ${owner} ADD CONSTRAINT ${sql(row.name)} ${sql.literal(row.definition)} NOT VALID`),
                    ...retyping,
                    ...stale.flatMap((link) => [...(links.some((row) => row.name === link.name) ? [sql`ALTER TABLE ${owner} DROP CONSTRAINT ${sql(link.name)}`] : []), sql`ALTER TABLE ${owner} ADD CONSTRAINT ${sql(link.name)} ${link.text} NOT VALID`]),
                    ...grants(owner, entry.live.grants),
                    ...(tenant && !entry.live.rls ? [sql`ALTER TABLE ${owner} ENABLE ROW LEVEL SECURITY`] : []),
                    ...(tenant && !entry.live.force ? [sql`ALTER TABLE ${owner} FORCE ROW LEVEL SECURITY`] : []),
                    ...Option.match(entry.rendered.policy, { onNone: () => [], onSome: policy }),
                    ...stamped(owner, Option.none(), name, entry.recorded, entry.identity),
                    ...entry.fields.filter(({ found }) => Option.isSome(found)).flatMap(({ definition, recorded, identity }) => stamped(owner, Option.some(definition.name), definition.name, recorded, { ...identity, digest: definition.digest })),
                    ...added.flatMap(({ definition }) => definition.comment.statements(owner, Option.some(definition.name))),
                ],
                dropped: [...plans.flatMap(({ dropped }) => dropped), ...entry.live.indexes.filter((row) => entry.retiredIndexes.includes(row.name)).map((row) => row.name)],
                built: plans.filter(({ keep, rename }) => !(keep || rename)).map(({ index }) => index.text('CONCURRENTLY IF NOT EXISTS ')),
                reindexed: plans.filter(({ index, keep, rename }) => (keep || rename) && Option.exists(index.configuration, (configuration) => drifted.includes(configuration.name))).map(({ index }) => sql`REINDEX INDEX CONCURRENTLY ${sql(index.name)}`),
                validated: [...entry.live.invalid.map(relabeled), ...additions.map((row) => row.name), ...tightened.map(({ constraint }) => constraint), ...missing.map((row) => row.name), ...stale.map((link) => link.name)],
                undeclared: {
                    indexes: entry.live.indexes.filter((row) => !(entry.retiredIndexes.includes(row.name) || plans.some(({ index }) => [index.name, prior(index.parts)].includes(row.name)))).map((row) => row.name),
                    references: links.filter((row) => !entry.rendered.links.some((link) => link.name === row.name)).map(({ columns, target }) => ({ table: name, columns, target })),
                },
            };
        };
        const scratch = (name: string) => Array.findFirst(live.scratch, (desired) => desired.name === name);
        const tables = resolved.matched.flatMap((entry) => Option.toArray(scratch(entry.declared.name)).map((desired) => analyze(entry, desired)));
        return yield* resolved.plan({
            findings: [],
            replaced: [],
            populated,
            session: [...settings, { name: 'search_path', value: path }],
            drifted: ({ declared, live: table }, { definition, found, recorded }) => Option.exists(Option.all({ desired: scratch(declared.name), now: found }), ({ desired, now }) => drifts(table, desired, now, definition, Option.flatMapNullishOr(recorded, Struct.get('digest')))),
            preamble,
            opening: [
                ...absent.map((extension) => sql`CREATE EXTENSION IF NOT EXISTS ${sql(extension)}`),
                ...configurations.flatMap(({ name, configuration, configured, drift }) => [
                    ...(configured ? [] : [sql`CREATE TEXT SEARCH CONFIGURATION ${sql(name)} (COPY = ${sql.literal(configuration)})`]),
                    ...drift.map(
                        ({ tokens, dictionaries }) =>
                            sql`ALTER TEXT SEARCH CONFIGURATION ${sql(name)} ${Option.match(dictionaries, { onNone: () => sql`DROP MAPPING IF EXISTS FOR ${sql.literal(tokens.join(', '))}`, onSome: (chain) => sql`ALTER MAPPING FOR ${sql.literal(tokens.join(', '))} WITH ${sql.literal(chain.join(', '))}` })}`,
                    ),
                ]),
            ],
            statements: [
                ...tables.flatMap(({ relabels }) => relabels),
                ...resolved.created.flatMap(({ declared, rendered: table, deferred }) => [
                    Reconcile.render(sql, Pg, catalog, declared, { inline: (link) => !deferred.some(({ name }) => name === link.name) }).create,
                    ...table.comments,
                    ...table.indexes.map(({ text }) => text('')),
                    ...grants(table.relation, []),
                    ...Option.toArray(declared.tenant).flatMap(() => [sql`ALTER TABLE ${table.relation} ENABLE ROW LEVEL SECURITY`, sql`ALTER TABLE ${table.relation} FORCE ROW LEVEL SECURITY`]),
                    ...Option.toArray(table.policy).map(({ text }) => text(role)),
                ]),
                ...resolved.created.flatMap(({ rendered: table, deferred }) => deferred.map((link) => sql`ALTER TABLE ${table.relation} ADD ${Pg.constraint(sql, link.name, link.text)}`)),
                ...tables.flatMap((entry) => entry.statements),
            ],
            concurrent: [
                ...tables.flatMap(({ dropped }) => dropped.map((index) => sql`DROP INDEX CONCURRENTLY IF EXISTS ${sql(index)}`)),
                ...tables.flatMap(({ built }) => built),
                ...tables.flatMap(({ reindexed }) => reindexed),
                ...tables.flatMap(({ rendered: table, validated }) => validated.map((constraint) => sql`ALTER TABLE ${table.relation} VALIDATE CONSTRAINT ${sql(constraint)}`)),
            ],
            undeclared: { indexes: tables.flatMap(({ undeclared }) => undeclared.indexes), references: tables.flatMap(({ undeclared }) => undeclared.references) },
        });
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Pg };
