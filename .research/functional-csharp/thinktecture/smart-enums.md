# [SMART_ENUMS]

A Smart Enum is a class with a fixed set of items declared as `public static readonly` fields. The source generator supplies the constructor, the lookup, equality, conversion, and pattern matching. Each item is an object with its own data and behavior. A consumer calls a method on the item instead of branching on it.

## [01]-[DECLARATION]

Declare a `partial` class and apply `[SmartEnum<TKey>]` for a keyed Smart Enum or `[SmartEnum]` for a keyless one. Items are `public static readonly` fields of the enclosing type. A non-public item is an error (`TTRESG002`). A static property is not an item and raises `TTRESG101`. A Smart Enum without items raises `TTRESG100`.

The generator emits one private constructor per base class constructor. The parameters arrive in a fixed order. The key comes first, then the own fields and properties in declaration order, then the base class constructor parameters. One delegate per `[UseDelegateFromConstructor]` method comes last. The hook `ValidateConstructorArguments` receives the key, the own fields and properties, and the base arguments by `ref`, not the delegates. A `null` key throws `ArgumentNullException` after the hook returns.

```csharp
internal abstract class IsoCoded(int numericCode) {
    public int NumericCode { get; } = numericCode;
}

[SmartEnum<string>]
internal sealed partial class Country : IsoCoded {
    public static readonly Country Germany = new("de", "Germany", 276);
    public static readonly Country France = new("fr", "France", 250);

    public string Name { get; }

    static partial void ValidateConstructorArguments(ref string key, ref string name, ref int numericCode) {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key must not be empty.", nameof(key));
        key = key.Trim().ToUpperInvariant();
    }
}
```

`Country.Germany.Key` is `"DE"` because the hook rewrites the key. The generated signature is `(string key, string name, int numericCode)`, and `numericCode` flows to the base constructor.

- An instance field is read-only (`TTRESG001`), an instance property is read-only (`TTRESG003`), and `[IgnoreMember]` hides a member from the generator and the analyzer. The same two rules on a plain base class are `TTRESG034` and `TTRESG035`.
- A primary constructor is not allowed on the Smart Enum type (`TTRESG043`). Derived nested classes and plain base classes use one.
- The generator adds `sealed` to a Smart Enum that declares no derived types. A derived nested class that is neither abstract nor a base of another derived class must be `sealed` (`TTRESG037`).
- Every enclosing type of a nested Smart Enum is `partial`, because the generator emits the enum as a nested partial declaration.

## [02]-[KEYED_AND_KEYLESS]

A `string` key is the default choice for APIs, JSON, and display text. An `int` or `Guid` key serves persistence and ordering.

A keyless Smart Enum has no key member, no `Get`, no conversion operators, and no comparer settings. Its attribute exposes only `EqualityComparisonOperators`, `SwitchMethods`, `MapMethods`, and `SwitchMapStateParameterName`. No serializer, model binder, or value converter handles it.

The generator emits no `JsonConverterAttribute` for a keyless Smart Enum, so `System.Text.Json` writes the public instance properties and throws `NotSupportedException` on read because no usable constructor exists. `[ObjectFactory<string>]` with a static `Validate` method is the only way to serialize or bind a keyless Smart Enum. Serialization needs `UseForSerialization` set and an instance `ToValue()` (`TTRESG062`), and model binding needs `UseForModelBinding = true`. `HasCorrespondingConstructor = true` on that factory is an error (`TTRESG060`).

## [03]-[GENERATED_API]

`Items` lists the items in declaration order. `Get` returns `null` for a `null` key and throws `UnknownSmartEnumIdentifierException` for an unknown one, while `Validate` reports a `null` key as unknown. `UnknownSmartEnumIdentifierException` is a `KeyNotFoundException` with the message `There is no item of type 'OrderStatus' with the identifier 'nope'.`. `TryGet(key, out item)` returns `false` for an unknown key. `Validate(key, provider, out item)` returns `null` or a `ValidationError` with the same message, and `[ValidationError<TError>]` replaces the error type. The implicit conversion to the key type returns `Key` and returns `null` for a `null` item. The explicit conversion from the key type calls `Get`. `ToString()` returns the key's string form, and a `TypeConverterAttribute` points at `ThinktectureTypeConverter<T, TKey, TValidationError>`.

Items compare by identity. `GetHashCode` is computed once in the constructor from the key with the configured equality comparer. Two items with the same key throw `ArgumentException` on the first lookup.

