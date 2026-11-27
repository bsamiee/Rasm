import type { Entry } from '@rasm/applications/catalog';
import { type Declaration, Description, type Source as Host } from '@rasm/applications/description';
import { InterfaceMap } from '@rasm/applications/map';
import { ApplicationServer } from '@rasm/applications/server/client';
import { Element } from '@rasm/applications/server/element';
import { ApplicationFailure } from '@rasm/applications/server/failure';
import type { Instance } from '@rasm/applications/server/instance';
import { Format } from '@rasm/mcp/report';
import { Handle } from '@rasm/mcp/server';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Cause, Context, Data, DateTime, Effect, Function, HashMap, HashSet, Match, Option, Order, pipe, Predicate, Record, Result, Schema, String, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Container = typeof Element.Container.Type;
type Control = typeof Element.Control.Type;
type Fact = typeof Element.Fact.Type;
type Model = typeof Element.Model.Type;
type Linked = Pick<Host<Error, never, never, never, never>, 'describe' | 'operate'>;
type Target = Data.TaggedEnum<{
    Container: { readonly container: Container };
    Control: { readonly control: Control; readonly container: Container };
    Option: { readonly option: NonNullable<(typeof Element.Control.Type)['options']>[number] };
    RememberedContainer: { readonly locator: string; readonly entry: InterfaceMap['containers'][string] };
    RememberedControl: { readonly locator: string; readonly container: string; readonly control: InterfaceMap['containers'][string]['controls'][string]; readonly entry: InterfaceMap['containers'][string] };
}>;
interface Aimed {
    readonly row: Entry;
    readonly live: Pick<Instance, 'instance' | 'identifier'> & { readonly language: string };
}
interface Item {
    readonly target: Target;
    readonly token: Option.Option<string>;
    readonly name: Option.Option<string>;
    readonly ancestors: readonly string[];
    readonly value: Option.Option<string>;
    readonly kind: Option.Option<(typeof Element.Control.Type)['kind']>;
    readonly live: boolean;
}
interface Shown {
    readonly container: Container;
    readonly item: Item;
    readonly controls: readonly { readonly control: Control; readonly item: Item; readonly options: readonly { readonly option: NonNullable<(typeof Element.Control.Type)['options']>[number]; readonly item: Item }[] }[];
}
interface Index {
    readonly model: Model;
    readonly items: readonly Item[];
    readonly shown: readonly Shown[];
    readonly tokens: Readonly<Record<string, Item>>;
    readonly live: Readonly<Record<string, Item>>;
    readonly located: Readonly<Record<string, Array.NonEmptyReadonlyArray<Item>>>;
    readonly remembered: { readonly build: Option.Option<string>; readonly containers: readonly Item[]; readonly controls: Readonly<Record<string, readonly Item[]>> };
    readonly reads: Readonly<Record<string, { readonly sequence: number; readonly whole: boolean }>>;
    readonly left: Readonly<Record<string, { readonly container: Pick<Container, 'ref' | 'locator'>; readonly locator: string }>>;
    readonly closed: Readonly<Record<string, { readonly container: Container; readonly facts: readonly string[] }>>;
    readonly routes: Readonly<Record<string, Description['containers'][number]['locator']>>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SEPARATOR = ' > ';
const STATES = ['off', 'on', 'mixed'] as const;
const WORDS = new Intl.Segmenter(undefined, { granularity: 'word' });
const RUN = Array.makeEquivalence(Unmatched.named);
const REFERENCE = /^@\p{L}(?<ref>\p{Nd}+)$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Target = Data.taggedEnum<Target>();
const At = Schema.NonEmptyString.annotate({
    description: 'Reference a result printed (@a12), path find prints for a remembered item (File > Export… > Units), or labels from the outer container in joined by > (New Document > Units), matched ignoring case, accents, and punctuation',
});
const Steps = Schema.Json.pipe(Schema.Array, Schema.fromJsonString);

// --- [SERVICES] ------------------------------------------------------------------------

const Source = Context.Reference<(row: Entry) => Effect.Effect<Linked, Error>>('@rasm/application-tools/handle/address/Source', { defaultValue: () => () => new Cause.NoSuchElementError() });
const Toolbar = Context.Reference<(row: Entry) => Effect.Effect<Declaration, Error>>('@rasm/application-tools/handle/address/Toolbar', { defaultValue: () => () => Effect.succeed({ tools: HashMap.empty(), toolbar: [] }) });
const Indexed = Context.Reference<Option.Option<{ readonly instance: Instance['instance']; readonly index: Index }>>('@rasm/application-tools/handle/address/Indexed', { defaultValue: Option.none });

// --- [OPERATIONS] ----------------------------------------------------------------------

const label = (text: string | undefined) =>
    Option.fromUndefinedOr(text).pipe(
        Option.map((shown) => shown.replaceAll(/\s+/gu, ' ').trim()),
        Option.filter(String.isNonEmpty),
    );
const measured = (values: readonly number[], by: string) => values.map((each) => Format.count(each)).join(by);
const printFact: (fact: Fact) => string = Match.type<Fact>().pipe(
    Match.tagsExhaustive({
        bool: ({ value }) => (value ? 'on' : 'off'),
        integer: ({ value }) => `${value}`,
        real: ({ value }) => Format.count(value),
        text: ({ value }) => `"${value}"`,
        date: ({ value }) => value,
        url: ({ value }) => value,
        point: ({ value }) => measured(value, ','),
        size: ({ value }) => measured(value, '×'),
        rect: ({ value }) => `${measured(value[0], ',')} ${measured(value[1], '×')}`,
        color: ({ red, green, blue, alpha }) => `sRGB ${measured([red, green, blue, alpha], ' ')}`,
        range: ({ location, length }) => `${location}+${length}`,
        element: ({ id }) => `element ${id}`,
        refused: ({ error }) => `unread, AXError ${error}`,
        list: ({ values }) => `[${values.map((each) => printFact(each)).join(', ')}]`,
        record: ({ fields }) => `{${Record.collect(fields, (field, each) => `${field}: ${printFact(each)}`).join(', ')}}`,
    }),
);
const printValue = (kind: (typeof Element.Control.Type)['kind'], fact: Fact) => (kind === 'toggle' && fact._tag === 'integer' ? (STATES[fact.value] ?? printFact(fact)) : printFact(fact));
const printStored = (kind: (typeof Element.Control.Type)['kind'], value: (typeof Element.Observation.Type)['values'][string]) =>
    Match.value(value).pipe(
        Match.when(Match.boolean, (on) => printFact({ _tag: 'bool', value: on })),
        Match.when(Match.number, (amount) => printValue(kind, { _tag: Number.isInteger(amount) ? 'integer' : 'real', value: amount })),
        Match.orElse((text) => printFact({ _tag: 'text', value: text })),
    );
const token = (kind: string, ref: number) => `@${kind.charAt(0)}${ref}`;
const path = (item: Item) => [...item.ancestors, ...Option.toArray(item.name)];
const keyOf = (steps: readonly Schema.Json[]) => JSON.stringify(steps);
const stepsOf = (locator: string) => Option.getOrElse(Schema.decodeOption(Steps)(locator), Array.empty);
const numbered = (at: string) => Option.fromNullishOr(REFERENCE.exec(at)?.groups?.['ref']);
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
        Container: ({ container }) => ({ ref: Option.some(token(container.kind, container.ref)), name: label(container.title), value: Option.none<string>(), kind: Option.none<(typeof Element.Control.Type)['kind']>() }),
        Control: ({ control }) => ({ ref: Option.some(token(control.kind, control.ref)), name: label(control.label?.text), value: Option.map(Option.fromUndefinedOr(control.value), (fact) => printValue(control.kind, fact)), kind: Option.some(control.kind) }),
        Option: ({ option }) => ({ ref: Option.some(token('option', option.ref)), name: label(option.label), value: Option.none(), kind: Option.none() }),
        RememberedContainer: ({ entry: { container } }) => ({ ref: Option.none(), name: Option.orElse(Option.flatMap(container.title, label), () => Option.some(container.kind)), value: Option.none(), kind: Option.none() }),
        RememberedControl: ({ control }) => ({ ref: Option.none(), name: Option.flatMap(control.label, ({ text }) => label(text)), value: Option.map(control.value, (was) => printStored(control.kind, was)), kind: Option.some(control.kind) }),
    });
    return { target, ancestors, name: read.name, value: read.value, kind: read.kind, live: Option.isSome(read.ref), token: Option.orElse(read.ref, () => Option.map(read.name, (name) => [...ancestors, name].join(SEPARATOR))) };
};
const locatorOf = (target: Target) =>
    Target.$match(target, { Container: ({ container }) => Option.some(container.locator), Control: ({ control }) => Option.some(control.locator), Option: () => Option.none(), RememberedContainer: ({ locator }) => Option.some(locator), RememberedControl: ({ locator }) => Option.some(locator) });
