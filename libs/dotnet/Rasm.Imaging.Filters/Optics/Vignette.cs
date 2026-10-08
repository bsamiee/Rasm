using System.Numerics;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Filters.Optics;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record Vignette : IStateRecord<Vignette, VignetteParameter, InvalidOptics>, IPixelStage<Vignette> {
    public static Vignette Default { get; } = Natural.Default;

    public static Option<PixelPass> Pass(Vignette state, PassContext context) =>
        state.Switch(context,
            natural: static (run, _) => run.Camera.Bind(static camera => camera.Frustum.Switch(perspective: static frustum => Some(frustum.Window), parallel: static _ => Option<ViewWindow>.None))
                .Map<PixelPass>(window => Falloff(Vector3.Zero, (column, line) => window.WindowPoint(column + 0.5d, run.Extent.Height - line - 0.5d, run.Extent) switch {
                    var tangent => (1d + ((double)tangent.X * tangent.X) + ((double)tangent.Y * tangent.Y)) switch { var spread => (float)(1d / (spread * spread)) },
                })),
            outline: static (run, outline) => (outline.Shape.Field(run.Extent).Sample, outline.Shape.Fit.Scale(run.Extent)) switch {
                var (sample, scale) => (ShapeEdge.Band(outline.Feather, run.Extent, float.Min(scale.X, scale.Y)) / run.Extent.ShortSide) switch {
                    var band => Some<PixelPass>(Falloff(outline.Pull.SceneLight(run.Working), (column, line) => ShapeEdge.Coverage(sample(column, line).Distance, band))),
                },
            });

    public sealed record Natural : Vignette, IStateRecord<Natural> {
        public static new Natural Default { get; } = new();
    }

    public sealed record Outline(ShapePrimitive Shape, ShortSideLength Feather, Swatch Pull) : Vignette, IStateRecord<Outline, VignetteOutlineParameter, InvalidOptics> {
        public static new Outline Default { get; } = new(ShapePrimitive.Default with { Width = ShortSideExtent.Create(0.8f), Height = ShortSideExtent.Create(0.45f), Fit = ShapeFit.LongSide },
            ShortSideLength.Create(0.2f * float.Sqrt(float.Tau) / 3f), Swatch.Black);
    }

    private static PixelPass.Pointwise Falloff(Vector3 pull, Func<int, int, float> weight) =>
        new((row, column, line) => {
            for (int x = 0; x < row.Length; x++)
                row[x] = new(Vector3.Lerp(pull, row[x].AsVector3(), weight(column + x, line)), row[x].W);
        });
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public abstract partial class VignetteMode : IStateCase<Vignette> {
    public static readonly VignetteMode Natural = new Case<Vignette.Natural>("natural");
    public static readonly VignetteMode Outline = new Case<Vignette.Outline>("outline");

    public abstract TResult Accept<TResult>(IStateCaseVisitor<Vignette, TResult> visitor);

    private sealed class Case<TCase>(string key) : VignetteMode(key) where TCase : Vignette, IStateRecord<TCase> {
        public override TResult Accept<TResult>(IStateCaseVisitor<Vignette, TResult> visitor) => visitor.Case<TCase>();
    }
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VignetteParameter : IStateParameter<Vignette> {
    public static readonly VignetteParameter Falloff = new("falloff", new StateParameter<Vignette>.Variant<Vignette, VignetteMode, InvalidOptics>(Lens.identity<Vignette>()));

    public StateParameter<Vignette> Kind { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VignetteOutlineParameter : IStateParameter<Vignette.Outline> {
    public static readonly VignetteOutlineParameter Shape = new("shape", new StateParameter<Vignette.Outline>.Record<ShapePrimitive>(Lens<Vignette.Outline, ShapePrimitive>.New(static outline => outline.Shape, static shape => outline => outline with { Shape = shape })));
    public static readonly VignetteOutlineParameter Feather = new("feather", new StateParameter<Vignette.Outline>.Bounded<ShortSideLength, float, InvalidPixelValue>(Lens<Vignette.Outline, ShortSideLength>.New(static outline => outline.Feather, static feather => outline => outline with { Feather = feather }), ShortSideLength.Presentation));
    public static readonly VignetteOutlineParameter Pull = new("pull", new StateParameter<Vignette.Outline>.Color(Lens<Vignette.Outline, Swatch>.New(static outline => outline.Pull, static pull => outline => outline with { Pull = pull })));

    public StateParameter<Vignette.Outline> Kind { get; }
}
