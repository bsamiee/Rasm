using System.Globalization;
using Eto;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Drafting;
using Rasm.Imaging.Files;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI;
using Rasm.UI;
using Rasm.UI.Styling;
using Rhino;
using Rhino.UI;
using SkiaSharp;

namespace Rasm.Rhino.Viewport.Printing;

// --- [TYPES] ---------------------------------------------------------------------------
internal delegate Fin<Unit> PdfPagePrinter(Graphics graphics, SKData page);

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PrintJob(LocalizeStringPair Name, SheetOrientation Orientation, PixelDensity Resolution, IterableNE<Func<SKSize, Node>> Pages, Rasm.UI.Styling.Theme Theme);

[SmartEnum]
public sealed partial class PrintView {
    public static readonly PrintView Print = new(static (document, doc) =>
        DisposalOps.Within(static () => new PrintDialog(), dialog => IO.lift(() => dialog.ShowDialog(RhinoEtoApp.MainWindowForDocument(doc), document))));

    public static readonly PrintView Preview = new(static (document, doc) =>
        DisposalOps.Within(() => new PrintPreviewDialog(document), dialog => IO.lift(() => dialog.ShowDialog(RhinoEtoApp.MainWindowForDocument(doc)))));

    [UseDelegateFromConstructor]
    public partial IO<DialogResult> ShowDialog(PrintDocument document, RhinoDoc doc);

    public IO<Unit> Show(RhinoDoc doc, PrintJob job, IPlugInSink sink) =>
        from attended in HostDialogs.Attended
        from shipped in Styles.Embedded
        from printer in IO.lift(static () => Refused.UnlessFound(Platform.Instance.Find<PdfPagePrinter>(), nameof(PdfPagePrinter)).Map(static create => create()))
        let pages = job.Pages.ToSeq().Strict()
        from result in DisposalOps.Within(static () => new PrintDocument(), document =>
            DisposalOps.Within<IDisposable, DialogResult>(retain =>
                from configured in IO.lift(() => {
                    document.Name = job.Name.Local;
                    document.PageCount = pages.Count;
                    document.PrintSettings.Orientation = job.Orientation.Map(portrait: PageOrientation.Portrait, landscape: PageOrientation.Landscape);
                })
                from attached in Subscriptions.Attach(
                    h => document.PrintPage += h,
                    h => document.PrintPage -= h,
                    Callbacks.Synchronous<PrintPageEventArgs>(
                        page => Printed(page, pages[page.CurrentPage], job, shipped, printer),
                        new CallbackSite(sink, typeof(PrintDocument), nameof(PrintDocument.PrintPage))), retain)
                from shown in ShowDialog(document, doc)
                select shown, DisposalOps.Release))
        from accepted in unless(result is DialogResult.Ok, IO.fail<Unit>(Errors.Cancelled)).As()
        select accepted;

    private static IO<Unit> Printed(PrintPageEventArgs page, Func<SKSize, Node> layout, PrintJob job, Shipped shipped, PdfPagePrinter printer) =>
        from size in IO.pure(new SKSize(page.PageSize.Width, page.PageSize.Height))
        from mounted in IO.lift(() => Mount.Create(new Host(size, job.Theme, Visibility.Shown, Cycles: false, CultureInfo.CurrentUICulture), shipped, layout(size), HashMap<Seq<Key>, float>()))
        from printed in DisposalOps.Within(
            () => Paged(size, job.Resolution, mounted.Mount),
            pdf => IO.lift(() => printer(page.Graphics, pdf)))
        select printed;

    private static SKData Paged(SKSize size, PixelDensity resolution, Mount mount) {
        float dpi = (float)(double)resolution;
        using SKDynamicMemoryWStream stream = new();
        using (SKDocument document = SKDocument.CreatePdf(stream, dpi))
        using (SKRegion damage = new(SKRectI.Ceiling(SKRect.Create(size))))
        using (PaintCache cache = new(new SKImageInfo(damage.Bounds.Width, damage.Bounds.Height, SKColorType.Rgba8888, SKAlphaType.Premul, SKColorSpace.CreateSrgb()),
                   new SKSurfaceProperties(SKSurfacePropsFlags.UseDeviceIndependentFonts, SKPixelGeometry.Unknown), dpi / SKDocument.DefaultRasterDpi)) {
            Painter.Paint(document.BeginPage(size.Width, size.Height), cache, mount, damage, Duration.Zero);
            document.EndPage();
            document.Close();
        }
        return stream.DetachAsData();
    }
}
