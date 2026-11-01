# [TYPES]

Covers types derived from the values that own them.

## [01]-[DERIVED_TYPES]

Return types infer from the body, and every type a consumer names derives at its use from the value that owns it. Packages export the owning value in place of a type:

| [INDEX] | [OWNER]            | [DERIVED_TYPE]                                                      |
| :-----: | :----------------- | :------------------------------------------------------------------ |
|  [01]   | `Effect`, `Stream` | `<Module>.Success<typeof x>`, `<Module>.Error`, `<Module>.Services` |
|  [02]   | `Layer`            | `Layer.Success<typeof x>` built, `Layer.Services` read              |
|  [03]   | Service            | `Context.Service.Shape<typeof X>`                                   |
|  [04]   | Schema             | `typeof X.Type`, `typeof X.Encoded`, `typeof X.DecodingServices`    |
|  [05]   | Field map          | `Schema.Struct.Type<typeof fields>`                                 |
|  [06]   | Schema union       | `typeof X.cases.<Case>.Type`                                        |
|  [07]   | Tagged enum        | `Data.TaggedEnum.Value<X, '<Case>'>`                                |

- Aliases and interfaces stand where no schema, table, or function states the shape, and a `Schema.Class` names its own type
- Package option types pass whole as `Omit<Options, '<supplied>'> & { readonly <added>: A }`, omitting the keys the calling layer fills
- `Types.ExtractTag<E, '<Case>'>` reads one case of an error channel joining errors from other packages
- `Types.ExtractReason<E, '<Reason>'>` reads one reason of a host error whose `reason` is a tagged union (`HttpClientError`, `PlatformError`)

## [02]-[RANKED_CASES]

Ranked cases are one `Schema.Literals` in rank order, and every ranked value (a count, a cumulative set, a step) derives from a case's index and a policy parameter callers fix first:

```ts
const Plan = Schema.Literals(['basic', 'plus', 'pro']);

const allowance =
    <A>(channels: readonly A[], seatBase = 5) =>
    (plan: typeof Plan.Type) => {
        const rank = Plan.literals.indexOf(plan) + 1;
        return { seats: seatBase ** rank, channels: Array.take(channels, rank) };
    };
```

`Channel` is a literal schema of contact channels, and `allowance(Channel.literals)` gives each plan a seat count from one base and every channel up to its rank. New plans are one literal at their rank:
- `Plan.pick(['plus', 'pro'])` derives a subset schema with its own decoder and type
- Ranked cases holding data no rule decides are `as const` table rows, `Struct.keys(table)` returning cases in rank order
- `Schema.TemplateLiteral([Plan, ':', Channel])` joins case axes into one wire key typed as their cross product, with a decoder checking each part

## [03]-[BRANDS]

Brands mark a value a check validated, each constraint stated once on the type decoding enforces. Refinements derive from the brand they narrow, and their type intersects both brands:

```ts
const Amount = Schema.Finite.check(Schema.isGreaterThan(0)).pipe(Schema.brand('Amount'));
const Quantity = Amount.check(Schema.isInt()).pipe(Schema.brand('Quantity'));
```

`typeof Quantity.Type` is `number & Brand<'Amount'> & Brand<'Quantity'>`, a quantity passes to every parameter taking an amount, and decoding one runs both checks:
- Checks one record field reads stay field checks, brands mark a value signatures across a package take
- Identities kept apart from same-typed values are a brand over their format check (`Schema.isULID()`), decoded at their source
- Arithmetic on a branded number yields `number`, and `Quantity.makeOption(sum)` reruns every check and returns the branded value or `Option.none()`

## [04]-[GENERICS]

Shapes differing in one member schema are one function taking the schema:

```ts
const Page = <S extends Schema.Constraint>(item: S) => Schema.Struct({ items: Schema.Array(item), cursor: Schema.OptionFromOptionalKey(Schema.String) });
```

`Page(Invoice)` decodes `readonly Invoice[]`, encodes each invoice's wire form, and requires the services decoding `Invoice` requires:
- `Schema.Constraint` accepts any schema a body passes to a constructor or decoder, `Schema.Top` one a body calls methods on (`check`, `annotate`)
- `Schema.ConstraintDecoder<A, R>` fixes the decoded type a body reads, and `R` adds the services decoding requires to the result
- Functions passing an effect through keep `A`, `E`, and `R` as type parameters and add their own error and services to them
