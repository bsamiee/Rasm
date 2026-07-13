# [RASM_PERSISTENCE_API_THINKTECTURE_SERIALIZATION]

The cross-codec composition tier for Thinktecture-generated Smart Enums, Value Objects, and
Unions: one factory or resolver registration projects every generated owner across the System.Text.Json
snapshot codec, the MessagePack snapshot codec, and EF Core value conversion — never a hand-written
converter, formatter, or `HasConversion` per type. This page owns the MessagePack formatter surface and
the stacking recipe that mounts all three onto the `SnapshotCodec`/`ConverterRail` rails; the deep
System.Text.Json member roster lives in `api-thinktecture-json.md` and the deep EF surface in
`api-thinktecture-ef.md`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Thinktecture.Runtime.Extensions.MessagePack`
- package: `Thinktecture.Runtime.Extensions.MessagePack`
- license: file LICENSE.md (Pawel Gerr)
- assembly: `Thinktecture.Runtime.Extensions.MessagePack`
- namespace: `Thinktecture`, `Thinktecture.Formatters`
- core package: `Thinktecture.Runtime.Extensions` (generator + `IObjectFactory`/`IConvertible`/`IValidationError` contracts)
- target: `net8.0` only; the `net10.0` consumer binds `lib/net8.0`
- rail: snapshot-codec

[PACKAGE_SURFACE]: `Thinktecture.Runtime.Extensions.Json`
- package: `Thinktecture.Runtime.Extensions.Json`
- license: file LICENSE.md
- assembly: `Thinktecture.Runtime.Extensions.Json`
- namespace: `Thinktecture`, `Thinktecture.Text.Json.Serialization`, `Thinktecture.Internal`
- target: multi-target (`net8.0`, `net9.0`); the `net10.0` consumer binds `lib/net9.0`
- deep roster: `api-thinktecture-json.md`
- rail: snapshot-codec

[PACKAGE_SURFACE]: `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`
- package: `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`
- license: file LICENSE.md
- assembly: `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`
- namespace: `Thinktecture`, `Thinktecture.EntityFrameworkCore`, `Thinktecture.EntityFrameworkCore.Storage.ValueConversion`
- target: `net10.0` only
- deep roster: `api-thinktecture-ef.md`
- rail: store-provider

## [02]-[PUBLIC_TYPES]

[MESSAGEPACK_TYPES]: MessagePack formatter surface (namespaces `Thinktecture`, `Thinktecture.Formatters`)
- rail: snapshot-codec

Both keyed formatters are generic over `<T, TKey, TValidationError>` and key their generated owner; the resolver routes each owner onto its reference or value formatter.

| [INDEX] | [SYMBOL]                                    | [PACKAGE_ROLE]     | [CAPABILITY]                                            |
| :-----: | :------------------------------------------ | :----------------- | :------------------------------------------------------ |
|  [01]   | `ThinktectureMessageFormatterResolver`      | formatter resolver | `: IFormatterResolver`; `Instance`, `GetFormatter<T>`   |
|  [02]   | `ThinktectureMessagePackFormatter<…>`       | class formatter    | `: IMessagePackFormatter<T?>`                           |
|  [03]   | `ThinktectureStructMessagePackFormatter<…>` | struct formatter   | `: IMessagePackFormatter<T>, IMessagePackFormatter<T?>` |

[JSON_TYPES]: System.Text.Json codec roots (deep roster `api-thinktecture-json.md`)
- rail: snapshot-codec

Both roots register on `JsonSerializerOptions.Converters`; the span factory is generic over `<T, TValidationError>`.

| [INDEX] | [SYMBOL]                                          | [PACKAGE_ROLE]    | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------------ | :---------------- | :------------------------------------------------------------ |
|  [01]   | `ThinktectureJsonConverterFactory`                | converter factory | `: JsonConverterFactory`; admits all generated owners         |
|  [02]   | `ThinktectureSpanParsableJsonConverterFactory<…>` | span factory      | zero-allocation UTF-8-span converter for span-parsable owners |

[EF_TYPES]: EF Core store roots (deep roster `api-thinktecture-ef.md`)
- rail: store-provider

| [INDEX] | [SYMBOL]                            | [PACKAGE_ROLE]    | [CAPABILITY]                                                        |
| :-----: | :---------------------------------- | :---------------- | :------------------------------------------------------------------ |
|  [01]   | `DbContextOptionsBuilderExtensions` | convention entry  | `UseThinktectureValueConverters` installs the model convention      |
|  [02]   | `Configuration`                     | conversion policy | `Default`/`NoMaxLength`; carries the key-column max-length strategy |
|  [03]   | `ThinktectureValueConverterFactory` | converter factory | `Create<T, TKey>` builds a `ValueConverter<T, TKey>` directly       |

The convention plugin, options extension, MessagePack `SerializationContext`, and Json reflection probes are `internal`, reachable only through the factory/resolver/extension surfaces above.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: MessagePack codec admission
- rail: snapshot-codec

The resolver constructor optionally receives `skipObjectsWithMessagePackFormatterAttribute`; filtering routes through `MetadataLookup.FindMetadataForConversion` on `SerializationFrameworks.MessagePack`.

| [INDEX] | [SURFACE]                                                          | [CALL_SHAPE]    | [CAPABILITY]                                   |
| :-----: | :----------------------------------------------------------------- | :-------------- | :--------------------------------------------- |
|  [01]   | `ThinktectureMessageFormatterResolver.Instance`                    | static resolver | singleton in the resolver chain                |
|  [02]   | `new ThinktectureMessageFormatterResolver(skipObjectsWith…: true)` | resolver ctor   | skips owners with `[MessagePackFormatter]`     |
|  [03]   | `GetFormatter<T>()`                                                | resolver call   | reference/struct formatter, or `null` to defer |

[ENTRYPOINT_SCOPE]: System.Text.Json codec admission
- rail: snapshot-codec

The factory constructor optionally receives `skipObjectsWithJsonConverterAttribute` and a `Func<Type, bool>?` span-deserialization opt-out callback.

| [INDEX] | [SURFACE]                                                      | [CALL_SHAPE]      | [CAPABILITY]                                   |
| :-----: | :------------------------------------------------------------- | :---------------- | :--------------------------------------------- |
|  [01]   | `new ThinktectureJsonConverterFactory()`                       | converter factory | admits all generated owners                    |
|  [02]   | `new ThinktectureJsonConverterFactory(skipObjectsWith…: true)` | converter factory | skips attributed owners; per-type span opt-out |
|  [03]   | `CanConvert` / `CreateConverter`                               | factory overrides | resolves span/string/keyed converter per type  |

[ENTRYPOINT_SCOPE]: EF value-converter admission
- rail: store-provider

| [INDEX] | [SURFACE]                                             | [CALL_SHAPE]       | [CAPABILITY]                                           |
| :-----: | :---------------------------------------------------- | :----------------- | :----------------------------------------------------- |
|  [01]   | `UseThinktectureValueConverters([Configuration])`     | convention         | installs converters + max-length context-wide          |
|  [02]   | `AddThinktectureValueConverters`                      | model/entity entry | bulk registration in builder scope                     |
|  [03]   | `HasThinktectureValueConverter`                       | property entry     | converts one declared scalar/complex/collection member |
|  [04]   | `Configuration.Default` / `Configuration.NoMaxLength` | policy value       | bounded vs. unbounded key-column width                 |

## [04]-[IMPLEMENTATION_LAW]

[CODEC_PROFILE]:
- One registration per codec derives every per-type converter/formatter: `ThinktectureJsonConverterFactory` on `JsonSerializerOptions.Converters`, `ThinktectureMessageFormatterResolver.Instance` in the MessagePack `CompositeResolver` chain, and `UseThinktectureValueConverters` on the `DbContextOptionsBuilder`.
- The MessagePack resolver discriminates reference owners onto `ThinktectureMessagePackFormatter<T, TKey, TVErr>` (`class` constraint) and value owners onto `ThinktectureStructMessagePackFormatter<T, TKey, TVErr>` (`struct` constraint), so a keyed value-object struct never boxes through the reference formatter.
- All three packages share the `Thinktecture.Runtime.Extensions` generator contracts (`IObjectFactory<T, TKey, TValidationError>`, `IConvertible<TKey>`, `IValidationError<…>`); a generated owner's static factory + key conversion is the single source the Json converter, the MessagePack formatter, and the EF `ValueConverter` all read, so the three codecs round-trip a value identically.
- Span-parsable owners take `ThinktectureSpanParsableJsonConverter<T, TVErr>` on the Json rail (zero-alloc UTF-8-span read); the MessagePack rail keys through the resolver; the EF rail keys through `ThinktectureValueConverterFactory.Create<T, TKey>`.

[LOCAL_ADMISSION]:
- `Element/codec#CODEC_AXIS` composes the Json + MessagePack stack: `SnapshotCodec.JsonStj`'s `SnapshotJson` options carry `new ThinktectureJsonConverterFactory()`, and `SnapshotCodec.MessagePackBinary`'s `Binary` options carry `ThinktectureMessageFormatterResolver.Instance` composed ahead of `SourceGeneratedFormatterResolver.Instance` and `StandardResolver.Instance` in one `CompositeResolver.Create`, under `MessagePackCompression.Lz4BlockArray`. A doubly-registered converter never shadows the source-generated one because the `skipObjectsWith…: true` ctors arm only where a source context already owns the type.
- `Element/identity#ELEMENT_IDENTITY` composes the EF leg: `ConverterRail.Compose` mounts `.UseThinktectureValueConverters(Configuration.Default)`, so the same generated owners that cross the JSON/MessagePack wire also persist as bounded key columns — one generator, three codecs, zero hand-written conversion classes.
- A `[Union]`/`[SmartEnum]` key parses inbound from a UTF-8 span through `ThinktectureSpanParsableJsonConverter`; max-length policy is conversion metadata on the EF `Configuration` (`SmartEnumConfiguration`/`KeyedValueObjectConfiguration`), never a column annotation; the `Foreign` MessagePack route arms `MessagePackSecurity.UntrustedData` over the same resolver for the cross-process boundary.
- Codec profiles register the factory or resolver once; per-type converters/formatters are derived, not declared; a hand-written converter, formatter, or `HasConversion` beside the generated ones is the named defect.

[RAIL_LAW]:
- Packages: `Thinktecture.Runtime.Extensions.Json`, `Thinktecture.Runtime.Extensions.MessagePack`, `Thinktecture.Runtime.Extensions.EntityFrameworkCore10` (all, over `Thinktecture.Runtime.Extensions`)
- Own: the single-registration codec and store projection of Thinktecture-generated owners across JSON, MessagePack, and EF Core
- Accept: `ThinktectureJsonConverterFactory` on `JsonSerializerOptions`, `ThinktectureMessageFormatterResolver.Instance` in the `CompositeResolver`, `UseThinktectureValueConverters(Configuration.Default)` on the options builder
- Reject: handwritten converters/formatters for generated owners, a per-type `HasConversion`, a second registration shadowing the source-generated codec
