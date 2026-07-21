# [RASM_RHINO_OBJECTS_HISTORY]

`HistoryScript` writes one generated slot family into a leased `HistoryRecord`; the same `SlotValue` cases recover the host-readable subset and reject write-only cases. `Regrown` owns the replay update family, `ReplayProgram` compiles preparation plus regrowth into `CommandPolicy.Replay`, and `Chronicle` owns undo-recorded linkage, dependency topology, and scoped `HistorySettings` conduct.

## [01]-[INDEX]

- [02]-[SLOT_ALGEBRA]: `SlotKey`, `SlotValue`, and the write dispatch onto the native record.
- [03]-[SCRIPT]: `HistoryScript` — the single-use record mint and its threading law.
- [04]-[REPLAY_READS]: `SlotValue.Recover` — read dispatch on the write-family case.
- [05]-[REGROWN]: `Regrown` — generated update dispatch over the `UpdateTo*` family.
- [06]-[REPLAY_PROGRAM]: `ReplayProgram` — the compiled delegate and the strict-`bool` seam.
- [07]-[CHRONICLE]: `BondOp`, `WebAsk`, `HistoryConduct`, and the linkage entry.
- [08]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[SLOT_ALGEBRA]

- Owner: `SlotKey` `[ValueObject<int>]` closes non-negative slot identity. `SlotValue` `[Union]` covers every catalogued scalar, geometry, object-source, and plural setter payload.
- Law: the transform setter is the host's own misspelling — `SetTransorm` is the literal managed member (the native call underneath is spelled correctly), so the `Motion` arm names it verbatim and no local alias "fixes" it.
- Law: source slots arm replay — `Source` and `PointOnSource` register a dependency through `SetObjRef`/`SetPoint3dOnObject`, so editing the referenced object fires the replay; a script recording only value slots never re-fires on a geometry edit, which makes at least one source slot the practical floor for a history-aware command.
- Law: read asymmetry is payload law — plural readers exist only for ints, doubles, and guids, and no geometry reader exists at all, so geometry and the other plural payloads are write-only evidence: a replay program re-derives geometry from its source slots, and a plural point, vector, color, text, or bool payload that must read back encodes as scalar slots under consecutive keys.
- Growth: a new payload kind is one `SlotValue` case whose write and read arms are compiler-coupled; write-only cases return typed unreadable failure.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Domain;
using Rasm.Rhino.Blocks;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using Web = QuikGraph.BidirectionalGraph<System.Guid, QuikGraph.SEdge<System.Guid>>;

namespace Rasm.Rhino.Objects;

// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<int>]
public readonly partial struct SlotKey {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref int value) {
        validationError = value < 0 ? new ValidationError(message: "slot key is negative") : validationError;
    }
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SlotValue {
    private SlotValue() { }
    public sealed record Toggle(bool Value) : SlotValue;
    public sealed record Count(int Value) : SlotValue;
    public sealed record Scalar(double Value) : SlotValue;
    public sealed record Point(Point3d Value) : SlotValue;
    public sealed record Direction(Vector3d Value) : SlotValue;
    public sealed record Motion(Transform Value) : SlotValue;
    public sealed record Paint(System.Drawing.Color Value) : SlotValue;
    public sealed record Id(Guid Value) : SlotValue;
    public sealed record Text(string Value) : SlotValue;
    public sealed record CurveSlot(Curve Value) : SlotValue;
    public sealed record SurfaceSlot(Surface Value) : SlotValue;
    public sealed record BrepSlot(Brep Value) : SlotValue;
    public sealed record MeshSlot(Mesh Value) : SlotValue;
    public sealed record Source(HistorySource Value) : SlotValue;
    public sealed record PointOnSource(HistorySource Value, Point3d At) : SlotValue;
    public sealed record Toggles(Seq<bool> Values) : SlotValue;
    public sealed record Counts(Seq<int> Values) : SlotValue;
    public sealed record Scalars(Seq<double> Values) : SlotValue;
    public sealed record Points(Seq<Point3d> Values) : SlotValue;
    public sealed record Directions(Seq<Vector3d> Values) : SlotValue;
    public sealed record Paints(Seq<System.Drawing.Color> Values) : SlotValue;
    public sealed record Ids(Seq<Guid> Values) : SlotValue;
    public sealed record Texts(Seq<string> Values) : SlotValue;

    internal Fin<SlotValue> Admit(Op op) =>
        Switch(
            op,
            toggle: static (_, slot) => Fin.Succ<SlotValue>(slot),
            count: static (_, slot) => Fin.Succ<SlotValue>(slot),
            scalar: static (key, slot) => guard(double.IsFinite(slot.Value), key.InvalidInput()).ToFin()
                .Map(_ => (SlotValue)slot),
            point: static (key, slot) => key.AcceptInput(value: slot.Value).Map(_ => (SlotValue)slot),
            direction: static (key, slot) => key.AcceptInput(value: slot.Value).Map(_ => (SlotValue)slot),
            motion: static (key, slot) => key.AcceptInput(value: slot.Value).Map(_ => (SlotValue)slot),
            paint: static (_, slot) => Fin.Succ<SlotValue>(slot),
            id: static (_, slot) => Fin.Succ<SlotValue>(slot),
            text: static (key, slot) => key.Need(slot.Value).Map(_ => (SlotValue)slot),
            curveSlot: static (key, slot) => guard(slot.Value is { IsValid: true }, key.InvalidInput()).ToFin()
                .Map(_ => (SlotValue)slot),
            surfaceSlot: static (key, slot) => guard(slot.Value is { IsValid: true }, key.InvalidInput()).ToFin()
                .Map(_ => (SlotValue)slot),
            brepSlot: static (key, slot) => guard(slot.Value is { IsValid: true }, key.InvalidInput()).ToFin()
                .Map(_ => (SlotValue)slot),
            meshSlot: static (key, slot) => guard(slot.Value is { IsValid: true }, key.InvalidInput()).ToFin()
                .Map(_ => (SlotValue)slot),
            source: static (key, slot) => key.Need(slot.Value).Map(_ => (SlotValue)slot),
            pointOnSource: static (key, slot) =>
                from source in key.Need(slot.Value)
                from point in key.AcceptInput(value: slot.At)
                select (SlotValue)new PointOnSource(Value: source, At: point),
            toggles: static (_, slot) => Fin.Succ<SlotValue>(slot),
            counts: static (_, slot) => Fin.Succ<SlotValue>(slot),
            scalars: static (key, slot) => guard(slot.Values.ForAll(double.IsFinite), key.InvalidInput()).ToFin()
                .Map(_ => (SlotValue)slot),
            points: static (key, slot) => slot.Values.TraverseM(value => key.AcceptInput(value)).As()
                .Map(values => (SlotValue)new Points(Values: values)),
            directions: static (key, slot) => slot.Values.TraverseM(value => key.AcceptInput(value)).As()
                .Map(values => (SlotValue)new Directions(Values: values)),
            paints: static (_, slot) => Fin.Succ<SlotValue>(slot),
            ids: static (_, slot) => Fin.Succ<SlotValue>(slot),
            texts: static (key, slot) => slot.Values.TraverseM(value => key.Need(value)).As()
                .Map(values => (SlotValue)new Texts(Values: values)));

    internal Fin<Unit> Write(RhinoDoc document, HistoryRecord record, SlotKey key, Op op) =>
        op.Catch(() => Switch(
            (Document: document, Record: record, Id: (int)key, Op: op),
            toggle: static (context, slot) => context.Op.Confirm(success: context.Record.SetBool(id: context.Id, value: slot.Value)),
            count: static (context, slot) => context.Op.Confirm(success: context.Record.SetInt(id: context.Id, value: slot.Value)),
            scalar: static (context, slot) => context.Op.Confirm(success: context.Record.SetDouble(id: context.Id, value: slot.Value)),
            point: static (context, slot) => context.Op.Confirm(success: context.Record.SetPoint3d(id: context.Id, value: slot.Value)),
            direction: static (context, slot) => context.Op.Confirm(success: context.Record.SetVector3d(id: context.Id, value: slot.Value)),
            motion: static (context, slot) => context.Op.Confirm(success: context.Record.SetTransorm(id: context.Id, value: slot.Value)),
            paint: static (context, slot) => context.Op.Confirm(success: context.Record.SetColor(id: context.Id, value: slot.Value)),
            id: static (context, slot) => context.Op.Confirm(success: context.Record.SetGuid(id: context.Id, value: slot.Value)),
            text: static (context, slot) => context.Op.Confirm(success: context.Record.SetString(id: context.Id, value: slot.Value)),
            curveSlot: static (context, slot) => context.Op.Confirm(success: context.Record.SetCurve(id: context.Id, value: slot.Value)),
            surfaceSlot: static (context, slot) => context.Op.Confirm(success: context.Record.SetSurface(id: context.Id, value: slot.Value)),
            brepSlot: static (context, slot) => context.Op.Confirm(success: context.Record.SetBrep(id: context.Id, value: slot.Value)),
            meshSlot: static (context, slot) => context.Op.Confirm(success: context.Record.SetMesh(id: context.Id, value: slot.Value)),
            source: static (context, slot) => slot.Value.Use(
                document: context.Document,
                body: source => context.Op.Confirm(success: context.Record.SetObjRef(id: context.Id, value: source)),
                op: context.Op),
            pointOnSource: static (context, slot) => slot.Value.Use(
                document: context.Document,
                body: source => context.Op.Confirm(
                    success: context.Record.SetPoint3dOnObject(id: context.Id, objref: source, value: slot.At)),
                op: context.Op),
            toggles: static (context, slot) => context.Op.Confirm(success: context.Record.SetBools(id: context.Id, values: slot.Values.AsIterable())),
            counts: static (context, slot) => context.Op.Confirm(success: context.Record.SetInts(id: context.Id, values: slot.Values.AsIterable())),
            scalars: static (context, slot) => context.Op.Confirm(success: context.Record.SetDoubles(id: context.Id, values: slot.Values.AsIterable())),
            points: static (context, slot) => context.Op.Confirm(success: context.Record.SetPoint3ds(id: context.Id, values: slot.Values.AsIterable())),
            directions: static (context, slot) => context.Op.Confirm(success: context.Record.SetVector3ds(id: context.Id, values: slot.Values.AsIterable())),
            paints: static (context, slot) => context.Op.Confirm(success: context.Record.SetColors(id: context.Id, values: slot.Values.AsIterable())),
            ids: static (context, slot) => context.Op.Confirm(success: context.Record.SetGuids(id: context.Id, values: slot.Values.AsIterable())),
            texts: static (context, slot) => context.Op.Confirm(success: context.Record.SetStrings(id: context.Id, values: slot.Values.AsIterable()))));

    public Fin<SlotValue> Recover(ReplayHistoryData data, SlotKey key, Op? keyOp = null) {
        Op op = keyOp.OrDefault();
        return from active in op.Need(data)
               from value in op.Catch(() => Switch(
                   (Data: active, Id: (int)key, Op: op),
                   toggle: static (context, _) => context.Data.TryGetBool(context.Id, out bool value)
                       ? Fin.Succ<SlotValue>(new Toggle(value)) : Fin.Fail<SlotValue>(context.Op.MissingContext()),
                   count: static (context, _) => context.Data.TryGetInt(context.Id, out int value)
                       ? Fin.Succ<SlotValue>(new Count(value)) : Fin.Fail<SlotValue>(context.Op.MissingContext()),
                   scalar: static (context, _) => context.Data.TryGetDouble(context.Id, out double value)
                       ? Fin.Succ<SlotValue>(new Scalar(value)) : Fin.Fail<SlotValue>(context.Op.MissingContext()),
                   point: static (context, _) => context.Data.TryGetPoint3d(context.Id, out Point3d value)
                       ? Fin.Succ<SlotValue>(new Point(value)) : Fin.Fail<SlotValue>(context.Op.MissingContext()),
                   direction: static (context, _) => context.Data.TryGetVector3d(context.Id, out Vector3d value)
                       ? Fin.Succ<SlotValue>(new Direction(value)) : Fin.Fail<SlotValue>(context.Op.MissingContext()),
                   motion: static (context, _) => context.Data.TryGetTransform(context.Id, out Transform value)
                       ? Fin.Succ<SlotValue>(new Motion(value)) : Fin.Fail<SlotValue>(context.Op.MissingContext()),
                   paint: static (context, _) => context.Data.TryGetColor(context.Id, out System.Drawing.Color value)
                       ? Fin.Succ<SlotValue>(new Paint(value)) : Fin.Fail<SlotValue>(context.Op.MissingContext()),
                   id: static (context, _) => context.Data.TryGetGuid(context.Id, out Guid value)
                       ? Fin.Succ<SlotValue>(new Id(value)) : Fin.Fail<SlotValue>(context.Op.MissingContext()),
                   text: static (context, _) => context.Data.TryGetString(context.Id, out string value)
                       ? Fin.Succ<SlotValue>(new Text(value)) : Fin.Fail<SlotValue>(context.Op.MissingContext()),
                   curveSlot: static (context, _) => Fin.Fail<SlotValue>(context.Op.InvalidInput()),
                   surfaceSlot: static (context, _) => Fin.Fail<SlotValue>(context.Op.InvalidInput()),
                   brepSlot: static (context, _) => Fin.Fail<SlotValue>(context.Op.InvalidInput()),
                   meshSlot: static (context, _) => Fin.Fail<SlotValue>(context.Op.InvalidInput()),
                   source: static (context, _) => RecoverSource(context.Data, context.Id, context.Op)
                       .Map(value => (SlotValue)new Source(value)),
                   pointOnSource: static (context, _) => context.Data.TryGetPoint3dOnObject(context.Id, out Point3d value)
                       ? RecoverSource(context.Data, context.Id, context.Op)
                           .Map(source => (SlotValue)new PointOnSource(source, value))
                       : Fin.Fail<SlotValue>(context.Op.MissingContext()),
                   toggles: static (context, _) => Fin.Fail<SlotValue>(context.Op.InvalidInput()),
                   counts: static (context, _) => context.Data.TryGetInts(context.Id, out int[] values)
                       ? Fin.Succ<SlotValue>(new Counts(toSeq(values))) : Fin.Fail<SlotValue>(context.Op.MissingContext()),
                   scalars: static (context, _) => context.Data.TryGetDoubles(context.Id, out double[] values)
                       ? Fin.Succ<SlotValue>(new Scalars(toSeq(values))) : Fin.Fail<SlotValue>(context.Op.MissingContext()),
                   points: static (context, _) => Fin.Fail<SlotValue>(context.Op.InvalidInput()),
                   directions: static (context, _) => Fin.Fail<SlotValue>(context.Op.InvalidInput()),
                   paints: static (context, _) => Fin.Fail<SlotValue>(context.Op.InvalidInput()),
                   ids: static (context, _) => context.Data.TryGetGuids(context.Id, out Guid[] values)
                       ? Fin.Succ<SlotValue>(new Ids(toSeq(values))) : Fin.Fail<SlotValue>(context.Op.MissingContext()),
                   texts: static (context, _) => Fin.Fail<SlotValue>(context.Op.InvalidInput())))
               from admitted in value.Admit(op)
               select admitted;
    }

    private static Fin<HistorySource> RecoverSource(ReplayHistoryData data, int id, Op op) =>
        op.Catch(() => {
            using ObjRef? source = data.GetRhinoObjRef(id: id);
            return Optional(source).ToFin(Fail: op.MissingContext()).Bind(value => HistorySource.Capture(value, op));
        });
}
```

## [03]-[SCRIPT]

- Owner: `HistorySource` is detached object-plus-component identity; `HistoryScript` is the whole authoring intent as one value: version, replace-survival grant, and the keyed slot sequence.
- Law: each mint is single-use — `Mint` constructs one native record against the owning `Command`, writes every slot, stamps `CopyOnReplaceObject`, and answers an owned lease. A mid-write refusal disposes the half-written record. One lease threads into `TableOp.Add`, `TableOp.Cloud`, block placement, or `BondOp.Attach`, then disposes after the host copies it.
- Law: source slots reconstruct `ObjRef` from `HistorySource` only while `Mint` writes into the document-bound record; authoring intent carries no live selection handle.
- Law: the version is the compatibility key — `ReplayProgram` folds a mismatched `HistoryVersion` to graceful non-replay, so bumping the script version retires stale records without faulting old documents.
- Boundary: the record constructor keys on `Command.Id`, so scripts mint inside the command that will replay them; the command page's adapter is the only `ReplayHistory` override site.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class HistorySource : IDetachedDocumentResult {
    public Guid Id { get; }
    public ComponentIndex Component { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Guid id,
        ref ComponentIndex component) {
        validationError = id == Guid.Empty
            ? new ValidationError(message: "history source is invalid")
            : validationError;
    }

    internal static Fin<HistorySource> Capture(ObjRef source, Op op) =>
        Create(id: source.ObjectId, component: source.GeometryComponentIndex).ToFin()
            .MapFail(_ => op.InvalidResult());

    internal Fin<T> Use<T>(RhinoDoc document, Func<ObjRef, Fin<T>> body, Op op) =>
        op.Catch(() => {
            using ObjRef source = new(document, Id, Component);
            return body(source);
        });
}

public sealed class HistoryScript {
    private HistoryScript(int version, ObjectSignal copyOnReplace, Seq<(SlotKey Key, SlotValue Value)> slots) {
        Version = version;
        CopyOnReplace = copyOnReplace;
        Slots = slots;
    }

    public int Version { get; }
    public ObjectSignal CopyOnReplace { get; }
    public Seq<(SlotKey Key, SlotValue Value)> Slots { get; }

    public static Fin<HistoryScript> Of(
        int version,
        ObjectSignal copyOnReplace,
        params ReadOnlySpan<(SlotKey Key, SlotValue Value)> slots) {
        Op op = Op.Of(name: nameof(HistoryScript));
        return from _ in guard(version > 0, op.InvalidInput()).ToFin()
               from survival in op.Need(copyOnReplace)
               from rows in toSeq(slots.ToArray()).TraverseM(slot => op.Need(slot.Value)
                   .Bind(value => value.Admit(op: op))
                   .Map(value => (slot.Key, value))).As()
               from __ in guard(!rows.IsEmpty && rows.Map(static slot => slot.Key).Distinct().Count == rows.Count, op.InvalidInput()).ToFin()
               select new HistoryScript(version: version, copyOnReplace: survival, slots: rows);
    }

    public Fin<Lease<HistoryRecord>> Mint(RhinoDoc document, Command owner, Op? key = null) {
        Op op = key.OrDefault();
        return from host in op.Need(document)
               from command in op.Need(owner)
               from record in op.Catch(() => Optional(new HistoryRecord(command: command, version: Version)).ToFin(Fail: op.InvalidResult()))
               from lease in Slots
                   .TraverseM(slot => slot.Value.Write(document: host, record: record, key: slot.Key, op: op)).As()
                   .Bind(_ => op.Catch(() => {
                       record.CopyOnReplaceObject = CopyOnReplace.On;
                       return Fin.Succ<Lease<HistoryRecord>>(value: new Lease<HistoryRecord>.Owned(Value: record));
                   }))
                   .MapFail(error => {
                       record.Dispose();
                       return error;
                   })
               select lease;
    }
}
```

