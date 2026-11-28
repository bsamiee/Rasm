import { type UserInfo, userInfo } from 'node:os';
import { basename, extname, join } from 'node:path';
import { Catalog } from '@rasm/database/catalog';
import { Relation } from '@rasm/database/relation';
import { sqliteNodeEngine } from '@rasm/database/sqlite-node';
import { Array, Context, Data, DateTime, Duration, Effect, Equal, Function, HashMap, HashSet, Layer, Option, Order, Predicate, pipe, RcMap, Record, Result, Stream, String, Struct, SubscriptionRef, SynchronizedRef } from 'effect';
import { SqlClient } from 'effect/sql';
import { Description } from '#/description/description';
import { applications, catalog, containers } from '#/map/record';
import { Act } from '#/server/act';
import { ApplicationServer } from '#/server/client';
import { Element } from '#/server/element';
import { Instance } from '#/server/instance';

// --- [TYPES] ---------------------------------------------------------------------------

type Watched = Pick<typeof Instance.Watched.Type, 'instance' | 'identifier' | 'language'>;
type Change = Data.TaggedEnum<{
    Appeared: { readonly at: string };
    Changed: { readonly at: string };
    Relabelled: { readonly at: string; readonly from: Option.Option<string>; readonly to: Option.Option<string> };
    Gone: { readonly at: string };
}>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const LIMITS = { builds: 8, openers: 4, misses: 3, epochs: 2, pairing: 8 } as const;
const STATIC = Struct.keys(Struct.omit(Element.Shape.fields, ['label', 'options', 'opens']));

// --- [MODELS] --------------------------------------------------------------------------

const Change = Data.taggedEnum<Change>();
class InterfaceMap extends Data.Class<{ readonly application: (typeof applications.schema.Type)['body']; readonly containers: Readonly<Record<string, (typeof containers.schema.Type)['body']>> }> {
    // --- [LOOKUPS]

