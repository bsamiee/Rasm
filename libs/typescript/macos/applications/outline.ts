import { Unmatched } from '@rasm/search';
import { Array, DateTime, Equal, HashSet, Number, Option, Order, Record, Result, type Schema, Struct } from 'effect';
import { Address, type Index, type Item, type Shown } from '#/address';
import { Condition, type Container, type Control, type KeyStep, type Kind, type Pasteboard, type Ref } from '#/contract';

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
const SELECTED: Readonly<Record<NonNullable<(typeof Control.Type)['selected']>, Partial<Record.ReadonlyRecord<typeof Kind.Type, string>>>> = { attribute: { row: 'selected' }, marked: { row: 'selected (inferred)', choice: 'value inferred' } };

// --- [OPERATIONS] ----------------------------------------------------------------------

const facts = (head: string, rest: readonly string[]) => [head, ...rest].join(' · ');
const flags = (held: Readonly<Record<string, boolean>>) => Record.keys(Record.filter(held, (on) => on));
const quoted = (key: string, value: Schema.Json) => `${key}:${JSON.stringify(value)}`;
const shortcut = ({ key, modifiers }: typeof KeyStep.Type) => `shortcut {key:{${quoted('key', key)},${quoted('modifiers', modifiers)}}}`;
const call = (held: readonly string[]) => `inspect {${held.join(', ')}}`;
const node = (line: string, tokens: readonly string[], rest: Partial<Node> = {}): Node => ({ line, tokens, pinned: false, cut: NEVER, children: Option.none(), ...rest });
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
    const described = ({ target: { control }, name }: (typeof listed)[number]) =>
        facts([...Option.toArray(control.mark), ...Option.toArray(name), control.kind, ...Option.toArray(Option.flatMap(control.opens, (opens) => Option.map(Option.flatMap(Record.get(index.located, opens), Struct.get('token')), (token) => `› ${token}`)))].join(' '), [
            ...(Option.contains(control.separated, true) ? [SEPARATED] : []),
            ...Option.toArray(Option.map(control.shortcut, shortcut)),
        ]);
    return alternation(
        listed,
        ({ target }) => target.locator,
        ({ target }) => target.control.alternate,
    ).map(({ entry, alternates }) => {
        const { target, token, name, value } = entry;
        return node(
            target.entry.container.kind === 'menu'
                ? facts(described(entry), [...alternates.map((each) => `alternate (${described(each)})`), 'not read now'])
                : facts(['▸', ...Option.toArray(name), ...Option.toArray(Option.map(value, (was) => `was ${was}`)), target.control.kind].join(' '), [
                      ...Option.match(target.control.presence, {
                          onNone: Array.empty,
                          onSome: ({ with: seen, against }) => seen.filter((atom) => against.some((other) => other._tag === atom._tag && (atom._tag !== 'Value' || (other._tag === 'Value' && other.at === atom.at)) && !Equal.equals(other, atom))),
                      }).flatMap((atom) =>
                          Condition.match(atom, {
                              Value: ({ at, value: held }) =>
                                  Option.toArray(
                                      Record.get(index.located, at).pipe(
                                          Option.flatMap(({ name: label, kind }) => Option.all({ label, kind })),
                                          Option.map(({ label, kind }) => `when ${label} is ${Address.value(kind, held)}`),
                                      ),
                                  ),
                              Documents: ({ held }) => [held ? 'when a document is open' : 'when no document is open'],
                              Modal: ({ at }) => (at === null ? ['when no dialog is open'] : Option.toArray(Option.map(Option.flatMap(Record.get(index.located, at), Struct.get('name')), (title) => `when ${title} is open`))),
                          }),
                      ),
                      `seen ${target.control.tally.seen}×`,
                  ]),
            Option.toArray(token),
            { cut: CUTS.indexOf('map') },
        );
    });
};
const controlNode =
    (index: Index, container: typeof Container.Type, changed: HashSet.HashSet<typeof Ref.Type>, leveled: boolean, within: Option.Option<string>) =>
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
        const chosen = control.value !== undefined && 'text' in control.value ? Option.some(control.value.text.value) : Option.none();
        const listed = options.map(({ option, item: each }) => {
            const selected = Option.contains(chosen, option.label);
            return node(facts([...Option.toArray(each.token), Option.getOrElse(each.name, () => 'unlabelled')].join(' '), selected ? ['selected'] : []), Option.toArray(each.token), { pinned: selected });
        });
        const last = Option.getOrElse(Array.findFirstIndex(listed, Struct.get('pinned')), () => -1) + KEPT.options;
        const leveling = new Set(nested.filter(({ entry }) => entry.control.kind === 'row').map(({ entry }) => entry.control.level)).size > 1;
        const lines = Array.isReadonlyArrayNonEmpty(nested) ? Array.groupWith(nested, (each, head) => each.entry.control.row === head.entry.control.ref) : [];
        const nodes = [
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
            ...Array.fromNullishOr(control.range).flatMap(({ minimum, maximum, step }) => [...(minimum === undefined && maximum === undefined ? [] : [`range ${minimum ?? '-∞'}–${maximum ?? '∞'}`]), ...Array.fromNullishOr(step).map((by) => `step ${by}`)]),
            ...Array.fromNullishOr(control.shortcut).map(shortcut),
            ...Array.fromNullishOr(control.placeholder).map((text) => `placeholder ${JSON.stringify(text)}`),
            ...Array.fromNullishOr(control.level).flatMap((level) => (leveled ? [`level ${level}`] : [])),
            ...Array.fromNullishOr(control.expanded).map((open) => (open ? 'expanded' : 'collapsed')),
            ...(control.kind === 'choice' && control.options === undefined ? ['options listed only while open'] : []),
            ...Array.fromNullishOr(control.help)
                .filter((help) => help !== control.label?.text)
                .map((help) => `help ${JSON.stringify(help)}`),
            ...besides.map(({ line }) => `alternate (${line})`),
        ]);
        return {
            node: node([...Option.toArray(item.token), described].join(' '), [...Option.toArray(item.token), ...besides.flatMap(Struct.get('tokens'))], { pinned, children: Array.isReadonlyArrayNonEmpty(nodes) ? Option.map(item.token, (at) => ({ at, nodes })) : Option.none() }),
            run: Option.isNone(item.name) && Array.isReadonlyArrayEmpty(nodes) && Array.isReadonlyArrayEmpty(besides) ? Option.some(described) : Option.none(),
        };
    };
