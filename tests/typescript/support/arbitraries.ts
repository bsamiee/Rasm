import { Arbitrary, Array, Effect, flow, Schema } from 'effect';

// --- [OPERATIONS] ----------------------------------------------------------------------

const uniqueArray = <S extends Schema.Constraint>(item: S, length: number): Arbitrary.Arbitrary<readonly S['Type'][]> =>
    Arbitrary.schema(Schema.UniqueArray(item).pipe(Schema.check(Schema.isBetweenLength(length, length))));

const missingLabels = <A, const Label extends string>(
    arbitrary: Arbitrary.Arbitrary<A>,
    classify: (value: A) => Label | readonly Label[],
    labels: readonly Label[],
    options: Arbitrary.SampleOptions,
): Effect.Effect<readonly Label[], Arbitrary.SampleError> =>
    Effect.map(Arbitrary.sampleEffect(arbitrary, options), (samples) => Array.difference(labels, Array.flatMap(samples, flow(classify, Array.ensure))));

// --- [EXPORTS] -------------------------------------------------------------------------

export { missingLabels, uniqueArray };
