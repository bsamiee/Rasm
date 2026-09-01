# [MAPPERLY_API]

Mapperly is a .NET source generator for object mappings. A mapper is a `partial` class marked with `MapperAttribute`. The author declares `partial` mapping methods, and the generator emits their bodies at compile time. The generated code uses no reflection.

Mapperly reports diagnostics with identifiers of the form `RMG###`. Set the severity of one in an `.editorconfig` file with `dotnet_diagnostic.RMG020.severity = error`.

## [01]-[MAPPING_METHODS]

[METHOD_SCOPE]: the `partial` method declarations the generator implements. Declare them on a `[Mapper] partial class` or a `[Mapper] static partial class`. A non-static mapper that declares one static mapping method must declare every mapping method static.

| [INDEX] | [DECLARATION]                                                    | [CAPABILITY]                    |
| :-----: | :--------------------------------------------------------------- | :------------------------------ |
|  [01]   | `partial TTarget Map(TSource source)`                            | new target instance             |
|  [02]   | `static partial TTarget ToDto(this TSource source)`              | extension mapping               |
|  [03]   | `static partial TTarget? Map(TSource? source)`                   | nullable source and target      |
|  [04]   | `partial void Update(TSource source, TTarget target)`            | existing target, second holds it |
|  [05]   | `partial void Update([MappingTarget] TTarget t, TSource s)`      | existing target, first holds it  |
|  [06]   | `static partial IQueryable<TTarget> ProjectTo(this IQueryable<TSource> q)` | queryable projection   |
|  [07]   | `partial TTarget Map<TTarget>(TSource source)`                   | target type resolved by caller  |
|  [08]   | `partial TTarget Map<TSource, TTarget>(TSource source)`          | source and target both generic  |
|  [09]   | `partial object Map(object source, Type targetType)`             | target type known at run time   |
|  [10]   | `partial TTarget Map(TSource s, [ReferenceHandler] IReferenceHandler h)` | caller supplies the handler |
|  [11]   | `partial TTarget Map(TSource source, int extra)`                 | additional mapping parameter    |
|  [12]   | `[MapDerivedType<TA, TB>] partial TBase Map(TSourceBase source)` | derived type mapping            |

An existing-target method returns `void`. The second parameter is the target unless `MappingTargetAttribute` names another parameter. `MappingTargetAttribute` accepts the `this` parameter of an extension method.

A generic or runtime-target-type method dispatches to the mappings declared in the same mapper. An unknown pair at run time throws `ArgumentException`. Neither form accepts additional parameters. Both accept `MapDerivedTypeAttribute` on the method itself.

An additional parameter matches a target member by name, without case. It ranks below a `MapPropertyAttribute` configuration and above a by-name source member. Mapperly does not pass an additional parameter to a nested mapping. A method that takes one cannot be the default mapping, and cannot also be a generic, runtime-target-type, or derived-type mapping.

A static mapping method satisfies a `static abstract` interface member, so a `[Mapper] partial class` may implement a mapping interface.

## [02]-[ATTRIBUTES]

[TYPE_SCOPE]: every attribute in `Riok.Mapperly.Abstractions`, except `ReferenceHandlerAttribute`, which is in `Riok.Mapperly.Abstractions.ReferenceHandling`. `[MULTIPLE]` states whether the attribute sets `AllowMultiple`.

