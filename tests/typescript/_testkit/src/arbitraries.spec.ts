import { describe, expect, it } from '@effect/vitest';
import { Array, Either, Schema } from 'effect';
import * as FastCheck from 'effect/FastCheck';
import { Arbitrate } from './arbitraries.ts';

// --- [MODELS] ----------------------------------------------------------------------------

class Frame extends Schema.Class<Frame>('Frame')({
    rank: Schema.Int,
    label: Schema.optionalWith(Schema.String, { as: 'Option' }),
    note: Schema.optionalWith(Schema.String, { as: 'Option', exact: true }),
}) {}

// --- [CONSTANTS] -------------------------------------------------------------------------

const _SEED = { seed: 7, numRuns: 64 } as const;

const _decode = Schema.decodeUnknownEither(Frame);

// --- [OPERATIONS] ------------------------------------------------------------------------

describe('absence lane', () => {
    it('varies optional-key presence across the encoded corpus', () => {
        const samples = FastCheck.sample(Arbitrate.absence(Frame), _SEED);
        const present = Array.filter(samples, (sample) => Object.hasOwn(sample, 'label'));
        expect(present.length).toBeGreaterThan(0);
        expect(present.length).toBeLessThan(samples.length);
    });

    it('every generated encoded value crosses the decode seam green', () => {
        const samples = FastCheck.sample(Arbitrate.absence(Frame), _SEED);
        expect(Array.every(samples, (sample) => Either.isRight(_decode(sample)))).toBe(true);
    });

    it('an empty key mask is the identity lane', () => {
        const base = FastCheck.record({ rank: FastCheck.integer() });
        const samples = FastCheck.sample(Arbitrate.absence(base, []), _SEED);
        expect(Array.every(samples, (sample) => Object.hasOwn(sample, 'rank'))).toBe(true);
    });
});

describe('distinct lane', () => {
    it('generated payload sets are pairwise distinct even over a two-value domain', () => {
        const samples = FastCheck.sample(
            Arbitrate.distinct(FastCheck.integer({ min: 0, max: 1 }), 2, (a, b) => a === b),
            _SEED,
        );
        expect(Array.every(samples, (pair) => pair.length === 2 && pair[0] !== pair[1])).toBe(true);
    });

    it('an unconstrained pair generator produces the equal placeholders the lane forbids', () => {
        const samples = FastCheck.sample(FastCheck.tuple(FastCheck.integer({ min: 0, max: 1 }), FastCheck.integer({ min: 0, max: 1 })), _SEED);
        expect(Array.some(samples, ([left, right]) => left === right)).toBe(true);
    });
});
