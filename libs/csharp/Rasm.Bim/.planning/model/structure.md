# [BIM_SPATIAL_STRUCTURE]

The host-neutral spatial-structure tree and the closed decomposition algebra: the project-to-site-to-building-to-storey-to-space-to-element hierarchy as a portable `BimAssembly` tree, plus the `AssemblyRel` union mirroring `IfcRelAggregates`/`IfcRelNests`/`IfcRelContainedInSpatialStructure`/`IfcRelVoidsElement`/`IfcRelConnectsElements`. The assembly is the host-neutral counterpart to the Rhino-native block/layer capture; the two coexist at the universal IFC contract, neither thinned to feed the other.

## [01]-[INDEX]

- [01]-[ASSEMBLY_TREE]: `SpatialContainer` node, `AssemblyRel` decomposition union, `BimAssembly` tree, the `Assemble` fold, and the polymorphic `BimAssembly.Traverse(globalId, TraversalDirection)` fold over the five-arm decomposition graph.

## [02]-[ASSEMBLY_TREE]

- Owner: `BimAssembly` the host-neutral spatial-structure tree mirroring the IFC spatial hierarchy projected from the `IfcSemanticModel.SpatialNode` family; `SpatialContainer` the tree-node record carrying the spatial entity-type string (the spatial-container vocabulary — `IfcProject`/`IfcSite`/`IfcBuilding`/`IfcBuildingStorey`/`IfcSpace` — is distinct from the `Model/elements#ELEMENT_MODEL` `IfcClass` element vocabulary, so the node keeps the raw entity-type rather than forcing the element discriminant), the GlobalId, and the contained-element/child-container sets derived from the `AssemblyRel.Aggregates` graph; `AssemblyRel` `[Union]` the closed decomposition-relationship family mirroring the IFC `IfcRel*` relationships.
- Entry: `BimAssembly.Assemble(IfcSemanticModel semantic)` folds the spatial hierarchy and the decomposition relationships into the host-neutral assembly tree, resolving the root as the `IfcProject` node (falling back to the non-aggregated whole when the schema omits the project node) — `Fin<T>` aborts on a missing spatial root (`Model/faults#FAULT_BAND` `BimFault.DanglingReference`) lowered with `.ToError()`; `BimAssembly.Traverse(string globalId, TraversalDirection direction)` is the one polymorphic walk over the complete five-arm decomposition graph — `Descendants` follows the `Aggregates`/`Nests`/`ContainedIn` whole→part edges to the transitive part closure, `Ancestors` follows them in reverse to the root chain, `Contained` reads the direct `ContainedIn`/`Aggregates` children of a spatial container, and `Voids` reads the `Voids` opening set of an element — one direction-discriminated breadth-first fold over the typed `AssemblyRel` edges, never a per-direction recursion.
- Packages: GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new spatial-structure level is one `SpatialContainer` projection from an existing `IfcSemanticModel.SpatialNode` row; a new decomposition relationship is one `AssemblyRel` union arm the `Traverse` fold reads on one adjacency row; a new traversal direction is one `TraversalDirection` case reading the same `AssemblyRel` edge set; never a per-relationship type and never a per-direction traversal method.
- Boundary: the assembly is HOST-NEUTRAL — it mirrors the IFC spatial hierarchy as a portable tree and a RhinoCommon `Layer`/`InstanceDefinition` binding is the named seam violation; the spatial hierarchy projects from the `IfcSemanticModel.SpatialNode` family owned at `Exchange/import#IMPORT_RAIL`, consumed as settled vocabulary; the decomposition relationships are a closed `AssemblyRel` union mirroring the IFC `IfcRel*` entities, never a per-relationship class — all five arms (`Aggregates`/`Nests`/`ContainedIn`/`Voids`/`Connects`) are populated by the `Exchange/import#IMPORT_RAIL` `Semantic` extract, not only `Aggregates`, so the decomposition graph is complete and the `Traverse` fold reads every edge kind; the `BimAssembly.Traverse` walk is ONE direction-discriminated breadth-first fold over the typed `AssemblyRel` adjacency — a `TraverseDescendants`/`TraverseAncestors`/`TraverseContained` operation family or a hand-rolled per-level recursion is the deleted form per the no-operation-family law, the `TraversalDirection` case selecting the edge kinds and the walk threading an immutable visited-set fold, never a mutable `HashSet<string>` mutated in place; the `Model/query#ELEMENT_SET` `BySpatialContainer` arm and the `Review/validation#IDS_FACETS` `PartOf` facet both read the complete decomposition graph the `Traverse` fold walks, and the `Semantics/georeference#GEO_REFERENCE` root-reconciliation and `Review/diff#MODEL_DIFF` move-detection compose the same `Traverse` walk; the assembly is the host-neutral counterpart to the Rhino-native `Rasm.Rhino/Exchange` block-and-layer capture — the two coexist, neither gutted, meeting only at the universal IFC semantic contract; the `Semantics/georeference#GEO_REFERENCE` `GeoReference` reconciles the tree root into one real-world frame so federated assemblies share an origin.

