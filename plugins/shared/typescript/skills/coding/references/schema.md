# [SCHEMA]

## [01]-[CLASSES]

Records a signature names are classes over their schema, one declaration holding its codec and its decoded type's name. `Schema.Opaque` returns the struct it wraps, so AST, wire form, JSON Schema, and decoded plain objects stay the struct's, and a class costs nothing a `const` saves. How a record's values come to exist selects the form:

| [INDEX] | [FORM]                                                            | [VALUES]                                        |
| :-----: | :---------------------------------------------------------------- | :---------------------------------------------- |
|  [01]   | `Schema.Opaque<X>()(struct)`                                      | Plain objects any site builds as a literal      |
|  [02]   | `Schema.Opaque<X, { readonly brand: unique symbol }>()(struct)`   | Plain objects decoding and `X.make` alone build |
|  [03]   | `Schema.Class<X, { readonly brand: unique symbol }>('X')(fields)` | Instances holding getters and methods           |

```ts
class Presigned extends Schema.Opaque<Presigned>()(Schema.Struct({ url: Schema.String, headers: Schema.Record(Schema.String, Schema.String) })) {}
class Registered extends Schema.Opaque<Registered, { readonly brand: unique symbol }>()(Schema.Struct({ code: Schema.String, bundle: Schema.String, build: Schema.OptionFromNullOr(Schema.String) })) {}
class Span extends Schema.Class<Span, { readonly brand: unique symbol }>('Span')(Schema.Struct(Struct.Record(['start', 'end'], Schema.Int.check(Schema.isGreaterThanOrEqualTo(0)))).check(Schema.makeFilter(({ start, end }) => start <= end))) {
    get length() {
        return this.end - this.start;
    }
}
```

`Presigned` is an object-storage signature a handler returns as a literal, `Registered` a Creative Cloud registry row its SQL read alone builds, and `Span` a UTF-16 range whose readers call `length`:
- Declaration emit, hovers, and errors print `X` where an inferred struct type prints every field, and `X.make` returns `X`
- `typeof X.Type` naming a record in a signature marks a `Schema.Struct` constant its class replaces, `X['<field>']` reading each field type after
- Plain `Schema.Struct` constants stay where inference alone carries their type: a field of another schema, a decoder's return, a module-private codec
- `Opaque<X>()` defaults its brand to `{}`, an `Opaque<X, unknown>` restating the default
- Branded records take `X.make` at each build site, a literal failing on the missing brand (TS2741), and a spread of one keeps its brand
- Branded `Opaque` records fail a `Schema.Codec<X, I>` annotation and decode unbranded through a parameter typed `Schema.Codec<A, I>`
- Records holding a branded `Opaque` encode to its unbranded shape, their `Schema.suspend` annotation stating `Schema.Codec<X, Encoded>`
- Exported values inferring a mapped type of another module's branded class (`Struct.evolve`) fail to emit (TS2527), `X.make` naming the result
- `Schema.Class` constructs an instance on decode and writes a `$ref` definition in JSON Schema, a struct turned class changing both
- Class instances lose their prototype through a spread, `structuredClone`, or a worker message, and encoding the copy fails with `Expected X`
- `Equal.equals` and `Hash.hash` read every getter of a class instance, a getter returning a function or host handle breaking both
- Scalars, unions, brands, and literal sets take no class (TS2509)
- `mapFields` returns a plain struct, and a variant a signature names takes its own class
- Classes are exported from their module where TypeScript names them in a declaration (TS4023), and package entries list public API alone
- TS2883 on a class no entry exports marks an exported value whose signature leaks it, rebuilt to return what its callers read

## [02]-[MODELS]

Variants of a record derive from the class owning its fields, each boundary decoding the variant it reads:

```ts
const FacetControl = Control.mapFields((fields) => ({ ...Struct.omit(fields, ['ref', 'positional', 'opens', 'alternate']), locator: Locator, opens: Schema.optionalKey(Locator) }));
```

