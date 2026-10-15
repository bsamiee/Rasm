import type { Entry } from '../model/document.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const emailId = 'public-email';

// --- [OPERATIONS] ----------------------------------------------------------------------

const titleId = (entry: typeof Entry.Type): string => `entry-title-${entry.id}`;

// --- [EXPORTS] -------------------------------------------------------------------------

export { emailId, titleId };
