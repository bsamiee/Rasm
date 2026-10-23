import { Effect, Schema, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type AppleEventValue = null | boolean | number | string | readonly AppleEventValue[] | { readonly [term: string]: AppleEventValue };

// --- [MODELS] --------------------------------------------------------------------------

const AppleEventValue: Schema.Codec<AppleEventValue> = Schema.Union([
    Schema.Null,
    Schema.Boolean,
    Schema.Finite,
    Schema.String.annotate({ description: 'Text, or enumerator, class, date, or file URL per parameter type' }),
    Schema.Array(Schema.suspend(() => AppleEventValue)),
    Schema.TaggedStruct('record', {
        type: Schema.String.annotate({ description: 'Four-character descriptor type' }),
        fields: Schema.Record(
            Schema.String,
            Schema.suspend(() => AppleEventValue),
        ).annotate({ description: 'Values by property name or code' }),
    }).annotate({ description: 'Record with custom descriptor type' }),
    Schema.TaggedStruct('type', { name: Schema.String }).annotate({ description: 'Class or property by name or code' }),
    Schema.TaggedStruct('enumerator', { name: Schema.String }).annotate({ description: 'Enumerator by name or code' }),
    Schema.TaggedStruct('date', { date: Schema.String.annotate({ description: 'ISO 8601 date' }) }),
    Schema.TaggedStruct('file', { url: Schema.String }),
    Schema.TaggedStruct('specifier', {
        want: Schema.String.annotate({ description: 'Specified object class' }),
        form: Schema.Literals(['property', 'index', 'name', 'id', 'relative', 'range', 'test', 'whose']),
        seld: Schema.suspend(() => AppleEventValue).annotate({ description: 'Selection data, or first, middle, last, any, or every for index form' }),
        from: Schema.optionalKey(Schema.suspend(() => AppleEventValue).annotate({ description: 'Container specifier, null or absent for application' })),
    }).annotate({ description: 'Object specifier' }),
    Schema.TaggedStruct('opaque', { type: Schema.String.annotate({ description: 'Four-character descriptor type' }), data: Schema.String.annotate({ description: 'Descriptor bytes in base64' }), value: Schema.optionalKey(Schema.String) }).annotate({ description: 'Descriptor other forms alter, passed back unchanged' }),
    Schema.Record(
        Schema.String,
        Schema.suspend(() => AppleEventValue),
    ).annotate({ description: 'Plain record by property name or code' }),
]);
const Kind = Schema.Literals(['suite', 'command', 'class', 'record-type', 'value-type', 'enumeration']).annotate({ description: 'Scripting definition element kind' });
const Terms = Schema.Struct(Struct.Record(Kind.literals, Schema.Array(Schema.Struct({ name: Schema.String, code: Schema.String.annotate({ description: 'Code as scripting definition spells it' }) })))).annotate({ description: 'Every declaration by kind, name, and code' });
class Execute extends Schema.Class<Execute, { readonly brand: unique symbol }>('Execute')({
    command: Schema.String.annotate({ description: 'Dictionary command name or code, get and set included' }),
    arguments: Schema.Record(Schema.String, AppleEventValue)
        .annotate({ description: 'Parameters by name or code, direct for direct parameter, accepts reply values' })
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    artifacts: Schema.Array(Schema.URLFromString)
        .annotate({ description: 'Files command writes, returned with reply' })
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
}) {
    static readonly Result = Schema.Struct({
        reply: AppleEventValue,
        artifacts: Schema.Array(Schema.Result(Schema.Struct({ uri: Schema.String, mimeType: Schema.OptionFromOptionalKey(Schema.String), data: Schema.Uint8ArrayFromBase64 }), Schema.ErrorInstance())).annotate({ description: 'File contents with media type, or read error' }),
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { AppleEventValue, Execute, Kind, Terms };
