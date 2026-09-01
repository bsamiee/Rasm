# [MAPPING_BOUNDARIES]

Mapperly generates structural mappings. It does not establish domain invariants, select recovery, run effects, or perform domain transitions.

## [01]-[BOUNDARY_CONTRACT]

The adapter that can reference both representations owns the mapper. The domain does not reference Mapperly or an external contract.

Keep mapper types internal unless mapping is an intentional public contract. Prefer a static partial mapper for transformations that need no stored inputs.

An instance mapper remains deterministic when its stored collaborators are immutable and pure. Keep mutable state, service location, and ambient reads outside every mapper.

| [DIRECTION] | [MAPPERLY_ROLE] | [REQUIRED_FORM] |
| :-- | :-- | :-- |
| External contract to raw input model | Structural mapping | Validate the raw model before domain construction |
| External contract to constrained domain value | None | Call the domain `Validate` adapter and retain its typed error |
| Validated components to domain aggregate | Optional final assembly | Use only a constructor that is total for those component types |
| Domain value to transport or persistence contract | Structural projection | Map only after the domain result is successful |
| Domain snapshot to next domain snapshot | None | Call a named transition that returns the complete next value |
| Closed union to external case | Case-specific structural mapping | Dispatch through the union's exhaustive `Switch` or `Map` |
| Mutable boundary value to owned mutable target | Existing-target mapping | Confine mutation to the scope that owns the target |
| Persistence query to read model | Expression projection | Materialize before domain construction or effects |

A mapping that can reject input is not a plain `TSource -> TTarget` function. Validation owns rejection and its error type.

Map a successful value inside its existing context:

```csharp
internal static Fin<OrderDto> ToDto(Fin<Order> value) =>
    value.Map(OrderMapper.ToDto);
```

Apply the same rule to `Option`, `Either`, `Validation`, `IO`, `Eff`, transformers, and higher-kinded contexts. Do not make Mapperly select or rebuild their cases.

## [02]-[MAPPING_CONTRACT]

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

An inbound invariant-sensitive mapper never admits `ParseMethod`, `Constructor`, `StaticConvertMethods`, or casts as a substitute for validation. An outbound mapper admits `ImplicitCast` only for a canonical key whose valid source makes the conversion total. A named mapping selected with `Use` owns every other semantic conversion. `ToStringMethod` is formatting, not a wire contract, and uses a fixed provider or an explicit culture input.

Strict null settings make a missing required value a defect in an already-validated mapping. They prevent empty text, `default`, parameterless fallback objects, and skipped assignments from appearing as valid output. Nullable-to-nullable mapping remains valid when the target contract represents absence. A suppression is valid only when independent evidence proves that the declared source nullability is wrong.

Automatic member matching applies only when names and meanings agree. A semantic rename, path, constant, or omission is explicit, and every ignore carries a local `Justification`. With automatic user discovery disabled, `[UserMapping]` admits a method to type-pair discovery; a helper selected only through `Use` needs no attribute. One exact pair has one intentional default, alternatives have unique names, generic templates are disjoint, and no selection depends on declaration order.

External mappings stay local to the mapper that consumes them. Assembly-wide registrations expose only disjoint canonical pairs. Configuration inclusion applies only when direction, member meaning, null policy, and omissions are identical, because inclusion copies configuration rather than implementation. Additional parameters carry immutable values that the boundary already resolved. Before or after behavior is a pure hand-written wrapper; the host boundary owns telemetry and effects.

## [03]-[DOMAIN_TYPE_INTEGRATION]

Raw input reaches a Thinktecture value object through `Validate` and a typed adapter. Raw keys reach a Smart Enum through `Validate` or `TryGet`. `Create`, `Parse`, accessible constructors, static conversion methods, and explicit operators can turn expected rejection into an exception. Disabling those automatic conversions is defense in depth and does not replace validation.

A valid value object or Smart Enum maps outward through its canonical key or declared representation. `ToString()` does not define that representation. Mapperly enum configuration applies only to CLR enums. Independent CLR enum contracts map by case-sensitive name or explicit value pairs, never by numeric position. A fallback is valid only when the target contract defines that value as the exact representation of unknown input.

A closed Thinktecture union uses its full generated `Switch` or `Map` as the outer dispatcher. Mapperly maps one known case inside each arm. This division keeps case selection exhaustive and member translation structural.

```csharp
internal static EventDto ToDto(Event value) =>
    value.Switch(
        created: EventMapper.ToDto,
        cancelled: EventMapper.ToDto);
```

`MapDerivedType`, a second case roster, a partial match, and a fallback arm do not preserve closed-union exhaustiveness. Generated generic, runtime-target, and ordinary derived dispatch throw for an unregistered pair or subtype. Those forms belong only to controlled runtime sets where a mismatch is a defect.

Mapperly cannot consume another source generator's output from the same compilation. A referenced assembly exposes its accessible generated members as metadata, so automatic conversion can change when a generated type moves between projects. Exact representation mappings prevent project topology from choosing semantics. An ordinary user-method body can bind peer-generated members after generation, but Mapperly cannot inline unresolved peer-generated members into a projection.

LanguageExt owns absence, failure, validation, effects, traversal, and transformer topology. A Mapperly method supplies only the total leaf function passed to `Map`, `BiMap`, `Apply`, or traversal. Automatic wrapper construction is unsafe: constructor or cast discovery can manufacture a success case, unwrap a failure, or discard source elements. A generic wrapper helper is selected explicitly with `Use` and preserves every case.