## [04]-[REPLAY_READS]

- Owner: `SlotValue.Recover` dispatches from the same generated case that wrote the slot; no parallel type roster can drift from the setter family.
- Law: host-readable cases call their matching `TryGet*` member, while geometry and unsupported plural cases return typed unreadable failure; a false host result remains missing-or-mistyped evidence.
- Law: source recovery projects `GetRhinoObjRef` into `HistorySource` and disposes the live handle inside the callback; source geometry re-enters only through `ReplayFrame.Use`.

## [05]-[REGROWN]

- Owner: `Regrown` `[Union]` is the single update family; each case owns its payload, admission, and `ReplayHistoryResult.UpdateTo*` arm, while `LineGrowth`, `ClippingGrowth`, and `RawTextGrowth` carry multi-field payloads.
- Law: replay updates the existing result. Every case delegates to one verified `ReplayHistoryResult.UpdateTo*` overload; no add or replace path exists.
- Law: attributes are optional by host contract — every `UpdateTo*` overload accepts a null attribute set, so an existing result regrows with its object's live attributes while a result minted by `AppendHistoryResult` carries no existing object and regrows with absent attributes the host defaults.
- Law: clipping-plane arity collapses onto the plural overload. One viewport is a one-element sequence, and empty viewport rosters are refused at admission.
- Growth: a host overload adds one `Regrown` case and the generator forces both admission and application arms.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
public readonly record struct LineGrowth(Point3d From, Point3d To);
public readonly record struct ClippingGrowth(Plane Frame, double U, double V, Seq<Guid> Viewports);
public readonly record struct RawTextGrowth(string Text, Plane Frame, double Height, string Font, bool Bold, bool Italic, TextJustification Justification);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Regrown {
    private Regrown() { }
    public sealed record Point(Point3d Value) : Regrown;
    public sealed record Dot(TextDot Value) : Regrown;
    public sealed record Line(LineGrowth Value) : Regrown;
    public sealed record Polyline(Seq<Point3d> Values) : Regrown;
    public sealed record Arc(Rhino.Geometry.Arc Value) : Regrown;
    public sealed record Circle(Rhino.Geometry.Circle Value) : Regrown;
    public sealed record Ellipse(Rhino.Geometry.Ellipse Value) : Regrown;
    public sealed record Sphere(Rhino.Geometry.Sphere Value) : Regrown;
    public sealed record Curve(Rhino.Geometry.Curve Value) : Regrown;
    public sealed record Surface(Rhino.Geometry.Surface Value) : Regrown;
    public sealed record Extrusion(Rhino.Geometry.Extrusion Value) : Regrown;
    public sealed record Mesh(Rhino.Geometry.Mesh Value) : Regrown;
    public sealed record SubD(Rhino.Geometry.SubD Value) : Regrown;
    public sealed record Brep(Rhino.Geometry.Brep Value) : Regrown;
    public sealed record PointCloud(Rhino.Geometry.PointCloud Value) : Regrown;
    public sealed record PointCloudPoints(Seq<Point3d> Values) : Regrown;
    public sealed record ClippingPlane(ClippingGrowth Value) : Regrown;
    public sealed record LinearDimension(Rhino.DocObjects.LinearDimension Value) : Regrown;
    public sealed record RadialDimension(Rhino.DocObjects.RadialDimension Value) : Regrown;
    public sealed record AngularDimension(Rhino.DocObjects.AngularDimension Value) : Regrown;
    public sealed record Leader(Rhino.DocObjects.Leader Value) : Regrown;
    public sealed record Hatch(Rhino.DocObjects.Hatch Value) : Regrown;
    public sealed record Text(TextEntity Value) : Regrown;
    public sealed record RawText(RawTextGrowth Value) : Regrown;
    public sealed record Instance(InstanceReferenceGeometry Value) : Regrown;

    public Fin<Regrown> Admit(Op? key = null) {
        Op op = key.OrDefault();
        return Switch(
            op,
            point: static (rail, value) => rail.AcceptInput(value.Value).Map(_ => (Regrown)value),
            dot: static (rail, value) => AdmitGeometry(value.Value, rail).Map(_ => (Regrown)value),
            line: static (rail, value) =>
                from start in rail.AcceptInput(value.Value.From)
                from end in rail.AcceptInput(value.Value.To)
                from _ in guard(start != end, rail.InvalidInput()).ToFin()
                select (Regrown)new Line(new LineGrowth(start, end)),
            polyline: static (rail, value) => guard(value.Values.Count >= 2, rail.InvalidInput()).ToFin()
                .Bind(_ => AdmitPoints(value.Values, rail))
                .Map(points => (Regrown)new Polyline(points)),
            arc: static (rail, value) => rail.AcceptInput(value.Value).Map(_ => (Regrown)value),
            circle: static (rail, value) => rail.AcceptInput(value.Value).Map(_ => (Regrown)value),
            ellipse: static (rail, value) => rail.AcceptInput(value.Value).Map(_ => (Regrown)value),
            sphere: static (rail, value) => rail.AcceptInput(value.Value).Map(_ => (Regrown)value),
            curve: static (rail, value) => AdmitGeometry(value.Value, rail).Map(_ => (Regrown)value),
            surface: static (rail, value) => AdmitGeometry(value.Value, rail).Map(_ => (Regrown)value),
            extrusion: static (rail, value) => AdmitGeometry(value.Value, rail).Map(_ => (Regrown)value),
            mesh: static (rail, value) => AdmitGeometry(value.Value, rail).Map(_ => (Regrown)value),
            subD: static (rail, value) => AdmitGeometry(value.Value, rail).Map(_ => (Regrown)value),
            brep: static (rail, value) => AdmitGeometry(value.Value, rail).Map(_ => (Regrown)value),
            pointCloud: static (rail, value) => AdmitGeometry(value.Value, rail).Map(_ => (Regrown)value),
            pointCloudPoints: static (rail, value) => guard(!value.Values.IsEmpty, rail.InvalidInput()).ToFin()
                .Bind(_ => AdmitPoints(value.Values, rail))
                .Map(points => (Regrown)new PointCloudPoints(points)),
            clippingPlane: static (rail, value) =>
                from frame in rail.AcceptInput(value.Value.Frame)
                from _ in guard(
                    double.IsFinite(value.Value.U) && value.Value.U > 0.0
                    && double.IsFinite(value.Value.V) && value.Value.V > 0.0
                    && !value.Value.Viewports.IsEmpty,
                    rail.InvalidInput()).ToFin()
                from ids in value.Value.Viewports.TraverseM(id => id != Guid.Empty
                    ? Fin.Succ(id)
                    : Fin.Fail<Guid>(rail.InvalidInput())).As()
                select (Regrown)new ClippingPlane(value.Value with { Frame = frame, Viewports = ids.Distinct() }),
            linearDimension: static (rail, value) => AdmitGeometry(value.Value, rail).Map(_ => (Regrown)value),
            radialDimension: static (rail, value) => AdmitGeometry(value.Value, rail).Map(_ => (Regrown)value),
            angularDimension: static (rail, value) => AdmitGeometry(value.Value, rail).Map(_ => (Regrown)value),
            leader: static (rail, value) => AdmitGeometry(value.Value, rail).Map(_ => (Regrown)value),
            hatch: static (rail, value) => AdmitGeometry(value.Value, rail).Map(_ => (Regrown)value),
            text: static (rail, value) => AdmitGeometry(value.Value, rail).Map(_ => (Regrown)value),
            rawText: static (rail, value) =>
                from text in rail.AcceptText(value.Value.Text)
                from frame in rail.AcceptInput(value.Value.Frame)
                from height in rail.Positive(value.Value.Height)
                from font in rail.AcceptText(value.Value.Font)
                select (Regrown)new RawText(value.Value with { Text = text, Frame = frame, Height = height, Font = font }),
            instance: static (rail, value) => AdmitGeometry(value.Value, rail).Map(_ => (Regrown)value));
    }

    internal Fin<Unit> Apply(ReplayHistoryResult result, Option<ObjectAttributes> attributes, Op op) {
        ObjectAttributes? nativeAttributes = attributes.Match<ObjectAttributes?>(
            Some: static value => value,
            None: static () => null);
        return op.Catch(() => op.Confirm(success: Switch(
            (Result: result, Attributes: nativeAttributes),
            point: static (context, value) => context.Result.UpdateToPoint(value.Value, context.Attributes),
            dot: static (context, value) => context.Result.UpdateToTextDot(value.Value, context.Attributes),
            line: static (context, value) => context.Result.UpdateToLine(value.Value.From, value.Value.To, context.Attributes),
            polyline: static (context, value) => context.Result.UpdateToPolyline(value.Values.AsIterable(), context.Attributes),
            arc: static (context, value) => context.Result.UpdateToArc(value.Value, context.Attributes),
            circle: static (context, value) => context.Result.UpdateToCircle(value.Value, context.Attributes),
            ellipse: static (context, value) => context.Result.UpdateToEllipse(value.Value, context.Attributes),
            sphere: static (context, value) => context.Result.UpdateToSphere(value.Value, context.Attributes),
            curve: static (context, value) => context.Result.UpdateToCurve(value.Value, context.Attributes),
            surface: static (context, value) => context.Result.UpdateToSurface(value.Value, context.Attributes),
            extrusion: static (context, value) => context.Result.UpdateToExtrusion(value.Value, context.Attributes),
            mesh: static (context, value) => context.Result.UpdateToMesh(value.Value, context.Attributes),
            subD: static (context, value) => context.Result.UpdateToSubD(value.Value, context.Attributes),
            brep: static (context, value) => context.Result.UpdateToBrep(value.Value, context.Attributes),
            pointCloud: static (context, value) => context.Result.UpdateToPointCloud(value.Value, context.Attributes),
            pointCloudPoints: static (context, value) => context.Result.UpdateToPointCloud(value.Values.AsIterable(), context.Attributes),
            clippingPlane: static (context, value) => context.Result.UpdateToClippingPlane(
                value.Value.Frame,
                value.Value.U,
                value.Value.V,
                value.Value.Viewports.AsIterable(),
                context.Attributes),
            linearDimension: static (context, value) => context.Result.UpdateToLinearDimension(value.Value, context.Attributes),
            radialDimension: static (context, value) => context.Result.UpdateToRadialDimension(value.Value, context.Attributes),
            angularDimension: static (context, value) => context.Result.UpdateToAngularDimension(value.Value, context.Attributes),
            leader: static (context, value) => context.Result.UpdateToLeader(value.Value, context.Attributes),
            hatch: static (context, value) => context.Result.UpdateToHatch(value.Value, context.Attributes),
            text: static (context, value) => context.Result.UpdateToText(value.Value, context.Attributes),
            rawText: static (context, value) => context.Result.UpdateToText(
                value.Value.Text,
                value.Value.Frame,
                value.Value.Height,
                value.Value.Font,
                value.Value.Bold,
                value.Value.Italic,
                value.Value.Justification,
                context.Attributes),
            instance: static (context, value) => context.Result.UpdateToInstanceReferenceGeometry(
                value.Value,
                context.Attributes))));
    }

    private static Fin<Seq<Point3d>> AdmitPoints(Seq<Point3d> values, Op op) =>
        values.TraverseM(value => op.AcceptInput(value)).As();

    private static Fin<Unit> AdmitGeometry(GeometryBase? value, Op op) =>
        guard(value is { IsValid: true }, op.InvalidInput()).ToFin();
}
```

## [06]-[REPLAY_PROGRAM]

- Owner: `ReplayRoster` `[Union]` closes preserve, append, and indexed-retain preparation; `ReplayFrame` is a stack-only read facade over `ReplayHistoryData`; `ReplayStep` returns one `Regrown` value without receiving a native result; `ReplayProgram` owns version, roster, and step.
- Law: the seam contract is strict `bool`. `true` means every prepared result updated; version mismatch or any fault returns `false`, and the authoring page's `ObjectsTelemetry` preserves the typed `Error` under the `FaultSite.Replay` site of its one callback-fault event.
- Law: the program lands on the policy row — `CommandPolicy.Replay` carries `program.Delegate`, so the command page's sealed `ReplayHistory` override routes here with zero adapter code and a replay-free command keeps the absent row.
- Law: the plan admits every regrow value before any result setter runs, settles the roster with complete prior-array compensation, then emits setters. Native replay commits only a successful strict-`bool` callback, so any setter refusal keeps existing geometry and drops the failed history link; appended results carry absent attributes.
- Boundary: `ReplayFrame` is `readonly ref struct` and enters `ReplayStep` as `scoped ref readonly`; only the sealed adapter delegate receives `ReplayHistoryData`, and no consumer callback receives `ReplayHistoryResult`.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReplayRoster {
    private ReplayRoster() { }
    public sealed record Preserve : ReplayRoster;
    public sealed record Append(int Count) : ReplayRoster;
    public sealed record Retain(Seq<int> Indices) : ReplayRoster;

    internal Fin<ReplayRoster> Admit(Op op) => Switch(
        op,
        preserve: static (_, value) => Fin.Succ<ReplayRoster>(value),
        append: static (key, value) => guard(value.Count > 0, key.InvalidInput()).ToFin().Map(_ => (ReplayRoster)value),
        retain: static (key, value) => guard(
            !value.Indices.IsEmpty && value.Indices.ForAll(static index => index >= 0),
            key.InvalidInput()).ToFin().Map(_ => (ReplayRoster)new Retain(value.Indices.Distinct())));

    internal Fin<Seq<int>> Staging(int existing, Op op) =>
        guard(existing >= 0, op.InvalidInput()).ToFin().Bind(_ => Switch(
            (Existing: existing, Op: op),
            preserve: static (context, _) => Fin.Succ(Range(count: context.Existing)),
            append: static (context, value) => context.Op.Catch(() =>
                Fin.Succ(Range(count: checked(context.Existing + value.Count)))),
            retain: static (context, value) => guard(
                value.Indices.ForAll(index => index < context.Existing),
                context.Op.MissingContext()).ToFin().Map(_ => value.Indices)));

    internal Fin<Unit> Apply(ReplayHistoryData data, Op op) => op.Catch(() => {
        ReplayHistoryResult[] prior = data.Results;
        Fin<Unit> mutation = Switch(
            (Data: data, Op: op),
            preserve: static (_, _) => Fin.Succ(unit),
            append: static (context, value) => Enumerable.Range(0, value.Count).AsIterable().ToSeq()
                .TraverseM(_ => context.Op.Catch(() => Optional(context.Data.AppendHistoryResult())
                    .ToFin(Fail: context.Op.InvalidResult()).Map(static _ => unit))).As()
                .Map(static _ => unit),
            retain: static (context, value) => context.Op.Catch(() => {
                ReplayHistoryResult[] current = context.Data.Results;
                return value.Indices.TraverseM(index => index < current.Length
                        ? Fin.Succ(current[index])
                        : Fin.Fail<ReplayHistoryResult>(context.Op.MissingContext())).As()
                    .Map(rows => {
                        context.Data.UpdateResultArray(newResults: rows.AsIterable());
                        return unit;
                    });
            }));
        return mutation.BindFail(primary => op.Catch(() => {
            data.UpdateResultArray(newResults: prior);
            return Fin.Succ(value: unit);
        }).Match(
            Succ: _ => Fin.Fail<Unit>(error: primary),
            Fail: cleanup => Fin.Fail<Unit>(error: primary + cleanup)));
    });

    private static Seq<int> Range(int count) =>
        Enumerable.Range(start: 0, count: count).AsIterable().ToSeq();
}

public readonly ref struct ReplayFrame {
    private readonly ReplayHistoryData data;

    internal ReplayFrame(ReplayHistoryData data) => this.data = data;

    public Guid RecordId => data.RecordId;
    public int Version => data.HistoryVersion;
    public Fin<SlotValue> Recover(SlotKey key, SlotValue shape, Op? op = null) => shape.Recover(data, key, op);
    public Fin<T> Use<T>(HistorySource source, Func<ObjRef, Fin<T>> body, Op? key = null) {
        Op op = key.OrDefault();
        return from active in op.Need(source)
               from run in op.Need(body)
               from result in active.Use(document: data.Document, body: run, op: op)
               select result;
    }
}

public delegate Fin<Regrown> ReplayStep(scoped ref readonly ReplayFrame frame, int index);

public sealed class ReplayProgram {
    private readonly ReplayRoster roster;
    private readonly ReplayStep regrow;

    private ReplayProgram(
        int version,
        ReplayRoster roster,
        ReplayStep regrow) {
        Version = version;
        this.roster = roster;
        this.regrow = regrow;
    }

    public int Version { get; }

    public static Fin<ReplayProgram> Of(
        int version,
        ReplayRoster roster,
        ReplayStep regrow) {
        Op op = Op.Of(name: nameof(ReplayProgram));
        return from _ in guard(version > 0, op.InvalidInput()).ToFin()
               from plan in op.Need(roster).Bind(value => value.Admit(op))
               from body in op.Need(regrow)
               select new ReplayProgram(version: version, roster: plan, regrow: body);
    }

    public Func<ReplayHistoryData, bool> Delegate => data => {
        Op op = Op.Of(name: nameof(ReplayProgram));
        return op.Catch(() => Drive(data: data, op: op)).Match(
            Succ: static replayed => replayed,
            Fail: error => {
                _ = ObjectsTelemetry.Publish(site: FaultSite.Replay, error: error);
                return false;
            });
    };

    private Fin<bool> Drive(ReplayHistoryData data, Op op) {
        ReplayProgram program = this;
        return from active in op.Need(data)
               from replayed in active.HistoryVersion != program.Version
                   ? Fin.Succ(value: false)
                   : from indices in program.roster.Staging(existing: active.Results.Length, op: op)
                     from staged in indices.TraverseM(index => op.Catch(() => {
                                 ReplayFrame frame = new(active);
                                 return program.regrow(in frame, index);
                             })
                             .Bind(proposed => proposed.Admit(op))).As()
                     from _ in program.roster.Apply(active, op)
                     from results in Fin.Succ(value: toSeq(active.Results))
                     from __ in guard(results.Count == staged.Count, op.InvalidResult()).ToFin()
                     from ___ in results
                         .Map(static (result, index) => (Result: result, Index: index))
                         .TraverseM(row => staged[row.Index].Apply(
                             result: row.Result,
                             attributes: Optional(row.Result.ExistingObject).Map(static held => held.Attributes),
                             op: op)).As()
                     select true
               select replayed;
    }
}

```

