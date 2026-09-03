---
name: dotnet-mapperly
description: "Use when mapping between domain types and transport, persistence, or read-model contracts with Mapperly: mapper placement, the conversion allowlist, attributes, options, and projections."
---

# [DOTNET_MAPPERLY]

Covers mapping at the host boundary with `Riok.Mapperly`: where the mapper sits, its configuration levels and conversion allowlist, how a target is constructed and owned, how generated domain types and LanguageExt contexts pass through it, query projections, the mapping method shapes, every attribute and option, the enum and strategy types, conversion priority, and reference handling. Where the boundary sits and which result type crosses it are decisions that `dotnet-coding` states, conversions between result types belong to `dotnet-languageext`, and declaring a value object, smart enum, or union belongs to `dotnet-thinktecture`.

Mapperly generates each mapping at build time as ordinary property assignments, with no reflection, expression compilation, or hidden allocation, and an unmapped member fails the build. It serves performance-critical paths, transport and persistence contracts, read models, message payloads, view models, and AOT compilation, and `EmitCompilerGeneratedFiles` writes the generated mappings under `obj/` as C# source. Mapperly cannot consume another source generator's output from the same compilation, a referenced assembly exposes its generated members as metadata, an automatic conversion can change when a generated type moves between projects, and an explicit mapping declaration keeps project layout from choosing conversions.

## [01]-[BOUNDARIES]

The adapter that references both representations owns the mapper, and the domain references neither Mapperly nor an external contract:
- Define one mapper per aggregate or feature area, and keep it internal unless mapping is an intentional public contract
- Declare a static partial mapper for a transformation with no stored inputs, and keep an instance mapper deterministic through immutable and pure collaborators
- Keep mutable state, service location, and ambient reads outside every mapper

| [INDEX] | [DIRECTION]                                       | [MAPPERLY_ROLE]             | [REQUIRED_FORM]                                       |
| :-----: | :------------------------------------------------ | :-------------------------- | :---------------------------------------------------- |
|  [01]   | External contract to raw input model              | Structural mapping          | Validate the raw model before domain construction     |
|  [02]   | External contract to constrained domain value     | None                        | Call the hand-written `From` factory, keep its error  |
|  [03]   | Validated components to domain aggregate          | Optional construction       | Use one constructor total for those components        |
|  [04]   | Domain value to transport or persistence          | Structural projection       | Map only after the domain result is successful        |
|  [05]   | Domain snapshot to next domain snapshot           | None                        | Call a named transition returning the next value      |
|  [06]   | Closed union to external case                     | Structural mapping per case | Dispatch through the union's exhaustive `Switch`      |
|  [07]   | Mutable boundary value to a caller-created target | Existing-target mapping     | Mutate a target that never escapes its creating scope |
|  [08]   | Persistence query to read model                   | Expression projection       | Materialize before domain construction or effects     |

A mapping that can reject input is not a plain `TSource -> TTarget` function, validation owns the rejection and returns the typed `Expected` record its package declares, and the mapper maps the successful value inside its existing context, a transformer stack at its innermost value:

```csharp
internal static Fin<ItemDto> ToDto(Fin<Item> value) => value.Map(ItemMapper.ToDto);
```

Mapperly never selects or rebuilds the cases of `Option`, `Either`, `Validation`, `Try`, `IO`, `Eff`, a transformer, or a higher-kinded context.
- See `dotnet-coding/references/results.md` for combining independent `From` results with the tuple `Apply` before the aggregate is constructed

## [02]-[MAPPER_CONFIGURATION]

Configure every project from one `PropertyGroup` in `Directory.Build.props`, naming only what differs from the defaults. `Riok.Mapperly.targets` declares one compiler-visible property per option with the option name prefixed by `Mapperly`, global analyzer configuration sets the same options through a `build_property.Mapperly<Option>` key without an MSBuild property, and a project that does not reference the package ignores the group:

```xml
<PropertyGroup Label="Mapperly">
    <MapperlyEnumMappingStrategy>ByName</MapperlyEnumMappingStrategy>
    <MapperlyAutoUserMappings>false</MapperlyAutoUserMappings>
    <MapperlyThrowOnPropertyMappingNullMismatch>true</MapperlyThrowOnPropertyMappingNullMismatch>
    <MapperlyEnabledConversions>Queryable, Enumerable, Dictionary, Span, Memory, EnumToEnum</MapperlyEnabledConversions>
</PropertyGroup>
```

