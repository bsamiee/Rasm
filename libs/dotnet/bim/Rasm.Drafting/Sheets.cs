using System.Globalization;
using UnitsNet;

namespace Rasm.Drafting;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record DraftingConvention(
    Length Binding, Length Border, bool RowsFromTop, bool ColumnsFromRight, bool SmallestTopRight, Option<(Length Reach, Length TitleBlock)> Centering,
    LetteringStandard Lettering, RevisionAlphabet Revisions, Func<Length, Length, Length, Seq<Length>> Divisions) {
    public static readonly DraftingConvention Iso = new(Binding: Length.FromMillimeters(20), Border: Length.FromMillimeters(10),
        RowsFromTop: true, ColumnsFromRight: false, SmallestTopRight: true, Centering: Some((Reach: Length.FromMillimeters(10), TitleBlock: Length.FromMillimeters(180))),
        LetteringStandard.Iso3098, RevisionAlphabet.Iso,
        static (side, _, _) =>
            from pitch in Seq(Length.FromMillimeters(50))
            from half in Seq((int)QuantityValue.Round(side / pitch / 2, MidpointRounding.ToEven))
            from k in toSeq(Range(1 - half, (2 * half) - 1))
            select (side / 2) + (pitch * k));
    public static readonly DraftingConvention Asme = new(Binding: Length.FromInches(0.5m), Border: Length.FromInches(0.5m),
        RowsFromTop: false, ColumnsFromRight: true, SmallestTopRight: false, Centering: None,
        LetteringStandard.AsmeY142, RevisionAlphabet.Asme,
        static (side, _, _) =>
            from count in Seq(2 * (int)QuantityValue.Round(side / Length.FromInches(5.5m) / 2, MidpointRounding.ToPositiveInfinity))
            from k in toSeq(Range(1, count - 1))
            select side * k / count);
    public static readonly DraftingConvention Ncs = new(Binding: Length.FromInches(1.5m), Border: Length.FromInches(0.75m),
        RowsFromTop: false, ColumnsFromRight: false, SmallestTopRight: false, Centering: None,
        LetteringStandard.AsmeY142, RevisionAlphabet.Asme,
        static (_, first, last) =>
            from pitch in Seq(Length.FromInches(1.5m))
            from k in toSeq(Range(1, (int)QuantityValue.Round((last - first) / pitch, MidpointRounding.ToPositiveInfinity) - 1))
            select first + (pitch * k));

    public (int Mask, int Offset, int Extension, int Landing) Dimensioning { get; init; } = (Mask: 2, Offset: 8, Extension: 8, Landing: 20);
}

public sealed record SheetSize(string Name, DraftingConvention Convention, (Length Short, Length Long) Sides) {
    public static readonly SheetSize A0 = new("A0", DraftingConvention.Iso, (Length.FromMillimeters(841), Length.FromMillimeters(1189)));
    public static readonly SheetSize A1 = new("A1", DraftingConvention.Iso, (Length.FromMillimeters(594), Length.FromMillimeters(841)));
    public static readonly SheetSize A2 = new("A2", DraftingConvention.Iso, (Length.FromMillimeters(420), Length.FromMillimeters(594)));
    public static readonly SheetSize A3 = new("A3", DraftingConvention.Iso, (Length.FromMillimeters(297), Length.FromMillimeters(420)));
    public static readonly SheetSize A4 = new("A4", DraftingConvention.Iso, (Length.FromMillimeters(210), Length.FromMillimeters(297)));
    public static readonly SheetSize AnsiA = new("ANSI A", DraftingConvention.Asme, (Length.FromInches(8.5m), Length.FromInches(11)));
    public static readonly SheetSize AnsiB = new("ANSI B", DraftingConvention.Asme, (Length.FromInches(11), Length.FromInches(17)));
    public static readonly SheetSize AnsiC = new("ANSI C", DraftingConvention.Asme, (Length.FromInches(17), Length.FromInches(22)));
    public static readonly SheetSize AnsiD = new("ANSI D", DraftingConvention.Asme, (Length.FromInches(22), Length.FromInches(34)));
    public static readonly SheetSize AnsiE = new("ANSI E", DraftingConvention.Asme, (Length.FromInches(34), Length.FromInches(44)));
    public static readonly SheetSize ArchA = new("ARCH A", DraftingConvention.Ncs, (Length.FromInches(9), Length.FromInches(12)));
    public static readonly SheetSize ArchB = new("ARCH B", DraftingConvention.Ncs, (Length.FromInches(12), Length.FromInches(18)));
    public static readonly SheetSize ArchC = new("ARCH C", DraftingConvention.Ncs, (Length.FromInches(18), Length.FromInches(24)));
    public static readonly SheetSize ArchD = new("ARCH D", DraftingConvention.Ncs, (Length.FromInches(24), Length.FromInches(36)));
    public static readonly SheetSize ArchE = new("ARCH E", DraftingConvention.Ncs, (Length.FromInches(36), Length.FromInches(48)));
    public static readonly SheetSize ArchF = new("ARCH F", DraftingConvention.Ncs, (Length.FromInches(30), Length.FromInches(42)));

