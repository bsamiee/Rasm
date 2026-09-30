using System.Drawing;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Commands;
using Rhino.DocObjects;

namespace Rasm.Rhino.Objects;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HistoryValue {
    public sealed record BoolValue(bool Value) : HistoryValue;

    public sealed record IntValue(int Value) : HistoryValue;

    public sealed record DoubleValue(double Value) : HistoryValue;

    public sealed record Point3dValue(Point3d Value) : HistoryValue;

    public sealed record Vector3dValue(Vector3d Value) : HistoryValue;

    public sealed record TransformValue(Transform Value) : HistoryValue;

    public sealed record ColorValue(Color Value) : HistoryValue;

    public sealed record GuidValue(Guid Value) : HistoryValue;

    public sealed record StringValue(string Value) : HistoryValue;

    public sealed record CurveValue(Curve Value) : HistoryValue;

    public sealed record SurfaceValue(Surface Value) : HistoryValue;

    public sealed record BrepValue(Brep Value) : HistoryValue;

    public sealed record MeshValue(Mesh Value) : HistoryValue;

    public sealed record ObjRefValue(Guid ObjectId, ComponentIndex Component) : HistoryValue;

    public sealed record PointOnObjectValue(Guid ObjectId, ComponentIndex Component, Point3d Point) : HistoryValue;

    public sealed record BoolsValue(Seq<bool> Values) : HistoryValue;

    public sealed record IntsValue(Seq<int> Values) : HistoryValue;

    public sealed record DoublesValue(Seq<double> Values) : HistoryValue;

    public sealed record Point3dsValue(Seq<Point3d> Values) : HistoryValue;

    public sealed record Vector3dsValue(Seq<Vector3d> Values) : HistoryValue;

    public sealed record ColorsValue(Seq<Color> Values) : HistoryValue;

    public sealed record GuidsValue(Seq<Guid> Values) : HistoryValue;

    public sealed record StringsValue(Seq<string> Values) : HistoryValue;

    public IO<Unit> Write(RhinoDoc doc, HistoryRecord record, int id) =>
        Switch(
            (Doc: doc, Record: record, Id: id),
            boolValue: static (state, value) => Set(() => state.Record.SetBool(state.Id, value.Value), state.Id, nameof(HistoryRecord.SetBool)),
            intValue: static (state, value) => Set(() => state.Record.SetInt(state.Id, value.Value), state.Id, nameof(HistoryRecord.SetInt)),
            doubleValue: static (state, value) => Set(() => state.Record.SetDouble(state.Id, value.Value), state.Id, nameof(HistoryRecord.SetDouble)),
            point3dValue: static (state, value) => Set(() => state.Record.SetPoint3d(state.Id, value.Value), state.Id, nameof(HistoryRecord.SetPoint3d)),
            vector3dValue: static (state, value) => Set(() => state.Record.SetVector3d(state.Id, value.Value), state.Id, nameof(HistoryRecord.SetVector3d)),
            transformValue: static (state, value) => Set(() => state.Record.SetTransorm(state.Id, value.Value), state.Id, nameof(HistoryRecord.SetTransorm)),
            colorValue: static (state, value) => Set(() => state.Record.SetColor(state.Id, value.Value), state.Id, nameof(HistoryRecord.SetColor)),
            guidValue: static (state, value) => Set(() => state.Record.SetGuid(state.Id, value.Value), state.Id, nameof(HistoryRecord.SetGuid)),
            stringValue: static (state, value) => Set(() => state.Record.SetString(state.Id, value.Value), state.Id, nameof(HistoryRecord.SetString)),
            curveValue: static (state, value) => Set(() => state.Record.SetCurve(state.Id, value.Value), state.Id, nameof(HistoryRecord.SetCurve)),
            surfaceValue: static (state, value) => Set(() => state.Record.SetSurface(state.Id, value.Value), state.Id, nameof(HistoryRecord.SetSurface)),
            brepValue: static (state, value) => Set(() => state.Record.SetBrep(state.Id, value.Value), state.Id, nameof(HistoryRecord.SetBrep)),
            meshValue: static (state, value) => Set(() => state.Record.SetMesh(state.Id, value.Value), state.Id, nameof(HistoryRecord.SetMesh)),
            objRefValue: static (state, value) => Referenced(
                state.Doc,
                state.Id,
                value.ObjectId,
                value.Component,
                reference => state.Record.SetObjRef(state.Id, reference),
                nameof(HistoryRecord.SetObjRef)),
            pointOnObjectValue: static (state, value) => Referenced(
                state.Doc,
                state.Id,
                value.ObjectId,
                value.Component,
                reference => state.Record.SetPoint3dOnObject(state.Id, reference, value.Point),
                nameof(HistoryRecord.SetPoint3dOnObject)),
            boolsValue: static (state, values) => Set(() => state.Record.SetBools(state.Id, values.Values), state.Id, nameof(HistoryRecord.SetBools)),
            intsValue: static (state, values) => Set(() => state.Record.SetInts(state.Id, values.Values), state.Id, nameof(HistoryRecord.SetInts)),
            doublesValue: static (state, values) => Set(() => state.Record.SetDoubles(state.Id, values.Values), state.Id, nameof(HistoryRecord.SetDoubles)),
            point3dsValue: static (state, values) => Set(() => state.Record.SetPoint3ds(state.Id, values.Values), state.Id, nameof(HistoryRecord.SetPoint3ds)),
            vector3dsValue: static (state, values) => Set(() => state.Record.SetVector3ds(state.Id, values.Values), state.Id, nameof(HistoryRecord.SetVector3ds)),
            colorsValue: static (state, values) => Set(() => state.Record.SetColors(state.Id, values.Values), state.Id, nameof(HistoryRecord.SetColors)),
            guidsValue: static (state, values) => Set(() => state.Record.SetGuids(state.Id, values.Values), state.Id, nameof(HistoryRecord.SetGuids)),
            stringsValue: static (state, values) => Set(() => state.Record.SetStrings(state.Id, values.Values), state.Id, nameof(HistoryRecord.SetStrings)));

