import { describe, expect, it } from '@effect/vitest';
import { Array, Either, Schema } from 'effect';
import * as FastCheck from 'effect/FastCheck';
import { Arbitrate } from './arbitraries.ts';

// --- [MODELS] --------------------------------------------------------------------------

class Frame extends Schema.Class<Frame>('Frame')({
    rank: Schema.Int,
    label: Schema.optionalWith(Schema.String, { as: 'Option' }),
    note: Schema.optionalWith(Schema.String, { as: 'Option', exact: true }),
}) {}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _SEED = { seed: 7, numRuns: 64 } as const;

const _decode = Schema.decodeUnknownEither(Frame);

// --- [OPERATIONS] ----------------------------------------------------------------------

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

    it('the UNSET dialect lands explicit undefined only where the encoded signature admits it', () => {
        const samples = FastCheck.sample(Arbitrate.absence(Frame), { ..._SEED, numRuns: 256 });
        const unset = Array.filter(samples, (sample) => Object.hasOwn(sample, 'label') && sample.label === undefined);
        expect(unset.length).toBeGreaterThan(0);
        expect(Array.some(samples, (sample) => Object.hasOwn(sample, 'note') && sample.note === undefined)).toBe(false);
    });

    it('an empty key mask is the identity lane', () => {
        const base = FastCheck.record({ rank: FastCheck.integer() });
        const samples = FastCheck.sample(Arbitrate.absence(base, []), _SEED);
        expect(Array.every(samples, (sample) => Object.hasOwn(sample, 'rank'))).toBe(true);
    });

    it('the model lane varies non-required keys and always keeps the required ones', () => {
        const samples = FastCheck.sample(Arbitrate.absence({ rank: FastCheck.integer(), label: FastCheck.string() }, ['rank']), _SEED);
        expect(Array.every(samples, (sample) => Object.hasOwn(sample, 'rank'))).toBe(true);
        const labeled = Array.filter(samples, (sample) => Object.hasOwn(sample, 'label'));
        expect(labeled.length).toBeGreaterThan(0);
        expect(labeled.length).toBeLessThan(samples.length);
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

describe('coverage gauge', () => {
    it('a full-range generator covers every parity label', () => {
        expect(Arbitrate.coverage(FastCheck.integer(), (value) => (value % 2 === 0 ? 'even' : 'odd'), ['even', 'odd'])).toEqual([]);
    });

    it('an over-biased generator is refuted by the labels it never produces', () => {
        const missing = Arbitrate.coverage(FastCheck.integer({ min: 0, max: 0 }), (value) => (value % 2 === 0 ? 'even' : 'odd'), ['even', 'odd']);
        expect(missing).toEqual(['odd']);
    });

    it('multi-label classification credits every arm one sample touches', () => {
        const missing = Arbitrate.coverage(FastCheck.integer({ min: 1, max: 1 }), () => ['positive', 'small'], ['positive', 'small', 'huge']);
        expect(missing).toEqual(['huge']);
    });
});
