using Rasm.UI.Layout;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Scenes;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Reserved {
    public sealed record Slot(string Id, SKRect Frame) : Reserved;
    public sealed record Backdrop(Glass Glass, SKRect Frame, float Radius) : Reserved;
}

public sealed record Scene(Theme Theme, SKSize Size, Laid Layout) {
    // --- [RECONCILE]
    public static Scene Build(Theme theme, Node root, SKSize size) => new(theme, size, Layouts.Arrange(theme, root, size));

    public (Scene Scene, Seq<SKRect> Damage) Reconcile(Node root) =>
        Build(Theme, root, Size) switch {
            var next => (next, Damage(next)),
        };

    public (Scene Scene, Seq<SKRect> Damage) Restyle(Theme theme) => Whole(Build(theme, Layout.Node, Size));

    public (Scene Scene, Seq<SKRect> Damage) Resize(SKSize size) => Whole(Build(Theme, Layout.Node, size));

    public Seq<SKRect> Damage(Scene next) {
        HashMap<string, Laid> prior = toHashMap(Layout.Flattened.Map(static laid => (laid.Id, laid)));
        Seq<Laid> after = next.Layout.Flattened;
        LanguageExt.HashSet<string> present = toHashSet(after.Map(static laid => laid.Id));
        return after.Bind(laid => prior.Find(laid.Id).Match(
                Some: held => held.Signature == laid.Signature ? Seq<SKRect>() : Seq(held.Bounds(Theme), laid.Bounds(next.Theme)),
                None: () => Seq(laid.Bounds(next.Theme))))
            .Concat(prior.Values.Filter(held => !present.Contains(held.Id)).Map(held => held.Bounds(Theme)))
            .Distinct();
    }

    // --- [QUERY]
    public bool Opaque(SKPoint at) => Layout.Chain(at).Exists(static laid => laid.Look.IsSome || laid.Node.Leaf);

    public Seq<Reserved> Reservations =>
        Layout.Flattened.Choose(laid => laid.Node is Node.Slot slot
            ? new Reserved.Slot(slot.Id, laid.Frame)
            : from look in laid.Look
              from backdrop in Optional(look.Fill as Material.Backdrop)
              where !Theme.Display.ReduceTransparency
              select (Reserved)new Reserved.Backdrop(backdrop.Glass, laid.Frame, look.Radius.Of(Theme.Metrics, laid.Frame.Size)));

    private static (Scene Scene, Seq<SKRect> Damage) Whole(Scene next) => (next, Seq(SKRect.Create(next.Size)));
}
