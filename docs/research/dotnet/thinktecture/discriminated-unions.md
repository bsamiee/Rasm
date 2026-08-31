# [DISCRIMINATED_UNIONS]

A discriminated union holds one value that belongs to exactly one of its declared cases. A union names data alternatives. The source generator emits `Switch` and `Map` with one arm per case, and a missing arm is a compile error.

## [01]-[MOTIVATION]

A tuple such as `(Order? Order, bool IsSoftDeleted, string? Error)` admits every combination of its fields, and every consumer interprets those fields by convention. A `Result<T>` class with `IsSuccess`, `Value`, and `Error` lets a consumer read `Value` after a failure, and a new status adds no compile error. An exception for an expected outcome hides that outcome from the method signature, and a missing `catch` compiles.

A union moves the alternatives into the type. Candidates for a union are boolean or enum fields that decide how other properties are read, chains of type tests, tuples with mutually exclusive nullable fields, and base classes whose derived classes differ only in state data.

## [02]-[AD_HOC_VERSUS_REGULAR]

An ad hoc union combines existing types that share no base, such as `string`, `int`, or `List<ValidationError>`. A regular union is a class hierarchy whose cases derive from one abstract partial base and carry their own properties and behavior. A smart enum is a fixed set of named instances with one shape, and a union is a fixed set of shapes. A smart enum item can return a union.

| [INDEX] | [ASPECT]    | [AD_HOC_UNION]                                                  | [REGULAR_UNION]                                              |
| :-----: | :---------- | :-------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | Declaration | `partial class`, `partial struct`, or `ref partial struct`      | `partial class` or `partial record`, generated as `abstract` |
|  [02]   | Attribute   | `[Union<T1, T2>]` through `[Union<..., T5>]`, or `[AdHocUnion]` | `[Union]` on the base                                        |
|  [03]   | Cases       | the type arguments                                              | nested types that derive from the base                       |
|  [04]   | Generic     | `TypeParamRef1` to `TypeParamRef5` name the type parameters     | the base can be generic, a case cannot (`TTRESG053`)         |

## [03]-[AD_HOC_UNIONS]

`AdHocUnion` with `typeof` exists for member types a generic attribute cannot spell, such as `List<string?>`. Both attributes share the five-member bound and generate the same members. The generated names derive from the member types: `IsString`, `AsString`, `IsInt32`, `AsInt32`. `Value` returns the current member as `object`.

`AsString` on a union that holds an `int` throws `InvalidOperationException`. The text is `'TextNumberOrFlag' is not of type 'string' but of type 'int'.` The explicit cast takes the same path. Equality compares the discriminator and then the member value, and two `string` members compare with `StringComparison.OrdinalIgnoreCase` unless `DefaultStringComparison` says otherwise. `ToString` and `GetHashCode` delegate to the member, and the generated `ToString` returns `string?`.

C# forbids user-defined conversions from `object` and from interfaces, so a member of one of those types receives a constructor and no operator. Every member type must be at least as accessible as the union, because the generated operators and accessors expose it (`TTRESG077`). A union needs at least two member types (`TTRESG067`) and no more than one union attribute (`TTRESG066`).

## [04]-[AD_HOC_SETTINGS]

