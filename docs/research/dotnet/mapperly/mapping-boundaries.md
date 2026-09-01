# [MAPPING_BOUNDARIES]

`Riok.Mapperly` is a compile-time object mapper uses source generators to produce mapping code at build time. The generated code is equivalent to hand-written property assignments, with no runtime reflection, no expression tree compilation, and no hidden allocations. Use Mapperly for performance-critical paths, DTOs, domain models, AOT (ahead-of-time) compilation scenarios, and where compile-time validation of mappings is needed.

Mapperly cannot consume another source generator's output from the same compilation. Referenced assemblies expose accessible generated members as metadata, automatic conversion can change when a generated type moves between projects. Exact representation mappings prevent project topology from choosing semantics.

Best Practices:
- Define one mapper class per aggregate or feature area to keep mapping logic organized and discoverable
- Use `[MapProperty]` for name mismatches instead of renaming domain or DTO properties, preserving the natural naming of each layer
- Use `[MapperIgnoreSource]` and `[MapperIgnoreTarget]` to explicitly suppress warnings for properties that should not be mapped like computed fields
- Use `[MapEnum(EnumMappingStrategy.ByName)]` when source and destination enums have the same member names but different underlying values to avoid silent data corruption
- Enable `ThrowOnMappingNullMismatch` in the `[Mapper]` attribute during development to surface null-safety issues
- Provide custom non-partial methods for complex value transformations and Mapperly will automatically use them for matching types
- Review the generated source code in `obj/` to verify that Mapperly produces the expected assignments

## [01]-[BOUNDARIES]

Adapters that reference both representations owns the mapper, the domain does NOT reference Mapperly or an external contract:
- Keep mapper types internal unless mapping is an intentional public contract
- Prefer a static partial mapper for transformations that need no stored inputs
- An instance mapper remains deterministic when its stored collaborators are immutable and pure, keep mutable state, service location, and ambient reads outside every mapper

| [INDEX] | [DIRECTION]                                       | [MAPPERLY_ROLE]                  | [REQUIRED_FORM]                                                |
| :-----: | :------------------------------------------------ | :------------------------------- | :------------------------------------------------------------- |
|  [01]   | External contract to raw input model              | Structural mapping               | Validate the raw model before domain construction              |
|  [02]   | External contract to constrained domain value     | None                             | Call the domain `Validate` adapter and retain its typed error  |
|  [03]   | Validated components to domain aggregate          | Optional final assembly          | Use only a constructor that is total for those component types |
|  [04]   | Domain value to transport or persistence contract | Structural projection            | Map only after the domain result is successful                 |
|  [05]   | Domain snapshot to next domain snapshot           | None                             | Call a named transition that returns the complete next value   |
|  [06]   | Closed union to external case                     | Case-specific structural mapping | Dispatch through the union's exhaustive `Switch` or `Map`      |
|  [07]   | Mutable boundary value to owned mutable target    | Existing-target mapping          | Confine mutation to the scope that owns the target             |
|  [08]   | Persistence query to read model                   | Expression projection            | Materialize before domain construction or effects              |

Mappings that can reject input are not plain `TSource -> TTarget` functions, validation owns rejection and its error type. Map a successful value inside its existing context:

```csharp
internal static Fin<OrderDto> ToDto(Fin<Order> value) => value.Map(OrderMapper.ToDto);
```

Apply the same rule to `Option`, `Either`, `Validation`, `IO`, `Eff`, transformers, and higher-kinded contexts. Do NOT make Mapperly select or rebuild their cases.

## [02]-[MAPPER_CONFIGURATION]

The assembly default closes every implicit selection path that can change meaning. Each mapper then admits only the conversions required by its representation contract. Direct assignment and object-member mapping remain available when `EnabledConversions` is `None`.

```csharp
[assembly: MapperDefaults(
    PropertyNameMappingStrategy = PropertyNameMappingStrategy.CaseSensitive,
    EnumMappingStrategy = EnumMappingStrategy.ByName,
    EnumMappingIgnoreCase = false,
    RequiredMappingStrategy = RequiredMappingStrategy.Both,
    RequiredEnumMappingStrategy = RequiredMappingStrategy.Both,
    ThrowOnMappingNullMismatch = true,
    ThrowOnPropertyMappingNullMismatch = true,
    AllowNullPropertyAssignment = true,
    AutoUserMappings = false,
    IncludedMembers = MemberVisibility.AllAccessible,
    IncludedConstructors = MemberVisibility.AllAccessible,
    EnabledConversions = MappingConversionType.None)]
```

