<!-- [15] recurring dates and currency amount stay for a use-case reference, its `FileUrn` integrated into .claude/skills/dotnet-coding-thinktecture/references/factory-paths.md -->
# [VALUE_OBJECTS]

Value objects are domain values defined by their content, not by identity. Two instances with the same content are equal and interchangeable. `Thinktecture.Runtime.Extensions` generates the factory methods, equality, comparison, parsing, formatting, and conversion members of a value object at compile time. The hand-written part is the validation hook and the domain behavior.

<!-- Decision held by dotnet-coding [01.1] honest signatures
## [01]-[PRIMITIVE_OBSESSION]

`string`, `int`, or `decimal` in a signature carries no rule and no meaning. Two parameters of the same primitive type swap without a compiler error. Validation of the same value repeats at every construction site, and a rule change misses a copy. Normalization (trimming, rounding) is skipped where a construction site forgets it. The signals for a value object are duplicated validation, confusable parameters, magic strings or numbers, primitives that always travel together, and behavior tied to a raw value. Concepts with one value become simple value objects. Concepts with several values belonging together become complex value objects.
-->

<!-- Integrated into .claude/skills/dotnet-coding-thinktecture/SKILL.md
## [02]-[SIMPLE_AND_COMPLEX]

Simple value objects wrap one key member and carry `[ValueObject<TKey>]`. Complex value objects hold several read-only members and carry `[ComplexValueObject]`. Both are `partial` classes or structs. The generator adds `sealed` to a class and `readonly` to a struct, and writing those modifiers is allowed. Primary constructors draw `TTRESG043`, because the generator owns the private constructor. Every field is read-only (`TTRESG001`), every property has no setter (`TTRESG003`), and an `init` accessor is private (`TTRESG042`). The key member is non-nullable. String keys need an equality comparer, or `TTRESG048` warns, and a complex value object with string members needs `DefaultStringComparison`, or `TTRESG049` warns. The complex form accepts one member or none, and a complex value object with one member does not receive the members generated from a key member.

```csharp
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
internal sealed partial class ProductName {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        if (string.IsNullOrWhiteSpace(value)) {
            validationError = new ValidationError("Product name cannot be empty.");
            return;
        }

        value = value.Trim();

        if (value.Length < 3)
            validationError = new ValidationError("Product name must be at least three characters long.");
    }
}

[ComplexValueObject]
internal sealed partial class Boundary {
    public decimal Lower { get; }
    public decimal Upper { get; }
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref decimal lower, ref decimal upper) {
        if (lower > upper) {
            validationError = new ValidationError(string.Create(CultureInfo.InvariantCulture, $"Lower boundary '{lower}' must be less than or equal to upper boundary '{upper}'."));
            return;
        }

        lower = Math.Round(lower, 2, MidpointRounding.ToEven);
        upper = Math.Round(upper, 2, MidpointRounding.ToEven);
    }
}
```
-->

<!-- Integrated into .claude/skills/dotnet-coding-thinktecture/SKILL.md
## [03]-[GENERATED_API]

`Create(value)` validates and returns the instance, or throws `System.ComponentModel.DataAnnotations.ValidationException` with the message `validationError.ToString()`. `TryCreate(value, out T? obj)` returns `false` on rejection, and `TryCreate(value, out T? obj, out ValidationError? validationError)` also hands back the error. `Validate(value, IFormatProvider? provider, out T? obj)` returns the error or `null` and never throws. Complex value objects take one argument per member in declaration order, and `Validate` has no provider parameter. `null` arguments for a non-nullable key return the error `The argument 'value' must not be null.`. `null` non-nullable members of a complex value object return `The member "PostalCode" of type "Address" must not be null.` before the hook runs.

Equality, `GetHashCode`, `==`, and `!=` run through the configured comparer. `ToString()` of a simple value object returns the key's `ToString()`. `ToString()` of a complex value object formats the equality members as `{ Lower = 1.23, Upper = 2.57 }`. The simple form implements `IComparable<T>` and `IComparable` when the key is comparable, and `IFormattable` when the key is formattable. It implements `IParsable<T>` when the key is parsable or a `string`. It implements `ISpanParsable<T>` when the key is span-parsable. `Parse(string s, IFormatProvider? provider)` throws `FormatException` with the validation text, and `TryParse(s, provider, out obj)` returns `false`. `[TypeConverter(typeof(ThinktectureTypeConverter<T, TKey, TValidationError>))]` is emitted on every simple value object with factory methods.

