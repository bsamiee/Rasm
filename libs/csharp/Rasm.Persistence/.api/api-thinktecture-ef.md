# [RASM_PERSISTENCE_API_THINKTECTURE_EF]

`Thinktecture.Runtime.Extensions.EntityFrameworkCore10` projects Thinktecture-generated Smart
Enums and keyed Value Objects into EF Core 10 value conversion through a model-building convention
plus context/model/entity/property/complex/collection extension surfaces — no hand-written
`HasConversion` per property. The public `Configuration` ADT carries the read-strategy and the
Smart-Enum / keyed-Value-Object max-length strategy that bounds the projected column width, so a
key column's `nvarchar(n)` is policy data, not a per-column annotation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`
- package: `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`
- version: `10.4.0` (rides the `Thinktecture.Runtime.Extensions` 10.4.0 core generator line)
- license: file LICENSE.md (Pawel Gerr; permissive, source on GitHub)
- assembly: `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`
- core package: `Thinktecture.Runtime.Extensions` (the generator + `IObjectFactory`/`IConvertible` contracts)
- namespace: `Thinktecture` (extensions), `Thinktecture.EntityFrameworkCore` (`Configuration`, max-length strategy), `Thinktecture.EntityFrameworkCore.Storage.ValueConversion` (`ThinktectureValueConverterFactory`)
- target: `net10.0` only — single-TFM, binds the `net10.0` consumer directly (no fallback-TFM risk)
- asset: pure-managed runtime library
- rail: store-provider

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: builder extension family (namespace `Thinktecture`)
- rail: store-provider

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]         | [CAPABILITY]                                                        |
| :-----: | :--------------------------------------- | :-------------------- | :----------------------------------------------------------------- |
|  [01]   | `DbContextOptionsBuilderExtensions`      | options builder ext   | `UseThinktectureValueConverters` — convention plugin, context-wide |
|  [02]   | `ModelBuilderExtensions`                 | model builder ext     | `AddThinktectureValueConverters` — bulk per-model registration     |
|  [03]   | `EntityTypeBuilderExtensions`            | entity builder ext    | per-entity, owned-navigation, and complex-property registration    |
|  [04]   | `PropertyBuilderExtensions`              | property builder ext  | `HasThinktectureValueConverter` on one scalar property             |
|  [05]   | `ComplexTypePropertyBuilderExtensions`   | complex property ext  | `HasThinktectureValueConverter` on a complex-type member           |
|  [06]   | `PrimitiveCollectionBuilderExtensions`   | collection element ext | `HasThinktectureValueConverter` on a primitive-collection element  |

[PUBLIC_TYPE_SCOPE]: conversion policy and factory (namespaces `Thinktecture.EntityFrameworkCore*`)
- rail: store-provider

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]        | [CAPABILITY]                                                       |
| :-----: | :-------------------------------- | :------------------- | :---------------------------------------------------------------- |
|  [01]   | `Configuration`                   | conversion policy    | `Default`/`NoMaxLength`; `SmartEnums`, `KeyedValueObjects`, `UseConstructorForRead` |
|  [02]   | `SmartEnumConfiguration`          | smart-enum policy    | `Default`/`NoMaxLength`; carries `ISmartEnumMaxLengthStrategy`     |
|  [03]   | `KeyedValueObjectConfiguration`   | value-object policy  | `NoMaxLength`; carries `IKeyedValueObjectMaxLengthStrategy`        |
|  [04]   | `ISmartEnumMaxLengthStrategy`     | strategy contract    | `GetMaxLength(type, keyType, items) -> MaxLengthChange`            |
|  [05]   | `IKeyedValueObjectMaxLengthStrategy` | strategy contract | `GetMaxLength(type, keyType) -> MaxLengthChange`                  |
|  [06]   | `ThinktectureValueConverterFactory` | converter factory  | `Create<T, TKey>(useConstructorForRead) -> ValueConverter<T, TKey>` |

[PUBLIC_TYPE_SCOPE]: max-length strategy implementations (namespace `Thinktecture.EntityFrameworkCore`)
- rail: store-provider

