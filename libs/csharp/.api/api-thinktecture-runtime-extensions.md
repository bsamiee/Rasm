# [RASM_API_THINKTECTURE_RUNTIME_EXTENSIONS]

`Thinktecture.Runtime.Extensions` is the source-generator and runtime-contract substrate for generated Value Objects, Smart Enums, Unions, object factories, validation errors, key conversion, comparison policy, and serialization metadata. It is injected as a workspace foundation package for normal C# library builds, so package-local generated owners share one attribute and contract identity. JSON, MessagePack, EF, Mapperly, and model-binding adapters consume the generated contracts; they do not re-declare domain identity or validation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Thinktecture.Runtime.Extensions`
- package: `Thinktecture.Runtime.Extensions`
- assembly: `Thinktecture.Runtime.Extensions`
- bound asset: `lib/net9.0` for `net10.0` consumers
- namespaces: `Thinktecture`, `Thinktecture.Collections`, `Thinktecture.Internal`, `JetBrains.Annotations`
- build assets: `build/Thinktecture.Runtime.Extensions.props`, `buildTransitive/Thinktecture.Runtime.Extensions.props`
- generator companions: `Thinktecture.Runtime.Extensions.Analyzers`, `Thinktecture.Runtime.Extensions.Refactorings`, `Thinktecture.Runtime.Extensions.SourceGenerator`
- admission: injected by `Directory.Build.props` under `Workspace Foundation Packages` when `UseWorkspaceLibraries == true`
- rail: generated-domain-owners

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: declaration attributes
- rail: generated-domain-owners
- `SmartEnumAttribute<TKey>` and `ValueObjectAttribute<TKey>` require `TKey : notnull`; `ObjectFactoryAttribute<T>` admits `T : allows ref struct`; and `ValidationErrorAttribute<TValidationError>` requires `TValidationError : class, IValidationError<TValidationError>`.

| [INDEX] | [SYMBOL]                                     | [KIND]                | [CAPABILITY]                  |
| :-----: | :------------------------------------------- | :-------------------- | :---------------------------- |
|  [01]   | `SmartEnumAttribute`                         | generator attribute   | generates keyless smart enums |
|  [02]   | `SmartEnumAttribute<TKey>`                   | generator attribute   | generates keyed smart enums   |
|  [03]   | `ValueObjectAttribute<TKey>`                 | generator attribute   | generates keyed values        |
|  [04]   | `ComplexValueObjectAttribute`                | generator attribute   | generates composite values    |
|  [05]   | `UnionAttribute`                             | generator attribute   | generates regular unions      |
|  [06]   | `UnionAttribute<T...>`                       | generator attribute   | generates ad hoc unions       |
|  [07]   | `AdHocUnionAttribute`                        | generator attribute   | generates runtime-type unions |
|  [08]   | `ObjectFactoryAttribute`                     | attribute base        | owns factory policy           |
|  [09]   | `ObjectFactoryAttribute<T>`                  | factory attribute     | generates typed factories     |
|  [10]   | `ValidationErrorAttribute`                   | attribute base        | owns validation-error type    |
|  [11]   | `ValidationErrorAttribute<TValidationError>` | generator attribute   | selects validation errors     |
|  [12]   | `IgnoreMemberAttribute`                      | member attribute      | excludes owner members        |
|  [13]   | `UseDelegateFromConstructorAttribute`        | constructor attribute | delegates construction        |

[PUBLIC_TYPE_SCOPE]: generated owner contracts and validation
- rail: generated-domain-owners
- `IValidationError<out T>` requires `T : class`; `IConvertible<out T>` requires `T : notnull, allows ref struct`; and `IKeyedObject<TKey>` requires `TKey : notnull`.
- `IObjectFactory<T, TValue, out TValidationError>` requires `TValue : notnull, allows ref struct` and `TValidationError : class, IValidationError<TValidationError>`.

