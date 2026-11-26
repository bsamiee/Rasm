import { Unmatched } from '@rasm/search/unmatched';
import { Array, DateTime, Equal, HashSet, Number, Option, Order, pipe, Record, Result, type Schema, Struct } from 'effect';
import { Address, type Index, type Item, type Shown } from '#/address';
import { Condition, type Container, type Control, type KeyStep, type Pasteboard } from '#/contract';

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
const KEPT = { options: 6, rows: 20 } as const;
const INDENT = '  ';
const SEPARATED = 'separator above';
const SELECTED: Readonly<Record<NonNullable<Control['selected']>, Partial<Record.ReadonlyRecord<Control['kind'], string>>>> = { attribute: { row: 'selected' }, marked: { row: 'selected (inferred)', choice: 'value inferred' } };

// --- [OPERATIONS] ----------------------------------------------------------------------

const facts = (head: string, rest: readonly string[]) => [head, ...rest].join(' · ');
const flags = (held: Readonly<Record<string, boolean>>) => Record.keys(Record.filter(held, (on) => on));
const quoted = (key: string, value: Schema.Json) => `${key}:${JSON.stringify(value)}`;
const shortcut = ({ key, modifiers }: KeyStep) => `shortcut {${[quoted('key', key), ...(Array.isReadonlyArrayNonEmpty(modifiers) ? [quoted('modifiers', modifiers)] : [])].join(',')}}`;
const help = (text: string | undefined, label: string | undefined) => Array.fromNullishOr(text).flatMap((held) => (held === label ? [] : [`help ${JSON.stringify(held)}`]));
const call = (held: readonly string[]) => `inspect {${held.join(', ')}}`;
const node = (line: string, tokens: readonly string[], rest: Partial<Node> = {}): Node => ({ line, tokens, pinned: false, cut: NEVER, children: Option.none(), ...rest });
const below = (depth: number, { at, nodes }: { readonly at: string; readonly nodes: readonly Node[] }): Page => ({ nodes, call: [quoted('in', at)], depth: depth + 1 });
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
    const named = (at: string) => Option.map(Record.get(index.located, at), Array.headNonEmpty);
    const when = Condition.match({
        Value: ({ at, value }) =>
            Option.toArray(
                pipe(
                    named(at),
                    Option.flatMap(({ name, kind }) => Option.all({ name, kind })),
                    Option.map(({ name, kind }) => `when ${name} is ${Address.value(kind, value)}`),
                ),
            ),
        Documents: ({ held }) => [held ? 'when a document is open' : 'when no document is open'],
        Modal: ({ at }) => (at === null ? ['when no dialog is open'] : Option.toArray(Option.map(Option.flatMap(named(at), Struct.get('name')), (title) => `when ${title} is open`))),
    });
    const described = ({ target: { control }, token, name, value }: (typeof listed)[number]) =>
        facts([...Option.toArray(control.mark), ...Option.toArray(Option.orElse(token, () => name)), ...Option.toArray(Option.map(value, (was) => `was ${was}`)), control.kind, ...Option.toArray(Option.map(Option.flatMap(Option.flatMap(control.opens, named), Struct.get('token')), (at) => `› ${at}`))].join(' '), [
            ...(Option.contains(control.separated, true) ? [SEPARATED] : []),
            ...Option.toArray(Option.map(control.shortcut, shortcut)),
            ...Option.toArray(Option.map(control.keyEquivalent, (text) => `keys ${JSON.stringify(text)}`)),
            ...Option.match(control.presence, {
                onNone: Array.empty,
                onSome: ({ with: seen, against }) => seen.filter((atom) => against.some((other) => other._tag === atom._tag && (atom._tag !== 'Value' || (other._tag === 'Value' && other.at === atom.at)) && !Equal.equals(other, atom))),
            }).flatMap(when),
            `seen ${control.tally.seen}×`,
        ]);
    return alternation(
        listed,
        ({ target }) => target.locator,
        ({ target }) => target.control.alternate,
    ).map(({ entry, alternates }) =>
        node(
            `▸ ${facts(
                described(entry),
                alternates.map((each) => `alternate (${described(each)})`),
            )}`,
            Option.toArray(entry.token),
            { cut: CUTS.indexOf('map') },
        ),
    );
};
const controlNode =
    (index: Index, container: Container, changed: HashSet.HashSet<Control['ref']>, leveled: boolean, within: Option.Option<string>) =>
    ({ entry: { control, item, options }, alternates, nested }: Folded & { readonly nested: readonly Folded[] }): { readonly node: Node; readonly run: Option.Option<string> } => {
        const marks = {
            [SEPARATED]: control.separated === true,
            default: container.default === control.ref,
            cancel: container.cancel === control.ref,
            close: container.close === control.ref,
            disabled: control.enabled === false,
            focused: control.focused === true,
            changed: HashSet.has(changed, control.ref),
        };
        const besides = alternates.map((each) => controlNode(index, container, changed, leveled, Option.none())({ entry: each, alternates: [], nested: [] }).node);
        const pinned = marks.default || marks.cancel || marks.close || marks.focused || marks.changed || control.selected !== undefined || (container.asking && control.kind === 'action') || besides.some(Struct.get('pinned'));
        const chosen = control.value?._tag === 'text' ? Option.some(control.value.value) : Option.none();
        const listed = options.map(({ option, item: each }) => {
            const selected = Option.contains(chosen, option.label);
            return node(facts([...Option.toArray(each.token), Option.getOrElse(each.name, () => 'unlabelled')].join(' '), [...(selected ? ['selected'] : []), ...Array.fromNullishOr(option.shortcut).map(shortcut), ...help(option.help, option.label)]), Option.toArray(each.token), { pinned: selected });
        });
        const last = Option.getOrElse(Array.findFirstIndex(listed, Struct.get('pinned')), () => -1) + KEPT.options;
        const leveling = new Set(nested.filter(({ entry }) => entry.control.kind === 'row').map(({ entry }) => entry.control.level)).size > 1;
        const lines = Array.isReadonlyArrayNonEmpty(nested) ? Array.groupWith(nested, (each, head) => each.entry.control.row === head.entry.control.ref) : [];
        const runs = (control.runs ?? []).map(({ range, text, attributes, frame }) =>
            node(facts(JSON.stringify(text), [`characters ${range.location}+${range.length}`, ...Record.collect(attributes, (key, value) => `${key} ${Address.fact('text', value)}`), ...Array.fromNullishOr(frame).map((at) => `frame ${at[0].join(',')} ${at[1].join('×')}`)]), []),
        );
        const nodes = [
            ...runs,
            ...listed.map((each, at) => ({ ...each, cut: at > last ? CUTS.indexOf('options') : NEVER })),
            ...lines.map((group, at) => ({ ...controlNode(index, container, changed, leveling, item.name)({ ...Array.headNonEmpty(group), nested: Array.tailNonEmpty(group) }).node, cut: at < KEPT.rows ? NEVER : CUTS.indexOf('rows') })),
        ];
        const head = [
            ...Array.fromNullishOr(control.mark),
            ...Option.toArray(Option.filter(item.name, (name) => !Option.contains(within, name))),
            ...Option.toArray(item.value),
            control.kind,
            ...Array.fromNullishOr(control.opens).flatMap((opens) => Option.toArray(Option.map(Option.flatMap(Record.get(index.live, `${opens}`), Struct.get('token')), (token) => `› ${token}`))),
        ];
        const described = facts(head.join(' '), [
            ...flags(marks),
            ...Array.fromNullishOr(control.selected).flatMap((evidence) => Array.fromNullishOr(SELECTED[evidence][control.kind])),
            ...Array.fromNullishOr(control.range).flatMap(({ minimum, maximum, step, wraps }) => [...(minimum === undefined && maximum === undefined ? [] : [`range ${minimum ?? '-∞'}–${maximum ?? '∞'}`]), ...Array.fromNullishOr(step).map((by) => `step ${by}`), ...(wraps === true ? ['wraps'] : [])]),
            ...Array.fromNullishOr(control.shortcut).map(shortcut),
            ...Array.fromNullishOr(control.keyEquivalent).map((text) => `keys ${JSON.stringify(text)}`),
            ...Array.fromNullishOr(control.placeholder).map((text) => `placeholder ${JSON.stringify(text)}`),
            ...Array.fromNullishOr(control.level).flatMap((level) => (leveled ? [`level ${level}`] : [])),
            ...Array.fromNullishOr(control.expanded).map((open) => (open ? 'expanded' : 'collapsed')),
            ...Array.fromNullishOr(control.disclosedBy).flatMap((up) => Option.toArray(Option.map(Option.flatMap(Record.get(index.live, `${up}`), Struct.get('token')), (token) => `under ${token}`))),
            ...Array.fromNullishOr(control.total).map((total) => `${total} rows`),
            ...Array.fromNullishOr(control.sort).map((order) => `sorted ${order}`),
            ...Array.fromNullishOr(control.column).map((column) => `column ${JSON.stringify(column)}`),
            ...Array.fromNullishOr(control.cells).map((cells) => cells.map(({ column, text }) => [...Array.fromNullishOr(column), JSON.stringify(text)].join(' ')).join(' | ')),
            ...Array.fromNullishOr(control.scroll).map(({ contentSize, horizontalScrollBar, verticalScrollBar }) =>
                ['scroll', ...Array.fromNullishOr(verticalScrollBar).map((at) => `vertical ${at}`), ...Array.fromNullishOr(horizontalScrollBar).map((at) => `horizontal ${at}`), ...Array.fromNullishOr(contentSize).map((size) => `of ${size.join('×')}`)].join(' '),
            ),
            ...Array.fromNullishOr(control.selectedTextRanges).flatMap((ranges) => (control.focused === true || ranges.some(({ length }) => length > 0) ? [`selection ${ranges.map(({ location, length }) => `${location}+${length}`).join(', ')}`] : [])),
            ...Array.fromNullishOr(control.numberOfCharacters).flatMap((count) => (control.value?._tag === 'text' && control.value.value.length === count ? [] : [`${count} characters`])),
            ...Array.fromNullishOr(control.insertionPointLineNumber).flatMap((line) => (control.focused === true ? [`line ${line}`] : [])),
            ...Array.fromNullishOr(control.actions).map((actions) => `actions ${actions.map((each) => JSON.stringify(each)).join(', ')}`),
            ...Array.fromNullishOr(control.alternateUIVisible).map((shown) => (shown ? 'hover controls shown' : 'hover controls hidden')),
            ...Array.fromNullishOr(control.invalid).map((invalid) => `invalid ${invalid}`),
            ...(control.required === true ? ['required'] : []),
            ...Array.fromNullishOr(control.current).map((current) => `current ${current}`),
            ...(control.kind === 'choice' && control.options === undefined ? ['options listed only while open'] : []),
            ...help(control.help, control.label?.text),
            ...Array.fromNullishOr(control.labels).map((labels) => `also ${labels.map((each) => JSON.stringify(each)).join(', ')}`),
            ...besides.map(({ line }) => `alternate (${line})`),
        ]);
        return {
            node: node([...Option.toArray(item.token), described].join(' '), [...Option.toArray(item.token), ...besides.flatMap(Struct.get('tokens'))], { pinned, children: Array.isReadonlyArrayNonEmpty(nodes) ? Option.map(item.token, (at) => ({ at, nodes })) : Option.none() }),
            run: Option.isNone(item.name) && Array.isReadonlyArrayEmpty(nodes) && Array.isReadonlyArrayEmpty(besides) ? Option.some(described) : Option.none(),
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
        const grouped = (Array.isReadonlyArrayNonEmpty(primaries) ? Array.groupWith(primaries, ({ entry: each }, { entry: head }) => (head.control.kind === 'multi' && (each.control.kind === 'row' || each.control.row !== undefined)) || each.control.row === head.control.ref) : []).map((group) =>
            controlNode(index, container, changed, true, Option.none())({ ...Array.headNonEmpty(group), nested: Array.tailNonEmpty(group) }),
        );
        const folded = (Array.isReadonlyArrayNonEmpty(grouped) ? Array.groupWith(grouped, (each, head) => Option.isSome(head.run) && Equal.equals(each.run, head.run)) : []).map((run) => {
            const tokens = run.flatMap((each) => each.node.tokens);
            const head = Array.headNonEmpty(run);
            return Option.match(head.run, { onNone: () => head.node, onSome: (described) => ({ ...head.node, tokens, line: `${tokens.join(',')} ${described}` }) });
        });
        const nodes = [...folded.map((each) => ({ ...each, cut: primary.has(container.ref) ? NEVER : CUTS.indexOf('containers') })), ...remembered(index, container.locator), ...container.text.map((text) => node(JSON.stringify(text), [], { cut: CUTS.indexOf('text') }))];
        const line = facts([...Option.toArray(item.token), container.kind, ...Option.toArray(Option.map(item.name, (title) => JSON.stringify(title)))].join(' '), [
            ...flags({ asking: container.asking, modal: container.modal, focused: container.focused === true, unsaved: container.edited === true, 'full screen': container.fullScreen === true, loading: container.loaded === false }),
            ...Array.fromNullishOr(container.loadingProgress).flatMap((progress) => (progress < 1 ? [`loaded ${Math.round(progress * 100)}%`] : [])),
            ...Array.fromNullishOr(container.parent)
                .flatMap((parent) => Option.toArray(Option.flatMap(Record.get(index.live, `${parent}`), Struct.get('token'))))
                .map((parent) => `in ${parent}`),
            ...Array.fromNullishOr(container.document).map((document) => `document ${document}`),
            ...help(container.help, container.title),
            ...(container.truncated ? Option.toArray(item.token).map((at) => `partly read, ${call([quoted('in', at)])} reads more`) : []),
        ]);
        return node(line, Option.toArray(item.token), { pinned: container.asking || nodes.some(Struct.get('pinned')), children: Option.map(item.token, (at) => ({ at, nodes })) });
    });
    const recalled = index.remembered.containers.flatMap(({ target, token }) =>
        target._tag === 'RememberedContainer'
            ? [
                  node(
                      `▸ ${facts(`${Option.getOrElse(token, () => target.entry.container.kind)} ${target.entry.container.kind}`, [
                          `seen ${target.entry.container.tally.seen}×, last ${DateTime.formatIsoDateUtc(target.entry.container.tally.last)}`,
                          ...(Option.exists(index.remembered.build, (build) => build !== target.entry.container.tally.verified) ? [`last in build ${target.entry.container.tally.verified}`] : []),
                          ...flags({ modal: target.entry.container.modal }),
                      ])}`,
                      Option.toArray(token),
                      { children: Option.map(token, (at) => ({ at, nodes: remembered(index, target.locator) })) },
                  ),
              ]
            : [],
    );
    return [...shown, ...recalled];
};
const scopes = (nodes: readonly Node[], depth: number): readonly { readonly at: string; readonly nodes: readonly Node[]; readonly depth: number }[] => nodes.flatMap((held) => Option.match(held.children, { onNone: Array.empty, onSome: (children) => [{ ...children, depth }, ...scopes(children.nodes, depth + 1)] }));
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
    `${INDENT.repeat(depth)}+${nodes.length - at} more · ${call([
        ...held,
        ...Option.toArray(
            Option.map(
                Option.flatMap(Array.get(nodes, at - 1), (each) => Array.last(each.tokens)),
                (token) => quoted('after', token),
            ),
        ),
    ])}`;
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
                onNone: () => Result.fail(Unmatched.of({ subject: 'container, or control with listed options or rows', query: Option.some(token), candidates: Record.keys(listed) })),
                onSome: ({ at, nodes, depth }): Result.Result<Page, Unmatched> => Result.succeed({ nodes, call: [quoted('in', at)], depth }),
            });
        },
    });
    return Result.flatMap(page, (held) => Result.map(start(held, after), (from) => fit(held, from, budget)));
};
const hits = (found: readonly Item[], held: readonly string[], { after = Option.none(), budget = BUDGET, opening = Option.none() }: { readonly after?: Option.Option<Item>; readonly budget?: typeof BUDGET; readonly opening?: Option.Option<Index> } = {}) => {
    const opener = (item: Item) => Option.flatMap(opening, (index) => Option.map(Address.opener(index, item), (openers) => Array.getSomes(openers.map(Struct.get('token'))).join(', ')));
    const page: Page = {
        nodes: found.map((item) =>
            node(
                facts(
                    [
                        ...(item.live ? [...Option.toArray(item.token), Address.path(item).join(Address.separator)] : ['▸', Option.getOrElse(item.token, () => Address.path(item).join(Address.separator))]),
                        ...Option.toArray(Option.map(item.value, (value) => (item.live ? value : `was ${value}`))),
                        Address.Target.$match(item.target, {
                            Container: ({ container }) => container.kind,
                            Control: ({ control }) => control.kind,
                            Option: () => 'option',
                            RememberedContainer: ({ entry }) => entry.container.kind,
                            RememberedControl: ({ control }) => control.kind,
                        }),
                    ].join(' '),
                    Option.toArray(Option.map(opener(item), (token) => `opened by ${token}`)),
                ),
                Option.toArray(item.token),
            ),
        ),
        call: held,
        depth: 0,
    };
    return Result.map(start(page, after), (from) => fit(page, from, budget));
};
const clipboard = ({ changeCount, types, access, patterns, contentType, content }: Pasteboard, ref: Control['ref']) =>
    facts([Address.token('clipboard', ref), 'clipboard', ...(content?._tag === 'text' ? [JSON.stringify(content.value.slice(0, BUDGET.page).toWellFormed())] : [])].join(' '), [
        `change ${changeCount}`,
        Array.isReadonlyArrayNonEmpty(types) ? `types ${types.join(', ')}` : 'empty',
        ...(Array.isReadonlyArrayNonEmpty(patterns) ? [`detects ${patterns.join(', ')}`] : []),
        ...Array.fromNullishOr(contentType).map((type) => `file of type ${type}`),
        ...(content?._tag === 'text' && content.value.length > BUDGET.page ? [`first ${BUDGET.page} of ${content.value.length} characters`] : []),
        ...(content?._tag === 'withheld' ? [`text withheld: Paste from Other Apps is ${access} for this server (System Settings > Privacy & Security > Paste from Other Apps, listed once a read raised the paste alert)`] : []),
    ]);
const Outline = { render, hits, clipboard, budget: BUDGET, rows: KEPT.rows, call, quoted, shortcut } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Outline };
