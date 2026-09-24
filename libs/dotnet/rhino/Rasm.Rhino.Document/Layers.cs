using System.Drawing;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Document;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerRef {
    public sealed record Row(ComponentRef Address) : LayerRef;

    public sealed record Current() : LayerRef;
}

public sealed record LayerAttributes(Color Color, Color PlotColor, PlotWeight Plot, int LinetypeIndex, int RenderMaterialIndex, int SectionStyleIndex);

public sealed record PerViewportSettings(Guid Viewport, Color Color, Color PlotColor, PlotWeight Plot, bool Visible, bool PersistentVisibility);

public sealed record LayerNode(
    Guid Id,
    int Index,
    string FullPath,
    string Name,
    Option<Guid> Parent,
    LayerAttributes Attributes,
    bool Visible,
    bool Locked,
    bool PersistentVisibility,
    bool PersistentLocking,
    bool Expanded,
    bool Current,
    bool Reference,
    int SortIndex,
    Seq<PerViewportSettings> PerViewport,
    Seq<LayerNode> Children);

public sealed record LayerTree(Seq<LayerNode> Roots, Guid Current);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerEdit {
    public sealed record Rename(string Name) : LayerEdit;

    public sealed record Color(System.Drawing.Color Value) : LayerEdit;

    public sealed record PlotColor(System.Drawing.Color Value) : LayerEdit;

    public sealed record Plot(PlotWeight Weight) : LayerEdit;

    public sealed record LinetypeIndex(int Index) : LayerEdit;

    public sealed record RenderMaterialIndex(int Index) : LayerEdit;

    public sealed record SectionStyleIndex(int Index) : LayerEdit;

    public sealed record IgesLevel(int Level) : LayerEdit;

    public sealed record CustomSectionStyle(Option<SectionStyle> Style) : LayerEdit;

    public sealed record Visible(bool On) : LayerEdit;

    public sealed record Locked(bool On) : LayerEdit;

    public sealed record Expanded(bool On) : LayerEdit;

    public sealed record PersistentVisibility(Option<bool> Value) : LayerEdit;

    public sealed record PersistentLocking(Option<bool> Value) : LayerEdit;

    public sealed record Description(Option<string> Text) : LayerEdit;

    public sealed record PerViewport(LayerOverride Detail) : LayerEdit;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerOverride {
    public sealed record Color(Guid Viewport, Option<System.Drawing.Color> Value) : LayerOverride;

    public sealed record PlotColor(Guid Viewport, Option<System.Drawing.Color> Value) : LayerOverride;

    public sealed record Visible(Guid Viewport, Option<bool> Value) : LayerOverride;

    public sealed record PersistentVisibility(Guid Viewport, Option<bool> Value) : LayerOverride;

    public sealed record Plot(Guid Viewport, Option<PlotWeight> Value) : LayerOverride;

    public sealed record NewDetailVisibility(bool On) : LayerOverride;

    public sealed record Delete(Guid Viewport) : LayerOverride;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerOp {
    public sealed record Create(string Name, Option<LayerRef> Parent, Seq<LayerEdit> Edits) : LayerOp;

    public sealed record AddPath(string Path, Option<Color> Color) : LayerOp;

    public sealed record Modify(LayerRef Target, Seq<LayerEdit> Edits) : LayerOp;

    public sealed record Reparent(LayerRef Target, Option<LayerRef> Parent) : LayerOp;

    public sealed record Merge(LayerRef Source, LayerRef Target) : LayerOp;

    public sealed record Duplicate(Seq<LayerRef> Targets, bool Objects, bool Sublayers) : LayerOp;

    public sealed record Delete(LayerRef Target, bool Quiet) : LayerOp;

    public sealed record Purge(LayerRef Target, bool Quiet) : LayerOp;

    public sealed record Undelete(LayerRef Target) : LayerOp;

    public sealed record SetCurrent(LayerRef Target, bool Quiet) : LayerOp;

