using Eto.Drawing;
using Grasshopper2;
using Grasshopper2.UI.Skinning;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.UI.Styling;
using Rasm.UI;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.Grasshopper.UI.Surfaces;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CanvasTheme {
    // --- [PLACEMENT]
    public static HostPlacement Placement => HostPlacement.Panel;

    // --- [PROJECTION]
    public static IO<ColorScheme> Current =>
        IO.lift(static () => Skin.Current switch {
            var skin => ColorScheme.Validated(
                HostTheme.Projected(compact: false).Chrome.AddOrUpdateRange(Shaded(skin)),
                Settings.Animations.Value ? HostTheme.Conditions : HostTheme.Conditions.TryAdd(Condition.ReduceMotion),
                new TextFace { Family = skin.Shape.FontWord.FamilyName },
                skin.Shape.FontWord.Size),
        });

    private static Seq<(Token Token, SKColorF Color)> Shaded(Skin skin) =>
        (from row in Seq<(ShadeKind Kind, Option<Condition> Condition)>((ShadeKind.Normal, None), (ShadeKind.Disabled, Condition.Disabled), (ShadeKind.NormalSelected, Condition.Selected))
         let shade = skin.Shades[row.Kind]
         from channel in Seq((Channel: Channel.Fill, Color: shade.Slab), (Channel: Channel.Stroke, Color: shade.Edge), (Channel: Channel.Text, Color: shade.Text))
         select (new Token(Placement.Root, channel.Channel) { Condition = row.Condition }, Painted(channel.Color)))
        + Seq(
            (new Token(Role.Window, Channel.Text) { Condition = Condition.Warning }, Painted(skin.Messaging.Warning)),
            (new Token(Role.Window, Channel.Text) { Condition = Condition.Faulted }, Painted(skin.Messaging.Error)));

    private static SKColorF Painted(Color color) => new(color.R, color.G, color.B, color.A);

    // --- [CHANGE]
    public static readonly HostEvent<Unit> Changed =
        Subscriptions.Merged(
            GrasshopperEvents.SkinChanged.Choose(static _ => Some(unit)),
            GrasshopperEvents.SettingChanged(Settings.Animations).Choose(static _ => Some(unit)),
            HostTheme.Changed);
}
