# [RASM_PERSISTENCE_API_THINKTECTURE_SERIALIZATION]

One registration per rail projects every Thinktecture-generated Smart Enum, Value Object, and keyed Union onto the System.Text.Json wire, the MessagePack wire, and EF Core value conversion, each per-type converter or formatter derived from the owner's generated conversion metadata rather than declared. All three rails read the same generated static factory and key projection, so a value round-trips identically on every wire and persists as its key column with no hand-written converter, formatter, or `HasConversion` beside it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Thinktecture.Runtime.Extensions.Json`
- package: `Thinktecture.Runtime.Extensions.Json` (BSD-3-Clause, Pawel Gerr)
- assembly: `Thinktecture.Runtime.Extensions.Json`
- namespace: `Thinktecture`, `Thinktecture.Text.Json.Serialization`, `Thinktecture.Internal`
- rail: snapshot-codec

[PACKAGE_SURFACE]: `Thinktecture.Runtime.Extensions.MessagePack`
- package: `Thinktecture.Runtime.Extensions.MessagePack` (BSD-3-Clause, Pawel Gerr)
- assembly: `Thinktecture.Runtime.Extensions.MessagePack`
- namespace: `Thinktecture`, `Thinktecture.Formatters`
- rail: snapshot-codec

[PACKAGE_SURFACE]: `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`
- package: `Thinktecture.Runtime.Extensions.EntityFrameworkCore10` (BSD-3-Clause, Pawel Gerr)
- assembly: `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`
- namespace: `Thinktecture`, `Thinktecture.EntityFrameworkCore`, `Thinktecture.EntityFrameworkCore.Storage.ValueConversion`
- rail: store-provider

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: registration roots the composition mounts and the arms each closes over its owner's type, key type, and validation-error type

| [INDEX] | [SYMBOL]                                                            | [TYPE_FAMILY] | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------ | :------------ | :--------------------------------------- |
|  [01]   | `ThinktectureJsonConverterFactory`                                  | class         | selects a converter per generated owner  |
|  [02]   | `ThinktectureSpanParsableJsonConverterFactory<T, TValidationError>` | class         | binds the zero-allocation UTF-8 span arm |
|  [03]   | `ThinktectureMessageFormatterResolver`                              | class         | derives a formatter per generated owner  |
|  [04]   | `ThinktectureMessagePackFormatter<T, TKey, TValidationError>`       | class         | codecs reference-type owners             |
|  [05]   | `ThinktectureStructMessagePackFormatter<T, TKey, TValidationError>` | class         | codecs value owners and their nullable   |
|  [06]   | `DbContextOptionsBuilderExtensions`                                 | static class  | installs the EF conversion convention    |
|  [07]   | `Configuration`                                                     | sealed class  | max-length and read-strategy presets     |
|  [08]   | `ThinktectureValueConverterFactory`                                 | class         | builds one `ValueConverter<T, TKey>`     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: rail admission — the one registration each rail takes and the per-type resolution derived from it

