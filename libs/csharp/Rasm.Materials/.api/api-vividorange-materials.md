# [RASM_MATERIALS_API_VIVIDORANGE_MATERIALS]

`VividOrange.Materials` supplies the EN/Eurocode structural-material grade DATA owner — the concrete
implementation of the `VividOrange.IMaterials` interface floor. It carries (a) the EN grade-record classes
`EnConcreteMaterial` / `EnSteelMaterial` / `EnRebarMaterial` (each a registered `grade -> {fck/fy, partial
factors, exposure/spec metadata}` record over a `NationalAnnex`), (b) the four constitutive ANALYSIS materials
`LinearElasticMaterial` / `BiLinearMaterial` / `LinearElasticOrthotropicMaterial` / `ParabolaRectangleMaterial`
(the stress-strain laws an analysis consumes), and (c) the `EnConcreteFactory` / `EnSteelFactory` /
`EnRebarFactory` grade->property tables (EN 1992-1-1 / EN 1993-1-1 Table 3.1 strengths, partial factors
γ_c/γ_M0/γ_M1/γ_M2, the EN 10025-5 `EnSteelSpecification` corrosion rules) plus the `AnalysisMaterialFactory`
dispatcher that lowers any `IStandardMaterial` to its `ILinearElasticMaterial`. It REPLACES hand-keyed
material-grade scalars: a Profiles/Connection page names a `EnConcreteGrade`/`EnSteelGrade`/`EnRebarGrade` and
reads `fck`/`fy`/`E` + the full stress-strain law + partial factors as `UnitsNet` quantities from the registered
table. It is the MATERIAL half of the RC section seam — it supplies the `IMaterial` of `MaterialType.Reinforcement`
the admitted `VividOrange.Sections` `Rebar`/`ConcreteSection` consume (`api-vividorange-sections.md`) and the
`AnalysisMaterialFactory.CreateLinearElastic` strengths the `VividOrange.InteractionDiagram` fibre integral reads
(`api-vividorange-interactiondiagram.md`). All quantities are `UnitsNet` (`api-unitsnet.md`); every type
implements `ITaxonomySerializable` and round-trips through `VividOrange.Serialization` (`api-vividorange-serialization.md`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Materials`
- package: `VividOrange.Materials`
- version: `0.1.0`
- license: MIT (`licenses.nuget.org/MIT` — MagmaWorks / VividOrange taxonomy)
- assembly: `VividOrange.Materials`
- namespace: `VividOrange.Materials` (the four constitutive analysis materials + `AnalysisMaterialFactory`),
  `VividOrange.Materials.StandardMaterials.En` (the EN grade records + factories + `EnSteelSpecification`)
- asset: runtime library, pure-managed AnyCPU, NO native RID asset. Multi-TFM `net8.0` / `net7.0` / `net6.0` /
  `netstandard2.0` / `net48`; the consumer `net10.0` binds the highest managed asset `lib/net8.0` (`net8.0` is the
  bound TFM — no `net9.0`+ asset, so the `api resolve` primary `net8.0` IS the consumed surface and there is no
  multi-TFM signature drift to defend against).
- rail: profiles / connection (material grade data)
- ABI floor: a `0.1.0` PRE-1.0 contract — the grade-record + factory member set may break across a minor bump.
  The interface floor (`IMaterial`, `IAnalysisMaterial`, `IBiLinearMaterial`, `ILinearElasticMaterial`,
  `ILinearElasticOrthotropicMaterial`, `IParabolaRectangleMaterial`, `IStandardMaterial`, `IEnConcreteMaterial`,
  `IEnRebarMaterial`, `IEnSteelMaterial`, `IEnSteelSpecification`) AND every enum the public surface names
  (`MaterialType`, `EnConcreteGrade`, `EnSteelGrade`, `EnRebarGrade`, `EnCementClass`, `EnConcreteExposureClass`,
  `EnExecutionClass`, `EnSteelCorrosionResistance`, `EnSteelDeliveryCondition`, `EnSteelFormingTemperature`,
  `EnSteelImpactTemperatureProperty`, `EnSteelQualityClass`) live in the transitive `VividOrange.IMaterials`
  `0.1.0` floor (`InvalidSteelSpecificationException` is thrown from there too), centrally pinned. The
  `IStandard`/`StandardBody`/`NationalAnnex`/`En19xxPart` types AND `MissingNationalAnnexException` come from the
  `VividOrange.IStandards` floor (`api-vividorange-standards.md`); the `IStandard` concretes (`En1992`/`En1993`)
  from `VividOrange.Standards`. `UnitsNet` `5.75.0` is the shared quantity floor; `ITaxonomySerializable` is the
  `VividOrange.ISerialization` floor contract.

[FLOOR_SCOPE_GATE]: the `VividOrange.IMaterials` floor also declares `AS3600ConcreteGrade` / `ACI318ConcreteGrade`
/ `AASHTOConcreteGrade` enums, but `VividOrange.Materials` implements ONLY the EN/Eurocode standard (the
`StandardMaterials.En` namespace) — there is NO `AS3600`/`ACI318`/`AASHTO` factory or material in this assembly.
The composable grade vocabulary is EN-only; an Australian/American grade is an UNADMITTED floor enum with no
producer here. `AnalysisMaterialFactory.CreateLinearElastic` itself throws `ArgumentException` for any
`Standard.Body` other than `EN` (`StandardBody.EN`).

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: EN grade-record materials (the registered `grade -> property` DATA)
- rail: profiles / connection
- These are the structural-grade DATA carriers: each holds a `Grade` enum + an `IStandard` + the EN-tabulated
  partial factors and metadata. They are NOT the stress-strain law (that is the analysis-material family below) —
  they are the design-grade identity + factor record an `AnalysisMaterialFactory` lowers to a constitutive material.

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE]    | [CAPABILITY]                                                                                  |
| :-----: | :-------------------- | :---------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `EnConcreteMaterial`  | concrete grade DATA | `IEnConcreteMaterial, IStandardMaterial, IMaterial, ITaxonomySerializable` — `Grade` (`EnConcreteGrade`), `Standard` (`En1992`), `ExposureClasses`/`CementClass`/`MaximumAggregateSize`/`CrackWidthLimit`/`MinimumCover`, partial factors `PartialFactor`/`AccidentalPartialFactor`/`LongTermCompression(Tension)Factor`, `Type => MaterialType.Concrete` |
|  [02]   | `EnSteelMaterial`     | steel grade DATA  | `IEnSteelMaterial, IStandardMaterial, IMaterial, ITaxonomySerializable` — `Grade` (`EnSteelGrade`), `Specification` (`IEnSteelSpecification`), `Standard` (`En1993`), `PartialFactor`/`MemberInstabilityPartialFactor`/`TensionFracturePartialFactor` (γ_M0/γ_M1/γ_M2), `Type => MaterialType.Steel` |
|  [03]   | `EnRebarMaterial`     | rebar grade DATA  | `IEnRebarMaterial, IStandardMaterial, IMaterial, ITaxonomySerializable` — `Grade` (`EnRebarGrade`), `Standard` (`En1992`), `PartialFactor`/`AccidentalPartialFactor`, `Type => MaterialType.Reinforcement` — the `IMaterial` the `VividOrange.Sections` `Rebar`/`ConcreteSection` consume |
|  [04]   | `EnSteelSpecification`| steel spec DATA   | `IEnSteelSpecification, ITaxonomySerializable` — the EN 10025 delivery/forming/corrosion/execution sub-record (`DeliveryCondition`/`FormingTemperature`/`CorrosionResistance`/`ExecutionClass…`/`QualityClass`/`HollowSection`); its `Validate(EnSteelGrade)` enforces the EN 10025-5 corrosion-class rules (throws `InvalidSteelSpecificationException`). Public construction is via `EnSteelMaterial.Specification` / `TryCreateFromDesignition`, not a public `.ctor` (the `.ctor` is `internal`) |

[PUBLIC_TYPE_SCOPE]: constitutive ANALYSIS materials (the stress-strain LAWS an analysis consumes)
- rail: profiles / connection
- These are the material BEHAVIOUR carriers — the σ(ε) law a fibre integration or a section solver reads. A grade
  record is lowered to one of these by a factory; they are also directly constructible for a non-standard material.

| [INDEX] | [SYMBOL]                            | [PACKAGE_ROLE]        | [CAPABILITY]                                                                                  |
| :-----: | :---------------------------------- | :-------------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `LinearElasticMaterial`             | linear-elastic law    | `ILinearElasticMaterial, IAnalysisMaterial, IMaterial, ITaxonomySerializable` — `ElasticModulus`/`Strength` (`Pressure`), `PeakStrain => Strength/ElasticModulus` (`Ratio`), `Type` (`MaterialType`) |
|  [02]   | `BiLinearMaterial`                  | bilinear law          | `IBiLinearMaterial, IAnalysisMaterial, …` — `ElasticModulus`/`YieldStrength`/`UltimateStrength` (`Pressure`), `YieldStrain => YieldStrength/ElasticModulus`, `FailureStrain` (`Ratio`) — the elastic-plastic-with-hardening law |
|  [03]   | `ParabolaRectangleMaterial`         | parabola-rectangle law | `IParabolaRectangleMaterial, IAnalysisMaterial, …` — `YieldStrength` (`Pressure`), `YieldStrain`/`FailureStrain` (`Ratio`), `Exponent` (`double`), and `StressAt(Ratio strain) -> Pressure` (the EC2 parabola-rectangle concrete σ(ε) evaluator) |
|  [04]   | `LinearElasticOrthotropicMaterial`  | orthotropic law       | `ILinearElasticOrthotropicMaterial, IAnalysisMaterial, …` — per-axis `ElasticModulusX/Y/Z` + `StrengthX/Y/Z` (`Pressure`); the timber/composite directional-stiffness law |

[PUBLIC_TYPE_SCOPE]: grade->property FACTORIES (the EN-tabulated `grade -> material` dispatchers)
- rail: profiles / connection
- These hold the EN data tables (private `Dictionary<Enum, …>` keyed by grade × `EnSteelSpecification`) and the
  derivation formulae (e.g. EC2 secant modulus, the EN 1992 parabola-rectangle strain limits). They are `static`
  factories — the entrypoint a design page calls to turn a grade into a constitutive material.

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE]   | [CAPABILITY]                                                                                  |
| :-----: | :------------------------ | :--------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `AnalysisMaterialFactory` | dispatcher       | `CreateLinearElastic(IStandardMaterial)` — routes on `Standard.Body == EN` + the concrete material interface to the right `En*Factory`; throws `ArgumentException` for a non-EN body or an unimplemented EN material type |
|  [02]   | `EnConcreteFactory`       | concrete factory | `CreateLinearElastic<T>(T grade) where T : Enum` (parses `Cxx/xx` -> `fck`, derives the EC2 secant `Ecm`) and `CreateParabolaRectangleAnalysisMaterial(EnConcreteGrade) -> IParabolaRectangleMaterial` (the EC2 ε_c2/ε_cu2 + exponent strain-limit law) |
|  [03]   | `EnSteelFactory`          | steel factory    | `CreateLinearElastic(IEnSteelMaterial[, Length elementThickness])` / `CreateBiLinear(IEnSteelMaterial[, Length elementThickness])` — reads the EN 1993-1-1 Table 3.1 `f_y`/`f_u` by grade × `EnSteelSpecification` (AR/N/M/W tables) with the ≤40 mm vs 40–80 mm thickness split |
|  [04]   | `EnRebarFactory`          | rebar factory    | `CreateLinearElastic<T>(T grade) where T : Enum` (parses the digits -> `f_yk`, fixed `Es = 200 GPa`) and `CreateBiLinear(EnRebarGrade) -> IBiLinearMaterial` (the ductility-class A/B/C k-factor + ε_uk hardening law) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: build an EN grade material (the registered grade DATA)
- rail: profiles / connection

| [INDEX] | [SURFACE]                                                                                     | [CALL_SHAPE]   | [CAPABILITY]                                                                    |
| :-----: | :-------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------------------------- |
|  [01]   | `new EnConcreteMaterial(EnConcreteGrade grade, NationalAnnex na)`                              | constructor    | the concrete grade with EC2 partial factors set for the annex (default exposure/cement/cover) |
|  [02]   | `new EnConcreteMaterial(grade, na, EnConcreteExposureClass, Length maxAggregate, EnCementClass[, Length crackWidthLimit, Length minimumCover])` | constructor | the full durability-detailed concrete grade |
|  [03]   | `new EnSteelMaterial(EnSteelGrade grade, NationalAnnex na)`                                    | constructor    | the steel grade with γ_M0/γ_M1/γ_M2 set for the annex                            |
|  [04]   | `EnSteelMaterial.TryCreateFromDesignition(string designition, NationalAnnex na, out EnSteelMaterial)` | static `Try` | parse an EN designation (e.g. `"S355N"`/`"S355W"`/`"S355H"`) into a grade + delivery/hollow/corrosion `Specification` — the ONLY non-`throw` material constructor on this surface (note the upstream spelling `Designition`) |
|  [05]   | `new EnRebarMaterial(EnRebarGrade grade, NationalAnnex na)`                                    | constructor    | the reinforcement grade (`MaterialType.Reinforcement`) the `VividOrange.Sections` `Rebar`/`ConcreteSection` consume |

[ENTRYPOINT_SCOPE]: lower a grade to a constitutive ANALYSIS material (the σ(ε) law)
- rail: profiles / connection
- the canonical path: build a grade record (above), then call a factory to get the `ILinearElasticMaterial` /
  `IBiLinearMaterial` / `IParabolaRectangleMaterial` an analysis or a fibre integral reads.

| [INDEX] | [SURFACE]                                                                                     | [CALL_SHAPE]   | [CAPABILITY]                                                                    |
| :-----: | :-------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------------------------- |
|  [01]   | `AnalysisMaterialFactory.CreateLinearElastic(IStandardMaterial material)`                      | static call    | the polymorphic entry — lowers any EN concrete/steel/rebar grade to its `ILinearElasticMaterial` (routes to the right `En*Factory`); the surface `VividOrange.InteractionDiagram` calls |
|  [02]   | `EnConcreteFactory.CreateLinearElastic<EnConcreteGrade>(grade)`                                | static call    | the EC2 secant-modulus linear-elastic concrete (`Ecm` from `fck`)               |
|  [03]   | `EnConcreteFactory.CreateParabolaRectangleAnalysisMaterial(EnConcreteGrade grade)`            | static call    | the EC2 parabola-rectangle concrete σ(ε) law (`fcd`, ε_c2/ε_cu2, `n` exponent)  |
|  [04]   | `EnSteelFactory.CreateLinearElastic(IEnSteelMaterial[, Length elementThickness])`             | static call    | the linear-elastic steel from Table 3.1 `f_y` (thickness-banded)                |
|  [05]   | `EnSteelFactory.CreateBiLinear(IEnSteelMaterial[, Length elementThickness])`                  | static call    | the bilinear steel (`f_y` -> `f_u`, ε_u) from Table 3.1                          |
|  [06]   | `EnRebarFactory.CreateLinearElastic<EnRebarGrade>(grade)` / `CreateBiLinear(EnRebarGrade)`     | static call    | the linear-elastic / bilinear reinforcement (Es = 200 GPa, ductility k/ε_uk by class A/B/C) |
|  [07]   | `ParabolaRectangleMaterial.StressAt(Ratio strain) -> Pressure`                                 | method         | evaluate the concrete σ at a given strain (the parabola-rectangle ordinate; clamps past `FailureStrain`) |

[ENTRYPOINT_SCOPE]: a non-standard constitutive material (direct construction, no grade table)
- rail: properties (a measured/user material that is NOT an EN grade)

| [INDEX] | [SURFACE]                                                                                     | [CALL_SHAPE]   | [CAPABILITY]                                                                    |
| :-----: | :-------------------------------------------------------------------------------------------- | :------------- | :----------------------------------------------------------------------------- |
|  [01]   | `new LinearElasticMaterial(MaterialType, Pressure elasticModulus, Pressure strength)`          | constructor    | a directly-specified linear-elastic material (any `MaterialType`)               |
|  [02]   | `new BiLinearMaterial(MaterialType, Pressure E, Pressure yield, Pressure ultimate, Ratio failureStrain)` / `new BiLinearMaterial(ILinearElasticMaterial, Pressure ultimate, Ratio failureStrain)` | constructor | a directly-specified bilinear material (the second ctor promotes a linear-elastic to bilinear) |
|  [03]   | `new ParabolaRectangleMaterial(MaterialType, Pressure yield, Ratio yieldStrain, Ratio failureStrain, double exponent)` | constructor | a directly-specified parabola-rectangle material |
|  [04]   | `new LinearElasticOrthotropicMaterial(MaterialType, Pressure Ex, Pressure Sx, Pressure Ey, Pressure Sy, Pressure Ez, Pressure Sz)` | constructor | a directly-specified orthotropic material (timber/composite) |

## [04]-[IMPLEMENTATION_LAW]

[GRADE_DATA_ALGEBRA]:
- root: a grade record (`EnConcreteMaterial`/`EnSteelMaterial`/`EnRebarMaterial`) is the DESIGN-GRADE identity + the
  EN partial-factor + durability/spec metadata; it carries a `Grade` enum and an `IStandard` (`En1992`/`En1993`), NOT
  a σ(ε) law.
- lowering: a constitutive ANALYSIS material (`LinearElasticMaterial`/`BiLinearMaterial`/`ParabolaRectangleMaterial`/
  `LinearElasticOrthotropicMaterial`) is the BEHAVIOUR — the σ(ε) law. A grade record is lowered to one by an
  `En*Factory` (the table read + EC formula); the analysis materials are also directly constructible for a
  non-standard material with no grade table.
- dispatcher: `AnalysisMaterialFactory.CreateLinearElastic(IStandardMaterial)` is the ONE polymorphic entry — it
  discriminates on `Standard.Body` (`EN` only) and the concrete material interface (`IEnConcreteMaterial` /
  `IEnRebarMaterial` / `IEnSteelMaterial`) to route to the right `En*Factory`, so a consumer calls ONE method with
  any EN grade rather than a per-grade-type factory selection.
- units: every strength/modulus is a `UnitsNet.Pressure`, every strain/factor a `UnitsNet.Ratio`, every dimension a
  `UnitsNet.Length` — there is NO raw-`double` scalar on the material surface (the lone exception is the
  parabola-rectangle `Exponent`, a dimensionless shape parameter). The `UnitsNet` family-gate (`api-unitsnet.md`)
  applies to every read.

[EN_TABLE_CONTRACT]:
- `EnSteelFactory` holds the EN 1993-1-1 Table 3.1 `f_y`/`f_u` as four private `Dictionary<Enum, …>` tables keyed by
  `EnSteelGrade`, one per `EnSteelSpecification` rolling family — AR (as-rolled), N (normalized), M (thermomechanical),
  W (weathering) — each row a (`f_y`, `f_u`) pair for the ≤40 mm and 40–80 mm thickness bands; `CreateLinearElastic`/
  `CreateBiLinear` select the row by the material's grade × `Specification` and the band by `elementThickness`.
- `EnConcreteFactory.CreateLinearElastic` parses the `Cxx`/`Cxx_yy` grade token to `fck` and derives the EC2 secant
  modulus `Ecm = fck / ε` with `ε = 1.75 + 0.55·((fck−50)/40)` ‰ above C50; `CreateParabolaRectangleAnalysisMaterial`
  derives the EC2 ε_c2 / ε_cu2 strain limits + the `n` exponent (the high-strength branches above C50).
- `EnRebarFactory.CreateLinearElastic` parses the grade digits to `f_yk` with a fixed `Es = 200 GPa`; `CreateBiLinear`
  applies the EN 1992 ductility-class k-factor + ε_uk by the trailing class letter (A: k=1.05 ε=2.5%, B: k=1.08 ε=5%,
  C: k=1.15 ε=7.5%) — an unknown class throws `ArgumentException`.
- `EnConcreteMaterial`/`EnRebarMaterial` validate the `NationalAnnex` on construction (only RecommendedValues / Germany
  / UnitedKingdom are tabulated for the rebar partial factors) — an unsupported annex throws
  `MissingNationalAnnexException` (the floor type); `EnSteelSpecification.Validate` enforces the EN 10025-5 corrosion
  rules (throws `InvalidSteelSpecificationException`).

[BOUNDARY_EXCEPTION_LAW]:
- This package is exception-throwing at its construction/derivation boundary: `ArgumentException` (non-EN body,
  unknown grade/class), `MissingNationalAnnexException` (untabulated annex), `InvalidSteelSpecificationException`
  (illegal corrosion/delivery combination). These are NOT a typed `Fin`/`Validation` rail. A Materials owner that
  composes this DATA traps them at the in-folder boundary and lowers them onto the canonical typed material-grade
  error rail (`LanguageExt.Fin`) — the EN derivation throw NEVER propagates into an interior domain signature.
- `EnSteelMaterial.TryCreateFromDesignition` is the ONE non-throwing entry (an `out` + `bool`); a designation-parse
  path uses it and maps `false` to the typed parse failure, rather than catching a throw.

[LOCAL_ADMISSION]:
- The grade DATA is admitted ONLY through the Materials boundary that needs a structural-material grade — the steel
  `Profile` family, the RC `ConcreteSection`, and the reinforcement `Rebar`. A design page NAMES an
  `EnConcreteGrade`/`EnSteelGrade`/`EnRebarGrade` + a `NationalAnnex` and reads the registered `fck`/`fy`/`E` + partial
  factors as `UnitsNet` quantities, replacing every hand-keyed grade scalar.
- The canonical material concept maps the grade record onto the Materials `MaterialProperty` engine at the edge: `fck`/
  `fy`/`Ecm` become the mechanical-property `Pressure` fields, the partial factors the design-resistance reductions —
  one `UnitsNet`-typed material surface, never a parallel grade-scalar table.
- The constitutive (σ(ε)) materials are admitted at the Properties/analysis boundary that needs a stress-strain law;
  a non-EN measured material is constructed directly, an EN grade is lowered through `AnalysisMaterialFactory`.

[STACK]:
- reinforcement seam: `EnRebarMaterial` (`MaterialType.Reinforcement`) is the `IMaterial` the admitted
  `VividOrange.Sections` `Rebar(IMaterial, Length|BarDiameter)` / `ConcreteSection(IProfile, IMaterial, …)` consume
  (`api-vividorange-sections.md`) — the Materials grade DATA and the Sections reinforcement geometry meet at the
  `IMaterial` contract, so a rebar carries a registered EN grade, never a hand-keyed `f_yk`.
- capacity seam: `AnalysisMaterialFactory.CreateLinearElastic` over the section's `IEnConcreteMaterial` /
  `IEnRebarMaterial` supplies the `fcd`/`fyd` strengths the `VividOrange.InteractionDiagram` fibre integral reads
  (`api-vividorange-interactiondiagram.md`) — the grade DATA and the N-M-M capacity COMPUTATION meet at
  `AnalysisMaterialFactory` (the engine casts `section.Material`/`rebar.Material` to the EN material interfaces, so a
  non-EN grade is rejected at the integral, [FLOOR_SCOPE_GATE]).
- standard seam: every grade record cites an `IStandard` concrete (`EnConcreteMaterial.Standard` = `En1992`,
  `EnSteelMaterial.Standard` = `En1993`) from `VividOrange.Standards` (`api-vividorange-standards.md`), and a
  `NationalAnnex` (the `VividOrange.IStandards` floor enum) — the design-code identity the material cites is the same
  `IStandard` the design pages name, never an inline code literal.
- units seam: a computed section property (`api-vividorange-sections-sectionproperties.md`) and a material strength are
  the SAME `UnitsNet` quantity type (`Pressure`/`Area`/`Length`/`Ratio`, `api-unitsnet.md`) — a design check folds the
  material `fcd` and the section modulus through one unit-typed surface; a series of material moduli folds through
  `UnitMath.Sum<T>`, never a raw accumulation.
- wire seam: every type is `ITaxonomySerializable`, so a grade record round-trips through `VividOrange.Serialization`
  `ToJson<T>`/`FromJson<T>` (`api-vividorange-serialization.md`) over the `UnitsNet` Json.NET converter
  (`api-unitsnet-serialization-jsonnet.md`) as canonical SI scalar + unit token + the polymorphic `$type` tag (so the
  exact `EnConcreteMaterial`/`EnSteelMaterial` runtime type is preserved on decode) — a TS/Python peer reads the same
  shape.

[RAIL_LAW]:
- Package: `VividOrange.Materials` `0.1.0` (MIT, pure-managed AnyCPU, `net10.0` binds `net8.0`, PRE-1.0 contract)
- Owns: the EN/Eurocode structural-material grade DATA — the `EnConcreteMaterial`/`EnSteelMaterial`/`EnRebarMaterial`
  grade records (grade -> `fck`/`fy`/`E` + partial factors + durability/spec metadata over a `NationalAnnex`), the four
  constitutive analysis materials (`LinearElastic`/`BiLinear`/`ParabolaRectangle`/`LinearElasticOrthotropic` σ(ε)
  laws), and the `EnConcreteFactory`/`EnSteelFactory`/`EnRebarFactory` + `AnalysisMaterialFactory` grade->material
  tables (EN 1992-1-1 / EN 1993-1-1 Table 3.1, the EN 10025-5 `EnSteelSpecification` rules) — all returning `UnitsNet`
  quantities, every type `ITaxonomySerializable`.
- Accept: a structural-material grade NAMED by its EN grade enum + a `NationalAnnex`, read as registered `UnitsNet`-typed
  `fck`/`fy`/`E` + partial-factor DATA; lowered to a constitutive σ(ε) material through `AnalysisMaterialFactory`; the
  EN derivation throws trapped at the in-folder boundary onto the typed material-grade `Fin` rail; the grade DATA
  admitted at the Materials material/Profiles/Connection boundary.
- Reject: a hand-keyed `fck`/`fy`/`E` scalar where a registered grade record carries it; a raw-`double` read of a
  `UnitsNet` strength/modulus; an `AS3600`/`ACI318`/`AASHTO` grade enum (no producer in this EN-only assembly,
  [FLOOR_SCOPE_GATE]); a non-EN `IStandardMaterial` fed to `AnalysisMaterialFactory.CreateLinearElastic` (it throws);
  an EN derivation `ArgumentException`/`MissingNationalAnnexException`/`InvalidSteelSpecificationException` left to
  propagate into an interior domain signature instead of being lowered onto the typed rail.
