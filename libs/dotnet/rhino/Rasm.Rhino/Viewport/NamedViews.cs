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

public sealed record FocalBlur(ViewInfoFocalBlurModes FocalBlurMode, double FocalBlurDistance, double FocalBlurAperture, double FocalBlurJitter, uint FocalBlurSampleCount) {
    public static IO<FocalBlur> Read(ViewInfo view) => IO.lift(() => NamedViewMapper.ToFocalBlur(view));

    public IO<Unit> Write(ViewInfo view) => IO.lift(() => NamedViewMapper.Update(this, view));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class NamedViewMapper {
    internal static partial FocalBlur ToFocalBlur(ViewInfo view);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update(FocalBlur blur, ViewInfo view);
}

public static class NamedViews {
    // --- [TABLE]
    public static IO<int> Add(RhinoDoc doc, RhinoViewport viewport, Option<string> name) =>
        IO.lift(() => Conversions.Required(doc.NamedViews.Add(Conversions.Unset(name), viewport.Id), nameof(NamedViewTable.Add)));

    public static IO<Unit> Rename(RhinoDoc doc, string name, string newName) =>
        Find(doc, name).Bind(index => IO.lift(() => Refused.Unless(doc.NamedViews.Rename(index, newName), nameof(NamedViewTable.Rename))));

    public static IO<Unit> Delete(RhinoDoc doc, string name) =>
        Find(doc, name).Bind(index => IO.lift(() => Refused.Unless(doc.NamedViews.Delete(index), nameof(NamedViewTable.Delete))));

    private static IO<int> Find(RhinoDoc doc, string name) =>
        IO.lift(() => Conversions.Present(doc.NamedViews.FindByName(name)).ToFin(new Missing(nameof(NamedViewTable.FindByName))));

    // --- [RESTORE]
    public static IO<Seq<bool>> Restore(RhinoDoc doc, ViewportSet viewports, string name, RestorePacing pacing, RedrawPolicy redraw) =>
        Find(doc, name).Bind(index => Navigation.ApplyToViewports(doc, viewports, viewport => {
            string held = viewport.Name;
            Fin<Unit> restored = pacing.Switch(
                (Table: doc.NamedViews, Index: index, Viewport: viewport),
                immediate: static (at, _) => Refused.Unless(at.Table.Restore(at.Index, at.Viewport), nameof(NamedViewTable.Restore)),
                matchAspect: static (at, _) => Refused.Unless(!OperatingSystem.IsMacOS() && at.Table.RestoreWithAspectRatio(at.Index, at.Viewport), nameof(NamedViewTable.RestoreWithAspectRatio)),
                constantSpeed: static (at, speed) => Conversions.Whole(speed.Delay, Duration.FromMilliseconds(1)).Bind(delay => Refused.Unless(at.Table.RestoreAnimatedConstantSpeed(at.Index, at.Viewport, speed.UnitsPerFrame, delay), nameof(NamedViewTable.RestoreAnimatedConstantSpeed))),
                constantTime: static (at, time) => Conversions.Whole(time.Delay, Duration.FromMilliseconds(1)).Bind(delay => Refused.Unless(at.Table.RestoreAnimatedConstantTime(at.Index, at.Viewport, time.Frames, delay), nameof(NamedViewTable.RestoreAnimatedConstantTime))));
            viewport.Name = held;
            return restored;
        }, redraw));

    // --- [STORED]
    public static IO<TValue> Read<TValue>(RhinoDoc doc, string name, Func<ViewInfo, IO<TValue>> read) =>
        use(Find(doc, name).Bind(index => IO.lift(() => Missing.Unless(doc.NamedViews[index], nameof(NamedViewTable))))).Bind(read).Bracket();

    public static IO<int> Update(RhinoDoc doc, string name, Func<ViewInfo, IO<Unit>> edit) =>
        Read(doc, name, view => edit(view).Bind(_ => IO.lift(() => Conversions.Required(doc.NamedViews.Add(view), nameof(NamedViewTable.Add)))));
}
