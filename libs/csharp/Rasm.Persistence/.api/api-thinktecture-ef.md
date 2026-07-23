# [RASM_PERSISTENCE_API_THINKTECTURE_EF]

`Thinktecture.Runtime.Extensions.EntityFrameworkCore10` projects every generated Smart Enum and keyed Value Object onto an EF Core value conversion through one model-building convention, so a key column's converter is the generated expression pair rather than a per-property `HasConversion`. `Configuration` carries the read strategy and the per-axis max-length strategy bounding the projected column, so key-column width is conversion policy the model finalization applies.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`
- package: `Thinktecture.Runtime.Extensions.EntityFrameworkCore10` (LICENSE.md)
- assembly: `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`
- namespace: `Thinktecture`, `Thinktecture.EntityFrameworkCore`, `Thinktecture.EntityFrameworkCore.Internal`, `Thinktecture.EntityFrameworkCore.Storage.ValueConversion`
- depends: `Thinktecture.Runtime.Extensions`, `Microsoft.EntityFrameworkCore.Relational`
- target: `net10.0`
- asset: pure-managed runtime library
- rail: store-provider

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: conversion policy, the column-width strategy family, and the direct converter factory

[EXTENSION_OWNERS]: `DbContextOptionsBuilderExtensions` `ModelBuilderExtensions` `EntityTypeBuilderExtensions` `PropertyBuilderExtensions` `ComplexTypePropertyBuilderExtensions` `PrimitiveCollectionBuilderExtensions`

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [CAPABILITY]                         |
| :-----: | :---------------------------------------- | :------------ | :----------------------------------- |
|  [01]   | `Configuration`                           | class         | policy root carrying both axes       |
|  [02]   | `SmartEnumConfiguration`                  | class         | smart-enum width policy              |
|  [03]   | `KeyedValueObjectConfiguration`           | class         | keyed-value-object width policy      |
|  [04]   | `ISmartEnumMaxLengthStrategy`             | interface     | width from key type and items        |
|  [05]   | `IKeyedValueObjectMaxLengthStrategy`      | interface     | width from owner and key type        |
|  [06]   | `ISmartEnumItem`                          | interface     | item key value and identifier        |
|  [07]   | `MaxLengthChange`                         | struct        | computed width delta                 |
|  [08]   | `DefaultSmartEnumMaxLengthStrategy`       | class         | width of the widest item key         |
|  [09]   | `FixedSmartEnumMaxLengthStrategy`         | class         | pinned smart-enum width              |
|  [10]   | `CustomSmartEnumMaxLengthStrategy`        | class         | caller delegate decides width        |
|  [11]   | `NoOpSmartEnumMaxLengthStrategy`          | class         | unbounded smart-enum column          |
|  [12]   | `FixedKeyedValueObjectMaxLengthStrategy`  | class         | pinned value-object width            |
|  [13]   | `CustomKeyedValueObjectMaxLengthStrategy` | class         | caller delegate decides width        |
|  [14]   | `NoOpKeyedValueObjectMaxLengthStrategy`   | class         | unbounded value-object column        |
|  [15]   | `MaxLengthCache`                          | class         | memoized per-type width delta        |
|  [16]   | `ThinktectureValueConverterFactory`       | class         | direct `ValueConverter` construction |

- Each `Fixed*` strategy takes `(int, bool)` and each `Custom*` a `(Func<…, MaxLengthChange>, bool)` pair whose second argument overwrites an existing width; `Default*` and `NoOp*` expose a static `Instance`.
- `MaxLengthChange` converts implicitly from `int?` and carries `None`, `IsSet`, `Value`; `MaxLengthCache` keys each memo on the strategy instance with the owner type, and `Clear` drops both tables.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: converter registration by builder grain, and the direct factory for a property EF cannot resolve

| [INDEX] | [SURFACE]                                                                 | [SHAPE] | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------------------------ | :------ | :----------------------------------------------- |
|  [01]   | `DbContextOptionsBuilder.UseThinktectureValueConverters(Configuration)`   | static  | installs the convention context-wide             |
|  [02]   | `ModelBuilder.AddThinktectureValueConverters(Configuration)`              | static  | every property of every entity type              |
|  [03]   | `EntityTypeBuilder.AddThinktectureValueConverters(Configuration)`         | static  | one entity, cascading into its owned types       |
|  [04]   | `OwnedNavigationBuilder.AddThinktectureValueConverters(Configuration)`    | static  | owned-navigation properties                      |
|  [05]   | `ComplexPropertyBuilder.AddThinktectureValueConverters(Configuration)`    | static  | members of a complex property                    |
|  [06]   | `PropertyBuilder.HasThinktectureValueConverter(Configuration)`            | static  | one declared scalar property                     |
|  [07]   | `ComplexTypePropertyBuilder.HasThinktectureValueConverter(Configuration)` | static  | one complex-type member                          |
|  [08]   | `PrimitiveCollectionBuilder.HasThinktectureValueConverter(Configuration)` | static  | one primitive-collection element                 |
|  [09]   | `ThinktectureValueConverterFactory.Create<T, TKey>(bool)`                 | factory | `ValueConverter<T, TKey>` over `ValidationError` |
|  [10]   | `Create<T, TKey, TValidationError>(bool)`                                 | factory | owner carrying a custom validation error         |
|  [11]   | `Create(Type, bool)`                                                      | factory | runtime type resolved off metadata               |

