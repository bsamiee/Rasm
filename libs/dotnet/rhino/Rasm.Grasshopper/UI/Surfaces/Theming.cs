using Grasshopper2;
using Grasshopper2.UI.Skinning;
using Rasm.Rhino;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Styling;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.Grasshopper.UI.Surfaces;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CanvasTheme {
    // --- [PROJECTION]
    public static IO<Theme> Current =>
        IO.lift(static () => Skin.Current switch {
            var skin => ValidationResults.ToFin(Theme.Validate(
                HostTheme.Projected(compact: false).Chrome.AddOrUpdateRange(Shaded(skin.Shades)),
                Settings.Animations.Value ? HostTheme.Conditions : HostTheme.Conditions.TryAdd(Condition.ReduceMotion),
                new TextFace { Family = skin.Shape.FontWord.FamilyName },
                skin.Shape.FontWord.Size,
                out Theme? theme), theme),
        });

    private static Seq<(Token Token, SKColorF Ink)> Shaded(Shades shades) =>
        from row in Seq<(ShadeKind Kind, Option<Condition> Condition)>((ShadeKind.Normal, None), (ShadeKind.Disabled, Condition.Disabled), (ShadeKind.NormalSelected, Condition.Selected))
        let shade = shades[row.Kind]
        from channel in Seq((Channel: Channel.Fill, Ink: shade.Slab), (Channel: Channel.Edge, Ink: shade.Edge), (Channel: Channel.Text, Ink: shade.Text))
        select (new Token(Role.Window, channel.Channel) { Condition = row.Condition }, Conversions.ToSKColorF(channel.Ink));

    // --- [CHANGE]
    public static readonly HostEvent<Unit> Changed =
        Subscriptions.Host(typeof(CanvasTheme), static () => Seq(
            GrasshopperEvents.SkinChanged.Choose(static _ => Some(unit)),
            GrasshopperEvents.SettingChanged(Settings.Animations).Choose(static _ => Some(unit)),
            HostTheme.Changed));
}
