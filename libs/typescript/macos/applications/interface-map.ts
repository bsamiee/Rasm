import { type UserInfo, userInfo } from 'node:os';
import { basename, extname, join } from 'node:path';
import { layer as sqlite } from '@effect/sql-sqlite-node/SqliteClient';
import { Catalog } from '@rasm/database/catalog';
import { Relation } from '@rasm/database/relation';
import { Sqlite } from '@rasm/database/sqlite';
import { Array, Context, Data, DateTime, Duration, Effect, Equal, Equivalence, FileSystem, Function, HashMap, HashSet, Layer, Option, Order, Predicate, pipe, RcMap, Record, Ref, Result, Schedule, Schema, SchemaGetter, SchemaIssue, Sink, Stream, String, Struct, SubscriptionRef } from 'effect';
import { SqlClient } from 'effect/sql';
import { ApplicationServer } from '#/client';
import { Act, Build, Condition, Cursor, Frame, type InstanceSnapshot, InstanceState, Instant, Locator, type Model, Observation, Opener, Shape, Value } from '#/contract';
import { Description, FacetControl } from '#/source';
import { Workspace } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

type Driven = Key & Pick<InstanceSnapshot, 'instance'>;
type Contact = Pick<InstanceSnapshot, 'instance' | 'identifier'> & { readonly language: string };
type Opened = Effect.Success<ReturnType<typeof InterfaceMap.open>>;
interface Proof {
    readonly at: Opener;
    readonly from: typeof Act.Rung.Type;
    readonly outcome: (typeof Act.Result.Type)['outcomes'][number];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const LIMITS = { builds: 8, openers: 4, misses: 3, epochs: 2, pairing: 8, interval: '2 seconds' } as const;
const REDACTED = { home: '~', other: '<private>' } as const;
const STATIC = ['kind', 'role', 'subrole', 'labels', 'section', 'help', 'range', 'shortcut', 'keyEquivalent', 'mark', 'separated', 'alternate', 'parent', 'positional'] as const;
const PAIRED = ['parent', 'role', 'kind', 'help'] as const;
const ELLIPSIS = /(?:…|\.{3})$/u;

// --- [MODELS] --------------------------------------------------------------------------

class Tally extends Schema.Opaque<Tally>()(
    Schema.Struct({
        seen: Schema.Natural.annotate({ description: 'Epochs it was seen in' }),
        missed: Schema.Natural.annotate({ description: 'Complete reads that lacked it' }),
        first: Instant,
        last: Instant,
        epoch: Instant.annotate({ description: 'Opening of the last epoch it was seen in' }),
        since: Schema.OptionFromOptionalKey(Build.annotate({ description: 'Build it was first seen in, absent where the bundle states no CFBundleVersion' })),
        verified: Schema.OptionFromOptionalKey(Build.annotate({ description: 'Build it was last seen in, absent where the bundle states no CFBundleVersion' })),
        persistent: Schema.Boolean.annotate({ description: 'Seen again after retiring' }),
        unexplained: Schema.Array(Instant).annotate({ description: 'Epochs of the last three misses no atom explains' }),
    }),
) {}
const Presence = Schema.Struct({ with: Schema.Array(Condition), against: Schema.Array(Condition) });
class Application extends Schema.Opaque<Application>()(
    Schema.Struct({
        builds: Schema.Array(Build).annotate({ description: 'Builds read, newest first' }),
        options: Schema.Record(Build, Schema.Array(Schema.Tuple([Description.Locator, FacetControl.fields.options.schema.value.mapFields(Struct.omit(['locator']))])))
            .annotate({ description: 'Label, shortcut, and help a host source stated for each facet option while the option was selected, per build, served over what the source lists for the option' })
            .pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
        cursors: Schema.Record(Build, Schema.Record(Cursor.cases.custom.fields.digest, Schema.Record(Schema.String, Schema.Natural)))
            .annotate({ description: 'Tools each custom cursor showed with while the application was active under the real pointer, keyed by its digest, with the reads that saw the pair, per build' })
            .pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    }),
) {}
class Container extends Schema.Opaque<Container>()(
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
    route: Schema.OptionFromOptionalKey(Schema.Struct({ build: Schema.OptionFromOptionalKey(Build), passed: Schema.OptionFromOptionalKey(Act.Rung), failed: Schema.Array(Act.Rung) }).annotate({ description: 'Rung whose read-back last passed a step on the control in the build, and the rungs that failed it there' })),
});
const Positions = Schema.Array(Schema.Natural);
class Entry extends Schema.Opaque<Entry>()(Schema.Struct({ container: Container, controls: Schema.Record(Locator, Control) }).annotate({ description: 'One container and every control read in it, keyed by locator, each condition written inline' })) {}
const Interned = Schema.Struct({
    conditions: Schema.Array(Condition).annotate({ description: 'Each condition the reads and controls of the container hold, once, referred to by its position' }),
    container: Container.mapFields(Struct.assign({ scopes: Schema.Array(Container.fields.scopes.value.mapFields(Struct.assign({ atoms: Positions }))) })),
    controls: Schema.Record(Locator, Control.mapFields(Struct.assign({ presence: Schema.OptionFromOptionalKey(Presence.mapFields(Struct.assign({ with: Positions, against: Positions }))) }))),
})
    .pipe(
        Schema.decodeTo(Schema.toType(Entry), {
            decode: SchemaGetter.transformEffect((interned, options) => {
                const { conditions, container, controls } = interned;
                const atoms = (positions: readonly number[]) => Effect.forEach(positions, (at) => Effect.fromOption(Array.get(conditions, at)));
                return Effect.all({
                    container: Effect.map(
                        Effect.forEach(container.scopes, (scope) => Effect.map(atoms(scope.atoms), (held) => ({ ...scope, atoms: held }))),
                        (scopes) => ({ ...container, scopes }),
                    ),
                    controls: Effect.all(
                        Record.map(controls, (control) =>
                            control.presence.pipe(
                                Option.map(({ with: seen, against }) => Effect.all({ with: atoms(seen), against: atoms(against) })),
                                Effect.transposeOption,
                                Effect.map((presence) => ({ ...control, presence })),
                            ),
                        ),
                    ),
                }).pipe(Effect.mapError(() => new SchemaIssue.InvalidValue({ message: `Expected condition positions below ${conditions.length}` }, interned, options)));
            }),
            encode: SchemaGetter.transform(({ container, controls }) => {
                const conditions = Array.dedupe([...container.scopes.flatMap(Struct.get('atoms')), ...Record.values(controls).flatMap(({ presence }) => Option.match(presence, { onNone: Array.empty, onSome: ({ with: seen, against }) => [...seen, ...against] }))]);
                const positions = HashMap.fromIterable(conditions.map((atom, at) => [atom, at] as const));
                const place = Array.flatMap((atom: typeof Condition.Type) => Option.toArray(HashMap.get(positions, atom)));
                return {
                    conditions,
                    container: { ...container, scopes: container.scopes.map((scope) => ({ ...scope, atoms: place(scope.atoms) })) },
                    controls: Record.map(controls, (control) => ({ ...control, presence: Option.map(control.presence, Struct.evolve({ with: place, against: place })) })),
                };
            }),
        }),
    )
    .annotate({ description: 'One container and every control read in it, keyed by locator, each condition written once and referred to by position' });
