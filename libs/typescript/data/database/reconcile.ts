import { Array, Effect, Equal, Function, Graph, Match, Option, Order, Result, Schema, String, Struct } from 'effect';
import { type SqlClient, SqlSchema, type Statement } from 'effect/sql';
import type { Catalog } from '#/catalog';
import { Column } from '#/column';
import type { Engine } from '#/engine';
import { ReconcileError } from '#/finding';
import type { Relation } from '#/relation';

// --- [TYPES] ---------------------------------------------------------------------------

type Finding = typeof ReconcileError.Finding.Type;
type Refusal = Extract<Finding['reason'], { readonly _tag: 'Unsupported' }>['refusal'];
type Method = (typeof methods)[number];
interface Live {
    readonly name: string;
    readonly identity: string | null;
}
interface Reference {
    readonly columns: Array.NonEmptyReadonlyArray<string>;
    readonly target: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const identifierBytes = 63;
const digestBase = 257;
const digestModulus = 2_147_483_647;
const methods = ['btree', 'gin', 'gist', 'brin'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Identity = Schema.Struct({ origin: Schema.String, generation: Schema.Int, digest: Schema.optionalKey(Schema.Int) });
const stored = Schema.decodeOption(Schema.fromJsonString(Identity));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [TEXT]
const literal = (text: string) => `'${text.replaceAll("'", "''")}'`;
const digest = (text: string) => new TextEncoder().encode(text).reduce((hash, byte) => (hash * digestBase + byte) % digestModulus, 0);
const identifier = (parts: readonly string[]) => {
    const name = parts.map(String.camelToSnake).join('_');
    const radix = 36;
    const suffix = `_${digest(name).toString(radix)}`;
    const encoder = new TextEncoder();
    return encoder.encode(name).length <= identifierBytes ? name : `${name.slice(0, encoder.encodeInto(name, new Uint8Array(identifierBytes - suffix.length)).read)}${suffix}`;
};

// --- [REFUSALS]
const finding = (table: string, columns: readonly string[], reason: Finding['reason']): Finding => ({ table, columns, reason });
const unsupported = <C extends { readonly name: string }>(owner: string, refusal: Refusal, rows: readonly C[], refuses: (row: C) => boolean) =>
    Array.match(
        rows.filter(refuses).map(({ name }) => name),
        { onEmpty: (): readonly Finding[] => [], onNonEmpty: (columns) => [finding(owner, columns, { _tag: 'Unsupported', refusal })] },
    );
const declaredRefusals = ({ name: owner, key, references, columns, indexes, search }: Relation) => {
    const [all, linked] = [Object.values(columns), [...key, ...references.flatMap((reference) => reference.columns)]];
    const conditions = [...indexes.flatMap(({ where }) => where), ...all.flatMap(({ projection }) => Option.toArray(projection).flatMap((projected) => (projected._tag === 'Where' ? projected.where : [])))];
    const constants = (entry: Column) => [
        ...Option.toArray(entry.default),
        ...conditions
            .filter(({ column }) => column === entry.name)
            .flatMap(({ predicate }) =>
                Match.value(predicate).pipe(
                    Match.when(null, () => []),
                    Match.when({ not: null }, () => []),
                    Match.when({ not: Match.defined }, ({ not }) => not),
                    Match.orElse(Function.identity),
                ),
            ),
    ];
    const names = [...indexes.map(({ parts }) => identifier([owner, ...parts])), ...Option.toArray(search).map(() => identifier([owner, 'search']))];
    return [
        ...(['unbounded', 'mixed'] as const).flatMap((refusal) => unsupported(owner, refusal, all, (entry) => Option.contains(entry.refusal, refusal))),
        ...unsupported(owner, 'projected', all, ({ name, projection }) => Option.isSome(projection) && linked.includes(name)),
        ...unsupported(owner, 'invalid', all, (entry) => constants(entry).some((value) => Option.isNone(Schema.decodeUnknownOption(entry.schema)(value)))),
        ...unsupported(
            owner,
            'collision',
            Array.dedupe(names.filter((name, index) => names.indexOf(name) !== index)).map((name) => ({ name })),
            Function.constTrue,
        ),
    ];
};
const refusals = ({ tables }: Catalog) => {
    const names = tables.map(({ name }) => name);
    const kinds = (relation: Relation, parts: readonly string[]) =>
        parts.map((part) =>
            Option.map(
                Array.findFirst(Object.values(relation.columns), ({ name }) => name === part),
                Struct.get('kind'),
            ),
        );
    const references = (relation: Relation) =>
        relation.references.flatMap(({ columns, table }) =>
            Option.match(
                Array.findFirst(tables, ({ name }) => name === table.name),
                {
                    onNone: () => [finding(relation.name, columns, { _tag: 'Unsupported', refusal: 'dangling' })],
                    onSome: (parent) => (Equal.equals(kinds(relation, columns), kinds(parent, parent.key)) ? [] : [finding(relation.name, columns, { _tag: 'Unsupported', refusal: 'mismatched' })]),
                },
            ),
        );
    return [
        ...tables.flatMap(declaredRefusals),
        ...tables.flatMap(references),
        ...Array.dedupe(tables.filter((relation, index) => names.indexOf(relation.name) !== index || Option.exists(relation.renamedFrom, (from) => from !== relation.name && names.includes(from))).map(({ name }) => name)).map((name) => finding(name, [], { _tag: 'Unsupported', refusal: 'duplicate' })),
    ];
};

// --- [RENDER]
const render = (
    sql: SqlClient.SqlClient,
    engine: Engine['Service'],
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
    const projection = (entry: Column): Option.Option<Statement.Fragment> => Option.map(entry.projection, (projected) => (projected._tag === 'Path' ? engine.projection(sql, entry, projected) : conjunction(projected.where.map(predicate))));
    const operand = (name: string): Statement.Fragment => Option.match(Option.flatMap(named(name), projection), { onNone: () => quoted(name), onSome: (expression) => engine.projected(sql, expression, name) });
    const predicate = ({ column: name, predicate: rule }: Relation['indexes'][number]['where'][number]): Statement.Fragment => {
        const members = (values: readonly (string | number)[]) => spaced([operand(name), `IN (${Array.getSomes(values.map((member) => Option.map(named(name), (entry) => engine.constant(entry, member)))).join(', ')})`]);
        return Match.value(rule).pipe(
            Match.when(null, () => spaced([operand(name), 'IS NULL'])),
            Match.when({ not: null }, () => spaced([operand(name), 'IS NOT NULL'])),
            Match.when({ not: Match.defined }, ({ not }) => sql.join('', false)(['(', members(not), ') IS NOT TRUE'])),
            Match.orElse(members),
        );
    };
    const keyed = ({ column: name, casefold }: Relation['indexes'][number]['keys'][number], method: Method) => {
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
                        ...(Array.isReadonlyArrayNonEmpty(entry.literals) ? [sql`${subject} IN (${sql.literal(entry.literals.map((value) => engine.constant(entry, typeof value === 'number' ? value : `${value}`)).join(', '))})`] : []),
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
        const stamp = Option.getOrElse(identities(entry.name), () => ({ origin: Option.getOrElse(entry.renamedFrom, () => entry.name), generation: entry.generation }));
        const hashed = digest(sql`${spaced([...parts, ...Option.toArray(rule(sql.literal('subject')))])}`.compile(true)[0]);
        const comment = engine.comment(sql, entry.name, { origin: stamp.origin, generation: stamp.generation, digest: hashed });
        return {
            ...entry,
            type,
            check: Option.map(rule(name), (text) => ({ ...checkName, text })),
            default: defaulted,
            identity: stamp,
            digest: hashed,
            comment,
            text: spaced([sql`${name}${comment.inline}`, ...parts, ...Option.toArray(rule(name)).map((text) => engine.constraint(sql, checkName.name, text))]),
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
const diff = <L extends Live, C extends Live>(
    sql: SqlClient.SqlClient,
    catalog: Catalog,
    rendered: readonly { readonly declared: Relation; readonly rendered: ReturnType<typeof render> }[],
    { tables, columns, references }: { readonly tables: readonly L[]; readonly columns: (live: L) => readonly C[]; readonly references: (live: L) => readonly Reference[] },
) => {
    const claim = <R extends Live, D extends { readonly name: string; readonly renamedFrom: Option.Option<string>; readonly identity: typeof Identity.Type }>(live: readonly R[], retired: readonly string[], declared: readonly D[]) => {
        const [standing, retiring] = Array.partition(live, (row) => {
            const recorded = Option.flatMap(Option.fromNullOr(row.identity), stored);
            const { origin, generation } = Option.getOrElse(recorded, () => ({ origin: row.name, generation: 0 }));
            return generation < Array.countBy(retired, Equal.equals(row.name)) ? Result.fail(row) : Result.succeed({ row, origin, generation, recorded });
        });
        type Candidate = (typeof standing)[number];
        const unclaimed: readonly Candidate[] = [];
        const owns = (item: D, candidate: Candidate) => candidate.generation === item.identity.generation && [candidate.row.name, candidate.origin].some((name) => name === item.name || Option.contains(item.renamedFrom, name));
        const take = (claimed: readonly Candidate[], pick: Option.Option<Candidate>) => [Option.match(pick, { onNone: () => claimed, onSome: (candidate) => [...claimed, candidate] }), pick] as const;
        const [owners, owned] = Array.mapAccum(declared, unclaimed, (claimed, item) =>
            take(
                claimed,
                Array.findFirst(standing, (candidate) => !claimed.includes(candidate) && owns(item, candidate)),
            ),
        );
        const [, chosen] = Array.mapAccum(Array.zip(declared, owned), owners, (claimed, [item, own]) =>
            Option.isSome(own)
                ? ([claimed, own] as const)
                : take(
                      claimed,
                      Array.findFirst(standing, (candidate) => !claimed.includes(candidate) && candidate.row.name === item.name),
                  ),
        );
        const found = Array.zip(declared, chosen).map(([item, choice]) => ({
            item,
            row: Option.map(choice, Struct.get('row')),
            owned: Option.exists(choice, (candidate) => owns(item, candidate)),
            identity: Option.match(choice, { onNone: () => ({ origin: item.identity.origin, generation: item.identity.generation }), onSome: ({ origin, generation }) => ({ origin, generation }) }),
            recorded: Option.flatMap(choice, Struct.get('recorded')),
        }));
        return { found, retiring, claimed: Array.getSomes(found.map(Struct.get('row'))) };
    };
    const located = claim(
        tables,
        catalog.retired,
        rendered.map((table) => ({ table, name: table.declared.name, renamedFrom: table.declared.renamedFrom, identity: table.rendered.identity })),
    );
    const entries = located.found.map(({ item: { table }, row, owned, identity, recorded }) => {
        const live = Option.match(row, { onNone: () => [], onSome: columns });
        const resolved = claim(live, table.declared.retired.columns, table.rendered.definitions);
        const fields = resolved.found.map(({ item: definition, row: found, ...claimed }) => ({ definition, found, ...claimed }));
        const renames = fields.flatMap(({ definition, found }) =>
            Option.toArray(found)
                .filter(({ name }) => name !== definition.name)
                .map(({ name }) => ({ from: name, to: definition.name })),
        );
        const dropping = owned ? resolved.retiring : [];
        const added = Array.sortWith(
            fields.filter(({ found }) => Option.isNone(found)).map(({ definition }) => definition),
            (definition) => Option.exists(definition.projection, Column.Projection.$is('Where')),
            Order.Boolean,
        );
        return {
            ...table,
            row,
            owned,
            identity,
            recorded,
            fields,
            dropping,
            retained: Array.difference(live, [...dropping, ...resolved.claimed]),
            renames,
            renamed: (name: string) => renames.find(({ from }) => from === name)?.to ?? name,
            original: (name: string) => renames.find(({ to }) => to === name)?.from ?? name,
            added,
            required: added.filter((definition) => !definition.nullable && Option.isNone(definition.default) && Option.isNone(definition.projection)),
        };
    });
    const [matched, unordered] = Array.partition(entries, (entry) => Option.match(entry.row, { onNone: () => Result.fail(entry), onSome: (live) => Result.succeed({ ...entry, live, retiredIndexes: entry.declared.retired.indexes.map((parts) => identifier([live.name, ...parts])) }) }));
    const edges = unordered.flatMap((entry, child) =>
        entry.rendered.links.flatMap((link) =>
            Option.toArray(Array.findFirstIndex(unordered, ({ declared }) => declared.name === link.target))
                .filter((parent) => parent !== child)
                .map((parent) => ({ parent, child, link })),
        ),
    );
    const graph = (included: typeof edges) =>
        Graph.directed<number, number>((mutable) => {
            unordered.forEach((_, index) => {
                Graph.addNode(mutable, index);
            });
            included.forEach(({ parent, child }) => {
                Graph.addEdge(mutable, parent, child, child);
            });
        });
    const components = Graph.stronglyConnectedComponents(graph(edges));
    const cyclic = edges.filter(({ parent, child }) => components.some((component) => component.includes(parent) && component.includes(child)));
    const created = Array.fromIterable(Graph.values(Graph.topo()(graph(Array.difference(edges, cyclic))))).flatMap((index) =>
        Option.toArray(Array.get(unordered, index)).map((entry) => ({
            ...entry,
            deferred: cyclic.filter(({ child }) => child === index).map(Struct.get('link')),
        })),
    );
    const kept = Array.difference(tables, [...located.retiring, ...located.claimed]);
    const removed = located.retiring.map(({ name }) => name);
    const occupied = SqlSchema.findOne({ Request: Schema.String, Result: Schema.Struct({ populated: Schema.BooleanFromBit }), execute: (name) => sql`SELECT EXISTS (SELECT 1 FROM ${sql(name)}) AS populated` });
    return {
        created,
        matched,
        removed,
        kept,
        populated: Effect.map(
            Effect.forEach(
                matched.filter(({ required }) => Array.isReadonlyArrayNonEmpty(required)).map(({ live }) => live.name),
                (name) => Effect.map(occupied(name), ({ populated }) => (populated ? [name] : [])),
            ),
            Array.flatten,
        ),
        referenced: (live: string) =>
            Option.match(
                Array.findFirst(matched, (entry) => entry.live.name === live),
                { onNone: () => ({ name: live, renamed: Function.identity<string> }), onSome: (entry) => ({ name: entry.declared.name, renamed: entry.renamed }) },
            ),
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
            const findings = [
                ...matched.filter(({ live }) => parts.populated.includes(live.name)).flatMap(({ declared, required }) => Array.match(required.map(Struct.get('name')), { onEmpty: (): readonly Finding[] => [], onNonEmpty: (named) => [finding(declared.name, named, { _tag: 'Required' })] })),
                ...matched.flatMap((entry) =>
                    Array.match(
                        entry.fields.filter((field) => entry.owned && field.owned && !field.definition.retype && parts.drifted(entry, field)).map(({ definition }) => definition.name),
                        { onEmpty: (): readonly Finding[] => [], onNonEmpty: (moved) => [finding(entry.declared.name, moved, { _tag: 'Retype' })] },
                    ),
                ),
                ...kept.flatMap((row) =>
                    references(row)
                        .filter(({ target }) => dropped.includes(target))
                        .map(({ columns: linked, target }) => finding(row.name, linked, { _tag: 'RetainedReference', target })),
                ),
                ...matched.flatMap((entry) =>
                    references(entry.live)
                        .filter(({ columns: linked, target }) => removed.includes(target) && Array.isReadonlyArrayEmpty(Array.intersection(linked, entry.dropping.map(Struct.get('name')))))
                        .map((reference) => ({ ...reference, columns: Array.map(reference.columns, entry.renamed) }))
                        .filter(({ columns: linked }) => !entry.rendered.links.some((link) => Equal.equals(link.columns, linked)))
                        .map(({ columns: linked, target }) => finding(entry.declared.name, linked, { _tag: 'RetainedReference', target })),
                ),
                ...parts.findings,
            ];
            const planned = {
                preamble: parts.preamble,
                statements: [
                    ...parts.opening,
                    ...removed.map((name) => sql`DROP TABLE ${sql(name)}`),
                    ...matched.filter(({ live, declared }) => live.name !== declared.name).map(({ live, rendered: { relation } }) => sql`ALTER TABLE ${sql(live.name)} RENAME TO ${relation}`),
                    ...matched.flatMap(({ rendered: { relation }, renames }) => renames.map(({ from, to }) => sql`ALTER TABLE ${relation} RENAME COLUMN ${sql(from)} TO ${sql(to)}`)),
                    ...parts.statements,
                ],
                concurrent: parts.concurrent,
                session: parts.session,
                undeclared: { tables: kept.map(({ name }) => name), columns: matched.flatMap(({ declared, retained }) => retained.map(({ name }) => ({ table: declared.name, column: name }))), ...parts.undeclared },
            };
            return Array.match(findings, { onEmpty: () => Effect.succeed(planned), onNonEmpty: (all) => Effect.fail(ReconcileError.make({ findings: all })) });
        },
    };
};

// --- [RECONCILE]
const Reconcile = {
    literal,
    identifier,
    constant: ({ kind }: Column, value: string | number) =>
        Match.value(kind).pipe(
            Match.whenOr('integer', 'real', 'boolean', () => `${value}`),
            Match.orElse(() => literal(`${value}`)),
        ),
    unsupported,
    refusals,
    render,
    diff,
    Identity,
    referentialActions: ['NO ACTION', 'RESTRICT', 'CASCADE', 'SET NULL', 'SET DEFAULT'],
    methods,
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Reconcile };
