import { Description, type Source } from '@rasm/applications/description';
import { InterfaceMap } from '@rasm/applications/map';
import { ApplicationServer } from '@rasm/applications/server/client';
import { Element } from '@rasm/applications/server/element';
import { ApplicationFailure } from '@rasm/applications/server/failure';
import type { Instance } from '@rasm/applications/server/instance';
import { Format } from '@rasm/mcp/report';
import { Handle } from '@rasm/mcp/server';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Cause, Context, DateTime, Effect, Function, HashMap, HashSet, Match, Option, Order, Predicate, pipe, Record, Result, Schema, String, Struct } from 'effect';
import type { Target } from '#/handle/target';

// --- [TYPES] ---------------------------------------------------------------------------

type Container = typeof Element.cases.container.Type;
type Stored = InterfaceMap['containers'][string];
type Remembered = { readonly locator: string } & (({ readonly _tag: 'container' } & Stored['container']) | ({ readonly _tag: 'control' } & Stored['controls'][string]));
type Node = { readonly token: Option.Option<string>; readonly name: Option.Option<string>; readonly value: Option.Option<string>; readonly above: readonly string[]; readonly path: Option.Option<string> } & (
    | { readonly live: true; readonly element: Element; readonly containers: Array.NonEmptyReadonlyArray<Container> }
    | { readonly live: false; readonly element: Remembered; readonly containers: Array.NonEmptyReadonlyArray<Extract<Remembered, { readonly _tag: 'container' }>> }
);
type Current = Effect.Success<typeof Target.current>;
interface Address {
    readonly instance: Instance['instance'];
    readonly model: typeof Element.Model.Type;
    readonly map: InterfaceMap;
    readonly reads: HashMap.HashMap<typeof Element.Ref.Type, { readonly sequence: number; readonly whole: boolean }>;
    readonly left: HashMap.HashMap<typeof Element.Ref.Type, { readonly node: Extract<Node, { readonly live: true }>; readonly gone: Option.Option<readonly string[]> }>;
    readonly routes: Record.ReadonlyRecord<string, typeof Description.Locator.Type>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SEPARATOR = ' > ';
const WORDS = new Intl.Segmenter(undefined, { granularity: 'word' });
const REFERENCE = /^@\p{L}(?<ref>\p{Nd}+)$/u;
const APPLICATION = Element.Scope.cases.application.make({});

// --- [MODELS] --------------------------------------------------------------------------

const At = Schema.NonEmptyString.annotate({
    description: 'Reference a result printed (@a12), path find prints for a remembered item (File > Export… > Units), or labels from the outer container in joined by > (New Document > Units), matched ignoring case, accents, and punctuation',
});

// --- [SERVICES] ------------------------------------------------------------------------

const Held = Context.Reference<Option.Option<Address>>('@rasm/application-tools/handle/address/Held', { defaultValue: Option.none });
const Linked = Context.Reference<(row: Current['row']) => Effect.Effect<Pick<Source<Error>, 'describe' | 'operate'>, Error>>('@rasm/application-tools/handle/address/Linked', { defaultValue: () => () => new Cause.NoSuchElementError() });
const Toolbar = Context.Reference<(row: Current['row']) => Effect.Effect<Parameters<typeof Description.fold>[1], Error>>('@rasm/application-tools/handle/address/Toolbar', { defaultValue: () => () => Effect.succeed({ tools: HashMap.empty(), toolbar: [] }) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const isContainer = Element.isAnyOf(['container']);
const label = (text: string | undefined) =>
    Option.fromUndefinedOr(text).pipe(
        Option.map((shown) => shown.replaceAll(/\s+/gu, ' ').trim()),
        Option.filter(String.isNonEmpty),
    );
const trace = (above: readonly string[], name: Option.Option<string>) => ({ above, name, path: Option.map(Option.liftPredicate([...above, ...Option.toArray(name)], Array.isReadonlyArrayNonEmpty), Array.join(SEPARATOR)) });
const parse = (at: string) => Option.map(Option.fromNullishOr(REFERENCE.exec(at)?.groups?.['ref']), Number);
const measure = (values: readonly number[], by: string) => values.map((each) => Format.count(each)).join(by);
const refer = (kind: string, ref: typeof Element.Ref.Type) => `@${kind.charAt(0)}${ref}`;
const print =
    (kind: (typeof Element.cases.control.Type)['kind']) =>
    (value: typeof Element.Fact.Type | typeof Element.Value.Type): string =>
        Match.value(value).pipe(
            Match.when(Match.boolean, (on) => print(kind)({ _tag: 'bool', value: on })),
            Match.when(Match.number, (amount) => print(kind)({ _tag: Number.isInteger(amount) ? 'integer' : 'real', value: amount })),
            Match.when(Match.string, (text) => print(kind)({ _tag: 'text', value: text })),
            Match.tagsExhaustive({
                bool: (fact) => (fact.value ? 'on' : 'off'),
                integer: (fact) => (kind === 'toggle' ? ((['off', 'on', 'mixed'] as const)[fact.value] ?? `${fact.value}`) : `${fact.value}`),
                real: (fact) => Format.count(fact.value),
                text: (fact) => `"${fact.value}"`,
                date: (fact) => fact.value,
                url: (fact) => fact.value,
                point: (fact) => measure(fact.value, ','),
                size: (fact) => measure(fact.value, '×'),
                rect: (fact) => `${measure(fact.value[0], ',')} ${measure(fact.value[1], '×')}`,
                color: ({ red, green, blue, alpha }) => `sRGB ${measure([red, green, blue, alpha], ' ')}`,
                range: ({ location, length }) => `${location}+${length}`,
                element: ({ id }) => `element ${id}`,
                refused: ({ error }) => `unread, AXError ${error}`,
                list: ({ values }) => `[${values.map(print(kind)).join(', ')}]`,
                record: ({ fields }) => `{${Record.collect(fields, (field, each) => `${field}: ${print(kind)(each)}`).join(', ')}}`,
            }),
        );
const segment = (text: string) =>
    Iterator.from(WORDS.segment(text))
        .filter(({ isWordLike }) => isWordLike === true)
        .map((word) => word.segment)
        .toArray();
const holds = (step: string) => {
    const [wanted, same] = [segment(step), Array.makeEquivalence(Unmatched.named)];
    return (name: string) => Unmatched.named(name, step) || Array.window(segment(name), wanted.length).some((run) => same(run, wanted));
};
const split = (at: string) => {
    const segments = String.split(at, '>').map(String.trim).filter(String.isNonEmpty);
    const tests = Array.dropRight(segments, 1).map(holds);
    return {
        last: Option.getOrElse(Array.last(segments), () => ''),
        descends: (above: readonly string[]) => Array.isReadonlyArrayEmpty(above.reduce((left, held) => (Option.exists(Array.head(left), (test) => test(held)) ? Array.drop(left, 1) : left), tests)),
    };
};
const sift = (elements: readonly Element[], place: (held: Container, nested: () => readonly Element[], keep: (adopted: readonly Element[]) => readonly Element[]) => readonly Element[]): readonly Element[] =>
    elements.filter(isContainer).flatMap((held) => {
        const nested = () => sift(held.children, place);
        return place(held, nested, (adopted) => [{ ...held, children: [...held.children.filter(Predicate.not(isContainer)), ...nested(), ...adopted] }]);
    });
const flatten = (elements: readonly Element[], above: readonly string[], containers: readonly Container[]): readonly Extract<Node, { readonly live: true }>[] =>
    elements.flatMap((element) => {
        const { kind, named, value } = Element.match(element, {
            container: (held) => ({ kind: held.kind, named: held.title, value: Option.none<string>() }),
            control: (held) => ({ kind: held.kind, named: held.label?.text, value: Option.map(Option.fromUndefinedOr(held.value), print(held.kind)) }),
            option: (held) => ({ kind: held._tag, named: held.label, value: Option.none<string>() }),
        });
        const traced = trace(element._tag === 'control' ? [...above, ...Array.fromNullishOr(element.section)] : above, label(named));
        const within = isContainer(element) ? [...containers, element] : containers;
        const below = flatten(element.children, [...traced.above, ...Option.toArray(traced.name)], within);
        return Array.isReadonlyArrayNonEmpty(within) ? [{ ...traced, live: true as const, element, containers: within, token: Option.some(refer(kind, element.ref)), value }, ...below] : below;
    });
const walk = Function.memoize((read: Address) => {
    const shown = flatten(read.model.elements, [], []);
    const carried = HashMap.toValues(read.left).flatMap(({ node, gone }) => (Option.isNone(gone) ? [node] : []));
    const pairs = [...carried, ...shown].map((node) => [node.element.ref, node] as const);
    const refs = new Set(pairs.map(([ref]) => ref));
    const unread = shown.flatMap((node) => (node.element._tag === 'control' && node.element.opens !== undefined && !refs.has(node.element.opens) ? [[node.element.opens, { ...node, token: Option.some(refer('menu', node.element.opens)) }] as const] : []));
    const placed = [...shown, ...carried].flatMap((node) => (node.element._tag === 'option' ? [] : [{ at: node.element.locator, node }]));
    return { nodes: [...shown, ...carried], byRef: HashMap.fromIterable([...unread, ...pairs]), located: Record.map(Array.groupBy(placed, Struct.get('at')), Array.map(Struct.get('node'))) };
});
const remember = Function.memoize((read: Address) => {
    const { containers } = read.map;
    const { located } = walk(read);
    const lead = (locator: string, seen: HashSet.HashSet<string>): readonly string[] =>
        Option.gen(function* () {
            const { container } = yield* Record.get(containers, locator);
            const openers = Array.sortWith(container.openers, ({ tally }) => tally.first, DateTime.Order);
            const from = yield* Array.findFirst(openers, (opener) => !HashSet.has(seen, opener.container));
            const held = yield* Record.get(containers, from.container);
            const control = yield* Record.get(held.controls, from.control);
            return [...lead(from.container, HashSet.add(seen, from.container)), ...Option.toArray(Option.flatMap(held.container.title, label)), ...Option.toArray(control.section), ...Option.toArray(Option.flatMap(Option.map(control.label, Struct.get('text')), label))];
        }).pipe(Option.getOrElse(Array.empty<string>));
    const listed = Array.sortWith(Record.toEntries(containers), ([, entry]) => entry.container.tally.first, DateTime.Order).flatMap(([locator, entry]): readonly Extract<Node, { readonly live: false }>[] => {
        const container = { ...entry.container, _tag: 'container' as const, locator };
        const title = Option.orElse(Option.flatMap(entry.container.title, label), () => Option.some<string>(entry.container.kind));
        const traced = trace(lead(locator, HashSet.make(locator)), title);
        const controls = Array.sortWith(Record.toEntries(entry.controls), ([, control]) => control.tally.first, DateTime.Order).filter(([, control]) => Option.isNone(control.retired));
        return [
            { ...traced, live: false, element: container, containers: [container], token: traced.path, value: Option.none() },
            ...controls.map(([at, control]) => {
                const named = trace([...traced.above, ...Option.toArray(title), ...Option.toArray(control.section)], Option.flatMap(Option.map(control.label, Struct.get('text')), label));
                return { ...named, live: false as const, element: { ...control, _tag: 'control' as const, locator: at }, containers: [container] as const, token: Option.zipRight(named.name, named.path), value: Option.map(control.value, print(control.kind)) };
            }),
        ];
    });
    const [, counted] = Array.mapAccum(listed, HashMap.empty<string, number>(), (counts, each) => {
        const count = Option.match(each.token, { onNone: () => 1, onSome: (plain) => Option.getOrElse(HashMap.get(counts, plain), () => 0) + 1 });
        return [Option.match(each.token, { onNone: () => counts, onSome: (plain) => HashMap.set(counts, plain, count) }), count === 1 ? each : { ...each, token: Option.map(each.token, (plain) => `${plain} #${count}`) }] as const;
    });
    const [aliases, nodes] = Array.partition(counted, (each) => Option.match(Record.get(located, each.element.locator), { onNone: () => Result.fail(each), onSome: (found) => Result.succeed(Option.all({ at: each.token, to: Option.liftPredicate(found, Predicate.isTupleOf(1)) })) }));
    return {
        nodes,
        paths: Record.fromEntries<readonly [string, Node]>([...Array.getSomes(aliases).map(({ at, to }) => [at, to[0]] as const), ...nodes.flatMap((each) => Option.toArray(Option.map(each.token, (at) => [at, each] as const)))]),
        located: Record.fromIterableBy(nodes, ({ element }) => element.locator),
        under: Array.groupBy(
            nodes.filter(({ element }) => element._tag === 'control'),
            ({ containers: within }) => Array.headNonEmpty(within).locator,
        ),
    };
});
const get = (read: Address, ref: typeof Element.Ref.Type) => HashMap.get(walk(read).byRef, ref);
const locate = (read: Address, locator: string): readonly Node[] => Option.getOrElse(Record.get(walk(read).located, locator), () => Option.toArray(Record.get(remember(read).located, locator)));
const list = (read: Address) => walk(read).nodes;
const recall = (read: Address, within?: string): readonly Node[] => (within === undefined ? remember(read).nodes : Option.getOrElse(Record.get(remember(read).under, within), Array.empty<Node>));
const route = (read: Address, ref: typeof Element.Ref.Type) => Record.get(read.routes, `${ref}`);
const narrow = (read: Address, elements: readonly Element[]): Address => ({ ...read, model: { ...read.model, elements }, left: HashMap.empty() });
const intern = (instance: Instance['instance'], node: Node) => Effect.flatMap(node.live ? Effect.succeed([node.element.ref]) : ApplicationServer.use((server) => server.intern({ instance, locators: [node.element.locator] })), (refs) => Effect.fromOption(Array.head(refs)));
const caption = (kind: Container['kind'], title: Option.Option<string>, ref: Option.Option<number>) => [...Option.toArray(Option.map(ref, (held) => refer(kind, held))), String.noCase()(kind), ...Option.toArray(Option.map(title, Format.quoted))].join(' ');
const show = (read: Address, locator: string) =>
    Option.toArray(Record.get(walk(read).located, locator))
        .flat()
        .flatMap(({ element }) => (isContainer(element) ? [element] : []));
const refuse = (read: Address, subject: string, locator: string, known: string) => {
    const open = show(read, locator);
    const shut = HashMap.toValues(read.left).flatMap(({ node, gone }) => (Option.isSome(gone) && isContainer(node.element) && node.element.locator === locator ? [[caption(node.element.kind, label(node.element.title), Option.some(node.element.ref)), ...gone.value]] : []));
    const values = Array.match(open, { onEmpty: () => Option.getOrElse(Array.head(shut), () => [known]), onNonEmpty: (held) => held.map(({ kind, title, ref }) => caption(kind, label(title), Option.some(ref))) });
    return ApplicationFailure.of(Array.isReadonlyArrayEmpty(open) ? 'notOpen' : 'absent', subject, values);
};
const seek = (read: Address, at: string, recalled: Option.Option<ReturnType<typeof remember>>): Result.Result<Node, ApplicationFailure | Unmatched> => {
    const { nodes, byRef, located } = walk(read);
    const miss = (subject: string, candidates: readonly Node[]) => Result.fail(Unmatched.of({ subject, query: Option.some(at), candidates: candidates.map((each) => [...Option.toArray(each.token), ...(each.live ? Option.toArray(each.path) : [])].map(Format.quoted).join(' ')) }));
    return Option.match(parse(at), {
        onSome: (ref) => {
            const held = HashMap.get(byRef, ref);
            const lapsed = Option.filter(HashMap.get(read.left, ref), ({ gone }) => Option.isSome(gone));
            const again: readonly Node[] = pipe(
                Option.flatMap(lapsed, ({ node }) => Option.liftPredicate(node.element, Element.isAnyOf(['container', 'control']))),
                Option.match({ onNone: Array.empty<Node>, onSome: ({ locator }) => [...Option.toArray(Record.get(located, locator)).flat(), ...Option.toArray(Option.flatMap(recalled, (map) => Record.get(map.located, locator)))] }),
            );
            return Match.value({ exact: Option.filter(held, ({ token }) => Option.contains(token, at)), lapsed }).pipe(
                Match.when({ exact: Option.isSome }, ({ exact }) => Result.succeed(exact.value)),
                Match.when({ lapsed: Option.isSome }, ({ lapsed: lost }) => {
                    const sat = Array.lastNonEmpty(lost.value.node.containers);
                    return Result.fromOption(Array.head(again), () => refuse(read, at, sat.locator, caption(sat.kind, label(sat.title), Option.some(sat.ref))));
                }),
                Match.orElse(() => miss('reference', Option.toArray(held))),
            );
        },
        onNone: () => {
            const { last, descends } = split(at);
            const pools: readonly (readonly Node[])[] = [nodes, ...Option.toArray(recalled).map((map) => map.nodes)];
            const tiers = Array.cartesian(pools, [(name: string) => Unmatched.named(name, last), holds(last)]);
            const found = Array.findFirst(tiers, ([pool, test]) =>
                Option.liftPredicate(
                    pool.filter((each) => Option.exists(each.name, test) && descends(each.above)),
                    Array.isReadonlyArrayNonEmpty,
                ),
            );
            const sectioned = nodes.flatMap((each) => (each.element._tag === 'control' && each.element.section !== undefined && Unmatched.named(each.element.section, last) && descends(Array.dropRight(each.above, 1)) ? Option.toArray(HashMap.get(byRef, Array.lastNonEmpty(each.containers).ref)) : []));
            const candidates = Option.getOrElse(found, (): readonly Node[] => [...new Set(sectioned)]);
            const nearest = () =>
                Unmatched.rank(
                    last,
                    pools.flat().filter(({ name }) => Option.isSome(name)),
                    ({ name }) => Option.getOrElse(name, () => last),
                );
            return Array.match(candidates, { onEmpty: () => miss('control, option, or container', nearest()), onNonEmpty: (several) => (Predicate.isTupleOf(several, 1) ? Result.succeed(several[0]) : miss('single control, option, or container', several)) });
        },
    });
};
const resolve = (read: Address, at: string) => Result.orElse(seek(read, at, Option.none()), () => Option.match(Record.get(remember(read).paths, at), { onSome: (exact) => Result.succeed(exact), onNone: () => seek(read, at, Option.some(remember(read))) }));
const find = (read: Address, query: string, kind: Option.Option<(typeof Element.cases.control.Type)['kind']>, among: readonly Node[] = [...walk(read).nodes, ...remember(read).nodes]) => {
    const { last, descends } = split(query);
    const kept = among.filter((each) => descends(each.above) && Option.match(kind, { onNone: () => Option.isSome(each.name) || Option.isSome(each.value), onSome: (wanted) => each.element._tag === 'control' && each.element.kind === wanted }));
    const ranked = Unmatched.rank(last, kept, (each) =>
        Match.value(each).pipe(
            Match.when({ live: true }, ({ element }) => [...Array.fromNullishOr(element.help), ...(element._tag === 'control' ? (element.labels ?? []) : [])]),
            Match.when({ element: { _tag: 'control' } }, ({ element }) => [...Option.toArray(element.help), ...Option.getOrElse(element.labels, Array.empty<string>)]),
            Match.orElse(Array.empty<string>),
            Array.prependAll([...Option.toArray(each.name), ...Option.toArray(each.value)]),
            Array.join(' '),
        ),
    );
    return Array.sortWith(ranked, ({ live }) => live, Order.flip(Order.Boolean));
};
const open = (read: Address, wanted: Node): Result.Result<readonly Node[], ApplicationFailure | Unmatched> => {
    const holder = (node: Node) => (node.live ? Option.none() : Option.some(Array.lastNonEmpty(node.containers)));
    const links = Array.reverse(
        Array.unfold({ at: wanted, seen: HashSet.empty<string>() }, ({ at, seen }) =>
            Option.gen(function* () {
                const shut = yield* Option.filter(holder(at), ({ locator }) => Array.isReadonlyArrayEmpty(show(read, locator)));
                const { control } = yield* Array.findFirst(shut.openers, (opener) => !HashSet.has(seen, opener.control) && Array.isReadonlyArrayNonEmpty(locate(read, opener.control)));
                const found = yield* Option.liftPredicate(locate(read, control), Array.isReadonlyArrayNonEmpty);
                return [found, { at: Array.headNonEmpty(found), seen: HashSet.add(seen, control) }] as const;
            }),
        ),
    );
    const chain = links.map(Array.headNonEmpty);
    const root = Array.headNonEmpty(Array.append(chain, wanted));
    const sits = holder(root);
    const subject = Option.getOrElse(Option.firstSomeOf([wanted.token, wanted.path]), () => wanted.element._tag);
    const alike = (tokens: readonly string[]) => Result.fail(Unmatched.of({ subject: `single item reaching ${subject}`, candidates: tokens }));
    const menu = ({ containers }: Node) => containers.at(-1)?.kind === 'menu';
    const shown = Option.match(sits, { onNone: Array.empty<Container>, onSome: ({ locator }) => show(read, locator) });
    return Match.value({ doubled: Array.findFirst(links, (found) => found.length > 1), reached: root.live || menu(root), shown, sits }).pipe(
        Match.when({ doubled: Option.isSome }, ({ doubled }) => alike(Array.getSomes(doubled.value.map(({ token }) => token)))),
        Match.when({ reached: true }, () => Result.succeed(chain.filter((_, at) => !Option.exists(Array.get([...chain, wanted], at + 1), menu)))),
        Match.when({ shown: (held: readonly Container[]) => held.length > 1 }, ({ shown: several }) => alike(several.map(({ kind, ref }) => refer(kind, ref)))),
        Match.when({ shown: (held: readonly Container[]) => held.some(({ truncated }) => truncated) }, () => Result.succeed([])),
        Match.when({ sits: Option.isSome }, ({ sits: held }) => Result.fail(refuse(read, subject, held.value.locator, caption(held.value.kind, Option.flatMap(held.value.title, label), Option.none())))),
        Match.orElse(() => Result.succeed([])),
    );
};
const settle = (before: Address, stated: typeof Element.Model.Type, cause: typeof Element.Scope.Type | { readonly _tag: 'closed'; readonly ref: typeof Element.Ref.Type; readonly facts: readonly string[] }, map: InterfaceMap, routes: Address['routes']): Address => {
    const { nodes, byRef } = walk(before);
    const [whole, facts, said] = [cause._tag === 'application', cause._tag === 'closed' ? cause.facts : [], flatten(stated.elements, [], [])];
    const newer = (ref: number) => Option.exists(HashMap.get(before.reads, ref), ({ sequence }) => sequence > stated.sequence);
    const fresh = HashMap.fromIterable(said.flatMap(({ element }) => (isContainer(element) && !newer(element.ref) ? [[element.ref, element] as const] : [])));
    const held = (ref: number) => Option.flatMap(HashMap.get(byRef, ref), ({ element }) => (isContainer(element) && element.ref === ref ? Option.some(element) : Option.none()));
    const gone = ({ ref }: Container) => !HashMap.has(fresh, ref) && !newer(ref) && (cause._tag === 'closed' ? ref === cause.ref : whole && Option.exists(HashMap.get(before.reads, ref), (read) => read.whole));
    const host = cause._tag === 'menu' ? Option.map(HashMap.get(byRef, cause.ref), ({ containers }) => Array.lastNonEmpty(containers).ref) : Option.none<number>();
    const [top, rooted] = [new Set(before.model.elements.map((root) => root.ref)), stated.elements.filter(isContainer).filter(({ ref }) => HashMap.has(fresh, ref))];
    const roots = new Set(rooted.map(({ ref }) => ref));
    const adopt = (ref: number) => (Option.contains(host, ref) ? rooted.filter((root) => Option.isNone(held(root.ref))).map(graft) : []);
    const graft = (version: Container): Container => ({
        ...version,
        children: [...version.children.filter((child) => !isContainer(child) || HashMap.has(fresh, child.ref)).map((child) => (isContainer(child) ? graft(child) : child)), ...Option.match(held(version.ref), { onNone: Array.empty<Element>, onSome: ({ children }) => sift(children, place) }), ...adopt(version.ref)],
    });
    const place: Parameters<typeof sift>[1] = (prior, nested, keep) => Option.match(HashMap.get(fresh, prior.ref), { onSome: (version) => (roots.has(version.ref) && !top.has(version.ref) ? [graft(version)] : []), onNone: () => (gone(prior) ? nested() : keep(adopt(prior.ref))) });
    const raised = rooted.filter(({ ref }) => top.has(ref) || (Option.isNone(host) && Option.isNone(held(ref))));
    const owned = (listed: typeof said) =>
        Map.groupBy(
            listed.filter(({ element }) => !isContainer(element)),
            ({ containers }) => Array.lastNonEmpty(containers).ref,
        );
    const [now, was] = [owned(said), owned(nodes)];
    const versions = HashMap.toValues(fresh).map((version) => {
        const listed = (now.get(version.ref) ?? []).map(({ element }) => element);
        const refs = new Set(listed.map(({ ref }) => ref));
        const unlisted = listed.reduce((count, each) => count + (each._tag === 'control' && each.kind === 'multi' ? (each.total ?? 0) : 0) - (each._tag === 'control' && each.kind === 'row' ? 1 : 0), 0);
        return { version, short: version.truncated || unlisted > 0, lacking: (was.get(version.ref) ?? []).filter(({ element }) => !refs.has(element.ref)) };
    });
    const dropped = nodes.filter(({ containers }) => gone(Array.lastNonEmpty(containers)));
    const complete = new Set(versions.flatMap(({ version, short }) => (short ? [] : [version.locator])));
    const present = new Set(said.map(({ element }) => element.ref));
    const pointer = whole && stated.sequence >= before.model.sequence ? stated.pointer : before.model.pointer;
    const marks = [...HashMap.keys(fresh), ...dropped.flatMap(({ element }) => (isContainer(element) ? [element.ref] : []))].map((ref) => [ref, { sequence: stated.sequence, whole: whole || Option.exists(HashMap.get(before.reads, ref), (read) => read.whole) }] as const);
    return {
        instance: before.instance,
        model: { sequence: Math.max(before.model.sequence, stated.sequence), elements: [...raised.map(graft), ...sift(before.model.elements, place)], ...(pointer === undefined ? {} : { pointer }) },
        map,
        reads: HashMap.setMany(before.reads, marks),
        left: pipe(
            before.left,
            HashMap.filter(({ node, gone: lost }) => Option.isNone(lost) || !complete.has(Array.lastNonEmpty(node.containers).locator)),
            HashMap.map((lost) => (cause._tag === 'closed' && Option.isSome(lost.gone) && Array.lastNonEmpty(lost.node.containers).ref === cause.ref ? { ...lost, gone: Option.some(facts) } : lost)),
            HashMap.setMany(versions.flatMap(({ short, lacking }) => lacking.map((node) => [node.element.ref, { node, gone: short ? Option.none<readonly string[]>() : Option.some<readonly string[]>([]) }] as const))),
            HashMap.setMany(dropped.map((node) => [node.element.ref, { node, gone: Option.some(facts) }] as const)),
            HashMap.filter((_, ref) => !present.has(ref)),
        ),
        routes: { ...before.routes, ...routes },
    };
};
const close = (read: Address, sequence: number, ref: typeof Element.Ref.Type, facts: readonly string[]) => (HashMap.has(walk(read).byRef, ref) || HashMap.has(read.left, ref) ? settle(read, { sequence, elements: [] }, { _tag: 'closed', ref, facts }, read.map, {}) : read);
const scan = Effect.fnUntraced(function* ({ row, live }: Current, scope: typeof Element.Scope.Type) {
    const [server, maps, handle, source, toolbar] = yield* Effect.all([ApplicationServer, InterfaceMap.Store, Handle, Linked, Toolbar]);
    const [map, last, client] = yield* Effect.all([maps.recall(live), handle.get(Held), Effect.option(source(row))], { concurrency: 'unbounded' });
    const own = Option.filter((held: Address) => held.instance === live.instance);
    const whole = scope._tag === 'application';
    const within = Option.flatMap(Option.liftPredicate(scope, Element.Scope.isAnyOf(['container', 'menu', 'rows'])), ({ ref }) => Option.flatMap(own(last), (held) => route(held, ref)));
    const linked = Option.filter(client, () => whole || Option.isSome(within));
    const scopes: readonly (typeof Element.Scope.Type)[] = [...(whole || Option.isNone(linked) ? [scope] : []), ...(whole ? [{ _tag: 'menus' as const }] : [])];
    const known = Option.getOrUndefined(Option.map(map.known, (verified) => ({ known: verified })));
    const models = yield* Effect.forEach(scopes, (each) => server.model({ instance: live.instance, scope: each, ...(each._tag === 'application' ? known : {}) }), { concurrency: 'unbounded' });
    const accessible = Option.getOrElse(Array.head(models), (): typeof Element.Model.Type => ({ sequence: Option.match(own(last), { onNone: () => 0, onSome: (held) => held.model.sequence }), elements: [] }));
    const described = yield* Effect.transposeOption(Option.map(linked, (from) => Effect.all([from.describe(Option.match(within, { onNone: () => ({}), onSome: (at) => ({ within: at }) })), toolbar(row)])));
    const { model, routes } = yield* Option.match(described, {
        onNone: () => Effect.succeed({ model: accessible, routes: {} }),
        onSome: ([hosted, tools]) => Effect.flatMap(maps.learn(live, Description.fold(hosted, tools)), (learned) => Description.compose(accessible, learned, (locators) => server.intern({ instance: live.instance, locators }))),
    });
    yield* Effect.when(maps.sight(live, model.pointer), Effect.succeed(whole));
    const menus = Array.drop(models, 1).flatMap(Struct.get('elements'));
    const stated = { ...model, elements: Array.unionWith(model.elements, menus, (left, right) => left.ref === right.ref) };
    const merge = (state: Option.Option<Address>) =>
        settle(
            Option.getOrElse(own(state), (): Address => ({ instance: live.instance, model: { sequence: 0, elements: [] }, map, reads: HashMap.empty(), left: HashMap.empty(), routes: {} })),
            stated,
            scope,
            map,
            routes,
        );
    yield* handle.update(Held, (state) => state.pipe(merge, Option.some));
    return { stated, held: Option.getOrElse(own(yield* handle.get(Held)), () => merge(Option.none())) };
});
const read = (target: Current, scope: typeof Element.Scope.Type = APPLICATION) => Effect.map(scan(target, scope), ({ stated, held }) => (scope._tag === 'application' ? held : narrow(held, stated.elements)));
const reach = Effect.fnUntraced(function* (target: Current, at: string) {
    const held = Option.filter(yield* Handle.use((handle) => handle.get(Held)), (last) => last.instance === target.live.instance);
    const shown = Option.flatMap(held, (last) => Option.filter(Result.getSuccess(seek(last, at, Option.none())), ({ live }) => live));
    const sat = Option.flatMap(held, (last) =>
        Option.map(
            Option.flatMap(parse(at), (ref) => HashMap.get(last.left, ref)),
            ({ node }) => ({ last, ref: Array.lastNonEmpty(node.containers).ref }),
        ),
    );
    const again = Option.match(sat, {
        onNone: () => Effect.map(scan(target, APPLICATION), (now) => now.held),
        onSome: ({ last, ref }) =>
            Effect.catchIf(
                Effect.map(scan(target, { _tag: 'container', ref }), (now) => now.held),
                Schema.is(ApplicationFailure),
                () => Effect.succeed(last),
            ),
    });
    return yield* Option.match(shown, { onSome: (found) => Effect.succeed(found), onNone: () => Effect.flatMap(again, (now) => Effect.fromResult(resolve(now, at))) });
});
const Address = { At, Held, Linked, Toolbar, refer, print, read, reach, get, locate, list, recall, route, resolve, find, open, intern, narrow, close } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Address };
