using System.Runtime.CompilerServices;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Styling;
using Rhino.Resources;

namespace Rasm.Rhino.UI.Drawing;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class PartRole {
    public static readonly PartRole Image = new(activates: false, adjusts: false);
    public static readonly PartRole Button = new(activates: true, adjusts: false);
    public static readonly PartRole Cell = new(activates: true, adjusts: false);
    public static readonly PartRole Handle = new(activates: false, adjusts: true);
    public static readonly PartRole Slider = new(activates: false, adjusts: true);

    public bool Activates { get; }
    public bool Adjusts { get; }
}

public sealed record PartFacet(PartRole Role, Cursor Cursor, string Label, Option<string> Value, Option<string> Help);

public sealed record Part(MarkShape Shape, float Reach, PartFacet Facet);

public readonly record struct Interaction<TPart>(Option<TPart> Hovered, Option<TPart> Pressed) where TPart : notnull;

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Edit<TValue> where TValue : notnull {
    public Option<TValue> Carried =>
        Switch(preview: static held => Some(held.Value), step: static held => Some(held.Value), commit: static held => Some(held.Value), cancel: static _ => Option<TValue>.None);

    public Option<TValue> Settled =>
        Switch(preview: static _ => Option<TValue>.None, step: static held => Some(held.Value), commit: static held => Some(held.Value), cancel: static _ => Option<TValue>.None);

    public sealed record Preview(TValue Value) : Edit<TValue>;

    public sealed record Step(TValue Value) : Edit<TValue>;

    public sealed record Commit(TValue Value) : Edit<TValue>;

    public sealed record Cancel : Edit<TValue>;
}

public sealed class EditEventArgs<TValue>(Edit<TValue> edit) : EventArgs where TValue : notnull {
    public Edit<TValue> Edit { get; } = edit;
}

public readonly record struct Transition<TState, TValue>(TState State, Option<Edit<TValue>> Edit) where TState : notnull where TValue : notnull;

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class ComponentControl : Drawable {
    public const float DragThreshold = 3f;
    private Option<Func<(SizeF Size, float Scale)>> hosted;
    private Option<string> help;

    public static string HandlerStyle { get; } = StyleName(typeof(ComponentControl));

    protected ComponentControl(IPlugInSink sink) {
        Sink = sink;
        Style = HandlerStyle;
        CanFocus = true;
    }

    public IPlugInSink Sink { get; }

    public IO<Unit> Describe(Option<string> text) => IO.lift(() => {
        help = text;
        RefreshToolTip();
    });

    protected (SizeF Size, float Scale) Surface => hosted.Match(Some: static read => read(), None: () => (Size, ParentWindow.LogicalPixelSize));

    protected Option<(SizeF Size, float Scale)> CurrentSurface =>
        hosted.Match(Some: static read => Some(read()), None: () => Optional(ParentWindow).Map(window => ((SizeF)Size, window.LogicalPixelSize)));

    public virtual IO<Unit> Hosted(Control host, Func<(SizeF Size, float Scale)> surface, Action<IDisposable> retain) =>
        DisposalOps.Acquire(IO.lift(() => new Disposal<Func<(SizeF Size, float Scale)>>(surface, held => {
            if (hosted.Exists(read => ReferenceEquals(read, held))) hosted = None;
        })), lease => { retain(lease); hosted = Some(surface); }).Map(unit);

    public abstract Seq<Part> Parts { get; }
    public abstract bool Adjust(PointF at, int steps);
    public abstract bool Activate(PointF at);
    public abstract IO<Unit> Print(Graphics graphics, SizeF size, float scale);
    public abstract IO<Unit> Render(PlotCanvas canvas, SizeF size, bool interactive);
    public abstract IO<Unit> Press(SizeF size, float scale, MouseEventArgs e);
    public abstract IO<Unit> Move(SizeF size, float scale, MouseEventArgs e);
    public abstract IO<Unit> Release(SizeF size, float scale, MouseEventArgs e);
    public abstract IO<Unit> DoubleClick(SizeF size, float scale, MouseEventArgs e);
    public abstract IO<Unit> Scroll(SizeF size, float scale, MouseEventArgs e);
    public abstract IO<Unit> Key(SizeF size, float scale, KeyEventArgs e);
    public abstract IO<Unit> Leave(SizeF size, float scale);
    public abstract IO<Unit> Cancel(SizeF size, float scale);
    public abstract Option<float> PreferredHeight(float width);
    public abstract bool Captured { get; }
    public abstract IO<Unit> SurfaceChanged { get; }
    protected abstract Option<PartFacet> HoveredFacet { get; }

    public event EventHandler? Redraw;
    public event EventHandler? PreferredSizeChanged;

    protected void RedrawRegion(RectangleF region) {
        Invalidate(Rectangle.Ceiling(region));
        Redraw?.Invoke(this, EventArgs.Empty);
    }

    protected void RefreshPreferredHeight(Option<float> before, Option<float> after) {
        if (before != after) PreferredSizeChanged?.Invoke(this, EventArgs.Empty);
    }

    protected void RefreshToolTip() => ToolTip = (HoveredFacet.Map(facet => (facet.Help | help).IfNone(facet.Label)) | help).ValueUnsafe();

    public static string StyleName(Type control) => $"{control.Assembly.GetName().Name}.{control.Name}";
    public static float Scale(Keys modifiers) => (modifiers & Application.Instance.CommonModifier) is not Keys.None ? 0.1f : 1f;
    public static bool Snaps(Keys modifiers) => modifiers.HasFlag(Keys.Shift);
}