`ConversionToKeyMemberType` defaults to `Implicit` and converts the value object to `TKey`, with `null` in giving `null` out for a class. `ConversionFromKeyMemberType` defaults to `Explicit` and calls `Create`. `UnsafeConversionToKeyMemberType` defaults to `Explicit` and converts a class to a value-type key, throwing `NullReferenceException` on `null`. Each accepts `ConversionOperatorsGeneration.None`, `Implicit`, or `Explicit`. `SkipToString`, `SkipIComparable`, `SkipIFormattable`, `SkipIParsable`, `SkipISpanParsable`, and `SkipEqualityComparison` remove the matching member. `SkipEqualityComparison` removes `Equals`, `GetHashCode`, and `IEquatable<T>`.
-->

<!-- Integrated into .claude/skills/dotnet-coding-thinktecture/SKILL.md
## [04]-[VALIDATION_HOOK]

`ValidateFactoryArguments` is a `static partial void` method. The first parameter is `ref ValidationError? validationError`, and each following parameter is the key or a member by `ref`. The hook rejects input by assigning `validationError` and normalizes input by assigning the `ref` parameter. The compiler erases a `void` partial method without an implementation. Types without rules pay nothing. Every entry point runs the hook: `Create`, `TryCreate`, `Validate`, the conversion from the key, `Parse`, the JSON converters, the MessagePack formatter, and model binding. `ValidateConstructorArguments(ref TKey value)` exists, and it rejects input by throwing alone. The factory hook is the correct place for a rule over one value and reports the first violated rule, independent rules over several inputs accumulate at the input boundary.
-->

<!-- Integrated into .claude/skills/dotnet-coding-thinktecture/SKILL.md
## [05]-[HOOK_PARAMETERS]

The hook accepts trailing parameters after the key or members. Declare them by value and without a default, or `TTRESG076` reports the parameter. The generator reproduces each additional parameter on its own declaration, re-declares a reference type as nullable, and gives it the default `null` or `default`. With additional parameters present, the generator emits `private static TValidationError? ValidateCore(members, extras, out T? obj)` and `private static T CreateCore(members, extras)`. The public `Validate` delegates to `ValidateCore` and passes `default` for every additional parameter, a rule over a clock, state, or a setting runs in the transition that receives it as an argument. `Create`, `TryCreate`, serialization, and model binding keep their shape. `CreateCore` follows `CreateFactoryMethodName`: a factory renamed `FromRange` gets `FromRangeCore`. The `ValidateCore` name is fixed.

```csharp
[ValueObject<decimal>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Zero", MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
internal readonly partial struct Money : System.Numerics.IMultiplyOperators<Money, int, Money> {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref decimal value, MidpointRounding rounding) {
        if (value < 0) {
            validationError = new ValidationError("Amount cannot be negative.");
            return;
        }

        value = decimal.Round(value, 2, rounding);
    }

    public static Money Create(decimal amount, MidpointRounding rounding) => CreateCore(amount, rounding);
    public static Money operator *(Money left, int right) => Create(left._value * right);
    public static Money operator *(int left, Money right) => Create(right._value * left);
}
```

The generated default is `MidpointRounding.ToEven`. `Money.Create(19.999m)` yields `20.00`. `Money.Create(19.999m, MidpointRounding.ToNegativeInfinity)` yields `19.99`. Rounding happens once, inside the hook, whichever factory is called. Multiplication by `decimal` is disabled because the product needs a rounding decision, and multiplication by `int` is hand-written through `Create`.

The hook has a second form with a return value. When the implementation is declared `private static partial string ValidateFactoryArguments(ref ValidationError? validationError, ref int value)`, the generator emits the same declaration. The generated `Validate` passes the returned value to `partial void FactoryPostInit(string factoryArgumentsValidationError)` on the constructed instance. `FactoryPostInit` runs only when validation succeeded. The instance already satisfies every rule. `[IgnoreMember]` exempts the receiving field from `TTRESG001`, and the field needs an initializer, because the generated constructor leaves it unset (`CS8618`). `readonly` structs cannot hold such a field. The pattern belongs to a class. Hand-written factories calling the hook drop the return value. Hand-written factories delegate to `CreateCore` and `ValidateCore`.
-->

<!-- Integrated into .claude/skills/dotnet-coding-thinktecture/SKILL.md
## [06]-[COMPARERS]

