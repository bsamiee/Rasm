import { type UserInfo, userInfo } from 'node:os';
import { basename, extname } from 'node:path';
import { Catalog, Relation } from '@rasm/database';
import { Array, type Cause, Data, Effect, Equal, Fiber, Option, Order, Predicate, Queue, Record, Ref, Schema, Stream, Struct } from 'effect';
import { SqlClient } from 'effect/sql';

// --- [TYPES] ---------------------------------------------------------------------------

interface Entry {
    readonly container: typeof Container.Type;
    readonly controls: Readonly<Record<string, typeof Control.Type>>;
}
interface Folded {
    readonly control: typeof Control.Type;
    readonly changes: readonly (typeof Change.Type)[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const LIMITS = { builds: 8, openers: 4, misses: 3, epochs: 2, pairing: 8, batch: 64, interval: '2 seconds' } as const;
const REDACTED = { home: '~', other: '<private>' } as const;
const STATIC = ['kind', 'role', 'subrole', 'help', 'range', 'shortcut', 'mark', 'parent', 'positional'] as const;
const PAIRED = ['parent', 'role', 'kind', 'help'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Locator = Schema.String.annotate({ description: 'Canonical encoding of a locator, its steps from the application element down' });
const Build = Schema.String.annotate({ description: 'CFBundleVersion of the instance read' });
const Instant = Schema.DateTimeUtcFromString;
const Value = Schema.Union([Schema.Boolean, Schema.Finite, Schema.String]);
const Frame = Schema.Tuple([Schema.Tuple([Schema.Finite, Schema.Finite]), Schema.Tuple([Schema.Finite, Schema.Finite])]).annotate({ description: 'Origin in its container and size, in points' });
const Kind = Schema.Literals(['action', 'toggle', 'choice', 'multi', 'row', 'number', 'measure', 'text', 'color', 'date', 'item', 'region']);
const Valued = Kind.pick(['choice', 'toggle', 'number', 'measure', 'color', 'date']);
const Condition = Schema.TaggedUnion({
    Value: { at: Locator, value: Value },
    Documents: { held: Schema.Boolean },
    Modal: { at: Schema.NullOr(Locator) },
});
const Opener = Schema.Struct({ container: Locator, control: Locator });
const Shape = Schema.Struct({
    kind: Kind,
    role: Schema.String.annotate({ description: 'AXRole' }),
    subrole: Schema.OptionFromOptionalKey(Schema.String),
    label: Schema.OptionFromOptionalKey(Schema.String),
    help: Schema.OptionFromOptionalKey(Schema.String),
    range: Schema.OptionFromOptionalKey(Schema.Struct({ minimum: Schema.optionalKey(Schema.Finite), maximum: Schema.optionalKey(Schema.Finite), step: Schema.optionalKey(Schema.Finite) })),
    options: Schema.OptionFromOptionalKey(Schema.Array(Schema.String)),
    shortcut: Schema.OptionFromOptionalKey(Schema.String),
    mark: Schema.OptionFromOptionalKey(Schema.String),
    parent: Schema.OptionFromOptionalKey(Locator),
    positional: Schema.Boolean.annotate({ description: 'Keyed by its offset from the nearest keyed sibling' }),
});
const Observation = Schema.Struct({
    build: Build,
    container: Locator,
    kind: Schema.Literals(['window', 'dialog', 'panel', 'sheet', 'popover', 'drawer', 'prompt', 'menuBar', 'menu', 'statusItem', 'toolbar', 'tabs', 'web']),
    title: Schema.OptionFromOptionalKey(Schema.String),
    document: Schema.Boolean.annotate({ description: 'The window holds AXDocument' }),
    modal: Schema.Boolean,
    layer: Schema.Int.annotate({ description: 'Window server layer' }),
    size: Schema.Tuple([Schema.Finite, Schema.Finite]),
    default: Schema.OptionFromOptionalKey(Locator),
    cancel: Schema.OptionFromOptionalKey(Locator),
    close: Schema.OptionFromOptionalKey(Locator),
    opened: Instant.annotate({ description: 'When the container last opened, the epoch of this read' }),
    at: Instant.annotate({ description: 'When this read was made' }),
    complete: Schema.Boolean.annotate({ description: 'The read reached every element of the container' }),
    cause: Schema.OptionFromOptionalKey(Opener.annotate({ description: 'Control whose step opened the container' })),
    documents: Schema.Array(Schema.String.annotate({ description: 'Path of a document open in the instance' })),
    scope: Schema.Array(Condition),
    controls: Schema.Record(Locator, Shape),
    layout: Schema.Record(Locator, Frame),
    values: Schema.Record(Locator, Value),
}).annotate({ description: 'One read of a container: its static facts by locator, their layout and values, and the atoms in scope' });
const Tally = Schema.Struct({
    seen: Schema.Natural.annotate({ description: 'Epochs it was seen in' }),
    missed: Schema.Natural.annotate({ description: 'Complete reads that lacked it' }),
    first: Instant,
    last: Instant,
    epoch: Instant.annotate({ description: 'Opening of the last epoch it was seen in' }),
    since: Build,
    verified: Build.annotate({ description: 'Build it was last seen in' }),
    persistent: Schema.Boolean.annotate({ description: 'Seen again after retiring' }),
    unexplained: Schema.Array(Instant).annotate({ description: 'Epochs of the last three misses no atom explains' }),
});
const Presence = Schema.Struct({ with: Schema.Array(Condition), against: Schema.Array(Condition) });
const Application = Schema.Struct({ builds: Schema.NonEmptyArray(Build).annotate({ description: 'Builds read, newest first' }) });
const Container = Schema.Struct({
    ...Struct.pick(Observation.fields, ['kind', 'title', 'modal', 'layer', 'size', 'default', 'cancel', 'close']),
    openers: Schema.Array(Schema.Struct({ ...Opener.fields, tally: Tally })),
    tally: Tally,
    scopes: Schema.Array(Schema.Struct({ at: Instant, atoms: Schema.Array(Condition) })).annotate({ description: 'Atoms of each read holding the last sighting of a control with no condition yet' }),
});
const Control = Schema.Struct({
    ...Struct.omit(Shape.fields, ['options']),
    options: Schema.OptionFromOptionalKey(Schema.Union([Schema.Array(Schema.String), Schema.Struct({ count: Schema.Natural })])),
    frame: Schema.OptionFromOptionalKey(Frame),
    value: Schema.OptionFromOptionalKey(Value),
    opens: Schema.OptionFromOptionalKey(Locator),
    presence: Schema.OptionFromOptionalKey(Presence),
    tally: Tally,
    retired: Schema.OptionFromOptionalKey(Build),
});
const Change = Schema.TaggedUnion({
    Appeared: { at: Locator },
    Changed: { at: Locator },
    Relabelled: { at: Locator, from: Schema.OptionFromOptionalKey(Schema.String), to: Schema.OptionFromOptionalKey(Schema.String) },
    Gone: { at: Locator },
});
const Keyed = { bundle: Schema.String, language: Schema.String };
const applications = Relation.make('applications', { ...Keyed, body: Schema.fromJsonString(Application) }, { key: ['bundle', 'language'], withoutRowid: true });
const containers = Relation.make('containers', { ...Keyed, locator: Locator, body: Schema.fromJsonString(Container) }, { key: ['bundle', 'language', 'locator'], withoutRowid: true });
const controls = Relation.make('controls', { ...Keyed, container: Locator, locator: Locator, body: Schema.fromJsonString(Control) }, { key: ['bundle', 'language', 'container', 'locator'], withoutRowid: true });
const key = Condition.match({ Value: ({ _tag, at }) => ({ _tag, at }), Documents: ({ _tag }) => ({ _tag }), Modal: ({ _tag }) => ({ _tag }) });

class InterfaceMap extends Data.Class<{ readonly application: Option.Option<typeof Application.Type>; readonly containers: Readonly<Record<string, Entry>> }> {
    static readonly Observation = Observation;
    static readonly Change = Change;
    static readonly empty = new InterfaceMap({ application: Option.none(), containers: {} });
    static readonly catalog = Catalog.make({ tables: [applications, containers, controls] });
    static readonly open = Effect.fn('InterfaceMap.open')(function* (bundle: string, language: string) {
        const where = { bundle: [bundle], language: [language] } as const;
        const [held, entries, items, user] = yield* Effect.all([Relation.select(applications, where), Relation.select(containers, where), Relation.select(controls, where), Effect.sync(() => userInfo())]);
        const grouped = Array.groupBy(items, Struct.get('container'));
        const state = yield* Ref.make(
            new InterfaceMap({
                application: Option.map(Array.head(held), Struct.get('body')),
                containers: Record.fromIterableWith(entries, ({ locator, body }) => [locator, { container: body, controls: Record.fromIterableWith(grouped[locator] ?? [], (row) => [row.locator, row.body]) }]),
            }),
        );
        const dirty = yield* Queue.unbounded<string, Cause.Done>();
        const write = (map: InterfaceMap, touched: readonly string[]) => {
            const kept = Array.getSomes(touched.map((locator) => Option.map(Record.get(map.containers, locator), (entry) => ({ locator, entry }))));
            const rows = kept.flatMap(({ locator, entry }) => Record.collect(entry.controls, (control, body) => ({ bundle, language, container: locator, locator: control, body })));
            return SqlClient.SqlClient.use((sql) =>
                sql.withTransaction(
                    Effect.all([
                        Option.match(map.application, { onNone: () => Effect.void, onSome: (body) => Relation.upsert(applications, [{ bundle, language, body }]) }),
                        Array.match(
                            kept.map(({ locator, entry }) => ({ bundle, language, locator, body: entry.container })),
                            { onEmpty: () => Effect.void, onNonEmpty: (written) => Relation.upsert(containers, written) },
                        ),
                        Array.match(rows, { onEmpty: () => Effect.void, onNonEmpty: (written) => Relation.upsert(controls, written) }),
                        Effect.forEach(touched, (locator) => Relation.remove(controls, { ...where, container: [locator], ...Array.match(Record.keys(map.containers[locator]?.controls ?? {}), { onEmpty: () => ({}), onNonEmpty: (present) => ({ locator: { not: present } }) }) })),
                    ]),
                ),
            );
        };
        const flushing = yield* Stream.fromQueue(dirty).pipe(
            Stream.groupedWithin(LIMITS.batch, LIMITS.interval),
            Stream.runForEach((touched) => Effect.flatMap(Ref.get(state), (map) => write(map, Array.dedupe(touched)))),
            Effect.forkScoped,
        );
        yield* Effect.addFinalizer(() => Effect.orDie(Effect.andThen(Queue.end(dirty), Fiber.join(flushing))));
        return {
            present: Ref.get(state),
            observe: (read: typeof Observation.Type) =>
                Ref.modify(state, (map) => {
                    const { map: next, changes, touched } = map.fold(read, user);
                    return [{ changes, touched }, next];
                }).pipe(
                    Effect.tap(({ touched }) => Queue.offerAll(dirty, touched)),
                    Effect.map(({ changes }) => changes),
                ),
        };
    });

    fold(read: typeof Observation.Type, { username, homedir }: Pick<UserInfo<string>, 'username' | 'homedir'>) {
        const texts = Array.sort(Array.dedupe([...read.documents.flatMap((path) => [path, basename(path), basename(path, extname(path))]), homedir, username]), Order.flip(Order.mapInput(Order.Number, (text: string) => text.length)));
        const pattern = new RegExp(texts.map((text) => `(?<![\\p{L}\\p{N}])${RegExp.escape(text)}(?![\\p{L}\\p{N}])`).join('|'), 'gu');
        const hide = (text: string) => text.replaceAll(pattern, (found) => (found === homedir ? REDACTED.home : REDACTED.other));
        const valued = (value: typeof Value.Type) => (typeof value === 'string' ? hide(value) : value);
        const { build, opened, at, complete } = read;
        const container = hide(read.container);
        const scope: (typeof Observation.Type)['scope'] = read.scope.map(Condition.match({ Value: (atom) => ({ ...atom, at: hide(atom.at), value: valued(atom.value) }), Documents: (atom) => atom, Modal: (atom) => ({ ...atom, at: atom.at === null ? null : hide(atom.at) }) }));
        const layout = Record.mapKeys(read.layout, hide);
        const values = Record.map(Record.mapKeys(read.values, hide), valued);
        const cause = Option.map(read.cause, Struct.evolve({ container: hide, control: hide }));
        const shapes = Record.map(
            Record.filter(Record.mapKeys(read.controls, hide), ({ kind }) => kind !== 'row'),
            Struct.evolve({ label: Option.map(hide), help: Option.map(hide), options: Option.map(Array.map(hide)), parent: Option.map(hide) }),
        );
        const builds = Option.match(this.application, { onNone: () => Array.of(build), onSome: ({ builds: listed }) => (Array.headNonEmpty(listed) === build ? listed : Array.prepend(Array.take(listed, LIMITS.builds - 1), build)) });
        const entry = Option.fromUndefinedOr(this.containers[container]);
        const fresh = !Option.exists(entry, ({ container: held }) => Equal.equals(held.tally.epoch, opened));
        const sight = (tally: Option.Option<typeof Tally.Type>): typeof Tally.Type =>
            Option.match(tally, {
                onNone: () => ({ seen: 1, missed: 0, first: at, last: at, epoch: opened, since: build, verified: build, persistent: false, unexplained: [] }),
                onSome: (held) => ({ ...held, seen: held.seen + (Equal.equals(held.epoch, opened) ? 0 : 1), last: at, epoch: opened, verified: build }),
            });
        const stored: Entry['controls'] = Option.match(entry, { onNone: () => ({}), onSome: ({ controls: held }) => held });
        const scopes: Entry['container']['scopes'] = Option.match(entry, { onNone: () => [], onSome: ({ container: held }) => held.scopes });
        const candidates = Record.toEntries(stored).filter(([locator, control]) => !Record.has(shapes, locator) && control.positional && Option.isNone(control.retired) && control.tally.verified !== build);
        const [, pairs] = Array.mapAccum(
            Record.toEntries(shapes).filter(([locator, shape]) => shape.positional && !Record.has(stored, locator)),
            candidates,
            (left, [locator, shape]) =>
                Option.match(
                    Array.findFirst(left, ([, control]) => Equal.equals(Struct.pick(control, PAIRED), Struct.pick(shape, PAIRED)) && Option.exists(Option.all([control.frame, Record.get(layout, locator)]), ([[[x, y]], [[across, down]]]) => Math.max(Math.abs(x - across), Math.abs(y - down)) <= LIMITS.pairing)),
                    { onNone: () => [left, []], onSome: ([from]) => [left.filter(([held]) => held !== from), [[locator, from] as const]] },
                ),
        );
        const paired: Readonly<Record<string, string>> = Object.fromEntries(pairs.flat());
        const moved = new Set(Object.values(paired));
        const seen = Record.map(shapes, (shape, locator): Folded => {
            const prior = Option.fromUndefinedOr(stored[paired[locator] ?? locator]);
            const [kept, current] = [Option.flatMap(prior, Struct.get('options')), Option.exists(prior, ({ tally }) => tally.verified === build)];
            const options = Option.map(shape.options, (listed) => (Option.exists(kept, (held) => !Equal.equals(held, listed) && (!Array.isArray(held) || current)) ? { count: listed.length } : listed));
            const control: typeof Control.Type = {
                ...shape,
                options,
                frame: Record.get(layout, locator),
                value: Option.filter(Record.get(values, locator), () => Schema.is(Valued)(shape.kind)),
                opens: Option.flatMap(prior, Struct.get('opens')),
                presence: Option.map(Option.flatMap(prior, Struct.get('presence')), (held) => ({ ...held, with: Array.union(held.with, scope) })),
                tally: { ...sight(Option.map(prior, Struct.get('tally'))), persistent: Option.exists(prior, ({ tally, retired }) => tally.persistent || Option.isSome(retired)) },
                retired: Option.none(),
            };
            const changes = Option.match(prior, {
                onNone: () => [{ _tag: 'Appeared', at: locator } as const],
                onSome: (held) => [
                    ...(Option.isSome(held.retired) ? [{ _tag: 'Appeared', at: locator } as const] : []),
                    ...(Equal.equals(held.label, shape.label) ? [] : [{ _tag: 'Relabelled', at: locator, from: held.label, to: shape.label } as const]),
                    ...(Equal.equals(Struct.pick(held, STATIC), Struct.pick(shape, STATIC)) && (Equal.equals(held.options, options) || !Option.exists(options, Array.isArray)) ? [] : [{ _tag: 'Changed', at: locator } as const]),
                ],
            });
            return { control, changes };
        });
        const absent = Record.filter(stored, (control, locator) => !Record.has(shapes, locator) && !moved.has(locator) && !Option.exists(control.retired, (retiring) => !Array.take(builds, 2).includes(retiring)));
        const missed = Record.map(absent, (control, locator): Folded => {
            const seeded: typeof Presence.Type = Option.getOrElse(control.presence, () => ({ with: scopes.filter((held) => Equal.equals(held.at, control.tally.last)).flatMap(Struct.get('atoms')), against: [] }));
            const explained = scope.some((atom) => seeded.with.some((held) => Equal.equals(key(held), key(atom))) && !seeded.with.some((held) => Equal.equals(held, atom)));
            const unexplained = explained ? control.tally.unexplained : Array.take(Array.prepend(control.tally.unexplained, opened), LIMITS.misses);
            const retiring = !explained && (control.tally.verified !== build || (!control.tally.persistent && unexplained.length >= LIMITS.misses && Array.dedupe(unexplained).length >= LIMITS.epochs));
            return Option.isSome(control.retired) || !complete
                ? { control, changes: [] }
                : {
                      control: { ...control, presence: Option.some(explained ? { ...seeded, against: Array.union(seeded.against, scope) } : seeded), tally: { ...control.tally, missed: control.tally.missed + 1, unexplained }, retired: retiring ? Option.some(build) : Option.none() },
                      changes: retiring ? [{ _tag: 'Gone', at: locator } as const] : [],
                  };
        });
        const folded = { ...seen, ...missed };
        const held = Record.map(folded, Struct.get('control'));
        const openers: Entry['container']['openers'] = Option.match(entry, { onNone: () => [], onSome: ({ container: prior }) => prior.openers });
        const next = Record.set(this.containers, container, {
            container: {
                ...Struct.pick(read, ['kind', 'modal', 'layer', 'size']),
                ...Record.map(Struct.pick(read, ['default', 'cancel', 'close']), Option.map(hide)),
                title: read.document ? Option.none() : Option.map(read.title, hide),
                openers: Option.match(
                    Option.filter(cause, () => fresh),
                    {
                        onNone: () => openers,
                        onSome: (by) => {
                            const same = (opener: (typeof openers)[number]) => Equal.equals(Struct.pick(opener, ['container', 'control']), by);
                            return Array.take([{ ...by, tally: sight(Option.map(Array.findFirst(openers, same), Struct.get('tally'))) }, ...openers.filter(Predicate.not(same))], LIMITS.openers);
                        },
                    },
                ),
                tally: sight(Option.map(entry, ({ container: prior }) => prior.tally)),
                scopes: [{ at, atoms: scope }, ...scopes].filter((reading) => Record.some(held, (control) => Option.isNone(control.presence) && Option.isNone(control.retired) && Equal.equals(control.tally.last, reading.at))),
            },
            controls: held,
        });
        const linked = Option.gen(function* () {
            const by = yield* Option.filter(cause, ({ container: from }) => fresh && from !== container);
            const from = yield* Record.get(next, by.container);
            const opening = yield* Record.modify(from.controls, by.control, (control) => ({ ...control, opens: Option.some(container) }));
            return [by.container, { ...from, controls: opening }] as const;
        });
        return {
            map: new InterfaceMap({ application: Option.some({ builds }), containers: Option.match(linked, { onNone: () => next, onSome: ([from, linking]) => Record.set(next, from, linking) }) }),
            changes: [...(Option.isNone(entry) ? [{ _tag: 'Appeared', at: container } as const] : []), ...Record.values(folded).flatMap(({ changes }) => changes)],
            touched: [container, ...Option.toArray(Option.map(linked, ([from]) => from))],
        };
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { InterfaceMap };
