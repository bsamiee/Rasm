import { SourceRpcs } from '@rasm/applications/source';
import { Adjust, Typeset, TypesettingRpcs } from '@rasm/indesign';
import { adjust, call, change, describe, findChange, font, inspect, KASHIDA_WIDTHS, kerns, listen, Dom, operate, preflight, render, runs, STYLISTIC_TAGS, suppressDialogs, thousandths, typeset } from '@rasm/indesign/uxp';
import type { Upstream } from '@rasm/mcp';
import { HostError, RenderRpcs } from '@rasm/object-model';
import { read } from '@rasm/object-model/host';
import { Settings } from '@rasm/typography';
import { Plugin, Uxp } from '@rasm/uxp/plugin';
import { element, picker } from '@rasm/uxp/spectrum';
import { Array, Cause, Effect, Function, flow, Layer, Match, Number, Option, Predicate, Queue, Record, Schema, Scope, Semaphore, Stream, Struct } from 'effect';
import declarations from 'indesign-uxp-types/flat?inline';
import { Tools } from '#/tools';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SHORTCUT_MULTIPLE = 5;

// --- [MODELS] --------------------------------------------------------------------------

class Steps extends Schema.Opaque<Steps, unknown>()(Schema.Struct({ kerning: thousandths, baselineShift: Schema.Finite })) {}
const isElongated = Settings.fields.kashida.schema.isAnyOf(['Tatweel', 'Stretched']);

// --- [OPERATIONS] ----------------------------------------------------------------------

