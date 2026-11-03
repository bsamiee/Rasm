import { Schema } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const AbsolutePath = Schema.String.check(Schema.isStartingWith('/')).annotate({ description: 'Absolute path' });

// --- [EXPORTS] -------------------------------------------------------------------------

export { AbsolutePath };
