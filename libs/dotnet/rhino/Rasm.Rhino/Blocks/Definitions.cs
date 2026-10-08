using System.Drawing;
using LanguageExt.ClassInstances;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Shapes;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using Rhino.DocObjects.Tables;
using Rhino.FileIO;

namespace Rasm.Rhino.Blocks;

// --- [TYPES] ---------------------------------------------------------------------------
public enum ReferenceScope { TopLevel = 0, TopLevelAndNested = 1, FromOtherDefinitions = 2 }

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Hyperlink(string Address, Option<string> Tag) {
    public Option<string> Tag { get; } = Tag.Bind(Conversions.Present);
}

public sealed record DefinitionMetadata(string Name, Option<string> Description, Option<Hyperlink> Link) {
    public Option<string> Description { get; } = Description.Bind(Conversions.Present);
    public Option<Hyperlink> Link { get; } = Link.Filter(static link => link.Address.Length > 0);
    internal string Url => Conversions.Unset(Link.Map(static link => link.Address));
    internal string UrlTag => Conversions.Unset(Link.Bind(static link => link.Tag));

    public static DefinitionMetadata Of(InstanceDefinitionGeometry definition) =>
        new(
            definition.Name,
            Optional(definition.Description),
            Optional(definition.Url).Map(address => new Hyperlink(address, Optional(definition.UrlDescription))));
}

public sealed record SourceReference(string FullPath, Option<string> RelativePath);

public sealed record LinkState(
    SourceReference Source,
    InstanceDefinitionUpdateType UpdateType,
    InstanceDefinitionArchiveFileStatus Status,
    InstanceDefinitionLayerStyle LayerStyle,
    bool SkipNested,
    bool IsTenuous);

public sealed record DefinitionUsage(int TopLevel, int Nested) {
    public int Total => TopLevel + Nested;
}