```csharp
[Mapper(
    EnumMappingStrategy = EnumMappingStrategy.ByName,
    EnumMappingIgnoreCase = true,
    ThrowOnMappingNullMismatch = true,
    ThrowOnPropertyMappingNullMismatch = false,
    UseDeepCloning = false)]
public partial class StrictMapper
{
    public partial OrderDto MapOrder(Order order);
}
```

- An inbound invariant-sensitive mapper never admits `ParseMethod`, `Constructor`, `StaticConvertMethods`, or casts as a substitute for validation
- An outbound mapper admits `ImplicitCast` only for a canonical key whose valid source makes the conversion total
- Named mappings selected with `Use` own every other semantic conversion
- `ToStringMethod` is formatting, not a wire contract, and uses a fixed provider or an explicit culture input
- Automatic member matching applies only when names and meanings agree, One exact pair has one intentional default, alternatives have unique names, generic templates are disjoint, and no selection depends on declaration order

External mappings stay local to the mapper that consumes them. Assembly-wide registrations expose only disjoint pairs. Configuration inclusion applies only when direction, member meaning, null policy, and omissions are identical, inclusion copies configuration rather than implementation. Additional parameters carry immutable values that the boundary already resolved.

## [03]-[CONSTRUCTION_AND_OWNERSHIP]

| [FORM]             | [TARGET_STATE]                                  | [CONSTRAINT]                                                    |
| :----------------- | :---------------------------------------------- | :-------------------------------------------------------------- |
| New instance       | Mapper-owned until return                       | Constructor and init values precede writable assignments        |
| Object factory     | Factory result followed by writable assignments | Constructor parameters and init-only members are not mapped     |
| Existing target    | Caller-owned mutable value                      | Assignments and collection additions are observable mutation    |
| Reference handling | Identity graph under one handler                | Registration follows construction and precedes writable members |
| Runtime dispatch   | Registered source and target pairs              | Unregistered pair or subtype throws                             |

Prefer one total constructor for an immutable external record. `[MapperConstructor]` chooses between semantically equivalent external constructors, it does not select a domain transition. Types whose invariants depend on later setter repair are not valid mapping targets. An init-only assignment cannot be skipped to preserve its member initializer.

Mapperly object factories allocate or select a direct target and expose no typed failure channel. The factory is pure, deterministic, synchronous, and non-null. Nullable results fall back to a public parameterless construction or throw `NullReferenceException`. Factories do not resolve services, allocate domain identity, or call a rejecting domain factory.

Member and constructor visibility stays at `AllAccessible`. Unsafe accessors can use private constructors and write hidden members, it does NOT grant invariant ownership:
- They never construct or modify a constrained domain type
- Direct assignment can return the source reference, sharing is valid only when the complete reachable graph is immutable
- Deep cloning is an allocation strategy, not proof of ownership, validity, or transition semantics

An existing-target mapping mutates its argument. Existing collections add without replacement:
- Lists add, queues enqueue, and stacks push the added segment in reverse source order
- Null source collections leave the target unchanged

Null-skipping implements merge behavior, not patch presence. It cannot distinguish omission from clearing. An explicit presence type can fold against `[MappingTargetOriginalValue]` at a mutable boundary, while constructor and init-only targets receive `default` as the original value.

Reference handling materializes external graphs that require cycles or shared identity. One handler belongs to one graph operation. Constructor and init-only edges run before registration, they cannot close a generated cycle. An existing-target root is not registered automatically, register the pair before mapping when a back-reference must retain the supplied root. Domain identity uses explicit identifiers rather than mapper reference state.

## [04]-[DOMAIN_TYPE_INTEGRATION]

Raw input reaches a Thinktecture value object through `Validate` and a typed adapter. Raw keys reach a Smart Enum through `Validate` or `TryGet`. `Create`, `Parse`, accessible constructors, static conversion methods, and explicit operators can turn expected rejection into an exception.

Valid value objects or Smart Enums map outward through the canonical key or declared representation. `ToString()` does not define that representation. Mapperly enum configuration applies only to CLR enums. Independent CLR enum contracts map by case-sensitive name or explicit value pairs, never by numeric position.

Closed Thinktecture unions use the full generated `Switch` or `Map` as the outer dispatcher. Mapperly maps one known case inside each, this keeps case selection exhaustive and member translation structural.

