# [RASM_MATERIALS_API_VIVIDORANGE_SECTIONS]

`VividOrange.Sections` owns the concrete reinforced-section and reinforcement data over the `VividOrange.ISections` interface floor: `Section` and `ConcreteSection` bind profile, material, longitudinal bars, links, cover, and spacing policy, and the `VividOrange.Sections.Reinforcement` namespace owns the bar primitives, layout strategies, placement engines, and the EC2 minimum-spacing rule. Every type carries `ITaxonomySerializable`; bar positions and layer paths ride `VividOrange.Geometry` `ILocalPoint2d`/`ILocalPolyline2d`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Sections`
- package: `VividOrange.Sections` (MIT)
- assembly: `VividOrange.Sections`
- namespace: `VividOrange.Sections` owns `Section`/`ConcreteSection`; `.Reinforcement` owns the bar, layout, and layer engines; `.Exceptions` owns boundary throw types
- asset: pure-managed AnyCPU, no native RID asset; the `net10.0` consumer binds the `lib/net8.0` managed asset
- rail: profiles / connection — the reinforced section and reinforcement data
- floor: `ISections` interface contracts and the `BarDiameter`/`SectionFace` vocabulary resolve to the centrally pinned `VividOrange.ISections` package; the concrete carriers resolve to this assembly
- depends: `VividOrange.IProfiles` / `VividOrange.Profiles.Perimeter` / `VividOrange.Geometry` supply profile inputs and geometry returns, `VividOrange.IMaterials` the grades, `UnitsNet` the quantities

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the section carriers — an `IProfile` and an `IMaterial`

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]                           |
| :-----: | :---------------- | :------------ | :------------------------------------- |
|  [01]   | `Section`         | class         | profile + material section identity    |
|  [02]   | `ConcreteSection` | class         | RC section: bars, link, cover, spacing |

- `ConcreteSection.Rebars` re-materializes the `IList<ILongitudinalReinforcement>` from the layers on each read, never a stored field.

[PUBLIC_TYPE_SCOPE]: reinforcement (`VividOrange.Sections.Reinforcement`)

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                           |
| :-----: | :----------------------------- | :------------ | :------------------------------------- |
|  [01]   | `Rebar`                        | class         | bar primitive over an EN rebar grade   |
|  [02]   | `Link`                         | class         | stirrup/tie extending `Rebar`          |
|  [03]   | `LongitudinalReinforcement`    | class         | bar positioned at a section coordinate |
|  [04]   | `ReinforcementLayoutByCount`   | class         | bar-count rule                         |
|  [05]   | `ReinforcementLayoutBySpacing` | class         | bar-spacing rule                       |
|  [06]   | `FaceReinforcementLayer`       | class         | places a layout along one section face |
|  [07]   | `PerimeterReinforcementLayer`  | class         | places a layout around the perimeter   |
|  [08]   | `MinimumReinforcementSpacing`  | class         | EC2 minimum clear-spacing rule         |
|  [09]   | `BarDiameter`                  | enum          | EN-10080 D6..D50 diameter catalogue    |

- `BarDiameter` and `SectionFace` are `VividOrange.ISections` floor types; the rest resolve to this assembly.

[PUBLIC_TYPE_SCOPE]: section-plane geometry carriers (`VividOrange.Geometry`)

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [CAPABILITY]              |
| :-----: | :---------------- | :------------ | :------------------------ |
|  [01]   | `LocalPoint2d`    | class         | Y-Z section-plane point   |
|  [02]   | `LocalPolyline2d` | class         | closed section-plane loop |

- `LocalPoint2d` carries `{ Length Y; Length Z; }` in the Y-Z section plane, not an X-Y pair.
- `LocalPolyline2d` derives `GetArea() -> Area`, `GetBarycenter() -> LocalPoint2d`, `Offset(Length) -> ILocalPolyline2d`, and `Domain() -> ILocalDomain2d`.
- Interior code threads the `ILocalPoint2d`/`ILocalPolyline2d` interfaces; the Materials parametric-section builder constructs the concrete carriers at the edge, never raw `double` coordinates or `Rhino.Geometry` types.

[PUBLIC_TYPE_SCOPE]: boundary exceptions (`VividOrange.Sections.Exceptions`)

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :----------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `InvalidMaterialTypeException` | class         | illegal material type at construction           |
|  [02]   | `InvalidProfileTypeException`  | class         | profile shape with no face-reinforcement layout |

- Both are plain `Exception` subclasses thrown at the section-construction boundary, not a `Fin`/`Validation` rail; `InvalidProfileTypeException.ValidateProfileForFaceReinforcement(IProfile)` gates the profile.
- `Reinforcement.Utility` stays `internal`, owning the `GetPath`/`GetRebars` layout geometry; consumers call the public layer surfaces.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: build a section

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :------------------------------------------------------------------ | :------- | :--------------------------------- |
|  [01]   | `new Section(IProfile, IMaterial)`                                  | ctor     | plain section                      |
|  [02]   | `new ConcreteSection(IProfile, IMaterial)`                          | ctor     | bare RC section                    |
|  [03]   | `new ConcreteSection(IProfile, IMaterial, IRebar, Length?, IList?)` | ctor     | RC section, bar promoted to `Link` |
|  [04]   | `new ConcreteSection(IProfile, IMaterial, ILink, Length?, IList?)`  | ctor     | RC section with explicit stirrup   |
|  [05]   | `ConcreteSection.AddRebarLayer(IReinforcementLayer)`                | instance | route a face or perimeter layer    |
|  [06]   | `ConcreteSection.ClearRebars()`                                     | instance | clear every added layer            |
|  [07]   | `ConcreteSection.Rebars`                                            | property | collected positioned bars          |

- `AddRebarLayer` routes `Top`/`Bottom` to the top-bottom set, `Left`/`Right` to the side set, `Sides` to both sides, and a perimeter layer to the perimeter set.

[ENTRYPOINT_SCOPE]: build the reinforcement

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :--------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `new Rebar(IMaterial, BarDiameter\|Length)`                | ctor     | bar from grade + catalogued or raw diameter |
|  [02]   | `new Link(IMaterial, BarDiameter)` / `new Link(IRebar)`    | ctor     | stirrup, or promote an existing bar         |
|  [03]   | `new LongitudinalReinforcement(IRebar, ILocalPoint2d)`     | ctor     | bar at an explicit coordinate               |
|  [04]   | `new FaceReinforcementLayer(SectionFace, IRebar, int)`     | ctor     | count-based face layer                      |
|  [05]   | `new FaceReinforcementLayer(SectionFace, IRebar, Length)`  | ctor     | spacing-based face layer                    |
|  [06]   | `new PerimeterReinforcementLayer(IRebar, int\|Length)`     | ctor     | count or spacing perimeter layer            |
|  [07]   | `layer.GetPath(IProfile, Length) -> ILocalPolyline2d`      | instance | bar centroid line, offset inward            |
|  [08]   | `layer.GetRebars(ILocalPolyline2d) -> IList<...>`          | instance | materialize positioned bars                 |
|  [09]   | `new MinimumReinforcementSpacing(NationalAnnex)`           | ctor     | select the EC2 annex                        |
|  [10]   | `spacing.GetMinimumReinforcementSpacing(Length) -> Length` | instance | EC2 minimum clear spacing                   |

- `GetPath` offsets the face or perimeter inward by cover and the bar radius.

[ENTRYPOINT_SCOPE]: build the section-plane geometry

| [INDEX] | [SURFACE]                                   | [SHAPE] | [CAPABILITY] |
| :-----: | :------------------------------------------ | :------ | :----------- |
|  [01]   | `new LocalPoint2d(Length, Length)`          | ctor    | Y-Z point    |
|  [02]   | `new LocalPolyline2d(IList<ILocalPoint2d>)` | ctor    | ordered loop |

- `LocalPolyline2d` throws `ArgumentException` under two points; a parametric `IProfile` perimeter assembles its outer and void loops from these carriers before `new Perimeter(outer, voids)`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- A `Section` pairs a profile and a material; `ConcreteSection` extends it with the reinforcement payload — layers, link, cover, min-spacing. A layout (`...ByCount`/`...BySpacing`) is the bar rule; a layer binds a layout to a face or perimeter and is the placement engine, so one layout reuses across faces. `ConcreteSection.Rebars` collects the positioned bars each read by materializing every layer through `GetPath`/`GetRebars`.
- Diameters, spacing, and cover are `UnitsNet.Length`, positions `ILocalPoint2d`, paths `ILocalPolyline2d` — no raw `double` on the surface; a diameter resolves from the `BarDiameter` EN-10080 catalogue, a minimum clear spacing from `MinimumReinforcementSpacing`.

[STACKING]:
- `api-vividorange-materials.md`: every `Rebar`/`Link` carries an admitted `EnRebarMaterial` (`MaterialType.Reinforcement`) as its `IMaterial` and `ConcreteSection.Material` is an `EnConcreteMaterial`, so the reinforcement geometry and the grade data meet at `IMaterial` and a bar carries a registered EN grade.
- `api-vividorange-profiles-catalogue.md`: an `IProfile` input is a `CatalogueFactory` AISC/EN section or a parametric `IProfile`; the layer engines derive bar paths over its `IPerimeter`, so reinforcement places identically over a catalogued or parametric section.
- `api-vividorange-sections-sectionproperties.md`: a `ConcreteSection` minted here IS the `IConcreteSection` the `ConcreteSectionProperties` transformed-section solver and `.Utility` `Rebars` kernel consume.
- `api-vividorange-interactiondiagram.md`: that same `ConcreteSection` and its `Rebars` are the `IConcreteSection` the `VividOrange.InteractionDiagram` N-M-M engine reads (`section.Profile`, `section.Material`, `section.Rebars[i].Rebar.{Diameter, Material}`), driving the biaxial capacity hull.
- `api-vividorange-serialization.md`: every section and bar type is `ITaxonomySerializable`, round-tripping through `ToJson<T>`/`FromJson<T>` with the `$type` tag preserving polymorphic bar/layer runtime types; the two-lane marker split is `[MARKER_FLOOR_SPLIT]` there.
- geometry floor: bar positions and layer paths live in the `VividOrange.Geometry` `ILocalPoint2d`/`ILocalPolyline2d` Y-Z section space the `SectionProperties` `Centroid`/`Extends` returns share.

[LOCAL_ADMISSION]:
- A reinforced section admits through the Materials boundary owning the RC section and the `Connection/reinforcement` seam: a `ConcreteSection` from an `IProfile` + `EnConcreteMaterial` + an `EnRebarMaterial`-backed `Rebar`/layer arrangement maps onto the canonical Materials `ConnectionItem` (`ConnectionFamily.Reinforcement`) at the edge. A diameter reads from `BarDiameter`, a minimum clear spacing from `MinimumReinforcementSpacing.GetMinimumReinforcementSpacing`.
- A Materials boundary traps `InvalidMaterialTypeException`/`InvalidProfileTypeException` at the in-folder edge and lowers them onto the typed section error rail (`LanguageExt.Fin`); the throw never reaches an interior domain signature, and consumers never call the `internal` `Reinforcement.Utility`.

[RAIL_LAW]:
- Package: `VividOrange.Sections` (MIT)
- Owns: the concrete reinforced-section and reinforcement data — the `Section`/`ConcreteSection` carriers, the `Rebar`/`Link`/`LongitudinalReinforcement` primitives over the `BarDiameter` catalogue, the `ReinforcementLayoutByCount`/`BySpacing` strategies, the `FaceReinforcementLayer`/`PerimeterReinforcementLayer` placement engines (`GetPath`/`GetRebars`), and the `MinimumReinforcementSpacing` EC2 rule — the concrete impl of the `VividOrange.ISections` floor, every type `ITaxonomySerializable`.
- Accept: an RC `ConcreteSection` from an admitted `IProfile` + `EnConcreteMaterial`/`EnRebarMaterial` + a face/perimeter layer arrangement, mapped onto the canonical Materials `ConnectionFamily.Reinforcement` owner at the boundary; diameters from `BarDiameter`, spacing from `MinimumReinforcementSpacing`; the section consumed as the `IConcreteSection` input to the `SectionProperties`/`InteractionDiagram` RC solvers.
- Reject: a hand-rolled `BarSize`/`RebarSection`/diameter literal where `BarDiameter` + `Rebar` carry it; a raw-`double` read of a `Length` diameter/spacing/cover; calling the `internal` `Reinforcement.Utility` directly; an `InvalidMaterialTypeException`/`InvalidProfileTypeException` left to propagate into an interior signature.
