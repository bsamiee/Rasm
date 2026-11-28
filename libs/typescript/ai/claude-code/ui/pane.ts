import type { EngineInterface, EventResult, Frozen, Next, PaneCloseArgs, PaneCloseInput, PaneOpenArgs, UiScrollArgs } from 'claude-code';
import { Array, Context, Effect, type ManagedRuntime, Option, PlatformError, PubSub, Record, Struct } from 'effect';
import { runHook } from '#/engine/events';
import { call } from '#/engine/services';
import type { Held, Render } from '#/ui/render';

// --- [SERVICES] ------------------------------------------------------------------------

class Panes extends Context.Service<Panes, ReturnType<typeof makePanes>>()('@rasm/claude-code/ui/pane/Panes') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const closePane = <S, R, E, F>(rows: NonNullable<Render<S, R, F>['Pane']>, e: Frozen<PaneCloseInput>, next: Next<'ui.close'>, runtime: () => ManagedRuntime.ManagedRuntime<NoInfer<R> | Panes, E>) =>
    Option.match(Option.flatMapNullishOr(Record.get(rows, e.id), Struct.get('closes')), {
        onNone: () => next(e),
        onSome: (closes) =>
            runHook(
                runtime(),
                Panes.use(({ list }) => list).pipe(
                    Effect.flatMap((held) => (Array.some(held, ({ id }) => id === e.id) ? Effect.catchCause(closes, (cause) => Effect.as(Effect.logWarning('close failed', cause).pipe(Effect.annotateLogs({ pane: e.id })), true)) : Effect.succeed(true))),
                    Effect.flatMap((passes) => (passes ? call('ui.close', e.id, () => next(e)) : Effect.succeed<EventResult<'ui.close'>>({ value: undefined }))),
                ),
                next.signal,
            ),
    });

// --- [COMPOSITION] ---------------------------------------------------------------------

function makePanes<S, R, F>($: EngineInterface, table: Held<S, R, F>) {
    const list = call('ui.panes', $.plugin.name, () => $.ui.panes());
    return {
        open: Effect.fnUntraced(function* (pane: PaneOpenArgs, reveal?: Omit<UiScrollArgs, 'in'>) {
            const { row, drawn } = yield* Effect.fromOption(
                Option.flatMap(table, (held) => Option.all({ row: Option.fromNullishOr(held.Pane?.[pane.id]), drawn: Option.some(held.drawn) })),
                () => PlatformError.badArgument({ module: 'Panes', method: 'open', description: `no render row, pane ${pane.id}` }),
            );
            const open = call('ui.open', pane.id, () => $.ui.open({ ...Struct.omit(row, ['draw', 'closes']), ...pane }));
            if (reveal === undefined) {
                return yield* open;
            }
            const placed = Array.some(yield* list, ({ id, isPlaced }) => id === pane.id && isPlaced);
            const settled = PubSub.take(yield* PubSub.subscribe(drawn)).pipe(Effect.repeat({ until: (id) => id === pane.id }));
            const opened = yield* open;
            yield* Effect.when(settled.pipe(Effect.andThen(Effect.sync(() => $.ui.invalidate('ui.render'))), Effect.andThen(settled)), Effect.succeed(opened.isPlaced && !placed));
            yield* Effect.when(
                call(
                    'ui.scroll',
                    pane.id,
                    () => $.ui.scroll({ ...reveal, in: pane.id }),
                    ({ deny }) => deny,
                ),
                Effect.succeed(opened.isPlaced),
            );
            return opened;
        }, Effect.scoped),
        close: (pane: PaneCloseArgs) => call('ui.close', pane.id, () => $.ui.close(pane)),
        list,
    };
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { closePane, Panes, makePanes };