| [INDEX] | [PROPERTY]                    | [DEFAULT]           | [EFFECT]                                                                                              |
| :-----: | :---------------------------- | :------------------ | :---------------------------------------------------------------------------------------------------- |
|  [01]   | `T1Name` to `T5Name`          | type name           | renames `IsX`, `AsX`, `CreateX`, `NormalizeX`, and the `Switch`/`Map` arm                             |
|  [02]   | `DefaultStringComparison`     | `OrdinalIgnoreCase` | comparison for `string` members in `Equals` and `GetHashCode`                                         |
|  [03]   | `SkipToString`                | `false`             | suppresses the `ToString` override                                                                    |
|  [04]   | `SkipEqualityComparison`      | `false`             | suppresses `Equals`, `GetHashCode`, `==`, `!=`, `IEquatable<T>`, and `IEqualityOperators<T, T, bool>` |
|  [05]   | `ConstructorAccessModifier`   | `Public`            | accessibility of constructors and factory methods, never of operators                                 |
|  [06]   | `ConversionFromValue`         | `Implicit`          | operator from a member type to the union, `None` disables                                             |
|  [07]   | `ConversionToValue`           | `Explicit`          | operator from the union to a member type, `None` disables                                             |
|  [08]   | `T1IsNullableReferenceType`   | `false`             | types the member as `string?` and admits `null`                                                       |
|  [09]   | `T1IsStateless`               | `false`             | stores only the discriminator for that member                                                         |
|  [10]   | `ValueMemberAccessModifier`   | `Public`            | accessibility of `Value`, generated code keeps reading it                                             |
|  [11]   | `ValueMemberName`             | `Value`             | renames the raw accessor and frees `Value` for a hand-written property                                |
|  [12]   | `UseSingleBackingField`       | `false`             | boxes every member into one `object?` field                                                           |
|  [13]   | `SingleBackingFieldType`      | none                | types the single field and `Value`                                                                    |
|  [14]   | `FactoryMethodGeneration`     | `Default`           | `Always` or `None` overrides the trigger rule for `CreateX`                                           |
|  [15]   | `DefaultValueHandling`        | `Disallow`          | `MapToFirstMember` makes `default` of a struct union the first member                                 |
|  [16]   | `SwitchMethods`, `MapMethods` | `Default`           | `DefaultWithPartialOverloads` adds `SwitchPartially` and `MapPartially`, `None` removes all           |
|  [17]   | `SwitchMapStateParameterName` | `state`             | name of the state parameter in `Switch` and `Map`                                                     |

A union that adds its own properties sets `ConversionFromValue = ConversionOperatorsGeneration.None` and `ConstructorAccessModifier = UnionConstructorAccessModifier.Private`. The hand-written constructors are then the only entry points and chain to the generated ones.

```csharp
[Union<string, int>(T1Name = "Text", T2Name = "Number", ConversionFromValue = ConversionOperatorsGeneration.None, ConstructorAccessModifier = UnionConstructorAccessModifier.Private)]
internal sealed partial class LabeledTextOrNumber {
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public LabeledTextOrNumber(string text, string label) : this(text) => Label = label;
    public required string Label { get; init; }
}
```

The generated private constructors do not assign `Label`, so the property is `required` and the public constructors declare `SetsRequiredMembers`.

The generator declares `static partial void Normalize{MemberName}(ref TMember {memberName})` for every member that carries state. An implementation rewrites the value before the backing field is written, so equality, `ToString`, `Switch`, `Value`, and every serializer read the normalized value.

```csharp
[Union<string, int>(T1Name = "Text", T2Name = "Number")]
internal sealed partial class NamedTextOrNumber {
    static partial void NormalizeText(ref string text) => text = text?.Trim().ToLowerInvariant() ?? "";
}
```

The parameter name is the member name in camel case, and a different name compiles with `CS8826`. The call is the first statement of the generated constructor, and no null check precedes it, so a `null` argument reaches the `Normalize` method body. The conversion operators delegate to that constructor. A member whose type another member repeats gets `Create{MemberName}` instead of a constructor, and the call moves there. A stateless member has no `Normalize` method. A union with an object factory reads through `Validate`, which constructs the union, so every framework read runs the `Normalize` method.

## [05]-[BACKING_FIELDS]

A union with at most one reference type keeps one typed field per member. Two or more reference types share one `object?` field, and value types keep their fields, so no boxing occurs. `UseSingleBackingField = true` moves the value types into the shared field and boxes them. `SingleBackingFieldType` names a common base or interface for that field, and `Value` takes that type.

`Value` typed as the shared interface reaches the members every case declares, without a `Switch`. `SingleBackingFieldType` implies `UseSingleBackingField = true`, and an explicit `false` beside it is `TTRESG075`. Every member type needs a built-in implicit conversion to the field type, because the stored value must stay the original instance. A user-defined conversion is `TTRESG079`. `typeof(object)` equals `UseSingleBackingField = true`.

## [06]-[STATELESS_MEMBERS]

