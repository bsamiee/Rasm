import uFuzzy from '@leeoniya/ufuzzy';

// --- [OPERATIONS] ----------------------------------------------------------------------

const rank = (query: string, candidates: readonly string[]): readonly string[] => {
    const [found, info, order] = new uFuzzy({ intraMode: 1, unicode: true, interSplit: "[^\\p{L}\\d']+", intraSplit: '\\p{Ll}\\p{Lu}', intraBound: '\\p{L}\\d|\\d\\p{L}|\\p{Ll}\\p{Lu}', intraChars: "[\\p{L}\\d']", intraContr: "'\\p{L}{1,2}\\b" }).search([...candidates], query, 0, Number.POSITIVE_INFINITY);
    return found === null ? candidates : (info === null ? found : order.flatMap((position) => info.idx[position] ?? [])).flatMap((index) => candidates[index] ?? []);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { rank };
