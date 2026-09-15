// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Equal, FastCheck, flow } from 'effect';

// --- [OPERATIONS] ----------------------------------------------------------------------

const uniqueArray = <A>(arbitrary: FastCheck.Arbitrary<A>, length: number): FastCheck.Arbitrary<readonly A[]> =>
    FastCheck.uniqueArray(arbitrary, { minLength: length, maxLength: length, comparator: Equal.equals });

const missingLabels = <A, const Label extends string>(
    arbitrary: FastCheck.Arbitrary<A>,
    classify: (value: A) => Label | readonly Label[],
    labels: readonly Label[],
    parameters: FastCheck.Parameters<A>,
): readonly Label[] => Array.difference(labels, Array.flatMap(FastCheck.sample(arbitrary, parameters), flow(classify, Array.ensure)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { missingLabels, uniqueArray };
