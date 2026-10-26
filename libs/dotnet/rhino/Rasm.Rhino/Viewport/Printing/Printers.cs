using Rasm.Drafting;
using Rasm.Imaging.Files;
using Rhino;
using Rhino.FileIO;
using Rhino.Runtime;
using UnitsNet;

namespace Rasm.Rhino.Viewport.Printing;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PrinterMargins(Length Left, Length Top, Length Right, Length Bottom);

public sealed record PrinterForm(string Name, Length Width, Length Height, HashMap<SheetOrientation, PrinterMargins> Margins);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Printers {
    // --- [PRINTERS]
    public static IO<Seq<PrinterForm>> Forms(string printer) =>
        IO.lift(() => toSeq(HostUtils.GetPrinterFormNames(printer))
            .Choose(form => Callbacks.Found(HostUtils.GetPrinterFormSize(printer, form, out double width, out double height), form)
                .Map(sized => new PrinterForm(sized, Length.FromMillimeters(width), Length.FromMillimeters(height), toHashMap(toSeq(SheetOrientation.Items).Choose(orientation => Callbacks.Found(
                    HostUtils.GetPrinterFormMargins(printer, sized, orientation.Map(portrait: true, landscape: false), out double left, out double top, out double right, out double bottom),
                    (orientation, new PrinterMargins(Length.FromMillimeters(left), Length.FromMillimeters(top), Length.FromMillimeters(right), Length.FromMillimeters(bottom)))))))))
            .Strict());

    public static IO<(PixelDensity Horizontal, PixelDensity Vertical)> Resolution(string printer) =>
        IO.lift(() => (Density(printer, horizontal: true), Density(printer, horizontal: false)).Apply(static (across, down) => (across, down)).As());

    private static Fin<PixelDensity> Density(string printer, bool horizontal) =>
        PixelDensity.Validated(HostUtils.GetPrinterDPI(printer, horizontal)).MapFail(static _ => new Refused(nameof(HostUtils.GetPrinterDPI), Refused.Declined));

    // --- [CUSTOM_PAGES]
    public static IO<T> WithCustomPages<T>(Seq<PrintedPageDefinition> pages, IO<T> write) =>
        DisposalOps.Within(FilePdf.GetCustomPages,
            prior => IO.lift(() => FilePdf.SetCustomPages([.. prior, .. pages])).Bind(_ => write),
            static prior => IO.lift(() => FilePdf.SetCustomPages(prior)));
}