The allowlist omits every conversion that parses, formats, casts, or constructs, direct assignment and object-member mapping carry no bit and stay available, and every disabled conversion falls through to object-member mapping. A `string`, enum, or primitive target has no mappable members and reports `RMG007` as a member and `RMG008` as a mapping method, and a composite target maps its members and reports `RMG013`, `RMG066`, or nothing. `MappingConversionType.None` is not the stricter setting, because it clears `Enumerable` and `Dictionary`, and `List` and `Dictionary` members with a differing element type then map to empty collections with no diagnostic. The workspace compiles warnings as errors, RMG codes fail the build, and the silent fallthroughs are the cases an explicit mapping must cover.

The MSBuild property, `[assembly: MapperDefaults]`, `[Mapper]`, and the per-method attributes (`MapperRequiredMappingAttribute`, `MapperIgnoreObsoleteMembersAttribute`, `MapEnumAttribute`) configure a mapper, each overriding the one before it. `MapperDefaultsAttribute` derives from `MapperAttribute` with the same options, an override replaces the whole value, and a deviating mapper names its full `EnabledConversions` allowlist:
- Inbound mappers that feed constrained domain types never allow `ParseMethod`, `Constructor`, `StaticConvertMethods`, or a cast in place of validation
- Outbound mappers enable `ImplicitCast` for the generated value-object-to-key operator, and the inbound operator is explicit and calls the throwing `Create`
- Named mappings selected with `Use` own every remaining conversion
- Mappers that add `ToStringMethod` treat it as formatting, not a wire contract, and pass a fixed provider or an explicit culture input
- Automatic member matching applies only when names and meanings agree, one exact pair has one intentional default, alternatives have unique names, generic mapping methods are disjoint, and no selection depends on declaration order

External mappings stay local to the mapper that consumes them, and assembly-wide registrations expose only disjoint pairs. Configuration inclusion copies configuration rather than implementation and needs identical direction, member meaning, null policy, and omissions. Additional parameters hold immutable values that the boundary already resolved and forward to nested user mappings and `Use` methods where remaining parameter names match. A type pair shared between mappers belongs in one `internal static class` reached through `[UseStaticMapper<T>]`, and a private `[UserMapping]` stays for a mapper-local pair.

## [03]-[CONSTRUCTION_AND_OWNERSHIP]

The mapping form decides who owns the target while its members are written:

| [INDEX] | [FORM]                  | [TARGET_STATE]                            | [CONSTRAINT]                                                    |
| :-----: | :---------------------- | :---------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | New instance            | Mapper-owned until return                 | Constructor and init values precede writable assignments        |
|  [02]   | Mapperly object factory | Factory result, then writable assignments | Constructor parameters and init-only members are not mapped     |
|  [03]   | Existing target         | Caller-owned mutable value                | Assignments and collection additions are observable mutation    |
|  [04]   | Reference handling      | Identity graph under one handler          | Registration follows construction and precedes writable members |
|  [05]   | Runtime dispatch        | Registered source and target pairs        | Unregistered pair or subtype throws                             |

Prefer one total constructor for an immutable external record. `[MapperConstructor]` chooses between equivalent external constructors and does not select a domain transition, a type with invariants that depend on assignments after construction is not a valid mapping target, and Mapperly cannot skip an init-only assignment to preserve its member initializer.

Mapperly object factories allocate or select the target and expose no typed failure channel: the factory is pure, deterministic, and synchronous, returns a non-void type, takes zero parameters or one, can be generic with or without constraints, and the first factory with a matching signature wins. A nullable result falls back to a public parameterless construction or throws `NullReferenceException`, and a factory never resolves services, allocates domain identity, or calls a rejecting domain factory. The Mapperly object factory is unrelated to the Thinktecture `[ObjectFactory<T>]`, which declares a validating conversion to and from one other type.

Member and constructor visibility stays at `AllAccessible`, and an unsafe accessor that calls a private constructor or writes a hidden member never constructs or modifies a constrained domain type. Direct assignment can return the source reference, and sharing is valid only when the complete reachable graph is immutable, because deep cloning is an allocation strategy, not proof of ownership, validity, or a completed domain transition.

Existing-target mappings mutate their target, and an existing collection adds without replacement: lists add, queues enqueue, stacks push the added segment in reverse source order, and a null source collection leaves the target unchanged. Null-skipping implements merge behavior, not patch semantics, because it cannot distinguish an omitted member from one cleared to null, and an explicit optional wrapper folds against `[MappingTargetOriginalValue]` at a mutable boundary, where a constructor or init-only target receives `default` as the original value.

Reference handling materializes an external graph that requires cycles or shared identity, and one handler serves one mapping call. Constructor and init-only edges run before registration and cannot close a generated cycle, an existing-target root starts unregistered, the pair is registered before mapping when a back-reference must retain the supplied root, and domain identity uses explicit identifiers rather than mapper reference state.

