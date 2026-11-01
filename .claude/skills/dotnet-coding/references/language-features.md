# [LANGUAGE_FEATURES]

Covers declarations that replace a family of members with one.

## [01]-[TYPE_PARAMETERS]

Members differing by numeric type, element width, or per-type construction are one generic member, with a type argument stating what each copy spelled. Constraints name the narrowest interfaces a body calls, and limits, sizes, and constants derive from the type argument:

```csharp
internal static class Kernels {
    public static Fin<TCount> Taps<TCount>(double radius) where TCount : IBinaryInteger<TCount>, IMinMaxValue<TCount> =>
        ((2d * double.Ceiling(radius)) + 1d) switch {
            var taps => Fault.Unless(radius >= 0d && taps <= double.CreateTruncating(TCount.MaxValue), Reason.Invalid, radius).Map(_ => TCount.CreateTruncating(taps)),
        };
}
```

- Constants enter through `T.CreateChecked` where they must fit and `T.CreateSaturating` where they clamp, `T.CreateTruncating` keeps an integer's low bits and clamps a floating value
- Integer arithmetic meant to wrap (hash mixing, generator steps) sits inside `unchecked(…)` and keeps its meaning under a build that checks overflow
- `where TState : allows ref struct` accepts a span as state a generic member passes to its `Func<TState, R>` callback

## [02]-[STATIC_INTERFACE_MEMBERS]

Static abstract members move a per-case constructor onto its case type, and one generic operation serves every case implementing the interface:

```csharp
internal interface IStreamed<TSelf, TStep> where TSelf : Gesture, IStreamed<TSelf, TStep> {
    public static abstract TSelf Of(Vector2 at, TStep step);
    public TSelf Folded(TStep step);
}

[Union]
internal abstract partial record Gesture {
    internal sealed record Scroll(Vector2 At, Vector2 Travel) : Gesture, IStreamed<Scroll, Vector2> {
        static Scroll IStreamed<Scroll, Vector2>.Of(Vector2 at, Vector2 step) => new(at, step);
        Scroll IStreamed<Scroll, Vector2>.Folded(Vector2 step) => this with { Travel = Travel + step };
    }

    internal sealed record Zoom(Vector2 About, float Scale) : Gesture, IStreamed<Zoom, float> {
        static Zoom IStreamed<Zoom, float>.Of(Vector2 at, float step) => new(at, step);
        Zoom IStreamed<Zoom, float>.Folded(float step) => this with { Scale = Scale * step };
    }
}

internal static class Streams {
    public static TGesture Accumulated<TGesture, TStep>(Option<TGesture> live, Vector2 at, TStep step) where TGesture : Gesture, IStreamed<TGesture, TStep> =>
        live.Match(Some: held => held.Folded(step), None: () => TGesture.Of(at, step));
}
```

- Self-constrained interfaces (`TSelf : IStreamed<TSelf, TStep>`) return the case type with no cast at a caller
- Explicit implementations keep `Of` and `Folded` out of each case's public members

`static virtual` members state a default every implementer inherits, and an implementer overrides one with a public static member of the same signature. Generic consumers call `TStage.Member`:

```csharp
internal interface IStage<TSelf> where TSelf : IStage<TSelf> {
    public static abstract TSelf Default { get; }
    public static abstract Option<Func<float, float>> Pass(TSelf state);
    public static virtual Seq<string> Inputs(TSelf state) => [];
}

internal sealed record Gain(float Factor) : IStage<Gain> {
    public static Gain Default { get; } = new(1f);
    public static Option<Func<float, float>> Pass(Gain state) => state.Factor is 1f ? None : Some<Func<float, float>>(sample => sample * state.Factor);
}

internal sealed record Masked(string Channel) : IStage<Masked> {
    public static Masked Default { get; } = new("alpha");
    public static Option<Func<float, float>> Pass(Masked state) => Some<Func<float, float>>(static sample => float.Clamp(sample, 0f, 1f));
    public static Seq<string> Inputs(Masked state) => [state.Channel];
}

internal static class Stages {
    public static (Option<Func<float, float>> Pass, Seq<string> Inputs) Planned<TStage>(Option<TStage> state) where TStage : IStage<TStage> =>
        state.IfNone(TStage.Default) switch { var held => (TStage.Pass(held), TStage.Inputs(held)) };
}
```

