# [MAPPERLY_API]

Mapperly is a .NET source generator for object mappings.

## [01]-[MAPPING_METHODS]

[METHOD_SCOPE]: the `partial` method declarations the generator implements. Declare them on a `[Mapper] partial class` or a `[Mapper] static partial class`. Non-static mappers that declare one static mapping method must declare every mapping method static.

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

An existing-target method returns `void`. The second parameter is the target unless `MappingTargetAttribute` names another parameter. `MappingTargetAttribute` accepts the `this` parameter of an extension method.

Generic or runtime-target-type methods dispatch to the mappings declared in the same mapper. An unknown pair at run time throws `ArgumentException`. Neither form accepts additional parameters. Both accept `MapDerivedTypeAttribute` on the method itself.

An additional parameter matches a target member by name, without case. It ranks below a `MapPropertyAttribute` configuration and above a by-name source member. Mapperly forwards matching parameters to explicitly declared nested mapping methods and methods selected by `Use`. It does not synthesize a nested method with additional parameters. Queryable projection parameters become captured expression values. Methods that take one cannot be the default mapping, and cannot also be a generic, runtime-target-type, or derived-type mapping.

Static mapping methods satisfy a `static abstract` interface member, a `[Mapper] partial class` may implement a mapping interface.

Methods with additional parameters cannot be the default mapping. `RMG081` rejects `[UserMapping(Default = true)]` on one. `RMG099` rejects two additional parameters whose names differ only in case, because matching ignores case and only the first would ever bind. `RMG082` reports an additional parameter that no target member consumed. `RMG097` reports unsatisfied parameters on a `MapValue` method, and `RMG098` reports them on a named mapping.

Projection-expression methods return `Expression<Func<TSource, TTarget>>` and take no parameters, because the mapped types come from the delegate's type arguments. They share the rules of an `IQueryable<T>` projection, including the inlining limits and `RMG068`. `MappingConversionType.Expression` names this conversion, but the generator reads that bit nowhere, clearing it disables nothing.

`MappingTargetOriginalValueAttribute` marks a parameter that receives the target member's current value, a hand-written mapping can fold the new value into the old one. It is legal on a user-implemented method that returns a value, and its position among the parameters is free. `RMG100` rejects it on a generated `partial` method, and `RMG001` rejects it on a `void` existing-target method. Mapperly passes `default` for an init-only member or constructor parameter because the target instance is unavailable while it builds the construction expression.

## [02]-[ATTRIBUTES]

[TYPE_SCOPE]: every attribute in `Riok.Mapperly.Abstractions`, except `ReferenceHandlerAttribute`, which is in `Riok.Mapperly.Abstractions.ReferenceHandling`. `[MULTIPLE]` states whether the attribute sets `AllowMultiple`.

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

`MapperDefaultsAttribute` derives from `MapperAttribute`, it carries the same options. `MapperAttribute` overrides it, and `MapperRequiredMappingAttribute` and `MapperIgnoreObsoleteMembersAttribute` override both for one method.

`UseStaticMapperAttribute(Type mapperType)` and `UseStaticMapperAttribute<T>` take the type whose static mapping methods enter the mapper. `UseMapperAttribute` marks a field or property whose instance and static mapping methods enter the mapper. The author initializes that member.

Both `UseStaticMapperAttribute` forms also apply to an assembly, which enters those static mappings into every mapper in it. For one type pair the mapper's own method wins, then an assembly-level `UseStaticMapperAttribute`, then a class-level one, then `UseMapperAttribute`, because the first registration for a pair is kept. `RMG048` rejects a nullable `UseMapperAttribute` member.

`MapperConstructorAttribute` marks the constructor Mapperly calls. Without it Mapperly ranks accessible constructors: a parameterless one first when `PreferParameterlessConstructors` is `true`, otherwise by descending parameter count. Mapperly considers a constructor marked `ObsoleteAttribute` last. It uses the first constructor whose parameters all map, and matches parameter names without case.

`ObjectFactoryAttribute` marks a method that returns a non-void type and takes zero parameters or one parameter. Mapperly passes the source object to that one parameter. Factories may be generic, with or without constraints. Mapperly uses the first factory whose signature matches one of these forms. It cannot map to an init-only property or a constructor parameter of a type an object factory builds.

```csharp
TargetType CreateTargetType();
TargetType CreateTargetType(SourceType source);
TargetType CreateTargetType<S>(S source);
T CreateTargetType<T>();
T CreateTargetType<T>(SourceType source);
TTarget CreateTargetType<TSource, TTarget>(TSource source);
TTarget CreateTargetType<TTarget, TSource>(TSource source);
```

