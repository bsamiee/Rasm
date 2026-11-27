# [TYPES]

## [01]-[DERIVED_TYPES]

Return types infer from the body, and every type a consumer names derives at its use from the value owning it. Annotations stay where they add a fact (a parameter, a recursive return, `unknown` over a host `any`, a host type checking an object literal), and one restating an inferred type goes. Packages export the owning value in place of a type:

| [INDEX] | [OWNER]            | [DERIVED_TYPE]                                                                   |
| :-----: | :----------------- | :------------------------------------------------------------------------------- |
|  [01]   | `Effect`, `Stream` | `<Module>.Success<typeof x>`, `<Module>.Error`, `<Module>.Services`              |
|  [02]   | `Layer`            | `Layer.Success<typeof x>` built, `Layer.Services` read                           |
|  [03]   | Service            | `X['Service']`, `keyof X['Service']` for a member name                           |
|  [04]   | Schema class       | `X`, `X['<field>']`, `X['<list>'][number]['<field>']`                            |
|  [05]   | Schema parameter   | `S['Type']`, `S['Encoded']`, `S['DecodingServices']`                             |
|  [06]   | Union case         | `typeof X.cases.<Case>.Type`, `typeof X.fields.<field>.cases.<Case>.Type`        |
|  [07]   | Tagged union type  | `Data.TaggedEnum.Value<X, '<Case>'>`, `ReturnType<typeof X.<Case>>` with no type |
|  [08]   | Joined error       | `Data.TaggedEnum.Value<Types.ReasonOf<E>, '<Reason>'>` for one reason            |
|  [09]   | Command config     | `Command.Command.Config.Infer<typeof config>`                                    |
|  [10]   | Atom               | `Atom.Type<typeof x>`, `Atom.Success`, `Atom.Failure`                            |
|  [11]   | Generic function   | `ReturnType<typeof f<A>>` at chosen type arguments                               |
|  [12]   | Function parameter | `Parameters<typeof f>[n]`, an exported interface restating it                    |

`Data.TaggedEnum.Value` constrains its tag to the union's tags, where `Extract<U, { readonly _tag: '<Case>' }>`, `Types.ExtractTag`, and `Types.ExtractReason` take any string and yield `never` for a misspelled one:
- Schema types take their name from a class, the class itself or an indexed access through its holder (`Composition['items'][number]['asset']`)
- Aliases (`type X = typeof X.Type`), helper types, and second classes naming a schema's type restate its class
- Schemas no class holds flow by inference: a decoder's return, a `match` handler, a callback written at its call, a generic over the schema
- Unions, brands, literal sets, and encoded sides alone in a signature take their record, the case they read, or their own fold's parameter
- Declarations carrying only a type to its reader go, an alias or interface stating a shape no schema or function states (a host contract)
- Package option types pass whole (`Types.Concurrency`, never a `number` narrowing it), and added fields join as `& { readonly <added>: A }`
- Options a layer fills omit the keys of what fills them (`Omit<D1Client.D1ClientConfig, keyof Dialect['names']>`), a literal key list going stale
- Host calls typed `any` return through `(): unknown =>` into a decode, an `any` annotated as a host type or decoded by `Schema.Any` being a cast
- Fields typed by their own value map through `Struct.map(record, Struct.lambda<L>(f))`, `Struct.Apply<L, V>` naming one output in a mapped type
- Code loading no Effect checks objects it builds against a type-only import of their class (`satisfies Pick<Unmatched, '_tag' | 'subject'>`)

### [01.1]-[HOST_MEMBERS]

Host members are read and called on an object the host's type package types or a struct schema decoding the host types, so a renamed or removed member fails to compile at each read. Strings naming a member, method, or event (`const TOOL = 'toolType'`, `app[TOOL]()`, an event named `'toolType'`) are refused with or without `satisfies keyof`, as rename and find-references miss them and each member gains a second name:
- Members read, write, and call by property access on the typed host value (`toolBox.currentToolName`, `project.toolType = value`)
- Names a result prints come from the host object (`menuActions.constructorName`) or its schema's keys (`Struct.keys(Reference.fields)`)
- Literal copies of a host value restate it, a `satisfies typeof <type-only import>` check over the copy included
- Key sets derive from the host's value (`Schema.Literals(Struct.keys(GlyphClass))`), `Record.findFirst(ToolType, (v) => v === code)` naming a code
- Intrinsic string types filter `keyof` by spelling, `Extract<keyof typeof E, Uppercase<string>>` keeping each member's `UPPER_SNAKE` alias
- Tables keyed by host members state their key type, `satisfies Record<Keys, A>` requiring a row per key and `Partial<Record<Keys, A>>` a subset
- Host member types read by indexed access (`NonNullable<Parameters<PDFViewer['setDocument']>[0]>`), a restated field type or literal union drifting
- `Parameters` and `ReturnType` read an overload set's last signature

## [02]-[LITERAL_SETS]

One `Schema.Literals` states a closed set once, and every other view of the set derives from it:

```ts
const LineAlignment = Schema.Literals(['start', 'center', 'end', 'justified']);
const Alignment = Schema.TaggedUnion({
    Aligned: { side: LineAlignment.pick(['start', 'center', 'end']) },
    Justified: { last: LineAlignment, single: LineAlignment },
});
```

