# Serilog Destructuring

The package `Thinktecture.Runtime.Extensions.Serilog` adds one destructuring policy to a Serilog logger. The policy unwraps a keyed Smart Enum and a simple Value Object to the key. It unwraps an ad hoc union to its current `Value`. Every other type falls through to Serilog's own destructuring. Without the policy, a `{@Property}` hole renders the public properties of the object. A simple Value Object keeps its key in a private field, so its value disappears from the line.

## Registration

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

The `{Message:j}` format renders every captured property as JSON. A string scalar shows quotes, a number shows none, and a structure shows braces with a `$type` tag.

## What the policy does

`TryDestructure` receives the boxed value and calls `MetadataLookup.Find` on its runtime type. The lookup walks base types, so a Smart Enum item declared as a derived class unwraps to its key. A type without Thinktecture metadata returns `null`, and the policy declines. A keyed Smart Enum and a simple Value Object pass `GetKey(value)` to `CreatePropertyValue` with `destructureObjects: true`, and an ad hoc union passes `GetValue(value)`. A complex Value Object, a keyless Smart Enum, and a regular union return `null`, so `TryDestructure` returns `false` and Serilog reflection renders them.

`GetKey` reads the key member, and `GetValue` reads the union's current `Value`. The `destructureObjects: true` argument hands the inner value back to the full Serilog pipeline. The policy runs again on that inner value, so a union that holds a Smart Enum unwraps through both layers. An inner value without metadata reaches Serilog reflection with the same limits as any `{@Property}`.

## Behavior by type family

`Order` is a plain record whose members are Thinktecture types.

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

The three unwrapped families render their inner value, and that value renders as a scalar only when it is one. The record renders through Serilog reflection, and each member passes through the policy again. The three declined families render their public instance properties, so the keyless `Channel` shows `Name` because the type declares it.

A logger without the policy prints the same calls as follows.

```text
keyed smart enum: {"Key": "Paid", "$type": "OrderStatus"}
simple value object: {"$type": "Amount"}
union holding value object: {"IsAmount": true, "IsString": false, "AsAmount": {"$type": "Amount"}, "AsString": "The property accessor threw an exception: InvalidOperationException", "Value": {"$type": "Amount"}, "$type": "AmountOrText"}
record with members: {"Status": {"Key": "Paid", "$type": "OrderStatus"}, "Total": {"$type": "Amount"}, "$type": "Order"}
```

The generated key member of a simple Value Object is a private field, so reflection finds no property and the amount disappears. An ad hoc union exposes every `IsX` and `AsX` accessor, and the inactive `AsX` accessor throws into the log line.

## Depth limits

`CreateBounded` applies Serilog's depth limit. `ToMaximumCollectionCount` and `ToMaximumStringLength` chain the same way and cap collections and strings reached through a Thinktecture value. The limits cap every destructuring, including graphs reached through a Thinktecture value. Each unwrap by the policy re-enters the pipeline one level deeper, so a Thinktecture value costs one depth level per layer. With `CreateBounded(1)` a keyed Smart Enum at the top level renders `null`, and with `CreateBounded(2)` a union that holds a Value Object renders `null`.

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

With `CreateBounded(3)` every line of `Families.Log` renders as in the unbounded output. When a depth limit is set, count the layers between the log call and the innermost key. A record member that is a union holding a Smart Enum needs a limit of four.

## Rendering as strings

`TypesToRenderAsString` is a `[Flags]` enum. A family that carries its flag renders as `new ScalarValue(value)`, and Serilog calls `ToString()` on the value when it writes the event. A family without its flag unwraps as before.

| Member         | Value | Effect                                           |
| -------------- | ----- | ------------------------------------------------ |
| `None`         | 0     | every supported family unwraps                   |
| `SmartEnums`   | 1     | keyed Smart Enums render through `ToString()`    |
| `ValueObjects` | 2     | simple Value Objects render through `ToString()` |
| `AdHocUnions`  | 4     | ad hoc unions render through `ToString()`        |
| `All`          | 7     | `SmartEnums \| ValueObjects \| AdHocUnions`      |

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