    static readonly #known = Function.memoize(({ application, containers: held }: InterfaceMap) => {
        const build = Array.head(application.builds);
        const current = Record.filter(held, ({ container }) => Equal.equals(container.tally.verified, build));
        const labels = Record.values(current)
            .filter(({ container }) => container.kind === 'menu')
            .flatMap(({ controls }) => Record.values(controls))
            .filter(({ retired }) => Option.isNone(retired))
            .flatMap(({ label }) => Option.toArray(label))
            .filter(({ attribute }) => attribute !== 'recognized')
            .map(({ text }) => text);
        return Record.isEmptyRecord(held) ? Option.none() : Option.some({ build, containers: Record.keys(current), labels: Array.dedupe(labels) });
    });
    static readonly #coverage = Function.memoize(({ application, containers: held }: InterfaceMap) => {
        const build = Array.head(application.builds);
        const current = Record.toEntries(held).filter(([, entry]) => Equal.equals(build, entry.container.tally.verified));
        const openers = current
            .flatMap(([container, entry]) => Record.toEntries(entry.controls).map(([control, stored]) => ({ at: { container, control }, stored })))
            .filter(({ stored }) => Option.isNone(stored.retired))
            .filter(({ stored }) => Option.match(stored.opens, { onNone: () => Option.exists(stored.label, ({ text }) => text.endsWith('…') || text.endsWith('...')), onSome: (at) => !Record.has(held, at) }) || (stored.kind === 'choice' && Option.isNone(stored.options)))
            .map(({ at }) => at);
        return {
            build,
            containers: Record.map(
                Array.groupBy(current, ([, entry]): string => entry.container.kind),
                Array.length,
            ),
            earlier: Record.size(held) - current.length,
            openers,
        };
    });
    static readonly #byOpener = Function.memoize(({ containers: held }: InterfaceMap) =>
        Record.fromEntries(
            Record.toEntries(held)
                .filter(([, entry]) => entry.container.kind === 'menu')
                .flatMap(([locator, entry]) => entry.container.openers.map(({ control }) => [control, locator] as const)),
        ),
    );

    get known() {
        return InterfaceMap.#known(this);
    }

    get coverage() {
        return InterfaceMap.#coverage(this);
    }

    level(opener: string) {
        return pipe(
            Record.get(InterfaceMap.#byOpener(this), opener),
            Option.flatMap((locator) => Option.map(Record.get(this.containers, locator), (entry) => ({ locator, entry }))),
            Option.filter(({ entry }) => Equal.equals(Array.head(this.application.builds), entry.container.tally.verified)),
        );
    }

    route({ container, control }: typeof Element.Opener.Type) {
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

    // --- [FOLDS]

    static readonly empty = new InterfaceMap({ application: { builds: [], options: {}, cursors: {} }, containers: {} });

    fold(read: typeof Element.Observation.Type, { username, homedir }: Pick<UserInfo<string>, 'username' | 'homedir'>): readonly [readonly Change[], InterfaceMap] {
        const { build, container, opened } = read;
        const texts = Array.sort(Array.dedupe([...read.documents.flatMap((path) => [path, basename(path), basename(path, extname(path))]), homedir, username].filter(String.isNonEmpty)), Order.mapInput(Order.flip(Order.Number), String.length));
        const pattern = new RegExp(`(?<![\\p{L}\\p{M}\\p{N}])(?:${texts.map(RegExp.escape).join('|')})(?![\\p{L}\\p{M}\\p{N}])`, 'gu');
        const hide = (text: string) => text.replaceAll(pattern, (found) => (found === homedir ? '~' : '<private>'));
        const hideValue = (value: typeof Element.Value.Type) => (Predicate.isString(value) ? hide(value) : value);
        const scope: (typeof Element.Observation.Type)['conditions'] = read.conditions.map(Element.Condition.matchOrElse({ Value: (atom) => ({ ...atom, value: hideValue(atom.value) }) }, Function.identity));
        const shapes = Record.map(read.controls, (shape, locator) => ({
            ...Struct.evolve(shape, { label: Option.map(Struct.evolve({ text: hide })), help: Option.map(hide), options: Option.map(Array.map(hide)) }),
            frame: Record.get(read.layout, locator),
            value: Option.map(Record.get(read.values, locator), hideValue),
        }));
        const builds = Array.take(Array.dedupe([...Option.toArray(build), ...this.application.builds]), LIMITS.builds);
        const application = Equal.equals(builds, this.application.builds) ? this.application : { ...this.application, builds };
        const entry = Record.get(this.containers, container);
        const { controls: stored, container: last } = Option.getOrElse(entry, (): Pick<InterfaceMap['containers'][string], 'controls'> & { readonly container: Pick<InterfaceMap['containers'][string]['container'], 'openers' | 'scopes'> } => ({ controls: {}, container: { openers: [], scopes: [] } }));
        const cause: typeof read.opener = Option.exists(entry, (held) => !DateTime.isGreaterThan(opened, held.container.tally.epoch)) ? Option.none() : read.opener;
        const sight = (tally: Option.Option<InterfaceMap['containers'][string]['container']['tally']>) =>
            Option.match(tally, {
                onNone: () => ({ seen: 1, missed: 0, first: read.at, last: read.at, epoch: opened, since: build, verified: build, persistent: false, unexplained: [] }),
                onSome: (held) => ({ ...held, seen: held.seen + (DateTime.isGreaterThan(opened, held.epoch) ? 1 : 0), last: read.at, epoch: DateTime.max(held.epoch, opened), verified: build }),
            });
        const alike = ['parent', 'role', 'kind', 'help'] as const;
        const [, pairs] = Array.mapAccum(
            Record.toEntries(shapes).filter(([locator, shape]) => shape.positional && !Record.has(stored, locator)),
            Record.toEntries(stored).filter(([locator, control]) => !Record.has(shapes, locator) && control.positional && Option.isNone(control.retired) && !Equal.equals(control.tally.verified, build)),
            (left, [locator, shape]) =>
                Option.match(
                    Array.findFirst(left, ([, control]) => Equal.equals(Struct.pick(control, alike), Struct.pick(shape, alike)) && Option.exists(Option.all([control.frame, shape.frame]), ([from, to]) => Math.max(Math.abs(from[0][0] - to[0][0]), Math.abs(from[0][1] - to[0][1])) <= LIMITS.pairing)),
                    { onNone: () => [left, Option.none()], onSome: ([from]) => [left.filter(([held]) => held !== from), Option.some([locator, from] as const)] },
                ),
        );
        const paired = Record.fromEntries(Array.getSomes(pairs));
        const moved = new Set(Record.values(paired));
        const seen = Record.map(shapes, (shape, locator) => {
            const before = Record.get(stored, paired[locator] ?? locator);
            const verified = Option.exists(before, ({ tally }) => Equal.equals(tally.verified, build));
            const options = Option.map(shape.options, (listed) => (Option.exists(Option.flatMap(before, Struct.get('options')), (held) => !Equal.equals(held, listed) && (!Array.isArray(held) || verified)) ? { count: listed.length } : listed));
            return {
                control: {
                    ...shape,
                    options,
                    opens: Option.orElse(shape.opens, () => Option.flatMap(before, Struct.get('opens'))),
                    presence: Option.map(Option.flatMap(before, Struct.get('presence')), (held) => ({ ...held, with: Array.union(held.with, scope) })),
                    tally: { ...sight(Option.map(before, Struct.get('tally'))), persistent: Option.exists(before, ({ tally, retired }) => tally.persistent || Option.isSome(retired)) },
                    retired: Option.none(),
                    route: Option.flatMap(before, Struct.get('route')),
                },
                changes: Option.match(before, {
                    onNone: () => [Change.Appeared({ at: locator })],
                    onSome: (held) => [
                        ...(Option.isSome(held.retired) ? [Change.Appeared({ at: locator })] : []),
                        ...Option.toArray(Option.liftPredicate(Change.Relabelled({ at: locator, from: Option.map(held.label, Struct.get('text')), to: Option.map(shape.label, Struct.get('text')) }), ({ from, to }) => !Equal.equals(from, to))),
                        ...(Equal.equals(Struct.pick(held, STATIC), Struct.pick(shape, STATIC)) && (Equal.equals(held.options, options) || !Option.exists(options, Array.isArray)) ? [] : [Change.Changed({ at: locator })]),
                    ],
                }),
            };
        });
        const recent = Array.take(application.builds, 2);
        const absent = Record.filter(stored, (control, locator) => !Record.has(shapes, locator) && !moved.has(locator) && !Option.exists(control.retired, (under) => !recent.includes(under)));
        const subject = Element.Condition.matchOrElse({ Value: ({ _tag, at }) => ({ _tag, at }) }, ({ _tag }) => ({ _tag }));
        const missed = Record.map(absent, (control, locator) => {
            const seeded = Option.getOrElse(control.presence, () => ({ with: last.scopes.filter((held) => Equal.equals(held.at, control.tally.last)).flatMap(Struct.get('atoms')), against: [] }));
            const explained = Array.isReadonlyArrayNonEmpty(Array.intersection(Array.difference(scope, seeded.with).map(subject), seeded.with.map(subject)));
            const unexplained = explained ? control.tally.unexplained : Array.take(Array.prepend(control.tally.unexplained, opened), LIMITS.misses);
            const retiring = !explained && (!Equal.equals(control.tally.verified, build) || (!control.tally.persistent && unexplained.length >= LIMITS.misses && Array.dedupe(unexplained).length >= LIMITS.epochs));
            return Option.isSome(control.retired) || read.truncated
                ? { control, changes: [] }
                : {
                      control: { ...control, presence: Option.some(explained ? { ...seeded, against: Array.union(seeded.against, scope) } : seeded), tally: { ...control.tally, missed: control.tally.missed + 1, unexplained }, retired: Option.filter(build, () => retiring) },
                      changes: retiring ? [Change.Gone({ at: locator })] : [],
                  };
        });
        const folded = { ...seen, ...missed };
        const controls = Record.filterMap(folded, ({ control, changes }) => (Option.isNone(build) && changes.some(Change.$is('Gone')) ? Result.failVoid : Result.succeed(control)));
        const pending = HashSet.fromIterable(
            Record.values(controls)
                .filter((control) => Option.isNone(control.presence) && Option.isNone(control.retired))
                .map((control) => control.tally.last),
        );
        const lead = Option.map(cause, (by) => ({
            ...by,
            tally: sight(
                Option.map(
                    Array.findFirst(last.openers, (held) => held.container === by.container && held.control === by.control),
                    Struct.get('tally'),
                ),
            ),
        }));
        const kept: InterfaceMap['containers'][string] = {
            container: {
                ...Struct.pick(read, ['kind', 'modal', 'layer', 'size', 'default', 'cancel', 'close']),
                title: Option.map(read.title, hide),
                openers: Array.take(
                    Array.dedupeWith([...Option.toArray(lead), ...last.openers], (left, right) => left.container === right.container && left.control === right.control),
                    LIMITS.openers,
                ),
                tally: sight(Option.map(entry, (held) => held.container.tally)),
                scopes: [{ at: read.at, atoms: scope }, ...last.scopes].filter((reading) => HashSet.has(pending, reading.at)),
            },
            controls,
        };
        const next = Record.set(this.containers, container, kept);
        const linked = Option.gen(function* () {
            const by = yield* Option.filter(cause, (leading) => leading.container !== container);
            const from = yield* Record.get(next, by.container);
            const opener = yield* Option.filter(Record.get(from.controls, by.control), ({ opens }) => !Option.contains(opens, container));
            return Record.set(next, by.container, { ...from, controls: Record.set(from.controls, by.control, { ...opener, opens: Option.some(container) }) });
        });
        return [[...(Option.isNone(entry) ? [Change.Appeared({ at: container })] : []), ...Record.values(folded).flatMap(({ changes }): readonly Change[] => changes)], new InterfaceMap({ application, containers: Option.getOrElse(linked, () => next) })];
    }

    learn(described: Description): readonly [Description, InterfaceMap] {
        const { builds, options: stored } = this.application;
        const build = Array.head(builds);
        const held = HashMap.fromIterable(
            Option.getOrElse(
                Option.flatMap(build, (at) => Record.get(stored, at)),
                Array.empty,
            ),
        );
        const seen = described.containers
            .flatMap(({ controls }) => controls)
            .flatMap(({ value, options = [] }) => options.filter(({ label }) => value?._tag === 'text' && value.value === label))
            .map(({ locator, ...stated }) => [locator, stated] as const);
        const learned = HashMap.union(held, HashMap.fromIterable(seen));
        const fresh = seen.some(([locator, stated]) => !Option.contains(HashMap.get(held, locator), stated));
        const taught = Option.map(
            Option.filter(build, () => fresh),
            (at) => new InterfaceMap({ containers: this.containers, application: { ...this.application, options: { ...Record.filter(stored, (_, under) => builds.includes(under)), [at]: Array.fromIterable(learned) } } }),
        );
        return [
            Description.make({
                ...described,
                containers: described.containers.map(({ controls, ...container }) => ({
                    ...container,
                    controls: controls.map(({ options, ...control }) => ({ ...control, ...(options === undefined ? {} : { options: options.map((option) => ({ ...option, ...Option.getOrUndefined(HashMap.get(learned, option.locator)) })) }) })),
                })),
            }),
            Option.getOrElse(taught, () => this),
        ];
    }

    prove({ at, from, outcome }: { readonly at: typeof Element.Opener.Type; readonly from: typeof Act.Route.Kind.Type; readonly outcome: (typeof Act.Outcome.Type)['outcomes'][number] }): InterfaceMap {
        const build = Array.head(this.application.builds);
        const attempt = Result.match(outcome, {
            onSuccess: ({ proof, route, tried }) => (proof === 'unanswered' || proof === 'exit' || proof === 'standing' ? Option.none() : Option.some({ passed: Option.some<typeof Act.Route.Kind.Type>(route?._tag ?? 'accessibility'), failed: tried.map(Struct.get('route')) })),
            onFailure: () => Option.some({ passed: Option.none<typeof Act.Route.Kind.Type>(), failed: Array.drop(Act.Route.Kind.literals, Act.Route.Kind.literals.indexOf(from)) }),
        });
        const proven = Option.gen(this, function* () {
            const { passed, failed } = yield* attempt;
            const entry = yield* Record.get(this.containers, at.container);
            const control = yield* Record.get(entry.controls, at.control);
            const prior = Option.filter(control.route, (held) => Equal.equals(held.build, build));
            const route = { build, passed, failed: Array.difference(Array.union(Option.match(prior, { onNone: Array.empty, onSome: Struct.get('failed') }), failed), Option.toArray(passed)) };
            return new InterfaceMap({ application: this.application, containers: Record.set(this.containers, at.container, { ...entry, controls: Record.set(entry.controls, at.control, { ...control, route: Option.some(route) }) }) });
        });
        return Option.getOrElse(proven, () => this);
    }

    sight(pointer: (typeof Element.Model.Type)['pointer']): InterfaceMap {
        const seen = Option.all({
            build: Array.head(this.application.builds),
            tool: Option.fromUndefinedOr(pointer?.tool),
            digest: Option.flatMap(Option.fromUndefinedOr(pointer?.cursor), Element.Pointer.fields.cursor.schema.match({ system: () => Option.none<string>(), custom: ({ digest }) => Option.some(digest) })),
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

    // --- [STORAGE]

    static readonly catalog = catalog;
    static readonly Store = Context.Service<
        InterfaceMap,
        {
            readonly recall: (instance: Watched) => Effect.Effect<InterfaceMap, Effect.Error<ReturnType<typeof InterfaceMap.open>>>;
            readonly learn: (instance: Watched, described: Description) => Effect.Effect<Description, Effect.Error<ReturnType<typeof InterfaceMap.open>>>;
            readonly read: (bundle: string) => Effect.Effect<Readonly<Record<string, InterfaceMap>>, Effect.Error<ReturnType<typeof InterfaceMap.open>>>;
            readonly prove: (instance: Watched, proofs: readonly Parameters<InterfaceMap['prove']>[0][]) => Effect.Effect<void, Effect.Error<ReturnType<typeof InterfaceMap.open>>>;
            readonly sight: (instance: Watched, pointer: (typeof Element.Model.Type)['pointer']) => Effect.Effect<void, Effect.Error<ReturnType<typeof InterfaceMap.open>>>;
        }
    >('@rasm/applications/map/map/Store');
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
        const [loaded, user, sql, services] = yield* Effect.all([InterfaceMap.read(bundle, language), Effect.sync(() => userInfo()), SqlClient.SqlClient, Effect.context<Effect.Services<ReturnType<typeof InterfaceMap.read>>>()]);
        const state = yield* SynchronizedRef.make(loaded);
        const change = <A>(step: (map: InterfaceMap) => readonly [A, InterfaceMap]) =>
            SynchronizedRef.modifyEffect(state, (map) => {
                const [value, next] = step(map);
                const rows = Record.collect(
                    Record.filter(next.containers, (entry, locator) => entry !== map.containers[locator]),
                    (locator, body) => ({ bundle, language, locator, body }),
                );
                const writes = [...(next.application === map.application ? [] : [Relation.upsert(applications, [{ bundle, language, body: next.application }])]), ...(Array.isReadonlyArrayNonEmpty(rows) ? [Relation.upsert(containers, rows)] : [])];
                return sql.withTransaction(Effect.all(writes)).pipe(Effect.when(Effect.succeed(Array.isReadonlyArrayNonEmpty(writes))), Effect.as([value, next] as const), Effect.provideContext(services));
            });
        return {
            present: SynchronizedRef.get(state),
            observe: Effect.fn('InterfaceMap.observe')(function* (read: typeof Element.Observation.Type) {
                const changes = yield* change((map) => map.fold(read, user));
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
            learn: (described: Description) => change((map) => map.learn(described)),
            prove: (proofs: readonly Parameters<InterfaceMap['prove']>[0][]) => change((map) => [undefined, proofs.reduce((proven, proof) => proven.prove(proof), map)] as const),
            sight: (pointer: (typeof Element.Model.Type)['pointer']) => change((map) => [undefined, map.sight(pointer)] as const),
        };
    });
    static readonly layer = (folder: string) =>
        Layer.effect(
            InterfaceMap.Store,
            Effect.gen(function* () {
                const [server, scope, stored] = yield* Effect.all([ApplicationServer, Effect.scope, Effect.context<Effect.Services<ReturnType<typeof InterfaceMap.read>>>()]);
                const exited = (instance: Instance['instance']) =>
                    SubscriptionRef.changes(server.running).pipe(
                        Stream.dropWhile((rows) => !Record.has(Instance.byInstance(rows), instance)),
                        Stream.filter((rows) => !Record.has(Instance.running(rows), instance)),
                        Stream.runHead,
                    );
                const maps = yield* RcMap.make({ lookup: ({ identifier, language }: Pick<Watched, 'identifier' | 'language'>) => InterfaceMap.open(identifier, language) });
                const driven: RcMap.RcMap<Watched, Effect.Success<ReturnType<typeof InterfaceMap.open>>, Effect.Error<ReturnType<typeof InterfaceMap.open>>> = yield* RcMap.make({
                    lookup: ({ instance, ...application }: Watched) => RcMap.get(maps, application).pipe(Effect.tap(exited(instance).pipe(Effect.andThen(RcMap.invalidate(driven, { instance, ...application })), Effect.forkIn(scope)))),
                    idleTimeToLive: Duration.infinity,
                });
                const using = <A, E>({ instance, identifier, language }: Watched, use: (opened: Effect.Success<ReturnType<typeof InterfaceMap.open>>) => Effect.Effect<A, E>) =>
                    RcMap.get(driven, { instance, identifier, language }).pipe(
                        Effect.tapError(() => RcMap.invalidate(driven, { instance, identifier, language })),
                        Effect.flatMap(use),
                        Effect.scoped,
                    );
                yield* server.observations.pipe(
                    Stream.filterMap(({ instance, identifier, language, observation }) => (identifier === undefined ? Result.failVoid : Result.succeed({ watched: { instance, identifier, language }, observation }))),
                    Stream.mapEffect(({ watched, observation }) =>
                        Effect.catch(
                            using(watched, ({ observe }) => observe(observation)),
                            (error) => Effect.annotateLogs(Effect.logWarning('Interface map fold failed', error), watched),
                        ),
                    ),
                    Stream.runDrain,
                    Effect.tapCause((cause) => Effect.logError('Interface map observations ended', cause)),
                    Effect.forkScoped,
                );
                return {
                    recall: (instance) => using(instance, Struct.get('present')),
                    learn: (instance, described) => using(instance, ({ learn }) => learn(described)),
                    read: Effect.fnUntraced(
                        function* (bundle: string) {
                            const rows = yield* Relation.select(applications, { bundle: [bundle] });
                            return yield* Effect.all(Record.fromIterableWith(rows, ({ language }) => [language, Effect.flatMap(RcMap.getOption(maps, { identifier: bundle, language }), Option.match({ onNone: () => InterfaceMap.read(bundle, language), onSome: (opened) => opened.present }))]));
                        },
                        Effect.scoped,
                        Effect.provideContext(stored),
                    ),
                    prove: (instance, proofs) => using(instance, ({ prove }) => prove(proofs)),
                    sight: (instance, pointer) => using(instance, ({ sight }) => sight(pointer)),
                };
            }),
        ).pipe(Layer.provide(Catalog.layer(InterfaceMap.catalog, sqliteNodeEngine({ filename: join(folder, 'map.sqlite') }))));
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { InterfaceMap };
