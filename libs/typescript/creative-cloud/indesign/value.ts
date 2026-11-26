import { Specifier } from '@rasm/object-model/model';
import { Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Value<Leaf = Enumerated | Located> = Leaf | string | number | boolean | null | readonly Value<Leaf>[] | { readonly [field: string]: Value<Leaf> };

// --- [MODELS] --------------------------------------------------------------------------

class Enumerated extends Schema.Opaque<Enumerated>()(Schema.Struct({ type: Schema.String, member: Schema.String })) {}
class Located extends Schema.Opaque<Located>()(Schema.Struct({ specifier: Specifier.FromString })) {}
const Nested = Schema.suspend(() => Codec);
const Codec: Schema.Codec<Value, Value<Enumerated | { readonly specifier: string }>> = Schema.Union([Enumerated, Located, Schema.String, Schema.Finite, Schema.Boolean, Schema.Null, Schema.Array(Nested), Schema.Record(Schema.String, Nested)]).annotate({
    description: 'Enumerator or object name, {type, member} enumerator, {specifier} object, points or unit string measurement, record of nested preferences',
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const Value = Object.assign(Codec, { Fields: Schema.Record(Schema.String, Codec), Enumerated, Located });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Enumerated, Located, Value };