`MapperIgnoreAttribute` excludes a member from every mapping. Applied to a method, it also removes that method from the conversion candidates. Those candidates are `Parse`, an instance `ToTarget` method, and a static `Create`, `CreateFrom`, `FromTSource`, or `ToTTarget` method.

`MapperIgnoreAttribute`, `MapperIgnoreSourceAttribute`, `MapperIgnoreTargetAttribute`, `MapperIgnoreSourceValueAttribute`, and `MapperIgnoreTargetValueAttribute` each expose `Justification` as a `string?`. The value documents the exclusion and changes no mapping. `RMG096` reports an ignore whose `Justification` is absent or whitespace, and defaults to `hidden`. Raising its severity above `hidden` is the documented way to require one.

`UserMappingAttribute` marks a hand-written method as a mapping method. Mapperly distinguishes an omitted `Default` from an explicit `false`: omission permits implicit default selection, `false` excludes pair-wide automatic selection, and `true` selects the pair default. Only one mapping per pair may set `true`. `Ignore = true` removes the method from discovery. Mapperly discovers unattributed hand-written methods by signature while `AutoUserMappings` is `true`. It reports `RMG060` when automatic selection finds several mappings without one explicit default.

Hand-written methods' parameter and return types must match the mapped types exactly, including nullability. To run code before or after a generated mapping, call the generated method from a hand-written wrapper and mark the wrapper `[UserMapping(Default = true)]`. Hand-written existing-target methods may declare the target parameter `ref`, and the generated code then updates the caller's reference.

`NamedMappingAttribute(string name)` gives a mapping a name that `Use` and `IncludeMappingConfigurationAttribute` reference instead of the method name. `IncludeMappingConfigurationAttribute(string name)` merges ignored source and target members, explicit and nested member mappings, values, derived pairs, and enum fallback, ignore, and explicit-value configuration. The including method keeps its enum strategy, casing, naming, and required-enum strategy. The named method must map the same types or their base types. Colliding configurations are errors, and an ambiguous name reports `RMG062`.

`IncludeMappingConfigurationAttribute` sets `AllowMultiple`, a method may include several configurations. Lists concatenate. The including method's required-member and ignore-obsolete settings outrank included settings; otherwise, the first included value wins. `RMG074` reports two configurations that reach the same target member, `RMG091` reports a circular include, and `RMG092` and `RMG093` report a source or target type that is not assignable to the included one.

`Use` values and `IncludeMappingConfigurationAttribute` names both accept a reference outside the mapper. Prefix the path with `@` inside `nameof`, or write the fully qualified path as a string. `@` also works in a `MapPropertyAttribute` path, where `nameof(@MyNamespace.Car.Make.Id)` yields the path `Make.Id`.

## [03]-[MAPPER_OPTIONS]

[TYPE_SCOPE]: the settable properties of `MapperAttribute`, and of `MapperDefaultsAttribute`.

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

`AllowNullPropertyAssignment` decides whether a null source value reaches a nullable target member. When it is `false`, or the target member is not nullable, `ThrowOnPropertyMappingNullMismatch` decides between an `ArgumentNullException` and a skipped assignment. Setting `AllowNullPropertyAssignment` to `false` turns an existing-target mapping into a merge.

`ThrowOnMappingNullMismatch` decides what a mapping method with a non-nullable return type does when the result is null. When it is `false`, Mapperly returns `string.Empty` for a string and `default` for a value type. For a reference type it creates a new instance through a public parameterless constructor. Without such a constructor it throws `ArgumentNullException`.

The null options do not apply to a required init property or to an `IQueryable<T>` projection.

`UseDeepCloning` never copies a well-known .NET immutable type. `RMG083` reports a member of an immutable type that Mapperly cannot clone.

`StackCloningStrategy` decides the element order whenever Mapperly builds a `Stack<T>` through `Stack<T>(IEnumerable<T>)`, which reverses the sequence. `PreserveOrder` emits a `Reverse` call, and `ReverseOrder` emits the bare constructor. It applies to every such new-instance mapping, not to deep clones alone, and it never applies to an existing-target stack or to a `Queue<T>`.

