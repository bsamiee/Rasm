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
public sealed record HatchFill(ComponentRef<HatchPattern> Pattern, double Rotation, double Scale);

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record HatchForm {
    public sealed record Loops(Plane Plane, Curve Outer, Seq<Curve> Inner) : HatchForm;
    public sealed record Boundaries(Seq<Curve> Curves, Tolerances Tolerances) : HatchForm;
    public sealed record OnFace(BrepFace Face, Point3d BasePoint) : HatchForm;
}

public sealed record HatchEdit(
    Option<ComponentRef<HatchPattern>> Pattern = default,
    Option<double> PatternRotation = default,
    Option<double> PatternScale = default,
    Option<Point3d> BasePoint = default,
    Option<Plane> Plane = default,
    Option<Transform> Rescale = default,
    Option<Option<ColorGradient>> Gradient = default) {
    public IO<Unit> Apply(RhinoDoc doc, Hatch hatch) =>
        from patternIndex in Pattern.TraverseM(address => TableOps.Find(doc.HatchPatterns, address, includeDeleted: false).Map(static row => row.Index)).As()
        from written in IO.lift(() => HatchMapper.Update((patternIndex, PatternRotation, PatternScale, BasePoint, Plane), hatch))
        from rescaled in IO.lift(() => Rescale.Iter(hatch.ScalePattern))
        from filled in IO.lift(() => Gradient.Iter(fill => hatch.SetGradientFill(fill.ValueUnsafe())))
        select filled;
}

public sealed record HatchState(Option<int> PatternIndex, Plane Plane, Point3d BasePoint, double PatternRotation, double PatternScale, Option<ColorGradient> Gradient);

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
    internal static partial HatchState ToState(Hatch hatch);

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
                    Copies.Acquire(nameof(Hatch.Create), _ => Answer.One(Hatch.Create(loops.Plane, loops.Outer, loops.Inner, state.Index, state.Fill.Rotation, state.Fill.Scale)).Map(static hatch => Seq(hatch)), state.Retain),
                boundaries: static (state, boundaries) =>
                    Copies.Acquire(nameof(Hatch.Create), _ => Answer.NonEmpty(Hatch.Create(boundaries.Curves, state.Index, state.Fill.Rotation, state.Fill.Scale, boundaries.Tolerances.Absolute)), state.Retain),
                onFace: static (state, face) =>
                    Copies.Acquire(nameof(Hatch.CreateFromBrep), _ => Answer.One(Hatch.CreateFromBrep(face.Face.Brep, face.Face.FaceIndex, state.Index, state.Fill.Rotation, state.Fill.Scale, face.BasePoint)).Map(static hatch => Seq(hatch)), state.Retain))
            select hatches, retain);

    // --- [EDITS]
    public static IO<Unit> Revise(RhinoDoc doc, Guid id, Seq<HatchEdit> edits) =>
        RhinoObjects.ReplaceGeometry(doc, id, (Hatch target, Action<IDisposable> _) => edits.TraverseM(edit => edit.Apply(doc, target)).As().Map(_ => (target, unit)));

    // --- [READS]
    public static IO<HatchState> State(Hatch hatch) =>
        IO.lift(() => HatchMapper.ToState(hatch));

    // --- [DISPLAY]
    public static IO<T> Display<T>(Hatch hatch, HatchPattern pattern, double patternScale, Func<HatchDisplay, IO<T>> body) =>
        DisposalOps.Within((Action<IDisposable> held) => Copies.Acquire(nameof(Hatch.CreateDisplayGeometry), _ => {
            hatch.CreateDisplayGeometry(pattern, patternScale, out Curve[] bounds, out Line[] lines, out Brep solid);
            return (Answer.Many(bounds), Answer.Optional(solid)).Apply((kept, fill) => new HatchDisplay(kept, Conversions.Rows(lines), fill)).As();
        }, held).Bind(body), DisposalOps.Release);
}
