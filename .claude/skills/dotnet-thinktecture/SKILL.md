---
name: dotnet-thinktecture
description: "ENTER LATER AFTER FINISHED, <20-25 WORDS MAX"
---

# [DOTNET_THINKTECTURE]

Covers declaring the types that `Thinktecture.Runtime.Extensions` generates (value objects, smart enums, ad hoc and regular unions), their generated API and settings, the validation and normalization hooks, `Switch` and `Map`, object factories for a second wire format, the convenience members, and the packages that carry those types across JSON, MessagePack, model binding, OpenAPI, Entity Framework Core, and Serilog. Which type a domain concept becomes, how a union is designed, and how `Validate` maps to `Fin<T>` are decisions that `dotnet-coding` states, and the `Expected` records that `[ValidationError<T>]` names take their shape from `dotnet-languageext`.

[REFERENCES]:
- [01]-[SETTINGS](references/settings.md): Attribute settings of value objects, smart enums, and ad hoc unions with defaults and effects
- [02]-[FACTORY_PATHS](references/factory-paths.md): Entity Framework Core read path, span-based JSON, multiple factories, runtime factory selection, polymorphic discriminators
- [03]-[SERILOG](references/serilog.md): Destructuring policy, depth limits, string rendering, caveats

Every package name omits the prefix `Thinktecture.Runtime.Extensions.`, every analyzer code omits the prefix `TTRESG`, and each analyzer rule fails the build.

## [01]-[GENERATOR_CONFIGURATION]

The generator reads project-level MSBuild properties, each with the prefix `ThinktectureRuntimeExtensions_SourceGenerator_` and forwarded to the compiler as `build_property.<PropertyName>`, and they apply to every generated type in the project:

| [INDEX] | [PROPERTY]                     | [VALUES]                                                                      | [DEFAULT] |
| :-----: | :----------------------------- | :---------------------------------------------------------------------------- | :-------- |
|   [01]  | `LogFilePath`                  | File or folder path, trimmed                                                  | No log    |
|   [02]  | `LogFilePathMustBeUnique`      | `true` or `false`                                                             | `true`    |
|   [03]  | `LogLevel`                     | `Trace`, `Debug`, `Information`, `Warning`, `Error`, `None`, case-insensitive | `Warning` |
|   [04]  | `LogMessageInitialBufferSize`  | Integer of at least 100                                                       | `100`     |
|   [05]  | `GenerateJetBrainsAnnotations` | `disable`, `disabled`, `false`, or `0` turn it off, case-insensitive          | On        |
|   [06]  | `Counter`                      | `enable`, `enabled`, `true`, or `1` turn it on, case-insensitive              | Off       |

- Keep the properties out of the committed project file, pass them with `-p:` for one diagnostic build, or keep them in a local ignored props file
- `LogFilePath` gates the other logging properties, must name a folder that exists before the build, and blank disables file logging
- `LogLevel` at `Information` shows the generator run and which serialization generators participate, and only `Information`, `Warning`, and `Error` create a file logger
- `LogFilePathMustBeUnique` at `false` collects every compiler process in one file, and the default `true` names a new file per process with a UTC timestamp and a guid
- Leave `GenerateJetBrainsAnnotations` unset, because the generator skips the annotation file when `JetBrains.Annotations.dll` is referenced, and turning it off elsewhere fails with `CS0122` on every `Switch` delegate parameter
- Use `Counter` only to detect regeneration (every emitted file starts with `// COUNTER: <n>`), and turn it off before generated files are compared or committed

## [02]-[VALUE_OBJECTS]

Simple value objects wrap one key member under `[ValueObject<TKey>]`, complex value objects hold read-only members under `[ComplexValueObject]`, both are `partial`, the generator adds `sealed` to a class and `readonly` to a struct and owns the private constructor, and the hand-written part is the validation hook and the domain behavior:

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
[ValidationError<InvalidInterval>]
internal sealed partial class Interval {
    public decimal Lower { get; }
    public decimal Upper { get; }