## [04]-[DOMAIN_TYPE_INTEGRATION]

A generated domain type crosses the mapper only through its declared conversions, inbound through the `From` factory and outbound through the key member or the `ToValue` of a declared `[ObjectFactory<T>]`, and `ToString()` does not define that representation. `Create`, `Parse`, an accessible constructor, a static conversion method, and an explicit operator turn expected rejection into an exception. Mapperly enum configuration applies only to CLR enums, and independent CLR enum contracts map by case-sensitive name or explicit value pairs, never by numeric position.
- See `dotnet-coding` for the `From` factory that maps `Validate` to `Fin<T>`, and `dotnet-thinktecture` for the lookup that maps `TryGet` to `Option<T>`

A closed union uses its generated `Switch` as the outer dispatcher, Mapperly maps one known case inside each arm, and case selection stays exhaustive while member translation stays structural. `Map` takes one value per case and receives no mapper call:

```csharp
internal static ChangeDto ToDto(Change value) =>
    value.Switch(added: ChangeMapper.ToDto, removed: ChangeMapper.ToDto);
```

`MapDerivedType`, a duplicated case list, `SwitchPartially`, `MapPartially`, a `@default` arm, and a `StopAt` overload do not preserve closed-union exhaustiveness, and generic, runtime-target, and derived dispatch belong only to a runtime-registered type set where a mismatch is a defect.

LanguageExt owns absence, failure, validation, effects, traversal, and transformer stacks, and a Mapperly method supplies the function passed to `Map`, `BiMap`, `Apply`, or a traversal, total over a validated source. The throw from `ThrowOnPropertyMappingNullMismatch` signals a defect, not an expected error. Automatic wrapper construction is unsafe, because constructor or cast discovery can manufacture a success case, unwrap a failure, or discard source elements, and a generic wrapper helper needs explicit `Use` selection and preserves every case.

LanguageExt collections keep their own construction policy: direct assignment shares an immutable collection only when sharing is intentional, an element-changing sequence uses the collection's `Map`, a mutable source is snapshotted before domain publication, an arbitrary enumerable materializes first, and a map is built only after key validation defines ordering, uniqueness, and collision behavior, which incidental enumerable-tuple construction does not define.

## [05]-[QUERY_PROJECTIONS]

Query projections are expression trees that a query provider interprets, they belong in the data adapter, return a read model or transport contract, and compose into the query as `query.ProjectToDto().ToListAsync()`. Keep a projection declaration separate from an in-memory mapping when their conversion, null, or user-method policies differ. The projection method takes member configuration from an element mapping with the same source and target pair, and a projection with additional parameters reads configuration from an element mapping where the parameters match by name.

Mapperly must inline each user method, and the query provider must translate the resulting expression: inlining needs an expression body, one return statement, or one local declaration followed by a return, any other shape reports `RMG068` and leaves the call in the expression. Additional parameters are immutable scalar query values, and services, mapper state, clocks, configuration objects, and request contexts do not enter the expression.

Nullable analysis and the property-null options do not apply inside a projection, a nullable path can become empty text, `default`, or a conditional fallback, and the read model matches storage nullability. Object factories, existing-target mapping, dictionary mapping, deep cloning, and reference handling do not apply, reference handling reports `RMG029`, and unsupported enum configuration reports `RMG032` and emits a value cast. Project stored values, materialize the query, then validate and construct domain values, and the `Fin`-returning `From` factories, LanguageExt composition, and effects run after materialization. An unmatched derived projection returns `default(TTarget)`, valid only when the target contract states that result, and `AsEnumerable` does not hide the boundary, because a narrow query materializes before the in-memory pipeline begins.

## [06]-[MAPPING_METHODS]

Attributes on a `[Mapper] partial class` or `[Mapper] static partial class` and its partial methods carry the configuration, and a non-partial method with the matching types implements a member mapping by hand. Under `AutoUserMappings = false`, a hand-written mapping needs `[UserMapping]` for its type pair, and Mapperly then uses it in place of an automatic conversion, where `Default` marks the pair's one default mapping and `Ignore` excludes a discovered method. One user mapping carries the `ToValue` of a complex value object that declares `[ObjectFactory<string>]` outward, and another formats with a text pattern:

```csharp
[Mapper]
internal static partial class ItemMapper {
    public static partial ItemDto ToDto(Item item);
    public static partial IEnumerable<ItemDto> ToDtos(IEnumerable<Item> items);

    [UserMapping]
    private static string MapRange(Interval range) => range.ToValue();

    [UserMapping]
    private static string MapListed(Instant listed) => InstantPattern.ExtendedIso.Format(listed);
}

internal sealed record Item(Guid Id, Interval Range, decimal Amount, Instant ListedAt, Seq<Line> Lines);
internal sealed record ItemDto(Guid Id, string Range, decimal Amount, string ListedAt, IReadOnlyList<LineDto> Lines);
```

