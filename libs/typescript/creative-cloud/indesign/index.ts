import { Schema, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const MODES = ['Text', 'Grep', 'Glyph', 'Object', 'Color', 'Transliterate'] as const;

// --- [MODELS] --------------------------------------------------------------------------

class Extent extends Schema.Class<Extent, { readonly brand: unique symbol }>('Extent')(Struct.Record(['top', 'left', 'bottom', 'right'], Schema.Finite), { description: 'Outer stroke bounds in spread points, x 0 at spine or spread center' }) {}
const Search = Object.assign(Schema.Literals(MODES).annotate({ description: 'Find/change kind, each with its find and change methods and preference objects' }), {
    members: <M extends (typeof MODES)[number]>(mode: M) => ({ find: `find${mode}` as const, change: `change${mode}` as const, preferences: [`find${mode}Preferences`, `findChange${mode}Options`, `change${mode}Preferences`] as const }),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Specifier } from '#/specifier';
export { Value } from '#/value';
export { Extent, Search };
