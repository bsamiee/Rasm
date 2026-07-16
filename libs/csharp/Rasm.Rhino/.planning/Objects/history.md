# [RASM_RHINO_OBJECTS_HISTORY]

Parametric history belongs to `Rasm.Rhino.Objects`. `HistoryScript` writes the complete catalogued slot family into one leased `HistoryRecord`; `SlotShape` reads the host-supported subset; `RegrowKind<T>` parameterizes the complete `ReplayHistoryResult.UpdateTo*` overload family; and `ReplayProgram` compiles preparation plus regrowth into the strict `Func<ReplayHistoryData, bool>` carried by `CommandPolicy.Replay`. `Chronicle` owns undo-recorded linkage, reachable dependency topology, cycle grouping through `CycleGroups`, and process-wide `HistorySettings` conduct.

## [01]-[INDEX]

- [02]-[SLOT_ALGEBRA]: `SlotKey`, `SlotValue`, and the write dispatch onto the native record.
- [03]-[SCRIPT]: `HistoryScript` — the single-use record mint and its threading law.
- [04]-[REPLAY_READS]: `SlotShape` — the typed reader rows over `ReplayHistoryData`.
- [05]-[REGROWN]: `RegrowKind<T>`/`Regrown` — typed row generation over the `UpdateTo*` family.
- [06]-[REPLAY_PROGRAM]: `ReplayProgram` — the compiled delegate and the strict-`bool` seam.
- [07]-[CHRONICLE]: `BondOp`, `WebAsk`, `HistoryConduct`, and the linkage entry.
- [08]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[SLOT_ALGEBRA]

- Owner: `SlotKey` `[ValueObject<int>]` closes non-negative slot identity. `SlotValue` `[Union]` covers every catalogued scalar, geometry, object-source, and plural setter payload.
- Law: the transform setter is the host's own misspelling — `SetTransorm` is the literal managed member (the native call underneath is spelled correctly), so the `Motion` arm names it verbatim and no local alias "fixes" it.
- Law: source slots arm replay — `Source` and `PointOnSource` register a dependency through `SetObjRef`/`SetPoint3dOnObject`, so editing the referenced object fires the replay; a script recording only value slots never re-fires on a geometry edit, which makes at least one source slot the practical floor for a history-aware command.
- Law: read asymmetry is payload law — plural readers exist only for ints, doubles, and guids, and no geometry reader exists at all, so geometry and the other plural payloads are write-only evidence: a replay program re-derives geometry from its source slots, and a plural point, vector, color, text, or bool payload that must read back encodes as scalar slots under consecutive keys.
- Growth: a new payload kind is one `SlotValue` case with its write arm and, where the host reads it, one `SlotShape` row.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Generic;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Domain;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;

namespace Rasm.Rhino.Objects;

// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<int>]
public readonly partial struct SlotKey {
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
    public sealed record Source(ObjRef Value) : SlotValue;
    public sealed record PointOnSource(ObjRef Value, Point3d At) : SlotValue;
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
            context: op,
            toggle: static (_, slot) => Fin.Succ<SlotValue>(slot),
            count: static (_, slot) => Fin.Succ<SlotValue>(slot),
            scalar: static (_, slot) => Fin.Succ<SlotValue>(slot),
            point: static (key, slot) => key.AcceptInput(value: slot.Value).Map(_ => (SlotValue)slot),
            direction: static (key, slot) => key.AcceptInput(value: slot.Value).Map(_ => (SlotValue)slot),
            motion: static (key, slot) => key.AcceptInput(value: slot.Value).Map(_ => (SlotValue)slot),
            paint: static (_, slot) => Fin.Succ<SlotValue>(slot),
            id: static (_, slot) => Fin.Succ<SlotValue>(slot),
            text: static (key, slot) => Optional(slot.Value).ToFin(Fail: key.InvalidInput()).Map(_ => (SlotValue)slot),
            curveSlot: static (key, slot) => Optional(slot.Value).ToFin(Fail: key.InvalidInput()).Map(_ => (SlotValue)slot),
            surfaceSlot: static (key, slot) => Optional(slot.Value).ToFin(Fail: key.InvalidInput()).Map(_ => (SlotValue)slot),
            brepSlot: static (key, slot) => Optional(slot.Value).ToFin(Fail: key.InvalidInput()).Map(_ => (SlotValue)slot),
            meshSlot: static (key, slot) => Optional(slot.Value).ToFin(Fail: key.InvalidInput()).Map(_ => (SlotValue)slot),
            source: static (key, slot) => Optional(slot.Value).ToFin(Fail: key.InvalidInput()).Map(_ => (SlotValue)slot),
            pointOnSource: static (key, slot) =>
                from source in Optional(slot.Value).ToFin(Fail: key.InvalidInput())
                from point in key.AcceptInput(value: slot.At)
                select (SlotValue)new PointOnSource(Value: source, At: point),
            toggles: static (_, slot) => Fin.Succ<SlotValue>(slot),
            counts: static (_, slot) => Fin.Succ<SlotValue>(slot),
            scalars: static (_, slot) => Fin.Succ<SlotValue>(slot),
            points: static (_, slot) => Fin.Succ<SlotValue>(slot),
            directions: static (_, slot) => Fin.Succ<SlotValue>(slot),
            paints: static (_, slot) => Fin.Succ<SlotValue>(slot),
            ids: static (_, slot) => Fin.Succ<SlotValue>(slot),
            texts: static (key, slot) => slot.Values.TraverseM(value => Optional(value).ToFin(Fail: key.InvalidInput())).As()
                .Map(values => (SlotValue)new Texts(Values: values)));

    internal Fin<Unit> Write(HistoryRecord record, SlotKey key, Op op) =>
        op.Catch(() => Switch(
            context: (Record: record, Id: (int)key, Op: op),
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
            source: static (context, slot) => context.Op.Confirm(success: context.Record.SetObjRef(id: context.Id, value: slot.Value)),
            pointOnSource: static (context, slot) => context.Op.Confirm(
                success: context.Record.SetPoint3dOnObject(id: context.Id, objref: slot.Value, value: slot.At)),
            toggles: static (context, slot) => context.Op.Confirm(success: context.Record.SetBools(id: context.Id, values: slot.Values.AsIterable())),
            counts: static (context, slot) => context.Op.Confirm(success: context.Record.SetInts(id: context.Id, values: slot.Values.AsIterable())),
            scalars: static (context, slot) => context.Op.Confirm(success: context.Record.SetDoubles(id: context.Id, values: slot.Values.AsIterable())),
            points: static (context, slot) => context.Op.Confirm(success: context.Record.SetPoint3ds(id: context.Id, values: slot.Values.AsIterable())),
            directions: static (context, slot) => context.Op.Confirm(success: context.Record.SetVector3ds(id: context.Id, values: slot.Values.AsIterable())),
            paints: static (context, slot) => context.Op.Confirm(success: context.Record.SetColors(id: context.Id, values: slot.Values.AsIterable())),
            ids: static (context, slot) => context.Op.Confirm(success: context.Record.SetGuids(id: context.Id, values: slot.Values.AsIterable())),
            texts: static (context, slot) => context.Op.Confirm(success: context.Record.SetStrings(id: context.Id, values: slot.Values.AsIterable()))));
}
```

## [03]-[SCRIPT]

- Owner: `HistoryScript` — the whole authoring intent as one value: version, replace-survival grant, and the keyed slot sequence.
- Law: the mint is single-use — `Mint` constructs the native record against the owning `Command`, writes every slot, stamps `CopyOnReplaceObject`, and answers an owned lease. A mid-write refusal disposes the half-written record. One lease threads into `TableOp.Add`, `TableOp.Cloud`, block placement, or `BondOp.Attach`, then disposes after the host copies it.
- Law: the version is the compatibility key — `ReplayProgram` folds a mismatched `HistoryVersion` to graceful non-replay, so bumping the script version retires stale records without faulting old documents.
- Boundary: the record constructor keys on `Command.Id`, so scripts mint inside the command that will replay them; the command page's adapter is the only `ReplayHistory` override site.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
public sealed class HistoryScript {
    private HistoryScript(int version, bool copyOnReplace, Seq<(SlotKey Key, SlotValue Value)> slots) {
        Version = version;
        CopyOnReplace = copyOnReplace;
        Slots = slots;
    }

    public int Version { get; }
    public bool CopyOnReplace { get; }
    public Seq<(SlotKey Key, SlotValue Value)> Slots { get; }

    public static Fin<HistoryScript> Of(int version, bool copyOnReplace, params ReadOnlySpan<(SlotKey Key, SlotValue Value)> slots) {
        Op op = Op.Of(name: nameof(HistoryScript));
        return from _ in guard(version > 0, op.InvalidInput()).ToFin()
               from rows in toSeq(slots.ToArray()).TraverseM(slot => Optional(slot.Value).ToFin(Fail: op.InvalidInput())
                   .Bind(value => value.Admit(op: op))
                   .Map(value => (slot.Key, value))).As()
               from __ in guard(!rows.IsEmpty && rows.Map(static slot => slot.Key).Distinct().Count == rows.Count, op.InvalidInput()).ToFin()
               select new HistoryScript(version: version, copyOnReplace: copyOnReplace, slots: rows);
    }

    public Fin<Lease<HistoryRecord>> Mint(Command owner, Op? key = null) {
        Op op = key.OrDefault();
        return from command in Optional(owner).ToFin(Fail: op.InvalidInput())
               from record in op.Catch(() => Optional(new HistoryRecord(command: command, version: Version)).ToFin(Fail: op.InvalidResult()))
               from lease in Slots
                   .TraverseM(slot => slot.Value.Write(record: record, key: slot.Key, op: op)).As()
                   .Bind(_ => op.Catch(() => {
                       record.CopyOnReplaceObject = CopyOnReplace;
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

- Owner: `SlotShape` `[SmartEnum<int>]` — one reader row per payload the host reads back, each a `[UseDelegateFromConstructor]` delegate over the matching `TryGet*` member answering the same `SlotValue` case the script wrote, so write and read share one payload vocabulary.
- Law: a missing or mistyped slot is absence with cause — every `TryGet*` answers `false` for both, the row lifts it to a typed fault carrying the asked shape, and a program distinguishing "never recorded" from "recorded differently" re-reads under the sibling shape.
- Law: sources recover live — the `Origin` row lifts `GetRhinoObjRef` so the replay body reaches the current state of the recorded object; the returned `ObjRef` lives only inside the replay callback, because the host disposes the whole `ReplayHistoryData` the moment the override returns.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SlotShape {
    public static readonly SlotShape Toggle = new(key: 0, read: static (data, id, op) =>
        data.TryGetBool(id: id, value: out bool value) ? Fin.Succ<SlotValue>(value: new SlotValue.Toggle(Value: value)) : Fin.Fail<SlotValue>(error: op.MissingContext()));
    public static readonly SlotShape Count = new(key: 1, read: static (data, id, op) =>
        data.TryGetInt(id: id, value: out int value) ? Fin.Succ<SlotValue>(value: new SlotValue.Count(Value: value)) : Fin.Fail<SlotValue>(error: op.MissingContext()));
    public static readonly SlotShape Counts = new(key: 2, read: static (data, id, op) =>
        data.TryGetInts(id: id, values: out int[] values) ? Fin.Succ<SlotValue>(value: new SlotValue.Counts(Values: toSeq(values))) : Fin.Fail<SlotValue>(error: op.MissingContext()));
    public static readonly SlotShape Scalar = new(key: 3, read: static (data, id, op) =>
        data.TryGetDouble(id: id, value: out double value) ? Fin.Succ<SlotValue>(value: new SlotValue.Scalar(Value: value)) : Fin.Fail<SlotValue>(error: op.MissingContext()));
    public static readonly SlotShape Scalars = new(key: 4, read: static (data, id, op) =>
        data.TryGetDoubles(id: id, values: out double[] values) ? Fin.Succ<SlotValue>(value: new SlotValue.Scalars(Values: toSeq(values))) : Fin.Fail<SlotValue>(error: op.MissingContext()));
    public static readonly SlotShape Point = new(key: 5, read: static (data, id, op) =>
        data.TryGetPoint3d(id: id, value: out Point3d value) ? Fin.Succ<SlotValue>(value: new SlotValue.Point(Value: value)) : Fin.Fail<SlotValue>(error: op.MissingContext()));
    public static readonly SlotShape PointOnSource = new(key: 6, read: static (data, id, op) =>
        data.TryGetPoint3dOnObject(id: id, value: out Point3d value)
            ? Optional(data.GetRhinoObjRef(id: id)).ToFin(Fail: op.MissingContext())
                .Map(source => (SlotValue)new SlotValue.PointOnSource(Value: source, At: value))
            : Fin.Fail<SlotValue>(error: op.MissingContext()));
    public static readonly SlotShape Direction = new(key: 7, read: static (data, id, op) =>
        data.TryGetVector3d(id: id, value: out Vector3d value) ? Fin.Succ<SlotValue>(value: new SlotValue.Direction(Value: value)) : Fin.Fail<SlotValue>(error: op.MissingContext()));
    public static readonly SlotShape Motion = new(key: 8, read: static (data, id, op) =>
        data.TryGetTransform(id: id, value: out Transform value) ? Fin.Succ<SlotValue>(value: new SlotValue.Motion(Value: value)) : Fin.Fail<SlotValue>(error: op.MissingContext()));
    public static readonly SlotShape Paint = new(key: 9, read: static (data, id, op) =>
        data.TryGetColor(id: id, value: out System.Drawing.Color value) ? Fin.Succ<SlotValue>(value: new SlotValue.Paint(Value: value)) : Fin.Fail<SlotValue>(error: op.MissingContext()));
    public static readonly SlotShape Id = new(key: 10, read: static (data, id, op) =>
        data.TryGetGuid(id: id, value: out Guid value) ? Fin.Succ<SlotValue>(value: new SlotValue.Id(Value: value)) : Fin.Fail<SlotValue>(error: op.MissingContext()));
    public static readonly SlotShape Ids = new(key: 11, read: static (data, id, op) =>
        data.TryGetGuids(id: id, values: out Guid[] values) ? Fin.Succ<SlotValue>(value: new SlotValue.Ids(Values: toSeq(values))) : Fin.Fail<SlotValue>(error: op.MissingContext()));
    public static readonly SlotShape Text = new(key: 12, read: static (data, id, op) =>
        data.TryGetString(id: id, value: out string value) ? Fin.Succ<SlotValue>(value: new SlotValue.Text(Value: value)) : Fin.Fail<SlotValue>(error: op.MissingContext()));
    public static readonly SlotShape Origin = new(key: 13, read: static (data, id, op) =>
        Optional(data.GetRhinoObjRef(id: id)).ToFin(Fail: op.MissingContext())
            .Map(source => (SlotValue)new SlotValue.Source(Value: source)));

    [UseDelegateFromConstructor]
    internal partial Fin<SlotValue> Read(ReplayHistoryData data, int id, Op op);

    public Fin<SlotValue> Recover(ReplayHistoryData data, SlotKey key, Op? keyOp = null) {
        Op op = keyOp.OrDefault();
        return from active in Optional(data).ToFin(Fail: op.InvalidInput())
               from value in op.Catch(() => Read(data: active, id: (int)key, op: op))
               select value;
    }
}
```