```csharp
internal static EventDto ToDto(Event value) =>
    value.Switch(
        created: EventMapper.ToDto,
        cancelled: EventMapper.ToDto);
```

`MapDerivedType`, a duplicated case list, a partial match, or fallback do not preserve closed-union exhaustiveness. Generated generic, runtime-target, and ordinary derived dispatch throw for an unregistered pair or subtype. Those forms belong only to controlled runtime sets where a mismatch is a defect.

LanguageExt owns absence, failure, validation, effects, traversal, and transformer topology. Mapperly methods supply the total leaf function passed to `Map`, `BiMap`, `Apply`, or traversal. Automatic wrapper construction is unsafe:
- Constructor or cast discovery can manufacture a success case, unwrap a failure, or discard source elements
- Generic wrapper helpers need explicit `Use` selection and preserve every case

LanguageExt collections keep their own construction policy. Direct assignment shares an immutable collection only when sharing is intentional. An element-changing sequence uses the collection's `Map`; a mutable source is snapshotted before domain publication, and an arbitrary enumerable is forced first. Build a map only after key validation defines ordering, uniqueness, and collision behavior. Incidental enumerable-tuple construction does not define those rules.

## [05]-[QUERY_PROJECTIONS]

1. Define a [Mapper] class with a ProjectToXxx method that takes IQueryable<TSource> and returns IQueryable<TTarget>
2. Add a private partial mapping method with [MapProperty] to handle name mismatches and nested properties
3. Use the generated projection in services: query.ProjectToXxx().ToListAsync()

Query projections are expression trees interpreted by a query provider. They live in the data adapter and return a read model or transport contract. Keep projection declarations separate from in-memory mappings when their conversion, null, or user-method policies differ. The projection method obtains member configuration from an element mapping with the same source and target pair.

Mapperly must inline each user method, the selected provider must translate the resulting expression. Additional parameters are immutable scalar query values. Services, mapper state, clocks, configuration objects, and request contexts do not enter the expression.

Projection null behavior is not in-memory null behavior. Nullable analysis and mapper property-null controls do not apply, a nullable path can become empty text, `default`, or a conditional fallback. The read model matches storage nullability. Object factories, existing-target mapping, deep cloning, and reference handling do not supply projection semantics. Unsupported enum behavior remains outside the query expression.

Project stored values, materialize the query, and then start validation and domain construction. Thinktecture factories, LanguageExt composition, result selection, and effects run after materialization. An unmatched derived projection returns `default(TTarget)`, derived projection is valid only when that result is explicit in the target contract. `AsEnumerable` does not hide this boundary; a deliberately narrow query materializes before the in-memory pipeline begins.

## [BASIC_MAPPER]

Define a partial class with the `[Mapper]` attribute. Declare partial methods for each mapping:

```csharp
[Mapper]
public partial class OrderMapper
{
    public partial OrderDto MapToDto(Order order);
    public partial Order MapToEntity(CreateOrderRequest request);
    public partial IEnumerable<OrderDto> MapToDtos(IEnumerable<Order> orders);
}

public record Order(Guid Id, string CustomerName, decimal Total, DateTime CreatedAt, List<OrderItem> Items);
public record OrderItem(string ProductName, int Quantity, decimal Price);
public record OrderDto(Guid Id, string CustomerName, decimal Total, DateTime CreatedAt, List<OrderItemDto> Items);
public record OrderItemDto(string ProductName, int Quantity, decimal Price);
public record CreateOrderRequest(string CustomerName, decimal Total, List<OrderItemDto> Items);
```

## [CUSTOM_PROPERTY_MAPPING]

Use `[MapProperty]` to map between properties with different names:

```csharp
[Mapper]
public partial class CustomerMapper
{
    [MapProperty(nameof(Customer.FullName), nameof(CustomerDto.Name))]
    [MapProperty(
        nameof(Customer.EmailAddress),
        nameof(CustomerDto.Email))]
    public partial CustomerDto MapToDto(Customer customer);

    [MapperIgnoreSource(nameof(Customer.PasswordHash))]
    [MapperIgnoreTarget(nameof(CustomerDto.DisplayBadge))]
    public partial CustomerDto MapToPublicDto(Customer customer);
}

public record Customer(Guid Id, string FullName, string EmailAddress, string PasswordHash, DateTime CreatedAt);
public record CustomerDto(Guid Id, string Name, string Email, DateTime CreatedAt, string? DisplayBadge);
```