const located = (items: readonly Item[]) => {
    const placed = items.flatMap((each) => Option.toArray(Option.map(locatorOf(each.target), (at) => ({ at, each }))));
    const grouped = Array.groupBy(placed, Struct.get('at'));
    return Record.map(grouped, (held) => Array.map(held, Struct.get('each')));
};
const tokensOf = (items: readonly Item[]) => Record.fromEntries(items.flatMap((each) => Option.toArray(Option.map(each.token, (at) => [at, each] as const))));
const candidateOf = (each: Item) => (each.live ? [...Option.toArray(each.token), path(each).join(SEPARATOR)] : Option.toArray(each.token)).map(Format.quoted).join(' ');
const caption = (kind: (typeof Element.Container.Type)['kind'], title: Option.Option<string>, ref?: number) => [...Array.fromNullishOr(ref).map((held) => token(kind, held)), String.noCase()(kind), ...Option.toArray(title).map(Format.quoted)].join(' ');
const refuse = (reason: 'notOpen' | 'absent', subject: string, values: readonly string[]) => ApplicationFailure.make({ reason, subject, message: [subject, String.noCase()(reason), values.join(', ')].filter(String.isNonEmpty).join(' ') });
const tomb = (container: Container, locator: string) => ({ container: Struct.pick(container, ['ref', 'locator']), locator });
const own = (state: Context.Service.Shape<typeof Indexed>, of: Instance['instance']) => Option.flatMap(state, ({ instance, index: held }) => (instance === of ? Option.some(held) : Option.none()));
const index = (model: Model, carried: readonly (readonly [string, Item])[] = []): Index => {
    const byRef = Record.fromIterableWith(model.containers, (container) => [`${container.ref}`, container]);
    const names = Record.getSomes(
        Record.fromIterableWith([...model.containers.map(({ locator, title }) => [locator, title] as const), ...model.containers.flatMap(({ controls }) => controls.map(({ locator, label: named }) => [locator, named?.text] as const))], ([locator, text]) => [keyOf(stepsOf(locator)), label(text)]),
    );
    const ancestry = (container: Container): readonly string[] => {
        const [steps, above] = [stepsOf(container.locator), Option.flatMap(Option.fromUndefinedOr(container.parent), (parent) => Record.get(byRef, `${parent}`))];
        return Option.match(above, { onSome: (parent) => [...ancestry(parent), ...Option.toArray(label(parent.title))], onNone: () => Array.getSomes(Array.dropRight(steps, 1).map((_, at) => Record.get(names, keyOf(steps.slice(0, at + 1))))) });
    };
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
    const refs = [...carried, ...shown.flatMap(({ container, item: held, controls }) => [[`${container.ref}`, held] as const, ...controls.flatMap(({ control, item: owner, options }) => [[`${control.ref}`, owner] as const, ...options.map(({ option, item: each }) => [`${option.ref}`, each] as const)])])];
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
        reads: Record.fromIterableWith(model.containers, ({ ref }) => [`${ref}`, { sequence: model.sequence, whole: false }]),
        left: {},
        closed: {},
        routes: {},
    };
};
const partial = ({ truncated, controls }: Container) => truncated || controls.reduce((unlisted, { kind, total = 0 }) => unlisted + (kind === 'multi' ? total : 0) - (kind === 'row' ? 1 : 0), 0) > 0;
const settle = (
    before: Index,
    {
        stated,
        gone,
        facts,
        sequence,
        whole,
        routes,
        pointer,
    }: { readonly stated: readonly Container[]; readonly gone: readonly Container[]; readonly facts: readonly string[]; readonly sequence: number; readonly whole: boolean; readonly routes: Index['routes']; readonly pointer: (typeof Element.Model.Type)['pointer'] },
): Index => {
    const controls = Record.toEntries(before.live).flatMap(([ref, each]) => (each.target._tag === 'Control' && `${each.target.control.ref}` === ref ? [{ ref, each, locator: each.target.control.locator, sat: `${each.target.container.ref}` }] : []));
    const held = Array.groupBy(controls, Struct.get('sat'));
    const [open, shut, restated] = [new Set(stated.map(Struct.get('ref'))), new Set(gone.map(Struct.get('ref'))), new Set(stated.map(Struct.get('locator')))];
    const stay = before.model.containers.filter(({ ref }) => !open.has(ref) && !shut.has(ref));
    const versions = [...stated.map((container) => ({ container, short: partial(container) })), ...stay.map((container) => ({ container, short: true }))];
    const lacking = versions.flatMap(({ container, short }) => {
        const now = new Set(container.controls.map(({ ref }) => `${ref}`));
        return (held[`${container.ref}`] ?? []).filter(({ ref }) => !now.has(ref)).map((absent) => ({ ...absent, container, short }));
    });
    const complete = new Set(versions.flatMap(({ container, short }) => (short ? [] : [container.locator])));
    const carried = lacking.flatMap(({ ref, each, short }) => (short ? [[ref, each] as const] : []));
    const built = index({ sequence: Math.max(before.model.sequence, sequence), containers: [...stated, ...stay], ...(pointer === undefined ? {} : { pointer }) }, carried);
    const left = {
        ...Record.filter(before.left, ({ container }) => !complete.has(container.locator)),
        ...Record.fromEntries(lacking.flatMap(({ ref, locator, container, short }) => (short ? [] : [[ref, tomb(container, locator)] as const]))),
        ...Record.fromEntries(gone.flatMap((container) => [[`${container.ref}`, tomb(container, container.locator)] as const, ...(held[`${container.ref}`] ?? []).map(({ ref, locator }) => [ref, tomb(container, locator)] as const)])),
    };
    return {
        ...built,
        reads: { ...before.reads, ...Record.fromEntries([...stated, ...gone].map(({ ref }) => [`${ref}`, { sequence, whole: whole || Option.exists(Record.get(before.reads, `${ref}`), Struct.get('whole')) }] as const)) },
        left: Record.filter(left, (_, ref) => !Record.has(built.live, ref)),
        closed: { ...Record.filter(before.closed, (_, locator) => !restated.has(locator)), ...Record.fromEntries(gone.map((container) => [container.locator, { container, facts }] as const)) },
        routes: { ...before.routes, ...routes },
    };
};
const merge = (last: Option.Option<Index>, model: Model, whole: boolean, routes: Index['routes']): Index => {
    const before = Option.getOrElse(last, () => index({ sequence: model.sequence, containers: [] }));
    const newer = ({ ref }: Container) => Option.exists(Record.get(before.reads, `${ref}`), ({ sequence }) => sequence > model.sequence);
    const stated = new Set(model.containers.map(Struct.get('ref')));
    const gone = before.model.containers.filter((container) => whole && !stated.has(container.ref) && !newer(container) && Option.exists(Record.get(before.reads, `${container.ref}`), Struct.get('whole')));
    return settle(before, { stated: model.containers.filter(Predicate.not(newer)), gone, facts: [], sequence: model.sequence, whole, routes, pointer: whole && model.sequence >= before.model.sequence ? model.pointer : before.model.pointer });
};
const close = (held: Index, sequence: number, ref: (typeof Element.Container.Type)['ref'], facts: readonly string[]): Index => {
    const open = Option.flatMap(Record.get(held.live, `${ref}`), ({ target }) => (target._tag === 'Container' ? Option.some(target.container) : Option.none()));
    const reread = Option.exists(Record.get(held.reads, `${ref}`), (read) => read.sequence > sequence);
    const shut = Option.flatMap(Record.get(held.left, `${ref}`), ({ container: { locator } }) => Option.map(Record.get(held.closed, locator), (was) => ({ ...held, closed: Record.set(held.closed, locator, { ...was, facts }) })));
    return Option.match(open, {
        onSome: (container) => (reread ? held : settle(held, { stated: [], gone: [container], facts, sequence, whole: false, routes: {}, pointer: held.model.pointer })),
        onNone: () => Option.getOrElse(shut, () => held),
    });
};
const opened = (map: InterfaceMap, locator: string, seen: HashSet.HashSet<string>): readonly string[] =>
    Option.gen(function* () {
        const { container } = yield* Record.get(map.containers, locator);
        const firsts = Array.sortWith(container.openers, ({ tally }) => tally.first, DateTime.Order);
        const from = yield* Array.findFirst(firsts, ({ container: at }) => !HashSet.has(seen, at));
        const held = yield* Record.get(map.containers, from.container);
        const control = yield* Record.get(held.controls, from.control);
        return [...opened(map, from.container, HashSet.add(seen, from.container)), ...Option.toArray(Option.flatMap(held.container.title, label)), ...Option.toArray(control.section), ...Option.toArray(Option.flatMap(control.label, ({ text }) => label(text)))];
    }).pipe(Option.getOrElse(Array.empty<string>));
