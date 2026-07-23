# [RASM_MATERIALS_API_VIVIDORANGE_MATERIALS]

`VividOrange.Materials` owns the EN/Eurocode structural-material grade DATA: a grade enum with a `NationalAnnex` reads back `fck`/`fy`/`E`, partial factors, and durability metadata as `UnitsNet` quantities, and an `En*Factory` lowers that grade to the constitutive σ(ε) law an analysis consumes. It is the material half of the RC section seam — it supplies the `IMaterial` a section's `Rebar`/`ConcreteSection` bind and the strengths the interaction-diagram fibre integral reads. This assembly implements EN grades alone; an Australian or American grade has no producer here.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `VividOrange.Materials`
- package: `VividOrange.Materials` (MIT)
- assembly: `VividOrange.Materials`
- namespace: `VividOrange.Materials` (constitutive materials + `AnalysisMaterialFactory`), `VividOrange.Materials.StandardMaterials.En` (grade records, factories, `EnSteelSpecification`)
- asset: pure-managed AnyCPU runtime library, no native RID; the consumer binds the `net8.0` managed asset
- rail: profiles / connection (material grade data)
- depends: the floor interfaces (`IMaterial`, `IStandardMaterial`, `IEn*Material`), grade/spec enums, `MaterialType`, and `InvalidSteelSpecificationException` come from the transitive `VividOrange.IMaterials` floor; `IStandard`/`StandardBody`/`NationalAnnex`/`En19xxPart` and `MissingNationalAnnexException` from `VividOrange.IStandards`, the `En1992`/`En1993` concretes from `VividOrange.Standards` (`api-vividorange-standards.md`). Quantities are `UnitsNet` (`libs/csharp/.api/api-unitsnet.md`); the `ITaxonomySerializable` round-trip marker and its two-lane CLR-identity split ride `api-vividorange-serialization.md` [MARKER_FLOOR_SPLIT].

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: EN grade records — the registered `grade -> property` DATA; each carries a `Grade` enum, an `IStandard`, EN partial factors, and durability/spec metadata, and implements `IStandardMaterial`, `IMaterial`, `ITaxonomySerializable`, and its own `IEn*Material`. A record is design-grade identity, never a σ(ε) law; an `AnalysisMaterialFactory` lowers it to a constitutive material.

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :--------------------- | :------------ | :------------------------------------------------------------- |
|  [01]   | `EnConcreteMaterial`   | class         | concrete grade — `fck`, factors, durability over `En1992`      |
|  [02]   | `EnSteelMaterial`      | class         | steel grade — γ_M0/γ_M1/γ_M2, `Specification` over `En1993`    |
|  [03]   | `EnRebarMaterial`      | class         | rebar grade (`MaterialType.Reinforcement`) — `f_yk`, factors   |
|  [04]   | `EnSteelSpecification` | class         | EN 10025-5 steel spec — delivery, corrosion, execution, hollow |

[`EnConcreteMaterial`]: `Grade` `Standard` `Type` `ExposureClasses` `CementClass` `MaximumAggregateSize` `CrackWidthLimit` `MinimumCover` `PartialFactor` `AccidentalPartialFactor` `LongTermCompressionFactor` `LongTermTensionFactor`
[`EnSteelMaterial`]: `Grade` `Standard` `Type` `Specification` `PartialFactor` `MemberInstabilityPartialFactor` `TensionFracturePartialFactor`
[`EnRebarMaterial`]: `Grade` `Standard` `Type` `PartialFactor` `AccidentalPartialFactor`
[`EnSteelSpecification`]: `DeliveryCondition` `FormingTemperature` `CorrosionResistance` `ExecutionClass` `QualityClass` `HollowSection`

- `EnSteelSpecification.Validate(EnSteelGrade)` enforces the EN 10025-5 corrosion-class rules and throws `InvalidSteelSpecificationException`; the `.ctor` is `internal`, so a spec is reached through `EnSteelMaterial.Specification` or `TryCreateFromDesignition`.
- `EnRebarMaterial` is the `IMaterial` the admitted `VividOrange.Sections` `Rebar`/`ConcreteSection` bind.

