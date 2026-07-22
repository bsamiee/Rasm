# [RASM_API_THINKTECTURE_RUNTIME_EXTENSIONS]

`Thinktecture.Runtime.Extensions` mints every generated domain owner in the branch: one declaration attribute over a `partial` type emits that owner's key member, factory pair, validation seam, conversion operators, comparison policy, and exhaustive dispatch, and each policy property narrows the emission rather than replacing it. Its runtime half carries the static-abstract contracts and the cached metadata a companion adapter binds, so an adapter projects a generated owner onto its own wire while identity, admission, and key projection stay here.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Thinktecture.Runtime.Extensions`
- package: `Thinktecture.Runtime.Extensions` (LICENSE.md, © Pawel Gerr)
- assembly: `Thinktecture.Runtime.Extensions` carries the attributes, contracts, comparers, and metadata; the source generator, analyzers, and refactorings ride as compile-only companions that never bind at runtime
- namespace: `Thinktecture`, `Thinktecture.Collections`, `Thinktecture.Internal`, `JetBrains.Annotations`
- depends: `Thinktecture.Runtime.Extensions.SourceGenerator`, `Thinktecture.Runtime.Extensions.Analyzers`, `Thinktecture.Runtime.Extensions.Refactorings`
- abi: a generated owner implements the static-abstract `IMetadataOwner` and `IObjectFactoryOwner`, so `MetadataLookup` resolves it through one type-keyed cache instead of an attribute scan
- rail: generated-domain-owners

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: declaration attributes — one concrete form per owner kind, each abstract base carrying the generation policy its forms share.

| [INDEX] | [SYMBOL]                                     | [TYPE_FAMILY]  | [CAPABILITY]                          |
| :-----: | :------------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `SmartEnumAttribute`                         | class          | keyless smart-enum declaration        |
|  [02]   | `SmartEnumAttribute<TKey>`                   | class          | keyed smart-enum declaration          |
|  [03]   | `ValueObjectAttribute<TKey>`                 | class          | keyed value-object declaration        |
|  [04]   | `ComplexValueObjectAttribute`                | class          | multi-member value-object declaration |
|  [05]   | `ValueObjectAttributeBase`                   | abstract class | policy shared by both value forms     |
|  [06]   | `UnionAttribute`                             | class          | regular union over nested cases       |
|  [07]   | `UnionAttribute<T1..T5>`                     | class          | ad-hoc union over type parameters     |
|  [08]   | `AdHocUnionAttribute`                        | class          | ad-hoc union over `Type` slots        |
|  [09]   | `UnionAttributeBase`                         | abstract class | policy shared by ad-hoc union forms   |
|  [10]   | `UnionSwitchMapOverloadAttribute`            | class          | extra dispatch overload via `StopAt`  |
|  [11]   | `ObjectFactoryAttribute`                     | abstract class | factory plane policy                  |
|  [12]   | `ObjectFactoryAttribute<T>`                  | class          | repeatable alternate-factory binding  |
|  [13]   | `ValidationErrorAttribute`                   | abstract class | validation-error type carrier         |
|  [14]   | `ValidationErrorAttribute<TValidationError>` | class          | selects the owner's error type        |
|  [15]   | `IgnoreMemberAttribute`                      | class          | drops a member from generation        |
|  [16]   | `UseDelegateFromConstructorAttribute`        | class          | ctor-supplied delegate backs a method |

- `UnionAttribute<T1..T5>`: arity two through five, each slot declaring `T<N>Name`, `T<N>IsStateless`, and `T<N>IsNullableReferenceType`.
- `AdHocUnionAttribute`: takes case types as `Type` values rather than type parameters, so every slot past `T2` is optional.
- `IgnoreMemberAttribute`: drops the member from construction, equality, and every codec; a member kept in construction but out of equality carries `MemberEqualityComparerAttribute` instead.

[PUBLIC_TYPE_SCOPE]: generated owner contracts — static-abstract surfaces the generator implements on the owner, so a consumer constrains a generic parameter instead of reflecting.

