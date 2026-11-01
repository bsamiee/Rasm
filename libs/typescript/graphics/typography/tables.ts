import { Effect, Schema, Struct } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Tables = Schema.Struct({
    directory: Schema.Struct({ tables: Schema.Record(Schema.String, Schema.Unknown) }),
    hhea: Schema.Struct(Struct.Record(['ascent', 'descent', 'lineGap'], Schema.Finite)),
    head: Schema.Struct(Struct.Record(['xMin', 'yMin', 'xMax', 'yMax'], Schema.Finite)),
    post: Schema.Struct({ isFixedPitch: Schema.Finite, italicAngle: Schema.Finite }),
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
    fvar: Schema.Struct({ instance: Schema.Array(Schema.Struct({ nameID: Schema.Int, coord: Schema.Array(Schema.Finite), postscriptNameID: Schema.OptionFromOptionalKey(Schema.Int) })) }).pipe(Schema.withDecodingDefault(Effect.succeed({ instance: [] }))),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Tables };
