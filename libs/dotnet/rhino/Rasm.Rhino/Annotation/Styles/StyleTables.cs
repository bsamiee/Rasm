using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.TopologicalSort;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Annotation.Styles;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record StyleSpec(
    TableSpec<HatchPatternDefinition> HatchPatterns,
    TableSpec<LinetypeDefinition> Linetypes,
    TableSpec<SectionStyleDefinition> SectionStyles,
    TableSpec<DimensionStyleSpec> DimensionStyles,
    Option<ComponentRef<HatchPattern>> CurrentHatchPattern,
    Option<LinetypeTableState> LinetypeTable,
    Option<ComponentRef<DimensionStyle>> CurrentDimensionStyle);

public sealed record StylePlan(
    TablePlan<HatchPattern, HatchPatternDefinition> HatchPatterns,
    TablePlan<Linetype, LinetypeDefinition> Linetypes,
    TablePlan<SectionStyle, SectionStyleDefinition> SectionStyles,
    TablePlan<DimensionStyle, DimensionStyleSpec> DimensionStyles);

public sealed record StyleRows(
    Seq<(string Name, int Index)> HatchPatterns,
    Seq<(string Name, int Index)> Linetypes,
    Seq<(string Name, int Index)> SectionStyles,
    Seq<(string Name, int Index)> DimensionStyles);

public sealed record ReconciledStyles(StyleRows Written, LanguageExt.HashSet<ComponentIdentity> Removed);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class StyleTables {
    // --- [ORDER]
    private static Seq<TableUpsert<DimensionStyle, DimensionStyleSpec>> WriteOrder(Seq<TableUpsert<DimensionStyle, DimensionStyleSpec>> upserts) =>
        (from dependent in upserts
         from address in dependent.Spec.Parent.ToSeq() + dependent.Spec.Source.Bind(static source => source.Switch(
             builtIn: static _ => Option<ComponentRef<DimensionStyle>>.None,
             row: static row => Some(row.Address))).ToSeq()
         from dependency in upserts
         where dependency != dependent && address.Switch(
             dependency,
             byId: static (row, byId) => row.Existing.Exists(live => live.Id == byId.Id),
             byIndex: static (row, byIndex) => row.Existing.Exists(live => live.Index == byIndex.Index),
             byName: static (row, byName) => TableOps.Names<DimensionStyle>().Equals(row.Name, byName.Name))
         select new SEquatableEdge<TableUpsert<DimensionStyle, DimensionStyleSpec>>(dependency, dependent))
        .ToLookup(static edge => edge.Source) switch {
            var edges => toSeq(upserts.ToBidirectionalGraph(row => edges[row], allowParallelEdges: false)
                .SourceFirstBidirectionalTopologicalSort(TopologicalSortDirection.Forward)),
        };

    // --- [GRAPH]
    public static IO<ArrayBidirectionalGraph<ComponentIdentity, SEquatableEdge<ComponentIdentity>>> Graph(RhinoDoc doc) =>
        from rows in IO.lift(() => Seq<IEnumerable<ModelComponent>>(doc.HatchPatterns, doc.Linetypes, doc.SectionStyles, doc.DimStyles)
            .Bind(static table => Conversions.Rows(table)).Filter(static row => !row.IsDeleted).Strict())
        let edges = (from style in rows.OfType<SectionStyle>()
                     from reference in Seq((Type: ModelComponentType.HatchPattern, Index: style.HatchIndex), (Type: ModelComponentType.LinePattern, Index: style.BoundaryLinetypeIndex))
                     join dependency in rows on reference equals (dependency.ComponentType, dependency.Index)
                     select Edge(dependency, style))
            .Concat(from style in rows.OfType<DimensionStyle>()
                    join parent in rows on new ComponentIdentity(style.ParentId, ModelComponentType.DimStyle) equals new ComponentIdentity(parent.Id, parent.ComponentType)
                    select Edge(parent, style))
            .ToLookup(static edge => edge.Source)
        select rows.Map(static row => new ComponentIdentity(row.Id, row.ComponentType))
            .ToBidirectionalGraph(row => edges[row], allowParallelEdges: false).ToArrayBidirectionalGraph();

    private static SEquatableEdge<ComponentIdentity> Edge(ModelComponent dependency, ModelComponent dependent) =>
        new(new ComponentIdentity(dependency.Id, dependency.ComponentType), new ComponentIdentity(dependent.Id, dependent.ComponentType));

    // --- [PRUNE]
    private static Seq<(ComponentIdentity Row, IO<bool> Delete)> Candidates<T>(RhinoDocCommonTable<T> table, Seq<T> rows, Func<T, bool> held) where T : ModelComponent =>
        rows.Map(row => (
            new ComponentIdentity(row.Id, row.ComponentType),
            IO.lift(() => held(row) ? Fin.Succ(value: false) : RefusedElement.Unless(table.Delete(row), value: true, nameof(RhinoDocCommonTable<>.Delete), row.Index))));

