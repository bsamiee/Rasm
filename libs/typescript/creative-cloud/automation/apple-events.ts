import { Unmatched } from '@rasm/search/unmatched';
import { Array, Effect, Option, Schema, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type AppleEventValue =
    | null
    | boolean
    | number
    | string
    | readonly AppleEventValue[]
    | { readonly _tag: 'record'; readonly type: string; readonly fields: { readonly [term: string]: AppleEventValue } }
    | { readonly _tag: 'specifier'; readonly want: string; readonly form: typeof Form.Type; readonly seld: AppleEventValue; readonly from?: AppleEventValue }
    | typeof Leaf.Type
    | { readonly _tag?: never; readonly [term: string]: AppleEventValue };

// --- [CONSTANTS] -----------------------------------------------------------------------

const LANGUAGE = 'doLg';

// --- [MODELS] --------------------------------------------------------------------------

const Nested = Schema.suspend((): Schema.Codec<AppleEventValue> => AppleEventValue);
const Code = Schema.String.annotate({ description: 'Four-character descriptor type' });
const Form = Schema.Literals(['property', 'index', 'name', 'id', 'relative', 'range', 'test', 'whose']);
const Leaf = Schema.Union([
    Schema.TaggedStruct('type', { name: Schema.String }).annotate({ description: 'Class or property by name or code' }),
    Schema.TaggedStruct('enumerator', { name: Schema.String }).annotate({ description: 'Enumerator by name or code' }),
    Schema.TaggedStruct('date', { date: Schema.String.annotate({ description: 'ISO 8601 date' }) }),
    Schema.TaggedStruct('file', { url: Schema.String }),
    Schema.TaggedStruct('opaque', { type: Code, data: Schema.String.annotate({ description: 'Descriptor bytes in base64' }), value: Schema.optionalKey(Schema.String) }).annotate({ description: 'Descriptor no other case decodes, sent back unchanged' }),
]);
const AppleEventValue: Schema.Codec<AppleEventValue> = Schema.Union([
    Schema.Null,
    Schema.Boolean,
    Schema.Finite,
    Schema.String.annotate({ description: 'Text, or enumerator, class, date, or file URL per parameter type' }),
    Schema.Array(Nested),
    Schema.TaggedStruct('record', { type: Code, fields: Schema.Record(Schema.String, Nested).annotate({ description: 'Values by property name or code' }) }).annotate({ description: 'Record with custom descriptor type' }),
    Schema.TaggedStruct('specifier', {
        want: Schema.String.annotate({ description: 'Specified object class' }),
        form: Form,
        seld: Nested.annotate({ description: 'Selection data, or first, middle, last, any, or every for index form' }),
        from: Schema.optionalKey(Nested.annotate({ description: 'Container specifier, null or absent for application' })),
    }).annotate({ description: 'Object specifier' }),
    Leaf,
    Schema.StructWithRest(Schema.Struct({ _tag: Schema.optionalKey(Schema.Never) }), [Schema.Record(Schema.String, Nested)]).annotate({ description: 'Plain record by property name or code' }),
]);
const Kind = Schema.Literals(['suite', 'command', 'class', 'record-type', 'value-type', 'enumeration']).annotate({ description: 'Scripting definition element kind' });
const Term = Schema.Struct({ name: Schema.String, code: Schema.String.annotate({ description: 'Code as scripting definition spells it' }) });
class Execute extends Schema.Class<Execute, { readonly brand: unique symbol }>('Execute')({
    command: Schema.String.annotate({ description: 'Dictionary command name or code, get and set included' }),
    arguments: Schema.Record(Schema.String, AppleEventValue)
        .annotate({ description: 'Parameters by name or code, direct for direct parameter, accepts reply values' })
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed({})), Schema.withConstructorDefault(Effect.succeed({}))),
    artifacts: Schema.Array(Schema.URLFromString)
        .annotate({ description: 'Files command writes, returned with reply' })
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed([])), Schema.withConstructorDefault(Effect.succeed([]))),
}) {
    static readonly Artifact = Schema.Struct({ uri: Schema.String, mimeType: Schema.OptionFromOptionalKey(Schema.String), data: Schema.Uint8Array });
    static readonly Result = Schema.Struct({
        reply: AppleEventValue,
        artifacts: Schema.Array(Schema.Result(Execute.Artifact, Schema.ErrorInstance())).annotate({ description: 'File contents with media type, or read error' }),
    });
}
class Terms extends Schema.Class<Terms, { readonly brand: unique symbol }>('Terms')(
    { ...Struct.Record(Kind.literals, Schema.Array(Term)), command: Schema.Array(Term.mapFields(Struct.assign({ parameters: Schema.Array(Term) }))) },
    { description: 'Every declaration by kind, name, and code, commands with their parameters' },
) {
    static readonly Kind = Kind;

    static readonly script = Effect.fnUntraced(function* ({ command }: Terms, { direct, values = [], language = 'JSLg' }: { readonly direct: AppleEventValue; readonly values?: readonly AppleEventValue[]; readonly language?: string }) {
        const { name, parameters } = yield* Effect.fromOption(
            Array.findFirst(['DjxM', 'dosc'], (event) => Array.findFirst(command, ({ code }) => code.endsWith(event))),
            () => Unmatched.make({ subject: 'script command', candidates: Array.map(command, Struct.get('name')) }),
        );
        const passed = Array.isReadonlyArrayEmpty(values) ? {} : { [(yield* Unmatched.single(parameters, `parameter of ${name}`, Option.some('with arguments'), (parameter) => [parameter.name, parameter.code])).code]: values };
        const spoken = Array.some(parameters, ({ code }) => code === LANGUAGE) ? { [LANGUAGE]: { _tag: 'enumerator', name: language } as const } : {};
        return Execute.make({ command: name, arguments: { direct, ...passed, ...spoken } });
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { AppleEventValue, Execute, Terms };
