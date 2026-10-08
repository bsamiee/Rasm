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

    public static DefinitionMetadata Of(InstanceDefinitionGeometry definition) =>
        new(
            definition.Name,
            Conversions.Present(definition.Description),
            Conversions.Present(definition.Url).Map(address => new Hyperlink(address, Conversions.Present(definition.UrlDescription))));
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

    public static DefinitionUsage Of(InstanceDefinition definition) {
        _ = definition.UseCount(out int topLevel, out int nested);
        return new DefinitionUsage(topLevel, nested);
    }
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

    public static DefinitionState Of(InstanceDefinition definition) =>
        new(
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
            DefinitionUsage.Of(definition),
            Document.Tables.UserStrings.Held(definition.GetUserStrings()));
}

[Union]
public abstract partial record PreviewMethod {
    public sealed record ByMode(DefinedViewportProjection Projection, DisplayMode DisplayMode, bool ApplyDpiScaling, Option<Guid> Selected) : PreviewMethod;

    public sealed record ByCamera(Guid DisplayModeId, DefinedViewportProjection Projection, IsometricCamera Camera, bool DrawDecorations, bool ApplyDpiScaling) : PreviewMethod;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DefinitionEdit {
    public sealed record Modify(DefinitionMetadata Metadata) : DefinitionEdit;

    public sealed record ModifyGeometry(Seq<GeometryPair> Members) : DefinitionEdit;

    public sealed record ModifyInsertionPlane(Plane Plane) : DefinitionEdit;

    public sealed record ModifyUserData(UserData Data) : DefinitionEdit;

    public sealed record ModifySourceArchive(SourceReference Source, InstanceDefinitionUpdateType UpdateType, InstanceDefinitionLayerStyle LayerStyle) : DefinitionEdit;

    public sealed record DestroySourceArchive() : DefinitionEdit;

    public sealed record RefreshLinkedBlock() : DefinitionEdit;

    public sealed record UpdateLinked(string Path, bool UpdateNestedLinks) : DefinitionEdit;

    public sealed record Set(Action<InstanceDefinition> Write) : DefinitionEdit;

    public sealed record Delete(bool DeleteReferences) : DefinitionEdit;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DefinitionOp {
    public sealed record Add(DefinitionMetadata Metadata, Point3d BasePoint, Seq<GeometryPair> Members, bool OverrideExisting) : DefinitionOp;

    public sealed record CreateFromFile(
        string Path,
        DefinitionMetadata Metadata,
        InstanceDefinitionUpdateType UpdateType,
        InstanceDefinitionLayerStyle LayerStyle,
        InstanceDefinitionNameConflictResolution Conflict,
        bool SkipNestedLinkedDefinitions) : DefinitionOp;

    public sealed record Edit(ComponentRef<InstanceDefinition> Address, DefinitionEdit Change) : DefinitionOp;

    public sealed record Undelete(ComponentRef<InstanceDefinition> Address) : DefinitionOp;
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
        TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: false).Map(static definition => DefinitionMetadata.Of(definition) switch {
            var metadata => ContentKey.Of(KeyDomain.Definition, stream => stream
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
                    static (row, value) => row.Integer<int>(value))),
        });

