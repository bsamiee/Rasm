---
name: dotnet-coding-thinktecture
description: "Use when declaring a Thinktecture value object, smart enum, or union, or a TTRESG diagnostic appears, covering generated API, Switch, and Map."
---

# [DOTNET_CODING_THINKTECTURE]

Covers declaring the types `Thinktecture.Runtime.Extensions` generates (value objects, smart enums, ad hoc and regular unions), from their generated API to the packages that integrate them with the frameworks.

[REFERENCES]:
- [01]-[SETTINGS](references/settings.md): Attribute settings of every generated family with defaults and effects, the generator's MSBuild properties
- [02]-[FACTORY_PATHS](references/factory-paths.md): How each integration point reaches an object factory, with the Entity Framework Core read path
- [03]-[SERILOG](references/serilog.md): Serilog destructuring policy with its depth limits and string rendering

Every package name omits the prefix `Thinktecture.Runtime.Extensions.` and every analyzer code omits the prefix `TTRESG`:
- `Analyzers` reports 048, 049, 098, 100 to 110, and 1000 as warnings, 1001 as information, and every other rule as an error
- Every generated type and every type that encloses one is `partial` (006)

## [01]-[VALUE_OBJECTS]

Simple value objects wrap one key member under `[ValueObject<TKey>]`, complex value objects hold read-only members under `[ComplexValueObject]`:
- Both are `partial`, the generator adds `sealed` to a class and `readonly` to a struct and owns the private constructor
- Hand-written part is the validation hook and the domain behavior

```csharp
[ValueObject<string>]
[ValidationError<InvalidCode>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
internal sealed partial class Code {
    public int Length => _value.Length;

    static partial void ValidateFactoryArguments(ref InvalidCode? validationError, ref string value) {
        string? trimmed = value.TrimOrNullify(maxLength: 16);
        if (trimmed is null) {
            validationError = new InvalidCode();
            return;
        }
        value = trimmed.ToUpperInvariant();
    }
}

[ComplexValueObject]
[ValidationError<InvalidBounds>]
internal sealed partial class Bounds {
    public decimal Lower { get; }
    public decimal Upper { get; }

    static partial void ValidateFactoryArguments(ref InvalidBounds? validationError, ref decimal lower, ref decimal upper) {
        if (lower > upper) {
            validationError = new InvalidBounds();
            return;
        }
        lower = Math.Round(lower, 2, MidpointRounding.ToEven);
        upper = Math.Round(upper, 2, MidpointRounding.ToEven);
    }
}
```

Declaration rules the analyzer enforces:
- Every field is read-only (001), every property has no setter (003), an `init` accessor is private (042), a primary constructor is rejected (043)
- Key member is non-nullable
- String keys need both comparer attributes (048), a complex value object with string members sets `DefaultStringComparison` (049)
- `[IgnoreMember]` removes a member from equality, the factories, and every other generated member
- Complex form accepts one member or none, and with one member it receives no key-derived members
- `[ValidationError<T>]` switches the hook parameter, the second `TryCreate` overload, and `Validate` to a type implementing `IValidationError<T>`
- Generator calls `static abstract T Create(string message)` of that type for its own errors
- `ToString()` of the error type is the text that reaches `ValidationException`, `FormatException`, the JSON converters, and model state

### [01.1]-[GENERATED_API]

| [INDEX] | [MEMBER]                             | [BEHAVIOR]                                                                              |
| :-----: | :----------------------------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | `Create(value)`                      | Validates and returns the instance, or throws `ValidationException` with the error text |
|  [02]   | `TryCreate(value, out obj)`          | Returns `false` on rejection, and the 3-parameter overload returns the error            |
|  [03]   | `Validate(value, provider, out obj)` | Returns the error or `null` and never throws, the complex form has no provider          |
|  [04]   | Equality, `GetHashCode`, `==`, `!=`  | Run through the configured comparer                                                     |
|  [05]   | `ToString()`                         | Key's `ToString()`, or `{ Lower = 1.23, Upper = 2.57 }` for the complex form            |
|  [06]   | `IComparable<T>`, `IFormattable`     | Present when the key is comparable or formattable, simple form only                     |
|  [07]   | `IParsable<T>`, `ISpanParsable<T>`   | Present when the key is parsable or a `string`, `Parse` throws `FormatException`        |
|  [08]   | Conversions                          | To the key implicit, from the key explicit through `Create`, unsafe to a value-type key |
|  [09]   | `[TypeConverter]`                    | Emitted on every simple value object that has factory methods                           |

