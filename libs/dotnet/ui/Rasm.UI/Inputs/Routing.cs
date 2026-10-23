using Rasm.UI.Layout;
using Rasm.UI.Scenes;
using SkiaSharp;

namespace Rasm.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Press(SKPoint At, PointerButtons Button, TimeSpan Time, int Count) {
    public bool Near(SKPoint at, PointerButtons button, TimeSpan time) =>
        Button == button && SKPoint.Distance(At, at) <= Routes.ClickDistance && time - Time <= Routes.ClickTime;
}

public sealed record Routing(Option<string> Hover, Option<string> Captured, Seq<string> Focus, Option<Press> Pressed, Option<Press> Clicked) {
    public static Routing Idle { get; } = new(None, None, [], None, None);

    public Routing Pruned(Laid root) =>
        toHashSet(root.Flattened.Map(static laid => laid.Id)) switch {
            var present => this with {
                Hover = Hover.Filter(present.Contains),
                Captured = Captured.Filter(present.Contains),
                Focus = Focus.Filter(present.Contains),
            },
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Routes {
    // --- [THRESHOLDS]
    public const float ClickDistance = 5f;
    public const float DragThreshold = 3f;
    public static readonly TimeSpan ClickTime = TimeSpan.FromMilliseconds(500);

    // --- [DISPATCH]
    public static IO<(Routing Routing, Response Response)> Dispatch(IO<Scene> scene, Routing routing, Input input) =>
        input.Switch(
            (Scene: scene, Routing: routing, Input: input),
            pointer: static (held, pointer) => held.Scene.Bind(current => Pointed(held.Scene, current.Layout.Chain(pointer.At), held.Routing, pointer)),
            click: static (held, click) => Settled(held.Routing, Deliver(held.Scene, current => current.Layout.Chain(click.At), held.Input)),
            scroll: static (held, scroll) => Settled(held.Routing, Deliver(held.Scene, current => Targets(current, held.Routing, scroll.At)
                .Filter(laid => !laid.Node.Focusable || held.Routing.Focus.Head == laid.Id), held.Input)),
            gestured: static (held, gestured) => Settled(held.Routing, Deliver(held.Scene, current => Targets(current, held.Routing, gestured.At), held.Input)),
            key: static (held, _) => Settled(held.Routing, Deliver(held.Scene, current => Focused(current, held.Routing), held.Input)),
            composition: static (held, _) => Settled(held.Routing, Deliver(held.Scene, current => Focused(current, held.Routing), held.Input)),
            focus: static (held, _) => Settled(held.Routing, Deliver(held.Scene, current => Focused(current, held.Routing), held.Input)),
            act: static (held, act) => Settled(held.Routing, Deliver(held.Scene, current => current.Layout.Chain(act.At), held.Input)));

    private static IO<(Routing Routing, Response Response)> Pointed(IO<Scene> scene, Seq<Laid> hit, Routing routing, Input.Pointer pointer) {
        Routing hovered = routing with { Hover = hit.Find(static laid => laid.Node.Handler.IsSome).Map(static laid => laid.Id) };
        Seq<string> focus = hit.Find(static laid => laid.Node.Focusable).Match(Some: laid => laid.Id.Cons(routing.Focus.Filter(id => !string.Equals(id, laid.Id, StringComparison.Ordinal))), None: () => routing.Focus);
        IO<Unit> left = Notified(scene, routing.Hover.Filter(id => hovered.Hover != id), pointer with { Phase = PointerPhase.Leave });
        return pointer.Phase switch {
            PointerPhase.Down =>
                from exited in left
                from blurred in when(focus.Head != routing.Focus.Head, Notified(scene, routing.Focus.Head, new Input.Focus(Gained: false))).As()
                from entered in when(focus.Head != routing.Focus.Head, Notified(scene, focus.Head, new Input.Focus(Gained: true))).As()
                from delivered in Deliver(scene, current => Targets(current, routing, pointer.At), pointer)
                select (Captured(hovered with { Pressed = new Press(pointer.At, pointer.Buttons, pointer.Time, 0), Focus = focus }, delivered), delivered.Response),
            PointerPhase.Move =>
                from exited in left
                from delivered in Deliver(scene, current => Targets(current, routing, pointer.At), pointer)
                select (Captured(hovered, delivered), delivered.Response),
            PointerPhase.Up =>
                from exited in left
                from delivered in Deliver(scene, current => Targets(current, routing, pointer.At), pointer)
                from clicked in Clicked(scene, hovered with { Pressed = None, Captured = None }, routing.Pressed, pointer, delivered.Response)
                select clicked,
            PointerPhase.Leave => Deliver(scene, current => routing.Hover.Bind(current.Layout.Path).IfNone([]), pointer).Map(delivered => (routing with { Hover = None }, delivered.Response)),
        };
    }

    private static IO<(Routing Routing, Response Response)> Clicked(IO<Scene> scene, Routing routing, Option<Press> pressed, Input.Pointer pointer, Response released) =>
        pressed.Filter(press => press.Near(pointer.At, press.Button, pointer.Time)).Match(
            Some: press => routing.Clicked.Filter(prior => prior.Near(pointer.At, press.Button, pointer.Time)).Map(static prior => prior.Count + 1).IfNone(1) switch {
                var count => Deliver(scene, current => current.Layout.Chain(pointer.At), new Input.Click(pointer.At, press.Button, pointer.Modifiers, count)).Map(delivered =>
                    (routing with { Clicked = new Press(pointer.At, press.Button, pointer.Time, count) }, released is Response.Ignored ? delivered.Response : released)),
            },
            None: () => IO.pure((routing, released)));

    private static IO<Unit> Notified(IO<Scene> scene, Option<string> target, Input input) =>
        Deliver(scene, current => target.Bind(current.Layout.Path).Map(static path => path.Take(1)).IfNone([]), input).Map(static _ => unit);

    private static IO<(Routing Routing, Response Response)> Settled(Routing routing, IO<(Option<Laid> By, Response Response)> delivery) =>
        delivery.Map(delivered => (Captured(routing, delivered), delivered.Response));

    private static Routing Captured(Routing routing, (Option<Laid> By, Response Response) delivered) =>
        delivered.Response switch {
            Response.Capture => routing with { Captured = delivered.By.Map(static laid => laid.Id) },
            Response.Release => routing with { Captured = None },
            Response.Handled or Response.Ignored => routing,
        };

    private static IO<(Option<Laid> By, Response Response)> Deliver(IO<Scene> scene, Func<Scene, Seq<Laid>> chain, Input input) =>
        scene.Bind(current => chain(current).Choose(static laid => laid.Node.Handler.Map(handler => (Laid: laid, Handler: handler)))
            .Fold(IO.pure((By: Option<Laid>.None, Response: Response.Ignored)), (prior, next) => prior.Bind(held => held.Response is Response.Ignored
                ? next.Handler(input, next.Laid).Map(response => (By: Some(next.Laid), Response: response))
                : IO.pure(held))));

    private static Seq<Laid> Targets(Scene scene, Routing routing, SKPoint at) => routing.Captured.Bind(scene.Layout.Path).IfNone(() => scene.Layout.Chain(at));

    private static Seq<Laid> Focused(Scene scene, Routing routing) => routing.Focus.Head.Bind(scene.Layout.Path).IfNone([]);
}
