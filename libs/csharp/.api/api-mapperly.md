# [RASM_API_MAPPERLY]

`Riok.Mapperly` transcribes one declared shape into another at compile time: a `[Mapper]` partial class declares mapping-method signatures and configuration attributes, and the Roslyn generator fills each body with ordinary assignment, construction, loop, and type-switch C#. Boundary transcription between the canonical graph vocabulary and wire, DTO, and row shapes is its whole charter — identity, hashing, equality, and the failure rail stay with their owners.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Riok.Mapperly`
- package: `Riok.Mapperly` (Apache-2.0)
- assembly: `Riok.Mapperly.Abstractions` carries the attribute and enum surface; `Riok.Mapperly` ships the Roslyn generator under `analyzers/dotnet/cs` and never binds at runtime
- namespace: `Riok.Mapperly.Abstractions`, `Riok.Mapperly.Abstractions.ReferenceHandling`
- rail: mapping (boundary transcription)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: every attribute the generator reads — declaration and composition at class and assembly level, member and enum configuration at method level, each cell carrying the abbreviated constructor shape.

| [INDEX] | [SYMBOL]                                                              | [TYPE_FAMILY]        | [CAPABILITY]                     |
| :-----: | :-------------------------------------------------------------------- | :------------------- | :------------------------------- |
|  [01]   | `MapperAttribute`                                                     | attribute            | mapper declaration and policy    |
|  [02]   | `MapperDefaultsAttribute`                                             | attribute            | assembly-wide policy defaults    |
|  [03]   | `UseMapperAttribute`                                                  | attribute            | field or property delegation     |
|  [04]   | `UseStaticMapperAttribute(Type)`                                      | repeatable attribute | named static-mapper delegation   |
|  [05]   | `UseStaticMapperAttribute<T>`                                         | repeatable attribute | generic static-mapper delegation |
|  [06]   | `MapperConstructorAttribute`                                          | attribute            | target constructor selection     |
|  [07]   | `ObjectFactoryAttribute`                                              | attribute            | target factory selection         |
|  [08]   | `MapperIgnoreAttribute`                                               | attribute            | method or member exclusion       |
|  [09]   | `UserMappingAttribute`                                                | attribute            | hand-written mapping admission   |
|  [10]   | `NamedMappingAttribute(string)`                                       | attribute            | names a mapping for `Use`        |
|  [11]   | `IncludeMappingConfigurationAttribute(string)`                        | attribute            | configuration reuse by method    |
|  [12]   | `MappingTargetAttribute`                                              | attribute            | existing-target parameter mark   |
|  [13]   | `FormatProviderAttribute`                                             | attribute            | `IFormatProvider` member binding |
|  [14]   | `ReferenceHandlerAttribute`                                           | attribute            | handler parameter binding        |
|  [15]   | `MapPropertyAttribute(string\|string[], string\|string[])`            | repeatable attribute | rename flatten and unflatten     |
|  [16]   | `MapPropertyFromSourceAttribute(string\|string[])`                    | repeatable attribute | whole-source member mapping      |
|  [17]   | `MapNestedPropertiesAttribute(string\|string[])`                      | repeatable attribute | nested-source root flattening    |
|  [18]   | `MapValueAttribute(string\|string[], object?)`                        | repeatable attribute | constant or computed assignment  |
|  [19]   | `MapperIgnoreSourceAttribute(string)`                                 | repeatable attribute | source member exclusion          |
|  [20]   | `MapperIgnoreTargetAttribute(string)`                                 | repeatable attribute | target member exclusion          |
|  [21]   | `MapperRequiredMappingAttribute(RequiredMappingStrategy)`             | attribute            | per-method diagnostic policy     |
|  [22]   | `MapperIgnoreObsoleteMembersAttribute(IgnoreObsoleteMembersStrategy)` | attribute            | per-method obsolete policy       |
|  [23]   | `MapDerivedTypeAttribute(Type, Type)`                                 | repeatable attribute | derived-type switch arm          |
|  [24]   | `MapDerivedTypeAttribute<TSource, TTarget>`                           | repeatable attribute | generic derived-type switch arm  |
|  [25]   | `MapEnumAttribute(EnumMappingStrategy)`                               | attribute            | per-method enum policy           |
|  [26]   | `MapEnumValueAttribute(object, object)`                               | repeatable attribute | explicit enum-member pairing     |
|  [27]   | `MapperIgnoreSourceValueAttribute(object)`                            | repeatable attribute | source enum-value exclusion      |
|  [28]   | `MapperIgnoreTargetValueAttribute(object)`                            | repeatable attribute | target enum-value exclusion      |

