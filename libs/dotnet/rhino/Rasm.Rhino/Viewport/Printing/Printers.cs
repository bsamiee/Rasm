using Rasm.Drafting;
using Rhino;
using Rhino.FileIO;
using Rhino.Runtime;

namespace Rasm.Rhino.Viewport.Printing;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PrinterMargins(Length Left, Length Top, Length Right, Length Bottom);

public sealed record PrinterForm(string Name, Length Width, Length Height, HashMap<SheetOrientation, PrinterMargins> Margins);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Printers {
    // --- [PRINTERS]
    public static IO<Seq<PrinterForm>> Forms(string printer) =>
        IO.lift(() => toSeq(HostUtils.GetPrinterFormNames(printer))
            .Choose(form => Conversions.Found(HostUtils.GetPrinterFormSize(printer, form, out double width, out double height), form)
                .Map(sized => new PrinterForm(sized, Length.FromMillimeters(width), Length.FromMillimeters(height), toHashMap(toSeq(SheetOrientation.Items).Choose(orientation => Conversions.Found(
                    HostUtils.GetPrinterFormMargins(printer, sized, orientation.Map(portrait: true, landscape: false), out double left, out double top, out double right, out double bottom),
                    (orientation, new PrinterMargins(Length.FromMillimeters(left), Length.FromMillimeters(top), Length.FromMillimeters(right), Length.FromMillimeters(bottom)))))))))
            .Strict());

    public static IO<(double Horizontal, double Vertical)> Resolution(string printer) =>
        IO.lift(() => (HostUtils.GetPrinterDPI(printer, horizontal: true), HostUtils.GetPrinterDPI(printer, horizontal: false)));

    // --- [CUSTOM_PAGES]
    public static IO<T> WithCustomPages<T>(Seq<PrintedPageDefinition> pages, IO<T> write) =>
        IO.lift(FilePdf.GetCustomPages).Bind(prior =>
            IO.lift(() => FilePdf.SetCustomPages([.. prior, .. pages])) >> write.Finally(IO.lift(() => FilePdf.SetCustomPages(prior))));
}
