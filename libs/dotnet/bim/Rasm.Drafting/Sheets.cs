using System.Globalization;
using UnitsNet;

namespace Rasm.Drafting;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class DraftingConvention {
    public static readonly DraftingConvention Iso = new(binding: Length.FromMillimeters(20), border: Length.FromMillimeters(10),
        rowsFromTop: true, columnsFromRight: false, smallestTopRight: true, centering: Some(Length.FromMillimeters(10)),
        lettering: LetteringStandard.Iso3098, revisions: RevisionAlphabet.Iso,
        divisions: static (side, _, _) =>
            from pitch in Seq(Length.FromMillimeters(50))
            from half in Seq((int)QuantityValue.Round(side / pitch / 2, MidpointRounding.ToEven))
            from k in toSeq(Range(1 - half, (2 * half) - 1))
            select (side / 2) + (pitch * k));
    public static readonly DraftingConvention Asme = new(binding: Length.FromInches(0.5m), border: Length.FromInches(0.5m),
        rowsFromTop: false, columnsFromRight: true, smallestTopRight: false, centering: None,
        lettering: LetteringStandard.AsmeY142, revisions: RevisionAlphabet.Asme,
        divisions: static (side, _, _) =>
            from count in Seq(2 * (int)QuantityValue.Round(side / Length.FromInches(5.5m) / 2, MidpointRounding.ToPositiveInfinity))
            from k in toSeq(Range(1, count - 1))
            select side * k / count);
    public static readonly DraftingConvention Ncs = new(binding: Length.FromInches(1.5m), border: Length.FromInches(0.75m),
        rowsFromTop: false, columnsFromRight: false, smallestTopRight: false, centering: None,
        lettering: LetteringStandard.AsmeY142, revisions: RevisionAlphabet.Asme,
        divisions: static (_, first, last) =>
            from pitch in Seq(Length.FromInches(1.5m))
            from k in toSeq(Range(1, (int)QuantityValue.Round((last - first) / pitch, MidpointRounding.ToPositiveInfinity) - 1))
            select first + (pitch * k));

    public Length Binding { get; }
    public Length Border { get; }
    public bool RowsFromTop { get; }
    public bool ColumnsFromRight { get; }
    public bool SmallestTopRight { get; }
    public Option<Length> Centering { get; }
    public LetteringStandard Lettering { get; }
    public RevisionAlphabet Revisions { get; }

    [UseDelegateFromConstructor]
    public partial Seq<Length> Divisions(Length side, Length first, Length last);
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class SheetStep {
    public static readonly SheetStep Halving = new(
        static (seed, step) => Range(0, step).Fold(seed,
            static (sides, _) => (Length.FromMillimeters(QuantityValue.Round(sides.Long.Millimeters / 2, MidpointRounding.ToZero)), sides.Short)),
        static step => step.ToString(CultureInfo.InvariantCulture));
    public static readonly SheetStep Doubling = new(
        static (seed, step) => Range(0, step).Fold(seed, static (sides, _) => (sides.Long, sides.Short * 2)),
        static step => ((char)('A' + step)).ToString(CultureInfo.InvariantCulture));
    public static readonly SheetStep Seed = new(static (seed, _) => seed, static _ => "");

    [UseDelegateFromConstructor]
    public partial (Length Short, Length Long) At((Length Short, Length Long) seed, int step);

    [UseDelegateFromConstructor]
    public partial string Suffix(int step);
}

[SmartEnum]
public sealed partial class SheetSeries {
    public static readonly SheetSeries IsoA = new(prefix: "A", convention: DraftingConvention.Iso,
        seed: (Length.FromMillimeters(841), Length.FromMillimeters(1189)), step: SheetStep.Halving);
    public static readonly SheetSeries IsoB = new(prefix: "B", convention: DraftingConvention.Iso,
        seed: (Length.FromMillimeters(1000), Length.FromMillimeters(1414)), step: SheetStep.Halving);
    public static readonly SheetSeries Ansi = new(prefix: "ANSI ", convention: DraftingConvention.Asme,
        seed: (Length.FromInches(8.5m), Length.FromInches(11)), step: SheetStep.Doubling);
    public static readonly SheetSeries Arch = new(prefix: "ARCH ", convention: DraftingConvention.Ncs,
        seed: (Length.FromInches(9), Length.FromInches(12)), step: SheetStep.Doubling);
    public static readonly SheetSeries ArchE1 = new(prefix: "ARCH E1", convention: DraftingConvention.Ncs,
        seed: (Length.FromInches(30), Length.FromInches(42)), step: SheetStep.Seed);

