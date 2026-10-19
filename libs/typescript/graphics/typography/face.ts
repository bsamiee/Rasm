import { Buffer } from 'node:buffer';
import { single, Unmatched } from '@rasm/search';
import { Array, Cause, Effect, Number, Option, Order, Predicate, Record, Schema, SchemaGetter, SchemaIssue, Struct } from 'effect';
import { create } from 'fontkit';
import { AxisFlags, Blob, Feature, Font, Face as Handle, MetricsTag, Buffer as Segment, shape, Variation } from 'harfbuzzjs';

// --- [TYPES] ---------------------------------------------------------------------------

type Named = (typeof Tables.Type)['fvar']['instance'][number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const IGNORED = /[\p{Cc}\p{Default_Ignorable_Code_Point}]/u;

// --- [MODELS] --------------------------------------------------------------------------

const Instance = Schema.Struct({
    coordinates: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.Finite).annotate({ description: "Axis values in the face's axis order, clamped to each axis; omitted trailing axes take their defaults, absent coordinates take the named instance the PostScript name selects, else the defaults" })),
    size: Schema.OptionFromOptionalKey(Schema.Finite.check(Schema.isGreaterThan(0)).annotate({ description: 'Point size; an opsz axis takes it, clamped to its range' })),
});
const Run = Schema.Struct({
    text: Schema.String.annotate({ description: 'One script and direction run to shape and check for glyphs; omitted text returns no glyph geometry' }).pipe(Schema.withDecodingDefaultKey(Effect.succeed(''))),
    features: Schema.Array(
        Schema.String.annotate({ description: 'OpenType feature setting in HarfBuzz syntax: tag, +tag, -tag, tag=value, or tag[start:end] over character positions' }).pipe(
            Schema.decodeTo(Schema.instanceOf(Feature), {
                decode: SchemaGetter.transformEffect((given: string) => Effect.mapError(Effect.fromNullishOr(Feature.fromString(given)), () => new SchemaIssue.InvalidValue({ message: `Not a feature setting: ${given}` }, given))),
                encode: SchemaGetter.transform((setting) => setting.toString()),
            }),
        ),
    ).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
});
const Shaped = Schema.Struct({
    glyphs: Schema.Array(
        Schema.Struct({
            glyph: Schema.Int.annotate({ description: 'Glyph ID in the face' }),
            cluster: Schema.Int.annotate({ description: 'UTF-16 offset in the input text; a cluster may contain several characters or glyphs' }),
            ...Struct.Record(['xAdvance', 'yAdvance', 'xOffset', 'yOffset'], Schema.Finite),
            extents: Schema.OptionFromNullOr(Schema.Struct(Struct.Record(['xBearing', 'yBearing', 'width', 'height'], Schema.Finite))).pipe(Schema.annotateEncoded({ description: 'Glyph bounds from its origin; positive y is up, height normally negative. Top is yOffset + yBearing above the pen baseline' })),
        }),
    ).annotate({ description: 'Shaped text in visual glyph order with default and requested features at this instance, positions in em; not application-composed text or document coordinates' }),
    missing: Schema.Array(Schema.String).annotate({ description: 'Characters of the text the face maps to no glyph, excluding controls and Unicode default-ignorable characters' }),
});
const Extents = Schema.Struct({ ascender: Schema.Finite, descender: Schema.Finite, lineGap: Schema.Finite });
const Stroke = Schema.Struct({ position: Schema.Finite.annotate({ description: 'Stroke centre, negative below the baseline' }), thickness: Schema.Finite });
const Tables = Schema.Struct({
    post: Schema.Struct({ isFixedPitch: Schema.Finite }),
    'OS/2': Schema.OptionFromOptionalKey(Schema.Struct({ ...Struct.Record(['typoAscender', 'typoDescender', 'typoLineGap'], Schema.optional(Schema.Finite)), fsSelection: Schema.Struct({ useTypoMetrics: Schema.Boolean }) })),
    'CFF ': Schema.OptionFromOptionalKey(Schema.Struct({ topDict: Schema.Struct({ UnderlinePosition: Schema.Finite, UnderlineThickness: Schema.Finite, FontMatrix: Schema.Tuple([Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite]) }) })),
    fvar: Schema.Struct({ instance: Schema.Array(Schema.Struct({ nameID: Schema.Int, coord: Schema.Array(Schema.Finite), postscriptNameID: Schema.optional(Schema.Int) })) }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({ instance: [] }))),
});
const HANDLES = new WeakMap<Face, { readonly handle: Handle; readonly shaper: Font }>();
class Face extends Schema.Class<Face, { readonly brand: unique symbol }>('Face')({
    ...Record.map({ postscriptName: 'PostScript name of the face (name ID 6)', fullName: 'Full name (name ID 4)', family: 'Typographic family (name ID 16), else the family (name ID 1)', style: 'Typographic subfamily (name ID 17), else the subfamily (name ID 2)' }, (description) =>
        Schema.OptionFromOptionalKey(Schema.String.annotate({ description: `${description}; absent when the name table states none` })),
    ),
    coverage: Schema.Int.annotate({ description: 'Unicode code points the face maps to glyphs' }),
    extents: Schema.Struct({
        hhea: Extents.annotate({ description: 'Horizontal header ascender, descender (negative), and line gap' }),
        typo: Schema.OptionFromNullOr(Extents).pipe(Schema.annotateEncoded({ description: 'OS/2 typographic ascender, descender, and line gap at this instance; null when the table states none' })),
        win: Schema.OptionFromNullOr(Schema.Struct({ ascender: Schema.Finite, descender: Schema.Finite })).pipe(Schema.annotateEncoded({ description: 'OS/2 Windows clipping ascent and descent at this instance, descent positive; null when the table states none' })),
        useTypoMetrics: Schema.Boolean.annotate({ description: 'The face asks layout to take the typographic set over the header set' }),
    }).annotate({ description: 'Every ascent and descent the face states, in em' }),
    ...Record.map({ xHeight: 'OS/2 height of non-ascending lowercase letters at this instance; null when the table states none', capHeight: 'OS/2 height of capital letters at this instance; null when the table states none' }, (description) =>
        Schema.OptionFromNullOr(Schema.Finite).pipe(Schema.annotateEncoded({ description })),
    ),
    underline: Schema.Struct({
        post: Schema.OptionFromNullOr(Stroke).pipe(Schema.annotateEncoded({ description: 'The post table underline at this instance; null when the face sets no stroke' })),
        cff: Schema.OptionFromNullOr(Stroke).pipe(Schema.annotateEncoded({ description: 'The CFF Top DICT underline of a CFF face; null when the face is not CFF or sets no stroke' })),
    }).annotate({ description: 'Underline strokes the face states, in em' }),
    strikeout: Schema.OptionFromNullOr(Stroke).pipe(Schema.annotateEncoded({ description: 'OS/2 strikeout stroke at this instance; null when the face sets no stroke' })),
    bounds: Schema.Struct(Struct.Record(['minX', 'minY', 'maxX', 'maxY'], Schema.Finite)).annotate({ description: 'Declared font-wide box at the default instance, in em' }),
    italicAngle: Schema.Finite.annotate({ description: 'Degrees counterclockwise from vertical, negative for a right lean; the slnt axis value at this instance when the face has one' }),
    monospace: Schema.Boolean.annotate({ description: 'The face declares fixed-width spacing' }),
    features: Schema.Array(Schema.Struct({ tag: Schema.String, name: Schema.OptionFromOptionalKey(Schema.String) })).annotate({ description: 'GSUB and GPOS feature tags across all scripts, plus mapped AAT tags and kern table kerning; names identify stylistic sets and character variants' }),
    axes: Schema.Array(Schema.Struct({ tag: Schema.String, name: Schema.OptionFromOptionalKey(Schema.String), ...Struct.Record(['min', 'default', 'max', 'value'], Schema.Finite), hidden: Schema.Boolean })).annotate({
        description: 'Variation axes in fvar order with the value this instance applies; hidden axes are not meant for user interfaces',
    }),
    instances: Schema.Array(Schema.Struct({ ...Record.map({ name: 'Subfamily name of the instance', postscriptName: 'PostScript name of the instance' }, (description) => Schema.OptionFromOptionalKey(Schema.String.annotate({ description }))), coordinates: Schema.Array(Schema.Finite) })).annotate({
        description: 'Named instances with their values in axes order',
    }),
}) {
    static readonly open = Effect.fn('open')(function* (data: Uint8Array, postscriptName: Option.Option<string>, { coordinates, size }: typeof Instance.Type) {
        const container = yield* Effect.try(() => create(Buffer.from(data.buffer, data.byteOffset, data.byteLength))).pipe(
            Effect.filterOrFail(
                ({ type }) => type !== 'WOFF' && type !== 'WOFF2',
                ({ type }) => new Cause.IllegalArgumentError(`${type} webfont containers must be decoded to OpenType or TrueType font bytes`),
            ),
        );
        const blob = new Blob(data);
        const label = (named: Handle, id: number | undefined) => Option.flatMap(Option.fromUndefinedOr(id), (each) => Option.liftPredicate(named.getName(each, 'en'), Predicate.isTruthy));
        const faces = yield* Effect.forEach('fonts' in container ? container.fonts : [container], (each, index) => Effect.map(Schema.decodeUnknownEffect(Tables)(each), (decoded) => ({ font: each, tables: decoded, handle: new Handle(blob, index) })));
        const openable = Array.getSomes(
            faces
                .flatMap((row) => [{ ...row, instance: Option.none<Named>(), name: Option.fromNullOr(row.font.postscriptName) }, ...row.tables.fvar.instance.map((each) => ({ ...row, instance: Option.some(each), name: label(row.handle, each.postscriptNameID) }))])
                .map(({ name, ...candidate }) => Option.map(name, (named) => ({ ...candidate, name: named }))),
        );
        const { font, tables, handle, instance } = yield* Option.match(postscriptName, {
            onNone: () =>
                Effect.map(
                    Effect.fromOption(faces.length === 1 ? Array.head(faces) : Option.none(), () => Unmatched.make({ subject: 'single face in the collection', query: '', candidates: openable.map(Struct.get('name')) })),
                    (only) => ({ ...only, instance: Option.none<Named>() }),
                ),
            onSome: (name) => single('face or named instance', name)(openable, { names: (each) => [each.name], label: Struct.get('name') }),
        });
        const infos = Array.sortWith(Object.values(handle.getAxisInfos()), Struct.get('axisIndex'), Order.Number);
        const chosen = yield* Effect.filterOrFail(
            Effect.succeed(
                Option.getOrElse(
                    Option.orElse(coordinates, () => Option.map(instance, Struct.get('coord'))),
                    Array.empty<number>,
                ),
            ),
            (values) => values.length <= infos.length,
            (values) => new Cause.IllegalArgumentError(`The face has ${infos.length} variation axes, ${values.length} coordinates given`),
        );
        const axes = infos.map(({ tag, nameId, flags, min, default: fallback, max }, at) => ({
            tag,
            name: label(handle, nameId),
            min,
            default: fallback,
            max,
            value: Number.clamp({ minimum: min, maximum: max })(Option.getOrElse(tag === 'opsz' ? Option.orElse(size, () => Array.get(chosen, at)) : Array.get(chosen, at), () => fallback)),
            hidden: (flags & AxisFlags.HIDDEN) !== 0,
        }));
        const shaper = new Font(handle);
        shaper.setVariations(axes.map(({ tag, value }) => new Variation(tag, value)));
        const em = (units: number) => units / handle.upem;
        const metric = (tag: MetricsTag) => Option.fromUndefinedOr(shaper.getMetricPosition(tag));
        const drawn = Option.filter(({ thickness }: typeof Stroke.Type) => thickness !== 0);
        const stroke = ([offset, thickness]: readonly [MetricsTag, MetricsTag]) => drawn(Option.map(Option.all({ position: metric(offset), thickness: metric(thickness) }), ({ position, thickness: width }) => Record.map({ position: position - width / 2, thickness: width }, em)));
        const face = Face.make({
            postscriptName: Option.fromNullOr(font.postscriptName),
            fullName: Option.fromNullOr(font.fullName),
            family: Option.orElse(Option.fromNullOr(font.getName('preferredFamily', 'en')), () => Option.fromNullOr(font.familyName)),
            style: Option.orElse(Option.fromNullOr(font.getName('preferredSubfamily', 'en')), () => Option.fromNullOr(font.subfamilyName)),
            coverage: handle.collectUnicodes().length,
            extents: {
                hhea: Record.map({ ascender: font.ascent, descender: font.descent, lineGap: font.lineGap }, em),
                typo: Option.map(
                    Option.flatMap(tables['OS/2'], ({ typoAscender, typoDescender, typoLineGap }) => Option.all({ ascender: Option.fromUndefinedOr(typoAscender), descender: Option.fromUndefinedOr(typoDescender), lineGap: Option.fromUndefinedOr(typoLineGap) })),
                    ({ ascender, descender, lineGap }) => Record.map({ ascender: ascender + shaper.getMetricVariation(MetricsTag.HORIZONTAL_ASCENDER), descender: descender + shaper.getMetricVariation(MetricsTag.HORIZONTAL_DESCENDER), lineGap: lineGap + shaper.getMetricVariation(MetricsTag.HORIZONTAL_LINE_GAP) }, em),
                ),
                win: Option.map(Option.all({ ascender: metric(MetricsTag.HORIZONTAL_CLIPPING_ASCENT), descender: metric(MetricsTag.HORIZONTAL_CLIPPING_DESCENT) }), Record.map(em)),
                useTypoMetrics: Option.exists(tables['OS/2'], ({ fsSelection }) => fsSelection.useTypoMetrics),
            },
            ...Record.map({ xHeight: MetricsTag.X_HEIGHT, capHeight: MetricsTag.CAP_HEIGHT }, (tag) => Option.map(Option.filter(metric(tag), Predicate.isTruthy), em)),
            underline: {
                post: stroke([MetricsTag.UNDERLINE_OFFSET, MetricsTag.UNDERLINE_SIZE]),
                cff: drawn(Option.map(tables['CFF '], ({ topDict: { UnderlinePosition, UnderlineThickness, FontMatrix } }) => ({ position: UnderlinePosition * FontMatrix[3], thickness: UnderlineThickness * FontMatrix[3] }))),
            },
            strikeout: stroke([MetricsTag.STRIKEOUT_OFFSET, MetricsTag.STRIKEOUT_SIZE]),
            bounds: Record.map(Struct.pick(font.bbox, ['minX', 'minY', 'maxX', 'maxY']), em),
            italicAngle: Option.match(
                Array.findFirst(axes, ({ tag }) => tag === 'slnt'),
                { onNone: () => font.italicAngle, onSome: Struct.get('value') },
            ),
            monospace: tables.post.isFixedPitch !== 0,
            features: Array.dedupeWith(
                [...(['GSUB', 'GPOS'] as const).flatMap((table) => handle.getTableFeatureTags(table).map((tag, at) => ({ tag, name: label(handle, handle.getFeatureNameIds(table, at)?.uiLabelNameId) }))), ...font.availableFeatures.map((tag) => ({ tag, name: Option.none<string>() }))],
                (first, second) => first.tag === second.tag,
            ),
            axes,
            instances: tables.fvar.instance.map(({ nameID, coord, postscriptNameID }) => ({ name: label(handle, nameID), postscriptName: label(handle, postscriptNameID), coordinates: coord })),
        });
        HANDLES.set(face, { handle, shaper });
        return face;
    });

    shape({ text, features }: typeof Run.Type): Effect.Effect<typeof Shaped.Type, Cause.NoSuchElementError> {
        return Effect.map(Effect.fromNullishOr(HANDLES.get(this)), ({ handle, shaper }) => {
            const em = (units: number) => units / handle.upem;
            const segment = new Segment();
            segment.addText(text);
            segment.guessSegmentProperties();
            const characters = segment.getGlyphInfos().map(Struct.get('codepoint'));
            shape(shaper, segment, [...features]);
            return {
                glyphs: Array.zipWith(segment.getGlyphInfos(), segment.getGlyphPositions(), ({ codepoint, cluster }, position) => ({ glyph: codepoint, cluster, ...Record.map(position, em), extents: Option.map(Option.fromUndefinedOr(shaper.glyphExtents(codepoint)), Record.map(em)) })),
                missing: Array.dedupe(characters.filter((character) => Predicate.isUndefined(shaper.nominalGlyph(character))))
                    .map((character) => String.fromCodePoint(character))
                    .filter((character) => !IGNORED.test(character)),
            };
        });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Face, Instance, Run, Shaped };