| Logged value                       | `None`                                           | `All`                         | `AdHocUnions`                 |
| ---------------------------------- | ------------------------------------------------ | ----------------------------- | ----------------------------- |
| keyed smart enum                   | `"Paid"`                                         | `"Paid"`                      | `"Paid"`                      |
| simple value object                | `99.95`                                          | `"99.95"`                     | `99.95`                       |
| union holding value object         | `99.95`                                          | `"99.95"`                     | `"99.95"`                     |
| union holding complex value object | `{"Lower": 1, "Upper": 10, "$type": "Boundary"}` | `"{ Lower = 1, Upper = 10 }"` | `"{ Lower = 1, Upper = 10 }"` |
| value object with SkipToString     | `3`                                              | full type name                | `3`                           |

A keyed Smart Enum with a string key prints the same text under both settings, because its `ToString()` returns the key. A decimal key changes from a number to a string. A union under `AdHocUnions` stops unwrapping and renders through the generated `ToString()` of the union. That method returns the active member's text. Under `AdHocUnions` alone the bare `Amount` still unwraps to a number, so the flags act per family, not per graph.

`SkipToString = true` removes the generated `ToString()` override and keeps the generated `IFormattable` implementation. An interpolated string still prints the key through `IFormattable`, and Serilog then prints the full type name, namespace included. A plain output template renders the scalar through `IFormattable` and prints the key, so a JSON sink exposes the type name first. Nothing at runtime detects this case. Confirm that no type in a family declares `SkipToString = true` before that family receives its flag.

## Caveats

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

Object factories do not reach the log. The metadata carries the factories, and the policy reads the key alone, so `Percentage` logs `42` while `ToValue()` returns `42%`. A serializer follows the factory, and a log line does not. Model binding and Entity Framework Core need `UseForModelBinding` and `UseWithEntityFramework`.

An ad hoc union struct that was never assigned has no active member. Its `Value` throws `InvalidOperationException` with the message `This struct of type 'StatusOrText' is not initialized. Make sure all fields, properties and variables are initialized with non-default values.` Serilog catches the exception during property capture and writes the placeholder string shown above. The log call itself does not throw, and the event survives without the value. The analyzer reports `TTRESG047` on `default(StatusOrText)` and on `new StatusOrText()`. A field of a struct union type draws `TTRESG104`, which requires the member to be marked `required`. An array element and a generic `default` draw nothing, so an uninitialized union reaches a logger through those two routes. `DefaultValueHandling = UnionDefaultValueHandling.MapToFirstMember` on a struct union with a stateless first member makes `default` a valid value. `Value` returns `default` of the first member type, so a reference marker logs `null` and a struct marker logs its type tag.

The `@` operator selects destructuring. Without it Serilog calls `ToString()` and the policy never runs. A keyed Smart Enum with a string key prints the same text either way. A decimal Value Object becomes a string, and a record collapses into one string.

## Design rules

- Register the policy once, before `CreateLogger()`. A second registration is inert, because Serilog takes the first policy that succeeds, so the first `renderAsString` wins.
- Write `{@Property}` for every Thinktecture value. A plain hole renders `ToString()` and bypasses the policy.
- Log keyed types when the payload matters. A complex Value Object, a keyless Smart Enum, and a regular union render their public properties through reflection, so declare those properties on purpose.
- Read the key from the log and the factory value from the serializer. A `ToValue()` result never appears in a log line.
- Leave `TypesToRenderAsString` at `None` unless every type in the flagged family renders a meaningful `ToString()`. A type with `SkipToString = true` logs its full type name under a JSON formatter.
- Raise `ToMaximumDepth` by one for every Thinktecture layer between the log call and the key. A limit that fits the plain object graph renders the keys as `null`.
- Assign every struct union before it reaches a log call. An unassigned struct logs a placeholder string in place of the value. With `MapToFirstMember` a reference marker logs `null` and a struct marker logs its type tag.