| [INDEX] | [SYMBOL]                                  | [TARGET]              | [MULTIPLE] | [CAPABILITY]                          |
| :-----: | :---------------------------------------- | :-------------------- | :--------: | :------------------------------------ |
|  [01]   | `MapperAttribute`                         | class                 |     no     | mapper declaration and options        |
|  [02]   | `MapperDefaultsAttribute`                 | assembly              |     no     | same options for every mapper         |
|  [03]   | `UseMapperAttribute`                      | field, property       |     no     | use the member's mapping methods      |
|  [04]   | `UseStaticMapperAttribute`                | class                 |    yes     | use a type's static mapping methods   |
|  [05]   | `UseStaticMapperAttribute<T>`             | class                 |    yes     | same, generic form                    |
|  [06]   | `MapperConstructorAttribute`              | constructor           |     no     | select the constructor to call        |
|  [07]   | `ObjectFactoryAttribute`                  | method                |     no     | construct or resolve the target       |
|  [08]   | `MapperIgnoreAttribute`                   | property, field, method |   no     | exclude a member or a method          |
|  [09]   | `UserMappingAttribute`                    | method                |     no     | user-implemented mapping method       |
|  [10]   | `NamedMappingAttribute`                   | method                |     no     | name a mapping for `Use`              |
|  [11]   | `IncludeMappingConfigurationAttribute`    | method                |     no     | reuse another method's configuration  |
|  [12]   | `MappingTargetAttribute`                  | parameter             |     no     | mark the parameter as the target      |
|  [13]   | `FormatProviderAttribute`                 | field, property       |     no     | expose an `IFormatProvider`           |
|  [14]   | `ReferenceHandlerAttribute`               | parameter             |     no     | mark the reference-handler parameter  |
|  [15]   | `MapPropertyAttribute`                    | method                |    yes     | rename, flatten, and unflatten        |
|  [16]   | `MapPropertyFromSourceAttribute`          | method                |    yes     | map the source object to a member     |
|  [17]   | `MapNestedPropertiesAttribute`            | method                |    yes     | flatten every member of a path        |
|  [18]   | `MapValueAttribute`                       | method                |    yes     | assign a constant or generated value  |
|  [19]   | `MapperIgnoreSourceAttribute`             | method                |    yes     | exclude a source member               |
|  [20]   | `MapperIgnoreTargetAttribute`             | method                |    yes     | exclude a target member               |
|  [21]   | `MapperRequiredMappingAttribute`          | method                |     no     | unmapped-member diagnostics           |
|  [22]   | `MapperIgnoreObsoleteMembersAttribute`    | method                |     no     | obsolete-member policy                |
|  [23]   | `MapDerivedTypeAttribute`                 | method                |    yes     | one derived source and target pair    |
|  [24]   | `MapDerivedTypeAttribute<TSource,TTarget>` | method               |    yes     | same, generic form                    |
|  [25]   | `MapEnumAttribute`                        | method                |     no     | enum strategy for one mapping         |
|  [26]   | `MapEnumValueAttribute`                   | method                |    yes     | pair two enum members                 |
|  [27]   | `MapperIgnoreSourceValueAttribute`        | method                |    yes     | exclude a source enum value           |
|  [28]   | `MapperIgnoreTargetValueAttribute`        | method                |    yes     | exclude a target enum value           |

`MapperDefaultsAttribute` derives from `MapperAttribute`, so it carries the same options. `MapperAttribute` overrides it, and `MapperRequiredMappingAttribute` and `MapperIgnoreObsoleteMembersAttribute` override both for one method.

`UseStaticMapperAttribute(Type mapperType)` and `UseStaticMapperAttribute<T>` take the type whose static mapping methods enter the mapper. `UseMapperAttribute` marks a field or property whose instance and static mapping methods enter the mapper. The author initializes that member.

`MapperConstructorAttribute` marks the constructor Mapperly calls. Without it Mapperly ranks accessible constructors: a parameterless one first when `PreferParameterlessConstructors` is `true`, otherwise by descending parameter count. Mapperly considers a constructor marked `ObsoleteAttribute` last. It uses the first constructor whose parameters all map, and matches parameter names without case.

`ObjectFactoryAttribute` marks a method that returns a non-void type and takes zero parameters or one parameter. Mapperly passes the source object to that one parameter. A factory may be generic, with or without type constraints. Mapperly uses the first factory whose signature matches one of these forms. It cannot map to an init-only property or a constructor parameter of a type an object factory builds.