`IParsable<T>` appears when the key implements `IParsable<TKey>`, which includes `string`, and a failed `Parse` throws `FormatException` with the message of `UnknownSmartEnumIdentifierException`. `ISpanParsable<T>` appears on .NET 9 and later when the key implements `ISpanParsable<TKey>`, which also includes `string`. `IComparable`, `IComparable<T>`, and the comparison operators appear for a comparable key. `IFormattable` appears for a formattable key such as `int`. Lookups use `FrozenDictionary` on .NET 8 and later. String keys gain span overloads of `Get`, `TryGet`, `Validate`, `Parse`, and `TryParse` on .NET 9 and later.

`Items`, `Get`, `TryGet`, and `Validate` implement static abstract members of `ISmartEnum<TKey, T, TValidationError>`. Generic code reaches them through the constraint.

```csharp
internal sealed record UnknownTier() : Expected("tier is not basic or plus", 1), IValidationError<UnknownTier> {
    public static UnknownTier Create(string message) => new();
}

[SmartEnum<string>]
[ValidationError<UnknownTier>]
internal sealed partial class Tier {
    public static readonly Tier Basic = new("basic");
    public static readonly Tier Plus = new("plus");
}

internal static class Lookup {
    public static Option<T> Find<T, TKey>(TKey key) where T : ISmartEnum<TKey, T, ValidationError> where TKey : notnull =>
        T.TryGet(key, out T? item) ? Some(item) : None;
    public static Fin<T> Admit<T, TKey, TError>(TKey key) where T : ISmartEnum<TKey, T, TError> where TKey : notnull where TError : Error, IValidationError<TError> =>
        T.Validate(key, CultureInfo.InvariantCulture, out T? item) is { } error ? error : item!;
}
```

`Find` returns `Option<T>` because a miss has no reason. `Admit` returns `Fin<T>` because `Validate` supplies the typed `Expected` that `[ValidationError<TError>]` names.

## [04]-[SWITCH_AND_MAP]

`Switch` takes one `Action` per item, `Switch<TResult>` takes one `Func<TResult>` per item, and `Map<TResult>` takes one value per item. Every argument is named after its item in camel case, and unnamed arguments are an error (`TTRESG046`). Every lambda argument of `Switch` or `Map` without the `static` modifier raises `TTRESG1001`. The state-passing overloads take the state as the first argument and hand it to every `static` lambda. The state parameter is named `state` unless `SwitchMapStateParameterName` renames it, and `TState : allows ref struct` holds on .NET 9 and later. When the lambdas return different but compatible types, write `TResult` on the call so the compiler reports the one lambda that disagrees.

`SwitchPartially` and `MapPartially` exist only when `SwitchMethods` and `MapMethods` are set to `SwitchMapMethodsGeneration.DefaultWithPartialOverloads`. The void `SwitchPartially` takes an optional `@default` of type `Action<T>` or `Action<TState, T>`, and an unhandled item without a default is a no-op. The value-returning `SwitchPartially` and `MapPartially` require `@default`.

```csharp
[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads, MapMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
internal sealed partial class ProductType {
    public static readonly ProductType Groceries = new("Groceries");
    public static readonly ProductType Housewares = new("Housewares");
}

internal static class Matching {
    public static string Label(ProductType type, decimal weight) =>
        type.Switch(
            weight,
            groceries: static w => string.Create(CultureInfo.InvariantCulture, $"cold chain, {w} kg"),
            housewares: static w => string.Create(CultureInfo.InvariantCulture, $"fragile, {w} kg"));

    public static string Handling(ProductType type) => type.MapPartially(@default: "standard", groceries: "cold chain");
    public static void RecordColdChain(ProductType type, ICollection<string> log) => type.SwitchPartially(log, groceries: static l => l.Add("cold chain"));
    public static void RecordAll(ProductType type, ICollection<string> log) =>
        type.SwitchPartially(log, @default: static (l, item) => l.Add(item.Key), groceries: static l => l.Add("cold chain"));
}
```

`RecordColdChain(ProductType.Housewares, log)` adds nothing. `RecordAll` receives the unhandled item in `@default` and adds its key. Every generated `Switch` and `Map` ends in an arm that throws `InvalidOperationException` with the message `Unknown item 'Rogue'.` for an instance with key `Rogue`. Only an instance outside `Items` reaches that arm.

