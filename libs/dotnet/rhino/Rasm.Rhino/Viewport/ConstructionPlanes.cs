using System.Runtime.CompilerServices;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Document.Units;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.FileIO;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ConstructionPlaneState(StateName Name, Option<ViewportTarget> Source) : NamedState(ConstructionPlanes.Table, Name) {
    public Option<Func<ConstructionPlane, IO<Unit>>> Stage { get; init; }

    internal override IO<NamedWrite> Save(RhinoDoc doc) =>
        from source in Source.Match(
            Some: target => Viewports.With(doc, target, static row => ConstructionPlanes.Read(row.Viewport)),
            None: static () => IO.lift(static () => new ConstructionPlane()))
        from staged in Stage.Traverse(stage => stage(source)).As()
        from named in IO.lift(() => { source.Name = Name; })
        from written in ConstructionPlanes.Written(doc, source)
        select written;
}

public sealed record ConstructionPlaneRestore(StateName Name, ViewportSet Viewports) : NamedRestore(ConstructionPlanes.Table, Name) {
    internal override IO<NamedWrite> Restore(RhinoDoc doc) =>
        from plane in ConstructionPlanes.Found(doc, Name)
        from restored in Viewport.Viewports.Edit(doc, Viewports, viewport => IO.lift(() => viewport.PushConstructionPlane(plane)), new RedrawPolicy.Silent())
        select new NamedWrite(
            Viewport.Viewports.Edit(doc, Viewports, static viewport => Products.Accepted(viewport.PopConstructionPlane), new RedrawPolicy.AllViews(Deferred: true)).Map(unit),
            IO.pure(unit));
}

// --- [TABLES] --------------------------------------------------------------------------
public static class ConstructionPlanes {
    public static NamedTable Table { get; } = new(nameof(ConstructionPlanes), StringComparer.OrdinalIgnoreCase) {
        Entries = Entries,
        Rename = Rename,
        Delete = Delete,
        Hold = static (doc, viewport) => Held(doc, viewport.Match(
            Some: static id => new ViewportSet.One(new ViewportTarget.Id(id)),
            None: static () => (ViewportSet)new ViewportSet.One(new ViewportTarget.Active()))),
        Transform = Some(Transform),
        Import = Some(Import),
        Archive = Some<Func<RhinoDoc, Seq<NamedEntry>, File3dm, IO<Unit>>>(static (doc, entries, archive) =>
            entries.TraverseM(entry => Found(doc, entry.Name).Bind(plane => IO.lift(() => archive.AllNamedConstructionPlanes.Add(plane)))).As().Map(static _ => unit)),
        Current = Some(Current),
    };

    // --- [VIEWS]
    public static IO<ConstructionPlane> Read(ViewInfo view) =>
        IO.lift(() => {
            ConstructionPlane plane = view.GetConstructionPlane();
            plane.ShowZAxis = view.ShowConstructionZAxis;
            return plane;
        });

    public static IO<Unit> Write(ViewInfo view, ConstructionPlane plane) =>
        IO.lift(() => {
            view.SetConstructionPlane(plane);
            view.ShowConstructionZAxis = plane.ShowZAxis;
        });

    // --- [VIEWPORTS]
    public static IO<ConstructionPlane> Read(RhinoViewport viewport) =>
        use(() => new ViewInfo(viewport)).Bind(static view => Read(view)).Bracket();

    public static IO<Unit> Write(RhinoViewport viewport, ConstructionPlane plane) =>
        from pushed in IO.lift(() => viewport.PushConstructionPlane(plane))
        from grid in IO.lift(() => viewport.ConstructionGridVisible = plane.ShowGrid)
        from axes in IO.lift(() => viewport.ConstructionAxesVisible = plane.ShowAxes)
        from axis in use(() => new ViewInfo(viewport)).Bind(view => IO.lift(() => { view.ShowConstructionZAxis = plane.ShowZAxis; })).Bracket()
        select unit;

    // --- [ROWS]
    private static IO<Seq<NamedEntry>> Entries(RhinoDoc doc) =>
        IO.lift(() => Conversions.Rows(doc.NamedConstructionPlanes).Map(static plane => Entry(plane.Name)).Strict());

    private static NamedEntry Entry(string name) => new(Table, name, None);

    private static IO<Option<NamedEntry>> Current(RhinoDoc doc, Guid viewport) =>
        from plane in Viewports.With(doc, new ViewportTarget.Id(viewport), static row => IO.lift(() => row.Viewport.ConstructionPlane()))
        from tolerances in Tolerances.Read(doc, Space.Model)
        select Conversions.Rows(doc.NamedConstructionPlanes)
            .Find(stored => tolerances.Matches(stored.Plane, plane))
            .Map(static stored => Entry(stored.Name));