    public sealed record ForceVisible(LayerRef Target) : LayerOp;

    public sealed record SortByName(bool Ascending) : LayerOp;

    public sealed record Sort(Seq<LayerRef> Order) : LayerOp;

    public sealed record UndoModify(LayerRef Target, Option<uint> Serial) : LayerOp;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Layers {
    // --- [RESOLUTION]
    public static IO<Layer> ResolveLayer(RhinoDoc doc, LayerRef address, bool includeDeleted) =>
        from index in address.Switch(
            (Doc: doc, IncludeDeleted: includeDeleted),
            row: static (state, row) => TableOps.Find<int>(
                    row.Address,
                    id => Stored(state.Doc, state.Doc.Layers.Find(id, ignoreDeletedLayers: !state.IncludeDeleted, RhinoMath.UnsetIntIndex)),
                    static index => Some(index),
                    name => Stored(state.Doc, state.Doc.Layers.FindByFullPath(name, RhinoMath.UnsetIntIndex)))
                .Bind(static found => IO.lift(found.ToFin(new Missing(nameof(LayerTable.Find))))),
            current: static (state, _) => IO.lift(() => state.Doc.Layers.CurrentLayerIndex))
        from row in IO.lift(() => Missing.Unless(doc.Layers.FindIndex(index), nameof(LayerTable.FindIndex)))
        from live in IO.lift(() => Missing.Unless(includeDeleted || !row.IsDeleted, nameof(Layer.IsDeleted)))
        select row;

    private static Option<int> Stored(RhinoDoc doc, int index) =>
        Answers.Present(index).Filter(found => found < doc.Layers.Count);

    // --- [READS]
    public static IO<LayerTree> ReadLayers(RhinoDoc doc, Seq<Guid> detailViewports) =>
        from rows in IO.lift(() => toSeq(doc.Layers).Filter(static layer => !layer.IsDeleted).Map(layer => Node(layer, detailViewports)).Strict())
        from current in IO.lift(() => doc.Layers.CurrentLayer.Id)
        from roots in IO.lift(() => Tree(rows))
        select new LayerTree(roots, current);

    private static LayerNode Node(Layer layer, Seq<Guid> detailViewports) =>
        new(
            layer.Id,
            layer.Index,
            layer.FullPath,
            layer.Name,
            Answers.Present(layer.ParentLayerId),
            LayerMapper.ToAttributes(layer),
            layer.IsVisible,
            layer.IsLocked,
            layer.GetPersistentVisibility(),
            layer.GetPersistentLocking(),
            layer.IsExpanded,
            layer.IsCurrent,
            layer.IsReference,
            layer.SortIndex,
            detailViewports.Filter(layer.HasPerViewportSettings).Map(viewport => PerViewport(layer, viewport)).Strict(),
            Seq<LayerNode>());

    private static PerViewportSettings PerViewport(Layer layer, Guid viewport) =>
        new(
            viewport,
            layer.PerViewportColor(viewport),
            layer.PerViewportPlotColor(viewport),
            PlotWeight.FromHost(layer.PerViewportPlotWeight(viewport)),
            layer.PerViewportIsVisible(viewport),
            layer.PerViewportPersistentVisibility(viewport));

    private static Fin<Seq<LayerNode>> Tree(Seq<LayerNode> rows) =>
        from byId in Pure(toHashMap(rows.Map(static row => (row.Id, row)))).ToFin()
        from climbed in rows.TraverseM(row => Climb(byId, row.Id, row, HashSet(row.Id))).As()
        select Children(rows, Option<Guid>.None);

    private static Fin<Unit> Climb(HashMap<Guid, LayerNode> byId, Guid start, LayerNode node, LanguageExt.HashSet<Guid> seen) =>
        node.Parent.Match(
            Some: parent => seen.Contains(parent)
                ? new LayerCycle(start)
                : byId.Find(parent).Match(
                    Some: above => Climb(byId, start, above, seen.Add(parent)),
                    None: () => new LayerOrphan(node.Id, parent)),
            None: static () => unit);