## [05]-[ITEM_SPECIFIC_BEHAVIOR]

A method whose inputs differ per item reads them from fields that the constructor filled. A method whose algorithm differs per item takes a delegate through `[UseDelegateFromConstructor]`. The attributed method is `partial` (`TTRESG050`) and has no type parameters (`TTRESG051`). The generator adds a private delegate field, appends the delegate parameter after the members, and implements the method as a call through the field. `Round` produces a `Func<decimal, decimal> _round` field. `DelegateName = "DateParser"` on a method `GetDateTime` instead emits a private nested delegate type `DateParser` and a field `_dateParser`. A parameter a `Func` cannot carry, such as `ref`, also forces a delegate type, named after the method.

Static fields initialize in declaration order, so an item cannot reference a later item in its own initializer. The compiler reports a possible `null` when an initializer lambda reads a later field. `Lazy<T>` built from a static method defers the read until every item exists.

```csharp
[SmartEnum<string>]
internal sealed partial class ShippingMethod {
    public static readonly ShippingMethod Standard = new("STANDARD", basePrice: 5.99m, weightMultiplier: 0.5m);
    public static readonly ShippingMethod Express = new("EXPRESS", basePrice: 15.99m, weightMultiplier: 0.75m);

    private readonly decimal _basePrice;
    private readonly decimal _weightMultiplier;

    public decimal Price(decimal orderWeight) => _basePrice + (orderWeight * _weightMultiplier);
}

[SmartEnum]
internal sealed partial class MoneyRoundingStrategy {
    public static readonly MoneyRoundingStrategy Nearest = new("Nearest", static d => decimal.Round(d, 2, MidpointRounding.ToEven));
    public static readonly MoneyRoundingStrategy Up = new("Up", static d => decimal.Round(d, 2, MidpointRounding.ToPositiveInfinity));

    public string Name { get; }

    [UseDelegateFromConstructor]
    public partial decimal Round(decimal value);

    public override string ToString() => Name;
}

[SmartEnum<string>]
internal sealed partial class OrderStatus {
    public static readonly OrderStatus Pending = new("Pending", new(PendingNext));
    public static readonly OrderStatus Shipped = new("Shipped", new(ShippedNext));
    public static readonly OrderStatus Delivered = new("Delivered", new(NoNext));

    private readonly Lazy<IReadOnlyList<OrderStatus>> _nextStates;

    public bool CanTransitionTo(OrderStatus next) => _nextStates.Value.Contains(next);

    private static IReadOnlyList<OrderStatus> PendingNext() => [Shipped];
    private static IReadOnlyList<OrderStatus> ShippedNext() => [Delivered];
    private static IReadOnlyList<OrderStatus> NoNext() => [];
}
```

A keyless Smart Enum has no generated `ToString`. An override supplies the item name that `Switch`, `Map`, and Serilog otherwise render as the type name. A public readonly delegate field is the manual form, and `Thinktecture.Empty.Action` converts to `Action` for an item without behavior.

Inheritance carries a per-item algorithm as an override. Derived classes are nested inside the Smart Enum. First-level derived classes are `private` (`TTRESG014`), deeper derived classes are `public` (`TTRESG015`), and a derived class can be generic.

```csharp
[SmartEnum<string>]
internal abstract partial class NotificationChannel {
    public static readonly NotificationChannel Email = new Typed<string>("email");
    public static readonly NotificationChannel Sms = new Typed<int>("sms");

    public abstract Type PayloadType { get; }

    private sealed class Typed<TPayload>(string key) : NotificationChannel(key) {
        public override Type PayloadType => typeof(TPayload);
    }
}
```

`NotificationChannel.Email.PayloadType` is `typeof(string)`, and `Items` doubles as the list of permitted implementations.

## [06]-[COMPARERS]

A string key uses `StringComparer.OrdinalIgnoreCase` for equality, for the hash code, for `CompareTo`, and for the comparison operators, so `TryGet("PENDING")` finds `Pending`. Every other key type uses its default comparer. `[KeyMemberEqualityComparer<TAccessor, TKey>]` replaces the equality comparer, and `[KeyMemberComparer<TAccessor, TKey>]` replaces the ordering comparer. An accessor implements `IEqualityComparerAccessor<T>` with a static `EqualityComparer` property, or `IComparerAccessor<T>` with a static `Comparer` property. `ComparerAccessors.StringOrdinal`, `StringOrdinalIgnoreCase`, `CurrentCulture`, `CurrentCultureIgnoreCase`, `InvariantCulture`, `InvariantCultureIgnoreCase`, and `Default<T>` implement both interfaces. An accessor whose type does not match the key type is an error (`TTRESG041`).