public abstract class ComponentControl<TState, TPart, TValue>(IPlugInSink sink, TState initial, Option<Func<Control, IO<Func<TState, TState>>>> sampled) : ComponentControl(sink)
    where TState : notnull where TPart : notnull where TValue : notnull {
    // --- [STATE]
    private Option<TPart> hovered;
    private Option<Grip> grip;
    private Option<IDisposable> lifetime;

    protected TState State { get; private set; } = initial;
    protected Option<PointF> Pointer { get; private set; }

    public IO<Unit> Advance(Func<TState, Transition<TState, TValue>> transition) => IO.lift(() => Apply(CurrentSurface, transition(State)));

    public IO<Unit> Receive(Option<TValue> value) => Advance(held => new(Received(held, value), None));

    public IO<Unit> Receive(SizeF size, float scale, Option<TValue> value) => IO.lift(() => Apply(size, scale, new(Received(State, value), None)));

    public override IO<Unit> SurfaceChanged => IO.lift(() => Refresh(CurrentSurface, State, Cues));

    public override IO<Unit> Hosted(Control host, Func<(SizeF Size, float Scale)> surface, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
            from bound in base.Hosted(host, surface, owned)
            from theme in HostTheme.In(host).Inline(_ => IO.lift(() => RedrawRegion(new RectangleF(surface().Size))), Sink, owned)
            from clock in sampled.Traverse(read => FrameClocks.Sample(host,
                read(this).Bind(fold => Advance(held => new(fold(held), None))), Site(nameof(FrameClocks.Sample)), owned)).As()
            select unit, retain);

    private sealed record Grip(PointF Origin, PointF Last, Option<TPart> Part, bool Dragging, Option<TValue> Open);

    protected Interaction<TPart> Cues => new(hovered, grip.Bind(static held => held.Part));

    protected override Option<PartFacet> HoveredFacet => hovered.Map(key => Facet(State, key));

    protected CallbackSite Site([CallerMemberName] string member = "") => new(Sink, GetType(), member);

    private Unit Apply(SizeF size, float scale, Option<Transition<TState, TValue>> next) => next.Match(Some: held => Apply(size, scale, held), None: static () => unit);

    private Unit Apply(SizeF size, float scale, Transition<TState, TValue> next) => Apply(Some((size, scale)), next);

    private Unit Apply(Option<(SizeF Size, float Scale)> surface, Transition<TState, TValue> next) {
        (TState before, Interaction<TPart> was) = (State, Cues);
        State = next.State;
        grip = grip.Map(held => next.Edit.Map(edit => Folded(held, edit)).IfNone(held));
        _ = next.Edit.Iter(edit => Edited?.Invoke(this, new EditEventArgs<TValue>(edit)));
        return Refresh(surface, before, was);
    }

    private Unit Tracked(SizeF size, float scale, Action change) {
        Interaction<TPart> was = Cues;
        change();
        return Refresh(Some((size, scale)), State, was);
    }

    private Unit Refresh(Option<(SizeF Size, float Scale)> surface, TState before, Interaction<TPart> was) {
        Seq<PlotMark<TPart>> marks = surface.ToSeq().Bind(held => Layout(State, held.Size, held.Scale));
        hovered = Pointer.Bind(at => Plots.Hit(marks, at));
        Interaction<TPart> now = Cues;
        _ = (EqualityComparer<TState>.Default.Equals(before, State) ? Seq<RectangleF>() : surface.ToSeq().Map(held => Damage(before, State, held.Size)))
            .Concat(was == now ? Seq<RectangleF>() : Regions(marks, toHashSet(Seq(was.Hovered, now.Hovered, was.Pressed, now.Pressed).Somes())))
            .Iter(RedrawRegion);
        Cursor = (now.Pressed | now.Hovered).Match(Some: key => Facet(State, key).Cursor, None: static () => Cursors.Default);
        if (now.Hovered != was.Hovered) RefreshToolTip();
        _ = surface.Iter(held => RefreshPreferredHeight(Fitted(before, held.Size.Width), Fitted(State, held.Size.Width)));
        _ = Optional(ParentWindow).Iter(_ => Fit());
        Callbacks.Run(Reconcile(before, State, was, now), Site(nameof(Reconcile)));
        return unit;
    }

    private Unit Fit() =>
        Fitted(State, Width).Map(static height => (int)MathF.Ceiling(height)).Filter(height => height != Height).Iter(height => Height = height);

    protected override void OnSizeChanged(EventArgs e) {
        base.OnSizeChanged(e);
        Callbacks.Run(IO.lift(Fit), Site());
    }

    private Option<TPart> Hit(SizeF size, float scale, PointF at) => Plots.Hit(Layout(State, size, scale), at);

    private static Grip Folded(Grip grip, Edit<TValue> edit) =>
        edit.Switch(
            grip,
            preview: static (held, preview) => held with { Open = Some(preview.Value) },
            step: static (held, _) => held,
            commit: static (held, _) => held with { Open = None },
            cancel: static (held, _) => held with { Open = None });

    private static Seq<RectangleF> Regions(Seq<PlotMark<TPart>> marks, LanguageExt.HashSet<TPart> keys) =>
        marks.Choose(mark => mark.Part.Filter(part => keys.Contains(part.Key)).Map(part => mark.Shape.Region(part.Reach)));

    // --- [TRANSITIONS]
    protected abstract Seq<PlotMark<TPart>> Layout(TState state, SizeF size, float scale);
    protected abstract PartFacet Facet(TState state, TPart key);
    protected abstract IO<Unit> Draw(PlotCanvas canvas, RectangleF bounds, TState state, Seq<PlotMark<TPart>> marks, Interaction<TPart> interaction);
    protected abstract TState Received(TState state, Option<TValue> value);
    protected virtual RectangleF Damage(TState before, TState after, SizeF size) => new(size);
    protected virtual Option<float> Fitted(TState state, float width) => None;
    protected virtual IO<Unit> Reconcile(TState before, TState after, Interaction<TPart> was, Interaction<TPart> now) => IO.pure(unit);
    protected virtual Option<Transition<TState, TValue>> Pressed(TState state, SizeF size, float scale, Option<TPart> part, MouseEventArgs e) => None;
    protected virtual Transition<TState, TValue> Dragged(TState state, SizeF size, float scale, Option<TPart> part, PointF origin, PointF previous, MouseEventArgs e) => new(state, None);
    protected virtual Transition<TState, TValue> Released(TState state, SizeF size, float scale, Option<TPart> part, MouseEventArgs e) => new(state, None);
    protected virtual Option<Transition<TState, TValue>> DoubleClicked(TState state, SizeF size, float scale, Option<TPart> part, MouseEventArgs e) => None;
    protected virtual Option<Transition<TState, TValue>> Scrolled(TState state, SizeF size, float scale, Option<TPart> part, MouseEventArgs e) => None;
    protected virtual Option<Transition<TState, TValue>> KeyPressed(TState state, SizeF size, float scale, Option<TPart> part, KeyEventArgs e) => None;
    protected virtual Option<Transition<TState, TValue>> Magnified(TState state, SizeF size, float scale, Option<TPart> part, Option<PointF> pointer, float change) => None;
    protected virtual Option<Transition<TState, TValue>> Rotated(TState state, SizeF size, float scale, Option<TPart> part, Option<PointF> pointer, float radians) => None;
    protected virtual Option<Transition<TState, TValue>> Stepped(TState state, SizeF size, float scale, TPart part, int steps) => None;
    protected virtual Option<Transition<TState, TValue>> Activated(TState state, SizeF size, float scale, TPart part) => None;

    // --- [POINTER]
    public override bool Captured => grip.IsSome;

    public override Option<float> PreferredHeight(float width) => Fitted(State, width);

    public override IO<Unit> Press(SizeF size, float scale, MouseEventArgs e) => IO.lift(() => Pressing(size, scale, e));

    public override IO<Unit> Move(SizeF size, float scale, MouseEventArgs e) =>
        IO.lift(() => grip.Match(Some: held => Dragging(size, scale, e, held), None: () => Tracked(size, scale, () => Pointer = e.Location)));

    public override IO<Unit> Release(SizeF size, float scale, MouseEventArgs e) {
        return IO.lift(() => e.Buttons is MouseButtons.None ? Abandon(size, scale) : grip.Match(Some: Releasing, None: static () => unit));

        Unit Releasing(Grip held) {
            _ = Apply(size, scale, Released(State, size, scale, held.Part, e));
            _ = grip.Bind(static current => current.Open).Iter(open => Closed(size, scale, new Edit<TValue>.Commit(open)));
            return Tracked(size, scale, () => grip = None);
        }
    }

    public override IO<Unit> DoubleClick(SizeF size, float scale, MouseEventArgs e) =>
        IO.lift(() => Claimed(size, scale, DoubleClicked(State, size, scale, Hit(size, scale, e.Location), e), () => {
            e.Handled = true;
            _ = Abandon(size, scale);
        }));

    public override IO<Unit> Scroll(SizeF size, float scale, MouseEventArgs e) =>
        IO.lift(() => Claimed(size, scale, Scrolled(State, size, scale, Hit(size, scale, e.Location), e), () => e.Handled = true));

    public override IO<Unit> Key(SizeF size, float scale, KeyEventArgs e) => IO.lift(() => Keying(size, scale, e));

    public override IO<Unit> Leave(SizeF size, float scale) => IO.lift(() => grip.IsSome ? unit : Tracked(size, scale, () => Pointer = None));

    public override IO<Unit> Cancel(SizeF size, float scale) => IO.lift(() => {
        _ = Abandon(size, scale);
        return Tracked(size, scale, () => Pointer = None);
    });

    protected override void OnMouseDown(MouseEventArgs e) {
        Answered(scale => Pressing(Size, scale, e));
        if (e.Handled) return;

        base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        Callbacks.Run(Move(Size, ParentWindow.LogicalPixelSize, e), Site());
        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        Callbacks.Run(Release(Size, ParentWindow.LogicalPixelSize, e), Site());
        base.OnMouseUp(e);
    }

    protected override void OnMouseDoubleClick(MouseEventArgs e) {
        Callbacks.Run(DoubleClick(Size, ParentWindow.LogicalPixelSize, e), Site());
        if (e.Handled) return;

        base.OnMouseDoubleClick(e);
    }

    protected override void OnMouseWheel(MouseEventArgs e) {
        if (HasFocus) Callbacks.Run(Scroll(Size, ParentWindow.LogicalPixelSize, e), Site());
        if (e.Handled) return;

        base.OnMouseWheel(e);
    }

    protected override void OnMouseLeave(MouseEventArgs e) {
        Callbacks.Run(Leave(Size, ParentWindow.LogicalPixelSize), Site());
        base.OnMouseLeave(e);
    }

    private void Answered(Func<float, Unit> input, [CallerMemberName] string member = "") =>
        Callbacks.Run(IO.lift(() => input(ParentWindow.LogicalPixelSize)), Site(member));

    private Unit Claimed(SizeF size, float scale, Option<Transition<TState, TValue>> next, Action claim) {
        _ = next.Iter(_ => claim());
        return Apply(size, scale, next);
    }

    private Unit Pressing(SizeF size, float scale, MouseEventArgs e) {
        _ = Abandon(size, scale);
        Option<TPart> part = Hit(size, scale, e.Location);
        Pointer = e.Location;
        return Claimed(size, scale, Pressed(State, size, scale, part, e), () => {
            e.Handled = true;
            grip = new Grip(e.Location, e.Location, part, Dragging: false, Open: None);
        });
    }

    private Unit Dragging(SizeF size, float scale, MouseEventArgs e, Grip held) {
        bool dragging = held.Dragging || PointF.Distance(held.Origin, e.Location) > DragThreshold;
        Option<Transition<TState, TValue>> next = dragging ? Some(Dragged(State, size, scale, held.Part, held.Origin, held.Last, e)) : None;
        Pointer = e.Location;
        grip = held with { Last = dragging ? e.Location : held.Last, Dragging = dragging };
        return Apply(size, scale, next);
    }

    private Unit Abandon(SizeF size, float scale) {
        _ = grip.Bind(static held => held.Open).Iter(_ => Closed(size, scale, new Edit<TValue>.Cancel()));
        return grip.IsSome ? Tracked(size, scale, () => grip = None) : unit;
    }

    private Unit Closed(SizeF size, float scale, Edit<TValue> edit) => Apply(size, scale, new Transition<TState, TValue>(State, Some(edit)));

    // --- [KEYS]
    protected override void OnKeyDown(KeyEventArgs e) {
        Answered(scale => Keying(Size, scale, e));
        if (e.Handled) return;

        base.OnKeyDown(e);
    }

    protected override void OnGotFocus(EventArgs e) {
        base.OnGotFocus(e);
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e) {
        Answered(scale => Abandon(Size, scale));
        Invalidate();
        base.OnLostFocus(e);
    }

    protected override void OnEnabledChanged(EventArgs e) {
        Answered(scale => Abandon(Size, scale));
        Invalidate();
        base.OnEnabledChanged(e);
    }

    private Unit Keying(SizeF size, float scale, KeyEventArgs e) {
        bool escapes = e.Key is Keys.Escape && grip.Exists(static held => held.Open.IsSome);
        e.Handled = escapes;
        return escapes ? Abandon(size, scale) : Claimed(size, scale, KeyPressed(State, size, scale, hovered, e), () => e.Handled = true);
    }

    // --- [GESTURES]
    private IO<Unit> Gestured<TGesture>(Func<TGesture, float, Option<Transition<TState, TValue>>> transition, Action<IDisposable> retain) where TGesture : Gesture, new() =>
        DisposalOps.Acquire(owned =>
            from gesture in DisposalOps.Acquire(IO.lift(static () => new TGesture()), owned)
            from removal in DisposalOps.Acquire(IO.lift(() => new Disposal<Gesture>(gesture, held => Gestures.Remove(held))), owned)
            from added in IO.lift(() => Gestures.Add(gesture))
            from activated in Subscriptions.Host<EventArgs>(typeof(TGesture), handler => gesture.Activated += handler, handler => gesture.Activated -= handler, nameof(Gesture.Activated))
                .Inline(_ => IO.lift(() => Apply(Size, ParentWindow.LogicalPixelSize, transition(gesture, ParentWindow.LogicalPixelSize))), Sink, owned)
            select activated, retain);

    // --- [ACCESSIBILITY]
    public override Seq<Part> Parts =>
        Surface switch {
            var surface => Layout(State, surface.Size, surface.Scale).Choose(mark => mark.Part.Map(part => new Part(mark.Shape, part.Reach, Facet(State, part.Key)))),
        };

    public override bool Adjust(PointF at, int steps) =>
        Acted(at, static role => role.Adjusts, (held, size, scale, key) => Stepped(held, size, scale, key, steps));

    public override bool Activate(PointF at) => Acted(at, static role => role.Activates, Activated);

    private bool Acted(PointF at, Func<PartRole, bool> takes, Func<TState, SizeF, float, TPart, Option<Transition<TState, TValue>>> act, [CallerMemberName] string member = "") =>
        Callbacks.Succeeded(
            IO.lift(() => Surface)
                .Map(surface => Apply(surface.Size, surface.Scale, Hit(surface.Size, surface.Scale, at).Filter(key => takes(Facet(State, key).Role)).Bind(key => act(State, surface.Size, surface.Scale, key)))),
            Site(member));

    // --- [PAINT]
    protected override void OnPaint(PaintEventArgs e) {
        Callbacks.Run(
            IO.lift(() => ParentWindow.LogicalPixelSize)
                .Bind(scale => PlotCanvas.Of(e.Graphics, scale, EtoFonts.SmallFont, Enabled, HasFocus))
                .Bind(canvas => Render(canvas, Size, interactive: true)),
            Site());
        base.OnPaint(e);
    }

    public override IO<Unit> Print(Graphics graphics, SizeF size, float scale) =>
        PlotCanvas.Of(graphics, scale, EtoFonts.SmallFont, enabled: true, focused: false)
            .Bind(canvas => Render(canvas, size, interactive: false));

    public override IO<Unit> Render(PlotCanvas canvas, SizeF size, bool interactive) =>
        Draw(canvas, new RectangleF(size), State, Layout(State, size, canvas.Scale), interactive ? Cues : new Interaction<TPart>(None, None));

    protected override void OnThemeChanged(EventArgs e) {
        base.OnThemeChanged(e);
        Invalidate();
    }

    // --- [EDITS]
    public event EventHandler<EditEventArgs<TValue>>? Edited;

    // --- [LIFETIME]
    protected override void OnLoad(EventArgs e) {
        base.OnLoad(e);
        Callbacks.Run(Held, Site());
    }

    protected override void OnUnLoad(EventArgs e) {
        Option<IDisposable> held = lifetime;
        lifetime = None;
        Callbacks.Run(Callbacks.Each(Seq(
            Cancel(Size, ParentWindow.LogicalPixelSize),
            IO.lift(() => held.Iter(static lease => lease.Dispose())),
            IO.lift(() => base.OnUnLoad(e)))), Site());
    }

    private IO<Unit> Held =>
        DisposalOps.Acquire(keep => Seq(
                HostTheme.In(this).Inline(_ => IO.lift(Invalidate), Sink, keep),
                Gestured<MagnificationGesture>((gesture, scale) => Magnified(State, Size, scale, hovered, Pointer, gesture.Magnification), keep),
                Gestured<RotationGesture>((gesture, scale) => Rotated(State, Size, scale, hovered, Pointer, float.DegreesToRadians(-gesture.Rotation)), keep))
            .Concat(sampled.Map(read => FrameClocks.Sample(this, read(this).Bind(fold => Advance(held => new(fold(held), None))), Site(nameof(FrameClocks.Sample)), keep)).ToSeq())
            .TraverseM(static attach => attach).As(), (_, lease) => lifetime = Some(lease)).Map(static _ => unit);
}
