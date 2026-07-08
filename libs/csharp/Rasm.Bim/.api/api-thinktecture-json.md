# [RASM_BIM_API_THINKTECTURE_JSON]

Full surface and stacking: `libs/csharp/.api/api-thinktecture-json.md` (shared-tier canonical owner).

## [01]-[IMPLEMENTATION_LAW]

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
