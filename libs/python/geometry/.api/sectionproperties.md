# [PY_GEOMETRY_API_SECTIONPROPERTIES]

`sectionproperties` supplies cross-section structural analysis for arbitrary 2D profiles: a `Geometry`/`CompoundGeometry` polygon model with holes and control points, a triangular finite-element mesh (`create_mesh`), and a `Section` solver computing geometric properties (area, centroids, second moments, principal axes), warping properties (torsion constant, shear/warping centers, shear areas), plastic properties (plastic moduli, shape factors), and per-point stress (`get_stress_at_points`) over a `pre.Material` library. It is a GATED ENRICHMENT row inside the geometry ifc/structural rail — it enriches IFC structural members with warping/plastic section receipts the cp315-clean spine (`ifcopenshell` plus numpy section integrals) cannot derive; the spine never depends on it. Deferred usage is tracked by card `geometry [STRUCTURAL_SECTION_PROPS_GATED] [BLOCKED]` (reference only).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `sectionproperties`
- package: `sectionproperties`
- import: `import sectionproperties`
- owner: `geometry`
- rail: ifc/structural / section-property-enrichment
- license: `MIT` (own); transitive copyleft — the mesh backend `cytriangle` (3.0.2) is `LGPLv3`, a dynamically-linked native triangulation extension pulled into the runtime, so the structural-enrichment owner accounts for LGPL obligations on the meshing dependency
- floor: `sectionproperties 3.10.2` declares Requires-Python `>=3.11` (no upper bound) and ships a pure-Python `py3-none-any` wheel that installs on any interpreter; its `requires_dist` lists `cytriangle` and `scipy` UNPINNED. The package's own metadata imposes NO ceiling and carries a `Programming Language :: Python :: 3.14` classifier — there is no `<3.14` gate from `sectionproperties` itself. The only native dep `cytriangle` resolves to 3.0.2, which ships cp311–cp314 wheels, so no cp314 wheel gap exists at the current resolved versions (only the older `cytriangle` 2.0.0 capped at cp313). It is a GATED ENRICHMENT row by rail policy — NEVER the spine
- spine boundary: the cp315-clean spine is `ifcopenshell` plus numpy section integrals; `sectionproperties` adds warping/plastic/shear receipts beyond closed-form integrals as a gated enrichment, never a spine dependency
- entry points: none (library only)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: geometry model (`sectionproperties.pre`)
- rail: section-property-enrichment

`Geometry` is the single-region 2D polygon model; `CompoundGeometry` is the multi-region assembly. Both carry control points and holes and produce a triangular mesh consumed by `Section`.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]      | [CAPABILITY]                                           |
| :-----: | :--------------------- | :----------------- | :----------------------------------------------------- |
|  [01]   | `pre.Geometry`         | single-region body | one polygon region with holes, control point, material |
|  [02]   | `pre.CompoundGeometry` | multi-region body  | union of `Geometry` regions for built-up sections      |
|  [03]   | `pre.Material`         | material           | elastic modulus, Poisson ratio, yield, density, color  |
|  [04]   | `pre.DEFAULT_MATERIAL` | material           | unit-property default material when none is assigned   |

[PUBLIC_TYPE_SCOPE]: solver and results (`sectionproperties.analysis` / `.post`)
- rail: section-property-enrichment

`Section` is the single solver bound to a meshed geometry; the property result carriers are returned by the calculate/getter entrypoints rather than constructed directly.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]     | [CAPABILITY]                                          |
| :-----: | :----------------------- | :---------------- | :---------------------------------------------------- |
|  [01]   | `analysis.Section`       | section solver    | geometric/warping/plastic property solver over a mesh |
|  [02]   | `post.SectionProperties` | property receipt  | accumulated geometric/warping/plastic property fields |
|  [03]   | `post.StressResult`      | stress receipt    | per-element stress arrays from a stress calculation   |
|  [04]   | `post.StressPost`        | stress aggregator | the stress-analysis result carrier from a load case   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: geometry construction and meshing (`pre.Geometry` / `pre.CompoundGeometry`)
- rail: section-property-enrichment

Construct a geometry from points or a DXF, subtract holes, then mesh with a maximum triangle area before binding it to a `Section`.

