import { Schema, SchemaTransformation, String } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const TAG_LENGTH = 4;
const PRINTABLE_ASCII = /^[\u{20}-\u{7E}]+$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Tag = Schema.String.check(Schema.isBetweenLength(1, TAG_LENGTH), Schema.isPattern(PRINTABLE_ASCII)).pipe(Schema.decodeTo(Schema.String.check(Schema.isBetweenLength(TAG_LENGTH, TAG_LENGTH)), SchemaTransformation.transform({ decode: String.padEnd(TAG_LENGTH), encode: String.trimEnd })));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Tag };
