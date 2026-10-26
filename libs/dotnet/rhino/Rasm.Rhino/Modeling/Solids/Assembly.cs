using System.Globalization;
using Rhino.Geometry.Collections;

namespace Rasm.Rhino.Modeling.Solids;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<RhinoFault>]
public sealed partial class BrepTopology {
    public Seq<Surface> Surfaces { get; }

    public Seq<Curve> Curves { get; }

    public Seq<(Point3d Point, Option<double> Tolerance)> Vertices { get; }

    public Seq<(int Curve, int Start, int End, Option<Interval> Domain, Option<double> Tolerance)> Edges { get; }

    public Seq<Face> Faces { get; }

    [Union(MapMethods = SwitchMapMethodsGeneration.None)]
    public abstract partial record Face {
        public sealed record Trimmed(int Surface, bool Reversed, IterableNE<Loop> Loops) : Face;

        public sealed record Planar(int Surface, IterableNE<(BrepLoopType Type, IterableNE<Curve> Boundary)> Loops) : Face;
    }

    public sealed record Loop(BrepLoopType Type, IterableNE<Trim> Trims);

    [Union(MapMethods = SwitchMapMethodsGeneration.None)]
    public abstract partial record Trim {
        public sealed record Along(int Edge, bool Rev3d, Curve Curve, Option<IsoStatus> Iso) : Trim;

        public sealed record Singular(int Vertex, Curve Curve, IsoStatus Iso) : Trim;
    }

    static partial void ValidateFactoryArguments(
        ref RhinoFault? validationError, ref Seq<Surface> surfaces, ref Seq<Curve> curves, ref Seq<(Point3d Point, Option<double> Tolerance)> vertices,
        ref Seq<(int Curve, int Start, int End, Option<Interval> Domain, Option<double> Tolerance)> edges, ref Seq<Face> faces) =>
        validationError = Violations(surfaces, curves.Count, vertices, edges, faces) switch {
            { IsEmpty: true } => null,
            var rules => new RhinoFault(string.Join("; ", rules)),
        };

    private static Seq<string> Violations(
        Seq<Surface> surfaces, int curves, Seq<(Point3d Point, Option<double> Tolerance)> vertices,
        Seq<(int Curve, int Start, int End, Option<Interval> Domain, Option<double> Tolerance)> edges, Seq<Face> faces) =>
        (vertices.Map(static (vertex, at) => Seq(
                (Held: Tolerant(vertex.Tolerance), Rule: string.Create(CultureInfo.InvariantCulture, $"vertex {at} tolerance {vertex.Tolerance} is negative or not finite"))))
            + edges.Map((edge, at) => Seq(
                (Held: Inside(edge.Curve, curves), Rule: string.Create(CultureInfo.InvariantCulture, $"edge {at} curve {edge.Curve} lies outside {curves} curves")),
                (Held: Inside(edge.Start, vertices.Count) && Inside(edge.End, vertices.Count),
                    Rule: string.Create(CultureInfo.InvariantCulture, $"edge {at} vertices ({edge.Start}, {edge.End}) lie outside {vertices.Count} vertices")),
                (Held: edge.Domain.ForAll(static domain => domain.IsIncreasing), Rule: string.Create(CultureInfo.InvariantCulture, $"edge {at} domain {edge.Domain} is not increasing")),
                (Held: Tolerant(edge.Tolerance), Rule: string.Create(CultureInfo.InvariantCulture, $"edge {at} tolerance {edge.Tolerance} is negative or not finite"))))
            + faces.Map((face, at) => face.Switch(
                (Surfaces: surfaces, Vertices: vertices.Count, Edges: edges.Count, At: at),
                trimmed: static (of, trimmed) =>
                    Seq((Held: Inside(trimmed.Surface, of.Surfaces.Count),
                        Rule: string.Create(CultureInfo.InvariantCulture, $"face {of.At} surface {trimmed.Surface} lies outside {of.Surfaces.Count} surfaces")))
                    + toSeq(trimmed.Loops).Bind(static loop => toSeq(loop.Trims)).Map(trim => trim.Switch(
                        of,
                        along: static (of, along) => (Held: Inside(along.Edge, of.Edges),
                            Rule: string.Create(CultureInfo.InvariantCulture, $"face {of.At} trim edge {along.Edge} lies outside {of.Edges} edges")),
                        singular: static (of, singular) => (Held: Inside(singular.Vertex, of.Vertices),
                            Rule: string.Create(CultureInfo.InvariantCulture, $"face {of.At} singular trim vertex {singular.Vertex} lies outside {of.Vertices} vertices")))),
                planar: static (of, planar) =>
                    Seq((Held: Inside(planar.Surface, of.Surfaces.Count) && of.Surfaces[planar.Surface] is PlaneSurface,
                        Rule: string.Create(CultureInfo.InvariantCulture, $"planar face {of.At} surface {planar.Surface} names no PlaneSurface of {of.Surfaces.Count} surfaces"))))))
        .Bind(static rows => rows)
        .Filter(static row => !row.Held)
        .Map(static row => row.Rule);

    private static bool Inside(int index, int count) => index >= 0 && index < count;