const Change = Schema.TaggedUnion({
    Appeared: { at: Locator },
    Changed: { at: Locator },
    Relabelled: { at: Locator, from: Schema.OptionFromOptionalKey(Schema.String), to: Schema.OptionFromOptionalKey(Schema.String) },
    Gone: { at: Locator },
});
class Key extends Schema.Opaque<Key>()(Schema.Struct({ bundle: Schema.String, language: Schema.String })) {}
const applications = Relation.make('applications', { ...Key.fields, body: Schema.fromJsonString(Application) }, { key: ['bundle', 'language'], documents: { body: [] } });
const containers = Relation.make('containers', { ...Key.fields, locator: Locator, body: Schema.fromJsonString(Interned) }, { key: ['bundle', 'language', 'locator'], documents: { body: [Schema.fromJsonString(Entry)] } });

class InterfaceMap extends Data.Class<{ readonly application: Application; readonly containers: Readonly<Record<string, Entry>> }> {
    static readonly Change = Change;
    static readonly empty = new InterfaceMap({ application: { builds: [], options: {}, cursors: {} }, containers: {} });
    static readonly catalog = Catalog.make({ tables: [applications, containers] });
    static readonly languages = (bundle: string) => Effect.map(Relation.select(applications, { bundle: [bundle] }), Array.map(Struct.get('language')));
    static readonly read = Effect.fn('InterfaceMap.read')(function* (bundle: string, language: string) {
        const where = { bundle: [bundle], language: [language] } as const;
        const [held, entries] = yield* Effect.annotateLogs(Effect.all([Relation.select(applications, where), Relation.select(containers, where)]), { bundle, language });
        return new InterfaceMap({
            application: pipe(
                Array.head(held),
                Option.map(Struct.get('body')),
                Option.getOrElse(() => InterfaceMap.empty.application),
            ),
            containers: Record.fromIterableWith(entries, ({ locator, body }) => [locator, body]),
        });
    });
    static readonly open = Effect.fn('InterfaceMap.open')(function* (bundle: string, language: string) {
        const [loaded, user, sql] = yield* Effect.all([InterfaceMap.read(bundle, language), Effect.sync(() => userInfo()), SqlClient.SqlClient]);
        const [state, written] = yield* Effect.all([SubscriptionRef.make(loaded), Ref.make(loaded)]);
        const flush = Effect.gen(function* () {
            const [current, stored] = yield* Effect.all([SubscriptionRef.get(state), Ref.get(written)]);
            const writes = [
                ...(current.application === stored.application ? [] : [Relation.upsert(applications, [{ bundle, language, body: current.application }])]),
                ...Array.match(
                    Record.collect(
                        Record.filter(current.containers, (entry, locator) => entry !== stored.containers[locator]),
                        (locator, body) => ({ bundle, language, locator, body }),
                    ),
                    { onEmpty: Array.empty, onNonEmpty: (rows) => [Relation.upsert(containers, rows)] },
                ),
            ];
            yield* Array.match(writes, { onEmpty: () => Effect.void, onNonEmpty: (statements) => Effect.andThen(sql.withTransaction(Effect.all(statements)), Ref.set(written, current)) });
        }).pipe(Effect.catch((error) => Effect.annotateLogs(Effect.logWarning('Interface map flush failed', error), { bundle, language })));
        yield* Effect.addFinalizer(() => flush);
        yield* SubscriptionRef.changes(state).pipe(
            Stream.changesWith(Equivalence.strictEqual()),
            Stream.dropWhile((map) => map === loaded),
            Stream.aggregateWithin(Sink.drain, Schedule.spaced(LIMITS.interval)),
            Stream.runForEach(() => flush),
            Effect.forkScoped,
        );
        return {
            present: SubscriptionRef.get(state),
            observe: Effect.fn('InterfaceMap.observe')(function* (read: Observation) {
                const changes = yield* SubscriptionRef.modify(state, (map) => map.fold(read, user));
                yield* Array.match(changes, {
                    onEmpty: () => Effect.void,
                    onNonEmpty: (each) =>
                        Effect.annotateLogs(Effect.logDebug('Interface map changed'), {
                            bundle,
                            language,
                            container: read.container,
                            kind: read.kind,
                            new: each.some(({ at }) => at === read.container),
                            ...Record.map(
                                Array.groupBy(each, ({ _tag }): string => _tag),
                                Array.length,
                            ),
                        }),
                });
            }),
            learn: (described: Description) => SubscriptionRef.modify(state, (map) => map.learn(described)),
            prove: (proofs: readonly Proof[]) => SubscriptionRef.update(state, (map) => proofs.reduce((proven, proof) => proven.prove(proof), map)),
            sight: (pointer: Model['pointer']) => SubscriptionRef.update(state, (map) => map.sight(pointer)),
        };
    });