`Control` is the control record the Swift helper reports with an interned `ref`, and `FacetControl` the control a host plugin states, addressed by its `Locator`:
- Variants derive through `X.mapFields` with `Struct.assign`, `omit`, `pick`, or `map(Schema.optionalKey)`, a `...X.fields` spread restating them
- `X.fields.<f>.schema` reads the schema under `optionalKey` or `withConstructorDefault`, a re-declared shape restating it
- Struct filters drop from a `mapFields` variant, and `{ unsafePreserveChecks: true }` keeps them where the variant holds every field they read
- `Schema.fieldsAssign(fields)` takes a `Struct` alone, adding fields to each struct member of a union through `mapMembers`
- Fields sharing one schema spread from `Struct.Record(keys, S)`, fields differing in an annotation from `Record.map` over the table owning the keys
- Patches are a `mapFields` variant of `optionalKey` fields, `OptionFromNullOr` unsetting an `Option` and `Record(K, NullOr(V))` removing an entry
- `.check(Schema.isMinProperties(1))` on a patch of optional fields rejects an empty patch
- Merges apply a patch through `Struct.evolve` into `X.makeEffect`, rerunning class checks, `new Schema.SchemaError(issue)` lifting its issue
- Records differing per SQL and JSON operation are one `Model.Class` from `effect/schema`, other boundaries declaring `VariantSchema.make`
- `Model.GeneratedByDb` reaches read variants alone, `GeneratedByApp` all but `jsonCreate` and `jsonUpdate`, and `Sensitive` all but JSON
- `Model.DateTimeInsert` and `DateTimeUpdate` stamp writes from the clock, `Model.FieldOption` decoding a `null` column or absent JSON key

Facts other fields decide are getters of a `Schema.Class`, a stored field holding one contradicting its sources. Rules over more than one field are one `.check(Schema.makeFilter(f))` on the record, `f` returning `true`, a message, or `{ path, issue }` at each field a reader fixes:
- Whole-value fixes decode with `{ errors: 'all' }`, `SchemaIssue.makeFormatterStandardSchemaV1()(error.issue).issues` listing one message per path
- Formatter paths are always `PropertyKey[]`, the standard's optional `{ key }` segment type needing no default or mapping
- Issues crossing a wire are a reason case of `Schema.StandardSchemaV1FailureResult.fields` the formatter's result spreads into

## [03]-[UNIONS]

Fields every case shares stay on the record, and the varying part is one `Schema.TaggedUnion` field keyed by tag with each case's fields as the value:

```ts
class Event extends Schema.Opaque<Event>()(Schema.Struct({ instance: ProcessInstance, sequence: Schema.Int, change: Schema.TaggedUnion({ opened: { container: Ref, kind: ContainerKind }, value: { control: Ref, kind: Kind, fact: Fact }, exited: { termination: Exited } }) })) {}

const changed = Event.fields.change.matchOrElse({ value: ({ control }) => Option.some(control) }, Option.none);
```

`ProcessInstance`, `Ref`, `ContainerKind`, `Kind`, `Fact`, and `Exited` are field schemas of the Swift helper's contract, and `changed` returns the control a `value` change names:
- `match` takes a handler per tag and `matchOrElse` the handled tags with one fallback, each data-first or data-last
- `isAnyOf([...])` narrows a decoded value by tag, `guards.<Case>` validates unknown input as one case, and `cases.<Case>.make` builds one
- `Schema.Literals(Struct.keys(Event.fields.change.cases))` decodes a tag parameter
- Unions decode through an index on one required literal key, and members holding none are tried in order, each a full decode
- Wires a repository peer writes state a required `_tag` on every case, untagged encodings staying for wires no repository code writes
- Cases holding derived facts are `Schema.TaggedClass` members with getters, joined through `Schema.Union` piped into `Schema.toTaggedUnion`
- `Schema.Union(cases).pipe(Schema.toTaggedUnion(key))` over a foreign discriminator derives `match`, `guards`, and `cases`, replacing `key ===` tests
- Untagged wire cases declare `_tag: Schema.tagDefaultOmit('<Case>')` closed by `toTaggedUnion('_tag')`, the wire unchanged and readers folding by tag
- Cases keyed by their one property (`{ running: {...} }`) take the key as tag, cases told apart by the fields they hold the operation's name
- Record cases beside tagged ones declare `_tag: Schema.optionalKey(Schema.Never)` in a `StructWithRest`, a malformed tagged value failing at its case
- Cases differing in tag alone share one fields record with a check on the union, a shared-field reader taking `Extract<X['<field>'], { <shared> }>`
- Modes a consumer acts on are cases of a required union, an optional field whose absence stands for a mode restating one
- Host wires decode into a host-neutral union, each wire shape through `decodeTo(Schema.toType(U.cases.<Case>), { decode, encode })`
- Recursive records are classes reading themselves through `Schema.suspend((): Schema.Codec<X> => X)`, a hand interface restating their fields
- Recursive unions sit inline in the field of the record class they recurse through, named `X['<field>']`
- Unions recursing through no record are the one hand type the compiler requires, their const annotated `Schema.Codec<T, E>` against it
- Foreign recursive exports a program reads alone annotate `Schema.Codec<Domain, Export>`, `SchemaGetter.forbidden(() => '<reason>')` refusing encode