`string` keys compare with `StringComparer.OrdinalIgnoreCase` by default, and every other key compares with its own `Equals`. `[KeyMemberEqualityComparer<TAccessor, TKey>]` selects the equality comparer of a simple value object. `[KeyMemberComparer<TAccessor, TKey>]` selects the ordering comparer for `IComparable<T>` and the comparison operators, and it exists for simple value objects alone. `TTRESG102` fires whenever a comparer stands without an equality comparer. `TTRESG103` fires only when the key type is comparable and `SkipIComparable` is not `true`. `SkipEqualityComparison = true` suppresses `TTRESG048`.

The accessors are `ComparerAccessors.StringOrdinal`, `StringOrdinalIgnoreCase`, `CurrentCulture`, `CurrentCultureIgnoreCase`, `InvariantCulture`, `InvariantCultureIgnoreCase`, and `Default<T>`. Custom accessors implement `IEqualityComparerAccessor<T>` with `static abstract IEqualityComparer<T> EqualityComparer`, or `IComparerAccessor<T>` with `static abstract IComparer<T> Comparer`.

Complex value objects compare every assignable member. `DefaultStringComparison` sets the comparison of its string members and defaults to `OrdinalIgnoreCase`. `[MemberEqualityComparer<TAccessor, TMember>]` on one member changes the comparer of that member, and the set of members that take part in equality. Members without the attribute drop out of equality and hashing as soon as one member carries it. Put `[MemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]` on `Identifier` and nothing on `Name`. Then `Create("1", "Item 1") == Create("1", "Item 2")` is `true`, and `Create("a", "x") == Create("A", "x")` is `false`. `[IgnoreMember]` removes a member from equality, from the factory methods, and from every other generated member.
-->

<!-- Integrated into .claude/skills/dotnet-coding-thinktecture/SKILL.md
## [07]-[CUSTOM_VALIDATION_ERROR]

`ValidationError` is a sealed class with a `Message`. Custom error types implement `IValidationError<T>`, with the one member `static abstract T Create(string message)`. The generator calls `Create` for its own errors (the null-argument error). `ToString()` is the text that reaches `ValidationException`, `FormatException`, the JSON converters, and model state. The custom type overrides it, and `Expected.ToString()` returns `Message`. `[ValidationError<T>]` on the value object switches the hook parameter, the second `TryCreate` overload, and `Validate` to the custom type.

```csharp
internal sealed record BoundaryValidationError(string Message, decimal? Lower, decimal? Upper) : IValidationError<BoundaryValidationError> {
    public static BoundaryValidationError Create(string message) => new(message, Lower: null, Upper: null);
    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{Message} (Lower={Lower}|Upper={Upper})");
}

[ComplexValueObject]
[ValidationError<BoundaryValidationError>]
internal sealed partial class Interval {
    public decimal Lower { get; }
    public decimal Upper { get; }
    static partial void ValidateFactoryArguments(ref BoundaryValidationError? validationError, ref decimal lower, ref decimal upper) {
        if (lower > upper)
            validationError = new BoundaryValidationError("Lower boundary must be less than upper boundary.", lower, upper);
    }
}
```

`Interval.Create(2, 1)` throws with the message `Lower boundary must be less than upper boundary. (Lower=2|Upper=1)`.
-->

<!-- Integrated into .claude/skills/dotnet-coding-thinktecture/SKILL.md
## [08]-[FACTORY_SETTINGS]

`ConstructorAccessModifier` defaults to `Private`. `CreateFactoryMethodName` and `TryCreateFactoryMethodName` rename `Create` and `TryCreate`. `SkipFactoryMethods = true` removes both factory methods, the `TypeConverter`, `IObjectFactory<T>`, the conversion from the key, `IParsable`, `ISpanParsable`, and the serialization converters, and sets the arithmetic operators to `None`. Types without factory methods still serialize when they carry `[ObjectFactory<T>(UseForSerialization = ...)]`, because the object factory supplies the conversion.

| [INDEX] | [SETTING]                                                 | [CASCADE]                                                             |
| :-----: | :-------------------------------------------------------- | :-------------------------------------------------------------------- |
|  [01]   | `SkipIParsable = true`                                    | `ISpanParsable` skipped                                               |
|  [02]   | `SkipEqualityComparison = true`                           | `ComparisonOperators` and `EqualityComparisonOperators` become `None` |
|  [03]   | `EqualityComparisonOperators = None`                      | `ComparisonOperators` becomes `None`                                  |
|  [04]   | `ComparisonOperators` above `EqualityComparisonOperators` | `EqualityComparisonOperators` is raised to match                      |
|  [05]   | `EmptyStringInFactoryMethodsYieldsNull = true`            | `NullInFactoryMethodsYieldsNull` becomes `true`                       |
-->

