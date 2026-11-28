import type { EngineInterface, Frozen, PaneOpenArgs, RenderComponent, RenderElement, RenderInput, RenderPropsOf, ThemeScene, UiPointerInput, UiPointerResult } from 'claude-code';
import { Effect, Layer, type ManagedRuntime, Match, Option, type PlatformError, Predicate, type Record, Ref, Stream } from 'effect';
import { Engine } from '#/engine/interface';
import { call } from '#/engine/services';
import { draw, type Element } from '#/ui/element';

// --- [TYPES] ---------------------------------------------------------------------------

type Keyed = 'Backdrop' | 'Pane';
type Redraw<C extends RenderComponent> = (rewrite?: Partial<Frozen<RenderPropsOf[C]>>) => Effect.Effect<RenderElement, PlatformError.PlatformError>;
type Row<C extends RenderComponent, S, R, F> = (e: Frozen<RenderInput<C>>, state: S, next: Redraw<C>) => Effect.Effect<Element<R, F> | RenderElement, F | PlatformError.PlatformError, R>;
type Rows<S, R, F> = { readonly [C in Exclude<RenderComponent, Keyed>]: Row<C, S, R, F> };
interface Scene<S, R, F> {
    readonly draw: Row<'Backdrop', S, R, F>;
    readonly pointer?: (e: Frozen<UiPointerInput>, state: S) => Effect.Effect<unknown, F, R>;
}
interface Pane<S, R, F> extends Omit<PaneOpenArgs, 'id'> {
    readonly draw: Row<'Pane', S, R, F>;
    readonly closes?: Effect.Effect<boolean, F, R>;
}
type Render<S, R, F> = Partial<Rows<S, R, F>> & {
    readonly state: Stream.Stream<S, F, R>;
    readonly Backdrop?: Record.ReadonlyRecord<ThemeScene['scene'], Scene<S, R, F>>;
    readonly Pane?: Record.ReadonlyRecord<PaneOpenArgs['id'], Pane<S, R, F>>;
};
type Held<S, R, F> = Option.Option<Render<S, R, F> & { readonly held: Ref.Ref<Option.Option<S>> }>;
type Drawn<C extends RenderComponent> = (e: Frozen<RenderInput<C>>) => Promise<RenderElement>;

// --- [OPERATIONS] ----------------------------------------------------------------------

const redrawn = Engine.use((engine) => engine.invalidate('ui.render'));
const watched = <S, R, F>(table: Held<S, R, F>) =>
    Option.match(table, {
        onNone: () => Layer.empty,
        onSome: ({ held, state }) =>
            state.pipe(
                Stream.changes,
                Stream.runForEach((value) => Effect.andThen(Ref.set(held, Option.some(value)), redrawn)),
                Effect.catchCause((cause) => Effect.logWarning('render state failed', cause)),
                Effect.forkScoped,
                Effect.andThen(Effect.addFinalizer(() => Effect.andThen(Ref.set(held, Option.none()), redrawn))),
                Layer.effectDiscard,
            ),
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

function drawn<C extends RenderComponent, S, R, E, F>($: EngineInterface, row: Option.Option<Row<C, S, R, F>>, state: S, e: Frozen<RenderInput<C>>, next: Drawn<C>, runtime: () => ManagedRuntime.ManagedRuntime<R, E>) {
    const redraw: Redraw<C> = (rewrite) => call('ui.render', e.requestId, () => next(rewrite === undefined ? e : { ...e, props: { ...e.props, ...rewrite } }));
    return Option.match(row, {
        onNone: () => next(e),
        onSome: (answer) =>
            runtime().runPromise(
                answer(e, state, redraw).pipe(
                    Effect.map((tree) => (Predicate.hasProperty(tree, '_tag') ? draw($, e, tree, runtime) : tree)),
                    Effect.catchCause((cause) => Effect.andThen(Effect.logWarning(`${e.component} ${e.requestId} draw failed`, cause), redraw())),
                ),
            ),
    });
}

function rows<C extends Exclude<RenderComponent, Keyed>, S, R, E, F>($: EngineInterface, table: Partial<Rows<S, R, F>>, state: S, component: C, e: Frozen<RenderInput<C>>, next: Drawn<C>, runtime: () => ManagedRuntime.ManagedRuntime<R, E>) {
    return drawn($, Option.fromUndefinedOr(table[component]), state, e, next, runtime);
}

function render<S, R, E, F>($: EngineInterface, table: Held<S, R, F>, e: Frozen<RenderInput>, next: Drawn<RenderComponent>, runtime: () => ManagedRuntime.ManagedRuntime<R, E>) {
    runtime();
    return Option.match(
        table.pipe(
            Option.filter(() => !Predicate.hasProperty(e.props, 'plugin') || e.props.plugin === $.plugin.name),
            Option.flatMap((held) => Option.map(Ref.getUnsafe(held.held), (state) => ({ held, state }))),
        ),
        {
            onNone: () => next(e),
            onSome: ({ held, state }) =>
                Match.value(e).pipe(
                    Match.when({ component: 'Backdrop' }, (drawing) => drawn($, Option.fromUndefinedOr(held.Backdrop?.[drawing.props.scene]?.draw), state, drawing, next, runtime)),
                    Match.when({ component: 'Pane' }, (drawing) => drawn($, Option.fromUndefinedOr(held.Pane?.[drawing.requestId]?.draw), state, drawing, next, runtime)),
                    Match.orElse((drawing) => rows($, held, state, drawing.component, drawing, next, runtime)),
                ),
        },
    );
}

function pointer<S, R, E, F>($: EngineInterface, table: Held<S, R, F>, e: Frozen<UiPointerInput>, next: (e: Frozen<UiPointerInput>) => Promise<UiPointerResult>, runtime: () => ManagedRuntime.ManagedRuntime<R, E>) {
    return Option.match(
        Option.flatMap(table, (held) => Option.all({ heard: Option.fromUndefinedOr(held.Backdrop?.[e.scene]?.pointer), state: Ref.getUnsafe(held.held) })),
        {
            onNone: () => next(e),
            onSome: ({ heard, state }) =>
                runtime().runPromise(
                    heard(e, state).pipe(
                        Effect.catchCause((cause) => Effect.logWarning(`Backdrop ${e.scene} pointer failed`, cause)),
                        Effect.andThen(Effect.sync(() => $.ui.invalidate('ui.render'))),
                        Effect.andThen(call('ui.pointer', e.scene, () => next(e))),
                    ),
                ),
        },
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { pointer, type Render, render, watched };
