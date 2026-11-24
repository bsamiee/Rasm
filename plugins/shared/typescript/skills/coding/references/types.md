# [TYPES]

## [01]-[DERIVED_TYPES]

Return types infer from the body, and every type a consumer names derives at its use from the value that owns it. An annotation stays where it adds a fact: a parameter, a recursive return, `unknown` over a host `any`, a host type checking an object literal; one restating the inferred type goes. Packages export the owning value in place of a type:

| [INDEX] | [OWNER]            | [DERIVED_TYPE]                                                            |
| :-----: | :----------------- | :------------------------------------------------------------------------ |
|  [01]   | `Effect`, `Stream` | `<Module>.Success<typeof x>`, `<Module>.Error`, `<Module>.Services`       |
|  [02]   | `Layer`            | `Layer.Success<typeof x>` built, `Layer.Services` read                    |
|  [03]   | Service            | `X['Service']`                                                            |
|  [04]   | Schema class       | `X`, `X['<field>']`, `X['<list>'][number]`                                |
|  [05]   | Schema parameter   | `S['Type']`, `S['Encoded']`, `S['DecodingServices']`                      |
|  [06]   | Union case         | `typeof X.cases.<Case>.Type`, `typeof X.fields.<field>.cases.<Case>.Type` |
|  [07]   | Tagged enum        | `ReturnType<typeof X.<Case>>`, a `$match` handler's parameter             |
|  [08]   | Command config     | `Command.Command.Config.Infer<typeof config>`                             |
|  [09]   | Atom               | `Atom.Type<typeof x>`, `Atom.Success`, `Atom.Failure`                     |
|  [10]   | Generic function   | `ReturnType<typeof f<A>>` at chosen type arguments                        |
|  [11]   | Function parameter | `Parameters<typeof f>[n]`, an exported interface restating it             |
|  [12]   | Bundler `define`   | `ImportMetaEnv['<KEY>']` the reading module declares                      |

- Schema types take their name from a class: the class itself, or an indexed access on the class holding the schema as a field
- `Extract<U, { readonly _tag: '<Case>' }>` restates the case type a union's `cases` holds, spelling its tag as a string
- Schemas no class holds flow by inference: a decoder's return, a `match` handler, a callback written at its call, a generic over the schema
- `typeof X.Type` queries and `type X = typeof X.Type` aliases restate a class name, and a helper type wrapping either restates both
- Unions, brands, literal sets, and encoded sides alone in a signature have no form free of both, a record holds each or its operation derives it
- Declarations only carrying a type to its reader (a class, alias, helper type, or sibling of a member) are indirection and surface, and go
- Aliases and interfaces stand where no schema, table, or function states the shape (a host contract, a recursive union)
- Package option types pass whole (`Types.Concurrency`, a `number` narrowing it), and added fields join as `& { readonly <added>: A }`
- Options a layer fills omit `keyof` the filler's output (`Effect.Success<ReturnType<typeof fill>>`), nested ones their own keys, a literal list going stale
- Host member types read by indexed access (`Viewer['scale']`, `Parameters<Events['error']>[1]`), a restated field type or literal union drifting
- `Parameters` reads an overload set's last signature, one overload reading through `typeof f extends { (o: infer O): unknown; (...rest: never): unknown }`
- Host constants the typings declare and the runtime omits are literals `satisfies typeof <type-only import>`, failing to compile when a release changes one
- Structs whose field types depend on each field's own type map through `Struct.map(record, Struct.lambda<L>(f))`, `Struct.Apply<L, V>` naming one output
- Per-member types of a generic schema factory map its type parameter (`Schema.Struct<{ readonly [K in keyof E]: ... }>['Type']`), where `typeof f<M>` reads the constraint
- Code that loads no Effect types its wire through a type-only import of the wire's schema class, its `switch` closed by `x satisfies never`
- Annotations over a value typed `any` (`(): T =>` over a host `any`, `decodeTo(Schema.Any)`) are casts no check sees, a schema decodes the value
- Key shapes are `Pick<X, '<key>'>`, a hand interface restating fields
- Host and package schemas take their type from the member reading them (`Parameters<Client['Service']['complete']>[0]`)
- `Types.ExtractTag<E, '<Case>'>` reads one case of an error channel joining errors from other packages
- `Types.ExtractReason<E, '<Reason>'>` reads one reason of an error with a tagged union `reason`

## [02]-[ORDERED_LITERALS]

Ordered literals are one `Schema.Literals` in rank order, named through the record holding them, and every value a rank decides (a count, a cumulative set, a step) derives from a literal's index and a policy parameter callers fix first:

