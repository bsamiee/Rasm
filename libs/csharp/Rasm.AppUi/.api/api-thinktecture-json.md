# [RASM_APPUI_API_THINKTECTURE_JSON]

`Thinktecture.Runtime.Extensions.Json` supplies System.Text.Json converters and converter factories that route `[ValueObject]`, `[SmartEnum]`, and `[Union]` owners through generated key conversion, static `Validate` rails, span-based zero-allocation deserialization, and number-handling-aware key codecs.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Thinktecture.Runtime.Extensions.Json`
- package: `Thinktecture.Runtime.Extensions.Json` `10.4.0`
- assembly: `Thinktecture.Runtime.Extensions.Json` (bound asset `lib/net9.0/...` — the package ships `net8.0`+`net9.0` only; `net9.0` is the TFM-precedence pick for the `net10.0` consumer, so no net10-specific surface exists)
- license: MIT
- namespace: `Thinktecture` (public `Utf8JsonReaderExtensions`)
- namespace: `Thinktecture.Text.Json.Serialization` (the converters, the typed factories, and the `IUtf8JsonFactory`/`ObjectFactoryAdapter` seam)
- namespace: `Thinktecture.Internal` (infrastructure: `Utf8JsonReaderHelper`, `JsonSerializerOptionsExtensions`, the per-numeric-type key converters)
- depends: `Thinktecture.Runtime.Extensions` (the source-generator core that emits the `[ValueObject]`/`[SmartEnum]`/`[Union]` owners + `IObjectFactory`/`IValidationError` contracts these converters bind)
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

| [INDEX] | [SYMBOL]                                    | [NAMESPACE]                           | [CAPABILITY]                              |
| :-----: | :------------------------------------------ | :------------------------------------ | :---------------------------------------- |
|  [01]   | `Utf8JsonReaderExtensions`                  | `Thinktecture`                        | number-handling-aware numeric read/write extensions |
|  [02]   | `IUtf8JsonFactory<T, TValidationError>`     | `Thinktecture.Text.Json.Serialization`| custom UTF-8 span validation factory seam |
|  [03]   | `ObjectFactoryAdapter<T, TValidationError>` | `Thinktecture.Text.Json.Serialization`| struct adapter bridging `IObjectFactory` to the UTF-8 factory |
|  [04]   | `Utf8JsonReaderHelper`                      | `Thinktecture.Internal`               | `ValidateFromUtf8` span parse + static `Validate` dispatch |
|  [05]   | `JsonSerializerOptionsExtensions`           | `Thinktecture.Internal`               | `GetCustomMemberConverter` key-type converter resolution |

[PUBLIC_TYPE_SCOPE]: per-numeric-type key converter family (`Thinktecture.Internal`) — the singleton codecs numeric-keyed owners resolve through
- rail: serialization

| [INDEX] | [SYMBOL]                                              | [PACKAGE_ROLE] | [CAPABILITY]                                            |
| :-----: | :---------------------------------------------------- | :------------- | :----------------------------------------------------- |
|  [01]   | `ByteKeyConverter` / `SByteKeyConverter`              | key converter  | byte/sbyte dictionary-key codec honoring `JsonNumberHandling` |
|  [02]   | `ShortKeyConverter` / `UShortKeyConverter`            | key converter  | 16-bit key codec                                       |
|  [03]   | `IntKeyConverter` / `UIntKeyConverter`                | key converter  | 32-bit key codec                                       |
|  [04]   | `LongKeyConverter` / `ULongKeyConverter`              | key converter  | 64-bit key codec                                       |
|  [05]   | `SingleKeyConverter` / `DoubleKeyConverter` / `DecimalKeyConverter` | key converter | floating/decimal key codec                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: factory registration and selection
- rail: serialization

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

## [04]-[INTEGRATION_STACKING]

[GENERATOR_HANDSHAKE]: the JSON rail is the serialization half of the Thinktecture core generator — it never sees a hand-written owner.
- `Thinktecture.Runtime.Extensions` (the core, globally injected per `Directory.Packages.props`) source-generates each `[ValueObject<T>]`/`[SmartEnum<TKey>]`/`[ComplexValueObject]`/`[Union]` with the `IObjectFactory<T, TKey, TValidationError>`, `IConvertible<TKey>`, and `IValidationError<TValidationError>` contracts; this package's `MetadataLookup.FindMetadataForConversion` discovers exactly those, and `ThinktectureJsonConverterFactory.CreateConverter` selects the span/string/keyed converter from that metadata. AppUi owners get System.Text.Json support for free by being generated owners — no per-type converter.
- The generated `[JsonConverter(typeof(ThinktectureJsonConverterFactory<…>))]` attribute on each owner is what wires the typed factory; the non-generic `ThinktectureJsonConverterFactory` (options-level) is the fallback for owners reached without attribute wiring. Both routes terminate in the same converters.

[VALIDATION_RAIL]: deserialization is a validating boundary, not a blind bind — it stacks with the LanguageExt/Validation error model.
- `ThinktectureJsonConverter.Read` decodes the key then calls the owner's static `Validate`, which returns the generated `IValidationError` (the domain's typed validation error). A non-null error surfaces as `JsonException` at the serializer edge — the same typed-error discipline the C# rails use, so a malformed wire payload is a typed failure, not an exception thrown from inside domain logic.
- `ThinktectureSpanParsableJsonConverter` + `Utf8JsonReaderHelper.ValidateFromUtf8` do this zero-allocation over the UTF-8 span for span-parsable owners (the default for non-`string` keys), so hot deserialization paths (live-data, notebook, table streams) avoid the intermediate `string`.

[WIRE_BOUNDARY]: this is the System.Text.Json edge of the AppUi<->AppHost<->Persistence contract.
- AppHost ports and Persistence query results cross as canonical generated owners; the AppUi boundary registers `ThinktectureJsonConverterFactory` on the `JsonSerializerOptions` once (or relies on the per-owner attribute) so every owner round-trips identically across the wire. Internal code never pre-converts keys or post-validates values — `[LOCAL_ADMISSION]` below is the law.
- `Dock.Serializer.SystemTextJson` (the layout-persistence serializer, `api-dock-serializer.md`) and any STJ surface in the shell share the SAME `JsonSerializerOptions`; admitting this factory once covers dock-layout payloads that embed generated owners.

[FORMAT_SIBLING]: JSON is one of two Thinktecture serialization rails — MessagePack is the binary mirror.
- `Thinktecture.Runtime.Extensions.MessagePack` (admitted in the substrate) gives the identical generated owners a `MessagePack` formatter; the JSON converters here and the MessagePack formatters are parallel rails over one metadata source, so a generated owner serializes losslessly to human-readable JSON (config, evidence, debug) AND compact MessagePack (hot transport) without a second model. Choose the rail by transport, never duplicate the owner.

## [05]-[IMPLEMENTATION_LAW]

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