    public string Prefix { get; }
    public DraftingConvention Convention { get; }
    public (Length Short, Length Long) Seed { get; }
    public SheetStep Step { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidDrafting>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SheetSize {
    public static readonly SheetSize A0 = new("a0", SheetSeries.IsoA, step: 0);
    public static readonly SheetSize A1 = new("a1", SheetSeries.IsoA, step: 1);
    public static readonly SheetSize A2 = new("a2", SheetSeries.IsoA, step: 2);
    public static readonly SheetSize A3 = new("a3", SheetSeries.IsoA, step: 3);
    public static readonly SheetSize A4 = new("a4", SheetSeries.IsoA, step: 4);
    public static readonly SheetSize B0 = new("b0", SheetSeries.IsoB, step: 0);
    public static readonly SheetSize B1 = new("b1", SheetSeries.IsoB, step: 1);
    public static readonly SheetSize B2 = new("b2", SheetSeries.IsoB, step: 2);
    public static readonly SheetSize B3 = new("b3", SheetSeries.IsoB, step: 3);
    public static readonly SheetSize B4 = new("b4", SheetSeries.IsoB, step: 4);
    public static readonly SheetSize AnsiA = new("ansi-a", SheetSeries.Ansi, step: 0);
    public static readonly SheetSize AnsiB = new("ansi-b", SheetSeries.Ansi, step: 1);
    public static readonly SheetSize AnsiC = new("ansi-c", SheetSeries.Ansi, step: 2);
    public static readonly SheetSize AnsiD = new("ansi-d", SheetSeries.Ansi, step: 3);
    public static readonly SheetSize AnsiE = new("ansi-e", SheetSeries.Ansi, step: 4);
    public static readonly SheetSize ArchA = new("arch-a", SheetSeries.Arch, step: 0);
    public static readonly SheetSize ArchB = new("arch-b", SheetSeries.Arch, step: 1);
    public static readonly SheetSize ArchC = new("arch-c", SheetSeries.Arch, step: 2);
    public static readonly SheetSize ArchD = new("arch-d", SheetSeries.Arch, step: 3);
    public static readonly SheetSize ArchE = new("arch-e", SheetSeries.Arch, step: 4);
    public static readonly SheetSize ArchE1 = new("arch-e1", SheetSeries.ArchE1, step: 0);

    public SheetSeries Series { get; }
    internal int Step { get; }

    public (Length Short, Length Long) Sides => Series.Step.At(Series.Seed, Step);
    public string Designation => Series.Prefix + Series.Step.Suffix(Step);
}

[SmartEnum]
public sealed partial class SheetOrientation {
    public static readonly SheetOrientation Portrait = new(static size => size.Sides);
    public static readonly SheetOrientation Landscape = new(static size => (size.Sides.Long, size.Sides.Short));

    [UseDelegateFromConstructor]
    public partial (Length Width, Length Height) Extent(SheetSize size);
}

public sealed record PaperRectangle {
    internal PaperRectangle(Length left, Length bottom, Length right, Length top) => (Left, Bottom, Right, Top) = (left, bottom, right, top);

    public Length Left { get; }
    public Length Bottom { get; }
    public Length Right { get; }
    public Length Top { get; }
}

public sealed record ZoneGrid {
    private const string Letters = "ABCDEFGHJKLMNPQRSTUVWXYZ";

    internal ZoneGrid(Seq<Length> columns, Seq<Length> rows, DraftingConvention convention, bool allEdges) =>
        (Columns, Rows, Convention, AllEdges) = (columns, rows, convention, allEdges);

    public Seq<Length> Columns { get; }
    public Seq<Length> Rows { get; }
    public bool AllEdges { get; }
    internal DraftingConvention Convention { get; }

    public string Column(int index) =>
        (Convention.ColumnsFromRight ? Columns.Count + 1 - index : index + 1).ToString(CultureInfo.InvariantCulture);
    public string Row(int index) =>
        (Convention.RowsFromTop ? Rows.Count - index : index) switch {
            var row => new(Letters[row % Letters.Length], (row / Letters.Length) + 1),
        };
}

public sealed record Sheet(SheetSize Size, SheetOrientation Orientation) {
    public (Length Width, Length Height) Extent => Orientation.Extent(Size);
    public PaperRectangle Field =>
        (Size.Series.Convention, Extent) switch {
            var (convention, (width, height)) => new(convention.Binding, convention.Border, width - convention.Border, height - convention.Border),
        };
    public ZoneGrid Zones =>
        (Size.Series.Convention, Extent, Field) switch {
            var (convention, extent, drawing) => new(
                convention.Divisions(extent.Width, drawing.Left, drawing.Right),
                convention.Divisions(extent.Height, drawing.Bottom, drawing.Top),
                convention,
                !convention.SmallestTopRight
                    || toSeq(SheetSize.Items).Exists(other => other.Series.Convention == convention && other.Sides.Short < Size.Sides.Short)),
        };

    public Option<string> Locate(Length x, Length y) =>
        (Field, Zones) switch {
            var (drawing, zones) when x >= drawing.Left && x <= drawing.Right && y >= drawing.Bottom && y <= drawing.Top =>
                Some(zones.Row(zones.Rows.Filter(division => division <= y).Count) + zones.Column(zones.Columns.Filter(division => division <= x).Count)),
            _ => None,
        };

    public static Option<Sheet> Match(Length width, Length height) =>
        (from size in toSeq(SheetSize.Items)
         from orientation in toSeq(SheetOrientation.Items)
         select new Sheet(size, orientation))
            .Find(sheet => sheet.Extent == (width, height));
}
