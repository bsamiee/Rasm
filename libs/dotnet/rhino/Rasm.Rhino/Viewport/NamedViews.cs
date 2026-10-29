using System.Runtime.CompilerServices;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Document.Units;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.FileIO;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record RestorePacing {
    public sealed record Immediate : RestorePacing;

    public sealed record MatchAspect : RestorePacing;

    public sealed record ConstantSpeed(double UnitsPerFrame, Duration Delay) : RestorePacing;

    public sealed record ConstantTime(int Frames, Duration Delay) : RestorePacing;
}

public sealed record NamedViewState(StateName Name, ViewportTarget Source) : NamedState(NamedViews.Table, Name) {
    public Option<Func<RhinoViewport, ViewInfo, IO<Unit>>> Stage { get; init; }

    internal override IO<NamedWrite> Save(RhinoDoc doc) =>
        Viewports.With(doc, Source, static row => IO.lift(() => new RhinoViewport(row.Viewport)))
            .Bind(copy => NamedViews.Staged(doc, Name, copy, Stage));
}

public sealed record NamedViewRestore(StateName Name, ViewportSet Viewports, RestorePacing Pacing) : NamedRestore(NamedViews.Table, Name) {
    internal override IO<NamedWrite> Restore(RhinoDoc doc) =>
        from index in NamedViews.Find(doc, Name)
        from held in NamedViews.Held(doc, Viewports)
        from restored in Viewport.Viewports.Edit(
                doc,
                Viewports,
                viewport => IO.lift(() => viewport.Name).Bind(name => NamedViews.Applied(doc, index, viewport, Pacing).Finally(IO.lift(() => { viewport.Name = name; }))),
                new RedrawPolicy.Silent())
            .Catch(error => NamedWrite.Undone([held]) >> IO.fail<Seq<bool>>(error)).As()
        select held;
}

// --- [TABLES] --------------------------------------------------------------------------
public static class NamedViews {
    public static NamedTable Table { get; } = new(nameof(NamedViews), StringComparer.OrdinalIgnoreCase) {
        Entries = static doc => Rows(doc, static view => IO.pure(Entry(view))),
        Rename = Rename,
        Delete = Delete,
        Hold = static (doc, viewport) => Held(doc, viewport.Match(
            Some: static id => new ViewportSet.One(new ViewportTarget.Id(id)),
            None: static () => (ViewportSet)new ViewportSet.One(new ViewportTarget.Active()))),
        Transform = Some(Transform),
        Import = Some(Import),
        Archive = Some(Archive),
        Current = Some(Current),
    };

    // --- [ROWS]
    private static IO<Option<NamedEntry>> Current(RhinoDoc doc, Guid viewport) =>
        from pose in Viewports.With(doc, new ViewportTarget.Id(viewport), static row => Cameras.Read(row.Viewport))
        from tolerances in Tolerances.Read(doc, Space.Model)
        from matched in Rows(doc, view => IO.lift(() => Cameras.Read(view.Viewport).Map(stored => Conversions.Present(stored.Matches(pose, tolerances), Entry(view)))))
        select matched.Somes().Head;

    private static IO<NamedWrite> Rename(RhinoDoc doc, NamedEntry entry, StateName name) =>
        from found in Find(doc, entry.Name)
        from renamed in Commits.Undoable(doc, $"{nameof(Rename)} {entry.Name}", IO.pure((From: (string)name, To: entry.Name)),
            names => Products.Accepted(() => doc.NamedViews.Rename(names.From, names.To)),
            (From: entry.Name, To: (string)name))
        select NamedWrite.Undoable;

    private static IO<NamedWrite> Delete(RhinoDoc doc, NamedEntry entry) =>
        from prior in Read(doc, entry.Name, Held)
        from deleted in Products.Transfer(prior, held => Commits.Undoable(doc, $"{nameof(Delete)} {entry.Name}", IO.pure(Some(held)), Put(doc, entry.Name), Option<RhinoViewport>.None))
        select NamedWrite.Undoable;

    internal static IO<int> Find(RhinoDoc doc, string name) =>
        IO.lift(() => Conversions.Present(doc.NamedViews.FindByName(name)).ToFin(Table.Missing(name)));