```csharp
TargetType CreateTargetType();
TargetType CreateTargetType(SourceType source);
TargetType CreateTargetType<S>(S source);
T CreateTargetType<T>();
T CreateTargetType<T>(SourceType source);
TTarget CreateTargetType<TSource, TTarget>(TSource source);
TTarget CreateTargetType<TTarget, TSource>(TSource source);
```

`MapperIgnoreAttribute` excludes a member from every mapping. Applied to a method, it also removes that method from the conversion candidates. Those candidates are `Parse`, an instance `ToTarget` method, and a static `Create`, `CreateFrom`, `From`, or `To` method.

`UserMappingAttribute` marks a hand-written method as a mapping method. `Default` names the one mapping Mapperly uses for a type pair, and only one mapping per pair may set it. `Ignore` removes the method from discovery. Both are `bool` and default to `false`. Mapperly discovers hand-written methods by signature while `AutoUserMappings` is `true`. It reports `RMG060` when a pair has several mappings and no default.

A hand-written method's parameter and return types must match the mapped types exactly, including nullability. To run code before or after a generated mapping, call the generated method from a hand-written wrapper and mark the wrapper `[UserMapping(Default = true)]`. A hand-written existing-target method may declare its target parameter `ref`, and the generated code then updates the caller's reference.

`NamedMappingAttribute(string name)` gives a mapping a name that `Use` and `IncludeMappingConfigurationAttribute` reference instead of the method name. `IncludeMappingConfigurationAttribute(string name)` copies the `MapPropertyAttribute`, `MapPropertyFromSourceAttribute`, `MapperIgnoreTargetAttribute`, `MapperIgnoreSourceAttribute`, `MapperIgnoreObsoleteMembersAttribute`, `MapperRequiredMappingAttribute`, `MapValueAttribute`, and `MapDerivedTypeAttribute` configuration of the named method. The named method must map the same types or their base types. A colliding configuration is an error, and an ambiguous name reports `RMG062`.

A `Use` value and an `IncludeMappingConfigurationAttribute` name both accept a reference outside the mapper. Prefix the path with `@` inside `nameof`, or write the fully qualified path as a string. `@` also works in a `MapPropertyAttribute` path, where `nameof(@MyNamespace.Car.Make.Id)` yields the path `Make.Id`.

## [03]-[MAPPER_OPTIONS]

[TYPE_SCOPE]: the settable properties of `MapperAttribute`, and therefore of `MapperDefaultsAttribute`.

| [INDEX] | [PROPERTY]                           | [TYPE]                          | [DEFAULT]       | [EFFECT]                      |
| :-----: | :----------------------------------- | :------------------------------ | :-------------- | :---------------------------- |
|  [01]   | `PropertyNameMappingStrategy`        | `PropertyNameMappingStrategy`   | `CaseSensitive` | member-name matching          |
|  [02]   | `EnumMappingStrategy`                | `EnumMappingStrategy`           | `ByValue`       | enum-member matching          |
|  [03]   | `EnumNamingStrategy`                 | `EnumNamingStrategy`            | `MemberName`    | enum-to-string naming         |
|  [04]   | `EnumMappingIgnoreCase`              | `bool`                          | `false`         | enum-match casing             |
|  [05]   | `ThrowOnMappingNullMismatch`         | `bool`                          | `true`          | null return, non-null result  |
|  [06]   | `ThrowOnPropertyMappingNullMismatch` | `bool`                          | `false`         | null source, non-null member  |
|  [07]   | `AllowNullPropertyAssignment`        | `bool`                          | `true`          | assign null to a nullable one |
|  [08]   | `UseDeepCloning`                     | `bool`                          | `false`         | copy instead of reuse         |
|  [09]   | `EnabledConversions`                 | `MappingConversionType`         | `All`           | admitted conversions          |
|  [10]   | `UseReferenceHandling`               | `bool`                          | `false`         | circular-reference support    |
|  [11]   | `IgnoreObsoleteMembersStrategy`      | `IgnoreObsoleteMembersStrategy` | `None`          | obsolete-member policy        |
|  [12]   | `RequiredMappingStrategy`            | `RequiredMappingStrategy`       | `Both`          | unmapped-member diagnostics   |
|  [13]   | `RequiredEnumMappingStrategy`        | `RequiredMappingStrategy`       | `Both`          | unmapped-value diagnostics    |
|  [14]   | `IncludedMembers`                    | `MemberVisibility`              | `AllAccessible` | mapped member accessibility   |
|  [15]   | `IncludedConstructors`               | `MemberVisibility`              | `AllAccessible` | constructor accessibility     |
|  [16]   | `PreferParameterlessConstructors`    | `bool`                          | `true`          | constructor order             |
|  [17]   | `AutoUserMappings`                   | `bool`                          | `true`          | discovery by signature        |