    private static bool Tolerant(Option<double> tolerance) => tolerance.ForAll(static value => double.IsFinite(value) && value >= 0d);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SolidAssembly {
    public static IO<Brep> Build(BrepTopology topology, Action<IDisposable> retain) =>
        Copies.Owned(static () => new Brep(), brep =>
            from declared in IO.lift(() => Declared(brep, topology))
            from valid in Copies.Acquire(nameof(Brep.IsValidTopology), _ => Answer.Accepted(brep.IsValidTopology(out string log), log), retain)
            select brep, retain);

    private static Fin<Unit> Declared(Brep brep, BrepTopology topology) =>
        from surfaces in Callbacks.Each(topology.Surfaces, (surface, at) => Indexed(brep.AddSurface(surface), nameof(Brep.AddSurface), at))
        from curves in Callbacks.Each(topology.Curves, (curve, at) => Indexed(brep.AddEdgeCurve(curve), nameof(Brep.AddEdgeCurve), at))
        from vertices in Callbacks.Each(topology.Vertices, (vertex, at) =>
            Added(brep.Vertices.Add(vertex.Point, Conversions.Unset(vertex.Tolerance)), nameof(BrepVertexList.Add), at))
        from edges in Callbacks.Each(topology.Edges, (edge, at) =>
            Added(brep.Edges.Add(vertices[edge.Start], vertices[edge.End], curves[edge.Curve], edge.Domain.IfNone(Interval.Unset), Conversions.Unset(edge.Tolerance)), nameof(BrepEdgeList.Add), at))
        from trims in Callbacks.Each(topology.Faces, (face, at) => Faced((Brep: brep, Surfaces: surfaces, Vertices: vertices, Edges: edges, At: at), face))
        select Settled(brep, trims.Bind(static held => held));

    private static Fin<Seq<(BrepTrim Trim, Option<IsoStatus> Iso)>> Faced((Brep Brep, Seq<int> Surfaces, Seq<BrepVertex> Vertices, Seq<BrepEdge> Edges, int At) of, BrepTopology.Face face) =>
        face.Switch(
            of,
            trimmed: static (of, trimmed) => Added(of.Brep.Faces.Add(of.Surfaces[trimmed.Surface]), nameof(BrepFaceList.Add), of.At).Bind(added => {
                added.OrientationIsReversed = trimmed.Reversed;
                return Callbacks.Each(toSeq(trimmed.Loops), (loop, _) => Added(of.Brep.Loops.Add(loop.Type, added), nameof(BrepLoopList.Add), of.At)
                        .Bind(held => Callbacks.Each(toSeq(loop.Trims), (trim, _) => Trimmed((of.Brep, Loop: held, of.Vertices, of.Edges, of.At), trim))))
                    .Map(static loops => loops.Bind(static trims => trims));
            }),
            planar: static (of, planar) => Added(of.Brep.Faces.Add(of.Surfaces[planar.Surface]), nameof(BrepFaceList.Add), of.At).Bind(added =>
                Callbacks.Each(toSeq(planar.Loops), (loop, _) =>
                    Added(of.Brep.Loops.AddPlanarFaceLoop(added.FaceIndex, loop.Type, loop.Boundary), nameof(BrepLoopList.AddPlanarFaceLoop), of.At))
                .Map(static _ => Seq<(BrepTrim Trim, Option<IsoStatus> Iso)>())));

    private static Fin<(BrepTrim Trim, Option<IsoStatus> Iso)> Trimmed((Brep Brep, BrepLoop Loop, Seq<BrepVertex> Vertices, Seq<BrepEdge> Edges, int At) of, BrepTopology.Trim trim) =>
        trim.Switch(
            of,
            along: static (of, along) => Indexed(of.Brep.AddTrimCurve(along.Curve), nameof(Brep.AddTrimCurve), of.At)
                .Bind(curve => Added(of.Brep.Trims.Add(of.Edges[along.Edge], along.Rev3d, of.Loop, curve), nameof(BrepTrimList.Add), of.At))
                .Map(added => (Trim: added, along.Iso)),
            singular: static (of, singular) => Indexed(of.Brep.AddTrimCurve(singular.Curve), nameof(Brep.AddTrimCurve), of.At)
                .Bind(curve => Added(of.Brep.Trims.AddSingularTrim(of.Vertices[singular.Vertex], of.Loop, singular.Iso, curve), nameof(BrepTrimList.AddSingularTrim), of.At))
                .Map(added => (Trim: added, Iso: Some(singular.Iso))));

    private static Unit Settled(Brep brep, Seq<(BrepTrim Trim, Option<IsoStatus> Iso)> trims) {
        brep.SetTrimIsoFlags();
        _ = trims.Iter(static held => held.Iso.Iter(iso => held.Trim.IsoStatus = iso));
        brep.SetTolerancesBoxesAndFlags(
            bLazy: true, bSetVertexTolerances: true, bSetEdgeTolerances: true, bSetTrimTolerances: true,
            bSetTrimIsoFlags: false, bSetTrimTypeFlags: true, bSetLoopTypeFlags: true, bSetTrimBoxes: true);
        return unit;
    }

    private static Fin<int> Indexed(int index, string member, int at) => Refused.Unless(index >= 0, index, member, at);

    private static Fin<T> Added<T>(T? added, string member, int at) where T : class => added is { } held ? held : new Refused(member, Refused.Absent, at);
}
