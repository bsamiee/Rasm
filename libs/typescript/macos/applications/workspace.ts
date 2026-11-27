import { isAbsolute } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { Handle, Server } from '@rasm/mcp/server';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, ByteSize, Cause, Context, Data, DateTime, Duration, Effect, Equal, Equivalence, Function, HashSet, Layer, Option, Predicate, Queue, Record, Ref, Result, Schema, SchemaGetter, Sink, Stream, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { Address, type Index } from '#/address';
import { ApplicationServer } from '#/client';
import { Installation } from '#/installations';
import { Event, InstanceSnapshot, InstanceState, type Running } from '#/instances';
import { Container, Locator } from '#/interface';
import { Outline } from '#/outline';

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
interface Seen {
    readonly started: Record.ReadonlyRecord<string, string>;
    readonly stalled: HashSet.HashSet<string>;
    readonly concealed: HashSet.HashSet<string>;
    readonly secure: readonly (typeof Secure.Type)[];
}
type Hold = Data.TaggedEnum<{
    Using: { readonly instance: InstanceSnapshot['instance'] };
    Launching: { readonly instance: InstanceSnapshot['instance']; readonly name: string };
    Quitting: { readonly instance: InstanceSnapshot['instance'] };
}>;
type Raised = Data.TaggedEnum<{
    Changed: { readonly application: string; readonly event: Event; readonly seen: Seen; readonly next: Seen; readonly row: Option.Option<string> };
    Ended: { readonly instance: InstanceSnapshot['instance']; readonly key: string; readonly left: boolean };
}>;
type Cataloguer<E, R> = (offered: readonly Installation[]) => Effect.Effect<readonly Omit<Entry, 'claim'>[], E, R>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const CAUSE = { running: 'computing', runnable: 'waiting for a processor', paging: 'paging', reading: 'reading disk', writing: 'writing disk', idle: 'idle' } as const satisfies Record.ReadonlyRecord<NonNullable<Running['usage']>['cause'], string>;
const PRESENTED = Schema.is(Container.fields.kind.pick(['window', 'dialog', 'panel', 'sheet', 'popover', 'drawer', 'prompt']));
const UNSEEN: Seen = { started: Record.empty(), stalled: HashSet.empty(), concealed: HashSet.empty(), secure: [] };

// --- [MODELS] --------------------------------------------------------------------------