## [04]-[TRANSFORMATIONS]

Transformations pair a decode getter from encoded form to domain type with an encode getter back. Schemas declare one direction, `Schema.flip` derives the reverse, and each conversion selects its form:

| [INDEX] | [CONVERSION]                 | [FORM]                                                                              |
| :-----: | :--------------------------- | :---------------------------------------------------------------------------------- |
|  [01]   | Scalar written as text       | `BigDecimalFromString`, `DateTimeUtcFromString`, `DurationFromString`               |
|  [02]   | Number in a unit             | `DurationFromMillis`, `DurationFromNanos`, `ByteSizeFromNumber`                     |
|  [03]   | Encoded literal to domain    | `Schema.Literals(wire).transform(domain)`                                           |
|  [04]   | Coercion a getter names      | `{ decode: SchemaGetter.Boolean(), encode: SchemaGetter.Number() }`                 |
|  [05]   | Delimited list               | `SchemaGetter.split()` decoding, `SchemaGetter.transform(Array.join(','))` encoding |
|  [06]   | Key-value text               | `decodeTo(Schema.Record(K, V), SchemaTransformation.splitKeyValue({ separator }))`  |
|  [07]   | Text grammar                 | `Schema.TemplateLiteralParser` piped into `decodeTo`                                |
|  [08]   | Total conversion             | `decodeTo(to, SchemaTransformation.transform({ decode, encode }))`                  |
|  [09]   | Change within one type       | `Schema.decode(t)`, `Schema.encode(t)` over encoded form                            |
|  [10]   | Throwing or service parse    | `SchemaGetter.transformEffect` with `Effect.try` failing `SchemaIssue.InvalidValue` |
|  [11]   | Lookup over a closed key set | `SchemaGetter.transformOptional(Option.flatMap(lookup))`                            |
|  [12]   | Constant per source case     | `decode: SchemaGetter.succeed(value)`, filling an absent key                        |
|  [13]   | Decode-only source           | `encode: SchemaGetter.forbidden(() => reason)`, `forbiddenEncoding` with no reason  |
|  [14]   | Fallback over a failed parse | `Schema.catchDecoding((issue) => Effect.succeedSome(value))`                        |

```ts
const Plist = Schema.Union([Schema.String, Schema.Uint8Array]).pipe(
    Schema.decodeTo(Schema.Unknown, {
        decode: SchemaGetter.transformEffect((input: string | Uint8Array, options) => Effect.try({ try: () => parse(Predicate.isString(input) ? input : new Uint8Array(input).buffer), catch: (cause) => new SchemaIssue.InvalidValue({ expected: 'a property list', cause }, input, options) })),
        encode: SchemaGetter.forbiddenEncoding,
    }),
);

const Info = Plist.pipe(Schema.decodeTo(Schema.Struct({ CFBundleIdentifier: Schema.String, CFBundleShortVersionString: Schema.String })));
```

