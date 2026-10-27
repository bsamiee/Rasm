using System.Runtime.CompilerServices;
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
        from matched in Rows(doc, view => Cameras.Read(view.Viewport).Map(stored => Conversions.Found(stored.Matches(pose, tolerances), Entry(view))))
        select matched.Somes().Head;

    internal static IO<NamedWrite> Rename(RhinoDoc doc, NamedEntry entry, StateName name, object owner) =>
        from found in Find(doc, entry.Name)
        from renamed in TableOps.Undoable(doc, $"{nameof(Rename)} {entry.Name}", IO.pure((From: (string)name, To: entry.Name)),
            names => IO.lift(() => RhinoFault.Unless(doc.NamedViews.Rename(names.From, names.To))),
            (From: entry.Name, To: (string)name), owner)
        select (NamedWrite)new NamedWrite.Recorded();

    internal static IO<NamedWrite> Delete(RhinoDoc doc, NamedEntry entry, object owner) =>
        from prior in Read(doc, entry.Name, Held)
        from deleted in TableOps.Undoable(doc, $"{nameof(Delete)} {entry.Name}", IO.pure(Some(prior)), Put(doc, entry.Name), Option<RhinoViewport>.None, owner)
            .Catch(error => IO.lift(prior.Dispose) >> IO.fail<Unit>(error)).As()
        select (NamedWrite)new NamedWrite.Recorded();

    internal static IO<int> Find(RhinoDoc doc, string name) =>
        IO.lift(() => Conversions.Present(doc.NamedViews.FindByName(name)).ToFin(new RhinoFault(name, RhinoReason.Missing)));

    // --- [SAVE]
    internal static IO<NamedWrite> Save(RhinoDoc doc, NamedState.View state, object owner) =>
        from copy in Viewports.With(doc, state.Source, static row => IO.lift(() => new RhinoViewport(row.Viewport)))
        from written in Staged(doc, state.Name, copy, state.Stage, owner)
        select written;

    private static IO<NamedWrite> Staged(RhinoDoc doc, string name, RhinoViewport copy, Option<Func<RhinoViewport, ViewInfo, IO<Unit>>> stage, object owner, [CallerMemberName] string verb = "") =>
        (from staged in use(() => new ViewInfo(copy)).Bind(view => stage.Traverse(run => run(copy, view)).As()).Bracket()
         from prior in Stored(doc, name)
         from same in Same(doc, prior, copy).Catch(error => Released(prior.ToSeq()) >> IO.fail<bool>(error)).As()
         from written in same
             ? Released(prior.ToSeq().Add(copy))
             : TableOps.Undoable(doc, $"{verb} {name}", IO.pure(prior), Put(doc, name), Some(copy), owner)
         select (NamedWrite)new NamedWrite.Recorded())
        .Catch(error => IO.lift(copy.Dispose) >> IO.fail<NamedWrite>(error)).As();

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
        use(() => new ViewInfo(viewport)).Map(static view => view.DisplayModeId).Bracket();

    // --- [RESTORE]
    internal static IO<NamedWrite> Restore(RhinoDoc doc, NamedRestore.View target) =>
        from index in Find(doc, target.Name)
        from held in Hold(doc, Some(target.Viewports))
        from restored in Viewports.Edit(
                doc,
                target.Viewports,
                viewport => IO.lift(() => viewport.Name).Bind(name => Applied(doc, index, viewport, target.Pacing).Finally(IO.lift(() => { viewport.Name = name; }))),
                new RedrawPolicy.Silent())
            .Catch(error => NamedWrite.Settled([held]) >> IO.fail<Seq<bool>>(error)).As()
        select held;

    internal static IO<Unit> Applied(RhinoDoc doc, int index, RhinoViewport viewport, RestorePacing pacing) =>
        from restored in IO.lift(() => Restored(doc.NamedViews, index, viewport, pacing))
        from settings in AppSettings.DefinedViews.Read
        from stored in Products.Made(doc.NamedViews[index]).Map(static view => view.DisplayModeId).Bracket()
        from moded in when(settings.DefinedViewSetDisplayMode, Moded(viewport, stored)).As()
        select moded;

    private static IO<Unit> Moded(RhinoViewport viewport, Guid id) =>
        Conversions.Present(id).Traverse(found => DisplayModes.With(found, mode => IO.lift(() => { viewport.DisplayMode = mode; }))).As().Map(static _ => unit);

    private static Fin<Unit> Restored(NamedViewTable table, int index, RhinoViewport viewport, RestorePacing pacing) =>
        pacing.Switch(
            (Table: table, Index: index, Viewport: viewport),
            immediate: static (at, _) => RhinoFault.Unless(at.Table.Restore(at.Index, at.Viewport)),
            matchAspect: static (at, _) => RhinoFault.Unless(!OperatingSystem.IsMacOS() && at.Table.RestoreWithAspectRatio(at.Index, at.Viewport)),
            constantSpeed: static (at, speed) =>
                from delay in Conversions.Whole(speed.Delay, Duration.FromMilliseconds(1))
                from restored in RhinoFault.Unless(at.Table.RestoreAnimatedConstantSpeed(at.Index, at.Viewport, speed.UnitsPerFrame, delay))
                select restored,
            constantTime: static (at, time) =>
                from delay in Conversions.Whole(time.Delay, Duration.FromMilliseconds(1))
                from restored in RhinoFault.Unless(at.Table.RestoreAnimatedConstantTime(at.Index, at.Viewport, time.Frames, delay))
                select restored);

    // --- [HOLD]
    internal static IO<NamedWrite> Hold(RhinoDoc doc, Option<ViewportSet> scope) =>
        scope.IfNone(static () => new ViewportSet.One(new ViewportTarget.Active())) switch {
            var set =>
                from held in Viewports.Resolve(doc, set).Map(static rows => toHashMap(rows.Map(static row => (row.Viewport.Id, new RhinoViewport(row.Viewport))))).Bracket()
                select (NamedWrite)new NamedWrite.Compensated(
                    Viewports.Edit(doc, set, viewport => held.Find(viewport.Id).Traverse(copy => Pushed(viewport, copy)).As().Map(static _ => unit), new RedrawPolicy.AllViews(Deferred: true))
                        .Map(static _ => unit),
                    Released(toSeq(held.Values))),
        };

    private static IO<Unit> Pushed(RhinoViewport viewport, RhinoViewport copy) =>
        (from view in use(() => new ViewInfo(copy))
         from pushed in IO.lift(() => RhinoFault.Unless(viewport.PushViewInfo(view, includeTraceImage: true)))
         from named in IO.lift(() => { viewport.Name = copy.Name; })
         from moded in Moded(viewport, view.DisplayModeId)
         select moded).Bracket();

    private static IO<Unit> Released(Seq<RhinoViewport> copies) =>
        IO.lift(() => copies.Iter(static copy => copy.Dispose()));

    // --- [EXCHANGE]
    internal static IO<NamedWrite> Transform(RhinoDoc doc, NamedEntry entry, Transform xform, object owner) =>
        from copy in Read(doc, entry.Name, Held)
        from written in Staged(doc, entry.Name, copy, Some(Moved(xform)), owner)
        select written;

    internal static IO<NamedWrite> Import(RhinoDoc doc, ArchiveSource source, Option<Seq<StateName>> names, object owner) =>
        NamedTable.Views.Imported(doc, source, names, static archive => archive.AllNamedViews, static view => view.Name,
            (view, scaling) => Held(view).Bind(copy => Staged(doc, view.Name, copy, Some(Detached(scaling)), owner)));

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
        return RhinoFault.Unless(camera.TransformCamera(xform))
            .Bind(_ => RhinoFault.Unless(
                camera.GetFrustum(out double left, out double right, out double bottom, out double top, out double near, out double far)
                && camera.SetFrustum(left * dilation, right * dilation, bottom * dilation, top * dilation, near * dilation, far * dilation)))
            .Bind(_ => RhinoFault.Unless(viewport.SetViewProjection(camera, updateTargetLocation: false)))
            .Map(fun((Unit _) => viewport.SetCameraTarget(camera.TargetPoint, updateCameraLocation: false)));
    }

    // --- [STORED]
    public static IO<TValue> Read<TValue>(RhinoDoc doc, string name, Func<ViewInfo, IO<TValue>> read) =>
        Find(doc, name).Bind(index => Products.Made(doc.NamedViews[index]).Bind(read).Bracket());

    internal static IO<Unit> Reassigned(RhinoDoc doc, HashMap<Guid, Guid> moves) =>
        Rows(doc, view => moves.Find(view.DisplayModeId).Traverse(target => IO.lift(() => {
            view.DisplayModeId = target;
            return Conversions.Found(Conversions.Present(doc.NamedViews.Add(view)));
        })).As()).Map(static _ => unit);

    private static IO<Option<RhinoViewport>> Stored(RhinoDoc doc, string name) =>
        IO.lift(() => Conversions.Present(doc.NamedViews.FindByName(name))).Bind(found => found.Traverse(index =>
            Products.Made(doc.NamedViews[index]).Bind(Held).Bracket()).As());

    private static IO<RhinoViewport> Held(ViewInfo view) =>
        IO.lift(static () => new RhinoViewport()).Bind(held =>
            (from pushed in IO.lift(() => RhinoFault.Unless(held.PushViewInfo(view, includeTraceImage: true)))
             from moded in use(() => new ViewInfo(held)).Bind(copy => IO.lift(() => { copy.DisplayModeId = view.DisplayModeId; })).Bracket()
             select held)
            .Catch(error => IO.lift(held.Dispose) >> IO.fail<RhinoViewport>(error)).As());

    private static Func<Option<RhinoViewport>, IO<Unit>> Put(RhinoDoc doc, string name) =>
        row => row.Match(
            Some: held => (from view in use(() => new ViewInfo(held) { Name = name })
                           from added in IO.lift(() => Conversions.Found(Conversions.Present(doc.NamedViews.Add(view))))
                           select unit).Bracket(),
            None: () => IO.lift(() => RhinoFault.Unless(doc.NamedViews.Delete(name))));

    private static IO<Seq<T>> Rows<T>(RhinoDoc doc, Func<ViewInfo, IO<T>> read) =>
        IO.lift(() => toSeq(Range(0, doc.NamedViews.Count))).Bind(indices => indices.TraverseM(index =>
            Products.Made(doc.NamedViews[index]).Bind(read).Bracket()).As());

    private static NamedEntry Entry(ViewInfo view) =>
        new(NamedTable.Views, view.Name, Conversions.Present(view.NamedViewId));
}
