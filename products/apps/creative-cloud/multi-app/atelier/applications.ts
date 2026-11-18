import { Event, Installation, Launching, Pressure, type ProcessInstance, RunningInstance, State } from '@rasm/applications';
import { ApplicationServer } from '@rasm/applications/node';
import { Application } from '@rasm/creative-cloud-desktop';
import { Handle, Link, Server } from '@rasm/mcp';
import { Unmatched } from '@rasm/search';
import { Array, Cause, Context, Effect, Equivalence, Function, Layer, Match, Option, Record, Schema, Stream, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { McpSchema, Tool } from 'effect/ai';

// --- [MODELS] --------------------------------------------------------------------------

const Happened = Schema.Union([Schema.Struct({ application: Schema.String, change: Event }), Schema.Struct({ pressure: Pressure })]).annotate({ description: 'Change of a monitored application, or of kernel memory pressure and thermal state' });
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
        application: Schema.optionalKey(Schema.String.annotate({ description: 'Listed name, or product code for its first listed form; close without one quits every running listed application' })),
    }),
    success: Schema.Union([Listed, Opened, Closed]),
}).annotateMerge(Context.make(Tool.Destructive, false).pipe(Context.add(Tool.Idempotent, true), Context.add(Tool.OpenWorld, false), Context.add(Tool.Meta, { 'anthropic/alwaysLoad': true })));

// --- [SERVICES] ------------------------------------------------------------------------

const Target = Context.Reference<Option.Option<{ readonly row: Application; readonly instance: typeof ProcessInstance.Type }>>('@rasm/atelier/applications/Target', { defaultValue: Option.none });

