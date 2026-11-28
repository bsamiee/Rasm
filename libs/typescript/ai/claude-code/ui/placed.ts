import type { EngineInterface, Frozen, RasterProps, RenderInput, UiBlitArgs } from 'claude-code';
import { Array, Context, Effect, Layer, Option, type PlatformError, Record, type Stream, Struct, SubscriptionRef } from 'effect';
import { call } from '#/engine/services';

// --- [TYPES] ---------------------------------------------------------------------------

interface Cell {
    readonly glyph: string;
    readonly foreground?: number;
    readonly background?: number;
}
interface Row {
    readonly requestId: string;
    readonly onScreen: Record.ReadonlyRecord<string, Option.Option<NonNullable<Frozen<RenderInput<'Placed'>>['props']['onScreen']>>>;
    readonly fullscreen: boolean;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const unset = 0x01_00_00_00;

// --- [SERVICES] ------------------------------------------------------------------------

class Placed extends Context.Service<
    Placed,
    {
        readonly place: (id: string) => Effect.Effect<Awaited<ReturnType<EngineInterface['ui']['place']>>, PlatformError.PlatformError>;
        readonly blit: (args: UiBlitArgs) => Effect.Effect<void, PlatformError.PlatformError>;
        readonly rows: Stream.Stream<Record.ReadonlyRecord<string, Row>>;
    }
>()('@rasm/claude-code/ui/placed') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const raster = ({ columns, rows }: Pick<RasterProps, 'columns' | 'rows'>, cell: (column: number, row: number) => Cell): Pick<RasterProps, 'columns' | 'rows' | 'cells'> => ({
    columns,
    rows,
    cells: new Uint8Array(Uint32Array.from(Array.makeBy(columns * rows, (index) => cell(index % columns, Math.trunc(index / columns))).flatMap(({ glyph, foreground, background }) => [glyph.codePointAt(0) ?? 0, foreground ?? unset, background ?? unset])).buffer).toBase64(),
});
const compacted = (rows: SubscriptionRef.SubscriptionRef<Record.ReadonlyRecord<string, Row>>) =>
    SubscriptionRef.update(
        rows,
        Record.filter(({ fullscreen }) => fullscreen),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

function seen($: EngineInterface, rows: SubscriptionRef.SubscriptionRef<Record.ReadonlyRecord<string, Row>>, e: Frozen<RenderInput<'Placed'>>) {
    const { id, plugin, onScreen } = e.props;
    Effect.runSync(
        Effect.when(
            SubscriptionRef.update(rows, (held) => {
                const row = Option.filter(Record.get(held, id), (kept) => kept.requestId === e.requestId);
                const shown = Option.match(row, { onNone: () => ({}), onSome: Struct.get('onScreen') });
                return Record.set(held, id, {
                    requestId: e.requestId,
                    fullscreen: e.surface === 'terminal' ? e.viewport?.isFullscreen === true : Option.exists(row, Struct.get('fullscreen')),
                    onScreen: onScreen === undefined ? Record.remove(shown, e.surface) : Record.set(shown, e.surface, Option.fromNullOr(onScreen)),
                });
            }),
            Effect.succeed(plugin === $.plugin.name),
        ),
    );
}

function placed($: EngineInterface, rows: SubscriptionRef.SubscriptionRef<Record.ReadonlyRecord<string, Row>>) {
    return Layer.succeed(Placed)({
        place: (id) =>
            call('ui.place', id, () => $.ui.place({ id })).pipe(
                Effect.tap((answer) =>
                    Option.match(Option.fromUndefinedOr(answer.requestId), {
                        onNone: () => Effect.void,
                        onSome: (requestId) => SubscriptionRef.update(rows, (held) => (Option.exists(Record.get(held, id), (row) => row.requestId === requestId) ? held : Record.set(held, id, { requestId, onScreen: {}, fullscreen: false }))),
                    }),
                ),
            ),
        blit: (args) =>
            Effect.asVoid(
                call(
                    'ui.blit',
                    args.key,
                    () => $.ui.blit(args),
                    ({ deny }) => deny,
                ),
            ),
        rows: SubscriptionRef.changes(rows),
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { compacted, Placed, placed, raster, type Row, seen };
