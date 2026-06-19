# [RASM_PERSISTENCE_API_MESSAGEPACK_ANALYZER]

`MessagePackAnalyzer` supplies analyzer, code-fix, and source-generator assets
for MessagePack contracts, generated formatters, generated resolvers, and
contract diagnostics.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MessagePackAnalyzer`
- package: `MessagePackAnalyzer`
- assembly: analyzer assets
- namespace: analyzer and generator assets
- asset: analyzer package
- rail: snapshot-codec

## [02]-[PUBLIC_TYPES]

[PACKAGE_ASSET_SCOPE]: analyzer and generator assets
- rail: snapshot-codec

| [INDEX] | [SYMBOL]                              | [PACKAGE_ROLE]   | [CAPABILITY]             |
| :-----: | :------------------------------------ | :--------------- | :----------------------- |
|  [01]   | `MessagePack.SourceGenerator.dll`     | source generator | emits formatter code     |
|  [02]   | `MessagePack.Analyzers.CodeFixes.dll` | analyzer asset   | reports contract issues  |
|  [03]   | analyzer diagnostics                  | diagnostic asset | classifies contract gaps |
|  [04]   | generated formatter templates         | generator asset  | emits formatter shapes   |
|  [05]   | generated resolver templates          | generator asset  | emits resolver shapes    |

[RECOGNIZED_TYPES]: recognized contract markers
- rail: snapshot-codec

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE]        | [CAPABILITY]             |
| :-----: | :-------------------------------------- | :-------------------- | :----------------------- |
|  [01]   | `MessagePackObjectAttribute`            | contract attribute    | marks object contracts   |
|  [02]   | `KeyAttribute`                          | contract attribute    | declares member keys     |
|  [03]   | `IgnoreMemberAttribute`                 | contract attribute    | excludes members         |
|  [04]   | `UnionAttribute`                        | contract attribute    | declares union cases     |
|  [05]   | `MessagePackFormatterAttribute`         | formatter attribute   | selects formatter        |
|  [06]   | `SerializationConstructorAttribute`     | constructor attribute | selects constructor      |
|  [07]   | `GeneratedMessagePackResolverAttribute` | resolver attribute    | marks generated resolver |
|  [08]   | `CompositeResolverAttribute`            | resolver attribute    | declares resolver inputs |

[ANALYZER_CLASS_SCOPE]: diagnostic analyzers
- rail: snapshot-codec

| [INDEX] | [SYMBOL]                               | [PACKAGE_ROLE]        | [CAPABILITY]                        |
| :-----: | :------------------------------------- | :-------------------- | :---------------------------------- |
|  [01]   | `MsgPack001SpecifyOptionsAnalyzer`     | diagnostic analyzer   | reports missing serializer options  |
|  [02]   | `MsgPack002UseConstantOptionsAnalyzer` | diagnostic analyzer   | reports mutable shared options      |
|  [03]   | `MsgPack00xMessagePackAnalyzer`        | diagnostic analyzer   | reports contract correctness issues |
|  [04]   | `MessagePackGenerator`                 | incremental generator | emits typed formatter code          |
|  [05]   | `CompositeResolverGenerator`           | incremental generator | emits resolver code                 |
|  [06]   | `FormatterCodeFixProvider`             | code-fix provider     | fixes formatter-access issues       |
|  [07]   | `MessagePackCodeFixProvider`           | code-fix provider     | fixes MsgPack003 and MsgPack004     |
|  [08]   | `MsgPack015CodeFixProvider`            | code-fix provider     | fixes MsgPack015 AllowPrivate       |

[DIAGNOSTIC_SCOPE]: contract diagnostics
- rail: snapshot-codec

All diagnostics are `DiagnosticDescriptor` static fields on `MsgPack00xMessagePackAnalyzer` (MsgPack003–MsgPack009, MsgPack015, MsgPack018) or as `const string` ID fields; MsgPack001–002 live on their named analyzer classes.

| [INDEX] | [DIAGNOSTIC_ID] | [SEVERITY] | [DESCRIPTOR_NAME]                                     | [TRIGGER]                                                 |
| :-----: | :-------------- | :--------- | :---------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `MsgPack001`    | Warning    | `MissingOptionsDescriptor`                            | `MessagePackSerializer` call without explicit options     |
|  [02]   | `MsgPack002`    | Warning    | `MutableSharedOptionsDescriptor`                      | `DefaultOptions` mutated after startup                    |
|  [03]   | `MsgPack003`    | Error      | `TypeMustBeMessagePackObject`                         | type serialized without `[MessagePackObject]`             |
|  [04]   | `MsgPack004`    | Error      | `MemberNeedsKey`                                      | member without `[Key]` or `[IgnoreMember]`                |
|  [05]   | `MsgPack005`    | Error      | `InvalidMessagePackObject` / `UnionAttributeRequired` | invalid contract shape                                    |
|  [06]   | `MsgPack006`    | Error      | `MessageFormatterMustBeMessagePackFormatter`          | `[MessagePackFormatter]` type not `IMessagePackFormatter` |
|  [07]   | `MsgPack007`    | Error      | `NoDeserializingConstructor` / parameter diagnostics  | missing or mismatched deserializing constructor           |
|  [08]   | `MsgPack008`    | Error      | `AotUnionAttributeRequiresTypeArg`                    | `[Union]` without type argument in AOT mode               |
|  [09]   | `MsgPack009`    | Error      | `CollidingFormatters`                                 | multiple formatters for the same type                     |
|  [10]   | `MsgPack010`    | —          | `InaccessibleFormatterTypeId`                         | formatter type inaccessible to generator                  |
|  [11]   | `MsgPack011`    | —          | `PartialTypeRequiredId`                               | generated type not declared `partial`                     |
|  [12]   | `MsgPack012`    | —          | `InaccessibleDataTypeId`                              | data type inaccessible to formatter                       |
|  [13]   | `MsgPack013`    | —          | `InaccessibleFormatterInstanceId`                     | formatter instance inaccessible to resolver               |
|  [14]   | `MsgPack014`    | —          | `NullableReferenceTypeFormatterId`                    | nullable reference type without nullable formatter        |
|  [15]   | `MsgPack015`    | Warning    | `MessagePackObjectAllowPrivateRequired`               | non-public type or member needs `AllowPrivate = true`     |
|  [16]   | `MsgPack016`    | —          | `AOTDerivedKeyId`                                     | derived key used in AOT mode                              |
|  [17]   | `MsgPack017`    | —          | `AOTInitPropertyId`                                   | init property in AOT mode                                 |
|  [18]   | `MsgPack018`    | Error      | `CollidingMemberNamesInForceMapMode`                  | duplicate member name in force-map mode                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: diagnostics and generated surfaces
- rail: snapshot-codec

| [INDEX] | [SURFACE]                                              | [CALL_SHAPE]       | [CAPABILITY]                                    |
| :-----: | :----------------------------------------------------- | :----------------- | :---------------------------------------------- |
|  [01]   | `MsgPack003` (`TypeMustBeMessagePackObject`)           | error diagnostic   | blocks unattributed serialized types            |
|  [02]   | `MsgPack015` (`MessagePackObjectAllowPrivateRequired`) | warning diagnostic | enforces `AllowPrivate` on non-public contracts |
|  [03]   | `MsgPack004` (`MemberNeedsKey`)                        | error diagnostic   | blocks unkeyed members                          |
|  [04]   | `MsgPack009` (`CollidingFormatters`)                   | error diagnostic   | blocks colliding formatters                     |
|  [05]   | `MsgPack005` (`InvalidMessagePackObject`)              | error diagnostic   | blocks invalid contract shapes                  |
|  [06]   | `MsgPack005` (`UnionAttributeRequired`)                | error diagnostic   | blocks missing `[Union]`                        |
|  [07]   | generated formatter class                              | generated output   | formats snapshot values                         |
|  [08]   | generated resolver class                               | generated output   | resolves snapshot values                        |

## [04]-[IMPLEMENTATION_LAW]

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
