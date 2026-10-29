using System.Globalization;
using UnitsNet;

namespace Rasm.Drafting;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class DraftingConvention {
    public static readonly DraftingConvention Iso = new(binding: Length.FromMillimeters(20), border: Length.FromMillimeters(10), rowsFromTop: true, columnsFromRight: false, smallestTopRight: true,
        centering: Some((Reach: Length.FromMillimeters(10), TitleBlock: Length.FromMillimeters(180))), lettering: LetteringStandard.Iso3098, terminator: Terminator.ClosedArrow, revisions: RevisionAlphabet.Iso,
        divisions: static (side, _, _) =>
            from pitch in Seq(Length.FromMillimeters(50))
            from half in Seq((int)QuantityValue.Round(side / pitch / 2, MidpointRounding.ToEven))
            from k in toSeq(Range(1 - half, (2 * half) - 1))
            select (side / 2) + (pitch * k));
    public static readonly DraftingConvention Asme = new(binding: Length.FromInches(0.5m), border: Length.FromInches(0.5m), rowsFromTop: false, columnsFromRight: true, smallestTopRight: false,
        centering: None, lettering: LetteringStandard.AsmeY142, terminator: Terminator.ClosedArrow, revisions: RevisionAlphabet.Asme,
        divisions: static (side, _, _) =>
            from count in Seq(2 * (int)QuantityValue.Round(side / Length.FromInches(5.5m) / 2, MidpointRounding.ToPositiveInfinity))
            from k in toSeq(Range(1, count - 1))
            select side * k / count);
    public static readonly DraftingConvention Ncs = new(binding: Length.FromInches(1.5m), border: Length.FromInches(0.75m), rowsFromTop: false, columnsFromRight: false, smallestTopRight: false,
        centering: None, lettering: LetteringStandard.AsmeY142, terminator: Terminator.Oblique, revisions: RevisionAlphabet.Asme,
        divisions: static (_, first, last) =>
            from pitch in Seq(Length.FromInches(1.5m))
            from k in toSeq(Range(1, (int)QuantityValue.Round((last - first) / pitch, MidpointRounding.ToPositiveInfinity) - 1))
            select first + (pitch * k));

    public Length Binding { get; }
    public Length Border { get; }
    public bool RowsFromTop { get; }
    public bool ColumnsFromRight { get; }
    public bool SmallestTopRight { get; }
    public Option<(Length Reach, Length TitleBlock)> Centering { get; }
    public LetteringStandard Lettering { get; }
    public Terminator Terminator { get; }
    public RevisionAlphabet Revisions { get; }

    [UseDelegateFromConstructor]
    public partial Seq<Length> Divisions(Length side, Length first, Length last);
}

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[ValidationError<DraftingFault>]
public sealed partial class SheetSize {
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
    public static readonly SheetSize ArchE1 = new("ARCH E1", DraftingConvention.Ncs, (Length.FromInches(30), Length.FromInches(42)));
    public static readonly SheetSize ArchE = new("ARCH E", DraftingConvention.Ncs, (Length.FromInches(36), Length.FromInches(48)));

    public DraftingConvention Convention { get; }
    public (Length Short, Length Long) Sides { get; }
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
    public string Column(int index) => (Convention.ColumnsFromRight ? Columns.Count + 1 - index : index + 1).ToString(CultureInfo.InvariantCulture);
    public string Row(int index) => Letter(RevisionAlphabet.Iso.Symbols, Convention.RowsFromTop ? Rows.Count - index : index);

    private static string Letter(string letters, int row) => new(letters[row % letters.Length], (row / letters.Length) + 1);
}

[Union]
public abstract partial record FrameMark {
    public sealed record Stroke(LineWeight Weight, Seq<(Length X, Length Y)> Points) : FrameMark;

    public sealed record Label(string Text, (Length X, Length Y) Center, Length Height) : FrameMark;
}

