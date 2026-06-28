# [RASM_MATERIALS_API_VIVIDORANGE_SECTIONS]

`VividOrange.Sections` supplies the concrete reinforced-section + reinforcement DATA owner — the concrete
implementation of the `VividOrange.ISections` interface floor. It carries (a) the `Section` (`IProfile` +
`IMaterial`) and `ConcreteSection : Section, IConcreteSection` (profile + concrete material + longitudinal
rebar + links + cover) section classes, and (b) the full `VividOrange.Sections.Reinforcement` namespace — the
`Rebar`/`Link` bar primitives (over the EN-10080 `BarDiameter` catalogue), the `LongitudinalReinforcement`
positioned-bar, the `ReinforcementLayoutByCount`/`ReinforcementLayoutBySpacing` layout strategies, the
`FaceReinforcementLayer`/`PerimeterReinforcementLayer` rebar-PLACEMENT engines (`GetPath`/`GetRebars` over an
`IProfile` perimeter), and the `MinimumReinforcementSpacing` EC2 spacing rule. It is the package that CLOSES
the reinforced-concrete input the rest of the RC pipeline needs: the `ConcreteSection` it mints is the
`IConcreteSection` the `VividOrange.InteractionDiagram` N-M-M capacity engine consumes
(`api-vividorange-interactiondiagram.md`) and the `ConcreteSectionProperties` transformed-section solver requires
(`api-vividorange-sections-sectionproperties.md`) — so this admission makes the full RC section-property and
capacity path constructible end-to-end. The reinforcement carries `IMaterial` grades from the admitted
`VividOrange.Materials` (`EnRebarMaterial`, `api-vividorange-materials.md`); the geometry returns are
`VividOrange.Geometry` `ILocalPoint2d`/`ILocalPolyline2d`; every type is `ITaxonomySerializable`
(`api-vividorange-serialization.md`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Sections`
- package: `VividOrange.Sections`
- version: `0.1.0`
- license: MIT (`licenses.nuget.org/MIT` — MagmaWorks / VividOrange taxonomy)
- assembly: `VividOrange.Sections`
- namespace: `VividOrange.Sections` (`Section`/`ConcreteSection`), `VividOrange.Sections.Reinforcement` (the bar +
  layout + layer engines), `VividOrange.Sections.Exceptions` (the boundary throw types)
- asset: runtime library, pure-managed AnyCPU, NO native RID asset. Multi-TFM `net8.0` / `net7.0` / `net6.0` /
  `netstandard2.0` / `net48`; the consumer `net10.0` binds the highest managed asset `lib/net8.0` (`net8.0` is the
  bound TFM — no `net9.0`+ asset, so the `api resolve` primary `net8.0` IS the consumed surface).
- rail: profiles / connection (reinforced section + reinforcement data)
- ABI floor: a `0.1.0` PRE-1.0 contract — the section + reinforcement member set may break across a minor bump. The
  interface floor (`ISection`, `IConcreteSection`, `IComposite`, and the full reinforcement contract set `IRebar`,
  `ILink`, `ILongitudinalReinforcement`, `IReinforcementLayer`/`IFaceReinforcementLayer`/`IPerimeterReinforcementLayer`,
  `IReinforcementLayout`/`IReinforcementLayoutByCount`/`IReinforcementLayoutBySpacing`, `IMinimumReinforcementSpacing`)
  AND the `SectionFace` + `BarDiameter` enums live in the transitive `VividOrange.ISections` `0.1.0` floor, centrally
  pinned. The `IProfile`/`IPerimeter` inputs and the `ILocalPoint2d`/`ILocalPolyline2d` geometry returns come from the
  `VividOrange.IProfiles` / `VividOrange.Profiles.Perimeter` / `VividOrange.Geometry` (`1.8.0`) floor; the `IMaterial`
  grades from `VividOrange.IMaterials` (`api-vividorange-materials.md`). `UnitsNet` `5.75.0` is the shared quantity
  floor; `ITaxonomySerializable` is the `VividOrange.ISerialization` floor contract. That floor marker's FQN is the `0.1.0`
`VividOrange.Serialization.ITaxonomySerializable` (assembly `VividOrange.ISerialization`) — a DISTINCT CLR type
identity from the `0.2.0` `VividOrange.Taxonomy.Serialization.ITaxonomySerializable` (assembly
`VividOrange.Taxonomy.ISerialization`) the `VividOrange.Uncertainties` packages ride
(`api-vividorange-uncertainties.md`); the `0.1.0` `TaxonomyJsonSerializer` does NOT serialize the `0.2.0` types, so a
Materials design page never assumes one shared VividOrange serializer covers both lanes.

[RC_FLOOR_CLOSURE]: this package SHIPS the `VividOrange.Sections.Reinforcement` namespace and the `ConcreteSection`
concrete, so the reinforced-concrete input path IS reachable from the admitted Materials set. This SUPERSEDES the
`api-vividorange-sections-sectionproperties.md` `[ADMISSION_GATE]` note (which predates this admission and wrongly
treats the reinforcement floor as unrestored): `IRebar`/`ILink`/`ILongitudinalReinforcement`/`SectionFace` resolve
to `VividOrange.ISections` (admitted) and the `Reinforcement.*` concretes resolve to THIS assembly. The `Rebars`
kernel in `api-vividorange-sections-sectionproperties.md` `.Utility` and the `ConcreteSectionProperties` carrier are
therefore CALLABLE once a `ConcreteSection` is built here — the RC section-property and capacity rails are first-class
composable, NOT admission-gated.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the section carriers (`IProfile` + `IMaterial` -> a section)
- rail: profiles / connection

| [INDEX] | [SYMBOL]          | [PACKAGE_ROLE]      | [CAPABILITY]                                                                                  |
| :-----: | :---------------- | :------------------ | :-------------------------------------------------------------------------------------------- |
|  [01]   | `Section`         | section carrier     | `ISection, ITaxonomySerializable` — the plain `{ IProfile Profile; IMaterial Material; }` section (a profile + a material); the base of `ConcreteSection` |
|  [02]   | `ConcreteSection` | RC section carrier  | `: Section, IConcreteSection, ITaxonomySerializable` — adds `Rebars` (`IList<ILongitudinalReinforcement>`, collected from the added layers), `Link` (`ILink` stirrup), `Cover` (`Length`), `MinimumReinforcementSpacing`; mutated via `AddRebarLayer`/`ClearRebars` — the `IConcreteSection` the RC capacity + transformed-section solvers consume |

[PUBLIC_TYPE_SCOPE]: reinforcement bar primitives (`VividOrange.Sections.Reinforcement`)
- rail: connection (reinforcement) / profiles
- The bar carriers — a single bar (`Rebar`), a stirrup/tie (`Link`), and a positioned bar (`LongitudinalReinforcement`).
  Each takes an `IMaterial` (the EN rebar grade) and a diameter (a raw `Length` or a catalogued `BarDiameter`).

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]    | [CAPABILITY]                                                                                  |
| :-----: | :-------------------------- | :---------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `Rebar`                     | bar primitive     | `IRebar, ITaxonomySerializable` — `{ Length Diameter; IMaterial Material; }`; ctors `(IMaterial, Length)` and `(IMaterial, BarDiameter)` (the EN-10080 D6..D50 catalogue diameter) |
|  [02]   | `Link`                      | stirrup primitive | `: Rebar, ILink, IRebar` — a `Rebar` plus `MinimumMandrelDiameter`; the shear-link / tie bar; ctors `(IMaterial, Length)`, `(IMaterial, BarDiameter)`, `(IRebar)` (promote a bar to a link) |
|  [03]   | `LongitudinalReinforcement` | positioned bar    | `ILongitudinalReinforcement, IComposite, ITaxonomySerializable` — `{ ILocalPoint2d Position; IRebar Rebar; int CountPerBundle; }`; ctors `(IRebar, ILocalPoint2d)` and `(IMaterial, Length, ILocalPoint2d)` — a bar placed at a section coordinate |

[PUBLIC_TYPE_SCOPE]: reinforcement layout strategies + face/perimeter layer engines
- rail: connection (reinforcement) / profiles
- A LAYOUT (`...LayoutByCount`/`...BySpacing`) is the rule for how many bars / what spacing; a LAYER
  (`FaceReinforcementLayer`/`PerimeterReinforcementLayer`) binds a layout to a section face or perimeter and is the
  rebar-PLACEMENT engine — `GetPath(IProfile, offset)` derives the bar line and `GetRebars(path)` materializes the
  positioned `LongitudinalReinforcement` bars on it.

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]   | [CAPABILITY]                                                                                  |
| :-----: | :-------------------------------- | :--------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `ReinforcementLayoutByCount`      | layout strategy  | `IReinforcementLayoutByCount, IReinforcementLayout, …` — `{ int NumberOfBars; IRebar Rebar; }`; ctor `(IRebar, int numberOfBars)` |
|  [02]   | `ReinforcementLayoutBySpacing`    | layout strategy  | `IReinforcementLayoutBySpacing, IReinforcementLayout, …` — `{ Length MaximumSpacing; IRebar Rebar; }`; ctor `(IRebar, Length maxSpacing)` |
|  [03]   | `FaceReinforcementLayer`          | placement engine | `IFaceReinforcementLayer, IReinforcementLayer, …` — `{ IReinforcementLayout Layout; SectionFace Face; }`; ctors `(SectionFace, IRebar, int numberOfRebars)` / `(SectionFace, IRebar, Length maxSpacing)`; `GetPath(IProfile, Length offset) -> ILocalPolyline2d` + `GetRebars(ILocalPolyline2d) -> IList<ILongitudinalReinforcement>` — places bars along one section face |
|  [04]   | `PerimeterReinforcementLayer`     | placement engine | `IPerimeterReinforcementLayer, IReinforcementLayer, …` — `{ IReinforcementLayout Layout; }`; ctors `(IRebar, int numberOfRebars)` / `(IRebar, Length maxSpacing)`; `GetPath`/`GetRebars` as above — distributes bars around the whole section perimeter |
|  [05]   | `MinimumReinforcementSpacing`     | spacing rule     | `IMinimumReinforcementSpacing, ITaxonomySerializable` — the EC2 min clear-spacing rule (`BarDiameterFactor`/`AdditionalAggregateFactor`/`AbsoluteMinimumSpacing`/`MaximumAggregateSize`); `GetMinimumReinforcementSpacing(Length barDiameter) -> Length`; ctors `()` and `(NationalAnnex)` |

[PUBLIC_TYPE_SCOPE]: boundary exceptions (`VividOrange.Sections.Exceptions`)
- rail: profiles / connection
- gate: these are public `Exception` subclasses THROWN at the section-construction boundary — they are NOT a typed
  `Fin`/`Validation` rail. A Materials owner traps them at the in-folder boundary and lowers them onto the canonical
  typed section error rail; the throw NEVER propagates into an interior domain signature.

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]   | [CAPABILITY]                                                                  |
| :-----: | :----------------------------- | :--------------- | :--------------------------------------------------------------------------- |
|  [01]   | `InvalidMaterialTypeException` | boundary throw   | `: Exception` — thrown when a section/material's `MaterialType` is illegal for the operation |
|  [02]   | `InvalidProfileTypeException`  | boundary throw   | `: Exception` — thrown when an `IProfile` shape is invalid for a face-reinforcement layout (`ValidateProfileForFaceReinforcement`) |

The `VividOrange.Sections.Reinforcement.Utility` static class is `internal` (the layout-geometry mechanism behind
`GetPath`/`GetRebars`) — it is NOT a consumer-callable surface.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: build a section
- rail: profiles / connection

| [INDEX] | [SURFACE]                                                                                            | [CALL_SHAPE]   | [CAPABILITY]                                                                    |
| :-----: | :--------------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------------------------- |
|  [01]   | `new Section(IProfile profile, IMaterial material)`                                                  | constructor    | a plain section (the `IProfile` from `CatalogueFactory`/a parametric profile + an EN material grade) |
|  [02]   | `new ConcreteSection(IProfile, IMaterial)`                                                           | constructor    | a bare RC section (no link/rebar yet — add layers below)                         |
|  [03]   | `new ConcreteSection(IProfile, IMaterial, IRebar link[, Length cover[, IList<ILongitudinalReinforcement> rebars]])` | constructor | an RC section with a link bar (promoted to `Link`), optional cover + explicit bars |
|  [04]   | `new ConcreteSection(IProfile, IMaterial, ILink link[, Length cover[, IList<ILongitudinalReinforcement> rebars]])`  | constructor | an RC section with an explicit `Link` stirrup, optional cover + explicit bars   |
|  [05]   | `concreteSection.AddRebarLayer(IReinforcementLayer layer)`                                           | method         | add a `FaceReinforcementLayer`/`PerimeterReinforcementLayer` — the section dispatches on the layer kind + `SectionFace` (`Top`/`Bottom` -> top-bottom set, `Left`/`Right`/`Sides` -> side set, perimeter -> perimeter set) to its rebar collection |
|  [06]   | `concreteSection.ClearRebars()`                                                                      | method         | clear all added rebar layers                                                    |
|  [07]   | `concreteSection.Rebars`                                                                             | property       | `IList<ILongitudinalReinforcement>` — the collected positioned bars (materialized from the added layers) — the input the RC capacity/transformed-section solvers iterate |

[ENTRYPOINT_SCOPE]: build the reinforcement
- rail: connection (reinforcement) / profiles

| [INDEX] | [SURFACE]                                                                                            | [CALL_SHAPE]   | [CAPABILITY]                                                                    |
| :-----: | :--------------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------------------------- |
|  [01]   | `new Rebar(IMaterial material, BarDiameter diameter)` / `new Rebar(IMaterial, Length)`               | constructor    | a single bar at a catalogued EN-10080 diameter (or a raw `Length`); the material is an `EnRebarMaterial` |
|  [02]   | `new Link(IMaterial, BarDiameter)` / `new Link(IRebar)`                                              | constructor    | a stirrup/tie bar (with a min mandrel diameter); promote an `IRebar` to a `Link` |
|  [03]   | `new LongitudinalReinforcement(IRebar rebar, ILocalPoint2d position)`                                | constructor    | a bar placed at an explicit section coordinate (for a manual bar layout)        |
|  [04]   | `new FaceReinforcementLayer(SectionFace face, IRebar rebar, int numberOfRebars)`                     | constructor    | a count-based layer on one section face (`SectionFace` = `Top`/`Left`/`Right`/`Bottom`/`Sides`) |
|  [05]   | `new FaceReinforcementLayer(SectionFace face, IRebar rebar, Length maxSpacing)`                      | constructor    | a spacing-based layer on one section face                                       |
|  [06]   | `new PerimeterReinforcementLayer(IRebar rebar, int numberOfRebars)` / `(IRebar, Length maxSpacing)`  | constructor    | a count/spacing layer distributed around the whole perimeter                     |
|  [07]   | `layer.GetPath(IProfile profile, Length offset) -> ILocalPolyline2d`                                 | method         | the bar centroid line (the face/perimeter offset inward by cover + bar radius)   |
|  [08]   | `layer.GetRebars(ILocalPolyline2d path) -> IList<ILongitudinalReinforcement>`                        | method         | materialize the positioned `LongitudinalReinforcement` bars along the path       |
|  [09]   | `new MinimumReinforcementSpacing(NationalAnnex na)` ; `.GetMinimumReinforcementSpacing(Length barDiameter) -> Length` | constructor + method | the EC2 min clear bar spacing for the annex + bar diameter |

## [04]-[IMPLEMENTATION_LAW]

[SECTION_ALGEBRA]:
- root: `Section(IProfile, IMaterial)` is the section identity — a profile (the geometry, from `CatalogueFactory` or a
  parametric `IProfile`) plus a material (an EN grade). `ConcreteSection : Section` extends it with the reinforcement
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
  transitive `1.8.0` floor) — the same 2D local-section coordinate space the `SectionProperties` `Centroid`/`Extends`
  returns live in.
- wire seam: every section + bar type is `ITaxonomySerializable` — an RC section round-trips through
  `VividOrange.Serialization` `ToJson<T>`/`FromJson<T>` (`api-vividorange-serialization.md`) over the `UnitsNet`
  Json.NET converter (`api-unitsnet-serialization-jsonnet.md`), preserving the polymorphic bar/layer runtime types via
  the `$type` tag, so a TS/Python peer reconstructs the exact reinforcement arrangement.

[RAIL_LAW]:
- Package: `VividOrange.Sections` `0.1.0` (MIT, pure-managed AnyCPU, `net10.0` binds `net8.0`, PRE-1.0 contract)
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
