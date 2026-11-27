import { Unmatched } from '@rasm/search/unmatched';
import { Array, Duration, Equal, HashSet, Match, Number, Option, Order, Predicate, pipe, Record, Result, type Schema, String, Struct, UndefinedOr } from 'effect';
import { Address, type Index, type Item, type Shown } from '#/address';
import type { Pasteboard } from '#/instances';
import { Condition, type Container, type Control, type KeyStep } from '#/interface';

// --- [TYPES] ---------------------------------------------------------------------------

interface Folded {
    readonly entry: Shown['controls'][number];
    readonly alternates: Shown['controls'];
}
interface Node {
    readonly line: string;
    readonly tokens: readonly string[];
    readonly pinned: boolean;
    readonly cut: number;
    readonly children: Option.Option<{ readonly at: string; readonly nodes: readonly Node[] }>;
}
interface Draft {
    readonly token: Option.Option<string>;
    readonly tokens: readonly string[];
    readonly head: string;
    readonly facts: readonly string[];
    readonly pinned: boolean;
    readonly foldable: boolean;
    readonly children: readonly Node[];
}
interface Page {
    readonly nodes: readonly Node[];
    readonly call: readonly string[];
    readonly depth: number;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const NEVER = globalThis.Number.POSITIVE_INFINITY;
const BUDGET = { page: 6000, ceiling: 12_000 };
const UNBOUNDED: typeof BUDGET = { page: NEVER, ceiling: NEVER };
const CUTS = ['text', 'map', 'options', 'rows', 'containers'] as const;
const KEPT = { options: 6, rows: 20, words: 6, showing: 8 } as const;
const WORDS = new Intl.Segmenter(undefined, { granularity: 'word' });
const MEASURE = new Intl.NumberFormat('en', { maximumFractionDigits: 2, maximumSignificantDigits: 3, roundingPriority: 'morePrecision', useGrouping: false });
const PERCENT = new Intl.NumberFormat('en', { style: 'percent' });
const PLURAL = new Intl.PluralRules('en');
const LIST = new Intl.ListFormat('en', { type: 'conjunction' });
const SPANS = { whole: new Intl.DurationFormat('en', { style: 'narrow' }), tenths: new Intl.DurationFormat('en', { style: 'narrow', milliseconds: 'numeric', fractionalDigits: 1 }) };
const INDENT = '  ';
const SAY = {
    asking: 'asks for an answer',
    gone: 'not shown now',
    closed: 'closed, act presses its items without opening it',
    unlisted: 'options list only while open, to chooses one by its label',
    separated: 'separator above',
    changed: 'the application changed',
    drawn: 'the window changed',
    following: 'proof follows as event',
    front: 'application in front',
    unfronted: 'bring to front refused',
    secure: 'person fills it, no step can',
    unseen: 'not in view',
} as const;
const NOUNS = {
    window: ['window', 'windows'],
    dialog: ['dialog', 'dialogs'],
    panel: ['panel', 'panels'],
    sheet: ['sheet', 'sheets'],
    popover: ['popover', 'popovers'],
    drawer: ['drawer', 'drawers'],
    prompt: ['prompt', 'prompts'],
    menuBar: ['menu bar', 'menu bars'],
    statusItem: ['status item', 'status items'],
    menu: ['menu', 'menus'],
    toolbar: ['toolbar', 'toolbars'],
    web: ['web view', 'web views'],
    item: ['item', 'items'],
    row: ['row', 'rows'],
    character: ['character', 'characters'],
    popUp: ['pop-up', 'pop-ups'],
    match: ['match', 'matches'],
    container: ['container', 'containers'],
} as const satisfies Record<Container['kind'] | 'item' | 'row' | 'character' | 'popUp' | 'match' | 'container', readonly [string, string]>;
const UNSPOKEN: ReadonlySet<Control['kind']> = new Set(['action', 'item']);
const SELECTED: Readonly<Record<NonNullable<Control['selected']>, Partial<Record.ReadonlyRecord<Control['kind'], string>>>> = { attribute: { row: 'selected' }, marked: { row: 'selected, inferred', choice: 'value inferred' } };

// --- [OPERATIONS] ----------------------------------------------------------------------

const facts = (head: string, rest: readonly string[]) => [head, ...rest].join(', ');
const flags = (held: Readonly<Record<string, boolean>>) => Record.keys(Record.filter(held, (on) => on));
const count = (amount: number, noun: keyof typeof NOUNS) => `${amount} ${NOUNS[noun][PLURAL.select(amount) === 'one' ? 0 : 1]}`;
const named = (text: string) => (text.includes(',') || text.startsWith(' ') ? JSON.stringify(text) : text);
const duration = (elapsed: Duration.Input) => {
    const whole = Duration.millis(Math.round(Duration.toMillis(elapsed)));
    const { days, hours, minutes, seconds, millis } = Duration.parts(whole);
    return Match.value(whole).pipe(
        Match.when(Duration.isLessThan(Duration.seconds(1)), () => SPANS.whole.format({ milliseconds: millis })),
        Match.when(Duration.isLessThan(Duration.minutes(1)), () => SPANS.tenths.format({ seconds, milliseconds: millis })),
        Match.orElse(() => SPANS.whole.format({ days, hours, minutes, seconds })),
    );
};
const scrolled = (vertical: number | undefined, horizontal: number | undefined) => [...Array.fromNullishOr(vertical).map((at) => `scrolled ${PERCENT.format(at)} down`), ...Array.fromNullishOr(horizontal).map((at) => `scrolled ${PERCENT.format(at)} across`)];
const showing = ({ rows, showing: shown }: { readonly rows?: number; readonly showing: readonly { readonly index: number; readonly label?: string }[] }) =>
    Array.match(shown, {
        onEmpty: Array.empty,
        onNonEmpty: (held) => {
            const span = [`rows ${Array.headNonEmpty(held).index + 1} to ${Array.lastNonEmpty(held).index + 1}`, ...Array.fromNullishOr(rows).map((total) => `of ${total}`)].join(' ');
            const labels = [...Array.take(held, KEPT.showing).map(({ index, label }) => UndefinedOr.match(label, { onUndefined: () => `row ${index + 1}`, onDefined: named })), ...(held.length > KEPT.showing ? [`${held.length - KEPT.showing} more`] : [])];
            return [`${span}: ${labels.join(', ')}`];
        },
    });
const heading = (parts: readonly string[], value: Option.Option<string>) => Option.match(value, { onNone: () => parts.join(' '), onSome: (held) => `${parts.join(' ')}: ${held}` });
const spoken = (kind: Control['kind']) => (UNSPOKEN.has(kind) ? [] : [kind]);
const quoted = (key: string, value: Schema.Json) => `${key}:${JSON.stringify(value)}`;
const keys = ({ key, modifiers }: KeyStep) => [...modifiers.map(String.capitalize), key].join('-');
const shortcut = (step: KeyStep) => `shortcut ${keys(step)}`;
const help = (text: string | undefined, label: string | undefined) => Array.fromNullishOr(text).flatMap((held) => (held === label ? [] : [`help: ${named(held)}`]));
const call = (held: readonly string[]) => `inspect {${held.join(', ')}}`;
const node = (line: string, tokens: readonly string[], rest: Partial<Node> = {}): Node => ({ line, tokens, pinned: false, cut: NEVER, children: Option.none(), ...rest });
const below = (depth: number, { at, nodes }: { readonly at: string; readonly nodes: readonly Node[] }): Page => ({ nodes, call: [quoted('in', at)], depth: depth + 1 });
const opensOf = (token: Option.Option<string>) => Option.toArray(Option.map(token, (at) => `opens ${at}`));
const siblings = (drafts: readonly Draft[]): readonly Node[] => {
    const common = Array.match(drafts, {
        onEmpty: () => new Set<string>(),
        onNonEmpty: (all) => (Predicate.isTupleOf(all, 1) ? new Set<string>() : Array.tailNonEmpty(all).reduce((kept, each) => kept.intersection(new Set(each.facts)), new Set(Array.headNonEmpty(all).facts))),
    });
    const composed = drafts.map((held) => ({ held, described: [held.head, ...held.facts.filter((fact) => !common.has(fact))].filter(String.isNonEmpty).join(', ') }));
    const alike = Array.groupBy(
        composed.filter(({ held }) => held.foldable && Array.isReadonlyArrayEmpty(held.children)),
        Struct.get('described'),
    );
    return composed.flatMap(({ held, described }) => {
        const same = Option.getOrElse(
            Option.filter(Record.get(alike, described), () => held.foldable && Array.isReadonlyArrayEmpty(held.children)),
            () => [{ held, described }],
        );
        const tokens = same.flatMap((each) => each.held.tokens);
        const leading = same.flatMap((each) => Option.toArray(each.held.token));
        return Option.exists(Array.head(same), (first) => first.held === held)
            ? [node([leading.join(','), described].filter(String.isNonEmpty).join(' '), tokens, { pinned: same.some((each) => each.held.pinned), children: Array.isReadonlyArrayNonEmpty(held.children) ? Option.map(held.token, (at) => ({ at, nodes: held.children })) : Option.none() })]
            : [];
    });
};
const alternation = <A>(entries: readonly A[], key: (entry: A) => string, alternate: (entry: A) => Option.Option<string>) => {
    const held = new Set(entries.map(key));
    const links = Record.getSomes(Record.fromIterableWith(entries, (entry) => [key(entry), Option.filter(alternate(entry), (up) => up !== key(entry) && held.has(up))]));
    const climb = (at: string, steps: number): Option.Option<string> => Option.match(Record.get(links, at), { onNone: () => Option.some(at), onSome: (up) => (steps === 0 ? Option.none() : climb(up, steps - 1)) });
    const rooted = entries.flatMap((entry) =>
        Option.toArray(
            Option.map(
                Option.filter(climb(key(entry), entries.length), (root) => root !== key(entry)),
                (root) => ({ root, entry }),
            ),
        ),
    );
    const beside = Array.groupBy(rooted, Struct.get('root'));
    const folded = new Set(rooted.map(({ entry }) => key(entry)));
    return entries.filter((entry) => !folded.has(key(entry))).map((entry) => ({ entry, alternates: Option.getOrElse(Record.get(beside, key(entry)), Array.empty).map(Struct.get('entry')) }));
};
const remembered = (index: Index, locator: string): readonly Node[] => {
    const listed = Option.getOrElse(Record.get(index.remembered.controls, locator), Array.empty).flatMap(({ target, ...item }) => (target._tag === 'RememberedControl' ? [{ ...item, target }] : []));
    const located = (at: string) => Option.map(Record.get(index.located, at), Array.headNonEmpty);
    const when = Condition.match({
        Value: ({ at, value }) =>
            Option.toArray(
                pipe(
                    located(at),
                    Option.flatMap(({ name, kind }) => Option.all({ name, kind })),
                    Option.map(({ name, kind }) => `when ${name} is ${Address.value(kind, value)}`),
                ),
            ),
        Documents: ({ held }) => [held ? 'when a document is open' : 'when no document is open'],
        Modal: ({ at }) => (at === null ? ['when no dialog is open'] : Option.toArray(Option.map(Option.flatMap(located(at), Struct.get('name')), (title) => `when ${title} is open`))),
    });
    const described = ({ target: { control }, token, name, value }: (typeof listed)[number]) =>
        facts([...Option.toArray(Option.orElse(token, () => name)), ...spoken(control.kind)].join(' '), [
            SAY.gone,
            ...Option.toArray(Option.map(value, (was) => `last value: ${was}`)),
            ...(Option.isSome(control.mark) ? ['checked'] : []),
            ...opensOf(Option.flatMap(Option.flatMap(control.opens, located), Struct.get('token'))),
            ...(Option.contains(control.separated, true) ? [SAY.separated] : []),
            ...Option.toArray(Option.map(control.shortcut, shortcut)),
            ...Option.toArray(Option.map(control.keyEquivalent, (text) => `key equivalent: ${named(text)}`)),
            ...Option.match(control.presence, {
                onNone: Array.empty,
                onSome: ({ with: seen, against }) => seen.filter((atom) => against.some((other) => other._tag === atom._tag && (atom._tag !== 'Value' || (other._tag === 'Value' && other.at === atom.at)) && !Equal.equals(other, atom))),
            }).flatMap(when),
        ]);
    return alternation(
        listed,
        ({ target }) => target.locator,
        ({ target }) => target.control.alternate,
    ).map(({ entry, alternates }) =>
        node(
            facts(
                described(entry),
                alternates.map((each) => `alternate (${described(each)})`),
            ),
            Option.toArray(entry.token),
            { cut: CUTS.indexOf('map') },
        ),
    );
};
const draft =
    (index: Index, container: Container, changed: HashSet.HashSet<Control['ref']>, leveled: boolean, within: Option.Option<string>) =>
    ({ entry: { control, item, options }, alternates, nested }: Folded & { readonly nested: readonly Folded[] }): Draft => {
        const marks = {
            [SAY.separated]: control.separated === true,
            default: container.default === control.ref,
            cancel: container.cancel === control.ref,
            close: container.close === control.ref,
            disabled: control.enabled === false,
            focused: control.focused === true,
            changed: HashSet.has(changed, control.ref),
        };
        const besides = alternates.flatMap((each) => siblings([draft(index, container, changed, leveled, Option.none())({ entry: each, alternates: [], nested: [] })]));
        const pinned = marks.default || marks.cancel || marks.close || marks.focused || marks.changed || control.selected !== undefined || (container.asking && control.kind === 'action') || besides.some(Struct.get('pinned'));
        const chosen = control.value?._tag === 'text' ? Option.some(control.value.value) : Option.none();
        const listed = options.map(({ option, item: each }) => {
            const selected = Option.contains(chosen, option.label);
            return node(facts([...Option.toArray(each.token), named(Option.getOrElse(each.name, () => 'unlabelled'))].join(' '), [...(selected ? ['selected'] : []), ...Array.fromNullishOr(option.shortcut).map(shortcut), ...help(option.help, option.label)]), Option.toArray(each.token), { pinned: selected });
        });
        const last = Option.getOrElse(Array.findFirstIndex(listed, Struct.get('pinned')), () => -1) + KEPT.options;
        const inline = control.kind === 'row' ? nested.map(({ entry }) => (entry.control.kind === 'text' && entry.control.settable !== true ? Option.getOrElse(Option.map(entry.item.value, named), () => 'empty') : heading([...Option.toArray(entry.item.token), ...spoken(entry.control.kind)], entry.item.value))) : [];
        const groups = control.kind !== 'row' && Array.isReadonlyArrayNonEmpty(nested) ? Array.groupWith(nested, (each, head) => each.entry.control.row === head.entry.control.ref) : [];
        const columns = Array.dedupe(Option.getOrElse(Array.head(groups), Array.empty).flatMap(({ entry }) => Array.fromNullishOr(entry.control.column)));
        const disclosed = groups.some((group) => Array.headNonEmpty(group).entry.control.disclosedBy !== undefined);
        const leveling = !disclosed && new Set(groups.map((group) => Array.headNonEmpty(group).entry.control.level)).size > 1;
        const rows = groups.map((group) => ({ control: Array.headNonEmpty(group).entry.control, drafted: draft(index, container, changed, leveling, item.name)({ ...Array.headNonEmpty(group), nested: Array.tailNonEmpty(group) }) }));
        const present = new Set(rows.map(({ control: { ref } }) => ref));
        const beneath: Record.ReadonlyRecord<string, readonly (typeof rows)[number][]> = Array.groupBy(
            rows.filter(({ control: { disclosedBy } }) => disclosedBy !== undefined && present.has(disclosedBy)),
            ({ control: { disclosedBy } }): string => `${disclosedBy}`,
        );
        const arrange = (level: readonly (typeof rows)[number][]): readonly Node[] => siblings(level.map(({ control: { ref }, drafted }) => ({ ...drafted, children: [...drafted.children, ...arrange(Option.getOrElse(Record.get(beneath, `${ref}`), Array.empty))] })));
        const tops = rows
            .filter(({ control: { disclosedBy } }) => disclosedBy === undefined || !present.has(disclosedBy))
            .map((row) => ({ ...row, drafted: { ...row.drafted, facts: [...row.drafted.facts, ...Array.fromNullishOr(row.control.disclosedBy).flatMap((up) => Option.toArray(Option.map(Option.flatMap(Record.get(index.live, `${up}`), Struct.get('token')), (token) => `under ${token}`)))] } }));
        const runs = (control.runs ?? []).map(({ range, text, attributes }) => node(facts(JSON.stringify(text), [`characters ${range.location} to ${range.location + range.length}`, ...Record.collect(attributes, (key, value) => `${key}: ${Address.fact('text', value)}`)]), []));
        const head = Array.match(inline, {
            onEmpty: () =>
                heading(
                    [
                        ...Option.toArray(
                            Option.map(
                                Option.filter(item.name, (name) => !Option.contains(within, name)),
                                named,
                            ),
                        ),
                        ...spoken(control.kind),
                    ],
                    item.value,
                ),
            onNonEmpty: (values) =>
                [
                    ...Option.toArray(
                        Option.map(
                            Option.filter(
                                item.name,
                                (name) =>
                                    !Option.contains(within, name) &&
                                    !Option.contains(
                                        Option.flatMap(Array.head(nested), ({ entry }) => entry.item.value),
                                        name,
                                    ),
                            ),
                            named,
                        ),
                    ),
                    ...values,
                ].join(', '),
        });
        return {
            token: item.token,
            tokens: [...Option.toArray(item.token), ...besides.flatMap(Struct.get('tokens'))],
            head,
            pinned,
            foldable: Array.isReadonlyArrayEmpty(besides),
            children: [...runs, ...listed.map((each, at) => ({ ...each, cut: at > last ? CUTS.indexOf('options') : NEVER })), ...arrange(tops).map((each, at) => ({ ...each, cut: at < KEPT.rows ? NEVER : CUTS.indexOf('rows') }))],
            facts: [
                ...(control.mark === undefined ? [] : ['checked']),
                ...Array.fromNullishOr(control.opens).flatMap((opens) => opensOf(Option.flatMap(Record.get(index.live, `${opens}`), Struct.get('token')))),
                ...flags(marks),
                ...Array.fromNullishOr(control.selected).flatMap((evidence) => Array.fromNullishOr(SELECTED[evidence][control.kind])),
                ...Array.fromNullishOr(control.range).flatMap(({ minimum, maximum, step, wraps }) => [
                    ...Match.value({ minimum, maximum }).pipe(
                        Match.when({ minimum: Match.number, maximum: Match.number }, ({ minimum: low, maximum: high }) => [`range ${MEASURE.format(low)} to ${MEASURE.format(high)}`]),
                        Match.when({ minimum: Match.number }, ({ minimum: low }) => [`at least ${MEASURE.format(low)}`]),
                        Match.when({ maximum: Match.number }, ({ maximum: high }) => [`at most ${MEASURE.format(high)}`]),
                        Match.orElse((): readonly string[] => []),
                    ),
                    ...Array.fromNullishOr(step).map((by) => `step ${MEASURE.format(by)}`),
                    ...(wraps === true ? ['wraps'] : []),
                ]),
                ...Array.fromNullishOr(control.shortcut).map(shortcut),
                ...Array.fromNullishOr(control.keyEquivalent).map((text) => `key equivalent: ${named(text)}`),
                ...Array.fromNullishOr(control.placeholder).map((text) => `placeholder: ${named(text)}`),
                ...Array.fromNullishOr(control.level).flatMap((level) => (leveled ? [`level ${level}`] : [])),
                ...Array.fromNullishOr(control.expanded).map((open) => (open ? 'expanded' : 'collapsed')),
                ...Array.fromNullishOr(control.actions).map((actions) => `actions: ${actions.map(named).join(', ')}`),
                ...Array.fromNullishOr(control.total).map((total) => count(total, 'row')),
                ...Array.match(columns, { onEmpty: Array.empty, onNonEmpty: (titles) => [`columns: ${titles.map(named).join(', ')}`] }),
                ...Array.fromNullishOr(control.sort).map((order) => `sorted ${order}`),
                ...Array.fromNullishOr(control.column).map((column) => `column: ${named(column)}`),
                ...Array.fromNullishOr(control.cells).flatMap((cells) => cells.map(({ column, text }) => UndefinedOr.match(column, { onUndefined: () => named(text), onDefined: (title) => `${title}: ${named(text)}` }))),
                ...Array.fromNullishOr(control.scroll).flatMap(({ contentSize, horizontalScrollBar, verticalScrollBar }) => [...scrolled(verticalScrollBar, horizontalScrollBar), ...Array.fromNullishOr(contentSize).map(([wide, high]) => `content ${MEASURE.format(wide)} by ${MEASURE.format(high)}`)]),
                ...Array.fromNullishOr(control.selectedTextRanges).flatMap((ranges) => (control.focused === true || ranges.some(({ length }) => length > 0) ? [`selection ${ranges.map(({ location, length }) => `of ${count(length, 'character')} at ${location}`).join(' and ')}`] : [])),
                ...Array.fromNullishOr(control.numberOfCharacters).flatMap((total) => (control.value?._tag === 'text' && control.value.value.length === total ? [] : [count(total, 'character')])),
                ...Array.fromNullishOr(control.insertionPointLineNumber).flatMap((line) => (control.focused === true ? [`line ${line}`] : [])),
                ...(control.alternateUIVisible === true ? ['hover controls shown'] : []),
                ...Array.fromNullishOr(control.invalid).map((invalid) => `invalid: ${invalid}`),
                ...(control.required === true ? ['required'] : []),
                ...Array.fromNullishOr(control.current).map((current) => `current: ${current}`),
                ...(control.kind === 'choice' && control.options === undefined ? [SAY.unlisted] : []),
                ...help(control.help, control.label?.text),
                ...Array.fromNullishOr(control.labels).map((labels) => `also labelled ${labels.map(named).join(', ')}`),
                ...besides.map(({ line }) => `alternate (${line})`),
            ],
        };
    };
const tree = (index: Index, changed: HashSet.HashSet<Control['ref']>): readonly Node[] => {
    const { containers } = index.model;
    const primary = new Set(Option.getOrElse(Array.findFirst([containers.filter(Struct.get('asking')), containers.filter((container) => container.focused === true), Array.take(containers, 1)], Array.isReadonlyArrayNonEmpty), Array.empty).map(Struct.get('ref')));
    const shown = Array.sortWith(index.shown, ({ container }) => [container.asking, container.focused === true] as const, Order.flip(Order.Tuple([Order.Boolean, Order.Boolean]))).map(({ container, item, controls }) => {
        const primaries = alternation(
            controls,
            ({ control }) => `${control.ref}`,
            ({ control }) => Option.map(Option.fromUndefinedOr(control.alternate), (ref) => `${ref}`),
        );
        const folded = siblings(
            (Array.isReadonlyArrayNonEmpty(primaries) ? Array.groupWith(primaries, ({ entry: each }, { entry: head }) => (head.control.kind === 'multi' && (each.control.kind === 'row' || each.control.row !== undefined)) || each.control.row === head.control.ref) : []).map((group) =>
                draft(index, container, changed, false, Option.none())({ ...Array.headNonEmpty(group), nested: Array.tailNonEmpty(group) }),
            ),
        );
        const nodes = [...folded.map((each) => ({ ...each, cut: primary.has(container.ref) ? NEVER : CUTS.indexOf('containers') })), ...remembered(index, container.locator), ...container.text.map((text) => node(JSON.stringify(text), [], { cut: CUTS.indexOf('text') }))];
        const line = facts([...Option.toArray(item.token), NOUNS[container.kind][0], ...Option.toArray(Option.map(item.name, named))].join(' '), [
            ...flags({
                [SAY.asking]: container.asking,
                modal: container.modal,
                focused: container.focused === true,
                unsaved: container.edited === true,
                'full screen': container.fullScreen === true,
                loading: container.loaded === false,
                [SAY.closed]: container.closed,
            }),
            ...(container.unreadable ? Option.toArray(item.token).map((at) => `content the application does not expose, view {of:${JSON.stringify(at)}} shows it`) : []),
            ...Array.fromNullishOr(container.loadingProgress).flatMap((progress) => (progress < 1 ? [`loaded ${PERCENT.format(progress)}`] : [])),
            ...Array.fromNullishOr(container.parent)
                .flatMap((parent) => Option.toArray(Option.flatMap(Record.get(index.live, `${parent}`), Struct.get('token'))))
                .map((parent) => `in ${parent}`),
            ...Array.fromNullishOr(container.document).map((document) => `document: ${document}`),
            ...help(container.help, container.title),
            ...(container.truncated ? Option.toArray(item.token).map((at) => `partly read, ${call([quoted('in', at)])} reads more`) : []),
        ]);
        return node(line, Option.toArray(item.token), { pinned: container.asking || nodes.some(Struct.get('pinned')), children: Option.map(item.token, (at) => ({ at, nodes })) });
    });
    const recalled = index.remembered.containers.flatMap(({ target, token }) =>
        target._tag === 'RememberedContainer'
            ? [
                  node(
                      facts([...Option.toArray(token), NOUNS[target.entry.container.kind][0]].join(' '), [
                          SAY.gone,
                          ...(Option.isSome(index.remembered.build) && !Equal.equals(index.remembered.build, target.entry.container.tally.verified) ? [`last seen in build ${Option.getOrElse(target.entry.container.tally.verified, () => 'stating no CFBundleVersion')}`] : []),
                          ...flags({ modal: target.entry.container.modal }),
                      ]),
                      Option.toArray(token),
                      { children: Option.map(token, (at) => ({ at, nodes: remembered(index, target.locator) })) },
                  ),
              ]
            : [],
    );
    return [...shown, ...recalled];
};
const scopes = (nodes: readonly Node[], depth: number): readonly { readonly at: string; readonly nodes: readonly Node[]; readonly depth: number }[] => nodes.flatMap((held) => Option.match(held.children, { onNone: Array.empty, onSome: (children) => [{ ...children, depth }, ...scopes(children.nodes, depth + 1)] }));
const flattened = (nodes: readonly Node[]): readonly Node[] => nodes.flatMap((held) => [held, ...Option.match(held.children, { onNone: Array.empty, onSome: (children) => flattened(children.nodes) })]);
const layout = ({ nodes }: Page, from: number, top: boolean, level: number) => {
    const groups = Array.chop(nodes.slice(from), (rest) => {
        const [loose, tail] = Array.span(rest, (each) => Array.isReadonlyArrayEmpty(each.tokens));
        return [[...loose, ...Array.take(tail, 1)], Array.drop(tail, 1)] as const;
    });
    const pinned = Option.getOrElse(Array.findLastIndex(nodes, Struct.get('pinned')), () => -1) + 1;
    const cut = Option.getOrElse(
        Array.findFirstIndex(nodes, (each) => each.cut < level),
        () => nodes.length,
    );
    return { groups, pinned, wanted: from > 0 ? nodes.length : Math.max(cut, pinned, top ? (groups[0]?.length ?? 0) : 0) };
};
const continuation = ({ nodes, call: held, depth }: Page, at: number) =>
    `${INDENT.repeat(depth)}${nodes.length - at} more, ${call([
        ...held,
        ...Option.toArray(
            Option.map(
                Option.flatMap(Array.get(nodes, at - 1), (each) => Array.last(each.tokens)),
                (token) => quoted('after', token),
            ),
        ),
    ])} lists them`;
const measure = (page: Page, from: number, top: boolean, level: number): number => {
    const { groups, wanted } = layout(page, from, top, level);
    const [, ends] = Array.mapAccum(groups, from, (start, group) => [start + group.length, start + group.length] as const);
    const at = Option.getOrElse(
        Array.findLast(ends, (end) => end <= wanted),
        () => from,
    );
    const kept = Number.sumAll(page.nodes.slice(from, at).map((each) => INDENT.length * page.depth + each.line.length + 1 + Option.match(each.children, { onNone: () => 0, onSome: (children) => measure(below(page.depth, children), 0, false, level) })));
    return kept + (at < page.nodes.length ? continuation(page, at).length + 1 : 0);
};
const print =
    (page: Page, from: number, top: boolean, level: number, budget: typeof BUDGET) =>
    (used: number): { readonly lines: readonly string[]; readonly used: number } => {
        const indent = INDENT.repeat(page.depth);
        const { groups, pinned, wanted } = layout(page, from, top, level);
        const [state, chunks] = Array.mapAccum(groups, { start: from, used, cut: Option.none<number>() }, (spent, group) => {
            const cost = Number.sumAll(group.map((each) => indent.length + each.line.length + 1));
            const fits = Option.isNone(spent.cut) && spent.start + group.length <= wanted && ((top && spent.start === from) || spent.used + cost <= budget.page || (spent.start < pinned && spent.used + cost <= budget.ceiling));
            const [total, lines] = Array.mapAccum(fits ? group : [], spent.used, (sum, each) => {
                const line = `${indent}${each.line}`;
                const inner = Option.match(each.children, { onNone: () => ({ lines: [], used: sum + line.length + 1 }), onSome: (children) => print(below(page.depth, children), 0, false, level, budget)(sum + line.length + 1) });
                return [inner.used, [line, ...inner.lines]] as const;
            });
            return [{ start: spent.start + group.length, used: total, cut: fits ? spent.cut : Option.orElse(spent.cut, () => Option.some(spent.start)) }, lines.flat()] as const;
        });
        const continued = Option.map(state.cut, (at) => continuation(page, at));
        return { lines: [...chunks.flat(), ...Option.toArray(continued)], used: state.used + Option.match(continued, { onNone: () => 0, onSome: (line) => line.length + 1 }) };
    };
const fit = (page: Page, from: number, budget: typeof BUDGET) =>
    Option.match(
        Array.findFirstIndex(CUTS, (_cut, level) => measure(page, from, true, level) <= budget.page),
        {
            onSome: (level) => print(page, from, true, level, UNBOUNDED)(0),
            onNone: () => print(page, from, true, CUTS.length, budget)(0),
        },
    ).lines;
const start = ({ nodes, call: held }: Page, after: Option.Option<Item>): Result.Result<number, Unmatched> =>
    Option.match(Option.flatMap(after, Struct.get('token')), {
        onNone: () => Result.succeed(0),
        onSome: (token) =>
            Option.match(
                Array.findFirstIndex(nodes, (each) => each.tokens.includes(token)),
                {
                    onNone: () => Result.fail(Unmatched.of({ subject: `item of ${call(held)}`, query: Option.some(token), candidates: nodes.flatMap((each) => Option.toArray(Array.last(each.tokens))) })),
                    onSome: (at) => Result.succeed(at + 1),
                },
            ),
    });
const render = (
    index: Index,
    { in: within = Option.none(), after = Option.none(), changed = HashSet.empty(), budget = BUDGET }: { readonly in?: Option.Option<Item>; readonly after?: Option.Option<Item>; readonly changed?: HashSet.HashSet<Control['ref']>; readonly budget?: typeof BUDGET } = {},
): Result.Result<readonly string[], Unmatched> => {
    const root: Page = { nodes: tree(index, changed), call: [], depth: 0 };
    const page = Option.match(Option.flatMap(within, Struct.get('token')), {
        onNone: () => Result.succeed(root),
        onSome: (token) => {
            const listed = Record.fromIterableWith(scopes(root.nodes, 1), (scope) => [scope.at, scope]);
            return Option.match(Record.get(listed, token), {
                onNone: () =>
                    Option.match(
                        Array.findFirst(flattened(root.nodes), (each) => each.tokens.includes(token)),
                        {
                            onNone: () => Result.fail(Unmatched.of({ subject: 'container, or control with listed options or rows', query: Option.some(token), candidates: Record.keys(listed) })),
                            onSome: (held): Result.Result<Page, Unmatched> => Result.succeed({ nodes: [held], call: [], depth: 0 }),
                        },
                    ),
                onSome: ({ at, nodes, depth }): Result.Result<Page, Unmatched> => Result.succeed({ nodes, call: [quoted('in', at)], depth }),
            });
        },
    });
    return Result.flatMap(page, (held) => Result.map(start(held, after), (from) => fit(held, from, budget)));
};
const hits = (found: readonly Item[], held: readonly string[], { after = Option.none(), budget = BUDGET, opening = Option.none() }: { readonly after?: Option.Option<Item>; readonly budget?: typeof BUDGET; readonly opening?: Option.Option<Index> } = {}) => {
    const opener = (item: Item) => Option.flatMap(opening, (index) => Option.map(Address.opener(index, item), (openers) => Array.getSomes(openers.map(Struct.get('token'))).join(', ')));
    const page: Page = {
        nodes: found.map((item) => {
            const kind = Address.Target.$match(item.target, {
                Container: ({ container }) => [NOUNS[container.kind][0]],
                Control: ({ control }) => spoken(control.kind),
                Option: () => ['option'],
                RememberedContainer: ({ entry }) => [NOUNS[entry.container.kind][0]],
                RememberedControl: ({ control }) => spoken(control.kind),
            });
            const path = Address.path(item).join(Address.separator);
            return node(
                facts(item.live ? heading([...Option.toArray(item.token), path, ...kind], item.value) : [Option.getOrElse(item.token, () => path), ...kind].join(' '), [
                    ...(item.live ? [] : [SAY.gone, ...Option.toArray(Option.map(item.value, (value) => `last value: ${value}`))]),
                    ...Option.toArray(Option.map(opener(item), (token) => `opened by ${token}`)),
                ]),
                Option.toArray(item.token),
            );
        }),
        call: held,
        depth: 0,
    };
    return Result.map(start(page, after), (from) => fit(page, from, budget));
};
const clipboard = ({ types, access, patterns, contentType, content }: Pasteboard, ref: Control['ref'], asked: boolean) => {
    const words = asked ? NEVER : KEPT.words;
    const head = `${Address.token('clipboard', ref)} clipboard`;
    const text = Option.map(content?._tag === 'text' ? Option.some(content.value) : Option.none<string>(), (value) => ({
        value,
        end: Math.min(
            BUDGET.page,
            Option.getOrElse(
                Array.head(
                    Iterator.from(WORDS.segment(value))
                        .filter(({ isWordLike }) => isWordLike === true)
                        .drop(words - 1)
                        .take(1)
                        .map(({ index, segment }) => index + segment.length)
                        .toArray(),
                ),
                () => value.length,
            ),
        ),
    }));
    return content?._tag === 'concealed'
        ? facts(head, ['holds a concealed item'])
        : facts(
              heading(
                  [head],
                  Option.map(text, ({ value, end }) => JSON.stringify(value.slice(0, end).toWellFormed())),
              ),
              [
                  ...Option.toArray(Option.filter(text, ({ value, end }) => value.length > end)).map(({ value }) => count(value.length, 'character')),
                  ...(Array.isReadonlyArrayEmpty(types) ? ['empty'] : []),
                  ...(asked && Array.isReadonlyArrayNonEmpty(types) ? [`types ${types.join(', ')}`] : []),
                  ...(asked && Array.isReadonlyArrayNonEmpty(patterns) ? [`detects ${patterns.join(', ')}`] : []),
                  ...Array.fromNullishOr(contentType).map((type) => `file of type ${type}`),
                  ...(content?._tag === 'withheld' ? [`text withheld, as Paste from Other Apps is ${access} for this server in System Settings > Privacy & Security > Paste from Other Apps, which lists it once a read raised the paste alert`] : []),
              ],
          );
};
const changes = (before: Index, after: Index, touched: ReadonlySet<Control['ref']>) => {
    const [held, kept] = [new Set(before.model.containers.map(Struct.get('ref'))), new Set(after.model.containers.map(Struct.get('ref')))];
    const changed = HashSet.fromIterable(after.shown.flatMap(({ controls }) => controls.filter(({ control, item }) => touched.has(control.ref) && Option.exists(Record.get(before.live, `${control.ref}`), (prior) => !Equal.equals(prior.value, item.value))).map(({ control }) => control.ref)));
    const containers = after.model.containers.flatMap((container) =>
        held.has(container.ref)
            ? Array.match(
                  container.controls.filter(({ ref }) => HashSet.has(changed, ref)),
                  { onEmpty: Array.empty, onNonEmpty: (controls) => [{ ...container, controls, text: [], truncated: false }] },
              )
            : [container],
    );
    return [
        ...fit({ nodes: tree(Address.index({ ...after.model, containers }), changed), call: [], depth: 0 }, 0, BUDGET),
        ...before.shown.filter(({ container }) => !kept.has(container.ref)).map(({ container, item }) => [...Option.toArray(item.token), NOUNS[container.kind][0], ...Option.toArray(Option.map(item.name, named)), 'closed'].join(' ')),
    ];
};
const Outline = { render, hits, changes, clipboard, budget: BUDGET, rows: KEPT.rows, say: SAY, call, quoted, keys, shortcut, count, named, duration, noun: (kind: Container['kind']) => NOUNS[kind][0], percent: PERCENT.format, list: (items: Iterable<string>) => LIST.format(items), scrolled, showing } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Outline };