A comparer without an equality comparer raises `TTRESG102`. An equality comparer without a comparer raises `TTRESG103` unless the key is not comparable or `SkipIComparable` is set. `TTRESG048` applies to Value Objects only: a string-keyed Smart Enum without a comparer attribute compiles without a warning and keeps the case-insensitive default.

On .NET 9 and later the span-based lookup uses the alternate lookup of `FrozenDictionary`. For a predefined accessor the generator calls `GetAlternateLookup<ReadOnlySpan<char>>()`. For a custom accessor it calls `TryGetAlternateLookup`, and when the comparer lacks `IAlternateEqualityComparer<ReadOnlySpan<char>, string>` the span overloads allocate a string per call. A comparer can be its own accessor.

```csharp
internal sealed class TrimmedOrdinalComparer : IEqualityComparer<string>, IComparer<string>, IAlternateEqualityComparer<ReadOnlySpan<char>, string>, IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    private static readonly TrimmedOrdinalComparer Instance = new();

    public static IEqualityComparer<string> EqualityComparer => Instance;
    public static IComparer<string> Comparer => Instance;

    public bool Equals(string? x, string? y) => string.Equals(x?.Trim(), y?.Trim(), StringComparison.Ordinal);
    public int GetHashCode(string obj) => string.GetHashCode(obj.AsSpan().Trim(), StringComparison.Ordinal);
    public int Compare(string? x, string? y) => string.CompareOrdinal(x?.Trim(), y?.Trim());
    public string Create(ReadOnlySpan<char> alternate) => alternate.Trim().ToString();
    public bool Equals(ReadOnlySpan<char> alternate, string other) => alternate.Trim().SequenceEqual(other.AsSpan().Trim());
    public int GetHashCode(ReadOnlySpan<char> alternate) => string.GetHashCode(alternate.Trim(), StringComparison.Ordinal);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<TrimmedOrdinalComparer, string>]
[KeyMemberComparer<TrimmedOrdinalComparer, string>]
internal sealed partial class Ticker {
    public static readonly Ticker Msft = new("MSFT");
    public static readonly Ticker Aapl = new("AAPL");
}
```

`Ticker.TryGet(" AAPL".AsSpan(), out Ticker? item)` finds `Aapl` through the alternate comparer. `[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]` with the matching `[KeyMemberComparer]` is the short form for a case-sensitive key.

## [07]-[SETTINGS]

| [INDEX] | [PROPERTY]                                                  | [DEFAULT]                     | [EFFECT]                                                                 |
| :-----: | :---------------------------------------------------------- | :---------------------------- | :----------------------------------------------------------------------- |
|  [01]   | `KeyMemberName`, `KeyMemberAccessModifier`, `KeyMemberKind` | `"Key"`, `Public`, `Property` | key member shape, `"_key"` for a private field                           |
|  [02]   | `SkipIComparable`, `SkipIParsable`, `SkipISpanParsable`     | `false`                       | drop the interface, `SkipIParsable` also drops `ISpanParsable<T>`        |
|  [03]   | `SkipIFormattable`, `SkipToString`                          | `false`                       | drop `IFormattable`, keep a hand-written `ToString`                      |
|  [04]   | `ComparisonOperators`, `EqualityComparisonOperators`        | `Default`                     | `None` or `DefaultWithKeyTypeOverloads`                                  |
|  [05]   | `ConversionToKeyMemberType`, `ConversionFromKeyMemberType`  | `Implicit`, `Explicit`        | `ConversionOperatorsGeneration.None` removes an operator                 |
|  [06]   | `SwitchMethods`, `MapMethods`                               | `Default`                     | `None` or `DefaultWithPartialOverloads`                                  |
|  [07]   | `SerializationFrameworks`                                   | `All`                         | `SystemTextJson`, `NewtonsoftJson`, `Json`, `MessagePack`, `None`, flags |
|  [08]   | `DisableSpanBasedJsonConversion`                            | `false`                       | string keys only, other keys ignore it                                   |

`EqualityComparisonOperators = None` forces `ComparisonOperators` to `None`. `TTRESG105` warns whenever the two settings differ, in either direction. The generator raises the equality setting to match a higher comparison setting. `SkipIComparable` removes `IComparable` and `IComparable<T>` but leaves the comparison operators in place.

