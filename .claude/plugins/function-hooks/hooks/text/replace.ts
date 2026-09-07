// Table-driven value replacement in both directions, a function replacer keeps a $ in a replacement literal

// --- [IMPORTS] -------------------------------------------------------------------------

import { fromPredicate, map, type Option, some } from '../composition/option.ts';

// --- [TYPES] ---------------------------------------------------------------------------

// RegExp from values hold the g flag replaceAll requires and apply forward alone
interface Pair {
    readonly from: string | RegExp;
    readonly to: string;
    readonly note: string;
}

type Direction = 'forward' | 'backward';

interface Replacement {
    readonly text: string;
    readonly applied: readonly Pair[];
}

interface Substitution {
    readonly pattern: string | RegExp;
    readonly value: string;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// The substitution a pair makes in each direction, none for a RegExp pair read backward
const _DIRECTION: Readonly<Record<Direction, (pair: Pair) => Option<Substitution>>> = {
    forward: (pair: Pair): Option<Substitution> => some({ pattern: pair.from, value: pair.to }),
    backward: (pair: Pair): Option<Substitution> =>
        map((from: string): Substitution => ({ pattern: pair.to, value: from }))(
            fromPredicate((from: string | RegExp): from is string => typeof from === 'string')(pair.from),
        ),
};

// Applies one pair to the text so far and records it when the text changed
const _step =
    (direction: Direction) =>
    (state: Replacement, pair: Pair): Replacement =>
        _DIRECTION[direction](pair).match<Replacement>({
            some: (substitution) =>
                fromPredicate((candidate: string): candidate is string => candidate !== state.text)(
                    state.text.replaceAll(substitution.pattern, () => substitution.value),
                ).match<Replacement>({
                    some: (text) => ({ text, applied: [...state.applied, pair] }),
                    none: () => state,
                }),
            none: () => state,
        });

const replace =
    (pairs: readonly Pair[], direction: Direction): ((text: string) => Replacement) =>
    (text: string): Replacement =>
        pairs.reduce<Replacement>(_step(direction), { text, applied: [] });

// The context lines of a replacement, one per distinct non-empty note in table order
const notes = (replacement: Replacement): readonly string[] =>
    [...new Set(replacement.applied.map((pair) => pair.note))].filter((note) => note !== '');

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Direction, Pair, Replacement };
export { notes, replace };
