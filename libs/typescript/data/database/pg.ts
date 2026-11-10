import { PgClient, PgTypes } from '@effect/sql-pg';
import { traverse } from '@pgsql/traverse';
import type { Node } from '@pgsql/types';
import { Array, Config, Duration, Effect, Equal, Equivalence, Exit, Filter, Function, flow, HashSet, Layer, Match, Option, Record, Result, Schema, Struct } from 'effect';
import { SqlClient, SqlSchema, Statement } from 'effect/sql';
import { deparseSync, loadModule, parseSync } from 'pgsql-parser';
import type { Catalog } from '#/catalog';
import type { Kind } from '#/column';
import { type Dialect, Engine, Rows } from '#/engine';
import { constant, diff, type Identity, identifier, literal, mergeTreeRefusals, methods, type referentialActions, rename, render, unsupported } from '#/reconcile';
import { Tokenizer } from '#/relation';

// --- [CONSTANTS] -----------------------------------------------------------------------

const trigram = { extension: 'pg_trgm', classes: { gin: 'gin_trgm_ops', gist: 'gist_trgm_ops' } } as const;
const types = { text: 'text', uuid: 'uuid', json: 'jsonb', instant: 'timestamptz(3)', integer: 'bigint', decimal: 'numeric', real: 'double precision', boolean: 'boolean', blob: 'bytea' } as const satisfies { readonly [K in Kind]: string };
const confdeltype = { 'NO ACTION': 'a', RESTRICT: 'r', CASCADE: 'c', 'SET NULL': 'n', 'SET DEFAULT': 'd' } as const satisfies { readonly [A in (typeof referentialActions)[number]]: string };

// --- [MODELS] --------------------------------------------------------------------------

