import { Array, Effect, Equal, Function, Option, Order, Record, Result } from 'effect';
import type { Catalog } from '#/definition';
import { identifier, quote, transforms } from '#/lexical';
import { ddl } from '#/reconcile/ddl';
import { Finding, ReconcileError } from '#/reconcile/finding';
import { type Entry, ledger } from '#/reconcile/ledger';

// --- [OPERATIONS] ----------------------------------------------------------------------

const catalogDdl = (dialect: Parameters<typeof ddl>[0], catalog: Catalog) => [ledger, ...catalog.tables].map((declared) => ({ declared, rendered: ddl(dialect, declared, quote, { elements: [], autoincrement: false }) }));
const diff = <L extends { readonly name: string }, C extends { readonly name: string }>(catalog: Catalog, drawings: ReturnType<typeof catalogDdl>, { stored, tables, columns }: { readonly stored: Option.Option<typeof ledger.Type>; readonly tables: readonly L[]; readonly columns: (live: L) => readonly C[] }) => {
    const { revision, identity } = Option.getOrElse(stored, (): typeof ledger.Type => ({ revision: 0, identity: {} }));
    const claim = <R extends { readonly name: string }, D extends { readonly name: string; readonly earlier: Option.Option<string>; readonly incarnation: number }>(live: readonly R[], recorded: Readonly<Record<string, typeof Entry.Type>>, retired: readonly string[], declared: readonly D[]) => {
        const [standing, retiring] = Array.partition(live, (row) => {
            const { origin = row.name, incarnation = 0 } = recorded[row.name] ?? {};
            return incarnation < Array.countBy(retired, Equal.equals(row.name)) ? Result.fail(row) : Result.succeed({ row, origin, incarnation });
        });
        const found = declared.map((item) => {
            const owns = (candidate: (typeof standing)[number]) => candidate.incarnation === item.incarnation && [candidate.row.name, candidate.origin].some((name) => name === item.name || Option.contains(item.earlier, name));
            const chosen = Array.head(
                Array.sort(
                    standing.filter((candidate) => owns(candidate) || candidate.row.name === item.name),
                    Order.mapInput(Order.Tuple([Order.Boolean, Order.Boolean]), (candidate: (typeof standing)[number]) => [!owns(candidate), candidate.row.name !== item.name] as const),
                ),
            );
            const { origin, incarnation } = Option.getOrElse(chosen, () => ({ origin: Option.getOrElse(item.earlier, () => item.name), incarnation: item.incarnation }));
            return { item, row: Option.map(chosen, ({ row }) => row), owned: Option.exists(chosen, owns), entry: { ...(origin === item.name ? {} : { origin }), ...(incarnation === 0 ? {} : { incarnation }) } };
        });
        return { found, retiring, claimed: found.flatMap(({ row }) => Option.toArray(row)) };
    };
    const retiredTables = catalog.retired.map(transforms.transformQueryNames);
    const located = claim(
        tables,
        identity,
        retiredTables,
        drawings.map((drawing) => ({ drawing, name: drawing.declared.definition.name, earlier: drawing.declared.definition.earlier, incarnation: Array.countBy(retiredTables, Equal.equals(drawing.declared.definition.name)) })),
    );
    const entries = located.found.map(({ item: { drawing }, row, owned, entry }) => {
        const recorded = Option.match(row, { onNone: Record.empty<string, typeof Entry.Type>, onSome: (relation) => identity[relation.name]?.columns ?? {} });
        const live = Option.match(row, { onNone: () => [], onSome: columns });
        const resolved = claim(live, recorded, drawing.declared.definition.retired.columns, drawing.rendered.definitions);
        const fields = resolved.found.map(({ item: definition, row: found }) => ({ definition, found }));
        const renames = fields.flatMap(({ definition, found }) =>
            Option.toArray(found)
                .filter(({ name }) => name !== definition.name)
                .map(({ name }) => ({ from: name, to: definition.name })),
        );
        const doomed = owned ? resolved.retiring : [];
        const retained = Array.difference(live, [...doomed, ...resolved.claimed]);
        const identities = Record.filter(Record.fromEntries([...resolved.found.map(({ item, entry: column }) => [item.name, column] as const), ...retained.map(({ name }) => [name, recorded[name] ?? {}] as const)]), (column) => Object.keys(column).length > 0);
        return {
            ...drawing,
            row,
            owned,
            fields,
            doomed,
            retained,
            renames,
            renamed: (name: string) => renames.find(({ from }) => from === name)?.to ?? name,
            original: (name: string) => renames.find(({ to }) => to === name)?.from ?? name,
            added: Array.sortWith(
                fields.filter(({ found }) => Option.isNone(found)).map(({ definition }) => definition),
                (definition) => Option.exists(definition.projection, (projected) => projected._tag === 'Where'),
                Order.Boolean,
            ),
            recorded: [drawing.declared.definition.name, { ...entry, ...(Record.isEmptyRecord(identities) ? {} : { columns: identities }) }] as const,
        };
    });
    const [matched, created] = Array.partition(entries, (entry) => Option.match(entry.row, { onNone: () => Result.fail(entry), onSome: (live) => Result.succeed({ ...entry, live, retiredIndexes: entry.declared.definition.retired.indexes.map((parts) => identifier([live.name, ...parts])) }) }));
    const kept = Array.difference(tables, [...located.retiring, ...located.claimed]);
    const document = Record.filter(Record.fromEntries([...entries.map(({ recorded }) => recorded), ...kept.map(({ name }) => [name, identity[name] ?? {}] as const)]), (value) => Object.keys(value).length > 0);
    return {
        created,
        matched,
        referenced: (live: string) =>
            Option.match(
                Array.findFirst(matched, (entry) => entry.live.name === live),
                { onNone: () => ({ name: live, renamed: Function.identity<string> }), onSome: (entry) => ({ name: entry.declared.definition.name, renamed: entry.renamed }) },
            ),
        removed: located.retiring,
        kept,
        plan: <U extends object>(parts: Record<'preamble' | 'opening' | 'statements' | 'concurrent', readonly string[]> & { readonly findings: readonly (typeof Finding.Type)[]; readonly undeclared: U }) => {
            const statements = [
                ...parts.opening,
                ...located.retiring.map(({ name }) => `DROP TABLE ${quote(name)}`),
                ...matched.filter(({ live, declared }) => live.name !== declared.definition.name).map(({ live, rendered }) => `ALTER TABLE ${quote(live.name)} RENAME TO ${rendered.relation}`),
                ...matched.flatMap(({ rendered, renames }) => renames.map(({ from, to }) => `ALTER TABLE ${rendered.relation} RENAME COLUMN ${quote(from)} TO ${quote(to)}`)),
                ...parts.statements,
            ];
            const findings = [
                ...matched.flatMap(({ declared, added }) =>
                    Array.match(
                        added.filter((column) => !column.nullable && Option.isNone(column.fallback) && Option.isNone(column.projection)).map(({ name }) => name),
                        { onEmpty: () => [], onNonEmpty: (required) => [Finding.cases.Required.make({ table: declared.definition.name, columns: required })] },
                    ),
                ),
                ...parts.findings,
            ];
            const planned = {
                preamble: parts.preamble,
                statements,
                ledger: Option.liftPredicate({ revision, document }, () => Array.isReadonlyArrayNonEmpty(statements) || !Equal.equals(document, identity)),
                concurrent: parts.concurrent,
                undeclared: { tables: kept.map(({ name }) => name), columns: matched.flatMap(({ declared, retained }) => retained.map(({ name }) => ({ table: declared.definition.name, column: name }))), ...parts.undeclared },
            };
            return Array.isReadonlyArrayNonEmpty(findings) ? Effect.fail(ReconcileError.make({ findings })) : Effect.succeed(planned);
        },
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { catalogDdl, diff };
