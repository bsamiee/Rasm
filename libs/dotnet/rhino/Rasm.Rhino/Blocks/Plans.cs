using LanguageExt.ClassInstances;
using QuikGraph;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Blocks;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record PlannedMember {
    private PlannedMember(Option<ObjectAttributes> attributes) => Attributes = attributes;

    public Option<ObjectAttributes> Attributes { get; }

    public sealed record Shape(GeometryBase Geometry, Option<ObjectAttributes> Attributes = default) : PlannedMember(Attributes);

    public sealed record Instance(string Definition, Transform Xform, Option<ObjectAttributes> Attributes = default) : PlannedMember(Attributes);
}

public sealed record PlannedDefinition(DefinitionMetadata Metadata, Point3d BasePoint, Seq<PlannedMember> Members);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Plans {
    // --- [PROGRAM]
    public static IO<Seq<RowPlan<InstanceDefinition, PlannedDefinition>>> Plan(RhinoDoc doc, Seq<PlannedDefinition> planned, Prune prune) =>
        from ordered in IO.lift(Ordered(planned))
        from rows in IO.lift(() => Conversions.Rows(doc.InstanceDefinitions.GetList(ignoreDeleted: true)).Filter(static row => !row.IsReference).Strict())
        from matched in IO.lift(() => Rows.Plan(rows, ordered, static row => row.Name, static spec => spec.Metadata.Name, StringComparer.OrdinalIgnoreCase, prune))
        from nesting in DefinitionGraph.Read(doc)
        let removable = toHashSet(nesting.Removable(toHashSet(matched.Choose(static step => step is RowPlan<InstanceDefinition, PlannedDefinition>.Remove remove ? Some(remove.Row.Id) : None))))
        from pending in matched.TraverseM(step => step.Switch(
                (Table: doc.InstanceDefinitions, Removable: removable),
                create: static (_, _) => IO.pure(value: true),
                change: static (state, change) => Stored(state.Table, change.Row, change.Spec).Map(stored => !stored || DefinitionMetadata.Of(change.Row) != change.Spec.Metadata),
                remove: static (state, remove) => IO.pure(state.Removable.Contains(remove.Row.Id)),
                order: static (_, _) => IO.pure(value: true))
            .Map(kept => Conversions.Present(kept, step))).As()
        select pending.Somes().Strict();

    public static IO<Committed<Seq<RowPlan<InstanceDefinition, PlannedDefinition>>>> Reconcile(RhinoDoc doc, Seq<PlannedDefinition> planned, Prune prune, string name, RedrawPolicy redraw) =>
        Commits.Commit(doc, name, redraw,
            from plan in Plan(doc, planned, prune)
            from applied in plan.TraverseM(step => Applied(doc, step)).As()
            select plan);

    // --- [ORDER]
    private static Fin<Seq<PlannedDefinition>> Ordered(Seq<PlannedDefinition> planned) =>
        toHashMap<EqStringOrdinalIgnoreCase, string, PlannedDefinition>(planned.Map(static row => (row.Metadata.Name, row))) switch {
            var byName => planned.Map(static row => row.Metadata.Name)
                .ToBidirectionalGraph(name => byName.Find(name).ToSeq().Bind(static row => row.Members.Choose(static member => member is PlannedMember.Instance nested ? Some(nested.Definition) : None))
                    .Choose(nested => byName.Find(nested).Map(found => new SEquatableEdge<string>(found.Metadata.Name, name))), allowParallelEdges: false)
                .ToArrayBidirectionalGraph().Condensed.SourceFirst
                .Map(order => order.Choose(byName.Find).Strict()),
        };

    // --- [CHANGES]
    private static IO<Unit> Applied(RhinoDoc doc, RowPlan<InstanceDefinition, PlannedDefinition> step) =>
        step.Switch(
            doc,
            create: static (document, create) => Added(document, create.Spec, overrideExisting: false),
            change: static (document, change) =>
                from stored in Stored(document.InstanceDefinitions, change.Row, change.Spec)
                from added in unless(stored, Added(document, change.Spec, overrideExisting: true)).As()
                from named in when(DefinitionMetadata.Of(change.Row) != change.Spec.Metadata, change.Spec.Metadata.Written(document.InstanceDefinitions, change.Row)).As()
                select unit,
            remove: static (document, remove) => Products.Accepted(() => document.InstanceDefinitions.Delete(remove.Row.Index, deleteReferences: false, quiet: true)),
            order: static (_, _) => IO.pure(unit));

    private static IO<Unit> Added(RhinoDoc doc, PlannedDefinition planned, bool overrideExisting) =>
        (from members in planned.Members.Map((member, index) => Owned(doc, member, index)).TraverseM(static owned => owned).As()
         from added in Products.Accepted(() => doc.InstanceDefinitions.Add(
             planned.Metadata.Name, Conversions.Unset(planned.Metadata.Description), planned.Metadata.Url, planned.Metadata.UrlTag, planned.BasePoint,
             members.Map(static member => member.Geometry), members.Map(static member => member.Attributes), overrideExisting) >= 0)
         select unit).Bracket();

    private static IO<bool> Stored(InstanceDefinitionTable table, InstanceDefinition live, PlannedDefinition planned) =>
        IO.lift(() => toSeq(live.GetObjects()).Map(Optional).Strict()).Bind(held =>
            held.Count == planned.Members.Count && planned.Members.ForAll(static member => member.Attributes.IsNone)
                ? planned.Members.Zip(held).TraverseM(pair => (from shape in Resolved(table, pair.First) from member in pair.Second select (Shape: shape, Member: member)).Match(
                    Some: matched => Same(matched.Shape, planned.BasePoint, matched.Member.Geometry),
                    None: static () => IO.pure(value: false))).As().Map(static matches => matches.ForAll(static matched => matched))
                : IO.pure(value: false));

    private static IO<bool> Same(GeometryBase planned, Point3d basePoint, GeometryBase stored) =>
        (from placed in use(() => planned.DuplicateShallow())
         from moved in Products.Accepted(() => placed.Translate(Point3d.Origin - basePoint))
         select GeometryBase.GeometryEquals(placed, stored)).Bracket();

    // --- [MEMBERS]
    private static Option<GeometryBase> Resolved(InstanceDefinitionTable table, PlannedMember member) =>
        member.Switch(
            table,
            shape: static (_, shape) => Some(shape.Geometry),
            instance: static (rows, nested) => Optional(rows.Find(nested.Definition)).Map(GeometryBase (found) => new InstanceReferenceGeometry(found.Id, nested.Xform)));

    private static IO<(GeometryBase Geometry, ObjectAttributes Attributes)> Owned(RhinoDoc doc, PlannedMember member, int index) =>
        from geometry in IO.lift(() => Resolved(doc.InstanceDefinitions, member).ToFin(new RhinoFault(nameof(PlannedMember.Instance.Definition), RhinoReason.Missing, index)))
        from created in when(member is PlannedMember.Instance, use(() => geometry).Map(unit)).As()
        from attributes in member.Attributes.Match(Some: IO.pure, None: () => use(doc.CreateDefaultAttributes))
        select (geometry, attributes);
}