| [INDEX] | [SURFACE]                                                                            | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :----------------------------------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `ThinktectureJsonConverterFactory()`                                                 | ctor     | skips `[JsonConverter]` owners       |
|  [02]   | `ThinktectureJsonConverterFactory(bool, Func<Type, bool>?)`                          | ctor     | attribute policy plus a span opt-out |
|  [03]   | `ThinktectureJsonConverterFactory.CreateConverter(Type, JsonSerializerOptions)`      | instance | binds the span, string, or keyed arm |
|  [04]   | `ThinktectureMessageFormatterResolver.Instance`                                      | static   | resolver a composite chain seats     |
|  [05]   | `ThinktectureMessageFormatterResolver(bool)`                                         | ctor     | selects the attribute-skip policy    |
|  [06]   | `ThinktectureMessageFormatterResolver.GetFormatter<T>()`                             | instance | closed arm, or null to defer         |
|  [07]   | `DbContextOptionsBuilderExtensions.UseThinktectureValueConverters(Configuration)`    | static   | installs the convention context-wide |
|  [08]   | `ModelBuilderExtensions.AddThinktectureValueConverters(Configuration)`               | static   | bulk registration in builder scope   |
|  [09]   | `PropertyBuilderExtensions.HasThinktectureValueConverter(Configuration)`             | static   | converts one declared scalar member  |
|  [10]   | `ThinktectureValueConverterFactory.Create<T, TKey>(bool) -> ValueConverter<T, TKey>` | static   | builds a converter EF cannot resolve |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- All three rails read one generated contract set — `IObjectFactory<T, TKey, TValidationError>` for the static factory, `IConvertible<TKey>` for key projection, `IValidationError<…>` for the failure — so an owner's wire form and its stored column derive from the one declaration.
- `MetadataLookup.FindMetadataForConversion`, filtered on the owner's `SerializationFrameworks` flag, decides admission per rail; an owner the metadata declines falls through untouched to the next converter or resolver in the chain.
- `ConversionMetadata.Type.IsClass` selects the MessagePack arm — reference owners onto `ThinktectureMessagePackFormatter<…>`, value owners onto `ThinktectureStructMessagePackFormatter<…>` — so a keyed value-object struct never boxes through the reference arm.
- A span-parsable owner takes `ThinktectureSpanParsableJsonConverter<T, TValidationError>` on the Json rail, and the factory's `Func<Type, bool>?` callback drops one type back to the keyed converter.
- Every decode terminates in the generated static `Validate`, so a rejected payload surfaces at the serializer or provider edge and never materializes an owner past its invariant.

[STACKING]:
- `Thinktecture.Runtime.Extensions.Json`(`../../.api/api-thinktecture-json.md`): `ThinktectureJsonConverterFactory` lands on `JsonSerializerOptions.Converters`; the keyed, string, and span converter families and the `Utf8JsonReaderHelper` UTF-8 seam resolve there.
- `Thinktecture.Runtime.Extensions.MessagePack`(`../../.api/api-thinktecture-messagepack.md`): `ThinktectureMessageFormatterResolver.Instance` seats through `CompositeResolver.Create`; both formatter arms and their nil-key and validation verdicts resolve there.
- `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`(`.api/api-thinktecture-ef.md`): `UseThinktectureValueConverters` installs the model-building convention; the `Configuration` max-length strategy family resolves there.
- within-lib wire: `Element/codec#CODEC_AXIS` composes both wire rails off one declaration — `ElementJson.Options` carries `new ThinktectureJsonConverterFactory()` beside the GeoJSON factory, and `SnapshotCodec.Binary` seats `ThinktectureMessageFormatterResolver.Instance` ahead of `GeneratedMessagePackResolver.Instance` and `StandardResolver.Instance` under `MessagePackSecurity.UntrustedData` and `MessagePackCompression.Lz4BlockArray`.
- within-lib store: `Element/identity#ELEMENT_IDENTITY` mounts the third registration on the one `ConverterRail.Compose` options row under `Configuration.Default`, so an owner crossing both wires persists as the same bounded smart-enum key column.

[LOCAL_ADMISSION]:
- Codec profiles register the factory or resolver once and derive every per-type converter and formatter from it; a hand-written converter, a hand-written formatter, or a per-property `HasConversion` beside a generated owner is the named defect.
- Each default constructor already skips an owner carrying its own `[JsonConverter]` or `[MessagePackFormatter]`, so attribute wiring and options-level registration never double-bind one type.

[RAIL_LAW]:
- Package: `Thinktecture.Runtime.Extensions.Json`, `Thinktecture.Runtime.Extensions.MessagePack`, `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`
- Owns: the one-registration projection of every generated owner across the JSON wire, the MessagePack wire, and EF Core value conversion
- Accept: `ThinktectureJsonConverterFactory` on `JsonSerializerOptions.Converters`, `ThinktectureMessageFormatterResolver.Instance` in a `CompositeResolver` chain, `UseThinktectureValueConverters(Configuration.Default)` on the options builder
- Reject: a hand-written converter or formatter for a generated owner, a per-property `HasConversion`, a second registration shadowing the source-generated codec
