import { Array, Cause, Effect, Option, Predicate, Record, Schema, Struct } from 'effect';
import { open } from 'fontkit';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CHARACTERS = { ascender: 'd', descender: 'p', xHeight: 'x', capHeight: 'X', cjk: '一あ가', monospace: 'abcdefghijklmnopqrstuvwxyz0123456789' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Font = Schema.Struct({
    ...Record.map({ ascender: 'Ascent above the baseline', descender: 'Descent below the baseline, negative', xHeight: `Height of "${CHARACTERS.xHeight}"`, capHeight: `Height of "${CHARACTERS.capHeight}"` }, (description) => Schema.OptionFromNullOr(Schema.Finite).pipe(Schema.annotateEncoded({ description }))),
    lineGap: Schema.Finite.annotate({ description: 'Designed gap between lines' }),
    bounds: Schema.Struct(Struct.Record(['minX', 'minY', 'maxX', 'maxY'], Schema.Finite)).annotate({ description: 'Box enclosing every glyph' }),
    ...Record.map({ underline: 'Underline stroke centre, negative below the baseline, and thickness; null when the face sets none', strikeout: 'Strikeout stroke centre and thickness; null when the face sets none' }, (description) =>
        Schema.OptionFromNullOr(Schema.Struct({ position: Schema.Finite, thickness: Schema.Finite })).pipe(Schema.annotateEncoded({ description })),
    ),
    italicAngle: Schema.Finite.annotate({ description: 'Degrees counterclockwise from vertical, negative for a right lean' }),
    monospace: Schema.Boolean.annotate({ description: 'Every a-z and 0-9 glyph shares one advance' }),
    features: Schema.Array(Schema.Struct({ tag: Schema.String, name: Schema.OptionFromOptionalKey(Schema.String) })).annotate({ description: 'Feature tags of the default script, AAT features as OpenType tags, with the name of a stylistic set or character variant' }),
    axes: Schema.Array(Schema.Struct({ tag: Schema.String, name: Schema.String, ...Struct.Record(['min', 'default', 'max', 'value'], Schema.Finite) })).annotate({ description: 'Variation axes in fvar order with the value this instance applies' }),
    instances: Schema.Array(Schema.Struct({ name: Schema.OptionFromOptionalKey(Schema.String), coordinates: Schema.Array(Schema.Finite) })).annotate({ description: 'Named instances with their values in axes order' }),
    missing: Schema.Array(Schema.String).annotate({ description: 'Characters of the text without a glyph, control characters aside' }),
}).annotate({ description: 'Design metrics of the face at the instance, lengths in em' });
const Options = Schema.Struct({
    text: Schema.String.annotate({ description: 'Characters to check for glyphs' }).pipe(Schema.withDecodingDefaultKey(Effect.succeed(''))),
    coordinates: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.Finite).annotate({ description: "Axis values in the face's axis order, its own instance when absent" })),
    size: Schema.OptionFromOptionalKey(Schema.Finite.check(Schema.isGreaterThan(0)).annotate({ description: 'Point size; an opsz axis follows it' })),
});
const Name = Schema.optional(Schema.Struct({ en: Schema.optional(Schema.Union([Schema.String, Schema.Uint8Array])) }));
const Tables = Schema.Struct({
    directory: Schema.Struct({ tables: Schema.Record(Schema.String, Schema.Struct({ offset: Schema.Int, length: Schema.Int })) }),
    stream: Schema.Struct({ view: Schema.instanceOf(DataView) }),
    variationCoords: Schema.NullOr(Schema.Array(Schema.Finite)),
    'CFF ': Schema.optional(Schema.Struct({ topDict: Schema.Struct({ UnderlinePosition: Schema.Finite, UnderlineThickness: Schema.Finite, FontMatrix: Schema.Tuple([Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite]) }) })),
    GSUB: Schema.optional(Schema.Struct({ featureList: Schema.Array(Schema.Struct({ tag: Schema.String, feature: Schema.Struct({ featureParams: Schema.NullOr(Schema.Struct({ nameID: Schema.Int })) }) })) })),
    fvar: Schema.optional(Schema.Struct({ instance: Schema.Array(Schema.Struct({ nameID: Schema.Int, coord: Schema.Array(Schema.Finite) })) })),
    name: Schema.Struct({ records: Schema.Struct({ fontSubfamily: Name, preferredSubfamily: Name, fontFeatures: Schema.Record(Schema.String, Name).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))) }) }),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.fn('read')(function* (file: string, postscriptName: string, { text, coordinates, size }: typeof Options.Type) {
    const base = yield* Effect.tryPromise(() => open(file)).pipe(
        Effect.map((opened) => ('fonts' in opened ? opened.getFont(postscriptName) : opened)),
        Effect.filterOrFail(Predicate.isNotNull, () => new Cause.NoSuchElementError(`${file} holds no face ${postscriptName}`)),
        Effect.filterOrFail(
            ({ variationAxes }) => Option.isNone(coordinates) || !Record.isEmptyRecord(variationAxes),
            () => new Cause.IllegalArgumentError(`${postscriptName} in ${file} has no variation axes to take coordinates`),
        ),
    );
    const font = Record.isEmptyRecord(base.variationAxes) ? base : base.getVariation({ ...Record.fromEntries(Array.zip(Record.keys(base.variationAxes), Option.getOrElse(coordinates, Array.empty))), ...Option.match(size, { onNone: () => ({}), onSome: (opsz) => ({ opsz }) }) });
    const { directory, stream, variationCoords, 'CFF ': cff, GSUB, fvar, name } = yield* Schema.decodeUnknownEffect(Tables)(font);
    const names: typeof name.records.fontFeatures = { ...name.records.fontFeatures, 2: name.records.fontSubfamily, 17: name.records.preferredSubfamily };
    const label = (id: number) => Option.liftPredicate(names[id]?.en, Predicate.isString);
    const em = (units: number) => units / font.unitsPerEm;
    const drawn = Record.map(CHARACTERS, (characters) => font.glyphsForString(characters).filter(({ id }) => id !== 0));
    const TYPO = { ascender: 68, descender: 70, lineGap: 72 } as const;
    const typo = Option.map(
        Option.filter(Record.get(directory.tables, 'OS/2'), ({ length }) => length >= TYPO.lineGap + Int16Array.BYTES_PER_ELEMENT),
        ({ offset }) => ({ ascender: stream.view.getInt16(offset + TYPO.ascender), descender: -Math.abs(stream.view.getInt16(offset + TYPO.descender)), lineGap: stream.view.getInt16(offset + TYPO.lineGap) }),
    );
    const declared = drawn.cjk.length > 0 ? Option.none() : typo;
    return {
        ...Record.map({ ascender: 'maxY', descender: 'minY' } as const, (side, field) =>
            Option.map(
                Option.match(Array.head(drawn[field]), {
                    onNone: () => Option.map(declared, Struct.get(field)),
                    onSome: ({ id, bbox }) => Option.some(Option.match(declared, { onNone: () => bbox[side], onSome: (metrics) => metrics[field] + ('MVAR' in directory.tables ? 0 : bbox[side] - base.getGlyph(id).bbox[side]) })),
                }),
                em,
            ),
        ),
        ...Record.map({ xHeight: font.xHeight, capHeight: font.capHeight }, (stated, field) => Option.map(Option.match(Array.head(drawn[field]), { onNone: () => Option.liftPredicate(stated, Predicate.isTruthy), onSome: ({ bbox }) => Option.some(bbox.maxY) }), em)),
        lineGap: em(Option.match(typo, { onNone: () => font.lineGap, onSome: Struct.get('lineGap') })),
        bounds: Record.map(Struct.pick(font.bbox, ['minX', 'minY', 'maxX', 'maxY']), em),
        underline: Option.liftPredicate(
            cff === undefined
                ? { position: em(font.underlinePosition - ('CFF2' in directory.tables ? font.underlineThickness / 2 : 0)), thickness: em(font.underlineThickness) }
                : { position: cff.topDict.UnderlinePosition * cff.topDict.FontMatrix[3], thickness: cff.topDict.UnderlineThickness * cff.topDict.FontMatrix[3] },
            ({ thickness }) => thickness !== 0,
        ),
        strikeout: Option.flatMap(Option.fromUndefinedOr(font['OS/2']), ({ yStrikeoutPosition, yStrikeoutSize }) => Option.liftPredicate({ position: em(yStrikeoutPosition - yStrikeoutSize / 2), thickness: em(yStrikeoutSize) }, ({ thickness }) => thickness !== 0)),
        italicAngle: font.italicAngle,
        monospace: Array.dedupe(drawn.monospace.map(({ advanceWidth }) => advanceWidth)).length === 1,
        features: Array.dedupe(font.availableFeatures).map((tag) => ({ tag, name: Array.findFirst(GSUB?.featureList ?? [], ({ tag: own, feature: { featureParams } }) => (own === tag && featureParams !== null ? label(featureParams.nameID) : Option.none())) })),
        axes: Array.zipWith(Record.toEntries(Record.filter(font.variationAxes, Predicate.isNotUndefined)), variationCoords ?? [], ([tag, axis], value) => ({ tag, ...axis, value })),
        instances: (fvar?.instance ?? []).map(({ nameID, coord }) => ({ name: label(nameID), coordinates: coord })),
        missing: Array.dedupe([...text.replaceAll(/\p{Cc}/gu, '')]).filter((character) => font.glyphsForString(character).every(({ id }) => id === 0)),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { CHARACTERS, Font, Options, read };
