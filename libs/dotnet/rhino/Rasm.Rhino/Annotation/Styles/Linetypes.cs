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

public sealed record LinetypeSettings(
    Seq<Length> Pattern,
    LinetypeWidth Width,
    LineCapStyle LineCapStyle,
    LineJoinStyle LineJoinStyle,
    bool AlwaysModelDistances,
    HashMap<EqStringOrdinalIgnoreCase, string, string> UserStrings) {
    internal Unit Written(Linetype staged, double width, UnitSystem widthUnits) {
        (staged.LineCapStyle, staged.LineJoinStyle, staged.AlwaysModelDistances, staged.Width, staged.WidthUnits) = (LineCapStyle, LineJoinStyle, AlwaysModelDistances, width, widthUnits);
        return unit;
    }
}

public sealed record LinetypeDefinition(LinetypeSettings Settings, Option<LinetypeShapes> Shapes) {
    public static LinetypeDefinition Of(LineType type, LineWidth width) =>
        new(new LinetypeSettings(toSeq(type.Pattern(width)), new LinetypeWidth.Distance(width.Width, None), LineCapStyle.Flat, LineJoinStyle.Round, AlwaysModelDistances: false, HashMap<EqStringOrdinalIgnoreCase, string, string>()), None);
}

public sealed record LinetypeTableState(LinetypeRef Current, ObjectLinetypeSource CurrentLinetypeSource, double LinetypeScale);

public sealed record LinetypePreview(Seq<Seq<Vector2>> Dashes, Seq<Seq<Vector2>> CurveShapes, Seq<Seq<Vector2>> TextSurfaceShapes);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Linetypes {
    // --- [READS]
    public static IO<LinetypeDefinition> Definition(Linetype linetype) =>
        IO.lift(() => new LinetypeDefinition(
            new LinetypeSettings(Segments(linetype), Width(linetype, toSeq(linetype.GetTaperPoints())), linetype.LineCapStyle, linetype.LineJoinStyle, linetype.AlwaysModelDistances, UserStrings.Held(linetype.GetUserStrings())),
            None));

