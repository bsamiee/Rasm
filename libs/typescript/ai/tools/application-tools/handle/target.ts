import { ApplicationServer } from '@rasm/applications/client';
import { Installation, InstanceSnapshot, InstanceState, Locator } from '@rasm/applications/contract';
import { byBundle, byInstance, type Entry, instancesOf, runningOf, unlisted, Workspace } from '@rasm/applications/workspace';
import { Call, Format, Line, Report } from '@rasm/mcp/report';
import { Handle, Server } from '@rasm/mcp/server';
import { ToolCall } from '@rasm/mcp/toolkit';
import { Array, Cause, Context, Data, Effect, Equivalence, Function, HashSet, Layer, Option, Record, Ref, Result, Schema, SchemaGetter, Semaphore, Stream, String, Struct, SubscriptionRef } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Named = Pick<InstanceSnapshot, 'instance'> & Pick<Entry, 'name'>;
type Hold = Data.TaggedEnum<{
    Using: Pick<Named, 'instance'>;
    Launching: Named;
    Quitting: Pick<Named, 'instance'>;
}>;

// --- [MODELS] --------------------------------------------------------------------------

const Stored = Schema.Struct({
    held: Schema.OptionFromOptionalKey(Schema.Struct({ url: Installation.fields.url, instance: InstanceSnapshot.fields.instance, language: Schema.String })),
    followed: Schema.Array(Schema.Struct({ instance: InstanceSnapshot.fields.instance, locators: Locator.pipe(Schema.Array, Schema.OptionFromOptionalKey) })),
    named: Schema.OptionFromOptionalKey(Installation.fields.url),
    opened: InstanceSnapshot.fields.instance.pipe(Schema.Array, Schema.OptionFromOptionalKey),
});
const Hold = Data.taggedEnum<Hold>();
class Aim extends Data.Class<{
    readonly held: Option.Option<{ readonly row: Entry; readonly instance: InstanceSnapshot['instance']; readonly language: string }>;
    readonly named: Option.Option<Entry>;
    readonly holds: readonly Hold[];
    readonly opened: HashSet.HashSet<InstanceSnapshot['instance']>;
}> {
    static readonly empty = new Aim({ held: Option.none(), named: Option.none(), holds: [], opened: HashSet.empty() });
    get followed() {
        return HashSet.fromIterable([...Option.toArray(this.held), ...this.holds].map(({ instance }) => instance));
    }
    quitting(instance: InstanceSnapshot['instance']) {
        return this.holds.some((each) => Hold.$is('Quitting')(each) && each.instance === instance);
    }
    without(gone: Iterable<InstanceSnapshot['instance']>) {
        const left = HashSet.fromIterable(gone);
        const ended = Option.filter(this.held, ({ instance }) => HashSet.has(left, instance));
        return new Aim({
            held: Option.isSome(ended) ? Option.none() : this.held,
            named: Option.orElse(Option.map(ended, Struct.get('row')), () => this.named),
            holds: this.holds.filter(({ instance }) => !HashSet.has(left, instance)),
            opened: HashSet.difference(this.opened, left),
        });
    }
    static readonly stored = ({
        server,
        catalog,
        running,
        tables,
        ended,
    }: {
        readonly server: ApplicationServer['Service'];
        readonly catalog: SubscriptionRef.SubscriptionRef<readonly Entry[]>;
        readonly running: SubscriptionRef.SubscriptionRef<readonly InstanceSnapshot[]>;
        readonly tables: Ref.Ref<Record.ReadonlyRecord<string, readonly string[]>>;
        readonly ended: Ref.Ref<readonly Named[]>;
    }) =>
        Stored.pipe(
            Schema.decodeTo(Schema.instanceOf(Aim), {
                decode: SchemaGetter.transformEffect(
                    Effect.fnUntraced(function* ({ held, followed, named, opened }: typeof Stored.Type) {
                        const [rows, instances, installed] = yield* Effect.all([SubscriptionRef.get(catalog), SubscriptionRef.get(running), server.currentInstallations]);
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
                        const [aimed, last] = yield* Effect.all([Effect.transposeOption(Option.map(held, ({ url, ...aim }) => entryAt(url).pipe(Effect.map(Option.map((row) => ({ row, ...aim })))))), Effect.transposeOption(Option.map(named, entryAt))]);
                        const target = Option.flatten(aimed);
                        yield* Effect.transposeOption(
                            Option.map(target, ({ row, instance }) => {
                                const seed = Array.findFirst(followed, (each) => each.instance === instance).pipe(
                                    Option.flatMap(Struct.get('locators')),
                                    Option.map((locators) => ({ locators })),
                                );
                                return Record.has(runningOf(instances), instance)
                                    ? Effect.ignore(server.watch({ instance, ...Option.getOrUndefined(seed) }), { log: 'Warn', message: `Watch of ${instance} failed to restore, its references are read again` })
                                    : Ref.update(ended, Array.append({ instance, name: row.name }));
                            }),
                        );
                        const alive = Option.getOrElse(opened, Array.empty<InstanceSnapshot['instance']>).filter((instance) => Record.has(runningOf(instances), instance));
                        return new Aim({ held: target, named: Option.flatten(last), holds: [], opened: HashSet.fromIterable(alive) });
                    }),
                ),
                encode: SchemaGetter.transformEffect(({ held, named, opened }: Aim) =>
                    Effect.map(Ref.get(tables), (read) => ({
                        held: Option.map(held, ({ row, instance, language }) => ({ url: row.url, instance, language })),
                        followed: Option.toArray(held).map(({ instance }) => ({ instance, locators: Record.get(read, instance) })),
                        named: Option.map(named, ({ url }) => url),
                        opened: Option.some(Iterator.from(opened).toArray()),
                    })),
                ),
            }),
        );
}