| [INDEX] | [SYMBOL]                                               | [TYPE_FAMILY] | [CAPABILITY]                      |
| :-----: | :----------------------------------------------------- | :------------ | :-------------------------------- |
|  [01]   | `IConvertible<out T>`                                  | interface     | `ToValue()` value projection      |
|  [02]   | `IKeyedObject<TKey>`                                   | interface     | keyed-owner marker                |
|  [03]   | `ISmartEnum<TKey>`                                     | interface     | smart-enum marker constraint      |
|  [04]   | `ISmartEnum<TKey, T, out TValidationError>`            | interface     | item roster and keyed lookup      |
|  [05]   | `IObjectFactory<TValue>`                               | interface     | factory marker constraint         |
|  [06]   | `IObjectFactory<T, TValue, out TValidationError>`      | interface     | static admission of a raw value   |
|  [07]   | `IValidationError<out T>`                              | interface     | error minting contract            |
|  [08]   | `ValidationError`                                      | class         | default message-carrying error    |
|  [09]   | `IDisallowDefaultValue`                                | interface     | rejects default struct values     |
|  [10]   | `Argument<T>`                                          | struct        | ref-struct optional argument slot |
|  [11]   | `UnknownSmartEnumIdentifierException`                  | class         | unknown-key lookup failure        |
|  [12]   | `ThinktectureTypeConverter<T, TKey, TValidationError>` | class         | `TypeConverter` over the key      |

- `ISmartEnum<TKey>` and `IObjectFactory<TValue>`: memberless arities, so a generic constraint names the owner kind without naming its case or error type.
- `Argument<T>`: `readonly ref struct` carrying `IsSet` with an implicit conversion from `T`, so a generated update distinguishes an omitted argument from a null one.
- `UnknownSmartEnumIdentifierException`: extends `KeyNotFoundException` and carries `EnumType` with the offending `Value`.

[PUBLIC_TYPE_SCOPE]: comparison policy — accessor contracts, the built-in accessor catalogue, and the attributes binding one accessor to a key or member.

| [INDEX] | [SYMBOL]                                              | [TYPE_FAMILY] | [CAPABILITY]                      |
| :-----: | :---------------------------------------------------- | :------------ | :-------------------------------- |
|  [01]   | `IComparerAccessor<in T>`                             | interface     | static `Comparer` supply          |
|  [02]   | `IEqualityComparerAccessor<in T>`                     | interface     | static `EqualityComparer` supply  |
|  [03]   | `ComparerAccessors`                                   | class         | built-in accessor catalogue       |
|  [04]   | `KeyMemberComparerAttribute<TAccessor, TKey>`         | class         | key ordering policy               |
|  [05]   | `KeyMemberEqualityComparerAttribute<TAccessor, TKey>` | class         | key equality policy               |
|  [06]   | `MemberEqualityComparerAttribute<T, TMember>`         | class         | member equality policy            |
|  [07]   | `StringKeyedObjectComparer<T>`                        | class         | comparer over string-keyed owners |
|  [08]   | `ProjectionEqualityComparer<T, TItem>`                | class         | equality through a selector       |

[ComparerAccessors]: `Default<T>` `StringOrdinal` `StringOrdinalIgnoreCase` `CurrentCulture` `CurrentCultureIgnoreCase` `InvariantCulture` `InvariantCultureIgnoreCase`
[StringKeyedObjectComparer<T>]: `Ordinal` `OrdinalIgnoreCase` `CurrentCulture` `CurrentCultureIgnoreCase` `InvariantCulture` `InvariantCultureIgnoreCase`

- `StringKeyedObjectComparer<T>`: constrains `T : IConvertible<string>` and exposes each culture as a static singleton, so a string-keyed owner keys a dictionary without a projection lambda.

[PUBLIC_TYPE_SCOPE]: generation-policy vocabularies — closed value sets a declaration attribute takes.

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                         |
| :-----: | :----------------------------------- | :------------ | :----------------------------------- |
|  [01]   | `AccessModifier`                     | enum          | key member and ctor visibility       |
|  [02]   | `MemberKind`                         | enum          | field or property key member         |
|  [03]   | `UnionConstructorAccessModifier`     | enum          | union ctor visibility                |
|  [04]   | `OperatorsGeneration`                | enum          | comparison and equality operator set |
|  [05]   | `ConversionOperatorsGeneration`      | enum          | implicit or explicit conversion      |
|  [06]   | `FactoryMethodGeneration`            | enum          | union factory-method emission        |
|  [07]   | `SwitchMapMethodsGeneration`         | enum          | dispatch-method emission             |
|  [08]   | `NestedUnionParameterNameGeneration` | enum          | nested-union parameter naming        |
|  [09]   | `SerializationFrameworks`            | flags enum    | codec participation                  |
|  [10]   | `SerializationFrameworksExtensions`  | class         | flag membership test                 |

