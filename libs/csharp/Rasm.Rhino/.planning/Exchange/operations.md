# [RASM_RHINO_OPERATIONS]

`Exchanges.Run` owns document-bound import, export, persistence, geolocation, preset composition, in-session programs, and cross-document conversion. `ExchangeBudget` parameterizes parallel headless work; `CodecRequest`, `Presets.Commit`, and `DocumentCommit.Sealed` remain the owning seam contracts.

## [01]-[INDEX]

- [02]-[LANE_AND_OUTPUT]: `FieldOverride<T>` the folder-wide override vocabulary; `ExchangeBudget` and `IoLane` the cross-document concurrency product; `CollisionRule`, `DirectoryRule`, `OutputPolicy`, and `MutationTrace` the egress vocabulary, landing kernel, and residue cell.
- [03]-[PRESET_COMPOSITION]: `PresetOperation` and `Presets.Commit` — the Persistence owner composed by `ExchangeOp.PresetCase`.
- [04]-[GEOLOCATION]: `GeoPoint`, `EarthAnchor`, and `AnchorOp` — read, write, planes, and the model↔earth correspondence on one owner.
- [05]-[TRANSACTION_RAIL]: `ExchangeOp`, `ExchangeFact`/`ExchangeReceipt`, `BatchPolicy`/`ConversionPolicy` with the `ExchangeHalt` cancellation carrier, and `Exchanges` — one session-proved dispatch plus the cross-document conversion fan.

## [02]-[LANE_AND_OUTPUT]

- Owner: `FieldOverride<T>` is the folder's one three-state override vocabulary. `ExchangeBudget` admits I/O degree and scheduler once. `IoLane` closes sequential and budgeted-parallel conversion. `CollisionRule`, `DirectoryRule`, and `OutputPolicy` settle and land every egress path under one declared collision, directory, staging, durability, and content-identity contract. `MutationTrace` is the folder's one residue cell, armed by the exchange rail at bracket entry and by the archive rail at its landing hook.
- Law: `FieldOverride<T>` states three intentions and no more — `Keep` leaves the host baseline standing, `Set` writes a gate plus its value, and `Clear` forces the gate off so the host inherits. `Apply` drives the paired write/inherit actions and `Accepts` admits only the `Set` payload, so a caller carrying an override never spells a second enable flag beside its value. The sheet and dial pages compose this owner; a per-page override union beside it is the deleted form.
- Law: `MutationTrace` carries attempt and residue as one cell — `Enter` marks an attempt that an undo bracket or preset commit can still roll back, `Landing` marks the filesystem touch behind which no undo serial stands, and the reading step folds both into its own evidence. One trace type serves both rails, so a step's residue claim reads the same regardless of which rail observed it.
- Law: direct host writers settle against the filesystem at dispatch instant, while staged artifacts validate, flush, and hash before the collision row atomically moves them onto an admitted destination; both return the settled `DocumentPath` on the receipt, so no fallible work follows commit and the caller never re-derives the ordinal.
- Law: `Fail` and `AppendOrdinal` use no-clobber moves. `AppendOrdinal` walks its bounded candidate roster as one fold on the rail — a refusal whose candidate now exists lost the seat to a concurrent creator and the walk continues, any other refusal settles as the reported fault, and exhaustion is the seed fault; an unbounded rename loop is unrepresentable because the bound is a `Dimension` policy value, and an exception filter deciding continuation is the deleted form.
- Law: `Land` is the sole staging kernel for every artifact this package writes itself — archive persistence and amendment, embedded-file extraction, fresh-archive geometry emission, and every publish delivery stage through it; a second temp-write-verify-move spelling beside it is the deleted form. Host writers that dispatch on the destination extension or mutate document identity (`RhinoDoc.Export`, `ExportSelected`, `Save`, `SaveAs`, the direct engines) write their settled path directly, because a `.partial` staging name forks the host's own format dispatch.
- Boundary: filesystem probes, durable flush, cleanup, and atomic move stay inside `CollisionRule`, `DirectoryRule`, and `OutputPolicy.Land`; their ordered statements are the platform-forced file-kernel exemption.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rasm.Rhino.Persistence;
using Rhino.FileIO;
using Rhino.Render;
using System.Runtime.InteropServices;

namespace Rasm.Rhino.Exchange;

// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FieldOverride<T> {
    private FieldOverride() { }
    public sealed record KeepCase : FieldOverride<T>;
    public sealed record SetCase(T Value) : FieldOverride<T>;
    public sealed record ClearCase : FieldOverride<T>;

    public static FieldOverride<T> Keep { get; } = new KeepCase();

    internal bool IsActive => this is not KeepCase;

    internal Unit Apply(Action<T> set, Action inherit) => Switch(
        (Set: set, Inherit: inherit),
        keepCase: static (_, _) => unit,
        setCase: static (write, field) => Op.Side(() => write.Set(field.Value)),
        clearCase: static (write, _) => Op.Side(write.Inherit));

    internal bool Accepts(Func<T, bool> admitted) => Switch(
        state: admitted,
        keepCase: static (_, _) => true,
        setCase: static (accepts, field) => accepts(field.Value),
        clearCase: static (_, _) => true);
}

