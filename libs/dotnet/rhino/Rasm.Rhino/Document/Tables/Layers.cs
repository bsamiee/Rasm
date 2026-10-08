using System.Drawing;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.TopologicalSort;
using Rasm.Drafting;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.FileIO;
using Rhino.Render;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Document.Tables;

// --- [MODELS] --------------------------------------------------------------------------
public sealed class LayerNames : IEqualityComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer { get; } =
        EqualityComparer<string>.Create(static (left, right) => Hash(left) == Hash(right), static name => Hash(name).GetHashCode());

    private static NameHash Hash(string? name) => new(name, Guid.Empty, ModelComponentType.Layer);
}

[ValueObject<string>(SkipIParsable = true, SkipIComparable = true, EqualityComparisonOperators = OperatorsGeneration.None, ComparisonOperators = OperatorsGeneration.None, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
[KeyMemberEqualityComparer<LayerNames, string>]
public sealed partial class LeafName {
    public static bool operator ==(LeafName? left, LeafName? right) => Equals(left, right);

    public static bool operator !=(LeafName? left, LeafName? right) => !Equals(left, right);

    internal static Fin<LeafName> Segment(string name, int segment) =>
        Conversions.Validated<LeafName, string, InvalidRhinoValue>(name).MapFail(cause => new InvalidLayerSegment(segment, cause));

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref string value) =>
        validationError = ModelComponent.IsValidComponentName(value) && !value.Contains(ModelComponent.NamePathSeparator, StringComparison.Ordinal) ? null : new InvalidRhinoValue();
}

[ValueObject<int>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct IgesLevel : System.Numerics.IMinMaxValue<IgesLevel> {
    public static IgesLevel MinValue { get; } = new(0);
    public static IgesLevel MaxValue { get; } = new(int.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref int value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

public sealed record LayerPath(Seq<LeafName> Ancestors, LeafName Leaf) {
    public Seq<LeafName> Segments => Ancestors.Add(Leaf);

    public Option<LayerPath> Parent => Ancestors.Last.Map(last => new LayerPath(Ancestors.Init, last));

    public string Text => string.Join(ModelComponent.NamePathSeparator, Segments);

    public LayerPath Child(LeafName name) => new(Segments, name);

    public static Fin<LayerPath> Parse(string text) => Crossed(toSeq(text.Split(ModelComponent.NamePathSeparator)));

    public static Fin<LayerPath> Of(Seq<StandardName> ancestors, StandardName leaf) => Crossed(ancestors.Add(leaf).Map(static name => name.Text));

    private static Fin<LayerPath> Crossed(Seq<string> parts) =>
        Callbacks.Each(parts, LeafName.Segment).Map(static names => new LayerPath(names.Init, names[^1]));
}

[Union]
public abstract partial record LayerRef {
    public sealed record Row(ComponentRef<Layer> Address) : LayerRef;

    public sealed record Path(LayerPath Value) : LayerRef;

    public sealed record Current() : LayerRef;
}

public sealed record PerViewportSettings(Color Color, Color PlotColor, PlotWeight PlotWeight, bool IsVisible, bool PersistentVisibility);

public sealed record LayerNode(
    Guid Id, LeafName Name, Color Color, Color PlotColor, PlotWeight PlotWeight, Option<LinetypeRef> Linetype,
    Option<int> RenderMaterialIndex, Option<int> SectionStyleIndex, Option<IgesLevel> IgesLevel, Option<string> Description,
    bool IsVisible, bool IsLocked, bool IsExpanded, bool IsReference, bool ModelIsVisible, bool PersistentVisibility,
    bool ModelPersistentVisibility, bool PersistentLocking, bool PerViewportIsVisibleInNewDetails, bool HasCustomSectionStyle,
    HashMap<Guid, PerViewportSettings> PerViewport);

public sealed record LayerTree(ArrayBidirectionalGraph<Guid, SEquatableEdge<Guid>> Graph, HashMap<Guid, LayerNode> Nodes, Seq<Guid> Order, Guid Current) {
    public Seq<Guid> Roots => Order.Filter(Graph.IsInEdgesEmpty);

    public Seq<Guid> Children(Guid layer) => toSeq(Graph.OutEdges(layer)).Map(static edge => edge.Target);

    public Option<Guid> Parent(Guid layer) => toSeq(Graph.InEdges(layer)).Head.Map(static edge => edge.Source);

    public HashMap<Guid, int> Depths =>
        toSeq(Graph.SourceFirstBidirectionalTopologicalSort(TopologicalSortDirection.Forward))
            .Fold(HashMap<Guid, int>(), (depths, id) => depths.Add(id, Parent(id).Map(parent => depths[parent] + 1).IfNone(0)));

    public HashMap<Guid, LayerPath> Paths =>
        toSeq(Graph.SourceFirstBidirectionalTopologicalSort(TopologicalSortDirection.Forward)).Fold(
            HashMap<Guid, LayerPath>(),
            (paths, id) => paths.Add(id, Parent(id).Match(Some: parent => paths[parent].Child(Nodes[id].Name), None: () => new LayerPath(Seq<LeafName>(), Nodes[id].Name))));
}

public sealed record LayerEdit(string Member, RestoreLayerProperties Restores, Option<Guid> Viewport, Func<LayerNode, bool> Holds, Func<RhinoDoc, Layer, IO<Unit>> Write);

public sealed record LayerProperty<TValue>(string Member, RestoreLayerProperties Restores, Option<Guid> Viewport, Func<LayerNode, TValue, bool> Holds, Func<RhinoDoc, Layer, TValue, IO<Unit>> Write) {
    public LayerEdit Set(TValue value) => new(Member, Restores, Viewport, node => Holds(node, value), (doc, staged) => Write(doc, staged, value));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayerOp {
    public sealed record Create(LeafName Name, Option<LayerRef> Parent, Option<Guid> Id, Seq<LayerEdit> Edits) : LayerOp;

    public sealed record AddPath(LayerPath Path, Option<Color> Color) : LayerOp;

    public sealed record Modify(LayerRef Target, Seq<LayerEdit> Edits) : LayerOp;

    public sealed record Reparent(LayerRef Target, Option<LayerRef> Parent, Seq<LayerEdit> Edits) : LayerOp;

    public sealed record AssignMaterial(LayerRef Target, Option<RenderMaterial> Material) : LayerOp;

    public sealed record Merge(LayerRef Source, LayerRef Target) : LayerOp;

    public sealed record Duplicate(Seq<LayerRef> Targets, bool Objects, bool Sublayers) : LayerOp;

    public sealed record Delete(LayerRef Target) : LayerOp;

    public sealed record Purge(LayerRef Target) : LayerOp;

    public sealed record Undelete(ComponentRef<Layer> Target) : LayerOp;

    public sealed record SetCurrent(LayerRef Target) : LayerOp;

    public sealed record ForceVisible(LayerRef Target) : LayerOp;

    public sealed record SortByName(bool Ascending) : LayerOp;

    public sealed record Sort(Seq<LayerRef> Order) : LayerOp;

    public sealed record UndoModify(LayerRef Target, Option<uint> Serial) : LayerOp;

    public sealed record SelectInPanel(Seq<LayerRef> Targets, bool Deselect) : LayerOp;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class LayerMapper {
    [MapProperty(nameof(Layer.PlotWeight), nameof(LayerNode.PlotWeight), Use = nameof(@PlotWeight.FromHost))]
    [MapProperty(nameof(Layer.LinetypeIndex), nameof(LayerNode.Linetype), Use = nameof(@LinetypeRef.FromHost))]
    [MapPropertyFromSource(nameof(LayerNode.PersistentLocking), Use = nameof(PersistentLocking))]
    [MapPropertyFromSource(nameof(LayerNode.HasCustomSectionStyle), Use = nameof(HasCustomSectionStyle))]
    internal static partial LayerNode ToNode(Layer layer, LeafName name, Option<IgesLevel> igesLevel, HashMap<Guid, PerViewportSettings> perViewport);

    private static bool PersistentLocking(Layer layer) => layer.GetPersistentLocking();

    private static bool HasCustomSectionStyle(Layer layer) => DisposalOps.Present(layer.GetCustomSectionStyle());
}

public static class LayerProperties {
    // --- [MODEL]
    public static readonly LayerProperty<LeafName> Name =
        Plain<LeafName>(nameof(Layer.Name), RestoreLayerProperties.None, static (node, value) => string.Equals(node.Name, value, StringComparison.Ordinal), static (staged, value) => staged.Name = value);

    public static readonly LayerProperty<Color> Color =
        Plain<Color>(nameof(Layer.Color), RestoreLayerProperties.Color, static (node, value) => Conversions.Same(node.Color, value), static (staged, value) => staged.Color = value);

    public static readonly LayerProperty<Option<Color>> PlotColor =
        Unsettable<Color>(nameof(Layer.PlotColor), RestoreLayerProperties.PrintColor, static (node, value) => Conversions.Same(node.PlotColor, value), static (staged, value) => staged.PlotColor = value, static staged => staged.DeletePlotColor());

    public static readonly LayerProperty<PlotWeight> PlotWeight =
        Plain<PlotWeight>(nameof(Layer.PlotWeight), RestoreLayerProperties.PrintWidth, static (node, value) => node.PlotWeight == value, static (staged, value) => staged.PlotWeight = value.ToHost());

    public static readonly LayerProperty<LinetypeRef> Linetype =
        Indexed<LinetypeRef>(nameof(Layer.LinetypeIndex), RestoreLayerProperties.Linetype, static (node, value) => node.Linetype == Some(value), static (doc, value) => value.Resolve(doc), static (staged, index) => staged.LinetypeIndex = index);

    public static readonly LayerProperty<Option<ComponentRef<Material>>> RenderMaterialIndex =
        Indexed<Option<ComponentRef<Material>>>(
            nameof(Layer.RenderMaterialIndex),
            RestoreLayerProperties.RenderMaterial,
            static (node, value) => node.RenderMaterialIndex.Map<ComponentRef<Material>>(static index => index) == value,
            static (doc, value) => TableOps.Index(doc.Materials, value),
            static (staged, index) => staged.RenderMaterialIndex = index);

    public static readonly LayerProperty<Option<ComponentRef<SectionStyle>>> SectionStyleIndex =
        Indexed<Option<ComponentRef<SectionStyle>>>(
            nameof(Layer.SectionStyleIndex),
            RestoreLayerProperties.SectionStyle,
            static (node, value) => node.SectionStyleIndex.Map<ComponentRef<SectionStyle>>(static index => index) == value,
            static (doc, value) => TableOps.Index(doc.SectionStyles, value),
            static (staged, index) => staged.SectionStyleIndex = index);

    public static readonly LayerProperty<Option<IgesLevel>> IgesLevel =
        Plain<Option<IgesLevel>>(nameof(Layer.IgesLevel), RestoreLayerProperties.None, static (node, value) => node.IgesLevel == value, static (staged, value) => staged.IgesLevel = Conversions.Unset(value.Map(static level => (int)level)));

    public static readonly LayerProperty<Option<SectionStyle>> CustomSectionStyle =
        Plain<Option<SectionStyle>>(
            nameof(Layer.SetCustomSectionStyle),
            RestoreLayerProperties.None,
            static (node, value) => value.IsNone && !node.HasCustomSectionStyle,
            static (staged, value) => value.Match(Some: staged.SetCustomSectionStyle, None: staged.RemoveCustomSectionStyle));

    public static readonly LayerProperty<Option<string>> Description =
        Plain<Option<string>>(nameof(Layer.Description), RestoreLayerProperties.None, static (node, value) => node.Description == value, static (staged, value) => staged.Description = Conversions.Unset(value));

    // --- [VISIBILITY]
    public static readonly LayerProperty<bool> IsVisible =
        Plain<bool>(nameof(Layer.IsVisible), RestoreLayerProperties.Visible, static (node, value) => node.IsVisible == value, static (staged, value) => staged.IsVisible = value);

    public static readonly LayerProperty<bool> IsLocked =
        Plain<bool>(nameof(Layer.IsLocked), RestoreLayerProperties.Locked, static (node, value) => node.IsLocked == value, static (staged, value) => staged.IsLocked = value);

    public static readonly LayerProperty<bool> IsExpanded =
        Plain<bool>(nameof(Layer.IsExpanded), RestoreLayerProperties.Expanded, static (node, value) => node.IsExpanded == value, static (staged, value) => staged.IsExpanded = value);

    public static readonly LayerProperty<Option<bool>> PersistentVisibility =
        Unsettable<bool>(nameof(Layer.SetPersistentVisibility), RestoreLayerProperties.None, static (node, value) => node.PersistentVisibility == value, static (staged, value) => staged.SetPersistentVisibility(value), static staged => staged.UnsetPersistentVisibility());

    public static readonly LayerProperty<Option<bool>> PersistentLocking =
        Unsettable<bool>(nameof(Layer.SetPersistentLocking), RestoreLayerProperties.None, static (node, value) => node.PersistentLocking == value, static (staged, value) => staged.SetPersistentLocking(value), static staged => staged.UnsetPersistentLocking());

    public static readonly LayerProperty<Option<bool>> ModelIsVisible =
        Unsettable<bool>(nameof(Layer.ModelIsVisible), RestoreLayerProperties.None, static (node, value) => node.ModelIsVisible == value, static (staged, value) => staged.ModelIsVisible = value, static staged => staged.DeleteModelVisible());

    public static readonly LayerProperty<Option<bool>> ModelPersistentVisibility =
        Unsettable<bool>(nameof(Layer.ModelPersistentVisibility), RestoreLayerProperties.None, static (node, value) => node.ModelPersistentVisibility == value, static (staged, value) => staged.ModelPersistentVisibility = value, static staged => staged.UnsetModelPersistentVisibility());

    public static readonly LayerProperty<bool> PerViewportIsVisibleInNewDetails =
        Plain<bool>(nameof(Layer.PerViewportIsVisibleInNewDetails), RestoreLayerProperties.NewDetailOn, static (node, value) => node.PerViewportIsVisibleInNewDetails == value, static (staged, value) => staged.PerViewportIsVisibleInNewDetails = value);

    // --- [DETAILS]
    public static LayerProperty<Option<Color>> PerViewportColor(Guid viewport) =>
        Detail<Color>(nameof(Layer.SetPerViewportColor), RestoreLayerProperties.ViewportColor, viewport, static (row, value) => Conversions.Same(row.Color, value), static (staged, id, value) => staged.SetPerViewportColor(id, value), static (staged, id) => staged.DeletePerViewportColor(id));

    public static LayerProperty<Option<Color>> PerViewportPlotColor(Guid viewport) =>
        Detail<Color>(nameof(Layer.SetPerViewportPlotColor), RestoreLayerProperties.ViewportPrintColor, viewport, static (row, value) => Conversions.Same(row.PlotColor, value), static (staged, id, value) => staged.SetPerViewportPlotColor(id, value), static (staged, id) => staged.DeletePerViewportPlotColor(id));

    public static LayerProperty<Option<PlotWeight>> PerViewportPlotWeight(Guid viewport) =>
        Detail<PlotWeight>(nameof(Layer.SetPerViewportPlotWeight), RestoreLayerProperties.ViewportPrintWidth, viewport, static (row, value) => row.PlotWeight == value, static (staged, id, value) => staged.SetPerViewportPlotWeight(id, value.ToHost()), static (staged, id) => staged.DeletePerViewportPlotWeight(id));

    public static LayerProperty<Option<bool>> PerViewportIsVisible(Guid viewport) =>
        Detail<bool>(nameof(Layer.SetPerViewportVisible), RestoreLayerProperties.ViewportVisible, viewport, static (row, value) => row.IsVisible == value, static (staged, id, value) => staged.SetPerViewportVisible(id, value), static (staged, id) => staged.DeletePerViewportVisible(id));

    public static LayerProperty<Option<bool>> PerViewportPersistentVisibility(Guid viewport) =>
        Detail<bool>(nameof(Layer.SetPerViewportPersistentVisibility), RestoreLayerProperties.None, viewport, static (row, value) => row.PersistentVisibility == value, static (staged, id, value) => staged.SetPerViewportPersistentVisibility(id, value), static (staged, id) => staged.UnsetPerViewportPersistentVisibility(id));

    public static LayerProperty<Unit> DeletePerViewportSettings(Guid viewport) =>
        new(nameof(Layer.DeletePerViewportSettings), RestoreLayerProperties.None, Some(viewport), (node, _) => Cleared(node, viewport), (_, staged, _) => IO.lift(() => staged.DeletePerViewportSettings(viewport)));

    // --- [ROWS]
    private static LayerProperty<TValue> Plain<TValue>(string member, RestoreLayerProperties restores, Func<LayerNode, TValue, bool> holds, Action<Layer, TValue> write) =>
        new(member, restores, None, holds, (_, staged, value) => IO.lift(() => write(staged, value)));

    private static LayerProperty<Option<TValue>> Unsettable<TValue>(string member, RestoreLayerProperties restores, Func<LayerNode, TValue, bool> equal, Action<Layer, TValue> set, Action<Layer> unset) =>
        Plain<Option<TValue>>(member, restores, (node, value) => value.Exists(wanted => equal(node, wanted)), (staged, value) => value.Match(Some: wanted => set(staged, wanted), None: () => unset(staged)));

    private static LayerProperty<TValue> Indexed<TValue>(string member, RestoreLayerProperties restores, Func<LayerNode, TValue, bool> holds, Func<RhinoDoc, TValue, IO<int>> resolve, Action<Layer, int> assign) =>
        new(member, restores, None, holds, (doc, staged, value) => resolve(doc, value).Bind(index => IO.lift(() => assign(staged, index))));

    private static LayerProperty<Option<TValue>> Detail<TValue>(string member, RestoreLayerProperties restores, Guid viewport, Func<PerViewportSettings, TValue, bool> equal, Action<Layer, Guid, TValue> set, Action<Layer, Guid> delete) =>
        new(
            member,
            restores,
            Some(viewport),
            (node, value) => value.Match(Some: wanted => node.PerViewport.Find(viewport).Exists(row => equal(row, wanted)), None: () => Cleared(node, viewport)),
            (_, staged, value) => IO.lift(() => value.Match(Some: wanted => set(staged, viewport, wanted), None: () => delete(staged, viewport))));

    private static bool Cleared(LayerNode node, Guid viewport) =>
        Conversions.Present(viewport).Exists(id => !node.PerViewport.ContainsKey(id));
}

public static class Layers {
    // --- [RESOLUTION]
    public static IO<Layer> Resolve(RhinoDoc doc, LayerRef address) =>
        address.Switch(
            doc,
            row: static (document, row) => TableOps.Find(document.Layers, row.Address, includeDeleted: false),
            path: static (document, path) => IO.lift(() =>
                Conversions.Present(document.Layers.FindByFullPath(path.Value.Text, RhinoMath.UnsetIntIndex))
                    .ToFin(new MissingLayerPath(path.Value.Text))
                    .Map(index => document.Layers[index])),
            current: static (document, _) => IO.lift(() => document.Layers.CurrentLayer));

    // --- [READS]
    public static IO<LayerTree> Read(RhinoDoc doc, Seq<Guid> viewports) =>
        IO.lift(() =>
            from order in Conversions.NonEmpty(toSeq(doc.Layers.GetSorted()).Map(index => doc.Layers[index]).Strict(), nameof(LayerTable.GetSorted))
            from nodes in Callbacks.Each(order, (layer, row) => Node(layer, row, viewports))
            let ids = order.Map(static layer => layer.Id).Strict()
            select new LayerTree(
                Graph(ids, order.Fold(HashMap<Guid, Seq<Guid>>(), static (children, layer) => children.AddOrUpdate(layer.ParentLayerId, held => held.Add(layer.Id), Seq(layer.Id)))),
                toHashMap(nodes.Map(static node => (node.Id, node))),
                ids,
                doc.Layers.CurrentLayer.Id));

    public static IO<Option<Seq<Guid>>> Selected(RhinoDoc doc) =>
        IO.lift(() => Callbacks.Found(doc.Layers.GetSelected(out List<int> indices), toSeq(indices).Map(index => doc.Layers[index].Id).Strict()));

    private static Fin<LayerNode> Node(Layer layer, int row, Seq<Guid> viewports) =>
        (LeafName.Segment(layer.Name, row).ToValidation(),
         Conversions.Present(layer.IgesLevel).Traverse(static level => Conversions.Validated<IgesLevel, int, InvalidRhinoValue>(level)).As().ToValidation())
            .Apply((name, level) => LayerMapper.ToNode(layer, name, level, Overrides(layer, viewports)))
            .As()
            .ToFin();

    private static HashMap<Guid, PerViewportSettings> Overrides(Layer layer, Seq<Guid> viewports) =>
        toHashMap(viewports.Filter(viewport => Conversions.Present(viewport).Exists(layer.HasPerViewportSettings)).Map(viewport => (viewport, new PerViewportSettings(
            layer.PerViewportColor(viewport),
            layer.PerViewportPlotColor(viewport),
            PlotWeight.FromHost(layer.PerViewportPlotWeight(viewport)),
            layer.PerViewportIsVisible(viewport),
            layer.PerViewportPersistentVisibility(viewport)))));

    private static ArrayBidirectionalGraph<Guid, SEquatableEdge<Guid>> Graph(Seq<Guid> order, HashMap<Guid, Seq<Guid>> children) =>
        order.ToBidirectionalGraph(
                parent => children.Find(parent).ToSeq().Flatten().Map(child => new SEquatableEdge<Guid>(parent, child)),
                allowParallelEdges: false)
            .ToArrayBidirectionalGraph();

    // --- [WRITES]
    public static IO<Seq<Guid>> Apply(RhinoDoc doc, LayerOp op) =>
        op.Switch(
            doc,
            create: static (document, create) =>
                from parent in create.Parent.Traverse(address => Resolve(document, address)).As()
                from id in (
                    from staged in use(Layer.GetDefaultLayerProperties)
                    from parented in Parented(staged, parent)
                    from identified in IO.lift(() => create.Id.Iter(value => staged.Id = value))
                    from written in Written(document, staged, LayerProperties.Name.Set(create.Name).Cons(create.Edits))
                    from added in IO.lift(() => Conversions.Present(TableKinds.Layers.Add(document, staged))
                        .ToFin(new Taken(nameof(LayerTable.Add), create.Name))
                        .Map(index => document.Layers[index].Id))
                    select added).Bracket()
                select Seq(id),
            addPath: static (document, add) => IO.lift(() =>
                Conversions.Required(add.Color.Match(Some: color => document.Layers.AddPath(add.Path.Text, color), None: () => document.Layers.AddPath(add.Path.Text)), nameof(LayerTable.AddPath))
                    .Map(index => Seq(document.Layers[index].Id))),
            modify: static (document, modify) =>
                from row in Resolve(document, modify.Target)
                from landed in Landed(document, row, staged => Written(document, staged, modify.Edits))
                select Seq(row.Id),
            reparent: static (document, reparent) =>
                from target in Resolve(document, reparent.Target)
                from parent in reparent.Parent.Traverse(address => Resolve(document, address)).As()
                from acyclic in IO.lift(() => parent.Traverse(row => LayerCycle.Unless(!Within(row, target), target.Id, row.Id)).As())
                from landed in Landed(document, target, staged => Parented(staged, parent).Bind(_ => Written(document, staged, reparent.Edits)))
                select Seq(target.Id),
            assignMaterial: static (document, assign) =>
                from row in Resolve(document, assign.Target)
                from owned in IO.lift(() => assign.Material.Traverse(material =>
                    ForeignContent.Unless(Conversions.Serial(material.DocumentOwner) == Some(document.RuntimeSerialNumber), material)).As())
                from assigned in IO.lift(() => { row.RenderMaterial = owned.ValueUnsafe(); })
                select Seq(row.Id),
            merge: static (document, merge) =>
                from source in Resolve(document, merge.Source)
                from target in Resolve(document, merge.Target)
                from acyclic in IO.lift(() => LayerCycle.Unless(!Within(target, source), source.Id, target.Id))
                from moved in TableOps.Apply(document, new TableOp.ModifyAttributes(
                    new ObjectTarget.Query(new ObjectEnumeratorSettings { HiddenObjects = true, IncludeLights = true, LayerIndexFilter = source.Index }),
                    attributes => IO.lift(() => { attributes.LayerIndex = target.Index; }),
                    Quiet: true))
                from adopted in Conversions.Rows(source.GetChildren()).TraverseM(child => Landed(document, child, staged => Parented(staged, Some(target)))).As()
                from deleted in IO.lift(() => Refused.Unless(document.Layers.Delete(source.Index, quiet: true), nameof(LayerTable.Delete)))
                select Seq(target.Id),
            duplicate: static (document, duplicate) =>
                from rows in duplicate.Targets.TraverseM(address => Resolve(document, address)).As()
                from created in IO.lift(() => Conversions.NonEmpty(
                    toSeq(document.Layers.Duplicate(rows.Map(static row => row.Index), duplicate.Objects, duplicate.Sublayers)).Map(index => document.Layers[index].Id).Strict(),
                    nameof(LayerTable.Duplicate)))
                select created,
            delete: static (document, delete) =>
                Called(Resolve(document, delete.Target), row => document.Layers.Delete(row.Index, quiet: true) || row.IsDeleted, nameof(LayerTable.Delete)),
            purge: static (document, purge) =>
                Called(Resolve(document, purge.Target), row => document.Layers.Purge(row.Index, quiet: true), nameof(LayerTable.Purge)),
            undelete: static (document, undelete) =>
                Called(TableOps.Find(document.Layers, undelete.Target, includeDeleted: true), row => document.Layers.Undelete(row.Index), nameof(LayerTable.Undelete)),
            setCurrent: static (document, current) =>
                Called(Resolve(document, current.Target), row => document.Layers.SetCurrentLayerIndex(row.Index, quiet: true), nameof(LayerTable.SetCurrentLayerIndex)),
            forceVisible: static (document, force) =>
                Called(Resolve(document, force.Target), row => row.IsVisible || document.Layers.ForceLayerVisible(row.Id), nameof(LayerTable.ForceLayerVisible)),
            sortByName: static (document, sort) =>
                IO.lift(() => document.Layers.SortByLayerName(sort.Ascending)).Map(static _ => Seq<Guid>()),
            sort: static (document, sort) =>
                from rows in sort.Order.TraverseM(address => Resolve(document, address)).As()
                from sorted in IO.lift(() => document.Layers.Sort(rows.Map(static row => row.Index)))
                select rows.Map(static row => row.Id),
            undoModify: static (document, undo) =>
                Called(Resolve(document, undo.Target), row => document.Layers.UndoModify(row.Index, Conversions.Unset(undo.Serial)), nameof(LayerTable.UndoModify)),
            selectInPanel: static (document, panel) =>
                from rows in panel.Targets.TraverseM(address => Resolve(document, address)).As()
                from shown in IO.lift(() => Missing.Unless(document.Layers.Select(rows.Map(static row => row.Index), panel.Deselect), nameof(LayerTable.Select)))
                select rows.Map(static row => row.Id));

    private static IO<Unit> Landed(RhinoDoc doc, Layer live, Func<Layer, IO<Unit>> stage) =>
        (from staged in use(() => TableKinds.Layers.Copy(live))
         from staging in stage(staged)
         from landed in IO.lift(() => Refused.Unless(TableKinds.Layers.Modify(doc, staged, live.Index), nameof(LayerTable.Modify)))
         select landed).Bracket();

    private static IO<Unit> Written(RhinoDoc doc, Layer staged, Seq<LayerEdit> edits) =>
        edits.TraverseM(edit => edit.Write(doc, staged)).As().Map(static _ => unit);

    private static IO<Unit> Parented(Layer staged, Option<Layer> parent) =>
        IO.lift(() => { staged.ParentLayerId = Conversions.Unset(parent.Map(static row => row.Id)); });

    private static bool Within(Layer row, Layer root) =>
        row.Id == root.Id || row.IsChildOf(root.Id);

    private static IO<Seq<Guid>> Called(IO<Layer> resolved, Func<Layer, bool> call, string member) =>
        resolved.Bind(row => IO.lift(() => Refused.Unless(call(row), Seq(row.Id), member)));
}
