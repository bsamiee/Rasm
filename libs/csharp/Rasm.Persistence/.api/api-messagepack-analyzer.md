# [RASM_PERSISTENCE_API_MESSAGEPACK_ANALYZER]

`MessagePackAnalyzer` ships the MessagePack source generator and the Roslyn analyzer/code-fix assets
that gate the `Version/snapshots#CODEC_AXIS` codec: it emits the AOT formatters/resolvers the
`PersistenceResolver`/`GeneratedMessagePackResolver` landmark binds and raises the MsgPack### contract
diagnostics that reject an unattributed, unkeyed, colliding, inaccessible, or AOT-incompatible
`[MessagePackObject]` before runtime. It is an analyzer-only development dependency — its assets never
enter the runtime closure.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MessagePackAnalyzer`
- package: `MessagePackAnalyzer` (3.1.7; pairs the runtime `MessagePack` 3.1.7)
- assets: `analyzers/roslyn4.3/cs/MessagePack.SourceGenerator.dll`, `analyzers/roslyn4.3/cs/MessagePack.Analyzers.CodeFixes.dll`
- abi: Roslyn 4.3 analyzer ABI (`Microsoft.CodeAnalysis` ≥ 4.3); `<developmentDependency>true</developmentDependency>`
- namespace: `MessagePack.SourceGenerator`, `MessagePack.SourceGenerator.Analyzers`, `MessagePack.SourceGenerator.Transforms`, `MessagePack.Analyzers.CodeFixes`
- license: MIT
- asset: analyzer + incremental source generator (compile-time only)
- rail: snapshot-codec

## [02]-[GENERATOR_ANALYZER_ASSETS]

[GENERATOR_SCOPE]: incremental generators + code-fix providers
- rail: snapshot-codec

| [INDEX] | [SYMBOL]                                                  | [PACKAGE_ROLE]        | [CAPABILITY]                                        |
| :-----: | :-------------------------------------------------------- | :-------------------- | :-------------------------------------------------- |
|  [01]   | `MessagePack.SourceGenerator.MessagePackGenerator`        | incremental generator | emits per-type formatters from `[MessagePackObject]` contracts |
|  [02]   | `MessagePack.SourceGenerator.CompositeResolverGenerator`  | incremental generator | emits the `[GeneratedMessagePackResolver]`/`[CompositeResolver]` resolver class |
|  [03]   | `MessagePack.Analyzers.CodeFixes.MessagePackCodeFixProvider` | code-fix provider   | fixes MsgPack003 (add `[MessagePackObject]`) and MsgPack004 (add `[Key]`/`[IgnoreMember]`) |
|  [04]   | `MessagePack.Analyzers.CodeFixes.FormatterCodeFixProvider`  | code-fix provider   | fixes formatter-accessibility diagnostics (MsgPack010/013) |
|  [05]   | `MessagePack.Analyzers.CodeFixes.MsgPack015CodeFixProvider` | code-fix provider    | fixes MsgPack015 — sets `MessagePackObject(AllowPrivate = true)` |

[GENERATOR_TEMPLATE_SCOPE]: `MessagePack.SourceGenerator.Transforms` emitted shapes
- rail: snapshot-codec

| [INDEX] | [SYMBOL]                            | [PACKAGE_ROLE]   | [CAPABILITY]                                        |
| :-----: | :---------------------------------- | :--------------- | :-------------------------------------------------- |
|  [01]   | `FormatterTemplate`                 | generated shape  | array-keyed `[Key(int)]` formatter body             |
|  [02]   | `StringKeyFormatterTemplate`        | generated shape  | map-keyed `[Key(string)]` / map-mode formatter body |
|  [03]   | `UnionTemplate`                     | generated shape  | `[Union]` polymorphic dispatch formatter            |
|  [04]   | `EnumTemplate`                      | generated shape  | enum formatter body                                 |
|  [05]   | `ResolverTemplate`                  | generated shape  | per-assembly `IFormatterResolver` body              |
|  [06]   | `CompositeResolverTemplate`         | generated shape  | `[CompositeResolver]` aggregate resolver body       |

[RECOGNIZED_CONTRACT_SCOPE]: attributes the generator/analyzer reads (defined in runtime `MessagePack`)
- rail: snapshot-codec

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE]        | [CAPABILITY]                                |
| :-----: | :-------------------------------------- | :-------------------- | :------------------------------------------ |
|  [01]   | `MessagePackObjectAttribute`            | contract attribute    | marks a serialised type; `AllowPrivate`/map-mode ctor args |
|  [02]   | `KeyAttribute`                          | contract attribute    | declares an int or string member key        |
|  [03]   | `IgnoreMemberAttribute`                 | contract attribute    | excludes a public member                    |
|  [04]   | `UnionAttribute`                        | contract attribute    | declares a polymorphic union case (Type arg) |
|  [05]   | `MessagePackFormatterAttribute`         | formatter attribute   | selects an `IMessagePackFormatter` for a type |
|  [06]   | `SerializationConstructorAttribute`     | constructor attribute | selects the deserialising constructor       |
|  [07]   | `GeneratedMessagePackResolverAttribute` | resolver attribute    | marks the partial class the resolver generator fills |
|  [08]   | `CompositeResolverAttribute`            | resolver attribute    | declares the ordered resolver inputs of an aggregate resolver |

## [03]-[DIAGNOSTICS]

`MsgPack003`–`MsgPack018` are `DiagnosticDescriptor` fields on
`MessagePack.SourceGenerator.Analyzers.MsgPack00xMessagePackAnalyzer` (`SupportedDiagnostics` carries
27 descriptors — several IDs own multiple descriptors); `MsgPack001`/`MsgPack002` are single
descriptors on their named `DiagnosticAnalyzer` classes. Category is `Usage` for MsgPack003–018 and
`Reliability` for MsgPack001/002; `GetHelpLink` resolves each ID's help URL.

| [INDEX] | [DIAGNOSTIC_ID] | [SEVERITY] | [DESCRIPTOR(S)]                                                           | [TRIGGER]                                                 |
| :-----: | :-------------- | :--------- | :----------------------------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `MsgPack001`    | Warning (off-by-default) | `MissingOptionsDescriptor`                                  | `MessagePackSerializer` call without explicit options     |
|  [02]   | `MsgPack002`    | Warning (off-by-default) | `MutableSharedOptionsDescriptor`                            | mutable shared `MessagePackSerializerOptions` (mutate after init) |
|  [03]   | `MsgPack003`    | Error      | `TypeMustBeMessagePackObject`                                            | type serialised without `[MessagePackObject]`             |
|  [04]   | `MsgPack004`    | Error      | `MemberNeedsKey`, `BaseTypeContainsUnattributedPublicMembers`            | member (or base-type member) without `[Key]`/`[IgnoreMember]` |
|  [05]   | `MsgPack005`    | Error      | `InvalidMessagePackObject`, `BothStringAndIntKeyAreNull`, `DoNotMixStringAndIntKeys`, `KeysMustBeUnique`, `UnionAttributeRequired`, `KeyAnnotatedMemberInMapMode` | invalid contract: null/mixed/duplicate keys, missing `[Union]`, key in map mode |
|  [06]   | `MsgPack006`    | Error      | `MessageFormatterMustBeMessagePackFormatter`                             | `[MessagePackFormatter]` type not `IMessagePackFormatter` |
|  [07]   | `MsgPack007`    | Error      | `NoDeserializingConstructor` + parameter type/count/name/duplicate descriptors | missing or mismatched deserialising constructor      |
|  [08]   | `MsgPack008`    | Error      | `AotUnionAttributeRequiresTypeArg`, `AotArrayRankTooHigh`               | `[Union]` without Type arg, or array rank too high, under AOT generation |
|  [09]   | `MsgPack009`    | Error      | `CollidingFormatters`                                                    | multiple formatters registered for one type              |
|  [10]   | `MsgPack010`    | Warning    | `InaccessibleFormatterType`                                             | formatter type below internal visibility — omitted from resolver |
|  [11]   | `MsgPack011`    | Error      | `PartialTypeRequired`                                                    | type (or nesting type) with private serialised members not `partial` |
|  [12]   | `MsgPack012`    | Error      | `InaccessibleDataType`                                                   | serialised data type below internal visibility           |
|  [13]   | `MsgPack013`    | Warning    | `InaccessibleFormatterInstance`                                          | formatter lacks accessible ctor or `static readonly Instance` |
|  [14]   | `MsgPack014`    | Warning    | `NullableReferenceTypeFormatter`                                        | nullable reference type without nullable-annotated formatter |
|  [15]   | `MsgPack015`    | Warning    | `MessagePackObjectAllowPrivateRequired`                                 | non-public type/member serialised without `AllowPrivate = true` |
|  [16]   | `MsgPack016`    | Error      | `AOTDerivedKeyAttribute`                                                | `KeyAttribute`-derived attribute under AOT generation     |
|  [17]   | `MsgPack017`    | Warning    | `AOTInitProperty`                                                       | init-accessor property with initializer reset on AOT deserialise |
|  [18]   | `MsgPack018`    | Error      | `CollidingMemberNamesInForceMapMode`                                    | duplicate serialised member name in force-map mode        |

DiagnosticSeverity numerics: `2`=Warning, `3`=Error. MsgPack001/002 are `isEnabledByDefault: false`
(opt-in `.editorconfig` escalation); every MsgPack003–018 descriptor is enabled by default.

## [04]-[IMPLEMENTATION_LAW]

[ANALYZER_ADMISSION]:
- dependency role: analyzer-only development dependency (`PrivateAssets="all"`); the two assets resolve under `analyzers/roslyn4.3/cs/` and never become a runtime `MessagePack` dependency.
- generator root: `MessagePackGenerator` + `CompositeResolverGenerator` as `IIncrementalGenerator`; their emitted bodies are the `Transforms.*Template` shapes.
- analyzer root: `MsgPack00xMessagePackAnalyzer` (contract correctness, MsgPack003–018), `MsgPack001SpecifyOptionsAnalyzer`, `MsgPack002UseConstantOptionsAnalyzer` (reliability, opt-in).
- code-fix root: `MessagePackCodeFixProvider`, `FormatterCodeFixProvider`, `MsgPack015CodeFixProvider`.
- output root: the `[GeneratedMessagePackResolver]` partial filled by the resolver generator and the per-type formatters in the array/string-key/union/enum templates.

[LOCAL_ADMISSION]:
- The analyzer gates the snapshot codec shape: `Version/snapshots#CODEC_AXIS` declares `[MessagePackObject]` records over `[Key(int)]` array keys (the array template, not map mode), so MsgPack003/004/005 reject an unattributed or unkeyed snapshot type at build before the `PersistenceResolver` resolver chain (`InstantFormatter.Instance` → `ThinktectureMessageFormatterResolver.Instance` → `SourceGeneratedFormatterResolver.Instance` → `StandardResolver.Instance`) can bind it.
- The AOT path is the load-bearing one: the `GeneratedMessagePackResolver` partial carrying `[CompositeResolverAttribute]` over `[GeneratedMessagePackResolverAttribute]` is filled by `CompositeResolverGenerator`, and MsgPack008/011/012/013/016 are the diagnostics that keep a published-AOT build's generated resolver able to construct every formatter — so a private serialised member forces `partial` (MsgPack011) and `AllowPrivate = true` (MsgPack015) rather than a runtime reflection fallback.
- MsgPack001/002 stay opt-in: the rail's serializer-options discipline (one frozen `MessagePackSerializerOptions Binary`/`Foreign` per snapshot context) is enforced by review and an `.editorconfig` escalation of these two to error where a call site could pass ad-hoc options, never by their default state.
- Thinktecture value-objects/smart-enums/keyed-unions are formatted by the registered `ThinktectureMessageFormatterResolver`, so the generator never sees those types directly — the analyzer's contract diagnostics apply only to the package's own `[MessagePackObject]` wire records, and a hand-written formatter beside a generated one is the MsgPack009 collision the rail rejects.

[RAIL_LAW]:
- Package: `MessagePackAnalyzer`
- Owns: the MessagePack source generator, contract analyzers, and code-fix providers
- Accept: generated formatter/resolver assets proving the AOT codec; MsgPack003–018 enforced at build
- Reject: a runtime dependency on the analyzer assets, a hand-written formatter colliding with a generated one, an unattributed/unkeyed/non-`partial` snapshot contract
