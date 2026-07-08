# [RASM_API_MAPPERLY]

`Riok.Mapperly` is a compile-time Roslyn source generator that materializes object-to-object mapping methods from attributed partial declarations: zero reflection, zero runtime allocation beyond the target instances, trimming- and AOT-safe generated code that reads like hand-written assignment. The only runtime asset is `Riok.Mapperly.Abstractions` (a netstandard2.0 attribute/enum surface marked `[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]`, so the attributes are erased from the final IL); the generator (`Riok.Mapperly.dll`) runs only inside the compiler. A mapper is a `partial class`/`struct` (instance) or `static partial class` (static/extension) carrying `[Mapper]`; each declared `partial` mapping method is filled in by the generator. Its value to the seam: Mapperly generates every per-case seam↔wire field transcription (the `Rasm.Element` `Graph/wire` `WireCodec` — flat columns generated, `[UserMapping]` carrier codecs owning the `Option`/`Seq`/`Map`/identity crossings), while `[MapDerivedType]` emits a real polymorphic type-switch for abstract-to-abstract CLASS-HIERARCHY pairs — a protobuf `oneof` envelope's case messages share no base type, so there the case dispatch rides the union's generated total `Switch` (encode) and the generated `PayloadCase` closed enum (decode), Mapperly owning the per-case field mapping the protobuf runtime does not; `IQueryable` projection mappings inline an EF/LINQ-translatable expression tree for the columnar read lane; `void`-returning existing-target methods update an instance in place; `[UseMapper]`/`[UseStaticMapper]` compose one seam mapper from per-concern projectors. This is the `ElementGraph`↔DTO/proto rail of the §4E integration map — Mapperly owns the boundary transcription, the kernel owns identity/hash, LanguageExt owns the result rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Riok.Mapperly`
- package: `Riok.Mapperly` (, Apache-2.0, © Riok / Mapperly contributors)
- assembly: `Riok.Mapperly.Abstractions` (runtime attributes/enums); `Riok.Mapperly` (Roslyn analyzer/generator, never referenced at runtime)
- namespace: `Riok.Mapperly.Abstractions`, `Riok.Mapperly.Abstractions.ReferenceHandling`
- asset: source generator + analyzer (`analyzers/dotnet/cs`, multi-Roslyn `roslyn4.0`…`roslyn5.0`) plus the `lib/netstandard2.0` abstractions assembly; `build/Riok.Mapperly.targets` wires the generator. Reference as an analyzer-style package (`PrivateAssets="all"`); the abstractions flow transitively to consumers that declare mappers.
- rail: mapping (boundary transcription)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: mapper declaration, defaults, and composition (class/assembly level)
- rail: mapping

| [INDEX] | [SYMBOL] | [TARGET] | [CAPABILITY] |
|:-----: |:----------------------------- |:------------------ |:-------------------------------------------------------------------------- |
| [01] | `MapperAttribute` | class | marks a `partial` class/struct as a mapper; carries the full per-mapper default policy (see below) |
| [02] | `MapperDefaultsAttribute` | assembly | `: MapperAttribute` — sets the same policy as assembly-wide defaults for every mapper |
| [03] | `UseMapperAttribute` | property / field | includes all accessible (static + instance) mapping methods of the member's type as delegated sub-mappers |
| [04] | `UseStaticMapperAttribute` | class (`AllowMultiple`) | `(Type)` ctor — includes all static mapping methods of the named type; generic `UseStaticMapperAttribute<T>` (C# ≥ 11) |
| [05] | `MapperConstructorAttribute` | constructor | selects the constructor Mapperly uses when activating the target type |
| [06] | `ObjectFactoryAttribute` | method | marks a (optionally generic, 0-or-1-param) non-void method as the target-instance factory — the seam hook for minting through `IObjectFactory` instead of `new()` |

[MapperAttribute] policy properties (each also settable on `MapperDefaultsAttribute`):

| [INDEX] | [PROPERTY] | [TYPE] | [DEFAULT] | [CAPABILITY] |
|:-----: |:---------------------------------- |:--------------------------- |:------------------------- |:------------------------------------------------------- |
| [01] | `PropertyNameMappingStrategy` | `PropertyNameMappingStrategy` | `CaseSensitive` | case sensitivity of member-name matching |
| [02] | `EnumMappingStrategy` | `EnumMappingStrategy` | `ByValue` | default enum→enum match mode |
| [03] | `EnumNamingStrategy` | `EnumNamingStrategy` | `MemberName` | default enum↔string naming |
| [04] | `EnumMappingIgnoreCase` | `bool` | `false` | ignore case on enum mappings |
| [05] | `ThrowOnMappingNullMismatch` | `bool` | `true` | throw `ArgumentNullException` when a non-nullable return does be null |
| [06] | `ThrowOnPropertyMappingNullMismatch` | `bool` | `false` | throw vs ignore when a non-nullable property does be null |
| [07] | `AllowNullPropertyAssignment` | `bool` | `true` | assign null to a nullable target vs never |
| [08] | `UseDeepCloning` | `bool` | `false` | deep-copy same-type members instead of reusing the reference |
| [09] | `EnabledConversions` | `MappingConversionType` | `All` | the implicit-conversion flag set Mapperly may apply |
| [10] | `UseReferenceHandling` | `bool` | `false` | thread an `IReferenceHandler` to preserve identity across cycles |
| [11] | `IgnoreObsoleteMembersStrategy` | `IgnoreObsoleteMembersStrategy` | `None` | how `[Obsolete]` members participate |
| [12] | `RequiredMappingStrategy` | `RequiredMappingStrategy` | `Both` | unmapped-member diagnostic strictness |
| [13] | `RequiredEnumMappingStrategy` | `RequiredMappingStrategy` | `Both` | unmapped-enum-member diagnostic strictness |
| [14] | `IncludedMembers` | `MemberVisibility` | `AllAccessible` | accessibility of members Mapperly maps |
| [15] | `IncludedConstructors` | `MemberVisibility` | `AllAccessible` | accessibility of constructors considered |
| [16] | `PreferParameterlessConstructors` | `bool` | `true` | prefer `new()` + init over the widest parameterized ctor |
| [17] | `AutoUserMappings` | `bool` | `true` | auto-discover signature-shaped methods vs require `[UserMapping]` |

[PUBLIC_TYPE_SCOPE]: per-mapping-method configuration (method level)
- rail: mapping

| [INDEX] | [SYMBOL] | [SHAPE] | [CAPABILITY] |
|:-----: |:---------------------------------- |:----------------------------------------- |:---------------------------------------------------------------------- |
| [01] | `MapPropertyAttribute` | `(string\| string[] source, string\| string[] target)`, `AllowMultiple` | rename / flatten / unflatten a member path (`.`-joined or `string[]`); `StringFormat`, `FormatProvider`, `Use` (named sub-mapping), `SuppressNullMismatchDiagnostic` |
| [02] | `MapPropertyFromSourceAttribute` | `(string\| string[] target)`, `AllowMultiple` | maps a target member from the whole source object; `StringFormat`/`FormatProvider`/`Use` |
| [03] | `MapNestedPropertiesAttribute` | `(string\| string[] source)`, `AllowMultiple` | flattens every member of a nested source path onto the target root; `SourceFullName` |
| [04] | `MapValueAttribute` | `(string\| string[] target, object? value)` / `(target)`+`Use`, `AllowMultiple` | assigns a constant (`Value`) or a parameterless-method-produced (`Use`) value to a target member |
| [05] | `MapperIgnoreSourceAttribute` | `(string source)`, `AllowMultiple` | excludes a source member from required-mapping diagnostics |
| [06] | `MapperIgnoreTargetAttribute` | `(string target)`, `AllowMultiple` | excludes a target member from mapping/diagnostics |
| [07] | `MapperRequiredMappingAttribute` | `(RequiredMappingStrategy)` | per-method override of the unmapped-member diagnostic strictness |
| [08] | `MapperIgnoreObsoleteMembersAttribute` | `(IgnoreObsoleteMembersStrategy = Both)` | per-method override of `[Obsolete]` handling |
| [09] | `MapDerivedTypeAttribute(Type sourceType, Type targetType)` | method, `AllowMultiple`; generic `MapDerivedTypeAttribute<TSource,TTarget>` (C# ≥ 11) | emits a polymorphic type-switch over the source object for an abstract/base-typed mapping method — the `[Union]`→wire-case rail |
| [10] | `IncludeMappingConfigurationAttribute` | `(string name)` | reuses the `[MapProperty]`/etc. configuration of another named mapping method |
| [11] | `NamedMappingAttribute` | `(string name)` | names a mapping so `Use = "..."`/`IncludeMappingConfiguration` can reference it |
| [12] | `UserMappingAttribute` | method; `Default`/`Ignore` | declares a hand-written method as a (default or ignorable) user mapping when `AutoUserMappings = false` |
| [13] | `MapEnumAttribute` | `(EnumMappingStrategy)`, `AllowMultiple` | per-method enum policy: `Strategy`, `IgnoreCase`, `FallbackValue`, `NamingStrategy` |
| [14] | `MapEnumValueAttribute` | `(object source, object target)`, `AllowMultiple` | maps one explicit enum member to another |
| [15] | `MapperIgnoreSourceValueAttribute` | `(object source)`, `AllowMultiple` | drops a source enum value (`SourceValue`) from the enum mapping |
| [16] | `MapperIgnoreTargetValueAttribute` | `(object target)`, `AllowMultiple` | drops a target enum value (`TargetValue`) from the enum mapping |

[PUBLIC_TYPE_SCOPE]: member/parameter markers
- rail: mapping

| [INDEX] | [SYMBOL] | [TARGET] | [CAPABILITY] |
|:-----: |:----------------------------- |:------------------ |:-------------------------------------------------------------------------- |
| [01] | `MappingTargetAttribute` | parameter | marks the parameter that is the (existing) mapping target for a `void`/update method |
| [02] | `MapperIgnoreAttribute` | method/property/field | excludes a member from mapping, or excludes a method from mapping-method discovery |
| [03] | `FormatProviderAttribute` | property/field | marks an `IFormatProvider`-typed member as a format provider; `Default` makes it the implicit provider for all `IFormattable` conversions |
| [04] | `ReferenceHandlerAttribute` | parameter | marks an `IReferenceHandler` parameter threaded through the mapping for cycle/identity preservation |

[PUBLIC_TYPE_SCOPE]: strategy and conversion enums
- rail: mapping

| [INDEX] | [SYMBOL] | [MEMBERS] |
|:-----: |:------------------------------ |:------------------------------------------------------------------------------------------------ |
| [01] | `EnumMappingStrategy` | `ByValue`, `ByName`, `ByValueCheckDefined` |
| [02] | `EnumNamingStrategy` | `MemberName`, `CamelCase`, `PascalCase`, `SnakeCase`, `UpperSnakeCase`, `KebabCase`, `UpperKebabCase`, `ComponentModelDescriptionAttribute`, `SerializationEnumMemberAttribute` |
| [03] | `PropertyNameMappingStrategy` | `CaseSensitive`, `CaseInsensitive` |
| [04] | `RequiredMappingStrategy` | `[Flags]` `None=0`, `Both=-1`, `Source=1`, `Target=2` |
| [05] | `IgnoreObsoleteMembersStrategy` | `[Flags]` `None=0`, `Both=-1`, `Source=1`, `Target=2` |
| [06] | `MemberVisibility` | `[Flags]` `AllAccessible=0x1F`, `All=0x1E`, `Accessible=1`, `Public=2`, `Internal=4`, `Protected=8`, `Private=0x10` (`All`/`Private` use `UnsafeAccessor` on net8+) |
| [07] | `MappingConversionType` | `[Flags]` `None=0`, `Constructor`, `ImplicitCast`, `ExplicitCast`, `ParseMethod`, `ToStringMethod`, `StringToEnum`, `EnumToString`, `EnumToEnum`, `DateTimeToDateOnly`, `DateTimeToTimeOnly`, `Queryable`, `Enumerable`, `Dictionary`, `Span`, `Memory`, `Tuple`, `EnumUnderlyingType`, `ToTargetMethod`, `StaticConvertMethods`, `All=-1` |

[PUBLIC_TYPE_SCOPE]: reference handling (`Riok.Mapperly.Abstractions.ReferenceHandling`)
- rail: mapping

| [INDEX] | [SYMBOL] | [SHAPE] | [CAPABILITY] |
|:-----: |:----------------------------- |:----------------- |:-------------------------------------------------------------------------- |
| [01] | `IReferenceHandler` | interface | `TryGetReference<TSource,TTarget>(TSource source, out TTarget? target)` + `SetReference<TSource,TTarget>(TSource source, TTarget target)` (both `where TSource:notnull where TTarget:notnull`) — resolve/store already-mapped targets to break cycles |
| [02] | `PreserveReferenceHandler` | sealed class | the built-in `IReferenceHandler` returning the same target for the same source identity (reference-equality keyed); generator-internal — not a hand-authored seam type |
| [03] | `ReferenceHandlerAttribute` | parameter marker | (re-listed) marks the `IReferenceHandler` parameter the mapper threads |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the generation contract — declare a `partial` method, the generator emits the body
- rail: mapping
- surface-root: a `[Mapper] partial class` (instance) or `[Mapper] static partial class` (static/extension)

| [INDEX] | [DECLARED_SIGNATURE] | [GENERATED_CAPABILITY] |
|:-----: |:--------------------------------------------------------------------------- |:----------------------------------------------------- |
| [01] | `partial TTarget Map(TSource source);` | new-instance mapping; ctor/init/property assignment |
| [02] | `static partial TTarget Map(this TSource source);` | static extension-method mapping (on `static partial class`) |
| [03] | `partial void Update(TSource source, [MappingTarget] TTarget target);` | mutate an existing target in place |
| [04] | `partial TTarget? Map(TSource? source);` | nullable-aware mapping (policy via `ThrowOnMappingNullMismatch`) |
| [05] | `static partial IQueryable<TTarget> ProjectTo(this IQueryable<TSource> q);` | EF/LINQ-translatable projection (object-initializer expression tree; `MappingConversionType.Queryable`) |
| [06] | `[MapDerivedType<A,ADto>][MapDerivedType<B,BDto>] partial BaseDto Map(Base source);` | polymorphic type-switch over the source runtime type |
| [07] | `partial void Map<TSource,TTarget>(TSource source, TTarget target);` / `partial TTarget Map<TSource,TTarget>(TSource source);` | generic dispatch routed to the concrete partial mappings declared in the same mapper |
| [08] | `partial TTarget Map(TSource source, [ReferenceHandler] IReferenceHandler refs);` | cycle-safe mapping threading a caller `IReferenceHandler` |
| [09] | `partial TTarget Map(TSource source, FooContext ctx);` | extra parameters flow as available values to `[MapValue(Use=...)]`/sub-mappings |

[ENTRYPOINT_SCOPE]: composition and instantiation
- rail: mapping

| [INDEX] | [SURFACE] | [CAPABILITY] |
|:-----: |:----------------------------------------------------------------- |:-------------------------------------------- |
| [01] | `[Mapper] partial class M {... }` → `new M()` | instance mapper; DI-friendly, holds `[UseMapper]` sub-mapper fields |
| [02] | `[Mapper] static partial class M {... }` | static mapper; methods are callable statically / as extensions |
| [03] | `[UseMapper] private readonly SubMapper _sub;` / `[UseStaticMapper(typeof(X))]` | delegate unmatched type-pairs to a referenced mapper |
| [04] | `[ObjectFactory] T Create<T>() / T Create<T>(TSource src)` | route target activation through a factory (the `IObjectFactory` mint seam) instead of `new()` |
| [05] | `[MapperDefaults]` on the assembly | one policy applied to every mapper in the assembly |
| [06] | `[UserMapping] private TTarget MapCore(TSource s) {... }` | a hand-written mapping the generator composes into other mappings |

## [04]-[IMPLEMENTATION_LAW]

[GENERATION_MODEL]:
- Mapperly is wholly compile-time. The mapper body the generator emits is ordinary C# — direct member assignment, `new`/init, `foreach` element loops, a `switch` over derived types — verifiable by reading the generated partial. There is no runtime mapping engine, no expression compilation (except the `IQueryable` projection, which is itself a static expression tree), no reflection, no per-map allocation beyond the produced targets. The result is trimming- and Native-AOT-safe by construction.
- The runtime footprint is `Riok.Mapperly.Abstractions` only, and every attribute is `[Conditional("MAPPERLY_ABSTRACTIONS_SCOPE_RUNTIME")]` — absent that symbol the attribute calls are erased, so the attributes carry zero IL weight in the shipped assembly. Reference the package analyzer-style (`PrivateAssets="all"`); a consumer that declares no mapper still inherits nothing.
- Unmapped members are a build-time diagnostic (`RMG` codes), not a silent drop: `RequiredMappingStrategy`/`MapperRequiredMapping` tune severity and `[MapperIgnoreSource]`/`[MapperIgnoreTarget]` waive a member explicitly. A mapping that does not round-trip is a compiler warning, surfaced where it is written.

[LOCAL_ADMISSION]:
- Mapperly owns boundary transcription only: `ElementGraph`/`Node`/`Relationship`/`PropertyValue` ↔ protobuf DTOs, DuckDB/Marten row shapes, and external import/export records. It never owns identity, content hashing, equality, or the result rail — `NodeId`/`ContentAddress` and `XxHash128` stay in the kernel, equality stays in `Generator.Equals`, and the failure rail stays in LanguageExt `Fin`/`Validation`.
- A mapper is a thin generated capsule, not a domain owner: it declares partial method shapes and configuration attributes, and the generated body is the whole implementation. Do not hand-roll a `switch`/assignment mapper where `[Mapper]` + `[MapDerivedType]` generates it; do not wrap the generated mapper in a forwarding adapter.
- `[Union]` and `[SmartEnum]` owners map through their generated key/case surface: a `[Union]` lowers via `[MapDerivedType]` (one attribute per case → a total type-switch) ONLY when the wire cases share a base type — a protobuf `oneof` envelope's case messages do not, so there the dispatch rides the union's generated `Switch` (encode) and the generated `PayloadCase` enum (decode) while Mapperly keeps the per-case field mapping (the `Rasm.Element` `Graph/wire#WIRE_CODEC` division); a `[ValueObject<T>]`/`[SmartEnum<TKey>]` maps via its key (`MappingConversionType.Constructor`/`ParseMethod`/`StaticConvertMethods`, or a `[UseStaticMapper]` over the Thinktecture key codec). Tighten `EnabledConversions` (e.g. `All & ~ToStringMethod`) when an implicit conversion does mask a real mismatch.
- Boundary policy (culture, null strictness, enum naming) is declared on the mapper attribute, never hidden in an interior helper: `[FormatProvider(Default = true)]` carries the culture; `ThrowOnPropertyMappingNullMismatch`/`AllowNullPropertyAssignment` carry the null contract; `EnumNamingStrategy.SerializationEnumMemberAttribute` aligns enum strings with the wire schema.