A stateless member is a type whose presence is the whole information. The union stores only the discriminator, `AsX` and `Value` return `default(T)`, equality compares the discriminator alone, and `CreateX` is parameterless. Prefer a `readonly record struct`, because `default` of a class is `null`, and a stateless reference type sets `TxIsNullableReferenceType = true` on its own.

```csharp
internal sealed record SuccessResponse(string Data);
internal readonly record struct NotFoundError;

[Union<SuccessResponse, NotFoundError>(T1Name = "Success", T2Name = "NotFound", T2IsStateless = true)]
internal sealed partial class ApiResponse;
```

`default` of a struct union has no member.

- The analyzer reports `TTRESG047` on `default(TUnion)` and `new TUnion()`
- At runtime `Value`, `Switch`, `Map`, `ToString`, and `GetHashCode` throw `InvalidOperationException` with the text `This struct of type '...' is not initialized.`
- The `==` operator throws only when both operands are uninitialized, and the `IsX` properties return `false` without throwing
- `DefaultValueHandling = UnionDefaultValueHandling.MapToFirstMember` turns `default` into the first member
- The first member must be stateless (`TTRESG082`), and the union must be a struct (`TTRESG081`)
- The generator then drops `IDisallowDefaultValue` from the base list, so `TTRESG047` stops
- A hand-written `IDisallowDefaultValue` restores the analyzer check while the runtime value stays valid
- The analyzer also reports `default` inside the union itself, so a hand-written property such as `MaybeInt.None` constructs the stateless member instead

```csharp
internal readonly record struct Absent;

[Union<Absent, int>(T1IsStateless = true, DefaultValueHandling = UnionDefaultValueHandling.MapToFirstMember)]
internal readonly partial struct MaybeInt : IDisallowDefaultValue {
    public static MaybeInt None => new Absent();
}
```

An uninitialized `MaybeInt`, such as an array element, has `IsAbsent == true`, equals `MaybeInt.None`, shares its hash code, and `Value` returns `default(Absent)`.

## [07]-[GENERIC_AD_HOC_UNIONS]

`TypeParamRef1` to `TypeParamRef5` name the union's own type parameters, also inside constructed types such as `List<TypeParamRef1>`.

- A type parameter member gets no conversion operator, because the operator breaks for specific type arguments
- `Result<string>` makes every conversion ambiguous (`CS0457`), and an interface argument makes the conversion from an interface-typed variable fail (`CS0029`)
- An `object` argument makes `(object)union` return the boxed union
- An `object` argument also sends `Result<object> r = "text";` into the `string` member, because overload resolution picks the most specific parameter type
- The generator emits `CreateX` factory methods for every member as soon as one member is a type parameter, an interface, `object`, or a duplicate type
- An author who controls every instantiation adds the operator and delegates to the factory, so normalization still runs

```csharp
[Union<TypeParamRef1, string>]
internal readonly partial struct Result<T> {
    public static implicit operator Result<T>(T value) => CreateT(value);
}
```

`Result<int>.CreateT(42)`, `new Result<int>(42)`, and the generated `Result<int>.CreateString("text")` all exist, and the `string` member keeps its implicit operator. `TypeParamRef` above the type parameter count is `TTRESG071`, and on a non-generic union it is `TTRESG072`. An `allows ref struct` type parameter is `TTRESG073`. A generic union that references no type parameter is `TTRESG107`.

## [08]-[REGULAR_UNIONS]

The generator gives the base a private constructor, so a type declared outside the base cannot derive from it. A class case is `sealed` or has private constructors only (`TTRESG054`), and a record case is `sealed` (`TTRESG055`). A non-abstract case is at least as accessible as the base (`TTRESG056`). A nested type that does not derive from the base is `TTRESG106`. A generated type never receives a primary constructor (`TTRESG043`), so a positional record case is the natural shape, and the base declares no primary constructor.

```csharp
[Union]
internal abstract partial record OrderState {
    public abstract bool CanCancel();

    internal sealed record Placed(string CreatedBy) : OrderState {
        public override bool CanCancel() => true;
    }
    internal sealed record Processing(DateTime StartedAt) : OrderState {
        public override bool CanCancel() => true;
    }
    internal sealed record Shipped(DateTime ShippedAt, string TrackingNumber) : OrderState {
        public override bool CanCancel() => false;
    }
}
```

