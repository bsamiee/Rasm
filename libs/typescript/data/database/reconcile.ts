import { D1Client } from '@effect/sql-d1';
import { SqliteClient } from '@effect/sql-sqlite-do';
import { Array, Effect, Equal, Function, Match, Option, Order, Record, Schema } from 'effect';
import { SqlClient, SqlSchema } from 'effect/sql';
import { type Catalog, type Column, dialects, identifier, limits, literal, quote, type Table, table } from './catalog.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Dialect = keyof typeof dialects;
type Entry = typeof Entry.Type;
type Finding = ReconcileError['findings'][number];
type OnDelete = Table['table']['references'][number]['onDelete'];
interface Planned {
    readonly statements: readonly string[];
    readonly ledger: Option.Option<{ readonly revision: number; readonly document: typeof Identity.Type }>;
}

// --- [MODELS] --------------------------------------------------------------------------

const Entry = Schema.Struct({ origin: Schema.optionalKey(Schema.String), incarnation: Schema.optionalKey(Schema.Int) });
const Identity = Schema.Record(Schema.String, Schema.Struct({ ...Entry.fields, columns: Schema.optionalKey(Schema.Record(Schema.String, Entry)) }));
const ledger = table('reconcile_revision', { revision: Schema.Int, identity: Schema.fromJsonString(Identity) }, { key: ['revision'], defaults: { identity: '{}' } });
const Stored = Schema.Struct({ revision: Schema.Int, identity: Schema.optionalKey(ledger.fields.identity) });
const Sqlite = Schema.Tuple([
    Schema.Struct({
        catalog: Schema.fromJsonString(
            Schema.Struct({
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
            }),
        ),
    }),
]);
const Pg = Schema.Tuple([
    Schema.Struct({
        catalog: Schema.fromJsonString(
            Schema.Struct({
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
            }),
        ),
    }),
]);
const Prelude = Schema.Tuple([Schema.Struct({ owner: Schema.String, extensions: Schema.Array(Schema.String) })]);

// --- [ERRORS] --------------------------------------------------------------------------

