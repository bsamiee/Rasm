import { Array, Effect, Equal, Match, Option, Record, Result, Schema, String, Struct } from 'effect';
import type { SqlClient, Statement } from 'effect/sql';
import type { Catalog } from '#/catalog';
import type { Engine } from '#/engine';
import { Finding } from '#/finding';
import { constant, diff, identifier, literal, referentialActions, render, unsupported } from '#/reconcile';
import { atomic } from '#/transaction';

// --- [CONSTANTS] -----------------------------------------------------------------------

const patterns = { words: /\s+/u, comment: /"(?<name>(?:[^"]|"")+)"\s*\/\*\s*(?<identity>\{.*?\})\s*\*\//gu };

// --- [MODELS] --------------------------------------------------------------------------

const Excluded = Schema.fromJsonString(Schema.Array(Schema.String));
const SqliteTable = Schema.Struct({
    name: Schema.String,
    strict: Schema.BooleanFromBit,
    withoutRowid: Schema.BooleanFromBit,
    sql: Schema.String,
    columns: Schema.Array(Schema.Struct({ name: Schema.String, type: Schema.String, required: Schema.BooleanFromBit, fallback: Schema.OptionFromNullOr(Schema.String), hidden: Schema.Int })),
    references: Schema.Array(Schema.Struct({ target: Schema.String, onDelete: Schema.Literals(Record.keys(referentialActions)), onUpdate: Schema.Literals(Record.keys(referentialActions)), columns: Schema.NonEmptyArray(Schema.String), to: Schema.Array(Schema.String) })),
    indexes: Schema.Array(Schema.Struct({ name: Schema.String, sql: Schema.String, unique: Schema.BooleanFromBit, partial: Schema.BooleanFromBit, keys: Schema.Array(Schema.Struct({ name: Schema.NullOr(Schema.String), collation: Schema.String, descending: Schema.BooleanFromBit })) })),
    triggers: Schema.Array(Schema.Struct({ name: Schema.String, sql: Schema.String })),
});
class Sqlite extends Schema.Class<Sqlite, { readonly brand: unique symbol }>('Sqlite')({
    tables: Schema.Array(SqliteTable),
    virtual: Schema.Array(Schema.Struct({ name: Schema.String, sql: Schema.String })),
    sequences: Schema.Array(Schema.Struct({ name: Schema.String, seq: Schema.Int })),
}) {
    // --- [ROWS]
    static readonly types = { text: 'TEXT', uuid: 'TEXT', json: 'TEXT', instant: 'TEXT', integer: 'INTEGER', int64: 'INTEGER', decimal: 'TEXT', real: 'REAL', number: 'REAL', boolean: 'INTEGER', blob: 'BLOB' } as const satisfies Engine['types'];
    static readonly member: Engine['member'] = (sql, field) => sql`e.value ->> ${sql.literal(literal(`$."${field}"`))}`;
    static readonly element: Engine['element'] = (sql, { kind }, text) => (kind === 'blob' ? sql`unhex(${text})` : text);
    static readonly texts: Engine['texts'] = (sql, body) => sql.join(' ', false)([sql`json_each(${body})`, 'e']);
    static readonly insertion: Engine['insertion'] = (sql, body) => sql.join(' ', false)([Sqlite.texts(sql, body), 'WHERE true']);
    static readonly read: Engine['read'] = (sql, { name, kind }, scope) => (kind === 'int64' ? sql.join(' AS ', false)([sql`CAST(${scope}${sql(name)} AS TEXT)`, sql`${sql(name)}`]) : sql`${scope}${sql(name)}`);
    static readonly upsert: Engine['upsert'] = ({ sql, owner, prefix, bare, order, ignore, element, keys, reads, inserting }, body, set) =>
        Effect.map(
            atomic([
                inserting(body, ignore, reads(bare, '1 AS inserted')),
                ...Array.match(set, {
                    onEmpty: () => [],
                    onNonEmpty: (changed) => [
                        sql`UPDATE ${owner} SET (${sql.csv(changed.map(({ name }) => sql`${sql(name)}`))}) = (${sql.csv(changed.map(element))}) FROM ${Sqlite.texts(sql, body)} WHERE (${keys(prefix)}) = (${order}) AND (${sql.csv(changed.map(({ name }) => sql`${prefix}${sql(name)}`))}) IS DISTINCT FROM (${sql.csv(changed.map(element))}) RETURNING ${reads(bare, '0 AS inserted')}`,
                    ],
                }),
            ]),
            Array.flatten,
        );
    static readonly ensure: Engine['ensure'] = ({ sql, owner, prefix, bare, order, ignore, keys, reads, inserting }, body) =>
        Effect.map(atomic([inserting(body, ignore, reads(bare)), sql`SELECT ${reads(bare)} FROM ${owner} WHERE (${keys(bare)}) IN (SELECT ${order} FROM ${Sqlite.texts(sql, body)}) ORDER BY ${keys(prefix)}`]), Array.lastNonEmpty);
    static readonly latest: Engine['latest'] = ({ sql, owner, keys, reads }, body, ranked, newest) => {
        const [outer, inner] = [sql.literal('t.'), sql.literal('i.')];
        return sql`SELECT ${reads(outer)} FROM ${Sqlite.texts(sql, body)} JOIN ${owner} t ON (${keys(outer)}) IN (${newest(keys(inner))}) ORDER BY e.key, ${ranked(outer)}`;
    };
    static readonly search: Engine['search'] = ({ sql, name: table, owner, prefix, keys, reads }, { columns: searched, tokenizer }, text, pattern, marks, count) => {
        const virtual = sql(`${table}_fts`);
        const terms = (tokenizer === 'trigram' ? [text] : text.trim().split(patterns.words)).map((term) => `"${term.replaceAll('"', '""')}"`).join(' ');
        const marked = Option.match(marks, { onNone: () => sql.literal('NULL'), onSome: ({ open, close }) => sql`json_object(${sql.csv(searched.flatMap((entry, index) => [literal(entry.field), sql`highlight(${virtual}, ${sql.literal(`${index}`)}, ${open}, ${close})`]))})` });
        const gram = 3;
        const [filter, rank] = tokenizer === 'trigram' && [...text].length < gram ? [sql.or(searched.map(({ name }) => sql`${virtual}.${sql(name)} LIKE ${pattern} ESCAPE '\\'`)), sql.literal('0')] : [sql`${virtual} MATCH ${terms}`, sql`-${virtual}."rank"`];
        return sql`SELECT ${reads(prefix)}, ${rank} AS "rank", ${marked} AS "highlight" FROM ${virtual} JOIN ${owner} ON ${owner}."rowid" = ${virtual}."rowid" WHERE ${filter} ORDER BY ${virtual}."rank", ${keys(prefix)} LIMIT ${count}`;
    };

    // --- [DDL]
    static readonly constant: Engine['constant'] = constant;
    static readonly codePoints: Engine['codePoints'] = (sql, subject) => sql`length(${subject})`;
    static readonly domain: Engine['domain'] = (sql, subject, kind) =>
        Match.value(kind).pipe(
            Match.when('boolean', () => [sql`${subject} IN (0, 1)`]),
            Match.when('json', () => [sql`json_valid(${subject})`]),
            Match.orElse(() => []),
        );
    static readonly projection: Engine['projection'] = (sql, { kind }, from, path) => sql.join(' ', false)([sql`${sql(from)}`, kind === 'json' ? '->' : '->>', literal(`$${path.map((part) => `."${part}"`).join('')}`)]);
    static readonly generated: Engine['generated'] = (sql, expression) => sql.join(' ', false)(['AS', sql`(${expression})`, 'VIRTUAL']);
    static readonly constraint: Engine['constraint'] = (_, __, text) => text;
    static readonly casefold: Engine['casefold'] = (sql, name) => sql`${sql(name)} COLLATE NOCASE`;
    static readonly opclass: Engine['opclass'] = () => Option.none();
    static readonly projected: Engine['projected'] = (sql, _, name) => sql`${sql(name)}`;
    static readonly methods: Engine['methods'] = ['btree'];
    static readonly covering: Engine['covering'] = (sql, unique, include) => ({ keys: unique ? [] : include.map((name) => sql`${sql(name)}`), clause: [] });
    static readonly suffix: Engine['suffix'] = (withoutRowid) => (withoutRowid ? ' STRICT, WITHOUT ROWID' : ' STRICT');
    static readonly fulltext: Engine['fulltext'] = (sql, owner, { columns: searched, tokenizer }) => {
        const fts = `${owner}_fts`;
        const [virtual, table, values] = [sql(fts), sql(owner), sql.csv(searched.map(({ name }) => sql`${sql(name)}`))];
        const row = (prefix: string) => sql.csv(searched.map(({ name }) => sql`${sql.literal(prefix)}.${sql(name)}`));
        const [inserted, deleted] = [sql`INSERT INTO ${virtual} ("rowid", ${values}) VALUES (NEW."rowid", ${row('NEW')});`, sql`INSERT INTO ${virtual} (${virtual}, "rowid", ${values}) VALUES ('delete', OLD."rowid", ${row('OLD')});`];
        const trigger = (event: string, verb: string, body: Statement.Fragment) => ({ name: `${fts}_${event}`, create: sql`CREATE TRIGGER ${sql(`${fts}_${event}`)} AFTER ${sql.literal(verb)} ON ${table} BEGIN ${body} END` });
        return {
            indexes: [],
            fts: Option.some({
                name: fts,
                create: sql`CREATE VIRTUAL TABLE ${virtual} USING fts5 (${values}, content = ${sql.literal(literal(owner))}, content_rowid = 'rowid', tokenize = ${sql.literal(literal(tokenizer === 'trigram' ? tokenizer : tokenizer.tokenize))})`,
                triggers: [trigger('insert', 'INSERT', inserted), trigger('delete', 'DELETE', deleted), trigger('update', 'UPDATE', sql.join(' ', false)([deleted, inserted]))],
                rebuild: sql`INSERT INTO ${virtual} (${virtual}) VALUES ('rebuild')`,
            }),
        };
    };
    static readonly policy: Engine['policy'] = () => Option.none();
    static readonly comment: Engine['comment'] = (sql, _, identity) => ({ inline: sql.literal(` /* ${JSON.stringify(identity).replaceAll('*/', '*\\/')} */`), statements: () => [] });

    // --- [RECONCILE]
    static readonly refusals: Engine['refusals'] = ({ name: owner, key, columns, search, tenant, withoutRowid }) => {
        const all = Object.values(columns);
        const unkeyed = Option.isSome(search) && (withoutRowid || !all.some(({ name, kind }) => kind === 'integer' && Equal.equals(key, [name])));
        return [...unsupported(owner, 'tenant', all, ({ name }) => Option.exists(tenant, ({ column }) => column === name)), ...unsupported(owner, 'search', all, ({ name }) => unkeyed && key.includes(name))];
    };
    static readonly open: Engine['open'] = (sql) => sql`PRAGMA optimize = 0x10002`;
    static readonly commented = ({ columns, ...table }: typeof SqliteTable.Type) => {
        const found = Array.filterMap(String.matchAll(patterns.comment)(table.sql), ({ groups }) => (groups?.name === undefined || groups.identity === undefined ? Result.failVoid : Result.succeed({ name: groups.name.replaceAll('""', '"'), comment: groups.identity })));
        const own = Option.filter(Array.head(found), ({ name }) => name === table.name);
        const rest = Option.isSome(own) ? Array.drop(found, 1) : found;
        return {
            ...table,
            identity: Option.getOrNull(Option.map(own, Struct.get('comment'))),
            columns: columns.map((column) => ({
                ...column,
                identity: Option.getOrNull(
                    Option.map(
                        Array.findFirst(rest, ({ name }) => name === column.name),
                        Struct.get('comment'),
                    ),
                ),
            })),
        };
    };
    static readonly observe = (sql: SqlClient.SqlClient, excluded: readonly string[]) =>
        Effect.all([Schema.encodeEffect(Excluded)(excluded), sql`SELECT name FROM pragma_table_list WHERE schema = 'main' AND name = 'sqlite_sequence'`]).pipe(
            Effect.flatMap(
                ([prefixes, listed]) =>
                    sql`WITH relations AS (SELECT l.name, l.type, l.strict, l.wr, m.sql FROM pragma_table_list l JOIN sqlite_schema m ON m.name = l.name WHERE l.schema = 'main' AND l.name NOT GLOB 'sqlite_*' AND NOT EXISTS (SELECT 1 FROM json_each(${prefixes}) p WHERE instr(l.name, p.value) = 1))
                    SELECT json_object(
                        'tables', (SELECT json_group_array(json_object('name', r.name, 'strict', r.strict, 'withoutRowid', r.wr, 'sql', r.sql,
                            'columns', (SELECT json_group_array(json_object('name', x.name, 'type', x.type, 'required', x."notnull", 'fallback', x.dflt_value, 'hidden', x.hidden)) FROM pragma_table_xinfo(r.name) x),
                            'references', (SELECT json_group_array(json_object('target', f."table", 'onDelete', f.on_delete, 'onUpdate', f.on_update,
                                'columns', (SELECT json_group_array(k."from" ORDER BY k.seq) FROM pragma_foreign_key_list(r.name) k WHERE k.id = f.id),
                                'to', (SELECT json_group_array(k."to" ORDER BY k.seq) FROM pragma_foreign_key_list(r.name) k WHERE k.id = f.id AND k."to" IS NOT NULL))) FROM pragma_foreign_key_list(r.name) f WHERE f.seq = 0),
                            'indexes', (SELECT json_group_array(json_object('name', i.name, 'sql', m.sql, 'unique', i."unique", 'partial', i.partial,
                                'keys', (SELECT json_group_array(json_object('name', k.name, 'collation', k.coll, 'descending', k."desc")) FROM pragma_index_xinfo(i.name) k WHERE k."key" = 1))) FROM pragma_index_list(r.name) i JOIN sqlite_schema m ON m.name = i.name AND m.type = 'index' WHERE i.origin = 'c'),
                            'triggers', (SELECT json_group_array(json_object('name', m.name, 'sql', m.sql)) FROM sqlite_schema m WHERE m.type = 'trigger' AND m.tbl_name = r.name))) FROM relations r WHERE r.type = 'table'),
                        'virtual', (SELECT json_group_array(json_object('name', r.name, 'sql', r.sql)) FROM relations r WHERE r.type = 'virtual'),
                        'sequences', ${Array.isReadonlyArrayNonEmpty(listed) ? sql`(SELECT json_group_array(json_object('name', s.name, 'seq', s.seq)) FROM sqlite_sequence s)` : sql.literal('json_array()')}
                    ) AS catalog`,
            ),
            Effect.flatMap(Schema.decodeUnknownEffect(Schema.Tuple([Schema.Struct({ catalog: Schema.fromJsonString(Sqlite) })]))),
            Effect.map(([{ catalog }]) => ({ tables: catalog.tables.map(Sqlite.commented), virtual: catalog.virtual, sequences: catalog.sequences })),
        );
    static readonly plan: Engine['plan'] = Effect.fnUntraced(function* (sql: SqlClient.SqlClient, catalog: Catalog) {
        const live = yield* Sqlite.observe(sql, catalog.excluded ?? []);
        const resolved = diff(
            sql,
            catalog,
            catalog.tables.map((declared) => ({ declared, rendered: render(sql, Sqlite, catalog, declared) })),
            { tables: live.tables, columns: ({ columns }) => columns },
        );
        const quoted = (name: string) => sql`${sql(name)}`;
        const analyze = (entry: (typeof resolved.matched)[number]) => {
            const dropping = entry.doomed.map(({ name }) => name);
            const present = [...entry.fields.map(({ definition }) => definition.name), ...entry.retained.map(({ name }) => name)];
            const linked = entry.live.references.map((reference) => {
                const parent = resolved.referenced(reference.target);
                return { ...reference, target: parent.name, columns: Array.map(reference.columns, entry.renamed), to: reference.to.map(parent.renamed) };
            });
            const wanted = entry.rendered.indexes.map((index) => ({ ...index, created: index.text(''), prior: identifier([entry.live.name, ...index.parts.map(entry.original)]) }));
            const fits = (row: (typeof entry.live.indexes)[number], index: (typeof wanted)[number]) =>
                Option.exists(
                    index.declared,
                    ({ keys, unique, include, where }) =>
                        row.unique === unique &&
                        row.partial === Array.isReadonlyArrayNonEmpty(where) &&
                        Equal.equals(
                            row.keys.map(({ name, collation, descending }) => ({ name: name === null ? name : entry.renamed(name), collation, descending })),
                            [...keys.map(({ column, casefold }) => ({ name: column, collation: casefold ? 'NOCASE' : 'BINARY', descending: false })), ...(unique ? [] : include).map((name) => ({ name, collation: 'BINARY', descending: false }))],
                        ) &&
                        (!row.partial || row.sql === index.created.compile(true)[0]),
                );
            const stale = (row: (typeof entry.live.indexes)[number]) => wanted.some((index) => (index.name === row.name && !fits(row, index)) || (index.prior === row.name && index.prior !== index.name));
            const dropped = entry.live.indexes.filter((row) => entry.retiredIndexes.includes(row.name) || row.keys.some(({ name }) => name !== null && dropping.includes(name)) || stale(row));
            return {
                ...entry,
                linked,
                unstated: linked.filter(({ columns }) => columns.every((column) => present.includes(column)) && !entry.rendered.links.some((link) => Equal.equals(link.columns, columns))),
                rebuild:
                    entry.owned &&
                    (!entry.live.strict ||
                        entry.live.withoutRowid !== entry.declared.withoutRowid ||
                        entry.rendered.links.some((link) => !linked.some(({ target, columns, onDelete }) => target === link.target && onDelete === link.onDelete && Equal.equals(columns, link.columns))) ||
                        linked.some(({ columns }) => columns.some((column) => dropping.includes(column))) ||
                        (entry.live.name !== entry.declared.name && entry.live.identity === null) ||
                        entry.fields.some(({ definition, found }) => Option.exists(found, ({ name, identity }) => name !== definition.name && identity === null)) ||
                        entry.fields.some(
                            ({ definition, found, recorded }) =>
                                definition.retype && Option.exists(found, ({ type, required, fallback }) => type !== definition.type || required === definition.nullable || !Equal.equals(fallback, definition.fallback) || !Option.contains(Option.map(recorded, Struct.get('digest')), definition.digest)),
                        )),
                dropped,
                indexes: wanted.map(({ created }) => created),
                missing: wanted.filter(({ name }) => !entry.live.indexes.some((row) => row.name === name && !dropped.includes(row))).map(({ created }) => created),
                undeclared: entry.live.indexes.filter((row) => !(dropped.includes(row) || wanted.some(({ name }) => name === row.name))),
            };
        };
        const tables = resolved.matched.map(analyze);
        const closure = (members: readonly (typeof tables)[number][]): readonly (typeof tables)[number][] => {
            const parents = members.map(({ declared }) => declared.name);
            const next = tables.filter((entry) => members.includes(entry) || entry.linked.some(({ target }) => parents.includes(target)));
            return next.length === members.length ? members : closure(next);
        };
        const rebuilds = closure(tables.filter(({ rebuild }) => rebuild));
        const temporary = (name: string) => quoted(rebuilds.some(({ declared }) => declared.name === name) ? `${name}__rebuild` : name);
        const searched = (entry: (typeof tables)[number], recreated: boolean) =>
            Option.match(entry.rendered.fts, {
                onNone: () => ({ fresh: false, dropped: [], created: [] }),
                onSome: (fts) => {
                    const fresh = live.virtual.find(({ name }) => name === fts.name)?.sql !== fts.create.compile(true)[0];
                    const stale = live.virtual.filter(({ name }) => (name === fts.name && fresh) || (name === `${entry.live.name}_fts` && name !== fts.name));
                    const triggers = recreated ? [] : entry.live.triggers.filter(({ name }) => name.startsWith(`${entry.live.name}_fts_`) || name.startsWith(`${fts.name}_`));
                    const current = triggers.filter((row) => fts.triggers.some(({ name, create }) => name === row.name && create.compile(true)[0] === row.sql));
                    return {
                        fresh,
                        dropped: [...stale.map(({ name }) => sql`DROP TABLE ${sql(name)}`), ...Array.difference(triggers, current).map(({ name }) => sql`DROP TRIGGER ${sql(name)}`)],
                        created: [...(fresh ? [fts.create] : []), ...fts.triggers.filter(({ name }) => !current.some((row) => row.name === name)).map(({ create }) => create), ...(fresh ? [fts.rebuild] : [])],
                    };
                },
            });
        const sequenced = ({ name: owner }: (typeof tables)[number]['live']) => live.sequences.some(({ name }) => name === owner);
        const rebuilt = (entry: (typeof tables)[number]) => {
            const kept = entry.retained.filter(({ hidden }) => hidden === 0);
            const copied = sql.csv([...(entry.live.withoutRowid || entry.declared.withoutRowid ? [] : [quoted('rowid')]), ...[...entry.fields.filter(({ definition, found }) => Option.isNone(definition.projection) && Option.isSome(found)).map(({ definition }) => definition), ...kept].map(({ name }) => quoted(name))]);
            const observed = sql.csv([...(entry.live.withoutRowid ? [] : [quoted('rowid')]), ...entry.live.columns.map(({ name }) => quoted(entry.renamed(name)))]);
            const elements = [
                ...kept.map(({ name, type, required: needed, fallback, identity }) => sql.join(' ', false)([sql`${sql(name)}${sql.literal(identity === null ? '' : ` /* ${identity} */`)}`, type === '' ? 'ANY' : type, ...(needed ? ['NOT NULL'] : []), ...Option.toArray(fallback).map((value) => `DEFAULT ${value}`)])),
                ...entry.unstated.map(({ columns, target, to, onDelete, onUpdate }) =>
                    sql.join(' ', false)(['FOREIGN KEY', sql`(${sql.csv(columns.map(quoted))})`, 'REFERENCES', temporary(target), ...Array.match(to, { onEmpty: () => [], onNonEmpty: (keys) => [sql`(${sql.csv(keys.map(quoted))})`] }), `ON UPDATE ${onUpdate} ON DELETE ${onDelete}`]),
                ),
            ];
            return {
                ...entry,
                search: searched(entry, true),
                create: render(sql, Sqlite, catalog, entry.declared, {
                    target: temporary,
                    elements,
                    autoincrement: sequenced(entry.live),
                    identity: Option.some(entry.identity),
                    identities: (name) =>
                        Option.map(
                            Array.findFirst(entry.fields, ({ definition }) => definition.name === name),
                            Struct.get('identity'),
                        ),
                }).create,
                copy: sql`WITH live(${observed}) AS (SELECT ${sql.literal(entry.live.withoutRowid ? '*' : '"rowid", *')} FROM ${entry.rendered.relation}) INSERT INTO ${temporary(entry.declared.name)} (${copied}) SELECT ${copied} FROM live`,
            };
        };
        const altered = Array.difference(tables, rebuilds).map((entry) => ({ ...entry, search: searched(entry, false) }));
        const rebuilding = rebuilds.map(rebuilt);
        const removed = resolved.removed.map(({ name }) => name);
        const replaced = [...rebuilds.map(({ live: row }) => row.name), ...removed];
        return yield* resolved.plan({
            findings: [
                ...rebuilds.flatMap((entry) => unsupported(entry.declared.name, 'generated', entry.retained, ({ hidden }) => hidden !== 0)),
                ...[
                    ...resolved.kept.flatMap((row) => row.references.filter(({ target }) => replaced.includes(target)).map((reference) => ({ ...reference, table: row.name }))),
                    ...tables.flatMap(({ declared, unstated }) => unstated.filter(({ target }) => removed.includes(target)).map((reference) => ({ ...reference, table: declared.name }))),
                ].map(({ table, columns, target }) => Finding.cases.RetainedReference.make({ table, columns, target })),
            ],
            drifted: ({ live: row }, { definition, found, recorded }) =>
                row.strict &&
                Option.exists(
                    found,
                    ({ type, required: needed, fallback }) =>
                        type !== definition.type || needed === definition.nullable || !Equal.equals(fallback, definition.fallback) || (Option.isNone(definition.projection) && Option.exists(Option.flatMapNullishOr(recorded, Struct.get('digest')), (digest) => digest !== definition.digest)),
                ),
            preamble: [],
            opening: Array.isReadonlyArrayNonEmpty(rebuilds) || Array.isReadonlyArrayNonEmpty(removed) ? [sql`PRAGMA defer_foreign_keys = on`] : [],
            statements: [
                ...live.virtual.filter(({ name }) => removed.some((owner) => name === `${owner}_fts`)).map(({ name }) => sql`DROP TABLE ${sql(name)}`),
                ...resolved.created.flatMap(({ rendered }) => [rendered.create, ...rendered.indexes.map(({ text }) => text('')), ...Option.match(rendered.fts, { onNone: () => [], onSome: (fts) => [fts.create, ...fts.triggers.map(({ create }) => create)] })]),
                ...altered.flatMap((entry) => [
                    ...entry.dropped.map(({ name }) => sql`DROP INDEX ${sql(name)}`),
                    ...entry.search.dropped,
                    ...entry.doomed.map(({ name }) => sql`ALTER TABLE ${entry.rendered.relation} DROP COLUMN ${sql(name)}`),
                    ...entry.added.map(({ text }) => sql`ALTER TABLE ${entry.rendered.relation} ADD COLUMN ${text}`),
                    ...entry.missing,
                    ...entry.search.created,
                ]),
                ...rebuilding.map(({ create }) => create),
                ...(rebuilding.some(({ live: row }) => sequenced(row)) ? [sql`INSERT INTO sqlite_sequence (name, seq) SELECT m.name, s.seq FROM sqlite_sequence s JOIN sqlite_schema m ON m.name = s.name || '__rebuild'`] : []),
                ...rebuilding.map(({ copy }) => copy),
                ...rebuilding.map(({ rendered }) => sql`DROP TABLE ${rendered.relation}`),
                ...rebuilding.map(({ declared, rendered }) => sql`ALTER TABLE ${temporary(declared.name)} RENAME TO ${rendered.relation}`),
                ...rebuilding.flatMap((entry) => [
                    ...entry.indexes,
                    ...entry.undeclared.map((index) => sql`${sql.literal(index.sql)}`),
                    ...entry.live.triggers.filter(({ name }) => !(name.startsWith(`${entry.live.name}_fts_`) || name.startsWith(`${entry.declared.name}_fts_`))).map((trigger) => sql`${sql.literal(trigger.sql)}`),
                    ...entry.search.dropped,
                    ...entry.search.created,
                ]),
                ...(resolved.created.some(({ rendered }) => Array.isReadonlyArrayNonEmpty(rendered.indexes) || Option.isSome(rendered.fts)) ||
                altered.some(({ missing, search }) => Array.isReadonlyArrayNonEmpty(missing) || search.fresh) ||
                rebuilding.some(({ indexes, search }) => Array.isReadonlyArrayNonEmpty(indexes) || search.fresh)
                    ? [sql`PRAGMA optimize`]
                    : []),
            ],
            concurrent: [],
            undeclared: { indexes: tables.flatMap(({ undeclared }) => undeclared.map(({ name }) => name)), references: tables.flatMap(({ declared, unstated }) => unstated.map(({ columns, target }) => ({ table: declared.name, columns, target }))) },
        });
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Sqlite };
