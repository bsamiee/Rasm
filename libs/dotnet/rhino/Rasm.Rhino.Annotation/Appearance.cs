using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.UI;

namespace Rasm.Rhino.Annotation;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record AppearanceTables(
    TableSpec<HatchPatternDefinition> HatchPatterns,
    TableSpec<LinetypeDefinition> Linetypes,
    TableSpec<SectionStyleDefinition> SectionStyles,
    TableSpec<DimensionStyleSpec> DimensionStyles,
    Option<ComponentRef> CurrentDimensionStyle);

public sealed record AppearanceIndices(
    Seq<(string Name, int Index)> HatchPatterns,
    Seq<(string Name, int Index)> Linetypes,
    Seq<(string Name, int Index)> SectionStyles,
    Seq<(string Name, int Index)> DimensionStyles);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Appearance {
    public static IO<AppearanceIndices> Reconcile(RhinoDoc doc, AppearanceTables tables, Func<AppearanceIndices, IO<Unit>> assign) {
        TableAccessors<HatchPattern> hatches = TableAccessors.HatchPatterns(doc);
        TableAccessors<Linetype> linetypes = TableAccessors.Linetypes(doc);
        TableAccessors<SectionStyle> sections = TableAccessors.SectionStyles(doc);
        TableAccessors<DimensionStyle> dimensions = TableAccessors.DimStyles(doc);
        return
            from hatchRows in TableOps.Present(doc.HatchPatterns)
            from linetypeRows in TableOps.Present(doc.Linetypes)
            from sectionRows in TableOps.Present(doc.SectionStyles)
            from dimensionRows in TableOps.Present(doc.DimStyles)
            from plans in IO.lift(() => zip(
                    TableOps.Plan(hatchRows, tables.HatchPatterns, static row => row.Name),
                    TableOps.Plan(linetypeRows, tables.Linetypes, static row => row.Name),
                    TableOps.Plan(sectionRows, tables.SectionStyles, static row => row.Name),
                    TableOps.Plan(dimensionRows, tables.DimensionStyles, static row => Some(row.Name)))
                .As()
                .ToFin())
            from indices in Commits.Commit(
                doc,
                LOC.STR("Reconcile appearance tables"),
                new RedrawPolicy.Silent(),
                from hatched in TableOps.Upsert(
                    hatches,
                    plans.First.Upserts,
                    HatchPatterns.Written,
                    static (live, staged) => (HatchPatterns.Row(live), HatchPatterns.Row(staged)).Apply(static (held, wanted) => held.Definition == wanted.Definition).As(),
                    quiet: true)
                from linetyped in TableOps.Upsert(linetypes, plans.Second.Upserts, Linetypes.Written, static (_, _) => IO.pure(value: false), quiet: true)
                from sectioned in TableOps.Upsert(
                    sections,
                    plans.Third.Upserts,
                    (staged, definition) => SectionStyles.Written(doc, staged, definition),
                    static (live, staged) => IO.lift(() => SectionStyles.Row(live).Definition == SectionStyles.Row(staged).Definition),
                    quiet: true)
                from styled in TableOps.Upsert(dimensions, plans.Fourth.Upserts, (staged, spec) => DimensionStyles.Written(doc, staged, spec), static (_, _) => IO.pure(value: false), quiet: true)
                let upserted = new AppearanceIndices(hatched, linetyped, sectioned, styled)
                from current in tables.CurrentDimensionStyle
                    .Traverse(address => TableOps.Find(doc.DimStyles, address, includeDeleted: false).Bind(style => DimensionStyles.SetCurrent(doc, style.Index, quiet: true)))
                    .As()
                from assigned in assign(upserted)
                from dimensionsPruned in TableOps.DeleteRows(dimensions, plans.Fourth.Prunes, quiet: true)
                from sectionsPruned in TableOps.DeleteRows(sections, plans.Third.Prunes.Filter(static row => !row.InUse).Strict(), quiet: true)
                from held in IO.lift(() => toSeq(doc.SectionStyles).Filter(static style => !style.IsDeleted).Map(static style => (Linetype: style.BoundaryLinetypeIndex, Hatch: style.HatchIndex)).Strict())
                from linetypesPruned in TableOps.DeleteRows(linetypes, plans.Second.Prunes.Filter(row => !row.InUse && !held.Exists(style => style.Linetype == row.Index)).Strict(), quiet: true)
                from hatchesPruned in TableOps.DeleteRows(hatches, plans.First.Prunes.Filter(row => !row.InUse && !held.Exists(style => style.Hatch == row.Index)).Strict(), quiet: true)
                select upserted)
            select indices;
    }

    internal static IO<Unit> WriteUserStrings(UserStringAccessors accessors, HashMap<string, string> strings) =>
        from edits in IO.lift(() => toSeq<(string Key, string Value)>(strings).Traverse(static pair => UserStringEdit.Set.From(pair.Key, pair.Value)).As())
        from written in edits.TraverseM(edit => GeometryOps.EditUserStrings(accessors, edit)).As()
        select unit;
}
