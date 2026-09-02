# [OBJECT_FACTORIES]

`[ObjectFactory<T>]` declares a conversion between a type and one other type `T`. The attribute sits on a class, struct, or record. The target is a smart enum with or without a key, a simple or complex value object, or an ad hoc or regular union. Plain partial types with no other attribute are also targets. The source generator adds `IObjectFactory<TSelf, T, ValidationError>` and demands a static `Validate` method. `string` factories add `IParsable<TSelf>`. `ISpanParsable<TSelf>` joins `IParsable<TSelf>` when the key member is span parsable or a `ReadOnlySpan<char>` factory exists.

One pair of methods serves System.Text.Json, Newtonsoft.Json, MessagePack, ASP.NET Core model binding, and Entity Framework Core. The generated `[JsonConverter]`, Newtonsoft.Json `[JsonConverter]`, and `[MessagePackFormatter]` attributes bind the factory value type at compile time. The registered converter factory, model binder provider, and value converter factory read the type's object factories at runtime.

## [01]-[VALIDATE_CONTRACT]

The generator adds the interface with this exact shape, and a type that lacks the method fails to compile with `CS0535`.

```text
static ValidationError? Validate(T? value, IFormatProvider? provider, out TSelf? item)
```

- `T?` becomes `T` when `T` is a value type, a ref struct included, and `TSelf?` becomes `TSelf` when the type is a struct
- The method returns `null` on success and sets `item`, and returns a `ValidationError` on failure and sets `item` to `null`
- `null` input sets `item` to `null` and returns `null`, and no serializer and no model binder passes `null` to `Validate`
- The generated `TryParse` returns `false` for a `null` argument without calling `Validate`
- The `provider` argument arrives from the caller, and the generated `Parse` forwards its own argument
- The model binder passes the culture of the value provider result, and every serializer converter and the value converter pass `null`

Factories on a keyed type or a complex value object delegate to the generated `Validate` of the key member or of the members. Normalization in `ValidateFactoryArguments` then runs once for both construction paths. Factories where `T` equals the key type of a keyed type collide with the generated `Validate` overload and do not compile.

## [02]-[CONVERSION_DIRECTIONS]

Factories default to one-way: the type accepts `T` and never produces it. `UseForSerialization` other than `None`, or `UseWithEntityFramework = true`, makes the conversion two-way and adds `IConvertible<T>`. Types without an instance `T ToValue()` method then fail to compile with `CS0535`. The second factory implements `ToValue` as an explicit interface member, `char IConvertible<char>.ToValue()`, because two methods with the same name and no parameters cannot coexist.

## [03]-[ATTRIBUTE_PROPERTIES]

| [INDEX] | [PROPERTY]                    | [TYPE]                    | [DEFAULT]   | [EFFECT]                                                                             |
| :-----: | :---------------------------- | :------------------------ | :---------- | :----------------------------------------------------------------------------------- |
|  [01]   | `Type`                        | `Type`                    | `typeof(T)` | The value type of the factory, read-only                                             |
|  [02]   | `UseForSerialization`         | `SerializationFrameworks` | `None`      | Frameworks that convert through the factory instead of the key member                |
|  [03]   | `UseWithEntityFramework`      | `bool`                    | `false`     | Entity Framework Core persists the type as one column of type `T`                    |
|  [04]   | `UseForModelBinding`          | `bool`, init-only         | `false`     | ASP.NET Core binds the type from one route, query, header, or form value of type `T` |
|  [05]   | `HasCorrespondingConstructor` | `bool`, init-only         | `false`     | Constructor takes one `T`, and Entity Framework Core reads through it                |

`SerializationFrameworks` is a flags enum with the values `None`, `SystemTextJson`, `NewtonsoftJson`, `Json`, `MessagePack`, and `All`. `Json` combines `SystemTextJson` and `NewtonsoftJson`.

For a keyed smart enum or a simple value object, a flag replaces the key-based conversion of that integration point. For a complex value object or a union, a flag enables a conversion to and from one value that does not exist without a factory. Flags register nothing.

