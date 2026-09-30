using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Annotation;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LinetypeShape {
    public sealed record FromCurve(Curve Curve, double Offset) : LinetypeShape;

    public sealed record FromText(TextEntity Text, double Offset) : LinetypeShape;
}

public sealed record LinetypeTaper(double StartWidth, Option<Point2d> TaperPoint, double EndWidth);

public sealed record LinetypeDefinition(
    Option<string> Name,
    Seq<double> Segments,
    Seq<LinetypeShape> Shapes,
    double ShapeSpacing,
    double ShapeGap,
    Vector2d ShapeLocalOffset,
    Option<LinetypeTaper> Taper,
    LineCapStyle LineCapStyle,
    LineJoinStyle LineJoinStyle,
    double Width,
    UnitSystem WidthUnits,
    bool AlwaysModelDistances,
    HashMap<string, string> UserStrings);

public sealed record LinetypeRow(
    Guid Id,
    int Index,
    Option<string> Name,
    Seq<double> Segments,
    double PatternLength,
    bool HasShapes,
    double ShapeSpacing,
    double ShapeGap,
    Vector2d ShapeLocalOffset,
    BoundingBox ShapeBounds,
    Seq<Point2d> TaperPoints,
    LineCapStyle LineCapStyle,
    LineJoinStyle LineJoinStyle,
    double Width,
    UnitSystem WidthUnits,
    bool AlwaysModelDistances,
    bool IsPatternLocked,
    bool InUse,
    bool IsModified,
    bool IsDeleted,
    bool IsReference,
    HashMap<string, string> UserStrings);

public sealed record LinetypeTableState(int Count, int ActiveCount, Option<LinetypeRef> CurrentLinetype, ObjectLinetypeSource CurrentLinetypeSource, double LinetypeScale);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
public static partial class Linetypes {
    // --- [READS]
    public static IO<LinetypeRow> Row(Linetype linetype) =>
        from userStrings in GeometryOps.ReadUserStrings(GeometryOps.UserStrings(linetype))
        select Project(linetype, userStrings);

    [MapPropertyFromSource(nameof(LinetypeRow.Segments), Use = nameof(Segments))]
    [MapPropertyFromSource(nameof(LinetypeRow.TaperPoints), Use = nameof(TaperPoints))]
    private static partial LinetypeRow Project(Linetype linetype, HashMap<string, string> userStrings);

    private static Seq<double> Segments(Linetype linetype) =>
        toSeq(Range(0, linetype.SegmentCount)).Map(index => {
            linetype.GetSegment(index, out double length, out bool solid);
            return solid ? length : -length;
        }).Strict();

    private static Seq<Point2d> TaperPoints(Linetype linetype) =>
        toSeq(linetype.GetTaperPoints());

    public static IO<LinetypeTableState> ReadTable(RhinoDoc doc) =>
        IO.lift(() => Table(doc.Linetypes));

    [MapProperty(nameof(LinetypeTable.CurrentLinetypeIndex), nameof(LinetypeTableState.CurrentLinetype), Use = nameof(@LinetypeRef.FromHost))]
    private static partial LinetypeTableState Table(LinetypeTable table);

    // --- [WRITES]
    public static IO<Linetype> FromPatternString(string patternString, bool millimeters) =>
        IO.lift(() => Missing.Unless(Linetype.CreateFromPatternString(patternString, millimeters), nameof(Linetype.CreateFromPatternString)));

    public static IO<Unit> Written(Linetype staged, LinetypeDefinition definition) =>
        from named in TableOps.Named(staged, definition.Name)
        from segmented in IO.lift(() => Invalid.Unless(!definition.Segments.IsEmpty, nameof(definition.Segments))
            .Bind(_ => Refused.Unless(staged.SetSegments(definition.Segments), nameof(Linetype.SetSegments))))
        from unshaped in IO.lift(staged.RemoveAllShapes)
        from shaped in IO.lift(() => Answers.Each(
            definition.Shapes,
            shape => shape.Switch(
                staged,
                fromCurve: static (target, curve) => target.AddShape(curve.Curve, curve.Offset),
                fromText: static (target, text) => target.AddShape(text.Text, text.Offset)),
            nameof(Linetype.AddShape)))
        from settings in IO.lift(() => Update(definition, staged))
        from tapered in IO.lift(() => definition.Taper.Match(
            Some: taper => taper.TaperPoint.Match(
                Some: point => staged.SetTaper(taper.StartWidth, point, taper.EndWidth),
                None: () => staged.SetTaper(taper.StartWidth, taper.EndWidth)),
            None: staged.RemoveTaper))
        from stored in Appearance.WriteUserStrings(GeometryOps.UserStrings(staged), definition.UserStrings)
        select unit;

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(LinetypeDefinition.Name), Justification = "TableOps.Named")]
    [MapperIgnoreSource(nameof(LinetypeDefinition.Segments), Justification = "Linetype.SetSegments")]
    [MapperIgnoreSource(nameof(LinetypeDefinition.Shapes), Justification = "Linetype.AddShape")]
    [MapperIgnoreSource(nameof(LinetypeDefinition.Taper), Justification = "Linetype.SetTaper")]
    [MapperIgnoreSource(nameof(LinetypeDefinition.UserStrings), Justification = "Appearance.WriteUserStrings")]
    private static partial void Update(LinetypeDefinition definition, Linetype staged);

    public static IO<Unit> UndoModify(RhinoDoc doc, int index) =>
        IO.lift(() => Refused.Unless(doc.Linetypes.UndoModify(index), nameof(LinetypeTable.UndoModify)));

    public static IO<Unit> Undelete(RhinoDoc doc, ComponentRef address) =>
        TableOps.Find(doc.Linetypes, address, includeDeleted: true).Bind(row => IO.lift(() => Refused.Unless(doc.Linetypes.Undelete(row.Index), nameof(LinetypeTable.Undelete))));

    public static IO<Unit> SetCurrent(RhinoDoc doc, LinetypeRef linetype, bool quiet) =>
        from index in linetype.Resolve(doc)
        from set in IO.lift(() => Refused.Unless(doc.Linetypes.SetCurrentLinetypeIndex(index, quiet), nameof(LinetypeTable.SetCurrentLinetypeIndex)))
        select set;

    // --- [FILES]
    public static IO<Seq<LinetypeRow>> ReadFile(string path) =>
        from existing in Answers.ExistingPath(path)
        from rows in TableOps.ReadRows(() => Linetype.ReadFromFile(existing), Row)
        select rows;
}
