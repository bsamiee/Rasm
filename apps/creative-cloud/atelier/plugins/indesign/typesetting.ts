import { Adjust, Typeset } from '@rasm/automation';
import { InDesignModel, InDesignSettings, inDesignHandlers, type linkInDesign } from '@rasm/indesign/uxp';
import { Settings } from '@rasm/typography';
import { Array, Cause, Effect, Function, Number, Option, Record, Schema, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SHORTCUT_MULTIPLE = 5;

// --- [MODELS] --------------------------------------------------------------------------

const Steps = Schema.Struct({ kerning: InDesignSettings.Thousandths, baselineShift: Schema.Finite });
const elongating = Settings.Kashida.isAnyOf(['Tatweel', 'Stretched']);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [CONTROLS]

const row = (...children: readonly HTMLElement[]) => {
    const element = document.createElement('div');
    Object.assign(element.style, { display: 'flex', flexWrap: 'wrap', alignItems: 'center' });
    children.forEach((child) => element.appendChild(child));
    return element;
};
const picker = <A>(options: readonly (readonly [string, A])[]) => {
    const element = document.createElement('select');
    options.forEach(([text]) => element.appendChild(Object.assign(document.createElement('option'), { textContent: text })));
    return { element, read: () => Option.map(Array.get(options, element.selectedIndex), ([, value]) => value), show: (value: unknown) => Object.assign(element, { selectedIndex: options.findIndex(([, each]) => each === value) }) };
};

// --- [REQUESTS]

const typesetSelection = (settings: Option.Option<typeof Settings.Partial.Type>) => Effect.flatMap(InDesignModel.selection, (item) => inDesignHandlers.typeset(Typeset.make({ target: item.toSpecifier(), settings })));
const nudged = Effect.fnUntraced(function* (deltas: Option.Option<(steps: typeof Steps.Type) => Adjust['deltas']>, multiple: number) {
    const [{ app, owning }, item, delta] = yield* Effect.all([InDesignModel, InDesignModel.selection, Effect.fromOption(deltas)]);
    const styled = yield* Effect.fromOption(Option.liftPredicate(item, owning('textStyleRanges')), () => new Cause.NoSuchElementError(`text style range in selection ${item.toSpecifier()}`));
    const steps = yield* InDesignModel.read(Steps, 'text preferences', () => ({ kerning: multiple * app.textPreferences.kerningKeyIncrement, baselineShift: (multiple * app.textPreferences.baselineShiftKeyIncrement) / styled.textStyleRanges.firstItem().pointSize }));
    yield* inDesignHandlers.adjust(Adjust.make({ target: item.toSpecifier(), deltas: delta(steps) }));
    return Option.none();
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const typesetting = (scripted: Effect.Success<typeof linkInDesign>['scripted']) =>
    Effect.fnUntraced(function* (root: HTMLElement, fork: (work: Effect.Effect<unknown, unknown>) => void) {
        const { app } = yield* InDesignModel;
        const mode = picker(
            Record.toEntries({
                Marks: { horizontal: ({ kerning }) => ({ marks: { x: kerning, y: 0 } }), vertical: ({ kerning }) => ({ marks: { x: 0, y: kerning } }) },
                Kerning: { horizontal: ({ kerning }) => ({ kerning }), vertical: ({ baselineShift }) => ({ baselineShift }) },
                Tracking: { horizontal: ({ kerning }) => ({ tracking: kerning }), vertical: ({ baselineShift }) => ({ baselineShift }) },
            } as const satisfies Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, (steps: typeof Steps.Type) => Adjust['deltas']>>),
        );
        const elongation = picker(Array.map(Struct.keys(Settings.Kashida.cases), (tag) => [tag, tag] as const));
        const width = picker(Record.toEntries(Struct.omit(InDesignSettings.kashidaWidths, ['None'])));
        const sets = Array.map(InDesignSettings.stylisticSets, (tag) => [tag, Object.assign(document.createElement('input'), { type: 'checkbox', title: tag })] as const);
        const buttons = Record.collect({ '←': { axis: 'horizontal', sign: -1 }, '→': { axis: 'horizontal', sign: 1 }, '↑': { axis: 'vertical', sign: 1 }, '↓': { axis: 'vertical', sign: -1 } } as const, (label, arrow) => [arrow, Object.assign(document.createElement('button'), { textContent: label })] as const);
        const status = document.createElement('div');
        const controls = row(mode.element, ...buttons.map(([, button]) => button), elongation.element, width.element, ...sets.map(([tag, box]) => row(box, Object.assign(document.createElement('span'), { textContent: tag }))));
        const failed = (cause: Cause.Cause<unknown>) =>
            Effect.sync(() => {
                status.textContent = Cause.pretty(cause);
                controls.style.display = 'none';
            });
        const checked =
            (features: Settings['features']) =>
            ([tag, box]: (typeof sets)[number]) =>
                Object.assign(box, { checked: Option.exists(Record.get(features, tag), Number.isGreaterThan(0)) });
        const displayed = ({ kashida, features }: Settings) =>
            Effect.sync(() => {
                status.textContent = '';
                controls.style.display = 'flex';
                elongation.show(kashida._tag);
                if (elongating(kashida)) {
                    width.show(kashida.lower);
                }
                sets.forEach(checked(features));
            });
        const acted = <E>(change: Effect.Effect<Option.Option<typeof Settings.Partial.Type>, E, InDesignModel>) => fork(scripted(Effect.flatMap(change, typesetSelection)).pipe(Effect.matchCauseEffect({ onFailure: failed, onSuccess: displayed })));
        const changed = () => acted(Effect.succeedNone);
        const elongated = ({ kashida }: Settings) => Option.map(Option.all([elongation.read(), width.read()]), ([_tag, lower]) => ({ kashida: _tag === 'None' ? { _tag } : { _tag, lower, upper: lower, fractional: elongating(kashida) && kashida.fractional } }));
        const reshaped = () => acted(Effect.map(typesetSelection(Option.none()), elongated));
        const toggled = ([tag, box]: (typeof sets)[number]) => box.addEventListener('change', () => acted(Effect.succeedSome({ features: { [tag]: box.checked ? 1 : null } })));
        const nudging = ([{ axis, sign }, button]: (typeof buttons)[number]) => button.addEventListener('click', ({ metaKey, ctrlKey }) => acted(nudged(Option.map(mode.read(), Struct.get(axis)), sign * (metaKey || ctrlKey ? SHORTCUT_MULTIPLE : 1))));
        [elongation, width].forEach(({ element }) => element.addEventListener('change', reshaped));
        sets.forEach(toggled);
        buttons.forEach(nudging);
        Object.assign(root, { textContent: '' }).style.color = 'var(--uxp-host-text-color)';
        [status, controls].forEach((child) => root.appendChild(child));
        const events = ['afterSelectionChanged', 'afterSelectionAttributeChanged', 'afterContextChanged'] as const;
        const listened = (event: (typeof events)[number]) =>
            Effect.acquireRelease(
                Effect.sync(() => app.addEventListener(event, changed)),
                Function.constant(Effect.sync(() => app.removeEventListener(event, changed))),
            );
        yield* Effect.forEach(events, listened, { discard: true });
        yield* Effect.sync(changed);
    }, scripted);

// --- [EXPORTS] -------------------------------------------------------------------------

export { typesetting };
