// Line operations over the text a child wrote or a reply holds, the non-empty lines and the first of them

// --- [CONSTANTS] -----------------------------------------------------------------------

const _LINE = /\r?\n/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

// The non-empty lines of a text under either line ending
const lines = (text: string): readonly string[] => text.split(_LINE).filter((line) => line !== '');

// The first non-empty line, '' for a text without one
const first = (text: string): string => lines(text)[0] ?? '';

// --- [EXPORTS] -------------------------------------------------------------------------

export { first, lines };