    get known() {
        return known(this);
    }

    get coverage() {
        return coverage(this);
    }

    level(opener: string) {
        return pipe(
            Record.get(openedBy(this), opener),
            Option.flatMap((locator) => Option.map(Record.get(this.containers, locator), (entry) => ({ locator, entry }))),
            Option.filter(({ entry }) => Equal.equals(Array.head(this.application.builds), entry.container.tally.verified)),
        );
    }

    route({ container, control }: Opener) {
        return pipe(
            Record.get(this.containers, container),
            Option.flatMap(({ controls }) => Record.get(controls, control)),
            Option.flatMap(({ route }) => Option.filter(route, ({ build }) => Equal.equals(Array.head(this.application.builds), build))),
            Option.flatMap(Struct.get('passed')),
        );
    }

    cursor(digest: string) {
        return Option.flatMap(Array.head(this.application.builds), (build) => Option.flatMap(Record.get(this.application.cursors, build), Record.get(digest)));
    }

    sight(pointer: Model['pointer']): InterfaceMap {
        const seen = Option.all({
            build: Array.head(this.application.builds),
            tool: Option.fromUndefinedOr(pointer?.tool),
            digest: Option.flatMap(Option.fromUndefinedOr(pointer?.cursor), Cursor.match({ system: () => Option.none<string>(), custom: ({ digest }) => Option.some(digest) })),
        });
        return Option.match(seen, {
            onNone: () => this,
            onSome: ({ build, tool, digest }) => {
                const { builds, cursors } = this.application;
                const held = Option.getOrElse(Record.get(cursors, build), Record.empty<string, Readonly<Record<string, number>>>);
                const tools = Option.getOrElse(Record.get(held, digest), Record.empty<string, number>);
                const counted = { ...held, [digest]: { ...tools, [tool]: Option.getOrElse(Record.get(tools, tool), () => 0) + 1 } };
                return new InterfaceMap({ containers: this.containers, application: { ...this.application, cursors: { ...Record.filter(cursors, (_, at) => builds.includes(at)), [build]: counted } } });
            },
        });
    }