    public static IO<Option<LinetypeShapeLayout>> Layout(Linetype linetype) =>
        IO.lift(() => Conversions.Found(
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
        from held in IO.lift(() => Segments(staged))
        from patterned in unless(held == definition.Settings.Pattern, IO.lift(() => RhinoFault.Unless(staged.SetSegments(definition.Settings.Pattern.Map(Millimeters))))).As()
        let stroke = definition.Settings.Width.Switch(
            screen: static screen => (screen.Width, WidthUnits: UnitSystem.None, Taper: screen.Taper.Map(static taper => taper.Lowered(identity))),
            distance: static distance => (Width: Millimeters(distance.Width), WidthUnits: UnitSystem.Millimeters, Taper: distance.Taper.Map(static taper => taper.Lowered(Millimeters))))
        from styled in IO.lift(() => definition.Settings.Written(staged, stroke.Width, stroke.WidthUnits))
        from tapered in IO.lift(() => stroke.Taper.Match(
            Some: taper => taper.Waist.Match(
                Some: waist => staged.SetTaper(taper.Start, waist, taper.End),
                None: () => staged.SetTaper(taper.Start, taper.End)),
            None: staged.RemoveTaper))
        from cleared in IO.lift(staged.RemoveAllShapes)
        let shapes = definition.Shapes.ToSeq().Bind(static held => toSeq(held.Items))
        from added in Callbacks.Each(shapes.Map((shape, index) => IO.lift(() => shape.Switch(
            (Row: staged, Index: index),
            fromCurve: static (at, curve) => RhinoFault.Unless(at.Row.AddShape(curve.Curve, Millimeters(curve.Offset)), at.Index),
            fromText: static (at, text) => RhinoFault.Unless(at.Row.AddShape(text.Text, Millimeters(text.Offset)), at.Index)))))
        from laid in IO.lift(() => definition.Shapes.Iter(shapes =>
            (staged.ShapeSpacing, staged.ShapeGap, staged.ShapeLocalOffset) = (
                Millimeters(shapes.Layout.ShapeSpacing), Millimeters(shapes.Layout.ShapeGap), new Vector2d(Millimeters(shapes.Layout.ShapeLocalOffset.X), Millimeters(shapes.Layout.ShapeLocalOffset.Y)))))
        from strings in IO.lift(() => UserStrings.Held(staged.GetUserStrings()))
        from stored in UserStrings.Write(strings, staged.SetUserString, UserStrings.Replacing(strings, definition.Settings.UserStrings))
        select unit;

    public static IO<int> AddReference(RhinoDoc doc, string name, LinetypeDefinition definition) =>
        (from staged in use(static () => new Linetype())
         from named in TableOps.Named(staged, Some(name))
         from written in Written(staged, definition)
         from index in IO.lift(() => Conversions.Found(Conversions.Present(doc.Linetypes.AddReferenceLinetype(staged))))
         select index).Bracket();

    // --- [TABLE]
    public static IO<LinetypeTableState> ReadTable(RhinoDoc doc) =>
        IO.lift(() => Conversions.Found(LinetypeRef.FromHost(doc.Linetypes.CurrentLinetypeIndex))
            .Map(current => new LinetypeTableState(current, doc.Linetypes.CurrentLinetypeSource, doc.Linetypes.LinetypeScale)));

    public static IO<Unit> WriteTable(RhinoDoc doc, LinetypeTableState state) =>
        from index in state.Current.Resolve(doc)
        from current in IO.lift(() => RhinoFault.Unless(doc.Linetypes.CurrentLinetypeIndex == index || doc.Linetypes.SetCurrentLinetypeIndex(index, quiet: true)))
        from source in IO.lift(() => { (doc.Linetypes.CurrentLinetypeSource, doc.Linetypes.LinetypeScale) = (state.CurrentLinetypeSource, state.LinetypeScale); })
        select source;

    public static IO<LinetypeRef> Effective(RhinoDoc doc, RhinoObject target) =>
        IO.lift(() => Conversions.Found(LinetypeRef.FromHost(doc.Linetypes.LinetypeIndexForObject(target))));

    public static IO<Unit> UndoModify(RhinoDoc doc, ComponentRef<Linetype> address) =>
        from row in TableOps.Find(doc.Linetypes, address, includeDeleted: false)
        from restored in IO.lift(() => RhinoFault.Unless(doc.Linetypes.UndoModify(row.Index)))
        select restored;

    public static IO<Unit> Undelete(RhinoDoc doc, ComponentRef<Linetype> address) =>
        from row in TableOps.Find(doc.Linetypes, address, includeDeleted: true)
        from restored in IO.lift(() => RhinoFault.Unless(doc.Linetypes.Undelete(row.Index)))
        select restored;

    // --- [TEXT]
    public static IO<Seq<Length>> Parse(string text, bool millimeters) =>
        (from created in IO.lift(() => Optional(Linetype.CreateFromPatternString(text, millimeters)).ToFin(new RhinoFault(text, RhinoReason.Unreadable)))
         from native in use(() => created)
         select Segments(native)).Bracket();

    public static IO<string> Text(Seq<Length> pattern, bool millimeters) =>
        (from scratch in use(static () => new Linetype())
         from written in IO.lift(() => RhinoFault.Unless(scratch.SetSegments(pattern.Map(Millimeters))))
         from text in IO.lift(() => scratch.PatternString(millimeters))
         select text).Bracket();

    // --- [PREVIEW]
    private const double Span = 50d;

    public static IO<Seq<LinetypePreview>> Preview(Seq<Linetype> linetypes, Size pixels) =>
        (from curve in use(static () => new LineCurve(Point3d.Origin, new Point3d(Span, 0d, 0d)))
         from tallest in IO.lift(() => linetypes.Fold(0d, static (held, linetype) => Math.Max(held, linetype.ShapeBounds.Diagonal.Y)))
         from previews in linetypes.TraverseM(linetype => Previewed(curve, linetype, pixels, tallest)).As()
         select previews).Bracket();

    private static IO<LinetypePreview> Previewed(Curve curve, Linetype linetype, Size pixels, double tallest) =>
        (from copy in use(() => new Linetype(linetype) { WidthUnits = UnitSystem.None })
         from factor in IO.lift(() => copy.SegmentCount > 1 && copy.PatternLength > 0d && (Span / copy.PatternLength is < 5d or > 50d) ? 10d / copy.PatternLength : 1d)
         from rescaled in IO.lift(() => Callbacks.Each(toSeq(Range(0, copy.SegmentCount)), (segment, index) => {
             copy.GetSegment(segment, out double length, out bool solid);
             return RhinoFault.Unless(copy.SetSegment(segment, length * factor, solid), index);
         }))
         let scale = tallest > 0d ? 0.6d * Span * pixels.Height / pixels.Width / tallest : factor
         select new LinetypePreview(Channel(curve, copy, pixels, scale, 0), Channel(curve, copy, pixels, scale, 1), Channel(curve, copy, pixels, scale, 2))).Bracket();

    private static Seq<Seq<Vector2>> Channel(Curve curve, Linetype linetype, Size pixels, double scale, int kind) =>
        toSeq(DrawingUtilities.CreateLinetypePreviewGeometryEx(curve, linetype, pixels.Width, pixels.Height, scale, kind))
            .Map(static run => toSeq(run).Map(static point => new Vector2(point.X, point.Y)).Strict())
            .Strict();

    // --- [FILES]
    public static IO<Seq<(string Name, LinetypeDefinition Definition)>> ReadFile(InputPath path) =>
        (from read in IO.lift(() => Linetype.ReadFromFile(path))
         from rows in Products.Made(read)
         from definitions in rows.TraverseM(static row => (IO.pure(row.Name), Definition(row)).Apply(ValueTuple.Create).As()).As()
         select definitions).Bracket();

    // --- [UNITS]
    private static double Millimeters(Length value) => value.Millimeters.ToDouble();
}
