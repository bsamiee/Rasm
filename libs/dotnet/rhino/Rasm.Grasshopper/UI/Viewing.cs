using Eto.Drawing;
using Grasshopper2.Doc;
using Grasshopper2.UI.Canvas;
using Rasm.Rhino;
using Rasm.Rhino.UI.Assets;

namespace Rasm.Grasshopper.UI;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Viewing {
    // --- [DURATIONS]
    public static IO<Grasshopper2.UI.Animation.Duration> Length(Grasshopper2.UI.Animation.Duration duration) =>
        IO.lift(() => HostTheme.Accessibility.ReduceMotion ? Grasshopper2.UI.Animation.Duration.Abrupt : duration);

    // --- [NAVIGATION]
    public static IO<Unit> Navigate(Canvas canvas, NamedView view, (float min, float max) limits, Grasshopper2.UI.Animation.Duration duration) =>
        (from length in Length(duration)
         from frame in IO.lift(() => Missing.Unless(view.ResolveRectangle(canvas.Document, out RectangleF found), found, nameof(NamedView.ResolveRectangle)))
         from _ in IO.lift(() => canvas.Navigate(frame, limits, length))
         select unit)
        .OnUiThread();

    // --- [PICTURES]
    public static IO<Bitmap> Picture(Canvas canvas, RectangleF region, Size pixels, bool drawBackground, bool drawWires, bool drawMessages, Action<IDisposable> retain) =>
        DisposalOps.Acquire(IO.lift(() => Missing.Unless(
            canvas.DrawToBitmap(region, pixels.Width, pixels.Height, drawBackground, drawWires, drawMessages),
            nameof(Canvas.DrawToBitmap))), static bitmap => IO.lift(bitmap.Dispose), retain)
            .OnUiThread();
}
