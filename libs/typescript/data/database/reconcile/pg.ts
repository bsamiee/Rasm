import { Array, Effect, Equal, Function, Option, Schema, Struct } from 'effect';
import type { SqlClient } from 'effect/sql';
import type { Catalog, Relation } from '../definition.ts';
import { identifier, literal, quote } from '../lexical.ts';
import { ddl } from './ddl.ts';
import { catalogDdl, diff } from './diff.ts';
import { latestRevision } from './ledger.ts';

// --- [MODELS] --------------------------------------------------------------------------

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
class PgPrelude extends Schema.Class<PgPrelude, { readonly brand: unique symbol }>('PgPrelude')({
    migrator: Schema.String,
    absent: Schema.Array(Schema.String),
    configurations: Schema.fromJsonString(Schema.Array(Schema.Struct({ name: Schema.String, configuration: Schema.String, configured: Schema.Boolean, drift: Schema.Array(Schema.Struct({ tokens: Schema.NonEmptyArray(Schema.String), dictionaries: Schema.OptionFromNullOr(Schema.NonEmptyArray(Schema.String)) })) }))),
}) {
    static readonly preamble = ["SET LOCAL lock_timeout = '5s'", "SET LOCAL transaction_timeout = '2min'"];
    static readonly open = (sql: SqlClient.SqlClient, required: readonly string[], filters: readonly string[], configurations: readonly Extract<Option.Option.Value<Relation['definition']['search']>['tokenizer'], object>[]) =>
        Effect.forEach(PgPrelude.preamble, (text) => sql.unsafe(text), { discard: true }).pipe(
            Effect.andThen(
                sql`SELECT pg_advisory_xact_lock(hashtextextended(current_schema(), 0))::text AS lock, set_config('search_path', concat_ws(', ', quote_ident(current_schema()),
                    (SELECT string_agg(quote_ident(n.nspname), ', ' ORDER BY n.nspname) FROM pg_namespace n WHERE n.oid NOT IN (current_schema()::regnamespace, 'pg_catalog'::regnamespace)
                        AND (EXISTS (SELECT 1 FROM pg_extension e WHERE e.extnamespace = n.oid AND e.extname = ANY(${required}::text[])) OR EXISTS (SELECT 1 FROM pg_ts_dict d WHERE d.dictnamespace = n.oid AND d.dictname = ANY(${filters}::text[])))), 'pg_temp'), true) AS search_path`,
            ),
            Effect.andThen(
                sql`WITH maps AS (SELECT m.mapcfg, m.maptokentype, array_agg(m.mapdict::regdictionary::text ORDER BY m.mapseqno) AS dictionaries FROM pg_ts_config_map m GROUP BY m.mapcfg, m.maptokentype),
                    declared AS (SELECT d.name, d.configuration::regconfig::oid AS base, (SELECT c.oid FROM pg_ts_config c WHERE c.cfgnamespace = current_schema()::regnamespace AND c.cfgname = d.name) AS live, array(SELECT quote_ident(f) FROM unnest(d.filters) f) AS filters FROM json_to_recordset(${JSON.stringify(configurations)}::json) d(name text, configuration text, filters text[])),
                    tokens AS (SELECT d.name, t.tokid, t.alias, CASE WHEN b.dictionaries IS NOT NULL THEN d.filters || b.dictionaries END AS wanted, l.dictionaries AS now
                        FROM declared d JOIN pg_ts_config c ON c.oid = d.base CROSS JOIN ts_token_type(c.cfgparser) t
                        LEFT JOIN maps b ON (b.mapcfg, b.maptokentype) = (d.base, t.tokid) LEFT JOIN maps l ON (l.mapcfg, l.maptokentype) = (coalesce(d.live, d.base), t.tokid))
                    SELECT current_user::text AS migrator,
                        array(SELECT a.name FROM (SELECT x FROM unnest(${required}::text[]) x WHERE NOT EXISTS (SELECT 1 FROM pg_extension e WHERE e.extname = x)
                            UNION SELECT f FROM unnest(${filters}::text[]) f WHERE NOT EXISTS (SELECT 1 FROM pg_ts_dict d WHERE d.dictname = f) AND EXISTS (SELECT 1 FROM pg_available_extensions v WHERE v.name = f)) a(name) ORDER BY a.name) AS absent, array_to_json(array(SELECT json_build_object('name', d.name, 'configuration', d.base::regconfig::text, 'configured', d.live IS NOT NULL,
                        'drift', array(SELECT json_build_object('tokens', array_agg(k.alias ORDER BY k.tokid), 'dictionaries', k.wanted) FROM tokens k WHERE k.name = d.name AND k.now IS DISTINCT FROM k.wanted GROUP BY k.wanted)) FROM declared d))::text AS configurations`,
            ),
            Effect.flatMap(Schema.decodeUnknownEffect(Schema.Tuple([PgPrelude]))),
            Effect.map(([prelude]) => prelude),
        );
}
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
    static readonly plan = Effect.fnUntraced(function* (sql: SqlClient.SqlClient, catalog: Catalog, runtime: Option.Option<string>) {
        const drawings = catalogDdl('pg', catalog);
        const indexes = drawings.flatMap(({ rendered }) => rendered.indexes);
        const required = Array.dedupe(indexes.flatMap(Struct.get('extensions')));
        const owned = Array.dedupeWith(
            indexes.flatMap(({ configuration }) => Option.toArray(configuration)),
            (self, that) => self.name === that.name,
        );
        const { migrator, absent, configurations } = yield* PgPrelude.open(sql, required, Array.dedupe(owned.flatMap(Struct.get('filters'))), owned);
        const [existing, drifted] = [configurations.filter(Struct.get('configured')).map(Struct.get('name')), configurations.filter(({ drift }) => Array.isReadonlyArrayNonEmpty(drift)).map(Struct.get('name'))];
        const ready = ({ extensions: needed, configuration }: (typeof indexes)[number]) => !needed.some((name) => absent.includes(name)) && Option.toArray(configuration).every(({ name }) => existing.includes(name));
        const stored = yield* latestRevision(sql);
        const role = Option.match(runtime, { onNone: () => 'PUBLIC', onSome: quote });
        const block = (body: string) => `DO ${literal(`BEGIN ${body} END`)}`;
        yield* sql.unsafe(
            block(
                drawings
                    .map(({ declared }) => ddl('pg', declared, (name) => `pg_temp.${quote(name)}`, { elements: [], autoincrement: false }))
                    .flatMap((rendered) => [`${rendered.create} ON COMMIT DROP`, ...rendered.indexes.filter(ready).map(({ text }) => text('')), ...Option.toArray(rendered.policy).map(({ text }) => text(role))])
                    .map((statement) => `${statement};`)
                    .join(' '),
            ),
        );
        const live = yield* Pg.observe(sql);
        const resolved = diff(catalog, drawings, { stored, tables: live.tables, columns: ({ columns }) => columns });
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
            const { name } = entry.declared.definition;
            const owner = entry.rendered.relation;
            const prior = (parts: readonly string[]) => identifier([entry.live.name, ...parts.map(entry.original)]);
            const named = [...entry.live.checks, ...entry.live.references].map(({ name: constraint }) => constraint);
            const relabels = [...entry.rendered.definitions.flatMap(({ check }) => Option.toArray(check)), ...entry.rendered.links]
                .filter(({ name: target, parts }) => named.includes(prior(parts)) && prior(parts) !== target && !named.includes(target))
                .map(({ name: target, parts }) => ({ from: prior(parts), to: target }));
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
            const tenant = Option.isSome(entry.declared.definition.tenant);
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
        const tables = resolved.matched.flatMap((entry) => Option.toArray(Array.findFirst(live.scratch, ({ name }) => name === entry.declared.definition.name)).map((desired) => analyze(entry, desired)));
        const creating = ({ declared, rendered }: (typeof resolved.created)[number]) => [
            rendered.create,
            ...rendered.indexes.map(({ text }) => text('')),
            ...grants(declared, rendered.relation, []),
            ...Option.toArray(declared.definition.tenant).flatMap(() => [`ALTER TABLE ${rendered.relation} ENABLE ROW LEVEL SECURITY`, `ALTER TABLE ${rendered.relation} FORCE ROW LEVEL SECURITY`]),
            ...Option.toArray(rendered.policy).map(({ text }) => text(role)),
        ];
        return yield* resolved.plan({
            findings: [],
            preamble: PgPrelude.preamble,
            opening: [
                ...absent.map((extension) => `CREATE EXTENSION IF NOT EXISTS ${quote(extension)}`),
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
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Pg };
