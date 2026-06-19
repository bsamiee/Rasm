# [RASM_APPHOST_API_THINKTECTURE_JSON]

`Thinktecture.Runtime.Extensions.Json` supplies System.Text.Json converters and
converter factories that route `[ValueObject]`, `[SmartEnum]`, and `[Union]` owners
through generated key conversion, static `Validate` rails, span-based zero-allocation
deserialization, and number-handling-aware key codecs.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Thinktecture.Runtime.Extensions.Json`
- package: `Thinktecture.Runtime.Extensions.Json`
- assembly: `Thinktecture.Runtime.Extensions.Json`
- namespace: `Thinktecture`
- namespace: `Thinktecture.Text.Json.Serialization`
- namespace: `Thinktecture.Internal`
- asset: runtime library
- rail: serialization

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: converter factory family
- rail: serialization

| [INDEX] | [SYMBOL]                                                            | [PACKAGE_ROLE]   | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------------------ | :--------------- | :-------------------------------------- |
|  [01]   | `ThinktectureJsonConverterFactory`                                  | metadata factory | converter selection for any keyed owner |
|  [02]   | `ThinktectureJsonConverterFactory<T, TKey, TValidationError>`       | typed factory    | keyed owner converter creation          |
|  [03]   | `ThinktectureJsonConverterFactory<T, TValidationError>`             | typed factory    | string-keyed owner converter creation   |
|  [04]   | `ThinktectureSpanParsableJsonConverterFactory<T, TValidationError>` | typed factory    | span-parsable converter creation        |

[PUBLIC_TYPE_SCOPE]: converter family
- rail: serialization

| [INDEX] | [SYMBOL]                                                     | [PACKAGE_ROLE]   | [CAPABILITY]                               |
| :-----: | :----------------------------------------------------------- | :--------------- | :----------------------------------------- |
|  [01]   | `ThinktectureJsonConverter<T, TKey, TValidationError>`       | keyed converter  | key decode, validate rail, `ToValue` write |
|  [02]   | `ThinktectureJsonConverter<T, TValidationError>`             | string converter | string codec plus property-name codec      |
|  [03]   | `ThinktectureSpanParsableJsonConverter<T, TValidationError>` | span converter   | zero-allocation UTF-8 validation read      |

[PUBLIC_TYPE_SCOPE]: reader writer and metadata seam family
- rail: serialization

| [INDEX] | [SYMBOL]                                    | [PACKAGE_ROLE]        | [CAPABILITY]                              |
| :-----: | :------------------------------------------ | :-------------------- | :---------------------------------------- |
|  [01]   | `Utf8JsonReaderExtensions`                  | reader/writer surface | number-handling-aware numeric codec       |
|  [02]   | `IUtf8JsonFactory<T, TValidationError>`     | factory contract      | UTF-8 span validation seam                |
|  [03]   | `ObjectFactoryAdapter<T, TValidationError>` | adapter value         | bridges `IObjectFactory` to UTF-8 factory |
|  [04]   | `Utf8JsonReaderHelper`                      | validation projection | UTF-8 span parse plus `Validate` dispatch |
|  [05]   | `JsonSerializerOptionsExtensions`           | options projection    | custom member converter resolution        |

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
|  [01]   | `Read`                                            | `ref Utf8JsonReader`, type, options            | key decode then static `Validate` rail  |
|  [02]   | `Write`                                           | `Utf8JsonWriter`, value, options               | `ToValue` projection write              |
|  [03]   | `ReadAsPropertyName` / `WriteAsPropertyName`      | property-name codec                            | dictionary-key owner support            |
|  [04]   | `ValidateFromUtf8<T, TValidationError>`           | `ref Utf8JsonReader`, provider, `out` result   | span parse plus validation-error return |
|  [05]   | `ValidateFromUtf8<T, TValidationError, TFactory>` | struct factory overload                        | custom UTF-8 factory dispatch           |
|  [06]   | `Get*WithNumberHandling`                          | `ref Utf8JsonReader` plus `JsonNumberHandling` | quoted-number tolerant numeric reads    |
|  [07]   | `WriteNumberWithNumberHandling`                   | writer, numeric value, `JsonNumberHandling`    | string-encoded numeric writes           |
|  [08]   | `GetCustomMemberConverter`                        | options plus member `Type`                     | key-type converter resolution           |

## [04]-[IMPLEMENTATION_LAW]

[CONVERTER_TOPOLOGY]:
- namespaces: `Thinktecture`, `Thinktecture.Text.Json.Serialization`, `Thinktecture.Internal`
- discovery: `MetadataLookup.FindMetadataForConversion` filtered to `SerializationFrameworks.SystemTextJson` object factories
- selection: span-parsable owners take `ThinktectureSpanParsableJsonConverter<T, TValidationError>`; `string` and `ReadOnlySpan<char>` keys take `ThinktectureJsonConverter<T, TValidationError>`; all other keys take `ThinktectureJsonConverter<T, TKey, TValidationError>`
- validation: deserialization routes through the owner's static `Validate`; a non-null validation error surfaces as `JsonException`
- null policy: `IDisallowDefaultValue` owners reject null and null-key payloads during read
- key codec: numeric keys resolve through internal singleton key converters honoring `JsonNumberHandling` via `Utf8JsonReaderExtensions`
- span policy: string-keyed smart enums opt out via `DisableSpanBasedJsonConversion`; the factory callback opts out per type
- nullable structs: the metadata factory declines `Nullable<T>` and defers to the framework's nullable wrapper

[LOCAL_ADMISSION]:
- Generated owners carry a `[JsonConverter]` factory attribute; the metadata factory skips attributed owners by default.
- Options-level registration of the non-generic factory covers owners without attribute wiring.
- Validation failures are typed validation errors projected to `JsonException` at the serializer boundary.
- Key conversion stays inside the converter; callers never pre-convert keys or post-validate values.

[RAIL_LAW]:
- Package: `Thinktecture.Runtime.Extensions.Json`
- Owns: System.Text.Json conversion for Thinktecture-generated owners
- Accept: validation-railed key conversion through factories
- Reject: hand-rolled converters for generated owners
