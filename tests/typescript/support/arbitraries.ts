// --- [IMPORTS] -------------------------------------------------------------------------

import { Array, Effect, Equal, flow } from 'effect';
import { Arbitrary } from 'effect/unstable/arbitrary';

// --- [OPERATIONS] ----------------------------------------------------------------------

const uniqueArray = <A>(arbitrary: Arbitrary.Arbitrary<A>, length: number): Arbitrary.Arbitrary<readonly A[]> =>
    Arbitrary.all(Array.replicate(arbitrary, length)).pipe(Arbitrary.filter((values) => Array.dedupeWith(values, Equal.equals).length === length));

const missingLabels = <A, const Label extends string>(
    arbitrary: Arbitrary.Arbitrary<A>,
    classify: (value: A) => Label | readonly Label[],
    labels: readonly Label[],
    options: Arbitrary.SampleOptions,
): Effect.Effect<readonly Label[], Arbitrary.SampleError> =>
    Effect.map(Arbitrary.sampleEffect(arbitrary, options), (samples) => Array.difference(labels, Array.flatMap(samples, flow(classify, Array.ensure))));

// --- [EXPORTS] -------------------------------------------------------------------------

export { missingLabels, uniqueArray };