Mapperly reads nullability attributes from `System.Diagnostics.CodeAnalysis`, on two independent axes. `MaybeNullAttribute` widens a non-nullable member for reading, and `NotNullAttribute` narrows a nullable one. `AllowNullAttribute` widens a non-nullable member for writing, and `DisallowNullAttribute` narrows a nullable one. The member's own type decides which of the pair Mapperly consults, one lookup answers each axis. For a member that a referenced assembly declares, Mapperly also reads the getter's return attributes and the setter's value parameter, because the compiler records them there rather than on the property. All four move `RMG089`, and they reach `RMG090` through the member type they imply.

`EnabledConversions` excludes `ExplicitCast` by default, an explicit cast operator converts nothing until a mapper names `All` or adds the bit. The consequence differs by type. Narrowing numeric conversions lack another route and report `RMG008`. Types that offer an explicit operator fall through to a later conversion instead: a target of `string` reaches `ToStringMethod` and emits `ToString`, and any other target reaches member mapping, which silently produces a default or a member-wise copy when the target has an accessible parameterless constructor and otherwise fails to compile on the emitted construction. `RMG066` and `RMG020` are the only reports of the silent outcome.

`IncludedMembers` and `IncludedConstructors` need .NET 8.0 or later once the `Accessible` bit is cleared, because the accessors depend on `UnsafeAccessorAttribute`. `RMG053` reports a compilation without it, and Mapperly then restores the bit. Clearing the bit also raises the metadata import level, a referenced project must set `ProduceReferenceAssembly` to `false` before its private members become visible. Package references deliver an implementation assembly and need no such setting.

`Parse` conversions accept `TTarget Parse(string)` and `TTarget Parse(string, IFormatProvider)`. Mapperly prefers the two-parameter form when a format provider resolves, and passes `null` to a nullable provider parameter when none does.

`IncludedMembers` and `IncludedConstructors` set independently. `AllAccessible` maps every member the mapper can reach directly. `All` drops the `Accessible` bit, Mapperly emits `UnsafeAccessorAttribute` accessors for the members it cannot reach. Those accessors use no reflection.

Every option above also reads from an MSBuild property named `Mapperly` plus the property name. `Riok.Mapperly.targets` declares one `CompilerVisibleProperty` per option, and `MapperBuildConfigurationReader` enumerates the configuration record by reflection, the two names never drift.

```xml
<PropertyGroup>
  <MapperlyRequiredMappingStrategy>Source;Target</MapperlyRequiredMappingStrategy>
</PropertyGroup>
```

`bool` parses through `bool.TryParse`, accepting only `true` and `false` in any casing. An enum parses through `Enum.Parse` without case, and also accepts the underlying number. `[Flags]` enums combine members with `,`, `;`, or `|`, because the reader rewrites `;` and `|` to `,` first. `Enum.Parse` reads no complement operator, a value such as `All & ~ExplicitCast` is expressible only by the member name `Default`.

`RMG095` reports a value that fails to parse, and names the canonical property spelling rather than the one the author typed. The failed property stays unset and the next source supplies the value. An absent or whitespace value is skipped with no diagnostic. `Enum.Parse` checks neither that a combined value targets a `[Flags]` enum nor that a number lies in range, `RMG095` reports neither.

Precedence runs from the `MapperAttribute` initializer, through the MSBuild property, then `MapperDefaultsAttribute`, then `MapperAttribute`, and last the per-method attribute. Each option is nullable in the configuration record, an option written explicitly is distinguishable from an omitted one at every tier. `MapEnumAttribute`, `MapperRequiredMappingAttribute`, and `MapperIgnoreObsoleteMembersAttribute` are the only per-method overrides; the remaining options are mapper-wide.

`RequiredEnumMappingStrategy` is the one option with a longer chain: unset, it inherits `RequiredMappingStrategy` before it reaches the initializer. Relaxing `RequiredMappingStrategy` also relaxes the enum diagnostics unless the mapper sets both.

`Riok.Mapperly.targets` is packaged under `build`, not `buildTransitive`. Projects that reference `Riok.Mapperly` transitively still receive the generator, but declare no `CompilerVisibleProperty`, every `Mapperly` property is ignored there and `RMG095` cannot fire. `MapperDefaultsAttribute` travels with the compilation and carries assembly-wide policy without that limit.

## [04]-[MEMBER_CONFIGURATION]

[TYPE_SCOPE]: the attributes that configure one mapping method's members. Rows [01] to [14] carry `AllowMultiple`, a method may repeat them. Rows [15] and [16] apply once.

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

Paired constructors take source before target. `string` path arguments split on `.` into segments, and the `string[]` overload takes the segments as written. `Source` and `Target` are `IReadOnlyCollection<string>`, and `SourceFullName` and `TargetFullName` rejoin the segments with `.`.

