using QuikGraph;
using Rasm.Rhino.Blocks;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
public abstract record HistoryReplay(int Version) {
    public bool CopyOnReplace { get; init; }

    public bool ReplayOnAttributeChange { get; init; }

    public abstract IO<Unit> Replay(ReplayHistoryData data);
}

public sealed record HistoryDefinition<T>(int Version, HistoryKey<T> Inputs, Func<T, IO<Seq<GeometryBase>>> Build) : HistoryReplay(Version) where T : notnull {
    // --- [RUN]
    public IO<Seq<Guid>> Run(RhinoDoc doc, Command command, T inputs, Option<ObjectAttributes> attributes) =>
        from unique in IO.lift(Conversions.Unique(Inputs.Ids, static id => id))
        from added in (from record in use(() => new HistoryRecord(command, Version) { CopyOnReplaceObject = CopyOnReplace })
                       from built in Build(inputs)
                       from written in Inputs.Write(doc, record, inputs)
                       from placed in IO.lift(() => Products.Each(built, (geometry, index) =>
                           Conversions.Required(Conversions.Present(doc.Objects.Add(geometry, attributes.ValueUnsafe(), record, reference: false)), index)))
                       select placed).Bracket()
        select added;

    // --- [REPLAY]
    public override IO<Unit> Replay(ReplayHistoryData data) =>
        from version in IO.lift(() => RhinoFault.Unless(data.HistoryVersion == Version, RhinoReason.Mismatched, data.HistoryVersion))
        from inputs in Inputs.Read(data)
        from built in Build(inputs)
        from held in IO.lift(() => toSeq(data.Results))
        from appended in Products.Each(toSeq(Range(held.Count, int.Max(built.Count - held.Count, 0))).Map(_ => Products.Of(data.AppendHistoryResult)))
        from trimmed in when(built.Count < held.Count, IO.lift(() => data.UpdateResultArray(held.Take(built.Count)))).As()
        from updated in IO.lift(() => Products.Each(toSeq(data.Results).Zip(built), static (pair, index) => Histories.Updated(pair.First, pair.Second, index)))
        select unit;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Histories {
    // --- [RESULTS]
    internal static Fin<Unit> Updated(ReplayHistoryResult result, GeometryBase geometry, int index) =>
        (geometry switch {
            Point point => Some(result.UpdateToPoint(point.Location, attributes: null)),
            PointCloud cloud => Some(result.UpdateToPointCloud(cloud, attributes: null)),
            TextDot dot => Some(result.UpdateToTextDot(dot, attributes: null)),
            LinearDimension dimension => Some(result.UpdateToLinearDimension(dimension, attributes: null)),
            RadialDimension dimension => Some(result.UpdateToRadialDimension(dimension, attributes: null)),
            AngularDimension dimension => Some(result.UpdateToAngularDimension(dimension, attributes: null)),
            TextEntity text => Some(result.UpdateToText(text, attributes: null)),
            Leader leader => Some(result.UpdateToLeader(leader, attributes: null)),
            Hatch hatch => Some(result.UpdateToHatch(hatch, attributes: null)),
            InstanceReferenceGeometry instance => Some(result.UpdateToInstanceReferenceGeometry(instance, attributes: null)),
            SubD subd => Some(result.UpdateToSubD(subd, attributes: null)),
            Mesh mesh => Some(result.UpdateToMesh(mesh, attributes: null)),
            Extrusion extrusion => Some(result.UpdateToExtrusion(extrusion, attributes: null)),
            Brep brep => Some(result.UpdateToBrep(brep, attributes: null)),
            ClippingPlaneSurface clip => Some(result.UpdateToClippingPlane(new Plane(clip.PointAt(clip.Domain(0).T0, clip.Domain(1).T0), clip.Plane.XAxis, clip.Plane.YAxis), clip.Domain(0).Length, clip.Domain(1).Length, clip.ViewportIds(), attributes: null)),
            Surface surface => Some(result.UpdateToSurface(surface, attributes: null)),
            Curve curve => Some(result.UpdateToCurve(curve, attributes: null)),
            _ => Option<bool>.None,
        }).Match(
            Some: updated => RhinoFault.Unless(updated, RhinoReason.Declined, geometry.ObjectType, index),
            None: () => new RhinoFault(geometry.ObjectType.ToString(), RhinoReason.Unsupported, index));

    // --- [GRAPH]
    public static IO<ArrayBidirectionalGraph<Guid, SEquatableEdge<Guid>>> Graph(RhinoDoc doc, Guid id) =>
        Graphs.Reached(
                id,
                vertex => IO.lift(() => Conversions.Required(doc.Objects.FindId(vertex)).Map(found =>
                    toSeq(found.HistoryParents()).Map(parent => new SEquatableEdge<Guid>(parent, vertex)) + toSeq(found.HistoryChildren()).Map(child => new SEquatableEdge<Guid>(vertex, child)))),
                static edges => edges)
            .Map(static reached => reached.Graph);
}
