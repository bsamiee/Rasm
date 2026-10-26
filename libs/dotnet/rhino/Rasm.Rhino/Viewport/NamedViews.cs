using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record RestorePacing {
    public sealed record Immediate() : RestorePacing;

    public sealed record MatchAspect() : RestorePacing;

    public sealed record ConstantSpeed(double UnitsPerFrame, Duration Delay) : RestorePacing;

    public sealed record ConstantTime(FrameCount Frames, Duration Delay) : RestorePacing;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class NamedViews {
    // --- [TABLE]
    public static IO<int> Add(RhinoDoc doc, RhinoViewport viewport, Option<string> name) =>
        IO.lift(() => Conversions.Required(doc.NamedViews.Add(Conversions.Unset(name), viewport.Id), nameof(NamedViewTable.Add)));

    public static IO<int> Save(RhinoDoc doc, RhinoViewport source, Option<string> name, Func<RhinoViewport, IO<Unit>> stage) =>
        DisposalOps.Within(() => new RhinoViewport(source), copy =>
            from staged in stage(copy)
            from saved in DisposalOps.Within(() => new ViewInfo(copy) { Name = Conversions.Unset(name) },
                view => IO.lift(() => Conversions.Required(doc.NamedViews.Add(view), nameof(NamedViewTable.Add))))
            select saved);

    public static IO<Unit> Rename(RhinoDoc doc, string name, string newName) =>
        IO.lift(() => Refused.Unless(doc.NamedViews.Rename(name, newName), nameof(NamedViewTable.Rename)));

    public static IO<Unit> Delete(RhinoDoc doc, string name) =>
        IO.lift(() => Refused.Unless(doc.NamedViews.Delete(name), nameof(NamedViewTable.Delete)));

    private static IO<int> Find(RhinoDoc doc, string name) =>
        IO.lift(() => Conversions.Present(doc.NamedViews.FindByName(name)).ToFin(new Refused(nameof(NamedViewTable.FindByName), Refused.Absent)));

    // --- [RESTORE]
    public static IO<Seq<bool>> Restore(RhinoDoc doc, ViewportSet viewports, string name, RestorePacing pacing, RedrawPolicy redraw) =>
        from index in Find(doc, name)
        from restored in Viewports.Edit(
            doc,
            viewports,
            viewport => DisposalOps.Within(() => viewport.Name,
                _ => IO.lift(() => Restored(doc.NamedViews, index, viewport, pacing)),
                held => IO.lift(() => { viewport.Name = held; })),
            redraw)
        select restored;

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

    // --- [STORED]
    public static IO<TValue> Read<TValue>(RhinoDoc doc, string name, Func<ViewInfo, IO<TValue>> read) =>
        Find(doc, name).Bind(index => DisposalOps.Found(() => doc.NamedViews[index], nameof(NamedViewTable), read));
}