Projects referencing `Thinktecture.Runtime.Extensions.Json` receive a generated `[JsonConverter]` attribute unless the type carries its own. When the project that declares the type does not reference the package, the project that configures the serializer adds `ThinktectureJsonConverterFactory` to `JsonSerializerOptions.Converters`. Model binding needs `ThinktectureModelBinderProvider` inserted at index zero of `ModelBinderProviders`, and its constructor parameter `skipBindingFromBody` defaults to `true`. Entity Framework Core needs `UseThinktectureValueConverters` on `DbContextOptionsBuilder`, or `AddThinktectureValueConverters` on `ModelBuilder`, `EntityTypeBuilder`, `OwnedNavigationBuilder`, or `ComplexPropertyBuilder`, or `HasThinktectureValueConverter` on one property builder.

Serilog destructuring ignores factories. Keyed smart enums and simple value objects log the key, and an ad hoc union logs the value of the active case.

## [04]-[STRING_FACTORY_EXAMPLE]

`ShippingMethod` has an `int` key and a `string` factory. Every serializer and the model binder read and write the slug, and no serializer writes the `int` key. The generator supplies the constructor `ShippingMethod(int key, string slug)` from the key and the get-only property.

```csharp
[SmartEnum<int>]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.All, UseForModelBinding = true)]
internal sealed partial class ShippingMethod {
    public static readonly ShippingMethod Standard = new(1, "standard");
    public static readonly ShippingMethod Express = new(2, "express");

    public string Slug { get; }

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out ShippingMethod? item) {
        if (value is null) {
            item = null;
            return null;
        }
        item = value switch {
            "standard" => Standard,
            "express" => Express,
            _ => null,
        };
        return item is null ? new ValidationError($"Unknown shipping method '{value}'") : null;
    }

    public string ToValue() => Slug;
}
```

`Serialize(ShippingMethod.Express)` yields `"express"`, and a JSON number is rejected with a `JsonException` because the converter expects a string token. Slugs that `Validate` rejects become a `JsonException` carrying the message of the `ValidationError`. The generated `Parse` throws `FormatException` with the same message, and `TryParse` returns `false` for `null` and for a rejected value. The type also implements `ISpanParsable<ShippingMethod>` because the `int` key is parsable. The span overloads of `Parse` and `TryParse` parse the `int` key: `Parse("express".AsSpan(), provider: null)` throws `FormatException`.

```csharp
internal static class ShippingMethods {
    public static string Wire(ShippingMethod method) => System.Text.Json.JsonSerializer.Serialize(method);
    public static ShippingMethod? Read(string json) => System.Text.Json.JsonSerializer.Deserialize<ShippingMethod>(json);
    public static bool Accepts(string slug) => ShippingMethod.TryParse(slug, provider: null, out _);
    public static ShippingMethod Parsed(string slug) => ShippingMethod.Parse(slug, provider: null);
}
```

## [05]-[ENTITY_FRAMEWORK_READ_PATH]

`HasCorrespondingConstructor = true` declares a constructor with one parameter of type `T`, and the generated metadata carries the expression `static TSelf (T value) => new TSelf(value)`. The value converter compiles it for reads when `UseConstructorForRead` is `true`, which is its default in the `Configuration` class. Reads through the constructor skip `Validate` and every normalization. The stored value is trusted as it is. `TTRESG059` reports a type missing the declared constructor, and `TTRESG060` reports a smart enum with the property set, because smart enum items are predefined. The constructor is used by Entity Framework Core alone. JSON, MessagePack, and model binding always go through `Validate`.

Without the constructor, or with `UseConstructorForRead = false`, a read calls `Validate(value, null, out item)`. `ValidationError` becomes a `ValidationException`, and a `null` item for a non-null column value becomes an exception with a message naming the factory and the type. Writes call `ToValue`. The max length strategies apply to smart enums and keyed value objects, and they skip a type that has a factory with `UseWithEntityFramework = true`. The default configuration applies a strategy to smart enums alone, and every other column length comes from `HasMaxLength`.

`FileLocation` is a complex value object with two members and one `string` representation. `Validate` splits the string and delegates to the generated member `Validate`. `ValidateFactoryArguments` trims and rejects on both paths. The private constructor exists for the read path alone.

