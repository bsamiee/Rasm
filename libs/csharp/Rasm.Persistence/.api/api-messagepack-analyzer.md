# [RASM_PERSISTENCE_API_MESSAGEPACK_ANALYZER]

`MessagePackAnalyzer` supplies analyzer, code-fix, and source-generator assets
for MessagePack contracts, generated formatters, generated resolvers, and
contract diagnostics.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MessagePackAnalyzer`
- package: `MessagePackAnalyzer`
- assembly: analyzer assets
- namespace: analyzer and generator assets
- asset: analyzer package
- rail: snapshot-codec

## [2]-[PACKAGE_ASSETS]

[PACKAGE_ASSET_SCOPE]: analyzer and generator assets
- rail: snapshot-codec

| [INDEX] | [SYMBOL]                              | [PACKAGE_ROLE]   | [CAPABILITY]             |
| :-----: | :------------------------------------ | :--------------- | :----------------------- |
|   [1]   | `MessagePack.SourceGenerator.dll`     | source generator | emits formatter code     |
|   [2]   | `MessagePack.Analyzers.CodeFixes.dll` | analyzer asset   | reports contract issues  |
|   [3]   | analyzer diagnostics                  | diagnostic asset | classifies contract gaps |
|   [4]   | generated formatter templates         | generator asset  | emits formatter shapes   |
|   [5]   | generated resolver templates          | generator asset  | emits resolver shapes    |

[RECOGNIZED_TYPES]: recognized contract markers
- rail: snapshot-codec

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE]        | [CAPABILITY]             |
| :-----: | :-------------------------------------- | :-------------------- | :----------------------- |
|   [1]   | `MessagePackObjectAttribute`            | contract attribute    | marks object contracts   |
|   [2]   | `KeyAttribute`                          | contract attribute    | declares member keys     |
|   [3]   | `IgnoreMemberAttribute`                 | contract attribute    | excludes members         |
|   [4]   | `UnionAttribute`                        | contract attribute    | declares union cases     |
|   [5]   | `MessagePackFormatterAttribute`         | formatter attribute   | selects formatter        |
|   [6]   | `SerializationConstructorAttribute`     | constructor attribute | selects constructor      |
|   [7]   | `GeneratedMessagePackResolverAttribute` | resolver attribute    | marks generated resolver |
|   [8]   | `CompositeResolverAttribute`            | resolver attribute    | declares resolver inputs |

## [3]-[ANALYZER_CLASSES]

[ANALYZER_CLASS_SCOPE]: diagnostic analyzers
- rail: snapshot-codec

| [INDEX] | [SYMBOL]                               | [PACKAGE_ROLE]        | [CAPABILITY]                        |
| :-----: | :------------------------------------- | :-------------------- | :---------------------------------- |
|   [1]   | `MsgPack001SpecifyOptionsAnalyzer`     | diagnostic analyzer   | reports missing serializer options  |
|   [2]   | `MsgPack002UseConstantOptionsAnalyzer` | diagnostic analyzer   | reports mutable shared options      |
|   [3]   | `MsgPack00xMessagePackAnalyzer`        | diagnostic analyzer   | reports contract correctness issues |
|   [4]   | `MessagePackGenerator`                 | incremental generator | emits typed formatter code          |
|   [5]   | `CompositeResolverGenerator`           | incremental generator | emits resolver code                 |
|   [6]   | `FormatterCodeFixProvider`             | code-fix provider     | fixes formatter-access issues       |
|   [7]   | `MessagePackCodeFixProvider`           | code-fix provider     | fixes MsgPack003 and MsgPack004     |
|   [8]   | `MsgPack015CodeFixProvider`            | code-fix provider     | fixes MsgPack015 AllowPrivate       |

## [4]-[DIAGNOSTIC_SURFACE]

[DIAGNOSTIC_SCOPE]: contract diagnostics
- rail: snapshot-codec

All diagnostics are `DiagnosticDescriptor` static fields on `MsgPack00xMessagePackAnalyzer` (MsgPack003–MsgPack009, MsgPack015, MsgPack018) or as `const string` ID fields; MsgPack001–002 live on their named analyzer classes.

| [INDEX] | [DIAGNOSTIC_ID] | [SEVERITY] | [DESCRIPTOR_NAME]                                     | [TRIGGER]                                                 |
| :-----: | :-------------- | :--------- | :---------------------------------------------------- | :-------------------------------------------------------- |
|   [1]   | `MsgPack001`    | Warning    | `MissingOptionsDescriptor`                            | `MessagePackSerializer` call without explicit options     |
|   [2]   | `MsgPack002`    | Warning    | `MutableSharedOptionsDescriptor`                      | `DefaultOptions` mutated after startup                    |
|   [3]   | `MsgPack003`    | Error      | `TypeMustBeMessagePackObject`                         | type serialized without `[MessagePackObject]`             |
|   [4]   | `MsgPack004`    | Error      | `MemberNeedsKey`                                      | member without `[Key]` or `[IgnoreMember]`                |
|   [5]   | `MsgPack005`    | Error      | `InvalidMessagePackObject` / `UnionAttributeRequired` | invalid contract shape                                    |
|   [6]   | `MsgPack006`    | Error      | `MessageFormatterMustBeMessagePackFormatter`          | `[MessagePackFormatter]` type not `IMessagePackFormatter` |
|   [7]   | `MsgPack007`    | Error      | `NoDeserializingConstructor` / parameter diagnostics  | missing or mismatched deserializing constructor           |
|   [8]   | `MsgPack008`    | Error      | `AotUnionAttributeRequiresTypeArg`                    | `[Union]` without type argument in AOT mode               |
|   [9]   | `MsgPack009`    | Error      | `CollidingFormatters`                                 | multiple formatters for the same type                     |
|  [10]   | `MsgPack010`    | —          | `InaccessibleFormatterTypeId`                         | formatter type inaccessible to generator                  |
|  [11]   | `MsgPack011`    | —          | `PartialTypeRequiredId`                               | generated type not declared `partial`                     |
|  [12]   | `MsgPack012`    | —          | `InaccessibleDataTypeId`                              | data type inaccessible to formatter                       |
|  [13]   | `MsgPack013`    | —          | `InaccessibleFormatterInstanceId`                     | formatter instance inaccessible to resolver               |
|  [14]   | `MsgPack014`    | —          | `NullableReferenceTypeFormatterId`                    | nullable reference type without nullable formatter        |
|  [15]   | `MsgPack015`    | Warning    | `MessagePackObjectAllowPrivateRequired`               | non-public type or member needs `AllowPrivate = true`     |
|  [16]   | `MsgPack016`    | —          | `AOTDerivedKeyId`                                     | derived key used in AOT mode                              |
|  [17]   | `MsgPack017`    | —          | `AOTInitPropertyId`                                   | init property in AOT mode                                 |
|  [18]   | `MsgPack018`    | Error      | `CollidingMemberNamesInForceMapMode`                  | duplicate member name in force-map mode                   |

## [5]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: diagnostics and generated surfaces
- rail: snapshot-codec

| [INDEX] | [SURFACE]                                              | [CALL_SHAPE]       | [CAPABILITY]                                    |
| :-----: | :----------------------------------------------------- | :----------------- | :---------------------------------------------- |
|   [1]   | `MsgPack003` (`TypeMustBeMessagePackObject`)           | error diagnostic   | blocks unattributed serialized types            |
|   [2]   | `MsgPack015` (`MessagePackObjectAllowPrivateRequired`) | warning diagnostic | enforces `AllowPrivate` on non-public contracts |
|   [3]   | `MsgPack004` (`MemberNeedsKey`)                        | error diagnostic   | blocks unkeyed members                          |
|   [4]   | `MsgPack009` (`CollidingFormatters`)                   | error diagnostic   | blocks colliding formatters                     |
|   [5]   | `MsgPack005` (`InvalidMessagePackObject`)              | error diagnostic   | blocks invalid contract shapes                  |
|   [6]   | `MsgPack005` (`UnionAttributeRequired`)                | error diagnostic   | blocks missing `[Union]`                        |
|   [7]   | generated formatter class                              | generated output   | formats snapshot values                         |
|   [8]   | generated resolver class                               | generated output   | resolves snapshot values                        |

## [6]-[IMPLEMENTATION_LAW]

[ANALYZER_ADMISSION]:
- dependency role: analyzer-only package (`PrivateAssets="all"`)
- generator root: `MessagePackGenerator` and `CompositeResolverGenerator` as `IIncrementalGenerator`
- diagnostic root: `MsgPack00xMessagePackAnalyzer`, `MsgPack001SpecifyOptionsAnalyzer`, `MsgPack002UseConstantOptionsAnalyzer`
- code-fix root: `MessagePackCodeFixProvider`, `FormatterCodeFixProvider`, `MsgPack015CodeFixProvider`
- output root: generated formatter and resolver surfaces

[LOCAL_ADMISSION]:
- Analyzer and generator assets enforce snapshot contract shape before runtime use.
- Generated formatters and resolvers belong to snapshot codec implementation folders.
- Analyzer diagnostics block ambiguous, unkeyed, colliding, or invalid MessagePack contracts.
- Analyzer assets stay private and never become runtime dependencies.

[RAIL_LAW]:
- Package: `MessagePackAnalyzer`
- Owns: MessagePack contract analyzer and generator assets
- Accept: generated formatter and resolver proof
- Reject: runtime dependency on analyzer assets
