import { Array, Effect, Equal, Option, Record, Schema } from 'effect';
import type { SqlClient } from 'effect/sql';
import { type Catalog, ftsTable, referentialActions } from '#/definition';
import { identifier, quote } from '#/lexical';
import { ddl } from '#/reconcile/ddl';
import { catalogDdl, diff } from '#/reconcile/diff';
import { Finding, unsupported } from '#/reconcile/finding';
import { latestRevision } from '#/reconcile/ledger';

// --- [MODELS] --------------------------------------------------------------------------

class Sqlite extends Schema.Class<Sqlite, { readonly brand: unique symbol }>('Sqlite')({
    tables: Schema.Array(
        Schema.Struct({
            name: Schema.String,
            strict: Schema.BooleanFromBit,
            withoutRowid: Schema.BooleanFromBit,
            autoincrement: Schema.BooleanFromBit,
            sql: Schema.String,
            columns: Schema.Array(Schema.Struct({ name: Schema.String, type: Schema.String, required: Schema.BooleanFromBit, fallback: Schema.OptionFromNullOr(Schema.String), hidden: Schema.Int })),
            references: Schema.Array(Schema.Struct({ target: Schema.String, onDelete: Schema.Literals(Record.keys(referentialActions)), onUpdate: Schema.Literals(Record.keys(referentialActions)), columns: Schema.NonEmptyArray(Schema.String), to: Schema.Array(Schema.String) })),
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
    static readonly plan = Effect.fnUntraced(function* (sql: SqlClient.SqlClient, catalog: Catalog) {
        const stored = yield* latestRevision(sql);
        const live = yield* Sqlite.observe(sql);
        const resolved = diff(catalog, catalogDdl('sqlite', catalog), { stored, tables: live.tables, columns: ({ columns }) => columns });
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
                        entry.live.withoutRowid !== entry.declared.definition.withoutRowid ||
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
            const parents = members.map(({ declared }) => declared.definition.name);
            const next = tables.filter((entry) => members.includes(entry) || entry.linked.some(({ target }) => parents.includes(target)));
            return next.length === members.length ? members : closure(next);
        };
        const rebuilds = closure(tables.filter(({ rebuild }) => rebuild));
        const temporary = (name: string) => quote(rebuilds.some(({ declared }) => declared.definition.name === name) ? `${name}__rebuild` : name);
        const searched = (entry: (typeof tables)[number], recreated: boolean) =>
            Option.match(entry.rendered.fts, {
                onNone: () => ({ fresh: false, dropped: [], created: [] }),
                onSome: (fts) => {
                    const fresh = live.virtual.find(({ name }) => name === fts.name)?.sql !== fts.sql;
                    const stale = live.virtual.filter(({ name }) => (name === fts.name && fresh) || (name === ftsTable(entry.live.name) && name !== fts.name));
                    const triggers = recreated ? [] : entry.live.triggers.filter(({ name }) => name.startsWith(`${ftsTable(entry.live.name)}_`) || name.startsWith(`${fts.name}_`));
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
            const copied = [...(entry.live.withoutRowid || entry.declared.definition.withoutRowid ? [] : ['"rowid"']), ...[...entry.fields.filter(({ definition, found }) => Option.isNone(definition.projection) && Option.isSome(found)).map(({ definition }) => definition), ...kept].map(({ name }) => quote(name))].join(
                ', ',
            );
            const elements = [
                ...kept.map(({ name, type, required: needed, fallback }) => [quote(name), type === '' ? 'ANY' : type, ...(needed ? ['NOT NULL'] : []), ...Option.toArray(fallback).map((value) => `DEFAULT ${value}`)].join(' ')),
                ...entry.unstated.map(({ columns, target, to, onDelete, onUpdate }) => `FOREIGN KEY (${columns.map(quote).join(', ')}) REFERENCES ${temporary(target)}${Array.match(to, { onEmpty: () => '', onNonEmpty: (keys) => ` (${keys.map(quote).join(', ')})` })} ON UPDATE ${onUpdate} ON DELETE ${onDelete}`),
            ];
            return {
                ...entry,
                search: searched(entry, true),
                create: ddl('sqlite', entry.declared, temporary, { elements, autoincrement: entry.live.autoincrement }).create,
                copy: `INSERT INTO ${temporary(entry.declared.definition.name)} (${copied}) SELECT ${copied} FROM ${entry.rendered.relation}`,
            };
        };
        const altered = tables.filter((entry) => !rebuilds.includes(entry)).map((entry) => ({ ...entry, search: searched(entry, false) }));
        const rebuilding = rebuilds.map(rebuilt);
        const creating = ({ rendered }: (typeof resolved.created)[number]) => [rendered.create, ...rendered.indexes.map(({ text }) => text('')), ...Option.match(rendered.fts, { onNone: () => [], onSome: (found) => [found.sql, ...found.triggers.map((trigger) => trigger.sql)] })];
        const removed = resolved.removed.map(({ name }) => name);
        const replaced = [...rebuilds.map(({ live: row }) => row.name), ...removed];
        return yield* resolved.plan({
            findings: [
                ...rebuilds.flatMap((entry) => unsupported(entry.declared.definition.name, 'generated', entry.retained, ({ hidden }) => hidden !== 0)),
                ...[
                    ...resolved.kept.flatMap((row) => row.references.filter(({ target }) => replaced.includes(target)).map((reference) => ({ ...reference, table: row.name }))),
                    ...tables.flatMap(({ declared, unstated }) => unstated.filter(({ target }) => removed.includes(target)).map((reference) => ({ ...reference, table: declared.definition.name }))),
                ].map(({ table: owner, columns, target }) => Finding.cases.RetainedReference.make({ table: owner, columns, target })),
            ],
            preamble: [],
            opening: Array.isReadonlyArrayNonEmpty(rebuilds) || Array.isReadonlyArrayNonEmpty(removed) ? ['PRAGMA defer_foreign_keys = on'] : [],
            statements: [
                ...live.virtual.filter(({ name }) => removed.some((owner) => name === ftsTable(owner))).map(({ name }) => `DROP TABLE ${quote(name)}`),
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
                ...rebuilding.map(({ declared, rendered }) => `ALTER TABLE ${temporary(declared.definition.name)} RENAME TO ${rendered.relation}`),
                ...rebuilding.flatMap((entry) => [
                    ...entry.indexes,
                    ...entry.undeclared.map((index) => index.sql),
                    ...entry.live.triggers.filter(({ name }) => !(name.startsWith(`${ftsTable(entry.live.name)}_`) || name.startsWith(`${ftsTable(entry.declared.definition.name)}_`))).map((trigger) => trigger.sql),
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
            undeclared: { indexes: tables.flatMap(({ undeclared }) => undeclared.map(({ name }) => name)), references: tables.flatMap(({ declared, unstated }) => unstated.map(({ columns, target }) => ({ table: declared.definition.name, columns, target }))) },
        });
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Sqlite };