```csharp
[ComplexValueObject(DefaultStringComparison = StringComparison.OrdinalIgnoreCase)]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.Json, UseWithEntityFramework = true, UseForModelBinding = true, HasCorrespondingConstructor = true)]
internal sealed partial class FileLocation {
    public string Store { get; }
    public string Path { get; }

    private FileLocation(string value) => (Store, Path) = Split(value);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string store, ref string path) {
        if (string.IsNullOrWhiteSpace(store) || store.Contains(':', StringComparison.Ordinal)) validationError = new ValidationError("Store must be non-empty and must not contain ':'");
        else if (string.IsNullOrWhiteSpace(path)) validationError = new ValidationError("Path must be non-empty");
        store = store.Trim();
        path = path.Trim();
    }

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out FileLocation? item) {
        item = null;
        if (string.IsNullOrWhiteSpace(value)) return new ValidationError("A file location is not empty");
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

Reads of `" store :doc"` through the constructor keep the padding, and a read through `Validate` trims it.

## [06]-[MULTIPLE_FACTORIES]

Several factories with distinct `T` coexist on one type, one `Validate` and one `ToValue` per `T`. Each integration point belongs to at most one factory. `TTRESG068` reports two factories with `UseWithEntityFramework = true`, and `TTRESG069` reports two with `UseForModelBinding = true`. `TTRESG070` reports two with overlapping `UseForSerialization` flags and names the shared framework. At most one factory per integration point survives. The order of the attributes does not select a factory.

```csharp
[SmartEnum<int>]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.Json, UseForModelBinding = true)]
[ObjectFactory<char>(UseForSerialization = SerializationFrameworks.MessagePack)]
internal sealed partial class Dual {
    public static readonly Dual One = new(1);
    public static readonly Dual Two = new(2);

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out Dual? item) {
        if (value is null) {
            item = null;
            return null;
        }
        item = value switch {
            "one" => One,
            "two" => Two,
            _ => null,
        };
        return item is null ? new ValidationError($"Unknown value '{value}'") : null;
    }

    public static ValidationError? Validate(char value, IFormatProvider? provider, out Dual? item) {
        item = value switch {
            '1' => One,
            '2' => Two,
            _ => null,
        };
        return item is null ? new ValidationError($"Unknown value '{value}'") : null;
    }

    public string ToValue() => Key == 1 ? "one" : "two";

    char IConvertible<char>.ToValue() => Key == 1 ? '1' : '2';
}
```

Both JSON serializers write `"two"`. MessagePack converts through the `char` factory, and its formatter encodes `'2'` as the code unit 50. `char` is a struct: its `Validate` takes `char value` without a nullable annotation.

## [07]-[SPAN_BASED_JSON]

`ReadOnlySpan<char>` factories flagged `SystemTextJson` receive the JSON string as a span instead of a `string`. The generated attribute is `ThinktectureSpanParsableJsonConverterFactory<TSelf, ValidationError>`. The reader transcodes a value of at most 128 UTF-8 bytes into a `stackalloc` buffer of 128 characters. Longer values rent from `ArrayPool<char>.Shared`. Escaped values are unescaped through `CopyString` first. Tokens other than a string or a property name draw a `JsonException`. Spans match string constants, and a known value creates no `string`.

`TTRESG078` reports a ref-struct factory with `UseWithEntityFramework = true` or `UseForModelBinding = true`. Neither a value converter nor a model binder accepts a ref struct as a generic argument. `TTRESG108` warns when a ref-struct factory is flagged for a framework that ignores it, and its message lists those frameworks. `ReadOnlySpan<char>` with `SystemTextJson` is the one supported combination, and any other ref struct also triggers the warning under `SystemTextJson`. The generator then binds the key member in the attribute it emits for those frameworks. `string` and `ReadOnlySpan<char>` factories on one type must not both carry `SystemTextJson`, and `TTRESG070` reports the overlap. String-keyed smart enums already deserialize through the span converter, and `DisableSpanBasedJsonConversion = true` on `[SmartEnum<string>]` opts out.

`Region` is a `string`-keyed value object. The span `Validate` matches the known values and delegates to the generated `string` `Validate`, and the unknown branch allocates one `string`. `ToValue` returns the generated key field `_value`.

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

`Region` implements `ISpanParsable<Region>` because the `string` key is parsable, and `Region.Parse("eu".AsSpan(), provider: null)` reaches the span `Validate`. Plain classes with a span factory and no `string` factory implement neither `IParsable` nor `ISpanParsable`. JSON `null` raises a `JsonException` for `Region`, because a value object struct disallows its default value.

## [08]-[UNIONS_AND_PLAIN_TYPES]

Ad hoc unions carry no type discriminator. Factories alone serialize them as one value. `Validate` assigns a `T1` or `T2` value to `item` through the implicit conversion of the union, and `ToValue` renders the active case with `Switch`. Regular unions, abstract partial records with case records, accept the same attribute as an alternative to polymorphic JSON.

```csharp
[Union<string, int>(T1Name = "Text", T2Name = "Number")]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.All, UseForModelBinding = true)]
internal sealed partial class TextOrNumber {
    public static ValidationError? Validate(string? value, IFormatProvider? provider, out TextOrNumber? item) {
        item = null;
        if (value is null) return null;
        if (value.StartsWith("text:", StringComparison.Ordinal)) item = value["text:".Length..];
        else if (value.StartsWith("number:", StringComparison.Ordinal) && int.TryParse(value["number:".Length..], NumberStyles.Integer, CultureInfo.InvariantCulture, out int number)) item = number;
        return item is null ? new ValidationError($"Unknown text-or-number '{value}'") : null;
    }

