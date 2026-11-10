using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using UnitsNet;

namespace Rasm.Drafting;

// --- [MODELS] --------------------------------------------------------------------------

public sealed record DraftingConvention {
    private DraftingConvention(Length binding, Length border, bool rowsFromTop, bool columnsFromRight, LetteringStandard lettering, Terminator terminator, RevisionAlphabet revisions,
        string zoneLetters, Func<Length, Length, Length, Seq<Length>> divisions, Option<Length> topRightZonesUpTo = default, Option<(Length Reach, Length TitleBlock)> centering = default) =>
        (Binding, Border, RowsFromTop, ColumnsFromRight, TopRightZonesUpTo, Centering, Lettering, Terminator, Revisions, ZoneLetters, Divisions) =
        (binding, border, rowsFromTop, columnsFromRight, topRightZonesUpTo, centering, lettering, terminator, revisions, zoneLetters, divisions);

    public Length Binding { get; }
    public Length Border { get; }
    public bool RowsFromTop { get; }
    public bool ColumnsFromRight { get; }
    public Option<Length> TopRightZonesUpTo { get; }
    public Option<(Length Reach, Length TitleBlock)> Centering { get; }
    public LetteringStandard Lettering { get; }
    public Terminator Terminator { get; }
    public RevisionAlphabet Revisions { get; }
    public string ZoneLetters { get; }
    public Func<Length, Length, Length, Seq<Length>> Divisions { get; }

    private const string IsoZoneLetters = "ABCDEFGHJKLMNPQRSTUVWXYZ";

    public static readonly DraftingConvention Iso = new(Length.FromMillimeters(20), Length.FromMillimeters(10), rowsFromTop: true, columnsFromRight: false,
        LetteringStandard.Iso3098, Terminator.ClosedArrow, RevisionAlphabet.Iso, IsoZoneLetters,
        static (side, _, _) =>
            from pitch in Seq(Length.FromMillimeters(50))
            from half in Seq((int)QuantityValue.Round(side / pitch / 2, MidpointRounding.ToEven))
            from k in toSeq(Range(1 - half, (2 * half) - 1))
            select (side / 2) + (pitch * k),
        topRightZonesUpTo: Length.FromMillimeters(210), centering: (Reach: Length.FromMillimeters(10), TitleBlock: Length.FromMillimeters(180)));
    public static readonly DraftingConvention Asme = new(Length.FromInches(0.5m), Length.FromInches(0.5m), rowsFromTop: false, columnsFromRight: true,
        LetteringStandard.AsmeY142, Terminator.ClosedArrow, RevisionAlphabet.Asme, IsoZoneLetters,
        static (side, _, _) =>
            from count in Seq(2 * (int)QuantityValue.Round(side / Length.FromInches(5.5m) / 2, MidpointRounding.ToPositiveInfinity))
            from k in toSeq(Range(1, count - 1))
            select side * k / count);
    public static readonly DraftingConvention Ncs = new(Length.FromInches(1.5m), Length.FromInches(0.75m), rowsFromTop: false, columnsFromRight: false,
        LetteringStandard.AsmeY142, Terminator.Oblique, RevisionAlphabet.Asme, IsoZoneLetters,
        static (_, first, last) =>
            from pitch in Seq(Length.FromInches(1.5m))
            from k in toSeq(Range(1, (int)QuantityValue.Round((last - first) / pitch, MidpointRounding.ToPositiveInfinity) - 1))
            select first + (pitch * k));

    public static Fin<DraftingConvention> Validated(Length binding, Length border, bool rowsFromTop, bool columnsFromRight, LetteringStandard lettering, Terminator terminator, RevisionAlphabet revisions,
        string zoneLetters, Func<Length, Length, Length, Seq<Length>> divisions, Option<Length> topRightZonesUpTo = default, Option<(Length Reach, Length TitleBlock)> centering = default) =>
        (DraftingFault.Unless(Seq(binding, border).ForAll(Measured) && topRightZonesUpTo.ForAll(Measured) && centering.ForAll(static held => Measured(held.Reach) && Measured(held.TitleBlock)),
                DraftingReason.Measure, (binding, border, topRightZonesUpTo, centering)),
         DraftingFault.Unless(zoneLetters.Length > 0 && toHashSet(zoneLetters).Count == zoneLetters.Length, DraftingReason.Repeated, zoneLetters))
            .Apply((_, _) => new DraftingConvention(binding, border, rowsFromTop, columnsFromRight, lettering, terminator, revisions, zoneLetters, divisions, topRightZonesUpTo, centering)).As();

    internal static bool Measured(Length length) => QuantityValue.IsFinite(length.Value) && length >= Length.Zero;
}