`parse` is the `@plist/parse` export, throwing on malformed input. Text and binary property lists decode through one schema, and `Effect.flatMap(files.readFile(plist), Schema.decodeEffect(Info))` fails a bad `Info.plist` with a `SchemaError` holding the thrown value as `cause`, in place of a parse outside the decode:
- Decode getters read their source's `Type` and target checks run on their output, `FiniteFromString` rejecting the `NaN` `NumberFromString` returns
- Getter throws die as defects, and a `Result` or `Option` parse (`Toml.parse`) fails the getter through its `match` into `SchemaIssue.InvalidValue`
- Units without a codec (seconds) decode through `decodeTo(Schema.Duration, transform({ decode: Duration.seconds, encode: Duration.toSeconds }))`
- Text normalized before a stock codec decodes through `decodeTo(Codec, { decode: SchemaGetter.transform(f), encode: SchemaGetter.passthrough() })`
- Host option grammars decode through the package formatter writing them, `Duration.format` over seconds checked by the host's `TemplateLiteral` union
- `TemplateLiteralParser` parts decode transformed schemas in place, a trailing `Schema.String` holding the rest of a path a resolver recurses on
- `Schema.String` parts take text up to the last occurrence of the literal after them, `mcp__a__b__c` decoding to `a__b` and `c`
- Leading and trailing `Schema.String` parts scan text for the parts between them (`[Schema.String, '(Range: ', Schema.Finite, ')', Schema.String]`)
- Host sentinel answers stay a literal member of the decoded union, a caller refusing one through `Effect.filterOrFail(Schema.is(Reply), f)`
- `transform` and `transformEffect` skip an absent key, `transformOptional` receives it as `Option.none()` and returns `Option.none()` to omit it
- Ids naming a record decode to it and back, through `transformOptional` where a literal set rules out a miss, else a failing `transformEffect`
- References to one union case take its kind, decode to `Schema.toType(U.cases[kind])`, and narrow the found record through `filterOrFail(Schema.is)`
- Fields named for the record keep the id's wire key through `Schema.encodeKeys({ record: 'recordId' })`
- Errors a host without Effect states decode from `Schema.Struct(Struct.omit(E.fields, ['message']))` into `Schema.toType(E)` through `E.of`
- `Schema` codecs (`URLFromString`, `DateFromString`) hold a transformation with an encoded-side check, a `decodeTo` restating one lacks it
- `Schema.Class` holds a transformation constructing its instance, `decodeUnknown(Struct)` followed by `new X(...)` restating it
- `Schema.Class(id)` takes fields or a `Struct`, and `X.pipe(Schema.encodeTo(Wire, t))` adds a whole-object transformation to a class
- `encodeKeys`, `extendTo`, `withDecodingDefault*`, `tagDefaultOmit`, and `TemplateLiteralParser` hold their own transformation
- `decodeTo(to)` without a transformation composes schemas, `Schema.decode(t)` targets `Schema.toType(source)` and keeps source checks on output
- `decodeTo` from a source that ran the target's field transformations targets `Schema.toType(target)`, picking keys through `Struct.keys(fields)`
- Transformations shared by more than one schema are one value, `SchemaTransformation.composeTransformation` chaining them with no middle schema
- Services a transformation reads join `DecodingServices` and `EncodingServices`, and the decode effect requires them
- JSON side output is a `toCodecJson` annotation whose encode getter writes through an `Output` reference, every derived codec writing it
- Host handles skip encoding through `encodeTo(Schema.optionalKey(Schema.Never), { decode: SchemaGetter.forbidden(f), encode: SchemaGetter.omit() })`

## [05]-[OPTIONALITY]

Absence a host returns decodes to `Option`, absence a domain default fills decodes to the value, and the encoded spelling selects the form:

