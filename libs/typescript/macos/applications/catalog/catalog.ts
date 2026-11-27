import { isAbsolute } from 'node:path';
import { pathToFileURL } from 'node:url';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Cause, Context, Effect, Equivalence, Function, HashSet, Layer, Option, Predicate, Record, Result, type Schema, Stream, Struct, SubscriptionRef } from 'effect';
import { ApplicationServer } from '#/server/client';
import type { Installation } from '#/server/installation';
import { Instance } from '#/server/instance';

// --- [TYPES] ---------------------------------------------------------------------------

interface Entry {
    readonly url: URL;
    readonly identifier: string;
    readonly name: string;
    readonly product: string;
    readonly aliases: readonly string[];
    readonly facts: Record.ReadonlyRecord<string, Schema.Json>;
    readonly claim: Installation['claim'];
}
type Cataloguer<E, R> = (offered: readonly Installation[]) => Effect.Effect<readonly Omit<Entry, 'claim'>[], E, R>;

// --- [SERVICES] ------------------------------------------------------------------------

class Catalog extends Context.Service<Catalog>()('@rasm/applications/catalog/catalog', {
    make: Effect.fnUntraced(function* <E, R>(cataloguer: Cataloguer<E, R>) {
        const server = yield* ApplicationServer;
        const installations = Effect.map(SubscriptionRef.get(server.installed), Struct.get('installations'));
        const list = Effect.gen(function* () {
            const installed = yield* installations;
            return claim(installed, yield* cataloguer(installed.filter(Predicate.not(backgroundOnly))));
        });
        const entries = yield* Effect.flatMap(list, (rows) => SubscriptionRef.make(rows));
        yield* SubscriptionRef.changes(server.installed).pipe(
            Stream.drop(1),
            Stream.runForEach(() =>
                Effect.catchCause(
                    Effect.flatMap(list, (rows) => SubscriptionRef.set(entries, rows)),
                    (cause) => Effect.logError('Application listing failed, the last listing stands', cause),
                ),
            ),
            Effect.forkScoped,
        );
        yield* Effect.forkScoped(
            Stream.runForEach(
                Stream.mapAccum(SubscriptionRef.changes(server.installed), HashSet.empty<string>, (logged, { unreadable }) => [HashSet.union(logged, HashSet.fromIterable(unreadable.map(Struct.get('subject')))), unreadable.filter(({ subject }) => !HashSet.has(logged, subject))]),
                ({ subject, reason, message }) => Effect.logWarning(`Application bundle ${subject} is left out of the listing: ${reason}`, ...Array.fromNullishOr(message)),
            ),
        );
        const find = Effect.fnUntraced(function* (given: string) {
            const [rows, installed, instances, stated] = yield* Effect.all([SubscriptionRef.get(entries), installations, SubscriptionRef.get(server.running), Effect.transposeOption(Option.map(bundleAt(given), (url) => server.installation({ url })))]);
            const offered = unlisted(byBundle(rows), installed);
            const outside = Array.dedupe(instances.map(({ url }) => url.href).filter((href) => !Record.has(byBundle(rows), href)));
            const searched = Effect.all([server.search(given), Effect.filterMapEffect(outside, (href) => Effect.result(Effect.tapError(server.installation({ url: new URL(href) }), Effect.logWarning)), { concurrency: 'unbounded' })], { concurrency: 'unbounded' });
            return yield* lookup(
                [...rows, ...stated.pipe(Option.toArray, offered)],
                Option.match(stated, { onNone: () => given, onSome: ({ url }) => url.href }),
                Effect.map(searched, (found) => offered(found.flat())),
            );
        });
        const resolve = (given: string) =>
            Effect.flatMap(find(given), (row) =>
                Option.match(row.claim, {
                    onNone: () => Effect.succeed(row),
                    onSome: ({ server: owner, scope }) => new Cause.IllegalArgumentError(`${row.name} is driven by MCP server ${owner} (${scope}), call its tools for this application`),
                }),
            );
        return { entries, find, resolve };
    }),
}) {
    static readonly layer = <E, R>(cataloguer: Cataloguer<E, R>) => Layer.effect(Catalog, Catalog.make(cataloguer));
    static readonly fromInstallations = (offered: readonly Installation[]) => Effect.succeed(entriesOf(offered));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const byBundle = Function.memoize((rows: readonly Entry[]) => Record.fromIterableBy(rows, (row) => row.url.href));
const instancesOf = (rows: readonly Instance[], followed: HashSet.HashSet<Instance['instance']>) => (row: Pick<Entry, 'url' | 'identifier'>) => {
    const [own, apart] = Array.partition(Option.getOrElse(Record.get(byIdentifier(rows), row.identifier), Array.empty), (each) => (each.url.href === row.url.href ? Result.succeed(each) : Result.fail(each)));
    const [driven, foreign] = Array.partition(apart, (each) => (HashSet.has(followed, each.instance) ? Result.succeed(each) : Result.fail(each)));
    return { meant: Array.isReadonlyArrayNonEmpty(own) ? own : driven, apart: foreign };
};
const lookup = Effect.fnUntraced(function* <E = never>(rows: readonly Entry[], query: string, further: Effect.Effect<readonly Entry[], E> = Effect.succeed([])) {
    const named = (held: readonly Entry[]) => held.filter((row) => Array.containsWith(Unmatched.named)([row.name, row.product, row.identifier, ...row.aliases, row.url.href], query));
    const listed = named(rows);
    const more = Array.isReadonlyArrayEmpty(listed) ? yield* further : [];
    const all = [...rows, ...more];
    const matched = Option.liftPredicate(listed, Array.isReadonlyArrayNonEmpty).pipe(
        Option.orElse(() => Option.liftPredicate(named(more), Array.isReadonlyArrayNonEmpty)),
        Option.orElse(() => Option.liftPredicate(Array.intersection(all, Unmatched.rank(query, all, Struct.get('name'))), Array.isReadonlyArrayNonEmpty)),
    );
    const forms = Array.dedupeWith(
        Option.getOrElse(matched, Array.empty),
        Equivalence.mapInput(Equivalence.String, (row: Entry) => row.product),
    );
    return Predicate.isTupleOf(forms, 1)
        ? forms[0]
        : yield* Unmatched.of({
              query: Option.some(query),
              ...Option.match(matched, {
                  onNone: () => ({ subject: 'application in the application folders, the Spotlight index, or the running applications', candidates: Array.dedupe(all.map(Struct.get('name'))) }),
                  onSome: () => ({ subject: 'single application', candidates: forms.map(Struct.get('name')) }),
              }),
          });
});
const unlisted = (listed: Record.ReadonlyRecord<string, Entry>, installed: readonly Installation[]) => (found: readonly Installation[]) => {
    const fresh = Record.values(
        Record.fromIterableBy(
            found.filter((each) => !backgroundOnly(each) && !Record.has(listed, each.url.href)),
            (each) => each.url.href,
        ),
    );
    return claim([...installed, ...fresh], entriesOf(fresh));
};
const byIdentifier = Function.memoize((rows: readonly Instance[]) => Array.groupBy(Record.values(Instance.running(rows)), (each) => each.identifier));
const bundleAt = (given: string) => (URL.canParse(given) ? Option.filter(Option.some(new URL(given)), ({ protocol }) => protocol === 'file:') : Option.map(Option.liftPredicate(given, isAbsolute), pathToFileURL));
const claim = (installed: readonly Installation[], rows: readonly Omit<Entry, 'claim'>[]): readonly Entry[] => {
    const claims = Array.getSomes(installed.map(Struct.get('claim')));
    return rows.map((row): Entry => ({ ...row, claim: Array.findFirst(claims, ({ prefixes }) => prefixes.some((prefix) => row.identifier.startsWith(prefix))) }));
};
const entriesOf = (offered: readonly Installation[]) => offered.map(({ url, identifier, name, bundleName, version }) => ({ url, identifier, name, product: identifier, aliases: Option.toArray(bundleName), facts: Record.getSomes({ version }) }));
const backgroundOnly = ({ activationPolicy }: Installation) => activationPolicy === 'prohibited';
const Entry = { byBundle, instancesOf, lookup, unlisted } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Catalog, Entry };
