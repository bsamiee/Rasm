# [SCHEMA]

Covers schemas as codecs at a boundary, from classes and derived variants to transformations, optionality, and wire codecs.

## [01]-[CLASSES]

`Schema.Class` is one declaration for the type, constructor, guard, equality, and codec of a value that crosses a wire, storage, or host. `Schema.decodeUnknownEffect(X)` runs where host data enters and `Schema.encodeEffect(X)` where it leaves. Constraints sit on the field that owns them, a rule relating fields is one `makeFilter` on the struct, and facts other fields decide are getters:

```ts
class Invoice extends Schema.Class<Invoice, { readonly brand: unique symbol }>('Invoice')(
    Schema.Struct({
        id: Schema.String.check(Schema.isULID()).pipe(Schema.brand('InvoiceId')),
        issued: Schema.DateTimeUtc,
        due: Schema.DateTimeUtc,
        lines: Schema.NonEmptyArray(
            Schema.Struct({
                sku: Schema.NonEmptyString,
                price: Schema.BigDecimal,
                quantity: Schema.Int.check(Schema.isBetween({ minimum: 1, maximum: 999 })),
            }),
        ),
        note: Schema.OptionFromOptionalKey(Schema.String),
    }).check(Schema.makeFilter(({ issued, due }) => DateTime.isLessThan(issued, due) || { path: ['due'], issue: 'precedes issued' })),
) {
    get total() {
        return BigDecimal.sumAll(Array.map(this.lines, ({ price, quantity }) => BigDecimal.multiply(price, BigDecimal.fromBigInt(BigInt(quantity)))));
    }

    static readonly json = Schema.fromJsonString(Schema.toCodecJson(this));
}
```

- `{ readonly brand: unique symbol }` makes the instance type nominal, a plain object or another class with equal fields fails to compile as an `Invoice`
- `Invoice.fields.id` is the field schema wherever an invoice id appears, `Invoice.fields.lines.value` the element schema of the array field
- Statics hold values derived from the class (codecs, orders, constants), getters hold facts derived from one instance
- `makeFilter` runs once every field decodes, so it reads valid fields, and reports `{ path, issue }` at the field a reader repairs, an array of them every failure of one value
- Decoding stops at the first failure, `{ errors: 'all' }` as the decode options reports every failure where a reader repairs the whole value at once
- Host objects decode at the read through a struct of the members code reads, a host class instance is a `Schema.instanceOf(C)` field and a host function a `Schema.declare(Predicate.isFunction)` field

## [02]-[VARIANTS]

Request, patch, and projection shapes derive from the class, so a field changes in one place:

```ts
const Draft = Invoice.mapFields(Struct.omit(['id', 'issued']));
const Revision = Invoice.mapFields(Struct.mapOmit(['id'], Schema.optionalKey));
```

- `mapFields` takes a `Struct` function (`pick`, `omit`, `assign`, `mapPick` or `mapOmit` with a schema lambda) and returns a `Struct` of the field schemas with their checks, `Schema.Class` over it makes a class
- Struct filters stay on the source, and a variant keeping the fields a filter reads reattaches that filter with `.check`
- `Invoice.extend<Self>('<Id>')(fields)` derives a subclass that adds fields and keeps every check of the base

## [03]-[UNIONS]

Cases with no shared fields are one `Schema.TaggedUnion`. Cases sharing fields are `TaggedStruct` members that `fieldsAssign` extends once through `mapMembers`, and `toTaggedUnion` adds `match`, `matchOrElse`, `guards`, `cases`, and the ordered `discriminants`:

```ts
const Event = Schema.Union([
    Schema.TaggedStruct('Issued', { lines: Invoice.fields.lines }),
    Schema.TaggedStruct('Settled', { reference: Schema.NonEmptyString }),
    Schema.TaggedStruct('Voided', { reason: Schema.Literals(['duplicate', 'dispute', 'error']) }),
]).mapMembers(Tuple.map(Schema.fieldsAssign({ invoice: Invoice.fields.id, at: Schema.DateTimeUtc }))).pipe(Schema.toTaggedUnion('_tag'));

const settledBy = Event.matchOrElse({ Settled: ({ reference }) => Option.some(reference) }, Option.none);
```

- Cases holding derived facts are `Schema.TaggedClass` members with getters, joined through `Schema.Union` and `toTaggedUnion('_tag')`
- `Schema.Literals(Event.discriminants)` decodes a tag parameter, `Schema.Literals(Struct.keys(X.cases))` the tags of a `Schema.TaggedUnion`
- `toTaggedUnion('<key>')` reads a foreign discriminator (`type`, `kind`) from a wire union, the domain keeps the wire's key

## [04]-[TRANSFORMATIONS]

Transformations map the encoded form to the domain type in one schema, so decode and encode are one declaration. The conversion selects the form:

