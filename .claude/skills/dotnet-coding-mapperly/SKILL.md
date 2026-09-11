---
name: dotnet-coding-mapperly
description: "Use when Mapperly maps domain types to or from contracts, covering boundaries, ownership, projections, attributes, options, and conversions."
---

# [DOTNET_CODING_MAPPERLY]

Covers mapping at the host boundary with `Riok.Mapperly`, from where the mapper sits to conversion priority and reference handling.

Mapperly generates each mapping at build time as ordinary member assignments, with no reflection, expression compilation, or hidden allocation:
- Unmapped members report a diagnostic
- `EmitCompilerGeneratedFiles` writes the generated mappings under `obj/` as C# source
- Mapperly cannot consume another source generator's output from the same compilation, a referenced assembly exposes its generated members as metadata
- Automatic conversions can change when a generated type moves between projects
- Explicit mapping declarations keep project layout from choosing conversions

## [01]-[BOUNDARIES]

Adapters that reference both representations own the mapper, and the domain references neither Mapperly nor an external contract:
- Define one mapper per aggregate or feature area, and keep it internal unless mapping is an intentional public contract
- Declare a static partial mapper for a transformation with no stored inputs
- Keep an instance mapper deterministic through immutable and pure collaborators
- Keep mutable state, service location, and ambient reads outside every mapper

| [INDEX] | [DIRECTION]                                       | [MAPPERLY_ROLE]             | [REQUIRED_FORM]                                       |
| :-----: | :------------------------------------------------ | :-------------------------- | :---------------------------------------------------- |
|  [01]   | External contract to raw input model              | Structural mapping          | Validate the raw model before domain construction     |
|  [02]   | External contract to constrained domain value     | None                        | Call the hand-written `From` factory, keep its error  |
|  [03]   | Validated components to domain aggregate          | Optional construction       | Use one constructor total over the components         |
|  [04]   | Domain value to transport or persistence          | Structural projection       | Map only after the domain result is successful        |
|  [05]   | Domain snapshot to next domain snapshot           | None                        | Call a named transition returning the next value      |
|  [06]   | Closed union to external case                     | Structural mapping per case | Dispatch through the union's exhaustive `Switch`      |
|  [07]   | Mutable boundary value to a caller-created target | Existing-target mapping     | Mutate a target that never escapes its creating scope |
|  [08]   | Persistence query to read model                   | Expression projection       | Materialize before domain construction or effects     |

Mappings that can reject input are not plain `TSource -> TTarget` functions:
- Validation owns the rejection and returns the typed `Expected` record its package declares
- Mapper maps the successful value inside its existing context, a transformer stack at its innermost value

```csharp
internal static Fin<ItemDto> ToDto(Fin<Item> value) => value.Map(ItemMapper.ToDto);
```

## [02]-[MAPPER_CONFIGURATION]

MSBuild properties configure every mapper of a project, set once in the shared props file and naming only what differs from the defaults:
- `Riok.Mapperly.targets` declares one compiler-visible property per option with the option name prefixed by `Mapperly`
- Global analyzer configuration sets the same options through a `build_property.Mapperly<Option>` key without an MSBuild property
- Projects that do not reference the package ignore the group

```xml
<PropertyGroup Label="Mapperly">
    <MapperlyEnumMappingStrategy>ByName</MapperlyEnumMappingStrategy>
    <MapperlyAutoUserMappings>false</MapperlyAutoUserMappings>
    <MapperlyThrowOnPropertyMappingNullMismatch>true</MapperlyThrowOnPropertyMappingNullMismatch>
    <MapperlyEnabledConversions>Queryable, Enumerable, Dictionary, Span, Memory, EnumToEnum</MapperlyEnabledConversions>
</PropertyGroup>
```

- Allowlist omits every conversion that parses, formats, casts, or constructs
- Direct assignment and object-member mapping have no bit and stay available, every disabled conversion falls through to object-member mapping
- `string`, enum, and primitive targets have no mappable members and report `RMG007` as a member and `RMG008` as a mapping method
- Composite targets map their members and report `RMG013`, `RMG066`, or nothing
- `MappingConversionType.None` clears `Enumerable` and `Dictionary`
- `List` and `Dictionary` members with a differing element type then map to empty collections with no diagnostic
- Builds that treat warnings as errors fail on every RMG warning, the silent fallthroughs are the cases an explicit mapping covers

