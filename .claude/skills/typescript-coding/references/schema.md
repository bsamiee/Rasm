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
- `Model.GeneratedByDb` reaches the read variants alone, `Model.GeneratedByApp` every variant but `jsonCreate` and `jsonUpdate`, and `Model.Sensitive` every variant but JSON
- `Model.DateTimeUpdate` stamps each write, and `Model.FieldOption` decodes a `null` column and a missing JSON key to one `Option`
- Boundaries other than a table and a JSON API declare their variant set once through `VariantSchema.make({ variants, defaultVariant })`, with `Class`, `Field`, and `FieldExcept` deriving the same way
- `X.mapFields(Struct.assign(...))`, `Struct.omit`, and `Struct.map(Schema.optionalKey)` derive a variant, `Schema.fieldsAssign(fields)` adds fields to each struct member of a union through `mapMembers`, and a `...X.fields` spread restates them
- Fields sharing one schema spread from `Struct.Record(keys, S)`, the keys a literal list or `X.literals`
- Relations no getter derives are one struct `Schema.makeFilter` returning `{ path, issue }` at each field a reader fixes
- Readers fixing a whole value decode with `{ errors: 'all' }` and read `SchemaIssue.makeFormatterStandardSchemaV1()(error.issue).issues`, one message per path
- Struct filters drop from a `mapFields` variant, `{ unsafePreserveChecks: true }` keeps them where the variant holds every field they read
- Host objects decode at the read through a struct of read members, with `Schema.instanceOf(C)` and `Schema.declare(Predicate.isFunction)` fields
- `Schema.declare(Schema.is(Struct))` returns the host object itself, its methods keeping their receiver
- `Struct.pick` reads own enumerable keys, and prototype getters copy through `Record.fromIterableWith(Struct.keys(X.fields), (k) => [k, host[k]])`

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

- `match` takes a handler per tag, `matchOrElse` the handled tags and one fallback, `isAnyOf([...])` narrows a decoded value by tag, `guards.Paid` validates unknown input as one case, and `cases.Paid.make` builds one
- `Schema.Literals(Struct.keys(Activity.fields.change.cases))` decodes a tag parameter
- Cases holding derived facts are `Schema.TaggedClass` members with getters, joined through `Schema.Union` piped into `Schema.toTaggedUnion`
- `Schema.Union(cases).pipe(Schema.toTaggedUnion(key))` over a foreign discriminator derives `match`, `guards`, and `cases`, replacing `key ===` tests
- Unions keyed by each case's one property (`{ running: {...} }`) decode each case through `decodeTo(TaggedStruct)`, closed by `toTaggedUnion('_tag')`
- `Schema.tagDefaultOmit` fills a tag the encoded form leaves out

## [03]-[TRANSFORMATIONS]

Transformations pair a decode getter from encoded form to domain type with an encode getter back. Schemas declare one direction, `Schema.flip` derives the reverse, and each conversion selects its form:

| [INDEX] | [CONVERSION]                 | [FORM]                                                                              |
| :-----: | :--------------------------- | :---------------------------------------------------------------------------------- |
|  [01]   | Scalar written as text       | `BigDecimalFromString`, `DateTimeUtcFromString`, `DurationFromString`               |
|  [02]   | Encoded literal to domain    | `Schema.Literals(wire).transform(domain)`                                           |
|  [03]   | Coercion a getter names      | `{ decode: SchemaGetter.Boolean(), encode: SchemaGetter.Number() }`                 |
|  [04]   | Delimited list               | `SchemaGetter.split()` decoding, `SchemaGetter.transform(Array.join(','))` encoding |
|  [05]   | Key-value text               | `decodeTo(Schema.Record(K, V), SchemaTransformation.splitKeyValue({ separator }))`  |
|  [06]   | Text grammar                 | `Schema.TemplateLiteralParser` piped into `decodeTo`                                |
|  [07]   | Total conversion             | `decodeTo(to, SchemaTransformation.transform({ decode, encode }))`                  |
|  [08]   | Change within one type       | `Schema.decode(t)`, `Schema.encode(t)` over encoded form                            |
|  [09]   | Throwing or service parse    | `SchemaGetter.transformEffect` with `Effect.try` failing `SchemaIssue.InvalidValue` |
|  [10]   | Constant per source case     | `decode: SchemaGetter.succeed(value)`, filling an absent key                        |
|  [11]   | Decode-only source           | `encode: SchemaGetter.forbiddenEncoding`                                            |
|  [12]   | Fallback over a failed parse | `Schema.catchDecoding(f)` around whole parse effect                                 |

