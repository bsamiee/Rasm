import { Predicate, Record, Schema, Struct } from 'effect';
import { Specifier } from '#/dom/specifier';

// --- [TYPES] ---------------------------------------------------------------------------

type Value = typeof Enumerated.Type | typeof Located.Type | string | number | boolean | null | readonly Value[] | Fields;
type Encoded = typeof Enumerated.Encoded | typeof Located.Encoded | string | number | boolean | null | readonly Encoded[] | { readonly [field: string]: Encoded };
interface Fields {
    readonly [field: string]: Value;
}

// --- [MODELS] --------------------------------------------------------------------------

const Enumerated = Schema.Struct({ type: Schema.String, member: Schema.String });
const Located = Schema.Struct({ specifier: Specifier.Path });
const Nested = Schema.suspend((): Schema.Codec<Value, Encoded> => Codec);
const Codec: Schema.Codec<Value, Encoded> = Schema.Union([Enumerated, Located, Schema.String, Schema.Finite, Schema.Boolean, Schema.Null, Schema.Array(Nested), Schema.Record(Schema.String, Nested)]).annotate({
    description: 'Enumerator or object name, {type, member} enumerator, {specifier} object, points or unit string measurement, record of nested preferences',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const Value = Object.assign(Codec, {
    Fields: Schema.Record(Schema.String, Codec),
    Enumerated,
    Located,
    plainObject: (value: unknown): value is Record.ReadonlyRecord<string, unknown> => Predicate.isObject(value) && Object.getPrototypeOf(value) === Object.prototype,
    picked:
        <F extends Schema.Struct.Fields>(fields: F) =>
        (object: object): Record.ReadonlyRecord<string, unknown> =>
            Record.fromIterableWith(
                Struct.keys(fields).filter((key) => key in object),
                (key) => [key, Reflect.get(object, key)],
            ),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Value };