MSBuild property, `[assembly: MapperDefaults]`, `[Mapper]`, and the per-method attributes (`MapperRequiredMappingAttribute`, `MapperIgnoreObsoleteMembersAttribute`, `MapEnumAttribute`) configure a mapper, each overriding the one before it:
- `MapperDefaultsAttribute` derives from `MapperAttribute` with the same options
- Overrides replace the whole value, a deviating mapper names its full `EnabledConversions` allowlist
- Inbound mappers that feed constrained domain types keep `ParseMethod`, `Constructor`, `StaticConvertMethods`, and the casts out of their allowlist
- Outbound mappers enable `ImplicitCast` for the generated value-object-to-key operator
- Inbound operator from the key is explicit and calls the throwing `Create`
- Named mappings selected with `Use` own every remaining conversion
- Mappers that add `ToStringMethod` treat it as formatting and pass a fixed provider or an explicit culture input
- Automatic member matching applies where names and meanings agree, one exact pair has one intentional default
- Alternative mappings have unique names, generic mapping methods are disjoint, and no selection depends on declaration order

External mappings stay local to the mapper that consumes them, and assembly-wide registrations expose only disjoint pairs:
- Configuration inclusion copies configuration, not implementation, and needs identical direction, member meaning, null policy, and omissions
- Additional parameters hold immutable values that the boundary already resolved
- Additional parameters forward to nested user mappings and `Use` methods where remaining parameter names match
- Type pairs shared between mappers belong in one `internal static class` reached through `[UseStaticMapper<T>]`
- Private `[UserMapping]` methods stay for a mapper-local pair

## [03]-[CONSTRUCTION_AND_OWNERSHIP]

Mapping form decides who owns the target while its members are written:

| [INDEX] | [FORM]                  | [TARGET_STATE]                            | [CONSTRAINT]                                                    |
| :-----: | :---------------------- | :---------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | New instance            | Mapper-owned until return                 | Constructor and init values precede writable assignments        |
|  [02]   | Mapperly object factory | Factory result, then writable assignments | Constructor parameters and init-only members are not mapped     |
|  [03]   | Existing target         | Caller-owned mutable value                | Assignments and collection additions are observable mutation    |
|  [04]   | Reference handling      | Identity graph under one handler          | Registration follows construction and precedes writable members |
|  [05]   | Runtime dispatch        | Registered source and target pairs        | Unregistered pair or subtype throws                             |

- Prefer one total constructor for an immutable external record
- `[MapperConstructor]` chooses between equivalent external constructors and does not select a domain transition
- Types with invariants that depend on assignments after construction are not valid mapping targets
- Mapperly cannot skip an init-only assignment to preserve its member initializer

Mapperly object factories allocate or select the target and expose no typed failure channel:
- Factory is pure, deterministic, and synchronous, returns a non-void type, and takes zero parameters or one
- Factory can be generic with or without constraints, the first factory with a matching signature wins
- Nullable results fall back to a public parameterless construction or throw `NullReferenceException`
- Mapperly object factory is unrelated to the Thinktecture `[ObjectFactory<T>]`, which declares a validating conversion to and from one other type

Member and constructor visibility stays at `AllAccessible`:
- Direct assignment can return the source reference, sharing is valid only when the complete reachable graph is immutable
- Deep cloning is an allocation strategy, not proof of ownership, validity, or a completed domain transition

Existing-target mappings mutate their target, and an existing collection adds without replacement:
- Lists add, queues enqueue, stacks push in source order, and a null source collection leaves the target unchanged
- Null-skipping implements merge behavior, it cannot distinguish an omitted member from one cleared to null
- Explicit optional wrappers fold against `[MappingTargetOriginalValue]` at a mutable boundary
- Constructor and init-only targets receive `default` as the original value

Reference handling materializes an external graph that requires cycles or shared identity, and one handler serves one mapping call:
- Constructor and init-only edges run before registration and cannot close a generated cycle
- Existing-target roots start unregistered, the pair is registered before mapping when a back-reference must retain the supplied root
- Domain identity uses explicit identifiers, not mapper reference state