```csharp signature
public enum TraversalDirection : byte { Descendants = 0, Ancestors = 1, Contained = 2, Voids = 3 }

[Union]
public partial record AssemblyRel {
    partial record Aggregates(string WholeId, Seq<string> PartIds);
    partial record Nests(string HostId, Seq<string> NestedIds);
    partial record ContainedIn(string ContainerId, Seq<string> ElementIds);
    partial record Voids(string ElementId, string OpeningId);
    partial record Connects(string FirstId, string SecondId);

    // The whole→part directed edge each decomposition arm contributes: the parent and the children it
    // owns. Voids is the element→opening edge, Connects the undirected sibling pair. One Switch, the
    // Traverse walk reads the directed pairs without a per-arm branch at the call site.
    public (string Parent, Seq<string> Children) Edge => Switch(
        aggregates:  static a => (a.WholeId, a.PartIds),
        nests:       static n => (n.HostId, n.NestedIds),
        containedIn: static c => (c.ContainerId, c.ElementIds),
        voids:       static v => (v.ElementId, Seq(v.OpeningId)),
        connects:    static c => (c.FirstId, Seq(c.SecondId)));
}

public sealed record SpatialContainer(
    string GlobalId,
    string EntityType,
    string Name,
    string LongName,
    Seq<string> ContainedElementIds,
    Seq<string> ChildContainerIds);

public sealed record BimAssembly(
    Seq<SpatialContainer> Containers,
    Seq<AssemblyRel> Relationships,
    string RootGlobalId) {
    public static Fin<BimAssembly> Assemble(IfcSemanticModel semantic) {
        var aggregatedParts = toHashSet(semantic.Decomposition
            .Choose(static rel => rel is AssemblyRel.Aggregates a ? Some(a.PartIds) : None)
            .Bind(static parts => parts));
        var containers = semantic.Spatial.Map(node => new SpatialContainer(
            node.GlobalId, node.EntityType, node.Name, node.LongName, node.ContainedGlobalIds,
            semantic.Decomposition.Choose(rel => rel is AssemblyRel.Aggregates a && a.WholeId == node.GlobalId ? Some(a.PartIds) : None)
                .Bind(static parts => parts)));
        return containers
            .Find(static c => c.EntityType == "IfcProject")
            .OrElse(() => containers.Find(c => !aggregatedParts.Contains(c.GlobalId)))
            .ToFin(new BimFault.DanglingReference("spatial-root-miss").ToError())
            .Map(root => new BimAssembly(containers, semantic.Decomposition, root.GlobalId));
    }

    // One polymorphic walk over the complete five-arm decomposition graph: the direction selects the
    // edge kinds and the adjacency orientation, the closure threads an immutable visited-set fold.
    public Seq<string> Traverse(string globalId, TraversalDirection direction) {
        var down = direction is TraversalDirection.Descendants or TraversalDirection.Contained or TraversalDirection.Voids;
        var arms = direction switch {
            TraversalDirection.Voids => Relationships.Filter(static r => r is AssemblyRel.Voids),
            _                        => Relationships.Filter(static r => r is not AssemblyRel.Connects),
        };
        var adjacency = arms
            .Map(static r => r.Edge)
            .Bind(edge => down
                ? edge.Children.Map(child => (From: edge.Parent, To: child))
                : edge.Children.Map(child => (From: child, To: edge.Parent)))
            .GroupBy(static e => e.From)
            .ToMap(static g => g.Key, static g => g.Map(static e => e.To).ToSeq());
        return direction is TraversalDirection.Contained
            ? adjacency.Find(globalId).IfNone(Seq<string>())
            : Closure(adjacency, Seq(globalId), toHashSet(globalId), Seq<string>());
    }

    static Seq<string> Closure(Map<string, Seq<string>> adjacency, Seq<string> frontier, HashSet<string> seen, Seq<string> reached) =>
        frontier.HeadOrNone().Match(
            None: () => reached,
            Some: node => {
                var next = adjacency.Find(node).IfNone(Seq<string>()).Filter(n => !seen.Contains(n));
                return Closure(adjacency, frontier.Tail.Concat(next), next.Fold(seen, static (s, n) => s.Add(n)), reached.Concat(next));
            });
}
```

## [03]-[RESEARCH]

- [ASSEMBLY_PROJECTION]: the `BimAssembly.Assemble`/`Traverse` folds projecting the IFC spatial hierarchy and the five `IfcRel*` decomposition relationships into the host-neutral tree and the `AssemblyRel` union arms are verified against the live GeometryGym decompile — the `Exchange/import#IMPORT_RAIL` `Semantic` extract now populates every arm: `IfcRelAggregates.RelatingObject`/`RelatedObjects` onto `Aggregates`, `IfcRelNests.RelatingObject` (`IfcObjectDefinition`)/`RelatedObjects` (`LIST<IfcObjectDefinition>`) onto `Nests`, `IfcRelContainedInSpatialStructure.RelatingStructure` (`IfcSpatialElement`)/`RelatedElements` (`SET<IfcProduct>`) onto `ContainedIn`, `IfcRelVoidsElement.RelatingBuildingElement` (`IfcElement`)/`RelatedOpeningElement` (`IfcFeatureElementSubtraction`) onto `Voids`, and `IfcRelConnectsElements.RelatingElement`/`RelatedElement` (each `IfcElement`) onto `Connects` — the member spellings confirmed so the `IfcSemanticModel.Decomposition` family carries every closed-union arm and the `Traverse` fold reads a complete decomposition graph rather than the `Aggregates`-only subset; the `BySpatialContainer` query arm, the IDS `PartOf` facet, and the `Review/diff#MODEL_DIFF` move detection all read the populated graph, and the `Traverse` direction-discriminated breadth-first fold is the one walk those consumers compose rather than a per-direction recursion.
