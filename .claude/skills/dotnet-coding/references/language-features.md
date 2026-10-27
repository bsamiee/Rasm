# [LANGUAGE_FEATURES]

Covers C# language and runtime features that reshape declarations under the repository's language version and target, each as its selecting condition and resulting form. `System.Numerics`, `System.Globalization`, and `System.Runtime.CompilerServices` sit outside global usings, and a file imports each one it reads.

## [01]-[GENERIC_MATH]

Generic math interfaces let one member over a constrained `T` replace one overload per numeric type:
- Constraints name the operations a body calls, an operator interface (`IAdditionOperators<T, T, T>`) for one operator
- `INumberBase<T>` covers arithmetic with `Zero`, `One`, `IsZero`, and parsing, `INumber<T>` adds comparison
- `IFloatingPoint<T>` adds rounding, `IBinaryInteger<T>` adds bit operations and integer division
- Constants enter through `T.CreateChecked` when they must fit, `T.CreateSaturating` to clamp, and `T.CreateTruncating` to keep low bits
- Generic parsing calls `T.TryParse(text, style, provider, out T value)` from `INumberBase<T>`, one parser serves every numeric type

```csharp
using System.Numerics;

internal static class Means {
    public static Option<T> Weighted<T>(Seq<(T Value, T Weight)> samples) where T : INumberBase<T> {
        (T sum, T weight) = samples.Fold((Sum: T.Zero, Weight: T.Zero), static (state, x) => (state.Sum + (x.Value * x.Weight), state.Weight + x.Weight));
        return T.IsZero(weight) ? None : sum / weight;
    }
}
```

Projects compile in a checked context (`CheckForOverflowUnderflow`), and `+` on an integer `T` binds the type's checked operator and throws `OverflowException`. Wrapping arithmetic (hash mixing, modular counters) sits inside `unchecked(...)`:

```csharp
using System.Numerics;

internal static class Hashes {
    public static T Mixed<T>(T hash, T value) where T : IBinaryInteger<T> => unchecked((hash * T.CreateChecked(31)) + value);
}
```

## [02]-[EXTENSION_MEMBERS]

Extension blocks add members a receiver type cannot declare:
- Properties state a derived fact with no argument (`items.Total`) in place of a `GetTotal()` method
- Operators sit in a block as `static` members and give a package type `+`, `-`, or `*`
- Blocks with a `where` clause add their members only to receivers meeting it, a constraint the type declaration cannot state

```csharp
using System.Numerics;

internal static class Arrays {
    extension<T>(ImmutableArray<T>) where T : IAdditionOperators<T, T, T> {
        public static ImmutableArray<T> operator +(ImmutableArray<T> left, ImmutableArray<T> right) =>
            [.. left.Zip(right, static (x, y) => x + y)];
    }
    extension<T>(ImmutableArray<T> items) where T : INumberBase<T> {
        public T Total => items.Aggregate(T.Zero, static (sum, x) => sum + x);
    }
}
```

Blocks without a receiver name (`extension<T>(Seq<T>)`) hold static members, factories and constants callers reach through the type name. Under static `Prelude` using, `Seq<int>.Repeated(3)` binds `Seq` to a `Prelude` method (`CS0119`), and calls spell `LanguageExt.Seq<int>` while block declarations keep `Seq<T>`:

```csharp
using System.Numerics;

internal static class Units {
    extension<T>(Seq<T>) where T : INumberBase<T> {
        public static Seq<T> Repeated(int count) => toSeq(Enumerable.Repeat(T.One, count));
    }
    public static int Three => LanguageExt.Seq<int>.Repeated(3).Count;
}
```

## [03]-[FIELD_BACKED_PROPERTIES]