A paired constructor orders its arguments source then target. Each path-carrying attribute splits a `string` argument on `.` into segments while the `string[]` overload takes segments verbatim, exposing them as `Source`/`Target` and the rejoined path as `SourceFullName`/`TargetFullName`. Settable properties an author sets past the constructor:

[MapPropertyAttribute]: `StringFormat` `FormatProvider` `Use` `SuppressNullMismatchDiagnostic`
[MapPropertyFromSourceAttribute]: `StringFormat` `FormatProvider` `Use`
[MapValueAttribute]: `Use`
[MapEnumAttribute]: `IgnoreCase` `FallbackValue` `NamingStrategy`
[UserMappingAttribute]: `Default` `Ignore`
[FormatProviderAttribute]: `Default`

[PUBLIC_TYPE_SCOPE]: `MapperAttribute` policy properties, each also settable through `MapperDefaultsAttribute` and each overridable by the narrower declaration.

| [INDEX] | [PROPERTY]                           | [TYPE]                          | [DEFAULT]       | [EFFECT]                     |
| :-----: | :----------------------------------- | :------------------------------ | :-------------- | :--------------------------- |
|  [01]   | `PropertyNameMappingStrategy`        | `PropertyNameMappingStrategy`   | `CaseSensitive` | member-name matching         |
|  [02]   | `EnumMappingStrategy`                | `EnumMappingStrategy`           | `ByValue`       | enum-member matching         |
|  [03]   | `EnumNamingStrategy`                 | `EnumNamingStrategy`            | `MemberName`    | enum-string naming           |
|  [04]   | `EnumMappingIgnoreCase`              | `bool`                          | `false`         | enum-match casing            |
|  [05]   | `ThrowOnMappingNullMismatch`         | `bool`                          | `true`          | null-return mismatch         |
|  [06]   | `ThrowOnPropertyMappingNullMismatch` | `bool`                          | `false`         | null-property mismatch       |
|  [07]   | `AllowNullPropertyAssignment`        | `bool`                          | `true`          | nullable-property assignment |
|  [08]   | `UseDeepCloning`                     | `bool`                          | `false`         | same-type member cloning     |
|  [09]   | `EnabledConversions`                 | `MappingConversionType`         | `All`           | admitted conversion set      |
|  [10]   | `UseReferenceHandling`               | `bool`                          | `false`         | reference-cycle identity     |
|  [11]   | `IgnoreObsoleteMembersStrategy`      | `IgnoreObsoleteMembersStrategy` | `None`          | obsolete-member policy       |
|  [12]   | `RequiredMappingStrategy`            | `RequiredMappingStrategy`       | `Both`          | member diagnostic policy     |
|  [13]   | `RequiredEnumMappingStrategy`        | `RequiredMappingStrategy`       | `Both`          | enum diagnostic policy       |
|  [14]   | `IncludedMembers`                    | `MemberVisibility`              | `AllAccessible` | admitted member visibility   |
|  [15]   | `IncludedConstructors`               | `MemberVisibility`              | `AllAccessible` | admitted constructor scope   |
|  [16]   | `PreferParameterlessConstructors`    | `bool`                          | `true`          | constructor preference       |
|  [17]   | `AutoUserMappings`                   | `bool`                          | `true`          | user-mapping discovery       |

Null-return mismatch throws when enabled and otherwise returns a default; null-property mismatch throws when enabled and otherwise suppresses the assignment.

[PUBLIC_TYPE_SCOPE]: strategy and conversion vocabularies. `IgnoreObsoleteMembersStrategy`, `MemberVisibility`, `MappingConversionType`, and `RequiredMappingStrategy` are `[Flags]` sets combined with `|` and narrowed with `& ~`; `None = 0` and `All = -1` are the members whose numeric value is the contract. `MemberVisibility.AllAccessible` sets every bit including `Accessible`, while `All` clears `Accessible` so inaccessible members route through `UnsafeAccessor`.

[EnumMappingStrategy]: `ByValue` `ByName` `ByValueCheckDefined`
[EnumNamingStrategy]: `MemberName` `CamelCase` `PascalCase` `SnakeCase` `UpperSnakeCase` `KebabCase` `UpperKebabCase` `ComponentModelDescriptionAttribute` `SerializationEnumMemberAttribute`
[PropertyNameMappingStrategy]: `CaseSensitive` `CaseInsensitive`
[RequiredMappingStrategy]: `None = 0` `Both = -1` `Source` `Target`
[IgnoreObsoleteMembersStrategy]: `None = 0` `Both = -1` `Source` `Target`
[MemberVisibility]: `AllAccessible` `All` `Accessible` `Public` `Internal` `Protected` `Private`
[MappingConversionType]: `None = 0` `Constructor` `ImplicitCast` `ExplicitCast` `ParseMethod` `ToStringMethod` `StringToEnum` `EnumToString` `EnumToEnum` `DateTimeToDateOnly` `DateTimeToTimeOnly` `Queryable` `Enumerable` `Dictionary` `Span` `Memory` `Tuple` `EnumUnderlyingType` `ToTargetMethod` `StaticConvertMethods` `All = -1`

