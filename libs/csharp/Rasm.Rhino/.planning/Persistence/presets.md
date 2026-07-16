# [RASM_RHINO_PERSISTENCE_PRESETS]

Document saved-state presets (`Rasm.Rhino.Persistence`). `CPlaneModel`, `PositionEdit`, and `LayerStateEdit` follow the distinct identity and payload regimes of their host tables. `PresetProgram` unifies only commit timing: one admitted table-specific program, one `DocumentSession` mutation window, one `UndoBracket`, and one redraw policy carried at composition. `PresetRead` returns complete detached table snapshots. Named views remain with the viewport and table owners.

## [01]-[INDEX]

- [02]-[CPLANE]: shared grid vocabulary and full construction-plane projection.
- [03]-[LAYER_SCOPE]: bounded layer-property mask composition.
- [04]-[PROGRAM_RAIL]: table-specific edits under one commit carrier.
- [05]-[READ_RAIL]: complete detached preset snapshots.
- [06]-[SURFACE_LEDGER]: ownership and entry points.

## [02]-[CPLANE]

- Grid owner: `CPlaneGrid` collapses the grid members shared by `ConstructionPlane` and `ConstructionPlaneGridDefaults`; each host carrier retains its distinct world-axis or Z-axis member outside that product.
- Model owner: `CPlaneModel` combines plane identity, shared grid, depth, Z-axis visibility, and concrete colors. Private construction forces plane and grid admission before minting.
- Defaults: `CPlaneDefaults` detaches the grid-default carrier and its world-axis setting; `MintDefaults` reconstructs the full carrier instead of hardcoding constructor values in consumers.
- Table behavior: `NamedConstructionPlaneTable.Add` saves or replaces and returns the resulting index; empty names remain valid because the host mints one. Delete accepts both verified index and name overloads.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Geometry;

namespace Rasm.Rhino.Persistence;

// --- [CPLANE_MODELS] ------------------------------------------------------------------------
public sealed record CPlaneGrid {
    private CPlaneGrid(
        double gridSpacing,
        double snapSpacing,
        int lineCount,
        int thickFrequency,
        bool showGrid,
        bool showAxes) =>
        (GridSpacing, SnapSpacing, LineCount, ThickFrequency, ShowGrid, ShowAxes) =
        (gridSpacing, snapSpacing, lineCount, thickFrequency, showGrid, showAxes);

    public double GridSpacing { get; }
    public double SnapSpacing { get; }
    public int LineCount { get; }
    public int ThickFrequency { get; }
    public bool ShowGrid { get; }
    public bool ShowAxes { get; }

    public static Fin<CPlaneGrid> Create(
        double gridSpacing,
        double snapSpacing,
        int lineCount,
        int thickFrequency,
        bool showGrid,
        bool showAxes,
        Op? key = null) {
        Op op = key.OrDefault();
        return double.IsFinite(gridSpacing) && gridSpacing > 0.0
            && double.IsFinite(snapSpacing) && snapSpacing > 0.0
            && lineCount >= 0 && thickFrequency >= 0
                ? Fin.Succ(value: new CPlaneGrid(
                    gridSpacing: gridSpacing,
                    snapSpacing: snapSpacing,
                    lineCount: lineCount,
                    thickFrequency: thickFrequency,
                    showGrid: showGrid,
                    showAxes: showAxes))
                : Fin.Fail<CPlaneGrid>(error: op.InvalidInput());
    }

    public static Fin<CPlaneGrid> Of(ConstructionPlane source, Op? key = null) {
        Op op = key.OrDefault();
        return from value in Optional(source).ToFin(Fail: op.InvalidInput())
               from grid in Create(
                   gridSpacing: value.GridSpacing,
                   snapSpacing: value.SnapSpacing,
                   lineCount: value.GridLineCount,
                   thickFrequency: value.ThickLineFrequency,
                   showGrid: value.ShowGrid,
                   showAxes: value.ShowAxes,
                   key: op)
               select grid;
    }

