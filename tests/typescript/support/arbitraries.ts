import { Array, Equal, HashSet, Option, pipe, Schema, SchemaAST } from 'effect';
import * as Arbitrary from 'effect/Arbitrary';
import * as FastCheck from 'effect/FastCheck';

// --- [TYPES] ---------------------------------------------------------------------------

declare namespace Arbitraries {
    type Model<T> = { readonly [K in keyof T]: FastCheck.Arbitrary<T[K]> };
    type Sampling = { readonly numRuns?: number; readonly seed?: number };
}

type _Key = { readonly name: string; readonly undefinable: boolean };

// --- [CONSTANTS] -----------------------------------------------------------------------

const _SAMPLING = { numRuns: 256, seed: 0 } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const _optionalKeys = (schema: Schema.Schema.Any): ReadonlyArray<_Key> =>
    pipe(
        SchemaAST.getPropertySignatures(Schema.encodedBoundSchema(schema).ast),
        Array.filterMap((signature) =>
            signature.isOptional && typeof signature.name === 'string'
                ? Option.some<_Key>({
                      name: signature.name,
                      undefinable: SchemaAST.isUnion(signature.type)
                          ? Array.some(
                                signature.type.types,
                                SchemaAST.isUndefinedKeyword,
                            )
                          : SchemaAST.isUndefinedKeyword(signature.type),
                  })
                : Option.none(),
        ),
    );

const _withOptionalFields = <A>(
    value: A,
    dropped: ReadonlyArray<string>,
    unset: ReadonlyArray<string>,
): A => {
    const draft: Record<string, unknown> = {
        ...(value as Record<string, unknown>),
    };
    for (const key of unset) {
        draft[key] = undefined;
    }
    for (const key of dropped) {
        delete draft[key];
    }
    return draft as A;
};

const _varyOptionalFields = <A>(
    base: FastCheck.Arbitrary<A>,
    keys: ReadonlyArray<_Key>,
): FastCheck.Arbitrary<A> =>
    base.chain((value) =>
        FastCheck.tuple(
            FastCheck.subarray(Array.map(keys, (key) => key.name)),
            FastCheck.subarray(
                Array.filterMap(keys, (key) =>
                    key.undefinable ? Option.some(key.name) : Option.none(),
                ),
            ),
        ).map(([dropped, unset]) => _withOptionalFields(value, dropped, unset)),
    );

function optionalFields<S extends Schema.Schema.Any>(
    schema: S,
): FastCheck.Arbitrary<Schema.Schema.Encoded<S>>;
function optionalFields<A>(
    arb: FastCheck.Arbitrary<A>,
    keys: ReadonlyArray<string>,
): FastCheck.Arbitrary<A>;
function optionalFields<T>(
    model: Arbitraries.Model<T>,
    required?: ReadonlyArray<keyof T & string>,
): FastCheck.Arbitrary<Partial<T>>;
function optionalFields(
    input:
        | Schema.Schema.Any
        | FastCheck.Arbitrary<unknown>
        | Arbitraries.Model<Record<string, unknown>>,
    keys?: ReadonlyArray<string>,
): FastCheck.Arbitrary<unknown> {
    return Schema.isSchema(input)
        ? _varyOptionalFields(
              Arbitrary.make(Schema.encodedBoundSchema(input)),
              _optionalKeys(input),
          )
        : input instanceof FastCheck.Arbitrary
          ? _varyOptionalFields(
                input,
                Array.map(keys ?? [], (name) => ({ name, undefinable: false })),
            )
          : FastCheck.record(input, { requiredKeys: [...(keys ?? [])] });
}

const distinctArray = <A>(
    base: FastCheck.Arbitrary<A>,
    count: number,
    equals: (self: A, that: A) => boolean = Equal.equals,
): FastCheck.Arbitrary<ReadonlyArray<A>> =>
    FastCheck.uniqueArray(base, {
        minLength: count,
        maxLength: count,
        comparator: equals,
    });

const missingClassifications = <A, Label extends string>(
    arbitrary: FastCheck.Arbitrary<A>,
    classify: (value: A) => Label | ReadonlyArray<Label>,
    labels: ReadonlyArray<Label>,
    sampling: Arbitraries.Sampling = _SAMPLING,
): ReadonlyArray<Label> => {
    const seen = HashSet.fromIterable(
        Array.flatMap(
            FastCheck.sample(arbitrary, {
                numRuns: sampling.numRuns ?? _SAMPLING.numRuns,
                seed: sampling.seed ?? _SAMPLING.seed,
            }),
            (value) => {
                const hit = classify(value);
                return typeof hit === 'string' ? [hit] : hit;
            },
        ),
    );
    return Array.filter(labels, (label) => !HashSet.has(seen, label));
};

const Arbitraries = {
    distinctArray,
    missingClassifications,
    optionalFields,
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Arbitraries };
