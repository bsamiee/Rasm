using System.Numerics;
using Rasm.Imaging.Filters.Generators;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using UnitsNet;
using UnitsNet.Units;

namespace Rasm.Imaging.Filters.Optics;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Vignette : IStateRecord<Vignette, VignetteParameter, InvalidOptics>, IPixelStage<Vignette> {
    public static Vignette Default { get; } = new Natural();

    public static Option<PixelPass> Pass(Vignette state, PassContext context) =>
        state.Switch(context,
            natural: static (run, _) => run.Camera.Bind(camera => camera.Frustum.Switch(run.Extent,
                perspective: static (extent, perspective) => Some<PixelPass>(Falloff(perspective.Window, extent)),
                parallel: static (_, _) => None)),
            outline: static (run, outline) => Some(outline.Shape.Kind.Accept(outline.Shape, run.Extent,
                new VignetteFill(outline.Shape.Placement.Frame(run.Extent), int.Min(run.Extent.Width, run.Extent.Height),
                    ShapeEdge.Band(outline.Feather, run.Extent, outline.Shape.Fit.Scale(run.Extent)), outline.Pull.SceneLight(run.Working)))));

    private static PixelPass.Pointwise Falloff(ViewWindow window, PixelExtent extent) =>
        new((row, column, line) => {
            for (int x = 0; x < row.Length; x++) {
                Vector2 tangent = window.WindowPoint(column + x + 0.5d, extent.Height - line - 0.5d, extent);
                double lift = 1d + ((double)tangent.X * tangent.X) + ((double)tangent.Y * tangent.Y);
                row[x] *= new Vector4(new Vector3((float)(1d / (lift * lift))), 1f);
            }
        });

    public sealed record Natural : Vignette, IStateRecord<Natural> {
        public static new Natural Default { get; } = new();
        public static IReadOnlyList<IStateParameter<Natural>> Parameters => [];
        public static Option<IStateParameter<Natural>> FindParameter(string key) => None;
    }

    public sealed record Outline(ShapePrimitive Shape, ShortSideLength Feather, Swatch Pull)
        : Vignette, IStateRecord<Outline, VignetteOutlineParameter, InvalidOptics> {
        public static new Outline Default { get; } = new(
            new(ShapeKind.Ellipse, Placement.Centered, ShortSideExtent.Create(0.8f), ShortSideExtent.Create(0.45f),
                AxisFraction.MinValue, SuperellipseExponent.Ellipse, SideCount.Five, AxisFraction.MinValue) { Fit = ShapeFit.LongSide },
            ShortSideLength.Create(0.2f * MathF.Sqrt(float.Tau) / 3f), Swatch.Black);
    }
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
    public static readonly VignetteParameter Falloff = new("falloff", new StateParameter<Vignette>.Variant<Vignette, VignetteMode, InvalidOptics>(
        Lens.identity<Vignette>()));

    public StateParameter<Vignette> Kind { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidOptics>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class VignetteOutlineParameter : IStateParameter<Vignette.Outline> {
    private static readonly (StateParameter<Vignette.Outline> Kind,
        (StateParameter<Vignette.Outline> CenterX, StateParameter<Vignette.Outline> CenterY, StateParameter<Vignette.Outline> Rotation) Placement,
        StateParameter<Vignette.Outline> Width, StateParameter<Vignette.Outline> Height, StateParameter<Vignette.Outline> Roundness,
        StateParameter<Vignette.Outline> Exponent, StateParameter<Vignette.Outline> Sides, StateParameter<Vignette.Outline> Inset,
        StateParameter<Vignette.Outline> Fit) Shape =
        ShapePrimitive.Kinds(Lens<Vignette.Outline, ShapePrimitive>.New(static outline => outline.Shape, static shape => outline => outline with { Shape = shape }));

    public static readonly VignetteOutlineParameter ShapeKind = new("kind", Shape.Kind);
    public static readonly VignetteOutlineParameter CenterX = new("center-x", Shape.Placement.CenterX);
    public static readonly VignetteOutlineParameter CenterY = new("center-y", Shape.Placement.CenterY);
    public static readonly VignetteOutlineParameter Rotation = new("rotation", Shape.Placement.Rotation);
    public static readonly VignetteOutlineParameter Width = new("width", Shape.Width);
    public static readonly VignetteOutlineParameter Height = new("height", Shape.Height);
    public static readonly VignetteOutlineParameter Roundness = new("roundness", Shape.Roundness);
    public static readonly VignetteOutlineParameter Exponent = new("exponent", Shape.Exponent);
    public static readonly VignetteOutlineParameter Sides = new("sides", Shape.Sides);
    public static readonly VignetteOutlineParameter Inset = new("inset", Shape.Inset);
    public static readonly VignetteOutlineParameter Fit = new("fit", Shape.Fit);
    public static readonly VignetteOutlineParameter Feather = new("feather", new StateParameter<Vignette.Outline>.Bounded<ShortSideLength, float, InvalidPixelValue>(
        Lens<Vignette.Outline, ShortSideLength>.New(static outline => outline.Feather, static feather => outline => outline with { Feather = feather }),
        new() { Unit = Quantity.GetUnitInfo(RatioUnit.DecimalFraction) }));
    public static readonly VignetteOutlineParameter Pull = new("pull", new StateParameter<Vignette.Outline>.Color(
        Lens<Vignette.Outline, Swatch>.New(static outline => outline.Pull, static pull => outline => outline with { Pull = pull })));

    public StateParameter<Vignette.Outline> Kind { get; }
}

file sealed class VignetteFill(FieldFrame frame, float pixels, float band, Vector3 pull) : IFieldVisitor<PixelPass> {
    public PixelPass Visit<TField>(TField field) where TField : struct, IDistanceField =>
        new PixelPass.Pointwise((row, column, line) => {
            for (int x = 0; x < row.Length; x++) {
                float weight = ShapeEdge.Coverage(field.Distance(frame.Point(column + x, line)) * pixels, band);
                row[x] = new Vector4(Vector3.Lerp(pull, row[x].AsVector3(), weight), row[x].W);
            }
        });
}
