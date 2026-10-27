using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Document.Units;
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
        from written in Seq(
                patternIndex.Map(static index => (Func<Hatch, IO<Unit>>)(held => IO.lift(() => { held.PatternIndex = index; }))),
                PatternRotation.Map(static rotation => (Func<Hatch, IO<Unit>>)(held => IO.lift(() => { held.PatternRotation = rotation; }))),
                PatternScale.Map(static scale => (Func<Hatch, IO<Unit>>)(held => IO.lift(() => { held.PatternScale = scale; }))),
                BasePoint.Map(static point => (Func<Hatch, IO<Unit>>)(held => IO.lift(() => { held.BasePoint = point; }))),
                Plane.Map(static plane => (Func<Hatch, IO<Unit>>)(held => IO.lift(() => { held.Plane = plane; }))),
                Rescale.Map(static xform => (Func<Hatch, IO<Unit>>)(held => IO.lift(() => held.ScalePattern(xform)))),
                Gradient.Map(static fill => (Func<Hatch, IO<Unit>>)(held => IO.lift(() => held.SetGradientFill(fill.ValueUnsafe())))))
            .Somes()
            .TraverseM(edit => edit(hatch))
            .As()
        select unit;
}

public sealed record HatchState(Option<int> PatternIndex, Plane Plane, Point3d BasePoint, double PatternRotation, double PatternScale, Option<ColorGradient> Gradient);

public sealed record HatchDisplay(Seq<Curve> Bounds, Seq<Line> Lines, Option<Brep> Solid);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class HatchMapper {
    [MapPropertyFromSource(nameof(HatchState.Gradient), Use = nameof(Gradient))]
    internal static partial HatchState ToState(Hatch hatch);

    private static Option<ColorGradient> Gradient(Hatch hatch) =>
        Some(hatch.GetGradientFill()).Filter(static fill => fill.GradientType is not GradientType.None);
}

public static class Hatches {
    // --- [CREATION]
    public static IO<Seq<Hatch>> Create(RhinoDoc doc, HatchFill fill, HatchForm form) =>
        from row in TableOps.Find(doc.HatchPatterns, fill.Pattern, includeDeleted: false)
        from hatches in form.Switch(
            (row.Index, Fill: fill),
            loops: static (state, loops) =>
                Products.Made(Hatch.Create(loops.Plane, loops.Outer, loops.Inner, state.Index, state.Fill.Rotation, state.Fill.Scale)).Map(static hatch => Seq(hatch)),
            boundaries: static (state, boundaries) =>
                from made in Products.Made(Hatch.Create(boundaries.Curves, state.Index, state.Fill.Rotation, state.Fill.Scale, boundaries.Tolerances.Absolute))
                from present in IO.lift(() => RhinoFault.Unless(!made.IsEmpty))
                select made,
            onFace: static (state, face) =>
                Products.Made(Hatch.CreateFromBrep(face.Face.Brep, face.Face.FaceIndex, state.Index, state.Fill.Rotation, state.Fill.Scale, face.BasePoint)).Map(static hatch => Seq(hatch)))
        select hatches;

    // --- [EDITS]
    public static IO<Unit> Revise(RhinoDoc doc, Guid id, Seq<HatchEdit> edits) =>
        RhinoObjects.ReplaceGeometry(doc, id, (Hatch target) => edits.TraverseM(edit => edit.Apply(doc, target)).As().Map(_ => (target, unit)));

    // --- [READS]
    public static IO<HatchState> State(Hatch hatch) =>
        IO.lift(() => HatchMapper.ToState(hatch));

    // --- [DISPLAY]
    public static IO<T> Display<T>(Hatch hatch, HatchPattern pattern, double patternScale, Func<HatchDisplay, IO<T>> body) =>
        (from display in IO.lift(() => {
            hatch.CreateDisplayGeometry(pattern, patternScale, out Curve[] bounds, out Line[] lines, out Brep solid);
            return (Bounds: bounds, Lines: lines, Solid: solid);
        })
         from bounds in Products.Made(display.Bounds)
         from solid in Products.Held(display.Solid)
         from value in body(new HatchDisplay(bounds, Conversions.Rows(display.Lines), solid))
         select value).Bracket();
}
