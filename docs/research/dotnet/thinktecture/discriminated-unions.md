# [DISCRIMINATED_UNIONS]

Discriminated unions hold one value belonging to exactly one declared case. Unions name data alternatives. The source generator emits `Switch` and `Map` with one arm per case, and a missing arm is a compile error.

<!-- Decision held by dotnet-coding [04.3] unions
## [01]-[MOTIVATION]

Tuples like `(Order? Order, bool IsSoftDeleted, string? Error)` allow any combination of their fields, and every consumer interprets those fields by convention. `Result<T>` classes with `IsSuccess`, `Value`, and `Error` let a consumer read `Value` after a failure, and a new status adds no compile error. Exceptions for an expected outcome hide that outcome from the method signature, and a missing `catch` compiles.

Unions move the alternatives into the type. Candidates for a union are boolean or enum fields that decide how other properties are read, chains of type tests, tuples with mutually exclusive nullable fields, and base classes where derived classes differ only in state data.
-->

<!-- Integrated into .claude/skills/dotnet-thinktecture/SKILL.md
## [02]-[AD_HOC_VERSUS_REGULAR]

Ad hoc unions combine existing types that share no base (`string`, `int`, `List<ValidationError>`). Regular unions are class hierarchies where cases derive from one abstract partial base and carry their properties and behavior. Smart enums are fixed sets of named instances with one shape, and unions are fixed sets of shapes. Smart enum items can return a union.

| [INDEX] | [ASPECT]    | [AD_HOC_UNION]                                                  | [REGULAR_UNION]                                              |
| :-----: | :---------- | :-------------------------------------------------------------- | :----------------------------------------------------------- |
|  [01]   | Declaration | `partial class`, `partial struct`, or `ref partial struct`      | `partial class` or `partial record`, generated as `abstract` |
|  [02]   | Attribute   | `[Union<T1, T2>]` through `[Union<..., T5>]`, or `[AdHocUnion]` | `[Union]` on the base                                        |
|  [03]   | Cases       | The type arguments                                              | Nested types that derive from the base                       |
|  [04]   | Generic     | `TypeParamRef1` to `TypeParamRef5` name the type parameters     | The base can be generic, a case cannot (`TTRESG053`)         |
-->

<!-- Integrated into .claude/skills/dotnet-thinktecture/SKILL.md
## [03]-[AD_HOC_UNIONS]

`AdHocUnion` with `typeof` exists for member types a generic attribute cannot spell (`List<string?>`). Both attributes share the five-member bound and generate the same members. The generated names derive from the member types: `IsString`, `AsString`, `IsInt32`, `AsInt32`. `Value` returns the current member as `object`.

`AsString` on a union that holds an `int` throws `InvalidOperationException`. The text is `'TextNumberOrFlag' is not of type 'string' but of type 'int'.` The explicit cast takes the same path. Equality compares the discriminator and then the member value, and two `string` members compare with `StringComparison.OrdinalIgnoreCase` unless `DefaultStringComparison` says otherwise. `ToString` and `GetHashCode` delegate to the member, and the generated `ToString` returns `string?`.

C# forbids user-defined conversions from `object` and from interfaces. Members of one of those types receive a constructor and no operator. Every member type must be at least as accessible as the union, because the generated operators and accessors expose it (`TTRESG077`). Unions need at least two member types (`TTRESG067`) and no more than one union attribute (`TTRESG066`).
-->

<!-- Integrated into .claude/skills/dotnet-thinktecture/SKILL.md
## [04]-[AD_HOC_SETTINGS]

