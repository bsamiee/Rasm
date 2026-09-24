using LanguageExt.UnsafeValueAccess;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Annotation;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record HatchLineDefinition(double Angle, Point2d BasePoint, Vector2d Offset, Seq<double> Dashes);

public sealed record HatchPatternDefinition(
    Option<string> Name,
    Option<string> Description,
    HatchPatternFillType FillType,
    UnitSystem PatternUnitSystem,
    bool AlwaysModelDistances,
    Seq<HatchLineDefinition> Lines,
    HashMap<string, string> UserStrings);

public sealed record HatchPatternRow(Guid Id, int Index, bool InUse, bool IsReference, HatchPatternDefinition Definition);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
public static partial class HatchPatterns {
    // --- [TABLE]
    internal static TableAccessors<HatchPattern> Accessors(RhinoDoc doc) =>
        new(doc.HatchPatterns, doc.HatchPatterns.Add, doc.HatchPatterns.Modify, doc.HatchPatterns.FindName, doc.HatchPatterns.FindIndex);

    public static IO<HatchPatternRow> Row(HatchPattern pattern) =>
        from count in IO.lift(() => pattern.HatchLineCount)
        from row in Disposal.Using(
            Disposal.AcquireAll(toSeq(Range(0, count)).Map(index => IO.lift(() => Missing.Unless(pattern.HatchLineAt(index), nameof(HatchPattern.HatchLineAt))))),
            lines =>
                from userStrings in GeometryOps.ReadUserStrings(GeometryOps.UserStrings(pattern))
                from projected in IO.lift(() => Project(pattern, lines.Map(Line).Strict(), userStrings))
                select projected)
        select row;

    [MapPropertyFromSource(nameof(HatchPatternRow.Definition), Use = nameof(Definition))]
    private static partial HatchPatternRow Project(HatchPattern pattern, Seq<HatchLineDefinition> lines, HashMap<string, string> userStrings);

    private static partial HatchPatternDefinition Definition(HatchPattern pattern, Seq<HatchLineDefinition> lines, HashMap<string, string> userStrings);

    [MapPropertyFromSource(nameof(HatchLineDefinition.Dashes), Use = nameof(Dashes))]
    private static partial HatchLineDefinition Line(HatchLine line);

    private static Seq<double> Dashes(HatchLine line) =>
        toSeq(line.GetDashes).Strict();

    // --- [ROWS]
    public static IO<int> Add(RhinoDoc doc, HatchPatternDefinition definition) =>
        TableOps.AddRow(Accessors(doc), IO.lift(static () => new HatchPattern()), staged => Written(staged, definition));

    public static IO<Unit> Modify(RhinoDoc doc, int index, HatchPatternDefinition definition, bool quiet) =>
        TableOps.ModifyRow(Accessors(doc), index, static live => new HatchPattern(live), staged => Written(staged, definition), quiet);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(HatchPatternDefinition.Name), Justification = "Written under the table name lock")]
    [MapperIgnoreSource(nameof(HatchPatternDefinition.Lines), Justification = "Written through SetHatchLines")]
    [MapperIgnoreSource(nameof(HatchPatternDefinition.UserStrings), Justification = "Written through the user string table")]
    [MapProperty(nameof(HatchPatternDefinition.Description), nameof(HatchPattern.Description), Use = nameof(Description))]
    private static partial void Update(HatchPatternDefinition definition, HatchPattern staged);

    private static string? Description(Option<string> description) => description.ValueUnsafe();

    private static IO<Unit> Written(HatchPattern staged, HatchPatternDefinition definition) =>
        Disposal.Using(
            Disposal.AcquireAll(definition.Lines.Map(static line => IO.lift(() => {
                HatchLine created = new() { Angle = line.Angle, BasePoint = line.BasePoint, Offset = line.Offset };
                created.SetDashes(line.Dashes);
                return created;
            }))),
            lines =>
                from named in TableOps.Locked(IO.lift(() => definition.Name.Iter(name => staged.Name = name)), nameof(HatchPattern.Name))
                from written in IO.lift(() => Update(definition, staged))
                from lined in IO.lift(() => Mismatch.Unless(staged.SetHatchLines(lines) == lines.Count, nameof(HatchPattern.SetHatchLines)))
                from stored in GeometryOps.WriteUserStrings(GeometryOps.UserStrings(staged), definition.UserStrings)
                select unit);

    public static IO<Unit> Delete(RhinoDoc doc, Seq<int> indices, bool quiet) =>
        IO.lift(() => CountMismatch.Unless(indices.Count, doc.HatchPatterns.Delete(indices, quiet), nameof(HatchPatternTable.Delete)));

    public static IO<Unit> SetCurrent(RhinoDoc doc, int index) =>
        IO.lift(() => {
            doc.HatchPatterns.CurrentHatchPatternIndex = index;
            return Mismatch.Unless(doc.HatchPatterns.CurrentHatchPatternIndex == index, nameof(HatchPatternTable.CurrentHatchPatternIndex));
        });

    public static IO<Option<HatchPatternRow>> Find(RhinoDoc doc, ComponentRef address) =>
        from found in TableOps.Find(Accessors(doc), address)
        from row in found.Traverse(static pattern => Row(pattern)).As()
        select row;

    // --- [FILES]
    public static IO<Seq<HatchPatternRow>> Defaults() =>
        TableOps.ReadRows(HatchPattern.GetDefaultHatchPatterns, Row);

    public static IO<Seq<HatchPatternRow>> ReadFile(string path) =>
        from existing in Answers.ExistingPath(path)
        from rows in TableOps.ReadRows(() => HatchPattern.ReadFromFile(existing, quiet: true), Row)
        select rows;

    public static IO<Unit> WriteFile(RhinoDoc doc, string path, Seq<int> indices) =>
        from qualified in Answers.QualifiedPath(path)
        from rows in indices.TraverseM(index => IO.lift(() => Missing.Unless(doc.HatchPatterns.FindIndex(index), nameof(HatchPatternTable.FindIndex)))).As()
        from written in IO.lift(() => Refused.Unless(HatchPattern.WriteToFile(qualified, rows), nameof(HatchPattern.WriteToFile)))
        select written;

    public static IO<Seq<Line>> Preview(RhinoDoc doc, int index, int width, int height, double angle) =>
        IO.lift(() => Missing.Unless(doc.HatchPatterns.FindIndex(index), nameof(HatchPatternTable.FindIndex))
            .Map(pattern => toSeq(pattern.CreatePreviewGeometry(width, height, angle))));
}
