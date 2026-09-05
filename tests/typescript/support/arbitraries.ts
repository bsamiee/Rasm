import { Arbitrary, Array, Equal, FastCheck, HashSet, Match, Option, Predicate, pipe, Record, Schema, SchemaAST } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type ArbitraryModel<T> = { readonly [K in keyof T]: FastCheck.Arbitrary<T[K]> };

interface Sampling {
    readonly numRuns?: number;
    readonly seed?: number;
}

interface OptionalKey {
    readonly name: string;
    readonly undefinable: boolean;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _SAMPLING = { numRuns: 256, seed: 0 } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const _undefinable = (type: SchemaAST.AST): boolean =>
    Match.value(type).pipe(
        Match.when(SchemaAST.isUnion, (union) => Array.some(union.types, SchemaAST.isUndefinedKeyword)),
        Match.orElse(SchemaAST.isUndefinedKeyword),
    );

const _optionalKeys = (schema: Schema.Schema.Any): readonly OptionalKey[] =>
    Array.filterMap(SchemaAST.getPropertySignatures(Schema.encodedBoundSchema(schema).ast), (signature) =>
        Option.map(
            Option.liftPredicate(signature.name, (name): name is string => signature.isOptional && Predicate.isString(name)),
            (name) => ({ name, undefinable: _undefinable(signature.type) }),
        ),
    );

const _varyOptionalFields = <A>(base: FastCheck.Arbitrary<A>, keys: readonly OptionalKey[]): FastCheck.Arbitrary<A> =>
    base.chain((value) =>
        FastCheck.tuple(
            FastCheck.subarray(Array.map(keys, (key) => key.name)),
            FastCheck.subarray(Array.filterMap(keys, (key) => Option.liftPredicate(key.name, () => key.undefinable))),
        ).map(
            ([dropped, unset]) =>
                Record.filter(
                    { ...(value as Record<string, unknown>), ...Record.fromIterableWith(unset, (key) => [key, undefined]) },
                    (_, key) => !Array.contains(dropped, key),
                ) as A,
        ),
    );

function optionalFields<S extends Schema.Schema.Any>(schema: S): FastCheck.Arbitrary<Schema.Schema.Encoded<S>>;
function optionalFields<A>(arb: FastCheck.Arbitrary<A>, keys: readonly string[]): FastCheck.Arbitrary<A>;
function optionalFields<T>(model: ArbitraryModel<T>, required?: readonly (keyof T & string)[]): FastCheck.Arbitrary<Partial<T>>;
function optionalFields(
    input: Schema.Schema.Any | FastCheck.Arbitrary<unknown> | ArbitraryModel<Record<string, unknown>>,
    keys: readonly string[] = [],
): FastCheck.Arbitrary<unknown> {
    return Match.value(input).pipe(
        Match.when(Schema.isSchema, (schema) => _varyOptionalFields(Arbitrary.make(Schema.encodedBoundSchema(schema)), _optionalKeys(schema))),
        Match.when(Match.instanceOfUnsafe(FastCheck.Arbitrary), (arb) =>
            _varyOptionalFields(
                arb,
                Array.map(keys, (name) => ({ name, undefinable: false })),
            ),
        ),
        Match.orElse((model) => FastCheck.record(model, { requiredKeys: [...keys] })),
    );
}

const distinctArray = <A>(
    base: FastCheck.Arbitrary<A>,
    count: number,
    equals: (self: A, that: A) => boolean = Equal.equals,
): FastCheck.Arbitrary<readonly A[]> => FastCheck.uniqueArray(base, { minLength: count, maxLength: count, comparator: equals });

const missingClassifications = <A, Label extends string>(
    arbitrary: FastCheck.Arbitrary<A>,
    classify: (value: A) => Label | readonly Label[],
    labels: readonly Label[],
    sampling: Sampling = _SAMPLING,
): readonly Label[] => {
    const seen = pipe(
        FastCheck.sample(arbitrary, { ..._SAMPLING, ...sampling }),
        Array.flatMap((value) => Array.ensure(classify(value))),
        HashSet.fromIterable,
    );
    return Array.filter(labels, (label) => !HashSet.has(seen, label));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { type ArbitraryModel, distinctArray, missingClassifications, optionalFields, type Sampling };