`ItemDto.Lines` is a BCL collection because the DTO is the host's contract, and the enabled `Enumerable` conversion maps each element of the `Seq<Line>` source through the `Line` to `LineDto` mapping into an array, `IReadOnlyList<T>`, `IEnumerable<T>`, `ICollection<T>`, `IList<T>`, `Queue<T>`, or `HashSet<T>` target. A `List<T>` target from a source that implements `IReadOnlyCollection<A>` outside the BCL (`Seq<A>`, `Lst<A>`, `Arr<A>`) with an element mapping generates the loop and still reports `RMG020` for every source member.

| [INDEX] | [DECLARATION]                                                              | [CAPABILITY]                     |
| :-----: | :------------------------------------------------------------------------- | :------------------------------- |
|  [01]   | `partial TTarget Map(TSource source)`                                      | New target instance              |
|  [02]   | `static partial TTarget ToDto(this TSource source)`                        | Extension mapping                |
|  [03]   | `static partial TTarget? Map(TSource? source)`                             | Nullable source and target       |
|  [04]   | `partial void Update(TSource source, TTarget target)`                      | Existing target, second holds it |
|  [05]   | `partial void Update([MappingTarget] TTarget t, TSource s)`                | Existing target, first holds it  |
|  [06]   | `static partial IQueryable<TTarget> ProjectTo(this IQueryable<TSource> q)` | Queryable projection             |
|  [07]   | `partial TTarget Map<TTarget>(TSource source)`                             | Target type resolved by caller   |
|  [08]   | `partial TTarget Map<TSource, TTarget>(TSource source)`                    | Source and target both generic   |
|  [09]   | `partial object Map(object source, Type targetType)`                       | Target type known at run time    |
|  [10]   | `partial TTarget Map(TSource s, [ReferenceHandler] IReferenceHandler h)`   | Caller supplies the handler      |
|  [11]   | `partial TTarget Map(TSource source, int extra)`                           | Additional mapping parameter     |
|  [12]   | `[MapDerivedType<TA, TB>] partial TBase Map(TSourceBase source)`           | Derived type mapping             |
|  [13]   | `static partial Expression<Func<TSource, TTarget>> Project()`              | Projection expression            |

- A non-static mapper that declares any static mapping method declares every mapping method static
- Static mapping methods satisfy a `static abstract` interface member, and a `[Mapper] partial class` can implement a mapping interface
- Existing-target methods return `void`, the second parameter is the target unless `MappingTargetAttribute` names another, and the attribute accepts the `this` parameter of an extension method
- Generic and runtime-target methods dispatch to the mappings declared in the same mapper, throw `ArgumentException` for an unknown pair at run time, accept no additional parameter, and accept `MapDerivedTypeAttribute` on the method itself

## [07]-[ATTRIBUTES]

Every attribute Mapperly reads, with its declaration target and whether it repeats:

| [INDEX] | [SYMBOL]                                   | [TARGET]                | [ALLOW_MULTIPLE] | [CAPABILITY]                         |
| :-----: | :----------------------------------------- | :---------------------- | :--------------: | :----------------------------------- |
|  [01]   | `MapperAttribute`                          | class                   |        No        | Mapper declaration and options       |
|  [02]   | `MapperDefaultsAttribute`                  | assembly                |        No        | Same options for every mapper        |
|  [03]   | `UseMapperAttribute`                       | field, property         |        No        | Use the member's mapping methods     |
|  [04]   | `UseStaticMapperAttribute`                 | class, assembly         |       Yes        | Use a type's static mapping methods  |
|  [05]   | `UseStaticMapperAttribute<T>`              | class, assembly         |       Yes        | Same, generic form                   |
|  [06]   | `MapperConstructorAttribute`               | constructor             |        No        | Select the constructor to call       |
|  [07]   | `ObjectFactoryAttribute`                   | method                  |        No        | Construct or resolve the target      |
|  [08]   | `MapperIgnoreAttribute`                    | property, field, method |        No        | Exclude a member or a method         |
|  [09]   | `UserMappingAttribute`                     | method                  |        No        | User-implemented mapping method      |
|  [10]   | `NamedMappingAttribute`                    | method                  |        No        | Name a mapping for `Use`             |
|  [11]   | `IncludeMappingConfigurationAttribute`     | method                  |       Yes        | Reuse another method's configuration |
|  [12]   | `MappingTargetAttribute`                   | parameter               |        No        | Mark the parameter as the target     |
|  [13]   | `MappingTargetOriginalValueAttribute`      | parameter               |        No        | Pass the target member's prior value |
|  [14]   | `FormatProviderAttribute`                  | field, property         |        No        | Expose an `IFormatProvider`          |
|  [15]   | `ReferenceHandlerAttribute`                | parameter               |        No        | Mark the reference-handler parameter |
|  [16]   | `MapPropertyAttribute`                     | method                  |       Yes        | Rename, flatten, and unflatten       |
|  [17]   | `MapPropertyFromSourceAttribute`           | method                  |       Yes        | Map the source object to a member    |
|  [18]   | `MapNestedPropertiesAttribute`             | method                  |       Yes        | Flatten every member of a path       |
|  [19]   | `MapValueAttribute`                        | method                  |       Yes        | Assign a constant or generated value |
|  [20]   | `MapperIgnoreSourceAttribute`              | method                  |       Yes        | Exclude a source member              |
|  [21]   | `MapperIgnoreTargetAttribute`              | method                  |       Yes        | Exclude a target member              |
|  [22]   | `MapperRequiredMappingAttribute`           | method                  |        No        | Unmapped-member diagnostics          |
|  [23]   | `MapperIgnoreObsoleteMembersAttribute`     | method                  |        No        | Obsolete-member policy               |
|  [24]   | `MapDerivedTypeAttribute`                  | method                  |       Yes        | One derived source and target pair   |
|  [25]   | `MapDerivedTypeAttribute<TSource,TTarget>` | method                  |       Yes        | Same, generic form                   |
|  [26]   | `MapEnumAttribute`                         | method                  |        No        | Enum strategy for one mapping        |
|  [27]   | `MapEnumValueAttribute`                    | method                  |       Yes        | Pair enum members                    |
|  [28]   | `MapperIgnoreSourceValueAttribute`         | method                  |       Yes        | Exclude a source enum value          |
|  [29]   | `MapperIgnoreTargetValueAttribute`         | method                  |       Yes        | Exclude a target enum value          |

`Use` values and `IncludeMappingConfigurationAttribute` names accept a reference outside the mapper, and every ignore attribute except `MapperIgnoreObsoleteMembersAttribute` exposes `Justification` as a `string?`.

## [08]-[OPTIONS_AND_MEMBERS]

Settable properties of `MapperAttribute` and `MapperDefaultsAttribute`, where `RequiredEnumMappingStrategy` takes `RequiredMappingStrategy`, `EnabledConversions` takes `MappingConversionType`, `IncludedMembers` and `IncludedConstructors` take `MemberVisibility`, and every other option takes `bool` or the type its name spells:

| [INDEX] | [PROPERTY]                           | [DEFAULT]       | [EFFECT]                       |
| :-----: | :----------------------------------- | :-------------- | :----------------------------- |
|  [01]   | `PropertyNameMappingStrategy`        | `CaseSensitive` | Member-name matching           |
|  [02]   | `EnumMappingStrategy`                | `ByValue`       | Enum-member matching           |
|  [03]   | `EnumNamingStrategy`                 | `MemberName`    | Enum-to-string naming          |
|  [04]   | `EnumMappingIgnoreCase`              | `false`         | Enum-match casing              |
|  [05]   | `ThrowOnMappingNullMismatch`         | `true`          | Null return, non-null result   |
|  [06]   | `ThrowOnPropertyMappingNullMismatch` | `false`         | Null source, non-null member   |
|  [07]   | `AllowNullPropertyAssignment`        | `true`          | Assign null to nullable member |
|  [08]   | `UseDeepCloning`                     | `false`         | Copy instead of reuse          |
|  [09]   | `StackCloningStrategy`               | `PreserveOrder` | Element order of a built stack |
|  [10]   | `EnabledConversions`                 | `Default`       | Enabled conversions            |
|  [11]   | `UseReferenceHandling`               | `false`         | Circular-reference support     |
|  [12]   | `IgnoreObsoleteMembersStrategy`      | `None`          | Obsolete-member policy         |
|  [13]   | `RequiredMappingStrategy`            | `Both`          | Unmapped-member diagnostics    |
|  [14]   | `RequiredEnumMappingStrategy`        | `Both`          | Unmapped-value diagnostics     |
|  [15]   | `IncludedMembers`                    | `AllAccessible` | Mapped member accessibility    |
|  [16]   | `IncludedConstructors`               | `AllAccessible` | Constructor accessibility      |
|  [17]   | `PreferParameterlessConstructors`    | `true`          | Constructor order              |
|  [18]   | `AutoUserMappings`                   | `true`          | Discovery by signature         |