## [05]-[REGROWN]

- Owner: `RegrowKind<T>` is the typed overload row; `RegrowKinds` is the complete catalogued row table; `Regrown` erases `T` only after `Of<T>` proves the row/payload pair. `LineGrowth`, `ClippingGrowth`, and `RawTextGrowth` carry the only multi-field payloads.
- Law: replay updates the existing result. Every row delegates to one verified `ReplayHistoryResult.UpdateTo*` overload with the existing object's attributes; no add or replace path exists.
- Law: clipping-plane arity collapses onto the plural overload. One viewport is a one-element sequence, and empty viewport rosters are refused at admission.
- Growth: a host overload adds one `RegrowKind<T>` row; dispatch, replay signatures, and consumers remain unchanged.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
public readonly record struct LineGrowth(Point3d From, Point3d To);
public readonly record struct ClippingGrowth(Plane Frame, double U, double V, Seq<Guid> Viewports);
public readonly record struct RawTextGrowth(string Text, Plane Frame, double Height, string Font, bool Bold, bool Italic, TextJustification Justification);

public sealed class RegrowKind<T> where T : notnull {
    private readonly Func<ReplayHistoryResult, T, ObjectAttributes, bool> update;
    private readonly Option<Func<T, Op, Fin<T>>> admit;

    internal RegrowKind(
        Func<ReplayHistoryResult, T, ObjectAttributes, bool> update,
        Option<Func<T, Op, Fin<T>>> admit = default) {
        this.update = update;
        this.admit = admit;
    }

