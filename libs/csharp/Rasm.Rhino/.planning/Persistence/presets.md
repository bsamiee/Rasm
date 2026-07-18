# [DOCUMENT_PRESETS]

`PresetOperation` retains every construction-plane, named-position, and named-layer-state verb through one interpreter. `Presets.Commit` derives session needs, undo, redraw, and answer shape from that operation; named views remain viewport ownership.

## [01]-[DETACHED_MODELS]

`CPlaneModel` carries the complete `ConstructionPlane` field set. `PositionSnapshot` captures participant transforms, while `LayerStateSnapshot` stops at the host's readable name roster.

```csharp signature
namespace Rasm.Rhino.Persistence;

using System.Drawing;
using LanguageExt;
using Rhino.Geometry;
using Thinktecture;

[ValueObject<string>]
public readonly partial struct PresetName
{
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            validationError = new ValidationError("Preset name is empty.");
            return;
        }

        value = value.Trim();
        validationError = null;
    }
}

[ComplexValueObject]
public sealed partial record CPlaneGrid(
    double GridSpacing,
    double SnapSpacing,
    int GridLineCount,
    int ThickLineFrequency)
{
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref double gridSpacing,
        ref double snapSpacing,
        ref int gridLineCount,
        ref int thickLineFrequency) =>
        validationError = !double.IsFinite(gridSpacing) || gridSpacing <= 0.0
            || !double.IsFinite(snapSpacing) || snapSpacing <= 0.0
            || gridLineCount < 1 || thickLineFrequency < 1
            ? new ValidationError("Construction-plane grid metrics are invalid.")
            : null;
}

public sealed record CPlaneVisibility(
    bool ShowGrid,
    bool ShowAxes,
    bool ShowZAxis,
    bool DepthBuffered);

public sealed record CPlanePalette(
    Color ThinLine,
    Color ThickLine,
    Color XAxis,
    Color YAxis,
    Color ZAxis);

[ComplexValueObject]
public sealed partial record CPlaneModel(
    PresetName Name,
    Plane Plane,
    CPlaneGrid Grid,
    CPlaneVisibility Visibility,
    CPlanePalette Palette)
{
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref PresetName name,
        ref Plane plane,
        ref CPlaneGrid grid,
        ref CPlaneVisibility visibility,
        ref CPlanePalette palette) =>
        validationError = plane.IsValid
            && grid is not null
            && visibility is not null
            && palette is not null
            ? null
            : new ValidationError("Construction-plane model is incomplete or invalid.");
}

public sealed record PositionObject(Guid ObjectId, Transform Transform);

public sealed record PositionSnapshot(
    Guid Id,
    PresetName Name,
    Seq<PositionObject> Objects);

public sealed record LayerStateSnapshot(Seq<PresetName> Names);

public sealed record PresetSnapshot(
    Seq<CPlaneModel> ConstructionPlanes,
    Seq<PositionSnapshot> Positions,
    LayerStateSnapshot LayerStates);
```

## [02]-[POLICY_AND_OPERATION]

`LayerFacet` derives every host flag from `RestoreLayerProperties`; no numeric mask is duplicated. `PresetOperation` includes reads and mutations, so a new table verb breaks one total dispatcher.

