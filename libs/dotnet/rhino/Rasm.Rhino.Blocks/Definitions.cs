using System.Drawing;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.FileIO;
using Riok.Mapperly.Abstractions;

[assembly: UseStaticMapper(typeof(Answers))]

namespace Rasm.Rhino.Blocks;

// --- [TYPES] ---------------------------------------------------------------------------
public enum ReferenceScope { TopLevel = 0, TopLevelAndNested = 1, FromOtherDefinitions = 2 }

// --- [MODELS] --------------------------------------------------------------------------
public sealed record BlockUsage(int TopLevel, int Nested) {
    public int Total => TopLevel + Nested;
}

public sealed record BlockPlacement(Guid Id, Transform Xform);

public sealed record Hyperlink(string Address, Option<string> Tag) {
    public Option<string> Tag { get; } = Tag.Bind(Answers.Present);
}

public sealed record BlockMetadata(string Name, Option<string> Description, Option<Hyperlink> Link) {
    public Option<string> Description { get; } = Description.Bind(Answers.Present);

    public Option<Hyperlink> Link { get; } = Link.Filter(static link => link.Address.Length > 0);
}

public sealed record SourceReference(string FullPath, Option<string> RelativePath);

public sealed record BlockState(
    Guid Id,
    int Index,
    BlockMetadata Metadata,
    InstanceDefinitionUpdateType UpdateType,
    Option<SourceReference> Archive,
    InstanceDefinitionArchiveFileStatus ArchiveFileStatus,
    InstanceDefinitionLayerStyle LayerStyle,
    bool SkipNestedLinkedDefinitions,
    bool IsTenuous,
    bool IsReference,
    UnitSystem UnitSystem,
    Seq<Guid> MemberIds,
    BlockUsage Usage,
    HashMap<string, string> UserStrings) {
    public bool Hidden => Metadata.Name.StartsWith('*');
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PreviewMethod {
    public sealed record ByMode(DefinedViewportProjection Projection, DisplayMode DisplayMode, Size Size, bool ApplyDpiScaling) : PreviewMethod;

    public sealed record ByCamera(Guid DisplayModeId, DefinedViewportProjection Projection, IsometricCamera Camera, bool DrawDecorations, Size Size, bool ApplyDpiScaling) : PreviewMethod;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DefinitionEdit {
    public sealed record Modify(BlockMetadata Metadata) : DefinitionEdit;

    public sealed record ModifyGeometry(Seq<GeometryPair> Members) : DefinitionEdit;

    public sealed record ModifyInsertionPlane(Plane Plane) : DefinitionEdit;

    public sealed record ModifySourceArchive(SourceReference Source, InstanceDefinitionUpdateType UpdateType, InstanceDefinitionLayerStyle LayerStyle) : DefinitionEdit;

    public sealed record DestroySourceArchive() : DefinitionEdit;

    public sealed record RefreshLinkedBlock() : DefinitionEdit;

    public sealed record UpdateLinked(string Path, bool UpdateNestedLinks) : DefinitionEdit;

    public sealed record SetLayerStyle(InstanceDefinitionLayerStyle Style) : DefinitionEdit;

    public sealed record SetSkipNested(bool Skip) : DefinitionEdit;

    public sealed record Delete(bool DeleteReferences) : DefinitionEdit;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DefinitionOp {
    public sealed record Add(BlockMetadata Metadata, Point3d BasePoint, Seq<GeometryPair> Members, bool OverrideExisting) : DefinitionOp;

    public sealed record CreateFromFile(
        string Path,
        BlockMetadata Metadata,
        InstanceDefinitionUpdateType UpdateType,
        InstanceDefinitionLayerStyle LayerStyle,
        InstanceDefinitionNameConflictResolution Conflict,
        bool SkipNestedLinkedDefinitions) : DefinitionOp;

    public sealed record Edit(ComponentRef Key, DefinitionEdit Change) : DefinitionOp;

    public sealed record Undelete(ComponentRef Key) : DefinitionOp;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class DefinitionMapper {
    [MapProperty(nameof(InstanceObject.InstanceXform), nameof(BlockPlacement.Xform))]
    internal static partial BlockPlacement ToPlacement(InstanceObject reference);

    [MapPropertyFromSource(nameof(BlockState.Metadata), Use = nameof(MetadataOf))]
    [MapPropertyFromSource(nameof(BlockState.Archive), Use = nameof(ArchiveOf))]
    [MapPropertyFromSource(nameof(BlockState.MemberIds), Use = nameof(MemberIdsOf))]
    [MapPropertyFromSource(nameof(BlockState.Usage), Use = nameof(UsageOf))]
    internal static partial BlockState ToState(InstanceDefinition definition, HashMap<string, string> userStrings);

    [MapProperty(nameof(InstanceDefinitionGeometry.IsLinkedType), nameof(DefinitionNode.Linked))]
    [MapPropertyFromSource(nameof(DefinitionNode.Members), Use = nameof(MemberIdsOf))]
    internal static partial DefinitionNode ToNode(InstanceDefinitionGeometry definition);

    internal static BlockMetadata MetadataOf(InstanceDefinition definition) =>
        new(
            definition.Name,
            Answers.Present(definition.Description),
            Answers.Present(definition.Url).Map(address => new Hyperlink(address, Answers.Present(definition.UrlDescription))));

    private static Option<SourceReference> ArchiveOf(InstanceDefinition definition) =>
        Answers.Present(definition.SourceArchive).Map(path => new SourceReference(path, Answers.Present(definition.SourceArchiveRelativePath)));

    private static Seq<Guid> MemberIdsOf<TDefinition>(TDefinition definition) where TDefinition : InstanceDefinitionGeometry =>
        toSeq(definition.GetObjectIds());

    private static BlockUsage UsageOf(InstanceDefinition definition) {
        _ = definition.UseCount(out int topLevel, out int nested);
        return new BlockUsage(topLevel, nested);
    }
}

public static class Definitions {
    // --- [READS]
    public static IO<BlockState> Snapshot(RhinoDoc doc, ComponentRef key) =>
        TableOps.Find(doc.InstanceDefinitions, key, includeDeleted: false).Bind(State);

    public static IO<Seq<BlockPlacement>> References(RhinoDoc doc, ComponentRef key, ReferenceScope scope) =>
        TableOps.Find(doc.InstanceDefinitions, key, includeDeleted: false).Map(definition =>
            Answers.Present(definition.GetReferences((int)scope)).Map(DefinitionMapper.ToPlacement).Strict());

    public static IO<Option<int>> Nesting(RhinoDoc doc, ComponentRef outer, ComponentRef inner) =>
        from container in TableOps.Find(doc.InstanceDefinitions, outer, includeDeleted: false)
        from nested in TableOps.Find(doc.InstanceDefinitions, inner, includeDeleted: false)
        select container.Index == nested.Index ? Option<int>.None : Some(container.UsesDefinition(nested.Index)).Filter(static levels => levels > 0);

    public static IO<bool> UsesLayer(RhinoDoc doc, ComponentRef key, int layerIndex) =>
        TableOps.Find(doc.InstanceDefinitions, key, includeDeleted: false).Map(definition => definition.UsesLayer(layerIndex));

    public static IO<bool> UsesLinetype(RhinoDoc doc, ComponentRef key, int linetypeIndex) =>
        TableOps.Find(doc.InstanceDefinitions, key, includeDeleted: false).Map(definition => definition.UsesLinetype(linetypeIndex));

    public static IO<string> UnusedName(RhinoDoc doc, Option<string> root) =>
        IO.lift(() => root.Match(Some: doc.InstanceDefinitions.GetUnusedInstanceDefinitionName, None: doc.InstanceDefinitions.GetUnusedInstanceDefinitionName));

    public static IO<TValue> Preview<TValue>(RhinoDoc doc, ComponentRef key, PreviewMethod method, Func<Bitmap, IO<TValue>> body) =>
        TableOps.Find(doc.InstanceDefinitions, key, includeDeleted: false).Bind(definition => DisposalOps.Using(
            IO.lift(() => Missing.Unless(
                method.Switch(
                    definition,
                    byMode: static (target, mode) => target.CreatePreviewBitmap(mode.Projection, mode.DisplayMode, mode.Size, mode.ApplyDpiScaling),
                    byCamera: static (target, camera) =>
                        target.CreatePreviewBitmap(camera.DisplayModeId, camera.Projection, camera.Camera, camera.DrawDecorations, camera.Size, camera.ApplyDpiScaling)),
                nameof(InstanceDefinition.CreatePreviewBitmap))),
            body));

    internal static IO<Seq<InstanceDefinition>> Rows(RhinoDoc doc) =>
        IO.lift(() => Answers.Present(doc.InstanceDefinitions.GetList(ignoreDeleted: true)));

    private static IO<BlockState> State(InstanceDefinition definition) =>
        GeometryOps.ReadUserStrings(GeometryOps.UserStrings(definition)).Map(strings => DefinitionMapper.ToState(definition, strings));

    // --- [WRITES]
    public static IO<Seq<int>> Commit(RhinoDoc doc, string name, RedrawPolicy redraw, Seq<DefinitionOp> ops) =>
        Commits.Commit(doc, name, redraw, ops.TraverseM(op => Apply(doc, op)).As());

    public static IO<Unit> Purge(RhinoDoc doc, ComponentRef key) =>
        TableOps.Find(doc.InstanceDefinitions, key, includeDeleted: true).Bind(definition => IO.lift(() => Refused.Unless(doc.InstanceDefinitions.Purge(definition.Index), nameof(InstanceDefinitionTable.Purge))));

    public static IO<Unit> Export(RhinoDoc doc, ComponentRef key, string path) =>
        TableOps.Find(doc.InstanceDefinitions, key, includeDeleted: false).Bind(definition => IO.lift(() => Refused.Unless(doc.InstanceDefinitions.Export(definition.Index, path), nameof(InstanceDefinitionTable.Export))));

    public static IO<Unit> EditUserStrings(RhinoDoc doc, ComponentRef key, Seq<UserStringEdit> edits) =>
        from definition in TableOps.Find(doc.InstanceDefinitions, key, includeDeleted: false)
        let accessors = GeometryOps.UserStrings(definition)
        from edited in edits.TraverseM(edit => GeometryOps.EditUserStrings(accessors, edit)).As()
        select unit;

    private static IO<int> Apply(RhinoDoc doc, DefinitionOp op) =>
        op.Switch(
            doc,
            add: static (document, add) => WithMembers(document, add.Members, (geometry, attributes) => Answers.Required(
                document.InstanceDefinitions.Add(
                    add.Metadata.Name, Answers.Unset(add.Metadata.Description), Url(add.Metadata), UrlTag(add.Metadata), add.BasePoint, geometry, attributes, add.OverrideExisting),
                nameof(InstanceDefinitionTable.Add))),
            createFromFile: static (document, create) =>
                from count in IO.lift(() => document.InstanceDefinitions.Count)
                from index in IO.lift(() => Answers.Required(
                    document.InstanceDefinitions.CreateFromFile(
                        create.Path,
                        create.Metadata.Name,
                        Answers.Unset(create.Metadata.Description),
                        Url(create.Metadata),
                        UrlTag(create.Metadata),
                        create.UpdateType,
                        create.LayerStyle,
                        create.Conflict,
                        create.SkipNestedLinkedDefinitions),
                    nameof(InstanceDefinitionTable.CreateFromFile)))
                from row in TableOps.Row(document.InstanceDefinitions, index)
                from created in IO.lift(() => AlreadyLinked.Unless(index >= count || TableOps.Names<InstanceDefinition>().Equals(row.Name, create.Metadata.Name), create.Path, row.Id))
                select index,
            edit: static (document, edit) =>
                from definition in TableOps.Find(document.InstanceDefinitions, edit.Key, includeDeleted: false)
                from edited in Edited(document, definition, edit.Change)
                select definition.Index,
            undelete: static (document, undelete) =>
                from definition in TableOps.Find(document.InstanceDefinitions, undelete.Key, includeDeleted: true)
                from restored in IO.lift(() => Refused.Unless(document.InstanceDefinitions.Undelete(definition.Index), nameof(InstanceDefinitionTable.Undelete)))
                select definition.Index);

    private static IO<Unit> Edited(RhinoDoc doc, InstanceDefinition definition, DefinitionEdit change) =>
        change.Switch(
            (Doc: doc, Definition: definition),
            modify: static (state, modify) => IO.lift(() => Refused.Unless(
                state.Doc.InstanceDefinitions.Modify(
                    state.Definition.Index, modify.Metadata.Name, Answers.Unset(modify.Metadata.Description), Url(modify.Metadata), UrlTag(modify.Metadata), quiet: true),
                nameof(InstanceDefinitionTable.Modify))),
            modifyGeometry: static (state, modify) => WithMembers(state.Doc, modify.Members, (geometry, attributes) => Refused.Unless(
                state.Doc.InstanceDefinitions.ModifyGeometry(state.Definition.Index, geometry, attributes),
                nameof(InstanceDefinitionTable.ModifyGeometry))),
            modifyInsertionPlane: static (state, modify) => IO.lift(() => Refused.Unless(
                state.Doc.InstanceDefinitions.ModifyInsertionPlane(state.Definition.Index, modify.Plane),
                nameof(InstanceDefinitionTable.ModifyInsertionPlane))),
            modifySourceArchive: static (state, modify) => DisposalOps.Using(
                IO.lift(() => Missing.Unless(
                    FileReference.CreateFromFullAndRelativePaths(modify.Source.FullPath, modify.Source.RelativePath.ValueUnsafe()),
                    nameof(FileReference))),
                reference => IO.lift(() => Refused.Unless(
                    state.Doc.InstanceDefinitions.ModifySourceArchive(state.Definition.Index, reference, modify.UpdateType, modify.LayerStyle, quiet: true),
                    nameof(InstanceDefinitionTable.ModifySourceArchive)))),
            destroySourceArchive: static (state, _) => IO.lift(() => Refused.Unless(
                state.Doc.InstanceDefinitions.DestroySourceArchive(state.Definition, quiet: true),
                nameof(InstanceDefinitionTable.DestroySourceArchive))),
            refreshLinkedBlock: static (state, _) => IO.lift(() => Refused.Unless(
                state.Doc.InstanceDefinitions.RefreshLinkedBlock(state.Definition),
                nameof(InstanceDefinitionTable.RefreshLinkedBlock))),
            updateLinked: static (state, update) => IO.lift(() => Refused.Unless(
                state.Doc.InstanceDefinitions.UpdateLinkedInstanceDefinition(state.Definition.Index, update.Path, update.UpdateNestedLinks, quiet: true),
                nameof(InstanceDefinitionTable.UpdateLinkedInstanceDefinition))),
            setLayerStyle: static (state, set) =>
                from accepted in IO.lift(() => Invalid.Unless(
                    (state.Definition.UpdateType == InstanceDefinitionUpdateType.Linked) && (set.Style != InstanceDefinitionLayerStyle.None),
                    nameof(InstanceDefinition.LayerStyle)))
                from written in IO.lift(() => { state.Definition.LayerStyle = set.Style; })
                select written,
            setSkipNested: static (state, set) => IO.lift(() => { state.Definition.SkipNestedLinkedDefinitions = set.Skip; }),
            delete: static (state, delete) =>
                from free in IO.lift(() => DefinitionContained.Unless(state.Definition.Id, toSeq(state.Definition.GetContainers()).Map(static container => container.Id).Strict()))
                from deleted in IO.lift(() => Refused.Unless(
                    state.Doc.InstanceDefinitions.Delete(state.Definition.Index, delete.DeleteReferences, quiet: true),
                    nameof(InstanceDefinitionTable.Delete)))
                select deleted);

    private static string Url(BlockMetadata metadata) =>
        Answers.Unset(metadata.Link.Map(static link => link.Address));

    private static string UrlTag(BlockMetadata metadata) =>
        Answers.Unset(metadata.Link.Bind(static link => link.Tag));

    private static IO<TAnswer> WithMembers<TAnswer>(RhinoDoc doc, Seq<GeometryPair> members, Func<Seq<GeometryBase>, Seq<ObjectAttributes>, Fin<TAnswer>> call) =>
        TableOps.WithAttributes(doc, members.Map(static member => member.Attributes), attributes => IO.lift(() => call(members.Map(static member => member.Geometry), attributes)));
}
