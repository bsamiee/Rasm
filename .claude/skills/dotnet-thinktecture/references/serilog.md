# [SERILOG]

The `Serilog` package (Serilog 4.0.0 or newer) adds one `IDestructuringPolicy` that unwraps a keyed smart enum and a simple value object to the key and an ad hoc union to its current `Value`, and every other type falls through to Serilog's own destructuring.

## [01]-[REGISTRATION]

`UsingThinktectureRuntimeExtensions` is an extension method on `LoggerDestructuringConfiguration` with an optional `TypesToRenderAsString renderAsString` parameter (default `None`) that returns the `LoggerConfiguration` for chaining, it registers the policy through `Destructure.With` with no further configuration, and it runs once before `CreateLogger()`, because Serilog takes the first policy that succeeds and the first `renderAsString` wins:

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

The `{Message:j}` format renders every captured property as JSON: a string scalar shows quotes, a number shows none, and a structure shows braces with a `$type` tag.

## [02]-[POLICY]

`TryDestructure` receives the boxed value, calls `MetadataLookup.Find` on its runtime type, and the lookup walks base types, so a smart enum item declared as a derived class unwraps to its key. Types without metadata return `null` and the policy declines, keyed smart enums and simple value objects pass `GetKey(value)` to `CreatePropertyValue` with `destructureObjects: true`, ad hoc unions pass `GetValue(value)`, and complex value objects, keyless smart enums, and regular unions return `null`, so `TryDestructure` returns `false` and Serilog reflection renders their public instance properties with a `$type` property. `destructureObjects: true` hands the inner value back to the pipeline, the policy runs again on it, a union that holds a smart enum unwraps through both layers, and an inner value without metadata reaches reflection with the limits of any `{@Property}`. Object factories never reach the log, the policy reads the key alone, and a value object with a `string` factory that renders `42%` logs `42`:

```csharp
[SmartEnum<string>]
internal sealed partial class Status {
    public static readonly Status Pending = new("Pending");
    public static readonly Status Paid = new("Paid");
}

[ValueObject<decimal>]
internal readonly partial struct Amount;

[ComplexValueObject]
internal sealed partial class Interval {
    public decimal Lower { get; }
    public decimal Upper { get; }
}

[Union<Amount, string>]
internal sealed partial class AmountOrText;

[Union<Interval, int>]
internal sealed partial class IntervalOrNumber;

[Union<Status, string>]
internal readonly partial struct StatusOrText;

[SmartEnum]
internal sealed partial class Channel {
    public static readonly Channel Email = new("email");

    public string Name { get; }

    public override string ToString() => Name;
}

[Union]
internal abstract partial record Shape {
    internal sealed record Circle(double Radius) : Shape;
}

internal sealed record Entry(Status Status, Amount Total);

internal static class Families {
    public static void Log(Logger logger) {
        Amount amount = Amount.Create(99.95m);
        Interval interval = Interval.Create(1m, 10m);
        logger.Information("keyed smart enum: {@Value}", Status.Paid);
        logger.Information("simple value object: {@Value}", amount);
        logger.Information("union holding string: {@Value}", (AmountOrText)"pending");
        logger.Information("union holding value object: {@Value}", (AmountOrText)amount);
        logger.Information("union holding smart enum: {@Value}", (StatusOrText)Status.Paid);
        logger.Information("union holding complex value object: {@Value}", (IntervalOrNumber)interval);
        logger.Information("record with members: {@Value}", new Entry(Status.Paid, amount));
        logger.Information("complex value object: {@Value}", interval);
        logger.Information("keyless smart enum: {@Value}", Channel.Email);
        logger.Information("regular union: {@Value}", new Shape.Circle(2.5));
    }
}
```

```text
keyed smart enum: "Paid"
simple value object: 99.95
union holding string: "pending"
union holding value object: 99.95
union holding smart enum: "Paid"
union holding complex value object: {"Lower": 1, "Upper": 10, "$type": "Interval"}
record with members: {"Status": "Paid", "Total": 99.95, "$type": "Entry"}
complex value object: {"Lower": 1, "Upper": 10, "$type": "Interval"}
keyless smart enum: {"Name": "email", "$type": "Channel"}
regular union: {"Radius": 2.5, "$type": "Circle"}
```

The unwrapped families render their inner value, that value renders as a scalar only when it is one, the record renders through reflection with each member passing through the policy again, and the declined families render the public instance properties they declare. Without the policy the same calls print `{"Key": "Paid", "$type": "Status"}` for the smart enum, `{"$type": "Amount"}` for the value object because the generated key member is a private field that reflection never finds, and an ad hoc union prints every `IsX` and `AsX` accessor with the inactive `AsX` throwing `InvalidOperationException` into the log line.

