<!-- Integrated into .claude/skills/dotnet-mapperly/SKILL.md
# [MAPPING_BOUNDARIES]

`Riok.Mapperly` is a compile-time object mapper that generates mapping code at build time. The generated code matches hand-written property assignments: no runtime reflection, no expression-tree compilation, no hidden allocations. Use Mapperly for performance-critical paths, transport and persistence contracts, read models, message payloads, view models, and AOT compilation. Unmapped members fail the build. `EmitCompilerGeneratedFiles` writes the generated mappings under `obj/` as ordinary C# source.

Mapperly cannot consume another source generator's output from the same compilation. Referenced assemblies expose accessible generated members as metadata: automatic conversion can change when a generated type moves between projects. Explicit mapping declarations prevent project layout from choosing conversions.
-->

<!-- Integrated into .claude/skills/dotnet-mapperly/SKILL.md
## [01]-[BOUNDARIES]

Adapters that reference both representations own the mapper, the domain references neither Mapperly nor an external contract:
- Define one mapper per aggregate or feature area
- Keep mapper types internal unless mapping is an intentional public contract
- Prefer a static partial mapper for transformations that need no stored inputs
- Instance mappers stay deterministic when their stored collaborators are immutable and pure
- Mutable state, service location, and ambient reads stay outside every mapper

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

Mappings that can reject input are not plain `TSource -> TTarget` functions, validation owns rejection and returns the typed `Expected` record its package declares. Map a successful value inside its existing context:

```csharp
internal static Fin<OrderDto> ToDto(Fin<Order> value) => value.Map(OrderDtoMapper.ToDto);
```

Independent boundary validations accumulate: each `From` becomes a `Validation`, one `Apply` builds the domain value, and `ToFin` returns the boundary result.

```csharp
internal static Fin<Shipment> ToShipment(ShipmentDto dto) =>
    (Recipient.From(dto.Recipient).ToValidation(), Weight.From(dto.Weight).ToValidation())
        .Apply(static (recipient, weight) => new Shipment(recipient, weight))
        .As().ToFin();
```

Apply the same rule to `Option`, `Either`, `Validation`, `Try`, `IO`, `Eff`, transformers, and higher-kinded contexts. Mapperly never selects or rebuilds their cases, and a transformer stack maps at its innermost value.
-->

<!-- Integrated into .claude/skills/dotnet-mapperly/SKILL.md
## [02]-[MAPPER_CONFIGURATION]

Configure every project from one `PropertyGroup` in `Directory.Build.props`, naming only what differs from the defaults. `Riok.Mapperly.targets` declares one compiler-visible property per option, the option name prefixed with `Mapperly`. Global analyzer configuration sets the same options with a `build_property.Mapperly<Option>` key and needs no MSBuild property. Projects that do not reference the package ignore the group.

```xml
<PropertyGroup Label="Mapperly">
    <MapperlyEnumMappingStrategy>ByName</MapperlyEnumMappingStrategy>
    <MapperlyAutoUserMappings>false</MapperlyAutoUserMappings>
    <MapperlyThrowOnPropertyMappingNullMismatch>true</MapperlyThrowOnPropertyMappingNullMismatch>
    <MapperlyEnabledConversions>Queryable, Enumerable, Dictionary, Span, Memory, EnumToEnum</MapperlyEnabledConversions>
</PropertyGroup>
```

The allowlist omits every conversion that parses, formats, casts, or constructs. Direct assignment and object-member mapping carry no bit and stay available. Every disabled conversion falls through to object-member mapping. Targets that are `string`, an enum, or a primitive have no mappable members and report `RMG007`. Composite targets map their members instead and report `RMG013`, `RMG066`, or nothing. `MappingConversionType.None` is not the stricter setting: it also clears `Enumerable` and `Dictionary`. `List` and `Dictionary` members then map to empty collections with no diagnostic. The workspace compiles warnings as errors, RMG codes fail the build, and the silent fallthroughs are the cases an explicit mapping must cover.