    internal Fin<T> Admit(T value, Op key) => admit.Match(
        Some: validate => validate(value, key),
        None: () => Fin.Succ(value: value));

    internal Fin<Unit> Apply(ReplayHistoryResult result, T value, ObjectAttributes attributes, Op key) =>
        key.Catch(() => key.Confirm(success: update(result, value, attributes)));
}

public static class RegrowKinds {
    public static RegrowKind<Point3d> Point { get; } = new(static (result, value, attributes) => result.UpdateToPoint(point: value, attributes: attributes));
    public static RegrowKind<TextDot> Dot { get; } = new(static (result, value, attributes) => result.UpdateToTextDot(dot: value, attributes: attributes));
    public static RegrowKind<LineGrowth> Line { get; } = new(static (result, value, attributes) => result.UpdateToLine(from: value.From, to: value.To, attributes: attributes));
    public static RegrowKind<Seq<Point3d>> Polyline { get; } = new(
        update: static (result, value, attributes) => result.UpdateToPolyline(points: value.AsIterable(), attributes: attributes),
        admit: static (value, key) => guard(!value.IsEmpty, key.InvalidInput()).ToFin().Map(_ => value));
    public static RegrowKind<Arc> Arc { get; } = new(static (result, value, attributes) => result.UpdateToArc(arc: value, attributes: attributes));
    public static RegrowKind<Circle> Circle { get; } = new(static (result, value, attributes) => result.UpdateToCircle(circle: value, attributes: attributes));
    public static RegrowKind<Ellipse> Ellipse { get; } = new(static (result, value, attributes) => result.UpdateToEllipse(ellipse: value, attributes: attributes));
    public static RegrowKind<Sphere> Sphere { get; } = new(static (result, value, attributes) => result.UpdateToSphere(sphere: value, attributes: attributes));
    public static RegrowKind<Curve> Curve { get; } = new(static (result, value, attributes) => result.UpdateToCurve(curve: value, attributes: attributes));
    public static RegrowKind<Surface> Surface { get; } = new(static (result, value, attributes) => result.UpdateToSurface(surface: value, attributes: attributes));
    public static RegrowKind<Extrusion> Extrusion { get; } = new(static (result, value, attributes) => result.UpdateToExtrusion(extrusion: value, attributes: attributes));
    public static RegrowKind<Mesh> Mesh { get; } = new(static (result, value, attributes) => result.UpdateToMesh(mesh: value, attributes: attributes));
    public static RegrowKind<SubD> SubD { get; } = new(static (result, value, attributes) => result.UpdateToSubD(subD: value, attributes: attributes));
    public static RegrowKind<Brep> Brep { get; } = new(static (result, value, attributes) => result.UpdateToBrep(brep: value, attributes: attributes));
    public static RegrowKind<PointCloud> PointCloud { get; } = new(static (result, value, attributes) => result.UpdateToPointCloud(cloud: value, attributes: attributes));
    public static RegrowKind<Seq<Point3d>> PointCloudPoints { get; } = new(
        update: static (result, value, attributes) => result.UpdateToPointCloud(points: value.AsIterable(), attributes: attributes),
        admit: static (value, key) => guard(!value.IsEmpty, key.InvalidInput()).ToFin().Map(_ => value));
    public static RegrowKind<ClippingGrowth> ClippingPlane { get; } = new(
        update: static (result, value, attributes) => result.UpdateToClippingPlane(
            plane: value.Frame, uMagnitude: value.U, vMagnitude: value.V,
            clippedViewportIds: value.Viewports.AsIterable(), attributes: attributes),
        admit: static (value, key) =>
            from frame in key.AcceptInput(value: value.Frame)
            from _ in guard(value.U > 0.0 && value.V > 0.0 && !value.Viewports.IsEmpty, key.InvalidInput()).ToFin()
            from ids in value.Viewports.TraverseM(id => id != Guid.Empty
                ? Fin.Succ(value: id)
                : Fin.Fail<Guid>(error: key.InvalidInput())).As()
            select value with { Frame = frame, Viewports = ids.Distinct() });
    public static RegrowKind<LinearDimension> LinearDimension { get; } = new(static (result, value, attributes) => result.UpdateToLinearDimension(dimension: value, attributes: attributes));
    public static RegrowKind<RadialDimension> RadialDimension { get; } = new(static (result, value, attributes) => result.UpdateToRadialDimension(dimension: value, attributes: attributes));
    public static RegrowKind<AngularDimension> AngularDimension { get; } = new(static (result, value, attributes) => result.UpdateToAngularDimension(dimension: value, attributes: attributes));
    public static RegrowKind<Leader> Leader { get; } = new(static (result, value, attributes) => result.UpdateToLeader(leader: value, attributes: attributes));
    public static RegrowKind<Hatch> Hatch { get; } = new(static (result, value, attributes) => result.UpdateToHatch(hatch: value, attributes: attributes));
    public static RegrowKind<TextEntity> Text { get; } = new(static (result, value, attributes) => result.UpdateToText(text: value, attributes: attributes));
    public static RegrowKind<RawTextGrowth> RawText { get; } = new(
        update: static (result, value, attributes) => result.UpdateToText(
            text: value.Text, plane: value.Frame, height: value.Height, fontName: value.Font,
            bold: value.Bold, italic: value.Italic, justification: value.Justification, attributes: attributes),
        admit: static (value, key) =>
            from text in key.AcceptText(value: value.Text)
            from frame in key.AcceptInput(value: value.Frame)
            from height in key.Positive(value: value.Height)
            from font in key.AcceptText(value: value.Font)
            select value with { Text = text, Frame = frame, Height = height, Font = font });
    public static RegrowKind<InstanceReferenceGeometry> Instance { get; } = new(static (result, value, attributes) => result.UpdateToInstanceReferenceGeometry(
        instanceReference: value, attributes: attributes));
}

