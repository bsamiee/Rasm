import { Array, Cause, Effect, Equal, Filter, Function, flow, Graph, HashSet, Match, Option, Order, Record, Result, Schema, Struct } from 'effect';
import { Digest } from 'effect/crypto';
import { type SqlClient, SqlSchema, type Statement } from 'effect/sql';
import type { Catalog } from '#/catalog';
import { Column } from '#/column';
import type { Engine } from '#/engine';
import { ReconcileError } from '#/error';
import type { Relation } from '#/relation';

// --- [TYPES] ---------------------------------------------------------------------------

type Finding = ReconcileError['findings'][number];
interface Live {
    readonly name: string;
    readonly identity: Option.Option<typeof Identity.Type>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const encoder = new TextEncoder();
const referentialActions = ['NO ACTION', 'RESTRICT', 'CASCADE', 'SET NULL', 'SET DEFAULT'] as const;
const methods = ['btree', 'gin', 'gist', 'brin'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Identity = Schema.Struct({ origin: Schema.String, generation: Schema.Int, digest: Schema.optionalKey(Schema.String.pipe(Schema.catchDecoding(() => Effect.succeedNone))) });
const Stamp = Schema.fromJsonString(Identity);
const Populated = Schema.Struct({ populated: Schema.BooleanFromBit });

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [TEXT]

const literal = (text: string) => `'${text.replaceAll("'", "''")}'`;
const constant = ({ kind }: Column, value: string | number) =>
    Match.value(kind).pipe(
        Match.whenOr('integer', 'real', 'boolean', () => `${value}`),
        Match.orElse(() => literal(`${value}`)),
    );
const renameTable: Engine['Service']['rename'] = (sql, from, to) => sql`ALTER TABLE ${sql(from)} RENAME TO ${to}`;
const unmigrated: Engine['Service']['role'] = (sql) => new Cause.IllegalArgumentError(`${sql.driver} has no role to migrate as`);
const digest = (text: string) =>
    Effect.provide(
        Digest.Digest.use((service) => service.digestHex(Digest.sha256, encoder.encode(text))),
        Digest.layerWebCrypto,
    );
const replacement = (table: string) => `${table}__rebuild`;
const identifier = (parts: readonly string[]) => {
    const bytes = 63;
    const radix = 36;
    const base = 257;
    const modulus = 2_147_483_647;
    const name = parts.join('_');
    const suffix = `_${encoder
        .encode(name)
        .reduce((hash, byte) => (hash * base + byte) % modulus, 0)
        .toString(radix)}`;
    return encoder.encode(name).length <= bytes ? name : `${name.slice(0, encoder.encodeInto(name, new Uint8Array(bytes - suffix.length)).read)}${suffix}`;
};

// --- [REFUSALS]

const findings = (table: string, columns: readonly string[], reason: Finding['reason']): readonly Finding[] => Array.match(columns, { onEmpty: () => [], onNonEmpty: (named) => [{ table, columns: named, reason }] });
const unsupported = <C extends { readonly name: string }>(table: string, refusal: Extract<Finding['reason'], { readonly _tag: 'Unsupported' }>['refusal'], rows: readonly C[], refuses: (row: C) => boolean) => findings(table, rows.filter(refuses).map(Struct.get('name')), { _tag: 'Unsupported', refusal });
const mergeTreeRefusals = ({ name: owner, mergeTree: { expires, version, codecs } }: Relation) => findings(owner, Array.dedupe([...Option.toArray(expires).map(Struct.get('column')), ...Option.toArray(version), ...Object.keys(codecs)]), { _tag: 'Unsupported', refusal: 'mergeTree' });
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

const rendering = Effect.fnUntraced(function* (sql: SqlClient.SqlClient, engine: Omit<Engine['Service'], 'names'>, catalog: Catalog, declared: Relation) {
    const { name: owner, key, references, search, tenant, indexes } = declared;
    const quoted = (name: string) => sql`${sql(name)}`;
    const [entries, relation] = [Object.values(declared.columns), quoted(owner)];
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
    const definitions = yield* Effect.forEach(
        entries,
        Effect.fnUntraced(function* (entry) {
            const [type, defaulted, name] = [engine.type(entry), Option.map(entry.default, (value) => engine.constant(entry, value)), quoted(entry.name)];
            const checked = (subject: Statement.Fragment) =>
                Column.Bound.$match({
                    Compare: ({ op, value, subject: measured }) => spaced([measured === 'codePoints' ? engine.codePoints(sql, subject) : subject, op, value instanceof Date ? literal(value.toISOString()) : `${value}`]),
                    MultipleOf: ({ divisor }) => spaced([subject, '%', `${divisor}`, '= 0']),
                    Cased: ({ case: casing }) => engine.cased(sql, subject, entry.kind, casing),
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
                            ...(entry.kind === 'integer' && !entry.textual ? [sql`${subject} BETWEEN ${sql.literal((-Number.MAX_SAFE_INTEGER).toString())} AND ${sql.literal(Number.MAX_SAFE_INTEGER.toString())}`] : []),
                            ...entry.bounds.map(checked(subject)),
                            ...engine.domain(sql, subject, entry.kind),
                        ],
                        (all) => Option.isNone(entry.projection) && Array.isReadonlyArrayNonEmpty(all),
                    ),
                    flow(conjunction, (condition) => ({ condition, text: spaced(['CHECK', sql`(${condition})`]) })),
                );
            const [checkName, check] = [label([entry.name, 'check']), rule(name)];
            const parts = [type, ...Option.toArray(projection(entry)).map((expression) => engine.generated(sql, expression)), ...(entry.nullable ? [] : ['NOT NULL']), ...Option.toArray(defaulted).map((value) => `DEFAULT ${value}`)];
            const identity = { origin: Option.getOrElse(entry.renamedFrom, () => entry.name), generation: entry.generation };
            const hashed = yield* digest(sql`${spaced([...parts, ...Option.toArray(rule(sql.literal('subject'))).map(Struct.get('text'))])}`.compile(true)[0]);
            const [stamp, written] = [{ ...identity, digest: hashed }, (held: typeof identity) => spaced([sql`${name}${engine.comment(sql, entry.name, { ...held, digest: hashed }).inline}`, ...parts, ...Option.toArray(check).map(({ text }) => engine.constraint(sql, checkName.name, text))])];
            return { ...entry, type, check: Option.map(check, Struct.assign(checkName)), default: defaulted, identity, stamp, digest: hashed, comment: engine.comment(sql, entry.name, stamp), written, text: written(identity) };
        }),
    );
    const linked = (target: (name: string) => Statement.Fragment) =>
        references.map(({ columns, target: parent, key: parentKey, onDelete }) => ({ ...label([...columns, 'fkey']), columns, target: parent, onDelete, text: spaced(['FOREIGN KEY', sql`(${list(columns)})`, 'REFERENCES', target(parent), sql`(${list(parentKey)})`, `ON DELETE ${onDelete}`]) }));
    const built = indexes
        .filter(({ method }) => engine.methods.includes(method))
        .map((index) => {
            const { keys, unique, include, where, method, parts } = index;
            const [{ name }, covering, columns] = [label(parts), engine.covering(sql, unique, include), keys.map((part) => keyed(part, method))];
            const text = (modifiers: readonly string[], on: Statement.Fragment = relation) =>
                sql`${spaced([
                    'CREATE',
                    ...(unique ? ['UNIQUE'] : []),
                    'INDEX',
                    ...modifiers,
                    quoted(name),
                    'ON',
                    on,
                    ...(method === 'btree' ? [] : ['USING', method]),
                    sql.join('', false)(['(', sql.csv([...columns.map(Struct.get('text')), ...covering.keys]), ')']),
                    ...covering.clause,
                    ...(Array.isReadonlyArrayNonEmpty(where) ? [spaced(['WHERE', conjunction(where.map(predicate))])] : []),
                ])}`;
            return { name, parts, extensions: Array.dedupe(columns.flatMap(Struct.get('extensions'))), configuration: Option.none(), text, declared: Option.some(index) };
        });
    const searched = Option.map(search, (searchable) => engine.fulltext(sql, owner, searchable));
    const identity = { origin: Option.getOrElse(declared.renamedFrom, () => owner), generation: Array.countBy(catalog.retired, Equal.equals(owner)) };
    return {
        relation,
        identity,
        definitions,
        links: linked(quoted),
        indexes: [...built, ...Option.toArray(searched).flatMap(({ indexes: found }) => found.map((index) => ({ ...index, text: (modifiers: readonly string[], on: Statement.Fragment = relation) => index.text(modifiers, on), declared: Option.none<Relation['indexes'][number]>() })))],
        virtual: Option.flatMap(searched, Struct.get('virtual')),
        policy: Option.flatMap(tenant, ({ column, setting }) => Option.map(engine.policy(sql, owner, column.name, engine.type(column), setting), (rule) => ({ ...rule, text: (role: Statement.Fragment, on: Statement.Fragment = relation) => rule.text(on, role) }))),
        comments: [...engine.comment(sql, owner, identity).statements(relation, Option.none()), ...definitions.flatMap(({ name, comment }) => comment.statements(relation, Option.some(name)))],
        create: ({
            target = quoted,
            elements = [],
            autoincrement = false,
            inline = Function.constTrue,
            claimed = Option.none(),
        }: {
            readonly target?: (name: string) => Statement.Fragment;
            readonly elements?: readonly Statement.Fragment[];
            readonly autoincrement?: boolean;
            readonly inline?: (link: { readonly name: string }) => boolean;
            readonly claimed?: Option.Option<{ readonly identity: typeof identity; readonly fields: readonly { readonly definition: { readonly name: string }; readonly identity: typeof identity }[] }>;
        } = {}) => {
            const stamps = Record.fromIterableWith(Option.match(claimed, { onNone: () => [], onSome: Struct.get('fields') }), (field) => [field.definition.name, field.identity]);
            const stamp = Option.match(claimed, { onNone: () => identity, onSome: Struct.get('identity') });
            const columns = definitions.map((definition) => definition.written(Option.getOrElse(Record.get(stamps, definition.name), () => definition.identity)));
            const primary = spaced(['PRIMARY KEY', sql.join('', false)(['(', spaced([list(key), ...(autoincrement ? ['AUTOINCREMENT'] : [])]), ')'])]);
            const constraints = linked(target)
                .filter(inline)
                .map(({ name, text }) => engine.constraint(sql, name, text));
            return sql`CREATE TABLE ${target(owner)}${engine.comment(sql, owner, stamp).inline} (${sql.csv([...columns, ...elements, primary, ...constraints])})${engine.suffix(sql, declared)}`;
        },
    };
});
const render = (sql: SqlClient.SqlClient, engine: Omit<Engine['Service'], 'names'>, catalog: Catalog) => Effect.forEach(catalog.tables, (declared) => Effect.map(rendering(sql, engine, catalog, declared), (rendered) => ({ declared, rendered })));

// --- [DIFF]

const diff = <L extends Live & { readonly columns: readonly Live[]; readonly references: readonly { readonly columns: Array.NonEmptyReadonlyArray<string>; readonly target: string }[] }>(
    sql: SqlClient.SqlClient,
    engine: Pick<Engine['Service'], 'rename'>,
    catalog: Catalog,
    rendered: Effect.Success<ReturnType<typeof render>>,
    observed: readonly L[],
) => {
    const claim = <R extends Live, D extends { readonly name: string; readonly renamedFrom: Option.Option<string>; readonly identity: typeof Identity.Type }>(live: readonly R[], retired: readonly string[], declared: readonly D[]) => {
        const generations = Map.groupBy(retired, Function.identity);
        const [standing, retiring] = Array.partition(live, (row) => {
            const { origin, generation } = Option.getOrElse(row.identity, () => ({ origin: row.name, generation: 0 }));
            return generation < (generations.get(row.name)?.length ?? 0) ? Result.fail(row) : Result.succeed({ row, origin, generation, recorded: row.identity });
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
        const found = Iterator.zip([declared, owned, chosen])
            .map(([item, own, choice]) => ({
                item,
                row: Option.map(choice, Struct.get('row')),
                owned: Option.isSome(own),
                identity: Option.match(choice, { onNone: () => ({ origin: item.identity.origin, generation: item.identity.generation }), onSome: ({ origin, generation }) => ({ origin, generation }) }),
                recorded: Option.flatMap(choice, Struct.get('recorded')),
            }))
            .toArray();
        const renamed = Array.filterMap(
            found,
            Filter.fromPredicateOption(({ item, row }) => Option.filter(row, ({ name }) => name !== item.name).pipe(Option.map((current) => ({ row: current, item })))),
        );
        return { found, retiring, renamed, claimed: Array.getSomes(found.map(Struct.get('row'))) };
    };
    const located = claim(
        observed,
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
    const [matched, unordered] = Array.partition(entries, (entry) =>
        Option.match(entry.row, { onNone: () => Result.fail(entry), onSome: (live) => Result.succeed({ ...entry, live, prior: (parts: readonly string[]) => identifier([live.name, ...parts.map(entry.original)]), retiredIndexes: entry.declared.retired.indexes.map((parts) => identifier([live.name, ...parts])) }) }),
    );
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
    const kept = Array.difference(observed, [...located.retiring, ...located.claimed]);
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
                    ...located.renamed.map(({ row, item }) => engine.rename(sql, row.name, item.table.rendered.relation)),
                    ...matched.flatMap(({ live, rendered: { relation }, renames }) => (parts.replaced.includes(live.name) ? [] : renames.map(({ from, to }) => sql`ALTER TABLE ${relation} RENAME COLUMN ${sql(from)} TO ${sql(to)}`))),
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

export { constant, diff, digest, Identity, identifier, literal, mergeTreeRefusals, methods, referentialActions, refusals, renameTable, render, replacement, Stamp, unmigrated, unsupported };
