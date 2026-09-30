using System.Drawing;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.UI;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Annotation;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SectionStyleDefinition(
    Option<string> Name,
    ObjectSectionFillRule SectionFillRule,
    SectionBackgroundFillMode BackgroundFillMode,
    Color BackgroundFillColor,
    Color BackgroundFillPrintColor,
    bool BoundaryVisible,
    Color BoundaryColor,
    Color BoundaryPrintColor,
    double BoundaryWidthScale,
    Option<PlotWeight> BoundaryPlotWeight,
    Option<LinetypeRef> BoundaryLinetype,
    Option<ComponentRef> Hatch,
    double HatchScale,
    double HatchRotationRadians,
    Option<PlotWeight> HatchPatternPlotWeight,
    Color HatchPatternColor,
    Color HatchPatternPrintColor);

public sealed record SectionStyleRow(Guid Id, int Index, bool IsUnset, bool InUse, bool IsReference, bool IsDeleted, SectionStyleDefinition Definition);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
public static partial class SectionStyles {
    // --- [READS]
    [MapPropertyFromSource(nameof(SectionStyleRow.Definition), Use = nameof(Definition))]
    public static partial SectionStyleRow Row(SectionStyle style);

    [MapProperty(nameof(SectionStyle.BoundaryPlotWeightMillimeters), nameof(SectionStyleDefinition.BoundaryPlotWeight), Use = nameof(@PlotWeight.FromInheritable))]
    [MapProperty(nameof(SectionStyle.HatchPatternPlotWeightMillimeters), nameof(SectionStyleDefinition.HatchPatternPlotWeight), Use = nameof(@PlotWeight.FromInheritable))]
    [MapProperty(nameof(SectionStyle.BoundaryLinetypeIndex), nameof(SectionStyleDefinition.BoundaryLinetype), Use = nameof(@LinetypeRef.FromHost))]
    [MapProperty(nameof(SectionStyle.HatchIndex), nameof(SectionStyleDefinition.Hatch), Use = nameof(HatchOf))]
    private static partial SectionStyleDefinition Definition(SectionStyle style);

    private static Option<ComponentRef> HatchOf(int index) =>
        ComponentRef.ByIndex.From(index).ToOption();

    public static IO<(bool InUse, int InstanceDefinitions, int Objects, int Layers)> Usage(RhinoDoc doc, int index) =>
        IO.lift(() => {
            bool used = doc.SectionStyles.InUse(index, out int definitions, out int objects, out int layers);
            return (InUse: used, InstanceDefinitions: definitions, Objects: objects, Layers: layers);
        });

    public static IO<Option<LinetypeRow>> BoundaryLinetype(RhinoDoc doc, int index) =>
        from style in TableOps.Row(doc.SectionStyles, index)
        from row in IO.lift(() => Optional(style.GetBoundaryLinetype())).Bracket(
            Use: static held => held.Traverse(Linetypes.Row).As(),
            Fin: static held => DisposalOps.Release(held.ToSeq()))
        select row;

    // --- [WRITES]
    public static IO<Unit> Written(RhinoDoc doc, SectionStyle staged, SectionStyleDefinition definition) =>
        from hatch in definition.Hatch.Traverse(address => TableOps.Find(doc.HatchPatterns, address, includeDeleted: false).Map(static found => found.Index)).As()
        from linetype in definition.BoundaryLinetype.Traverse(address => address.Resolve(doc)).As()
        from named in TableOps.Named(staged, definition.Name)
        from settings in IO.lift(() => {
            Update(definition, staged);
            staged.HatchIndex = hatch.IfNone(RhinoMath.UnsetIntIndex);
            staged.BoundaryLinetypeIndex = linetype.IfNone(RhinoMath.UnsetIntIndex);
        })
        select unit;

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(SectionStyleDefinition.Name), Justification = "TableOps.Named")]
    [MapperIgnoreSource(nameof(SectionStyleDefinition.BoundaryLinetype), Justification = "LinetypeRef.Resolve")]
    [MapperIgnoreSource(nameof(SectionStyleDefinition.Hatch), Justification = "TableOps.Find")]
    [MapProperty(nameof(SectionStyleDefinition.BoundaryPlotWeight), nameof(SectionStyle.BoundaryPlotWeightMillimeters), Use = nameof(@PlotWeight.ToInheritable))]
    [MapProperty(nameof(SectionStyleDefinition.HatchPatternPlotWeight), nameof(SectionStyle.HatchPatternPlotWeightMillimeters), Use = nameof(@PlotWeight.ToInheritable))]
    private static partial void Update(SectionStyleDefinition definition, SectionStyle staged);

    // --- [FILES]
    public static IO<(Seq<SectionStyleRow> Styles, Seq<HatchPatternRow> Patterns)> ReadFile(string path) =>
        WithFile(path, static read =>
            from styles in IO.lift(() => read.Styles.Map(Row).Strict())
            from patterns in read.Patterns.TraverseM(HatchPatterns.Row).As()
            select (styles, patterns));

    public static IO<Seq<int>> Import(RhinoDoc doc, string path) =>
        WithFile(path, read => Commits.Commit(doc, LOC.STR("Import section styles"), new RedrawPolicy.Silent(), Imported(doc, read.Styles, read.Patterns)));

    private static IO<Seq<int>> Imported(RhinoDoc doc, Seq<SectionStyle> styles, Seq<HatchPattern> patterns) =>
        from indices in patterns.TraverseM(pattern => IO.lift(() =>
                (Optional(doc.HatchPatterns.FindName(pattern.Name)?.Index) || Answers.Present(doc.HatchPatterns.Add(pattern)))
                    .ToFin(new Refused(nameof(HatchPatternTable.Add)))))
            .As()
        from added in styles.TraverseM(style =>
                from rebased in IO.lift(() => Answers.Present(style.HatchIndex).Traverse(local => indices.At(local).ToFin(new InvalidAnswer(nameof(SectionStyle.ReadFromFile)))).As())
                from written in IO.lift(() => { style.HatchIndex = rebased.IfNone(style.HatchIndex); })
                from index in IO.lift(() => Answers.Required(doc.SectionStyles.Add(style), nameof(SectionStyleTable.Add)))
                select index)
            .As()
        select added;

    private static IO<TValue> WithFile<TValue>(string path, Func<(Seq<SectionStyle> Styles, Seq<HatchPattern> Patterns), IO<TValue>> body) =>
        from existing in Answers.ExistingPath(path)
        from value in IO.lift(() => Refused.Unless(
                SectionStyle.ReadFromFile(existing, out SectionStyle[] styles, out HatchPattern[] patterns),
                (Styles: toSeq(styles), Patterns: toSeq(patterns)),
                nameof(SectionStyle.ReadFromFile)))
            .Bracket(
                Use: body,
                Fin: static read => DisposalOps.Release<IDisposable>([.. read.Styles, .. read.Patterns]))
        select value;
}
