using System.Runtime.CompilerServices;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Assets;
using Rhino.Resources;

namespace Rasm.Rhino.UI.Components;

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

    public static string HandlerStyle { get; } = StyleName(typeof(ComponentControl));

    protected ComponentControl(IPlugInSink sink) {
        Sink = sink;
        Style = HandlerStyle;
        CanFocus = true;
    }

    public IPlugInSink Sink { get; }

    public abstract Seq<Part> Parts { get; }
    public abstract bool Adjust(PointF at, int steps);
    public abstract bool Activate(PointF at);
    public abstract IO<Unit> Print(Graphics graphics, SizeF size, float scale);

    public static string StyleName(Type control) => $"{control.Assembly.GetName().Name}.{control.Name}";
    public static float Scale(Keys modifiers) => (modifiers & Application.Instance.CommonModifier) != Keys.None ? 0.1f : 1f;
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

    public IO<Unit> Advance(Func<TState, Transition<TState, TValue>> transition) => IO.lift(() => Apply(transition(State)));

    public IO<Unit> Receive(Option<TValue> value) => Advance(held => new(Received(held, value), None));

    private sealed record Grip(PointF Origin, PointF Last, Option<TPart> Part, bool Dragging, Option<TValue> Open);

    private Interaction<TPart> Cues => new(hovered, grip.Bind(static held => held.Part));

    protected CallbackSite Site([CallerMemberName] string member = "") => new(Sink, GetType(), member);

    private Unit Apply(Option<Transition<TState, TValue>> next) => next.Match(Some: Apply, None: static () => unit);

    private Unit Apply(Transition<TState, TValue> next) {
        (TState before, Interaction<TPart> was) = (State, Cues);
        State = next.State;
        grip = grip.Map(held => next.Edit.Map(edit => Folded(held, edit)).IfNone(held));
        _ = next.Edit.Iter(edit => Edited?.Invoke(this, new EditEventArgs<TValue>(edit)));
        return Refresh(before, was);
    }

    private Unit Tracked(Action change) {
        Interaction<TPart> was = Cues;
        change();
        return Refresh(State, was);
    }

    private Unit Refresh(TState before, Interaction<TPart> was) {
        Seq<PlotMark<TPart>> marks = Optional(ParentWindow).ToSeq().Bind(window => Layout(State, Size, window.LogicalPixelSize));
        hovered = Pointer.Bind(at => Plots.Hit(marks, at));
        Interaction<TPart> now = Cues;
        _ = (EqualityComparer<TState>.Default.Equals(before, State) ? Seq<RectangleF>() : Seq(Damage(before, State, Size)))
            .Concat(was == now ? Seq<RectangleF>() : Regions(marks, toHashSet(Seq(was.Hovered, now.Hovered, was.Pressed, now.Pressed).Somes())))
            .Iter(region => Invalidate(Rectangle.Ceiling(region)));
        Cursor = (now.Pressed | now.Hovered).Match(Some: key => Facet(State, key).Cursor, None: static () => Cursors.Default);
        _ = Fit();
        return Callbacks.Answer(Reconcile(before, State, was, now), static () => unit, Site(nameof(Reconcile)));
    }

    private Unit Fit() =>
        Fitted(State, Width).Map(static height => (int)MathF.Ceiling(height)).Filter(height => height != Height).Iter(height => Height = height);

    protected override void OnSizeChanged(EventArgs e) {
        base.OnSizeChanged(e);
        _ = Callbacks.Answer(IO.lift(Fit), static () => unit, Site());
    }

    private Option<TPart> Hit(float scale, PointF at) => Plots.Hit(Layout(State, Size, scale), at);

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
    protected override void OnMouseDown(MouseEventArgs e) {
        _ = Answered(scale => Pressing(scale, e));
        if (e.Handled) return;

        base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e) {
        _ = Answered(scale => grip.Match(Some: held => Dragging(scale, e, held), None: () => Tracked(() => Pointer = e.Location)));
        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e) {
        _ = Answered(scale => e.Buttons == MouseButtons.None ? Abandon() : grip.Match(Some: held => Releasing(scale, e, held), None: static () => unit));
        base.OnMouseUp(e);
    }

    protected override void OnMouseDoubleClick(MouseEventArgs e) {
        _ = Answered(scale => Claimed(DoubleClicked(State, Size, scale, Hit(scale, e.Location), e), () => {
            e.Handled = true;
            _ = Abandon();
        }));
        if (e.Handled) return;

        base.OnMouseDoubleClick(e);
    }

    protected override void OnMouseWheel(MouseEventArgs e) {
        _ = Answered(scale => HasFocus ? Claimed(Scrolled(State, Size, scale, Hit(scale, e.Location), e), () => e.Handled = true) : unit);
        if (e.Handled) return;

        base.OnMouseWheel(e);
    }

    protected override void OnMouseLeave(MouseEventArgs e) {
        _ = Callbacks.Answer(IO.lift(() => grip.IsSome ? unit : Tracked(() => Pointer = None)), static () => unit, Site());
        base.OnMouseLeave(e);
    }

    private Unit Answered(Func<float, Unit> input, [CallerMemberName] string member = "") =>
        Callbacks.Answer(IO.lift(() => input(ParentWindow.LogicalPixelSize)), static () => unit, Site(member));

    private Unit Claimed(Option<Transition<TState, TValue>> next, Action claim) {
        _ = next.Iter(_ => claim());
        return Apply(next);
    }

    private Unit Pressing(float scale, MouseEventArgs e) {
        _ = Abandon();
        Option<TPart> part = Hit(scale, e.Location);
        Pointer = e.Location;
        return Claimed(Pressed(State, Size, scale, part, e), () => {
            e.Handled = true;
            grip = new Grip(e.Location, e.Location, part, Dragging: false, Open: None);
        });
    }

    private Unit Dragging(float scale, MouseEventArgs e, Grip held) {
        bool dragging = held.Dragging || PointF.Distance(held.Origin, e.Location) > DragThreshold;
        Option<Transition<TState, TValue>> next = dragging ? Some(Dragged(State, Size, scale, held.Part, held.Origin, held.Last, e)) : None;
        Pointer = e.Location;
        grip = held with { Last = dragging ? e.Location : held.Last, Dragging = dragging };
        return Apply(next);
    }

    private Unit Releasing(float scale, MouseEventArgs e, Grip held) {
        _ = Apply(Released(State, Size, scale, held.Part, e));
        _ = grip.Bind(static current => current.Open).Iter(open => Closed(new Edit<TValue>.Commit(open)));
        return Tracked(() => grip = None);
    }

    private Unit Abandon() {
        _ = grip.Bind(static held => held.Open).Iter(_ => Closed(new Edit<TValue>.Cancel()));
        return grip.IsSome ? Tracked(() => grip = None) : unit;
    }

    private Unit Closed(Edit<TValue> edit) => Apply(new Transition<TState, TValue>(State, Some(edit)));

    // --- [KEYS]
    protected override void OnKeyDown(KeyEventArgs e) {
        _ = Answered(scale => Keying(scale, e));
        if (e.Handled) return;

        base.OnKeyDown(e);
    }

    protected override void OnGotFocus(EventArgs e) {
        base.OnGotFocus(e);
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e) {
        _ = Callbacks.Answer(IO.lift(Abandon), static () => unit, Site());
        Invalidate();
        base.OnLostFocus(e);
    }

    protected override void OnEnabledChanged(EventArgs e) {
        _ = Callbacks.Answer(IO.lift(Abandon), static () => unit, Site());
        Invalidate();
        base.OnEnabledChanged(e);
    }

    private Unit Keying(float scale, KeyEventArgs e) {
        bool escapes = e.Key == Keys.Escape && grip.Exists(static held => held.Open.IsSome);
        e.Handled = escapes;
        return escapes ? Abandon() : Claimed(KeyPressed(State, Size, scale, hovered, e), () => e.Handled = true);
    }

    // --- [GESTURES]
    private IO<IDisposable> Gestured<TGesture>(Func<TGesture, float, Option<Transition<TState, TValue>>> transition) where TGesture : Gesture, new() =>
        IO.lift(static () => new TGesture())
            .Bind(gesture => DisposalOps.AcquireAll(
                Seq(IO.lift(() => (IDisposable)Added(gesture)),
                    Subscriptions.Host<EventArgs>(typeof(TGesture), handler => gesture.Activated += handler, handler => gesture.Activated -= handler, nameof(Gesture.Activated))
                        .Inline(_ => IO.lift(() => Apply(transition(gesture, ParentWindow.LogicalPixelSize))), Sink)),
                DisposalOps.Release))
            .Map(held => DisposalOps.Composite(held, Site(nameof(OnUnLoad))));

    private Disposal<Gesture> Added(Gesture gesture) {
        Gestures.Add(gesture);
        return new Disposal<Gesture>(gesture, held => {
            _ = Gestures.Remove(held);
            held.Dispose();
        });
    }

    // --- [ACCESSIBILITY]
    public override Seq<Part> Parts =>
        Layout(State, Size, ParentWindow.LogicalPixelSize).Choose(mark => mark.Part.Map(part => new Part(mark.Shape, part.Reach, Facet(State, part.Key))));

    public override bool Adjust(PointF at, int steps) =>
        Acted(at, static role => role.Adjusts, (held, size, scale, key) => Stepped(held, size, scale, key, steps));

    public override bool Activate(PointF at) => Acted(at, static role => role.Activates, Activated);

    private bool Acted(PointF at, Func<PartRole, bool> takes, Func<TState, SizeF, float, TPart, Option<Transition<TState, TValue>>> act, [CallerMemberName] string member = "") =>
        Callbacks.Succeeded(
            IO.lift(() => ParentWindow.LogicalPixelSize)
                .Map(scale => Apply(Hit(scale, at).Filter(key => takes(Facet(State, key).Role)).Bind(key => act(State, Size, scale, key)))),
            Site(member));

    // --- [PAINT]
    protected override void OnPaint(PaintEventArgs e) {
        _ = Callbacks.Answer(
            IO.lift(() => ParentWindow.LogicalPixelSize).Bind(scale => Painted(e.Graphics, Size, scale, Enabled, HasFocus, Cues)),
            static () => unit,
            Site());
        base.OnPaint(e);
    }

    public override IO<Unit> Print(Graphics graphics, SizeF size, float scale) =>
        Painted(graphics, size, scale, enabled: true, focused: false, new Interaction<TPart>(None, None));

    protected override void OnThemeChanged(EventArgs e) {
        base.OnThemeChanged(e);
        Invalidate();
    }

    private IO<Unit> Painted(Graphics graphics, SizeF size, float scale, bool enabled, bool focused, Interaction<TPart> cues) =>
        PlotCanvas.Of(graphics, scale, EtoFonts.SmallFont, enabled, focused)
            .Bind(canvas => Draw(canvas, new RectangleF(size), State, Layout(State, size, canvas.Scale), cues));

    // --- [EDITS]
    public event EventHandler<EditEventArgs<TValue>>? Edited;

    // --- [LIFETIME]
    protected override void OnLoad(EventArgs e) {
        base.OnLoad(e);
        lifetime = Callbacks.Answer(Held.Map(static held => Some(held)), static () => Option<IDisposable>.None, Site());
    }

    protected override void OnUnLoad(EventArgs e) {
        _ = Callbacks.Answer(IO.lift(Abandon), static () => unit, Site());
        _ = lifetime.Iter(static held => held.Dispose());
        lifetime = None;
        base.OnUnLoad(e);
    }

    private IO<IDisposable> Held =>
        DisposalOps.AcquireAll(
                Seq(HostTheme.Changed.Inline(_ => IO.lift(() => Invalidate()), Sink),
                    Gestured<MagnificationGesture>((gesture, scale) => Magnified(State, Size, scale, hovered, Pointer, gesture.Magnification)),
                    Gestured<RotationGesture>((gesture, scale) => Rotated(State, Size, scale, hovered, Pointer, float.DegreesToRadians(-gesture.Rotation))))
                    .Concat(sampled.Map(read => FrameClocks.Sample(this, read(this).Bind(fold => Advance(held => new(fold(held), None))), Site(nameof(FrameClocks.Sample)))).ToSeq()),
                DisposalOps.Release)
            .Map(held => DisposalOps.Composite(held, Site(nameof(OnUnLoad))));
}
