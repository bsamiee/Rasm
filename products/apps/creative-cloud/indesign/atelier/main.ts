import { Description } from '@rasm/applications/description';
import { Adjust, Typeset, TypesettingRpcs } from '@rasm/indesign/typesetting';
import { Dom } from '@rasm/indesign/uxp/dom';
import { call, change, findChange, font, fontSource, inspect, preflight, render, suppressDialogs } from '@rasm/indesign/uxp/operations';
import { KASHIDA_WIDTHS, STYLISTIC_TAGS, thousandths } from '@rasm/indesign/uxp/settings';
import { toolbar, workspace } from '@rasm/indesign/uxp/source';
import { adjust, kerns, runs, typeset } from '@rasm/indesign/uxp/typesetting';
import { HostError } from '@rasm/object-model/model';
import { RenderRpcs } from '@rasm/object-model/rpcs';
import { Settings } from '@rasm/typography/settings';
import { Plugin, Uxp } from '@rasm/uxp/plugin';
import { createElement, createPicker } from '@rasm/uxp/spectrum';
import { Array, Cause, Effect, Function, flow, Layer, Match, Number, Option, pipe, Record, Result, Schema, Scope, Semaphore, Stream, Struct } from 'effect';
import declarations from 'indesign-uxp-types/flat?inline';
import { Tools } from '#/tools';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SHORTCUT_MULTIPLE = 5;

// --- [MODELS] --------------------------------------------------------------------------

class Steps extends Schema.Opaque<Steps>()(Schema.Struct({ kerning: thousandths, baselineShift: Schema.Finite })) {}
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
const hosted = Effect.provideService(Dom, dom);
const scripting = hosted(Effect.andThen(Semaphore.takeScoped(permit, 1), suppressDialogs));
const runScript = <A, E, R>(effect: Effect.Effect<A, E, R>) => hosted(Effect.scopedWith((scope) => Effect.andThen(Scope.provide(scripting, scope), effect)));
const events = Dom.events(({ app, Document }) => [app, [Document.AFTER_OPEN, Document.AFTER_SAVE, Document.BEFORE_CLOSE]]).pipe(
    Stream.filterMap(({ eventType, target }) => (dom.document(target) ? Result.succeed({ name: eventType, target: Option.some(target.toSpecifier()), detail: Option.none() }) : Result.failVoid)),
    Stream.provideService(Dom, dom),
);
const source = pipe(Description.tools(toolbar), Description.merge(workspace), Description.merge({ changes: events }));
const application = Effect.tryPromise({ try: () => dom.app.fullName, catch: HostError.refused('application fullName') }).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Uxp.Application)), Effect.map(Struct.get('url')));
const panel = Effect.fnUntraced(function* (root: HTMLElement, fork: <A, X>(work: Effect.Effect<A, X, Plugin>) => void) {
    const mode = createPicker(
        'Mode',
        Record.toEntries({
            Marks: { horizontal: ({ kerning }) => ({ marks: { x: kerning, y: 0 } }), vertical: ({ kerning }) => ({ marks: { x: 0, y: kerning } }) },
            Kerning: { horizontal: ({ kerning }) => ({ kerning }), vertical: ({ baselineShift }) => ({ baselineShift }) },
            Tracking: { horizontal: ({ kerning }) => ({ tracking: kerning }), vertical: ({ baselineShift }) => ({ baselineShift }) },
        } as const satisfies Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, (steps: Steps) => Adjust['deltas']>>),
    );
    const elongation = createPicker(
        'Elongation',
        Array.map(Struct.keys(Settings.fields.kashida.schema.cases), (tag) => [tag, tag] as const),
    );
    const width = createPicker('Width', Record.toEntries(Struct.omit(KASHIDA_WIDTHS, ['None'])));
    const sets = Array.map(STYLISTIC_TAGS, (tag) => [tag, createElement('sp-checkbox', { textContent: tag })] as const);
    const buttons = Record.collect({ '←': { axis: 'horizontal', sign: -1 }, '→': { axis: 'horizontal', sign: 1 }, '↑': { axis: 'vertical', sign: 1 }, '↓': { axis: 'vertical', sign: -1 } } as const, (textContent, arrow) => [arrow, createElement('sp-action-button', { quiet: true, textContent })] as const);
    const groups: Record.ReadonlyRecord<string, readonly HTMLElement[]> = { Nudge: [mode.element, ...Array.map(buttons, ([, button]) => button)], Kashida: [elongation.element, width.element], 'Stylistic sets': Array.map(sets, ([, box]) => box) };
    const inputs = Array.flatten(Object.values(groups));
    const disableInputs = (held: readonly HTMLElement[]) =>
        inputs.forEach((input) => {
            Object.assign(input, { disabled: held.includes(input) });
        });
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
    const reshape = () => Option.none().pipe(typesetSelection, Effect.map(readKashida), act);
    elongation.onChange(reshape);
    width.onChange(reshape);
    sets.forEach(([tag, box]) => box.addEventListener('change', () => act(Effect.succeedSome({ features: { [tag]: box.checked ? 1 : null } }))));
    buttons.forEach(([{ axis, sign }, button]) => button.addEventListener('click', ({ metaKey, ctrlKey }) => act(nudge(Option.map(mode.read(), Struct.get(axis)), sign * (metaKey || ctrlKey ? SHORTCUT_MULTIPLE : 1)))));
    Object.assign(root, { textContent: '' });
    root.append(...Record.collect(groups, (textContent, children) => createElement('div', {}, createElement('sp-detail', { textContent }), ...children)));
    fork(
        Stream.runForEach(
            Dom.events(({ app, Application }) => [app, [Application.AFTER_SELECTION_CHANGED, Application.AFTER_SELECTION_ATTRIBUTE_CHANGED, Application.AFTER_CONTEXT_CHANGED]]),
            () => Effect.sync(refresh),
        ).pipe(Effect.provideService(Dom, dom)),
    );
    yield* Effect.sync(refresh);
}, runScript);

Plugin.serve({
    link: Plugin.dial({
        application,
        group: RenderRpcs.merge(TypesettingRpcs),
        toolkit: Tools,
        handlers: Layer.mergeAll(
            Tools.toLayer({ read: flow(inspect, hosted), change: flow(change, hosted), call: flow(call, hosted), find_change: flow(findChange, hosted), adjust: flow(adjust, hosted), preflight: flow(preflight, hosted) }),
            RenderRpcs.toLayer({ render: ({ target }) => hosted(Uxp.renderFile((file) => render(target, file.nativePath))) }),
            TypesettingRpcs.toLayer({ runs: flow(runs, hosted), kerns: flow(kerns, hosted), typeset: flow(typeset, hosted), font: flow(font, hosted), fontSource: flow(fontSource, hosted) }),
        ),
        source: { describe: flow(source.describe, hosted), operate: flow(source.operate, hosted), changes: Stream.provideService(source.changes, Dom, dom) },
        dialogs: scripting,
    }),
    panels: { typesetting: panel },
});
