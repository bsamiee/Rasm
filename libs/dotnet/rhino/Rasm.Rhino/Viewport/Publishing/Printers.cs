using Rasm.Drafting;
using Rhino;
using Rhino.FileIO;
using Rhino.Runtime;

namespace Rasm.Rhino.Viewport.Publishing;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PrinterMargins(double Left, double Top, double Right, double Bottom);

public sealed record PrinterForm(string Name, double Width, double Height, Option<PrinterMargins> Portrait, Option<PrinterMargins> Landscape) {
    public Option<PrinterMargins> Margins(SheetOrientation orientation) => orientation.Map(portrait: Portrait, landscape: Landscape);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct Dpi : System.Numerics.IMinMaxValue<Dpi> {
    public static Dpi MinValue { get; } = new(double.BitIncrement(0d));
    public static Dpi MaxValue { get; } = new(double.MaxValue);
    public static Dpi PrintDefault { get; } = new(300d);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Printers {
    // --- [PRINTERS]
    public static IO<Seq<PrinterForm>> Forms(string printer) =>
        IO.lift(() => toSeq(HostUtils.GetPrinterFormNames(printer))
            .Choose(form => Callbacks.Found(HostUtils.GetPrinterFormSize(printer, form, out double width, out double height), (Width: width, Height: height))
                .Map(size => new PrinterForm(form, size.Width, size.Height, Margins(printer, form, portrait: true), Margins(printer, form, portrait: false))))
            .Strict());

    public static IO<Dpi> Resolution(string printer) =>
        IO.lift(() => Conversions.Validated<Dpi, double, InvalidRhinoValue>(HostUtils.GetPrinterDPI(printer, horizontal: true))
            .MapFail(static _ => new Refused(nameof(HostUtils.GetPrinterDPI))));

    private static Option<PrinterMargins> Margins(string printer, string form, bool portrait) =>
        Callbacks.Found(
            HostUtils.GetPrinterFormMargins(printer, form, portrait, out double left, out double top, out double right, out double bottom),
            new PrinterMargins(left, top, right, bottom));

    // --- [CUSTOM_PAGES]
    public static IO<T> WithCustomPages<T>(Seq<SheetSize> sheets, IO<T> write) =>
        use(FilePdf.GetCustomPages, FilePdf.SetCustomPages)
            .Bind(prior => IO.lift(() => FilePdf.SetCustomPages([
                .. prior,
                .. sheets.Map(static sheet => new PrintedPageDefinition {
                    Description = sheet.Designation,
                    Width = sheet.Sides.Short.Millimeters.ToDouble(),
                    Height = sheet.Sides.Long.Millimeters.ToDouble(),
                    Units = UnitSystem.Millimeters,
                }),
            ])))
            .Bind(_ => write)
            .Bracket();
}
