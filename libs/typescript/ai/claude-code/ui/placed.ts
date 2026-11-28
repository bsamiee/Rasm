import type { Args, EngineInterface, EventResult, Frozen, OnScreen, RasterProps, RenderInput, RenderSurface, UiBlitArgs } from 'claude-code';
import { Array, Context, Effect, Function, Option, Record, Struct, SubscriptionRef } from 'effect';
import { call } from '#/engine/services';

// --- [TYPES] ---------------------------------------------------------------------------

interface Row {
    readonly requestId: string;
    readonly onScreen: Partial<Record.ReadonlyRecord<RenderSurface, Option.Option<OnScreen>>>;
    readonly fullscreen: boolean;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const UNSET = 0x01_00_00_00;

// --- [SERVICES] ------------------------------------------------------------------------

class Placed extends Context.Service<Placed, ReturnType<typeof makePlaced>>()('@rasm/claude-code/ui/placed') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const paint = (props: Omit<RasterProps, 'cells'>, cell: (column: number, row: number) => { readonly glyph: string; readonly foreground?: number; readonly background?: number }): RasterProps => ({
    ...props,
    cells: new Uint8Array(Uint32Array.from(Array.makeBy(props.columns * props.rows, (index) => cell(index % props.columns, Math.trunc(index / props.columns))).flatMap(({ glyph, foreground = UNSET, background = UNSET }) => [glyph.codePointAt(0) ?? 0, foreground, background])).buffer).toBase64(),
});
const stand = (rows: SubscriptionRef.SubscriptionRef<Record.ReadonlyRecord<string, Row>>, id: string, requestId: string, change: (row: Row) => Row = Function.identity) =>
    SubscriptionRef.update(rows, (held) => {
        const kept = Option.filter(Record.get(held, id), (row) => row.requestId === requestId);
        return Record.set(held, id, change(Option.getOrElse(kept, (): Row => ({ requestId, onScreen: {}, fullscreen: false }))));
    });
const prune = (rows: SubscriptionRef.SubscriptionRef<Record.ReadonlyRecord<string, Row>>, e: Frozen<Args<'session.compact'>>, result: EventResult<'session.compact'>) =>
    Effect.when(SubscriptionRef.update(rows, Record.filter(Struct.get('fullscreen'))), Effect.succeed(e.trigger !== 'precompute' && e.agentId === undefined && result.skip === undefined));

// --- [COMPOSITION] ---------------------------------------------------------------------

function record($: EngineInterface, rows: SubscriptionRef.SubscriptionRef<Record.ReadonlyRecord<string, Row>>, e: Frozen<RenderInput<'Placed'>>) {
    const { id, plugin, onScreen } = e.props;
    stand(rows, id, e.requestId, (row) => ({
        ...row,
        fullscreen: e.surface === 'terminal' ? e.viewport?.isFullscreen === true : row.fullscreen,
        onScreen: onScreen === undefined ? Struct.omit(row.onScreen, [e.surface]) : { ...row.onScreen, [e.surface]: Option.fromNullOr(onScreen) },
    })).pipe(Effect.when(Effect.succeed(plugin === $.plugin.name)), Effect.runSync);
}

function makePlaced($: EngineInterface, rows: SubscriptionRef.SubscriptionRef<Record.ReadonlyRecord<string, Row>>) {
    return {
        place: (id: string) => call('ui.place', id, () => $.ui.place({ id })).pipe(Effect.tap(({ requestId }) => (requestId === undefined ? Effect.void : stand(rows, id, requestId)))),
        blit: (args: UiBlitArgs) => Effect.asVoid(call('ui.blit', args.key, () => $.ui.blit(args), Struct.get('deny'))),
        rows: SubscriptionRef.changes(rows),
    };
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { paint, Placed, makePlaced, prune, type Row, record };