- Complex value objects take one argument per member in declaration order
- `null` arguments for a non-nullable key or member return an error before the hook runs, the hook never repeats that null check

### [01.2]-[HOOK]

`ValidateFactoryArguments` is `static partial void` with `ref TError? validationError` first and the key or each member by `ref` in declaration order:
- Hook rejects by assigning the error and returning, and normalizes by assigning the `ref` parameter
- Compiler erases an absent hook
- Every entry point runs the hook: `Create`, `TryCreate`, `Validate`, the conversion from the key, and `Parse`
- JSON converters, the MessagePack formatter, and model binding run the hook
- Hook reports the first violated rule over one value, independent rules over many inputs accumulate at the input boundary
- `ValidateConstructorArguments(ref TKey value)` exists beside it and rejects by throwing alone

Trailing parameters after the members are declared by value without a default (076):
- Generator then emits `private static TError? ValidateCore(members, extras, out T? obj)` and `private static T CreateCore(members, extras)`
- Public `Validate` passes `default` for every extra, a hand-written factory delegates to `CreateCore`

```csharp
[ValueObject<decimal>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Zero", MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<NegativeAmount>]
internal readonly partial struct Amount {
    static partial void ValidateFactoryArguments(ref NegativeAmount? validationError, ref decimal value, MidpointRounding rounding) {
        if (value < 0) {
            validationError = new NegativeAmount();
            return;
        }
        value = decimal.Round(value, 2, rounding);
    }

    public static Amount Create(decimal value, MidpointRounding rounding) => CreateCore(value, rounding);
}
```

- Rounding runs once inside the hook whichever factory is called
- Multiplication by `decimal` stays disabled, the product needs a rounding decision
- Hooks declared `private static partial string ValidateFactoryArguments(...)` return a value for the constructed instance
- Generated `Validate` passes that value to `partial void FactoryPostInit(string value)` after validation succeeded
- Receiving field has `[IgnoreMember]` and an initializer, a `readonly` struct cannot hold it, that form belongs to a class

### [01.3]-[COMPARERS_AND_SETTINGS]

String keys compare with `StringComparer.OrdinalIgnoreCase` by default and every other key with its own `Equals`:
- `[KeyMemberEqualityComparer<TAccessor, TKey>]` selects the equality comparer
- `[KeyMemberComparer<TAccessor, TKey>]` selects the ordering comparer for `IComparable<T>` and the comparison operators on the simple form alone
- Comparers without an equality comparer are 102
- Equality comparers without a comparer are 103 when the key is comparable and `SkipIComparable` is not set
- Accessors are `ComparerAccessors.StringOrdinal`, `StringOrdinalIgnoreCase`, `CurrentCulture`, and `CurrentCultureIgnoreCase`
- `InvariantCulture`, `InvariantCultureIgnoreCase`, and `Default<T>` complete the accessors
- Custom accessors implement `IEqualityComparerAccessor<T>` or `IComparerAccessor<T>` with one static property
- Complex value objects compare every assignable member, `DefaultStringComparison` sets the comparison of their string members
- `[MemberEqualityComparer<TAccessor, TMember>]` on one member changes its comparer and drops every unattributed member out of equality and hashing

`DefaultWithKeyTypeOverloads` adds operator overloads with the key type in both operand positions, `amount > 42m` compiles without a conversion:
- Generator emits `operator checked +` beside the unchecked form when the key declares it
- Struct value objects reject `default(T)` and `new T()` through `IDisallowDefaultValue` (047)
- Settable properties of the type elsewhere warn until they are `required` (104)
- `AllowDefaultStructs` stays `false` when the key is a reference type (057) or a member disallows default (058)
- `AllowDefaultStructs` stays `false` when the type implements `IDisallowDefaultValue` by hand (080)
- `IDisallowDefaultValue` on a class warns (110)
- Choose a struct for a small value that is always valid, allow the default when it has a domain meaning (zero, an open end)
- Represent absence as `Option<T>` in place of a `null` class
- `SkipKeyMember = true` with `KeyMemberName` lets a nullable backing field map the CLR default to a domain value
- Hand-written `ToString()` sets `SkipToString` and `SkipIFormattable` together, the generated `IFormattable` formats the key on its own