    public static Fin<CPlaneGrid> Of(ConstructionPlaneGridDefaults source, Op? key = null) {
        Op op = key.OrDefault();
        return from value in Optional(source).ToFin(Fail: op.InvalidInput())
               from grid in Create(
                   gridSpacing: value.GridSpacing,
                   snapSpacing: value.SnapSpacing,
                   lineCount: value.GridLineCount,
                   thickFrequency: value.GridThickFrequency,
                   showGrid: value.ShowGrid,
                   showAxes: value.ShowGridAxes,
                   key: op)
               select grid;
    }

    internal Unit Apply(ConstructionPlane target) => Op.Side(() => {
        target.GridSpacing = GridSpacing;
        target.SnapSpacing = SnapSpacing;
        target.GridLineCount = LineCount;
        target.ThickLineFrequency = ThickFrequency;
        target.ShowGrid = ShowGrid;
        target.ShowAxes = ShowAxes;
    });

    internal ConstructionPlaneGridDefaults MintDefaults(bool showWorldAxes) => new() {
        GridSpacing = GridSpacing,
        SnapSpacing = SnapSpacing,
        GridLineCount = LineCount,
        GridThickFrequency = ThickFrequency,
        ShowGrid = ShowGrid,
        ShowGridAxes = ShowAxes,
        ShowWorldAxes = showWorldAxes,
    };
}

public sealed record CPlaneDefaults : IDetachedDocumentResult {
    private CPlaneDefaults(CPlaneGrid grid, bool showWorldAxes) => (Grid, ShowWorldAxes) = (grid, showWorldAxes);

    public CPlaneGrid Grid { get; }
    public bool ShowWorldAxes { get; }

    public static Fin<CPlaneDefaults> Of(ConstructionPlaneGridDefaults source, Op? key = null) {
        Op op = key.OrDefault();
        return from value in Optional(source).ToFin(Fail: op.InvalidInput())
               from grid in CPlaneGrid.Of(source: value, key: op)
               select new CPlaneDefaults(grid: grid, showWorldAxes: value.ShowWorldAxes);
    }

    public ConstructionPlaneGridDefaults Mint() => Grid.MintDefaults(showWorldAxes: ShowWorldAxes);
}

public sealed record CPlaneModel : IDetachedDocumentResult {
    private CPlaneModel(
        Plane frame,
        string name,
        CPlaneGrid grid,
        bool depthBuffered,
        bool showZAxis,
        System.Drawing.Color thinLineColor,
        System.Drawing.Color thickLineColor,
        System.Drawing.Color gridXColor,
        System.Drawing.Color gridYColor,
        System.Drawing.Color gridZColor) =>
        (Frame, Name, Grid, DepthBuffered, ShowZAxis, ThinLineColor, ThickLineColor, GridXColor, GridYColor, GridZColor) =
        (frame, name, grid, depthBuffered, showZAxis, thinLineColor, thickLineColor, gridXColor, gridYColor, gridZColor);

    public Plane Frame { get; }
    public string Name { get; }
    public CPlaneGrid Grid { get; }
    public bool DepthBuffered { get; }
    public bool ShowZAxis { get; }
    public System.Drawing.Color ThinLineColor { get; }
    public System.Drawing.Color ThickLineColor { get; }
    public System.Drawing.Color GridXColor { get; }
    public System.Drawing.Color GridYColor { get; }
    public System.Drawing.Color GridZColor { get; }

    public static Fin<CPlaneModel> Create(
        Plane frame,
        string name,
        CPlaneGrid grid,
        bool depthBuffered,
        bool showZAxis,
        System.Drawing.Color thinLineColor,
        System.Drawing.Color thickLineColor,
        System.Drawing.Color gridXColor,
        System.Drawing.Color gridYColor,
        System.Drawing.Color gridZColor,
        Op? key = null) {
        Op op = key.OrDefault();
        return from _ in guard(frame.IsValid, op.InvalidInput()).ToFin()
               from label in Optional(name).ToFin(Fail: op.InvalidInput())
               from metric in Optional(grid).ToFin(Fail: op.InvalidInput())
               select new CPlaneModel(
                   frame: frame,
                   name: label,
                   grid: metric,
                   depthBuffered: depthBuffered,
                   showZAxis: showZAxis,
                   thinLineColor: thinLineColor,
                   thickLineColor: thickLineColor,
                   gridXColor: gridXColor,
                   gridYColor: gridYColor,
                   gridZColor: gridZColor);
    }

