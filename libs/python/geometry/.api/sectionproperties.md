# [PY_GEOMETRY_API_SECTIONPROPERTIES]

`sectionproperties` computes geometric, warping, plastic, and stress properties for an arbitrary 2D profile over a triangular FE mesh. It is the gated enrichment on the geometry ifc/structural rail, deriving the warping and plastic section receipts the closed-form spine (`ifcopenshell` profile geometry over numpy section integrals) cannot; the spine never depends on it. Card `geometry [STRUCTURAL_SECTION_PROPS_GATED] [BLOCKED]` tracks the deferred consumer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `sectionproperties`
- package: `sectionproperties` (MIT)
- import: `import sectionproperties`
- owner: `geometry`
- rail: ifc/structural section-property enrichment
- entry points: none (library only)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry model (`sectionproperties.pre`)

`Geometry` models one polygon region with holes and a control point; `CompoundGeometry` assembles multi-region built-up sections. Both produce the triangular mesh `Section` consumes.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]      | [CAPABILITY]                                           |
| :-----: | :--------------------- | :----------------- | :----------------------------------------------------- |
|  [01]   | `pre.Geometry`         | single-region body | one polygon region with holes, control point, material |
|  [02]   | `pre.CompoundGeometry` | multi-region body  | union of `Geometry` regions for built-up sections      |
|  [03]   | `pre.Material`         | material           | elastic modulus, Poisson ratio, yield, density, color  |
|  [04]   | `pre.DEFAULT_MATERIAL` | material           | unit-property default material when none is assigned   |

[PUBLIC_TYPE_SCOPE]: solver and results (`sectionproperties.analysis` / `.post`)

`Section` is the solver bound to a meshed geometry; the calculate and getter entrypoints return the property and stress carriers.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]     | [CAPABILITY]                                          |
| :-----: | :----------------------- | :---------------- | :---------------------------------------------------- |
|  [01]   | `analysis.Section`       | section solver    | geometric/warping/plastic property solver over a mesh |
|  [02]   | `post.SectionProperties` | property receipt  | accumulated geometric/warping/plastic property fields |
|  [03]   | `post.StressResult`      | stress receipt    | per-element stress arrays from a stress calculation   |
|  [04]   | `post.StressPost`        | stress aggregator | the stress-analysis result carrier from a load case   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: geometry construction and meshing (`pre.Geometry` / `pre.CompoundGeometry`)

Build from points, a DXF, or a Rhino `.3dm`, register interior voids through the `-` operator or `from_points(holes=...)`, then `create_mesh` at a maximum triangle area before binding to a `Section`.

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :---------------------------------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `Geometry.from_points(points, facets, control_points, holes, material)` | factory  | polygon region from point/facet rings     |
|  [02]   | `Geometry.from_dxf(dxf_filepath) -> Geometry \| CompoundGeometry`       | factory  | import a DXF profile into a geometry body |
|  [03]   | `Geometry.from_3dm(filepath)`                                           | factory  | Rhino `.3dm` profile import               |
|  [04]   | `CompoundGeometry(geoms)`                                               | ctor     | assemble built-up section from regions    |
|  [05]   | `geometry - hole` (`__sub__`)                                           | operator | subtract a polygon for an interior void   |
|  [06]   | `geometry.offset_perimeter(amount, where)`                              | instance | inset/outset the perimeter for thickness  |
|  [07]   | `geometry.create_mesh(mesh_sizes) -> Geometry`                          | instance | triangulate region(s) at a max-area bound |

[ENTRYPOINT_SCOPE]: property and stress calculation (`analysis.Section`)

`calculate_geometric_properties` runs first; the warping and plastic passes depend on it. Query stress at points only after a stress pass. Mesh census reads `elements`/`num_nodes`/`mesh_elements`/`mesh_nodes`; no `num_elements` exists.

