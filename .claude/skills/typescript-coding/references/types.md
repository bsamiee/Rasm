# [TYPES]

Covers type-level derivation, from types read off their owning value to case tables, brands, schema-generic functions, and string grammars.

## [01]-[DERIVED_TYPES]

Types are read from the value that states them. Parameters take annotations, return types infer, and every other type a consumer names is an extractor over its owner, written inline at each use:

| [INDEX] | [OWNER]                     | [DERIVED_TYPE]                                                      |
| :-----: | :-------------------------- | :------------------------------------------------------------------ |
|  [01]   | Function                    | `ReturnType<typeof f>`, `Parameters<typeof f>[0]`                   |
|  [02]   | `Effect`, `Stream`, `Layer` | `<Module>.Success<typeof x>`, `<Module>.Error`, `<Module>.Services` |
|  [03]   | Service                     | `Context.Service.Shape<typeof Accounts>`                            |
|  [04]   | Schema                      | `typeof X.Type`, `typeof X.Encoded`, `typeof X.DecodingServices`    |
|  [05]   | Field map                   | `Schema.Struct.Type<typeof fields>`                                 |
|  [06]   | Schema union                | `typeof X.cases.<Case>.Type`                                        |
|  [07]   | `_tag` union                | `Types.ExtractTag<U, '<Case>'>`                                     |
|  [08]   | Package options             | `Omit<Options, '<supplied>'> & { readonly <added>: A }`             |
|  [09]   | Keyed table, tuple          | `(typeof table)[K]`, `(typeof list)[number]`                        |

- `Layer.Success` names the services a layer builds, `Layer.Services` the ones it reads
- `Types.ExtractTag` reads one case of a `Data.TaggedEnum` or of an error channel joining errors from other packages
- Parameters taking a package's options or props take its own type with the keys the function supplies omitted, so each option it gains passes through
- Records a factory builds and other modules name are a class with the factory as a static, `ReturnType` reads a result no declaration states

## [02]-[CASES]

Tables declared `as const` state each case, its order, and data no rule decides. Values following a rule over case order derive from each ordinal and a policy parameter callers fix first:

```ts
const tiers = { basic: '#64748b', plus: '#2563eb', pro: '#7c3aed' } as const;
const Tier = Schema.Literals(Struct.keys(tiers));
const Feature = Schema.Literals(['export', 'audit', 'sso']);

const entitlement = (seatBase: number) => (tier: typeof Tier.Type) => {
    const rank = Tier.literals.indexOf(tier) + 1;
    return { accent: tiers[tier], seats: seatBase ** rank, features: Array.take(Feature.literals, rank) };
};
```

New tiers are one row holding an accent, and each gains every lower tier's features and a seat count from one base:
- Key order is case order, `Tier.literals.indexOf` reads the ordinal a rank, cumulative set, or step derives from
- Cases with no data of their own are a `Schema.Literals` tuple, `literals` keeps their order and literal types
- Tables another module keys by the same cases take `as const satisfies Record<typeof Tier.Type, unknown>`, a new case fails each one missing its row
- `Tier.pick(['plus', 'pro'])` derives a subset schema with its own decoder and type

## [03]-[BRANDS]

Brands mark a value a check validated, so each constraint is stated once on a type decoding enforces. Refinements derive from the brand they narrow, and their type intersects both brands:

```ts
const Amount = Schema.Finite.check(Schema.isGreaterThan(0)).pipe(Schema.brand('Amount'));
const Quantity = Amount.check(Schema.isInt()).pipe(Schema.brand('Quantity'));
```

`typeof Quantity.Type` is `number & Brand<'Amount'> & Brand<'Quantity'>`, so a quantity passes to every parameter taking an amount, and decoding one runs both checks:
- Checks one record field reads stay field checks, brands mark a value signatures across a package take
- Identities kept apart from same-typed values are a brand after their format check (`Schema.isULID()`), decoded at their source
- Arithmetic on a branded number yields `number`, `Amount.makeOption(sum)` reruns every check and answers the branded value or `Option.none()`

## [04]-[GENERICS]

Type parameters relate positions: an input to the output, or one input to another. Constraints name the fields a body reads, and results keep each caller's full type:

```ts
const latest = <A extends { readonly at: DateTime.Utc }>(items: Array.NonEmptyReadonlyArray<A>) =>
    Array.max(items, Order.mapInput(DateTime.Order, (item: A) => item.at));
```

- Type parameters read in one position are their constraint's concrete type
- `const` type parameters keep literal types of an array or object argument, `K extends string` infers a literal key alone
- `NoInfer<T>` marks parameters that check against `T`, so one other parameter decides it
- Functions passing an effect through keep `A`, `E`, and `R` as type parameters and add their own error and services to them

Shapes differing in one member schema are one function over that schema, and each of its views flows into the result:

```ts
const Page = <S extends Schema.Constraint>(item: S) => Schema.Struct({ items: Schema.Array(item), cursor: Schema.OptionFromOptionalKey(Schema.String) });
```

`Page(Order)` decodes `ReadonlyArray<Order>`, encodes each order's wire form, and requires the services decoding `Order` requires:
- `Schema.Constraint` admits any schema a body passes to a constructor or decoder, `Schema.Top` one whose methods (`check`, `annotate`) a body calls
- `Schema.ConstraintDecoder<A, R>` fixes the decoded type a body reads, and `R` carries the services decoding requires into the result

## [05]-[TEMPLATE_LITERALS]

Template literal schemas state a string grammar once. Their `Encoded` type checks strings written in code, and decoding yields parts as a typed tuple:

```ts
const Topic = Schema.TemplateLiteralParser([Schema.Literals(['order', 'account']), '.', Schema.Literals(['open', 'closed'])]);
```

- `typeof Topic.Encoded` is the cross product of the axes (`'order.open' | 'account.closed' | ...`), each axis one literal schema
- `typeof Topic.Type` is `readonly ['order' | 'account', '.', 'open' | 'closed']`, a consumer destructures `[entity, , state]`
- `Schema.TemplateLiteral` keeps the decoded value a string where a consumer reads it whole
- Key remapping with an intrinsic type derives a key family from a table (`` [K in keyof typeof table as `on${Capitalize<K>}`] ``)