Complex value objects compose simple value objects, smart enums, and other complex value objects:
- Each component keeps its own rule, the composite adds the rule that spans components
- `[ValueObject<TypeParamRef1>]` through `TypeParamRef5` bind the key to a type parameter that has a `notnull`, `struct`, or `class` constraint (074)
- Generated members follow the constraints, `where T : INumber<T>` yields parsing, comparison, formatting, and arithmetic together

## [02]-[SMART_ENUMS]

Smart enums declare a fixed set of items as `public static readonly` fields of a `partial` class under `[SmartEnum<TKey>]`, or under `[SmartEnum]` for a keyless set:
- Each item holds its own data and behavior
- Consumers call a method on the item in place of branching on it

```csharp
[SmartEnum<string>]
[ValidationError<UnknownKind>]
internal sealed partial class Kind {
    public static readonly Kind Standard = new("standard", rate: 0.5m, static amount => decimal.Round(amount, 2, MidpointRounding.ToEven));
    public static readonly Kind Express = new("express", rate: 0.75m, static amount => decimal.Round(amount, 2, MidpointRounding.ToPositiveInfinity));

    private readonly decimal _rate;

    public decimal Price(decimal weight) => weight * _rate;

    [UseDelegateFromConstructor]
    public partial decimal Round(decimal amount);

    static partial void ValidateConstructorArguments(ref string key, ref decimal rate) {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key must not be empty.", nameof(key));
        key = key.Trim().ToUpperInvariant();
    }
}
```

Generator emits one private constructor per base constructor with parameters in a fixed order: the key, the own fields and properties in declaration order, the base constructor parameters, and one delegate per `[UseDelegateFromConstructor]` method last. Declaration rules the analyzer and generator enforce:
- Items are `public static readonly` fields (002), static properties are not items (101), a set without items is 100, non-public items are rejected
- Two items with one key throw `ArgumentException` on the first lookup
- Instance fields and properties are read-only (001, 003, and 034, 035 on a plain base class), `[IgnoreMember]` hides a member from the generator
- Type has no primary constructor (043), and the generator seals a smart enum that declares no derived class
- `ValidateConstructorArguments` receives the key, the own members, and the base arguments by `ref`, not the delegates, and rejects by throwing
- `null` keys throw `ArgumentNullException` after `ValidateConstructorArguments` returns
- `[UseDelegateFromConstructor]` marks a `partial` method without type parameters (050, 051), a private delegate field implements it
- `DelegateName` or a parameter a `Func` cannot hold (`ref`) makes the generator emit a nested delegate type
- `Empty.Action` supplies the `Action` of an item without behavior
- Static fields initialize in declaration order, an item that refers to a later item reads it through a `Lazy<T>` built from a static method
- Derived classes nest inside the smart enum, first-level derived classes are `private` (014) and deeper ones `public` (015)
- Derived classes that are neither abstract nor a base are `sealed` (037), a derived class can be generic, `Items` lists the permitted implementations
- Keyless smart enums have no key member, `Get`, conversion operators, comparer settings, or generated `ToString`
- Only `[ObjectFactory<string>]` serializes or binds a keyless smart enum
- `ToString` overrides on a keyless smart enum supply the item name that `Switch`, `Map`, and Serilog otherwise render as the type name

### [02.1]-[GENERATED_API]

| [INDEX] | [MEMBER]                            | [BEHAVIOR]                                                                                      |
| :-----: | :---------------------------------- | :---------------------------------------------------------------------------------------------- |
|  [01]   | `Items`                             | Items in declaration order                                                                      |
|  [02]   | `Get(key)`                          | `null` for a `null` key, `UnknownSmartEnumIdentifierException` for an unknown key               |
|  [03]   | `TryGet(key, out item)`             | `false` for an unknown key                                                                      |
|  [04]   | `Validate(key, provider, out item)` | `null` or the error, a `null` key counts as unknown, `[ValidationError<T>]` types the error     |
|  [05]   | Conversions                         | To the key implicit and `null` for a `null` item, from the key explicit through `Get`           |
|  [06]   | Equality                            | Identity, `GetHashCode` computed once from the key with the configured comparer                 |
|  [07]   | `IParsable<T>`, `ISpanParsable<T>`  | Present when the key implements them, which includes `string`, `Parse` throws `FormatException` |
|  [08]   | `IComparable<T>`, `IFormattable`    | Present for a comparable or formattable key, with the comparison operators                      |
|  [09]   | `ToString()`, `[TypeConverter]`     | Key's string form, and `ThinktectureTypeConverter<T, TKey, TValidationError>`                   |