## [04]-[DOMAIN_TYPE_INTEGRATION]

Generated domain types cross the mapper only through their declared conversions:
- Inbound through the `From` factory, outbound through the key member or the `ToValue` of a declared `[ObjectFactory<T>]`
- `ToString()` does not define that representation
- `Create`, `Parse`, an accessible constructor, a static conversion method, and an explicit operator turn expected rejection into an exception
- Mapperly enum configuration applies only to CLR enums, independent CLR enum contracts map by case-sensitive name or explicit value pairs

Closed unions use their generated `Switch` as the outer dispatcher, Mapperly maps one known case inside each arm, and case selection stays exhaustive while member translation stays structural. `Map` takes one value per case and receives no mapper call:

```csharp
internal static ChangeDto ToDto(Change value) =>
    value.Switch(added: ChangeMapper.ToDto, removed: ChangeMapper.ToDto);
```

- Closed-union exhaustiveness is lost under `MapDerivedType`, a duplicated case list, `SwitchPartially`, `MapPartially`, a `@default` arm, or `StopAt`
- Generic, runtime-target, and derived dispatch belong only to a runtime-registered type set where a mismatch is a defect

LanguageExt owns absence, failure, validation, effects, traversal, and transformer stacks:
- Mapperly methods supply the function passed to `Map`, `BiMap`, `Apply`, or a traversal, total over a validated source
- Throw from `ThrowOnPropertyMappingNullMismatch` signals a defect, not an expected error
- Automatic wrapper construction through constructor or cast discovery can manufacture a success case, unwrap a failure, or discard source elements
- Generic wrapper helpers need explicit `Use` selection and preserve every case

LanguageExt collections keep their own construction policy:
- Direct assignment shares an immutable collection only when sharing is intentional
- Element-changing sequences use the collection's `Map`
- Mutable sources are snapshotted before domain publication, an arbitrary enumerable materializes first
- Maps are built only after key validation defines ordering, uniqueness, and collision behavior
- Incidental enumerable-tuple construction defines none of those

## [05]-[QUERY_PROJECTIONS]

Query projections are expression trees that a query provider interprets:
- Projections belong in the data adapter, return a read model or transport contract, and compose as `query.ProjectToDto().ToListAsync()`
- Keep a projection declaration separate from an in-memory mapping when their conversion, null, or user-method policies differ
- Projection method takes member configuration from an element mapping with the same source and target pair
- Projections with additional parameters read configuration from an element mapping where the parameters match by name

Mapperly must inline each user method, and the query provider must translate the resulting expression:
- Inlining needs an expression body, one return statement, or one local declaration followed by a return
- Any other shape reports `RMG068` and leaves the call in the expression
- Additional parameters are immutable scalar query values
- Services, mapper state, clocks, configuration objects, and request contexts do not enter the expression

Nullable analysis and the property-null options do not apply inside a projection:
- Nullable paths can become empty text, `default`, or a conditional fallback, the read model matches storage nullability
- Object factories, existing-target mapping, dictionary mapping, deep cloning, and reference handling do not apply
- Reference handling reports `RMG029`, unsupported enum configuration reports `RMG032` and emits a value cast
- Project stored values, materialize the query, then validate and construct domain values
- `Fin`-returning `From` factories, LanguageExt composition, and effects run after materialization
- Unmatched derived projections return `default(TTarget)`, valid only when the target contract states that result
- `AsEnumerable` leaves the boundary where it is, a narrow query materializes before the in-memory pipeline begins

## [06]-[MAPPING_METHODS]

Attributes on a `[Mapper] partial class` or `[Mapper] static partial class` and its partial methods hold the configuration:
- Non-partial methods with the matching types implement a member mapping by hand
- Under `AutoUserMappings = false`, a hand-written mapping needs `[UserMapping]` for its type pair
- Mapperly then uses the user mapping in place of an automatic conversion
- `Default` marks the pair's one default mapping, `Ignore` excludes a discovered method
- One user mapping holds the `ToValue` of a complex value object that declares `[ObjectFactory<string>]` outward, another formats with a text pattern

