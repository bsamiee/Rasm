# [RASM_PERSISTENCE_API_MESSAGEPACK_ANALYZER]

`MessagePackAnalyzer` ships the MessagePack source generator and the Roslyn analyzer/code-fix assets gating the snapshot codec. Its generator emits the AOT formatters and resolvers the `PersistenceResolver`/`GeneratedMessagePackResolver` landmark binds; its analyzer raises the `MsgPack###` contract diagnostics rejecting an unattributed, unkeyed, colliding, inaccessible, or AOT-incompatible `[MessagePackObject]` before serialize.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MessagePackAnalyzer`
- package: `MessagePackAnalyzer` (MIT)
- assembly: `analyzers/roslyn4.3/cs/MessagePack.SourceGenerator.dll`, `analyzers/roslyn4.3/cs/MessagePack.Analyzers.CodeFixes.dll`
- namespace: `MessagePack.SourceGenerator`, `MessagePack.SourceGenerator.Analyzers`, `MessagePack.SourceGenerator.Transforms`, `MessagePack.Analyzers.CodeFixes`
- abi: Roslyn 4.3 analyzer generation, selected by the `analyzers/roslyn4.3/cs/` asset path
- role: analyzer-only development dependency (`<developmentDependency>true</developmentDependency>`, `PrivateAssets="all"`)
- rail: snapshot-codec

## [02]-[GENERATOR_ANALYZER_ASSETS]

[GENERATOR_SCOPE]: incremental generators under `MessagePack.SourceGenerator`, code-fix providers under `MessagePack.Analyzers.CodeFixes`

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]         | [CAPABILITY]                                                   |
| :-----: | :--------------------------- | :-------------------- | :------------------------------------------------------------- |
|  [01]   | `MessagePackGenerator`       | incremental generator | emits per-type formatters from `[MessagePackObject]` contracts |
|  [02]   | `CompositeResolverGenerator` | incremental generator | emits the `[GeneratedMessagePackResolver]` partial's resolver  |
|  [03]   | `MessagePackCodeFixProvider` | code-fix provider     | adds `[MessagePackObject]`, `[Key]`, `[IgnoreMember]`          |
|  [04]   | `FormatterCodeFixProvider`   | code-fix provider     | fixes formatter accessibility                                  |
|  [05]   | `MsgPack015CodeFixProvider`  | code-fix provider     | sets `MessagePackObject(AllowPrivate = true)`                  |

[GENERATOR_TEMPLATE_SCOPE]: `MessagePack.SourceGenerator.Transforms` emitted bodies

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]   | [CAPABILITY]                                        |
| :-----: | :--------------------------- | :-------------- | :-------------------------------------------------- |
|  [01]   | `FormatterTemplate`          | generated shape | array-keyed `[Key(int)]` formatter body             |
|  [02]   | `StringKeyFormatterTemplate` | generated shape | map-keyed `[Key(string)]` / map-mode formatter body |
|  [03]   | `UnionTemplate`              | generated shape | `[Union]` polymorphic dispatch formatter            |
|  [04]   | `EnumTemplate`               | generated shape | enum formatter body                                 |
|  [05]   | `ResolverTemplate`           | generated shape | per-assembly `IFormatterResolver` body              |
|  [06]   | `CompositeResolverTemplate`  | generated shape | `[CompositeResolver]` aggregate resolver body       |

Generator and analyzer both read the `[MessagePackObject]` contract attributes `api-messagepack` owns: a snapshot type declares them, this package validates and emits from them.

## [03]-[DIAGNOSTICS]

`MsgPack003`–`MsgPack018` are `DiagnosticDescriptor` fields on `MessagePack.SourceGenerator.Analyzers.MsgPack00xMessagePackAnalyzer`; `MsgPack001` and `MsgPack002` are single descriptors on `MsgPack001SpecifyOptionsAnalyzer` and `MsgPack002UseConstantOptionsAnalyzer`. Category is `Usage` for `MsgPack003`–`018` and `Reliability` for `MsgPack001`/`002`; `AnalyzerUtilities.GetHelpLink` resolves each ID's help URL.

| [INDEX] | [DIAGNOSTIC_ID] | [SEVERITY] | [TRIGGER]                                                                       |
| :-----: | :-------------- | :--------- | :------------------------------------------------------------------------------ |
|  [01]   | `MsgPack001`    | Warning    | `MessagePackSerializer` call without explicit options                           |
|  [02]   | `MsgPack002`    | Warning    | mutable shared `MessagePackSerializerOptions` (mutate after init)               |
|  [03]   | `MsgPack003`    | Error      | type serialised without `[MessagePackObject]`                                   |
|  [04]   | `MsgPack004`    | Error      | member (or base-type member) without `[Key]`/`[IgnoreMember]`                   |
|  [05]   | `MsgPack005`    | Error      | invalid contract: null/mixed/duplicate keys, missing `[Union]`, key in map mode |
|  [06]   | `MsgPack006`    | Error      | `[MessagePackFormatter]` type not `IMessagePackFormatter`                       |
|  [07]   | `MsgPack007`    | Error      | missing or mismatched deserialising constructor                                 |
|  [08]   | `MsgPack008`    | Error      | `[Union]` without Type arg, or array rank too high, under AOT generation        |
|  [09]   | `MsgPack009`    | Error      | multiple formatters registered for one type                                     |
|  [10]   | `MsgPack010`    | Warning    | formatter type below internal visibility — omitted from resolver                |
|  [11]   | `MsgPack011`    | Error      | type (or nesting type) with private serialised members not `partial`            |
|  [12]   | `MsgPack012`    | Error      | serialised data type below internal visibility                                  |
|  [13]   | `MsgPack013`    | Warning    | formatter lacks accessible ctor or `static readonly Instance`                   |
|  [14]   | `MsgPack014`    | Warning    | nullable reference type without nullable-annotated formatter                    |
|  [15]   | `MsgPack015`    | Warning    | non-public type/member serialised without `AllowPrivate = true`                 |
|  [16]   | `MsgPack016`    | Error      | `KeyAttribute`-derived attribute under AOT generation                           |
|  [17]   | `MsgPack017`    | Warning    | init-accessor property with initializer reset on AOT deserialise                |
|  [18]   | `MsgPack018`    | Error      | duplicate serialised member name in force-map mode                              |

`MsgPack001` and `MsgPack002` ship `isEnabledByDefault: false`, escalating to error only through `.editorconfig`; every `MsgPack003`–`018` descriptor is enabled by default.

[DESCRIPTORS]: the `DiagnosticDescriptor` field(s) each ID owns.
- [01]-[MSGPACK001]: `MissingOptionsDescriptor`
- [02]-[MSGPACK002]: `MutableSharedOptionsDescriptor`
- [03]-[MSGPACK003]: `TypeMustBeMessagePackObject`
- [04]-[MSGPACK004]: `MemberNeedsKey`, `BaseTypeContainsUnattributedPublicMembers`
- [05]-[MSGPACK005]: `InvalidMessagePackObject`, `BothStringAndIntKeyAreNull`, `DoNotMixStringAndIntKeys`, `KeysMustBeUnique`, `UnionAttributeRequired`, `KeyAnnotatedMemberInMapMode`
- [06]-[MSGPACK006]: `MessageFormatterMustBeMessagePackFormatter`
- [07]-[MSGPACK007]: `NoDeserializingConstructor`, `DeserializingConstructorParameterTypeMismatch`, `DeserializingConstructorParameterIndexMissing`, `DeserializingConstructorParameterNameMissing`, `DeserializingConstructorParameterNameDuplicate`
- [08]-[MSGPACK008]: `AotUnionAttributeRequiresTypeArg`, `AotArrayRankTooHigh`
- [09]-[MSGPACK009]: `CollidingFormatters`
- [10]-[MSGPACK010]: `InaccessibleFormatterType`
- [11]-[MSGPACK011]: `PartialTypeRequired`
- [12]-[MSGPACK012]: `InaccessibleDataType`
- [13]-[MSGPACK013]: `InaccessibleFormatterInstance`
- [14]-[MSGPACK014]: `NullableReferenceTypeFormatter`
- [15]-[MSGPACK015]: `MessagePackObjectAllowPrivateRequired`
- [16]-[MSGPACK016]: `AOTDerivedKeyAttribute`
- [17]-[MSGPACK017]: `AOTInitProperty`
- [18]-[MSGPACK018]: `CollidingMemberNamesInForceMapMode`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every `[MessagePackObject]` contract folds through one compile-time gate: the generators emit its formatter and resolver as `Transforms.*Template` bodies and `MsgPack00xMessagePackAnalyzer` refuses any contract the AOT resolver cannot construct, so a build that compiles carries a constructible resolver and no reflection fallback.

[STACKING]:
- `api-messagepack`(`.api/api-messagepack.md`): the generated formatters and the `GeneratedMessagePackResolver` partial back the `PersistenceResolver` chain (`InstantFormatter.Instance` → `ThinktectureMessageFormatterResolver.Instance` → `SourceGeneratedFormatterResolver.Instance` → `StandardResolver.Instance`) that `Element/codec` binds, so every serialize-time contract fault surfaces at compile instead.
- codec floor: `MsgPack008`/`011`/`012`/`016` prove the AOT-generated resolver constructs every formatter, so the codec bytes the `Version/timetravel` fold and the `api-redis`/`api-nats` snapshot wire replay read are constructible under published AOT before any store profile writes them.

[LOCAL_ADMISSION]:
- `Element/codec` declares `[MessagePackObject]` records over `[Key(int)]` array keys, so `MsgPack003`/`004`/`005` reject an unattributed or unkeyed snapshot type at build before the resolver chain binds it.
- Published AOT is the load-bearing path: `CompositeResolverGenerator` fills the `GeneratedMessagePackResolver` partial carrying `[CompositeResolverAttribute]` over `[GeneratedMessagePackResolverAttribute]`, and a private serialised member takes `partial` (`MsgPack011`) with `AllowPrivate = true` (`MsgPack015`).
- `MsgPack001`/`MsgPack002` stay opt-in: one frozen `MessagePackSerializerOptions` per snapshot context is held by review and an `.editorconfig` escalation to error at any call site passing ad-hoc options, never by their default state.
- `ThinktectureMessageFormatterResolver` formats the value-objects, smart-enums, and keyed unions, so the generator sees only this package's own `[MessagePackObject]` wire records; a hand-written formatter beside a generated one is the `MsgPack009` collision.

[RAIL_LAW]:
- Package: `MessagePackAnalyzer`
- Owns: the MessagePack source generator, contract analyzers, and code-fix providers
- Accept: generated formatter/resolver assets proving the AOT codec, `MsgPack003`–`018` enforced at build
- Reject: a runtime dependency on the analyzer assets, a hand-written formatter colliding with a generated one, an unattributed/unkeyed/non-`partial` snapshot contract
