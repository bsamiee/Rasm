using System.Drawing;
using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Commands;
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
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ResultUpdate {
    public sealed record ToPoint(Point3d Point) : ResultUpdate;

    public sealed record ToTextDot(TextDot Dot) : ResultUpdate;

    public sealed record ToLine(Point3d From, Point3d To) : ResultUpdate;

    public sealed record ToPolyline(Seq<Point3d> Points) : ResultUpdate;

    public sealed record ToArc(Arc Arc) : ResultUpdate;

    public sealed record ToCircle(Circle Circle) : ResultUpdate;

    public sealed record ToEllipse(Ellipse Ellipse) : ResultUpdate;

    public sealed record ToSphere(Sphere Sphere) : ResultUpdate;

    public sealed record ToCurve(Curve Curve) : ResultUpdate;

    public sealed record ToSurface(Surface Surface) : ResultUpdate;

    public sealed record ToExtrusion(Extrusion Extrusion) : ResultUpdate;

    public sealed record ToMesh(Mesh Mesh) : ResultUpdate;

    public sealed record ToSubD(SubD SubD) : ResultUpdate;

    public sealed record ToBrep(Brep Brep) : ResultUpdate;

    public sealed record ToPointCloud(PointCloud Cloud) : ResultUpdate;

    public sealed record ToPoints(Seq<Point3d> Points) : ResultUpdate;

    public sealed record ToClippingPlane(Plane Plane, double UMagnitude, double VMagnitude, Seq<Guid> Viewports) : ResultUpdate;

    public sealed record ToLinearDimension(LinearDimension Dimension) : ResultUpdate;

    public sealed record ToRadialDimension(RadialDimension Dimension) : ResultUpdate;

    public sealed record ToAngularDimension(AngularDimension Dimension) : ResultUpdate;

    public sealed record ToLeader(Leader Leader) : ResultUpdate;

    public sealed record ToHatch(Hatch Hatch) : ResultUpdate;

    public sealed record ToText(TextEntity Text) : ResultUpdate;

    public sealed record ToRawText(string Text, Plane Plane, double Height, string Font, bool Bold, bool Italic, TextJustification Justification) : ResultUpdate;

    public sealed record ToInstanceReference(InstanceReferenceGeometry Reference) : ResultUpdate;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ResultsChange {
    public sealed record Keep() : ResultsChange;

    public sealed record Append(int ItemCount) : ResultsChange;

    public sealed record Retain(Seq<int> Indices) : ResultsChange;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HistoryLink {
    public sealed record Attach(HistoryRecord Record) : HistoryLink;

    public sealed record Detach() : HistoryLink;

    public sealed record CopyOnReplace(bool Copy) : HistoryLink;
}

[SmartEnum<string>]
public sealed partial class HistoryValueKind {
    public static readonly HistoryValueKind Bool = new(nameof(Bool), static (data, id) => IO.lift(() => Answers.Found<HistoryValue>(data.TryGetBool(id, out bool value), new HistoryValue.BoolValue(value))));

    public static readonly HistoryValueKind Int = new(nameof(Int), static (data, id) => IO.lift(() => Answers.Found<HistoryValue>(data.TryGetInt(id, out int value), new HistoryValue.IntValue(value))));

    public static readonly HistoryValueKind Ints = new(nameof(Ints), static (data, id) => IO.lift(() => Answers.Found<HistoryValue>(data.TryGetInts(id, out int[] values), new HistoryValue.IntsValue(toSeq(values)))));

    public static readonly HistoryValueKind Double = new(nameof(Double), static (data, id) => IO.lift(() => Answers.Found<HistoryValue>(data.TryGetDouble(id, out double value), new HistoryValue.DoubleValue(value))));

    public static readonly HistoryValueKind Doubles = new(nameof(Doubles), static (data, id) => IO.lift(() => Answers.Found<HistoryValue>(data.TryGetDoubles(id, out double[] values), new HistoryValue.DoublesValue(toSeq(values)))));

    public static readonly HistoryValueKind Point3d = new(nameof(Point3d), static (data, id) => IO.lift(() => Answers.Found<HistoryValue>(data.TryGetPoint3d(id, out Point3d value), new HistoryValue.Point3dValue(value))));

    public static readonly HistoryValueKind PointOnObject = new(nameof(PointOnObject), static (data, id) =>
        from point in IO.lift(() => Answers.Found(data.TryGetPoint3dOnObject(id, out Point3d value), value))
        from reference in History.GetRhinoObjRef(data, id)
        select from located in point
               from captured in reference
               select (HistoryValue)new HistoryValue.PointOnObjectValue(captured.ObjectId, captured.Component, located));

    public static readonly HistoryValueKind Vector3d = new(nameof(Vector3d), static (data, id) => IO.lift(() => Answers.Found<HistoryValue>(data.TryGetVector3d(id, out Vector3d value), new HistoryValue.Vector3dValue(value))));

    public static readonly HistoryValueKind Transform = new(nameof(Transform), static (data, id) => IO.lift(() => Answers.Found<HistoryValue>(data.TryGetTransform(id, out Transform value), new HistoryValue.TransformValue(value))));

    public static readonly HistoryValueKind Color = new(nameof(Color), static (data, id) => IO.lift(() => Answers.Found<HistoryValue>(data.TryGetColor(id, out Color value), new HistoryValue.ColorValue(value))));

    public static readonly HistoryValueKind Guid = new(nameof(Guid), static (data, id) => IO.lift(() => Answers.Found<HistoryValue>(data.TryGetGuid(id, out Guid value), new HistoryValue.GuidValue(value))));

    public static readonly HistoryValueKind Guids = new(nameof(Guids), static (data, id) => IO.lift(() => Answers.Found<HistoryValue>(data.TryGetGuids(id, out Guid[] values), new HistoryValue.GuidsValue(toSeq(values)))));

    public static readonly HistoryValueKind String = new(nameof(String), static (data, id) => IO.lift(() => Answers.Found<HistoryValue>(data.TryGetString(id, out string value), new HistoryValue.StringValue(value))));

    public static readonly HistoryValueKind ObjRef = new(nameof(ObjRef), static (data, id) => History.GetRhinoObjRef(data, id).Map(static reference => reference.Map(static captured => (HistoryValue)new HistoryValue.ObjRefValue(captured.ObjectId, captured.Component))));

    [UseDelegateFromConstructor]
    public partial IO<Option<HistoryValue>> Read(ReplayHistoryData data, int id);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class History {
    // --- [RECORDS]
    public static IO<TValue> CreateRecord<TValue>(RhinoDoc doc, Command owner, int version, bool copyOnReplace, Seq<(int Id, HistoryValue Value)> values, Func<HistoryRecord, IO<TValue>> body) =>
        from versioned in IO.lift(() => Invalid.Unless(version != 0, nameof(HistoryRecord.SetHistoryVersion)))
        from distinct in IO.lift(() => Answers.Unique(values.Map(static value => value.Id), static (id, _) => new DuplicateHistoryValueId(id)).ToFin())
        from result in Disposal.Using(() => new HistoryRecord(owner, version), record =>
            from flagged in IO.lift(() => { record.CopyOnReplaceObject = copyOnReplace; })
            from written in values.TraverseM(value => Written(doc, record, value.Id, value.Value)).As()
            from result in body(record)
            select result)
        select result;

    private static IO<Unit> Written(RhinoDoc doc, HistoryRecord record, int id, HistoryValue entry) =>
        entry.Switch(
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
        IO.lift(() => Accepted(set(), id, member));

    private static Fin<Unit> Accepted(bool accepted, int id, string member) =>
        accepted ? unit : new HistoryValueRefused(id, member);

    private static IO<Unit> Referenced(RhinoDoc doc, int id, Guid objectId, ComponentIndex component, Func<ObjRef, bool> set, string member) =>
        from keyed in IO.lift(() => Answers.NonEmpty(objectId, nameof(ObjRef)))
        from accepted in Disposal.Using(() => new ObjRef(doc, keyed, component), reference => IO.lift(() => Accepted(set(reference), id, member)))
        select accepted;

    // --- [REPLAY]
    internal static IO<Option<PickCapture>> GetRhinoObjRef(ReplayHistoryData data, int id) =>
        IO.lift(() => Optional(data.GetRhinoObjRef(id)))
            .Bind(static reference => reference.Traverse(static owned => Selections.CaptureOwned(Seq(owned))).As())
            .Map(static captures => captures.Bind(static rows => rows.Head));

    public static IO<Unit> UpdateResult(ReplayHistoryResult result, ResultUpdate update, Option<ObjectAttributes> attributes) =>
        IO.lift(() => update.Switch<(ReplayHistoryResult Result, ObjectAttributes? Attributes), Fin<Unit>>(
            (result, attributes.ValueUnsafe()),
            toPoint: static (state, point) => Refused.Unless(state.Result.UpdateToPoint(point.Point, state.Attributes), nameof(ReplayHistoryResult.UpdateToPoint)),
            toTextDot: static (state, dot) => Refused.Unless(state.Result.UpdateToTextDot(dot.Dot, state.Attributes), nameof(ReplayHistoryResult.UpdateToTextDot)),
            toLine: static (state, line) => Refused.Unless(state.Result.UpdateToLine(line.From, line.To, state.Attributes), nameof(ReplayHistoryResult.UpdateToLine)),
            toPolyline: static (state, polyline) => Refused.Unless(state.Result.UpdateToPolyline(polyline.Points, state.Attributes), nameof(ReplayHistoryResult.UpdateToPolyline)),
            toArc: static (state, arc) => Refused.Unless(state.Result.UpdateToArc(arc.Arc, state.Attributes), nameof(ReplayHistoryResult.UpdateToArc)),
            toCircle: static (state, circle) => Refused.Unless(state.Result.UpdateToCircle(circle.Circle, state.Attributes), nameof(ReplayHistoryResult.UpdateToCircle)),
            toEllipse: static (state, ellipse) => Refused.Unless(state.Result.UpdateToEllipse(ellipse.Ellipse, state.Attributes), nameof(ReplayHistoryResult.UpdateToEllipse)),
            toSphere: static (state, sphere) => Refused.Unless(state.Result.UpdateToSphere(sphere.Sphere, state.Attributes), nameof(ReplayHistoryResult.UpdateToSphere)),
            toCurve: static (state, curve) => Refused.Unless(state.Result.UpdateToCurve(curve.Curve, state.Attributes), nameof(ReplayHistoryResult.UpdateToCurve)),
            toSurface: static (state, surface) => Refused.Unless(state.Result.UpdateToSurface(surface.Surface, state.Attributes), nameof(ReplayHistoryResult.UpdateToSurface)),
            toExtrusion: static (state, extrusion) => Refused.Unless(state.Result.UpdateToExtrusion(extrusion.Extrusion, state.Attributes), nameof(ReplayHistoryResult.UpdateToExtrusion)),
            toMesh: static (state, mesh) => Refused.Unless(state.Result.UpdateToMesh(mesh.Mesh, state.Attributes), nameof(ReplayHistoryResult.UpdateToMesh)),
            toSubD: static (state, subD) => Refused.Unless(state.Result.UpdateToSubD(subD.SubD, state.Attributes), nameof(ReplayHistoryResult.UpdateToSubD)),
            toBrep: static (state, brep) => Refused.Unless(state.Result.UpdateToBrep(brep.Brep, state.Attributes), nameof(ReplayHistoryResult.UpdateToBrep)),
            toPointCloud: static (state, cloud) => Refused.Unless(state.Result.UpdateToPointCloud(cloud.Cloud, state.Attributes), nameof(ReplayHistoryResult.UpdateToPointCloud)),
            toPoints: static (state, points) =>
                from filled in Invalid.Unless(!points.Points.IsEmpty, nameof(ReplayHistoryResult.UpdateToPointCloud))
                from updated in Refused.Unless(state.Result.UpdateToPointCloud(points.Points, state.Attributes), nameof(ReplayHistoryResult.UpdateToPointCloud))
                select updated,
            toClippingPlane: static (state, clipping) =>
                from valid in Invalid.Unless(clipping.Plane.IsValid, nameof(Plane.IsValid))
                from updated in Refused.Unless(
                    state.Result.UpdateToClippingPlane(clipping.Plane, clipping.UMagnitude, clipping.VMagnitude, clipping.Viewports, state.Attributes),
                    nameof(ReplayHistoryResult.UpdateToClippingPlane))
                select updated,
            toLinearDimension: static (state, dimension) => Refused.Unless(state.Result.UpdateToLinearDimension(dimension.Dimension, state.Attributes), nameof(ReplayHistoryResult.UpdateToLinearDimension)),
            toRadialDimension: static (state, dimension) => Refused.Unless(state.Result.UpdateToRadialDimension(dimension.Dimension, state.Attributes), nameof(ReplayHistoryResult.UpdateToRadialDimension)),
            toAngularDimension: static (state, dimension) => Refused.Unless(state.Result.UpdateToAngularDimension(dimension.Dimension, state.Attributes), nameof(ReplayHistoryResult.UpdateToAngularDimension)),
            toLeader: static (state, leader) => Refused.Unless(state.Result.UpdateToLeader(leader.Leader, state.Attributes), nameof(ReplayHistoryResult.UpdateToLeader)),
            toHatch: static (state, hatch) => Refused.Unless(state.Result.UpdateToHatch(hatch.Hatch, state.Attributes), nameof(ReplayHistoryResult.UpdateToHatch)),
            toText: static (state, text) => Refused.Unless(state.Result.UpdateToText(text.Text, state.Attributes), nameof(ReplayHistoryResult.UpdateToText)),
            toRawText: static (state, text) => Refused.Unless(
                state.Result.UpdateToText(text.Text, text.Plane, text.Height, text.Font, text.Bold, text.Italic, text.Justification, state.Attributes),
                nameof(ReplayHistoryResult.UpdateToText)),
            toInstanceReference: static (state, reference) => Refused.Unless(
                state.Result.UpdateToInstanceReferenceGeometry(reference.Reference, state.Attributes),
                nameof(ReplayHistoryResult.UpdateToInstanceReferenceGeometry))));

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
                from kept in IO.lift(() => retain.Indices.TraverseM(index => current.At(index).ToFin(new IndexOutOfRange(nameof(ResultsChange.Retain), index, current.Count))).As())
                from updated in IO.lift(() => source.UpdateResultArray(kept))
                from rows in IO.lift(() => toSeq(source.Results))
                select rows);

    // --- [SETTINGS]
    public static IO<TValue> WithHistorySettings<TValue>(IO<TValue> body) =>
        Disposal.Bracketed(
            IO.lift(static () => (
                HistorySettings.RecordingEnabled,
                HistorySettings.RecordNextCommand,
                HistorySettings.UpdateEnabled,
                HistorySettings.ObjectLockingEnabled,
                HistorySettings.BrokenRecordWarningEnabled)),
            static saved => IO.lift(() => {
                HistorySettings.RecordingEnabled = saved.RecordingEnabled;
                HistorySettings.RecordNextCommand = saved.RecordNextCommand;
                HistorySettings.UpdateEnabled = saved.UpdateEnabled;
                HistorySettings.ObjectLockingEnabled = saved.ObjectLockingEnabled;
                HistorySettings.BrokenRecordWarningEnabled = saved.BrokenRecordWarningEnabled;
            }),
            _ => body);

    // --- [LINKS]
    public static IO<Unit> Link(RhinoObject o, HistoryLink link) =>
        IO.lift(() => link.Switch(
            o,
            attach: static (target, attach) => Refused.Unless(target.SetHistory(attach.Record), nameof(RhinoObject.SetHistory)),
            detach: static (target, _) => Refused.Unless(target.DeleteHistoryRecord(), nameof(RhinoObject.DeleteHistoryRecord)),
            copyOnReplace: static (target, copy) => {
                target.SetCopyHistoryOnReplace(copy.Copy);
                return Mismatch.Unless(target.CopyHistoryOnReplace() == copy.Copy, nameof(RhinoObject.CopyHistoryOnReplace));
            }));
}
