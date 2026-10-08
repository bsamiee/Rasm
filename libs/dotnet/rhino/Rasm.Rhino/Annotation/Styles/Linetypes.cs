using LanguageExt.ClassInstances;
using Rasm.Drafting;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using UnitsNet;
using UnitSystem = Rhino.UnitSystem;

namespace Rasm.Rhino.Annotation.Styles;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<InvalidRhinoValue>]
public sealed partial class LinetypePattern {
    public Seq<Length> Segments { get; }

    public Length PatternLength => Segments.Fold(Length.Zero, static (sum, segment) => sum + (segment < Length.Zero ? -segment : segment));

    public Seq<double> Millimeters => Segments.Map(static segment => segment.Millimeters.ToDouble()).Strict();

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref Seq<Length> segments) =>
        validationError = segments.ForAll(static segment => QuantityValue.IsFinite(segment.Value)) && segments switch {
            [] => false,
            [var only] => only > Length.Zero,
            _ => segments.Zip(segments.Tail).ForAll(static pair => (pair.First >= Length.Zero) != (pair.Second >= Length.Zero)),
        } ? null : new InvalidRhinoValue();
}

[ComplexValueObject]
[ValidationError<InvalidRhinoValue>]
public sealed partial class LinetypeTaper<TWidth> where TWidth : struct, IComparable<TWidth> {
    public TWidth Start { get; }
    public Option<(double Position, TWidth Width)> Waist { get; }
    public TWidth End { get; }

    public (double Start, Option<Point2d> Waist, double End) Lowered(Func<TWidth, double> scalar) =>
        (scalar(Start), Waist.Map(point => new Point2d(point.Position, scalar(point.Width))), scalar(End));

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref TWidth start, ref Option<(double Position, TWidth Width)> waist, ref TWidth end) =>
        validationError = start.CompareTo(default) >= 0 && end.CompareTo(default) >= 0
            && waist.ForAll(static point => point.Position is >= 0.0 and <= 1.0 && point.Width.CompareTo(default) >= 0) ? null : new InvalidRhinoValue();
}

[Union]
public abstract partial record LinetypeWidth {
    public sealed record Pixels(double Width, Option<LinetypeTaper<double>> Taper) : LinetypeWidth;

    public sealed record Distance(Length Width, Option<LinetypeTaper<Length>> Taper) : LinetypeWidth;
}

[Union]
public abstract partial record LinetypeShape {
    public sealed record FromCurve(Curve Curve, Length Offset) : LinetypeShape;

    public sealed record FromText(TextEntity Text, Length Offset) : LinetypeShape;
}

public sealed record LinetypeShapeLayout(Length ShapeSpacing, Length ShapeGap, (Length X, Length Y) ShapeLocalOffset);

public sealed record LinetypeShapes(IterableNE<LinetypeShape> Items, LinetypeShapeLayout Layout);

public sealed record LinetypeSettings(
    LinetypePattern Pattern,
    LinetypeWidth Width,
    LineCapStyle LineCapStyle,
    LineJoinStyle LineJoinStyle,
    bool AlwaysModelDistances,
    HashMap<EqStringOrdinalIgnoreCase, string, string> UserStrings);

public sealed record LinetypeDefinition(LinetypeSettings Settings, Option<LinetypeShapes> Shapes) {
    public static Fin<LinetypeDefinition> Of(LineType type, LineWidth width) =>
        LinetypePattern.Validate(toSeq(type.Pattern(width)), out LinetypePattern? pattern) is { } error
            ? error
            : new LinetypeDefinition(
                new LinetypeSettings(pattern!, new LinetypeWidth.Distance(width.Width, None), LineCapStyle.Flat, LineJoinStyle.Round, AlwaysModelDistances: false, HashMap<EqStringOrdinalIgnoreCase, string, string>()),
                None);
}

public sealed record LinetypeTableState(LinetypeRef Current, ObjectLinetypeSource CurrentLinetypeSource, double LinetypeScale);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Linetypes {
    // --- [READS]
    public static IO<LinetypeDefinition> Definition(Linetype linetype) =>
        IO.lift(Fin<LinetypeDefinition> () => LinetypePattern.Validate(Segments(linetype), out LinetypePattern? pattern) is { } error
            ? error
            : Width(linetype, toSeq(linetype.GetTaperPoints())).Map(width => new LinetypeDefinition(
                new LinetypeSettings(pattern!, width, linetype.LineCapStyle, linetype.LineJoinStyle, linetype.AlwaysModelDistances, UserStrings.Held(linetype.GetUserStrings())),
                None)));

