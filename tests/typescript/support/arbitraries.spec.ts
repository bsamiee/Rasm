import { describe, expect, it } from '@effect/vitest';
import { Array, Either, Schema } from 'effect';
import * as FastCheck from 'effect/FastCheck';
import { Arbitraries } from './arbitraries.ts';

// --- [MODELS] --------------------------------------------------------------------------

class OptionalFieldsRecord extends Schema.Class<OptionalFieldsRecord>(
    'OptionalFieldsRecord',
)({
    version: Schema.Int,
    label: Schema.optionalWith(Schema.String, { as: 'Option' }),
    note: Schema.optionalWith(Schema.String, { as: 'Option', exact: true }),
}) {}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _SEED = { seed: 7, numRuns: 64 } as const;

const _decode = Schema.decodeUnknownEither(OptionalFieldsRecord);

// --- [OPERATIONS] ----------------------------------------------------------------------

describe('optional field generation', () => {
    it('varies optional-key presence across the encoded samples', () => {
        const samples = FastCheck.sample(
            Arbitraries.optionalFields(OptionalFieldsRecord),
            _SEED,
        );
        const present = Array.filter(samples, (sample) =>
            Object.hasOwn(sample, 'label'),
        );
        expect(present.length).toBeGreaterThan(0);
        expect(present.length).toBeLessThan(samples.length);
    });

    it('every generated encoded value decodes successfully', () => {
        const samples = FastCheck.sample(
            Arbitraries.optionalFields(OptionalFieldsRecord),
            _SEED,
        );
        expect(
            Array.every(samples, (sample) => Either.isRight(_decode(sample))),
        ).toBe(true);
    });

    it('explicit undefined is generated only for encoded fields that accept it', () => {
        const samples = FastCheck.sample(
            Arbitraries.optionalFields(OptionalFieldsRecord),
            {
                ..._SEED,
                numRuns: 256,
            },
        );
        const unset = Array.filter(
            samples,
            (sample) =>
                Object.hasOwn(sample, 'label') && sample.label === undefined,
        );
        expect(unset.length).toBeGreaterThan(0);
        expect(
            Array.some(
                samples,
                (sample) =>
                    Object.hasOwn(sample, 'note') && sample.note === undefined,
            ),
        ).toBe(false);
    });

    it('empty optional-key lists preserve every generated field', () => {
        const base = FastCheck.record({ version: FastCheck.integer() });
        const samples = FastCheck.sample(
            Arbitraries.optionalFields(base, []),
            _SEED,
        );
        expect(
            Array.every(samples, (sample) => Object.hasOwn(sample, 'version')),
        ).toBe(true);
    });

    it('the record model varies optional keys and always keeps required keys', () => {
        const samples = FastCheck.sample(
            Arbitraries.optionalFields(
                { version: FastCheck.integer(), label: FastCheck.string() },
                ['version'],
            ),
            _SEED,
        );
        expect(
            Array.every(samples, (sample) => Object.hasOwn(sample, 'version')),
        ).toBe(true);
        const labeled = Array.filter(samples, (sample) =>
            Object.hasOwn(sample, 'label'),
        );
        expect(labeled.length).toBeGreaterThan(0);
        expect(labeled.length).toBeLessThan(samples.length);
    });
});

describe('distinct arrays', () => {
    it('generated payload sets are pairwise distinct even over a two-value domain', () => {
        const samples = FastCheck.sample(
            Arbitraries.distinctArray(
                FastCheck.integer({ min: 0, max: 1 }),
                2,
                (a, b) => a === b,
            ),
            _SEED,
        );
        expect(
            Array.every(
                samples,
                (pair) => pair.length === 2 && pair[0] !== pair[1],
            ),
        ).toBe(true);
    });

    it('unconstrained pair generators can produce equal values', () => {
        const samples = FastCheck.sample(
            FastCheck.tuple(
                FastCheck.integer({ min: 0, max: 1 }),
                FastCheck.integer({ min: 0, max: 1 }),
            ),
            _SEED,
        );
        expect(Array.some(samples, ([left, right]) => left === right)).toBe(
            true,
        );
    });
});

describe('classification coverage', () => {
    it('full-range generators cover every parity label', () => {
        expect(
            Arbitraries.missingClassifications(
                FastCheck.integer(),
                (value) => (value % 2 === 0 ? 'even' : 'odd'),
                ['even', 'odd'],
            ),
        ).toEqual([]);
    });

    it('over-biased generators report labels they never produce', () => {
        const missing = Arbitraries.missingClassifications(
            FastCheck.integer({ min: 0, max: 0 }),
            (value) => (value % 2 === 0 ? 'even' : 'odd'),
            ['even', 'odd'],
        );
        expect(missing).toEqual(['odd']);
    });

    it('multi-label classification records every label produced by one sample', () => {
        const missing = Arbitraries.missingClassifications(
            FastCheck.integer({ min: 1, max: 1 }),
            () => ['positive', 'small'],
            ['positive', 'small', 'huge'],
        );
        expect(missing).toEqual(['huge']);
    });
});
