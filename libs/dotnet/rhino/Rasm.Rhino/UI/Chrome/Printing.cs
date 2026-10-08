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
        use(static () => new PrintDialog()).Bind(dialog => IO.lift(() => dialog.ShowDialog(RhinoEtoApp.MainWindowForDocument(doc), document))).Bracket());

    public static readonly PrintView Preview = new(static (document, doc) =>
        use(() => new PrintPreviewDialog(document)).Bind(dialog => IO.lift(() => dialog.ShowDialog(RhinoEtoApp.MainWindowForDocument(doc)))).Bracket());

    [UseDelegateFromConstructor]
    public partial IO<DialogResult> ShowDialog(PrintDocument document, RhinoDoc doc);

    public IO<Unit> Show(RhinoDoc doc, PrintJob job) =>
        from attended in HostDialogs.Attended
        from result in use(() => new PrintDocument {
            Name = job.Name.Local,
            PageCount = job.Pages.Count(),
            PrintSettings = { Orientation = job.Orientation.Map(portrait: PageOrientation.Portrait, landscape: PageOrientation.Landscape) },
        })
            .Bind(document => use(Subscriptions.Attach(
                    h => document.PrintPage += h,
                    h => document.PrintPage -= h,
                    Callbacks.Handler<PrintPageEventArgs>(
                        page => job.Pages.At(page.CurrentPage).Match(
                            Some: control => control.Print(page.Graphics, page.PageSize, (float)(job.Resolution / Screen.PrimaryScreen.DPI)),
                            None: static () => IO.pure(unit)),
                        new CallbackSite(job.Pages.Head.Sink, typeof(PrintDocument), nameof(PrintDocument.PrintPage)))))
                .Bind(_ => ShowDialog(document, doc))
                .Bracket())
            .Bracket()
        from accepted in unless(result == DialogResult.Ok, IO.fail<Unit>(Errors.Cancelled)).As()
        select accepted;
}