Four levels configure a mapper, each overriding the one before it: the MSBuild property, `[assembly: MapperDefaults]`, `[Mapper]`, and the per-method attributes. `MapperDefaultsAttribute` derives from `MapperAttribute` and carries the same options. `MapperRequiredMappingAttribute`, `MapperIgnoreObsoleteMembersAttribute`, and `MapEnumAttribute` are the per-method level. Overrides replace the whole value, and a deviating mapper names its full `EnabledConversions` allowlist.

- Inbound mappers feeding constrained domain types never allow `ParseMethod`, `Constructor`, `StaticConvertMethods`, or casts in place of validation
- Outbound mappers enable `ImplicitCast` for the generated value-object-to-key operator, the inbound one is explicit and calls the throwing `Create`
- Named mappings selected with `Use` own every remaining conversion
- Mappers that add `ToStringMethod` treat it as formatting, not a wire contract, and pass a fixed provider or an explicit culture input
- Automatic member matching applies only when names and meanings agree, one exact pair has one intentional default, alternatives have unique names, generic mapping methods are disjoint, and no selection depends on declaration order

External mappings stay local to the mapper that consumes them. Assembly-wide registrations expose only disjoint pairs. Configuration inclusion needs identical direction, member meaning, null policy, and omissions, it copies configuration rather than implementation. Additional parameters carry immutable values that the boundary already resolved, and forward to nested user mappings and `Use` methods where remaining parameter names match. Type pairs shared by several mappers belong in one `internal static class` reached through `[UseStaticMapper<T>]`, a private `[UserMapping]` stays for a mapper-local pair.
-->

<!-- Integrated into .claude/skills/dotnet-mapperly/SKILL.md
## [03]-[CONSTRUCTION_AND_OWNERSHIP]

| [INDEX] | [FORM]                  | [TARGET_STATE]                            | [CONSTRAINT]                                                    |
| :-----: | :---------------------- | :---------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | New instance            | Mapper-owned until return                 | Constructor and init values precede writable assignments        |
|  [02]   | Mapperly object factory | Factory result, then writable assignments | Constructor parameters and init-only members are not mapped     |
|  [03]   | Existing target         | Caller-owned mutable value                | Assignments and collection additions are observable mutation    |
|  [04]   | Reference handling      | Identity graph under one handler          | Registration follows construction and precedes writable members |
|  [05]   | Runtime dispatch        | Registered source and target pairs        | Unregistered pair or subtype throws                             |

Prefer one total constructor for an immutable external record. `[MapperConstructor]` chooses between equivalent external constructors, it does not select a domain transition. Types where invariants depend on assignments after construction are not valid mapping targets. Mapperly cannot skip an init-only assignment to preserve its member initializer.

Mapperly object factories allocate or select the target and expose no typed failure channel. The factory is pure, deterministic, and synchronous. Nullable results fall back to a public parameterless construction or throw `NullReferenceException`. Factories do not resolve services, allocate domain identity, or call a rejecting domain factory. Object factories return a non-void type and take zero parameters or one parameter. Factories can be generic, with or without constraints, and the first factory with a matching signature wins. The Mapperly object factory is unrelated to the Thinktecture `[ObjectFactory<T>]`, which declares a validating conversion between two types.

Member and constructor visibility stays at `AllAccessible`. Unsafe accessors can call private constructors and write hidden members:
- They never construct or modify a constrained domain type
- Direct assignment can return the source reference, sharing is valid only when the complete reachable graph is immutable
- Deep cloning is an allocation strategy, not proof of ownership, validity, or a completed domain transition

Existing-target mappings mutate their target. Existing collections add without replacement:
- Lists add, queues enqueue, and stacks push the added segment in reverse source order
- Null source collections leave the target unchanged

Null-skipping implements merge behavior, not patch semantics. It cannot distinguish an omitted member from one cleared to null. Explicit optional wrappers can fold against `[MappingTargetOriginalValue]` at a mutable boundary, while constructor and init-only targets receive `default` as the original value.

Reference handling materializes external graphs that require cycles or shared identity. One handler serves one mapping call. Constructor and init-only edges run before registration, they cannot close a generated cycle. Existing-target roots start unregistered, register the pair before mapping when a back-reference must retain the supplied root. Domain identity uses explicit identifiers rather than mapper reference state.
-->