[STACKING]:
- `Generator.Equals` (`api-generator-equals.md`): the same DTO/wire records Mapperly produces carry `[Equatable]` structural equality, so a mapped target compares by content for the diff/dedup lane — Mapperly transcribes the shape, `Generator.Equals` decides identity, neither reimplements the other.
- `Google.Protobuf` (`api-protobuf.md`): the protobuf-generated message classes are the wire DTOs Mapperly maps to/from; a `oneof` envelope's case messages share no base type, so the `Node`/`Relationship`/`PropertyValue` case dispatch rides the union's generated total `Switch` (encode) and the generated `PayloadCase` closed enum (decode) while Mapperly owns the per-case field-by-field transcription the protobuf runtime does not (the `Rasm.Element` `Graph/wire#WIRE_CODEC` division); existing-target `[UserMapping]` update methods fill the get-only `RepeatedField`/`MapField` members.
- Thinktecture (`api-thinktecture-runtime-extensions.md`): a `[ValueObject<string>]` `NodeId` or `[SmartEnum]` `Discipline` maps through its generated key — `[UseStaticMapper]` over the Thinktecture factory/key accessor, or `MappingConversionType.Constructor`/`StaticConvertMethods` resolving `IObjectFactory.Create`. Mapperly never re-parses a value object by hand.
- LanguageExt (`api-languageext.md`): a mapper method returns the bare target; the seam wraps the call in `Fin`/`Validation` at the call site (`Try`/`Eff`), because Mapperly throws (or returns default) per its null policy rather than returning a typed result. Keep the rail outside the generated method.
- Marten / DuckDB (`api-marten.md`, `api-duckdb.md`): a `static partial IQueryable<TDto> ProjectTo(this IQueryable<T>)` projection inlines an EF/LINQ-translatable expression tree for the columnar read lane, so the projection executes in-store (SQL) rather than materializing the graph; pair it with `IReferenceHandler` only on the in-memory object path, never on the `IQueryable` path (reference handling and projection are mutually exclusive by design).