[PUBLIC_TYPE_SCOPE]: constitutive analysis materials — the σ(ε) law a fibre integration or section solver reads; each implements `IAnalysisMaterial`, `IMaterial`, `ITaxonomySerializable`, and its own interface. A factory lowers a grade record to one; each is also directly constructible for a non-standard material.

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [CAPABILITY]                           |
| :-----: | :--------------------------------- | :------------ | :------------------------------------- |
|  [01]   | `LinearElasticMaterial`            | class         | linear-elastic σ(ε)                    |
|  [02]   | `BiLinearMaterial`                 | class         | elastic-plastic with hardening         |
|  [03]   | `ParabolaRectangleMaterial`        | class         | EC2 parabola-rectangle concrete σ(ε)   |
|  [04]   | `LinearElasticOrthotropicMaterial` | class         | timber/composite directional stiffness |

[`LinearElasticMaterial`]: `ElasticModulus` `Strength` `Type` `PeakStrain`(= `Strength`/`ElasticModulus`)
[`BiLinearMaterial`]: `ElasticModulus` `YieldStrength` `UltimateStrength` `FailureStrain` `YieldStrain`(= `YieldStrength`/`ElasticModulus`)
[`ParabolaRectangleMaterial`]: `YieldStrength` `YieldStrain` `FailureStrain` `Exponent` `StressAt(Ratio)`
[`LinearElasticOrthotropicMaterial`]: per-axis `ElasticModulusX/Y/Z` `StrengthX/Y/Z`

[PUBLIC_TYPE_SCOPE]: grade -> property factories — `static` dispatchers that turn a grade into a constitutive material. `EnSteelFactory` holds the EN 1993-1-1 tables keyed by grade; `EnConcreteFactory` and `EnRebarFactory` parse the grade token and derive the EC formula.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :------------------------ | :------------ | :------------------------------------------------------------ |
|  [01]   | `AnalysisMaterialFactory` | static class  | polymorphic lower of any EN grade to `ILinearElasticMaterial` |
|  [02]   | `EnConcreteFactory`       | static class  | token-parse + EC2 secant modulus and parabola-rectangle law   |
|  [03]   | `EnSteelFactory`          | static class  | EN 1993-1-1 table read, thickness-banded `f_y`/`f_u`          |
|  [04]   | `EnRebarFactory`          | static class  | digit-parse `f_yk` + EN 1992 ductility law                    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: build an EN grade record.

| [INDEX] | [SURFACE]                                                                      | [SHAPE] | [CAPABILITY]                          |
| :-----: | :----------------------------------------------------------------------------- | :------ | :------------------------------------ |
|  [01]   | `new EnConcreteMaterial(EnConcreteGrade, NationalAnnex)`                       | ctor    | grade, annex factors, default cover   |
|  [02]   | `new EnConcreteMaterial(…, EnConcreteExposureClass, Length, EnCementClass, …)` | ctor    | durability-detailed concrete grade    |
|  [03]   | `new EnSteelMaterial(EnSteelGrade, NationalAnnex)`                             | ctor    | grade, annex γ_M0/γ_M1/γ_M2           |
|  [04]   | `EnSteelMaterial.TryCreateFromDesignition(string, NationalAnnex, out) -> bool` | static  | parse designation to grade + spec     |
|  [05]   | `new EnRebarMaterial(EnRebarGrade, NationalAnnex)`                             | ctor    | reinforcement grade the sections bind |

- `TryCreateFromDesignition` is the one non-throwing constructor; a parse path maps `false` to the typed failure. Designations `"S355N"`/`"S355W"`/`"S355H"` become a grade with delivery/hollow/corrosion `Specification`; the upstream member name is `Designition`.
- Durability ctor `[02]` full shape: `(EnConcreteGrade, NationalAnnex, EnConcreteExposureClass, Length maxAggregate, EnCementClass, Length crackWidthLimit, Length minimumCover)`.

[ENTRYPOINT_SCOPE]: lower a grade to a constitutive analysis material — build a grade record, then call a factory for the `ILinearElastic`/`IBiLinear`/`IParabolaRectangle` material an analysis reads.