Target paths also name a constructor parameter. Write the parameter name as a string literal when it differs from the property name. `RMG074` reports two configurations that reach the same target member.

Settable properties past the constructor:
- `MapPropertyAttribute`: `StringFormat`, `FormatProvider`, `Use`, `SuppressNullMismatchDiagnostic`
- `MapPropertyFromSourceAttribute`: `StringFormat`, `FormatProvider`, `Use`
- `MapValueAttribute`: `Use`
- `FormatProviderAttribute`: `Default`

`StringFormat` is the format string Mapperly passes to a `ToString` call on a type that implements `IFormattable`. `FormatProvider` names a field or property marked `FormatProviderAttribute`. Mapperly falls back to the one member that sets `Default` to `true`, and a mapper may set it on one member only. `Use` names the method that maps the member, and `SuppressNullMismatchDiagnostic` suppresses `RMG089`.

`MapValueAttribute` assigns a constant when the constructor takes a value, and the value type must match the target type. With `Use` it assigns the result of a parameterless method whose return type matches the target type. Both forms also reach a constructor parameter.

Mapperly resolves a flattening such as `Car.Make.Id` to `CarDto.MakeId` from PascalCase names. It tries at most 256 member-path permutations. It does not resolve unflattening, which needs `MapPropertyAttribute`. Mapperly ignores indexed members.

`MapNestedPropertiesAttribute` brings every member below one path into scope, as if the source declared them. An immediate source member outranks a nested one, and automatic flattening outranks both. Two nested paths that reach the same target member have no defined order. Name that mapping with `MapPropertyAttribute`.

## [05]-[ENUM_CONFIGURATION]

[TYPE_SCOPE]: the attributes that configure one enum mapping.

| [INDEX] | [SYMBOL]                                              | [MEMBERS]                     |
| :-----: | :---------------------------------------------------- | :---------------------------- |
|  [01]   | `MapEnumAttribute(EnumMappingStrategy strategy)`      | `Strategy`                    |
|  [02]   | `MapEnumValueAttribute(object source, object target)` | `Source` `Target` as `object` |
|  [03]   | `MapperIgnoreSourceValueAttribute(object source)`     | `SourceValue` as `Enum?`      |
|  [04]   | `MapperIgnoreTargetValueAttribute(object target)`     | `TargetValue` as `Enum?`      |

`MapEnumAttribute` applies to a mapping method that takes an enum. It also takes `IgnoreCase` as `bool`, which defaults to `false`, `FallbackValue` as `object?`, which defaults to null, and `NamingStrategy` as `EnumNamingStrategy`, which defaults to `MemberName`. `FallbackValue` replaces the throw for an unmapped value, and works with `ByName` and `ByValueCheckDefined` only. Neither `FallbackValue` nor `MapEnumValueAttribute` passes through the naming strategy.

`MapEnumValueAttribute` applies to an enum-to-enum, enum-to-string, or string-to-enum mapping. It takes `object` for both arguments, it pairs an enum member with another enum member or with a string literal. `MapperIgnoreSourceValueAttribute` and `MapperIgnoreTargetValueAttribute` cast their argument to `Enum`, they take an enum member only.

## [06]-[STRATEGY_TYPES]

[TYPE_SCOPE]: the strategy vocabularies the options and attributes above name. `RequiredMappingStrategy`, `IgnoreObsoleteMembersStrategy`, `MemberVisibility`, and `MappingConversionType` carry `[Flags]`, `|` adds a member and `& ~` removes one. `EnumMappingStrategy`, `EnumNamingStrategy`, and `PropertyNameMappingStrategy` do not.

[EnumMappingStrategy]: `ByValue` `ByName` `ByValueCheckDefined`
[EnumNamingStrategy]: `MemberName` `CamelCase` `PascalCase` `SnakeCase` `UpperSnakeCase` `KebabCase` `UpperKebabCase` `ComponentModelDescriptionAttribute` `SerializationEnumMemberAttribute`
[PropertyNameMappingStrategy]: `CaseSensitive` `CaseInsensitive` `SnakeCase` `UpperSnakeCase`
[RequiredMappingStrategy]: `None = 0` `Both = ~None` `Source = 1 << 0` `Target = 1 << 1`
[IgnoreObsoleteMembersStrategy]: `None = 0` `Both = ~None` `Source = 1 << 0` `Target = 1 << 1`
[MemberVisibility]: `AllAccessible = All | Accessible` `All = Public | Internal | Protected | Private` `Accessible = 1 << 0` `Public = 1 << 1` `Internal = 1 << 2` `Protected = 1 << 3` `Private = 1 << 4`
[StackCloningStrategy]: `PreserveOrder` `ReverseOrder`

