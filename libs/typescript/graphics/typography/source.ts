import { Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Source = Schema.Struct({
    data: Schema.Uint8Array.annotate({ description: 'OpenType or TrueType font file or collection bytes' }),
    postscriptName: Schema.OptionFromOptionalKey(
        Schema.String.annotate({
            description:
                "Name of the face or instance the bytes select: a face's PostScript name, a named instance's PostScript name, the named-instance form CoreText reports as kCTFontNameAttribute, <default PostScript name>_<subfamily with spaces as hyphens> (Skia-Regular_Black-Condensed), or an Adobe Technical Note 5902 generated name; absent selects the default instance of the one face the bytes hold",
        }),
    ),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Source };
