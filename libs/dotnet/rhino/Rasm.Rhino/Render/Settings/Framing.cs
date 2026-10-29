using System.Drawing;
using Rasm.Rhino.Render.Views;
using Rhino;
using Rhino.Commands;
using Rhino.Render;
using Rhino.Render.DataSources;

namespace Rasm.Rhino.Render.Settings;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RenderSize(Option<Size> Size, bool Viewport) {
    public bool Preset => !Viewport && Size.IsSome;

    internal Step<RenderSettings> Applied =>
        new(settings => IO.lift(() => {
            settings.UseViewportSize = Viewport;
            _ = Size.Iter(size => settings.ImageSize = size);
            return settings;
        }), nameof(RenderSize));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Framing {
    // --- [EXTENT]
    public static ReadModel<Size> Extent { get; } = new(
        [ReadModels.Marks(Subscriptions.On<DocumentEventArgs>(static h => RhinoDoc.DocumentPropertiesChanged += h, static h => RhinoDoc.DocumentPropertiesChanged -= h).Documents()),
         ReadModels.Marks(Subscriptions.On<UndoRedoEventArgs>(static h => Command.UndoRedo += h, static h => Command.UndoRedo -= h).Documents(static args => args.DocumentSerialNumber))],
        static doc => IO.lift(() => RenderPipeline.RenderSize(doc, fromRenderSources: true)));

    // --- [SIZES]
    public static Seq<RenderSize> Sizes(RhinoSettings settings, Size screenPort) =>
        [new(Some(screenPort), Viewport: true), new(None, Viewport: false), .. toSeq(settings.GetCustomRenderSizes()).Map(static size => new RenderSize(Some(size), Viewport: false))];

    public static IO<Unit> Choose(RenderSettingsSource.Section section, RenderSize size) =>
        from sized in RenderSettingsSources.Edit(section, scope => size.Applied.Run(scope.Settings))
        from preset in SectionModels.Write(section.Host, Provider.RhinoSettings, data => IO.lift(() => data.CustomImageSizeIsPreset = size.Preset))
        select unit;

    // --- [GUIDES]
    public static Option<Rectangle> Action(SafeFrame frame, Size size) =>
        Guide(size, frame.Enabled && frame.ActionFrameOn, frame.ActionFrameXScale, frame.ActionFrameLinked ? frame.ActionFrameXScale : frame.ActionFrameYScale);

    public static Option<Rectangle> Title(SafeFrame frame, Size size) =>
        Guide(size, frame.Enabled && frame.TitleFrameOn, frame.TitleFrameXScale, frame.TitleFrameLinked ? frame.TitleFrameXScale : frame.TitleFrameYScale);

    private static Option<Rectangle> Guide(Size size, bool on, double x, double y) =>
        Conversions.Present(on, Rectangle.FromLTRB(Edge(size.Width, (1d - x) / 2d), Edge(size.Height, (1d - y) / 2d), Edge(size.Width, (1d + x) / 2d), Edge(size.Height, (1d + y) / 2d)));

    private static int Edge(int side, double fraction) => (int)((float)fraction * side);
}
