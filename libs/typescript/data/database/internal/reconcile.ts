import { Array, Effect, Equal, Filter, Function, Graph, HashSet, Match, Option, Order, Result, Schema, String, Struct } from 'effect';
import { type SqlClient, SqlSchema, type Statement } from 'effect/sql';
import type { Catalog } from '#/catalog';
import { Column } from '#/column';
import { ReconcileError } from '#/error';
import type { Engine } from '#/internal/engine';
import type { Relation } from '#/relation';

// --- [TYPES] ---------------------------------------------------------------------------

type Finding = typeof ReconcileError.Finding.Type;
interface Live {
    readonly name: string;
    readonly identity: string | null;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const digestBase = 257;
const digestModulus = 2_147_483_647;
const encoder = new TextEncoder();
const referentialActions = ['NO ACTION', 'RESTRICT', 'CASCADE', 'SET NULL', 'SET DEFAULT'] as const;
const methods = ['btree', 'gin', 'gist', 'brin'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Identity = Schema.Struct({ origin: Schema.String, generation: Schema.Int, digest: Schema.optionalKey(Schema.Int) });
const Populated = Schema.Struct({ populated: Schema.BooleanFromBit });
const stored = Schema.decodeOption(Schema.fromJsonString(Identity));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [TEXT]

const literal = (text: string) => `'${text.replaceAll("'", "''")}'`;
const constant = ({ kind }: Column, value: string | number) =>
    Match.value(kind).pipe(
        Match.whenOr('integer', 'real', 'boolean', () => `${value}`),
        Match.orElse(() => literal(`${value}`)),
    );
const digest = (text: string) => encoder.encode(text).reduce((hash, byte) => (hash * digestBase + byte) % digestModulus, 0);
const identifier = (parts: readonly string[]) => {
    const identifierBytes = 63;
    const name = parts.map(String.camelToSnake).join('_');
    const radix = 36;
    const suffix = `_${digest(name).toString(radix)}`;
    return encoder.encode(name).length <= identifierBytes ? name : `${name.slice(0, encoder.encodeInto(name, new Uint8Array(identifierBytes - suffix.length)).read)}${suffix}`;
};

// --- [REFUSALS]

const findings = (table: string, columns: readonly string[], reason: Finding['reason']): readonly Finding[] => Array.match(columns, { onEmpty: () => [], onNonEmpty: (named) => [{ table, columns: named, reason }] });
const unsupported = <C extends { readonly name: string }>(table: string, refusal: Extract<Finding['reason'], { readonly _tag: 'Unsupported' }>['refusal'], rows: readonly C[], refuses: (row: C) => boolean) => findings(table, rows.filter(refuses).map(Struct.get('name')), { _tag: 'Unsupported', refusal });
const refusals = ({ name: owner, key, references, columns, indexes }: Relation) => {
    const [all, linked] = [Object.values(columns), [...key, ...references.flatMap((reference) => reference.columns)]];
    const conditions = [...indexes.flatMap(({ where }) => where), ...all.flatMap(({ projection }) => Option.toArray(Option.filter(projection, Column.Projection.$is('Where'))).flatMap(Struct.get('where')))];
    const constants = (entry: Column) => [...Option.toArray(entry.default), ...conditions.filter(({ column }) => column === entry.name).flatMap(({ values }) => Option.getOrElse(values, () => []))];
    return [
        ...(['unbounded', 'mixed'] as const).flatMap((refusal) => unsupported(owner, refusal, all, (entry) => Option.contains(entry.refusal, refusal))),
        ...unsupported(owner, 'projected', all, ({ name, projection }) => Option.isSome(projection) && linked.includes(name)),
        ...unsupported(owner, 'invalid', all, (entry) => constants(entry).some((value) => Option.isNone(Schema.decodeOption(entry.schema)(value)))),
    ];
};

// --- [RENDERING]

const render = (
    sql: SqlClient.SqlClient,
    engine: Omit<Engine['Service'], 'search'>,
    catalog: Catalog,
    declared: Relation,
    {
        target = (name) => sql`${sql(name)}`,
        elements = [],
        autoincrement = false,
        identity = Option.none(),
        identities = () => Option.none(),
        inline = Function.constTrue,
    }: {
        readonly target?: (name: string) => Statement.Fragment;
        readonly elements?: readonly Statement.Fragment[];
        readonly autoincrement?: boolean;
        readonly identity?: Option.Option<typeof Identity.Type>;
        readonly identities?: (name: string) => Option.Option<typeof Identity.Type>;
        readonly inline?: (link: { readonly name: string; readonly target: string }) => boolean;
    } = {},
) => {
    const { name: owner, key, references, search, tenant, withoutRowid, indexes } = declared;
    const [entries, relation] = [Object.values(declared.columns), target(owner)];
    const quoted = (name: string) => sql`${sql(name)}`;
    const spaced = sql.join(' ', false);
    const conjunction = sql.join(' AND ', false);
    const list = (columns: readonly string[]) => sql.csv(columns.map(quoted));
    const label = (parts: readonly string[]) => ({ name: identifier([owner, ...parts]), parts });
    const named = (name: string) => Array.findFirst(entries, (entry) => entry.name === name);
    const projection = (entry: Column): Option.Option<Statement.Fragment> => Option.map(entry.projection, Column.Projection.$match({ Path: (projected) => engine.projection(sql, entry, projected), Where: ({ where }) => conjunction(where.map(predicate)) }));
    const operand = (name: string): Statement.Fragment => Option.match(Option.flatMap(named(name), projection), { onNone: () => quoted(name), onSome: (expression) => engine.projected(sql, expression, name) });
    const within = (subject: Statement.Fragment, entry: Column, values: readonly (string | number)[]) => sql`${subject} IN (${sql.literal(values.map((value) => engine.constant(entry, value)).join(', '))})`;
    const predicate = ({ column: name, negated, values }: Relation['indexes'][number]['where'][number]): Statement.Fragment =>
        Option.match(Option.all([values, named(name)]), {
            onNone: () => spaced([operand(name), negated ? 'IS NOT NULL' : 'IS NULL']),
            onSome: negated ? ([members, entry]) => sql`(${within(operand(name), entry, members)}) IS NOT TRUE` : ([members, entry]) => within(operand(name), entry, members),
        });
    const keyed = ({ column: name, casefold }: Relation['indexes'][number]['keys'][number], method: Relation['indexes'][number]['method']) => {
        const opclass = Option.flatMap(named(name), ({ kind }) => Option.flatten(engine.opclass(method, kind)));
        return {
            text: spaced([casefold ? engine.casefold(sql, name) : operand(name), ...Option.toArray(opclass).map(Struct.get('name'))]),
            extensions: Option.match(opclass, { onNone: () => [], onSome: Struct.get('extensions') }),
        };
    };
    const definitions = entries.map((entry) => {
        const [type, defaulted, name] = [engine.types[entry.kind], Option.map(entry.default, (value) => engine.constant(entry, value)), quoted(entry.name)];
        const checked = (subject: Statement.Fragment) =>
            Column.Bound.$match({
                Compare: ({ op, value, subject: measured }) => spaced([measured === 'codePoints' ? engine.codePoints(sql, subject) : subject, op, value instanceof Date ? literal(value.toISOString()) : `${value}`]),
                MultipleOf: ({ divisor }) => spaced([subject, '%', `${divisor}`, '= 0']),
                Cased: ({ case: casing }): Statement.Fragment => {
                    const text = engine.text(sql, subject, entry.kind);
                    return sql`${sql.literal(casing)}(${text}) = ${text}`;
                },
            });
        const rule = (subject: Statement.Fragment) =>
            Option.map(
                Option.liftPredicate(
                    [
                        ...(Array.isReadonlyArrayNonEmpty(entry.literals)
                            ? [
                                  within(
                                      subject,
                                      entry,
                                      entry.literals.map((value) => (typeof value === 'number' ? value : `${value}`)),
                                  ),
                              ]
                            : []),
                        ...(entry.kind === 'integer' && !entry.textual ? [sql`${subject} BETWEEN ${sql.literal(`${-Number.MAX_SAFE_INTEGER}`)} AND ${sql.literal(`${Number.MAX_SAFE_INTEGER}`)}`] : []),
                        ...entry.bounds.map(checked(subject)),
                        ...engine.domain(sql, subject, entry.kind),
                    ],
                    (all) => Option.isNone(entry.projection) && Array.isReadonlyArrayNonEmpty(all),
                ),
                (all) => spaced(['CHECK', sql`(${conjunction(all)})`]),
            );
        const checkName = label([entry.name, 'check']);
        const parts = [type, ...Option.toArray(projection(entry)).map((expression) => engine.generated(sql, expression)), ...(entry.nullable ? [] : ['NOT NULL']), ...Option.toArray(defaulted).map((value) => `DEFAULT ${value}`)];
        const origin = Option.getOrElse(entry.renamedFrom, () => entry.name);
        const stamp = Option.getOrElse(identities(entry.name), () => ({ origin, generation: entry.generation }));
        const hashed = digest(sql`${spaced([...parts, ...Option.toArray(rule(sql.literal('subject')))])}`.compile(true)[0]);
        const comment = engine.comment(sql, entry.name, { origin: stamp.origin, generation: stamp.generation, digest: hashed });
        const check = rule(name);
        return {
            ...entry,
            type,
            check: Option.map(check, (text) => ({ ...checkName, text })),
            default: defaulted,
            identity: stamp,
            digest: hashed,
            comment,
            text: spaced([sql`${name}${comment.inline}`, ...parts, ...Option.toArray(check).map((text) => engine.constraint(sql, checkName.name, text))]),
        };
    });
    const links = references.map(({ columns, table: parent, onDelete }) => ({ ...label([...columns, 'fkey']), columns, target: parent.name, onDelete, text: spaced(['FOREIGN KEY', sql`(${list(columns)})`, 'REFERENCES', target(parent.name), sql`(${list(parent.key)})`, `ON DELETE ${onDelete}`]) }));
    const built = indexes
        .filter(({ method }) => engine.methods.includes(method))
        .map((index) => {
            const { keys, unique, include, where, method, parts } = index;
            const [{ name }, covering, columns] = [label(parts), engine.covering(sql, unique, include), keys.map((part) => keyed(part, method))];
            const text = (prefix: string) =>
                sql`${spaced([
                    sql`CREATE ${sql.literal(unique ? 'UNIQUE ' : '')}INDEX ${sql.literal(prefix)}${sql(name)} ON ${relation}${sql.literal(method === 'btree' ? '' : ` USING ${method}`)} (${sql.csv([...columns.map(Struct.get('text')), ...covering.keys])})`,
                    ...covering.clause,
                    ...(Array.isReadonlyArrayNonEmpty(where) ? [spaced(['WHERE', conjunction(where.map(predicate))])] : []),
                ])}`;
            return { name, parts, extensions: Array.dedupe(columns.flatMap(Struct.get('extensions'))), configuration: Option.none(), text, declared: Option.some(index) };
        });
    const searched = Option.map(search, (found) => engine.fulltext(sql, owner, found));
    const stamp = Option.getOrElse(identity, () => ({ origin: Option.getOrElse(declared.renamedFrom, () => owner), generation: Array.countBy(catalog.retired, Equal.equals(owner)) }));
    const comment = engine.comment(sql, owner, stamp);
    return {
        relation,
        identity: stamp,
        definitions,
        links,
        indexes: [...built, ...Option.toArray(searched).flatMap(({ indexes: found }) => found.map((index) => ({ ...index, text: (prefix: string) => index.text(prefix, relation), declared: Option.none<Relation['indexes'][number]>() })))],
        virtual: Option.flatMap(searched, Struct.get('virtual')),
        policy: Option.flatMap(tenant, ({ column, setting }) => Option.map(engine.policy(sql, owner, column.name, engine.types[column.kind], setting), (rule) => ({ ...rule, text: (role: Statement.Fragment) => rule.text(relation, role) }))),
        create: sql`CREATE TABLE ${relation}${comment.inline} (${sql.csv([...definitions.map(({ text }) => text), ...elements, spaced(['PRIMARY KEY', sql`(${list(key)}${sql.literal(autoincrement ? ' AUTOINCREMENT' : '')})`]), ...links.filter(inline).map(({ name, text }) => engine.constraint(sql, name, text))])})${sql.literal(engine.suffix(withoutRowid))}`,
        comments: [...comment.statements(relation, Option.none()), ...definitions.flatMap(({ name, comment: stamped }) => stamped.statements(relation, Option.some(name)))],
    };
};

// --- [DIFF]

const diff = <L extends Live & { readonly columns: readonly Live[]; readonly references: readonly { readonly columns: Array.NonEmptyReadonlyArray<string>; readonly target: string }[] }>(
    sql: SqlClient.SqlClient,
    catalog: Catalog,
    rendered: readonly { readonly declared: Relation; readonly rendered: ReturnType<typeof render> }[],
    tables: readonly L[],
) => {
    const claim = <R extends Live, D extends { readonly name: string; readonly renamedFrom: Option.Option<string>; readonly identity: typeof Identity.Type }>(live: readonly R[], retired: readonly string[], declared: readonly D[]) => {
        const generations = Map.groupBy(retired, Function.identity);
        const [standing, retiring] = Array.partition(live, (row) => {
            const recorded = Option.flatMap(Option.fromNullOr(row.identity), stored);
            const { origin, generation } = Option.getOrElse(recorded, () => ({ origin: row.name, generation: 0 }));
            return generation < (generations.get(row.name)?.length ?? 0) ? Result.fail(row) : Result.succeed({ row, origin, generation, recorded });
        });
        type Candidate = (typeof standing)[number];
        const take = (claimed: HashSet.HashSet<string>, matches: (candidate: Candidate) => boolean) => {
            const pick = Array.findFirst(standing, (candidate) => !HashSet.has(claimed, candidate.row.name) && matches(candidate));
            return [Option.match(pick, { onNone: () => claimed, onSome: (candidate) => HashSet.add(claimed, candidate.row.name) }), pick] as const;
        };
        const unclaimed = HashSet.empty<string>();
        const [owners, owned] = Array.mapAccum(declared, unclaimed, (claimed, item) => {
            const aliases = new Set([item.name, ...Option.toArray(item.renamedFrom)]);
            return take(claimed, (candidate) => candidate.generation === item.identity.generation && (aliases.has(candidate.row.name) || aliases.has(candidate.origin)));
        });
        const [, chosen] = Array.mapAccum(Array.zip(declared, owned), owners, (claimed, [item, own]) => (Option.isSome(own) ? ([claimed, own] as const) : take(claimed, (candidate) => candidate.row.name === item.name)));
        const found = Array.zip(Array.zip(declared, owned), chosen).map(([[item, own], choice]) => ({
            item,
            row: Option.map(choice, Struct.get('row')),
            owned: Option.isSome(own),
            identity: Option.match(choice, { onNone: () => ({ origin: item.identity.origin, generation: item.identity.generation }), onSome: ({ origin, generation }) => ({ origin, generation }) }),
            recorded: Option.flatMap(choice, Struct.get('recorded')),
        }));
        const renamed = Array.filterMap(
            found,
            Filter.fromPredicateOption(({ item, row }) => Option.filter(row, ({ name }) => name !== item.name).pipe(Option.map((current) => ({ row: current, item })))),
        );
        return { found, retiring, renamed, claimed: Array.getSomes(found.map(Struct.get('row'))) };
    };
    const located = claim(
        tables,
        catalog.retired,
        rendered.map((table) => ({ table, name: table.declared.name, renamedFrom: table.declared.renamedFrom, identity: table.rendered.identity })),
    );
    const entries = located.found.map(({ item: { table }, row, owned, identity, recorded }) => {
        const live = Option.match(row, { onNone: Array.empty<L['columns'][number]>, onSome: ({ columns }): readonly L['columns'][number][] => columns });
        const { found, retiring, claimed, renamed: renamedRows } = claim(live, table.declared.retired.columns, table.rendered.definitions);
        const fields = found.map(({ item: definition, row: present, ...ownership }) => ({ definition, found: present, ...ownership }));
        const renames = renamedRows.map(({ row: current, item }) => ({ from: current.name, to: item.name }));
        const renamed = new Map(renames.map(({ from, to }) => [from, to]));
        const original = new Map(renames.map(({ from, to }) => [to, from]));
        const dropping = owned ? retiring : [];
        const missing = fields.filter(({ found: present }) => Option.isNone(present)).map(({ definition }) => definition);
        const added = Array.sortWith(missing, (definition) => Option.exists(definition.projection, Column.Projection.$is('Where')), Order.Boolean);
        return {
            ...table,
            row,
            owned,
            identity,
            recorded,
            fields,
            dropping,
            retained: Array.difference(live, [...dropping, ...claimed]),
            renames,
            renamed: (name: string) => renamed.get(name) ?? name,
            original: (name: string) => original.get(name) ?? name,
            added,
            required: added.filter((definition) => !definition.nullable && Option.isNone(definition.default) && Option.isNone(definition.projection)),
        };
    });
    const [matched, unordered] = Array.partition(entries, (entry) => Option.match(entry.row, { onNone: () => Result.fail(entry), onSome: (live) => Result.succeed({ ...entry, live, retiredIndexes: entry.declared.retired.indexes.map((parts) => identifier([live.name, ...parts])) }) }));
    const positions = new Map(unordered.map(({ declared }, index) => [declared.name, index]));
    const edges = unordered.flatMap((entry, child) =>
        Array.filterMap(entry.rendered.links, (link) => {
            const parent = positions.get(link.target);
            return parent === undefined || parent === child ? Result.failVoid : Result.succeed({ parent, child, link });
        }),
    );
    const graph = (included: typeof edges) =>
        Graph.directed<(typeof unordered)[number], number>((mutable) => {
            unordered.forEach((entry) => Graph.addNode(mutable, entry));
            included.forEach(({ parent, child }) => Graph.addEdge(mutable, parent, child, child));
        });
    const components = Graph.stronglyConnectedComponents(graph(edges));
    const cyclic = edges.filter(({ parent, child }) => components.some((component) => component.includes(parent) && component.includes(child)));
    const created = Array.fromIterable(Graph.entries(Graph.topo()(graph(Array.difference(edges, cyclic))))).map(([index, entry]) => ({ ...entry, deferred: cyclic.filter(({ child }) => child === index).map(Struct.get('link')) }));
    const kept = Array.difference(tables, [...located.retiring, ...located.claimed]);
    const removed = located.retiring.map(({ name }) => name);
    const occupied = SqlSchema.findOne({ Request: Schema.String, Result: Populated, execute: (name) => sql`SELECT EXISTS (SELECT 1 FROM ${sql(name)}) AS populated` });
    const referenced = new Map(matched.map((entry) => [entry.live.name, entry]));
    return {
        created,
        matched,
        removed,
        kept,
        populated: Effect.filter(
            matched.filter(({ required }) => Array.isReadonlyArrayNonEmpty(required)).map(({ live }) => live.name),
            (name) => Effect.map(occupied(name), Struct.get('populated')),
        ),
        referenced: (live: string) => {
            const entry = referenced.get(live);
            return entry === undefined ? { name: live, renamed: Function.identity<string> } : { name: entry.declared.name, renamed: entry.renamed };
        },
        plan: (
            parts: Record<'preamble' | 'opening' | 'statements' | 'concurrent', readonly Statement.Statement<object>[]> & {
                readonly session: readonly { readonly name: string; readonly value: string }[];
                readonly findings: readonly Finding[];
                readonly replaced: readonly string[];
                readonly populated: readonly string[];
                readonly undeclared: { readonly indexes: readonly string[]; readonly references: readonly { readonly table: string; readonly columns: readonly string[]; readonly target: string }[] };
                readonly drifted: (entry: (typeof matched)[number], field: (typeof matched)[number]['fields'][number]) => boolean;
            },
        ) => {
            const dropped = [...removed, ...parts.replaced];
            const all = [
                ...matched.flatMap(({ live, declared, required }) => findings(declared.name, parts.populated.includes(live.name) ? required.map(Struct.get('name')) : [], { _tag: 'Required' })),
                ...matched.flatMap((entry) => {
                    const moved = entry.fields.filter((field) => entry.owned && field.owned && !field.definition.retype && parts.drifted(entry, field)).map(({ definition }) => definition.name);
                    return findings(entry.declared.name, moved, { _tag: 'Retype' });
                }),
                ...kept.flatMap((row) => row.references.flatMap(({ columns: linked, target }) => findings(row.name, dropped.includes(target) ? linked : [], { _tag: 'RetainedReference', target }))),
                ...matched.flatMap((entry) => {
                    const declaredLinks = HashSet.fromIterable(entry.rendered.links.map(Struct.get('columns')));
                    return entry.live.references
                        .filter(({ columns: linked, target }) => removed.includes(target) && Array.isReadonlyArrayEmpty(Array.intersection(linked, entry.dropping.map(Struct.get('name')))))
                        .map((reference) => ({ ...reference, columns: Array.map(reference.columns, entry.renamed) }))
                        .filter(({ columns: linked }) => !HashSet.has(declaredLinks, linked))
                        .flatMap(({ columns: linked, target }) => findings(entry.declared.name, linked, { _tag: 'RetainedReference', target }));
                }),
                ...parts.findings,
            ];
            const planned = {
                preamble: parts.preamble,
                statements: [
                    ...parts.opening,
                    ...removed.map((name) => sql`DROP TABLE ${sql(name)}`),
                    ...located.renamed.map(({ row, item }) => sql`ALTER TABLE ${sql(row.name)} RENAME TO ${item.table.rendered.relation}`),
                    ...matched.flatMap(({ rendered: { relation }, renames }) => renames.map(({ from, to }) => sql`ALTER TABLE ${relation} RENAME COLUMN ${sql(from)} TO ${sql(to)}`)),
                    ...parts.statements,
                ],
                concurrent: parts.concurrent,
                session: parts.session,
                undeclared: { tables: kept.map(({ name }) => name), columns: matched.flatMap(({ declared, retained }) => retained.map(({ name }) => ({ table: declared.name, column: name }))), ...parts.undeclared },
            };
            return Effect.as(ReconcileError.raise(all), planned);
        },
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { constant, diff, Identity, identifier, literal, methods, referentialActions, refusals, render, unsupported };