    prove({ at, from, outcome }: Proof): InterfaceMap {
        const build = Array.head(this.application.builds);
        const found = Record.get(this.containers, at.container).pipe(
            Option.filter(() => Result.match(outcome, { onSuccess: ({ proof }) => proof !== 'unanswered' && proof !== 'exit', onFailure: Function.constTrue })),
            Option.flatMap((entry) => Option.map(Record.get(entry.controls, at.control), (control) => ({ entry, control }))),
        );
        return Option.match(found, {
            onNone: () => this,
            onSome: ({ entry, control }) => {
                const prior = Option.filter(control.route, (held) => Equal.equals(held.build, build));
                const { passed, failed } = Result.match(outcome, {
                    onSuccess: ({ route, tried }) => ({ passed: Option.some<typeof Act.Rung.Type>(route?._tag ?? 'accessibility'), failed: tried.map(Struct.get('rung')) }),
                    onFailure: () => ({ passed: Option.none<typeof Act.Rung.Type>(), failed: Array.drop(Act.Rung.literals, Act.Rung.literals.indexOf(from)) }),
                });
                const route = { build, passed, failed: Array.difference(Array.union(Option.match(prior, { onNone: Array.empty, onSome: Struct.get('failed') }), failed), Option.toArray(passed)) };
                return new InterfaceMap({ application: this.application, containers: Record.set(this.containers, at.container, { ...entry, controls: Record.set(entry.controls, at.control, { ...control, route: Option.some(route) }) }) });
            },
        });
    }