[PUBLIC_TYPE_SCOPE]: reference handling (`Riok.Mapperly.Abstractions.ReferenceHandling`), threaded through every mapping method once `UseReferenceHandling` is set.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                    |
| :-----: | :------------------------- | :------------ | :------------------------------ |
|  [01]   | `IReferenceHandler`        | interface     | cycle identity registry         |
|  [02]   | `PreserveReferenceHandler` | sealed class  | generator-owned reference table |

- `IReferenceHandler`: resolves through `TryGetReference<TSource, TTarget>(TSource, out TTarget?)` and records through `SetReference<TSource, TTarget>(TSource, TTarget)`, both constraining source and target to `notnull`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the generation contract — declare the `partial` method shape on a `[Mapper] partial class` or `[Mapper] static partial class`, and the generator emits its body.

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :-------------------------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `Map(TSource) -> TTarget`                                       | instance | target construction            |
|  [02]   | `Map(this TSource) -> TTarget`                                  | static   | extension mapping              |
|  [03]   | `Map(TSource?) -> TTarget?`                                     | instance | nullable mapping               |
|  [04]   | `Update(TSource, [MappingTarget] TTarget)`                      | instance | existing-target update         |
|  [05]   | `ProjectTo(this IQueryable<TSource>) -> IQueryable<TTarget>`    | static   | in-store queryable projection  |
|  [06]   | `Map<TSource, TTarget>(TSource, TTarget)`                       | instance | generic existing-target update |
|  [07]   | `Map<TTarget>(TSource) -> TTarget`                              | instance | generic target mapping         |
|  [08]   | `Map(TSource, [ReferenceHandler] IReferenceHandler) -> TTarget` | instance | cycle-safe mapping             |
|  [09]   | `Map(TSource, TContext) -> TTarget`                             | instance | additional-value mapping       |
|  [10]   | `[MapDerivedType<TSource, TTarget>] Map(TBase) -> TBaseTarget`  | instance | derived-type switch            |

- `Map(TSource, TContext)`: an additional parameter maps to a same-name target member after explicit `[MapProperty]` configuration and before source-member matching.

[ENTRYPOINT_SCOPE]: mapper declaration, delegation, and activation forms.

| [INDEX] | [SURFACE]                                      | [SHAPE] | [CAPABILITY]                   |
| :-----: | :--------------------------------------------- | :------ | :----------------------------- |
|  [01]   | `[Mapper] partial class Shape`                 | ctor    | instance mapper root           |
|  [02]   | `[Mapper] static partial class Shape`          | ctor    | static mapper root             |
|  [03]   | `[assembly: MapperDefaults]`                   | ctor    | assembly policy root           |
|  [04]   | `[UseMapper] private readonly Mapper _mapper;` | ctor    | instance mapper delegation     |
|  [05]   | `[UseStaticMapper(typeof(Mapper))]`            | ctor    | static mapper delegation       |
|  [06]   | `[UserMapping] private TTarget Map(TSource)`   | ctor    | hand-written mapping admission |
|  [07]   | `[ObjectFactory] TTarget Create()`             | factory | parameterless activation       |
|  [08]   | `[ObjectFactory] TTarget Create(TSource)`      | factory | source-passed activation       |

- `[ObjectFactory]`: admits any non-void method with zero or one parameter; type parameters stand for the source, the target, or both, and the source object is passed wherever the parameter exists.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every attribute carries `[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]`, so the compiler reads its syntax and elides it from consumer IL.
- A `partial` mapping method's body is ordinary inspectable C# — direct assignment, object initialization, element loops, derived-type switches — leaving no mapping engine, runtime expression compilation, or reflection; target construction owns per-map allocation.
- `ProjectTo` emits a static expression tree the LINQ provider translates, so a projection never materializes the source graph; reference handling and projection are mutually exclusive by construction.
- An unmapped member raises an `RMG` build diagnostic: `RequiredMappingStrategy` and `MapperRequiredMappingAttribute` set severity, `MapperIgnoreSourceAttribute` and `MapperIgnoreTargetAttribute` waive one member explicitly.
- Configuration resolves outward-in — assembly defaults, then class policy, then method attributes — so the narrowest declaration wins.

