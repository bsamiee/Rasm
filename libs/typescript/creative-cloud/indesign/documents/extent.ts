import type { Page, PageItem } from 'adobe:indesign';
import { Array, Cause, Effect, Number, Schema, Struct } from 'effect';
import { Model } from '../dom/model.ts';

// --- [MODELS] --------------------------------------------------------------------------

class Extent extends Schema.Class<Extent, { readonly brand: unique symbol }>('Extent')(Struct.Record(['top', 'left', 'bottom', 'right'], Schema.Finite), {
    description: 'Outer stroke box in spread points, x 0 at the spine of a facing-pages spread and at the center of any other spread',
}) {
    static readonly of = Effect.fn('Extent.of')(function* (...placed: readonly (Page | PageItem)[]) {
        const { AnchorPoint, BoundingBoxLimits, CoordinateSpaces } = yield* Model;
        return yield* Effect.try({
            try: () => {
                const corners: ReadonlyArray<readonly [number, number]> = Array.flatMap(Array.cartesian(placed, [AnchorPoint.TOP_LEFT_ANCHOR, AnchorPoint.BOTTOM_RIGHT_ANCHOR]), ([each, anchor]) =>
                    each.resolve([anchor, BoundingBoxLimits.OUTER_STROKE_BOUNDS, CoordinateSpaces.SPREAD_COORDINATES], CoordinateSpaces.SPREAD_COORDINATES),
                );
                return Extent.make(Struct.makeReducer<typeof Extent.Encoded>({ top: Number.ReducerMin, left: Number.ReducerMin, bottom: Number.ReducerMax, right: Number.ReducerMax }).combineAll(Array.map(corners, ([x, y]) => ({ top: y, left: x, bottom: y, right: x }))));
            },
            catch: (cause) => new Cause.UnknownError(cause, `extent of ${Array.map(placed, (each) => each.toSpecifier()).join(', ')}`),
        });
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Extent };
