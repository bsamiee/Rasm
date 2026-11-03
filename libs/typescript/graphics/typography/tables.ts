import { Array, Effect, Predicate, Schema, SchemaGetter, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const GLYPH_ID_MASK = 0xff_ff;

// --- [MODELS] --------------------------------------------------------------------------

const decoded = <From extends Schema.Top, To extends Schema.Top>(from: From, to: To, decode: (value: From['Type']) => To['Encoded']) => from.pipe(Schema.decodeTo(to, { decode: SchemaGetter.transform(decode), encode: SchemaGetter.forbiddenEncoding }));
const Glyphs = Schema.Array(Schema.Int);
const LazyArray = Schema.declare(Schema.is(Schema.Struct(Struct.Record(['get', 'toArray'], Schema.declare(Predicate.isFunction))))).annotate({ description: 'fontkit lazy array, items decoding where read' });
const Coverage = decoded(Schema.Union([Schema.Struct({ version: Schema.Literal(1), glyphs: Glyphs }), Schema.Struct({ version: Schema.Literal(2), rangeRecords: Schema.Array(Schema.Struct(Struct.Record(['start', 'end'], Schema.Int))) })]), Glyphs, (coverage) =>
    coverage.version === 1 ? coverage.glyphs : coverage.rangeRecords.flatMap(({ start, end }) => Array.range(start, end)),
);
const Substitution = Schema.Struct({ coverage: Glyphs, alternates: Schema.Array(Glyphs).annotate({ description: 'Alternates by coverage index, empty for lookups choosing none' }) });
const Subtable: Schema.Codec<typeof Substitution.Type, unknown> = Schema.Union([
    decoded(Schema.Struct({ coverage: Coverage, deltaGlyphID: Schema.Int }), Substitution, ({ coverage, deltaGlyphID }) => ({ coverage, alternates: coverage.map((glyph) => [(glyph + deltaGlyphID) & GLYPH_ID_MASK]) })),
    decoded(Schema.Struct({ coverage: Coverage, substitute: LazyArray }), Substitution, ({ coverage, substitute }) => ({ coverage, alternates: substitute.toArray().map(Array.of) })),
    decoded(Schema.Struct({ coverage: Coverage, alternateSet: LazyArray }), Substitution, ({ coverage, alternateSet }) => ({ coverage, alternates: alternateSet.toArray() })),
    decoded(Schema.Struct({ extension: Schema.suspend(() => Subtable) }), Substitution, Struct.get('extension')),
    decoded(Schema.Struct({ coverage: Coverage }), Substitution, ({ coverage }) => ({ coverage, alternates: [] })),
    decoded(Schema.Struct({}), Substitution, () => ({ coverage: [], alternates: [] })),
]).annotate({ description: 'GSUB subtable, a format without one coverage table reading as empty' });
const Tables = Schema.Struct({
    post: Schema.Struct({ isFixedPitch: Schema.Finite.pipe(Schema.decodeTo(Schema.Boolean, { decode: SchemaGetter.Boolean(), encode: SchemaGetter.Number() })) }),
    'OS/2': Schema.OptionFromOptional(Schema.Struct({ usWeightClass: Schema.Int, usWidthClass: Schema.Int, fsType: Schema.Struct(Struct.Record(['noEmbedding', 'viewOnly', 'editable', 'noSubsetting', 'bitmapOnly'], Schema.Boolean)) })),
    'CFF ': Schema.OptionFromOptional(Schema.Struct({ topDict: Schema.Struct({ UnderlinePosition: Schema.Finite, UnderlineThickness: Schema.Finite, FontMatrix: Schema.Tuple([Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite]) }) })),
    GSUB: Schema.OptionFromOptional(Schema.Struct({ lookupList: LazyArray })),
    JSTF: Schema.Struct({ scriptList: Schema.Array(Schema.Struct({ tag: Schema.String, script: Schema.Struct({ extenderGlyphs: Schema.OptionFromNullOr(Schema.Array(Schema.Int)) }) })) }).pipe(Schema.withDecodingDefault(Effect.succeed({ scriptList: [] }))),
    fvar: Schema.Struct({ instance: Schema.Array(Schema.Struct({ nameID: Schema.Int, coord: Schema.Array(Schema.Finite), postscriptNameID: Schema.OptionFromOptionalKey(Schema.Int) })) }).pipe(Schema.withDecodingDefault(Effect.succeed({ instance: [] }))),
});
const Lookup = Schema.Struct({ subTables: Schema.Array(Subtable) });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Lookup, Tables };
