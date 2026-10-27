using Eto.Drawing;
using Grasshopper2.Doc;
using Grasshopper2.UI.Canvas;
using Rasm.RhinoCommon;

namespace Rasm.Grasshopper.UI;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Viewing {
    // --- [NAVIGATION]
    public static IO<Unit> Navigate(Canvas canvas, NamedView view, (float min, float max) limits, Duration duration) =>
        (from frame in IO.lift(() => GrasshopperFault.Unless(view.ResolveRectangle(canvas.Document, out RectangleF found)).Map(_ => found))
         from _ in IO.lift(() => canvas.Navigate(frame, limits, duration.ToTimeSpan()))
         select unit)
        .OnUiThread();

    // --- [PICTURES]
    public static IO<Bitmap> Picture(Canvas canvas, RectangleF region, Size pixels, bool drawBackground, bool drawWires, bool drawMessages) =>
        unitIO.Bind(_ => Products.Made(canvas.DrawToBitmap(region, pixels.Width, pixels.Height, drawBackground, drawWires, drawMessages))).OnUiThread();
}