```csharp
[SmartEnum<int>(ComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads, EqualityComparisonOperators = OperatorsGeneration.DefaultWithKeyTypeOverloads)]
internal sealed partial class Priority {
    public static readonly Priority Low = new(1);
    public static readonly Priority High = new(3);
}
```

`Priority.High > 1` and `Priority.High == 3` compile through the key type overloads, and `Priority.Low.ToString("000", CultureInfo.InvariantCulture)` yields `"001"`.

## [08]-[GENERIC_KEY_TYPES]

`TypeParamRef1` through `TypeParamRef5` stand for the type parameters of a generic Smart Enum, and the referenced parameter carries a `notnull` constraint (`TTRESG074`). `Get`, `TryGet`, `Validate`, `Items`, equality, `Switch`, `Map`, and the conversion operators are always generated. `IParsable<T>`, `ISpanParsable<T>`, `IComparable<T>`, `IFormattable`, and the comparison operators appear when the type parameter is constrained to the matching interface, and `INumber<T>` implies all of them.

```csharp
[SmartEnum<TypeParamRef1>]
internal sealed partial class Metric<T> where T : System.Numerics.INumber<T> {
    public static readonly Metric<T> Temperature = new(T.Zero);
    public static readonly Metric<T> Humidity = new(T.One);
}
```

`Metric<int>.Parse("1", null)` returns `Humidity`, and `Metric<double>.Get(0.0)` returns `Temperature`.

## [09]-[FRAMEWORK_INTEGRATION]

A project that references `Thinktecture.Runtime.Extensions.Json`, in its own manifest or through another package, gets a `JsonConverterAttribute` on every keyed Smart Enum. Without that reference, register `ThinktectureJsonConverterFactory` in `JsonSerializerOptions.Converters`. MVC reads the options of `AddControllers().AddJsonOptions`, and minimal APIs read the options of `ConfigureHttpJsonOptions`. One registration leaves the other path on the default handling. Its constructor `(bool skipObjectsWithJsonConverterAttribute, Func<Type, bool>? skipSpanBasedDeserialization)` skips attributed types and opts single types out of span-based deserialization.

`Thinktecture.Runtime.Extensions.Newtonsoft.Json` does the same with `ThinktectureNewtonsoftJsonConverterFactory`, and `Thinktecture.Runtime.Extensions.MessagePack` adds a `MessagePackFormatterAttribute` or the resolver `ThinktectureMessageFormatterResolver.Instance`. `SerializationFrameworks` restricts which attributes the generator emits.

A keyed Smart Enum serializes as its key. An unknown key on read throws `JsonException` with the message of `UnknownSmartEnumIdentifierException`. On .NET 9 and later a string key reads through a span-based converter that rejects a non-string token with `JsonException`. `DisableSpanBasedJsonConversion = true` returns to the non-span converter.

A Smart Enum also serves as the discriminator of a polymorphic converter. The item names the case in the payload and reads the case back through a delegate with a `ref Utf8JsonReader` parameter.

```csharp
internal abstract record Shape;
internal sealed record Circle(double Radius) : Shape;
internal sealed record Square(double Side) : Shape;

[SmartEnum<string>]
internal sealed partial class ShapeKind {
    public static readonly ShapeKind Circle = new("Circle", Read<Circle>);
    public static readonly ShapeKind Square = new("Square", Read<Square>);

    [UseDelegateFromConstructor]
    public partial Shape? Read(ref System.Text.Json.Utf8JsonReader reader, System.Text.Json.JsonSerializerOptions options);

    private static Shape? Read<T>(ref System.Text.Json.Utf8JsonReader reader, System.Text.Json.JsonSerializerOptions options) where T : Shape =>
        System.Text.Json.JsonSerializer.Deserialize<T>(ref reader, options);
}
```

Minimal APIs bind a Smart Enum through `IParsable<T>.TryParse`. Only a `bool` reaches the host, so a failed bind answers with a generic 400 response. MVC controllers reference `Thinktecture.Runtime.Extensions.AspNetCore` and insert `ThinktectureModelBinderProvider` at index 0 of `ModelBinderProviders`, so the default providers never see the type. Its parameter `skipBindingFromBody` defaults to `true` and leaves body values to the JSON serializer. An unknown value adds a model error, and `[ApiController]` turns the invalid state into a 400 response with the message. The model error and the `JsonException` both carry the validation error message, so `[ValidationError<TError>]` shapes the response body.