| [INDEX] | [SURFACE]                                                                    | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :--------------------------------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `AnalysisMaterialFactory.CreateLinearElastic(IStandardMaterial)`             | static   | lower any EN grade to linear-elastic         |
|  [02]   | `EnConcreteFactory.CreateLinearElastic<T>(T) where T : Enum`                 | static   | EC2 secant-modulus linear-elastic concrete   |
|  [03]   | `EnConcreteFactory.CreateParabolaRectangleAnalysisMaterial(EnConcreteGrade)` | static   | EC2 parabola-rectangle concrete σ(ε)         |
|  [04]   | `EnSteelFactory.CreateLinearElastic(IEnSteelMaterial, Length)`               | static   | linear-elastic steel, thickness-banded `f_y` |
|  [05]   | `EnSteelFactory.CreateBiLinear(IEnSteelMaterial, Length)`                    | static   | bilinear steel, `f_y` to `f_u` and ε_u       |
|  [06]   | `EnRebarFactory.CreateLinearElastic<T>(T) where T : Enum`                    | static   | linear-elastic reinforcement, `Es = 200 GPa` |
|  [07]   | `EnRebarFactory.CreateBiLinear(EnRebarGrade)`                                | static   | bilinear reinforcement, ductility by class   |
|  [08]   | `ParabolaRectangleMaterial.StressAt(Ratio) -> Pressure`                      | instance | σ ordinate, clamped past `FailureStrain`     |

- `AnalysisMaterialFactory.CreateLinearElastic` is the one polymorphic lowering entry; it throws `ArgumentException` for a non-EN `Standard.Body` or an unimplemented EN material type, and feeds `VividOrange.InteractionDiagram`.
- Steel factory thickness `Length` is optional; the concrete and rebar generic overloads accept any grade enum.

[ENTRYPOINT_SCOPE]: construct a non-standard constitutive material directly, with no grade table.

| [INDEX] | [SURFACE]                                                                     | [SHAPE] | [CAPABILITY]                       |
| :-----: | :---------------------------------------------------------------------------- | :------ | :--------------------------------- |
|  [01]   | `new LinearElasticMaterial(MaterialType, Pressure, Pressure)`                 | ctor    | direct linear-elastic material     |
|  [02]   | `new BiLinearMaterial(MaterialType, Pressure, Pressure, Pressure, Ratio)`     | ctor    | direct bilinear material           |
|  [03]   | `new BiLinearMaterial(ILinearElasticMaterial, Pressure, Ratio)`               | ctor    | promote linear-elastic to bilinear |
|  [04]   | `new ParabolaRectangleMaterial(MaterialType, Pressure, Ratio, Ratio, double)` | ctor    | direct parabola-rectangle material |
|  [05]   | `new LinearElasticOrthotropicMaterial(MaterialType, Pressure, Pressure, …)`   | ctor    | direct orthotropic, per-axis E/S   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- A grade record is design-grade identity — `Grade` enum, `IStandard`, EN partial factors, durability/spec metadata — and a constitutive material is the σ(ε) behaviour; an `En*Factory` lowers the first to the second through a table read and EC formula, and a non-standard material constructs the second directly.
- `AnalysisMaterialFactory.CreateLinearElastic(IStandardMaterial)` discriminates on `Standard.Body` (EN only) and the concrete material interface (`IEnConcreteMaterial`/`IEnRebarMaterial`/`IEnSteelMaterial`) to route to the owning factory; a non-EN body throws `ArgumentException`, so a design check calls one method for any EN grade.
- Every strength/modulus is a `UnitsNet.Pressure`, every strain/factor a `UnitsNet.Ratio`, every dimension a `UnitsNet.Length`; the sole raw `double` is the parabola-rectangle `Exponent`.
- `EnSteelFactory.GetTable3_1Properties(IEnSteelSpecification)` selects the EN table by `EnSteelDeliveryCondition` `{AR, N, M, Q}` × `HollowSection`, then `CreateLinearElastic`/`CreateBiLinear` key the grade and pick the band by thickness (≤40 mm vs 40–80 mm), `E` fixed at `210 GPa`. It throws `ArgumentException` for `ColdFormed` forming temperature, an unset `HollowSection`, a `Q` grade with a hollow section, a `> 80 mm` thickness, or a missing grade key. Corrosion `W` is not a delivery condition, so a `"S355W"` grade lowers through its delivery condition; `EnSteelGrade` tops at `S460`, so EN 10025-6 `S690` has no producer here.
- `EnConcreteFactory.CreateLinearElastic` parses the `Cxx`/`Cxx_yy` token to `fck` and derives `Ecm = fck / ε` with `ε = 1.75 + 0.55·((fck−50)/40)`‰ above C50 and `1.75`‰ at or below; `CreateParabolaRectangleAnalysisMaterial` derives ε_c2, ε_cu2, and the `n` exponent with the high-strength branch above C50.
- `EnRebarFactory.CreateLinearElastic` parses the grade digits to `f_yk` and fixes `Es = 200 GPa`; `CreateBiLinear` selects the ductility k-factor and ε_uk by the trailing class letter — A `k = 1.05`, ε_uk `2.5%`; B `k = 1.08`, ε_uk `5%`; C `k = 1.15`, ε_uk `7.5%` — and throws `ArgumentException` for an unknown class.

