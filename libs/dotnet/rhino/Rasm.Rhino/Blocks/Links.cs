using System.Collections.Generic;
using QuikGraph;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Persistence;
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
    public bool Outdated => Status.Exists(static status => status.Outdated);

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
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Links {
    // --- [CLOSURE]
    public static IO<LinkClosure> Closure(LinkedModel root) =>
        Graphs.Reached(root, Read, static reading => reading.Links).Map(reached => new LinkClosure(root, reached.Graph, reached.Readings));

    private static IO<LinkReading> Read(LinkedModel model) =>
        model.Switch(
            opened: static opened => IO.lift(() => Conversions.Required(RhinoDoc.FromRuntimeSerialNumber(opened.Document)))
                .Map(doc => new LinkReading(doc.ModelUnitSystem, None, Linked(opened, Conversions.Present(doc.Path), doc.InstanceDefinitions.GetList(ignoreDeleted: true)))),
            stored: static stored => IO.lift(() => InputPath.Of(stored.Path)).Bind(path => Archives.Read(
                new ArchiveSource.Located(path, File3dm.TableTypeFilter.InstanceDefinition | File3dm.TableTypeFilter.Settings, File3dm.ObjectTypeFilter.InstanceReference),
                read => IO.lift(() => new LinkReading(read.Model.Settings.ModelUnitSystem, read.Log, Linked(stored, Some(stored.Path), read.Model.AllInstanceDefinitions))))));

    private static Seq<DefinitionLink> Linked(LinkedModel source, Option<string> holder, IEnumerable<InstanceDefinitionGeometry> definitions) =>
        Conversions.Rows(definitions).Filter(static definition => definition.IsLinkedType && !definition.IsReference)
            .Map(definition => new DefinitionLink(source, new LinkedModel.Stored(Resolved(holder, definition).IfNone(definition.SourceArchive)), definition)).Strict();

    private static Option<string> Resolved(Option<string> holder, InstanceDefinitionGeometry definition) =>
        Conversions.Present(definition.SourceArchive).Filter(File.Exists)
        || (from relative in Conversions.Present(definition.SourceArchiveRelativePath)
            from folder in holder.Bind(static path => Optional(Path.GetDirectoryName(path)))
            select Path.GetFullPath(relative.Replace('\\', Path.DirectorySeparatorChar), folder)).Filter(File.Exists)
        || Conversions.Present(definition.SourceArchive).Bind(static path => Conversions.Present(FileSettings.FindFile(Path.GetFileName(path.Replace('\\', Path.DirectorySeparatorChar)))));

    // --- [REPAIR]
    public static IO<Unit> Relink(RhinoDoc doc, InstanceDefinition definition, InputPath path, bool relative) =>
        (from folder in IO.lift(() => relative
             ? Conversions.Present(doc.Path).Bind(static held => Optional(Path.GetDirectoryName(held))).ToFin(new RhinoFault(doc.Name, RhinoReason.Unqualified)).Map(Some)
             : Option<string>.None)
         from reference in Products.Of(() => FileReference.CreateFromFullAndRelativePaths(path, folder.Map(held => Path.GetRelativePath(held, path)).ValueUnsafe()))
         from modified in Products.Accepted(() => doc.InstanceDefinitions.ModifySourceArchive(definition.Index, reference, definition.UpdateType, definition.LayerStyle, quiet: true))
         select modified).Bracket();

    public static IO<Committed<Seq<Guid>>> Repair(RhinoDoc doc, bool relative, string name, RedrawPolicy redraw) =>
        Rewritten(doc, name, redraw, definition => !File.Exists(definition.SourceArchive)
            ? Resolved(Conversions.Present(doc.Path), definition).Bind(static found => InputPath.Of(found).ToOption()).Map(path => Relink(doc, definition, path, relative))
            : None);

    public static IO<Committed<Seq<Guid>>> Relative(RhinoDoc doc, bool relative, string name, RedrawPolicy redraw) =>
        Rewritten(doc, name, redraw, definition => InputPath.Of(definition.SourceArchive).ToOption().Map(path => Relink(doc, definition, path, relative)));

    public static IO<Committed<Seq<Guid>>> Reload(RhinoDoc doc, string name, RedrawPolicy redraw) =>
        Rewritten(doc, name, redraw, definition => Conversions.Present(definition.ArchiveFileStatus.Outdated, Products.Accepted(() => doc.InstanceDefinitions.RefreshLinkedBlock(definition))));

    private static IO<Committed<Seq<Guid>>> Rewritten(RhinoDoc doc, string name, RedrawPolicy redraw, Func<InstanceDefinition, Option<IO<Unit>>> rewrite) =>
        IO.lift(() => Conversions.Rows(doc.InstanceDefinitions.GetList(ignoreDeleted: true)).Filter(static definition => definition.IsLinkedType && !definition.IsReference)
                .Choose(definition => rewrite(definition).Map(edit => edit.Map(_ => definition.Id))).Strict())
            .Bind(edits => Commits.Commit(doc, name, redraw, edits.TraverseM(static edit => edit).As()));
}