| [INDEX] | [SYMBOL]                                 | [TYPE_FAMILY]      | [CAPABILITY]                                                  |
| :-----: | :--------------------------------------- | :----------------- | :----------------------------------------------------------- |
|  [01]   | `FixedSmartEnumMaxLengthStrategy`        | smart-enum strategy | `(maxLength, overwriteExisting)` — pins a fixed column width |
|  [02]   | `DefaultSmartEnumMaxLengthStrategy`      | smart-enum strategy | computes width from the widest string-keyed item             |
|  [03]   | `CustomSmartEnumMaxLengthStrategy`       | smart-enum strategy | delegates width to a caller `Func<…, MaxLengthChange>`        |
|  [04]   | `NoOpSmartEnumMaxLengthStrategy`         | smart-enum strategy | leaves the column unbounded                                  |
|  [05]   | `FixedKeyedValueObjectMaxLengthStrategy` | value-object strategy | pins a fixed keyed-value-object column width               |
|  [06]   | `CustomKeyedValueObjectMaxLengthStrategy` | value-object strategy | delegates width to a caller `Func<Type, Type, MaxLengthChange>` |
|  [07]   | `NoOpKeyedValueObjectMaxLengthStrategy`  | value-object strategy | leaves the keyed-value-object column unbounded              |
|  [08]   | `MaxLengthCache`                         | strategy cache     | memoizes the per-type computed `MaxLengthChange`             |
|  [09]   | `MaxLengthChange`                        | strategy result    | `readonly struct`; `None`, `IsSet` — the computed width delta |

The convention plugin (`ThinktectureConventionsPlugin`, `ThinktectureConventionSetPlugin`), its options carrier (`ThinktectureDbContextOptionsExtension`), and the reflection probes (`PropertyInfoExtensions`, `TypeExtensions`) are `internal` and reachable only through the public extension methods above.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: context-wide convention (`DbContextOptionsBuilder`)
- rail: store-provider

| [INDEX] | [SURFACE]                                                                          | [ENTRY_FAMILY]   | [CAPABILITY]                                          |
| :-----: | :--------------------------------------------------------------------------------- | :--------------- | :--------------------------------------------------- |
|  [01]   | `UseThinktectureValueConverters()`                                                 | convention       | installs the plugin with `Configuration.Default`     |
|  [02]   | `UseThinktectureValueConverters(Configuration configuration)`                      | convention       | installs the plugin with an explicit `Configuration` |
|  [03]   | `UseThinktectureValueConverters(bool useThinktectureConverters, bool useConstructorForRead, Action<IConventionProperty>? configure)` | convention | inline flags + per-property convention callback   |
|  [04]   | `UseThinktectureValueConverters<T>(…)` (3 overloads mirroring [01]-[03])            | typed convention | generic `DbContextOptionsBuilder<T>` overloads       |

[ENTRYPOINT_SCOPE]: model/entity/owned/complex bulk registration
- rail: store-provider

| [INDEX] | [SURFACE]                                                                       | [ENTRY_FAMILY]     | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------------------------------------------ | :----------------- | :----------------------------------------------------------- |
|  [01]   | `ModelBuilder.AddThinktectureValueConverters([Configuration | useConstructorForRead, configure])` | model register | adds converters to every property of every entity type |
|  [02]   | `EntityTypeBuilder(<T>).AddThinktectureValueConverters([Configuration | useConstructorForRead, addConvertersForOwnedTypes, configure])` | entity register | per-entity, optionally cascading into owned types |
|  [03]   | `OwnedNavigationBuilder(<TEntity, TRelatedEntity>).AddThinktectureValueConverters(…)` | owned nav register | adds converters for owned-navigation properties         |
|  [04]   | `ComplexPropertyBuilder(<TComplex>).AddThinktectureValueConverters(…)`           | complex register   | adds converters for the members of a complex property        |

[ENTRYPOINT_SCOPE]: per-property registration and direct factory
- rail: store-provider

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY]     | [CAPABILITY]                                              |
| :-----: | :------------------------------------------------------------------------- | :----------------- | :------------------------------------------------------- |
|  [01]   | `PropertyBuilder<T>.HasThinktectureValueConverter([useConstructorForRead | Configuration])` | property register | converter on one scalar property              |
|  [02]   | `ComplexTypePropertyBuilder<T>.HasThinktectureValueConverter(…)`           | complex register   | converter on one complex-type member                     |
|  [03]   | `PrimitiveCollectionBuilder<T>.HasThinktectureValueConverter(…)`           | collection register | converter on one primitive-collection element           |
|  [04]   | `ThinktectureValueConverterFactory.Create<T, TKey>(bool useConstructorForRead = true)` | factory call | builds a `ValueConverter<T, TKey>` directly when EF cannot resolve it |

