import { Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const AbsolutePath = Schema.String.check(Schema.isStartingWith('/'));

// --- [EXPORTS] -------------------------------------------------------------------------

export { AbsolutePath };
