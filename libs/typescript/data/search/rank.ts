import uFuzzy from '@leeoniya/ufuzzy';
import { Digits } from '#/digits';

// --- [CONSTANTS] -----------------------------------------------------------------------

const FUZZY = new uFuzzy({ intraMode: 1, unicode: true, interSplit: "[^\\p{L}\\p{Nd}']+", intraSplit: '\\p{Ll}\\p{Lu}', intraBound: '\\p{L}\\p{Nd}|\\p{Nd}\\p{L}|\\p{Ll}\\p{Lu}', intraChars: "[\\p{L}\\p{Nd}']", intraContr: "'\\p{L}{1,2}\\b" });

// --- [OPERATIONS] ----------------------------------------------------------------------

const rank = (query: string, candidates: readonly string[]): readonly string[] => {
    const [found, info, order] = FUZZY.search(candidates.map(Digits.fold), Digits.fold(query), 0, Number.POSITIVE_INFINITY);
    return found === null ? candidates : (info === null ? found : order.flatMap((position) => info.idx[position] ?? [])).flatMap((index) => candidates[index] ?? []);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { rank };
