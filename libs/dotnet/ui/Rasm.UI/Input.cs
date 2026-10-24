using System.Numerics;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI;

// --- [TYPES] ---------------------------------------------------------------------------
public enum PointerPhase {
    Down,
    Move,
    Up,
    Leave,
}

public enum GesturePhase {
    Begins,
    Continues,
    Ends,
}

public enum Heading {
    Back,
    Forth,
}

public enum Cursor {
    Default,
    Pointer,
    Text,
    Crosshair,
    Move,
    ColumnResize,
    RowResize,
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Modifiers(bool Common, bool Shift, bool Alt) {
    public static Modifiers Plain { get; } = new(Common: false, Shift: false, Alt: false);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record Act {
    public sealed record Press : Act;

    public sealed record Confirm : Act;

    public sealed record Cancel : Act;

    public sealed record Delete : Act;

    public sealed record Menu : Act;

    public sealed record All : Act;

    public sealed record Reveal : Act;

    public sealed record Step(Orientation Along, int Count) : Act;

    public sealed record Page(Orientation Along, int Count) : Act;

    public sealed record Extreme(Orientation Along, Heading Toward) : Act;

    public sealed record Tab(Heading Toward) : Act;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, SwitchMethods = SwitchMapMethodsGeneration.DefaultWithPartialOverloads)]
public abstract partial record Input {
    public sealed record Pointer(PointerPhase Phase, SKPoint At, Modifiers Modifiers, int Count, Option<float> Pressure) : Input;

    public sealed record Menu(SKPoint At, Modifiers Modifiers) : Input;

    public sealed record Scroll(SKPoint At, SKPoint Delta, Modifiers Modifiers, bool Precise, GesturePhase Phase) : Input;

    public sealed record Pan(SKPoint At, SKPoint Translation, SKPoint Velocity, GesturePhase Phase) : Input;

    public sealed record Magnify(SKPoint At, float Amount, GesturePhase Phase) : Input;

    public sealed record Rotate(SKPoint At, float Radians, GesturePhase Phase) : Input;

    public sealed record Perform(Act Act, Modifiers Modifiers, Option<Seq<Key>> Target) : Input;

    public sealed record Edit(EditCommand Command, Option<Perform> Otherwise) : Input;

    public sealed record Focus(bool Gained) : Input;

    public sealed record Enter(Heading Toward) : Input;
}

public sealed record Routing(Option<Seq<Key>> Hover, Option<Seq<Key>> Captured, Option<Seq<Key>> Latched, Option<Seq<Key>> Focus) {
    // --- [LIFETIME]
    public static Routing Idle { get; } = new(None, None, None, None);

    public Routing Pruned(Scene scene) =>
        new(Hover.Filter(scene.Index.ContainsKey), Captured.Filter(scene.Index.ContainsKey), Latched.Filter(scene.Index.ContainsKey), Focus.Filter(scene.Index.ContainsKey));

    // --- [FOCUS]
    private const float Detour = 2f;

    public Option<Seq<Key>> Next(Scene scene, Heading toward, bool cycles) =>
        (Runs: Runs(scene), Sign: toward switch { Heading.Back => -1, Heading.Forth => 1 }) switch {
            var (runs, sign) => Focus.Bind(focus => runs.Map(static (run, index) => (Run: run, Index: index)).Find(held => held.Run.Exists(path => path == focus))).Match(
                    Some: held => held.Index + sign,
                    None: () => sign > 0 ? 0 : runs.Count - 1) switch {
                        var target when !runs.IsEmpty && (cycles || (target >= 0 && target < runs.Count)) => Entered(runs[((target % runs.Count) + runs.Count) % runs.Count], toward),
                        _ => None,
                    },
        };

    public Option<Seq<Key>> Toward(Scene scene, Orientation along, int sign) =>
        from focus in Focus
        from origin in scene.Frame(focus)
        from run in Runs(scene).Find(run => run.Exists(path => path == focus))
        from target in Toward(origin, run.Filter(path => path != focus).Choose(path => scene.Frame(path).Map(frame => (Item: path, Frame: frame))), along, sign)
        select target;

    internal static Option<T> Toward<T>(SKRect origin, Seq<(T Item, SKRect Frame)> candidates, Orientation along, int sign) =>
        (along.Forward * sign) switch {
            var heading => candidates
                .Map(candidate => (candidate.Item, Offset: new Vector2(candidate.Frame.MidX - origin.MidX, candidate.Frame.MidY - origin.MidY)))
                .Map(held => (held.Item, Distance: held.Offset.Length(), Turn: 1f - Vector2.Dot(Vector2.Normalize(held.Offset), heading)))
                .Filter(static held => held.Turn < 1f) switch {
                    var ahead => ahead.IsEmpty ? None : Some(ahead.MinBy(static held => held.Distance * (1f + (Detour * held.Turn))).Item),
                },
        };

    private static Option<Seq<Key>> Entered(Seq<Seq<Key>> run, Heading toward) =>
        toward switch {
            Heading.Back => run.Last,
            Heading.Forth => run.Head,
        };

    private static Seq<Seq<Seq<Key>>> Runs(Scene scene) =>
        scene.Order.Choose(static laid => laid.Node.Part.Map(part => (laid.Path, Pressed: part is Part.Press)))
            .Filter(stop => !stop.Pressed || !Scene.Above(stop.Path).Tail.Exists(prefix => scene.Index.Find(prefix).Exists(static owner => owner.Node.Part.Exists(static part => part is Part.Choice))))
            .Map(stop => (stop.Path, Bar: stop.Pressed && scene.Index.Find(stop.Path.Init).Exists(static parent => parent.Node.Role == Some(Role.Toolbar))))
            .Fold(Seq<Seq<(Seq<Key> Path, bool Bar)>>(), static (runs, stop) =>
                runs.Last.Bind(static run => run.Last).Exists(prior => stop.Bar && prior.Bar && prior.Path.Init == stop.Path.Init)
                    ? runs.Init.Add(runs.Last.Map(run => run.Add(stop)).IfNone([stop]))
                    : runs.Add([stop]))
            .Map(static run => run.Map(static stop => stop.Path));
}
