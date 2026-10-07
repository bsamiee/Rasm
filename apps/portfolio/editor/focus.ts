import type { Entry } from '../model/document.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const emailId = 'public-email';

// --- [OPERATIONS] ----------------------------------------------------------------------

const titleId = (entry: typeof Entry.Type): string => `entry-title-${entry.id}`;
const focus = (id: string): number => requestAnimationFrame(() => document.querySelector<HTMLElement>(`#${CSS.escape(id)}`)?.focus());

// --- [EXPORTS] -------------------------------------------------------------------------

export { emailId, focus, titleId };