| [INDEX] | [SYMBOL]                                          | [KIND]               | [CAPABILITY]              |
| :-----: | :------------------------------------------------ | :------------------- | :------------------------ |
|  [01]   | `ValidationError`                                 | validation value     | carries validation errors |
|  [02]   | `IValidationError<out T>`                         | validation contract  | creates validation errors |
|  [03]   | `IObjectFactory<T, TValue, out TValidationError>` | factory contract     | owns static validation    |
|  [04]   | `IConvertible<out T>`                             | conversion contract  | projects generated values |
|  [05]   | `IKeyedObject<TKey>`                              | key contract         | exposes generated keys    |
|  [06]   | `ISmartEnum<TKey, T, out TValidationError>`       | smart-enum contract  | owns keyed lookup         |
|  [07]   | `IDisallowDefaultValue`                           | default-value policy | rejects default values    |
|  [08]   | `UnknownSmartEnumIdentifierException`             | lookup failure       | reports unknown keys      |
|  [09]   | `ThinktectureTypeConverter`                       | conversion bridge    | converts generated owners |

[PUBLIC_TYPE_SCOPE]: serialization, comparison, and generator policy
- rail: generated-domain-owners
- `IComparerAccessor<in T>` exposes `static abstract IComparer<T> Comparer { get; }`, and `IEqualityComparerAccessor<in T>` exposes `static abstract IEqualityComparer<T> EqualityComparer { get; }`.
- `KeyMemberComparerAttribute<TAccessor, TKey>` requires `TAccessor : IComparerAccessor<TKey>` and `TKey : notnull`; the equality counterpart requires `TAccessor : IEqualityComparerAccessor<TKey>`.
- `MemberEqualityComparerAttribute<TAccessor, TMember>` requires `TAccessor : IEqualityComparerAccessor<TMember>` and keeps the member in generated construction. `IgnoreMemberAttribute` excludes the member from all generated processing.

| [INDEX] | [SYMBOL]                                              | [KIND]             | [CAPABILITY]                 |
| :-----: | :---------------------------------------------------- | :----------------- | :--------------------------- |
|  [01]   | `SerializationFrameworks`                             | codec selector     | selects companion codecs     |
|  [02]   | `SerializationFrameworksExtensions`                   | codec selector     | tests codec selections       |
|  [03]   | `ComparerAccessors`                                   | comparer policy    | catalogs built-in accessors  |
|  [04]   | `IComparerAccessor<in T>`                             | comparer contract  | supplies ordering comparers  |
|  [05]   | `IEqualityComparerAccessor<in T>`                     | comparer contract  | supplies equality comparers  |
|  [06]   | `KeyMemberComparerAttribute<TAccessor, TKey>`         | comparer attribute | selects key ordering         |
|  [07]   | `KeyMemberEqualityComparerAttribute<TAccessor, TKey>` | comparer attribute | selects key equality         |
|  [08]   | `MemberEqualityComparerAttribute<TAccessor, TMember>` | comparer attribute | selects member equality      |
|  [09]   | `OperatorsGeneration`                                 | generator policy   | selects operator generation  |
|  [10]   | `ConversionOperatorsGeneration`                       | generator policy   | selects conversion operators |
|  [11]   | `FactoryMethodGeneration`                             | generator policy   | selects factory methods      |
|  [12]   | `SwitchMapMethodsGeneration`                          | generator policy   | selects dispatch methods     |
|  [13]   | `NestedUnionParameterNameGeneration`                  | generator policy   | selects nested names         |

[PUBLIC_TYPE_SCOPE]: metadata consumed by companion packages
- rail: generated-domain-owners

| [INDEX] | [SYMBOL]                | [NAMESPACE]             | [CAPABILITY]                                 |
| :-----: | :---------------------- | :---------------------- | :------------------------------------------- |
|  [01]   | `MetadataLookup`        | `Thinktecture.Internal` | metadata discovery for generated owners      |
|  [02]   | `Metadata`              | `Thinktecture.Internal` | generated-owner metadata shape               |
|  [03]   | `ObjectFactoryMetadata` | `Thinktecture.Internal` | object-factory metadata consumed by adapters |
|  [04]   | `ConversionMetadata`    | `Thinktecture.Internal` | key conversion metadata consumed by codecs   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: generated-owner declaration
- rail: generated-domain-owners
- `ISmartEnum<TKey, T, out TValidationError>` requires `TKey : notnull`, `T : ISmartEnum<TKey>`, and `TValidationError : class, IValidationError<TValidationError>`.
- The smart-enum contract exposes `static abstract IReadOnlyList<T> Items { get; }`, `[return: NotNullIfNotNull("key")] static abstract T? Get(TKey? key)`, and `static abstract bool TryGet(TKey? key, [MaybeNullWhen(false)] out T item)`.
- `Get` returns `null` for a null key and throws `UnknownSmartEnumIdentifierException` for an unknown non-null key; `TryGet` returns only `bool`. An owning vocabulary lifts `TryGet` to `Option<T>` when its domain rail requires optional lookup.
- `IObjectFactory<T, TValue, out TValidationError>.Validate` is `static abstract TValidationError? Validate(TValue? value, IFormatProvider? provider, out T? item)`.