```ts
class Certificate extends Schema.Class<Certificate, { readonly brand: unique symbol }>('Certificate')({
    subject: Schema.NonEmptyString,
    fingerprint: Schema.String.check(Schema.isPattern(/^(?:[\dA-F]{2}:){31}[\dA-F]{2}$/)),
    validFrom: Schema.DateTimeUtcFromDate,
    validTo: Schema.DateTimeUtcFromDate,
}) {}

const Thrown = Schema.Struct({ message: Schema.String });

const CertificateFromPem = Schema.String.pipe(
    Schema.decodeTo(Certificate, {
        decode: SchemaGetter.transformEffect((pem, options) =>
            Effect.try({
                try: () => new X509Certificate(pem),
                catch: (cause) => new SchemaIssue.InvalidValue({ expected: 'PEM certificate', message: pipe(cause, Schema.decodeUnknownOption(Thrown), Option.map(Struct.get('message')), Option.getOrUndefined) }, pem, options),
            }).pipe(
                Effect.map(({ subject, fingerprint256, validFromDate, validToDate }) => ({ subject, fingerprint: fingerprint256, validFrom: validFromDate, validTo: validToDate })),
            ),
        ),
        encode: SchemaGetter.forbiddenEncoding,
    }),
);
```

`CertificateFromPem` replaces `new X509Certificate(pem)` inside `try` followed by `new Certificate(...)`. `X509Certificate` from `node:crypto` throws on malformed text, `Thrown` reads the message an `Error` or a thrown plain object holds, and the getter returns encoded fields for `Certificate` to decode its dates, check `fingerprint`, and construct itself:
- Decode getters read their source's `Type` and target checks run on their output, `FiniteFromString` rejects `NaN` that `NumberFromString` returns
- Getter throws die as defects, a `Result` parse fails through `Effect.fromResult` and an `Option` parse through `Option.match`
- `transform` and `transformEffect` skip an absent key, `transformOptional` receives it as `Option.none()` and returns `Option.none()` to omit it
- `Schema` codecs (`URLFromString`, `DateFromString`) hold a transformation with an encoded-side check, a `decodeTo` restating one lacks it
- `Schema.Class` holds a transformation constructing its instance, `decodeUnknown(Struct)` followed by `new X(...)` restates it
- `Schema.Class(id)` takes fields or a `Struct`, `X.pipe(Schema.encodeTo(Wire, t))` adds a whole-object transformation to a class
- `encodeKeys`, `extendTo`, `withDecodingDefault*`, `tagDefaultOmit`, and `TemplateLiteralParser` hold their own transformation
- `decodeTo(to)` without a transformation composes schemas, `Schema.decode(t)` targets `Schema.toType(source)` and keeps source checks on output
- Transformations shared by more than one schema are one value, `composeTransformation` chains transformations in place of an intermediate schema
- `Schema.StringFromUriComponent` as a `TemplateLiteralParser` part escapes each URI path segment on encode
- Services a transformation reads join `DecodingServices` and `EncodingServices`, and the decode effect requires them
- Runtime handles stay out of the encoded value through `encodeTo(Schema.optionalKey(Schema.Never), { decode: SchemaGetter.forbidden(f), encode: SchemaGetter.omit() })`
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

- Absence as `null` or `''` decodes by `transform({ decode: flow(Option.fromNullOr, Option.filter(String.isNonEmpty)), encode: Option.getOrNull })`
- Defaults are effects producing the decoded value, `Schema.withDecodingDefaultTypeKey(DateTime.now)` stamps an instant from the clock
- `{ encodingStrategy: 'omit' }` drops the key from every encode, a set value included, and the default `'passthrough'` writes it
- `Schema.withConstructorDefault(effect)` lets `make` omit a key, filled by `Effect.succeedNone` for an `Option` field or the decoding default

## [05]-[SERIALIZATION]

Field schemas declare the domain type, and each format derives its codec from the class or model variant it encodes. Key spellings a rule decides derive from the fields:

```ts
const snakeCased = <S extends Schema.Top & { readonly fields: Schema.Struct.Fields }>(schema: S) => schema.pipe(Schema.encodeKeys(Record.map(schema.fields, (_, key) => String.camelToSnake(key))));
```

`Schema.fromJsonString(Schema.toCodecJson(snakeCased(Invoice.json)))` reads and writes `issued_at` as `issuedAt`. `toCodecJson` encodes `DateTime`, `BigDecimal`, `Duration`, `Option`, and classes as JSON values, `toCodecStringTree` every leaf as a string:

| [INDEX] | [FORMAT]                  | [CODEC]                                                   |
| :-----: | :------------------------ | :-------------------------------------------------------- |
|  [01]   | JSON text                 | `Schema.fromJsonString(Schema.toCodecJson(X))`            |
|  [02]   | Parsed JSON value         | `Schema.toCodecJson(X)`                                   |
|  [03]   | JSON a Go service writes  | `Schema.toCodecGoJson(X)`                                 |
|  [04]   | Query string or fragment  | `Schema.fromURLSearchParams(Schema.toCodecStringTree(X))` |
|  [05]   | Form body                 | `Schema.fromFormData(Schema.toCodecStringTree(X))`        |
|  [06]   | Tool, form, or API schema | `Schema.toJsonSchemaDocument(X)`                          |
|  [07]   | Standard Schema validator | `Schema.toStandardSchemaV1(X)`                            |

- `toCodecGoJson` decodes the `null` Go writes for a nil slice, map, or optional field as empty or absent
- `Schema.toCodecArrayFromSingle` over an array field accepts the lone value a query string or form sends for a one-item list
- `Schema.ArrayEnsure(S)` normalizes one value or an array of values into an array on decode, an `Array.ensure` over a decoded value restating it
