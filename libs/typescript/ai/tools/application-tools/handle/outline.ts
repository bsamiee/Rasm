import type { Act } from '@rasm/applications/server/act';
import { ApplicationServer } from '@rasm/applications/server/client';
import { Element } from '@rasm/applications/server/element';
import { ApplicationFailure } from '@rasm/applications/server/failure';
import { Instance } from '@rasm/applications/server/instance';
import { Handle } from '@rasm/mcp/server';
import { Call, Format, Line, Report } from '@rasm/report';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Effect, Equal, Function, HashMap, HashSet, Match, Option, Order, Predicate, Record, Result, String, Struct, UndefinedOr } from 'effect';
import { Address } from '#/handle/address';
import { Target } from '#/handle/target';

// --- [TYPES] ---------------------------------------------------------------------------

type Container = typeof Element.cases.container.Type;
type Node = ReturnType<typeof Address.recall>[number];
interface Marks {
    readonly read: Address;
    readonly changed: HashSet.HashSet<typeof Element.Ref.Type>;
    readonly gone: ReadonlyMap<typeof Element.Ref.Type, readonly string[]>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const CUTS = ['text', 'options', 'rows', 'containers'] as const;
const KEPT = { options: 6, rows: 20, words: 6, showing: 8 } as const;
const WORDS = new Intl.Segmenter(undefined, { granularity: 'word' });
const NAMING = /[\p{L}\p{N}]/u;
const TITLE_BAR: ReadonlySet<string> = new Set(['AXCloseButton', 'AXMinimizeButton', 'AXZoomButton', 'AXFullScreenButton']);
const SAY = { gone: 'not shown now', front: 'in front' } as const;
const NOUNS = {
    window: { one: 'window', other: 'windows' },
    dialog: { one: 'dialog', other: 'dialogs' },
    panel: { one: 'panel', other: 'panels' },
    sheet: { one: 'sheet', other: 'sheets' },
    popover: { one: 'popover', other: 'popovers' },
    drawer: { one: 'drawer', other: 'drawers' },
    prompt: { one: 'prompt', other: 'prompts' },
    menuBar: { one: 'menu bar', other: 'menu bars' },
    statusItem: { one: 'status item', other: 'status items' },
    menu: { one: 'menu', other: 'menus' },
    toolbar: { one: 'toolbar', other: 'toolbars' },
    web: { one: 'web view', other: 'web views' },
    control: { one: 'control', other: 'controls' },
    option: { one: 'option', other: 'options' },
    item: { one: 'item', other: 'items' },
    row: { one: 'row', other: 'rows' },
    character: { one: 'character', other: 'characters' },
    popUp: { one: 'pop-up', other: 'pop-ups' },
    match: { one: 'match', other: 'matches' },
} as const satisfies Record<Container['kind'] | 'control' | 'option' | 'item' | 'row' | 'character' | 'popUp' | 'match', { readonly one: string; readonly other: string }>;

// --- [OPERATIONS] ----------------------------------------------------------------------

const inspect = (given: Call['given']) => new Call({ tool: 'inspect', given });
const label = (held: Readonly<Record<string, string | number | boolean | undefined>>) =>
    Record.collect(
        Record.filter(held, (value): value is string | number | true => value !== undefined && value !== false && value !== ''),
        (key, value) => (value === true ? String.noCase()(key) : `${String.noCase()(key)}: ${Predicate.isNumber(value) ? Format.count(value) : value}`),
    );
const shortcut = ({ key, modifiers }: typeof Element.Shortcut.Type) => [...modifiers.map(String.capitalize), key].join('-');
const help = (text: string | undefined, name: string | undefined, whole: boolean) => (text !== undefined && text !== name && (whole || name === undefined || !NAMING.test(name)) ? Format.quoted(text) : undefined);
const caption = (kind: Container['kind'], ref?: typeof Element.Ref.Type, title?: string) => [...Array.fromNullishOr(ref).map((at) => Address.refer(kind, at)), NOUNS[kind].one, ...Array.fromNullishOr(title).filter(String.isNonEmpty).map(Format.quoted)];
const classify = (element: Node['element']): readonly string[] =>
    Match.value(element).pipe(
        Match.tag('container', ({ kind }) => [NOUNS[kind].one]),
        Match.tag('control', ({ kind }) => (kind === 'action' || kind === 'item' || kind === 'label' ? [] : [kind])),
        Match.orElse(({ _tag }) => [_tag]),
    );
const link = (read: Address, ref: typeof Element.Ref.Type | undefined) =>
    Option.fromUndefinedOr(ref).pipe(
        Option.flatMap((at) => Address.get(read, at)),
        Option.flatMap(({ token }) => token),
        Option.getOrUndefined,
    );
const shows = (read: Address, ref: typeof Element.Ref.Type) => Option.exists(Address.get(read, ref), ({ element }) => element.ref === ref);
const members = Function.memoize((read: Address) => Map.groupBy(Address.list(read), ({ containers }) => Array.lastNonEmpty(containers).ref));
const scroll = ({ vertical = 0, horizontal = 0, rows, showing }: NonNullable<Result.Result.Success<(typeof Act.Outcome.Type)['outcomes'][number]>['scrolled']>, wheel: boolean): readonly string[] => {
    const moved = Record.collect(
        Record.filter({ down: vertical, across: horizontal }, (at) => at !== 0),
        (way, at) => `scrolled ${Format.percent(at)} ${way}`,
    );
    const listed = Array.match(showing, {
        onEmpty: Array.empty<string>,
        onNonEmpty: (held) => {
            const span = [`rows ${Array.headNonEmpty(held).index + 1} to ${Array.lastNonEmpty(held).index + 1}`, ...Array.fromNullishOr(rows).map((total) => `of ${total}`)].join(' ');
            const [named, rest] = Array.splitAt(held, KEPT.showing);
            const labels = [...named.map(({ index, label: text }) => UndefinedOr.match(text, { onUndefined: () => `row ${index + 1}`, onDefined: Format.quoted })), ...(Array.isReadonlyArrayEmpty(rest) ? [] : [Format.count(rest.length, { other: 'more' })])];
            return [`${span}: ${labels.join(', ')}`];
        },
    });
    return wheel ? [...moved, ...listed] : moved.map((fact) => `${fact} first`);
};
const nest = (marks: Marks, within: Option.Option<Container>, children: readonly Element[], chosen?: string) => {
    const named = new Set([...Option.toArray(within).flatMap(({ title }) => Array.fromNullishOr(title)), ...children.flatMap((child) => (child._tag === 'control' && child.kind !== 'label' ? Array.fromNullishOr(child.label?.text) : []))]);
    const [bare, shown] = Array.partition(
        children.filter((child) => !(child._tag === 'control' && ((child.subrole !== undefined && TITLE_BAR.has(child.subrole)) || (child.kind === 'label' && Predicate.isTagged(child.value, 'text') && named.has(child.value.value))))),
        (child) => (child._tag === 'control' && child.enabled === false && child.label === undefined && child.value === undefined && Array.isReadonlyArrayEmpty(child.children) ? Result.succeed(child) : Result.fail(child)),
    );
    const rows = shown.map(draw(marks, within, false, chosen));
    const common = rows.length > 1 ? Array.drop(rows, 1).reduce((kept, { facts = [] }) => kept.intersection(new Set(facts)), new Set(rows[0]?.facts)) : new Set<string>();
    const noun =
        NOUNS[
            Option.getOrElse(
                Array.findFirst(['option', 'row', 'item'] as const, (kind) => shown.every((child) => (child._tag === 'option' ? child._tag : child.kind) === kind)),
                () => 'control' as const,
            )
        ].other;
    return {
        facts: [...Array.fromIterable(common).map((fact) => `all ${noun} ${fact}`), ...(Array.isReadonlyArrayEmpty(bare) ? [] : [`${Format.count(bare.length)} unnamed disabled`])],
        rows: rows.map((each) => new Line({ ...each, facts: (each.facts ?? []).filter((fact) => !common.has(fact)) })),
    };
};
const container = (marks: Marks, held: Container): Line => {
    const { read, gone } = marks;
    const token = Address.refer(held.kind, held.ref);
    const said = new Set((members(read).get(held.ref) ?? []).flatMap(({ name, value }) => [...Option.toArray(name), ...Option.toArray(value)]));
    const below = nest(marks, Option.some(held), held.children);
    const rows = [
        ...below.rows,
        ...held.text.filter((text) => !said.has(text) && !said.has(Address.print('text')(text))).map((text) => new Line({ head: [], value: Address.print('text')(text), cut: CUTS.indexOf('text') })),
        ...Array.fromNullishOr(gone.get(held.ref))
            .filter(Array.isReadonlyArrayNonEmpty)
            .map((tokens) => new Line({ head: ['gone'], value: tokens.join(', ') })),
    ];
    return new Line({
        head: caption(held.kind, held.ref, held.title),
        facts: [
            ...label({
                ...Struct.pick(held, ['focused', 'modal', 'asking', 'edited', 'fullScreen', 'closed']),
                loading: held.loaded === false,
                loaded: UndefinedOr.map(held.loadingProgress, (done) => (done < 1 ? Format.percent(done) : undefined)),
                close: link(read, held.close),
                document: held.document,
                help: help(held.help, held.title, false),
            }),
            ...below.facts,
            ...Record.collect(
                Record.filter({ unreadable: new Call({ tool: 'view', given: { of: token } }), truncated: inspect({ in: token }) }, (_, key) => held[key]),
                (key, call) => `${key} - ${Format.call(call)}`,
            ),
        ],
        pinned: held.asking || rows.some(({ pinned }) => pinned === true),
        rows,
        call: inspect({ in: token }),
    });
};
const control = (marks: Marks, within: Option.Option<Container>, held: typeof Element.cases.control.Type, whole: boolean): Line => {
    const { read, changed } = marks;
    const [token, print] = [Address.refer(held.kind, held.ref), Address.print(held.kind)];
    const attribute = Predicate.hasProperty(held.label, 'attribute') ? held.label.attribute : undefined;
    const text = Predicate.isTagged(held.value, 'text') ? held.value.value : undefined;
    const [cells, listed]: readonly [readonly Element[], readonly Element[]] = held.kind === 'row' ? [held.children, []] : [[], held.children];
    const below = nest(marks, within, listed, text);
    const name = Option.filter(
        Option.flatMap(Address.get(read, held.ref), ({ name: stated }) => stated),
        () => !cells.some((cell) => cell._tag === 'control' && cell.label?.text === held.label?.text),
    );
    const inline = cells.flatMap((cell) => {
        const { head, value } = draw(marks, within)(cell);
        const stated = cell._tag === 'control' && Predicate.hasProperty(cell.label, 'attribute') && cell.label.attribute === 'AXRoleDescription' ? Option.fromUndefinedOr(cell.value) : Option.none();
        return Option.match(stated, {
            onNone: () => [[head.join(' '), ...Array.fromNullishOr(value)].join(': ')],
            onSome: (fact) => (Predicate.isTagged(fact, 'text') && Option.contains(name, fact.value) ? [] : [[...(cell._tag === 'control' ? Array.fromNullishOr(cell.column) : []), ...Array.fromNullishOr(value)].join(': ')]),
        });
    });
    const joins = (stated: string) => {
        const group = Option.filter(within, () => held.role === 'AXGroup' && held.value === undefined);
        const names = Option.toArray(group).flatMap((sat) => [...(members(read).get(sat.ref) ?? []).flatMap((each) => Option.toArray(each.name)), ...sat.text]);
        const parts = names.filter((part) => part.length < stated.length && stated.includes(part)).toSorted((left, right) => right.length - left.length);
        return parts.length > 1 && !NAMING.test(parts.reduce((rest, part) => rest.replaceAll(part, ' '), stated));
    };
    const naming = Option.exists(name, joins) ? ['group'] : Option.toArray(Option.map(name, Format.quoted));
    const fact = Option.filter(Option.fromUndefinedOr(held.value), () => Array.isReadonlyArrayEmpty(inline) && attribute !== 'AXValue' && held.kind !== 'label');
    const role = { default: Option.exists(within, (sat) => sat.default === held.ref), cancel: Option.exists(within, (sat) => sat.cancel === held.ref), focused: held.focused, changed: HashSet.has(changed, held.ref) };
    const kept = held.kind === 'multi' ? { cut: CUTS.indexOf('rows'), through: KEPT.rows - 1 } : { cut: CUTS.indexOf('options'), through: listed.findIndex((child) => child._tag === 'option' && child.label === text) + KEPT.options };
    const rows = [
        ...(held.runs ?? []).map(({ range, text: run, attributes }) => new Line({ head: [], value: print(run), facts: [...label({ range: print({ _tag: 'range', ...range }) }), ...Record.collect(attributes, (key, value) => `${key}: ${print(value)}`)] })),
        ...below.rows.map((each, at) => (at > kept.through ? new Line({ ...each, cut: kept.cut }) : each)),
    ];
    return new Line({
        head: [token, ...classify(held), ...[[...naming, ...inline].join(', ')].filter(String.isNonEmpty)],
        ...Option.getOrUndefined(Option.map(fact, (stated) => ({ value: print(stated) }))),
        facts: [
            ...label({
                disabled: held.enabled === false,
                ...role,
                selected: held.selected === 'marked' ? held.selected : held.kind === 'row' && held.selected !== undefined,
                mark: UndefinedOr.map(held.mark, Format.quoted),
                opens: link(read, held.opens),
                'alternate of': link(read, held.alternate),
                separated: held.separated,
                ...(whole ? held.range : UndefinedOr.map(held.range, (range) => Struct.pick(range, ['minimum', 'maximum']))),
                shortcut: UndefinedOr.map(held.shortcut, shortcut),
                keyEquivalent: UndefinedOr.map(held.keyEquivalent, Format.quoted),
                placeholder: String.isNonEmpty(text ?? '') ? undefined : UndefinedOr.map(held.placeholder, Format.quoted),
                level: held.level === 0 ? undefined : held.level,
                expanded: held.expanded,
                collapsed: held.expanded === false,
                rows: held.total,
                columns: Array.dedupe(
                    Array.fromNullishOr(listed.find((child) => child._tag === 'control' && child.kind === 'row'))
                        .flatMap((row) => [row, ...row.children])
                        .flatMap((each) => (each._tag === 'control' ? Array.fromNullishOr(each.column) : [])),
                )
                    .map(Format.quoted)
                    .join(', '),
                sort: held.sort,
                column: UndefinedOr.map(held.column, Format.quoted),
                selectedTextRanges: (held.selectedTextRanges ?? [])
                    .filter(({ length }) => whole || length > 0)
                    .map((range) => print({ _tag: 'range', ...range }))
                    .join(', '),
                numberOfCharacters: held.numberOfCharacters === text?.length ? undefined : held.numberOfCharacters,
                invalid: held.invalid,
                required: held.required,
                current: held.current,
                help: help(held.help, attribute === 'AXRoleDescription' ? undefined : held.label?.text, whole),
                ...(whole
                    ? {
                          actions: UndefinedOr.map(held.actions, Format.list),
                          frame: UndefinedOr.map(held.frame, (value) => print({ _tag: 'rect', value })),
                          contentSize: UndefinedOr.map(held.scroll?.contentSize, (value) => print({ _tag: 'size', value })),
                          insertionPointLineNumber: held.insertionPointLineNumber,
                          labels: UndefinedOr.map(held.labels, Format.list),
                          alternateUIVisible: held.alternateUIVisible,
                      }
                    : {}),
            }),
            ...Array.fromNullishOr(held.scroll).flatMap(({ verticalScrollBar = 0, horizontalScrollBar = 0 }) => scroll({ vertical: verticalScrollBar, horizontal: horizontalScrollBar, showing: [] }, true)),
            ...below.facts,
        ],
        pinned: Record.some({ ...role, selected: held.selected !== undefined, answer: held.kind === 'action' && Option.exists(within, (sat) => sat.asking), below: rows.some(({ pinned }) => pinned === true) }, (on) => on === true),
        ...(Array.isReadonlyArrayEmpty(rows) ? {} : { rows, call: inspect({ in: token }) }),
        ...(held.kind === 'label' ? { cut: CUTS.indexOf('text') } : {}),
    });
};
const draw =
    (marks: Marks, within: Option.Option<Container>, whole = false, chosen?: string) =>
    (element: Element): Line =>
        Element.match(element, {
            container: (held) => container(marks, held),
            control: (held) => control(marks, within, held, whole),
            option: (held) =>
                new Line({
                    head: [Address.refer(held._tag, held.ref), Format.quoted(held.label)],
                    facts: label({ selected: chosen === held.label, shortcut: UndefinedOr.map(held.shortcut, shortcut), help: help(held.help, held.label, whole) }),
                    pinned: chosen === held.label,
                }),
        });
const survey = (marks: Marks): readonly Line[] => {
    const { read } = marks;
    const tops = (flag: 'asking' | 'focused') => new Set(Address.list(read).flatMap(({ element, containers }) => (element._tag === 'container' && element[flag] === true ? [Array.headNonEmpty(containers).ref] : [])));
    const [asking, focused] = [tops('asking'), tops('focused')];
    const ordered = Array.sortWith(read.model.elements, ({ ref }) => [asking.has(ref), focused.has(ref)] as const, Order.flip(Order.Tuple([Order.Boolean, Order.Boolean])));
    const primary = Option.getOrElse(
        Array.findFirst([asking, focused], (held) => held.size > 0),
        () => new Set(Array.take(ordered, 1).map(({ ref }) => ref)),
    );
    return ordered.map((element) => {
        const line = draw(marks, Option.none())(element);
        return primary.has(element.ref) ? line : new Line({ ...line, rows: (line.rows ?? []).map((row) => new Line({ ...row, cut: CUTS.indexOf('containers') })) });
    });
};
const mention =
    (read: Address, gone = true) =>
    (node: Node): Line => {
        const build = Array.head(read.map.application.builds);
        const at = (locator: Option.Option<string>) =>
            locator.pipe(
                Option.flatMap((held) => Array.head(Address.locate(read, held))),
                Option.flatMap(({ token }) => token),
                Option.getOrUndefined,
            );
        const when = Element.Condition.match({
            Value: ({ at: locator, value }) => Array.take(Address.locate(read, locator), 1).flatMap(({ name, element }) => (element._tag === 'control' && Option.isSome(name) ? [`when ${name.value} is ${Address.print(element.kind)(value)}`] : [])),
            Documents: ({ held }) => [held ? 'when a document is open' : 'when no document is open'],
            Modal: ({ at: locator }) => (locator === null ? ['when no dialog is open'] : Array.take(Address.locate(read, locator), 1).flatMap(({ name }) => Option.toArray(name).map((title) => `when ${title} is open`))),
        });
        const facts = Match.value(node).pipe(
            Match.when({ live: true }, Array.empty<string>),
            Match.when({ element: { _tag: 'container' } }, ({ element }) => label({ [SAY.gone]: true, build: Option.isNone(build) || Equal.equals(build, element.tally.verified) ? undefined : Option.getOrElse(element.tally.verified, () => 'unversioned'), modal: element.modal })),
            Match.when({ element: { _tag: 'control' } }, ({ element, value }) => [
                ...label({
                    [SAY.gone]: gone,
                    'last value': Option.getOrUndefined(value),
                    mark: Option.getOrUndefined(Option.map(element.mark, Format.quoted)),
                    opens: at(element.opens),
                    'alternate of': at(element.alternate),
                    separated: Option.getOrUndefined(element.separated),
                    shortcut: Option.getOrUndefined(Option.map(element.shortcut, shortcut)),
                    keyEquivalent: Option.getOrUndefined(Option.map(element.keyEquivalent, Format.quoted)),
                }),
                ...Option.match(element.presence, { onNone: Array.empty, onSome: ({ with: seen, against }) => seen.filter((atom) => against.some((other) => other._tag === atom._tag && (atom._tag !== 'Value' || (other._tag === 'Value' && other.at === atom.at)) && !Equal.equals(other, atom))) }).flatMap(when),
            ]),
            Match.orElse(Array.empty<string>),
        );
        return new Line({
            head: [...Option.toArray(Option.firstSomeOf(node.live ? [node.token] : [node.token, node.path])), ...classify(node.element), ...(node.live ? Option.toArray(Option.map(node.path, Format.quoted)) : [])],
            facts,
            ...(node.live ? Option.getOrUndefined(Option.map(node.value, (value) => ({ value }))) : {}),
        });
    };
const page = (held: Line, after: Option.Option<Node>): Result.Result<readonly Line[], Unmatched> => {
    const rows = held.rows ?? [];
    const start: Result.Result<number, Unmatched> = Option.match(
        Option.flatMap(after, ({ token }) => token),
        {
            onNone: () => Result.succeed(0),
            onSome: (token) =>
                Option.match(
                    Array.findFirstIndex(rows, ({ head }) => Option.contains(Array.head(head), token)),
                    {
                        onNone: () => Result.fail(Unmatched.of({ subject: ['item', ...Array.fromNullishOr(held.call).map((call) => `of ${Format.call(call)}`)].join(' '), query: Option.some(token), candidates: rows.flatMap(({ head }) => Array.take(head, 1)) })),
                        onSome: (found) => Result.succeed(found + 1),
                    },
                ),
        },
    );
    return Result.map(start, (from) => [new Line({ ...held, rows: Array.drop(rows, from).map((each) => new Line(Struct.omit(each, ['cut']))) })]);
};
const render = (read: Address, { in: within = Option.none(), after = Option.none(), changed = HashSet.empty() }: { readonly in?: Option.Option<Node>; readonly after?: Option.Option<Node>; readonly changed?: HashSet.HashSet<typeof Element.Ref.Type> } = {}) => {
    const marks: Marks = { read, changed, gone: new Map() };
    const held: Result.Result<Line, Unmatched> = Option.match(within, {
        onNone: () => Result.succeed(new Line({ head: [], rows: survey(marks), call: inspect({}) })),
        onSome: (node) =>
            node.live
                ? Option.match(Address.get(read, node.element.ref), {
                      onNone: () => Result.fail(Unmatched.of({ subject: 'container or control of this read', query: node.token, candidates: Address.list(read).flatMap(({ element, token }) => (element._tag === 'container' ? Option.toArray(token) : [])) })),
                      onSome: (found) => Result.succeed(draw(marks, Option.some(Array.lastNonEmpty(found.containers)), true)(found.element)),
                  })
                : Result.succeed(
                      new Line({
                          ...mention(read)(node),
                          ...Option.getOrUndefined(
                              Option.map(
                                  Option.filter(node.token, () => node.element._tag === 'container'),
                                  (token) => ({ rows: Address.recall(read, node.element.locator).map(mention(read, false)), call: inspect({ in: token }) }),
                              ),
                          ),
                      }),
                  ),
    });
    return Result.flatMap(held, (line) => page(line, after));
};
const hits = (read: Address, found: readonly Node[], given: Call['given'], after: Option.Option<Node> = Option.none()) => {
    const rows = found.map((node) => {
        const line = mention(read)(node);
        const openers = Result.match(Address.open(read, node), { onFailure: Array.empty<string>, onSuccess: (chain) => chain.flatMap(({ token }) => Option.toArray(token)) });
        return new Line({ ...line, facts: [...(line.facts ?? []), ...label({ 'opened by': openers.join(', ') })] });
    });
    return page(new Line({ head: [], rows, call: inspect(given) }), after);
};
const changes = (before: Address, after: Address, touched: ReadonlySet<typeof Element.Ref.Type>): readonly Line[] => {
    const steady = (element: Element): readonly (typeof Element.Ref.Type)[] => element.children.flatMap((child) => (child._tag === 'container' || (child._tag === 'control' && child.kind === 'row') ? [] : [child.ref, ...steady(child)]));
    const own = (read: Address, ref: typeof Element.Ref.Type) => new Set(Option.match(Address.get(read, ref), { onNone: Array.empty<typeof Element.Ref.Type>, onSome: ({ element }) => steady(element) }));
    const changed = HashSet.fromIterable(
        Address.list(after).flatMap(({ element }) =>
            element._tag === 'control' && touched.has(element.ref) && Option.exists(Address.get(before, element.ref), (prior) => prior.element._tag === 'control' && !Equal.equals(Struct.pick(prior.element, ['value', 'selected']), Struct.pick(element, ['value', 'selected']))) ? [element.ref] : [],
        ),
    );
    const keep = (element: Element): readonly Element[] => {
        const whole = element._tag === 'container' && !(shows(before, element.ref) && own(before, element.ref).symmetricDifference(own(after, element.ref)).size === 0);
        const children = element.children.flatMap((child) => (whole && child._tag !== 'container' ? [child] : keep(child)));
        const pruned = element._tag === 'container' && !whole ? { ...element, children, text: [], truncated: false } : { ...element, children };
        return whole || HashSet.has(changed, element.ref) || Array.isReadonlyArrayNonEmpty(children) ? [pruned] : [];
    };
    const gone = new Map(
        members(before)
            .entries()
            .map(([ref, nodes]) => [ref, nodes.flatMap(({ element, token }) => (element._tag === 'container' || shows(after, element.ref) ? [] : Option.toArray(token)))] as const),
    );
    const closed = Address.list(before).flatMap(({ element, name }) =>
        element._tag === 'container' && !shows(after, element.ref)
            ? [
                  new Line({
                      head: caption(element.kind, element.ref, Option.getOrUndefined(name)),
                      facts: [
                          'closed',
                          ...Option.getOrElse(
                              Option.flatMap(HashMap.get(after.left, element.ref), ({ gone: facts }) => facts),
                              Array.empty<string>,
                          ),
                      ],
                  }),
              ]
            : [],
    );
    return [...after.model.elements.flatMap(keep).map(draw({ read: after, changed, gone }, Option.none())), ...closed];
};
const clipboard = (instance: Instance['instance'], { locator, types, accessBehavior, detectedPatterns, contentType, content }: typeof Act.Pasteboard.Type, asked: boolean) => {
    const text = content?._tag === 'text' ? content.value : undefined;
    const shown = UndefinedOr.map(text, (value) => {
        const words = Iterator.from(WORDS.segment(value)).filter(({ isWordLike }) => isWordLike === true);
        const end = asked
            ? value.length
            : (words
                  .drop(KEPT.words - 1)
                  .map(({ index, segment }) => index + segment.length)
                  .next().value ?? value.length);
        return value.slice(0, Math.min(Report.page, end)).toWellFormed();
    });
    const facts = [
        ...(text === undefined || text.length === shown?.length ? [] : [Format.count(text.length, NOUNS.character)]),
        ...label({
            empty: Array.isReadonlyArrayEmpty(types),
            content: content === undefined || content._tag === 'text' ? undefined : content._tag,
            types: asked && Format.list(types),
            detectedPatterns: asked && Format.list(detectedPatterns),
            contentType,
        }),
        ...(content?._tag === 'withheld' ? [`Paste from Other Apps ${accessBehavior} in System Settings > Privacy & Security`] : []),
    ];
    return ApplicationServer.use((server) => server.intern({ instance, locators: [locator] })).pipe(Effect.map(Array.map((ref) => new Line({ head: [Address.refer('clipboard', ref), 'clipboard'], facts, ...(shown === undefined ? {} : { value: Address.print('text')(shown) }) }))));
};
const subject = Effect.fnUntraced(function* (name: string, { instance, state }: Pick<Instance, 'instance' | 'state'>, { verb, facts = [], troubled = false, keyed = false }: { readonly verb?: string; readonly facts?: readonly string[]; readonly troubled?: boolean; readonly keyed?: boolean } = {}) {
    const [{ session, pressure }, handle] = yield* Effect.all([ApplicationServer, Handle]);
    const [screen, strain, aim] = yield* Effect.all([session, pressure, handle.get(Target)]);
    const stalled = Instance.State.match(state, {
        running: ({ status }) => Instance.State.Status.matchOrElse(status, { responding: Array.empty<string> }, ({ _tag, ...held }) => [_tag, ...label(held)]),
        failed: ({ reason, subject: failed, values }) => [ApplicationFailure.of(reason, failed, values).message],
        exited: ({ _tag }) => [_tag],
    });
    const departures = [
        ...stalled,
        ...label({ [SAY.front]: state._tag === 'running' && state.active }),
        ...Option.match(screen, { onNone: Array.empty<string>, onSome: ({ onConsole, locked, asleep, secureInput }) => label({ 'screen in another session': !onConsole, 'screen locked': locked, 'display asleep': asleep, 'secure input on': keyed && secureInput }) }),
        ...Option.match(
            Option.filter(strain, () => troubled || Array.isReadonlyArrayNonEmpty(stalled)),
            {
                onNone: Array.empty<string>,
                onSome: ({ memory, thermal, advisory = {} }) => label({ 'memory pressure': memory === 'normal' ? undefined : memory, 'thermal state': thermal === 'serious' || thermal === 'critical' ? thermal : undefined, 'system load advisory': Record.some(advisory, (level) => level === 'bad') && 'bad' }),
            },
        ),
        ...facts,
    ];
    return verb !== undefined || Array.isReadonlyArrayNonEmpty(departures) || !Option.exists(aim.held, (aimed) => aimed.instance === instance) ? [new Line({ head: [name, ...Array.fromNullishOr(verb)], facts: departures, pinned: true })] : [];
});
const Outline = { subject, caption, render, hits, changes, clipboard, scroll, shortcut, nouns: NOUNS, say: SAY, rows: KEPT.rows } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Outline };