    private static Seq<LayerNode> Children(Seq<LayerNode> rows, Option<Guid> parent) =>
        toSeq(rows.Filter(row => row.Parent == parent).OrderBy(static row => row.SortIndex).ThenBy(static row => row.Name, StringComparer.OrdinalIgnoreCase))
            .Map(row => row with { Children = Children(rows, Some(row.Id)) })
            .Strict();

    // --- [NAMES]
    public static bool IsLeafName(string name) =>
        ModelComponent.IsValidComponentName(name) && !name.Contains(ModelComponent.NamePathSeparator, StringComparison.Ordinal);

    // --- [WRITES]
    public static IO<Unit> ModifyLayer(RhinoDoc doc, int index, Seq<LayerEdit> edits) =>
        Staged(doc, index, staged => edits.TraverseM(edit => Edit(doc, staged, edit)).As().Map(static _ => unit));

    public static IO<Seq<int>> ApplyLayerOp(RhinoDoc doc, LayerOp op) =>
        op.Switch(
            doc,
            create: static (document, create) =>
                from leaf in IO.lift(() => Invalid.Unless(IsLeafName(create.Name), nameof(ModelComponent.IsValidComponentName)))
                from parent in create.Parent.Traverse(address => ResolveLayer(document, address, includeDeleted: false)).As()
                from index in TableOps.AddRow(
                    Accessors(document.Layers),
                    IO.lift(() => new Layer { Name = create.Name, ParentLayerId = ParentId(parent) }),
                    staged => create.Edits.TraverseM(edit => Edit(document, staged, edit)).As().Map(static _ => unit))
                select Seq(index),
            addPath: static (document, add) => IO.lift(() =>
                Answers.NonNegative(add.Color.Match(Some: color => document.Layers.AddPath(add.Path, color), None: () => document.Layers.AddPath(add.Path)), nameof(LayerTable.AddPath))
                    .Map(static index => Seq(index))),
            modify: static (document, modify) =>
                from row in ResolveLayer(document, modify.Target, includeDeleted: false)
                from modified in ModifyLayer(document, row.Index, modify.Edits)
                select Seq(row.Index),
            reparent: static (document, reparent) =>
                from target in ResolveLayer(document, reparent.Target, includeDeleted: false)
                from parent in reparent.Parent.Traverse(address => ResolveLayer(document, address, includeDeleted: false)).As()
                from acyclic in IO.lift<Unit>(() => parent.Exists(row => (row.Id == target.Id) || row.IsChildOf(target.Id)) ? new LayerCycle(target.Id) : unit)
                from moved in Staged(document, target.Index, staged => IO.lift(() => { staged.ParentLayerId = ParentId(parent); }))
                select Seq(target.Index),
            merge: static (document, merge) =>
                from source in ResolveLayer(document, merge.Source, includeDeleted: false)
                from target in ResolveLayer(document, merge.Target, includeDeleted: false)
                from moved in TableOps.Apply(document, new TableOp.ModifyAttributes(
                    new ObjectTarget.Query(ObjectQuery.Default with { HiddenObjects = true, IncludeLights = true, LayerIndexFilter = Some(source.Index) }, Seq<ObjectPredicate>()),
                    attributes => IO.lift(() => { attributes.LayerIndex = target.Index; }),
                    Quiet: true))
                from deleted in IO.lift(() => Refused.Unless(document.Layers.Delete(source.Index, quiet: true), nameof(LayerTable.Delete)))
                select Seq(source.Index, target.Index),
            duplicate: static (document, duplicate) =>
                from rows in duplicate.Targets.TraverseM(address => ResolveLayer(document, address, includeDeleted: false)).As()
                from indices in IO.lift(() => Answers.NonEmpty(
                    toSeq(document.Layers.Duplicate(rows.Map(static row => row.Index), duplicate.Objects, duplicate.Sublayers)),
                    nameof(LayerTable.Duplicate)))
                select indices,
            delete: static (document, delete) =>
                Call(document, delete.Target, includeDeleted: false, row => document.Layers.Delete(row.Index, delete.Quiet), nameof(LayerTable.Delete)),
            purge: static (document, purge) =>
                Call(document, purge.Target, includeDeleted: true, row => document.Layers.Purge(row.Index, purge.Quiet), nameof(LayerTable.Purge)),
            undelete: static (document, undelete) =>
                Call(document, undelete.Target, includeDeleted: true, row => document.Layers.Undelete(row.Index), nameof(LayerTable.Undelete)),
            setCurrent: static (document, current) =>
                Call(document, current.Target, includeDeleted: false, row => document.Layers.SetCurrentLayerIndex(row.Index, current.Quiet), nameof(LayerTable.SetCurrentLayerIndex)),
            forceVisible: static (document, force) =>
                Call(document, force.Target, includeDeleted: false, row => document.Layers.ForceLayerVisible(row.Id), nameof(LayerTable.ForceLayerVisible)),
            sortByName: static (document, sort) => IO.lift(() => document.Layers.SortByLayerName(sort.Ascending)).Map(static _ => Seq<int>()),
            sort: static (document, sort) =>
                from rows in sort.Order.TraverseM(address => ResolveLayer(document, address, includeDeleted: false)).As()
                from indices in IO.lift(() => Permutation(rows.Map(static row => row.Index).Strict(), document.Layers.ActiveCount))
                from sorted in IO.lift(() => document.Layers.Sort(indices))
                select indices,
            undoModify: static (document, undo) =>
                Call(
                    document,
                    undo.Target,
                    includeDeleted: false,
                    row => undo.Serial.Match(Some: serial => document.Layers.UndoModify(row.Index, serial), None: () => document.Layers.UndoModify(row.Index)),
                    nameof(LayerTable.UndoModify)));

