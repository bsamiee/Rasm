using Rasm.RhinoCommon.Document;
using Rasm.RhinoCommon.Document.Tables;
using Rasm.RhinoCommon.Document.Units;
using Rasm.RhinoCommon.Persistence;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.FileIO;

namespace Rasm.RhinoCommon.Viewport;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ConstructionPlanes {
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

    public static Fin<Unit> Write(RhinoViewport viewport, ConstructionPlane plane) {
        viewport.PushConstructionPlane(plane);
        using ViewInfo view = new(viewport);
        (view.ShowConstructionGrid, view.ShowConstructionAxes, view.ShowConstructionZAxis) = (plane.ShowGrid, plane.ShowAxes, plane.ShowZAxis);
        return unit;
    }

    // --- [ROWS]
    internal static IO<Seq<NamedEntry>> Entries(RhinoDoc doc) =>
        IO.lift(() => Conversions.Rows(doc.NamedConstructionPlanes).Map(static plane => new NamedEntry(NamedTable.ConstructionPlanes, plane.Name, None)).Strict());

    internal static IO<Option<NamedEntry>> Current(RhinoDoc doc, ViewportTarget target) =>
        from plane in Viewports.With(doc, target, static row => IO.lift(() => row.Viewport.ConstructionPlane()))
        from tolerances in Tolerances.Read(doc, modelUnits: true)
        select Conversions.Rows(doc.NamedConstructionPlanes)
            .Find(stored => tolerances.Matches(stored.Plane, plane))
            .Map(static stored => new NamedEntry(NamedTable.ConstructionPlanes, stored.Name, None));

    internal static IO<NamedWrite> Rename(RhinoDoc doc, NamedEntry entry, StateName name, object owner) =>
        from present in Found(doc, entry.Name)
        from renamed in TableOps.Undoable(doc, $"{nameof(Rename)} {entry.Name}", IO.pure((From: (string)name, To: entry.Name)),
            names => Stored(doc, names.From).Bind(held => held.Traverse(row =>
                    from named in IO.lift(() => { row.Name = names.To; })
                    from added in Put(doc, names.To)(Some(row))
                    from deleted in Put(doc, names.From)(None)
                    select deleted).As())
                .Map(static _ => unit),
            (From: entry.Name, To: (string)name), owner)
        select (NamedWrite)new NamedWrite.Recorded();

    internal static IO<NamedWrite> Delete(RhinoDoc doc, NamedEntry entry, object owner) =>
        from prior in Found(doc, entry.Name)
        from deleted in TableOps.Undoable(doc, $"{nameof(Delete)} {entry.Name}", IO.pure(Some(prior)), Put(doc, entry.Name), Option<ConstructionPlane>.None, owner)
        select (NamedWrite)new NamedWrite.Recorded();

    // --- [STATES]
    internal static IO<NamedWrite> Save(RhinoDoc doc, NamedState.ConstructionPlane state, object owner) =>
        from source in state.Source.Match(
            Some: target => Viewports.With(doc, target, static row => Read(row.Viewport)),
            None: static () => IO.lift(static () => new ConstructionPlane()))
        from staged in state.Stage.Traverse(stage => stage(source)).As()
        from named in IO.lift(() => { source.Name = state.Name; })
        from written in Written(doc, nameof(Save), source, owner)
        select written;

    internal static IO<NamedWrite> Restore(RhinoDoc doc, NamedRestore.ConstructionPlane target) =>
        from plane in Found(doc, target.Name)
        from restored in Viewports.Edit(doc, target.Viewports, viewport => IO.lift(() => viewport.PushConstructionPlane(plane)), new RedrawPolicy.Silent())
        select (NamedWrite)new NamedWrite.Compensated(
            Viewports.Edit(doc, target.Viewports, static viewport => IO.lift(() => RhinoFault.Unless(viewport.PopConstructionPlane())), new RedrawPolicy.AllViews(Deferred: true))
                .Map(static _ => unit),
            IO.pure(unit));