An abstract method suits behavior that belongs to the state and needs no dependency. A transition that reads context lives outside the union and passes that context through the `Switch` overload that takes a state parameter. The arms return different case types, so the call names `TResult`.

```csharp
internal sealed record ShipRequest(DateTime Now, string TrackingNumber, bool CanShip);

internal static class OrderTransitions {
    public static OrderState Ship(OrderState state, ShipRequest request) =>
        state.Switch<ShipRequest, OrderState>(request,
            placed: static (_, placed) => placed,
            processing: static (ship, processing) => ship.CanShip ? new OrderState.Shipped(ship.Now, ship.TrackingNumber) : processing,
            shipped: static (_, shipped) => shipped);
}
```

The generator emits an implicit conversion to the base for every case with a single-parameter constructor whose parameter type is unique among those cases. `OrderState` converts from `string` into `Placed` and from `DateTime` into `Processing`, so `return "me";` in a method that returns `OrderState` yields a `Placed`. `ConversionFromValue = ConversionOperatorsGeneration.None` on `[Union]` removes these operators.

A class case with `[Union]` becomes an abstract nested union with a generated private constructor, and its own cases nest inside it. Records cannot nest unions, because every record case is `sealed`.

- The arm names of the outer `Switch` prefix the nested case with its parent, `failureNotFound`
- `NestedUnionParameterNames = NestedUnionParameterNameGeneration.Simple` drops the prefix
- The exhaustive signature lists the nested cases first in reverse declaration order, then the direct cases in declaration order
- Simple names collide when two nested unions declare a case with the same name, and the compiler reports the duplicate parameter
- `[UnionSwitchMapOverload(StopAt = [typeof(Failure)])]` adds a non-exhaustive overload with one arm per listed type and its siblings
- The `StopAt` overload lets a dedicated method handle the nested union
- The nested union's own `Switch` uses its own case names, `notFound` and `unauthorized`

```csharp
[Union]
[UnionSwitchMapOverload(StopAt = [typeof(Failure)])]
internal abstract partial class RequestOutcome {
    internal sealed class Success : RequestOutcome;

    [Union]
    internal abstract partial class Failure : RequestOutcome {
        internal sealed class NotFound : Failure;
        internal sealed class Unauthorized : Failure;
    }
}

internal static class RequestOutcomes {
    public static int StatusCode(RequestOutcome outcome) => outcome.Map(failureUnauthorized: 401, failureNotFound: 404, success: 200);
    public static string Group(RequestOutcome outcome) => outcome.Map(success: "ok", failure: "failed");
}
```

The `StopAt` overload gives up exhaustiveness: a new case under `Failure` compiles without a new arm in `Group`.

## [09]-[SWITCH_AND_MAP]

`Switch` has a void overload with one `Action` per case and a value overload with one `Func<TCase, TResult>` per case. A state overload of each passes a `TState` first. `Map` takes one `TResult` value per case. Every argument is named (`TTRESG046`), and a lambda without `static` is `TTRESG1001`, so captured context travels through the state parameter and the lambdas are `static`. When arms return different types, `TResult` inference fails on the whole call, and an explicit `Switch<TResult>` moves the error to the one arm that differs.

`SwitchMethods`, `MapMethods`, `SwitchMapStateParameterName`, and `ConversionFromValue` are declared on `[Union]` as well as on the ad hoc attributes. On an ad hoc union the `@default` arm receives the current member as `object?`, and on a regular union it receives the base type. The value overload requires `@default`, and the void overload declares it optional. A void `SwitchPartially` without `@default` does nothing for an unhandled case. `MapPartially` requires `@default` and takes the other arms as optional values.

```csharp
[Union<string, int, bool>(SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads, MapMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
internal sealed partial class TextNumberOrFlag;

internal static class PartialMatching {
    public static string Label(TextNumberOrFlag union) => union.MapPartially(@default: "other", @string: "text");
    public static int Length(TextNumberOrFlag union) => union.SwitchPartially(@default: static _ => 0, @string: static text => text.Length);
}
```