[STACKING]:
- `VividOrange.Sections`(`.api/api-vividorange-sections.md`): `EnRebarMaterial` (`MaterialType.Reinforcement`) is the `IMaterial` a `Rebar(IMaterial, …)`/`ConcreteSection(IProfile, IMaterial, …)` binds, so reinforcement geometry carries a registered EN grade, never a hand-keyed `f_yk`.
- `VividOrange.InteractionDiagram`(`.api/api-vividorange-interactiondiagram.md`): `AnalysisMaterialFactory.CreateLinearElastic` over the section's `IEnConcreteMaterial`/`IEnRebarMaterial` supplies the `fcd`/`fyd` the fibre integral reads; the engine casts to the EN interfaces, so a non-EN grade is rejected at the integral.
- `VividOrange.Standards`(`.api/api-vividorange-standards.md`): every grade record cites an `IStandard` concrete (`En1992`/`En1993`) and a `NationalAnnex` floor enum — the design-code identity the design pages name, never an inline literal.
- `UnitsNet`(`libs/csharp/.api/api-unitsnet.md`): a material strength and a computed section property are the same `Pressure`/`Area`/`Length`/`Ratio` type folded through one unit-typed surface; a series of moduli folds through `UnitMath.Sum<T>`.
- `VividOrange.Serialization`(`.api/api-vividorange-serialization.md`): `ITaxonomySerializable` round-trips a grade record through `ToJson<T>`/`FromJson<T>` as SI scalar + unit token + polymorphic `$type` tag; the marker lane is the CLR-identity split of [MARKER_FLOOR_SPLIT].
- within-lib: the canonical `MaterialProperty` engine maps a grade record at the folder edge — `fck`/`fy`/`Ecm` become the mechanical-property `Pressure` fields and the partial factors the design-resistance reductions, one `UnitsNet`-typed surface rather than a parallel grade-scalar table.

[LOCAL_ADMISSION]:
- Grade DATA is admitted only through the Materials boundary that needs a structural-material grade — the steel `Profile` family, the RC `ConcreteSection`, the reinforcement `Rebar`. A design page names an `EnConcreteGrade`/`EnSteelGrade`/`EnRebarGrade` with a `NationalAnnex` and reads `fck`/`fy`/`E` and partial factors as `UnitsNet` quantities.
- An EN derivation throw (`ArgumentException`/`MissingNationalAnnexException`/`InvalidSteelSpecificationException`) is trapped at the in-folder boundary and lowered onto the typed material-grade `LanguageExt.Fin` rail; `TryCreateFromDesignition`'s `false` maps to the typed parse failure instead of catching a throw.
- Constitutive materials are admitted at the analysis boundary that needs a stress-strain law: a non-EN measured material constructs directly, an EN grade lowers through `AnalysisMaterialFactory`.

[RAIL_LAW]:
- Package: `VividOrange.Materials`
- Owns: the EN/Eurocode grade DATA — the `EnConcrete`/`EnSteel`/`EnRebarMaterial` grade records, the four constitutive σ(ε) materials, and the `EnConcrete`/`EnSteel`/`EnRebarFactory` and `AnalysisMaterialFactory` grade -> material tables, all returning `UnitsNet` quantities.
- Accept: a grade named by its EN enum with a `NationalAnnex`, read as `UnitsNet`-typed `fck`/`fy`/`E` and partial-factor DATA and lowered to a constitutive material through `AnalysisMaterialFactory`.
- Reject: a hand-keyed `fck`/`fy`/`E` scalar where a grade record carries it; a raw-`double` read of a `UnitsNet` strength; an `AS3600`/`ACI318`/`AASHTO` grade fed here (no producer in this EN-only assembly); a non-EN `IStandardMaterial` fed to `AnalysisMaterialFactory`; an EN derivation throw left to propagate into an interior domain signature.