- `UnknownSmartEnumIdentifierException` is a `KeyNotFoundException` with the message `There is no item of type 'Kind' with the identifier 'nope'.`
- Lookups use a `FrozenDictionary`
- String keys gain span overloads of `Get`, `TryGet`, `Validate`, `Parse`, and `TryParse`
- `Items`, `Get`, `TryGet`, and `Validate` implement the static abstract members of `ISmartEnum<TKey, T, TValidationError>`
- Generic code reaches those members through the constraint

```csharp
internal static class Lookup {
    public static Option<T> Find<T, TKey>(TKey key) where T : ISmartEnum<TKey, T, ValidationError> where TKey : notnull =>
        T.TryGet(key, out T? item) ? Some(item) : None;
    public static Fin<T> Require<T, TKey, TError>(TKey key) where T : ISmartEnum<TKey, T, TError> where TKey : notnull where TError : Error, IValidationError<TError> =>
        T.Validate(key, CultureInfo.InvariantCulture, out T? item) is { } error ? error : item!;
}
```

### [02.2]-[COMPARERS_AND_SETTINGS]

String keys use `StringComparer.OrdinalIgnoreCase` for equality, the hash code, `CompareTo`, and the comparison operators, `TryGet("STANDARD")` finds `Standard`:
- Every other key uses its default comparer
- `[KeyMemberEqualityComparer<TAccessor, TKey>]` and `[KeyMemberComparer<TAccessor, TKey>]` replace them with the same accessors as a value object
- Accessors that do not match the key type are 041, 102 and 103 apply as for a value object
- String-keyed smart enums without comparer attributes compile without 048 and keep the case-insensitive default
- Span-based lookup uses the alternate lookup of `FrozenDictionary`, a predefined accessor gets `GetAlternateLookup<ReadOnlySpan<char>>()`
- Custom comparers without `IAlternateEqualityComparer<ReadOnlySpan<char>, string>` allocate a string per span call

`SkipIComparable` removes `IComparable` and `IComparable<T>` and leaves the comparison operators in place:
- Keyless attribute exposes only `EqualityComparisonOperators`, `SwitchMethods`, `MapMethods`, and `SwitchMapStateParameterName`
- `[SmartEnum<TypeParamRef1>]` binds the key to a `notnull` type parameter (074)
- Under `TypeParamRef1` the generator always emits `Get`, `TryGet`, `Validate`, `Items`, equality, `Switch`, `Map`, and the conversions
- Interfaces of a `TypeParamRef1` smart enum follow the constraints
- Smart enums model a closed set of named items with one shape, cases with different shapes are a union
- Items are `static readonly` fields that cannot serve as an attribute argument or a `case` label

## [03]-[UNIONS]

Ad hoc unions combine existing types that share no base, regular unions are class hierarchies where every case derives from one abstract partial base and holds its own properties and behavior, and smart enum items can return a union:

| [INDEX] | [ASPECT]    | [AD_HOC_UNION]                                          | [REGULAR_UNION]                                              |
| :-----: | :---------- | :------------------------------------------------------ | :----------------------------------------------------------- |
|  [01]   | Declaration | `partial class`, `partial struct`, `ref partial struct` | `partial class` or `partial record`, generated as `abstract` |
|  [02]   | Attribute   | `[Union<T1, T2>]` up to 5 types, or `[AdHocUnion]`      | `[Union]` on the base                                        |
|  [03]   | Cases       | Type arguments                                          | Nested types that derive from the base                       |
|  [04]   | Generic     | `TypeParamRef1` to `TypeParamRef5` name type parameters | Base can be generic, a case cannot (053)                     |

### [03.1]-[AD_HOC_UNIONS]

