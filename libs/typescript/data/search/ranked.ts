import uFuzzy from '@leeoniya/ufuzzy';

// --- [CONSTANTS] -----------------------------------------------------------------------

const fuzzy = new uFuzzy({ intraMode: 1, unicode: true, interSplit: "[^\\p{L}\\d']+", intraSplit: '\\p{Ll}\\p{Lu}', intraBound: '\\p{L}\\d|\\d\\p{L}|\\p{Ll}\\p{Lu}', intraChars: "[\\p{L}\\d']", intraContr: "'\\p{L}{1,2}\\b" });

// --- [OPERATIONS] ----------------------------------------------------------------------

const ranked = (query: string, candidates: readonly string[]): readonly string[] => {
    const [found, info, order] = fuzzy.search([...candidates], query, 0, Number.POSITIVE_INFINITY);
    const matched = (order === null || info === null ? (found ?? []) : order.flatMap((position) => info.idx[position] ?? [])).flatMap((index) => candidates[index] ?? []);
    return matched.length === 0 ? candidates : matched;
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { ranked };