```csharp signature
namespace Rasm.Rhino.Persistence;

using LanguageExt;
using Rasm.Rhino.Document;
using Rhino.DocObjects.Tables;
using Thinktecture;

[SmartEnum<string>]
public sealed partial class LayerFacet
{
    public static readonly LayerFacet Current = new("current", RestoreLayerProperties.Current);
    public static readonly LayerFacet Visible = new("visible", RestoreLayerProperties.Visible);
    public static readonly LayerFacet Locked = new("locked", RestoreLayerProperties.Locked);
    public static readonly LayerFacet Color = new("color", RestoreLayerProperties.Color);
    public static readonly LayerFacet Linetype = new("linetype", RestoreLayerProperties.Linetype);
    public static readonly LayerFacet PrintColor = new("print-color", RestoreLayerProperties.PrintColor);
    public static readonly LayerFacet PrintWidth = new("print-width", RestoreLayerProperties.PrintWidth);
    public static readonly LayerFacet ViewportVisible = new("viewport-visible", RestoreLayerProperties.ViewportVisible);
    public static readonly LayerFacet ViewportColor = new("viewport-color", RestoreLayerProperties.ViewportColor);
    public static readonly LayerFacet ViewportPrintColor = new("viewport-print-color", RestoreLayerProperties.ViewportPrintColor);
    public static readonly LayerFacet ViewportPrintWidth = new("viewport-print-width", RestoreLayerProperties.ViewportPrintWidth);
    public static readonly LayerFacet RenderMaterial = new("render-material", RestoreLayerProperties.RenderMaterial);
    public static readonly LayerFacet SectionStyle = new("section-style", RestoreLayerProperties.SectionStyle);
    public static readonly LayerFacet NewDetailOn = new("new-detail-on", RestoreLayerProperties.NewDetailOn);
    public static readonly LayerFacet Expanded = new("expanded", RestoreLayerProperties.Expanded);

    internal RestoreLayerProperties Native { get; }
}

[Union]
public abstract partial record LayerRestore
{
    public sealed record AllCase : LayerRestore;
    public sealed record SelectedCase(LayerMask Mask) : LayerRestore;

    internal RestoreLayerProperties ToNative() => Switch<RestoreLayerProperties>(
        allCase: static _ => RestoreLayerProperties.All,
        selectedCase: static selected => selected.Mask.Facets.Fold(
            RestoreLayerProperties.None,
            static (mask, facet) => mask | facet.Native));
}

[ComplexValueObject]
public sealed partial record LayerMask(LanguageExt.HashSet<LayerFacet> Facets)
{
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref LanguageExt.HashSet<LayerFacet> facets) =>
        validationError = facets.IsEmpty
            ? new ValidationError("Layer restore mask is empty.")
            : null;
}

[Union]
public abstract partial record PositionRef
{
    public sealed record IdCase(Guid Id) : PositionRef;
    public sealed record NameCase(PresetName Name) : PositionRef;
}

[ValueObject<string>]
public readonly partial struct PresetArchivePath
{
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            validationError = new ValidationError("Layer-state import path is empty.");
            return;
        }

        value = value.Trim();
        validationError = !Path.IsPathFullyQualified(value) || !string.Equals(Path.GetExtension(value), ".3dm", StringComparison.OrdinalIgnoreCase)
            ? new ValidationError("Layer-state import requires an absolute .3dm path.")
            : null;
    }
}

[Union]
public abstract partial record PresetOperation
{
    public sealed record ReadAllCase : PresetOperation;
    public sealed record PutCPlaneCase(CPlaneModel Model) : PresetOperation;
    public sealed record DeleteCPlaneCase(PresetName Name) : PresetOperation;
    public sealed record SavePositionCase(PresetName Name, Seq<Guid> ObjectIds) : PresetOperation;
    public sealed record RestorePositionCase(PositionRef Position) : PresetOperation;
    public sealed record UpdatePositionCase(PositionRef Position) : PresetOperation;
    public sealed record AppendPositionCase(PositionRef Position, Seq<Guid> ObjectIds) : PresetOperation;
    public sealed record RenamePositionCase(PositionRef Position, PresetName Name) : PresetOperation;
    public sealed record DeletePositionCase(PositionRef Position) : PresetOperation;
    public sealed record ReadPositionTransformCase(PositionRef Position, Guid ObjectId) : PresetOperation;
    public sealed record SaveLayerStateCase(PresetName Name, Option<Guid> ViewportId) : PresetOperation;
    public sealed record RestoreLayerStateCase(PresetName Name, LayerRestore Properties, Option<Guid> ViewportId) : PresetOperation;
    public sealed record RenameLayerStateCase(PresetName Current, PresetName Next) : PresetOperation;
    public sealed record DeleteLayerStateCase(PresetName Name) : PresetOperation;
    public sealed record ImportLayerStatesCase(PresetArchivePath Path) : PresetOperation;

    internal PresetExecution Execution => this switch
    {
        ReadAllCase or ReadPositionTransformCase => PresetExecution.Read,
        RestorePositionCase or RestoreLayerStateCase => PresetExecution.Restore,
        _ => PresetExecution.Mutate,
    };
}

[SmartEnum<string>]
public sealed partial class PresetExecution
{
    public static readonly PresetExecution Read = new(
        "read",
        mutation: false,
        redraw: RedrawPolicy.None,
        needs: static () => [SessionNeed.Read]);
    public static readonly PresetExecution Mutate = new(
        "mutate",
        mutation: true,
        redraw: RedrawPolicy.None,
        needs: static () => [SessionNeed.Read, SessionNeed.Mutate, SessionNeed.Undo]);
    public static readonly PresetExecution Restore = new(
        "restore",
        mutation: true,
        redraw: RedrawPolicy.Continuous,
        needs: static () => [SessionNeed.Read, SessionNeed.Mutate, SessionNeed.Undo, SessionNeed.Redraw]);

    public bool Mutation { get; }

    public RedrawPolicy Redraw { get; }

    [UseDelegateFromConstructor]
    internal partial SessionNeed[] Needs();
}

public sealed record PresetRosters(
    Seq<PresetName> ConstructionPlanes,
    Seq<PresetName> Positions,
    Seq<PresetName> LayerStates);

public sealed record PresetMutationReceipt(
    PresetOperation Operation,
    Option<PresetName> Name,
    Option<Guid> Id,
    int Affected,
    PresetRosters RostersAfter,
    Option<uint> UndoSerial);

[Union]
public abstract partial record PresetAnswer
{
    public sealed record SnapshotCase(PresetSnapshot Snapshot) : PresetAnswer;
    public sealed record TransformCase(PositionObject Object) : PresetAnswer;
    public sealed record MutationCase(PresetMutationReceipt Receipt) : PresetAnswer;
}
```

