using Eto.Drawing;
using Grasshopper2;
using Grasshopper2.UI.Skinning;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Styling;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.Grasshopper.UI.Surfaces;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CanvasTheme {
    // --- [PROJECTION]
    public static IO<Theme> Current =>
        IO.lift(static () => (Skin: Skin.Current, Font: Skin.Current.Shape.FontWord) switch {
            var held => Theme.Create(
                HostTheme.Projected(compact: false).Chrome.AddOrUpdateRange(Shaded(held.Skin.Shades)),
                Settings.Animations.Value ? HostTheme.Conditions : HostTheme.Conditions.TryAdd(Condition.ReduceMotion),
                new TextFace { Family = held.Font.FamilyName },
                held.Font.Size),
        });

    private static Seq<(Token Token, SKColorF Ink)> Shaded(Shades shades) =>
        Seq<(ShadeKind Kind, Option<Condition> Condition)>((ShadeKind.Normal, None), (ShadeKind.Disabled, Condition.Disabled), (ShadeKind.NormalSelected, Condition.Selected))
            .Bind(row => shades[row.Kind] switch {
                var shade => Seq<(Channel Channel, Color Ink)>((Channel.Fill, shade.Slab), (Channel.Edge, shade.Edge), (Channel.Text, shade.Text))
                    .Map(channel => (new Token(Role.Window, channel.Channel) { Condition = row.Condition }, Colored(channel.Ink))),
            });

    private static SKColorF Colored(Color color) => new(color.R, color.G, color.B, color.A);

    // --- [CHANGE]
    public static readonly HostEvent<Unit> Changed =
        Subscriptions.Host(typeof(CanvasTheme), static () => Seq(
            GrasshopperEvents.SkinChanged.Choose(static _ => Some(unit)),
            GrasshopperEvents.SettingChanged(Settings.Animations).Choose(static _ => Some(unit)),
            HostTheme.Changed));
}
