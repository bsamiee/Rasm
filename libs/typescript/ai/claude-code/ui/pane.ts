import type { EngineInterface, EventResult, Frozen, Next, PaneCloseArgs, PaneCloseInput, PaneOpenArgs, UiOpenResult, UiPane } from 'claude-code';
import { Array, Context, Effect, Layer, type ManagedRuntime, Option, PlatformError, Record, Struct } from 'effect';
import { runHook } from '#/engine/events';
import { call } from '#/engine/services';
import type { Render } from '#/ui/render';

// --- [SERVICES] ------------------------------------------------------------------------

class Panes extends Context.Service<
    Panes,
    {
        readonly open: (pane: PaneOpenArgs) => Effect.Effect<UiOpenResult, PlatformError.PlatformError>;
        readonly close: (pane: PaneCloseArgs) => Effect.Effect<void, PlatformError.PlatformError>;
        readonly list: Effect.Effect<readonly UiPane[], PlatformError.PlatformError>;
    }
>()('@rasm/claude-code/ui/pane/Panes') {}

// --- [COMPOSITION] ---------------------------------------------------------------------

function panes<S, R, F>($: EngineInterface, rows: NonNullable<Render<S, R, F>['Pane']>) {
    return Layer.succeed(Panes)({
        open: (pane) =>
            Option.match(Record.get(rows, pane.id), {
                onNone: () => PlatformError.badArgument({ module: 'Panes', method: 'open', description: `no render row, pane ${pane.id}` }),
                onSome: (row) => call('ui.open', pane.id, () => $.ui.open({ ...Struct.omit(row, ['draw', 'closes']), ...pane })),
            }),
        close: (pane) => call('ui.close', pane.id, () => $.ui.close(pane)),
        list: call('ui.panes', $.plugin.name, () => $.ui.panes()),
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
