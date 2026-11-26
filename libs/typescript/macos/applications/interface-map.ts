import { type UserInfo, userInfo } from 'node:os';
import { basename, extname, join } from 'node:path';
import { layer as sqlite } from '@effect/sql-sqlite-node/SqliteClient';
import { Catalog, Relation } from '@rasm/database';
import { Sqlite } from '@rasm/database/sqlite';
import { Output } from '@rasm/mcp/cli';
import { Array, Context, Data, Duration, Effect, Equal, FileSystem, Function, HashSet, Layer, Option, Order, Predicate, pipe, RcMap, Record, Ref, Schedule, Schema, Stream, String, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { SqlClient } from 'effect/sql';
import { ApplicationServer } from '#/client';
import { Build, Condition, Frame, Instant, type InstanceSnapshot, InstanceState, Locator, Observation, Opener, Shape, Value } from '#/contract';
import { Workspace } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

type Driven = Key & Pick<InstanceSnapshot, 'instance'>;
type Opened = Effect.Success<ReturnType<typeof InterfaceMap.open>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const LIMITS = { builds: 8, openers: 4, misses: 3, epochs: 2, pairing: 8, interval: '2 seconds' } as const;
const REDACTED = { home: '~', other: '<private>' } as const;
const STATIC = ['kind', 'role', 'subrole', 'help', 'range', 'shortcut', 'mark', 'separated', 'alternate', 'parent', 'positional'] as const;
const PAIRED = ['parent', 'role', 'kind', 'help'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const subject = Condition.match({ Value: ({ _tag, at }) => ({ _tag, at }), Documents: ({ _tag }) => ({ _tag }), Modal: ({ _tag }) => ({ _tag }) });
class Tally extends Schema.Opaque<Tally, unknown>()(
    Schema.Struct({
        seen: Schema.Natural.annotate({ description: 'Epochs it was seen in' }),
        missed: Schema.Natural.annotate({ description: 'Complete reads that lacked it' }),
        first: Instant,
        last: Instant,
        epoch: Instant.annotate({ description: 'Opening of the last epoch it was seen in' }),
        since: Build,
        verified: Build.annotate({ description: 'Build it was last seen in' }),
        persistent: Schema.Boolean.annotate({ description: 'Seen again after retiring' }),
        unexplained: Schema.Array(Instant).annotate({ description: 'Epochs of the last three misses no atom explains' }),
    }),
) {}
const Presence = Schema.Struct({ with: Schema.Array(Condition), against: Schema.Array(Condition) });
class Application extends Schema.Opaque<Application, unknown>()(Schema.Struct({ builds: Schema.Array(Build).annotate({ description: 'Builds read, newest first' }) })) {}
class Container extends Schema.Opaque<Container, unknown>()(
    Schema.Struct({
        ...Struct.pick(Observation.fields, ['kind', 'title', 'modal', 'layer', 'size', 'default', 'cancel', 'close']),
        openers: Schema.Array(Schema.Struct({ ...Opener.fields, tally: Tally })),
        tally: Tally,
        scopes: Schema.Array(Schema.Struct({ at: Instant, atoms: Schema.Array(Condition) })).annotate({ description: 'Atoms of each read holding the last sighting of a control with no condition yet' }),
    }),
) {}
const Control = Schema.Struct({
    ...Struct.omit(Shape.fields, ['options']),
    options: Schema.OptionFromOptionalKey(Schema.Union([Schema.Array(Schema.String), Schema.Struct({ count: Schema.Natural })])),
    frame: Schema.OptionFromOptionalKey(Frame),
    value: Schema.OptionFromOptionalKey(Value),
    presence: Schema.OptionFromOptionalKey(Presence),
    tally: Tally,
    retired: Schema.OptionFromOptionalKey(Build),
});
class Entry extends Schema.Opaque<Entry, unknown>()(Schema.Struct({ container: Container, controls: Schema.Record(Locator, Control) }).annotate({ description: 'One container and every control read in it, keyed by locator' })) {}
const Change = Schema.TaggedUnion({
    Appeared: { at: Locator },
    Changed: { at: Locator },
    Relabelled: { at: Locator, from: Schema.OptionFromOptionalKey(Schema.String), to: Schema.OptionFromOptionalKey(Schema.String) },
    Gone: { at: Locator },
});
class Key extends Schema.Opaque<Key, unknown>()(Schema.Struct({ bundle: Schema.String, language: Schema.String })) {}
const Stored = <S extends Schema.Constraint>(document: S) => Schema.fromJsonString(Schema.OptionFromNullOr(document).pipe(Schema.catchDecoding(() => Effect.succeedSome(Option.none()))));
const applications = Relation.make('applications', { ...Key.fields, body: Stored(Application) }, { key: ['bundle', 'language'], withoutRowid: true });
const containers = Relation.make('containers', { ...Key.fields, locator: Locator, body: Stored(Entry) }, { key: ['bundle', 'language', 'locator'], withoutRowid: true });

class InterfaceMap extends Data.Class<{ readonly application: Application; readonly containers: Readonly<Record<string, Entry>> }> {
    static readonly Change = Change;
    static readonly empty = new InterfaceMap({ application: { builds: [] }, containers: {} });
    static readonly catalog = Catalog.make({ tables: [applications, containers] });
    static readonly open = Effect.fn('InterfaceMap.open')(function* (bundle: string, language: string) {
        const where = { bundle: [bundle], language: [language] } as const;
        const [held, entries, user, sql] = yield* Effect.all([Relation.select(applications, where), Relation.select(containers, where), Effect.sync(() => userInfo()), SqlClient.SqlClient]);
        const stale = Array.countBy([...held, ...entries], ({ body }) => Option.isNone<unknown>(body));
        yield* stale > 0 ? Effect.annotateLogs(Effect.logInfo('Interface map dropped stored documents this build no longer reads'), { bundle, language, stale }) : Effect.void;
        const loaded = new InterfaceMap({
            application: Array.head(held).pipe(
                Option.flatMap(Struct.get('body')),
                Option.getOrElse(() => InterfaceMap.empty.application),
            ),
            containers: Record.getSomes(Record.fromIterableWith(entries, ({ locator, body }) => [locator, body])),
        });
        const [state, written] = yield* Effect.all([Ref.make(loaded), Ref.make(loaded)]);
        const flush = Effect.fnUntraced(
            function* (closing: boolean) {
                const [current, stored] = yield* Effect.all([Ref.get(state), Ref.get(written)]);
                const changed = Record.filter(current.containers, (entry, locator) => entry !== stored.containers[locator] && (closing || !Option.contains(Option.map(Record.get(stored.containers, locator), settled), settled(entry))));
                const writes = [
                    ...(current.application === stored.application ? [] : [Relation.upsert(applications, [{ bundle, language, body: Option.some(current.application) }])]),
                    ...Array.match(
                        Record.collect(changed, (locator, body) => ({ bundle, language, locator, body: Option.some(body) })),
                        { onEmpty: Array.empty, onNonEmpty: (rows) => [Relation.upsert(containers, rows)] },
                    ),
                ];
                yield* Array.match(writes, {
                    onEmpty: () => Effect.void,
                    onNonEmpty: (statements) => Effect.andThen(sql.withTransaction(Effect.all(statements)), Ref.set(written, new InterfaceMap({ application: current.application, containers: { ...stored.containers, ...changed } }))),
                });
            },
            Effect.catch((error) => Effect.annotateLogs(Effect.logWarning('Interface map flush failed', error), { bundle, language })),
        );
        yield* Effect.addFinalizer(() => flush(true));
        yield* Effect.forkScoped(Effect.repeat(flush(false), Schedule.spaced(LIMITS.interval)));
        return {
            present: Ref.get(state),
            observe: (read: Observation) =>
                Ref.modify(state, (map) => {
                    const { map: next, changes } = map.fold(read, user);
                    return [changes, next] as const;
                }),
        };
    });

    get known() {
        return Option.map(Array.head(this.application.builds), (build) => ({ build, containers: Record.keys(Record.filter(this.containers, ({ container }) => container.tally.verified === build)) }));
    }

    fold(read: Observation, { username, homedir }: Pick<UserInfo<string>, 'username' | 'homedir'>) {
        const { build, opened, at, complete } = read;
        const texts = Array.sort(Array.dedupe([...read.documents.flatMap((path) => [path, basename(path), basename(path, extname(path))]), homedir, username]), Order.mapInput(Order.flip(Order.Number), String.length));
        const pattern = new RegExp(`(?<![\\p{L}\\p{N}])(?:${texts.map(RegExp.escape).join('|')})(?![\\p{L}\\p{N}])`, 'gu');
        const hide = (text: string) => text.replaceAll(pattern, (found) => (found === homedir ? REDACTED.home : REDACTED.other));
        const shown = (value: Observation['values'][string]) => (Predicate.isString(value) ? hide(value) : value);
        const container = hide(read.container);
        const scope: Observation['scope'] = read.scope.map(Condition.match({ Value: (atom) => ({ ...atom, at: hide(atom.at), value: shown(atom.value) }), Documents: (atom) => atom, Modal: (atom) => ({ ...atom, at: atom.at === null ? null : hide(atom.at) }) }));
        const cause = Option.map(read.cause, Struct.evolve({ container: hide, control: hide }));
        const shapes = pipe(
            read.controls,
            Record.filter(({ kind }) => kind !== 'row'),
            Record.mapEntries((shape, locator) => [
                hide(locator),
                {
                    ...Struct.evolve(shape, { label: Option.map(Struct.evolve({ text: hide })), help: Option.map(hide), options: Option.map(Array.map(hide)), opens: Option.map(hide), alternate: Option.map(hide), parent: Option.map(hide) }),
                    frame: Record.get(read.layout, locator),
                    value: Option.map(Record.get(read.values, locator), shown),
                },
            ]),
        );
        const application = Option.contains(Array.head(this.application.builds), build) ? this.application : { builds: [build, ...Array.take(this.application.builds, LIMITS.builds - 1)] };
        const entry = Option.fromUndefinedOr(this.containers[container]);
        const fresh = !Option.exists(entry, ({ container: held }) => Equal.equals(held.tally.epoch, opened));
        const sight = (tally: Option.Option<Tally>): Tally =>
            Option.match(tally, {
                onNone: () => ({ seen: 1, missed: 0, first: at, last: at, epoch: opened, since: build, verified: build, persistent: false, unexplained: [] }),
                onSome: (held) => ({ ...held, seen: held.seen + (Equal.equals(held.epoch, opened) ? 0 : 1), last: at, epoch: opened, verified: build }),
            });
        const [stored, openers, scopes] = Option.match(entry, {
            onNone: (): readonly [Entry['controls'], Container['openers'], Container['scopes']] => [{}, [], []],
            onSome: ({ controls, container: held }) => [controls, held.openers, held.scopes] as const,
        });
        const [, pairs] = Array.mapAccum(
            Record.toEntries(shapes).filter(([locator, shape]) => shape.positional && !Record.has(stored, locator)),
            Record.toEntries(stored).filter(([locator, control]) => !Record.has(shapes, locator) && control.positional && Option.isNone(control.retired) && control.tally.verified !== build),
            (left, [locator, shape]) =>
                Option.match(
                    Array.findFirst(left, ([, control]) => Equal.equals(Struct.pick(control, PAIRED), Struct.pick(shape, PAIRED)) && Option.exists(Option.all([control.frame, shape.frame]), ([[[x, y]], [[across, down]]]) => Math.max(Math.abs(x - across), Math.abs(y - down)) <= LIMITS.pairing)),
                    { onNone: () => [left, []], onSome: ([from]) => [left.filter(([held]) => held !== from), [[locator, from] as const]] },
                ),
        );
        const paired = Record.fromEntries(pairs.flat());
        const moved = new Set(Record.values(paired));
        const seen = Record.map(shapes, (shape, locator) => {
            const prior = Record.get(stored, paired[locator] ?? locator);
            const [earlier, current] = [Option.flatMap(prior, Struct.get('options')), Option.exists(prior, ({ tally }) => tally.verified === build)];
            const options = Option.map(shape.options, (listed) => (Option.exists(earlier, (held) => !Equal.equals(held, listed) && (!Array.isArray(held) || current)) ? { count: listed.length } : listed));
            return {
                control: {
                    ...shape,
                    options,
                    opens: Option.orElse(shape.opens, () => Option.flatMap(prior, Struct.get('opens'))),
                    presence: Option.map(Option.flatMap(prior, Struct.get('presence')), (held) => ({ ...held, with: Array.union(held.with, scope) })),
                    tally: { ...sight(Option.map(prior, Struct.get('tally'))), persistent: Option.exists(prior, ({ tally, retired }) => tally.persistent || Option.isSome(retired)) },
                    retired: Option.none(),
                },
                changes: Option.match(prior, {
                    onNone: () => [{ _tag: 'Appeared', at: locator } as const],
                    onSome: (held) => [
                        ...(Option.isSome(held.retired) ? [{ _tag: 'Appeared', at: locator } as const] : []),
                        ...[{ _tag: 'Relabelled', at: locator, from: Option.map(held.label, Struct.get('text')), to: Option.map(shape.label, Struct.get('text')) } as const].filter(({ from, to }) => !Equal.equals(from, to)),
                        ...(Equal.equals(Struct.pick(held, STATIC), Struct.pick(shape, STATIC)) && (Equal.equals(held.options, options) || !Option.exists(options, Array.isArray)) ? [] : [{ _tag: 'Changed', at: locator } as const]),
                    ],
                }),
            };
        });
        const absent = Record.filter(stored, (control, locator) => !Record.has(shapes, locator) && !moved.has(locator) && !Option.exists(control.retired, (retiring) => !Array.take(application.builds, 2).includes(retiring)));
        const missed = Record.map(absent, (control, locator) => {
            const seeded = Option.getOrElse(control.presence, () => ({ with: scopes.filter((held) => Equal.equals(held.at, control.tally.last)).flatMap(Struct.get('atoms')), against: [] }));
            const explained = scope.some((atom) => seeded.with.some((held) => Equal.equals(subject(held), subject(atom))) && !seeded.with.some((held) => Equal.equals(held, atom)));
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
        const controls = Record.map(folded, Struct.get('control'));
        const pending = HashSet.fromIterable(
            Record.values(controls)
                .filter((control) => Option.isNone(control.presence) && Option.isNone(control.retired))
                .map((control) => control.tally.last),
        );
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
                scopes: [{ at, atoms: scope }, ...scopes].filter((reading) => HashSet.has(pending, reading.at)),
            },
            controls,
        });
        const linked = Option.gen(function* () {
            const by = yield* Option.filter(cause, ({ container: from }) => fresh && from !== container);
            const from = yield* Record.get(next, by.container);
            const opener = yield* Option.filter(Record.get(from.controls, by.control), ({ opens }) => !Option.contains(opens, container));
            return Record.set(next, by.container, { ...from, controls: Record.set(from.controls, by.control, { ...opener, opens: Option.some(container) }) });
        });
        return {
            map: new InterfaceMap({ application, containers: Option.getOrElse(linked, () => next) }),
            changes: [...(Option.isNone(entry) ? [{ _tag: 'Appeared', at: container } as const] : []), ...Record.values(folded).flatMap(({ changes }): readonly (typeof Change.Type)[] => changes)],
        };
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const settled = Function.memoize(({ container, controls }: Entry) => ({
    container: { ...container, tally: { ...container.tally, last: container.tally.epoch }, scopes: container.scopes.map(Struct.get('atoms')) },
    controls: Record.map(controls, (control) => ({ ...control, tally: { ...control.tally, last: control.tally.epoch } })),
}));

// --- [SERVICES] ------------------------------------------------------------------------

class InterfaceMaps extends Context.Service<InterfaceMaps, { readonly recall: (snapshot: InstanceSnapshot) => Effect.Effect<InterfaceMap, Effect.Error<ReturnType<typeof InterfaceMap.open>>> }>()('@rasm/applications/interface-map/InterfaceMaps') {
    static readonly layer = Layer.effect(
        InterfaceMaps,
        Effect.gen(function* () {
            const [server, { running }, scope] = yield* Effect.all([ApplicationServer, Workspace, Effect.scope]);
            const exited = (instance: InstanceSnapshot['instance']) =>
                SubscriptionRef.changes(running).pipe(
                    Stream.dropWhile((rows) => !rows.some((row) => row.instance === instance)),
                    Stream.filter((rows) => rows.every((row) => row.instance !== instance || InstanceState.isAnyOf(['exited'])(row.state))),
                    Stream.runHead,
                );
            const maps = yield* RcMap.make({ lookup: ({ bundle, language }: Key) => InterfaceMap.open(bundle, language) });
            const driven: RcMap.RcMap<Driven, Opened, Effect.Error<ReturnType<typeof InterfaceMap.open>>> = yield* RcMap.make({
                lookup: ({ instance, ...key }: Driven) => Effect.tap(RcMap.get(maps, key), () => Effect.forkIn(Effect.andThen(exited(instance), RcMap.invalidate(driven, { instance, ...key })), scope)),
                idleTimeToLive: Duration.infinity,
            });
            const using = <A, E>(held: Driven, use: (opened: Opened) => Effect.Effect<A, E>) =>
                Effect.scoped(
                    Effect.flatMap(
                        Effect.tapError(RcMap.get(driven, held), () => RcMap.invalidate(driven, held)),
                        use,
                    ),
                );
            yield* server.observations({}).pipe(
                Stream.runForEach(({ instance, identifier, language, observation }) =>
                    UndefinedOr.match(identifier, {
                        onUndefined: () => Effect.void,
                        onDefined: (bundle) => using({ instance, bundle, language }, ({ observe }) => observe(observation)).pipe(Effect.catch((error) => Effect.annotateLogs(Effect.logWarning('Interface map fold failed', error), { instance, bundle, language }))),
                    }),
                ),
                Effect.tapCause((cause) => Effect.logError('Interface map observations ended', cause)),
                Effect.forkScoped,
            );
            return {
                recall: ({ instance, identifier, language }) => UndefinedOr.match(language, { onUndefined: () => Effect.succeed(InterfaceMap.empty), onDefined: (held) => using({ instance, bundle: identifier, language: held }, Struct.get('present')) }),
            };
        }),
    ).pipe(
        Layer.provide(
            Layer.unwrap(
                Effect.gen(function* () {
                    const [{ state }, files] = yield* Effect.all([Output, FileSystem.FileSystem]);
                    yield* files.makeDirectory(state, { recursive: true });
                    return Catalog.layer(
                        InterfaceMap.catalog,
                        Sqlite.layer((names) => sqlite({ filename: join(state, 'map.sqlite'), ...names })),
                    );
                }),
            ),
        ),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Entry, InterfaceMap, InterfaceMaps };
