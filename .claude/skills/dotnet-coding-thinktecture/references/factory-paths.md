# [FACTORY_PATHS]

One `Validate` and `ToValue` pair serves System.Text.Json, Newtonsoft.Json, MessagePack, model binding, and Entity Framework Core, each integration point reaches it on its own path:
- Generated `[JsonConverter]`, Newtonsoft `[JsonConverter]`, and `[MessagePackFormatter]` attributes bind the factory value type at compile time
- Registered converter factory, model binder provider, and value converter factory read the type's factories from its metadata at runtime

## [01]-[RUNTIME_SELECTION]

Each integration point filters the type's factories by its flag and value type and falls back to the key member when no factory matches, a type without a key member has no conversion at that point:

| [INDEX] | [INTEGRATION]                | [CONDITION]                                                      | [FALLBACK]                          |
| :-----: | :--------------------------- | :--------------------------------------------------------------- | :---------------------------------- |
|  [01]   | System.Text.Json             | `SystemTextJson` flag, `T` no ref struct or `ReadOnlySpan<char>` | Key member                          |
|  [02]   | Newtonsoft.Json, MessagePack | Matching flag, `T` no ref struct                                 | Key member                          |
|  [03]   | Entity Framework Core        | `UseWithEntityFramework = true`, `T` no `ReadOnlySpan<char>`     | Key member                          |
|  [04]   | Model binding                | `UseForModelBinding = true`, `T` no `ReadOnlySpan<char>`         | Key member                          |
|  [05]   | Serilog destructuring        | None                                                             | Key member, union value, or nothing |

`ISpanParsable<TSelf>` joins `IParsable<TSelf>` when the key member is span parsable or a `ReadOnlySpan<char>` factory exists:
- Span overloads of `Parse` and `TryParse` parse the key
- `Parse("express".AsSpan(), provider: null)` on an `int`-keyed type with a `string` factory throws `FormatException`

## [02]-[ENTITY_FRAMEWORK_READ_PATH]

`HasCorrespondingConstructor = true` declares a constructor with one parameter of type `T`, generated metadata holds the expression `static TSelf (T value) => new TSelf(value)`, and the value converter compiles it for reads while `UseConstructorForRead` is `true`:
- Rows read through the constructor materialize without `Validate` and without normalization, the stored value is trusted as it is
- Rows that break the format fail inside the constructor (an index error on a row without its separator)
- JSON, MessagePack, and model binding read through `Validate` in every case, Entity Framework Core alone uses the constructor
- 059 reports a missing constructor, 060 reports a smart enum with the setting
- Without the constructor, or with `UseConstructorForRead = false`, a read calls `Validate(value, null, out item)` and the error becomes a `ValidationException`
- `null` items from `Validate` for a non-null column become an exception that names the factory and the type
- Every write calls `ToValue`

```csharp
[ComplexValueObject(DefaultStringComparison = StringComparison.OrdinalIgnoreCase)]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.Json, UseWithEntityFramework = true, UseForModelBinding = true, HasCorrespondingConstructor = true)]
internal sealed partial class Location {
    public string Store { get; }
    public string Path { get; }

    private Location(string value) => (Store, Path) = Split(value);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string store, ref string path) {
        if (string.IsNullOrWhiteSpace(store) || store.Contains(':', StringComparison.Ordinal)) validationError = new ValidationError($"store '{store}' is blank or holds ':'");
        else if (string.IsNullOrWhiteSpace(path)) validationError = new ValidationError("path is blank");
        store = store.Trim();
        path = path.Trim();
    }

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out Location? item) {
        item = null;
        if (string.IsNullOrWhiteSpace(value)) return new ValidationError("location is blank");
        (string store, string path) = Split(value);
        return Validate(store, path, out item);
    }

    public string ToValue() => $"{Store}:{Path}";

    private static (string Store, string Path) Split(string value) {
        int separator = value.IndexOf(':', StringComparison.Ordinal);
        return separator < 0 ? (value, "") : (value[..separator], value[(separator + 1)..]);
    }
}
```

`string` `Validate` splits the text and delegates to the generated member `Validate`:
- Hook trims and rejects on the JSON, model binding, and `Parse` paths
- Reads of `" store :doc"` keep the padding through the constructor and trim it through `Validate`

`Configuration` passed to `UseThinktectureValueConverters`, `AddThinktectureValueConverters`, or `HasThinktectureValueConverter` sets column lengths, strategies skip a type with a factory flagged `UseWithEntityFramework`, and every other column length comes from `HasMaxLength`:

| [INDEX] | [SETTING]                             | [DEFAULT]                               | [EFFECT]                                           |
| :-----: | :------------------------------------ | :-------------------------------------- | :------------------------------------------------- |
|  [01]   | `UseConstructorForRead`               | `true`                                  | Factories with the constructor read through it     |
|  [02]   | `SmartEnums.MaxLengthStrategy`        | `DefaultSmartEnumMaxLengthStrategy`     | Longest string key, rounded up to a multiple of 10 |
|  [03]   | `KeyedValueObjects.MaxLengthStrategy` | `NoOpKeyedValueObjectMaxLengthStrategy` | No length                                          |

