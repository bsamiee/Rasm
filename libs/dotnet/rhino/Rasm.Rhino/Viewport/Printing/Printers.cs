using System.Collections.Generic;
using Rasm.Drafting;
using Rhino;
using Rhino.FileIO;
using Rhino.Runtime;
using Rasm.Rhino.Document.Units;

namespace Rasm.Rhino.Viewport.Printing;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PrinterMargins(Length Left, Length Top, Length Right, Length Bottom);

public sealed record PrinterForm(string Name, Length Width, Length Height, HashMap<SheetOrientation, PrinterMargins> Margins);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Printers {
    // --- [PRINTERS]
    public static IO<Seq<PrinterForm>> Forms(string printer) =>
        IO.lift(() => Conversions.Rows(HostUtils.GetPrinterFormNames(printer)).Choose(form =>
                from size in Conversions.Present(HostUtils.GetPrinterFormSize(printer, form, out double width, out double height), (Width: width, Height: height))
                select new PrinterForm(form, Length.FromMillimeters(Quantities.Exact(size.Width)), Length.FromMillimeters(Quantities.Exact(size.Height)), toHashMap(toSeq(SheetOrientation.Items).Choose(orientation =>
                    Conversions.Present(
                        HostUtils.GetPrinterFormMargins(printer, form, orientation.Map(portrait: true, landscape: false), out double left, out double top, out double right, out double bottom),
                        (orientation, new PrinterMargins(Length.FromMillimeters(Quantities.Exact(left)), Length.FromMillimeters(Quantities.Exact(top)), Length.FromMillimeters(Quantities.Exact(right)), Length.FromMillimeters(Quantities.Exact(bottom)))))))))
            .Strict());

    public static IO<(double Horizontal, double Vertical)> Resolution(string printer) =>
        IO.lift(() => (HostUtils.GetPrinterDPI(printer, horizontal: true), HostUtils.GetPrinterDPI(printer, horizontal: false)));

    // --- [CUSTOM_PAGES]
    public static IO<T> WithCustomPages<T>(Seq<PrintedPageDefinition> pages, IO<T> write) =>
        (from prior in IO.lift(FilePdf.GetCustomPages)
         from added in Products.Changed<IEnumerable<PrintedPageDefinition>>(FilePdf.GetCustomPages, FilePdf.SetCustomPages, [.. prior, .. pages])
         from written in write
         select written).Bracket();
}