## [03]-[INTERPRETER]

`PresetOperation.Execution` projects each case onto one policy row that owns mutation, redraw, and session needs. Mutations use one `UndoBracket`, restore cases request redraw, and read cases request only `SessionNeed.Read`.

Rhino's table mutation, ref-parameter transform read, undo, and redraw calls form the platform-forced statement seam. `Apply` remains exhaustive across the full operation family, including typed rejection of read cases.

```csharp signature
namespace Rasm.Rhino.Persistence;

using LanguageExt;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Geometry;
using static LanguageExt.Prelude;

public static class Presets
{
    public static Fin<PresetAnswer> Commit(
        DocumentSession session,
        PresetOperation operation,
        Op? key = null)
    {
        Op op = key.OrDefault();
        return from owner in Optional(session).ToFin(op.MissingContext())
               from active in Optional(operation).ToFin(op.InvalidInput()).Bind(value => Admit(value, op))
               let execution = active.Execution
               from answer in owner.Demand(
                   use: document => execution.Mutation
                       ? Mutate(document, active, op, execution.Redraw)
                       : Read(document, active, op),
                   key: op,
                   needs: execution.Needs())
               select answer;
    }

    private static Fin<PresetOperation> Admit(PresetOperation operation, Op op) => operation.Switch<Op, Fin<PresetOperation>>(
        state: op,
        readAllCase: static (_, _) => Succ<PresetOperation>(new PresetOperation.ReadAllCase()),
        putCPlaneCase: static (op, value) => Admit(value.Model, op)
            .Map<PresetOperation>(static model => new PresetOperation.PutCPlaneCase(model)),
        deleteCPlaneCase: static (op, value) => AdmitName(value.Name, op)
            .Map<PresetOperation>(static name => new PresetOperation.DeleteCPlaneCase(name)),
        savePositionCase: static (op, value) =>
            from name in AdmitName(value.Name, op)
            from ids in AdmitIds(value.ObjectIds, op)
            select (PresetOperation)new PresetOperation.SavePositionCase(name, ids),
        restorePositionCase: static (op, value) => Admit(value.Position, op)
            .Map<PresetOperation>(static position => new PresetOperation.RestorePositionCase(position)),
        updatePositionCase: static (op, value) => Admit(value.Position, op)
            .Map<PresetOperation>(static position => new PresetOperation.UpdatePositionCase(position)),
        appendPositionCase: static (op, value) =>
            from position in Admit(value.Position, op)
            from ids in AdmitIds(value.ObjectIds, op)
            select (PresetOperation)new PresetOperation.AppendPositionCase(position, ids),
        renamePositionCase: static (op, value) =>
            from position in Admit(value.Position, op)
            from name in AdmitName(value.Name, op)
            select (PresetOperation)new PresetOperation.RenamePositionCase(position, name),
        deletePositionCase: static (op, value) => Admit(value.Position, op)
            .Map<PresetOperation>(static position => new PresetOperation.DeletePositionCase(position)),
        readPositionTransformCase: static (op, value) =>
            from position in Admit(value.Position, op)
            from _object in guard(value.ObjectId != Guid.Empty, op.InvalidInput()).ToFin()
            select (PresetOperation)new PresetOperation.ReadPositionTransformCase(position, value.ObjectId),
        saveLayerStateCase: static (op, value) =>
            from name in AdmitName(value.Name, op)
            from viewport in AdmitViewport(value.ViewportId, op)
            select (PresetOperation)new PresetOperation.SaveLayerStateCase(name, viewport),
        restoreLayerStateCase: static (op, value) =>
            from name in AdmitName(value.Name, op)
            from properties in Admit(value.Properties, op)
            from viewport in AdmitViewport(value.ViewportId, op)
            select (PresetOperation)new PresetOperation.RestoreLayerStateCase(name, properties, viewport),
        renameLayerStateCase: static (op, value) =>
            from current in AdmitName(value.Current, op)
            from next in AdmitName(value.Next, op)
            select (PresetOperation)new PresetOperation.RenameLayerStateCase(current, next),
        deleteLayerStateCase: static (op, value) => AdmitName(value.Name, op)
            .Map<PresetOperation>(static name => new PresetOperation.DeleteLayerStateCase(name)),
        importLayerStatesCase: static (op, value) => op.AcceptValidated<PresetArchivePath>(value.Path.Value)
            .Map<PresetOperation>(static path => new PresetOperation.ImportLayerStatesCase(path)));

    private static Fin<CPlaneModel> Admit(CPlaneModel? model, Op op) =>
        from source in Optional(model).ToFin(Fail: op.InvalidInput())
        from name in AdmitName(source.Name, op)
        from grid in Admit(source.Grid, op)
        from visibility in Optional(source.Visibility).ToFin(Fail: op.InvalidInput())
        from palette in Optional(source.Palette).ToFin(Fail: op.InvalidInput())
        from admitted in op.AcceptValidated<CPlaneModel>(
            CPlaneModel.Validate(name, source.Plane, grid, visibility, palette, out CPlaneModel? value),
            value)
        select admitted;

    private static Fin<CPlaneGrid> Admit(CPlaneGrid? grid, Op op) =>
        from source in Optional(grid).ToFin(Fail: op.InvalidInput())
        from admitted in op.AcceptValidated<CPlaneGrid>(
            CPlaneGrid.Validate(
                source.GridSpacing,
                source.SnapSpacing,
                source.GridLineCount,
                source.ThickLineFrequency,
                out CPlaneGrid? value),
            value)
        select admitted;

    private static Fin<PositionRef> Admit(PositionRef? position, Op op) =>
        Optional(position).ToFin(Fail: op.InvalidInput()).Bind(value => value.Switch<Op, Fin<PositionRef>>(
            state: op,
            idCase: static (op, id) => guard(id.Id != Guid.Empty, op.InvalidInput()).ToFin()
                .Map<PositionRef>(_ => new PositionRef.IdCase(id.Id)),
            nameCase: static (op, name) => AdmitName(name.Name, op)
                .Map<PositionRef>(static admitted => new PositionRef.NameCase(admitted))));

    private static Fin<LayerRestore> Admit(LayerRestore? restore, Op op) =>
        Optional(restore).ToFin(Fail: op.InvalidInput()).Bind(value => value.Switch<Op, Fin<LayerRestore>>(
            state: op,
            allCase: static (_, _) => Succ<LayerRestore>(new LayerRestore.AllCase()),
            selectedCase: static (op, selected) => Admit(selected.Mask, op)
                .Map<LayerRestore>(static mask => new LayerRestore.SelectedCase(mask))));

    private static Fin<LayerMask> Admit(LayerMask? mask, Op op) =>
        from source in Optional(mask).ToFin(Fail: op.InvalidInput())
        from _facets in guard(source.Facets.ForAll(static facet => facet is not null), op.InvalidInput()).ToFin()
        from admitted in op.AcceptValidated<LayerMask>(LayerMask.Validate(source.Facets, out LayerMask? value), value)
        select admitted;

    private static Fin<PresetName> AdmitName(PresetName name, Op op) =>
        op.AcceptValidated<PresetName>(name.Value);

    private static Fin<Option<Guid>> AdmitViewport(Option<Guid> viewport, Op op) => viewport.Match(
        Some: id => guard(id != Guid.Empty, op.InvalidInput()).ToFin().Map(_ => Some(id)),
        None: static () => Succ(Option<Guid>.None));

    private static Fin<PresetAnswer> Read(RhinoDoc document, PresetOperation operation, Op op) => operation switch
    {
        PresetOperation.ReadAllCase => Snapshot(document, op).Map<PresetAnswer>(static value => new PresetAnswer.SnapshotCase(value)),
        PresetOperation.ReadPositionTransformCase read =>
            from id in Resolve(document.NamedPositions, read.Position, op)
            from transform in ObjectTransform(document.NamedPositions, id, read.ObjectId, op)
            select (PresetAnswer)new PresetAnswer.TransformCase(new PositionObject(read.ObjectId, transform)),
        _ => Fail<PresetAnswer>(op.InvalidInput())
    };

    private static Fin<PresetAnswer> Mutate(
        RhinoDoc document,
        PresetOperation operation,
        Op op,
        RedrawPolicy redraw) =>
        DocumentCommit.Sealed(
            document: document,
            name: nameof(Commit),
            recordsUndo: true,
            redraw: redraw,
            run: () => Apply(document, operation, op),
            stamp: static (receipt, serial) => receipt with { UndoSerial = serial > 0u ? Some(serial) : None },
            op: op)
        .Map<PresetAnswer>(static receipt => new PresetAnswer.MutationCase(receipt));

    private static Fin<PresetMutationReceipt> Apply(RhinoDoc document, PresetOperation operation, Op op) =>
        operation.Switch<Fin<PresetMutationReceipt>>(
        readAllCase: _ => Fail<PresetMutationReceipt>(op.InvalidInput()),
        putCPlaneCase: put => PutCPlane(document, put.Model, op),
        deleteCPlaneCase: delete => Confirm(
            () => document.NamedConstructionPlanes.Delete(delete.Name.Value),
            operation,
            Some(delete.Name),
            None,
            1,
            document,
            op),
        savePositionCase: save =>
            from id in op.Catch(() => Fin.Succ(value: document.NamedPositions.Save(save.Name.Value, save.ObjectIds)))
            from accepted in id != Guid.Empty
                ? Receipt(operation, Some(save.Name), Some(id), save.ObjectIds.Count, document, op)
                : Fail<PresetMutationReceipt>(op.InvalidResult(detail: "Named position save was rejected."))
            select accepted,
        restorePositionCase: restore => PositionBool(
            document.NamedPositions,
            restore.Position,
            static (table, id) => table.Restore(id),
            operation,
            document,
            op),
        updatePositionCase: update => PositionBool(
            document.NamedPositions,
            update.Position,
            static (table, id) => table.Update(id),
            operation,
            document,
            op),
        appendPositionCase: append =>
            from id in Resolve(document.NamedPositions, append.Position, op)
            from name in PositionName(document.NamedPositions, id, op)
            from receipt in Confirm(
                () => document.NamedPositions.Append(id, append.ObjectIds),
                operation,
                name,
                Some(id),
                append.ObjectIds.Count,
                document,
                op)
            select receipt,
        renamePositionCase: rename =>
            from id in Resolve(document.NamedPositions, rename.Position, op)
            from receipt in Confirm(
                () => document.NamedPositions.Rename(id, rename.Name.Value),
                operation,
                Some(rename.Name),
                Some(id),
                1,
                document,
                op)
            select receipt,
        deletePositionCase: delete =>
            from id in Resolve(document.NamedPositions, delete.Position, op)
            from name in PositionName(document.NamedPositions, id, op)
            from receipt in Confirm(
                () => document.NamedPositions.Delete(id),
                operation,
                name,
                Some(id),
                1,
                document,
                op)
            select receipt,
        readPositionTransformCase: _ => Fail<PresetMutationReceipt>(op.InvalidInput()),
        saveLayerStateCase: save =>
            from index in op.Catch(() => Fin.Succ(value: save.ViewportId.Match(
                Some: viewport => document.NamedLayerStates.Save(save.Name.Value, viewport),
                None: () => document.NamedLayerStates.Save(save.Name.Value))))
            from receipt in index >= 0
                ? Receipt(operation, Some(save.Name), None, 1, document, op)
                : Fail<PresetMutationReceipt>(op.InvalidResult(detail: "Named layer state save was rejected."))
            select receipt,
        restoreLayerStateCase: restore => Confirm(
            () => restore.ViewportId.Match(
                Some: viewport => document.NamedLayerStates.Restore(restore.Name.Value, restore.Properties.ToNative(), viewport),
                None: () => document.NamedLayerStates.Restore(restore.Name.Value, restore.Properties.ToNative())),
            operation,
            Some(restore.Name),
            None,
            1,
            document,
            op),
        renameLayerStateCase: rename => Confirm(
            () => document.NamedLayerStates.Rename(rename.Current.Value, rename.Next.Value),
            operation,
            Some(rename.Next),
            None,
            1,
            document,
            op),
        deleteLayerStateCase: delete => Confirm(
            () => document.NamedLayerStates.Delete(delete.Name.Value),
            operation,
            Some(delete.Name),
            None,
            1,
            document,
            op),
        importLayerStatesCase: import =>
            from count in op.Catch(() => Fin.Succ(value: document.NamedLayerStates.Import(import.Path.Value)))
            from receipt in count >= 0
                ? Receipt(operation, None, None, count, document, op)
                : Fail<PresetMutationReceipt>(op.InvalidResult(detail: "Named layer-state import was rejected."))
            select receipt);

    private static Fin<PresetMutationReceipt> PutCPlane(RhinoDoc document, CPlaneModel model, Op op) =>
        from native in op.Catch(() => Fin.Succ(value: new ConstructionPlane
        {
            Name = model.Name.Value,
            Plane = model.Plane,
            GridSpacing = model.Grid.GridSpacing,
            SnapSpacing = model.Grid.SnapSpacing,
            GridLineCount = model.Grid.GridLineCount,
            ThickLineFrequency = model.Grid.ThickLineFrequency,
            ShowGrid = model.Visibility.ShowGrid,
            ShowAxes = model.Visibility.ShowAxes,
            ShowZAxis = model.Visibility.ShowZAxis,
            DepthBuffered = model.Visibility.DepthBuffered,
            ThinLineColor = model.Palette.ThinLine,
            ThickLineColor = model.Palette.ThickLine,
            GridXColor = model.Palette.XAxis,
            GridYColor = model.Palette.YAxis,
            GridZColor = model.Palette.ZAxis
        }))
        from index in op.Catch(() => Fin.Succ(value: document.NamedConstructionPlanes.Add(native)))
        from receipt in index >= 0
            ? Receipt(new PresetOperation.PutCPlaneCase(model), Some(model.Name), None, 1, document, op)
            : Fail<PresetMutationReceipt>(op.InvalidResult(detail: "Construction-plane preset write was rejected."))
        select receipt;

    private static Fin<PresetSnapshot> Snapshot(RhinoDoc document, Op op) =>
        from cplanes in Project(
            source: () => document.NamedConstructionPlanes,
            project: value => Capture(value, op),
            op: op)
        from positions in Project(
            source: () => document.NamedPositions.Ids,
            project: id => Capture(document.NamedPositions, id, op),
            op: op)
        from layers in Names(() => document.NamedLayerStates.Names, op)
        select new PresetSnapshot(cplanes, positions, new LayerStateSnapshot(layers));

    private static Fin<CPlaneModel> Capture(ConstructionPlane source, Op op) =>
        from name in op.AcceptValidated<PresetName>(source.Name)
        from grid in op.Catch(() => Fin.Succ(value: CPlaneGrid.Create(
            source.GridSpacing,
            source.SnapSpacing,
            source.GridLineCount,
            source.ThickLineFrequency)))
        from model in op.Catch(() => Fin.Succ(value: CPlaneModel.Create(
            name,
            source.Plane,
            grid,
            new CPlaneVisibility(source.ShowGrid, source.ShowAxes, source.ShowZAxis, source.DepthBuffered),
            new CPlanePalette(source.ThinLineColor, source.ThickLineColor, source.GridXColor, source.GridYColor, source.GridZColor))))
        select model;

    private static Fin<PositionSnapshot> Capture(NamedPositionTable table, Guid id, Op op) =>
        from name in op.AcceptValidated<PresetName>(table.Name(id))
        from objects in Project(
            source: () => table.ObjectIds(id),
            project: objectId => ObjectTransform(table, id, objectId, op)
                .Map(transform => new PositionObject(objectId, transform)),
            op: op)
        select new PositionSnapshot(id, name, objects);

    private static Fin<Transform> ObjectTransform(NamedPositionTable table, Guid id, Guid objectId, Op op) =>
        op.Catch(() =>
        {
            Transform transform = Transform.Unset;
            return table.ObjectXform(id, objectId, ref transform) && transform.IsValid
                ? Fin.Succ(value: transform)
                : Fin.Fail<Transform>(error: op.InvalidResult(detail: $"Named position '{id}' has no valid transform for '{objectId}'."));
        });

    private static Fin<Guid> Resolve(NamedPositionTable table, PositionRef position, Op op) =>
        from candidate in position.Switch<(NamedPositionTable Table, Op Op), Fin<Guid>>(
            state: (table, op),
            idCase: static (state, id) => id.Id != Guid.Empty
                ? Succ(id.Id)
                : Fail<Guid>(state.Op.InvalidInput()),
            nameCase: static (state, name) => state.Op.Catch(() => Fin.Succ(value: state.Table.Id(name.Name.Value))))
        from present in op.Catch(() => Fin.Succ(value: table.Ids.Contains(candidate)))
        from admitted in present
            ? Succ(candidate)
            : Fail<Guid>(op.MissingContext())
        select admitted;

    private static Fin<PresetMutationReceipt> PositionBool(
        NamedPositionTable table,
        PositionRef position,
        Func<NamedPositionTable, Guid, bool> mutate,
        PresetOperation operation,
        RhinoDoc document,
        Op op) =>
        from id in Resolve(table, position, op)
        from name in PositionName(table, id, op)
        from receipt in Confirm(
            () => mutate(table, id),
            operation,
            name,
            Some(id),
            1,
            document,
            op)
        select receipt;

    private static Fin<Seq<Guid>> AdmitIds(Seq<Guid> ids, Op op) =>
        !ids.IsEmpty
        && ids.ForAll(static id => id != Guid.Empty)
        && ids.Distinct().Count == ids.Count
            ? Succ(ids)
            : Fail<Seq<Guid>>(op.InvalidInput());

    private static Fin<Option<PresetName>> PositionName(NamedPositionTable table, Guid id, Op op) =>
        op.Catch(() => Fin.Succ(value: Optional(table.Name(id))))
            .Bind(name => name.Match(
                Some: value => op.AcceptValidated<PresetName>(value).Map(Some),
                None: static () => Succ(Option<PresetName>.None)));

    private static Fin<Seq<PresetName>> Names(Func<IEnumerable<string>> source, Op op) =>
        Project(source, name => op.AcceptValidated<PresetName>(name), op)
            .Map(values => values.OrderBy(static value => value.Value, StringComparer.Ordinal).ToSeq());

    private static Fin<Seq<TResult>> Project<TSource, TResult>(
        Func<IEnumerable<TSource>> source,
        Func<TSource, Fin<TResult>> project,
        Op op) =>
        op.Catch(() => toSeq(source())
            .Map(project)
            .Traverse(static value => value));

    private static Fin<PresetRosters> Rosters(RhinoDoc document, Op op) =>
        from cplanes in Names(() => document.NamedConstructionPlanes.Map(static value => value.Name), op)
        from positions in Names(() => document.NamedPositions.Names, op)
        from layers in Names(() => document.NamedLayerStates.Names, op)
        select new PresetRosters(cplanes, positions, layers);

    private static Fin<PresetMutationReceipt> Confirm(
        Func<bool> mutate,
        PresetOperation operation,
        Option<PresetName> name,
        Option<Guid> id,
        int affected,
        RhinoDoc document,
        Op op) =>
        from _accepted in op.Catch(() => op.Confirm(success: mutate()))
        from receipt in Receipt(operation, name, id, affected, document, op)
        select receipt;

    private static Fin<PresetMutationReceipt> Receipt(
        PresetOperation operation,
        Option<PresetName> name,
        Option<Guid> id,
        int affected,
        RhinoDoc document,
        Op op) =>
        Rosters(document, op).Map(rosters => new PresetMutationReceipt(operation, name, id, affected, rosters, None));
}
```

## [04]-[LIFECYCLE]

`PresetOperation` admits once, `Presets.Commit` derives needs, and one total interpreter reaches the owning Rhino table. Named-position addresses resolve through table membership for both id and name forms, and object-id batches preserve their admitted sequence only when every id is unique. Mutation receipts carry the resolved name/id, affected count, post-operation roster, and sealed undo serial.

Construction-plane projection covers plane, grid, visibility, depth, and five colors. Named-position projection covers identity, participants, and every stored transform. Layer-state reads remain name-only because Rhino exposes no stored-property payload reader.

## [05]-[SEAMS]

`Presets.Commit` composes `DocumentSession` and `UndoBracket` directly. Named views stay in Viewport; the layer tree — topology, face, and per-detail overrides — lives on the Document layer owner, while `LayerRestore` consumes only the host's state mask.
