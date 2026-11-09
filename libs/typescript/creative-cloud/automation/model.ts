import { Unmatched } from '@rasm/search';
import { Array, Effect, Option, Schema, SchemaTransformation, Struct } from 'effect';

// --- [MODELS] --------------------------------------------------------------------------

const Member = Schema.Struct({
    name: Schema.String,
    type: Schema.OptionFromOptionalKey(Schema.String),
    types: Schema.OptionFromOptionalKey(Schema.Array(Schema.String)),
    description: Schema.OptionFromOptionalKey(Schema.String),
    array: Schema.Boolean.pipe(Schema.withDecodingDefaultKey(Effect.succeed(false))),
    ...Struct.Record(['minimum', 'maximum'], Schema.OptionFromOptionalKey(Schema.Finite)),
    members: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.String)),
}).annotate({ identifier: 'Member' });
const Described = Member.mapFields(Struct.pick(['name', 'description']));
const SelectorName = Schema.String.check(Schema.isPattern(/^(?![\s\S]*"\])/u)).annotate({ description: 'Name holding no closing quote and bracket' });

class Reference extends Schema.Class<Reference, { readonly brand: unique symbol }>('Reference')({ type: Schema.String, specifier: Schema.String, name: Schema.OptionFromOptionalKey(Schema.String), valid: Schema.Boolean }) {
    static readonly Selector = Schema.Union([
        Schema.TemplateLiteralParser(['[@id=', Schema.Int, ']']).pipe(Schema.decodeTo(Schema.TaggedStruct('Identified', { id: Schema.Int }), SchemaTransformation.transform({ decode: ([, id]) => ({ _tag: 'Identified', id }) as const, encode: ({ id }) => ['[@id=', id, ']'] as const }))),
        Schema.TemplateLiteralParser(['[', Schema.Int, ']']).pipe(Schema.decodeTo(Schema.TaggedStruct('Indexed', { index: Schema.Int }), SchemaTransformation.transform({ decode: ([, index]) => ({ _tag: 'Indexed', index }) as const, encode: ({ index }) => ['[', index, ']'] as const }))),
        Schema.TemplateLiteralParser(['[@name="', SelectorName, '"]']).pipe(Schema.decodeTo(Schema.TaggedStruct('Named', { name: SelectorName }), SchemaTransformation.transform({ decode: ([, name]) => ({ _tag: 'Named', name }) as const, encode: ({ name }) => ['[@name="', name, '"]'] as const }))),
    ])
        .annotate({ description: 'Specifier step selector by id, index, or name' })
        .pipe(Schema.toTaggedUnion('_tag'));
    static readonly Written = Reference.mapFields(Struct.assign({ written: Schema.Array(Schema.String).annotate({ description: 'Member names' }) }));
}
class ObjectModel extends Schema.Class<ObjectModel, { readonly brand: unique symbol }>('ObjectModel')({ name: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Name in any case, absent lists every name' })) }) {
    static readonly Field = Member.pipe(Schema.fieldsAssign({ settable: Schema.Boolean, unavailable: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Reason reading or writing crashes the host' })) })).annotate({ identifier: 'Field' });
    static readonly Method = Described.pipe(
        Schema.fieldsAssign({
            parameters: Schema.Array(Member.pipe(Schema.fieldsAssign({ optional: Schema.Boolean, default: Schema.optionalKey(Schema.Json.annotate({ description: 'Value an omitted argument takes' })) }))),
            returns: Schema.OptionFromOptionalKey(Schema.Struct({ type: Schema.String, array: Member.fields.array })),
        }),
    ).annotate({ identifier: 'Method' });
    static readonly Result = Schema.TaggedUnion({
        Index: { classes: Schema.Array(Schema.String), enumerations: Schema.Array(Schema.String) },
        Class: { name: Schema.String, parents: Schema.Array(Schema.String).annotate({ description: 'Superclasses, nearest first' }), fields: Schema.Array(ObjectModel.Field), methods: Schema.Array(ObjectModel.Method) },
        Enumeration: { name: Schema.String, members: Schema.Array(Described) },
    });

    get label(): string {
        return Option.match(this.name, { onNone: () => 'List classes and enumerations', onSome: (name) => `Describe ${name}` });
    }

    describe<E, R>(classes: readonly string[], enumerations: readonly string[], declared: (name: string) => Effect.Effect<typeof ObjectModel.Result.Type, E, R>): Effect.Effect<typeof ObjectModel.Result.Type, E | Unmatched, R> {
        return Option.match(this.name, {
            onNone: () => Effect.succeed(ObjectModel.Result.cases.Index.make({ classes, enumerations })),
            onSome: (query) => Effect.flatMap(Unmatched.single([...classes, ...enumerations], 'class or enumeration', Option.some(query), Array.of), declared),
        });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { ObjectModel, Reference };
