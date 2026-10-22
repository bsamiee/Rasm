import { D1Client } from '@effect/sql-d1';
import { Array, Cause, Effect, Equal, Function, Option, Order, Record, Result, Schema, Struct } from 'effect';
import { SqlClient, SqlSchema } from 'effect/sql';
import { render } from './ddl.ts';
import { actions, atomic, type Catalog, identifier, literal, names, quote, type Relation, table } from './table.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const refusals = {
    unbounded: 'Numbers without isFinite, which each engine stores differently',
    reserved: 'rowid alias names (rowid, oid, _rowid_), which node SQLite refuses',
    projected: 'projections in a key or reference, which PostgreSQL refuses on virtual columns',
    tenant: 'a tenant scope, which needs PostgreSQL row security',
    search: 'a search key other than one Schema.Int field, which FTS5 rowids need',
    generated: 'undeclared generated columns a rebuild would lose',
};

// --- [MODELS] --------------------------------------------------------------------------

const Entry = Schema.Struct({ origin: Schema.optionalKey(Schema.String), incarnation: Schema.optionalKey(Schema.Int) });
const ledger = table('reconcile_revision', { revision: Schema.Int, identity: Schema.fromJsonString(Schema.Record(Schema.String, Schema.Struct({ ...Entry.fields, columns: Schema.optionalKey(Schema.Record(Schema.String, Entry)) }))) }, { key: ['revision'] });
const Finding = Schema.TaggedUnion({
    Unsupported: { table: Schema.String, reason: Schema.Literals(Record.keys(refusals)), columns: Schema.NonEmptyArray(Schema.String) },
    Required: { table: Schema.String, columns: Schema.NonEmptyArray(Schema.String) },
    RetainedReference: { table: Schema.String, columns: Schema.NonEmptyArray(Schema.String), target: Schema.String },
});
class Sqlite extends Schema.Class<Sqlite, { readonly brand: unique symbol }>('Sqlite')({
    tables: Schema.Array(
        Schema.Struct({
            name: Schema.String,
            strict: Schema.BooleanFromBit,
            withoutRowid: Schema.BooleanFromBit,
            autoincrement: Schema.BooleanFromBit,
            sql: Schema.String,
            columns: Schema.Array(Schema.Struct({ name: Schema.String, type: Schema.String, required: Schema.BooleanFromBit, fallback: Schema.OptionFromNullOr(Schema.String), hidden: Schema.Int })),
            references: Schema.Array(Schema.Struct({ target: Schema.String, onDelete: Schema.Literals(Record.keys(actions)), onUpdate: Schema.Literals(Record.keys(actions)), columns: Schema.NonEmptyArray(Schema.String), to: Schema.Array(Schema.String) })),
            indexes: Schema.Array(Schema.Struct({ name: Schema.String, sql: Schema.String, keys: Schema.Array(Schema.NullOr(Schema.String)) })),
            triggers: Schema.Array(Schema.Struct({ name: Schema.String, sql: Schema.String })),
        }),
    ),
    virtual: Schema.Array(Schema.Struct({ name: Schema.String, sql: Schema.String })),
}) {
    static readonly observe = (sql: SqlClient.SqlClient) =>
        sql`WITH relations AS (SELECT l.name, l.type, l.strict, l.wr, m.sql FROM pragma_table_list l JOIN sqlite_schema m ON m.name = l.name WHERE l.schema = 'main' AND l.name NOT GLOB 'sqlite_*' AND l.name NOT GLOB '_cf_*')
            SELECT json_object(
                'tables', (SELECT json_group_array(json_object('name', r.name, 'strict', r.strict, 'withoutRowid', r.wr, 'autoincrement', r.sql LIKE '%AUTOINCREMENT%', 'sql', r.sql,
                    'columns', (SELECT json_group_array(json_object('name', x.name, 'type', x.type, 'required', x."notnull", 'fallback', x.dflt_value, 'hidden', x.hidden)) FROM pragma_table_xinfo(r.name) x),
                    'references', (SELECT json_group_array(json_object('target', f."table", 'onDelete', f.on_delete, 'onUpdate', f.on_update,
                        'columns', (SELECT json_group_array(k."from" ORDER BY k.seq) FROM pragma_foreign_key_list(r.name) k WHERE k.id = f.id),
                        'to', (SELECT json_group_array(k."to" ORDER BY k.seq) FROM pragma_foreign_key_list(r.name) k WHERE k.id = f.id AND k."to" IS NOT NULL))) FROM pragma_foreign_key_list(r.name) f WHERE f.seq = 0),
                    'indexes', (SELECT json_group_array(json_object('name', m.name, 'sql', m.sql, 'keys', (SELECT json_group_array(k.name) FROM pragma_index_xinfo(m.name) k WHERE k."key" = 1))) FROM sqlite_schema m WHERE m.type = 'index' AND m.tbl_name = r.name AND m.sql IS NOT NULL),
                    'triggers', (SELECT json_group_array(json_object('name', m.name, 'sql', m.sql)) FROM sqlite_schema m WHERE m.type = 'trigger' AND m.tbl_name = r.name))) FROM relations r WHERE r.type = 'table'),
                'virtual', (SELECT json_group_array(json_object('name', r.name, 'sql', r.sql)) FROM relations r WHERE r.type = 'virtual')
            ) AS catalog`.pipe(
            Effect.flatMap(Schema.decodeUnknownEffect(Schema.Tuple([Schema.Struct({ catalog: Schema.fromJsonString(Sqlite) })]))),
            Effect.map(([{ catalog }]) => catalog),
        );
}
const PgRelation = Schema.Struct({
    name: Schema.String,
    rls: Schema.Boolean,
    force: Schema.Boolean,
    grants: Schema.Array(Schema.Tuple([Schema.String, Schema.String])),
    columns: Schema.Array(Schema.Struct({ name: Schema.String, type: Schema.String, expression: Schema.NullOr(Schema.String), generated: Schema.Literals(['', 's', 'v']), required: Schema.Boolean, sources: Schema.Array(Schema.String) })),
    checks: Schema.Array(Schema.Struct({ name: Schema.String, definition: Schema.String, columns: Schema.Array(Schema.String) })),
    references: Schema.Array(Schema.Struct({ name: Schema.String, columns: Schema.NonEmptyArray(Schema.String), target: Schema.String, onDelete: Schema.String })),
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
class Pg extends Schema.Class<Pg, { readonly brand: unique symbol }>('Pg')({ tables: Schema.Array(PgRelation), scratch: Schema.Array(PgRelation) }) {
    static readonly observe = (sql: SqlClient.SqlClient) =>
        sql`WITH relations AS (SELECT c.oid, c.relname, c.relowner, c.relacl, c.relrowsecurity, c.relforcerowsecurity, c.relnamespace = pg_my_temp_schema() AS scratch FROM pg_class c WHERE c.relnamespace IN (current_schema()::regnamespace, pg_my_temp_schema()) AND c.relkind = 'r'),
            constraints AS (SELECT n.oid, n.conrelid, n.conname, n.contype, n.convalidated, n.confrelid, n.confdeltype,
                array(SELECT a.attname::text FROM unnest(n.conkey) WITH ORDINALITY k(number, position) JOIN pg_attribute a ON (a.attrelid, a.attnum) = (n.conrelid, k.number) ORDER BY k.position) AS columns
                FROM pg_constraint n JOIN relations r ON r.oid = n.conrelid),
            described AS (SELECT r.scratch, json_build_object('name', r.relname, 'rls', r.relrowsecurity, 'force', r.relforcerowsecurity,
                'grants', array(SELECT json_build_array(a.grantee::regrole::text, a.privilege_type) FROM aclexplode(r.relacl) a WHERE a.grantee <> r.relowner),
                'columns', array(SELECT json_build_object('name', a.attname, 'type', format_type(a.atttypid, a.atttypmod), 'expression', pg_get_expr(d.adbin, d.adrelid), 'generated', a.attgenerated::text,
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
}
class PgPrelude extends Schema.Class<PgPrelude, { readonly brand: unique symbol }>('PgPrelude')({
    migrator: Schema.String,
    extensions: Schema.Array(Schema.String),
    configurations: Schema.fromJsonString(Schema.Array(Schema.Struct({ name: Schema.String, configuration: Schema.String, configured: Schema.Boolean, drift: Schema.Array(Schema.Struct({ tokens: Schema.NonEmptyArray(Schema.String), dictionaries: Schema.OptionFromNullOr(Schema.NonEmptyArray(Schema.String)) })) }))),
}) {
    static readonly preamble = ["SET LOCAL lock_timeout = '5s'", "SET LOCAL transaction_timeout = '2min'"];
    static readonly open = (sql: SqlClient.SqlClient, required: readonly string[], configurations: readonly Extract<Option.Option.Value<Relation['table']['search']>['tokenizer'], object>[]) =>
        Effect.forEach(PgPrelude.preamble, (text) => sql.unsafe(text), { discard: true }).pipe(
            Effect.andThen(
                sql`SELECT pg_advisory_xact_lock(hashtextextended(current_schema(), 0))::text AS lock, set_config('search_path', concat_ws(', ', quote_ident(current_schema()),
                    (SELECT string_agg(DISTINCT quote_ident(n.nspname), ', ') FROM pg_extension e JOIN pg_namespace n ON n.oid = e.extnamespace WHERE e.extname::text = ANY(${required}::text[]) AND n.oid <> current_schema()::regnamespace), 'pg_temp'), true) AS search_path`,
            ),
            Effect.andThen(
                sql`WITH maps AS (SELECT m.mapcfg, m.maptokentype, array_agg(m.mapdict::regdictionary::text ORDER BY m.mapseqno) AS dictionaries FROM pg_ts_config_map m GROUP BY m.mapcfg, m.maptokentype),
                    declared AS (SELECT d.name, d.configuration::regconfig::oid AS base, (SELECT c.oid FROM pg_ts_config c WHERE c.cfgnamespace = current_schema()::regnamespace AND c.cfgname = d.name) AS live, array(SELECT quote_ident(f) FROM unnest(d.filters) f) AS filters FROM json_to_recordset(${JSON.stringify(configurations)}::json) d(name text, configuration text, filters text[])),
                    tokens AS (SELECT d.name, t.tokid, t.alias, CASE WHEN b.dictionaries IS NOT NULL THEN d.filters || b.dictionaries END AS wanted, l.dictionaries AS now
                        FROM declared d JOIN pg_ts_config c ON c.oid = d.base CROSS JOIN ts_token_type(c.cfgparser) t
                        LEFT JOIN maps b ON (b.mapcfg, b.maptokentype) = (d.base, t.tokid) LEFT JOIN maps l ON (l.mapcfg, l.maptokentype) = (coalesce(d.live, d.base), t.tokid))
                    SELECT current_user::text AS migrator, array(SELECT extname::text FROM pg_extension) AS extensions, array_to_json(array(SELECT json_build_object('name', d.name, 'configuration', d.base::regconfig::text, 'configured', d.live IS NOT NULL,
                        'drift', array(SELECT json_build_object('tokens', array_agg(k.alias ORDER BY k.tokid), 'dictionaries', k.wanted) FROM tokens k WHERE k.name = d.name AND k.now IS DISTINCT FROM k.wanted GROUP BY k.wanted)) FROM declared d))::text AS configurations`,
            ),
            Effect.flatMap(Schema.decodeUnknownEffect(Schema.Tuple([PgPrelude]))),
            Effect.map(([prelude]) => prelude),
        );
}

// --- [ERRORS] --------------------------------------------------------------------------

class ReconcileError extends Schema.TaggedError<ReconcileError, { readonly brand: unique symbol }>()('ReconcileError', { findings: Schema.NonEmptyArray(Finding) }) {
    override get message() {
        return this.findings
            .map(
                Finding.match({
                    Unsupported: ({ table: owner, reason, columns }) => `${owner} (${columns.join(', ')}): ${refusals[reason]}`,
                    Required: ({ table: owner, columns }) => `${owner} (${columns.join(', ')}): required with no default for existing rows`,
                    RetainedReference: ({ table: owner, columns, target }) => `${owner} (${columns.join(', ')}): references ${target}, which this plan replaces`,
                }),
            )
            .join('; ');
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [RENDER]
const draw = (dialect: Parameters<typeof render>[0], catalog: Catalog) => [ledger, ...catalog.tables].map((declared) => ({ declared, rendered: render(dialect, declared, quote, { elements: [], autoincrement: false }) }));

// --- [IDENTITY]
const refused = <C extends { readonly name: string }>(owner: string, reason: keyof typeof refusals, rows: readonly C[], refuses: (row: C) => boolean) =>
    Array.match(
        rows.filter(refuses).map(({ name }) => name),
        { onEmpty: () => [], onNonEmpty: (columns) => [Finding.cases.Unsupported.make({ table: owner, reason, columns })] },
    );
const refuse = (sql: SqlClient.SqlClient, catalog: Catalog) =>
    Array.match(
        catalog.tables.flatMap(({ table: facts }) => {
            const [all, linked] = [Object.values(facts.columns), [...facts.key, ...facts.references.flatMap(({ columns }) => columns)]];
            const unkeyed = Option.isSome(facts.search) && (facts.withoutRowid || !all.some(({ name, kind }) => kind === 'integer' && Equal.equals(facts.key, [name])));
            return [
                ...refused(facts.name, 'unbounded', all, ({ kind }) => kind === 'number'),
                ...refused(facts.name, 'reserved', all, ({ name }) => ['rowid', 'oid', '_rowid_'].includes(name.toLowerCase())),
                ...refused(facts.name, 'projected', all, ({ name, projection }) => Option.isSome(projection) && linked.includes(name)),
                ...sql.onDialectOrElse({ sqlite: () => [...refused(facts.name, 'tenant', all, ({ name }) => Option.contains(facts.tenant, name)), ...refused(facts.name, 'search', all, ({ name }) => unkeyed && facts.key.includes(name))], orElse: () => [] }),
            ];
        }),
        { onEmpty: () => Effect.void, onNonEmpty: (findings) => Effect.fail(ReconcileError.make({ findings })) },
    );
const match = <L extends { readonly name: string }, C extends { readonly name: string }>(catalog: Catalog, drawings: ReturnType<typeof draw>, { stored, tables, columns }: { readonly stored: Option.Option<typeof ledger.Type>; readonly tables: readonly L[]; readonly columns: (live: L) => readonly C[] }) => {
    const { revision, identity } = Option.getOrElse(stored, (): typeof ledger.Type => ({ revision: 0, identity: {} }));
    const claim = <R extends { readonly name: string }, D extends { readonly name: string; readonly earlier: Option.Option<string>; readonly incarnation: number }>(live: readonly R[], recorded: Readonly<Record<string, typeof Entry.Type>>, retired: readonly string[], declared: readonly D[]) => {
        const [standing, retiring] = Array.partition(live, (row) => {
            const { origin = row.name, incarnation = 0 } = recorded[row.name] ?? {};
            return incarnation < Array.countBy(retired, Equal.equals(row.name)) ? Result.fail(row) : Result.succeed({ row, origin, incarnation });
        });
        const found = declared.map((item) => {
            const owns = (candidate: (typeof standing)[number]) => candidate.incarnation === item.incarnation && [candidate.row.name, candidate.origin].some((name) => name === item.name || Option.contains(item.earlier, name));
            const chosen = Array.head(
                Array.sort(
                    standing.filter((candidate) => owns(candidate) || candidate.row.name === item.name),
                    Order.mapInput(Order.Tuple([Order.Boolean, Order.Boolean]), (candidate: (typeof standing)[number]) => [!owns(candidate), candidate.row.name !== item.name] as const),
                ),
            );
            const { origin, incarnation } = Option.getOrElse(chosen, () => ({ origin: Option.getOrElse(item.earlier, () => item.name), incarnation: item.incarnation }));
            return { item, row: Option.map(chosen, ({ row }) => row), owned: Option.exists(chosen, owns), entry: { ...(origin === item.name ? {} : { origin }), ...(incarnation === 0 ? {} : { incarnation }) } };
        });
        return { found, retiring, claimed: found.flatMap(({ row }) => Option.toArray(row)) };
    };
    const retiredTables = catalog.retired.map(names.transformQueryNames);
    const located = claim(
        tables,
        identity,
        retiredTables,
        drawings.map((drawing) => ({ drawing, name: drawing.declared.table.name, earlier: drawing.declared.table.earlier, incarnation: Array.countBy(retiredTables, Equal.equals(drawing.declared.table.name)) })),
    );
    const entries = located.found.map(({ item: { drawing }, row, owned, entry }) => {
        const recorded = Option.match(row, { onNone: Record.empty<string, typeof Entry.Type>, onSome: (relation) => identity[relation.name]?.columns ?? {} });
        const live = Option.match(row, { onNone: () => [], onSome: columns });
        const resolved = claim(live, recorded, drawing.declared.table.retired.columns, drawing.rendered.definitions);
        const fields = resolved.found.map(({ item: definition, row: found }) => ({ definition, found }));
        const renames = fields.flatMap(({ definition, found }) =>
            Option.toArray(found)
                .filter(({ name }) => name !== definition.name)
                .map(({ name }) => ({ from: name, to: definition.name })),
        );
        const doomed = owned ? resolved.retiring : [];
        const retained = Array.difference(live, [...doomed, ...resolved.claimed]);
        const identities = Record.filter(Record.fromEntries([...resolved.found.map(({ item, entry: column }) => [item.name, column] as const), ...retained.map(({ name }) => [name, recorded[name] ?? {}] as const)]), (column) => Object.keys(column).length > 0);
        return {
            ...drawing,
            row,
            owned,
            fields,
            doomed,
            retained,
            renames,
            renamed: (name: string) => renames.find(({ from }) => from === name)?.to ?? name,
            original: (name: string) => renames.find(({ to }) => to === name)?.from ?? name,
            added: Array.sortWith(
                fields.filter(({ found }) => Option.isNone(found)).map(({ definition }) => definition),
                (definition) => Option.exists(definition.projection, (projected) => projected._tag === 'Where'),
                Order.Boolean,
            ),
            recorded: [drawing.declared.table.name, { ...entry, ...(Record.isEmptyRecord(identities) ? {} : { columns: identities }) }] as const,
        };
    });
    const [matched, created] = Array.partition(entries, (entry) => Option.match(entry.row, { onNone: () => Result.fail(entry), onSome: (live) => Result.succeed({ ...entry, live, retiredIndexes: entry.declared.table.retired.indexes.map((parts) => identifier([live.name, ...parts])) }) }));
    const kept = Array.difference(tables, [...located.retiring, ...located.claimed]);
    const document = Record.filter(Record.fromEntries([...entries.map(({ recorded }) => recorded), ...kept.map(({ name }) => [name, identity[name] ?? {}] as const)]), (value) => Object.keys(value).length > 0);
    return {
        created,
        matched,
        referenced: (live: string) =>
            Option.match(
                Array.findFirst(matched, (entry) => entry.live.name === live),
                { onNone: () => ({ name: live, renamed: Function.identity<string> }), onSome: (entry) => ({ name: entry.declared.table.name, renamed: entry.renamed }) },
            ),
        removed: located.retiring,
        kept,
        plan: <U extends object>(parts: Record<'preamble' | 'opening' | 'statements' | 'concurrent', readonly string[]> & { readonly findings: readonly (typeof Finding.Type)[]; readonly undeclared: U }) => {
            const statements = [
                ...parts.opening,
                ...located.retiring.map(({ name }) => `DROP TABLE ${quote(name)}`),
                ...matched.filter(({ live, declared }) => live.name !== declared.table.name).map(({ live, rendered }) => `ALTER TABLE ${quote(live.name)} RENAME TO ${rendered.relation}`),
                ...matched.flatMap(({ rendered, renames }) => renames.map(({ from, to }) => `ALTER TABLE ${rendered.relation} RENAME COLUMN ${quote(from)} TO ${quote(to)}`)),
                ...parts.statements,
            ];
            const findings = [
                ...matched.flatMap(({ declared, added }) =>
                    Array.match(
                        added.filter((column) => !column.nullable && Option.isNone(column.fallback) && Option.isNone(column.projection)).map(({ name }) => name),
                        { onEmpty: () => [], onNonEmpty: (required) => [Finding.cases.Required.make({ table: declared.table.name, columns: required })] },
                    ),
                ),
                ...parts.findings,
            ];
            const planned = {
                preamble: parts.preamble,
                statements,
                ledger: Option.liftPredicate({ revision, document }, () => Array.isReadonlyArrayNonEmpty(statements) || !Equal.equals(document, identity)),
                concurrent: parts.concurrent,
                undeclared: { tables: kept.map(({ name }) => name), columns: matched.flatMap(({ declared, retained }) => retained.map(({ name }) => ({ table: declared.table.name, column: name }))), ...parts.undeclared },
            };
            return Array.isReadonlyArrayNonEmpty(findings) ? Effect.fail(ReconcileError.make({ findings })) : Effect.succeed(planned);
        },
    };
};

// --- [PLAN]
const latestRevision = (sql: SqlClient.SqlClient) =>
    Effect.flatMap(
        sql.onDialectOrElse({
            pg: () => sql`SELECT relname FROM pg_class WHERE relnamespace = current_schema()::regnamespace AND relname = ${ledger.table.name}`,
            orElse: () => sql`SELECT name FROM sqlite_schema WHERE name = ${ledger.table.name}`,
        }),
        (rows) => (Array.isReadonlyArrayNonEmpty(rows) ? SqlSchema.findOneOption({ Request: Schema.Void, Result: ledger, execute: () => sql`SELECT * FROM ${sql(ledger.table.name)}` })(undefined) : Effect.succeedNone),
    );
const planSqlite = Effect.fnUntraced(function* (sql: SqlClient.SqlClient, catalog: Catalog) {
    const stored = yield* latestRevision(sql);
    const live = yield* Sqlite.observe(sql);
    const resolved = match(catalog, draw('sqlite', catalog), { stored, tables: live.tables, columns: ({ columns }) => columns });
    const analyze = (entry: (typeof resolved.matched)[number]) => {
        const dropping = entry.doomed.map(({ name }) => name);
        const present = [...entry.fields.map(({ definition }) => definition.name), ...entry.retained.map(({ name }) => name)];
        const linked = entry.live.references.map((reference) => {
            const parent = resolved.referenced(reference.target);
            return { ...reference, target: parent.name, columns: Array.map(reference.columns, entry.renamed), to: reference.to.map(parent.renamed) };
        });
        const wanted = entry.rendered.indexes.map((index) => ({ name: index.name, text: index.text(''), prior: identifier([entry.live.name, ...index.parts.map(entry.original)]) }));
        const stale = (row: (typeof entry.live.indexes)[number]) => wanted.some((declared) => (declared.name === row.name && declared.text !== row.sql) || (declared.prior === row.name && declared.prior !== declared.name));
        const dropped = entry.live.indexes.filter((row) => entry.retiredIndexes.includes(row.name) || row.keys.some((key) => Array.contains(dropping, key)) || stale(row));
        return {
            ...entry,
            linked,
            unstated: linked.filter(({ columns }) => columns.every((column) => present.includes(column)) && !entry.rendered.links.some((link) => Equal.equals(link.columns, columns))),
            rebuild:
                entry.owned &&
                (!entry.live.strict ||
                    entry.live.withoutRowid !== entry.declared.table.withoutRowid ||
                    entry.rendered.links.some((link) => !linked.some(({ target, columns, onDelete }) => target === link.target && onDelete === link.action && Equal.equals(columns, link.columns))) ||
                    linked.some(({ columns }) => columns.some((column) => dropping.includes(column))) ||
                    entry.fields.some(({ definition, found }) => definition.retype && Option.isSome(found) && !entry.live.sql.includes(`${definition.text}, `))),
            dropped,
            indexes: wanted.map(({ text }) => text),
            missing: wanted.filter(({ name }) => !entry.live.indexes.some((row) => row.name === name && !dropped.includes(row))).map(({ text }) => text),
            undeclared: entry.live.indexes.filter((row) => !(dropped.includes(row) || wanted.some(({ name }) => name === row.name))),
        };
    };
    const tables = resolved.matched.map(analyze);
    const closure = (members: readonly (typeof tables)[number][]): readonly (typeof tables)[number][] => {
        const parents = members.map(({ declared }) => declared.table.name);
        const next = tables.filter((entry) => members.includes(entry) || entry.linked.some(({ target }) => parents.includes(target)));
        return next.length === members.length ? members : closure(next);
    };
    const rebuilds = closure(tables.filter(({ rebuild }) => rebuild));
    const temporary = (name: string) => quote(rebuilds.some(({ declared }) => declared.table.name === name) ? `${name}__rebuild` : name);
    const searched = (entry: (typeof tables)[number], recreated: boolean) =>
        Option.match(entry.rendered.fts, {
            onNone: () => ({ fresh: false, dropped: [], created: [] }),
            onSome: (fts) => {
                const fresh = live.virtual.find(({ name }) => name === fts.name)?.sql !== fts.sql;
                const stale = live.virtual.filter(({ name }) => (name === fts.name && fresh) || (name === `${entry.live.name}_fts` && name !== fts.name));
                const triggers = recreated ? [] : entry.live.triggers.filter(({ name }) => name.startsWith(`${entry.live.name}_fts_`) || name.startsWith(`${fts.name}_`));
                const current = triggers.filter((row) => fts.triggers.some(({ name, sql: text }) => name === row.name && text === row.sql));
                return {
                    fresh,
                    dropped: [...stale.map(({ name }) => `DROP TABLE ${quote(name)}`), ...Array.difference(triggers, current).map(({ name }) => `DROP TRIGGER ${quote(name)}`)],
                    created: [...(fresh ? [fts.sql] : []), ...fts.triggers.filter(({ name }) => !current.some((row) => row.name === name)).map((trigger) => trigger.sql), ...(fresh ? [fts.rebuild] : [])],
                };
            },
        });
    const rebuilt = (entry: (typeof tables)[number]) => {
        const kept = entry.retained.filter(({ hidden }) => hidden === 0);
        const copied = [...(entry.live.withoutRowid || entry.declared.table.withoutRowid ? [] : ['"rowid"']), ...[...entry.fields.filter(({ definition, found }) => Option.isNone(definition.projection) && Option.isSome(found)).map(({ definition }) => definition), ...kept].map(({ name }) => quote(name))].join(', ');
        const elements = [
            ...kept.map(({ name, type, required: needed, fallback }) => [quote(name), type === '' ? 'ANY' : type, ...(needed ? ['NOT NULL'] : []), ...Option.toArray(fallback).map((value) => `DEFAULT ${value}`)].join(' ')),
            ...entry.unstated.map(({ columns, target, to, onDelete, onUpdate }) => `FOREIGN KEY (${columns.map(quote).join(', ')}) REFERENCES ${temporary(target)}${Array.match(to, { onEmpty: () => '', onNonEmpty: (keys) => ` (${keys.map(quote).join(', ')})` })} ON UPDATE ${onUpdate} ON DELETE ${onDelete}`),
        ];
        return {
            ...entry,
            search: searched(entry, true),
            create: render('sqlite', entry.declared, temporary, { elements, autoincrement: entry.live.autoincrement }).create,
            copy: `INSERT INTO ${temporary(entry.declared.table.name)} (${copied}) SELECT ${copied} FROM ${entry.rendered.relation}`,
        };
    };
    const altered = tables.filter((entry) => !rebuilds.includes(entry)).map((entry) => ({ ...entry, search: searched(entry, false) }));
    const rebuilding = rebuilds.map(rebuilt);
    const creating = ({ rendered }: (typeof resolved.created)[number]) => [rendered.create, ...rendered.indexes.map(({ text }) => text('')), ...Option.match(rendered.fts, { onNone: () => [], onSome: (found) => [found.sql, ...found.triggers.map((trigger) => trigger.sql)] })];
    const removed = resolved.removed.map(({ name }) => name);
    const replaced = [...rebuilds.map(({ live: row }) => row.name), ...removed];
    return yield* resolved.plan({
        findings: [
            ...rebuilds.flatMap((entry) => refused(entry.declared.table.name, 'generated', entry.retained, ({ hidden }) => hidden !== 0)),
            ...[
                ...resolved.kept.flatMap((row) => row.references.filter(({ target }) => replaced.includes(target)).map((reference) => ({ ...reference, table: row.name }))),
                ...tables.flatMap(({ declared, unstated }) => unstated.filter(({ target }) => removed.includes(target)).map((reference) => ({ ...reference, table: declared.table.name }))),
            ].map(({ table: owner, columns, target }) => Finding.cases.RetainedReference.make({ table: owner, columns, target })),
        ],
        preamble: [],
        opening: Array.isReadonlyArrayNonEmpty(rebuilds) || Array.isReadonlyArrayNonEmpty(removed) ? ['PRAGMA defer_foreign_keys = on'] : [],
        statements: [
            ...live.virtual.filter(({ name }) => removed.some((owner) => name === `${owner}_fts`)).map(({ name }) => `DROP TABLE ${quote(name)}`),
            ...resolved.created.flatMap(creating),
            ...altered.flatMap((entry) => [
                ...entry.dropped.map(({ name }) => `DROP INDEX ${quote(name)}`),
                ...entry.search.dropped,
                ...entry.doomed.map(({ name }) => `ALTER TABLE ${entry.rendered.relation} DROP COLUMN ${quote(name)}`),
                ...entry.added.map(({ text }) => `ALTER TABLE ${entry.rendered.relation} ADD COLUMN ${text}`),
                ...entry.missing,
                ...entry.search.created,
            ]),
            ...rebuilding.map(({ create }) => create),
            ...(rebuilding.some(({ live: row }) => row.autoincrement) ? ["INSERT INTO sqlite_sequence (name, seq) SELECT m.name, s.seq FROM sqlite_sequence s JOIN sqlite_schema m ON m.name = s.name || '__rebuild'"] : []),
            ...rebuilding.map(({ copy }) => copy),
            ...rebuilding.map(({ rendered }) => `DROP TABLE ${rendered.relation}`),
            ...rebuilding.map(({ declared, rendered }) => `ALTER TABLE ${temporary(declared.table.name)} RENAME TO ${rendered.relation}`),
            ...rebuilding.flatMap((entry) => [
                ...entry.indexes,
                ...entry.undeclared.map((index) => index.sql),
                ...entry.live.triggers.filter(({ name }) => !(name.startsWith(`${entry.live.name}_fts_`) || name.startsWith(`${entry.declared.table.name}_fts_`))).map((trigger) => trigger.sql),
                ...entry.search.dropped,
                ...entry.search.created,
            ]),
            ...(resolved.created.some(({ rendered }) => Array.isReadonlyArrayNonEmpty(rendered.indexes) || Option.isSome(rendered.fts)) ||
            altered.some(({ missing, search }) => Array.isReadonlyArrayNonEmpty(missing) || search.fresh) ||
            rebuilding.some(({ indexes, search }) => Array.isReadonlyArrayNonEmpty(indexes) || search.fresh)
                ? ['PRAGMA optimize']
                : []),
        ],
        concurrent: [],
        undeclared: { indexes: tables.flatMap(({ undeclared }) => undeclared.map(({ name }) => name)), references: tables.flatMap(({ declared, unstated }) => unstated.map(({ columns, target }) => ({ table: declared.table.name, columns, target }))) },
    });
});
const planPg = Effect.fnUntraced(function* (sql: SqlClient.SqlClient, catalog: Catalog, runtime: Option.Option<string>) {
    const drawings = draw('pg', catalog);
    const indexes = drawings.flatMap(({ rendered }) => rendered.indexes);
    const required = Array.dedupe(indexes.flatMap(Struct.get('extensions')));
    const { migrator, extensions, configurations } = yield* PgPrelude.open(
        sql,
        required,
        Array.dedupeWith(
            indexes.flatMap(({ configuration }) => Option.toArray(configuration)),
            (self, that) => self.name === that.name,
        ),
    );
    const [existing, drifted] = [configurations.filter(Struct.get('configured')).map(Struct.get('name')), configurations.filter(({ drift }) => Array.isReadonlyArrayNonEmpty(drift)).map(Struct.get('name'))];
    const ready = ({ extensions: needed, configuration }: (typeof indexes)[number]) => needed.every((name) => extensions.includes(name)) && Option.toArray(configuration).every(({ name }) => existing.includes(name));
    const stored = yield* latestRevision(sql);
    const role = Option.match(runtime, { onNone: () => 'PUBLIC', onSome: quote });
    const block = (body: string) => `DO ${literal(`BEGIN ${body} END`)}`;
    yield* sql.unsafe(
        block(
            drawings
                .map(({ declared }) => render('pg', declared, (name) => `pg_temp.${quote(name)}`, { elements: [], autoincrement: false }))
                .flatMap((rendered) => [`${rendered.create} ON COMMIT DROP`, ...rendered.indexes.filter(ready).map(({ text }) => text('')), ...Option.toArray(rendered.policy).map(({ text }) => text(role))])
                .map((statement) => `${statement};`)
                .join(' '),
        ),
    );
    const live = yield* Pg.observe(sql);
    const resolved = match(catalog, drawings, { stored, tables: live.tables, columns: ({ columns }) => columns });
    const privileges = [...Option.toArray(runtime).map((grantee) => ({ grantee, all: ['SELECT', 'INSERT', 'UPDATE', 'DELETE'] })), ...Array.fromNullishOr(catalog.reader).map((grantee) => ({ grantee, all: ['SELECT'] }))].filter(({ grantee }) => grantee !== migrator);
    const grants = (declared: Relation, on: string, held: readonly (readonly [string, string])[]) =>
        (catalog.tables.includes(declared) ? privileges : []).flatMap(({ grantee, all }) =>
            Array.match(
                Array.difference(
                    all,
                    held.filter(([holder]) => holder === grantee).map(([, granted]) => granted),
                ),
                { onEmpty: () => [], onNonEmpty: (missing) => [`GRANT ${missing.join(', ')} ON ${on} TO ${quote(grantee)}`] },
            ),
        );
    const shape = (row: (typeof PgRelation.Type)['indexes'][number], rename: (column: string) => string) => ({ unique: row.unique, method: row.method, width: row.width, classes: row.classes, predicate: row.predicate, keys: row.keys.map(({ column, text }) => (column ? rename(text) : text)) });
    const analyze = (entry: (typeof resolved.matched)[number], desired: typeof PgRelation.Type) => {
        const { name } = entry.declared.table;
        const owner = entry.rendered.relation;
        const prior = (parts: readonly string[]) => identifier([entry.live.name, ...parts.map(entry.original)]);
        const named = [...entry.live.checks, ...entry.live.references].map(({ name: constraint }) => constraint);
        const relabels = [...entry.rendered.definitions.flatMap(({ check }) => Option.toArray(check)), ...entry.rendered.links].filter(({ name: target, parts }) => named.includes(prior(parts)) && prior(parts) !== target && !named.includes(target)).map(({ name: target, parts }) => ({ from: prior(parts), to: target }));
        const relabeled = (constraint: string) => relabels.find(({ from }) => from === constraint)?.to ?? constraint;
        const links = entry.live.references.map((row) => ({ ...row, name: relabeled(row.name), columns: Array.map(row.columns, entry.renamed), target: resolved.referenced(row.target).name }));
        const stale = entry.rendered.links.filter((link) => !links.some((row) => row.name === link.name && Equal.equals(row.columns, link.columns) && row.target === link.target && row.onDelete === link.action));
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
                ...Option.toArray(before).map((row) => `ALTER POLICY ${quote(row.name)} ON ${owner} RENAME TO ${quote(rule.name)}`),
                ...Option.match(
                    Option.orElse(
                        Array.findFirst(entry.live.policies, (row) => row.name === rule.name),
                        () => before,
                    ),
                    { onNone: () => [rule.text(role)], onSome: (present) => (Option.contains(wanted, { ...present, name: rule.name }) ? [] : [`ALTER POLICY ${quote(rule.name)} ON ${owner} TO ${role} USING (${rule.rule}) WITH CHECK (${rule.rule})`]) },
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
        const tightened = pairs.filter(({ wants, now }) => wants.required && !now.required).map(({ column }) => ({ column: quote(column.name), constraint: identifier([name, column.name, 'not', 'null']) }));
        const changes = pairs.flatMap(({ column, wants, now }) => [
            ...(now.type === wants.type ? [] : [`ALTER COLUMN ${quote(column.name)} TYPE ${wants.type} USING ${quote(column.name)}::${wants.type}`]),
            ...(wants.generated !== '' || now.expression === wants.expression ? [] : [Option.match(Option.fromNullOr(wants.expression), { onNone: () => `ALTER COLUMN ${quote(column.name)} DROP DEFAULT`, onSome: (expression) => `ALTER COLUMN ${quote(column.name)} SET DEFAULT ${expression}` })]),
            ...(now.required && !wants.required ? [`ALTER COLUMN ${quote(column.name)} DROP NOT NULL`] : []),
        ]);
        const retyping = [
            ...obsolete.map((row) => `ALTER TABLE ${owner} DROP CONSTRAINT ${quote(row.name)}`),
            ...dependents.map((row) => `ALTER TABLE ${owner} DROP COLUMN ${quote(row.name)}`),
            ...moved
                .filter(({ now }) => now.type !== 'text')
                .map(({ column, wants, now }) =>
                    block(
                        `IF EXISTS (SELECT 1 FROM ${owner} WHERE (${quote(column.name)}::${wants.type})::${now.type} IS DISTINCT FROM ${quote(column.name)}) THEN RAISE EXCEPTION 'ALTER COLUMN %.% TYPE % changes stored values', ${literal(name)}, ${literal(column.name)}, ${literal(wants.type)} USING ERRCODE = 'check_violation'; END IF;`,
                    ),
                ),
            ...Array.match(changes, { onEmpty: () => [], onNonEmpty: (all) => [`ALTER TABLE ${owner} ${all.join(', ')}`] }),
            ...tightened.map(({ column, constraint }) => `ALTER TABLE ${owner} ADD CONSTRAINT ${quote(constraint)} NOT NULL ${column} NOT VALID`),
            ...missing.map((row) => `ALTER TABLE ${owner} ADD CONSTRAINT ${quote(row.name)} ${row.definition} NOT VALID`),
            ...dependents.map((row) => `ALTER TABLE ${owner} ADD COLUMN ${quote(row.name)} ${row.type} GENERATED ALWAYS AS (${row.expression}) ${row.generated === 's' ? 'STORED' : 'VIRTUAL'}`),
            ...entry.live.indexes.filter((index) => dependents.some((row) => index.columns.includes(row.name))).map(({ definition }) => definition),
        ];
        const added = entry.added.flatMap((definition) => desired.columns.filter((row) => row.name === definition.name).map((row) => ({ definition, row })));
        const adding = added.map(({ definition }) => definition.name);
        const additions = desired.checks.filter((row) => row.columns.every((part) => adding.includes(part)));
        const tenant = Option.isSome(entry.declared.table.tenant);
        return {
            ...entry,
            relabels: [...relabels.map(({ from, to }) => `ALTER TABLE ${owner} RENAME CONSTRAINT ${quote(from)} TO ${quote(to)}`), ...plans.filter(({ rename }) => rename).map(({ index }) => `ALTER INDEX ${quote(prior(index.parts))} RENAME TO ${quote(index.name)}`)],
            statements: [
                ...entry.doomed.map((row) => `ALTER TABLE ${owner} DROP COLUMN ${quote(row.name)}`),
                ...added.map(
                    ({ definition, row }) =>
                        `ALTER TABLE ${owner} ADD COLUMN ${quote(definition.name)} ${row.type}${row.generated === 'v' ? ` GENERATED ALWAYS AS (${row.expression}) VIRTUAL` : ''}${definition.nullable ? '' : ' NOT NULL'}${row.generated === '' && row.expression !== null ? ` DEFAULT ${row.expression}` : ''}`,
                ),
                ...additions.map((row) => `ALTER TABLE ${owner} ADD CONSTRAINT ${quote(row.name)} ${row.definition} NOT VALID`),
                ...retyping,
                ...stale.flatMap((link) => [...(links.some((row) => row.name === link.name) ? [`ALTER TABLE ${owner} DROP CONSTRAINT ${quote(link.name)}`] : []), `ALTER TABLE ${owner} ADD CONSTRAINT ${quote(link.name)} ${link.text} NOT VALID`]),
                ...grants(entry.declared, owner, entry.live.grants),
                ...(tenant && !entry.live.rls ? [`ALTER TABLE ${owner} ENABLE ROW LEVEL SECURITY`] : []),
                ...(tenant && !entry.live.force ? [`ALTER TABLE ${owner} FORCE ROW LEVEL SECURITY`] : []),
                ...Option.match(entry.rendered.policy, { onNone: () => [], onSome: policy }),
            ],
            dropped: [...plans.flatMap(({ dropped }) => dropped), ...entry.live.indexes.filter((row) => entry.retiredIndexes.includes(row.name)).map((row) => row.name)],
            built: plans.filter(({ keep, rename }) => !(keep || rename)).map(({ index }) => index.text('CONCURRENTLY IF NOT EXISTS ')),
            reindexed: plans.filter(({ index, keep, rename }) => (keep || rename) && Option.exists(index.configuration, (configuration) => drifted.includes(configuration.name))).map(({ index }) => `REINDEX INDEX CONCURRENTLY ${quote(index.name)}`),
            validated: [...entry.live.invalid.map(relabeled), ...additions.map((row) => row.name), ...tightened.map(({ constraint }) => constraint), ...missing.map((row) => row.name), ...stale.map((link) => link.name)],
            undeclared: {
                indexes: entry.live.indexes.filter((row) => !(entry.retiredIndexes.includes(row.name) || plans.some(({ index }) => [index.name, prior(index.parts)].includes(row.name)))).map((row) => row.name),
                references: links.filter((row) => !entry.rendered.links.some((link) => link.name === row.name)).map(({ columns, target }) => ({ table: name, columns, target })),
            },
        };
    };
    const tables = resolved.matched.flatMap((entry) => Option.toArray(Array.findFirst(live.scratch, ({ name }) => name === entry.declared.table.name)).map((desired) => analyze(entry, desired)));
    const creating = ({ declared, rendered }: (typeof resolved.created)[number]) => [
        rendered.create,
        ...rendered.indexes.map(({ text }) => text('')),
        ...grants(declared, rendered.relation, []),
        ...Option.toArray(declared.table.tenant).flatMap(() => [`ALTER TABLE ${rendered.relation} ENABLE ROW LEVEL SECURITY`, `ALTER TABLE ${rendered.relation} FORCE ROW LEVEL SECURITY`]),
        ...Option.toArray(rendered.policy).map(({ text }) => text(role)),
    ];
    return yield* resolved.plan({
        findings: [],
        preamble: PgPrelude.preamble,
        opening: [
            ...Array.difference(required, extensions).map((extension) => `CREATE EXTENSION IF NOT EXISTS ${quote(extension)}`),
            ...configurations.flatMap(({ name, configuration, configured, drift }) => [
                ...(configured ? [] : [`CREATE TEXT SEARCH CONFIGURATION ${quote(name)} (COPY = ${configuration})`]),
                ...drift.map(({ tokens, dictionaries }) => `ALTER TEXT SEARCH CONFIGURATION ${quote(name)} ${Option.match(dictionaries, { onNone: () => `DROP MAPPING IF EXISTS FOR ${tokens.join(', ')}`, onSome: (chain) => `ALTER MAPPING FOR ${tokens.join(', ')} WITH ${chain.join(', ')}` })}`),
            ]),
        ],
        statements: [...tables.flatMap(({ relabels }) => relabels), ...resolved.created.flatMap(creating), ...tables.flatMap((entry) => entry.statements)],
        concurrent: [
            ...tables.flatMap(({ dropped }) => dropped.map((index) => `DROP INDEX CONCURRENTLY IF EXISTS ${quote(index)}`)),
            ...tables.flatMap(({ built }) => built),
            ...tables.flatMap(({ reindexed }) => reindexed),
            ...tables.flatMap(({ rendered, validated }) => validated.map((constraint) => `ALTER TABLE ${rendered.relation} VALIDATE CONSTRAINT ${quote(constraint)}`)),
        ],
        undeclared: { indexes: tables.flatMap(({ undeclared }) => undeclared.indexes), references: tables.flatMap(({ undeclared }) => undeclared.references) },
    });
});

// --- [APPLY]
const apply = (sql: SqlClient.SqlClient, { statements, ledger: written }: Effect.Success<ReturnType<typeof planSqlite>>) =>
    Effect.transposeOption(
        Option.map(written, ({ revision, document }) =>
            Effect.flatMap(Schema.encodeEffect(ledger)({ revision: revision + 1, identity: document }), (row) =>
                atomic(
                    Array.prependAll(
                        [sql`INSERT INTO ${sql(ledger.table.name)} ${sql.insert(row)}`, sql`DELETE FROM ${sql(ledger.table.name)} WHERE ${sql('revision')} = ${revision}`],
                        statements.map((text) => sql.unsafe(text)),
                    ),
                ),
            ),
        ),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const converge = Effect.fnUntraced(function* <E, R>(catalog: Catalog, runtime: Option.Option<string>, after: (sql: SqlClient.SqlClient, planned: Effect.Success<ReturnType<typeof planSqlite>>) => Effect.Effect<unknown, E, R>) {
    const [sql, d1] = yield* Effect.all([
        Effect.filterOrFail(
            SqlClient.SqlClient,
            (client) => client.onDialectOrElse({ sqlite: () => true, pg: () => true, orElse: () => false }),
            () => new Cause.IllegalArgumentError('reconcile runs on SQLite and PostgreSQL clients'),
        ),
        Effect.serviceOption(D1Client.D1Client),
    ]);
    yield* refuse(sql, catalog);
    const steps = sql.onDialectOrElse({ pg: () => planPg(sql, catalog, runtime), orElse: () => planSqlite(sql, catalog) });
    const applied = Effect.tap(steps, (planned) => after(sql, planned));
    return yield* Option.match(d1, {
        onNone: () => sql.withTransaction(applied),
        onSome: () =>
            Effect.catchTag(applied, 'SqlError', (error) =>
                Effect.filterOrFail(
                    steps,
                    ({ ledger: written }) => Option.isNone(written),
                    () => error,
                ),
            ),
    });
});
const plan = Effect.fn('plan')((catalog: Catalog, runtime: Option.Option<string>) => converge(catalog, runtime, () => Effect.void), Effect.map(Struct.pick(['preamble', 'statements', 'concurrent', 'undeclared'])));
const reconcile = Effect.fn('reconcile')(
    (catalog: Catalog, runtime: Option.Option<string>) => converge(catalog, runtime, apply),
    Effect.tap(({ concurrent }) => SqlClient.SqlClient.use((sql) => Effect.forEach(concurrent, (text) => sql.unsafe(text), { discard: true }))),
    Effect.map(Struct.get('undeclared')),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { plan, ReconcileError, reconcile };
