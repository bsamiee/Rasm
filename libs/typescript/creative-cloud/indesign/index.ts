import { Record, Schema, Struct } from 'effect';

// --- [TABLES] --------------------------------------------------------------------------

const SEARCHES = {
    Text: { find: 'findText', change: 'changeText', preferences: ['findTextPreferences', 'findChangeTextOptions', 'changeTextPreferences'] },
    Grep: { find: 'findGrep', change: 'changeGrep', preferences: ['findGrepPreferences', 'findChangeGrepOptions', 'changeGrepPreferences'] },
    Glyph: { find: 'findGlyph', change: 'changeGlyph', preferences: ['findGlyphPreferences', 'findChangeGlyphOptions', 'changeGlyphPreferences'] },
    Object: { find: 'findObject', change: 'changeObject', preferences: ['findObjectPreferences', 'findChangeObjectOptions', 'changeObjectPreferences'] },
    Color: { find: 'findColor', change: 'changeColor', preferences: ['findColorPreferences', 'findChangeColorOptions', 'changeColorPreferences'] },
    Transliterate: { find: 'findTransliterate', change: 'changeTransliterate', preferences: ['findTransliteratePreferences', 'findChangeTransliterateOptions', 'changeTransliteratePreferences'] },
} as const satisfies { readonly [M in string]: { readonly find: `find${M}`; readonly change: `change${M}`; readonly preferences: readonly [`find${M}Preferences`, `findChange${M}Options`, `change${M}Preferences`] } };

// --- [MODELS] --------------------------------------------------------------------------

class Extent extends Schema.Class<Extent, { readonly brand: unique symbol }>('Extent')(Struct.Record(['top', 'left', 'bottom', 'right'], Schema.Finite), { description: 'Outer stroke bounds in spread points, x 0 at spine or spread center' }) {}
const Search = Object.assign(Schema.Literals(Record.keys(SEARCHES)).annotate({ description: 'Find/change kind, each with its find and change methods and preference objects' }), { members: SEARCHES });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Specifier } from '#/specifier';
export { Value } from '#/value';
export { Extent, Search };
