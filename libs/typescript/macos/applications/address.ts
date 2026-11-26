import { Unmatched } from '@rasm/search/unmatched';
import { Array, Data, DateTime, Function, HashMap, HashSet, Match, Option, Order, pipe, Predicate, Record, Result, Schema, String, Struct } from 'effect';
import type { Container, Control, Fact, Model, Observation } from '#/contract';
import type { Entry, InterfaceMap } from '#/interface-map';

// --- [TYPES] ---------------------------------------------------------------------------

type Target = Data.TaggedEnum<{
    Container: { readonly container: Container };
    Control: { readonly control: Control; readonly container: Container };
    Option: { readonly option: NonNullable<Control['options']>[number] };
    RememberedContainer: { readonly locator: string; readonly entry: Entry };
    RememberedControl: { readonly locator: string; readonly container: string; readonly control: Entry['controls'][string]; readonly entry: Entry };
}>;
interface Item {
    readonly target: Target;
    readonly token: Option.Option<string>;
    readonly name: Option.Option<string>;
    readonly ancestors: readonly string[];
    readonly value: Option.Option<string>;
    readonly kind: Option.Option<Control['kind']>;
    readonly live: boolean;
}
interface Shown {
    readonly container: Container;
    readonly item: Item;
    readonly controls: readonly { readonly control: Control; readonly item: Item; readonly options: readonly { readonly option: NonNullable<Control['options']>[number]; readonly item: Item }[] }[];
}
interface Index {
    readonly model: Model;
    readonly items: readonly Item[];
    readonly shown: readonly Shown[];
    readonly tokens: Readonly<Record<string, Item>>;
    readonly live: Readonly<Record<string, Item>>;
    readonly located: Readonly<Record<string, Array.NonEmptyReadonlyArray<Item>>>;
    readonly remembered: { readonly build: Option.Option<string>; readonly containers: readonly Item[]; readonly controls: Readonly<Record<string, readonly Item[]>> };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SEPARATOR = ' > ';
const STATES = ['off', 'on', 'mixed'] as const;
const WORDS = new Intl.Segmenter(undefined, { granularity: 'word' });
const RUN = Array.makeEquivalence(Unmatched.named);
const REFERENCE = /^@\p{L}(?<ref>\p{Nd}+)$/u;
const BY_FIRST = Order.mapInput(DateTime.Order, ({ tally }: { readonly tally: Entry['container']['tally'] }) => tally.first);

// --- [MODELS] --------------------------------------------------------------------------

const Target = Data.taggedEnum<Target>();
const At = Schema.NonEmptyString.annotate({
    description:
        'A reference as results print it: @ with a kind letter and number for what is read now (@d3), or the path of a remembered item (File > Export… > Export > Units, numbered #2 where paths repeat), which keeps naming the item once it is open; else a label path from the outer container in, labels joined by > (New Document > Units), each label matched ignoring case, accents, and punctuation or by a run of the words it holds, items read now first',
});
const Steps = Schema.fromJsonString(Schema.Array(Schema.Json));

// --- [OPERATIONS] ----------------------------------------------------------------------

const label = (text: string | undefined) =>
    Option.fromUndefinedOr(text).pipe(
        Option.map((shown) => shown.replaceAll(/\s+/gu, ' ').trim()),
        Option.filter(String.isNonEmpty),
    );
const printFact: (fact: Fact) => string = Match.type<Fact>().pipe(
    Match.tagsExhaustive({
        bool: ({ value }) => (value ? 'on' : 'off'),
        integer: ({ value }) => `${value}`,
        real: ({ value }) => `${value}`,
        text: ({ value }) => JSON.stringify(value),
        date: ({ value }) => value,
        url: ({ value }) => value,
        point: ({ value }) => value.join(','),
        size: ({ value }) => value.join('×'),
        rect: ({ value }) => `${value[0].join(',')} ${value[1].join('×')}`,
        color: ({ red, green, blue, alpha }) => `sRGB ${[red, green, blue, alpha].join(' ')}`,
        range: ({ location, length }) => `${location}+${length}`,
        element: ({ id }) => `element ${id}`,
        refused: ({ error }) => `unread, AXError ${error}`,
        list: ({ values }) => `[${values.map((each) => printFact(each)).join(', ')}]`,
        record: ({ fields }) => `{${Record.collect(fields, (field, each) => `${field}: ${printFact(each)}`).join(', ')}}`,
    }),
);
const printValue = (kind: Control['kind'], fact: Fact) => (kind === 'toggle' && fact._tag === 'integer' ? (STATES[fact.value] ?? printFact(fact)) : printFact(fact));
const printStored = (kind: Control['kind'], value: Observation['values'][string]) =>
    printValue(
        kind,
        Match.value(value).pipe(
            Match.when(Match.boolean, (bool): Fact => ({ _tag: 'bool', value: bool })),
            Match.when(Match.number, (number): Fact => (Number.isInteger(number) ? { _tag: 'integer', value: number } : { _tag: 'real', value: number })),
            Match.orElse((text): Fact => ({ _tag: 'text', value: text })),
        ),
    );
const token = (kind: string, ref: number) => `@${kind.charAt(0)}${ref}`;
const path = (item: Item) => [...item.ancestors, ...Option.toArray(item.name)];
const keyOf = (steps: readonly Schema.Json[]) => JSON.stringify(steps);
const stepsOf = (locator: string) => Option.getOrElse(Schema.decodeOption(Steps)(locator), Array.empty);
const words = (text: string) =>
    Iterator.from(WORDS.segment(text))
        .filter(({ isWordLike }) => isWordLike === true)
        .map(({ segment }) => segment)
        .toArray();
const segmentsOf = (text: string) => String.split(text, '>').map(String.trim).filter(String.isNonEmpty);
const loose = (segment: string) => {
    const wanted = words(segment);
    return (name: string) => Unmatched.named(name, segment) || Array.window(words(name), wanted.length).some((run) => RUN(run, wanted));
};
const within = (segments: readonly string[]) => {
    const tests = segments.map(loose);
    return ({ ancestors }: Item) => Array.isReadonlyArrayEmpty(ancestors.reduce((left, held) => (Option.exists(Array.head(left), (test) => test(held)) ? Array.drop(left, 1) : left), tests));
};
const item = (target: Target, ancestors: readonly string[]): Item => {
    const read = Target.$match(target, {
        Container: ({ container }) => ({ ref: Option.some(token(container.kind, container.ref)), name: label(container.title), value: Option.none<string>(), kind: Option.none<Control['kind']>() }),
        Control: ({ control }) => ({ ref: Option.some(token(control.kind, control.ref)), name: label(control.label?.text), value: Option.map(Option.fromUndefinedOr(control.value), (fact) => printValue(control.kind, fact)), kind: Option.some(control.kind) }),
        Option: ({ option }) => ({ ref: Option.some(token('option', option.ref)), name: label(option.label), value: Option.none(), kind: Option.none() }),
        RememberedContainer: ({ entry: { container } }) => ({ ref: Option.none(), name: Option.orElse(Option.flatMap(container.title, label), () => Option.some(container.kind)), value: Option.none(), kind: Option.none() }),
        RememberedControl: ({ control }) => ({ ref: Option.none(), name: Option.flatMap(control.label, ({ text }) => label(text)), value: Option.map(control.value, (was) => printStored(control.kind, was)), kind: Option.some(control.kind) }),
    });
    return { target, ancestors, name: read.name, value: read.value, kind: read.kind, live: Option.isSome(read.ref), token: Option.orElse(read.ref, () => Option.map(read.name, (name) => [...ancestors, name].join(SEPARATOR))) };
};
const locatorOf = (target: Target) =>
    Target.$match(target, { Container: ({ container }) => Option.some(container.locator), Control: ({ control }) => Option.some(control.locator), Option: () => Option.none(), RememberedContainer: ({ locator }) => Option.some(locator), RememberedControl: ({ locator }) => Option.some(locator) });
const located = (items: readonly Item[]) =>
    Record.map(
        Array.groupBy(
            items.flatMap((each) => Option.toArray(Option.map(locatorOf(each.target), (at) => [at, each] as const))),
            ([at]) => at,
        ),
        (pairs) => Array.map(pairs, ([, each]) => each),
    );
const tokensOf = (items: readonly Item[]) => Record.fromEntries(items.flatMap((each) => Option.toArray(Option.map(each.token, (at) => [at, each] as const))));
const candidateOf = (each: Item) => (each.live ? [...Option.toArray(each.token), JSON.stringify(path(each).join(SEPARATOR))] : Option.toArray(Option.map(each.token, (held) => JSON.stringify(held)))).join(' ');
const index = (model: Model): Index => {
    const byRef = Record.fromIterableWith(model.containers, (container) => [`${container.ref}`, container]);
    const names = Record.getSomes(
        Record.fromIterableWith([...model.containers.map(({ locator, title }) => [locator, title] as const), ...model.containers.flatMap(({ controls }) => controls.map(({ locator, label: named }) => [locator, named?.text] as const))], ([locator, text]) => [keyOf(stepsOf(locator)), label(text)]),
    );
    const ancestry = (container: Container): readonly string[] =>
        Option.match(
            Option.flatMap(Option.fromUndefinedOr(container.parent), (parent) => Record.get(byRef, `${parent}`)),
            {
                onSome: (parent) => [...ancestry(parent), ...Option.toArray(label(parent.title))],
                onNone: () => {
                    const steps = stepsOf(container.locator);
                    return Array.getSomes(Array.dropRight(steps, 1).map((_, at) => Record.get(names, keyOf(steps.slice(0, at + 1)))));
                },
            },
        );
    const shown = model.containers.map((container): Shown => {
        const held = item(Target.Container({ container }), ancestry(container));
        return {
            container,
            item: held,
            controls: container.controls.map((control) => {
                const owner = item(Target.Control({ control, container }), [...path(held), ...Array.fromNullishOr(control.section)]);
                return { control, item: owner, options: (control.options ?? []).map((option) => ({ option, item: item(Target.Option({ option }), path(owner)) })) };
            }),
        };
    });
    const refs = shown.flatMap(({ container, item: held, controls }) => [[`${container.ref}`, held] as const, ...controls.flatMap(({ control, item: owner, options }) => [[`${control.ref}`, owner] as const, ...options.map(({ option, item: each }) => [`${option.ref}`, each] as const)])]);
    const interned = Record.fromEntries(refs);
    const unread = shown.flatMap(({ controls }) => controls.flatMap(({ control, item: owner }) => (control.opens === undefined || Record.has(interned, `${control.opens}`) ? [] : [[`${control.opens}`, { ...owner, token: Option.some(token('menu', control.opens)) }] as const])));
    const items = refs.map(([, each]) => each);
    return {
        model,
        items,
        shown,
        tokens: tokensOf([...items, ...unread.map(([, each]) => each)]),
        live: { ...Record.fromEntries(unread), ...interned },
        located: located(items),
        remembered: { build: Option.none(), containers: [], controls: {} },
    };
};
const opened = (map: InterfaceMap, locator: string, seen: HashSet.HashSet<string>): readonly string[] =>
    Option.getOrElse(
        Option.gen(function* () {
            const { container } = yield* Record.get(map.containers, locator);
            const from = yield* Array.findFirst(Array.sort(container.openers, BY_FIRST), ({ container: at }) => !HashSet.has(seen, at));
            const held = yield* Record.get(map.containers, from.container);
            const control = yield* Record.get(held.controls, from.control);
            return [...opened(map, from.container, HashSet.add(seen, from.container)), ...Option.toArray(Option.flatMap(held.container.title, label)), ...Option.toArray(control.section), ...Option.toArray(Option.flatMap(control.label, ({ text }) => label(text)))];
        }),
        Array.empty<string>,
    );
const remember = (held: Index, map: InterfaceMap): Index => {
    const listed = Array.sort(
        Record.toEntries(map.containers),
        Order.mapInput(BY_FIRST, ([, { container }]: readonly [string, Entry]) => container),
    ).flatMap(([locator, entry]) => {
        const container = item(Target.RememberedContainer({ locator, entry }), opened(map, locator, HashSet.make(locator)));
        const controls = Array.sort(
            Record.toEntries(Record.filter(entry.controls, ({ retired }) => Option.isNone(retired))),
            Order.mapInput(BY_FIRST, ([, control]: readonly [string, Entry['controls'][string]]) => control),
        );
        return [container, ...controls.map(([at, control]) => item(Target.RememberedControl({ locator: at, container: locator, control, entry }), [...path(container), ...Option.toArray(control.section)]))];
    });
    const [, numbered] = Array.mapAccum(listed, HashMap.empty<Option.Option<string>, number>(), (counts, each) => {
        const count = Option.getOrElse(HashMap.get(counts, each.token), () => 0) + 1;
        return [HashMap.set(counts, each.token, count), { ...each, token: Option.map(each.token, (plain) => (count === 1 ? plain : `${plain} #${count}`)) }] as const;
    });
    const [aliases, remembered] = Array.partition(numbered, (each) =>
        Option.match(
            Option.flatMap(locatorOf(each.target), (at) => Record.get(held.located, at)),
            {
                onNone: () => Result.fail(each),
                onSome: (found) =>
                    Result.succeed(
                        Option.filter(
                            Option.map(each.token, (at) => [at, Array.headNonEmpty(found)] as const),
                            () => Predicate.isTupleOf(found, 1),
                        ),
                    ),
            },
        ),
    );
    const [controls, containers] = Array.partition(remembered, (each) => (each.target._tag === 'RememberedControl' ? Result.succeed([each.target.container, each] as const) : Result.fail(each)));
    return {
        ...held,
        items: [...held.items, ...remembered],
        tokens: { ...held.tokens, ...Record.fromEntries(Array.getSomes(aliases)), ...tokensOf(remembered) },
        located: { ...held.located, ...located(remembered) },
        remembered: {
            build: Array.head(map.application.builds),
            containers,
            controls: Record.map(
                Array.groupBy(controls, ([container]) => container),
                (pairs) => pairs.map(([, each]) => each),
            ),
        },
    };
};
const recall = Function.memoize((held: Index) => Function.memoize((map: InterfaceMap) => remember(held, map)));
const locate = ({ target }: Item) =>
    Target.$match(target, {
        Container: () => Option.none(),
        Control: ({ control: held, container }) => Option.some({ container: container.locator, control: held.locator }),
        Option: () => Option.none(),
        RememberedContainer: () => Option.none(),
        RememberedControl: ({ locator, container }) => Option.some({ container, control: locator }),
    });
const shell = (held: Index, locator: string) =>
    Option.flatMap(Record.get(held.located, locator), (found) =>
        Array.match(
            found.flatMap(({ target }) => (target._tag === 'Container' ? [target.container] : [])),
            { onEmpty: Option.none, onNonEmpty: Option.some },
        ),
    );
const menu = (target: Target) => Target.$match(target, { Container: () => false, Control: ({ container }) => container.kind === 'menu', Option: () => false, RememberedContainer: ({ entry }) => entry.container.kind === 'menu', RememberedControl: ({ entry }) => entry.container.kind === 'menu' });
const opener = (held: Index, { target }: Item, seen: HashSet.HashSet<string> = HashSet.empty()) =>
    Match.value(target).pipe(
        Match.tag('RememberedContainer', 'RememberedControl', (at) => at),
        Match.option,
        Option.filter((at) => Option.isNone(shell(held, at._tag === 'RememberedControl' ? at.container : at.locator))),
        Option.flatMap(({ entry }) => Array.findFirst(entry.container.openers, ({ control }) => (HashSet.has(seen, control) ? Option.none() : Record.get(held.located, control)))),
    );
const opening = (held: Index, wanted: Item): Result.Result<readonly Item[], string> => {
    const links = Array.reverse(
        Array.unfold({ at: wanted, seen: HashSet.empty<string>() }, ({ at, seen }) => Option.map(opener(held, at, seen), (found) => [found, { at: Array.headNonEmpty(found), seen: HashSet.union(seen, HashSet.fromIterable(Option.toArray(locatorOf(Array.headNonEmpty(found).target)))) }] as const)),
    );
    const chain = links.map(Array.headNonEmpty);
    const root = Array.headNonEmpty(Array.append(chain, wanted));
    const named = JSON.stringify(path(wanted).join(SEPARATOR));
    const alike = (count: number, kind: string, tokens: readonly string[]) => Result.fail(`${named} is reached through ${count} open ${kind}s alike (${tokens.join(', ')}); act on the one meant by its reference, then act again`);
    const unreachable = ({ container }: Entry) =>
        Result.fail(`${named} sits in a closed ${container.kind} that nothing shown now opens, as ${Array.match(container.openers, { onEmpty: () => 'the map never saw what opened it', onNonEmpty: (openers) => `none of the ${openers.length} items the map saw open it is shown` })}; open it, then act again`);
    return Match.value({ target: root.target, doubled: Array.findFirst(links, (found) => found.length > 1) }).pipe(
        Match.when({ doubled: Option.isSome }, ({ doubled }) => alike(doubled.value.length, 'item', Array.getSomes(doubled.value.map(Struct.get('token'))))),
        Match.when(
            () => root.live || menu(root.target),
            () => Result.succeed(chain.filter((_, at) => !Option.exists(Array.get([...chain, wanted], at + 1), ({ target }) => menu(target)))),
        ),
        Match.when({ target: { _tag: 'RememberedControl' } }, ({ target }) =>
            Option.match(shell(held, target.container), {
                onNone: () => unreachable(target.entry),
                onSome: (open) => {
                    const { kind, title, truncated } = Array.headNonEmpty(open);
                    const shown = [kind, ...Option.toArray(Option.map(label(title), (text) => JSON.stringify(text)))].join(' ');
                    return Match.value({ count: open.length, truncated }).pipe(
                        Match.when({ count: (count) => count > 1 }, () =>
                            alike(
                                open.length,
                                kind,
                                open.map(({ kind: each, ref }) => token(each, ref)),
                            ),
                        ),
                        Match.when({ truncated: true }, () => Result.succeed([])),
                        Match.orElse(() => Result.fail(`${named} is missing from the open ${shown}, read whole now, though the map saw it there ${target.control.tally.seen}×`)),
                    );
                },
            }),
        ),
        Match.when({ target: { _tag: 'RememberedContainer' } }, ({ target }) => unreachable(target.entry)),
        Match.orElse(() => Result.succeed([])),
    );
};
const resolve = (held: Index, at: string): Result.Result<Item, Unmatched> => {
    const refused = (subject: string, candidates: readonly Item[]) => Result.fail(Unmatched.of({ subject, query: Option.some(at), candidates: candidates.map(candidateOf) }));
    const [living, recalled] = Array.partition(held.items, (each) => (each.live ? Result.succeed(each) : Result.fail(each)));
    const named = held.items.filter(({ name }) => Option.isSome(name));
    return pipe(
        Record.get(held.tokens, at),
        Option.map((exact): Result.Result<Item, Unmatched> => Result.succeed(exact)),
        Option.orElse(() => Option.map(Option.fromNullishOr(REFERENCE.exec(at)?.groups?.['ref']), (ref) => refused('reference in this read', Option.match(Record.get(held.live, ref), { onSome: Array.of, onNone: () => held.shown.map(Struct.get('item')) })))),
        Option.getOrElse(() =>
            Array.matchRight(segmentsOf(at), {
                onEmpty: () => refused('label path', named),
                onNonEmpty: (init, last) => {
                    const inside = within(init);
                    const tests = [(name: string) => Unmatched.named(name, last), loose(last)];
                    const found = Array.findFirst(Array.cartesian([living, recalled], tests), ([tier, test]) =>
                        Array.match(
                            tier.filter((each) => Option.exists(each.name, test) && inside(each)),
                            { onEmpty: Option.none, onNonEmpty: Option.some },
                        ),
                    );
                    return Option.match(found, {
                        onNone: () =>
                            refused(
                                'control, option, or container',
                                Unmatched.rank(last, named, (each) => Option.getOrElse(each.name, () => '')),
                            ),
                        onSome: (candidates) => (Predicate.isTupleOf(candidates, 1) ? Result.succeed(candidates[0]) : refused('single control, option, or container', candidates)),
                    });
                },
            }),
        ),
    );
};
const find = (held: Index, query: string, kind: Option.Option<Control['kind']>) => {
    const segments = segmentsOf(query);
    const inside = within(Array.dropRight(segments, 1));
    const kept = held.items.filter((each) => inside(each) && Option.match(kind, { onNone: () => Option.isSome(each.name) || Option.isSome(each.value), onSome: (wanted) => Option.contains(each.kind, wanted) }));
    return Array.sortWith(
        Unmatched.rank(
            Option.getOrElse(Array.last(segments), () => ''),
            kept,
            (each) =>
                [
                    ...Option.toArray(each.name),
                    ...Option.toArray(each.value),
                    ...Match.value(each.target).pipe(
                        Match.tag('Container', ({ container }) => Array.fromNullishOr(container.help)),
                        Match.tag('Control', ({ control }) => [...Array.fromNullishOr(control.help), ...(control.labels ?? [])]),
                        Match.tag('Option', ({ option }) => Array.fromNullishOr(option.help)),
                        Match.tag('RememberedControl', ({ control }) => [...Option.toArray(control.help), ...Option.getOrElse(control.labels, Array.empty)]),
                        Match.orElse(Array.empty<string>),
                    ),
                ].join(' '),
        ),
        Struct.get('live'),
        Order.flip(Order.Boolean),
    );
};
const Address = { At, Target, index, recall, opener, opening, locate, resolve, find, path, token, fact: printValue, value: printStored, separator: SEPARATOR } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Address, type Index, type Item, type Shown };