[AccessModifier]: `Private` `Protected` `Internal` `Public` `PrivateProtected` `ProtectedInternal`
[OperatorsGeneration]: `None` `Default` `DefaultWithKeyTypeOverloads`
[ConversionOperatorsGeneration]: `None` `Implicit` `Explicit`
[FactoryMethodGeneration]: `Default` `None` `Always`
[SwitchMapMethodsGeneration]: `None` `Default` `DefaultWithPartialOverloads`
[SerializationFrameworks]: `None` `SystemTextJson` `NewtonsoftJson` `Json` `MessagePack` `All`

[PUBLIC_TYPE_SCOPE]: metadata contracts in `Thinktecture.Internal` — the discovery surface every companion adapter reads.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :----------------------------- | :------------ | :---------------------------------- |
|  [01]   | `IMetadataOwner`               | interface     | static metadata exposure            |
|  [02]   | `IObjectFactoryOwner`          | interface     | static factory roster exposure      |
|  [03]   | `MetadataLookup`               | class         | cached owner-metadata resolution    |
|  [04]   | `Metadata`                     | union         | owner-kind case family              |
|  [05]   | `ObjectFactoryMetadata`        | class         | one factory's plane flags           |
|  [06]   | `ConversionMetadata`           | record struct | key conversion expression pair      |
|  [07]   | `SmartEnumItemMetadata`        | class         | keyed item with its identifier      |
|  [08]   | `KeylessSmartEnumItemMetadata` | class         | keyless item with its identifier    |
|  [09]   | `TryGetFromKey`                | delegate      | boxed key lookup carrying its error |
|  [10]   | `ValidationErrorCreator`       | class         | static-abstract error minting       |
|  [11]   | `StaticAbstractInvoker`        | class         | static-abstract admission and parse |
|  [12]   | `WriteOnceInt`                 | class         | volatile write-once slot            |

[Metadata]: `Keyed.SmartEnum` `Keyed.ValueObject` `KeylessSmartEnum` `ComplexValueObject` `RegularUnion` `AdHocUnion`

- `Metadata`: carries `[Union]` itself and nests `Keyed` as a second `[Union]`, so an adapter folds owner kinds through the same exhaustive dispatch a domain owner uses.
- `Metadata.Keyed`: exposes `KeyType`, `ValidationErrorType`, `ConvertToKey`/`ConvertToKeyExpression`, and `GetKey`, so a store or codec takes the conversion as a delegate or an expression tree.

[PUBLIC_TYPE_SCOPE]: collection and text utilities — allocation-free degenerate collections and the wrappers that keep a count or projection read-only.

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                         |
| :-----: | :----------------------------- | :------------ | :----------------------------------- |
|  [01]   | `Empty`                        | class         | zero-item collection factories       |
|  [02]   | `SingleItem`                   | class         | one-item collection factories        |
|  [03]   | `EnumerableExtensions`         | class         | counted read-only collection wrapper |
|  [04]   | `ReadOnlyCollectionExtensions` | class         | projected read-only collection       |
|  [05]   | `StringExtensions`             | class         | trim-or-null text normalization      |

