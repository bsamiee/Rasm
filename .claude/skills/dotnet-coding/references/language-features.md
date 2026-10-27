# [LANGUAGE_FEATURES]

Covers declaration forms that turn a family of members into one: type parameters that carry behavior, members on types the code does not own, and arguments and labels a call site states once.

## [01]-[TYPE_PARAMETERS]

Members that differ by numeric type, element width, or per-type construction are one generic member, and its type argument states what each copy spelled. Constraints name the narrowest interfaces a body calls, and limits, sizes, and constants derive from the argument:

```csharp
internal static class Kernels {
    public static Fin<TCount> Taps<TCount>(double radius) where TCount : IBinaryInteger<TCount>, IMinMaxValue<TCount> =>
        ((2d * double.Ceiling(radius)) + 1d) switch {
            var taps => Fault.Unless(radius >= 0d && taps <= double.CreateTruncating(TCount.MaxValue), Reason.Invalid, radius).Map(_ => TCount.CreateTruncating(taps)),
        };
}
```

- Constants enter through `T.CreateChecked` where they must fit, `T.CreateSaturating` to clamp, and `T.CreateTruncating` to keep low bits
- Generic parsing calls `T.TryParse(span, style, provider, out T value)` from `INumberBase<T>`, and a parser per type restates it
- Integer arithmetic meant to wrap (hash mixing, generator steps) sits inside `unchecked(…)`, so a build that checks overflow keeps its meaning

Static abstract members move a per-case constructor onto its case type, so one generic operation serves every case that implements the interface, and each case states its start and its fold once:

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

- Self-constrained interfaces (`TSelf : IStreamed<TSelf, TStep>`) return the case type, and no caller casts
- Explicit implementations keep `Of` and `Folded` out of each case's public members

## [02]-[EXTENSION_MEMBERS]

Facts derived from a type the code does not own (BCL, package, host) are extension members at the layer that owns the derivation, in place of a static `GetX(x)` helper or a wrapper type. Properties state a derived value, operators give the type arithmetic, and a `where` clause gives members to receivers that meet it alone:

```csharp
internal static class Arrays {
    extension<T>(ImmutableArray<T> items) where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T> {
        public T Total => TensorPrimitives.Sum<T>(items.AsSpan());

        public static ImmutableArray<T> operator +(ImmutableArray<T> left, ImmutableArray<T> right) {
            T[] sums = GC.AllocateUninitializedArray<T>(left.Length);
            TensorPrimitives.Add(left.AsSpan(), right.AsSpan(), sums);
            return ImmutableCollectionsMarshal.AsImmutableArray(sums);
        }
    }
}
```

- Blocks on one receiver with and without a constraint overload a member, the constrained form drops an argument its constraint supplies
- Blocks without a receiver name (`extension<T>(Seq<T>)`) hold static factories callers reach through the type name

## [03]-[DERIVED_PROPERTIES]

`field` names a property's backing field inside its accessors, so normalization on write and a cached derivation declare no field. Accessors run for object initializers, constructor assignments, and `with`, and every copy holds the normalized value:

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
- Value objects mark a cached member `[IgnoreMember]`, and equality reads stated members alone
- Positional record parameters passed through a property initializer (`= Name`) skip its accessor's normalization

## [04]-[COLLECTION_ARGUMENTS]

Collection expressions build any target with a builder (`Seq<A>`, `ImmutableArray<T>`, spans), and spreads join optional and conditional parts in one expression in place of `Concat`, `Add`, and builder branches. `params` takes those targets: `params ReadOnlySpan<T>` for arguments a call reads before returning, `params Seq<T>` for arguments a returned closure or effect holds:

```csharp
internal sealed record Face(string Caption, Option<string> Glyph);

[Union]
internal abstract partial record Node {
    internal sealed record Text(string Value) : Node;
    internal sealed record Art(string Source) : Node;
    internal sealed record Choice(Seq<Seq<Node>> Options) : Node;
}

internal static class Faces {
    public static Seq<Node> Labelled(Face face) => [.. face.Glyph.Map(static glyph => (Node)new Node.Art(glyph)).ToSeq(), new Node.Text(face.Caption)];

    public static Seq<Node> Captioned(Face face) => [new Node.Text(face.Caption)];

    public static Func<Face, Seq<Node>> Fitting(params Seq<Func<Face, Seq<Node>>> preferred) => face => [new Node.Choice(preferred.Map(faced => faced(face)))];

    public static Func<Face, Seq<Node>> Toolbar => Fitting(Labelled, Captioned);
}
```

- Optional items spread as `.. option.ToSeq()`, a conditional item as `.. (flag ? Some(item) : None).ToSeq()`
- Strategies pass as method groups in preference order, and `params` collects them with no lambda list
- `FrozenDictionary<TKey, TValue>` declares no builder (`CS0144`), and a frozen set under a comparer calls `FrozenSet.Create(comparer, …)`

## [05]-[CALLER_TEXT]

Labels derive from call text: `[CallerArgumentExpression]` names any argument (a value, a tuple of rejected values, a method group a caller passes as a table), and the set an error lists derives from that same table. Callers pass a format's method group (`Png.Setting`) as `table`:

```csharp
[SmartEnum<string>]
internal sealed partial class Depth {
    public static readonly Depth Byte = new("uint8", 8);
    public static readonly Depth Half = new("float16", 16);

    public int Bits { get; }
}

internal static class Depths {
    public static Seq<Depth> Where<T>(Func<Depth, Option<T>> table) => toSeq(Depth.Items).Filter(depth => table(depth).IsSome);

    public static Fin<T> Sampled<T>(Depth depth, Func<Depth, Option<T>> table, [CallerArgumentExpression(nameof(table))] string format = "") =>
        table(depth).ToFin(new Fault($"{format} {depth} beside {Where(table)}", Reason.Unsupported));
}
```

- `[CallerMemberName]` names the running operation, and a hand `nameof` at a call restates it
- Overloads without a caller-info parameter win over one with it, `[OverloadResolutionPriority(-1)]` on them binds source calls to the caller-info form