    public static IO<Option<LinetypeShapeLayout>> Layout(Linetype linetype) =>
        IO.lift(() => Callbacks.Found(
            linetype.HasShapes,
            new LinetypeShapeLayout(
                Length.FromMillimeters(linetype.ShapeSpacing),
                Length.FromMillimeters(linetype.ShapeGap),
                (Length.FromMillimeters(linetype.ShapeLocalOffset.X), Length.FromMillimeters(linetype.ShapeLocalOffset.Y)))));

    public static IO<bool> Unchanged(Linetype live, Linetype staged) =>
        (Definition(live), Definition(staged), IO.lift(() => live.HasShapes || staged.HasShapes))
            .Apply(static (held, written, shaped) => !shaped && held == written)
            .As();

    private static Seq<Length> Segments(Linetype linetype) =>
        toSeq(Range(0, linetype.SegmentCount)).Map(index => {
            linetype.GetSegment(index, out double length, out bool solid);
            return Length.FromMillimeters(solid ? length : -length);
        }).Strict();

    private static Fin<LinetypeWidth> Width(Linetype linetype, Seq<Point2d> taper) =>
        linetype.WidthUnits is UnitSystem.None
            ? Taper(taper, static count => count).Map<LinetypeWidth>(profile => new LinetypeWidth.Pixels(linetype.Width, profile))
            : Measured(linetype.Width, taper, LengthUnit.FromKnownUnitSystem(linetype.WidthUnits));

    private static Fin<LinetypeWidth> Measured(double width, Seq<Point2d> taper, LengthUnit widthUnit) =>
        Taper(taper, value => Quantities.From(value, widthUnit)).Map<LinetypeWidth>(profile => new LinetypeWidth.Distance(Quantities.From(width, widthUnit), profile));

    private static Fin<Option<LinetypeTaper<TWidth>>> Taper<TWidth>(Seq<Point2d> points, Func<double, TWidth> width) where TWidth : struct, IComparable<TWidth> =>
        (from start in points.Head
         from end in points.Last
         select (Start: start, End: end))
            .Traverse(Fin<LinetypeTaper<TWidth>> (ends) =>
                LinetypeTaper<TWidth>.Validate(width(ends.Start.Y), points.Tail.Init.Head.Map(point => (point.X, width(point.Y))), width(ends.End.Y), out LinetypeTaper<TWidth>? taper) is { } error
                    ? error
                    : taper!)
            .As();

    // --- [WRITES]
    public static IO<Unit> Written(Linetype staged, LinetypeDefinition definition) =>
        from held in IO.lift(() => Segments(staged))
        from patterned in unless(held == definition.Settings.Pattern.Segments, IO.lift(() =>
            Refused.Unless(staged.SetSegments(definition.Settings.Pattern.Millimeters), nameof(Linetype.SetSegments)))).As()
        from styled in IO.lift(() => {
            staged.LineCapStyle = definition.Settings.LineCapStyle;
            staged.LineJoinStyle = definition.Settings.LineJoinStyle;
            staged.AlwaysModelDistances = definition.Settings.AlwaysModelDistances;
        })
        from widened in definition.Settings.Width.Switch(
            staged,
            pixels: static (row, pixels) => Stroked(row, pixels.Width, UnitSystem.None, pixels.Taper.Map(static taper => taper.Lowered(static count => count))),
            distance: static (row, distance) => Stroked(
                row, distance.Width.Millimeters.ToDouble(), UnitSystem.Millimeters, distance.Taper.Map(static taper => taper.Lowered(static length => length.Millimeters.ToDouble()))))
        from cleared in IO.lift(staged.RemoveAllShapes)
        from shaped in definition.Shapes.Traverse(shapes => Shaped(staged, shapes)).As()
        from strings in IO.lift(() => UserStrings.Held(staged.GetUserStrings()))
        from stored in UserStrings.Write(strings, staged.SetUserString, UserStrings.Replacing(strings, definition.Settings.UserStrings))
        select unit;

    public static IO<int> AddReference(RhinoDoc doc, string name, LinetypeDefinition definition) =>
        (from staged in use(static () => new Linetype())
         from named in TableOps.Named(staged, Some(name))
         from written in Written(staged, definition)
         from index in IO.lift(() => Conversions.Required(doc.Linetypes.AddReferenceLinetype(staged), nameof(LinetypeTable.AddReferenceLinetype)))
         select index).Bracket();

    private static IO<Unit> Stroked(Linetype staged, double width, UnitSystem units, Option<(double Start, Option<Point2d> Waist, double End)> taper) =>
        IO.lift(() => {
            (staged.Width, staged.WidthUnits) = (width, units);
            taper.Match(
                Some: profile => profile.Waist.Match(
                    Some: waist => staged.SetTaper(profile.Start, waist, profile.End),
                    None: () => staged.SetTaper(profile.Start, profile.End)),
                None: staged.RemoveTaper);
        });

