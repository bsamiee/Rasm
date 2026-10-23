import { Array, Option, Predicate, Record, Schema, Struct } from 'effect';
import { type Font, MetricsTag } from 'harfbuzzjs';
import type { Tables } from '#/tables';

// --- [MODELS] --------------------------------------------------------------------------

const Extents = Schema.Struct(Struct.Record(['ascender', 'descender', 'lineGap'], Schema.Finite));
const Stroke = Schema.Struct({ position: Schema.Finite.annotate({ description: 'Stroke center, negative below baseline' }), thickness: Schema.Finite });
class Metrics extends Schema.Class<Metrics, { readonly brand: unique symbol }>('Metrics')({
    extents: Schema.Struct({
        hhea: Extents.annotate({ description: 'Values at default instance, descender negative' }),
        typo: Schema.OptionFromOptionalKey(Extents.annotate({ description: 'OS/2 values' })),
        win: Schema.OptionFromOptionalKey(Extents.mapFields(Struct.omit(['lineGap'])).annotate({ description: 'OS/2 Windows clipping values, descender positive' })),
        vertical: Schema.OptionFromOptionalKey(Extents.annotate({ description: 'vhea values' })),
        useTypoMetrics: Schema.Boolean.annotate({ description: 'Layout prefers typo over hhea' }),
    }),
    ...Struct.Record(['xHeight', 'capHeight'], Schema.OptionFromOptionalKey(Schema.Finite.annotate({ description: 'OS/2 value' }))),
    underline: Schema.Struct({
        post: Schema.OptionFromOptionalKey(Stroke.annotate({ description: 'post table stroke' })),
        cff: Schema.OptionFromOptionalKey(Stroke.annotate({ description: 'CFF Top DICT stroke' })),
    }),
    strikeout: Schema.OptionFromOptionalKey(Stroke.annotate({ description: 'OS/2 stroke' })),
    bounds: Schema.Struct(Struct.Record(['minX', 'minY', 'maxX', 'maxY'], Schema.Finite)).annotate({ description: 'head table box at default instance' }),
    italicAngle: Schema.Finite.annotate({ description: 'Degrees counterclockwise from vertical, slnt axis value when present' }),
}) {
    static readonly of = ({ font, tables, axes }: { readonly font: Font; readonly tables: typeof Tables.Type; readonly axes: ReadonlyArray<{ readonly tag: string; readonly value: number }> }) => {
        const em = (units: number) => units / font.face.upem;
        const metric = (tag: MetricsTag) => Option.map(Option.fromUndefinedOr(font.getMetricPosition(tag)), em);
        const drawn = Option.filter(({ thickness }: typeof Stroke.Type) => thickness !== 0);
        const stroke = ([offset, width]: readonly [MetricsTag, MetricsTag]) => drawn(Option.map(Option.all({ position: metric(offset), thickness: metric(width) }), ({ position, thickness }) => ({ position: position - thickness / 2, thickness })));
        return Metrics.make({
            extents: {
                hhea: Record.map({ ascender: tables.hhea.ascent, descender: tables.hhea.descent, lineGap: tables.hhea.lineGap }, em),
                typo: tables['OS/2'].pipe(
                    Option.flatMap(({ typoAscender: ascender, typoDescender: descender, typoLineGap: lineGap }) => Option.all({ ascender, descender, lineGap })),
                    Option.map(({ ascender, descender, lineGap }) =>
                        Record.map({ ascender: ascender + font.getMetricVariation(MetricsTag.HORIZONTAL_ASCENDER), descender: descender + font.getMetricVariation(MetricsTag.HORIZONTAL_DESCENDER), lineGap: lineGap + font.getMetricVariation(MetricsTag.HORIZONTAL_LINE_GAP) }, em),
                    ),
                ),
                win: Option.all({ ascender: metric(MetricsTag.HORIZONTAL_CLIPPING_ASCENT), descender: metric(MetricsTag.HORIZONTAL_CLIPPING_DESCENT) }),
                vertical: Option.all({ ascender: metric(MetricsTag.VERTICAL_ASCENDER), descender: metric(MetricsTag.VERTICAL_DESCENDER), lineGap: metric(MetricsTag.VERTICAL_LINE_GAP) }),
                useTypoMetrics: Option.exists(tables['OS/2'], ({ fsSelection }) => fsSelection.useTypoMetrics),
            },
            ...Record.map({ xHeight: MetricsTag.X_HEIGHT, capHeight: MetricsTag.CAP_HEIGHT }, (tag) => Option.filter(metric(tag), Predicate.isTruthy)),
            underline: {
                post: stroke([MetricsTag.UNDERLINE_OFFSET, MetricsTag.UNDERLINE_SIZE]),
                cff: drawn(Option.map(tables['CFF '], ({ topDict: { UnderlinePosition, UnderlineThickness, FontMatrix } }) => ({ position: UnderlinePosition * FontMatrix[3], thickness: UnderlineThickness * FontMatrix[3] }))),
            },
            strikeout: stroke([MetricsTag.STRIKEOUT_OFFSET, MetricsTag.STRIKEOUT_SIZE]),
            bounds: Record.map({ minX: tables.head.xMin, minY: tables.head.yMin, maxX: tables.head.xMax, maxY: tables.head.yMax }, em),
            italicAngle: Option.match(
                Array.findFirst(axes, ({ tag }) => tag === 'slnt'),
                { onNone: () => tables.post.italicAngle, onSome: Struct.get('value') },
            ),
        });
    };
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Metrics };