`field` names the compiler's backing field inside an accessor, and a property that normalizes on write declares no field of its own:
- `init => field = <normalized value>` holds a total normalization (trim, case, rounding), a rule that rejects input belongs in `Validated`
- Accessors run for object initializers, constructor assignments, and `with`, every copy holds the normalized value
- Property initializers (`= Name`) write the backing field directly, a positional record parameter passed through one skips normalization
- `dotnet_style_prefer_auto_properties` at error (`IDE0032`) turns a declared backing field with accessor logic into `field`
- Non-nullable `field` properties without `required` or an initializer fail `CS9264`
- Lazy getters (`=> field ??= <derivation>`) cache a derivation of immutable inputs and need no initializer
- `get => field` beside a custom `init` is `get;` (`IDE0360`)
- Locals named `field` inside an accessor fail `CS9273`, and a member named `field` reads there as `this.field` (`CS9258`)

```csharp
internal sealed record Tag {
    public required string Name { get; init => field = value.Trim().ToUpperInvariant(); }
}
```

## [04]-[PARAMS_COLLECTIONS]

`params` takes any collection-expression target, and argument lifetime selects the type:
- `params ReadOnlySpan<T>` serves a callee that reads its arguments within the call
- `params Seq<T>` serves a callee that stores its arguments or reads them inside a returned effect
- Overloads taking `params ReadOnlySpan<T>` win over `params T[]` for every call both accept
- Callers pass separate arguments, or one collection expression with spreads (`[.. items, last]`), and no argument gives an empty collection

```csharp
internal static class Labels {
    public static string Joined(params ReadOnlySpan<string> parts) => string.Join('/', parts);

    public static IO<Unit> Written(Action<string> write, params Seq<string> lines) => IO.lift(() => lines.Iter(write));
}
```

## [05]-[COLLECTION_EXPRESSIONS]

Collection expressions build their target type and have no natural type (`var items = [1]` fails `CS9176`):
- Targets are arrays, spans, `List<T>`, interfaces (`IReadOnlyList<T>`, `IEnumerable<T>`), and `[CollectionBuilder]` types
- `Seq<A>`, `ImmutableArray<T>`, and `FrozenSet<T>` declare a builder, `FrozenDictionary<TKey, TValue>` declares none (`CS0144`)
- Spreads join sequences and single items in one expression in place of `Concat`, `Append`, and `AddRange` chains
- `dotnet_style_prefer_collection_expression` at error turns `new[] { … }`, `new List<T>()`, `Create` calls, and `Empty` members into `[…]`
- `IDE0028` and the `IDE030x` rules report each form, a LanguageExt constructor call (`Seq(1, 2)`) stays

```csharp
internal sealed record Item(string Name);

internal static class Queues {
    public static Seq<Item> Requeued(Seq<Item> pending, Item urgent, Seq<Item> deferred) => [urgent, .. pending, .. deferred];
}
```

## [06]-[PRIMARY_CONSTRUCTORS]

Primary constructor parameters stay in scope for every member, and a parameter a member reads becomes a captured hidden field:
- Types whose parameters are dependencies or fixed inputs every member reads declare a primary constructor
- Constructors that only assign parameters to members fail `IDE0290`
- Primary constructors have no body, a type that validates or derives before storing takes a private constructor behind a factory
- Parameters both read in a member and copied into a member initializer store twice (`CS9124`), each parameter takes one form
- Class parameters are mutable and `readonly struct` parameters reject assignment (`CS9114`), members read parameters and never assign them
- Class parameters stay private, a value consumers read is a record parameter or a property initialized from a parameter no member reads

```csharp
internal sealed class Pricer(Func<string, Option<decimal>> rates, decimal margin) {
    public Option<decimal> Quote(string code, decimal amount) => rates(code).Map(rate => amount * rate * (1m + margin));
}
```

## [07]-[OVERLOAD_PRIORITY]

`[OverloadResolutionPriority]` ranks overloads of one type ahead of argument conversions, and a higher priority wins over a better match:
- Overloads kept for compiled callers take `[OverloadResolutionPriority(-1)]`, source calls bind their replacement
- Caller-info overloads (`[CallerArgumentExpression]`) lose to an overload without the optional parameter until priority ranks them first
- Ranked calls (`Require(count > 0)`) carry their argument text into the error subject