<!-- Integrated into .claude/skills/dotnet-coding-thinktecture/SKILL.md
## [09]-[NULL_AND_EMPTY_INPUT]

`NullInFactoryMethodsYieldsNull = true` makes the factory methods of a class return `null` for a `null` argument instead of an error. `EmptyStringInFactoryMethodsYieldsNull = true` extends this to empty and whitespace strings. Structs cannot be `null`: both settings are ignored, and `EmptyStringInFactoryMethodsYieldsNull` on a struct warns with `TTRESG109`. With either setting, `TryCreate` returns `true` with a `null` object, and the generated `out` parameter loses `[NotNullWhen(true)]`. `Validate` returns a `null` error with a `null` object. `Parse` keeps its non-nullable contract. With `EmptyStringInFactoryMethodsYieldsNull`, `Parse` of an empty string throws `FormatException`, and the message states that empty or whitespace input yields `null`. Minimal APIs bind through `TryParse`. Empty input fails to bind for such a type.
-->

<!-- Integrated into .claude/skills/dotnet-coding-thinktecture/SKILL.md
## [10]-[OPERATORS]

`EqualityComparisonOperators` and `ComparisonOperators` accept `OperatorsGeneration.None`, `Default`, or `DefaultWithKeyTypeOverloads`. `Default` generates operators between two value objects. `DefaultWithKeyTypeOverloads` adds overloads with the key type in both operand positions. `amount > 42m` compiles without a conversion. The two settings must match, or `TTRESG105` warns. `AdditionOperators`, `SubtractionOperators`, `MultiplyOperators`, and `DivisionOperators` take the same values and default to `Default` when the key supports the operation. Every arithmetic result goes through `Create`. The invariant holds after every operation, and `Amount.Create(1m) - 5m` throws. The generator emits `operator checked +` and its siblings beside the unchecked forms when the key type declares the checked operator.

```csharp
[ValueObject<decimal>(
    AllowDefaultStructs = true,
    DefaultInstancePropertyName = "Zero",
    EqualityComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads,
    ComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads,
    AdditionOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads,
    SubtractionOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads)]
internal readonly partial struct Amount {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref decimal value) {
        if (value < 0)
            validationError = new ValidationError("Amount must be positive.");
    }
}
```
-->

<!-- Integrated into .claude/skills/dotnet-coding-thinktecture/SKILL.md
## [11]-[STRUCT_DEFAULTS]

Struct value objects reject `default(T)` and `new T()` by default. The generator adds `IDisallowDefaultValue` to the type, and `TTRESG047` turns every `default` expression or parameterless construction into an error. The JSON converters, the MessagePack formatter, and the model binder read the same interface at runtime.

- Settable properties of the type in another class warn with `TTRESG104` until they are `required`
- `AllowDefaultStructs = true` accepts the default and emits `public static readonly T Empty = default`, renamed through `DefaultInstancePropertyName`
- `AllowDefaultStructs` stays `false` when the key is a reference type (`TTRESG057`) or when a member disallows default (`TTRESG058`)
- It also stays `false` when the type implements `IDisallowDefaultValue` by hand (`TTRESG080`)
- `IDisallowDefaultValue` on a class warns with `TTRESG110`, because a class defaults to `null`

Choose a class when absence is a domain state and `null` expresses it. Choose a struct for a small value that is always valid. Allow the default when it has a domain meaning (zero, infinity).
-->

<!-- Integrated into .claude/skills/dotnet-coding-thinktecture/SKILL.md
## [12]-[CUSTOM_KEY_MEMBER]

`KeyMemberName`, `KeyMemberAccessModifier`, and `KeyMemberKind` shape the generated key member, which defaults to `private readonly TKey _value`. `SkipKeyMember = true` leaves the key member to the hand-written part, and `KeyMemberName` names it. Missing members are `TTRESG044`, and type mismatches `TTRESG045`. `OpenEndDate` uses this to give `default` the meaning of an open end. `SkipToString = true` removes the `ToString()` override alone, and the generated `IFormattable` still formats the key. Types with a hand-written `ToString()` also set `SkipIFormattable = true`, or a provider-aware caller prints the key.