`AllowNullPropertyAssignment` decides whether a null source value reaches a nullable target member. When it is `false`, or the target member is not nullable, `ThrowOnPropertyMappingNullMismatch` decides between an `ArgumentNullException` and a skipped assignment. Setting `AllowNullPropertyAssignment` to `false` turns an existing-target mapping into a merge.

`ThrowOnMappingNullMismatch` decides what a mapping method with a non-nullable return type does when the result is null. When it is `false`, Mapperly returns `string.Empty` for a string and `default` for a value type. For a reference type it creates a new instance through a public parameterless constructor. Without such a constructor it throws `ArgumentNullException`.

The three null options do not apply to a required init property or to an `IQueryable<T>` projection.

`UseDeepCloning` never copies a well-known .NET immutable type. `RMG083` reports a member of an immutable type that Mapperly cannot clone.

`IncludedMembers` and `IncludedConstructors` set independently. `AllAccessible` maps every member the mapper can reach directly. `All` drops the `Accessible` bit, so Mapperly emits `UnsafeAccessorAttribute` accessors for the members it cannot reach. Those accessors use no reflection.

## [04]-[MEMBER_CONFIGURATION]

[TYPE_SCOPE]: the attributes that configure one mapping method's members. Rows [01] to [14] carry `AllowMultiple`, so a method may repeat them. Rows [15] and [16] apply once.

| [INDEX] | [CONSTRUCTOR]                                          | [EXPOSES]                          |
| :-----: | :----------------------------------------------------- | :--------------------------------- |
|  [01]   | `MapPropertyAttribute(string source, string target)`   | `Source` `Target`                  |
|  [02]   | `MapPropertyAttribute(string[] source, string target)` | `Source` `Target`                  |
|  [03]   | `MapPropertyAttribute(string source, string[] target)` | `Source` `Target`                  |
|  [04]   | `MapPropertyAttribute(string[] source, string[] target)` | `Source` `Target`                |
|  [05]   | `MapPropertyFromSourceAttribute(string target)`        | `Target`                           |
|  [06]   | `MapPropertyFromSourceAttribute(string[] target)`      | `Target`                           |
|  [07]   | `MapNestedPropertiesAttribute(string source)`          | `Source`                           |
|  [08]   | `MapNestedPropertiesAttribute(string[] source)`        | `Source`                           |
|  [09]   | `MapValueAttribute(string target, object? value)`      | `Target` `Value`                   |
|  [10]   | `MapValueAttribute(string[] target, object? value)`    | `Target` `Value`                   |
|  [11]   | `MapValueAttribute(string target)`                     | `Target`, needs `Use`              |
|  [12]   | `MapValueAttribute(string[] target)`                   | `Target`, needs `Use`              |
|  [13]   | `MapperIgnoreSourceAttribute(string source)`           | `Source` as `string`               |
|  [14]   | `MapperIgnoreTargetAttribute(string target)`           | `Target` as `string`               |
|  [15]   | `MapperRequiredMappingAttribute(RequiredMappingStrategy requiredMappingStrategy)` | `RequiredMappingStrategy` |
|  [16]   | `MapperIgnoreObsoleteMembersAttribute(IgnoreObsoleteMembersStrategy ignoreObsoleteStrategy = Both)` | `IgnoreObsoleteStrategy` |

