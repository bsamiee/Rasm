# [RASM_API_THINKTECTURE_RUNTIME_EXTENSIONS]

`Thinktecture.Runtime.Extensions` is the source-generator and runtime-contract substrate for generated Value Objects, Smart Enums, Unions, object factories, validation errors, key conversion, comparison policy, and serialization metadata. It is injected as a workspace foundation package for normal C# library builds, so package-local generated owners share one attribute and contract identity. JSON, MessagePack, EF, Mapperly, and model-binding adapters consume the generated contracts; they do not re-declare domain identity or validation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Thinktecture.Runtime.Extensions`
- package: `Thinktecture.Runtime.Extensions` `10.4.0`
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

| [INDEX] | [SYMBOL]                                      | [PACKAGE_ROLE]       | [CAPABILITY]                                      |
| :-----: | :-------------------------------------------- | :------------------- | :------------------------------------------------ |
|  [01]   | `SmartEnumAttribute` / `SmartEnumAttribute<TKey>` | generator attribute | keyless and keyed smart enum generation           |
|  [02]   | `ValueObjectAttribute` / `ValueObjectAttribute<T>` | generator attribute | single-key value-object generation                |
|  [03]   | `ComplexValueObjectAttribute`                 | generator attribute  | multi-member value-object generation              |
|  [04]   | `UnionAttribute` / `UnionAttribute<T...>`     | generator attribute  | regular discriminated-union generation            |
|  [05]   | `AdHocUnionAttribute`                         | generator attribute  | ad-hoc union generation                           |
|  [06]   | `ObjectFactoryAttribute` / `ObjectFactoryAttribute<T>` | factory attribute | alternate key/object factory for serialization, EF, and model binding |
|  [07]   | `ValidationErrorAttribute`                    | generator attribute  | generated validation-error projection             |
|  [08]   | `IgnoreMemberAttribute`                       | member attribute     | excludes a member from generated owner processing |
|  [09]   | `UseDelegateFromConstructorAttribute`         | constructor attribute| routes generated construction through a constructor delegate |

[PUBLIC_TYPE_SCOPE]: generated owner contracts and validation
- rail: generated-domain-owners

| [INDEX] | [SYMBOL]                                      | [PACKAGE_ROLE]       | [CAPABILITY]                                      |
| :-----: | :-------------------------------------------- | :------------------- | :------------------------------------------------ |
|  [01]   | `ValidationError`                             | validation value     | built-in validation-error carrier                 |
|  [02]   | `IValidationError<T>`                         | validation contract  | typed generated validation-error projection       |
|  [03]   | `IObjectFactory<T, TKey, TValidationError>`   | factory contract     | generated static validate/create contract read by codecs |
|  [04]   | `IConvertible<TKey>`                          | conversion contract  | key projection contract                           |
|  [05]   | `IKeyedObject<TKey>`                          | key contract         | generated key access contract                     |
|  [06]   | `IDisallowDefaultValue`                       | null/default policy  | generated owner policy for default-value rejection |
|  [07]   | `UnknownSmartEnumIdentifierException`         | lookup failure       | smart-enum unknown-key failure                    |
|  [08]   | `ThinktectureTypeConverter`                   | type conversion      | generated owner type-converter bridge             |

[PUBLIC_TYPE_SCOPE]: serialization, comparison, and generator policy
- rail: generated-domain-owners

| [INDEX] | [SYMBOL]                                      | [PACKAGE_ROLE]       | [CAPABILITY]                                      |
| :-----: | :-------------------------------------------- | :------------------- | :------------------------------------------------ |
|  [01]   | `SerializationFrameworks`                     | codec selector       | `SystemTextJson`, `NewtonsoftJson`, `MessagePack`, and `All` selector values |
|  [02]   | `SerializationFrameworksExtensions`           | codec selector       | selector helpers consumed by codec metadata       |
|  [03]   | `ComparerAccessors`                           | comparer policy      | generated comparer accessor catalog               |
|  [04]   | `KeyMemberComparerAttribute`                  | comparer attribute   | comparison policy for key members                 |
|  [05]   | `KeyMemberEqualityComparerAttribute`          | comparer attribute   | equality policy for key members                   |
|  [06]   | `MemberEqualityComparerAttribute`             | comparer attribute   | equality policy for non-key members               |
|  [07]   | `OperatorsGeneration`                         | generator policy     | generated operator policy                         |
|  [08]   | `ConversionOperatorsGeneration`               | generator policy     | generated conversion-operator policy              |
|  [09]   | `FactoryMethodGeneration`                     | generator policy     | generated factory-method policy                   |
|  [10]   | `SwitchMapMethodsGeneration`                  | generator policy     | generated `Switch`/`Map` method policy            |
|  [11]   | `NestedUnionParameterNameGeneration`          | generator policy     | nested-union generated parameter naming policy    |