- `AllowNullPropertyAssignment` at `false` turns an existing-target mapping into a merge, and the null options do not apply to a required init property
- `StackCloningStrategy` decides the element order whenever Mapperly builds a `Stack<T>` through `Stack<T>(IEnumerable<T>)`, in every new-instance mapping and never for an existing-target stack or a `Queue<T>`, where `PreserveOrder` emits a `Reverse` call and `ReverseOrder` the bare constructor, which reverses the sequence

Mapperly resolves a flattening (`Item.Owner.Id` to `ItemDto.OwnerId`) from PascalCase names, does not resolve unflattening, which needs `MapPropertyAttribute`, and ignores indexed members. `MapNestedPropertiesAttribute` brings every member under one path into scope as if the source declared them, immediate source members outrank nested ones, automatic flattening outranks both, and nested paths that reach the same target member have no defined order, and `MapPropertyAttribute` names that mapping. `MapPropertyAttribute` resolves a name mismatch instead of renaming domain or DTO members, and `MapperIgnoreSourceAttribute` and `MapperIgnoreTargetAttribute` silence the unmapped-member diagnostic for a computed field or another deliberate omission:

```csharp
[Mapper]
internal static partial class ProfileMapper {
    [MapProperty(nameof(Profile.FullName), nameof(ProfileDto.Name))]
    [MapperIgnoreSource(nameof(Profile.Secret), Justification = "Never leaves the domain")]
    [MapperIgnoreTarget(nameof(ProfileDto.Badge), Justification = "Presentation computes it")]
    public static partial ProfileDto ToDto(Profile profile);
}
```

- Paired constructors take source before target, a `string` path splits on `.` into segments, the `string[]` overload takes the segments as written, `@` works in a path (`nameof(@Some.Namespace.Item.Owner.Id)` yields `Owner.Id`), `Source` and `Target` are `IReadOnlyCollection<string>`, and `SourceFullName` and `TargetFullName` rejoin the segments with `.`
- `MapPropertyAttribute` takes source and target, `MapPropertyFromSourceAttribute` takes a target, `MapValueAttribute` takes a target and an `object?` `Value`, `MapNestedPropertiesAttribute` takes a source, `MapperIgnoreSourceAttribute` and `MapperIgnoreTargetAttribute` take one `string`, `MapperRequiredMappingAttribute` takes a `RequiredMappingStrategy`, and `MapperIgnoreObsoleteMembersAttribute` takes an `IgnoreObsoleteMembersStrategy` that defaults to `Both`
- `StringFormat` is the format string Mapperly passes to `ToString` on an `IFormattable` type, `FormatProvider` names a field or property marked `FormatProviderAttribute`, Mapperly falls back to the one member that sets `Default` to `true`, and a mapper sets it on one member only
- `MapValueAttribute` assigns a constant of the target type, or with `Use` the result of a method returning that type, its parameters filled by name from the additional mapping parameters
- Settable past the constructor are `StringFormat`, `FormatProvider`, `Use`, and `SuppressNullMismatchDiagnostic` on `MapPropertyAttribute`, `StringFormat`, `FormatProvider`, and `Use` on `MapPropertyFromSourceAttribute`, `Use` on `MapValueAttribute`, and `Default` on `FormatProviderAttribute`

## [09]-[ENUMS_AND_STRATEGIES]

Attributes that configure one enum mapping:

| [INDEX] | [SYMBOL]                                              | [MEMBERS]                     |
| :-----: | :---------------------------------------------------- | :---------------------------- |
|  [01]   | `MapEnumAttribute(EnumMappingStrategy strategy)`      | `Strategy`                    |
|  [02]   | `MapEnumValueAttribute(object source, object target)` | `Source` `Target` as `object` |
|  [03]   | `MapperIgnoreSourceValueAttribute(object source)`     | `SourceValue` as `Enum?`      |
|  [04]   | `MapperIgnoreTargetValueAttribute(object target)`     | `TargetValue` as `Enum?`      |

`MapEnumAttribute` applies to a mapping method that takes an enum and takes `IgnoreCase` (`bool`, default `false`), `FallbackValue` (`object?`, default `null`), and `NamingStrategy` (`EnumNamingStrategy`, default `MemberName`), where `FallbackValue` replaces the throw for an unmapped value under `ByName` and `ByValueCheckDefined` only, and neither `FallbackValue` nor `MapEnumValueAttribute` passes through the naming strategy. `MapEnumValueAttribute` applies to an enum-to-enum, enum-to-string, or string-to-enum mapping and pairs an enum member with another enum member or a string literal, and the ignore-value attributes cast their argument to `Enum` and take an enum member only. Under the workspace `ByName` default, members that match by name need no configuration, and explicit pairs cover the rest:

```csharp
[Mapper]
internal static partial class StateMapper {
    [MapEnumValue(InternalState.InProgress, ExternalState.Active)]
    [MapEnumValue(InternalState.Done, ExternalState.Completed)]
    public static partial ExternalState ToExternal(InternalState state);
}

internal enum InternalState { Draft, InProgress, Done, Cancelled }
internal enum ExternalState { Active, Completed, Cancelled, Draft }
```

The options and attributes name these strategy types, and `RequiredMappingStrategy`, `IgnoreObsoleteMembersStrategy`, and `MemberVisibility` carry `[Flags]`, where `|` adds a member and `& ~` removes one:
- `StackCloningStrategy`: `PreserveOrder` `ReverseOrder`
- `PropertyNameMappingStrategy`: `CaseSensitive` `CaseInsensitive` `SnakeCase` `UpperSnakeCase`
- `RequiredMappingStrategy` and `IgnoreObsoleteMembersStrategy`: `None = 0` `Both = ~None` `Source = 1 << 0` `Target = 1 << 1`
- `EnumMappingStrategy`: `ByValue` `ByName` `ByValueCheckDefined`, where the last maps by value and checks that the value is defined
- `EnumNamingStrategy`: `MemberName` `CamelCase` `PascalCase` `SnakeCase` `UpperSnakeCase` `KebabCase` `UpperKebabCase` `ComponentModelDescriptionAttribute` `SerializationEnumMemberAttribute`, where `ComponentModelDescriptionAttribute` reads `DescriptionAttribute.Description`, `SerializationEnumMemberAttribute` reads `EnumMemberAttribute.Value`, and both fall back to the member name
- `MemberVisibility`: `AllAccessible = All | Accessible` `All = Public | Internal | Protected | Private` `Accessible = 1 << 0` `Public = 1 << 1` `Internal = 1 << 2` `Protected = 1 << 3` `Private = 1 << 4`

`Source` and `Target` name the side each strategy acts on, `RequiredMappingStrategy` warns about unmapped members there, `IgnoreObsoleteMembersStrategy` skips obsolete ones, and `MapPropertyAttribute` maps an obsolete member whatever the strategy says.

## [10]-[CONVERSIONS_AND_REFERENCES]

`MappingConversionType` is the `[Flags]` set that `EnabledConversions` takes, `None = 0` disables every automatic conversion, `All = ~None` enables every one, the unset value is `Default = All & ~ExplicitCast`, and an explicit cast operator converts nothing until the mapper names `All` or adds the bit. The generator tries the conversions in one fixed order and the first enabled match wins: step 1 is direct assignment, applied when the source type is assignable to the target type and `UseDeepCloning` is `false`, and step 17 creates a new target instance and maps its members:

| [INDEX] | [MEMBER]               | [BIT]     | [STEP] | [CONDITION]                                                  |
| :-----: | :--------------------- | :-------- | :----: | :----------------------------------------------------------- |
|  [01]   | `Constructor`          | `1 << 0`  |   9    | Target has a constructor taking the source type              |
|  [02]   | `ImplicitCast`         | `1 << 1`  |   7    | Implicit cast operator exists                                |
|  [03]   | `ExplicitCast`         | `1 << 2`  |   13   | Explicit cast operator exists                                |
|  [04]   | `ParseMethod`          | `1 << 3`  |   8    | Source is `string`, target has a static `Parse` overload     |
|  [05]   | `ToStringMethod`       | `1 << 4`  |   14   | Target is `string`, calls `ToString` on the source           |
|  [06]   | `StringToEnum`         | `1 << 5`  |   10   | Source is `string`, target is an enum                        |
|  [07]   | `EnumToString`         | `1 << 6`  |   11   | Source is an enum, target is `string`                        |
|  [08]   | `EnumToEnum`           | `1 << 7`  |   12   | Both are enums, follows `EnumMappingStrategy`                |
|  [09]   | `DateTimeToDateOnly`   | `1 << 8`  |   16   | `DateTime` to `DateOnly` through `FromDateTime`              |
|  [10]   | `DateTimeToTimeOnly`   | `1 << 9`  |   16   | `DateTime` to `TimeOnly` through `FromDateTime`              |
|  [11]   | `Queryable`            | `1 << 10` |   2    | Both are `IQueryable<T>`, expression-tree element mapping    |
|  [12]   | `Enumerable`           | `1 << 11` |   6    | Both are `IEnumerable<T>`, maps each element                 |
|  [13]   | `Dictionary`           | `1 << 12` |   3    | Both are `IDictionary` or `IReadOnlyDictionary`              |
|  [14]   | `Span`                 | `1 << 13` |   4    | Either is `Span<T>` or `ReadOnlySpan<T>`                     |
|  [15]   | `Memory`               | `1 << 14` |   5    | Either is `Memory<T>` or `ReadOnlyMemory<T>`                 |
|  [16]   | `Tuple`                | `1 << 15` |   17   | Target is a `ValueTuple` or a tuple expression               |
|  [17]   | `EnumUnderlyingType`   | `1 << 16` |   12   | Maps an enum from or to its underlying type                  |
|  [18]   | `ToTargetMethod`       | `1 << 17` |   15   | Source has an instance `TTarget ToTTarget()`, not `ToString` |
|  [19]   | `StaticConvertMethods` | `1 << 18` |   16   | Static `ToTTarget`, `Create`, `CreateFrom`, `FromTSource`    |
|  [20]   | `Expression`           | `1 << 19` |   -    | Declared only, the generator never reads it                  |