public sealed class Regrown {
    private readonly Func<ReplayHistoryResult, ObjectAttributes, Op, Fin<Unit>> apply;

    private Regrown(Func<ReplayHistoryResult, ObjectAttributes, Op, Fin<Unit>> apply) => this.apply = apply;

    public static Fin<Regrown> Of<T>(RegrowKind<T> kind, T value, Op? key = null) where T : notnull {
        Op op = key.OrDefault();
        return from row in Optional(kind).ToFin(Fail: op.InvalidInput())
               from payload in Optional(value).ToFin(Fail: op.InvalidInput())
               from admitted in row.Admit(value: payload, key: op)
               select new Regrown(apply: (result, attributes, rail) => row.Apply(
                   result: result,
                   value: admitted,
                   attributes: attributes,
                   key: rail));
    }

    internal Fin<Unit> Apply(ReplayHistoryResult result, ObjectAttributes attributes, Op op) =>
        apply(result, attributes, op);
}
```

## [06]-[REPLAY_PROGRAM]

- Owner: `ReplayProgram` admits an expected history version, an optional result-roster preparation fold, and one per-result regrow function. Preparation may call `AppendHistoryResult` or `UpdateResultArray`; traversal snapshots `Results` only after preparation succeeds.
- Law: the seam contract is strict `bool`. `true` means every prepared result updated; version mismatch or any fault returns `false`. Faults log before returning because the host converts a thrown override into an opaque zero and exposes no cancel outcome.
- Law: the program lands on the policy row — `CommandPolicy.Replay` carries `program.Delegate`, so the command page's sealed `ReplayHistory` override routes here with zero adapter code and a replay-free command keeps the absent row.
- Law: result growth precedes regrowth. Preparation settles the roster once, then the driver indexes that settled array; mutation during per-result traversal has no path.
- Boundary: every `ObjRef`, result, and reader lives only inside the callback — the host constructs `ReplayHistoryData` in a `using` scope and disposes it the moment the override returns, so nothing recovered here survives outward.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
public sealed class ReplayProgram {
    private readonly Option<Func<ReplayHistoryData, Fin<Unit>>> prepare;
    private readonly Func<ReplayHistoryData, ReplayHistoryResult, int, Fin<Regrown>> regrow;

    private ReplayProgram(
        int version,
        Option<Func<ReplayHistoryData, Fin<Unit>>> prepare,
        Func<ReplayHistoryData, ReplayHistoryResult, int, Fin<Regrown>> regrow) {
        Version = version;
        this.prepare = prepare;
        this.regrow = regrow;
    }

    public int Version { get; }

    public static Fin<ReplayProgram> Of(
        int version,
        Func<ReplayHistoryData, ReplayHistoryResult, int, Fin<Regrown>> regrow,
        Option<Func<ReplayHistoryData, Fin<Unit>>> prepare = default) {
        Op op = Op.Of(name: nameof(ReplayProgram));
        return from _ in guard(version > 0, op.InvalidInput()).ToFin()
               from body in Optional(regrow).ToFin(Fail: op.InvalidInput())
               from setup in prepare.Traverse(value => Optional(value).ToFin(Fail: op.InvalidInput())).As()
               select new ReplayProgram(version: version, prepare: setup, regrow: body);
    }

    public Func<ReplayHistoryData, bool> Delegate => data => {
        Op op = Op.Of(name: nameof(ReplayProgram));
        return op.Catch(() => Drive(data: data, op: op)).Match(
            Succ: static replayed => replayed,
            Fail: error => {
                RhinoApp.WriteLine(message: error.Message);
                return false;
            });
    };

    private Fin<bool> Drive(ReplayHistoryData data, Op op) {
        ReplayProgram program = this;
        return from active in Optional(data).ToFin(Fail: op.InvalidInput())
               from replayed in active.HistoryVersion != program.Version
                   ? Fin.Succ(value: false)
                   : from _ in program.prepare.Match(
                         Some: arrange => op.Catch(() => arrange(active)),
                         None: () => Fin.Succ(value: unit))
                     from results in toSeq(active.Results)
                         .Map(static (result, index) => (Result: result, Index: index))
                         .TraverseM(row =>
                             from shape in op.Catch(() => program.regrow(active, row.Result, row.Index))
                             from existing in Optional(row.Result.ExistingObject).ToFin(Fail: op.MissingContext())
                             from __ in shape.Apply(result: row.Result, attributes: existing.Attributes, op: op)
                             select unit).As()
                     select true
               select replayed;
    }
}
```

