import { Record, Schema, Struct } from 'effect';
import { ColorPaletteFlags, type Face } from 'harfbuzzjs';
import type { Names } from '#/names';

// --- [MODELS] --------------------------------------------------------------------------

class Color extends Schema.Class<Color, { readonly brand: unique symbol }>('Color')(
    {
        palettes: Schema.Array(
            Schema.Struct({ name: Schema.OptionFromOptionalKey(Schema.String), colors: Schema.Array(Schema.Struct({ ...Struct.Record(['red', 'green', 'blue', 'alpha'], Schema.Int), name: Schema.OptionFromOptionalKey(Schema.String) })), ...Struct.Record(['light', 'dark'], Schema.Boolean) }).annotate({
                description: 'Unpremultiplied 8-bit colors with light and dark background flags',
            }),
        ),
        ...Struct.Record(['layers', 'paint', 'png'], Schema.Boolean),
    },
    { description: 'CPAL palettes, COLRv0 layers, COLRv1 paint, CBDT or sbix PNG images' },
) {
    static readonly of = ({ handle, names }: { readonly handle: Face; readonly names: Names }) =>
        Color.make({
            palettes: handle.getColorPalettes().map(({ colors, nameId, flags }) => ({
                name: names.named(nameId),
                colors: colors.map(({ nameId: label, ...channels }) => ({ ...channels, name: names.named(label) })),
                ...Record.map({ light: ColorPaletteFlags.USABLE_WITH_LIGHT_BACKGROUND, dark: ColorPaletteFlags.USABLE_WITH_DARK_BACKGROUND }, (flag) => (flags & flag) !== 0),
            })),
            layers: handle.hasColorLayers(),
            paint: handle.hasColorPaint(),
            png: handle.hasColorPng(),
        });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Color };
