# [RASM_PERSISTENCE_API_MESSAGEPACK]

`MessagePack` supplies compact binary serialization, readers, writers, formatters, resolvers, options, and annotation contracts.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MessagePack`
- package: `MessagePack`
- assembly: `MessagePack`
- namespace: `MessagePack`
- asset: runtime library
- rail: snapshot-codec

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: MessagePack family
- rail: snapshot-codec

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]     | [CAPABILITY]               |
| :-----: | :----------------------------- | :----------------- | :------------------------- |
|   [1]   | `MessagePackSerializer`        | codec surface      | defines codec path         |
|   [2]   | `MessagePackReader`            | reader surface     | reads encoded payload      |
|   [3]   | `MessagePackWriter`            | writer surface     | writes encoded payload     |
|   [4]   | `MessagePackSerializerOptions` | policy object      | carries policy input       |
|   [5]   | `IMessagePackFormatter<T>`     | formatter contract | defines formatter boundary |
|   [6]   | `IFormatterResolver`           | resolver contract  | resolves formatter         |
|   [7]   | `MessagePackSecurity`          | security policy    | anchors codec policy       |
|   [8]   | `CompositeResolver`            | resolver surface   | composes formatters        |

[ANNOTATION_CONTRACTS]:
- rail: snapshot-codec

| [INDEX] | [SYMBOL]                     | [PACKAGE_ROLE]      | [CAPABILITY]          |
| :-----: | :--------------------------- | :------------------ | :-------------------- |
|   [1]   | `MessagePackObjectAttribute` | annotation contract | marks object contract |
|   [2]   | `KeyAttribute`               | annotation contract | assigns field key     |
|   [3]   | `IgnoreMemberAttribute`      | annotation contract | excludes member       |
|   [4]   | `UnionAttribute`             | annotation contract | declares union case   |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: codec operations
- rail: snapshot-codec

| [INDEX] | [SURFACE]                            | [CALL_SHAPE]     | [CAPABILITY]            |
| :-----: | :----------------------------------- | :--------------- | :---------------------- |
|   [1]   | `Serialize`                          | operation call   | writes encoded payload  |
|   [2]   | `Deserialize`                        | operation call   | reads encoded payload   |
|   [3]   | `ConvertToJson`                      | JSON projection  | projects debug JSON     |
|   [4]   | `WithResolver`                       | fluent option    | applies resolver policy |
|   [5]   | `WithSecurity`                       | fluent option    | applies security policy |
|   [6]   | `IFormatterResolver.GetFormatter<T>` | resolver lookup  | resolves formatter      |
|   [7]   | `CompositeResolver.Create`           | resolver factory | composes formatters     |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `MessagePack`
- Owns: binary snapshot codec
- Accept: codec choice is snapshot profile data
- Reject: serializer-specific public APIs
