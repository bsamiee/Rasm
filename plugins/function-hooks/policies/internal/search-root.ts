import { isInside, operands, programRow, resolvePath } from './invocation.ts';
import { type Call, refuse, type Verdict } from './verdict.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const searchRoots = ({ system: { root, home }, script: { commands } }: Call): readonly Verdict[] =>
    commands.flatMap(({ invocations: chain, cwd }) => {
        const last = chain.at(-1);
        if (last === undefined || chain.slice(0, -1).some(([name]) => programRow(name).stdin === 'always')) {
            return [];
        }
        const { roots, base } = operands(last);
        const folder = base.reduce((place, word) => resolvePath(word, place, home), cwd);
        const wide = (place: string): boolean => place.lastIndexOf('/') === 0 || home.values.some((user) => isInside(user, place)) || root.values.some((top) => top !== place && isInside(top, place));
        return roots.flatMap((word) => resolvePath(word, folder, home).values.filter(wide)).map((place) => refuse(`${last[0]} root ${place} is too wide, pass a package folder`));
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { searchRoots };
