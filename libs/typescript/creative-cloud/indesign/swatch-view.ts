import type { Gradient, MixedInk, MixedInkGroup, PlainColor, PlainSwatch, Tint } from 'adobe:indesign';
import { Array, Effect, flow, Match, Option, Schema, Struct } from 'effect';
import { read } from '#/dom/encoding';
import type { Model } from '#/dom/model';
import { Reference } from '#/dom/reference';

// --- [MODELS] --------------------------------------------------------------------------

class SwatchView extends Schema.TaggedClass<SwatchView, { readonly brand: unique symbol }>()('Swatch', {
    ...Reference.fields,
    group: Schema.OptionFromOptionalKey(Schema.String).annotate({ description: 'Color group name' }),
    value: Schema.TaggedUnion({
        Swatch: {},
        Color: { model: Schema.String, space: Schema.String, colorValue: Schema.Array(Schema.Finite) },
        Tint: { baseColor: Reference, tintValue: Schema.Finite },
        Gradient: { type: Schema.String, gradientStops: Schema.Array(Schema.Struct({ stopColor: Reference, location: Schema.Finite, midpoint: Schema.Finite })) },
        MixedInk: { baseColor: Reference, inkPercentages: Schema.Array(Schema.Finite) },
    }).annotate({ description: 'Empty Swatch for None and mixed ink groups' }),
}) {
    static readonly viewed = ({ owning }: Model['Service']) => flow(Option.liftPredicate(owning<PlainSwatch | PlainColor | Tint | Gradient | MixedInk | MixedInkGroup>('parentColorGroup')), Option.map(SwatchView.of));
    static readonly of = Effect.fn('SwatchView.of')(function* (swatch: PlainSwatch | PlainColor | Tint | Gradient | MixedInk | MixedInkGroup) {
        const value = Match.value(swatch).pipe(
            Match.when({ constructorName: 'Color' }, ({ model, space, colorValue }) => ({ _tag: 'Color', model, space, colorValue })),
            Match.when({ constructorName: 'Tint' }, ({ baseColor, tintValue }) => ({ _tag: 'Tint', baseColor, tintValue })),
            Match.when({ constructorName: 'Gradient' }, ({ type, gradientStops }) => ({ _tag: 'Gradient', type, gradientStops: Array.map(gradientStops.everyItem().getElements(), ({ stopColor, location, midpoint }) => ({ stopColor, location, midpoint })) })),
            Match.when({ constructorName: 'MixedInk' }, ({ baseColor, inkPercentages }) => ({ _tag: 'MixedInk', baseColor, inkPercentages })),
            Match.orElse(() => ({ _tag: 'Swatch' })),
        );
        return yield* read(SwatchView, swatch.toSpecifier(), () => ({ _tag: 'Swatch', ...Reference.of(swatch), group: Option.map(Option.fromNullOr(swatch.parentColorGroup), Struct.get('name')), value }));
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { SwatchView };
