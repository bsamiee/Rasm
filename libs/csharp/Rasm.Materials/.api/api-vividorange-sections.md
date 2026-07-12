# [RASM_MATERIALS_API_VIVIDORANGE_SECTIONS]

`VividOrange.Sections` owns the concrete reinforced-section and reinforcement data over the `VividOrange.ISections` interface floor. Its `Section` and `ConcreteSection` carriers bind profiles, materials, longitudinal bars, links, cover, and spacing policy; its reinforcement namespace owns the bar primitives, layout strategies, placement engines, and EC2 spacing rule.

The resulting `ConcreteSection` supplies the `IConcreteSection` consumed by the N-M-M capacity engine in `api-vividorange-interactiondiagram.md` and the transformed-section solver in `api-vividorange-sections-sectionproperties.md`. Reinforcement carries `VividOrange.Materials` `EnRebarMaterial` grades, placement returns `VividOrange.Geometry` `ILocalPoint2d` and `ILocalPolyline2d` values, and every type implements `ITaxonomySerializable` through `api-vividorange-serialization.md`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Sections`

- package: `VividOrange.Sections`
- license: MIT (`licenses.nuget.org/MIT` — MagmaWorks / VividOrange taxonomy)
- assembly: `VividOrange.Sections`
- namespace: `VividOrange.Sections` owns `Section` and `ConcreteSection`; `VividOrange.Sections.Reinforcement` owns the bar, layout, and layer engines; `VividOrange.Sections.Exceptions` owns the boundary throw types.
- asset: pure-managed AnyCPU runtime library with no native RID asset; the `net10.0` consumer binds the highest managed `lib/net8.0` asset from the `net8.0` / `net7.0` / `net6.0` / `netstandard2.0` / `net48` targets.
- rail: profiles / connection (reinforced section + reinforcement data)
- ABI floor: this PRE-1.0 section and reinforcement contract may break across a minor bump.
- section contracts: `ISection`, `IConcreteSection`, `IComposite`, `IRebar`, `ILink`, `ILongitudinalReinforcement`, `IReinforcementLayer`, `IFaceReinforcementLayer`, `IPerimeterReinforcementLayer`, `IReinforcementLayout`, `IReinforcementLayoutByCount`, `IReinforcementLayoutBySpacing`, `IMinimumReinforcementSpacing`, `SectionFace`, and `BarDiameter` live in the centrally pinned `VividOrange.ISections` floor.
- transitive floors: `VividOrange.IProfiles`, `VividOrange.Profiles.Perimeter`, and `VividOrange.Geometry` supply profile inputs and geometry returns; `VividOrange.IMaterials` supplies the grades registered in `api-vividorange-materials.md`; `UnitsNet` supplies quantities.
- serialization identity: `VividOrange.Serialization.ITaxonomySerializable` from `VividOrange.ISerialization` is distinct from `VividOrange.Taxonomy.Serialization.ITaxonomySerializable` from `VividOrange.Taxonomy.ISerialization`, which the packages in `api-vividorange-uncertainties.md` use. `TaxonomyJsonSerializer` does not serialize these section types, so the two serializer lanes remain distinct.

[RC_FLOOR_CLOSURE]: `VividOrange.Sections.Reinforcement` and `ConcreteSection` make the reinforced-concrete input path reachable from the admitted Materials set. `IRebar`, `ILink`, `ILongitudinalReinforcement`, and `SectionFace` resolve to `VividOrange.ISections`, while the concrete reinforcement types resolve to this assembly. A constructed `ConcreteSection` therefore activates the `.Utility` `Rebars` kernel and `ConcreteSectionProperties` carrier described in `api-vividorange-sections-sectionproperties.md`; the RC section-property and capacity rails are composable without an admission gate.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the section carriers (`IProfile` + `IMaterial` -> a section)

- rail: profiles / connection

| [INDEX] | [SYMBOL]          | [PACKAGE_ROLE]     |
| :-----: | :---------------- | :----------------- |
|  [01]   | `Section`         | section carrier    |
|  [02]   | `ConcreteSection` | RC section carrier |

[Section]: `ISection, ITaxonomySerializable`

- Shape: `{ IProfile Profile; IMaterial Material; }`.
- Inheritance: base of `ConcreteSection`.

[ConcreteSection]: `Section, IConcreteSection, ITaxonomySerializable`

- Fields: `Rebars` as the `IList<ILongitudinalReinforcement>` collected from added layers, `Link` as the `ILink` stirrup, `Cover` as `Length`, and `MinimumReinforcementSpacing`.
- Mutation: `AddRebarLayer` and `ClearRebars`.
- Consumer: the RC capacity and transformed-section solvers accept this `IConcreteSection`.

[PUBLIC_TYPE_SCOPE]: reinforcement bar primitives (`VividOrange.Sections.Reinforcement`)

- rail: connection (reinforcement) / profiles
- material: each bar carries the EN rebar grade as `IMaterial` and accepts a raw `Length` or catalogued `BarDiameter`.

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]    |
| :-----: | :-------------------------- | :---------------- |
|  [01]   | `Rebar`                     | bar primitive     |
|  [02]   | `Link`                      | stirrup primitive |
|  [03]   | `LongitudinalReinforcement` | positioned bar    |

[Rebar]: `IRebar, ITaxonomySerializable`

- Shape: `{ Length Diameter; IMaterial Material; }`.
- Constructors: `(IMaterial, Length)` and `(IMaterial, BarDiameter)`; `BarDiameter` carries the EN-10080 D6..D50 catalogue value.

[Link]: `Rebar, ILink, IRebar`

- Shape: a shear link or tie extends `Rebar` with `MinimumMandrelDiameter`.
- Constructors: `(IMaterial, Length)`, `(IMaterial, BarDiameter)`, and `(IRebar)`; the final overload promotes a bar to a link.

[LongitudinalReinforcement]: `ILongitudinalReinforcement, IComposite, ITaxonomySerializable`

- Shape: `{ ILocalPoint2d Position; IRebar Rebar; int CountPerBundle; }` places a bar at a section coordinate.
- Constructors: `(IRebar, ILocalPoint2d)` and `(IMaterial, Length, ILocalPoint2d)`.

[PUBLIC_TYPE_SCOPE]: reinforcement layout strategies + face/perimeter layer engines

- rail: connection (reinforcement) / profiles
- layout: `ReinforcementLayoutByCount` and `ReinforcementLayoutBySpacing` define bar count or spacing.
- layer: `FaceReinforcementLayer` and `PerimeterReinforcementLayer` bind a layout to a section face or perimeter; `GetPath(IProfile, offset)` derives the bar line and `GetRebars(path)` materializes positioned bars.

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]   |
| :-----: | :----------------------------- | :--------------- |
|  [01]   | `ReinforcementLayoutByCount`   | layout strategy  |
|  [02]   | `ReinforcementLayoutBySpacing` | layout strategy  |
|  [03]   | `FaceReinforcementLayer`       | placement engine |
|  [04]   | `PerimeterReinforcementLayer`  | placement engine |
|  [05]   | `MinimumReinforcementSpacing`  | spacing rule     |

[ReinforcementLayoutByCount]: `IReinforcementLayoutByCount, IReinforcementLayout, …`

- Shape: `{ int NumberOfBars; IRebar Rebar; }`.
- Constructor: `(IRebar, int numberOfBars)`.

[ReinforcementLayoutBySpacing]: `IReinforcementLayoutBySpacing, IReinforcementLayout, …`

- Shape: `{ Length MaximumSpacing; IRebar Rebar; }`.
- Constructor: `(IRebar, Length maxSpacing)`.

[FaceReinforcementLayer]: `IFaceReinforcementLayer, IReinforcementLayer, …`

- Shape: `{ IReinforcementLayout Layout; SectionFace Face; }` places bars along one section face.
- Constructors: `(SectionFace, IRebar, int numberOfRebars)` and `(SectionFace, IRebar, Length maxSpacing)`.
- Placement: `GetPath(IProfile, Length offset) -> ILocalPolyline2d` and `GetRebars(ILocalPolyline2d) -> IList<ILongitudinalReinforcement>`.

[PerimeterReinforcementLayer]: `IPerimeterReinforcementLayer, IReinforcementLayer, …`

- Shape: `{ IReinforcementLayout Layout; }` distributes bars around the section perimeter.
- Constructors: `(IRebar, int numberOfRebars)` and `(IRebar, Length maxSpacing)`.
- Placement: `GetPath` and `GetRebars` use the same contracts as the face layer.

[MinimumReinforcementSpacing]: `IMinimumReinforcementSpacing, ITaxonomySerializable`

- Policy: the EC2 minimum clear-spacing rule carries `BarDiameterFactor`, `AdditionalAggregateFactor`, `AbsoluteMinimumSpacing`, and `MaximumAggregateSize`.
- Constructors: `()` and `(NationalAnnex)`.
- Calculation: `GetMinimumReinforcementSpacing(Length barDiameter) -> Length`.

[PUBLIC_TYPE_SCOPE]: `VividOrange.Geometry` concrete section-plane carriers (the `ILocalPoint2d`/`ILocalPolyline2d` impls)

- rail: profiles / connection
- construction: a parametric `IProfile` perimeter assembles its outer and void loops from these carriers before `new Perimeter(outer, voids)`.
- coordinates: the section plane is Y-Z, so `LocalPoint2d` carries `{ Length Y; Length Z; }`, not an X-Y pair.
- boundary: interior code threads `ILocalPoint2d` and `ILocalPolyline2d`; the Materials parametric-section builder constructs the concrete `VividOrange.Geometry` carriers at the edge without introducing raw `double` coordinates or `Rhino.Geometry` types.

| [INDEX] | [SYMBOL]          | [PACKAGE_ROLE]      |
| :-----: | :---------------- | :------------------ |
|  [01]   | `LocalPoint2d`    | section-plane point |
|  [02]   | `LocalPolyline2d` | section-plane loop  |

[LocalPoint2d]: `ILocalPoint2d, ILocalCartesian2d<Length, Length>, IGeometryBase`

- Shape: `{ Length Y; Length Z; }` carries the Y-Z section coordinate.
- Constructors: `(Length y, Length z)`, `(double y, double z, LengthUnit unit)`, and copy construction from `(ILocalPoint2d)` or `(IPoint2d)`.

[LocalPolyline2d]: `ILocalPolyline2d, IGeometryBase, IPolylineBase<ILocalDomain2d, ILocalPoint2d>`

- Shape: `{ IList<ILocalPoint2d> Points; bool IsClosed; }`.
- Constructor: `(IList<ILocalPoint2d> points)` throws `ArgumentException` for fewer than two points.
- Derived reads: `GetArea() -> Area`, `GetBarycenter() -> LocalPoint2d`, `Offset(Length) -> ILocalPolyline2d`, and `Domain() -> ILocalDomain2d`.

[PUBLIC_TYPE_SCOPE]: boundary exceptions (`VividOrange.Sections.Exceptions`)

- rail: profiles / connection
- gate: the public `Exception` subclasses throw only at the section-construction boundary and do not constitute a `Fin` or `Validation` rail. The Materials owner traps them there and lowers them onto the canonical typed section error rail before entering an interior domain signature.

| [INDEX] | [SYMBOL]                       | [CONDITION]           |
| :-----: | :----------------------------- | :-------------------- |
|  [01]   | `InvalidMaterialTypeException` | illegal material type |
|  [02]   | `InvalidProfileTypeException`  | invalid face profile  |

`InvalidMaterialTypeException : Exception` guards a section or material whose `MaterialType` is illegal for the operation. `InvalidProfileTypeException : Exception` guards an `IProfile` shape rejected by `ValidateProfileForFaceReinforcement`.

The `internal` `VividOrange.Sections.Reinforcement.Utility` static class owns the layout geometry behind `GetPath` and `GetRebars`; consumers call the public layer surfaces.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: build a section

- rail: profiles / connection

| [INDEX] | [ENTRY]         | [CALL_SHAPE] |
| :-----: | :-------------- | :----------- |
|  [01]   | section         | constructor  |
|  [02]   | bare RC section | constructor  |
|  [03]   | RC rebar link   | constructor  |
|  [04]   | RC stirrup link | constructor  |
|  [05]   | add rebar layer | method       |
|  [06]   | clear rebar     | method       |
|  [07]   | collected rebar | property     |

[SECTION]: `new Section(IProfile profile, IMaterial material)` builds a plain section from a `CatalogueFactory` or parametric profile and an EN material grade.

[BARE_RC]: `new ConcreteSection(IProfile, IMaterial)` builds an RC section without links or reinforcement layers.

[REBAR_LINK_RC]: `new ConcreteSection(IProfile, IMaterial, IRebar link[, Length cover[, IList<ILongitudinalReinforcement> rebars]])` promotes the link bar to `Link` and accepts optional cover and explicit bars.

[STIRRUP_LINK_RC]: `new ConcreteSection(IProfile, IMaterial, ILink link[, Length cover[, IList<ILongitudinalReinforcement> rebars]])` accepts an explicit stirrup with optional cover and bars.

[ADD_LAYER]: `concreteSection.AddRebarLayer(IReinforcementLayer layer)` accepts a face or perimeter layer. The section routes `Top` and `Bottom` to the top-bottom set, `Left`, `Right`, and `Sides` to the side set, and a perimeter layer to the perimeter set.

[CLEAR_REBAR]: `concreteSection.ClearRebars()` clears every added reinforcement layer.

[COLLECTED_REBAR]: `concreteSection.Rebars` returns the `IList<ILongitudinalReinforcement>` materialized from the layers for the RC capacity and transformed-section solvers.

[ENTRYPOINT_SCOPE]: build the reinforcement

- rail: connection (reinforcement) / profiles

| [INDEX] | [ENTRY]             | [CALL_SHAPE]         |
| :-----: | :------------------ | :------------------- |
|  [01]   | bar                 | constructor          |
|  [02]   | link                | constructor          |
|  [03]   | positioned bar      | constructor          |
|  [04]   | face count layer    | constructor          |
|  [05]   | face spacing layer  | constructor          |
|  [06]   | perimeter layer     | constructor          |
|  [07]   | placement path      | method               |
|  [08]   | positioned bar list | method               |
|  [09]   | minimum spacing     | constructor + method |

[BAR]: `new Rebar(IMaterial material, BarDiameter diameter)` or `new Rebar(IMaterial, Length)` builds one bar from an `EnRebarMaterial` and a catalogued EN-10080 diameter or raw length.

[LINK]: `new Link(IMaterial, BarDiameter)` builds a stirrup with a minimum mandrel diameter, while `new Link(IRebar)` promotes an existing bar.

[POSITIONED_BAR]: `new LongitudinalReinforcement(IRebar rebar, ILocalPoint2d position)` places a bar at an explicit section coordinate for a manual layout.

[FACE_COUNT]: `new FaceReinforcementLayer(SectionFace face, IRebar rebar, int numberOfRebars)` creates a count-based layer on `Top`, `Left`, `Right`, `Bottom`, or `Sides`.

[FACE_SPACING]: `new FaceReinforcementLayer(SectionFace face, IRebar rebar, Length maxSpacing)` creates a spacing-based layer on one face.

[PERIMETER]: `new PerimeterReinforcementLayer(IRebar rebar, int numberOfRebars)` and `(IRebar, Length maxSpacing)` distribute a count-based or spacing-based layer around the perimeter.

[PATH]: `layer.GetPath(IProfile profile, Length offset) -> ILocalPolyline2d` derives the bar centroid line by offsetting the face or perimeter inward by cover plus the bar radius.

[REBARS]: `layer.GetRebars(ILocalPolyline2d path) -> IList<ILongitudinalReinforcement>` materializes positioned bars along the path.

[MINIMUM_SPACING]: `new MinimumReinforcementSpacing(NationalAnnex na)` selects the annex, and `.GetMinimumReinforcementSpacing(Length barDiameter) -> Length` returns the EC2 minimum clear spacing.

[ENTRYPOINT_SCOPE]: build the section-plane geometry (`VividOrange.Geometry`)

- rail: profiles / connection
- note: a parametric `IProfile` perimeter assembles its outer and void loops from these concrete carriers before `new Perimeter(outer, voids)`; positions use Y-Z `Length` coordinates.

| [INDEX] | [ENTRY]       | [CALL_SHAPE] |
| :-----: | :------------ | :----------- |
|  [01]   | section point | constructor  |
|  [02]   | section loop  | constructor  |

[SECTION_POINT]: `new LocalPoint2d(Length y, Length z)` builds the `{ Length Y; Length Z; }` carrier at a Y-Z coordinate.

[SECTION_LOOP]: `new LocalPolyline2d(IList<ILocalPoint2d> points)` accepts an ordered `List<ILocalPoint2d>` and throws `ArgumentException` for fewer than two points.

## [04]-[IMPLEMENTATION_LAW]

[SECTION_ALGEBRA]:

- root: `Section(IProfile, IMaterial)` is the section identity — a profile (the geometry, from `CatalogueFactory` or a
  parametric `IProfile`) plus a material (an EN grade). `ConcreteSection: Section` extends it with the reinforcement
  payload (rebar layers, links, cover, min-spacing).
- reinforcement composition: a `ConcreteSection` is built bare, then `AddRebarLayer(IReinforcementLayer)` adds
  face/perimeter LAYERS; the section discriminates the layer (`IFaceReinforcementLayer` vs
  `IPerimeterReinforcementLayer`) and the `SectionFace` to route bars into its top/bottom/side/perimeter sets. `Rebars`
  is the COLLECTED positioned-bar list (materialized from the layers via each layer's `GetPath`/`GetRebars`) — the
  iterable the RC solvers read.
- layout vs layer: a LAYOUT (`...ByCount`/`...BySpacing`) is the bar RULE (how many / what spacing); a LAYER binds a
  layout to a face/perimeter and is the placement ENGINE. The two-step split keeps the bar rule independent of where it
  is applied — one `ReinforcementLayoutByCount` reused on multiple faces.
- units: diameters/spacing/cover are `UnitsNet.Length`, factors `UnitsNet.Ratio`; positions are `VividOrange.Geometry`
  `ILocalPoint2d` and bar paths `ILocalPolyline2d` — no raw `double` on the surface. The `BarDiameter` enum is the
  EN-10080 D6..D50 catalogue (a catalogued diameter resolves to a `Length`).

[BOUNDARY_EXCEPTION_LAW]:

- Section construction + layout validation throw `InvalidMaterialTypeException` / `InvalidProfileTypeException` at the
  boundary (an illegal material type for the section, an `IProfile` shape that has no valid face-reinforcement layout).
  These are NOT a typed rail. A Materials RC owner traps them at the in-folder boundary and lowers them onto the
  canonical typed section error rail (`LanguageExt.Fin`) — the throw NEVER reaches an interior domain signature.

[LOCAL_ADMISSION]:

- The reinforced section + reinforcement is admitted through the Materials boundary that owns the RC section + the
  `Connection/reinforcement` seam: build a `ConcreteSection` from an `IProfile` (Profiles) + an `EnConcreteMaterial`
  (Materials) + an `EnRebarMaterial`-backed `Rebar`/layer arrangement, mapped onto the canonical Materials
  `ConnectionItem` (`ConnectionFamily.Reinforcement`) at the edge — replacing a hand-rolled `BarSize`/`RebarSection`
  table.
- A bar diameter is read from the `BarDiameter` catalogue, never a hand-keyed mm literal; a min clear spacing from
  `MinimumReinforcementSpacing.GetMinimumReinforcementSpacing`, never an inline EC2 constant.

[STACK]:

- material seam: every `Rebar`/`Link` carries an `IMaterial` — the admitted `VividOrange.Materials` `EnRebarMaterial`
  (`MaterialType.Reinforcement`, `api-vividorange-materials.md`); the `ConcreteSection.Material` is an
  `EnConcreteMaterial`. The Sections reinforcement geometry and the Materials grade DATA meet at the `IMaterial`
  contract — a bar carries a registered EN grade, never a hand-keyed `f_yk`.
- profile seam: the `IProfile` input is the admitted `VividOrange.Profiles.Catalogue` `CatalogueFactory` output (an
  AISC/EN section) OR a parametric `IProfile` (`api-vividorange-profiles-catalogue.md`); the layer engines derive the
  bar paths over its `IPerimeter` — so reinforcement places identically over a catalogued or a parametric section.
- section-property seam: the `ConcreteSection` minted here IS the `IConcreteSection` the
  `ConcreteSectionProperties` transformed-section solver requires (`api-vividorange-sections-sectionproperties.md`,
  [RC_FLOOR_CLOSURE]) — this admission closes the RC section-property path the solver's `[ADMISSION_GATE]` note
  wrongly treats as unreachable; the `.Utility` `Rebars` kernel and the `ConcreteSectionProperties` carrier are
  callable over a section built here.
- capacity seam: the `ConcreteSection` (+ its `Rebars`) IS the `IConcreteSection` the `VividOrange.InteractionDiagram`
  N-M-M capacity engine constructs from (`api-vividorange-interactiondiagram.md`); the engine reads `section.Profile`,
  `section.Material`, and each `section.Rebars[i].Rebar.{Diameter, Material}` — so the section built here drives the
  biaxial capacity hull directly.
- geometry seam: bar positions are `VividOrange.Geometry` `ILocalPoint2d` and layer paths `ILocalPolyline2d` (the
  transitive floor) — the same 2D local-section coordinate space the `SectionProperties` `Centroid`/`Extends`
  returns live in.
- wire seam: every section + bar type is `ITaxonomySerializable` — an RC section round-trips through
  `VividOrange.Serialization` `ToJson<T>`/`FromJson<T>` (`api-vividorange-serialization.md`) over the `UnitsNet`
  Json.NET converter (`api-vividorange-serialization.md` [TRANSITIVE_UNITSNET_JSONNET]), preserving the polymorphic bar/layer runtime types via
  the `$type` tag, so a TS/Python peer reconstructs the exact reinforcement arrangement.

[RAIL_LAW]:

- Package: `VividOrange.Sections` (MIT, pure-managed AnyCPU, `net10.0` binds `net8.0`, PRE-1.0 contract)
- Owns: the concrete reinforced-section + reinforcement DATA — the `Section`/`ConcreteSection` carriers, the
  `Rebar`/`Link`/`LongitudinalReinforcement` bar primitives over the EN-10080 `BarDiameter` catalogue, the
  `ReinforcementLayoutByCount`/`BySpacing` strategies, the `FaceReinforcementLayer`/`PerimeterReinforcementLayer`
  bar-placement engines (`GetPath`/`GetRebars` over an `IProfile`), and the `MinimumReinforcementSpacing` EC2 rule —
  the concrete impl of the `VividOrange.ISections` floor, every type `ITaxonomySerializable`. This admission CLOSES
  the RC reinforcement floor for the whole VividOrange section pipeline ([RC_FLOOR_CLOSURE]).
- Accept: an RC `ConcreteSection` built from an admitted `IProfile` (Profiles) + an `EnConcreteMaterial`/`EnRebarMaterial`
  (Materials) + a face/perimeter rebar-layer arrangement, mapped onto the canonical Materials RC section /
  `ConnectionFamily.Reinforcement` owner at the boundary; diameters from the `BarDiameter` catalogue, spacing from the
  EC2 `MinimumReinforcementSpacing` rule; the section consumed as the `IConcreteSection` input to the
  `SectionProperties`/`InteractionDiagram` RC solvers.
- Reject: a hand-rolled `BarSize`/`RebarSection`/diameter literal where the `BarDiameter` catalogue + `Rebar` carry it;
  a raw-`double` read of a `Length` diameter/spacing/cover; treating the RC path as admission-gated (this admission IS
  the closure, [RC_FLOOR_CLOSURE]); calling the `internal` `Reinforcement.Utility` layout mechanism directly; an
  `InvalidMaterialTypeException`/`InvalidProfileTypeException` left to propagate into an interior domain signature
  instead of being lowered onto the typed section rail.
