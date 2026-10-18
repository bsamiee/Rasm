import { Buffer } from 'node:buffer';
import { Array, Cause, Effect, flow, Option, Predicate, Record, Schema, Struct } from 'effect';
import { create } from 'fontkit';
import { Blob, Face, Buffer as HarfBuzzBuffer, Font as HarfBuzzFont, MetricsTag, shape, Variation } from 'harfbuzzjs';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CHARACTERS = { xHeight: 'x', capHeight: 'H' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Font = Schema.Struct({
    ...Record.map({ ascender: 'Horizontal ascent above the baseline', descender: 'Horizontal descent below the baseline, negative', xHeight: 'Height of non-ascending lowercase letters', capHeight: 'Height of capital letters' }, (description) =>
        Schema.OptionFromNullOr(Schema.Finite).pipe(Schema.annotateEncoded({ description })),
    ),
    lineGap: Schema.Finite.annotate({ description: 'Designed gap between lines' }),
    bounds: Schema.Struct(Struct.Record(['minX', 'minY', 'maxX', 'maxY'], Schema.Finite)).annotate({ description: 'Declared font-wide box at the default instance' }),
    ...Record.map({ underline: 'Underline stroke centre, negative below the baseline, and thickness; null when the face sets none', strikeout: 'Strikeout stroke centre and thickness; null when the face sets none' }, (description) =>
        Schema.OptionFromNullOr(Schema.Struct({ position: Schema.Finite, thickness: Schema.Finite })).pipe(Schema.annotateEncoded({ description })),
    ),
    italicAngle: Schema.Finite.annotate({ description: 'Degrees counterclockwise from vertical, negative for a right lean' }),
    monospace: Schema.Boolean.annotate({ description: 'The font declares fixed-width spacing' }),
    features: Schema.Array(Schema.Struct({ tag: Schema.String, name: Schema.OptionFromOptionalKey(Schema.String) })).annotate({ description: 'GSUB and GPOS feature tags across all scripts, plus mapped AAT tags; names identify stylistic sets and character variants' }),
    axes: Schema.Array(Schema.Struct({ tag: Schema.String, name: Schema.String, ...Struct.Record(['min', 'default', 'max', 'value'], Schema.Finite) })).annotate({ description: 'Variation axes in fvar order with the value this instance applies' }),
    instances: Schema.Array(Schema.Struct({ name: Schema.OptionFromOptionalKey(Schema.String), coordinates: Schema.Array(Schema.Finite) })).annotate({ description: 'Named instances with their values in axes order' }),
    glyphs: Schema.Array(
        Schema.Struct({
            codepoint: Schema.Int.annotate({ description: 'Shaped glyph ID' }),
            cluster: Schema.Int.annotate({ description: 'UTF-16 offset in the input text; a cluster may contain several characters or glyphs' }),
            ...Struct.Record(['xAdvance', 'yAdvance', 'xOffset', 'yOffset'], Schema.Finite),
            extents: Schema.OptionFromNullOr(Schema.Struct(Struct.Record(['xBearing', 'yBearing', 'width', 'height'], Schema.Finite))).pipe(Schema.annotateEncoded({ description: 'Native glyph bounds from its origin; positive y is up, height normally negative. Top is yOffset + yBearing above the pen baseline' })),
        }),
    ).annotate({ description: 'Shaped text in visual glyph order, with native default features and positions in em at this instance; not application-composed text or document coordinates' }),
    missing: Schema.Array(Schema.String).annotate({ description: 'Characters of the text without a glyph, excluding controls and Unicode default-ignorable characters' }),
}).annotate({ description: 'OpenType and TrueType design metrics at the selected instance, lengths in em' });
const Options = Schema.Struct({
    text: Schema.String.annotate({ description: 'One script and direction run to shape and check for glyphs; omitted text returns no glyph geometry' }).pipe(Schema.withDecodingDefaultKey(Effect.succeed(''))),
    coordinates: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.Finite).annotate({ description: "Axis values in the face's axis order; omitted trailing axes use their defaults, absent coordinates use the face's own instance" })),
    size: Schema.OptionFromOptionalKey(Schema.Finite.check(Schema.isGreaterThan(0)).annotate({ description: 'Point size; an opsz axis follows it' })),
});
const Tables = Schema.Struct({
    stream: Schema.Struct({ buffer: Schema.Uint8Array }),
    post: Schema.Struct({ isFixedPitch: Schema.Finite }),
    'CFF ': Schema.optional(Schema.Struct({ topDict: Schema.Struct({ UnderlinePosition: Schema.Finite, UnderlineThickness: Schema.Finite, FontMatrix: Schema.Tuple([Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite]) }) })),
    fvar: Schema.optional(Schema.Struct({ instance: Schema.Array(Schema.Struct({ nameID: Schema.Int, coord: Schema.Array(Schema.Finite), postscriptNameID: Schema.optional(Schema.Int) })) })),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.fn('read')(function* (data: Uint8Array, postscriptName: string, { text, coordinates, size }: typeof Options.Type) {
    const opened = yield* Effect.try(() => create(Buffer.from(data.buffer, data.byteOffset, data.byteLength)));
    const [faceIndex, base] = yield* Effect.fromOption('fonts' in opened ? Array.findFirst(Array.fromIterable(opened.fonts.entries()), ([, { postscriptName: name }]) => name === postscriptName) : Option.some([0, opened] as const)).pipe(
        Effect.mapError(() => new Cause.NoSuchElementError(`No face ${postscriptName} in the font file`)),
        Effect.filterOrFail(
            ([, { type }]) => type === 'TTF',
            ([, { type }]) => new Cause.IllegalArgumentError(`${type} webfont containers must be decoded to OpenType or TrueType font bytes`),
        ),
        Effect.filterOrFail(
            ([, { variationAxes }]) => Option.isNone(coordinates) || coordinates.value.length <= Record.keys(variationAxes).length,
            ([, { variationAxes }]) => new Cause.IllegalArgumentError(`${postscriptName} accepts at most ${Record.keys(variationAxes).length} variation coordinates`),
        ),
    );
    const { stream, post, 'CFF ': cff, fvar } = yield* Schema.decodeUnknownEffect(Tables)(base);
    const face = new Face(new Blob(stream.buffer), opened.type === 'TTC' ? faceIndex : 0);
    const label = (id: number) => Option.liftPredicate(face.getName(id, 'en'), Predicate.isTruthy);
    const selected = Option.orElse(coordinates, () =>
        Option.map(
            Array.findFirst(fvar?.instance ?? [], ({ postscriptNameID }) => postscriptNameID !== undefined && Option.contains(label(postscriptNameID), postscriptName)),
            Struct.get('coord'),
        ),
    );
    const font = yield* Effect.try(() => (Record.isEmptyRecord(base.variationAxes) ? base : base.getVariation({ ...Record.fromEntries(Array.zip(Record.keys(base.variationAxes), Option.getOrElse(selected, Array.empty))), ...Option.match(size, { onNone: () => ({}), onSome: (opsz) => ({ opsz }) }) })));
    const { variationCoords } = yield* Schema.decodeUnknownEffect(Schema.Struct({ variationCoords: Schema.NullOr(Schema.Array(Schema.Finite)) }))(font);
    const axes = Array.zipWith(Record.toEntries(Record.filter(font.variationAxes, Predicate.isNotUndefined)), variationCoords ?? [], ([tag, axis], value) => ({ tag, ...axis, value }));
    const metrics = new HarfBuzzFont(face);
    metrics.setVariations(axes.map(({ tag, value }) => new Variation(tag, value)));
    const buffer = new HarfBuzzBuffer();
    buffer.addText(text);
    buffer.guessSegmentProperties();
    shape(metrics, buffer);
    const em = (units: number) => units / font.unitsPerEm;
    const metric = (tag: MetricsTag) => Option.fromUndefinedOr(metrics.getMetricPosition(tag));
    return {
        ...Record.map({ ascender: MetricsTag.HORIZONTAL_ASCENDER, descender: MetricsTag.HORIZONTAL_DESCENDER }, flow(metric, Option.map(em))),
        ...Record.map({ xHeight: MetricsTag.X_HEIGHT, capHeight: MetricsTag.CAP_HEIGHT }, (tag, field) =>
            Option.orElse(Option.filter(metric(tag), Predicate.isTruthy), () => Option.map(Array.findFirst(font.glyphsForString(CHARACTERS[field]), flow(Struct.get('id'), Predicate.isTruthy)), flow(Struct.get('bbox'), Struct.get('maxY')))).pipe(Option.map(em)),
        ),
        lineGap: em(metrics.hExtents().lineGap),
        bounds: Record.map(Struct.pick(font.bbox, ['minX', 'minY', 'maxX', 'maxY']), em),
        ...Record.map({ underline: [MetricsTag.UNDERLINE_OFFSET, MetricsTag.UNDERLINE_SIZE], strikeout: [MetricsTag.STRIKEOUT_OFFSET, MetricsTag.STRIKEOUT_SIZE] } as const, ([offsetTag, sizeTag], field) =>
            Option.filter(
                field === 'underline' && cff !== undefined
                    ? Option.some({ position: cff.topDict.UnderlinePosition * cff.topDict.FontMatrix[3], thickness: cff.topDict.UnderlineThickness * cff.topDict.FontMatrix[3] })
                    : Option.map(Option.all({ position: metric(offsetTag), thickness: metric(sizeTag) }), ({ position, thickness }) => Record.map({ position: position - thickness / 2, thickness }, em)),
                ({ thickness }) => thickness !== 0,
            ),
        ),
        italicAngle: Option.match(
            Array.findFirst(axes, ({ tag }) => tag === 'slnt'),
            { onNone: () => font.italicAngle, onSome: Struct.get('value') },
        ),
        monospace: post.isFixedPitch !== 0,
        features: Array.dedupeWith(
            [...(['GSUB', 'GPOS'] as const).flatMap((table) => face.getTableFeatureTags(table).map((tag, index) => ({ tag, name: Option.flatMap(Option.fromUndefinedOr(face.getFeatureNameIds(table, index)?.uiLabelNameId), label) }))), ...font.availableFeatures.map((tag) => ({ tag, name: Option.none<string>() }))],
            (first, second) => first.tag === second.tag,
        ),
        axes,
        instances: (fvar?.instance ?? []).map(({ nameID, coord }) => ({ name: label(nameID), coordinates: coord })),
        glyphs: Array.zipWith(buffer.getGlyphInfos(), buffer.getGlyphPositions(), ({ codepoint, cluster }, position) => ({ codepoint, cluster, ...Record.map(position, em), extents: Option.map(Option.fromUndefinedOr(metrics.glyphExtents(codepoint)), Record.map(em)) })),
        missing: Array.dedupe([...text.replaceAll(/[\p{Cc}\p{Default_Ignorable_Code_Point}]/gu, '')]).filter((character) => font.glyphsForString(character).every(({ id }) => id === 0)),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { CHARACTERS, Font, Options, read };