[Empty]: `Collection` `Collection<T>` `Dictionary<TKey, TValue>` `Set<T>` `Lookup<TKey, TValue>` `Disposable` `AsyncDisposable` `Action<T1..T16>`
[SingleItem]: `Dictionary<TKey, TValue>` `Lookup<TKey, TElement>` `Set<T>`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the declaration a domain owner carries and the members the generator emits onto it.

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :------------------------------------------------------------ | :------- | :----------------------------- |
|  [01]   | `[SmartEnum<TKey>]`                                           | ctor     | keyed smart-enum declaration   |
|  [02]   | `[ValueObject<T>]`                                            | ctor     | keyed value-object declaration |
|  [03]   | `[ComplexValueObject]`                                        | ctor     | multi-member value declaration |
|  [04]   | `[Union]`                                                     | ctor     | regular-union declaration      |
|  [05]   | `[Union<T1..T5>]`                                             | ctor     | ad-hoc union declaration       |
|  [06]   | `[ObjectFactory<T>]`                                          | ctor     | alternate factory binding      |
|  [07]   | `[ValidationError<TValidationError>]`                         | ctor     | error type selection           |
|  [08]   | `Owner.Items -> IReadOnlyList<T>`                             | property | smart-enum item roster         |
|  [09]   | `Owner.Get(TKey?) -> T?`                                      | static   | keyed lookup                   |
|  [10]   | `Owner.TryGet(TKey?, out T) -> bool`                          | static   | tested keyed lookup            |
|  [11]   | `Owner.Create(TValue) -> Owner`                               | factory  | validated construction         |
|  [12]   | `Owner.TryCreate(TValue, out Owner) -> bool`                  | factory  | tested construction            |
|  [13]   | `Owner.Validate(TValue?, IFormatProvider?, out Owner?)`       | static   | admission returning the error  |
|  [14]   | `ValidateFactoryArguments(ref TValidationError?, ref TValue)` | static   | domain validation seam         |
|  [15]   | `Owner.ToValue() -> TKey`                                     | instance | key projection                 |
|  [16]   | `Owner.Switch(TState, …)`                                     | fold     | exhaustive case dispatch       |
|  [17]   | `Owner.Map(TResult, …) -> TResult`                            | fold     | exhaustive case projection     |
|  [18]   | `Owner.Empty`                                                 | property | default instance of a struct   |
|  [19]   | `operator TKey(Owner)`                                        | operator | key projection conversion      |
|  [20]   | `operator Owner(TKey)`                                        | operator | key admission conversion       |

- `Owner.Get`: returns `null` for a null key and throws `UnknownSmartEnumIdentifierException` for an unknown non-null key.
- `Owner.Create`: throws on a validation error, so a boundary value enters through `TryCreate` or `Validate`.
- `Owner.Create`, `Owner.TryCreate`, and `Owner.Empty`: named by `CreateFactoryMethodName`, `TryCreateFactoryMethodName`, and `DefaultInstancePropertyName`.
- `ValidateFactoryArguments`: takes each argument by `ref`, so normalization lands before construction and a trimmed or clamped value reaches the generated ctor.
- `Owner.Switch` and `Owner.Map`: `SwitchMapStateParameterName` threads caller state into every arm, so an arm lambda closes over nothing; `DefaultWithPartialOverloads` adds the overload that handles a subset.
- `operator TKey(Owner)` and `operator Owner(TKey)`: `ConversionToKeyMemberType` and `ConversionFromKeyMemberType` select implicit, explicit, or absent per direction.

[GENERATION_POLICY]: properties on the declaration attribute, each naming or narrowing what the generator emits.
- `[SmartEnumAttribute<TKey>]`: `KeyMemberType` `KeyMemberName` `KeyMemberKind` `KeyMemberAccessModifier` `ComparisonOperators` `EqualityComparisonOperators` `ConversionFromKeyMemberType` `ConversionToKeyMemberType` `SwitchMethods` `MapMethods` `SwitchMapStateParameterName` `SerializationFrameworks` `DisableSpanBasedJsonConversion` `SkipIComparable` `SkipIParsable` `SkipISpanParsable` `SkipIFormattable` `SkipToString`
- `[ValueObjectAttribute<TKey>]`: shares the key-member and conversion set, adds `SkipKeyMember` `UnsafeConversionToKeyMemberType` `NullInFactoryMethodsYieldsNull` `EmptyStringInFactoryMethodsYieldsNull` `AdditionOperators` `SubtractionOperators` `MultiplyOperators` `DivisionOperators`
- `[ValueObjectAttributeBase]`: `SkipFactoryMethods` `ConstructorAccessModifier` `CreateFactoryMethodName` `TryCreateFactoryMethodName` `DefaultInstancePropertyName` `AllowDefaultStructs` `SkipToString` `SkipEqualityComparison` `SerializationFrameworks`
- `[ComplexValueObjectAttribute]`: `DefaultStringComparison`
- `[UnionAttribute]`: `ConversionFromValue` `NestedUnionParameterNames` `SwitchMethods` `MapMethods` `SwitchMapStateParameterName`
- `[UnionAttributeBase]`: `ConstructorAccessModifier` `ConversionFromValue` `ConversionToValue` `FactoryMethodGeneration` `UseSingleBackingField` `SingleBackingFieldType` `DefaultStringComparison` `SwitchMethods` `MapMethods` `SwitchMapStateParameterName` `SkipToString` `SkipEqualityComparison`
- `[ObjectFactoryAttribute]`: `Type` `UseForSerialization` `UseWithEntityFramework` `UseForModelBinding` `HasCorrespondingConstructor`

