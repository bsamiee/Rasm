import { Array, Effect, Formatter, Number, Option, Predicate, pipe, Record, Schema, Struct } from 'effect';
import { type BBOX, type Font as Face, open } from 'fontkit';

// --- [CONSTANTS] -----------------------------------------------------------------------

const CHARACTERS = { ascender: 'd', descender: 'p', xHeight: 'x', capHeight: 'X', cjk: '一あ가', monospace: 'abcdefghijklmnopqrstuvwxyz0123456789' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Font = Schema.Struct({
    ...Record.map({ ascender: 'Ascent above the baseline', descender: 'Descent below the baseline, negative', xHeight: `Height of "${CHARACTERS.xHeight}"`, capHeight: `Height of "${CHARACTERS.capHeight}"` }, (description) => Schema.OptionFromNullOr(Schema.Finite).pipe(Schema.annotateEncoded({ description }))),
    lineGap: Schema.Finite.annotate({ description: 'Designed gap between lines' }),
    bounds: Schema.Struct(Struct.Record(['minX', 'minY', 'maxX', 'maxY'], Schema.Finite)).annotate({ description: 'Box enclosing every glyph' }),
    ...Record.map({ underline: 'Underline stroke centre, negative below the baseline, and thickness; null when the face sets none', strikeout: 'Strikeout stroke centre and thickness; null when the face sets none' }, (description) =>
        Schema.OptionFromNullOr(Schema.Struct(Struct.Record(['position', 'thickness'], Schema.Finite))).pipe(Schema.annotateEncoded({ description })),
    ),
    italicAngle: Schema.Finite.annotate({ description: 'Degrees counterclockwise from vertical, negative for a right lean' }),
    monospace: Schema.Boolean.annotate({ description: 'Every a-z and 0-9 glyph shares one advance' }),
    features: Schema.Array(Schema.Struct({ tag: Schema.String, name: Schema.OptionFromOptionalKey(Schema.String) })).annotate({ description: 'Feature tags of the default script, AAT features as OpenType tags, with the name of a stylistic set or character variant' }),
    axes: Schema.Array(Schema.Struct({ tag: Schema.String, name: Schema.String, ...Struct.Record(['min', 'default', 'max', 'value'], Schema.Finite) })).annotate({ description: 'Variation axes in fvar order with the value this instance applies' }),
    instances: Schema.Array(Schema.Struct({ name: Schema.OptionFromOptionalKey(Schema.String), coordinates: Schema.Array(Schema.Finite) })).annotate({ description: 'Named instances with their values in axes order' }),
    missing: Schema.Array(Schema.String).annotate({ description: 'Characters of the text without a glyph, control characters aside' }),
}).annotate({ description: 'Design metrics of the face at the instance, lengths in em' });
const Options = Schema.Struct({
    text: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Characters to check for glyphs' })),
    coordinates: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.Finite).annotate({ description: "Axis values in the face's axis order, its own instance when absent" })),
    size: Schema.OptionFromOptionalKey(Schema.Finite.check(Schema.isGreaterThan(0)).annotate({ description: 'Point size; an opsz axis follows it' })),
});

// --- [ERRORS] --------------------------------------------------------------------------