- Defaults dispatch through a type parameter alone, `Gain.Inputs` names no member (`CS0117`)
- `TStage.Inputs` converts to a `Func<TStage, Seq<string>>` method group, one generic factory turns each implementer into functions a registry holds
- Behavior one implementer adds is a `static virtual` member of the constraint in place of a type test on a type parameter (`state is Masked`, `typeof(TStage) == typeof(Masked)`)

## [03]-[EXTENSION_MEMBERS]

Facts derived from a type the code does not own (BCL, package, host) are extension members at the layer that owns the derivation, in place of a static `GetX(x)` helper or a wrapper type. Extension properties state derived values, extension operators give arithmetic, and a `where` clause limits members to receivers meeting it:

```csharp
internal static class Arrays {
    extension<T>(ReadOnlySpan<T> items) where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T> {
        public T Total => TensorPrimitives.Sum(items);
    }

    extension<T>(ImmutableArray<T>) where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T> {
        public static ImmutableArray<T> operator +(ImmutableArray<T> left, ImmutableArray<T> right) {
            T[] sums = GC.AllocateUninitializedArray<T>(left.Length);
            TensorPrimitives.Add(left.AsSpan(), right.AsSpan(), sums);
            return ImmutableCollectionsMarshal.AsImmutableArray(sums);
        }
    }
}
```

- Span receivers take arrays and spans through the implicit span conversion, one `ReadOnlySpan<T>` block in place of one per buffer type
- Blocks without a receiver name hold static members (operators, factories) callers reach through the type name
- Blocks on one receiver with and without a constraint overload a member, the constrained form drops an argument its constraint supplies
- Receivers constrained on generator-emitted interfaces (`ISmartEnum<TKey, T, TValidationError>`, `IObjectFactory<T, TValue, TValidationError>`) give one member to every generated type
- Extension members satisfy no interface constraint, a generic algorithm calling a member takes an interface the type implements

## [04]-[FIELD_BACKED_PROPERTIES]

`field` names a property's backing field inside its accessors, and normalization on write and cached derivations declare no field. `init` accessors run for object initializers, constructor assignments, and `with`, every copy holds the normalized value:

```csharp
internal sealed class Profile {
    public required string Name { get; init => field = value.Trim(); }
    public required ImmutableArray<double> Stops { get; init; }
    public required ImmutableArray<double> Levels { get; init; }
    public IInterpolation Fitted => field ??= CubicSpline.InterpolatePchipSorted([.. Stops], [.. Levels]);
}
```

- Normalizations that cannot fail (trim, case, rounding) sit in `init`, and a rule that rejects input belongs in its type's `Validated` factory
- Cached getters (`field ??= …`) sit in a class, a record compares its cache and `with` copies a stale one
- Value objects mark a cached member `[IgnoreMember]`
- Positional record parameters passed through a property initializer (`= Name`) skip its accessor's normalization

## [05]-[COLLECTION_EXPRESSIONS]

Collection expressions build spans, arrays, and types with a collection builder (`Seq<A>`, `ImmutableArray<T>`, `FrozenSet<T>`). Spreads join optional and conditional parts in one expression in place of `Concat`, `Add`, and builder branches. `params` takes the same targets, `params ReadOnlySpan<T>` for arguments a call reads before returning, `params Seq<T>` for arguments a returned closure or effect holds:

```csharp
internal sealed record Face(string Caption, Option<string> Glyph);

[Union]
internal abstract partial record Node {
    internal sealed record Text(string Value) : Node;
    internal sealed record Art(string Source) : Node;
    internal sealed record Choice(Seq<Seq<Node>> Options) : Node;
}

internal static class Faces {
    public static Seq<Node> Labelled(Face face) => [.. face.Glyph.Map<Node>(static glyph => new Node.Art(glyph)).ToSeq(), new Node.Text(face.Caption)];

    public static Seq<Node> Captioned(Face face) => [new Node.Text(face.Caption)];

    public static Func<Face, Seq<Node>> Fitting(params Seq<Func<Face, Seq<Node>>> preferred) => face => [new Node.Choice(preferred.Map(strategy => strategy(face)))];

    public static Func<Face, Seq<Node>> Toolbar => Fitting(Labelled, Captioned);
}
```

- Optional items spread as `.. option.ToSeq()`, a conditional item as `.. (flag ? Some(item) : None).ToSeq()`
- Strategies pass as method groups in preference order, and `params` collects them with no lambda list
- `FrozenDictionary<TKey, TValue>` declares no builder (`CS0144`), and a frozen set under a comparer calls `FrozenSet.Create(comparer, …)`

## [06]-[UNSAFE_ACCESSORS]

Typed `[UnsafeAccessor]` externs replace reflection by string for non-public host members, one class per concern. Each class cites proof that no public member, setting, or command states the same fact. `Viewport` is a host type with private `_scale` field and `Invalidate(bool)` method, `Host.Views` an internal type with static `Find(int)`:

```csharp
internal static class Viewports {
    [UnsafeAccessor(UnsafeAccessorKind.StaticMethod)]
    private static extern Viewport? Find([UnsafeAccessorType("Host.Views, Host")] object? owner, int serial);

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_scale")]
    private static extern ref double Scale(Viewport view);

    [UnsafeAccessor(UnsafeAccessorKind.Method)]
    private static extern void Invalidate(Viewport view, bool deep);

    public static IO<Option<Viewport>> Open(int serial) => IO.lift(() => Optional(Find(owner: null, serial)));

    public static IO<Unit> Zoomed(Viewport view, double factor) =>
        IO.lift(() => {
            Scale(view) *= factor;
            Invalidate(view, deep: false);
        });
}
```

- Externs named after their member omit `Name`, a member name the naming rules refuse an extern (`_scale`) sets it
- `ref` returns write a field in place
- Static member externs take their owner as an ignored first argument
- `[UnsafeAccessorType]` names an owner, parameter, or return type no reference reaches, typed `object?` at the call
- Renamed or removed members throw `MissingFieldException` or `MissingMethodException` at first call

## [07]-[CALLER_INFORMATION]

Labels derive from call text, `[CallerArgumentExpression]` naming an argument and `[CallerMemberName]` its calling member. One private helper's error states which search failed on which value, with no text written at a caller:

```csharp
internal static class Searches {
    public static Fin<double> Crossing(double level) => Root(level, x => (x * x) - level, 0d, 1d);

    private static Fin<double> Root<T>(
        T subject, Func<double, double> margin, double lower, double upper,
        [CallerArgumentExpression(nameof(subject))] string named = "", [CallerMemberName] string search = "") =>
        Bisection.TryFindRoot(margin, lower, upper, 1e-9, 100, out double root)
            ? root
            : new Fault(string.Create(CultureInfo.InvariantCulture, $"{search} at {named} = {subject} over [{lower}, {upper}]"), Reason.Invalid);
}
```

- Delegate parameters capture the whole lambda text, `[CallerArgumentExpression(nameof(add))]` on `Action<EventHandler<TArgs>> add` labels a host event `h => view.Changed += h`
- Delegates wrapping one host call pass as a method group (`store.Delete`), and the captured text names that member
- Overloads without a caller information parameter win over one with it, `[OverloadResolutionPriority(-1)]` on them binds source calls to the caller information form
