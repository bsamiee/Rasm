using QuikGraph;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Geolocation;
using Rasm.Rhino.Document.Tables;
using Rhino.DocObjects;
using Rhino.FileIO;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Persistence;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record ArchiveSource {
    public sealed record Located(string Path, File3dm.TableTypeFilter Tables, File3dm.ObjectTypeFilter Objects) : ArchiveSource;

    public sealed record Buffered(Arr<byte> Bytes) : ArchiveSource;
}

public sealed record ArchiveRead(File3dm Model, Option<string> Log) : IDisposable {
    public void Dispose() => Model.Dispose();
}

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

[SmartEnum]
public sealed partial class RelationKind {
    public static readonly RelationKind OnLayer = new(ModelComponentType.Layer, static archive =>
        from item in Conversions.Rows(archive.Objects)
        from index in Conversions.Present(item.Attributes.LayerIndex).ToSeq()
        select (item.Id, Left<int, Guid>(index)));

    public static readonly RelationKind UsesMaterial = new(ModelComponentType.Material, static archive =>
        from item in Conversions.Rows(archive.Objects)
        where item.Attributes.MaterialSource == ObjectMaterialSource.MaterialFromObject
        from index in Conversions.Present(item.Attributes.MaterialIndex).ToSeq()
        select (item.Id, Left<int, Guid>(index)));

    public static readonly RelationKind InGroup = new(ModelComponentType.Group, static archive =>
        from item in Conversions.Rows(archive.Objects)
        from index in Conversions.Rows(item.Attributes.GetGroupList())
        select (item.Id, Left<int, Guid>(index)));

    public static readonly RelationKind MemberOf = new(ModelComponentType.InstanceDefinition, static archive =>
        from definition in Conversions.Rows(archive.AllInstanceDefinitions)
        from member in Conversions.Rows(definition.GetObjectIds())
        select (member, Right<int, Guid>(definition.Id)));

    public static readonly RelationKind InstanceOf = new(ModelComponentType.InstanceDefinition, static archive =>
        from item in Conversions.Rows(archive.Objects)
        from placed in Optional(item.Geometry as InstanceReferenceGeometry).ToSeq()
        select (item.Id, Right<int, Guid>(placed.ParentIdefId)));

    public static readonly RelationKind ChildOf = new(ModelComponentType.Layer, static archive =>
        from layer in Conversions.Rows(archive.AllLayers)
        from parent in Conversions.Present(layer.ParentLayerId).ToSeq()
        select (layer.Id, Right<int, Guid>(parent)));

    public static readonly RelationKind LayerMaterial = new(ModelComponentType.Material, static archive =>
        from layer in Conversions.Rows(archive.AllLayers)
        from index in Conversions.Present(layer.RenderMaterialIndex).ToSeq()
        select (layer.Id, Left<int, Guid>(index)));

    public ModelComponentType Target { get; }

    [UseDelegateFromConstructor]
    public partial Seq<(Guid Source, Either<int, Guid> Reference)> Read(File3dm archive);
}

public readonly record struct ComponentRelation(ComponentIdentity Source, ComponentIdentity Target, RelationKind Kind) : IEdge<ComponentIdentity>;

public sealed record DanglingLink(Guid Source, RelationKind Kind, Either<int, Guid> Reference);

public sealed record ArchiveGraph(ArrayBidirectionalGraph<ComponentIdentity, ComponentRelation> Components, Seq<DanglingLink> Dangling, HashMap<Guid, string> Sources) {
    public Seq<ComponentIdentity> Unreferenced =>
        toSeq(Components.Vertices).Filter(vertex => Components.IsInEdgesEmpty(vertex) && toSeq(RelationKind.Items).Exists(kind => kind.Target == vertex.Type)).Strict();

