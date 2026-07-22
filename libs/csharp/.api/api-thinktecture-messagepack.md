# [RASM_API_THINKTECTURE_MESSAGEPACK]

`Thinktecture.Runtime.Extensions.MessagePack` derives one MessagePack formatter per generated owner from that owner's conversion metadata and holds it in per-closed-generic static state. Its resolver reads the key type, validation-error type, and reference-or-value shape off the metadata, closes the matching formatter arm over that triple, and returns null for every type the metadata does not claim. Serialization projects the generated key through the composed options' own key formatter; deserialization runs the generated static validation rail before an owner materializes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Thinktecture.Runtime.Extensions.MessagePack`
- package: `Thinktecture.Runtime.Extensions.MessagePack` (BSD-3-Clause, Pawel Gerr)
- assembly: `Thinktecture.Runtime.Extensions.MessagePack`
- namespace: `Thinktecture`, `Thinktecture.Formatters`
- depends: `Thinktecture.Runtime.Extensions`, `MessagePack`
- rail: wire-messagepack

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: resolver and its two generated-owner formatter arms

| [INDEX] | [SYMBOL]                                                            | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :------------------------------------------------------------------ | :------------ | :----------------------------------------------- |
|  [01]   | `ThinktectureMessageFormatterResolver`                              | class         | derives one formatter per generated owner        |
|  [02]   | `ThinktectureMessagePackFormatter<T, TKey, TValidationError>`       | class         | codecs reference-type owners                     |
|  [03]   | `ThinktectureStructMessagePackFormatter<T, TKey, TValidationError>` | class         | codecs value-type owners and their nullable form |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: registration, lookup, and codec ops — every codec op takes a trailing `MessagePackSerializerOptions` whose `Resolver` supplies the `TKey` formatter

| [INDEX] | [SURFACE]                                                                        | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :------------------------------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `ThinktectureMessageFormatterResolver.Instance`                                  | static   | shared resolver an engine chain seats  |
|  [02]   | `ThinktectureMessageFormatterResolver()`                                         | ctor     | skips attribute-formatted owners       |
|  [03]   | `ThinktectureMessageFormatterResolver(bool)`                                     | ctor     | selects the attribute-skip policy      |
|  [04]   | `ThinktectureMessageFormatterResolver.GetFormatter<T>()`                         | instance | returns the cached arm or null         |
|  [05]   | `ThinktectureMessagePackFormatter.Serialize(ref MessagePackWriter, T?)`          | instance | nil for null, else the projected key   |
|  [06]   | `ThinktectureMessagePackFormatter.Deserialize(ref MessagePackReader) -> T?`      | instance | key read, then generated validation    |
|  [07]   | `ThinktectureStructMessagePackFormatter.Serialize(ref MessagePackWriter, T)`     | instance | projects the value owner's key         |
|  [08]   | `ThinktectureStructMessagePackFormatter.Serialize(ref MessagePackWriter, T?)`    | instance | nil for the empty nullable             |
|  [09]   | `ThinktectureStructMessagePackFormatter.Deserialize(ref MessagePackReader) -> T` | instance | key read into the value owner          |
|  [10]   | `IMessagePackFormatter<T?>.Deserialize(ref MessagePackReader) -> T?`             | instance | value arm's nullable read, nil to null |

- Both `Deserialize` arms throw `ValidationException` when the generated `Validate` returns an error, so no wire value materializes an owner past its invariant.
- `ThinktectureStructMessagePackFormatter.Deserialize`: a nil key throws `MessagePackSerializationException` for an `IDisallowDefaultValue` owner and yields `default(T)` otherwise, where the reference arm returns null.
- `ThinktectureMessageFormatterResolver.GetFormatter<T>`: throws when the metadata-selected formatter type fails to instantiate.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ThinktectureMessageFormatterResolver.Instance` binds ahead of the contract and source-generated fallbacks, so a generated owner resolves through its metadata before any contract scan reaches the type.
- Owner admission filters on `SerializationFrameworks.MessagePack`: an object factory without that framework flag falls through untouched and `GetFormatter<T>()` returns null, handing the decision to the next resolver in the chain.
- `ConversionMetadata.Type.IsClass` selects the arm, and its `KeyType` and `ValidationErrorType` close it; one closed generic instantiates its formatter once per process, so resolution cost is a first touch rather than a per-call lookup.

[STACKING]:
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions.md`): `MetadataLookup.FindMetadataForConversion` yields the `ConversionMetadata` triple both arms close over, and every op crosses the generated contracts — `IConvertible<TKey>.ToValue()` outbound, static `IObjectFactory<T, TKey, TValidationError>.Validate` inbound, `IDisallowDefaultValue` deciding the value arm's nil-key verdict.
- `MessagePack`: `Instance` is an `IFormatterResolver` the engine composes — `CompositeResolver.Create(params IFormatterResolver[])` fixes its precedence ahead of `StandardResolver`, and each op reaches the key's own codec through `FormatterResolverExtensions.GetFormatterWithVerify<TKey>` over `MessagePackSerializerOptions.Resolver`.
- within-lib: one `MessagePackSerializerOptions` carries the whole wire profile — `WithResolver` seats the composed chain covering reference and value owners from a single registration, a generated owner keyed by another generated owner nests through that same chain, and security and compression policy ride the one options value.

[LOCAL_ADMISSION]:
- A wire profile registers one resolver chain, and every generated owner's formatter derives from its metadata rather than standing beside the value object or smart enum.
- An owner carrying `[MessagePackFormatter]` keeps that explicit formatter under the default ctor's skip policy; `ThinktectureMessageFormatterResolver(false)` overrides it where the generated projection wins.

[RAIL_LAW]:
- Package: `Thinktecture.Runtime.Extensions.MessagePack`
- Owns: metadata-derived formatter resolution and key projection for generated owners on the MessagePack wire.
- Accept: `Instance` composed into an engine resolver chain, key-projected writes, generated validation on read, and deferral for unclaimed types.
- Reject: a hand-written MessagePack formatter beside a generated owner.
