using MathNet.Numerics.Interpolation;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Modeling.Curves;
using Rhino;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Alignment {
    private Alignment(Option<(SweepFrame Frame, Vector3d Normal)> host, Func<Curve, Seq<double>, Fin<Seq<Plane>>> frames) => (Host, Frames) = (host, frames);

    public Option<(SweepFrame Frame, Vector3d Normal)> Host { get; }

    public Func<Curve, Seq<double>, Fin<Seq<Plane>>> Frames { get; }

    public sealed record Freeform() : Alignment(Some((SweepFrame.Freeform, Vector3d.Unset)), Minimized);

    public sealed record Frenet() : Alignment(None, static (path, at) =>
        Products.Each(at, (t, index) => RhinoFault.Unless(path.FrameAt(t, out Plane frame), RhinoReason.Declined, t, index).Map(_ => new Plane(frame.Origin, frame.YAxis, frame.ZAxis))));

    public sealed record Roadlike(Vector3d Up) : Alignment(Some((SweepFrame.Roadlike, Up)), (path, at) =>
        Railed(path, nurbs => nurbs.CreatePlanarRailFrames(at, Up)));

    public sealed record Surface(BrepTrim Trim) : Alignment(Some((SweepFrame.AlignWithSurface, Vector3d.Unset)), (path, at) =>
        Products.Each(at, (t, index) =>
            from closest in RhinoFault.Unless(Trim.Face.ClosestPoint(path.PointAt(t), out double u, out double v), RhinoReason.Declined, t, index).Map(_ => (U: u, V: v))
            let normal = Trim.Face.NormalAt(closest.U, closest.V)
            let side = Vector3d.CrossProduct(path.TangentAt(t), normal)
            from crossed in RhinoFault.Unless(side.IsValid && !side.IsTiny(), RhinoReason.Invalid, side, index)
            select new Plane(path.PointAt(t), normal, side)));

    private static Fin<Seq<Plane>> Minimized(Curve path, Seq<double> at) =>
        path.IsClosed && at.Last.Exists(t => t < path.Domain.T1)
            ? from loop in Conversions.Required(path.GetPerpendicularFrames(at.Add(path.Domain.T1))).Map(static frames => toSeq(frames))
              let residual = Vector3d.VectorAngle(loop[^1].XAxis, loop[0].XAxis, new Plane(loop[0].Origin, loop[0].ZAxis))
              let span = path.Domain.Length
              select loop.Init.Map((frame, index) => Transform.Rotation(residual * (at[index] - path.Domain.T0) / span, frame.ZAxis, frame.Origin) switch {
                  var turn => new Plane(frame.Origin, turn * frame.XAxis, turn * frame.YAxis),
              }).Strict()
            : Conversions.Required(path.GetPerpendicularFrames(at)).Map(static frames => toSeq(frames));

    private static Fin<Seq<Plane>> Railed(Curve path, Func<NurbsCurve, Plane[]> frames) {
        using NurbsCurve? converted = path is NurbsCurve ? null : path.ToNurbsCurve();
        return from nurbs in Conversions.Required(path as NurbsCurve ?? converted)
               let planes = frames(nurbs)
               from framed in RhinoFault.Unless(planes.Length > 0, RhinoReason.Declined, planes.Length).Map(_ => toSeq(planes))
               select framed;
    }
}

public sealed record Rail(Curve Path) {
    public Alignment Alignment { get; init; } = new Alignment.Freeform();

    public static Rail Along(BrepTrim trim) => new(trim.Edge) { Alignment = new Alignment.Surface(trim) };

