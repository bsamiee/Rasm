using Rasm.Rhino.Display;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Settings;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.FileIO;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record RestorePacing {
    public sealed record Immediate() : RestorePacing;

    public sealed record MatchAspect() : RestorePacing;

    public sealed record ConstantSpeed(double UnitsPerFrame, Duration Delay) : RestorePacing;

    public sealed record ConstantTime(int Frames, Duration Delay) : RestorePacing;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class NamedViews {
    // --- [ROWS]
    internal static IO<Seq<NamedEntry>> Entries(RhinoDoc doc) =>
        Rows(doc, static view => IO.pure(Entry(view)));

    internal static IO<Option<NamedEntry>> Current(RhinoDoc doc, ViewportTarget target) =>
        from pose in Viewports.With(doc, target, static row => Cameras.Read(row.Viewport))
        from tolerances in Tolerances.Read(doc, modelUnits: true)
        from matched in Rows(doc, view => Cameras.Read(view.Viewport).Map(stored => Callbacks.Found(stored.Matches(pose, tolerances), Entry(view))))
        select matched.Somes().Head;

    internal static IO<NamedWrite> Rename(RhinoDoc doc, NamedEntry entry, StateName name, CallbackSite site) =>
        from found in Find(doc, entry.Name)
        from renamed in TableOps.Undoable(doc, $"{nameof(Rename)} {entry.Name}", IO.pure((From: (string)name, To: entry.Name)),
            names => IO.lift(() => Refused.Unless(doc.NamedViews.Rename(names.From, names.To), nameof(NamedViewTable.Rename))),
            (From: entry.Name, To: (string)name), site)
        select (NamedWrite)new NamedWrite.Recorded();

    internal static IO<NamedWrite> Delete(RhinoDoc doc, NamedEntry entry, CallbackSite site) =>
        from prior in Read(doc, entry.Name, Held)
        from deleted in DisposalOps.OnFailure(
            TableOps.Undoable(doc, $"{nameof(Delete)} {entry.Name}", IO.pure(Some(prior)), Put(doc, entry.Name), Option<RhinoViewport>.None, site),
            IO.lift(prior.Dispose))
        select (NamedWrite)new NamedWrite.Recorded();

    internal static IO<int> Find(RhinoDoc doc, string name) =>
        IO.lift(() => Conversions.Present(doc.NamedViews.FindByName(name)).ToFin(new NotFound(nameof(NamedTable.Views), name)));

    // --- [SAVE]
    internal static IO<NamedWrite> Save(RhinoDoc doc, NamedState.View state, CallbackSite site) =>
        from copy in Viewports.With(doc, state.Source, static row => IO.lift(() => new RhinoViewport(row.Viewport)))
        from written in Staged(doc, nameof(Save), state.Name, copy, state.Stage, site)
        select written;

    private static IO<NamedWrite> Staged(RhinoDoc doc, string verb, string name, RhinoViewport copy, Option<Func<RhinoViewport, ViewInfo, IO<Unit>>> stage, CallbackSite site) =>
        DisposalOps.OnFailure(
            from staged in DisposalOps.Within(() => new ViewInfo(copy), view => stage.Traverse(run => run(copy, view)).As())
            from prior in Stored(doc, name)
            from same in DisposalOps.OnFailure(Same(doc, prior, copy), DisposalOps.Release(prior.ToSeq()))
            from written in same
                ? DisposalOps.Release(prior.ToSeq().Add(copy))
                : TableOps.Undoable(doc, $"{verb} {name}", IO.pure(prior), Put(doc, name), Some(copy), site)
            select (NamedWrite)new NamedWrite.Recorded(),
            IO.lift(copy.Dispose));

    private static IO<bool> Same(RhinoDoc doc, Option<RhinoViewport> prior, RhinoViewport staged) =>
        prior.Match(
            Some: held =>
                from tolerances in Tolerances.Read(doc, modelUnits: true)
                from stored in Cameras.Read(held)
                from next in Cameras.Read(staged)
                from heldMode in ModeId(held)
                from nextMode in ModeId(staged)
                select stored.Matches(next, tolerances) && heldMode == nextMode,
            None: static () => IO.pure(value: false));

    private static IO<Guid> ModeId(RhinoViewport viewport) =>
        DisposalOps.Within(() => new ViewInfo(viewport), static view => IO.pure(view.DisplayModeId));

    // --- [RESTORE]
    internal static IO<NamedWrite> Restore(RhinoDoc doc, NamedRestore.View target) =>
        from index in Find(doc, target.Name)
        from held in Hold(doc, Some(target.Viewports))
        from restored in DisposalOps.OnFailure(
            Viewports.Edit(
                doc,
                target.Viewports,
                viewport => DisposalOps.Within(() => viewport.Name,
                    _ => Applied(doc, index, viewport, target.Pacing),
                    name => IO.lift(() => { viewport.Name = name; })),
                new RedrawPolicy.Silent()),
            NamedWrite.Settled([held]))
        select held;

    internal static IO<Unit> Applied(RhinoDoc doc, int index, RhinoViewport viewport, RestorePacing pacing) =>
        from restored in IO.lift(() => Restored(doc.NamedViews, index, viewport, pacing))
        from settings in AppSettings.DefinedViews.Read
        from stored in DisposalOps.Found(() => doc.NamedViews[index], nameof(NamedViewTable), static view => IO.pure(view.DisplayModeId))
        from moded in when(settings.DefinedViewSetDisplayMode, Moded(viewport, stored)).As()
        select moded;

    private static IO<Unit> Moded(RhinoViewport viewport, Guid id) =>
        Conversions.Present(id).Traverse(found => DisplayModes.With(found, mode => IO.lift(() => { viewport.DisplayMode = mode; }))).As().Map(static _ => unit);

    private static Fin<Unit> Restored(NamedViewTable table, int index, RhinoViewport viewport, RestorePacing pacing) =>
        pacing.Switch(
            (Table: table, Index: index, Viewport: viewport),
            immediate: static (at, _) => Refused.Unless(at.Table.Restore(at.Index, at.Viewport), nameof(NamedViewTable.Restore)),
            matchAspect: static (at, _) => Refused.Unless(!OperatingSystem.IsMacOS() && at.Table.RestoreWithAspectRatio(at.Index, at.Viewport), nameof(NamedViewTable.RestoreWithAspectRatio)),
            constantSpeed: static (at, speed) =>
                from delay in Conversions.Whole(speed.Delay, Duration.FromMilliseconds(1))
                from restored in Refused.Unless(at.Table.RestoreAnimatedConstantSpeed(at.Index, at.Viewport, speed.UnitsPerFrame, delay), nameof(NamedViewTable.RestoreAnimatedConstantSpeed))
                select restored,
            constantTime: static (at, time) =>
                from delay in Conversions.Whole(time.Delay, Duration.FromMilliseconds(1))
                from restored in Refused.Unless(at.Table.RestoreAnimatedConstantTime(at.Index, at.Viewport, time.Frames, delay), nameof(NamedViewTable.RestoreAnimatedConstantTime))
                select restored);

    // --- [HOLD]
    internal static IO<NamedWrite> Hold(RhinoDoc doc, Option<ViewportSet> scope) =>
        scope.IfNone(static () => new ViewportSet.One(new ViewportTarget.Active())) switch {
            var set =>
                from held in DisposalOps.Within(
                    (Action<IDisposable> retain) => Viewports.Resolve(doc, set, retain).Map(static rows => toHashMap(rows.Map(static row => (row.Viewport.Id, new RhinoViewport(row.Viewport))))),
                    DisposalOps.Release)
                select (NamedWrite)new NamedWrite.Compensated(
                    Viewports.Edit(doc, set, viewport => held.Find(viewport.Id).Traverse(copy => Pushed(viewport, copy)).As().Map(static _ => unit), new RedrawPolicy.AllViews(Deferred: true))
                        .Map(static _ => unit),
                    DisposalOps.Release(toSeq(held.Values))),
        };

    private static IO<Unit> Pushed(RhinoViewport viewport, RhinoViewport copy) =>
        DisposalOps.Within(() => new ViewInfo(copy), view =>
            from pushed in IO.lift(() => Refused.Unless(viewport.PushViewInfo(view, includeTraceImage: true), nameof(RhinoViewport.PushViewInfo)))
            from named in IO.lift(() => { viewport.Name = copy.Name; })
            from moded in Moded(viewport, view.DisplayModeId)
            select moded);

    // --- [EXCHANGE]
    internal static IO<NamedWrite> Transform(RhinoDoc doc, NamedEntry entry, Transform xform, CallbackSite site) =>
        from copy in Read(doc, entry.Name, Held)
        from written in Staged(doc, nameof(Transform), entry.Name, copy, Some(Moved(xform)), site)
        select written;

    internal static IO<NamedWrite> Import(RhinoDoc doc, ArchiveSource source, Option<Seq<StateName>> names, CallbackSite site) =>
        NamedTable.Views.Imported(doc, source, names, static archive => archive.AllNamedViews, static view => view.Name,
            (view, scaling) => Held(view).Bind(copy => Staged(doc, nameof(Import), view.Name, copy, Some(Detached(scaling)), site)));

    internal static IO<Unit> Archive(RhinoDoc doc, Seq<NamedEntry> entries, File3dm archive) =>
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
        return Refused.Unless(camera.TransformCamera(xform), nameof(ViewportInfo.TransformCamera))
            .Bind(_ => Refused.Unless(
                camera.GetFrustum(out double left, out double right, out double bottom, out double top, out double near, out double far)
                && camera.SetFrustum(left * dilation, right * dilation, bottom * dilation, top * dilation, near * dilation, far * dilation),
                nameof(ViewportInfo.SetFrustum)))
            .Bind(_ => Refused.Unless(viewport.SetViewProjection(camera, updateTargetLocation: false), nameof(RhinoViewport.SetViewProjection)))
            .Map(fun((Unit _) => viewport.SetCameraTarget(camera.TargetPoint, updateCameraLocation: false)));
    }

    // --- [STORED]
    public static IO<TValue> Read<TValue>(RhinoDoc doc, string name, Func<ViewInfo, IO<TValue>> read) =>
        Find(doc, name).Bind(index => DisposalOps.Found(() => doc.NamedViews[index], nameof(NamedViewTable), read));

    internal static IO<Unit> Reassigned(RhinoDoc doc, HashMap<Guid, Guid> moves) =>
        Rows(doc, view => moves.Find(view.DisplayModeId).Traverse(target => IO.lift(Fin<int> () => {
            view.DisplayModeId = target;
            return Conversions.Required(doc.NamedViews.Add(view), nameof(NamedViewTable.Add));
        })).As()).Map(static _ => unit);

    private static IO<Option<RhinoViewport>> Stored(RhinoDoc doc, string name) =>
        IO.lift(() => Conversions.Present(doc.NamedViews.FindByName(name))).Bind(found => found.Traverse(index =>
            DisposalOps.Found(() => doc.NamedViews[index], nameof(NamedViewTable), Held)).As());

    private static IO<RhinoViewport> Held(ViewInfo view) =>
        IO.lift(static () => new RhinoViewport()).Bind(held =>
            DisposalOps.OnFailure(
                from pushed in IO.lift(() => Refused.Unless(held.PushViewInfo(view, includeTraceImage: true), nameof(RhinoViewport.PushViewInfo)))
                from moded in DisposalOps.Within(() => new ViewInfo(held), copy => IO.lift(() => { copy.DisplayModeId = view.DisplayModeId; }))
                select held,
                IO.lift(held.Dispose)));

    private static Func<Option<RhinoViewport>, IO<Unit>> Put(RhinoDoc doc, string name) =>
        row => row.Match(
            Some: held => DisposalOps.Within(() => new ViewInfo(held) { Name = name },
                view => IO.lift(() => Conversions.Required(doc.NamedViews.Add(view), nameof(NamedViewTable.Add)).Map(static _ => unit))),
            None: () => IO.lift(() => Refused.Unless(doc.NamedViews.Delete(name), nameof(NamedViewTable.Delete))));

    private static IO<Seq<T>> Rows<T>(RhinoDoc doc, Func<ViewInfo, IO<T>> read) =>
        IO.lift(() => toSeq(Range(0, doc.NamedViews.Count))).Bind(indices => indices.TraverseM(index =>
            DisposalOps.Found(() => doc.NamedViews[index], nameof(NamedViewTable), read)).As());

    private static NamedEntry Entry(ViewInfo view) =>
        new(NamedTable.Views, view.Name, Conversions.Present(view.NamedViewId));
}