## [07]-[CHRONICLE]

- Owner: `BondOp` `[Union]` closes attach, detach, and replace-survival linkage; `WebAsk` separates document census from a targeted `HistoryWeb` row; `HistoryWeb` owns adjacency, order, cycles, transitive closure/reduction, and condensation projections; `HistoryConduct` `[SmartEnum<int>]` owns process switches.
- Law: targeted topology expands the reachable parent/child graph once. Edges orient parent to dependent; Blocks' `GraphFold` and `GraphProjection` own the order, cycle, closure, reduction, and condensation projections, and Chronicle composes them without re-deriving a fold.
- Law: conduct is process state, not document state — the rows drive `Rhino.ApplicationSettings.HistorySettings` statics (`RecordingEnabled`, `RecordNextCommand`, `UpdateEnabled`, `ObjectLockingEnabled`, `BrokenRecordWarningEnabled`), demand no session, and answer the post-write value; `RecordNextCommand` arms one command only, and `ObjectLockingEnabled` makes history-bearing outputs edit only through their inputs.
- Law: linkage mutates on the shared spine. `Bind` rides `ObjectSpine.Commit` with redraw `None`, resolves once, applies every bond, and returns affected ids plus the sealed undo serial as `ObjectReceipt<Guid>`.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BondOp {
    private BondOp() { }
    public sealed record Attach(Command Owner, HistoryScript Script) : BondOp;
    public sealed record Detach : BondOp;
    public sealed record Survival(ObjectSignal Signal) : BondOp;

    internal Fin<BondOp> Admit(Op op) =>
        Switch(
            op,
            attach: static (key, bond) =>
                from _ in key.Need(bond.Owner)
                from __ in key.Need(bond.Script)
                select (BondOp)bond,
            detach: static (_, bond) => Fin.Succ<BondOp>(bond),
            survival: static (key, bond) => key.Need(bond.Signal).Map(_ => (BondOp)bond));

    internal Fin<Unit> Apply(RhinoDoc document, RhinoObject native, Op op) =>
        Switch(
            (Document: document, Native: native, Op: op),
            attach: static (context, bond) => bond.Script.Mint(document: context.Document, owner: bond.Owner, key: context.Op)
                .Bind(lease => lease.Use(record => context.Op.Confirm(success: context.Native.SetHistory(history: record)))),
            detach: static (context, _) => context.Op.Confirm(success: context.Native.DeleteHistoryRecord()),
            survival: static (context, bond) => context.Op.Catch(() => context.Native.SetCopyHistoryOnReplace(bCopy: bond.Signal.On)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WebAsk {
    private WebAsk() { }
    public sealed record Targeted(TableTarget Target, HistoryWeb View) : WebAsk;
    public sealed record Census : WebAsk;
}

[SmartEnum<int>]
public sealed partial class HistoryWeb {
    public static readonly HistoryWeb Parents = new(key: 0, read: Chronicle.Parents);
    public static readonly HistoryWeb Children = new(key: 1, read: Chronicle.Children);
    public static readonly HistoryWeb Order = new(key: 2, read: Chronicle.Order);
    public static readonly HistoryWeb Loops = new(key: 3, read: Chronicle.Loops);
    public static readonly HistoryWeb Closure = new(key: 4, read: Chronicle.Closure);
    public static readonly HistoryWeb Reduction = new(key: 5, read: Chronicle.Reduction);
    public static readonly HistoryWeb Condensation = new(key: 6, read: Chronicle.Condensation);

    [UseDelegateFromConstructor]
    internal partial Fin<WebAnswer> Read(RhinoDoc document, TableTarget target, Op op);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WebAnswer : IDetachedDocumentResult {
    private WebAnswer() { }
    public sealed record Edges(Seq<(Guid Id, Seq<Guid> Linked)> Rows) : WebAnswer;
    public sealed record Ordered(Seq<Guid> UpdateOrder) : WebAnswer;
    public sealed record Groups(Seq<Seq<Guid>> Cyclic) : WebAnswer;
    public sealed record Condensed(Seq<Seq<Guid>> Components, Seq<(int From, int To)> Edges) : WebAnswer;
    public sealed record Count(int Records) : WebAnswer;
}

[SmartEnum<int>]
public sealed partial class HistoryConduct {
    public static readonly HistoryConduct Recording = Row(
        key: 0,
        read: static () => HistorySettings.RecordingEnabled,
        write: static value => HistorySettings.RecordingEnabled = value);
    public static readonly HistoryConduct RecordNext = Row(
        key: 1,
        read: static () => HistorySettings.RecordNextCommand,
        write: static value => HistorySettings.RecordNextCommand = value);
    public static readonly HistoryConduct Updating = Row(
        key: 2,
        read: static () => HistorySettings.UpdateEnabled,
        write: static value => HistorySettings.UpdateEnabled = value);
    public static readonly HistoryConduct Locking = Row(
        key: 3,
        read: static () => HistorySettings.ObjectLockingEnabled,
        write: static value => HistorySettings.ObjectLockingEnabled = value);
    public static readonly HistoryConduct BrokenWarning = Row(
        key: 4,
        read: static () => HistorySettings.BrokenRecordWarningEnabled,
        write: static value => HistorySettings.BrokenRecordWarningEnabled = value);

    [UseDelegateFromConstructor]
    internal partial bool ReadRaw();

    [UseDelegateFromConstructor]
    internal partial Unit WriteRaw(bool value);

    private static HistoryConduct Row(int key, Func<bool> read, Action<bool> write) =>
        new(
            key: key,
            readRaw: read,
            writeRaw: value => {
                write(value);
                return unit;
            });

    internal Fin<bool> Read(Op op) => op.Catch(() => Fin.Succ(value: ReadRaw()));

    internal Fin<Unit> Write(bool value, Op op) => op.Catch(() => Fin.Succ(value: WriteRaw(value)));

    internal Fin<T> Within<T>(ObjectSignal signal, Func<Fin<T>> body, Op op) =>
        Read(op).Bind(prior => Write(value: signal.On, op: op).Bind(_ => {
            Fin<T> primary = op.Catch(body);
            Fin<Unit> cleanup = Write(value: prior, op: op);
            return cleanup.Match(
                Succ: _ => primary,
                Fail: restore => primary.Match(
                    Succ: _ => Fin.Fail<T>(error: restore),
                    Fail: fault => Fin.Fail<T>(error: fault + restore)));
        }));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Chronicle {
    public static Fin<ObjectReceipt<Guid>> Bind(DocumentSession session, TableTarget target, BondOp bond) {
        Op op = Op.Of();
        return from active in op.Need(bond).Bind(value => value.Admit(op: op))
               from receipt in ObjectSpine.Commit(
                   session: session,
                   name: nameof(Chronicle),
                   redraw: RedrawPolicy.None,
                   fold: (document, key) => Objects.Resolve(document: document, target: target, key: key)
                       .Bind(natives => natives.TraverseM(native => active.Apply(document: document, native: native, op: key)
                           .Map(_ => native.Id)).As()),
                   op: op)
               select receipt;
    }

    public static Fin<WebAnswer> Ask(DocumentSession session, WebAsk ask) {
        Op op = Op.Of();
        return from active in op.Need(ask)
               from answer in session.Demand(
                   use: document => active.Switch(
                       (Document: document, Op: op),
                       targeted: static (ctx, query) =>
                           from target in ctx.Op.Need(query.Target)
                           from view in ctx.Op.Need(query.View)
                           from result in view.Read(document: ctx.Document, target: target, op: ctx.Op)
                           select result,
                       census: static (ctx, _) => ctx.Op.Catch(() =>
                           Fin.Succ(value: (WebAnswer)new WebAnswer.Count(Records: ctx.Document.Objects.HistoryRecordCount)))),
                   key: op,
                   needs: [SessionNeed.Read])
               select answer;
    }

    public static Fin<bool> Conduct(HistoryConduct row, Option<ObjectSignal> set = default) {
        Op op = Op.Of();
        return from active in op.Need(row)
               from _ in set.Traverse(signal => active.Write(value: signal.On, op: op)).As()
               from value in active.Read(op)
               select value;
    }

    public static Fin<T> Under<T>(HistoryConduct row, ObjectSignal signal, Func<Fin<T>> body) {
        Op op = Op.Of();
        return from active in op.Need(row)
               from state in op.Need(signal)
               from run in op.Need(body)
               from result in active.Within(signal: state, body: run, op: op)
               select result;
    }

    internal static Fin<WebAnswer> Parents(RhinoDoc document, TableTarget target, Op op) =>
        Adjacent(document: document, target: target, op: op, linked: static native => native.HistoryParents());

    internal static Fin<WebAnswer> Children(RhinoDoc document, TableTarget target, Op op) =>
        Adjacent(document: document, target: target, op: op, linked: static native => native.HistoryChildren());

    internal static Fin<WebAnswer> Order(RhinoDoc document, TableTarget target, Op op) =>
        Projected(document, target, op, static (graph, key) => GraphFold.Ordered(graph: graph, op: key)
            .Map(static ordered => (WebAnswer)new WebAnswer.Ordered(UpdateOrder: ordered)));

    internal static Fin<WebAnswer> Loops(RhinoDoc document, TableTarget target, Op op) =>
        Projected(document, target, op, static (graph, key) => key.Catch(() =>
            Fin.Succ(value: (WebAnswer)new WebAnswer.Groups(Cyclic: GraphFold.Cycles(graph: graph)))));

    internal static Fin<WebAnswer> Closure(RhinoDoc document, TableTarget target, Op op) =>
        Projected(document, target, op, static (graph, key) => GraphProjection.Closure.Project(graph, key).Map(Edges));

    internal static Fin<WebAnswer> Reduction(RhinoDoc document, TableTarget target, Op op) =>
        Projected(document, target, op, static (graph, key) => GraphFold.Reduced(graph: graph, op: key).Map(Edges));

    internal static Fin<WebAnswer> Condensation(RhinoDoc document, TableTarget target, Op op) =>
        Projected(document, target, op, static (graph, key) => key.Catch(() => {
            (Seq<Seq<Guid>> components, Seq<(int From, int To)> edges) = GraphFold.Condensed(graph: graph);
            return Fin.Succ(value: (WebAnswer)new WebAnswer.Condensed(Components: components, Edges: edges));
        }));

    private static Fin<WebAnswer> Adjacent(RhinoDoc document, TableTarget target, Op op, Func<RhinoObject, Guid[]> linked) =>
        from natives in Objects.Resolve(document: document, target: target, key: op)
        from rows in natives.TraverseM(native => op.Catch(() =>
            Fin.Succ(value: (native.Id, toSeq(linked(native)))))).As()
        select (WebAnswer)new WebAnswer.Edges(Rows: rows);

    private static Fin<WebAnswer> Projected(RhinoDoc document, TableTarget target, Op op, Func<Web, Op, Fin<WebAnswer>> project) =>
        from natives in Objects.Resolve(document: document, target: target, key: op)
        from graph in Woven(document: document, natives: natives, op: op)
        from answer in project(graph, op)
        select answer;

    private static WebAnswer Edges(Web graph) =>
        new WebAnswer.Edges(Rows: graph.Vertices.AsIterable().ToSeq()
            .Map(vertex => (vertex, graph.OutEdges(vertex).AsIterable().Map(static edge => edge.Target).ToSeq())));

    private static Fin<Web> Woven(RhinoDoc document, Seq<RhinoObject> natives, Op op) =>
        op.Catch(() => {
            Web graph = new(allowParallelEdges: false);
            Queue<Guid> pending = new(natives.Map(static native => native.Id).AsIterable());
            HashSet<Guid> visited = [];
            HashSet<(Guid Parent, Guid Child)> edges = [];
            while (pending.TryDequeue(out Guid id)) {
                if (!visited.Add(id)) { continue; }
                _ = graph.AddVertex(id);
                RhinoObject? native = document.Objects.FindId(id);
                if (native is null) { return Fin.Fail<Web>(op.MissingContext()); }
                foreach (Guid parent in native.HistoryParents()) {
                    if (edges.Add((parent, id))) { _ = graph.AddVerticesAndEdge(new SEdge<Guid>(parent, id)); }
                    pending.Enqueue(parent);
                }
                foreach (Guid child in native.HistoryChildren()) {
                    if (edges.Add((id, child))) { _ = graph.AddVerticesAndEdge(new SEdge<Guid>(id, child)); }
                    pending.Enqueue(child);
                }
            }
            return Fin.Succ(value: graph);
        });
}
```

## [08]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]           | [OWNER]          | [FORM]                                  | [ENTRY]          |
| :-----: | :------------------ | :--------------- | :-------------------------------------- | :--------------- |
|  [01]   | slot payloads       | `SlotValue`      | closed union with total native write    | `HistoryScript`  |
|  [02]   | record authoring    | `HistoryScript`  | leased mint from detached sources       | `Mint`           |
|  [03]   | slot recovery       | `SlotValue`      | generated write/read correspondence     | `Recover`        |
|  [04]   | geometry regrowth   | `Regrown`        | admitted generated update dispatch      | `Apply`          |
|  [05]   | replay body         | `ReplayProgram`  | strict-`bool` telemetry delegate        | `Delegate`       |
|  [06]   | linkage mutation    | `BondOp`         | shared-spine linkage union              | `Chronicle.Bind` |
|  [07]   | dependency topology | `HistoryWeb`     | delegate-column web projection          | `Chronicle.Ask`  |
|  [08]   | process governance  | `HistoryConduct` | scoped settings rows with restoration   | `Under`          |

## [09]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
