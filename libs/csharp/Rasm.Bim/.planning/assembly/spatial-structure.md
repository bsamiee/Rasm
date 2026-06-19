# [BIM_SPATIAL_STRUCTURE]

The host-neutral spatial-structure tree and the closed decomposition algebra: the project-to-site-to-building-to-storey-to-space-to-element hierarchy as a portable `BimAssembly` tree, plus the `AssemblyRel` union mirroring `IfcRelAggregates`/`IfcRelNests`/`IfcRelContainedInSpatialStructure`/`IfcRelVoidsElement`/`IfcRelConnectsElements`. The assembly is the host-neutral counterpart to the Rhino-native block/layer capture; the two coexist at the universal IFC contract, neither thinned to feed the other.

## [1]-[INDEX]

- [2]-[ASSEMBLY_TREE]: `SpatialContainer` node, `AssemblyRel` decomposition union, `BimAssembly` tree, and the `Assemble` fold.

## [2]-[ASSEMBLY_TREE]

- Owner: `BimAssembly` the host-neutral spatial-structure tree mirroring the IFC spatial hierarchy projected from the `IfcSemanticModel.SpatialNode` family; `SpatialContainer` the tree-node record carrying the spatial entity-type string (the spatial-container vocabulary — `IfcProject`/`IfcSite`/`IfcBuilding`/`IfcBuildingStorey`/`IfcSpace` — is distinct from the `model/elements#ELEMENT_MODEL` `IfcClass` element vocabulary, so the node keeps the raw entity-type rather than forcing the element discriminant), the GlobalId, and the contained-element/child-container sets derived from the `AssemblyRel.Aggregates` graph; `AssemblyRel` `[Union]` the closed decomposition-relationship family mirroring the IFC `IfcRel*` relationships.
- Entry: `BimAssembly.Assemble(IfcSemanticModel semantic)` folds the spatial hierarchy and the decomposition relationships into the host-neutral assembly tree, resolving the root as the `IfcProject` node (falling back to the non-aggregated whole when the schema omits the project node) — `Fin<T>` aborts on a missing spatial root (`faults#FAULT_BAND` `BimFault.DanglingReference`) lowered with `.ToError()`; the tree is traversable by GlobalId and queryable through the `query/element-set#ELEMENT_SET` spatial-container predicate.
- Packages: GeometryGymIFC_Core, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new spatial-structure level is one `SpatialContainer` projection from an existing `IfcSemanticModel.SpatialNode` row; a new decomposition relationship is one `AssemblyRel` union arm; never a per-relationship type.
- Boundary: the assembly is HOST-NEUTRAL — it mirrors the IFC spatial hierarchy as a portable tree and a RhinoCommon `Layer`/`InstanceDefinition` binding is the named seam violation; the spatial hierarchy projects from the `IfcSemanticModel.SpatialNode` family owned at `exchange/import-rail#IMPORT_RAIL`, consumed as settled vocabulary; the decomposition relationships are a closed `AssemblyRel` union mirroring the IFC `IfcRel*` entities, never a per-relationship class; the assembly is the host-neutral counterpart to the Rhino-native `Rasm.Rhino/Exchange` block-and-layer capture — the two coexist, neither gutted, meeting only at the universal IFC semantic contract; the `georeferencing/coordinate-reference#GEO_REFERENCE` `GeoReference` reconciles the tree root into one real-world frame so federated assemblies share an origin.

```csharp signature
[Union]
public partial record AssemblyRel {
    partial record Aggregates(string WholeId, Seq<string> PartIds);
    partial record Nests(string HostId, Seq<string> NestedIds);
    partial record ContainedIn(string ContainerId, Seq<string> ElementIds);
    partial record Voids(string ElementId, string OpeningId);
    partial record Connects(string FirstId, string SecondId);
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
}
```

## [3]-[RESEARCH]

- [ASSEMBLY_PROJECTION]: the `BimAssembly.Assemble` fold projecting the IFC spatial hierarchy and the `IfcRel*` decomposition relationships into the host-neutral tree and the `AssemblyRel` union arms ground against the GeometryGym spatial-structure and relationship entity surface; the `exchange/import-rail#IMPORT_RAIL` `Semantic` extract populates only the `AssemblyRel.Aggregates` arm from `IfcRelAggregates`, so the `Nests`/`ContainedIn`/`Voids`/`Connects` arms confirm their `IfcRelNests`/`IfcRelContainedInSpatialStructure`/`IfcRelVoidsElement`/`IfcRelConnectsElements` extract spellings against the GeometryGym surface before the `IfcSemanticModel.Decomposition` family carries every closed-union arm.
