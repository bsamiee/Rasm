# [RASM_MATERIALS_API_VIVIDORANGE_MATERIALS]

`VividOrange.Materials` supplies the EN/Eurocode structural-material grade DATA owner — the concrete implementation of the `VividOrange.IMaterials` interface floor. It carries (a) the EN grade-record classes `EnConcreteMaterial` / `EnSteelMaterial` / `EnRebarMaterial` (each a registered `grade -> {fck/fy, partial factors, exposure/spec metadata}` record over a `NationalAnnex`), (b) the four constitutive ANALYSIS materials `LinearElasticMaterial` / `BiLinearMaterial` / `LinearElasticOrthotropicMaterial` / `ParabolaRectangleMaterial` (the stress-strain laws an analysis consumes), and (c) the `EnConcreteFactory` / `EnSteelFactory` / `EnRebarFactory` grade->property tables (EN 1992-1-1 / EN 1993-1-1 Table strengths, partial factors γ_c/γ_M0/γ_M1/γ_M2, the EN 10025-5 `EnSteelSpecification` corrosion rules) plus the `AnalysisMaterialFactory` dispatcher that lowers any `IStandardMaterial` to its `ILinearElasticMaterial`. It REPLACES hand-keyed material-grade scalars: a Profiles/Connection page names a `EnConcreteGrade`/`EnSteelGrade`/`EnRebarGrade` and reads `fck`/`fy`/`E` + the full stress-strain law + partial factors as `UnitsNet` quantities from the registered table. It is the MATERIAL half of the RC section seam — it supplies the `IMaterial` of `MaterialType.Reinforcement` the admitted `VividOrange.Sections` `Rebar`/`ConcreteSection` consume (`api-vividorange-sections.md`) and the `AnalysisMaterialFactory.CreateLinearElastic` strengths the `VividOrange.InteractionDiagram` fibre integral reads (`api-vividorange-interactiondiagram.md`). All quantities are `UnitsNet` (`api-unitsnet.md`); every type implements `ITaxonomySerializable` and round-trips through `VividOrange.Serialization` (`api-vividorange-serialization.md`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Materials`
- package: `VividOrange.Materials`
- license: MIT (`licenses.nuget.org/MIT` — MagmaWorks / VividOrange taxonomy)
- assembly: `VividOrange.Materials`
- namespace: `VividOrange.Materials` (the four constitutive analysis materials + `AnalysisMaterialFactory`),
  `VividOrange.Materials.StandardMaterials.En` (the EN grade records + factories + `EnSteelSpecification`)
- asset: runtime library, pure-managed AnyCPU, NO native RID asset. Multi-TFM `net8.0` / `net7.0` / `net6.0` /
  `netstandard2.0` / `net48`; the consumer `net10.0` binds the highest managed asset `lib/net8.0` (`net8.0` is the
  bound TFM — no `net9.0`+ asset, so the `api resolve` primary `net8.0` IS the consumed surface and there is no
  multi-TFM signature drift to defend against).
- rail: profiles / connection (material grade data)
- ABI floor: a PRE-1.0 contract — the grade-record + factory member set may break across a minor bump.
  The interface floor (`IMaterial`, `IAnalysisMaterial`, `IBiLinearMaterial`, `ILinearElasticMaterial`,
  `ILinearElasticOrthotropicMaterial`, `IParabolaRectangleMaterial`, `IStandardMaterial`, `IEnConcreteMaterial`,
  `IEnRebarMaterial`, `IEnSteelMaterial`, `IEnSteelSpecification`) AND every enum the public surface names
  (`MaterialType`, `EnConcreteGrade`, `EnSteelGrade`, `EnRebarGrade`, `EnCementClass`, `EnConcreteExposureClass`,
  `EnExecutionClass`, `EnSteelCorrosionResistance`, `EnSteelDeliveryCondition`, `EnSteelFormingTemperature`,
  `EnSteelImpactTemperatureProperty`, `EnSteelQualityClass`) live in the transitive `VividOrange.IMaterials`
  floor (`InvalidSteelSpecificationException` is thrown from there too), centrally pinned. The
  `IStandard`/`StandardBody`/`NationalAnnex`/`En19xxPart` types AND `MissingNationalAnnexException` come from the
  `VividOrange.IStandards` floor (`api-vividorange-standards.md`); the `IStandard` concretes (`En1992`/`En1993`)
  from `VividOrange.Standards`. `UnitsNet` is the shared quantity floor; `ITaxonomySerializable` is the
  `VividOrange.ISerialization` floor contract. That floor marker's FQN is the
  `VividOrange.Serialization.ITaxonomySerializable` (assembly `VividOrange.ISerialization`) — a DISTINCT CLR type
  identity from the `VividOrange.Taxonomy.Serialization.ITaxonomySerializable` (assembly
  `VividOrange.Taxonomy.ISerialization`) the `VividOrange.Uncertainties` packages ride
  (`api-vividorange-uncertainties.md`); the `TaxonomyJsonSerializer` does NOT serialize the types, so
  a Materials design page never assumes one shared VividOrange serializer covers both lanes.

[FLOOR_SCOPE_GATE]: the `VividOrange.IMaterials` floor also declares `AS3600ConcreteGrade` / `ACI318ConcreteGrade` / `AASHTOConcreteGrade` enums, but `VividOrange.Materials` implements ONLY the EN/Eurocode standard (the `StandardMaterials.En` namespace) — there is NO `AS3600`/`ACI318`/`AASHTO` factory or material in this assembly. The composable grade vocabulary is EN-only; an Australian/American grade is an UNADMITTED floor enum with no producer here. `AnalysisMaterialFactory.CreateLinearElastic` itself throws `ArgumentException` for any `Standard.Body` other than `EN` (`StandardBody.EN`).

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: EN grade-record materials (the registered `grade -> property` DATA)
- rail: profiles / connection
- These are the structural-grade DATA carriers: each holds a `Grade` enum + an `IStandard` + the EN-tabulated
  partial factors and metadata. They are NOT the stress-strain law (that is the analysis-material family below) —
  they are the design-grade identity + factor record an `AnalysisMaterialFactory` lowers to a constitutive material.

| [INDEX] | [SYMBOL]               | [ROLE]         |
| :-----: | :--------------------- | :------------- |
|  [01]   | `EnConcreteMaterial`   | concrete grade |
|  [02]   | `EnSteelMaterial`      | steel grade    |
|  [03]   | `EnRebarMaterial`      | rebar grade    |
|  [04]   | `EnSteelSpecification` | steel spec     |

[EnConcreteMaterial]:

- Interfaces: `IEnConcreteMaterial`, `IStandardMaterial`, `IMaterial`, `ITaxonomySerializable`.
- Identity: `Grade` (`EnConcreteGrade`), `Standard` (`En1992`), `Type => MaterialType.Concrete`.
- Durability: `ExposureClasses`, `CementClass`, `MaximumAggregateSize`, `CrackWidthLimit`, `MinimumCover`.
- Factors: `PartialFactor`, `AccidentalPartialFactor`, `LongTermCompression(Tension)Factor`.

[EnSteelMaterial]:

- Interfaces: `IEnSteelMaterial`, `IStandardMaterial`, `IMaterial`, `ITaxonomySerializable`.
- Identity: `Grade` (`EnSteelGrade`), `Standard` (`En1993`), `Type => MaterialType.Steel`.
- Specification: `Specification` (`IEnSteelSpecification`).
- Factors: `PartialFactor`, `MemberInstabilityPartialFactor`, `TensionFracturePartialFactor` (γ_M0/γ_M1/γ_M2).

[EnRebarMaterial]:

- Interfaces: `IEnRebarMaterial`, `IStandardMaterial`, `IMaterial`, `ITaxonomySerializable`.
- Identity: `Grade` (`EnRebarGrade`), `Standard` (`En1992`), `Type => MaterialType.Reinforcement`.
- Factors: `PartialFactor`, `AccidentalPartialFactor`.
- Consumer: the `IMaterial` consumed by `VividOrange.Sections` `Rebar` and `ConcreteSection`.

[EnSteelSpecification]:

- Interfaces: `IEnSteelSpecification`, `ITaxonomySerializable`.
- Fields: `DeliveryCondition`, `FormingTemperature`, `CorrosionResistance`, `ExecutionClass…`, `QualityClass`, `HollowSection`.
- Validation: `Validate(EnSteelGrade)` enforces the EN 10025-5 corrosion-class rules and throws `InvalidSteelSpecificationException`.
- Construction: `EnSteelMaterial.Specification` and `TryCreateFromDesignition`; the `.ctor` is `internal`.

[PUBLIC_TYPE_SCOPE]: constitutive ANALYSIS materials (the stress-strain LAWS an analysis consumes)
- rail: profiles / connection
- These are the material BEHAVIOUR carriers — the σ(ε) law a fibre integration or a section solver reads. A grade
  record is lowered to one of these by a factory; they are also directly constructible for a non-standard material.

Every constitutive type implements `IAnalysisMaterial`, `IMaterial`, and `ITaxonomySerializable`.

| [INDEX] | [SYMBOL]                           | [LAW]              |
| :-----: | :--------------------------------- | :----------------- |
|  [01]   | `LinearElasticMaterial`            | linear elastic     |
|  [02]   | `BiLinearMaterial`                 | bilinear           |
|  [03]   | `ParabolaRectangleMaterial`        | parabola rectangle |
|  [04]   | `LinearElasticOrthotropicMaterial` | orthotropic        |

[LinearElasticMaterial]:

- Interface: `ILinearElasticMaterial`.
- Fields: `ElasticModulus`, `Strength` (`Pressure`), `Type` (`MaterialType`).
- Derived: `PeakStrain => Strength/ElasticModulus` (`Ratio`).

[BiLinearMaterial]:

- Interface: `IBiLinearMaterial`.
- Fields: `ElasticModulus`, `YieldStrength`, `UltimateStrength` (`Pressure`), `FailureStrain` (`Ratio`).
- Derived: `YieldStrain => YieldStrength/ElasticModulus` (`Ratio`).
- Behaviour: elastic-plastic with hardening.

[ParabolaRectangleMaterial]:

- Interface: `IParabolaRectangleMaterial`.
- Fields: `YieldStrength` (`Pressure`), `YieldStrain` and `FailureStrain` (`Ratio`), `Exponent` (`double`).
- Evaluator: `StressAt(Ratio strain) -> Pressure`, the EC2 parabola-rectangle concrete σ(ε) ordinate.

[LinearElasticOrthotropicMaterial]:

- Interface: `ILinearElasticOrthotropicMaterial`.
- Fields: per-axis `ElasticModulusX/Y/Z` and `StrengthX/Y/Z` (`Pressure`).
- Behaviour: timber/composite directional stiffness.

[PUBLIC_TYPE_SCOPE]: grade->property FACTORIES (the EN-tabulated `grade -> material` dispatchers)
- rail: profiles / connection
- These hold the EN data tables (the steel `Dictionary<Enum, Table3_1Properties>` set keyed by grade, the table selected
  by `EnSteelSpecification`) and the token-parse derivation formulae (`EnConcreteFactory`/`EnRebarFactory` parse the grade
  token — `Cxx`/`Cxx_yy` split, the rebar digit run — never a table). They are `static`
  factories — the entrypoint a design page calls to turn a grade into a constitutive material.

| [INDEX] | [SYMBOL]                  | [ROLE]     |
| :-----: | :------------------------ | :--------- |
|  [01]   | `AnalysisMaterialFactory` | dispatcher |
|  [02]   | `EnConcreteFactory`       | concrete   |
|  [03]   | `EnSteelFactory`          | steel      |
|  [04]   | `EnRebarFactory`          | rebar      |

[AnalysisMaterialFactory]:

- Surface: `CreateLinearElastic(IStandardMaterial)`.
- Dispatch: routes `Standard.Body == EN` and the concrete material interface to the corresponding `En*Factory`.
- Failure: throws `ArgumentException` for a non-EN body or an unimplemented EN material type.

[EnConcreteFactory]:

- Linear: `CreateLinearElastic<T>(T grade) where T: Enum` parses `Cxx/xx` into `fck` and derives the EC2 secant `Ecm`.
- Nonlinear: `CreateParabolaRectangleAnalysisMaterial(EnConcreteGrade) -> IParabolaRectangleMaterial` derives the EC2 ε_c2/ε_cu2 and exponent strain-limit law.

[EnSteelFactory]:

- Surfaces: `CreateLinearElastic(IEnSteelMaterial[, Length elementThickness])`, `CreateBiLinear(IEnSteelMaterial[, Length elementThickness])`.
- Table: reads EN 1993-1-1 `f_y`/`f_u` through `GetTable3_1Properties(IEnSteelSpecification)`.
- Selection: delivery condition `AR`/`N`/`M`/`Q` × solid-or-hollow, then grade and thickness band ≤40 mm or 40–80 mm.
- Modulus: fixed `E = 210 GPa`.

[EnRebarFactory]:

- Linear: `CreateLinearElastic<T>(T grade) where T: Enum` parses the digits into `f_yk` and fixes `Es = 200 GPa`.
- Bilinear: `CreateBiLinear(EnRebarGrade) -> IBiLinearMaterial` applies the ductility-class A/B/C k-factor and ε_uk hardening law.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: build an EN grade material (the registered grade DATA)
- rail: profiles / connection

| [INDEX] | [OWNER]              | [CALL]          |
| :-----: | :------------------- | :-------------- |
|  [01]   | `EnConcreteMaterial` | default ctor    |
|  [02]   | `EnConcreteMaterial` | durability ctor |
|  [03]   | `EnSteelMaterial`    | ctor            |
|  [04]   | `EnSteelMaterial`    | static `Try`    |
|  [05]   | `EnRebarMaterial`    | ctor            |

[CONCRETE_DEFAULT]:
- Surface: `new EnConcreteMaterial(EnConcreteGrade grade, NationalAnnex na)`.
- Result: concrete grade with annex EC2 partial factors and default exposure, cement, and cover.

[CONCRETE_DURABILITY]:
- Surface: `new EnConcreteMaterial(grade, na, EnConcreteExposureClass, Length maxAggregate, EnCementClass[, Length crackWidthLimit, Length minimumCover])`.
- Result: full durability-detailed concrete grade.

[STEEL_GRADE]:
- Surface: `new EnSteelMaterial(EnSteelGrade grade, NationalAnnex na)`.
- Result: steel grade with annex γ_M0/γ_M1/γ_M2.

[STEEL_DESIGNATION]:
- Surface: `EnSteelMaterial.TryCreateFromDesignition(string designition, NationalAnnex na, out EnSteelMaterial)`.
- Parse: EN designations such as `"S355N"`, `"S355W"`, and `"S355H"` become a grade plus delivery/hollow/corrosion `Specification`.
- Failure: the only non-throwing material constructor on this surface.
- Spelling: the upstream member is `Designition`.

[REBAR_GRADE]:
- Surface: `new EnRebarMaterial(EnRebarGrade grade, NationalAnnex na)`.
- Result: reinforcement grade (`MaterialType.Reinforcement`) consumed by `VividOrange.Sections` `Rebar` and `ConcreteSection`.

[ENTRYPOINT_SCOPE]: lower a grade to a constitutive ANALYSIS material (the σ(ε) law)
- rail: profiles / connection
- the canonical path: build a grade record (above), then call a factory to get the `ILinearElasticMaterial` /
  `IBiLinearMaterial` / `IParabolaRectangleMaterial` an analysis or a fibre integral reads.

| [INDEX] | [OWNER]                     | [CALL]             |
| :-----: | :-------------------------- | :----------------- |
|  [01]   | `AnalysisMaterialFactory`   | linear             |
|  [02]   | `EnConcreteFactory`         | linear             |
|  [03]   | `EnConcreteFactory`         | parabola rectangle |
|  [04]   | `EnSteelFactory`            | linear             |
|  [05]   | `EnSteelFactory`            | bilinear           |
|  [06]   | `EnRebarFactory`            | linear             |
|  [07]   | `EnRebarFactory`            | bilinear           |
|  [08]   | `ParabolaRectangleMaterial` | stress ordinate    |

[ANALYSIS_LINEAR]:
- Surface: `AnalysisMaterialFactory.CreateLinearElastic(IStandardMaterial material)`.
- Result: polymorphically lowers any EN concrete, steel, or rebar grade to `ILinearElasticMaterial` through the corresponding `En*Factory`.
- Consumer: `VividOrange.InteractionDiagram`.

[CONCRETE_LINEAR]:
- Surface: `EnConcreteFactory.CreateLinearElastic<EnConcreteGrade>(grade)`.
- Result: EC2 secant-modulus linear-elastic concrete, with `Ecm` derived from `fck`.

[CONCRETE_PARABOLA_RECTANGLE]:
- Surface: `EnConcreteFactory.CreateParabolaRectangleAnalysisMaterial(EnConcreteGrade grade)`.
- Result: EC2 parabola-rectangle concrete σ(ε) law with `fcd`, ε_c2/ε_cu2, and the `n` exponent.

[STEEL_LINEAR]:
- Surface: `EnSteelFactory.CreateLinearElastic(IEnSteelMaterial[, Length elementThickness])`.
- Result: linear-elastic steel from thickness-banded Table `f_y`.

[STEEL_BILINEAR]:
- Surface: `EnSteelFactory.CreateBiLinear(IEnSteelMaterial[, Length elementThickness])`.
- Result: bilinear steel from Table `f_y` to `f_u` and ε_u.

[REBAR_LINEAR]:
- Surface: `EnRebarFactory.CreateLinearElastic<EnRebarGrade>(grade)`.
- Result: linear-elastic reinforcement with `Es = 200 GPa`.

[REBAR_BILINEAR]:
- Surface: `EnRebarFactory.CreateBiLinear(EnRebarGrade)`.
- Result: bilinear reinforcement with ductility k/ε_uk selected by class A/B/C.

[PARABOLA_RECTANGLE_STRESS]:
- Surface: `ParabolaRectangleMaterial.StressAt(Ratio strain) -> Pressure`.
- Result: evaluates the concrete parabola-rectangle σ ordinate and clamps past `FailureStrain`.

[ENTRYPOINT_SCOPE]: a non-standard constitutive material (direct construction, no grade table)
- rail: properties (a measured/user material that is NOT an EN grade)

| [INDEX] | [OWNER]                            | [CALL]         |
| :-----: | :--------------------------------- | :------------- |
|  [01]   | `LinearElasticMaterial`            | ctor           |
|  [02]   | `BiLinearMaterial`                 | direct ctor    |
|  [03]   | `BiLinearMaterial`                 | promotion ctor |
|  [04]   | `ParabolaRectangleMaterial`        | ctor           |
|  [05]   | `LinearElasticOrthotropicMaterial` | ctor           |

[DIRECT_LINEAR]:
- Surface: `new LinearElasticMaterial(MaterialType, Pressure elasticModulus, Pressure strength)`.
- Result: directly specified linear-elastic material of any `MaterialType`.

[DIRECT_BILINEAR]:
- Surface: `new BiLinearMaterial(MaterialType, Pressure E, Pressure yield, Pressure ultimate, Ratio failureStrain)`.
- Result: directly specified bilinear material.

[PROMOTED_BILINEAR]:
- Surface: `new BiLinearMaterial(ILinearElasticMaterial, Pressure ultimate, Ratio failureStrain)`.
- Result: promotes a linear-elastic material to bilinear.

[DIRECT_PARABOLA_RECTANGLE]:
- Surface: `new ParabolaRectangleMaterial(MaterialType, Pressure yield, Ratio yieldStrain, Ratio failureStrain, double exponent)`.
- Result: directly specified parabola-rectangle material.

[DIRECT_ORTHOTROPIC]:
- Surface: `new LinearElasticOrthotropicMaterial(MaterialType, Pressure Ex, Pressure Sx, Pressure Ey, Pressure Sy, Pressure Ez, Pressure Sz)`.
- Result: directly specified orthotropic timber/composite material.

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
- `EnSteelFactory` holds the EN 1993-1-1 Table `f_y`/`f_u` as EIGHT private `Dictionary<Enum, Table3_1Properties>`
  tables keyed by `EnSteelGrade` — five solid (`…_AR`/`…_N`/`…_M`/`…_W`/`…_Q`) and three hollow (`…_ARH`/`…_NH`/`…_MH`);
  each `Table3_1Properties` value carries four `Pressure` bands (`F_y`/`F_u` at ≤40 mm and at 40–80 mm), with `E` fixed at
  `210 GPa`. `GetTable3_1Properties(IEnSteelSpecification)` selects the table by switching on `EnSteelDeliveryCondition`
  `{AR, N, M, Q}` (EN 10025-2/-3/-4/-6) × `HollowSection` (solid -> base table, hollow -> the `*H` table); it throws
  `ArgumentException` for `ColdFormed` forming temperature, an unset `HollowSection`, and a `Q` + hollow combination (no
  `QH` table exists). `CreateLinearElastic`/`CreateBiLinear` then key the grade row inside the selected table and pick the
  band by `elementThickness` (≤40 mm vs 40–80 mm; `> 80 mm` throws; a missing grade key or a `0`-valued 40–80 mm entry throws).
- Weathering: the `…_W` table is UNROUTED DEAD DATA because weathering is the `EnSteelCorrosionResistance` axis (`None`/`W`/`WP`), not an `EnSteelDeliveryCondition`; `GetTable3_1Properties` switches only on delivery condition × hollow, so no specification reaches `…_W`. A `"S355W"` designation parses to `CorrosionResistance = W` but lowers through its delivery condition (`AR`/`N`/`M`).
- Quenched: the `…_Q` table holds only `S460` (`460`/`570` at ≤40 mm, `440`/`550` at 40–80 mm); `EnSteelGrade` tops out at `S460`, so EN 10025-6 `S690` has no producer in this assembly.
- `EnConcreteFactory.CreateLinearElastic` parses the `Cxx`/`Cxx_yy` grade token to `fck` and derives the EC2 secant
  modulus `Ecm = fck / ε` with `ε = + ·((fck−50)/40)` ‰ above C50; `CreateParabolaRectangleAnalysisMaterial`
  derives the EC2 ε_c2 / ε_cu2 strain limits + the `n` exponent (the high-strength branches above C50).
- `EnRebarFactory.CreateLinearElastic` parses the grade digits to `f_yk` with a fixed `Es = 200 GPa`; `CreateBiLinear`
  applies the EN 1992 ductility-class k-factor + ε_uk by the trailing class letter (A: k= ε=%, B: k= ε=5%,
  C: k= ε=%) — an unknown class throws `ArgumentException`.
- `EnConcreteMaterial`/`EnRebarMaterial` validate the `NationalAnnex` on construction (only RecommendedValues / Germany
  / UnitedKingdom are tabulated for the rebar partial factors) — an unsupported annex throws
  `MissingNationalAnnexException` (the floor type); `EnSteelSpecification.Validate` enforces the EN 10025-5 corrosion
  rules (throws `InvalidSteelSpecificationException`).

[BOUNDARY_EXCEPTION_LAW]:
- Boundary: construction and derivation throw `ArgumentException` for a non-EN body or unknown grade/class, `MissingNationalAnnexException` for an untabulated annex, and `InvalidSteelSpecificationException` for an illegal corrosion/delivery combination. These are NOT a typed `Fin`/`Validation` rail.
- Lowering: a Materials owner traps these exceptions at the in-folder boundary and lowers them onto the canonical typed material-grade error rail (`LanguageExt.Fin`); an EN derivation throw NEVER propagates into an interior domain signature.
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
  `VividOrange.Sections` `Rebar(IMaterial, Length\|BarDiameter)` / `ConcreteSection(IProfile, IMaterial, …)` consume
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
  (`api-vividorange-serialization.md` [TRANSITIVE_UNITSNET_JSONNET]) as canonical SI scalar + unit token + the polymorphic `$type` tag (so the
  exact `EnConcreteMaterial`/`EnSteelMaterial` runtime type is preserved on decode) — a TS/Python peer reads the same
  shape.

[RAIL_LAW]:
- Package: `VividOrange.Materials` (MIT, pure-managed AnyCPU, `net10.0` binds `net8.0`, PRE-1.0 contract)
- Owns: the EN/Eurocode structural-material grade DATA — the `EnConcreteMaterial`/`EnSteelMaterial`/`EnRebarMaterial`
  grade records (grade -> `fck`/`fy`/`E` + partial factors + durability/spec metadata over a `NationalAnnex`), the four
  constitutive analysis materials (`LinearElastic`/`BiLinear`/`ParabolaRectangle`/`LinearElasticOrthotropic` σ(ε)
  laws), and the `EnConcreteFactory`/`EnSteelFactory`/`EnRebarFactory` + `AnalysisMaterialFactory` grade->material
  tables (EN 1992-1-1 / EN 1993-1-1 Table, the EN 10025-5 `EnSteelSpecification` rules) — all returning `UnitsNet`
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