| [INDEX] | [SURFACE]                  | [KIND]       | [CAPABILITY]                |
| :-----: | :------------------------- | :----------- | :-------------------------- |
|  [01]   | `[SmartEnum]`              | attribute    | generates keyless enums     |
|  [02]   | `[SmartEnum<TKey>]`        | attribute    | generates keyed enums       |
|  [03]   | `Items`                    | lookup       | enumerates smart-enum rows  |
|  [04]   | `Get`                      | lookup       | returns keyed rows          |
|  [05]   | `TryGet`                   | lookup       | tests keyed lookup          |
|  [06]   | `[ValueObject<T>]`         | attribute    | generates keyed values      |
|  [07]   | `[ComplexValueObject]`     | attribute    | generates composite values  |
|  [08]   | `[Union]`                  | attribute    | generates regular unions    |
|  [09]   | `[Union<T...>]`            | attribute    | generates ad hoc unions     |
|  [10]   | `[ObjectFactory<T>]`       | attribute    | binds alternate factories   |
|  [11]   | `ValidateFactoryArguments` | partial seam | validates factory arguments |
|  [12]   | `Validate`                 | factory seam | admits boundary values      |
|  [13]   | `ToValue`                  | projection   | extracts generated keys     |
|  [14]   | `Switch`                   | dispatch     | exhausts generated cases    |
|  [15]   | `Map`                      | dispatch     | projects generated cases    |

## [04]-[IMPLEMENTATION_LAW]

[OWNER_SELECTION]:
- A single-key domain owner is a Thinktecture `[ValueObject<T>]` or keyed `[SmartEnum<TKey>]`; its equality, conversion, validation, and codec metadata come from this generator.
- A multi-case closed vocabulary is a Thinktecture `[Union]` when generated exhaustive dispatch and codec metadata are required.
- Multi-member structural record equality belongs to `Generator.Equals`, not to a generated key owner. Do not stack both ownership models on the same type.

[CODEC_HANDSHAKE]:
- `Thinktecture.Runtime.Extensions.Json` and `Thinktecture.Runtime.Extensions.MessagePack` consume the metadata and factory contracts from this package. They are serialization adapters over generated owners, not declaration owners.
- `SerializationFrameworks` selects which generated object factories participate in companion codecs. Codec packages must filter through generated metadata rather than rediscovering attributes.

[LOCAL_ADMISSION]:
- Generated owners are declared once in the domain package that owns the vocabulary. Downstream code consumes their generated key projection, validation, conversion, and dispatch surfaces.
- The workspace uses the central injected package identity so generated owners, JSON, MessagePack, EF, and mapper adapters all agree on the same public contracts.
- Hand-written parse/validate/switch/key-conversion helpers beside a generated owner are defects unless they are the explicit partial validation seam the generator calls.

[RAIL_LAW]:
- Package: `Thinktecture.Runtime.Extensions`
- Owns: declaration attributes, generated owner contracts, generated validation/factory/key projection, generator policy enums, and metadata consumed by companion packages.
- Accept: `[ValueObject<T>]`, `[SmartEnum<TKey>]`, `[ComplexValueObject]`, `[Union]`, `[ObjectFactory<T>]`, generated `Validate`, `ToValue`, `Switch`, and `Map`.
- Reject: treating JSON/MessagePack packages as the source of generated-owner identity, hand-written conversion/validation beside generated owners, or direct use of internal metadata where a generated public contract exists.