| [INDEX] | [CONVERSION]                          | [FORM]                                                                   |
| :-----: | :------------------------------------ | :----------------------------------------------------------------------- |
|  [01]   | Scalar a host writes as text          | `FiniteFromString`, `DateTimeUtcFromString`, `BigDecimalFromString`      |
|  [02]   | Wire literal to domain literal        | `Schema.Literals([...]).transform([...])`                                |
|  [03]   | Wire key to domain key                | `X.pipe(Schema.encodeKeys({ <field>: '<wire_key>' }))`                   |
|  [04]   | String grammar with typed parts       | `Schema.TemplateLiteralParser` piped into `decodeTo`                     |
|  [05]   | Total conversion both ways            | `decodeTo(to, SchemaTransformation.transform({ decode, encode }))`       |
|  [06]   | Partial or service-reading conversion | `decodeTo(to, SchemaTransformation.transformEffect({ decode, encode }))` |
|  [07]   | Decode-only source                    | `encode: SchemaGetter.forbidden(() => '<reason>')`                       |

```ts
const Line = Invoice.fields.lines.value;

const LineFromText = Schema.TemplateLiteralParser([Line.fields.sku, ' x', Line.fields.quantity, ' @ ', Schema.BigDecimalFromString]).pipe(
    Schema.decodeTo(
        Line,
        SchemaTransformation.transform({
            decode: ([sku, , quantity, , price]) => ({ sku, quantity, price }),
            encode: ({ sku, quantity, price }) => [sku, ' x', quantity, ' @ ', price] as const,
        }),
    ),
);
```

`LineFromText` reads `A-17 x3 @ 12.50` into the line schema `Invoice` declares. Parts whose encoded form is text or a number are the line's own field schemas, matched with their checks, so `x0` fails the quantity range, and `price` parses through `BigDecimalFromString` because its field encodes as a `BigDecimal`:
- Lists joined by one separator decode into `Schema.Array(X)` through `{ decode: SchemaGetter.split(), encode: SchemaGetter.transform(Array.join(',')) }`
- `transformEffect` fails with `new SchemaIssue.InvalidValue({ message }, input, options)`, the issue joins the decode report
- Services a transformation reads appear in `DecodingServices` and `EncodingServices`, the decode effect requires them

## [05]-[OPTIONALITY]

Absence a host returns decodes to `Option`, absence a domain default fills decodes to the value, and the wire shape selects the form:

| [INDEX] | [WIRE]                              | [DECODED]   | [FORM]                                                         |
| :-----: | :---------------------------------- | :---------- | :------------------------------------------------------------- |
|  [01]   | Key missing                         | `Option<A>` | `Schema.OptionFromOptionalKey(A)`                              |
|  [02]   | Key present, value `null`           | `Option<A>` | `Schema.OptionFromNullOr(A)`                                   |
|  [03]   | Key missing or value `null`         | `Option<A>` | `Schema.OptionFromOptionalNullOr(A)`                           |
|  [04]   | Key missing, domain default         | `A`         | `A.pipe(Schema.withDecodingDefaultTypeKey(Effect.succeed(a)))` |
|  [05]   | Key missing or `undefined`, default | `A`         | `A.pipe(Schema.withDecodingDefaultType(Effect.succeed(a)))`    |
|  [06]   | Key a caller omits                  | `?: A`      | `Schema.optionalKey(A)`                                        |

- Defaults are effects producing the decoded value, `Schema.withDecodingDefaultTypeKey(DateTime.now)` stamps an instant field from the clock
- `{ encodingStrategy: 'omit' }` drops a defaulted key on encode, `'passthrough'` writes the value

## [06]-[WIRES]

Field schemas declare the domain type (`Schema.DateTimeUtc`, `Schema.BigDecimal`, `Schema.Duration`), and each wire derives its codec from the class. `toCodecJson` lowers `DateTime`, `BigDecimal`, `Option`, `Duration`, and classes to JSON values, `toCodecStringTree` lowers every leaf to a string, and decoding returns `X`'s type:

| [INDEX] | [WIRE]                              | [CODEC]                                                   |
| :-----: | :---------------------------------- | :-------------------------------------------------------- |
|  [01]   | JSON text (body, header, key entry) | `Schema.fromJsonString(Schema.toCodecJson(X))`            |
|  [02]   | Parsed JSON value                   | `Schema.toCodecJson(X)`                                   |
|  [03]   | Query string                        | `Schema.fromURLSearchParams(Schema.toCodecStringTree(X))` |
|  [04]   | Form body                           | `Schema.fromFormData(Schema.toCodecStringTree(X))`        |
|  [05]   | Contract for a tool, form, or API   | `Schema.toJsonSchemaDocument(X)`                          |
