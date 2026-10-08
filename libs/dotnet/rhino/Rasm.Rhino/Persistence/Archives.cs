using QuikGraph;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Geolocation;
using Rasm.Rhino.Document.Tables;
using Rhino.DocObjects;
using Rhino.FileIO;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------
[Union(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ArchiveSource {
    public sealed record Located(string Path, File3dm.TableTypeFilter Tables, File3dm.ObjectTypeFilter Objects) : ArchiveSource {
        internal override IO<ArchiveRead> Open() =>
            from path in IO.lift(() => Exchange.ExistingPath(Path))
            from read in IO.lift(() => ArchiveRejected.Unless(File3dm.ReadWithLog(path, Tables, Objects, out string log), nameof(File3dm.ReadWithLog), log)
                .Map(model => new ArchiveRead(model, Conversions.Present(log))))
            select read;
    }

    public sealed record Buffered(Arr<byte> Bytes) : ArchiveSource {
        internal override IO<ArchiveRead> Open() =>
            IO.lift(() => Missing.Unless(File3dm.FromByteArray([.. Bytes]), nameof(File3dm.FromByteArray)).Map(static model => new ArchiveRead(model, None)));
    }

    internal abstract IO<ArchiveRead> Open();
}

public sealed record ArchiveRead(File3dm Model, Option<string> Log);

public sealed record ArchiveHeader(
    int ArchiveVersion,
    Option<string> Notes,
    Option<string> CreatedBy,
    Option<string> LastEditedBy,
    int Revision,
    Option<LocalDateTime> Created,
    Option<LocalDateTime> LastEdited,
    Option<string> ApplicationName,
    Option<string> ApplicationUrl,
    Option<string> ApplicationDetails);

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class RelationKind {
    public static readonly RelationKind OnLayer = new(ModelComponentType.Layer, static archive =>
        Conversions.Rows(archive.Objects).SelectMany(static item => Conversions.Present(item.Attributes.LayerIndex).ToSeq(), static (item, index) => (item.Id, Left<int, Guid>(index))));
    public static readonly RelationKind UsesMaterial = new(ModelComponentType.Material, static archive =>
        Conversions.Rows(archive.Objects).Filter(static item => item.Attributes.MaterialSource == ObjectMaterialSource.MaterialFromObject)
            .SelectMany(static item => Conversions.Present(item.Attributes.MaterialIndex).ToSeq(), static (item, index) => (item.Id, Left<int, Guid>(index))));
    public static readonly RelationKind InGroup = new(ModelComponentType.Group, static archive =>
        Conversions.Rows(archive.Objects).SelectMany(static item => Conversions.Rows(item.Attributes.GetGroupList()), static (item, index) => (item.Id, Left<int, Guid>(index))));
    public static readonly RelationKind MemberOf = new(ModelComponentType.InstanceDefinition, static archive =>
        Conversions.Rows(archive.AllInstanceDefinitions).SelectMany(static definition => Conversions.Rows(definition.GetObjectIds()), static (definition, member) => (member, Right<int, Guid>(definition.Id))));
    public static readonly RelationKind InstanceOf = new(ModelComponentType.InstanceDefinition, static archive =>
        Conversions.Rows(archive.Objects).SelectMany(static item => Optional(item.Geometry as InstanceReferenceGeometry).ToSeq(), static (item, placed) => (item.Id, Right<int, Guid>(placed.ParentIdefId))));
    public static readonly RelationKind ChildOf = new(ModelComponentType.Layer, static archive =>
        Conversions.Rows(archive.AllLayers).SelectMany(static layer => Conversions.Present(layer.ParentLayerId).ToSeq(), static (layer, parent) => (layer.Id, Right<int, Guid>(parent))));
    public static readonly RelationKind LayerMaterial = new(ModelComponentType.Material, static archive =>
        Conversions.Rows(archive.AllLayers).SelectMany(static layer => Conversions.Present(layer.RenderMaterialIndex).ToSeq(), static (layer, index) => (layer.Id, Left<int, Guid>(index))));

    internal ModelComponentType Target { get; }
    internal Func<File3dm, Seq<(Guid Source, Either<int, Guid> Reference)>> Read { get; }
}

public readonly record struct ComponentRelation(ComponentIdentity Source, ComponentIdentity Target, RelationKind Kind) : IEdge<ComponentIdentity>;

public sealed record DanglingLink(Guid Source, RelationKind Kind, Either<int, Guid> Reference);

public sealed record ArchiveGraph(ArrayBidirectionalGraph<ComponentIdentity, ComponentRelation> Components, Seq<DanglingLink> Dangling, HashMap<Guid, string> Sources) {
    public Seq<ComponentIdentity> Unreferenced =>
        toSeq(Components.Vertices).Filter(vertex => Components.IsInEdgesEmpty(vertex) && toSeq(RelationKind.Items).Exists(kind => kind.Target == vertex.Type)).Strict();

    public HashMap<RelationKind, int> Coverage =>
        toHashMap(Components.Edges.CountBy(static edge => edge.Kind).Select(static count => (count.Key, count.Value)));
}

public sealed record ObjectFinding(Guid Id, Option<string> Log);

public sealed record ArchiveDiff(
    LanguageExt.HashSet<ComponentIdentity> Added,
    LanguageExt.HashSet<ComponentIdentity> Removed,
    LanguageExt.HashSet<ComponentRelation> Linked,
    LanguageExt.HashSet<ComponentRelation> Unlinked) {
    public static ArchiveDiff Of(ArchiveGraph before, ArchiveGraph after) =>
        (toHashSet(before.Components.Vertices), toHashSet(after.Components.Vertices), toHashSet(before.Components.Edges), toHashSet(after.Components.Edges)) switch {
            var (was, now, wasLinked, nowLinked) => new(now.Except(was), was.Except(now), nowLinked.Except(wasLinked), wasLinked.Except(nowLinked)),
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
public static partial class Archives {
    // --- [SCOPES]
    public static IO<T> Read<T>(ArchiveSource source, Func<ArchiveRead, IO<T>> body) =>
        use(source.Open(), static read => read.Model.Dispose()).Bind(body).Bracket();

    // --- [READS]
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(new[] { nameof(File3dm.Notes), nameof(File3dmNotes.Notes) }, nameof(ArchiveHeader.Notes))]
    public static partial ArchiveHeader Header(File3dm archive);

    public static IO<EarthAnchor> Anchor(File3dm archive) =>
        (from point in use(() => archive.EarthAnchorPoint)
         from anchor in IO.lift(() => EarthAnchor.Of(point))
         select anchor).Bracket();

    public static IO<ArchiveGraph> Graph(File3dm archive) =>
        from read in IO.lift(() => (
            Vertices: Seq<IEnumerable<ModelComponent>>(archive.AllLayers, archive.AllMaterials, archive.AllGroups, archive.AllInstanceDefinitions, archive.Objects)
                .Bind(static table => Conversions.Rows(table)).Filter(static component => !component.IsSystemComponent)
                .Map(static component => (Identity: new ComponentIdentity(component.Id, component.ComponentType), component.Index)).Strict(),
            Links: toSeq(RelationKind.Items).SelectMany(kind => kind.Read(archive), static (kind, reference) => new DanglingLink(reference.Source, kind, reference.Reference)).Strict(),
            Sources: toHashMap(Conversions.Rows(archive.AllInstanceDefinitions).Filter(static definition => definition.IsLinkedType).Map(static definition => (definition.Id, definition.SourceArchive)))))
        let byId = toHashMap(read.Vertices.Map(static vertex => (vertex.Identity.Id, vertex.Identity)))
        let byIndex = toHashMap(read.Vertices.Map(static vertex => ((vertex.Identity.Type, vertex.Index), vertex.Identity)))
        let resolved = (from link in read.Links
                        let edge = from source in byId.Find(link.Source)
                                   from target in link.Reference.Match(Left: index => byIndex.Find((link.Kind.Target, index)), Right: byId.Find)
                                   select new ComponentRelation(source, target, link.Kind)
                        select edge.ToEither(link)).PartitionSequence()
        let outEdges = resolved.Rights.ToLookup(static edge => edge.Source)
        select new ArchiveGraph(
            read.Vertices.Map(static vertex => vertex.Identity).ToBidirectionalGraph(vertex => outEdges[vertex], allowParallelEdges: true).ToArrayBidirectionalGraph(),
            resolved.Lefts, read.Sources);

    public static IO<Seq<ObjectFinding>> Verify(File3dm archive) =>
        IO.lift(() => Conversions.Rows(archive.Objects).Choose(static item => item.Geometry switch {
            null => Some(new ObjectFinding(item.Id, None)),
            var geometry => geometry.IsValidWithLog(out string log) ? None : Some(new ObjectFinding(item.Id, Some(log))),
        }).Strict());

    // --- [WRITES]
    public static IO<Option<string>> Write(File3dm archive, DocumentKey key, Option<string> value) =>
        IO.lift(() => Conversions.Present(archive.Strings.SetString(key.Text, Conversions.Unset(value))));

    public static IO<Unit> Write(File3dm archive, EarthAnchor anchor) =>
        (from point in use(() => archive.EarthAnchorPoint)
         from applied in anchor.Apply(point)
         from committed in IO.lift(() => { archive.EarthAnchorPoint = point; })
         select committed).Bracket();

    public static IO<Unit> Embed(File3dm archive, string path) =>
        from existing in IO.lift(() => Exchange.ExistingPath(path))
        from added in IO.lift(() => Refused.Unless(archive.EmbeddedFiles.Add(existing), nameof(File3dmEmbeddedFiles.Add)))
        select added;

    public static IO<Seq<string>> Extract(File3dm archive, string folder) =>
        from target in IO.lift(() => Exchange.QualifiedPath(folder))
        let candidates = from file in Conversions.Rows(archive.EmbeddedFiles)
                         select (File: file, Path: Path.Combine(target, Path.GetFileName(file.Filename.Replace('\\', Path.AltDirectorySeparatorChar))))
        from rows in IO.lift(() => Callbacks.Unique(candidates.Strict(), static row => row.Path, nameof(File3dmEmbeddedFile.SaveToFile), StringComparer.OrdinalIgnoreCase).ToFin())
        from saved in IO.lift(() => Callbacks.Each(rows, static row => row.File.SaveToFile(row.Path), nameof(File3dmEmbeddedFile.SaveToFile)))
        select rows.Map(static row => row.Path);

    public static IO<StepOutcome> Persist(File3dm archive, string path, File3dmWriteOptions options) =>
        from qualified in IO.lift(() => Exchange.QualifiedPath(path))
        from outcome in StepOutcome.Land(qualified, target => IO.lift(() =>
            ArchiveRejected.Unless(archive.WriteWithLog(target, options, out string log), nameof(File3dm.WriteWithLog), log)))
        select outcome;
}