```csharp
[ValueObject<DateOnly>(
    SkipKeyMember = true,
    KeyMemberName = nameof(Date),
    DefaultInstancePropertyName = "Infinite",
    EqualityComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads,
    ComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads,
    AllowDefaultStructs = true,
    SkipToString = true,
    SkipIFormattable = true)]
internal readonly partial struct OpenEndDate {
    private readonly DateOnly? _date;
    private DateOnly Date {
        get => _date ?? DateOnly.MaxValue;
        init => _date = value;
    }

    public override string ToString() => this == Infinite ? "Infinite" : Date.ToString("O", CultureInfo.InvariantCulture);
}
```

The nullable backing field maps the CLR default to `DateOnly.MaxValue`. `default(OpenEndDate) == OpenEndDate.Infinite` and `OpenEndDate.Infinite == DateOnly.MaxValue` both hold. `Where(p => p.EndDate >= today)` needs one comparison and no `null` branch.
-->

<!-- Integrated into .claude/skills/dotnet-coding-thinktecture/SKILL.md
## [13]-[COMPOSITION]

Complex value objects compose simple value objects, smart enums, and other complex value objects. Each component keeps its own rule, and the composite adds the rule that spans components. The hook does not repeat the generated null check. Structs compose structs the same way. `Period` with `DateOnly From` and `OpenEndDate Until` rejects `from >= until` in its hook. The key-type overloads of `OpenEndDate` make that comparison compile.

```csharp
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
internal sealed partial class PostalCode {
    public int Length => _value.Length;
}

[SmartEnum<string>]
internal sealed partial class CountryCode {
    public static readonly CountryCode DE = new("DE", 5);
    public static readonly CountryCode CH = new("CH", 4);
    public int PostalCodeLength { get; }
}

[ComplexValueObject]
internal sealed partial class Address {
    public PostalCode PostalCode { get; }
    public CountryCode Country { get; }
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref PostalCode postalCode, ref CountryCode country) {
        if (postalCode.Length != country.PostalCodeLength)
            validationError = new ValidationError(string.Create(CultureInfo.InvariantCulture, $"Postal code length for country {country} must be {country.PostalCodeLength}."));
    }
}
```
-->

<!-- Integrated into .claude/skills/dotnet-coding-thinktecture/SKILL.md
## [14]-[GENERIC_KEY_TYPES]

`[ValueObject<TypeParamRef1>]` through `TypeParamRef5` bind the key to the type parameter at that position. The parameter needs a `notnull`, `struct`, or `class` constraint, or `TTRESG074` reports it. `Create`, `TryCreate`, `Validate`, equality, and the conversion operators are always generated. The rest follows the constraints on the parameter: `where T : INumber<T>` yields parsing, comparison, formatting, and arithmetic together.

```csharp
[ValueObject<TypeParamRef1>]
internal readonly partial struct Measure<T> where T : System.Numerics.INumber<T>;
```
-->

## [15]-[USE_CASES]

Recurring dates have a day and month and no year. `DateOnly` keys pinned to one leap year give ordering, equality, and the calendar rules. `ConversionFromKeyMemberType = ConversionOperatorsGeneration.Implicit` accepts any `DateOnly`, and `ConversionToKeyMemberType = ConversionOperatorsGeneration.None` hides the pinned year. The hook rewrites the year, and every instance shares it.

Behavior that belongs to a value lives on the value. A `CurrencyAmount` struct with `decimal Amount` and `CurrencyCode Currency` declares `operator +` by hand. The operator rejects two different currencies with `InvalidOperationException` and returns `Create(left.Amount + right.Amount, left.Currency)`. The `CurrencyCode` hook normalizes with `ToUpperInvariant()`. `CurrencyCode.Create("eur") == CurrencyCode.EUR` holds with an ordinal comparer.

<!-- Integrated into .claude/skills/dotnet-coding-thinktecture/references/factory-paths.md
Complex value objects with a string representation carry `[ObjectFactory<string>]`. The attribute adds `IObjectFactory<FileUrn, string, ValidationError>`, which requires a static `Validate(string? value, IFormatProvider? provider, out FileUrn? item)`. `UseForSerialization = SerializationFrameworks.All` adds `IConvertible<string>`, which requires `ToValue()`, and makes every serializer read and write the string. `HasCorrespondingConstructor = true` requires a constructor with one `string` parameter, which `TTRESG059` enforces. Entity Framework Core builds the instance through that constructor only when the factory also sets `UseWithEntityFramework = true`. The constructor trusts the stored value. Rows without a separator fail materialization with an index error. Every other framework keeps calling `Validate`. The string factory also supplies `Parse` and `TryParse`. The type binds in a minimal API.

