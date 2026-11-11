# [SCHEMA]

## [01]-[MODELS]

Records differing in shape per boundary operation are one `Model.Class` from `effect/schema`. Each field reaches the variants its role names, the class decodes a stored row, and `insert`, `update`, `json`, `jsonCreate`, and `jsonUpdate` are the codecs of each operation. A fact other fields decide is a getter no stored field contradicts:

```ts
class Invoice extends Model.Class<Invoice>('Invoice')({
    id: Model.UuidV7Insert(Schema.String.check(Schema.isUUID()).pipe(Schema.brand('InvoiceId'))),
    customer: Schema.NonEmptyString,
    term: Schema.DurationFromMillis.check(Schema.makeIsGreaterThan({ order: Duration.Order })(Duration.zero)),
    lines: Model.JsonFromString(Schema.NonEmptyArray(Line)),
    issuedAt: Model.DateTimeInsert,
}) {
    get due() {
        return DateTime.addDuration(this.issuedAt, this.term);
    }
}
```

`Invoice.jsonCreate` decodes a body holding neither `id` nor `issuedAt`, `Invoice.insert.makeEffect(created)` fills both from the clock, and `SqlModel.makeRepository(Invoice, { tableName: 'invoices', spanPrefix: 'Invoices', idColumn: 'id' })` writes the row and returns the decoded class. `lines` is a JSON text column in the table and an array in every JSON variant:
- `Model.GeneratedByDb` reaches read variants alone, `GeneratedByApp` all but `jsonCreate` and `jsonUpdate`, and `Sensitive` all but JSON
- `Model.DateTimeUpdate` stamps each write, and `Model.FieldOption` decodes a `null` column and a missing JSON key to one `Option`
- Boundaries besides SQL and JSON declare variants in `VariantSchema.make({ variants, defaultVariant })` with `Class`, `Field`, and `FieldExcept`
- Variants derive through `X.mapFields` with `Struct.assign`, `omit`, `pick`, or `map(Schema.optionalKey)`, a `...X.fields` spread restating them
- `Schema.fieldsAssign(fields)` takes a `Struct` alone, adding fields to each struct member of a union through `mapMembers`
- Patches are a `mapFields` variant of `optionalKey` fields, `OptionFromNullOr` unsetting an `Option` and `Record(K, NullOr(V))` removing an entry
- Merges apply a patch through `Struct.evolve` into `X.makeEffect`, rerunning class checks, `new Schema.SchemaError(issue)` lifting its issue
- Fields sharing one schema spread from `Struct.Record(keys, S)`, fields differing in an annotation from `Record.map` over the table owning the keys
- `X.fields.<f>.schema` reads the schema under `optionalKey` or `withConstructorDefault`, a re-declared shape restating it
- Relations no getter derives are one struct `Schema.makeFilter` returning `{ path, issue }` at each field a reader fixes
- Whole-value fixes decode with `{ errors: 'all' }`, `SchemaIssue.makeFormatterStandardSchemaV1()(error.issue).issues` listing one message per path
- Formatter paths are always `PropertyKey[]`, the standard's optional `{ key }` segment type needing no default or mapping
- Issues crossing a wire are a reason case of `Schema.StandardSchemaV1FailureResult.fields` the formatter's result spreads into
- Struct filters drop from a `mapFields` variant, `{ unsafePreserveChecks: true }` keeps them where the variant holds every field they read
- Host objects decode at the read through a struct of read members stating every runtime form the host returns, its typings stating fewer
- Host struct fields take `Schema.instanceOf(C)` and `Schema.declare(Predicate.isFunction)`, host enum fields `Schema.Enum(<HostEnum>)` over `Schema.Int`
- Host records with one supported form among refused ones decode as `StructWithRest` of an optional key and `Record(String, Json)`, naming present keys
- Caller options for a host call decode through the host's option fields with `{ onExcessProperty: 'error' }`, the default stripping unknown keys
- `Schema.declare(Schema.is(Struct))` returns the host object itself, its methods keeping their receiver
- `Struct.pick` reads own enumerable keys, and host getters read by walking `SchemaAST.toEncoded(View.ast)` by field, `_tag` or array member, at every depth

## [02]-[UNIONS]

Fields every case shares stay on the class, and the varying part is one `Schema.TaggedUnion` field keyed by tag with each case's fields as the value:

```ts
class Activity extends Schema.Class<Activity, { readonly brand: unique symbol }>('Activity')({
    invoice: Invoice.fields.id,
    at: Schema.DateTimeUtc,
    change: Schema.TaggedUnion({
        Paid: { amount: Schema.BigDecimal, reference: Schema.NonEmptyString },
        Disputed: { reason: Schema.NonEmptyString },
        Voided: {},
    }),
}) {
    get paid() {
        return Activity.fields.change.matchOrElse(this.change, { Paid: ({ amount }) => Option.some(amount) }, Option.none);
    }
}
```