    // --- [SAVE]
    internal static IO<NamedWrite> Staged(RhinoDoc doc, string name, RhinoViewport copy, Option<Func<RhinoViewport, ViewInfo, IO<Unit>>> stage, [CallerMemberName] string verb = "") =>
        Products.Transfer(copy, held =>
            from staged in use(() => new ViewInfo(held)).Bind(view => stage.Traverse(run => run(held, view)).As()).Bracket()
            from prior in Stored(doc, name)
            from same in Same(doc, prior, held)
            from written in same
                ? IO.lift(held.Dispose) >> prior.Traverse(static stored => IO.lift(stored.Dispose)).As().Map(unit)
                : Commits.Undoable(doc, $"{verb} {name}", IO.pure(prior), Put(doc, name), Some(held))
            select NamedWrite.Undoable);

    private static IO<bool> Same(RhinoDoc doc, Option<RhinoViewport> prior, RhinoViewport staged) =>
        prior.Match(
            Some: held =>
                from tolerances in Tolerances.Read(doc, Space.Model)
                from stored in Cameras.Read(held)
                from next in Cameras.Read(staged)
                from heldMode in ModeId(held)
                from nextMode in ModeId(staged)
                select stored.Matches(next, tolerances) && heldMode == nextMode,
            None: static () => IO.pure(value: false));

    private static IO<Guid> ModeId(RhinoViewport viewport) =>
        use(() => new ViewInfo(viewport)).Map(static view => view.DisplayModeId).Bracket();

    // --- [RESTORE]
    internal static IO<Unit> Applied(RhinoDoc doc, int index, RhinoViewport viewport, RestorePacing pacing) =>
        from restored in IO.lift(() => Restored(doc.NamedViews, index, viewport, pacing))
        from stored in Products.Of(() => doc.NamedViews[index]).Map(static view => view.DisplayModeId).Bracket()
        from moded in when(ViewSettings.DefinedViewSetDisplayMode, Moded(viewport, stored)).As()
        select moded;

    private static IO<Unit> Moded(RhinoViewport viewport, Guid id) =>
        Conversions.Present(id).Traverse(found => Products.Of(() => DisplayModeDescription.GetDisplayMode(found)).Bind(mode => IO.lift(() => { viewport.DisplayMode = mode; })).Bracket())
            .As().Map(static _ => unit);

    private static Fin<Unit> Restored(NamedViewTable table, int index, RhinoViewport viewport, RestorePacing pacing) =>
        pacing.Switch(
            (Table: table, Index: index, Viewport: viewport),
            immediate: static (at, _) => RhinoFault.Unless(at.Table.Restore(at.Index, at.Viewport), RhinoReason.Declined, at.Index),
            matchAspect: static (at, _) => RhinoFault.Unless(!OperatingSystem.IsMacOS() && at.Table.RestoreWithAspectRatio(at.Index, at.Viewport), RhinoReason.Unsupported, at.Index),
            constantSpeed: static (at, speed) =>
                from delay in Conversions.Whole(speed.Delay, Duration.FromMilliseconds(1))
                from restored in RhinoFault.Unless(at.Table.RestoreAnimatedConstantSpeed(at.Index, at.Viewport, speed.UnitsPerFrame, delay), RhinoReason.Declined, at.Index)
                select restored,
            constantTime: static (at, time) =>
                from delay in Conversions.Whole(time.Delay, Duration.FromMilliseconds(1))
                from restored in RhinoFault.Unless(at.Table.RestoreAnimatedConstantTime(at.Index, at.Viewport, time.Frames, delay), RhinoReason.Declined, at.Index)
                select restored);

    // --- [HOLD]
    internal static IO<NamedWrite> Held(RhinoDoc doc, ViewportSet set) =>
        Viewports.Resolve(doc, set).Bind(rows => rows.TraverseM(row => IO.lift(() => new ViewInfo(row.Viewport)).Map(held => new NamedWrite(
                Viewports.Edit(doc, new ViewportSet.One(new ViewportTarget.Id(row.Viewport.Id)), viewport => Pushed(viewport, held), new RedrawPolicy.AllViews(Deferred: true)).Map(unit),
                IO.lift(held.Dispose)))).As()).Bracket()
            .Map(static writes => new NamedWrite(
                Products.Each(writes.Map(static write => write.Revert)).Map(unit),
                Products.Each(writes.Map(static write => write.Release)).Map(unit)));

    private static IO<Unit> Pushed(RhinoViewport viewport, ViewInfo held) =>
        from pushed in Products.Accepted(() => viewport.PushViewInfo(held, includeTraceImage: true))
        from named in IO.lift(() => { viewport.Name = held.Name; })
        from moded in Moded(viewport, held.DisplayModeId)
        select moded;

    // --- [EXCHANGE]
    private static IO<NamedWrite> Transform(RhinoDoc doc, NamedEntry entry, Transform xform) =>
        Read(doc, entry.Name, Held).Bind(copy => Staged(doc, entry.Name, copy, Some(Moved(xform))));

