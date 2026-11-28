import type { EngineInterface, EventResult, Frozen, Next, PaneCloseArgs, PaneCloseInput, PaneOpenArgs, UiOpenResult, UiPane, UiScrollArgs } from 'claude-code';
import { Array, Context, Effect, Layer, type ManagedRuntime, Option, PlatformError, PubSub, Record, Struct } from 'effect';
import { runHook } from '#/engine/events';
import { call } from '#/engine/services';
import type { Held, Render } from '#/ui/render';

// --- [SERVICES] ------------------------------------------------------------------------

class Panes extends Context.Service<
    Panes,
    {
        readonly open: (pane: PaneOpenArgs, reveal?: Omit<UiScrollArgs, 'in'>) => Effect.Effect<UiOpenResult, PlatformError.PlatformError>;
        readonly close: (pane: PaneCloseArgs) => Effect.Effect<void, PlatformError.PlatformError>;
        readonly list: Effect.Effect<readonly UiPane[], PlatformError.PlatformError>;
    }
>()('@rasm/claude-code/ui/pane/Panes') {}

// --- [COMPOSITION] ---------------------------------------------------------------------

function panes<S, R, F>($: EngineInterface, table: Held<S, R, F>) {
    const list = call('ui.panes', $.plugin.name, () => $.ui.panes());
    return Layer.succeed(Panes)({
        open: (pane, reveal) =>
            Option.match(
                Option.flatMap(table, (held) => Option.map(Option.fromNullishOr(held.Pane?.[pane.id]), (row) => ({ row, drawn: held.drawn }))),
                {
                    onNone: () => PlatformError.badArgument({ module: 'Panes', method: 'open', description: `no render row, pane ${pane.id}` }),
                    onSome: Effect.fnUntraced(function* ({ row, drawn }) {
                        const open = call('ui.open', pane.id, () => $.ui.open({ ...Struct.omit(row, ['draw', 'closes']), ...pane }));
                        if (reveal === undefined) {
                            return yield* open;
                        }
                        const placed = Array.some(yield* list, ({ id, isPlaced }) => id === pane.id && isPlaced);
                        const settled = PubSub.take(yield* PubSub.subscribe(drawn)).pipe(Effect.repeat({ until: (id) => id === pane.id }));
                        const opened = yield* open;
                        if (opened.isPlaced) {
                            yield* Effect.when(Effect.andThen(settled, Effect.sync(() => $.ui.invalidate('ui.render'))).pipe(Effect.andThen(settled)), Effect.succeed(!placed));
                            yield* call('ui.scroll', pane.id, () => $.ui.scroll({ ...reveal, in: pane.id }), ({ deny }) => deny);
                        }
                        return opened;
                    }, Effect.scoped),
                },
            ),
        close: (pane) => call('ui.close', pane.id, () => $.ui.close(pane)),
        list,
    });
}

function closePane<S, R, E, F>($: EngineInterface, rows: NonNullable<Render<S, R, F>['Pane']>, e: Frozen<PaneCloseInput>, next: Next<'ui.close'>, runtime: () => ManagedRuntime.ManagedRuntime<R, E>) {
    return Option.match(Option.flatMapNullishOr(Record.get(rows, e.id), Struct.get('closes')), {
        onNone: () => next(e),
        onSome: (closes) =>
            runHook(
                runtime(),
                call('ui.panes', e.id, () => $.ui.panes()).pipe(
                    Effect.flatMap((held) => (Array.some(held, ({ id }) => id === e.id) ? Effect.catchCause(closes, (cause) => Effect.as(Effect.logWarning('close failed', cause).pipe(Effect.annotateLogs({ pane: e.id })), true)) : Effect.succeed(true))),
                    Effect.flatMap((passes) => (passes ? call('ui.close', e.id, () => next(e)) : Effect.succeed<EventResult<'ui.close'>>({ value: undefined }))),
                ),
                next.signal,
            ),
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { closePane, Panes, panes };
