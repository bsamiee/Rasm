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

| [INDEX] | [SYMBOL]                                                            | [PACKAGE_ROLE]     | [CAPABILITY]                                           |
| :-----: | :------------------------------------------------------------------ | :----------------- | :----------------------------------------------------- |
|  [01]   | `ThinktectureMessageFormatterResolver`                              | formatter resolver | `IFormatterResolver`; generated-owner formatter lookup |
|  [02]   | `ThinktectureMessageFormatterResolver.Cache<T>`                     | resolver cache     | metadata-backed per-type formatter cache               |
|  [03]   | `ThinktectureMessagePackFormatter<T, TKey, TValidationError>`       | class formatter    | reference-type generated-owner codec                   |
|  [04]   | `ThinktectureStructMessagePackFormatter<T, TKey, TValidationError>` | struct formatter   | value-type generated-owner codec                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: resolver registration and lookup
- rail: wire-messagepack

| [INDEX] | [SURFACE]                                                                                     | [CALL_SHAPE]            | [CAPABILITY]                                                        |
| :-----: | :-------------------------------------------------------------------------------------------- | :---------------------- | :------------------------------------------------------------------ |
|  [01]   | `ThinktectureMessageFormatterResolver.Instance`                                               | static resolver         | singleton resolver for `CompositeResolver`                          |
|  [02]   | `new ThinktectureMessageFormatterResolver()`                                                  | constructor             | skips `[MessagePackFormatter]` owners by default                    |
|  [03]   | `new ThinktectureMessageFormatterResolver(bool skipObjectsWithMessagePackFormatterAttribute)` | constructor             | controls attributed-owner skip behavior                             |
|  [04]   | `GetFormatter<T>()`                                                                           | resolver call           | returns a generated-owner class/struct formatter or `null` to defer |
|  [05]   | `CompositeResolver.Create(ThinktectureMessageFormatterResolver.Instance,...)`                 | MessagePack composition | place generated-owner resolver before standard fallback             |

[ENTRYPOINT_SCOPE]: formatter operation
- rail: wire-messagepack

| [INDEX] | [SURFACE]                                                                            | [CALL_SHAPE]    | [CAPABILITY]                                        |
| :-----: | :----------------------------------------------------------------------------------- | :-------------- | :-------------------------------------------------- |
|  [01]   | `ThinktectureMessagePackFormatter<T, TKey, TValidationError>.Serialize(...)`         | formatter write | writes generated owner by key projection            |
|  [02]   | `ThinktectureMessagePackFormatter<T, TKey, TValidationError>.Deserialize(...)`       | formatter read  | decodes key and validates through generated factory |
|  [03]   | `ThinktectureStructMessagePackFormatter<T, TKey, TValidationError>.Serialize(...)`   | formatter write | value-type generated owner write path               |
|  [04]   | `ThinktectureStructMessagePackFormatter<T, TKey, TValidationError>.Deserialize(...)` | formatter read  | value-type generated owner read and validation path |

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
- Keep the neuecc `MessagePack` engine catalogue in `api-messagepack.md`; this page owns only the Thinktecture bridge package.
- A type carrying an explicit `[MessagePackFormatter]` is skipped by resolver policy so the explicit formatter remains the owner.

[RAIL_LAW]:
- Package: `Thinktecture.Runtime.Extensions.MessagePack`
- Owns: MessagePack resolver and formatter projection for Thinktecture-generated owners.
- Accept: `ThinktectureMessageFormatterResolver.Instance` in a `CompositeResolver`, generated-owner key serialization, generated validation on read, and resolver deferral for non-generated or explicitly formatted types.
- Reject: hand-written MessagePack formatters for generated owners, conflating this bridge with the `MessagePack` engine package, or registering this resolver after a fallback that does shadow it.
