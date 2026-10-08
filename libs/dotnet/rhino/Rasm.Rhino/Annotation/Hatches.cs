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

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record HatchForm {
    public sealed record Loops(Plane Plane, Curve Outer, Seq<Curve> Inner) : HatchForm;
    public sealed record Boundaries(Seq<Curve> Curves, Tolerances Tolerances) : HatchForm;
    public sealed record OnFace(BrepFace Face, Point3d BasePoint) : HatchForm;
}

public sealed record HatchEdit(
    Option<ComponentRef<HatchPattern>> Pattern = default,
    Option<double> PatternRotation = default,
    Option<HatchScale> PatternScale = default,
    Option<Point3d> BasePoint = default,
    Option<Plane> Plane = default,
    Option<Transform> Rescale = default,
    Option<Option<ColorGradient>> Gradient = default) {
    public IO<Unit> Apply(RhinoDoc doc, Hatch hatch) =>
        from patternIndex in Pattern.TraverseM(address => TableOps.Find(doc.HatchPatterns, address, includeDeleted: false).Map(static row => row.Index)).As()
        from written in IO.lift(() => HatchMapper.Update((patternIndex.ToNullable(), PatternRotation.ToNullable(), PatternScale.ToNullable(), BasePoint.ToNullable(), Plane.ToNullable()), hatch))
        from rescaled in IO.lift(() => Rescale.Iter(hatch.ScalePattern))
        from filled in IO.lift(() => Gradient.Iter(fill => hatch.SetGradientFill(fill.ValueUnsafe())))
        select filled;
}

public sealed record HatchState(Option<int> PatternIndex, Plane Plane, Point3d BasePoint, double PatternRotation, HatchScale PatternScale, Option<ColorGradient> Gradient);

public sealed record HatchDisplay(Seq<Curve> Bounds, Seq<Line> Lines, Option<Brep> Solid);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper(AllowNullPropertyAssignment = false, ThrowOnPropertyMappingNullMismatch = false)]
internal static partial class HatchMapper {
    // --- [EDITS]
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update((int? PatternIndex, double? PatternRotation, HatchScale? PatternScale, Point3d? BasePoint, Plane? Plane) edit, Hatch hatch);

    [UserMapping]
    private static double Key(HatchScale scale) => scale.ToValue();

    // --- [READS]
    [MapPropertyFromSource(nameof(HatchState.Gradient), Use = nameof(Gradient))]
    internal static partial HatchState ToState(Hatch hatch, HatchScale patternScale);

    private static Option<ColorGradient> Gradient(Hatch hatch) =>
        Some(hatch.GetGradientFill()).Filter(static fill => fill.GradientType != GradientType.None);
}

public static class Hatches {
    // --- [CREATION]
    public static IO<Seq<Hatch>> Create(RhinoDoc doc, HatchFill fill, HatchForm form) =>
        from row in TableOps.Find(doc.HatchPatterns, fill.Pattern, includeDeleted: false)
        from hatches in form.Switch(
            (row.Index, Fill: fill),
            loops: static (state, loops) =>
                Copies.Acquire(() => Hatch.Create(loops.Plane, loops.Outer, loops.Inner, state.Index, state.Fill.Rotation, state.Fill.Scale), nameof(Hatch.Create))
                    .Map(static hatch => Seq(hatch)),
            boundaries: static (state, boundaries) =>
                Copies.AcquireNonEmpty(() => Hatch.Create(boundaries.Curves, state.Index, state.Fill.Rotation, state.Fill.Scale, boundaries.Tolerances.Absolute), nameof(Hatch.Create)),
            onFace: static (state, face) =>
                Copies.Acquire(() => Hatch.CreateFromBrep(face.Face.Brep, face.Face.FaceIndex, state.Index, state.Fill.Rotation, state.Fill.Scale, face.BasePoint), nameof(Hatch.CreateFromBrep))
                    .Map(static hatch => Seq(hatch)))
        select hatches;

    // --- [EDITS]
    public static IO<Unit> Revise(RhinoDoc doc, Guid id, Seq<HatchEdit> edits) =>
        RhinoObjects.ReplaceGeometry(doc, id, (Hatch copy) => edits.TraverseM(edit => edit.Apply(doc, copy)).As().Map(static _ => unit));

    // --- [READS]
    public static IO<HatchState> State(Hatch hatch) =>
        from scale in IO.lift(() => Conversions.Validated<HatchScale, double, InvalidRhinoValue>(hatch.PatternScale))
        select HatchMapper.ToState(hatch, scale);

    // --- [DISPLAY]
    public static IO<A> Display<A>(Hatch hatch, HatchPattern pattern, double patternScale, Func<HatchDisplay, IO<A>> body) =>
        IO.lift(() => {
            hatch.CreateDisplayGeometry(pattern, patternScale, out Curve[] bounds, out Line[] lines, out Brep solid);
            return new HatchDisplay(Conversions.Rows(bounds), Conversions.Rows(lines), Optional(solid));
        }).Bracket(
            Use: display => IO.pure(display).Bind(body),
            Fin: static display => DisposalOps.Release<GeometryBase>([.. display.Bounds, .. display.Solid.ToSeq()]));
}
