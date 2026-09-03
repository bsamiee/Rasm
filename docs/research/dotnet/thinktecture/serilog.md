<!-- Integrated into .claude/skills/dotnet-coding-thinktecture/references/serilog.md -->
# [SERILOG_DESTRUCTURING]

<!-- Integrated into .claude/skills/dotnet-coding-thinktecture/references/serilog.md
The package `Thinktecture.Runtime.Extensions.Serilog` adds one destructuring policy to a Serilog logger. The policy unwraps a keyed Smart Enum and a simple Value Object to the key. It unwraps an ad hoc union to its current `Value`. Every other type falls through to Serilog's own destructuring.

## [01]-[REGISTRATION]

`UsingThinktectureRuntimeExtensions` is an extension method on `LoggerDestructuringConfiguration`. It takes an optional `TypesToRenderAsString renderAsString` parameter with the default `TypesToRenderAsString.None` and returns the `LoggerConfiguration` for further chaining. Call it once, before `CreateLogger()`. The method registers one `IDestructuringPolicy` through `Destructure.With`, and the policy needs no further configuration.

```csharp
internal static class Logging {
    public static Logger Create(TypesToRenderAsString renderAsString) =>
        new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "{Message:j}{NewLine}", formatProvider: CultureInfo.InvariantCulture)
            .Destructure.UsingThinktectureRuntimeExtensions(renderAsString)
            .CreateLogger();

    public static Logger CreateBounded(int maximumDepth) =>
        new LoggerConfiguration()
            .WriteTo.Console(outputTemplate: "{Message:j}{NewLine}", formatProvider: CultureInfo.InvariantCulture)
            .Destructure.UsingThinktectureRuntimeExtensions()
            .Destructure.ToMaximumDepth(maximumDepth)
            .CreateLogger();
}
```

The `{Message:j}` format renders every captured property as JSON. String scalars show quotes, a number shows none, and a structure shows braces with a `$type` tag.

## [02]-[POLICY_BEHAVIOR]

`TryDestructure` receives the boxed value and calls `MetadataLookup.Find` on its runtime type. The lookup walks base types, and a Smart Enum item declared as a derived class unwraps to its key. Types without Thinktecture metadata return `null`, and the policy declines. Keyed Smart Enums and simple Value Objects pass `GetKey(value)` to `CreatePropertyValue` with `destructureObjects: true`, and an ad hoc union passes `GetValue(value)`. Complex Value Objects, keyless Smart Enums, and regular unions return `null`. `TryDestructure` then returns `false`, and Serilog reflection renders them.

`GetKey` reads the key member, and `GetValue` reads the union's current `Value`. The `destructureObjects: true` argument hands the inner value back to the Serilog pipeline. The policy runs again on that inner value: a union that holds a Smart Enum unwraps through both layers. Inner values without metadata reach Serilog reflection with the same limits as any `{@Property}`.

## [03]-[TYPE_FAMILY_BEHAVIOR]

`Order` is a plain record with Thinktecture-typed members.

```csharp
[SmartEnum<string>]
internal sealed partial class OrderStatus {
    public static readonly OrderStatus Pending = new("Pending");
    public static readonly OrderStatus Paid = new("Paid");
    public static readonly OrderStatus Shipped = new("Shipped");
}

[ValueObject<decimal>]
internal readonly partial struct Amount;

[ComplexValueObject]
internal sealed partial class Boundary {
    public decimal Lower { get; }
    public decimal Upper { get; }
}

[Union<Amount, string>]
internal sealed partial class AmountOrText;

[Union<Boundary, int>]
internal sealed partial class BoundaryOrNumber;

[Union<OrderStatus, string>]
internal readonly partial struct StatusOrText;

[SmartEnum]
internal sealed partial class Channel {
    public static readonly Channel Email = new("email");
    public static readonly Channel Sms = new("sms");

    public string Name { get; }

    public override string ToString() => Name;
}

[Union]
internal abstract partial record Shape {
    internal sealed record Circle(double Radius) : Shape;
}

internal sealed record Order(OrderStatus Status, Amount Total);

internal static class Families {
    public static void Log(Logger logger) {
        Amount amount = Amount.Create(99.95m);
        Boundary boundary = Boundary.Create(1m, 10m);
        logger.Information("keyed smart enum: {@Value}", OrderStatus.Paid);
        logger.Information("simple value object: {@Value}", amount);
        logger.Information("union holding string: {@Value}", (AmountOrText)"pending");
        logger.Information("union holding value object: {@Value}", (AmountOrText)amount);
        logger.Information("union holding smart enum: {@Value}", (StatusOrText)OrderStatus.Paid);
        logger.Information("union holding complex value object: {@Value}", (BoundaryOrNumber)boundary);
        logger.Information("record with members: {@Value}", new Order(OrderStatus.Paid, amount));
        logger.Information("complex value object: {@Value}", boundary);
        logger.Information("keyless smart enum: {@Value}", Channel.Email);
        logger.Information("regular union: {@Value}", new Shape.Circle(2.5));
    }
}
```