    private static IO<Unit> Shaped(Linetype staged, LinetypeShapes shapes) =>
        from added in IO.lift(() => Callbacks.Each(
            toSeq(shapes.Items),
            shape => shape.Switch(
                staged,
                fromCurve: static (row, curve) => row.AddShape(curve.Curve, curve.Offset.Millimeters.ToDouble()),
                fromText: static (row, text) => row.AddShape(text.Text, text.Offset.Millimeters.ToDouble())),
            nameof(Linetype.AddShape)))
        from laid in IO.lift(() => {
            staged.ShapeSpacing = shapes.Layout.ShapeSpacing.Millimeters.ToDouble();
            staged.ShapeGap = shapes.Layout.ShapeGap.Millimeters.ToDouble();
            staged.ShapeLocalOffset = new Vector2d(shapes.Layout.ShapeLocalOffset.X.Millimeters.ToDouble(), shapes.Layout.ShapeLocalOffset.Y.Millimeters.ToDouble());
        })
        select laid;

    // --- [TABLE]
    public static IO<LinetypeTableState> ReadTable(RhinoDoc doc) =>
        IO.lift(() => LinetypeRef.FromHost(doc.Linetypes.CurrentLinetypeIndex)
            .ToFin(new InvalidAnswer(nameof(LinetypeTable.CurrentLinetypeIndex)))
            .Map(current => new LinetypeTableState(current, doc.Linetypes.CurrentLinetypeSource, doc.Linetypes.LinetypeScale)));

    public static IO<Unit> WriteTable(RhinoDoc doc, LinetypeTableState state) =>
        from index in state.Current.Resolve(doc)
        from current in IO.lift(() => Refused.Unless(
            doc.Linetypes.CurrentLinetypeIndex == index || doc.Linetypes.SetCurrentLinetypeIndex(index, quiet: true), nameof(LinetypeTable.SetCurrentLinetypeIndex)))
        from source in IO.lift(() => {
            doc.Linetypes.CurrentLinetypeSource = state.CurrentLinetypeSource;
            doc.Linetypes.LinetypeScale = state.LinetypeScale;
        })
        select source;

    public static IO<LinetypeRef> Effective(RhinoDoc doc, RhinoObject target) =>
        IO.lift(() => LinetypeRef.FromHost(doc.Linetypes.LinetypeIndexForObject(target)).ToFin(new InvalidAnswer(nameof(LinetypeTable.LinetypeIndexForObject))));

    public static IO<Unit> UndoModify(RhinoDoc doc, ComponentRef<Linetype> address) =>
        TableOps.Find(doc.Linetypes, address, includeDeleted: false)
            .Bind(row => IO.lift(() => Refused.Unless(doc.Linetypes.UndoModify(row.Index), nameof(LinetypeTable.UndoModify))));

    public static IO<Unit> Undelete(RhinoDoc doc, ComponentRef<Linetype> address) =>
        TableOps.Find(doc.Linetypes, address, includeDeleted: true)
            .Bind(row => IO.lift(() => Refused.Unless(doc.Linetypes.Undelete(row.Index), nameof(LinetypeTable.Undelete))));

    // --- [TEXT]
    public static IO<LinetypePattern> Parse(string text, bool millimeters) =>
        use(IO.lift(() => Optional(Linetype.CreateFromPatternString(text, millimeters))
                .ToFin(new UnparsedText(typeof(Linetype), nameof(Linetype.CreateFromPatternString), text))))
            .Bind(static parsed => IO.lift(Fin<LinetypePattern> () => LinetypePattern.Validate(Segments(parsed), out LinetypePattern? pattern) is { } error ? error : pattern!))
            .Bracket();

    public static IO<string> Text(LinetypePattern pattern, bool millimeters) =>
        use(static () => new Linetype())
            .Bind(scratch => IO.lift(() => Refused.Unless(scratch.SetSegments(pattern.Millimeters), nameof(Linetype.SetSegments)).Map(_ => scratch.PatternString(millimeters))))
            .Bracket();

    // --- [FILES]
    public static IO<Seq<(string Name, LinetypeDefinition Definition)>> ReadFile(string path) =>
        from existing in IO.lift(() => Exchange.ExistingPath(path))
        from rows in IO.lift(() => Conversions.Rows(Linetype.ReadFromFile(existing))).Bracket(
            Use: static held => held.TraverseM(static row => Definition(row).Map(definition => (row.Name, Definition: definition))).As(),
            Fin: DisposalOps.Release)
        select rows;
}