[STACKING]:
- `Google.Protobuf`(`.api/api-protobuf.md`): protobuf message classes are the wire DTOs each mapping method crosses; `[MapDerivedType]` emits the type switch only where source and target hierarchies share a base type, so a `oneof` envelope routes case dispatch through the union's generated `Switch` and `PayloadCase` while Mapperly transcribes each case's fields. Get-only `RepeatedField<T>` and `MapField<TKey, TValue>` members fill through an existing-target `[UserMapping]` method.
- `NodaTime.Serialization.Protobuf`(`.api/api-nodatime-protobuf.md`): `[UseStaticMapper(typeof(NodaExtensions))]` and `[UseStaticMapper(typeof(ProtobufExtensions))]` register the whole `ToTimestamp`/`ToInstant`/`ToProtobufDuration`/`ToNodaDuration`/`ToDate`/`ToLocalDate` family as mapping methods, so a temporal member crosses with no per-member configuration.
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions.md`): a `[ValueObject<T>]` or `[SmartEnum<TKey>]` crosses through its generated key — `MappingConversionType.StaticConvertMethods` resolves the generated static `Create(TValue)` inbound and `ImplicitCast`/`ExplicitCast` takes the generated conversion operator outbound, with `[UseStaticMapper]` over a key codec registering a whole family at once.
- `UnitsNet`(`.api/api-unitsnet.md`): a quantity carries its unit and a wire scalar does not, so each quantity member binds a `[NamedMapping]` converter over `Length.As(LengthUnit)` outbound and `Length.FromMeters(QuantityValue)` inbound, reached by `[MapProperty(Use = …)]`; the unit is the converter's decision.
- `Generator.Equals`(`.api/api-generator-equals.md`): a mapped DTO or wire record carries `[Equatable]`, so the generated comparer decides identity over the shape Mapperly transcribed and neither generator reimplements the other.
- `LanguageExt`(`.api/api-languageext.md`): a generated mapping method returns the bare target and throws or defaults per `ThrowOnMappingNullMismatch`, so the seam lifts the call onto `Fin` at the call site and the rail stays outside the generated body.
- `Marten`(`Rasm.Persistence/.api/api-marten.md`): `IMartenQueryable<T>` is the LINQ provider a `ProjectTo` partial composes, so the projection translates to SQL and executes in-store instead of materializing the graph.
- `DuckDB`(`Rasm.Persistence/.api/api-duckdb.md`): the analytical lane reads flat rows through `DuckDBDataReader` and writes them through `DuckDBAppender` behind no LINQ provider, so a row DTO crosses through an ordinary `[Mapper]` partial rather than a projection.
- within-library: one `[Mapper]` partial owns a whole seam by composing per-concern projectors through `[UseMapper]` fields and `[UseStaticMapper]` types, so `[MapProperty(Use = …)]` resolves to a `[NamedMapping]` in any of them; `IncludeMappingConfigurationAttribute` then reuses one method's entire configuration on a sibling method.

[LOCAL_ADMISSION]:
- Mapperly owns boundary transcription alone — `ElementGraph`, `Node`, `Relationship`, and `PropertyValue` against protobuf DTOs, store row shapes, and import/export records. Identity and content hashing stay in the kernel, equality in `Generator.Equals`, the failure rail in LanguageExt.
- A mapper is a generated capsule whose declared partial shapes are its whole implementation; a domain decision inside a mapper body belongs to its owner.
- Boundary policy declares on the mapper attribute: `[FormatProvider(Default = true)]` carries the culture, `ThrowOnPropertyMappingNullMismatch` and `AllowNullPropertyAssignment` carry the null contract, `EnumNamingStrategy.SerializationEnumMemberAttribute` aligns enum strings with the wire schema.
- `EnabledConversions` narrows the automatic set where an implicit conversion masks a real mismatch — `All & ~ToStringMethod` keeps every conversion but the silent `ToString` fallback.

[RAIL_LAW]:
- Package: `Riok.Mapperly`
- Owns: compile-time transcription between a declared source shape and a declared target shape, generated from attributed partial declarations.
- Accept: boundary transcription between the canonical graph vocabulary and wire, DTO, and row shapes; `[MapDerivedType]` dispatch where source and target hierarchies share a base type; in-store `IQueryable` projection; existing-target update; `[ObjectFactory]` activation.
- Reject: a hand-rolled switch or assignment mapper over a type pair `[Mapper]` generates, a runtime or reflection mapper, a forwarding wrapper around a generated mapper, identity/hashing/equality/result-rail logic inside a mapper body, and `PreserveReferenceHandler` treated as a stable hand-authored surface.