| [INDEX] | [ENCODED]                    | [DECODED]   | [FORM]                                                         |
| :-----: | :--------------------------- | :---------- | :------------------------------------------------------------- |
|  [01]   | Key missing                  | `Option<A>` | `Schema.OptionFromOptionalKey(A)`                              |
|  [02]   | Value `null`                 | `Option<A>` | `Schema.OptionFromNullOr(A)`                                   |
|  [03]   | Key missing or `null`        | `Option<A>` | `Schema.OptionFromOptionalNullOr(A)`                           |
|  [04]   | Key missing or `undefined`   | `Option<A>` | `Schema.OptionFromOptional(A)`                                 |
|  [05]   | Value `null` or `undefined`  | `Option<A>` | `Schema.OptionFromNullishOr(A)`                                |
|  [06]   | Key missing                  | `A`         | `A.pipe(Schema.withDecodingDefaultTypeKey(Effect.succeed(a)))` |
|  [07]   | Key missing, encoded default | `A`         | `A.pipe(Schema.withDecodingDefaultKey(Effect.succeed(i)))`     |
|  [08]   | Key missing or `undefined`   | `A`         | `A.pipe(Schema.withDecodingDefaultType(Effect.succeed(a)))`    |
|  [09]   | Key a caller omits           | `?: A`      | `Schema.optionalKey(A)`                                        |

Keys written only where their value differs from the default pair a defaulting decode with an encode dropping the default:

```ts
const Default = <S extends Schema.Top>(schema: S, value: S['Encoded']) => schema.pipe(Schema.toEncoded, Schema.optionalKey, Schema.decodeTo(schema, { decode: SchemaGetter.withDefault(Effect.succeed(value)), encode: SchemaGetter.transformOptional(Option.filter((held) => !Equal.equals(held, value))) }));
```

`Default(Schema.Boolean, false)` decodes a missing key of a command usage table to `false` and encodes `false` as no key, where `{ encodingStrategy: 'omit' }` on a decoding default drops the key from every encode, a set value included:
- Readers of a key `OptionFromOptionalKey` decodes take its `Option`, an `optionalKey` field readers pass to `Option.fromUndefinedOr` restating it
- Caller text decodes through `Schema.Trim.check(Schema.isNonEmpty())`, every reader taking it trimmed and present
- Absence as `null` or `''` decodes by `transform({ decode: flow(Option.fromNullOr, Option.filter(String.isNonEmpty)), encode: Option.getOrNull })`
- Defaults are effects producing the decoded value, `Schema.withDecodingDefaultTypeKey(DateTime.now)` stamping an instant from the clock
- `Schema.withConstructorDefault(effect)` lets `make` omit a key (`Effect.succeedNone` for an `Option`), where a decoding default keeps it required
- Keys stored documents and `make` both omit pipe `withDecodingDefaultKey`, then `withConstructorDefault`, over one `S['Type'] & S['Encoded']` value
- Stored documents this build cannot read decode as absent through `OptionFromNullOr(D).pipe(catchDecoding(() => Effect.succeedSome(Option.none())))`
- `catchDecoding` on the document's own field fails no row or list holding a stale document, and inside `fromJsonString` catches malformed text too
- Readers count the documents decoding as absent and log the count once

## [06]-[SERIALIZATION]

Field schemas declare the domain type, and each format derives its codec from the class or variant it encodes. `toCodecJson` encodes `DateTime`, `BigDecimal`, `Duration`, `Option`, and classes as JSON values, `toCodecStringTree` every leaf as a string:

| [INDEX] | [FORMAT]                  | [CODEC]                                                             |
| :-----: | :------------------------ | :------------------------------------------------------------------ |
|  [01]   | JSON text, JSON fields    | `Schema.fromJsonString(X)`                                          |
|  [02]   | JSON text, other fields   | `X.pipe(Schema.toCodecJson, Schema.fromJsonString)`                 |
|  [03]   | Parsed JSON value         | `Schema.toCodecJson(X)`                                             |
|  [04]   | JSON a Go service writes  | `Schema.toCodecGoJson(X)`                                           |
|  [05]   | Query string or fragment  | `X.pipe(Schema.toCodecStringTree, Schema.fromURLSearchParams)`      |
|  [06]   | Form body                 | `X.pipe(Schema.toCodecStringTree, Schema.fromFormData)`             |
|  [07]   | JSON Schema, draft 2020   | `Schema.toJsonSchemaDocument(X)`                                    |
|  [08]   | JSON Schema, draft 07     | `X.pipe(Schema.toJsonSchemaDocument, JsonSchema.toDocumentDraft07)` |
|  [09]   | Standard Schema validator | `Schema.toStandardSchemaV1(X)`                                      |

