namespace Rasm.UI.Layout;

// --- [MODELS] --------------------------------------------------------------------------

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record Sizing {
    public Limits Limits { get; init; } = Limits.Open;

    internal virtual float GrowWeight => 0f;
    internal Option<int> ShrinkTier => Switch(@fixed: static _ => None, fit: static _ => Some(1), grow: static _ => Some(0), percent: static _ => Some(1));
    internal bool Stretches => GrowWeight > 0f;
    internal virtual Option<float> Definite => None;

    public static Seq<float> Shared(Seq<Sizing> sizes, Seq<float> content, float space) =>
        Layouts.Distribute(sizes.Zip(content).Map(held => (held.First, Layouts.Based(held.First, held.Second, space), held.First.Floor(0f))), space);

    internal float Floor(float content) => float.Max(Limits.Min, content);

    public sealed record Fixed(float Extent) : Sizing {
        internal override Option<float> Definite => Some(Limits.Clamped(Extent));
    }

    public sealed record Fit() : Sizing {
        public static Fit Content { get; } = new();
    }

    public sealed record Grow(Factor Weight) : Sizing {
        public static Grow Unit { get; } = new(Factor.Unit);

        internal override float GrowWeight => (float)Weight;
    }

    public sealed record Percent(Fraction Fraction) : Sizing;
}

public sealed record Tracks(Seq<Sizing> Sizes) {
    public bool AutoFill { get; init; }
}

public sealed record CellSpan(int Columns, int Rows);

[SmartEnum]
public sealed partial class Baseline {
    public static readonly Baseline First = new(0, Fraction.MinValue);
    public static readonly Baseline Last = new(^1, Fraction.MinValue);
    public static readonly Baseline Cap = new(0, Fraction.MaxValue);

    internal Index Line { get; }
    internal Fraction Rise { get; }
}