`[AdHocUnion]` with `typeof` exists for a member type a generic attribute cannot spell (`List<string?>`):
- Both forms generate `IsX` and `AsX` named after the member type (`IsString`, `AsInt32`) and `Value` as `object`
- `Normalize{Member}` partial hooks per stateful member run first in the generated constructor before any null check
- Equality, `ToString`, `Switch`, `Value`, and every serializer read the normalized value

```csharp
[Union<string, int>(T1Name = "Text", T2Name = "Count")]
internal sealed partial class TextOrCount {
    static partial void NormalizeText(ref string text) => text = text?.Trim() ?? "";
}
```

- `AsX` on the wrong member and the explicit cast throw `InvalidOperationException` (`'TextOrCount' is not of type 'string' but of type 'int'.`)
- Equality compares the discriminator and then the member value, `ToString` and `GetHashCode` delegate to the member
- `string` members compare with `OrdinalIgnoreCase` unless `DefaultStringComparison` says otherwise
- Members of type `object` or an interface receive a constructor and no operator
- Every member type is at least as accessible as the union (077), a union has at least two members (067) and one union attribute (066)
- `CreateX` factories replace the constructor for a member typed as a type parameter, an interface, `object`, or a duplicate of another member
- Type parameter members get no operator, `T` equal to another member's type makes every conversion ambiguous (`CS0457`)
- Interface arguments never apply an operator (`CS0029`), an `object` argument boxes the union or routes into the more specific member
- Hand-written operators for a type parameter member return `CreateT(value)` to keep normalization running
- `TypeParamRef` past the parameter count is 071, on a non-generic union 072, an `allows ref struct` parameter 073, no referenced parameter 107
- At most one reference-type member keeps typed fields, more share one `object?` field with value types unboxed
- `UseSingleBackingField` boxes everything into one field, `SingleBackingFieldType` names a base or interface for that field and `Value` (075, 079)
- Stateless members are `readonly record struct`s with `TxIsStateless = true`, the union stores the discriminator alone
- `AsX` of a stateless member returns `default(T)`, and its `CreateX` is parameterless
- `default` of a struct union has no member, 047 reports `default(TUnion)` and `new TUnion()`
- `Value`, `Switch`, `Map`, `ToString`, and `GetHashCode` throw on an uninitialized struct union
- `DefaultValueHandling = MapToFirstMember` turns `default` into a stateless first member (081, 082)
- Unions that add their own properties set `ConversionFromValue = None` and `ConstructorAccessModifier = Private`
- Hand-written constructors of such a union chain to the generated ones under `[SetsRequiredMembers]`

### [03.2]-[REGULAR_UNIONS]

Generator gives the base a private constructor, types declared outside it cannot derive from it:
- Class cases are `sealed` or keep private constructors (054), record cases are `sealed` (055)
- Non-abstract cases are no less accessible than the base (056), a nested type that does not derive from the base is 106
- Positional record cases are the natural form, abstract members hold behavior that needs no dependency
- Transitions that read context pass it through the `Switch` state overload

```csharp
[Union]
internal abstract partial record Phase {
    public abstract bool CanCancel();

    internal sealed record Open(string By) : Phase {
        public override bool CanCancel() => true;
    }
    internal sealed record Closed(Instant At, string Reference) : Phase {
        public override bool CanCancel() => false;
    }
}

internal sealed record CloseRequest(Instant Now, string Reference, bool Allowed);

internal static class Transitions {
    public static Phase Close(Phase phase, CloseRequest request) =>
        phase.Switch<CloseRequest, Phase>(request,
            open: static (close, open) => close.Allowed ? new Phase.Closed(close.Now, close.Reference) : open,
            closed: static (_, closed) => closed);
}
```

- Cases with a single-parameter constructor of a type unique among the cases get an implicit conversion from that type to the base
- `ConversionFromValue = None` on `[Union]` removes the operators
- Class cases with `[Union]` become nested unions with their own cases, records cannot nest a union
- Outer `Switch` prefixes nested arm names with the parent (`failureNotFound`)
- `NestedUnionParameterNames = Simple` drops the prefix and collides when two nested unions declare a case with one name
- `[UnionSwitchMapOverload(StopAt = [typeof(Nested)])]` adds a non-exhaustive overload that delegates the nested union to its own `Switch`
- Cases can be value objects or smart enums, the union names the kind and each case owns its value and rules
- `Unknown` cases are a `[ComplexValueObject(SkipFactoryMethods = true)]` with one `Instance` in place of `null`
- Shared data sits on the base with a private constructor that the record cases pass it to
- Hand-written operators on the base can accept an external type

