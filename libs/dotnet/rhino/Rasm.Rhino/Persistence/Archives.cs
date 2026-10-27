using QuikGraph;
using Rasm.RhinoCommon.Document.Files;
using Rasm.RhinoCommon.Document.Tables;
using Rhino;
using Rhino.DocObjects;
using Rhino.FileIO;
using Riok.Mapperly.Abstractions;

namespace Rasm.RhinoCommon.Persistence;

// --- [MODELS] --------------------------------------------------------------------------
[Union(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ArchiveSource {
    public sealed record Located(InputPath Path, File3dm.TableTypeFilter Tables, File3dm.ObjectTypeFilter Objects) : ArchiveSource {
        internal override IO<InputPath> Existing() => IO.pure(Path);

        internal override IO<ArchiveRead> Open() =>
            IO.lift(Fin<ArchiveRead> () => File3dm.ReadWithLog(Path, Tables, Objects, out string log) is { } model
                ? new ArchiveRead(model, Conversions.Present(log))
                : new RhinoFault(Path, RhinoReason.Unreadable, Inner: Error.New(log)));
    }

    public sealed record Buffered(Arr<byte> Bytes) : ArchiveSource {
        internal override IO<InputPath> Existing() =>
            IO.fail<InputPath>(Error.New("buffered archive names no file"));

        internal override IO<ArchiveRead> Open() =>
            IO.lift(() => Conversions.Found(File3dm.FromByteArray([.. Bytes])).Map(static model => new ArchiveRead(model, None)));
    }

    internal abstract IO<InputPath> Existing();

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
        Conversions.Rows(archive.Objects).Filter(static item => item.Attributes.MaterialSource is ObjectMaterialSource.MaterialFromObject)
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
        toHashSet(toSeq(RelationKind.Items).Map(static kind => kind.Target)) switch {
            var targets => toSeq(Components.Vertices).Filter(vertex => Components.IsInEdgesEmpty(vertex) && targets.Contains(vertex.Type)).Strict(),
        };

    public HashMap<RelationKind, int> Coverage =>
        toHashMap(Components.Edges.CountBy(static edge => edge.Kind).Select(static count => (count.Key, count.Value)));
}

public sealed record ObjectFinding(Guid Id, Option<string> Log);

public sealed record ArchiveDiff(
    HashSet<ComponentIdentity> Added,
    HashSet<ComponentIdentity> Removed,
    HashSet<ComponentRelation> Linked,
    HashSet<ComponentRelation> Unlinked) {
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
        (from read in use(source.Open(), static read => IO.lift(read.Model.Dispose))
         from value in body(read)
         select value).Bracket();

    // --- [READS]
    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(new[] { nameof(File3dm.Notes), nameof(File3dmNotes.Notes) }, nameof(ArchiveHeader.Notes))]
    public static partial ArchiveHeader Header(File3dm archive);

    public static IO<ArchiveGraph> Graph(File3dm archive) =>
        from read in IO.lift(() => (
            Vertices: (Conversions.Rows<ModelComponent>(archive.AllLayers) + Conversions.Rows<ModelComponent>(archive.AllMaterials) + Conversions.Rows<ModelComponent>(archive.AllGroups)
                    + Conversions.Rows<ModelComponent>(archive.AllInstanceDefinitions) + Conversions.Rows<ModelComponent>(archive.Objects))
                .Filter(static component => !component.IsSystemComponent)
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

    public static IO<Transform> Scaling(File3dm archive, RhinoDoc doc) =>
        IO.lift(() => Conversions.Found(Conversions.Present(LengthUnit.Scale(archive.Settings.ModelUnits, doc.ModelUnits)).Filter(static scale => scale > 0d))
            .Map(static scale => Transform.Scale(Point3d.Origin, scale)));

    public static IO<Seq<ObjectFinding>> Verify(File3dm archive) =>
        IO.lift(() => Conversions.Rows(archive.Objects).Choose(static item => item.Geometry switch {
            null => Some(new ObjectFinding(item.Id, None)),
            var geometry => geometry.IsValidWithLog(out string log) ? None : Some(new ObjectFinding(item.Id, Some(log))),
        }).Strict());

    // --- [WRITES]
    public static IO<Option<string>> Write(File3dm archive, DocumentKey key, Option<string> value) =>
        from prior in IO.lift(() => Conversions.Present(archive.Strings.GetValue(key.Text)))
        from written in value.Bind(Conversions.Present).Match(
            Some: text => IO.lift(() => ignore(archive.Strings.SetString(key.Text, text))),
            None: () => IO.lift(() => archive.Strings.Delete(key.Text)))
        select prior;

    public static IO<Unit> Embed(File3dm archive, InputPath path) =>
        IO.lift(() => RhinoFault.Unless(archive.EmbeddedFiles.Add(path)));

    public static IO<Seq<OutputPath>> Extract(File3dm archive, string folder) =>
        from candidates in IO.lift(() => Conversions.Rows(archive.EmbeddedFiles)
            .Traverse(file => OutputPath.Of(Path.Join(folder, Path.GetFileName(file.Filename.Replace('\\', Path.AltDirectorySeparatorChar)))).Map(path => (File: file, Path: path)))
            .As())
        from rows in IO.lift(() => Conversions.Unique(candidates, static row => (string)row.Path, StringComparer.OrdinalIgnoreCase))
        from saved in IO.lift(() => Callbacks.Each(rows, static (row, index) => RhinoFault.Unless(row.File.SaveToFile(row.Path), index)))
        select rows.Map(static row => row.Path);

    public static IO<StepOutcome> Persist(File3dm archive, OutputPath path, File3dmWriteOptions options) =>
        StepOutcome.Land(path, target => IO.lift(() =>
            archive.WriteWithLog(target, options, out string log) ? Fin.Succ(unit) : new RhinoFault(target, RhinoReason.Unwritable, Inner: Error.New(log))));
}