public sealed record DefinitionState(
    Guid Id,
    int Index,
    DefinitionMetadata Metadata,
    Option<LinkState> Link,
    UnitSystem UnitSystem,
    bool IsReference,
    Seq<Guid> Members,
    Seq<Guid> Containers,
    DefinitionUsage Usage,
    HashMap<EqStringOrdinalIgnoreCase, string, string> UserStrings) {
    public bool Hidden => Metadata.Name.StartsWith('*');

    public bool ClippingDrawing => UserStrings.ContainsKey("SectionTools");

    public static DefinitionState Of(InstanceDefinition definition) {
        _ = definition.UseCount(out int topLevel, out int nested);
        return new(
            definition.Id,
            definition.Index,
            DefinitionMetadata.Of(definition),
            definition.IsLinkedType
                ? new LinkState(
                    new SourceReference(definition.SourceArchive, Conversions.Present(definition.SourceArchiveRelativePath)),
                    definition.UpdateType,
                    definition.ArchiveFileStatus,
                    definition.LayerStyle,
                    definition.SkipNestedLinkedDefinitions,
                    definition.IsTenuous)
                : Option<LinkState>.None,
            definition.UnitSystem,
            definition.IsReference,
            Conversions.Rows(definition.GetObjectIds()),
            Conversions.Rows(definition.GetContainers()).Map(static container => container.Id).Strict(),
            new DefinitionUsage(topLevel, nested),
            Document.Tables.UserStrings.Held(definition.GetUserStrings()));
    }
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record PreviewMethod {
    public sealed record ByMode(DefinedViewportProjection Projection, DisplayMode DisplayMode, bool ApplyDpiScaling, Option<Guid> Selected) : PreviewMethod;

    public sealed record ByCamera(Guid DisplayModeId, DefinedViewportProjection Projection, IsometricCamera Camera, bool DrawDecorations, bool ApplyDpiScaling) : PreviewMethod;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record DefinitionEdit {
    internal abstract IO<Unit> Apply(RhinoDoc doc, InstanceDefinition definition);

    public sealed record Modify(DefinitionMetadata Metadata) : DefinitionEdit {
        internal override IO<Unit> Apply(RhinoDoc doc, InstanceDefinition definition) =>
            IO.lift(() => Refused.Unless(doc.InstanceDefinitions.Modify(
                definition.Index, Metadata.Name, Conversions.Unset(Metadata.Description), Metadata.Url, Metadata.UrlTag, quiet: true), nameof(InstanceDefinitionTable.Modify)));
    }

    public sealed record ModifyGeometry(Seq<GeometryPair> Members) : DefinitionEdit {
        internal override IO<Unit> Apply(RhinoDoc doc, InstanceDefinition definition) =>
            TableOps.WithAttributes(doc.CreateDefaultAttributes, Members, (geometry, attributes) => IO.lift(() => Refused.Unless(
                doc.InstanceDefinitions.ModifyGeometry(definition.Index, geometry, attributes), nameof(InstanceDefinitionTable.ModifyGeometry))));
    }

    public sealed record ModifyInsertionPlane(Plane Plane) : DefinitionEdit {
        internal override IO<Unit> Apply(RhinoDoc doc, InstanceDefinition definition) =>
            IO.lift(() => Refused.Unless(doc.InstanceDefinitions.ModifyInsertionPlane(definition.Index, Plane), nameof(InstanceDefinitionTable.ModifyInsertionPlane)));
    }

    public sealed record ModifyUserData(UserData Data) : DefinitionEdit {
        internal override IO<Unit> Apply(RhinoDoc doc, InstanceDefinition definition) =>
            IO.lift(() => Refused.Unless(doc.InstanceDefinitions.Modify(definition.Index, Data, quiet: true), nameof(InstanceDefinitionTable.Modify)));
    }

    public sealed record ModifySourceArchive(SourceReference Source, InstanceDefinitionUpdateType UpdateType, InstanceDefinitionLayerStyle LayerStyle) : DefinitionEdit {
        internal override IO<Unit> Apply(RhinoDoc doc, InstanceDefinition definition) =>
            (from path in IO.lift(() => Exchange.ExistingPath(Source.FullPath))
             from reference in use(IO.lift(() => Missing.Unless(
                 FileReference.CreateFromFullAndRelativePaths(path, Source.RelativePath.ValueUnsafe()), nameof(FileReference.CreateFromFullAndRelativePaths))))
             from modified in IO.lift(() => Refused.Unless(
                 doc.InstanceDefinitions.ModifySourceArchive(definition.Index, reference, UpdateType, LayerStyle, quiet: true), nameof(InstanceDefinitionTable.ModifySourceArchive)))
             select modified).Bracket();
    }

    public sealed record DestroySourceArchive : DefinitionEdit {
        internal override IO<Unit> Apply(RhinoDoc doc, InstanceDefinition definition) =>
            IO.lift(() => Refused.Unless(doc.InstanceDefinitions.DestroySourceArchive(definition, quiet: true), nameof(InstanceDefinitionTable.DestroySourceArchive)));
    }

    public sealed record RefreshLinkedBlock : DefinitionEdit {
        internal override IO<Unit> Apply(RhinoDoc doc, InstanceDefinition definition) =>
            IO.lift(() => Refused.Unless(doc.InstanceDefinitions.RefreshLinkedBlock(definition), nameof(InstanceDefinitionTable.RefreshLinkedBlock)));
    }

    public sealed record UpdateLinked(string Path, bool UpdateNestedLinks) : DefinitionEdit {
        internal override IO<Unit> Apply(RhinoDoc doc, InstanceDefinition definition) =>
            from path in IO.lift(() => Exchange.ExistingPath(Path))
            from modified in IO.lift(() => Refused.Unless(
                doc.InstanceDefinitions.UpdateLinkedInstanceDefinition(definition.Index, path, UpdateNestedLinks, quiet: true), nameof(InstanceDefinitionTable.UpdateLinkedInstanceDefinition)))
            select modified;
    }

    public sealed record Set(Action<InstanceDefinition> Write) : DefinitionEdit {
        internal override IO<Unit> Apply(RhinoDoc doc, InstanceDefinition definition) => IO.lift(() => Write(definition));
    }

    public sealed record Delete(bool DeleteReferences) : DefinitionEdit {
        internal override IO<Unit> Apply(RhinoDoc doc, InstanceDefinition definition) =>
            from containers in IO.lift(() => Conversions.Rows(definition.GetContainers()).Map(static container => container.Id).Strict())
            from free in guard<Error>(containers.IsEmpty, new DefinitionContained(definition.Id, containers))
            from deleted in IO.lift(() => Refused.Unless(
                doc.InstanceDefinitions.Delete(definition.Index, DeleteReferences, quiet: true), nameof(InstanceDefinitionTable.Delete)))
            select deleted;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record DefinitionOp {
    internal abstract IO<int> Apply(RhinoDoc doc);

    public sealed record Add(DefinitionMetadata Metadata, Point3d BasePoint, Seq<GeometryPair> Members, bool OverrideExisting) : DefinitionOp {
        internal override IO<int> Apply(RhinoDoc doc) =>
            TableOps.WithAttributes(doc.CreateDefaultAttributes, Members, (geometry, attributes) => IO.lift(() => Conversions.Required(
                doc.InstanceDefinitions.Add(Metadata.Name, Conversions.Unset(Metadata.Description), Metadata.Url, Metadata.UrlTag, BasePoint, geometry, attributes, OverrideExisting),
                nameof(InstanceDefinitionTable.Add))));
    }

    public sealed record CreateFromFile(
        string Path,
        DefinitionMetadata Metadata,
        InstanceDefinitionUpdateType UpdateType,
        InstanceDefinitionLayerStyle LayerStyle,
        InstanceDefinitionNameConflictResolution Conflict,
        bool SkipNestedLinkedDefinitions) : DefinitionOp {
        internal override IO<int> Apply(RhinoDoc doc) =>
            from path in IO.lift(() => Exchange.ExistingPath(Path))
            from count in IO.lift(() => doc.InstanceDefinitions.Count)
            from index in IO.lift(() => Conversions.Required(doc.InstanceDefinitions.CreateFromFile(
                path, Metadata.Name, Conversions.Unset(Metadata.Description), Metadata.Url, Metadata.UrlTag,
                UpdateType, LayerStyle, Conflict, SkipNestedLinkedDefinitions), nameof(InstanceDefinitionTable.CreateFromFile)))
            from row in TableOps.Find(doc.InstanceDefinitions, index, includeDeleted: false)
            from created in guard<Error>(index >= count || TableOps.Names<InstanceDefinition>().Equals(row.Name, Metadata.Name), new AlreadyLinked(path, row.Id))
            select index;
    }

    public sealed record Edit(ComponentRef<InstanceDefinition> Address, DefinitionEdit Change) : DefinitionOp {
        internal override IO<int> Apply(RhinoDoc doc) =>
            from definition in TableOps.Find(doc.InstanceDefinitions, Address, includeDeleted: false)
            from edited in Change.Apply(doc, definition)
            select definition.Index;
    }

    public sealed record Undelete(ComponentRef<InstanceDefinition> Address) : DefinitionOp {
        internal override IO<int> Apply(RhinoDoc doc) =>
            from definition in TableOps.Find(doc.InstanceDefinitions, Address, includeDeleted: true)
            from restored in IO.lift(() => Refused.Unless(doc.InstanceDefinitions.Undelete(definition.Index), nameof(InstanceDefinitionTable.Undelete)))
            select definition.Index;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Definitions {
    // --- [READS]
    public static IO<DefinitionState> State(RhinoDoc doc, ComponentRef<InstanceDefinition> address) =>
        TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: false).Map(DefinitionState.Of);

    public static IO<Option<int>> Nesting(RhinoDoc doc, ComponentRef<InstanceDefinition> outer, ComponentRef<InstanceDefinition> inner) =>
        from container in TableOps.Find(doc.InstanceDefinitions, outer, includeDeleted: false)
        from nested in TableOps.Find(doc.InstanceDefinitions, inner, includeDeleted: false)
        select container.Index == nested.Index ? Option<int>.None : Some(container.UsesDefinition(nested.Index)).Filter(static level => level > 0);

    public static IO<ContentKey> Stamp(RhinoDoc doc, ComponentRef<InstanceDefinition> address) =>
        from definition in TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: false)
        let metadata = DefinitionMetadata.Of(definition)
        select ContentKey.Of(KeyDomain.Definition, stream => stream
                .Text(metadata.Name)
                .Rows(metadata.Description.ToSeq(), static (row, text) => row.Text(text))
                .Rows(metadata.Link.ToSeq(), static (row, link) => row.Text(link.Address).Rows(link.Tag.ToSeq(), static (tag, text) => tag.Text(text)))
                .Integer((int)definition.UpdateType)
                .Integer((int)definition.LayerStyle)
                .Integer(Convert.ToInt32(definition.SkipNestedLinkedDefinitions))
                .Integer((int)definition.UnitSystem)
                .Integer(Measurements.Checksum(Conversions.Rows(definition.GetObjects()).Map(static member => member.Geometry).Strict()))
                .Rows(
                    definition.IsLinkedType ? toSeq(ContentHash.CreateFromFile(definition.SourceArchive).Sha1ContentHash) : Seq<byte>(),
                    static (row, value) => row.Integer((int)value)));

    // --- [PREVIEW]
    public static IO<TValue> Preview<TValue>(RhinoDoc doc, ComponentRef<InstanceDefinition> address, PreviewMethod method, Size pixels, Func<Bitmap, IO<TValue>> body) =>
        (from definition in TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: false)
         from bitmap in use(IO.lift(() => Missing.Unless(
                    method.Switch(
                        (Definition: definition, Pixels: pixels),
                        byMode: static (target, mode) =>
                            target.Definition.CreatePreviewBitmap(Conversions.Unset(mode.Selected), mode.Projection, mode.DisplayMode, target.Pixels, mode.ApplyDpiScaling),
                        byCamera: static (target, camera) =>
                            target.Definition.CreatePreviewBitmap(camera.DisplayModeId, camera.Projection, camera.Camera, camera.DrawDecorations, target.Pixels, camera.ApplyDpiScaling)),
                    nameof(InstanceDefinition.CreatePreviewBitmap))))
         from result in body(bitmap)
         select result).Bracket();

    // --- [WRITES]
    public static IO<Committed<Seq<int>>> Commit(RhinoDoc doc, string name, RedrawPolicy redraw, Seq<DefinitionOp> ops) =>
        Commits.Commit(doc, name, redraw, ops.TraverseM(op => op.Apply(doc)).As());

    // --- [TABLE]
    public static IO<Unit> Purge(RhinoDoc doc, ComponentRef<InstanceDefinition> address) =>
        from definition in TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: true)
        from purged in IO.lift(() => Refused.Unless(doc.InstanceDefinitions.Purge(definition.Index), nameof(InstanceDefinitionTable.Purge)))
        select purged;

    public static IO<Unit> Export(RhinoDoc doc, ComponentRef<InstanceDefinition> address, string path) =>
        from target in IO.lift(() => Exchange.QualifiedPath(path))
        from definition in TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: false)
        from written in IO.lift(() => Refused.Unless(doc.InstanceDefinitions.Export(definition.Index, target), nameof(InstanceDefinitionTable.Export)))
        select written;

    public static IO<Unit> UndoModify(RhinoDoc doc, ComponentRef<InstanceDefinition> address) =>
        from definition in TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: false)
        from restored in IO.lift(() => Refused.Unless(doc.InstanceDefinitions.UndoModify(definition.Index), nameof(InstanceDefinitionTable.UndoModify)))
        select restored;

    // --- [ARCHIVE]
    public static IO<int> Add(File3dm archive, DefinitionMetadata metadata, Point3d basePoint, Seq<GeometryPair> members) =>
        TableOps.WithAttributes(static () => new ObjectAttributes(), members, (geometry, attributes) => IO.lift(() => Conversions.Required(
            archive.AllInstanceDefinitions.Add(metadata.Name, Conversions.Unset(metadata.Description), metadata.Url, metadata.UrlTag, basePoint, geometry, attributes),
            nameof(File3dmInstanceDefinitionTable.Add))));

    public static IO<int> AddLinked(File3dm archive, string path, string name, Option<string> description) =>
        from target in IO.lift(() => Exchange.QualifiedPath(path))
        from index in IO.lift(() => Conversions.Required(
            archive.AllInstanceDefinitions.AddLinked(target, name, Conversions.Unset(description)),
            nameof(File3dmInstanceDefinitionTable.AddLinked)))
        select index;
}