    public static Seq<SheetSize> Standard { get; } = [A0, A1, A2, A3, A4, AnsiA, AnsiB, AnsiC, AnsiD, AnsiE, ArchA, ArchB, ArchC, ArchD, ArchE, ArchF];
}

[SmartEnum]
public sealed partial class SheetOrientation {
    public static readonly SheetOrientation Portrait = new(static size => size.Sides);
    public static readonly SheetOrientation Landscape = new(static size => (size.Sides.Long, size.Sides.Short));

    [UseDelegateFromConstructor]
    public partial (Length Width, Length Height) Extent(SheetSize size);
}

public sealed record PaperRectangle(Length Left, Length Bottom, Length Right, Length Top);

public sealed record ZoneGrid(Seq<Length> Columns, Seq<Length> Rows, DraftingConvention Convention, bool AllEdges) {
    private const string Letters = "ABCDEFGHJKLMNPQRSTUVWXYZ";

    public string Column(int index) =>
        (Convention.ColumnsFromRight ? Columns.Count + 1 - index : index + 1).ToString(CultureInfo.InvariantCulture);
    public string Row(int index) => Letter(Convention.RowsFromTop ? Rows.Count - index : index);

    private static string Letter(int row) => new(Letters[row % Letters.Length], (row / Letters.Length) + 1);
}

public sealed record Sheet(SheetSize Size, SheetOrientation Orientation) {
    public (Length Width, Length Height) Extent => Orientation.Extent(Size);
    public PaperRectangle Field => new(Size.Convention.Binding, Size.Convention.Border, Extent.Width - Size.Convention.Border, Extent.Height - Size.Convention.Border);
    public ZoneGrid Zones =>
        new(
            Size.Convention.Divisions(Extent.Width, Field.Left, Field.Right),
            Size.Convention.Divisions(Extent.Height, Field.Bottom, Field.Top),
            Size.Convention,
            !Size.Convention.SmallestTopRight
                || SheetSize.Standard.Exists(other => other.Convention == Size.Convention && other.Sides.Short < Size.Sides.Short));

    public Option<string> Locate(Length x, Length y) =>
        (Field, Zones) switch {
            var (drawing, zones) when x >= drawing.Left && x <= drawing.Right && y >= drawing.Bottom && y <= drawing.Top =>
                Some(zones.Row(zones.Rows.Filter(division => division <= y).Count) + zones.Column(zones.Columns.Filter(division => division <= x).Count)),
            _ => None,
        };

    public static Option<Sheet> Match(Length width, Length height, Length tolerance) =>
        (from size in SheetSize.Standard
         from orientation in toSeq(SheetOrientation.Items)
         select new Sheet(size, orientation))
            .Find(sheet => sheet.Extent.Width.Equals(width, tolerance) && sheet.Extent.Height.Equals(height, tolerance));
}