Every generated `Switch` and `Map` ends in an unreachable branch. On an ad hoc union `Switch` throws `IndexOutOfRangeException` and `Map` throws `InvalidOperationException`. Both carry the text `Unexpected value index '...'.` A regular union throws `InvalidOperationException` with the text `Unexpected type '...'.` from every arm.

## [10]-[USE_CASES]

A date known to the year, the month, or the day is three cases with a shared `Year`. The base holds the shared property and a private constructor, and the record cases pass `Year` up. The hand-written operator admits `DateOnly`.

```csharp
[System.Text.Json.Serialization.JsonDerivedType(typeof(YearOnly), "Year")]
[System.Text.Json.Serialization.JsonDerivedType(typeof(YearMonth), "YearMonth")]
[System.Text.Json.Serialization.JsonDerivedType(typeof(Exact), "Date")]
[Union]
internal abstract partial record PartiallyKnownDate {
    private PartiallyKnownDate(int year) => Year = year;

    public int Year { get; }

    internal sealed record YearOnly(int Year) : PartiallyKnownDate(Year);
    internal sealed record YearMonth(int Year, int Month) : PartiallyKnownDate(Year);
    internal sealed record Exact(int Year, int Month, int Day) : PartiallyKnownDate(Year);

    public static implicit operator PartiallyKnownDate(DateOnly date) => new Exact(date.Year, date.Month, date.Day);
}
```

`YearOnly(int)` is the only single-parameter case, so the generator also emits an implicit conversion from `int`. `System.Text.Json.JsonSerializer.Serialize<PartiallyKnownDate>(date)` writes `{"$type":"Date","Month":3,"Day":15,"Year":2024}`, and `Deserialize<PartiallyKnownDate>` returns an `Exact`.

A case can be a value object or a smart enum, so the union names the kind and each case owns its value and rules. `Unknown` is a declared case with one instance, not `null`.

```csharp
[Union]
internal abstract partial class Jurisdiction {
    [ValueObject<string>(KeyMemberName = "IsoCode")]
    [KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
    [KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
    internal sealed partial class Country : Jurisdiction;

    [ComplexValueObject(SkipFactoryMethods = true)]
    internal sealed partial class Unknown : Jurisdiction {
        public static readonly Unknown Instance = new();
    }

    [SmartEnum<string>]
    internal sealed partial class Continent : Jurisdiction {
        public static readonly Continent Europe = new("Europe");
        public static readonly Continent Asia = new("Asia");
    }
}
```

`Unknown` has a private constructor, so `Instance` is its only value.

## [11]-[FRAMEWORK_INTEGRATION]

An ad hoc union carries no type discriminator, so no serializer handles it on its own. `[ObjectFactory<string>]` declares one wire format: `ToValue` renders the union, `Validate` parses it back and returns a `ValidationError` for bad input. With `UseForSerialization` set and `Thinktecture.Runtime.Extensions.Json` referenced, the generator applies a `JsonConverter` attribute with `ThinktectureJsonConverterFactory<TUnion, ValidationError>` to the type, so no options registration is needed. `UseForModelBinding = true` and `UseWithEntityFramework = true` reuse the same pair. A `string` factory also implements `IParsable<TUnion>`.

```csharp
[Union<string, int>(T1Name = "Text", T2Name = "Number")]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.SystemTextJson)]
internal sealed partial class TextOrNumberSerializable {
    public string ToValue() => Switch(
        text: static text => $"Text|{text}",
        number: static number => string.Create(CultureInfo.InvariantCulture, $"Number|{number}"));

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out TextOrNumberSerializable? item) {
        if (value is not null && value.StartsWith("Text|", StringComparison.Ordinal)) {
            item = value[5..];
            return null;
        }
        if (value is not null && value.StartsWith("Number|", StringComparison.Ordinal) && int.TryParse(value.AsSpan(7), NumberStyles.Integer, provider, out int number)) {
            item = number;
            return null;
        }
        item = null;
        return new ValidationError("Expected 'Text|<text>' or 'Number|<digits>'.");
    }
}
```