    private static TableAccessors<Layer> Accessors(LayerTable layers) =>
        new(layers, layers.Add, layers.Modify, layers.FindName, layers.FindIndex);

    private static IO<Unit> Staged(RhinoDoc doc, int index, Func<Layer, IO<Unit>> stage) =>
        TableOps.ModifyRow(
            Accessors(doc.Layers),
            index,
            static live => {
                Layer staged = new();
                staged.CopyAttributesFrom(live);
                return staged;
            },
            stage,
            quiet: true);

    private static IO<Unit> Edit(RhinoDoc doc, Layer staged, LayerEdit edit) =>
        edit.Switch(
            (Row: staged, Doc: doc),
            rename: static (state, rename) =>
                from leaf in IO.lift(() => Invalid.Unless(IsLeafName(rename.Name), nameof(ModelComponent.IsValidComponentName)))
                from named in TableOps.Locked(IO.lift(() => { state.Row.Name = rename.Name; }), nameof(Layer.Name))
                select named,
            color: static (state, color) => IO.lift(() => { state.Row.Color = color.Value; }),
            plotColor: static (state, plotColor) => IO.lift(() => { state.Row.PlotColor = plotColor.Value; }),
            plot: static (state, plot) => IO.lift(() => { state.Row.PlotWeight = plot.Weight.ToHost(); }),
            linetypeIndex: static (state, linetype) => Indexed(Assignable(linetype.Index, state.Doc.Linetypes.Count, nameof(Layer.LinetypeIndex)), index => state.Row.LinetypeIndex = index),
            renderMaterialIndex: static (state, material) => Indexed(Assignable(material.Index, state.Doc.Materials.Count, nameof(Layer.RenderMaterialIndex)), index => state.Row.RenderMaterialIndex = index),
            sectionStyleIndex: static (state, style) => Indexed(Assignable(style.Index, state.Doc.SectionStyles.Count, nameof(Layer.SectionStyleIndex)), index => state.Row.SectionStyleIndex = index),
            igesLevel: static (state, iges) => Indexed(Limits.AtLeast(0).Check(iges.Level, nameof(Layer.IgesLevel)), level => state.Row.IgesLevel = level),
            customSectionStyle: static (state, custom) => IO.lift(() => custom.Style.Match(state.Row.SetCustomSectionStyle, state.Row.RemoveCustomSectionStyle)),
            visible: static (state, visible) => IO.lift(() => { state.Row.IsVisible = visible.On; }),
            locked: static (state, locked) => IO.lift(() => { state.Row.IsLocked = locked.On; }),
            expanded: static (state, expanded) => IO.lift(() => { state.Row.IsExpanded = expanded.On; }),
            persistentVisibility: static (state, persistent) => IO.lift(() => persistent.Value.Match(state.Row.SetPersistentVisibility, state.Row.UnsetPersistentVisibility)),
            persistentLocking: static (state, persistent) => IO.lift(() => persistent.Value.Match(state.Row.SetPersistentLocking, state.Row.UnsetPersistentLocking)),
            description: static (state, description) => IO.lift(() => { state.Row.Description = description.Text.IfNone(""); }),
            perViewport: static (state, perViewport) => Override(state.Row, perViewport.Detail));

