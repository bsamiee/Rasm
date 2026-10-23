using Rasm.UI.Access;
using Rasm.UI.Inputs;
using Rasm.UI.Layout;
using Rasm.UI.Scenes;
using SkiaSharp;

namespace Rasm.UI.Components;

// --- [MODELS] --------------------------------------------------------------------------
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

public readonly record struct Transition<TState, TValue>(TState State, Option<Edit<TValue>> Edit) where TState : notnull where TValue : notnull;

public readonly record struct Interaction<TPart>(Option<TPart> Hovered, Option<TPart> Pressed) where TPart : notnull;

public sealed record Pointing(SKRect Frame, SKPoint Origin, SKPoint Previous, SKPoint At, PointerButtons Buttons, Modifiers Modifiers);

public sealed record Behavior<TState, TValue> where TState : notnull where TValue : notnull {
    public static Behavior<TState, TValue> Inert { get; } = new();

    public CursorKind Cursor { get; init; }
    public Func<TState, Pointing, Option<Transition<TState, TValue>>> Press { get; init; } = static (_, _) => None;
    public Func<TState, Pointing, Transition<TState, TValue>> Drag { get; init; } = static (state, _) => new(state, None);
    public Func<TState, Pointing, Transition<TState, TValue>> Release { get; init; } = static (state, _) => new(state, None);
    public Func<TState, Pointing, Option<Transition<TState, TValue>>> DoubleClick { get; init; } = static (_, _) => None;
    public Func<TState, int, Modifiers, Option<Transition<TState, TValue>>> Step { get; init; } = static (_, _, _) => None;
    public Func<TState, Option<Transition<TState, TValue>>> Activate { get; init; } = static _ => None;
    public Func<TState, Pointing, SKPoint, Option<Transition<TState, TValue>>> Scroll { get; init; } = static (_, _, _) => None;
    public Func<TState, Pointing, Gesture, float, Option<Transition<TState, TValue>>> Gestured { get; init; } = static (_, _, _, _) => None;
    public Func<TState, Input.Key, Option<Transition<TState, TValue>>> Key { get; init; } = static (_, _) => None;
}

public abstract record Control<TSelf, TPart, TValue> : Component<TPart>
    where TSelf : Control<TSelf, TPart, TValue> where TPart : notnull where TValue : notnull {
    public Interaction<TPart> Cues { get; init; }

    public abstract Behavior<TSelf, TValue> Behavior(Option<TPart> part);

    public abstract Facet Facet(TPart part);

    public override Seq<(SKRect Bounds, Facet Facet)> Facets(SKRect frame) =>
        Parts(frame).Choose(mark => mark.Part.Map(part => (mark.Shape.Region(part.Reach), Facet(part.Key))));

    public TSelf Cued(Interaction<TPart> cues) => (TSelf)(this with { Cues = cues });
}

public sealed record Grip<TPart, TValue>(SKPoint Origin, SKPoint Last, Option<TPart> Part, bool Dragging, Option<TValue> Open) where TPart : notnull where TValue : notnull;

