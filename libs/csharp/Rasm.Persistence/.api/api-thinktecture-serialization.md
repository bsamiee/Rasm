# [RASM_PERSISTENCE_API_THINKTECTURE_SERIALIZATION]

`Thinktecture.Runtime.Extensions.Json`, `Thinktecture.Runtime.Extensions.MessagePack`,
and `Thinktecture.Runtime.Extensions.EntityFrameworkCore10` project Thinktecture
smart enums, value objects, and unions across the JSON snapshot codec, the
MessagePack snapshot codec, and EF Core value conversion.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Thinktecture.Runtime.Extensions.Json`
- package: `Thinktecture.Runtime.Extensions.Json`
- assembly: `Thinktecture.Runtime.Extensions.Json`
- namespace: `Thinktecture.Text.Json.Serialization`
- core package: `Thinktecture.Runtime.Extensions`
- asset: runtime library
- rail: snapshot-codec

[PACKAGE_SURFACE]: `Thinktecture.Runtime.Extensions.MessagePack`
- package: `Thinktecture.Runtime.Extensions.MessagePack`
- assembly: `Thinktecture.Runtime.Extensions.MessagePack`
- namespace: `Thinktecture`, `Thinktecture.Formatters`
- core package: `Thinktecture.Runtime.Extensions`
- asset: runtime library
- rail: snapshot-codec

[PACKAGE_SURFACE]: `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`
- package: `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`
- assembly: `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`
- namespace: `Thinktecture`, `Thinktecture.EntityFrameworkCore`
- core package: `Thinktecture.Runtime.Extensions`
- asset: runtime library
- rail: store-provider

## [2]-[PUBLIC_TYPES]

[JSON_TYPES]: System.Text.Json converter surfaces
- rail: snapshot-codec

| [INDEX] | [SYMBOL]                                                            | [PACKAGE_ROLE]    | [CAPABILITY]                   |
| :-----: | :------------------------------------------------------------------ | :---------------- | :----------------------------- |
|   [1]   | `ThinktectureJsonConverterFactory`                                  | converter factory | converts all generated types   |
|   [2]   | `ThinktectureJsonConverterFactory<T, TKey, TValidationError>`       | typed factory     | converts one keyed type        |
|   [3]   | `ThinktectureJsonConverterFactory<T, TValidationError>`             | typed factory     | converts one string-keyed type |
|   [4]   | `ThinktectureJsonConverter<T, TKey, TValidationError>`              | converter         | converts via object factory    |
|   [5]   | `ThinktectureJsonConverter<T, TValidationError>`                    | converter         | converts string-keyed values   |
|   [6]   | `ThinktectureSpanParsableJsonConverter<T, TValidationError>`        | span converter    | parses from UTF-8 spans        |
|   [7]   | `ThinktectureSpanParsableJsonConverterFactory<T, TValidationError>` | span factory      | creates span converters        |
|   [8]   | `Utf8JsonReaderExtensions`                                          | reader extension  | reads and writes key values    |

[MESSAGEPACK_TYPES]: MessagePack formatter surfaces
- rail: snapshot-codec

| [INDEX] | [SYMBOL]                                                            | [PACKAGE_ROLE]     | [CAPABILITY]                  |
| :-----: | :------------------------------------------------------------------ | :----------------- | :---------------------------- |
|   [1]   | `ThinktectureMessageFormatterResolver`                              | formatter resolver | resolves generated formatters |
|   [2]   | `ThinktectureMessagePackFormatter<T, TKey, TValidationError>`       | class formatter    | formats keyed reference types |
|   [3]   | `ThinktectureStructMessagePackFormatter<T, TKey, TValidationError>` | struct formatter   | formats keyed value types     |

[EF_TYPES]: EF Core value-conversion surfaces
- rail: store-provider

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE]       | [CAPABILITY]                     |
| :-----: | :-------------------------------------- | :------------------- | :------------------------------- |
|   [1]   | `DbContextOptionsBuilderExtensions`     | builder extension    | admits converters context-wide   |
|   [2]   | `ModelBuilderExtensions`                | model extension      | adds converters per model        |
|   [3]   | `EntityTypeBuilderExtensions`           | entity extension     | adds converters per entity       |
|   [4]   | `PropertyBuilderExtensions`             | property extension   | converts one property            |
|   [5]   | `ComplexTypePropertyBuilderExtensions`  | complex extension    | converts complex-type properties |
|   [6]   | `PrimitiveCollectionBuilderExtensions`  | collection extension | converts collection elements     |
|   [7]   | `ThinktectureValueConverterFactory`     | converter factory    | creates value converters         |
|   [8]   | `Configuration`                         | conversion policy    | carries converter settings       |
|   [9]   | `SmartEnumConfiguration`                | smart-enum policy    | sets smart-enum max-length       |
|  [10]   | `KeyedValueObjectConfiguration`         | value-object policy  | sets value-object max-length     |
|  [11]   | `ThinktectureConventionsPlugin`         | convention plugin    | installs conversion conventions  |
|  [12]   | `ThinktectureDbContextOptionsExtension` | options extension    | carries plugin policy            |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: JSON codec admission
- rail: snapshot-codec

| [INDEX] | [SURFACE]                                                   | [CALL_SHAPE]        | [CAPABILITY]                 |
| :-----: | :---------------------------------------------------------- | :------------------ | :--------------------------- |
|   [1]   | `new ThinktectureJsonConverterFactory()`                    | options converter   | converts all generated types |
|   [2]   | `ThinktectureJsonConverterFactory(bool)`                    | factory constructor | skips attributed types       |
|   [3]   | `ThinktectureJsonConverterFactory(bool, Func<Type, bool>?)` | factory constructor | gates span deserialization   |
|   [4]   | `CanConvert` / `CreateConverter`                            | factory overrides   | resolves per-type converters |

[ENTRYPOINT_SCOPE]: MessagePack codec admission
- rail: snapshot-codec

| [INDEX] | [SURFACE]                                        | [CALL_SHAPE]         | [CAPABILITY]                  |
| :-----: | :----------------------------------------------- | :------------------- | :---------------------------- |
|   [1]   | `ThinktectureMessageFormatterResolver.Instance`  | static resolver      | resolves generated formatters |
|   [2]   | `new ThinktectureMessageFormatterResolver(bool)` | resolver constructor | skips attributed types        |
|   [3]   | `GetFormatter<T>`                                | resolver call        | returns the typed formatter   |

[ENTRYPOINT_SCOPE]: EF value-converter admission
- rail: store-provider

| [INDEX] | [SURFACE]                                             | [CALL_SHAPE]       | [CAPABILITY]                     |
| :-----: | :---------------------------------------------------- | :----------------- | :------------------------------- |
|   [1]   | `UseThinktectureValueConverters`                      | provider option    | admits converters context-wide   |
|   [2]   | `AddThinktectureValueConverters`                      | model extension    | adds converters in builder scope |
|   [3]   | `HasThinktectureValueConverter`                       | property extension | converts one declared property   |
|   [4]   | `ThinktectureValueConverterFactory.Create`            | factory call       | creates a typed `ValueConverter` |
|   [5]   | `Configuration.Default` / `Configuration.NoMaxLength` | policy value       | selects max-length policy        |

## [4]-[IMPLEMENTATION_LAW]

[CODEC_PROFILE]:
- profile: Thinktecture serialization is codec and store policy for generated domain types
- JSON root: `ThinktectureJsonConverterFactory` on `JsonSerializerOptions.Converters`
- MessagePack root: `ThinktectureMessageFormatterResolver.Instance` composed into the resolver chain
- store root: `UseThinktectureValueConverters` on `DbContextOptionsBuilder`

[LOCAL_ADMISSION]:
- Generated smart enums, value objects, and keyed unions cross codecs only through these surfaces.
- Codec profiles register the factory or resolver once; per-type converters are derived, not declared.
- Store conversion enters through the options builder; per-property overrides stay model-local.
- Max-length policy is conversion metadata and lives in `Configuration`, not in column annotations.

[RAIL_LAW]:
- Packages: `Thinktecture.Runtime.Extensions.Json`, `Thinktecture.Runtime.Extensions.MessagePack`, `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`
- Own: codec and store projection of Thinktecture-generated types
- Accept: factory, resolver, and options-builder admission
- Reject: handwritten converters or formatters for generated types