- `SkipISpanParsable` follows `SkipIParsable`, `SkipIParsable` follows `SkipFactoryMethods`, and `NullInFactoryMethodsYieldsNull` follows `EmptyStringInFactoryMethodsYieldsNull`, so narrowing one property silently narrows its dependents.
- `EqualityComparisonOperators` floors at `ComparisonOperators`, so an ordering operator set drags equality operators up with it.
- `KeyMemberName` defaults to `Key` or `Value` for a public or property member and to `_key` or `_value` for a private field; `ConstructorAccessModifier` defaults to `Private` and `SerializationFrameworks` to `All`.

[ENTRYPOINT_SCOPE]: metadata discovery and static-abstract admission — the surface a companion adapter and a boundary codec bind.

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :---------------------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `MetadataLookup.Find(Type?) -> Metadata?`                         | static   | cached metadata resolution      |
|  [02]   | `MetadataLookup.FindMetadataForConversion(Type?, Func, Func)`     | static   | filtered conversion metadata    |
|  [03]   | `IMetadataOwner.Metadata`                                         | property | static owner metadata           |
|  [04]   | `IObjectFactoryOwner.ObjectFactories`                             | property | static factory roster           |
|  [05]   | `Metadata.Switch(TState, …)`                                      | fold     | owner-kind dispatch             |
|  [06]   | `Metadata.Map(TResult, …) -> TResult`                             | fold     | owner-kind projection           |
|  [07]   | `ValidationError.Create(string) -> ValidationError`               | factory  | default error minting           |
|  [08]   | `ValidationErrorCreator.CreateValidationError<TError>(string)`    | static   | typed error minting             |
|  [09]   | `StaticAbstractInvoker.Validate<T, TKey, TValidationError>(…)`    | static   | keyed admission by constraint   |
|  [10]   | `StaticAbstractInvoker.Validate<T, TValidationError>(…)`          | static   | span admission with no `string` |
|  [11]   | `StaticAbstractInvoker.ParseValue<TValue>(string, …)`             | static   | constrained parse of a key      |
|  [12]   | `StaticAbstractInvoker.ParseValue<TValue>(ReadOnlySpan<char>, …)` | static   | constrained span parse          |
|  [13]   | `HasSerializationFramework(SerializationFrameworks) -> bool`      | instance | codec flag membership           |
|  [14]   | `ProjectionEqualityComparer<T, TItem>(Func, IEqualityComparer)`   | ctor     | selector-projected equality     |
|  [15]   | `ToReadOnlyCollection(int) -> IReadOnlyCollection<T>`             | instance | counted enumerable wrapper      |
|  [16]   | `ToReadOnlyCollection(Func<T, TResult>)`                          | instance | projected collection wrapper    |
|  [17]   | `TrimOrNullify(int)`                                              | instance | trim, bound, and nullify text   |

- `MetadataLookup.FindMetadataForConversion`: takes an `ObjectFactoryMetadata` filter and a `Metadata.Keyed` filter, so a codec selects only the conversion its own plane admits.
- `StaticAbstractInvoker.Validate` and `ParseValue`: each carries a `ReadOnlySpan<char>` arity beside the `TKey`/`string` one and a `TryParseValue` counterpart returning `bool`, so a UTF-8 wire read validates without materializing a `string`.
- `TrimOrNullify`: a bare arity omits the length bound.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- A single-key domain owner declares `[ValueObject<T>]` or `[SmartEnum<TKey>]` and derives its equality, conversion, validation, and codec metadata from the key; a multi-member owner declares `[ComplexValueObject]`, and a closed case set declares `[Union]` so exhaustive dispatch and codec metadata generate together.
- `Generator.Equals` owns multi-member structural equality, so stacking both ownership models on one type mints two comparers over the same members.
- `SerializationFrameworks` on the declaration filters which generated factories a codec sees, so a codec that rediscovers declaration attributes picks up owners the declaration excluded.
- `Empty` and `SingleItem` mint allocation-free zero- and one-item `IReadOnlyList`, `IReadOnlySet`, `IReadOnlyDictionary`, and `ILookup` instances, so a degenerate collection result costs neither an array nor a hash table.