```csharp
services.AddControllers(static options => options.ModelBinderProviders.Insert(0, new ThinktectureModelBinderProvider()));
services.AddSwaggerGen().AddThinktectureOpenApiFilters(static options => {
    options.SmartEnumSchemaFilter = SmartEnumSchemaFilter.Default;
    options.SmartEnumSchemaExtension = SmartEnumSchemaExtension.VarNamesFromDotnetIdentifiers;
});
```

`Thinktecture.Runtime.Extensions.Swashbuckle` describes a Smart Enum as its key type with the allowed values. `SmartEnumSchemaFilter` selects `Default` (`enum: [...]`), `OneOf`, `AnyOf`, `AllOf`, or `FromDependencyInjection`, which resolves an `ISmartEnumSchemaFilter`. `SmartEnumSchemaExtension` selects `None`, `VarNamesFromStringRepresentation`, `VarNamesFromDotnetIdentifiers`, or `FromDependencyInjection`, and the two `VarNames` values add `x-enum-varnames`.

Entity Framework Core stores a keyed Smart Enum as its key through a value converter from `Thinktecture.Runtime.Extensions.EntityFrameworkCore10`. Each entry point has a `Configuration` overload.

- `UseThinktectureValueConverters()` on `DbContextOptionsBuilder` registers the converters for the whole model
- `AddThinktectureValueConverters()` on `ModelBuilder`, `EntityTypeBuilder`, `OwnedNavigationBuilder`, or `ComplexPropertyBuilder` narrows the scope
- `HasThinktectureValueConverter()` on `PropertyBuilder<T>`, `ComplexTypePropertyBuilder<T>`, or `PrimitiveCollectionBuilder<T>` registers one property

`Configuration.Default` measures the longest string key of each Smart Enum and rounds up to the next multiple of ten. It sets that column max length unless one is configured. `Configuration.NoMaxLength` skips that step. `SmartEnumConfiguration.MaxLengthStrategy` accepts `DefaultSmartEnumMaxLengthStrategy.Instance`, a `FixedSmartEnumMaxLengthStrategy`, a `CustomSmartEnumMaxLengthStrategy` built from a `Func<Type, Type, IReadOnlyList<ISmartEnumItem>, MaxLengthChange>`, or `NoOpSmartEnumMaxLengthStrategy.Instance`.

```csharp
services.AddDbContext<ShopDbContext>(static builder => builder.UseSqlServer(connectionString).UseThinktectureValueConverters());
Configuration fixedWidth = new() { SmartEnums = new SmartEnumConfiguration { MaxLengthStrategy = new FixedSmartEnumMaxLengthStrategy(32) } };
```

Serilog renders a keyed Smart Enum as its key once `Destructure.UsingThinktectureRuntimeExtensions()` is registered and the template uses the `@` operator. `TypesToRenderAsString.SmartEnums` switches the rendering to `ToString()`. A keyless Smart Enum is declined and falls through to Serilog's default object destructuring.

```csharp
internal static class Logging {
    public static void Emit(OrderStatus status, Serilog.Core.ILogEventSink sink) {
        using Serilog.Core.Logger logger = new Serilog.LoggerConfiguration().Destructure.UsingThinktectureRuntimeExtensions().WriteTo.Sink(sink).CreateLogger();
        logger.Information("status {@Status}", status);
    }
}
```

The rendered line is `status "Pending"`.

A discriminated union models alternatives with different shapes. A Smart Enum models a closed set of named items with the same shape and different data. When the cases need different fields, a union is the right type. An item is a `static readonly` field, not a constant, so it cannot serve as an attribute argument or a `case` label.

## [10]-[DESIGN_RULES]

- A consumer `switch` over items skips the compiler check for new items. Use `Switch` or `Map` with named arguments.
- Use the partial overloads only for an intended fallback.
- A `Switch` or `Map` lambda without `static` allocates and raises `TTRESG1001`. Use the state overload with a `static` lambda, as `Matching.Label` shows.
- `Get` at a boundary throws for user input. Map `Validate` to `Fin<T>` or `TryGet` to `Option<T>`, as `Lookup` shows.
- Declare `[ValidationError<TError>]` with a typed `Expected` record so `Validate` returns the typed error.