public sealed record Engine<TControl, TPart, TValue>(TControl Control, Option<Grip<TPart, TValue>> Grip)
    where TControl : Control<TControl, TPart, TValue> where TPart : notnull where TValue : notnull {
    private static readonly FrozenDictionary<KeyCode, int> Steps =
        new Dictionary<KeyCode, int> { [KeyCode.ArrowRight] = 1, [KeyCode.ArrowUp] = 1, [KeyCode.ArrowLeft] = -1, [KeyCode.ArrowDown] = -1 }.ToFrozenDictionary();

    public static Engine<TControl, TPart, TValue> Of(TControl control) => new(control, None);

    public Engine<TControl, TPart, TValue> Received(TControl control) => this with { Control = control.Cued(Control.Cues) };

    // --- [VIEW]
    public Node.Widget View(Func<Engine<TControl, TPart, TValue>, Seq<Edit<TValue>>, IO<Unit>> publish) =>
        new(Control) {
            Focusable = true,
            Cursor = Control.Behavior(Control.Cues.Pressed | Control.Cues.Hovered).Cursor,
            Handler = Some<Func<Input, Laid, IO<Response>>>((input, laid) => Receive(laid.Content, input) switch {
                var (next, edits, response) => unless(next == this && edits.IsEmpty, publish(next, edits)).As().Map(_ => response),
            }),
        };

    // --- [TRANSITIONS]
    public (Engine<TControl, TPart, TValue> Next, Seq<Edit<TValue>> Edits, Response Response) Receive(SKRect frame, Input input) =>
        input.Switch(
            (Engine: this, Frame: frame),
            pointer: static (held, pointer) => held.Engine.Pointed(held.Frame, pointer),
            click: static (held, click) => click.Count >= 2
                ? held.Engine.Settle(held.Engine.Control.Behavior(held.Engine.Control.Hit(held.Frame, click.At))
                    .DoubleClick(held.Engine.Control, new Pointing(held.Frame, click.At, click.At, click.At, click.Button, click.Modifiers)))
                : held.Engine.Kept(Response.Ignored),
            scroll: static (held, scroll) => held.Engine.Scrolled(held.Frame, scroll),
            gestured: static (held, gestured) => held.Engine.Settle(held.Engine.Control.Behavior(held.Engine.Control.Hit(held.Frame, gestured.At))
                .Gestured(held.Engine.Control, new Pointing(held.Frame, gestured.At, gestured.At, gestured.At, PointerButtons.None, Modifiers.None), gestured.Gesture, gestured.Amount)),
            key: static (held, key) => key.Down ? held.Engine.Keyed(key) : held.Engine.Kept(Response.Ignored),
            composition: static (held, _) => held.Engine.Kept(Response.Ignored),
            focus: static (held, focus) => focus.Gained ? held.Engine.Kept(Response.Ignored) : held.Engine.Abandoned() switch {
                var (next, edits) => (next, edits, Response.Ignored),
            },
            act: static (held, act) => held.Engine.Acted(held.Frame, act));

    private (Engine<TControl, TPart, TValue>, Seq<Edit<TValue>>, Response) Pointed(SKRect frame, Input.Pointer pointer) =>
        pointer.Phase switch {
            PointerPhase.Down => Pressed(frame, pointer),
            PointerPhase.Move => Grip.Match(Some: held => Dragged(frame, pointer, held), None: () => Hovering(Control.Hit(frame, pointer.At))),
            PointerPhase.Up => Grip.Match(Some: held => Released(frame, pointer, held), None: () => Kept(Response.Ignored)),
            PointerPhase.Leave => Grip.IsSome ? Kept(Response.Ignored) : Hovering(None),
        };

    private (Engine<TControl, TPart, TValue>, Seq<Edit<TValue>>, Response) Pressed(SKRect frame, Input.Pointer pointer) =>
        Abandoned() switch {
            var (open, cancelled) => open.Gripped(frame, pointer, open.Control.Hit(frame, pointer.At)) switch {
                var (next, edits, response) => (next, cancelled.Concat(edits), response),
            },
        };

    private (Engine<TControl, TPart, TValue>, Seq<Edit<TValue>>, Response) Gripped(SKRect frame, Input.Pointer pointer, Option<TPart> part) =>
        Control.Behavior(part).Press(Control, new Pointing(frame, pointer.At, pointer.At, pointer.At, pointer.Buttons, pointer.Modifiers)).Match(
            Some: transition => Holding((this with { Grip = new Grip<TPart, TValue>(pointer.At, pointer.At, part, Dragging: false, Open: None) }).Applied(transition), part),
            None: () => Kept(Response.Ignored));

    private (Engine<TControl, TPart, TValue>, Seq<Edit<TValue>>, Response) Dragged(SKRect frame, Input.Pointer pointer, Grip<TPart, TValue> held) =>
        held.Dragging || SKPoint.Distance(held.Origin, pointer.At) > Routes.DragThreshold
            ? Applied(Control.Behavior(held.Part).Drag(Control, new Pointing(frame, held.Origin, held.Last, pointer.At, pointer.Buttons, pointer.Modifiers))) switch {
                var (next, edits) => (next.Moved(pointer.At), edits, Response.Handled),
            }
            : Kept(Response.Handled);

    private (Engine<TControl, TPart, TValue>, Seq<Edit<TValue>>, Response) Released(SKRect frame, Input.Pointer pointer, Grip<TPart, TValue> held) =>
        Applied(Control.Behavior(held.Part).Release(Control, new Pointing(frame, held.Origin, held.Last, pointer.At, pointer.Buttons, pointer.Modifiers))) switch {
            var (released, edits) => ((released with { Grip = None }).Recued(released.Control.Hit(frame, pointer.At)),
                edits.Concat(released.Grip.Bind(static grip => grip.Open).Map(static open => (Edit<TValue>)new Edit<TValue>.Commit(open)).ToSeq()),
                Response.Release),
        };

    private (Engine<TControl, TPart, TValue>, Seq<Edit<TValue>>, Response) Keyed(Input.Key key) =>
        key.Code is KeyCode.Escape && Grip.Exists(static held => held.Open.IsSome)
            ? Abandoned() switch {
                var (next, edits) => (next, edits, Response.Handled),
            }
            : Control.Behavior(Control.Cues.Hovered) switch {
                var behavior => Settle(behavior.Key(Control, key)
                    | Optional(Steps.GetValueOrDefault(key.Code)).Filter(static steps => steps is not 0).Bind(steps => behavior.Step(Control, steps, key.Modifiers))
                    | (key.Code is KeyCode.Enter or KeyCode.Space ? behavior.Activate(Control) : None)),
            };

    private (Engine<TControl, TPart, TValue>, Seq<Edit<TValue>>, Response) Scrolled(SKRect frame, Input.Scroll scroll) =>
        Control.Behavior(Control.Hit(frame, scroll.At)) switch {
            var behavior => Settle(behavior.Scroll(Control, new Pointing(frame, scroll.At, scroll.At, scroll.At, PointerButtons.None, scroll.Modifiers), scroll.Delta)
                | Optional(MathF.Sign(scroll.Delta.Y)).Filter(static steps => steps is not 0).Bind(steps => behavior.Step(Control, steps, scroll.Modifiers))),
        };

    private (Engine<TControl, TPart, TValue>, Seq<Edit<TValue>>, Response) Acted(SKRect frame, Input.Act act) =>
        Control.Hit(frame, act.At) switch {
            var part => act.Action switch {
                AccessAction.Press or AccessAction.Confirm => Settle(Control.Behavior(part).Activate(Control)),
                AccessAction.Increment => Stepped(part, 1, Modifiers.None),
                AccessAction.Decrement => Stepped(part, -1, Modifiers.None),
                AccessAction.Cancel => Abandoned() switch {
                    var (next, edits) => (next, edits, Response.Handled),
                },
                AccessAction.ShowMenu => Kept(Response.Ignored),
            },
        };

    private (Engine<TControl, TPart, TValue>, Seq<Edit<TValue>>, Response) Stepped(Option<TPart> part, int steps, Modifiers modifiers) =>
        steps is 0 ? Kept(Response.Ignored) : Settle(Control.Behavior(part).Step(Control, steps, modifiers));

    private (Engine<TControl, TPart, TValue>, Seq<Edit<TValue>>, Response) Settle(Option<Transition<TControl, TValue>> transition) =>
        transition.Match(
            Some: held => Applied(held) switch {
                var (next, edits) => (next, edits, Response.Handled),
            },
            None: () => Kept(Response.Ignored));

    private (Engine<TControl, TPart, TValue>, Seq<Edit<TValue>>, Response) Hovering(Option<TPart> part) =>
        (this with { Control = Control.Cued(Control.Cues with { Hovered = part }) }, Seq<Edit<TValue>>(), Response.Ignored);

    private (Engine<TControl, TPart, TValue>, Seq<Edit<TValue>>, Response) Kept(Response response) => (this, Seq<Edit<TValue>>(), response);

    private (Engine<TControl, TPart, TValue> Next, Seq<Edit<TValue>> Edits) Applied(Transition<TControl, TValue> transition) =>
        (new Engine<TControl, TPart, TValue>(transition.State.Cued(Control.Cues), Grip.Map(held => transition.Edit.Map(edit => Folded(held, edit)).IfNone(held))), transition.Edit.ToSeq());

    private (Engine<TControl, TPart, TValue> Next, Seq<Edit<TValue>> Edits) Abandoned() =>
        Grip.Bind(static held => held.Open).Match(
            Some: _ => Applied(new Transition<TControl, TValue>(Control, new Edit<TValue>.Cancel())) switch {
                var (next, edits) => ((next with { Grip = None }).Recued(Control.Cues.Hovered), edits),
            },
            None: () => ((this with { Grip = None }).Recued(Control.Cues.Hovered), Seq<Edit<TValue>>()));

    private static (Engine<TControl, TPart, TValue>, Seq<Edit<TValue>>, Response) Holding((Engine<TControl, TPart, TValue> Next, Seq<Edit<TValue>> Edits) applied, Option<TPart> part) =>
        (applied.Next.Recued(part), applied.Edits, Response.Capture);

    private Engine<TControl, TPart, TValue> Moved(SKPoint at) => this with { Grip = Grip.Map(grip => grip with { Last = at, Dragging = true }) };

    private Engine<TControl, TPart, TValue> Recued(Option<TPart> hovered) => this with { Control = Control.Cued(new Interaction<TPart>(hovered, Grip.Bind(static held => held.Part))) };

    private static Grip<TPart, TValue> Folded(Grip<TPart, TValue> grip, Edit<TValue> edit) =>
        edit.Switch(
            grip,
            preview: static (held, preview) => held with { Open = Some(preview.Value) },
            step: static (held, _) => held,
            commit: static (held, _) => held with { Open = None },
            cancel: static (held, _) => held with { Open = None });
}