A paired constructor takes source before target. A `string` path argument splits on `.` into segments, and the `string[]` overload takes the segments as written. `Source` and `Target` are `IReadOnlyCollection<string>`, and `SourceFullName` and `TargetFullName` rejoin the segments with `.`.

A target path also names a constructor parameter. Write the parameter name as a string literal when it differs from the property name. `RMG074` reports two configurations that reach the same target member.

Settable properties past the constructor:

- `MapPropertyAttribute`: `StringFormat`, `FormatProvider`, `Use`, `SuppressNullMismatchDiagnostic`
- `MapPropertyFromSourceAttribute`: `StringFormat`, `FormatProvider`, `Use`
- `MapValueAttribute`: `Use`
- `FormatProviderAttribute`: `Default`

`StringFormat` is the format string Mapperly passes to a `ToString` call on a type that implements `IFormattable`. `FormatProvider` names a field or property marked `FormatProviderAttribute`. Mapperly falls back to the one member that sets `Default` to `true`, and a mapper may set it on one member only. `Use` names the method that maps the member, and `SuppressNullMismatchDiagnostic` suppresses `RMG089`.

`MapValueAttribute` assigns a constant when the constructor takes a value, and the value type must match the target type. With `Use` it assigns the result of a parameterless method whose return type matches the target type. Both forms also reach a constructor parameter.

Mapperly resolves a flattening such as `Car.Make.Id` to `CarDto.MakeId` from PascalCase names. It tries at most 256 member-path permutations. It does not resolve unflattening, which needs `MapPropertyAttribute`. Mapperly ignores indexed members.

`MapNestedPropertiesAttribute` brings every member below one path into scope, as if the source declared them. An immediate source member outranks a nested one, and automatic flattening outranks both. Two nested paths that reach the same target member have no defined order, so name that mapping with `MapPropertyAttribute`.

## [05]-[ENUM_CONFIGURATION]

[TYPE_SCOPE]: the attributes that configure one enum mapping.

| [INDEX] | [SYMBOL]                                          | [MEMBERS]                                 |
| :-----: | :------------------------------------------------ | :---------------------------------------- |
|  [01]   | `MapEnumAttribute(EnumMappingStrategy strategy)`  | `Strategy`                                |
|  [02]   | `MapEnumValueAttribute(object source, object target)` | `Source` `Target` as `object`         |
|  [03]   | `MapperIgnoreSourceValueAttribute(object source)` | `SourceValue` as `Enum?`                  |
|  [04]   | `MapperIgnoreTargetValueAttribute(object target)` | `TargetValue` as `Enum?`                  |

`MapEnumAttribute` applies to a mapping method that takes an enum. It also takes `IgnoreCase` as `bool`, which defaults to `false`, `FallbackValue` as `object?`, which defaults to null, and `NamingStrategy` as `EnumNamingStrategy`, which defaults to `MemberName`. `FallbackValue` replaces the throw for an unmapped value, and works with `ByName` and `ByValueCheckDefined` only. Neither `FallbackValue` nor `MapEnumValueAttribute` passes through the naming strategy.

`MapEnumValueAttribute` applies to an enum-to-enum, enum-to-string, or string-to-enum mapping. It takes `object` for both arguments, so it pairs an enum member with another enum member or with a string literal. `MapperIgnoreSourceValueAttribute` and `MapperIgnoreTargetValueAttribute` cast their argument to `Enum`, so they take an enum member only.

## [06]-[STRATEGY_TYPES]

[TYPE_SCOPE]: the strategy vocabularies the options and attributes above name. `RequiredMappingStrategy`, `IgnoreObsoleteMembersStrategy`, `MemberVisibility`, and `MappingConversionType` carry `[Flags]`, so `|` adds a member and `& ~` removes one. `EnumMappingStrategy`, `EnumNamingStrategy`, and `PropertyNameMappingStrategy` do not.