| [INDEX] | [SURFACE]                                                     | [SHAPE]   | [CAPABILITY]                                         |
| :-----: | :------------------------------------------------------------ | :-------- | :--------------------------------------------------- |
|  [01]   | `Section(geometry)`                                           | ctor      | solver bound to a meshed geometry                    |
|  [02]   | `Section.calculate_geometric_properties()`                    | instance  | area, centroid, second moments, principal axes       |
|  [03]   | `Section.calculate_warping_properties()`                      | instance  | torsion constant, shear/warping centers, shear areas |
|  [04]   | `Section.calculate_plastic_properties()`                      | instance  | plastic centroids, plastic moduli, shape factors     |
|  [05]   | `Section.calculate_frame_properties() -> tuple`               | instance  | reduced area/inertia/torsion tuple, no warping pass  |
|  [06]   | `Section.calculate_stress(...) -> post.StressPost`            | instance  | full-field stress under a design-action load case    |
|  [07]   | `Section.get_stress_at_points(pts, n, mxx, myy, ...) -> list` | instance  | per-point stress under normal-force/moment case      |
|  [08]   | `Section.elements -> list[fea.Tri6]`                          | attribute | Tri6 element roster                                  |
|  [09]   | `Section.num_nodes -> int`                                    | attribute | node count                                           |
|  [10]   | `Section.mesh_elements -> np.ndarray`                         | attribute | triangle connectivity                                |
|  [11]   | `Section.mesh_nodes -> np.ndarray`                            | attribute | vertex coordinates                                   |

[ENTRYPOINT_SCOPE]: computed-property accessors (`Section.get_*`)

Each `get_*` reads a scalar off the `post.SectionProperties` receipt after its owning calculate pass populates it.

- [GEOMETRIC]: `get_area` `get_perimeter` `get_mass` `get_ic` `get_z` `get_rc` `get_phi`
- [WARPING]: `get_j` `get_sc` `get_sc_p` `get_sc_t` `get_as` `get_as_p` `get_gamma` `get_beta`
- [PLASTIC]: `get_pc` `get_mp` `get_s` `get_zp` `get_sf`
- [COMPOSITE]: `get_ea` `get_eic` `get_e_eff` `get_g_eff` `get_nu_eff` `get_e_ref`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- geometry axis: `from_points`/`from_dxf`/`from_3dm` build a single region, `CompoundGeometry` assembles built-up sections, interior voids register through the `-` operator or `from_points(holes=...)`, and `create_mesh(mesh_sizes)` triangulates at a maximum-area bound — the mesh is the prerequisite for every `Section` calculation.
- solver axis: `calculate_geometric_properties` is the prerequisite for `calculate_warping_properties` and `calculate_plastic_properties`; each populates the `post.SectionProperties` receipt read back through the `get_*` accessors, and `calculate_frame_properties` returns the reduced area/inertia/torsion tuple a frame analysis needs, bypassing the staged receipt.
- stress axis: `get_stress_at_points(pts, n, mxx, myy, ...)` evaluates stress at sample points under a normal-force/moment case once warping properties exist, and `calculate_stress(...)` runs the full-field pass returning a `post.StressPost` whose `StressResult` arrays hold the per-element fields.

[STACKING]:
- `ifcopenshell`(`.api/ifcopenshell.md`): the `IfcProfileDef` point ring read through `util.element` feeds `Geometry.from_points`, and the staged warping/plastic receipt writes back onto the structural member's property set through the `ifcopenshell.api.<module>.<action>` authoring dispatch.
- geometry `structural.py`: the section-integral owner over `IfcProfileDef` composes `sectionproperties` for the warping, plastic, and shear FE closed-form numpy integrals cannot derive, staging `calculate_geometric_properties` then `calculate_warping_properties`/`calculate_plastic_properties` over the `create_mesh` triangulation.

[LOCAL_ADMISSION]:
- A meshed `Geometry`/`CompoundGeometry` profile enriches an IFC structural member's section receipt, admitted as a gated enrichment by rail policy; the closed-form spine stays independent of it.

[RAIL_LAW]:
- Package: `sectionproperties`
- Owns: arbitrary-profile cross-section warping, plastic, shear, and stress properties over a triangular FE mesh, beyond closed-form section integrals
- Accept: a meshed profile enriching an IFC structural member's section receipt
- Reject: a hand-rolled warping/plastic/shear FE solve or torsion-constant integration where sectionproperties owns it; a closed-form-only section owner claiming warping or plastic properties
