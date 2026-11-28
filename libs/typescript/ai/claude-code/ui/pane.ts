import type { EngineInterface, EventResult, Frozen, PaneCloseArgs, PaneCloseInput, PaneOpenArgs, UiOpenResult, UiPane } from 'claude-code';
import { Array, Context, Effect, Layer, type ManagedRuntime, Option, PlatformError, Record, Struct } from 'effect';
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
                onNone: () => PlatformError.badArgument({ module: 'Panes', method: 'open', description: `pane ${pane.id}, which this mod declares no body for` }),
                onSome: (row) => call('ui.open', pane.id, () => $.ui.open({ ...Struct.omit(row, ['draw', 'closes']), ...pane })),
            }),
        close: (pane) => call('ui.close', pane.id, () => $.ui.close(pane)),
        list: call('ui.panes', $.plugin.name, () => $.ui.panes()),
    });
}

function closing<S, R, E, F>($: EngineInterface, rows: NonNullable<Render<S, R, F>['Pane']>, e: Frozen<PaneCloseInput>, next: (e: Frozen<PaneCloseInput>) => Promise<EventResult<'ui.close'>>, runtime: () => ManagedRuntime.ManagedRuntime<R, E>) {
    const kept: EventResult<'ui.close'> = { value: undefined };
    return Option.match(Option.flatMapNullishOr(Record.get(rows, e.id), Struct.get('closes')), {
        onNone: () => next(e),
        onSome: (closes) =>
            runtime().runPromise(
                call('ui.panes', e.id, () => $.ui.panes()).pipe(
                    Effect.flatMap((held) => (Array.some(held, ({ id }) => id === e.id) ? Effect.catchCause(closes, (cause) => Effect.as(Effect.logWarning(`Pane ${e.id} close failed`, cause), true)) : Effect.succeed(true))),
                    Effect.flatMap((passes) => (passes ? call('ui.close', e.id, () => next(e)) : Effect.succeed(kept))),
                ),
            ),
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { closing, Panes, panes };
