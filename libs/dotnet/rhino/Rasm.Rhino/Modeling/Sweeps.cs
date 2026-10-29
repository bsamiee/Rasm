using System.Collections.Generic;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Sweep {
    public bool Closed { get; init; }

    public CurveFit Fit { get; init; } = new CurveFit.AsIs();

    public sealed record Unplaced(Rail Rail, Seq<Curve> Shapes) : Sweep {
        public SweepBlend Blend { get; init; } = SweepBlend.Local;

        public SweepMiter Miter { get; init; } = SweepMiter.None;

        public Option<Point3d> Start { get; init; }

        public Option<Point3d> End { get; init; }

        public bool RefitRail { get; init; } = true;
    }

    public sealed record Placed(Rail Rail, Seq<(Curve Shape, double At)> Shapes) : Sweep {
        public SweepBlend Blend { get; init; } = SweepBlend.Local;

        public SweepMiter Miter { get; init; } = SweepMiter.None;
    }

    public sealed record Driven(Rail Rail, Curve Shape, Plane Basis, Spacing Spacing) : Sweep {
        public Placement Placement { get; init; } = new();

        public SweepBlend Blend { get; init; } = SweepBlend.Local;

        public SweepMiter Miter { get; init; } = SweepMiter.None;
    }

    public sealed record Rails(Curve First, Curve Second, Seq<Curve> Shapes) : Sweep {
        public bool MaintainHeight { get; init; }

        public bool AutoAdjust { get; init; } = true;

        public Option<Point3d> Start { get; init; }

        public Option<Point3d> End { get; init; }
    }

    public sealed record RailsPlaced(Curve First, Curve Second, Seq<(Curve Shape, double First, double Second)> Shapes) : Sweep {
        public bool MaintainHeight { get; init; }
    }

    public sealed record Pipe(Curve Rail, Law Radius, Spacing Spacing) : Sweep {
        public Option<Law> Inner { get; init; }

        public PipeCapMode Cap { get; init; } = PipeCapMode.Round;

        public bool LocalBlending { get; init; }

        public bool FitRail { get; init; }
    }

    // --- [BREPS]
    public IO<Seq<Brep>> ToBreps(Tolerances tolerances) =>
        Switch(
            (Sweep: this, Tolerances: tolerances),
            unplaced: static (at, sweep) => Hosted(at, sweep.Rail, sweep.Shapes, sweep.Start, sweep.End, sweep.Blend, sweep.Miter, sweep.RefitRail),
            placed: static (at, sweep) => OneRail(at, sweep.Rail, sweep.Blend, sweep.Miter, sweep.Shapes),
            driven: static (at, sweep) => Positioned(at.Tolerances, sweep).Bind(shapes => OneRail(at, sweep.Rail, sweep.Blend, sweep.Miter, shapes)),
            rails: static (at, sweep) => at.Sweep.Fit.Parameters switch {
                var fit => Products.Many(() => Brep.CreateFromSweep(
                    sweep.First, sweep.Second, sweep.Shapes, Conversions.Unset(sweep.Start), Conversions.Unset(sweep.End), at.Sweep.Closed, at.Tolerances.Absolute,
                    fit.RebuildType, fit.RebuildPointCount, fit.RefitTolerance, sweep.MaintainHeight, sweep.AutoAdjust)),
            },
            railsPlaced: static (at, sweep) =>
                from sweeper in IO.lift(() => new SweepTwoRail {
                    SweepTolerance = at.Tolerances.Absolute, AngleToleranceRadians = at.Tolerances.Angle, ClosedSweep = at.Sweep.Closed, MaintainHeight = sweep.MaintainHeight, UseLegacySweeper = true,
                })
                let shapes = sweep.Shapes.Map(static row => row.Shape)
                let first = sweep.Shapes.Map(static row => row.First)
                let second = sweep.Shapes.Map(static row => row.Second)
                from swept in at.Sweep.Fit.Switch(
                    (Sweeper: sweeper, Sweep: sweep, Shapes: shapes, First: first, Second: second),
                    asIs: static (run, _) => Products.Many(() => run.Sweeper.PerformSweep(run.Sweep.First, run.Sweep.Second, run.Shapes, run.First, run.Second)),
                    rebuild: static (run, rebuild) => Products.Many(() => run.Sweeper.PerformSweepRebuild(run.Sweep.First, run.Sweep.Second, run.Shapes, run.First, run.Second, rebuild.Count)),
                    refit: static (run, refit) => Products.Many(() => run.Sweeper.PerformSweepRefit(run.Sweep.First, run.Sweep.Second, run.Shapes, run.First, run.Second, refit.Tolerance)))
                select swept,
            pipe: static (at, sweep) =>
                from sampled in IO.lift(() => sweep.Spacing.Along(at.Tolerances, sweep.Rail))
                let parameters = sampled.Map(static row => row.Parameter)
                let outer = sampled.Map(row => sweep.Radius.At(row.Normalized))
                from piped in sweep.Inner.Match(
                    Some: inner => Products.Many(() => Brep.CreateThickPipe(
                        sweep.Rail, parameters, sampled.Map(row => inner.At(row.Normalized)), outer, sweep.LocalBlending, sweep.Cap, sweep.FitRail, at.Tolerances.Absolute, at.Tolerances.Angle)),
                    None: () => Products.Many(() => Brep.CreatePipe(sweep.Rail, parameters, outer, sweep.LocalBlending, sweep.Cap, sweep.FitRail, at.Tolerances.Absolute, at.Tolerances.Angle)))
                select piped);

    // --- [SUBDS]
    public IO<SubD> ToSubD(Tolerances tolerances, bool corners) =>
        Switch(
            (Sweep: this, Tolerances: tolerances, Corners: corners),
            unplaced: static (at, sweep) => Railed(sweep.Rail, sweep.Shapes, at.Sweep.Closed, at.Corners),
            placed: static (at, sweep) => Railed(sweep.Rail, sweep.Shapes.Map(static row => row.Shape), at.Sweep.Closed, at.Corners),
            driven: static (at, sweep) => Positioned(at.Tolerances, sweep).Bind(shapes => Railed(sweep.Rail, shapes.Map(static row => row.Shape), at.Sweep.Closed, at.Corners)),
            rails: static (at, sweep) => Paired(sweep.First, sweep.Second, sweep.Shapes, at.Sweep.Closed, at.Corners),
            railsPlaced: static (at, sweep) => Paired(sweep.First, sweep.Second, sweep.Shapes.Map(static row => row.Shape), at.Sweep.Closed, at.Corners),
            pipe: static (_, _) => IO.fail<SubD>(new RhinoFault(nameof(Pipe), RhinoReason.Unsupported)));

    // --- [LOWERINGS]
    private static IO<Seq<Brep>> Hosted((Sweep Sweep, Tolerances Tolerances) at, Rail rail, Seq<Curve> shapes, Option<Point3d> start, Option<Point3d> end, SweepBlend blend, SweepMiter miter, bool refitRail) =>
        from host in IO.lift(() => rail.Alignment.Host.ToFin(new RhinoFault(rail.Alignment.GetType().Name, RhinoReason.Unsupported)))
        let fit = at.Sweep.Fit.Parameters
        from swept in Products.Many(() => Brep.CreateFromSweep(
            rail.Alignment is Alignment.Surface surface ? surface.Trim : rail.Path, shapes, Conversions.Unset(start), Conversions.Unset(end),
            host.Frame, host.Normal, at.Sweep.Closed, blend, miter, at.Tolerances.Absolute, fit.RebuildType, fit.RebuildPointCount, fit.RefitTolerance, refitRail))
        select swept;

    private static IO<Seq<Brep>> OneRail((Sweep Sweep, Tolerances Tolerances) at, Rail rail, SweepBlend blend, SweepMiter miter, Seq<(Curve Shape, double At)> stations) =>
        rail.Alignment is Alignment.Surface
            ? Hosted(at, rail, toSeq(stations.OrderBy(static row => row.At)).Map(static row => row.Shape), None, None, blend, miter, refitRail: true)
            : Swept(at, rail, blend, miter, stations);

    private static IO<Seq<Brep>> Swept((Sweep Sweep, Tolerances Tolerances) at, Rail rail, SweepBlend blend, SweepMiter miter, Seq<(Curve Shape, double At)> stations) =>
        from host in IO.lift(() => Framed(rail))
        from sweeper in IO.lift(() => {
            SweepOneRail made = new() {
                SweepTolerance = at.Tolerances.Absolute, AngleToleranceRadians = at.Tolerances.Angle, ClosedSweep = at.Sweep.Closed, GlobalShapeBlending = blend is SweepBlend.Global, MiterType = (int)miter,
            };
            if (host.Frame is SweepFrame.Roadlike)
                made.SetRoadlikeUpDirection(host.Normal);
            return made;
        })
        let ordered = toSeq(stations.OrderBy(static row => row.At))
        let shapes = ordered.Map(static row => row.Shape)
        let parameters = ordered.Map(static row => row.At)
        from swept in at.Sweep.Fit.Switch(
            (Sweeper: sweeper, rail.Path, Shapes: shapes, At: parameters),
            asIs: static (run, _) => Products.Many(() => run.Sweeper.PerformSweep(run.Path, run.Shapes, run.At)),
            rebuild: static (run, rebuild) => Products.Many(() => run.Sweeper.PerformSweepRebuild(run.Path, run.Shapes, run.At, rebuild.Count)),
            refit: static (run, refit) => Products.Many(() => run.Sweeper.PerformSweepRefit(run.Path, run.Shapes, run.At, refit.Tolerance)))
        select swept;

    private static IO<Seq<(Curve Shape, double At)>> Positioned(Tolerances tolerances, Driven sweep) =>
        IO.lift(() => new Frames.Along(sweep.Rail, sweep.Spacing).Placements(tolerances, sweep.Basis, sweep.Placement)).Bind(placed => placed.TraverseM(row =>
            from copy in Products.Of(sweep.Shape.DuplicateCurve)
            from moved in Products.Accepted(() => copy.Transform(row.Xform))
            select (Shape: copy, At: row.Station.Parameter)).As());

    private static IO<SubD> Railed(Rail rail, Seq<Curve> shapes, bool closed, bool corners) =>
        from host in IO.lift(() => Framed(rail))
        from path in Products.Of(rail.Path.ToNurbsCurve)
        from sections in Nurbs(shapes)
        from made in Products.Of(() => SubD.CreateFromSweep(path, sections, closed, corners, host.Frame is SweepFrame.Roadlike, host.Normal))
        select made;

    private static IO<SubD> Paired(Curve first, Curve second, Seq<Curve> shapes, bool closed, bool corners) =>
        from one in Products.Of(first.ToNurbsCurve)
        from two in Products.Of(second.ToNurbsCurve)
        from sections in Nurbs(shapes)
        from made in Products.Of(() => SubD.CreateFromSweep(one, two, sections, closed, corners))
        select made;

    private static Fin<(SweepFrame Frame, Vector3d Normal)> Framed(Rail rail) =>
        rail.Alignment.Host.Filter(static held => held.Frame is not SweepFrame.AlignWithSurface).ToFin(new RhinoFault(rail.Alignment.GetType().Name, RhinoReason.Unsupported));

    private static IO<Seq<NurbsCurve>> Nurbs(IEnumerable<Curve> shapes) => Products.Many(() => shapes.Select(static shape => shape.ToNurbsCurve()));
}