`Fault` and `Reason` are the package error record and reason dotnet-coding declares:

```csharp
using System.Runtime.CompilerServices;

internal static class Checks {
    [OverloadResolutionPriority(-1)]
    public static Fin<Unit> Require(bool flag) => Require(flag, subject: "condition");

    public static Fin<Unit> Require(bool flag, [CallerArgumentExpression(nameof(flag))] string subject = "") =>
        flag ? unit : new Fault(subject, Reason.Invalid);
}
```

## [08]-[INTERPOLATED_STRING_HANDLERS]

Interpolated strings bind to a handler type when their target parameter takes one, and that handler decides whether holes evaluate:
- Holes of culture-sensitive types (numbers, dates) fail `MA0076` in a plain `$"…"`, `string.Create(provider, $"…")` formats them
- Members that build text under a condition take a `ref struct` marked `[InterpolatedStringHandler]`
- `[InterpolatedStringHandlerArgument]` on that parameter passes its condition to a handler constructor
- Handler constructors take `(int literalLength, int formattedCount, <condition>, out bool shouldAppend)`
- `shouldAppend = false` skips every hole and its side effects
- `AppendLiteral` and `AppendFormatted<T>` forward to a `DefaultInterpolatedStringHandler` built with `CultureInfo.InvariantCulture`

```csharp
using System.Globalization;
using System.Runtime.CompilerServices;

[InterpolatedStringHandler]
internal ref struct FailureText {
    private DefaultInterpolatedStringHandler inner;

    public FailureText(int literalLength, int formattedCount, bool flag, out bool shouldAppend) {
        inner = new DefaultInterpolatedStringHandler(literalLength, formattedCount, CultureInfo.InvariantCulture);
        shouldAppend = !flag;
    }

    public void AppendLiteral(string text) => inner.AppendLiteral(text);

    public void AppendFormatted<T>(T value) => inner.AppendFormatted(value);

    public string ToStringAndClear() => inner.ToStringAndClear();
}

internal static class Failures {
    public static Fin<Unit> Unless(bool flag, [InterpolatedStringHandlerArgument(nameof(flag))] ref FailureText text) =>
        flag ? unit : new Fault(text.ToStringAndClear(), Reason.Invalid);
}
```

## [09]-[UNSAFE_ACCESSORS]

`[UnsafeAccessor]` on a `static extern` method binds a non-public member of a referenced assembly at JIT compilation, without reflection:
- Accessors reach host and package members with no public route, a project member takes the accessibility its caller needs
- `Name` spells the member as metadata does (`get_<Name>` and `set_<Name>` for property accessors)
- `Name` is the one member name a string holds, no compiler-visible member exists to derive it from
- `Method` and `Field` take an instance first, `StaticMethod` and `StaticField` a `null` of the declaring type, `Constructor` returns one
- `Field` and `StaticField` return `ref`, and a write goes through the reference
- `[UnsafeAccessorType("<Namespace>.<Type>, <Assembly>")]` on a parameter or return types an inaccessible type as `object`
- Members of a generic type bind from an accessor inside a generic class with matching type parameters, a closed signature misses them
- Missing members throw `MissingMethodException` or `MissingFieldException` at first call, and each call sits inside an `IO.lift` that captures it

`HostItem` is a host type:

```csharp
using System.Runtime.CompilerServices;

internal static class HostInternals {
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Hidden")]
    private static extern int Hidden(HostItem item);

    [UnsafeAccessor(UnsafeAccessorKind.StaticField, Name = "_registry")]
    private static extern ref Dictionary<string, object> Registry([UnsafeAccessorType("Host.Runtime.Registry, Host")] object? owner);

    public static IO<int> HiddenOf(HostItem item) => IO.lift(() => Hidden(item));

    public static IO<bool> Registered(string key) => IO.lift(() => Registry(owner: null).ContainsKey(key));
}
```