    static partial void ValidateFactoryArguments(ref InvalidInterval? validationError, ref decimal lower, ref decimal upper) {
        if (lower > upper) {
            validationError = new InvalidInterval();
            return;
        }
        lower = Math.Round(lower, 2, MidpointRounding.ToEven);
        upper = Math.Round(upper, 2, MidpointRounding.ToEven);
    }
}
```

The declaration rules the analyzer enforces:
- Every field is read-only (001), every property has no setter (003), an `init` accessor is private (042), a primary constructor is rejected (043), and the key member is non-nullable
- String keys carry both comparer attributes (048), a complex value object with string members sets `DefaultStringComparison` (049), and `[IgnoreMember]` removes a member from equality, the factories, and every other generated member
- The complex form accepts one member or none, and with one member it receives no key-derived members
- `[ValidationError<T>]` switches the hook parameter, the second `TryCreate` overload, and `Validate` to a type that implements `IValidationError<T>` with `static abstract T Create(string message)`, which the generator calls for its own errors, and `ToString()` of that type is the text that reaches `ValidationException`, `FormatException`, the JSON converters, and model state

### [02.1]-[GENERATED_API]

| [INDEX] | [MEMBER]                             | [BEHAVIOR]                                                                              |
| :-----: | :----------------------------------- | :-------------------------------------------------------------------------------------- |
|   [01]  | `Create(value)`                      | Validates and returns the instance, or throws `ValidationException` with the error text |
|   [02]  | `TryCreate(value, out obj)`          | Returns `false` on rejection, the 3-parameter overload also returns the error           |
|   [03]  | `Validate(value, provider, out obj)` | Returns the error or `null` and never throws, the complex form has no provider          |
|   [04]  | Equality, `GetHashCode`, `==`, `!=`  | Run through the configured comparer                                                     |
|   [05]  | `ToString()`                         | The key's `ToString()`, or `{ Lower = 1.23, Upper = 2.57 }` for the complex form        |
|   [06]  | `IComparable<T>`, `IFormattable`     | Present when the key is comparable or formattable, simple form only                     |
|   [07]  | `IParsable<T>`, `ISpanParsable<T>`   | Present when the key is parsable or a `string`, `Parse` throws `FormatException`        |
|   [08]  | Conversions                          | To the key implicit, from the key explicit through `Create`, unsafe to a value-type key |
|   [09]  | `[TypeConverter]`                    | Emitted on every simple value object that has factory methods                           |

Complex value objects take one argument per member in declaration order, a `null` argument for a non-nullable key or member returns an error before the hook runs, and the hook never repeats that null check.

### [02.2]-[HOOK]

`ValidateFactoryArguments` is `static partial void` with `ref TError? validationError` first and the key or each member by `ref` in declaration order, it rejects by assigning the error and returning, it normalizes by assigning the `ref` parameter, the compiler erases an absent hook, and every entry point runs it: `Create`, `TryCreate`, `Validate`, the conversion from the key, `Parse`, the JSON converters, the MessagePack formatter, and model binding. The hook reports the first violated rule over one value, and independent rules over several inputs accumulate at the input boundary. `ValidateConstructorArguments(ref TKey value)` exists beside it and rejects by throwing alone.

Trailing parameters after the members are declared by value without a default (076), and the generator then emits `private static TError? ValidateCore(members, extras, out T? obj)` and `private static T CreateCore(members, extras)`, where the public `Validate` passes `default` for every extra and a hand-written factory delegates to `CreateCore`:

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

Rounding runs once inside the hook whichever factory is called, and multiplication by `decimal` is disabled because the product needs a rounding decision. A hook declared `private static partial string ValidateFactoryArguments(...)` returns a value that the generated `Validate` passes to `partial void FactoryPostInit(string value)` on the constructed instance after validation succeeded, the receiving field carries `[IgnoreMember]` and an initializer, and a `readonly` struct cannot hold it, so that form belongs to a class.

### [02.3]-[COMPARERS_AND_SETTINGS]

String keys compare with `StringComparer.OrdinalIgnoreCase` by default and every other key with its own `Equals`. `[KeyMemberEqualityComparer<TAccessor, TKey>]` selects the equality comparer, `[KeyMemberComparer<TAccessor, TKey>]` selects the ordering comparer for `IComparable<T>` and the comparison operators on the simple form alone, a comparer without an equality comparer is 102, an equality comparer without a comparer is 103 when the key is comparable and `SkipIComparable` is not set, and the accessors are `ComparerAccessors.StringOrdinal`, `StringOrdinalIgnoreCase`, `CurrentCulture`, `CurrentCultureIgnoreCase`, `InvariantCulture`, `InvariantCultureIgnoreCase`, and `Default<T>`, where a custom accessor implements `IEqualityComparerAccessor<T>` or `IComparerAccessor<T>` with one static property. Complex value objects compare every assignable member, `DefaultStringComparison` sets the comparison of their string members, and `[MemberEqualityComparer<TAccessor, TMember>]` on one member changes its comparer and drops every unattributed member out of equality and hashing.

The attribute settings are listed in `references/settings.md`. `DefaultWithKeyTypeOverloads` adds overloads with the key type in both operand positions, so `amount > 42m` compiles without a conversion, and the generator emits `operator checked +` beside the unchecked form when the key declares it. Struct value objects reject `default(T)` and `new T()` through `IDisallowDefaultValue` (047), a settable property of the type elsewhere warns until it is `required` (104), `AllowDefaultStructs` stays `false` when the key is a reference type (057), a member disallows default (058), or the type implements `IDisallowDefaultValue` by hand (080), and `IDisallowDefaultValue` on a class warns (110). Choose a struct for a small value that is always valid, allow the default when it has a domain meaning (zero, an open end), and represent absence as `Option<T>` rather than a `null` class. `SkipKeyMember = true` with `KeyMemberName` lets a nullable backing field map the CLR default to a domain value, and a hand-written `ToString()` sets `SkipToString` and `SkipIFormattable` together, because the generated `IFormattable` still formats the key.

Complex value objects compose simple value objects, smart enums, and other complex value objects, each component keeps its own rule, and the composite adds the rule that spans components. `[ValueObject<TypeParamRef1>]` through `TypeParamRef5` bind the key to a type parameter that carries a `notnull`, `struct`, or `class` constraint (074), and the generated surface follows the constraints, so `where T : INumber<T>` yields parsing, comparison, formatting, and arithmetic together.

## [03]-[SMART_ENUMS]

Smart enums declare a fixed set of items as `public static readonly` fields of a `partial` class under `[SmartEnum<TKey>]`, or under `[SmartEnum]` for a keyless set, each item holds its own data and behavior, and a consumer calls a method on the item in place of branching on it:

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

The generator emits one private constructor per base constructor, and its parameters arrive in a fixed order: the key, the own fields and properties in declaration order, the base constructor parameters, and one delegate per `[UseDelegateFromConstructor]` method last. The rules:
- Items are `public static readonly` fields (002), static properties are not items (101), a set without items is 100, non-public items are rejected, and two items with the same key throw `ArgumentException` on the first lookup
- Instance fields and properties are read-only (001, 003), `[IgnoreMember]` hides a member from the generator, the type has no primary constructor (043), and every enclosing type of a nested smart enum is `partial`
- `ValidateConstructorArguments` receives the key, the own members, and the base arguments by `ref` and not the delegates, rejects by throwing alone, and a `null` key throws `ArgumentNullException` after it returns
- `[UseDelegateFromConstructor]` marks a `partial` method without type parameters (050, 051), the generator adds a private delegate field and implements the method through it, `DelegateName` or a parameter a `Func` cannot carry (`ref`) makes it emit a nested delegate type
- Static fields initialize in declaration order, so an item that refers to a later item reads it through a `Lazy<T>` built from a static method
- Derived classes nest inside the smart enum, first-level derived classes are `private` (014) and deeper ones `public` (015), a derived class that is neither abstract nor a base is `sealed` (037), a derived class can be generic, and `Items` doubles as the list of permitted implementations
- Keyless smart enums have no key member, `Get`, conversion operators, comparer settings, or generated `ToString`, only `[ObjectFactory<string>]` serializes or binds them, and a `ToString` override supplies the item name that `Switch`, `Map`, and Serilog otherwise render as the type name

### [03.1]-[GENERATED_API]

| [INDEX] | [MEMBER]                            | [BEHAVIOR]                                                                                      |
| :-----: | :---------------------------------- | :---------------------------------------------------------------------------------------------- |
|   [01]  | `Items`                             | The items in declaration order                                                                  |
|   [02]  | `Get(key)`                          | `null` for a `null` key, `UnknownSmartEnumIdentifierException` for an unknown key               |
|   [03]  | `TryGet(key, out item)`             | `false` for an unknown key                                                                      |
|   [04]  | `Validate(key, provider, out item)` | `null` or the error, a `null` key counts as unknown, `[ValidationError<T>]` types the error     |
|   [05]  | Conversions                         | To the key implicit and `null` for a `null` item, from the key explicit through `Get`           |
|   [06]  | Equality                            | Identity, `GetHashCode` computed once from the key with the configured comparer                 |
|   [07]  | `IParsable<T>`, `ISpanParsable<T>`  | Present when the key implements them, which includes `string`, `Parse` throws `FormatException` |
|   [08]  | `IComparable<T>`, `IFormattable`    | Present for a comparable or formattable key, with the comparison operators                      |
|   [09]  | `ToString()`, `[TypeConverter]`     | The key's string form, and `ThinktectureTypeConverter<T, TKey, TValidationError>`               |

`UnknownSmartEnumIdentifierException` is a `KeyNotFoundException` with the message `There is no item of type 'Kind' with the identifier 'nope'.`, lookups use a `FrozenDictionary`, string keys gain span overloads of `Get`, `TryGet`, `Validate`, `Parse`, and `TryParse`, and `Items`, `Get`, `TryGet`, and `Validate` implement the static abstract members of `ISmartEnum<TKey, T, TValidationError>`, which generic code reaches through the constraint:

```csharp
internal static class Lookup {
    public static Option<T> Find<T, TKey>(TKey key) where T : ISmartEnum<TKey, T, ValidationError> where TKey : notnull =>
        T.TryGet(key, out T? item) ? Some(item) : None;
    public static Fin<T> Require<T, TKey, TError>(TKey key) where T : ISmartEnum<TKey, T, TError> where TKey : notnull where TError : Error, IValidationError<TError> =>
        T.Validate(key, CultureInfo.InvariantCulture, out T? item) is { } error ? error : item!;
}
```

### [03.2]-[COMPARERS_AND_SETTINGS]

String keys use `StringComparer.OrdinalIgnoreCase` for equality, the hash code, `CompareTo`, and the comparison operators, so `TryGet("STANDARD")` finds `Standard`, every other key uses its default comparer, `[KeyMemberEqualityComparer<TAccessor, TKey>]` and `[KeyMemberComparer<TAccessor, TKey>]` replace them with the same accessors as a value object, an accessor that does not match the key type is 041, 102 and 103 apply as for a value object, and a string-keyed smart enum without comparer attributes compiles without 048 and keeps the case-insensitive default. The span-based lookup uses the alternate lookup of `FrozenDictionary`, a predefined accessor gets `GetAlternateLookup<ReadOnlySpan<char>>()`, and a custom comparer without `IAlternateEqualityComparer<ReadOnlySpan<char>, string>` allocates a string per span call.

The attribute settings are listed in `references/settings.md`. `SkipIComparable` removes `IComparable` and `IComparable<T>` and leaves the comparison operators in place, the keyless attribute exposes only `EqualityComparisonOperators`, `SwitchMethods`, `MapMethods`, and `SwitchMapStateParameterName`, and `[SmartEnum<TypeParamRef1>]` binds the key to a `notnull` type parameter (074) where `Get`, `TryGet`, `Validate`, `Items`, equality, `Switch`, `Map`, and the conversions are always generated and the interfaces follow the constraints. Smart enums model a closed set of named items with one shape, cases with different shapes are a union, and items are `static readonly` fields that cannot serve as an attribute argument or a `case` label.

## [04]-[UNIONS]

Ad hoc unions combine existing types that share no base, regular unions are class hierarchies where every case derives from one abstract partial base and carries its own properties and behavior, and smart enum items can return a union:

| [INDEX] | [ASPECT]    | [AD_HOC_UNION]                                          | [REGULAR_UNION]                                              |
| :-----: | :---------- | :------------------------------------------------------ | :----------------------------------------------------------- |
|   [01]  | Declaration | `partial class`, `partial struct`, `ref partial struct` | `partial class` or `partial record`, generated as `abstract` |
|   [02]  | Attribute   | `[Union<T1, T2>]` up to 5 types, or `[AdHocUnion]`      | `[Union]` on the base                                        |
|   [03]  | Cases       | The type arguments                                      | Nested types that derive from the base                       |
|   [04]  | Generic     | `TypeParamRef1` to `TypeParamRef5` name type parameters | The base can be generic, a case cannot (053)                 |

### [04.1]-[AD_HOC_UNIONS]

`[AdHocUnion]` with `typeof` exists for a member type a generic attribute cannot spell (`List<string?>`), both forms generate `IsX` and `AsX` named after the member type (`IsString`, `AsInt32`), `Value` as `object`, and a `Normalize{Member}` partial hook per stateful member that runs first in the generated constructor before any null check, so equality, `ToString`, `Switch`, `Value`, and every serializer read the normalized value:

```csharp
[Union<string, int>(T1Name = "Text", T2Name = "Count")]
internal sealed partial class TextOrCount {
    static partial void NormalizeText(ref string text) => text = text?.Trim() ?? "";
}
```

- `AsX` on the wrong member and the explicit cast throw `InvalidOperationException` (`'TextOrCount' is not of type 'string' but of type 'int'.`)
- Equality compares the discriminator and then the member value, `string` members compare with `OrdinalIgnoreCase` unless `DefaultStringComparison` says otherwise, and `ToString` and `GetHashCode` delegate to the member
- Members of type `object` or an interface receive a constructor and no operator, every member type is at least as accessible as the union (077), and a union has at least 2 members (067) and one union attribute (066)
- `CreateX` factories replace the constructor for a member typed as a type parameter, an interface, `object`, or a duplicate of another member, and a hand-written operator for a type parameter member returns `CreateT(value)` so normalization still runs
- At most one reference-type member keeps typed fields, 2 or more share one `object?` field with value types unboxed, `UseSingleBackingField` boxes everything into one field, and `SingleBackingFieldType` names a base or interface for that field and for `Value` (075, 079)
- Stateless members are `readonly record struct`s with `TxIsStateless = true`, the union stores only the discriminator, `AsX` returns `default(T)`, and `CreateX` is parameterless
- `default` of a struct union has no member, 047 reports `default(TUnion)` and `new TUnion()`, `Value`, `Switch`, `Map`, `ToString`, and `GetHashCode` throw at runtime, and `DefaultValueHandling = MapToFirstMember` turns `default` into a stateless first member (081, 082)
- Unions that add their own properties set `ConversionFromValue = None` and `ConstructorAccessModifier = Private`, and their hand-written constructors chain to the generated ones under `[SetsRequiredMembers]`

The attribute settings are listed in `references/settings.md`.

### [04.2]-[REGULAR_UNIONS]

The generator gives the base a private constructor, so types declared outside it cannot derive from it, class cases are `sealed` or keep private constructors (054), record cases are `sealed` (055), a non-abstract case is no less accessible than the base (056), a nested type that does not derive from the base is 106, positional record cases are the natural form, abstract members hold behavior that needs no dependency, and a transition that reads context passes it through the `Switch` state overload:

```csharp
[Union]
internal abstract partial record Phase {
    public abstract bool CanCancel();

