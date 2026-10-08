using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Objects;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Annotation;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct HatchScale : System.Numerics.IMinMaxValue<HatchScale> {
    public static HatchScale MinValue { get; } = new(double.BitIncrement(0.001));
    public static HatchScale MaxValue { get; } = new(double.MaxValue);
    public static HatchScale Unit { get; } = new(1.0);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

public sealed record HatchFill(ComponentRef<HatchPattern> Pattern, double Rotation, HatchScale Scale);

[Union]
public abstract partial record HatchForm {
    private HatchForm(HatchFill fill) => Fill = fill;

    public HatchFill Fill { get; }

    public sealed record Loops(HatchFill Fill, Plane Plane, Curve Outer, Seq<Curve> Inner) : HatchForm(Fill);

    public sealed record Boundaries(HatchFill Fill, Seq<Curve> Curves, Tolerances Tolerances) : HatchForm(Fill);

    public sealed record OnFace(HatchFill Fill, Brep Brep, int FaceIndex, Point3d BasePoint) : HatchForm(Fill);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HatchEdit {
    public sealed record Pattern(ComponentRef<HatchPattern> Address) : HatchEdit;

    public sealed record Rotation(double Radians) : HatchEdit;

    public sealed record Scale(HatchScale Factor) : HatchEdit;

    public sealed record Rescale(Transform Xform) : HatchEdit;

    public sealed record BasePoint(Point3d Point) : HatchEdit;

    public sealed record HatchPlane(Plane Plane) : HatchEdit;

    public sealed record Fill(Option<ColorGradient> Gradient) : HatchEdit;

    public IO<Unit> Apply(RhinoDoc doc, Hatch hatch) =>
        Switch(
            (Doc: doc, Hatch: hatch),
            pattern: static (state, pattern) => TableOps.Find(state.Doc.HatchPatterns, pattern.Address, includeDeleted: false)
                .Bind(row => IO.lift(() => { state.Hatch.PatternIndex = row.Index; })),
            rotation: static (state, rotation) => IO.lift(() => { state.Hatch.PatternRotation = rotation.Radians; }),
            scale: static (state, scale) => IO.lift(() => { state.Hatch.PatternScale = scale.Factor; }),
            rescale: static (state, rescale) => IO.lift(() => state.Hatch.ScalePattern(rescale.Xform)),
            basePoint: static (state, point) => IO.lift(() => { state.Hatch.BasePoint = point.Point; }),
            hatchPlane: static (state, plane) => IO.lift(() => { state.Hatch.Plane = plane.Plane; }),
            fill: static (state, fill) => IO.lift(() => state.Hatch.SetGradientFill(fill.Gradient.ValueUnsafe())));
}

public sealed record HatchState(Option<int> PatternIndex, Plane Plane, Point3d BasePoint, double PatternRotation, HatchScale PatternScale, Option<ColorGradient> Gradient);

public sealed record HatchDisplay(Seq<Curve> Bounds, Seq<Line> Lines, Option<Brep> Solid);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class HatchMapper {
    [MapPropertyFromSource(nameof(HatchState.Gradient), Use = nameof(Gradient))]
    internal static partial HatchState ToState(Hatch hatch, HatchScale patternScale);

    private static Option<ColorGradient> Gradient(Hatch hatch) =>
        Some(hatch.GetGradientFill()).Filter(static fill => fill.GradientType != GradientType.None);
}

public static class Hatches {
    // --- [CREATION]
    public static IO<Seq<Hatch>> Create(RhinoDoc doc, HatchForm form) =>
        from row in TableOps.Find(doc.HatchPatterns, form.Fill.Pattern, includeDeleted: false)
        from hatches in form.Switch(
            row.Index,
            loops: static (index, loops) =>
                Copies.Acquire(() => Hatch.Create(loops.Plane, loops.Outer, loops.Inner, index, loops.Fill.Rotation, loops.Fill.Scale), nameof(Hatch.Create))
                    .Map(static hatch => Seq(hatch)),
            boundaries: static (index, boundaries) =>
                Copies.AcquireNonEmpty(() => Hatch.Create(boundaries.Curves, index, boundaries.Fill.Rotation, boundaries.Fill.Scale, boundaries.Tolerances.Absolute), nameof(Hatch.Create)),
            onFace: static (index, face) =>
                Copies.Acquire(
                        () => IndexOutOfRange.Unless(face.FaceIndex, face.Brep.Faces.Count, nameof(Hatch.CreateFromBrep))
                            .Bind(_ => Missing.Unless(Hatch.CreateFromBrep(face.Brep, face.FaceIndex, index, face.Fill.Rotation, face.Fill.Scale, face.BasePoint), nameof(Hatch.CreateFromBrep))),
                        nameof(Hatch.CreateFromBrep))
                    .Map(static hatch => Seq(hatch)))
        select hatches;

    // --- [EDITS]
    public static IO<Unit> Revise(RhinoDoc doc, Guid id, Seq<HatchEdit> edits) =>
        RhinoObjects.ReplaceGeometry<Hatch>(doc, id, copy => edits.TraverseM(edit => edit.Apply(doc, copy)).As().Map(static _ => unit));

    // --- [READS]
    public static IO<HatchState> State(Hatch hatch) =>
        IO.lift(() => Conversions.Validated<HatchScale, double, InvalidRhinoValue>(hatch.PatternScale).Map(scale => HatchMapper.ToState(hatch, scale)));

    // --- [DISPLAY]
    public static IO<A> Display<A>(Hatch hatch, HatchPattern pattern, double patternScale, Func<HatchDisplay, IO<A>> body) =>
        IO.lift(() => {
            hatch.CreateDisplayGeometry(pattern, patternScale, out Curve[] bounds, out Line[] lines, out Brep solid);
            return new HatchDisplay(Conversions.Rows(bounds), toSeq(lines), Optional(solid));
        }).Bracket(
            Use: body,
            Fin: static display => DisposalOps.Release<GeometryBase>([.. display.Bounds, .. display.Solid.ToSeq()]));
}