[Union(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial class SheetSize {
    private SheetSize((Length Short, Length Long) sides, DraftingConvention convention) => (Sides, Convention) = (sides, convention);
    public abstract string Name { get; }
    public (Length Short, Length Long) Sides { get; }
    public DraftingConvention Convention { get; }

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is SheetSize other && (Name, Sides, Convention).Equals((other.Name, other.Sides, other.Convention));
    public override int GetHashCode() => (Name, Sides, Convention).GetHashCode();

    [SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
    public sealed partial class Standard : SheetSize {
        public static readonly Standard A0 = new("A0", (Length.FromMillimeters(841), Length.FromMillimeters(1189)), DraftingConvention.Iso);
        public static readonly Standard A1 = new("A1", (Length.FromMillimeters(594), Length.FromMillimeters(841)), DraftingConvention.Iso);
        public static readonly Standard A2 = new("A2", (Length.FromMillimeters(420), Length.FromMillimeters(594)), DraftingConvention.Iso);
        public static readonly Standard A3 = new("A3", (Length.FromMillimeters(297), Length.FromMillimeters(420)), DraftingConvention.Iso);
        public static readonly Standard A4 = new("A4", (Length.FromMillimeters(210), Length.FromMillimeters(297)), DraftingConvention.Iso);
        public static readonly Standard AnsiA = new("ANSI A", (Length.FromInches(8.5m), Length.FromInches(11)), DraftingConvention.Asme);
        public static readonly Standard AnsiB = new("ANSI B", (Length.FromInches(11), Length.FromInches(17)), DraftingConvention.Asme);
        public static readonly Standard AnsiC = new("ANSI C", (Length.FromInches(17), Length.FromInches(22)), DraftingConvention.Asme);
        public static readonly Standard AnsiD = new("ANSI D", (Length.FromInches(22), Length.FromInches(34)), DraftingConvention.Asme);
        public static readonly Standard AnsiE = new("ANSI E", (Length.FromInches(34), Length.FromInches(44)), DraftingConvention.Asme);
        public static readonly Standard ArchA = new("ARCH A", (Length.FromInches(9), Length.FromInches(12)), DraftingConvention.Ncs);
        public static readonly Standard ArchB = new("ARCH B", (Length.FromInches(12), Length.FromInches(18)), DraftingConvention.Ncs);
        public static readonly Standard ArchC = new("ARCH C", (Length.FromInches(18), Length.FromInches(24)), DraftingConvention.Ncs);
        public static readonly Standard ArchD = new("ARCH D", (Length.FromInches(24), Length.FromInches(36)), DraftingConvention.Ncs);
        public static readonly Standard ArchE1 = new("ARCH E1", (Length.FromInches(30), Length.FromInches(42)), DraftingConvention.Ncs);
        public static readonly Standard ArchE = new("ARCH E", (Length.FromInches(36), Length.FromInches(48)), DraftingConvention.Ncs);

        public override string Name => Key;
    }

    public sealed class Custom : SheetSize {
        private Custom(string name, (Length Short, Length Long) sides, DraftingConvention convention) : base(sides, convention) => Name = name;
        public override string Name { get; }

        public static Fin<Custom> Validated(string name, Length width, Length height, DraftingConvention convention) =>
            name.Trim() switch {
                var trimmed => (DraftingFault.Unless(trimmed.Length > 0, DraftingReason.Blank, name),
                     DraftingFault.Unless(!Standard.TryGet(trimmed, out _), DraftingReason.Repeated, name),
                     DraftingFault.Unless(DraftingConvention.Measured(width) && DraftingConvention.Measured(height) && width > Length.Zero && height > Length.Zero, DraftingReason.Measure, (width, height)))
                    .Apply((_, _, _) => new Custom(trimmed, width <= height ? (width, height) : (height, width), convention)).As(),
            };
    }
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
    public string Row(int index) => (Convention.ZoneLetters, Convention.RowsFromTop ? Rows.Count - index : index) switch { var (letters, row) => new(letters[row % letters.Length], (row / letters.Length) + 1) };
}

[Union]
public abstract partial record FrameMark {
    public sealed record Stroke(LineWeight Weight, Seq<(Length X, Length Y)> Points) : FrameMark;

    public sealed record Label(string Text, (Length X, Length Y) Center, Length Height) : FrameMark;
}

public sealed record Sheet(SheetSize Size, SheetOrientation Orientation, DraftingConvention Convention) {
    public (Length Width, Length Height) Extent => Orientation.Extent(Size);
    public PaperRectangle Field => Extent switch { var (width, height) => new(Convention.Binding, Convention.Border, width - Convention.Border, height - Convention.Border) };
    public ZoneGrid Zones =>
        (Extent, Field) switch {
            var ((width, height), drawing) => new(Convention.Divisions(width, drawing.Left, drawing.Right), Convention.Divisions(height, drawing.Bottom, drawing.Top), Convention,
                Convention.TopRightZonesUpTo.ForAll(limit => Size.Sides.Short > limit)),
        };

    public Seq<FrameMark> Frame {
        get {
            ((Length width, Length height), PaperRectangle paper, ZoneGrid zones, Length letter) = (Extent, Field, Zones, Convention.Lettering.ZoneHeight);
            Option<Length> reach = Convention.Centering.Map(static centering => centering.Reach);
            Option<Length> blocked = Convention.Centering.Map(centering => width / 2 < paper.Right - centering.TitleBlock ? centering.Reach : Length.Zero);
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

    public static Option<Sheet> Match(Seq<SheetSize> sizes, Length width, Length height, Length tolerance) =>
        (from size in sizes from orientation in toSeq(SheetOrientation.Items) select new Sheet(size, orientation, size.Convention))
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
