using Eto.Forms;
using Rasm.Drafting;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Components;
using Rasm.Rhino.UI.Views;
using Rasm.Rhino.Viewport.Publishing;
using Rhino;
using Rhino.UI;

namespace Rasm.Rhino.UI.Chrome;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PrintJob(LocalizeStringPair Name, SheetOrientation Orientation, Dpi Resolution, IterableNE<ComponentControl> Pages);

[SmartEnum]
public sealed partial class PrintView {
    public static readonly PrintView Print = new(static (document, doc) =>
        DisposalOps.Within(static () => new PrintDialog(), dialog => IO.lift(() => dialog.ShowDialog(RhinoEtoApp.MainWindowForDocument(doc), document)), static dialog => IO.lift(dialog.Dispose)));

    public static readonly PrintView Preview = new(static (document, doc) =>
        DisposalOps.Within(() => new PrintPreviewDialog(document), dialog => IO.lift(() => dialog.ShowDialog(RhinoEtoApp.MainWindowForDocument(doc))), static dialog => IO.lift(dialog.Dispose)));

    [UseDelegateFromConstructor]
    public partial IO<DialogResult> ShowDialog(PrintDocument document, RhinoDoc doc);

    public IO<Unit> Show(RhinoDoc doc, PrintJob job) =>
        from attended in HostDialogs.Attended
        let pages = job.Pages.ToSeq()
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
                    Callbacks.Handler<PrintPageEventArgs>(
                        page => pages[page.CurrentPage].Print(page.Graphics, page.PageSize, (float)(job.Resolution / Screen.PrimaryScreen.DPI)),
                        new CallbackSite(job.Pages.Head.Sink, typeof(PrintDocument), nameof(PrintDocument.PrintPage))), retain)
                from shown in ShowDialog(document, doc)
                select shown, DisposalOps.Release), static document => IO.lift(document.Dispose))
        from accepted in unless(result == DialogResult.Ok, IO.fail<Unit>(Errors.Cancelled)).As()
        select accepted;
}