    internal static IO<NamedWrite> Hold(RhinoDoc doc, Option<ViewportSet> scope) =>
        scope.IfNone(static () => new ViewportSet.One(new ViewportTarget.Active())) switch {
            var set =>
                from held in Viewports.Resolve(doc, set).Map(static rows => toHashMap(rows.Map(static row => (row.Viewport.Id, row.Viewport.GetConstructionPlane())))).Bracket()
                select (NamedWrite)new NamedWrite.Compensated(
                    Viewports.Edit(doc, set, viewport => IO.lift(() => held.Find(viewport.Id).Iter(viewport.PushConstructionPlane)), new RedrawPolicy.AllViews(Deferred: true))
                        .Map(static _ => unit),
                    IO.pure(unit)),
        };

    // --- [EXCHANGE]
    internal static IO<NamedWrite> Transform(RhinoDoc doc, NamedEntry entry, Transform xform, object owner) =>
        from stored in Found(doc, entry.Name)
        from moved in Moved(stored, xform)
        from written in Written(doc, nameof(Transform), moved, owner)
        select written;

    internal static IO<NamedWrite> Import(RhinoDoc doc, ArchiveSource source, Option<Seq<StateName>> names, object owner) =>
        NamedTable.ConstructionPlanes.Imported(doc, source, names, static archive => archive.AllNamedConstructionPlanes, static plane => plane.Name,
            (plane, scaling) => Moved(plane, scaling).Bind(moved => Written(doc, nameof(Import), moved, owner)));

    internal static IO<Unit> Archive(RhinoDoc doc, Seq<NamedEntry> entries, File3dm archive) =>
        entries.TraverseM(entry => Found(doc, entry.Name).Bind(plane => IO.lift(() => archive.AllNamedConstructionPlanes.Add(plane)))).As().Map(static _ => unit);

    internal static IO<ConstructionPlane> Moved(ConstructionPlane plane, Transform xform) =>
        from dilation in IO.lift(() => NamedTable.Dilation(xform))
        from moved in IO.lift(() => {
            Plane staged = plane.Plane;
            return RhinoFault.Unless(staged.Transform(xform)).Map(_ => staged);
        })
        from carried in IO.lift(() => { (plane.Plane, plane.GridSpacing, plane.SnapSpacing) = (moved, plane.GridSpacing * dilation, plane.SnapSpacing * dilation); })
        select plane;

    // --- [STORED]
    private static IO<Option<ConstructionPlane>> Stored(RhinoDoc doc, string name) =>
        IO.lift(() => Conversions.Present(doc.NamedConstructionPlanes.Find(name)).Map(index => doc.NamedConstructionPlanes[index]));

    private static IO<ConstructionPlane> Found(RhinoDoc doc, string name) =>
        Stored(doc, name).Bind(found => IO.lift(() => found.ToFin(new RhinoFault(name, RhinoReason.Missing))));

    private static IO<NamedWrite> Written(RhinoDoc doc, string verb, ConstructionPlane plane, object owner) =>
        from prior in Stored(doc, plane.Name)
        from tolerances in Tolerances.Read(doc, modelUnits: true)
        from written in unless(prior.Exists(held => Same(held, plane, tolerances)),
            TableOps.Undoable(doc, $"{verb} {plane.Name}", IO.pure(prior), Put(doc, plane.Name), Some(plane), owner)).As()
        select (NamedWrite)new NamedWrite.Recorded();

    private static Func<Option<ConstructionPlane>, IO<Unit>> Put(RhinoDoc doc, string name) =>
        row => IO.lift(() => row.Match(
            Some: plane => Conversions.Found(Conversions.Present(doc.NamedConstructionPlanes.Add(plane))).Map(static _ => unit),
            None: () => RhinoFault.Unless(doc.NamedConstructionPlanes.Delete(name))));

    private static bool Same(ConstructionPlane stored, ConstructionPlane staged, Tolerances tolerances) =>
        string.Equals(stored.Name, staged.Name, StringComparison.Ordinal)
        && tolerances.Matches(stored.Plane, staged.Plane)
        && Math.Abs(stored.GridSpacing - staged.GridSpacing) <= tolerances.Absolute
        && Math.Abs(stored.SnapSpacing - staged.SnapSpacing) <= tolerances.Absolute
        && stored.GridLineCount == staged.GridLineCount
        && stored.ThickLineFrequency == staged.ThickLineFrequency
        && stored.DepthBuffered == staged.DepthBuffered;
}
