# [LANGUAGE_FEATURES]

Covers declarations that replace a family of members with one.

## [01]-[TYPE_PARAMETERS]

Members differing by numeric type, element width, or per-type construction are one generic member, with a type argument stating what each copy spelled. Constraints name the narrowest interfaces a body calls, and limits, sizes, and constants derive from the type argument:

```csharp
internal static class Kernels {
    public static Fin<TCount> Taps<TCount>(double radius) where TCount : IBinaryInteger<TCount>, IMinMaxValue<TCount> =>
        ((2d * double.Ceiling(radius)) + 1d) switch {
            var taps => Fault.Unless(radius >= 0d && taps <= double.CreateTruncating(TCount.MaxValue), Reason.Range, radius).Map(_ => TCount.CreateTruncating(taps)),
        };
}
```

- Constants enter through `T.CreateChecked` where they must fit and `T.CreateSaturating` where they clamp, `T.CreateTruncating` keeps an integer's low bits and clamps a floating value
- Integer arithmetic meant to wrap (hash mixing, generator steps) sits inside `unchecked(…)` and keeps its meaning under a build that checks overflow
- `where TState : allows ref struct` accepts a span as state a generic member passes to its `Func<TState, R>` callback
- Type parameters exist only where they change what a caller passes or receives
- Static classes whose every member repeats one type-parameter list move their members onto the generic type
- Parallel scalar fields that scale together are one `Vector2`, `Vector3`, or `Vector4` with its operators and `Lerp`

## [02]-[STATIC_INTERFACE_MEMBERS]

Static abstract members state what a generic member reads before it holds an instance (a default, a factory, a caption), and `static virtual` members state a default each implementer inherits or replaces with a public static member of the same signature. One generic consumer serves every implementer through `TStage.Member`:

```csharp
internal interface IStage<TSelf> where TSelf : IStage<TSelf>, IEquatable<TSelf> {
    public static abstract TSelf Default { get; }
    public static abstract Func<float, float> Pass(TSelf state);
    public static virtual bool Bypassed(TSelf state) => state.Equals(TSelf.Default);
}

internal sealed record Gain(float Factor) : IStage<Gain> {
    public static Gain Default { get; } = new(1f);
    public static Func<float, float> Pass(Gain state) => sample => sample * state.Factor;
}

internal sealed record Clamp(float Low, float High) : IStage<Clamp> {
    public static Clamp Default { get; } = new(0f, 1f);
    public static Func<float, float> Pass(Clamp state) => sample => float.Clamp(sample, state.Low, state.High);
    public static bool Bypassed(Clamp state) => false;
}

internal static class Stages {
    public static Option<Func<float, float>> Planned<TStage>(TStage state) where TStage : IStage<TStage>, IEquatable<TStage> =>
        TStage.Bypassed(state) ? None : Some(TStage.Pass(state));
}
```

- Self-constrained interfaces (`TSelf : IStage<TSelf>`) take and return the implementing type with no cast at a caller
- Defaults dispatch through a type parameter alone, `Gain.Bypassed` names no member (`CS0117`)
- `TStage.Pass` converts to a `Func<TStage, Func<float, float>>` method group, one generic factory turns each implementer into functions a registry holds
- Behavior one implementer adds is a `static virtual` member of the constraint in place of a type test on a type parameter (`state is Clamp`, `typeof(TStage) == typeof(Clamp)`)
- Host subclasses derive from one base `Node<TSelf> where TSelf : Node<TSelf>, INode<TSelf>` whose constructor calls a static abstract `TSelf.Define`, each input it registers returns a typed reader (`Func<TSource, Fin<T>>`), and the solve function closes over those readers in place of an index or name
- Per-case behavior over existing instances is a `virtual` base member returning the default (`None`), overridden only by the cases that act

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

`field` names a property's backing field inside its accessors, and normalization on write and derived values declare no property of their own. `init` accessors run for object initializers and for each property `with` assigns, so a companion value set beside `field` stays current in every copy:

```csharp
internal sealed record Frame {
    private readonly Option<Matrix3x2> inverse;

    public required string Caption { get; init => field = value.Trim(); }

    public required Matrix3x2 Transform {
        get;
        init {
            field = value;
            inverse = Matrix3x2.Invert(value, out Matrix3x2 inverted) ? Some(inverted) : None;
        }
    }

    public Option<Vector2> Local(Vector2 world) => inverse.Map(matrix => Vector2.Transform(world, matrix));
}
```

- Normalizations that cannot fail (trim, case, rounding) sit in `init`, and a rule that rejects input belongs in its type's factory
- Cached getters (`field ??= …`) sit in a class, a record compares its cache and `with` copies a stale one
- Property initializers, and positional record parameters passed through one (`= Name`), write the backing field directly and skip the accessor

## [05]-[COLLECTION_EXPRESSIONS]

Collection expressions build spans, arrays, and types with a collection builder (`Seq<A>`, `ImmutableArray<T>`, `FrozenSet<T>`, `FrozenDictionary<TKey, TValue>`). Spreads join optional and conditional parts in one expression in place of `Concat`, `Add`, and builder branches. `params` takes the same targets, `params ReadOnlySpan<T>` for arguments a call reads before returning, `params Seq<T>` for arguments a returned closure or effect holds:

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
- Dictionary items are `KeyValuePair.Create(key, value)`, and a frozen table under a comparer calls `FrozenSet.Create(comparer, …)` or `FrozenDictionary.Create(comparer, …)`

## [06]-[UNSAFE_ACCESSORS]

Typed `[UnsafeAccessor]` externs replace reflection by string for a non-public host member that no public member, setting, or command states, one class per concern. `Viewport` is a host type with private `_scale` field and `Invalidate(bool)` method, `Host.Views` an internal type with static `Find(int)`:

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
- Static member externs take their owner as an ignored first argument
- `[UnsafeAccessorType]` names an owner, parameter, or return type no reference reaches, typed `object?` at the call

## [07]-[CALLER_INFORMATION]

Labels derive from call text, `[CallerArgumentExpression]` capturing an argument's source and `[CallerMemberName]` the calling member, and no caller writes a label. One getter and setter pair over a host object written in place derives the edit, the test that skips it when the host already holds the value, and its label:

```csharp
internal sealed record Change<T>(Func<T, IO<T>> Apply, Func<T, bool> Unchanged, string Member) where T : class {
    public static Change<T> Set<TValue>(Func<T, TValue> read, Action<T, TValue> write, TValue value, [CallerArgumentExpression(nameof(read))] string member = "") =>
        new(target => IO.lift(() => write(target, value)).Map(_ => target), target => EqualityComparer<TValue>.Default.Equals(read(target), value), member);
}
```

- `[CallerMemberName]` on a record's constructor parameter names each static item after its field (`static readonly Role Window = new();`)
- Delegate parameters capture the whole lambda text, `[CallerArgumentExpression(nameof(add))]` on `Action<EventHandler<TArgs>> add` labels a host event `h => view.Changed += h`
- Delegates wrapping one host call pass as a method group (`store.Delete`), and the captured text names that member