    learn(described: Description): readonly [Description, InterfaceMap] {
        const { builds, options: stored } = this.application;
        const seen = Array.flatMap(
            Array.flatMap(described.containers, ({ controls }) => controls),
            ({ value, options = [] }: FacetControl) => {
                const chosen = value?._tag === 'text' ? Option.some(value.value) : Option.none<string>();
                return options.filter(({ label }) => Option.contains(chosen, label)).map(({ locator, ...stated }) => [locator, stated] as const);
            },
        );
        const held = HashMap.fromIterable(
            Option.getOrElse(
                Option.flatMap(Array.head(builds), (build) => Record.get(stored, build)),
                Array.empty,
            ),
        );
        const learned = HashMap.union(held, HashMap.fromIterable(seen));
        return [
            Description.make({
                ...described,
                containers: Array.map(described.containers, ({ controls, ...container }) => ({
                    ...container,
                    controls: Array.map(controls, ({ options, ...control }) => ({ ...control, ...(options === undefined ? {} : { options: Array.map(options, (option) => ({ ...option, ...Option.getOrUndefined(HashMap.get(learned, option.locator)) })) }) })),
                })),
            }),
            Option.match(
                Option.filter(Array.head(builds), () => Array.some(seen, ([locator, stated]) => !Option.contains(HashMap.get(held, locator), stated))),
                {
                    onNone: () => this,
                    onSome: (build) => new InterfaceMap({ containers: this.containers, application: { ...this.application, options: { ...Record.filter(stored, (_, at) => builds.includes(at)), [build]: Array.fromIterable(learned) } } }),
                },
            ),
        ];
    }