<!-- Integrated into .claude/skills/dotnet-mapperly/SKILL.md
## [04]-[DOMAIN_TYPE_INTEGRATION]

Raw input reaches a Thinktecture value object through a hand-written `From` adapter that maps `Validate` to `Fin<T>`. Smart enums take the same adapter shape over `Validate`, or `TryGet` for `Option<T>`. `Create`, `Parse`, accessible constructors, static conversion methods, and explicit operators can turn expected rejection into an exception.

Valid value objects or smart enums map outward through the key member or the `ToValue` of a declared `[ObjectFactory<T>]`. `ToString()` does not define that representation. Mapperly enum configuration applies only to CLR enums. Independent CLR enum contracts map by case-sensitive name or explicit value pairs, never by numeric position. One user mapping carries `ToValue` outward:

```csharp
[Mapper]
internal static partial class LineMapper {
    public static partial LineDto ToDto(Line line);

    [UserMapping]
    private static string MapUrn(FileUrn urn) => urn.ToValue();
}

internal sealed record Line(Guid Id, FileUrn Document);
internal sealed record LineDto(Guid Id, string Document);
```

Closed Thinktecture unions use the full generated `Switch` as the outer dispatcher. Mapperly maps one known case inside each arm, case selection stays exhaustive and member translation structural. `Map` takes one value per case and receives no mapper call.

```csharp
internal static EventDto ToDto(Event value) =>
    value.Switch(created: EventMapper.ToDto, cancelled: EventMapper.ToDto);
```

`MapDerivedType`, a duplicated case list, `SwitchPartially`, `MapPartially`, a `@default` arm, and a `StopAt` overload do not preserve closed-union exhaustiveness. Generic, runtime-target, and derived dispatch belong only to runtime-registered type sets where a mismatch is a defect.

LanguageExt owns absence, failure, validation, effects, traversal, and transformer stacks. Mapperly methods supply the function passed to `Map`, `BiMap`, `Apply`, or traversal, total over a validated source. The throw from `ThrowOnPropertyMappingNullMismatch` signals a defect, not an expected error. Automatic wrapper construction is unsafe:
- Constructor or cast discovery can manufacture a success case, unwrap a failure, or discard source elements
- Generic wrapper helpers need explicit `Use` selection and preserve every case

LanguageExt collections keep their own construction policy. Direct assignment shares an immutable collection only when sharing is intentional. Element-changing sequences use the collection's `Map`. Snapshot a mutable source before domain publication and materialize an arbitrary enumerable first. Build a map only after key validation defines ordering, uniqueness, and collision behavior. Incidental enumerable-tuple construction does not define those rules.
-->

<!-- Integrated into .claude/skills/dotnet-mapperly/SKILL.md
## [05]-[QUERY_PROJECTIONS]

Query projections are expression trees interpreted by a query provider. They live in the data adapter and return a read model or transport contract. Compose the generated projection into the query: `query.ProjectToDto().ToListAsync()`. Keep projection declarations separate from in-memory mappings when their conversion, null, or user-method policies differ. The projection method obtains member configuration from an element mapping with the same source and target pair. Projections with additional parameters read configuration from an element mapping where additional parameters match by name.

Mapperly must inline each user method, and the query provider must translate the resulting expression. Inlining needs an expression body, one return statement, or one local declaration followed by a return, any other shape reports `RMG068` and leaves the call in the expression. Additional parameters are immutable scalar query values. Services, mapper state, clocks, configuration objects, and request contexts do not enter the expression.

Nullable analysis and mapper property-null controls do not apply: a nullable path can become empty text, `default`, or a conditional fallback. The read model matches storage nullability. Object factories, existing-target mapping, dictionary mapping, deep cloning, and reference handling do not apply inside a projection, and reference handling reports `RMG029`. Unsupported enum configuration reports `RMG032` and emits a value cast.

Project stored values, materialize the query, then validate and construct domain values. The `Fin`-returning `From` adapters, LanguageExt composition, and effects run after materialization. `Match`, `RunSafe`, and `IfFail` stay at the host. Unmatched derived projections return `default(TTarget)`, which is valid only when the target contract states that result. `AsEnumerable` does not hide this boundary, a narrow query materializes before the in-memory pipeline begins.
-->

