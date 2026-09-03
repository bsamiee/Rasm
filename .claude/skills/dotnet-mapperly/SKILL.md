---
name: dotnet-mapperly
description: "ENTER LATER AFTER FINISHED, <20-25 WORDS MAX"
---

# [DOTNET_MAPPERLY]

Covers mapping at the host boundary with `Riok.Mapperly`: where the mapper sits, its configuration levels and conversion allowlist, how a target is constructed and owned, how generated domain types and LanguageExt contexts pass through it, query projections, the mapping method shapes, every attribute and option, the enum and strategy types, conversion priority, and reference handling. Where the boundary sits and which result type crosses it are decisions that `dotnet-coding` states, conversions between result types belong to `dotnet-languageext`, and declaring a value object, smart enum, or union belongs to `dotnet-thinktecture`.

Mapperly generates each mapping at build time as ordinary property assignments, with no reflection, expression compilation, or hidden allocation, and an unmapped member fails the build. It serves performance-critical paths, transport and persistence contracts, read models, message payloads, view models, and AOT compilation, and `EmitCompilerGeneratedFiles` writes the generated mappings under `obj/` as C# source. Mapperly cannot consume another source generator's output from the same compilation, a referenced assembly exposes its generated members as metadata, so an automatic conversion can change when a generated type moves between projects, and an explicit mapping declaration keeps project layout from choosing conversions.

## [01]-[BOUNDARIES]

The adapter that references both representations owns the mapper, and the domain references neither Mapperly nor an external contract:
- Define one mapper per aggregate or feature area, and keep it internal unless mapping is an intentional public contract
- Declare a static partial mapper for a transformation with no stored inputs, and keep an instance mapper deterministic through immutable and pure collaborators
- Keep mutable state, service location, and ambient reads outside every mapper

| [INDEX] | [DIRECTION]                                       | [MAPPERLY_ROLE]             | [REQUIRED_FORM]                                       |
| :-----: | :------------------------------------------------ | :-------------------------- | :---------------------------------------------------- |
|  [01]   | External contract to raw input model              | Structural mapping          | Validate the raw model before domain construction     |
|  [02]   | External contract to constrained domain value     | None                        | Call the hand-written `From` adapter, keep its error  |
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
- See `dotnet-coding` for combining independent `From` results with the tuple `Apply` before the aggregate is constructed

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

The allowlist omits every conversion that parses, formats, casts, or constructs, direct assignment and object-member mapping carry no bit and stay available, and every disabled conversion falls through to object-member mapping. A `string`, enum, or primitive target has no mappable members and reports `RMG007`, and a composite target maps its members and reports `RMG013`, `RMG066`, or nothing. `MappingConversionType.None` is not the stricter setting, because it also clears `Enumerable` and `Dictionary`, and `List` and `Dictionary` members then map to empty collections with no diagnostic. The workspace compiles warnings as errors, so RMG codes fail the build, and the silent fallthroughs are the cases an explicit mapping must cover.

Four levels configure a mapper, each overriding the one before it: the MSBuild property, `[assembly: MapperDefaults]`, `[Mapper]`, and the per-method attributes (`MapperRequiredMappingAttribute`, `MapperIgnoreObsoleteMembersAttribute`, `MapEnumAttribute`). `MapperDefaultsAttribute` derives from `MapperAttribute` with the same options, an override replaces the whole value, and a deviating mapper names its full `EnabledConversions` allowlist:
- Inbound mappers that feed constrained domain types never allow `ParseMethod`, `Constructor`, `StaticConvertMethods`, or a cast in place of validation
- Outbound mappers enable `ImplicitCast` for the generated value-object-to-key operator, and the inbound operator is explicit and calls the throwing `Create`
- Named mappings selected with `Use` own every remaining conversion
- Mappers that add `ToStringMethod` treat it as formatting, not a wire contract, and pass a fixed provider or an explicit culture input
- Automatic member matching applies only when names and meanings agree, one exact pair has one intentional default, alternatives have unique names, generic mapping methods are disjoint, and no selection depends on declaration order

External mappings stay local to the mapper that consumes them, and assembly-wide registrations expose only disjoint pairs. Configuration inclusion copies configuration rather than implementation and needs identical direction, member meaning, null policy, and omissions. Additional parameters hold immutable values that the boundary already resolved and forward to nested user mappings and `Use` methods where remaining parameter names match. A type pair shared by several mappers belongs in one `internal static class` reached through `[UseStaticMapper<T>]`, and a private `[UserMapping]` stays for a mapper-local pair.

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