const Stored = Schema.Struct({
    held: Schema.OptionFromOptionalKey(Schema.Struct({ url: Installation.fields.url, instance: InstanceSnapshot.fields.instance, language: Schema.String })),
    followed: Schema.Array(
        Schema.Struct({
            instance: InstanceSnapshot.fields.instance,
            name: Schema.String.annotate({ description: 'Name of the application as the server stopped, which an exit during the restart is reported by' }),
            locators: Schema.OptionFromOptionalKey(Schema.Array(Locator).annotate({ description: 'Locator table of the Watch in order of first sight, read as the server stopped, which a new Watch is seeded with so every reference keeps its number' })),
        }),
    ),
    named: Schema.OptionFromOptionalKey(Installation.fields.url),
}).annotate({ description: 'Target of a handle as the server stores it across a restart' });
const Secure = Schema.Struct({ instance: InstanceSnapshot.fields.instance, container: Event.fields.change.cases.asking.fields.container });
const SecureKey = Schema.fromJsonString(Secure);
const Hold = Data.taggedEnum<Hold>();
const Raised = Data.taggedEnum<Raised>();
class Aim extends Data.Class<{
    readonly held: Option.Option<{ readonly row: Entry; readonly instance: InstanceSnapshot['instance']; readonly language: string }>;
    readonly named: Option.Option<Entry>;
    readonly holds: readonly Hold[];
}> {
    static readonly empty = new Aim({ held: Option.none(), named: Option.none(), holds: [] });
    get followed() {
        return HashSet.fromIterable([...Option.toArray(this.held), ...this.holds].map(({ instance }) => instance));
    }
    static readonly stored = ({
        server,
        catalog,
        running,
        tables,
        vanished,
    }: {
        readonly server: ApplicationServer['Service'];
        readonly catalog: SubscriptionRef.SubscriptionRef<readonly Entry[]>;
        readonly running: SubscriptionRef.SubscriptionRef<readonly InstanceSnapshot[]>;
        readonly tables: Ref.Ref<Record.ReadonlyRecord<string, readonly string[]>>;
        readonly vanished: Ref.Ref<Record.ReadonlyRecord<string, { readonly instance: InstanceSnapshot['instance']; readonly name: string }>>;
    }) =>
        Stored.pipe(
            Schema.decodeTo(Schema.instanceOf(Aim), {
                decode: SchemaGetter.transformEffect(
                    Effect.fnUntraced(function* ({ held, followed, named }: typeof Stored.Type) {
                        const [rows, instances, installed] = yield* Effect.all([SubscriptionRef.get(catalog), SubscriptionRef.get(running), server.currentInstallations]);
                        const aimedAt = followed.filter(({ instance }) => Option.exists(held, (aim) => aim.instance === instance));
                        const [alive, gone] = Array.partition(aimedAt, (each) => (Record.has(runningOf(instances), each.instance) ? Result.succeed(each) : Result.fail(each)));
                        const seeded = Array.getSomes(alive.map(({ instance, locators }) => Option.map(locators, (seed) => ({ instance, seed }))));
                        const attached = yield* Effect.filterMapEffect(
                            seeded,
                            ({ instance, seed }) =>
                                server.watch({ instance, locators: seed }).pipe(
                                    Effect.tapError((cause) => Effect.logWarning(`Watch of ${instance} failed to restore, its references are read again`, cause)),
                                    Effect.as(instance),
                                    Effect.result,
                                ),
                            { concurrency: 'unbounded' },
                        );
                        const kept = HashSet.fromIterable([...attached, ...gone.map(Struct.get('instance'))]);
                        const entryAt = (url: URL) =>
                            Option.match(Record.get(byBundle(rows), url.href), {
                                onSome: Effect.succeedSome,
                                onNone: () =>
                                    server.installation({ url }).pipe(
                                        Effect.map((found) => Array.head(unlisted(byBundle(rows), installed)([found]))),
                                        Effect.tapError((cause) => Effect.logWarning(`Application bundle ${url.href} is unreadable, the restored handle forgets it`, cause)),
                                        Effect.orElseSucceed(Option.none<Entry>),
                                    ),
                            });
                        const [aimed, last] = yield* Effect.all([
                            held.pipe(
                                Option.filter(({ instance }) => HashSet.has(kept, instance)),
                                Option.map(({ url, instance, language }) =>
                                    Effect.map(
                                        entryAt(url),
                                        Option.map((row) => ({ row, instance, language })),
                                    ),
                                ),
                                Effect.transposeOption,
                            ),
                            Effect.transposeOption(Option.map(named, entryAt)),
                        ]);
                        yield* Ref.update(vanished, (known) => ({ ...known, ...Record.fromIterableWith(gone, (each) => [each.instance, each]) }));
                        return new Aim({ ...Aim.empty, held: Option.flatten(aimed), named: Option.flatten(last) });
                    }),
                ),
                encode: SchemaGetter.transformEffect(
                    Effect.fnUntraced(function* ({ held, followed, named }: Aim) {
                        const [read, instances] = yield* Effect.all([Ref.get(tables), SubscriptionRef.get(running)]);
                        return {
                            held: Option.map(held, ({ row, instance, language }) => ({ url: row.url, instance, language })),
                            followed: Iterator.from(followed)
                                .flatMap((instance) => Option.toArray(Option.map(Record.get(runningOf(instances), instance), (each) => ({ instance, name: each.name ?? each.identifier, locators: Record.get(read, instance) }))))
                                .toArray(),
                            named: Option.map(named, ({ url }) => url),
                        };
                    }),
                ),
            }),
        );
}

// --- [SERVICES] ------------------------------------------------------------------------

