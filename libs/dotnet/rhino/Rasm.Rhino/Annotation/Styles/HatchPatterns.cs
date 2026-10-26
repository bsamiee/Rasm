using System.Numerics;
using LanguageExt.ClassInstances;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Persistence;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Annotation.Styles;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record HatchLineDefinition(double Angle, Point2d BasePoint, Vector2d Offset, Seq<double> Dashes);

public sealed record HatchPatternDefinition(
    Option<string> Description,
    HatchPatternFillType FillType,
    UnitSystem PatternUnitSystem,
    bool AlwaysModelDistances,
    Seq<HatchLineDefinition> Lines,
    HashMap<EqStringOrdinalIgnoreCase, string, string> UserStrings);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class HatchPatternMapper {
    // --- [PROJECTIONS]
    internal static partial HatchPatternDefinition ToState(
        HatchPattern pattern, Seq<HatchLineDefinition> lines, HashMap<EqStringOrdinalIgnoreCase, string, string> userStrings);

    [MapProperty(nameof(HatchLine.GetDashes), nameof(HatchLineDefinition.Dashes), Use = nameof(@Conversions.Rows))]
    internal static partial HatchLineDefinition ToState(HatchLine line);

    // --- [UPDATES]
    [MapperRequiredMapping(RequiredMappingStrategy.Both)]
    [MapperIgnoreSource(nameof(HatchPatternDefinition.Lines), Justification = nameof(HatchPattern.SetHatchLines))]
    [MapperIgnoreSource(nameof(HatchPatternDefinition.UserStrings), Justification = nameof(UserStrings.Write))]
    [MapperIgnoreTarget(nameof(ModelComponent.Name), Justification = nameof(TableOps.Named))]
    [MapperIgnoreTarget(nameof(ModelComponent.Id), Justification = nameof(HatchPatternTable))]
    [MapperIgnoreTarget(nameof(ModelComponent.Index), Justification = nameof(HatchPatternTable))]
    [MapperIgnoreTarget(nameof(ModelComponent.ComponentStatus), Justification = nameof(HatchPatternTable))]
    internal static partial void Update(HatchPatternDefinition definition, HatchPattern staged);

    [MapperRequiredMapping(RequiredMappingStrategy.Both)]
    [MapperIgnoreSource(nameof(HatchLineDefinition.Dashes), Justification = nameof(HatchLine.SetDashes))]
    internal static partial void Update(HatchLineDefinition line, HatchLine created);
}

public static class HatchPatterns {
    // --- [DEFINITIONS]
    public static IO<HatchPatternDefinition> Definition(HatchPattern pattern) =>
        DisposalOps.Within((Action<HatchLine> held) =>
            from count in IO.lift(() => pattern.HatchLineCount)
            from lines in toSeq(Range(0, count)).TraverseM(index => DisposalOps.Acquire(IO.lift(() => pattern.HatchLineAt(index)), held)).As()
            from definition in IO.lift(() => HatchPatternMapper.ToState(
                pattern, lines.Map(HatchPatternMapper.ToState).Strict(), UserStrings.Held(pattern.GetUserStrings())))
            select definition, DisposalOps.Release);

    public static IO<bool> Unchanged(HatchPattern live, HatchPattern staged) =>
        (Definition(live), Definition(staged)).Apply(static (held, wanted) => held == wanted).As();

    public static IO<Unit> Written(HatchPattern staged, HatchPatternDefinition definition) =>
        DisposalOps.Within((Action<HatchLine> retain) =>
            from lines in definition.Lines.TraverseM(_ => DisposalOps.Acquire(IO.lift(static () => new HatchLine()), retain)).As()
            from settings in IO.lift(() => HatchPatternMapper.Update(definition, staged))
            from configured in IO.lift(() => lines.Zip(definition.Lines).Iter(static pair => {
                HatchPatternMapper.Update(pair.Second, pair.First);
                pair.First.SetDashes(pair.Second.Dashes);
            }))
            from lined in IO.lift(() => Refused.Unless(staged.SetHatchLines(lines) == lines.Count, nameof(HatchPattern.SetHatchLines)))
            from held in IO.lift(() => UserStrings.Held(staged.GetUserStrings()))
            from stored in UserStrings.Write(held, staged.SetUserString, UserStrings.Replacing(held, definition.UserStrings))
            select unit, DisposalOps.Release);

