using System.Collections.Generic;
using System.Drawing;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Condensation;
using QuikGraph.Algorithms.Search;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record HistoryInput(int Id, Func<RhinoDoc, HistoryRecord, IO<Unit>> Write);

public sealed class HistoryKey<T>(int id, Func<RhinoDoc, HistoryRecord, int, T, IO<Unit>> write, Func<ReplayHistoryData, int, IO<T>> read) where T : notnull {
    public HistoryInput Input(T value) => new(id, (doc, record) => write(doc, record, id, value));

    public Func<ReplayHistoryData, IO<T>> Read { get; } = lpar(read, id);
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record HistoryOutput {
    public sealed record Kept(ReplayHistoryResult Existing) : HistoryOutput;

    public sealed record Changed(Option<ReplayHistoryResult> Existing, Func<ReplayHistoryResult, Fin<Unit>> Update) : HistoryOutput;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Histories {
    // --- [KEYS]
    public static HistoryKey<bool> Bool(int id) =>
        Plain(id, static (record, slot, value) => RhinoFault.Unless(record.SetBool(slot, value), slot),
            static (data, slot) => RhinoFault.Unless(data.TryGetBool(slot, out bool value), slot).Map(_ => value));

    public static HistoryKey<int> Int(int id) =>
        Plain(id, static (record, slot, value) => RhinoFault.Unless(record.SetInt(slot, value), slot),
            static (data, slot) => RhinoFault.Unless(data.TryGetInt(slot, out int value), slot).Map(_ => value));

    public static HistoryKey<Seq<int>> Ints(int id) =>
        Plain(id, static (record, slot, values) => RhinoFault.Unless(record.SetInts(slot, values), slot),
            static (data, slot) => RhinoFault.Unless(data.TryGetInts(slot, out int[] values), slot).Map(_ => toSeq(values)));

    public static HistoryKey<double> Double(int id) =>
        Plain(id, static (record, slot, value) => RhinoFault.Unless(record.SetDouble(slot, value), slot),
            static (data, slot) => RhinoFault.Unless(data.TryGetDouble(slot, out double value), slot).Map(_ => value));

    public static HistoryKey<Seq<double>> Doubles(int id) =>
        Plain(id, static (record, slot, values) => RhinoFault.Unless(record.SetDoubles(slot, values), slot),
            static (data, slot) => RhinoFault.Unless(data.TryGetDoubles(slot, out double[] values), slot).Map(_ => toSeq(values)));

    public static HistoryKey<Point3d> Point3d(int id) =>
        Plain(id, static (record, slot, value) => RhinoFault.Unless(record.SetPoint3d(slot, value), slot),
            static (data, slot) => RhinoFault.Unless(data.TryGetPoint3d(slot, out Point3d value), slot).Map(_ => value));

    public static HistoryKey<Vector3d> Vector3d(int id) =>
        Plain(id, static (record, slot, value) => RhinoFault.Unless(record.SetVector3d(slot, value), slot),
            static (data, slot) => RhinoFault.Unless(data.TryGetVector3d(slot, out Vector3d value), slot).Map(_ => value));

    public static HistoryKey<Transform> Transform(int id) =>
        Plain(id, static (record, slot, value) => RhinoFault.Unless(record.SetTransorm(slot, value), slot),
            static (data, slot) => RhinoFault.Unless(data.TryGetTransform(slot, out Transform value), slot).Map(_ => value));

    public static HistoryKey<Color> Color(int id) =>
        Plain(id, static (record, slot, value) => RhinoFault.Unless(record.SetColor(slot, value), slot),
            static (data, slot) => RhinoFault.Unless(data.TryGetColor(slot, out Color value), slot).Map(_ => value));

    public static HistoryKey<Guid> Guid(int id) =>
        Plain(id, static (record, slot, value) => RhinoFault.Unless(record.SetGuid(slot, value), slot),
            static (data, slot) => RhinoFault.Unless(data.TryGetGuid(slot, out Guid value), slot).Map(_ => value));

    public static HistoryKey<Seq<Guid>> Guids(int id) =>
        Plain(id, static (record, slot, values) => RhinoFault.Unless(record.SetGuids(slot, values), slot),
            static (data, slot) => RhinoFault.Unless(data.TryGetGuids(slot, out Guid[] values), slot).Map(_ => toSeq(values)));

    public static HistoryKey<string> String(int id) =>
        Plain(id, static (record, slot, value) => RhinoFault.Unless(record.SetString(slot, value), slot),
            static (data, slot) => RhinoFault.Unless(data.TryGetString(slot, out string value), slot).Map(_ => value));

    public static HistoryKey<PickCapture> ObjRef(int id) =>
        new(id,
            static (doc, record, slot, target) => Referenced(doc, target, reference => RhinoFault.Unless(record.SetObjRef(slot, reference), slot)),
            Captured);

    public static HistoryKey<(PickCapture Target, Point3d Point)> Point3dOnObject(int id) =>
        new(id,
            static (doc, record, slot, value) => Referenced(doc, value.Target, reference => RhinoFault.Unless(record.SetPoint3dOnObject(slot, reference, value.Point), slot)),
            static (data, slot) => Captured(data, slot).Zip(IO.lift(() => RhinoFault.Unless(data.TryGetPoint3dOnObject(slot, out Point3d point), slot).Map(_ => point))).As());

    private static HistoryKey<T> Plain<T>(int id, Func<HistoryRecord, int, T, Fin<Unit>> write, Func<ReplayHistoryData, int, Fin<T>> read) where T : notnull =>
        new(id,
            (_, record, slot, value) => IO.lift(() => write(record, slot, value)),
            (data, slot) => IO.lift(() => read(data, slot)));

    private static IO<Unit> Referenced(RhinoDoc doc, PickCapture target, Func<ObjRef, Fin<Unit>> write) =>
        (from reference in use(() => new ObjRef(doc, target.ObjectId, Conversions.Unset(target.GeometryComponentIndex)))
         from written in IO.lift(() => write(reference))
         select written).Bracket();

    private static IO<PickCapture> Captured(ReplayHistoryData data, int slot) =>
        Products.Made(data.GetRhinoObjRef(slot)).Bind(PickCapture.Of).Bracket();

    // --- [RECORDS]
    public static IO<T> Record<T>(RhinoDoc doc, Command command, int version, bool copyOnReplace, Seq<HistoryInput> inputs, Func<HistoryRecord, IO<T>> body) =>
        from unique in IO.lift(Conversions.Unique(inputs, static input => input.Id))
        from result in (from record in use(() => new HistoryRecord(command, version) { CopyOnReplaceObject = copyOnReplace })
                        from written in unique.TraverseM(input => input.Write(doc, record)).As()
                        from answer in body(record)
                        select answer).Bracket()
        select result;

    // --- [REPLAY]
    public static IO<Unit> Replay(ReplayHistoryData data, int version, Func<Seq<ReplayHistoryResult>, IO<Seq<HistoryOutput>>> outputs) =>
        from results in IO.lift(() => RhinoFault.Unless(data.HistoryVersion == version).Map(_ => toSeq(data.Results)))
        from rows in outputs(results)
        let continued = rows.Map(static row => row.Switch(kept: static kept => kept.Existing, changed: static changed => changed.Existing.ValueUnsafe()))
        from arranged in IO.lift(() => data.UpdateResultArray(continued))
        let changed = toSeq(data.Results).Zip(rows).Choose(static pair => pair.Second is HistoryOutput.Changed change ? Some((pair.First, change.Update)) : None)
        from updated in IO.lift(() => Callbacks.Each(changed, static (pair, _) => pair.Update(pair.First)))
        select unit;

    // --- [TOPOLOGY]
    public static IO<ArrayBidirectionalGraph<Guid, SEquatableEdge<Guid>>> Web(RhinoDoc doc, Guid id) =>
        from table in IO.lift(() => doc.Objects)
        let links = memoUnsafe((Guid vertex) => Optional(table.FindId(vertex)).Map(static found => (Parents: toSeq(found.HistoryParents()).Strict(), Children: toSeq(found.HistoryChildren()).Strict())))
        from origin in IO.lift(() => Conversions.Found(links(id)))
        let joined = new Func<Guid, IEnumerable<SEquatableEdge<Guid>>>(vertex => links(vertex).ToSeq().Bind(static found => found.Parents.Concat(found.Children)).Map(next => new SEquatableEdge<Guid>(vertex, next)))
        let reach = new ImplicitDepthFirstSearchAlgorithm<Guid, SEquatableEdge<Guid>>(joined.ToDelegateIncidenceGraph())
        from visited in IO.lift(() => reach.Compute(id))
        select reach.VerticesColors.Keys.ToBidirectionalGraph(vertex => links(vertex).ToSeq().Bind(static found => found.Children).Map(child => new SEquatableEdge<Guid>(vertex, child)), allowParallelEdges: false).ToArrayBidirectionalGraph();

    extension(IBidirectionalGraph<AdjacencyGraph<Guid, SEquatableEdge<Guid>>, CondensedEdge<Guid, SEquatableEdge<Guid>, AdjacencyGraph<Guid, SEquatableEdge<Guid>>>> graph) {
        public Seq<Guid> UpdateOrder => toSeq(graph.SourceFirstBidirectionalTopologicalSort()).Bind(static component => toSeq(component.Vertices)).Strict();

        public Seq<Seq<Guid>> CycleGroups => toSeq(graph.Vertices).Filter(static component => component.EdgeCount > 0).Map(static component => toSeq(component.Vertices).Strict()).Strict();
    }
}