    private static IO<LanguageExt.HashSet<ComponentIdentity>> Pruned(ArrayBidirectionalGraph<ComponentIdentity, SEquatableEdge<ComponentIdentity>> graph, Seq<(ComponentIdentity Row, IO<bool> Delete)> candidates) =>
        toHashMap(candidates.Map(static candidate => (candidate.Row, candidate))) switch {
            var byRow => toSeq(graph.SourceFirstBidirectionalTopologicalSort(TopologicalSortDirection.Backward))
                .Choose(byRow.Find)
                .FoldBackM(LanguageExt.HashSet<ComponentIdentity>.Empty, (removed, candidate) =>
                    graph.OutEdges(candidate.Row).All(edge => removed.Contains(edge.Target))
                        ? candidate.Delete.Map(deleted => deleted ? removed.Add(candidate.Row) : removed)
                        : IO.pure(removed))
                .As(),
        };

    // --- [PROGRAM]
    public static IO<StylePlan> Plan(RhinoDoc doc, StyleSpec spec) =>
        from rows in ApplicativeExtensions.Apply(
            (TableOps.Rows(doc.HatchPatterns), TableOps.Rows(doc.Linetypes), TableOps.Rows(doc.SectionStyles), TableOps.Rows(doc.DimStyles)),
            static (hatchPatterns, linetypes, sectionStyles, dimensionStyles) => (hatchPatterns, linetypes, sectionStyles, dimensionStyles))
            .As()
        from plan in IO.lift(
            (TableOps.Plan(rows.hatchPatterns, spec.HatchPatterns), TableOps.Plan(rows.linetypes, spec.Linetypes),
                TableOps.Plan(rows.sectionStyles, spec.SectionStyles), TableOps.Plan(rows.dimensionStyles, spec.DimensionStyles))
            .Apply(static (hatchPlan, linetypePlan, sectionPlan, dimensionPlan) =>
                new StylePlan(hatchPlan, linetypePlan, sectionPlan, dimensionPlan with { Upserts = WriteOrder(dimensionPlan.Upserts) }))
            .As()
            .ToFin())
        select plan;

    public static IO<Committed<ReconciledStyles>> Reconcile(RhinoDoc doc, StyleSpec spec, Func<StyleRows, IO<Unit>> reassign, string name, RedrawPolicy redraw, IPlugInSink sink) =>
        from plan in Plan(doc, spec)
        from committed in Commits.Commit(doc, RowText.Localize(name, table: Some<object>(sink)).Local, redraw,
            from hatchPatterns in TableOps.Upsert(doc, TableKinds.HatchPatterns, plan.HatchPatterns.Upserts, HatchPatterns.Written, HatchPatterns.Unchanged)
            from linetypes in TableOps.Upsert(doc, TableKinds.Linetypes, plan.Linetypes.Upserts, Linetypes.Written, Linetypes.Unchanged)
            from sectionStyles in TableOps.Upsert(doc, TableKinds.SectionStyles, plan.SectionStyles.Upserts, (staged, definition) => SectionStyles.Written(doc, staged, definition), SectionStyles.Unchanged)
            from dimensionStyles in TableOps.Upsert(doc, TableKinds.DimensionStyles, plan.DimensionStyles.Upserts, (staged, row) => DimensionStyles.Written(doc, staged, row), DimensionStyles.Unchanged)
            let written = new StyleRows(hatchPatterns, linetypes, sectionStyles, dimensionStyles)
            from hatchCurrent in spec.CurrentHatchPattern.Traverse(address => HatchPatterns.SetCurrent(doc, address)).As()
            from linetypeTable in spec.LinetypeTable.Traverse(table => Linetypes.WriteTable(doc, table)).As()
            from dimensionCurrent in spec.CurrentDimensionStyle.Traverse(address => DimensionStyles.SetCurrent(doc, address)).As()
            from reassigned in reassign(written)
            from graph in Graph(doc)
            from removed in Pruned(
                graph,
                Candidates(doc.HatchPatterns, plan.HatchPatterns.Prunes, row => row.InUse || row.Index == doc.HatchPatterns.CurrentHatchPatternIndex)
                + Candidates(doc.Linetypes, plan.Linetypes.Prunes, row => row.InUse || row.Index == doc.Linetypes.CurrentLinetypeIndex)
                + Candidates(doc.SectionStyles, plan.SectionStyles.Prunes, static row => row.InUse)
                + Candidates(doc.DimStyles, plan.DimensionStyles.Prunes, row => row.Index == doc.DimStyles.CurrentIndex))
            select new ReconciledStyles(written, removed))
        select committed;
}
