using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
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

public sealed record HatchPatternRow(Guid Id, int Index, bool InUse, bool IsReference, bool IsDeleted, HatchPatternDefinition Definition);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
public static partial class HatchPatterns {
    // --- [READS]
    public static IO<HatchPatternRow> Row(HatchPattern pattern) =>
        DisposalOps.Using(IO.lift(() => toSeq(pattern.HatchLines).Strict()), lines =>
            GeometryOps.ReadUserStrings(GeometryOps.UserStrings(pattern)).Map(userStrings => Project(pattern, lines.Map(Line).Strict(), userStrings)));

    [MapPropertyFromSource(nameof(HatchPatternRow.Definition), Use = nameof(Definition))]
    private static partial HatchPatternRow Project(HatchPattern pattern, Seq<HatchLineDefinition> lines, HashMap<string, string> userStrings);

    private static partial HatchPatternDefinition Definition(HatchPattern pattern, Seq<HatchLineDefinition> lines, HashMap<string, string> userStrings);

    [MapPropertyFromSource(nameof(HatchLineDefinition.Dashes), Use = nameof(Dashes))]
    private static partial HatchLineDefinition Line(HatchLine line);

    private static Seq<double> Dashes(HatchLine line) =>
        toSeq(line.GetDashes).Strict();

    // --- [WRITES]
    public static IO<Unit> Written(HatchPattern staged, HatchPatternDefinition definition) =>
        DisposalOps.Using(
            DisposalOps.AcquireAll(definition.Lines.Map(static line => IO.lift(() => {
                HatchLine created = ToLine(line);
                created.SetDashes(line.Dashes);
                return created;
            }))),
            lines =>
                from named in TableOps.Named(staged, definition.Name)
                from settings in IO.lift(() => Update(definition, staged))
                from lined in IO.lift(() => CountMismatch.Unless(lines.Count, staged.SetHatchLines(lines), nameof(HatchPattern.SetHatchLines)))
                from stored in Appearance.WriteUserStrings(GeometryOps.UserStrings(staged), definition.UserStrings)
                select unit);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(HatchPatternDefinition.Name), Justification = "TableOps.Named")]
    [MapperIgnoreSource(nameof(HatchPatternDefinition.Lines), Justification = "HatchPattern.SetHatchLines")]
    [MapperIgnoreSource(nameof(HatchPatternDefinition.UserStrings), Justification = "Appearance.WriteUserStrings")]
    private static partial void Update(HatchPatternDefinition definition, HatchPattern staged);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(HatchLineDefinition.Dashes), Justification = "HatchLine.SetDashes")]
    private static partial HatchLine ToLine(HatchLineDefinition line);

    public static IO<Unit> SetCurrent(RhinoDoc doc, int index) =>
        from live in TableOps.Row(doc.HatchPatterns, index)
        from current in IO.lift(() => { doc.HatchPatterns.CurrentHatchPatternIndex = live.Index; })
        select current;

    // --- [FILES]
    public static IO<Seq<HatchPatternRow>> Defaults() =>
        TableOps.ReadRows(HatchPattern.GetDefaultHatchPatterns, Row);

    public static IO<Seq<HatchPatternRow>> ReadFile(string path) =>
        from existing in Answers.ExistingPath(path)
        from rows in TableOps.ReadRows(() => HatchPattern.ReadFromFile(existing, quiet: true), Row)
        select rows;

    public static IO<Unit> WriteFile(RhinoDoc doc, string path, Seq<int> indices) =>
        from qualified in Answers.QualifiedPath(path)
        from rows in indices.TraverseM(index => TableOps.Row(doc.HatchPatterns, index)).As()
        from written in IO.lift(() => Refused.Unless(HatchPattern.WriteToFile(qualified, rows), nameof(HatchPattern.WriteToFile)))
        select written;
}