const FontReason = Schema.TaggedUnion({ Unreadable: { cause: Schema.Defect() }, Absent: { postscriptName: Schema.String }, Static: { postscriptName: Schema.String } });
class FontError extends Schema.TaggedError<FontError>()('FontError', { file: Schema.String, reason: FontReason }) {
    override get message(): string {
        return FontReason.match(this.reason, {
            Unreadable: ({ cause }) => `Cannot read ${this.file}: ${Formatter.format(cause)}`,
            Absent: ({ postscriptName }) => `${this.file} holds no face ${postscriptName}`,
            Static: ({ postscriptName }) => `${postscriptName} in ${this.file} has no variation axes to take coordinates`,
        });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const read = Effect.fn('read')(function* (file: string, postscriptName: string, { text, coordinates, size }: typeof Options.Type): Effect.fn.Return<typeof Font.Type, FontError> {
    const unreadable = (cause: unknown): FontError => new FontError({ file, reason: { _tag: 'Unreadable', cause } });
    const opened = yield* Effect.tryPromise({ try: () => open(file), catch: unreadable });
    const base = yield* Effect.filterOrFail(
        'fonts' in opened ? Effect.fromOption(Option.fromNullishOr(opened.getFont(postscriptName)), () => new FontError({ file, reason: { _tag: 'Absent', postscriptName } })) : Effect.succeed(opened),
        (face) => Option.isNone(coordinates) || !Record.isEmptyRecord(face.variationAxes),
        () => new FontError({ file, reason: { _tag: 'Static', postscriptName } }),
    );
    return yield* Effect.try({
        try: () => {
            const TYPO = { ascender: 68, descender: 70, lineGap: 72, length: 78 } as const;
            const Name = Schema.optional(Schema.Struct({ en: Schema.optional(Schema.Union([Schema.String, Schema.Uint8Array])) }));
            const font = Record.isEmptyRecord(base.variationAxes) ? base : base.getVariation({ ...Record.fromEntries(Array.zip(Record.keys(base.variationAxes), Option.getOrElse(coordinates, Array.empty))), ...Option.getOrUndefined(Option.map(size, (opsz) => ({ opsz }))) });
            const face = Schema.decodeUnknownSync(
                Schema.Struct({
                    directory: Schema.Struct({ tables: Schema.Record(Schema.String, Schema.Struct(Struct.Record(['offset', 'length'], Schema.Int))) }),
                    stream: Schema.Struct({ view: Schema.instanceOf(DataView) }),
                    variationCoords: Schema.NullOr(Schema.Array(Schema.Finite)),
                    'CFF ': Schema.OptionFromOptional(Schema.Struct({ topDict: Schema.Struct({ UnderlinePosition: Schema.Finite, UnderlineThickness: Schema.Finite, FontMatrix: Schema.Tuple([Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite]) }) })),
                    GSUB: Schema.OptionFromOptional(Schema.Struct({ featureList: Schema.Array(Schema.Struct({ tag: Schema.String, feature: Schema.Struct({ featureParams: Schema.OptionFromNullOr(Schema.Struct({ nameID: Schema.Int })) }) })) })),
                    fvar: Schema.OptionFromOptional(Schema.Struct({ instance: Schema.Array(Schema.Struct({ nameID: Schema.Int, coord: Schema.Array(Schema.Finite) })) })),
                    name: Schema.Struct({ records: Schema.Struct({ fontSubfamily: Name, preferredSubfamily: Name, fontFeatures: Schema.optional(Schema.Record(Schema.String, Name)) }) }),
                }),
            )(font);
            const { records } = face.name;
            const names: NonNullable<typeof records.fontFeatures> = { ...records.fontFeatures, 2: records.fontSubfamily, 17: records.preferredSubfamily };
            const os2 = Option.fromUndefinedOr(font['OS/2']);
            const em = (units: number) => units / font.unitsPerEm;
            const label = (id: number) => Option.filter(Option.fromNullishOr(names[id]?.en), Predicate.isString);
            const drawn = (instance: Face, characters: string) => Array.filter(instance.glyphsForString(characters), (glyph) => glyph.id !== 0);
            const edge = (instance: Face, character: string, side: keyof BBOX) => Option.map(Array.head(drawn(instance, character)), (glyph) => glyph.bbox[side]);
            const stroke = (position: number, thickness: number) => (thickness === 0 ? Option.none() : Option.some({ position, thickness }));
            const typo = Option.map(
                Option.filter(Option.fromUndefinedOr(face.directory.tables['OS/2']), ({ length }) => length >= TYPO.length),
                ({ offset }) => ({ ascender: face.stream.view.getInt16(offset + TYPO.ascender), descender: -Math.abs(face.stream.view.getInt16(offset + TYPO.descender)), lineGap: face.stream.view.getInt16(offset + TYPO.lineGap) }),
            );
            const cjk = Array.isReadonlyArrayNonEmpty(drawn(font, CHARACTERS.cjk));
            const vertical = (field: 'ascender' | 'descender', side: keyof BBOX) =>
                pipe(
                    typo,
                    Option.filter(() => !cjk),
                    Option.map((fields) => fields[field] + ('MVAR' in face.directory.tables ? 0 : Option.getOrElse(Option.zipWith(edge(font, CHARACTERS[field], side), edge(base, CHARACTERS[field], side), Number.subtract), () => 0))),
                    Option.orElse(() => edge(font, CHARACTERS[field], side)),
                    Option.map(em),
                );
            const height = (field: 'xHeight' | 'capHeight') =>
                pipe(
                    edge(font, CHARACTERS[field], 'maxY'),
                    Option.orElse(() => Option.filter(Option.flatMapNullishOr(os2, Struct.get(field)), (units) => units !== 0)),
                    Option.map(em),
                );
            return {
                ascender: vertical('ascender', 'maxY'),
                descender: vertical('descender', 'minY'),
                lineGap: em(Option.getOrElse(Option.map(typo, Struct.get('lineGap')), () => font.hhea.lineGap)),
                xHeight: height('xHeight'),
                capHeight: height('capHeight'),
                bounds: Record.map(Struct.pick(font.bbox, ['minX', 'minY', 'maxX', 'maxY']), em),
                underline: Option.match(face['CFF '], {
                    onSome: ({ topDict: { UnderlinePosition, UnderlineThickness, FontMatrix } }) => stroke(UnderlinePosition * FontMatrix[3], UnderlineThickness * FontMatrix[3]),
                    onNone: () => stroke(em(font.underlinePosition - ('CFF2' in face.directory.tables ? font.underlineThickness / 2 : 0)), em(font.underlineThickness)),
                }),
                strikeout: Option.flatMap(os2, ({ yStrikeoutPosition, yStrikeoutSize }) => stroke(em(yStrikeoutPosition - yStrikeoutSize / 2), em(yStrikeoutSize))),
                italicAngle: font.italicAngle,
                monospace: pipe(drawn(font, CHARACTERS.monospace), Array.map(Struct.get('advanceWidth')), Array.dedupe).length === 1,
                features: Array.map(Array.dedupe(font.availableFeatures), (tag) => ({
                    tag,
                    name: Option.flatMap(face.GSUB, ({ featureList }) => Array.findFirst(featureList, ({ tag: own, feature }) => (own === tag ? Option.flatMap(feature.featureParams, ({ nameID }) => label(nameID)) : Option.none()))),
                })),
                axes: Array.zipWith(pipe(font.variationAxes, Record.filter(Predicate.isNotUndefined), Record.toEntries), face.variationCoords ?? [], ([tag, axis], value) => ({ tag, ...axis, value })),
                instances: Array.flatMap(Option.toArray(face.fvar), ({ instance }) => Array.map(instance, ({ nameID, coord }) => ({ name: label(nameID), coordinates: coord }))),
                missing: pipe(
                    Array.flatMap(Option.toArray(text), (characters) => Array.fromIterable(characters.replaceAll(/\p{Cc}/gu, ''))),
                    Array.dedupe,
                    Array.filter((character) => Array.isReadonlyArrayEmpty(drawn(font, character))),
                ),
            };
        },
        catch: unreadable,
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { CHARACTERS, Font, FontError, Options, read };
