import { HostError } from '@rasm/automation/requests';
import { Adjust, Typeset } from '@rasm/automation/typesetting';
import type { host } from '@rasm/indesign/host';
import { Model } from '@rasm/indesign/model';
import { HostSettings } from '@rasm/indesign/settings';
import { adjust, typeset } from '@rasm/indesign/typesetting';
import { Settings } from '@rasm/typography/settings';
import { Array, Cause, Effect, Function, flow, Number, Option, Record, Schema, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Steps = Required<Pick<Adjust['deltas'], 'kerning' | 'baselineShift'>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const SHORTCUT_MULTIPLE = 5;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [CONTROLS]

const row = (...children: readonly HTMLElement[]) => {
    const element = document.createElement('div');
    element.style.display = 'flex';
    element.style.flexWrap = 'wrap';
    element.style.alignItems = 'center';
    children.forEach((child) => element.appendChild(child));
    return element;
};
const picker = <A>(options: readonly (readonly [string, A])[]) => {
    const element = document.createElement('select');
    options.forEach(([text]) => element.appendChild(Object.assign(document.createElement('option'), { textContent: text })));
    return { element, read: () => Option.map(Array.get(options, element.selectedIndex), ([, value]) => value), show: (value: unknown) => Object.assign(element, { selectedIndex: options.findIndex(([, each]) => each === value) }) };
};

// --- [REQUESTS]

const target = Effect.map(Model.selection, (item) => item.toSpecifier());
const typesetSelection = (settings: Option.Option<typeof Settings.Partial.Type>) => Effect.flatMap(target, (path) => typeset(Typeset.make({ target: path, settings })));
const steps = Effect.fnUntraced(function* (multiple: number) {
    const { app } = yield* Model;
    const { kerning, baselineShift } = yield* Effect.try({
        try: () => ({ kerning: multiple * app.textPreferences.kerningKeyIncrement, baselineShift: multiple * app.textPreferences.baselineShiftKeyIncrement }),
        catch: HostError.refused('text preferences'),
    });
    return { kerning: yield* Effect.orDie(Schema.decodeEffect(HostSettings.Thousandths)(kerning)), baselineShift } satisfies Steps;
});
const nudged = (deltas: Option.Option<(steps: Steps) => Adjust['deltas']>, multiple: number) => Effect.flatMap(Effect.all([target, steps(multiple), Effect.fromOption(deltas)]), ([path, step, delta]) => adjust(Adjust.make({ target: path, deltas: delta(step) })));

// --- [COMPOSITION] ---------------------------------------------------------------------

const typesetting = (scripted: ReturnType<typeof host>['scripted']) =>
    Effect.fnUntraced(function* (root: HTMLElement, fork: (work: Effect.Effect<unknown, unknown>) => void) {
        const { app } = yield* Model;
        const status = document.createElement('div');
        const noted = (text: string) => Effect.sync(() => Object.assign(status, { textContent: text }));
        const acted = <E>(work: Effect.Effect<unknown, E, Model>) =>
            fork(
                scripted(work).pipe(
                    Effect.tap(() => noted('')),
                    Effect.tapCause(flow(Cause.pretty, noted)),
                ),
            );
        const written = <E>(change: Effect.Effect<Option.Option<typeof Settings.Partial.Type>, E, Model>) => acted(change.pipe(Effect.flatMap(typesetSelection), Effect.flatMap(displayed)));
        const mode = picker(
            Record.toEntries({
                Marks: { horizontal: ({ kerning }) => ({ marks: { x: kerning, y: 0 } }), vertical: ({ kerning }) => ({ marks: { x: 0, y: kerning } }) },
                Kerning: { horizontal: ({ kerning }) => ({ kerning }), vertical: ({ baselineShift }) => ({ baselineShift }) },
                Tracking: { horizontal: ({ kerning }) => ({ tracking: kerning }), vertical: ({ baselineShift }) => ({ baselineShift }) },
            } as const satisfies Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, (steps: Steps) => Adjust['deltas']>>),
        );
        const elongation = picker(Array.map(Struct.keys(Settings.fields.kashida.schema.cases), (tag) => [tag, tag] as const));
        const width = picker(Record.toEntries(HostSettings.kashidaWidths).filter(([, each]) => each !== HostSettings.kashidaWidths.None));
        const elongated = ({ kashida }: Settings) => Option.map(Option.all([elongation.read(), width.read()]), ([_tag, lower]) => ({ kashida: _tag === 'None' ? { _tag } : { _tag, lower, upper: lower, fractional: kashida._tag !== 'None' && kashida.fractional } }));
        [elongation, width].forEach(({ element }) => element.addEventListener('change', () => written(Effect.map(typesetSelection(Option.none()), elongated))));
        const sets = HostSettings.stylisticSets.map((tag) => {
            const box = Object.assign(document.createElement('input'), { type: 'checkbox', title: tag });
            box.addEventListener('change', () => written(Effect.succeedSome({ features: { [tag]: box.checked ? 1 : null } })));
            return { tag, box };
        });
        const buttons = Record.collect({ '←': { axis: 'horizontal', sign: -1 }, '→': { axis: 'horizontal', sign: 1 }, '↑': { axis: 'vertical', sign: 1 }, '↓': { axis: 'vertical', sign: -1 } } as const, (label, { axis, sign }) => {
            const button = Object.assign(document.createElement('button'), { textContent: label });
            button.addEventListener('click', ({ metaKey, ctrlKey }) => acted(nudged(Option.map(mode.read(), Struct.get(axis)), sign * (metaKey || ctrlKey ? SHORTCUT_MULTIPLE : 1))));
            return button;
        });
        const controls = row(mode.element, ...buttons, elongation.element, width.element, ...sets.map(({ tag, box }) => row(box, Object.assign(document.createElement('span'), { textContent: tag }))));
        const displayed = ({ kashida, features }: Settings) =>
            Effect.sync(() => {
                controls.style.display = 'flex';
                elongation.show(kashida._tag);
                if (kashida._tag !== 'None') {
                    width.show(kashida.lower);
                }
                sets.forEach(({ tag, box }) => Object.assign(box, { checked: Option.exists(Record.get(features, tag), Number.isGreaterThan(0)) }));
            });
        const refresh = typesetSelection(Option.none()).pipe(
            Effect.matchCauseEffect({
                onFailure: (cause) =>
                    Effect.sync(() => {
                        controls.style.display = 'none';
                        status.textContent = Cause.pretty(cause);
                    }),
                onSuccess: (settings) => Effect.andThen(noted(''), displayed(settings)),
            }),
        );
        const changed = () => fork(scripted(refresh));
        const body = document.createElement('div');
        body.style.color = 'var(--uxp-host-text-color)';
        body.appendChild(status);
        body.appendChild(controls);
        Object.assign(root, { textContent: '' }).appendChild(body);
        yield* Effect.forEach(
            ['afterSelectionChanged', 'afterSelectionAttributeChanged', 'afterContextChanged'] as const,
            (event) =>
                Effect.acquireRelease(
                    Effect.sync(() => app.addEventListener(event, changed)),
                    Function.constant(Effect.sync(() => app.removeEventListener(event, changed))),
                ),
            { discard: true },
        );
        yield* refresh;
    }, scripted);

// --- [EXPORTS] -------------------------------------------------------------------------

export { typesetting };
