using System.Drawing;
using System.Numerics;
using LanguageExt.ClassInstances;
using Rasm.Drafting;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Persistence;
using Rhino;
using Rhino.DocObjects;
using Rhino.UI;

namespace Rasm.Rhino.Annotation.Styles;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record LinetypeTaper<TWidth>(TWidth Start, Option<(double Position, TWidth Width)> Waist, TWidth End) where TWidth : struct {
    public (double Start, Option<Point2d> Waist, double End) Lowered(Func<TWidth, double> scalar) =>
        (scalar(Start), Waist.Map(point => new Point2d(point.Position, scalar(point.Width))), scalar(End));
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record LinetypeWidth {
    public sealed record Screen(double Width, Option<LinetypeTaper<double>> Taper) : LinetypeWidth;

    public sealed record Distance(Length Width, Option<LinetypeTaper<Length>> Taper) : LinetypeWidth;
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record LinetypeShape {
    public sealed record FromCurve(Curve Curve, Length Offset) : LinetypeShape;

    public sealed record FromText(TextEntity Text, Length Offset) : LinetypeShape;
}

public sealed record LinetypeShapeLayout(Length ShapeSpacing, Length ShapeGap, (Length X, Length Y) ShapeLocalOffset);

public sealed record LinetypeShapes(IterableNE<LinetypeShape> Items, LinetypeShapeLayout Layout);

public sealed record LinetypeSettings(Seq<Length> Pattern, LinetypeWidth Width, LineCapStyle LineCapStyle) {
    public Option<LineJoinStyle> LineJoinStyle { get; init; }
    public Option<bool> AlwaysModelDistances { get; init; }
    public HashMap<EqStringOrdinalIgnoreCase, string, string> UserStrings { get; init; }
}

public sealed record LinetypeDefinition(LinetypeSettings Settings, Option<LinetypeShapes> Shapes) {
    public static LinetypeDefinition Of(LineType type, LineWidth width, LineCap cap) =>
        new(new LinetypeSettings(toSeq(type.Pattern(width, cap)), new LinetypeWidth.Distance(width.Width, None), cap.Map(butt: LineCapStyle.Flat, round: LineCapStyle.Round)), None);
}

public sealed record LinetypeTableState(LinetypeRef Current, ObjectLinetypeSource CurrentLinetypeSource, double LinetypeScale);

public sealed record LinetypePreview(Seq<Seq<Vector2>> Dashes, Seq<Seq<Vector2>> CurveShapes, Seq<Seq<Vector2>> TextSurfaceShapes);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Linetypes {
    // --- [NAMES]
    public static string Name(LineType type, LineWidth width) =>
        StyleTables.Sized($"ISO 128 {type.Key}", width);

    // --- [READS]
    public static IO<LinetypeDefinition> Definition(Linetype linetype) =>
        IO.lift(() => new LinetypeDefinition(
            new LinetypeSettings(Segments(linetype), Width(linetype, toSeq(linetype.GetTaperPoints())), linetype.LineCapStyle) {
                LineJoinStyle = linetype.LineJoinStyle, AlwaysModelDistances = linetype.AlwaysModelDistances, UserStrings = UserStrings.Of(linetype.GetUserStrings()),
            },
            None));

    public static IO<Option<LinetypeShapeLayout>> Layout(Linetype linetype) =>
        IO.lift(() => Conversions.Present(
            linetype.HasShapes,
            new LinetypeShapeLayout(
                Length.FromMillimeters(Quantities.Exact(linetype.ShapeSpacing)),
                Length.FromMillimeters(Quantities.Exact(linetype.ShapeGap)),
                (Length.FromMillimeters(Quantities.Exact(linetype.ShapeLocalOffset.X)), Length.FromMillimeters(Quantities.Exact(linetype.ShapeLocalOffset.Y))))));

    public static IO<bool> Unchanged(Linetype live, Linetype staged) =>
        (Definition(live), Definition(staged), IO.lift(() => live.HasShapes || staged.HasShapes))
            .Apply(static (held, written, shaped) => !shaped && held == written)
            .As();

    public static IO<LinetypeRef> Effective(RhinoDoc doc, RhinoObject target) =>
        IO.lift(() => Conversions.Required(LinetypeRef.FromHost(doc.Linetypes.LinetypeIndexForObject(target))));

    private static Seq<Length> Segments(Linetype linetype) =>
        toSeq(Range(0, linetype.SegmentCount)).Map(index => {
            linetype.GetSegment(index, out double length, out bool solid);
            return Length.FromMillimeters(Quantities.Exact(solid ? length : -length));
        }).Strict();

    private static LinetypeWidth Width(Linetype linetype, Seq<Point2d> taper) =>
        linetype.WidthUnits is UnitSystem.None
            ? new LinetypeWidth.Screen(linetype.Width, Taper(taper, identity))
            : LengthUnit.FromKnownUnitSystem(linetype.WidthUnits) switch {
                var widthUnit => new LinetypeWidth.Distance(Quantities.From(linetype.Width, widthUnit), Taper(taper, value => Quantities.From(value, widthUnit))),
            };

    private static Option<LinetypeTaper<TWidth>> Taper<TWidth>(Seq<Point2d> points, Func<double, TWidth> width) where TWidth : struct =>
        from start in points.Head
        from end in points.Last
        select new LinetypeTaper<TWidth>(width(start.Y), points.Tail.Init.Head.Map(point => (point.X, width(point.Y))), width(end.Y));

    // --- [WRITES]
    public static IO<Unit> Written(Linetype staged, LinetypeDefinition definition) =>
        from present in IO.lift(() => Segments(staged))
        from patterned in unless(present == definition.Settings.Pattern, IO.lift(() => RhinoFault.Unless(
            !definition.Settings.Pattern.IsEmpty && staged.SetSegments(definition.Settings.Pattern.Map(Millimeters)), RhinoReason.Declined, definition.Settings.Pattern))).As()
        from taper in IO.lift(() => definition.Settings.Width.Switch(
            staged,
            screen: static (row, screen) => {
                (row.Width, row.WidthUnits) = (screen.Width, UnitSystem.None);
                return screen.Taper.Map(static held => held.Lowered(identity));
            },
            distance: static (row, distance) => {
                (row.Width, row.WidthUnits) = (Millimeters(distance.Width), UnitSystem.Millimeters);
                return distance.Taper.Map(static held => held.Lowered(Millimeters));
            }))
        from styled in IO.lift(() => {
            staged.LineCapStyle = definition.Settings.LineCapStyle;
            _ = definition.Settings.LineJoinStyle.Iter(joint => staged.LineJoinStyle = joint);
            _ = definition.Settings.AlwaysModelDistances.Iter(model => staged.AlwaysModelDistances = model);
        })
        from tapered in IO.lift(() => taper.Match(
            Some: lowered => lowered.Waist.Match(
                Some: waist => staged.SetTaper(lowered.Start, waist, lowered.End),
                None: () => staged.SetTaper(lowered.Start, lowered.End)),
            None: staged.RemoveTaper))
        from cleared in IO.lift(staged.RemoveAllShapes)
        let shapes = definition.Shapes.ToSeq().Bind(static held => toSeq(held.Items))
        from added in IO.lift(() => Products.Each(shapes, (shape, index) => shape.Switch(
            (Row: staged, Index: index),
            fromCurve: static (at, curve) => RhinoFault.Unless(at.Row.AddShape(curve.Curve, Millimeters(curve.Offset)), RhinoReason.Declined, curve.Offset, at.Index),
            fromText: static (at, text) => RhinoFault.Unless(at.Row.AddShape(text.Text, Millimeters(text.Offset)), RhinoReason.Declined, text.Offset, at.Index))))
        from laid in IO.lift(() => definition.Shapes.Iter(placed =>
            (staged.ShapeSpacing, staged.ShapeGap, staged.ShapeLocalOffset) = (
                Millimeters(placed.Layout.ShapeSpacing), Millimeters(placed.Layout.ShapeGap), new Vector2d(Millimeters(placed.Layout.ShapeLocalOffset.X), Millimeters(placed.Layout.ShapeLocalOffset.Y)))))
        from strung in StyleTables.Strung(staged.GetUserStrings, staged.SetUserString, definition.Settings.UserStrings)
        select unit;

    public static IO<int> AddReference(RhinoDoc doc, string name, LinetypeDefinition definition) =>
        (from staged in use(static () => new Linetype())
         from named in IO.lift(() => { staged.Name = name; })
         from written in Written(staged, definition)
         from index in IO.lift(() => Conversions.Required(Conversions.Present(doc.Linetypes.AddReferenceLinetype(staged))))
         select index).Bracket();

    // --- [TABLE]
    public static IO<LinetypeTableState> ReadTable(RhinoDoc doc) =>
        IO.lift(() => Conversions.Required(LinetypeRef.FromHost(doc.Linetypes.CurrentLinetypeIndex))
            .Map(current => new LinetypeTableState(current, doc.Linetypes.CurrentLinetypeSource, doc.Linetypes.LinetypeScale)));

    public static IO<Unit> WriteTable(RhinoDoc doc, LinetypeTableState state) =>
        from index in state.Current.Resolve(doc)
        from current in unless(doc.Linetypes.CurrentLinetypeIndex == index, Products.Accepted(() => doc.Linetypes.SetCurrentLinetypeIndex(index, quiet: true))).As()
        from source in IO.lift(() => { (doc.Linetypes.CurrentLinetypeSource, doc.Linetypes.LinetypeScale) = (state.CurrentLinetypeSource, state.LinetypeScale); })
        select source;

    // --- [TEXT]
    public static IO<Seq<Length>> Parse(string text, bool millimeters) =>
        Products.Of(() => Linetype.CreateFromPatternString(text, millimeters)).Map(Segments).Bracket();

    public static IO<string> Text(Seq<Length> pattern, bool millimeters) =>
        (from scratch in use(static () => new Linetype())
         from written in IO.lift(() => RhinoFault.Unless(!pattern.IsEmpty && scratch.SetSegments(pattern.Map(Millimeters)), RhinoReason.Declined, pattern))
         select scratch.PatternString(millimeters)).Bracket();

    // --- [PREVIEW]
    private const double Span = 50d;

    public static IO<Seq<LinetypePreview>> Preview(Seq<Linetype> linetypes, Size pixels) =>
        (from curve in use(static () => new LineCurve(Point3d.Origin, new Point3d(Span, 0d, 0d)))
         let extent = linetypes.Fold(0d, static (held, linetype) => Math.Max(held, LinetypePreviews.MaxExtent(owner: null, linetype)))
         from previews in linetypes.TraverseM(linetype => Previewed(curve, linetype, pixels, extent)).As()
         select previews).Bracket();

    private static IO<LinetypePreview> Previewed(Curve curve, Linetype linetype, Size pixels, double extent) =>
        (from copy in use(() => new Linetype(linetype) { WidthUnits = UnitSystem.None })
         let factor = copy.SegmentCount > 1 && copy.PatternLength > 0d && (Span / copy.PatternLength is < 5d or > 50d) ? 10d / copy.PatternLength : 1d
         from rescaled in IO.lift(() => Products.Each(toSeq(Range(0, copy.SegmentCount)), (segment, index) => {
             copy.GetSegment(segment, out double length, out bool solid);
             return RhinoFault.Unless(copy.SetSegment(segment, length * factor, solid), RhinoReason.Declined, length, index);
         }))
         let scale = extent > 0d ? 0.6d * Span * pixels.Height / pixels.Width / extent : factor
         select new LinetypePreview(Channel(curve, copy, pixels, scale, 0), Channel(curve, copy, pixels, scale, 1), Channel(curve, copy, pixels, scale, 2))).Bracket();

    private static Seq<Seq<Vector2>> Channel(Curve curve, Linetype linetype, Size pixels, double scale, int kind) =>
        toSeq(DrawingUtilities.CreateLinetypePreviewGeometryEx(curve, linetype, pixels.Width, pixels.Height, scale, kind))
            .Map(static run => toSeq(run).Map(static point => new Vector2(point.X, point.Y)).Strict())
            .Strict();

    // --- [FILES]
    public static IO<Seq<(string Name, LinetypeDefinition Definition)>> ReadFile(InputPath path) =>
        (from rows in Products.Many(() => Linetype.ReadFromFile(path))
         from definitions in rows.TraverseM(static row => Definition(row).Map(definition => (row.Name, Definition: definition))).As()
         select definitions).Bracket();

    // --- [UNITS]
    private static double Millimeters(Length value) => value.Millimeters.ToDouble();
}
