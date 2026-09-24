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
    int BoundaryLinetypeIndex,
    int HatchIndex,
    double HatchScale,
    double HatchRotationRadians,
    Option<PlotWeight> HatchPatternPlotWeight,
    Color HatchPatternColor,
    Color HatchPatternPrintColor);

public sealed record SectionStyleRow(Guid Id, int Index, bool IsUnset, bool InUse, bool IsReference, bool IsDeleted, SectionStyleDefinition Definition);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
public static partial class SectionStyles {
    // --- [TABLE]
    internal static TableAccessors<SectionStyle> Accessors(RhinoDoc doc, bool reference) =>
        new(
            doc.SectionStyles,
            reference ? doc.SectionStyles.AddReferenceSectionStyle : doc.SectionStyles.Add,
            doc.SectionStyles.Modify,
            doc.SectionStyles.FindName,
            doc.SectionStyles.FindIndex);

    [MapPropertyFromSource(nameof(SectionStyleRow.Definition), Use = nameof(Definition))]
    public static partial SectionStyleRow Row(SectionStyle style);

    [MapProperty(nameof(SectionStyle.BoundaryPlotWeightMillimeters), nameof(SectionStyleDefinition.BoundaryPlotWeight), Use = nameof(@PlotWeight.FromHatchBoundary))]
    [MapProperty(nameof(SectionStyle.HatchPatternPlotWeightMillimeters), nameof(SectionStyleDefinition.HatchPatternPlotWeight), Use = nameof(@PlotWeight.FromHatchBoundary))]
    private static partial SectionStyleDefinition Definition(SectionStyle style);

    // --- [ROWS]
    public static IO<int> Add(RhinoDoc doc, SectionStyleDefinition definition, Option<Linetype> boundaryLinetype, bool reference) =>
        TableOps.AddRow(Accessors(doc, reference), IO.lift(static () => new SectionStyle()), staged => Written(staged, definition, boundaryLinetype));

    public static IO<Unit> Modify(RhinoDoc doc, int index, SectionStyleDefinition definition, Option<Linetype> boundaryLinetype, bool quiet) =>
        TableOps.ModifyRow(Accessors(doc, reference: false), index, static live => new SectionStyle(live), staged => Written(staged, definition, boundaryLinetype), quiet);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreSource(nameof(SectionStyleDefinition.Name), Justification = "Written under the table name lock")]
    [MapProperty(nameof(SectionStyleDefinition.BoundaryPlotWeight), nameof(SectionStyle.BoundaryPlotWeightMillimeters), Use = nameof(@PlotWeight.ToHatchBoundary))]
    [MapProperty(nameof(SectionStyleDefinition.HatchPatternPlotWeight), nameof(SectionStyle.HatchPatternPlotWeightMillimeters), Use = nameof(@PlotWeight.ToHatchBoundary))]
    private static partial void Update(SectionStyleDefinition definition, SectionStyle staged);

    private static IO<Unit> Written(SectionStyle staged, SectionStyleDefinition definition, Option<Linetype> boundary) =>
        from named in TableOps.Locked(IO.lift(() => definition.Name.Iter(name => staged.Name = name)), nameof(SectionStyle.Name))
        from settings in IO.lift(() => {
            Update(definition, staged);
            _ = boundary.Match(Some: staged.SetBoundaryLinetype, None: staged.RemoveBoundaryLinetype);
        })
        select unit;

    public static IO<Unit> Delete(RhinoDoc doc, Seq<int> indices, bool quiet) =>
        IO.lift(() => CountMismatch.Unless(indices.Count, doc.SectionStyles.Delete(indices, quiet), nameof(SectionStyleTable.Delete)));

    public static IO<(bool InUse, int InstanceDefinitions, int Objects, int Layers)> Usage(RhinoDoc doc, int index) =>
        IO.lift(() => {
            bool used = doc.SectionStyles.InUse(index, out int definitions, out int objects, out int layers);
            return (InUse: used, InstanceDefinitions: definitions, Objects: objects, Layers: layers);
        });

    public static IO<Option<LinetypeRow>> BoundaryLinetype(RhinoDoc doc, int index) =>
        from style in IO.lift(() => Missing.Unless(doc.SectionStyles.FindIndex(index), nameof(SectionStyleTable.FindIndex)))
        from row in Disposal.Bracketed(
            IO.lift(() => Optional(style.GetBoundaryLinetype())),
            static held => Disposal.Release(held.ToSeq()),
            static held => held.Traverse(Linetypes.Row).As())
        select row;

    public static IO<Option<SectionStyleRow>> Find(RhinoDoc doc, ComponentRef address) =>
        from found in TableOps.Find(Accessors(doc, reference: false), address)
        from row in IO.lift(() => found.Map(Row))
        select row;

    // --- [FILES]
    public static IO<(Seq<SectionStyleRow> Styles, Seq<HatchPatternRow> Patterns)> ReadFile(string path) =>
        WithFile(path, static read =>
            from styles in read.Styles.TraverseM(static style => IO.lift(() => Row(style))).As()
            from patterns in read.Patterns.TraverseM(HatchPatterns.Row).As()
            select (styles, patterns));

    public static IO<Seq<int>> Import(RhinoDoc doc, string path) =>
        WithFile(path, read => Commits.Commit(doc, LOC.STR("Import section styles"), new RedrawPolicy.Silent(), Imported(doc, read.Styles, read.Patterns)));

    private static IO<Seq<int>> Imported(RhinoDoc doc, Seq<SectionStyle> styles, Seq<HatchPattern> patterns) =>
        from indices in patterns.TraverseM(pattern => IO.lift(() =>
                (Optional(doc.HatchPatterns.FindName(pattern.Name)).Map(static found => found.Index) || Answers.Present(doc.HatchPatterns.Add(pattern)))
                    .ToFin(new Refused(nameof(HatchPatternTable.Add)))))
            .As()
        from added in styles.TraverseM(style =>
                from rebased in IO.lift(() => indices.At(style.HatchIndex).Iter(index => style.HatchIndex = index))
                from index in IO.lift(() => Answers.NonNegative(doc.SectionStyles.Add(style), nameof(SectionStyleTable.Add)))
                select index)
            .As()
        select added;

    private static IO<TValue> WithFile<TValue>(string path, Func<(Seq<SectionStyle> Styles, Seq<HatchPattern> Patterns), IO<TValue>> body) =>
        from existing in Answers.ExistingPath(path)
        from read in IO.lift(() => Refused.Unless(
            SectionStyle.ReadFromFile(existing, out SectionStyle[] styles, out HatchPattern[] patterns),
            (Styles: toSeq(styles), Patterns: toSeq(patterns)),
            nameof(SectionStyle.ReadFromFile)))
        from value in Disposal.Using(IO.pure(read.Styles.Map<IDisposable>(static style => style) + read.Patterns.Map<IDisposable>(static pattern => pattern)), _ => body(read))
        select value;
}