| [INDEX] | [PROPERTY]                    | [DEFAULT]           | [EFFECT]                                                                                              |
| :-----: | :---------------------------- | :------------------ | :---------------------------------------------------------------------------------------------------- |
|  [01]   | `T1Name` to `T5Name`          | Type name           | Renames `IsX`, `AsX`, `CreateX`, `NormalizeX`, and the `Switch`/`Map` arm                             |
|  [02]   | `DefaultStringComparison`     | `OrdinalIgnoreCase` | Comparison for `string` members in `Equals` and `GetHashCode`                                         |
|  [03]   | `SkipToString`                | `false`             | Suppresses the `ToString` override                                                                    |
|  [04]   | `SkipEqualityComparison`      | `false`             | Suppresses `Equals`, `GetHashCode`, `==`, `!=`, `IEquatable<T>`, and `IEqualityOperators<T, T, bool>` |
|  [05]   | `ConstructorAccessModifier`   | `Public`            | Accessibility of constructors and factory methods, never of operators                                 |
|  [06]   | `ConversionFromValue`         | `Implicit`          | Operator from a member type to the union, `None` disables                                             |
|  [07]   | `ConversionToValue`           | `Explicit`          | Operator from the union to a member type, `None` disables                                             |
|  [08]   | `T1IsNullableReferenceType`   | `false`             | Types the member as `string?`, allows `null`                                                       |
|  [09]   | `T1IsStateless`               | `false`             | Stores only the discriminator for that member                                                         |
|  [10]   | `ValueMemberAccessModifier`   | `Public`            | Accessibility of `Value`, generated code keeps reading it                                             |
|  [11]   | `ValueMemberName`             | `Value`             | Renames the raw accessor and frees `Value` for a hand-written property                                |
|  [12]   | `UseSingleBackingField`       | `false`             | Boxes every member into one `object?` field                                                           |
|  [13]   | `SingleBackingFieldType`      | None                | Types the single field and `Value`                                                                    |
|  [14]   | `FactoryMethodGeneration`     | `Default`           | `Always` or `None` overrides the trigger rule for `CreateX`                                           |
|  [15]   | `DefaultValueHandling`        | `Disallow`          | `MapToFirstMember` makes `default` of a struct union the first member                                 |
|  [16]   | `SwitchMethods`, `MapMethods` | `Default`           | `DefaultWithPartialOverloads` adds `SwitchPartially` and `MapPartially`, `None` removes all           |
|  [17]   | `SwitchMapStateParameterName` | `state`             | Name of the state parameter in `Switch` and `Map`                                                     |

Unions adding their own properties set `ConversionFromValue = ConversionOperatorsGeneration.None` and `ConstructorAccessModifier = UnionConstructorAccessModifier.Private`. The hand-written constructors are then the only entry points and chain to the generated ones.

```csharp
[Union<string, int>(T1Name = "Text", T2Name = "Number", ConversionFromValue = ConversionOperatorsGeneration.None, ConstructorAccessModifier = UnionConstructorAccessModifier.Private)]
internal sealed partial class LabeledTextOrNumber {
    [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
    public LabeledTextOrNumber(string text, string label) : this(text) => Label = label;
    public required string Label { get; init; }
}
```

The generated private constructors do not assign `Label`. The property is `required`, and the public constructors declare `SetsRequiredMembers`.

The generator declares `static partial void Normalize{MemberName}(ref TMember {memberName})` for every member that carries state. Implementations rewrite the value before the backing field is written. Equality, `ToString`, `Switch`, `Value`, and every serializer read the normalized value.

```csharp
[Union<string, int>(T1Name = "Text", T2Name = "Number")]
internal sealed partial class NamedTextOrNumber {
    static partial void NormalizeText(ref string text) => text = text?.Trim().ToLowerInvariant() ?? "";
}
```

The parameter name is the member name in camel case, and a different name compiles with `CS8826`. The call is the first statement of the generated constructor, and no null check precedes it: a `null` argument reaches the `Normalize` method body. The conversion operators delegate to that constructor. Members sharing a type with another member get `Create{MemberName}` instead of a constructor, and the call moves there. Stateless members lack a `Normalize` method. Unions with an object factory read through `Validate`, which constructs the union. Every framework read runs the `Normalize` method.
-->

<!-- Integrated into .claude/skills/dotnet-thinktecture/SKILL.md
## [05]-[BACKING_FIELDS]