const remember = (held: Index, map: InterfaceMap): Index => {
    const listed = Array.sortWith(Record.toEntries(map.containers), ([, { container }]) => container.tally.first, DateTime.Order).flatMap(([locator, entry]) => {
        const container = item(Target.RememberedContainer({ locator, entry }), opened(map, locator, HashSet.make(locator)));
        const controls = Array.sortWith(Record.toEntries(entry.controls), ([, control]) => control.tally.first, DateTime.Order);
        return [container, ...controls.flatMap(([at, control]) => (Option.isSome(control.retired) ? [] : [item(Target.RememberedControl({ locator: at, container: locator, control, entry }), [...path(container), ...Option.toArray(control.section)])]))];
    });
    const [, counted] = Array.mapAccum(listed, HashMap.empty<Option.Option<string>, number>(), (counts, each) => {
        const count = Option.getOrElse(HashMap.get(counts, each.token), () => 0) + 1;
        return [HashMap.set(counts, each.token, count), { ...each, token: Option.map(each.token, (plain) => (count === 1 ? plain : `${plain} #${count}`)) }] as const;
    });
    const shownAt = ({ target }: Item) => Option.flatMap(locatorOf(target), (at) => Record.get(held.located, at));
    const [aliases, remembered] = Array.partition(counted, (each) => Option.match(shownAt(each), { onNone: () => Result.fail(each), onSome: (found) => Result.succeed(Option.all({ at: each.token, to: Option.liftPredicate(found, Predicate.isTupleOf(1)) })) }));
    const [controls, containers] = Array.partition(remembered, (each) => (each.target._tag === 'RememberedControl' ? Result.succeed({ sat: each.target.container, each }) : Result.fail(each)));
    const grouped = Array.groupBy(controls, Struct.get('sat'));
    return {
        ...held,
        items: [...held.items, ...remembered],
        tokens: { ...held.tokens, ...Record.fromEntries(Array.getSomes(aliases).map(({ at, to }) => [at, to[0]] as const)), ...tokensOf(remembered) },
        located: { ...held.located, ...located(remembered) },
        remembered: { build: Array.head(map.application.builds), containers, controls: Record.map(grouped, (under) => under.map(Struct.get('each'))) },
    };
};
const recall = Function.memoize((held: Index) => Function.memoize((map: InterfaceMap) => remember(held, map)));
const locate = ({ target }: Item) =>
    Match.value(target).pipe(
        Match.tag('Control', ({ control, container }) => ({ container: container.locator, control: control.locator })),
        Match.tag('RememberedControl', ({ locator, container }) => ({ container, control: locator })),
        Match.option,
    );