## [04]-[SWITCH_AND_MAP]

Smart enums and unions generate `Switch` with one `Action` per case, `Switch<TResult>` with one `Func` per case, and `Map<TResult>` with one value per case:
- Every argument is named after its case in camel case (046), every lambda is `static` (1001)
- Captured context enters through the state overloads that take `TState` first and pass it to every lambda
- State parameter is named `state` unless `SwitchMapStateParameterName` renames it, `TState : allows ref struct` holds
- When the arms return different but compatible types an explicit `TResult` on the call moves the error to the one arm that disagrees

```csharp
internal static class Matching {
    public static string Label(Kind kind, decimal weight) =>
        kind.Switch(
            weight,
            standard: static w => string.Create(CultureInfo.InvariantCulture, $"ground, {w} kg"),
            express: static w => string.Create(CultureInfo.InvariantCulture, $"air, {w} kg"));
    public static string Handling(Kind kind) => kind.MapPartially(@default: "standard", express: "priority");
}
```

- `SwitchPartially` and `MapPartially` exist only under `SwitchMethods` and `MapMethods` set to `DefaultWithPartialOverloads`
- Void `SwitchPartially` takes an optional `@default` and does nothing for an unhandled case, the value-returning forms require `@default`
- On an ad hoc union `@default` receives the current member as `object?`, on a regular union the base type
- Partial forms serve an intended fallback alone, the exhaustive form turns a new case into a compile error at every call
- Every generated `Switch` and `Map` ends in an unreachable arm that throws `InvalidOperationException`
- Smart enum arm message is `Unknown item 'Rogue'.`, regular union arm message is `Unexpected type '...'.`
- Ad hoc `Map` arm message is `Unexpected value index '...'.`, ad hoc `Switch` throws `IndexOutOfRangeException` in that arm

## [05]-[OBJECT_FACTORIES]

`[ObjectFactory<T>]` declares a conversion between a type and one other type `T` on a smart enum, a value object, a union, or a plain partial type:
- Generator adds `IObjectFactory<TSelf, T, ValidationError>` and demands one static method (061)
- `string` factories add `IParsable<TSelf>`

```text
static ValidationError? Validate(T? value, IFormatProvider? provider, out TSelf? item)
```

- Method returns `null` and sets `item` on success, and returns the error with a `null` item on failure
- `null` input sets a `null` item and returns `null`, no serializer or model binder passes it, `Parse` returns `null`
- Factories on a keyed type or a complex value object delegate to the generated `Validate`, the hook normalizes once for both paths
- Factories with `T` equal to the key type collide with the generated overload
- Factories are one-way until `UseForSerialization` other than `None` or `UseWithEntityFramework = true` makes them two-way
- Two-way factories add `IConvertible<T>` and demand an instance `T ToValue()` (062)
- `UseForSerialization` is a flags enum (`SystemTextJson`, `NewtonsoftJson`, `Json` for both, `MessagePack`, `All`)
- `UseForModelBinding = true` binds from one route, query, header, or form value
- `HasCorrespondingConstructor = true` declares a one-`T` constructor for the Entity Framework Core read path
- Flags replace the key-based conversion of a keyed smart enum or simple value object at that integration point
- Flags enable a conversion a complex value object or a union lacks otherwise, and the flags register nothing at the host
- Each integration point belongs to at most one factory
- `SkipFactoryMethods = true` on a value object removes its converters until a factory with `UseForSerialization` restores them

Ad hoc unions serialize no discriminator, a `string` factory is their one wire format, `Validate` assigns a member through the implicit conversion and `ToValue` renders the active case through `Switch`:

