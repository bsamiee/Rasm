using Eto.Drawing;
using Grasshopper2.Doc;
using Grasshopper2.UI.Canvas;
using Rasm.Rhino;
using Rasm.Rhino.UI.Styling;

namespace Rasm.Grasshopper.UI;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Viewing {
    // --- [NAVIGATION]
    public static IO<Unit> Navigate(Canvas canvas, NamedView view, (float min, float max) limits, Grasshopper2.UI.Animation.Duration duration) =>
        (from length in IO.lift(() => HostTheme.Accessibility.ReduceMotion ? Grasshopper2.UI.Animation.Duration.Abrupt : duration)
         from frame in IO.lift(() => view.ResolveRectangle(canvas.Document, out RectangleF found) ? Fin.Succ(found) : new Refused(nameof(NamedView.ResolveRectangle), Refused.Absent))
         from _ in IO.lift(() => canvas.Navigate(frame, limits, length))
         select unit)
        .OnUiThread();

    // --- [PICTURES]
    public static IO<Bitmap> Picture(Canvas canvas, RectangleF region, Size pixels, bool drawBackground, bool drawWires, bool drawMessages, Action<IDisposable> retain) =>
        DisposalOps.Acquire(IO.lift(() => Refused.UnlessFound(
            canvas.DrawToBitmap(region, pixels.Width, pixels.Height, drawBackground, drawWires, drawMessages),
            nameof(Canvas.DrawToBitmap))), retain)
            .OnUiThread();
}
