using System.Numerics;
using LanguageExt.ClassInstances;
using Rasm.UI.Layout;
using Rasm.UI.Plotting;
using Rasm.UI.Rendering;
using SkiaSharp;
using SuperLinq;

namespace Rasm.UI.Input;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
internal abstract partial record Gesture {
    internal KeyModifiers Modifiers { get; init; } = KeyModifiers.Empty;

    internal virtual Option<Gesture> Accumulated(Gesture increment) => None;

    internal sealed record Press(SKPoint From, Option<SKPoint> Dragged, int Clicks) : Gesture {
        internal SKPoint At => Dragged.IfNone(From);
        internal SKRect Swept => new SKRect(From.X, From.Y, At.X, At.Y).Standardized;
        internal Seq<PointerSample> Trail { get; init; }
        internal Vector2 Scrolled { get; init; }
        internal bool Ended { get; init; }
        internal bool Clicked => Ended && Dragged.IsNone;

        internal Press Updated(PointerSample sample, float slop) =>
            Dragged.IsSome || SKPoint.Distance(sample.At, From) >= slop ? this with { Dragged = Some(sample.At), Trail = Trail.Add(sample) } : this;

        internal Option<Figure.Region> Enclosed(PlotFrame frame) =>
            from start in frame.At(From)
            from end in frame.At(At)
            where Dragged.IsSome
            select new Figure.Region { First = Spanned(frame.Plane, PhysicalAxis.Horizontal, start, end), Second = Spanned(frame.Plane, PhysicalAxis.Vertical, start, end) };

        private static Option<AxisRange> Spanned(PlotPlane plane, PhysicalAxis along, Vector2 start, Vector2 end) =>
            plane.Dimension(along).Bind(_ => AxisRange.Validated(along.Of(start), along.Of(end)).ToOption());
    }

    internal sealed record Scroll(SKPoint At, Vector2 Delta) : Gesture {
        internal override Option<Gesture> Accumulated(Gesture increment) => Optional(increment as Scroll).Map(next => (Gesture)(next with { Delta = Delta + next.Delta }));
    }

    internal sealed record Magnify(SKPoint About, float Magnification) : Gesture {
        internal override Option<Gesture> Accumulated(Gesture increment) => Optional(increment as Magnify).Map(next => (Gesture)(next with { Magnification = ((1f + Magnification) * (1f + next.Magnification)) - 1f }));
    }

    internal sealed record Rotate(SKPoint About, float Rotation) : Gesture {
        internal override Option<Gesture> Accumulated(Gesture increment) => Optional(increment as Rotate).Map(next => (Gesture)(next with { Rotation = Rotation + next.Rotation }));
    }

    internal sealed record Perform(Act Act) : Gesture;
}

internal sealed record Reply(Seq<IO<Unit>> Effects, Option<Track> Track);

internal sealed record Track(Func<Gesture, Reply> Moved, Func<Gesture, Seq<IO<Unit>>> Released, Func<Seq<IO<Unit>>> Cancelled) {
    internal CursorShape Cursor { get; init; } = CursorShape.Default;
    internal Option<Vector2> Detent { get; init; }
    internal Option<Figure> Preview { get; init; }
    internal Option<(Transfer.Keys Keys, IO<Transfer> Payload)> Lifted { get; init; }
}

internal sealed record Receiver<TState>(Func<TState, TState, bool> Same, Func<Edit<TState>, Option<IO<Unit>>> Emit) where TState : notnull {
    internal static Receiver<TState> Of<TOut>(Func<TState, TOut> project, Func<Edit<TOut>, Option<IO<Unit>>> edited) where TOut : notnull =>
        new((before, after) => EqDefault<TOut>.Equals(project(before), project(after)), edit => edited(edit.Map(project).As()));
}

internal sealed record Session<TState>(TState Start, Seq<Receiver<TState>> Receivers, CursorShape Cursor) where TState : notnull {
    // --- [STATE]
    internal Func<TState, Gesture, Option<TState>> Step { get; init; } = static (_, _) => None;
    internal Func<TState, TState> Released { get; init; } = static state => state;
    internal Func<TState, Option<Vector2>> Detent { get; init; } = static _ => None;
    internal Func<TState, Option<Figure>> Preview { get; init; } = static _ => None;
    internal Func<TState, Option<(Transfer.Keys Keys, IO<Transfer> Payload)>> Lifted { get; init; } = static _ => None;

    private TState Last { get; init; } = Start;
    private bool Dragging { get; init; }
    private HashSet<int> Opened { get; init; }

    // --- [EMIT]
    internal Option<Reply> Began(Gesture first) => Step(Start, first).Map(state => Continued(state, Drags(first)));

    internal Option<Reply> Stepped(Gesture gesture) => Stepped(start => Step(start, gesture));

    internal Option<Reply> Stepped(Func<TState, Option<TState>> next) =>
        next(Start).Map(state => new Reply(Receivers.Filter(receiver => !receiver.Same(Start, state)).Choose(receiver => receiver.Emit(Edit.Of(EditPhase.Step, state))), None));

    private Seq<(Receiver<TState> Receiver, int Index)> Indexed => Receivers.Map(static (receiver, index) => (Receiver: receiver, Index: index));

    private Track Tracked =>
        new(gesture => Step(Start, gesture).Match(Some: state => Continued(state, Drags(gesture)), None: () => Continued(Last, Dragging)), gesture => Finished(Released(Step(Start, gesture).IfNone(Last))),
            () => Indexed.Filter(held => Opened.Contains(held.Index)).Choose(static held => held.Receiver.Emit(Edit.Cancel<TState>()))) {
            Cursor = Dragging ? Cursor.Dragged : Cursor,
            Detent = Detent(Last),
            Preview = Preview(Last),
            Lifted = Dragging ? Lifted(Last) : None,
        };

    private Reply Continued(TState state, bool dragging) {
        Seq<(int Index, IO<Unit> Effect)> moved = Indexed.Filter(held => !held.Receiver.Same(Last, state))
            .Choose(held => held.Receiver.Emit(Edit.Of(Opened.Contains(held.Index) ? EditPhase.Preview : EditPhase.Begin, state)).Map(effect => (held.Index, effect)));
        return new(moved.Map(static held => held.Effect), Some((this with { Last = state, Dragging = dragging, Opened = Opened.Union(moved.Map(static held => held.Index)) }).Tracked));
    }

    private Seq<IO<Unit>> Finished(TState state) =>
        Indexed.Choose(held =>
            !held.Receiver.Same(Start, state) ? held.Receiver.Emit(Edit.Of(EditPhase.Commit, state))
            : Opened.Contains(held.Index) ? held.Receiver.Emit(Edit.Cancel<TState>())
            : None);

    private static bool Drags(Gesture gesture) => gesture is Gesture.Press { Dragged.IsSome: true };
}

internal sealed record HitTarget(Scene Scene, PartOwner Owner, Seq<Key> Hit) {
    internal Option<PlotFrame> Frame => Owner.Layout.Plotted(Owner.Bounds.Location);
}

internal sealed record HoverState(Scene Scene, LayoutNode Layout, Seq<Seq<Key>> Hover, Option<SKPoint> Pointer, Option<Nearest> Nearest) {
    internal Option<Nearest> Near => Nearest.Filter(held => held.Plot == Layout.Path);

    internal Option<Vector2> Plotted =>
        from pointer in Pointer
        where Hover.Exists(path => path.StartsWith(Layout.Path))
        from bounds in Scene.Frame(Layout.Path)
        from frame in Layout.Plotted(bounds.Location)
        from at in frame.At(pointer)
        select at;
}