class Inventory extends Context.Service<Inventory>()('@rasm/atelier/applications/Inventory', {
    make: Effect.gen(function* () {
        const [server, installed] = yield* Effect.all([ApplicationServer, Application.installed]);
        const [catalog, running] = yield* Effect.all([SubscriptionRef.make<readonly Application[]>(installed), SubscriptionRef.make<readonly RunningInstance[]>([])]);
        const identified = Stream.changesWith(
            Stream.map(SubscriptionRef.changes(catalog), (rows) => Array.dedupe(rows.map(Struct.get('identifier')))),
            Array.makeEquivalence(Equivalence.String),
        );
        const monitored = <A, E>(subscribe: (identifiers: Array.NonEmptyReadonlyArray<string>) => Stream.Stream<A, E>) => Stream.switchMap(identified, Array.match({ onEmpty: () => Stream.empty, onNonEmpty: subscribe }));
        const named = Effect.fnUntraced(function* (change: typeof Event.Type) {
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
        });
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
        const happened: Stream.Stream<typeof Happened.Type> = Stream.merge(
            Stream.mapEffect(
                monitored((identifiers) => server.events({ identifiers })),
                named,
            ),
            pressures,
        ).pipe(Stream.catchCause((cause) => Stream.fromEffect(Effect.andThen(Effect.logError('Application events ended', cause), Effect.never))));
        const { ends } = yield* Server.use((served) => served.events({ schema: Happened, text: rendered, stream: happened }));
        const target = Effect.flatMap(Handle, (handle) => handle.get(Target));
        const located = Effect.fnUntraced(function* (given: string) {
            const rows = yield* SubscriptionRef.get(catalog);
            const [byName, byProduct] = [Array.findFirst(rows, ({ name }) => name === given), Array.findFirst(rows, ({ product }) => product === given)];
            const refused = Unmatched.of({ subject: 'installed Adobe application', query: Option.some(given), candidates: rows.flatMap(({ name, product }) => [name, product]) });
            return yield* Effect.fromOption(
                Option.orElse(byName, () => byProduct),
                () => refused,
            );
        });
        const current = Effect.gen(function* () {
            const [held, rows, listed] = yield* Effect.all([target, SubscriptionRef.get(running), SubscriptionRef.get(catalog)]);
            const byBundle = Record.fromIterableBy(listed, (each) => each.url.href);
            const runningRows = Array.getSomes(rows.filter((each) => summary(each).windows > 0).map((each) => Option.all({ row: Record.get(byBundle, each.url.href), instance: Option.some(each.instance) })));
            const names = runningRows.map((each) => each.row.name);
            const { row, instance } = yield* Effect.fromOption(
                Option.orElse(held, () => (runningRows.length === 1 ? Array.head(runningRows) : Option.none())),
                () => new Cause.NoSuchElementError(`No application is the target of this handle and ${names.length === 0 ? 'none runs' : `${names.join(', ')} run`}, applications open makes one the target`),
            );
            const live = yield* Effect.fromOption(
                Array.findFirst(rows, (each) => each.instance === instance && !State.isAnyOf(['exited'])(each.state)),
                () => new Cause.NoSuchElementError(`${row.name} instance ${instance} no longer runs, applications open starts or targets it again`),
            );
            return { row, live };
        });
        const ending = Effect.fnUntraced(function* (event: typeof Happened.Type) {
            const held = yield* target;
            const own = 'change' in event && Option.exists(held, ({ instance }) => instance === event.change.instance);
            return own && Event.matchOrElse(event.change, { asking: Function.constTrue, exited: Function.constTrue, status: ({ status }) => status === 'unresponsive' }, Function.constFalse);
        });
        const route = Effect.fnUntraced(function* (peers: Array.NonEmptyReadonlyArray<Link>) {
            const held = yield* target;
            const refused = McpSchema.InvalidParams.make({ message: `Linked in ${peers.map((peer) => String(peer.application)).join(', ')}, applications open makes one the target` });
            return yield* Effect.fromOption(
                Option.flatMap(held, ({ row }) => Array.findFirst(peers, Link.hosted(row))),
                () => refused,
            );
        });
        return { catalog, running, target, located, current, route, ends: ends(ending) };
    }),
}) {
    static readonly layer = Layer.effect(Inventory, Inventory.make);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const live = (rows: readonly RunningInstance[], row: Application) => rows.filter(Installation.runs(row));
const shown = ({ number, title, document }: { readonly number: number; readonly title?: string; readonly document?: string }) => [`window ${number}`, ...(title === undefined ? [] : [`"${title}"`]), ...(document === undefined ? [] : [`of ${document}`])].join(' ');
const ended = Match.type<Extract<typeof Event.Type, { readonly _tag: 'exited' }>['exited']>().pipe(
    Match.when({ _tag: 'Status' }, ({ status }) => `with status ${status}`),
    Match.when({ _tag: 'Signal' }, ({ signal }) => `on signal ${signal}`),
    Match.orElse(() => 'with unread status'),
);
const rendered = (happened: typeof Happened.Type) =>
    'pressure' in happened
        ? `memory ${happened.pressure.memory}, thermal ${happened.pressure.thermal}`
        : `${happened.application} ${Event.match(happened.change, {
              started: () => 'started',
              opened: ({ opened }) => `opened ${shown(opened)}`,
              closed: ({ closed }) => `closed ${shown(closed)}`,
              asking: ({ asking }) => `${asking.kind} in window ${asking.window} asks${UndefinedOr.match(asking.title, { onUndefined: () => '', onDefined: (title) => ` "${title}"` })}: ${asking.text.join(' ')}, answers ${asking.answers.join(', ')}`,
              answered: ({ answered }) => `${answered.kind} in window ${answered.window} answered`,
              status: ({ status, process }) => UndefinedOr.match(process, { onUndefined: () => status, onDefined: (thread) => `${status}, main thread ${thread}` }),
              exited: ({ exited }) => `exited ${ended(exited)}${UndefinedOr.match(exited.report, { onUndefined: () => '', onDefined: (report) => `, ${report.kind} report ${report.url}` })}`,
          })}`;
const summary = (each: RunningInstance) => ({
    instance: each.instance,
    status: State.match(each.state, { running: ({ running }) => running.status, failed: () => 'failed', exited: () => 'exited' }),
    windows: State.matchOrElse(each.state, { running: ({ running }) => running.windows.filter(Struct.get('presented')).length }, () => 0),
});
const listing = (instances: readonly RunningInstance[]) => (row: Application) => ({ ...Struct.omit(row, ['identifier', 'documents', 'url']), ...Option.getOrUndefined(Option.map(Array.head(live(instances, row)), (each) => ({ running: summary(each) }))) });
const quitting = (instances: readonly RunningInstance[]) => (row: Application) => live(instances, row).map((each) => ({ application: row.name, instance: each.instance }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const applications = Effect.gen(function* () {
    const [server, { catalog, running, target, located }] = yield* Effect.all([ApplicationServer, Inventory]);
    const list = Effect.gen(function* () {
        const [rows, instances, held] = yield* Effect.all([SubscriptionRef.get(catalog), SubscriptionRef.get(running), target]);
        return { installed: rows.map(listing(instances)), ...Option.getOrUndefined(Option.map(held, ({ row }) => ({ current: row.name }))) };
    });
    const open = Effect.fnUntraced(function* (given: string) {
        const [row, instances, handle] = yield* Effect.all([located(given), SubscriptionRef.get(running), Handle]);
        const launched = Effect.map(Launching.launched(server.launch({ url: row.url })), ({ instance, created }) => ({ instance, created }));
        const { instance, created } = yield* Option.match(Array.head(live(instances, row)), { onSome: ({ instance: reused }) => Effect.succeed({ instance: reused, created: false }), onNone: () => launched });
        yield* handle.set(Target, Option.some({ row, instance }));
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

export { applications, Inventory, Target };
