import { Array, Function, Hash, Match, Option, Struct } from 'effect';
import { actions, type Column, document, literal, names, quote, type Relation, types } from './table.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [NAMES]
const constant = ({ kind }: Column, value: string | number) =>
    Match.value(kind).pipe(
        Match.whenOr('integer', 'int64', 'real', 'number', 'boolean', () => `${value}`),
        Match.orElse(() => literal(`${value}`)),
    );
const identifier = (parts: readonly string[]) => {
    const name = parts.map(names.transformQueryNames).join('_');
    const limit = 63;
    const radix = 36;
    const digest = Math.abs(Hash.string(name)).toString(radix);
    const encoder = new TextEncoder();
    return encoder.encode(name).length <= limit ? name : `${name.slice(0, encoder.encodeInto(name, new Uint8Array(limit - digest.length - 1)).read)}_${digest}`;
};

// --- [DIALECTS]
const dialects = {
    sqlite: {
        constant,
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
        action: Function.identity<keyof typeof actions>,
        casefold: (name: string) => `${quote(name)} COLLATE NOCASE`,
        projected: (_: string, name: string) => quote(name),
        methods: ['btree'],
        covering: (unique: boolean, include: readonly string[]) => ({ keys: unique ? [] : include.map(quote), clause: [] }),
        suffix: (withoutRowid: boolean) => (withoutRowid ? ' STRICT, WITHOUT ROWID' : ' STRICT'),
        search: (owner: string, columns: readonly string[], trigram: boolean) => {
            const [virtual, values] = [quote(`${owner}_fts`), columns.map(quote).join(', ')];
            const row = (prefix: string) => columns.map((name) => `${prefix}.${quote(name)}`).join(', ');
            const [inserted, deleted] = [`INSERT INTO ${virtual} ("rowid", ${values}) VALUES (NEW."rowid", ${row('NEW')});`, `INSERT INTO ${virtual} (${virtual}, "rowid", ${values}) VALUES ('delete', OLD."rowid", ${row('OLD')});`];
            const trigger = (event: string, verb: string, body: string) => ({ name: `${owner}_fts_${event}`, sql: `CREATE TRIGGER ${quote(`${owner}_fts_${event}`)} AFTER ${verb} ON ${quote(owner)} BEGIN ${body} END` });
            return {
                indexes: [],
                fts: Option.some({
                    name: `${owner}_fts`,
                    sql: `CREATE VIRTUAL TABLE ${virtual} USING fts5 (${values}, content = ${literal(owner)}, content_rowid = 'rowid', tokenize = ${literal(trigram ? 'trigram' : 'unicode61 remove_diacritics 2')})`,
                    triggers: [trigger('insert', 'INSERT', inserted), trigger('delete', 'DELETE', deleted), trigger('update', 'UPDATE', `${deleted} ${inserted}`)],
                    rebuild: `INSERT INTO ${virtual} (${virtual}) VALUES ('rebuild')`,
                }),
            };
        },
        policy: () => Option.none(),
    },
    pg: {
        constant: (entry: Column, value: string | number) => (entry.kind === 'boolean' ? `${value === 1}` : constant(entry, value)),
        length: 'char_length',
        domain: ({ name, kind }: Column) =>
            Match.value(kind).pipe(
                Match.whenOr('real', 'decimal', () => [`${quote(name)} NOT IN ('NaN', 'Infinity', '-Infinity')`]),
                Match.when('instant', () => [`isfinite(${quote(name)})`]),
                Match.orElse(() => []),
            ),
        projection: ({ kind }: Column, from: string, path: readonly string[]) => `(${quote(from)} ${kind === 'json' ? '#>' : '#>>'} ARRAY[${path.map(literal).join(', ')}])${['json', 'text'].includes(kind) ? '' : `::${types.pg[kind]}`}`,
        generated: (expression: string) => `GENERATED ALWAYS AS (${expression}) VIRTUAL`,
        constraint: (name: string, text: string) => `CONSTRAINT ${quote(name)} ${text}`,
        action: (rule: keyof typeof actions) => actions[rule],
        casefold: (name: string) => `casefold(${quote(name)} COLLATE pg_unicode_fast)`,
        projected: (expression: string) => `(${expression})`,
        methods: ['btree', 'gin', 'gist', 'brin'],
        covering: (_: boolean, include: readonly string[]) => ({ keys: [], clause: Array.isReadonlyArrayNonEmpty(include) ? [`INCLUDE (${include.map(quote).join(', ')})`] : [] }),
        suffix: (_: boolean) => '',
        search: (owner: string, columns: readonly string[], trigram: boolean) => {
            const [name, body] = [identifier([owner, 'search']), document(columns)];
            return {
                indexes: [{ name, parts: ['search'], trigram, text: (prefix: string, on: string) => `CREATE INDEX ${prefix}${quote(name)} ON ${on} USING gin (${trigram ? `(${body}) gin_trgm_ops` : `to_tsvector('simple', ${body})`})` }],
                fts: Option.none(),
            };
        },
        policy: (owner: string, name: string, type: string) => {
            const [policy, rule] = [identifier([owner, 'tenant']), `${quote(name)} = nullif(current_setting('app.tenant', true), '')::${type}`];
            return Option.some({ name: policy, rule, text: (on: string, role: string) => `CREATE POLICY ${quote(policy)} ON ${on} TO ${role} USING (${rule}) WITH CHECK (${rule})` });
        },
    },
};

