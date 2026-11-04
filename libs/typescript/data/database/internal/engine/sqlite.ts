import { Array, Cause, Effect, Equal, Filter, Function, Graph, HashSet, Match, Option, Record, Result, Schema, Struct, UndefinedOr } from 'effect';
import { type SqlClient, SqlSchema, type Statement } from 'effect/sql';
import { atomic } from '#/atomic';
import type { Catalog } from '#/catalog';
import type { Engine } from '#/internal/engine/engine';
import { constant, diff, identifier, literal, referentialActions, render, unsupported } from '#/internal/reconcile';

// --- [CONSTANTS] -----------------------------------------------------------------------

const words = /\s+/u;
const lexemes = /'(?:[^']|'')*'|"(?:[^"]|"")*"|`(?:[^`]|``)*`|\[[^\]]*\]|\/\*[\s\S]*?\*\/|--[^\n]*|[(),]|[^\s'"`[(),/-]+|\S/gu;
const keywords: readonly string[] = ['CHECK', 'COLLATE', 'UNIQUE', 'REFERENCES', 'PRIMARY', 'CONSTRAINT', 'FOREIGN'];

// --- [MODELS] --------------------------------------------------------------------------

const SqliteTable = Schema.Struct({
    name: Schema.String,
    strict: Schema.BooleanFromBit,
    withoutRowid: Schema.BooleanFromBit,
    sql: Schema.String,
    columns: Schema.Array(Schema.Struct({ name: Schema.String, type: Schema.String, required: Schema.BooleanFromBit, default: Schema.OptionFromNullOr(Schema.String), hidden: Schema.Int })),
    references: Schema.Array(Schema.Struct({ target: Schema.String, onDelete: Schema.Literals(referentialActions), onUpdate: Schema.Literals(referentialActions), columns: Schema.NonEmptyArray(Schema.String), to: Schema.Array(Schema.String) })),
    indexes: Schema.Array(Schema.Struct({ name: Schema.String, sql: Schema.String, unique: Schema.BooleanFromBit, partial: Schema.BooleanFromBit, keys: Schema.Array(Schema.Struct({ name: Schema.NullOr(Schema.String), collation: Schema.String, descending: Schema.BooleanFromBit })) })),
    triggers: Schema.Array(Schema.Struct({ name: Schema.String, sql: Schema.String })),
});
class Sqlite extends Schema.Class<Sqlite, { readonly brand: unique symbol }>('Sqlite')({
    tables: Schema.Array(SqliteTable),
    virtual: Schema.Array(Schema.Struct({ name: Schema.String, sql: Schema.String })),
    sequences: Schema.Array(Schema.Struct({ name: Schema.String, seq: Schema.Int })),
}) {
    // --- [ROWS]
    static readonly types = { text: 'TEXT', uuid: 'TEXT', json: 'TEXT', instant: 'TEXT', integer: 'INTEGER', decimal: 'TEXT', real: 'REAL', boolean: 'INTEGER', blob: 'BLOB' } as const satisfies Engine['Service']['types'];
    static readonly member: Engine['Service']['member'] = (sql, field) => sql`e.value ->> ${sql.literal(literal(`$."${field}"`))}`;
    static readonly element: Engine['Service']['element'] = (sql, { kind }, text) => (kind === 'blob' ? sql`unhex(${text})` : text);
    static readonly texts: Engine['Service']['texts'] = (sql, body) => sql.join(' ', false)([sql`json_each(${body})`, 'e']);
    static readonly insertion: Engine['Service']['insertion'] = (sql, body) => sql.join(' ', false)([Sqlite.texts(sql, body), 'WHERE true']);
    static readonly read: Engine['Service']['read'] = (sql, { name, textual }, scope) => (textual ? sql`CAST(${scope}${sql(name)} AS TEXT) AS ${sql(name)}` : sql`${scope}${sql(name)}`);
    static readonly upsert: Engine['Service']['upsert'] = ({ sql, owner, prefix, unqualified, order, ignore, element, keys, reads, inserting, flags }, body, set) => {
        const inserted = (flag: 0 | 1) => sql`${sql.literal(`${flag}`)} AS ${sql(flags.inserted)}`;
        const update = sql`UPDATE ${owner} SET (${sql.csv(set.map(({ name }) => sql`${sql(name)}`))}) = (${sql.csv(set.map(element))}) FROM ${Sqlite.texts(sql, body)} WHERE (${keys(prefix)}) = (${order}) AND (${sql.csv(set.map(({ name }) => sql`${prefix}${sql(name)}`))}) IS DISTINCT FROM (${sql.csv(set.map(element))}) RETURNING ${reads(unqualified, inserted(0))}`;
        return Effect.map(atomic([inserting(body, ignore, reads(unqualified, inserted(1))), ...(Array.isReadonlyArrayNonEmpty(set) ? [update] : [])]), Array.flatten);
    };
    static readonly ensure: Engine['Service']['ensure'] = ({ sql, owner, prefix, unqualified, order, ignore, keys, reads, inserting }, body) =>
        Effect.map(atomic([inserting(body, ignore, reads(unqualified)), sql`SELECT ${reads(unqualified)} FROM ${owner} WHERE (${keys(unqualified)}) IN (SELECT ${order} FROM ${Sqlite.texts(sql, body)}) ORDER BY ${keys(prefix)}`]), Array.lastNonEmpty);
    static readonly search: Engine['Service']['search'] = ({ sql, name: table, owner, prefix, keys, reads, flags }, { columns: searched, tokenizer }, text, pattern, marks, count) => {
        const virtual = sql(`${table}_fts`);
        const gram = 3;
        const terms = (tokenizer === 'trigram' ? [text] : text.trim().split(words)).map((term) => `"${term.replaceAll('"', '""')}"`).join(' ');
        const marked = Option.match(marks, { onNone: () => sql.literal('NULL'), onSome: ({ open, close }) => sql`json_object(${sql.csv(searched.flatMap((entry, index) => [literal(entry.field), sql`highlight(${virtual}, ${sql.literal(`${index}`)}, ${open}, ${close})`]))})` });
        const [filter, rank] = tokenizer === 'trigram' && [...text].length < gram ? [sql.or(searched.map(({ name }) => sql`${virtual}.${sql(name)} LIKE ${pattern} ESCAPE '\\'`)), sql.literal('0')] : [sql`${virtual} MATCH ${terms}`, sql`-${virtual}."rank"`];
        return sql`SELECT ${reads(prefix)}, ${rank} AS ${sql(flags.rank)}, ${marked} AS ${sql(flags.highlight)} FROM ${virtual} JOIN ${owner} ON ${owner}."rowid" = ${virtual}."rowid" WHERE ${filter} ORDER BY ${virtual}."rank", ${keys(prefix)} LIMIT ${count}`;
    };

    // --- [DDL]
    static readonly constant: Engine['Service']['constant'] = constant;
    static readonly codePoints: Engine['Service']['codePoints'] = (sql, subject) => sql`length(${subject})`;
    static readonly text: Engine['Service']['text'] = (_, subject) => subject;
    static readonly domain: Engine['Service']['domain'] = (sql, subject, kind) =>
        Match.value(kind).pipe(
            Match.when('boolean', () => [sql`${subject} IN (0, 1)`]),
            Match.when('json', () => [sql`json_valid(${subject})`]),
            Match.orElse(() => []),
        );
    static readonly projection: Engine['Service']['projection'] = (sql, { kind }, { from, jsonPath }) => sql.join(' ', false)([sql`${sql(from)}`, kind === 'json' ? '->' : '->>', literal(jsonPath)]);
    static readonly generated: Engine['Service']['generated'] = (sql, expression) => sql.join(' ', false)(['AS', sql`(${expression})`, 'VIRTUAL']);
    static readonly constraint: Engine['Service']['constraint'] = (_, __, text) => text;
    static readonly casefold: Engine['Service']['casefold'] = (sql, name) => sql`${sql(name)} COLLATE NOCASE`;
    static readonly opclass: Engine['Service']['opclass'] = (method) => (method === 'btree' ? Option.some(Option.none()) : Option.none());
    static readonly projected: Engine['Service']['projected'] = (sql, _, name) => sql`${sql(name)}`;
    static readonly methods: Engine['Service']['methods'] = ['btree'];
    static readonly covering: Engine['Service']['covering'] = (sql, unique, include) => ({ keys: unique ? [] : include.map((name) => sql`${sql(name)}`), clause: [] });
    static readonly suffix: Engine['Service']['suffix'] = (withoutRowid) => (withoutRowid ? ' STRICT, WITHOUT ROWID' : ' STRICT');
    static readonly fulltext: Engine['Service']['fulltext'] = (sql, owner, { columns: searched, tokenizer }) => {
        const name = `${owner}_fts`;
        const [virtual, table, values] = [sql(name), sql(owner), sql.csv(searched.map((entry) => sql`${sql(entry.name)}`))];
        const row = (prefix: string) => sql.csv(searched.map((entry) => sql`${sql.literal(prefix)}.${sql(entry.name)}`));
        const [inserted, deleted] = [sql`INSERT INTO ${virtual} ("rowid", ${values}) VALUES (NEW."rowid", ${row('NEW')});`, sql`INSERT INTO ${virtual} (${virtual}, "rowid", ${values}) VALUES ('delete', OLD."rowid", ${row('OLD')});`];
        const trigger = (event: string, verb: string, body: Statement.Fragment) => ({ name: `${name}_${event}`, create: sql`CREATE TRIGGER ${sql(`${name}_${event}`)} AFTER ${sql.literal(verb)} ON ${table} BEGIN ${body} END` });
        return {
            indexes: [],
            virtual: Option.some({
                name,
                create: sql`CREATE VIRTUAL TABLE ${virtual} USING fts5 (${values}, content = ${sql.literal(literal(owner))}, content_rowid = 'rowid', tokenize = ${sql.literal(literal(tokenizer === 'trigram' ? tokenizer : tokenizer.tokenize))})`,
                triggers: [trigger('insert', 'INSERT', inserted), trigger('delete', 'DELETE', deleted), trigger('update', 'UPDATE', sql.join(' ', false)([deleted, inserted]))],
                rebuild: sql`INSERT INTO ${virtual} (${virtual}) VALUES ('rebuild')`,
            }),
        };
    };
    static readonly policy: Engine['Service']['policy'] = () => Option.none();
    static readonly comment: Engine['Service']['comment'] = (sql, _, identity) => ({ inline: sql.literal(` /* ${JSON.stringify(identity).replaceAll('*/', '*\\/')} */`), statements: () => [] });

    // --- [RECONCILE]
    static readonly refusals: Engine['Service']['refusals'] = ({ name: owner, key, columns, indexes, search, tenant, withoutRowid }) => {
        const [all, aliases] = [Object.values(columns), ['rowid', 'oid', '_rowid_']];
        const unkeyed = Option.isSome(search) && (withoutRowid || !all.some(({ name, kind }) => kind === 'integer' && Equal.equals(key, [name])));
        const folded = indexes.flatMap(({ keys }) => keys.filter(Struct.get('casefold')).map(Struct.get('column')));
        return [
            ...unsupported(owner, 'reserved', all, ({ name }) => aliases.includes(name.toLowerCase())),
            ...unsupported(owner, 'tenant', all, ({ name }) => Option.exists(tenant, ({ column }) => column.name === name)),
            ...unsupported(owner, 'search', all, ({ name }) => unkeyed && key.includes(name)),
            ...unsupported(owner, 'casefold', all, ({ name, ascii }) => !ascii && folded.includes(name)),
        ];
    };
    static readonly role: Engine['Service']['role'] = (sql) => Effect.fail(new Cause.IllegalArgumentError(`${sql.driver} has no role to migrate as`));
    static readonly open: Engine['Service']['open'] = (sql) => sql`PRAGMA optimize = 0x10002`;
    static readonly identities = ({ columns, ...table }: typeof SqliteTable.Type) => {
        const tokens: readonly string[] = table.sql.match(lexemes) ?? [];
        const quotes: Readonly<Record<string, string>> = { '"': '"', '`': '`', '[': ']' };
        const opening = tokens.indexOf('(');
        const [, depths] = Array.mapAccum(Array.drop(tokens, opening + 1), 1, (depth, token) => {
            const next = depth + (token === '(' ? 1 : 0) - (token === ')' ? 1 : 0);
            return [next, { token, depth, next }];
        });
        const body = Array.takeWhile(depths, ({ next }) => next > 0);
        const cuts = Array.filterMap(body, ({ token, depth }, index) => (depth === 1 && token === ',' ? Result.succeed(index) : Result.failVoid));
        const segments = Array.window([-1, ...cuts, body.length], 2).map(([start, end]) => body.slice(start + 1, end).map(Struct.get('token')));
        const comment = (token: Option.Option<string>) =>
            Option.map(
                Option.filter(token, (text) => text.startsWith('/*')),
                (text) => text.slice(2, -2).trim(),
            );
        const unquoted = (token: string) => UndefinedOr.match(quotes[token.charAt(0)], { onUndefined: () => token, onDefined: (close) => token.slice(1, -1).replaceAll(`${close}${close}`, close) });
        const defined = Array.filterMap(
            segments,
            Filter.fromPredicateOption((segment) => {
                const metadata = { identity: comment(Array.get(segment, 1)), constrained: Array.drop(segment, 1).some((token) => keywords.includes(token.toUpperCase())) };
                return Array.head(segment).pipe(
                    Option.filter((first) => !keywords.includes(first.toUpperCase())),
                    Option.map((first) => ({ name: unquoted(first), ...metadata })),
                );
            }),
        );
        const definitions = Record.fromIterableBy(defined, Struct.get('name'));
        return {
            ...table,
            identity: Option.getOrNull(comment(Array.findFirst(Array.take(tokens, Math.max(opening, 0)), (token) => token.startsWith('/*')))),
            columns: columns.map((column) => {
                const found = Record.get(definitions, column.name);
                return { ...column, identity: Option.getOrNull(Option.flatMap(found, Struct.get('identity'))), constrained: Option.exists(found, Struct.get('constrained')) };
            }),
        };
    };
    static readonly plan: Engine['Service']['plan'] = Effect.fnUntraced(function* (sql: SqlClient.SqlClient, catalog: Catalog) {
        const listed = yield* sql`SELECT name FROM pragma_table_list WHERE schema = 'main' AND name = 'sqlite_sequence'`;
        const { observed: live } = yield* SqlSchema.findOne({
            Request: Names,
            Result: Observed,
            execute: (prefixes) =>
                sql`WITH relations AS (SELECT l.name, l.type, l.strict, l.wr, m.sql FROM pragma_table_list l JOIN sqlite_schema m ON m.name = l.name WHERE l.schema = 'main' AND l.name NOT GLOB 'sqlite_*' AND NOT EXISTS (SELECT 1 FROM json_each(${prefixes}) p WHERE instr(l.name, p.value) = 1))
                    SELECT json_object(
                        'tables', (SELECT json_group_array(json_object('name', r.name, 'strict', r.strict, 'withoutRowid', r.wr, 'sql', r.sql,
                            'columns', (SELECT json_group_array(json_object('name', x.name, 'type', x.type, 'required', x."notnull", 'default', x.dflt_value, 'hidden', x.hidden)) FROM pragma_table_xinfo(r.name) x),
                            'references', (SELECT json_group_array(json_object('target', f."table", 'onDelete', f.on_delete, 'onUpdate', f.on_update,
                                'columns', (SELECT json_group_array(k."from" ORDER BY k.seq) FROM pragma_foreign_key_list(r.name) k WHERE k.id = f.id),
                                'to', (SELECT json_group_array(k."to" ORDER BY k.seq) FROM pragma_foreign_key_list(r.name) k WHERE k.id = f.id AND k."to" IS NOT NULL))) FROM pragma_foreign_key_list(r.name) f WHERE f.seq = 0),
                            'indexes', (SELECT json_group_array(json_object('name', i.name, 'sql', m.sql, 'unique', i."unique", 'partial', i.partial,
                                'keys', (SELECT json_group_array(json_object('name', k.name, 'collation', k.coll, 'descending', k."desc")) FROM pragma_index_xinfo(i.name) k WHERE k."key" = 1))) FROM pragma_index_list(r.name) i JOIN sqlite_schema m ON m.name = i.name AND m.type = 'index' WHERE i.origin = 'c'),
                            'triggers', (SELECT json_group_array(json_object('name', m.name, 'sql', m.sql)) FROM sqlite_schema m WHERE m.type = 'trigger' AND m.tbl_name = r.name))) FROM relations r WHERE r.type = 'table'),
                        'virtual', (SELECT json_group_array(json_object('name', r.name, 'sql', r.sql)) FROM relations r WHERE r.type = 'virtual'),
                        'sequences', ${Array.isReadonlyArrayNonEmpty(listed) ? sql`(SELECT json_group_array(json_object('name', s.name, 'seq', s.seq)) FROM sqlite_sequence s)` : sql.literal('json_array()')}
                    ) AS observed`,
        })(catalog.excludedPrefixes);
        const rendered = catalog.tables.map((declared) => ({ declared, rendered: render(sql, Sqlite, catalog, declared) }));
        const resolved = diff(sql, catalog, rendered, live.tables.map(Sqlite.identities));
        const populated = yield* resolved.populated;
        return yield* live.#reconcile(sql, catalog, resolved, populated);
    });
    #reconcile(sql: SqlClient.SqlClient, catalog: Catalog, resolved: ReturnType<typeof diff<ReturnType<typeof Sqlite.identities>>>, populated: readonly string[]) {
        const quoted = (name: string) => sql`${sql(name)}`;
        const analyze = (entry: (typeof resolved.matched)[number]) => {
            const dropping = new Set(entry.dropping.map(Struct.get('name')));
            const present = new Set([...entry.fields.map(({ definition }) => definition.name), ...entry.retained.map(Struct.get('name'))]);
            const linked = entry.live.references.map((reference) => {
                const parent = resolved.referenced(reference.target);
                return { ...reference, target: parent.name, columns: Array.map(reference.columns, entry.renamed), to: reference.to.map(parent.renamed) };
            });
            const links = HashSet.fromIterable(linked.map(Struct.pick(['target', 'columns', 'onDelete'])));
            const declaredColumns = HashSet.fromIterable(entry.rendered.links.map(Struct.get('columns')));
            const fields = Array.filterMap(
                entry.fields,
                Filter.fromPredicateOption(({ found, ...field }) => found.pipe(Option.bindTo('found'), Option.map(Struct.assign(field)))),
            );
            const wanted = entry.rendered.indexes.map((index) => ({ ...index, created: index.text(''), prior: identifier([entry.live.name, ...index.parts.map(entry.original)]) }));
            const wantedNames = new Set(wanted.map(Struct.get('name')));
            const indexesByName = Map.groupBy(
                wanted.flatMap((index) => Array.dedupe([index.name, index.prior]).map((name) => ({ name, index }))),
                Struct.get('name'),
            );
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
            const stale = (row: (typeof entry.live.indexes)[number]) => (indexesByName.get(row.name) ?? []).some(({ index }) => (index.name === row.name && !fits(row, index)) || (index.prior === row.name && index.prior !== index.name));
            const dropped = entry.live.indexes.filter((row) => entry.retiredIndexes.includes(row.name) || !new Set(row.keys.map(Struct.get('name'))).isDisjointFrom(dropping) || stale(row));
            const droppedIndexes = new Set(dropped);
            const retainedIndexes = new Set(Array.difference(entry.live.indexes, dropped).map(Struct.get('name')));
            return {
                ...entry,
                linked,
                unstated: linked.filter(({ columns }) => new Set(columns).isSubsetOf(present) && !HashSet.has(declaredColumns, columns)),
                rebuild:
                    entry.owned &&
                    (!entry.live.strict ||
                        entry.live.withoutRowid !== entry.declared.withoutRowid ||
                        (Array.isReadonlyArrayNonEmpty(entry.required) && !populated.includes(entry.live.name)) ||
                        entry.rendered.links.some((link) => !HashSet.has(links, Struct.pick(link, ['target', 'columns', 'onDelete']))) ||
                        linked.some(({ columns }) => !new Set(columns).isDisjointFrom(dropping)) ||
                        (entry.live.name !== entry.declared.name && entry.live.identity === null) ||
                        fields.some(({ definition, found }) => found.name !== definition.name && found.identity === null) ||
                        fields.some(
                            ({ definition, found: { type, required, default: stored }, recorded }) => definition.retype && (type !== definition.type || required === definition.nullable || !Equal.equals(stored, definition.default) || !Option.contains(Option.map(recorded, Struct.get('digest')), definition.digest)),
                        )),
                dropped,
                indexes: wanted.map(({ created }) => created),
                missing: wanted.filter(({ name }) => !retainedIndexes.has(name)).map(Struct.get('created')),
                undeclared: entry.live.indexes.filter((row) => !(droppedIndexes.has(row) || wantedNames.has(row.name))),
            };
        };
        const tables = resolved.matched.map(analyze);
        const positions = new Map(tables.map(({ declared }, index) => [declared.name, index]));
        const edges = tables.flatMap((entry, child) => Array.filterMap(entry.linked, ({ target }) => Result.fromOption(Option.fromUndefinedOr(positions.get(target)), Function.constVoid)).map((parent) => [parent, child] as const));
        const graph = Graph.directed<(typeof tables)[number], void>((mutable) => {
            tables.forEach((entry) => Graph.addNode(mutable, entry));
            edges.forEach(([parent, child]) => Graph.addEdge(mutable, parent, child, undefined));
        });
        const reached = new Set(Graph.values(Graph.dfs(graph, { start: Graph.findNodes(graph, Struct.get('rebuild')) })));
        const rebuilds = tables.filter((entry) => reached.has(entry));
        const rebuildingNames = new Set(rebuilds.map(({ declared }) => declared.name));
        const temporary = (name: string) => quoted(rebuildingNames.has(name) ? `${name}__rebuild` : name);
        const virtualSql = Record.map(Record.fromIterableBy(this.virtual, Struct.get('name')), Struct.get('sql'));
        const searched = (entry: (typeof tables)[number], recreated: boolean) =>
            Option.match(entry.rendered.virtual, {
                onNone: () => ({ fresh: false, dropped: [], created: [] }),
                onSome: (virtual) => {
                    const fresh = !Option.contains(Record.get(virtualSql, virtual.name), virtual.create.compile(true)[0]);
                    const stale = this.virtual.filter(({ name }) => (name === virtual.name && fresh) || (name === `${entry.live.name}_fts` && name !== virtual.name));
                    const triggers = recreated ? [] : entry.live.triggers.filter(({ name }) => name.startsWith(`${entry.live.name}_fts_`) || name.startsWith(`${virtual.name}_`));
                    const expected = new Map(virtual.triggers.map(({ name, create }) => [name, create.compile(true)[0]]));
                    const current = triggers.filter((row) => expected.get(row.name) === row.sql);
                    const currentNames = new Set(current.map(Struct.get('name')));
                    return {
                        fresh,
                        dropped: [...stale.map(({ name }) => sql`DROP TABLE ${sql(name)}`), ...Array.difference(triggers, current).map(({ name }) => sql`DROP TRIGGER ${sql(name)}`)],
                        created: [...(fresh ? [virtual.create] : []), ...virtual.triggers.filter(({ name }) => !currentNames.has(name)).map(Struct.get('create')), ...(fresh ? [virtual.rebuild] : [])],
                    };
                },
            });
        const sequences = new Set(this.sequences.map(Struct.get('name')));
        const sequenced = ({ name }: (typeof tables)[number]['live']) => sequences.has(name);
        const rebuilt = (entry: (typeof tables)[number]) => {
            const kept = entry.retained.filter(({ hidden }) => hidden === 0);
            const copied = sql.csv([...(entry.live.withoutRowid || entry.declared.withoutRowid ? [] : [quoted('rowid')]), ...[...entry.fields.filter(({ definition, found }) => Option.isNone(definition.projection) && Option.isSome(found)).map(({ definition }) => definition), ...kept].map(({ name }) => quoted(name))]);
            const observed = sql.csv([...(entry.live.withoutRowid ? [] : [quoted('rowid')]), ...entry.live.columns.map(({ name }) => quoted(entry.renamed(name)))]);
            const elements = [
                ...kept.map(({ name, type, required: needed, default: stored, identity }) => sql.join(' ', false)([sql`${sql(name)}${sql.literal(identity === null ? '' : ` /* ${identity} */`)}`, type === '' ? 'ANY' : type, ...(needed ? ['NOT NULL'] : []), ...Option.toArray(stored).map((value) => `DEFAULT ${value}`)])),
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
        return resolved.plan({
            findings: rebuilds.flatMap((entry) => [...unsupported(entry.declared.name, 'generated', entry.retained, ({ hidden }) => hidden !== 0), ...unsupported(entry.declared.name, 'retained', entry.retained, ({ hidden, constrained }) => hidden === 0 && constrained)]),
            replaced: rebuilds.map(({ live: row }) => row.name),
            populated: tables.filter(({ owned, required, live: row }) => Array.isReadonlyArrayNonEmpty(required) && (!owned || populated.includes(row.name))).map(({ live: row }) => row.name),
            session: [],
            drifted: ({ live: row }, { definition, found, recorded }) => {
                const digestChanged = Option.exists(Option.flatMapNullishOr(recorded, Struct.get('digest')), (digest) => digest !== definition.digest);
                return row.strict && Option.exists(found, ({ type, required: needed, default: stored }) => type !== definition.type || needed === definition.nullable || !Equal.equals(stored, definition.default) || (Option.isNone(definition.projection) && digestChanged));
            },
            preamble: [],
            opening: Array.isReadonlyArrayNonEmpty(rebuilds) || Array.isReadonlyArrayNonEmpty(resolved.removed) ? [sql`PRAGMA defer_foreign_keys = on`] : [],
            statements: [
                ...this.virtual.filter(({ name }) => resolved.removed.some((owner) => name === `${owner}_fts`)).map(({ name }) => sql`DROP TABLE ${sql(name)}`),
                ...resolved.created.flatMap(({ rendered: table }) => [table.create, ...table.indexes.map(({ text }) => text('')), ...Option.match(table.virtual, { onNone: () => [], onSome: (virtual) => [virtual.create, ...virtual.triggers.map(({ create }) => create)] })]),
                ...altered.flatMap((entry) => [
                    ...entry.dropped.map(({ name }) => sql`DROP INDEX ${sql(name)}`),
                    ...entry.search.dropped,
                    ...entry.dropping.map(({ name }) => sql`ALTER TABLE ${entry.rendered.relation} DROP COLUMN ${sql(name)}`),
                    ...entry.added.map(({ text }) => sql`ALTER TABLE ${entry.rendered.relation} ADD COLUMN ${text}`),
                    ...entry.missing,
                    ...entry.search.created,
                ]),
                ...rebuilding.map(({ create }) => create),
                ...(rebuilding.some(({ live: row }) => sequenced(row)) ? [sql`INSERT INTO sqlite_sequence (name, seq) SELECT m.name, s.seq FROM sqlite_sequence s JOIN sqlite_schema m ON m.name = s.name || '__rebuild'`] : []),
                ...rebuilding.map(({ copy }) => copy),
                ...rebuilding.map(({ rendered: table }) => sql`DROP TABLE ${table.relation}`),
                ...rebuilding.map(({ declared, rendered: table }) => sql`ALTER TABLE ${temporary(declared.name)} RENAME TO ${table.relation}`),
                ...rebuilding.flatMap((entry) => [
                    ...entry.indexes,
                    ...entry.undeclared.map((index) => sql`${sql.literal(index.sql)}`),
                    ...entry.live.triggers.filter(({ name }) => !(name.startsWith(`${entry.live.name}_fts_`) || name.startsWith(`${entry.declared.name}_fts_`))).map((trigger) => sql`${sql.literal(trigger.sql)}`),
                    ...entry.search.dropped,
                    ...entry.search.created,
                ]),
                ...(resolved.created.some(({ rendered: table }) => Array.isReadonlyArrayNonEmpty(table.indexes) || Option.isSome(table.virtual)) ||
                altered.some(({ missing, search }) => Array.isReadonlyArrayNonEmpty(missing) || search.fresh) ||
                rebuilding.some(({ indexes, search }) => Array.isReadonlyArrayNonEmpty(indexes) || search.fresh)
                    ? [sql`PRAGMA optimize`]
                    : []),
            ],
            concurrent: [],
            undeclared: { indexes: tables.flatMap(({ undeclared }) => undeclared.map(({ name }) => name)), references: tables.flatMap(({ declared, unstated }) => unstated.map(({ columns, target }) => ({ table: declared.name, columns, target }))) },
        });
    }
}
const Names = Schema.fromJsonString(Schema.Array(Schema.String));
const Observed = Schema.Struct({ observed: Schema.fromJsonString(Sqlite) });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Sqlite };