| [INDEX] | [SURFACE]                                                               | [ENTRY_FAMILY] | [CAPABILITY]                                                                                                          |
| :-----: | :---------------------------------------------------------------------- | :------------- | :-------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Geometry.from_points(points, facets, control_points, holes, material)` | factory        | polygon region from explicit point/facet rings                                                                        |
|  [02]   | `Geometry.from_dxf(dxf_filepath) -> Geometry \| CompoundGeometry`       | converter      | import a DXF profile into a geometry body                                                                             |
|  [03]   | `CompoundGeometry(geoms: list[Geometry])`                               | constructor    | assemble built-up section from regions                                                                                |
|  [04]   | `geometry - hole` (`__sub__`)                                           | operator       | subtract a polygon to register an interior void; holes are also supplied at construction via `from_points(holes=...)` |
|  [05]   | `geometry.create_mesh(mesh_sizes, ...) -> Geometry`                     | mesher         | triangulate region(s) at a max-area bound                                                                             |

[ENTRYPOINT_SCOPE]: property and stress calculation (`analysis.Section`)
- rail: section-property-enrichment

Bind the meshed geometry to a `Section`, then run the geometric pass first (warping and plastic passes depend on it), and query stress at points after a stress analysis.

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [CAPABILITY]                                           |
| :-----: | :------------------------------------------------------------ | :------------- | :----------------------------------------------------- |
|  [01]   | `Section(geometry: Geometry)`                                 | constructor    | solver bound to a meshed geometry                      |
|  [02]   | `Section.calculate_geometric_properties() -> None`            | solver         | area, centroid, second moments, principal axes         |
|  [03]   | `Section.calculate_warping_properties() -> None`              | solver         | torsion constant, shear/warping centers, shear areas   |
|  [04]   | `Section.calculate_plastic_properties() -> None`              | solver         | plastic centroids, plastic moduli, shape factors       |
|  [05]   | `Section.get_stress_at_points(pts, n, mxx, myy, ...) -> list` | query          | per-point stress under a normal force/moment load case |
|  [06]   | `Section.get_*()` (e.g. `get_area`/`get_ic`/`get_j`)          | accessor       | read computed scalars off the property receipt         |

## [04]-[IMPLEMENTATION_LAW]

[SECTION_TOPOLOGY]:
- import: `import sectionproperties.pre` / `sectionproperties.analysis` / `sectionproperties.post` at boundary scope only; module-level import is banned by the manifest import policy.
- geometry axis: `Geometry.from_points`/`from_dxf` build a single region, `CompoundGeometry` assembles built-up sections, interior voids are registered via the `-` operator (`geometry - hole`, `__sub__`) or the `from_points(holes=...)` constructor argument, and `create_mesh(mesh_sizes)` triangulates the region(s) at a maximum-area bound — the mesh is the prerequisite for every `Section` calculation.
- solver axis: `Section.calculate_geometric_properties` runs first and is the prerequisite for `calculate_warping_properties` (torsion constant, shear/warping centers, shear areas) and `calculate_plastic_properties` (plastic moduli, shape factors); each populates the `post.SectionProperties` receipt read back through the `Section.get_*` accessors.
- stress axis: `get_stress_at_points(pts, n, mxx, myy, ...)` evaluates section stress at sample points under a normal-force/moment load case once warping properties exist; the full-field stress pass returns a `post.StressPost`/`StressResult` carrier.
- evidence: each enrichment captures the geometric receipt (area, centroid, second moments, principal axes), the warping receipt (torsion constant `j`, shear centers, shear areas), the plastic receipt (plastic moduli, shape factors), and the per-point/full-field stress arrays as the structural section receipt the ifc/structural owner graduates.
- boundary: `sectionproperties` owns warping/plastic/shear cross-section receipts that exceed closed-form integrals; it is admitted as a gated enrichment by rail policy. Spine geometric section integrals (area, centroid, second moments) stay numpy over `ifcopenshell` geometry on cp315; IFC parse and tessellation stay `ifcopenshell`. The spine never depends on `sectionproperties`.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `sectionproperties`
- Owns: arbitrary-profile cross-section warping, plastic, shear, and stress properties over a triangular FE mesh, beyond closed-form section integrals
- Accept: a meshed `Geometry`/`CompoundGeometry` profile enriching an IFC structural member's section receipt, admitted as a gated enrichment by rail policy
- Reject: a spine dependency on this package (the cp315-clean spine is `ifcopenshell` plus numpy section integrals); a hand-rolled warping/plastic/shear-area solver or triangular FE section mesher where `sectionproperties` is admitted on a gated interpreter; closed-form geometric integrals (area, centroid, second moments) re-routed here when numpy owns the spine path

[CAPTURE_GAP]:
- floor: `sectionproperties 3.10.2` declares Requires-Python `>=3.11` (no upper bound) and ships a pure-Python `py3-none-any` wheel; `requires_dist` lists `cytriangle` and `scipy` UNPINNED. The package's own metadata imposes no interpreter ceiling and carries a `Programming Language :: Python :: 3.14` classifier. Its only native dep `cytriangle` resolves to 3.0.2 (cp311–cp314 wheels), so no cp314 wheel gap exists at current versions; only the older `cytriangle` 2.0.0 capped at cp313. Gating is a rail-policy enrichment decision, not a wheel-resolution blocker.
- members: derived from package reflection/source — the `pre.Geometry`/`CompoundGeometry` (`from_points`/`from_dxf`/`create_mesh`, with voids via the `-` operator or `from_points(holes=...)`), `pre.Material`, `analysis.Section` (`calculate_geometric_properties`/`calculate_warping_properties`/`calculate_plastic_properties`/`get_stress_at_points`), and `post` result carriers; confirm the exact `get_*` accessor set against `analysis.Section` source before any fence transcribes it
