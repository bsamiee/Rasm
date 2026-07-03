import { Array, Equal, Option, pipe, Schema, SchemaAST } from 'effect';
import * as Arbitrary from 'effect/Arbitrary';
import * as FastCheck from 'effect/FastCheck';

// --- [OPERATIONS] ------------------------------------------------------------------------

const _optionalKeys = (schema: Schema.Schema.Any): ReadonlyArray<string> =>
    pipe(
        SchemaAST.getPropertySignatures(Schema.encodedBoundSchema(schema).ast),
        Array.filterMap((signature) => (signature.isOptional && typeof signature.name === 'string' ? Option.some(signature.name) : Option.none())),
    );

const _dropped = <A>(value: A, keys: ReadonlyArray<string>): A => {
    // BOUNDARY ADAPTER: optional-key deletion over an encoded record; removing optional keys preserves the encoded type.
    const draft: Record<string, unknown> = { ...(value as Record<string, unknown>) };
    for (const key of keys) {
        delete draft[key];
    }
    return draft as A;
};

const _absent = <A>(base: FastCheck.Arbitrary<A>, keys: ReadonlyArray<string>): FastCheck.Arbitrary<A> =>
    base.chain((value) => FastCheck.subarray([...keys]).map((dropped) => _dropped(value, dropped)));

// The field-absence lane: encoded-side generation that varies optional-key presence, attacking every wire absence dialect at the decode seam.
function absence<S extends Schema.Schema.Any>(schema: S): FastCheck.Arbitrary<Schema.Schema.Encoded<S>>;
function absence<A>(arb: FastCheck.Arbitrary<A>, keys: ReadonlyArray<string>): FastCheck.Arbitrary<A>;
function absence(input: Schema.Schema.Any | FastCheck.Arbitrary<unknown>, keys?: ReadonlyArray<string>): FastCheck.Arbitrary<unknown> {
    return Schema.isSchema(input) ? _absent(Arbitrary.make(Schema.encodedBoundSchema(input)), _optionalKeys(input)) : _absent(input, keys ?? []);
}

// The distinct-payload lane: a subject transporting several inputs never receives equal placeholders that hide swapped arguments.
const distinct = <A>(
    base: FastCheck.Arbitrary<A>,
    count: number,
    equals: (self: A, that: A) => boolean = Equal.equals,
): FastCheck.Arbitrary<ReadonlyArray<A>> => FastCheck.uniqueArray(base, { minLength: count, maxLength: count, comparator: equals });

const Arbitrate = { absence, distinct } as const;

// --- [EXPORTS] ---------------------------------------------------------------------------

export { Arbitrate };
