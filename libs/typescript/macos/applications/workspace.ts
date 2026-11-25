import { Handle, Server } from '@rasm/mcp';
import { Unmatched } from '@rasm/search';
import { Array, Cause, Context, Effect, Equivalence, Function, HashSet, Layer, Option, Predicate, Record, Result, Schema, Stream, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { ApplicationServer } from '#/client';
import { type Dialog, Event, Installation, Pressure, type InstanceSnapshot, InstanceState } from '#/contract';

// --- [TYPES] ---------------------------------------------------------------------------

type DialogFacts = Pick<Dialog, 'kind' | 'window' | 'text'> & { readonly [Key in 'title' | 'answers' | 'element']?: Dialog[Key] | undefined };
interface Entry {
    readonly url: URL;
    readonly identifier: string;
    readonly name: string;
    readonly aliases: readonly string[];
    readonly facts: Record.ReadonlyRecord<string, Schema.Json>;
    readonly claim: Installation['claim'];
}
type Cataloguer<E, R> = (offered: readonly Installation[]) => Effect.Effect<readonly Omit<Entry, 'claim'>[], E, R>;

// --- [SERVICES] ------------------------------------------------------------------------

const Target = Context.Reference<{ readonly held: Option.Option<{ readonly row: Entry; readonly instance: InstanceSnapshot['instance'] }>; readonly followed: HashSet.HashSet<InstanceSnapshot['instance']> }>('@rasm/applications/workspace/Target', {
    defaultValue: () => ({ held: Option.none(), followed: HashSet.empty() }),
});

class Workspace extends Context.Service<Workspace>()('@rasm/applications/workspace', {
    make: Effect.fnUntraced(function* <E, R>(cataloguer: Cataloguer<E, R>) {
        const server = yield* ApplicationServer;
        const listEntries = Effect.gen(function* () {
            const installed = yield* server.currentInstallations;
            const [claims, rows] = [Array.getSomes(installed.map(Struct.get('claim'))), yield* cataloguer(installed.filter(({ activationPolicy }) => activationPolicy !== 'prohibited'))];
            return rows.map((row): Entry => ({ ...row, claim: Array.findFirst(claims, covers(row.identifier)) }));
        });
        const [catalog, running] = yield* Effect.all([Effect.flatMap(listEntries, (rows) => SubscriptionRef.make(rows)), SubscriptionRef.make<readonly InstanceSnapshot[]>([])]);
        const identifierChanges = Stream.changesWith(
            Stream.map(SubscriptionRef.changes(catalog), (rows) => Array.dedupe(rows.map(Struct.get('identifier')))),
            Array.makeEquivalence(Equivalence.String),
        );
        const monitor = <A, X>(subscribe: (identifiers: Array.NonEmptyReadonlyArray<string>) => Stream.Stream<A, X>) => Stream.switchMap(identifierChanges, Array.match({ onEmpty: () => Stream.empty, onNonEmpty: subscribe }));
        const refresh = Effect.catchCause(
            Effect.flatMap(listEntries, (rows) => SubscriptionRef.set(catalog, rows)),
            (cause) => Effect.logError('Application listing failed, the last listing stands', cause),
        );
        yield* Effect.forkScoped(Stream.runForEach(Stream.drop(SubscriptionRef.changes(server.installations), 1), () => refresh));
        yield* Effect.forkScoped(
            Stream.runForEach(
                monitor((identifiers) => server.instances({ identifiers })),
                (rows) => SubscriptionRef.set(running, rows),
            ),
        );
        const pressures = server.pressure().pipe(
            Stream.filterMap(Function.identity),
            Stream.changesWith(Schema.toEquivalence(Pressure)),
            Stream.drop(1),
            Stream.map((pressure) => ({ pressure })),
        );
        const events = Stream.merge(
            Stream.mapEffect(
                monitor((identifiers) => server.events({ identifiers })),
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
                stream: events,
                render: (event) => Effect.succeed('pressure' in event ? `memory ${event.pressure.memory}, thermal ${event.pressure.thermal}` : `${event.application} ${describeEvent(event.change)}`),
                owns: (event, state) => 'pressure' in event || HashSet.has(Context.get(state, Target).followed, event.change.instance),
            }),
        );
        const setTarget = Effect.fnUntraced(function* (aim: { readonly row: Entry; readonly instance: InstanceSnapshot['instance'] }) {
            const [handle, { followed }] = yield* Effect.all([Handle, target]);
            yield* handle.set(Target, { held: Option.some(aim), followed: HashSet.add(followed, aim.instance) });
        });
        const find = (given: string) =>
            Effect.flatMap(
                SubscriptionRef.get(catalog),
                Unmatched.single('listed application', Option.some(given), (row: Entry) => [row.name, ...row.aliases, row.url.href]),
            );
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
            const presenting = Array.getSomes(rows.filter((each) => InstanceState.matchOrElse(each.state, { running: ({ running: live }) => live.windows.some(Struct.get('presented')) }, Function.constFalse)).map((each) => Option.all({ row: Record.get(driven, each.url.href), instance: Option.some(each.instance) })));
            return { held, presenting, sole: Option.orElse(held, () => (presenting.length === 1 ? Array.head(presenting) : Option.none())) };
        });
        const current = Effect.gen(function* () {
            const [{ held, presenting, sole }, rows] = yield* Effect.all([targetState, SubscriptionRef.get(running)]);
            const names = presenting.map((each) => each.row.name);
            const aim = yield* Effect.fromOption(sole, () => new Cause.NoSuchElementError(`No application is the target of this handle and ${names.length === 0 ? 'none runs' : `${names.join(', ')} run`}, applications open makes one the target`));
            yield* Option.isNone(held) ? setTarget(aim) : Effect.void;
            const live = yield* Effect.fromOption(
                Array.findFirst(rows, (each) => each.instance === aim.instance && !InstanceState.isAnyOf(['exited'])(each.state)),
                () => new Cause.NoSuchElementError(`${aim.row.name} instance ${aim.instance} no longer runs, applications open starts or targets it again`),
            );
            return { row: aim.row, live };
        });
        const blocking = (row: Pick<Entry, 'url' | 'name'>, instance?: InstanceSnapshot['instance']) =>
            Context.make(Server.EarlyReturn, SubscriptionRef.changes(running).pipe(Stream.map(Array.findFirst((each: InstanceSnapshot) => (instance === undefined ? Installation.runs(row)(each) : each.instance === instance))), Stream.filterMap(refuseBlocked(row.name)), Stream.map(Effect.succeed)));
        const ends = Context.make(Server.EarlyReturn, Stream.unwrap(Effect.map(targetState, ({ sole }) => Option.match(sole, { onNone: () => Stream.never, onSome: ({ row, instance }) => Context.get(blocking(row, instance), Server.EarlyReturn) }))));
        return { catalog, running, target, setTarget, find, resolve, current, ends, blocking };
    }),
}) {
    static readonly layer = <E, R>(cataloguer: Cataloguer<E, R>) => Layer.effect(Workspace, Workspace.make(cataloguer));
    static readonly fromInstallations = (offered: readonly Installation[]) => Effect.succeed(offered.map(({ url, identifier, name, bundleName, version }) => ({ url, identifier, name, aliases: [identifier, ...Option.toArray(bundleName)], facts: Record.getSomes({ identifier: Option.some(identifier), version }) })));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const covers =
    (identifier: string) =>
    ({ prefixes }: Option.Option.Value<Installation['claim']>) =>
        prefixes.some((prefix) => identifier.startsWith(prefix));
const describeWindow = ({ number, title, document }: { readonly number: number; readonly title?: string; readonly document?: string }) => [`window ${number}`, ...(title === undefined ? [] : [`"${title}"`]), ...(document === undefined ? [] : [`of ${document}`])].join(' ');
const describeExitStatus = InstanceState.Exited.match({ Status: ({ status }) => `with status ${status}`, Signal: ({ signal }) => `on signal ${signal}`, Unread: () => 'with unread status' });
const describeExit = (exit: Extract<InstanceSnapshot['state'], { readonly _tag: 'exited' }>['exited']) => `exited ${describeExitStatus(exit)}${UndefinedOr.match(exit.report, { onUndefined: () => '', onDefined: (report) => `, ${report.kind} report ${report.url.href}` })}`;
const describeControls = (controls: readonly { readonly id: string; readonly role: string; readonly label?: string }[]) => controls.map(({ id, role, label }) => `${label ?? role} ${id}`).join(', ');
const describeAnswers = ({ answers, element }: Pick<DialogFacts, 'answers' | 'element'>) => {
    const marked = Array.dedupeWith([answers?.accept, answers?.discard, answers?.cancel].filter(Predicate.isNotUndefined), Equivalence.mapInput(Equivalence.String, Struct.get('id')));
    const others = Array.filter(answers?.buttons ?? [], (button) => !marked.some(({ id }) => id === button.id));
    return Array.match(marked, {
        onEmpty: () => `answers ${describeControls(others)}`,
        onNonEmpty: (picked) => `answers ${describeControls(picked)}${Array.match(others, { onEmpty: () => '', onNonEmpty: (rest) => `, ${rest.length} more buttons inspect of ${element ?? 'the dialog'} lists` })}`,
    });
};
const describeDialog = ({ kind, window, title, text, answers, element }: DialogFacts) =>
    kind === 'blocking'
        ? `window ${window} blocks its application, which answers no Accessibility read while it shows: view shows it, and act with a key answers it`
        : `${kind} in window ${window} asks${UndefinedOr.match(title, { onUndefined: () => '', onDefined: (named) => ` "${named}"` })}: ${text.join(' ')}, ${describeAnswers({ answers, element })}`;
const describeEvent = Event.match({
    started: () => 'started',
    opened: ({ opened }) => `opened ${describeWindow(opened)}`,
    closed: ({ closed }) => `closed ${describeWindow(closed)}`,
    asking: ({ asking }) => describeDialog(asking),
    answered: ({ answered }) => `${answered.kind} in window ${answered.window} answered`,
    status: ({ status, process }) => UndefinedOr.match(process, { onUndefined: () => status, onDefined: (thread) => `${status}, main thread ${thread}` }),
    exited: ({ exited: exit }) => describeExit(exit),
});
const refuseBlocked = (name: string) => (found: Option.Option<InstanceSnapshot>) =>
    Option.match(found, {
        onNone: () => Result.succeed(`${name} no longer runs`),
        onSome: ({ state }) =>
            InstanceState.match(state, {
                exited: ({ exited: exit }) => Result.succeed(`${name} ${describeExit(exit)}`),
                failed: () => Result.failVoid,
                running: ({ running }) =>
                    Array.match(
                        running.windows.filter(Struct.get('asking')).map((window) => `${name} ${describeDialog({ kind: window.kind, window: window.number, title: window.element?.title ?? window.title, text: window.element?.detail?.text ?? [], answers: window.element?.answers, element: window.element?.id })}`),
                        {
                            onNonEmpty: (dialogs) => Result.succeed(dialogs.join('; ')),
                            onEmpty: () => (running.status === 'unresponsive' ? Result.succeed(`${name} stopped responding${UndefinedOr.match(running.process, { onUndefined: () => '', onDefined: (thread) => `, main thread ${thread}` })}`) : Result.failVoid),
                        },
                    ),
            }),
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Entry, Workspace };
