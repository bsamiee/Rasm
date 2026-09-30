using System.Drawing;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Render;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Document;

// --- [MODELS] --------------------------------------------------------------------------
public sealed class LayerNames : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => TableOps.Names<Layer>();

    public static IComparer<string> Comparer => TableOps.Names<Layer>();
}

[ValueObject<string>]
[ValidationError<ValidationFailure>]
[KeyMemberEqualityComparer<LayerNames, string>]
[KeyMemberComparer<LayerNames, string>]
public sealed partial class LayerName {
    static partial void ValidateFactoryArguments(ref ValidationFailure? validationError, ref string value) {
        if (!ModelComponent.IsValidComponentName(value) || value.Contains(ModelComponent.NamePathSeparator, StringComparison.Ordinal))
            validationError = new Invalid(nameof(LayerName));
    }
}

public sealed record LayerPath(Option<LayerPath> Parent, LayerName Leaf) {
    public Seq<LayerPath> Ancestors => Parent.ToSeq().Bind(static parent => parent.Ancestors.Add(parent));

    public string FullPath => string.Join(ModelComponent.NamePathSeparator, Ancestors.Add(this).Map(static path => (string)path.Leaf));

    public static Fin<LayerPath> Parse(string text) =>
        toSeq(text.Split(ModelComponent.NamePathSeparator))
            .Map(static (segment, index) => (Segment: segment, Index: index))
            .Traverse(static row => Answers.Validated<LayerName, string>(row.Segment).MapFail(_ => new InvalidElement(nameof(LayerPath), row.Index)))
            .As()
            .Bind(static names => names.Fold(Option<LayerPath>.None, static (parent, name) => Some(new LayerPath(parent, name))).ToFin(new Invalid(nameof(LayerPath))));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerRef {
    public sealed record Row(ComponentRef Address) : LayerRef;

    public sealed record Current() : LayerRef;
}

public sealed record PerViewportSettings(Guid Viewport, Color Color, Color PlotColor, PlotWeight PlotWeight, bool IsVisible, bool PersistentVisibility);

public sealed record LayerNode(
    Guid Id,
    int Index,
    string Name,
    Option<Guid> ParentLayerId,
    Color Color,
    Color PlotColor,
    PlotWeight PlotWeight,
    Option<LinetypeRef> Linetype,
    Option<int> RenderMaterialIndex,
    Option<int> SectionStyleIndex,
    Option<int> IgesLevel,
    Option<string> Description,
    bool IsVisible,
    bool IsLocked,
    bool IsExpanded,
    bool IsReference,
    bool ModelIsVisible,
    bool PersistentVisibility,
    bool PersistentLocking,
    bool PerViewportIsVisibleInNewDetails,
    bool HasCustomSectionStyle,
    Seq<PerViewportSettings> PerViewport,
    Seq<LayerNode> Children) {
    public Seq<LayerNode> Subtree => this.Cons(Children.Bind(static child => child.Subtree));

    public Option<PerViewportSettings> Detail(Guid viewport) => PerViewport.Find(row => row.Viewport == viewport);
}

public sealed record LayerTree(Seq<LayerNode> Roots, Guid Current) {
    public Seq<LayerNode> Nodes => Roots.Bind(static root => root.Subtree);
}

public sealed record LayerEdit(string Name, Option<RestoreLayerProperties> Restored, Option<Guid> Viewport, Func<LayerNode, bool> Holds, Func<RhinoDoc, Layer, IO<Unit>> Write);

public sealed record LayerProperty<TValue>(
    string Name,
    Option<RestoreLayerProperties> Restored,
    Option<Guid> Viewport,
    Func<LayerNode, TValue, bool> Holds,
    Func<RhinoDoc, Layer, TValue, IO<Unit>> Write) {
    public LayerEdit Set(TValue value) =>
        new(Name, Restored, Viewport, node => Holds(node, value), (doc, staged) => Write(doc, staged, value));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerOp {
    public sealed record Create(LayerName Name, Option<LayerRef> Parent, Option<Guid> Id, Seq<LayerEdit> Edits) : LayerOp;

    public sealed record AddPath(LayerPath Path, Option<Color> Color) : LayerOp;

    public sealed record Modify(LayerRef Target, Seq<LayerEdit> Edits) : LayerOp;

    public sealed record Reparent(LayerRef Target, Option<LayerRef> Parent) : LayerOp;

    public sealed record SetRenderMaterial(LayerRef Target, Option<RenderMaterial> Material) : LayerOp;

    public sealed record Merge(LayerRef Source, LayerRef Target) : LayerOp;

    public sealed record Duplicate(Seq<LayerRef> Targets, bool Objects, bool Sublayers) : LayerOp;

    public sealed record Delete(LayerRef Target, bool Quiet) : LayerOp;

    public sealed record Purge(LayerRef Target, bool Quiet) : LayerOp;

    public sealed record Undelete(LayerRef Target) : LayerOp;

    public sealed record SetCurrent(LayerRef Target, bool Quiet) : LayerOp;

    public sealed record ForceVisible(LayerRef Target) : LayerOp;

    public sealed record SortByName(bool Ascending) : LayerOp;

    public sealed record Sort(Seq<LayerRef> Order) : LayerOp;

    public sealed record SelectInPanel(Seq<LayerRef> Targets, bool Deselect) : LayerOp;

    public sealed record UndoModify(LayerRef Target, Option<uint> Serial) : LayerOp;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class LayerMapper {
    [MapProperty(nameof(Layer.PlotWeight), nameof(LayerNode.PlotWeight), Use = nameof(@PlotWeight.FromHost))]
    [MapProperty(nameof(Layer.LinetypeIndex), nameof(LayerNode.Linetype), Use = nameof(@LinetypeRef.FromHost))]
    [MapPropertyFromSource(nameof(LayerNode.HasCustomSectionStyle), Use = nameof(HasCustomSectionStyle))]
    [MapProperty(nameof(Layer.Name), nameof(LayerNode.Name), SuppressNullMismatchDiagnostic = true)]
    internal static partial LayerNode ToNode(Layer layer, bool persistentLocking, Seq<PerViewportSettings> perViewport, Seq<LayerNode> children);

    private static bool HasCustomSectionStyle(Layer layer) => DisposalOps.Present(layer.GetCustomSectionStyle());
}

public static class LayerProperties {
    // --- [ATTRIBUTES]
    public static readonly LayerProperty<LayerName> Name =
        new(
            nameof(Layer.Name),
            Option<RestoreLayerProperties>.None,
            Option<Guid>.None,
            static (node, value) => string.Equals(node.Name, value, StringComparison.Ordinal),
            static (_, staged, value) => TableOps.Named(staged, Some<string>(value)));

    public static readonly LayerProperty<Color> Color =
        Plain<Color>(nameof(Layer.Color), Some(RestoreLayerProperties.Color), static (node, value) => Answers.Same(node.Color, value), static (staged, value) => staged.Color = value);

    public static readonly LayerProperty<Option<Color>> PlotColor =
        Unsettable<Color>(
            nameof(Layer.PlotColor),
            Some(RestoreLayerProperties.PrintColor),
            static (node, value) => value.Exists(color => Answers.Same(node.PlotColor, color)),
            static (staged, color) => staged.PlotColor = color,
            static staged => staged.DeletePlotColor());

    public static readonly LayerProperty<PlotWeight> PlotWeight =
        Plain<PlotWeight>(nameof(Layer.PlotWeight), Some(RestoreLayerProperties.PrintWidth), static (node, value) => node.PlotWeight == value, static (staged, value) => staged.PlotWeight = value.ToHost());

    public static readonly LayerProperty<LinetypeRef> Linetype =
        Indexed<LinetypeRef>(
            nameof(Layer.LinetypeIndex),
            Some(RestoreLayerProperties.Linetype),
            static (node, value) => node.Linetype == Some(value),
            static (doc, value) => value.Resolve(doc),
            static (staged, index) => staged.LinetypeIndex = index);

    public static readonly LayerProperty<Option<ComponentRef>> RenderMaterialIndex =
        Indexed<Option<ComponentRef>>(
            nameof(Layer.RenderMaterialIndex),
            Some(RestoreLayerProperties.RenderMaterial),
            static (node, value) => node.RenderMaterialIndex.Map<ComponentRef>(static index => new ComponentRef.ByIndex(index)) == value,
            static (doc, value) => TableOps.Index(doc.Materials, value),
            static (staged, index) => staged.RenderMaterialIndex = index);

    public static readonly LayerProperty<Option<ComponentRef>> SectionStyleIndex =
        Indexed<Option<ComponentRef>>(
            nameof(Layer.SectionStyleIndex),
            Some(RestoreLayerProperties.SectionStyle),
            static (node, value) => node.SectionStyleIndex.Map<ComponentRef>(static index => new ComponentRef.ByIndex(index)) == value,
            static (doc, value) => TableOps.Index(doc.SectionStyles, value),
            static (staged, index) => staged.SectionStyleIndex = index);

    public static readonly LayerProperty<Option<int>> IgesLevel =
        Indexed<Option<int>>(
            nameof(Layer.IgesLevel),
            Option<RestoreLayerProperties>.None,
            static (node, value) => node.IgesLevel == value,
            static (_, value) => IO.lift(() => value.Traverse(static level => Limits.AtLeast(0).Check(level, nameof(Layer.IgesLevel))).As().Map(Answers.Unset)),
            static (staged, level) => staged.IgesLevel = level);

    public static readonly LayerProperty<Option<SectionStyle>> CustomSectionStyle =
        Unsettable<SectionStyle>(
            nameof(Layer.SetCustomSectionStyle),
            Option<RestoreLayerProperties>.None,
            static (node, value) => value.IsNone && !node.HasCustomSectionStyle,
            static (staged, style) => staged.SetCustomSectionStyle(style),
            static staged => staged.RemoveCustomSectionStyle());

    public static readonly LayerProperty<Option<string>> Description =
        Plain<Option<string>>(nameof(Layer.Description), Option<RestoreLayerProperties>.None, static (node, value) => node.Description == value, static (staged, value) => staged.Description = Answers.Unset(value));

    // --- [VISIBILITY]
    public static readonly LayerProperty<bool> IsVisible =
        Plain<bool>(
            nameof(Layer.IsVisible),
            Some(RestoreLayerProperties.Visible),
            static (node, value) => node.PersistentVisibility == value,
            static (staged, value) => {
                staged.IsVisible = value;
                staged.SetPersistentVisibility(value);
            });

    public static readonly LayerProperty<bool> IsLocked =
        Plain<bool>(
            nameof(Layer.IsLocked),
            Some(RestoreLayerProperties.Locked),
            static (node, value) => node.PersistentLocking == value,
            static (staged, value) => {
                staged.IsLocked = value;
                staged.SetPersistentLocking(value);
            });

    public static readonly LayerProperty<bool> IsExpanded =
        Plain<bool>(nameof(Layer.IsExpanded), Some(RestoreLayerProperties.Expanded), static (node, value) => node.IsExpanded == value, static (staged, value) => staged.IsExpanded = value);

    public static readonly LayerProperty<bool> PerViewportIsVisibleInNewDetails =
        Plain<bool>(
            nameof(Layer.PerViewportIsVisibleInNewDetails),
            Some(RestoreLayerProperties.NewDetailOn),
            static (node, value) => node.PerViewportIsVisibleInNewDetails == value,
            static (staged, value) => staged.PerViewportIsVisibleInNewDetails = value);

    public static readonly LayerProperty<Option<bool>> ModelIsVisible =
        Unsettable<bool>(
            nameof(Layer.ModelIsVisible),
            Option<RestoreLayerProperties>.None,
            static (node, value) => value == Some(node.ModelIsVisible),
            static (staged, on) => staged.ModelIsVisible = on,
            static staged => staged.DeleteModelVisible());

    public static readonly LayerProperty<Option<bool>> PersistentVisibility =
        Unsettable<bool>(
            nameof(Layer.PersistentVisibility),
            Option<RestoreLayerProperties>.None,
            static (_, _) => false,
            static (staged, on) => staged.SetPersistentVisibility(on),
            static staged => staged.UnsetPersistentVisibility());

    public static readonly LayerProperty<Option<bool>> ModelPersistentVisibility =
        Unsettable<bool>(
            nameof(Layer.ModelPersistentVisibility),
            Option<RestoreLayerProperties>.None,
            static (_, _) => false,
            static (staged, on) => staged.ModelPersistentVisibility = on,
            static staged => staged.UnsetModelPersistentVisibility());

    public static readonly LayerProperty<Option<bool>> PersistentLocking =
        Unsettable<bool>(
            nameof(Layer.SetPersistentLocking),
            Option<RestoreLayerProperties>.None,
            static (_, _) => false,
            static (staged, on) => staged.SetPersistentLocking(on),
            static staged => staged.UnsetPersistentLocking());

    // --- [DETAILS]
    public static Fin<LayerProperty<Option<Color>>> PerViewportColor(Guid viewport) =>
        Detail<Color>(
            nameof(Layer.PerViewportColor),
            Some(RestoreLayerProperties.ViewportColor),
            viewport,
            static (row, value) => Answers.Same(row.Color, value),
            static (staged, id, color) => staged.SetPerViewportColor(id, color),
            static (staged, id) => staged.DeletePerViewportColor(id));

    public static Fin<LayerProperty<Option<Color>>> PerViewportPlotColor(Guid viewport) =>
        Detail<Color>(
            nameof(Layer.PerViewportPlotColor),
            Some(RestoreLayerProperties.ViewportPrintColor),
            viewport,
            static (row, value) => Answers.Same(row.PlotColor, value),
            static (staged, id, color) => staged.SetPerViewportPlotColor(id, color),
            static (staged, id) => staged.DeletePerViewportPlotColor(id));

    public static Fin<LayerProperty<Option<PlotWeight>>> PerViewportPlotWeight(Guid viewport) =>
        Detail<PlotWeight>(
            nameof(Layer.PerViewportPlotWeight),
            Some(RestoreLayerProperties.ViewportPrintWidth),
            viewport,
            static (row, value) => row.PlotWeight == value,
            static (staged, id, weight) => staged.SetPerViewportPlotWeight(id, weight.ToHost()),
            static (staged, id) => staged.DeletePerViewportPlotWeight(id));

    public static Fin<LayerProperty<Option<bool>>> PerViewportIsVisible(Guid viewport) =>
        Detail<bool>(
            nameof(Layer.PerViewportIsVisible),
            Some(RestoreLayerProperties.ViewportVisible),
            viewport,
            static (row, value) => row.IsVisible == value,
            static (staged, id, on) => staged.SetPerViewportVisible(id, on),
            static (staged, id) => staged.DeletePerViewportVisible(id));

    public static Fin<LayerProperty<Option<bool>>> PerViewportPersistentVisibility(Guid viewport) =>
        Detail<bool>(
            nameof(Layer.PerViewportPersistentVisibility),
            Option<RestoreLayerProperties>.None,
            viewport,
            static (row, value) => row.PersistentVisibility == value,
            static (staged, id, on) => staged.SetPerViewportPersistentVisibility(id, on),
            static (staged, id) => staged.UnsetPerViewportPersistentVisibility(id));

    public static Fin<LayerProperty<Unit>> DeletePerViewportSettings(Guid viewport) =>
        from id in Addressed(viewport)
        select new LayerProperty<Unit>(
            nameof(Layer.DeletePerViewportSettings),
            Option<RestoreLayerProperties>.None,
            Some(id),
            (node, _) => node.Detail(id).IsNone,
            (_, staged, _) => IO.lift(() => staged.DeletePerViewportSettings(id)));

    // --- [ROWS]
    private static LayerProperty<TValue> Plain<TValue>(string name, Option<RestoreLayerProperties> restored, Func<LayerNode, TValue, bool> holds, Action<Layer, TValue> write) =>
        new(name, restored, Option<Guid>.None, holds, (_, staged, value) => IO.lift(() => write(staged, value)));

    private static LayerProperty<Option<TValue>> Unsettable<TValue>(string name, Option<RestoreLayerProperties> restored, Func<LayerNode, Option<TValue>, bool> holds, Action<Layer, TValue> set, Action<Layer> unset) =>
        Plain(name, restored, holds, (staged, value) => value.Match(wanted => set(staged, wanted), () => unset(staged)));

    private static LayerProperty<TValue> Indexed<TValue>(string name, Option<RestoreLayerProperties> restored, Func<LayerNode, TValue, bool> holds, Func<RhinoDoc, TValue, IO<int>> resolve, Action<Layer, int> assign) =>
        new(name, restored, Option<Guid>.None, holds, (doc, staged, value) => resolve(doc, value).Bind(index => IO.lift(() => assign(staged, index))));

    private static Fin<LayerProperty<Option<TValue>>> Detail<TValue>(
        string name,
        Option<RestoreLayerProperties> restored,
        Guid viewport,
        Func<PerViewportSettings, TValue, bool> holds,
        Action<Layer, Guid, TValue> set,
        Action<Layer, Guid> delete) =>
        from id in Addressed(viewport)
        select new LayerProperty<Option<TValue>>(
            name,
            restored,
            Some(id),
            (node, value) => value.Match(Some: wanted => node.Detail(id).Exists(row => holds(row, wanted)), None: () => node.Detail(id).IsNone),
            (_, staged, value) => IO.lift(() => value.Match(Some: wanted => set(staged, id, wanted), None: () => delete(staged, id))));

    private static Fin<Guid> Addressed(Guid viewport) =>
        Answers.Present(viewport).ToFin(new Invalid(nameof(viewport)));
}

public static class Layers {
    // --- [RESOLUTION]
    public static IO<Layer> ResolveLayer(RhinoDoc doc, LayerRef address, bool includeDeleted) =>
        address.Switch(
            (Doc: doc, IncludeDeleted: includeDeleted),
            row: static (state, row) => IO.lift(() => row.Address.Switch(
                    state.Doc.Layers,
                    byId: static (_, byId) => Fin.Succ<ComponentRef>(byId),
                    byIndex: static (_, byIndex) => Fin.Succ<ComponentRef>(byIndex),
                    byName: Pathed))
                .Bind(address => TableOps.Find(state.Doc.Layers, address, state.IncludeDeleted)),
            current: static (state, _) => IO.lift(() => state.Doc.Layers.CurrentLayer));

    private static Fin<ComponentRef> Pathed(LayerTable layers, ComponentRef.ByName byName) =>
        layers.FindByFullPath(byName.Name, RhinoMath.UnsetIntIndex) is var index and >= 0 ? new ComponentRef.ByIndex(index) : new MissingComponent(layers.ComponentType, byName);

    // --- [READS]
    public static IO<LayerTree> ReadLayers(RhinoDoc doc, Seq<Guid> viewports) =>
        from rows in IO.lift(() => Sorted(doc).Map(order => order.Map(index => doc.Layers[index]).Strict()))
        from roots in IO.lift(() => Tree(rows, viewports))
        from current in IO.lift(() => doc.Layers.CurrentLayer.Id)
        select new LayerTree(roots, current);

    public static IO<Option<Seq<int>>> PanelSelection(RhinoDoc doc) =>
        IO.lift(() => Answers.Found(doc.Layers.GetSelected(out List<int> indices), toSeq(indices)));

    private static Fin<Seq<int>> Sorted(RhinoDoc doc) =>
        Answers.NonEmpty(toSeq(doc.Layers.GetSorted()), nameof(LayerTable.GetSorted));

    private static Fin<Seq<LayerNode>> Tree(Seq<Layer> rows, Seq<Guid> viewports) {
        Seq<LayerNode> roots = Grown(toHashMap(rows.GroupBy(static row => row.ParentLayerId).Select(static group => (group.Key, toSeq(group).Strict()))), Guid.Empty, viewports);
        LanguageExt.HashSet<Guid> reached = toHashSet(roots.Bind(static root => root.Subtree).Map(static node => node.Id));
        return reached.Count == rows.Count ? roots : new LayerUnreachable(rows.Map(static row => row.Id).Filter(id => !reached.Contains(id)).Strict());
    }

    private static Seq<LayerNode> Grown(HashMap<Guid, Seq<Layer>> byParent, Guid parent, Seq<Guid> viewports) =>
        byParent.Find(parent).ToSeq().Flatten()
            .Map(row => LayerMapper.ToNode(
                row,
                row.GetPersistentLocking(),
                viewports.Filter(row.HasPerViewportSettings).Map(viewport => PerViewport(row, viewport)).Strict(),
                Grown(byParent, row.Id, viewports)))
            .Strict();

    private static PerViewportSettings PerViewport(Layer layer, Guid viewport) =>
        new(
            viewport,
            layer.PerViewportColor(viewport),
            layer.PerViewportPlotColor(viewport),
            PlotWeight.FromHost(layer.PerViewportPlotWeight(viewport)),
            layer.PerViewportIsVisible(viewport),
            layer.PerViewportPersistentVisibility(viewport));

    // --- [WRITES]
    public static IO<Seq<int>> ApplyLayerOp(RhinoDoc doc, LayerOp op) =>
        op.Switch(
            doc,
            create: static (document, create) =>
                from id in IO.lift(() => create.Id.Traverse(static id => Answers.Present(id).ToFin(new Invalid(nameof(LayerOp.Create.Id)))).As())
                from parent in create.Parent.Traverse(address => ResolveLayer(document, address, includeDeleted: false)).As()
                from index in TableOps.AddRow(TableAccessors.Layers(document), IO.lift(() => Fresh(create.Name, ParentId(parent), id)), staged => Written(document, staged, create.Edits))
                select Seq(index),
            addPath: static (document, add) => IO.lift(() =>
                Answers.Required(add.Color.Match(Some: color => document.Layers.AddPath(add.Path.FullPath, color), None: () => document.Layers.AddPath(add.Path.FullPath)), nameof(LayerTable.AddPath))
                    .Map(static index => Seq(index))),
            modify: static (document, modify) =>
                from row in ResolveLayer(document, modify.Target, includeDeleted: false)
                from modified in Staged(document, row.Index, staged =>
                    from written in Written(document, staged, modify.Edits)
                    from shown in IO.lift(() => Invalid.Unless((row.Index != document.Layers.CurrentLayerIndex) || (staged.IsVisible && !staged.IsLocked), nameof(LayerTable.CurrentLayer)))
                    select shown)
                select Seq(row.Index),
            reparent: static (document, reparent) =>
                from target in ResolveLayer(document, reparent.Target, includeDeleted: false)
                from parent in reparent.Parent.Traverse(address => ResolveLayer(document, address, includeDeleted: false)).As()
                from acyclic in IO.lift(() => LayerCycle.Unless(!parent.Exists(row => Within(row, target)), target.Id))
                from moved in Reparented(document, target.Index, ParentId(parent))
                select Seq(target.Index),
            setRenderMaterial: static (document, assign) =>
                from owned in IO.lift(() => Invalid.Unless(
                    assign.Material.ForAll(material => material.DocumentOwner?.RuntimeSerialNumber == document.RuntimeSerialNumber),
                    nameof(RenderContent.DocumentOwner)))
                from row in ResolveLayer(document, assign.Target, includeDeleted: false)
                from assigned in IO.lift(() => { row.RenderMaterial = assign.Material.ValueUnsafe(); })
                select Seq(row.Index),
            merge: static (document, merge) =>
                from source in ResolveLayer(document, merge.Source, includeDeleted: false)
                from target in ResolveLayer(document, merge.Target, includeDeleted: false)
                from acyclic in IO.lift(() => LayerCycle.Unless(!Within(target, source), target.Id))
                from moved in TableOps.Apply(document, new TableOp.ModifyAttributes(
                    new ObjectTarget.Query(new ObjectEnumeratorSettings { HiddenObjects = true, IncludeLights = true, LayerIndexFilter = source.Index }),
                    attributes => IO.lift(() => { attributes.LayerIndex = target.Index; }),
                    Quiet: true))
                from adopted in Answers.Present(source.GetChildren()).TraverseM(child => Reparented(document, child.Index, target.Id)).As()
                from deleted in IO.lift(() => Refused.Unless(document.Layers.Delete(source.Index, quiet: true), nameof(LayerTable.Delete)))
                select Seq(source.Index, target.Index),
            duplicate: static (document, duplicate) =>
                from rows in duplicate.Targets.TraverseM(address => ResolveLayer(document, address, includeDeleted: false)).As()
                from indices in IO.lift(() => Answers.NonEmpty(
                    toSeq(document.Layers.Duplicate(rows.Map(static row => row.Index), duplicate.Objects, duplicate.Sublayers)),
                    nameof(LayerTable.Duplicate)))
                select indices,
            delete: static (document, delete) =>
                Call(document, delete.Target, includeDeleted: false, row => document.Layers.Delete(row.Index, delete.Quiet) || row.IsDeleted, nameof(LayerTable.Delete)),
            purge: static (document, purge) =>
                Call(document, purge.Target, includeDeleted: false, row => document.Layers.Purge(row.Index, purge.Quiet), nameof(LayerTable.Purge)),
            undelete: static (document, undelete) =>
                Call(document, undelete.Target, includeDeleted: true, row => document.Layers.Undelete(row.Index), nameof(LayerTable.Undelete)),
            setCurrent: static (document, current) =>
                Call(document, current.Target, includeDeleted: false, row => document.Layers.SetCurrentLayerIndex(row.Index, current.Quiet), nameof(LayerTable.SetCurrentLayerIndex)),
            forceVisible: static (document, force) =>
                Call(document, force.Target, includeDeleted: false, row => document.Layers.ForceLayerVisible(row.Id), nameof(LayerTable.ForceLayerVisible)),
            sortByName: static (document, sort) =>
                from sorted in IO.lift(() => document.Layers.SortByLayerName(sort.Ascending))
                from order in IO.lift(() => Sorted(document))
                select order,
            sort: static (document, sort) =>
                from rows in sort.Order.TraverseM(address => ResolveLayer(document, address, includeDeleted: false)).As()
                let indices = rows.Map(static row => row.Index).Strict()
                from sorted in IO.lift(() => document.Layers.Sort(indices))
                select indices,
            selectInPanel: static (document, panel) =>
                from rows in panel.Targets.TraverseM(address => ResolveLayer(document, address, includeDeleted: false)).As()
                from shown in IO.lift(() => Refused.Unless(document.Layers.Select(rows.Map(static row => row.Index), panel.Deselect), nameof(LayerTable.Select)))
                select rows.Map(static row => row.Index),
            undoModify: static (document, undo) =>
                Call(
                    document,
                    undo.Target,
                    includeDeleted: false,
                    row => undo.Serial.Match(Some: serial => document.Layers.UndoModify(row.Index, serial), None: () => document.Layers.UndoModify(row.Index)),
                    nameof(LayerTable.UndoModify)));

    private static IO<Unit> Written(RhinoDoc doc, Layer staged, Seq<LayerEdit> edits) =>
        edits.TraverseM(edit => edit.Write(doc, staged)).As().Map(static _ => unit);

    private static Layer Fresh(LayerName name, Guid parent, Option<Guid> id) {
        Layer row = Layer.GetDefaultLayerProperties();
        row.Name = name;
        row.ParentLayerId = parent;
        _ = id.Iter(value => row.Id = value);
        return row;
    }

    private static IO<Unit> Staged(RhinoDoc doc, int index, Func<Layer, IO<Unit>> stage) =>
        TableOps.ModifyRow(TableAccessors.Layers(doc), index, stage, quiet: true);

    private static IO<Unit> Reparented(RhinoDoc doc, int index, Guid parent) =>
        Staged(doc, index, staged => IO.lift(() => { staged.ParentLayerId = parent; }));

    private static bool Within(Layer row, Layer root) =>
        (row.Id == root.Id) || row.IsChildOf(root.Id);

    private static Guid ParentId(Option<Layer> parent) =>
        Answers.Unset(parent.Map(static row => row.Id));

    private static IO<Seq<int>> Call(RhinoDoc doc, LayerRef address, bool includeDeleted, Func<Layer, bool> call, string member) =>
        ResolveLayer(doc, address, includeDeleted).Bind(row => IO.lift(() => Refused.Unless(call(row), member).Map(_ => Seq(row.Index))));
}