<!-- Integrated into .claude/skills/dotnet-mapperly/SKILL.md
## [06]-[MAPPING_METHODS]

Attributes on partial classes and methods carry the configuration. Declare them on a `[Mapper] partial class` or a `[Mapper] static partial class`.

```csharp
[Mapper]
internal static partial class OrderMapper {
    public static partial OrderDto MapToDto(Order order);
    public static partial IEnumerable<OrderDto> MapToDtos(IEnumerable<Order> orders);
}

internal sealed record Order(Guid Id, string CustomerName, decimal Total, DateTime CreatedAt, List<OrderItem> Items);
internal sealed record OrderItem(string ProductName, int Quantity, decimal Price);
internal sealed record OrderDto(Guid Id, string CustomerName, decimal Total, DateTime CreatedAt, List<OrderItemDto> Items);
internal sealed record OrderItemDto(string ProductName, int Quantity, decimal Price);
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

- Non-static mappers that declare any static mapping method must declare every mapping method static
- Static mapping methods satisfy a `static abstract` interface member, and a `[Mapper] partial class` can implement a mapping interface
- Existing-target methods return `void`
- The second parameter is the target unless `MappingTargetAttribute` names another parameter
- `MappingTargetAttribute` accepts the `this` parameter of an extension method

Generic or runtime-target-type methods dispatch to the mappings declared in the same mapper. Unknown pairs at run time throw `ArgumentException`. Neither form accepts additional parameters. Both accept `MapDerivedTypeAttribute` on the method itself.

Implement a member mapping with a non-partial method for the matching types. Under `AutoUserMappings = false`, a hand-written mapping needs `UserMappingAttribute` for its type pair, and Mapperly then uses it in place of an automatic conversion. `UserMappingAttribute` exposes `Default`, which marks the pair's one default mapping, and `Ignore`, which excludes a discovered method:

```csharp
[Mapper]
internal static partial class ProductMapper {
    public static partial ProductDto MapToDto(Product product);

    [UserMapping]
    private static string MapMoney(Money money) => string.Create(CultureInfo.InvariantCulture, $"{money.Amount:F2} {money.Currency}");

