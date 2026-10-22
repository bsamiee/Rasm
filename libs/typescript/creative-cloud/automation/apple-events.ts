import { Effect, Schema, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type AppleEventValue = null | boolean | number | string | readonly AppleEventValue[] | { readonly [term: string]: AppleEventValue };

// --- [MODELS] --------------------------------------------------------------------------

const AppleEventValue: Schema.Codec<AppleEventValue> = Schema.Union([
    Schema.Null,
    Schema.Boolean,
    Schema.Finite,
    Schema.String.annotate({ description: 'Text, or an enumerator, class, date, or file URL where the parameter type takes one' }),
    Schema.Array(Schema.suspend(() => AppleEventValue)),
    Schema.TaggedStruct('record', {
        type: Schema.String.annotate({ description: 'Four-character descriptor type' }),
        fields: Schema.Record(
            Schema.String,
            Schema.suspend(() => AppleEventValue),
        ).annotate({ description: 'Fields by property name or four-character keyword' }),
    }).annotate({ description: 'Record of a descriptor type other than a plain record' }),
    Schema.TaggedStruct('type', { name: Schema.String }).annotate({ description: 'Class or property by name or four-character code' }),
    Schema.TaggedStruct('enumerator', { name: Schema.String }).annotate({ description: 'Enumerator by name or four-character code' }),
    Schema.TaggedStruct('date', { date: Schema.String.annotate({ description: 'ISO 8601 date' }) }),
    Schema.TaggedStruct('file', { url: Schema.String }),
    Schema.TaggedStruct('specifier', {
        want: Schema.String.annotate({ description: 'Class of the objects specified' }),
        form: Schema.Literals(['property', 'index', 'name', 'id', 'relative', 'range', 'test', 'whose']),
        seld: Schema.suspend(() => AppleEventValue).annotate({ description: 'Key data the form selects by, index form taking first, middle, last, any, or every' }),
        from: Schema.optionalKey(Schema.suspend(() => AppleEventValue).annotate({ description: 'Container specifier, null or absent for the application' })),
    }).annotate({ description: 'Object specifier' }),
    Schema.TaggedStruct('opaque', { type: Schema.String.annotate({ description: 'Four-character descriptor type' }), data: Schema.String.annotate({ description: 'Descriptor bytes in base64' }), value: Schema.optionalKey(Schema.String) }).annotate({
        description: 'Descriptor no other form holds exactly, passed back unchanged',
    }),
    Schema.Record(
        Schema.String,
        Schema.suspend(() => AppleEventValue),
    ).annotate({ description: 'Plain record by property name or four-character keyword' }),
]);
const Kind = Schema.Literals(['suite', 'command', 'class', 'record-type', 'value-type', 'enumeration']).annotate({ description: 'Scripting definition element kind' });
const Terms = Schema.Struct(Struct.Record(Kind.literals, Schema.Array(Schema.Struct({ name: Schema.String, code: Schema.String.annotate({ description: 'Code as the scripting definition writes it' }) })))).annotate({ description: 'Every declaration of the scripting dictionary by kind, name, and code' });
class Execute extends Schema.Class<Execute, { readonly brand: unique symbol }>('Execute')({
    command: Schema.String.annotate({ description: 'Command name or code from the application dictionary, get and set included' }),
    arguments: Schema.Record(Schema.String, AppleEventValue)
        .annotate({ description: 'Parameters by name or code, direct for the direct parameter, reply values reused unchanged' })
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    artifacts: Schema.Array(Schema.URLFromString)
        .annotate({ description: 'File URLs the command writes, returned with the reply' })
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
}) {
    static readonly Result = Schema.Struct({
        reply: AppleEventValue,
        artifacts: Schema.Array(Schema.Result(Schema.Struct({ uri: Schema.String, mimeType: Schema.OptionFromOptionalKey(Schema.String), data: Schema.Uint8ArrayFromBase64 }), Schema.ErrorInstance())).annotate({
            description: 'Each artifact file with its media type, or why reading it failed',
        }),
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { AppleEventValue, Execute, Kind, Terms };
