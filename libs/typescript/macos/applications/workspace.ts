import { isAbsolute } from 'node:path';
import { pathToFileURL } from 'node:url';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Cause, Context, Effect, Equivalence, Function, HashSet, Layer, Option, Predicate, Record, Result, type Schema, Sink, Stream, Struct, SubscriptionRef } from 'effect';
import { ApplicationServer } from '#/client';
import { type Installation, type InstanceSnapshot, InstanceState } from '#/contract';

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

class Workspace extends Context.Service<Workspace>()('@rasm/applications/workspace', {
    make: Effect.fnUntraced(function* <E, R>(cataloguer: Cataloguer<E, R>) {
        const server = yield* ApplicationServer;
        const listEntries = Effect.gen(function* () {
            const installed = yield* server.currentInstallations;
            return claimed(installed, yield* cataloguer(installed.filter(Predicate.not(backgroundOnly))));
        });
        const [first, rest] = yield* Stream.peel(server.instances({}).pipe(successes('Instance listing')), Sink.head());
        const [catalog, running, session, pressure] = yield* Effect.all([
            Effect.flatMap(listEntries, (rows) => SubscriptionRef.make(rows)),
            Effect.flatMap(
                Effect.fromOption(first, () => new Cause.NoSuchElementError('instances ended before its first value')),
                (rows) => SubscriptionRef.make(rows),
            ),
            track('Session reading', server.session()),
            track('Pressure reading', server.pressure()),
        ]);
        const refresh = Effect.catchCause(
            Effect.flatMap(listEntries, (rows) => SubscriptionRef.set(catalog, rows)),
            (cause) => Effect.logError('Application listing failed, the last listing stands', cause),
        );
        yield* server.installations.pipe(
            SubscriptionRef.changes,
            Stream.drop(1),
            Stream.runForEach(() => refresh),
            Effect.forkScoped,
        );
        yield* Effect.forkScoped(
            Stream.runForEach(
                Stream.mapAccum(SubscriptionRef.changes(server.installations), HashSet.empty<string>, (logged, { unreadable }) => [HashSet.union(logged, HashSet.fromIterable(unreadable.map(Struct.get('subject')))), unreadable.filter(({ subject }) => !HashSet.has(logged, subject))]),
                ({ subject, reason, message }) => Effect.logWarning(`Application bundle ${subject} is left out of the listing: ${reason}`, ...Array.fromNullishOr(message)),
            ),
        );
        yield* Effect.forkScoped(Stream.runForEach(rest, (rows) => SubscriptionRef.set(running, rows)));
        const changes = yield* Stream.share(server.events({}).pipe(successes('Modal panel scan'), logEnd('Application events')), { capacity: 'unbounded' });
        const find = Effect.fnUntraced(function* (given: string) {
            const [rows, installed, instances, read] = yield* Effect.all([SubscriptionRef.get(catalog), server.currentInstallations, SubscriptionRef.get(running), Effect.transposeOption(Option.map(bundleAt(given), (url) => server.installation({ url })))]);
            const offered = unlisted(byBundle(rows), installed);
            const outside = Array.dedupe(instances.map(({ url }) => url.href).filter((href) => !Record.has(byBundle(rows), href)));
            const searched = Effect.all([server.searchInstallations(given), Effect.filterMapEffect(outside, (href) => Effect.result(Effect.tapError(server.installation({ url: new URL(href) }), Effect.logWarning)), { concurrency: 'unbounded' })], { concurrency: 'unbounded' });
            return yield* lookupEntry(
                [...rows, ...read.pipe(Option.toArray, offered)],
                Option.match(read, { onNone: () => given, onSome: ({ url }) => url.href }),
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
        return { catalog, running, changes, session: SubscriptionRef.get(session), pressure: SubscriptionRef.get(pressure), find, resolve };
    }),
}) {
    static readonly layer = <E, R>(cataloguer: Cataloguer<E, R>) => Layer.effect(Workspace, Workspace.make(cataloguer));
    static readonly fromInstallations = (offered: readonly Installation[]) => Effect.succeed(entriesOf(offered));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const byBundle = Function.memoize((rows: readonly Entry[]) => Record.fromIterableBy(rows, (row) => row.url.href));
const byInstance = Function.memoize((rows: readonly InstanceSnapshot[]) => Record.fromIterableBy(rows, (row) => row.instance));
const runningOf = Function.memoize((rows: readonly InstanceSnapshot[]) => Record.filter(byInstance(rows), (each) => !InstanceState.isAnyOf(['exited'])(each.state)));
const byIdentifier = Function.memoize((rows: readonly InstanceSnapshot[]) => Array.groupBy(Record.values(runningOf(rows)), (each) => each.identifier));
const instancesOf = (rows: readonly InstanceSnapshot[], followed: HashSet.HashSet<InstanceSnapshot['instance']>) => (row: Pick<Entry, 'url' | 'identifier'>) => {
    const [own, apart] = Array.partition(Option.getOrElse(Record.get(byIdentifier(rows), row.identifier), Array.empty), (each) => (each.url.href === row.url.href ? Result.succeed(each) : Result.fail(each)));
    const [driven, foreign] = Array.partition(apart, (each) => (HashSet.has(followed, each.instance) ? Result.succeed(each) : Result.fail(each)));
    return { meant: Array.isReadonlyArrayNonEmpty(own) ? own : driven, apart: foreign };
};
const lookupEntry = Effect.fnUntraced(function* <E = never>(rows: readonly Entry[], query: string, further: Effect.Effect<readonly Entry[], E> = Effect.succeed([])) {
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
const bundleAt = (given: string) => (URL.canParse(given) ? Option.filter(Option.some(new URL(given)), ({ protocol }) => protocol === 'file:') : Option.map(Option.liftPredicate(given, isAbsolute), pathToFileURL));
const covers =
    (identifier: string) =>
    ({ prefixes }: Option.Option.Value<Installation['claim']>) =>
        prefixes.some((prefix) => identifier.startsWith(prefix));
const claimed = (installed: readonly Installation[], rows: readonly Omit<Entry, 'claim'>[]): readonly Entry[] => {
    const claims = Array.getSomes(installed.map(Struct.get('claim')));
    return rows.map((row): Entry => ({ ...row, claim: Array.findFirst(claims, covers(row.identifier)) }));
};
const unlisted = (listed: Record.ReadonlyRecord<string, Entry>, installed: readonly Installation[]) => (found: readonly Installation[]) => {
    const fresh = Record.values(
        Record.fromIterableBy(
            found.filter((each) => !backgroundOnly(each) && !Record.has(listed, each.url.href)),
            (each) => each.url.href,
        ),
    );
    return claimed([...installed, ...fresh], entriesOf(fresh));
};
const entriesOf = (offered: readonly Installation[]) => offered.map(({ url, identifier, name, bundleName, version }) => ({ url, identifier, name, product: identifier, aliases: Option.toArray(bundleName), facts: Record.getSomes({ version }) }));
const backgroundOnly = ({ activationPolicy }: Installation) => activationPolicy === 'prohibited';
const logEnd =
    (subject: string) =>
    <A, X>(self: Stream.Stream<A, X>) =>
        Stream.catchCause(self, (cause) => Stream.fromEffect(Effect.andThen(Effect.logError(`${subject} ended`, cause), Effect.never)));
const successes =
    (subject: string) =>
    <A, E, X>(self: Stream.Stream<Result.Result<A, E>, X>) =>
        self.pipe(
            Stream.tap((read) => Result.match(read, { onSuccess: () => Effect.void, onFailure: (failure) => Effect.logWarning(`${subject} failed, the last value stands`, failure) })),
            Stream.filterMap(Function.identity),
        );
const track = Effect.fnUntraced(function* <A, X>(subject: string, read: Stream.Stream<Result.Result<A, Error>, X>) {
    const held = yield* SubscriptionRef.make(Option.none<A>());
    yield* Effect.forkScoped(Stream.runForEach(read.pipe(successes(subject), logEnd(subject)), (value) => SubscriptionRef.set(held, Option.some(value))));
    return held;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { byBundle, byInstance, type Entry, instancesOf, lookupEntry, runningOf, unlisted, Workspace };
