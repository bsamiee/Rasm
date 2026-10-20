import { Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Source = Schema.Struct({
    data: Schema.Uint8Array.annotate({ description: 'OpenType or TrueType font file or collection bytes' }),
    postscriptName: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'PostScript name of the face, named instance, or generated instance the bytes select; absent selects the default instance of the one face the bytes hold' })),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Source };