## [07]-[CHRONICLE]

- Owner: `BondOp` `[Union]` closes attach, detach, and replace-survival linkage. `WebAsk` distinguishes a document-wide census from a targeted `HistoryWeb` row; the target parameter exists only where resolution needs it. `HistoryConduct` `[SmartEnum<int>]` owns the catalogued process switches.
- Law: targeted order and loop reads expand the full reachable parent/child closure from their seeds. Edges orient parent to dependent, source-first topological order handles acyclic closure, and `CycleGroups.Of` handles strongly connected groups without a second SCC implementation.
- Law: conduct is process state, not document state — the rows drive `Rhino.ApplicationSettings.HistorySettings` statics (`RecordingEnabled`, `RecordNextCommand`, `UpdateEnabled`, `ObjectLockingEnabled`, `BrokenRecordWarningEnabled`), demand no session, and answer the post-write value; `RecordNextCommand` arms one command only, and `ObjectLockingEnabled` makes history-bearing outputs edit only through their inputs.
- Law: linkage mutates inside one undo-capable grant. `Bind` resolves once, applies every bond under `UndoBracket`, and returns affected ids plus the sealed undo serial.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BondOp {
    private BondOp() { }
    public sealed record Attach(Command Owner, HistoryScript Script) : BondOp;
    public sealed record Detach : BondOp;
    public sealed record Survival(bool CopyOnReplace) : BondOp;

    internal Fin<BondOp> Admit(Op op) =>
        Switch(
            context: op,
            attach: static (key, bond) =>
                from _ in Optional(bond.Owner).ToFin(Fail: key.InvalidInput())
                from __ in Optional(bond.Script).ToFin(Fail: key.InvalidInput())
                select (BondOp)bond,
            detach: static (_, bond) => Fin.Succ<BondOp>(bond),
            survival: static (_, bond) => Fin.Succ<BondOp>(bond));

    internal Fin<Unit> Apply(RhinoObject native, Op op) =>
        Switch(
            context: (Native: native, Op: op),
            attach: static (context, bond) => bond.Script.Mint(owner: bond.Owner, key: context.Op)
                .Bind(lease => lease.Use(record => context.Op.Confirm(success: context.Native.SetHistory(history: record)))),
            detach: static (context, _) => context.Op.Confirm(success: context.Native.DeleteHistoryRecord()),
            survival: static (context, bond) => context.Op.Catch(() => {
                context.Native.SetCopyHistoryOnReplace(bCopy: bond.CopyOnReplace);
                return Fin.Succ(value: unit);
            }));
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

    [UseDelegateFromConstructor]
    internal partial Fin<WebAnswer> Read(RhinoDoc document, TableTarget target, Op op);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WebAnswer : IDetachedDocumentResult {
    private WebAnswer() { }
    public sealed record Edges(Seq<(Guid Id, Seq<Guid> Linked)> Rows) : WebAnswer;
    public sealed record Ordered(Seq<Guid> UpdateOrder) : WebAnswer;
    public sealed record Groups(Seq<Seq<Guid>> Cyclic) : WebAnswer;
    public sealed record Count(int Records) : WebAnswer;
}

public readonly record struct HistoryReceipt(Seq<Guid> Ids, Option<uint> UndoSerial) : IDetachedDocumentResult;

[SmartEnum<int>]
public sealed partial class HistoryConduct {
    public static readonly HistoryConduct Recording = new(key: 0,
        read: static () => HistorySettings.RecordingEnabled,
        write: static value => { HistorySettings.RecordingEnabled = value; return unit; });
    public static readonly HistoryConduct RecordNext = new(key: 1,
        read: static () => HistorySettings.RecordNextCommand,
        write: static value => { HistorySettings.RecordNextCommand = value; return unit; });
    public static readonly HistoryConduct Updating = new(key: 2,
        read: static () => HistorySettings.UpdateEnabled,
        write: static value => { HistorySettings.UpdateEnabled = value; return unit; });
    public static readonly HistoryConduct Locking = new(key: 3,
        read: static () => HistorySettings.ObjectLockingEnabled,
        write: static value => { HistorySettings.ObjectLockingEnabled = value; return unit; });
    public static readonly HistoryConduct BrokenWarning = new(key: 4,
        read: static () => HistorySettings.BrokenRecordWarningEnabled,
        write: static value => { HistorySettings.BrokenRecordWarningEnabled = value; return unit; });

    [UseDelegateFromConstructor]
    internal partial bool Read();

    [UseDelegateFromConstructor]
    internal partial Unit Write(bool value);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Chronicle {
    public static Fin<HistoryReceipt> Bind(DocumentSession session, TableTarget target, BondOp bond) {
        Op op = Op.Of();
        return from active in Optional(bond).ToFin(Fail: op.InvalidInput()).Bind(value => value.Admit(op: op))
               from receipt in session.Demand(
                   use: document => op.Catch(() => {
                       using UndoBracket undo = UndoBracket.Begin(
                           document: document,
                           name: nameof(Chronicle),
                           recordsUndo: true);
                       Fin<HistoryReceipt> outcome = guard(undo.Admitted, op.InvalidResult()).ToFin()
                           .Bind(_ => Objects.Resolve(document: document, target: target, key: op))
                           .Bind(natives => natives.TraverseM(native => active.Apply(native: native, op: op)
                               .Map(_ => native.Id)).As())
                           .Map(ids => new HistoryReceipt(Ids: ids, UndoSerial: Option<uint>.None));
                       return undo.Seal(
                           outcome: outcome,
                           stamp: static (value, serial) => value with {
                               UndoSerial = serial > 0u ? Some(serial) : Option<uint>.None,
                           },
                           key: op);
                   }),
                   key: op,
                   needs: [SessionNeed.Mutate, SessionNeed.Undo])
               select receipt;
    }

    public static Fin<WebAnswer> Ask(DocumentSession session, WebAsk ask) {
        Op op = Op.Of();
        return from active in Optional(ask).ToFin(Fail: op.InvalidInput())
               from answer in session.Demand(
                   use: document => active.Switch(
                       state: (Document: document, Op: op),
                       targeted: static (ctx, query) =>
                           from target in Optional(query.Target).ToFin(Fail: ctx.Op.InvalidInput())
                           from view in Optional(query.View).ToFin(Fail: ctx.Op.InvalidInput())
                           from result in view.Read(document: ctx.Document, target: target, op: ctx.Op)
                           select result,
                       census: static (ctx, _) => ctx.Op.Catch(() =>
                           Fin.Succ(value: (WebAnswer)new WebAnswer.Count(Records: ctx.Document.Objects.HistoryRecordCount))),
                   key: op,
                   needs: [SessionNeed.Read])
               select answer;
    }

    public static Fin<bool> Conduct(HistoryConduct row, Option<bool> set = default) {
        Op op = Op.Of();
        return from active in Optional(row).ToFin(Fail: op.InvalidInput())
               from _ in op.Catch(() => Fin.Succ(value: set.Map(active.Write).IfNone(noneValue: unit)))
               from value in op.Catch(() => Fin.Succ(value: active.Read()))
               select value;
    }

    internal static Fin<WebAnswer> Parents(RhinoDoc document, TableTarget target, Op op) =>
        from natives in Objects.Resolve(document: document, target: target, key: op)
        from rows in natives.TraverseM(native => op.Catch(() =>
            Fin.Succ(value: (native.Id, toSeq(native.HistoryParents()))))).As()
        select (WebAnswer)new WebAnswer.Edges(Rows: rows);

    internal static Fin<WebAnswer> Children(RhinoDoc document, TableTarget target, Op op) =>
        from natives in Objects.Resolve(document: document, target: target, key: op)
        from rows in natives.TraverseM(native => op.Catch(() =>
            Fin.Succ(value: (native.Id, toSeq(native.HistoryChildren()))))).As()
        select (WebAnswer)new WebAnswer.Edges(Rows: rows);

    internal static Fin<WebAnswer> Order(RhinoDoc document, TableTarget target, Op op) =>
        from natives in Objects.Resolve(document: document, target: target, key: op)
        from graph in Woven(document: document, natives: natives, op: op)
        from ordered in op.Catch(() => graph.IsDirectedAcyclicGraph()
            ? Fin.Succ(value: toSeq(graph.SourceFirstBidirectionalTopologicalSort()))
            : Fin.Fail<Seq<Guid>>(error: op.InvalidResult(detail: nameof(HistoryWeb.Loops))))
        select (WebAnswer)new WebAnswer.Ordered(UpdateOrder: ordered);

    internal static Fin<WebAnswer> Loops(RhinoDoc document, TableTarget target, Op op) =>
        from natives in Objects.Resolve(document: document, target: target, key: op)
        from graph in Woven(document: document, natives: natives, op: op)
        from groups in op.Catch(() => Fin.Succ(value: Rasm.Rhino.Blocks.CycleGroups.Of(graph: graph)))
        select (WebAnswer)new WebAnswer.Groups(Cyclic: groups);

    private static Fin<BidirectionalGraph<Guid, SEdge<Guid>>> Woven(RhinoDoc document, Seq<RhinoObject> natives, Op op) =>
        op.Catch(() => {
            BidirectionalGraph<Guid, SEdge<Guid>> graph = new(allowParallelEdges: false);
            Queue<Guid> pending = new(natives.Map(static native => native.Id).AsIterable());
            HashSet<Guid> visited = [];
            HashSet<(Guid Parent, Guid Child)> edges = [];
            while (pending.TryDequeue(out Guid id)) {
                if (!visited.Add(id)) { continue; }
                _ = graph.AddVertex(id);
                RhinoObject? native = document.Objects.FindId(id);
                if (native is null) { continue; }
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

| [INDEX] | [CONCERN]           | [OWNER]          | [FORM]                                                  | [ENTRY]                              |
| :-----: | :------------------ | :--------------- | :------------------------------------------------------- | :----------------------------------- |
|  [01]   | slot payloads       | `SlotValue`      | one closed union, total write onto the native record     | `HistoryScript` slot rows            |
|  [02]   | record authoring    | `HistoryScript`  | single-use leased mint keyed to the owning command       | `Mint(owner)` / carrier threading    |
|  [03]   | slot recovery       | `SlotShape`      | typed reader rows mirroring the write vocabulary         | `Recover(data, key)`                 |
|  [04]   | geometry regrowth   | `RegrowKind<T>`  | typed rows own catalogued `UpdateTo*` dispatch            | `Regrown.Of(kind, value)`           |
|  [05]   | replay body         | `ReplayProgram`  | strict-`bool` compiled delegate on the policy row        | `CommandPolicy.Replay = Delegate`    |
|  [06]   | linkage mutation    | `BondOp`         | undo-recorded attach, detach, and survival                | `Chronicle.Bind`                    |
|  [07]   | dependency topology | `HistoryWeb`     | reachable QuikGraph closure, order, and cycle rows        | `Chronicle.Ask`                     |
|  [08]   | process governance  | `HistoryConduct` | catalogued `HistorySettings` switches as read/write rows | `Chronicle.Conduct`                  |