Options schemas a host reads as JSON Schema are the schema each run decodes through, so one declaration states both:

```ts
const Clean = Schema.Struct({
    targets: Schema.Array(Schema.String).annotate({ description: 'Targets whose declared outputs are removed', default: [] }),
    __unparsed__: Schema.String.pipe(Schema.Array, Schema.optionalKey).annotate({ $default: { $source: 'unparsed' }, hidden: true, 'x-priority': 'internal' }),
});

const document = JsonSchema.toDocumentDraft07(Schema.toJsonSchemaDocument(Clean, { includeAnnotationKey: (key) => key.startsWith('$') || key.startsWith('x-') || key === 'hidden' }));
```

`Clean` holds the options of the `clean` executor, which decodes them through it, and the `executor-schemas` sync generator writes `document` as the executor's `schema.json`. Nx fills each `default` and routes the words after `--` to the `$default: { $source: 'unparsed' }` key before the decode runs, and resolves `#/definitions/` references alone, which draft-07 conversion writes from `$defs`:
- `toJsonSchemaDocument` describes the encoded side of `toCodecJson(X)` with standard keys, `includeAnnotationKey` passing vendor keys
- `fromJsonString(X)` keeps the encoded type of `X` a peer types against, where `toCodecJson` types it `Json`
- Key spellings a rule decides derive from the fields (`Schema.encodeKeys(Record.map(X.fields, (_, key) => String.camelToSnake(key)))`)
- Structured text builds through its encoder (`encodeEffect(fromJsonString(X, { space }))`, `NetAddress.formatInet`), a template restating it
- Facts a schema holds read from its AST (`SchemaAST.toEncoded(s.ast)`, `SchemaAST.resolveDescription(s.ast)`), a caller argument restating them
- Values bound for host members encode through a struct reusing the host codec's fields, one `encodeEffect` yielding host units and member names
- Cache keys encode a decoded request back through its `TemplateLiteralParser`, spellings decoding equal sharing one entry
- `Cause` errors crossing a wire declare their class through `Schema.declare` with a `toCodecJson` `Schema.link`, `ErrorInstance` dropping their tag
- `toCodecGoJson` decodes the `null` Go writes for a nil slice, map, or optional field as empty or absent
- `Schema.toCodecArrayFromSingle` over an array field accepts the lone value a query string or form sends for a one-item list
- `Schema.Exit(A, E, Schema.Defect())` encodes a finished run, `Schema.Result(A, Schema.ErrorInstance())` a stream item holding its own failure
- `Schema.Result(A, Schema.Defect())` holds a host read refused with a non-`Error` throw as data beside the other fields
- Members typed `Schema.instanceOf(C, { toCodecJson: () => undefined })` encode to nothing in JSON
- `Schema.ArrayEnsure(S)` normalizes one value or an array of values into an array on decode, an `Array.ensure` over a decoded value restating it

## [07]-[HOST_OBJECTS]

Host objects decode at the read through a struct of the members code reads, each declared once, and the struct states every runtime form the host returns where its typings state fewer:
- Hosts with no typings declare each member as a struct field, `Schema.mutableKey` marking a member code writes (`toolType`)
- Fields of a typed host object check their keys through `satisfies { readonly [K in keyof Host]?: Schema.Top }`, a renamed key failing to compile
- Host fields take `Schema.instanceOf(C)` and `Schema.declare(Predicate.isFunction)`, enum fields `Schema.Enum(<HostEnum>)` where typings declare it
- Host enumerations set on `globalThis` with no typings decode once through a struct of the members read (`UserInteractionLevel.DONTDISPLAYALERTS`)
- Host records with one supported form among refused ones decode as `StructWithRest` of an optional key and `Record(String, Json)`
- Caller options for a host call decode through the host's option fields with `{ onExcessProperty: 'error' }`, the default stripping unknown keys
- `Schema.declare(Schema.is(Struct))` returns the host object itself, its methods keeping their receiver
- `Struct.pick` reads own enumerable keys, and host getters read by walking `SchemaAST.toEncoded(View.ast)` by field, `_tag`, or array member
