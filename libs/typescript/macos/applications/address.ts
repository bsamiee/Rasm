import { Unmatched } from '@rasm/search';
import { Array, Data, HashMap, Match, Option, Order, pipe, Record, Result, Schema, String, Struct } from 'effect';
import type { Container, Control, Fact, Model, Observation } from '#/contract';
import type { Entry, InterfaceMap } from '#/interface-map';

// --- [TYPES] ---------------------------------------------------------------------------

type Target = Data.TaggedEnum<{
    Container: { readonly container: Container };
    Control: { readonly control: Control; readonly container: Container };
    Option: { readonly option: NonNullable<Control['options']>[number] };
    RememberedContainer: { readonly locator: string; readonly entry: Entry };
    RememberedControl: { readonly locator: string; readonly control: Entry['controls'][string]; readonly entry: Entry };
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
    readonly remembered: { readonly containers: readonly Item[]; readonly controls: Readonly<Record<string, readonly Item[]>> };
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
    Match.when({ bool: Match.defined }, ({ bool }) => (bool.value ? 'on' : 'off')),
    Match.when({ integer: Match.defined }, ({ integer }) => `${integer.value}`),
    Match.when({ real: Match.defined }, ({ real }) => `${real.value}`),
    Match.when({ text: Match.defined }, ({ text }) => JSON.stringify(text.value)),
    Match.when({ date: Match.defined }, ({ date }) => date.value),
    Match.when({ url: Match.defined }, ({ url }) => url.value),
    Match.when({ point: Match.defined }, ({ point }) => point.value.join(',')),
    Match.when({ size: Match.defined }, ({ size }) => size.value.join('×')),
    Match.when({ rect: Match.defined }, ({ rect }) => `${rect.value[0].join(',')} ${rect.value[1].join('×')}`),
    Match.when({ range: Match.defined }, ({ range }) => `${range.location}+${range.length}`),
    Match.when({ element: Match.defined }, ({ element }) => `element ${element.id}`),
    Match.when({ refused: Match.defined }, ({ refused }) => `unread, AXError ${refused.error}`),
    Match.when({ list: Match.defined }, ({ list }) => `[${list.values.map((each) => printFact(each)).join(', ')}]`),
    Match.exhaustive,
);
const printValue = (kind: Control['kind'], fact: Fact) => (kind === 'toggle' && 'integer' in fact ? (STATES[fact.integer.value] ?? printFact(fact)) : printFact(fact));
const printStored = (kind: Control['kind'], value: Observation['values'][string]) =>
    printValue(
        kind,
        Match.value(value).pipe(
            Match.when(Match.boolean, (bool): Fact => ({ bool: { value: bool } })),
            Match.when(Match.number, (number): Fact => (Number.isInteger(number) ? { integer: { value: number } } : { real: { value: number } })),
            Match.orElse((text): Fact => ({ text: { value: text } })),
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
const index = (model: Model, map: InterfaceMap): Index => {
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
                    ancestors: path(item),
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
    const opened = located(live);
    const [, groups] = Array.mapAccum(Record.toEntries(map.containers), HashMap.empty<Option.Option<string>, number>(), (seen, [locator, entry]) => {
        const open = Option.map(
            Option.filter(Record.get(opened, locator), ({ target }) => target._tag === 'Container'),
            path,
        );
        const opener = pipe(
            Array.head(entry.container.openers),
            Option.flatMap(({ container, control }) =>
                Option.orElse(Option.map(Record.get(opened, control), path), () => pipe(Record.get(map.containers, container), Option.map(Struct.get('controls')), Option.flatMap(Record.get(control)), Option.flatMap(Struct.get('label')), Option.map(Struct.get('text')), Option.flatMap(label), Option.map(Array.of))),
            ),
            Option.getOrElse(Array.empty<string>),
        );
        const whole = [...opener, ...Option.toArray(Option.flatMap(entry.container.title, label))];
        const controls = (ancestors: readonly string[]) =>
            Record.toEntries(entry.controls)
                .filter(([at, control]) => Option.isNone(control.retired) && !Record.has(opened, at))
                .map(([at, control]): Item => {
                    const name = Option.flatMap(control.label, ({ text }) => label(text));
                    return { target: Target.RememberedControl({ locator: at, control, entry }), token: Option.map(name, (held) => [...ancestors, held].join(SEPARATOR)), name, ancestors, value: Option.map(control.value, (held) => printStored(control.kind, held)), kind: Option.some(control.kind), live: false };
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
            return [HashMap.set(counts, item.token, count), { ...item, token: Option.map(item.token, (held) => (count === 1 ? held : `${held} #${count}`)) }] as const;
        });
        return [counted, { locator, items: numbered }] as const;
    });
    const remembered = groups.flatMap(Struct.get('items'));
    const items = [...live, ...remembered];
    return {
        model,
        items,
        shown,
        tokens: Record.fromEntries([...items, ...unread.map(([, item]) => item)].flatMap((item) => Option.toArray(Option.map(item.token, (at) => [at, item] as const)))),
        live: { ...Record.fromEntries(unread), ...interned },
        located: { ...opened, ...located(remembered) },
        remembered: { containers: remembered.filter(({ target }) => target._tag === 'RememberedContainer'), controls: Record.fromEntries(groups.map(({ locator, items: held }) => [locator, held.filter(({ target }) => target._tag === 'RememberedControl')] as const)) },
    };
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
                    ...Option.toArray(
                        Match.value(item.target).pipe(
                            Match.tag('Control', ({ control }) => Option.fromUndefinedOr(control.help)),
                            Match.tag('RememberedControl', ({ control }) => control.help),
                            Match.orElse(() => Option.none()),
                        ),
                    ),
                ].join(' '),
        ),
        Struct.get('live'),
        Order.flip(Order.Boolean),
    );
const Address = { At, Target, index, resolve, find, path, token, fact: printValue, value: printStored, separator: SEPARATOR } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Address, type Index, type Item, type Shown };