const shell = (held: Index, locator: string) => Record.get(held.located, locator).pipe(Option.map(Array.flatMap(({ target }) => (target._tag === 'Container' ? [target.container] : []))), Option.filter(Array.isReadonlyArrayNonEmpty<Container>));
const menu = (target: Target) => Target.$match(target, { Container: () => false, Control: ({ container }) => container.kind === 'menu', Option: () => false, RememberedContainer: ({ entry }) => entry.container.kind === 'menu', RememberedControl: ({ entry }) => entry.container.kind === 'menu' });
const lost = (held: Index, subject: string, locator: string, mapped: Option.Option<InterfaceMap['containers'][string]['container']>) => {
    const [open, shut] = [Option.getOrElse(shell(held, locator), Array.empty<Container>), Option.toArray(Record.get(held.closed, locator))];
    const read = [...open, ...shut.map(Struct.get('container'))].map(({ kind, title, ref }) => caption(kind, label(title), ref));
    const known = Array.isReadonlyArrayEmpty(read) ? Option.toArray(mapped).map(({ kind, title }) => caption(kind, Option.flatMap(title, label))) : read;
    return refuse(Array.isReadonlyArrayEmpty(open) ? 'notOpen' : 'absent', subject, [...known, ...shut.flatMap(Struct.get('facts'))]);
};
const opener = (held: Index, { target }: Item, seen: HashSet.HashSet<string> = HashSet.empty()) =>
    Match.value(target).pipe(
        Match.tag('RememberedContainer', 'RememberedControl', (at) => at),
        Match.option,
        Option.filter((at) => Option.isNone(shell(held, at._tag === 'RememberedControl' ? at.container : at.locator))),
        Option.flatMap(({ entry }) => Array.findFirst(entry.container.openers, ({ control }) => (HashSet.has(seen, control) ? Option.none() : Record.get(held.located, control)))),
    );
