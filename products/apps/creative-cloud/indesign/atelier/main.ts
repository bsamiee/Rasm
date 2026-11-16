import { adjust, KASHIDA_WIDTHS, listened, Model, STYLISTIC_TAGS, thousandths, typeset } from '@rasm/indesign/uxp';
import { Settings } from '@rasm/typography';
import { Plugin } from '@rasm/uxp/plugin';
import { picker, widget } from '@rasm/uxp/spectrum';
import { Array, Cause, Effect, Function, flow, Match, Number, Option, Record, Schema, Struct } from 'effect';
import { Adjust, Typeset } from '#/contract';
import { link } from '#/handlers';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SHORTCUT_MULTIPLE = 5;

// --- [MODELS] --------------------------------------------------------------------------

const Steps = Schema.Struct({ kerning: thousandths, baselineShift: Schema.Finite });
const elongating = Settings.fields.kashida.schema.isAnyOf(['Tatweel', 'Stretched']);

// --- [OPERATIONS] ----------------------------------------------------------------------

const typesetSelection = (settings: Option.Option<typeof Settings.Partial.Type>) => Effect.flatMap(Model.selection, (item) => typeset(Typeset.make({ target: item.toSpecifier(), settings })));
const nudged = Effect.fnUntraced(function* (deltas: Option.Option<(steps: typeof Steps.Type) => Adjust['deltas']>, multiple: number) {
    const [{ app, owning }, item, delta] = yield* Effect.all([Model, Model.selection, Effect.fromOption(deltas)]);
    const styled = yield* Effect.fromOption(Option.liftPredicate(item, owning('textStyleRanges')), () => new Cause.NoSuchElementError(`text style range in selection ${item.toSpecifier()}`));
    const steps = yield* Model.read(Steps, 'text preferences', () => ({ kerning: multiple * app.textPreferences.kerningKeyIncrement, baselineShift: (multiple * app.textPreferences.baselineShiftKeyIncrement) / styled.textStyleRanges.firstItem().pointSize }));
    yield* adjust(Adjust.make({ target: item.toSpecifier(), deltas: delta(steps) }));
    return Option.none();
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const { linked, scripted } = Effect.runSync(link);
const typesetting = Effect.fnUntraced(function* (root: HTMLElement, fork: <A, X>(work: Effect.Effect<A, X>) => void) {
    const { id } = yield* Plugin;
    const mode = picker(
        'Mode',
        Record.toEntries({
            Marks: { horizontal: ({ kerning }) => ({ marks: { x: kerning, y: 0 } }), vertical: ({ kerning }) => ({ marks: { x: 0, y: kerning } }) },
            Kerning: { horizontal: ({ kerning }) => ({ kerning }), vertical: ({ baselineShift }) => ({ baselineShift }) },
            Tracking: { horizontal: ({ kerning }) => ({ tracking: kerning }), vertical: ({ baselineShift }) => ({ baselineShift }) },
        } as const satisfies Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, (steps: typeof Steps.Type) => Adjust['deltas']>>),
    );
    const elongation = picker(
        'Elongation',
        Array.map(Struct.keys(Settings.fields.kashida.schema.cases), (tag) => [tag, tag] as const),
    );
    const width = picker('Width', Record.toEntries(Struct.omit(KASHIDA_WIDTHS, ['None'])));
    const sets = Array.map(STYLISTIC_TAGS, (tag) => [tag, widget('sp-checkbox', { textContent: tag })] as const);
    const buttons = Record.collect({ '←': { axis: 'horizontal', sign: -1 }, '→': { axis: 'horizontal', sign: 1 }, '↑': { axis: 'vertical', sign: 1 }, '↓': { axis: 'vertical', sign: -1 } } as const, (textContent, arrow) => [arrow, widget('sp-action-button', { quiet: true, textContent })] as const);
    const groups: Record.ReadonlyRecord<string, readonly HTMLElement[]> = { Nudge: [mode.element, ...Array.map(buttons, ([, button]) => button)], Kashida: [elongation.element, width.element], 'Stylistic sets': Array.map(sets, ([, box]) => box) };
    const inputs = Array.flatten(Object.values(groups));
    const withheld = (held: readonly HTMLElement[]) => inputs.forEach((input) => (held.includes(input) ? input.setAttribute('disabled', '') : input.removeAttribute('disabled')));
    const checked =
        (features: Settings['features']) =>
        ([tag, box]: (typeof sets)[number]) => {
            Object.assign(box, { checked: Option.exists(Record.get(features, tag), Number.isGreaterThan(0)) });
        };
    const displayed = ({ kashida, features }: Settings) =>
        Effect.sync(() => {
            withheld(elongating(kashida) ? [] : [width.element]);
            elongation.show(kashida._tag);
            if (elongating(kashida)) {
                width.show(kashida.lower);
            }
            sets.forEach(checked(features));
        });
    const drawn = <E>(change: Effect.Effect<Option.Option<typeof Settings.Partial.Type>, E, Model>) => scripted(Effect.flatMap(change, typesetSelection)).pipe(Effect.flatMap(displayed));
    const changed = () => fork(drawn(Effect.succeedNone).pipe(Effect.onError(Function.constant(Effect.sync(() => withheld(inputs)))), Effect.catchTag('Unmatched', Function.constant(Effect.void))));
    const acted = flow(drawn, Effect.onError(Function.constant(Effect.sync(changed))), fork);
    const elongated = ({ kashida }: Settings) =>
        Option.map(Option.all([elongation.read(), width.read()]), ([tag, lower]) => ({
            kashida: Match.value(tag).pipe(
                Match.when('None', (none) => ({ _tag: none })),
                Match.orElse((stretch) => ({ _tag: stretch, lower, upper: lower, fractional: elongating(kashida) && kashida.fractional })),
            ),
        }));
    const reshaped = () => acted(Effect.map(typesetSelection(Option.none()), elongated));
    const toggled = ([tag, box]: (typeof sets)[number]) => box.addEventListener('change', () => acted(Effect.succeedSome({ features: { [tag]: box.checked ? 1 : null } })));
    const nudging = ([{ axis, sign }, button]: (typeof buttons)[number]) => button.addEventListener('click', ({ metaKey, ctrlKey }) => acted(nudged(Option.map(mode.read(), Struct.get(axis)), sign * (metaKey || ctrlKey ? SHORTCUT_MULTIPLE : 1))));
    elongation.picked(reshaped);
    width.picked(reshaped);
    sets.forEach(toggled);
    buttons.forEach(nudging);
    Object.assign(root, { textContent: '' });
    Record.collect(groups, (textContent, children) => widget('div', {}, widget('sp-detail', { textContent }), ...children)).forEach((child) => root.append(child));
    yield* listened(['afterSelectionChanged', 'afterSelectionAttributeChanged', 'afterContextChanged'], id, changed);
    yield* Effect.sync(changed);
}, scripted);

Plugin.serve({ link: linked, panels: { typesetting } });