## [CUSTOM_MAPPING_METHODS]

Implement specific member mappings by providing a non-partial method for matching types:

```csharp
[Mapper]
public partial class ProductMapper
{
    public partial ProductDto MapToDto(Product product);
    private string MapMoney(Money money) => $"{money.Amount:F2} {money.Currency}";
    private string MapDate(DateTimeOffset date) => date.ToString("yyyy-MM-dd");

    [MapEnum(EnumMappingStrategy.ByName)]  // Enum mapping with explicit values
    private partial ProductCategoryDto MapCategory(ProductCategory category);
}

public record Product(Guid Id, string Name, Money Price, DateTimeOffset ListedAt, ProductCategory Category);
public record Money(decimal Amount, string Currency);
public record ProductDto(Guid Id, string Name, string Price, string ListedAt, ProductCategoryDto Category);
public enum ProductCategory { Electronics, Clothing, Food }
public enum ProductCategoryDto { Electronics, Clothing, Food }
```

## [ENUM_MAPPING]

```csharp
[Mapper]
public partial class StatusMapper
{
    [MapEnum(EnumMappingStrategy.ByName)]
    public partial ExternalStatus MapStatus(InternalStatus status);

    // Explicit enum value mapping
    [MapEnumValue(InternalStatus.InProgress, ExternalStatus.Active)]
    [MapEnumValue(InternalStatus.Done, ExternalStatus.Completed)]
    public partial ExternalStatus MapStatusExplicit(InternalStatus status);
}

public enum InternalStatus { Draft, InProgress, Done, Cancelled }
public enum ExternalStatus { Draft, Active, Completed, Cancelled }
```

## [NULLABLE_AND_COLLECTION_HANDLING]

Mapperly handles nullable types, collections, and dictionaries automatically:

```csharp

[Mapper]
public partial class InventoryMapper
{
    public partial WarehouseDto? MapWarehouse(Warehouse? warehouse);
    public partial List<ItemDto> MapItems(List<Item> items);
    public partial Dictionary<string, ItemDto> MapInventory(Dictionary<string, Item> inventory);  // Dictionary mapping
    public partial ItemDto[] MapItemArray(Item[] items);                                          // Array mapping
}

public record Warehouse(string Name, string Location);
public record WarehouseDto(string Name, string Location);
public record Item(string Sku, string Name, int Quantity);
public record ItemDto(string Sku, string Name, int Quantity);
```
## [02]-[MAPPING_METHODS]

Configuration is done through attributes on partial classes and methods. Declare them on a `[Mapper] partial class` or a `[Mapper] static partial class`. If a property cannot be mapped (missing or incompatible types), the compiler produces a warning/error, catching mistakes during the build rather than at runtime. Use the `partial` method declarations the generator implements.

| [INDEX] | [DECLARATION]                                                              | [CAPABILITY]                     |
| :-----: | :------------------------------------------------------------------------- | :------------------------------- |
|  [01]   | `partial TTarget Map(TSource source)`                                      | new target instance              |
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

- Non-static mappers that declare one static mapping method must declare every mapping method static
- Static mapping methods satisfy a `static abstract` interface member, `[Mapper] partial class` may implement a mapping interface
- An existing-target method returns `void`
- The second parameter is the target unless `MappingTargetAttribute` names another parameter
- `MappingTargetAttribute` accepts the `this` parameter of an extension method

Generic or runtime-target-type methods dispatch to the mappings declared in the same mapper. An unknown pair at run time throws `ArgumentException`. Neither form accepts additional parameters. Both accept `MapDerivedTypeAttribute` on the method itself.

## [03]-[ATTRIBUTES]

`MappingTargetOriginalValueAttribute` marks a parameter that receives the target member's current value, a hand-written mapping can fold the new value into the old one, `[MULTIPLE]` states whether the attribute sets `AllowMultiple`:

| [INDEX] | [SYMBOL]                                   | [TARGET]                | [MULTIPLE] | [CAPABILITY]                         |
| :-----: | :----------------------------------------- | :---------------------- | :--------: | :----------------------------------- |
|  [01]   | `MapperAttribute`                          | class                   |     No     | Mapper declaration and options       |
|  [02]   | `MapperDefaultsAttribute`                  | assembly                |     No     | Same options for every mapper        |
|  [03]   | `UseMapperAttribute`                       | field, property         |     No     | Use the member's mapping methods     |
|  [04]   | `UseStaticMapperAttribute`                 | class, assembly         |    Yes     | Use a type's static mapping methods  |
|  [05]   | `UseStaticMapperAttribute<T>`              | class, assembly         |    Yes     | Same, generic form                   |
|  [06]   | `MapperConstructorAttribute`               | constructor             |     No     | Select the constructor to call       |
|  [07]   | `ObjectFactoryAttribute`                   | method                  |     No     | Construct or resolve the target      |
|  [08]   | `MapperIgnoreAttribute`                    | property, field, method |     No     | Exclude a member or a method         |
|  [09]   | `UserMappingAttribute`                     | method                  |     No     | User-implemented mapping method      |
|  [10]   | `NamedMappingAttribute`                    | method                  |     No     | Name a mapping for `Use`             |
|  [11]   | `IncludeMappingConfigurationAttribute`     | method                  |    Yes     | Reuse another method's configuration |
|  [12]   | `MappingTargetAttribute`                   | parameter               |     No     | Mark the parameter as the target     |
|  [13]   | `MappingTargetOriginalValueAttribute`      | parameter               |     No     | Pass the target member's prior value |
|  [14]   | `FormatProviderAttribute`                  | field, property         |     No     | Expose an `IFormatProvider`          |
|  [15]   | `ReferenceHandlerAttribute`                | parameter               |     No     | Mark the reference-handler parameter |
|  [16]   | `MapPropertyAttribute`                     | method                  |    Yes     | Rename, flatten, and unflatten       |
|  [17]   | `MapPropertyFromSourceAttribute`           | method                  |    Yes     | Map the source object to a member    |
|  [18]   | `MapNestedPropertiesAttribute`             | method                  |    Yes     | Flatten every member of a path       |
|  [19]   | `MapValueAttribute`                        | method                  |    Yes     | Assign a constant or generated value |
|  [20]   | `MapperIgnoreSourceAttribute`              | method                  |    Yes     | Exclude a source member              |
|  [21]   | `MapperIgnoreTargetAttribute`              | method                  |    Yes     | Exclude a target member              |
|  [22]   | `MapperRequiredMappingAttribute`           | method                  |     No     | Unmapped-member diagnostics          |
|  [23]   | `MapperIgnoreObsoleteMembersAttribute`     | method                  |     No     | Obsolete-member policy               |
|  [24]   | `MapDerivedTypeAttribute`                  | method                  |    Yes     | One derived source and target pair   |
|  [25]   | `MapDerivedTypeAttribute<TSource,TTarget>` | method                  |    Yes     | Same, generic form                   |
|  [26]   | `MapEnumAttribute`                         | method                  |     No     | enum strategy for one mapping        |
|  [27]   | `MapEnumValueAttribute`                    | method                  |    Yes     | Pair two enum members                |
|  [28]   | `MapperIgnoreSourceValueAttribute`         | method                  |    Yes     | Exclude a source enum value          |
|  [29]   | `MapperIgnoreTargetValueAttribute`         | method                  |    Yes     | Exclude a target enum value          |

- `MapperDefaultsAttribute` derives from `MapperAttribute`, it carries the same options
- `MapperAttribute` overrides it, and `MapperRequiredMappingAttribute` and `MapperIgnoreObsoleteMembersAttribute` override both for one method
- `Use` values and `IncludeMappingConfigurationAttribute` names both accept a reference outside the mapper
- `@` also works in a `MapPropertyAttribute` path, where `nameof(@MyNamespace.Car.Make.Id)` yields the path `Make.Id`
- `MapperIgnoreAttribute`, `MapperIgnoreSourceAttribute`, `MapperIgnoreTargetAttribute`, `MapperIgnoreSourceValueAttribute`, and `MapperIgnoreTargetValueAttribute` expose `Justification` as a `string?`

`ObjectFactoryAttribute` marks a method that returns a non-void type and takes zero parameters or one parameter. Factories may be generic, with or without constraints, the first factory with a matching signature forms. It cannot map to an init-only property or a constructor parameter of a type an object factory builds.

## [04]-[MAPPER_OPTIONS]

The settable properties of `MapperAttribute`, and of `MapperDefaultsAttribute`.

