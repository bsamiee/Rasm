import { Adjust, Typeset } from '@rasm/automation/typesetting';
import type { host } from '@rasm/indesign/host';
import { Model } from '@rasm/indesign/model';
import { HostSettings } from '@rasm/indesign/settings';
import { adjust, typeset } from '@rasm/indesign/typesetting';
import { Settings } from '@rasm/typography/settings';
import { Array, Cause, Effect, Number, Option, Record, Schema, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SHORTCUT_MULTIPLE = 5;

// --- [MODELS] --------------------------------------------------------------------------

const Steps = Schema.Struct({ kerning: HostSettings.Thousandths, baselineShift: Schema.Finite });

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

const typesetSelection = (settings: Option.Option<typeof Settings.Partial.Type>) => Effect.flatMap(Model.selection, (item) => typeset(Typeset.make({ target: item.toSpecifier(), settings })));
const nudged = Effect.fnUntraced(function* (deltas: Option.Option<(steps: typeof Steps.Type) => Adjust['deltas']>, multiple: number) {
    const [{ app }, item, delta] = yield* Effect.all([Model, Model.selection, Effect.fromOption(deltas)]);
    const steps = yield* Model.read(Steps, 'text preferences', () => ({ kerning: multiple * app.textPreferences.kerningKeyIncrement, baselineShift: multiple * app.textPreferences.baselineShiftKeyIncrement }));
    yield* adjust(Adjust.make({ target: item.toSpecifier(), deltas: delta(steps) }));
    return Option.none();
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const typesetting = (scripted: ReturnType<typeof host>['scripted']) =>
    Effect.fnUntraced(function* (root: HTMLElement, fork: (work: Effect.Effect<unknown, unknown>) => void) {
        const { app } = yield* Model;
        const mode = picker(
            Record.toEntries({
                Marks: { horizontal: ({ kerning }) => ({ marks: { x: kerning, y: 0 } }), vertical: ({ kerning }) => ({ marks: { x: 0, y: kerning } }) },
                Kerning: { horizontal: ({ kerning }) => ({ kerning }), vertical: ({ baselineShift }) => ({ baselineShift }) },
                Tracking: { horizontal: ({ kerning }) => ({ tracking: kerning }), vertical: ({ baselineShift }) => ({ baselineShift }) },
            } as const satisfies Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, (steps: typeof Steps.Type) => Adjust['deltas']>>),
        );
        const elongation = picker(Array.map(Struct.keys(Settings.fields.kashida.schema.cases), (tag) => [tag, tag] as const));
        const width = picker(Record.toEntries(Struct.omit(HostSettings.kashidaWidths, ['None'])));
        const sets = Array.map(HostSettings.stylisticSets, (tag) => [tag, Object.assign(document.createElement('input'), { type: 'checkbox', title: tag })] as const);
        const buttons = Record.collect({ '←': { axis: 'horizontal', sign: -1 }, '→': { axis: 'horizontal', sign: 1 }, '↑': { axis: 'vertical', sign: 1 }, '↓': { axis: 'vertical', sign: -1 } } as const, (label, arrow) => [arrow, Object.assign(document.createElement('button'), { textContent: label })] as const);
        const status = document.createElement('div');
        const controls = row(mode.element, ...buttons.map(([, button]) => button), elongation.element, width.element, ...sets.map(([tag, box]) => row(box, Object.assign(document.createElement('span'), { textContent: tag }))));
        const failed = (cause: Cause.Cause<unknown>) =>
            Effect.sync(() => {
                status.textContent = Cause.pretty(cause);
                controls.style.display = 'none';
            });
        const displayed = ({ kashida, features }: Settings) =>
            Effect.sync(() => {
                status.textContent = '';
                controls.style.display = 'flex';
                elongation.show(kashida._tag);
                if (kashida._tag !== 'None') {
                    width.show(kashida.lower);
                }
                sets.forEach(([tag, box]) => Object.assign(box, { checked: Option.exists(Record.get(features, tag), Number.isGreaterThan(0)) }));
            });
        const acted = <E>(change: Effect.Effect<Option.Option<typeof Settings.Partial.Type>, E, Model>) => fork(scripted(Effect.flatMap(change, typesetSelection)).pipe(Effect.matchCauseEffect({ onFailure: failed, onSuccess: displayed })));
        const changed = () => acted(Effect.succeedNone);
        const elongated = ({ kashida }: Settings) => Option.map(Option.all([elongation.read(), width.read()]), ([_tag, lower]) => ({ kashida: _tag === 'None' ? { _tag } : { _tag, lower, upper: lower, fractional: kashida._tag !== 'None' && kashida.fractional } }));
        [elongation, width].forEach(({ element }) => element.addEventListener('change', () => acted(Effect.map(typesetSelection(Option.none()), elongated))));
        sets.forEach(([tag, box]) => box.addEventListener('change', () => acted(Effect.succeedSome({ features: { [tag]: box.checked ? 1 : null } }))));
        buttons.forEach(([{ axis, sign }, button]) => button.addEventListener('click', ({ metaKey, ctrlKey }) => acted(nudged(Option.map(mode.read(), Struct.get(axis)), sign * (metaKey || ctrlKey ? SHORTCUT_MULTIPLE : 1)))));
        Object.assign(root, { textContent: '' }).style.color = 'var(--uxp-host-text-color)';
        [status, controls].forEach((child) => root.appendChild(child));
        const events = ['afterSelectionChanged', 'afterSelectionAttributeChanged', 'afterContextChanged'] as const;
        const listened = (event: (typeof events)[number]) =>
            Effect.acquireRelease(
                Effect.sync(() => app.addEventListener(event, changed)),
                () => Effect.sync(() => app.removeEventListener(event, changed)),
            );
        yield* Effect.forEach(events, listened, { discard: true });
        yield* Effect.sync(changed);
    }, scripted);

// --- [EXPORTS] -------------------------------------------------------------------------

export { typesetting };