`System.Text.Json.JsonSerializer.Serialize(union)` writes `"Text|hello"`, and `Deserialize<TextOrNumberSerializable>` returns an equal union. Invalid text surfaces as `JsonException` with the `ValidationError` message, `Parse` throws `FormatException` with the same message, and `TryParse` returns `false`. A JSON `null` deserializes to `null` without a `Validate` call. `SerializationFrameworks.All` covers System.Text.Json, Newtonsoft.Json, and MessagePack with one pair once each integration package is referenced.

Entity Framework Core stores the ad hoc union in one column through a value converter, registered globally by `UseThinktectureValueConverters` on the options builder. ASP.NET Core binds the union from a route or query value through the generated `IParsable<TUnion>`. MVC controllers get the best behavior from `ThinktectureModelBinderProvider` in `Thinktecture.Runtime.Extensions.AspNetCore`, registered by `ModelBinderProviders.Insert` at index zero. Entity Framework Core keeps a null column null, and model binding answers a missing or empty value, so `Validate` never receives a null.

A regular union is a polymorphic hierarchy. System.Text.Json needs one `JsonDerivedType` on the base per case. Newtonsoft.Json needs `TypeNameHandling`, which is a deserialization risk unless the binder restricts the types. MessagePack has its own `Union` attribute, and this library does not integrate with it. `[ObjectFactory<string>]` on the base gives every framework one wire format at the cost of the property structure. Entity Framework Core maps the hierarchy with table-per-hierarchy through `HasDiscriminator<string>` and one `HasValue<TCase>` per case, or with table-per-type. Two cases that declare the same property map to one column through `HasColumnName`.

Serilog destructures an ad hoc union to its current `Value` once `Destructure.UsingThinktectureRuntimeExtensions()` is registered. It recurses with destructuring enabled, so a generated inner value is unwrapped again and a plain object is destructured by Serilog. A regular union is declined and falls through to default object destructuring with a `$type` property. `TypesToRenderAsString.AdHocUnions` renders the union through `ToString` instead. `SkipToString = true` on the union leaves no override, so string rendering logs the type name. The `@` operator is required. An uninitialized struct union logs as the text `Capturing the property value threw an exception: InvalidOperationException`, because Serilog catches the throw from `Value` during capture.

A `TextNumberOrFlag` holding `42` logs as `42`, and a `Shipped` state logs as `{"ShippedAt": ..., "TrackingNumber": "TRK", "$type": "Shipped"}`.

## [12]-[DESIGN_RULES]

| [INDEX] | [WRONG_FORM]                                                        | [CORRECT_FORM]                                                        |
| :-----: | :------------------------------------------------------------------ | :-------------------------------------------------------------------- |
|  [01]   | A tuple or flag class with nullable fields for exclusive outcomes   | a union with one case per outcome                                     |
|  [02]   | A native `switch` with `_ =>` over a union value hides a new case   | the generated `Switch` or `Map`                                       |
|  [03]   | A lambda without `static` in a `Switch` arm                         | the state overload with a `static` lambda                             |
|  [04]   | `default(TUnion)` or `new TUnion()` on a struct union               | a member value, or `MapToFirstMember` with a stateless first member   |
|  [05]   | A `string` failure case beside a `string` success value             | a distinct failure type per case                                      |
|  [06]   | A stateless marker as a class                                       | a `readonly record struct`                                            |
|  [07]   | A hand-written operator for `TypeParamRef1` that bypasses `CreateT` | an operator that returns `CreateT(value)`                             |
|  [08]   | A wrapping user-defined conversion as `SingleBackingFieldType`      | an interface or abstract base the members implement                   |
|  [09]   | A `StopAt` overload as the default consumer                         | the exhaustive `Switch`, and `StopAt` only to delegate a nested union |
|  [10]   | `SwitchPartially` where every case matters                          | the exhaustive `Switch`                                               |
|  [11]   | A raw `string` serializer for an ad hoc union in application code   | `[ObjectFactory<string>]` with `ToValue` and `Validate`               |
|  [12]   | A regular union serialized without a discriminator                  | `JsonDerivedType` per case, or `[ObjectFactory<string>]` on the base  |
|  [13]   | A union whose cases are a success and a failure with a reason       | `Fin<A>` or `Validation<Error, A>` with typed `Expected` records      |
|  [14]   | A union whose cases are a value and its absence                     | `Option<A>`                                                           |
