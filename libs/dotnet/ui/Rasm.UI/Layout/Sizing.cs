namespace Rasm.UI.Layout;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record Sizing {
    private Sizing(float growWeight, Option<int> shrinkTier, Option<float> stated) => (GrowWeight, ShrinkTier, Stated) = (growWeight, shrinkTier, stated);

    public Limits Limits { get; init; } = Limits.Open;

    internal float GrowWeight { get; }
    internal Option<int> ShrinkTier { get; }
    internal bool Stretches => GrowWeight > 0f;
    internal Option<float> Definite => Stated.Map(Limits.Clamped);
    private Option<float> Stated { get; }

    public static Seq<float> Shared(Seq<Sizing> sizes, Seq<float> content, float space) =>
        Layouts.Distribute(sizes.Zip(content).Map(held => (held.First, Layouts.Based(held.First, held.Second, space), held.First.Floor(0f))), space);

    internal float Floor(float content) => float.Max(Limits.Min, content);

    public sealed record Fixed(float Extent) : Sizing(0f, None, Some(Extent));

    public sealed record Fit() : Sizing(0f, Some(1), None) {
        public static Fit Content { get; } = new();
    }

    public sealed record Grow(Factor Weight) : Sizing((float)Weight, Some(0), None) {
        public static Grow Unit { get; } = new(Factor.Unit);
    }

    public sealed record Percent(Fraction Fraction) : Sizing(0f, Some(1), None);
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
