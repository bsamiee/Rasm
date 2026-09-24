using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Document;
using Rasm.Rhino.Objects;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Annotation;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HatchForm {
    public abstract int PatternIndex { get; init; }

    public abstract double RotationRadians { get; init; }

    public abstract double Scale { get; init; }

    public sealed record Loops(int PatternIndex, double RotationRadians, double Scale, Plane HatchPlane, Curve OuterLoop, Seq<Curve> InnerLoops) : HatchForm;

    public sealed record CurveSet(int PatternIndex, double RotationRadians, double Scale, Seq<Curve> Curves, double Tolerance) : HatchForm;

    public sealed record OnFace(int PatternIndex, double RotationRadians, double Scale, Brep Brep, int FaceIndex, Point3d BasePoint) : HatchForm;
}

public sealed record HatchState(int PatternIndex, Plane Plane, Point3d BasePoint, double PatternRotation, double PatternScale, ColorGradient Gradient);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
public static partial class Hatches {
    // --- [PLACEMENT]
    public static IO<Seq<Guid>> Place(RhinoDoc doc, HatchForm form, Option<ObjectAttributes> attributes, Option<HistoryRecord> history, bool reference) =>
        Disposal.Using(
            form.Switch(
                loops: static loops => IO.lift(() =>
                    Missing.Unless(Hatch.Create(loops.HatchPlane, loops.OuterLoop, loops.InnerLoops, loops.PatternIndex, loops.RotationRadians, loops.Scale), nameof(Hatch.Create))
                        .Map(static hatch => Seq(hatch))),
                curveSet: static set => IO.lift(() => Answers.NonEmpty(toSeq(Hatch.Create(set.Curves, set.PatternIndex, set.RotationRadians, set.Scale, set.Tolerance)), nameof(Hatch.Create))),
                onFace: static face =>
                    from inRange in IO.lift(() => IndexOutOfRange.Unless(face.FaceIndex, face.Brep.Faces.Count, nameof(HatchForm.OnFace.FaceIndex)))
                    from made in IO.lift(() =>
                        Missing.Unless(Hatch.CreateFromBrep(face.Brep, face.FaceIndex, face.PatternIndex, face.RotationRadians, face.Scale, face.BasePoint), nameof(Hatch.CreateFromBrep))
                            .Map(static hatch => Seq(hatch)))
                    select made),
            hatches => TableOps.Apply(doc, new TableOp.Add(hatches.Map(hatch => new GeometryPair(hatch, attributes)), history, reference)));

    // --- [EDITS]
    public static IO<Unit> SetGradient(RhinoDoc doc, Guid id, Option<ColorGradient> fill) =>
        RhinoObjects.ReplaceGeometry<Hatch>(doc, id, hatch => IO.lift(() => hatch.SetGradientFill(fill.ValueUnsafe())));

    // --- [READS]
    public static IO<HatchState> State(RhinoDoc doc, Guid id) =>
        from resolved in Queries.Resolve<RhinoObject, Hatch>(doc, id)
        from state in IO.lift(() => Project(resolved.Geometry))
        select state;

    [MapPropertyFromSource(nameof(HatchState.Gradient), Use = nameof(Gradient))]
    private static partial HatchState Project(Hatch hatch);

    private static ColorGradient Gradient(Hatch hatch) =>
        hatch.GetGradientFill();

    public static IO<TValue> WithDisplay<TValue>(RhinoDoc doc, Guid id, double patternScale, Func<(Seq<Curve> Bounds, Seq<Line> Lines, Option<Brep> Solid), IO<TValue>> body) =>
        from resolved in Queries.Resolve<RhinoObject, Hatch>(doc, id)
        from row in IO.lift(() => Missing.Unless(doc.HatchPatterns.FindIndex(resolved.Geometry.PatternIndex), nameof(HatchPatternTable.FindIndex)))
        from drawn in IO.lift(() => {
            resolved.Geometry.CreateDisplayGeometry(row, patternScale, out Curve[] bounds, out Line[] lines, out Brep solid);
            return (Bounds: toSeq(bounds), Lines: toSeq(lines), Solid: Optional(solid));
        })
        from value in Disposal.Using(
            IO.pure(drawn.Bounds.Map<IDisposable>(static curve => curve) + drawn.Solid.ToSeq().Map<IDisposable>(static brep => brep)),
            _ => body(drawn))
        select value;
}
