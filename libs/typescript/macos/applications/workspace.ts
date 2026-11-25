import { Handle, Server } from '@rasm/mcp';
import { Unmatched } from '@rasm/search';
import { Array, Cause, Context, Effect, Equivalence, Function, HashSet, Layer, Option, Predicate, Record, Result, Schema, Stream, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { ApplicationServer } from '#/client';
import { type Dialog, Event, Installation, Pressure, type RunningInstance, State } from '#/contract';

// --- [TYPES] ---------------------------------------------------------------------------

type Asked = Pick<Dialog, 'kind' | 'window' | 'text'> & { readonly [Key in 'title' | 'answers' | 'element']?: Dialog[Key] | undefined };
interface Entry {
    readonly url: URL;
    readonly identifier: string;
    readonly name: string;
    readonly aliases: readonly string[];
    readonly facts: Record.ReadonlyRecord<string, Schema.Json>;
    readonly claim: Installation['claim'];
}
type Catalogued<E, R> = (offered: readonly Installation[]) => Effect.Effect<readonly Omit<Entry, 'claim'>[], E, R>;

// --- [SERVICES] ------------------------------------------------------------------------

const Target = Context.Reference<{ readonly held: Option.Option<{ readonly row: Entry; readonly instance: RunningInstance['instance'] }>; readonly followed: HashSet.HashSet<RunningInstance['instance']> }>('@rasm/applications/workspace/Target', {
    defaultValue: () => ({ held: Option.none(), followed: HashSet.empty() }),
});

class Inventory extends Context.Service<Inventory>()('@rasm/applications/workspace/Inventory', {
    make: Effect.fnUntraced(function* <E, R>(catalogued: Catalogued<E, R>) {
        const server = yield* ApplicationServer;
        const listed = Effect.gen(function* () {
            const installed = yield* server.installed;
            const [claims, rows] = [Array.getSomes(installed.map(Struct.get('claim'))), yield* catalogued(installed.filter(({ activationPolicy }) => activationPolicy !== 'prohibited'))];
            return rows.map((row): Entry => ({ ...row, claim: Array.findFirst(claims, covering(row.identifier)) }));
        });
        const [catalog, running] = yield* Effect.all([Effect.flatMap(listed, (rows) => SubscriptionRef.make(rows)), SubscriptionRef.make<readonly RunningInstance[]>([])]);
        const identified = Stream.changesWith(
            Stream.map(SubscriptionRef.changes(catalog), (rows) => Array.dedupe(rows.map(Struct.get('identifier')))),
            Array.makeEquivalence(Equivalence.String),
        );
        const monitored = <A, X>(subscribe: (identifiers: Array.NonEmptyReadonlyArray<string>) => Stream.Stream<A, X>) => Stream.switchMap(identified, Array.match({ onEmpty: () => Stream.empty, onNonEmpty: subscribe }));
        const refreshed = Effect.catchCause(
            Effect.flatMap(listed, (rows) => SubscriptionRef.set(catalog, rows)),
            (cause) => Effect.logError('Application listing failed, the last listing stands', cause),
        );
        yield* Effect.forkScoped(Stream.runForEach(Stream.drop(SubscriptionRef.changes(server.installations), 1), () => refreshed));
        yield* Effect.forkScoped(
            Stream.runForEach(
                monitored((identifiers) => server.instances({ identifiers })),
                (rows) => SubscriptionRef.set(running, rows),
            ),
        );
        const pressures = server.pressure().pipe(
            Stream.filterMap(Function.identity),
            Stream.changesWith(Schema.toEquivalence(Pressure)),
            Stream.drop(1),
            Stream.map((pressure) => ({ pressure })),
        );
        const happened = Stream.merge(
            Stream.mapEffect(
                monitored((identifiers) => server.events({ identifiers })),
                Effect.fnUntraced(function* (change) {
                    const [rows, entries] = yield* Effect.all([SubscriptionRef.get(running), SubscriptionRef.get(catalog)]);
                    const byBundle = Record.fromIterableBy(entries, (row) => row.url.href);
                    const bundle = Option.map(
                        Array.findFirst(rows, (row) => row.instance === change.instance),
                        Struct.get('url'),
                    );
                    const listedName = Option.map(
                        Option.flatMap(bundle, (url) => Record.get(byBundle, url.href)),
                        Struct.get('name'),
                    );
                    const startedName = Event.matchOrElse(change, { started: ({ name }) => Option.fromUndefinedOr(name) }, Option.none);
                    return {
                        application: Option.getOrElse(
                            Option.orElse(listedName, () => startedName),
                            () => change.instance,
                        ),
                        change,
                    };
                }),
            ),
            pressures,
        ).pipe(Stream.catchCause((cause) => Stream.fromEffect(Effect.andThen(Effect.logError('Application events ended', cause), Effect.never))));
        const target = Effect.flatMap(Handle, (handle) => handle.get(Target));
        yield* Server.use((served) =>
            served.events({
                stream: happened,
                render: (event) => Effect.succeed('pressure' in event ? `memory ${event.pressure.memory}, thermal ${event.pressure.thermal}` : `${event.application} ${changed(event.change)}`),
                owns: (event, state) => 'pressure' in event || HashSet.has(Context.get(state, Target).followed, event.change.instance),
            }),
        );
        const aimed = Effect.fnUntraced(function* (aim: { readonly row: Entry; readonly instance: RunningInstance['instance'] }) {
            const [handle, { followed }] = yield* Effect.all([Handle, target]);
            yield* handle.set(Target, { held: Option.some(aim), followed: HashSet.add(followed, aim.instance) });
        });
        const found = (given: string) =>
            Effect.flatMap(
                SubscriptionRef.get(catalog),
                Unmatched.single('listed application', Option.some(given), (row: Entry) => [row.name, ...row.aliases, row.url.href]),
            );
        const located = (given: string) =>
            Effect.flatMap(found(given), (row) =>
                Option.match(row.claim, {
                    onNone: () => Effect.succeed(row),
                    onSome: ({ server: owner, scope }) => new Cause.IllegalArgumentError(`${row.name} is driven by MCP server ${owner} (${scope}), call its tools for this application`),
                }),
            );
        const resolved = Effect.gen(function* () {
            const [{ held }, rows, entries] = yield* Effect.all([target, SubscriptionRef.get(running), SubscriptionRef.get(catalog)]);
            const driven = Record.fromIterableBy(
                entries.filter(({ claim }) => Option.isNone(claim)),
                (each) => each.url.href,
            );
            const presenting = Array.getSomes(rows.filter((each) => State.matchOrElse(each.state, { running: ({ running: shown }) => shown.windows.some(Struct.get('presented')) }, Function.constFalse)).map((each) => Option.all({ row: Record.get(driven, each.url.href), instance: Option.some(each.instance) })));
            return { held, presenting, sole: Option.orElse(held, () => (presenting.length === 1 ? Array.head(presenting) : Option.none())) };
        });
        const current = Effect.gen(function* () {
            const [{ held, presenting, sole }, rows] = yield* Effect.all([resolved, SubscriptionRef.get(running)]);
            const names = presenting.map((each) => each.row.name);
            const aim = yield* Effect.fromOption(sole, () => new Cause.NoSuchElementError(`No application is the target of this handle and ${names.length === 0 ? 'none runs' : `${names.join(', ')} run`}, applications open makes one the target`));
            yield* Option.isNone(held) ? aimed(aim) : Effect.void;
            const live = yield* Effect.fromOption(
                Array.findFirst(rows, (each) => each.instance === aim.instance && !State.isAnyOf(['exited'])(each.state)),
                () => new Cause.NoSuchElementError(`${aim.row.name} instance ${aim.instance} no longer runs, applications open starts or targets it again`),
            );
            return { row: aim.row, live };
        });
        const blocking = (row: Pick<Entry, 'url' | 'name'>, instance?: RunningInstance['instance']) =>
            Context.make(Server.EarlyReturn, SubscriptionRef.changes(running).pipe(Stream.map(Array.findFirst((each: RunningInstance) => (instance === undefined ? Installation.runs(row)(each) : each.instance === instance))), Stream.filterMap(blocked(row.name)), Stream.map(Effect.succeed)));
        const ends = Context.make(Server.EarlyReturn, Stream.unwrap(Effect.map(resolved, ({ sole }) => Option.match(sole, { onNone: () => Stream.never, onSome: ({ row, instance }) => Context.get(blocking(row, instance), Server.EarlyReturn) }))));
        return { catalog, running, target, aimed, found, located, current, ends, blocking };
    }),
}) {
    static readonly layer = <E, R>(catalogued: Catalogued<E, R>) => Layer.effect(Inventory, Inventory.make(catalogued));
    static readonly installed = (offered: readonly Installation[]) => Effect.succeed(offered.map(({ url, identifier, name, bundleName, version }) => ({ url, identifier, name, aliases: [identifier, ...Option.toArray(bundleName)], facts: Record.getSomes({ identifier: Option.some(identifier), version }) })));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const covering =
    (identifier: string) =>
    ({ prefixes }: Option.Option.Value<Installation['claim']>) =>
        prefixes.some((prefix) => identifier.startsWith(prefix));
const shown = ({ number, title, document }: { readonly number: number; readonly title?: string; readonly document?: string }) => [`window ${number}`, ...(title === undefined ? [] : [`"${title}"`]), ...(document === undefined ? [] : [`of ${document}`])].join(' ');
const ended = State.Exited.match({ Status: ({ status }) => `with status ${status}`, Signal: ({ signal }) => `on signal ${signal}`, Unread: () => 'with unread status' });
const exited = (exit: Extract<RunningInstance['state'], { readonly _tag: 'exited' }>['exited']) => `exited ${ended(exit)}${UndefinedOr.match(exit.report, { onUndefined: () => '', onDefined: (report) => `, ${report.kind} report ${report.url.href}` })}`;
const labelled = (controls: readonly { readonly id: string; readonly role: string; readonly label?: string }[]) => controls.map(({ id, role, label }) => `${label ?? role} ${id}`).join(', ');
const answering = ({ answers, element }: Pick<Asked, 'answers' | 'element'>) => {
    const marked = Array.dedupeWith([answers?.accept, answers?.discard, answers?.cancel].filter(Predicate.isNotUndefined), Equivalence.mapInput(Equivalence.String, Struct.get('id')));
    const others = Array.filter(answers?.buttons ?? [], (button) => !marked.some(({ id }) => id === button.id));
    return Array.match(marked, {
        onEmpty: () => `answers ${labelled(others)}`,
        onNonEmpty: (picked) => `answers ${labelled(picked)}${Array.match(others, { onEmpty: () => '', onNonEmpty: (rest) => `, ${rest.length} more buttons inspect of ${element ?? 'the dialog'} lists` })}`,
    });
};
const asked = ({ kind, window, title, text, answers, element }: Asked) =>
    kind === 'blocking'
        ? `window ${window} blocks its application, which answers no Accessibility read while it shows: view shows it, and act with a key answers it`
        : `${kind} in window ${window} asks${UndefinedOr.match(title, { onUndefined: () => '', onDefined: (named) => ` "${named}"` })}: ${text.join(' ')}, ${answering({ answers, element })}`;
const changed = Event.match({
    started: () => 'started',
    opened: ({ opened }) => `opened ${shown(opened)}`,
    closed: ({ closed }) => `closed ${shown(closed)}`,
    asking: ({ asking }) => asked(asking),
    answered: ({ answered }) => `${answered.kind} in window ${answered.window} answered`,
    status: ({ status, process }) => UndefinedOr.match(process, { onUndefined: () => status, onDefined: (thread) => `${status}, main thread ${thread}` }),
    exited: ({ exited: exit }) => exited(exit),
});
const blocked = (name: string) => (found: Option.Option<RunningInstance>) =>
    Option.match(found, {
        onNone: () => Result.succeed(`${name} no longer runs`),
        onSome: ({ state }) =>
            State.match(state, {
                exited: ({ exited: exit }) => Result.succeed(`${name} ${exited(exit)}`),
                failed: () => Result.failVoid,
                running: ({ running }) =>
                    Array.match(
                        running.windows.filter(Struct.get('asking')).map((window) => `${name} ${asked({ kind: window.kind, window: window.number, title: window.element?.title ?? window.title, text: window.element?.detail?.text ?? [], answers: window.element?.answers, element: window.element?.id })}`),
                        {
                            onNonEmpty: (dialogs) => Result.succeed(dialogs.join('; ')),
                            onEmpty: () => (running.status === 'unresponsive' ? Result.succeed(`${name} stopped responding${UndefinedOr.match(running.process, { onUndefined: () => '', onDefined: (thread) => `, main thread ${thread}` })}`) : Result.failVoid),
                        },
                    ),
            }),
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Entry, Inventory };