```csharp
[Union<string, int>(T1Name = "Text", T2Name = "Count")]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.All, UseForModelBinding = true)]
internal sealed partial class TextOrCount {
    public static ValidationError? Validate(string? value, IFormatProvider? provider, out TextOrCount? item) {
        item = null;
        if (value is null) return null;
        if (value.StartsWith("text:", StringComparison.Ordinal)) item = value["text:".Length..];
        else if (value.StartsWith("count:", StringComparison.Ordinal) && int.TryParse(value["count:".Length..], NumberStyles.Integer, CultureInfo.InvariantCulture, out int count)) item = count;
        return item is null ? new ValidationError($"Unknown text-or-count '{value}'") : null;
    }

    public string ToValue() => Switch(
        text: static text => $"text:{text}",
        count: static count => string.Create(CultureInfo.InvariantCulture, $"count:{count}"));
}
```

- Invalid text throws `JsonException` with the error message, `Parse` throws `FormatException` with the same message, `TryParse` returns `false`
- JSON `null` deserializes to `null` without a `Validate` call

## [06]-[CONVENIENCE_MEMBERS]

Package supplies plain static members beside its generators, and they stay at the BCL boundary, domain code uses `Seq<A>()`, `Seq(x)`, `Set(x)`, and `toSeq` in their place:
- `Thinktecture.Empty.Action` converts to every `Action` delegate up to 16 parameters
- `Empty.Disposable()` and `Empty.AsyncDisposable()` return cached no-op instances
- `Empty.Collection<T>()`, `Empty.Dictionary<TKey, TValue>()`, `Empty.Lookup<TKey, TValue>()`, and `Empty.Set<T>()` return cached read-only empties
- Empties follow the argument rules of the BCL collections, a `null` key throws on the dictionary
- `SingleItem.Set` and `SingleItem.Dictionary` build a read-only collection around one item with an optional comparer
- `SingleItem.Lookup` takes one key with its element sequence and enumerates that sequence live
- `ToReadOnlyCollection(count)` wraps a sequence without enumerating it and trusts the caller's count
- `ToReadOnlyCollection(selector)` composes a projection with the source count and reruns the selector on every enumeration
- `TrimOrNullify()` returns `null` for blank text and the trimmed text otherwise, `TrimOrNullify(maxLength)` cuts the trimmed text by `char` count
- `TrimOrNullify` sits inside a validation hook that assigns the result back to `value`, an absent domain value is `Option<string>` through `Optional`
- `Thinktecture.Collections.ProjectionEqualityComparer<T, TItem>` compares projections
- `StringKeyedObjectComparer<T>` compares `ToValue()` of any `IConvertible<string>` type with its `Ordinal` and culture fields
- `StringKeyedObjectComparer<T>.Ordinal` restores exact matches for one collection of a case-insensitive type

## [07]-[FRAMEWORK_INTEGRATION]

Simple value objects and keyed smart enums cross every boundary as their key, complex value objects cross JSON and MessagePack as objects with their members, and every type crosses a boundary as one value through an object factory:

| [INDEX] | [INTEGRATION]                  | [PACKAGE]               | [REGISTRATION]                                                          |
| :-----: | :----------------------------- | :---------------------- | :---------------------------------------------------------------------- |
|  [01]   | `System.Text.Json`             | `Json`                  | Referenced by the declaring project, the converter attribute is emitted |
|  [02]   | `System.Text.Json` at the host | `Json`                  | `options.Converters.Add(new ThinktectureJsonConverterFactory())`        |
|  [03]   | `Newtonsoft.Json`              | `Newtonsoft.Json`       | `ThinktectureNewtonsoftJsonConverterFactory`                            |
|  [04]   | MessagePack                    | `MessagePack`           | Generated formatter, or `ThinktectureMessageFormatterResolver.Instance` |
|  [05]   | MVC model binding              | `AspNetCore`            | `ModelBinderProviders.Insert(0, new ThinktectureModelBinderProvider())` |
|  [06]   | OpenAPI                        | `Swashbuckle`           | `services.AddThinktectureOpenApiFilters()`                              |
|  [07]   | Entity Framework Core          | `EntityFrameworkCore10` | `optionsBuilder.UseThinktectureValueConverters()`                       |
|  [08]   | Serilog                        | `Serilog`               | `Destructure.UsingThinktectureRuntimeExtensions()`                      |