[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct ExchangeBudget {
    public Dimension IoDegree { get; }
    public System.Threading.Tasks.TaskScheduler Scheduler { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Dimension ioDegree,
        ref System.Threading.Tasks.TaskScheduler scheduler) =>
        validationError = ioDegree.Value <= 0
            ? new ValidationError("Exchange I/O degree must be positive.")
            : scheduler is null
                ? new ValidationError("Exchange scheduler is required.")
                : null;

    public static Fin<ExchangeBudget> Of(
        Dimension ioDegree,
        System.Threading.Tasks.TaskScheduler scheduler,
        Op? key = null) {
        Op op = key.OrDefault();
        return Validate(ioDegree: ioDegree, scheduler: scheduler, item: out ExchangeBudget value) is null
            ? Fin.Succ(value: value)
            : Fin.Fail<ExchangeBudget>(error: op.InvalidInput());
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record IoLane {
    private IoLane() { }
    public sealed record SequentialCase : IoLane;
    public sealed record ParallelCase(ExchangeBudget Budget) : IoLane;

    public static IoLane Sequential { get; } = new SequentialCase();
    public static IoLane Parallel(ExchangeBudget budget) => new ParallelCase(Budget: budget);

    internal bool Admitted => Switch(
        sequentialCase: static _ => true,
        parallelCase: static lane => lane.Budget.IoDegree.Value > 0 && lane.Budget.Scheduler is not null);
}

[SmartEnum<int>]
public sealed partial class CollisionRule {
    public static readonly CollisionRule Fail = new(
        key: 0,
        settle: static (path, _, op) => System.IO.File.Exists(path.Value)
            ? Fin.Fail<DocumentPath>(error: op.InvalidInput())
            : Fin.Succ(value: path),
        land: static (temporary, path, _, op) => Move(temporary, path, overwrite: false, op));
    public static readonly CollisionRule Replace = new(
        key: 1,
        settle: static (path, _, _) => Fin.Succ(value: path),
        land: static (temporary, path, _, op) => Move(temporary, path, overwrite: true, op));
    public static readonly CollisionRule AppendOrdinal = new(key: 2, settle: static (path, bound, op) => {
        if (!System.IO.File.Exists(path.Value)) {
            return Fin.Succ(value: path);
        }
        return Candidates(path, bound).Tail
            .Find(candidate => !System.IO.File.Exists(candidate.Value))
            .ToFin(Fail: op.InvalidResult(detail: $"collision bound {bound.Value} exhausted"));
    }, land: Append);

    [UseDelegateFromConstructor]
    internal partial Fin<DocumentPath> Settle(DocumentPath path, Dimension bound, Op key);

    [UseDelegateFromConstructor]
    internal partial Fin<DocumentPath> Land(string temporary, DocumentPath path, Dimension bound, Op key);

    private static Fin<DocumentPath> Move(string temporary, DocumentPath path, bool overwrite, Op op) => op.Catch(() => {
        System.IO.File.Move(sourceFileName: temporary, destFileName: path.Value, overwrite: overwrite);
        return Fin.Succ(value: path);
    });

    // Bounded retry over the ordinal roster: a refused move whose candidate now exists lost the seat to a concurrent
    // creator and the walk continues, while any other refusal settles as the reported fault instead of being masked
    // by an exhaustion message the roster never reached.
    private static Fin<DocumentPath> Append(string temporary, DocumentPath path, Dimension bound, Op op) =>
        Candidates(path, bound).Fold(
            (Settled: false, Outcome: Fin.Fail<DocumentPath>(error: op.InvalidResult(detail: $"collision bound {bound.Value} exhausted"))),
            (state, candidate) => state.Settled
                ? state
                : Move(temporary: temporary, path: candidate, overwrite: false, op: op).Match(
                    Succ: landed => (Settled: true, Outcome: Fin.Succ(value: landed)),
                    Fail: failure => System.IO.File.Exists(path: candidate.Value)
                        ? state
                        : (Settled: true, Outcome: Fin.Fail<DocumentPath>(error: failure))))
            .Outcome;

    private static Seq<DocumentPath> Candidates(DocumentPath path, Dimension bound) {
        string stem = System.IO.Path.Join(
            System.IO.Path.GetDirectoryName(path.Value) ?? string.Empty,
            System.IO.Path.GetFileNameWithoutExtension(path.Value));
        string extension = System.IO.Path.GetExtension(path.Value);
        return Seq(path) + toSeq(Range(1, bound.Value)).Map(ordinal => DocumentPath.Create(value: $"{stem}-{ordinal}{extension}"));
    }
}

[SmartEnum<int>]
public sealed partial class DirectoryRule {
    public static readonly DirectoryRule Existing = new(key: 0, ensure: static (folder, op) =>
        guard(System.IO.Directory.Exists(folder), op.InvalidInput()).ToFin());
    public static readonly DirectoryRule Create = new(key: 1, ensure: static (folder, op) =>
        op.Catch(() => {
            _ = System.IO.Directory.CreateDirectory(folder);
            return Fin.Succ(value: unit);
        }));

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Ensure(string folder, Op key);
}

// --- [MODELS] -------------------------------------------------------------------------------
internal sealed class MutationTrace {
    private readonly Atom<(bool Attempted, bool MayRemain)> cell = Atom((Attempted: false, MayRemain: false));

    internal bool Attempted => cell.Value.Attempted;
    internal bool MayRemain => cell.Value.MayRemain;

    internal static MutationTrace Fresh() => new();

    // Bracket entry: an attempt a rollback still owns, so residue stays false.
    internal Fin<Unit> Enter(bool enabled) => enabled
        ? Fin.Succ(value: ignore(cell.Swap(static held => (Attempted: true, MayRemain: held.MayRemain))))
        : Fin.Succ(value: unit);

    // Landing hook: the staging kernel has begun touching the filesystem and no undo serial stands behind it.
    internal Fin<Unit> Landing() =>
        Fin.Succ(value: ignore(cell.Swap(static _ => (Attempted: true, MayRemain: true))));
}

public sealed record Landed<TStage>(DocumentPath Target, UInt128 ContentKey, TStage Stage);

[ComplexValueObject]
public sealed partial record OutputPolicy {
    public CollisionRule Collision { get; }
    public DirectoryRule Directory { get; }
    public Dimension OrdinalBound { get; }

    // One ordinal roster per settled destination: `<stem>-1` through `<stem>-64` stay legible as variants of the
    // requested name, and past that a caller wants a distinct destination rather than a deeper rename walk.
    public static Dimension OrdinalCeiling { get; } = Dimension.Create(value: 64);

    public static OutputPolicy Strict { get; } = Create(
        collision: CollisionRule.Fail,
        directory: DirectoryRule.Existing,
        ordinalBound: OrdinalCeiling);

    public static OutputPolicy Landing { get; } = Create(
        collision: CollisionRule.AppendOrdinal,
        directory: DirectoryRule.Create,
        ordinalBound: OrdinalCeiling);

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref CollisionRule collision,
        ref DirectoryRule directory,
        ref Dimension ordinalBound) =>
        validationError = collision is null || directory is null || ordinalBound.Value <= 0
            ? new ValidationError("Output policy requires a collision rule, directory rule, and positive ordinal bound.")
            : null;

    public static Fin<OutputPolicy> Of(
        CollisionRule collision,
        DirectoryRule directory,
        Dimension ordinalBound,
        Op? key = null) {
        Op op = key.OrDefault();
        return Validate(collision: collision, directory: directory, ordinalBound: ordinalBound, item: out OutputPolicy? policy) is null
            ? op.Need(policy)
            : Fin.Fail<OutputPolicy>(error: op.InvalidInput());
    }

    internal Fin<DocumentPath> Resolve(DocumentPath target, Option<FileCodec> codec = default, Op? key = null) {
        Op op = key.OrDefault();
        DocumentPath requested = codec.Map(row => DocumentPath.Create(value: row.EnsureExtension(path: target.Value))).IfNone(target);
        return from _folder in Directory.Ensure(folder: System.IO.Path.GetDirectoryName(requested.Value) ?? string.Empty, key: op)
               from settled in Collision.Settle(
                   path: requested,
                   bound: OrdinalBound,
                   key: op)
               select settled;
    }

    internal Fin<Landed<TStage>> Land<TStage>(
        DocumentPath target,
        Option<FileCodec> codec,
        Func<string, Fin<TStage>> stage,
        Option<Func<byte[], Fin<Unit>>> validate = default,
        Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(stage).Bind(writer => {
            DocumentPath requested = codec.Map(row => DocumentPath.Create(value: row.EnsureExtension(path: target.Value))).IfNone(target);
            string directory = System.IO.Path.GetDirectoryName(requested.Value) ?? string.Empty;
            return Directory.Ensure(folder: directory, key: op).Bind(_ => {
                string temporary = System.IO.Path.Join(
                    directory,
                    $".{System.IO.Path.GetFileName(requested.Value)}.{Guid.NewGuid():N}.partial");
                Fin<Landed<TStage>> outcome =
                    from staged in writer(arg: temporary)
                    from bytes in ReadNonempty(path: temporary, op: op)
                    from _staged in validate.Map(check => check(arg: bytes)).IfNone(Fin.Succ(value: unit))
                    from _durable in Flush(path: temporary, op: op)
                    let contentKey = ContentHash.Of(canonicalBytes: bytes)
                    from committed in Collision.Land(
                        temporary: temporary,
                        path: requested,
                        bound: OrdinalBound,
                        key: op)
                    select new Landed<TStage>(
                        Target: committed,
                        ContentKey: contentKey,
                        Stage: staged);
                return outcome.Match(
                    Succ: written => Fin.Succ(value: written),
                    Fail: primary => Cleanup(path: temporary, op: op).Match(
                        Succ: _ => Fin.Fail<Landed<TStage>>(error: primary),
                        Fail: cleanup => Fin.Fail<Landed<TStage>>(error: primary + cleanup)));
            });
        });
    }

    private static Fin<byte[]> ReadNonempty(string path, Op op) =>
        op.Catch(() => Fin.Succ(value: System.IO.File.ReadAllBytes(path: path)))
            .Bind(bytes => guard(bytes.Length > 0, op.InvalidResult()).ToFin().Map(_ => bytes));

    private static Fin<Unit> Flush(string path, Op op) => op.Catch(() => {
        using System.IO.FileStream stream = new(
            path: path,
            mode: System.IO.FileMode.Open,
            access: System.IO.FileAccess.ReadWrite,
            share: System.IO.FileShare.Read);
        stream.Flush(flushToDisk: true);
        return Fin.Succ(value: unit);
    });

    private static Fin<Unit> Cleanup(string path, Op op) => op.Catch(() => {
        if (System.IO.File.Exists(path: path)) {
            System.IO.File.Delete(path: path);
        }
        return Fin.Succ(value: unit);
    });
}
```

## [03]-[PRESET_COMPOSITION]

- Owner: `PresetOperation` and `Presets.Commit` own construction planes, named positions, named layer states, roster counts, identity resolution, participating object ids, and stored transforms. `ExchangeOp.PresetCase` composes that owner without a second saved-state vocabulary or host-table interpreter.
- Law: `Run` routes a preset request before any exchange demand because `Presets.Commit` derives its own read, mutation, undo, and redraw needs from `PresetOperation.Execution`; this rail reads that same policy row for its own profile rather than predicting mutation from the case shape. Batch execution re-enters `Run` per case, so preset and exchange programs share ordered failure and halt receipts without nesting document demands.
- Boundary: the composed seam is the Persistence surface below and nothing more — `PresetOperation` as the request, `PresetExecution` as its policy row, `Presets.Commit` as the entry, and `PresetAnswer` as the yield this rail wraps in one fact.

```csharp signature
// --- [COMPOSITION] --------------------------------------------------------------------------
// The whole Persistence seam this rail reaches: `PresetOperation` the request, `PresetExecution` its policy row,
// `Presets.Commit` the entry, `PresetAnswer` the yield. Every host table read and write stays behind that entry.
using Rasm.Rhino.Persistence;

internal static class PresetSeam {
    internal static Fin<ExchangeFact> Commit(DocumentSession session, PresetOperation operation, Op op) =>
        Presets.Commit(session: session, operation: operation, key: op)
            .Map(static answer => (ExchangeFact)new ExchangeFact.PresetCase(Answer: answer));
}
```

## [04]-[GEOLOCATION]

- Owner: `GeoPoint` and `EarthAnchor` are generated complex values. `GeoPoint.Of` accumulates coordinate gates; `EarthAnchor.Of` admits earth, model-frame, identity, and coordinate-system fields as one correlated product. `AnchorDemand` carries each host-location precondition as a policy row. `AnchorOp` closes read, write, plane with anchor north, compass, orientation with anchor north, model-to-earth, earth-to-model, and sun synchronization.
- Law: the host `EarthAnchorPoint` is disposable host material — every arm opens it inside a `using` window, projects detached values, and lets the window close; the anchor never rides a signature.
- Law: the sun arm is a read-modify-commit over one leased `RenderSettings` — the sub-owner mutation is inert until the same `RhinoDoc.RenderSettings` accessor takes the settings back, so a bound-and-forgotten `settings.Sun` write returns a success receipt for a synchronization that never lands; the north angle takes `RhinoMath.ToDegrees` and never a re-derived radian conversion.
- Law: earth-required and model-required preconditions gate per arm through `EarthLocationIsSet`/`ModelLocationIsSet` — a projection over an unset anchor is a typed refusal, never a garbage transform.
- Boundary: the model-to-earth transform is unit-aware — `GetModelToEarthTransform(modelUnits:)` receives the document's live `LengthUnit`, read inside the same demand window that uses it, so a stale unit regime cannot skew the projection.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
[ComplexValueObject]
[StructLayout(LayoutKind.Auto)]
public readonly partial struct GeoPoint {
    public double Latitude { get; }
    public double Longitude { get; }
    public double Elevation { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double latitude,
        ref double longitude,
        ref double elevation) =>
        validationError = !double.IsFinite(latitude) || latitude is < -90d or > 90d
            ? new ValidationError("Latitude must be finite and in [-90, 90].")
            : !double.IsFinite(longitude) || longitude is < -180d or > 180d
                ? new ValidationError("Longitude must be finite and in [-180, 180].")
                : !double.IsFinite(elevation)
                    ? new ValidationError("Elevation must be finite.")
                    : null;

    public static Fin<GeoPoint> Of(double latitude, double longitude, double elevation, Op? key = null) {
        Op op = key.OrDefault();
        return Validate(latitude: latitude, longitude: longitude, elevation: elevation, item: out GeoPoint value) is null
            ? Fin.Succ(value: value)
            : Fin.Fail<GeoPoint>(error: op.InvalidInput());
    }
}

[ComplexValueObject]
public sealed partial record EarthAnchor {
    public Option<GeoPoint> Basepoint { get; }
    public int ElevationCoordinateSystem { get; }
    public Option<Point3d> ModelBasePoint { get; }
    public Option<Vector3d> ModelNorth { get; }
    public Option<Vector3d> ModelEast { get; }
    public Option<string> Name { get; }
    public Option<string> Description { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Option<GeoPoint> basepoint,
        ref int elevationCoordinateSystem,
        ref Option<Point3d> modelBasePoint,
        ref Option<Vector3d> modelNorth,
        ref Option<Vector3d> modelEast,
        ref Option<string> name,
        ref Option<string> description) {
        name = name.Map(static text => text.Trim()).Filter(static text => !string.IsNullOrWhiteSpace(value: text));
        description = description.Map(static text => text.Trim()).Filter(static text => !string.IsNullOrWhiteSpace(value: text));
        bool completeModel = modelBasePoint.IsSome && modelNorth.IsSome && modelEast.IsSome;
        bool absentModel = modelBasePoint.IsNone && modelNorth.IsNone && modelEast.IsNone;
        bool noncollinear = modelNorth
            .Bind(north => modelEast.Map(east => Vector3d.CrossProduct(north, east).Length > 0d))
            .IfNone(true);
        bool validFrame = modelBasePoint.ForAll(static point => point.IsValid)
            && modelNorth.ForAll(static vector => vector.IsValid && vector.Length > 0d)
            && modelEast.ForAll(static vector => vector.IsValid && vector.Length > 0d)
            && noncollinear;
        validationError = !completeModel && !absentModel
            ? new ValidationError("Model basepoint, north, and east must be supplied together.")
            : !validFrame
                ? new ValidationError("Model frame must contain finite non-collinear axes.")
                : null;
    }

    public static Fin<EarthAnchor> Of(
        Option<GeoPoint> basepoint,
        int elevationCoordinateSystem,
        Option<Point3d> modelBasePoint,
        Option<Vector3d> modelNorth,
        Option<Vector3d> modelEast,
        Option<string> name = default,
        Option<string> description = default,
        Op? key = null) {
        Op op = key.OrDefault();
        return Validate(
            basepoint: basepoint,
            elevationCoordinateSystem: elevationCoordinateSystem,
            modelBasePoint: modelBasePoint,
            modelNorth: modelNorth,
            modelEast: modelEast,
            name: name,
            description: description,
            item: out EarthAnchor? anchor) is null
            ? op.Need(anchor)
            : Fin.Fail<EarthAnchor>(error: op.InvalidInput());
    }

    internal static Fin<EarthAnchor> From(EarthAnchorPoint anchor, Op op) =>
        (anchor.EarthLocationIsSet()
            ? GeoPoint.Of(
                latitude: anchor.EarthBasepointLatitude,
                longitude: anchor.EarthBasepointLongitude,
                elevation: anchor.EarthBasepointElevation,
                key: op).Map(Some)
            : Fin.Succ(Option<GeoPoint>.None)).Bind(basepoint => {
                bool modelSet = anchor.ModelLocationIsSet();
                return Of(
                    basepoint: basepoint,
                    elevationCoordinateSystem: anchor.EarthBasepointElevationCoordinateSystem,
                    modelBasePoint: modelSet ? Some(anchor.ModelBasePoint) : None,
                    modelNorth: modelSet ? Some(anchor.ModelNorth) : None,
                    modelEast: modelSet ? Some(anchor.ModelEast) : None,
                    name: Optional(anchor.Name),
                    description: Optional(anchor.Description),
                    key: op);
            });

    internal Fin<Unit> Write(RhinoDoc document, Op op) {
        return op.Catch(() => {
            using EarthAnchorPoint anchor = new();
            _ = Basepoint.Iter(point => {
                anchor.EarthBasepointLatitude = point.Latitude;
                anchor.EarthBasepointLongitude = point.Longitude;
                anchor.EarthBasepointElevation = point.Elevation;
            });
            anchor.EarthBasepointElevationCoordinateSystem = ElevationCoordinateSystem;
            _ = ModelBasePoint.Iter(value => anchor.ModelBasePoint = value);
            _ = ModelNorth.Iter(value => anchor.ModelNorth = value);
            _ = ModelEast.Iter(value => anchor.ModelEast = value);
            _ = Name.Iter(value => anchor.Name = value);
            _ = Description.Iter(value => anchor.Description = value);
            document.EarthAnchorPoint = anchor;
            return Fin.Succ(value: unit);
        });
    }
}

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
internal sealed partial class AnchorDemand {
    public static readonly AnchorDemand Any = new(key: 0, accepts: static _ => true);
    public static readonly AnchorDemand Model = new(key: 1, accepts: static anchor => anchor.ModelLocationIsSet());
    public static readonly AnchorDemand Located = new(key: 2,
        accepts: static anchor => anchor.EarthLocationIsSet() && anchor.ModelLocationIsSet());

    [UseDelegateFromConstructor]
    internal partial bool Accepts(EarthAnchorPoint anchor);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AnchorOp {
    private AnchorOp() { }
    public sealed record ReadCase : AnchorOp;
    public sealed record WriteCase(EarthAnchor Anchor) : AnchorOp;
    public sealed record PlaneCase : AnchorOp;
    public sealed record CompassCase : AnchorOp;
    public sealed record OrientCase(Plane Source) : AnchorOp;
    public sealed record ToEarthCase(Seq<Point3d> Points) : AnchorOp;
    public sealed record ToModelCase(Seq<GeoPoint> Points) : AnchorOp;
    public sealed record SunCase : AnchorOp;

    internal Fin<AnchorYield> Apply(RhinoDoc document, Op op) => Switch(
        (Document: document, Op: op),
        readCase: static (ctx, _) => Anchored(ctx.Document, ctx.Op, AnchorDemand.Any, use: static (anchor, _, op) =>
            EarthAnchor.From(anchor: anchor, op: op).Map(static value => (AnchorYield)new AnchorYield.AnchorCase(Anchor: value))),
        writeCase: static (ctx, edit) =>
            from anchor in ctx.Op.Need(edit.Anchor)
            from _written in anchor.Write(document: ctx.Document, op: ctx.Op)
            select (AnchorYield)new AnchorYield.AnchorCase(Anchor: anchor),
        planeCase: static (ctx, _) => Anchored(ctx.Document, ctx.Op, AnchorDemand.Model, use: static (anchor, _, op) => {
            Plane plane = anchor.GetEarthAnchorPlane(anchorNorth: out Vector3d north);
            return op.AcceptValue(value: plane)
                .Map(admitted => (AnchorYield)new AnchorYield.PlaneCase(Plane: admitted, North: north));
        }),
        compassCase: static (ctx, _) => Anchored(ctx.Document, ctx.Op, AnchorDemand.Model, use: static (anchor, _, op) =>
            op.AcceptValue(value: anchor.GetModelCompass())
                .Map(static plane => (AnchorYield)new AnchorYield.CompassCase(Plane: plane))),
        orientCase: static (ctx, edit) => Anchored(ctx.Document, ctx.Op, AnchorDemand.Model, use: (anchor, _, op) => {
            Plane target = anchor.GetEarthAnchorPlane(anchorNorth: out Vector3d north);
            return (edit.Source.IsValid, target.IsValid) switch {
                (true, true) => Fin.Succ(value: (AnchorYield)new AnchorYield.TransformCase(
                    Value: Transform.PlaneToPlane(plane0: edit.Source, plane1: target), North: north)),
                _ => Fin.Fail<AnchorYield>(error: op.InvalidInput()),
            };
        }),
        toEarthCase: static (ctx, edit) => Anchored(ctx.Document, ctx.Op, AnchorDemand.Located, use: (anchor, document, op) => {
            Transform projection = anchor.GetModelToEarthTransform(modelUnits: document.ModelUnits);
            return guard(projection.IsValid, op.InvalidResult()).ToFin().Bind(_ =>
                edit.Points.TraverseM(point => {
                    Point3d projected = point;
                    projected.Transform(xform: projection);
                    return GeoPoint.Of(
                        latitude: projected.X,
                        longitude: projected.Y,
                        elevation: projected.Z,
                        key: op);
                }).As().Map(points => (AnchorYield)new AnchorYield.EarthCase(Points: points)));
        }),
        toModelCase: static (ctx, edit) => Anchored(ctx.Document, ctx.Op, AnchorDemand.Located, use: (anchor, document, op) => {
            Transform projection = anchor.GetModelToEarthTransform(modelUnits: document.ModelUnits);
            return guard(projection.TryGetInverse(inverseTransform: out Transform inverse), op.InvalidResult()).ToFin().Map(_ =>
                (AnchorYield)new AnchorYield.ModelCase(Points: edit.Points.Map(point => {
                    Point3d model = new(x: point.Latitude, y: point.Longitude, z: point.Elevation);
                    model.Transform(xform: inverse);
                    return model;
                })));
        }),
        sunCase: static (ctx, _) => Anchored(ctx.Document, ctx.Op, AnchorDemand.Located, use: static (anchor, document, op) =>
            op.Catch(() => {
                using RenderSettings settings = document.RenderSettings;
                using Sun sun = settings.Sun;
                Vector3d north = anchor.ModelNorth;
                sun.Latitude = anchor.EarthBasepointLatitude;
                sun.Longitude = anchor.EarthBasepointLongitude;
                sun.North = RhinoMath.ToDegrees(radians: Math.Atan2(y: north.Y, x: north.X));
                document.RenderSettings = settings;
                return Fin.Succ(value: (AnchorYield)new AnchorYield.SunCase());
            })));

    private static Fin<AnchorYield> Anchored(
        RhinoDoc document, Op op, AnchorDemand demand,
        Func<EarthAnchorPoint, RhinoDoc, Op, Fin<AnchorYield>> use) =>
        op.Catch(() => {
            using EarthAnchorPoint? anchor = document.EarthAnchorPoint;
            return Optional(anchor).ToFin(Fail: op.InvalidResult()).Bind(live =>
                demand.Accepts(anchor: live)
                    ? use(arg1: live, arg2: document, arg3: op)
                    : Fin.Fail<AnchorYield>(error: op.MissingContext()));
        });
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AnchorYield {
    private AnchorYield() { }
    public sealed record AnchorCase(EarthAnchor Anchor) : AnchorYield;
    public sealed record PlaneCase(Plane Plane, Vector3d North) : AnchorYield;
    public sealed record CompassCase(Plane Plane) : AnchorYield;
    public sealed record TransformCase(Transform Value, Vector3d North) : AnchorYield;
    public sealed record EarthCase(Seq<GeoPoint> Points) : AnchorYield;
    public sealed record ModelCase(Seq<Point3d> Points) : AnchorYield;
    public sealed record SunCase : AnchorYield;
}
```

## [05]-[TRANSACTION_RAIL]

- Owner: `ExchangeOp` closes the three routes one request can take — a document edit, a preset commit, a program — and `DocumentOp` closes the six edits the document dispatcher executes: import, export, save, write, geometry, anchor. `ExchangeFact` is the ONE outcome vocabulary — imported source, artifact, save, preset, and anchor evidence by payload shape — and `ExchangeReceipt` carries that fact stream beside its evidence and an `Option<ExchangeProgram>`, so a nested program is absence-or-presence rather than a parallel case in a second yield family every construction site builds twice. `ExchangeStep` and `ExchangeProgram` preserve ordered outcomes, halt state, mutation truth, and native evidence.
- Entry: `Exchanges.Run(DocumentSession, ExchangeOp, Op?, ExchangeHalt)` owns session-bound work. `Exchanges.Run(Seq<(SessionSource, ExchangeOp)>, ConversionPolicy, CancellationToken, Op?)` owns cross-document conversion and awaits `Parallel.ForEachAsync` under the caller-supplied `ExchangeBudget`.
- Law: request families split by the rail that executes them, so every closed dispatch is total over what it actually runs. `Run` routes the three `ExchangeOp` cases — a preset delegates to `Presets.Commit`, a program re-enters `Run` per case, and an edit alone reaches the session demand — while `Dispatch` switches the six `DocumentOp` cases behind that demand. A refusal arm standing in a closed switch for a case the rail never receives is the deleted form, and a new route or a new edit lands in exactly one family.
- Law: `DocumentOp.Profile` derives demand, mutation, and surface evidence in one generated dispatch, and `ExchangeOp.Profile` reads it through for an edit, folds it across a program, and answers a preset off `PresetOperation.Execution` — the Persistence owner's own policy row, never a re-derived mutation predicate.
- Law: `MutationTrace` enters immediately before preset commit or `DocumentCommit.Sealed`; failed steps report that observed entry instead of predicting mutation from request shape. Owned records roll back on failure, command-owned records propagate failure, and successful receipts alone receive committed mutation evidence.
- Law: the trace is `Option`-shaped because only a program step reads it — `Step` mints one per row and folds `Attempted` into its `ExchangeStep`, while the single-op entry passes `None` rather than recording an attempt into a cell nothing projects; committed mutation on that path is the `DocumentCommit.Sealed` stamp's own `MutationCase` with its real undo serial.
- Law: cancellation is cooperative and case-bounded — `ExchangeHalt` composes every ambient and policy `CancellationToken`, `Run` refuses before snapshot acquisition, and each program fold observes the merged halt only between cases. `ExchangeProgram.Halted` is true only when cancellation prevented a case; a pre-dispatch halt has no mutation attempt and therefore earns no mutation evidence.
- Law: `BatchPolicy` owns continuation and cooperative halt. `ConversionPolicy` is the outer storage seam: it admits `IoLane`, rejects a zero-initialized parallel `ExchangeBudget`, and rejects a parallel lane paired with a halting batch policy — collecting-only is an admission contract, so an accepted budget always reaches `ParallelOptions` and a caller learns its lane was unusable at construction rather than watching it silently degrade to sequential with no refusal, no degradation evidence, and no receipt row; parallel conversion never reads ambient processor count.
- Law: `SaveCase` consults `SessionSnapshot.Modified` — saving an unmodified document is a no-op receipt fact, never a redundant host write; the dirty fact comes from the session snapshot, not a host re-probe. `SaveCase` pre-guards a non-empty `RhinoDoc.Path` and crosses `op.Catch` on the dirty branch, because the host member throws on an unpathed document and this arm carries no undo bracket to convert for it; `TemplateCase` admits the archive extension against the codec row before the call and crosses the same catch.
- Law: egress cases resolve their target through `OutputPolicy` exactly once and stamp the SETTLED path plus the artifact's `ContentHash.Of` content key on the receipt, so downstream indexing keys on evidence.
- Law: the write target's codec is a `DocumentWritePolicy` projection, never a constant — `SaveAsCase`, `ArchiveCase`, and `TemplateCase` answer the row carrying `CodecAbility.Archive`, while `DocumentCase` writes through the extension-dispatching general writer and therefore answers `Codecs.Detect(target)` and refuses an undetectable extension. One projection feeds both `OutputPolicy.Resolve` and the receipt fact, so a `.obj` document write neither gains a `.3dm` suffix nor stamps an archive codec it did not produce.
- Law: `GeometryCase` is a session-bound export that writes no live-document geometry — after the session proves export capability, a fresh `File3dm` receives the requested geometry rows and lands through `Archives.Land`, the archive rail's one `WriteWithLog`-hooked staging over `OutputPolicy.Land`, so the landed 3dm carries the same byte re-materialization parse proof every archive persistence carries; a failed write carries the native log in fault detail, and a successful non-empty log becomes `ExchangeEvidence.NativeCase` under the landed target.
- Law: `ExportScope` gates selection by `CodecAbility.Selection` and owns one noninteractive `FileWriteOptions` carrier. Native `3dm`, `OBJ`, and `PLY` engines receive that carrier through one `Codecs.Apply`; every other selection row is refused before host contact.
- Law: `BackupPolicy` closes no-backup, primary-backup, and complete auxiliary-backup behavior as rows on the existing document-write carrier; `FileWriteOptions.CreateBackupFiles` and `CreateOtherBackupFiles` receive those columns at the host edge.
- Boundary: `RhinoDoc.Open` and every headless constructor belong to the Document session sources; an exchange request that names a document to acquire is a session construction at the call site, and this rail's batch runs against the session it was handed. `Parallel.ForEachAsync`, cancellation catch, and `DocumentSession` disposal statements are the platform-forced `Task` and resource exemptions; the parallel body writes one array seat per source-row index and nothing else, so receipt order is the row order by construction rather than a concurrent map re-sorted after the join, and an unwritten seat is a cancelled row the halt flag already accounts for.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExchangeFact {
    private ExchangeFact() { }
    public sealed record ImportedCase(DocumentPath Source, FileCodec Codec) : ExchangeFact;
    public sealed record ArtifactCase(DocumentPath Target, FileCodec Codec, UInt128 ContentKey) : ExchangeFact;
    public sealed record SaveCase(bool Written) : ExchangeFact;
    public sealed record PresetCase(PresetAnswer Answer) : ExchangeFact;
    public sealed record AnchorCase(AnchorYield Yield) : ExchangeFact;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExportScope {
    private ExportScope() { }
    public sealed record AllCase : ExportScope;
    public sealed record SelectionCase : ExportScope;

    internal Fin<FileWriteOptions> Carrier(FileCodec codec, Op op) {
        Fin<bool> selected = Switch(
            state: (Codec: codec, Op: op),
            allCase: static (_, _) => Fin.Succ(value: false),
            selectionCase: static (ctx, _) => guard(ctx.Codec.Has(CodecAbility.Selection), ctx.Op.InvalidInput()).ToFin().Map(_ => true));
        return selected.Map(value => new FileWriteOptions {
            WriteSelectedObjectsOnly = value,
            SuppressAllInput = true,
            SuppressDialogBoxes = true,
        });
    }
}

[SmartEnum]
public sealed partial class BackupPolicy {
    public static readonly BackupPolicy None = new(primary: false, auxiliary: false);
    public static readonly BackupPolicy Primary = new(primary: true, auxiliary: false);
    public static readonly BackupPolicy Complete = new(primary: true, auxiliary: true);

    public bool Primary { get; }
    public bool Auxiliary { get; }
}

[ComplexValueObject]
public sealed partial record DocumentContent {
    public bool GeometryOnly { get; }
    public bool UserData { get; }
    public bool RenderMeshes { get; }
    public bool PreviewImage { get; }
    public bool BitmapTable { get; }
    public bool History { get; }
    public BackupPolicy Backups { get; }
    public bool Compression { get; }
}

[ComplexValueObject]
public sealed partial record SaveAsContent {
    public bool GeometryOnly { get; }
    public bool Small { get; }
    public bool Textures { get; }
    public bool PluginData { get; }
    public bool Compression { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DocumentWritePolicy {
    private DocumentWritePolicy() { }
    public sealed record SaveAsCase(Option<Dimension> Version, SaveAsContent Content) : DocumentWritePolicy;
    public sealed record DocumentCase(DocumentContent Content) : DocumentWritePolicy;
    public sealed record ArchiveCase(DocumentContent Content) : DocumentWritePolicy;
    public sealed record TemplateCase(Option<Dimension> Version = default) : DocumentWritePolicy;

    internal Fin<FileCodec> Codec(DocumentPath target, Op op) => Switch(
        (Target: target, Op: op),
        saveAsCase: static (ctx, _) => Archived(op: ctx.Op),
        documentCase: static (ctx, _) => Codecs.Detect(path: ctx.Target.Value).ToFin(Fail: ctx.Op.InvalidInput()),
        archiveCase: static (ctx, _) => Archived(op: ctx.Op),
        templateCase: static (ctx, _) => Archived(op: ctx.Op));

    private static Fin<FileCodec> Archived(Op op) => Codecs.Archive.ToFin(Fail: op.InvalidResult());

    internal Fin<Unit> Write(RhinoDoc document, string path, Op op) => Switch(
        (Document: document, Path: path, Op: op),
        saveAsCase: static (ctx, policy) =>
            from content in ctx.Op.Need(policy.Content)
            from saved in ctx.Op.Confirm(success: ctx.Document.SaveAs(
                file3dmPath: ctx.Path,
                version: policy.Version.Map(static value => value.Value).IfNone(0),
                saveSmall: content.Small,
                saveTextures: content.Textures,
                saveGeometryOnly: content.GeometryOnly,
                savePluginData: content.PluginData,
                useCompression: content.Compression))
            select saved,
        documentCase: static (ctx, policy) =>
            from content in ctx.Op.Need(policy.Content)
            from backups in ctx.Op.Need(content.Backups)
            from written in ctx.Op.Confirm(success: ctx.Document.WriteFile(
                path: ctx.Path,
                options: Host(content: content, backups: backups)))
            select written,
        archiveCase: static (ctx, policy) =>
            from content in ctx.Op.Need(policy.Content)
            from backups in ctx.Op.Need(content.Backups)
            from written in ctx.Op.Confirm(success: ctx.Document.Write3dmFile(
                path: ctx.Path,
                options: Host(content: content, backups: backups)))
            select written,
        templateCase: static (ctx, policy) =>
            from archived in Archived(op: ctx.Op)
            from _extension in guard(archived.EnsureExtension(path: ctx.Path) == ctx.Path, ctx.Op.InvalidInput()).ToFin()
            from written in ctx.Op.Catch(() => policy.Version.Match(
                Some: version => ctx.Op.Confirm(success: ctx.Document.SaveAsTemplate(file3dmTemplatePath: ctx.Path, version: version.Value)),
                None: () => ctx.Op.Confirm(success: ctx.Document.SaveAsTemplate(file3dmTemplatePath: ctx.Path))))
            select written);

    private static FileWriteOptions Host(DocumentContent content, BackupPolicy backups) => new() {
        WriteGeometryOnly = content.GeometryOnly,
        WriteUserData = content.UserData,
        IncludeRenderMeshes = content.RenderMeshes,
        IncludePreviewImage = content.PreviewImage,
        IncludeBitmapTable = content.BitmapTable,
        IncludeHistory = content.History,
        CreateBackupFiles = backups.Primary,
        CreateOtherBackupFiles = backups.Auxiliary,
        UseCompression = content.Compression,
    };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DocumentOp {
    private DocumentOp() { }
    public sealed record ImportCase(DocumentPath Source, Option<FileCodec> Codec, CodecTune Tune) : DocumentOp;
    public sealed record ExportCase(DocumentPath Target, ExportScope Scope, Option<FileCodec> Codec, CodecTune Tune, OutputPolicy Output) : DocumentOp;
    public sealed record SaveCase : DocumentOp;
    public sealed record WriteCase(DocumentPath Target, DocumentWritePolicy Policy, OutputPolicy Output) : DocumentOp;
    public sealed record GeometryCase(Seq<GeometryBase> Geometry, DocumentPath Target, ArchiveWritePolicy Policy, OutputPolicy Output) : DocumentOp;
    public sealed record AnchorCase(AnchorOp Edit) : DocumentOp;

    internal (Seq<SessionNeed> Needs, bool Mutates, string Surface) Profile => Switch<(Seq<SessionNeed>, bool, string)>(
        importCase: static _ => (SessionNeed.Mutation(undo: true, redraw: RedrawPolicy.None), true, nameof(ImportCase)),
        exportCase: static _ => (Seq(SessionNeed.Export), false, nameof(ExportCase)),
        saveCase: static _ => (Seq(SessionNeed.Export), false, nameof(SaveCase)),
        writeCase: static _ => (Seq(SessionNeed.Export), false, nameof(WriteCase)),
        geometryCase: static _ => (Seq(SessionNeed.Export), false, nameof(GeometryCase)),
        anchorCase: static edit => edit.Edit is AnchorOp.WriteCase or AnchorOp.SunCase
            ? (SessionNeed.Mutation(undo: true, redraw: RedrawPolicy.None), true, nameof(AnchorCase))
            : (Seq(SessionNeed.Read), false, nameof(AnchorCase)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExchangeOp {
    private ExchangeOp() { }
    public sealed record EditCase(DocumentOp Edit) : ExchangeOp;
    public sealed record PresetCase(PresetOperation Operation) : ExchangeOp;
    public sealed record BatchCase(Seq<ExchangeOp> Program, BatchPolicy Policy) : ExchangeOp;

    internal ExchangeHalt Halt(ExchangeHalt ambient) =>
        this is BatchCase batch ? ambient.Merge(batch.Policy.Halt) : ambient;

    internal (Seq<SessionNeed> Needs, bool Mutates, string Surface) Profile => Switch<(Seq<SessionNeed>, bool, string)>(
        editCase: static edit => edit.Edit.Profile,
        presetCase: static edit => edit.Operation.Execution.Mutation
            ? (Seq(SessionNeed.Read, SessionNeed.Mutate, SessionNeed.Undo, SessionNeed.Redraw), true, nameof(PresetCase))
            : (Seq(SessionNeed.Read), false, nameof(PresetCase)),
        batchCase: static batch => BatchProfile(batch));

    private static (Seq<SessionNeed> Needs, bool Mutates, string Surface) BatchProfile(BatchCase batch) => (
        Needs: batch.Program.IsEmpty
            ? Seq(SessionNeed.Read)
            : batch.Program.Fold(Seq<SessionNeed>(), static (needs, inner) => needs + inner.Profile.Needs).Distinct(),
        Mutates: batch.Program.Exists(static inner => inner.Profile.Mutates),
        Surface: nameof(BatchCase));
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct ExchangeHalt(Seq<System.Threading.CancellationToken> Tokens) {
    public static ExchangeHalt None { get; } = new(Tokens: Seq<System.Threading.CancellationToken>());
    public static ExchangeHalt Of(System.Threading.CancellationToken token) =>
        token.CanBeCanceled ? new ExchangeHalt(Tokens: Seq(token)) : None;
    public bool Requested => Tokens.Exists(static token => token.IsCancellationRequested);
    internal ExchangeHalt Merge(ExchangeHalt other) => new(Tokens: (Tokens + other.Tokens).Distinct());
}

public readonly record struct BatchPolicy(bool ContinueOnError, ExchangeHalt Halt = default) {
    public static BatchPolicy Halting { get; } = new(ContinueOnError: false);
    public static BatchPolicy Collecting { get; } = new(ContinueOnError: true);
}

[ComplexValueObject]
public sealed partial record ConversionPolicy {
    public BatchPolicy Batch { get; }
    public IoLane Lane { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref BatchPolicy batch,
        ref IoLane lane) =>
        validationError = lane is null || !lane.Admitted
            ? new ValidationError("Conversion lane is required, and parallel lanes require an admitted budget.")
            : lane is IoLane.ParallelCase && !batch.ContinueOnError
                ? new ValidationError("A parallel conversion lane requires a collecting batch policy.")
                : null;

    public static Fin<ConversionPolicy> Of(BatchPolicy batch, IoLane lane, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated(Validate(batch, lane, out ConversionPolicy? policy), policy);
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExchangeStep {
    private ExchangeStep() { }
    public sealed record SucceededCase(int Index, bool MutationAttempted, ExchangeReceipt Receipt) : ExchangeStep;
    public sealed record FailedCase(int Index, bool MutationAttempted, Error Failure) : ExchangeStep;

    private (bool AttemptedMutation, bool Failed, bool Halted, Seq<ExchangeFact> Facts, Seq<ExchangeEvidence> Evidence) Profile => Switch(
        succeededCase: static step => (
            AttemptedMutation: step.MutationAttempted,
            Failed: step.Receipt.Program.Map(static program => program.Failed).IfNone(noneValue: false),
            Halted: step.Receipt.Program.Map(static program => program.Halted).IfNone(noneValue: false),
            Facts: step.Receipt.Facts,
            Evidence: step.Receipt.Evidence),
        failedCase: static step => (
            AttemptedMutation: step.MutationAttempted,
            Failed: true,
            Halted: false,
            Facts: Seq<ExchangeFact>(),
            Evidence: Seq<ExchangeEvidence>()));

    internal bool AttemptedMutation => Profile.AttemptedMutation;
    internal bool Failed => Profile.Failed;
    internal bool Halted => Profile.Halted;
    internal Seq<ExchangeFact> Facts => Profile.Facts;
    internal Seq<ExchangeEvidence> Evidence => Profile.Evidence;
}

public sealed record ExchangeProgram {
    private readonly Seq<ExchangeEvidence> ownEvidence;
    private readonly bool ownHalted;

    private ExchangeProgram(Seq<ExchangeStep> steps, bool halted, Seq<ExchangeEvidence> evidence) =>
        (Steps, ownHalted, ownEvidence) = (steps, halted, evidence);

    public Seq<ExchangeStep> Steps { get; }
    public bool Halted => ownHalted || Steps.Exists(static step => step.Halted);
    public bool MutationAttempted => Steps.Exists(static step => step.AttemptedMutation);
    public bool Failed => Steps.Exists(static step => step.Failed);
    public Seq<ExchangeFact> Facts => Steps.Bind(static step => step.Facts);
    public Seq<ExchangeEvidence> Evidence => Steps.Bind(static step => step.Evidence) + ownEvidence;

    internal static ExchangeProgram Of(Seq<ExchangeStep> steps, bool halted) =>
        new(steps: steps, halted: halted, evidence: Seq<ExchangeEvidence>());
    internal ExchangeProgram Add(ExchangeEvidence evidence) =>
        new(steps: Steps, halted: Halted, evidence: ownEvidence.Add(evidence));
}

public sealed record ExchangeReceipt : IDetachedDocumentResult {
    private ExchangeReceipt(Seq<ExchangeFact> facts, Seq<ExchangeEvidence> evidence, Option<ExchangeProgram> program) =>
        (Facts, Evidence, Program) = (facts, evidence, program);

    public Seq<ExchangeFact> Facts { get; }
    public Seq<ExchangeEvidence> Evidence { get; }
    public Option<ExchangeProgram> Program { get; }

    internal static ExchangeReceipt One(ExchangeFact fact) =>
        new(facts: Seq(fact), evidence: Seq<ExchangeEvidence>(), program: None);
    internal static ExchangeReceipt Of(Seq<ExchangeFact> facts, Seq<ExchangeEvidence> evidence = default) =>
        new(facts: facts, evidence: evidence, program: None);
    internal static ExchangeReceipt Programmed(Seq<ExchangeStep> steps, bool halted) =>
        From(ExchangeProgram.Of(steps: steps, halted: halted));

    internal ExchangeReceipt Add(ExchangeEvidence evidence) => Program.Match(
        Some: program => From(program.Add(evidence)),
        None: () => new ExchangeReceipt(facts: Facts, evidence: Evidence.Add(evidence), program: None));

    private static ExchangeReceipt From(ExchangeProgram program) =>
        new(facts: program.Facts, evidence: program.Evidence, program: Some(program));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Exchanges {
    public static Fin<ExchangeReceipt> Run(DocumentSession session, ExchangeOp request, Op? key = null, ExchangeHalt halt = default) {
        Op op = key.OrDefault();
        return Apply(session: session, request: request, op: op, halt: halt, trace: None);
    }

    private static Fin<ExchangeReceipt> Apply(
        DocumentSession session,
        ExchangeOp request,
        Op op,
        ExchangeHalt halt,
        Option<MutationTrace> trace) {
        return from admitted in op.Need(request)
               let effective = admitted.Halt(ambient: halt)
               from receipt in effective.Requested
                   ? Fin.Succ(value: ExchangeReceipt.Programmed(steps: Seq<ExchangeStep>(), halted: true))
                   : admitted.Switch(
                       (Session: session, Op: op, Halt: effective, Trace: trace),
                       editCase: static (ctx, route) =>
                           from edit in ctx.Op.Need(route.Edit)
                           from snapshot in ctx.Session.Snapshot(key: ctx.Op)
                           from demanded in ctx.Session.Demand(
                               use: document => Recorded(
                                   document: document,
                                   edit: edit,
                                   dirty: snapshot.Modified,
                                   halt: ctx.Halt,
                                   op: ctx.Op,
                                   trace: ctx.Trace),
                               key: ctx.Op,
                               needs: [.. edit.Profile.Needs])
                           select demanded,
                       presetCase: static (ctx, route) => Optional(route.Operation)
                           .ToFin(Fail: ctx.Op.InvalidInput())
                           .Bind(operation =>
                               from _attempt in Entered(trace: ctx.Trace, enabled: operation.Execution.Mutation)
                               from fact in PresetSeam.Commit(session: ctx.Session, operation: operation, op: ctx.Op)
                               select ExchangeReceipt.One(fact: fact)),
                       batchCase: static (ctx, route) => Fin.Succ(value: Program(
                           rows: route.Program,
                           halt: ctx.Halt,
                           continueOnError: route.Policy.ContinueOnError,
                           one: (inner, index) => Step(
                               index: index,
                               run: innerTrace => Apply(
                                   session: ctx.Session,
                                   request: inner,
                                   op: ctx.Op,
                                   halt: ctx.Halt,
                                   trace: innerTrace)))))
               select receipt;
    }

    public static async System.Threading.Tasks.Task<Fin<ExchangeReceipt>> Run(
        Seq<(SessionSource Source, ExchangeOp Request)> rows,
        ConversionPolicy policy,
        System.Threading.CancellationToken cancellationToken = default,
        Op? key = null) {
        Op op = key.OrDefault();
        return await op.Need(policy).Match(
            Succ: async admitted => {
                ExchangeHalt effectiveHalt = admitted.Batch.Halt.Merge(ExchangeHalt.Of(token: cancellationToken));
                Func<(SessionSource Source, ExchangeOp Request), int, ExchangeStep> one = (row, index) => Step(
                    index: index,
                    run: trace => op.Catch(() =>
                        from session in DocumentSession.Of(source: row.Source, mode: SessionMode.Headless, needs: [.. row.Request.Profile.Needs])
                        from receipt in Use(session: session, request: row.Request, halt: effectiveHalt, op: op, trace: trace)
                        select receipt));
                if (admitted.Lane is not IoLane.ParallelCase parallel) {
                    return Fin.Succ(value: Program(
                        rows: rows,
                        halt: effectiveHalt,
                        continueOnError: admitted.Batch.ContinueOnError,
                        one: one));
                }
                // One seat per source row: the parallel body writes its own index and nothing else, so the ordered
                // receipt reads straight off the array and needs no concurrent map re-sorted after the join.
                ExchangeStep?[] completed = new ExchangeStep?[rows.Count];
                System.Threading.Tasks.ParallelOptions options = new() {
                    MaxDegreeOfParallelism = parallel.Budget.IoDegree.Value,
                    TaskScheduler = parallel.Budget.Scheduler,
                    CancellationToken = cancellationToken,
                };
                try {
                    await System.Threading.Tasks.Parallel.ForEachAsync(
                        rows.Map(static (row, index) => (Row: row, Index: index)).AsIterable(),
                        options,
                        (item, token) => {
                            if (!token.IsCancellationRequested && !effectiveHalt.Requested) {
                                completed[item.Index] = one(item.Row, item.Index);
                            }
                            return System.Threading.Tasks.ValueTask.CompletedTask;
                        });
                } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
                Seq<ExchangeStep> ordered = toSeq(completed).Choose(static step => Optional(step));
                return Fin.Succ(value: ExchangeReceipt.Programmed(
                    steps: ordered,
                    halted: ordered.Count < rows.Count));
            },
            Fail: failure => System.Threading.Tasks.Task.FromResult(Fin.Fail<ExchangeReceipt>(error: failure)));
    }

    private static Fin<ExchangeReceipt> Use(
        DocumentSession session,
        ExchangeOp request,
        ExchangeHalt halt,
        Op op,
        Option<MutationTrace> trace) {
        using (session) {
            return Apply(session: session, request: request, op: op, halt: halt, trace: trace);
        }
    }

    private static Fin<ExchangeReceipt> Recorded(
        RhinoDoc document,
        DocumentOp edit,
        bool dirty,
        ExchangeHalt halt,
        Op op,
        Option<MutationTrace> trace) {
        if (halt.Requested) {
            return Fin.Succ(value: ExchangeReceipt.Programmed(steps: Seq<ExchangeStep>(), halted: true));
        }
        if (!edit.Profile.Mutates) {
            return Dispatch(document: document, operation: edit, dirty: dirty, op: op);
        }
        return from _attempt in Entered(trace: trace, enabled: true)
               from receipt in DocumentCommit.Sealed(
                   document: document,
                   name: edit.Profile.Surface,
                   recordsUndo: true,
                   redraw: RedrawPolicy.None,
                   run: () => Dispatch(document: document, operation: edit, dirty: dirty, op: op),
                   stamp: (value, serial) => value.Add(new ExchangeEvidence.MutationCase(
                       Surface: edit.Profile.Surface,
                       Attempted: true,
                       Committed: true,
                       MayRemain: false,
                       UndoRecord: serial > 0u ? Some(serial) : None)),
                   op: op)
               select receipt;
    }

    private sealed record ProgramFold(Seq<ExchangeStep> RevSteps, bool Stopped, bool Halted);

    private static ExchangeReceipt Program<T>(Seq<T> rows, ExchangeHalt halt, bool continueOnError, Func<T, int, ExchangeStep> one) {
        ProgramFold folded = rows.Map(static (row, index) => (Row: row, Index: index)).Fold(
            new ProgramFold(RevSteps: Seq<ExchangeStep>(), Stopped: false, Halted: false),
            (state, item) => {
                if (state.Stopped) {
                    return state;
                }
                if (halt.Requested) {
                    return state with { Stopped = true, Halted = true };
                }
                ExchangeStep step = one(item.Row, item.Index);
                return new ProgramFold(
                    RevSteps: step.Cons(state.RevSteps),
                    Stopped: step.Halted || (!continueOnError && step.Failed),
                    Halted: step.Halted);
            });
        return ExchangeReceipt.Programmed(steps: folded.RevSteps.Rev(), halted: folded.Halted);
    }

    private static ExchangeStep Step(int index, Func<Option<MutationTrace>, Fin<ExchangeReceipt>> run) {
        MutationTrace trace = MutationTrace.Fresh();
        return run(Some(trace)).Match<ExchangeStep>(
            Succ: receipt => new ExchangeStep.SucceededCase(
                Index: index,
                MutationAttempted: trace.Attempted
                    || receipt.Program.Map(static program => program.MutationAttempted).IfNone(noneValue: false),
                Receipt: receipt),
            Fail: failure => new ExchangeStep.FailedCase(
                Index: index,
                MutationAttempted: trace.Attempted,
                Failure: failure));
    }

    private static Fin<Unit> Entered(Option<MutationTrace> trace, bool enabled) =>
        trace.Map(held => held.Enter(enabled: enabled)).IfNone(Fin.Succ(value: unit));

    private static Fin<FileCodec> Settled(Option<FileCodec> codec, DocumentPath path, Op op) =>
        codec.Map(static row => Fin.Succ(value: row))
            .IfNone(() => Codecs.Detect(path: path.Value).ToFin(Fail: op.InvalidInput()));

    private static Fin<UInt128> Keyed(string path, Op op) =>
        op.Catch(() => Fin.Succ(value: ContentHash.Of(canonicalBytes: System.IO.File.ReadAllBytes(path: path))));

    private static Fin<ExchangeReceipt> Dispatch(RhinoDoc document, DocumentOp operation, bool dirty, Op op) =>
        operation.Switch(
            (Document: document, Dirty: dirty, Op: op),
            importCase: static (ctx, edit) =>
                from tune in ctx.Op.Need(edit.Tune)
                from codec in Settled(codec: edit.Codec, path: edit.Source, op: ctx.Op)
                from _read in Codecs.Apply(
                    document: ctx.Document,
                    path: edit.Source,
                    codec: codec,
                    tune: tune,
                    request: new CodecRequest.ImportCase(Carrier: new FileReadOptions { ImportMode = true }),
                    key: ctx.Op)
                select ExchangeReceipt.One(fact: new ExchangeFact.ImportedCase(Source: edit.Source, Codec: codec)),
            exportCase: static (ctx, edit) =>
                from scope in ctx.Op.Need(edit.Scope)
                from tune in ctx.Op.Need(edit.Tune)
                from output in ctx.Op.Need(edit.Output)
                from codec in Settled(codec: edit.Codec, path: edit.Target, op: ctx.Op)
                from settled in output.Resolve(target: edit.Target, codec: codec, key: ctx.Op)
                from carrier in scope.Carrier(codec: codec, op: ctx.Op)
                from _written in Codecs.Apply(
                    document: ctx.Document,
                    path: settled,
                    codec: codec,
                    tune: tune,
                    request: new CodecRequest.ExportCase(Carrier: carrier),
                    key: ctx.Op)
                from keyed in Keyed(path: settled.Value, op: ctx.Op)
                select ExchangeReceipt.Of(
                    facts: Seq<ExchangeFact>(new ExchangeFact.ArtifactCase(Target: settled, Codec: codec, ContentKey: keyed)),
                    evidence: Seq<ExchangeEvidence>()),
            saveCase: static (ctx, _) =>
                ctx.Dirty
                    ? from _path in guard(!string.IsNullOrWhiteSpace(value: ctx.Document.Path), ctx.Op.MissingContext()).ToFin()
                      from _saved in ctx.Op.Catch(() => ctx.Op.Confirm(success: ctx.Document.Save()))
                      select ExchangeReceipt.One(fact: new ExchangeFact.SaveCase(Written: true))
                    : Fin.Succ(value: ExchangeReceipt.One(fact: new ExchangeFact.SaveCase(Written: false))),
            writeCase: static (ctx, edit) =>
                from output in ctx.Op.Need(edit.Output)
                from policy in ctx.Op.Need(edit.Policy)
                from codec in policy.Codec(target: edit.Target, op: ctx.Op)
                from settled in output.Resolve(target: edit.Target, codec: Some(codec), key: ctx.Op)
                from _written in policy.Write(document: ctx.Document, path: settled.Value, op: ctx.Op)
                from keyed in Keyed(path: settled.Value, op: ctx.Op)
                select ExchangeReceipt.One(
                    fact: new ExchangeFact.ArtifactCase(Target: settled, Codec: codec, ContentKey: keyed)),
            geometryCase: static (ctx, edit) =>
                from _rows in guard(!edit.Geometry.IsEmpty, ctx.Op.InvalidInput()).ToFin()
                from policy in ctx.Op.Need(edit.Policy)
                from output in ctx.Op.Need(edit.Output)
                from archived in Codecs.Archive.ToFin(Fail: ctx.Op.InvalidResult())
                from landed in ctx.Op.Catch(() => {
                    using File3dm archive = new();
                    using ObjectAttributes attributes = new();
                    Seq<Guid> added = edit.Geometry.Map(row => archive.Objects.Add(item: row, attributes: attributes)).Strict();
                    return guard(added.ForAll(static id => id != Guid.Empty), ctx.Op.InvalidResult()).ToFin().Bind(_ =>
                        Archives.Land(archive: archive, target: edit.Target, policy: policy, output: output, op: ctx.Op));
                })
                select ExchangeReceipt.Of(
                    facts: Seq<ExchangeFact>(new ExchangeFact.ArtifactCase(
                        Target: landed.Target,
                        Codec: archived,
                        ContentKey: landed.ContentKey)),
                    evidence: landed.Stage.Map(text => (ExchangeEvidence)new ExchangeEvidence.NativeCase(
                        Surface: nameof(File3dm.WriteWithLog),
                        Succeeded: true,
                        Detail: text,
                        Target: Some(landed.Target))).ToSeq()),
            anchorCase: static (ctx, edit) =>
                ctx.Op.Need(edit.Edit)
                    .Bind(request => request.Apply(document: ctx.Document, op: ctx.Op))
                    .Map(yield => ExchangeReceipt.One(fact: new ExchangeFact.AnchorCase(Yield: yield))));
}
```

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Rhino exchange transaction rail
    accDescr: One session-proved exchange request dispatches through host operation families into one detached receipt, while cancellation gates entry and typed evidence records native, degradation, and mutation outcomes.
    Session["DocumentSession — lifetime, needs, Modified"] --> Entry["Exchanges.Run"]
    Request["ExchangeOp — Edit · Preset · Batch"] --> Entry
    Halt["ExchangeHalt — case-boundary cancellation"] --> Entry
    Entry -->|"derived needs"| Demand{"Capability?"}
    Demand -->|"read or export"| Dispatch{"DocumentOp case?"}
    Demand -->|"mutation"| Undo["DocumentCommit.Sealed"]
    Undo --> Dispatch
    Dispatch -->|"import or export"| Matrix["Codecs rows"]
    Dispatch -->|"write or geometry"| Output["settled path · content key"]
    Entry -->|"preset request"| Tables["Presets.Commit"]
    Dispatch -->|"anchor"| Geo["forward · inverse"]
    Matrix --> Receipt[/ExchangeReceipt/]
    Output --> Receipt
    Tables --> Receipt
    Geo --> Receipt
    Evidence[(ExchangeEvidence)] -.->|"native · degradation · mutation"| Receipt
    Convert["Run — independent headless sessions"] --> Entry
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
