import { Effect, Schema, SchemaGetter, Struct } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Tables = Schema.Struct({
    post: Schema.Struct({ isFixedPitch: Schema.Finite.pipe(Schema.decodeTo(Schema.Boolean, { decode: SchemaGetter.Boolean(), encode: SchemaGetter.Number() })) }),
    'OS/2': Schema.OptionFromOptional(Schema.Struct({ usWeightClass: Schema.Int, usWidthClass: Schema.Int, fsType: Schema.Struct(Struct.Record(['noEmbedding', 'viewOnly', 'editable', 'noSubsetting', 'bitmapOnly'], Schema.Boolean)) })),
    'CFF ': Schema.OptionFromOptional(Schema.Struct({ topDict: Schema.Struct({ UnderlinePosition: Schema.Finite, UnderlineThickness: Schema.Finite, FontMatrix: Schema.Tuple([Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite]) }) })),
    JSTF: Schema.OptionFromOptional(Schema.Struct({ scriptList: Schema.Array(Schema.Struct({ tag: Schema.String, script: Schema.Struct({ extenderGlyphs: Schema.OptionFromNullOr(Schema.Array(Schema.Int)) }) })) })),
    fvar: Schema.Struct({ instance: Schema.Array(Schema.Struct({ nameID: Schema.Int, coord: Schema.Array(Schema.Finite), postscriptNameID: Schema.OptionFromOptionalKey(Schema.Int) })) }).pipe(Schema.withDecodingDefault(Effect.succeed({ instance: [] }))),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Tables };