[EnumMappingStrategy]: `ByValue` `ByName` `ByValueCheckDefined`
[EnumNamingStrategy]: `MemberName` `CamelCase` `PascalCase` `SnakeCase` `UpperSnakeCase` `KebabCase` `UpperKebabCase` `ComponentModelDescriptionAttribute` `SerializationEnumMemberAttribute`
[PropertyNameMappingStrategy]: `CaseSensitive` `CaseInsensitive`
[RequiredMappingStrategy]: `None = 0` `Both = ~None` `Source = 1 << 0` `Target = 1 << 1`
[IgnoreObsoleteMembersStrategy]: `None = 0` `Both = ~None` `Source = 1 << 0` `Target = 1 << 1`
[MemberVisibility]: `AllAccessible = All | Accessible` `All = Public | Internal | Protected | Private` `Accessible = 1 << 0` `Public = 1 << 1` `Internal = 1 << 2` `Protected = 1 << 3` `Private = 1 << 4`

`ByValueCheckDefined` maps by value and checks that the value is defined in the enum. `ComponentModelDescriptionAttribute` reads `DescriptionAttribute.Description`, and `SerializationEnumMemberAttribute` reads `EnumMemberAttribute.Value`. Both fall back to the member name when the attribute is absent.

`RequiredMappingStrategy.Source` warns about unmapped source members only, and `Target` warns about unmapped target members only. `IgnoreObsoleteMembersStrategy.Source` skips obsolete source members, and `Target` skips obsolete target members. `MapPropertyAttribute` maps an obsolete member whatever the strategy says.

## [07]-[CONVERSIONS]

[TYPE_SCOPE]: `MappingConversionType`, the `[Flags]` set that `EnabledConversions` admits. `None = 0` disables every automatic conversion and `All = ~None` enables every one.

| [INDEX] | [MEMBER]               | [BIT]     | [ORDER] | [CONDITION]                                            |
| :-----: | :--------------------- | :-------- | :-----: | :----------------------------------------------------- |
|  [01]   | `Constructor`          | `1 << 0`  |    10   | target has a constructor taking the source type        |
|  [02]   | `ImplicitCast`         | `1 << 1`  |    8    | an implicit cast operator exists                       |
|  [03]   | `ExplicitCast`         | `1 << 2`  |    14   | an explicit cast operator exists                       |
|  [04]   | `ParseMethod`          | `1 << 3`  |    9    | source is `string`, target has static `Parse(string)`  |
|  [05]   | `ToStringMethod`       | `1 << 4`  |    15   | target is `string`, calls `ToString` on the source     |
|  [06]   | `StringToEnum`         | `1 << 5`  |    11   | source is `string`, target is an enum                  |
|  [07]   | `EnumToString`         | `1 << 6`  |    12   | source is an enum, target is `string`                  |
|  [08]   | `EnumToEnum`           | `1 << 7`  |    13   | both are enums, follows `EnumMappingStrategy`          |
|  [09]   | `DateTimeToDateOnly`   | `1 << 8`  |    17   | `DateTime` to `DateOnly` through `FromDateTime`        |
|  [10]   | `DateTimeToTimeOnly`   | `1 << 9`  |    18   | `DateTime` to `TimeOnly` through `FromDateTime`        |
|  [11]   | `Queryable`            | `1 << 10` |    2    | both are `IQueryable<T>`, object initializers only      |
|  [12]   | `Enumerable`           | `1 << 11` |    4    | both are `IEnumerable<T>`, maps each element           |
|  [13]   | `Dictionary`           | `1 << 12` |    3    | both are `IDictionary` or `IReadOnlyDictionary`        |
|  [14]   | `Span`                 | `1 << 13` |    5    | either is `Span<T>` or `ReadOnlySpan<T>`               |
|  [15]   | `Memory`               | `1 << 14` |    7    | either is `Memory<T>` or `ReadOnlyMemory<T>`           |
|  [16]   | `Tuple`                | `1 << 15` |    6    | target is a `ValueTuple` or a tuple expression         |
|  [17]   | `EnumUnderlyingType`   | `1 << 16` |    —    | maps an enum from or to its underlying type            |
|  [18]   | `ToTargetMethod`       | `1 << 17` |    16   | source has an instance `TTarget ToTTarget()`, not `ToString` |
|  [19]   | `StaticConvertMethods` | `1 << 18` |    19   | a static `ToTTarget`, `Create`, `CreateFrom`, or `From` |

