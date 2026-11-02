import type { Text } from 'adobe:indesign';
import { HostError } from '@rasm/automation/requests';
import { adjustText } from '@rasm/indesign/adjust';
import type { host } from '@rasm/indesign/host';
import { Model } from '@rasm/indesign/model';
import { HostSettings, STYLISTIC_TAGS, THOUSANDTHS } from '@rasm/indesign/settings';
import { commit } from '@rasm/indesign/transactions';
import { Settings } from '@rasm/typography/settings';
import { Array, Cause, Console, Effect, Exit, FiberSet, flow, Option, Ref, Scope, Struct } from 'effect';

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
    const held = Ref.makeUnsafe(Option.none<Scope.Closeable>());
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
    const stylistic = (tag: string, enabled: boolean) => written(`Set ${tag}`, (text) => Effect.flatMap(HostSettings.read(text), ({ features }) => HostSettings.write(text, { features: enabled ? Array.union(features, [tag]) : features.filter((feature) => feature !== tag) })));
    const nudged = (axis: 'horizontal' | 'vertical', multiple: number) =>
        Effect.fnUntraced(function* (mode: keyof typeof modes) {
            const { app } = yield* Model;
            const em = (multiple * app.textPreferences.kerningKeyIncrement) / THOUSANDTHS;
            const delta = {
                markX: { _tag: 'Marks' as const, x: em, y: 0 },
                markY: { _tag: 'Marks' as const, x: 0, y: em },
                kerning: { _tag: 'Kerning' as const, em },
                tracking: { _tag: 'Tracking' as const, em },
                baseline: { _tag: 'BaselineShift' as const, points: multiple * app.textPreferences.baselineShiftKeyIncrement },
            }[modes[mode][axis]];
            return yield* written(`Adjust ${delta._tag}`, (text) => adjustText(text, [delta]));
        });
    const hide = () => Effect.flatMap(Ref.getAndSet(held, Option.none()), Option.match({ onNone: () => Effect.void, onSome: (scope) => Scope.close(scope, Exit.void) }));
    const shown = Effect.fnUntraced(function* (root: HTMLElement) {
        const [{ app }, fork, mode] = yield* Effect.all([Model, FiberSet.makeRuntime<never, void, never>(), Ref.make<keyof typeof modes>('Marks')]);
        const acted = <E>(work: Effect.Effect<unknown, E, Model>) => fork(local(Effect.andThen(work, refresh)).pipe(Effect.catchCause(logged)));
        const insertion = picker(Settings.fields.kashida.schema.fields.insertion.schema.literals, (value) => acted(kashida({ insertion: value })));
        const length = picker(Settings.fields.kashida.schema.fields.length.schema.literals, (value) => acted(kashida({ length: value })));
        const sets = STYLISTIC_TAGS.map((tag) => {
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
            length,
            ...sets.map(({ tag, box }) => row(box, Object.assign(document.createElement('span'), { textContent: tag }))),
        );
        const body = document.createElement('div');
        const refresh = Effect.gen(function* () {
            const selection = yield* selected;
            controls.style.display = Option.isSome(selection) ? 'flex' : 'none';
            empty.style.display = Option.isSome(selection) ? 'none' : 'block';
            if (Option.isSome(selection)) {
                const settings = yield* HostSettings.read(selection.value);
                insertion.value = settings.kashida.insertion;
                length.value = settings.kashida.length;
                sets.forEach(({ tag, box }) => Object.assign(box, { checked: settings.features.includes(tag) }));
            }
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
    return {
        show: Effect.fnUntraced(function* (root: HTMLElement) {
            yield* hide();
            const scope = yield* Scope.make();
            yield* Ref.set(held, Option.some(scope));
            yield* local(Scope.provide(shown(root), scope));
        }),
        hide,
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { typesetting };
