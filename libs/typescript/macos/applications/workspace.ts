import { isAbsolute } from 'node:path';
import { pathToFileURL } from 'node:url';
import { Handle, Server } from '@rasm/mcp/server';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Cause, Context, Effect, Equivalence, Function, HashSet, Layer, Option, Predicate, Record, Result, type Schema, Sink, Stream, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { Address } from '#/address';
import { ApplicationServer } from '#/client';
import { Event, Installation, type InstanceSnapshot, InstanceState, type Pressure, type Running, type Session } from '#/contract';

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

// --- [CONSTANTS] -----------------------------------------------------------------------

const SESSION = {
    onConsole: { normal: true, true: 'session on the console', false: 'session off the console' },
    locked: { normal: false, true: 'screen locked', false: 'screen unlocked' },
    asleep: { normal: false, true: 'display asleep', false: 'display awake' },
    secureInput: { normal: false, true: 'secure input on', false: 'secure input off' },
} as const;
const WINDOW_CHANGE: Record.ReadonlyRecord<string, string> = { created: 'opened', destroyed: 'closed', moved: 'moved', resized: 'resized', levelChanged: 'moved to its level', shown: 'showed', hidden: 'hid' };

// --- [SERVICES] ------------------------------------------------------------------------

const Target = Context.Reference<{ readonly held: Option.Option<{ readonly row: Entry; readonly instance: InstanceSnapshot['instance']; readonly language: string }>; readonly followed: HashSet.HashSet<InstanceSnapshot['instance']> }>('@rasm/applications/workspace/Target', {
    defaultValue: () => ({ held: Option.none(), followed: HashSet.empty() }),
});

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
            track('Session reading', Stream.map(server.session(), Result.map(conditions))),
            track('Pressure reading', Stream.map(server.pressure(), Result.map(strains))),
        ]);
        const refresh = Effect.catchCause(
            Effect.flatMap(listEntries, (rows) => SubscriptionRef.set(catalog, rows)),
            (cause) => Effect.logError('Application listing failed, the last listing stands', cause),
        );
        yield* Effect.forkScoped(Stream.runForEach(Stream.drop(SubscriptionRef.changes(server.installations), 1), () => refresh));
        yield* Effect.forkScoped(
            Stream.runForEach(
                Stream.mapAccum(SubscriptionRef.changes(server.installations), HashSet.empty<string>, (logged, { unreadable }) => [HashSet.union(logged, HashSet.fromIterable(unreadable.map(Struct.get('subject')))), unreadable.filter(({ subject }) => !HashSet.has(logged, subject))]),
                ({ subject, reason, message }) => Effect.logWarning(`Application bundle ${subject} is left out of the listing: ${reason}`, ...Array.fromNullishOr(message)),
            ),
        );
        yield* Effect.forkScoped(Stream.runForEach(rest, (rows) => SubscriptionRef.set(running, rows)));
        const changes = yield* Stream.share(server.events({}).pipe(successes('Modal panel scan'), logEnd('Application events')), { capacity: 'unbounded' });
        const named = yield* Stream.share(
            Stream.mapAccumEffect(changes, Record.empty<string, string>, (started, event) => Effect.map(Effect.all([SubscriptionRef.get(running), SubscriptionRef.get(catalog)]), ([rows, entries]) => nameEvent(rows, entries, started, event))),
            { capacity: 'unbounded' },
        );
        const target = Effect.flatMap(Handle, (handle) => handle.get(Target));
        yield* Server.use((served) =>
            served.events({
                stream: Stream.merge(named, Stream.merge(session.changes, pressure.changes)),
                render: (event) => ('system' in event ? Effect.succeed(event.system) : Effect.map(target, (aim) => `${event.application} ${describeEvent(event, aim)}`)),
                owns: ownsEvent,
            }),
        );
        const setTarget = Effect.fnUntraced(function* (aim: { readonly row: Entry; readonly instance: InstanceSnapshot['instance'] }) {
            const live = yield* server.watch({ instance: aim.instance });
            yield* followInstance(aim.instance);
            const [handle, aimed] = yield* Effect.all([Handle, target]);
            yield* handle.set(Target, { ...aimed, held: Option.some({ ...aim, language: live.language }) });
            return live;
        });
        const find = Effect.fnUntraced(function* (given: string) {
            const rows = yield* SubscriptionRef.get(catalog);
            const read = yield* Effect.transposeOption(Option.map(bundleAt(given), (url) => server.installation({ url })));
            const listed = new Set(rows.map((row) => row.url.href));
            const unlisted = Option.filter(read, (installation) => !backgroundOnly(installation) && !listed.has(installation.url.href));
            const offered = yield* Option.match(unlisted, {
                onNone: () => Effect.succeed(rows),
                onSome: (installation) => Effect.map(server.currentInstallations, (installed) => [...rows, ...claimed([...installed, installation], entriesOf([installation]))]),
            });
            return yield* lookupEntry(offered, Option.match(read, { onNone: () => given, onSome: ({ url }) => url.href }));
        });
        const resolve = (given: string) =>
            Effect.flatMap(find(given), (row) =>
                Option.match(row.claim, {
                    onNone: () => Effect.succeed(row),
                    onSome: ({ server: owner, scope }) => new Cause.IllegalArgumentError(`${row.name} is driven by MCP server ${owner} (${scope}), call its tools for this application`),
                }),
            );
        const targetState = Effect.gen(function* () {
            const [{ held }, rows, entries] = yield* Effect.all([target, SubscriptionRef.get(running), SubscriptionRef.get(catalog)]);
            const driven = Record.fromIterableBy(
                entries.filter(({ claim }) => Option.isNone(claim)),
                (each) => each.url.href,
            );
            const presenting = Array.getSomes(rows.filter((each) => InstanceState.matchOrElse(each.state, { running: ({ windows }) => windows.some(Struct.get('presented')) }, Function.constFalse)).map((each) => Option.all({ row: Record.get(driven, each.url.href), instance: Option.some(each.instance) })));
            const instances = Record.fromIterableBy(rows, (each) => each.instance);
            const snapshot = Option.filter(
                Option.flatMap(held, ({ instance }) => Record.get(instances, instance)),
                (each) => !InstanceState.isAnyOf(['exited'])(each.state),
            );
            const known = Option.map(Option.all({ aim: held, snapshot }), ({ aim: { row, language }, snapshot: each }): { readonly row: Entry; readonly live: InstanceSnapshot & { readonly language: string } } => ({ row, live: { ...each, language } }));
            return { presenting, known, sole: Option.orElse(held, () => (presenting.length === 1 ? Array.head(presenting) : Option.none())) };
        });
        const current = Effect.gen(function* () {
            const { presenting, known, sole } = yield* targetState;
            const names = presenting.map((each) => each.row.name);
            const aim = yield* Effect.fromOption(sole, () => new Cause.NoSuchElementError(`No application is the target of this handle and ${names.length === 0 ? 'none runs' : `${names.join(', ')} run`}, applications open makes one the target`));
            return yield* Option.match(known, {
                onSome: Effect.succeed,
                onNone: () =>
                    setTarget(aim).pipe(
                        Effect.filterOrFail(
                            (snapshot) => !InstanceState.isAnyOf(['exited'])(snapshot.state),
                            () => new Cause.NoSuchElementError(`${aim.row.name} instance ${aim.instance} no longer runs, applications open starts or targets it again`),
                        ),
                        Effect.map((live) => ({ row: aim.row, live })),
                    ),
            });
        });
        const prompted = Stream.filterMap(named, ({ application, event }) =>
            Event.fields.change.matchOrElse(event.change, { asking: ({ kind, container, title }) => (kind === 'prompt' ? Result.succeed(Effect.succeed(`${application} prompt ${Address.token(kind, container)}${titled(title)} asks for an answer`)) : Result.failVoid) }, () => Result.failVoid),
        );
        const blocked = (name: string, holds: Predicate.Predicate<InstanceSnapshot>) => SubscriptionRef.changes(running).pipe(Stream.map(Array.findFirst(holds)), Stream.filterMap(refuseBlocked(name)), Stream.map(Effect.succeed));
        const blocking = (row: Pick<Entry, 'url' | 'name'>) => Context.make(Server.EarlyReturn, Stream.merge(blocked(row.name, Installation.runs(row)), prompted));
        const ends = Context.make(Server.EarlyReturn, Stream.merge(prompted, Stream.unwrap(Effect.map(targetState, ({ sole }) => Option.match(sole, { onNone: () => Stream.never, onSome: ({ row, instance }) => blocked(row.name, (each) => each.instance === instance) })))));
        return { catalog, running, changes, session: session.state, pressure: pressure.state, target, setTarget, find, resolve, current, ends, blocking };
    }),
}) {
    static readonly layer = <E, R>(cataloguer: Cataloguer<E, R>) => Layer.effect(Workspace, Workspace.make(cataloguer));
    static readonly fromInstallations = (offered: readonly Installation[]) => Effect.succeed(entriesOf(offered));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const follows = (instance: InstanceSnapshot['instance'], state: Context.Context<never>) => HashSet.has(Context.get(state, Target).followed, instance);
const followInstance = Effect.fnUntraced(function* (instance: InstanceSnapshot['instance']) {
    const handle = yield* Handle;
    const aimed = yield* handle.get(Target);
    yield* handle.set(Target, { ...aimed, followed: HashSet.add(aimed.followed, instance) });
});
const ownsEvent = (event: { readonly system: string } | { readonly application: string; readonly event: Event }, state: Context.Context<never>) => 'system' in event || prompting(event.event.change) || follows(event.event.instance, state);
const lookupEntry = (rows: readonly Entry[], query: string) =>
    Unmatched.matching(rows, 'listed application', Option.some(query), (row: Entry) => [row.name, row.product, row.identifier, ...row.aliases, row.url.href]).pipe(
        Effect.catch((error) => Array.match(Unmatched.rank(query, rows, Struct.get('name')), { onEmpty: () => error, onNonEmpty: Effect.succeed })),
        Effect.map((matched) =>
            Array.dedupeWith(
                Array.intersection(rows, matched),
                Equivalence.mapInput(Equivalence.String, (row: Entry) => row.product),
            ),
        ),
        Effect.filterOrFail(Predicate.isTupleOf(1), (forms) => Unmatched.of({ subject: 'single listed application', query: Option.some(query), candidates: forms.map(Struct.get('name')) })),
        Effect.map(Array.headNonEmpty),
    );
const bundleAt = (given: string) => (URL.canParse(given) ? Option.filter(Option.some(new URL(given)), ({ protocol }) => protocol === 'file:') : Option.map(Option.liftPredicate(given, isAbsolute), pathToFileURL));
const covers =
    (identifier: string) =>
    ({ prefixes }: Option.Option.Value<Installation['claim']>) =>
        prefixes.some((prefix) => identifier.startsWith(prefix));
const claimed = (installed: readonly Installation[], rows: readonly Omit<Entry, 'claim'>[]) => {
    const claims = Array.getSomes(installed.map(Struct.get('claim')));
    return rows.map((row): Entry => ({ ...row, claim: Array.findFirst(claims, covers(row.identifier)) }));
};
const entriesOf = (offered: readonly Installation[]) => offered.map(({ url, identifier, name, bundleName, version }) => ({ url, identifier, name, product: identifier, aliases: Option.toArray(bundleName), facts: { identifier, ...Record.getSomes({ version }) } }));
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
const strains = ({ memory, available, thermal, lowPower, advisory }: typeof Pressure.Type): readonly { readonly normal: boolean; readonly text: string }[] => [
    { normal: memory === 'normal', text: `memory pressure ${memory} with ${available}% available` },
    { normal: thermal === 'nominal', text: `thermal state ${thermal}` },
    { normal: !lowPower, text: `low power mode ${lowPower ? 'on' : 'off'}` },
    ...UndefinedOr.match(advisory, { onUndefined: () => [{ normal: false, text: 'load advisory unread' }], onDefined: Record.collect((key, level) => ({ normal: level !== 'bad', text: `load advisory ${key} ${level}` })) }),
];
const conditions = (session: Session): ReturnType<typeof strains> => [...Struct.keys(SESSION).map((key) => ({ normal: session[key] === SESSION[key].normal, text: SESSION[key][`${session[key]}`] })), { normal: true, text: `current Space ${session.spaces.join(', ')}` }];
const track = Effect.fnUntraced(function* <X>(subject: string, read: Stream.Stream<Result.Result<ReturnType<typeof strains>, Error>, X>) {
    const held = yield* SubscriptionRef.make<ReturnType<typeof strains>>([]);
    yield* Effect.forkScoped(Stream.runForEach(read.pipe(successes(subject), logEnd(subject)), (lines) => SubscriptionRef.set(held, lines)));
    return {
        state: Effect.map(SubscriptionRef.get(held), (lines) => lines.filter(({ normal }) => !normal).map(Struct.get('text'))),
        changes: SubscriptionRef.changes(held).pipe(
            Stream.map((lines) => lines.map(Struct.get('text'))),
            Stream.filter(Array.isReadonlyArrayNonEmpty),
            Stream.zipWithPrevious,
            Stream.map(([before, after]) => Array.difference(after, Option.getOrElse(before, Function.constant(after)))),
            Stream.filter(Array.isReadonlyArrayNonEmpty),
            Stream.map((changed) => ({ system: changed.join(', ') })),
        ),
    };
});
const nameEvent = (rows: readonly InstanceSnapshot[], entries: readonly Entry[], started: Record.ReadonlyRecord<string, string>, event: Event) => {
    const announced = Event.fields.change.matchOrElse(event.change, { started: ({ name }) => Option.fromUndefinedOr(name) }, Option.none);
    const application = Array.findFirst(rows, (row) => row.instance === event.instance).pipe(
        Option.flatMap(({ url }) => Array.findFirst(entries, (entry) => entry.url.href === url.href)),
        Option.map(Struct.get('name')),
        Option.orElse(() => Record.get(started, event.instance)),
        Option.orElse(() => announced),
        Option.getOrElse(() => event.instance),
    );
    const kept = Event.fields.change.guards.exited(event.change) ? Record.remove(started, event.instance) : Option.match(announced, { onNone: () => started, onSome: (name) => Record.set(started, event.instance, name) });
    return [kept, [{ application, event }]] as const;
};
const describeUsage = ({ usage }: Running) => Array.match(usage?.assertions ?? [], { onEmpty: () => '', onNonEmpty: (names) => `, declares ${names.join(', ')}` });
const prompting = Event.fields.change.matchOrElse({ asking: ({ kind }) => kind === 'prompt', closed: ({ kind }) => kind === 'prompt' }, Function.constFalse);
const titled = (title: string | undefined) => UndefinedOr.match(title, { onUndefined: () => '', onDefined: (named) => ` "${named}"` });
const describeExitStatus = InstanceState.Exited.match({ status: ({ status }) => `with status ${status}`, signal: ({ signal }) => `on signal ${signal}`, statusFailure: () => 'with unread status' });
const describeExit = (exit: typeof InstanceState.Exited.Type) => `exited ${describeExitStatus(exit)}${UndefinedOr.match(exit.report, { onUndefined: () => '', onDefined: (report) => `, ${report.kind} report ${report.url.href}` })}`;
const describeEvent = ({ application, event: { instance, change } }: { readonly application: string; readonly event: Event }, { held, followed }: Context.Service.Shape<typeof Target>) =>
    Event.fields.change.match(change, {
        started: () => 'started',
        opened: (opened) => `opened ${opened.kind} ${Address.token(opened.kind, opened.container)}${titled(opened.title)}`,
        closed: (closed) => `closed ${closed.kind} ${Address.token(closed.kind, closed.container)}${titled(closed.title)}`,
        asking: (asking) => {
            const driven = HashSet.has(followed, instance);
            const answers = asking.answers.flatMap(({ ref, label }) => {
                const roles = (['default', 'cancel', 'close'] as const).filter((role) => asking[role] === ref).map((role) => `(${role})`);
                return driven ? [[...Array.fromNullishOr(label), ...roles, Address.token('action', ref)].join(' ')] : Array.fromNullishOr(label).map((text) => [text, ...roles].join(' '));
            });
            const reach = Option.exists(held, (aim) => aim.instance === instance) ? '' : ` once applications open ${application} makes it the target`;
            const answering = Array.match(answers, {
                onEmpty: () => (driven ? `inspect reads its answers${reach}` : 'the user answers it'),
                onNonEmpty: (listed) => (driven ? `act answers with ${listed.join(', ')}${reach}` : `the user answers with ${listed.join(', ')}`),
            });
            return `${asking.kind} ${Address.token(asking.kind, asking.container)}${titled(asking.title)} asks${Array.match(asking.text, { onEmpty: () => '', onNonEmpty: (lines) => `: ${lines.join(' ')}` })}; ${answering}`;
        },
        focused: (focused) => `focused ${Address.token(focused.kind, focused.control)}`,
        value: (value) => `${Address.token(value.kind, value.control)} now ${Address.fact(value.kind, value.fact)}`,
        selection: (selection) => `${Address.token(selection.kind, selection.control)} ${{ attribute: 'selected', marked: 'selected (inferred)' }[selection.evidence]}`,
        moved: (moved) => `moved ${moved.kind} ${Address.token(moved.kind, moved.container)} to ${moved.frame[0].join(',')} ${moved.frame[1].join('×')}`,
        frontmost: (frontmost) => (frontmost.active ? 'came to the front' : 'went behind'),
        hidden: (hidden) => (hidden.hidden ? 'was hidden' : 'was unhidden'),
        window: ({ change: notification, window }) =>
            [
                `${Option.getOrElse(Record.get(WINDOW_CHANGE, notification), Function.constant(notification))} ${window.band} window ${window.number}${titled(window.title)}`,
                `at ${window.frame[0].join(',')} ${window.frame[1].join('×')}`,
                ...Array.fromNullishOr(window.parent).map((parent) => `child of window ${parent}`),
                ...(window.shared ? [] : ['refuses capture']),
            ].join(', '),
        status: ({ status }) => InstanceState.Status.matchOrElse(status, { unresponsive: ({ thread }) => `unresponsive, main thread ${thread}` }, ({ _tag }) => _tag),
        exited: ({ termination }) => describeExit(termination),
    });
const refuseBlocked = (name: string) => (found: Option.Option<InstanceSnapshot>) =>
    Option.match(Option.map(found, Struct.get('state')), {
        onNone: () => Result.succeed(`${name} no longer runs`),
        onSome: InstanceState.match({
            exited: ({ termination }) => Result.succeed(`${name} ${describeExit(termination)}`),
            failed: () => Result.failVoid,
            running: (running) => {
                const stalled = InstanceState.Status.matchOrElse(running.status, { unresponsive: ({ thread }) => Result.succeed(`${name} stopped responding, main thread ${thread}${describeUsage(running)}`) }, () => Result.failVoid);
                return Array.match(
                    running.windows.filter(Struct.get('asking')).map(({ kind = 'window', container, number, title }) => `${name} ${kind} ${container === undefined ? number : Address.token(kind, container)}${titled(title)} asks for an answer: inspect reads it and act answers it`),
                    { onEmpty: () => stalled, onNonEmpty: (dialogs) => Result.succeed(dialogs.join('; ')) },
                );
            },
        }),
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { describeUsage, type Entry, followInstance, follows, lookupEntry, ownsEvent, strains, Workspace };