const typesetSelection = (settings: Typeset['settings']) => Effect.flatMap(Dom.selection, (item) => typeset(Typeset.make({ target: item.toSpecifier(), settings })));
const nudge = Effect.fnUntraced(function* (deltas: Option.Option<(steps: Steps) => Adjust['deltas']>, multiple: number) {
    const [{ app, owning }, item, delta] = yield* Effect.all([Dom, Dom.selection, Effect.fromOption(deltas)]);
    const styled = yield* Effect.fromOption(Option.liftPredicate(item, owning('textStyleRanges')), () => new Cause.NoSuchElementError(`text style range in selection ${item.toSpecifier()}`));
    const steps = yield* Dom.read(Steps, 'text preferences', () => ({ kerning: multiple * app.textPreferences.kerningKeyIncrement, baselineShift: (multiple * app.textPreferences.baselineShiftKeyIncrement) / styled.textStyleRanges.firstItem().pointSize }));
    yield* adjust(Adjust.make({ target: item.toSpecifier(), deltas: delta(steps) }));
    return Option.none();
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const [dom, permit] = Effect.runSync(Effect.all([Dom.make(declarations), Semaphore.make(1)]));
const runScript = <A, E, R>(effect: Effect.Effect<A, E, R>) => Effect.scopedWith((scope) => Effect.andThen(Scope.provide(suppressDialogs, scope), effect)).pipe(Semaphore.withPermit(permit), Effect.provideService(Dom, dom));
const events = Stream.unwrap(
    Plugin.useSync(({ id }) =>
        Stream.callback<Stream.Success<Upstream['events']>>((queue) => {
            const offer = (name: unknown, target: unknown, detail: Option.Option<Schema.Json>): void => {
                if (Predicate.isString(name) && dom.domObject(target)) {
                    Queue.offerUnsafe(queue, { name, target: Option.some(target.toSpecifier()), detail });
                }
            };
            return Effect.all([
                listen(
                    ({ app }) => app,
                    [dom.Document.AFTER_OPEN, dom.Document.AFTER_SAVE, dom.Document.BEFORE_CLOSE],
                    id,
                    (event) => {
                        const target = read(event, 'target');
                        if (dom.document(target)) {
                            offer(read(event, 'eventType'), target, Option.none());
                        }
                    },
                ),
                listen(
                    ({ app }) => app.toolBoxTools,
                    [dom.ToolBox.AFTER_ATTRIBUTE_CHANGED],
                    id,
                    (event) => offer(read(event, 'eventType'), dom.app.toolBoxTools, Option.some({ currentTool: dom.app.toolBoxTools.currentTool.toString() })),
                ),
            ]).pipe(Effect.provideService(Dom, dom));
        }),
    ),
);
const application = Effect.tryPromise({ try: () => dom.app.fullName, catch: HostError.refused('application fullName') }).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Uxp.Application)), Effect.map(Struct.get('url')));
const panel = Effect.fnUntraced(function* (root: HTMLElement, fork: <A, X>(work: Effect.Effect<A, X>) => void) {
    const { id } = yield* Plugin;
    const mode = picker(
        'Mode',
        Record.toEntries({
            Marks: { horizontal: ({ kerning }) => ({ marks: { x: kerning, y: 0 } }), vertical: ({ kerning }) => ({ marks: { x: 0, y: kerning } }) },
            Kerning: { horizontal: ({ kerning }) => ({ kerning }), vertical: ({ baselineShift }) => ({ baselineShift }) },
            Tracking: { horizontal: ({ kerning }) => ({ tracking: kerning }), vertical: ({ baselineShift }) => ({ baselineShift }) },
        } as const satisfies Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, (steps: Steps) => Adjust['deltas']>>),
    );
    const elongation = picker(
        'Elongation',
        Array.map(Struct.keys(Settings.fields.kashida.schema.cases), (tag) => [tag, tag] as const),
    );
    const width = picker('Width', Record.toEntries(Struct.omit(KASHIDA_WIDTHS, ['None'])));
    const sets = Array.map(STYLISTIC_TAGS, (tag) => [tag, element('sp-checkbox', { textContent: tag })] as const);
    const buttons = Record.collect({ '←': { axis: 'horizontal', sign: -1 }, '→': { axis: 'horizontal', sign: 1 }, '↑': { axis: 'vertical', sign: 1 }, '↓': { axis: 'vertical', sign: -1 } } as const, (textContent, arrow) => [arrow, element('sp-action-button', { quiet: true, textContent })] as const);
    const groups: Record.ReadonlyRecord<string, readonly HTMLElement[]> = { Nudge: [mode.element, ...Array.map(buttons, ([, button]) => button)], Kashida: [elongation.element, width.element], 'Stylistic sets': Array.map(sets, ([, box]) => box) };
    const inputs = Array.flatten(Object.values(groups));
    const disableInputs = (held: readonly HTMLElement[]) => inputs.forEach((input) => (held.includes(input) ? input.setAttribute('disabled', '') : input.removeAttribute('disabled')));
    const displaySettings = ({ kashida, features }: Settings) =>
        Effect.sync(() => {
            disableInputs(isElongated(kashida) ? [] : [width.element]);
            elongation.show(kashida._tag);
            if (isElongated(kashida)) {
                width.show(kashida.lower);
            }
            sets.forEach(([tag, box]) => {
                Object.assign(box, { checked: Option.exists(Record.get(features, tag), Number.isGreaterThan(0)) });
            });
        });
    const applySettings = <E>(requested: Effect.Effect<Typeset['settings'], E, Dom>) => runScript(Effect.flatMap(requested, typesetSelection)).pipe(Effect.flatMap(displaySettings));
    const refresh = () => fork(applySettings(Effect.succeedNone).pipe(Effect.onError(Function.constant(Effect.sync(() => disableInputs(inputs)))), Effect.catchTag('Unmatched', Function.constant(Effect.void))));
    const act = flow(applySettings, Effect.onError(Function.constant(Effect.sync(refresh))), fork);
    const readKashida = ({ kashida }: Settings) =>
        Option.map(Option.all([elongation.read(), width.read()]), ([tag, lower]) => ({
            kashida: Match.value(tag).pipe(
                Match.when('None', () => Settings.fields.kashida.schema.cases.None.make({})),
                Match.orElse((stretch) => Settings.fields.kashida.schema.cases[stretch].make({ ...(isElongated(kashida) ? Struct.omit(kashida, ['_tag']) : {}), lower, upper: lower })),
            ),
        }));
    const reshape = () => act(Effect.map(typesetSelection(Option.none()), readKashida));
    elongation.onChange(reshape);
    width.onChange(reshape);
    sets.forEach(([tag, box]) => box.addEventListener('change', () => act(Effect.succeedSome({ features: { [tag]: box.checked ? 1 : null } }))));
    buttons.forEach(([{ axis, sign }, button]) => button.addEventListener('click', ({ metaKey, ctrlKey }) => act(nudge(Option.map(mode.read(), Struct.get(axis)), sign * (metaKey || ctrlKey ? SHORTCUT_MULTIPLE : 1)))));
    Object.assign(root, { textContent: '' });
    Record.collect(groups, (textContent, children) => element('div', {}, element('sp-detail', { textContent }), ...children)).forEach((child) => root.append(child));
    yield* listen(({ app }) => app, ['afterSelectionChanged', 'afterSelectionAttributeChanged', 'afterContextChanged'], id, refresh);
    yield* Effect.sync(refresh);
}, runScript);

Plugin.serve({
    link: Plugin.dial({
        application,
        group: RenderRpcs.merge(TypesettingRpcs, SourceRpcs),
        toolkit: Tools,
        handlers: Layer.mergeAll(
            Tools.toLayer({ read: flow(inspect, runScript), change: flow(change, runScript), call: flow(call, runScript), find_change: flow(findChange, runScript), adjust: flow(adjust, runScript), preflight: flow(preflight, runScript) }),
            RenderRpcs.toLayer({ render: ({ target }) => runScript(Uxp.renderFile((file) => render(target, file.nativePath))) }),
            TypesettingRpcs.toLayer({ runs: flow(runs, runScript), kerns: flow(kerns, runScript), typeset: flow(typeset, runScript), font: flow(font, runScript) }),
            SourceRpcs.toLayer({ describe: flow(describe, runScript), operate: flow(operate, runScript) }),
        ),
        events,
    }),
    panels: { typesetting: panel },
});