`ByValueCheckDefined` maps by value and checks that the value is defined in the enum. `ComponentModelDescriptionAttribute` reads `DescriptionAttribute.Description`, and `SerializationEnumMemberAttribute` reads `EnumMemberAttribute.Value`. Both fall back to the member name when the attribute is absent.

`RequiredMappingStrategy.Source` warns about unmapped source members only, and `Target` warns about unmapped target members only. `IgnoreObsoleteMembersStrategy.Source` skips obsolete source members, and `Target` skips obsolete target members. `MapPropertyAttribute` maps an obsolete member whatever the strategy says.

## [07]-[CONVERSIONS]

[TYPE_SCOPE]: `MappingConversionType`, the `[Flags]` set that `EnabledConversions` admits. `None = 0` disables every automatic conversion and `All = ~None` enables every one. `Default = All & ~ExplicitCast` is what `EnabledConversions` holds when the author sets nothing, an explicit cast operator converts nothing until the mapper names `All` or adds the bit.

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
|  [17]   | `EnumUnderlyingType`   | `1 << 16` |         | Maps an enum from or to its underlying type                  |
|  [18]   | `ToTargetMethod`       | `1 << 17` |   16    | Source has an instance `TTarget ToTTarget()`, not `ToString` |
|  [19]   | `StaticConvertMethods` | `1 << 18` |   19    | Static `ToTTarget`, `Create`, `CreateFrom`, `FromTSource`    |
|  [20]   | `Expression`           | `1 << 19` |         | Declared only; the generator reads this bit nowhere          |

`StaticConvertMethods` admits a static `ToTTarget` on the source type. On the target type it admits `Create`, `CreateFrom`, `CreateFromTSource`, and `FromTSource`, matched without case; an array-typed source adds the `FromArray` and `CreateFromArray` spellings. It excludes the `DateTime` conversions, which rows [09] and [10] own. `Tuple` admits a tuple expression outside a queryable projection, and `ValueTuple` inside one.

The `[ORDER]` column gives the priority the docs list. Rank 1 is direct assignment, which applies when the source type is assignable to the target type and `UseDeepCloning` is `false`. Rank 20 creates a new target instance and maps its members. The docs give `EnumUnderlyingType` no rank.

`MapDerivedTypeAttribute(Type sourceType, Type targetType)` and `MapDerivedTypeAttribute<TSource, TTarget>` register one pair for a base-type or interface mapping. Every source type must extend or implement the parameter type. Every target type must extend or implement the return type. Each source type appears once, and several source types may share one target type. An ordinary mapping emits a runtime switch and throws `ArgumentException` for an unregistered type. An `IQueryable` or expression mapping emits `default(TTarget)` for the unmatched branch. Derived types work for a new-instance mapping and for an existing-target mapping.

An `IQueryable<T>` or expression projection compiles an element mapping into an expression tree. Dictionary and existing-target mappings are unavailable. Object factories and deep cloning do not apply. Reference handling reports `RMG029`. Nullable analysis and property null controls do not apply, nullable-to-non-nullable paths use generated fallbacks. Unsupported enum configuration reports `RMG032` and emits a value cast.

Mapperly inlines a hand-written mapping method when it has an expression body, one return statement, or one local declaration followed by a return. `RMG068` leaves a non-inlined method call in the generated expression.

## [08]-[REFERENCE_HANDLING]

[TYPE_SCOPE]: `Riok.Mapperly.Abstractions.ReferenceHandling`, which supports source graphs with circular references. Set `UseReferenceHandling` to `true` to enable it.

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

Mapperly calls `TryGetReference` before it creates a target. `true` results must set `target`, and Mapperly uses that instance. `false` results make Mapperly create a new instance and record it through `SetReference`. `PreserveReferenceHandler` returns the same target instance for the same source instance. Its own documentation reserves it for generated code and excludes it from the semantic version contract.

To supply another handler, add a parameter of type `IReferenceHandler` marked `ReferenceHandlerAttribute`. Hand-written mapping methods take the same parameter to join the same handler. `ReferenceHandlerAttribute` sits in `Riok.Mapperly.Abstractions.ReferenceHandling`, not beside the other attributes. Methods that take a handler need a second `using` directive.