// --- [SERVICES] ------------------------------------------------------------------------

class Watches extends Context.Service<Watches, Semaphore.Semaphore>()('@rasm/application-tools/handle/target/Watches') {
    static readonly layer: Layer.Layer<Watches, never, ApplicationServer | Workspace | Server> = Layer.effect(
        Watches,
        Effect.gen(function* () {
            const [server, { catalog, running }, served, turn, tables, ended] = yield* Effect.all([ApplicationServer, Workspace, Server, Semaphore.make(1), Ref.make(Record.empty<string, readonly string[]>()), Ref.make<readonly Named[]>([])]);
            const following = Effect.map(Stream.runHead(served.states(Target)), (aims) => followedBy(Option.getOrElse(aims, Array.empty<Aim>)));
            yield* served.persist('@rasm/applications/workspace/Target', Target, Aim.stored({ server, catalog, running, tables, ended }));
            yield* served.events({ stream: Stream.fromIterable(yield* Ref.get(ended)), route: exited });
            yield* served.states(Target).pipe(
                Stream.map(followedBy),
                Stream.changes,
                Stream.runFoldEffect(
                    HashSet.empty<InstanceSnapshot['instance']>,
                    Effect.fnUntraced(function* (kept: HashSet.HashSet<InstanceSnapshot['instance']>, seen: HashSet.HashSet<InstanceSnapshot['instance']>) {
                        const live = yield* following;
                        const unfollowed = HashSet.difference(HashSet.union(kept, seen), live);
                        yield* Effect.forEach(unfollowed, (instance) => server.release({ instance }), { discard: true }).pipe(Effect.ignoreCause({ log: 'Warn', message: 'Watch release failed, the Watch stays until its instance exits' }));
                        return live;
                    }, Semaphore.withPermit(turn)),
                ),
                Effect.forkScoped,
            );
            yield* Effect.addFinalizer(
                Effect.fnUntraced(function* () {
                    const [live, rows] = yield* Effect.all([following, SubscriptionRef.get(running)]);
                    const due = Iterator.from(live)
                        .filter((instance) => Record.has(runningOf(rows), instance))
                        .toArray();
                    const read = yield* Effect.forEach(due, (instance) => Effect.result(server.locators({ instance })), { concurrency: 'unbounded' });
                    const [tabled, unread] = Record.partition(Record.fromEntries(Array.zip(due, read)), Function.identity);
                    yield* Effect.forEach(Record.values(unread), (cause) => Effect.logWarning('Locator table is unread, the references of its instance are read again after the restart', cause), { discard: true });
                    yield* Ref.set(tables, tabled);
                }),
            );
            return turn;
        }),
    );
}
const Target = Object.assign(Context.Reference<Aim>('@rasm/application-tools/handle/target/Target', { defaultValue: () => Aim.empty }), { layer: Watches.layer });