Length presets and strategies:
- `Configuration.NoMaxLength` is the preset that skips the length step for both families
- Default and no-op strategies expose one `Instance`
- `FixedSmartEnumMaxLengthStrategy(32)` and `FixedKeyedValueObjectMaxLengthStrategy(32)` set one length
- `CustomSmartEnumMaxLengthStrategy` takes a `Func<Type, Type, IReadOnlyList<ISmartEnumItem>, MaxLengthChange>`
- `CustomKeyedValueObjectMaxLengthStrategy` takes a `Func<Type, Type, MaxLengthChange>`
- `MaxLengthChange.None` leaves a column alone
- Strategy's second constructor argument `overwriteExistingMaxLength` replaces a length `HasMaxLength` set before

```csharp
Configuration fixedWidth = new() { SmartEnums = new SmartEnumConfiguration { MaxLengthStrategy = new FixedSmartEnumMaxLengthStrategy(32) } };
```

## [03]-[MULTIPLE_FACTORIES]

Factories with distinct `T` coexist on one type with one `Validate` and one `ToValue` per `T`:
- Second `ToValue` is an explicit interface member (`char IConvertible<char>.ToValue()`), parameterless methods cannot share a name
- Each integration point belongs to at most one factory (068 for `UseWithEntityFramework`, 069 for `UseForModelBinding`, 070 for overlapping `UseForSerialization` flags, naming the shared framework)
- Attribute order selects nothing

```csharp
[SmartEnum<int>]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.Json, UseForModelBinding = true)]
[ObjectFactory<char>(UseForSerialization = SerializationFrameworks.MessagePack)]
internal sealed partial class Level {
    public static readonly Level Low = new(1);
    public static readonly Level High = new(2);

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out Level? item) {
        if (value is null) {
            item = null;
            return null;
        }
        item = value switch {
            "low" => Low,
            "high" => High,
            _ => null,
        };
        return item is null ? new ValidationError($"'{value}' names no level") : null;
    }

    public static ValidationError? Validate(char value, IFormatProvider? provider, out Level? item) {
        item = value switch {
            '1' => Low,
            '2' => High,
            _ => null,
        };
        return item is null ? new ValidationError($"'{value}' names no level") : null;
    }

    public string ToValue() => Key is 1 ? "low" : "high";

    char IConvertible<char>.ToValue() => Key is 1 ? '1' : '2';
}
```

- Both JSON serializers write `"high"`
- MessagePack converts through the `char` factory and encodes `'2'` as code unit 50
- `char` `Validate` takes `char value` without a nullable annotation, `T?` is `T` for a value type

## [04]-[SPAN_BASED_JSON]

`ReadOnlySpan<char>` factories flagged `SystemTextJson` receive the JSON string as a span through generated `ThinktectureSpanParsableJsonConverterFactory<TSelf, ValidationError>`:
- Reader transcodes a value of at most 128 UTF-8 bytes into a `stackalloc` buffer of 128 characters and rents longer values from `ArrayPool<char>.Shared`
- Reader unescapes an escaped value first and rejects a token other than a string or a property name with `JsonException`
- Spans matched against string constants create no `string` for a known value

```csharp
[ValueObject<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[ObjectFactory<ReadOnlySpan<char>>(UseForSerialization = SerializationFrameworks.SystemTextJson)]
internal readonly partial struct Region {
    public static ValidationError? Validate(ReadOnlySpan<char> value, IFormatProvider? provider, out Region item) =>
        value switch {
            "eu" => Validate("eu", provider, out item),
            "us" => Validate("us", provider, out item),
            _ => Validate(value.ToString(), provider, out item),
        };

    public ReadOnlySpan<char> ToValue() => _value;
}
```

- Span `Validate` delegates to the generated `string` `Validate`, the unknown branch allocates one `string`, `ToValue` returns the key field
- JSON `null` raises `JsonException` on a struct value object that disallows its default
- `ReadOnlySpan<char>` with `SystemTextJson` is the one supported ref-struct combination
- 078 reports a ref-struct factory with `UseWithEntityFramework` or `UseForModelBinding`, neither accepts a ref struct as a generic argument
- 108 warns when a ref-struct factory is flagged for a framework that ignores it, the generator binds the key member in the attributes for them
- `string` and `ReadOnlySpan<char>` factories on one type share no `SystemTextJson` flag (070)
- `Region` implements `ISpanParsable<Region>` through its parsable `string` key, `Region.Parse("eu".AsSpan(), null)` reaches the span `Validate`
- Plain classes with a span factory and no `string` factory implement neither `IParsable` nor `ISpanParsable`
- String-keyed smart enums read through the span converter without a factory, `DisableSpanBasedJsonConversion = true` returns to the string converter

## [05]-[POLYMORPHIC_DISCRIMINATOR]

Smart enums can be the discriminator of a polymorphic converter:
- Items name the case in the payload
- `[UseDelegateFromConstructor]` method with a `ref Utf8JsonReader` parameter reads the case back
- `ref` parameter makes the generator emit a nested delegate type

```csharp
internal abstract record Shape;
internal sealed record Circle(double Radius) : Shape;
internal sealed record Square(double Side) : Shape;

[SmartEnum<string>]
internal sealed partial class ShapeKind {
    public static readonly ShapeKind Circle = new("Circle", Read<Circle>);
    public static readonly ShapeKind Square = new("Square", Read<Square>);

    [UseDelegateFromConstructor]
    public partial Shape? Read(ref Utf8JsonReader reader, JsonSerializerOptions options);

    private static Shape? Read<T>(ref Utf8JsonReader reader, JsonSerializerOptions options) where T : Shape =>
        JsonSerializer.Deserialize<T>(ref reader, options);
}
```