```csharp
[ComplexValueObject(DefaultStringComparison = StringComparison.OrdinalIgnoreCase)]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.All, UseWithEntityFramework = true, HasCorrespondingConstructor = true)]
internal sealed partial class FileUrn {
    private FileUrn(string value) {
        string[] parts = value.Split(':', 2);
        FileStore = parts[0];
        Urn = parts[1];
    }

    public string FileStore { get; }
    public string Urn { get; }

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out FileUrn? item) {
        item = null;

        if (string.IsNullOrWhiteSpace(value))
            return new ValidationError("FileUrn cannot be empty.");

        int separatorIndex = value.IndexOf(':', StringComparison.Ordinal);
        return separatorIndex <= 0 || separatorIndex == value.Length - 1
            ? new ValidationError("Invalid FileUrn format. Expected 'fileStore:urn'.")
            : Validate(value[..separatorIndex], value[(separatorIndex + 1)..], out item);
    }

    public string ToValue() => $"{FileStore}:{Urn}";
}
```

`FileUrn.Create("blob", "a/b.pdf")` serializes to the JSON string `"blob:a/b.pdf"`, and `FileUrn.Parse("nocolon", null)` throws `FormatException` with the format message.
-->

<!-- Integrated into .claude/skills/dotnet-coding-thinktecture/SKILL.md
Value objects also serve as the cases of a regular union. `[Union]` abstract class `Jurisdiction` holds nested cases: a `[ValueObject<string>(KeyMemberName = "IsoCode")] Country`, a `[ValueObject<int>(KeyMemberName = "Number")] FederalState`, and a `[ValueObject<string>(KeyMemberName = "Name")] District`. `[ComplexValueObject] Unknown` with no members and a static `Instance` is the last case. The string cases carry both comparer attributes. `Unknown` receives value equality from the complex form: two `Unknown` instances are equal, and `Switch` covers every case.
-->

<!-- Integrated into .claude/skills/dotnet-coding-thinktecture/SKILL.md
## [16]-[FRAMEWORK_INTEGRATION]

Simple value objects cross every boundary as their key. Complex value objects cross JSON and MessagePack as objects with their members, and they cross a boundary carrying one value through an object factory alone.

Every package name below omits the prefix `Thinktecture.Runtime.Extensions.`.

| [INDEX] | [INTEGRATION]                  | [PACKAGE]                | [REGISTRATION]                                                                                   |
| :-----: | :----------------------------- | :----------------------- | :----------------------------------------------------------------------------------------------- |
|  [01]   | `System.Text.Json`             | `Json`                   | Referenced by the value object project, the generator emits the converter and `[JsonConverter]`  |
|  [02]   | `System.Text.Json` at the host | Same package in the host | `options.Converters.Add(new ThinktectureJsonConverterFactory())`                                 |
|  [03]   | `Newtonsoft.Json`              | `Newtonsoft.Json`        | As above with `ThinktectureNewtonsoftJsonConverterFactory`                                       |
|  [04]   | MessagePack                    | `MessagePack`            | Generated formatter, or `ThinktectureMessageFormatterResolver.Instance` in a `CompositeResolver` |
|  [05]   | MVC model binding              | `AspNetCore`             | `options.ModelBinderProviders.Insert(0, new ThinktectureModelBinderProvider())`                  |
|  [06]   | OpenAPI                        | `Swashbuckle`            | `services.AddThinktectureOpenApiFilters()`                                                       |
|  [07]   | Entity Framework Core          | `EntityFrameworkCore10`  | `optionsBuilder.UseThinktectureValueConverters()`                                                |
|  [08]   | Serilog                        | `Serilog`                | `Destructure.UsingThinktectureRuntimeExtensions()`                                               |

`System.Text.Json.JsonSerializer.Serialize` writes `Amount.Create(10.5m)` as `10.5`, `Boundary.Create(1m, 2m)` as `{"Lower":1,"Upper":2}`, and a `FileUrn` as its string. Record `Line(Amount Price, Boundary Range, FileUrn Document)` round-trips to an equal value. Failed JSON reads throw `JsonException` with the validation text.

