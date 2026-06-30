# [RASM_BIM_API_THINKTECTURE_JSON]

`Thinktecture.Runtime.Extensions.Json` supplies System.Text.Json converters and
converter factories that route `[ValueObject]`, `[SmartEnum]`, and `[Union]` owners
through generated key conversion, static `Validate` rails, span-based zero-allocation
deserialization, and number-handling-aware key codecs. The converters bind to the generated
owner through its `IObjectFactory<T, TKey, TValidationError>` and `IConvertible<TKey>`
interfaces (emitted by the `Thinktecture.Runtime.Extensions` source generator), so a keyed
owner round-trips JSON without a hand-rolled `JsonConverter` and a failed `Validate` surfaces
as a typed `IValidationError<TValidationError>` projected to `JsonException` at the boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Thinktecture.Runtime.Extensions.Json`
- package: `Thinktecture.Runtime.Extensions.Json`
- version: `10.4.0`
- license: Apache-2.0 (ships `LICENSE.md`)
- assembly: `Thinktecture.Runtime.Extensions.Json`
- namespace: `Thinktecture` (public `Utf8JsonReaderExtensions` number-handling codec)
- namespace: `Thinktecture.Text.Json.Serialization` (public converters and factories)
- namespace: `Thinktecture.Internal` (infrastructure seam: `Utf8JsonReaderHelper`, `IUtf8JsonFactory`, `ObjectFactoryAdapter`, `JsonSerializerOptionsExtensions` — public-but-infrastructure, not subject to SemVer)
- asset: net8.0, net9.0; the net10.0 consumer binds the `lib/net9.0` asset (no net10.0 asset ships, so the bound public surface is the net9.0 one)
- asset: IL-only AnyCPU managed assembly; pairs with the `Thinktecture.Runtime.Extensions` analyzer/source-generator that emits the `IObjectFactory`/`IConvertible` owner interfaces; ALC-safe
- rail: serialization

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: converter factory family
- rail: serialization

All four derive `System.Text.Json.Serialization.JsonConverterFactory`; the three typed factories carry an owner-bound `CanConvert`/`CreateConverter` for one closed owner type, while the non-generic factory is the options-level registration that admits every metadata-bearing owner.

| [INDEX] | [SYMBOL]                                                            | [PACKAGE_ROLE]   | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------ | :--------------- | :-------------------------------------- |
|  [01]   | `ThinktectureJsonConverterFactory`                                  | metadata factory | `JsonConverterFactory`; metadata-driven converter selection for any keyed owner; ctor opt-outs (attribute-skip, span-deserialization callback) |
|  [02]   | `ThinktectureJsonConverterFactory<T, TKey, TValidationError>`       | typed factory    | keyed owner converter creation (`TKey : notnull`)         |
|  [03]   | `ThinktectureJsonConverterFactory<T, TValidationError>`             | typed factory    | string-keyed owner converter creation (`T : IObjectFactory<T, string, TValidationError>, IConvertible<string>`)   |
|  [04]   | `ThinktectureSpanParsableJsonConverterFactory<T, TValidationError>` | typed factory    | span-parsable converter creation (`T : IObjectFactory<T, ReadOnlySpan<char>, TValidationError>`)       |

[PUBLIC_TYPE_SCOPE]: converter family
- rail: serialization

| [INDEX] | [SYMBOL]                                                     | [PACKAGE_ROLE]   | [CAPABILITY]                               |
| :-----: | :----------------------------------------------------------- | :--------------- | :----------------------------------------- |
|  [01]   | `ThinktectureJsonConverter<T, TKey, TValidationError>`       | keyed converter  | `JsonConverter<T>`; `Read` decodes `TKey` then static `Validate`, `Write` projects `IConvertible<TKey>.ToValue`. Constraints: `T : IObjectFactory<T, TKey, TValidationError>, IConvertible<TKey>`; `TKey : notnull`; `TValidationError : class, IValidationError<TValidationError>`. The single keyed converter — `TKey=string` is handled here, NOT a separate two-arg converter type |
|  [02]   | `ThinktectureSpanParsableJsonConverter<T, TValidationError>` | span converter   | `JsonConverter<T>`; zero-allocation UTF-8 span `Read`/`Write` plus the `ReadAsPropertyName`/`WriteAsPropertyName` dictionary-key codec (these property-name overrides live ONLY here). Constraints: `T : IObjectFactory<T, ReadOnlySpan<char>, TValidationError>, IConvertible<ReadOnlySpan<char>>` |

[PUBLIC_TYPE_SCOPE]: reader writer and metadata seam family
- rail: serialization

| [INDEX] | [SYMBOL]                                                       | [PACKAGE_ROLE]        | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------- | :-------------------- | :---------------------------------------- |
|  [01]   | `Thinktecture.Utf8JsonReaderExtensions`                        | reader/writer surface | static-class; 11 `Get<Numeric>WithNumberHandling(this ref Utf8JsonReader, JsonNumberHandling)` reads + 11 `WriteNumberWithNumberHandling(this Utf8JsonWriter, <numeric>, JsonNumberHandling)` writes (byte/sbyte/short/ushort/int/uint/long/ulong/decimal/float/double) |
|  [02]   | `Thinktecture.Internal.IUtf8JsonFactory<T, TValidationError>`  | factory contract      | UTF-8 span validation seam; `T : notnull`, `TValidationError : class, IValidationError<TValidationError>` |
|  [03]   | `Thinktecture.Internal.ObjectFactoryAdapter<T, TValidationError>` | adapter value (readonly struct) | bridges `IObjectFactory<T, ReadOnlySpan<char>, TValidationError>` to `IUtf8JsonFactory`; the default span-parsable factory |
|  [04]   | `Thinktecture.Internal.Utf8JsonReaderHelper`                   | validation projection | static-class; `ValidateFromUtf8` UTF-8 span parse plus `Validate` dispatch |
|  [05]   | `Thinktecture.Internal.JsonSerializerOptionsExtensions`        | options projection    | static-class; `GetCustomMemberConverter(this JsonSerializerOptions, Type)` key-type converter resolution |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: factory registration and selection
- rail: serialization

Factory constructors optionally receive `skipObjectsWithJsonConverterAttribute` and a `Func<Type, bool>?` span-deserialization opt-out callback.

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]              | [CAPABILITY]                                   |
| :-----: | :--------------------------------- | :------------------------ | :--------------------------------------------- |
|  [01]   | `ThinktectureJsonConverterFactory` | parameterless constructor | admits all metadata-bearing owners             |
|  [02]   | `ThinktectureJsonConverterFactory` | attribute-skip flag       | honors or overrides existing `[JsonConverter]` |
|  [03]   | `ThinktectureJsonConverterFactory` | skip flag plus opt-out    | per-type span-deserialization opt-out          |
|  [04]   | `CanConvert`                       | `Type` probe              | metadata presence plus attribute-skip decision |
|  [05]   | `CreateConverter`                  | type plus options         | selects span, string, or keyed converter       |

[ENTRYPOINT_SCOPE]: conversion operations
- rail: serialization

| [INDEX] | [SURFACE]                                         | [CALL_SHAPE]                                   | [CAPABILITY]                            |
| :-----: | :------------------------------------------------ | :--------------------------------------------- | :-------------------------------------- |
|  [01]   | `Read` (both converters)                          | `ref Utf8JsonReader, Type, JsonSerializerOptions` | key/span decode then static `Validate` rail  |
|  [02]   | `Write` (both converters)                         | `Utf8JsonWriter, T?, JsonSerializerOptions`    | `IConvertible.ToValue` projection write |
|  [03]   | `ReadAsPropertyName` / `WriteAsPropertyName`      | property-name codec (span converter only)      | dictionary-key owner support; the keyed `<T,TKey,TValidationError>` converter does NOT override these |
|  [04]   | `Utf8JsonReaderHelper.ValidateFromUtf8<T, TValidationError>`           | `(ref Utf8JsonReader, IFormatProvider?, out T?)` → `TValidationError?` | span parse plus validation-error return (`T : IObjectFactory<T, ReadOnlySpan<char>, TValidationError>`) |
|  [05]   | `Utf8JsonReaderHelper.ValidateFromUtf8<T, TValidationError, TFactory>` | `(ref Utf8JsonReader, IFormatProvider?, TFactory, out T?)` → `TValidationError?` | custom `struct IUtf8JsonFactory` dispatch (`T : notnull`, `TFactory : struct, IUtf8JsonFactory<T, TValidationError>`) |
|  [06]   | `Get<Numeric>WithNumberHandling`                  | `(this ref Utf8JsonReader, JsonNumberHandling)` | quoted-number tolerant numeric reads (11 numeric variants) |
|  [07]   | `WriteNumberWithNumberHandling`                   | `(this Utf8JsonWriter, <numeric>, JsonNumberHandling)` | string-encoded numeric writes (11 overloads) |
|  [08]   | `JsonSerializerOptionsExtensions.GetCustomMemberConverter`            | `(this JsonSerializerOptions, Type memberType)` → `JsonConverter` | key-type converter resolution           |

## [04]-[IMPLEMENTATION_LAW]

[CONVERTER_TOPOLOGY]:
- namespaces: `Thinktecture`, `Thinktecture.Text.Json.Serialization`, `Thinktecture.Internal`
- discovery: `MetadataLookup.FindMetadataForConversion` filtered to `SerializationFrameworks.SystemTextJson` object factories
- selection: span-parsable owners (`IConvertible<ReadOnlySpan<char>>`, opt-in) take `ThinktectureSpanParsableJsonConverter<T, TValidationError>`; every keyed owner — including `string` and `ReadOnlySpan<char>` keys — takes the single `ThinktectureJsonConverter<T, TKey, TValidationError>` with the appropriate `TKey`. There is NO distinct two-type-parameter `ThinktectureJsonConverter`; the string case is the keyed converter with `TKey=string`
- validation: deserialization routes through the owner's static `Validate`; a non-null `IValidationError<TValidationError>` surfaces as `JsonException`
- null policy: `IDisallowDefaultValue` owners reject null and null-key payloads during read
- key codec: numeric dictionary keys resolve through the internal singleton key converters (`ByteKeyConverter`…`DecimalKeyConverter`, internal under `Thinktecture.Text.Json.Serialization`) honoring `JsonNumberHandling` via the public `Thinktecture.Utf8JsonReaderExtensions` number-handling codec
- span policy: string-keyed smart enums opt out via `DisableSpanBasedJsonConversion`; the factory callback opts out per type
- nullable structs: the metadata factory declines `Nullable<T>` and defers to the framework's nullable wrapper

[LOCAL_ADMISSION]:
- Generated owners carry a `[JsonConverter]` factory attribute; the metadata factory skips attributed owners by default.
- Options-level registration of the non-generic factory covers owners without attribute wiring.
- Validation failures are typed validation errors projected to `JsonException` at the serializer boundary.
- Key conversion stays inside the converter; callers never pre-convert keys or post-validate values.

[STACKING]:
- with `Thinktecture.Runtime.Extensions` (the core generator): the `[ValueObject<TKey>]`/`[SmartEnum<TKey>]`/`[Union]` source generator emits the `IObjectFactory<T, TKey, TValidationError>`, `IConvertible<TKey>`, and `IValidationError<TValidationError>` interfaces this package binds against — the converter is the JSON edge of the same generated owner, so the `Bim/semantics/properties#PROPERTY_TEMPLATES` `PropertyKey` `[SmartEnum<string>]` and the `MeasureValue`/`PropertyValue` `[Union]` carriers serialize through `ThinktectureJsonConverter`/`ThinktectureSpanParsableJsonConverter` with their generated `Validate` rail intact, never a hand-rolled `JsonConverter`.
- with `Thinktecture.Runtime.Extensions.MessagePack` (the wire-format sibling): the same generated owner carries BOTH a STJ converter (this package) and a MessagePack formatter (the sibling); the `Bim` exchange wire shapes pick the format at the boundary while the in-memory owner stays one keyed `[ValueObject]`/`[SmartEnum]` — a second hand-authored DTO per wire format is the deleted form.
- with `System.Text.Json`: registration is either the generated per-owner `[JsonConverter(typeof(…Factory))]` attribute the metadata factory skips by default, or the options-level non-generic `ThinktectureJsonConverterFactory` added to `JsonSerializerOptions.Converters` for owners without attribute wiring; `JsonNumberHandling` flows from the options through `Utf8JsonReaderExtensions` so a quoted-number policy is honored on numeric-keyed owners without a custom reader.
- with `LanguageExt.Core`: a deserialization `Validate` failure is a typed `IValidationError<TValidationError>` projected to `JsonException` at the serializer edge; the `Bim` boundary capsule catches that `JsonException` and lowers it onto the `Fin<T>`/`BimFault` rail rather than letting a serializer exception cross into domain code.

[RAIL_LAW]:
- Package: `Thinktecture.Runtime.Extensions.Json`
- Owns: System.Text.Json conversion for Thinktecture-generated owners
- Accept: validation-railed key conversion through factories
- Reject: hand-rolled converters for generated owners, a second per-wire-format DTO, a pre-converted key or post-validated value at the call site
