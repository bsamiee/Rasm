using System.Drawing;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.DocObjects;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record HistoryKey<T>(Seq<int> Ids, Func<RhinoDoc, HistoryRecord, T, IO<Unit>> Write, Func<ReplayHistoryData, IO<T>> Read) where T : notnull {
    public HistoryKey<(T First, TNext Second)> Zip<TNext>(HistoryKey<TNext> next) where TNext : notnull =>
        new(Ids + next.Ids,
            (doc, record, value) => Write(doc, record, value.First) >> next.Write(doc, record, value.Second),
            data => from first in Read(data)
                    from second in next.Read(data)
                    select (first, second));

    public HistoryKey<TOut> Map<TOut>(Func<T, TOut> to, Func<TOut, T> from) where TOut : notnull =>
        new(Ids, (doc, record, value) => Write(doc, record, from(value)), data => Read(data).Map(to));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class HistoryKeys {
    // --- [KEYS]
    public static HistoryKey<bool> Bool(int id) => Plain(id, (record, value) => record.SetBool(id, value), data => (data.TryGetBool(id, out bool value), value));

    public static HistoryKey<int> Int(int id) => Plain(id, (record, value) => record.SetInt(id, value), data => (data.TryGetInt(id, out int value), value));

    public static HistoryKey<double> Double(int id) => Plain(id, (record, value) => record.SetDouble(id, value), data => (data.TryGetDouble(id, out double value), value));

    public static HistoryKey<Point3d> Point3d(int id) => Plain(id, (record, value) => record.SetPoint3d(id, value), data => (data.TryGetPoint3d(id, out Point3d value), value));

    public static HistoryKey<Vector3d> Vector3d(int id) => Plain(id, (record, value) => record.SetVector3d(id, value), data => (data.TryGetVector3d(id, out Vector3d value), value));

    public static HistoryKey<Transform> Transform(int id) => Plain(id, (record, value) => record.SetTransorm(id, value), data => (data.TryGetTransform(id, out Transform value), value));

    public static HistoryKey<Color> Color(int id) =>
        Plain(id, (record, value) => record.SetColor(id, value), data => (data.TryGetColor(id, out Color value), value));

    public static HistoryKey<Guid> Guid(int id) => Plain(id, (record, value) => record.SetGuid(id, value), data => (data.TryGetGuid(id, out Guid value), value));

    public static HistoryKey<string> String(int id) => Plain(id, (record, value) => record.SetString(id, value), data => (data.TryGetString(id, out string value), value));

    public static HistoryKey<Seq<int>> Ints(int id) =>
        Plain(id, (record, values) => record.SetInts(id, values), data => (data.TryGetInts(id, out int[] values), values)).Map(toSeq, static values => [.. values]);

    public static HistoryKey<Seq<double>> Doubles(int id) =>
        Plain(id, (record, values) => record.SetDoubles(id, values), data => (data.TryGetDoubles(id, out double[] values), values)).Map(toSeq, static values => [.. values]);

    public static HistoryKey<Seq<Guid>> Guids(int id) =>
        Plain(id, (record, values) => record.SetGuids(id, values), data => (data.TryGetGuids(id, out Guid[] values), values)).Map(toSeq, static values => [.. values]);

    public static HistoryKey<PickCapture> ObjRef(int id) =>
        new([id], (doc, record, target) => Referenced(doc, target, reference => Products.Accepted(() => record.SetObjRef(id, reference))), data => Captured(data, id));

    public static HistoryKey<(PickCapture Target, Point3d Point)> Point3dOnObject(int id) =>
        new([id],
            (doc, record, value) => Referenced(doc, value.Target, reference => Products.Accepted(() => record.SetPoint3dOnObject(id, reference, value.Point))),
            data => from target in Captured(data, id)
                    from point in Products.Accepted(() => (data.TryGetPoint3dOnObject(id, out Point3d value), value))
                    select (target, point));

    private static HistoryKey<T> Plain<T>(int id, Func<HistoryRecord, T, bool> write, Func<ReplayHistoryData, (bool Accepted, T Out)> read) where T : notnull =>
        new([id], (_, record, value) => Products.Accepted(() => write(record, value)), data => Products.Accepted(() => read(data)));

    private static IO<Unit> Referenced(RhinoDoc doc, PickCapture target, Func<ObjRef, IO<Unit>> write) =>
        (from reference in use(() => new ObjRef(doc, target.ObjectId, Conversions.Unset(target.GeometryComponentIndex)))
         from written in write(reference)
         select written).Bracket();

    private static IO<PickCapture> Captured(ReplayHistoryData data, int slot) =>
        Products.Of(() => data.GetRhinoObjRef(slot)).Bind(PickCapture.Of).Bracket();
}