    public static Fin<CPlaneModel> Of(ConstructionPlane source, Op? key = null) {
        Op op = key.OrDefault();
        return from value in Optional(source).ToFin(Fail: op.InvalidInput())
               from grid in CPlaneGrid.Of(source: value, key: op)
               from model in Create(
                   frame: value.Plane,
                   name: value.Name ?? string.Empty,
                   grid: grid,
                   depthBuffered: value.DepthBuffered,
                   showZAxis: value.ShowZAxis,
                   thinLineColor: value.ThinLineColor,
                   thickLineColor: value.ThickLineColor,
                   gridXColor: value.GridXColor,
                   gridYColor: value.GridYColor,
                   gridZColor: value.GridZColor,
                   key: op)
               select model;
    }

    internal ConstructionPlane Mint() {
        ConstructionPlane target = new() {
            Plane = Frame,
            Name = Name,
            DepthBuffered = DepthBuffered,
            ShowZAxis = ShowZAxis,
            ThinLineColor = ThinLineColor,
            ThickLineColor = ThickLineColor,
            GridXColor = GridXColor,
            GridYColor = GridYColor,
            GridZColor = GridZColor,
        };
        _ = Grid.Apply(target: target);
        return target;
    }
}
```

## [03]-[LAYER_SCOPE]

- Owner: `LayerFacet` rows carry every verified native property bit. `LayerScope` stores admitted facets as a frozen set.
- Boundary: bitwise folding occurs once in `Mask`, where the native flags enum is required. Empty and complete sets project to the host `None` and `All` members.
- Admission: `Create` rejects null rows and collapses duplicates; `With` uses the same row gate.

```csharp signature
// --- [LAYER_SCOPE] --------------------------------------------------------------------------
[SmartEnum<uint>]
public sealed partial class LayerFacet {
    public static readonly LayerFacet Current = new(key: 0x1u);
    public static readonly LayerFacet Visible = new(key: 0x2u);
    public static readonly LayerFacet Locked = new(key: 0x4u);
    public static readonly LayerFacet Color = new(key: 0x8u);
    public static readonly LayerFacet Linetype = new(key: 0x10u);
    public static readonly LayerFacet PrintColor = new(key: 0x20u);
    public static readonly LayerFacet PrintWidth = new(key: 0x40u);
    public static readonly LayerFacet ViewportVisible = new(key: 0x80u);
    public static readonly LayerFacet ViewportColor = new(key: 0x100u);
    public static readonly LayerFacet ViewportPrintColor = new(key: 0x200u);
    public static readonly LayerFacet ViewportPrintWidth = new(key: 0x400u);
    public static readonly LayerFacet RenderMaterial = new(key: 0x800u);
    public static readonly LayerFacet SectionStyle = new(key: 0x1000u);
    public static readonly LayerFacet NewDetailOn = new(key: 0x2000u);
    public static readonly LayerFacet Expanded = new(key: 0x4000u);
}

public sealed record LayerScope {
    private LayerScope(LanguageExt.HashSet<LayerFacet> facets) => Facets = facets;
    public LanguageExt.HashSet<LayerFacet> Facets { get; }

    public static LayerScope None { get; } = new(facets: LanguageExt.HashSet<LayerFacet>.Empty);
    public static LayerScope All { get; } = new(facets: toHashSet(toSeq(LayerFacet.Items)));

    public static Fin<LayerScope> Create(params ReadOnlySpan<LayerFacet> facets) {
        Op op = Op.Of();
        return toSeq(facets.ToArray()).TraverseM(facet => Optional(facet).ToFin(Fail: op.InvalidInput())).As()
            .Map(values => new LayerScope(facets: toHashSet(values)));
    }

    public Fin<LayerScope> With(LayerFacet facet) {
        Op op = Op.Of();
        return Optional(facet).ToFin(Fail: op.InvalidInput())
            .Map(value => new LayerScope(facets: Facets.Add(key: value)));
    }