// --- [OPERATIONS] ----------------------------------------------------------------------

const aimed = Handle.use((handle) => handle.get(Target));
const retarget = (change: (before: Aim) => Partial<Pick<Aim, 'held' | 'named' | 'holds' | 'opened'>>) => Handle.use((handle) => handle.update(Target, (before) => new Aim({ ...before, ...change(before) })));
const followedBy = (aims: readonly Aim[]) => Array.reduce(aims, HashSet.empty<InstanceSnapshot['instance']>(), (union, { followed }) => HashSet.union(union, followed));
const openCall = (application: string) => new Call({ tool: 'applications', given: { action: 'open', application } });
const refuseUntargeted = (named: Option.Option<Entry>, followed: Aim['followed'], rows: readonly InstanceSnapshot[], entries: readonly Entry[]) =>
    Report.text(
        Option.match(named, {
            onSome: (row) => new Report({ lines: [new Line({ head: ['no target'], facts: [`${row.name} ${Array.isReadonlyArrayNonEmpty(instancesOf(rows, followed)(row).meant) ? 'runs' : 'not running'}`] })], next: openCall(row.name) }),
            onNone: () => {
                const driven = Record.filter(byBundle(entries), ({ claim }) => Option.isNone(claim));
                const showing = rows.filter((each) => InstanceState.matchOrElse(each.state, { running: ({ windows, statusItems = 0 }) => statusItems > 0 || windows.some(Struct.get('presented')) }, Function.constFalse));
                const [shown, rest] = Array.splitAt(Array.dedupe(Array.getSomes(showing.map((each) => Option.map(Record.get(driven, each.url.href), Struct.get('name'))))), 5);
                const names = [...shown, ...(Array.isReadonlyArrayEmpty(rest) ? [] : [Format.count(rest.length, { other: 'more' })])];
                return new Report({ lines: [new Line({ head: ['no target'], call: openCall('<name>') }), ...(Array.isReadonlyArrayEmpty(names) ? [] : [new Line({ head: ['showing'], value: Format.list(names) })])] });
            },
        }),
    );
const refuseBlocked = (name: string) => (found: Option.Option<InstanceSnapshot>) => {
    const lines = Option.match(Option.map(found, Struct.get('state')), {
        onNone: () => [new Line({ head: [name, 'not running'] })],
        onSome: InstanceState.match({
            exited: ({ _tag }) => [new Line({ head: [name, _tag], call: openCall(name) })],
            failed: Array.empty<Line>,
            running: ({ status, windows }) => [
                ...windows.filter(Struct.get('asking')).map(({ kind = 'window', title }) => new Line({ head: [name, 'asking', String.noCase()(kind), ...Array.fromNullishOr(title).map(Format.quoted)] })),
                ...InstanceState.Status.matchOrElse(status, { unresponsive: ({ _tag, thread }) => [new Line({ head: [name, _tag], facts: [`main thread ${thread}`] })] }, Array.empty<Line>),
            ],
        }),
    });
    return Array.isReadonlyArrayEmpty(lines) ? Result.failVoid : Result.succeed(Report.text(new Report({ lines })));
};
const blocked =
    (running: SubscriptionRef.SubscriptionRef<readonly InstanceSnapshot[]>) =>
    ({ instance, name }: Named, quitting = false) =>
        SubscriptionRef.changes(running).pipe(
            Stream.map((rows) => Record.get(byInstance(rows), instance)),
            Stream.dropWhile(Option.isNone),
            Stream.takeWhile((found) => !quitting || Option.exists(found, ({ state }) => !InstanceState.isAnyOf(['exited'])(state))),
            Stream.filterMap(refuseBlocked(name)),
            Stream.map(Effect.succeed),
        );
