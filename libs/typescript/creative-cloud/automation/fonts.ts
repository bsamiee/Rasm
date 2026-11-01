import { Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

class AvailableFace extends Schema.Class<AvailableFace, { readonly brand: unique symbol }>('AvailableFace')({ postscriptName: Schema.String, family: Schema.String, style: Schema.String, displayName: Schema.String, file: Schema.URL }) {
    static readonly Name = Schema.String.annotate({ description: 'PostScript name, full name, or family for default face, any case' });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { AvailableFace };