## [03]-[DEPTH_LIMITS]

`ToMaximumDepth`, `ToMaximumCollectionCount`, and `ToMaximumStringLength` chain on `Destructure` and cap every destructuring, including a graph reached through an unwrapped value, and each unwrap by the policy re-enters the pipeline one level deeper, so a generated value costs one depth level per layer: `CreateBounded(1)` renders a keyed smart enum at the top level as `null`, `CreateBounded(2)` renders a union that holds a value object as `null` and a record as `{"Status": null, "Total": null, "$type": "Entry"}`, `CreateBounded(3)` renders every line of `Families.Log` as in the unbounded output, and a record member that is a union holding a smart enum needs 4. Raise the limit by one for every generated layer between the log call and the key.

## [04]-[STRING_RENDERING]

`TypesToRenderAsString` is a `[Flags]` enum, a flagged family renders as `new ScalarValue(value)` and Serilog calls `ToString()` on it when it writes the event, an unflagged family unwraps as before, and no flag reaches a complex value object, a keyless smart enum, or a regular union, because the policy declines those first:

| [INDEX] | [MEMBER]       | [VALUE] | [EFFECT]                                         |
| :-----: | :------------- | :------ | :----------------------------------------------- |
|  [01]   | `None`         | 0       | Every supported family unwraps                   |
|  [02]   | `SmartEnums`   | 1       | Keyed smart enums render through `ToString()`    |
|  [03]   | `ValueObjects` | 2       | Simple value objects render through `ToString()` |
|  [04]   | `AdHocUnions`  | 4       | Ad hoc unions render through `ToString()`        |
|  [05]   | `All`          | 7       | `SmartEnums \| ValueObjects \| AdHocUnions`      |

| [INDEX] | [INPUT]                          | `None`                                           | `All`                         | `AdHocUnions`                 |
| :-----: | :------------------------------- | :----------------------------------------------- | :---------------------------- | :---------------------------- |
|  [01]   | Keyed smart enum                 | `"Paid"`                                         | `"Paid"`                      | `"Paid"`                      |
|  [02]   | Simple value object              | `99.95`                                          | `"99.95"`                     | `99.95`                       |
|  [03]   | Value object in union            | `99.95`                                          | `"99.95"`                     | `"99.95"`                     |
|  [04]   | Complex value object in union    | `{"Lower": 1, "Upper": 10, "$type": "Interval"}` | `"{ Lower = 1, Upper = 10 }"` | `"{ Lower = 1, Upper = 10 }"` |
|  [05]   | Value object with `SkipToString` | `3`                                              | Full type name                | `3`                           |

A string-keyed smart enum prints the same text under every setting because its `ToString()` returns the key, a decimal key changes from a number to a string, a union under `AdHocUnions` stops unwrapping and renders through its generated `ToString()`, which returns the active member's text, and the flags act per family and not per graph, so a bare `Amount` under `AdHocUnions` alone still unwraps to a number. `SkipToString = true` removes the generated `ToString()` override and keeps the generated `IFormattable`, an interpolated string and a plain output template still print the key through `IFormattable`, Serilog prints the full type name with its namespace and a JSON sink exposes the type name first, nothing at runtime detects the case, so a family receives its flag only after every type in it is confirmed to declare no `SkipToString = true`, and `None` stays in place unless every type in the flagged family renders a meaningful `ToString()`.

## [05]-[CAVEATS]

The `@` operator selects destructuring, and without it Serilog calls `ToString()` and the policy never runs: a string-keyed smart enum prints the same text either way, a decimal value object becomes a string, and a record collapses into one string (`"Entry { Status = Paid, Total = 99.95 }"`). An ad hoc union struct that was never assigned has no active member, its `Value` throws `InvalidOperationException` (`This struct of type 'StatusOrText' is not initialized. Make sure all fields, properties and variables are initialized with non-default values.`), Serilog catches the exception during property capture and writes `"Capturing the property value threw an exception: InvalidOperationException"` in its place, and the log call survives without the value:
- The analyzer reports 047 on `default(StatusOrText)` and `new StatusOrText()` and 104 on a settable field or property of the type until it is `required`, and an array element or a generic `default` draws nothing, so an uninitialized union reaches a logger through those routes unless every struct union is assigned before the log call
- `DefaultValueHandling = UnionDefaultValueHandling.MapToFirstMember` with a stateless first member makes `default` valid, `Value` returns `default` of the first member type, a reference-type first member logs `null`, and a struct first member logs its type tag