Mapperly object factories allocate or select the target and expose no typed failure channel: the factory is pure, deterministic, and synchronous, returns a non-void type, takes zero parameters or one, can be generic with or without constraints, and the first factory with a matching signature wins. A nullable result falls back to a public parameterless construction or throws `NullReferenceException`, and a factory never resolves services, allocates domain identity, or calls a rejecting domain factory. The Mapperly object factory is unrelated to the Thinktecture `[ObjectFactory<T>]`, which declares a validating conversion between two types.

Member and constructor visibility stays at `AllAccessible`, and an unsafe accessor that calls a private constructor or writes a hidden member never constructs or modifies a constrained domain type. Direct assignment can return the source reference, and sharing is valid only when the complete reachable graph is immutable, because deep cloning is an allocation strategy, not proof of ownership, validity, or a completed domain transition.

Existing-target mappings mutate their target, and an existing collection adds without replacement: lists add, queues enqueue, stacks push the added segment in reverse source order, and a null source collection leaves the target unchanged. Null-skipping implements merge behavior, not patch semantics, because it cannot distinguish an omitted member from one cleared to null, and an explicit optional wrapper folds against `[MappingTargetOriginalValue]` at a mutable boundary, where a constructor or init-only target receives `default` as the original value.

Reference handling materializes an external graph that requires cycles or shared identity, and one handler serves one mapping call. Constructor and init-only edges run before registration and cannot close a generated cycle, an existing-target root starts unregistered, so register the pair before mapping when a back-reference must retain the supplied root, and domain identity uses explicit identifiers rather than mapper reference state.

## [04]-[DOMAIN_TYPE_INTEGRATION]

A generated domain type crosses the mapper only through its declared conversions, inbound through the `From` adapter and outbound through the key member or the `ToValue` of a declared `[ObjectFactory<T>]`, and `ToString()` does not define that representation. `Create`, `Parse`, an accessible constructor, a static conversion method, and an explicit operator turn expected rejection into an exception, so no automatic conversion reaches them. Mapperly enum configuration applies only to CLR enums, and two independent CLR enum contracts map by case-sensitive name or explicit value pairs, never by numeric position.
- See `dotnet-coding` for the `From` adapter that maps `Validate` to `Fin<T>` and `TryGet` to `Option<T>`

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

Nullable analysis and the property-null options do not apply inside a projection, a nullable path can become empty text, `default`, or a conditional fallback, and the read model matches storage nullability. Object factories, existing-target mapping, dictionary mapping, deep cloning, and reference handling do not apply, reference handling reports `RMG029`, and unsupported enum configuration reports `RMG032` and emits a value cast. Project stored values, materialize the query, then validate and construct domain values, so the `Fin`-returning `From` adapters, LanguageExt composition, and effects run after materialization. An unmatched derived projection returns `default(TTarget)`, valid only when the target contract states that result, and `AsEnumerable` does not hide the boundary, because a narrow query materializes before the in-memory pipeline begins.

## [06]-[MAPPING_METHODS]

Attributes on a `[Mapper] partial class` or `[Mapper] static partial class` and its partial methods carry the configuration, and a non-partial method with the matching types implements a member mapping by hand. Under `AutoUserMappings = false`, a hand-written mapping needs `[UserMapping]` for its type pair, and Mapperly then uses it in place of an automatic conversion, where `Default` marks the pair's one default mapping and `Ignore` excludes a discovered method. One user mapping carries a value object's `ToValue` outward, and another formats with a fixed culture:

```csharp
[Mapper]
internal static partial class ItemMapper {
    public static partial ItemDto ToDto(Item item);
    public static partial IEnumerable<ItemDto> ToDtos(IEnumerable<Item> items);

    [UserMapping]
    private static string MapCode(Code code) => code.ToValue();

    [UserMapping]
    private static string MapListed(DateTimeOffset listed) => listed.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}

internal sealed record Item(Guid Id, Code Code, decimal Amount, DateTimeOffset ListedAt, Seq<Line> Lines);
internal sealed record ItemDto(Guid Id, string Code, decimal Amount, string ListedAt, List<LineDto> Lines);
```

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
|  [27]   | `MapEnumValueAttribute`                    | method                  |       Yes        | Pair two enum members                |
|  [28]   | `MapperIgnoreSourceValueAttribute`         | method                  |       Yes        | Exclude a source enum value          |
|  [29]   | `MapperIgnoreTargetValueAttribute`         | method                  |       Yes        | Exclude a target enum value          |

`Use` values and `IncludeMappingConfigurationAttribute` names accept a reference outside the mapper, and the five ignore attributes expose `Justification` as a `string?`.