const tree = (index: Index, changed: HashSet.HashSet<typeof Ref.Type>): readonly Node[] => {
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
            ...flags({ asking: container.asking, modal: container.modal, focused: container.focused === true }),
            ...Array.fromNullishOr(container.parent)
                .flatMap((parent) => Option.toArray(Option.flatMap(Record.get(index.live, `${parent}`), Struct.get('token'))))
                .map((parent) => `in ${parent}`),
            ...Array.fromNullishOr(container.document).map((document) => `document ${document}`),
            ...(container.truncated ? Option.toArray(item.token).map((at) => `partly read, ${call([quoted('in', at)])} reads more`) : []),
        ]);
        return node(line, Option.toArray(item.token), { pinned: container.asking || nodes.some(Struct.get('pinned')), children: Option.map(item.token, (at) => ({ at, nodes })) });
    });
    const recalled = index.remembered.containers.flatMap(({ target, token, name, ancestors }) =>
        target._tag === 'RememberedContainer'
            ? [
                  node(
                      facts(`▸ ${target.entry.container.kind} ${JSON.stringify(Option.getOrElse(name, () => target.entry.container.kind))}`, [
                          `seen ${target.entry.container.tally.seen}×, last ${DateTime.formatIsoDateUtc(target.entry.container.tally.last)}`,
                          ...flags({ modal: target.entry.container.modal }),
                          ...(Array.isReadonlyArrayNonEmpty(ancestors) ? [`opened by ${ancestors.join(Address.separator)}`] : []),
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
const print =
    ({ nodes, call: held, depth }: Page, from: number, top: boolean, level: number, budget: typeof BUDGET) =>
    (used: number): { readonly lines: readonly string[]; readonly used: number } => {
        const indent = INDENT.repeat(depth);
        const groups = Array.chop(nodes.slice(from), (rest) => {
            const [loose, tail] = Array.span(rest, (each) => Array.isReadonlyArrayEmpty(each.tokens));
            return [[...loose, ...Array.take(tail, 1)], Array.drop(tail, 1)] as const;
        });
        const pinned = Option.getOrElse(Array.findLastIndex(nodes, Struct.get('pinned')), () => -1) + 1;
        const wanted = Math.max(
            Option.getOrElse(
                Array.findFirstIndex(nodes, (each) => each.cut < level),
                () => nodes.length,
            ),
            pinned,
            from + (top ? (groups[0]?.length ?? 0) : 0),
        );
        const [state, chunks] = Array.mapAccum(groups, { start: from, used, cut: Option.none<number>() }, (spent, group) => {
            const end = spent.start + group.length;
            const cost = Number.sumAll(group.map((each) => indent.length + each.line.length + 1));
            const fits = Option.isNone(spent.cut) && end <= wanted && ((top && spent.start === from) || spent.used + cost <= budget.page || (spent.start < pinned && spent.used + cost <= budget.ceiling));
            const [total, lines] = fits
                ? Array.mapAccum(group, spent.used, (sum, each) => {
                      const line = `${indent}${each.line}`;
                      const inner = Option.match(each.children, {
                          onNone: () => ({ lines: [], used: sum + line.length + 1 }),
                          onSome: ({ at, nodes: within }) => print({ nodes: within, call: [quoted('in', at)], depth: depth + 1 }, 0, false, level, budget)(sum + line.length + 1),
                      });
                      return [inner.used, [line, ...inner.lines]] as const;
                  })
                : ([spent.used, []] as const);
            return [{ start: end, used: total, cut: fits ? spent.cut : Option.orElse(spent.cut, () => Option.some(spent.start)) }, lines.flat()] as const;
        });
        const continued = Option.map(state.cut, (at) => {
            const after = Option.flatMap(Array.get(nodes, at - 1), (each) => Array.last(each.tokens));
            return `${indent}+${nodes.length - at} more · ${call([...held, ...Option.toArray(Option.map(after, (token) => quoted('after', token)))])}`;
        });
        return { lines: [...chunks.flat(), ...Option.toArray(continued)], used: state.used + Option.match(continued, { onNone: () => 0, onSome: (line) => line.length + 1 }) };
    };
const fit = (page: Page, from: number, budget: typeof BUDGET) =>
    Option.getOrElse(
        Array.findFirst(CUTS, (_cut, level) => Option.liftPredicate(print(page, from, true, level, UNBOUNDED)(0), ({ used }) => used <= budget.page)),
        () => print(page, from, true, CUTS.length, budget)(0),
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
    { in: within = Option.none(), after = Option.none(), changed = HashSet.empty(), budget = BUDGET }: { readonly in?: Option.Option<Item>; readonly after?: Option.Option<Item>; readonly changed?: HashSet.HashSet<typeof Ref.Type>; readonly budget?: typeof BUDGET } = {},
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
const hits = (found: readonly Item[], held: readonly string[], { after = Option.none(), budget = BUDGET }: { readonly after?: Option.Option<Item>; readonly budget?: typeof BUDGET } = {}) => {
    const page: Page = {
        nodes: found.map((item) =>
            node(
                [
                    ...(item.live ? Option.toArray(item.token) : ['▸']),
                    Address.path(item).join(Address.separator),
                    ...Option.toArray(Option.map(item.value, (value) => (item.live ? value : `was ${value}`))),
                    Address.Target.$match(item.target, {
                        Container: ({ container }) => container.kind,
                        Control: ({ control }) => control.kind,
                        Option: () => 'option',
                        RememberedContainer: ({ entry }) => entry.container.kind,
                        RememberedControl: ({ control }) => control.kind,
                    }),
                ].join(' '),
                Option.toArray(item.token),
            ),
        ),
        call: held,
        depth: 0,
    };
    return Result.map(start(page, after), (from) => fit(page, from, budget));
};
const clipboard = ({ changeCount, types, access, patterns, contentType, content }: typeof Pasteboard.Type, ref: typeof Ref.Type) =>
    facts([Address.token('clipboard', ref), 'clipboard', ...(content?._tag === 'text' ? [JSON.stringify(content.text.value.slice(0, BUDGET.page).toWellFormed())] : [])].join(' '), [
        `change ${changeCount}`,
        Array.isReadonlyArrayNonEmpty(types) ? `types ${types.join(', ')}` : 'empty',
        ...(Array.isReadonlyArrayNonEmpty(patterns) ? [`detects ${patterns.join(', ')}`] : []),
        ...Array.fromNullishOr(contentType).map((type) => `file of type ${type}`),
        ...(content?._tag === 'text' && content.text.value.length > BUDGET.page ? [`first ${BUDGET.page} of ${content.text.value.length} characters`] : []),
        ...(content?._tag === 'withheld' ? [`text withheld as Paste from Other Apps is ${access} for this server, System Settings > Privacy & Security > Paste from Other Apps allows it and lists the server once a read raised the paste alert`] : []),
    ]);
const Outline = { render, hits, clipboard, budget: BUDGET, rows: KEPT.rows, call, quoted } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Outline };