    private static IO<Unit> Override(Layer staged, LayerOverride detail) =>
        IO.lift(() => detail.Switch(
            staged,
            color: static (row, color) => color.Value.Match(value => row.SetPerViewportColor(color.Viewport, value), () => row.DeletePerViewportColor(color.Viewport)),
            plotColor: static (row, plotColor) => plotColor.Value.Match(value => row.SetPerViewportPlotColor(plotColor.Viewport, value), () => row.DeletePerViewportPlotColor(plotColor.Viewport)),
            visible: static (row, visible) => visible.Value.Match(on => row.SetPerViewportVisible(visible.Viewport, on), () => row.DeletePerViewportVisible(visible.Viewport)),
            persistentVisibility: static (row, persistent) => persistent.Value.Match(
                on => row.SetPerViewportPersistentVisibility(persistent.Viewport, on),
                () => row.UnsetPerViewportPersistentVisibility(persistent.Viewport)),
            plot: static (row, plot) => plot.Value.Match(weight => row.SetPerViewportPlotWeight(plot.Viewport, weight.ToHost()), () => row.DeletePerViewportPlotWeight(plot.Viewport)),
            newDetailVisibility: static (row, fresh) => row.PerViewportIsVisibleInNewDetails = fresh.On,
            delete: static (row, delete) => row.DeletePerViewportSettings(delete.Viewport)));

    private static IO<Unit> Indexed(Fin<int> index, Action<int> write) =>
        IO.lift(index).Bind(value => IO.lift(() => write(value)));

    private static Fin<int> Assignable(int index, int itemCount, string member) =>
        index == -1 ? index : IndexOutOfRange.Unless(index, itemCount, member).Map(_ => index);

    private static Guid ParentId(Option<Layer> parent) =>
        parent.Map(static row => row.Id).IfNone(Guid.Empty);

    private static IO<Seq<int>> Call(RhinoDoc doc, LayerRef address, bool includeDeleted, Func<Layer, bool> call, string member) =>
        ResolveLayer(doc, address, includeDeleted).Bind(row => IO.lift(() => Refused.Unless(call(row), member).Map(_ => Seq(row.Index))));

    private static Fin<Seq<int>> Permutation(Seq<int> indices, int active) =>
        (Answers.Unique(indices, static (index, _) => new DuplicateIndex(nameof(LayerTable.Sort), index))
         & CountMismatch.Unless(active, indices.Count, nameof(LayerTable.ActiveCount)).ToValidation())
        .ToFin()
        .Map(_ => indices);
}

[Mapper]
internal static partial class LayerMapper {
    [MapProperty(nameof(Layer.PlotWeight), nameof(LayerAttributes.Plot), Use = nameof(@PlotWeight.FromHost))]
    internal static partial LayerAttributes ToAttributes(Layer layer);
}
