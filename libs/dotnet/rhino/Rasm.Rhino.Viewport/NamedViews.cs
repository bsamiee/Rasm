using Rasm.Rhino.Document;
using Rhino;
using Rhino.ApplicationSettings;
using Rhino.Display;
using Rhino.DocObjects.Tables;

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

    private static Fin<int> Milliseconds(Duration delay) =>
        Invalid.Unless((delay >= Duration.Zero) && (delay.TotalMilliseconds <= int.MaxValue) && (delay == Duration.FromMilliseconds((long)delay.TotalMilliseconds)), nameof(delay))
            .Map(_ => (int)delay.TotalMilliseconds);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NamedViewOp {
    public sealed record Restore(string Name, RestoreAnimation Animation) : NamedViewOp;

    public sealed record Add(string Name) : NamedViewOp;

    public sealed record Rename(string Name, string NewName) : NamedViewOp;

    public sealed record Delete(string Name) : NamedViewOp;
}

public sealed record DefinedViewSettings(bool CPlane, bool Projection, bool ClippingPlanes, bool DisplayMode);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class NamedViews {
    // --- [ROWS]
    public static IO<int> Apply(RhinoDoc document, ViewportRef row, NamedViewOp op) =>
        op.Switch(
            (Doc: document, Row: row),
            restore: static (state, restore) =>
                from index in Resolve(state.Doc, restore.Name)
                from restored in Navigation.ApplyToRows(state.Doc, Seq(state.Row), port => Restored(state.Doc, index, port, restore.Animation), new RedrawPolicy.Silent())
                select index,
            add: static (state, add) => IO.lift(() =>
                Answers.NonNegative(state.Doc.NamedViews.Add(add.Name, state.Row.Viewport.Id), nameof(NamedViewTable.Add))),
            rename: static (state, rename) =>
                from index in Resolve(state.Doc, rename.Name)
                from renamed in IO.lift(() => Refused.Unless(state.Doc.NamedViews.Rename(index, rename.NewName), nameof(NamedViewTable.Rename)))
                select index,
            delete: static (state, delete) =>
                from index in Resolve(state.Doc, delete.Name)
                from deleted in IO.lift(() => Refused.Unless(state.Doc.NamedViews.Delete(index), nameof(NamedViewTable.Delete)))
                select index);

    internal static IO<int> Resolve(RhinoDoc document, string name) =>
        IO.lift(() => Answers.Present(document.NamedViews.FindByName(name)).ToFin(new Missing(nameof(NamedViewTable.FindByName))));

    private static IO<Unit> Restored(RhinoDoc document, int index, RhinoViewport viewport, RestoreAnimation animation) =>
        IO.lift(() => animation.Switch(
            (Table: document.NamedViews, Index: index, Viewport: viewport),
            instant: static (target, _) => Refused.Unless(target.Table.Restore(target.Index, target.Viewport), nameof(NamedViewTable.Restore)),
            matchAspect: static (target, _) => Refused.Unless(target.Table.RestoreWithAspectRatio(target.Index, target.Viewport), nameof(NamedViewTable.RestoreWithAspectRatio)),
            constantSpeed: static (target, speed) => Refused.Unless(
                target.Table.RestoreAnimatedConstantSpeed(target.Index, target.Viewport, speed.UnitsPerFrame, speed.DelayMilliseconds),
                nameof(NamedViewTable.RestoreAnimatedConstantSpeed)),
            constantTime: static (target, time) => Refused.Unless(
                target.Table.RestoreAnimatedConstantTime(target.Index, target.Viewport, time.Frames, time.DelayMilliseconds),
                nameof(NamedViewTable.RestoreAnimatedConstantTime))));

    // --- [DEFINED_VIEWS]
    public static IO<TValue> WithDefinedViewSettings<TValue>(DefinedViewSettings settings, IO<TValue> body) =>
        Disposal.Bracketed(
            IO.lift(static () => new DefinedViewSettings(ViewSettings.DefinedViewSetCPlane, ViewSettings.DefinedViewSetProjection, ViewSettings.DefinedViewSetClippingPlanes, ViewSettings.DefinedViewSetDisplayMode)),
            static prior => IO.lift(() => WriteDefinedViewSettings(prior)),
            _ => IO.lift(() => WriteDefinedViewSettings(settings)).Bind(_ => body));

    private static void WriteDefinedViewSettings(DefinedViewSettings settings) {
        ViewSettings.DefinedViewSetCPlane = settings.CPlane;
        ViewSettings.DefinedViewSetProjection = settings.Projection;
        ViewSettings.DefinedViewSetClippingPlanes = settings.ClippingPlanes;
        ViewSettings.DefinedViewSetDisplayMode = settings.DisplayMode;
    }
}