- Every registration entry carries a parameterless overload binding `Configuration.Default`; each `DbContextOptionsBuilder<T>` form mirrors its non-generic sibling returning the typed builder, and each `Create` defaults `useConstructorForRead` to `true`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Registration reaches the model only through the extension methods; the convention plugin, options extension, and reflection probes stay internal, so a consumer never binds a plugin type.
- `Configuration.Default` pairs `SmartEnumConfiguration.Default` with `KeyedValueObjectConfiguration.NoMaxLength` under `UseConstructorForRead`, and `Configuration.NoMaxLength` unbinds width on both axes; every carrier is `init`-only and `IEquatable`, so a policy value compares rather than mutates.
- A strategy returns a `MaxLengthChange` whose `OverwriteExistingMaxLength` decides whether it displaces an existing `HasMaxLength`, and the memo keys on the strategy instance, so a strategy that is not value-equal recomputes.
- `Create` constrains `T : IObjectFactory<T, TKey, TValidationError>, IConvertible<TKey>`, and `useConstructorForRead` selects the generated constructor over the static factory on the read path.
- `UseThinktectureValueConverters` applies during model finalization; `AddThinktectureValueConverters` applies the same converters imperatively inside `OnModelCreating` and skips a property already carrying a value converter.

[STACKING]:
- `Thinktecture.Runtime.Extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): the convention selects properties through `MetadataLookup.FindMetadataForConversion` filtered on `ObjectFactoryMetadata.UseWithEntityFramework`, takes `Metadata.Keyed.ConvertFromKeyExpression`/`ConvertToKeyExpression` as the converter pair, and folds `SmartEnumItemMetadata` items into the width strategy — declaration policy decides which owners EF sees.
- `Thinktecture.Runtime.Extensions.Json`(`libs/csharp/.api/api-thinktecture-json.md`) and `.MessagePack`(`libs/csharp/.api/api-thinktecture-messagepack.md`): the JSON converter factory and the MessagePack formatter resolver close over the same generated `ToValue` projection and static `Validate` admission this `ValueConverter` binds, so a stored key column, a JSON payload, and a binary snapshot carry one owner model.
- `Npgsql.EntityFrameworkCore.PostgreSQL`(`api-npgsql-ef.md`) and `Microsoft.EntityFrameworkCore.Sqlite`(`api-ef-sqlite.md`): the convention mounts on the same options builder `UseNpgsql` or `UseSqlite` binds, so provider mapping and generated conversion compose in one options value and a key column converts identically under either row.
- `EFCore.NamingConventions`(`api-ef-naming.md`): both are model-build conventions on the one builder — naming decides the column identifier while this one decides its store type and width, so a scaffolded migration carries both with no second pass.
- `Microsoft.EntityFrameworkCore.Design`(`api-ef-design.md`): migration scaffolding and compiled-model generation read the finalized model, so the converter and width this convention applied emit into both artifacts unchanged.
- within-library: `Configuration` threads a strategy pair into the convention, each strategy folds to a `MaxLengthChange` an `int?` mints implicitly, `MaxLengthCache` memoizes the fold per strategy and type, and `Create` reaches the same converter directly where the model resolves none.

[LOCAL_ADMISSION]:
- `Element/identity#ELEMENT_IDENTITY` mounts `UseThinktectureValueConverters(Configuration.Default)` on the one `ConverterRail.Compose` options composition, so every `[ValueObject]`, `[SmartEnum]`, and keyed `[Union]` column converts under a bounded key width; `Configuration.NoMaxLength` is rejected on key columns because an unbounded key column fractures the schema fingerprint.
- A single declared member converts through the property, complex-type, or primitive-collection entry — one builder call per column, never a converter class.
- Key-column width rides `Configuration`: the smart-enum and keyed-value-object strategies bound the `nvarchar(n)`/`varchar(n)` column as conversion metadata, never a per-entity `HasMaxLength` annotation.

[RAIL_LAW]:
- Package: `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`
- Owns: EF Core value-converter registration and key-column width policy for generated Smart Enums and keyed Value Objects
- Accept: `UseThinktectureValueConverters(Configuration.Default)` as the convention path, the `Add*`/`Has*` entries for per-scope and per-property grain, `Configuration` and its strategies for width policy
- Reject: a hand-written `HasConversion` for a generated owner, a per-entity `HasMaxLength` standing in for a width strategy, `Configuration.NoMaxLength` on a key column, a second registration over the same property
