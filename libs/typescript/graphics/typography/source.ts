import { Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Source = Schema.Struct({
    data: Schema.Uint8Array.annotate({ description: 'OpenType or TrueType font or collection bytes' }),
    postscriptName: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Face or instance name, CoreText kCTFontNameAttribute (Skia-Regular_Black-Condensed), or Adobe Technical Note 5902 name, absent for single-face default' })),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Source };
