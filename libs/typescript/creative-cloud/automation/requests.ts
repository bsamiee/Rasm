import { LongestSide } from '@rasm/applications/contract';
import { Unmatched } from '@rasm/search/error';
import { Array, Cause, Context, Effect, Option, Schema, SchemaGetter, SchemaIssue, SchemaTransformation, String, Struct } from 'effect';
import { Url } from 'effect/http';
import { Rpc, RpcGroup } from 'effect/rpc';
import { HostError } from '#/error';

// --- [TYPES] ---------------------------------------------------------------------------

type Requests = RpcGroup.Rpcs<typeof Requests>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const EXECUTE_SCRIPT = 'execute_script';
const BODY = 'function body() {';

// --- [MODELS] --------------------------------------------------------------------------

const Version = Schema.String.check(Schema.isPattern(/^\d+(?:\.\d+){0,2}$/u)).annotate({ description: 'major[.minor[.patch]]' });
const HostUrl = Schema.String.pipe(
    Schema.decodeTo(Schema.URL, {
        decode: SchemaGetter.transformEffect((text, options) => Effect.fromResult(Url.fromString(text)).pipe(Effect.mapError(() => new SchemaIssue.InvalidValue({ expected: 'a valid URL string' }, text, options)))),
        encode: SchemaGetter.transform(({ href }) => href),
    }),
);
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
class ExecuteScript extends Schema.Class<ExecuteScript, { readonly brand: unique symbol }>('ExecuteScript')({ code: Schema.String.annotate({ description: 'Function expression the host evaluates and calls' }) }) {
    static readonly Result = Schema.Struct({ result: Schema.OptionFromOptionalKey(Schema.Json) });
    static readonly Awaits = Context.Reference<boolean>('@rasm/automation/requests/ExecuteScript/Awaits', { defaultValue: () => false });
    static readonly expression = (body: string, awaits: boolean) => `(${awaits ? 'async ' : ''}${BODY}\n${body}\n})`;

    get label(): string {
        const [, ...body] = this.code.split(BODY);
        return `Run ${Option.getOrElse(Array.findFirst(Array.map(body.join(BODY).split('\n'), String.trim), String.isNonEmpty), () => EXECUTE_SCRIPT)}`;
    }
}
class Open extends Schema.Class<Open, { readonly brand: unique symbol }>('Open')({ file: HostUrl }) {
    static readonly Result = Reference;

    get label(): string {
        return `Open ${basename(this.file)}`;
    }
}
class Render extends Schema.Class<Render, { readonly brand: unique symbol }>('Render')({
    target: Schema.String.annotate({ description: 'Document, page, spread, artboard, layer, composition, or page item specifier' }),
    longestSide: LongestSide,
    options: Schema.OptionFromOptionalKey(Schema.Record(Schema.String, Schema.Json).annotate({ description: 'Option fields of the host exporter the target selects' })),
    file: HostUrl,
}) {
    static readonly Result = Schema.Void;

    get label(): string {
        return `Render ${this.target} to ${basename(this.file)}`;
    }
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

// --- [ERRORS] --------------------------------------------------------------------------

const IllegalArgument = Schema.declare(Cause.isIllegalArgumentError, {
    expected: 'IllegalArgumentError',
    toCodecJson: () => Schema.link<Cause.IllegalArgumentError>()(Schema.TaggedStruct('IllegalArgumentError', { message: Schema.String }), SchemaTransformation.transform({ decode: ({ message }) => new Cause.IllegalArgumentError(message), encode: ({ message }) => ({ _tag: 'IllegalArgumentError' as const, message }) })),
});
const Failure = Schema.Union([Unmatched, HostError, IllegalArgument, Schema.ErrorInstance()]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const basename = (file: Open['file']) => decodeURIComponent(file.pathname.slice(file.pathname.lastIndexOf('/') + 1));
const request = <const Tag extends string, Payload extends Schema.Top & { readonly Result: Schema.Top }>(tag: Tag, payload: Payload) => Rpc.make<Tag, Payload, Payload['Result'], typeof Failure>(tag, { payload, success: payload.Result, error: Failure });

// --- [COMPOSITION] ---------------------------------------------------------------------

const Requests = RpcGroup.make(request(EXECUTE_SCRIPT, ExecuteScript), request('open', Open), request('render', Render), request('object_model', ObjectModel));

// --- [EXPORTS] -------------------------------------------------------------------------

export { ExecuteScript, HostUrl, ObjectModel, Open, Reference, Render, Requests, request, Version };