Unions with at most one reference type keep one typed field per member. Two or more reference types share one `object?` field, and value types keep their fields without boxing. `UseSingleBackingField = true` moves the value types into the shared field and boxes them. `SingleBackingFieldType` names a common base or interface for that field, and `Value` takes that type.

`Value` typed as the shared interface reaches the members every case declares, without a `Switch`. `SingleBackingFieldType` implies `UseSingleBackingField = true`, and an explicit `false` beside it is `TTRESG075`. Every member type needs a built-in implicit conversion to the field type, because the stored value must stay the original instance. User-defined ones are `TTRESG079`. `typeof(object)` equals `UseSingleBackingField = true`.
-->

<!-- Integrated into .claude/skills/dotnet-thinktecture/SKILL.md
## [06]-[STATELESS_MEMBERS]

Stateless members are types where presence is the whole information. The union stores only the discriminator, `AsX` and `Value` return `default(T)`, equality compares the discriminator alone, and `CreateX` is parameterless. Prefer a `readonly record struct`, because `default` of a class is `null`, and a stateless reference type sets `TxIsNullableReferenceType = true` on its own.

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
- The generator then drops `IDisallowDefaultValue` from the base list, and `TTRESG047` stops
- Hand-written `IDisallowDefaultValue` restores the analyzer check while the runtime value stays valid
- The analyzer also reports `default` inside the union itself, a hand-written property (`MaybeInt.None`) constructs the stateless member instead

```csharp
internal readonly record struct Absent;

[Union<Absent, int>(T1IsStateless = true, DefaultValueHandling = UnionDefaultValueHandling.MapToFirstMember)]
internal readonly partial struct MaybeInt : IDisallowDefaultValue {
    public static MaybeInt None => new Absent();
}
```

An uninitialized `MaybeInt`, for example an array element, has `IsAbsent == true`, equals `MaybeInt.None`, shares its hash code, and `Value` returns `default(Absent)`.
-->

<!-- Integrated into .claude/skills/dotnet-thinktecture/SKILL.md
## [07]-[GENERIC_AD_HOC_UNIONS]

`TypeParamRef1` to `TypeParamRef5` name the union's own type parameters, also inside constructed types (`List<TypeParamRef1>`).

- Type parameter members get no conversion operator, because the operator breaks for specific type arguments
- `Result<string>` makes every conversion ambiguous (`CS0457`), and an interface argument makes the conversion from an interface-typed variable fail (`CS0029`)
- `object` arguments make `(object)union` return the boxed union
- `object` arguments also send `Result<object> r = "text";` into the `string` member, because overload resolution picks the most specific parameter type
- The generator emits `CreateX` factory methods for every member as soon as one member is a type parameter, an interface, `object`, or a duplicate type
- Authors who control every instantiation add the operator and delegate to the factory, and normalization still runs

```csharp
[Union<TypeParamRef1, string>]
internal readonly partial struct Result<T> {
    public static implicit operator Result<T>(T value) => CreateT(value);
}
```

`Result<int>.CreateT(42)`, `new Result<int>(42)`, and the generated `Result<int>.CreateString("text")` all exist, and the `string` member keeps its implicit operator. `TypeParamRef` above the type parameter count is `TTRESG071`, and on a non-generic union it is `TTRESG072`. `allows ref struct` type parameters are `TTRESG073`. Generic unions using no type parameter are `TTRESG107`.
-->

<!-- Integrated into .claude/skills/dotnet-thinktecture/SKILL.md
## [08]-[REGULAR_UNIONS]

The generator gives the base a private constructor. Types declared outside the base cannot derive from it. Class cases are `sealed` or keep constructors private (`TTRESG054`), and record cases are `sealed` (`TTRESG055`). Non-abstract cases are no less accessible than the base (`TTRESG056`). Nested types not derived from the base are `TTRESG106`. Generated types never receive a primary constructor (`TTRESG043`). Positional record cases are the natural form, and the base declares no primary constructor.

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