`pick` derives a subset schema with its own decoder and type, so `Aligned` refuses `justified` at decode and a literal the set lacks fails to compile. `Justified` sets its last line and a line holding one word to any member:
- Readers of a set read `literals` (`Config.Literals(Unification.literals, name)`, completion values, menu options)
- Literal sets no record holds type their operations generic over them, `<const T extends string>(literals: readonly T[])` keeping each literal
- Rows holding data no rule decides are `as const` table rows, `Schema.Literals(Struct.keys(table))` decoding row keys in table order
- `Schema.TemplateLiteral([Schema.Literals(['1', '12']), Schema.Literals(['m', 's'])])` types a product of parts, `TemplateLiteralParser` parsing each
- Sets a host enumerates at run time (`Intl.supportedValuesOf('numberingSystem')`) build from the query, typed `string` and checked at decode

Forbidden forms take the rebuild in their row:

| [INDEX] | [FORBIDDEN]                                                   | [REBUILD]                                            |
| :-----: | :------------------------------------------------------------ | :--------------------------------------------------- |
|  [01]   | Order by a hand list (`['foreground', ...].indexOf(run)`)     | `X.fields.run.literals.indexOf(run)` over the set    |
|  [02]   | Positional read naming a member (`Channel.literals[0]`)       | Literal spelled where a field typed by the set reads |
|  [03]   | Copied member list beside the set (completion, menu, setting) | `X.literals` read by each consumer                   |

## [03]-[BRANDS]

Brands mark a value a check validated and keep it apart from values of the same primitive type. Each constraint is stated once on the schema decoding enforces, and the value decodes at its source:

```ts
class Instance extends Schema.Class<Instance, { readonly brand: unique symbol }>('Instance')({ instance: Schema.String.check(Schema.isPattern(/^\d+@\d+$/u)).pipe(Schema.brand('ProcessInstance')), url: Schema.URL }) {}
```

Field `instance` holds a process ID and its kernel start time, unequal for a later process reusing the ID. Readers name it `Instance['instance']`, and a bare pid string passed where an instance belongs fails to compile:
- Brands take their type from the record holding them, a class over a primitive failing to compile
- Brands no record holds are a field missing from its record or a check their one reader decodes with, a hand `Brand.Branded` type restating them
- Refinements derive from the brand they narrow (`Base.check(rule).pipe(Schema.brand('<Narrow>'))`), carrying both brands and both checks
- Checks one record field reads stay field checks, and brands mark a value signatures across a package take
- Bounds use their module's check (`Schema.isBetweenLength(n, n)` for an exact size, `Schema.makeIsGreaterThan({ order })` for another `Order`)
- Arithmetic on a branded number yields `number`, and `X.makeOption(value)` reruns every check and returns the branded value or `Option.none()`

## [04]-[GENERICS]

Shapes differing in one member schema are one function taking the schema:

```ts
const recorded = <S extends Schema.Codec<unknown, string>>(stamp: S) => Schema.String.pipe(Schema.decodeTo(stamp.pipe(Schema.toType, Schema.Option), { decode: stamp.pipe(Schema.decodeOption, SchemaGetter.transform), encode: SchemaGetter.forbiddenEncoding }));
```

`recorded(Schema.fromJsonString(TableStamp))` decodes the stamp a ClickHouse table comment holds to `Option<TableStamp>`, a comment holding none reading as absent, and the bound refuses a schema whose encoded side is not text:
- Type parameters constrained to a schema read its views by indexed access (`S['Type']`, `S['Encoded']`)
- `Schema.Constraint` accepts a schema a body passes to a constructor or decoder, `Schema.Top` one a body calls methods on (`check`, `pipe`)
- `ConstraintDecoder<A, R>`, `ConstraintEncoder<I, R>`, and `ConstraintCodec<A, I, RD, RE>` fix the views a body reads, services joining the result
- `Schema.Schema<A>`, `Decoder<A, R>`, or `Codec<A, I>` fix them where a body passes the schema to an API taking `Top`, never `Top & { Type }`
- `Schema.WithoutConstructorDefault` bounds a schema a function adds a constructor default to
- Functions passing an effect through keep `A`, `E`, and `R` as type parameters and add their own error and services to them

Operations reading an option a constructor leaves optional bound the instance's option type, and a missing option fails to compile:

```ts
class Relation<const Shape extends Fields, const Options extends RelationOptions<Shape>> extends Data.Class<{ readonly schema: Schema.Struct<Shape>; readonly options: Option.Option<Options> }> {
    static readonly withTenant = <F extends Fields, O extends RelationOptions<F> & { readonly tenant: NonNullable<RelationOptions<F>['tenant']> }, A, E, R>(relation: Relation<F, O>, id: F[O['tenant']['column']]['Type'], effect: Effect.Effect<A, E, R>) =>
        scoped(relation, id, effect);
}
```

`Fields` is a record of column schemas, `RelationOptions<F>` the table options with an optional `tenant` naming a column and a setting, and `scoped` runs `effect` with the tenant setting inside one transaction. `withTenant` types `id` as the decoded type of the column `tenant` names, and a relation declared without `tenant` fails to compile where an `Option` read fails the call at run time:
- `const` type parameters keep each literal option on the instance type, and an operation's bound on `O` reads it
- Key types derive from fields: `{ readonly [K in keyof F]: null extends F[K]['Encoded'] ? never : K }[keyof F]` admits non-null columns alone
- Written rows omit derived columns by key (`Schema.Struct.Type<Omit<F, keyof O['projections']>>`), a second field list restating them
- Parameters a type condition requires or lets callers omit are a conditional rest tuple (`...[o]: 'id' extends Present<F> ? [O?] : [O & { key }]`)
