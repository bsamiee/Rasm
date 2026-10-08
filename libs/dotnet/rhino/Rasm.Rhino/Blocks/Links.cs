using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Stores;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.DocObjects;
using Rhino.FileIO;

namespace Rasm.Rhino.Blocks;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record LinkedModel {
    public sealed record Opened(uint Document) : LinkedModel;
    public sealed record Stored(string Path) : LinkedModel;
}

public sealed record DefinitionLink : IEdge<LinkedModel> {
    // --- [VALUES]
    public LinkedModel Source { get; }
    public LinkedModel Target { get; }
    public Guid Definition { get; }
    public string Name { get; }
    public UnitSystem Units { get; }
    public Option<InstanceDefinitionArchiveFileStatus> Status { get; }
    public bool Outdated => Status.Exists(static status => status is InstanceDefinitionArchiveFileStatus.LinkedFileIsNewer
        or InstanceDefinitionArchiveFileStatus.LinkedFileIsOlder or InstanceDefinitionArchiveFileStatus.LinkedFileIsDifferent);

    // --- [CONSTRUCTION]
    internal DefinitionLink(LinkedModel source, LinkedModel.Stored target, InstanceDefinitionGeometry definition) =>
        (Source, Target, Definition, Name, Units, Status) = (source, target, definition.Id, definition.Name, definition.UnitSystem,
            Optional(definition as InstanceDefinition).Map(static row => row.ArchiveFileStatus));
}

public sealed record LinkReading(UnitSystem Units, Option<string> Log, Seq<DefinitionLink> Links);

public sealed record LinkClosure(LinkedModel Root, ArrayBidirectionalGraph<LinkedModel, DefinitionLink> Graph, HashMap<LinkedModel, Fin<LinkReading>> Readings) {
    public Seq<(DefinitionLink Link, Error Error)> Broken =>
        toSeq(Graph.Edges).Choose(edge => Readings[edge.Target].Match(
            Succ: static _ => Option<(DefinitionLink Link, Error Error)>.None, Fail: error => Some((Link: edge, Error: error))));

    public Seq<DefinitionLink> Stale => toSeq(Graph.Edges).Filter(static edge => edge.Outdated);

    public Seq<(DefinitionLink Link, UnitSystem FileUnits)> Mismatched =>
        from edge in toSeq(Graph.Edges)
        from reading in Readings[edge.Target].ToSeq()
        where reading.Units != edge.Units
        select (Link: edge, FileUnits: reading.Units);

