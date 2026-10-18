using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Modeling;
using Rasm.Rhino.Objects;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Annotation;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct HatchScale : System.Numerics.IMinMaxValue<HatchScale> {
    public static HatchScale MinValue { get; } = new(double.BitIncrement(0.001));
    public static HatchScale MaxValue { get; } = new(double.MaxValue);
    public static HatchScale Unit { get; } = new(1.0);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
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
        from written in IO.lift(() => HatchMapper.Update((patternIndex, PatternRotation, PatternScale.Map(static scale => (double)scale), BasePoint, Plane), hatch))
        from rescaled in IO.lift(() => Rescale.Iter(hatch.ScalePattern))
        from filled in IO.lift(() => Gradient.Iter(fill => hatch.SetGradientFill(fill.ValueUnsafe())))
        select filled;
}

public sealed record HatchState(Option<int> PatternIndex, Plane Plane, Point3d BasePoint, double PatternRotation, HatchScale PatternScale, Option<ColorGradient> Gradient);

public sealed record HatchDisplay(Seq<Curve> Bounds, Seq<Line> Lines, Option<Brep> Solid);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
[UseStaticMapper(typeof(Patches))]
internal static partial class HatchMapper {
    // --- [EDITS]
    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update((Option<int> PatternIndex, Option<double> PatternRotation, Option<double> PatternScale, Option<Point3d> BasePoint, Option<Plane> Plane) edit, Hatch hatch);

    // --- [READS]
    [MapPropertyFromSource(nameof(HatchState.Gradient), Use = nameof(Gradient))]
    internal static partial HatchState ToState(Hatch hatch, HatchScale patternScale);

    private static Option<ColorGradient> Gradient(Hatch hatch) =>
        Some(hatch.GetGradientFill()).Filter(static fill => fill.GradientType is not GradientType.None);
}

public static class Hatches {
    // --- [CREATION]
    public static IO<Seq<Hatch>> Create(RhinoDoc doc, HatchFill fill, HatchForm form, Action<Seq<Hatch>, IDisposable> retain) =>
        DisposalOps.Acquire(held =>
            from row in TableOps.Find(doc.HatchPatterns, fill.Pattern, includeDeleted: false)
            from hatches in form.Switch(
                (row.Index, Fill: fill, Retain: held),
                loops: static (state, loops) =>
                    Copies.Acquire(() => Hatch.Create(loops.Plane, loops.Outer, loops.Inner, state.Index, state.Fill.Rotation, state.Fill.Scale), nameof(Hatch.Create), state.Retain)
                        .Map(static hatch => Seq(hatch)),
                boundaries: static (state, boundaries) =>
                    Copies.AcquireNonEmpty(() => Hatch.Create(boundaries.Curves, state.Index, state.Fill.Rotation, state.Fill.Scale, boundaries.Tolerances.Absolute), nameof(Hatch.Create), state.Retain),
                onFace: static (state, face) =>
                    Copies.Acquire(() => Hatch.CreateFromBrep(face.Face.Brep, face.Face.FaceIndex, state.Index, state.Fill.Rotation, state.Fill.Scale, face.BasePoint), nameof(Hatch.CreateFromBrep), state.Retain)
                        .Map(static hatch => Seq(hatch)))
            select hatches, retain);

    // --- [EDITS]
    public static IO<Unit> Revise(RhinoDoc doc, Guid id, Seq<HatchEdit> edits) =>
        RhinoObjects.ReplaceGeometry(doc, id, (Hatch copy) => edits.TraverseM(edit => edit.Apply(doc, copy)).As().Map(unit));

    // --- [READS]
    public static IO<HatchState> State(Hatch hatch) =>
        from scale in IO.lift(() => HatchScale.Validated(hatch.PatternScale))
        select HatchMapper.ToState(hatch, scale);

    // --- [DISPLAY]
    public static IO<T> Display<T>(Hatch hatch, HatchPattern pattern, double patternScale, Func<HatchDisplay, IO<T>> body) =>
        DisposalOps.Within(() => {
            hatch.CreateDisplayGeometry(pattern, patternScale, out Curve[] bounds, out Line[] lines, out Brep solid);
            return (Bounds: bounds, Lines: lines, Solid: solid);
        },
            display => IO.lift(() => new HatchDisplay(Conversions.Rows(display.Bounds), Conversions.Rows(display.Lines), Optional(display.Solid))).Bind(body),
            static display => DisposalOps.Release<GeometryBase?>([.. Conversions.Rows(display.Bounds), display.Solid]));
}