    public HashMap<RelationKind, int> Coverage =>
        toHashMap(Components.Edges.CountBy(static edge => edge.Kind).Select(static count => (count.Key, count.Value)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ObjectFinding {
    private ObjectFinding(Guid id) => Id = id;

    public Guid Id { get; }

    public sealed record Invalid(Guid Id, string Log) : ObjectFinding(Id);

    public sealed record Unrealized(Guid Id) : ObjectFinding(Id);
}

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
public static partial class ArchiveHeaders {
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(new[] { nameof(File3dm.Notes), nameof(File3dmNotes.Notes) }, nameof(ArchiveHeader.Notes))]
    public static partial ArchiveHeader Of(File3dm archive);
}

public static class Archives {
    // --- [SCOPES]
    public static IO<T> Read<T>(ArchiveSource source, Func<ArchiveRead, IO<T>> body) =>
        use(IO.lift(() => source.Switch(
                located: static located => Exchange.ExistingPath(located.Path).Bind(existing =>
                    File3dm.ReadWithLog(existing, located.Tables, located.Objects, out string log) is { } model
                        ? new ArchiveRead(model, Conversions.Present(log))
                        : Fin.Fail<ArchiveRead>(new ArchiveRejected(nameof(File3dm.ReadWithLog), log))),
                buffered: static buffered =>
                    Missing.Unless(File3dm.FromByteArray([.. buffered.Bytes.AsSpan()]), nameof(File3dm.FromByteArray)).Map(static model => new ArchiveRead(model, None)))))
            .Bind(body)
            .Bracket();

    // --- [READS]
    public static IO<EarthAnchor> Anchor(File3dm archive) =>
        use(() => archive.EarthAnchorPoint).Bind(static point => IO.lift(() => EarthAnchor.Of(point))).Bracket();

    public static IO<ArchiveGraph> Graph(File3dm archive) =>
        from read in IO.lift(() => (
            Vertices: Seq<IEnumerable<ModelComponent>>(archive.AllLayers, archive.AllMaterials, archive.AllGroups, archive.AllInstanceDefinitions, archive.Objects)
                .Bind(static table => Conversions.Rows(table))
                .Filter(static component => !component.IsSystemComponent)
                .Map(static component => (Identity: new ComponentIdentity(component.Id, component.ComponentType), component.Index))
                .Strict(),
            Links: (from kind in toSeq(RelationKind.Items)
                    from reference in kind.Read(archive)
                    select new DanglingLink(reference.Source, kind, reference.Reference)).Strict(),
            Sources: toHashMap(Conversions.Rows(archive.AllInstanceDefinitions).Filter(static definition => definition.IsLinkedType).Map(static definition => (definition.Id, definition.SourceArchive)))))
        let byId = toHashMap(read.Vertices.Map(static vertex => (vertex.Identity.Id, vertex.Identity)))
        let byIndex = toHashMap(read.Vertices.Map(static vertex => ((vertex.Identity.Type, vertex.Index), vertex.Identity)))
        let resolved = read.Links.Map(link =>
                (from source in byId.Find(link.Source)
                 from target in link.Reference.Match(Left: index => byIndex.Find((link.Kind.Target, index)), Right: byId.Find)
                 select new ComponentRelation(source, target, link.Kind))
                .ToEither(link))
            .PartitionSequence()
        let outEdges = resolved.Rights.Fold(HashMap<ComponentIdentity, Seq<ComponentRelation>>(), static (edges, edge) => edges.AddOrUpdate(edge.Source, held => held.Add(edge), Seq(edge)))
        select new ArchiveGraph(
            read.Vertices.Map(static vertex => vertex.Identity)
                .ToBidirectionalGraph(vertex => outEdges.Find(vertex).IfNone(Seq<ComponentRelation>()), allowParallelEdges: false)
                .ToArrayBidirectionalGraph(),
            resolved.Lefts,
            read.Sources);

    public static IO<Seq<ObjectFinding>> Verify(File3dm archive) =>
        IO.lift(() => Conversions.Rows(archive.Objects).Bind(static item => Optional(item.Geometry).Match(
            Some: geometry => geometry.IsValidWithLog(out string log) ? Seq<ObjectFinding>() : Seq<ObjectFinding>(new ObjectFinding.Invalid(item.Id, log)),
            None: () => Seq<ObjectFinding>(new ObjectFinding.Unrealized(item.Id)))).Strict());

    // --- [WRITES]
    public static IO<Option<string>> Write(File3dm archive, DocumentKey key, Option<string> value) =>
        IO.lift(() => Conversions.Present(archive.Strings.SetString(key.Text, Conversions.Unset(value))));

    public static IO<Unit> Write(File3dm archive, EarthAnchor anchor) =>
        (from point in use(() => archive.EarthAnchorPoint)
         from _ in anchor.Apply(point)
         from committed in IO.lift(() => { archive.EarthAnchorPoint = point; })
         select committed).Bracket();

    public static IO<Unit> Embed(File3dm archive, string path) =>
        IO.lift(() => Exchange.ExistingPath(path).Bind(existing => Refused.Unless(archive.EmbeddedFiles.Add(existing), nameof(File3dmEmbeddedFiles.Add))));

    public static IO<Seq<string>> Extract(File3dm archive, string folder) =>
        IO.lift(() =>
            from target in Exchange.QualifiedPath(folder)
            from rows in Callbacks.Unique(
                Conversions.Rows(archive.EmbeddedFiles).Map(file => (File: file, Path: Path.Combine(target, file.Filename[(file.Filename.LastIndexOfAny(['/', '\\']) + 1)..]))).Strict(),
                static row => row.Path,
                nameof(File3dmEmbeddedFile.SaveToFile),
                StringComparer.OrdinalIgnoreCase).ToFin()
            from saved in Callbacks.Each(rows, static row => row.File.SaveToFile(row.Path), nameof(File3dmEmbeddedFile.SaveToFile))
            select rows.Map(static row => row.Path));

    public static IO<StepOutcome> Persist(File3dm archive, string path, File3dmWriteOptions options) =>
        StepOutcome.Land(path, target => IO.lift(() => Exchange.QualifiedPath(target).Bind(qualified =>
            archive.WriteWithLog(qualified, options, out string log) ? Fin.Succ(unit) : new ArchiveRejected(nameof(File3dm.WriteWithLog), log))));
}
