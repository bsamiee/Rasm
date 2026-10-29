using System.Runtime.CompilerServices;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class NetworkContinuity {
    public static readonly NetworkContinuity Loose = new(0);
    public static readonly NetworkContinuity Position = new(1);
    public static readonly NetworkContinuity Tangency = new(2);
    public static readonly NetworkContinuity Curvature = new(3);
}

[SmartEnum<int>]
public sealed partial class NetworkFailure {
    public static readonly NetworkFailure Sorting = new(1, "network curves could not be sorted");
    public static readonly NetworkFailure Initialization = new(2, "network surface could not be initialized");
    public static readonly NetworkFailure Build = new(3, "network surface could not be built");
    public static readonly NetworkFailure Validity = new(4, "network surface is invalid");

    public string Message { get; }
}

[Union<Point3d, BrepTrim>(T1Name = "Apex", T2Name = "Tangent", SkipToString = true, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class LoftEnd;

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record LoftEnds {
    public sealed record Fitted : LoftEnds {
        public Option<Point3d> Start { get; init; }

        public Option<Point3d> End { get; init; }

        public CurveFit Fit { get; init; } = new CurveFit.AsIs();
    }

    public sealed record Tangent : LoftEnds {
        public Option<LoftEnd> Start { get; init; }

        public Option<LoftEnd> End { get; init; }
    }
}

public sealed record Loft(Seq<Curve> Sections) {
    public LoftType Type { get; init; } = LoftType.Normal;

    public bool Closed { get; init; }

    public LoftEnds Ends { get; init; } = new LoftEnds.Fitted();

    // --- [BREPS]
    public IO<Seq<Brep>> ToBreps(Tolerances tolerances) =>
        Ends.Switch(
            (Loft: this, tolerances.Angle),
            fitted: static (at, ends) => ends.Fit.Switch(
                (at.Loft, at.Angle, Start: Conversions.Unset(ends.Start), End: Conversions.Unset(ends.End)),
                asIs: static (run, _) => Products.Many(() => Brep.CreateFromLoft(run.Loft.Sections, run.Start, run.End, run.Loft.Type, run.Loft.Closed, run.Angle)),
                rebuild: static (run, rebuild) => Products.Many(() => Brep.CreateFromLoftRebuild(run.Loft.Sections, run.Start, run.End, run.Loft.Type, run.Loft.Closed, run.Angle, rebuild.Count)),
                refit: static (run, refit) => Products.Many(() => Brep.CreateFromLoftRefit(run.Loft.Sections, run.Start, run.End, run.Loft.Type, run.Loft.Closed, run.Angle, refit.Tolerance))),
            tangent: static (at, ends) => (Start: Placed(ends.Start), End: Placed(ends.End)) switch {
                var (start, end) => Products.Many(() => Brep.CreateFromLoft(
                    at.Loft.Sections, start.Apex, end.Apex, start.Trim.IsSome, end.Trim.IsSome, start.Trim.ValueUnsafe(), end.Trim.ValueUnsafe(), at.Loft.Type, at.Loft.Closed)),
            });

    // --- [SUBDS]
    public IO<SubD> ToSubD(int divisions, bool corners, bool creases) =>
        Ends is LoftEnds.Fitted { Start.IsNone: true, End.IsNone: true, Fit: CurveFit.AsIs }
            ? from sections in Products.Many(() => Sections.Map(static section => section.ToNurbsCurve()))
              from made in Products.Of(() => SubD.CreateFromLoft(sections, Closed, corners, creases, divisions))
              select made
            : IO.fail<SubD>(new RhinoFault(Ends.GetType().Name, RhinoReason.Unsupported));

    private static (Point3d Apex, Option<BrepTrim> Trim) Placed(Option<LoftEnd> end) =>
        end.Match(
            Some: static held => held.Switch(apex: static apex => (apex, Option<BrepTrim>.None), tangent: static trim => (Point3d.Unset, Some(trim))),
            None: static () => (Point3d.Unset, Option<BrepTrim>.None));
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record DevelopableLoft {
    public sealed record Density(bool Reverse0, bool Reverse1, int Count) : DevelopableLoft;

    public sealed record Rulings(Seq<Point2d> Fixed) : DevelopableLoft;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record NetworkMethod {
    public sealed record AutoSorted(Seq<Curve> Curves, NetworkContinuity Continuity) : NetworkMethod;

    public sealed record Ordered(
        (Seq<Curve> Curves, NetworkContinuity Start, NetworkContinuity End) U,
        (Seq<Curve> Curves, NetworkContinuity Start, NetworkContinuity End) V) : NetworkMethod;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Lofts {
    // --- [DEVELOPABLE]
    public static IO<Seq<Brep>> Developable(Curve first, Curve second, DevelopableLoft loft) =>
        loft.Switch(
            (First: first, Second: second),
            density: static (rails, density) => Products.Many(() => Brep.CreateDevelopableLoft(rails.First, rails.Second, density.Reverse0, density.Reverse1, density.Count)),
            rulings: static (rails, rulings) =>
                from one in Products.Of(rails.First.ToNurbsCurve)
                from two in Products.Of(rails.Second.ToNurbsCurve)
                from lofted in Products.Many(() => Brep.CreateDevelopableLoft(one, two, rulings.Fixed))
                select lofted);

    // --- [NETWORKS]
    public static IO<NurbsSurface> Network(Tolerances tolerances, NetworkMethod method, double interiorTolerance) =>
        method.Switch(
            (Interior: interiorTolerance, Tolerances: tolerances),
            autoSorted: static (at, sorted) => Networked(() =>
                (NurbsSurface.CreateNetworkSurface(sorted.Curves, sorted.Continuity, at.Tolerances.Absolute, at.Interior, at.Tolerances.Angle, out int error), error)),
            ordered: static (at, ordered) => Networked(() => (NurbsSurface.CreateNetworkSurface(
                ordered.U.Curves, ordered.U.Start, ordered.U.End, ordered.V.Curves, ordered.V.Start, ordered.V.End, at.Tolerances.Absolute, at.Interior, at.Tolerances.Angle, out int error), error)));

    private static IO<NurbsSurface> Networked(Func<(NurbsSurface? Surface, int Error)> network, [CallerArgumentExpression(nameof(network))] string call = "") =>
        from made in IO.lift(network)
        from held in Products.Maybe(() => made.Surface, call)
        from surface in IO.lift(() => NetworkFailure.TryGet(made.Error, out NetworkFailure? failure)
            ? new RhinoFault(failure.Message, RhinoReason.Declined)
            : held.ToFin(new RhinoFault(call, RhinoReason.Missing)))
        select surface;

    // --- [PATCHES]
    public static IO<(Brep Patch, Brep.VariationalPatchResult Result)> VariationalPatch(
        Seq<Brep.CurveConstraint> edges, Seq<Brep.CurveConstraint> internalCurves, Seq<Brep.PointConstraint> points, Brep.VariationalPatchSettings settings,
        bool multiThreading, Option<IProgress<double>> progress) =>
        from token in cancelToken
        from patched in IO.lift(() => (Patch: Brep.CreateVariationalPatch(edges, internalCurves, points, settings, multiThreading, token, progress.ValueUnsafe(), out Brep.VariationalPatchResult result), Result: result))
        from held in Products.Maybe(() => patched.Patch)
        from patch in IO.lift(() => Conversions.Unreported(held.IsNone ? patched.Result.Error : null).Bind(_ => Conversions.Required(held)))
        select (patch, patched.Result);
}