| [INDEX] | [PROPERTY]                           | [TYPE]                          | [DEFAULT]       | [EFFECT]                       |
| :-----: | :----------------------------------- | :------------------------------ | :-------------- | :----------------------------- |
|  [01]   | `PropertyNameMappingStrategy`        | `PropertyNameMappingStrategy`   | `CaseSensitive` | Member-name matching           |
|  [02]   | `EnumMappingStrategy`                | `EnumMappingStrategy`           | `ByValue`       | Enum-member matching           |
|  [03]   | `EnumNamingStrategy`                 | `EnumNamingStrategy`            | `MemberName`    | Enum-to-string naming          |
|  [04]   | `EnumMappingIgnoreCase`              | `bool`                          | `false`         | Enum-match casing              |
|  [05]   | `ThrowOnMappingNullMismatch`         | `bool`                          | `true`          | null return, non-null result   |
|  [06]   | `ThrowOnPropertyMappingNullMismatch` | `bool`                          | `false`         | null source, non-null member   |
|  [07]   | `AllowNullPropertyAssignment`        | `bool`                          | `true`          | Assign null to a nullable one  |
|  [08]   | `UseDeepCloning`                     | `bool`                          | `false`         | Copy instead of reuse          |
|  [09]   | `StackCloningStrategy`               | `StackCloningStrategy`          | `PreserveOrder` | Element order of a built stack |
|  [10]   | `EnabledConversions`                 | `MappingConversionType`         | `Default`       | Admitted conversions           |
|  [11]   | `UseReferenceHandling`               | `bool`                          | `false`         | Circular-reference support     |
|  [12]   | `IgnoreObsoleteMembersStrategy`      | `IgnoreObsoleteMembersStrategy` | `None`          | Obsolete-member policy         |
|  [13]   | `RequiredMappingStrategy`            | `RequiredMappingStrategy`       | `Both`          | Unmapped-member diagnostics    |
|  [14]   | `RequiredEnumMappingStrategy`        | `RequiredMappingStrategy`       | `Both`          | Unmapped-value diagnostics     |
|  [15]   | `IncludedMembers`                    | `MemberVisibility`              | `AllAccessible` | Mapped member accessibility    |
|  [16]   | `IncludedConstructors`               | `MemberVisibility`              | `AllAccessible` | Constructor accessibility      |
|  [17]   | `PreferParameterlessConstructors`    | `bool`                          | `true`          | Constructor order              |
|  [18]   | `AutoUserMappings`                   | `bool`                          | `true`          | Discovery by signature         |

- Setting `AllowNullPropertyAssignment` to `false` turns an existing-target mapping into a merge
- The null options do not apply to a required init property or to an `IQueryable<T>` projection
- `StackCloningStrategy` decides the element order whenever Mapperly builds a `Stack<T>` through `Stack<T>(IEnumerable<T>)`, which reverses the sequence
- `PreserveOrder` emits a `Reverse` call, and `ReverseOrder` emits the bare constructor, applies to all new-instance mapping, not to deep clones alone, and it never applies to an existing-target stack or to a `Queue<T>`

All options above reads from the MSBuild property `Mapperly` + property name. `Riok.Mapperly.targets` declares one `CompilerVisibleProperty` per option, and `MapperBuildConfigurationReader` enumerates the configuration record by reflection, the two names never drift.

[04]-[MEMBER_CONFIGURATION]

Mapperly resolves a flattening such as `Car.Make.Id` to `CarDto.MakeId` from PascalCase names. It does not resolve unflattening, which needs `MapPropertyAttribute`. Mapperly ignores indexed members. `MapNestedPropertiesAttribute` brings every member below one path into scope, as if the source declared them. An immediate source member outranks a nested one, and automatic flattening outranks both. Two nested paths that reach the same target member have no defined order. Name that mapping with `MapPropertyAttribute`.

- Paired constructors take source before target
- `string` path arguments split on `.` into segments, the `string[]` overload takes the segments as written
- `Source` and `Target` are `IReadOnlyCollection<string>`, `SourceFullName` and `TargetFullName` rejoin the segments with `.`
- `StringFormat` is the format string Mapperly passes to a `ToString` call on a type that implements `IFormattable`
- `FormatProvider` names a field or property marked `FormatProviderAttribute`
- Mapperly falls back to the one member that sets `Default` to `true`, and a mapper may set it on one member only
- `MapValueAttribute` assigns a constant when the constructor takes a value, and the value type must match the target type, With `Use` it assigns the result of a parameterless method whose return type matches the target type

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

## [05]-[ENUM_CONFIGURATION]

Attributes that configure one enum mapping:

