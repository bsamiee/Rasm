import { isAbsolute } from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';
import { Handles } from '@rasm/mcp/handles';
import { Handle, Server } from '@rasm/mcp/server';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, ByteSize, Cause, Context, Data, DateTime, Duration, Effect, Equivalence, Function, HashSet, Layer, Option, Predicate, Queue, Record, Ref, Result, Schema, SchemaGetter, Sink, Stream, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { Address } from '#/address';
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
    readonly prompts: Record.ReadonlyRecord<string, HashSet.HashSet<number>>;
    readonly opens: Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, number>>;
}
interface Routed {
    readonly application: string;
    readonly event: Event;
    readonly render: Effect.Effect<string>;
    readonly leads: () => boolean;
    readonly delivers: Option.Option<typeof Handles.Kind.Type>;
    readonly standing: Option.Option<typeof Handles.Standing.Type>;
}
type Cataloguer<E, R> = (offered: readonly Installation[]) => Effect.Effect<readonly Omit<Entry, 'claim'>[], E, R>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const CAUSE = { running: 'computing', runnable: 'waiting for a processor', paging: 'paging', reading: 'reading disk', writing: 'writing disk', idle: 'idle' } as const satisfies Record.ReadonlyRecord<NonNullable<Running['usage']>['cause'], string>;
const PRESENTED = Schema.is(Container.fields.kind.pick(['window', 'dialog', 'panel', 'sheet', 'popover', 'drawer', 'prompt']));
const UNSEEN: Seen = { started: Record.empty(), stalled: HashSet.empty(), concealed: HashSet.empty(), prompts: Record.empty(), opens: Record.empty() };
const CHOICES = new Intl.ListFormat('en', { type: 'disjunction' });

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
class Aim extends Data.Class<{
    readonly held: Option.Option<{ readonly row: Entry; readonly instance: InstanceSnapshot['instance']; readonly language: string }>;
    readonly followed: HashSet.HashSet<InstanceSnapshot['instance']>;
    readonly named: Option.Option<Entry>;
}> {
    static readonly empty = new Aim({ held: Option.none(), followed: HashSet.empty(), named: Option.none() });
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
                        const [alive, gone] = Array.partition(followed, (each) => (Record.has(runningOf(instances), each.instance) ? Result.succeed(each) : Result.fail(each)));
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
                        return new Aim({ held: Option.flatten(aimed), followed: kept, named: Option.flatten(last) });
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
const Shown = Context.Reference<Record.ReadonlyRecord<string, HashSet.HashSet<number>>>('@rasm/applications/workspace/Shown', { defaultValue: () => Record.empty() });
const Proving = Context.Reference<Record.ReadonlyRecord<string, string>>('@rasm/applications/workspace/Proving', { defaultValue: () => Record.empty() });
const sight = Effect.fnUntraced(function* (instance: string, opening: number, open: boolean) {
    const handle = yield* Handle;
    const all = yield* handle.get(Shown);
    const held = Option.getOrElse(Record.get(all, instance), HashSet.empty<number>);
    yield* handle.set(Shown, Record.set(all, instance, open ? HashSet.add(held, opening) : HashSet.remove(held, opening)));
    return HashSet.has(held, opening);
});

class Workspace extends Context.Service<Workspace>()('@rasm/applications/workspace', {
    make: Effect.fnUntraced(function* <E, R>(cataloguer: Cataloguer<E, R>) {
        const server = yield* ApplicationServer;
        const listEntries = Effect.gen(function* () {
            const installed = yield* server.currentInstallations;
            return claimed(installed, yield* cataloguer(installed.filter(Predicate.not(backgroundOnly))));
        });
        const [first, rest] = yield* Stream.peel(server.instances({}).pipe(successes('Instance listing')), Sink.head());
        const [catalog, running, session, pressure, tables, vanished, driven, exits, latest] = yield* Effect.all([
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
            Queue.unbounded<Routed>(),
            Ref.make(UNSEEN),
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
        const named = yield* Stream.share(
            changes.pipe(
                Stream.filterMapEffect(
                    Effect.fnUntraced(function* (event: Event) {
                        const [instances, following, seen] = yield* Effect.all([SubscriptionRef.get(running), Ref.get(driven), Ref.get(latest)]);
                        const next = advance(seen, event, following);
                        yield* Ref.set(latest, next);
                        const application = Option.map(Record.get(byInstance(instances), event.instance), (each) => each.name ?? each.identifier).pipe(
                            Option.orElse(() => Record.get(next.started, event.instance)),
                            Option.orElse(() => Record.get(seen.started, event.instance)),
                            Option.getOrElse(() => 'a process outside the Dock'),
                        );
                        return Result.fromOption(route(seen, next, application, event, latest), Function.constVoid);
                    }),
                ),
            ),
            { capacity: 'unbounded' },
        );
        yield* Server.persist(Target, Aim.stored({ server, catalog, running, tables, vanished }));
        yield* Server.use((served) =>
            served.events({
                stream: named.pipe(Stream.merge(Stream.fromQueue(exits))),
                render: (each) => each.render,
                owns: ownsEvent,
                leads: (each) => each.leads(),
                delivers: (each) => each.delivers,
                standing: (each) => each.standing,
            }),
        );
        const { states } = yield* Server;
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
                    yield* Queue.offerAll(exits, Array.getSomes(Record.collect(due, (_, { instance, name }) => route(UNSEEN, UNSEEN, name, Event.make({ instance, sequence: 0, at, change: { _tag: 'exited', termination: { _tag: 'statusFailure', statusFailure: 'the server restarted meanwhile' } } }), latest))));
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
            yield* follow(aim.instance, true);
            const [handle, before] = yield* Effect.all([Handle, aimed]);
            yield* handle.set(Target, new Aim({ ...before, held: Option.some({ ...aim, language: live.language }) }));
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
            const [handle, before] = yield* Effect.all([Handle, aimed]);
            yield* handle.set(Target, new Aim({ ...before, named: Option.some(row) }));
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
        const prompted = named.pipe(
            Stream.filterMap(({ application, event: { instance, change } }) => Event.fields.change.matchOrElse(change, { asking: ({ kind, container, title }) => (kind === 'prompt' ? Result.succeed({ application, instance, shown: describeContainer(kind, container, title) }) : Result.failVoid) }, () => Result.failVoid)),
            Stream.map(({ application, instance, shown }) => Effect.map(aimed, ({ followed }) => (HashSet.has(followed, instance) ? `${application} ${shown} ${Outline.say.asking}` : `${application} prompt ${Outline.say.asking}, the user answers it`))),
        );
        const blocked = (name: string, holds: Predicate.Predicate<InstanceSnapshot>) => SubscriptionRef.changes(running).pipe(Stream.map(Array.findFirst(holds)), Stream.filterMap(refuseBlocked(name)), Stream.map(Effect.succeed));
        const blocking = (row: Pick<Entry, 'url' | 'name'>) => Context.make(Server.EarlyReturn, Stream.merge(blocked(row.name, Installation.runs(row)), prompted));
        const ends = Context.make(Server.EarlyReturn, Stream.merge(prompted, Stream.unwrap(Effect.map(aimed, ({ held }) => Option.match(held, { onNone: () => Stream.never, onSome: ({ row, instance }) => blocked(row.name, (each) => each.instance === instance) })))));
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
const follow = Effect.fnUntraced(function* (instance: InstanceSnapshot['instance'], kept: boolean) {
    const [handle, before] = yield* Effect.all([Handle, aimed]);
    yield* handle.set(Target, new Aim({ ...before, followed: kept ? HashSet.add(before.followed, instance) : HashSet.remove(before.followed, instance) }));
});
const byIdentifier = Function.memoize((rows: readonly InstanceSnapshot[]) => Array.groupBy(Record.values(runningOf(rows)), (each) => each.identifier));
const instancesOf = (rows: readonly InstanceSnapshot[], followed: HashSet.HashSet<InstanceSnapshot['instance']>) => (row: Pick<Entry, 'url' | 'identifier'>) => {
    const [own, apart] = Array.partition(Option.getOrElse(Record.get(byIdentifier(rows), row.identifier), Array.empty), (each) => (each.url.href === row.url.href ? Result.succeed(each) : Result.fail(each)));
    const [driven, foreign] = Array.partition(apart, (each) => (HashSet.has(followed, each.instance) ? Result.succeed(each) : Result.fail(each)));
    return { meant: Array.isReadonlyArrayNonEmpty(own) ? own : driven, apart: foreign };
};
const ownsEvent = ({ event: { instance } }: { readonly event: Pick<Event, 'instance'> }, state: Context.Context<never>) => HashSet.has(Context.get(state, Target).followed, instance);
const advance = (seen: Seen, { instance, sequence, change }: Event, driven: HashSet.HashSet<InstanceSnapshot['instance']>): Seen => {
    const [prompts, opens] = [Option.getOrElse(Record.get(seen.prompts, instance), HashSet.empty<number>), Option.getOrElse(Record.get(seen.opens, instance), Record.empty<string, number>)];
    return Event.fields.change.matchOrElse(
        change,
        {
            started: ({ name }) => ({ ...seen, started: UndefinedOr.match(name, { onUndefined: () => seen.started, onDefined: (announced) => Record.set(seen.started, instance, announced) }) }),
            opened: ({ container }) => ({ ...seen, opens: Record.set(seen.opens, instance, Record.set(opens, `${container}`, sequence)) }),
            asking: ({ kind, container }) => (kind === 'prompt' ? { ...seen, prompts: Record.set(seen.prompts, instance, HashSet.add(prompts, container)) } : seen),
            closed: ({ kind, container }) => ({ ...seen, opens: Record.set(seen.opens, instance, Record.remove(opens, `${container}`)), prompts: kind === 'prompt' ? Record.set(seen.prompts, instance, HashSet.remove(prompts, container)) : seen.prompts }),
            hidden: ({ hidden }) => ({ ...seen, concealed: hidden && HashSet.has(driven, instance) ? HashSet.add(seen.concealed, instance) : HashSet.remove(seen.concealed, instance) }),
            status: ({ status }) => ({ ...seen, stalled: status._tag === 'unresponsive' ? HashSet.add(seen.stalled, instance) : HashSet.remove(seen.stalled, instance) }),
            exited: () => ({ started: Record.remove(seen.started, instance), stalled: HashSet.remove(seen.stalled, instance), concealed: HashSet.remove(seen.concealed, instance), prompts: Record.remove(seen.prompts, instance), opens: Record.remove(seen.opens, instance) }),
        },
        () => seen,
    );
};
const route = (seen: Seen, next: Seen, application: string, event: Event, latest: Ref.Ref<Seen>): Option.Option<Routed> => {
    const { instance, sequence, change } = event;
    const lead = (line: Effect.Effect<string>, stated: Partial<Pick<Routed, 'delivers' | 'standing' | 'leads'>> = {}) => Option.some<Routed>({ application, event, render: Effect.map(line, (text) => `${application} ${text}`), delivers: Option.none(), standing: Option.none(), leads: Function.constTrue, ...stated });
    const settled = Option.some(Handles.Standing.cases.Settled.make({ key: instance }));
    const unled = Option.none<Routed>();
    return Event.fields.change.match(change, {
        started: () => unled,
        opened: ({ kind, container, title }) =>
            PRESENTED(kind) ? lead(Effect.as(sight(instance, sequence, true), `opened ${describeContainer(kind, container, title)}`), { leads: () => Option.exists(Record.get(Ref.getUnsafe(latest).opens, instance), (open) => Option.contains(Record.get(open, `${container}`), sequence)) }) : unled,
        closed: ({ kind, container, title }) => {
            const shown = describeContainer(kind, container, title);
            const line = Option.match(Option.flatMap(Record.get(seen.opens, instance), Record.get(`${container}`)), {
                onNone: () => Effect.succeed(`closed ${shown}`),
                onSome: (opening) => Effect.map(sight(instance, opening, false), (seenOpen) => (seenOpen ? `closed ${shown}` : `${shown} opened and closed`)),
            });
            return PRESENTED(kind) ? lead(line, { standing: kind === 'prompt' && Option.exists(Record.get(next.prompts, instance), HashSet.isEmpty) ? settled : Option.none() }) : unled;
        },
        asking: (asking) => {
            const answers = asking.answers.map(({ ref, label }) => [...Array.fromNullishOr(label).map(Outline.named), Address.token('action', ref), ...(['default', 'cancel', 'close'] as const).filter((role) => asking[role] === ref).map((role) => `(${role})`)].join(' '));
            const asks = [
                [`${describeContainer(asking.kind, asking.container, asking.title)} ${Outline.say.asking}`, ...Array.match(asking.text, { onEmpty: Array.empty, onNonEmpty: (lines) => [Outline.named(lines.join(' '))] })].join(': '),
                Array.match(answers, { onEmpty: () => 'inspect reads its answers', onNonEmpty: (listed) => `act answers with ${CHOICES.format(listed)}` }),
                ...Array.fromNullishOr(asking.secure).map(({ ref, label }) => [`secure field ${[...Array.fromNullishOr(label).map(Outline.named), Address.token('text', ref)].join(' ')}`, Outline.say.secure].join(', ')),
                ...Array.fromNullishOr(asking.presentation).map((held) => (held._tag === 'forward' ? Outline.say.front : Outline.say.unfronted)),
            ].join(', ');
            return lead(
                Effect.map(aimed, ({ held }) => (Option.exists(held, (aim) => aim.instance === instance) ? asks : `${asks} once ${openCall(application)} makes it the target`)),
                asking.kind === 'prompt' ? { standing: Option.some(Handles.Standing.cases.Waiting.make({ key: instance })) } : { delivers: Option.some('asks') },
            );
        },
        focused: () => unled,
        value: () => unled,
        selection: () => unled,
        moved: () => unled,
        frontmost: () => unled,
        hidden: ({ hidden }) => (HashSet.has(seen.concealed, instance) === HashSet.has(next.concealed, instance) ? unled : lead(Effect.succeed(hidden ? 'was hidden' : 'was unhidden'))),
        window: () => unled,
        status: ({ status }) =>
            HashSet.has(seen.stalled, instance) === HashSet.has(next.stalled, instance)
                ? unled
                : lead(Effect.succeed(InstanceState.Status.matchOrElse(status, { unresponsive: ({ thread }) => `is not responding, main thread ${thread}` }, () => 'responds again')), { delivers: HashSet.has(next.stalled, instance) ? Option.some('hung') : Option.none() }),
        announced: ({ text, priority }) => lead(Effect.succeed([`announced: ${Outline.named(text)}`, ...(priority === 'high' ? ['high priority'] : [])].join(', '))),
        dragging: () => unled,
        proven: ({ followed, drawn }) =>
            lead(
                Effect.gen(function* () {
                    const handle = yield* Handle;
                    const held = yield* handle.get(Proving);
                    const key = `${instance} ${followed}`;
                    yield* handle.set(Proving, Record.remove(held, key));
                    return `proved ${Option.getOrElse(Record.get(held, key), () => 'posted step')}: ${drawn ? Outline.say.drawn : Outline.say.changed}`;
                }),
            ),
        logged: ({ entry: { messageType, subsystem, category, eventMessage } }) =>
            lead(
                Effect.succeed(
                    `${[
                        `logged ${messageType.toLowerCase()}`,
                        ...Array.match(
                            [subsystem, category].filter((part) => part !== ''),
                            { onEmpty: Array.empty, onNonEmpty: (parts) => [`in ${parts.join(' ')}`] },
                        ),
                    ].join(' ')}: ${eventMessage}`,
                ),
            ),
        reported: ({ report }) => lead(Effect.succeed([`wrote ${report.kind} report`, ...Array.fromNullishOr(report.event).map((cause) => `for ${cause}`), `at ${fileURLToPath(report.url)}`].join(' ')), { delivers: Option.some(report.kind === 'crash' ? 'crashed' : 'hung') }),
        sampled: ({ report }) => lead(Effect.succeed(`was sampled while not responding, report at ${fileURLToPath(report)}`), { delivers: Option.some('hung') }),
        exited: ({ termination }) => lead(Effect.as(follow(instance, false), describeExit(termination)), { delivers: Option.some(termination._tag === 'signal' || termination.report?.kind === 'crash' || (termination._tag === 'status' && termination.reporting === true) ? 'crashed' : 'exited'), standing: settled }),
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
const describeExitStatus = InstanceState.Exited.match({ status: ({ status }) => (status === 0 ? [] : [`with status ${status}`]), signal: ({ signal }) => [`on signal ${signal}`], statusFailure: ({ statusFailure }) => [`with its status unread: ${statusFailure}`] });
const describeExit = (exit: typeof InstanceState.Exited.Type) => [['exited', ...describeExitStatus(exit)].join(' '), ...Array.fromNullishOr(exit.report).map((report) => `${report.kind} report at ${fileURLToPath(report.url)}`)].join(', ');
const refuseBlocked = (name: string) => (found: Option.Option<InstanceSnapshot>) =>
    Option.match(Option.map(found, Struct.get('state')), {
        onNone: () => Result.succeed(`${name} no longer runs`),
        onSome: InstanceState.match({
            exited: ({ termination }) => Result.succeed(`${name} ${describeExit(termination)}`),
            failed: () => Result.failVoid,
            running: (running) => {
                const stalled = InstanceState.Status.matchOrElse(running.status, { unresponsive: ({ thread }) => Result.succeed(`${name} is not responding, main thread ${thread}${describeUsage(running)}`) }, () => Result.failVoid);
                return Array.match(
                    running.windows.filter(Struct.get('asking')).map(({ kind = 'window', container, title }) => `${name} ${describeContainer(kind, container, title)} ${Outline.say.asking}, inspect reads it and act answers it`),
                    { onEmpty: () => stalled, onNonEmpty: (dialogs) => Result.succeed(dialogs.join('; ')) },
                );
            },
        }),
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { describeUsage, type Entry, follow, lookupEntry, ownsEvent, Proving, Workspace };
