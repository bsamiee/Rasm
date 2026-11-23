import { type Dialog, Event, Installation, Pressure, RunningInstance, State } from '@rasm/applications';
import { ApplicationServer } from '@rasm/applications/node';
import { Application } from '@rasm/creative-cloud-desktop';
import { Handle, Link, Server } from '@rasm/mcp';
import { Unmatched } from '@rasm/search';
import { Array, Cause, Context, Effect, Equivalence, Function, HashSet, Layer, Match, Option, Predicate, Record, Result, Schema, Stream, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { McpSchema, Tool } from 'effect/ai';

// --- [TYPES] ---------------------------------------------------------------------------

type Asked = Pick<Dialog, 'kind' | 'window' | 'text'> & { readonly [Key in 'title' | 'answers' | 'element']?: Dialog[Key] | undefined };

// --- [MODELS] --------------------------------------------------------------------------

const Listed = Schema.Struct({
    installed: Schema.Array(
        Schema.Struct({
            ...Struct.omit(Application.fields, ['identifier', 'documents', 'url']),
            running: Schema.optionalKey(Schema.Struct({ instance: RunningInstance.fields.instance, status: Schema.String, windows: Schema.Int.annotate({ description: 'Presented windows' }) })),
        }),
    ),
    current: Schema.optionalKey(Schema.String.annotate({ description: 'Application the handle targets' })),
});
const Opened = Schema.Struct({ application: Schema.String, instance: RunningInstance.fields.instance, created: Schema.Boolean.annotate({ description: 'False when the running instance became the target' }) });
const Closed = Schema.Struct({ closed: Schema.Array(Schema.Struct({ application: Schema.String, instance: RunningInstance.fields.instance, state: State })) });
const Applications = Tool.make('applications', {
    description: 'Lists installed Adobe applications grouped by product, newest form first, with the running instance of each; open launches one in the background, or makes its running instance the target of the handle; close quits one, or every running one, each save dialog arriving as an event act answers',
    parameters: Schema.Struct({
        action: Schema.optionalKey(Schema.Literals(['list', 'open', 'close']).annotate({ description: 'list when absent' })),
        application: Schema.optionalKey(Schema.String.annotate({ description: 'Listed name, the name its bundle displays, or product code for its first listed form; close without one quits every running listed application' })),
    }),
    success: Schema.Union([Listed, Opened, Closed]),
}).annotateMerge(Context.make(Tool.Destructive, false).pipe(Context.add(Tool.Idempotent, true), Context.add(Tool.OpenWorld, false), Context.add(Tool.Meta, { 'anthropic/alwaysLoad': true })));

// --- [SERVICES] ------------------------------------------------------------------------

const Target = Context.Reference<{ readonly held: Option.Option<{ readonly row: Application; readonly instance: RunningInstance['instance'] }>; readonly followed: HashSet.HashSet<RunningInstance['instance']> }>('@rasm/atelier/applications/Target', {
    defaultValue: () => ({ held: Option.none(), followed: HashSet.empty() }),
});

class Inventory extends Context.Service<Inventory>()('@rasm/atelier/applications/Inventory', {
    make: Effect.gen(function* () {
        const [server, installed] = yield* Effect.all([ApplicationServer, Application.installed]);
        const [catalog, running] = yield* Effect.all([SubscriptionRef.make<readonly Application[]>(installed), SubscriptionRef.make<readonly RunningInstance[]>([])]);
        const identified = Stream.changesWith(
            Stream.map(SubscriptionRef.changes(catalog), (rows) => Array.dedupe(rows.map(Struct.get('identifier')))),
            Array.makeEquivalence(Equivalence.String),
        );
        const monitored = <A, E>(subscribe: (identifiers: Array.NonEmptyReadonlyArray<string>) => Stream.Stream<A, E>) => Stream.switchMap(identified, Array.match({ onEmpty: () => Stream.empty, onNonEmpty: subscribe }));
        const refreshed = Effect.catchCause(
            Effect.flatMap(Application.installed, (rows) => SubscriptionRef.set(catalog, rows)),
            (cause) => Effect.logError('Adobe application listing failed, the last listing stands', cause),
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
                    const [rows, listed] = yield* Effect.all([SubscriptionRef.get(running), SubscriptionRef.get(catalog)]);
                    const byBundle = Record.fromIterableBy(listed, (row) => row.url.href);
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
                text: (event) => ('pressure' in event ? `memory ${event.pressure.memory}, thermal ${event.pressure.thermal}` : `${event.application} ${changed(event.change)}`),
                shown: (event) => ('pressure' in event ? Effect.succeed(true) : Effect.map(target, ({ followed }) => HashSet.has(followed, event.change.instance))),
                stream: happened,
            }),
        );
        const aimed = Effect.fnUntraced(function* (aim: { readonly row: Application; readonly instance: RunningInstance['instance'] }) {
            const [handle, { followed }] = yield* Effect.all([Handle, target]);
            yield* handle.set(Target, { held: Option.some(aim), followed: HashSet.add(followed, aim.instance) });
        });
        const located = Effect.fnUntraced(function* (given: string) {
            const [rows, installations] = yield* Effect.all([SubscriptionRef.get(catalog), server.installed]);
            const byBundle = Record.fromIterableBy(installations, (each) => each.url.href);
            const names = (row: Application) => [row.name, row.product, ...Option.toArray(Option.map(Record.get(byBundle, row.url.href), Struct.get('name')))];
            const refused = Unmatched.of({ subject: 'installed Adobe application', query: Option.some(given), candidates: Array.dedupe(rows.flatMap(names)) });
            return yield* Effect.fromOption(
                Array.findFirst(rows, (row) => names(row).includes(given)),
                () => refused,
            );
        });
        const resolved = Effect.gen(function* () {
            const [{ held }, rows, listed] = yield* Effect.all([target, SubscriptionRef.get(running), SubscriptionRef.get(catalog)]);
            const byBundle = Record.fromIterableBy(listed, (each) => each.url.href);
            const presenting = Array.getSomes(rows.filter((each) => summary(each).windows > 0).map((each) => Option.all({ row: Record.get(byBundle, each.url.href), instance: Option.some(each.instance) })));
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
        const ends = Context.make(
            Server.Ends,
            Stream.unwrap(
                Effect.map(resolved, ({ sole }) =>
                    Option.match(sole, {
                        onNone: () => Stream.never,
                        onSome: ({ row, instance }) => SubscriptionRef.changes(running).pipe(Stream.map(Array.findFirst((each: RunningInstance) => each.instance === instance)), Stream.filterMap(blocked(row.name))),
                    }),
                ),
            ),
        );
        const route = Effect.fnUntraced(function* (peers: Array.NonEmptyReadonlyArray<Link>) {
            const { held } = yield* target;
            const refused = McpSchema.InvalidParams.make({ message: `Linked in ${peers.map((peer) => String(peer.application)).join(', ')}, applications open makes one the target` });
            return yield* Effect.fromOption(
                Option.flatMap(held, ({ row }) => Array.findFirst(peers, Link.hosted(row))),
                () => refused,
            );
        });
        return { catalog, running, target, aimed, located, current, route, ends };
    }),
}) {
    static readonly layer = Layer.effect(Inventory, Inventory.make);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

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
const asked = ({ kind, window, title, text, answers, element }: Asked) => `${kind} in window ${window} asks${UndefinedOr.match(title, { onUndefined: () => '', onDefined: (named) => ` "${named}"` })}: ${text.join(' ')}, ${answering({ answers, element })}`;
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
const summary = (each: RunningInstance) => ({
    instance: each.instance,
    status: State.match(each.state, { running: ({ running }) => running.status, failed: () => 'failed', exited: () => 'exited' }),
    windows: State.matchOrElse(each.state, { running: ({ running }) => running.windows.filter(Struct.get('presented')).length }, () => 0),
});
const listing = (instances: readonly RunningInstance[]) => (row: Application) => ({ ...Struct.omit(row, ['identifier', 'documents', 'url']), ...Option.getOrUndefined(Option.map(Array.findFirst(instances, Installation.runs(row)), (each) => ({ running: summary(each) }))) });
const quitting = (instances: readonly RunningInstance[]) => (row: Application) => instances.filter(Installation.runs(row)).map((each) => ({ application: row.name, instance: each.instance }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const applications = Effect.gen(function* () {
    const [server, { catalog, running, target, aimed, located }] = yield* Effect.all([ApplicationServer, Inventory]);
    const list = Effect.gen(function* () {
        const [rows, instances, { held }] = yield* Effect.all([SubscriptionRef.get(catalog), SubscriptionRef.get(running), target]);
        return { installed: rows.map(listing(instances)), ...Option.getOrUndefined(Option.map(held, ({ row }) => ({ current: row.name }))) };
    });
    const open = Effect.fnUntraced(function* (given: string) {
        const [row, instances] = yield* Effect.all([located(given), SubscriptionRef.get(running)]);
        const launched = Effect.map(server.launched({ url: row.url }), ({ instance, created }) => ({ instance, created }));
        const { instance, created } = yield* Option.match(Array.findFirst(instances, Installation.runs(row)), { onSome: ({ instance: reused }) => Effect.succeed({ instance: reused, created: false }), onNone: () => launched });
        yield* aimed({ row, instance });
        return { application: row.name, instance, created };
    });
    const close = Effect.fnUntraced(function* (given: string | undefined) {
        const [rows, instances] = yield* Effect.all([UndefinedOr.match(given, { onUndefined: () => SubscriptionRef.get(catalog), onDefined: (name) => Effect.map(located(name), Array.of) }), SubscriptionRef.get(running)]);
        const quit = rows.flatMap(quitting(instances));
        const states = yield* Effect.forEach(quit, ({ instance }) => server.quit({ instance }), { concurrency: 'unbounded' });
        return { closed: Array.zipWith(quit, states, (row, state) => ({ ...row, state })) };
    });
    const opened = (given: string | undefined) =>
        Effect.flatMap(
            Effect.mapError(Effect.fromNullishOr(given), () => new Cause.IllegalArgumentError('open names an application from the list')),
            open,
        );
    yield* Server.registerTool(Applications, ({ action = 'list', application }) =>
        Match.value(action).pipe(
            Match.when('list', () => list),
            Match.when('open', () => opened(application)),
            Match.when('close', () => close(application)),
            Match.exhaustive,
        ),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { applications, Inventory };