    private static IO<NamedWrite> Rename(RhinoDoc doc, NamedEntry entry, StateName name) =>
        from present in Found(doc, entry.Name)
        from renamed in Commits.Undoable(doc, $"{nameof(Rename)} {entry.Name}", IO.pure((From: (string)name, To: entry.Name)),
            names => Stored(doc, names.From).Bind(held => held.Traverse(row =>
                    from named in IO.lift(() => { row.Name = names.To; })
                    from added in Put(doc, names.To)(Some(row))
                    from deleted in Put(doc, names.From)(None)
                    select deleted).As())
                .Map(static _ => unit),
            (From: entry.Name, To: (string)name))
        select NamedWrite.Undoable;

    private static IO<NamedWrite> Delete(RhinoDoc doc, NamedEntry entry) =>
        from prior in Found(doc, entry.Name)
        from deleted in Commits.Undoable(doc, $"{nameof(Delete)} {entry.Name}", IO.pure(Some(prior)), Put(doc, entry.Name), Option<ConstructionPlane>.None)
        select NamedWrite.Undoable;

    // --- [HOLD]
    private static IO<NamedWrite> Held(RhinoDoc doc, ViewportSet set) =>
        from held in Viewports.Resolve(doc, set).Map(static rows => toHashMap(rows.Map(static row => (row.Viewport.Id, row.Viewport.GetConstructionPlane())))).Bracket()
        select new NamedWrite(
            Viewports.Edit(doc, set, viewport => IO.lift(() => held.Find(viewport.Id).Iter(viewport.PushConstructionPlane)), new RedrawPolicy.AllViews(Deferred: true)).Map(unit),
            IO.pure(unit));

    // --- [EXCHANGE]
    private static IO<NamedWrite> Transform(RhinoDoc doc, NamedEntry entry, Transform xform) =>
        Found(doc, entry.Name).Bind(stored => Moved(stored, xform)).Bind(moved => Written(doc, moved));

    private static IO<NamedWrite> Import(RhinoDoc doc, InputPath path, Option<Seq<StateName>> names) =>
        Table.Imported(doc, path, names, static archive => archive.AllNamedConstructionPlanes, static plane => plane.Name,
            (plane, scaling) => Moved(plane, scaling).Bind(moved => Written(doc, moved)));

    internal static IO<ConstructionPlane> Moved(ConstructionPlane plane, Transform xform) =>
        from dilation in IO.lift(() => NamedTable.Dilation(xform))
        from moved in IO.lift(() => {
            Plane staged = plane.Plane;
            return RhinoFault.Unless(staged.Transform(xform), RhinoReason.Declined, xform).Map(_ => staged);
        })
        from carried in IO.lift(() => { (plane.Plane, plane.GridSpacing, plane.SnapSpacing) = (moved, plane.GridSpacing * dilation, plane.SnapSpacing * dilation); })
        select plane;

    // --- [STORED]
    private static IO<Option<ConstructionPlane>> Stored(RhinoDoc doc, string name) =>
        IO.lift(() => Conversions.Present(doc.NamedConstructionPlanes.Find(name)).Map(index => doc.NamedConstructionPlanes[index]));

    internal static IO<ConstructionPlane> Found(RhinoDoc doc, string name) =>
        Stored(doc, name).Bind(found => IO.lift(() => found.ToFin(Table.Missing(name))));

    internal static IO<NamedWrite> Written(RhinoDoc doc, ConstructionPlane plane, [CallerMemberName] string verb = "") =>
        from prior in Stored(doc, plane.Name)
        from tolerances in Tolerances.Read(doc, Space.Model)
        from written in unless(prior.Exists(held => Same(held, plane, tolerances)),
            Commits.Undoable(doc, $"{verb} {plane.Name}", IO.pure(prior), Put(doc, plane.Name), Some(plane))).As()
        select NamedWrite.Undoable;

    private static Func<Option<ConstructionPlane>, IO<Unit>> Put(RhinoDoc doc, string name) =>
        row => row.Match(
            Some: plane => IO.lift(() => Conversions.Required(Conversions.Present(doc.NamedConstructionPlanes.Add(plane)))).Map(static _ => unit),
            None: () => Products.Accepted(() => doc.NamedConstructionPlanes.Delete(name)));

    private static bool Same(ConstructionPlane stored, ConstructionPlane staged, Tolerances tolerances) =>
        string.Equals(stored.Name, staged.Name, StringComparison.Ordinal)
        && tolerances.Matches(stored.Plane, staged.Plane)
        && Math.Abs(stored.GridSpacing - staged.GridSpacing) <= tolerances.Absolute
        && Math.Abs(stored.SnapSpacing - staged.SnapSpacing) <= tolerances.Absolute
        && stored.GridLineCount == staged.GridLineCount
        && stored.ThickLineFrequency == staged.ThickLineFrequency
        && stored.DepthBuffered == staged.DepthBuffered;
}
