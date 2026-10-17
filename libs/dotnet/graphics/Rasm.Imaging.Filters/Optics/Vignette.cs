using System.Numerics;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Filters.Optics;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record Vignette : IStateRecord<Vignette, VignetteParameter>, IPixelStage<Vignette> {
    public static Vignette Default { get; } = Natural.Default;

    public static Option<PixelPass> Pass(Vignette state, PassContext context) =>
        state.Switch(context,
            natural: static (run, _) => run.Camera.Bind(static camera => camera.Frustum.Switch(perspective: static frustum => Some(frustum.Window), parallel: static _ => Option<ViewWindow>.None))
                .Map<PixelPass>(window => Falloff(Vector3.Zero, (column, line) => window.WindowPoint(column + 0.5d, (double)run.Extent.Height - line - 0.5d, run.Extent) switch {
                    var tangent => (1d + (tangent.X * tangent.X) + (tangent.Y * tangent.Y)) switch { var spread => (float)(1d / (spread * spread)) },
                })),
            outline: static (run, outline) => (outline.Shape.Field(run.Extent, ShapeFields.Precision(run.Extent)).Sample, outline.Shape.Fit.Scale(run.Extent)) switch {
                var (sample, scale) => (ShapeEdge.Band(outline.Feather, run.Extent, double.Min(scale.X, scale.Y)) / run.Extent.ShortSide) switch {
                    var band => Some<PixelPass>(Falloff(outline.Pull.SceneLight(run.Working), (column, line) => ShapeEdge.Coverage(sample(column, line).Distance.ToDouble(), band))),
                },
            });

    public sealed record Natural : Vignette, IStateRecord<Natural> {
        public static new Natural Default { get; } = new();
    }

    public sealed record Outline(ShapePrimitive Shape, ShortSideLength Feather, Swatch Pull) : Vignette, IStateRecord<Outline, VignetteOutlineParameter> {
        public static new Outline Default { get; } = new(ShapePrimitive.Default with { Width = ShortSideLength.Create(0.8f), Height = ShortSideLength.Create(0.45f), Fit = ShapeFit.LongSide },
            ShortSideLength.Create(0.2f * float.Sqrt(float.Tau) / 3f), Swatch.Black);
    }

    private static PixelPass.Pointwise Falloff(Vector3 pull, Func<long, long, float> weight) =>
        new((row, column, line) => {
            for (int x = 0; x < row.Length; x++)
                row[x] = new(Vector3.Lerp(pull, row[x].AsVector3(), weight(column + x, line)), row[x].W);
        });
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VignetteParameter : IStateParameter<Vignette> {
    public static readonly VignetteParameter Falloff = new("falloff", new StateParameter<Vignette>.Variant<Vignette>(Lens.identity<Vignette>()),
        new("Falloff", "Camera illumination or a feathered outline"));

    public StateParameter<Vignette> Kind { get; }
    public ParameterText Text { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VignetteOutlineParameter : IStateParameter<Vignette.Outline> {
    public static readonly VignetteOutlineParameter Shape = new("shape", new StateParameter<Vignette.Outline>.Record<ShapePrimitive>(Lens<Vignette.Outline, ShapePrimitive>.New(static outline => outline.Shape, static shape => outline => outline with { Shape = shape })),
        new("Boundary", "Figure bounding the illuminated region"));
    public static readonly VignetteOutlineParameter Feather = new("feather", StateParameter.Bounded(Lens<Vignette.Outline, ShortSideLength>.New(static outline => outline.Feather, static feather => outline => outline with { Feather = feather })),
        new("Feather", "Edge softness as a share of the frame's short side"));
    public static readonly VignetteOutlineParameter Pull = new("pull", new StateParameter<Vignette.Outline>.Color(Lens<Vignette.Outline, Swatch>.New(static outline => outline.Pull, static pull => outline => outline with { Pull = pull })),
        new("Pull", "Scene-linear color beyond the outline"));

    public StateParameter<Vignette.Outline> Kind { get; }
    public ParameterText Text { get; }
}
