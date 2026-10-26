using Grasshopper2;
using Grasshopper2.UI.Skinning;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Styling;
using Rasm.UI;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.Grasshopper.UI.Surfaces;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CanvasTheme {
    // --- [PROJECTION]
    public static IO<ColorScheme> Current =>
        IO.lift(static () => Skin.Current switch {
            var skin => ColorScheme.TryCreate(
                HostTheme.Projected(compact: false).Chrome.AddOrUpdateRange(Shaded(skin.Shades)),
                Settings.Animations.Value ? HostTheme.Conditions : HostTheme.Conditions.TryAdd(Condition.ReduceMotion),
                new TextFace { Family = skin.Shape.FontWord.FamilyName },
                skin.Shape.FontWord.Size,
                out ColorScheme? theme,
                out Invalid? fault) ? Fin.Succ(theme) : fault,
        });

    private static Seq<(Token Token, SKColorF Color)> Shaded(Shades shades) =>
        from row in Seq<(ShadeKind Kind, Option<Condition> Condition)>((ShadeKind.Normal, None), (ShadeKind.Disabled, Condition.Disabled), (ShadeKind.NormalSelected, Condition.Selected))
        let shade = shades[row.Kind]
        from channel in Seq((Channel: Channel.Fill, Color: shade.Slab), (Channel: Channel.Stroke, Color: shade.Edge), (Channel: Channel.Text, Color: shade.Text))
        select (new Token(Role.Window, channel.Channel) { Condition = row.Condition }, new SKColorF(channel.Color.R, channel.Color.G, channel.Color.B, channel.Color.A));

    // --- [CHANGE]
    public static readonly HostEvent<Unit> Changed =
        Subscriptions.Host(typeof(CanvasTheme), static () => Seq(
            GrasshopperEvents.SkinChanged.Choose(static _ => Some(unit)),
            GrasshopperEvents.SettingChanged(Settings.Animations).Choose(static _ => Some(unit)),
            HostTheme.Changed));
}