    public string ToValue() => Switch(
        text: static text => $"text:{text}",
        number: static number => string.Create(CultureInfo.InvariantCulture, $"number:{number}"));
}
```

Keyless smart enums have no key member: a factory is the single route to serialization and model binding. `SkipFactoryMethods = true` removes a value object's converters, and a factory with `UseForSerialization` restores them.

```csharp
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.SystemTextJson)]
internal sealed partial class Slug {
    private Slug(string value) => Value = value;

    public string Value { get; }

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out Slug? item) {
        item = null;
        if (value is null) return null;
        if (value.Length == 0 || value.Contains(' ', StringComparison.Ordinal)) return new ValidationError("A slug has no spaces and is not empty");
        item = new Slug(value);
        return null;
    }

    public string ToValue() => Value;
}
```

`Slug` round-trips `"Hello"` unchanged, and JSON `null` yields a `null` reference.

## [09]-[RUNTIME_FACTORY_SELECTION]

Each integration point filters the type's object factories and falls back to the key member when no factory matches. Types with no key member have no conversion at that point.

| [INDEX] | [INTEGRATION]                | [CONDITION]                                                                | [FALLBACK]                          |
| :-----: | :--------------------------- | :------------------------------------------------------------------------- | :---------------------------------- |
|  [01]   | System.Text.Json             | `SystemTextJson` flag, `T` not a ref struct or `T` is `ReadOnlySpan<char>` | Key member                          |
|  [02]   | Newtonsoft.Json, MessagePack | The matching flag, `T` not a ref struct                                    | Key member                          |
|  [03]   | Entity Framework Core        | `UseWithEntityFramework = true`, `T` not `ReadOnlySpan<char>`              | Key member                          |
|  [04]   | ASP.NET Core model binding   | `UseForModelBinding = true`, `T` not `ReadOnlySpan<char>`                  | Key member                          |
|  [05]   | Serilog destructuring        | None                                                                       | Key member, union value, or nothing |

## [10]-[DESIGN_RULES]

- Reserve `Validate` returning no error and a `null` item for `null` input: it makes `Parse` return `null` and an Entity Framework Core read throw
- Register the serializer, the model binder provider, and the value converters at the host
- Set `HasCorrespondingConstructor = true` only when validation is expensive and the database is trusted
- Give a `ReadOnlySpan<char>` factory the `SystemTextJson` flag alone
- Give each factory a distinct `T` and each integration point one owner
- Serialize an ad hoc union through a `string` factory, never through a hand-written converter
- `Fin<T>` or `Validation<Error, T>` adapters call `Validate`, never `Create`
