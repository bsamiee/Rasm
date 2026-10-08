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
        toHashMap(upserts.Map(static upsert => (upsert.Name, upsert))) switch {
            var rows => toSeq(upserts
                    .Map(static upsert => upsert.Name)
                    .ToBidirectionalGraph<string, SEquatableEdge<string>>(
                        name => upserts
                            .Filter(dependent => dependent.Name != name && Addresses(dependent.Spec).Exists(address => Names(rows[name], address)))
                            .Map(dependent => new SEquatableEdge<string>(name, dependent.Name)),
                        allowParallelEdges: false)
                    .SourceFirstBidirectionalTopologicalSort(TopologicalSortDirection.Forward))
                .Map(name => rows[name])
                .Strict(),
        };

    private static Seq<ComponentRef<DimensionStyle>> Addresses(DimensionStyleSpec spec) =>
        spec.Parent.ToSeq()
        + spec.Source.Bind(static source => source.Switch(builtIn: static _ => Option<ComponentRef<DimensionStyle>>.None, row: static row => Some(row.Address))).ToSeq();

    private static bool Names(TableUpsert<DimensionStyle, DimensionStyleSpec> row, ComponentRef<DimensionStyle> address) =>
        address.Switch(
            (row.Name, Id: row.Existing.Map(static live => live.Id), Index: row.Existing.Map(static live => live.Index)),
            byId: static (held, byId) => held.Id == Some(byId.Id),
            byIndex: static (held, byIndex) => held.Index == Some(byIndex.Index),
            byName: static (held, byName) => TableOps.Names<DimensionStyle>().Equals(held.Name, byName.Name));

    // --- [GRAPH]
    public static IO<ArrayBidirectionalGraph<ComponentIdentity, SEquatableEdge<ComponentIdentity>>> Graph(RhinoDoc doc) =>
        IO.lift(() => (
                    from style in toSeq(doc.SectionStyles)
                    where !style.IsDeleted
                    from reference in Seq((Type: ModelComponentType.HatchPattern, Index: style.HatchIndex), (Type: ModelComponentType.LinePattern, Index: style.BoundaryLinetypeIndex))
                    from index in Conversions.Present(reference.Index).ToSeq()
                    from held in Optional(doc.Manifest.FindIndex(index, reference.Type)).ToSeq()
                    where !held.IsDeleted
                    select Edge(held, style))
                .Concat(
                    from style in toSeq(doc.DimStyles)
                    where !style.IsDeleted
                    from id in Conversions.Present(style.ParentId).ToSeq()
                    from parent in Optional(doc.Manifest.FindId(id, ModelComponentType.DimStyle)).ToSeq()
                    select Edge(parent, style))
                .ToLookup(static edge => edge.Source) switch {
                    var outEdges => Seq<IEnumerable<ModelComponent>>(doc.HatchPatterns, doc.Linetypes, doc.SectionStyles, doc.DimStyles)
                        .Bind(static table => toSeq(table))
                        .Filter(static row => !row.IsDeleted)
                        .Map(static row => new ComponentIdentity(row.Id, row.ComponentType))
                        .ToBidirectionalGraph(row => outEdges[row], allowParallelEdges: false)
                        .ToArrayBidirectionalGraph(),
                });

    private static SEquatableEdge<ComponentIdentity> Edge(ModelComponent dependency, ModelComponent dependent) =>
        new(new ComponentIdentity(dependency.Id, dependency.ComponentType), new ComponentIdentity(dependent.Id, dependent.ComponentType));

    // --- [PRUNE]
    private sealed record Candidate(ComponentIdentity Row, IO<bool> Held, IO<Unit> Delete);

    private static Seq<Candidate> Candidates<T>(RhinoDocCommonTable<T> table, Seq<T> rows, Func<T, bool> held) where T : ModelComponent =>
        rows.Map(row => new Candidate(
            new ComponentIdentity(row.Id, row.ComponentType),
            IO.lift(() => held(row)),
            IO.lift(() => RefusedElement.Unless(table.Delete(row), nameof(RhinoDocCommonTable<>.Delete), row.Index))));

    private static IO<LanguageExt.HashSet<ComponentIdentity>> Pruned(ArrayBidirectionalGraph<ComponentIdentity, SEquatableEdge<ComponentIdentity>> graph, Seq<Candidate> candidates) =>
        toHashMap(candidates.Map(static candidate => (candidate.Row, candidate))) switch {
            var byRow => toSeq(graph.SourceFirstBidirectionalTopologicalSort(TopologicalSortDirection.Backward))
                .Choose(row => byRow.Find(row))
                .FoldBackM(LanguageExt.HashSet<ComponentIdentity>.Empty, (removed, candidate) =>
                    from held in toSeq(graph.OutEdges(candidate.Row)).ForAll(edge => removed.Contains(edge.Target)) ? candidate.Held : IO.pure(true)
                    from deleted in unless(held, candidate.Delete).As()
                    select held ? removed : removed.Add(candidate.Row))
                .As(),
        };

    // --- [PROGRAM]
    public static IO<StylePlan> Plan(RhinoDoc doc, StyleSpec spec) =>
        from rows in (TableOps.Rows(doc.HatchPatterns), TableOps.Rows(doc.Linetypes), TableOps.Rows(doc.SectionStyles), TableOps.Rows(doc.DimStyles))
            .Apply(static (hatchPatterns, linetypes, sectionStyles, dimensionStyles) => (hatchPatterns, linetypes, sectionStyles, dimensionStyles))
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
        Plan(doc, spec).Bind(plan => Commits.Commit(doc, RowText.Localize(name, table: Some<object>(sink)), redraw,
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
            select new ReconciledStyles(written, removed)));
}
