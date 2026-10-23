import { Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const FaceName = Schema.String.annotate({ description: 'PostScript name, full name, or family for default face, any case' });
class AvailableFace extends Schema.Class<AvailableFace, { readonly brand: unique symbol }>('AvailableFace')({ postscriptName: Schema.String, family: Schema.String, style: Schema.String, displayName: Schema.String, file: Schema.URL }) {}

// --- [EXPORTS] -------------------------------------------------------------------------

export { AvailableFace, FaceName };
