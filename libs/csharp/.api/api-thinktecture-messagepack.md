# [RASM_API_THINKTECTURE_MESSAGEPACK]

`Thinktecture.Runtime.Extensions.MessagePack` is the MessagePack-CSharp bridge for Thinktecture-generated Value Objects, Smart Enums, and Unions. It contributes a resolver and formatter pair that read the generated owner metadata and serialize by generated key projection, so a binary MessagePack wire profile can carry the same generated owners as the System.Text.Json rail without a hand-written formatter per type.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Thinktecture.Runtime.Extensions.MessagePack`
- package: `Thinktecture.Runtime.Extensions.MessagePack`
- assembly: `Thinktecture.Runtime.Extensions.MessagePack`
- bound asset: `lib/net8.0` for `net10.0` consumers
- namespaces: `Thinktecture`, `Thinktecture.Formatters`
- dependencies: `Thinktecture.Runtime.Extensions`; `MessagePack` resolved through central/transitive package graph
- direct owners: `Rasm.Materials`, `Rasm.Persistence`
- rail: wire-messagepack

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: resolver and formatter surface
- rail: wire-messagepack

| [INDEX] | [SYMBOL]                                                            | [KIND]       | [CAPABILITY]             |
| :-----: | :------------------------------------------------------------------ | :----------- | :----------------------- |
|  [01]   | `ThinktectureMessageFormatterResolver`                              | resolver     | selects owner formatters |
|  [02]   | `ThinktectureMessageFormatterResolver.Cache<T>`                     | cache        | retains typed formatters |
|  [03]   | `ThinktectureMessagePackFormatter<T, TKey, TValidationError>`       | class codec  | codecs reference owners  |
|  [04]   | `ThinktectureStructMessagePackFormatter<T, TKey, TValidationError>` | struct codec | codecs value-type owners |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: resolver registration and lookup
- rail: wire-messagepack

| [INDEX] | [SURFACE]                                       | [KIND]      | [CAPABILITY]                 |
| :-----: | :---------------------------------------------- | :---------- | :--------------------------- |
|  [01]   | `ThinktectureMessageFormatterResolver.Instance` | singleton   | composes resolver chains     |
|  [02]   | `ThinktectureMessageFormatterResolver()`        | ctor        | skips attributed owners      |
|  [03]   | `ThinktectureMessageFormatterResolver(bool)`    | ctor        | selects attribute handling   |
|  [04]   | `GetFormatter<T>`                               | resolver    | returns or defers formatters |
|  [05]   | `CompositeResolver.Create`                      | composition | orders resolver precedence   |

[ENTRYPOINT_SCOPE]: formatter operation
- rail: wire-messagepack

| [INDEX] | [FORMATTER]                                                         | [OPERATION]   | [CAPABILITY]          |
| :-----: | :------------------------------------------------------------------ | :------------ | :-------------------- |
|  [01]   | `ThinktectureMessagePackFormatter<T, TKey, TValidationError>`       | `Serialize`   | projects keys         |
|  [02]   | `ThinktectureMessagePackFormatter<T, TKey, TValidationError>`       | `Deserialize` | decodes and validates |
|  [03]   | `ThinktectureStructMessagePackFormatter<T, TKey, TValidationError>` | `Serialize`   | projects struct keys  |
|  [04]   | `ThinktectureStructMessagePackFormatter<T, TKey, TValidationError>` | `Deserialize` | decodes struct keys   |

## [04]-[IMPLEMENTATION_LAW]

[RESOLVER_TOPOLOGY]:
- Compose `ThinktectureMessageFormatterResolver.Instance` ahead of the standard/source-generated MessagePack fallback so generated owners resolve through Thinktecture metadata first.
- `GetFormatter<T>()` returns `null` for types that are not Thinktecture-generated owners or that must be skipped because an explicit `[MessagePackFormatter]` owns them.
- Reference owners route through `ThinktectureMessagePackFormatter<T, TKey, TValidationError>`; value-type owners route through `ThinktectureStructMessagePackFormatter<T, TKey, TValidationError>`.

[GENERATOR_HANDSHAKE]:
- The formatter family depends on `Thinktecture.Runtime.Extensions` metadata, generated `Validate`, generated `ToValue`, and validation-error contracts. It does not declare generated owners itself.
- The MessagePack bridge and the STJ bridge are sibling adapters over one generated owner model. Use transport requirements to choose JSON or binary MessagePack; do not duplicate owner types.

[LOCAL_ADMISSION]:
- A MessagePack wire profile registers one resolver chain. Per-type generated-owner formatters are derived by the resolver and cache, not hand-authored beside each value object or smart enum.
- A type carrying an explicit `[MessagePackFormatter]` is skipped by resolver policy so the explicit formatter remains the owner.

[RAIL_LAW]:
- Package: `Thinktecture.Runtime.Extensions.MessagePack`
- Owns: MessagePack resolver and formatter projection for Thinktecture-generated owners.
- Accept: `ThinktectureMessageFormatterResolver.Instance` in a `CompositeResolver`, generated-owner key serialization, generated validation on read, and resolver deferral for non-generated or explicitly formatted types.
- Reject: hand-written MessagePack formatters for generated owners, conflating this bridge with the `MessagePack` engine package, or registering this resolver after a fallback that does shadow it.
