import { Array, Effect, Equal, Function, Hash, Match, Option, Order, Result, Schema, String, Struct } from 'effect';
import type { SqlClient, Statement } from 'effect/sql';
import type { Catalog } from '#/catalog';
import type { Column } from '#/column';
import type { Engine } from '#/engine';
import { Finding, ReconcileError } from '#/finding';
import type { Relation } from '#/relation';

// --- [CONSTANTS] -----------------------------------------------------------------------

const referentialActions = { 'NO ACTION': 'a', RESTRICT: 'r', CASCADE: 'c', 'SET NULL': 'n', 'SET DEFAULT': 'd' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Identity = Schema.Struct({ origin: Schema.String, incarnation: Schema.Int, digest: Schema.optionalKey(Schema.Int) });
const stored = Schema.decodeOption(Schema.fromJsonString(Identity));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [TEXT]
const literal = (text: string) => `'${text.replaceAll("'", "''")}'`;
const identifier = (parts: readonly string[]) => {
    const name = parts.map(String.camelToSnake).join('_');
    const limit = 63;
    const radix = 36;
    const digest = Math.abs(Hash.string(name)).toString(radix);
    const encoder = new TextEncoder();
    return encoder.encode(name).length <= limit ? name : `${name.slice(0, encoder.encodeInto(name, new Uint8Array(limit - digest.length - 1)).read)}_${digest}`;
};
const constant = ({ kind }: Column, value: string | number) =>
    Match.value(kind).pipe(
        Match.whenOr('integer', 'int64', 'real', 'number', 'boolean', () => `${value}`),
        Match.orElse(() => literal(`${value}`)),
    );

// --- [REFUSALS]
const unsupported = <C extends { readonly name: string }>(owner: string, reason: typeof Finding.cases.Unsupported.Type.reason, rows: readonly C[], refuses: (row: C) => boolean) =>
    Array.match(
        rows.filter(refuses).map(({ name }) => name),
        { onEmpty: () => [], onNonEmpty: (columns) => [Finding.cases.Unsupported.make({ table: owner, reason, columns })] },
    );
const refusals = ({ name: owner, key, references, columns, indexes }: Relation) => {
    const [all, linked] = [Object.values(columns), [...key, ...references.flatMap((reference) => reference.columns)]];
    const conditions = [...indexes.flatMap(({ where }) => where), ...all.flatMap(({ projection }) => Option.toArray(projection).flatMap((projected) => (projected._tag === 'Where' ? projected.where : [])))];
    const constants = ({ name, fallback }: Column) => [
        ...Option.toArray(fallback),
        ...conditions
            .filter(({ column }) => column === name)
            .flatMap(({ predicate }) =>
                Match.value(predicate).pipe(
                    Match.when(null, () => []),
                    Match.when({ not: null }, () => []),
                    Match.when({ not: Match.defined }, ({ not }) => not),
                    Match.orElse(Function.identity),
                ),
            ),
    ];
    return [
        ...unsupported(owner, 'unbounded', all, ({ kind }) => kind === 'number'),
        ...unsupported(owner, 'reserved', all, ({ name }) => ['rowid', 'oid', '_rowid_'].includes(name.toLowerCase())),
        ...unsupported(owner, 'projected', all, ({ name, projection }) => Option.isSome(projection) && linked.includes(name)),
        ...unsupported(owner, 'invalid', all, (entry) => constants(entry).some((value) => Option.isNone(Schema.decodeUnknownOption(entry.schema)(value)))),
    ];
};

// --- [RENDER]
const render = (
    sql: SqlClient.SqlClient,
    engine: Engine,
    catalog: Catalog,
    declared: Relation,
    {
        target = (name) => sql`${sql(name)}`,
        elements = [],
        autoincrement = false,
        identity = Option.none(),
        identities = () => Option.none(),
    }: {
        readonly target?: (name: string) => Statement.Fragment;
        readonly elements?: readonly Statement.Fragment[];
        readonly autoincrement?: boolean;
        readonly identity?: Option.Option<typeof Identity.Type>;
        readonly identities?: (name: string) => Option.Option<typeof Identity.Type>;
    } = {},
) => {
    const { name: owner, key, references, search, tenant, withoutRowid, indexes } = declared;
    const [entries, relation] = [Object.values(declared.columns), target(owner)];
    const quoted = (name: string) => sql`${sql(name)}`;
    const spaced = sql.join(' ', false);
    const list = (columns: readonly string[]) => sql.csv(columns.map(quoted));
    const label = (parts: readonly string[]) => ({ name: identifier([owner, ...parts]), parts });
    const named = (name: string) => Array.findFirst(entries, (entry) => entry.name === name);
    const path = (entry: Column) => Option.flatMap(entry.projection, (projected) => (projected._tag === 'Path' ? Option.some(engine.projection(sql, entry, projected.from, projected.path)) : Option.none()));
    const operand = (name: string) => Option.match(Option.flatMap(named(name), path), { onNone: () => quoted(name), onSome: (expression) => engine.projected(sql, expression, name) });
    const predicate = ({ column: name, predicate: rule }: Relation['indexes'][number]['where'][number]) => {
        const members = (values: readonly (string | number)[]) => spaced([operand(name), `IN (${values.flatMap((member) => Option.toArray(Option.map(named(name), (entry) => engine.constant(entry, member)))).join(', ')})`]);
        return Match.value(rule).pipe(
            Match.when(null, () => spaced([operand(name), 'IS NULL'])),
            Match.when({ not: null }, () => spaced([operand(name), 'IS NOT NULL'])),
            Match.when({ not: Match.defined }, ({ not }) => sql.join('', false)(['(', members(not), ') IS NOT TRUE'])),
            Match.orElse(members),
        );
    };
    const conjunction = sql.join(' AND ', false);
    const projection = (entry: Column) => Option.map(entry.projection, (projected) => (projected._tag === 'Path' ? engine.projection(sql, entry, projected.from, projected.path) : conjunction(projected.where.map(predicate))));
    const keyed = ({ column: name, casefold }: Relation['indexes'][number]['keys'][number], method: string) => {
        const found = named(name);
        const opclass = engine.opclass(
            method,
            Option.exists(found, ({ kind }) => kind === 'json'),
        );
        return {
            text: spaced([casefold ? engine.casefold(sql, name) : Option.match(Option.flatMap(found, projection), { onNone: () => quoted(name), onSome: (expression) => engine.projected(sql, expression, name) }), ...Option.toArray(opclass).map(Struct.get('name'))]),
            extensions: Option.match(opclass, { onNone: () => [], onSome: Struct.get('extensions') }),
        };
    };
    const definitions = entries.map((entry) => {
        const [type, fallback, name] = [engine.types[entry.kind], Option.map(entry.fallback, (value) => engine.constant(entry, value)), quoted(entry.name)];
        const rule = (subject: Statement.Fragment) =>
            Option.map(
                Option.liftPredicate(
                    [
                        ...(Array.isReadonlyArrayNonEmpty(entry.literals) ? [sql`${subject} IN (${sql.literal(entry.literals.map((value) => engine.constant(entry, typeof value === 'number' ? value : `${value}`)).join(', '))})`] : []),
                        ...(entry.kind === 'integer' ? [sql`${subject} BETWEEN ${sql.literal(`${-Number.MAX_SAFE_INTEGER}`)} AND ${sql.literal(`${Number.MAX_SAFE_INTEGER}`)}`] : []),
                        ...entry.bounds.map(({ op, value, length }) => spaced([length ? engine.codePoints(sql, subject) : subject, `${op} ${value}${op === '%' ? ' = 0' : ''}`])),
                        ...engine.domain(sql, subject, entry.kind),
                    ],
                    (all) => Option.isNone(entry.projection) && Array.isReadonlyArrayNonEmpty(all),
                ),
                (all) => spaced(['CHECK', sql`(${conjunction(all)})`]),
            );
        const checkName = label([entry.name, 'check']);
        const body = (subject: Statement.Fragment) =>
            spaced([type, ...Option.toArray(projection(entry)).map((expression) => engine.generated(sql, expression)), ...(entry.nullable ? [] : ['NOT NULL']), ...Option.toArray(fallback).map((value) => `DEFAULT ${value}`), ...Option.toArray(rule(subject)).map((text) => engine.constraint(sql, checkName.name, text))]);
        const stamp = Option.getOrElse(identities(entry.name), () => ({ origin: Option.getOrElse(entry.earlier, () => entry.name), incarnation: entry.incarnation }));
        const digest = Hash.string(sql`${body(sql.literal('subject'))}`.compile(true)[0]);
        const comment = engine.comment(sql, entry.name, { origin: stamp.origin, incarnation: stamp.incarnation, digest });
        return { ...entry, type, check: Option.map(rule(name), (text) => ({ ...checkName, text })), fallback, identity: stamp, digest, comment, text: spaced([sql`${name}${comment.inline}`, body(name)]) };
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
    const stamp = Option.getOrElse(identity, () => ({ origin: Option.getOrElse(declared.earlier, () => owner), incarnation: Array.countBy((catalog.retired ?? []).map(String.camelToSnake), Equal.equals(owner)) }));
    const comment = engine.comment(sql, owner, stamp);
    return {
        relation,
        identity: stamp,
        definitions,
        links,
        indexes: [...built, ...Option.toArray(searched).flatMap(({ indexes: found }) => found.map((index) => ({ ...index, text: (prefix: string) => index.text(prefix, relation), declared: Option.none<Relation['indexes'][number]>() })))],
        fts: Option.flatMap(searched, ({ fts }) => fts),
        policy: Option.flatMap(tenant, ({ column, setting }) => Option.flatMap(named(column), ({ name, kind }) => Option.map(engine.policy(sql, owner, name, engine.types[kind], setting), (rule) => ({ ...rule, text: (role: Statement.Fragment) => rule.text(relation, role) })))),
        create: sql`CREATE TABLE ${relation}${comment.inline} (${sql.csv([...definitions.map(({ text }) => text), ...elements, spaced(['PRIMARY KEY', sql`(${list(key)}${sql.literal(autoincrement ? ' AUTOINCREMENT' : '')})`]), ...links.map(({ name, text }) => engine.constraint(sql, name, text))])})${sql.literal(engine.suffix(withoutRowid))}`,
        comments: [...comment.statements(relation, Option.none()), ...definitions.flatMap(({ name, comment: stamped }) => stamped.statements(relation, Option.some(name)))],
    };
};

// --- [DIFF]
const diff = <L extends { readonly name: string; readonly identity: string | null }, C extends { readonly name: string; readonly identity: string | null }>(
    sql: SqlClient.SqlClient,
    catalog: Catalog,
    drawings: readonly { readonly declared: Relation; readonly rendered: ReturnType<typeof render> }[],
    { tables, columns }: { readonly tables: readonly L[]; readonly columns: (live: L) => readonly C[] },
) => {
    const claim = <R extends { readonly name: string; readonly identity: string | null }, D extends { readonly name: string; readonly earlier: Option.Option<string>; readonly identity: typeof Identity.Type }>(live: readonly R[], retired: readonly string[], declared: readonly D[]) => {
        const [standing, retiring] = Array.partition(live, (row) => {
            const recorded = Option.flatMap(Option.fromNullOr<string | null>(row.identity), stored);
            const { origin, incarnation } = Option.getOrElse(recorded, () => ({ origin: row.name, incarnation: 0 }));
            return incarnation < Array.countBy(retired, Equal.equals(row.name)) ? Result.fail(row) : Result.succeed({ row, origin, incarnation, recorded });
        });
        const found = declared.map((item) => {
            const owns = (candidate: (typeof standing)[number]) => candidate.incarnation === item.identity.incarnation && [candidate.row.name, candidate.origin].some((name) => name === item.name || Option.contains(item.earlier, name));
            const chosen = Array.head(
                Array.sort(
                    standing.filter((candidate) => owns(candidate) || candidate.row.name === item.name),
                    Order.mapInput(Order.Tuple([Order.Boolean, Order.Boolean]), (candidate: (typeof standing)[number]) => [!owns(candidate), candidate.row.name !== item.name] as const),
                ),
            );
            return {
                item,
                row: Option.map(chosen, ({ row }) => row),
                owned: Option.exists(chosen, owns),
                identity: Option.match(chosen, { onNone: () => ({ origin: item.identity.origin, incarnation: item.identity.incarnation }), onSome: ({ origin, incarnation }) => ({ origin, incarnation }) }),
                recorded: Option.flatMap(chosen, ({ recorded }) => recorded),
            };
        });
        return { found, retiring, claimed: found.flatMap(({ row }) => Option.toArray(row)) };
    };
    const located = claim(
        tables,
        (catalog.retired ?? []).map(String.camelToSnake),
        drawings.map((drawing) => ({ drawing, name: drawing.declared.name, earlier: drawing.declared.earlier, identity: drawing.rendered.identity })),
    );
    const entries = located.found.map(({ item: { drawing }, row, owned, identity, recorded }) => {
        const live = Option.match(row, { onNone: () => [], onSome: columns });
        const resolved = claim(live, drawing.declared.retired.columns, drawing.rendered.definitions);
        const fields = resolved.found.map(({ item: definition, row: found, ...claimed }) => ({ definition, found, ...claimed }));
        const renames = fields.flatMap(({ definition, found }) =>
            Option.toArray(found)
                .filter(({ name }) => name !== definition.name)
                .map(({ name }) => ({ from: name, to: definition.name })),
        );
        const doomed = owned ? resolved.retiring : [];
        return {
            ...drawing,
            row,
            owned,
            identity,
            recorded,
            fields,
            doomed,
            retained: Array.difference(live, [...doomed, ...resolved.claimed]),
            renames,
            renamed: (name: string) => renames.find(({ from }) => from === name)?.to ?? name,
            original: (name: string) => renames.find(({ to }) => to === name)?.from ?? name,
            added: Array.sortWith(
                fields.filter(({ found }) => Option.isNone(found)).map(({ definition }) => definition),
                (definition) => Option.exists(definition.projection, (projected) => projected._tag === 'Where'),
                Order.Boolean,
            ),
        };
    });
    const [matched, created] = Array.partition(entries, (entry) => Option.match(entry.row, { onNone: () => Result.fail(entry), onSome: (live) => Result.succeed({ ...entry, live, retiredIndexes: entry.declared.retired.indexes.map((parts) => identifier([live.name, ...parts])) }) }));
    const kept = Array.difference(tables, [...located.retiring, ...located.claimed]);
    return {
        created,
        matched,
        referenced: (live: string) =>
            Option.match(
                Array.findFirst(matched, (entry) => entry.live.name === live),
                { onNone: () => ({ name: live, renamed: Function.identity<string> }), onSome: (entry) => ({ name: entry.declared.name, renamed: entry.renamed }) },
            ),
        removed: located.retiring,
        kept,
        plan: <U extends object>(
            parts: Record<'preamble' | 'opening' | 'statements' | 'concurrent', readonly Statement.Statement<object>[]> & {
                readonly findings: readonly (typeof Finding.Type)[];
                readonly undeclared: U;
                readonly drifted: (entry: (typeof matched)[number], field: (typeof matched)[number]['fields'][number]) => boolean;
            },
        ) => {
            const findings = [
                ...matched.flatMap(({ declared, added }) =>
                    Array.match(
                        added.filter((column) => !column.nullable && Option.isNone(column.fallback) && Option.isNone(column.projection)).map(({ name }) => name),
                        { onEmpty: () => [], onNonEmpty: (required) => [Finding.cases.Required.make({ table: declared.name, columns: required })] },
                    ),
                ),
                ...matched.flatMap((entry) =>
                    Array.match(
                        entry.fields.filter((field) => entry.owned && field.owned && !field.definition.retype && parts.drifted(entry, field)).map(({ definition }) => definition.name),
                        { onEmpty: () => [], onNonEmpty: (moved) => [Finding.cases.Retype.make({ table: entry.declared.name, columns: moved })] },
                    ),
                ),
                ...parts.findings,
            ];
            const planned = {
                preamble: parts.preamble,
                statements: [
                    ...parts.opening,
                    ...located.retiring.map(({ name }) => sql`DROP TABLE ${sql(name)}`),
                    ...matched.filter(({ live, declared }) => live.name !== declared.name).map(({ live, rendered }) => sql`ALTER TABLE ${sql(live.name)} RENAME TO ${rendered.relation}`),
                    ...matched.flatMap(({ rendered, renames }) => renames.map(({ from, to }) => sql`ALTER TABLE ${rendered.relation} RENAME COLUMN ${sql(from)} TO ${sql(to)}`)),
                    ...parts.statements,
                ],
                concurrent: parts.concurrent,
                undeclared: { tables: kept.map(({ name }) => name), columns: matched.flatMap(({ declared, retained }) => retained.map(({ name }) => ({ table: declared.name, column: name }))), ...parts.undeclared },
            };
            return Array.isReadonlyArrayNonEmpty(findings) ? Effect.fail(ReconcileError.make({ findings })) : Effect.succeed(planned);
        },
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { constant, diff, Identity, identifier, literal, referentialActions, refusals, render, unsupported };
