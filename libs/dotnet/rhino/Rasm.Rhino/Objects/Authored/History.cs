using System.Drawing;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Condensation;
using QuikGraph.Algorithms.Search;
using QuikGraph.Algorithms.TopologicalSort;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Objects.Authored;

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
        Plain(id, nameof(HistoryRecord.SetBool), static (record, slot, value) => record.SetBool(slot, value), static (data, slot) => MissingHistoryInput.Unless(data.TryGetBool(slot, out bool value), value, slot));

    public static HistoryKey<int> Int(int id) =>
        Plain(id, nameof(HistoryRecord.SetInt), static (record, slot, value) => record.SetInt(slot, value), static (data, slot) => MissingHistoryInput.Unless(data.TryGetInt(slot, out int value), value, slot));

    public static HistoryKey<Seq<int>> Ints(int id) =>
        Plain(id, nameof(HistoryRecord.SetInts), static (record, slot, values) => record.SetInts(slot, values), static (data, slot) => MissingHistoryInput.Unless(data.TryGetInts(slot, out int[] values), values, slot).Map(static found => toSeq(found)));

    public static HistoryKey<double> Double(int id) =>
        Plain(id, nameof(HistoryRecord.SetDouble), static (record, slot, value) => record.SetDouble(slot, value), static (data, slot) => MissingHistoryInput.Unless(data.TryGetDouble(slot, out double value), value, slot));

    public static HistoryKey<Seq<double>> Doubles(int id) =>
        Plain(id, nameof(HistoryRecord.SetDoubles), static (record, slot, values) => record.SetDoubles(slot, values), static (data, slot) => MissingHistoryInput.Unless(data.TryGetDoubles(slot, out double[] values), values, slot).Map(static found => toSeq(found)));

    public static HistoryKey<Point3d> Point3d(int id) =>
        Plain(id, nameof(HistoryRecord.SetPoint3d), static (record, slot, value) => record.SetPoint3d(slot, value), static (data, slot) => MissingHistoryInput.Unless(data.TryGetPoint3d(slot, out Point3d value), value, slot));

    public static HistoryKey<Vector3d> Vector3d(int id) =>
        Plain(id, nameof(HistoryRecord.SetVector3d), static (record, slot, value) => record.SetVector3d(slot, value), static (data, slot) => MissingHistoryInput.Unless(data.TryGetVector3d(slot, out Vector3d value), value, slot));

    public static HistoryKey<Transform> Transform(int id) =>
        Plain(id, nameof(HistoryRecord.SetTransorm), static (record, slot, value) => record.SetTransorm(slot, value), static (data, slot) => MissingHistoryInput.Unless(data.TryGetTransform(slot, out Transform value), value, slot));

    public static HistoryKey<Color> Color(int id) =>
        Plain(id, nameof(HistoryRecord.SetColor), static (record, slot, value) => record.SetColor(slot, value), static (data, slot) => MissingHistoryInput.Unless(data.TryGetColor(slot, out Color value), value, slot));

    public static HistoryKey<Guid> Guid(int id) =>
        Plain(id, nameof(HistoryRecord.SetGuid), static (record, slot, value) => record.SetGuid(slot, value), static (data, slot) => MissingHistoryInput.Unless(data.TryGetGuid(slot, out Guid value), value, slot));

    public static HistoryKey<Seq<Guid>> Guids(int id) =>
        Plain(id, nameof(HistoryRecord.SetGuids), static (record, slot, values) => record.SetGuids(slot, values), static (data, slot) => MissingHistoryInput.Unless(data.TryGetGuids(slot, out Guid[] values), values, slot).Map(static found => toSeq(found)));

    public static HistoryKey<string> String(int id) =>
        Plain(id, nameof(HistoryRecord.SetString), static (record, slot, value) => record.SetString(slot, value), static (data, slot) => MissingHistoryInput.Unless(data.TryGetString(slot, out string value), value, slot));

    public static HistoryKey<PickCapture> ObjRef(int id) =>
        new(id,
            static (doc, record, slot, target) => Referenced(doc, target, slot, nameof(HistoryRecord.SetObjRef), reference => record.SetObjRef(slot, reference)),
            Captured);

    public static HistoryKey<(PickCapture Target, Point3d Point)> Point3dOnObject(int id) =>
        new(id,
            static (doc, record, slot, value) => Referenced(doc, value.Target, slot, nameof(HistoryRecord.SetPoint3dOnObject), reference => record.SetPoint3dOnObject(slot, reference, value.Point)),
            static (data, slot) => Captured(data, slot).Zip(IO.lift(() => MissingHistoryInput.Unless(data.TryGetPoint3dOnObject(slot, out Point3d point), point, slot))).As());

    private static HistoryKey<T> Plain<T>(int id, string member, Func<HistoryRecord, int, T, bool> write, Func<ReplayHistoryData, int, Fin<T>> read) where T : notnull =>
        new(id,
            (_, record, slot, value) => IO.lift(() => RefusedElement.Unless(write(record, slot, value), member, slot)),
            (data, slot) => IO.lift(() => read(data, slot)));

    private static IO<Unit> Referenced(RhinoDoc doc, PickCapture target, int slot, string member, Func<ObjRef, bool> write) =>
        (from reference in use(() => new ObjRef(doc, target.ObjectId, Conversions.Unset(target.GeometryComponentIndex)))
         from written in IO.lift(() => RefusedElement.Unless(write(reference), member, slot))
         select written).Bracket();

    private static IO<PickCapture> Captured(ReplayHistoryData data, int slot) =>
        use(IO.lift(() => Optional(data.GetRhinoObjRef(slot)).ToFin(new MissingHistoryInput(slot)))).Bind(PickCapture.Of).Bracket();

    // --- [RECORDS]
    public static IO<T> Record<T>(RhinoDoc doc, Command command, int version, bool copyOnReplace, Seq<HistoryInput> inputs, Func<HistoryRecord, IO<T>> body) =>
        from unique in IO.lift(Callbacks.Unique(inputs, static input => input.Id, nameof(HistoryRecord)).ToFin())
        from result in (from record in use(() => HistoryMapper.Create((command, version, copyOnReplace)))
                        from written in unique.TraverseM(input => input.Write(doc, record)).As()
                        from answer in body(record)
                        select answer).Bracket()
        select result;

    // --- [REPLAY]
    public static IO<Unit> Replay(ReplayHistoryData data, int version, Func<Seq<ReplayHistoryResult>, IO<Seq<HistoryOutput>>> outputs) =>
        from results in IO.lift(Fin<Seq<ReplayHistoryResult>> () => data.HistoryVersion switch {
            var recorded when recorded == version => toSeq(data.Results),
            var recorded => new StaleHistory(recorded, version),
        })
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
        from origin in IO.lift(() => links(id).ToFin(new Missing(nameof(ObjectTable.FindId))))
        let joined = new Func<Guid, IEnumerable<SEquatableEdge<Guid>>>(vertex => links(vertex).ToSeq().Bind(static found => found.Parents.Concat(found.Children)).Map(next => new SEquatableEdge<Guid>(vertex, next)))
        let reach = new ImplicitDepthFirstSearchAlgorithm<Guid, SEquatableEdge<Guid>>(joined.ToDelegateIncidenceGraph())
        from visited in IO.lift(() => reach.Compute(id))
        select reach.VerticesColors.Keys.ToBidirectionalGraph(vertex => links(vertex).ToSeq().Bind(static found => found.Children).Map(child => new SEquatableEdge<Guid>(vertex, child)), allowParallelEdges: false).ToArrayBidirectionalGraph();

    extension(IBidirectionalGraph<AdjacencyGraph<Guid, SEquatableEdge<Guid>>, CondensedEdge<Guid, SEquatableEdge<Guid>, AdjacencyGraph<Guid, SEquatableEdge<Guid>>>> graph) {
        public Seq<Guid> UpdateOrder => toSeq(graph.SourceFirstBidirectionalTopologicalSort(TopologicalSortDirection.Forward)).Bind(static component => toSeq(component.Vertices)).Strict();

        public Seq<Seq<Guid>> CycleGroups => toSeq(graph.Vertices).Filter(static component => component.EdgeCount > 0).Map(static component => toSeq(component.Vertices).Strict()).Strict();
    }
}

[Mapper]
internal static partial class HistoryMapper {
    internal static partial HistoryRecord Create((Command Command, int Version, bool CopyOnReplaceObject) source);
}