| [INDEX] | [SYMBOL]                                              | [MEMBERS]                     |
| :-----: | :---------------------------------------------------- | :---------------------------- |
|  [01]   | `MapEnumAttribute(EnumMappingStrategy strategy)`      | `Strategy`                    |
|  [02]   | `MapEnumValueAttribute(object source, object target)` | `Source` `Target` as `object` |
|  [03]   | `MapperIgnoreSourceValueAttribute(object source)`     | `SourceValue` as `Enum?`      |
|  [04]   | `MapperIgnoreTargetValueAttribute(object target)`     | `TargetValue` as `Enum?`      |

`MapEnumAttribute` applies to a mapping method that takes an enum. It also takes `IgnoreCase` as `bool`, which defaults to `false`, `FallbackValue` as `object?`, which defaults to null, and `NamingStrategy` as `EnumNamingStrategy`, which defaults to `MemberName`. `FallbackValue` replaces the throw for an unmapped value, and works with `ByName` and `ByValueCheckDefined` only. Neither `FallbackValue` nor `MapEnumValueAttribute` passes through the naming strategy.

`MapEnumValueAttribute` applies to an enum-to-enum, enum-to-string, or string-to-enum mapping. It takes `object` for both arguments, it pairs an enum member with another enum member or with a string literal. `MapperIgnoreSourceValueAttribute` and `MapperIgnoreTargetValueAttribute` cast their argument to `Enum`, they take an enum member only.

## [06]-[STRATEGY_TYPES]

Strategy Types, options and attributes above name. `RequiredMappingStrategy`, `IgnoreObsoleteMembersStrategy`, `MemberVisibility`, and `MappingConversionType` carry `[Flags]`, `|` adds a member and `& ~` removes one.

- StackCloningStrategy: `PreserveOrder` `ReverseOrder`
- PropertyNameMappingStrategy: `CaseSensitive` `CaseInsensitive` `SnakeCase` `UpperSnakeCase`
- RequiredMappingStrategy: `None = 0` `Both = ~None` `Source = 1 << 0` `Target = 1 << 1`
- IgnoreObsoleteMembersStrategy: `None = 0` `Both = ~None` `Source = 1 << 0` `Target = 1 << 1`
- EnumMappingStrategy: `ByValue` `ByName` `ByValueCheckDefined`
- EnumNamingStrategy: `MemberName` `CamelCase` `PascalCase` `SnakeCase` `UpperSnakeCase` `KebabCase` `UpperKebabCase` `ComponentModelDescriptionAttribute` `SerializationEnumMemberAttribute`
- MemberVisibility: `AllAccessible = All | Accessible` `All = Public | Internal | Protected | Private` `Accessible = 1 << 0` `Public = 1 << 1` `Internal = 1 << 2` `Protected = 1 << 3` `Private = 1 << 4`

`ByValueCheckDefined` maps by value and checks that the value is defined in the enum. `ComponentModelDescriptionAttribute` reads `DescriptionAttribute.Description`, and `SerializationEnumMemberAttribute` reads `EnumMemberAttribute.Value`. Both fall back to the member name when the attribute is absent.

`RequiredMappingStrategy.Source` warns about unmapped source members only, and `Target` warns about unmapped target members only. `IgnoreObsoleteMembersStrategy.Source` skips obsolete source members, and `Target` skips obsolete target members. `MapPropertyAttribute` maps an obsolete member whatever the strategy says.

## [07]-[CONVERSIONS]

`MappingConversionType`, the `[Flags]` set that `EnabledConversions` admits. `None = 0` disables every automatic conversion and `All = ~None` enables every one. `Default = All & ~ExplicitCast` is what `EnabledConversions` holds when the author sets nothing, an explicit cast operator converts nothing until the mapper names `All` or adds the bit.

The `[ORDER]` column gives the priority the docs list. Rank 1 is direct assignment, which applies when the source type is assignable to the target type and `UseDeepCloning` is `false`. Rank 20 creates a new target instance and maps its members.

