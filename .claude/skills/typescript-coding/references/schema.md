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
- `Invoice.jsonUpdate.mapFields(Struct.map(Schema.optionalKey))` derives the patch a revision sends
- Relations no getter derives are one struct `Schema.makeFilter` returning `{ path, issue }` at each field a reader fixes
- Readers fixing a whole value decode with `{ errors: 'all' }` and read `SchemaIssue.makeFormatterStandardSchemaV1()(error.issue).issues`, one message per path
- Struct filters drop from a `mapFields` variant, `{ unsafePreserveChecks: true }` keeps them where the variant holds every field they read
- Host objects decode at the read through a struct of read members, with `Schema.instanceOf(C)` and `Schema.declare(Predicate.isFunction)` fields

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
- `Schema.toTaggedUnion` over the encoded discriminator key decodes a foreign union, and `Schema.tagDefaultOmit` fills a tag the encoded form leaves out

## [03]-[TRANSFORMATIONS]

Transformations state decode and encode in one schema from the encoded form to the domain type, and the conversion selects the form:

| [INDEX] | [CONVERSION]               | [FORM]                                                                              |
| :-----: | :------------------------- | :---------------------------------------------------------------------------------- |
|  [01]   | Scalar written as text     | `BigDecimalFromString`, `DateTimeUtcFromString`, `DurationFromString`               |
|  [02]   | Encoded literal to domain  | `Schema.Literals(wire).transform(domain)`                                           |
|  [03]   | Delimited list             | `SchemaGetter.split()` decoding, `SchemaGetter.transform(Array.join(','))` encoding |
|  [04]   | Key-value text             | `decodeTo(Schema.Record(K, V), SchemaTransformation.splitKeyValue({ separator }))`  |
|  [05]   | Text grammar               | `Schema.TemplateLiteralParser` piped into `decodeTo`                                |
|  [06]   | Total conversion           | `decodeTo(to, SchemaTransformation.transform({ decode, encode }))`                  |
|  [07]   | Partial or service reading | `SchemaTransformation.transformEffect` failing with `SchemaIssue.InvalidValue`      |
|  [08]   | Decode-only source         | `encode: SchemaGetter.forbidden(() => reason)`                                      |

```ts
const LineFromText = Schema.TemplateLiteralParser([Schema.String, ' x', Schema.Finite, ' @ ', Schema.BigDecimalFromString]).pipe(
    Schema.decodeTo(Line, SchemaTransformation.transform({ decode: ([sku, , quantity, , price]) => ({ sku, quantity, price }), encode: ({ sku, quantity, price }) => [sku, ' x', quantity, ' @ ', price] as const })),
);
```

`LineFromText` reads `A-17 x3 @ 12.50` into a `Line`, the parts state the text grammar alone, and `Line` decodes them with its field checks once (`x0` fails at `quantity`):
- `Schema.StringFromUriComponent` parts escape each URI path segment on encode
- Services a transformation reads join `DecodingServices` and `EncodingServices`, and the decode effect requires them

## [04]-[OPTIONALITY]

Absence a host returns decodes to `Option`, absence a domain default fills decodes to the value, and the encoded spelling selects the form:

| [INDEX] | [ENCODED]                   | [DECODED]   | [FORM]                                                         |
| :-----: | :-------------------------- | :---------- | :------------------------------------------------------------- |
|  [01]   | Key missing                 | `Option<A>` | `Schema.OptionFromOptionalKey(A)`                              |
|  [02]   | Value `null`                | `Option<A>` | `Schema.OptionFromNullOr(A)`                                   |
|  [03]   | Key missing or `null`       | `Option<A>` | `Schema.OptionFromOptionalNullOr(A)`                           |
|  [04]   | Key missing or `undefined`  | `Option<A>` | `Schema.OptionFromOptional(A)`                                 |
|  [05]   | Value `null` or `undefined` | `Option<A>` | `Schema.OptionFromNullishOr(A)`                                |
|  [06]   | Key missing                 | `A`         | `A.pipe(Schema.withDecodingDefaultTypeKey(Effect.succeed(a)))` |
|  [07]   | Key missing or `undefined`  | `A`         | `A.pipe(Schema.withDecodingDefaultType(Effect.succeed(a)))`    |
|  [08]   | Key a caller omits          | `?: A`      | `Schema.optionalKey(A)`                                        |

- Absence as `null` or `''` decodes by `transform({ decode: flow(Option.fromNullOr, Option.filter(String.isNonEmpty)), encode: Option.getOrNull })`
- Defaults are effects producing the decoded value, `Schema.withDecodingDefaultTypeKey(DateTime.now)` stamps an instant from the clock
- `{ encodingStrategy: 'omit' }` drops a defaulted key on encode

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
|  [04]   | Query string              | `Schema.fromURLSearchParams(Schema.toCodecStringTree(X))` |
|  [05]   | Form body                 | `Schema.fromFormData(Schema.toCodecStringTree(X))`        |
|  [06]   | Tool, form, or API schema | `Schema.toJsonSchemaDocument(X)`                          |
|  [07]   | Standard Schema validator | `Schema.toStandardSchemaV1(X)`                            |

- `toCodecGoJson` decodes the `null` Go writes for a nil slice, map, or optional field as empty or absent
- `Schema.toCodecArrayFromSingle` over an array field accepts the lone value a query string or form sends for a one-item list
