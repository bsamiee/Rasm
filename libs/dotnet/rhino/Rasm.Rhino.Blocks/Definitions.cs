using System.Drawing;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.FileIO;
using Rhino.UI;
using Riok.Mapperly.Abstractions;

[assembly: UseStaticMapper(typeof(Answers))]

namespace Rasm.Rhino.Blocks;

// --- [TYPES] ---------------------------------------------------------------------------
public enum ReferenceScope { TopLevel = 0, TopLevelAndNested = 1, FromOtherDefinitions = 2 }

// --- [MODELS] --------------------------------------------------------------------------
public sealed record BlockUsage(int Total, int TopLevel, int Nested);

public sealed record BlockPlacement(Guid Id, Transform Xform);

public sealed record BlockState(
    Guid Id,
    int Index,
    string Name,
    string Description,
    InstanceDefinitionUpdateType UpdateType,
    InstanceDefinitionArchiveFileStatus ArchiveFileStatus,
    InstanceDefinitionLayerStyle LayerStyle,
    Option<string> SourceArchive,
    Option<string> SourceArchiveRelativePath,
    bool IsTenuous,
    bool SkipNestedLinkedDefinitions,
    UnitSystem UnitSystem,
    Seq<Guid> MemberIds,
    ReferenceScope Scope,
    Seq<BlockPlacement> Placements,
    BlockUsage Usage,
    Seq<Guid> ContainerIds,
    uint GeometryCrc);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Dependency {
    public sealed record OnLayer(int LayerIndex) : Dependency;

    public sealed record WithLinetype(int LinetypeIndex) : Dependency;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PreviewMethod {
    public sealed record DisplayMode(DefinedViewportProjection Projection, global::Rhino.DocObjects.DisplayMode Mode, Size Size, bool ApplyDpiScaling) : PreviewMethod;

    public sealed record Isometric(Guid DisplayModeId, DefinedViewportProjection Projection, IsometricCamera Camera, bool DrawDecorations, Size Size, bool ApplyDpiScaling) : PreviewMethod;
}

public sealed record LinkState(
    InstanceDefinitionUpdateType UpdateType,
    LinkedInstanceDefinitionUpdateStyle LinkedInstanceDefinitionUpdate,
    InstanceDefinitionArchiveFileStatus ArchiveFileStatus,
    bool SkipNestedLinkedDefinitions);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Hyperlink {
    public sealed record Keep() : Hyperlink;

    public sealed record Clear() : Hyperlink;

    public sealed record Set(Uri Url, string Tag) : Hyperlink;
}

public sealed record BlockMetadata(string Name, string Description, Hyperlink Hyperlink);

public sealed record SourceReference(string FullPath, Option<string> RelativePath);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Definitions {
    // --- [RESOLUTION]
    public static IO<Option<InstanceDefinition>> Resolve(RhinoDoc doc, ComponentRef key, bool includeDeleted) =>
        TableOps.Find<InstanceDefinition>(
            key,
            id => Optional(doc.InstanceDefinitions.Find(id, ignoreDeletedInstanceDefinitions: !includeDeleted)),
            index => Some(index).Filter(found => found < doc.InstanceDefinitions.Count).Bind(found => Live(Optional(doc.InstanceDefinitions[found]), includeDeleted)),
            name => Live(Optional(doc.InstanceDefinitions.Find(name)), includeDeleted));

    public static IO<Seq<InstanceDefinition>> Rows(RhinoDoc doc, bool includeDeleted) =>
        IO.lift(() => Answers.Present(doc.InstanceDefinitions.GetList(ignoreDeleted: !includeDeleted)));

    internal static IO<InstanceDefinition> Require(RhinoDoc doc, ComponentRef key, bool includeDeleted) =>
        Resolve(doc, key, includeDeleted).Bind(found => IO.lift(found.ToFin(new DefinitionMissing(key))));

    private static Option<InstanceDefinition> Live(Option<InstanceDefinition> found, bool includeDeleted) =>
        found.Filter(definition => includeDeleted || !definition.IsDeleted);

    // --- [READS]
    public static IO<Seq<BlockPlacement>> References(RhinoDoc doc, ComponentRef key, ReferenceScope scope) =>
        Require(doc, key, includeDeleted: false).Bind(definition => Placements(definition, scope));

    public static IO<Seq<Guid>> Containers(RhinoDoc doc, ComponentRef key) =>
        Require(doc, key, includeDeleted: false).Bind(ContainerIds);

    public static IO<BlockState> Snapshot(RhinoDoc doc, ComponentRef key, ReferenceScope scope) =>
        from definition in Require(doc, key, includeDeleted: false)
        from members in IO.lift(() => Answers.Present(definition.GetObjects()))
        from ids in IO.lift(() => members.TraverseM(member =>
            (member.Geometry.IsValidWithLog(out string geometryLog), member.Attributes.IsValidWithLog(out string attributesLog)) switch {
                (false, _) => new MemberInvalid(key, member.Id, geometryLog),
                (_, false) => new MemberInvalid(key, member.Id, attributesLog),
                _ => Answers.NonEmpty(member.Id, nameof(RhinoObject.Id)),
            }).As())
        from crc in IO.lift(() => members.Fold(0u, static (remainder, member) => member.Geometry.DataCRC(remainder)))
        from placements in Placements(definition, scope)
        from usage in IO.lift(() => new BlockUsage(definition.UseCount(out int topLevel, out int nested), topLevel, nested))
        from containers in ContainerIds(definition)
        from snapshot in IO.lift(() => new BlockState(
            definition.Id,
            definition.Index,
            definition.Name,
            definition.Description,
            definition.UpdateType,
            definition.ArchiveFileStatus,
            definition.LayerStyle,
            Answers.Present(definition.SourceArchive),
            Answers.Present(definition.SourceArchiveRelativePath),
            definition.IsTenuous,
            definition.SkipNestedLinkedDefinitions,
            definition.UnitSystem,
            ids,
            scope,
            placements,
            usage,
            containers,
            crc))
        select snapshot;

    public static IO<Option<int>> Nesting(RhinoDoc doc, ComponentRef outer, ComponentRef inner) =>
        from container in Require(doc, outer, includeDeleted: false)
        from nested in Require(doc, inner, includeDeleted: false)
        from depth in IO.lift(() => container.UsesDefinition(nested.Index))
        select Some(depth).Filter(static levels => levels > 0);

    public static IO<bool> Uses(RhinoDoc doc, ComponentRef key, Dependency dependency) =>
        from definition in Require(doc, key, includeDeleted: false)
        from used in IO.lift(() => dependency.Switch(
            (Doc: doc, Definition: definition),
            onLayer: static (state, layer) =>
                IndexOutOfRange.Unless(layer.LayerIndex, state.Doc.Layers.Count, nameof(Dependency.OnLayer.LayerIndex)).Map(_ => state.Definition.UsesLayer(layer.LayerIndex)),
            withLinetype: static (state, linetype) =>
                IndexOutOfRange.Unless(linetype.LinetypeIndex, state.Doc.Linetypes.Count, nameof(Dependency.WithLinetype.LinetypeIndex)).Map(_ => state.Definition.UsesLinetype(linetype.LinetypeIndex))))
        select used;

    public static IO<string> UnusedName(RhinoDoc doc, Option<string> root) =>
        TableOps.UnusedName(() => root.Match(
            Some: name => doc.InstanceDefinitions.GetUnusedInstanceDefinitionName(name),
            None: () => doc.InstanceDefinitions.GetUnusedInstanceDefinitionName()));

    internal static IO<Seq<BlockPlacement>> Placements(InstanceDefinition definition, ReferenceScope scope) =>
        IO.lift(() =>
            Answers.Present(definition.GetReferences((int)scope)).Map(static reference => DefinitionMapper.ToPlacement(reference)).Strict());

    internal static IO<Seq<Guid>> ContainerIds(InstanceDefinition definition) =>
        IO.lift(() => toSeq(definition.GetContainers()).Map(static container => container.Id).Strict());

    // --- [PREVIEW]
    public static IO<TValue> Preview<TValue>(RhinoDoc doc, ComponentRef key, PreviewMethod method, Func<Bitmap, IO<TValue>> body) =>
        from definition in Require(doc, key, includeDeleted: false)
        from value in Disposal.Using(IO.lift(() => method.Switch(
            definition,
            displayMode: static (target, displayMode) =>
                from projected in Invalid.Unless(displayMode.Projection != DefinedViewportProjection.None, nameof(PreviewMethod))
                from bitmap in Missing.Unless(target.CreatePreviewBitmap(displayMode.Projection, displayMode.Mode, displayMode.Size, displayMode.ApplyDpiScaling), nameof(InstanceDefinition.CreatePreviewBitmap))
                select bitmap,
            isometric: static (target, isometric) =>
                from projected in Invalid.Unless((isometric.Projection != DefinedViewportProjection.None) && (isometric.Camera != IsometricCamera.None), nameof(PreviewMethod))
                from bitmap in Missing.Unless(
                    target.CreatePreviewBitmap(isometric.DisplayModeId, isometric.Projection, isometric.Camera, isometric.DrawDecorations, isometric.Size, isometric.ApplyDpiScaling),
                    nameof(InstanceDefinition.CreatePreviewBitmap))
                select bitmap)), body)
        select value;

    // --- [LINK]
    public static IO<LinkState> ReadLink(RhinoDoc doc, ComponentRef key) =>
        Require(doc, key, includeDeleted: false).Bind(definition => IO.lift(() =>
            new LinkState(definition.UpdateType, doc.LinkedInstanceDefinitionUpdate, definition.ArchiveFileStatus, definition.SkipNestedLinkedDefinitions)));

    public static IO<Unit> SetLinkedInstanceDefinitionUpdate(RhinoDoc doc, LinkedInstanceDefinitionUpdateStyle style) =>
        IO.lift(() => {
            doc.LinkedInstanceDefinitionUpdate = style;
            return Mismatch.Unless(doc.LinkedInstanceDefinitionUpdate == style, nameof(RhinoDoc.LinkedInstanceDefinitionUpdate));
        });

    // --- [WRITES]
    public static IO<int> Add(RhinoDoc doc, BlockMetadata metadata, Point3d basePoint, Seq<GeometryPair> members, InstanceDefinitionNameConflictResolution conflict, RedrawPolicy redraw) =>
        Commits.Commit(doc, LOC.STR("Add block"), redraw,
            from existing in IO.lift(() => Optional(doc.InstanceDefinitions.Find(metadata.Name)).Map(static found => found.Index))
            from name in existing.IsSome && (conflict == InstanceDefinitionNameConflictResolution.KeepBoth) ? UnusedName(doc, Some(metadata.Name)) : IO.pure(metadata.Name)
            let link = UrlAndTag(metadata.Hyperlink)
            let geometry = members.Map(static member => member.Geometry)
            from index in existing.Filter(_ => conflict == InstanceDefinitionNameConflictResolution.KeepCurrent).Match(
                Some: static kept => IO.pure(kept),
                None: () => TableOps.WithAttributes(doc, members.Map(static member => member.Attributes), attributes => IO.lift(() => Answers.NonNegative(
                    doc.InstanceDefinitions.Add(
                        name,
                        metadata.Description,
                        link.Url,
                        link.Tag,
                        basePoint,
                        geometry,
                        attributes,
                        overrideExisting: existing.IsSome && (conflict == InstanceDefinitionNameConflictResolution.Redefine)),
                    nameof(InstanceDefinitionTable.Add)))))
            select index);

    public static IO<int> CreateFromFile(
        RhinoDoc doc,
        string filename,
        BlockMetadata metadata,
        InstanceDefinitionUpdateType updateType,
        InstanceDefinitionLayerStyle layerStyle,
        InstanceDefinitionNameConflictResolution conflict,
        bool skipNested,
        RedrawPolicy redraw) =>
        Commits.Commit(doc, LOC.STR("Create block from file"), redraw,
            IO.pure(UrlAndTag(metadata.Hyperlink)).Bind(link => IO.lift(() => Answers.NonNegative(
                doc.InstanceDefinitions.CreateFromFile(filename, metadata.Name, metadata.Description, link.Url, link.Tag, updateType, layerStyle, conflict, skipNested),
                nameof(InstanceDefinitionTable.CreateFromFile)))));

    public static IO<Unit> Modify(RhinoDoc doc, ComponentRef key, BlockMetadata metadata, bool quiet, RedrawPolicy redraw) =>
        Commits.Commit(doc, LOC.STR("Modify block"), redraw, Call(
            doc,
            key,
            includeDeleted: false,
            definition => metadata.Hyperlink.Switch(
                (Table: doc.InstanceDefinitions, definition.Index, Metadata: metadata, Quiet: quiet),
                keep: static (state, _) => state.Table.Modify(state.Index, state.Metadata.Name, state.Metadata.Description, state.Quiet),
                clear: static (state, _) => state.Table.Modify(state.Index, state.Metadata.Name, state.Metadata.Description, "", "", state.Quiet),
                set: static (state, set) => state.Table.Modify(state.Index, state.Metadata.Name, state.Metadata.Description, set.Url.OriginalString, set.Tag, state.Quiet)),
            nameof(InstanceDefinitionTable.Modify)));

    public static IO<Unit> ModifyGeometry(RhinoDoc doc, ComponentRef key, Seq<GeometryPair> members, RedrawPolicy redraw) =>
        Commits.Commit(doc, LOC.STR("Modify block geometry"), redraw,
            from definition in Require(doc, key, includeDeleted: false)
            from updateType in IO.lift(() => definition.UpdateType)
            from embedded in IO.lift(() => WrongUpdateType.Unless(key, updateType, Seq(InstanceDefinitionUpdateType.Static)))
            from replaced in TableOps.WithAttributes(doc, members.Map(static member => member.Attributes), attributes => IO.lift(() => Refused.Unless(
                doc.InstanceDefinitions.ModifyGeometry(definition.Index, members.Map(static member => member.Geometry), attributes),
                nameof(InstanceDefinitionTable.ModifyGeometry))))
            select replaced);

    private static (string? Url, string? Tag) UrlAndTag(Hyperlink link) =>
        link.Switch<(string? Url, string? Tag)>(
            keep: static _ => (null, null),
            clear: static _ => ("", ""),
            set: static set => (set.Url.OriginalString, set.Tag));

    // --- [SOURCE]
    public static IO<Unit> ModifySourceArchive(RhinoDoc doc, ComponentRef key, SourceReference source, InstanceDefinitionUpdateType updateType, InstanceDefinitionLayerStyle layerStyle, bool quiet, RedrawPolicy redraw) =>
        Commits.Commit(doc, LOC.STR("Modify block source archive"), redraw,
            from definition in Require(doc, key, includeDeleted: false)
            from bound in Disposal.Using(
                () => source.RelativePath.Match(
                    Some: relative => FileReference.CreateFromFullAndRelativePaths(source.FullPath, relative),
                    None: () => FileReference.CreateFromFullPath(source.FullPath)),
                reference => IO.lift(() => Refused.Unless(
                    doc.InstanceDefinitions.ModifySourceArchive(definition.Index, reference, updateType, layerStyle, quiet),
                    nameof(InstanceDefinitionTable.ModifySourceArchive))))
            select bound);

    public static IO<Unit> DestroySourceArchive(RhinoDoc doc, ComponentRef key, bool quiet, RedrawPolicy redraw) =>
        Commits.Commit(doc, LOC.STR("Destroy block source archive"), redraw,
            Call(doc, key, includeDeleted: false, definition => doc.InstanceDefinitions.DestroySourceArchive(definition, quiet), nameof(InstanceDefinitionTable.DestroySourceArchive)));

    public static IO<Unit> RefreshLinkedBlock(RhinoDoc doc, ComponentRef key, RedrawPolicy redraw) =>
        Commits.Commit(doc, LOC.STR("Refresh linked block"), redraw,
            from definition in Require(doc, key, includeDeleted: false)
            from state in IO.lift(() => (Tenuous: definition.IsTenuous, definition.UpdateType))
            from firm in when(state.Tenuous, IO.fail<Unit>(new TenuousDefinition(key)))
            from linked in IO.lift(() => WrongUpdateType.Unless(key, state.UpdateType, Seq(InstanceDefinitionUpdateType.Linked, InstanceDefinitionUpdateType.LinkedAndEmbedded)))
            from refreshed in IO.lift(() => Refused.Unless(doc.InstanceDefinitions.RefreshLinkedBlock(definition), nameof(InstanceDefinitionTable.RefreshLinkedBlock)))
            select refreshed);

    public static IO<Unit> UpdateLinked(RhinoDoc doc, ComponentRef key, string filename, bool updateNestedLinks, bool quiet, RedrawPolicy redraw) =>
        Commits.Commit(doc, LOC.STR("Update linked block"), redraw, Call(
            doc,
            key,
            includeDeleted: false,
            definition => doc.InstanceDefinitions.UpdateLinkedInstanceDefinition(definition.Index, filename, updateNestedLinks, quiet),
            nameof(InstanceDefinitionTable.UpdateLinkedInstanceDefinition)));

    public static IO<Unit> SetLayerStyle(RhinoDoc doc, ComponentRef key, InstanceDefinitionLayerStyle style, RedrawPolicy redraw) =>
        Commits.Commit(doc, LOC.STR("Set block layer style"), redraw,
            from definition in Require(doc, key, includeDeleted: false)
            from updateType in IO.lift(() => definition.UpdateType)
            from linked in IO.lift(() => WrongUpdateType.Unless(key, updateType, Seq(InstanceDefinitionUpdateType.Linked)))
            from valid in when(style == InstanceDefinitionLayerStyle.None, IO.fail<Unit>(new InvalidLayerStyle(key, style)))
            from written in IO.lift(() => {
                definition.LayerStyle = style;
                return Mismatch.Unless(definition.LayerStyle == style, nameof(InstanceDefinition.LayerStyle));
            })
            select written);

    public static IO<Unit> SetSkipNested(RhinoDoc doc, ComponentRef key, bool skip, RedrawPolicy redraw) =>
        Commits.Commit(doc, LOC.STR("Set block skip nested"), redraw,
            Require(doc, key, includeDeleted: false).Bind(definition => IO.lift(() => {
                definition.SkipNestedLinkedDefinitions = skip;
                return Mismatch.Unless(definition.SkipNestedLinkedDefinitions == skip, nameof(InstanceDefinition.SkipNestedLinkedDefinitions));
            })));

    // --- [LIFECYCLE]
    public static IO<Unit> Delete(RhinoDoc doc, ComponentRef key, bool deleteReferences, bool quiet, RedrawPolicy redraw) =>
        Commits.Commit(doc, LOC.STR("Delete block"), redraw,
            Call(doc, key, includeDeleted: false, definition => doc.InstanceDefinitions.Delete(definition.Index, deleteReferences, quiet), nameof(InstanceDefinitionTable.Delete)));

    public static IO<Unit> Undelete(RhinoDoc doc, ComponentRef key, RedrawPolicy redraw) =>
        Commits.Commit(doc, LOC.STR("Undelete block"), redraw,
            Call(doc, key, includeDeleted: true, definition => doc.InstanceDefinitions.Undelete(definition.Index), nameof(InstanceDefinitionTable.Undelete)));

    public static IO<Unit> Purge(RhinoDoc doc, ComponentRef key) =>
        Call(doc, key, includeDeleted: true, definition => doc.InstanceDefinitions.Purge(definition.Index), nameof(InstanceDefinitionTable.Purge));

    public static IO<Unit> Export(RhinoDoc doc, ComponentRef key, string path) =>
        Call(doc, key, includeDeleted: false, definition => doc.InstanceDefinitions.Export(definition.Index, path), nameof(InstanceDefinitionTable.Export));

    private static IO<Unit> Call(RhinoDoc doc, ComponentRef key, bool includeDeleted, Func<InstanceDefinition, bool> call, string member) =>
        Require(doc, key, includeDeleted).Bind(definition => IO.lift(() => Refused.Unless(call(definition), member)));
}

[Mapper]
internal static partial class DefinitionMapper {
    [MapProperty(nameof(InstanceObject.InstanceXform), nameof(BlockPlacement.Xform))]
    internal static partial BlockPlacement ToPlacement(InstanceObject reference);

    [MapValue(nameof(DefinitionNode.MembersUnread), false)]
    internal static partial DefinitionNode ToNode(InstanceDefinition definition);
}