The logger from `Logging.Create(TypesToRenderAsString.None)` prints these lines.

```text
keyed smart enum: "Paid"
simple value object: 99.95
union holding string: "pending"
union holding value object: 99.95
union holding smart enum: "Paid"
union holding complex value object: {"Lower": 1, "Upper": 10, "$type": "Boundary"}
record with members: {"Status": "Paid", "Total": 99.95, "$type": "Order"}
complex value object: {"Lower": 1, "Upper": 10, "$type": "Boundary"}
keyless smart enum: {"Name": "email", "$type": "Channel"}
regular union: {"Radius": 2.5, "$type": "Circle"}
```

The unwrapped families render their inner value, and that value renders as a scalar only when it is one. The record renders through Serilog reflection, and each member passes through the policy again. The declined families render their public instance properties: the keyless `Channel` shows `Name` because the type declares it.

Loggers without the policy print these lines for the same calls.

```text
keyed smart enum: {"Key": "Paid", "$type": "OrderStatus"}
simple value object: {"$type": "Amount"}
union holding value object: {"IsAmount": true, "IsString": false, "AsAmount": {"$type": "Amount"}, "AsString": "The property accessor threw an exception: InvalidOperationException", "Value": {"$type": "Amount"}, "$type": "AmountOrText"}
record with members: {"Status": {"Key": "Paid", "$type": "OrderStatus"}, "Total": {"$type": "Amount"}, "$type": "Order"}
```

The generated key member of a simple Value Object is a private field. Reflection finds no property, and the amount disappears. Ad hoc unions expose every `IsX` and `AsX` accessor, and the inactive `AsX` accessor throws into the log line.

## [04]-[DEPTH_LIMITS]

`CreateBounded` applies Serilog's depth limit. `ToMaximumCollectionCount` and `ToMaximumStringLength` chain the same way and cap collections and strings in every destructuring, including graphs reached through a Thinktecture value. Each unwrap by the policy re-enters the pipeline one level deeper: a Thinktecture value costs one depth level per layer. With `CreateBounded(1)` a keyed Smart Enum at the top level renders `null`, and with `CreateBounded(2)` a union that holds a Value Object renders `null`.

```text
== depth 1 ==
keyed smart enum: null
simple value object: null
== depth 2 ==
keyed smart enum: "Paid"
union holding value object: null
union holding smart enum: null
record with members: {"Status": null, "Total": null, "$type": "Order"}
```

With `CreateBounded(3)` every line of `Families.Log` renders as in the unbounded output. Record members that are unions holding a Smart Enum need a limit of four.

## [05]-[STRING_RENDERING]

`TypesToRenderAsString` is a `[Flags]` enum. Flagged families render as `new ScalarValue(value)`, and Serilog calls `ToString()` on the value when it writes the event. Families without a flag unwrap as before.

| [INDEX] | [MEMBER]       | [VALUE] | [EFFECT]                                         |
| :-----: | :------------- | :------ | :----------------------------------------------- |
|  [01]   | `None`         | 0       | Every supported family unwraps                   |
|  [02]   | `SmartEnums`   | 1       | Keyed Smart Enums render through `ToString()`    |
|  [03]   | `ValueObjects` | 2       | Simple Value Objects render through `ToString()` |
|  [04]   | `AdHocUnions`  | 4       | Ad hoc unions render through `ToString()`        |
|  [05]   | `All`          | 7       | `SmartEnums \| ValueObjects \| AdHocUnions`      |

No flag reaches a complex Value Object, a keyless Smart Enum, or a regular union, because the policy declines those families first.

```csharp
[ValueObject<int>(SkipToString = true)]
internal readonly partial struct Quantity;

internal static class Rendering {
    public static void Log(Logger logger) {
        logger.Information("keyed smart enum: {@Value}", OrderStatus.Paid);
        logger.Information("simple value object: {@Value}", Amount.Create(99.95m));
        logger.Information("union holding value object: {@Value}", (AmountOrText)Amount.Create(99.95m));
        logger.Information("union holding complex value object: {@Value}", (BoundaryOrNumber)Boundary.Create(1m, 10m));
        logger.Information("value object with SkipToString: {@Value}", Quantity.Create(3));
    }
}
```

The same calls print these results under `None`, `All`, and `AdHocUnions`.

