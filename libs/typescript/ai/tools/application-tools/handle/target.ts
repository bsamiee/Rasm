import { Catalog, Entry } from '@rasm/applications/catalog';
import { ApplicationServer } from '@rasm/applications/server/client';
import { Element } from '@rasm/applications/server/element';
import { ApplicationFailure } from '@rasm/applications/server/failure';
import { Installation } from '@rasm/applications/server/installation';
import { Instance } from '@rasm/applications/server/instance';
import { Call, Format, Line, Report } from '@rasm/mcp/report';
import { Handle, Server } from '@rasm/mcp/server';
import { ToolCall } from '@rasm/mcp/toolkit';
import { Array, Cause, Context, Data, Effect, Equal, Equivalence, Function, HashSet, Layer, Number, Option, Record, Ref, Result, Schema, SchemaGetter, Semaphore, Stream, String, Struct, SubscriptionRef } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Hold = Data.TaggedEnum<{
    Using: Pick<Instance, 'instance'>;
    Launching: Pick<Instance, 'instance'> & Pick<Entry, 'name'>;
    Quitting: Pick<Instance, 'instance'>;
}>;

// --- [MODELS] --------------------------------------------------------------------------

const Hold = Data.taggedEnum<Hold>();
const Stored = Schema.Struct({
    held: Schema.OptionFromOptionalKey(Schema.Struct({ url: Installation.fields.url, instance: Instance.fields.instance, language: Schema.String })),
    followed: Schema.Array(Schema.Struct({ instance: Instance.fields.instance, locators: Element.Locator.pipe(Schema.Array, Schema.OptionFromOptionalKey) })),
    named: Schema.OptionFromOptionalKey(Installation.fields.url),
    opened: Instance.fields.instance.pipe(Schema.Array, Schema.OptionFromOptionalKey),
});
class Aim extends Data.Class<{
    readonly held: Option.Option<{ readonly row: Entry; readonly instance: Instance['instance']; readonly language: string }>;
    readonly named: Option.Option<Entry>;
    readonly holds: readonly Hold[];
    readonly opened: HashSet.HashSet<Instance['instance']>;
}> {
    get followed() {
        return HashSet.fromIterable([...Option.toArray(this.held), ...this.holds].map(({ instance }) => instance));
    }
    quitting(instance: Instance['instance']) {
        return this.holds.some(Equal.equals(Hold.Quitting({ instance })));
    }
    without(gone: Iterable<Instance['instance']>) {
        const left = HashSet.fromIterable(gone);
        const ended = Option.filter(this.held, ({ instance }) => HashSet.has(left, instance));
        return new Aim({
            held: Option.isSome(ended) ? Option.none() : this.held,
            named: Option.orElse(Option.map(ended, Struct.get('row')), () => this.named),
            holds: this.holds.filter(({ instance }) => !HashSet.has(left, instance)),
            opened: HashSet.difference(this.opened, left),
        });
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

const Key = Context.Reference<Aim>('@rasm/application-tools/handle/target/Target', { defaultValue: () => new Aim({ held: Option.none(), named: Option.none(), holds: [], opened: HashSet.empty() }) });
class Watches extends Context.Service<Watches, Semaphore.Semaphore>()('@rasm/application-tools/handle/target/Watches') {
    static readonly layer: Layer.Layer<Watches, never, ApplicationServer | Catalog | Server> = Layer.effect(
        Watches,
        Effect.gen(function* () {
            const [server, catalog, served, turn, tables] = yield* Effect.all([ApplicationServer, Catalog, Server, Semaphore.make(1), Ref.make(Record.empty<string, readonly string[]>())]);
            const followed = Stream.map(served.states(Key), (states) => states.reduce((all, aim) => HashSet.union(all, aim.followed), HashSet.empty<Instance['instance']>()));
            const following = Effect.map(Stream.runHead(followed), Option.getOrElse(HashSet.empty<Instance['instance']>));
            const entryAt = ({ href }: URL) => Effect.tapError(catalog.find(href), (cause) => Effect.logWarning(`Application bundle ${href} is unread, the restored handle forgets it`, cause));
            yield* served.persist(
                '@rasm/applications/workspace/Target',
                Key,
                Stored.pipe(
                    Schema.decodeTo(Schema.instanceOf(Aim), {
                        decode: SchemaGetter.transformEffect(
                            Effect.fnUntraced(function* ({ held, followed: tabled, named, opened }: typeof Stored.Type) {
                                const [row, last] = yield* Effect.all([
                                    held.pipe(
                                        Effect.fromOption,
                                        Effect.flatMap(({ url }) => entryAt(url)),
                                        Effect.option,
                                    ),
                                    named.pipe(Effect.fromOption, Effect.flatMap(entryAt), Effect.option),
                                ]);
                                const stated = Record.getSomes(Record.fromIterableWith(tabled, ({ instance, locators }) => [instance, locators]));
                                yield* Ref.update(tables, (known) => ({ ...known, ...stated }));
                                return new Aim({
                                    held: Option.zipWith(row, held, (entry, { instance, language }) => ({ row: entry, instance, language })),
                                    named: last,
                                    holds: [],
                                    opened: HashSet.fromIterable(Option.getOrElse(opened, Array.empty<Instance['instance']>)),
                                });
                            }),
                        ),
                        encode: SchemaGetter.transformEffect(
                            Effect.fnUntraced(function* ({ held, named, opened }: Aim) {
                                const read = yield* Ref.get(tables);
                                return {
                                    held: Option.map(held, ({ row, instance, language }) => ({ url: row.url, instance, language })),
                                    followed: Option.toArray(held).map(({ instance }) => ({ instance, locators: Record.get(read, instance) })),
                                    named: Option.map(named, Struct.get('url')),
                                    opened: Option.some(Iterator.from(opened).toArray()),
                                };
                            }),
                        ),
                    }),
                ),
            );
            const [restored, rows, seeds] = yield* Effect.all([Stream.runHead(served.states(Key)), SubscriptionRef.get(server.running), Ref.getAndSet(tables, Record.empty<string, readonly string[]>())]);
            const [live, gone] = Array.partition(
                Option.getOrElse(restored, Array.empty<Aim>).flatMap(({ held }) => Option.toArray(held)),
                (target) => (Record.has(Instance.running(rows), target.instance) ? Result.succeed(target) : Result.fail(target)),
            );
            yield* Effect.forEach(live, ({ instance }) => Effect.ignore(server.watch({ instance, ...Option.getOrUndefined(Option.map(Record.get(seeds, instance), (locators) => ({ locators }))) }), { log: 'Warn', message: `Watch of ${instance} failed to restore, its references are read again` }), {
                discard: true,
            });
            yield* served.events({
                stream: Stream.fromIterable(gone),
                route: ({ row, instance }, state) => {
                    const aim = Context.get(state, Key);
                    const line = new Line({ head: [row.name, Instance.State.cases.exited.fields._tag.schema.literal], facts: ['during server restart'] });
                    return HashSet.has(aim.followed, instance) ? Option.some({ text: Report.text(new Report({ lines: [line] })), push: true, state: Context.add(state, Key, aim.without([instance])) }) : Option.none();
                },
            });
            yield* followed.pipe(
                Stream.changes,
                Stream.runFoldEffect(
                    HashSet.empty<Instance['instance']>,
                    Effect.fnUntraced(function* (kept: HashSet.HashSet<Instance['instance']>, seen: HashSet.HashSet<Instance['instance']>) {
                        const now = yield* following;
                        yield* Effect.forEach(HashSet.difference(HashSet.union(kept, seen), now), (instance) => server.release({ instance }), { discard: true }).pipe(Effect.ignoreCause({ log: 'Warn', message: 'Watch release failed, the Watch stays until its instance exits' }));
                        return now;
                    }, Semaphore.withPermit(turn)),
                ),
                Effect.forkScoped,
            );
            yield* Effect.addFinalizer(
                Effect.fnUntraced(function* () {
                    const read = yield* Effect.all(
                        Record.fromIterableWith(yield* following, (instance) => [instance, server.release({ instance })]),
                        { concurrency: 'unbounded', mode: 'result' },
                    );
                    const [released, unreleased] = Record.partition(read, Function.identity);
                    yield* Effect.forEach(Record.values(unreleased), (cause) => Effect.logWarning('Watch release failed at shutdown, the references of its instance are read again after the restart', cause), { discard: true });
                    yield* Ref.set(tables, released);
                }),
            );
            return turn;
        }),
    );
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const retarget = (change: (aim: Aim) => Partial<Pick<Aim, 'held' | 'named' | 'holds' | 'opened'>>) => Handle.use((handle) => handle.update(Key, (aim) => new Aim({ ...aim, ...change(aim) })));
const remember =
    <E, R>(lookup: (catalog: Catalog['Service']) => (given: string) => Effect.Effect<Entry, E, R>) =>
    (given: string) =>
        Effect.tap(
            Catalog.use((catalog) => lookup(catalog)(given)),
            (row) => retarget(() => ({ named: Option.some(row) })),
        );
const open = (application: string) => new Call({ tool: 'applications', given: { action: 'open', application } });
const diagnose = (name: string) => (found: Option.Option<Instance>) => {
    const lines = Option.match(Option.map(found, Struct.get('state')), {
        onNone: () => [new Line({ head: [name, 'not running'] })],
        onSome: Instance.State.match({
            exited: ({ _tag }) => [new Line({ head: [name, _tag], call: open(name) })],
            failed: Array.empty<Line>,
            running: ({ status, windows }) => [
                ...windows.filter(Struct.get('asking')).map(({ kind = 'window', title }) => new Line({ head: [name, 'asking', String.noCase()(kind), ...Array.fromNullishOr(title).map(Format.quoted)] })),
                ...Instance.State.Status.matchOrElse(status, { unresponsive: ({ _tag, thread }) => [new Line({ head: [name, _tag], facts: [`main thread ${thread}`] })] }, Array.empty<Line>),
            ],
        }),
    });
    return Array.isReadonlyArrayEmpty(lines) ? Result.failVoid : Result.succeed(Report.text(new Report({ lines })));
};
const track = (running: ApplicationServer['Service']['running'], instance: Instance['instance'], name: string, quitting = false) =>
    SubscriptionRef.changes(running).pipe(
        Stream.map((rows) => Record.get(Instance.byInstance(rows), instance)),
        Stream.dropWhile(Option.isNone),
        Stream.takeWhile((found) => !quitting || Option.exists(found, ({ state }) => !Instance.State.isAnyOf(['exited'])(state))),
        Stream.filterMap(diagnose(name)),
        Stream.map(Effect.succeed),
    );
const set = Effect.fnUntraced(function* ({ row, instance }: { readonly row: Entry; readonly instance: Instance['instance'] }) {
    const [server, turn] = yield* Effect.all([ApplicationServer, Watches]);
    return yield* turn.withPermit(Effect.tap(server.watch({ instance }), ({ language }) => retarget(({ opened }) => ({ held: Option.some({ row, instance, language }), named: Option.some(row), opened: HashSet.add(opened, instance) }))));
});
const hold = Effect.fnUntraced(function* (taken: readonly Hold[]) {
    const [{ running }, { returnEarly }] = yield* Effect.all([ApplicationServer, ToolCall]);
    const rows = Instance.byInstance(yield* SubscriptionRef.get(running));
    const release = Effect.gen(function* () {
        const live = Instance.running(yield* SubscriptionRef.get(running));
        const released = taken.filter((each) => !Hold.$is('Quitting')(each) || Record.has(live, each.instance));
        yield* retarget(({ holds }) => ({ holds: Array.differenceWith(Equivalence.strictEqual<Hold>())(holds, released) }));
    });
    yield* Effect.acquireRelease(
        retarget(({ holds }) => ({ holds: [...holds, ...taken] })),
        () => release,
    );
    const early = taken.flatMap(
        Hold.$match({
            Using: Array.empty<ReturnType<typeof track>>,
            Launching: ({ instance, name }) => [track(running, instance, name)],
            Quitting: ({ instance }) => Option.toArray(Record.get(rows, instance)).map((each) => track(running, instance, each.name ?? each.identifier, true)),
        }),
    );
    yield* Array.match(early, { onEmpty: () => Effect.void, onNonEmpty: (streams) => returnEarly(Stream.mergeAll(streams, { concurrency: 'unbounded' })) });
});
const current = Effect.gen(function* () {
    const [catalog, { running }, handle] = yield* Effect.all([Catalog, ApplicationServer, Handle]);
    const [{ held, named, followed }, rows, entries] = yield* Effect.all([handle.get(Key), SubscriptionRef.get(running), SubscriptionRef.get(catalog.entries)]);
    const found = Option.flatMap(held, ({ row, instance, language }) => Option.map(Record.get(Instance.running(rows), instance), (seen) => ({ row, live: Instance.Watched.make({ ...seen, language }) })));
    const aimed = yield* Effect.fromOption(found, () => {
        const last = Option.orElse(Option.map(held, Struct.get('row')), () => named);
        const meant = Option.flatMap(last, (application) => Array.head(Entry.instancesOf(rows, followed)(application).meant));
        const standing = Option.match(Option.map(meant, Struct.get('state')), {
            onNone: () => 'not running',
            onSome: Instance.State.matchOrElse({ failed: ({ reason, subject, values }) => ApplicationFailure.of(reason, subject, values).message }, ({ _tag }) => _tag),
        });
        const driven = Record.filter(Entry.byBundle(entries), ({ claim }) => Option.isNone(claim));
        const shown = rows.filter(({ state }) => Instance.State.matchOrElse(state, { running: ({ windows, statusItems }) => windows.some(Struct.get('presented')) || Option.exists(Option.flatMap(Option.fromUndefinedOr(statusItems), Result.getSuccess), Number.isGreaterThan(0)) }, Function.constFalse));
        const [listed, rest] = Array.splitAt(Array.dedupe(shown.flatMap(({ url }) => Option.toArray(Record.get(driven, url.href))).map(Struct.get('name'))), 5);
        const names = Array.isReadonlyArrayEmpty(rest) ? listed : [...listed, Format.count(rest.length, { other: 'more' })];
        const report = Option.match(last, {
            onNone: () => new Report({ lines: [new Line({ head: ['no target'], call: open('<name>') }), ...(Array.isReadonlyArrayEmpty(names) ? [] : [new Line({ head: ['showing'], value: Format.list(names) })])] }),
            onSome: ({ name }) => new Report({ lines: [new Line({ head: ['no target'], facts: [`${name} ${standing}`] })], next: open(name) }),
        });
        return new Cause.NoSuchElementError(Report.text(report));
    });
    yield* hold([Hold.Using({ instance: aimed.live.instance })]);
    return aimed;
});
const ends = Effect.gen(function* () {
    const [{ held }, { running }] = yield* Effect.all([Handle.use((handle) => handle.get(Key)), ApplicationServer]);
    return Option.match(held, { onNone: () => Stream.never, onSome: ({ row, instance }) => track(running, instance, row.name) });
});
const blocking = ({ name, ...row }: Pick<Entry, 'url' | 'name'>) => Effect.map(ApplicationServer, ({ running }) => SubscriptionRef.changes(running).pipe(Stream.map(Array.findFirst(Instance.runs(row))), Stream.filterMap(diagnose(name)), Stream.map(Effect.succeed)));
const Target = Object.assign(Key, { layer: Watches.layer, Hold, current, find: remember((catalog) => catalog.find), resolve: remember((catalog) => catalog.resolve), set, hold, ends, blocking, open });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Target };