- Declaring project references `Json` and receives the `[JsonConverter]` attribute
- Projects that cannot reference `Json` register the converter factory at the host
- MVC reads `AddControllers().AddJsonOptions`, minimal APIs read `ConfigureHttpJsonOptions`
- Factory constructor `(bool skipObjectsWithJsonConverterAttribute, Func<Type, bool>? skipSpanBasedDeserialization)` skips attributed types
- `skipSpanBasedDeserialization` opts single types out of span-based reads
- Unknown keys and rejected values on read throw `JsonException` with the validation text
- String keys read through a span-based converter
- Regular unions need one `[JsonDerivedType]` on the base per case or a `[ObjectFactory<string>]` on the base, MessagePack has no integration for them
- Newtonsoft `TypeNameHandling` reads a regular union and is a deserialization risk unless the binder restricts the types
- Minimal APIs bind through `IParsable<T>.TryParse` and answer a failed bind with a plain 400
- MVC runs `Validate`, writes the error into `ModelState`, and `[ApiController]` answers 400 with the text
- Binder provider goes in front of the default providers with `skipBindingFromBody` at its default `true`
- `AddThinktectureOpenApiFilters` renders a value object as its key or its members and a smart enum as its key with the allowed values
- `SmartEnumSchemaFilter` selects `Default`, `OneOf`, `AnyOf`, `AllOf`, or `FromDependencyInjection`
- `SmartEnumSchemaExtension` adds `x-enum-varnames`
- `RequiredMemberEvaluator` marks a member implementing `IDisallowDefaultValue` or a non-nullable reference member as required
- `RequiredMemberEvaluator.All` and `None` override the default evaluation
- Entity Framework Core stores a keyed type in one column of the key type
- `UseThinktectureValueConverters` applies to every context on the options
- `AddThinktectureValueConverters` narrows to a model, entity, owned, or complex builder, `HasThinktectureValueConverter` to one property
- Complex value objects map as a complex property or an owned type
- Regular unions map as table-per-hierarchy through `HasDiscriminator<string>` with one `HasValue<TCase>` per case, or as table-per-type
- Cases with one property name share a column through `HasColumnName`

## [08]-[ANTI_PATTERNS]

| [INDEX] | [WRONG_FORM]                                                        | [CORRECT_FORM]                                                   |
| :-----: | :------------------------------------------------------------------ | :--------------------------------------------------------------- |
|  [01]   | `throw` inside the hook, which skips `TryCreate` and the frameworks | Assign `validationError` and `return`                            |
|  [02]   | Hooks that trim into a local and never assign `value`               | `value = trimmed`                                                |
|  [03]   | `value.Trim().ToUpper()` in a hook depends on the current culture   | `value.Trim().ToUpperInvariant()`                                |
|  [04]   | `[ValueObject<string>]` without comparer attributes                 | Both `[KeyMemberEqualityComparer]` and `[KeyMemberComparer]`     |
|  [05]   | `TrimOrNullify(maxLength)` as a length rule in a hook               | Reject the over-long input, a cut merges distinct inputs         |
|  [06]   | `HasConversion` with a lambda that calls `Create`                   | `HasThinktectureValueConverter()` or the converter registration  |
|  [07]   | Host converter factory for a complex value object                   | `Json` referenced by the declaring project, or an object factory |
|  [08]   | Native `switch` with `_ =>` over a smart enum or union              | Generated `Switch` or `Map`                                      |
|  [09]   | Lambdas without `static` in a `Switch` arm                          | State overload with a `static` lambda                            |
|  [10]   | `SwitchPartially` where every case matters                          | Exhaustive `Switch`                                              |
|  [11]   | `default(TUnion)` or `new TUnion()` on a struct union               | Member values, or `MapToFirstMember` with a stateless first case |
|  [12]   | Stateless markers as classes                                        | `readonly record struct`                                         |
|  [13]   | `string` failure case beside a `string` success value               | One distinct type per case                                       |
|  [14]   | Hand-written serializers for an ad hoc union                        | `[ObjectFactory<string>]` with `ToValue` and `Validate`          |
|  [15]   | `new List<T>()` as an empty `IReadOnlyList<T>` at the BCL boundary  | `Thinktecture.Empty.Collection<T>()`                             |
|  [16]   | `Fin<T>` or `Validation<Error, T>` adapters that call `Create`      | `Validate`, which never throws                                   |