// --- [RENDER]
const render = (dialect: keyof typeof dialects, declared: Relation, target: (name: string) => string, retained: { readonly elements: readonly string[]; readonly autoincrement: boolean }) => {
    const { name: owner, key, references, search, tenant, withoutRowid, indexes } = declared.table;
    const [style, entries, relation] = [dialects[dialect], Object.values(declared.table.columns), target(owner)];
    const list = (columns: readonly string[]) => columns.map(quote).join(', ');
    const label = (parts: readonly string[]) => ({ name: identifier([owner, ...parts]), parts });
    const named = (name: string) => Array.findFirst(entries, (entry) => entry.name === name);
    const path = (entry: Column) => Option.flatMap(entry.projection, (projected) => (projected._tag === 'Path' ? Option.some(style.projection(entry, projected.from, projected.path)) : Option.none()));
    const operand = (name: string) => Option.match(Option.flatMap(named(name), path), { onNone: () => quote(name), onSome: (expression) => style.projected(expression, name) });
    const predicate = ({ column: name, predicate: rule }: (typeof indexes)[number]['where'][number]) => {
        const members = (values: readonly (string | number)[]) => `${operand(name)} IN (${values.flatMap((member) => Option.toArray(Option.map(named(name), (entry) => style.constant(entry, member)))).join(', ')})`;
        return Match.value(rule).pipe(
            Match.when(null, () => `${operand(name)} IS NULL`),
            Match.when({ not: null }, () => `${operand(name)} IS NOT NULL`),
            Match.when({ not: Match.defined }, ({ not }) => `(${members(not)}) IS NOT TRUE`),
            Match.orElse(members),
        );
    };
    const projection = (entry: Column) => Option.map(entry.projection, (projected) => (projected._tag === 'Path' ? style.projection(entry, projected.from, projected.path) : projected.where.map(predicate).join(' AND ')));
    const keyed = ({ column: name, casefold }: (typeof indexes)[number]['keys'][number], method: string) => {
        const found = named(name);
        const opclass = Match.value({ method, json: Option.exists(found, ({ kind }) => kind === 'json') }).pipe(
            Match.when({ method: 'gin', json: true }, () => 'jsonb_path_ops'),
            Match.when({ method: 'gin' }, () => 'gin_trgm_ops'),
            Match.when({ method: 'gist' }, () => 'gist_trgm_ops'),
            Match.option,
        );
        return [casefold ? style.casefold(name) : Option.match(Option.flatMap(found, projection), { onNone: () => quote(name), onSome: (expression) => style.projected(expression, name) }), ...Option.toArray(opclass)].join(' ');
    };
    const definitions = entries.map((entry) => {
        const [subject, type, fallback] = [quote(entry.name), types[dialect][entry.kind], Option.map(entry.fallback, (value) => style.constant(entry, value))];
        const conditions = [
            ...(Array.isReadonlyArrayNonEmpty(entry.literals) ? [`${subject} IN (${entry.literals.map((value) => style.constant(entry, typeof value === 'number' ? value : `${value}`)).join(', ')})`] : []),
            ...(entry.kind === 'integer' ? [`${subject} BETWEEN ${-Number.MAX_SAFE_INTEGER} AND ${Number.MAX_SAFE_INTEGER}`] : []),
            ...entry.bounds.map(({ op, value, length }) => `${length ? `${style.length}(${subject})` : subject} ${op} ${value}${op === '%' ? ' = 0' : ''}`),
            ...style.domain(entry),
        ];
        const check = Option.liftPredicate(conditions, (all) => Option.isNone(entry.projection) && Array.isReadonlyArrayNonEmpty(all)).pipe(Option.map((all) => ({ ...label([entry.name, 'check']), text: `CHECK (${all.join(' AND ')})` })));
        const text = [subject, type, ...Option.toArray(projection(entry)).map(style.generated), ...(entry.nullable ? [] : ['NOT NULL']), ...Option.toArray(fallback).map((value) => `DEFAULT ${value}`), ...Option.toArray(check).map((rule) => style.constraint(rule.name, rule.text))].join(' ');
        return { ...entry, type, check, fallback, text };
    });
    const links = references.map(({ columns, table: { table: parent }, onDelete }) => ({ ...label([...columns, 'fkey']), columns, target: parent.name, action: style.action(onDelete), text: `FOREIGN KEY (${list(columns)}) REFERENCES ${target(parent.name)} (${list(parent.key)}) ON DELETE ${onDelete}` }));
    const built = indexes
        .filter(({ method }) => style.methods.includes(method))
        .map(({ keys, unique, include, where, method, parts }) => {
            const [{ name }, covering] = [label(parts), style.covering(unique, include)];
            const text = (prefix: string) =>
                [
                    `CREATE ${unique ? 'UNIQUE ' : ''}INDEX ${prefix}${quote(name)} ON ${relation}${method === 'btree' ? '' : ` USING ${method}`} (${[...keys.map((part) => keyed(part, method)), ...covering.keys].join(', ')})`,
                    ...covering.clause,
                    ...(Array.isReadonlyArrayNonEmpty(where) ? [`WHERE ${where.map(predicate).join(' AND ')}`] : []),
                ].join(' ');
            return { name, parts, trigram: ['gin', 'gist'].includes(method) && keys.some(({ column: part }) => !Option.exists(named(part), ({ kind }) => kind === 'json')), text };
        });
    const searched = Option.map(search, ({ columns, trigram }) => style.search(owner, Array.map(columns, Struct.get('name')), trigram));
    return {
        relation,
        definitions,
        links,
        indexes: [...built, ...Option.toArray(searched).flatMap(({ indexes: found }) => found.map((index) => ({ ...index, text: (prefix: string) => index.text(prefix, relation) })))],
        fts: Option.flatMap(searched, ({ fts }) => fts),
        policy: Option.flatMap(Option.flatMap(tenant, named), ({ name, kind }) => Option.map(style.policy(owner, name, types[dialect][kind]), (rule) => ({ ...rule, text: (role: string) => rule.text(relation, role) }))),
        create: `CREATE TABLE ${relation} (${[...definitions.map(({ text }) => text), ...retained.elements, `PRIMARY KEY (${list(key)}${retained.autoincrement ? ' AUTOINCREMENT' : ''})`, ...links.map(({ name, text }) => style.constraint(name, text))].join(', ')})${style.suffix(withoutRowid)}`,
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { identifier, render };