class ReconcileError extends Schema.TaggedError<ReconcileError, { readonly brand: unique symbol }>()('ReconcileError', {
    findings: Schema.NonEmptyArray(Schema.Struct({ reason: Schema.Literals(['Unsupported', 'Required', 'RetainedReference']), table: Schema.String, subject: Schema.String })),
}) {
    override get message(): string {
        return this.findings
            .map(({ reason, table: owner, subject }) => ({ Unsupported: `${owner} declares ${subject}, which this store cannot hold`, Required: `${owner} adds required ${subject} with no default for its rows`, RetainedReference: `${owner} references ${subject}, which this plan replaces` })[reason])
            .join('; ');
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [RENDER]
const ddl = {
    sqlite: {
        constant: ({ kind }: Column, value: string | number) => (['integer', 'int64', 'real', 'number', 'boolean'].includes(kind) ? `${value}` : literal(`${value}`)),
        length: 'length',
        domain: ({ name, kind }: Column) =>
            Match.value(kind).pipe(
                Match.when('boolean', () => [`${quote(name)} IN (0, 1)`]),
                Match.when('json', () => [`json_valid(${quote(name)})`]),
                Match.orElse(() => []),
            ),
        projection: ({ kind }: Column, from: string, path: readonly string[]) => `${quote(from)} ${kind === 'json' ? '->' : '->>'} ${literal(`$${path.map((part) => `.${quote(part)}`).join('')}`)}`,
        generated: (expression: string) => `AS (${expression}) VIRTUAL`,
        constraint: (_: string, text: string) => text,
        action: Option.getOrElse((): string => 'NO ACTION'),
        casefold: (name: string) => `${quote(name)} COLLATE NOCASE`,
        projected: (_: string, name: string) => quote(name),
        methods: ['btree'],
        covering: (unique: boolean, include: readonly string[]) => ({ keys: unique ? [] : include.map(quote), clause: [] }),
        suffix: (withoutRowid: boolean) => (withoutRowid ? ' STRICT, WITHOUT ROWID' : ' STRICT'),
        search: (owner: string, names: readonly string[], tokenizer: string) => {
            const [virtual, values] = [quote(`${owner}_fts`), names.map(quote).join(', ')];
            const row = (prefix: string) => names.map((name) => `${prefix}.${quote(name)}`).join(', ');
            const [inserted, deleted] = [`INSERT INTO ${virtual} ("rowid", ${values}) VALUES (NEW."rowid", ${row('NEW')});`, `INSERT INTO ${virtual} (${virtual}, "rowid", ${values}) VALUES ('delete', OLD."rowid", ${row('OLD')});`];
            const trigger = (event: string, verb: string, body: string) => ({ name: `${owner}_fts_${event}`, sql: `CREATE TRIGGER ${quote(`${owner}_fts_${event}`)} AFTER ${verb} ON ${quote(owner)} BEGIN ${body} END` });
            return {
                indexes: [],
                fts: Option.some({
                    name: `${owner}_fts`,
                    sql: `CREATE VIRTUAL TABLE ${virtual} USING fts5 (${values}, content = ${literal(owner)}, content_rowid = 'rowid', tokenize = ${literal(tokenizer === 'trigram' ? 'trigram' : 'unicode61 remove_diacritics 2')})`,
                    triggers: [trigger('insert', 'INSERT', inserted), trigger('delete', 'DELETE', deleted), trigger('update', 'UPDATE', `${deleted} ${inserted}`)],
                    rebuild: `INSERT INTO ${virtual} (${virtual}) VALUES ('rebuild')`,
                }),
            };
        },
        policy: () => Option.none(),
    },
    pg: {
        constant: ({ kind }: Column, value: string | number) =>
            Match.value(kind).pipe(
                Match.when('boolean', () => `${value === 1}`),
                Match.whenOr('integer', 'int64', 'real', 'number', () => `${value}`),
                Match.orElse(() => literal(`${value}`)),
            ),
        length: 'char_length',
        domain: ({ name, kind }: Column) =>
            Match.value(kind).pipe(
                Match.when('real', () => [`${quote(name)} NOT IN ('NaN', 'Infinity', '-Infinity')`]),
                Match.when('instant', () => [`isfinite(${quote(name)})`]),
                Match.orElse(() => []),
            ),
        projection: ({ kind }: Column, from: string, path: readonly string[]) => `(${quote(from)} ${kind === 'json' ? '#>' : '#>>'} ARRAY[${path.map(literal).join(', ')}])${['json', 'text'].includes(kind) ? '' : `::${dialects.pg.types[kind]}`}`,
        generated: (expression: string) => `GENERATED ALWAYS AS (${expression}) VIRTUAL`,
        constraint: (name: string, text: string) => `CONSTRAINT ${quote(name)} ${text}`,
        action: Option.match({ onNone: () => 'a', onSome: (rule: Option.Option.Value<OnDelete>) => ({ CASCADE: 'c', RESTRICT: 'r', 'SET NULL': 'n', 'SET DEFAULT': 'd' })[rule] }),
        casefold: (name: string) => `casefold(${quote(name)} COLLATE pg_unicode_fast)`,
        projected: (expression: string) => `(${expression})`,
        methods: ['btree', 'gin', 'gist', 'brin'],
        covering: (_: boolean, include: readonly string[]) => ({ keys: [], clause: Array.isReadonlyArrayNonEmpty(include) ? [`INCLUDE (${include.map(quote).join(', ')})`] : [] }),
        suffix: (_: boolean) => '',
        search: (owner: string, names: readonly string[], tokenizer: string) => {
            const [name, document] = [identifier([owner, 'search']), dialects.pg.document(names)];
            return {
                indexes: [{ name, parts: ['search'], trigram: tokenizer === 'trigram', text: (prefix: string, on: string) => `CREATE INDEX ${prefix}${quote(name)} ON ${on} USING gin (${tokenizer === 'trigram' ? `(${document}) gin_trgm_ops` : `to_tsvector('simple', ${document})`})` }],
                fts: Option.none(),
            };
        },
        policy: (owner: string, name: string, type: string) => {
            const [policy, rule] = [identifier([owner, 'tenant']), `${quote(name)} = nullif(current_setting('app.tenant', true), '')::${type}`];
            return Option.some({ name: policy, rule, text: (on: string, role: string) => `CREATE POLICY ${quote(policy)} ON ${on} TO ${role} USING (${rule}) WITH CHECK (${rule})` });
        },
    },
};
const render = (dialect: Dialect, declared: Table, target: (name: string) => string, retained: readonly string[]) => {
    const { name: owner, key, references, search, tenant, withoutRowid, indexes } = declared.table;
    const style = ddl[dialect];
    const { types } = dialects[dialect];
    const columns = Object.values(declared.table.columns);
    const list = (names: readonly string[]) => names.map(quote).join(', ');
    const named = (name: string) => Option.fromUndefinedOr(columns.find((entry) => entry.name === name));
    const projection = (entry: Column) => Option.map(entry.projection, ({ from, path }) => style.projection(entry, from, path));
    const bound = ({ op, value, length }: Column['bounds'][number], name: string) => {
        const subject = length ? `${style.length}(${name})` : name;
        return op === '%' ? `${name} % ${value} = 0` : `${subject} ${op} ${value}`;
    };
    const check = (entry: Column) =>
        Option.map(
            Option.liftPredicate(
                Option.isSome(entry.projection)
                    ? []
                    : [
                          ...(Array.isReadonlyArrayNonEmpty(entry.literals) ? [`${quote(entry.name)} IN (${entry.literals.map((value) => style.constant(entry, typeof value === 'number' ? value : `${value}`)).join(', ')})`] : []),
                          ...(entry.kind === 'integer' ? [`${quote(entry.name)} BETWEEN ${-Number.MAX_SAFE_INTEGER} AND ${Number.MAX_SAFE_INTEGER}`] : []),
                          ...entry.bounds.map((rule) => bound(rule, quote(entry.name))),
                          ...style.domain(entry),
                      ],
                Array.isReadonlyArrayNonEmpty,
            ),
            (all) => ({ name: identifier([owner, entry.name, 'check']), parts: [entry.name, 'check'], text: `CHECK (${all.join(' AND ')})` }),
        );
    const definition = (entry: Column) => {
        const [checked, fallback] = [check(entry), Option.map(entry.fallback, (value) => style.constant(entry, value))];
        return {
            ...entry,
            type: types[entry.kind],
            check: checked,
            fallback,
            text: [quote(entry.name), types[entry.kind], ...Option.toArray(projection(entry)).map(style.generated), ...(entry.nullable ? [] : ['NOT NULL']), ...Option.toArray(fallback).map((text) => `DEFAULT ${text}`), ...Option.toArray(checked).map(({ name, text }) => style.constraint(name, text))].join(' '),
        };
    };
    const link = (reference: (typeof references)[number]) => ({
        name: identifier([owner, ...reference.columns, 'fkey']),
        parts: [...reference.columns, 'fkey'],
        columns: reference.columns,
        target: reference.table.table.name,
        action: style.action(reference.onDelete),
        text: [`FOREIGN KEY (${list(reference.columns)}) REFERENCES ${target(reference.table.table.name)} (${list(reference.table.table.key)})`, ...Option.toArray(reference.onDelete).map((action) => `ON DELETE ${action}`)].join(' '),
    });
    const keyed = ({ column: name, casefold }: (typeof indexes)[number]['keys'][number], method: string) =>
        Match.value({ casefold, projected: Option.flatMap(named(name), projection), method, json: Option.exists(named(name), ({ kind }) => kind === 'json') }).pipe(
            Match.when({ casefold: true }, () => style.casefold(name)),
            Match.when({ projected: Option.isSome }, ({ projected }) => style.projected(projected.value, name)),
            Match.when({ method: 'gin', json: true }, () => `${quote(name)} jsonb_path_ops`),
            Match.when({ method: 'gin' }, () => `${quote(name)} gin_trgm_ops`),
            Match.when({ method: 'gist' }, () => `${quote(name)} gist_trgm_ops`),
            Match.orElse(() => quote(name)),
        );
    const predicate = ({ column: name, predicate: rule }: (typeof indexes)[number]['where'][number]) =>
        Match.value(rule).pipe(
            Match.when(null, () => `${quote(name)} IS NULL`),
            Match.when({ not: null }, () => `${quote(name)} IS NOT NULL`),
            Match.orElse((members) => `${quote(name)} IN (${members.map((member) => (typeof member === 'string' ? literal(member) : `${member}`)).join(', ')})`),
        );
    const index = (entry: (typeof indexes)[number]) => {
        const [name, covering] = [identifier([owner, ...entry.parts]), style.covering(entry.unique, entry.include)];
        return {
            name,
            parts: entry.parts,
            trigram: ['gin', 'gist'].includes(entry.method) && entry.keys.some(({ column: part }) => !Option.exists(named(part), ({ kind }) => kind === 'json')),
            text: (prefix: string, on: string) =>
                [
                    `CREATE ${entry.unique ? 'UNIQUE ' : ''}INDEX ${prefix}${quote(name)} ON ${on}${entry.method === 'btree' ? '' : ` USING ${entry.method}`} (${[...entry.keys.map((part) => keyed(part, entry.method)), ...covering.keys].join(', ')})`,
                    ...covering.clause,
                    ...(Array.isReadonlyArrayNonEmpty(entry.where) ? [`WHERE ${entry.where.map(predicate).join(' AND ')}`] : []),
                ].join(' '),
        };
    };
    const definitions = columns.map(definition);
    const links = references.map(link);
    const searched = Option.map(search, ({ columns: names, tokenizer }) => style.search(owner, names, tokenizer));
    return {
        definitions,
        links,
        indexes: [...indexes.filter(({ method }) => style.methods.includes(method)).map(index), ...Option.match(searched, { onNone: () => [], onSome: (found) => found.indexes })],
        fts: Option.flatMap(searched, ({ fts }) => fts),
        policy: Option.flatMap(Option.flatMap(tenant, named), ({ name, kind }) => style.policy(owner, name, types[kind])),
        create: `CREATE TABLE ${target(owner)} (${[...definitions.map(({ text }) => text), ...retained, `PRIMARY KEY (${list(key)})`, ...links.map(({ name, text }) => style.constraint(name, text))].join(', ')})${style.suffix(withoutRowid)}`,
    };
};

// --- [IDENTITY]
const match = <C extends { readonly name: string }, L extends { readonly name: string }>(catalog: Catalog, stored: Option.Option<typeof Stored.Type>, tables: readonly L[], columns: (live: L) => readonly C[]) => {
    const identity: typeof Identity.Type = Option.match(stored, { onNone: () => ({}), onSome: (row) => row.identity ?? {} });
    const { retired } = catalog;
    const count = (names: readonly string[], name: string) => names.filter((entry) => entry === name).length;
    const sparse = (name: string, origin: string, incarnation: number): Option.Option<Entry> => (origin === name && incarnation === 0 ? Option.none() : Option.some({ ...(origin === name ? {} : { origin }), ...(incarnation === 0 ? {} : { incarnation }) }));
    const declaredIdentity = (name: string, earlier: Option.Option<string>, incarnation: number) =>
        sparse(
            name,
            Option.getOrElse(earlier, () => name),
            incarnation,
        );
    const resolve = <R extends { readonly name: string }>(live: readonly R[], entry: (name: string) => Entry, retire: readonly string[], declared: { readonly name: string; readonly earlier: Option.Option<string>; readonly incarnation: number }) => {
        const aliases = [declared.name, ...Option.toArray(declared.earlier)];
        const rows = live.map((row) => ({ row, origin: entry(row.name).origin ?? row.name, incarnation: entry(row.name).incarnation ?? 0 })).filter(({ row, incarnation }) => incarnation >= count(retire, row.name));
        const own = rows.filter(({ row, origin, incarnation }) => incarnation === declared.incarnation && (aliases.includes(row.name) || aliases.includes(origin)));
        const ordered = Array.sort(
            own,
            Order.mapInput(Order.Boolean, ({ row }: (typeof rows)[number]) => row.name !== declared.name),
        );
        return Option.orElse(
            Option.map(Array.head(ordered), (found) => ({ ...found, owned: true })),
            () =>
                Option.map(
                    Array.findFirst(rows, ({ row }) => row.name === declared.name),
                    (found) => ({ ...found, owned: false }),
                ),
        );
    };
    const located = (declared: Table, { row: live, origin, incarnation, owned }: Option.Option.Value<ReturnType<typeof resolve<L>>>) => {
        const entry = (name: string) => identity[live.name]?.columns?.[name] ?? {};
        const fields = Object.values(declared.table.columns).map((column) => ({ column, found: resolve(columns(live), entry, declared.table.retired.columns, column) }));
        const claimed = fields.flatMap(({ found }) => Option.toArray(found).map(({ row }) => row.name));
        const doomed = columns(live).filter(({ name }) => owned && !claimed.includes(name) && (entry(name).incarnation ?? 0) < count(declared.table.retired.columns, name));
        const retained = columns(live).filter((row) => !(claimed.includes(row.name) || doomed.includes(row)));
        const recorded = (column: Column, found: (typeof fields)[number]['found']) => [column.name, Option.match(found, { onNone: () => declaredIdentity(column.name, column.earlier, column.incarnation), onSome: (row) => sparse(column.name, row.origin, row.incarnation) })] as const;
        return {
            declared,
            live,
            owned,
            fields: fields.map(({ column, found }) => ({ column, found: Option.map(found, ({ row }) => row) })),
            doomed,
            retained,
            renames: fields.flatMap(({ column, found }) => Option.toArray(Option.filter(found, ({ row }) => row.name !== column.name)).map(({ row }) => ({ from: row.name, to: column.name }))),
            added: fields.filter(({ found }) => Option.isNone(found)).map(({ column }) => column),
            own: sparse(declared.table.name, origin, incarnation),
            columns: [...fields.map(({ column, found }) => recorded(column, found)), ...retained.map(({ name }) => [name, Option.fromUndefinedOr(identity[live.name]?.columns?.[name])] as const)],
        };
    };
    const entries = [ledger, ...catalog.tables].map((declared) => {
        const incarnation = count(retired, declared.table.name);
        const state = Option.map(
            resolve(tables, (name) => identity[name] ?? {}, retired, { name: declared.table.name, earlier: declared.table.earlier, incarnation }),
            (found) => located(declared, found),
        );
        const { own, columns: recorded } = Option.getOrElse(state, () => ({
            own: declaredIdentity(declared.table.name, declared.table.earlier, incarnation),
            columns: Object.values(declared.table.columns).map((column) => [column.name, declaredIdentity(column.name, column.earlier, column.incarnation)] as const),
        }));
        const kept = Record.getSomes(Record.fromEntries(recorded));
        return { declared, state, recorded: [declared.table.name, { ...Option.getOrElse(own, () => ({})), ...(Record.isEmptyRecord(kept) ? {} : { columns: kept }) }] as const };
    });
    const matched = entries.flatMap(({ state }) => Option.toArray(state));
    const owner = (name: string) => matched.some(({ live }) => live.name === name);
    const removed = tables.filter(({ name }) => !owner(name) && (identity[name]?.incarnation ?? 0) < count(retired, name));
    const kept = tables.filter((row) => !(owner(row.name) || removed.includes(row) || row.name === ledger.table.name));
    const document: typeof Identity.Type = Record.fromEntries([...entries.map(({ recorded }) => recorded), ...kept.map(({ name }) => [name, identity[name] ?? {}] as const)].filter(([, value]) => Object.keys(value).length > 0));
    const revision = Option.match(stored, { onNone: () => 0, onSome: (row) => row.revision });
    const required = matched.flatMap(({ declared, added }) => added.filter((column) => !column.nullable && Option.isNone(column.fallback) && Option.isNone(column.projection)).map(({ name }): Finding => ({ reason: 'Required', table: declared.table.name, subject: name })));
    return {
        created: entries.filter(({ state }) => Option.isNone(state)).map(({ declared }) => declared),
        matched,
        removed,
        kept,
        changed: !Equal.equals(document, identity),
        ledger: { revision, document },
        findings: [
            ...catalog.tables.flatMap(({ table: facts }) =>
                Object.values(facts.columns)
                    .filter(({ kind }) => kind === 'number')
                    .map(({ name }): Finding => ({ reason: 'Unsupported', table: facts.name, subject: name })),
            ),
            ...required,
        ],
    };
};

// --- [PLAN]
const checked = <A extends { readonly findings: readonly Finding[] }>(planned: A) => (Array.isReadonlyArrayNonEmpty(planned.findings) ? Effect.fail(ReconcileError.make({ findings: planned.findings })) : Effect.succeed(planned));
const latestRevision = (sql: SqlClient.SqlClient, columns: readonly string[]) =>
    Array.match(
        columns.filter((name) => Object.hasOwn(Stored.fields, name)),
        {
            onEmpty: () => Effect.succeedNone,
            onNonEmpty: SqlSchema.findOneOption({ Request: Schema.Array(Schema.String), Result: Stored, execute: (names) => sql`SELECT ${sql.literal(names.map(quote).join(', '))} FROM ${sql(ledger.table.name)} ORDER BY ${sql('revision')} DESC LIMIT 1` }),
        },
    );
const planSqlite = Effect.fnUntraced(function* (sql: SqlClient.SqlClient, catalog: Catalog, cloudflare: boolean) {
    const [{ catalog: observed }] = yield* Effect.flatMap(
        sql`SELECT json_object(
            'tables', (SELECT json_group_array(json_object('name', l.name, 'strict', l.strict, 'withoutRowid', l.wr, 'sql', m.sql,
                'columns', (SELECT json_group_array(json_object('name', x.name, 'type', x.type, 'required', x."notnull", 'fallback', x.dflt_value, 'hidden', x.hidden)) FROM pragma_table_xinfo(l.name) x),
                'references', (SELECT json_group_array(json_object('id', f.id, 'target', f."table", 'from', f."from", 'to', f."to", 'onDelete', f.on_delete)) FROM pragma_foreign_key_list(l.name) f)))
                FROM pragma_table_list l JOIN sqlite_schema m ON m.name = l.name WHERE l.schema = 'main' AND l.type = 'table' AND l.name NOT GLOB 'sqlite_*' AND l.name NOT GLOB '_cf_*'),
            'virtual', (SELECT json_group_array(json_object('name', l.name, 'sql', m.sql)) FROM pragma_table_list l JOIN sqlite_schema m ON m.name = l.name WHERE l.schema = 'main' AND l.type = 'virtual' AND l.name NOT GLOB '_cf_*'),
            'indexes', (SELECT json_group_array(json_object('name', m.name, 'table', m.tbl_name, 'sql', m.sql, 'keys', (SELECT json_group_array(k.name) FROM pragma_index_xinfo(m.name) k WHERE k."key" = 1)))
                FROM sqlite_schema m WHERE m.type = 'index' AND m.sql IS NOT NULL AND m.tbl_name NOT GLOB 'sqlite_*' AND m.tbl_name NOT GLOB '_cf_*'),
            'triggers', (SELECT json_group_array(json_object('name', m.name, 'table', m.tbl_name, 'sql', m.sql)) FROM sqlite_schema m WHERE m.type = 'trigger')
        ) AS catalog`,
        Schema.decodeUnknownEffect(Sqlite),
    );
    const stored = yield* latestRevision(sql, observed.tables.find(({ name }) => name === ledger.table.name)?.columns.map(({ name }) => name) ?? []);
    const resolved = match(catalog, stored, observed.tables, ({ columns }) => columns);
    const final = (declared: Table) => quote(declared.table.name);
    const declaredName = (live: string) => resolved.matched.find((entry) => entry.live.name === live)?.declared.table.name ?? live;
    const analyze = (entry: (typeof resolved.matched)[number]) => {
        const rendered = render('sqlite', entry.declared, quote, []);
        const renamed = (name: string) => entry.renames.find(({ from }) => from === name)?.to ?? name;
        const original = (name: string) => entry.renames.find(({ to }) => to === name)?.from ?? name;
        const dropping = entry.doomed.map(({ name }) => name);
        const groups = Object.values(Array.groupBy(entry.live.references, ({ id }) => `${id}`));
        const linked = groups.map((group) => ({ target: declaredName(group[0].target), columns: group.map(({ from }) => renamed(from)), action: group[0].onDelete }));
        const live = (name: string) => Option.flatMap(Option.fromUndefinedOr(entry.fields.find(({ column }) => column.name === name)), ({ found }) => found);
        const drifted = (definition: (typeof rendered.definitions)[number]) =>
            Option.exists(
                live(definition.name),
                (row) =>
                    row.type !== definition.type ||
                    row.required === definition.nullable ||
                    row.fallback !== Option.getOrNull(definition.fallback) ||
                    (row.hidden === 0) === Option.isSome(definition.projection) ||
                    !Option.match(definition.check, { onNone: () => true, onSome: ({ text }) => entry.live.sql.includes(text) }),
            );
        const indexes = observed.indexes.filter((row) => row.table === entry.live.name);
        const wanted = rendered.indexes.map((index) => ({ name: index.name, text: index.text('', final(entry.declared)), prior: identifier([entry.live.name, ...index.parts.map(original)]) }));
        const retired = entry.declared.table.retired.indexes.map((parts) => identifier([entry.live.name, ...parts]));
        const stale = (row: (typeof indexes)[number]) => wanted.some((declared) => (declared.name === row.name && declared.text !== row.sql) || (declared.prior === row.name && declared.prior !== declared.name));
        const dropped = indexes.filter((row) => retired.includes(row.name) || row.keys.some((key) => key !== null && dropping.includes(key)) || stale(row));
        const unlinked = rendered.links.some((link) => !linked.some(({ target, columns, action }) => target === link.target && action === link.action && Equal.equals(columns, link.columns)));
        return {
            ...entry,
            rendered,
            groups,
            rebuild: entry.owned && (!entry.live.strict || entry.live.withoutRowid !== entry.declared.table.withoutRowid || unlinked || entry.live.references.some(({ from }) => dropping.includes(from)) || rendered.definitions.some((definition) => definition.retype && drifted(definition))),
            dropped,
            indexes: wanted.map(({ text }) => text),
            missing: wanted.filter(({ name }) => !indexes.some((row) => row.name === name && !dropped.includes(row))).map(({ text }) => text),
            undeclared: indexes.filter((row) => !(dropped.includes(row) || wanted.some(({ name }) => name === row.name))),
            triggers: observed.triggers.filter((row) => row.table === entry.live.name),
        };
    };
    const tables = resolved.matched.map(analyze);
    const closure = (members: readonly (typeof tables)[number][]): readonly (typeof tables)[number][] => {
        const parent = (entry: (typeof tables)[number]) => entry.declared.table.references.some((reference) => reference.table !== entry.declared && members.some(({ declared }) => declared.table.name === reference.table.table.name));
        const next = tables.filter((entry) => members.includes(entry) || parent(entry));
        return next.length === members.length ? members : closure(next);
    };
    const rebuilds = closure(tables.filter(({ rebuild }) => rebuild));
    const temporary = (name: string) => (rebuilds.some(({ declared }) => declared.table.name === name) ? quote(`${name}__rebuild`) : quote(name));
    const depth = (declared: Table): number => Math.max(0, ...declared.table.references.filter((reference) => reference.table !== declared).map((reference) => depth(reference.table) + 1));
    const searched = (entry: (typeof tables)[number], recreated: boolean) =>
        Option.match(entry.rendered.fts, {
            onNone: () => [],
            onSome: (fts) => {
                const fresh = observed.virtual.find(({ name }) => name === fts.name)?.sql !== fts.sql;
                const stale = observed.virtual.filter(({ name }) => (name === fts.name && fresh) || (name === `${entry.live.name}_fts` && name !== fts.name));
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
    const retain = ({ name, type, required, fallback }: (typeof tables)[number]['retained'][number]) => [quote(name), type === '' ? 'ANY' : type, ...(required ? ['NOT NULL'] : []), ...(fallback === null ? [] : [`DEFAULT ${fallback}`])].join(' ');
    const relink = (group: (typeof tables)[number]['groups'][number]) => `FOREIGN KEY (${group.map(({ from }) => quote(from)).join(', ')}) REFERENCES ${quote(group[0].target)} (${group.map(({ from, to }) => quote(to ?? from)).join(', ')}) ON DELETE ${group[0].onDelete}`;
    const rebuilt = (entry: (typeof tables)[number]) => {
        const kept = entry.retained.filter(({ hidden }) => hidden === 0).map(({ name }) => name);
        const copied = [...(entry.live.withoutRowid || entry.declared.table.withoutRowid ? [] : ['"rowid"']), ...entry.fields.filter(({ column, found }) => Option.isNone(column.projection) && Option.isSome(found)).map(({ column }) => quote(column.name)), ...kept.map(quote)].join(', ');
        return {
            create: render('sqlite', entry.declared, temporary, [...entry.retained.filter(({ name }) => kept.includes(name)).map(retain), ...entry.groups.filter((group) => group.every(({ from }) => kept.includes(from))).map(relink)]).create,
            copy: `INSERT INTO ${temporary(entry.declared.table.name)} (${copied}) SELECT ${copied} FROM ${final(entry.declared)}`,
            restore: [...entry.indexes, ...entry.undeclared.map((index) => index.sql), ...entry.triggers.filter(({ name }) => !(name.startsWith(`${entry.live.name}_fts_`) || name.startsWith(`${entry.declared.table.name}_fts_`))).map((trigger) => trigger.sql), ...searched(entry, true)],
        };
    };
    const created = resolved.created.map((declared) => ({ declared, ...render('sqlite', declared, quote, []) }));
    const altered = tables.filter((entry) => !rebuilds.includes(entry));
    const altering = (entry: (typeof tables)[number]) => [
        ...entry.dropped.map(({ name }) => `DROP INDEX ${quote(name)}`),
        ...entry.doomed.map(({ name }) => `ALTER TABLE ${final(entry.declared)} DROP COLUMN ${quote(name)}`),
        ...entry.rendered.definitions.filter(({ name }) => entry.added.some((column) => column.name === name)).map(({ text }) => `ALTER TABLE ${final(entry.declared)} ADD COLUMN ${text}`),
        ...entry.missing,
        ...searched(entry, false),
    ];
    const creating = ({ declared, create, indexes, fts }: (typeof created)[number]) => [create, ...indexes.map(({ text }) => text('', final(declared))), ...Option.match(fts, { onNone: () => [], onSome: (found) => [found.sql, ...found.triggers.map((trigger) => trigger.sql)] })];
    const rebuilding = rebuilds.map(rebuilt);
    const statements = [
        ...(Array.isReadonlyArrayNonEmpty(rebuilds) || Array.isReadonlyArrayNonEmpty(resolved.removed) ? ['PRAGMA defer_foreign_keys = on'] : []),
        ...resolved.removed.map(({ name }) => `DROP TABLE ${quote(name)}`),
        ...tables.filter(({ live, declared }) => live.name !== declared.table.name).map(({ live, declared }) => `ALTER TABLE ${quote(live.name)} RENAME TO ${final(declared)}`),
        ...tables.flatMap(({ declared, renames }) => renames.map(({ from, to }) => `ALTER TABLE ${final(declared)} RENAME COLUMN ${quote(from)} TO ${quote(to)}`)),
        ...created.flatMap(creating),
        ...altered.flatMap(altering),
        ...rebuilding.map(({ create }) => create),
        ...rebuilding.map(({ copy }) => copy),
        ...Array.sortWith(rebuilds, ({ declared }) => depth(declared), Order.flip(Order.Number)).map(({ declared }) => `DROP TABLE ${final(declared)}`),
        ...rebuilds.map(({ declared }) => `ALTER TABLE ${temporary(declared.table.name)} RENAME TO ${final(declared)}`),
        ...rebuilding.flatMap(({ restore }) => restore),
    ];
    const indexing = [...created.flatMap(({ indexes, fts }) => [...indexes, ...Option.toArray(fts)]), ...altered.flatMap(({ missing }) => missing), ...rebuilds.flatMap(({ indexes }) => indexes)];
    const replaced = [...rebuilds.map(({ live }) => live.name), ...resolved.removed.map(({ name }) => name)];
    const refused = (facts: Table['table']): readonly Finding[] => [
        ...Option.toArray(facts.tenant).map((subject): Finding => ({ reason: 'Unsupported', table: facts.name, subject })),
        ...(Option.isSome(facts.search) && (facts.withoutRowid || !Object.values(facts.columns).some(({ name, kind }) => kind === 'integer' && Equal.equals(facts.key, [name]))) ? [{ reason: 'Unsupported' as const, table: facts.name, subject: 'search' }] : []),
        ...Object.values(facts.columns)
            .filter(({ name }) => ['rowid', 'oid', '_rowid_'].includes(name.toLowerCase()))
            .map(({ name }): Finding => ({ reason: 'Unsupported', table: facts.name, subject: name })),
        ...(cloudflare && Record.size(facts.columns) > limits.columns ? [{ reason: 'Unsupported' as const, table: facts.name, subject: 'columns' }] : []),
    ];
    const planned = {
        findings: [
            ...resolved.findings,
            ...catalog.tables.flatMap(({ table: facts }) => refused(facts)),
            ...rebuilds.flatMap((entry) => entry.retained.filter(({ hidden }) => hidden !== 0).map(({ name }): Finding => ({ reason: 'Unsupported', table: entry.declared.table.name, subject: name }))),
            ...resolved.kept.flatMap((row) => row.references.filter(({ target }) => replaced.includes(target)).map(({ target }): Finding => ({ reason: 'RetainedReference', table: row.name, subject: target }))),
        ],
        preamble: [],
        statements: [...statements, ...(Array.isReadonlyArrayNonEmpty(indexing) ? ['PRAGMA optimize'] : [])],
        ledger: Option.liftPredicate(resolved.ledger, () => Array.isReadonlyArrayNonEmpty(statements) || resolved.changed),
        concurrent: [],
        undeclared: {
            tables: resolved.kept.map(({ name }) => name),
            columns: tables.flatMap(({ declared, retained }) => retained.map(({ name }) => ({ table: declared.table.name, column: name }))),
            indexes: tables.flatMap(({ undeclared }) => undeclared.map(({ name }) => name)),
        },
    };
    return yield* checked(planned);
});
const planPg = Effect.fnUntraced(function* (sql: SqlClient.SqlClient, catalog: Catalog, runtime: Option.Option<string>, apply: boolean) {
    const preamble = ["SET LOCAL lock_timeout = '5s'", "SET LOCAL transaction_timeout = '2min'"];
    const role = Option.match(runtime, { onNone: () => 'PUBLIC', onSome: quote });
    const block = (body: string) => {
        const tag = `$${'r'.repeat(1 + Math.max(0, ...Array.fromIterable(body.matchAll(/r+/gu)).map(([run]) => run.length)))}$`;
        return `DO ${tag} BEGIN ${body} END ${tag}`;
    };
    const extensions = catalog.tables.some((declared) => render('pg', declared, quote, []).indexes.some(({ trigram }) => trigram)) ? ['pg_trgm'] : [];
    yield* Effect.forEach(preamble, (text) => sql.unsafe(text), { discard: true });
    const [{ owner: migrator, extensions: installed }] = yield* Effect.flatMap(
        sql`SELECT pg_advisory_xact_lock(hashtextextended(current_schema(), 0))::text AS lock, set_config('search_path', quote_ident(current_schema()) || ', pg_temp', true) AS search_path, current_user::text AS owner, array(SELECT extname::text FROM pg_extension) AS extensions`,
        Schema.decodeUnknownEffect(Prelude),
    );
    const absent = extensions.filter((extension) => !installed.includes(extension));
    yield* Effect.forEach(apply ? absent : [], (extension) => sql.unsafe(`CREATE EXTENSION IF NOT EXISTS ${quote(extension)}`), { discard: true });
    const available = apply ? [...installed, ...absent] : installed;
    const scratch = (name: string) => `pg_temp.${quote(name)}`;
    const temporary = (declared: Table) => {
        const rendered = render('pg', declared, scratch, []);
        return [`${rendered.create} ON COMMIT DROP`, ...rendered.indexes.filter(({ trigram }) => !trigram || available.includes('pg_trgm')).map(({ text }) => text('', scratch(declared.table.name))), ...Option.toArray(rendered.policy).map(({ text }) => text(scratch(declared.table.name), role))];
    };
    yield* sql.unsafe(
        block(
            [ledger, ...catalog.tables]
                .flatMap(temporary)
                .map((statement) => `${statement};`)
                .join(' '),
        ),
    );
    const within = 'c.relnamespace IN (current_schema()::regnamespace, pg_my_temp_schema())';
    const flag = `'scratch', c.relnamespace = pg_my_temp_schema()`;
    const [{ catalog: observed }] = yield* Effect.flatMap(
        sql.unsafe(`SELECT json_build_object(
            'tables', coalesce((SELECT json_agg(json_build_object(${flag}, 'name', c.relname, 'rls', c.relrowsecurity, 'force', c.relforcerowsecurity,
                'grants', coalesce((SELECT json_agg(json_build_array(a.grantee::regrole::text, a.privilege_type)) FROM aclexplode(c.relacl) a WHERE a.grantee <> c.relowner), '[]'::json)))
                FROM pg_class c WHERE ${within} AND c.relkind = 'r'), '[]'::json),
            'columns', coalesce((SELECT json_agg(json_build_object(${flag}, 'table', c.relname, 'name', a.attname, 'type', format_type(a.atttypid, a.atttypmod), 'expression', pg_get_expr(d.adbin, d.adrelid), 'generated', a.attgenerated::text,
                'required', EXISTS (SELECT 1 FROM pg_constraint n WHERE n.conrelid = a.attrelid AND n.contype = 'n' AND n.conkey = ARRAY[a.attnum] AND n.convalidated),
                'sources', array(SELECT r.attname::text FROM pg_depend p JOIN pg_attribute r ON (r.attrelid, r.attnum) = (p.refobjid, p.refobjsubid) WHERE p.classid = 'pg_attrdef'::regclass AND p.objid = d.oid AND p.refobjsubid > 0 AND r.attnum <> a.attnum)) ORDER BY a.attnum)
                FROM pg_attribute a JOIN pg_class c ON c.oid = a.attrelid LEFT JOIN pg_attrdef d ON (d.adrelid, d.adnum) = (a.attrelid, a.attnum) WHERE ${within} AND c.relkind = 'r' AND a.attnum > 0 AND NOT a.attisdropped), '[]'::json),
            'constraints', coalesce((SELECT json_agg(json_build_object(${flag}, 'table', c.relname, 'name', n.conname, 'kind', n.contype::text, 'definition', pg_get_constraintdef(n.oid, true), 'valid', n.convalidated,
                'columns', array(SELECT a.attname::text FROM unnest(n.conkey) WITH ORDINALITY k(number, position) JOIN pg_attribute a ON (a.attrelid, a.attnum) = (n.conrelid, k.number) ORDER BY k.position),
                'target', (SELECT t.relname FROM pg_class t WHERE t.oid = n.confrelid), 'onDelete', n.confdeltype::text))
                FROM pg_constraint n JOIN pg_class c ON c.oid = n.conrelid WHERE ${within} AND n.contype IN ('c', 'f', 'n')), '[]'::json),
            'indexes', coalesce((SELECT json_agg(json_build_object(${flag}, 'table', c.relname, 'name', i.relname, 'valid', x.indisvalid, 'unique', x.indisunique, 'method', am.amname, 'width', x.indnkeyatts,
                'keys', (SELECT json_agg(json_build_object('column', x.indkey[k - 1] <> 0, 'text', CASE WHEN x.indkey[k - 1] <> 0 THEN (SELECT a.attname::text FROM pg_attribute a WHERE a.attrelid = x.indrelid AND a.attnum = x.indkey[k - 1]) ELSE pg_get_indexdef(x.indexrelid, k, true) END) ORDER BY k) FROM generate_series(1, x.indnatts) k),
                'classes', array(SELECT o.opcname::text FROM unnest(x.indclass) WITH ORDINALITY u(oid, position) JOIN pg_opclass o ON o.oid = u.oid ORDER BY u.position),
                'predicate', pg_get_expr(x.indpred, x.indrelid, true), 'definition', pg_get_indexdef(x.indexrelid),
                'columns', array(SELECT a.attname::text FROM pg_depend p JOIN pg_attribute a ON (a.attrelid, a.attnum) = (p.refobjid, p.refobjsubid) WHERE p.objid = x.indexrelid AND p.classid = 'pg_class'::regclass AND p.refobjsubid > 0)))
                FROM pg_index x JOIN pg_class i ON i.oid = x.indexrelid JOIN pg_class c ON c.oid = x.indrelid JOIN pg_am am ON am.oid = i.relam
                WHERE ${within} AND NOT EXISTS (SELECT 1 FROM pg_constraint k WHERE k.conindid = x.indexrelid AND k.conrelid = x.indrelid)), '[]'::json),
            'policies', coalesce((SELECT json_agg(json_build_object(${flag}, 'table', c.relname, 'name', p.polname, 'using', pg_get_expr(p.polqual, p.polrelid, true), 'check', pg_get_expr(p.polwithcheck, p.polrelid, true)))
                FROM pg_policy p JOIN pg_class c ON c.oid = p.polrelid WHERE ${within}), '[]'::json)
        )::text AS catalog`),
        Schema.decodeUnknownEffect(Pg),
    );
    const own = <R extends { readonly scratch: boolean; readonly table: string }>(rows: readonly R[], name: string, scratched: boolean) => rows.filter((row) => row.scratch === scratched && row.table === name);
    const stored = yield* latestRevision(
        sql,
        own(observed.columns, ledger.table.name, false).map(({ name }) => name),
    );
    const resolved = match(
        catalog,
        stored,
        observed.tables.filter((row) => !row.scratch),
        ({ name }) => own(observed.columns, name, false),
    );
    const final = (declared: Table) => quote(declared.table.name);
    const privileges = [...Option.toArray(runtime).map((grantee) => ({ grantee, all: ['SELECT', 'INSERT', 'UPDATE', 'DELETE'] })), ...Option.toArray(Option.fromUndefinedOr(catalog.reader)).map((grantee) => ({ grantee, all: ['SELECT'] }))].filter(({ grantee }) => grantee !== migrator);
    const grants = (declared: Table, held: readonly (readonly [string, string])[]) =>
        privileges.flatMap(({ grantee, all }) =>
            Array.match(
                Array.difference(
                    all,
                    held.filter(([holder]) => holder === grantee).map(([, granted]) => granted),
                ),
                { onEmpty: () => [], onNonEmpty: (missing) => [`GRANT ${missing.join(', ')} ON ${final(declared)} TO ${quote(grantee)}`] },
            ),
        );
    const declaredName = (live: string) => resolved.matched.find((entry) => entry.live.name === live)?.declared.table.name ?? live;
    const shape = (row: (typeof observed.indexes)[number], rename: (column: string) => string) => ({ unique: row.unique, method: row.method, width: row.width, classes: row.classes, predicate: row.predicate, keys: row.keys.map(({ column, text }) => (column ? rename(text) : text)) });
    const analyze = (entry: (typeof resolved.matched)[number]) => {
        const { name } = entry.declared.table;
        const owner = final(entry.declared);
        const rendered = render('pg', entry.declared, quote, []);
        const renamed = (column: string) => entry.renames.find(({ from }) => from === column)?.to ?? column;
        const original = (column: string) => entry.renames.find(({ to }) => to === column)?.from ?? column;
        const prior = (parts: readonly string[]) => identifier([entry.live.name, ...parts.map(original)]);
        const desired = { columns: own(observed.columns, name, true), constraints: own(observed.constraints, name, true), indexes: own(observed.indexes, name, true), policies: own(observed.policies, name, true) };
        const live = { columns: own(observed.columns, entry.live.name, false), constraints: own(observed.constraints, entry.live.name, false), indexes: own(observed.indexes, entry.live.name, false), policies: own(observed.policies, entry.live.name, false) };
        const relabel = (target: string, parts: readonly string[]) => Option.filter(Option.fromUndefinedOr(live.constraints.find((row) => row.name === prior(parts))), (row) => row.name !== target && !live.constraints.some((other) => other.name === target));
        const relabels = [...rendered.definitions.flatMap(({ check }) => Option.toArray(check)), ...rendered.links].flatMap(({ name: target, parts }) => Option.toArray(relabel(target, parts)).map((row) => ({ from: row.name, to: target })));
        const constraints = live.constraints.map((row) => ({ ...row, name: relabels.find(({ from }) => from === row.name)?.to ?? row.name, columns: row.columns.map(renamed), target: Option.map(Option.fromNullOr(row.target), declaredName) }));
        const stale = rendered.links.filter((link) => !constraints.some((row) => row.kind === 'f' && row.name === link.name && Equal.equals(row.columns, link.columns) && Option.contains(row.target, link.target) && row.onDelete === link.action));
        const planned = (index: (typeof rendered.indexes)[number]) => {
            const target = Option.fromUndefinedOr(desired.indexes.find((row) => row.name === index.name));
            const fits = (row: (typeof observed.indexes)[number]) => row.valid && Option.match(target, { onNone: () => true, onSome: (wanted) => Equal.equals(shape(row, renamed), shape(wanted, Function.identity)) });
            const present = Option.fromUndefinedOr(live.indexes.find((row) => row.name === index.name));
            const before = Option.filter(Option.fromUndefinedOr(live.indexes.find((row) => row.name === prior(index.parts))), (row) => row.name !== index.name);
            const [keep, rename] = [Option.exists(present, fits), Option.isNone(present) && Option.exists(before, fits)];
            return { index, keep, rename, dropped: [...Option.toArray(present).filter(() => !keep), ...Option.toArray(before).filter(() => !rename)].map((row) => row.name) };
        };
        const plans = rendered.indexes.map(planned);
        const retired = entry.declared.table.retired.indexes.map((parts) => identifier([entry.live.name, ...parts]));
        const policy = (rule: Option.Option.Value<typeof rendered.policy>) => {
            const before = Option.filter(Option.fromUndefinedOr(live.policies.find((row) => row.name === prior(['tenant']))), (row) => row.name !== rule.name && !live.policies.some((other) => other.name === rule.name));
            const present = Option.orElse(Option.fromUndefinedOr(live.policies.find((row) => row.name === rule.name)), () => before);
            const wanted = Option.fromUndefinedOr(desired.policies.find((row) => row.name === rule.name));
            const same = Option.exists(Option.all({ present, wanted }), (pair) => pair.present.using === pair.wanted.using && pair.present.check === pair.wanted.check);
            return [
                ...Option.toArray(before).map((row) => `ALTER POLICY ${quote(row.name)} ON ${owner} RENAME TO ${quote(rule.name)}`),
                ...(Option.isNone(present) ? [rule.text(owner, role)] : []),
                ...(Option.isSome(present) && !same ? [`ALTER POLICY ${quote(rule.name)} ON ${owner} TO ${role} USING (${rule.rule}) WITH CHECK (${rule.rule})`] : []),
            ];
        };
        const pairs = entry.owned ? entry.fields.filter(({ column }) => column.retype).flatMap(({ column, found }) => Option.toArray(Option.all({ wants: Option.fromUndefinedOr(desired.columns.find((row) => row.name === column.name)), now: found })).map((pair) => ({ column, ...pair }))) : [];
        const moved = pairs.filter(({ wants, now }) => now.type !== wants.type);
        const constrained = <C extends { readonly kind: string; readonly columns: readonly string[] }>(rows: readonly C[]) => rows.filter((row) => row.kind === 'c' && pairs.some(({ column }) => Equal.equals(row.columns, [column.name])));
        const [had, needs] = [constrained(constraints), constrained(desired.constraints)];
        const [obsolete, missing] = [had.filter((row) => !needs.some((other) => other.definition === row.definition)), needs.filter((row) => !had.some((other) => other.definition === row.definition))];
        const dependents = live.columns.filter((row) => row.generated !== '' && moved.some(({ now }) => row.sources.includes(now.name)));
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
            ...live.indexes.filter((index) => dependents.some((row) => index.columns.includes(row.name))).map(({ definition }) => definition),
        ];
        const added = rendered.definitions.flatMap((definition) => (entry.added.some((field) => field.name === definition.name) ? Option.toArray(Option.fromUndefinedOr(desired.columns.find((row) => row.name === definition.name))).map((row) => ({ definition, row })) : []));
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
                ...grants(entry.declared, entry.live.grants),
                ...(tenant && !entry.live.rls ? [`ALTER TABLE ${owner} ENABLE ROW LEVEL SECURITY`] : []),
                ...(tenant && !entry.live.force ? [`ALTER TABLE ${owner} FORCE ROW LEVEL SECURITY`] : []),
                ...Option.match(rendered.policy, { onNone: () => [], onSome: policy }),
            ],
            dropped: [...plans.flatMap(({ dropped }) => dropped), ...live.indexes.filter((row) => retired.includes(row.name)).map((row) => row.name)],
            built: plans.filter(({ keep, rename }) => !(keep || rename)).map(({ index }) => index.text('CONCURRENTLY IF NOT EXISTS ', owner)),
            validated: [...constraints.filter((row) => !row.valid).map((row) => row.name), ...additions.map((row) => row.name), ...tightened.map(({ constraint }) => constraint), ...missing.map((row) => row.name), ...stale.map((link) => link.name)],
            undeclared: live.indexes.filter((row) => !(retired.includes(row.name) || plans.some(({ index }) => [index.name, prior(index.parts)].includes(row.name)))).map((row) => row.name),
        };
    };
    const tables = resolved.matched.map(analyze);
    const creating = (declared: Table) => {
        const rendered = render('pg', declared, quote, []);
        return [
            rendered.create,
            ...rendered.indexes.map(({ text }) => text('', final(declared))),
            ...grants(declared, []),
            ...Option.toArray(declared.table.tenant).flatMap(() => [`ALTER TABLE ${final(declared)} ENABLE ROW LEVEL SECURITY`, `ALTER TABLE ${final(declared)} FORCE ROW LEVEL SECURITY`]),
            ...Option.toArray(rendered.policy).map(({ text }) => text(final(declared), role)),
        ];
    };
    const removed = resolved.removed.map(({ name }) => name);
    const kept = resolved.kept.map(({ name }) => name);
    const statements = [
        ...extensions.filter((extension) => !available.includes(extension)).map((extension) => `CREATE EXTENSION IF NOT EXISTS ${quote(extension)}`),
        ...removed.map((name) => `DROP TABLE ${quote(name)}`),
        ...tables.filter(({ live, declared }) => live.name !== declared.table.name).map(({ live, declared }) => `ALTER TABLE ${quote(live.name)} RENAME TO ${final(declared)}`),
        ...tables.flatMap(({ declared, renames }) => renames.map(({ from, to }) => `ALTER TABLE ${final(declared)} RENAME COLUMN ${quote(from)} TO ${quote(to)}`)),
        ...tables.flatMap(({ relabels }) => relabels),
        ...resolved.created.flatMap(creating),
        ...tables.flatMap((entry) => entry.statements),
    ];
    const planned = {
        findings: [
            ...resolved.findings,
            ...observed.constraints.flatMap((row) => Option.toArray(Option.filter(Option.fromNullOr(row.target), (target) => !row.scratch && row.kind === 'f' && kept.includes(row.table) && removed.includes(target))).map((subject): Finding => ({ reason: 'RetainedReference', table: row.table, subject }))),
        ],
        preamble,
        statements,
        ledger: Option.liftPredicate(resolved.ledger, () => Array.isReadonlyArrayNonEmpty(statements) || resolved.changed),
        concurrent: [
            ...tables.flatMap(({ dropped }) => dropped.map((index) => `DROP INDEX CONCURRENTLY IF EXISTS ${quote(index)}`)),
            ...tables.flatMap(({ built }) => built),
            ...tables.flatMap(({ declared, validated }) => validated.map((constraint) => `ALTER TABLE ${final(declared)} VALIDATE CONSTRAINT ${quote(constraint)}`)),
        ],
        undeclared: {
            tables: kept,
            columns: tables.flatMap(({ declared, retained }) => retained.map((column) => ({ table: declared.table.name, column: column.name }))),
            indexes: tables.flatMap(({ undeclared }) => undeclared),
        },
    };
    return yield* checked(planned);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const environment = Effect.gen(function* () {
    const d1 = yield* Effect.serviceOption(D1Client.D1Client);
    return { sql: (yield* SqlClient.SqlClient).withoutTransforms(), d1, cloudflare: Option.isSome(d1) || Option.isSome(yield* Effect.serviceOption(SqliteClient.SqliteClient)) };
});
const plan = Effect.fn('plan')(function* (catalog: Catalog, runtime: Option.Option<string>) {
    const { sql, cloudflare } = yield* environment;
    const { preamble, statements, concurrent, undeclared } = yield* sql.onDialectOrElse({
        sqlite: () => planSqlite(sql, catalog, cloudflare),
        pg: () => sql.withTransaction(planPg(sql, catalog, runtime, false)),
        orElse: () => Effect.die('plan runs on SQLite and PostgreSQL clients'),
    });
    return { preamble, statements, concurrent, undeclared };
});
const reconcile = Effect.fn('reconcile')(function* (catalog: Catalog, runtime: Option.Option<string>) {
    const { sql, d1, cloudflare } = yield* environment;
    const written = ({ statements, ledger: write }: Planned) =>
        Effect.map(
            Effect.forEach(Option.toArray(write), ({ revision, document }) =>
                Effect.map(Schema.encodeEffect(ledger.fields.identity)(document), (identity) => [sql`INSERT INTO ${sql(ledger.table.name)} ${sql.insert({ revision: revision + 1, identity })}`, sql`DELETE FROM ${sql(ledger.table.name)} WHERE ${sql('revision')} <= ${revision}`]),
            ),
            (rows) => [...statements.map((text) => sql.unsafe(text)), ...rows.flat()],
        );
    const run = (planned: Planned) => Effect.flatMap(written(planned), (all) => Effect.all(all, { discard: true }));
    const batch = Effect.fn('batch')(function* (client: D1Client.D1Client) {
        const planned = yield* planSqlite(sql, catalog, cloudflare);
        const all = yield* written(planned);
        yield* Array.isReadonlyArrayNonEmpty(all) ? client.batch(all) : Effect.void;
        return planned;
    });
    const applied = yield* sql.onDialectOrElse({
        sqlite: () =>
            Option.match(d1, {
                onNone: () => sql.withTransaction(Effect.tap(planSqlite(sql, catalog, cloudflare), run)),
                onSome: (client) =>
                    Effect.catchTag(batch(client), 'SqlError', (error) =>
                        Effect.filterOrFail(
                            planSqlite(sql, catalog, cloudflare),
                            ({ ledger: write }) => Option.isNone(write),
                            () => error,
                        ),
                    ),
            }),
        pg: () => sql.withTransaction(Effect.tap(planPg(sql, catalog, runtime, true), run)),
        orElse: () => Effect.die('reconcile runs on SQLite and PostgreSQL clients'),
    });
    yield* Effect.forEach(applied.concurrent, (text) => sql.unsafe(text), { discard: true });
    return applied.undeclared;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { plan, ReconcileError, reconcile };