    // --- [PREVIEW]
    public static IO<TValue> Preview<TValue>(RhinoDoc doc, ComponentRef<InstanceDefinition> address, PreviewMethod method, Size pixels, Func<Bitmap, IO<TValue>> body) =>
        TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: false).Bind(definition =>
            use(IO.lift(() => Missing.Unless(
                    method.Switch(
                        (Definition: definition, Pixels: pixels),
                        byMode: static (target, mode) =>
                            target.Definition.CreatePreviewBitmap(Conversions.Unset(mode.Selected), mode.Projection, mode.DisplayMode, target.Pixels, mode.ApplyDpiScaling),
                        byCamera: static (target, camera) =>
                            target.Definition.CreatePreviewBitmap(camera.DisplayModeId, camera.Projection, camera.Camera, camera.DrawDecorations, target.Pixels, camera.ApplyDpiScaling)),
                    nameof(InstanceDefinition.CreatePreviewBitmap))))
                .Bind(body)
                .Bracket());

    // --- [WRITES]
    public static IO<Committed<Seq<int>>> Commit(RhinoDoc doc, string name, RedrawPolicy redraw, Seq<DefinitionOp> ops) =>
        Commits.Commit(doc, name, redraw, ops.TraverseM(op => Apply(doc, op)).As());

    internal static IO<int> Apply(RhinoDoc doc, DefinitionOp op) =>
        op.Switch(
            doc,
            add: static (document, add) => TableOps.WithAttributes(document.CreateDefaultAttributes, add.Members, (geometry, attributes) => IO.lift(() => Conversions.Required(
                document.InstanceDefinitions.Add(
                    add.Metadata.Name, Conversions.Unset(add.Metadata.Description), Url(add.Metadata), UrlTag(add.Metadata), add.BasePoint, geometry, attributes, add.OverrideExisting),
                nameof(InstanceDefinitionTable.Add)))),
            createFromFile: static (document, create) =>
                from path in IO.lift(() => Exchange.ExistingPath(create.Path))
                from count in IO.lift(() => document.InstanceDefinitions.Count)
                from index in IO.lift(() => Conversions.Required(
                    document.InstanceDefinitions.CreateFromFile(
                        path, create.Metadata.Name, Conversions.Unset(create.Metadata.Description), Url(create.Metadata), UrlTag(create.Metadata),
                        create.UpdateType, create.LayerStyle, create.Conflict, create.SkipNestedLinkedDefinitions),
                    nameof(InstanceDefinitionTable.CreateFromFile)))
                from row in TableOps.Find(document.InstanceDefinitions, index, includeDeleted: false)
                from created in IO.lift(() => AlreadyLinked.Unless(index >= count || TableOps.Names<InstanceDefinition>().Equals(row.Name, create.Metadata.Name), path, row.Id))
                select index,
            edit: static (document, edit) =>
                from definition in TableOps.Find(document.InstanceDefinitions, edit.Address, includeDeleted: false)
                from edited in Edited(document, definition, edit.Change)
                select definition.Index,
            undelete: static (document, undelete) =>
                from definition in TableOps.Find(document.InstanceDefinitions, undelete.Address, includeDeleted: true)
                from restored in IO.lift(() => Refused.Unless(document.InstanceDefinitions.Undelete(definition.Index), nameof(InstanceDefinitionTable.Undelete)))
                select definition.Index);

    private static IO<Unit> Edited(RhinoDoc doc, InstanceDefinition definition, DefinitionEdit change) =>
        change.Switch(
            (Doc: doc, Definition: definition),
            modify: static (state, modify) => IO.lift(() => Refused.Unless(
                state.Doc.InstanceDefinitions.Modify(
                    state.Definition.Index, modify.Metadata.Name, Conversions.Unset(modify.Metadata.Description), Url(modify.Metadata), UrlTag(modify.Metadata), quiet: true),
                nameof(InstanceDefinitionTable.Modify))),
            modifyGeometry: static (state, modify) => TableOps.WithAttributes(state.Doc.CreateDefaultAttributes, modify.Members, (geometry, attributes) => IO.lift(() => Refused.Unless(
                state.Doc.InstanceDefinitions.ModifyGeometry(state.Definition.Index, geometry, attributes),
                nameof(InstanceDefinitionTable.ModifyGeometry)))),
            modifyInsertionPlane: static (state, modify) => IO.lift(() => Refused.Unless(
                state.Doc.InstanceDefinitions.ModifyInsertionPlane(state.Definition.Index, modify.Plane),
                nameof(InstanceDefinitionTable.ModifyInsertionPlane))),
            modifyUserData: static (state, modify) => IO.lift(() => Refused.Unless(
                state.Doc.InstanceDefinitions.Modify(state.Definition.Index, modify.Data, quiet: true),
                nameof(InstanceDefinitionTable.Modify))),
            modifySourceArchive: static (state, modify) =>
                from path in IO.lift(() => Exchange.ExistingPath(modify.Source.FullPath))
                from landed in use(IO.lift(() => Missing.Unless(
                        FileReference.CreateFromFullAndRelativePaths(path, modify.Source.RelativePath.ValueUnsafe()),
                        nameof(FileReference.CreateFromFullAndRelativePaths))))
                    .Bind(reference => IO.lift(() => Refused.Unless(
                        state.Doc.InstanceDefinitions.ModifySourceArchive(state.Definition.Index, reference, modify.UpdateType, modify.LayerStyle, quiet: true),
                        nameof(InstanceDefinitionTable.ModifySourceArchive))))
                    .Bracket()
                select landed,
            destroySourceArchive: static (state, _) => IO.lift(() => Refused.Unless(
                state.Doc.InstanceDefinitions.DestroySourceArchive(state.Definition, quiet: true),
                nameof(InstanceDefinitionTable.DestroySourceArchive))),
            refreshLinkedBlock: static (state, _) => IO.lift(() => Refused.Unless(
                state.Doc.InstanceDefinitions.RefreshLinkedBlock(state.Definition),
                nameof(InstanceDefinitionTable.RefreshLinkedBlock))),
            updateLinked: static (state, update) =>
                from path in IO.lift(() => Exchange.ExistingPath(update.Path))
                from landed in IO.lift(() => Refused.Unless(
                    state.Doc.InstanceDefinitions.UpdateLinkedInstanceDefinition(state.Definition.Index, path, update.UpdateNestedLinks, quiet: true),
                    nameof(InstanceDefinitionTable.UpdateLinkedInstanceDefinition)))
                select landed,
            set: static (state, set) => IO.lift(() => set.Write(state.Definition)),
            delete: static (state, delete) =>
                from free in IO.lift(() => DefinitionContained.Unless(state.Definition.Id, Conversions.Rows(state.Definition.GetContainers()).Map(static container => container.Id).Strict()))
                from deleted in IO.lift(() => Refused.Unless(
                    state.Doc.InstanceDefinitions.Delete(state.Definition.Index, delete.DeleteReferences, quiet: true),
                    nameof(InstanceDefinitionTable.Delete)))
                select deleted);

    private static string Url(DefinitionMetadata metadata) =>
        Conversions.Unset(metadata.Link.Map(static link => link.Address));

    private static string UrlTag(DefinitionMetadata metadata) =>
        Conversions.Unset(metadata.Link.Bind(static link => link.Tag));

    // --- [TABLE]
    public static IO<Unit> Purge(RhinoDoc doc, ComponentRef<InstanceDefinition> address) =>
        TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: true).Bind(definition =>
            IO.lift(() => Refused.Unless(doc.InstanceDefinitions.Purge(definition.Index), nameof(InstanceDefinitionTable.Purge))));

    public static IO<Unit> Export(RhinoDoc doc, ComponentRef<InstanceDefinition> address, string path) =>
        from target in IO.lift(() => Exchange.QualifiedPath(path))
        from definition in TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: false)
        from written in IO.lift(() => Refused.Unless(doc.InstanceDefinitions.Export(definition.Index, target), nameof(InstanceDefinitionTable.Export)))
        select written;

    public static IO<Unit> UndoModify(RhinoDoc doc, ComponentRef<InstanceDefinition> address) =>
        TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: false).Bind(definition =>
            IO.lift(() => Refused.Unless(doc.InstanceDefinitions.UndoModify(definition.Index), nameof(InstanceDefinitionTable.UndoModify))));

    // --- [ARCHIVE]
    public static IO<int> Add(File3dm archive, DefinitionMetadata metadata, Point3d basePoint, Seq<GeometryPair> members) =>
        TableOps.WithAttributes(static () => new ObjectAttributes(), members, (geometry, attributes) => IO.lift(() => Conversions.Required(
            archive.AllInstanceDefinitions.Add(metadata.Name, Conversions.Unset(metadata.Description), Url(metadata), UrlTag(metadata), basePoint, geometry, attributes),
            nameof(File3dmInstanceDefinitionTable.Add))));

    public static IO<int> AddLinked(File3dm archive, string path, string name, Option<string> description) =>
        from target in IO.lift(() => Exchange.QualifiedPath(path))
        from index in IO.lift(() => Conversions.Required(
            archive.AllInstanceDefinitions.AddLinked(target, name, Conversions.Unset(description)),
            nameof(File3dmInstanceDefinitionTable.AddLinked)))
        select index;
}