```ts
const Durability = Schema.Literals(['zonal', 'regional', 'global']);

class Volume extends Schema.Opaque<Volume, { readonly brand: unique symbol }>()(Schema.Struct({ name: Schema.NonEmptyString, durability: Durability })) {}

const placement =
    <A>(regions: readonly A[], replicaBase: number) =>
    (durability: Volume['durability']) => {
        const rank = Durability.literals.indexOf(durability) + 1;
        return { replicas: replicaBase * rank, regions: Array.take(regions, rank) };
    };
```

`Region` is a literal schema of deployment regions, and `placement(Region.literals, replicaBase)` gives each durability level a replica count from one base and every region up to its rank. New levels are one literal at their rank:
- Literal sets no record holds type their operations through a generic over the schema, `<const L extends readonly string[]>(levels: Schema.Literals<L>) => (level: L[number]) => ...`
- `Durability.pick(['regional', 'global'])` derives a subset schema with its own decoder and type
- Ordered literals holding data no rule decides are `as const` table rows, `Struct.keys(table)` returning cases in rank order
- `Schema.TemplateLiteral([Durability, ':', Region])` types a key as the axes' cross product, `TemplateLiteralParser(Key.parts)` decoding its parts
- Literal schemas over a set the host enumerates (`Intl.supportedValuesOf`) build from the query, a `readonly string[]` checking membership with `Type` as `string`

## [03]-[BRANDS]

Brands mark a value a check validated, each constraint stated once on the type decoding enforces. Refinements derive from the brand they narrow, and their type intersects both brands:

```ts
const Amount = Schema.Finite.check(Schema.isGreaterThan(0)).pipe(Schema.brand('Amount'));
const Quantity = Amount.check(Schema.isInt()).pipe(Schema.brand('Quantity'));
```

Decoded `Quantity` values are `number & Brand<'Amount'> & Brand<'Quantity'>`, a quantity passes to every parameter taking an amount, and decoding one runs both checks:
- Checks one record field reads stay field checks, brands mark a value signatures across a package take
- Bounds use their module's check (`Schema.isBetweenDate`, `isBetweenLength(n, n)` for an exact size), or `Schema.makeIsGreaterThan({ order })` for another `Order`
- `Schema.makeFilter` states a rule no `Order` states
- Identities kept apart from same-typed values are a brand over their format check (`Schema.isULID()`) decoded at their source
- Brands take their type from the record holding them (`Line['quantity']`, `Entry['id']`), a class over a primitive failing to compile
- Brands no record holds are a field missing from its record or a check their one reader decodes with, a hand `Brand.Branded` type restating them
- Arithmetic on a branded number yields `number`, and `Quantity.makeOption(sum)` reruns every check and returns the branded value or `Option.none()`

## [04]-[GENERICS]

Shapes differing in one member schema are one function taking the schema:

```ts
const Page = <S extends Schema.Constraint>(item: S) => Schema.Struct({ items: Schema.Array(item), cursor: Schema.OptionFromOptionalKey(Schema.String) });
```

`Page(Invoice)` decodes `readonly Invoice[]`, encodes each invoice to its encoded form, and requires the services decoding `Invoice` requires:
- Type parameters constrained to a schema read its views by indexed access (`S['Type']`, `S['Encoded']`)
- `Schema.Constraint` accepts any schema a body passes to a constructor or decoder, `Schema.Top` one a body calls methods on (`check`, `annotate`)
- `Schema.ConstraintDecoder<A, R>` fixes the decoded type a body reads, and `R` adds the services decoding requires to the result
- `Schema.Schema<A>`, `Decoder<A, R>`, or `Codec<A, E>` fix it where a body passes the schema to an API taking `Top`, a `Top & { Type }` restating them
- Functions passing an effect through keep `A`, `E`, and `R` as type parameters and add their own error and services to them
- Parameters a type condition requires or lets callers omit are a conditional rest tuple (`...[options]: C extends true ? [O?] : [O & Required]`)

Operations reading an option a constructor leaves optional bound the instance's option type, and a missing option fails to compile:

```ts
class Channel<const O extends { readonly unit: string; readonly calibration?: { readonly gain: number; readonly offset: number } }> extends Data.Class<{ readonly name: string; readonly options: O }> {
    calibrated(this: Channel<O & { readonly calibration: NonNullable<O['calibration']> }>, raw: number) {
        return raw * this.options.calibration.gain + this.options.calibration.offset;
    }
}
```

`new Channel({ name: 'strain', options: { unit: 'µε' } }).calibrated(raw)` fails to compile, where an `Option` field read through `Effect.fromOption` fails the call at run time:
- `const` type parameters keep each literal option on the instance type, and a method's `this` bound reads it