- `match` takes a handler per tag, `matchOrElse` the handled tags and one fallback
- `isAnyOf([...])` narrows a decoded value by tag, `guards.Paid` validates unknown input as one case, and `cases.Paid.make` builds one
- `Schema.Literals(Struct.keys(Activity.fields.change.cases))` decodes a tag parameter
- Cases holding derived facts are `Schema.TaggedClass` members with getters, joined through `Schema.Union` piped into `Schema.toTaggedUnion`
- `Schema.Union(cases).pipe(Schema.toTaggedUnion(key))` over a foreign discriminator derives `match`, `guards`, and `cases`, replacing `key ===` tests
- Unions keyed by each case's one property (`{ running: {...} }`) decode each case through `decodeTo(TaggedStruct)`, closed by `toTaggedUnion('_tag')`
- Encoded cases with no tag declare `_tag: Schema.tagDefaultOmit('<Case>')` beside their `encodeKeys`, decoded cases folding through `Match.valueTags`
- Record cases beside tagged ones declare `_tag: Schema.optionalKey(Schema.Never)` in a `StructWithRest`, a malformed tagged value failing at its case
- Cases differing in tag alone share one fields record, a check spanning them sits on the union, and a shared-field reader takes `Extract<U, { field }>`
- Recursive schemas type only the recursing cases by hand, the rest `typeof Leaf.Type` of one schema union, each slot reading one `Schema.suspend` const
- Recursive consts annotate `Schema.Codec<T, E>`, and `Schema.suspend((): Schema.Codec<T, E> => X)` breaks the inference cycle

## [03]-[TRANSFORMATIONS]

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
|  [11]   | Constant per source case     | `decode: SchemaGetter.succeed(value)`, filling an absent key                        |
|  [12]   | Decode-only source           | `encode: SchemaGetter.forbiddenEncoding`                                            |
|  [13]   | Fallback over a failed parse | `Schema.catchDecoding(f)` around whole parse effect                                 |

```ts
const fromText = (expected: string, parse: (text: string) => unknown) =>
    Schema.String.pipe(
        Schema.decodeTo(Schema.Unknown, {
            decode: SchemaGetter.transformEffect((text: string, options) => Effect.try({ try: () => parse(text), catch: (cause) => new SchemaIssue.InvalidValue({ expected, cause }, text, options) })),
            encode: SchemaGetter.forbiddenEncoding,
        }),
    );

const Manifest = fromText('YAML', Yaml.parse).pipe(Schema.decodeTo(Schema.Struct({ name: Schema.NonEmptyString, items: Schema.Array(Item) })));
```

`fromText` holds one throwing parser per text format, its issue keeping the thrown value as `cause`, and every schema over that format decodes through it in place of a `try` around the parse followed by a decode. `Yaml` is the `effect/encoding` module, its `parse` throwing a `SyntaxError`, and `Item` a row schema:
- Decode getters read their source's `Type` and target checks run on their output, `FiniteFromString` rejects `NaN` that `NumberFromString` returns
- Getter throws die as defects, and a `Result` parse (`Toml.parse`) or an `Option` parse fails the getter through `Result.match` or `Option.match` into `SchemaIssue.InvalidValue`
- Units without a codec (seconds) decode through `decodeTo(Schema.Duration, transform({ decode: Duration.seconds, encode: Duration.toSeconds }))`
- Text normalized before a stock codec decodes through `decodeTo(Codec, { decode: SchemaGetter.transform(f), encode: SchemaGetter.passthrough() })`
- `TemplateLiteralParser` parts decode transformed schemas in place, a trailing `Schema.String` holding the rest of a path a resolver recurses on
- Host sentinel answers stay a literal member of the decoded union, a caller failing on one through `Effect.filterOrFail(Schema.is(Reply), onSentinel)`
- `transform` and `transformEffect` skip an absent key, `transformOptional` receives it as `Option.none()` and returns `Option.none()` to omit it
- `Schema` codecs (`URLFromString`, `DateFromString`) hold a transformation with an encoded-side check, a `decodeTo` restating one lacks it
- `Schema.Class` holds a transformation constructing its instance, `decodeUnknown(Struct)` followed by `new X(...)` restates it
- `Schema.Class(id)` takes fields or a `Struct`, `X.pipe(Schema.encodeTo(Wire, t))` adds a whole-object transformation to a class
- `encodeKeys`, `extendTo`, `withDecodingDefault*`, `tagDefaultOmit`, and `TemplateLiteralParser` hold their own transformation
- `decodeTo(to)` without a transformation composes schemas, `Schema.decode(t)` targets `Schema.toType(source)` and keeps source checks on output
- `decodeTo` from a source that ran the target's field transformations targets `Schema.toType(target)`, picking keys through `Struct.keys(fields)`
- Transformations shared by more than one schema are one value, `composeTransformation` chains transformations in place of an intermediate schema
- Ids naming a record decode to it through a `transformEffect` lookup and encode back to the id, a stored and a resolved schema pair restating them
- References to one union case take its kind, decode to `Schema.toType(U.cases[kind])`, and narrow the found record through `filterOrFail(Schema.is)`
- Fields named for the record keep the id's wire key through `Schema.encodeKeys({ record: 'recordId' })`
- `Schema.StringFromUriComponent` as a `TemplateLiteralParser` part escapes each URI path segment on encode
- Services a transformation reads join `DecodingServices` and `EncodingServices`, and the decode effect requires them
- JSON side output is a `toCodecJson` annotation whose encode getter writes through an `Output` reference, every derived codec writing it
- Host handles skip encoding through `encodeTo(Schema.optionalKey(Schema.Never), { decode: SchemaGetter.forbidden(f), encode: SchemaGetter.omit() })`
- `Schema.decodeEffect` types its input as the schema's `Encoded` and `decodeUnknownEffect` as `unknown`, both running one decode