    public static IO<T> Detached<T>(Seq<(string Name, HatchPatternDefinition Definition)> rows, Func<Seq<HatchPattern>, IO<T>> body) =>
        DisposalOps.Within((Action<HatchPattern> held) =>
            from patterns in rows.TraverseM(_ => DisposalOps.Acquire(IO.lift(static () => new HatchPattern()), held)).As()
            from written in patterns.Zip(rows).TraverseM(static pair => from named in TableOps.Named(pair.First, Some(pair.Second.Name))
                                                                        from definition in Written(pair.First, pair.Second.Definition)
                                                                        select definition).As()
            from answer in body(patterns)
            select answer, DisposalOps.Release);

    // --- [PREVIEW]
    public static IO<Seq<(Vector2 From, Vector2 To)>> Preview(HatchPattern pattern, double angle, System.Drawing.Size pixels) =>
        IO.lift(() => toSeq(pattern.CreatePreviewGeometry(pixels.Width, pixels.Height, angle))
            .Map(static line => (From: new Vector2((float)line.FromX, (float)-line.FromY), To: new Vector2((float)line.ToX, (float)-line.ToY)))
            .Strict());

    // --- [DEFAULTS]
    public static IO<int> FromDefaults(RhinoDoc doc, string name) =>
        TableOps.Find(doc.HatchPatterns, name, includeDeleted: false)
            .Map(static live => live.Index)
            .Catch(static error => error.IsType<NotFound>(), error => DisposalOps.Within(HatchPattern.GetDefaultHatchPatterns,
                defaults =>
                    from held in IO.lift(() => Conversions.Rows(defaults).Find(row => TableOps.Names<HatchPattern>().Equals(row.Name, name)).ToFin(error))
                    from index in IO.lift(() => Conversions.Required(TableKinds.HatchPatterns.Add(doc, held), nameof(TableKind<>.Add)))
                    select index,
                static defaults => DisposalOps.Release(Conversions.Rows(defaults))));

    // --- [CURRENT]
    public static IO<Unit> SetCurrent(RhinoDoc doc, ComponentRef<HatchPattern> address) =>
        from live in TableOps.Find(doc.HatchPatterns, address, includeDeleted: false)
        from changed in when(doc.HatchPatterns.CurrentHatchPatternIndex != live.Index,
            IO.lift(() => { doc.HatchPatterns.CurrentHatchPatternIndex = live.Index; })).As()
        select changed;

    // --- [TRANSFER]
    public static IO<Seq<(string Name, HatchPatternDefinition Definition)>> Read(Option<InputPath> path = default) =>
        DisposalOps.Within(() => path.Match(
                Some: static location => HatchPattern.ReadFromFile(location, quiet: true),
                None: HatchPattern.GetDefaultHatchPatterns),
            static patterns => IO.lift(() => Refused.UnlessFound(patterns, nameof(HatchPattern.ReadFromFile)))
                .Bind(static held => Conversions.Rows(held).TraverseM(static pattern => Definition(pattern).Map(definition => (pattern.Name, Definition: definition))).As()),
            static patterns => DisposalOps.Release(Conversions.Rows(patterns)));

    public static IO<Unit> WriteFile(OutputPath path, Seq<(string Name, HatchPatternDefinition Definition)> rows) =>
        Detached(rows, patterns => IO.lift(() => Refused.Unless(HatchPattern.WriteToFile(path, patterns), nameof(HatchPattern.WriteToFile))));
}