const Names = Schema.Array(Schema.String);
const Role = Schema.Struct({ role: Schema.String });
const SearchPath = Schema.Struct({ path: Schema.String });
const Configurations = Schema.Struct({
    migrator: Schema.String,
    absent: Names,
    configurations: Schema.fromJsonString(Schema.Array(Schema.Struct({ name: Schema.String, configuration: Schema.String, configured: Schema.Boolean, drift: Schema.Array(Schema.Struct({ tokens: Schema.NonEmptyArray(Schema.String), dictionaries: Schema.OptionFromNullOr(Schema.NonEmptyArray(Schema.String)) })) }))),
});
const trigramNamespace = SqlSchema.findOne({
    Request: Schema.Void,
    Result: Schema.Struct({ namespace: Schema.String }),
    execute: () => SqlClient.SqlClient.use((sql) => sql`SELECT quote_ident(n.nspname) AS namespace FROM pg_extension e JOIN pg_namespace n ON n.oid = e.extnamespace WHERE e.extname = ${trigram.extension}`),
});
const PgTable = Schema.Struct({
    name: Schema.String,
    identity: Schema.NullOr(Schema.String),
    rls: Schema.Boolean,
    force: Schema.Boolean,
    grants: Schema.Array(Schema.Tuple([Schema.String, Schema.String])),
    columns: Schema.Array(Schema.Struct({ name: Schema.String, identity: Schema.NullOr(Schema.String), type: Schema.String, expression: Schema.NullOr(Schema.String), generated: Schema.OptionFromNullOr(Schema.Literals(['STORED', 'VIRTUAL'])), required: Schema.Boolean, sources: Names })),
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
class Pg extends Schema.Class<Pg, { readonly brand: unique symbol }>('Pg')({ tables: Schema.Array(PgTable), scratch: Schema.Array(PgTable) }) {
    // --- [ROWS]

    static readonly member: Rows['Service']['member'] = (sql, field) => sql`e.value ->> ${sql.literal(literal(field))}`;
    static readonly element: Rows['Service']['element'] = (sql, { kind }, text) => (kind === 'blob' ? sql`decode(${text}, 'hex')` : sql`(${text})::${sql.literal(types[kind])}`);
    static readonly rows = (sql: SqlClient.SqlClient, body: string) => sql`jsonb_array_elements(${body}) e(value)`;
    static readonly texts: Rows['Service']['texts'] = (sql, body) => sql`jsonb_array_elements_text(${body}) WITH ORDINALITY e(value, position)`;
    static readonly insertion: Rows['Service']['insertion'] = (sql, body, order) => sql`${Pg.rows(sql, body)} ORDER BY ${order}`;
    static readonly read: Rows['Service']['read'] = (sql, { name, textual }, scope) => (textual ? sql`${scope}${sql(name)}::text AS ${sql(name)}` : sql`${scope}${sql(name)}`);
    static readonly upsert: Rows['Service']['upsert'] = ({ sql, owner, unqualified, ignore, keys, reads, inserting, flags }, body, set) => {
        const parts = set.map(({ name }) => sql(name));
        const update = sql.join('', false)(['ON CONFLICT (', keys(unqualified), ') DO UPDATE SET ', sql.csv(parts.map((part) => sql`${part} = excluded.${part}`)), ' WHERE (', sql.csv(parts.map((part) => sql`${owner}.${part}`)), ') IS DISTINCT FROM (', sql.csv(parts.map((part) => sql`excluded.${part}`)), ')']);
        return inserting(body, Array.isReadonlyArrayNonEmpty(parts) ? update : ignore, sql.join('', false)(['WITH (OLD AS ', Statement.fragment([sql(flags.previous)]), ') ', reads(unqualified, sql`${sql(flags.previous)} IS NULL AS ${sql(flags.inserted)}`)]));
    };
    static readonly ensure: Rows['Service']['ensure'] = ({ sql, owner, unqualified, order, ignore, keys, reads, inserting }, body) => {
        const outer = sql.literal('t.');
        return sql`WITH inserted AS (${inserting(body, ignore, sql.literal('*'))}) SELECT ${reads(outer)} FROM (SELECT * FROM inserted UNION ALL SELECT * FROM ${owner} WHERE (${keys(unqualified)}) IN (SELECT ${order} FROM ${Pg.rows(sql, body)})) t ORDER BY ${keys(outer)}`;
    };
    static readonly atomic = Engine.transaction;
    static readonly Configuration = Schema.Struct({ name: Schema.String, configuration: Schema.String, filters: Names });
    static readonly configuration = ({ configuration, filters }: { readonly configuration: string; readonly filters: readonly string[] }) => identifier([configuration, ...filters]);
    static readonly document = (sql: SqlClient.SqlClient, { columns, tokenizer }: Parameters<Engine['Service']['fulltext']>[2]) => {
        const joined = sql.join(` || ' ' || `, false)(columns.map(({ name }) => sql`coalesce(${sql(name)}, '')`));
        return Tokenizer.$match(tokenizer, { Trigram: () => joined, Words: (words) => sql.join('', false)(['to_tsvector(', literal(Pg.configuration(words)), ', ', joined, ')']) });
    };
    static readonly #engine = (names: Dialect['names']) =>
        Layer.merge(
            Layer.succeed(Engine, { ...Pg, names }),
            Layer.effect(
                Rows,
                Effect.map(
                    trigramNamespace(undefined).pipe(
                        Effect.map(Struct.get('namespace')),
                        Effect.cachedWithTTL((exit) => (Exit.isSuccess(exit) ? Duration.infinity : Duration.zero)),
                    ),
                    (extension) => ({ ...Pg, search: Pg.#search(extension) }),
                ),
            ),
        );
    static readonly #search = (extension: Effect.Effect<string, Effect.Error<ReturnType<Rows['Service']['search']>>, SqlClient.SqlClient>): Rows['Service']['search'] =>
        Effect.fnUntraced(function* (...[context, searchable, text, pattern, marks, count]: Parameters<Rows['Service']['search']>) {
            const { sql, owner, prefix, unqualified, keys, reads, flags } = context;
            const [body, rank] = [Pg.document(sql, searchable), sql(flags.rank)];
            const { score, source, filter, mark } = yield* Tokenizer.$match(searchable.tokenizer, {
                Trigram: () =>
                    Effect.map(Effect.provideService(extension, SqlClient.SqlClient, sql), (namespace) => ({
                        score: sql`${sql.literal(namespace)}.word_similarity(${text}, ${body})`,
                        source: owner,
                        filter: sql`(${body}) ILIKE ${pattern}`,
                        mark: (part: string, open: string, close: string) => sql`regexp_replace(${sql(part)}, ${['***=', text].join('')}, ${[open.replaceAll('\\', '\\\\'), '\\&', close.replaceAll('\\', '\\\\')].join('')}, 'gi')`,
                    })),
                Words: (words) => {
                    const dictionary = sql.literal(literal(Pg.configuration(words)));
                    return Effect.succeed({
                        score: sql`ts_rank(${body}, q)`,
                        source: sql`${owner} CROSS JOIN plainto_tsquery(${dictionary}, ${text}) q`,
                        filter: sql`${body} @@ q`,
                        mark: (part: string, open: string, close: string) => sql`ts_headline(${dictionary}, ${sql(part)}, q, ${['StartSel="', open.replaceAll('"', '""'), '", StopSel="', close.replaceAll('"', '""'), '", HighlightAll=true'].join('')})`,
                    });
                },
            });
            const marked = Option.match(marks, { onNone: () => sql.literal('NULL'), onSome: ({ open, close }) => sql`jsonb_build_object(${sql.csv(searchable.columns.flatMap((entry) => [literal(entry.name), mark(entry.name, open, close)]))})` });
            return yield* sql`SELECT ${reads(unqualified)}, ${score} AS ${rank}, ${marked} AS ${sql(flags.highlight)} FROM ${source} WHERE ${filter} ORDER BY ${rank} DESC, ${keys(prefix)} LIMIT ${count}`;
        });

    // --- [DDL]

    static readonly type: Engine['Service']['type'] = ({ kind }) => types[kind];
    static readonly constant: Engine['Service']['constant'] = (entry, value) => (entry.kind === 'boolean' ? `${value === 1}` : constant(entry, value));
    static readonly codePoints: Engine['Service']['codePoints'] = (sql, subject) => sql`char_length(${subject})`;
    static readonly text: Engine['Service']['text'] = (sql, subject, kind) => (kind === 'uuid' ? sql`${subject}::text` : subject);
    static readonly domain: Engine['Service']['domain'] = (sql, subject, kind) =>
        Match.value(kind).pipe(
            Match.whenOr('real', 'decimal', () => [sql`${subject} NOT IN ('NaN', 'Infinity', '-Infinity')`]),
            Match.when('instant', () => [sql`isfinite(${subject})`]),
            Match.orElse(() => []),
        );
    static readonly projection: Engine['Service']['projection'] = (sql, { kind }, { from, path }) => sql.join('', false)([sql`(${sql(from)} ${sql.literal(kind === 'json' ? '#>' : '#>>')} ARRAY[${sql.literal(path.map(literal).join(', '))}])`, ...(['json', 'text'].includes(kind) ? [] : ['::', types[kind]])]);
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
    static readonly methods: Engine['Service']['methods'] = methods;
    static readonly covering: Engine['Service']['covering'] = (sql, _, include) => ({ keys: [], clause: Array.isReadonlyArrayNonEmpty(include) ? [sql.join('', false)(['INCLUDE (', sql.csv(include.map((name) => Statement.fragment([sql(name)]))), ')'])] : [] });
    static readonly suffix: Engine['Service']['suffix'] = (sql) => sql.literal('');
    static readonly rename = rename;
    static readonly fulltext: Engine['Service']['fulltext'] = (sql, owner, search) => {
        const [name, body] = [identifier([owner, 'search']), Pg.document(sql, search)];
        const words = Option.map(Option.liftPredicate(search.tokenizer, Tokenizer.$is('Words')), ({ configuration, filters }) => ({ name: Pg.configuration({ configuration, filters }), configuration, filters }));
        return {
            indexes: [
                {
                    name,
                    parts: ['search'],
                    extensions: Option.match(words, { onNone: () => [trigram.extension], onSome: () => [] }),
                    configuration: Option.filter(words, ({ filters }) => Array.isReadonlyArrayNonEmpty(filters)),
                    text: (modifiers, on) => sql`CREATE INDEX ${sql.join(' ', false)([...modifiers, Statement.fragment([sql(name)])])} ON ${on} USING gin (${Option.match(words, { onNone: () => sql.join('', false)(['(', body, ') ', trigram.classes.gin]), onSome: () => body })})`,
                },
            ],
            virtual: Option.none(),
        };
    };
    static readonly policy: Engine['Service']['policy'] = (sql, owner, name, type, setting) => {
        const [policy, rule] = [identifier([owner, 'tenant']), sql`${sql(name)} = nullif(current_setting(${sql.literal(literal(setting))}, true), '')::${sql.literal(type)}`];
        return Option.some({ name: policy, rule, text: (on, role) => sql`CREATE POLICY ${sql(policy)} ON ${on} TO ${role} USING (${rule}) WITH CHECK (${rule})` });
    };
    static readonly commentTarget = (sql: SqlClient.SqlClient, relation: Statement.Fragment, column: Option.Option<string>) => Option.match(column, { onNone: () => sql`TABLE ${relation}`, onSome: (name) => sql`COLUMN ${relation}.${sql(name)}` });
    static readonly comment: Engine['Service']['comment'] = (sql, name, identity) => ({
        inline: sql.literal(''),
        statements: (relation, column) => (identity.origin === name && identity.generation === 0 && identity.digest === undefined ? [] : [sql`COMMENT ON ${Pg.commentTarget(sql, relation, column)} IS ${sql.literal(literal(JSON.stringify(identity)))}`]),
    });

    // --- [RECONCILIATION]

    static readonly refusals: Engine['Service']['refusals'] = (relation) => {
        const { name: owner, columns, indexes } = relation;
        const named = new Map(Object.values(columns).map((column) => [column.name, column]));
        const refused = indexes.flatMap(({ keys, method }) =>
            Array.filterMap(keys, ({ column, casefold }) => {
                const entry = named.get(column);
                return entry !== undefined && Option.isNone(Pg.opclass(method, casefold ? 'text' : entry.kind)) ? Result.succeed(entry) : Result.failVoid;
            }),
        );
        return [...unsupported(owner, 'opclass', Array.dedupe(refused), Function.constTrue), ...mergeTreeRefusals(relation)];
    };
    static readonly role: Engine['Service']['role'] = (sql) => Effect.map(SqlSchema.findOne({ Request: Schema.Void, Result: Role, execute: () => sql`SELECT current_user AS role` })(undefined), Struct.get('role'));
    static readonly open: Engine['Service']['open'] = () => Effect.void;
    static readonly execute = Engine.transaction;
    static readonly transactional = true;
    static readonly #indexText = flow(Struct.get<ReturnType<typeof render>['indexes'][number], 'text'>('text'), Function.apply([]));
    static readonly #reference: Node = { ColumnRef: { fields: [{ String: { sval: '$column' } }] } };
    static readonly #normalized = (column: string, definition: string) => {
        const parsed = parseSync(`ALTER TABLE _ ADD ${definition}`);
        traverse(parsed, { ColumnRef: (ref) => (Option.contains(Array.last<Node>(ref.node.fields), { String: { sval: column } }) ? ref.replaceWith(Pg.#reference) : undefined) });
        return deparseSync(parsed);
    };
    static readonly #block = (sql: SqlClient.SqlClient, statements: readonly Statement.Statement<object>[]) => sql`DO ${sql.literal(literal(['BEGIN', ...statements.map((statement) => [statement.compile(true)[0], ';'].join('')), 'END'].join(' ')))}`;
    static readonly plan: Engine['Service']['plan'] = Effect.fnUntraced(function* (sql: SqlClient.SqlClient, catalog: Catalog, runtime: Option.Option<string>) {
        const rendered = catalog.tables.map((declared) => ({ declared, rendered: render(sql, Pg, catalog, declared) }));
        const indexes = rendered.flatMap(({ rendered: table }) => table.indexes);
        const required = Array.dedupe(indexes.flatMap(Struct.get('extensions')));
        const owned = Array.dedupeWith(Array.getSomes(indexes.map(({ configuration }) => configuration)), Equivalence.mapInput(Equivalence.String, Struct.get('name')));
        const settings = Option.match(catalog.timeouts, { onNone: () => [], onSome: ({ lock, transaction }) => Record.collect({ lock_timeout: lock, transaction_timeout: transaction }, (name, limit) => ({ name, value: `${Duration.toMillis(limit)}ms` })) });
        const preamble = settings.map(({ name, value }) => sql`SET LOCAL ${sql.literal(name)} = ${sql.literal(literal(value))}`);
        const prelude = { required, filters: Array.dedupe(owned.flatMap(Struct.get('filters'))), declared: owned };
        yield* Effect.all(preamble, { discard: true });
        const { path } = yield* SqlSchema.findOne({
            Request: Prelude,
            Result: SearchPath,
            execute: (names) =>
                sql`SELECT pg_advisory_xact_lock(hashtextextended(current_schema(), 0))::text AS lock, set_config('search_path', concat_ws(', ', quote_ident(current_schema()),
                            (SELECT string_agg(quote_ident(n.nspname), ', ' ORDER BY n.nspname) FROM pg_namespace n WHERE n.oid NOT IN (current_schema()::regnamespace, 'pg_catalog'::regnamespace)
                                AND (EXISTS (SELECT 1 FROM pg_extension e WHERE e.extnamespace = n.oid AND e.extname = ANY(${names.required}::text[])) OR EXISTS (SELECT 1 FROM pg_ts_dict d WHERE d.dictnamespace = n.oid AND d.dictname = ANY(${names.filters}::text[])))), 'pg_temp'), true) AS path`,
        })(prelude);
        const environment = yield* SqlSchema.findOne({
            Request: Prelude,
            Result: Configurations,
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
        })(prelude);
        const { absent, configurations } = environment;
        const existing = configurations.filter(Struct.get('configured')).map(Struct.get('name'));
        const ready = ({ extensions: needed, configuration }: (typeof indexes)[number]) => !needed.some((name) => absent.includes(name)) && Option.toArray(configuration).every(({ name }) => existing.includes(name));
        const role = Option.match(runtime, { onNone: () => sql.literal('PUBLIC'), onSome: (name) => sql`${sql(name)}` });
        const policyText = flow(Struct.get<Option.Option.Value<ReturnType<typeof render>['policy']>, 'text'>('text'), Function.apply(role));
        const scratched = rendered.map(({ declared }) => render(sql, Pg, catalog, declared, { target: (name) => sql`pg_temp.${sql(name)}`, inline: Function.constFalse }));
        yield* Pg.#block(
            sql,
            scratched.flatMap((table) => [sql`${table.create} ON COMMIT DROP`, ...table.indexes.filter(ready).map(Pg.#indexText), ...Option.toArray(table.policy).map(policyText)]),
        );
        const { observed: live } = yield* SqlSchema.findOne({
            Request: Schema.Void,
            Result: Observed,
            execute: () =>
                sql`WITH relations AS (SELECT c.oid, c.relname, c.relowner, c.relacl, c.relrowsecurity, c.relforcerowsecurity, c.relnamespace = pg_my_temp_schema() AS scratch FROM pg_class c WHERE c.relnamespace IN (current_schema()::regnamespace, pg_my_temp_schema()) AND c.relkind = 'r'),
                    constraints AS (SELECT n.oid, n.conrelid, n.conname, n.contype, n.convalidated, n.confrelid, n.confdeltype,
                        array(SELECT a.attname::text FROM unnest(n.conkey) WITH ORDINALITY k(number, position) JOIN pg_attribute a ON (a.attrelid, a.attnum) = (n.conrelid, k.number) ORDER BY k.position) AS columns
                        FROM pg_constraint n JOIN relations r ON r.oid = n.conrelid),
                    described AS (SELECT r.scratch, json_build_object('name', r.relname, 'identity', obj_description(r.oid, 'pg_class'), 'rls', r.relrowsecurity, 'force', r.relforcerowsecurity,
                        'grants', array(SELECT json_build_array(g.rolname, a.privilege_type) FROM aclexplode(r.relacl) a JOIN pg_roles g ON g.oid = a.grantee WHERE a.grantee <> r.relowner),
                        'columns', array(SELECT json_build_object('name', a.attname, 'identity', col_description(a.attrelid, a.attnum), 'type', format_type(a.atttypid, a.atttypmod), 'expression', pg_get_expr(d.adbin, d.adrelid), 'generated', CASE a.attgenerated WHEN 's' THEN 'STORED' WHEN 'v' THEN 'VIRTUAL' END,
                            'required', EXISTS (SELECT 1 FROM pg_constraint n WHERE n.conrelid = a.attrelid AND n.contype = 'n' AND n.conkey = ARRAY[a.attnum] AND n.convalidated),
                            'sources', array(SELECT s.attname::text FROM pg_depend p JOIN pg_attribute s ON (s.attrelid, s.attnum) = (p.refobjid, p.refobjsubid) WHERE p.classid = 'pg_attrdef'::regclass AND p.objid = d.oid AND p.refobjsubid > 0 AND s.attnum <> a.attnum))
                            FROM pg_attribute a LEFT JOIN pg_attrdef d ON (d.adrelid, d.adnum) = (a.attrelid, a.attnum) WHERE a.attrelid = r.oid AND a.attnum > 0 AND NOT a.attisdropped ORDER BY a.attnum),
                        'checks', array(SELECT json_build_object('name', n.conname, 'definition', pg_get_constraintdef(n.oid, true), 'columns', n.columns) FROM constraints n WHERE n.conrelid = r.oid AND n.contype = 'c'),
                        'references', array(SELECT json_build_object('name', n.conname, 'columns', n.columns, 'target', t.relname, 'onDelete', n.confdeltype::text) FROM constraints n JOIN pg_class t ON t.oid = n.confrelid WHERE n.conrelid = r.oid AND n.contype = 'f'),
                        'invalid', array(SELECT n.conname::text FROM constraints n WHERE n.conrelid = r.oid AND NOT n.convalidated),
                        'indexes', array(SELECT json_build_object('name', i.relname, 'valid', x.indisvalid, 'unique', x.indisunique, 'method', am.amname, 'width', x.indnkeyatts,
                            'keys', array(SELECT json_build_object('column', x.indkey[k - 1] <> 0, 'text', coalesce(a.attname::text, pg_get_indexdef(x.indexrelid, k, true))) FROM generate_series(1, x.indnatts) k LEFT JOIN pg_attribute a ON a.attrelid = x.indrelid AND a.attnum = x.indkey[k - 1] ORDER BY k),
                            'classes', array(SELECT o.opcname::text FROM unnest(x.indclass) WITH ORDINALITY u(oid, position) JOIN pg_opclass o ON o.oid = u.oid ORDER BY u.position),
                            'predicate', pg_get_expr(x.indpred, x.indrelid, true), 'definition', pg_get_indexdef(x.indexrelid),
                            'columns', array(SELECT a.attname::text FROM pg_depend p JOIN pg_attribute a ON (a.attrelid, a.attnum) = (p.refobjid, p.refobjsubid) WHERE p.objid = x.indexrelid AND p.classid = 'pg_class'::regclass AND p.refobjsubid > 0))
                            FROM pg_index x JOIN pg_class i ON i.oid = x.indexrelid JOIN pg_am am ON am.oid = i.relam WHERE x.indrelid = r.oid AND NOT EXISTS (SELECT 1 FROM pg_constraint k WHERE k.conindid = x.indexrelid AND k.conrelid = x.indrelid)),
                        'policies', array(SELECT json_build_object('name', p.polname, 'using', pg_get_expr(p.polqual, p.polrelid, true), 'check', pg_get_expr(p.polwithcheck, p.polrelid, true), 'roles', p.polroles::regrole[]::text[]) FROM pg_policy p WHERE p.polrelid = r.oid)) AS relation
                        FROM relations r)
                    SELECT json_build_object('tables', array(SELECT d.relation FROM described d WHERE NOT d.scratch), 'scratch', array(SELECT d.relation FROM described d WHERE d.scratch))::text AS observed`,
        })();
        const resolved = diff(sql, Pg, catalog, rendered, live.tables);
        const populated = yield* resolved.populated;
        yield* Effect.promise(loadModule);
        return yield* live.#reconcile(sql, catalog, runtime, { resolved, populated, environment, path, settings, preamble, role, policyText });
    });
    #reconcile(
        sql: SqlClient.SqlClient,
        catalog: Catalog,
        runtime: Option.Option<string>,
        {
            resolved,
            populated,
            environment,
            path,
            settings,
            preamble,
            role,
            policyText,
        }: {
            readonly resolved: ReturnType<typeof diff<typeof PgTable.Type>>;
            readonly populated: readonly string[];
            readonly environment: typeof Configurations.Type;
            readonly path: string;
            readonly settings: readonly { readonly name: string; readonly value: string }[];
            readonly preamble: readonly Statement.Statement<object>[];
            readonly role: Statement.Fragment;
            readonly policyText: (policy: Option.Option.Value<ReturnType<typeof render>['policy']>) => Statement.Statement<object>;
        },
    ) {
        const { migrator, absent, configurations } = environment;
        const drifted = configurations.filter(({ drift }) => Array.isReadonlyArrayNonEmpty(drift)).map(Struct.get('name'));
        const scratch = Record.map(Record.fromIterableBy(this.scratch, Struct.get('name')), (table) => ({ ...table, columnsByName: Record.fromIterableBy(table.columns, Struct.get('name')) }));
        const privileges = [...Option.toArray(runtime).map((grantee) => ({ grantee, all: ['SELECT', 'INSERT', 'UPDATE', 'DELETE'] })), ...Option.toArray(catalog.reader).map((grantee) => ({ grantee, all: ['SELECT'] }))].filter(({ grantee }) => grantee !== migrator);
        const grants = (on: Statement.Fragment, held: readonly (readonly [string, string])[]) => {
            const byRole = Map.groupBy(held, Array.headNonEmpty);
            return Array.filterMap(privileges, ({ grantee, all }) => {
                const missing = Array.difference(all, Array.map(byRole.get(grantee) ?? [], Array.lastNonEmpty));
                return Array.isReadonlyArrayNonEmpty(missing) ? Result.succeed(sql`GRANT ${sql.literal(missing.join(', '))} ON ${on} TO ${sql(grantee)}`) : Result.failVoid;
            });
        };
        const stamped = (relation: Statement.Fragment, column: Option.Option<string>, name: string, recorded: Option.Option<typeof Identity.Type>, identity: typeof Identity.Type) =>
            Match.value({ writes: Pg.comment(sql, name, identity).statements(relation, column), recorded }).pipe(
                Match.when({ writes: Array.isReadonlyArrayEmpty, recorded: Option.isSome }, () => [sql`COMMENT ON ${Pg.commentTarget(sql, relation, column)} IS NULL`]),
                Match.when(
                    ({ recorded: prior }) => Option.contains(prior, identity),
                    () => [],
                ),
                Match.orElse(({ writes }) => writes),
            );
        const shape = (row: (typeof PgTable.Type)['indexes'][number], rename: (column: string) => string) => ({ unique: row.unique, method: row.method, width: row.width, classes: row.classes, predicate: row.predicate, keys: row.keys.map(({ column, text }) => (column ? rename(text) : text)) });
        const checks = ({ checks: rows }: typeof PgTable.Type, column: string) => HashSet.fromIterable(rows.filter(({ columns }) => Equal.equals(columns, [column])).map(({ definition }) => Pg.#normalized(column, definition)));
        const analyze = ({ entry, desired }: { readonly entry: (typeof resolved.matched)[number]; readonly desired: (typeof scratch)[string] }) => {
            const { name } = entry.declared;
            const owner = entry.rendered.relation;
            const prior = (parts: readonly string[]) => identifier([entry.live.name, ...parts.map(entry.original)]);
            const named = new Set([...entry.live.checks, ...entry.live.references].map(({ name: constraint }) => constraint));
            const relabels = [...Array.getSomes(entry.rendered.definitions.map(({ check }) => check)), ...entry.rendered.links].map(({ name: to, parts }) => ({ from: prior(parts), to })).filter(({ from, to }) => named.has(from) && from !== to && !named.has(to));
            const renamed = new Map(relabels.map(({ from, to }) => [from, to]));
            const relabeled = (constraint: string) => renamed.get(constraint) ?? constraint;
            const links = entry.live.references.map((row) => ({ ...row, name: relabeled(row.name), columns: Array.map(row.columns, entry.renamed), target: resolved.referenced(row.target).name }));
            const linked = HashSet.fromIterable(links);
            const linkNames = new Set(links.map(Struct.get('name')));
            const declaredLinks = new Set(entry.rendered.links.map(Struct.get('name')));
            const stale = entry.rendered.links.filter((link) => !HashSet.has(linked, Struct.pick(link, Struct.keys(PgTable.fields.references.value.fields))));
            const indexesByName = Record.fromIterableBy(entry.live.indexes, Struct.get('name'));
            const wantedIndexes = Record.map(Record.fromIterableBy(desired.indexes, Struct.get('name')), (row) => shape(row, Function.identity));
            const policiesByName = Record.fromIterableBy(entry.live.policies, Struct.get('name'));
            const wantedPolicies = Record.fromIterableBy(desired.policies, Struct.get('name'));
            const planned = (index: (typeof entry.rendered.indexes)[number]) => {
                const target = Record.get(wantedIndexes, index.name);
                const fits = (row: (typeof entry.live.indexes)[number]) => row.valid && Option.contains(target, shape(row, entry.renamed));
                const present = Record.get(indexesByName, index.name);
                const before = Option.filter(Record.get(indexesByName, prior(index.parts)), (row) => row.name !== index.name);
                const [keep, rename] = [Option.exists(present, fits), Option.isNone(present) && Option.exists(before, fits)];
                return { index, keep, rename, dropped: [...Option.toArray(present).filter(() => !keep), ...Option.toArray(before).filter(() => !rename)].map((row) => row.name) };
            };
            const plans = entry.rendered.indexes.map(planned);
            const plannedNames = new Set(plans.flatMap(({ index }) => [index.name, prior(index.parts)]));
            const policy = (rule: Option.Option.Value<typeof entry.rendered.policy>) => {
                const current = Record.get(policiesByName, rule.name);
                const before = Option.filter(Record.get(policiesByName, prior(['tenant'])), (row) => row.name !== rule.name && Option.isNone(current));
                const [present, wanted] = [Option.orElse(current, () => before), Record.get(wantedPolicies, rule.name)];
                return [
                    ...Option.toArray(before).map((row) => sql`ALTER POLICY ${sql(row.name)} ON ${owner} RENAME TO ${sql(rule.name)}`),
                    ...Match.value(present).pipe(
                        Match.when({ _tag: 'None' }, () => [rule.text(role)]),
                        Match.when({ value: (row) => Option.contains(wanted, { ...row, name: rule.name }) }, () => []),
                        Match.orElse(() => [sql`ALTER POLICY ${sql(rule.name)} ON ${owner} TO ${role} USING (${rule.rule}) WITH CHECK (${rule.rule})`]),
                    ),
                ];
            };
            const pairs = Array.filterMap(
                entry.owned ? entry.fields.filter(({ definition }) => definition.retype) : [],
                Filter.fromPredicateOption(({ definition: column, found }) => Option.all({ wants: Record.get(desired.columnsByName, column.name), now: found }).pipe(Option.map(Struct.assign({ column })))),
            );
            const moved = pairs.filter(({ wants, now }) => now.type !== wants.type);
            const constrainedColumns = HashSet.fromIterable(pairs.map(({ column }) => [column.name]));
            const constrained = (rows: readonly (typeof PgTable.Type)['checks'][number][]) => rows.filter((row) => HashSet.has(constrainedColumns, row.columns));
            const [had, needs] = [constrained(entry.live.checks.map(Struct.evolve({ name: relabeled, columns: Array.map(entry.renamed) }))), constrained(desired.checks)];
            const unmatched = Array.differenceWith(Equivalence.mapInput(Equivalence.String, Struct.get<(typeof had)[number], 'definition'>('definition')));
            const [obsolete, missing] = [unmatched(had, needs), unmatched(needs, had)];
            const movedColumns = new Set(moved.map(({ now }) => now.name));
            const dependents = Array.filterMap(
                entry.live.columns,
                Filter.fromPredicateOption((row) =>
                    Option.map(
                        Option.filter(Option.all({ keyword: row.generated, expression: Option.fromNullOr(row.expression) }), () => !new Set(row.sources).isDisjointFrom(movedColumns)),
                        (generated) => ({ ...row, ...generated }),
                    ),
                ),
            );
            const dependentColumns = new Set(dependents.map(Struct.get('name')));
            const tightened = pairs.filter(({ wants, now }) => wants.required && !now.required).map(({ column }) => ({ column: column.name, constraint: identifier([name, column.name, 'not', 'null']) }));
            const changes = pairs.flatMap(({ column, wants, now }) => [
                ...(now.type === wants.type ? [] : [sql`ALTER COLUMN ${sql(column.name)} TYPE ${sql.literal(wants.type)} USING ${sql(column.name)}::${sql.literal(wants.type)}`]),
                ...(Option.isSome(wants.generated) || now.expression === wants.expression
                    ? []
                    : [Option.match(Option.fromNullOr(wants.expression), { onNone: () => sql`ALTER COLUMN ${sql(column.name)} DROP DEFAULT`, onSome: (expression) => sql`ALTER COLUMN ${sql(column.name)} SET DEFAULT ${sql.literal(expression)}` })]),
                ...(now.required && !wants.required ? [sql`ALTER COLUMN ${sql(column.name)} DROP NOT NULL`] : []),
            ]);
            const retyping = [
                ...obsolete.map((row) => sql`ALTER TABLE ${owner} DROP CONSTRAINT ${sql(row.name)}`),
                ...dependents.map((row) => sql`ALTER TABLE ${owner} DROP COLUMN ${sql(row.name)}`),
                ...moved
                    .filter(({ now }) => now.type !== 'text')
                    .map(({ column, wants, now }) =>
                        Pg.#block(sql, [
                            sql`IF EXISTS (SELECT 1 FROM ${owner} WHERE (${sql(column.name)}::${sql.literal(wants.type)})::${sql.literal(now.type)} IS DISTINCT FROM ${sql(column.name)}) THEN RAISE EXCEPTION 'ALTER COLUMN %.% TYPE % changes stored values', ${sql.literal(literal(name))}, ${sql.literal(literal(column.name))}, ${sql.literal(literal(wants.type))} USING ERRCODE = 'check_violation'; END IF`,
                        ]),
                    ),
                ...Array.match(changes, { onEmpty: () => [], onNonEmpty: (all) => [sql`ALTER TABLE ${owner} ${sql.csv(all)}`] }),
                ...tightened.map(({ column, constraint }) => sql`ALTER TABLE ${owner} ADD CONSTRAINT ${sql(constraint)} NOT NULL ${sql(column)} NOT VALID`),
                ...missing.map((row) => sql`ALTER TABLE ${owner} ADD CONSTRAINT ${sql(row.name)} ${sql.literal(row.definition)} NOT VALID`),
                ...dependents.flatMap((row) => [
                    sql`ALTER TABLE ${owner} ADD COLUMN ${sql(row.name)} ${sql.literal(row.type)} GENERATED ALWAYS AS (${sql.literal(row.expression)}) ${sql.literal(row.keyword)}`,
                    ...Array.fromNullishOr(row.identity).map((comment) => sql`COMMENT ON COLUMN ${owner}.${sql(row.name)} IS ${sql.literal(literal(comment))}`),
                ]),
                ...entry.live.indexes.filter((index) => !new Set(index.columns).isDisjointFrom(dependentColumns)).map(({ definition }) => sql`${sql.literal(definition)}`),
            ];
            const added = Array.filterMap(
                entry.added,
                Filter.fromPredicateOption((definition) => Record.get(desired.columnsByName, definition.name).pipe(Option.bindTo('row'), Option.map(Struct.assign({ definition })))),
            );
            const adding = new Set(added.map(({ definition }) => definition.name));
            const additions = desired.checks.filter((row) => new Set(row.columns).isSubsetOf(adding));
            const tenant = Option.isSome(entry.declared.tenant);
            return {
                ...entry,
                relabels: [...relabels.map(({ from, to }) => sql`ALTER TABLE ${owner} RENAME CONSTRAINT ${sql(from)} TO ${sql(to)}`), ...plans.filter(({ rename }) => rename).map(({ index }) => sql`ALTER INDEX ${sql(prior(index.parts))} RENAME TO ${sql(index.name)}`)],
                statements: [
                    ...entry.dropping.map((row) => sql`ALTER TABLE ${owner} DROP COLUMN ${sql(row.name)}`),
                    ...added.map(({ definition, row }) => {
                        const expression = Option.fromNullOr(row.expression);
                        return sql`ALTER TABLE ${owner} ADD COLUMN ${sql.join(
                            ' ',
                            false,
                        )([
                            Statement.fragment([sql(definition.name)]),
                            row.type,
                            ...Option.toArray(Option.all([row.generated, expression])).map(([keyword, value]) => ['GENERATED ALWAYS AS (', value, ') ', keyword].join('')),
                            ...(definition.nullable ? [] : ['NOT NULL']),
                            ...Option.toArray(Option.filter(expression, () => Option.isNone(row.generated))).map((value) => ['DEFAULT', value].join(' ')),
                        ])}`;
                    }),
                    ...additions.map((row) => sql`ALTER TABLE ${owner} ADD CONSTRAINT ${sql(row.name)} ${sql.literal(row.definition)} NOT VALID`),
                    ...retyping,
                    ...stale.flatMap((link) => [...(linkNames.has(link.name) ? [sql`ALTER TABLE ${owner} DROP CONSTRAINT ${sql(link.name)}`] : []), sql`ALTER TABLE ${owner} ADD CONSTRAINT ${sql(link.name)} ${link.text} NOT VALID`]),
                    ...grants(owner, entry.live.grants),
                    ...(tenant && !entry.live.rls ? [sql`ALTER TABLE ${owner} ENABLE ROW LEVEL SECURITY`] : []),
                    ...(tenant && !entry.live.force ? [sql`ALTER TABLE ${owner} FORCE ROW LEVEL SECURITY`] : []),
                    ...Option.match(entry.rendered.policy, { onNone: () => [], onSome: policy }),
                    ...stamped(owner, Option.none(), name, entry.recorded, entry.identity),
                    ...entry.fields.filter(({ found }) => Option.isSome(found)).flatMap(({ definition, recorded, identity }) => stamped(owner, Option.some(definition.name), definition.name, recorded, { ...identity, digest: definition.digest })),
                    ...added.flatMap(({ definition }) => definition.comment.statements(owner, Option.some(definition.name))),
                ],
                dropped: [...plans.flatMap(({ dropped }) => dropped), ...entry.live.indexes.filter((row) => entry.retiredIndexes.includes(row.name)).map((row) => row.name)],
                built: plans.filter(({ keep, rename }) => !(keep || rename)).map(({ index }) => index.text(['CONCURRENTLY', 'IF NOT EXISTS'])),
                reindexed: plans.filter(({ index, keep, rename }) => (keep || rename) && Option.exists(index.configuration, (configuration) => drifted.includes(configuration.name))).map(({ index }) => sql`REINDEX INDEX CONCURRENTLY ${sql(index.name)}`),
                validated: [...entry.live.invalid.map(relabeled), ...additions.map((row) => row.name), ...tightened.map(({ constraint }) => constraint), ...missing.map((row) => row.name), ...stale.map((link) => link.name)],
                undeclared: {
                    indexes: entry.live.indexes.filter((row) => !(entry.retiredIndexes.includes(row.name) || plannedNames.has(row.name))).map(Struct.get('name')),
                    references: links.filter((row) => !declaredLinks.has(row.name)).map(({ columns, target }) => ({ table: name, columns, target })),
                },
            };
        };
        const tables = Array.filterMap(
            resolved.matched,
            Filter.fromPredicateOption((entry) => Record.get(scratch, entry.declared.name).pipe(Option.bindTo('desired'), Option.map(Struct.assign({ entry })), Option.map(analyze))),
        );
        return resolved.plan({
            findings: [],
            replaced: [],
            populated,
            session: [...settings, { name: 'search_path', value: path }],
            drifted: ({ declared, live: table }, { definition, found, recorded }) =>
                Option.exists(
                    Option.all({
                        expected: Record.get(scratch, declared.name).pipe(
                            Option.bindTo('desired'),
                            Option.bind('wants', ({ desired }) => Record.get(desired.columnsByName, definition.name)),
                        ),
                        now: found,
                    }),
                    ({ expected: { desired, wants }, now }) =>
                        now.type !== wants.type ||
                        now.required !== wants.required ||
                        !Equal.equals(now.generated, wants.generated) ||
                        (Option.isNone(wants.generated) && now.expression !== wants.expression) ||
                        Option.match(Option.flatMapNullishOr(recorded, Struct.get('digest')), {
                            onNone: () => !Equal.equals(checks(table, now.name), checks(desired, definition.name)),
                            onSome: (digest) => Option.isNone(definition.projection) && digest !== definition.digest,
                        }),
                ),
            preamble,
            opening: [
                ...absent.map((extension) => sql`CREATE EXTENSION IF NOT EXISTS ${sql(extension)}`),
                ...configurations.flatMap(({ name, configuration, configured, drift }) => [
                    ...(configured ? [] : [sql`CREATE TEXT SEARCH CONFIGURATION ${sql(name)} (COPY = ${sql.literal(configuration)})`]),
                    ...drift.map(
                        ({ tokens, dictionaries }) =>
                            sql`ALTER TEXT SEARCH CONFIGURATION ${sql(name)} ${Option.match(dictionaries, { onNone: () => sql.join(' ', false)(['DROP MAPPING IF EXISTS FOR', tokens.join(', ')]), onSome: (chain) => sql.join(' ', false)(['ALTER MAPPING FOR', tokens.join(', '), 'WITH', chain.join(', ')]) })}`,
                    ),
                ]),
            ],
            statements: [
                ...tables.flatMap(({ relabels }) => relabels),
                ...resolved.created.flatMap(({ declared, rendered: table, deferred }) => [
                    render(sql, Pg, catalog, declared, { inline: (link) => !deferred.some(({ name }) => name === link.name) }).create,
                    ...table.comments,
                    ...table.indexes.map(Pg.#indexText),
                    ...grants(table.relation, []),
                    ...Option.toArray(declared.tenant).flatMap(() => [sql`ALTER TABLE ${table.relation} ENABLE ROW LEVEL SECURITY`, sql`ALTER TABLE ${table.relation} FORCE ROW LEVEL SECURITY`]),
                    ...Option.toArray(table.policy).map(policyText),
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
    }

    // --- [LAYERS]

    static readonly #client = (names: Dialect['names']) => {
        const { OID, decode, encode } = PgTypes;
        const registry = PgTypes.makeRegistry();
        const register = (oid: number, read: PgTypes.Codec<unknown>['decode']) => registry.register(oid, { decode: read, encode: (value) => encode(value, oid) });
        register(OID.int8, (bytes) => Result.map(decode(bytes, OID.int8, 1), (value) => (Number.isSafeInteger(Number(value)) ? Number(value) : value)));
        register(OID.timestamptz, (bytes) => Result.flatMap(decode(bytes, OID.timestamptz, 1), (value) => Result.mapError(Schema.encodeUnknownResult(Schema.DateFromString)(value), (error) => new PgTypes.CodecError({ message: error.message }))));
        register(OID.jsonb, (bytes) => decode(bytes.subarray(1), OID.text, 1));
        register(OID.bool, (bytes) => Result.map(decode(bytes, OID.bool, 1), Number));
        return { ...names, transformJson: false, types: registry };
    };
    static readonly layer = (config: Omit<PgClient.PgPoolConfig, keyof ReturnType<typeof Pg.#client>>, names: Dialect['names'] = Engine.names.snake) => Pg.#engine(names).pipe(Layer.provideMerge(PgClient.layer({ ...config, ...Pg.#client(names) })));
    static readonly layerConfig = (config: Config.Wrap<Omit<PgClient.PgPoolConfig, keyof ReturnType<typeof Pg.#client>>>, names: Dialect['names'] = Engine.names.snake) =>
        Pg.#engine(names).pipe(
            Layer.provideMerge(
                Config.unwrap(config).pipe(
                    Config.map((settings) => ({ ...settings, ...Pg.#client(names) })),
                    PgClient.layerConfig,
                ),
            ),
        );
}
const Prelude = Schema.Struct({ required: Names, filters: Names, declared: Schema.fromJsonString(Schema.Array(Pg.Configuration)) });
const Observed = Schema.Struct({ observed: Schema.fromJsonString(Pg) });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Pg };
