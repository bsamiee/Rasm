using Rasm.Rhino.Document;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RestoreAnimation {
    public sealed record Instant() : RestoreAnimation;

    public sealed record MatchAspect() : RestoreAnimation;

    public sealed record ConstantSpeed : RestoreAnimation {
        private ConstantSpeed(double unitsPerFrame, int delayMilliseconds) => (UnitsPerFrame, DelayMilliseconds) = (unitsPerFrame, delayMilliseconds);

        public double UnitsPerFrame { get; }

        public int DelayMilliseconds { get; }

        public static Fin<RestoreAnimation> Create(double unitsPerFrame, Duration delay) =>
            Milliseconds(delay).Map<RestoreAnimation>(milliseconds => new ConstantSpeed(unitsPerFrame, milliseconds));
    }

    public sealed record ConstantTime : RestoreAnimation {
        private ConstantTime(int frames, int delayMilliseconds) => (Frames, DelayMilliseconds) = (frames, delayMilliseconds);

        public int Frames { get; }

        public int DelayMilliseconds { get; }

        public static Fin<RestoreAnimation> Create(int frames, Duration delay) =>
            Milliseconds(delay).Map<RestoreAnimation>(milliseconds => new ConstantTime(frames, milliseconds));
    }

    public Fin<Unit> Apply(NamedViewTable table, int index, RhinoViewport viewport) =>
        Switch(
            (Table: table, Index: index, Viewport: viewport),
            instant: static (target, _) => Refused.Unless(target.Table.Restore(target.Index, target.Viewport), nameof(NamedViewTable.Restore)),
            matchAspect: static (target, _) => Refused.Unless(target.Table.RestoreWithAspectRatio(target.Index, target.Viewport), nameof(NamedViewTable.RestoreWithAspectRatio)),
            constantSpeed: static (target, speed) => Refused.Unless(
                target.Table.RestoreAnimatedConstantSpeed(target.Index, target.Viewport, speed.UnitsPerFrame, speed.DelayMilliseconds),
                nameof(NamedViewTable.RestoreAnimatedConstantSpeed)),
            constantTime: static (target, time) => Refused.Unless(
                target.Table.RestoreAnimatedConstantTime(target.Index, target.Viewport, time.Frames, time.DelayMilliseconds),
                nameof(NamedViewTable.RestoreAnimatedConstantTime)));

    private static Fin<int> Milliseconds(Duration delay) =>
        Durations.Whole(delay, Duration.FromMilliseconds(1), Limits.AtLeast(0), nameof(delay));
}

public sealed record FocalBlur(ViewInfoFocalBlurModes FocalBlurMode, double FocalBlurDistance, double FocalBlurAperture, double FocalBlurJitter, uint FocalBlurSampleCount);

public sealed record DefinedViewSettings(bool CPlane, bool Projection, bool ClippingPlanes, bool DisplayMode);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class NamedViewMapper {
    internal static partial FocalBlur ToFocalBlur(ViewInfo view);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    internal static partial void Update(FocalBlur value, ViewInfo view);
}

public static class NamedViews {
    // --- [ROWS]
    public static IO<int> Add(RhinoDoc document, RhinoViewport viewport, string name) =>
        IO.lift(() => Answers.Required(document.NamedViews.Add(name, viewport.Id), nameof(NamedViewTable.Add)));

    public static IO<Unit> Restore(RhinoDoc document, ViewportRef row, string name, RestoreAnimation animation) =>
        from index in Resolve(document, name)
        from restored in Navigation.ApplyToRows(
            document,
            Seq(row),
            port =>
                from held in IO.lift(() => port.Name)
                from applied in IO.lift(() => animation.Apply(document.NamedViews, index, port))
                from renamed in IO.lift(() => { port.Name = held; })
                select renamed,
            new RedrawPolicy.Silent())
        select unit;

    public static IO<Unit> Rename(RhinoDoc document, string name, string newName) =>
        IO.lift(() => Refused.Unless(document.NamedViews.Rename(name, newName), nameof(NamedViewTable.Rename)));

    private static IO<int> Resolve(RhinoDoc document, string name) =>
        IO.lift(() => Answers.Present(document.NamedViews.FindByName(name)).ToFin(new Missing(nameof(NamedViewTable.FindByName))));

    // --- [FOCAL_BLUR]
    public static IO<FocalBlur> ReadFocalBlur(RhinoDoc document, string name) =>
        WithView(document, name, static view => IO.lift(() => NamedViewMapper.ToFocalBlur(view)));

    public static IO<int> WriteFocalBlur(RhinoDoc document, string name, FocalBlur value) =>
        WithView(document, name, view =>
            from updated in IO.lift(() => NamedViewMapper.Update(value, view))
            from index in IO.lift(() => Answers.Required(document.NamedViews.Add(view), nameof(NamedViewTable.Add)))
            select index);

    private static IO<TValue> WithView<TValue>(RhinoDoc document, string name, Func<ViewInfo, IO<TValue>> body) =>
        DisposalOps.Using(Resolve(document, name).Bind(index => IO.lift(() => Missing.Unless(document.NamedViews[index], nameof(NamedViewTable)))), body);

    // --- [DEFINED_VIEWS]
    public static IO<TValue> WithDefinedViewSettings<TValue>(DefinedViewSettings settings, IO<TValue> body) =>
        IO.lift(static () => new DefinedViewSettings(ViewSettings.DefinedViewSetCPlane, ViewSettings.DefinedViewSetProjection, ViewSettings.DefinedViewSetClippingPlanes, ViewSettings.DefinedViewSetDisplayMode))
            .Bracket(Use: _ => IO.lift(() => WriteDefinedViewSettings(settings)).Bind(_ => body), Fin: static prior => IO.lift(() => WriteDefinedViewSettings(prior)));

    private static void WriteDefinedViewSettings(DefinedViewSettings settings) {
        ViewSettings.DefinedViewSetCPlane = settings.CPlane;
        ViewSettings.DefinedViewSetProjection = settings.Projection;
        ViewSettings.DefinedViewSetClippingPlanes = settings.ClippingPlanes;
        ViewSettings.DefinedViewSetDisplayMode = settings.DisplayMode;
    }
}