Minimal APIs bind a simple value object through `IParsable<T>`. `TryParse` failures yield a plain `400` without the validation text, because the binding contract carries no message. The `MaybeBound<T, TKey, TValidationError>` pattern is application code. Its `TryParse` always returns `true`, parses the key with `TKey.TryParse`, calls `T.Validate`, and stores either the value or the error text. Endpoint filters or `IValidatableObject` then reject the stored error. MVC model binding runs `Validate` and writes the error into `ModelState`. `[ApiController]` answers `400` with the text. The model binder covers simple value objects and any type with `[ObjectFactory<string>(UseForModelBinding = true)]`, and the provider goes in front of the default providers.

`AddThinktectureOpenApiFilters` renders a simple value object as its key type and a complex value object as an object with its members. `ThinktectureSchemaFilterOptions.RequiredMemberEvaluator` defaults to `RequiredMemberEvaluator.FromDependencyInjection`. The registration adds `DefaultRequiredMemberEvaluator` as the `IRequiredMemberEvaluator`. It marks a member that implements `IDisallowDefaultValue` as required, and a non-nullable reference member as required. `All` and `None` override that per application, and Swashbuckle handles a member with `[Required]`.

Entity Framework Core stores a simple value object in one column of the key type through a generated value converter.

- `UseThinktectureValueConverters` on `DbContextOptionsBuilder` applies to every context that uses the options
- `AddThinktectureValueConverters` exists on `ModelBuilder`, `EntityTypeBuilder`, `OwnedNavigationBuilder`, and `ComplexPropertyBuilder`
- `HasThinktectureValueConverter` on `PropertyBuilder<TProperty>`, `ComplexTypePropertyBuilder<TProperty>`, `PrimitiveCollectionBuilder<TProperty>` applies converter to one property
- Both accept a `Configuration` where `UseConstructorForRead` defaults to `true`
- With that default a row materializes through the constructor, and the hook does not run on read
- Types with an object factory run `Validate` on read unless they declare `HasCorrespondingConstructor = true`
- `Configuration.KeyedValueObjects` defaults to `KeyedValueObjectConfiguration.NoMaxLength`
- `FixedKeyedValueObjectMaxLengthStrategy` or `CustomKeyedValueObjectMaxLengthStrategy` with a `Func<Type, Type, MaxLengthChange>` sets column lengths

Complex value objects map as a complex property or an owned type. `AddThinktectureValueConverters` on that builder converts the simple value objects nested inside it. Complex value objects with `[ObjectFactory<T>(UseWithEntityFramework = true)]` map to one column of `T` instead.

Serilog destructuring logs a simple value object as its key and declines a complex value object. `TypesToRenderAsString.ValueObjects` switches the simple form to `ToString()`.
-->

<!-- Integrated into .claude/skills/dotnet-coding-thinktecture/SKILL.md
## [17]-[DESIGN_RULES]

`HasConversion` with a lambda that calls `Create` re-validates every row read from the database.

| [INDEX] | [WRONG_FORM]                                                                                 | [CORRECT_FORM]                                                          |
| :-----: | :------------------------------------------------------------------------------------------- | :---------------------------------------------------------------------- |
|  [01]   | `throw` inside `ValidateFactoryArguments` skips `TryCreate`, `Validate`, and framework paths | Assign `validationError` and `return`                                   |
|  [02]   | `value.Trim().ToUpper()` in a hook depends on the current culture                            | `value.Trim().ToUpperInvariant()`                                       |
|  [03]   | `[ValueObject<string>]` without comparer attributes                                          | Both `[KeyMemberEqualityComparer<...>]` and `[KeyMemberComparer<...>]`  |
|  [04]   | `record Payment(decimal Amount, string Account)` validating in a throwing constructor        | `record Payment(Amount Amount, AccountNumber Account)`                  |
|  [05]   | `HasConversion(name => (string)name, s => ProductName.Create(s))`                            | `HasThinktectureValueConverter()` or `UseThinktectureValueConverters()` |

Do not register `ThinktectureJsonConverterFactory` at the host for a complex value object without an object factory. Reference `Thinktecture.Runtime.Extensions.Json` from the project that declares the type.

At a boundary that takes user input, `Create` and `Parse` throw. Declare `[ValidationError<X>]` with a typed `Expected` record that implements `IValidationError<X>`, and a hand-written `From` that maps `Validate` to `Fin<T>`.
-->
