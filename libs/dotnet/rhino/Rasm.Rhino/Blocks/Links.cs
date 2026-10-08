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
[Union]
public abstract partial record LinkedModel {
    public sealed record Opened(uint Document) : LinkedModel;

    public sealed record Stored(string Path) : LinkedModel;
}

public readonly record struct DefinitionLink(
    LinkedModel Source,
    LinkedModel.Stored Target,
    Guid Definition,
    string Name,
    UnitSystem Units,
    Option<InstanceDefinitionArchiveFileStatus> Status) : IEdge<LinkedModel> {
    LinkedModel IEdge<LinkedModel>.Target => Target;

    public bool Outdated =>
        Status.Exists(static status => status is InstanceDefinitionArchiveFileStatus.LinkedFileIsNewer
            or InstanceDefinitionArchiveFileStatus.LinkedFileIsOlder
            or InstanceDefinitionArchiveFileStatus.LinkedFileIsDifferent);
}

public sealed record LinkReading(UnitSystem Units, Option<string> Log, Seq<DefinitionLink> Links);

public sealed record LinkClosure(LinkedModel Root, ArrayBidirectionalGraph<LinkedModel, DefinitionLink> Graph, HashMap<LinkedModel, Fin<LinkReading>> Readings) {
    public Seq<(DefinitionLink Link, Error Error)> Broken =>
        toSeq(Graph.Edges).Choose(edge => Readings.Find(edge.Target)
            .Bind(static reading => reading.Match(Succ: static _ => Option<Error>.None, Fail: static error => Some(error)))
            .Map(error => (Link: edge, Error: error)));

    public Seq<DefinitionLink> Stale =>
        toSeq(Graph.Edges).Filter(static edge => edge.Outdated);

    public Seq<(DefinitionLink Link, UnitSystem FileUnits)> Mismatched =>
        toSeq(Graph.Edges).Choose(edge => Readings.Find(edge.Target)
            .Bind(static reading => reading.ToOption())
            .Filter(target => target.Units != edge.Units)
            .Map(target => (Link: edge, FileUnits: target.Units)));

