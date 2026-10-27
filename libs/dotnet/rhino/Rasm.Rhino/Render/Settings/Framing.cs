using System.Drawing;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI;
using Rhino.Render;
using Rhino.Render.DataSources;

namespace Rasm.Rhino.Render.Settings;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record RenderSize {
    public abstract void Applied(RenderSettings settings);

    public sealed record ViewportSize(Size ScreenPort) : RenderSize {
        public override void Applied(RenderSettings settings) => (settings.UseViewportSize, settings.ImageSize) = (true, ScreenPort);
    }

    public sealed record CustomSize : RenderSize {
        public override void Applied(RenderSettings settings) => settings.UseViewportSize = false;
    }

    public sealed record Listed(Size Size) : RenderSize {
        public override void Applied(RenderSettings settings) => (settings.UseViewportSize, settings.ImageSize) = (false, Size);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Framing {
    // --- [EXTENT]
    public static ReadModel<Size> Extent { get; } = new(
        [ReadModels.Marks(EventKind.DocumentPropertiesChanged), ReadModels.Marks(EventKind.UndoRedo)],
        static doc => IO.lift(() => RenderPipeline.RenderSize(doc, fromRenderSources: true)));

    // --- [SIZES]
    public static Seq<RenderSize> Sizes(RhinoSettings settings, Size screenPort) =>
        [new RenderSize.ViewportSize(screenPort), new RenderSize.CustomSize(), .. toSeq(settings.GetCustomRenderSizes()).Map(static size => new RenderSize.Listed(size))];

    public static IO<Unit> Choose(RenderSettingsSource.Section section, RenderSize size, object owner) =>
        from sized in RenderSettingsSources.Edit(section, scope => IO.lift(() => size.Applied(scope.Settings)), owner)
        from preset in SectionModels.Write(section.Host, Provider.RhinoSettings, data => IO.lift(() => data.CustomImageSizeIsPreset = size.Map(viewportSize: false, customSize: false, listed: true)))
        select unit;

    // --- [GUIDES]
    public static Option<Rectangle> Action(SafeFrame frame, Size size) =>
        Guide(size, frame.Enabled && frame.ActionFrameOn, frame.ActionFrameXScale, frame.ActionFrameLinked ? frame.ActionFrameXScale : frame.ActionFrameYScale);

    public static Option<Rectangle> Title(SafeFrame frame, Size size) =>
        Guide(size, frame.Enabled && frame.TitleFrameOn, frame.TitleFrameXScale, frame.TitleFrameLinked ? frame.TitleFrameXScale : frame.TitleFrameYScale);

    private static Option<Rectangle> Guide(Size size, bool on, double x, double y) =>
        on
            ? Some(Rectangle.FromLTRB(Edge(size.Width, (1d - x) / 2d), Edge(size.Height, (1d - y) / 2d), Edge(size.Width, (1d + x) / 2d), Edge(size.Height, (1d + y) / 2d)))
            : None;

    private static int Edge(int side, double fraction) => (int)((float)fraction * side);
}
