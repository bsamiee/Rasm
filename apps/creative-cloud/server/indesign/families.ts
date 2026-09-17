// --- [IMPORTS] -------------------------------------------------------------------------

import { Schema, Struct } from 'effect';

// --- [TABLE] ---------------------------------------------------------------------------

const FAMILIES = {
    'latin.sans': { family: 'TT Commons Pro', postScriptName: 'TTCommonsPro-Rg' },
    'latin.serif': { family: 'Adobe Garamond Pro', postScriptName: 'AGaramondPro-Regular' },
    'latin.mono': { family: 'Letter Gothic Std', postScriptName: 'LetterGothicStd' },
    persian: { family: 'Noto Sans Arabic', postScriptName: 'NotoSansArabic-Regular' },
    arabic: { family: 'Noto Sans Arabic', postScriptName: 'NotoSansArabic-Regular' },
} as const;

// --- [TYPES] ---------------------------------------------------------------------------

type FamilyKey = (typeof FamilyKey)['Type'];

// --- [MODELS] --------------------------------------------------------------------------

const FamilyKey: Schema.Literals<Array<keyof typeof FAMILIES>> = Schema.Literals(Struct.keys(FAMILIES));

// --- [EXPORTS] -------------------------------------------------------------------------

export { FAMILIES, FamilyKey };
