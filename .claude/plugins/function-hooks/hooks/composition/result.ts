// --- [TYPES] ---------------------------------------------------------------------------

type Result<T> = { readonly kind: 'ok'; readonly value: T } | { readonly kind: 'fault'; readonly reason: string };

// --- [CONSTRUCTORS] --------------------------------------------------------------------

const ok = <T>(value: T): Result<T> => ({ kind: 'ok', value });

const fault = <T>(reason: string): Result<T> => ({ kind: 'fault', reason });

// --- [OPERATIONS] ----------------------------------------------------------------------

const faults = (results: readonly Result<unknown>[]): readonly string[] =>
    results.flatMap((result) => (result.kind === 'fault' ? [result.reason] : []));

// Fault holding every reason of the results
const invalid = <T>(results: readonly Result<unknown>[]): Result<T> => fault(faults(results).join(', '));

// Independent results combine into one, every fault's reason kept
const all = <T>(results: readonly Result<T>[]): Result<readonly T[]> => {
    const reasons = faults(results);
    return reasons.length === 0 ? ok(results.flatMap((result) => (result.kind === 'ok' ? [result.value] : []))) : invalid(results);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Result };
export { all, fault, faults, invalid, ok };