    public Seq<Seq<LinkedModel>> Cycles =>
        toSeq(Graph.CondensateStronglyConnected<LinkedModel, DefinitionLink, AdjacencyGraph<LinkedModel, DefinitionLink>>().Vertices)
            .Filter(static group => group.EdgeCount > 0)
            .Map(static group => toSeq(group.Vertices));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Links {
    // --- [CLOSURE]
    public static IO<LinkClosure> Closure(RhinoDoc doc) =>
        IO.lift(() => new LinkReading(doc.ModelUnitSystem, None, Linked(doc)))
            .Bind(first => Walk(new LinkedModel.Opened(doc.RuntimeSerialNumber), first));

    public static IO<LinkClosure> Closure(string path) =>
        new LinkedModel.Stored(path) switch {
            var root => Read(root).Bind(first => Walk(root, first)),
        };

    private static IO<LinkClosure> Walk(LinkedModel root, LinkReading first) =>
        Monad.recur(
                new Frontier(Seq<LinkedModel.Stored>(), Seq<LinkedModel>(), HashMap<LinkedModel, Fin<LinkReading>>()).Add(root, first),
                static frontier => frontier.Pending.Head.Match(
                    Some: next => Visit(frontier with { Pending = frontier.Pending.Tail }, next).Map(Next.Loop<Frontier, Frontier>),
                    None: () => IO.pure(Next.Done<Frontier, Frontier>(frontier))))
            .As()
            .Map(frontier => new LinkClosure(
                root,
                frontier.Order
                    .ToBidirectionalGraph(model => frontier.Readings.Find(model).Bind(static reading => reading.ToOption()).ToSeq().Bind(static reading => reading.Links))
                    .ToArrayBidirectionalGraph(),
                frontier.Readings));

    private static IO<Frontier> Visit(Frontier frontier, LinkedModel.Stored file) =>
        frontier.Readings.ContainsKey(file)
            ? IO.pure(frontier)
            : Read(file)
                .Map(reading => frontier.Add(file, reading))
                .Catch(
                    static error => error.IsType<FileMissing>() || error.IsType<UnqualifiedPath>() || error.IsType<ArchiveRejected>(),
                    error => IO.pure(frontier.Add(file, error)));

    private static IO<LinkReading> Read(LinkedModel.Stored file) =>
        Archives.Read(
            new ArchiveSource.Located(file.Path, File3dm.TableTypeFilter.InstanceDefinition | File3dm.TableTypeFilter.Settings, File3dm.ObjectTypeFilter.InstanceReference),
            read => IO.lift(() => new LinkReading(
                read.Model.Settings.ModelUnitSystem,
                read.Log,
                toSeq(read.Model.AllInstanceDefinitions)
                    .Filter(static definition => definition.IsLinkedType && !definition.IsReference)
                    .Map(definition => new DefinitionLink(file, Resolved(file, definition), definition.Id, definition.Name, definition.UnitSystem, None))
                    .Strict())));

    private static LinkedModel.Stored Resolved(LinkedModel.Stored holder, InstanceDefinitionGeometry definition) =>
        definition.SourceArchive switch {
            var stored => new LinkedModel.Stored((
                    Conversions.Present(stored).Filter(File.Exists)
                    || Conversions.Present(definition.SourceArchiveRelativePath)
                        .Map(relative => Path.GetFullPath(Path.Combine([holder.Path, "..", .. relative.Split(['/', '\\'])])))
                        .Filter(File.Exists)
                    || Conversions.Present(stored).Bind(static held => Conversions.Present(FileSettings.FindFile(held[(held.LastIndexOfAny(['/', '\\']) + 1)..]))))
                .IfNone(stored)),
        };

    private static Seq<DefinitionLink> Linked(RhinoDoc doc) =>
        Conversions.Rows(doc.InstanceDefinitions.GetList(ignoreDeleted: true))
            .Filter(static definition => definition.IsLinkedType && !definition.IsReference)
            .Map(definition => new DefinitionLink(
                new LinkedModel.Opened(doc.RuntimeSerialNumber),
                new LinkedModel.Stored(definition.SourceArchive),
                definition.Id,
                definition.Name,
                definition.UnitSystem,
                Some(definition.ArchiveFileStatus)))
            .Strict();

    private sealed record Frontier(Seq<LinkedModel.Stored> Pending, Seq<LinkedModel> Order, HashMap<LinkedModel, Fin<LinkReading>> Readings) {
        public Frontier Add(LinkedModel model, Fin<LinkReading> reading) =>
            new(Pending + reading.ToSeq().Bind(static held => held.Links).Map(static link => link.Target), Order.Add(model), Readings.Add(model, reading));
    }

    // --- [STYLE]
    private const LinkedInstanceDefinitionUpdateStyle Factory = LinkedInstanceDefinitionUpdateStyle.Prompt;

    public static ValueStore<LinkedInstanceDefinitionUpdateStyle> Style(RhinoDoc doc) =>
        ValueStore.Of(
            IO.lift(() => Held(doc.LinkedInstanceDefinitionUpdate)),
            style => IO.lift(() => { doc.LinkedInstanceDefinitionUpdate = style.IfNone(Factory); }),
            Applied.Live,
            None);

    public static ValueStore<LinkedInstanceDefinitionUpdateStyle> Style(File3dm archive) =>
        ValueStore.Of(
            IO.lift(() => Held((LinkedInstanceDefinitionUpdateStyle)archive.Settings.InstanceDefinitionLinkUpdate)),
            style => IO.lift(() => { archive.Settings.InstanceDefinitionLinkUpdate = (int)style.IfNone(Factory); }),
            Applied.Live,
            None);

    private static Option<LinkedInstanceDefinitionUpdateStyle> Held(LinkedInstanceDefinitionUpdateStyle style) =>
        Some(style).Filter(static held => Enum.IsDefined(held) && held != Factory);

    // --- [RELOAD]
    public static IO<Committed<Seq<int>>> Reload(RhinoDoc doc, string name, RedrawPolicy redraw) =>
        IO.lift(() => Linked(doc).Filter(static link => link.Outdated).Strict())
            .Bind(outdated => Definitions.Commit(
                doc,
                name,
                redraw,
                outdated.Map<DefinitionOp>(static link => new DefinitionOp.Edit(link.Definition, new DefinitionEdit.RefreshLinkedBlock()))));
}
