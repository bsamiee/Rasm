import { Array, Equal, HashSet, Option, pipe, Schema, SchemaAST } from 'effect';
import * as Arbitrary from 'effect/Arbitrary';
import * as FastCheck from 'effect/FastCheck';

// --- [TYPES] -----------------------------------------------------------------------------

declare namespace Arbitrate {
    type Model<T> = { readonly [K in keyof T]: FastCheck.Arbitrary<T[K]> };
    type Sampling = { readonly numRuns?: number; readonly seed?: number };
}

type _Key = { readonly name: string; readonly undefinable: boolean };

// --- [CONSTANTS] -------------------------------------------------------------------------

const _SAMPLING = { numRuns: 256, seed: 0 } as const;

// --- [OPERATIONS] ------------------------------------------------------------------------

const _optionalKeys = (schema: Schema.Schema.Any): ReadonlyArray<_Key> =>
    pipe(
        SchemaAST.getPropertySignatures(Schema.encodedBoundSchema(schema).ast),
        Array.filterMap((signature) =>
            signature.isOptional && typeof signature.name === 'string'
                ? Option.some<_Key>({
                      name: signature.name,
                      undefinable: SchemaAST.isUnion(signature.type)
                          ? Array.some(signature.type.types, SchemaAST.isUndefinedKeyword)
                          : SchemaAST.isUndefinedKeyword(signature.type),
                  })
                : Option.none(),
        ),
    );

const _reshaped = <A>(value: A, dropped: ReadonlyArray<string>, unset: ReadonlyArray<string>): A => {
    // BOUNDARY ADAPTER: dialect surgery over an encoded record — explicit undefined lands only on keys whose encoded
    // signature admits it, deletion is legal on every optional key, and deletion wins on overlap.
    const draft: Record<string, unknown> = { ...(value as Record<string, unknown>) };
    for (const key of unset) {
        draft[key] = undefined;
    }
    for (const key of dropped) {
        delete draft[key];
    }
    return draft as A;
};

const _absent = <A>(base: FastCheck.Arbitrary<A>, keys: ReadonlyArray<_Key>): FastCheck.Arbitrary<A> =>
    base.chain((value) =>
        FastCheck.tuple(
            FastCheck.subarray(Array.map(keys, (key) => key.name)),
            FastCheck.subarray(Array.filterMap(keys, (key) => (key.undefinable ? Option.some(key.name) : Option.none()))),
        ).map(([dropped, unset]) => _reshaped(value, dropped, unset)),
    );

// The field-absence lane, one entry over three input shapes: a Schema derives its encoded optional keys and attacks BOTH
// wire absence dialects — key deleted, and key present as explicit undefined where the encoded signature admits it; a base
// arbitrary takes a caller key mask (the deletion dialect the caller owns); a per-field model rides fast-check's native
// requiredKeys record. Every absence dialect attacks the decode seam through the same name.
function absence<S extends Schema.Schema.Any>(schema: S): FastCheck.Arbitrary<Schema.Schema.Encoded<S>>;
function absence<A>(arb: FastCheck.Arbitrary<A>, keys: ReadonlyArray<string>): FastCheck.Arbitrary<A>;
function absence<T>(model: Arbitrate.Model<T>, required?: ReadonlyArray<keyof T & string>): FastCheck.Arbitrary<Partial<T>>;
function absence(
    input: Schema.Schema.Any | FastCheck.Arbitrary<unknown> | Arbitrate.Model<Record<string, unknown>>,
    keys?: ReadonlyArray<string>,
): FastCheck.Arbitrary<unknown> {
    return Schema.isSchema(input)
        ? _absent(Arbitrary.make(Schema.encodedBoundSchema(input)), _optionalKeys(input))
        : input instanceof FastCheck.Arbitrary
          ? _absent(
                input,
                Array.map(keys ?? [], (name) => ({ name, undefinable: false })),
            )
          : FastCheck.record(input, { requiredKeys: [...(keys ?? [])] });
}

// The distinct-payload lane: a subject transporting several inputs never receives equal placeholders that hide swapped arguments.
const distinct = <A>(
    base: FastCheck.Arbitrary<A>,
    count: number,
    equals: (self: A, that: A) => boolean = Equal.equals,
): FastCheck.Arbitrary<ReadonlyArray<A>> => FastCheck.uniqueArray(base, { minLength: count, maxLength: count, comparator: equals });

// Distribution gauge over a generator: samples the arbitrary and returns every expected label the corpus never produced —
// a lying or over-biased arbitrary is refuted by its own emptiness, never trusted on shape alone.
const coverage = <A, Label extends string>(
    arb: FastCheck.Arbitrary<A>,
    classify: (value: A) => Label | ReadonlyArray<Label>,
    labels: ReadonlyArray<Label>,
    sampling: Arbitrate.Sampling = _SAMPLING,
): ReadonlyArray<Label> => {
    const seen = HashSet.fromIterable(
        Array.flatMap(FastCheck.sample(arb, { numRuns: sampling.numRuns ?? _SAMPLING.numRuns, seed: sampling.seed ?? _SAMPLING.seed }), (value) => {
            const hit = classify(value);
            return typeof hit === 'string' ? [hit] : hit;
        }),
    );
    return Array.filter(labels, (label) => !HashSet.has(seen, label));
};

const Arbitrate = { absence, coverage, distinct } as const;

// --- [EXPORTS] ---------------------------------------------------------------------------

export { Arbitrate };
