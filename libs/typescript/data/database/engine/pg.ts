import { Array, Duration, Effect, Equal, Function, Match, Option, Order, Record, Schema, Struct, UndefinedOr } from 'effect';
import type { SqlClient, Statement } from 'effect/sql';
import type { Catalog } from '#/catalog';
import type { Engine } from '#/engine';
import { constant, diff, type Identity, identifier, literal, referentialActions, render } from '#/reconcile';

// --- [CONSTANTS] -----------------------------------------------------------------------

const trigram = { extension: 'pg_trgm', classes: { gin: 'gin_trgm_ops', gist: 'gist_trgm_ops' } } as const;

// --- [MODELS] --------------------------------------------------------------------------

const PgRelation = Schema.Struct({
    name: Schema.String,
    identity: Schema.NullOr(Schema.String),
    rls: Schema.Boolean,
    force: Schema.Boolean,
    grants: Schema.Array(Schema.Tuple([Schema.String, Schema.String])),
    columns: Schema.Array(Schema.Struct({ name: Schema.String, identity: Schema.NullOr(Schema.String), type: Schema.String, expression: Schema.NullOr(Schema.String), generated: Schema.Literals(['', 's', 'v']), required: Schema.Boolean, sources: Schema.Array(Schema.String) })),
    checks: Schema.Array(Schema.Struct({ name: Schema.String, definition: Schema.String, columns: Schema.Array(Schema.String) })),
    references: Schema.Array(Schema.Struct({ name: Schema.String, columns: Schema.NonEmptyArray(Schema.String), target: Schema.String, onDelete: Schema.Literals(Object.values(referentialActions)).transform(Record.keys(referentialActions)) })),
    invalid: Schema.Array(Schema.String),
    indexes: Schema.Array(
        Schema.Struct({
            name: Schema.String,
            valid: Schema.Boolean,
            unique: Schema.Boolean,
            method: Schema.String,
            width: Schema.Int,
            keys: Schema.Array(Schema.Struct({ column: Schema.Boolean, text: Schema.String })),
            classes: Schema.Array(Schema.String),
            predicate: Schema.NullOr(Schema.String),
            definition: Schema.String,
            columns: Schema.Array(Schema.String),
        }),
    ),
    policies: Schema.Array(Schema.Struct({ name: Schema.String, using: Schema.NullOr(Schema.String), check: Schema.NullOr(Schema.String), roles: Schema.Array(Schema.String) })),
});
const Configurations = Schema.fromJsonString(Schema.Array(Schema.Struct({ name: Schema.String, configuration: Schema.String, filters: Schema.Array(Schema.String) })));
class PgPrelude extends Schema.Class<PgPrelude, { readonly brand: unique symbol }>('PgPrelude')({
    migrator: Schema.String,
    absent: Schema.Array(Schema.String),
    configurations: Schema.fromJsonString(Schema.Array(Schema.Struct({ name: Schema.String, configuration: Schema.String, configured: Schema.Boolean, drift: Schema.Array(Schema.Struct({ tokens: Schema.NonEmptyArray(Schema.String), dictionaries: Schema.OptionFromNullOr(Schema.NonEmptyArray(Schema.String)) })) }))),
}) {
    static readonly decoded = Schema.decodeUnknownEffect(Schema.Tuple([PgPrelude]));
    static readonly preamble = (sql: SqlClient.SqlClient, timeouts: Catalog['timeouts']) =>
        UndefinedOr.match(timeouts, {
            onUndefined: () => [],
            onDefined: ({ lock, transaction }) => [sql`SET LOCAL lock_timeout = ${sql.literal(literal(`${Duration.toMillis(lock)}ms`))}`, sql`SET LOCAL transaction_timeout = ${sql.literal(literal(`${Duration.toMillis(transaction)}ms`))}`],
        });
    static readonly open = (sql: SqlClient.SqlClient, preamble: readonly Statement.Statement<object>[], required: readonly string[], filters: readonly string[], configurations: typeof Configurations.Type) =>
        Effect.forEach(preamble, Function.identity, { discard: true }).pipe(
            Effect.andThen(
                sql`SELECT pg_advisory_xact_lock(hashtextextended(current_schema(), 0))::text AS lock, set_config('search_path', concat_ws(', ', quote_ident(current_schema()),
                    (SELECT string_agg(quote_ident(n.nspname), ', ' ORDER BY n.nspname) FROM pg_namespace n WHERE n.oid NOT IN (current_schema()::regnamespace, 'pg_catalog'::regnamespace)
                        AND (EXISTS (SELECT 1 FROM pg_extension e WHERE e.extnamespace = n.oid AND e.extname = ANY(${required}::text[])) OR EXISTS (SELECT 1 FROM pg_ts_dict d WHERE d.dictnamespace = n.oid AND d.dictname = ANY(${filters}::text[])))), 'pg_temp'), true) AS search_path`,
            ),
            Effect.andThen(Schema.encodeEffect(Configurations)(configurations)),
            Effect.flatMap(
                (declared) =>
                    sql`WITH maps AS (SELECT m.mapcfg, m.maptokentype, array_agg(m.mapdict::regdictionary::text ORDER BY m.mapseqno) AS dictionaries FROM pg_ts_config_map m GROUP BY m.mapcfg, m.maptokentype),
                    declared AS (SELECT d.name, d.configuration::regconfig::oid AS base, (SELECT c.oid FROM pg_ts_config c WHERE c.cfgnamespace = current_schema()::regnamespace AND c.cfgname = d.name) AS live, array(SELECT quote_ident(f) FROM unnest(d.filters) f) AS filters FROM json_to_recordset(${declared}::json) d(name text, configuration text, filters text[])),
                    tokens AS (SELECT d.name, t.tokid, t.alias, CASE WHEN b.dictionaries IS NOT NULL THEN d.filters || b.dictionaries END AS wanted, l.dictionaries AS now
                        FROM declared d JOIN pg_ts_config c ON c.oid = d.base CROSS JOIN ts_token_type(c.cfgparser) t
                        LEFT JOIN maps b ON (b.mapcfg, b.maptokentype) = (d.base, t.tokid) LEFT JOIN maps l ON (l.mapcfg, l.maptokentype) = (coalesce(d.live, d.base), t.tokid))
                    SELECT current_user::text AS migrator,
                        array(SELECT a.name FROM (SELECT x FROM unnest(${required}::text[]) x WHERE NOT EXISTS (SELECT 1 FROM pg_extension e WHERE e.extname = x)
                            UNION SELECT f FROM unnest(${filters}::text[]) f WHERE NOT EXISTS (SELECT 1 FROM pg_ts_dict d WHERE d.dictname = f) AND EXISTS (SELECT 1 FROM pg_available_extensions v WHERE v.name = f)) a(name) ORDER BY a.name) AS absent, array_to_json(array(SELECT json_build_object('name', d.name, 'configuration', d.base::regconfig::text, 'configured', d.live IS NOT NULL,
                        'drift', array(SELECT json_build_object('tokens', array_agg(k.alias ORDER BY k.tokid), 'dictionaries', k.wanted) FROM tokens k WHERE k.name = d.name AND k.now IS DISTINCT FROM k.wanted GROUP BY k.wanted)) FROM declared d))::text AS configurations`,
            ),
            Effect.flatMap(PgPrelude.decoded),
            Effect.map(([prelude]) => prelude),
        );
}
class Pg extends Schema.Class<Pg, { readonly brand: unique symbol }>('Pg')({ tables: Schema.Array(PgRelation), scratch: Schema.Array(PgRelation) }) {
    // --- [ROWS]
    static readonly types = { text: 'text', uuid: 'uuid', json: 'jsonb', instant: 'timestamptz(3)', integer: 'bigint', int64: 'bigint', decimal: 'numeric', real: 'double precision', number: 'double precision', boolean: 'boolean', blob: 'bytea' } as const satisfies Engine['types'];
    static readonly member: Engine['member'] = (sql, field) => sql`e.value ->> ${sql.literal(literal(field))}`;
    static readonly element: Engine['element'] = (sql, { kind }, text) => (kind === 'blob' ? sql`decode(${text}, 'hex')` : sql`(${text})::${sql.literal(Pg.types[kind])}`);
    static readonly rows = (sql: SqlClient.SqlClient, body: string) => sql`jsonb_array_elements(${body}) e(value)`;
    static readonly texts: Engine['texts'] = (sql, body) => sql`jsonb_array_elements_text(${body}) WITH ORDINALITY e(value, position)`;
    static readonly insertion: Engine['insertion'] = (sql, body, order) => sql`${Pg.rows(sql, body)} ORDER BY ${order}`;
    static readonly read: Engine['read'] = (sql, { name, kind }, scope) => (kind === 'int64' ? sql`${scope}${sql(name)}::text AS ${sql(name)}` : sql`${scope}${sql(name)}`);
    static readonly upsert: Engine['upsert'] = ({ sql, owner, bare, ignore, keys, reads, inserting }, body, set) =>
        inserting(
            body,
            Array.match(set, {
                onEmpty: () => ignore,
                onNonEmpty: (changed) => {
                    const parts = changed.map(({ name }) => sql(name));
                    return sql`ON CONFLICT (${keys(bare)}) DO UPDATE SET ${sql.csv(parts.map((part) => sql`${part} = excluded.${part}`))} WHERE (${sql.csv(parts.map((part) => sql`${owner}.${part}`))}) IS DISTINCT FROM (${sql.csv(parts.map((part) => sql`excluded.${part}`))})`;
                },
            }),
            sql.csv('WITH (OLD AS inserted)', [reads(bare), 'inserted IS NULL AS inserted']),
        );
    static readonly ensure: Engine['ensure'] = ({ sql, owner, bare, order, ignore, keys, reads, inserting }, body) => {
        const outer = sql.literal('t.');
        return sql`WITH inserted AS (${inserting(body, ignore, sql.literal('*'))}) SELECT ${reads(outer)} FROM (SELECT * FROM inserted UNION ALL SELECT * FROM ${owner} WHERE (${keys(bare)}) IN (SELECT ${order} FROM ${Pg.rows(sql, body)})) t ORDER BY ${keys(outer)}`;
    };
    static readonly latest: Engine['latest'] = ({ sql, reads }, body, ranked, newest) => {
        const outer = sql.literal('l.');
        return sql`SELECT ${reads(outer)} FROM ${Pg.texts(sql, body)} CROSS JOIN LATERAL (${newest(sql.literal('*'))}) l ORDER BY e.position, ${ranked(outer)}`;
    };
    static readonly configuration = ({ configuration, filters }: { readonly configuration: string; readonly filters: readonly string[] }) => identifier([configuration, ...filters]);
    static readonly document = (sql: SqlClient.SqlClient, { columns, tokenizer }: Parameters<Engine['fulltext']>[2]) => {
        const joined = sql.join(` || ' ' || `, false)(columns.map(({ name }) => sql`coalesce(${sql(name)}, '')`));
        return tokenizer === 'trigram' ? joined : sql`to_tsvector(${sql.literal(literal(Pg.configuration(tokenizer)))}, ${joined})`;
    };
    static readonly search: Engine['search'] = ({ sql, owner, prefix, bare, keys, reads }, searchable, text, pattern, marks, count) => {
        const { columns: searched, tokenizer } = searchable;
        const body = Pg.document(sql, searchable);
        const marked = (mark: (name: string, open: string, close: string) => Statement.Fragment) => Option.match(marks, { onNone: () => sql.literal('NULL'), onSome: ({ open, close }) => sql`jsonb_build_object(${sql.csv(searched.flatMap((entry) => [literal(entry.field), mark(entry.name, open, close)]))})` });
        return tokenizer === 'trigram'
            ? sql`SELECT ${reads(bare)}, word_similarity(${text}, ${body}) AS rank, ${marked((part, open, close) => sql`regexp_replace(${sql(part)}, ${`***=${text}`}, ${`${open.replaceAll('\\', '\\\\')}\\&${close.replaceAll('\\', '\\\\')}`}, 'gi')`)} AS highlight FROM ${owner} WHERE (${body}) ILIKE ${pattern} ORDER BY rank DESC, ${keys(prefix)} LIMIT ${count}`
            : sql`SELECT ${reads(bare)}, ts_rank(${body}, q) AS rank, ${marked((part, open, close) => sql`ts_headline(${sql.literal(literal(Pg.configuration(tokenizer)))}, ${sql(part)}, q, ${`StartSel="${open.replaceAll('"', '""')}", StopSel="${close.replaceAll('"', '""')}", HighlightAll=true`})`)} AS highlight FROM ${owner} CROSS JOIN plainto_tsquery(${sql.literal(literal(Pg.configuration(tokenizer)))}, ${text}) q WHERE ${body} @@ q ORDER BY rank DESC, ${keys(prefix)} LIMIT ${count}`;
    };

