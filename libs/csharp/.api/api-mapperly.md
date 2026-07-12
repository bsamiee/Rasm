# [RASM_API_MAPPERLY]

`Riok.Mapperly` is a compile-time Roslyn source generator that materializes object-to-object mapping methods from attributed partial declarations: zero reflection, zero runtime allocation beyond the target instances, trimming- and AOT-safe generated code that reads like hand-written assignment. The only runtime asset is `Riok.Mapperly.Abstractions` (a netstandard2.0 attribute/enum surface marked `[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]`, so the attributes are erased from the final IL); the generator (`Riok.Mapperly.dll`) runs only inside the compiler. A mapper is an instance or static `partial class` carrying `[Mapper]`; each declared `partial` mapping method is filled in by the generator. Its value to the seam: Mapperly generates every per-case seam↔wire field transcription (the `Rasm.Element` `Graph/wire` `WireCodec` — flat columns generated, `[UserMapping]` carrier codecs owning the `Option`/`Seq`/`Map`/identity crossings), while `[MapDerivedType]` emits a real polymorphic type-switch for abstract-to-abstract CLASS-HIERARCHY pairs — a protobuf `oneof` envelope's case messages share no base type, so there the case dispatch rides the union's generated total `Switch` (encode) and the generated `PayloadCase` closed enum (decode), Mapperly owning the per-case field mapping the protobuf runtime does not; `IQueryable` projection mappings inline an EF/LINQ-translatable expression tree for the columnar read lane; `void`-returning existing-target methods update an instance in place; `[UseMapper]`/`[UseStaticMapper]` compose one seam mapper from per-concern projectors. This is the `ElementGraph`↔DTO/proto rail of the §4E integration map — Mapperly owns the boundary transcription, the kernel owns identity/hash, LanguageExt owns the result rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Riok.Mapperly`
- package: `Riok.Mapperly` (Apache-2.0, © Riok / Mapperly contributors)
- assembly: `Riok.Mapperly.Abstractions` (runtime attributes/enums); `Riok.Mapperly` (Roslyn analyzer/generator, never referenced at runtime)
- namespace: `Riok.Mapperly.Abstractions`, `Riok.Mapperly.Abstractions.ReferenceHandling`
- asset: source generator + analyzer (`analyzers/dotnet/cs`, multi-Roslyn `roslyn4.0`…`roslyn5.0`) plus the `lib/netstandard2.0` abstractions assembly; `build/Riok.Mapperly.targets` wires the generator. Reference as an analyzer-style package (`PrivateAssets="all"`); the abstractions flow transitively to consumers that declare mappers.
- rail: mapping (boundary transcription)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: mapper declaration, defaults, and composition (class/assembly level)
- rail: mapping

`MapperDefaultsAttribute` inherits the mapper policy. `UseMapperAttribute` delegates accessible static and instance mappings from an annotated field or property, while the repeatable `UseStaticMapperAttribute` delegates static mappings from its named type. `ObjectFactoryAttribute` admits generic non-void factories with zero or one parameter.

| [INDEX] | [SYMBOL]                      | [TARGET]    | [CAPABILITY]              |
| :-----: | :---------------------------- | :---------- | :------------------------ |
|  [01]   | `MapperAttribute`             | class       | mapper policy             |
|  [02]   | `MapperDefaultsAttribute`     | assembly    | assembly mapper policy    |
|  [03]   | `UseMapperAttribute`          | field       | member mapper delegation  |
|  [04]   | `UseMapperAttribute`          | property    | member mapper delegation  |
|  [05]   | `UseStaticMapperAttribute`    | class       | static mapper delegation  |
|  [06]   | `UseStaticMapperAttribute<T>` | class       | generic mapper delegation |
|  [07]   | `MapperConstructorAttribute`  | constructor | target constructor choice |
|  [08]   | `ObjectFactoryAttribute`      | method      | target factory choice     |

Every `MapperAttribute` policy property is also settable through `MapperDefaultsAttribute`. Null-return mismatch throws when enabled and otherwise attempts a default; null-property mismatch throws when enabled and otherwise suppresses assignment.

`AllowNullPropertyAssignment` governs nullable targets, `PreferParameterlessConstructors` selects constructor priority, and `AutoUserMappings` controls signature-based discovery.

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

[PUBLIC_TYPE_SCOPE]: per-mapping-method configuration (method level)
- rail: mapping

`MapPropertyAttribute` renames, flattens, or unflattens member paths. `MapPropertyFromSourceAttribute` maps the whole source, and `MapNestedPropertiesAttribute` flattens a nested source onto the target root. Paired constructor arguments are ordered source then target.

| [INDEX] | [SYMBOL]                                   | [INPUT]                                  | [MANY] | [CAPABILITY]                |
| :-----: | :----------------------------------------- | :--------------------------------------- | :----: | :-------------------------- |
|  [01]   | `MapPropertyAttribute`                     | `(string\|string[], string\|string[])`   |  yes   | member-path mapping         |
|  [02]   | `MapPropertyFromSourceAttribute`           | `(string\|string[])`                     |  yes   | whole-source mapping        |
|  [03]   | `MapNestedPropertiesAttribute`             | `(string\|string[])`                     |  yes   | nested-source flattening    |
|  [04]   | `MapValueAttribute`                        | `(string\|string[], object?)`            |  yes   | constant assignment         |
|  [05]   | `MapValueAttribute`                        | `(string\|string[])`                     |  yes   | mapping-produced assignment |
|  [06]   | `MapperIgnoreSourceAttribute`              | `(string)`                               |  yes   | source diagnostic exclusion |
|  [07]   | `MapperIgnoreTargetAttribute`              | `(string)`                               |  yes   | target mapping exclusion    |
|  [08]   | `MapperRequiredMappingAttribute`           | `(RequiredMappingStrategy)`              |   no   | diagnostic policy override  |
|  [09]   | `MapperIgnoreObsoleteMembersAttribute`     | `(IgnoreObsoleteMembersStrategy = Both)` |   no   | obsolete policy override    |
|  [10]   | `MapDerivedTypeAttribute`                  | `(Type, Type)`                           |  yes   | derived-type dispatch       |
|  [11]   | `MapDerivedTypeAttribute<TSource,TTarget>` | type arguments                           |  yes   | generic derived dispatch    |
|  [12]   | `IncludeMappingConfigurationAttribute`     | `(string)`                               |   no   | mapping configuration reuse |
|  [13]   | `NamedMappingAttribute`                    | `(string)`                               |   no   | mapping registration        |
|  [14]   | `UserMappingAttribute`                     | —                                        |   no   | user mapping registration   |
|  [15]   | `MapEnumAttribute`                         | `(EnumMappingStrategy)`                  |   no   | enum policy override        |
|  [16]   | `MapEnumValueAttribute`                    | `(object, object)`                       |  yes   | enum-member mapping         |
|  [17]   | `MapperIgnoreSourceValueAttribute`         | `(object)`                               |  yes   | source enum exclusion       |
|  [18]   | `MapperIgnoreTargetValueAttribute`         | `(object)`                               |  yes   | target enum exclusion       |

[ATTRIBUTE_MEMBER_SCOPE]: attribute policy members

Each row records one named policy member extracted from the method-level configuration surface.

| [INDEX] | [ATTRIBUTE]                        | [MEMBER]                         | [CAPABILITY]                |
| :-----: | :--------------------------------- | :------------------------------- | :-------------------------- |
|  [01]   | `MapPropertyAttribute`             | `StringFormat`                   | string format               |
|  [02]   | `MapPropertyAttribute`             | `FormatProvider`                 | format provider             |
|  [03]   | `MapPropertyAttribute`             | `Use`                            | named mapping               |
|  [04]   | `MapPropertyAttribute`             | `SuppressNullMismatchDiagnostic` | null-diagnostic suppression |
|  [05]   | `MapPropertyFromSourceAttribute`   | `StringFormat`                   | string format               |
|  [06]   | `MapPropertyFromSourceAttribute`   | `FormatProvider`                 | format provider             |
|  [07]   | `MapPropertyFromSourceAttribute`   | `Use`                            | named mapping               |
|  [08]   | `MapNestedPropertiesAttribute`     | `SourceFullName`                 | source path                 |
|  [09]   | `MapValueAttribute`                | `Value`                          | constant value              |
|  [10]   | `MapValueAttribute`                | `Use`                            | value mapping               |
|  [11]   | `UserMappingAttribute`             | `Default`                        | default selection           |
|  [12]   | `UserMappingAttribute`             | `Ignore`                         | discovery exclusion         |
|  [13]   | `MapEnumAttribute`                 | `Strategy`                       | match strategy              |
|  [14]   | `MapEnumAttribute`                 | `IgnoreCase`                     | name-match casing           |
|  [15]   | `MapEnumAttribute`                 | `FallbackValue`                  | unmatched fallback          |
|  [16]   | `MapEnumAttribute`                 | `NamingStrategy`                 | string naming               |
|  [17]   | `MapperIgnoreSourceValueAttribute` | `SourceValue`                    | source enum value           |
|  [18]   | `MapperIgnoreTargetValueAttribute` | `TargetValue`                    | target enum value           |

[PUBLIC_TYPE_SCOPE]: member/parameter markers
- rail: mapping

`FormatProviderAttribute.Default` selects the implicit `IFormattable` provider. Multi-target attributes retain one row per exact `AttributeUsage` target.

| [INDEX] | [SYMBOL]                    | [TARGET]  | [CAPABILITY]               |
| :-----: | :-------------------------- | :-------- | :------------------------- |
|  [01]   | `MappingTargetAttribute`    | parameter | existing-target binding    |
|  [02]   | `MapperIgnoreAttribute`     | method    | method-discovery exclusion |
|  [03]   | `MapperIgnoreAttribute`     | property  | member-mapping exclusion   |
|  [04]   | `MapperIgnoreAttribute`     | field     | member-mapping exclusion   |
|  [05]   | `FormatProviderAttribute`   | property  | format-provider binding    |
|  [06]   | `FormatProviderAttribute`   | field     | format-provider binding    |
|  [07]   | `ReferenceHandlerAttribute` | parameter | reference-handler binding  |

[PUBLIC_TYPE_SCOPE]: strategy and conversion enums
- rail: mapping

`RequiredMappingStrategy`, `IgnoreObsoleteMembersStrategy`, `MemberVisibility`, and `MappingConversionType` are flag vocabularies. `MemberVisibility.All` and `Private` reach non-public members through `UnsafeAccessor` where the target framework exposes it; explicit values remain part of the declaration.

| [INDEX] | [ENUM]                          | [DECLARATION]                        |
| :-----: | :------------------------------ | :----------------------------------- |
|  [01]   | `EnumMappingStrategy`           | `ByValue`                            |
|  [02]   | `EnumMappingStrategy`           | `ByName`                             |
|  [03]   | `EnumMappingStrategy`           | `ByValueCheckDefined`                |
|  [04]   | `EnumNamingStrategy`            | `MemberName`                         |
|  [05]   | `EnumNamingStrategy`            | `CamelCase`                          |
|  [06]   | `EnumNamingStrategy`            | `PascalCase`                         |
|  [07]   | `EnumNamingStrategy`            | `SnakeCase`                          |
|  [08]   | `EnumNamingStrategy`            | `UpperSnakeCase`                     |
|  [09]   | `EnumNamingStrategy`            | `KebabCase`                          |
|  [10]   | `EnumNamingStrategy`            | `UpperKebabCase`                     |
|  [11]   | `EnumNamingStrategy`            | `ComponentModelDescriptionAttribute` |
|  [12]   | `EnumNamingStrategy`            | `SerializationEnumMemberAttribute`   |
|  [13]   | `PropertyNameMappingStrategy`   | `CaseSensitive`                      |
|  [14]   | `PropertyNameMappingStrategy`   | `CaseInsensitive`                    |
|  [15]   | `RequiredMappingStrategy`       | `None = 0`                           |
|  [16]   | `RequiredMappingStrategy`       | `Both = -1`                          |
|  [17]   | `RequiredMappingStrategy`       | `Source = 1`                         |
|  [18]   | `RequiredMappingStrategy`       | `Target = 2`                         |
|  [19]   | `IgnoreObsoleteMembersStrategy` | `None = 0`                           |
|  [20]   | `IgnoreObsoleteMembersStrategy` | `Both = -1`                          |
|  [21]   | `IgnoreObsoleteMembersStrategy` | `Source = 1`                         |
|  [22]   | `IgnoreObsoleteMembersStrategy` | `Target = 2`                         |
|  [23]   | `MemberVisibility`              | `AllAccessible = 0x1F`               |
|  [24]   | `MemberVisibility`              | `All = 0x1E`                         |
|  [25]   | `MemberVisibility`              | `Accessible = 1`                     |
|  [26]   | `MemberVisibility`              | `Public = 2`                         |
|  [27]   | `MemberVisibility`              | `Internal = 4`                       |
|  [28]   | `MemberVisibility`              | `Protected = 8`                      |
|  [29]   | `MemberVisibility`              | `Private = 0x10`                     |
|  [30]   | `MappingConversionType`         | `None = 0`                           |
|  [31]   | `MappingConversionType`         | `Constructor`                        |
|  [32]   | `MappingConversionType`         | `ImplicitCast`                       |
|  [33]   | `MappingConversionType`         | `ExplicitCast`                       |
|  [34]   | `MappingConversionType`         | `ParseMethod`                        |
|  [35]   | `MappingConversionType`         | `ToStringMethod`                     |
|  [36]   | `MappingConversionType`         | `StringToEnum`                       |
|  [37]   | `MappingConversionType`         | `EnumToString`                       |
|  [38]   | `MappingConversionType`         | `EnumToEnum`                         |
|  [39]   | `MappingConversionType`         | `DateTimeToDateOnly`                 |
|  [40]   | `MappingConversionType`         | `DateTimeToTimeOnly`                 |
|  [41]   | `MappingConversionType`         | `Queryable`                          |
|  [42]   | `MappingConversionType`         | `Enumerable`                         |
|  [43]   | `MappingConversionType`         | `Dictionary`                         |
|  [44]   | `MappingConversionType`         | `Span`                               |
|  [45]   | `MappingConversionType`         | `Memory`                             |
|  [46]   | `MappingConversionType`         | `Tuple`                              |
|  [47]   | `MappingConversionType`         | `EnumUnderlyingType`                 |
|  [48]   | `MappingConversionType`         | `ToTargetMethod`                     |
|  [49]   | `MappingConversionType`         | `StaticConvertMethods`               |
|  [50]   | `MappingConversionType`         | `All = -1`                           |

[PUBLIC_TYPE_SCOPE]: reference handling (`Riok.Mapperly.Abstractions.ReferenceHandling`)
- rail: mapping

`IReferenceHandler` resolves targets through `TryGetReference<TSource,TTarget>(TSource source, out TTarget? target)` and records them through `SetReference<TSource,TTarget>(TSource source, TTarget target)`; both methods constrain source and target to `notnull`. `PreserveReferenceHandler` is generator-owned and keys source objects by reference identity.

| [INDEX] | [SYMBOL]                   | [SHAPE]      | [CAPABILITY]             |
| :-----: | :------------------------- | :----------- | :----------------------- |
|  [01]   | `IReferenceHandler`        | interface    | cycle identity registry  |
|  [02]   | `PreserveReferenceHandler` | sealed class | generator-owned registry |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the generation contract — declare a `partial` method, the generator emits the body
- rail: mapping
- surface-root: a `[Mapper] partial class` (instance) or `[Mapper] static partial class` (static/extension)

Each row is one generated method shape. Repeatable `MapDerivedTypeAttribute` instances compose the runtime type switch; additional parameters map to same-name target members after explicit `MapPropertyAttribute` configuration and before source-member matching.

| [INDEX] | [SIGNATURE]                                                                          | [CAPABILITY]             |
| :-----: | :----------------------------------------------------------------------------------- | :----------------------- |
|  [01]   | `partial TTarget Map(TSource source);`                                               | target construction      |
|  [02]   | `static partial TTarget Map(this TSource source);`                                   | extension mapping        |
|  [03]   | `partial void Update(TSource source, [MappingTarget] TTarget target);`               | existing-target update   |
|  [04]   | `partial TTarget? Map(TSource? source);`                                             | nullable mapping         |
|  [05]   | `static partial IQueryable<TTarget> ProjectTo(this IQueryable<TSource> source);`     | queryable projection     |
|  [06]   | `[MapDerivedType<Variant,RefinedVariant>] partial RefinedShape Map(Shape source);`   | derived-type dispatch    |
|  [07]   | `partial void Map<TSource,TTarget>(TSource source, TTarget target);`                 | generic target update    |
|  [08]   | `partial TTarget Map<TTarget>(Shape source);`                                        | generic target mapping   |
|  [09]   | `partial TTarget Map(TSource source, [ReferenceHandler] IReferenceHandler handler);` | cycle-safe mapping       |
|  [10]   | `partial TTarget Map(TSource source, TContext context);`                             | additional value mapping |

[ENTRYPOINT_SCOPE]: composition and instantiation
- rail: mapping

Each row shows one mapper composition or activation surface. Object-factory type parameters may represent the source, target, or both; member-target alternatives remain owned by the public-type registry.

| [INDEX] | [SURFACE]                                                         | [CAPABILITY]               |
| :-----: | :---------------------------------------------------------------- | :------------------------- |
|  [01]   | `[Mapper] partial class Shape`                                    | instance mapper            |
|  [02]   | `[Mapper] static partial class Shape`                             | static mapper              |
|  [03]   | `[UseMapper] private readonly Mapper _mapper;`                    | instance mapper delegation |
|  [04]   | `[UseStaticMapper(typeof(Mapper))]`                               | static mapper delegation   |
|  [05]   | `[ObjectFactory] RefinedShape Create()`                           | parameterless activation   |
|  [06]   | `[ObjectFactory] RefinedShape Create(Shape source)`               | source-based activation    |
|  [07]   | `[ObjectFactory] RefinedShape Create<TSource>(TSource source)`    | generic-source activation  |
|  [08]   | `[ObjectFactory] TTarget Create<TTarget>()`                       | generic-target activation  |
|  [09]   | `[ObjectFactory] TTarget Create<TTarget>(Shape source)`           | source-based target        |
|  [10]   | `[ObjectFactory] TTarget Create<TSource,TTarget>(TSource source)` | source-target activation   |
|  [11]   | `[ObjectFactory] TTarget Create<TTarget,TSource>(TSource source)` | target-source activation   |
|  [12]   | `[assembly: MapperDefaults]`                                      | assembly mapper policy     |
|  [13]   | `[UserMapping] private TTarget MapCore(TSource source)`           | user mapping composition   |

## [04]-[IMPLEMENTATION_LAW]

[GENERATION_MODEL]:
- Mapperly emits ordinary C# bodies at compile time: direct assignments, construction and initialization, element loops, and derived-type switches remain inspectable in the generated partial. `IQueryable` projection emits a static expression tree; no mapping engine, runtime expression compilation, or reflection remains, and target construction owns per-map allocation.
- The runtime footprint is `Riok.Mapperly.Abstractions` only, and every attribute is `[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]` — absent that symbol the attribute calls are erased, so the attributes carry zero IL weight in the shipped assembly. Reference the package analyzer-style (`PrivateAssets="all"`); a consumer that declares no mapper still inherits nothing.
- Unmapped members are a build-time diagnostic (`RMG` codes), not a silent drop: `RequiredMappingStrategy`/`MapperRequiredMapping` tune severity and `[MapperIgnoreSource]`/`[MapperIgnoreTarget]` waive a member explicitly. A mapping that does not round-trip is a compiler warning, surfaced where it is written.

[LOCAL_ADMISSION]:
- Mapperly owns boundary transcription only: `ElementGraph`/`Node`/`Relationship`/`PropertyValue` ↔ protobuf DTOs, DuckDB/Marten row shapes, and external import/export records. It never owns identity, content hashing, equality, or the result rail — `NodeId`/`ContentAddress` and `XxHash128` stay in the kernel, equality stays in `Generator.Equals`, and the failure rail stays in LanguageExt `Fin`/`Validation`.
- A mapper is a thin generated capsule, not a domain owner: it declares partial method shapes and configuration attributes, and the generated body is the whole implementation. Do not hand-roll a `switch`/assignment mapper where `[Mapper]` + `[MapDerivedType]` generates it; do not wrap the generated mapper in a forwarding adapter.
- `[Union]` and `[SmartEnum]` owners map through their generated key/case surface: a `[Union]` lowers via `[MapDerivedType]` (one attribute per case → a total type-switch) ONLY when the wire cases share a base type — a protobuf `oneof` envelope's case messages do not, so there the dispatch rides the union's generated `Switch` (encode) and the generated `PayloadCase` enum (decode) while Mapperly keeps the per-case field mapping; a `[ValueObject<T>]`/`[SmartEnum<TKey>]` maps via its key (`MappingConversionType.Constructor`/`ParseMethod`/`StaticConvertMethods`, or a `[UseStaticMapper]` over the Thinktecture key codec). Tighten `EnabledConversions` (e.g. `All & ~ToStringMethod`) when an implicit conversion does mask a real mismatch.
- Boundary policy (culture, null strictness, enum naming) is declared on the mapper attribute, never hidden in an interior helper: `[FormatProvider(Default = true)]` carries the culture; `ThrowOnPropertyMappingNullMismatch`/`AllowNullPropertyAssignment` carry the null contract; `EnumNamingStrategy.SerializationEnumMemberAttribute` aligns enum strings with the wire schema.

[STACKING]:
- `Generator.Equals` (`api-generator-equals.md`): the same DTO/wire records Mapperly produces carry `[Equatable]` structural equality, so a mapped target compares by content for the diff/dedup lane — Mapperly transcribes the shape, `Generator.Equals` decides identity, neither reimplements the other.
- `Google.Protobuf` (`api-protobuf.md`): the protobuf-generated message classes are the wire DTOs Mapperly maps to/from; a `oneof` envelope's case messages share no base type, so the `Node`/`Relationship`/`PropertyValue` case dispatch rides the union's generated total `Switch` (encode) and the generated `PayloadCase` closed enum (decode) while Mapperly owns the per-case field-by-field transcription the protobuf runtime does not; existing-target `[UserMapping]` update methods fill the get-only `RepeatedField`/`MapField` members.
- Thinktecture (`api-thinktecture-runtime-extensions.md`): a `[ValueObject<string>]` `NodeId` or `[SmartEnum]` `Discipline` maps through its generated key — `[UseStaticMapper]` over the Thinktecture factory/key accessor, or `MappingConversionType.Constructor`/`StaticConvertMethods` resolving `IObjectFactory.Create`. Mapperly never re-parses a value object by hand.
- LanguageExt (`api-languageext.md`): a mapper method returns the bare target; the seam wraps the call in `Fin`/`Validation` at the call site (`Try`/`Eff`), because Mapperly throws (or returns default) per its null policy rather than returning a typed result. Keep the rail outside the generated method.
- Marten / DuckDB (`api-marten.md`, `api-duckdb.md`): a `static partial IQueryable<TDto> ProjectTo(this IQueryable<T>)` projection inlines an EF/LINQ-translatable expression tree for the columnar read lane, so the projection executes in-store (SQL) rather than materializing the graph; pair it with `IReferenceHandler` only on the in-memory object path, never on the `IQueryable` path (reference handling and projection are mutually exclusive by design).

[RAIL_LAW]:
- Package: `Riok.Mapperly`
- Owns: compile-time object-to-object mapping — `[Mapper]` partial-method generation, member rename/flatten/unflatten, constant/computed value injection, enum mapping (by value/name, naming strategies, explicit value maps), polymorphic `[MapDerivedType]` type-switches, `IQueryable` projection, existing-target update, generic dispatch, mapper composition (`[UseMapper]`/`[UseStaticMapper]`), object-factory activation, and `IReferenceHandler` cycle preservation.
- Accept: boundary transcription between the canonical graph/value vocabulary and wire/DTO/row shapes; the `[Union]`→wire-case switch via `[MapDerivedType]` where the wire cases share a base type (a `oneof` envelope dispatches through the union's own generated `Switch`/`PayloadCase`); the in-store projection via a `ProjectTo` partial; activation through the `IObjectFactory` mint via `[ObjectFactory]`.
- Reject: hand-rolled `switch`/assignment mappers Mapperly does generate; a runtime/reflection mapper; a forwarding wrapper around a generated mapper; identity, hashing, equality, or result-rail logic smuggled into a mapper (those belong to the kernel, `Generator.Equals`, and LanguageExt); the generator-internal `PreserveReferenceHandler` API treated as a stable hand-authored surface.
