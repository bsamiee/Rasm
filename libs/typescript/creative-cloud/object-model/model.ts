import { Unmatched } from '@rasm/search';
import { Array, Effect, Formatter, Match, Option, Record, Schema, SchemaGetter, SchemaIssue, String, Struct } from 'effect';
import { format, parse } from '#/host';

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
const Field = Member.pipe(Schema.fieldsAssign({ settable: Schema.Boolean, unavailable: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Reason reading or writing crashes the host' })) })).annotate({ identifier: 'Field' });
const Method = Described.pipe(
    Schema.fieldsAssign({
        parameters: Schema.Array(Member.pipe(Schema.fieldsAssign({ optional: Schema.Boolean, default: Schema.optionalKey(Schema.Json.annotate({ description: 'Value an omitted argument takes' })) }))),
        returns: Schema.OptionFromOptionalKey(Schema.Struct({ type: Schema.String, array: Member.fields.array })),
    }),
).annotate({ identifier: 'Method' });
const Declared = Schema.TaggedUnion({
    Index: { classes: Schema.Array(Schema.String), enumerations: Schema.Array(Schema.String) },
    Class: { name: Schema.String, parents: Schema.Array(Schema.String).annotate({ description: 'Superclasses, nearest first' }), fields: Schema.Array(Field), methods: Schema.Array(Method) },
    Enumeration: { name: Schema.String, members: Schema.Array(Described) },
});
const Declaration = Object.assign(Declared, {
    Field,
    Method,
    describe: <E, R>(name: Option.Option<string>, classes: readonly string[], enumerations: readonly string[], declared: (name: string) => Effect.Effect<typeof Declared.Type, E, R>): Effect.Effect<typeof Declared.Type, E | Unmatched, R> =>
        Option.match(name, {
            onNone: () => Effect.succeed(Declared.cases.Index.make({ classes, enumerations })),
            onSome: (query) => Effect.flatMap(Unmatched.single([...classes, ...enumerations], 'class or enumeration', Option.some(query), Array.of), declared),
        }),
    enrich: <S extends typeof Field | typeof Method>(schema: S, declared: Record.ReadonlyRecord<string, S['Type']>, live: { readonly name: string; readonly settable?: boolean; readonly parameters?: readonly { readonly name: string; readonly optional: boolean }[] }) =>
        Option.match(Record.get(declared, live.name), { onSome: Effect.succeed, onNone: () => Effect.orDie(Schema.decodeEffect(schema)({ settable: false, parameters: [], ...live })) }),
});
const Selector = Schema.TaggedUnion({
    Index: { index: Schema.Int },
    Key: { key: Schema.String, value: Schema.Union([Schema.Int, Schema.String]) },
}).annotate({ description: 'Specifier step selector by index, or by key such as id, uuid, fullName, or name' });
const Step = Schema.Struct({ separator: Schema.Literals(['/', '//']), kind: Schema.String, selector: Schema.optionalKey(Selector) });
const Steps = Schema.Array(Step);

class Specifier extends Schema.Class<Specifier, { readonly brand: unique symbol }>('Specifier')({ steps: Steps, end: Schema.optionalKey(Steps.annotate({ description: 'Range end' })) }) {
    static readonly Path: Schema.Codec<Specifier, string> = Schema.String.annotate({ description: 'Object specifier, / for the application' }).pipe(
        Schema.decodeTo(Specifier, {
            decode: SchemaGetter.transformEffect((text: string, options) => Effect.fromNullishOr(parse(text)).pipe(Effect.mapError(() => new SchemaIssue.InvalidValue({ message: `No object specifier ${text}` }, text, options)))),
            encode: SchemaGetter.transform(format),
        }),
    );

    get path(): string {
        return format(this);
    }
    get root(): boolean {
        return Array.isReadonlyArrayEmpty(this.steps);
    }

    get selector(): Option.Option<typeof Selector.Type> {
        return Option.flatMapNullishOr(Array.last(this.steps), Struct.get('selector'));
    }
    get parent(): string {
        return format({ steps: Array.dropRight(this.steps, 1) });
    }
    get unselected(): string {
        return format({ steps: Array.map(this.steps, (step, index) => (index === this.steps.length - 1 ? Struct.omit(step, ['selector']) : step)) });
    }
    get property(): boolean {
        return Option.exists(Array.last(this.steps), ({ kind }) => String.startsWith('@')(kind));
    }
    get name(): Option.Option<string> {
        return Option.flatMap(Option.filter(this.selector, Selector.guards.Key), ({ key, value }) => (key === 'name' ? Option.some(`${value}`) : Option.none()));
    }
    get leaf(): string {
        return Array.map([this.steps, ...Option.toArray(Option.fromUndefinedOr(this.end))], (steps) => Option.match(Array.last(steps), { onNone: () => '/', onSome: (step) => format({ steps: [step] }).slice(step.separator.length) })).join(' to ');
    }
}
class Reference extends Schema.Class<Reference, { readonly brand: unique symbol }>('Reference')({ type: Schema.String, specifier: Schema.String, name: Schema.OptionFromOptionalKey(Schema.String), valid: Schema.Boolean }) {}

// --- [ERRORS] --------------------------------------------------------------------------

class HostError extends Schema.TaggedError<HostError, { readonly brand: unique symbol }>()('HostError', {
    reason: Schema.TaggedUnion({
        Refused: {},
        Unavailable: {},
        Cancelled: {},
        Thrown: { line: Schema.OptionFromOptionalKey(Schema.Int.annotate({ description: 'Body line that threw' })) },
    }),
    subject: Schema.String,
    cause: Schema.optionalKey(Schema.Defect()),
}) {
    static readonly refused = (subject: string) => (cause?: unknown) => HostError.make({ reason: { _tag: 'Refused' }, subject, ...(cause === undefined ? {} : { cause }) });
    static readonly thrown = (subject: string, line: Option.Option<number>) => (cause: unknown) => HostError.make({ reason: { _tag: 'Thrown', line }, subject, cause });

    override get message(): string {
        return Match.value(this.reason).pipe(
            Match.tag('Thrown', ({ line }) => [...Option.toArray(line).map((at) => `line ${at}`), this.subject].join(': ')),
            Match.orElse(({ _tag }) => [`${this.subject} ${String.toLowerCase(_tag)}`, ...(this.cause === undefined ? [] : [Formatter.format(this.cause)])].join(': ')),
        );
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Declaration, HostError, Reference, Selector, Specifier };