Abstract methods suit behavior that belongs to the state and needs no dependency. Transitions reading context live outside the union and pass that context through the `Switch` overload that takes a state parameter. The arms return different case types, and the call names `TResult`.

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

The generator emits an implicit conversion to the base for every case with a single-parameter constructor with a parameter type unique among those cases. `OrderState` converts from `string` into `Placed` and from `DateTime` into `Processing`: `return "me";` in a method that returns `OrderState` yields a `Placed`. `ConversionFromValue = ConversionOperatorsGeneration.None` on `[Union]` removes these operators.

Class cases with `[Union]` become abstract nested unions with a generated private constructor, and their cases nest inside them. Records cannot nest unions, because every record case is `sealed`.

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
-->

<!-- Integrated into .claude/skills/dotnet-thinktecture/SKILL.md
## [09]-[SWITCH_AND_MAP]

`Switch` has a void overload with one `Action` per case and a value overload with one `Func<TCase, TResult>` per case. State overloads of each pass a `TState` first. `Map` takes one `TResult` value per case. Every argument is named (`TTRESG046`), and a lambda without `static` is `TTRESG1001`. Captured context travels through the state parameter, and the lambdas are `static`. When arms return different types, `TResult` inference fails on the whole call, and an explicit `Switch<TResult>` moves the error to the one arm that differs.

`SwitchMethods`, `MapMethods`, `SwitchMapStateParameterName`, and `ConversionFromValue` are declared on `[Union]` as well as on the ad hoc attributes. On an ad hoc union the `@default` arm receives the current member as `object?`, and on a regular union it receives the base type. The value overload requires `@default`, and the void overload declares it optional. Void `SwitchPartially` without `@default` does nothing for an unhandled case. `MapPartially` requires `@default` and takes the other arms as optional values.

```csharp
[Union<string, int, bool>(SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads, MapMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
internal sealed partial class TextNumberOrFlag;

internal static class PartialMatching {
    public static string Label(TextNumberOrFlag union) => union.MapPartially(@default: "other", @string: "text");
    public static int Length(TextNumberOrFlag union) => union.SwitchPartially(@default: static _ => 0, @string: static text => text.Length);
}
```

Every generated `Switch` and `Map` ends in an unreachable branch. On an ad hoc union `Switch` throws `IndexOutOfRangeException` and `Map` throws `InvalidOperationException`. Both carry the text `Unexpected value index '...'.` Regular unions throw `InvalidOperationException` with the text `Unexpected type '...'.` from every arm.

<!-- Integrated into .claude/skills/dotnet-thinktecture/SKILL.md
-->

<!-- Integrated into .claude/skills/dotnet-thinktecture/SKILL.md
## [10]-[USE_CASES]

Dates known to the year, month, or day are three cases with a shared `Year`. The base holds the shared property and a private constructor, and the record cases pass `Year` up. The hand-written operator takes `DateOnly`.

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

`YearOnly(int)` is the only single-parameter case. The generator also emits an implicit conversion from `int`. `System.Text.Json.JsonSerializer.Serialize<PartiallyKnownDate>(date)` writes `{"$type":"Date","Month":3,"Day":15,"Year":2024}`, and `Deserialize<PartiallyKnownDate>` returns an `Exact`.

Cases can be value objects or smart enums. The union names the kind, and each case owns its value and rules. `Unknown` is a declared case with one instance, not `null`.

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

`Unknown` has a private constructor: `Instance` is its only value.
-->

<!-- Integrated into .claude/skills/dotnet-thinktecture/SKILL.md
## [11]-[FRAMEWORK_INTEGRATION]

Ad hoc unions carry no type discriminator. No serializer handles them on their own. `[ObjectFactory<string>]` declares one wire format: `ToValue` renders the union, `Validate` parses it back and returns a `ValidationError` for bad input. With `UseForSerialization` set and `Thinktecture.Runtime.Extensions.Json` referenced, the generator applies a `JsonConverter` attribute with `ThinktectureJsonConverterFactory<TUnion, ValidationError>` to the type. No options registration is needed. `UseForModelBinding = true` and `UseWithEntityFramework = true` reuse the same pair. `string` factories also implement `IParsable<TUnion>`.

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