```csharp
[Mapper]
internal static partial class ItemMapper {
    public static partial ItemDto ToDto(Item item);
    public static partial IEnumerable<ItemDto> ToDtos(IEnumerable<Item> items);

    [UserMapping]
    private static string MapRange(Bounds range) => range.ToValue();

    [UserMapping]
    private static string MapListed(Instant listed) => InstantPattern.ExtendedIso.Format(listed);
}

internal sealed record Item(Guid Id, Bounds Range, decimal Amount, Instant ListedAt, Seq<Line> Lines);
internal sealed record ItemDto(Guid Id, string Range, decimal Amount, string ListedAt, IReadOnlyList<LineDto> Lines);
```

`ItemDto.Lines` is a BCL collection, the DTO is the host's contract, and the enabled `Enumerable` conversion maps each `Seq<Line>` element through the `Line` to `LineDto` mapping into the `IReadOnlyList<LineDto>` target.

Mapping method declarations, one row per capability:

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

- Non-static mappers that declare any static mapping method declare every mapping method static
- Static mapping methods satisfy a `static abstract` interface member, and a `[Mapper] partial class` can implement a mapping interface
- Existing-target methods return `void`, the second parameter is the target unless `MappingTargetAttribute` names another
- `MappingTargetAttribute` accepts the `this` parameter of an extension method
- Generic and runtime-target methods dispatch to the mappings declared in the same mapper and throw `ArgumentException` for an unknown pair
- Generic and runtime-target methods accept no additional parameter and accept `MapDerivedTypeAttribute` on the method itself

## [07]-[ATTRIBUTES]

Every attribute Mapperly reads, with its declaration target and whether it repeats:

| [INDEX] | [SYMBOL]                                   | [TARGET]                      | [ALLOW_MULTIPLE] | [CAPABILITY]                         |
| :-----: | :----------------------------------------- | :---------------------------- | :--------------: | :----------------------------------- |
|  [01]   | `MapperAttribute`                          | `class`                       |        No        | Mapper declaration and options       |
|  [02]   | `MapperDefaultsAttribute`                  | `assembly`                    |        No        | Same options for every mapper        |
|  [03]   | `UseMapperAttribute`                       | `field`, `property`           |        No        | Use the member's mapping methods     |
|  [04]   | `UseStaticMapperAttribute`                 | `class`, `assembly`           |       Yes        | Use a type's static mapping methods  |
|  [05]   | `UseStaticMapperAttribute<T>`              | `class`, `assembly`           |       Yes        | Same, generic form                   |
|  [06]   | `MapperConstructorAttribute`               | `constructor`                 |        No        | Select the constructor to call       |
|  [07]   | `ObjectFactoryAttribute`                   | `method`                      |        No        | Construct or resolve the target      |
|  [08]   | `MapperIgnoreAttribute`                    | `property`, `field`, `method` |        No        | Exclude a member or a method         |
|  [09]   | `UserMappingAttribute`                     | `method`                      |        No        | User-implemented mapping method      |
|  [10]   | `NamedMappingAttribute`                    | `method`                      |        No        | Name a mapping for `Use`             |
|  [11]   | `IncludeMappingConfigurationAttribute`     | `method`                      |       Yes        | Reuse another method's configuration |
|  [12]   | `MappingTargetAttribute`                   | `parameter`                   |        No        | Mark the parameter as the target     |
|  [13]   | `MappingTargetOriginalValueAttribute`      | `parameter`                   |        No        | Pass the target member's prior value |
|  [14]   | `FormatProviderAttribute`                  | `field`, `property`           |        No        | Expose an `IFormatProvider`          |
|  [15]   | `ReferenceHandlerAttribute`                | `parameter`                   |        No        | Mark the reference-handler parameter |
|  [16]   | `MapPropertyAttribute`                     | `method`                      |       Yes        | Rename, flatten, and unflatten       |
|  [17]   | `MapPropertyFromSourceAttribute`           | `method`                      |       Yes        | Map the source object to a member    |
|  [18]   | `MapNestedPropertiesAttribute`             | `method`                      |       Yes        | Flatten every member of a path       |
|  [19]   | `MapValueAttribute`                        | `method`                      |       Yes        | Assign a constant or generated value |
|  [20]   | `MapperIgnoreSourceAttribute`              | `method`                      |       Yes        | Exclude a source member              |
|  [21]   | `MapperIgnoreTargetAttribute`              | `method`                      |       Yes        | Exclude a target member              |
|  [22]   | `MapperRequiredMappingAttribute`           | `method`                      |        No        | Unmapped-member diagnostics          |
|  [23]   | `MapperIgnoreObsoleteMembersAttribute`     | `method`                      |        No        | Obsolete-member policy               |
|  [24]   | `MapDerivedTypeAttribute`                  | `method`                      |       Yes        | One derived source and target pair   |
|  [25]   | `MapDerivedTypeAttribute<TSource,TTarget>` | `method`                      |       Yes        | Same, generic form                   |
|  [26]   | `MapEnumAttribute`                         | `method`                      |        No        | Enum strategy for one mapping        |
|  [27]   | `MapEnumValueAttribute`                    | `method`                      |       Yes        | Pair enum members                    |
|  [28]   | `MapperIgnoreSourceValueAttribute`         | `method`                      |       Yes        | Exclude a source enum value          |
|  [29]   | `MapperIgnoreTargetValueAttribute`         | `method`                      |       Yes        | Exclude a target enum value          |