const opening = (held: Index, wanted: Item): Result.Result<readonly Item[], ApplicationFailure | Unmatched> => {
    const links = Array.reverse(
        Array.unfold({ at: wanted, seen: HashSet.empty<string>() }, ({ at, seen }) => Option.map(opener(held, at, seen), (found) => [found, { at: Array.headNonEmpty(found), seen: HashSet.union(seen, locatorOf(Array.headNonEmpty(found).target).pipe(Option.toArray, HashSet.fromIterable)) }] as const)),
    );
    const chain = links.map(Array.headNonEmpty);
    const root = Array.headNonEmpty(Array.append(chain, wanted));
    const subject = Option.getOrElse(wanted.token, () => path(wanted).join(SEPARATOR));
    const alike = (tokens: readonly string[]) => Result.fail(Unmatched.of({ subject: `single item reaching ${subject}`, candidates: tokens }));
    const sits = Match.value(root.target).pipe(
        Match.tag('RememberedControl', ({ container, entry }) => ({ locator: container, entry })),
        Match.tag('RememberedContainer', ({ locator, entry }) => ({ locator, entry })),
        Match.option,
    );
    const open = Option.flatMap(sits, ({ locator }) => shell(held, locator));
    return Match.value({ doubled: Array.findFirst(links, (found) => found.length > 1), reached: root.live || menu(root.target), several: Option.filter(open, (found) => found.length > 1), short: Option.exists(open, (found) => found.some(Struct.get('truncated'))), sits }).pipe(
        Match.when({ doubled: Option.isSome }, ({ doubled }) => alike(Array.getSomes(doubled.value.map(Struct.get('token'))))),
        Match.when({ reached: true }, () => Result.succeed(chain.filter((_, at) => !Option.exists(Array.get([...chain, wanted], at + 1), ({ target }) => menu(target))))),
        Match.when({ several: Option.isSome }, ({ several }) => alike(several.value.map(({ kind, ref }) => token(kind, ref)))),
        Match.when({ short: true }, () => Result.succeed([])),
        Match.when({ sits: Option.isSome }, ({ sits: { value } }) => Result.fail(lost(held, subject, value.locator, Option.some(value.entry.container)))),
        Match.orElse(() => Result.succeed([])),
    );
};
const matched = (held: Index, at: string): Result.Result<Item, Unmatched> => {
    const refused = (subject: string, candidates: readonly Item[]) => Result.fail(Unmatched.of({ subject, query: Option.some(at), candidates: candidates.map(candidateOf) }));
    const segments = segmentsOf(at);
    const [inside, last] = [within(Array.dropRight(segments, 1)), Option.getOrElse(Array.last(segments), () => '')];
    const [living, recalled] = Array.partition(held.items, (each) => (each.live ? Result.succeed(each) : Result.fail(each)));
    const named = held.items.flatMap((each) => Option.toArray(Option.map(each.name, (name) => ({ each, name }))));
    const found = Array.findFirst(Array.cartesian([living, recalled], [(name: string) => Unmatched.named(name, last), loose(last)]), ([tier, test]) => {
        const hits = tier.filter((each) => Option.exists(each.name, test) && inside(each));
        return Array.isReadonlyArrayNonEmpty(hits) ? Option.some(hits) : Option.none();
    });
    const sectioned = new Set(
        living.flatMap((each) => (each.target._tag === 'Control' && each.target.control.section !== undefined && Unmatched.named(each.target.control.section, last) && inside({ ...each, ancestors: Array.dropRight(each.ancestors, 1) }) ? Option.toArray(Record.get(held.live, `${each.target.container.ref}`)) : [])),
    );
    const candidates = Option.getOrElse(found, () => [...sectioned]);
    return Array.match(candidates, {
        onEmpty: () => refused('control, option, or container', Unmatched.rank(last, named, Struct.get('name')).map(Struct.get('each'))),
        onNonEmpty: (several) => (Predicate.isTupleOf(several, 1) ? Result.succeed(several[0]) : refused('single control, option, or container', several)),
    });
};
const resolve = (held: Index, at: string): Result.Result<Item, ApplicationFailure | Unmatched> =>
    pipe(
        Option.map(Record.get(held.tokens, at), (exact): Result.Result<Item, ApplicationFailure | Unmatched> => Result.succeed(exact)),
        Option.orElse(() =>
            Option.map(
                Option.flatMap(numbered(at), (ref) => Record.get(held.left, ref)),
                ({ container, locator }) => Option.match(Record.get(held.located, locator), { onSome: (found) => Result.succeed(Array.headNonEmpty(found)), onNone: () => Result.fail(lost(held, at, container.locator, Option.none())) }),
            ),
        ),
        Option.orElse(() => Option.map(numbered(at), (ref) => Result.fail(Unmatched.of({ subject: 'reference', query: Option.some(at), candidates: Option.toArray(Option.flatMap(Record.get(held.live, ref), Struct.get('token'))) })))),
        Option.getOrElse(() => matched(held, at)),
    );
