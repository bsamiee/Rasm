import { Unmatched } from '@rasm/search';
import { Array, Effect, ErrorReporter, Formatter, type LogLevel, Match, Option, Order, Record, Schema, SchemaGetter, SchemaIssue, String, Struct } from 'effect';
import { format, parse } from '#/host';

// --- [MODELS] --------------------------------------------------------------------------

const Unstated = Schema.Boolean.pipe(Schema.withConstructorDefault(Effect.succeed(false)));
const Bounds = Struct.Record(['minimum', 'maximum'], Schema.OptionFromOptionalKey(Schema.Finite).pipe(Schema.withConstructorDefault(Effect.succeedNone)));
const Typed = Schema.Struct({
    name: Schema.String,
    description: Schema.OptionFromOptionalKey(Schema.String).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
    type: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Type as the source spells it, a method its return type' })).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
    ...Bounds,
});
const Tagged = { version: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Host version the source tags the declaration with' })).pipe(Schema.withConstructorDefault(Effect.succeedNone)) };
const Member = { ...Tagged, static: Unstated, async: Unstated.annotate({ description: 'Answers with a promise' }) };
const Parameter = Typed.pipe(Schema.fieldsAssign({ optional: Unstated })).annotate({ identifier: 'Parameter' });
const Field = Typed.pipe(Schema.fieldsAssign({ ...Member, readonly: Schema.Boolean, optional: Unstated })).annotate({ identifier: 'Field' });
const Method = Typed.pipe(Schema.fieldsAssign({ ...Member, parameters: Schema.Array(Parameter) })).annotate({ identifier: 'Method' });
const Named = Schema.Array(Schema.String).annotate({ description: 'Classes and enumerations a string names, empty where the value takes a string' });
const Model = Schema.Struct({
    classes: Schema.Record(
        Schema.String,
        Schema.Struct({
            fields: Schema.Record(
                Schema.String,
                Schema.Struct({
                    settable: Schema.Boolean,
                    ...Struct.Record(['collection', 'array'], Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Element class of a collection or array field' }))),
                    classes: Named,
                    ...Bounds,
                }),
            ),
            methods: Schema.Record(Schema.String, Schema.Array(Named).annotate({ description: 'Per parameter' })),
        }),
    ),
    enumerations: Schema.Array(Schema.String),
}).annotate({ identifier: 'Model', description: 'Fields and methods per class and the enumerations a host object read or write takes from its declarations' });
const Declared = Schema.TaggedUnion({
    Index: { classes: Schema.Array(Schema.String), enumerations: Schema.Array(Schema.String) },
    Class: {
        name: Schema.String,
        description: Typed.fields.description,
        ...Tagged,
        parents: Schema.Array(Schema.String).annotate({ description: 'Superclasses, nearest first' }),
        constructible: Schema.Boolean,
        fields: Schema.Array(Field),
        methods: Schema.Array(Method),
    },
    Enumeration: { name: Schema.String, description: Typed.fields.description, ...Tagged, members: Schema.Array(Schema.Struct({ name: Schema.String, description: Typed.fields.description, value: Schema.OptionFromOptionalKey(Schema.Union([Schema.String, Schema.Finite])) })) },
});
const Page = Schema.Union([Declared.cases.Class, Declared.cases.Enumeration]);
const Declaration = Object.assign(Declared, {
    Field,
    Method,
    Parameter,
    Page,
    Model,
    catalog: <A, P>(table: Record.ReadonlyRecord<string, A>, page: (entry: A) => P, enumerated: (entry: A) => boolean) => {
        const sorted = (enumerations: boolean) => Array.sort(Record.keys(Record.filter(table, (entry) => enumerated(entry) === enumerations)), Order.String);
        return {
            index: Declared.cases.Index.make({ classes: sorted(false), enumerations: sorted(true) }),
            page: (name: string) =>
                Effect.map(
                    Unmatched.single(Record.toEntries(table), 'class or enumeration', Option.some(name), ([key]) => [key]),
                    ([, found]) => page(found),
                ),
        };
    },
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

    get selector() {
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
    message: Schema.String,
}) {
    static readonly of = (reason: HostError['reason'], subject: string) => (cause?: unknown) =>
        HostError.make({
            reason,
            subject,
            ...(cause === undefined ? {} : { cause }),
            message: Match.value(reason).pipe(
                Match.tag('Thrown', ({ line }) => [...Option.toArray(line).map((at) => `line ${at}`), subject].join(': ')),
                Match.orElse(({ _tag }) => [`${subject} ${String.toLowerCase(_tag)}`, ...(cause === undefined ? [] : [Formatter.format(cause)])].join(': ')),
            ),
        });
    static readonly refused = (subject: string) => HostError.of({ _tag: 'Refused' }, subject);
    static readonly thrown = (subject: string, line: Option.Option<number>) => HostError.of({ _tag: 'Thrown', line }, subject);
    override readonly [ErrorReporter.severity]: LogLevel.Severity = this.reason._tag === 'Unavailable' ? 'Warn' : 'Info';
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Declaration, HostError, Reference, Selector, Specifier };