    fold(read: Observation, { username, homedir }: Pick<UserInfo<string>, 'username' | 'homedir'>): readonly [readonly (typeof Change.Type)[], InterfaceMap] {
        const { build, container, opened, at, truncated } = read;
        const texts = Array.sort(Array.dedupe([...read.documents.flatMap((path) => [path, basename(path), basename(path, extname(path))]), homedir, username].filter(String.isNonEmpty)), Order.mapInput(Order.flip(Order.Number), String.length));
        const pattern = new RegExp(`(?<![\\p{L}\\p{M}\\p{N}])(?:${texts.map(RegExp.escape).join('|')})(?![\\p{L}\\p{M}\\p{N}])`, 'gu');
        const hide = (text: string) => text.replaceAll(pattern, (found) => (found === homedir ? REDACTED.home : REDACTED.other));
        const shown = (value: Observation['values'][string]) => (Predicate.isString(value) ? hide(value) : value);
        const scope: Observation['conditions'] = read.conditions.map(Condition.matchOrElse({ Value: (atom) => ({ ...atom, value: shown(atom.value) }) }, Function.identity));
        const shapes = Record.map(read.controls, (shape, locator) => ({
            ...Struct.evolve(shape, { label: Option.map(Struct.evolve({ text: hide })), help: Option.map(hide), options: Option.map(Array.map(hide)) }),
            frame: Record.get(read.layout, locator),
            value: Option.map(Record.get(read.values, locator), shown),
        }));
        const application = Option.match(build, {
            onNone: () => this.application,
            onSome: (current) => (Option.contains(Array.head(this.application.builds), current) ? this.application : { ...this.application, builds: Array.take(Array.dedupe([current, ...this.application.builds]), LIMITS.builds) }),
        });
        const entry = Record.get(this.containers, container);
        const {
            controls: stored,
            container: { openers, scopes },
        } = Option.getOrElse(entry, (): Pick<Entry, 'controls'> & { readonly container: Pick<Container, 'openers' | 'scopes'> } => ({ controls: {}, container: { openers: [], scopes: [] } }));
        const cause = Option.filter(read.opener, () => !Option.exists(entry, ({ container: held }) => !DateTime.isGreaterThan(opened, held.tally.epoch)));
        const sight = (tally: Option.Option<Tally>): Tally =>
            Option.match(tally, {
                onNone: () => ({ seen: 1, missed: 0, first: at, last: at, epoch: opened, since: build, verified: build, persistent: false, unexplained: [] }),
                onSome: (held) => ({ ...held, seen: held.seen + (DateTime.isGreaterThan(opened, held.epoch) ? 1 : 0), last: at, epoch: DateTime.max(held.epoch, opened), verified: build }),
            });
        const [, pairs] = Array.mapAccum(
            Record.toEntries(shapes).filter(([locator, shape]) => shape.positional && !Record.has(stored, locator)),
            Record.toEntries(stored).filter(([locator, control]) => !Record.has(shapes, locator) && control.positional && Option.isNone(control.retired) && !Equal.equals(control.tally.verified, build)),
            (left, [locator, shape]) =>
                Option.match(
                    Array.findFirst(left, ([, control]) => Equal.equals(Struct.pick(control, PAIRED), Struct.pick(shape, PAIRED)) && Option.exists(Option.all([control.frame, shape.frame]), ([[[x, y]], [[across, down]]]) => Math.max(Math.abs(x - across), Math.abs(y - down)) <= LIMITS.pairing)),
                    { onNone: () => [left, Option.none()], onSome: ([from]) => [left.filter(([held]) => held !== from), Option.some([locator, from] as const)] },
                ),
        );
        const paired = Record.fromEntries(Array.getSomes(pairs));
        const moved = new Set(Record.values(paired));
        const seen = Record.map(shapes, (shape, locator) => {
            const prior = Record.get(stored, paired[locator] ?? locator);
            const verified = Option.exists(prior, ({ tally }) => Equal.equals(tally.verified, build));
            const options = Option.map(shape.options, (listed) => (Option.exists(Option.flatMap(prior, Struct.get('options')), (held) => !Equal.equals(held, listed) && (!Array.isArray(held) || verified)) ? { count: listed.length } : listed));
            return {
                control: {
                    ...shape,
                    options,
                    opens: Option.orElse(shape.opens, () => Option.flatMap(prior, Struct.get('opens'))),
                    presence: Option.map(Option.flatMap(prior, Struct.get('presence')), (held) => ({ ...held, with: Array.union(held.with, scope) })),
                    tally: { ...sight(Option.map(prior, Struct.get('tally'))), persistent: Option.exists(prior, ({ tally, retired }) => tally.persistent || Option.isSome(retired)) },
                    retired: Option.none(),
                    route: Option.flatMap(prior, Struct.get('route')),
                },
                changes: Option.match(prior, {
                    onNone: () => [{ _tag: 'Appeared', at: locator } as const],
                    onSome: (held) => [
                        ...(Option.isSome(held.retired) ? [{ _tag: 'Appeared', at: locator } as const] : []),
                        ...Option.toArray(Option.liftPredicate({ _tag: 'Relabelled', at: locator, from: Option.map(held.label, Struct.get('text')), to: Option.map(shape.label, Struct.get('text')) } as const, ({ from, to }) => !Equal.equals(from, to))),
                        ...(Equal.equals(Struct.pick(held, STATIC), Struct.pick(shape, STATIC)) && (Equal.equals(held.options, options) || !Option.exists(options, Array.isArray)) ? [] : [{ _tag: 'Changed', at: locator } as const]),
                    ],
                }),
            };
        });
        const absent = Record.filter(stored, (control, locator) => !Record.has(shapes, locator) && !moved.has(locator) && !Option.exists(control.retired, (retiring) => !Array.take(application.builds, 2).includes(retiring)));
        const missed = Record.map(absent, (control, locator) => {
            const seeded = Option.getOrElse(control.presence, () => ({ with: scopes.filter((held) => Equal.equals(held.at, control.tally.last)).flatMap(Struct.get('atoms')), against: [] }));
            const explained = Array.isReadonlyArrayNonEmpty(Array.intersection(Array.difference(scope, seeded.with).map(subject), seeded.with.map(subject)));
            const unexplained = explained ? control.tally.unexplained : Array.take(Array.prepend(control.tally.unexplained, opened), LIMITS.misses);
            const retiring = !explained && (!Equal.equals(control.tally.verified, build) || (!control.tally.persistent && unexplained.length >= LIMITS.misses && Array.dedupe(unexplained).length >= LIMITS.epochs));
            return Option.isSome(control.retired) || truncated
                ? { control, changes: [] }
                : {
                      control: { ...control, presence: Option.some(explained ? { ...seeded, against: Array.union(seeded.against, scope) } : seeded), tally: { ...control.tally, missed: control.tally.missed + 1, unexplained }, retired: Option.filter(build, () => retiring) },
                      changes: retiring ? [{ _tag: 'Gone', at: locator } as const] : [],
                  };
        });
        const folded = { ...seen, ...missed };
        const controls = Record.filterMap(folded, ({ control, changes }) => (Option.isNone(build) && changes.some(Change.guards.Gone) ? Result.failVoid : Result.succeed(control)));
        const pending = HashSet.fromIterable(
            Record.values(controls)
                .filter((control) => Option.isNone(control.presence) && Option.isNone(control.retired))
                .map((control) => control.tally.last),
        );
        const kept: Entry = {
            container: {
                ...Struct.pick(read, ['kind', 'modal', 'layer', 'size', 'default', 'cancel', 'close']),
                title: Option.map(read.title, hide),
                openers: Option.match(cause, {
                    onNone: () => openers,
                    onSome: (by) => {
                        const same = (opener: (typeof openers)[number]) => Equal.equals(Struct.pick(opener, ['container', 'control']), by);
                        return Array.take([{ ...by, tally: sight(Option.map(Array.findFirst(openers, same), Struct.get('tally'))) }, ...openers.filter(Predicate.not(same))], LIMITS.openers);
                    },
                }),
                tally: sight(Option.map(entry, ({ container: held }) => held.tally)),
                scopes: [{ at, atoms: scope }, ...scopes].filter((reading) => HashSet.has(pending, reading.at)),
            },
            controls,
        };
        const next = Record.set(this.containers, container, kept);
        const linked = Option.gen(function* () {
            const by = yield* Option.filter(cause, ({ container: from }) => from !== container);
            const from = yield* Record.get(next, by.container);
            const opener = yield* Option.filter(Record.get(from.controls, by.control), ({ opens }) => !Option.contains(opens, container));
            return Record.set(next, by.container, { ...from, controls: Record.set(from.controls, by.control, { ...opener, opens: Option.some(container) }) });
        });
        return [[...(Option.isNone(entry) ? [{ _tag: 'Appeared', at: container } as const] : []), ...Record.values(folded).flatMap(({ changes }): readonly (typeof Change.Type)[] => changes)], new InterfaceMap({ application, containers: Option.getOrElse(linked, () => next) })];
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const subject = Condition.matchOrElse({ Value: ({ _tag, at }) => ({ _tag, at }) }, ({ _tag }) => ({ _tag }));
const openedBy = Function.memoize((map: InterfaceMap) => Record.fromEntries(Record.toEntries(map.containers).flatMap(([locator, { container }]) => (container.kind === 'menu' ? container.openers.map(({ control }) => [control, locator] as const) : []))));
const known = Function.memoize((map: InterfaceMap) =>
    Option.map(
        Option.liftPredicate(map.containers, (held) => !Record.isEmptyRecord(held)),
        (held) => {
            const build = Array.head(map.application.builds);
            const current = Record.filter(held, ({ container }) => Equal.equals(container.tally.verified, build));
            const labels = Iterator.from(Record.values(current))
                .filter(({ container }) => container.kind === 'menu')
                .flatMap(({ controls }) => Record.values(controls))
                .filter(({ retired }) => Option.isNone(retired))
                .flatMap(({ label }) => Option.toArray(label))
                .filter(({ attribute }) => attribute !== 'recognized')
                .map(({ text }) => text);
            return { build, containers: Record.keys(current), labels: new Set(labels).values().toArray() };
        },
    ),
);
const coverage = Function.memoize((map: InterfaceMap) => {
    const build = Array.head(map.application.builds);
    const [current, earlier] = Array.partition(Record.toEntries(map.containers), (entry) => (Equal.equals(build, entry[1].container.tally.verified) ? Result.succeed(entry) : Result.fail(entry)));
    const controls = current.flatMap(([container, entry]) =>
        Record.collect(
            Record.filter(entry.controls, ({ retired }) => Option.isNone(retired)),
            (control, held) => ({ at: { container, control }, held }),
        ),
    );
    const gaps = {
        opens: ({ opens }: Entry['controls'][string]) => Option.exists(opens, (opened) => !Record.has(map.containers, opened)),
        ellipsis: ({ opens, label }: Entry['controls'][string]) => Option.isNone(opens) && Option.exists(label, ({ text }) => ELLIPSIS.test(text)),
        options: ({ kind, options }: Entry['controls'][string]) => kind === 'choice' && Option.isNone(options),
    };
    return {
        build,
        known: Record.map(
            Array.groupBy(current, ([, { container }]): string => container.kind),
            Array.length,
        ),
        earlier: earlier.length,
        unopened: Record.map(gaps, (lacks) => controls.filter(({ held }) => lacks(held)).map(({ at }): Opener => at)),
        openers: controls.filter(({ held }) => Record.some(gaps, (lacks) => lacks(held))).map(({ at }): Opener => at),
    };
});

// --- [SERVICES] ------------------------------------------------------------------------

class InterfaceMaps extends Context.Service<
    InterfaceMaps,
    {
        readonly recall: (contact: Contact) => Effect.Effect<InterfaceMap, Effect.Error<ReturnType<typeof InterfaceMap.open>>>;
        readonly learn: (contact: Contact, described: Description) => Effect.Effect<Description, Effect.Error<ReturnType<typeof InterfaceMap.open>>>;
        readonly held: (bundle: string) => Effect.Effect<Readonly<Record<string, InterfaceMap>>, Effect.Error<ReturnType<typeof InterfaceMap.open>>>;
        readonly prove: (contact: Contact, proofs: readonly Proof[]) => Effect.Effect<void, Effect.Error<ReturnType<typeof InterfaceMap.open>>>;
        readonly sight: (contact: Contact, pointer: Model['pointer']) => Effect.Effect<void, Effect.Error<ReturnType<typeof InterfaceMap.open>>>;
    }
>()('@rasm/applications/interface-map/InterfaceMaps') {
    static readonly layer = (folder: string) =>
        Layer.effect(
            InterfaceMaps,
            Effect.gen(function* () {
                const [server, { running }, scope, stored] = yield* Effect.all([ApplicationServer, Workspace, Effect.scope, Effect.context<Effect.Services<ReturnType<typeof InterfaceMap.languages>>>()]);
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
                const using = <A, E>({ instance, identifier, language }: Contact, use: (opened: Opened) => Effect.Effect<A, E>) =>
                    Effect.scoped(
                        Effect.flatMap(
                            Effect.tapError(RcMap.get(driven, { instance, bundle: identifier, language }), () => RcMap.invalidate(driven, { instance, bundle: identifier, language })),
                            use,
                        ),
                    );
                yield* server.observations({}).pipe(
                    Stream.filterMap(({ identifier, ...report }) => (identifier === undefined ? Result.failVoid : Result.succeed({ ...report, identifier }))),
                    Stream.mapEffect(({ observation, ...contact }) =>
                        Effect.catch(
                            using(contact, ({ observe }) => observe(observation)),
                            (error) => Effect.annotateLogs(Effect.logWarning('Interface map fold failed', error), contact),
                        ),
                    ),
                    Stream.catchCause((cause) => Stream.fromEffectDrain(Effect.logError('Interface map observations ended', cause))),
                    Stream.runDrain,
                    Effect.forkScoped,
                );
                return {
                    recall: (contact) => using(contact, Struct.get('present')),
                    learn: (contact, described) => using(contact, ({ learn }) => learn(described)),
                    held: Effect.fnUntraced(
                        function* (bundle) {
                            const languages = yield* InterfaceMap.languages(bundle);
                            const held = yield* Effect.forEach(languages, (language) => Effect.flatMap(RcMap.getOption(maps, { bundle, language }), Option.match({ onNone: Function.constant(InterfaceMap.read(bundle, language)), onSome: Struct.get('present') })));
                            return Record.fromEntries(Array.zip(languages, held));
                        },
                        Effect.scoped,
                        Effect.provideContext(stored),
                    ),
                    prove: (contact, proofs) => using(contact, ({ prove }) => prove(proofs)),
                    sight: (contact, pointer) => using(contact, ({ sight }) => sight(pointer)),
                };
            }),
        ).pipe(
            Layer.provide(
                Layer.unwrap(
                    Effect.as(
                        Effect.flatMap(FileSystem.FileSystem, (files) => files.makeDirectory(folder, { recursive: true })),
                        Catalog.layer(
                            InterfaceMap.catalog,
                            Sqlite.layer((names) => sqlite({ filename: join(folder, 'map.sqlite'), ...names })),
                        ),
                    ),
                ),
            ),
        );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Entry, InterfaceMap, InterfaceMaps };