public sealed record Sheet(SheetSize Size, SheetOrientation Orientation) {
    public (Length Width, Length Height) Extent => Orientation.Extent(Size);
    public PaperRectangle Field => Extent switch { var (width, height) => new(Size.Convention.Binding, Size.Convention.Border, width - Size.Convention.Border, height - Size.Convention.Border) };
    public ZoneGrid Zones =>
        (Extent, Field) switch {
            var ((width, height), drawing) => new(Size.Convention.Divisions(width, drawing.Left, drawing.Right), Size.Convention.Divisions(height, drawing.Bottom, drawing.Top), Size.Convention,
                !Size.Convention.SmallestTopRight || toSeq(SheetSize.Items).Exists(other => other.Convention == Size.Convention && other.Sides.Short < Size.Sides.Short)),
        };

    public Seq<FrameMark> Frame {
        get {
            ((Length width, Length height), PaperRectangle paper, ZoneGrid zones, Length letter) = (Extent, Field, Zones, Size.Convention.Lettering.ZoneHeight);
            Option<Length> reach = Size.Convention.Centering.Map(static centering => centering.Reach);
            Option<Length> blocked = Size.Convention.Centering.Map(centering => width / 2 < paper.Right - centering.TitleBlock ? centering.Reach : Length.Zero);
            return Seq<FrameMark>(new FrameMark.Stroke(LineWeight.ExtraWide, [(paper.Left, paper.Bottom), (paper.Right, paper.Bottom), (paper.Right, paper.Top), (paper.Left, paper.Top), (paper.Left, paper.Bottom)]))
                + Edges(zones.Columns, (paper.Left, paper.Right), width, letter, zones.Column, static (along, across) => (along, across),
                    [(paper.Top, height, true, reach), (paper.Bottom, Length.Zero, zones.AllEdges, blocked)])
                + Edges(zones.Rows, (paper.Bottom, paper.Top), height, letter, zones.Row, static (along, across) => (across, along),
                    [(paper.Right, width, true, reach), (paper.Left, Length.Zero, zones.AllEdges, reach)]);
        }
    }

    public Option<string> Locate(Length x, Length y) =>
        Some(Field).Filter(drawing => x >= drawing.Left && x <= drawing.Right && y >= drawing.Bottom && y <= drawing.Top)
            .Map(_ => Zones switch { var zones => zones.Row(zones.Rows.Filter(division => division <= y).Count) + zones.Column(zones.Columns.Filter(division => division <= x).Count) });

    public static Option<Sheet> Match(Length width, Length height, Length tolerance) =>
        (from size in toSeq(SheetSize.Items) from orientation in toSeq(SheetOrientation.Items) select new Sheet(size, orientation))
            .Find(sheet => sheet.Extent.Width.Equals(width, tolerance) && sheet.Extent.Height.Equals(height, tolerance));

    private static Seq<FrameMark> Edges(
        Seq<Length> divisions, (Length Near, Length Far) span, Length edge, Length letter, Func<int, string> label, Func<Length, Length, (Length X, Length Y)> at,
        Seq<(Length Inner, Length Outer, bool Zoned, Option<Length> Reach)> bands) =>
        (from band in bands
         where band.Zoned
         from mark in divisions.Map(division => (FrameMark)new FrameMark.Stroke(LineWeight.Wide, [at(division, band.Inner), at(division, band.Outer)]))
             + span.Near.Cons(divisions).Zip(divisions.Add(span.Far)).Map((zone, index) =>
                 (FrameMark)new FrameMark.Label(label(index), at((zone.First + zone.Second) / 2, (band.Inner + band.Outer) / 2), letter))
         select mark)
        + (from band in bands
           from inward in band.Reach.ToSeq()
           select (FrameMark)new FrameMark.Stroke(LineWeight.ExtraWide, [at(edge / 2, band.Outer), at(edge / 2, band.Inner < band.Outer ? band.Inner - inward : band.Inner + inward)]));
}
