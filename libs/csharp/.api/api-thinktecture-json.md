# [RASM_API_THINKTECTURE_JSON]

`Thinktecture.Runtime.Extensions.Json` supplies System.Text.Json converters and converter factories for Thinktecture-generated Value Objects, Smart Enums, and Unions. It reads the generated metadata and factory contracts from `Thinktecture.Runtime.Extensions`, decodes JSON keys through `Utf8JsonReader`, validates through the generated static factory rail, and writes values back through generated key projection. This page owns only the STJ package surface; the core generator and MessagePack bridge live in sibling shared catalogues.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Thinktecture.Runtime.Extensions.Json`
- package: `Thinktecture.Runtime.Extensions.Json`
- assembly: `Thinktecture.Runtime.Extensions.Json`
- bound asset: `lib/net9.0` for `net10.0` consumers
- namespaces: `Thinktecture`, `Thinktecture.Text.Json.Serialization`, `Thinktecture.Internal`
- dependency: `Thinktecture.Runtime.Extensions`
- rail: wire-json

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: converter factory family
- rail: wire-json

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
|:-----: |:------------------------------------------------------------------ |:--------------- |:------------------------------------------------ |
| [01] | `ThinktectureJsonConverterFactory` | options factory | `JsonConverterFactory` selecting generated-owner converters |
| [02] | `ThinktectureJsonConverterFactory<T, TKey, TValidationError>` | typed factory | keyed owner converter creation |
| [03] | `ThinktectureJsonConverterFactory<T, TValidationError>` | typed factory | string and readonly-span owner converter creation |
| [04] | `ThinktectureSpanParsableJsonConverterFactory<T, TValidationError>` | typed factory | span-parsable owner converter creation |

[PUBLIC_TYPE_SCOPE]: converter family
- rail: wire-json

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
|:-----: |:----------------------------------------------------------- |:--------------- |:------------------------------------------------ |
| [01] | `ThinktectureJsonConverter<T, TKey, TValidationError>` | keyed converter | key read/write plus generated static validation |
| [02] | `ThinktectureJsonConverter<T, TValidationError>` | string converter | string/property-name codec for generated owners |
| [03] | `ThinktectureSpanParsableJsonConverter<T, TValidationError>` | span converter | zero-allocation UTF-8 validation read |

[PUBLIC_TYPE_SCOPE]: reader, writer, and metadata seam family
- rail: wire-json

| [INDEX] | [SYMBOL] | [NAMESPACE] | [CAPABILITY] |
|:-----: |:------------------------------------------ |:------------------------------------- |:------------------------------------------------ |
| [01] | `Utf8JsonReaderExtensions` | `Thinktecture` | `JsonNumberHandling`-aware numeric read/write helpers |
| [02] | `IUtf8JsonFactory<T, TValidationError>` | `Thinktecture.Text.Json.Serialization` | custom UTF-8 span validation factory seam |
| [03] | `ObjectFactoryAdapter<T, TValidationError>` | `Thinktecture.Text.Json.Serialization` | bridges `IObjectFactory` into the UTF-8 factory seam |
| [04] | `Utf8JsonReaderHelper` | `Thinktecture.Internal` | `ValidateFromUtf8` span parse plus generated validation dispatch |
| [05] | `JsonSerializerOptionsExtensions` | `Thinktecture.Internal` | `GetCustomMemberConverter` lookup for key-type converters |

The per-numeric-type key converters (`ByteKeyConverter`/`SByteKeyConverter`/`ShortKeyConverter`/`UShortKeyConverter`/`IntKeyConverter`/`UIntKeyConverter`/`LongKeyConverter`/`ULongKeyConverter`/`SingleKeyConverter`/`DoubleKeyConverter`/`DecimalKeyConverter`) are `internal sealed` singletons in `Thinktecture.Internal`, reached only through `Utf8JsonReaderExtensions` (the `JsonNumberHandling`-aware read/write) and `JsonSerializerOptionsExtensions.GetCustomMemberConverter` — they are NOT a public consumption surface and are never constructed directly.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: factory registration and converter selection
- rail: wire-json

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:--------------------------------------------------------- |:------------------------------- |:------------------------------------------------ |
| [01] | `new ThinktectureJsonConverterFactory()` | parameterless constructor | admits all metadata-bearing generated owners |
| [02] | `new ThinktectureJsonConverterFactory(bool skipObjectsWithJsonConverterAttribute)` | constructor | skips or overrides owners already carrying `[JsonConverter]` |
| [03] | `new ThinktectureJsonConverterFactory(bool, Func<Type, bool>?)` | constructor | per-type span-deserialization opt-out |
| [04] | `CanConvert(Type)` | factory override | metadata presence plus attribute-skip decision |
| [05] | `CreateConverter(Type, JsonSerializerOptions)` | factory override | selects span, string, or keyed converter |

[ENTRYPOINT_SCOPE]: conversion operations
- rail: wire-json

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:------------------------------------------------ |:--------------------------------------------- |:------------------------------------------------ |
| [01] | `Read(...)` | reader, type, options | key decode followed by generated `Validate` |
| [02] | `Write(...)` | writer, value, options | generated `ToValue` projection write |
| [03] | `ReadAsPropertyName(...)` / `WriteAsPropertyName(...)` | property-name codec | dictionary-key generated-owner support |
| [04] | `ValidateFromUtf8<T, TValidationError>(...)` | reader plus provider and `out` result | UTF-8 span parse plus typed validation error |
| [05] | `ValidateFromUtf8<T, TValidationError, TFactory>(...)` | struct factory overload | custom UTF-8 factory dispatch |
| [06] | `Get*WithNumberHandling(...)` | reader plus `JsonNumberHandling` | quoted-number tolerant numeric reads |
| [07] | `WriteNumberWithNumberHandling(...)` | writer, numeric value, number handling | string-encoded numeric writes |
| [08] | `GetCustomMemberConverter(...)` | options plus member type | key-type converter resolution |

## [04]-[IMPLEMENTATION_LAW]

[GENERATOR_HANDSHAKE]:
- The core `Thinktecture.Runtime.Extensions` generator emits the owner metadata, `IObjectFactory<T, TKey, TValidationError>`, key conversion, and validation contracts these converters bind. This package does not own declaration attributes or generated domain owners; it owns their STJ projection.
- The non-generic `ThinktectureJsonConverterFactory` is the options-level route. Generated owners may also carry a typed `[JsonConverter]` factory attribute. Both paths terminate in the same converter family.

[VALIDATION_RAIL]:
- Deserialization decodes the JSON key and calls the generated static `Validate` rail. A non-null validation error surfaces at the serializer edge as `JsonException` rather than bypassing the generated owner invariant.
- Span-parsable owners use the UTF-8 span converter path so hot wire reads avoid an intermediate `string`; a string-keyed owner opts out via `DisableSpanBasedJsonConversion`, and the factory's `Func<Type, bool>?` callback opts out per type.
- `IDisallowDefaultValue` owners reject null and null-key payloads during read. The metadata factory declines `Nullable<T>` and defers to the framework's nullable wrapper.

[STACKING]:
- `api-thinktecture-runtime-extensions.md` owns the core source-generator contracts and generated owner surface.
- `api-thinktecture-messagepack.md` owns the binary MessagePack bridge over the same generated metadata.
- `api-nodatime-stj.md` owns NodaTime semantic-time converters. A shared options profile may register both NodaTime and Thinktecture converters because their type spaces are distinct.

[LOCAL_ADMISSION]:
- Register the non-generic factory once on an STJ options profile when generated owners cross that wire without per-owner attribute wiring.
- Keep key conversion inside the converter. Callers do not pre-convert keys, bypass validation, or post-validate generated owners.
- Do not hand-roll STJ converters for generated Value Objects, Smart Enums, or Unions.

[RAIL_LAW]:
- Package: `Thinktecture.Runtime.Extensions.Json`
- Owns: System.Text.Json conversion for Thinktecture-generated owners.
- Accept: validation-railed key conversion through the factory/converter family; `JsonNumberHandling`-aware numeric key codecs; property-name codecs for dictionary keys.
- Reject: hand-written converters for generated owners, direct key pre-conversion at call sites, or treating this JSON package as the owner of core generator attributes.
