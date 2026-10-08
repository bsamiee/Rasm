using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record RestorePacing {
    public sealed record Immediate() : RestorePacing;

    public sealed record MatchAspect() : RestorePacing;

    public sealed record ConstantSpeed(double UnitsPerFrame, Duration Delay) : RestorePacing;

    public sealed record ConstantTime(FrameCount Frames, Duration Delay) : RestorePacing;
}

public sealed record FocalBlur(ViewInfoFocalBlurModes FocalBlurMode, double FocalBlurDistance, double FocalBlurAperture, double FocalBlurJitter, uint FocalBlurSampleCount);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class NamedViewMapper {
    internal static partial FocalBlur ToFocalBlur(ViewInfo view);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update(FocalBlur blur, ViewInfo view);
}

public static class NamedViews {
    // --- [TABLE]
    public static IO<int> Find(RhinoDoc doc, string name) =>
        IO.lift(() => Conversions.Present(doc.NamedViews.FindByName(name)).ToFin(new Missing(nameof(NamedViewTable.FindByName))));

    public static IO<int> Add(RhinoDoc doc, RhinoViewport viewport, Option<string> name) =>
        IO.lift(() => Conversions.Required(doc.NamedViews.Add(Conversions.Unset(name), viewport.Id), nameof(NamedViewTable.Add)));

    public static IO<Unit> Rename(RhinoDoc doc, string name, string newName) =>
        Find(doc, name).Bind(index => IO.lift(() => Refused.Unless(doc.NamedViews.Rename(index, newName), nameof(NamedViewTable.Rename))));

    public static IO<Unit> Delete(RhinoDoc doc, string name) =>
        Find(doc, name).Bind(index => IO.lift(() => Refused.Unless(doc.NamedViews.Delete(index), nameof(NamedViewTable.Delete))));

    // --- [RESTORE]
    public static IO<Seq<bool>> Restore(RhinoDoc doc, ViewportSet viewports, string name, RestorePacing pacing, RedrawPolicy redraw) =>
        Find(doc, name).Bind(index => Navigation.ApplyToViewports(doc, viewports, port => Restored(doc.NamedViews, index, port, pacing), redraw));

    private static Fin<Unit> Restored(NamedViewTable table, int index, RhinoViewport viewport, RestorePacing pacing) =>
        viewport.Name switch {
            var held => pacing.Switch(
                    (Table: table, Index: index, Viewport: viewport),
                    immediate: static (target, _) => Refused.Unless(target.Table.Restore(target.Index, target.Viewport), nameof(NamedViewTable.Restore)),
                    matchAspect: static (target, _) => Refused.Unless(target.Table.RestoreWithAspectRatio(target.Index, target.Viewport), nameof(NamedViewTable.RestoreWithAspectRatio)),
                    constantSpeed: static (target, speed) =>
                        Conversions.Whole(speed.Delay, Duration.FromMilliseconds(1)).Bind(delay => Refused.Unless(
                            target.Table.RestoreAnimatedConstantSpeed(target.Index, target.Viewport, speed.UnitsPerFrame, delay),
                            nameof(NamedViewTable.RestoreAnimatedConstantSpeed))),
                    constantTime: static (target, time) =>
                        Conversions.Whole(time.Delay, Duration.FromMilliseconds(1)).Bind(delay => Refused.Unless(
                            target.Table.RestoreAnimatedConstantTime(target.Index, target.Viewport, time.Frames, delay),
                            nameof(NamedViewTable.RestoreAnimatedConstantTime))))
                .Map(fun((Unit _) => { viewport.Name = held; })),
        };

    // --- [STORED]
    public static IO<TValue> Read<TValue>(RhinoDoc doc, string name, Func<ViewInfo, IO<TValue>> read) =>
        use(Stored(doc, name)).Bind(read).Bracket();

    public static IO<int> Update(RhinoDoc doc, string name, Func<ViewInfo, IO<Unit>> edit) =>
        (from view in use(Stored(doc, name))
         from edited in edit(view)
         from index in IO.lift(() => Conversions.Required(doc.NamedViews.Add(view), nameof(NamedViewTable.Add)))
         select index).Bracket();

    public static IO<FocalBlur> ReadFocalBlur(RhinoDoc doc, string name) =>
        Read(doc, name, static view => IO.lift(() => NamedViewMapper.ToFocalBlur(view)));

    public static IO<int> WriteFocalBlur(RhinoDoc doc, string name, FocalBlur blur) =>
        Update(doc, name, view => IO.lift(() => NamedViewMapper.Update(blur, view)));

    private static IO<ViewInfo> Stored(RhinoDoc doc, string name) =>
        Find(doc, name).Bind(index => IO.lift(() => Missing.Unless(doc.NamedViews[index], nameof(NamedViewTable))));
}
