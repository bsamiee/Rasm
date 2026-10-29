using System.Collections.Specialized;
using System.Globalization;
using LanguageExt.ClassInstances;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.TopologicalSort;
using Rasm.Drafting;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Persistence;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Annotation.Styles;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record StyleSpec {
    public Seq<(string Name, Func<HatchPattern> Spec)> HatchPatterns { get; init; }
    public Seq<(string Name, LinetypeDefinition Spec)> Linetypes { get; init; }
    public Seq<(string Name, SectionStyleDefinition Spec)> SectionStyles { get; init; }
    public Seq<(string Name, DimensionStyleSpec Spec)> DimensionStyles { get; init; }
    public Prune Prune { get; init; } = Prune.Keep;
    public Option<ComponentRef<HatchPattern>> CurrentHatchPattern { get; init; }
    public Option<LinetypeTableState> LinetypeTable { get; init; }
    public Option<ComponentRef<DimensionStyle>> CurrentDimensionStyle { get; init; }

    public static StyleSpec From(SheetSize size, LineGroup lines, Seq<DrawingScale> scales) =>
        (Narrow: lines.Width(LineWeight.Narrow), Hatching: lines.Width(LineRole.Hatching.Weight), Cut: lines.Width(LineRole.Cut.Weight)) switch {
            var widths => new() {
                HatchPatterns = [(StyleTables.Sized(HatchingName, widths.Hatching), () => Styles.HatchPatterns.Hatching(lines))],
                Linetypes = toSeq(LineType.Items).Map(type => (Styles.Linetypes.Name(type, widths.Narrow), LinetypeDefinition.Of(type, widths.Narrow, LineCap.Butt))),
                SectionStyles = [(StyleTables.Sized("ISO 128 section", widths.Cut), new SectionStyleDefinition(
                    ObjectSectionFillRule.ClosedCurves,
                    new SectionFill(SectionBackgroundFillMode.Viewport),
                    Some(new SectionBoundary(1d, Some(PlotWeight.Of(widths.Cut)), None)),
                    Some(new SectionHatch(new ComponentRef<HatchPattern>.ByName(StyleTables.Sized(HatchingName, widths.Hatching)), 1d, UnitsNet.Angle.Zero, Some(PlotWeight.Of(widths.Hatching))))))],
                DimensionStyles = scales.Map(scale => (Scaled(size, scale), new DimensionStyleSpec {
                    Lettering = new StyleMetrics(size, size.Convention.Lettering.Form, size.Convention.Terminator, scale),
                })),
                CurrentDimensionStyle = scales.Head.Map<ComponentRef<DimensionStyle>>(scale => new ComponentRef<DimensionStyle>.ByName(Scaled(size, scale))),
            },
        };

    private const string HatchingName = "ISO 128-50";

    private static string Scaled(SheetSize size, DrawingScale scale) => $"{size.Key} {scale}";
}

public sealed record StylePlan(
    Seq<RowPlan<HatchPattern, (string Name, Func<HatchPattern> Spec)>> HatchPatterns,
    Seq<RowPlan<Linetype, (string Name, LinetypeDefinition Spec)>> Linetypes,
    Seq<RowPlan<SectionStyle, (string Name, SectionStyleDefinition Spec)>> SectionStyles,
    Seq<RowPlan<DimensionStyle, (string Name, DimensionStyleSpec Spec)>> DimensionStyles);

public sealed record StyleRows(
    Seq<(string Name, int Index)> HatchPatterns,
    Seq<(string Name, int Index)> Linetypes,
    Seq<(string Name, int Index)> SectionStyles,
    Seq<(string Name, int Index)> DimensionStyles);

public sealed record ReconciledStyles(StyleRows Written, HashSet<ComponentIdentity> Removed);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class StyleTables {
    // --- [NAMES]
    internal static string Sized(string stem, LineWidth width) => string.Create(CultureInfo.InvariantCulture, $"{stem} {width.Width.Millimeters.ToDouble():0.00}");

    // --- [STRINGS]
    internal static IO<Unit> Strung(Func<NameValueCollection> read, Func<string, string, bool> write, HashMap<EqStringOrdinalIgnoreCase, string, string> wanted) =>
        IO.lift(() => UserStrings.Of(read())).Bind(held => UserStrings.Write(held, write, UserStrings.Replacing(held, wanted)));

