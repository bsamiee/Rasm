import { Array, Option, Predicate, Record, Schema, Struct } from 'effect';
import { type Font, MetricsTag } from 'harfbuzzjs';
import type { Tables } from './tables.ts';

// --- [MODELS] --------------------------------------------------------------------------

const Extents = Schema.Struct(Struct.Record(['ascender', 'descender', 'lineGap'], Schema.Finite));
const Stroke = Schema.Struct({ position: Schema.Finite.annotate({ description: 'Stroke centre, negative below the baseline' }), thickness: Schema.Finite });
class Metrics extends Schema.Class<Metrics, { readonly brand: unique symbol }>('Metrics')(
    {
        extents: Schema.Struct({
            hhea: Extents.annotate({ description: 'Horizontal header ascender, descender (negative), and line gap' }),
            typo: Schema.OptionFromOptionalKey(Extents.annotate({ description: 'OS/2 typographic ascender, descender, and line gap at this instance; absent when the table states none' })),
            win: Schema.OptionFromOptionalKey(Extents.mapFields(Struct.omit(['lineGap'])).annotate({ description: 'OS/2 Windows clipping ascent and descent at this instance, descent positive; absent when the table states none' })),
            vertical: Schema.OptionFromOptionalKey(Extents.annotate({ description: 'Vertical header ascender, descender, and line gap at this instance; absent when the face has no vhea table' })),
            useTypoMetrics: Schema.Boolean.annotate({ description: 'The face asks layout to take the typographic set over the header set' }),
        }).annotate({ description: 'Every ascent and descent the face states, in em' }),
        ...Record.map({ xHeight: 'OS/2 height of non-ascending lowercase letters at this instance', capHeight: 'OS/2 height of capital letters at this instance' }, (description) => Schema.OptionFromOptionalKey(Schema.Finite.annotate({ description: `${description}; absent when the table states none` }))),
        underline: Schema.Struct({
            post: Schema.OptionFromOptionalKey(Stroke.annotate({ description: 'The post table underline at this instance; absent when the face sets no stroke' })),
            cff: Schema.OptionFromOptionalKey(Stroke.annotate({ description: 'The CFF Top DICT underline of a CFF face; absent when the face is not CFF or sets no stroke' })),
        }).annotate({ description: 'Underline strokes the face states, in em' }),
        strikeout: Schema.OptionFromOptionalKey(Stroke.annotate({ description: 'OS/2 strikeout stroke at this instance; absent when the face sets no stroke' })),
        bounds: Schema.Struct(Struct.Record(['minX', 'minY', 'maxX', 'maxY'], Schema.Finite)).annotate({ description: 'Declared font-wide box at the default instance, in em' }),
        italicAngle: Schema.Finite.annotate({ description: 'Degrees counterclockwise from vertical, negative for a right lean; the slnt axis value at this instance when the face has one' }),
    },
    { description: 'Vertical metrics, strokes, bounds, and slant the face states at this instance' },
) {
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