    internal sealed record Open(string By) : Phase {
        public override bool CanCancel() => true;
    }
    internal sealed record Closed(DateTime At, string Reference) : Phase {
        public override bool CanCancel() => false;
    }
}

internal sealed record CloseRequest(DateTime Now, string Reference, bool Allowed);

internal static class Transitions {
    public static Phase Close(Phase phase, CloseRequest request) =>
        phase.Switch<CloseRequest, Phase>(request,
            open: static (close, open) => close.Allowed ? new Phase.Closed(close.Now, close.Reference) : open,
            closed: static (_, closed) => closed);
}
```

- A case with a single-parameter constructor of a type unique among the cases gets an implicit conversion from that type to the base, and `ConversionFromValue = None` on `[Union]` removes those operators
- Class cases carrying `[Union]` become nested unions with their own cases, records cannot nest a union, the outer `Switch` prefixes nested arm names with the parent (`failureNotFound`), `NestedUnionParameterNames = Simple` drops the prefix, and `[UnionSwitchMapOverload(StopAt = [typeof(Nested)])]` adds a non-exhaustive overload that delegates the nested union to its own `Switch`
- Cases can be value objects or smart enums, the union names the kind and each case owns its value and rules, and an `Unknown` case is a `[ComplexValueObject(SkipFactoryMethods = true)]` with one `Instance` rather than `null`
- Shared data sits on the base with a private constructor that the record cases pass it to, and a hand-written operator on the base can accept an external type

## [05]-[SWITCH_AND_MAP]

Smart enums and unions generate `Switch` with one `Action` per case, `Switch<TResult>` with one `Func` per case, and `Map<TResult>` with one value per case, every argument is named after its case in camel case (046), every lambda is `static` (1001), captured context travels through the state overloads that take `TState` first and hand it to every lambda, the state parameter is named `state` unless `SwitchMapStateParameterName` renames it, `TState : allows ref struct` holds, and when the arms return different but compatible types an explicit `TResult` on the call moves the error to the one arm that disagrees:

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

`SwitchPartially` and `MapPartially` exist only under `SwitchMethods` and `MapMethods` set to `DefaultWithPartialOverloads`, the void `SwitchPartially` takes an optional `@default` and does nothing for an unhandled case, the value-returning forms require `@default`, on an ad hoc union `@default` receives the current member as `object?` and on a regular union the base type, and the partial forms serve an intended fallback only, because the exhaustive form turns a new case into a compile error at every call. Every generated `Switch` and `Map` ends in an unreachable arm that throws `InvalidOperationException` (`Unknown item 'Rogue'.` on a smart enum, `Unexpected type '...'.` on a regular union, `Unexpected value index '...'.` on an ad hoc union, where the ad hoc `Switch` throws `IndexOutOfRangeException`).

## [06]-[OBJECT_FACTORIES]

`[ObjectFactory<T>]` declares a conversion between a type and one other type `T` on a smart enum, a value object, a union, or a plain partial type, the generator adds `IObjectFactory<TSelf, T, ValidationError>` and demands one static method, and a `string` factory also adds `IParsable<TSelf>`:

```text
static ValidationError? Validate(T? value, IFormatProvider? provider, out TSelf? item)
```

- The method returns `null` and sets `item` on success, returns the error and a `null` item on failure, and `null` input sets a `null` item and returns `null`, which no serializer or model binder passes and which makes `Parse` return `null` and an Entity Framework Core read throw
- Factories on a keyed type or a complex value object delegate to the generated `Validate` of the key or the members, so normalization in the hook runs once for both paths, and a factory with `T` equal to the key type collides with the generated overload
- Factories are one-way until `UseForSerialization` other than `None` or `UseWithEntityFramework = true` makes them two-way, adds `IConvertible<T>`, and demands an instance `T ToValue()`
- `UseForSerialization` is a flags enum (`SystemTextJson`, `NewtonsoftJson`, `Json` for both, `MessagePack`, `All`), `UseForModelBinding = true` binds from one route, query, header, or form value, and `HasCorrespondingConstructor = true` declares a one-`T` constructor that Entity Framework Core reads through without `Validate` (059, and 060 on a smart enum)
- For a keyed smart enum or a simple value object a flag replaces the key-based conversion at that integration point, for a complex value object or a union it enables a conversion that does not exist otherwise, and the flags register nothing at the host
- Each integration point belongs to at most one factory (068, 069, 070), a keyless smart enum serializes and binds through a factory alone, and `SkipFactoryMethods = true` on a value object removes its converters until a factory with `UseForSerialization` restores them

An ad hoc union carries no discriminator, so a `string` factory is its one wire format, where `Validate` assigns a member through the implicit conversion and `ToValue` renders the active case through `Switch`:

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

Invalid text surfaces as `JsonException` with the error message, `Parse` throws `FormatException` with the same message, `TryParse` returns `false`, and JSON `null` deserializes to `null` without a `Validate` call.

## [07]-[CONVENIENCE_MEMBERS]

The package supplies plain static members beside its generators, and they stay at the BCL boundary, because domain code uses `Seq<A>()`, `Seq(x)`, `Set(x)`, and `toSeq` in their place:
- `Thinktecture.Empty.Action` converts to every `Action` delegate up to 16 parameters, `Empty.Disposable()` and `Empty.AsyncDisposable()` return cached no-op instances, and `Empty.Collection<T>()`, `Empty.Dictionary<TKey, TValue>()`, `Empty.Lookup<TKey, TValue>()`, and `Empty.Set<T>()` return cached read-only empties that follow the argument rules of the BCL collections (a `null` key throws on the dictionary)
- `SingleItem.Set`, `SingleItem.Dictionary`, and `SingleItem.Lookup` build a read-only collection around one item with an optional comparer, so an overload for one item wraps its argument and delegates to the collection overload, and `SingleItem.Lookup` exposes its element sequence live
- `ToReadOnlyCollection(count)` wraps a sequence without enumerating it and trusts the caller's count, and `ToReadOnlyCollection(selector)` composes a projection with the source count and reruns the selector on every enumeration
- `TrimOrNullify()` returns `null` for blank text and the trimmed text otherwise, `TrimOrNullify(maxLength)` cuts the trimmed text by `char` count, and its place is inside a validation hook that assigns the result back to `value`, never as an absence marker for a domain value, which is `Option<string>` through `Optional`
- `Thinktecture.Collections.ProjectionEqualityComparer<T, TItem>` compares projections, and `StringKeyedObjectComparer<T>` compares `ToValue()` of any `IConvertible<string>` type with its `Ordinal` and culture fields, so `Ordinal` restores exact matches for one collection of a case-insensitive type

## [08]-[FRAMEWORK_INTEGRATION]

Simple value objects and keyed smart enums cross every boundary as their key, complex value objects cross JSON and MessagePack as objects with their members, and every type crosses a boundary as one value through an object factory:

| [INDEX] | [INTEGRATION]                  | [PACKAGE]               | [REGISTRATION]                                                          |
| :-----: | :----------------------------- | :---------------------- | :---------------------------------------------------------------------- |
|   [01]  | `System.Text.Json`             | `Json`                  | Referenced by the declaring project, the converter attribute is emitted |
|   [02]  | `System.Text.Json` at the host | `Json`                  | `options.Converters.Add(new ThinktectureJsonConverterFactory())`        |
|   [03]  | `Newtonsoft.Json`              | `Newtonsoft.Json`       | `ThinktectureNewtonsoftJsonConverterFactory`                            |
|   [04]  | MessagePack                    | `MessagePack`           | Generated formatter, or `ThinktectureMessageFormatterResolver.Instance` |
|   [05]  | MVC model binding              | `AspNetCore`            | `ModelBinderProviders.Insert(0, new ThinktectureModelBinderProvider())` |
|   [06]  | OpenAPI                        | `Swashbuckle`           | `services.AddThinktectureOpenApiFilters()`                              |
|   [07]  | Entity Framework Core          | `EntityFrameworkCore10` | `optionsBuilder.UseThinktectureValueConverters()`                       |
|   [08]  | Serilog                        | `Serilog`               | `Destructure.UsingThinktectureRuntimeExtensions()`                      |

- The declaring project references `Json` and receives the `[JsonConverter]` attribute, and only a project that cannot do so registers the converter factory at the host, where MVC reads `AddControllers().AddJsonOptions` and minimal APIs read `ConfigureHttpJsonOptions`
- Unknown keys and rejected values on read throw `JsonException` with the validation text, string keys read through a span-based converter that rejects a non-string token, and a regular union needs one `[JsonDerivedType]` on the base per case or a `[ObjectFactory<string>]` on the base
- Minimal APIs bind through `IParsable<T>.TryParse` and answer a failed bind with a plain 400, MVC runs `Validate`, writes the error into `ModelState`, and `[ApiController]` answers 400 with the text, and the binder provider goes in front of the default providers with `skipBindingFromBody` at its default `true`
- `AddThinktectureOpenApiFilters` renders a value object as its key or its members and a smart enum as its key with the allowed values, `SmartEnumSchemaFilter` selects `Default`, `OneOf`, `AnyOf`, `AllOf`, or `FromDependencyInjection`, and `SmartEnumSchemaExtension` adds `x-enum-varnames`
- Entity Framework Core stores a keyed type in one column of the key type, `UseThinktectureValueConverters` applies to every context on the options, `AddThinktectureValueConverters` narrows to a model, entity, owned, or complex builder, `HasThinktectureValueConverter` to one property, `UseConstructorForRead` defaults to `true` so a row materializes without the hook, a complex value object maps as a complex property or an owned type, and a regular union maps as table-per-hierarchy through `HasDiscriminator<string>` with one `HasValue<TCase>` per case
- Serilog logs a keyed smart enum and a simple value object as the key and an ad hoc union as its `Value` once the policy is registered and the template uses `{@Property}`

## [09]-[ANTI_PATTERNS]

| [INDEX] | [WRONG_FORM]                                                        | [CORRECT_FORM]                                                    |
| :-----: | :------------------------------------------------------------------ | :---------------------------------------------------------------- |
|   [01]  | `throw` inside the hook, which skips `TryCreate` and the frameworks | Assign `validationError` and `return`                             |
|   [02]  | A hook that trims into a local and never assigns `value`            | `value = trimmed`                                                 |
|   [03]  | `value.Trim().ToUpper()` in a hook depends on the current culture   | `value.Trim().ToUpperInvariant()`                                 |
|   [04]  | `[ValueObject<string>]` without comparer attributes                 | Both `[KeyMemberEqualityComparer]` and `[KeyMemberComparer]`      |
|   [05]  | `TrimOrNullify(maxLength)` as a length rule in a hook               | Reject the over-long input, a cut maps 2 inputs to 1 value        |
|   [06]  | `HasConversion` with a lambda that calls `Create`                   | `HasThinktectureValueConverter()` or the converter registration   |
|   [07]  | The host converter factory for a complex value object               | `Json` referenced by the declaring project, or an object factory  |
|   [08]  | Native `switch` with `_ =>` over a smart enum or union              | The generated `Switch` or `Map`                                   |
|   [09]  | A lambda without `static` in a `Switch` arm                         | The state overload with a `static` lambda                         |
|   [10]  | `SwitchPartially` where every case matters                          | The exhaustive `Switch`                                           |
|   [11]  | `default(TUnion)` or `new TUnion()` on a struct union               | A member value, or `MapToFirstMember` with a stateless first case |
|   [12]  | A stateless marker as a class                                       | `readonly record struct`                                          |
|   [13]  | `string` failure case beside a `string` success value               | A distinct type per case                                          |
|   [14]  | A hand-written serializer for an ad hoc union                       | `[ObjectFactory<string>]` with `ToValue` and `Validate`           |
|   [15]  | `new List<T>()` as an empty `IReadOnlyList<T>` at the BCL boundary  | `Thinktecture.Empty.Collection<T>()`                              |
|   [16]  | A `Fin<T>` or `Validation<Error, T>` adapter that calls `Create`    | `Validate`, which never throws                                    |
