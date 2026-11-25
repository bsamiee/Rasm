import chunk1 from '#/usage/chunk1.json' with { type: 'json' };
import chunk2 from '#/usage/chunk2.json' with { type: 'json' };
import chunk3 from '#/usage/chunk3.json' with { type: 'json' };

// --- [CONSTANTS] -----------------------------------------------------------------------

const chunks = [chunk1, chunk2, chunk3] as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { chunks };