const Target = Context.Reference<Aim>('@rasm/applications/workspace/Target', { defaultValue: () => Aim.empty });
const aimed = Effect.flatMap(Handle, (handle) => handle.get(Target));
const retarget = (change: (before: Aim) => Partial<Pick<Aim, 'held' | 'named' | 'holds'>>) => Effect.flatMap(Handle, (handle) => handle.update(Target, (before) => new Aim({ ...before, ...change(before) })));
const untarget = (gone: ReadonlySet<InstanceSnapshot['instance']>) => retarget(({ held }) => ({ held: Option.filter(held, ({ instance }) => !gone.has(instance)) }));
const hold = (taken: readonly Hold[]) =>
    Effect.acquireRelease(
        retarget(({ holds }) => ({ holds: [...holds, ...taken] })),
        () => retarget(({ holds }) => ({ holds: holds.filter((each) => !taken.includes(each)) })),
    );
const Proving = Context.Reference<Record.ReadonlyRecord<string, string>>('@rasm/applications/workspace/Proving', { defaultValue: () => Record.empty() });
const Indexed = Context.Reference<Option.Option<{ readonly instance: InstanceSnapshot['instance']; readonly index: Index }>>('@rasm/applications/tools/Indexed', { defaultValue: Option.none });

class Workspace extends Context.Service<Workspace>()('@rasm/applications/workspace', {
    make: Effect.fnUntraced(function* <E, R>(cataloguer: Cataloguer<E, R>) {
        const server = yield* ApplicationServer;
        const listEntries = Effect.gen(function* () {
            const installed = yield* server.currentInstallations;
            return claimed(installed, yield* cataloguer(installed.filter(Predicate.not(backgroundOnly))));
        });
        const [first, rest] = yield* Stream.peel(server.instances({}).pipe(successes('Instance listing')), Sink.head());
        const [catalog, running, session, pressure, tables, vanished, driven, exits] = yield* Effect.all([
            Effect.flatMap(listEntries, (rows) => SubscriptionRef.make(rows)),
            Effect.flatMap(
                Effect.fromOption(first, () => new Cause.NoSuchElementError('instances ended before its first value')),
                (rows) => SubscriptionRef.make(rows),
            ),
            track('Session reading', server.session()),
            track('Pressure reading', server.pressure()),
            Ref.make(Record.empty<string, readonly string[]>()),
            Ref.make(Record.empty<string, { readonly instance: InstanceSnapshot['instance']; readonly name: string }>()),
            Ref.make(HashSet.empty<InstanceSnapshot['instance']>()),
            Queue.unbounded<Raised>(),
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
        const { states } = yield* Server;
        const left = states(Target).pipe(
            Stream.map(Array.map(({ followed }) => followed)),
            Stream.changesWith(Array.makeEquivalence<HashSet.HashSet<InstanceSnapshot['instance']>>(Equal.equals)),
            Stream.map(() => Option.none<Event>()),
        );
        const raised = yield* Stream.share(
            Stream.merge(Stream.map(changes, Option.some), left).pipe(
                Stream.mapAccumEffect(
                    Function.constant(UNSEEN),
                    Effect.fnUntraced(function* (seen: Seen, change: Option.Option<Event>) {
                        const [instances, following] = yield* Effect.all([SubscriptionRef.get(running), Ref.get(driven)]);
                        const next = Option.match(change, { onNone: () => seen, onSome: (event) => advance(seen, event, following) });
                        const ending = Option.isNone(change) ? seen.secure : Array.difference(seen.secure, next.secure);
                        const [keys, rows] = yield* Effect.all([Effect.forEach(ending, keyOf), Effect.forEach(Array.difference(next.secure, seen.secure), keyOf)]);
                        const ended = Array.zipWith(ending, keys, ({ instance }, key) => Raised.Ended({ instance, key, left: Option.isNone(change) }));
                        const at = Option.map(change, Struct.get('instance'));
                        const named = Option.firstSomeOf([
                            Option.flatMap(at, (instance) => Record.get(byInstance(instances), instance)).pipe(Option.map((each) => each.name ?? each.identifier)),
                            Option.flatMap(at, (instance) => Record.get(next.started, instance)),
                            Option.flatMap(at, (instance) => Record.get(seen.started, instance)),
                        ]);
                        const application = Option.getOrElse(named, () => 'a process outside the Dock');
                        const changed = Option.map(change, (event) => Raised.Changed({ application, event, seen, next, row: Array.head(rows) }));
                        return [next, [...ended, ...Option.toArray(changed)]] as const;
                    }),
                ),
            ),
            { capacity: 'unbounded' },
        );
        yield* Server.persist(Target, Aim.stored({ server, catalog, running, tables, vanished }));
        yield* Server.use((served) =>
            served.events({
                stream: raised.pipe(Stream.merge(Stream.fromQueue(exits))),
                route,
                press: (key) => Effect.asVoid(Effect.flatMap(Schema.decodeEffect(SecureKey)(key), ({ instance }) => server.activate({ instance }))),
            }),
        );
        yield* states(Target).pipe(
            Stream.map((aims) => Array.reduce(aims, HashSet.empty<InstanceSnapshot['instance']>(), (union, { followed }) => HashSet.union(union, followed))),
            Stream.changes,
            Stream.zipWithPrevious,
            Stream.runForEach(
                Effect.fnUntraced(function* ([before, after]: readonly [Option.Option<HashSet.HashSet<InstanceSnapshot['instance']>>, HashSet.HashSet<InstanceSnapshot['instance']>]) {
                    yield* Ref.set(driven, after);
                    yield* Effect.catchCause(
                        Effect.forEach(HashSet.difference(Option.getOrElse(before, HashSet.empty<InstanceSnapshot['instance']>), after), (instance) => server.release({ instance }), { discard: true }),
                        (cause) => Effect.logWarning('Watch release failed, the Watch stays until its instance exits', cause),
                    );
                    const [due, now] = yield* Effect.all([
                        Ref.modify(
                            vanished,
                            Record.partition((gone) => (HashSet.has(after, gone.instance) ? Result.succeed(gone) : Result.fail(gone))),
                        ),
                        DateTime.now,
                    ]);
                    const at = DateTime.formatIso(now);
                    yield* Queue.offerAll(
                        exits,
                        Record.collect(due, (_, { instance, name }) => Raised.Changed({ application: name, event: Event.make({ instance, sequence: 0, at, change: { _tag: 'exited', termination: { _tag: 'statusFailure', statusFailure: 'server restarted' } } }), seen: UNSEEN, next: UNSEEN, row: Option.none() })),
                    );
                }),
            ),
            Effect.forkScoped,
        );
        yield* Effect.addFinalizer(
            Effect.fnUntraced(function* () {
                const [instances, rows] = yield* Effect.all([Ref.get(driven), SubscriptionRef.get(running)]);
                const due = Iterator.from(instances)
                    .filter((instance) => Record.has(runningOf(rows), instance))
                    .toArray();
                const read = yield* Effect.forEach(due, (instance) => Effect.result(server.locators({ instance })), { concurrency: 'unbounded' });
                const [tabled, unread] = Record.partition(Record.fromEntries(Array.zip(due, read)), Function.identity);
                yield* Effect.forEach(Record.values(unread), (cause) => Effect.logWarning('Locator table is unread, the references of its instance are read again after the restart', cause), { discard: true });
                yield* Ref.set(tables, tabled);
            }),
        );
        const setTarget = Effect.fnUntraced(function* (aim: { readonly row: Entry; readonly instance: InstanceSnapshot['instance'] }) {
            const live = yield* server.watch({ instance: aim.instance });
            yield* retarget(() => ({ held: Option.some({ ...aim, language: live.language }) }));
            return live;
        });
        const find = Effect.fnUntraced(function* (given: string) {
            const [rows, installed, instances, read] = yield* Effect.all([SubscriptionRef.get(catalog), server.currentInstallations, SubscriptionRef.get(running), Effect.transposeOption(Option.map(bundleAt(given), (url) => server.installation({ url })))]);
            const offered = unlisted(byBundle(rows), installed);
            const outside = Array.dedupe(instances.map(({ url }) => url.href).filter((href) => !Record.has(byBundle(rows), href)));
            const searched = Effect.all([server.searchInstallations(given), Effect.filterMapEffect(outside, (href) => Effect.result(Effect.tapError(server.installation({ url: new URL(href) }), Effect.logWarning)), { concurrency: 'unbounded' })], { concurrency: 'unbounded' });
            const row = yield* lookupEntry(
                [...rows, ...read.pipe(Option.toArray, offered)],
                Option.match(read, { onNone: () => given, onSome: ({ url }) => url.href }),
                Effect.map(searched, (found) => offered(found.flat())),
            );
            yield* retarget(() => ({ named: Option.some(row) }));
            return row;
        });
        const resolve = (given: string) =>
            Effect.flatMap(find(given), (row) =>
                Option.match(row.claim, {
                    onNone: () => Effect.succeed(row),
                    onSome: ({ server: owner, scope }) => new Cause.IllegalArgumentError(`${row.name} is driven by MCP server ${owner} (${scope}), call its tools for this application`),
                }),
            );
        const current = Effect.gen(function* () {
            const [{ held, named: last, followed }, rows, entries] = yield* Effect.all([aimed, SubscriptionRef.get(running), SubscriptionRef.get(catalog)]);
            const aim = yield* Effect.fromOption(held, () => new Cause.NoSuchElementError(refuseUntargeted(last, instancesOf(rows, followed), rows, entries)));
            yield* hold([Hold.Using({ instance: aim.instance })]);
            return yield* Option.match(Record.get(runningOf(rows), aim.instance), {
                onSome: (each) => Effect.succeed({ row: aim.row, live: InstanceSnapshot.Watched.make({ ...each, language: aim.language }) }),
                onNone: () =>
                    setTarget(aim).pipe(
                        Effect.filterOrFail(
                            (snapshot) => !InstanceState.isAnyOf(['exited'])(snapshot.state),
                            () => new Cause.NoSuchElementError(`${aim.row.name} no longer runs, ${openCall(aim.row.name)} starts it again`),
                        ),
                        Effect.map((live) => ({ row: aim.row, live })),
                    ),
            });
        });
        const blocked = (name: string, holds: Predicate.Predicate<InstanceSnapshot>) => SubscriptionRef.changes(running).pipe(Stream.map(Array.findFirst(holds)), Stream.filterMap(refuseBlocked(name)), Stream.map(Effect.succeed));
        const blocking = (row: Pick<Entry, 'url' | 'name'>) => Context.make(Server.EarlyReturn, blocked(row.name, Installation.runs(row)));
        const ends = Context.make(Server.EarlyReturn, Stream.unwrap(Effect.map(aimed, ({ held }) => Option.match(held, { onNone: () => Stream.never, onSome: ({ row, instance }) => blocked(row.name, (each) => each.instance === instance) }))));
        const awaited = Context.make(
            Server.EarlyReturn,
            SubscriptionRef.changes(running).pipe(
                Stream.mapAccumEffect(
                    HashSet.empty<InstanceSnapshot['instance']>,
                    Effect.fnUntraced(function* (ran: HashSet.HashSet<InstanceSnapshot['instance']>, rows: readonly InstanceSnapshot[]) {
                        const { holds } = yield* aimed;
                        const launching = holds.filter(Hold.$is('Launching'));
                        const shown = HashSet.union(ran, HashSet.fromIterable(launching.map(({ instance }) => instance).filter((instance) => Record.has(runningOf(rows), instance))));
                        const quitting = holds.filter(Hold.$is('Quitting')).flatMap(({ instance }) => Option.toArray(Record.get(runningOf(rows), instance)));
                        const blocks = [...launching.filter(({ instance }) => HashSet.has(shown, instance)).map(({ instance, name }) => ({ name, found: Record.get(byInstance(rows), instance) })), ...quitting.map((each) => ({ name: each.name ?? each.identifier, found: Option.some(each) }))];
                        return [shown, Array.filterMap(blocks, ({ name, found }) => refuseBlocked(name)(found))] as const;
                    }),
                ),
                Stream.map(Effect.succeed),
            ),
        );
        return {
            catalog,
            running,
            changes,
            session: SubscriptionRef.get(session),
            pressure: SubscriptionRef.get(pressure),
            target: aimed,
            setTarget,
            find,
            resolve,
            instancesOf: Effect.map(Effect.all([SubscriptionRef.get(running), aimed]), ([rows, { followed }]) => instancesOf(rows, followed)),
            current,
            ends,
            blocking,
            awaited,
        };
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
const keyOf = (secure: typeof Secure.Type) => Effect.orDie(Schema.encodeEffect(SecureKey)(secure));
const advance = (seen: Seen, { instance, change }: Event, driven: HashSet.HashSet<InstanceSnapshot['instance']>): Seen =>
    Event.fields.change.matchOrElse(
        change,
        {
            started: ({ name }) => ({ ...seen, started: UndefinedOr.match(name, { onUndefined: () => seen.started, onDefined: (announced) => Record.set(seen.started, instance, announced) }) }),
            asking: ({ container, secure }) => (secure === undefined || seen.secure.some((each) => each.instance === instance && each.container === container) ? seen : { ...seen, secure: [...seen.secure, { instance, container }] }),
            closed: ({ container }) => ({ ...seen, secure: seen.secure.filter((each) => each.instance !== instance || each.container !== container) }),
            hidden: ({ hidden }) => ({ ...seen, concealed: hidden && HashSet.has(driven, instance) ? HashSet.add(seen.concealed, instance) : HashSet.remove(seen.concealed, instance) }),
            status: ({ status }) => ({ ...seen, stalled: status._tag === 'unresponsive' ? HashSet.add(seen.stalled, instance) : HashSet.remove(seen.stalled, instance) }),
            exited: () => ({ started: Record.remove(seen.started, instance), stalled: HashSet.remove(seen.stalled, instance), concealed: HashSet.remove(seen.concealed, instance), secure: seen.secure.filter((each) => each.instance !== instance) }),
        },
        () => seen,
    );
const notice = ({ application, event: { instance, sequence, change }, seen, next, row }: Extract<Raised, { readonly _tag: 'Changed' }>, state: Context.Context<never>): ReturnType<typeof route> => {
    const aim = Context.get(state, Target);
    const read = Option.exists(Context.get(state, Indexed), (held) => held.instance === instance && sequence <= held.index.model.sequence);
    const said = (push: boolean, words: readonly string[], facts: readonly string[] = []) => ({ text: [[application, ...words].join(' '), ...facts].join(', '), push });
    const presented = ({ _tag, kind, container, title }: Extract<Event['change'], { readonly _tag: 'opened' | 'closed' }>) => (PRESENTED(kind) && !read ? Option.some(said(false, [_tag, describeContainer(kind, container, title)])) : Option.none());
    return Event.fields.change.matchOrElse(
        change,
        {
            opened: presented,
            closed: presented,
            asking: ({ _tag, kind, container, title, text, answers, secure, presentation, ...roles }) => {
                const labels = Record.fromEntries(answers.flatMap(({ ref, label }) => Array.fromNullishOr(label).map((shown) => [`${ref}`, shown] as const)));
                const buttons = Record.collect(Record.getSomes({ default: Option.fromUndefinedOr(roles.default), cancel: Option.fromUndefinedOr(roles.cancel) }), (role, ref) => [role, Address.token('action', ref), ...Option.toArray(Record.get(labels, `${ref}`)).map(Outline.named)].join(' '));
                const field = Array.fromNullishOr(secure).flatMap(({ ref, label }) => [['secure', Address.token('text', ref), ...Array.fromNullishOr(label).map(Outline.named)].join(' '), Outline.say.secure]);
                const front = Array.fromNullishOr(presentation).map((held) => (held._tag === 'forward' ? Outline.say.front : Outline.say.unfronted));
                const untargeted = Option.exists(aim.held, (held) => held.instance === instance) ? [] : [`not the target - ${openCall(application)}`];
                const subject = [`${_tag} ${describeContainer(kind, container, title)}`, ...Array.match(text, { onEmpty: Array.empty, onNonEmpty: (lines) => [Outline.named(lines.join(' '))] })].join(': ');
                const needed = [`${application}: password needed`, ...Array.fromNullishOr(title).map((shown) => `in ${Outline.named(shown)}`)].join(' ');
                return Option.some({ ...said(true, [subject], [...buttons, ...field, ...front, ...untargeted]), ...Option.getOrUndefined(Option.map(row, (key) => ({ row: { key, text: needed } }))) });
            },
            hidden: ({ hidden }) => (HashSet.has(seen.concealed, instance) === HashSet.has(next.concealed, instance) ? Option.none() : Option.some(said(false, [hidden ? 'hidden' : 'unhidden']))),
            status: ({ status }) => (HashSet.has(seen.stalled, instance) === HashSet.has(next.stalled, instance) ? Option.none() : Option.some(said(true, [status._tag], InstanceState.Status.matchOrElse(status, { unresponsive: ({ thread }) => [`main thread ${thread}`] }, Array.empty)))),
            announced: ({ _tag, text, priority }) => Option.some(said(false, [`${_tag}: ${Outline.named(text)}`], priority === 'high' ? [`priority ${priority}`] : [])),
            proven: ({ followed, drawn }) => {
                const [held, key] = [Context.get(state, Proving), `${instance} ${followed}`];
                return Option.map(Record.get(held, key), (step) => ({ ...said(true, ['proved', step], [drawn ? Outline.say.drawn : Outline.say.changed]), state: Context.add(state, Proving, Record.remove(held, key)) }));
            },
            logged: ({ _tag, entry: { messageType, subsystem, category, eventMessage } }) => Option.some(said(false, [`${[_tag, messageType.toLowerCase(), subsystem, category].filter((part) => part !== '').join(' ')}: ${eventMessage}`])),
            reported: ({ _tag, report }) =>
                Option.some(
                    said(
                        true,
                        [_tag, report.kind, fileURLToPath(report.url)],
                        Array.fromNullishOr(report.event).map((cause) => `event ${cause}`),
                    ),
                ),
            sampled: ({ _tag, report }) => Option.some(said(true, [_tag, fileURLToPath(report)])),
            exited: ({ termination }) =>
                Option.some({
                    ...(aim.holds.some((each) => Hold.$is('Quitting')(each) && each.instance === instance) ? { push: false } : said(true, [describeExit(termination)])),
                    state: Context.add(state, Target, new Aim({ ...aim, holds: aim.holds.filter((each) => each.instance !== instance) })),
                }),
        },
        () => Option.none(),
    );
};
const route = (raised: Raised, state: Context.Context<never>): Option.Option<{ readonly text?: string; readonly push: boolean; readonly row?: { readonly key: string; readonly text: string }; readonly clears?: string; readonly state?: Context.Context<never> }> => {
    const { followed } = Context.get(state, Target);
    return Raised.$match(raised, {
        Ended: ({ instance, key, left }) => (left && HashSet.has(followed, instance) ? Option.none() : Option.some({ push: false, clears: key })),
        Changed: (changed) => (HashSet.has(followed, changed.event.instance) ? notice(changed, state) : Option.none()),
    });
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
const openCall = (application: string) => `applications {action:"open", application:${JSON.stringify(application)}}`;
const refuseUntargeted = (named: Option.Option<Entry>, runs: ReturnType<typeof instancesOf>, rows: readonly InstanceSnapshot[], entries: readonly Entry[]) =>
    Option.match(
        Option.filter(named, (row) => Array.isReadonlyArrayNonEmpty(runs(row).meant)),
        {
            onSome: ({ name }) => `No application is the target yet; ${name} runs, and ${openCall(name)} makes it the target`,
            onNone: () => {
                const driven = Record.filter(byBundle(entries), ({ claim }) => Option.isNone(claim));
                const showing = rows.filter((each) => InstanceState.matchOrElse(each.state, { running: ({ windows, statusItems = 0 }) => statusItems > 0 || windows.some(Struct.get('presented')) }, Function.constFalse));
                const [shown, rest] = Array.splitAt(Array.dedupe(Array.getSomes(showing.map((each) => Option.map(Record.get(driven, each.url.href), Struct.get('name'))))), 5);
                return Array.match(shown, {
                    onEmpty: () => `No application is the target yet, and none this server drives shows a window or a status item; ${openCall('<name>')} launches one as the target`,
                    onNonEmpty: (names) => `No application is the target yet; ${Outline.list(Array.isReadonlyArrayEmpty(rest) ? names : [...names, `${rest.length} more`])} show a window or a status item, and ${openCall('<name>')} makes one the target`,
                });
            },
        },
    );
const describeUsage = ({ pendingEventAge, usage }: Running) =>
    [
        ...Array.fromNullishOr(pendingEventAge).map((age) => `oldest event unread for ${Outline.duration(Duration.seconds(age))}`),
        ...Array.fromNullishOr(usage).map(({ cause, duration }) => `${CAUSE[cause]} over ${Outline.duration(Duration.seconds(duration))}`),
        ...Array.fromNullishOr(usage).flatMap(({ diskBytesRead, diskBytesWritten, pageWait, runnable }) => [
            ...(diskBytesRead > 0 ? [`read ${ByteSize.format(ByteSize.bytes(diskBytesRead))}`] : []),
            ...(diskBytesWritten > 0 ? [`wrote ${ByteSize.format(ByteSize.bytes(diskBytesWritten))}`] : []),
            ...(pageWait > 0 ? [`waited ${Outline.duration(Duration.seconds(pageWait))} on page faults`] : []),
            ...(runnable > 0 ? [`waited ${Outline.duration(Duration.seconds(runnable))} for a processor`] : []),
        ]),
        ...Array.match(usage?.end.assertions ?? [], { onEmpty: Array.empty, onNonEmpty: (names) => [`declares ${Outline.list(names)}`] }),
    ]
        .map((part) => `, ${part}`)
        .join('');
const describeContainer = (kind: Container['kind'], container: number | undefined, title: string | undefined) => [Outline.noun(kind), ...Array.fromNullishOr(container).map((ref) => Address.token(kind, ref)), ...Array.fromNullishOr(title).map(Outline.named)].join(' ');
const describeExitStatus = InstanceState.Exited.match({ status: ({ status }) => (status === 0 ? [] : [`status ${status}`]), signal: ({ signal }) => [`signal ${signal}`], statusFailure: ({ statusFailure }) => [`status unread: ${statusFailure}`] });
const describeExit = (exit: typeof InstanceState.Exited.Type) => [['exited', ...describeExitStatus(exit)].join(' '), ...Array.fromNullishOr(exit.report).map((report) => `${report.kind} report ${fileURLToPath(report.url)}`)].join(', ');
const refuseBlocked = (name: string) => (found: Option.Option<InstanceSnapshot>) =>
    Option.match(Option.map(found, Struct.get('state')), {
        onNone: () => Result.succeed(`${name} not running`),
        onSome: InstanceState.match({
            exited: ({ termination }) => Result.succeed(`${name} ${describeExit(termination)}`),
            failed: () => Result.failVoid,
            running: (running) => {
                const stalled = InstanceState.Status.matchOrElse(running.status, { unresponsive: ({ _tag, thread }) => Result.succeed(`${name} ${_tag}, main thread ${thread}${describeUsage(running)}`) }, () => Result.failVoid);
                return Array.match(
                    running.windows.filter(Struct.get('asking')).map(({ kind = 'window', container, title }) => `${name} asking ${describeContainer(kind, container, title)}`),
                    { onEmpty: () => stalled, onNonEmpty: (dialogs) => Result.succeed(dialogs.join('\n')) },
                );
            },
        }),
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { describeUsage, type Entry, Hold, hold, Indexed, lookupEntry, Proving, untarget, Workspace };
