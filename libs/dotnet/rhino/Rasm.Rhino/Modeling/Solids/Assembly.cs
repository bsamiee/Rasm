using System.Globalization;

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
            var rules => new RhinoFault(rules[0].Subject, RhinoReason.Outside, rules[0].At, rules.Tail.IsEmpty ? None : Some(Error.Many(rules.Tail.Map(static Error (rule) => new RhinoFault(rule.Subject, RhinoReason.Outside, rule.At))))),
        };

    private static Seq<(string Subject, int At)> Violations(
        Seq<Surface> surfaces, int curves, Seq<(Point3d Point, Option<double> Tolerance)> vertices,
        Seq<(int Curve, int Start, int End, Option<Interval> Domain, Option<double> Tolerance)> edges, Seq<Face> faces) =>
        (vertices.Map(static (vertex, at) => Seq(
                (Held: Tolerant(vertex.Tolerance), Rule: Rule($"vertex tolerance {vertex.Tolerance}", at))))
            + edges.Map((edge, at) => Seq(
                (Held: Inside(edge.Curve, curves), Rule: Rule($"edge curve {edge.Curve} of {curves} curves", at)),
                (Held: Inside(edge.Start, vertices.Count) && Inside(edge.End, vertices.Count), Rule: Rule($"edge vertices ({edge.Start}, {edge.End}) of {vertices.Count} vertices", at)),
                (Held: edge.Domain.ForAll(static domain => domain.IsIncreasing), Rule: Rule($"edge domain {edge.Domain}", at)),
                (Held: Tolerant(edge.Tolerance), Rule: Rule($"edge tolerance {edge.Tolerance}", at))))
            + faces.Map((face, at) => face.Switch(
                (Surfaces: surfaces, Vertices: vertices.Count, Edges: edges.Count, At: at),
                trimmed: static (of, trimmed) =>
                    Seq((Held: Inside(trimmed.Surface, of.Surfaces.Count), Rule: Rule($"face surface {trimmed.Surface} of {of.Surfaces.Count} surfaces", of.At)))
                    + toSeq(trimmed.Loops).Bind(static loop => toSeq(loop.Trims)).Map(trim => trim.Switch(
                        of,
                        along: static (of, along) => (Held: Inside(along.Edge, of.Edges), Rule: Rule($"face trim edge {along.Edge} of {of.Edges} edges", of.At)),
                        singular: static (of, singular) => (Held: Inside(singular.Vertex, of.Vertices), Rule: Rule($"face singular trim vertex {singular.Vertex} of {of.Vertices} vertices", of.At)))),
                planar: static (of, planar) =>
                    Seq((Held: Inside(planar.Surface, of.Surfaces.Count) && of.Surfaces[planar.Surface] is PlaneSurface,
                        Rule: Rule($"planar face surface {planar.Surface} of {of.Surfaces.Count} surfaces", of.At))))))
        .Bind(static rows => rows)
        .Filter(static row => !row.Held)
        .Map(static row => row.Rule);

    private static (string Subject, int At) Rule(FormattableString subject, int at) => (subject.ToString(CultureInfo.InvariantCulture), at);

    private static bool Inside(int index, int count) => index >= 0 && index < count;

    private static bool Tolerant(Option<double> tolerance) => tolerance.ForAll(static value => double.IsFinite(value) && value >= 0d);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SolidAssembly {
    public static IO<Brep> Build(BrepTopology topology) =>
        from brep in use(static () => new Brep())
        from declared in IO.lift(() => Declared(brep, topology))
        from valid in IO.lift(() => RhinoFault.Unless(brep.IsValidTopology(out string log)).MapFail(error => error + Error.New(log)))
        select brep;

    private static Fin<Unit> Declared(Brep brep, BrepTopology topology) =>
        from surfaces in Callbacks.Each(topology.Surfaces, (surface, at) => Conversions.Found(Conversions.Present(brep.AddSurface(surface)), at))
        from curves in Callbacks.Each(topology.Curves, (curve, at) => Conversions.Found(Conversions.Present(brep.AddEdgeCurve(curve)), at))
        from vertices in Callbacks.Each(topology.Vertices, (vertex, at) => Conversions.Found(brep.Vertices.Add(vertex.Point, Conversions.Unset(vertex.Tolerance)), at))
        from edges in Callbacks.Each(topology.Edges, (edge, at) =>
            Conversions.Found(edge.Domain.Match(
                Some: domain => brep.Edges.Add(vertices[edge.Start], vertices[edge.End], curves[edge.Curve], domain, Conversions.Unset(edge.Tolerance)),
                None: () => brep.Edges.Add(vertices[edge.Start], vertices[edge.End], curves[edge.Curve], Conversions.Unset(edge.Tolerance))), at))
        from trims in Callbacks.Each(topology.Faces, (face, at) => Faced((Brep: brep, Surfaces: surfaces, Vertices: vertices, Edges: edges, At: at), face))
        select Settled(brep, trims.Bind(static held => held));

    private static Fin<Seq<(BrepTrim Trim, Option<IsoStatus> Iso)>> Faced((Brep Brep, Seq<int> Surfaces, Seq<BrepVertex> Vertices, Seq<BrepEdge> Edges, int At) of, BrepTopology.Face face) =>
        face.Switch(
            of,
            trimmed: static (of, trimmed) => Conversions.Found(of.Brep.Faces.Add(of.Surfaces[trimmed.Surface]), of.At).Bind(added => {
                added.OrientationIsReversed = trimmed.Reversed;
                return Callbacks.Each(toSeq(trimmed.Loops), (loop, _) => Conversions.Found(of.Brep.Loops.Add(loop.Type, added), of.At)
                        .Bind(held => Callbacks.Each(toSeq(loop.Trims), (trim, _) => Trimmed((of.Brep, Loop: held, of.Vertices, of.Edges, of.At), trim))))
                    .Map(static loops => loops.Bind(static trims => trims));
            }),
            planar: static (of, planar) =>
                from added in Conversions.Found(of.Brep.Faces.Add(of.Surfaces[planar.Surface]), of.At)
                from loops in Callbacks.Each(toSeq(planar.Loops), (loop, _) => Conversions.Found(of.Brep.Loops.AddPlanarFaceLoop(added.FaceIndex, loop.Type, loop.Boundary), of.At))
                select Seq<(BrepTrim Trim, Option<IsoStatus> Iso)>());

    private static Fin<(BrepTrim Trim, Option<IsoStatus> Iso)> Trimmed((Brep Brep, BrepLoop Loop, Seq<BrepVertex> Vertices, Seq<BrepEdge> Edges, int At) of, BrepTopology.Trim trim) =>
        trim.Switch(
            of,
            along: static (of, along) => Conversions.Found(Conversions.Present(of.Brep.AddTrimCurve(along.Curve)), of.At)
                .Bind(curve => Conversions.Found(of.Brep.Trims.Add(of.Edges[along.Edge], along.Rev3d, of.Loop, curve), of.At))
                .Map(added => (Trim: added, along.Iso)),
            singular: static (of, singular) => Conversions.Found(Conversions.Present(of.Brep.AddTrimCurve(singular.Curve)), of.At)
                .Bind(curve => Conversions.Found(of.Brep.Trims.AddSingularTrim(of.Vertices[singular.Vertex], of.Loop, singular.Iso, curve), of.At))
                .Map(added => (Trim: added, Iso: Some(singular.Iso))));

    private static Unit Settled(Brep brep, Seq<(BrepTrim Trim, Option<IsoStatus> Iso)> trims) {
        brep.SetTrimIsoFlags();
        _ = trims.Iter(static held => held.Iso.Iter(iso => held.Trim.IsoStatus = iso));
        brep.SetTolerancesBoxesAndFlags(
            bLazy: true, bSetVertexTolerances: true, bSetEdgeTolerances: true, bSetTrimTolerances: true,
            bSetTrimIsoFlags: false, bSetTrimTypeFlags: true, bSetLoopTypeFlags: true, bSetTrimBoxes: true);
        return unit;
    }
}