- `Use` values and `IncludeMappingConfigurationAttribute` names accept a reference outside the mapper
- Every ignore attribute except `MapperIgnoreObsoleteMembersAttribute` exposes `Justification` as a `string?`

## [08]-[OPTIONS_AND_MEMBERS]

Settable properties of `MapperAttribute` and `MapperDefaultsAttribute`, `RequiredEnumMappingStrategy` takes `RequiredMappingStrategy`, `EnabledConversions` takes `MappingConversionType`, `IncludedMembers` and `IncludedConstructors` take `MemberVisibility`, and every other option takes `bool` or the type its name spells:

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

- `AllowNullPropertyAssignment` at `false` turns an existing-target mapping into a merge, `required` init properties ignore the null options
- `StackCloningStrategy` decides the element order whenever a new-instance mapping builds a `Stack<T>` through `Stack<T>(IEnumerable<T>)`
- `PreserveOrder` emits a `Reverse` call, `ReverseOrder` uses the plain constructor, which reverses the sequence

Mapperly resolves a flattening (`Item.Owner.Id` to `ItemDto.OwnerId`) from PascalCase names:
- Mapperly does not resolve unflattening, which needs `MapPropertyAttribute`, and ignores indexed members
- `MapNestedPropertiesAttribute` brings every member under one path into scope as if the source declared them
- Immediate source members outrank nested ones, automatic flattening outranks both
- Nested paths that reach the same target member have no defined order, `MapPropertyAttribute` names that mapping
- `MapPropertyAttribute` resolves a name mismatch while domain and DTO members keep their names
- `MapperIgnoreSourceAttribute` and `MapperIgnoreTargetAttribute` silence the unmapped-member diagnostic for a deliberate omission

```csharp
[Mapper]
internal static partial class ProfileMapper {
    [MapProperty(nameof(Profile.FullName), nameof(ProfileDto.Name))]
    [MapperIgnoreSource(nameof(Profile.Secret), Justification = "Never leaves the domain")]
    [MapperIgnoreTarget(nameof(ProfileDto.Badge), Justification = "Presentation computes it")]
    public static partial ProfileDto ToDto(Profile profile);
}
```

- Paired constructors take source before target, a `string` path splits on `.` into segments, the `string[]` overload takes the segments as written
- `@` works in a path, `nameof(@Some.Namespace.Item.Owner.Id)` yields `Owner.Id`
- `Source` and `Target` are `IReadOnlyCollection<string>`, `SourceFullName` and `TargetFullName` rejoin the segments with `.`
- `MapPropertyAttribute` takes source and target, `MapPropertyFromSourceAttribute` takes a target, `MapNestedPropertiesAttribute` takes a source
- `MapValueAttribute` takes a target and an `object?` `Value`, `MapperIgnoreSourceAttribute` and `MapperIgnoreTargetAttribute` take one `string`
- `MapperRequiredMappingAttribute` takes a `RequiredMappingStrategy`
- `MapperIgnoreObsoleteMembersAttribute` takes an `IgnoreObsoleteMembersStrategy` that defaults to `Both`
- `StringFormat` is the format string Mapperly passes to `ToString` on an `IFormattable` type
- `FormatProvider` names a field or property marked `FormatProviderAttribute`, one member per mapper sets `Default` to `true` as the fallback
- `MapValueAttribute` assigns a constant of the target type, or with `Use` the result of a method returning that type
- `Use` method parameters match by name from the additional mapping parameters
- `MapPropertyAttribute` sets `StringFormat`, `FormatProvider`, `Use`, and `SuppressNullMismatchDiagnostic` past the constructor
- `MapPropertyFromSourceAttribute` sets `StringFormat`, `FormatProvider`, and `Use` past the constructor
- `MapValueAttribute` sets `Use` and `FormatProviderAttribute` sets `Default` past the constructor