[RAIL_LAW]:
- Package: `Riok.Mapperly`
- Owns: compile-time object-to-object mapping — `[Mapper]` partial-method generation, member rename/flatten/unflatten, constant/computed value injection, enum mapping (by value/name, naming strategies, explicit value maps), polymorphic `[MapDerivedType]` type-switches, `IQueryable` projection, existing-target update, generic dispatch, mapper composition (`[UseMapper]`/`[UseStaticMapper]`), object-factory activation, and `IReferenceHandler` cycle preservation.
- Accept: boundary transcription between the canonical graph/value vocabulary and wire/DTO/row shapes; the `[Union]`→wire-case switch via `[MapDerivedType]` where the wire cases share a base type (a `oneof` envelope dispatches through the union's own generated `Switch`/`PayloadCase`); the in-store projection via a `ProjectTo` partial; activation through the `IObjectFactory` mint via `[ObjectFactory]`.
- Reject: hand-rolled `switch`/assignment mappers Mapperly does generate; a runtime/reflection mapper; a forwarding wrapper around a generated mapper; identity, hashing, equality, or result-rail logic smuggled into a mapper (those belong to the kernel, `Generator.Equals`, and LanguageExt); the generator-internal `PreserveReferenceHandler` API treated as a stable hand-authored surface.