## [10]-[INTERCEPTORS]

Interceptors redirect one call site to another method at compile time, and source generators alone emit them:
- Generators read each call site through `GetInterceptableLocation` on its semantic model
- `GetInterceptsLocationAttributeSyntax()` on that location writes the `[InterceptsLocation(version, data)]` attribute
- `InterceptsLocationAttribute` has no BCL declaration, each generator emits it as a `file` class in `System.Runtime.CompilerServices`
- `InterceptorsNamespaces` in the consuming project lists each namespace holding interceptors, an unlisted one fails to compile
- Interceptors are static methods of a non-generic type outside the global namespace
- Interceptors of an instance call take the receiver as a `this` parameter
- Intercepted methods stay complete, an interceptor specializes a call (constant arguments, a known type) and returns the same result
- Application code calls the intercepted method and writes no interceptor

## [11]-[PRECOMPUTED_LOOKUPS]

Frozen collections and `SearchValues<T>` cost more to build than a `HashSet<T>` and less to read, and the reading sites decide where a set lives:
- Literal sets one site tests stay a pattern (`c is '/' or '\\'`, a `switch` over string constants), the compiler lowers it
- Sets read at more than one site or built from data are one `static readonly` field, built once at type initialization
- Collection expressions build a frozen set under the default comparer, a set with a comparer calls `FrozenSet.Create(comparer, …)`

```csharp
internal static class Names {
    private static readonly FrozenSet<string> Reserved = FrozenSet.Create(StringComparer.OrdinalIgnoreCase, "con", "nul", "prn");

    public static bool IsReserved(string name) => Reserved.Contains(name);

    public static bool IsSeparator(char c) => c is '/' or '\\';
}
```

## [12]-[ANTI_PATTERNS]

| [INDEX] | [WRONG_FORM]                                                        | [CORRECT_FORM]                                           |
| :-----: | :------------------------------------------------------------------ | :------------------------------------------------------- |
|  [01]   | Overloads differing only in numeric type                            | One member over `T` with the interfaces its body calls   |
|  [02]   | Cast through `object` or a sum of `T.One` for a constant            | `T.CreateChecked(<literal>)`                             |
|  [03]   | Hash mixing on an integer `T` in a checked project context          | `unchecked(...)` around the wrapping expression          |
|  [04]   | Extension method `GetX()` taking no argument                        | Extension property                                       |
|  [05]   | Static helper class of factories for a package type                 | Static members in an `extension(<Type>)` block           |
|  [06]   | Declared backing field beside a normalizing accessor                | `field` inside the accessor                              |
|  [07]   | `init` accessor that throws on invalid input                        | `Validated` factory returning `Fin`                      |
|  [08]   | Positional record parameter assigned to a normalizing property      | `required` property with a normalizing `init`            |
|  [09]   | `params T[]` on a callee that reads arguments within the call       | `params ReadOnlySpan<T>`                                 |
|  [10]   | `Concat`, `Append`, or `toSeq(new[] { … })` building a sequence     | Collection expression with spreads                       |
|  [11]   | Primary constructor parameter read by members and copied to a field | One form per parameter                                   |
|  [12]   | Caller-info overload shadowed by an exact overload                  | `[OverloadResolutionPriority(-1)]` on the exact overload |
|  [13]   | Message text formatted before the check that discards it            | Handler parameter whose constructor declines holes       |
|  [14]   | Reflection `GetField` or `Invoke` over a host member                | `[UnsafeAccessor]` extern method                         |
|  [15]   | `[UnsafeAccessor]` over a project member                            | Member accessibility its caller needs                    |
|  [16]   | Hand-written `[InterceptsLocation]`                                 | Generator writing it from `GetInterceptableLocation`     |
|  [17]   | Frozen collection or `SearchValues<T>` built inside a method        | `static readonly` field                                  |
|  [18]   | Frozen set for one literal test at one site                         | Pattern (`is <a> or <b>`)                                |