## [09]-[ENUMS_AND_STRATEGIES]

Attributes that configure one enum mapping:

| [INDEX] | [SYMBOL]                                              | [MEMBERS]                     |
| :-----: | :---------------------------------------------------- | :---------------------------- |
|  [01]   | `MapEnumAttribute(EnumMappingStrategy strategy)`      | `Strategy`                    |
|  [02]   | `MapEnumValueAttribute(object source, object target)` | `Source` `Target` as `object` |
|  [03]   | `MapperIgnoreSourceValueAttribute(object source)`     | `SourceValue` as `Enum?`      |
|  [04]   | `MapperIgnoreTargetValueAttribute(object target)`     | `TargetValue` as `Enum?`      |

- `MapEnumAttribute` applies to a mapping method that takes an enum
- `MapEnumAttribute` takes `IgnoreCase` (`bool`, default `false`) and `FallbackValue` (`object?`, default `null`)
- `MapEnumAttribute` takes `NamingStrategy` (`EnumNamingStrategy`, default `MemberName`)
- `FallbackValue` replaces the throw for an unmapped value under `ByName` and `ByValueCheckDefined` only
- Neither `FallbackValue` nor `MapEnumValueAttribute` passes through the naming strategy
- `MapEnumValueAttribute` applies to an enum-to-enum, enum-to-string, or string-to-enum mapping
- `MapEnumValueAttribute` pairs an enum member with another enum member or a string literal
- Ignore-value attributes cast their argument to `Enum` and take an enum member only
- Under `ByName`, members that match by name need no configuration, and explicit pairs cover the rest

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

Options and attributes name the strategy types, and `RequiredMappingStrategy`, `IgnoreObsoleteMembersStrategy`, and `MemberVisibility` declare `[Flags]`, `|` adds a member and `& ~` removes one:
- `StackCloningStrategy`: `PreserveOrder` `ReverseOrder`
- `PropertyNameMappingStrategy`: `CaseSensitive` `CaseInsensitive` `SnakeCase` `UpperSnakeCase`
- `RequiredMappingStrategy` and `IgnoreObsoleteMembersStrategy`: `None = 0` `Both = ~None` `Source = 1 << 0` `Target = 1 << 1`
- `EnumMappingStrategy`: `ByValue` `ByName` `ByValueCheckDefined`, and the last maps by value and checks that the value is defined
- `EnumNamingStrategy`: `MemberName` `CamelCase` `PascalCase` `SnakeCase` `UpperSnakeCase` `KebabCase` `UpperKebabCase` and the attribute readers
- `ComponentModelDescriptionAttribute` reads `DescriptionAttribute.Description`, `SerializationEnumMemberAttribute` reads `EnumMemberAttribute.Value`
- Attribute readers fall back to the member name
- `MemberVisibility`: `Accessible = 1 << 0` `Public = 1 << 1` `Internal = 1 << 2` `Protected = 1 << 3` `Private = 1 << 4`
- `MemberVisibility.All = Public | Internal | Protected | Private`, `AllAccessible = All | Accessible`

`Source` and `Target` name the side each strategy acts on, `RequiredMappingStrategy` warns about unmapped members there, `IgnoreObsoleteMembersStrategy` skips obsolete ones, and `MapPropertyAttribute` maps an obsolete member whatever the strategy says.

## [10]-[CONVERSIONS_AND_REFERENCES]