[STACKING]:
- `Thinktecture.Runtime.Extensions.Json`(`.api/api-thinktecture-json.md`): `ThinktectureJsonConverterFactory` reads `MetadataLookup` and `ObjectFactoryMetadata.UseForSerialization` to select the keyed, string, or span converter, and a span-parsable owner routes `Utf8JsonReader` bytes into `StaticAbstractInvoker.Validate<T, TValidationError>(ReadOnlySpan<char>, …)` so a hot read never materializes a `string`.
- `Thinktecture.Runtime.Extensions.MessagePack`(`.api/api-thinktecture-messagepack.md`): `ThinktectureMessageFormatterResolver` selects the class or struct formatter off the same metadata, serializing through generated `ToValue` and deserializing through generated `Validate`, so the JSON and binary wires carry one owner model.
- `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`(`Rasm.Persistence/.api/api-thinktecture-ef.md`): the model-building convention takes `Metadata.Keyed.ConvertFromKeyExpression` and `ConvertToKeyExpression` as the EF `ValueConverter` pair, so a key column's conversion is the generated expression tree rather than a per-property `HasConversion`.
- `Generator.Equals`(`.api/api-generator-equals.md`): key projection already decides identity for a keyed owner, so `[Equatable]` binds the class-root union and the multi-member record whose collection members declare their own bag, sequence, or set semantics.
- `Riok.Mapperly`(`.api/api-mapperly.md`): `MappingConversionType.StaticConvertMethods` resolves the generated `Create(TValue)` inbound and `ImplicitCast`/`ExplicitCast` takes the generated conversion operator outbound, so a value object crosses a DTO boundary with no per-member configuration.
- within-library: one declaration yields the whole owner rail — `[Union]` `Switch`/`Map` threading caller state through `SwitchMapStateParameterName` closure-free, over `IObjectFactory.Validate` admission, generated `ToValue` key projection, and `KeyMemberComparerAttribute`/`MemberEqualityComparerAttribute` accessor policy — so `Fin<T>` lifts once at the boundary and every step below reads the owner's own contracts.

[LOCAL_ADMISSION]:
- `Directory.Build.props` injects the package under one central identity for every workspace library, so generated owners and every adapter agree on one attribute and contract set.
- `ThinktectureRuntimeExtensions_SourceGenerator_GenerateJetBrainsAnnotations` stays off and `JetBrains.Annotations` binds as a package, so the generator's internal polyfill never collides across projects.
- Each vocabulary declares once in the package owning it, and downstream code consumes its generated key projection, validation, conversion, and dispatch.
- An owning vocabulary lifts `TryGet` onto `Option<T>` where its domain rail requires optional lookup, so the throwing `Get` stays behind that lift.
- A hand-written parse, validate, switch, or key-conversion helper beside a generated owner is a defect unless it is the `ValidateFactoryArguments` partial the generator calls.

[RAIL_LAW]:
- Package: `Thinktecture.Runtime.Extensions`
- Owns: generated domain-owner declaration with the runtime contracts, comparison policy, generation vocabularies, and cached metadata every companion adapter binds.
- Accept: `[SmartEnum<TKey>]`, `[ValueObject<T>]`, `[ComplexValueObject]`, `[Union]`, and `[ObjectFactory<T>]` declarations; the generated `Create`/`TryCreate`/`Validate`/`ToValue`/`Switch`/`Map` rail; `MetadataLookup` and `StaticAbstractInvoker` for adapter authoring.
- Reject: a hand-written parse, validate, equality, or key-conversion helper beside a generated owner, a codec that rediscovers declaration attributes instead of reading metadata, and `Generator.Equals` stacked on a key-projected owner.