    private static IO<NamedWrite> Import(RhinoDoc doc, InputPath path, Option<Seq<StateName>> names) =>
        Table.Imported(doc, path, names, static archive => archive.AllNamedViews, static view => view.Name,
            (view, scaling) => Held(view).Bind(copy => Staged(doc, view.Name, copy, Some(Detached(scaling)))));

    private static IO<Unit> Archive(RhinoDoc doc, Seq<NamedEntry> entries, File3dm archive) =>
        entries.TraverseM(entry => Read(doc, entry.Name, view => IO.lift(() => archive.AllNamedViews.Add(view)))).As().Map(static _ => unit);

    private static Func<RhinoViewport, ViewInfo, IO<Unit>> Moved(Transform xform) =>
        (copy, view) =>
            from dilation in IO.lift(() => NamedTable.Dilation(xform))
            from carried in IO.lift(() => Carried(copy, xform, dilation))
            from plane in ConstructionPlanes.Read(view)
            from moved in ConstructionPlanes.Moved(plane, xform)
            from written in ConstructionPlanes.Write(view, moved)
            from blurred in IO.lift(() => { view.FocalBlurDistance *= dilation; })
            select blurred;

    private static Func<RhinoViewport, ViewInfo, IO<Unit>> Detached(Transform scaling) =>
        (copy, view) => Moved(scaling)(copy, view).Bind(_ => IO.lift(() => { view.NamedViewId = Guid.Empty; }));

    private static Fin<Unit> Carried(RhinoViewport viewport, Transform xform, double dilation) {
        using ViewportInfo camera = new(viewport);
        return RhinoFault.Unless(camera.TransformCamera(xform), RhinoReason.Declined, xform)
            .Bind(_ => RhinoFault.Unless(
                camera.GetFrustum(out double left, out double right, out double bottom, out double top, out double near, out double far)
                && camera.SetFrustum(left * dilation, right * dilation, bottom * dilation, top * dilation, near * dilation, far * dilation), RhinoReason.Declined, dilation))
            .Bind(_ => Cameras.Applied(viewport, camera));
    }

    // --- [STORED]
    public static IO<TValue> Read<TValue>(RhinoDoc doc, string name, Func<ViewInfo, IO<TValue>> read) =>
        Find(doc, name).Bind(index => Products.Of(() => doc.NamedViews[index]).Bind(read).Bracket());

    internal static IO<Unit> Reassigned(RhinoDoc doc, HashMap<Guid, Guid> moves) =>
        Rows(doc, view => moves.Find(view.DisplayModeId).Traverse(target => IO.lift(() => {
            view.DisplayModeId = target;
            return Conversions.Required(Conversions.Present(doc.NamedViews.Add(view)));
        })).As()).Map(static _ => unit);

    private static IO<Option<RhinoViewport>> Stored(RhinoDoc doc, string name) =>
        IO.lift(() => Conversions.Present(doc.NamedViews.FindByName(name))).Bind(found => found.Traverse(index =>
            Products.Of(() => doc.NamedViews[index]).Bind(Held).Bracket()).As());

    private static IO<RhinoViewport> Held(ViewInfo view) =>
        Products.Transfer(static () => new RhinoViewport(), held =>
            from pushed in Products.Accepted(() => held.PushViewInfo(view, includeTraceImage: true))
            from moded in use(() => new ViewInfo(held)).Bind(copy => IO.lift(() => { copy.DisplayModeId = view.DisplayModeId; })).Bracket()
            select held);

    private static Func<Option<RhinoViewport>, IO<Unit>> Put(RhinoDoc doc, string name) =>
        row => row.Match(
            Some: held => (from view in use(() => new ViewInfo(held) { Name = name })
                           from added in IO.lift(() => Conversions.Required(Conversions.Present(doc.NamedViews.Add(view))))
                           select unit).Bracket(),
            None: () => Products.Accepted(() => doc.NamedViews.Delete(name)));

    private static IO<Seq<T>> Rows<T>(RhinoDoc doc, Func<ViewInfo, IO<T>> read) =>
        IO.lift(() => toSeq(Range(0, doc.NamedViews.Count))).Bind(indices => indices.TraverseM(index =>
            Products.Of(() => doc.NamedViews[index]).Bind(read).Bracket()).As());

    private static NamedEntry Entry(ViewInfo view) =>
        new(Table, view.Name, Conversions.Present(view.NamedViewId));
}
