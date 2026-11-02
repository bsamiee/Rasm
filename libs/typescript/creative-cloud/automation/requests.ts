import { Unmatched } from '@rasm/search/unmatched';
import { Array, Context, Option, Schema, String, Struct } from 'effect';
import { Rpc, RpcGroup } from 'effect/rpc';
import { LongestSide } from '#/applications';

// --- [MODELS] --------------------------------------------------------------------------

const Member = Schema.Struct({
    name: Schema.String,
    type: Schema.OptionFromOptionalKey(Schema.String),
    types: Schema.Array(Schema.String),
    description: Schema.OptionFromOptionalKey(Schema.String),
    array: Schema.OptionFromOptionalKey(Schema.Literal(true)),
    ...Struct.Record(['minimum', 'maximum'], Schema.OptionFromOptionalKey(Schema.Finite)),
    members: Schema.OptionFromOptionalKey(Schema.NonEmptyArray(Schema.String)),
    unavailable: Schema.OptionFromOptionalKey(Schema.String),
}).annotate({ identifier: 'Member' });
const Field = Member.pipe(Schema.fieldsAssign({ settable: Schema.Boolean })).annotate({ identifier: 'Field' });
const Method = Member.mapFields(Struct.pick(['name', 'description']))
    .pipe(
        Schema.fieldsAssign({
            parameters: Schema.Array(Member.mapFields(Struct.omit(['unavailable'])).pipe(Schema.fieldsAssign({ optional: Schema.Boolean }))),
            returns: Schema.OptionFromOptionalKey(Member.mapFields(Struct.pick(['type', 'array']))),
        }),
    )
    .annotate({ identifier: 'Method' });
class Reference extends Schema.Class<Reference, { readonly brand: unique symbol }>('Reference')({ type: Schema.String, specifier: Schema.String, name: Schema.OptionFromOptionalKey(Schema.String), valid: Schema.Boolean }) {
    static readonly Kind = Schema.String.check(Schema.isPattern(/^@?[a-z][\w-]*$/u));
    static readonly Identified = Schema.TemplateLiteral(['[@id=', Schema.Int, ']']);
    static readonly Indexed = Schema.TemplateLiteral(['[', Schema.Int, ']']);
    static readonly Named = Schema.TemplateLiteral(['[@name="', Schema.String, '"]']);
}
class ExecuteScript extends Schema.Class<ExecuteScript, { readonly brand: unique symbol }>('ExecuteScript')({ code: Schema.String.annotate({ description: 'Function expression the host evaluates and calls' }) }) {
    static readonly Result = Schema.Struct({ result: Schema.OptionFromOptionalKey(Schema.Json) });
    static readonly Awaits = Context.Reference<boolean>('@rasm/automation/requests/ExecuteScript/Awaits', { defaultValue: () => false });

    get label(): string {
        const [, ...body] = this.code.split('\n');
        return `Run ${Option.getOrElse(Array.findFirst(Array.map(body, String.trim), String.isNonEmpty), () => 'execute_script')}`;
    }
}
class Open extends Schema.Class<Open, { readonly brand: unique symbol }>('Open')({ file: Schema.URLFromString }) {
    get label(): string {
        return `open ${this.file.href}`;
    }
}
class Render extends Schema.Class<Render, { readonly brand: unique symbol }>('Render')({
    target: Schema.String.annotate({ description: 'Document, page, spread, artboard, layer, composition, or page item specifier' }),
    longestSide: LongestSide,
    options: Schema.OptionFromOptionalKey(Schema.Record(Schema.String, Schema.Json).annotate({ description: 'PNG export option fields' })),
    file: Schema.URLFromString,
}) {
    get label(): string {
        return `render of ${this.target}`;
    }
}
class ObjectModel extends Schema.Class<ObjectModel, { readonly brand: unique symbol }>('ObjectModel')({ name: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Name in any case, absent lists every name' })) }) {
    static readonly Result = Schema.TaggedUnion({
        Index: { classes: Schema.Array(Schema.String), enumerations: Schema.Array(Schema.String) },
        Class: { name: Schema.String, parents: Schema.Array(Schema.String).annotate({ description: 'Superclasses, nearest first' }), fields: Schema.Array(Field), methods: Schema.Array(Method) },
        Enumeration: { name: Schema.String, members: Schema.Array(Member.mapFields(Struct.pick(['name', 'description']))) },
    });
}

// --- [ERRORS] --------------------------------------------------------------------------

class HostError extends Schema.TaggedError<HostError, { readonly brand: unique symbol }>()('HostError', {
    reason: Schema.TaggedUnion({ Refused: {}, Unavailable: { because: Schema.String }, Thrown: { line: Schema.Int.annotate({ description: 'Body line that threw' }) } }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
}) {
    static readonly refused = (subject: string) => (cause: unknown) => HostError.make({ reason: { _tag: 'Refused' }, subject, cause });
    static readonly thrown = (subject: string, line: Option.Option<number>) => (cause: unknown) => HostError.make({ reason: Option.match(line, { onNone: () => ({ _tag: 'Refused' }) as const, onSome: (at) => ({ _tag: 'Thrown', line: at }) as const }), subject, cause });

    override get message(): string {
        return HostError.fields.reason.match(this.reason, { Refused: () => this.subject, Unavailable: ({ because }) => `${this.subject}: ${because}`, Thrown: ({ line }) => `line ${line}: ${this.subject}` });
    }
}
const error = Schema.Union([Unmatched, HostError, Schema.ErrorInstance()]);

// --- [COMPOSITION] ---------------------------------------------------------------------

const Requests = RpcGroup.make(
    Rpc.make('execute_script', { payload: ExecuteScript, success: ExecuteScript.Result, error }),
    Rpc.make('open', { payload: Open, success: Reference, error }),
    Rpc.make('render', { payload: Render, error }),
    Rpc.make('object_model', { payload: ObjectModel, success: ObjectModel.Result, error }),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { ExecuteScript, error, Field, HostError, Method, ObjectModel, Open, Reference, Render, Requests };
