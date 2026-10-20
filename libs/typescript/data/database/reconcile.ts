import { D1Client } from '@effect/sql-d1';
import { Array, Effect, Equal, Function, Option, Order, Record, Result, Schema, Struct } from 'effect';
import { SqlClient, SqlSchema } from 'effect/sql';
import { identifier, render } from './ddl.ts';
import { atomic, type Catalog, literal, names, quote, type Relation, table } from './table.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Entry = Schema.Struct({ origin: Schema.optionalKey(Schema.String), incarnation: Schema.optionalKey(Schema.Int) });
const ledger = table('reconcile_revision', { revision: Schema.Int, identity: Schema.fromJsonString(Schema.Record(Schema.String, Schema.Struct({ ...Entry.fields, columns: Schema.optionalKey(Schema.Record(Schema.String, Entry)) }))) }, { key: ['revision'] });
const Finding = Schema.TaggedUnion({ Unsupported: { table: Schema.String, subject: Schema.String }, Required: { table: Schema.String, subject: Schema.String }, RetainedReference: { table: Schema.String, subject: Schema.String } });
class Sqlite extends Schema.Class<Sqlite, { readonly brand: unique symbol }>('Sqlite')({
    tables: Schema.Array(
        Schema.Struct({
            name: Schema.String,
            strict: Schema.BooleanFromBit,
            withoutRowid: Schema.BooleanFromBit,
            sql: Schema.String,
            columns: Schema.Array(Schema.Struct({ name: Schema.String, type: Schema.String, required: Schema.BooleanFromBit, fallback: Schema.NullOr(Schema.String), hidden: Schema.Int })),
            references: Schema.Array(Schema.Struct({ id: Schema.Int, target: Schema.String, from: Schema.String, to: Schema.NullOr(Schema.String), onDelete: Schema.String })),
        }),
    ),
    virtual: Schema.Array(Schema.Struct({ name: Schema.String, sql: Schema.String })),
    indexes: Schema.Array(Schema.Struct({ name: Schema.String, table: Schema.String, sql: Schema.String, keys: Schema.Array(Schema.NullOr(Schema.String)) })),
    triggers: Schema.Array(Schema.Struct({ name: Schema.String, table: Schema.String, sql: Schema.String })),
}) {
    static readonly observe = (sql: SqlClient.SqlClient) =>
        sql`WITH relations AS (SELECT l.name, l.type, l.strict, l.wr, m.sql FROM pragma_table_list l JOIN sqlite_schema m ON m.name = l.name WHERE l.schema = 'main' AND l.name NOT GLOB 'sqlite_*' AND l.name NOT GLOB '_cf_*')
            SELECT json_object(
                'tables', (SELECT json_group_array(json_object('name', r.name, 'strict', r.strict, 'withoutRowid', r.wr, 'sql', r.sql,
                    'columns', (SELECT json_group_array(json_object('name', x.name, 'type', x.type, 'required', x."notnull", 'fallback', x.dflt_value, 'hidden', x.hidden)) FROM pragma_table_xinfo(r.name) x),
                    'references', (SELECT json_group_array(json_object('id', f.id, 'target', f."table", 'from', f."from", 'to', f."to", 'onDelete', f.on_delete)) FROM pragma_foreign_key_list(r.name) f))) FROM relations r WHERE r.type = 'table'),
                'virtual', (SELECT json_group_array(json_object('name', r.name, 'sql', r.sql)) FROM relations r WHERE r.type = 'virtual'),
                'indexes', (SELECT json_group_array(json_object('name', m.name, 'table', m.tbl_name, 'sql', m.sql, 'keys', (SELECT json_group_array(k.name) FROM pragma_index_xinfo(m.name) k WHERE k."key" = 1))) FROM sqlite_schema m JOIN relations r ON r.name = m.tbl_name WHERE m.type = 'index' AND m.sql IS NOT NULL),
                'triggers', (SELECT json_group_array(json_object('name', m.name, 'table', m.tbl_name, 'sql', m.sql)) FROM sqlite_schema m WHERE m.type = 'trigger')
            ) AS catalog`.pipe(
            Effect.flatMap(Schema.decodeUnknownEffect(Schema.Tuple([Schema.Struct({ catalog: Schema.fromJsonString(Sqlite) })]))),
            Effect.map(([{ catalog }]) => catalog),
        );
}
class Pg extends Schema.Class<Pg, { readonly brand: unique symbol }>('Pg')({
    tables: Schema.Array(Schema.Struct({ scratch: Schema.Boolean, name: Schema.String, rls: Schema.Boolean, force: Schema.Boolean, grants: Schema.Array(Schema.Tuple([Schema.String, Schema.String])) })),
    columns: Schema.Array(Schema.Struct({ scratch: Schema.Boolean, table: Schema.String, name: Schema.String, type: Schema.String, expression: Schema.NullOr(Schema.String), generated: Schema.Literals(['', 's', 'v']), required: Schema.Boolean, sources: Schema.Array(Schema.String) })),
    constraints: Schema.Array(Schema.Struct({ scratch: Schema.Boolean, table: Schema.String, name: Schema.String, kind: Schema.String, definition: Schema.String, valid: Schema.Boolean, columns: Schema.Array(Schema.String), target: Schema.NullOr(Schema.String), onDelete: Schema.String })),
    indexes: Schema.Array(
        Schema.Struct({
            scratch: Schema.Boolean,
            table: Schema.String,
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
    policies: Schema.Array(Schema.Struct({ scratch: Schema.Boolean, table: Schema.String, name: Schema.String, using: Schema.NullOr(Schema.String), check: Schema.NullOr(Schema.String) })),
}) {
    static readonly observe = (sql: SqlClient.SqlClient) =>
        sql`WITH relations AS (SELECT c.oid, c.relname, c.relowner, c.relacl, c.relrowsecurity, c.relforcerowsecurity, c.relnamespace = pg_my_temp_schema() AS scratch FROM pg_class c WHERE c.relnamespace IN (current_schema()::regnamespace, pg_my_temp_schema()) AND c.relkind = 'r')
            SELECT json_build_object(
                'tables', array(SELECT json_build_object('scratch', r.scratch, 'name', r.relname, 'rls', r.relrowsecurity, 'force', r.relforcerowsecurity,
                    'grants', array(SELECT json_build_array(a.grantee::regrole::text, a.privilege_type) FROM aclexplode(r.relacl) a WHERE a.grantee <> r.relowner)) FROM relations r),
                'columns', array(SELECT json_build_object('scratch', r.scratch, 'table', r.relname, 'name', a.attname, 'type', format_type(a.atttypid, a.atttypmod), 'expression', pg_get_expr(d.adbin, d.adrelid), 'generated', a.attgenerated::text,
                    'required', EXISTS (SELECT 1 FROM pg_constraint n WHERE n.conrelid = a.attrelid AND n.contype = 'n' AND n.conkey = ARRAY[a.attnum] AND n.convalidated),
                    'sources', array(SELECT s.attname::text FROM pg_depend p JOIN pg_attribute s ON (s.attrelid, s.attnum) = (p.refobjid, p.refobjsubid) WHERE p.classid = 'pg_attrdef'::regclass AND p.objid = d.oid AND p.refobjsubid > 0 AND s.attnum <> a.attnum))
                    FROM pg_attribute a JOIN relations r ON r.oid = a.attrelid LEFT JOIN pg_attrdef d ON (d.adrelid, d.adnum) = (a.attrelid, a.attnum) WHERE a.attnum > 0 AND NOT a.attisdropped ORDER BY a.attnum),
                'constraints', array(SELECT json_build_object('scratch', r.scratch, 'table', r.relname, 'name', n.conname, 'kind', n.contype::text, 'definition', pg_get_constraintdef(n.oid, true), 'valid', n.convalidated,
                    'columns', array(SELECT a.attname::text FROM unnest(n.conkey) WITH ORDINALITY k(number, position) JOIN pg_attribute a ON (a.attrelid, a.attnum) = (n.conrelid, k.number) ORDER BY k.position),
                    'target', (SELECT t.relname FROM pg_class t WHERE t.oid = n.confrelid), 'onDelete', n.confdeltype::text)
                    FROM pg_constraint n JOIN relations r ON r.oid = n.conrelid WHERE n.contype IN ('c', 'f', 'n')),
                'indexes', array(SELECT json_build_object('scratch', r.scratch, 'table', r.relname, 'name', i.relname, 'valid', x.indisvalid, 'unique', x.indisunique, 'method', am.amname, 'width', x.indnkeyatts,
                    'keys', array(SELECT json_build_object('column', x.indkey[k - 1] <> 0, 'text', coalesce((SELECT a.attname::text FROM pg_attribute a WHERE a.attrelid = x.indrelid AND a.attnum = x.indkey[k - 1]), pg_get_indexdef(x.indexrelid, k, true))) FROM generate_series(1, x.indnatts) k ORDER BY k),
                    'classes', array(SELECT o.opcname::text FROM unnest(x.indclass) WITH ORDINALITY u(oid, position) JOIN pg_opclass o ON o.oid = u.oid ORDER BY u.position),
                    'predicate', pg_get_expr(x.indpred, x.indrelid, true), 'definition', pg_get_indexdef(x.indexrelid),
                    'columns', array(SELECT a.attname::text FROM pg_depend p JOIN pg_attribute a ON (a.attrelid, a.attnum) = (p.refobjid, p.refobjsubid) WHERE p.objid = x.indexrelid AND p.classid = 'pg_class'::regclass AND p.refobjsubid > 0))
                    FROM pg_index x JOIN pg_class i ON i.oid = x.indexrelid JOIN relations r ON r.oid = x.indrelid JOIN pg_am am ON am.oid = i.relam WHERE NOT EXISTS (SELECT 1 FROM pg_constraint k WHERE k.conindid = x.indexrelid AND k.conrelid = x.indrelid)),
                'policies', array(SELECT json_build_object('scratch', r.scratch, 'table', r.relname, 'name', p.polname, 'using', pg_get_expr(p.polqual, p.polrelid, true), 'check', pg_get_expr(p.polwithcheck, p.polrelid, true)) FROM pg_policy p JOIN relations r ON r.oid = p.polrelid)
            )::text AS catalog`.pipe(
            Effect.flatMap(Schema.decodeUnknownEffect(Schema.Tuple([Schema.Struct({ catalog: Schema.fromJsonString(Pg) })]))),
            Effect.map(([{ catalog }]) => catalog),
        );
}
class PgPrelude extends Schema.Class<PgPrelude, { readonly brand: unique symbol }>('PgPrelude')({ migrator: Schema.String, extensions: Schema.Array(Schema.String) }) {
    static readonly preamble = ["SET LOCAL lock_timeout = '5s'", "SET LOCAL transaction_timeout = '2min'"];
    static readonly open = (sql: SqlClient.SqlClient) =>
        Effect.forEach(PgPrelude.preamble, (text) => sql.unsafe(text), { discard: true }).pipe(
            Effect.andThen(sql`SELECT pg_advisory_xact_lock(hashtextextended(current_schema(), 0))::text AS lock, set_config('search_path', quote_ident(current_schema()) || ', pg_temp', true) AS search_path, current_user::text AS migrator, array(SELECT extname::text FROM pg_extension) AS extensions`),
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
                    Unsupported: ({ table: owner, subject }) => `${owner} declares ${subject}, which this store cannot hold`,
                    Required: ({ table: owner, subject }) => `${owner} adds required ${subject} with no default for its rows`,
                    RetainedReference: ({ table: owner, subject }) => `${owner} references ${subject}, which this plan replaces`,
                }),
            )
            .join('; ');
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [RENDER]
const draw = (dialect: Parameters<typeof render>[0], catalog: Catalog) => [ledger, ...catalog.tables].map((declared) => ({ declared, rendered: render(dialect, declared, quote, []) }));

// --- [IDENTITY]
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
            added: fields.filter(({ found }) => Option.isNone(found)).map(({ definition }) => definition),
            recorded: [drawing.declared.table.name, { ...entry, ...(Record.isEmptyRecord(identities) ? {} : { columns: identities }) }] as const,
        };
    });
    const [matched, created] = Array.partition(entries, (entry) => Option.match(entry.row, { onNone: () => Result.fail(entry), onSome: (live) => Result.succeed({ ...entry, live, retiredIndexes: entry.declared.table.retired.indexes.map((parts) => identifier([live.name, ...parts])) }) }));
    const kept = Array.difference(tables, [...located.retiring, ...located.claimed]);
    const document = Record.filter(Record.fromEntries([...entries.map(({ recorded }) => recorded), ...kept.map(({ name }) => [name, identity[name] ?? {}] as const)]), (value) => Object.keys(value).length > 0);
    return {
        created,
        matched,
        declaredName: (live: string) => matched.find((entry) => entry.live.name === live)?.declared.table.name ?? live,
        removed: located.retiring,
        kept,
        plan: (parts: Record<'preamble' | 'opening' | 'statements' | 'concurrent' | 'indexes', readonly string[]> & { readonly findings: readonly (typeof Finding.Type)[] }) => {
            const statements = [
                ...parts.opening,
                ...located.retiring.map(({ name }) => `DROP TABLE ${quote(name)}`),
                ...matched.filter(({ live, declared }) => live.name !== declared.table.name).map(({ live, rendered }) => `ALTER TABLE ${quote(live.name)} RENAME TO ${rendered.relation}`),
                ...matched.flatMap(({ rendered, renames }) => renames.map(({ from, to }) => `ALTER TABLE ${rendered.relation} RENAME COLUMN ${quote(from)} TO ${quote(to)}`)),
                ...parts.statements,
            ];
            const findings = [
                ...catalog.tables.flatMap(({ table: facts }) =>
                    Object.values(facts.columns)
                        .filter(({ kind }) => kind === 'number')
                        .map(({ name }) => Finding.cases.Unsupported.make({ table: facts.name, subject: name })),
                ),
                ...matched.flatMap(({ declared, added }) => added.filter((column) => !column.nullable && Option.isNone(column.fallback) && Option.isNone(column.projection)).map(({ name }) => Finding.cases.Required.make({ table: declared.table.name, subject: name }))),
                ...parts.findings,
            ];
            const planned = {
                preamble: parts.preamble,
                statements,
                ledger: Option.liftPredicate({ revision, document }, () => Array.isReadonlyArrayNonEmpty(statements) || !Equal.equals(document, identity)),
                concurrent: parts.concurrent,
                undeclared: { tables: kept.map(({ name }) => name), columns: matched.flatMap(({ declared, retained }) => retained.map(({ name }) => ({ table: declared.table.name, column: name }))), indexes: parts.indexes },
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
        const groups = Object.values(Array.groupBy(entry.live.references, ({ id }) => `${id}`));
        const linked = groups.map((group) => ({ target: resolved.declaredName(group[0].target), columns: group.map(({ from }) => entry.renamed(from)), action: group[0].onDelete }));
        const indexes = live.indexes.filter((row) => row.table === entry.live.name);
        const wanted = entry.rendered.indexes.map((index) => ({ name: index.name, text: index.text(''), prior: identifier([entry.live.name, ...index.parts.map(entry.original)]) }));
        const stale = (row: (typeof indexes)[number]) => wanted.some((declared) => (declared.name === row.name && declared.text !== row.sql) || (declared.prior === row.name && declared.prior !== declared.name));
        const dropped = indexes.filter((row) => entry.retiredIndexes.includes(row.name) || row.keys.some((key) => Array.contains(dropping, key)) || stale(row));
        const drifted = ({ definition, found }: (typeof entry.fields)[number]) =>
            definition.retype &&
            Option.exists(found, (row) => row.type !== definition.type || row.required === definition.nullable || row.fallback !== Option.getOrNull(definition.fallback) || (row.hidden === 0) === Option.isSome(definition.projection) || Option.exists(definition.check, ({ text }) => !entry.live.sql.includes(text)));
        const unlinked = entry.rendered.links.some((link) => !linked.some(({ target, columns, action }) => target === link.target && action === link.action && Equal.equals(columns, link.columns)));
        return {
            ...entry,
            groups,
            rebuild: entry.owned && (!entry.live.strict || entry.live.withoutRowid !== entry.declared.table.withoutRowid || unlinked || entry.live.references.some(({ from }) => dropping.includes(from)) || entry.fields.some(drifted)),
            dropped,
            indexes: wanted.map(({ text }) => text),
            missing: wanted.filter(({ name }) => !indexes.some((row) => row.name === name && !dropped.includes(row))).map(({ text }) => text),
            undeclared: indexes.filter((row) => !(dropped.includes(row) || wanted.some(({ name }) => name === row.name))),
            triggers: live.triggers.filter((row) => row.table === entry.live.name),
        };
    };
    const tables = resolved.matched.map(analyze);
    const closure = (members: readonly (typeof tables)[number][]): readonly (typeof tables)[number][] => {
        const parents = members.map(({ declared }) => declared.table.name);
        const next = tables.filter((entry) => members.includes(entry) || entry.declared.table.references.some((reference) => parents.includes(reference.table.table.name)));
        return next.length === members.length ? members : closure(next);
    };
    const rebuilds = closure(tables.filter(({ rebuild }) => rebuild));
    const temporary = (name: string) => (rebuilds.some(({ declared }) => declared.table.name === name) ? quote(`${name}__rebuild`) : quote(name));
    const depth = (declared: Relation): number => Math.max(0, ...declared.table.references.map((reference) => depth(reference.table) + 1));
    const searched = (entry: (typeof tables)[number], recreated: boolean) =>
        Option.match(entry.rendered.fts, {
            onNone: () => [],
            onSome: (fts) => {
                const fresh = live.virtual.find(({ name }) => name === fts.name)?.sql !== fts.sql;
                const stale = live.virtual.filter(({ name }) => (name === fts.name && fresh) || (name === `${entry.live.name}_fts` && name !== fts.name));
                const triggers = recreated ? [] : entry.triggers.filter(({ name }) => name.startsWith(`${entry.live.name}_fts_`) || name.startsWith(`${fts.name}_`));
                const current = triggers.filter((row) => fts.triggers.some(({ name, sql: text }) => name === row.name && text === row.sql));
                return [
                    ...stale.map(({ name }) => `DROP TABLE ${quote(name)}`),
                    ...triggers.filter((row) => !current.includes(row)).map(({ name }) => `DROP TRIGGER ${quote(name)}`),
                    ...(fresh ? [fts.sql] : []),
                    ...fts.triggers.filter(({ name }) => !current.some((row) => row.name === name)).map((trigger) => trigger.sql),
                    ...(fresh ? [fts.rebuild] : []),
                ];
            },
        });
    const retain = ({ name, type, required: needed, fallback }: (typeof tables)[number]['retained'][number]) => [quote(name), type === '' ? 'ANY' : type, ...(needed ? ['NOT NULL'] : []), ...(fallback === null ? [] : [`DEFAULT ${fallback}`])].join(' ');
    const relink = (group: (typeof tables)[number]['groups'][number]) => `FOREIGN KEY (${group.map(({ from }) => quote(from)).join(', ')}) REFERENCES ${quote(group[0].target)} (${group.map(({ from, to }) => quote(to ?? from)).join(', ')}) ON DELETE ${group[0].onDelete}`;
    const rebuilt = (entry: (typeof tables)[number]) => {
        const kept = entry.retained.filter(({ hidden }) => hidden === 0).map(({ name }) => name);
        const copied = [...(entry.live.withoutRowid || entry.declared.table.withoutRowid ? [] : ['"rowid"']), ...entry.fields.filter(({ definition, found }) => Option.isNone(definition.projection) && Option.isSome(found)).map(({ definition }) => quote(definition.name)), ...kept.map(quote)].join(', ');
        return {
            create: render('sqlite', entry.declared, temporary, [...entry.retained.filter(({ name }) => kept.includes(name)).map(retain), ...entry.groups.filter((group) => group.every(({ from }) => kept.includes(from))).map(relink)]).create,
            copy: `INSERT INTO ${temporary(entry.declared.table.name)} (${copied}) SELECT ${copied} FROM ${entry.rendered.relation}`,
            restore: [...entry.indexes, ...entry.undeclared.map((index) => index.sql), ...entry.triggers.filter(({ name }) => !(name.startsWith(`${entry.live.name}_fts_`) || name.startsWith(`${entry.declared.table.name}_fts_`))).map((trigger) => trigger.sql), ...searched(entry, true)],
        };
    };
    const altered = tables.filter((entry) => !rebuilds.includes(entry));
    const altering = (entry: (typeof tables)[number]) => [
        ...entry.dropped.map(({ name }) => `DROP INDEX ${quote(name)}`),
        ...entry.doomed.map(({ name }) => `ALTER TABLE ${entry.rendered.relation} DROP COLUMN ${quote(name)}`),
        ...entry.added.map(({ text }) => `ALTER TABLE ${entry.rendered.relation} ADD COLUMN ${text}`),
        ...entry.missing,
        ...searched(entry, false),
    ];
    const creating = ({ rendered }: (typeof resolved.created)[number]) => [rendered.create, ...rendered.indexes.map(({ text }) => text('')), ...Option.match(rendered.fts, { onNone: () => [], onSome: (found) => [found.sql, ...found.triggers.map((trigger) => trigger.sql)] })];
    const rebuilding = rebuilds.map(rebuilt);
    const indexing = [...resolved.created.flatMap(({ rendered }) => [...rendered.indexes, ...Option.toArray(rendered.fts)]), ...altered.flatMap(({ missing }) => missing), ...rebuilds.flatMap(({ indexes }) => indexes)];
    const replaced = [...rebuilds.map(({ live: row }) => row.name), ...resolved.removed.map(({ name }) => name)];
    return yield* resolved.plan({
        findings: [
            ...catalog.tables.flatMap(({ table: facts }) =>
                [
                    ...Option.toArray(facts.tenant),
                    ...(Option.isSome(facts.search) && (facts.withoutRowid || !Object.values(facts.columns).some(({ name, kind }) => kind === 'integer' && Equal.equals(facts.key, [name]))) ? ['search'] : []),
                    ...Object.values(facts.columns).flatMap(({ name }) => (['rowid', 'oid', '_rowid_'].includes(name.toLowerCase()) ? [name] : [])),
                ].map((subject) => Finding.cases.Unsupported.make({ table: facts.name, subject })),
            ),
            ...rebuilds.flatMap((entry) => entry.retained.filter(({ hidden }) => hidden !== 0).map(({ name }) => Finding.cases.Unsupported.make({ table: entry.declared.table.name, subject: name }))),
            ...resolved.kept.flatMap((row) => row.references.filter(({ target }) => replaced.includes(target)).map(({ target }) => Finding.cases.RetainedReference.make({ table: row.name, subject: target }))),
        ],
        preamble: [],
        opening: Array.isReadonlyArrayNonEmpty(rebuilds) || Array.isReadonlyArrayNonEmpty(resolved.removed) ? ['PRAGMA defer_foreign_keys = on'] : [],
        statements: [
            ...resolved.created.flatMap(creating),
            ...altered.flatMap(altering),
            ...rebuilding.map(({ create }) => create),
            ...rebuilding.map(({ copy }) => copy),
            ...Array.sortWith(rebuilds, ({ declared }) => depth(declared), Order.flip(Order.Number)).map(({ rendered }) => `DROP TABLE ${rendered.relation}`),
            ...rebuilds.map(({ declared, rendered }) => `ALTER TABLE ${temporary(declared.table.name)} RENAME TO ${rendered.relation}`),
            ...rebuilding.flatMap(({ restore }) => restore),
            ...(Array.isReadonlyArrayNonEmpty(indexing) ? ['PRAGMA optimize'] : []),
        ],
        concurrent: [],
        indexes: tables.flatMap(({ undeclared }) => undeclared.map(({ name }) => name)),
    });
});
const planPg = Effect.fnUntraced(function* (sql: SqlClient.SqlClient, catalog: Catalog, runtime: Option.Option<string>) {
    const { migrator, extensions } = yield* PgPrelude.open(sql);
    const stored = yield* latestRevision(sql);
    const drawings = draw('pg', catalog);
    const role = Option.match(runtime, { onNone: () => 'PUBLIC', onSome: quote });
    const block = (body: string) => {
        const tag = `$${'r'.repeat(1 + Math.max(0, ...Array.fromIterable(body.matchAll(/r+/gu)).map(([run]) => run.length)))}$`;
        return `DO ${tag} BEGIN ${body} END ${tag}`;
    };
    yield* sql.unsafe(
        block(
            drawings
                .map(({ declared }) => render('pg', declared, (name) => `pg_temp.${quote(name)}`, []))
                .flatMap((rendered) => [`${rendered.create} ON COMMIT DROP`, ...rendered.indexes.filter(({ trigram }) => !trigram || extensions.includes('pg_trgm')).map(({ text }) => text('')), ...Option.toArray(rendered.policy).map(({ text }) => text(role))])
                .map((statement) => `${statement};`)
                .join(' '),
        ),
    );
    const live = yield* Pg.observe(sql);
    const own = <R extends { readonly scratch: boolean; readonly table: string }>(rows: readonly R[], name: string, scratched: boolean) => rows.filter((row) => row.scratch === scratched && row.table === name);
    const resolved = match(catalog, drawings, { stored, tables: live.tables.filter((row) => !row.scratch), columns: ({ name }) => own(live.columns, name, false) });
    const privileges = [...Option.toArray(runtime).map((grantee) => ({ grantee, all: ['SELECT', 'INSERT', 'UPDATE', 'DELETE'] })), ...Option.toArray(Option.fromUndefinedOr(catalog.reader)).map((grantee) => ({ grantee, all: ['SELECT'] }))].filter(({ grantee }) => grantee !== migrator);
    const grants = (on: string, held: readonly (readonly [string, string])[]) =>
        privileges.flatMap(({ grantee, all }) => {
            const missing = Array.difference(
                all,
                held.filter(([holder]) => holder === grantee).map(([, granted]) => granted),
            );
            return Array.isReadonlyArrayNonEmpty(missing) ? [`GRANT ${missing.join(', ')} ON ${on} TO ${quote(grantee)}`] : [];
        });
    const shape = (row: (typeof live.indexes)[number], rename: (column: string) => string) => ({ unique: row.unique, method: row.method, width: row.width, classes: row.classes, predicate: row.predicate, keys: row.keys.map(({ column, text }) => (column ? rename(text) : text)) });
    const analyze = (entry: (typeof resolved.matched)[number]) => {
        const { name } = entry.declared.table;
        const owner = entry.rendered.relation;
        const prior = (parts: readonly string[]) => identifier([entry.live.name, ...parts.map(entry.original)]);
        const desired = { columns: own(live.columns, name, true), constraints: own(live.constraints, name, true), indexes: own(live.indexes, name, true), policies: own(live.policies, name, true) };
        const current = { columns: own(live.columns, entry.live.name, false), constraints: own(live.constraints, entry.live.name, false), indexes: own(live.indexes, entry.live.name, false), policies: own(live.policies, entry.live.name, false) };
        const relabel = (target: string, parts: readonly string[]) =>
            Option.filter(
                Array.findFirst(current.constraints, (row) => row.name === prior(parts)),
                (row) => row.name !== target && !current.constraints.some((other) => other.name === target),
            );
        const relabels = [...entry.rendered.definitions.flatMap(({ check }) => Option.toArray(check)), ...entry.rendered.links].flatMap(({ name: target, parts }) => Option.toArray(relabel(target, parts)).map((row) => ({ from: row.name, to: target })));
        const constraints = current.constraints.map((row) => ({ ...row, name: relabels.find(({ from }) => from === row.name)?.to ?? row.name, columns: row.columns.map(entry.renamed), target: Option.map(Option.fromNullOr(row.target), resolved.declaredName) }));
        const stale = entry.rendered.links.filter((link) => !constraints.some((row) => row.kind === 'f' && row.name === link.name && Equal.equals(row.columns, link.columns) && Option.contains(row.target, link.target) && row.onDelete === link.action));
        const planned = (index: (typeof entry.rendered.indexes)[number]) => {
            const target = Array.findFirst(desired.indexes, (row) => row.name === index.name);
            const fits = (row: (typeof live.indexes)[number]) => row.valid && Option.exists(target, (wanted) => Equal.equals(shape(row, entry.renamed), shape(wanted, Function.identity)));
            const present = Array.findFirst(current.indexes, (row) => row.name === index.name);
            const before = Option.filter(
                Array.findFirst(current.indexes, (row) => row.name === prior(index.parts)),
                (row) => row.name !== index.name,
            );
            const [keep, rename] = [Option.exists(present, fits), Option.isNone(present) && Option.exists(before, fits)];
            return { index, keep, rename, dropped: [...Option.toArray(present).filter(() => !keep), ...Option.toArray(before).filter(() => !rename)].map((row) => row.name) };
        };
        const plans = entry.rendered.indexes.map(planned);
        const policy = (rule: Option.Option.Value<typeof entry.rendered.policy>) => {
            const before = Option.filter(
                Array.findFirst(current.policies, (row) => row.name === prior(['tenant'])),
                (row) => row.name !== rule.name && !current.policies.some((other) => other.name === rule.name),
            );
            const present = Option.orElse(
                Array.findFirst(current.policies, (row) => row.name === rule.name),
                () => before,
            );
            const wanted = Array.findFirst(desired.policies, (row) => row.name === rule.name);
            const same = Option.exists(Option.all({ present, wanted }), (pair) => pair.present.using === pair.wanted.using && pair.present.check === pair.wanted.check);
            return [
                ...Option.toArray(before).map((row) => `ALTER POLICY ${quote(row.name)} ON ${owner} RENAME TO ${quote(rule.name)}`),
                ...(Option.isNone(present) ? [rule.text(role)] : []),
                ...(Option.isSome(present) && !same ? [`ALTER POLICY ${quote(rule.name)} ON ${owner} TO ${role} USING (${rule.rule}) WITH CHECK (${rule.rule})`] : []),
            ];
        };
        const pairs = (entry.owned ? entry.fields.filter(({ definition }) => definition.retype) : []).flatMap(({ definition: column, found }) => Option.toArray(Option.all({ wants: Array.findFirst(desired.columns, (row) => row.name === column.name), now: found })).map((pair) => ({ column, ...pair })));
        const moved = pairs.filter(({ wants, now }) => now.type !== wants.type);
        const constrained = <K extends { readonly kind: string; readonly columns: readonly string[] }>(rows: readonly K[]) => rows.filter((row) => row.kind === 'c' && pairs.some(({ column }) => Equal.equals(row.columns, [column.name])));
        const [had, needs] = [constrained(constraints), constrained(desired.constraints)];
        const [obsolete, missing] = [had.filter((row) => !needs.some((other) => other.definition === row.definition)), needs.filter((row) => !had.some((other) => other.definition === row.definition))];
        const dependents = current.columns.filter((row) => row.generated !== '' && moved.some(({ now }) => row.sources.includes(now.name)));
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
                    block(`IF EXISTS (SELECT 1 FROM ${owner} WHERE (${quote(column.name)}::${wants.type})::${now.type} IS DISTINCT FROM ${quote(column.name)}) THEN RAISE EXCEPTION 'ALTER COLUMN %.% TYPE % changes stored values', ${literal(name)}, ${literal(column.name)}, ${literal(wants.type)}; END IF;`),
                ),
            ...Array.match(changes, { onEmpty: () => [], onNonEmpty: (all) => [`ALTER TABLE ${owner} ${all.join(', ')}`] }),
            ...tightened.map(({ column, constraint }) => `ALTER TABLE ${owner} ADD CONSTRAINT ${quote(constraint)} NOT NULL ${column} NOT VALID`),
            ...missing.map((row) => `ALTER TABLE ${owner} ADD CONSTRAINT ${quote(row.name)} ${row.definition} NOT VALID`),
            ...dependents.map((row) => `ALTER TABLE ${owner} ADD COLUMN ${quote(row.name)} ${row.type} GENERATED ALWAYS AS (${row.expression}) ${row.generated === 's' ? 'STORED' : 'VIRTUAL'}`),
            ...current.indexes.filter((index) => dependents.some((row) => index.columns.includes(row.name))).map(({ definition }) => definition),
        ];
        const added = entry.added.flatMap((definition) => desired.columns.filter((row) => row.name === definition.name).map((row) => ({ definition, row })));
        const adding = added.map(({ definition }) => definition.name);
        const additions = desired.constraints.filter((row) => row.kind === 'c' && row.columns.every((part) => adding.includes(part)));
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
                ...stale.flatMap((link) => [...(constraints.some((row) => row.name === link.name) ? [`ALTER TABLE ${owner} DROP CONSTRAINT ${quote(link.name)}`] : []), `ALTER TABLE ${owner} ADD CONSTRAINT ${quote(link.name)} ${link.text} NOT VALID`]),
                ...grants(owner, entry.live.grants),
                ...(tenant && !entry.live.rls ? [`ALTER TABLE ${owner} ENABLE ROW LEVEL SECURITY`] : []),
                ...(tenant && !entry.live.force ? [`ALTER TABLE ${owner} FORCE ROW LEVEL SECURITY`] : []),
                ...Option.match(entry.rendered.policy, { onNone: () => [], onSome: policy }),
            ],
            dropped: [...plans.flatMap(({ dropped }) => dropped), ...current.indexes.filter((row) => entry.retiredIndexes.includes(row.name)).map((row) => row.name)],
            built: plans.filter(({ keep, rename }) => !(keep || rename)).map(({ index }) => index.text('CONCURRENTLY IF NOT EXISTS ')),
            validated: [...constraints.filter((row) => !row.valid).map((row) => row.name), ...additions.map((row) => row.name), ...tightened.map(({ constraint }) => constraint), ...missing.map((row) => row.name), ...stale.map((link) => link.name)],
            undeclared: current.indexes.filter((row) => !(entry.retiredIndexes.includes(row.name) || plans.some(({ index }) => [index.name, prior(index.parts)].includes(row.name)))).map((row) => row.name),
        };
    };
    const tables = resolved.matched.map(analyze);
    const creating = ({ declared, rendered }: (typeof resolved.created)[number]) => [
        rendered.create,
        ...rendered.indexes.map(({ text }) => text('')),
        ...grants(rendered.relation, []),
        ...Option.toArray(declared.table.tenant).flatMap(() => [`ALTER TABLE ${rendered.relation} ENABLE ROW LEVEL SECURITY`, `ALTER TABLE ${rendered.relation} FORCE ROW LEVEL SECURITY`]),
        ...Option.toArray(rendered.policy).map(({ text }) => text(role)),
    ];
    const removed = resolved.removed.map(({ name }) => name);
    const kept = resolved.kept.map(({ name }) => name);
    return yield* resolved.plan({
        findings: live.constraints.flatMap((row) => Option.toArray(Option.filter(Option.fromNullOr(row.target), (target) => !row.scratch && row.kind === 'f' && kept.includes(row.table) && removed.includes(target))).map((subject) => Finding.cases.RetainedReference.make({ table: row.table, subject }))),
        preamble: PgPrelude.preamble,
        opening: Array.difference(drawings.some(({ rendered }) => rendered.indexes.some(({ trigram }) => trigram)) ? ['pg_trgm'] : [], extensions).map((extension) => `CREATE EXTENSION IF NOT EXISTS ${quote(extension)}`),
        statements: [...tables.flatMap(({ relabels }) => relabels), ...resolved.created.flatMap(creating), ...tables.flatMap((entry) => entry.statements)],
        concurrent: [
            ...tables.flatMap(({ dropped }) => dropped.map((index) => `DROP INDEX CONCURRENTLY IF EXISTS ${quote(index)}`)),
            ...tables.flatMap(({ built }) => built),
            ...tables.flatMap(({ rendered, validated }) => validated.map((constraint) => `ALTER TABLE ${rendered.relation} VALIDATE CONSTRAINT ${quote(constraint)}`)),
        ],
        indexes: tables.flatMap(({ undeclared }) => undeclared),
    });
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const planning = (sql: SqlClient.SqlClient, catalog: Catalog, runtime: Option.Option<string>) => sql.onDialectOrElse({ sqlite: () => planSqlite(sql, catalog), pg: () => planPg(sql, catalog, runtime), orElse: () => Effect.die('reconcile runs on SQLite and PostgreSQL clients') });
const plan = Effect.fn('plan')(function* (catalog: Catalog, runtime: Option.Option<string>) {
    const [sql, d1] = yield* Effect.all([SqlClient.SqlClient, Effect.serviceOption(D1Client.D1Client)]);
    const steps = planning(sql, catalog, runtime);
    return Struct.pick(yield* Option.isSome(d1) ? steps : sql.withTransaction(steps), ['preamble', 'statements', 'concurrent', 'undeclared']);
});
const reconcile = Effect.fn('reconcile')(function* (catalog: Catalog, runtime: Option.Option<string>) {
    const [sql, d1] = yield* Effect.all([SqlClient.SqlClient, Effect.serviceOption(D1Client.D1Client)]);
    const apply = ({ statements, ledger: written }: Effect.Success<ReturnType<typeof planning>>) =>
        Effect.forEach(Option.toArray(written), ({ revision, document }) => Effect.all([ledger.insert([{ revision: revision + 1, identity: document }]), ledger.remove('revision', [revision])]).pipe(Effect.map(Array.prependAll(statements.map((text) => sql.unsafe(text)))), Effect.flatMap(atomic)));
    const applied = Effect.tap(planning(sql, catalog, runtime), apply);
    const { concurrent, undeclared } = yield* Option.match(d1, {
        onNone: () => sql.withTransaction(applied),
        onSome: () =>
            Effect.catchTag(applied, 'SqlError', (error) =>
                Effect.filterOrFail(
                    planning(sql, catalog, runtime),
                    ({ ledger: written }) => Option.isNone(written),
                    () => error,
                ),
            ),
    });
    yield* Effect.forEach(concurrent, (text) => sql.unsafe(text), { discard: true });
    return undeclared;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { plan, ReconcileError, reconcile };
