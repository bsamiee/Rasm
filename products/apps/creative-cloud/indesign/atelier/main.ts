import { Adjust, Typeset, Typesetting } from '@rasm/indesign';
import { adjust, call, change, findChange, font, inspect, KASHIDA_WIDTHS, kerns, listened, Model, preflight, render, runs, STYLISTIC_TAGS, session, thousandths, typeset } from '@rasm/indesign/uxp';
import { Link, type Upstream } from '@rasm/mcp';
import { HostError, Rendering } from '@rasm/object-model';
import { read } from '@rasm/object-model/host';
import { Settings } from '@rasm/typography';
import { UxpManifest } from '@rasm/uxp';
import { Plugin, Uxp } from '@rasm/uxp/plugin';
import { picker, widget } from '@rasm/uxp/spectrum';
import { Array, Cause, Effect, Function, flow, Layer, Match, Number, Option, type Path, Predicate, Queue, Record, Schema, Scope, Semaphore, Stream, Struct } from 'effect';
import declared from 'indesign-uxp-types/flat?inline';
import { InDesign } from '#/tools';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SHORTCUT_MULTIPLE = 5;

// --- [MODELS] --------------------------------------------------------------------------

class Steps extends Schema.Opaque<Steps, unknown>()(Schema.Struct({ kerning: thousandths, baselineShift: Schema.Finite })) {}
const elongating = Settings.fields.kashida.schema.isAnyOf(['Tatweel', 'Stretched']);

// --- [OPERATIONS] ----------------------------------------------------------------------

const typesetSelection = (settings: Typeset['settings']) => Effect.flatMap(Model.selection, (item) => typeset(Typeset.make({ target: item.toSpecifier(), settings })));
const nudged = Effect.fnUntraced(function* (deltas: Option.Option<(steps: Steps) => Adjust['deltas']>, multiple: number) {
    const [{ app, owning }, item, delta] = yield* Effect.all([Model, Model.selection, Effect.fromOption(deltas)]);
    const styled = yield* Effect.fromOption(Option.liftPredicate(item, owning('textStyleRanges')), () => new Cause.NoSuchElementError(`text style range in selection ${item.toSpecifier()}`));
    const steps = yield* Model.read(Steps, 'text preferences', () => ({ kerning: multiple * app.textPreferences.kerningKeyIncrement, baselineShift: (multiple * app.textPreferences.baselineShiftKeyIncrement) / styled.textStyleRanges.firstItem().pointSize }));
    yield* adjust(Adjust.make({ target: item.toSpecifier(), deltas: delta(steps) }));
    return Option.none();
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const [model, permit] = Effect.runSync(Effect.all([Model.make(declared), Semaphore.make(1)]));
const scripted = <A, E, R>(effect: Effect.Effect<A, E, R>) => Effect.scopedWith((scope) => Effect.andThen(Scope.provide(session, scope), effect)).pipe(Semaphore.withPermit(permit), Effect.provideService(Model, model));
const notices = (extension: string) =>
    Stream.callback<Stream.Success<Upstream['notices']>>((queue) =>
        listened([model.Document.AFTER_OPEN, model.Document.AFTER_SAVE, model.Document.BEFORE_CLOSE], extension, (event) => {
            const [name, target] = [read(event, 'eventType'), read(event, 'target')];
            if (Predicate.isString(name) && model.document(target)) {
                Queue.offerUnsafe(queue, { name, target: Option.some(target.toSpecifier()), detail: Option.none() });
            }
        }).pipe(Effect.provideService(Model, model)),
    );
const linked = (extension: string, root: Effect.Effect<URL, HostError, Path.Path>) =>
    Link.dial(
        Rendering.merge(Typesetting),
        Layer.mergeAll(
            Link.serve(InDesign, { notices: notices(extension) }).pipe(Layer.provide(InDesign.toLayer({ read: flow(inspect, scripted), change: flow(change, scripted), call: flow(call, scripted), find_change: flow(findChange, scripted), adjust: flow(adjust, scripted), preflight: flow(preflight, scripted) }))),
            Rendering.toLayer({ render: ({ target }) => scripted(Uxp.rendered((file) => render(target, file.nativePath), 'rendered.png')) }),
            Typesetting.toLayer({ runs: flow(runs, scripted), kerns: flow(kerns, scripted), typeset: flow(typeset, scripted), font: flow(font, scripted) }),
        ),
        new URL(UxpManifest.link),
        Effect.all([Effect.tryPromise({ try: () => model.app.fullName, catch: HostError.refused('application fullName') }).pipe(Effect.flatMap(Schema.decodeUnknownEffect(Uxp.Application))), root]).pipe(Effect.map(([{ url }, folder]) => Link.make({ application: url, extension, root: Option.some(folder) }))),
    );
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
    const drawn = <E>(requested: Effect.Effect<Typeset['settings'], E, Model>) => scripted(Effect.flatMap(requested, typesetSelection)).pipe(Effect.flatMap(displayed));
    const changed = () => fork(drawn(Effect.succeedNone).pipe(Effect.onError(Function.constant(Effect.sync(() => withheld(inputs)))), Effect.catchTag('Unmatched', Function.constant(Effect.void))));
    const acted = flow(drawn, Effect.onError(Function.constant(Effect.sync(changed))), fork);
    const elongated = ({ kashida }: Settings) =>
        Option.map(Option.all([elongation.read(), width.read()]), ([tag, lower]) => ({
            kashida: Match.value(tag).pipe(
                Match.when('None', () => Settings.fields.kashida.schema.cases.None.make({})),
                Match.orElse((stretch) => Settings.fields.kashida.schema.cases[stretch].make({ ...(elongating(kashida) ? Struct.omit(kashida, ['_tag']) : {}), lower, upper: lower })),
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

Plugin.serve({ link: linked, panels: { typesetting: panel } });
