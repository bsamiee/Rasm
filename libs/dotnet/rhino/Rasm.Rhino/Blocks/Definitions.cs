using System.Drawing;
using LanguageExt.ClassInstances;
using Rasm.RhinoCommon.Document;
using Rasm.RhinoCommon.Document.Files;
using Rasm.RhinoCommon.Document.Tables;
using Rasm.RhinoCommon.Modeling;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Custom;
using Rhino.FileIO;

namespace Rasm.RhinoCommon.Blocks;

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
            Persistence.UserStrings.Held(definition.GetUserStrings()));
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
            IO.lift(() => RhinoFault.Unless(doc.InstanceDefinitions.Modify(
                definition.Index, Metadata.Name, Conversions.Unset(Metadata.Description), Metadata.Url, Metadata.UrlTag, quiet: true)));
    }

    public sealed record ModifyGeometry(Seq<GeometryPair> Members) : DefinitionEdit {
        internal override IO<Unit> Apply(RhinoDoc doc, InstanceDefinition definition) =>
            TableOps.WithAttributes(doc.CreateDefaultAttributes, Members, (geometry, attributes) => IO.lift(() => RhinoFault.Unless(
                doc.InstanceDefinitions.ModifyGeometry(definition.Index, geometry, attributes))));
    }

    public sealed record ModifyInsertionPlane(Plane Plane) : DefinitionEdit {
        internal override IO<Unit> Apply(RhinoDoc doc, InstanceDefinition definition) =>
            IO.lift(() => RhinoFault.Unless(doc.InstanceDefinitions.ModifyInsertionPlane(definition.Index, Plane)));
    }

    public sealed record ModifyUserData(UserData Data) : DefinitionEdit {
        internal override IO<Unit> Apply(RhinoDoc doc, InstanceDefinition definition) =>
            IO.lift(() => RhinoFault.Unless(doc.InstanceDefinitions.Modify(definition.Index, Data, quiet: true)));
    }

    public sealed record ModifySourceArchive(SourceReference Source, InstanceDefinitionUpdateType UpdateType, InstanceDefinitionLayerStyle LayerStyle) : DefinitionEdit {
        internal override IO<Unit> Apply(RhinoDoc doc, InstanceDefinition definition) =>
            (from path in IO.lift(() => InputPath.Of(Source.FullPath))
             from reference in Products.Made(FileReference.CreateFromFullAndRelativePaths(path, Source.RelativePath.ValueUnsafe()))
             from modified in IO.lift(() => RhinoFault.Unless(doc.InstanceDefinitions.ModifySourceArchive(definition.Index, reference, UpdateType, LayerStyle, quiet: true)))
             select modified).Bracket();
    }

    public sealed record DestroySourceArchive : DefinitionEdit {
        internal override IO<Unit> Apply(RhinoDoc doc, InstanceDefinition definition) =>
            IO.lift(() => RhinoFault.Unless(doc.InstanceDefinitions.DestroySourceArchive(definition, quiet: true)));
    }

    public sealed record RefreshLinkedBlock : DefinitionEdit {
        internal override IO<Unit> Apply(RhinoDoc doc, InstanceDefinition definition) =>
            IO.lift(() => RhinoFault.Unless(doc.InstanceDefinitions.RefreshLinkedBlock(definition)));
    }

    public sealed record UpdateLinked(InputPath Path, bool UpdateNestedLinks) : DefinitionEdit {
        internal override IO<Unit> Apply(RhinoDoc doc, InstanceDefinition definition) =>
            IO.lift(() => RhinoFault.Unless(doc.InstanceDefinitions.UpdateLinkedInstanceDefinition(definition.Index, Path, UpdateNestedLinks, quiet: true)));
    }

    public sealed record Set(Action<InstanceDefinition> Write) : DefinitionEdit {
        internal override IO<Unit> Apply(RhinoDoc doc, InstanceDefinition definition) => IO.lift(() => Write(definition));
    }

    public sealed record Delete(bool DeleteReferences) : DefinitionEdit {
        internal override IO<Unit> Apply(RhinoDoc doc, InstanceDefinition definition) =>
            IO.lift(() => RhinoFault.Unless(doc.InstanceDefinitions.Delete(definition.Index, DeleteReferences, quiet: true)));
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record DefinitionOp {
    internal abstract IO<int> Apply(RhinoDoc doc);

    public sealed record Add(DefinitionMetadata Metadata, Point3d BasePoint, Seq<GeometryPair> Members, bool OverrideExisting) : DefinitionOp {
        internal override IO<int> Apply(RhinoDoc doc) =>
            TableOps.WithAttributes(doc.CreateDefaultAttributes, Members, (geometry, attributes) => IO.lift(() => Conversions.Found(Conversions.Present(
                doc.InstanceDefinitions.Add(Metadata.Name, Conversions.Unset(Metadata.Description), Metadata.Url, Metadata.UrlTag, BasePoint, geometry, attributes, OverrideExisting)))));
    }

    public sealed record CreateFromFile(
        InputPath Path,
        DefinitionMetadata Metadata,
        InstanceDefinitionUpdateType UpdateType,
        InstanceDefinitionLayerStyle LayerStyle,
        InstanceDefinitionNameConflictResolution Conflict,
        bool SkipNestedLinkedDefinitions) : DefinitionOp {
        internal override IO<int> Apply(RhinoDoc doc) =>
            IO.lift(() => Conversions.Found(Conversions.Present(doc.InstanceDefinitions.CreateFromFile(
                Path, Metadata.Name, Conversions.Unset(Metadata.Description), Metadata.Url, Metadata.UrlTag,
                UpdateType, LayerStyle, Conflict, SkipNestedLinkedDefinitions))));
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
            from restored in IO.lift(() => RhinoFault.Unless(doc.InstanceDefinitions.Undelete(definition.Index)))
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
        select ContentKey.Of<InstanceDefinition>(stream => stream
                .Text(metadata.Name)
                .Rows(metadata.Description.ToSeq(), static (row, text) => row.Text(text))
                .Rows(metadata.Link.ToSeq(), static (row, link) => row.Text(link.Address).Rows(link.Tag.ToSeq(), static (tag, text) => tag.Text(text)))
                .Integer((int)definition.UpdateType)
                .Integer((int)definition.LayerStyle)
                .Integer(Convert.ToInt32(definition.SkipNestedLinkedDefinitions))
                .Integer((int)definition.UnitSystem)
                .Integer(Measurements.Checksum(Conversions.Rows(definition.GetObjects()).Map(static member => member.Geometry).Strict()))
                .Bytes(definition.IsLinkedType ? ContentHash.CreateFromFile(definition.SourceArchive).Sha1ContentHash : []));

    // --- [PREVIEW]
    public static IO<TValue> Preview<TValue>(RhinoDoc doc, ComponentRef<InstanceDefinition> address, PreviewMethod method, Size pixels, Func<Bitmap, IO<TValue>> body) =>
        from definition in TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: false)
        let bitmap = method.Switch(
            (Definition: definition, Pixels: pixels),
            byMode: static (target, mode) =>
                target.Definition.CreatePreviewBitmap(Conversions.Unset(mode.Selected), mode.Projection, mode.DisplayMode, target.Pixels, mode.ApplyDpiScaling),
            byCamera: static (target, camera) =>
                target.Definition.CreatePreviewBitmap(camera.DisplayModeId, camera.Projection, camera.Camera, camera.DrawDecorations, target.Pixels, camera.ApplyDpiScaling))
        from result in Products.Made(bitmap).Bind(body).Bracket()
        select result;

    // --- [WRITES]
    public static IO<Committed<Seq<int>>> Commit(RhinoDoc doc, string name, RedrawPolicy redraw, Seq<DefinitionOp> ops) =>
        Commits.Commit(doc, name, redraw, ops.TraverseM(op => op.Apply(doc)).As());

    // --- [TABLE]
    public static IO<Unit> Purge(RhinoDoc doc, ComponentRef<InstanceDefinition> address) =>
        from definition in TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: true)
        from purged in IO.lift(() => RhinoFault.Unless(doc.InstanceDefinitions.Purge(definition.Index)))
        select purged;

    public static IO<Unit> Export(RhinoDoc doc, ComponentRef<InstanceDefinition> address, OutputPath path) =>
        from definition in TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: false)
        from written in IO.lift(() => RhinoFault.Unless(doc.InstanceDefinitions.Export(definition.Index, path)))
        select written;

    public static IO<Unit> UndoModify(RhinoDoc doc, ComponentRef<InstanceDefinition> address) =>
        from definition in TableOps.Find(doc.InstanceDefinitions, address, includeDeleted: false)
        from restored in IO.lift(() => RhinoFault.Unless(doc.InstanceDefinitions.UndoModify(definition.Index)))
        select restored;

    // --- [ARCHIVE]
    public static IO<int> Add(File3dm archive, DefinitionMetadata metadata, Point3d basePoint, Seq<GeometryPair> members) =>
        TableOps.WithAttributes(static () => new ObjectAttributes(), members, (geometry, attributes) => IO.lift(() => Conversions.Found(Conversions.Present(
            archive.AllInstanceDefinitions.Add(metadata.Name, Conversions.Unset(metadata.Description), metadata.Url, metadata.UrlTag, basePoint, geometry, attributes)))));

    public static IO<int> AddLinked(File3dm archive, InputPath path, string name, Option<string> description) =>
        IO.lift(() => Conversions.Found(Conversions.Present(archive.AllInstanceDefinitions.AddLinked(path, name, Conversions.Unset(description)))));
}