| [INDEX] | [MEMBER]               | [BIT]     | [ORDER] | [CONDITION]                                                  |
| :-----: | :--------------------- | :-------- | :-----: | :----------------------------------------------------------- |
|  [01]   | `Constructor`          | `1 << 0`  |   10    | Target has a constructor taking the source type              |
|  [02]   | `ImplicitCast`         | `1 << 1`  |    8    | Implicit cast operator exists                                |
|  [03]   | `ExplicitCast`         | `1 << 2`  |   14    | Explicit cast operator exists                                |
|  [04]   | `ParseMethod`          | `1 << 3`  |    9    | Source is `string`, target has static `Parse(string)`        |
|  [05]   | `ToStringMethod`       | `1 << 4`  |   15    | Target is `string`, calls `ToString` on the source           |
|  [06]   | `StringToEnum`         | `1 << 5`  |   11    | Source is `string`, target is an enum                        |
|  [07]   | `EnumToString`         | `1 << 6`  |   12    | Source is an enum, target is `string`                        |
|  [08]   | `EnumToEnum`           | `1 << 7`  |   13    | Both are enums, follows `EnumMappingStrategy`                |
|  [09]   | `DateTimeToDateOnly`   | `1 << 8`  |   17    | `DateTime` to `DateOnly` through `FromDateTime`              |
|  [10]   | `DateTimeToTimeOnly`   | `1 << 9`  |   18    | `DateTime` to `TimeOnly` through `FromDateTime`              |
|  [11]   | `Queryable`            | `1 << 10` |    2    | Both are `IQueryable<T>`, expression-tree element mapping    |
|  [12]   | `Enumerable`           | `1 << 11` |    4    | Both are `IEnumerable<T>`, maps each element                 |
|  [13]   | `Dictionary`           | `1 << 12` |    3    | Both are `IDictionary` or `IReadOnlyDictionary`              |
|  [14]   | `Span`                 | `1 << 13` |    5    | Either is `Span<T>` or `ReadOnlySpan<T>`                     |
|  [15]   | `Memory`               | `1 << 14` |    7    | Either is `Memory<T>` or `ReadOnlyMemory<T>`                 |
|  [16]   | `Tuple`                | `1 << 15` |    6    | Target is a `ValueTuple` or a tuple expression               |
|  [17]   | `EnumUnderlyingType`   | `1 << 16` |    -    | Maps an enum from or to its underlying type                  |
|  [18]   | `ToTargetMethod`       | `1 << 17` |   16    | Source has an instance `TTarget ToTTarget()`, not `ToString` |
|  [19]   | `StaticConvertMethods` | `1 << 18` |   19    | Static `ToTTarget`, `Create`, `CreateFrom`, `FromTSource`    |
|  [20]   | `Expression`           | `1 << 19` |    -    | Declared only; the generator reads this bit nowhere          |

`StaticConvertMethods` admits a static `ToTTarget` on the source type. On the target type it admits `Create`, `CreateFrom`, `CreateFromTSource`, and `FromTSource`, matched without case; an array-typed source adds the `FromArray` and `CreateFromArray` spellings. It excludes the `DateTime` conversions. `Tuple` admits a tuple expression outside a queryable projection, and `ValueTuple` inside one.

`MapDerivedTypeAttribute(Type sourceType, Type targetType)` and `MapDerivedTypeAttribute<TSource, TTarget>` register one pair for a base-type or interface mapping. Every source type must extend or implement the parameter type. Every target type must extend or implement the return type. Each source type appears once, and several source types may share one target type. An ordinary mapping emits a runtime switch and throws `ArgumentException` for an unregistered type. An `IQueryable` or expression mapping emits `default(TTarget)` for the unmatched branch. Derived types work for a new-instance mapping and for an existing-target mapping.

An `IQueryable<T>` or expression projection compiles an element mapping into an expression tree. Dictionary and existing-target mappings are unavailable. Object factories and deep cloning do not apply. Reference handling reports `RMG029`. Nullable analysis and property null controls do not apply, nullable-to-non-nullable paths use generated fallbacks. Unsupported enum configuration reports `RMG032` and emits a value cast.

Mapperly inlines a hand-written mapping method when it has an expression body, one return statement, or one local declaration followed by a return. `RMG068` leaves a non-inlined method call in the generated expression.

## [08]-[REFERENCE_HANDLING]

`Riok.Mapperly.Abstractions.ReferenceHandling` supports source graphs with circular references. Set `UseReferenceHandling` to `true` to enable it.

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

To supply another handler, add a parameter of type `IReferenceHandler` marked `ReferenceHandlerAttribute`. Hand-written mapping methods take the same parameter to join the same handler. `ReferenceHandlerAttribute` sits in `Riok.Mapperly.Abstractions.ReferenceHandling`, not beside the other attributes. Methods that take a handler need a second `using` directive.