## [04]-[IMPLEMENTATION_LAW]

[EF_TOPOLOGY]:
- The public surface is exclusively the six `*Extensions` classes in `Thinktecture` plus the `Configuration`/strategy/factory types in `Thinktecture.EntityFrameworkCore*`; everything else (the convention plugins, options extension, reflection probes) is internal.
- `Configuration` is a public sealed value type (`IEquatable`): `Default` selects `SmartEnumConfiguration.Default` (a default computed max-length) and `KeyedValueObjectConfiguration.NoMaxLength` with `UseConstructorForRead = true`; `NoMaxLength` selects unbounded on both axes.
- `SmartEnumConfiguration`/`KeyedValueObjectConfiguration` each carry a `MaxLengthStrategy`; the strategy ADT (`Fixed`/`Default`/`Custom`/`NoOp`) returns a `MaxLengthChange` whose `OverwriteExistingMaxLength` decides whether it overrides an existing `HasMaxLength`. `MaxLengthCache.GetOrCompute*` memoizes the per-type result.
- `ThinktectureValueConverterFactory.Create<T, TKey>` requires `where T : IObjectFactory<T, TKey, ValidationError>, IConvertible<TKey>` — the generated owner's factory + key-conversion contracts — and `useConstructorForRead` selects the constructor over the static `Get` factory on the read path.
- `UseThinktectureValueConverters` registers `ThinktectureDbContextOptionsExtension`, which installs the model-building convention (`IPropertyAddedConvention` et al.) so converters and column widths apply during model finalization; `AddThinktectureValueConverters` applies the same converters imperatively in `OnModelCreating` and skips any property that already has a value converter.

[LOCAL_ADMISSION]:
- `Schema/converters#CONVERTER_RAIL` mounts `.UseThinktectureValueConverters(Configuration.Default)` on the one `ConverterRail.Compose(DbContextOptionsBuilder)` row, so every `[ValueObject]`, `[SmartEnum]`, and keyed `[Union]` column converts with zero hand-written converter classes and a bounded `Configuration.Default` key width; `Configuration.NoMaxLength` is the rejected unbounded form because an unbounded key column fractures the schema fingerprint.
- A single declared property converts through `PropertyBuilder.HasThinktectureValueConverter`, a complex-type member through `ComplexTypePropertyBuilderExtensions`, and a primitive-collection element through `PrimitiveCollectionBuilderExtensions` — a per-column conversion is one builder call, never a converter class.
- Key-column width rides the registration's `Configuration` value: `SmartEnumConfiguration` bounds smart-enum columns and `KeyedValueObjectConfiguration` bounds keyed value-object columns to a declared max-length through their `MaxLengthStrategy`, so the `nvarchar(n)`/`varchar(n)` width is conversion metadata, never a per-entity `HasMaxLength` annotation.
- The convention path (`UseThinktectureValueConverters`) is the default because it applies during model finalization and survives the `dotnet ef dbcontext optimize` compiled-model path byte-identically; `AddThinktectureValueConverters` in `OnModelCreating` is the per-context fallback when DI options configuration is unavailable; properties with an explicit value provider are always skipped by the bulk methods.

[RAIL_LAW]:
- Package: `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`
- Owns: EF Core 10 value-converter registration and key-column max-length policy for Thinktecture Smart Enums and keyed Value Objects
- Accept: `UseThinktectureValueConverters(Configuration.Default)` as the default convention path, `Has*`/`Add*ThinktectureValueConverters` for per-property and per-scope overrides, `Configuration`/strategy types for column-width policy
- Reject: manual `HasConversion` for Thinktecture types, per-entity `HasMaxLength` standing in for a `MaxLengthStrategy`, `Configuration.NoMaxLength` on key columns, duplicate converter registration
