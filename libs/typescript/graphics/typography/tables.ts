import { Array, Effect, Match, Schema, SchemaGetter, Struct } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const ClassFormats = Schema.Union([Schema.Struct({ version: Schema.Literal(1), startGlyph: Schema.Int, classValueArray: Schema.Array(Schema.Int) }), Schema.Struct({ version: Schema.Literal(2), classRangeRecord: Schema.Array(Schema.Struct({ start: Schema.Int, end: Schema.Int, class: Schema.Int })) })]);
const classValues = Match.type<typeof ClassFormats.Type>().pipe(
    Match.when({ version: 1 }, ({ startGlyph, classValueArray }) => classValueArray.map((value, index) => [startGlyph + index, value] as const)),
    Match.when({ version: 2 }, ({ classRangeRecord }) => classRangeRecord.flatMap(({ start, end, class: value }) => Array.range(start, end).map((glyph) => [glyph, value] as const))),
    Match.exhaustive,
);
const ClassDefinition = ClassFormats.pipe(
    Schema.decodeTo(Schema.ReadonlyMap(Schema.Int, Schema.Int).annotate({ description: 'Class by glyph, class 0 omitted' }), {
        decode: SchemaGetter.transform((definition): ReadonlyMap<number, number> => new Map(classValues(definition).filter(([, value]) => value !== 0))),
        encode: SchemaGetter.forbiddenEncoding,
    }),
);
const Tables = Schema.Struct({
    directory: Schema.Struct({ tables: Schema.Record(Schema.String, Schema.Unknown) }),
    hhea: Schema.Struct(Struct.Record(['ascent', 'descent', 'lineGap'], Schema.Finite)),
    head: Schema.Struct(Struct.Record(['xMin', 'yMin', 'xMax', 'yMax'], Schema.Finite)),
    post: Schema.Struct({ isFixedPitch: Schema.Finite.pipe(Schema.decodeTo(Schema.Boolean, { decode: SchemaGetter.Boolean(), encode: SchemaGetter.Number() })), italicAngle: Schema.Finite }),
    'OS/2': Schema.OptionFromOptional(
        Schema.Struct({
            usWeightClass: Schema.Int,
            usWidthClass: Schema.Int,
            fsType: Schema.Struct(Struct.Record(['noEmbedding', 'viewOnly', 'editable', 'noSubsetting', 'bitmapOnly'], Schema.Boolean)),
            ...Struct.Record(['typoAscender', 'typoDescender', 'typoLineGap'], Schema.OptionFromOptionalKey(Schema.Finite)),
            fsSelection: Schema.Struct({ useTypoMetrics: Schema.Boolean }),
        }),
    ),
    'CFF ': Schema.OptionFromOptional(Schema.Struct({ topDict: Schema.Struct({ UnderlinePosition: Schema.Finite, UnderlineThickness: Schema.Finite, FontMatrix: Schema.Tuple([Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite]) }) })),
    GDEF: Schema.OptionFromOptional(Schema.Struct({ markAttachClassDef: Schema.OptionFromNullOr(ClassDefinition) })),
    JSTF: Schema.OptionFromOptional(Schema.Struct({ scriptList: Schema.Array(Schema.Struct({ tag: Schema.String, script: Schema.Struct({ extenderGlyphs: Schema.OptionFromNullOr(Schema.Array(Schema.Int)) }) })) })),
    fvar: Schema.Struct({ instance: Schema.Array(Schema.Struct({ nameID: Schema.Int, coord: Schema.Array(Schema.Finite), postscriptNameID: Schema.OptionFromOptionalKey(Schema.Int) })) }).pipe(Schema.withDecodingDefault(Effect.succeed({ instance: [] }))),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Tables };