const find = (held: Index, query: string, kind: Option.Option<(typeof Element.Control.Type)['kind']>) => {
    const segments = segmentsOf(query);
    const [inside, last] = [within(Array.dropRight(segments, 1)), Option.getOrElse(Array.last(segments), () => '')];
    const kept = held.items.filter((each) => inside(each) && Option.match(kind, { onNone: () => Option.isSome(each.name) || Option.isSome(each.value), onSome: (wanted) => Option.contains(each.kind, wanted) }));
    const stated = Target.$match({
        Container: ({ container }) => Array.fromNullishOr(container.help),
        Control: ({ control }) => [...Array.fromNullishOr(control.help), ...(control.labels ?? [])],
        Option: ({ option }) => Array.fromNullishOr(option.help),
        RememberedContainer: () => Array.empty<string>(),
        RememberedControl: ({ control }) => [...Option.toArray(control.help), ...Option.getOrElse(control.labels, Array.empty<string>)],
    });
    const ranked = Unmatched.rank(last, kept, ({ target, name, value }) => [...Option.toArray(name), ...Option.toArray(value), ...stated(target)].join(' '));
    return Array.sortWith(ranked, Struct.get('live'), Order.flip(Order.Boolean));
};
const route = (held: Index) => (ref: (typeof Element.Control.Type)['ref']) => Record.get(held.routes, `${ref}`);
const refOf = (instance: Instance['instance']) => {
    const interned = ({ locator }: { readonly locator: string }) => ApplicationServer.use((server) => server.intern({ instance, locators: [locator] })).pipe(Effect.map(Array.head), Effect.flatMap(Effect.fromOption));
    return ({ target }: Item) => Target.$match(target, { Container: ({ container }) => Effect.succeed(container.ref), Control: ({ control }) => Effect.succeed(control.ref), Option: ({ option }) => Effect.succeed(option.ref), RememberedContainer: interned, RememberedControl: interned });
};
const facets = Effect.fnUntraced(function* ({ row, live }: Aimed, from: Linked, model: Model, payload: { readonly within?: Description['containers'][number]['locator'] }) {
    const [server, maps, toolbar] = yield* Effect.all([ApplicationServer, InterfaceMap.Store, Toolbar]);
    const [described, tools] = yield* Effect.all([from.describe(payload), toolbar(row)]);
    return yield* Description.compose(model, yield* maps.learn(live, Description.fold(described, tools)), (locators) => server.intern({ instance: live.instance, locators }));
});
const scan = Effect.fnUntraced(function* (target: Aimed, scope: typeof Element.Scope.Type) {
    const { row, live } = target;
    const [server, maps, handle, source] = yield* Effect.all([ApplicationServer, InterfaceMap.Store, Handle, Source]);
    const [map, last, client] = yield* Effect.all([maps.recall(live), handle.get(Indexed), Effect.option(source(row))], { concurrency: 'unbounded' });
    const whole = scope._tag === 'application';
    const level = Option.flatMap(Option.liftPredicate(scope, Element.Scope.isAnyOf(['container', 'menu', 'rows'])), ({ ref }) => Option.flatMap(own(last, live.instance), (held) => Record.get(held.routes, `${ref}`)));
    const from = Option.filter(client, () => whole || Option.isSome(level));
    const known = Option.getOrUndefined(Option.map(map.known, (verified) => ({ known: verified })));
    const scopes: readonly (typeof Element.Scope.Type)[] = [...(whole || Option.isNone(from) ? [scope] : []), ...(whole ? [{ _tag: 'menus' as const }] : [])];
    const models = yield* Effect.forEach(scopes, (each) => server.model({ instance: live.instance, scope: each, ...(each._tag === 'application' ? known : {}) }), { concurrency: 'unbounded' });
    const accessible = Option.getOrElse(Array.head(models), (): Model => ({ sequence: Option.match(own(last, live.instance), { onNone: () => 0, onSome: (held) => held.model.sequence }), containers: [] }));
    const { model, routes } = yield* Option.match(from, {
        onNone: () => Effect.succeed({ model: accessible, routes: Record.empty<string, Description['containers'][number]['locator']>() }),
        onSome: (linked) => facets(target, linked, accessible, Option.match(level, { onNone: () => ({}), onSome: (at) => ({ within: at }) })),
    });
    yield* Effect.when(maps.sight(live, model.pointer), Effect.succeed(whole));
    const stated = { ...model, containers: Array.unionWith(model.containers, Array.drop(models, 1).flatMap(Struct.get('containers')), (left, right) => left.ref === right.ref) };
    const merged = (state: typeof last) => merge(own(state, live.instance), stated, whole, routes);
    yield* handle.update(Indexed, (state) => Option.some({ instance: live.instance, index: merged(state) }));
    return { model: stated, map, index: Option.getOrElse(own(yield* handle.get(Indexed), live.instance), () => merged(Option.none())) };
});
const read = (target: Aimed) =>
    Effect.map(scan(target, { _tag: 'application' }), ({ model: application, map, index: base }) => ({
        application,
        map,
        base,
        route: route(base),
        indexed: (containers: (typeof Element.Model.Type)['containers']) => index({ ...application, containers }),
        level: (scope: typeof Element.Scope.Type) => Effect.map(scan(target, scope), Struct.get('model')),
    }));