## [04]-[OPTIONALITY]

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

- Caller text decodes through `Schema.Trim` and `Schema.NonEmptyString` (`Schema.Trim.check(Schema.isNonEmpty())`), every reader taking it trimmed and present
- Absence as `null` or `''` decodes by `transform({ decode: flow(Option.fromNullOr, Option.filter(String.isNonEmpty)), encode: Option.getOrNull })`
- Defaults are effects producing the decoded value, `Schema.withDecodingDefaultTypeKey(DateTime.now)` stamps an instant from the clock
- `{ encodingStrategy: 'omit' }` drops the key from every encode, a set value included, and the default `'passthrough'` writes it
- `Schema.withConstructorDefault(effect)` lets `make` omit a key, `Effect.succeedNone` filling an `Option` field, where a decoding default keeps it required
- Keys stored documents and `make` both omit pipe `withDecodingDefaultKey` then `withConstructorDefault` over one value typed `S['Type'] & S['Encoded']`

## [05]-[SERIALIZATION]

Field schemas declare the domain type, and each format derives its codec from the class or model variant it encodes. Key spellings a rule decides derive from the fields:

```ts
const snakeCased = <S extends Schema.Top & { readonly fields: Schema.Struct.Fields }>(schema: S) => schema.pipe(Schema.encodeKeys(Record.map(schema.fields, (_, key) => String.camelToSnake(key))));
```

`Schema.fromJsonString(Schema.toCodecJson(snakeCased(Invoice.json)))` reads and writes `issued_at` as `issuedAt`. `toCodecJson` encodes `DateTime`, `BigDecimal`, `Duration`, `Option`, and classes as JSON values, `toCodecStringTree` every leaf as a string:

| [INDEX] | [FORMAT]                  | [CODEC]                                                   |
| :-----: | :------------------------ | :-------------------------------------------------------- |
|  [01]   | JSON text, JSON fields    | `Schema.fromJsonString(X)`                                |
|  [02]   | JSON text, other fields   | `Schema.fromJsonString(Schema.toCodecJson(X))`            |
|  [03]   | Parsed JSON value         | `Schema.toCodecJson(X)`                                   |
|  [04]   | JSON a Go service writes  | `Schema.toCodecGoJson(X)`                                 |
|  [05]   | Query string or fragment  | `Schema.fromURLSearchParams(Schema.toCodecStringTree(X))` |
|  [06]   | Form body                 | `Schema.fromFormData(Schema.toCodecStringTree(X))`        |
|  [07]   | Tool, form, or API schema | `Schema.toJsonSchemaDocument(X)`                          |
|  [08]   | Standard Schema validator | `Schema.toStandardSchemaV1(X)`                            |

- `fromJsonString(X)` keeps `typeof X.Encoded` a peer types against, where `toCodecJson` types it `Json`
- Structured text builds through its encoder (`encodeEffect(fromJsonString(X, { space }))` for people, `NetAddress.formatInet`), a template restating it
- Edits to a document go through its format library's concrete syntax tree, keeping comments and layout, a pattern rewrite restating it
- Source no TypeScript library parses (Swift) edits through the `ast-grep` CLI under the project's `sgconfig.yml`
- Facts a schema holds read from its AST (`SchemaAST.toEncoded(s.ast)`, `SchemaAST.resolveDescription(s.ast)`), a caller argument restating them
- Values bound for host members encode through a struct reusing the host codec's fields, one `encodeEffect` yielding host units and member names
- `Cause` errors crossing a wire declare their class through `Schema.declare` with a `toCodecJson` `Schema.link`, `ErrorInstance` dropping their tag
- `toCodecGoJson` decodes the `null` Go writes for a nil slice, map, or optional field as empty or absent
- `Schema.toCodecArrayFromSingle` over an array field accepts the lone value a query string or form sends for a one-item list
- `Schema.Exit(A, E, Schema.Defect())` encodes a finished run, `Schema.Result(A, Schema.ErrorInstance())` a stream item holding its own failure
- `Schema.Result(A, Schema.Defect())` holds a host read refused with a non-`Error` throw as data beside the other fields
- Members typed `Schema.instanceOf(C, { toCodecJson: () => undefined })` encode to nothing in JSON
- `.check(Schema.isMinProperties(1))` on an all-optional struct rejects an empty patch
- `Schema.ArrayEnsure(S)` normalizes one value or an array of values into an array on decode, an `Array.ensure` over a decoded value restating it