const exited = ({ instance, name }: Named, state: Context.Context<never>) => {
    const aim = Context.get(state, Target);
    return HashSet.has(aim.followed, instance) ? Option.some({ text: Report.text(new Report({ lines: [new Line({ head: [name, 'exited'], facts: ['during server restart'] })] })), push: true, state: Context.add(state, Target, aim.without([instance])) }) : Option.none();
};
const naming =
    <E, R>(lookup: (workspace: Workspace['Service']) => (given: string) => Effect.Effect<Entry, E, R>) =>
    (given: string) =>
        Effect.tap(
            Workspace.use((workspace) => lookup(workspace)(given)),
            (row) => retarget(() => ({ named: Option.some(row) })),
        );
const find = naming((workspace) => workspace.find);
const resolve = naming((workspace) => workspace.resolve);
const setTarget = Effect.fnUntraced(function* ({ row, instance }: { readonly row: Entry; readonly instance: InstanceSnapshot['instance'] }) {
    const [server, turn] = yield* Effect.all([ApplicationServer, Watches]);
    return yield* turn.withPermit(Effect.tap(server.watch({ instance }), ({ language }) => retarget(({ opened }) => ({ held: Option.some({ row, instance, language }), named: Option.some(row), opened: HashSet.add(opened, instance) }))));
});
const hold = Effect.fnUntraced(function* (taken: readonly Hold[]) {
    const [{ running }, { returnEarly }] = yield* Effect.all([Workspace, ToolCall]);
    const rows = yield* SubscriptionRef.get(running);
    yield* Effect.acquireRelease(
        retarget(({ holds }) => ({ holds: [...holds, ...taken] })),
        () => retarget(({ holds }) => ({ holds: Array.differenceWith(Equivalence.strictEqual<Hold>())(holds, taken) })),
    );
    const awaited = taken.flatMap(
        Hold.$match({
            Using: Array.empty<Stream.Stream<Effect.Effect<string>>>,
            Launching: ({ instance, name }) => [blocked(running)({ instance, name })],
            Quitting: ({ instance }) => Option.toArray(Option.map(Record.get(byInstance(rows), instance), (each) => blocked(running)({ instance, name: each.name ?? each.identifier }, true))),
        }),
    );
    yield* Array.match(awaited, { onEmpty: () => Effect.void, onNonEmpty: (streams) => returnEarly(Stream.mergeAll(streams, { concurrency: 'unbounded' })) });
});
const current = Effect.gen(function* () {
    const { catalog, running } = yield* Workspace;
    const [{ held, named, followed }, rows, entries] = yield* Effect.all([aimed, SubscriptionRef.get(running), SubscriptionRef.get(catalog)]);
    const found = Option.flatMap(held, (aim) => Option.map(Record.get(runningOf(rows), aim.instance), (each) => ({ ...aim, each })));
    const last = Option.orElse(Option.map(held, Struct.get('row')), () => named);
    const { row, instance, language, each } = yield* Effect.fromOption(found, () => new Cause.NoSuchElementError(refuseUntargeted(last, followed, rows, entries)));
    yield* hold([Hold.Using({ instance })]);
    return { row, live: InstanceSnapshot.Watched.make({ ...each, language }) };
});
const ends = Effect.map(Effect.all([aimed, Workspace]), ([{ held }, { running }]) => Option.match(held, { onNone: () => Stream.never, onSome: ({ row, instance }) => blocked(running)({ instance, name: row.name }) }));
const blocking = ({ name, ...row }: Pick<Entry, 'url' | 'name'>) => Effect.map(Workspace, ({ running }) => SubscriptionRef.changes(running).pipe(Stream.map(Array.findFirst(Installation.runs(row))), Stream.filterMap(refuseBlocked(name)), Stream.map(Effect.succeed)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { blocking, current, ends, find, Hold, hold, openCall, resolve, setTarget, Target };