LanguageExt collections keep their own construction policy. Direct assignment shares an immutable collection only when sharing is intentional. An element-changing sequence uses the collection's `Map`; a mutable source is snapshotted before domain publication, and an arbitrary enumerable is forced first. A map is built only after key validation defines ordering, uniqueness, and collision behavior. Incidental enumerable-tuple construction does not define those rules.

## [04]-[CONSTRUCTION_AND_OWNERSHIP]

Construction form determines ownership, mutation, and failure behavior.

| [FORM] | [TARGET_STATE] | [CONSTRAINT] |
| :-- | :-- | :-- |
| New instance | Mapper-owned until return | Constructor and init values precede writable assignments |
| Object factory | Factory result followed by writable assignments | Constructor parameters and init-only members are not mapped |
| Existing target | Caller-owned mutable value | Assignments and collection additions are observable mutation |
| Reference handling | Identity graph under one handler | Registration follows construction and precedes writable members |
| Runtime dispatch | Registered source and target pairs | An unregistered pair or subtype throws |

Prefer one total constructor for an immutable external record. `[MapperConstructor]` chooses between semantically equivalent external constructors; it does not select a domain transition. Constructor arguments run before init-only assignments, and writable assignments run after construction. A type whose invariant depends on later setter repair is not a valid mapping target. An init-only assignment cannot be skipped to preserve its member initializer.

A Mapperly object factory allocates or selects a direct target and exposes no typed failure channel. It is not a Thinktecture validation factory. The factory is pure, deterministic, synchronous, and non-null. A nullable result falls back to a public parameterless construction or throws `NullReferenceException`. Factories do not resolve services, allocate domain identity, or call a rejecting domain factory.

Member and constructor visibility stays at `AllAccessible`. Unsafe accessors can invoke private constructors and write hidden members, but that capability does not grant invariant ownership. They never construct or modify a constrained domain type. Direct assignment can return the source reference, so sharing is valid only when the complete reachable graph is immutable. Deep cloning is an allocation strategy, not proof of ownership, validity, or transition semantics.

An existing-target mapping mutates its argument. Existing collections add without replacement: lists add, queues enqueue, and stacks push the added segment in reverse source order. A null source collection leaves the target unchanged. Keep this form inside a scope that creates and owns the target, or inside an imperative host adapter. It never updates a published domain snapshot.

Null-skipping implements merge behavior, not patch presence. It cannot distinguish omission from clearing. An explicit presence type can fold against `[MappingTargetOriginalValue]` at a mutable boundary, while constructor and init-only targets receive `default` as the original value. Domain changes remain named transitions over complete immutable values.

Reference handling materializes external graphs that require cycles or shared identity. One handler belongs to one graph operation. Constructor and init-only edges run before registration, so they cannot close a generated cycle. An existing-target root is not registered automatically; register that pair before mapping when a back-reference must retain the supplied root. Domain identity uses explicit identifiers rather than mapper reference state.

## [05]-[QUERY_PROJECTIONS]

A query projection is an expression tree interpreted by a query provider. It belongs to the data adapter and returns a read model or transport contract. Keep projection declarations separate from in-memory mappings when their conversion, null, or user-method policies differ. The projection method obtains member configuration from an element mapping with the same source and target pair.

Mapperly must inline each user method, and the selected provider must translate the resulting expression. Additional parameters are immutable scalar query values. Services, mapper state, clocks, configuration objects, and request contexts do not enter the expression. `RMG068` proves that inlining failed; successful inlining does not prove provider translation.

Projection null behavior is not in-memory null behavior. Nullable analysis and mapper property-null controls do not apply, so a nullable path can become empty text, `default`, or a conditional fallback. The read model therefore matches storage nullability. Object factories, existing-target mapping, deep cloning, and reference handling do not supply projection semantics. Unsupported enum behavior remains outside the query expression.

Project stored values, materialize the query, and then start validation and domain construction. Thinktecture factories, LanguageExt composition, result selection, and effects run after materialization. An unmatched derived projection returns `default(TTarget)`, so derived projection is valid only when that result is explicit in the target contract. `AsEnumerable` does not hide this boundary; a deliberately narrow query materializes before the in-memory pipeline begins.

## [06]-[GENERATED_CONTRACT]

Each mapper-owning project references Mapperly directly and privately. Runtime assets remain excluded unless reference handling or retained attributes require the abstractions assembly. Generated mappings use no runtime reflection, but that property does not extend to user methods, factories, query providers, mapped types, or their dependencies.

Warnings fail the build. Mapperly also emits contract failures at information or hidden severity, so each mapper project promotes them explicitly.

```editorconfig
[*.cs]
dotnet_diagnostic.RMG033.severity = error
dotnet_diagnostic.RMG083.severity = error
dotnet_diagnostic.RMG089.severity = error
dotnet_diagnostic.RMG090.severity = error
dotnet_diagnostic.RMG096.severity = error
```

Unmapped members, nullability mismatches, ambiguous candidates, unused parameters, duplicate targets, unjustified ignores, and non-inlined projection methods reject the mapping. Generated source is inspected after a mapped shape, declaration, conversion policy, or package changes. The inspection covers constructor choice, invoked conversions, null branches, aliases, collection allocation, mutation, and runtime fallbacks. Generated bodies remain review evidence, not authored mapping logic, and their filenames do not form a contract.