    private static IO<Unit> Set(Func<bool> set, int id, string member) =>
        IO.lift(() => RefusedElement.Unless(set(), member, id));

    private static IO<Unit> Referenced(RhinoDoc doc, int id, Guid objectId, ComponentIndex component, Func<ObjRef, bool> set, string member) =>
        from keyed in IO.lift(() => Answers.Present(objectId).ToFin(new Invalid(nameof(objectId))))
        from accepted in DisposalOps.Using(() => new ObjRef(doc, keyed, component), reference => IO.lift(() => RefusedElement.Unless(set(reference), member, id)))
        select accepted;
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class HistoryValueKind {
    public static readonly HistoryValueKind Bool = new(static (data, id) => IO.lift(() => Answers.Found<HistoryValue>(data.TryGetBool(id, out bool value), new HistoryValue.BoolValue(value))));

    public static readonly HistoryValueKind Int = new(static (data, id) => IO.lift(() => Answers.Found<HistoryValue>(data.TryGetInt(id, out int value), new HistoryValue.IntValue(value))));

    public static readonly HistoryValueKind Ints = new(static (data, id) => IO.lift(() => Answers.Found<HistoryValue>(data.TryGetInts(id, out int[] values), new HistoryValue.IntsValue(toSeq(values)))));

    public static readonly HistoryValueKind Double = new(static (data, id) => IO.lift(() => Answers.Found<HistoryValue>(data.TryGetDouble(id, out double value), new HistoryValue.DoubleValue(value))));

    public static readonly HistoryValueKind Doubles = new(static (data, id) => IO.lift(() => Answers.Found<HistoryValue>(data.TryGetDoubles(id, out double[] values), new HistoryValue.DoublesValue(toSeq(values)))));

    public static readonly HistoryValueKind Point3d = new(static (data, id) => IO.lift(() => Answers.Found<HistoryValue>(data.TryGetPoint3d(id, out Point3d value), new HistoryValue.Point3dValue(value))));

    public static readonly HistoryValueKind PointOnObject = new(static (data, id) =>
        from point in IO.lift(() => Answers.Found(data.TryGetPoint3dOnObject(id, out Point3d value), value))
        from reference in Reference(data, id)
        select from located in point
               from captured in reference
               select (HistoryValue)new HistoryValue.PointOnObjectValue(captured.ObjectId, captured.Component, located));

    public static readonly HistoryValueKind Vector3d = new(static (data, id) => IO.lift(() => Answers.Found<HistoryValue>(data.TryGetVector3d(id, out Vector3d value), new HistoryValue.Vector3dValue(value))));

    public static readonly HistoryValueKind Transform = new(static (data, id) => IO.lift(() => Answers.Found<HistoryValue>(data.TryGetTransform(id, out Transform value), new HistoryValue.TransformValue(value))));

    public static readonly HistoryValueKind Color = new(static (data, id) => IO.lift(() => Answers.Found<HistoryValue>(data.TryGetColor(id, out Color value), new HistoryValue.ColorValue(value))));

    public static readonly HistoryValueKind Guid = new(static (data, id) => IO.lift(() => Answers.Found<HistoryValue>(data.TryGetGuid(id, out Guid value), new HistoryValue.GuidValue(value))));

    public static readonly HistoryValueKind Guids = new(static (data, id) => IO.lift(() => Answers.Found<HistoryValue>(data.TryGetGuids(id, out Guid[] values), new HistoryValue.GuidsValue(toSeq(values)))));

    public static readonly HistoryValueKind String = new(static (data, id) => IO.lift(() => Answers.Found<HistoryValue>(data.TryGetString(id, out string value), new HistoryValue.StringValue(value))));

    public static readonly HistoryValueKind ObjRef = new(static (data, id) => Reference(data, id).Map(static reference => reference.Map(static captured => (HistoryValue)new HistoryValue.ObjRefValue(captured.ObjectId, captured.Component))));

    [UseDelegateFromConstructor]
    public partial IO<Option<HistoryValue>> Read(ReplayHistoryData data, int id);

    private static IO<Option<PickCapture>> Reference(ReplayHistoryData data, int id) =>
        IO.lift(() => Optional(data.GetRhinoObjRef(id)))
            .Bind(static reference => reference.Traverse(static owned => DisposalOps.Using(IO.pure(owned), Captures.Capture)).As());
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ResultsChange {
    public sealed record Keep() : ResultsChange;

    public sealed record Append(int ItemCount) : ResultsChange;

    public sealed record Retain(Seq<int> Indices) : ResultsChange;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class History {
    // --- [RECORDS]
    public static IO<TValue> CreateRecord<TValue>(RhinoDoc doc, Command owner, int version, bool copyOnReplace, Seq<(int Id, HistoryValue Value)> values, Func<HistoryRecord, IO<TValue>> body) =>
        from distinct in IO.lift(() => Answers.Unique(values.Map(static value => value.Id), nameof(values)).ToFin())
        from result in DisposalOps.Using(() => new HistoryRecord(owner, version) { CopyOnReplaceObject = copyOnReplace }, record =>
            from written in values.TraverseM(value => value.Value.Write(doc, record, value.Id)).As()
            from result in body(record)
            select result)
        select result;

    // --- [REPLAY]
    public static IO<Seq<ReplayHistoryResult>> ChangeResults(ReplayHistoryData data, ResultsChange change) =>
        change.Switch(
            data,
            keep: static (source, _) => IO.lift(() => toSeq(source.Results)),
            append: static (source, append) =>
                from positive in IO.lift(() => Limits.AtLeast(1).Check(append.ItemCount, nameof(ResultsChange.Append.ItemCount)))
                from appended in toSeq(Range(1, append.ItemCount))
                    .TraverseM(_ => IO.lift(() => Missing.Unless(source.AppendHistoryResult(), nameof(ReplayHistoryData.AppendHistoryResult))))
                    .As()
                from rows in IO.lift(() => toSeq(source.Results))
                select rows,
            retain: static (source, retain) =>
                from current in IO.lift(() => toSeq(source.Results))
                from kept in IO.lift(() => retain.Indices.Traverse(index => current.At(index).ToFin(new IndexOutOfRange(nameof(ResultsChange.Retain), index, current.Count))).As())
                from updated in IO.lift(() => source.UpdateResultArray(kept))
                from rows in IO.lift(() => toSeq(source.Results))
                select rows);

    // --- [SETTINGS]
    public static IO<TValue> WithHistorySettings<TValue>(IO<TValue> body) =>
        IO.lift(static () => (
                HistorySettings.RecordingEnabled,
                HistorySettings.RecordNextCommand,
                HistorySettings.UpdateEnabled,
                HistorySettings.ObjectLockingEnabled,
                HistorySettings.BrokenRecordWarningEnabled))
            .Bracket(
                Use: _ => body,
                Fin: static saved => IO.lift(() => {
                    HistorySettings.RecordingEnabled = saved.RecordingEnabled;
                    HistorySettings.RecordNextCommand = saved.RecordNextCommand;
                    HistorySettings.UpdateEnabled = saved.UpdateEnabled;
                    HistorySettings.ObjectLockingEnabled = saved.ObjectLockingEnabled;
                    HistorySettings.BrokenRecordWarningEnabled = saved.BrokenRecordWarningEnabled;
                }));
}