    internal RestoreLayerProperties Mask => Facets.IsEmpty
        ? RestoreLayerProperties.None
        : Facets.Count == LayerFacet.Items.Count
            ? RestoreLayerProperties.All
            : (RestoreLayerProperties)Facets.AsIterable().Fold(0u, static (state, facet) => state | facet.Key);
}
```

## [04]-[PROGRAM_RAIL]

- Owners: `CPlaneEdit`, `PositionEdit`, and `LayerStateEdit` close only their table's real verbs. `PresetProgram` lowers each admitted table-specific edit to one fact arrow at construction, so one carrier owns undo label, redraw policy, and commit for all three tables and a fourth preset table lands as one factory over its own edit union.
- Cplanes: save-or-replace and delete-by-index-or-name compose the complete table mutation roster.
- Positions: `PositionRef` resolves name or id once; save and append resolve `TableTarget` inside the session, while restore, update, rename, and delete operate on the resolved id.
- Layer states: save updates existing names, restore applies one admitted `LayerScope`, viewport ids reject `Guid.Empty`, and import resolves `DocumentPath` as a `.3dm` file.
- Rail: edit traversal aborts on the first failure. `PresetFact` cases preserve only valid table/effect combinations, and `PresetReceipt` is their ordered stream.

```csharp signature
// --- [EDIT_VOCABULARIES] --------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CPlaneRef {
    private CPlaneRef() { }
    public sealed record Index(int Value) : CPlaneRef;
    public sealed record Name(string Value) : CPlaneRef;

    internal Fin<(int Index, string Name)> Resolve(NamedConstructionPlaneTable table, Op op) =>
        Switch(
            index: reference => reference.Value >= 0 && reference.Value < table.Count
                ? Fin.Succ(value: (reference.Value, table[reference.Value].Name ?? string.Empty))
                : Fin.Fail<(int, string)>(error: op.MissingContext()),
            name: reference => op.AcceptText(value: reference.Value).Bind(name =>
                table.Find(name) is var index && index >= 0
                    ? Fin.Succ(value: (index, name))
                    : Fin.Fail<(int, string)>(error: op.MissingContext())));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PositionRef {
    private PositionRef() { }
    public sealed record Id(Guid Value) : PositionRef;
    public sealed record Name(string Value) : PositionRef;

    internal Fin<Guid> Resolve(NamedPositionTable table, Op op) =>
        Switch(
            id: reference => reference.Value != Guid.Empty
                ? Fin.Succ(value: reference.Value)
                : Fin.Fail<Guid>(error: op.InvalidInput()),
            name: reference => op.AcceptText(value: reference.Value).Bind(name =>
                table.Id(name) is var id && id != Guid.Empty
                    ? Fin.Succ(value: id)
                    : Fin.Fail<Guid>(error: op.MissingContext())));
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CPlaneEdit {
    private CPlaneEdit() { }
    public sealed record Save(CPlaneModel Model) : CPlaneEdit;
    public sealed record Delete(CPlaneRef Target) : CPlaneEdit;

    internal Fin<PresetFact> Apply(RhinoDoc document, Op op) =>
        Switch(
            (Table: document.NamedConstructionPlanes, Op: op),
            save: static (context, edit) =>
                from model in Optional(edit.Model).ToFin(Fail: context.Op.InvalidInput())
                from index in context.Op.Catch(() => {
                    int added = context.Table.Add(constructionPlane: model.Mint());
                    return added >= 0 ? Fin.Succ(value: added) : Fin.Fail<int>(error: context.Op.InvalidResult());
                })
                select (PresetFact)new PresetFact.CPlaneSaved(
                    Index: index,
                    Name: context.Table[index].Name ?? string.Empty),
            delete: static (context, edit) =>
                from target in Optional(edit.Target).ToFin(Fail: context.Op.InvalidInput())
                from resolved in target.Resolve(table: context.Table, op: context.Op)
                from _ in context.Op.Confirm(success: context.Table.Delete(index: resolved.Index))
                select (PresetFact)new PresetFact.CPlaneDeleted(Name: resolved.Name));
}

[SmartEnum<int>]
public sealed partial class PositionEffect {
    public static readonly PositionEffect Saved = new(key: 0);
    public static readonly PositionEffect Restored = new(key: 1);
    public static readonly PositionEffect Updated = new(key: 2);
    public static readonly PositionEffect Appended = new(key: 3);
    public static readonly PositionEffect Renamed = new(key: 4);
    public static readonly PositionEffect Deleted = new(key: 5);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PositionEdit {
    private PositionEdit() { }
    public sealed record Save(string Name, TableTarget Members) : PositionEdit;
    public sealed record Restore(PositionRef Target) : PositionEdit;
    public sealed record Update(PositionRef Target) : PositionEdit;
    public sealed record Append(PositionRef Target, TableTarget Members) : PositionEdit;
    public sealed record Rename(PositionRef Target, string Name) : PositionEdit;
    public sealed record Delete(PositionRef Target) : PositionEdit;

    internal Fin<PresetFact> Apply(RhinoDoc document, Op op) =>
        Switch(
            (Document: document, Table: document.NamedPositions, Op: op),
            save: static (context, edit) =>
                from name in context.Op.AcceptText(value: edit.Name)
                from members in Optional(edit.Members).ToFin(Fail: context.Op.InvalidInput())
                from ids in members.Resolve(document: context.Document, key: context.Op)
                from _ in guard(!ids.IsEmpty, context.Op.InvalidInput()).ToFin()
                from id in context.Op.Catch(() => {
                    Guid saved = context.Table.Save(name: name, objectIds: ids.AsIterable());
                    return saved != Guid.Empty ? Fin.Succ(value: saved) : Fin.Fail<Guid>(error: context.Op.InvalidResult());
                })
                select (PresetFact)new PresetFact.Position(Id: id, Effect: PositionEffect.Saved),
            restore: static (context, edit) => ApplyReference(context, edit.Target, PositionEffect.Restored, static (table, id) => table.Restore(id)),
            update: static (context, edit) => ApplyReference(context, edit.Target, PositionEffect.Updated, static (table, id) => table.Update(id)),
            append: static (context, edit) =>
                from target in Optional(edit.Target).ToFin(Fail: context.Op.InvalidInput())
                from id in target.Resolve(table: context.Table, op: context.Op)
                from members in Optional(edit.Members).ToFin(Fail: context.Op.InvalidInput())
                from ids in members.Resolve(document: context.Document, key: context.Op)
                from _ in guard(!ids.IsEmpty, context.Op.InvalidInput()).ToFin()
                from __ in context.Op.Confirm(success: context.Table.Append(id: id, objectIds: ids.AsIterable()))
                select (PresetFact)new PresetFact.Position(Id: id, Effect: PositionEffect.Appended),
            rename: static (context, edit) =>
                from target in Optional(edit.Target).ToFin(Fail: context.Op.InvalidInput())
                from id in target.Resolve(table: context.Table, op: context.Op)
                from name in context.Op.AcceptText(value: edit.Name)
                from _ in context.Op.Confirm(success: context.Table.Rename(id: id, name: name))
                select (PresetFact)new PresetFact.Position(Id: id, Effect: PositionEffect.Renamed),
            delete: static (context, edit) => ApplyReference(context, edit.Target, PositionEffect.Deleted, static (table, id) => table.Delete(id)));

    private static Fin<PresetFact> ApplyReference(
        (RhinoDoc Document, NamedPositionTable Table, Op Op) context,
        PositionRef target,
        PositionEffect effect,
        Func<NamedPositionTable, Guid, bool> apply) =>
        from reference in Optional(target).ToFin(Fail: context.Op.InvalidInput())
        from id in reference.Resolve(table: context.Table, op: context.Op)
        from _ in context.Op.Catch(() => context.Op.Confirm(success: apply(context.Table, id)))
        select (PresetFact)new PresetFact.Position(Id: id, Effect: effect);
}

[SmartEnum<int>]
public sealed partial class LayerStateEffect {
    public static readonly LayerStateEffect Saved = new(key: 0);
    public static readonly LayerStateEffect Restored = new(key: 1);
    public static readonly LayerStateEffect Renamed = new(key: 2);
    public static readonly LayerStateEffect Deleted = new(key: 3);
}

[Union(SwitchMapStateParameterName = "context", ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerStateEdit {
    private LayerStateEdit() { }
    public sealed record Save(string Name, Option<Guid> Viewport = default) : LayerStateEdit;
    public sealed record Restore(string Name, LayerScope Scope, Option<Guid> Viewport = default) : LayerStateEdit;
    public sealed record Rename(string OldName, string Name) : LayerStateEdit;
    public sealed record Delete(string Name) : LayerStateEdit;
    public sealed record Import(DocumentPath Path) : LayerStateEdit;

    internal Fin<PresetFact> Apply(RhinoDoc document, Op op) =>
        Switch(
            (Table: document.NamedLayerStates, Op: op),
            save: static (context, edit) =>
                from name in context.Op.AcceptText(value: edit.Name)
                from viewport in AdmitViewport(edit.Viewport, context.Op)
                from index in context.Op.Catch(() => {
                    int saved = viewport.Match(
                        Some: id => context.Table.Save(name: name, viewportId: id),
                        None: () => context.Table.Save(name: name));
                    return saved >= 0 ? Fin.Succ(value: saved) : Fin.Fail<int>(error: context.Op.InvalidResult());
                })
                select (PresetFact)new PresetFact.LayerState(Name: name, Effect: LayerStateEffect.Saved),
            restore: static (context, edit) =>
                from name in context.Op.AcceptText(value: edit.Name)
                from scope in Optional(edit.Scope).ToFin(Fail: context.Op.InvalidInput())
                from viewport in AdmitViewport(edit.Viewport, context.Op)
                from _ in context.Op.Confirm(success: viewport.Match(
                    Some: id => context.Table.Restore(name: name, properties: scope.Mask, viewportId: id),
                    None: () => context.Table.Restore(name: name, properties: scope.Mask)))
                select (PresetFact)new PresetFact.LayerState(Name: name, Effect: LayerStateEffect.Restored),
            rename: static (context, edit) =>
                from oldName in context.Op.AcceptText(value: edit.OldName)
                from name in context.Op.AcceptText(value: edit.Name)
                from _ in context.Op.Confirm(success: context.Table.Rename(oldName: oldName, newName: name))
                select (PresetFact)new PresetFact.LayerState(Name: name, Effect: LayerStateEffect.Renamed),
            delete: static (context, edit) =>
                from name in context.Op.AcceptText(value: edit.Name)
                from _ in context.Op.Confirm(success: context.Table.Delete(name: name))
                select (PresetFact)new PresetFact.LayerState(Name: name, Effect: LayerStateEffect.Deleted),
            import: static (context, edit) =>
                from path in Optional(edit.Path).ToFin(Fail: context.Op.InvalidInput())
                from file in path.Resolve(file: DocumentFile.ThreeDm, key: context.Op)
                from count in context.Op.Catch(() => {
                    int imported = context.Table.Import(filename: file);
                    return imported >= 0 ? Fin.Succ(value: imported) : Fin.Fail<int>(error: context.Op.InvalidResult());
                })
                select (PresetFact)new PresetFact.LayerStatesImported(Count: count));

    private static Fin<Option<Guid>> AdmitViewport(Option<Guid> viewport, Op op) =>
        viewport.Traverse(id => id != Guid.Empty ? Fin.Succ(value: id) : Fin.Fail<Guid>(error: op.InvalidInput())).As();
}

// --- [PROGRAM_AND_RECEIPT] ------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PresetFact : IDetachedDocumentResult {
    private PresetFact() { }
    public sealed record CPlaneSaved(int Index, string Name) : PresetFact;
    public sealed record CPlaneDeleted(string Name) : PresetFact;
    public sealed record Position(Guid Id, PositionEffect Effect) : PresetFact;
    public sealed record LayerState(string Name, LayerStateEffect Effect) : PresetFact;
    public sealed record LayerStatesImported(int Count) : PresetFact;
}

public sealed record PresetReceipt(Seq<PresetFact> Facts) : IDetachedDocumentResult;

public sealed record PresetProgram {
    private PresetProgram(string name, RedrawPolicy redraw, Seq<Func<RhinoDoc, Op, Fin<PresetFact>>> edits) =>
        (Name, Redraw, Edits) = (name, redraw, edits);

    public string Name { get; }
    public RedrawPolicy Redraw { get; }
    internal Seq<Func<RhinoDoc, Op, Fin<PresetFact>>> Edits { get; }

    public static Fin<PresetProgram> CPlanes(string name, RedrawPolicy redraw, params ReadOnlySpan<CPlaneEdit> edits) =>
        Create(name: name, redraw: redraw, edits: edits, apply: static edit => edit.Apply);

    public static Fin<PresetProgram> Positions(string name, RedrawPolicy redraw, params ReadOnlySpan<PositionEdit> edits) =>
        Create(name: name, redraw: redraw, edits: edits, apply: static edit => edit.Apply);

    public static Fin<PresetProgram> LayerStates(string name, RedrawPolicy redraw, params ReadOnlySpan<LayerStateEdit> edits) =>
        Create(name: name, redraw: redraw, edits: edits, apply: static edit => edit.Apply);

    internal Fin<PresetReceipt> Commit(DocumentSession session, Op op) {
        Seq<SessionNeed> needs = Seq(SessionNeed.Mutate, SessionNeed.Undo)
            + (Redraw.Enabled ? Seq(SessionNeed.Redraw) : Seq<SessionNeed>());
        return session.Demand(
            use: document => op.Catch(() => {
                using UndoBracket undo = UndoBracket.Begin(document: document, name: Name, recordsUndo: true);
                Fin<PresetReceipt> outcome = guard(undo.Admitted, op.InvalidResult()).ToFin()
                    .Bind(_ => Edits.TraverseM(apply => apply(document, op)).As())
                    .Map(static facts => new PresetReceipt(Facts: facts));
                return undo.Seal(outcome: outcome, stamp: static (receipt, _) => receipt, key: op)
                    .Bind(receipt => Redraw.Enabled
                        ? op.Catch(() => document.Views.Redraw(deferred: Redraw.Defers)).Map(_ => receipt)
                        : Fin.Succ(value: receipt));
            }),
            key: op,
            needs: needs.ToArray());
    }

    private static Fin<PresetProgram> Create<TEdit>(
        string name,
        RedrawPolicy redraw,
        ReadOnlySpan<TEdit> edits,
        Func<TEdit, Func<RhinoDoc, Op, Fin<PresetFact>>> apply)
        where TEdit : class {
        Op op = Op.Of();
        return from label in op.AcceptText(value: name)
               from policy in Optional(redraw).ToFin(Fail: op.InvalidInput())
               from program in toSeq(edits.ToArray()).TraverseM(edit => Optional(edit).ToFin(Fail: op.InvalidInput())).As()
               from _ in guard(!program.IsEmpty, op.InvalidInput()).ToFin()
               select new PresetProgram(name: label, redraw: policy, edits: program.Map(apply));
    }
}

public static partial class Presets {
    public static Fin<PresetReceipt> Commit(DocumentSession session, PresetProgram program) {
        Op op = Op.Of();
        return from context in Optional(session).ToFin(Fail: op.InvalidInput())
               from active in Optional(program).ToFin(Fail: op.InvalidInput())
               from receipt in active.Commit(session: context, op: op)
               select receipt;
    }
}
```

## [05]-[READ_RAIL]

- Cplane snapshot: the table enumerates as `IEnumerable<ConstructionPlane>`, and every preset detaches through `CPlaneModel`.
- Position snapshot: every preset id retains its resolved name and a transform map for every stored object id; `ObjectXform` failure aborts the snapshot.
- Layer-state snapshot: names are the complete readable host surface because stored property payloads expose no read API.

```csharp signature
// --- [READ_MODELS] --------------------------------------------------------------------------
public sealed record PositionPreset(
    Guid Id,
    string Name,
    HashMap<Guid, Transform> Transforms) : IDetachedDocumentResult;

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PresetRead {
    private PresetRead() { }
    public sealed record CPlanes : PresetRead;
    public sealed record Positions : PresetRead;
    public sealed record LayerStates : PresetRead;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PresetAnswer : IDetachedDocumentResult {
    private PresetAnswer() { }
    public sealed record CPlanes(Seq<CPlaneModel> Values) : PresetAnswer;
    public sealed record Positions(Seq<PositionPreset> Values) : PresetAnswer;
    public sealed record LayerStates(Seq<string> Names) : PresetAnswer;
}

public static partial class Presets {
    public static Fin<PresetAnswer> Read(DocumentSession session, PresetRead request) {
        Op op = Op.Of();
        return from context in Optional(session).ToFin(Fail: op.InvalidInput())
               from active in Optional(request).ToFin(Fail: op.InvalidInput())
               from answer in context.Demand(
                   use: document => op.Catch(() => active.Switch(
                       (Document: document, Op: op),
                       cPlanes: static (state, _) => toSeq(state.Document.NamedConstructionPlanes)
                           .TraverseM(plane => CPlaneModel.Of(source: plane, key: state.Op))
                           .As()
                           .Map(values => (PresetAnswer)new PresetAnswer.CPlanes(Values: values)),
                       positions: static (state, _) => toSeq(state.Document.NamedPositions.Ids)
                           .TraverseM(id => SnapshotPosition(state.Document.NamedPositions, id, state.Op))
                           .As()
                           .Map(values => (PresetAnswer)new PresetAnswer.Positions(Values: values)),
                       layerStates: static (state, _) => state.Op.Catch(() => Fin.Succ<PresetAnswer>(
                           value: new PresetAnswer.LayerStates(Names: toSeq(state.Document.NamedLayerStates.Names)))))),
                   key: op,
                   needs: [SessionNeed.Read])
               select answer;
    }

    private static Fin<PositionPreset> SnapshotPosition(NamedPositionTable table, Guid id, Op op) =>
        op.Catch(() => SnapshotPositionCore(table: table, id: id, op: op));

    private static Fin<PositionPreset> SnapshotPositionCore(NamedPositionTable table, Guid id, Op op) =>
        from _ in id != Guid.Empty ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: op.InvalidResult())
        from transforms in toSeq(table.ObjectIds(id)).TraverseM(objectId => op.Catch(() => {
                Transform value = Transform.Identity;
                return table.ObjectXform(id: id, objId: objectId, xform: ref value)
                    ? Fin.Succ(value: (objectId, value))
                    : Fin.Fail<(Guid, Transform)>(error: op.MissingContext());
            }))
            .As()
        select new PositionPreset(
            Id: id,
            Name: table.Name(id) ?? string.Empty,
            Transforms: transforms.Fold(
                HashMap<Guid, Transform>(),
                static (state, pair) => state.AddOrUpdate(pair.objectId, pair.value)));
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]       | [OWNER]                                          | [FORM]                       | [ENTRY]                         |
| :-----: | :-------------- | :----------------------------------------------- | :--------------------------- | :------------------------------ |
|  [01]   | grid vocabulary | `CPlaneGrid`                                     | shared host grid product     | `Of` / `Apply` / `MintDefaults` |
|  [02]   | cplane value    | `CPlaneModel`                                    | admitted full cplane         | `Create` / `Of` / `Mint`        |
|  [03]   | layer mask      | `LayerScope`                                     | frozen verified facet set    | `Create` / `Mask`               |
|  [04]   | table edits     | `CPlaneEdit` / `PositionEdit` / `LayerStateEdit` | closed table-specific unions | `Apply`                         |
|  [05]   | commit rail     | `PresetProgram`                                  | undo/redraw program          | `Presets.Commit`                |
|  [06]   | receipt         | `PresetReceipt`                                  | ordered closed preset facts  | `Facts`                         |
|  [07]   | read rail       | `PresetRead`                                     | complete table snapshots     | `Presets.Read`                  |
|  [08]   | named views     | viewport and table owners                        | existing vocabulary          | seam only                       |