`StaticConvertMethods` admits a static `ToTTarget` on the source type. On the target type it admits `Create`, `CreateFrom`, `CreateFromTSource`, `From`, and `FromTSource`, including their `params` forms. It excludes the two `DateTime` conversions, which rows [09] and [10] own. `Tuple` admits a tuple expression outside a queryable projection, and `ValueTuple` inside one.

The `[ORDER]` column gives the priority the docs list. Rank 1 is direct assignment, which applies when the source type is assignable to the target type and `UseDeepCloning` is `false`. Rank 20 creates a new target instance and maps its members. The docs give `EnumUnderlyingType` no rank.

`MapDerivedTypeAttribute(Type sourceType, Type targetType)` and `MapDerivedTypeAttribute<TSource, TTarget>` register one pair for a base-type or interface mapping. Every source type must extend or implement the parameter type. Every target type must extend or implement the return type. Each source type appears once, and several source types may share one target type. The generator emits a `switch` over the runtime type and throws `ArgumentException` for an unregistered type. Derived types work for a new-instance mapping and for an existing-target mapping.

An `IQueryable<T>` projection compiles to an expression tree. It therefore drops these features:

- object factories
- constructors with unmatched optional parameters
- `ThrowOnPropertyMappingNullMismatch` and `AllowNullPropertyAssignment`
- the `ByName` enum strategy
- reference handling, which `RMG029` rejects
- nullable reference types
- deep cloning

Mapperly inlines a hand-written mapping method into the expression tree when the method is expression-bodied or holds one local variable declaration. It reports `RMG068` when it cannot inline one.

## [08]-[REFERENCE_HANDLING]

[TYPE_SCOPE]: `Riok.Mapperly.Abstractions.ReferenceHandling`, which supports source graphs with circular references. Set `UseReferenceHandling` to `true` to enable it.

| [INDEX] | [SYMBOL]                    | [DECLARATION] | [CAPABILITY]                       |
| :-----: | :-------------------------- | :------------ | :--------------------------------- |
|  [01]   | `IReferenceHandler`         | interface     | stores and resolves target objects |
|  [02]   | `PreserveReferenceHandler`  | sealed class  | the default handler                |
|  [03]   | `ReferenceHandlerAttribute` | attribute     | marks the handler parameter        |

```csharp
bool TryGetReference<TSource, TTarget>(TSource source, [NotNullWhen(true)] out TTarget? target)
    where TSource : notnull where TTarget : notnull;

void SetReference<TSource, TTarget>(TSource source, TTarget target)
    where TSource : notnull where TTarget : notnull;
```

Mapperly calls `TryGetReference` before it creates a target. A `true` result must set `target`, and Mapperly uses that instance. A `false` result makes Mapperly create a new instance and record it through `SetReference`. `PreserveReferenceHandler` returns the same target instance for the same source instance. Its own documentation reserves it for generated code and excludes it from the semantic version contract.

To supply another handler, add a parameter of type `IReferenceHandler` marked `ReferenceHandlerAttribute`. A hand-written mapping method takes the same parameter to join the same handler. `ReferenceHandlerAttribute` sits in `Riok.Mapperly.Abstractions.ReferenceHandling`, not beside the other attributes, so a method that takes a handler needs a second `using` directive.
