import type { Args, EngineInterface, EventResult, Frozen, OnScreen, RasterProps, RenderInput, RenderSurface, UiBlitArgs, UiPlaceResult } from 'claude-code';
import { Array, Context, Effect, Layer, Option, type PlatformError, Record, type Stream, Struct, SubscriptionRef } from 'effect';
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

class Placed extends Context.Service<
    Placed,
    {
        readonly place: (id: string) => Effect.Effect<UiPlaceResult, PlatformError.PlatformError>;
        readonly blit: (args: UiBlitArgs) => Effect.Effect<void, PlatformError.PlatformError>;
        readonly rows: Stream.Stream<Record.ReadonlyRecord<string, Row>>;
    }
>()('@rasm/claude-code/ui/placed') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const raster = (props: Omit<RasterProps, 'cells'>, cell: (column: number, row: number) => { readonly glyph: string; readonly foreground?: number; readonly background?: number }): RasterProps => ({
    ...props,
    cells: new Uint8Array(Uint32Array.from(Array.makeBy(props.columns * props.rows, (index) => cell(index % props.columns, Math.trunc(index / props.columns))).flatMap(({ glyph, foreground = UNSET, background = UNSET }) => [glyph.codePointAt(0) ?? 0, foreground, background])).buffer).toBase64(),
});
const prune = (rows: SubscriptionRef.SubscriptionRef<Record.ReadonlyRecord<string, Row>>, e: Frozen<Args<'session.compact'>>, result: EventResult<'session.compact'>) =>
    Effect.when(SubscriptionRef.update(rows, Record.filter(Struct.get('fullscreen'))), Effect.succeed(e.trigger !== 'precompute' && e.agentId === undefined && result.skip === undefined));

// --- [COMPOSITION] ---------------------------------------------------------------------

function record($: EngineInterface, rows: SubscriptionRef.SubscriptionRef<Record.ReadonlyRecord<string, Row>>, e: Frozen<RenderInput<'Placed'>>) {
    const { id, plugin, onScreen } = e.props;
    SubscriptionRef.update(rows, (held) => {
        const kept = Option.filter(Record.get(held, id), (row) => row.requestId === e.requestId);
        const shown = Option.match(kept, { onNone: (): Row['onScreen'] => ({}), onSome: Struct.get('onScreen') });
        return Record.set(held, id, {
            requestId: e.requestId,
            fullscreen: e.surface === 'terminal' ? e.viewport?.isFullscreen === true : Option.exists(kept, Struct.get('fullscreen')),
            onScreen: onScreen === undefined ? Struct.omit(shown, [e.surface]) : { ...shown, [e.surface]: Option.fromNullOr(onScreen) },
        });
    }).pipe(Effect.when(Effect.succeed(plugin === $.plugin.name)), Effect.runSync);
}

function placed($: EngineInterface, rows: SubscriptionRef.SubscriptionRef<Record.ReadonlyRecord<string, Row>>) {
    return Layer.succeed(Placed)({
        place: (id) =>
            call('ui.place', id, () => $.ui.place({ id })).pipe(
                Effect.tap(({ requestId }) => (requestId === undefined ? Effect.void : SubscriptionRef.update(rows, (held) => Record.union(held, { [id]: { requestId, onScreen: {}, fullscreen: false } }, (kept, made) => (kept.requestId === made.requestId ? kept : made))))),
            ),
        blit: (args) => Effect.asVoid(call('ui.blit', args.key, () => $.ui.blit(args), Struct.get('deny'))),
        rows: SubscriptionRef.changes(rows),
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { prune, Placed, placed, raster, type Row, record };