const reach = Effect.fnUntraced(function* (target: Aimed, at: string) {
    const [handle, maps] = yield* Effect.all([Handle, InterfaceMap.Store]);
    const held = own(yield* handle.get(Indexed), target.live.instance);
    const shown = Option.flatMap(held, (last) => Option.filter(Result.getSuccess(resolve(last, at)), Struct.get('live')));
    const sat = Option.flatMap(Option.all({ last: held, ref: numbered(at) }), ({ last, ref }) => Option.map(Record.get(last.left, ref), ({ container }) => ({ last, ref: container.ref })));
    const again = Option.match(sat, {
        onNone: () => Effect.map(scan(target, { _tag: 'application' }), Struct.get('index')),
        onSome: ({ last, ref }) =>
            scan(target, { _tag: 'container', ref }).pipe(
                Effect.map(Struct.get('index')),
                Effect.catchIf(Schema.is(ApplicationFailure), () => Effect.succeed(last)),
            ),
    });
    const recalled = (now: Index) => Effect.flatMap(maps.recall(target.live), (map) => Effect.fromResult(resolve(recall(now)(map), at)));
    return yield* Option.match(shown, { onSome: (found) => Effect.succeed(found), onNone: () => Effect.flatMap(again, recalled) });
});
const Address = { At, Target, index, recall, opener, opening, locate, resolve, reach, find, refOf, route, close, path, token, fact: printValue, value: printStored, separator: SEPARATOR } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Address, type Index, Indexed, type Item, read, type Shown, Source, Toolbar };
