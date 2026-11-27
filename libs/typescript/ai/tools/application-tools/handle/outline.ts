import type { Act } from '@rasm/applications/server/act';
import { ApplicationServer } from '@rasm/applications/server/client';
import { Element } from '@rasm/applications/server/element';
import { Instance } from '@rasm/applications/server/instance';
import { Call, Format, Line, Report } from '@rasm/mcp/report';
import { Handle } from '@rasm/mcp/server';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Effect, Equal, Function, HashSet, Match, Option, Order, Predicate, pipe, Record, Result, String, Struct, UndefinedOr } from 'effect';
import { Address, type Index, type Item, type Shown } from '#/handle/address';
import { Target } from '#/handle/target';

// --- [TYPES] ---------------------------------------------------------------------------

type Control = typeof Element.Control.Type;
type Pasteboard = typeof Act.Pasteboard.Type;
interface Folded {
    readonly entry: Shown['controls'][number];
    readonly alternates: Shown['controls'];
}
interface Draft {
    readonly token: Option.Option<string>;
    readonly head: readonly string[];
    readonly value: Option.Option<string>;
    readonly facts: readonly string[];
    readonly pinned: boolean;
    readonly children: readonly Line[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const CUTS = ['text', 'map', 'options', 'rows', 'containers'] as const;
const KEPT = { options: 6, rows: 20, words: 6, showing: 8 } as const;
const WORDS = new Intl.Segmenter(undefined, { granularity: 'word' });
const SPACES = /\s+/gu;
const NAMING = /[\p{L}\p{N}]/u;
const ROLES = { group: 'AXGroup', titleBar: new Set(['AXCloseButton', 'AXMinimizeButton', 'AXZoomButton', 'AXFullScreenButton']) } as const;
const SAY = { asking: 'asks for an answer', gone: 'not shown now', closed: 'closed', separated: 'separator above', front: 'in front' } as const;
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
    item: { one: 'item', other: 'items' },
    row: { one: 'row', other: 'rows' },
    character: { one: 'character', other: 'characters' },
    popUp: { one: 'pop-up', other: 'pop-ups' },
    match: { one: 'match', other: 'matches' },
    container: { one: 'container', other: 'containers' },
} as const satisfies Record<(typeof Element.Container.Type)['kind'] | 'control' | 'item' | 'row' | 'character' | 'popUp' | 'match' | 'container', { readonly one: string; readonly other: string }>;
const UNSPOKEN: ReadonlySet<(typeof Element.Control.Type)['kind']> = new Set(['action', 'item']);
const SELECTED: Readonly<Record<NonNullable<(typeof Element.Control.Type)['selected']>, Partial<Record.ReadonlyRecord<(typeof Element.Control.Type)['kind'], string>>>> = { attribute: { row: 'selected' }, marked: { row: 'selected, inferred', choice: 'value inferred' } };

// --- [OPERATIONS] ----------------------------------------------------------------------

const flags = (held: Readonly<Record<string, boolean>>) => Record.keys(Record.filter(held, (on) => on));
const spaced = (text: string) => text.replaceAll(SPACES, ' ').trim();
const texted = ({ value }: Control) => (value?._tag === 'text' ? Option.some(value.value) : Option.none<string>());
const scrolled = (vertical: number | undefined, horizontal: number | undefined) => [...Array.fromNullishOr(vertical).map((at) => `scrolled ${Format.percent(at)} down`), ...Array.fromNullishOr(horizontal).map((at) => `scrolled ${Format.percent(at)} across`)];
const showing = ({ rows, showing: shown }: { readonly rows?: number; readonly showing: readonly { readonly index: number; readonly label?: string }[] }) =>
    Array.match(shown, {
        onEmpty: Array.empty,
        onNonEmpty: (held) => {
            const span = [`rows ${Array.headNonEmpty(held).index + 1} to ${Array.lastNonEmpty(held).index + 1}`, ...Array.fromNullishOr(rows).map((total) => `of ${total}`)].join(' ');
            const labels = [...Array.take(held, KEPT.showing).map(({ index, label }) => UndefinedOr.match(label, { onUndefined: () => `row ${index + 1}`, onDefined: Format.quoted })), ...(held.length > KEPT.showing ? [`${held.length - KEPT.showing} more`] : [])];
            return [`${span}: ${labels.join(', ')}`];
        },
    });
const compose = (head: readonly string[], value: Option.Option<string>, stated: readonly string[]) => [[head.join(' '), ...Option.toArray(value)].filter(String.isNonEmpty).join(': '), ...stated].filter(String.isNonEmpty).join(', ');
const spoken = (kind: (typeof Element.Control.Type)['kind']) => (UNSPOKEN.has(kind) ? [] : [kind]);
const keys = ({ key, modifiers }: typeof Element.Shortcut.Type) => [...modifiers.map(String.capitalize), key].join('-');
const shortcut = (step: typeof Element.Shortcut.Type) => `shortcut ${keys(step)}`;
const help = (text: string | undefined, name: string | undefined, whole: boolean) => Array.fromNullishOr(text).flatMap((held) => (held !== name && (whole || name === undefined || !NAMING.test(name)) ? [`help: ${Format.quoted(held)}`] : []));
const inspect = (given: Call['given']) => new Call({ tool: 'inspect', given });
const opensOf = (token: Option.Option<string>) => Option.toArray(Option.map(token, (at) => `opens ${at}`));
const caption = (kind: (typeof Element.Container.Type)['kind'], ref?: number, title?: string) => [...Array.fromNullishOr(ref).map((at) => Address.token(kind, at)), NOUNS[kind].one, ...Array.fromNullishOr(title).map(Format.quoted)];
const under = (each: Control, head: Control) => (head.kind === 'multi' && (each.kind === 'row' || each.row !== undefined)) || each.row === head.ref;
const stating = Function.memoize(({ container, controls }: Shown) => {
    const names = controls.flatMap(({ item, options }) => [...Option.toArray(item.name), ...options.flatMap(({ item: each }) => Option.toArray(each.name))]);
    const values = controls.flatMap(({ control: { value } }) => (value?._tag === 'text' || value?._tag === 'integer' || value?._tag === 'real' ? [spaced(`${value.value}`)] : []));
    return { said: new Set([...names, ...values]), parts: Array.dedupe([...names, ...values, ...container.text.map(spaced)]).filter(String.isNonEmpty) };
});
const joins = (name: string, parts: readonly string[]) => {
    const held = parts.filter((part) => part.length < name.length && name.includes(part)).toSorted((left, right) => right.length - left.length);
    return held.length > 1 && !NAMING.test(held.reduce((rest, part) => rest.replaceAll(part, ' '), name));
};
const siblings = (drafts: readonly Draft[], noun: string) => {
    const common = Array.match(drafts, {
        onEmpty: () => new Set<string>(),
        onNonEmpty: (all) => (Predicate.isTupleOf(all, 1) ? new Set<string>() : Array.tailNonEmpty(all).reduce((kept, each) => kept.intersection(new Set(each.facts)), new Set(Array.headNonEmpty(all).facts))),
    });
    const composed = drafts.map((held) => {
        const stated = held.facts.filter((fact) => !common.has(fact));
        return { held, stated, described: compose(held.head, held.value, stated) };
    });
    const leaves = composed.filter(({ held }) => Array.isReadonlyArrayEmpty(held.children));
    const alike = Array.groupBy(leaves, Struct.get('described'));
    return {
        all: Array.fromIterable(common).map((fact) => `all ${noun} ${fact}`),
        rows: composed.flatMap((entry) => {
            const { held, stated, described } = entry;
            const same = Array.isReadonlyArrayEmpty(held.children) ? Option.getOrElse(Record.get(alike, described), () => [entry]) : [entry];
            return Option.exists(Array.head(same), (first) => first === entry)
                ? [
                      new Line({
                          head: [...same.flatMap((each) => Option.toArray(each.held.token)), ...held.head],
                          facts: stated,
                          pinned: same.some((each) => each.held.pinned),
                          ...Option.getOrUndefined(Option.map(held.value, (value) => ({ value }))),
                          ...(Array.isReadonlyArrayEmpty(held.children) ? {} : Option.getOrUndefined(Option.map(held.token, (at) => ({ rows: held.children, call: inspect({ in: at }) })))),
                      }),
                  ]
                : [];
        }),
    };
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
const remembered = (index: Index, locator: string): readonly Line[] => {
    const listed = Option.getOrElse(Record.get(index.remembered.controls, locator), Array.empty).flatMap(({ target, ...item }) => (target._tag === 'RememberedControl' ? [{ ...item, target }] : []));
    const located = (at: string) => Option.map(Record.get(index.located, at), Array.headNonEmpty);
    const when = Element.Condition.match({
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
    const described = ({ target: { control }, token, name, value }: (typeof listed)[number]) => ({
        head: [...Option.toArray(Option.orElse(token, () => name)), ...spoken(control.kind)],
        stated: [
            SAY.gone,
            ...Option.toArray(Option.map(value, (was) => `last value: ${was}`)),
            ...(Option.isSome(control.mark) ? ['checked'] : []),
            ...opensOf(Option.flatMap(Option.flatMap(control.opens, located), Struct.get('token'))),
            ...(Option.contains(control.separated, true) ? [SAY.separated] : []),
            ...Option.toArray(Option.map(control.shortcut, shortcut)),
            ...Option.toArray(Option.map(control.keyEquivalent, (text) => `key equivalent: ${Format.quoted(text)}`)),
            ...Option.match(control.presence, {
                onNone: Array.empty,
                onSome: ({ with: seen, against }) => seen.filter((atom) => against.some((other) => other._tag === atom._tag && (atom._tag !== 'Value' || (other._tag === 'Value' && other.at === atom.at)) && !Equal.equals(other, atom))),
            }).flatMap(when),
        ],
    });
    return alternation(
        listed,
        ({ target }) => target.locator,
        ({ target }) => target.control.alternate,
    ).map(({ entry, alternates }) => {
        const { head, stated } = described(entry);
        return new Line({ head, facts: [...stated, ...alternates.map(described).map((other) => `alternate (${compose(other.head, Option.none(), other.stated)})`)], cut: CUTS.indexOf('map') });
    });
};
const draft =
    (index: Index, shown: Shown, changed: HashSet.HashSet<(typeof Element.Control.Type)['ref']>, { leveled = false, within = Option.none<string>(), whole = false } = {}) =>
    ({ entry: { control, item, options }, alternates, nested }: Folded & { readonly nested: readonly Folded[] }): Draft => {
        const { container } = shown;
        const marks = {
            [SAY.separated]: control.separated === true,
            default: container.default === control.ref,
            cancel: container.cancel === control.ref,
            disabled: control.enabled === false,
            focused: control.focused === true,
            changed: HashSet.has(changed, control.ref),
        };
        const besides = alternates.map((each) => draft(index, shown, changed)({ entry: each, alternates: [], nested: [] }));
        const chosen = texted(control);
        const last = options.findIndex(({ option }) => Option.contains(chosen, option.label)) + KEPT.options;
        const listed = options.map(({ option, item: each }, at) => {
            const selected = Option.contains(chosen, option.label);
            return new Line({
                head: [...Option.toArray(each.token), ...Option.toArray(Option.map(each.name, Format.quoted))],
                facts: [...(selected ? ['selected'] : []), ...Array.fromNullishOr(option.shortcut).map(shortcut), ...help(option.help, option.label, whole)],
                pinned: selected,
                ...(at > last ? { cut: CUTS.indexOf('options') } : {}),
            });
        });
        const groups = control.kind !== 'row' && Array.isReadonlyArrayNonEmpty(nested) ? Array.groupWith(nested, (each, head) => under(each.entry.control, head.entry.control)) : [];
        const columns = Array.dedupe(Option.getOrElse(Array.head(groups), Array.empty).flatMap(({ entry }) => Array.fromNullishOr(entry.control.column)));
        const disclosed = groups.some((group) => Array.headNonEmpty(group).entry.control.disclosedBy !== undefined);
        const leveling = !disclosed && new Set(groups.map((group) => Array.headNonEmpty(group).entry.control.level)).size > 1;
        const rows = groups.map((group) => ({ control: Array.headNonEmpty(group).entry.control, drafted: draft(index, shown, changed, { leveled: leveling, within: item.name })({ ...Array.headNonEmpty(group), nested: Array.tailNonEmpty(group) }) }));
        const present = new Set(rows.map(({ control: { ref } }) => ref));
        const beneath: Record.ReadonlyRecord<string, readonly (typeof rows)[number][]> = Array.groupBy(
            rows.filter(({ control: { disclosedBy } }) => disclosedBy !== undefined && present.has(disclosedBy)),
            ({ control: { disclosedBy } }): string => `${disclosedBy}`,
        );
        const arrange = (level: readonly (typeof rows)[number][]): ReturnType<typeof siblings> =>
            siblings(
                level.map(({ control: { ref }, drafted }) => {
                    const inner = arrange(Option.getOrElse(Record.get(beneath, `${ref}`), Array.empty));
                    return { ...drafted, facts: [...drafted.facts, ...inner.all], children: [...drafted.children, ...inner.rows] };
                }),
                NOUNS.row.other,
            );
        const arranged = arrange(
            rows
                .filter(({ control: { disclosedBy } }) => disclosedBy === undefined || !present.has(disclosedBy))
                .map((row) => ({ ...row, drafted: { ...row.drafted, facts: [...row.drafted.facts, ...Array.fromNullishOr(row.control.disclosedBy).flatMap((up) => Option.toArray(Option.map(Option.flatMap(Record.get(index.live, `${up}`), Struct.get('token')), (token) => `under ${token}`)))] } })),
        );
        const runs = (control.runs ?? []).map(({ range, text, attributes }) => new Line({ head: [], value: `"${text}"`, facts: [`characters ${range.location} to ${range.location + range.length}`, ...Record.collect(attributes, (key, value) => `${key}: ${Address.fact('text', value)}`)] }));
        const cells = control.kind === 'row' ? nested.map(Struct.get('entry')) : [];
        const inline = cells.map((cell) => (cell.control.kind === 'text' && cell.control.settable !== true ? Option.match(texted(cell.control), { onNone: () => 'empty', onSome: Format.quoted }) : compose([...Option.toArray(cell.item.token), ...spoken(cell.control.kind)], cell.item.value, [])));
        const label = Option.filter(item.name, (name) => !Option.contains(within, name) && !Option.exists(Array.head(cells), (cell) => Option.contains(texted(cell.control), name)));
        const naming = control.role === ROLES.group && Option.isNone(item.value) && Option.exists(label, (name) => joins(name, stating(shown).parts)) ? ['group'] : Option.toArray(Option.map(label, Format.quoted));
        const named = control.label !== undefined && Predicate.hasProperty(control.label, 'attribute') && control.label.attribute === 'AXRoleDescription' ? undefined : control.label?.text;
        return {
            token: item.token,
            head: [...spoken(control.kind), ...Array.match(inline, { onEmpty: () => naming, onNonEmpty: (values) => [[...naming, ...values].join(', ')] })],
            value: Option.filter(item.value, () => Array.isReadonlyArrayEmpty(inline)),
            pinned: marks.default || marks.cancel || marks.focused || marks.changed || control.selected !== undefined || (container.asking && control.kind === 'action') || besides.some(Struct.get('pinned')),
            children: [...runs, ...listed, ...arranged.rows.map((each, at) => (at < KEPT.rows ? each : new Line({ ...each, cut: CUTS.indexOf('rows') })))],
            facts: [
                ...(control.mark === undefined ? [] : ['checked']),
                ...Array.fromNullishOr(control.opens).flatMap((opens) => opensOf(Option.flatMap(Record.get(index.live, `${opens}`), Struct.get('token')))),
                ...flags(marks),
                ...Array.fromNullishOr(control.selected).flatMap((evidence) => Array.fromNullishOr(SELECTED[evidence][control.kind])),
                ...Array.fromNullishOr(control.range).flatMap(({ minimum, maximum, step, wraps }) => [
                    ...Match.value({ minimum, maximum }).pipe(
                        Match.when({ minimum: Match.number, maximum: Match.number }, ({ minimum: low, maximum: high }) => [`range ${Format.count(low)} to ${Format.count(high)}`]),
                        Match.when({ minimum: Match.number }, ({ minimum: low }) => [`at least ${Format.count(low)}`]),
                        Match.when({ maximum: Match.number }, ({ maximum: high }) => [`at most ${Format.count(high)}`]),
                        Match.orElse((): readonly string[] => []),
                    ),
                    ...(whole ? [...Array.fromNullishOr(step).map((by) => `step ${Format.count(by)}`), ...flags({ wraps: wraps === true })] : []),
                ]),
                ...Array.fromNullishOr(control.shortcut).map(shortcut),
                ...Array.fromNullishOr(control.keyEquivalent).map((text) => `key equivalent: ${Format.quoted(text)}`),
                ...Array.fromNullishOr(control.placeholder)
                    .filter(String.isNonEmpty)
                    .map((text) => `placeholder: ${Format.quoted(text)}`),
                ...Array.fromNullishOr(control.level).flatMap((level) => (leveled ? [`level ${level}`] : [])),
                ...Array.fromNullishOr(control.expanded).map((open) => (open ? 'expanded' : 'collapsed')),
                ...Array.fromNullishOr(control.total).map((total) => Format.count(total, NOUNS.row)),
                ...arranged.all,
                ...Array.match(columns, { onEmpty: Array.empty, onNonEmpty: (titles) => [`columns: ${titles.map(Format.quoted).join(', ')}`] }),
                ...Array.fromNullishOr(control.sort).map((order) => `sorted ${order}`),
                ...Array.fromNullishOr(control.column).map((column) => `column: ${Format.quoted(column)}`),
                ...(control.cells ?? []).map(({ column, text }) => UndefinedOr.match(column, { onUndefined: () => Format.quoted(text), onDefined: (title) => `${title}: ${Format.quoted(text)}` })),
                ...Array.fromNullishOr(control.scroll).flatMap(({ horizontalScrollBar, verticalScrollBar }) => scrolled(verticalScrollBar === 0 ? undefined : verticalScrollBar, horizontalScrollBar === 0 ? undefined : horizontalScrollBar)),
                ...(control.selectedTextRanges ?? []).flatMap(({ location, length }) => (whole || length > 0 ? [[Format.count(length, NOUNS.character), 'selected', ...(whole ? [`at ${location}`] : [])].join(' ')] : [])),
                ...Array.fromNullishOr(control.numberOfCharacters).flatMap((total) => (Option.exists(chosen, (text) => text.length === total) ? [] : [Format.count(total, NOUNS.character)])),
                ...Array.fromNullishOr(control.invalid).map((invalid) => `invalid: ${invalid}`),
                ...flags({ required: control.required === true }),
                ...Array.fromNullishOr(control.current).map((current) => `current: ${current}`),
                ...help(control.help, named, whole),
                ...besides.map((each) => `alternate (${[...Option.toArray(each.token), compose(each.head, each.value, each.facts)].join(' ')})`),
                ...(whole
                    ? [
                          ...Array.fromNullishOr(control.actions).map((actions) => `actions: ${actions.map(Format.quoted).join(', ')}`),
                          ...Array.fromNullishOr(control.frame).map((frame) => `frame ${Address.fact(control.kind, { _tag: 'rect', value: frame })}`),
                          ...Array.fromNullishOr(control.scroll?.contentSize).map(([wide, high]) => `content ${Format.count(wide)} by ${Format.count(high)}`),
                          ...Array.fromNullishOr(control.insertionPointLineNumber).map((line) => `line ${line}`),
                          ...Array.fromNullishOr(control.labels).map((labels) => `also labelled ${labels.map(Format.quoted).join(', ')}`),
                          ...flags({ 'hover controls shown': control.alternateUIVisible === true }),
                      ]
                    : []),
            ],
        };
    };
const tree = (index: Index, changed: HashSet.HashSet<(typeof Element.Control.Type)['ref']>, gone: Record.ReadonlyRecord<string, readonly string[]> = {}): readonly Line[] => {
    const { containers } = index.model;
    const primary = new Set(Option.getOrElse(Array.findFirst([containers.filter(Struct.get('asking')), containers.filter((container) => container.focused === true), Array.take(containers, 1)], Array.isReadonlyArrayNonEmpty), Array.empty).map(Struct.get('ref')));
    const shown = Array.sortWith(index.shown, ({ container }) => [container.asking, container.focused === true] as const, Order.flip(Order.Tuple([Order.Boolean, Order.Boolean]))).map((held) => {
        const { container, item, controls } = held;
        const primaries = alternation(
            controls.filter(({ control: { subrole } }) => subrole === undefined || !ROLES.titleBar.has(subrole)),
            ({ control }) => `${control.ref}`,
            ({ control }) => Option.map(Option.fromUndefinedOr(control.alternate), (ref) => `${ref}`),
        );
        const folded = siblings(
            (Array.isReadonlyArrayNonEmpty(primaries) ? Array.groupWith(primaries, (each, head) => under(each.entry.control, head.entry.control)) : []).map((group) => draft(index, held, changed)({ ...Array.headNonEmpty(group), nested: Array.tailNonEmpty(group) })),
            NOUNS[primaries.every(({ entry }) => entry.control.kind === 'item') ? 'item' : 'control'].other,
        );
        const rows = [
            ...folded.rows.map((each) => (primary.has(container.ref) ? each : new Line({ ...each, cut: CUTS.indexOf('containers') }))),
            ...remembered(index, container.locator),
            ...container.text.filter((text) => !stating(held).said.has(spaced(text))).map((text) => new Line({ head: [], value: `"${text}"`, cut: CUTS.indexOf('text') })),
            ...Array.match(gone[container.ref] ?? [], { onEmpty: Array.empty, onNonEmpty: (tokens) => [new Line({ head: ['gone'], value: tokens.join(', ') })] }),
        ];
        return new Line({
            head: caption(container.kind, container.ref, Option.getOrUndefined(item.name)),
            facts: [
                ...flags({
                    focused: container.focused === true,
                    modal: container.modal,
                    [SAY.asking]: container.asking,
                    unsaved: container.edited === true,
                    'full screen': container.fullScreen === true,
                    loading: container.loaded === false,
                    [SAY.closed]: container.closed,
                }),
                ...Array.fromNullishOr(container.close).flatMap((ref) => Option.toArray(Option.map(Option.flatMap(Record.get(index.live, `${ref}`), Struct.get('token')), (token) => `close ${token}`))),
                ...Array.fromNullishOr(container.loadingProgress).flatMap((progress) => (progress < 1 ? [`loaded ${Format.percent(progress)}`] : [])),
                ...Array.fromNullishOr(container.document).map((document) => `document: ${document}`),
                ...help(container.help, container.title, false),
                ...folded.all,
                ...Option.toArray(item.token).flatMap((at) => [...(container.unreadable ? [`content not exposed - ${Format.call(new Call({ tool: 'view', given: { of: at } }))}`] : []), ...(container.truncated ? [`partly read - ${Format.call(inspect({ in: at }))}`] : [])]),
            ],
            pinned: container.asking || rows.some((each) => each.pinned === true),
            ...Option.getOrUndefined(Option.map(item.token, (at) => ({ rows, call: inspect({ in: at }) }))),
        });
    });
    const recalled = index.remembered.containers.flatMap(({ target, token }) =>
        target._tag === 'RememberedContainer'
            ? [
                  new Line({
                      head: [...Option.toArray(token), NOUNS[target.entry.container.kind].one],
                      facts: [
                          SAY.gone,
                          ...(Option.isSome(index.remembered.build) && !Equal.equals(index.remembered.build, target.entry.container.tally.verified) ? [Option.match(target.entry.container.tally.verified, { onNone: () => 'last seen in an unversioned build', onSome: (build) => `last seen in build ${build}` })] : []),
                          ...flags({ modal: target.entry.container.modal }),
                      ],
                      ...Option.getOrUndefined(Option.map(token, (at) => ({ rows: remembered(index, target.locator), call: inspect({ in: at }) }))),
                  }),
              ]
            : [],
    );
    return [...shown, ...recalled];
};
const flattened = (lines: readonly Line[]): readonly Line[] => lines.flatMap((held) => [held, ...flattened(held.rows ?? [])]);
const paged = (page: Line, after: Option.Option<Item>): Result.Result<readonly Line[], Unmatched> => {
    const rows = page.rows ?? [];
    const from = Option.match(Option.flatMap(after, Struct.get('token')), {
        onNone: () => Result.succeed(0),
        onSome: (token) => {
            const at = rows.findIndex(({ head }) => Option.contains(Array.head(head), token));
            return at === -1 ? Result.fail(Unmatched.of({ subject: ['item', ...Array.fromNullishOr(page.call).map((held) => `of ${Format.call(held)}`)].join(' '), query: Option.some(token), candidates: rows.flatMap(({ head }) => Array.take(head, 1)) })) : Result.succeed(at + 1);
        },
    });
    return Result.map(from, (start) => [new Line({ ...page, rows: Array.drop(rows, start).map((each) => new Line(Struct.omit(each, ['cut']))) })]);
};
const alone = (index: Index, changed: HashSet.HashSet<(typeof Element.Control.Type)['ref']>, ref: (typeof Element.Control.Type)['ref']) => {
    const drafted = index.shown.flatMap((shown) =>
        shown.controls.flatMap((entry, at) => (entry.control.ref === ref ? [draft(index, shown, changed, { whole: true })({ entry, alternates: [], nested: Array.takeWhile(Array.drop(shown.controls, at + 1), (each) => under(each.control, entry.control)).map((each) => ({ entry: each, alternates: [] })) })] : [])),
    );
    return Array.head(siblings(drafted, NOUNS.control.other).rows);
};
const render = (index: Index, { in: within = Option.none(), after = Option.none(), changed = HashSet.empty() }: { readonly in?: Option.Option<Item>; readonly after?: Option.Option<Item>; readonly changed?: HashSet.HashSet<(typeof Element.Control.Type)['ref']> } = {}) => {
    const scope = Option.flatMap(within, ({ token, target }) => Option.map(token, (at) => ({ at, target })));
    const page = Option.match(scope, {
        onNone: (): Result.Result<Line, Unmatched> => Result.succeed(new Line({ head: [], rows: tree(index, changed), call: inspect({}) })),
        onSome: ({ at, target }) => {
            const found = target._tag === 'Control' ? alone(index, changed, target.control.ref) : Array.findFirst(flattened(tree(index, changed)), ({ head }) => Option.contains(Array.head(head), at));
            return Result.fromOption(found, () => Unmatched.of({ subject: 'container or control of this read', query: Option.some(at), candidates: [...index.shown.map(Struct.get('item')), ...index.remembered.containers].flatMap(({ token }) => Option.toArray(token)) }));
        },
    });
    return Result.flatMap(page, (held) => paged(held, after));
};
const hits = (found: readonly Item[], given: Call['given'], { after = Option.none(), opening = Option.none() }: { readonly after?: Option.Option<Item>; readonly opening?: Option.Option<Index> } = {}) => {
    const rows = found.map((item) => {
        const kind = Address.Target.$match(item.target, {
            Container: ({ container }) => [NOUNS[container.kind].one],
            Control: ({ control }) => spoken(control.kind),
            Option: () => ['option'],
            RememberedContainer: ({ entry }) => [NOUNS[entry.container.kind].one],
            RememberedControl: ({ control }) => spoken(control.kind),
        });
        const path = Address.path(item).join(Address.separator);
        const opened = Option.flatMap(opening, (index) => Option.map(Address.opener(index, item), (openers) => Array.getSomes(openers.map(Struct.get('token'))).join(', ')));
        return new Line({
            head: item.live ? [...Option.toArray(item.token), ...kind, Format.quoted(path)] : [Option.getOrElse(item.token, () => path), ...kind],
            facts: [...(item.live ? [] : [SAY.gone, ...Option.toArray(Option.map(item.value, (value) => `last value: ${value}`))]), ...Option.toArray(Option.map(opened, (token) => `opened by ${token}`))],
            ...(item.live ? Option.getOrUndefined(Option.map(item.value, (value) => ({ value }))) : {}),
        });
    });
    return paged(new Line({ head: [], rows, call: inspect(given) }), after);
};
const clipboard = ({ types, access, patterns, contentType, content }: Pasteboard, ref: (typeof Element.Control.Type)['ref'], asked: boolean) => {
    const head = [Address.token('clipboard', ref), 'clipboard'];
    const text = content?._tag === 'text' ? Option.some(content.value) : Option.none<string>();
    const shown = Option.map(text, (value) => {
        const words = Iterator.from(WORDS.segment(value)).filter(({ isWordLike }) => isWordLike === true);
        const end = asked
            ? value.length
            : (words
                  .drop(KEPT.words - 1)
                  .map(({ index, segment }) => index + segment.length)
                  .next().value ?? value.length);
        return value.slice(0, Math.min(Report.page, end)).toWellFormed();
    });
    return content?._tag === 'concealed'
        ? new Line({ head, facts: [content._tag] })
        : new Line({
              head,
              ...Option.getOrUndefined(Option.map(shown, (value) => ({ value: `"${value}"` }))),
              facts: [
                  ...Option.toArray(Option.filter(text, (value) => Option.exists(shown, (kept) => kept.length < value.length))).map((value) => Format.count(value.length, NOUNS.character)),
                  ...flags({ empty: Array.isReadonlyArrayEmpty(types) }),
                  ...(asked && Array.isReadonlyArrayNonEmpty(types) ? [`types ${types.join(', ')}`] : []),
                  ...(asked && Array.isReadonlyArrayNonEmpty(patterns) ? [`detects ${patterns.join(', ')}`] : []),
                  ...Array.fromNullishOr(contentType).map((type) => `file type ${type}`),
                  ...(content?._tag === 'withheld' ? [`text ${content._tag}`, `Paste from Other Apps ${access} in System Settings > Privacy & Security`] : []),
              ],
          });
};
const clipped = (instance: Instance['instance'], pasteboard: Pasteboard, asked: boolean) => ApplicationServer.use((server) => server.intern({ instance, locators: [pasteboard.locator] })).pipe(Effect.map(Array.map((ref) => clipboard(pasteboard, ref, asked))));
const changes = (before: Index, after: Index, touched: ReadonlySet<(typeof Element.Control.Type)['ref']>): readonly Line[] => {
    const earlier = Record.fromIterableWith(before.model.containers, ({ ref, controls }) => [`${ref}`, new Set(controls.map(Struct.get('ref')))]);
    const changed = HashSet.fromIterable(after.model.containers.flatMap(({ controls }) => controls.filter(({ ref, value }) => touched.has(ref) && Option.exists(Record.get(before.live, `${ref}`), ({ target }) => target._tag === 'Control' && !Equal.equals(target.control.value, value))).map(Struct.get('ref'))));
    const read = after.model.containers.map((container) => {
        const [now, prior] = [new Set(container.controls.map(Struct.get('ref'))), Record.get(earlier, `${container.ref}`)];
        const went = Option.toArray(prior).flatMap((held) => Array.fromIterable(held.difference(now)).flatMap((ref) => Option.toArray(Option.flatMap(Record.get(before.live, `${ref}`), Struct.get('token')))));
        return { container, went, same: Option.exists(prior, (held) => held.symmetricDifference(now).size === 0), valued: container.controls.filter(({ ref }) => HashSet.has(changed, ref)) };
    });
    const containers = read.filter(({ same, valued }) => !same || Array.isReadonlyArrayNonEmpty(valued)).map(({ container, same, valued }) => (same ? { ...container, controls: valued, text: [], truncated: false } : container));
    const closed = before.shown.filter(({ container }) => !read.some((each) => each.container.ref === container.ref));
    return [
        ...tree(
            Address.index({ ...after.model, containers }),
            changed,
            Record.fromIterableWith(read, ({ container, went }) => [`${container.ref}`, went]),
        ),
        ...closed.map(({ container, item }) => new Line({ head: caption(container.kind, container.ref, Option.getOrUndefined(item.name)), facts: [SAY.closed] })),
    ];
};
const subject = Effect.fnUntraced(function* (application: string, { instance, state }: Pick<Instance, 'instance' | 'state'>, { verb, facts = [], troubled = false, keyed = false }: { readonly verb?: string; readonly facts?: readonly string[]; readonly troubled?: boolean; readonly keyed?: boolean } = {}) {
    const [{ session, pressure }, handle] = yield* Effect.all([ApplicationServer, Handle]);
    const [screen, strain, { held }] = yield* Effect.all([session, pressure, handle.get(Target)]);
    const { stalled, front } = Instance.State.match(state, {
        running: ({ status, active }) => ({ stalled: Instance.State.Status.matchOrElse(status, { responding: (): readonly string[] => [], unresponsive: ({ _tag, thread }) => [_tag, `main thread ${thread}`] }, ({ _tag }) => [_tag]), front: active }),
        failed: ({ reason, subject: failed, message }) => ({ stalled: [`state unread: ${message ?? `${failed} ${reason}`}`], front: false }),
        exited: ({ _tag }) => ({ stalled: [_tag], front: false }),
    });
    const departures = [
        ...stalled,
        ...flags({ [SAY.front]: front }),
        ...Option.match(screen, { onNone: Array.empty, onSome: ({ onConsole, locked, asleep, secureInput }) => flags({ 'screen in another session': !onConsole, 'screen locked': locked, 'display asleep': asleep, 'secure input on': keyed && secureInput }) }),
        ...Option.match(
            Option.filter(strain, () => troubled || Array.isReadonlyArrayNonEmpty(stalled)),
            { onNone: Array.empty, onSome: ({ memory, thermal, advisory = {} }) => flags({ [`memory pressure ${memory}`]: memory !== 'normal', [`thermal state ${thermal}`]: thermal === 'serious' || thermal === 'critical', 'system load advisory bad': Record.some(advisory, (level) => level === 'bad') }) },
        ),
        ...facts,
    ];
    return verb !== undefined || Array.isReadonlyArrayNonEmpty(departures) || !Option.exists(held, (aim) => aim.instance === instance) ? [new Line({ head: [application, ...Array.fromNullishOr(verb)], facts: departures, pinned: true })] : [];
});
const Outline = { subject, caption, render, hits, changes, clipped, rows: KEPT.rows, say: SAY, nouns: NOUNS, shortcut, scrolled, showing } as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Outline };