    public Seq<Seq<LinkedModel>> Cycles =>
        toSeq(Graph.CondensateStronglyConnected<LinkedModel, DefinitionLink, AdjacencyGraph<LinkedModel, DefinitionLink>>().Vertices)
            .Filter(static component => component.EdgeCount > 0).Map(static component => toSeq(component.Vertices));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Links {
    // --- [CLOSURE]
    public static IO<LinkClosure> Closure(LinkedModel root) =>
        from first in Read(root)
        from frontier in Monad.recur(
            new Frontier(Seq<LinkedModel>(), Seq<LinkedModel>(), HashMap<LinkedModel, Fin<LinkReading>>()).Add(root, first),
            static pending => pending.Pending.Match(
                Empty: () => IO.pure(Next.Done<Frontier, Frontier>(pending)),
                Tail: (next, rest) => Visit(pending with { Pending = rest }, next).Map(Next.Loop<Frontier, Frontier>))).As()
        select new LinkClosure(root,
            frontier.Order.ToBidirectionalGraph(model => frontier.Readings[model].ToSeq().Bind(static reading => reading.Links), allowParallelEdges: true)
                .ToArrayBidirectionalGraph(), frontier.Readings);

    private static IO<Frontier> Visit(Frontier frontier, LinkedModel model) =>
        frontier.Readings.ContainsKey(model) ? IO.pure(frontier) : Read(model).Map(reading => frontier.Add(model, reading))
            .Catch(static error => error.IsType<FileMissing>() || error.IsType<UnqualifiedPath>() || error.IsType<ArchiveRejected>(),
                error => IO.pure(frontier.Add(model, error)));

    private static IO<LinkReading> Read(LinkedModel model) =>
        model.Switch(
            opened: static opened => IO.lift(() => Missing.Unless(RhinoDoc.FromRuntimeSerialNumber(opened.Document), nameof(RhinoDoc.FromRuntimeSerialNumber)))
                .Map(doc => new LinkReading(doc.ModelUnitSystem, None, Linked(opened, doc.InstanceDefinitions.GetList(ignoreDeleted: true)))),
            stored: static stored => Archives.Read(
                new ArchiveSource.Located(stored.Path, File3dm.TableTypeFilter.InstanceDefinition | File3dm.TableTypeFilter.Settings, File3dm.ObjectTypeFilter.InstanceReference),
                read => IO.lift(() => new LinkReading(read.Model.Settings.ModelUnitSystem, read.Log, Linked(stored, read.Model.AllInstanceDefinitions)))));

    private static Seq<DefinitionLink> Linked(LinkedModel source, IEnumerable<InstanceDefinitionGeometry> definitions) =>
        Conversions.Rows(definitions).Filter(static definition => definition.IsLinkedType && !definition.IsReference)
            .Map(definition => new DefinitionLink(source, new LinkedModel.Stored(source.Switch(definition,
                opened: static (row, _) => row.SourceArchive,
                stored: static (row, file) => Resolved(file, row))), definition)).Strict();

    private static string Resolved(LinkedModel.Stored holder, InstanceDefinitionGeometry definition) =>
        definition.SourceArchive switch {
            var stored => (Conversions.Present(stored).Filter(File.Exists)
                || Conversions.Present(definition.SourceArchiveRelativePath)
                    .Map(relative => Path.GetFullPath(relative.Replace('\\', Path.DirectorySeparatorChar), Path.GetDirectoryName(holder.Path)!)).Filter(File.Exists)
                || Conversions.Present(stored).Bind(static path => Conversions.Present(FileSettings.FindFile(Path.GetFileName(path.Replace('\\', Path.DirectorySeparatorChar))))))
                .IfNone(stored),
        };

    private sealed record Frontier(Seq<LinkedModel> Pending, Seq<LinkedModel> Order, HashMap<LinkedModel, Fin<LinkReading>> Readings) {
        public Frontier Add(LinkedModel model, Fin<LinkReading> reading) =>
            new(Pending + reading.ToSeq().Bind(static held => held.Links).Map(static link => link.Target), Order.Add(model), Readings.Add(model, reading));
    }

    // --- [STYLE]
    private const LinkedInstanceDefinitionUpdateStyle Factory = LinkedInstanceDefinitionUpdateStyle.Prompt;

    public static ValueStore<LinkedInstanceDefinitionUpdateStyle> Style(RhinoDoc doc) =>
        Style(IO.lift(() => doc.LinkedInstanceDefinitionUpdate), style => doc.LinkedInstanceDefinitionUpdate = style);

    public static ValueStore<LinkedInstanceDefinitionUpdateStyle> Style(File3dm archive) =>
        Style(IO.lift(() => (LinkedInstanceDefinitionUpdateStyle)archive.Settings.InstanceDefinitionLinkUpdate), style => archive.Settings.InstanceDefinitionLinkUpdate = (int)style);

    private static ValueStore<LinkedInstanceDefinitionUpdateStyle> Style(IO<LinkedInstanceDefinitionUpdateStyle> read, Action<LinkedInstanceDefinitionUpdateStyle> write) =>
        ValueStore.Of(read.Map(static style => Some(style).Filter(static held => held != Factory)),
            style => IO.lift(() => write(style.IfNone(Factory))), Applied.Live, None);

    // --- [RELOAD]
    public static IO<Committed<Seq<int>>> Reload(RhinoDoc doc, string name, RedrawPolicy redraw) =>
        from outdated in IO.lift(() => Linked(new LinkedModel.Opened(doc.RuntimeSerialNumber), doc.InstanceDefinitions.GetList(ignoreDeleted: true)).Filter(static link => link.Outdated).Strict())
        from committed in Definitions.Commit(doc, name, redraw,
            outdated.Map<DefinitionOp>(static link => new DefinitionOp.Edit(link.Definition, new DefinitionEdit.RefreshLinkedBlock())))
        select committed;
}
