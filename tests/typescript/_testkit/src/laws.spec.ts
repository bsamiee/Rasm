import { describe, expect, it } from '@effect/vitest';
import { Effect } from 'effect';
import * as FastCheck from 'effect/FastCheck';
import { Law, LawTautology } from './laws.ts';

// --- [TYPES] -----------------------------------------------------------------------------

type Combine = (left: number, right: number) => number;

// --- [CONSTANTS] -------------------------------------------------------------------------

const _ARGS = { x: FastCheck.integer(), y: FastCheck.integer(), z: FastCheck.integer() } as const;

// Associativity over a foil that subtracts: (1-2)-3 diverges from 1-(2-3), so the witness refutes.
const _associativity = Law.make<Combine, typeof _ARGS>({
    name: 'combine is associative',
    arbitraries: _ARGS,
    predicate: (combine, { x, y, z }) => Effect.succeed(combine(combine(x, y), z) === combine(x, combine(y, z))),
    witness: { label: 'subtraction foil', foil: (left, right) => left - right, args: { x: 1, y: 2, z: 3 } },
});

// --- [OPERATIONS] ------------------------------------------------------------------------

describe('law registration', () => {
    Law.register(it, Math.min, [_associativity]);
});

describe('tautology audit', () => {
    it.effect('a genuine witness passes the audit', () =>
        Effect.asVoid(
            Law.audit({
                name: 'combine is associative',
                arbitraries: _ARGS,
                predicate: (combine: Combine, { x, y, z }) => Effect.succeed(combine(combine(x, y), z) === combine(x, combine(y, z))),
                witness: { label: 'subtraction foil', foil: (left, right) => left - right, args: { x: 1, y: 2, z: 3 } },
            }),
        ),
    );

    it.effect('a witness the predicate survives is itself the failure', () =>
        Effect.gen(function* () {
            const fault = yield* Effect.flip(
                Law.audit({
                    name: 'tautological registration',
                    arbitraries: _ARGS,
                    // The foil IS the subject: nothing can refute the predicate, so the registration must fail.
                    predicate: (combine: Combine, { x, y, z }) => Effect.succeed(combine(combine(x, y), z) === combine(x, combine(y, z))),
                    witness: { label: 'inert witness', foil: Math.min, args: { x: 1, y: 2, z: 3 } },
                }),
            );
            expect(fault).toBeInstanceOf(LawTautology);
            expect(fault.law).toBe('tautological registration');
        }),
    );
});
