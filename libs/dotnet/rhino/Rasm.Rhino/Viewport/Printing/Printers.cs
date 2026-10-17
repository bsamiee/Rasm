using Rasm.Drafting;
using Rasm.Rhino.Document.Units;
using Rhino;
using Rhino.FileIO;
using Rhino.Runtime;
using UnitsNet;

namespace Rasm.Rhino.Viewport.Printing;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PrinterMargins(double Left, double Top, double Right, double Bottom);

public sealed record PrinterForm(string Name, double Width, double Height, HashMap<SheetOrientation, PrinterMargins> Margins);

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct Dpi : System.Numerics.IMinMaxValue<Dpi> {
    public static Dpi MinValue { get; } = new(double.BitIncrement(0d));
    public static Dpi MaxValue { get; } = new(double.MaxValue);
    public static Dpi PrintDefault { get; } = new(300d);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Printers {
    // --- [PRINTERS]
    public static IO<Seq<PrinterForm>> Forms(string printer) =>
        IO.lift(() => toSeq(HostUtils.GetPrinterFormNames(printer))
            .Choose(form => Callbacks.Found(HostUtils.GetPrinterFormSize(printer, form, out double width, out double height), form)
                .Map(sized => new PrinterForm(sized, width, height, toHashMap(toSeq(SheetOrientation.Items).Choose(orientation => Callbacks.Found(
                    HostUtils.GetPrinterFormMargins(printer, sized, orientation.Map(portrait: true, landscape: false), out double left, out double top, out double right, out double bottom),
                    (orientation, new PrinterMargins(left, top, right, bottom))))))))
            .Strict());

    public static IO<Dpi> Resolution(string printer) =>
        IO.lift(() => Dpi.Validated(HostUtils.GetPrinterDPI(printer, horizontal: true)).MapFail(static _ => new Refused(nameof(HostUtils.GetPrinterDPI), Refused.Declined)));

    // --- [CUSTOM_PAGES]
    public static IO<T> WithCustomPages<T>(Seq<SheetSize> sheets, IO<T> write) =>
        from pages in IO.lift(sheets.Traverse(static sheet => Quantities.System(sheet.Sides.Short.Unit)
            .Map(system => new PrintedPageDefinition {
                Description = sheet.Key, Width = sheet.Sides.Short.Value.ToDouble(), Height = sheet.Sides.Long.As(sheet.Sides.Short.Unit).ToDouble(), Units = system,
            })
            .ToFin(Error.New($"sheet {sheet} has a unit no Rhino unit system holds"))).As())
        from written in DisposalOps.Within(FilePdf.GetCustomPages,
            prior => IO.lift(() => FilePdf.SetCustomPages([.. prior, .. pages])).Bind(_ => write),
            static prior => IO.lift(() => FilePdf.SetCustomPages(prior)))
        select written;
}
