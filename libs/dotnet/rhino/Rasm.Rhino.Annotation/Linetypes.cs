using LanguageExt.UnsafeValueAccess;
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
    string Name,
    Seq<double> Segments,
    Seq<LinetypeShape> Shapes,
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

public sealed record LinetypeTableState(
    int ItemCount,
    int ActiveCount,
    int CurrentLinetypeIndex,
    ObjectLinetypeSource CurrentLinetypeSource,
    double LinetypeScale,
    Option<string> ContinuousLinetypeName,
    Option<string> ByLayerLinetypeName,
    Option<string> ByParentLinetypeName);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
public static partial class Linetypes {
    // --- [TABLE]
    internal static TableAccessors<Linetype> Accessors(RhinoDoc doc, bool reference) =>
        new(
            doc.Linetypes,
            reference ? doc.Linetypes.AddReferenceLinetype : doc.Linetypes.Add,
            doc.Linetypes.Modify,
            name => Answers.Present(doc.Linetypes.Find(name)).Map(doc.Linetypes.FindIndex).ValueUnsafe(),
            doc.Linetypes.FindIndex);

    [MapPropertyFromSource(nameof(LinetypeRow.Segments), Use = nameof(Segments))]
    [MapPropertyFromSource(nameof(LinetypeRow.TaperPoints), Use = nameof(TaperPoints))]
    private static partial LinetypeRow Project(Linetype linetype, HashMap<string, string> userStrings);

    public static IO<LinetypeRow> Row(Linetype linetype) =>
        from userStrings in GeometryOps.ReadUserStrings(GeometryOps.UserStrings(linetype))
        from row in IO.lift(() => Project(linetype, userStrings))
        select row;

    private static Seq<double> Segments(Linetype linetype) =>
        toSeq(Range(0, linetype.SegmentCount)).Map(index => {
            linetype.GetSegment(index, out double length, out bool solid);
            return solid ? length : -length;
        }).Strict();

    private static Seq<Point2d> TaperPoints(Linetype linetype) =>
        toSeq(linetype.GetTaperPoints());

    [MapProperty(nameof(LinetypeTable.Count), nameof(LinetypeTableState.ItemCount))]
    private static partial LinetypeTableState Table(LinetypeTable table);

    // --- [ROWS]
    public static IO<int> Add(RhinoDoc doc, LinetypeDefinition definition, bool reference) =>
        TableOps.AddRow(Accessors(doc, reference), IO.lift(static () => new Linetype()), staged => Written(staged, definition));

    public static IO<int> AddFromPatternString(RhinoDoc doc, string name, string patternString, bool millimeters, bool reference) =>
        TableOps.AddRow(
            Accessors(doc, reference),
            IO.lift(() => Missing.Unless(Linetype.CreateFromPatternString(patternString, millimeters), nameof(Linetype.CreateFromPatternString))),
            staged => IO.lift(() => { staged.Name = name; }));

    public static IO<Unit> Modify(RhinoDoc doc, int index, LinetypeDefinition definition, bool quiet) =>
        TableOps.ModifyRow(Accessors(doc, reference: false), index, static live => new Linetype(live), staged => Written(staged, definition), quiet);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(LinetypeDefinition.Segments), Justification = "Written through SetSegments")]
    [MapperIgnoreSource(nameof(LinetypeDefinition.Shapes), Justification = "Written through AddShape")]
    [MapperIgnoreSource(nameof(LinetypeDefinition.Taper), Justification = "Written through SetTaper")]
    [MapperIgnoreSource(nameof(LinetypeDefinition.UserStrings), Justification = "Written through the user string table")]
    private static partial void Update(LinetypeDefinition definition, Linetype staged);

    private static IO<Unit> Written(Linetype staged, LinetypeDefinition definition) =>
        from settings in IO.lift(() => {
            Update(definition, staged);
            staged.RemoveAllShapes();
        })
        from segmented in IO.lift(() => Refused.Unless(staged.SetSegments(definition.Segments), nameof(Linetype.SetSegments)))
        from shaped in IO.lift(() => definition.Shapes.TraverseM(shape => Refused.Unless(
                shape.Switch(
                    staged,
                    fromCurve: static (target, curve) => target.AddShape(curve.Curve, curve.Offset),
                    fromText: static (target, text) => target.AddShape(text.Text, text.Offset)),
                nameof(Linetype.AddShape)))
            .As())
        from tapered in IO.lift(() => definition.Taper.Match(
            Some: taper => taper.TaperPoint.Match(
                Some: point => staged.SetTaper(taper.StartWidth, point, taper.EndWidth),
                None: () => staged.SetTaper(taper.StartWidth, taper.EndWidth)),
            None: staged.RemoveTaper))
        from stored in GeometryOps.WriteUserStrings(GeometryOps.UserStrings(staged), definition.UserStrings)
        select unit;

    public static IO<Unit> Reset(RhinoDoc doc, int index, bool quiet) =>
        TableOps.ModifyRow(Accessors(doc, reference: false), index, static live => new Linetype(live), static staged => IO.lift(staged.Default), quiet);

    public static IO<Unit> UndoModify(RhinoDoc doc, int index) =>
        IO.lift(() => Refused.Unless(doc.Linetypes.UndoModify(index), nameof(LinetypeTable.UndoModify)));

    public static IO<Unit> Delete(RhinoDoc doc, Seq<int> indices, bool quiet) =>
        IO.lift(() => Refused.Unless(doc.Linetypes.Delete(indices, quiet), nameof(LinetypeTable.Delete)));

    public static IO<Unit> Undelete(RhinoDoc doc, Guid id) =>
        from index in IO.lift(() => Answers.Present(doc.Linetypes.Find(id, ignoreDeletedLinetypes: false)).ToFin(new Missing(nameof(LinetypeTable.Find))))
        from undeleted in IO.lift(() => Refused.Unless(doc.Linetypes.Undelete(index), nameof(LinetypeTable.Undelete)))
        select undeleted;

    public static IO<Unit> SetCurrent(RhinoDoc doc, int index, bool quiet) =>
        IO.lift(() => Refused.Unless(doc.Linetypes.SetCurrentLinetypeIndex(index, quiet), nameof(LinetypeTable.SetCurrentLinetypeIndex)));

    public static IO<Option<LinetypeRow>> Find(RhinoDoc doc, ComponentRef address) =>
        from found in TableOps.Find(Accessors(doc, reference: false), address)
        from row in found.Traverse(static linetype => Row(linetype)).As()
        select row;

    public static IO<LinetypeTableState> ReadTable(RhinoDoc doc) =>
        IO.lift(() => Table(doc.Linetypes));

    public static IO<string> PatternString(RhinoDoc doc, int index, bool millimeters) =>
        IO.lift(() => Missing.Unless(doc.Linetypes.FindIndex(index), nameof(LinetypeTable.FindIndex)).Map(linetype => linetype.PatternString(millimeters)));

    // --- [FILES]
    public static IO<Seq<LinetypeRow>> ReadFile(string path) =>
        from existing in Answers.ExistingPath(path)
        from rows in TableOps.ReadRows(() => Linetype.ReadFromFile(existing), Row)
        select rows;
}
