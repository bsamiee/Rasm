import { Adjust, Typeset } from '@rasm/indesign';
import { InDesignModel, InDesignSettings, inDesignHandlers, type linkInDesign } from '@rasm/indesign/uxp';
import { Settings } from '@rasm/typography';
import { Array, Cause, Effect, Function, flow, MutableRef, Number, Option, Record, Schema, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

declare global {
    interface HTMLElementTagNameMap {
        readonly 'sp-action-button': HTMLElement & { quiet: boolean };
        readonly 'sp-checkbox': HTMLElement & { checked: boolean };
        readonly 'sp-detail': HTMLElement;
        readonly 'sp-label': HTMLElement;
        readonly 'sp-menu': HTMLElement;
        readonly 'sp-menu-item': HTMLElement;
        readonly 'sp-picker': HTMLElement & { selectedIndex: number };
    }
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SHORTCUT_MULTIPLE = 5;

// --- [MODELS] --------------------------------------------------------------------------

const Steps = Schema.Struct({ kerning: InDesignSettings.Thousandths, baselineShift: Schema.Finite });
const elongating = Settings.Kashida.isAnyOf(['Tatweel', 'Stretched']);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [CONTROLS]

const widget = <K extends keyof HTMLElementTagNameMap>(tag: K, fields: Partial<HTMLElementTagNameMap[K]>, ...children: readonly HTMLElement[]): HTMLElementTagNameMap[K] => {
    const element = document.createElement(tag);
    children.forEach((child) => element.appendChild(child));
    return Object.assign(element, fields);
};
const slotted = (slot: string, element: HTMLElement) => {
    element.setAttribute('slot', slot);
    return element;
};
const picker = <A>(label: string, options: readonly (readonly [string, A])[]) => {
    const shown = MutableRef.make(0);
    const element = widget('sp-picker', { selectedIndex: MutableRef.get(shown) }, slotted('label', widget('sp-label', { textContent: label })), slotted('options', widget('sp-menu', {}, ...Array.map(options, ([textContent]) => widget('sp-menu-item', { textContent })))));
    return {
        element,
        read: () => Option.map(Array.get(options, element.selectedIndex), ([, value]) => value),
        show: (value: unknown) =>
            Object.assign(element, {
                selectedIndex: MutableRef.setAndGet(
                    shown,
                    options.findIndex(([, each]) => each === value),
                ),
            }),
        picked: (work: () => void) =>
            element.addEventListener('change', () => {
                if (MutableRef.getAndSet(shown, element.selectedIndex) !== element.selectedIndex) {
                    work();
                }
            }),
    };
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
    Effect.fnUntraced(function* (root: HTMLElement, fork: <A, X>(work: Effect.Effect<A, X>) => void) {
        const { app } = yield* InDesignModel;
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
            Array.map(Struct.keys(Settings.Kashida.cases), (tag) => [tag, tag] as const),
        );
        const width = picker('Width', Record.toEntries(Struct.omit(InDesignSettings.kashidaWidths, ['None'])));
        const sets = Array.map(InDesignSettings.stylisticSets, (tag) => [tag, widget('sp-checkbox', { textContent: tag })] as const);
        const buttons = Record.collect({ '←': { axis: 'horizontal', sign: -1 }, '→': { axis: 'horizontal', sign: 1 }, '↑': { axis: 'vertical', sign: 1 }, '↓': { axis: 'vertical', sign: -1 } } as const, (textContent, arrow) => [arrow, widget('sp-action-button', { quiet: true, textContent })] as const);
        const groups: Record.ReadonlyRecord<string, readonly HTMLElement[]> = { Nudge: [mode.element, ...Array.map(buttons, ([, button]) => button)], Kashida: [elongation.element, width.element], 'Stylistic sets': Array.map(sets, ([, box]) => box) };
        const inputs = Array.flatten(Object.values(groups));
        const withheld = (held: readonly HTMLElement[]) => inputs.forEach((input) => (held.includes(input) ? input.setAttribute('disabled', '') : input.removeAttribute('disabled')));
        const checked =
            (features: Settings['features']) =>
            ([tag, box]: (typeof sets)[number]) =>
                Object.assign(box, { checked: Option.exists(Record.get(features, tag), Number.isGreaterThan(0)) });
        const displayed = ({ kashida, features }: Settings) =>
            Effect.sync(() => {
                withheld(elongating(kashida) ? [] : [width.element]);
                elongation.show(kashida._tag);
                if (elongating(kashida)) {
                    width.show(kashida.lower);
                }
                sets.forEach(checked(features));
            });
        const drawn = <E>(change: Effect.Effect<Option.Option<typeof Settings.Partial.Type>, E, InDesignModel>) => scripted(Effect.flatMap(change, typesetSelection)).pipe(Effect.flatMap(displayed));
        const changed = () => fork(drawn(Effect.succeedNone).pipe(Effect.onError(Function.constant(Effect.sync(() => withheld(inputs)))), Effect.catchTag('Unmatched', Function.constant(Effect.void))));
        const acted = flow(drawn, Effect.onError(Function.constant(Effect.sync(changed))), fork);
        const elongated = ({ kashida }: Settings) => Option.map(Option.all([elongation.read(), width.read()]), ([_tag, lower]) => ({ kashida: _tag === 'None' ? { _tag } : { _tag, lower, upper: lower, fractional: elongating(kashida) && kashida.fractional } }));
        const reshaped = () => acted(Effect.map(typesetSelection(Option.none()), elongated));
        const toggled = ([tag, box]: (typeof sets)[number]) => box.addEventListener('change', () => acted(Effect.succeedSome({ features: { [tag]: box.checked ? 1 : null } })));
        const nudging = ([{ axis, sign }, button]: (typeof buttons)[number]) => button.addEventListener('click', ({ metaKey, ctrlKey }) => acted(nudged(Option.map(mode.read(), Struct.get(axis)), sign * (metaKey || ctrlKey ? SHORTCUT_MULTIPLE : 1))));
        elongation.picked(reshaped);
        width.picked(reshaped);
        sets.forEach(toggled);
        buttons.forEach(nudging);
        Object.assign(root, { textContent: '' });
        Record.collect(groups, (textContent, children) => widget('div', {}, widget('sp-detail', { textContent }), ...children)).forEach((child) => root.appendChild(child));
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