    // --- [ORDER]
    private static Seq<RowPlan<DimensionStyle, (string Name, DimensionStyleSpec Spec)>> WriteOrder(Seq<RowPlan<DimensionStyle, (string Name, DimensionStyleSpec Spec)>> plan) =>
        (from dependent in plan
         from spec in Spec(dependent).ToSeq()
         from address in spec.Spec.Source.Bind(static source => source.Switch(
             builtIn: static _ => Option<ComponentRef<DimensionStyle>>.None,
             row: static row => Some(row.Address))).ToSeq()
         from dependency in plan
         where dependency != dependent && address.Switch(
             dependency,
             byId: static (step, byId) => Row(step).Exists(live => live.Id == byId.Id),
             byIndex: static (step, byIndex) => Row(step).Exists(live => live.Index == byIndex.Index),
             byName: static (step, byName) => Spec(step).Exists(held => Rows.Names<DimensionStyle>().Equals(held.Name, byName.Name)))
         select new SEquatableEdge<RowPlan<DimensionStyle, (string Name, DimensionStyleSpec Spec)>>(dependency, dependent))
        .ToLookup(static edge => edge.Source) switch {
            var edges => toSeq(plan.ToBidirectionalGraph(step => edges[step], allowParallelEdges: false).SourceFirstBidirectionalTopologicalSort()),
        };

    private static Option<TSpec> Spec<TRow, TSpec>(RowPlan<TRow, TSpec> step) =>
        step.Switch(create: static create => Some(create.Spec), change: static change => Some(change.Spec), remove: static _ => None, order: static _ => None);

    private static Option<TRow> Row<TRow, TSpec>(RowPlan<TRow, TSpec> step) =>
        step.Switch(create: static _ => None, change: static change => Some(change.Row), remove: static remove => Some(remove.Row), order: static _ => None);

    // --- [GRAPH]
    public static IO<ArrayBidirectionalGraph<ComponentIdentity, SEquatableEdge<ComponentIdentity>>> Graph(RhinoDoc doc) =>
        from rows in IO.lift(() => (Conversions.Rows<ModelComponent>(doc.HatchPatterns) + Conversions.Rows<ModelComponent>(doc.Linetypes)
                + Conversions.Rows<ModelComponent>(doc.SectionStyles) + Conversions.Rows<ModelComponent>(doc.DimStyles))
            .Filter(static row => !row.IsDeleted).Strict())
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
    private static IO<HashSet<Guid>> DimensionStylesInUse(RhinoDoc doc) =>
        from users in IO.lift(() => Conversions.Rows(doc.Objects.GetObjectList(new ObjectEnumeratorSettings {
            ObjectTypeFilter = ObjectType.Annotation | ObjectType.ClipPlane, HiddenObjects = true, ReferenceObjects = true, IdefObjects = true,
        })))
        select toHashSet(users.OfType<AnnotationObjectBase>().Select(static user => user.AnnotationGeometry.DimensionStyleId)
            .Concat(users.OfType<ClippingPlaneObject>().Select(static user => user.ClippingPlaneGeometry.DimensionStyleId)));

    private static Seq<(ComponentIdentity Row, IO<bool> Delete)> Candidates<T, TSpec>(RhinoDocCommonTable<T> table, Seq<RowPlan<T, TSpec>> plan, Func<T, bool> held)
        where T : ModelComponent =>
        plan.Map(static step => step.Switch(create: static _ => None, change: static _ => None, remove: static remove => Some(remove.Row), order: static _ => None)).Somes().Map(row => (
            new ComponentIdentity(row.Id, row.ComponentType),
            IO.lift(() => held(row)).Bind(kept => kept ? IO.pure(value: false) : Products.Accepted(() => table.Delete(row)).Map(static _ => true))));

    private static IO<HashSet<ComponentIdentity>> Pruned(ArrayBidirectionalGraph<ComponentIdentity, SEquatableEdge<ComponentIdentity>> graph, Seq<(ComponentIdentity Row, IO<bool> Delete)> candidates) =>
        toSeq(graph.SourceFirstBidirectionalTopologicalSort(TopologicalSortDirection.Backward))
            .Choose(toHashMap(candidates.Map(static candidate => (candidate.Row, candidate))).Find)
            .FoldM(HashSet<ComponentIdentity>(), (removed, candidate) =>
                graph.OutEdges(candidate.Row).All(edge => removed.Contains(edge.Target))
                    ? candidate.Delete.Map(deleted => deleted ? removed.Add(candidate.Row) : removed)
                    : IO.pure(removed))
            .As();

    // --- [WRITES]
    private static IO<Seq<(string Name, int Index)>> Applied<T, TSpec>(
        RhinoDoc doc, TableKind<T> kind, Seq<RowPlan<T, (string Name, TSpec Spec)>> plan, Func<Option<T>, TSpec, IO<T>> staged, Func<T, T, IO<bool>> unchanged)
        where T : ModelComponent =>
        plan.TraverseM(step => step.Switch(
                (Doc: doc, Kind: kind, Staged: staged, Unchanged: unchanged),
                create: static (state, create) =>
                    (from row in state.Staged(None, create.Spec.Spec)
                     from named in IO.lift(() => { row.Name = create.Spec.Name; })
                     from index in IO.lift(() => Conversions.Required(Conversions.Present(state.Kind.Add(state.Doc, row))))
                     select Some((create.Spec.Name, index))).Bracket(),
                change: static (state, change) =>
                    (from row in state.Staged(Some(change.Row), change.Spec.Spec)
                     from named in IO.lift(() => { row.Name = change.Spec.Name; })
                     from same in state.Unchanged(change.Row, row)
                     from modified in unless(same && string.Equals(change.Row.Name, change.Spec.Name, StringComparison.Ordinal),
                         Products.Accepted(() => state.Kind.Modify(state.Doc, row, change.Row.Index))).As()
                     select Some((change.Spec.Name, change.Row.Index))).Bracket(),
                remove: static (_, _) => IO.pure(Option<(string, int)>.None),
                order: static (_, _) => IO.pure(Option<(string, int)>.None)))
            .As()
            .Map(static written => written.Somes().Strict());