`StaticConvertMethods` finds a static `ToTTarget` on the source type and `Create`, `CreateFrom`, `CreateFromTSource`, and `FromTSource` in any casing on the target type, an array-typed source adds the `From<TElement>Array` and `CreateFrom<TElement>Array` spellings, the list matches the generated `Create(TKey)` of a value object, and the `Fin`-returning `From` factory matches no listed name. `Tuple` emits a tuple expression outside a queryable projection and `ValueTuple` inside one, the `Enumerable` and `Dictionary` bits cover arrays and every constructible collection target, `EnumUnderlyingType` casts inside the `EnumToEnum` step, the `DateTime` bits gate the static-method step for their pairs, and `Tuple` gates the tuple form of the member-mapping step.

`MapDerivedTypeAttribute(Type sourceType, Type targetType)` and `MapDerivedTypeAttribute<TSource, TTarget>` register one pair for a base-type or interface mapping: every source type extends or implements the parameter type, every target type extends or implements the return type, each source type appears once, source types can share one target type, an ordinary mapping emits a runtime switch that throws `ArgumentException` for an unregistered type, and derived types work for new-instance and existing-target mappings.

`Riok.Mapperly.Abstractions.ReferenceHandling` supports source graphs with circular references, `UseReferenceHandling = true` turns it on, it uses the package's runtime assets, and `runtime` stays out of `ExcludeAssets` on the package reference. `IReferenceHandler` stores and resolves target objects, `PreserveReferenceHandler` is the default handler and returns the same target instance for the same source instance, and `ReferenceHandlerAttribute` marks the handler parameter:

```csharp
bool TryGetReference<TSource, TTarget>(TSource source, [NotNullWhen(true)] out TTarget? target)
    where TSource : notnull where TTarget : notnull;

void SetReference<TSource, TTarget>(TSource source, TTarget target)
    where TSource : notnull where TTarget : notnull;
```

Mapperly calls `TryGetReference` before it creates a target, a `true` result sets `target` and Mapperly uses that instance, and a `false` result makes Mapperly create a new instance and record it through `SetReference`. To supply another handler, add a parameter of type `IReferenceHandler` marked `ReferenceHandlerAttribute`, and a hand-written mapping method takes the same parameter to join the same handler, with a second `using` directive for the namespace.

## [11]-[ANTI_PATTERNS]

| [INDEX] | [WRONG_FORM]                                                        | [CORRECT_FORM]                                             |
| :-----: | :------------------------------------------------------------------ | :--------------------------------------------------------- |
|  [01]   | Mapper conversion that calls `Create` or `Parse` on a domain type   | The hand-written `From` factory over `Validate`            |
|  [02]   | `MapDerivedType` or a partial `Switch` over a closed union          | The generated exhaustive `Switch`, one mapper call per arm |
|  [03]   | `EnabledConversions` on a mapper naming the one added bit           | The whole allowlist, the value replaces and never merges   |
|  [04]   | Mapper method that returns `Fin<T>` or unwraps one                  | Mapper over the success value, `Map` keeps the context     |
|  [05]   | `ToString()` as the wire contract of a value object                 | The key member, or `ToValue` of an `[ObjectFactory<T>]`    |
|  [06]   | Validation or effects inside a query projection                     | Project, materialize, then validate and construct          |
|  [07]   | Existing-target mapping over a value the caller published           | New-instance mapping, or a target that never escapes       |
|  [08]   | Private `[UserMapping]` repeated in every mapper that needs it      | One `internal static class` behind `[UseStaticMapper<T>]`  |
