import { Unmatched } from '@rasm/search/unmatched';
import { Array, Data, HashMap, HashSet, Match, Option, Order, pipe, Record, Result, Schema, String, Struct } from 'effect';
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
    readonly located: Readonly<Record<string, Item>>;
    readonly remembered: { readonly build: Option.Option<string>; readonly containers: readonly Item[]; readonly controls: Readonly<Record<string, readonly Item[]>> };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SEPARATOR = ' > ';
const STATES = ['off', 'on', 'mixed'] as const;
const WORDS = new Intl.Segmenter(undefined, { granularity: 'word' });

// --- [MODELS] --------------------------------------------------------------------------

const Target = Data.taggedEnum<Target>();
const At = Schema.NonEmptyString.annotate({
    description:
        'A reference as results print it, @ with a kind letter and number for what is shown (@d3) or the path of what is remembered (Export > Units, numbered #2 where paths repeat), else a label path from the outer container in, labels joined by > (New Document > Units), each label matched ignoring case, accents, and punctuation or by a run of the words it holds, shown items first',
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
        record: ({ fields }) =>
            `{${Object.entries(fields)
                .map(([field, each]) => `${field}: ${printFact(each)}`)
                .join(', ')}}`,
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
const stepsOf = (locator: string) => Option.getOrElse(Schema.decodeOption(Steps)(locator), Array.empty);
const words = (text: string) =>
    Iterator.from(WORDS.segment(text))
        .filter(({ isWordLike }) => isWordLike === true)
        .map(({ segment }) => segment)
        .toArray();
const holdsWords = (name: string, segment: string) => {
    const wanted = words(segment);
    return Array.window(words(name), wanted.length).some((run) => Array.makeEquivalence(Unmatched.named)(run, wanted));
};
const matches = (segments: Array.NonEmptyReadonlyArray<string>, compare: (name: string, segment: string) => boolean) => (item: Item) =>
    Option.exists(item.name, (name) => compare(name, Array.lastNonEmpty(segments))) &&
    Array.isReadonlyArrayEmpty(item.ancestors.reduce((left, held) => (Option.exists(Array.head(left), (segment) => Unmatched.named(held, segment) || holdsWords(held, segment)) ? Array.drop(left, 1) : left), Array.initNonEmpty(segments)));
const locatorOf = (target: Target) =>
    Target.$match(target, { Container: ({ container }) => Option.some(container.locator), Control: ({ control }) => Option.some(control.locator), Option: () => Option.none(), RememberedContainer: ({ locator }) => Option.some(locator), RememberedControl: ({ locator }) => Option.some(locator) });
const located = (items: readonly Item[]) => Record.fromEntries(items.flatMap((item) => Option.toArray(Option.map(locatorOf(item.target), (locator) => [locator, item] as const))));
const candidateOf = (item: Item) => (item.live ? [...Option.toArray(item.token), JSON.stringify(path(item).join(SEPARATOR))] : Option.toArray(Option.map(item.token, (held) => JSON.stringify(held)))).join(' ');
const index = (model: Model): Index => {
    const parsed = model.containers.map((container) => ({ container, steps: stepsOf(container.locator) }));
    const names = Record.fromEntries(
        parsed.flatMap(({ container, steps }) => [[steps, container.title] as const, ...container.controls.map((control) => [stepsOf(control.locator), control.label?.text] as const)]).flatMap(([steps, text]) => Option.toArray(Option.map(label(text), (name) => [JSON.stringify(steps), name] as const))),
    );
    const shown = parsed.map(({ container, steps }): Shown => {
        const item: Item = {
            target: Target.Container({ container }),
            token: Option.some(token(container.kind, container.ref)),
            name: label(container.title),
            ancestors: Array.getSomes(Array.dropRight(steps, 1).map((_, at) => Record.get(names, JSON.stringify(steps.slice(0, at + 1))))),
            value: Option.none(),
            kind: Option.none(),
            live: true,
        };
        return {
            container,
            item,
            controls: container.controls.map((control) => {
                const owner: Item = {
                    target: Target.Control({ control, container }),
                    token: Option.some(token(control.kind, control.ref)),
                    name: label(control.label?.text),
                    ancestors: [...path(item), ...Array.fromNullishOr(control.section)],
                    value: Option.map(Option.fromUndefinedOr(control.value), (fact) => printValue(control.kind, fact)),
                    kind: Option.some(control.kind),
                    live: true,
                };
                return { control, item: owner, options: (control.options ?? []).map((option) => ({ option, item: { target: Target.Option({ option }), token: Option.some(token('option', option.ref)), name: label(option.label), ancestors: path(owner), value: Option.none(), kind: Option.none(), live: true } })) };
            }),
        };
    });
    const refs = shown.flatMap(({ container, item, controls }) => [[`${container.ref}`, item] as const, ...controls.flatMap(({ control, item: owner, options }) => [[`${control.ref}`, owner] as const, ...options.map(({ option, item: each }) => [`${option.ref}`, each] as const)])]);
    const interned = Record.fromEntries(refs);
    const unread = shown.flatMap(({ controls }) => controls.flatMap(({ control, item }) => (control.opens === undefined || Record.has(interned, `${control.opens}`) ? [] : [[`${control.opens}`, { ...item, token: Option.some(token('menu', control.opens)) }] as const])));
    const live = refs.map(([, item]) => item);
    return {
        model,
        items: live,
        shown,
        tokens: Record.fromEntries([...live, ...unread.map(([, item]) => item)].flatMap((item) => Option.toArray(Option.map(item.token, (at) => [at, item] as const)))),
        live: { ...Record.fromEntries(unread), ...interned },
        located: located(live),
        remembered: { build: Option.none(), containers: [], controls: {} },
    };
};
const recall = (held: Index, map: InterfaceMap): Index => {
    const [, groups] = Array.mapAccum(Record.toEntries(map.containers), HashMap.empty<Option.Option<string>, number>(), (seen, [locator, entry]) => {
        const open = Option.map(
            Option.filter(Record.get(held.located, locator), ({ target }) => target._tag === 'Container'),
            path,
        );
        const opener = pipe(
            Array.head(entry.container.openers),
            Option.flatMap(({ container, control }) =>
                Option.orElse(Option.map(Record.get(held.located, control), path), () =>
                    pipe(Record.get(map.containers, container), Option.map(Struct.get('controls')), Option.flatMap(Record.get(control)), Option.flatMap(Struct.get('label')), Option.map(Struct.get('text')), Option.flatMap(label), Option.map(Array.of)),
                ),
            ),
            Option.getOrElse(Array.empty<string>),
        );
        const whole = [...opener, ...Option.toArray(Option.flatMap(entry.container.title, label))];
        const controls = (ancestors: readonly string[]) =>
            Record.toEntries(entry.controls)
                .filter(([at, control]) => Option.isNone(control.retired) && !Record.has(held.located, at))
                .map(([at, control]): Item => {
                    const name = Option.flatMap(control.label, ({ text }) => label(text));
                    const within = [...ancestors, ...Option.toArray(control.section)];
                    return {
                        target: Target.RememberedControl({ locator: at, container: locator, control, entry }),
                        token: Option.map(name, (named) => [...within, named].join(SEPARATOR)),
                        name,
                        ancestors: within,
                        value: Option.map(control.value, (was) => printStored(control.kind, was)),
                        kind: Option.some(control.kind),
                        live: false,
                    };
                });
        const items = Option.match(open, {
            onSome: controls,
            onNone: () =>
                Array.matchRight(whole, {
                    onEmpty: Array.empty,
                    onNonEmpty: (ancestors, name): readonly Item[] => [{ target: Target.RememberedContainer({ locator, entry }), token: Option.some(whole.join(SEPARATOR)), name: Option.some(name), ancestors, value: Option.none(), kind: Option.none(), live: false }, ...controls(whole)],
                }),
        });
        const [counted, numbered] = Array.mapAccum(items, seen, (counts, item) => {
            const count = Option.getOrElse(HashMap.get(counts, item.token), () => 0) + 1;
            return [HashMap.set(counts, item.token, count), { ...item, token: Option.map(item.token, (numberless) => (count === 1 ? numberless : `${numberless} #${count}`)) }] as const;
        });
        return [counted, { locator, items: numbered }] as const;
    });
    const remembered = groups.flatMap(Struct.get('items'));
    return {
        ...held,
        items: [...held.items, ...remembered],
        tokens: { ...held.tokens, ...Record.fromEntries(remembered.flatMap((item) => Option.toArray(Option.map(item.token, (at) => [at, item] as const)))) },
        located: { ...held.located, ...located(remembered) },
        remembered: {
            build: Array.head(map.application.builds),
            containers: remembered.filter(({ target }) => target._tag === 'RememberedContainer'),
            controls: Record.fromEntries(groups.map(({ locator, items }) => [locator, items.filter(({ target }) => target._tag === 'RememberedControl')] as const)),
        },
    };
};
const locate = ({ target }: Item) =>
    Target.$match(target, {
        Container: () => Option.none(),
        Control: ({ control: held, container }) => Option.some({ container: container.locator, control: held.locator }),
        Option: () => Option.none(),
        RememberedContainer: () => Option.none(),
        RememberedControl: ({ locator, container }) => Option.some({ container, control: locator }),
    });
const shell = (held: Index, locator: string) => Option.filter(Option.map(Record.get(held.located, locator), Struct.get('target')), Target.$is('Container'));
const opener = (held: Index, { target }: Item, seen: HashSet.HashSet<string> = HashSet.empty()) =>
    Match.value(target).pipe(
        Match.tag('RememberedContainer', 'RememberedControl', (at) => at),
        Match.option,
        Option.filter((at) => Option.isNone(shell(held, at._tag === 'RememberedControl' ? at.container : at.locator))),
        Option.flatMap(({ entry }) => Array.findFirst(entry.container.openers, ({ control }) => (HashSet.has(seen, control) ? Option.none() : Record.get(held.located, control)))),
    );
const opening = (held: Index, item: Item): Result.Result<readonly Item[], string> => {
    const chain = Array.reverse(Array.unfold({ at: item, seen: HashSet.empty<string>() }, ({ at, seen }) => Option.map(opener(held, at, seen), (found) => [found, { at: found, seen: HashSet.union(seen, HashSet.fromIterable(Option.toArray(locatorOf(found.target)))) }] as const)));
    const menu = ({ target }: Item) => (target._tag === 'Control' && target.container.kind === 'menu') || ((target._tag === 'RememberedControl' || target._tag === 'RememberedContainer') && target.entry.container.kind === 'menu');
    const root = Array.headNonEmpty(Array.append(chain, item));
    const named = JSON.stringify(path(item).join(SEPARATOR));
    const unreachable = ({ container }: Entry) =>
        Result.fail(`${named} sits in a closed ${container.kind} that nothing shown now opens, as ${Array.match(container.openers, { onEmpty: () => 'the map never saw what opened it', onNonEmpty: (openers) => `none of the ${openers.length} items the map saw open it is shown` })}; open it, then act again`);
    return root.live || menu(root)
        ? Result.succeed(chain.filter((_, at) => !Option.exists(Array.get([...chain, item], at + 1), menu)))
        : Match.value(root.target).pipe(
              Match.tag('RememberedControl', (at) =>
                  Option.match(shell(held, at.container), {
                      onNone: () => unreachable(at.entry),
                      onSome: ({ container }) =>
                          container.truncated ? Result.succeed([]) : Result.fail(`${named} is missing from the open ${container.kind}${Option.match(label(container.title), { onNone: () => '', onSome: (title) => ` ${JSON.stringify(title)}` })}, read whole now, though the map saw it there ${at.control.tally.seen}×`),
                  }),
              ),
              Match.tag('RememberedContainer', ({ entry }) => unreachable(entry)),
              Match.orElse(() => Result.succeed([])),
          );
};
const resolve = (held: Index, at: string): Result.Result<Item, Unmatched> => {
    const segments = Array.map(String.split(at, SEPARATOR.trim()), String.trim);
    const [living, recalled] = Array.partition(held.items, (item) => (item.live ? Result.succeed(item) : Result.fail(item)));
    const refused = (subject: string, candidates: readonly Item[]) => Result.fail(Unmatched.of({ subject, query: Option.some(at), candidates: candidates.map(candidateOf) }));
    const found = Array.findFirst([living, recalled], (tier) => Array.findFirst([Unmatched.named, holdsWords], (compare) => Array.match(tier.filter(matches(segments, compare)), { onEmpty: Option.none, onNonEmpty: Option.some })));
    return Option.match(Record.get(held.tokens, at), {
        onSome: Result.succeed,
        onNone: () =>
            at.startsWith('@')
                ? refused('reference in the current model', living)
                : Option.match(found, {
                      onNone: () =>
                          refused(
                              'control, option, or container',
                              held.items.filter(({ name }) => Option.isSome(name)),
                          ),
                      onSome: (candidates) => (candidates.length === 1 ? Result.succeed(Array.headNonEmpty(candidates)) : refused('single control, option, or container', candidates)),
                  }),
    });
};
const find = (held: Index, query: string, kind: Option.Option<Control['kind']>) =>
    Array.sortWith(
        Unmatched.rank(
            query,
            held.items.filter((item) => Option.match(kind, { onNone: () => Option.isSome(item.name) || Option.isSome(item.value), onSome: (wanted) => Option.contains(item.kind, wanted) })),
            (item) =>
                [
                    ...path(item),
                    ...Option.toArray(item.value),
                    ...Match.value(item.target).pipe(
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
const Address = { At, Target, index, recall, opener, opening, locate, resolve, find, path, token, fact: printValue, value: printStored, separator: SEPARATOR } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Address, type Index, type Item, type Shown };
