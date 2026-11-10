import { Predicate, type Record, Schema } from 'effect';
import { Specifier } from '#/specifier';

// --- [TYPES] ---------------------------------------------------------------------------

type Value<Leaf = typeof Enumerated.Type | typeof Located.Type> = Leaf | string | number | boolean | null | readonly Value<Leaf>[] | { readonly [field: string]: Value<Leaf> };

// --- [MODELS] --------------------------------------------------------------------------

const Enumerated = Schema.Struct({ type: Schema.String, member: Schema.String });
const Located = Schema.Struct({ specifier: Specifier.Path });
const Nested = Schema.suspend((): typeof Codec => Codec);
const Codec: Schema.Codec<Value, Value<typeof Enumerated.Encoded | typeof Located.Encoded>> = Schema.Union([Enumerated, Located, Schema.String, Schema.Finite, Schema.Boolean, Schema.Null, Schema.Array(Nested), Schema.Record(Schema.String, Nested)]).annotate({
    description: 'Enumerator or object name, {type, member} enumerator, {specifier} object, points or unit string measurement, record of nested preferences',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const Value = Object.assign(Codec, {
    Fields: Schema.Record(Schema.String, Codec),
    Enumerated,
    Located,
    plainObject: (value: unknown): value is Record.ReadonlyRecord<string, unknown> => Predicate.isObject(value) && Object.getPrototypeOf(value) === Object.prototype,
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Value };
