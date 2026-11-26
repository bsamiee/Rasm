import { isAbsolute } from 'node:path';
import { pathToFileURL } from 'node:url';
import { Handle, Server } from '@rasm/mcp/server';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Cause, Context, Effect, Function, HashSet, Layer, Option, Predicate, Record, Result, Schema, Sink, Stream, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { Address } from '#/address';
import { ApplicationServer } from '#/client';
import { Event, Installation, type InstanceSnapshot, InstanceState, Pressure, type Running, type Session } from '#/contract';

// --- [TYPES] ---------------------------------------------------------------------------

interface Entry {
    readonly url: URL;
    readonly identifier: string;
    readonly name: string;
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

// --- [SERVICES] ------------------------------------------------------------------------

const Target = Context.Reference<{ readonly held: Option.Option<{ readonly row: Entry; readonly instance: InstanceSnapshot['instance'] }>; readonly followed: HashSet.HashSet<InstanceSnapshot['instance']> }>('@rasm/applications/workspace/Target', {
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
        const [catalog, running] = yield* Effect.all([
            Effect.flatMap(listEntries, (rows) => SubscriptionRef.make(rows)),
            Effect.flatMap(
                Effect.fromOption(first, () => new Cause.NoSuchElementError('instances ended before its first value')),
                (rows) => SubscriptionRef.make(rows),
            ),
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
        const pressures = server.pressure().pipe(
            Stream.filterMap(Function.identity),
            Stream.changesWith(Schema.toEquivalence(Pressure)),
            Stream.drop(1),
            Stream.map((pressure) => ({ system: strains(pressure).map(Struct.get('text')).join(', ') })),
            logEnd('Pressure readings'),
        );
        const sessions = server.session().pipe(
            Stream.zipWithPrevious,
            Stream.map(([before, after]) => Option.match(before, { onNone: Array.empty<string>, onSome: (held) => [...said(after, (key) => held[key] !== after[key]), ...(held.space === after.space ? [] : ['active space changed'])] })),
            Stream.filter(Array.isReadonlyArrayNonEmpty),
            Stream.map((changed) => ({ system: changed.join(', ') })),
            logEnd('Session readings'),
        );
        const session = Effect.map(Stream.runHead(server.session()), Option.match({ onNone: Array.empty, onSome: (held) => said(held, (key) => held[key] !== SESSION[key].normal) }));
        const pressure = Effect.map(
            Stream.runHead(Stream.filterMap(server.pressure(), Function.identity)),
            Option.match({
                onNone: Array.empty,
                onSome: (held) =>
                    strains(held)
                        .filter(({ normal }) => !normal)
                        .map(Struct.get('text')),
            }),
        );
        const target = Effect.flatMap(Handle, (handle) => handle.get(Target));
        const events = Stream.merge(named, Stream.merge(pressures, sessions));
        yield* Server.use((served) =>
            served.events({
                stream: events,
                render: (event) => ('system' in event ? Effect.succeed(event.system) : Effect.map(target, (aim) => `${event.application} ${describeEvent(event, aim)}`)),
                owns: (event, state) => 'system' in event || prompting(event.event.change) || follows(event.event.instance, state),
            }),
        );
        const setTarget = Effect.fnUntraced(function* (aim: { readonly row: Entry; readonly instance: InstanceSnapshot['instance'] }) {
            const [handle, { followed }, live] = yield* Effect.all([Handle, target, server.watch({ instance: aim.instance })]);
            yield* handle.set(Target, { held: Option.some(aim), followed: HashSet.add(followed, aim.instance) });
            return live;
        });
        const find = Effect.fnUntraced(function* (given: string) {
            const [rows, installed, read] = yield* Effect.all([SubscriptionRef.get(catalog), server.currentInstallations, Effect.transposeOption(Option.map(bundleAt(given), (url) => server.installation({ url })))]);
            const listed = new Set(rows.map((row) => row.url.href));
            const unlisted = Option.toArray(Option.filter(read, (installation) => !backgroundOnly(installation) && !listed.has(installation.url.href)));
            const query = Option.match(read, { onNone: () => given, onSome: ({ url }) => url.href });
            return yield* Unmatched.single([...rows, ...claimed([...installed, ...unlisted], entriesOf(unlisted))], 'listed application', Option.some(query), (row: Entry) => [row.name, ...row.aliases, row.url.href]);
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
            return { presenting, sole: Option.orElse(held, () => (presenting.length === 1 ? Array.head(presenting) : Option.none())) };
        });
        const current = Effect.gen(function* () {
            const { presenting, sole } = yield* targetState;
            const names = presenting.map((each) => each.row.name);
            const aim = yield* Effect.fromOption(sole, () => new Cause.NoSuchElementError(`No application is the target of this handle and ${names.length === 0 ? 'none runs' : `${names.join(', ')} run`}, applications open makes one the target`));
            const live = yield* Effect.filterOrFail(
                setTarget(aim),
                (snapshot) => !InstanceState.isAnyOf(['exited'])(snapshot.state),
                () => new Cause.NoSuchElementError(`${aim.row.name} instance ${aim.instance} no longer runs, applications open starts or targets it again`),
            );
            return { row: aim.row, live };
        });
        const prompted = Stream.filterMap(named, ({ application, event }) =>
            Event.fields.change.matchOrElse(event.change, { asking: ({ kind, container, title }) => (kind === 'prompt' ? Result.succeed(Effect.succeed(`${application} prompt ${Address.token(kind, container)}${titled(title)} asks for an answer`)) : Result.failVoid) }, () => Result.failVoid),
        );
        const blocked = (row: Pick<Entry, 'url' | 'name'>, instance?: InstanceSnapshot['instance']) =>
            SubscriptionRef.changes(running).pipe(Stream.map(Array.findFirst((each: InstanceSnapshot) => (instance === undefined ? Installation.runs(row)(each) : each.instance === instance))), Stream.filterMap(refuseBlocked(row.name)), Stream.map(Effect.succeed));
        const blocking = (row: Pick<Entry, 'url' | 'name'>) => Context.make(Server.EarlyReturn, Stream.merge(blocked(row), prompted));
        const ends = Context.make(Server.EarlyReturn, Stream.merge(prompted, Stream.unwrap(Effect.map(targetState, ({ sole }) => Option.match(sole, { onNone: () => Stream.never, onSome: ({ row, instance }) => blocked(row, instance) })))));
        return { catalog, running, changes, session, pressure, target, setTarget, find, resolve, current, ends, blocking };
    }),
}) {
    static readonly layer = <E, R>(cataloguer: Cataloguer<E, R>) => Layer.effect(Workspace, Workspace.make(cataloguer));
    static readonly fromInstallations = (offered: readonly Installation[]) => Effect.succeed(entriesOf(offered));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const follows = (instance: InstanceSnapshot['instance'], state: Context.Context<never>) => HashSet.has(Context.get(state, Target).followed, instance);
const bundleAt = (given: string) => (URL.canParse(given) ? Option.filter(Option.some(new URL(given)), ({ protocol }) => protocol === 'file:') : Option.map(Option.liftPredicate(given, isAbsolute), pathToFileURL));
const covers =
    (identifier: string) =>
    ({ prefixes }: Option.Option.Value<Installation['claim']>) =>
        prefixes.some((prefix) => identifier.startsWith(prefix));
const claimed = (installed: readonly Installation[], rows: readonly Omit<Entry, 'claim'>[]) => {
    const claims = Array.getSomes(installed.map(Struct.get('claim')));
    return rows.map((row): Entry => ({ ...row, claim: Array.findFirst(claims, covers(row.identifier)) }));
};
const entriesOf = (offered: readonly Installation[]) => offered.map(({ url, identifier, name, bundleName, version }) => ({ url, identifier, name, aliases: [identifier, ...Option.toArray(bundleName)], facts: Record.getSomes({ identifier: Option.some(identifier), version }) }));
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
const strains = ({ memory, available, thermal, lowPower, advisory }: typeof Pressure.Type) => [
    { normal: memory === 'normal', text: `memory pressure ${memory} with ${available}% available` },
    { normal: thermal === 'nominal', text: `thermal state ${thermal}` },
    { normal: !lowPower, text: `low power mode ${lowPower ? 'on' : 'off'}` },
    ...Record.collect(advisory, (key, level) => ({ normal: level !== 'bad', text: `load advisory ${key} ${level}` })),
];
const said = (session: Session, keep: (key: keyof typeof SESSION) => boolean) =>
    Struct.keys(SESSION)
        .filter(keep)
        .map((key) => SESSION[key][`${session[key]}`]);
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
            const answers = asking.answers.flatMap(({ ref, label }) => (driven ? [[...Array.fromNullishOr(label), Address.token('action', ref)].join(' ')] : Array.fromNullishOr(label)));
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
                `${{ created: 'opened', destroyed: 'closed', moved: 'moved', resized: 'resized', levelChanged: 'moved to its level', shown: 'showed', hidden: 'hid' }[notification]} ${window.band} window ${window.number}${titled(window.title)}`,
                `at ${window.frame[0].join(',')} ${window.frame[1].join('×')}`,
                ...Array.fromNullishOr(window.parent).map((parent) => `child of window ${parent}`),
                ...(window.shared ? [] : ['refuses capture']),
            ].join(', '),
        status: ({ status }) => InstanceState.Status.matchOrElse(status, { unresponsive: ({ thread }) => `unresponsive, main thread ${thread}` }, ({ _tag }) => _tag),
        exited: ({ termination }) => describeExit(termination),
    });
const refuseBlocked = (name: string) => (found: Option.Option<InstanceSnapshot>) =>
    Option.match(found, {
        onNone: () => Result.succeed(`${name} no longer runs`),
        onSome: ({ state }) =>
            InstanceState.match(state, {
                exited: ({ termination }) => Result.succeed(`${name} ${describeExit(termination)}`),
                failed: () => Result.failVoid,
                running: (running) =>
                    Array.match(
                        running.windows
                            .filter(Struct.get('asking'))
                            .map(({ kind = 'window', container, number, title }) => `${name} ${kind} ${UndefinedOr.match(container, { onUndefined: () => `${number}`, onDefined: (ref) => Address.token(kind, ref) })}${titled(title)} asks for an answer: inspect reads it and act answers it`),
                        {
                            onNonEmpty: (dialogs) => Result.succeed(dialogs.join('; ')),
                            onEmpty: () => InstanceState.Status.matchOrElse(running.status, { unresponsive: ({ thread }) => Result.succeed(`${name} stopped responding, main thread ${thread}${describeUsage(running)}`) }, () => Result.failVoid),
                        },
                    ),
            }),
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { describeUsage, type Entry, follows, Workspace };
