# [BIM_SPATIAL_STRUCTURE]

The host-neutral spatial-structure tree and the closed decomposition algebra: the project-to-site-to-building-to-storey-to-space-to-element hierarchy as a portable `BimAssembly` tree, plus the `AssemblyRel` union mirroring `IfcRelAggregates`/`IfcRelNests`/`IfcRelContainedInSpatialStructure`/`IfcRelVoidsElement`/`IfcRelConnectsElements`. The assembly is the host-neutral counterpart to the Rhino-native block/layer capture; the two coexist at the universal IFC contract, neither thinned to feed the other.

## [1]-[INDEX]

- [2]-[ASSEMBLY_TREE]: `SpatialContainer` node, `AssemblyRel` decomposition union, `BimAssembly` tree, and the `Assemble` fold.

## [2]-[ASSEMBLY_TREE]

- Owner: `BimAssembly` the host-neutral spatial-structure tree mirroring the IFC spatial hierarchy projected from the `IfcSemanticModel.SpatialNode` family; `SpatialContainer` the tree-node record carrying the spatial-element class, the GlobalId, and the contained-element set; `AssemblyRel` `[Union]` the closed decomposition-relationship family mirroring the IFC `IfcRel*` relationships.
- Entry: `BimAssembly.Assemble(IfcSemanticModel semantic)` folds the spatial hierarchy and the decomposition relationships into the host-neutral assembly tree — `Fin<T>` aborts on an unmapped spatial class (`faults#FAULT_BAND` `BimFault.UnmappedClass`) or a dangling spatial reference (`BimFault.DanglingReference`), each lowered with `.ToError()`; the tree is traversable by GlobalId and queryable through the `query/element-set#ELEMENT_SET` spatial-container predicate.
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
    IfcClass Class,
    string Name,
    string LongName,
    Seq<string> ContainedElementIds,
    Seq<string> ChildContainerIds);

public sealed record BimAssembly(
    Seq<SpatialContainer> Containers,
    Seq<AssemblyRel> Relationships,
    string RootGlobalId) {
    public static Fin<BimAssembly> Assemble(IfcSemanticModel semantic) =>
        semantic.Spatial
            .TraverseM(node => IfcClass.TryGet(node.EntityType)
                .ToFin(new BimFault.UnmappedClass($"spatial-class-miss:{node.EntityType}").ToError())
                .Map(cls => new SpatialContainer(node.GlobalId, cls, node.Name, node.LongName, node.ContainedGlobalIds,
                    semantic.Spatial.Filter(child => child.ContainedGlobalIds.Contains(node.GlobalId)).Map(static c => c.GlobalId))))
            .As()
            .Bind(containers => containers.Find(c => c.Class.Domain == IfcDomain.Spatial)
                .ToFin(new BimFault.DanglingReference("spatial-root-miss").ToError())
                .Map(root => new BimAssembly(containers, semantic.Decomposition, root.GlobalId)));
}
```

## [3]-[RESEARCH]

- [ASSEMBLY_PROJECTION]: the `BimAssembly.Assemble` fold projecting the IFC spatial hierarchy and the `IfcRel*` decomposition relationships into the host-neutral tree and the `AssemblyRel` union arms ground against the GeometryGym spatial-structure and relationship entity surface; the `exchange/import-rail#IMPORT_RAIL` `Semantic` extract populates only the `AssemblyRel.Aggregates` arm from `IfcRelAggregates`, so the `Nests`/`ContainedIn`/`Voids`/`Connects` arms confirm their `IfcRelNests`/`IfcRelContainedInSpatialStructure`/`IfcRelVoidsElement`/`IfcRelConnectsElements` extract spellings against the GeometryGym surface before the `IfcSemanticModel.Decomposition` family carries every closed-union arm.