    private static Func<Option<T>, TSpec, IO<T>> Written<T, TSpec>(TableKind<T> kind, Func<T, TSpec, IO<Unit>> write) where T : ModelComponent =>
        (live, spec) =>
            from row in use(() => live.Match(Some: kind.Copy, None: kind.Fresh))
            from written in write(row, spec)
            select row;

    // --- [PROGRAM]
    public static IO<StylePlan> Plan(RhinoDoc doc, StyleSpec spec) =>
        IO.lift(() =>
            (Planned(TableKinds.HatchPatterns.Live(doc), spec.HatchPatterns, spec.Prune), Planned(TableKinds.Linetypes.Live(doc), spec.Linetypes, spec.Prune),
                Planned(TableKinds.SectionStyles.Live(doc), spec.SectionStyles, spec.Prune), Planned(TableKinds.DimensionStyles.Live(doc), spec.DimensionStyles, spec.Prune))
            .Apply(static (hatchPatterns, linetypes, sectionStyles, dimensionStyles) => new StylePlan(hatchPatterns, linetypes, sectionStyles, WriteOrder(dimensionStyles)))
            .As());

    private static Fin<Seq<RowPlan<T, (string Name, TSpec Spec)>>> Planned<T, TSpec>(Seq<T> rows, Seq<(string Name, TSpec Spec)> specs, Prune prune) where T : ModelComponent =>
        Rows.Plan(rows.Filter(static row => !row.IsReference), specs, static row => row.Name, static spec => spec.Name, Rows.Names<T>(), prune);

    public static IO<Committed<ReconciledStyles>> Reconcile(RhinoDoc doc, StyleSpec spec, string name, RedrawPolicy redraw, Option<Func<StyleRows, IO<Unit>>> reassign = default) =>
        from plan in Plan(doc, spec)
        from committed in Commits.Commit(doc, LocalText.Of(name).Local, redraw,
            from hatchPatterns in Applied(doc, TableKinds.HatchPatterns, plan.HatchPatterns, static (_, made) => use(made), HatchPatterns.Unchanged)
            from linetypes in Applied(doc, TableKinds.Linetypes, plan.Linetypes, Written<Linetype, LinetypeDefinition>(TableKinds.Linetypes, Linetypes.Written), Linetypes.Unchanged)
            from sectionStyles in Applied(doc, TableKinds.SectionStyles, plan.SectionStyles,
                Written<SectionStyle, SectionStyleDefinition>(TableKinds.SectionStyles, (staged, definition) => SectionStyles.Written(doc, staged, definition)), SectionStyles.Unchanged)
            from dimensionStyles in Applied(doc, TableKinds.DimensionStyles, plan.DimensionStyles,
                Written<DimensionStyle, DimensionStyleSpec>(TableKinds.DimensionStyles, (staged, row) => DimensionStyles.Written(doc, staged, row)), DimensionStyles.Unchanged)
            let written = new StyleRows(hatchPatterns, linetypes, sectionStyles, dimensionStyles)
            from hatchCurrent in spec.CurrentHatchPattern.Traverse(address => HatchPatterns.SetCurrent(doc, address)).As()
            from linetypeTable in spec.LinetypeTable.Traverse(table => Linetypes.WriteTable(doc, table)).As()
            from dimensionCurrent in spec.CurrentDimensionStyle.Traverse(address => DimensionStyles.SetCurrent(doc, address)).As()
            from reassigned in reassign.Traverse(move => move(written)).As()
            from graph in Graph(doc)
            from styled in DimensionStylesInUse(doc)
            from removed in Pruned(
                graph,
                Candidates(doc.HatchPatterns, plan.HatchPatterns, row => row.InUse || row.Index == doc.HatchPatterns.CurrentHatchPatternIndex)
                + Candidates(doc.Linetypes, plan.Linetypes, row => row.InUse || row.Index == doc.Linetypes.CurrentLinetypeIndex)
                + Candidates(doc.SectionStyles, plan.SectionStyles, static row => row.InUse)
                + Candidates(doc.DimStyles, plan.DimensionStyles, row => styled.Contains(row.Id) || row.Index == doc.DimStyles.CurrentIndex))
            select new ReconciledStyles(written, removed))
        select committed;
}
