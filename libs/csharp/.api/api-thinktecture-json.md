# [RASM_API_THINKTECTURE_JSON]

`Thinktecture.Runtime.Extensions.Json` owns System.Text.Json codec selection for Thinktecture-generated owners: one options-level `JsonConverterFactory` reads the generated conversion metadata and routes each owner to its keyed, string, or span-parsable converter. Every decode terminates in the generated static `Validate` rail, so a payload never mints an owner past its invariant.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Thinktecture.Runtime.Extensions.Json`
- package: `Thinktecture.Runtime.Extensions.Json`
- assembly: `Thinktecture.Runtime.Extensions.Json`
- namespaces: `Thinktecture`, `Thinktecture.Text.Json.Serialization`, `Thinktecture.Internal`
- depends: `Thinktecture.Runtime.Extensions`
- rail: wire-json

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: converters and factories under `Thinktecture.Text.Json.Serialization`, `Utf8JsonReaderExtensions` under `Thinktecture`, the decode seam under `Thinktecture.Internal`

| [INDEX] | [SYMBOL]                                                            | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------------ | :------------ | :--------------------------------------------- |
|  [01]   | `ThinktectureJsonConverterFactory`                                  | class         | selects converters from generated metadata     |
|  [02]   | `ThinktectureJsonConverterFactory<T, TKey, TValidationError>`       | class         | binds one keyed owner to its converter         |
|  [03]   | `ThinktectureJsonConverterFactory<T, TValidationError>`             | class         | binds one string-keyed owner to its converter  |
|  [04]   | `ThinktectureSpanParsableJsonConverterFactory<T, TValidationError>` | class         | binds one span-parsable owner to its converter |
|  [05]   | `ThinktectureJsonConverter<T, TKey, TValidationError>`              | class         | keyed codec over the resolved key converter    |
|  [06]   | `ThinktectureJsonConverter<T, TValidationError>`                    | class         | string codec carrying the property-name pair   |
|  [07]   | `ThinktectureSpanParsableJsonConverter<T, TValidationError>`        | class         | zero-allocation UTF-8 codec                    |
|  [08]   | `Utf8JsonReaderExtensions`                                          | static class  | numeric read and write under number handling   |
|  [09]   | `Utf8JsonReaderHelper`                                              | static class  | pooled UTF-8 decode into a `char` span         |
|  [10]   | `IUtf8JsonFactory<T, TValidationError>`                             | interface     | caller-supplied span validation seam           |
|  [11]   | `ObjectFactoryAdapter<T, TValidationError>`                         | struct        | default span seam over the generated factory   |
|  [12]   | `JsonSerializerOptionsExtensions`                                   | static class  | numeric key converter resolution               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: options-level admission and converter selection

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------------------------ | :------- | :------------------------------------------ |
|  [01]   | `ThinktectureJsonConverterFactory()`                                            | ctor     | admits owners carrying no `[JsonConverter]` |
|  [02]   | `ThinktectureJsonConverterFactory(bool)`                                        | ctor     | selects `[JsonConverter]` handling          |
|  [03]   | `ThinktectureJsonConverterFactory(bool, Func<Type, bool>?)`                     | ctor     | adds a per-type span opt-out callback       |
|  [04]   | `ThinktectureJsonConverterFactory.CanConvert(Type)`                             | instance | tests metadata and attribute policy         |
|  [05]   | `ThinktectureJsonConverterFactory.CreateConverter(Type, JsonSerializerOptions)` | instance | binds the keyed, string, or span converter  |

[ENTRYPOINT_SCOPE]: converter overrides on every `Thinktecture*JsonConverter`, and the UTF-8 validation seam

| [INDEX] | [SURFACE]                                                                        | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `ThinktectureJsonConverter<T, TKey, TValidationError>(JsonSerializerOptions)`    | ctor     | resolves the key converter once         |
|  [02]   | `Read(ref Utf8JsonReader, Type, JsonSerializerOptions) -> T?`                    | instance | decodes the key and folds `Validate`    |
|  [03]   | `Write(Utf8JsonWriter, T?, JsonSerializerOptions)`                               | instance | projects `ToValue` onto the wire        |
|  [04]   | `ReadAsPropertyName(ref Utf8JsonReader, Type, JsonSerializerOptions) -> T`       | instance | decodes a dictionary key                |
|  [05]   | `WriteAsPropertyName(Utf8JsonWriter, T, JsonSerializerOptions)`                  | instance | writes a dictionary key                 |
|  [06]   | `Utf8JsonReaderHelper.ValidateFromUtf8<T, TValidationError>`                     | static   | validates a span on the default adapter |
|  [07]   | `Utf8JsonReaderHelper.ValidateFromUtf8<T, TValidationError, TFactory>(TFactory)` | static   | decodes through a caller struct factory |
|  [08]   | `IUtf8JsonFactory<T, TValidationError>.Validate(ReadOnlySpan<char>, out T?)`     | instance | the implementable span seam             |
|  [09]   | `JsonSerializerOptionsExtensions.GetCustomMemberConverter(Type)`                 | static   | resolves the numeric key converter      |

- `ReadAsPropertyName` and `WriteAsPropertyName`: overridden on the string and span-parsable converters alone, so a dictionary keyed on a non-string owner carries no property-name path.
- `Utf8JsonReaderHelper.ValidateFromUtf8`: both overloads take `ref Utf8JsonReader`, `IFormatProvider?`, and `out T?` and return `TValidationError?`; the shorter form supplies `ObjectFactoryAdapter<T, TValidationError>` as its struct factory.
- `Utf8JsonReaderExtensions`: reads extend `ref Utf8JsonReader` and writes extend `Utf8JsonWriter`, each taking a trailing `JsonNumberHandling`; `WriteNumberWithNumberHandling` carries one overload per numeric type, and `GetSingleWithNumberHandling` and `GetDoubleWithNumberHandling` also honor `AllowNamedFloatingPointLiterals`.

[NUMBER_HANDLING_READS]: `GetByteWithNumberHandling` `GetSByteWithNumberHandling` `GetShortWithNumberHandling` `GetUShortWithNumberHandling` `GetIntWithNumberHandling` `GetUIntWithNumberHandling` `GetLongWithNumberHandling` `GetULongWithNumberHandling` `GetSingleWithNumberHandling` `GetDoubleWithNumberHandling` `GetDecimalWithNumberHandling`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `CreateConverter` routes each owner by its `ConversionMetadata`: a span-parsable owner takes `ThinktectureSpanParsableJsonConverter<T, TValidationError>`, a `string` or `ReadOnlySpan<char>` key takes `ThinktectureJsonConverter<T, TValidationError>`, and every other key takes the three-argument keyed converter.
- A non-null validation error throws `JsonException` carrying the error's text, so a rejected payload surfaces at the serializer edge rather than as a half-built owner.
- `IDisallowDefaultValue` owners throw on a null token or null key where every other owner decodes to `default(T)`, and `CanConvert` declines `Nullable<T>` so the framework wrapper keeps that arm.

[STACKING]:
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions.md`): `MetadataLookup.FindMetadataForConversion` filtered on `SerializationFrameworks.SystemTextJson` yields the `ConversionMetadata` this factory dispatches on, `SmartEnumAttribute<TKey>.DisableSpanBasedJsonConversion` is the declaration-side opt-out dropping a string-keyed owner off the span path, and `IObjectFactory<T, TValue, TValidationError>.Validate` is the rail every converter read terminates in.
- `NodaTime.Serialization.SystemTextJson`(`.api/api-nodatime-stj.md`): both append to one `JsonSerializerOptions.Converters` list under first-claim-wins order, the factory ahead of the per-type converters; their type spaces are disjoint, so order never contests.
- `Thinktecture.Runtime.Extensions.MessagePack`(`.api/api-thinktecture-messagepack.md`): the same generated metadata drives the binary rail, and the owner's `SerializationFrameworks` value selects which codecs an object factory serves, so one declaration carries both wires.
- Within the branch, one app-root `SuiteContracts.Wire` expression registers `ThinktectureJsonConverterFactory(skipObjectsWithJsonConverterAttribute: true)` once, so an attribute-wired owner keeps its own converter and every other generated owner rides the options-level factory.
- A span-parsable owner reaching that registration decodes through `Utf8JsonReaderHelper`'s stack-allocated 128-char window with an `ArrayPool<char>` spill above it; a caller supplying a `readonly struct` `IUtf8JsonFactory<T, TValidationError>` to the three-argument `ValidateFromUtf8` gets devirtualized dispatch on the same pooled path.

[LOCAL_ADMISSION]:
- Generated owners cross a wire through one options-level `ThinktectureJsonConverterFactory` registration; a per-owner `[JsonConverter]` attribute is the exception the default constructor already honors.
- Key conversion lives inside the converter, so the generated `Validate` rail sees every inbound key.
- A hot string-keyed wire keeps the span path, opted out at the declaration through `DisableSpanBasedJsonConversion` or at the registration through the factory's `Func<Type, bool>?` callback.

[RAIL_LAW]:
- Package: `Thinktecture.Runtime.Extensions.Json`
- Owns: System.Text.Json converter selection, key codec, and validation-railed decode for Thinktecture-generated owners
- Accept: one options-level factory registration, the three converter families it binds, `JsonNumberHandling`-aware numeric key codecs, and a caller-supplied `IUtf8JsonFactory` struct on the UTF-8 path
- Reject: a hand-written converter for a generated owner, key pre-conversion at the call site, and a second options owner for the same wire