    [UserMapping]
    private static string MapDate(DateTimeOffset date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}

internal sealed record Product(Guid Id, string Name, Money Price, DateTimeOffset ListedAt);
internal sealed record Money(decimal Amount, string Currency);
internal sealed record ProductDto(Guid Id, string Name, string Price, string ListedAt);
```
-->

<!-- Integrated into .claude/skills/dotnet-mapperly/SKILL.md
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

- `Use` values and `IncludeMappingConfigurationAttribute` names both accept a reference outside the mapper
- `MapperIgnoreAttribute`, `MapperIgnoreSourceAttribute`, `MapperIgnoreTargetAttribute`, `MapperIgnoreSourceValueAttribute`, and `MapperIgnoreTargetValueAttribute` expose `Justification` as a `string?`
-->

<!-- Integrated into .claude/skills/dotnet-mapperly/SKILL.md
## [08]-[MAPPER_OPTIONS]

Settable properties of `MapperAttribute` and `MapperDefaultsAttribute`:

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

`RequiredEnumMappingStrategy` takes `RequiredMappingStrategy`, `EnabledConversions` takes `MappingConversionType`, and `IncludedMembers` and `IncludedConstructors` take `MemberVisibility`. Every other option takes `bool` or the type its name spells.

- Setting `AllowNullPropertyAssignment` to `false` turns an existing-target mapping into a merge
- The null options do not apply to a required init property
- `StackCloningStrategy` decides the element order whenever Mapperly builds a `Stack<T>` through `Stack<T>(IEnumerable<T>)`, in every new-instance mapping and never for an existing-target stack or a `Queue<T>`
- `PreserveOrder` emits a `Reverse` call and `ReverseOrder` the bare constructor, which reverses the sequence
-->

<!-- Integrated into .claude/skills/dotnet-mapperly/SKILL.md
## [09]-[MEMBER_CONFIGURATION]

Mapperly resolves a flattening (`Car.Make.Id` to `CarDto.MakeId`) from PascalCase names. It does not resolve unflattening, which needs `MapPropertyAttribute`. Mapperly ignores indexed members. `MapNestedPropertiesAttribute` brings every member below one path into scope, as if the source declared them. Immediate source members outrank nested ones, and automatic flattening outranks both. Two nested paths that reach the same target member have no defined order. Name that mapping with `MapPropertyAttribute`.

`MapPropertyAttribute` resolves name mismatches instead of renaming domain or DTO members. `MapperIgnoreSourceAttribute` and `MapperIgnoreTargetAttribute` silence the unmapped-member diagnostic for a computed field or another deliberate omission:

```csharp
[Mapper]
internal static partial class CustomerMapper {
    [MapProperty(nameof(Customer.FullName), nameof(CustomerDto.Name))]
    [MapProperty(nameof(Customer.EmailAddress), nameof(CustomerDto.Email))]
    [MapperIgnoreSource(nameof(Customer.PasswordHash), Justification = "Secret, never leaves the domain")]
    [MapperIgnoreTarget(nameof(CustomerDto.DisplayBadge), Justification = "Presentation computes it")]
    public static partial CustomerDto MapToDto(Customer customer);
}

internal sealed record Customer(Guid Id, string FullName, string EmailAddress, string PasswordHash, DateTime CreatedAt);
internal sealed record CustomerDto(Guid Id, string Name, string Email, DateTime CreatedAt) {
    public string? DisplayBadge { get; set; }
}
```

- Paired constructors take source before target
- `string` path arguments split on `.` into segments, the `string[]` overload takes the segments as written
- `@` works in a `MapPropertyAttribute` path, where `nameof(@MyNamespace.Car.Make.Id)` yields the path `Make.Id`
- `Source` and `Target` are `IReadOnlyCollection<string>`, `SourceFullName` and `TargetFullName` rejoin the segments with `.`
- `StringFormat` is the format string Mapperly passes to a `ToString` call on a type that implements `IFormattable`
- `FormatProvider` names a field or property marked `FormatProviderAttribute`
- Mapperly falls back to the one member that sets `Default` to `true`, and a mapper can set it on one member only
- `MapValueAttribute` assigns a constant of the target type, and with `Use` the result of a method returning that type, parameters filled by name from the additional mapping parameters

| [INDEX] | [CONSTRUCTOR]                                                                                       | [EXPOSES]                 |
| :-----: | :-------------------------------------------------------------------------------------------------- | :------------------------ |
|  [01]   | `MapPropertyAttribute(string source, string target)`                                                | `Source` `Target`         |
|  [02]   | `MapPropertyAttribute(string[] source, string target)`                                              | `Source` `Target`         |
|  [03]   | `MapPropertyAttribute(string source, string[] target)`                                              | `Source` `Target`         |
|  [04]   | `MapPropertyAttribute(string[] source, string[] target)`                                            | `Source` `Target`         |
|  [05]   | `MapPropertyFromSourceAttribute(string target)`                                                     | `Target`                  |
|  [06]   | `MapPropertyFromSourceAttribute(string[] target)`                                                   | `Target`                  |
|  [07]   | `MapNestedPropertiesAttribute(string source)`                                                       | `Source`                  |
|  [08]   | `MapNestedPropertiesAttribute(string[] source)`                                                     | `Source`                  |
|  [09]   | `MapValueAttribute(string target, object? value)`                                                   | `Target` `Value`          |
|  [10]   | `MapValueAttribute(string[] target, object? value)`                                                 | `Target` `Value`          |
|  [11]   | `MapValueAttribute(string target)`                                                                  | `Target`, needs `Use`     |
|  [12]   | `MapValueAttribute(string[] target)`                                                                | `Target`, needs `Use`     |
|  [13]   | `MapperIgnoreSourceAttribute(string source)`                                                        | `Source` as `string`      |
|  [14]   | `MapperIgnoreTargetAttribute(string target)`                                                        | `Target` as `string`      |
|  [15]   | `MapperRequiredMappingAttribute(RequiredMappingStrategy requiredMappingStrategy)`                   | `RequiredMappingStrategy` |
|  [16]   | `MapperIgnoreObsoleteMembersAttribute(IgnoreObsoleteMembersStrategy ignoreObsoleteStrategy = Both)` | `IgnoreObsoleteStrategy`  |

Settable properties past the constructor:
- `MapPropertyAttribute`: `StringFormat`, `FormatProvider`, `Use`, `SuppressNullMismatchDiagnostic`
- `MapPropertyFromSourceAttribute`: `StringFormat`, `FormatProvider`, `Use`
- `MapValueAttribute`: `Use`
- `FormatProviderAttribute`: `Default`
-->

<!-- Integrated into .claude/skills/dotnet-mapperly/SKILL.md
## [10]-[ENUM_CONFIGURATION]

Attributes that configure one enum mapping:

| [INDEX] | [SYMBOL]                                              | [MEMBERS]                     |
| :-----: | :---------------------------------------------------- | :---------------------------- |
|  [01]   | `MapEnumAttribute(EnumMappingStrategy strategy)`      | `Strategy`                    |
|  [02]   | `MapEnumValueAttribute(object source, object target)` | `Source` `Target` as `object` |
|  [03]   | `MapperIgnoreSourceValueAttribute(object source)`     | `SourceValue` as `Enum?`      |
|  [04]   | `MapperIgnoreTargetValueAttribute(object target)`     | `TargetValue` as `Enum?`      |

`MapEnumAttribute` applies to a mapping method that takes an enum. It also takes `IgnoreCase` (`bool`, default `false`), `FallbackValue` (`object?`, default `null`), and `NamingStrategy` (`EnumNamingStrategy`, default `MemberName`). `FallbackValue` replaces the throw for an unmapped value, and works with `ByName` and `ByValueCheckDefined` only. Neither `FallbackValue` nor `MapEnumValueAttribute` passes through the naming strategy.

`MapEnumValueAttribute` applies to an enum-to-enum, enum-to-string, or string-to-enum mapping. It takes `object` for both arguments and pairs an enum member with another enum member or a string literal. `MapperIgnoreSourceValueAttribute` and `MapperIgnoreTargetValueAttribute` cast their argument to `Enum` and take an enum member only.

Under the workspace `ByName` default, members that match by name need no configuration, and explicit pairs cover the rest:

```csharp
[Mapper]
internal static partial class StatusMapper {
    [MapEnumValue(InternalStatus.InProgress, ExternalStatus.Active)]
    [MapEnumValue(InternalStatus.Done, ExternalStatus.Completed)]
    public static partial ExternalStatus MapStatus(InternalStatus status);
}

internal enum InternalStatus { Draft, InProgress, Done, Cancelled }
internal enum ExternalStatus { Active, Completed, Cancelled, Draft }
```
-->

<!-- Integrated into .claude/skills/dotnet-mapperly/SKILL.md
## [11]-[STRATEGY_TYPES]

The options and attributes above name these strategy types. `RequiredMappingStrategy`, `IgnoreObsoleteMembersStrategy`, and `MemberVisibility` carry `[Flags]`: `|` adds a member and `& ~` removes one.

- `StackCloningStrategy`: `PreserveOrder` `ReverseOrder`
- `PropertyNameMappingStrategy`: `CaseSensitive` `CaseInsensitive` `SnakeCase` `UpperSnakeCase`
- `RequiredMappingStrategy`: `None = 0` `Both = ~None` `Source = 1 << 0` `Target = 1 << 1`
- `IgnoreObsoleteMembersStrategy`: `None = 0` `Both = ~None` `Source = 1 << 0` `Target = 1 << 1`
- `EnumMappingStrategy`: `ByValue` `ByName` `ByValueCheckDefined`
- `EnumNamingStrategy`: `MemberName` `CamelCase` `PascalCase` `SnakeCase` `UpperSnakeCase` `KebabCase` `UpperKebabCase` `ComponentModelDescriptionAttribute` `SerializationEnumMemberAttribute`
- `MemberVisibility`: `AllAccessible = All | Accessible` `All = Public | Internal | Protected | Private` `Accessible = 1 << 0` `Public = 1 << 1` `Internal = 1 << 2` `Protected = 1 << 3` `Private = 1 << 4`

`ByValueCheckDefined` maps by value and checks that the value is defined in the enum. `ComponentModelDescriptionAttribute` reads `DescriptionAttribute.Description`, and `SerializationEnumMemberAttribute` reads `EnumMemberAttribute.Value`. Both fall back to the member name when the attribute is absent.

`Source` and `Target` name the side each strategy acts on: `RequiredMappingStrategy` warns about unmapped members there, and `IgnoreObsoleteMembersStrategy` skips obsolete ones. `MapPropertyAttribute` maps an obsolete member whatever the strategy says.
-->

<!-- Integrated into .claude/skills/dotnet-mapperly/SKILL.md
## [12]-[CONVERSIONS]

`MappingConversionType` is the `[Flags]` set that `EnabledConversions` takes. `None = 0` disables every automatic conversion and `All = ~None` enables every one. `EnabledConversions` holds `Default = All & ~ExplicitCast` when unset, an explicit cast operator converts nothing until the mapper names `All` or adds the bit.

Priority 1 is direct assignment, which applies when the source type is assignable to the target type and `UseDeepCloning` is `false`. Priority 20 creates a new target instance and maps its members.

| [INDEX] | [MEMBER]               | [BIT]     | [PRIORITY] | [CONDITION]                                                  |
| :-----: | :--------------------- | :-------- | :--------: | :----------------------------------------------------------- |
|  [01]   | `Constructor`          | `1 << 0`  |     10     | Target has a constructor taking the source type              |
|  [02]   | `ImplicitCast`         | `1 << 1`  |     8      | Implicit cast operator exists                                |
|  [03]   | `ExplicitCast`         | `1 << 2`  |     14     | Explicit cast operator exists                                |
|  [04]   | `ParseMethod`          | `1 << 3`  |     9      | Source is `string`, target has a static `Parse` overload     |
|  [05]   | `ToStringMethod`       | `1 << 4`  |     15     | Target is `string`, calls `ToString` on the source           |
|  [06]   | `StringToEnum`         | `1 << 5`  |     11     | Source is `string`, target is an enum                        |
|  [07]   | `EnumToString`         | `1 << 6`  |     12     | Source is an enum, target is `string`                        |
|  [08]   | `EnumToEnum`           | `1 << 7`  |     13     | Both are enums, follows `EnumMappingStrategy`                |
|  [09]   | `DateTimeToDateOnly`   | `1 << 8`  |     17     | `DateTime` to `DateOnly` through `FromDateTime`              |
|  [10]   | `DateTimeToTimeOnly`   | `1 << 9`  |     18     | `DateTime` to `TimeOnly` through `FromDateTime`              |
|  [11]   | `Queryable`            | `1 << 10` |     2      | Both are `IQueryable<T>`, expression-tree element mapping    |
|  [12]   | `Enumerable`           | `1 << 11` |     4      | Both are `IEnumerable<T>`, maps each element                 |
|  [13]   | `Dictionary`           | `1 << 12` |     3      | Both are `IDictionary` or `IReadOnlyDictionary`              |
|  [14]   | `Span`                 | `1 << 13` |     5      | Either is `Span<T>` or `ReadOnlySpan<T>`                     |
|  [15]   | `Memory`               | `1 << 14` |     7      | Either is `Memory<T>` or `ReadOnlyMemory<T>`                 |
|  [16]   | `Tuple`                | `1 << 15` |     6      | Target is a `ValueTuple` or a tuple expression               |
|  [17]   | `EnumUnderlyingType`   | `1 << 16` |     -      | Maps an enum from or to its underlying type                  |
|  [18]   | `ToTargetMethod`       | `1 << 17` |     16     | Source has an instance `TTarget ToTTarget()`, not `ToString` |
|  [19]   | `StaticConvertMethods` | `1 << 18` |     19     | Static `ToTTarget`, `Create`, `CreateFrom`, `FromTSource`    |
|  [20]   | `Expression`           | `1 << 19` |     -      | Declared only, the generator never reads it                  |

`StaticConvertMethods` finds a static `ToTTarget` on the source type. On the target type it finds `Create`, `CreateFrom`, `CreateFromTSource`, and `FromTSource` in any casing. Array-typed sources add the `From<TElement>Array` and `CreateFrom<TElement>Array` spellings. The list matches the generated `Create(TKey)` of a Thinktecture value object, and the `Fin`-returning `From` adapter matches no listed name. It excludes the `DateTime` conversions. `Tuple` emits a tuple expression outside a queryable projection, and `ValueTuple` inside one. The `Enumerable` and `Dictionary` bits also cover arrays and every constructible collection target. `EnumUnderlyingType` holds no rank in the priority list.

`MapDerivedTypeAttribute(Type sourceType, Type targetType)` and `MapDerivedTypeAttribute<TSource, TTarget>` register one pair for a base-type or interface mapping. Every source type must extend or implement the parameter type. Every target type must extend or implement the return type. Each source type appears once, and several source types can share one target type. Ordinary mappings emit a runtime switch and throw `ArgumentException` for an unregistered type. `IQueryable` and expression mappings emit `default(TTarget)` for the unmatched branch. Derived types work for a new-instance mapping and for an existing-target mapping.
-->

<!-- Integrated into .claude/skills/dotnet-mapperly/SKILL.md
## [13]-[REFERENCE_HANDLING]

`Riok.Mapperly.Abstractions.ReferenceHandling` supports source graphs with circular references. `UseReferenceHandling = true` turns it on. Reference handling uses the package's runtime assets, keep `runtime` out of `ExcludeAssets` on the package reference.

| [INDEX] | [SYMBOL]                    | [DECLARATION] | [CAPABILITY]                       |
| :-----: | :-------------------------- | :------------ | :--------------------------------- |
|  [01]   | `IReferenceHandler`         | interface     | Stores and resolves target objects |
|  [02]   | `PreserveReferenceHandler`  | sealed class  | The default handler                |
|  [03]   | `ReferenceHandlerAttribute` | attribute     | Marks the handler parameter        |

```csharp
bool TryGetReference<TSource, TTarget>(TSource source, [NotNullWhen(true)] out TTarget? target)
    where TSource : notnull where TTarget : notnull;

void SetReference<TSource, TTarget>(TSource source, TTarget target)
    where TSource : notnull where TTarget : notnull;
```

- Mapperly calls `TryGetReference` before it creates a target
- `true` results must set `target`, and Mapperly uses that instance
- `false` results make Mapperly create a new instance and record it through `SetReference`
- `PreserveReferenceHandler` returns the same target instance for the same source instance

To supply another handler, add a parameter of type `IReferenceHandler` marked `ReferenceHandlerAttribute`. Hand-written mapping methods take the same parameter to join the same handler. `ReferenceHandlerAttribute` sits in `Riok.Mapperly.Abstractions.ReferenceHandling`. Methods that take a handler need a second `using` directive.
-->

<!-- Integrated into .claude/skills/dotnet-mapperly/SKILL.md
## [14]-[DESIGN_RULES]

| [INDEX] | [WRONG_FORM]                                                        | [CORRECT_FORM]                                             |
| :-----: | :------------------------------------------------------------------ | :--------------------------------------------------------- |
|  [01]   | Mapper conversion that calls `Create` or `Parse` on a domain type   | The hand-written `From` adapter over `Validate`            |
|  [02]   | `MapDerivedType`, `SwitchPartially`, or a `@default` arm on a union | The generated exhaustive `Switch`, one mapper call per arm |
|  [03]   | `EnabledConversions` on a mapper naming the one added bit           | The whole allowlist, the value replaces and never merges   |
|  [04]   | Mapper method that returns `Fin<T>` or unwraps one                  | Mapper over the success value, `Map` keeps the context     |
|  [05]   | `ToString()` as the wire contract of a value object                 | The key member, or `ToValue` of an `[ObjectFactory<T>]`    |
|  [06]   | Validation or effects inside a query projection                     | Project, materialize, then validate and construct          |
|  [07]   | Existing-target mapping over a value the caller published           | New-instance mapping, or a target that never escapes       |
|  [08]   | Private `[UserMapping]` repeated in every mapper that needs it      | One `internal static class` behind `[UseStaticMapper<T>]`  |
-->