| [INDEX] | [INPUT]                        | `None`                                           | `All`                         | `AdHocUnions`                 |
| :-----: | :----------------------------- | :----------------------------------------------- | :---------------------------- | :---------------------------- |
|  [01]   | Keyed smart enum               | `"Paid"`                                         | `"Paid"`                      | `"Paid"`                      |
|  [02]   | Simple value object            | `99.95`                                          | `"99.95"`                     | `99.95`                       |
|  [03]   | Value object in union          | `99.95`                                          | `"99.95"`                     | `"99.95"`                     |
|  [04]   | Complex value object in union  | `{"Lower": 1, "Upper": 10, "$type": "Boundary"}` | `"{ Lower = 1, Upper = 10 }"` | `"{ Lower = 1, Upper = 10 }"` |
|  [05]   | Value object with SkipToString | `3`                                              | Full type name                | `3`                           |

String-keyed Smart Enums print the same text under both settings, because their `ToString()` returns the key. Decimal keys change from a number to a string. Unions under `AdHocUnions` stop unwrapping and render through the generated `ToString()` of the union. That method returns the active member's text. Under `AdHocUnions` alone the bare `Amount` still unwraps to a number: the flags act per family, not per graph.

`SkipToString = true` removes the generated `ToString()` override and keeps the generated `IFormattable` implementation. Interpolated strings still print the key through `IFormattable`, and Serilog then prints the full type name, namespace included. Plain output templates render the scalar through `IFormattable` and print the key. JSON sinks expose the type name first. Nothing at runtime detects this case. Confirm that no type in a family declares `SkipToString = true` before that family receives its flag.

## [06]-[CAVEATS]

```csharp
[ValueObject<int>]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.All)]
internal sealed partial class Percentage {
    public static ValidationError? Validate(string? value, IFormatProvider? provider, out Percentage? item) {
        if (int.TryParse(value.AsSpan().TrimEnd('%'), NumberStyles.Integer, provider, out int number))
            return Validate(number, provider, out item);
        item = null;
        return new ValidationError("A percentage ends with '%'.");
    }

    public string ToValue() => string.Create(CultureInfo.InvariantCulture, $"{_value}%");
}

internal static class Caveats {
    public static void Log(Logger logger) {
        logger.Information("value object with object factory: {@Value}", Percentage.Create(42));
        logger.Information("default struct union: {@Value}", Uninitialized<StatusOrText>());
        logger.Information("smart enum without @: {Value}", OrderStatus.Paid);
        logger.Information("value object without @: {Value}", Amount.Create(99.95m));
        logger.Information("record without @: {Value}", new Order(OrderStatus.Paid, Amount.Create(99.95m)));
    }

    private static T Uninitialized<T>() where T : struct => default;
}
```

```text
value object with object factory: 42
default struct union: "Capturing the property value threw an exception: InvalidOperationException"
smart enum without @: "Paid"
value object without @: "99.95"
record without @: "Order { Status = Paid, Total = 99.95 }"
```

Object factories do not reach the log. The metadata carries the factories, and the policy reads the key alone: `Percentage` logs `42` while `ToValue()` returns `42%`. Model binding and Entity Framework Core need `UseForModelBinding` and `UseWithEntityFramework`.

An ad hoc union struct that was never assigned has no active member. Its `Value` throws `InvalidOperationException` with the message `This struct of type 'StatusOrText' is not initialized. Make sure all fields, properties and variables are initialized with non-default values.` Serilog catches the exception during property capture and writes the placeholder string shown above. The log call does not throw, and the event survives without the value. The analyzer reports `TTRESG047` on `default(StatusOrText)` and on `new StatusOrText()`. Fields of a struct union type draw `TTRESG104`, which requires the member to be `required`. Array elements and generic `default` draw nothing: an uninitialized union reaches a logger through those routes. `DefaultValueHandling = UnionDefaultValueHandling.MapToFirstMember` on a struct union with a stateless first member makes `default` a valid value. `Value` returns `default` of the first member type. Reference-type first members log `null`, and a struct first member logs its type tag.

The `@` operator selects destructuring. Without it Serilog calls `ToString()` and the policy never runs. String-keyed Smart Enums print the same text either way. Decimal Value Objects become a string, and a record collapses into one string.

## [07]-[DESIGN_RULES]

- Register the policy once, before `CreateLogger()`. Later registrations are inert, because Serilog takes the first policy that succeeds, and the first `renderAsString` wins.
- Write `{@Property}` for every Thinktecture value
- Log keyed types when the payload matters, declare the public properties of the declined families on purpose
- Read the key from the log and the factory value from the serializer
- Leave `TypesToRenderAsString` at `None` unless every type in the flagged family renders a meaningful `ToString()`
- Raise `ToMaximumDepth` by one for every Thinktecture layer between the log call and the key
- Assign every struct union before it reaches a log call
-->