    public static Fin<Rail> Along(BrepEdge edge) =>
        Conversions.Required(toSeq(edge.TrimIndices()).Head, edge.EdgeIndex).Map(trim => Along(edge.Brep.Trims[trim]));
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Spacing {
    public sealed record Count(int Stations) : Spacing;

    public sealed record Distance(double Step) : Spacing {
        public bool FromEnd { get; init; }
    }

    public sealed record Equidistant(double Chord) : Spacing;

    public sealed record Lengths(Seq<double> Normalized) : Spacing;

    public sealed record Parameters(Seq<double> Values) : Spacing;

    public sealed record Inflections : Spacing;

    public sealed record CurvatureMaxima : Spacing;

    public sealed record Extremes(Vector3d Direction) : Spacing;

    public sealed record Discontinuities(Continuity Kind) : Spacing;

    public Fin<Seq<(double Parameter, double Normalized)>> Along(Tolerances tolerances, Curve path) =>
        Switch(
            (Path: path, Tolerances: tolerances),
            count: static (at, count) => Conversions.Required(at.Path.DivideByCount(at.Path.IsClosed ? count.Stations : count.Stations - 1, includeEnds: true)).Map(static found => toSeq(found)),
            distance: static (at, distance) => Conversions.Required(at.Path.DivideByLength(distance.Step, includeEnds: true, distance.FromEnd)).Map(static found => toSeq(found)),
            equidistant: static (at, equidistant) =>
                Conversions.Required(at.Path.DivideEquidistant(equidistant.Chord, out double[] parameters)).Map(_ => toSeq(parameters)),
            lengths: static (at, lengths) =>
                Conversions.Required(at.Path.NormalizedLengthParameters([.. lengths.Normalized], at.Tolerances.Absolute, at.Tolerances.Relative)).Map(static found => toSeq(found)),
            parameters: static (at, parameters) => Products.Each(parameters.Values, (t, index) => RhinoFault.Unless(at.Path.Domain.IncludesParameter(t), RhinoReason.Outside, t, index).Map(_ => t)),
            inflections: static (at, _) => at.Path.InflectionPoints(out double[] parameters) is null ? Seq<double>() : toSeq(parameters),
            curvatureMaxima: static (at, _) => at.Path.MaxCurvaturePoints(out double[] parameters) is null ? Seq<double>() : toSeq(parameters),
            extremes: static (at, extremes) => toSeq(at.Path.ExtremeParameters(extremes.Direction)) switch {
                var found => RhinoFault.Unless(!found.IsEmpty || at.Path.IsPeriodic, RhinoReason.Missing, extremes.Direction).Map(_ => found),
            },
            discontinuities: static (at, discontinuities) => CurveEvaluation.Kinks(at.Tolerances, at.Path, discontinuities.Kind))
        .Map(found => path.GetLength() switch {
            var length => toSeq(found.Distinct().Order()).Map(t => (Parameter: t, Normalized: length > 0d ? path.GetLength(new Interval(path.Domain.T0, t)) / length : 0d)).Strict(),
        });
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Law {
    private Law(Func<double, double> at) => At = at;

    public Func<double, double> At { get; }

    public sealed record Constant(double Value) : Law(_ => Value);

    public sealed record Linear(double Start, double End) : Law(t => Start + ((End - Start) * t));

    public sealed record Function(Func<double, double> Map) : Law(Map);

    public sealed record Through : Law {
        private Through(Seq<(double At, double Value)> keys, IInterpolation spline) : base(spline.Interpolate) => Keys = keys;

        public Seq<(double At, double Value)> Keys { get; }

        public static Fin<Through> Validated(Seq<(double At, double Value)> keys) =>
            toSeq(keys.DistinctBy(static key => key.At).OrderBy(static key => key.At)) switch {
                var sorted => RhinoFault.Unless(sorted.Count >= 2 && sorted.ForAll(static key => double.IsFinite(key.At) && double.IsFinite(key.Value)), RhinoReason.Outside, sorted.Count)
                    .Map(_ => new Through(sorted, CubicSpline.InterpolatePchipSorted([.. sorted.Map(static key => key.At)], [.. sorted.Map(static key => key.Value)]))),
            };
    }
}

public sealed record Placement {
    public Law Twist { get; init; } = new Law.Constant(0d);

    public Law Scale { get; init; } = new Law.Constant(1d);

    public Law OffsetX { get; init; } = new Law.Constant(0d);

    public Law OffsetY { get; init; } = new Law.Constant(0d);

    public Transform At(Plane basis, Plane frame, double t) =>
        Transform.PlaneToPlane(basis, new Plane(frame.Origin + (frame.XAxis * OffsetX.At(t)) + (frame.YAxis * OffsetY.At(t)), frame.XAxis, frame.YAxis))
        * Transform.Rotation(Twist.At(t), basis.ZAxis, basis.Origin)
        * Transform.Scale(basis, Scale.At(t), Scale.At(t), 1d);
}

public sealed record Station(double Parameter, double Normalized, Plane Frame);

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Frames {
    public sealed record Through(Seq<Plane> At) : Frames;

    public sealed record Linear : Frames {
        private Linear(Plane start, Vector3d step, int count) => (Start, Step, Count) = (start, step, count);

        public Plane Start { get; }

        public Vector3d Step { get; }

        public int Count { get; }

        public static Fin<Linear> Validated(Plane start, Vector3d step, int count) => RhinoFault.Unless(count >= 1, RhinoReason.Outside, count).Map(_ => new Linear(start, step, count));
    }

    public sealed record Polar : Frames {
        private Polar(Plane start, Line axis, int count, double sweep) => (Start, Axis, Count, Sweep) = (start, axis, count, sweep);

        public Plane Start { get; }

        public Line Axis { get; }

        public int Count { get; }

        public double Sweep { get; }

        public static Fin<Polar> Validated(Plane start, Line axis, int count, double sweep) =>
            RhinoFault.Unless(count >= 1 && double.IsFinite(sweep), RhinoReason.Outside, (count, sweep)).Map(_ => new Polar(start, axis, count, sweep));
    }

    public sealed record Along(Rail Rail, Spacing Spacing) : Frames;

    public sealed record OnSurface(Surface Surface, Seq<Point2d> At) : Frames;

    // --- [STATIONS]
    public Fin<Seq<Station>> Stations(Tolerances tolerances) =>
        Switch(
            tolerances,
            through: static (_, through) => Indexed(through.At),
            linear: static (_, linear) => Indexed(toSeq(Range(0, linear.Count)).Map(at => linear.Start with { Origin = linear.Start.Origin + (linear.Step * at) }).Strict()),
            polar: static (_, polar) => (Math.Abs(polar.Sweep) >= double.Tau - RhinoMath.ZeroTolerance ? polar.Count : polar.Count - 1) switch {
                var divisions => Products.Each(toSeq(Range(0, polar.Count)), (step, index) => polar.Start switch {
                    var plane => RhinoFault.Unless(plane.Transform(Transform.Rotation(polar.Sweep * step / Math.Max(divisions, 1), polar.Axis.Direction, polar.Axis.From)), RhinoReason.Declined, step, index)
                        .Map(_ => plane),
                }).Map(Indexed),
            },
            along: static (held, along) =>
                from at in along.Spacing.Along(held, along.Rail.Path)
                from frames in along.Rail.Alignment.Frames(along.Rail.Path, at.Map(static row => row.Parameter))
                select at.Zip(frames, static (row, frame) => new Station(row.Parameter, row.Normalized, frame)).Strict(),
            onSurface: static (_, onSurface) =>
                Products.Each(onSurface.At, (uv, index) => RhinoFault.Unless(onSurface.Surface.FrameAt(uv.X, uv.Y, out Plane frame), RhinoReason.Declined, uv, index).Map(_ => frame)).Map(Indexed));

    public Fin<Seq<Plane>> Planes(Tolerances tolerances) => Stations(tolerances).Map(static stations => stations.Map(static station => station.Frame).Strict());

    // --- [PLACEMENTS]
    public Fin<Seq<(Station Station, Transform Xform)>> Placements(Tolerances tolerances, Plane basis, Placement placement) =>
        Stations(tolerances).Map(stations => stations.Map(station => (Station: station, Xform: placement.At(basis, station.Frame, station.Normalized))).Strict());

    public IO<Seq<(Station Station, T Copy)>> Arrayed<T>(Tolerances tolerances, T geometry, Plane basis, Placement placement) where T : GeometryBase =>
        IO.lift(() => Placements(tolerances, basis, placement)).Bind(placed => placed.TraverseM(row =>
            from copy in Products.Of(() => geometry.Duplicate() as T)
            from moved in Transformations.Transformed(copy, row.Xform)
            select (row.Station, Copy: moved)).As());

    private static Seq<Station> Indexed(Seq<Plane> planes) =>
        planes.Map((plane, index) => new Station(index, planes.Count > 1 ? index / (planes.Count - 1d) : 0d, plane)).Strict();
}
