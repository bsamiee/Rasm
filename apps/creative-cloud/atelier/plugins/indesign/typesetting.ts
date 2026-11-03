import type { Text } from 'adobe:indesign';
import { HostError } from '@rasm/automation/requests';
import type { host } from '@rasm/indesign/host';
import { Model } from '@rasm/indesign/model';
import { HostSettings, KASHIDA_WIDTH, LONGEST_KASHIDA_WIDTH } from '@rasm/indesign/settings';
import { commit } from '@rasm/indesign/transactions';
import { adjustText } from '@rasm/indesign/typesetting';
import { Settings } from '@rasm/typography/settings';
import { Array, Cause, Console, Effect, FiberSet, flow, Option, Record, Ref, Schema, Struct } from 'effect';

// --- [OPERATIONS] ----------------------------------------------------------------------

const logged = flow(Cause.pretty, Console.error);
const row = (...children: readonly HTMLElement[]) => {
    const element = document.createElement('div');
    element.style.display = 'flex';
    element.style.flexWrap = 'wrap';
    element.style.alignItems = 'center';
    children.forEach((child) => element.appendChild(child));
    return element;
};
const picker = <L extends string>(literals: readonly L[], chosen: (value: L) => void) => {
    const element = document.createElement('select');
    literals.forEach((literal) => element.appendChild(Object.assign(document.createElement('option'), { value: literal, textContent: literal })));
    element.addEventListener('change', () =>
        Option.map(
            Array.findFirst(literals, (literal) => literal === element.value),
            chosen,
        ),
    );
    return element;
};
const counter = (title: string, max: number, chosen: (value: number) => void) => {
    const element = Object.assign(document.createElement('input'), { type: 'number', title, min: '0', max: `${max}`, step: '1' });
    element.addEventListener('change', () => Option.map(Schema.decodeOption(KASHIDA_WIDTH)(element.valueAsNumber), chosen));
    return element;
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const typesetting = (local: ReturnType<typeof host>['local']) => {
    const shortcutMultiple = 5;
    const selectionChanged = 'afterSelectionChanged';
    const modes = {
        Marks: { horizontal: 'markX', vertical: 'markY' },
        Kerning: { horizontal: 'kerning', vertical: 'baseline' },
        Tracking: { horizontal: 'tracking', vertical: 'baseline' },
    } as const;
    const arrows = [
        { label: '←', axis: 'horizontal', sign: -1 },
        { label: '→', axis: 'horizontal', sign: 1 },
        { label: '↑', axis: 'vertical', sign: 1 },
        { label: '↓', axis: 'vertical', sign: -1 },
    ] as const;
    const selected = Model.use(({ app, text }) => Effect.try({ try: () => Option.filter(Array.head(app.selection), text), catch: HostError.refused('selection') }));
    const written = <A, E, R>(label: string, write: (text: Text) => Effect.Effect<A, E, R>) =>
        Effect.flatMap(
            selected,
            Option.match({
                onNone: () => Effect.void,
                onSome: (text) => Effect.asVoid(commit(label, write(text))),
            }),
        );
    const kashida = (change: Partial<typeof Settings.Type.kashida>) => written('Set kashida', (text) => Effect.flatMap(HostSettings.read(text), (settings) => HostSettings.write(text, { kashida: { ...settings.kashida, ...change } })));
    const stylistic = (tag: string, enabled: boolean) => written(`Set ${tag}`, (text) => Effect.flatMap(HostSettings.read(text), ({ features }) => HostSettings.write(text, { features: enabled ? Record.set(features, tag, 1) : Record.remove(features, tag) })));
    const nudged = (axis: 'horizontal' | 'vertical', multiple: number) =>
        Effect.fnUntraced(function* (mode: keyof typeof modes) {
            const { app } = yield* Model;
            const em = HostSettings.em(multiple * app.textPreferences.kerningKeyIncrement);
            const deltas = {
                markX: { marks: { x: em, y: 0 } },
                markY: { marks: { x: 0, y: em } },
                kerning: { kerning: em },
                tracking: { tracking: em },
                baseline: { baselineShift: multiple * app.textPreferences.baselineShiftKeyIncrement },
            }[modes[mode][axis]];
            return yield* written(`Adjust ${Struct.keys(deltas).join(', ')}`, (text) => adjustText(text, deltas));
        });
    const shown = Effect.fnUntraced(function* (root: HTMLElement) {
        const [{ app }, fork, mode] = yield* Effect.all([Model, FiberSet.makeRuntime<never, void, never>(), Ref.make<keyof typeof modes>('Marks')]);
        const acted = <E>(work: Effect.Effect<unknown, E, Model>) => fork(local(Effect.andThen(work, refresh)).pipe(Effect.catchCause(logged)));
        const insertion = picker(Settings.fields.kashida.schema.fields.insertion.schema.literals, (value) => acted(kashida({ insertion: value })));
        const width = counter('Kashida width, a fixed elongation degree and bound', LONGEST_KASHIDA_WIDTH, (value) => acted(kashida({ degree: value, upper: value })));
        const sets = HostSettings.stylisticSets.map((tag) => {
            const box = Object.assign(document.createElement('input'), { type: 'checkbox', title: tag });
            box.addEventListener('change', () => acted(stylistic(tag, box.checked)));
            return { tag, box };
        });
        const buttons = arrows.map(({ label, axis, sign }) => {
            const button = Object.assign(document.createElement('button'), { textContent: label });
            button.addEventListener('click', ({ metaKey, ctrlKey }) => acted(Effect.flatMap(Ref.get(mode), nudged(axis, sign * (metaKey || ctrlKey ? shortcutMultiple : 1)))));
            return button;
        });
        const empty = Object.assign(document.createElement('div'), { textContent: 'Select text' });
        const controls = row(
            picker(Struct.keys(modes), (value) => fork(Ref.set(mode, value))),
            ...buttons,
            insertion,
            width,
            ...sets.map(({ tag, box }) => row(box, Object.assign(document.createElement('span'), { textContent: tag }))),
        );
        const body = document.createElement('div');
        const shownSettings = ({ kashida: { insertion: chosen, degree }, features }: Settings) => {
            insertion.value = chosen;
            width.valueAsNumber = degree;
            sets.forEach(({ tag, box }) => Object.assign(box, { checked: Record.has(features, tag) }));
        };
        const refresh = Effect.gen(function* () {
            const selection = yield* selected;
            controls.style.display = Option.isSome(selection) ? 'flex' : 'none';
            empty.style.display = Option.isSome(selection) ? 'none' : 'block';
            yield* Option.match(selection, { onNone: () => Effect.void, onSome: (text) => Effect.map(HostSettings.read(text), shownSettings) });
        });
        const changed = () => fork(local(refresh).pipe(Effect.catchCause(logged)));
        body.style.color = 'var(--uxp-host-text-color)';
        body.appendChild(empty);
        body.appendChild(controls);
        Object.assign(root, { textContent: '' }).appendChild(body);
        yield* Effect.acquireRelease(
            Effect.sync(() => app.addEventListener(selectionChanged, changed)),
            () => Effect.sync(() => app.removeEventListener(selectionChanged, changed)),
        );
        yield* refresh;
    });
    return flow(shown, local);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { typesetting };
