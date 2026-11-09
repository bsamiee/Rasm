import { Array, Option, Predicate, Record, Schema, Struct } from 'effect';
import { type Font, MetricsTag } from 'harfbuzzjs';
import type { Tables } from '#/shaping/internal/tables';
import type { ShapedText } from '#/shaping/shaped-text';

// --- [MODELS] --------------------------------------------------------------------------

const Extents = Schema.Struct(Struct.Record(['ascender', 'descender', 'lineGap'], Schema.Finite));
const Stroke = Schema.Struct({ position: Schema.Finite.annotate({ description: 'Stroke center, negative below baseline' }), thickness: Schema.Finite });
const Shift = Schema.Struct(Struct.Record(['size', 'offset'], Schema.Struct(Struct.Record(['x', 'y'], Schema.Finite))));
class Metrics extends Schema.Class<Metrics, { readonly brand: unique symbol }>('Metrics')({
    extents: Schema.Struct({
        horizontal: Extents.annotate({ description: 'At the instance, OS/2 typo values under fsSelection USE_TYPO_METRICS else hhea, descender negative' }),
        win: Schema.OptionFromOptionalKey(Extents.mapFields(Struct.omit(['lineGap'])).annotate({ description: 'OS/2 Windows clipping values, descender positive' })),
        vertical: Schema.OptionFromOptionalKey(Extents.annotate({ description: 'vhea values' })),
    }),
    ...Struct.Record(['xHeight', 'capHeight'], Schema.OptionFromOptionalKey(Schema.Finite.annotate({ description: 'OS/2 value' }))),
    underline: Schema.OptionFromOptionalKey(Stroke.annotate({ description: 'post table stroke, else CFF Top DICT stroke' })),
    strikeout: Schema.OptionFromOptionalKey(Stroke.annotate({ description: 'OS/2 stroke' })),
    superscript: Schema.OptionFromOptionalKey(Shift.annotate({ description: 'OS/2 superscript em size and offset from the baseline, y up' })),
    subscript: Schema.OptionFromOptionalKey(Shift.annotate({ description: 'OS/2 subscript em size and offset from the baseline, y down' })),
    bounds: Schema.Struct(Struct.Record(['minX', 'minY', 'maxX', 'maxY'], Schema.Finite)).annotate({ description: 'head table box at default instance' }),
    italicAngle: Schema.Finite.annotate({ description: 'Degrees counterclockwise from vertical, slnt axis value when present' }),
}) {
    static readonly of = ({ font, tables, axes }: { readonly font: Font; readonly tables: typeof Tables.Type; readonly axes: ReadonlyArray<{ readonly tag: string; readonly value: number }> }) => {
        const em = (units: number) => units / font.face.upem;
        const metric = (tag: MetricsTag) => Option.map(Option.fromUndefinedOr(font.getMetricPosition(tag)), em);
        const drawn = Option.filter(({ thickness }: typeof Stroke.Type) => thickness !== 0);
        const stroke = ([offset, width]: readonly [MetricsTag, MetricsTag]) => drawn(Option.map(Option.all({ position: metric(offset), thickness: metric(width) }), ({ position, thickness }) => ({ position: position - thickness / 2, thickness })));
        const shift = (position: 'SUPERSCRIPT' | 'SUBSCRIPT') => Option.all(Record.map({ size: 'SIZE', offset: 'OFFSET' } as const, (part) => Option.all(Record.map({ x: 'X', y: 'Y' } as const, (axis) => metric(MetricsTag[`${position}_EM_${axis}_${part}`])))));
        return Metrics.make({
            extents: {
                horizontal: Record.map(font.hExtents(), em),
                win: Option.all({ ascender: metric(MetricsTag.HORIZONTAL_CLIPPING_ASCENT), descender: metric(MetricsTag.HORIZONTAL_CLIPPING_DESCENT) }),
                vertical: Option.all({ ascender: metric(MetricsTag.VERTICAL_ASCENDER), descender: metric(MetricsTag.VERTICAL_DESCENDER), lineGap: metric(MetricsTag.VERTICAL_LINE_GAP) }),
            },
            ...Record.map({ xHeight: MetricsTag.X_HEIGHT, capHeight: MetricsTag.CAP_HEIGHT }, (tag) => Option.filter(metric(tag), Predicate.isTruthy)),
            underline: Option.orElse(stroke([MetricsTag.UNDERLINE_OFFSET, MetricsTag.UNDERLINE_SIZE]), () => drawn(Option.map(tables['CFF '], ({ topDict: { UnderlinePosition, UnderlineThickness, FontMatrix } }) => ({ position: UnderlinePosition * FontMatrix[3], thickness: UnderlineThickness * FontMatrix[3] })))),
            strikeout: stroke([MetricsTag.STRIKEOUT_OFFSET, MetricsTag.STRIKEOUT_SIZE]),
            superscript: shift('SUPERSCRIPT'),
            subscript: shift('SUBSCRIPT'),
            bounds: Record.map(Struct.renameKeys(tables.head, { xMin: 'minX', yMin: 'minY', xMax: 'maxX', yMax: 'maxY' }), em),
            italicAngle: Option.match(
                Array.findFirst(axes, ({ tag }) => tag === 'slnt'),
                { onNone: () => tables.post.italicAngle, onSome: Struct.get('value') },
            ),
        });
    };

    underlined({ extents }: Pick<ShapedText, 'extents'>) {
        return Option.map(this.underline, (stroke) => Option.match(extents, { onNone: () => stroke, onSome: ({ minY }) => ({ ...stroke, position: Math.min(stroke.position, minY - stroke.thickness - stroke.thickness / 2) }) }));
    }
    scripted(position: 'superscript' | 'subscript', size: number) {
        return Option.map(this[position], ({ size: scale, offset }) => ({ size: size * scale.y, shift: size * offset.y * (position === 'superscript' ? 1 : -1) }));
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Metrics };