    // --- [DDL]
    static readonly constant: Engine['constant'] = (entry, value) => (entry.kind === 'boolean' ? `${value === 1}` : constant(entry, value));
    static readonly codePoints: Engine['codePoints'] = (sql, subject) => sql`char_length(${subject})`;
    static readonly domain: Engine['domain'] = (sql, subject, kind) =>
        Match.value(kind).pipe(
            Match.whenOr('real', 'decimal', () => [sql`${subject} NOT IN ('NaN', 'Infinity', '-Infinity')`]),
            Match.when('instant', () => [sql`isfinite(${subject})`]),
            Match.orElse(() => []),
        );
    static readonly projection: Engine['projection'] = (sql, { kind }, from, path) => sql`(${sql(from)} ${sql.literal(kind === 'json' ? '#>' : '#>>')} ARRAY[${sql.literal(path.map(literal).join(', '))}])${sql.literal(['json', 'text'].includes(kind) ? '' : `::${Pg.types[kind]}`)}`;
    static readonly generated: Engine['generated'] = (sql, expression) => sql`GENERATED ALWAYS AS (${expression}) VIRTUAL`;
    static readonly constraint: Engine['constraint'] = (sql, name, text) => sql`CONSTRAINT ${sql(name)} ${text}`;
    static readonly casefold: Engine['casefold'] = (sql, name) => sql`casefold(${sql(name)} COLLATE pg_unicode_fast)`;
    static readonly opclass: Engine['opclass'] = (method, json) =>
        Match.value({ method, json }).pipe(
            Match.when({ method: 'gin', json: true }, () => ({ name: 'jsonb_path_ops', extensions: [] })),
            Match.when({ method: Match.is('gin', 'gist') }, ({ method: kind }) => ({ name: trigram.classes[kind], extensions: [trigram.extension] })),
            Match.option,
        );
    static readonly projected: Engine['projected'] = (sql, expression) => sql`(${expression})`;
    static readonly methods: Engine['methods'] = ['btree', 'gin', 'gist', 'brin'];
    static readonly covering: Engine['covering'] = (sql, _, include) => ({ keys: [], clause: Array.isReadonlyArrayNonEmpty(include) ? [sql`INCLUDE (${sql.csv(include.map((name) => sql`${sql(name)}`))})`] : [] });
    static readonly suffix: Engine['suffix'] = () => '';
    static readonly fulltext: Engine['fulltext'] = (sql, owner, search) => {
        const [name, body, words] = [
            identifier([owner, 'search']),
            Pg.document(sql, search),
            Option.map(
                Option.liftPredicate(search.tokenizer, (tokenizer) => tokenizer !== 'trigram'),
                (tokenizer) => ({ ...tokenizer, name: Pg.configuration(tokenizer) }),
            ),
        ];
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
            fts: Option.none(),
        };
    };
    static readonly policy: Engine['policy'] = (sql, owner, name, type, setting) => {
        const [policy, rule] = [identifier([owner, 'tenant']), sql`${sql(name)} = nullif(current_setting(${sql.literal(literal(setting))}, true), '')::${sql.literal(type)}`];
        return Option.some({ name: policy, rule, text: (on, role) => sql`CREATE POLICY ${sql(policy)} ON ${on} TO ${role} USING (${rule}) WITH CHECK (${rule})` });
    };
    static readonly commented = (sql: SqlClient.SqlClient, relation: Statement.Fragment, column: Option.Option<string>) => Option.match(column, { onNone: () => sql`TABLE ${relation}`, onSome: (name) => sql`COLUMN ${relation}.${sql(name)}` });
    static readonly comment: Engine['comment'] = (sql, name, { origin, incarnation }) => ({
        inline: sql.literal(''),
        statements: (relation, column) => (origin === name && incarnation === 0 ? [] : [sql`COMMENT ON ${Pg.commented(sql, relation, column)} IS ${sql.literal(literal(JSON.stringify({ origin, incarnation })))}`]),
    });

    // --- [RECONCILE]
    static readonly refusals: Engine['refusals'] = () => [];
    static readonly open: Engine['open'] = () => Effect.void;
    static readonly observe = (sql: SqlClient.SqlClient, excluded: readonly string[]) =>
        sql`WITH relations AS (SELECT c.oid, c.relname, c.relowner, c.relacl, c.relrowsecurity, c.relforcerowsecurity, c.relnamespace = pg_my_temp_schema() AS scratch FROM pg_class c WHERE c.relnamespace IN (current_schema()::regnamespace, pg_my_temp_schema()) AND c.relkind = 'r' AND NOT EXISTS (SELECT 1 FROM unnest(${excluded}::text[]) p(prefix) WHERE starts_with(c.relname::text, p.prefix))),
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
            SELECT json_build_object('tables', array(SELECT d.relation FROM described d WHERE NOT d.scratch), 'scratch', array(SELECT d.relation FROM described d WHERE d.scratch))::text AS catalog`.pipe(
            Effect.flatMap(Schema.decodeUnknownEffect(Schema.Tuple([Schema.Struct({ catalog: Schema.fromJsonString(Pg) })]))),
            Effect.map(([{ catalog }]) => catalog),
        );
    static readonly plan: Engine['plan'] = Effect.fnUntraced(function* (sql: SqlClient.SqlClient, catalog: Catalog, runtime: Option.Option<string>) {
        const drawings = catalog.tables.map((declared) => ({ declared, rendered: render(sql, Pg, catalog, declared) }));
        const indexes = drawings.flatMap(({ rendered }) => rendered.indexes);
        const required = Array.dedupe(indexes.flatMap(Struct.get('extensions')));
        const owned = Array.dedupeWith(
            indexes.flatMap(({ configuration }) => Option.toArray(configuration)),
            (self, that) => self.name === that.name,
        );
        const preamble = PgPrelude.preamble(sql, catalog.timeouts);
        const { migrator, absent, configurations } = yield* PgPrelude.open(sql, preamble, required, Array.dedupe(owned.flatMap(Struct.get('filters'))), owned);
        const [existing, drifted] = [configurations.filter(Struct.get('configured')).map(Struct.get('name')), configurations.filter(({ drift }) => Array.isReadonlyArrayNonEmpty(drift)).map(Struct.get('name'))];
        const ready = ({ extensions: needed, configuration }: (typeof indexes)[number]) => !needed.some((name) => absent.includes(name)) && Option.toArray(configuration).every(({ name }) => existing.includes(name));
        const role = Option.match(runtime, { onNone: () => sql.literal('PUBLIC'), onSome: (name) => sql`${sql(name)}` });
        const block = (body: string) => sql`DO ${sql.literal(literal(`BEGIN ${body} END`))}`;
        yield* block(
            drawings
                .map(({ declared }) => render(sql, Pg, catalog, declared, { target: (name) => sql`pg_temp.${sql(name)}` }))
                .flatMap((rendered) => [sql`${rendered.create} ON COMMIT DROP`, ...rendered.indexes.filter(ready).map(({ text }) => text('')), ...Option.toArray(rendered.policy).map(({ text }) => text(role))])
                .map((statement) => `${statement.compile(true)[0]};`)
                .join(' '),
        );
        const live = yield* Pg.observe(sql, catalog.excluded ?? []);
        const resolved = diff(sql, catalog, drawings, { tables: live.tables, columns: ({ columns }) => columns });
        const privileges = [...Option.toArray(runtime).map((grantee) => ({ grantee, all: ['SELECT', 'INSERT', 'UPDATE', 'DELETE'] })), ...Array.fromNullishOr(catalog.reader).map((grantee) => ({ grantee, all: ['SELECT'] }))].filter(({ grantee }) => grantee !== migrator);
        const grants = (on: Statement.Fragment, held: readonly (readonly [string, string])[]) =>
            privileges.flatMap(({ grantee, all }) =>
                Array.match(
                    Array.difference(
                        all,
                        held.filter(([holder]) => holder === grantee).map(([, granted]) => granted),
                    ),
                    { onEmpty: () => [], onNonEmpty: (missing) => [sql`GRANT ${sql.literal(missing.join(', '))} ON ${on} TO ${sql(grantee)}`] },
                ),
            );
        const stamped = (relation: Statement.Fragment, column: Option.Option<string>, name: string, recorded: Option.Option<typeof Identity.Type>, identity: typeof Identity.Type) =>
            Array.match(Pg.comment(sql, name, identity).statements(relation, column), {
                onEmpty: () => (Option.isSome(recorded) ? [sql`COMMENT ON ${Pg.commented(sql, relation, column)} IS NULL`] : []),
                onNonEmpty: (writes) => (Option.exists(recorded, ({ origin, incarnation }) => origin === identity.origin && incarnation === identity.incarnation) ? [] : writes),
            });
        const shape = (row: (typeof PgRelation.Type)['indexes'][number], rename: (column: string) => string) => ({ unique: row.unique, method: row.method, width: row.width, classes: row.classes, predicate: row.predicate, keys: row.keys.map(({ column, text }) => (column ? rename(text) : text)) });
        const analyze = (entry: (typeof resolved.matched)[number], desired: typeof PgRelation.Type) => {
            const { name } = entry.declared;
            const owner = entry.rendered.relation;
            const prior = (parts: readonly string[]) => identifier([entry.live.name, ...parts.map(entry.original)]);
            const named = [...entry.live.checks, ...entry.live.references].map(({ name: constraint }) => constraint);
            const relabels = [...entry.rendered.definitions.flatMap(({ check }) => Option.toArray(check)), ...entry.rendered.links]
                .filter(({ name: target, parts }) => named.includes(prior(parts)) && prior(parts) !== target && !named.includes(target))
                .map(({ name: target, parts }) => ({ from: prior(parts), to: target }));
            const relabeled = (constraint: string) => relabels.find(({ from }) => from === constraint)?.to ?? constraint;
            const links = entry.live.references.map((row) => ({ ...row, name: relabeled(row.name), columns: Array.map(row.columns, entry.renamed), target: resolved.referenced(row.target).name }));
            const stale = entry.rendered.links.filter((link) => !links.some((row) => row.name === link.name && Equal.equals(row.columns, link.columns) && row.target === link.target && row.onDelete === link.onDelete));
            const planned = (index: (typeof entry.rendered.indexes)[number]) => {
                const target = Array.findFirst(desired.indexes, (row) => row.name === index.name);
                const fits = (row: (typeof entry.live.indexes)[number]) => row.valid && Option.exists(target, (wanted) => Equal.equals(shape(row, entry.renamed), shape(wanted, Function.identity)));
                const present = Array.findFirst(entry.live.indexes, (row) => row.name === index.name);
                const before = Option.filter(
                    Array.findFirst(entry.live.indexes, (row) => row.name === prior(index.parts)),
                    (row) => row.name !== index.name,
                );
                const [keep, rename] = [Option.exists(present, fits), Option.isNone(present) && Option.exists(before, fits)];
                return { index, keep, rename, dropped: [...Option.toArray(present).filter(() => !keep), ...Option.toArray(before).filter(() => !rename)].map((row) => row.name) };
            };
            const plans = entry.rendered.indexes.map(planned);
            const policy = (rule: Option.Option.Value<typeof entry.rendered.policy>) => {
                const before = Option.filter(
                    Array.findFirst(entry.live.policies, (row) => row.name === prior(['tenant'])),
                    (row) => row.name !== rule.name && !entry.live.policies.some((other) => other.name === rule.name),
                );
                const wanted = Array.findFirst(desired.policies, (row) => row.name === rule.name);
                return [
                    ...Option.toArray(before).map((row) => sql`ALTER POLICY ${sql(row.name)} ON ${owner} RENAME TO ${sql(rule.name)}`),
                    ...Option.match(
                        Option.orElse(
                            Array.findFirst(entry.live.policies, (row) => row.name === rule.name),
                            () => before,
                        ),
                        { onNone: () => [rule.text(role)], onSome: (present) => (Option.contains(wanted, { ...present, name: rule.name }) ? [] : [sql`ALTER POLICY ${sql(rule.name)} ON ${owner} TO ${role} USING (${rule.rule}) WITH CHECK (${rule.rule})`]) },
                    ),
                ];
            };
            const pairs = (entry.owned ? entry.fields.filter(({ definition }) => definition.retype) : []).flatMap(({ definition: column, found }) => Option.toArray(Option.all({ wants: Array.findFirst(desired.columns, (row) => row.name === column.name), now: found })).map((pair) => ({ column, ...pair })));
            const moved = pairs.filter(({ wants, now }) => now.type !== wants.type);
            const constrained = (rows: readonly (typeof PgRelation.Type)['checks'][number][]) => rows.filter((row) => pairs.some(({ column }) => Equal.equals(row.columns, [column.name])));
            const [had, needs] = [constrained(entry.live.checks.map((row) => ({ ...row, name: relabeled(row.name), columns: row.columns.map(entry.renamed) }))), constrained(desired.checks)];
            const unmatched = Array.differenceWith<(typeof had)[number]>((self, that) => self.definition === that.definition);
            const [obsolete, missing] = [unmatched(had, needs), unmatched(needs, had)];
            const dependents = entry.live.columns.filter((row) => row.generated !== '' && moved.some(({ now }) => row.sources.includes(now.name)));
            const tightened = pairs.filter(({ wants, now }) => wants.required && !now.required).map(({ column }) => ({ column: column.name, constraint: identifier([name, column.name, 'not', 'null']) }));
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
                        block(
                            sql`IF EXISTS (SELECT 1 FROM ${owner} WHERE (${sql(column.name)}::${sql.literal(wants.type)})::${sql.literal(now.type)} IS DISTINCT FROM ${sql(column.name)}) THEN RAISE EXCEPTION 'ALTER COLUMN %.% TYPE % changes stored values', ${sql.literal(literal(name))}, ${sql.literal(literal(column.name))}, ${sql.literal(literal(wants.type))} USING ERRCODE = 'check_violation'; END IF;`.compile(
                                true,
                            )[0],
                        ),
                    ),
                ...Array.match(changes, { onEmpty: () => [], onNonEmpty: (all) => [sql`ALTER TABLE ${owner} ${sql.csv(all)}`] }),
                ...tightened.map(({ column, constraint }) => sql`ALTER TABLE ${owner} ADD CONSTRAINT ${sql(constraint)} NOT NULL ${sql(column)} NOT VALID`),
                ...missing.map((row) => sql`ALTER TABLE ${owner} ADD CONSTRAINT ${sql(row.name)} ${sql.literal(row.definition)} NOT VALID`),
                ...dependents.flatMap((row) => [
                    sql`ALTER TABLE ${owner} ADD COLUMN ${sql(row.name)} ${sql.literal(row.type)} GENERATED ALWAYS AS (${sql.literal(`${row.expression}`)}) ${sql.literal(row.generated === 's' ? 'STORED' : 'VIRTUAL')}`,
                    ...Array.fromNullishOr(row.identity).map((comment) => sql`COMMENT ON COLUMN ${owner}.${sql(row.name)} IS ${sql.literal(literal(comment))}`),
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
                    ...entry.doomed.map((row) => sql`ALTER TABLE ${owner} DROP COLUMN ${sql(row.name)}`),
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
                    ...entry.fields.filter(({ found }) => Option.isSome(found)).flatMap(({ definition, recorded, identity }) => stamped(owner, Option.some(definition.name), definition.name, recorded, identity)),
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
            drifted: ({ declared, live: table }, { definition, found }) =>
                Option.exists(Option.all({ desired: scratch(declared.name), now: found }), ({ desired, now }) => {
                    const checks = ({ checks: rows }: typeof PgRelation.Type, column: string) => Array.sort(rows.filter(({ columns }) => Equal.equals(columns, [column])).map(Struct.get('definition')), Order.String);
                    return Option.exists(
                        Array.findFirst(desired.columns, (row) => row.name === definition.name),
                        (wants) => now.type !== wants.type || now.required !== wants.required || now.generated !== wants.generated || (wants.generated === '' && now.expression !== wants.expression) || (now.name === definition.name && !Equal.equals(checks(table, now.name), checks(desired, now.name))),
                    );
                }),
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
                ...resolved.created.flatMap(({ declared, rendered }) => [
                    rendered.create,
                    ...rendered.comments,
                    ...rendered.indexes.map(({ text }) => text('')),
                    ...grants(rendered.relation, []),
                    ...Option.toArray(declared.tenant).flatMap(() => [sql`ALTER TABLE ${rendered.relation} ENABLE ROW LEVEL SECURITY`, sql`ALTER TABLE ${rendered.relation} FORCE ROW LEVEL SECURITY`]),
                    ...Option.toArray(rendered.policy).map(({ text }) => text(role)),
                ]),
                ...tables.flatMap((entry) => entry.statements),
            ],
            concurrent: [
                ...tables.flatMap(({ dropped }) => dropped.map((index) => sql`DROP INDEX CONCURRENTLY IF EXISTS ${sql(index)}`)),
                ...tables.flatMap(({ built }) => built),
                ...tables.flatMap(({ reindexed }) => reindexed),
                ...tables.flatMap(({ rendered, validated }) => validated.map((constraint) => sql`ALTER TABLE ${rendered.relation} VALIDATE CONSTRAINT ${sql(constraint)}`)),
            ],
            undeclared: { indexes: tables.flatMap(({ undeclared }) => undeclared.indexes), references: tables.flatMap(({ undeclared }) => undeclared.references) },
        });
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Pg };