`MappingConversionType` is the `[Flags]` set that `EnabledConversions` takes:
- `None = 0` disables every automatic conversion, `All = ~None` enables every one, the unset value is `Default = All & ~ExplicitCast`
- Explicit cast operators convert nothing until the mapper names `All` or adds the bit
- Generator tries the conversions in one fixed order and the first enabled match wins
- Step 1 is direct assignment, applied when the source type is assignable to the target type and `UseDeepCloning` is `false`
- Step 17 creates a new target instance and maps its members

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

- `StaticConvertMethods` finds a static `ToTTarget` on the source type
- `StaticConvertMethods` finds `Create`, `CreateFrom`, `CreateFromTSource`, and `FromTSource` in any casing on the target type
- Array-typed sources add the `From<TElement>Array` and `CreateFrom<TElement>Array` spellings
- Static-method list matches the generated `Create(TKey)` of a value object, the `Fin`-returning `From` factory matches no listed name
- `Tuple` emits a tuple expression outside a queryable projection and `ValueTuple` inside one
- `Enumerable` and `Dictionary` bits cover arrays and every constructible collection target
- `EnumUnderlyingType` casts inside the `EnumToEnum` step
- `DateTime` bits gate the static-method step for their pairs, `Tuple` gates the tuple form of the member-mapping step

`MapDerivedTypeAttribute(Type sourceType, Type targetType)` and `MapDerivedTypeAttribute<TSource, TTarget>` register one pair for a base-type or interface mapping:
- Every source type extends or implements the parameter type, every target type extends or implements the return type
- Each source type appears once, source types can share one target type
- Ordinary mappings emit a runtime switch that throws `ArgumentException` for an unregistered type
- Derived types work for new-instance and existing-target mappings

`Riok.Mapperly.Abstractions.ReferenceHandling` supports source graphs with circular references:
- `UseReferenceHandling = true` turns it on
- Reference handling uses the package's runtime assets, `runtime` stays out of `ExcludeAssets` on the package reference
- `IReferenceHandler` stores and resolves target objects
- `PreserveReferenceHandler` is the default handler and returns the same target instance for the same source instance
- `ReferenceHandlerAttribute` marks the handler parameter

```csharp
bool TryGetReference<TSource, TTarget>(TSource source, [NotNullWhen(true)] out TTarget? target)
    where TSource : notnull where TTarget : notnull;

void SetReference<TSource, TTarget>(TSource source, TTarget target)
    where TSource : notnull where TTarget : notnull;
```

- Mapperly calls `TryGetReference` before it creates a target, a `true` result sets `target` and Mapperly uses that instance
- `false` results make Mapperly create a new instance and record it through `SetReference`
- To supply another handler, add a parameter of type `IReferenceHandler` marked `ReferenceHandlerAttribute`
- Hand-written mapping methods take the same parameter to join the same handler, with a second `using` directive for the namespace

## [11]-[ANTI_PATTERNS]

| [INDEX] | [WRONG_FORM]                                                         | [CORRECT_FORM]                                            |
| :-----: | :------------------------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | Mapper conversion that calls `Create` or `Parse` on a domain type    | Hand-written `From` factory over `Validate`               |
|  [02]   | `MapDerivedType` or a partial `Switch` over a closed union           | Generated exhaustive `Switch`, one mapper call per arm    |
|  [03]   | `EnabledConversions` on a mapper naming the one added bit            | Whole allowlist, the value replaces and never merges      |
|  [04]   | Mapper method that returns `Fin<T>` or unwraps one                   | Mapper over the success value, `Map` keeps the context    |
|  [05]   | `ToString()` as the wire contract of a value object                  | Key member, or `ToValue` of an `[ObjectFactory<T>]`       |
|  [06]   | Validation or effects inside a query projection                      | Project, materialize, then validate and construct         |
|  [07]   | Existing-target mapping over a value the caller published            | New-instance mapping, or a target that never escapes      |
|  [08]   | Private `[UserMapping]` repeated in every mapper that needs it       | One `internal static class` behind `[UseStaticMapper<T>]` |
|  [09]   | Mapper-built `Option`, `Either`, `Validation`, `Try`, `IO`, or `Eff` | Value passed through, cases selected in boundary code     |
|  [10]   | Unsafe accessor over a private constructor or hidden member          | Declared factory of the constrained type                  |
|  [11]   | Object factory that resolves services                                | Pure factory over its parameters                          |