[PUBLIC_TYPE_SCOPE]: metadata consumed by companion packages
- rail: generated-domain-owners

| [INDEX] | [SYMBOL]                         | [NAMESPACE]             | [CAPABILITY]                                      |
| :-----: | :------------------------------- | :---------------------- | :------------------------------------------------ |
|  [01]   | `MetadataLookup`                 | `Thinktecture.Internal` | metadata discovery for generated owners           |
|  [02]   | `Metadata`                       | `Thinktecture.Internal` | generated-owner metadata shape                    |
|  [03]   | `ObjectFactoryMetadata`          | `Thinktecture.Internal` | object-factory metadata consumed by adapters      |
|  [04]   | `ConversionMetadata`             | `Thinktecture.Internal` | key conversion metadata consumed by codecs        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: generated-owner declaration
- rail: generated-domain-owners

| [INDEX] | [SURFACE]                         | [CALL_SHAPE]                 | [CAPABILITY]                                      |
| :-----: | :-------------------------------- | :--------------------------- | :------------------------------------------------ |
|  [01]   | `[SmartEnum]` / `[SmartEnum<TKey>]` | type attribute              | generates smart enum cases, lookup, parse, `Switch`, and `Map` |
|  [01a]  | `T Get(TKey key)` / `bool TryGet(TKey? key, out T? item)` / `Items` | generated lookup members | the ONLY generated lookups: throwing `Get` (`UnknownSmartEnumIdentifierException`), the `bool`/`out` `TryGet`, the `Items` roster — an `Option<T>`-returning `TryGet(key)` is NOT generated; the corpus idiom is the hand-written one-expression Option-lift overload `public static Option<X> TryGet(string key) => TryGet(key, out X? row) && row is { } hit ? Some(hit) : None;` declared ON the owning vocabulary (elements/spatial/zones pattern), never assumed |
|  [02]   | `[ValueObject<T>]`                | type attribute               | generates key-backed value object, factories, conversion, and equality |
|  [03]   | `[ComplexValueObject]`            | type attribute               | generates a validated multi-field owner           |
|  [04]   | `[Union]` / `[Union<T...>]`       | type attribute               | generates regular or ad-hoc union and exhaustive dispatch |
|  [05]   | `[ObjectFactory<T>]`              | type attribute               | binds alternate wire/store/model key factory      |
|  [06]   | `static partial ValidateFactoryArguments(...)` | generated partial seam | custom domain validation at construction          |
|  [07]   | `Validate(value, provider, out item)` | generated/static factory seam | validation-railed codec/store/model-binding creation |
|  [08]   | `ToValue()` / generated key projection | generated operation       | key extraction for codecs, stores, and mappings   |
|  [09]   | `Switch(...)` / `Map(...)`        | generated operation          | exhaustive generated dispatch                     |

## [04]-[IMPLEMENTATION_LAW]

[OWNER_SELECTION]:
- A single-key domain owner is a Thinktecture `[ValueObject<T>]` or keyed `[SmartEnum<TKey>]`; its equality, conversion, validation, and codec metadata come from this generator.
- A multi-case closed vocabulary is a Thinktecture `[Union]` when generated exhaustive dispatch and codec metadata are required.
- Multi-member structural record equality belongs to `Generator.Equals`, not to a generated key owner. Do not stack both ownership models on the same type.

[CODEC_HANDSHAKE]:
- `Thinktecture.Runtime.Extensions.Json` and `Thinktecture.Runtime.Extensions.MessagePack` consume the metadata and factory contracts from this package. They are serialization adapters over generated owners, not declaration owners.
- `SerializationFrameworks` selects which generated object factories participate in companion codecs. Codec packages should filter through generated metadata rather than rediscovering attributes.

[LOCAL_ADMISSION]:
- Generated owners are declared once in the domain package that owns the vocabulary. Downstream code consumes their generated key projection, validation, conversion, and dispatch surfaces.
- The workspace uses the central injected package identity so generated owners, JSON, MessagePack, EF, and mapper adapters all agree on the same public contracts.
- Hand-written parse/validate/switch/key-conversion helpers beside a generated owner are defects unless they are the explicit partial validation seam the generator calls.

[RAIL_LAW]:
- Package: `Thinktecture.Runtime.Extensions`
- Owns: declaration attributes, generated owner contracts, generated validation/factory/key projection, generator policy enums, and metadata consumed by companion packages.
- Accept: `[ValueObject<T>]`, `[SmartEnum<TKey>]`, `[ComplexValueObject]`, `[Union]`, `[ObjectFactory<T>]`, generated `Validate`, `ToValue`, `Switch`, and `Map`.
- Reject: treating JSON/MessagePack packages as the source of generated-owner identity, hand-written conversion/validation beside generated owners, or direct use of internal metadata where a generated public contract exists.
