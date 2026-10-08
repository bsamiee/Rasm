using LanguageExt.ClassInstances;
using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Document.Tables;
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
        IO.lift(() => Conversions.Rows(pattern.HatchLines)).Bracket(
            Use: lines => IO.lift(() => HatchPatternMapper.ToState(
                pattern, lines.Map(HatchPatternMapper.ToState).Strict(), UserStrings.Held(pattern.GetUserStrings()))),
            Fin: DisposalOps.Release);

    public static IO<bool> Unchanged(HatchPattern live, HatchPattern staged) =>
        (Definition(live), Definition(staged)).Apply(static (held, wanted) => held == wanted).As();

    public static IO<Unit> Written(HatchPattern staged, HatchPatternDefinition definition) =>
        DisposalOps.AcquireAll(definition.Lines.Map(static _ => IO.lift(static () => new HatchLine())), DisposalOps.Release).Bracket(
            Use: lines =>
                from settings in IO.lift(() => HatchPatternMapper.Update(definition, staged))
                from configured in IO.lift(() => lines.Zip(definition.Lines).Iter(static pair => {
                    HatchPatternMapper.Update(pair.Second, pair.First);
                    pair.First.SetDashes(pair.Second.Dashes);
                }))
                from lined in IO.lift(() => CountMismatch.Unless(lines.Count, staged.SetHatchLines(lines), nameof(HatchPattern.SetHatchLines)))
                from held in IO.lift(() => UserStrings.Held(staged.GetUserStrings()))
                from stored in UserStrings.Write(held, staged.SetUserString, UserStrings.Replacing(held, definition.UserStrings))
                select unit,
            Fin: DisposalOps.Release);

    public static IO<T> Detached<T>(Seq<(string Name, HatchPatternDefinition Definition)> rows, Func<Seq<HatchPattern>, IO<T>> body) =>
        DisposalOps.AcquireAll(rows.Map(static _ => IO.lift(static () => new HatchPattern())), DisposalOps.Release).Bracket(
            Use: patterns => from written in patterns.Zip(rows).TraverseM(static pair => from named in TableOps.Named(pair.First, Some(pair.Second.Name))
                                                                                         from definition in Written(pair.First, pair.Second.Definition)
                                                                                         select definition).As()
                             from answer in body(patterns)
                             select answer,
            Fin: DisposalOps.Release);

    // --- [DEFAULTS]
    public static IO<int> FromDefaults(RhinoDoc doc, string name) =>
        TableOps.Find(doc.HatchPatterns, name, includeDeleted: false)
            .Map(static live => live.Index)
            .Catch(static error => error.IsType<MissingComponent<HatchPattern>>(), error => IO.lift(static () => Conversions.Rows(HatchPattern.GetDefaultHatchPatterns())).Bracket(
                Use: defaults =>
                    from held in IO.lift(defaults.Find(row => TableOps.Names<HatchPattern>().Equals(row.Name, name)).ToFin(error))
                    from index in IO.lift(() => Conversions.Required(TableKinds.HatchPatterns.Add(doc, held), nameof(TableKind<>.Add)))
                    select index,
                Fin: DisposalOps.Release));

    // --- [CURRENT]
    public static IO<Unit> SetCurrent(RhinoDoc doc, ComponentRef<HatchPattern> address) =>
        from live in TableOps.Find(doc.HatchPatterns, address, includeDeleted: false)
        from changed in when(doc.HatchPatterns.CurrentHatchPatternIndex != live.Index,
            IO.lift(() => { doc.HatchPatterns.CurrentHatchPatternIndex = live.Index; })).As()
        select changed;

    // --- [TRANSFER]
    public static IO<Seq<(string Name, HatchPatternDefinition Definition)>> Read(Option<string> path = default) =>
        from existing in IO.lift(() => path.Traverse(Exchange.ExistingPath).As())
        from definitions in IO.lift(() => existing.Match(
                Some: static location => Missing.Unless(HatchPattern.ReadFromFile(location, quiet: true), nameof(HatchPattern.ReadFromFile)),
                None: static () => Fin.Succ(HatchPattern.GetDefaultHatchPatterns())))
            .Map(Conversions.Rows)
            .Bracket(
                Use: static patterns => patterns.TraverseM(static pattern => Definition(pattern).Map(definition => (pattern.Name, Definition: definition))).As(),
                Fin: DisposalOps.Release)
        select definitions;

    public static IO<Unit> WriteFile(string path, Seq<(string Name, HatchPatternDefinition Definition)> rows) =>
        from qualified in IO.lift(Exchange.QualifiedPath(path))
        from written in Detached(rows, patterns => IO.lift(() => Refused.Unless(HatchPattern.WriteToFile(qualified, patterns), nameof(HatchPattern.WriteToFile))))
        select written;
}
