import type { EngineInterface, EventResult, Frozen, Next, PaneCloseArgs, PaneCloseInput, PaneOpenArgs, RenderElement, RenderInput, UiOpenResult, UiPane } from 'claude-code';
import { Array, Context, Effect, Layer, type ManagedRuntime, Option, PlatformError, Record, Stream, Struct } from 'effect';
import { Engine } from '#/engine/interface';
import { call } from '#/engine/services';
import { draw, type Element } from '#/ui/element';

// --- [TYPES] ---------------------------------------------------------------------------

type Pane<R = never, F = never> = Omit<PaneOpenArgs, 'id'> & {
    readonly body: (e: Frozen<RenderInput<'Pane'>>) => Effect.Effect<Element<R, F>, F, R>;
    readonly changes?: Stream.Stream<unknown, F, R>;
    readonly closes?: Effect.Effect<boolean, F, R>;
};

// --- [SERVICES] ------------------------------------------------------------------------

class Panes extends Context.Service<
    Panes,
    {
        readonly open: (pane: PaneOpenArgs) => Effect.Effect<UiOpenResult, PlatformError.PlatformError>;
        readonly close: (pane: PaneCloseArgs) => Effect.Effect<void, PlatformError.PlatformError>;
        readonly list: Effect.Effect<readonly UiPane[], PlatformError.PlatformError>;
    }
>()('@rasm/claude-code/ui/pane/Panes') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const watched = <R, F>(rows: Record.ReadonlyRecord<string, Pane<R, F>>) =>
    Layer.effectDiscard(
        Effect.gen(function* () {
            const [held, engine] = yield* Effect.all([Panes, Engine]);
            const placed = (id: string) =>
                Effect.map(
                    held.list,
                    Array.some((pane) => pane.id === id && pane.isPlaced),
                );
            const streams = Record.getSomes(Record.map(rows, ({ changes }) => Option.fromUndefinedOr(changes)));
            yield* Effect.forEach(
                Record.toEntries(streams),
                ([id, changes]) =>
                    changes.pipe(
                        Stream.runForEach(() => Effect.when(engine.invalidate('ui.render'), placed(id))),
                        Effect.catchCause((cause) => Effect.logWarning(`${id} changes failed`, cause)),
                        Effect.forkScoped,
                    ),
                { discard: true },
            );
        }),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

function panes<R, F>($: EngineInterface, rows: Record.ReadonlyRecord<string, Pane<R, F>>) {
    return Layer.succeed(Panes)({
        open: (pane) =>
            Option.match(Record.get(rows, pane.id), {
                onNone: () => PlatformError.badArgument({ module: 'Panes', method: 'open', description: `pane ${pane.id}, which this mod declares no body for` }),
                onSome: (row) => call('ui.open', pane.id, () => $.ui.open({ ...Struct.omit(row, ['body', 'changes', 'closes']), ...pane })),
            }),
        close: (pane) => call('ui.close', pane.id, () => $.ui.close(pane)),
        list: call('ui.panes', $.plugin.name, () => $.ui.panes()),
    });
}

function render<R, E, F>($: EngineInterface, rows: Record.ReadonlyRecord<string, Pane<R, F>>, e: Frozen<RenderInput<'Pane'>>, next: ((e: Frozen<RenderInput<'Pane'>>) => Promise<RenderElement>) & Pick<Next, 'signal'>, runtime: () => ManagedRuntime.ManagedRuntime<R, E>) {
    return Option.match(Record.get(rows, e.requestId), {
        onNone: () => next(e),
        onSome: ({ body }) =>
            runtime().runPromise(
                body(e).pipe(
                    Effect.map((tree) => draw($, e, tree, runtime)),
                    Effect.catchCause((cause) =>
                        Effect.andThen(
                            Effect.logWarning(`Pane ${e.requestId} draw failed`, cause),
                            call('ui.render', e.requestId, () => next(e)),
                        ),
                    ),
                ),
                { signal: next.signal },
            ),
    });
}

function closing<R, E, F>($: EngineInterface, rows: Record.ReadonlyRecord<string, Pane<R, F>>, e: Frozen<PaneCloseInput>, next: (e: Frozen<PaneCloseInput>) => Promise<EventResult<'ui.close'>>, runtime: () => ManagedRuntime.ManagedRuntime<R, E>) {
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

export { closing, type Pane, Panes, panes, render, watched };
