import { type Option, Record, Schema, Struct } from 'effect';
import { ColorPaletteFlags, type Face } from 'harfbuzzjs';

// --- [MODELS] --------------------------------------------------------------------------

class Color extends Schema.Class<Color, { readonly brand: unique symbol }>('Color')(
    {
        palettes: Schema.Array(
            Schema.Struct({ name: Schema.OptionFromOptionalKey(Schema.String), colors: Schema.Array(Schema.Struct({ ...Struct.Record(['red', 'green', 'blue', 'alpha'], Schema.Int), name: Schema.OptionFromOptionalKey(Schema.String) })), ...Struct.Record(['light', 'dark'], Schema.Boolean) }).annotate({
                description: 'CPAL palette of unpremultiplied 8-bit colors, and whether it suits a light or a dark background',
            }),
        ),
        ...Struct.Record(['layers', 'paint', 'png'], Schema.Boolean),
    },
    { description: 'Color glyph data: CPAL palettes, COLRv0 layers, COLRv1 paint, and CBDT or sbix PNG images' },
) {
    static readonly of = ({ handle, named }: { readonly handle: Face; readonly named: (id: number | undefined) => Option.Option<string> }) =>
        Color.make({
            palettes: handle.getColorPalettes().map(({ colors, nameId, flags }) => ({
                name: named(nameId),
                colors: colors.map(({ nameId: label, ...channels }) => ({ ...channels, name: named(label) })),
                ...Record.map({ light: ColorPaletteFlags.USABLE_WITH_LIGHT_BACKGROUND, dark: ColorPaletteFlags.USABLE_WITH_DARK_BACKGROUND }, (flag) => (flags & flag) !== 0),
            })),
            layers: handle.hasColorLayers(),
            paint: handle.hasColorPaint(),
            png: handle.hasColorPng(),
        });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Color };
