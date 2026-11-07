# [TYPES]

## [01]-[DERIVED_TYPES]

Return types infer from the body, and every type a consumer names derives at its use from the value that owns it. Packages export the owning value in place of a type:

| [INDEX] | [OWNER]            | [DERIVED_TYPE]                                                      |
| :-----: | :----------------- | :------------------------------------------------------------------ |
|  [01]   | `Effect`, `Stream` | `<Module>.Success<typeof x>`, `<Module>.Error`, `<Module>.Services` |
|  [02]   | `Layer`            | `Layer.Success<typeof x>` built, `Layer.Services` read              |
|  [03]   | Service            | `X['Service']`                                                      |
|  [04]   | Schema             | `typeof X.Type`, `typeof X.Encoded`, `typeof X.DecodingServices`    |
|  [05]   | Field map          | `Schema.Struct.Type<typeof fields>`                                 |
|  [06]   | Schema union       | `typeof X.cases.<Case>.Type`                                        |
|  [07]   | Tagged enum        | `Data.TaggedEnum.Value<X, '<Case>'>`                                |
|  [08]   | Command config     | `Command.Command.Config.Infer<typeof config>`                       |
|  [09]   | Atom               | `Atom.Type<typeof x>`, `Atom.Success`, `Atom.Failure`               |
|  [10]   | Generic function   | `ReturnType<typeof f<A>>` at chosen type arguments                  |
|  [11]   | Function parameter | `Parameters<typeof f>[n]`, an exported interface restating it       |
|  [12]   | Bundler `define`   | `ImportMetaEnv['<KEY>']` the reading module declares                |

- Aliases and interfaces stand where no schema, table, or function states the shape, and a `Schema.Class` names its own type
- Package option types pass whole (`Types.Concurrency`, a `number` narrowing it), and added fields join as `& { readonly <added>: A }`
- Options a layer fills omit `keyof` the filler's output (`Effect.Success<ReturnType<typeof fill>>`), nested ones their own keys, a literal list going stale
- Host member types read by indexed access (`Viewer['scale']`, `Parameters<Events['error']>[1]`), a restated field type drifting from its host
- `Parameters` reads an overload set's last signature, one overload reading through `typeof f extends { (o: infer O): unknown; (...rest: never): unknown }`
- Host constants the typings declare and the runtime omits are literals `satisfies typeof <type-only import>`, failing to compile when a release changes one
- Structs whose field types depend on each field's own type map through `Struct.map(record, Struct.lambda<L>(f))`, `Struct.Apply<L, V>` naming one output
- Per-member types of a generic schema factory are a same-named alias distributing `Schema.Struct<{...}>['Encoded']`, where `typeof f<M>` reads the constraint
- Code that loads no Effect types wire cases through a type-only import as `Extract<typeof X.Encoded, { readonly _tag: Tag }>`, its `switch` closed by `x satisfies never`
- Annotations over a value typed `any` (`(): T =>` over a host `any`, `decodeTo(Schema.Any)`) are casts no check sees, a schema decodes the value
- Key shapes are `Pick<typeof X.Type, '<key>'>`, a hand interface restating fields
- `Types.ExtractTag<E, '<Case>'>` reads one case of an error channel joining errors from other packages
- `Types.ExtractReason<E, '<Reason>'>` reads one reason of an error with a tagged union `reason`

## [02]-[ORDERED_LITERALS]

Ordered literals are one `Schema.Literals` in rank order, and every value a rank decides (a count, a cumulative set, a step) derives from a literal's index and a policy parameter callers fix first:

```ts
const Durability = Schema.Literals(['zonal', 'regional', 'global']);

const placement =
    <A>(regions: readonly A[], replicaBase: number) =>
    (durability: typeof Durability.Type) => {
        const rank = Durability.literals.indexOf(durability) + 1;
        return { replicas: replicaBase * rank, regions: Array.take(regions, rank) };
    };
```

`Region` is a literal schema of deployment regions, and `placement(Region.literals, replicaBase)` gives each durability level a replica count from one base and every region up to its rank. New levels are one literal at their rank:
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

`typeof Quantity.Type` is `number & Brand<'Amount'> & Brand<'Quantity'>`, a quantity passes to every parameter taking an amount, and decoding one runs both checks:
- Checks one record field reads stay field checks, brands mark a value signatures across a package take
- Bounds use their module's check (`Schema.isBetweenDate`, `isBetweenLength(n, n)` for an exact size), or `Schema.makeIsGreaterThan({ order })` for another `Order`
- `Schema.makeFilter` states a rule no `Order` states
- Identities kept apart from same-typed values are a brand over their format check (`Schema.isULID()`) decoded at their source, typed `typeof Id.Type`
- Arithmetic on a branded number yields `number`, and `Quantity.makeOption(sum)` reruns every check and returns the branded value or `Option.none()`

## [04]-[GENERICS]

Shapes differing in one member schema are one function taking the schema:

```ts
const Page = <S extends Schema.Constraint>(item: S) => Schema.Struct({ items: Schema.Array(item), cursor: Schema.OptionFromOptionalKey(Schema.String) });
```

`Page(Invoice)` decodes `readonly Invoice[]`, encodes each invoice to `typeof Invoice.Encoded`, and requires the services decoding `Invoice` requires:
- `Schema.Constraint` accepts any schema a body passes to a constructor or decoder, `Schema.Top` one a body calls methods on (`check`, `annotate`)
- `Schema.ConstraintDecoder<A, R>` fixes the decoded type a body reads, and `R` adds the services decoding requires to the result
- `Schema.Schema<A>`, `Decoder<A, R>`, or `Codec<A, E>` fix it where a body passes the schema to an API taking `Top`, a `Top & { Type }` restating them
- Functions passing an effect through keep `A`, `E`, and `R` as type parameters and add their own error and services to them
- Parameters a type condition requires or lets callers omit are a conditional rest tuple (`...[options]: C extends true ? [O?] : [O & Required]`)

Operations reading an option a constructor leaves optional bound the instance's option type, and a missing option fails to compile:

```ts
type Options = { readonly unit: string; readonly calibration?: { readonly gain: number; readonly offset: number } };

class Channel<const O extends Options = Options> extends Data.Class<{ readonly name: string; readonly options: O }> {
    static readonly make = <const O extends Options>(name: string, options: O) => new Channel<O>({ name, options });

    calibrated(this: Channel<Options & { readonly calibration: NonNullable<Options['calibration']> }>, raw: number) {
        return raw * this.options.calibration.gain + this.options.calibration.offset;
    }
}
```

`Channel.make('strain', { unit: 'µε' }).calibrated(raw)` fails to compile, where an `Option` field read through `Effect.fromOption` fails the call at run time:
- `const` type parameters keep each literal option on the instance type, and a method's `this` bound reads it