`System.Text.Json.JsonSerializer.Serialize(union)` writes `"Text|hello"`, and `Deserialize<TextOrNumberSerializable>` returns an equal union. Invalid text surfaces as `JsonException` with the `ValidationError` message, `Parse` throws `FormatException` with the same message, and `TryParse` returns `false`. JSON `null` deserializes to `null` without a `Validate` call. `SerializationFrameworks.All` covers System.Text.Json, Newtonsoft.Json, and MessagePack with one pair once each integration package is referenced.

Entity Framework Core stores the ad hoc union in one column through a value converter, and ASP.NET Core binds it from a route or query value through the generated `IParsable<TUnion>`. MVC controllers use `ThinktectureModelBinderProvider`. Entity Framework Core keeps a null column null, and model binding answers a missing or empty value, `Validate` never receives a null.

Regular unions are polymorphic hierarchies. System.Text.Json needs one `JsonDerivedType` on the base per case. Newtonsoft.Json needs `TypeNameHandling`, which is a deserialization risk unless the binder restricts the types. MessagePack has its own `Union` attribute, and no integration exists. `[ObjectFactory<string>]` on the base gives every framework one wire format at the cost of the property structure. Entity Framework Core maps the hierarchy with table-per-hierarchy through `HasDiscriminator<string>` and one `HasValue<TCase>` per case, or with table-per-type. Two cases that declare the same property map to one column through `HasColumnName`.

Serilog destructures an ad hoc union to its current `Value` and declines a regular union, which falls through to default object destructuring with a `$type` property. `SkipToString = true` on the union leaves no override, and string rendering logs the type name. Uninitialized struct unions log a capture-exception placeholder instead of failing the log call.
-->

<!-- Integrated into .claude/skills/dotnet-thinktecture/SKILL.md
## [12]-[DESIGN_RULES]

| [INDEX] | [WRONG_FORM]                                                        | [CORRECT_FORM]                                                        |
| :-----: | :------------------------------------------------------------------ | :-------------------------------------------------------------------- |
|  [01]   | Tuple or flag class with nullable fields for exclusive outcomes     | Union with one case per outcome                                       |
|  [02]   | Native `switch` with `_ =>` over a union value hides a new case     | The generated `Switch` or `Map`                                       |
|  [03]   | Lambda without `static` in a `Switch` arm                           | The state overload with a `static` lambda                             |
|  [04]   | `default(TUnion)` or `new TUnion()` on a struct union               | Member value, or `MapToFirstMember` with a stateless first member     |
|  [05]   | `string` failure case beside a `string` success value               | Distinct failure type per case                                        |
|  [06]   | Stateless marker as a class                                         | `readonly record struct`                                              |
|  [07]   | Hand-written operator for `TypeParamRef1` that bypasses `CreateT`   | Operator that returns `CreateT(value)`                                |
|  [08]   | Wrapping user-defined conversion as `SingleBackingFieldType`        | Interface or abstract base the members implement                      |
|  [09]   | `StopAt` overload as the default consumer                           | The exhaustive `Switch`, and `StopAt` only to delegate a nested union |
|  [10]   | `SwitchPartially` where every case matters                          | The exhaustive `Switch`                                               |
|  [11]   | Raw `string` serializer for an ad hoc union in application code     | `[ObjectFactory<string>]` with `ToValue` and `Validate`               |
|  [12]   | Regular union serialized without a discriminator                    | `JsonDerivedType` per case, or `[ObjectFactory<string>]` on the base  |
|  [13]   | Union of a success and a failure with a reason         | `Fin<A>` or `Validation<Error, A>` with typed `Expected` records      |
|  [14]   | Union of a value and its absence                       | `Option<A>`                                                           |
-->
