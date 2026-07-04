# [MATERIALS_TIMBER]

THE TIMBER SEED FAMILY GROUNDED IN THE EN STRENGTH-CLASS TABLES. A sawn/glulam/LVL/PSL member and a cross-laminated panel are each one `ComponentRow` minted by the ONE generator `TimberSeed.Rows -> Component.Of` over the `ComponentFamily.Timber` policy row (`ComponentClass.Primary`, `DetailLane.None`, admits `SectionProfile.Rectangle` or `SectionProfile.Layered`, cross-nominal the section depth) — never a `GlulamBeam`/`CltPanel` type, never a hand-keyed strength literal, and never a bespoke `TimberSection` payload (DELETED: its geometric columns are the `SectionProfile` named `PositiveMagnitude` dims, its vocabulary columns the kept `TimberForm`/`ServiceClass`/`LoadDuration` policy rows plus the `TimberGrade` row table, its realization columns the seed rows). A profiled MEMBER is `SectionProfile.Rectangle` and `Sectioned: true` — its twenty-column `ComputedSection` is `component#SECTION_SOLVER`'s `SectionSolver.Solve`, never a local solve; a CLT PANEL is `SectionProfile.Layered` with role-tagged `Ply` rows (`"0"` longitudinal / `"90"` transverse — the cross-lamination IS the geometry) and `Sectioned: false` — the value-identity pin, since `SectionSolver` faults `Layered` as unsectioned and `graph.SectionOf` dereferences a CLT `ComponentId` to `(Component, None)` exactly as today. Timber contributes NO detail bag (`DetailLane.None` — the cross-ply build rides the `Layered` plies, the member parametrics ride the solved section and the Type geometry); the CLT-to-`LayerSet` composition selection is the `Projection/component#COMPOSITION_SELECTION` projector's read of the `Layered` arm, never a family method (`ToLayerSet`/`ToUnit`/`MaterialShapeKind` are the deleted forms — the profile arm IS the IFC shape discriminant). THIS page keeps what is irreducibly timber: `TimberForm` (the FORM-law `[SmartEnum]` carrying cross-ply, EN 1995-1-2 charring, per-form `γ_M`, and the per-form EC5 capacity columns `k_h`/`k_cr`/`β_c` that collapse the deleted enumerated ternaries), the `TimberGrade` frozen 11-row characteristic strength-AND-stiffness table (values verbatim, PUBLISHED — `VividOrange.Materials` ships NO timber factory, EN factories are concrete/steel/rebar only, so the row table IS the registered-class owner) with its `OrthotropicLaw`/`ToProperties` directional-stiffness lowerings (verified `LinearElasticOrthotropicMaterial(MaterialType.Timber, Pressure Ex, Sx, Ey, Sy, Ez, Sz)`; seam `MaterialPropertySet.OfOrthotropic`), the `ServiceClass`/`LoadDuration` EC5 `k_mod`/`k_def` policy axes, `TimberDesign` (the EN 1995-1-1 `Capacity` projection over the resolved `ComputedSection` or the `Layered` plies, the EN 1995-1-1 Annex B gamma-method `EffectiveStiffness` kernel, the EN 1995-1-2 `ResidualSection` charring), and the `TimberCapacity` receipt the `capacity#SECTION_CAPACITY` `SectionCapacity.Lift(TimberCapacity)` overload lifts WHOLE (receipt shape frozen). Growth is one row: a new section one `TimberRow`, a new strength class one `TimberGrade` row, a new product form one `TimberForm` row plus its EC5 columns — zero central edits.

## [01]-[INDEX]

- [02]-[TIMBER_FAMILY]: the `TimberForm` product-form `[SmartEnum]` (cross-ply flag, density-adjusted `β_n` charring, per-form `γ_M`, and the `k_h`/`k_cr`/`β_c` EC5 capacity columns), the `TimberGrade` frozen 11-row EN 14080/338/14374 characteristic vector with the `OrthotropicLaw`/`ToProperties` seam lowerings, the `ServiceClass`/`LoadDuration` `k_mod`/`k_def` policy axes (with the EN 16351 CLT service-class exclusion), the `TimberRow` authored table carrying per-row lamination builds, and the fail-loud `TimberSeed.Rows : Context -> Fin<Seq<ComponentRow>>` Traverse the `ComponentFamily.Timber` policy row binds.
- [03]-[TIMBER_CAPACITY]: the `TimberDesign.Capacity` EN 1995-1-1 design-resistance projection (one entry, member-vs-panel discriminated by the profile arm), the corrected Annex B gamma-method `EffectiveStiffness` over non-uniform role-tagged plies, the EN 1995-1-2 `ResidualSection` reduced cross-section, and the frozen `TimberCapacity` receipt the `capacity#SECTION_CAPACITY` rail lifts.

## [02]-[TIMBER_FAMILY]

- Owner: `TimberForm` the product-form policy row (sawn/glulam/clt/lvl/psl); `TimberGrade` the frozen characteristic-vector row table plus the `TimberGrades` roster; `ServiceClass`/`LoadDuration` the EC5 modification-factor axes; `TimberRow` the authored section table; `TimberSeed` the `Rows` fold the `component#COMPONENT_OWNER` `ComponentFamily.Timber` policy row binds.
- Cases: form {sawn (EN 338 solid), glulam (EN 14080 lamellae), clt (EN 16351 / APA PRG 320 cross-laminated), lvl (EN 14374 veneer), psl (parallel-strand, ETA-governed)} × grade {gl24h/gl28h/gl30h/gl32h homogeneous + gl24c/gl28c combined glulam, c16/c24/c30 sawn softwood, d40 sawn hardwood, lvl48p} × service {sc1/sc2/sc3} × duration {permanent/long/medium/short/instantaneous} — a section is one `TimberRow` over one form/grade plus its lamination build; a member is `SectionProfile.Rectangle`, a CLT panel `SectionProfile.Layered`, never a section subtype.
- Entry: `public static Fin<Seq<ComponentRow>> TimberSeed.Rows(Context context)` — ONE `Traverse` over the `TimberRow` table: each row builds its profile through the railed `SectionProfile` `Of` factory (`Rectangle.Of(w, d, key)` for a member; `Layered.Of(plies, overall, width, key)` for a cross-ply form, the plies role-tagged from the row's build with the grade-keyed `MaterialId` per ply), constructs `Component.Of(ComponentFamily.Timber, designation, profile, IfcBinding.Supertype(ComponentFamily.Timber.Class), Coring.None, En, substance, appearance, detail: None, key)` under a per-row `Op.Of(name: designation)` admission key (a fault names the exact row, never the whole-catalogue `Context` key), and pins `Sectioned: !row.Form.CrossPly` — a member row that cannot admit ABORTS the build (`Traverse`, never a swallowed `Choose` drop), and an unknown form/grade is UNREPRESENTABLE (rows reference the `TimberForm` items and `TimberGrades` statics directly, no string re-parse).
- Packages: Rasm.Numerics (project — `PositiveMagnitude` every length column), Rasm.Domain (project — `Context`/`Op`), Rasm.Element (project — `MaterialId`, `MaterialPropertySet` the `ToProperties` lowering mints), VividOrange.Materials (`LinearElasticOrthotropicMaterial`/`MaterialType.Timber` the along/across-grain law; `.api/api-vividorange-materials.md` — the `[FLOOR_SCOPE_GATE]` EN factories are concrete/steel/rebar only, so NO timber factory exists and the `TimberGrade` rows are the AUTHORED registered-class owner), UnitsNet (`Pressure.FromMegapascals` coercing the grade scalars at the orthotropic edge; `.api/api-unitsnet.md`), Rasm.Materials.Component (project — the parent `component#COMPONENT_OWNER` `Component`/`ComponentRow`/`ComponentFamily`/`ComponentFault`/`SectionProfile`/`Ply`/`IfcBinding`/`ComputedSection`/`ComponentStandard`/`ComponentAuthority`/`Coring`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new section is one `TimberRow` (its lamination build a `Seq<double>` column); a new strength class one `TimberGrade` row in the `TimberGrades` roster; a new product form one `TimberForm` row carrying its cross-ply flag, charring base, `γ_M`, and `k_h`/`k_cr`/`β_c` columns; a new service/duration point one `ServiceClass`/`LoadDuration` row — never a per-member type, never a hand-keyed strength literal, never a re-minted EC5 factor beside the form row.
- Boundary: the `TimberGrade` table is `SEED_ROW_LAW` `AUTHORED` — every one of the 12 numeric columns is a PUBLISHED normative printed value (EN 14080:2013 Table 5 glulam vectors with `E_90,g,mean = 300` for every class, EN 338 Table 1 sawn vectors, EN 14374 declared LVL values), stored verbatim and NEVER re-derived; the `Hardwood` species column is the EN 1995-1-2 Table 3.1 charring discriminant (D-classes hardwood — the SPECIES, not a density smear: a dense softwood LVL at `ρ_k = 510` charrs at the softwood/LVL `0.70`, never the hardwood `0.55` the deleted density-only interpolation mis-rated it to). `OrthotropicLaw`/`ToProperties` live ON the grade row (they read ONLY grade scalars): `OrthotropicLaw` is the `[BoundaryAdapter]` producing the verified 7-arg `LinearElasticOrthotropicMaterial` (`E0,mean` parallel = X, `E90,mean` perpendicular = Y/Z, per-axis strengths — the INDEPENDENT-`G` material the seam isotropic `G = E/(2(1+ν))` cannot model, timber's `G_mean ≈ E0/16`); `ToProperties` lowers the SAME scalars seam-neutrally through the verified `MaterialPropertySet.OfOrthotropic(density, e1Parallel, e2Perpendicular, shearModulus, strength1Parallel, strength2Perpendicular, thermalExpansion, key)` for the projected `Material` node (`Discipline.Structural`; the thermal/acoustic/fire disciplines ride the `Properties/properties#MATERIAL_PROPERTY_CATALOGUE` rows the projector composes alongside). The `Layered` plies carry the load-bearing cross-ply datum: `Ply.Role` `"0"`/`"90"` is the gamma-method longitudinal/transverse discriminant AND the `IfcMaterialLayer.Name` the `Projection/component#COMPOSITION_AUTHOR` `LayerSet` bridge round-trips; per-ply thickness is FIRST-CLASS (a mixed 30/34/34/34/34/34/30 build sums exactly to its overall — the deleted uniform-`PlyThicknessMm` layup could not spell a real manufacturer build). The IFC stamp is `IfcBinding.Supertype(ComponentFamily.Timber.Class)` (`IfcBuiltElement`/`NOTDEFINED` — a glulam serves as beam, column, or brace; the leaf is an occurrence refinement); the member-vs-panel WIRE shape (`IfcMaterialProfileSet` vs `IfcMaterialLayerSet`) is the projector's profile-arm read, so this page carries no IFC-shape column. `DetailLane.None` — a timber row carries no bag (the lane/detail totality law `Component.Of` proves); the EC5 design vocabulary (form, grade) is CALLER-carried into `TimberDesign.Capacity` exactly as the masonry `f'_m` and the RC grade are, never re-minted onto the `Component`.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Immutable;                  // ImmutableArray (the frozen TimberGrades roster)
using System.Linq;                                   // Enumerable.Repeat (the uniform lamination builds), Sum selectors over Seq
using LanguageExt;                                   // Fin, Seq, Option, Traverse
using Rasm.Numerics;                                  // PositiveMagnitude — the kernel >0 finite magnitude atom (Rasm.Numerics, NOT Rasm.Domain)
using Rasm.Domain;                                   // Context, Op, AcceptValidated
using Rasm.Element;                                  // MaterialId, MaterialPropertySet (the seam Orthotropic lowering target), PropertyBag
using Thinktecture;
using UnitsNet;                                      // Pressure (the orthotropic-law moduli/strengths, coerced at the edge)
using VividOrange.Materials;                         // LinearElasticOrthotropicMaterial, MaterialType (the along/across-grain law)
using static LanguageExt.Prelude;

// Seed pages share the parent namespace: the per-family owners are the collision-free TimberSeed/TimberDesign/
// TimberGrades statics, so the ComponentFamily policy row binds TimberSeed.Rows by its bare name.
namespace Rasm.Materials.Component;

// --- [TYPES] -------------------------------------------------------------------------------
// The timber product form: cross-ply flag (the Layered-profile + gamma-method discriminant — CLT is genuinely a
// layered panel, its profile arm the projector's IfcMaterialLayerSet-vs-ProfileSet selector), the EN 1995-1-2
// Table 3.1 SOFTWOOD notional charring rate β_n (per exposed side, corner rounding included: solid sawn/CLT 0.80,
// glulam/LVL ρ_k ≥ 480/PSL 0.70), the EN 1995-1-1 Table 2.3 material partial factor γ_M (solid 1.30, glulam/PSL
// 1.25, CLT/LVL 1.20 — the design-strength divisor, never flat), and the per-form EC5 capacity columns that
// COLLAPSE the deleted enumerated ternaries: the §3.2/§3.3/§3.4 depth size effect (reference depth, exponent, cap
// — solid 150/0.2/1.3, glulam 600/0.1/1.1 the CAP the flat-1.3 form over-credited, LVL 300 mm at the EN 14374
// declared s = 0.12 capped 1.2; CLT/PSL carry no clause, kh ≡ 1.0), the §6.1.7 shear crack factor k_cr (0.67
// solid AND glulam only — other wood-based products 1.0, the flat-0.67 form under-checked LVL/PSL), the
// §6.3.2 straightness imperfection β_c (solid 0.2; every engineered lamination 0.1), and the §6.1.6(2)
// stress-redistribution k_m (0.7 rectangular solid/glulam/LVL, 1.0 other wood-based products — CLT/PSL; the
// biaxial-interaction weight the capacity fold reads, a per-form column, never a flat const that would be
// unconservative for PSL/CLT). A new engineered product (box beam, curved glulam) is one row, never a per-member type.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TimberForm {
    public static readonly TimberForm Sawn   = new("sawn",   crossPly: false, betaNSoftwoodMmPerMin: 0.80, gammaM: 1.30, khRefDepthMm: 150.0, khExponent: 0.20, khCap: 1.3, kcrCrack: 0.67, betaC: 0.2, km: 0.7);
    public static readonly TimberForm Glulam = new("glulam", crossPly: false, betaNSoftwoodMmPerMin: 0.70, gammaM: 1.25, khRefDepthMm: 600.0, khExponent: 0.10, khCap: 1.1, kcrCrack: 0.67, betaC: 0.1, km: 0.7);
    public static readonly TimberForm Clt    = new("clt",    crossPly: true,  betaNSoftwoodMmPerMin: 0.80, gammaM: 1.20, khRefDepthMm: 0.0,   khExponent: 0.0,  khCap: 1.0, kcrCrack: 1.00, betaC: 0.1, km: 1.0);
    public static readonly TimberForm Lvl    = new("lvl",    crossPly: false, betaNSoftwoodMmPerMin: 0.70, gammaM: 1.20, khRefDepthMm: 300.0, khExponent: 0.12, khCap: 1.2, kcrCrack: 1.00, betaC: 0.1, km: 0.7);
    public static readonly TimberForm Psl    = new("psl",    crossPly: false, betaNSoftwoodMmPerMin: 0.70, gammaM: 1.25, khRefDepthMm: 0.0,   khExponent: 0.0,  khCap: 1.0, kcrCrack: 1.00, betaC: 0.1, km: 1.0);
    public bool CrossPly { get; }
    public double BetaNSoftwoodMmPerMin { get; }
    public double GammaM { get; }
    public double KhRefDepthMm { get; }
    public double KhExponent { get; }
    public double KhCap { get; }
    public double KcrCrack { get; }
    public double BetaC { get; }
    public double Km { get; }

    // EN 1995-1-2 Table 3.1 species-resolved notional charring rate: a SOFTWOOD form keeps its β_n regardless of
    // density (a C30 at ρ_k 380 still charrs 0.80; an lvl48p at 510 still 0.70); a solid HARDWOOD (the grade's
    // species column, EN 338 D-classes) charrs 0.70 at ρ_k 290 falling linearly to 0.55 at ρ_k ≥ 450 — the
    // Table 3.1 hardwood interpolation, never a density-only smear that mis-rates a dense softwood as hardwood.
    public double BetaN(TimberGrade grade) =>
        !grade.Hardwood ? BetaNSoftwoodMmPerMin
            : grade.DensityK >= 450.0 ? 0.55
            : grade.DensityK <= 290.0 ? 0.70
            : 0.70 - 0.15 * (grade.DensityK - 290.0) / 160.0;

    // EN 1995-1-1 §3.2/§3.3/§3.4 bending/tension depth size effect over the per-form columns; a form with no
    // clause (CLT plate, ETA-governed PSL) carries KhRefDepthMm 0 and reads 1.0.
    public double Kh(double depthMm) =>
        KhRefDepthMm > 0.0 && depthMm < KhRefDepthMm
            ? Math.Min(Math.Pow(KhRefDepthMm / depthMm, KhExponent), KhCap)
            : 1.0;
}

// The EN 1995-1-1 Table 3.1 service class — the in-service moisture environment driving k_mod AND k_def. SC1
// heated internal (≤12% moisture), SC2 sheltered (≤20%), SC3 exposed (>20%). k_def is FORM-JOINT data (EN
// 1995-1-1 Table 3.2 solid/glulam/LVL/PSL vs the EN 16351 CLT column), and CLT is NOT PERMITTED in SC3 — the
// None row the Capacity panel arm gates on, never an implicit dry assumption.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ServiceClass {
    public static readonly ServiceClass Sc1 = new("sc1", kdefSolid: 0.60, kdefCrossLam: Some(0.80));
    public static readonly ServiceClass Sc2 = new("sc2", kdefSolid: 0.80, kdefCrossLam: Some(1.00));
    public static readonly ServiceClass Sc3 = new("sc3", kdefSolid: 2.00, kdefCrossLam: Option<double>.None);
    public double KdefSolid { get; }
    public Option<double> KdefCrossLam { get; }

    // The creep factor a serviceability check reads: None = the form is not permitted in this service class
    // (EN 16351 bars cross-laminated timber from SC3).
    public Option<double> KdefFor(TimberForm form) => form.CrossPly ? KdefCrossLam : Some(KdefSolid);
}

// The EN 1995-1-1 Table 3.1 load-duration class: each duration carries its per-service-class k_mod triple
// (solid/glulam/CLT/LVL share these rows), the joint the design strength f_d = f_k·k_mod/γ_M reads. The EC5 axis
// KEEPS the name LoadDuration (the NDS C_d collision resolves on the connector side as LoadDurationFactor).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LoadDuration {
    public static readonly LoadDuration Permanent     = new("permanent",     kmodSc1: 0.60, kmodSc2: 0.60, kmodSc3: 0.50);
    public static readonly LoadDuration LongTerm      = new("long-term",     kmodSc1: 0.70, kmodSc2: 0.70, kmodSc3: 0.55);
    public static readonly LoadDuration MediumTerm    = new("medium-term",   kmodSc1: 0.80, kmodSc2: 0.80, kmodSc3: 0.65);
    public static readonly LoadDuration ShortTerm     = new("short-term",    kmodSc1: 0.90, kmodSc2: 0.90, kmodSc3: 0.70);
    public static readonly LoadDuration Instantaneous = new("instantaneous", kmodSc1: 1.10, kmodSc2: 1.10, kmodSc3: 0.90);
    public double KmodSc1 { get; }
    public double KmodSc2 { get; }
    public double KmodSc3 { get; }

    public double KmodFor(ServiceClass service) => service.Switch(
        state: this,
        sc1: static d => d.KmodSc1,
        sc2: static d => d.KmodSc2,
        sc3: static d => d.KmodSc3);
}

// --- [TABLES] ------------------------------------------------------------------------------
// The EN 14080 (glulam) / EN 338 (sawn) / EN 14374 (LVL) strength class as a FROZEN ROW — the SmartEnum-to-row
// conversion under SEED_ROW_LAW: pure standards data, identical columns, every numeric PUBLISHED verbatim (the
// EN tables' printed values, never re-derived — E90Mean is the EN 14080 MEAN E_90,g,mean = 300 for every glulam
// class including the combined gl24c/gl28c, not the 250 fractile). Columns: fMk bending, ft0k tension-∥, fc0k
// compression-∥, fc90k compression-⊥ (the bearing strength), fvk shear, fRvk rolling shear (the CLT gamma-method
// governor), E0Mean/E005 axial moduli (E005 the 5% stability fractile), E90Mean perpendicular, gMean/gRollMean
// shear moduli, densityK characteristic density (self-weight + embedment input). Hardwood is the EN 338 SPECIES
// class (D-grades) the EN 1995-1-2 charring rate discriminates on. VividOrange.Materials ships NO timber factory
// ([FLOOR_SCOPE_GATE]), so this table IS the registered timber-class owner.
public readonly record struct TimberGrade(
    string Key, double Fmk, double Ft0k, double Fc0k, double Fc90k, double Fvk, double FRvk,
    double E0Mean, double E005, double E90Mean, double GMean, double GRollMean, double DensityK, bool Hardwood) {

    public MaterialId Substance => MaterialId.Of($"wood.{Key}");

    // The along/across-grain directional-stiffness law the structural solver reads — the verified 7-arg ctor
    // (MaterialType, Ex, Sx, Ey, Sy, Ez, Sz), modulus then strength per axis, over the non-obsolete
    // MaterialType.Timber (SolidTimber/GluedLaminatedTimber deprecated). The INDEPENDENT-G orthotropic material
    // the seam isotropic G = E/(2(1+ν)) cannot model: G_mean ≈ E0/16 is a datum, not a Poisson derivation.
    [BoundaryAdapter]
    public LinearElasticOrthotropicMaterial OrthotropicLaw() =>
        new(MaterialType.Timber,
            Pressure.FromMegapascals(E0Mean),  Pressure.FromMegapascals(Fc0k),    // X: parallel-to-grain
            Pressure.FromMegapascals(E90Mean), Pressure.FromMegapascals(Fc90k),   // Y: perpendicular
            Pressure.FromMegapascals(E90Mean), Pressure.FromMegapascals(Fc90k));  // Z: perpendicular (radial≈tangential band)

    // The seam-NEUTRAL lowering of the SAME scalars (raw MPa doubles — the seam references no VividOrange) onto
    // the verified MaterialPropertySet.Orthotropic case the projected Material node carries and the Rasm.Compute
    // structural route reads; ~5e-6/K the grain-parallel expansion. Thermal/acoustic/fire disciplines ride the
    // Properties catalogue rows the projector composes alongside — this owns ONLY the grade-sourced stiffness.
    public Fin<Seq<MaterialPropertySet>> ToProperties(Op key) =>
        MaterialPropertySet.OfOrthotropic(DensityK, E0Mean, E90Mean, GMean, Fc0k, Fc90k, 5.0e-6, key)
            .Map(static orthotropic => Seq(orthotropic));
}

// PUBLISHED verbatim: EN 14080:2013 Table 5 / EN 338 Table 1 / EN 14374 declared values (MPa; ρ_k kg/m³).
public static class TimberGrades {
    //                                                     fMk    ft0k  fc0k  fc90k fvk  fRvk  E0Mean  E005    E90  gMean gRoll rhoK  hardwood
    public static readonly TimberGrade Gl24h  = new("gl24h",  24.0, 19.2, 24.0, 2.5, 3.5, 1.2, 11_500,  9_600, 300, 650, 65, 385, false);
    public static readonly TimberGrade Gl28h  = new("gl28h",  28.0, 22.3, 28.0, 2.5, 3.5, 1.2, 12_600, 10_500, 300, 650, 65, 425, false);
    public static readonly TimberGrade Gl30h  = new("gl30h",  30.0, 24.0, 30.0, 2.5, 3.5, 1.2, 13_600, 11_300, 300, 650, 65, 430, false);
    public static readonly TimberGrade Gl32h  = new("gl32h",  32.0, 25.6, 32.0, 2.5, 3.5, 1.2, 14_200, 11_800, 300, 650, 65, 440, false);
    public static readonly TimberGrade Gl24c  = new("gl24c",  24.0, 17.0, 21.5, 2.5, 3.5, 1.2, 11_000,  9_100, 300, 650, 65, 365, false);
    public static readonly TimberGrade Gl28c  = new("gl28c",  28.0, 19.5, 24.0, 2.5, 3.5, 1.2, 12_500, 10_400, 300, 650, 65, 390, false);
    public static readonly TimberGrade C16    = new("c16",    16.0,  8.5, 17.0, 2.2, 3.2, 1.0,  8_000,  5_400, 270, 500, 50, 310, false);
    public static readonly TimberGrade C24    = new("c24",    24.0, 14.5, 21.0, 2.5, 4.0, 1.2, 11_000,  7_400, 370, 690, 69, 350, false);
    public static readonly TimberGrade C30    = new("c30",    30.0, 19.0, 24.0, 2.7, 4.0, 1.2, 12_000,  8_000, 400, 750, 75, 380, false);
    public static readonly TimberGrade D40    = new("d40",    40.0, 24.0, 27.0, 8.3, 4.0, 1.2, 13_000, 10_900, 860, 810, 81, 550, true);
    public static readonly TimberGrade Lvl48p = new("lvl48p", 48.0, 36.0, 40.0, 6.0, 4.6, 2.3, 13_800, 11_700, 430, 760, 76, 510, false);

    public static readonly ImmutableArray<TimberGrade> Rows =
        [Gl24h, Gl28h, Gl30h, Gl32h, Gl24c, Gl28c, C16, C24, C30, D40, Lvl48p];
}

// One authored timber section: form and grade as TYPED refs (an unknown key is unrepresentable — no string
// re-parse, no TryGet rail), the gross breadth/depth, and the LAMINATION BUILD as an explicit per-ply thickness
// sequence (PUBLISHED product data on the form's OWN lamination axis: sawn one full-depth piece, glulam its EN 14080
// 45 mm lamellae across the DEPTH, edgewise P-grade LVL its 3 mm veneers across the WIDTH, CLT its exact manufacturer
// ply build summing to the overall depth — a mixed 30/34/…/30 build is one row the deleted uniform-thickness layup
// could not spell). Cross-ply forms alternate 0°/90° outer-longitudinal.
public readonly record struct TimberRow(string Designation, TimberForm Form, TimberGrade Grade, double WMm, double DMm, Seq<double> BuildMm);

// --- [OPERATIONS] --------------------------------------------------------------------------
// The ONE generator: TimberRows -> SectionProfile.Of -> Component.Of -> ComponentRow. Traverse is the rail — a
// row that cannot admit ABORTS the build (never a swallowed Choose drop). Sectioned = !CrossPly is the frozen
// value-identity pin: member rows populate the section map SectionSolver.Solve fills; a CLT row is unsectioned
// (graph.SectionOf yields None, exactly the prior panel filter) and SectionSolver faults a mis-flagged Layered.
public static class TimberSeed {
    // ComponentAuthority.En, region "eu": the EN 338/14080/14374/16351 + APA PRG 320 span; no mortar joint.
    static readonly ComponentStandard En = new("eu", StandardJointThicknessMm: 0.0, Authority: ComponentAuthority.En);

    static readonly Seq<TimberRow> TimberRows = Seq(
        new TimberRow("timber.sawn-c16-38x89",       TimberForm.Sawn,   TimberGrades.C16,    38.0,  89.0,  Seq(89.0)),
        new TimberRow("timber.sawn-c24-38x140",      TimberForm.Sawn,   TimberGrades.C24,    38.0,  140.0, Seq(140.0)),
        new TimberRow("timber.sawn-c24-38x184",      TimberForm.Sawn,   TimberGrades.C24,    38.0,  184.0, Seq(184.0)),
        new TimberRow("timber.sawn-c30-63x175",      TimberForm.Sawn,   TimberGrades.C30,    63.0,  175.0, Seq(175.0)),
        new TimberRow("timber.sawn-d40-75x225",      TimberForm.Sawn,   TimberGrades.D40,    75.0,  225.0, Seq(225.0)),
        new TimberRow("timber.glulam-gl24h-90x225",  TimberForm.Glulam, TimberGrades.Gl24h,  90.0,  225.0, toSeq(Enumerable.Repeat(45.0, 5))),
        new TimberRow("timber.glulam-gl28h-90x270",  TimberForm.Glulam, TimberGrades.Gl28h,  90.0,  270.0, toSeq(Enumerable.Repeat(45.0, 6))),
        new TimberRow("timber.glulam-gl30h-115x360", TimberForm.Glulam, TimberGrades.Gl30h,  115.0, 360.0, toSeq(Enumerable.Repeat(45.0, 8))),
        new TimberRow("timber.glulam-gl32h-115x405", TimberForm.Glulam, TimberGrades.Gl32h,  115.0, 405.0, toSeq(Enumerable.Repeat(45.0, 9))),
        new TimberRow("timber.glulam-gl28c-140x630", TimberForm.Glulam, TimberGrades.Gl28c,  140.0, 630.0, toSeq(Enumerable.Repeat(45.0, 14))),
        new TimberRow("timber.lvl-lvl48p-75x300",    TimberForm.Lvl,    TimberGrades.Lvl48p, 75.0,  300.0, toSeq(Enumerable.Repeat(3.0, 25))),   // veneers stack across the 75 mm WIDTH (edgewise P-grade)
        new TimberRow("timber.clt-c24-3ply-90",      TimberForm.Clt,    TimberGrades.C24,    1250.0, 90.0,  Seq(30.0, 30.0, 30.0)),
        new TimberRow("timber.clt-c24-5ply-150",     TimberForm.Clt,    TimberGrades.C24,    1250.0, 150.0, Seq(30.0, 30.0, 30.0, 30.0, 30.0)),
        new TimberRow("timber.clt-c24-7ply-230",     TimberForm.Clt,    TimberGrades.C24,    1250.0, 230.0, Seq(30.0, 34.0, 34.0, 34.0, 34.0, 34.0, 30.0)));

    // A member is the Rectangle gross; a cross-ply form is the Layered stack — each ply role-tagged "0"/"90"
    // outer-longitudinal (the gamma-method AND IfcMaterialLayer.Name datum) with the grade-keyed wood MaterialId.
    // Both routes go through the arm's railed Of (the PositiveMagnitude lift + the laminate-build gate live there).
    static Fin<SectionProfile> ProfileOf(TimberRow row, Op key) =>
        row.Form.CrossPly
            ? row.BuildMm.Map((mm, index) => (Mm: mm, Role: (index & 1) == 0 ? "0" : "90"))
                .Traverse(ply => key.AcceptValidated<PositiveMagnitude>(candidate: ply.Mm)
                    .Map(t => new Ply(row.Grade.Substance, t, ply.Role))).As()
                .Bind(plies => SectionProfile.Layered.Of(plies, overallMm: row.DMm, widthMm: row.WMm, key))
            : SectionProfile.Rectangle.Of(row.WMm, row.DMm, key);

    // Each row mints its OWN admission key (the sibling-seed law) — a fault names the exact designation, never the
    // whole-catalogue Context key.
    static Fin<ComponentRow> RowOf(TimberRow row) {
        Op key = Op.Of(name: row.Designation);
        return ProfileOf(row, key).Bind(profile =>
            Component.Of(
                ComponentFamily.Timber, row.Designation, profile,
                IfcBinding.Supertype(ComponentFamily.Timber.Class),
                Coring.None, En,
                substanceId: row.Grade.Substance, appearanceId: row.Grade.Substance,
                detail: Option<PropertyBag>.None, key)
            .Map(item => new ComponentRow(item, Sectioned: !row.Form.CrossPly)));
    }

    // The Context parameter is the ComponentFamily.Rows delegate contract; the timber seed reads no context column.
    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        TimberRows.Traverse(RowOf).As();
}
```

## [03]-[TIMBER_CAPACITY]

- Owner: `TimberDesign` the EN 1995-1-1 design-code operations owner — `Capacity` the ONE design-resistance projection (member-vs-panel discriminated by the profile arm), `EffectiveStiffness` the EN 1995-1-1 Annex B gamma-method kernel over role-tagged plies, `ResidualSection` the EN 1995-1-2 reduced cross-section; `TimberCapacity` the receipt (FROZEN — the `capacity#SECTION_CAPACITY` `SectionCapacity.Lift(TimberCapacity)` overload lifts it WHOLE onto `SectionCapacity.TimberEc5`).
- Cases: one `TimberCapacity` across all forms — design bending `M_Rd,y` (member: `k_crit·k_h·k_mod·f_m,k·W_x/γ_M` over the RESOLVED `ComputedSection.SxMm3` with the §6.3.3 lateral-torsional `k_crit` band; panel: the gamma-method `W_eff = (EI)_eff/(E0·h/2)`), minor-axis bending `M_Rd,z` (member: `k_h(w)·k_mod·f_m,k·S_y/γ_M` over the resolved `SyMm3` — `k_h` over the WIDTH, no `k_crit`; panel: research-GATED 0.0 — the in-plane CLT verification model is unsettled, so an in-plane `Mz` demand governs loud through the capacity `GuardedRatio` law), compression `N_Rd` (§6.3.2 `k_c`-reduced over `E005`; panel over the longitudinal net area and the effective radius `i_ef = √((EI)_eff/(E0·A_0))`), shear `V_Rd` (member `k_cr`-cracked longitudinal; panel rolling-shear `f_R,v,k`), perpendicular bearing `R_90,Rd`, §6.1.8 torsion `T_Rd = k_shape·f_v,d·W_tor`, the governing `λ_rel`, the §6.1.6(2) per-form `k_m` weight, and the applied `k_mod` — a capacity is a derived projection over the resolved section or the ply stack, never a per-form check surface.
- Entry: `public static Fin<TimberCapacity> TimberDesign.Capacity(TimberForm form, TimberGrade grade, SectionProfile profile, Option<ComputedSection> section, ServiceClass service, LoadDuration duration, double effectiveLengthMm, Op key)` — ONE entry, both modalities discriminated by INPUT SHAPE: a `Rectangle` member REQUIRES its resolved receipt (`Some` — the `component#SECTION_SOLVER` solve the catalogue section map / `graph.SectionOf` carries; a `None` member faults `ComponentFault.Section`, the deleted local `ParametricSection.Rectangle` call never re-runs here), a `Layered` panel carries `None` (unsectioned by law) and computes from its plies; a mismatched pair or a non-timber arm faults loud. `public static double EffectiveStiffness(Seq<Ply> plies, TimberGrade grade, double widthMm, double referenceSpanMm)` — the Annex B kernel (N·mm²). `public static ResidualSection TimberDesign.ResidualSection(TimberForm form, TimberGrade grade, SectionProfile profile, double exposureMinutes, int exposedSides)` — TOTAL, clamps to a ≥1 mm residual; a fire re-check calls `Capacity` over the residual rectangle at `k_mod = γ_M = 1.0`.
- Packages: Rasm.Numerics (project — `PositiveMagnitude`), Rasm.Domain (project — `Op`), Rasm.Materials.Component (project — `ComputedSection`/`SectionProfile`/`Ply`/`ComponentFault`), Thinktecture.Runtime.Extensions, LanguageExt.Core; the EN 1995 rules HAND-ROLLED (no .NET EC5 package exists — the SAME hand-roll the steel AISC and RC EC2 checks take), every factor a `TimberForm` column or a `TimberGrade` row read.
- Growth: a new design check is one `TimberCapacity` column plus its arm (a notched-beam `k_v`, a load-sharing `k_sys`); a new fire route one `ResidualSection` parameter (a protected member's `t_ch` delay); a new form's factor set is its `TimberForm` row columns — never a per-form capacity surface, never a re-minted characteristic where the grade row carries it.
- Boundary: the design strength is ALWAYS `f_k·k_mod/γ_M` (§2.4.1) — `k_mod` the `LoadDuration.KmodFor(service)` joint, `γ_M` the per-form column, `k_h` the per-form `(RefDepth, Exponent, Cap)` law (the glulam cap is 1.1 and the LVL law is §3.4 over the declared `s = 0.12` — the flat `600-vs-150 / 0.1-vs-0.2 / cap-1.3` ternaries are the deleted mis-transcription); the member bending resistance carries the §6.3.3 `k_crit` lateral-torsional band over `σ_m,crit = 0.78·b²·E_0,05/(h·l_ef)` — the SAME effective length the §6.3.2 buckling reads, so a deep unbraced glulam no longer credits full `f_m,d`; the member bending modulus is the RESOLVED `ComputedSection.SxMm3` (for a homogeneous member `(EI)_eff ≡ E0·I_gross`, so the gamma expression degrades EXACTLY to `S_x` — the member arm reads the receipt directly and the gamma kernel runs ONLY for the shear-flexible panel); the panel `(EI)_eff` is the CORRECTED Annex B sum `Σ(E0·b·t_i³/12 + γ_i·E0·A_i·z_i²)` over the LONGITUDINAL plies with `γ_i = 1/(1 + π²·E0·t_i·t̄_cross,i/(ℓ²·G_R))` — the per-ply connection efficiency over the ADJACENT transverse ply's rolling-shear slip (the deleted form multiplied `z_i²` into the γ denominator, a non-normative inflation), span-zero → `γ→0` no-composite lower bound, span-∞ → `γ→1` rigid-glued upper bound, non-uniform builds handled by cumulative-offset centroids; panel compression buckles over `i_ef = √((EI)_eff/(E0·A_0))` (the gamma-reduced wall stiffness, never the gross rectangle radius) with the §6.3.2 `k_c` over `E005` and the per-form `β_c`; panel shear reads `f_R,v,k` over the longitudinal net area (the rolling-shear screen), member shear `k_cr·(2/3)·A` longitudinal; a CLT panel in `ServiceClass.Sc3` faults `ComponentFault.Capacity` (EN 16351 — the deleted form designed exposed CLT silently); torsion is the §6.1.8 `T_Rd = k_shape·f_v,d·W_tor` over the Roark rectangular `α·h·b²` (`f_v,d` from the longitudinal `f_v,k`, never `f_R,v,k` — torsional shear flows grain-parallel), the `TorsionalNmm` column the `SectionCapacity.Lift(TimberCapacity)` overload reads onto `TimberEc5.TorsionalKnm` so `T_Ed/T_Rd` folds against a real resistance; the `ResidualSection` char depth is `d_ef = β_n·t + k₀·7 mm` per exposed FACE with `β_n = form.BetaN(grade)` (species-resolved) and `k₀ = min(t/20, 1)` the §4.2.2 zero-strength ramp, faces accruing bottom → both sides → top (three sides the classic beam, four the column — never `sides + 1` faces and never a 7 mm loss at zero exposure); the receipt lifts into the unified rail through `SectionCapacity.Lift(TimberCapacity)` so a timber and a steel column are checked through ONE `Check(demand)` fold differing only in the capacity case — the receipt columns are FROZEN (`BendingNmm`/`BendingMinorNmm`/`CompressionN`/`ShearN`/`BearingPerpN`/`TorsionalNmm`/`RelativeSlenderness`/`Km`/`Kmod`); the member `BendingMinorNmm` is `k_h(w)·k_mod·f_m,k·S_y/γ_M` — `k_h` over the WIDTH (the depth in weak-axis bending), NO `k_crit` because no LTB exists about the minor axis — and the PANEL minor is research-GATED at 0.0 (the Swedish CLT Handbook §3.2.3 parallel-layer net-section model is settled, the verification-strength side is not, pending EN 1995-1-1:2025), so an in-plane panel `Mz` demand surfaces as the governing over-ratio through the capacity `GuardedRatio` law, never a silent pass.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// The EN 1995-1-1 design-resistance receipt — FROZEN: the capacity#SECTION_CAPACITY SectionCapacity.Lift(TimberCapacity)
// overload lifts these nine columns WHOLE onto SectionCapacity.TimberEc5. SI N·mm / N.
public readonly record struct TimberCapacity(
    double BendingNmm,           // M_Rd,y = k_crit·k_h·k_mod·f_m,k·W / γ_M (W = Sx member with the §6.3.3 k_crit band; W_eff panel)
    double BendingMinorNmm,      // M_Rd,z member: k_h(w)·k_mod·f_m,k·Sy / γ_M — no k_crit (no minor-axis LTB); panel 0.0, research-gated
    double CompressionN,         // N_Rd = k_c·k_mod·f_c0,k·A / γ_M (§6.3.2 buckling-reduced)
    double ShearN,               // V_Rd = k_mod·f_v·k_cr·A_shear / γ_M (rolling-shear for a CLT panel)
    double BearingPerpN,         // R_90,Rd = k_mod·f_c90,k·A_bearing / γ_M
    double TorsionalNmm,         // T_Rd = k_shape·f_v,d·W_tor (§6.1.8)
    double RelativeSlenderness,  // λ_rel,c = √(f_c0,k / σ_c,crit) — the §6.3.2 stability input
    double Km,                   // the §6.1.6(2) per-form stress-redistribution weight the biaxial fold swaps
    double Kmod);                // the joint service×duration factor the resistances scaled by

// The EN 1995-1-2 reduced cross-section after d_ef = β_n·t + k₀·d₀ per exposed FACE (β_n species-resolved; d₀ = 7 mm
// with the §4.2.2 k₀ = min(t/20, 1) ramp — zero exposure loses zero section); a fire Capacity re-runs over the
// residual rectangle at k_mod = γ_M = 1.0. TOTAL — clamps to a ≥1 mm residual.
public readonly record struct ResidualSection(PositiveMagnitude ResidualWidthMm, PositiveMagnitude ResidualDepthMm, double CharDepthMm);

// --- [OPERATIONS] --------------------------------------------------------------------------
// The EN 1995-1-1 design projection. HAND-ROLLED (no .NET EC5 package); every factor is a TimberForm column or a
// TimberGrade row read; the member section is the RESOLVED SectionSolver receipt, never a local solve.
public static class TimberDesign {
    // ONE entry, modality by input shape: (Rectangle, Some cs) member · (Layered, None) panel; a mismatched pair
    // or a non-timber arm faults — the family admission (Rectangle|Layered) and the Sectioned pin make the two
    // legal pairs the only reachable ones from catalogue rows.
    public static Fin<TimberCapacity> Capacity(
        TimberForm form, TimberGrade grade, SectionProfile profile, Option<ComputedSection> section,
        ServiceClass service, LoadDuration duration, double effectiveLengthMm, Op key) =>
        (profile, section.Case) switch {
            (SectionProfile.Rectangle r, ComputedSection cs) => Fin.Succ(Member(form, grade, r, cs, service, duration, effectiveLengthMm)),
            (SectionProfile.Rectangle, _)                    => Fin.Fail<TimberCapacity>(ComponentFault.Section(key, "<timber-member-section-unresolved>")),
            (SectionProfile.Layered, null) when service.KdefFor(form).IsNone   // the ONE permission law — the EN 16351 CLT-in-SC3 bar is the policy row's None, never a re-spelled == Sc3
                                                             => Fin.Fail<TimberCapacity>(ComponentFault.Capacity(key, "<clt-not-permitted-service-class-3>")),
            (SectionProfile.Layered l, null)                 => Fin.Succ(Panel(form, grade, l, service, duration, effectiveLengthMm)),
            (SectionProfile.Layered, _)                      => Fin.Fail<TimberCapacity>(ComponentFault.Section(key, "<timber-panel-carries-no-section>")),
            _                                                => Fin.Fail<TimberCapacity>(ComponentFault.Family(key, $"<timber-profile-unsupported:{profile.GetType().Name}>")),
        };

    // Member: the resolved twenty-column receipt supplies W = SxMm3 (for a homogeneous member (EI)_eff ≡ E0·I, so
    // W_eff degrades EXACTLY to Sx), area, and the derived weak-axis GoverningRadiusMm; k_h/k_cr/β_c/k_m/γ_M are form
    // columns. Major bending carries the §6.3.3 k_crit LTB band over σ_m,crit = 0.78·b²·E_0,05/(h·l_ef) — the SAME
    // effective length serves §6.3.2 buckling and §6.3.3 bracing (the one-length modality; a deep unbraced glulam
    // no longer credits full f_m,d): 1 for λ_rel,m ≤ 0.75, 1.56 − 0.75·λ_rel,m to 1.4, 1/λ_rel,m² beyond. Minor
    // bending reads Kh over the WIDTH (the depth in weak-axis bending) with NO k_crit — no LTB exists about the
    // minor axis.
    static TimberCapacity Member(TimberForm f, TimberGrade g, SectionProfile.Rectangle r, ComputedSection cs, ServiceClass sc, LoadDuration ld, double lengthMm) {
        double kmod = ld.KmodFor(sc), gammaM = f.GammaM;
        double w = r.WidthMm.Value, d = r.DepthMm.Value, area = cs.AreaMm2.Value;
        (double kc, double lambdaRel) = BucklingKc(f, g, Slenderness(lengthMm, cs.GoverningRadiusMm));
        double sigmaMCrit = lengthMm > 0.0 ? 0.78 * w * w * g.E005 / (d * lengthMm) : double.PositiveInfinity;
        double lambdaRelM = double.IsPositiveInfinity(sigmaMCrit) ? 0.0 : Math.Sqrt(g.Fmk / sigmaMCrit);
        double kcrit = lambdaRelM <= 0.75 ? 1.0 : lambdaRelM <= 1.4 ? 1.56 - 0.75 * lambdaRelM : 1.0 / (lambdaRelM * lambdaRelM);
        return Receipt(g, kmod, gammaM,
            bendingNmm: kcrit * f.Kh(d) * kmod * g.Fmk * cs.SxMm3.Value / gammaM,
            bendingMinorNmm: f.Kh(w) * kmod * g.Fmk * cs.SyMm3.Value / gammaM,
            kc, area,
            shearN: kmod * g.Fvk * f.KcrCrack * area * 2.0 / 3.0 / gammaM,
            w, d, lambdaRel, km: f.Km);
    }

    // Panel: the gamma-method (EI)_eff drives flexure (W_eff = (EI)_eff/(E0·h/2)), the longitudinal net area and
    // the EFFECTIVE radius i_ef = √((EI)_eff/(E0·A_0)) drive wall buckling (the gamma-reduced stiffness, never the
    // gross rectangle), the rolling-shear f_R,v,k screens shear over the longitudinal area. The minor column is
    // RESEARCH-GATED at 0.0: in-plane CLT bending resolves over the parallel-layer net section (Swedish CLT
    // Handbook §3.2.3) but the verification-strength side (f_m vs the f_t/f_c edge model, k_sys) is unsettled and
    // ETA-dependent pending EN 1995-1-1:2025 — the gated 0 makes an in-plane Mz demand surface as the governing
    // over-ratio per the capacity GuardedRatio law, never a silent pass.
    static TimberCapacity Panel(TimberForm f, TimberGrade g, SectionProfile.Layered l, ServiceClass sc, LoadDuration ld, double lengthMm) {
        double kmod = ld.KmodFor(sc), gammaM = f.GammaM;
        double b = l.WidthMm.Value, h = l.OverallMm.Value;
        double areaLong = b * l.Plies.Filter(static p => p.Role == "0").Sum(static p => p.ThicknessMm.Value);
        double eiEff = EffectiveStiffness(l.Plies, g, b, lengthMm);
        double iEff = areaLong > 0.0 ? Math.Sqrt(eiEff / (g.E0Mean * areaLong)) : 0.0;
        (double kc, double lambdaRel) = BucklingKc(f, g, Slenderness(lengthMm, iEff));
        return Receipt(g, kmod, gammaM,
            bendingNmm: kmod * g.Fmk * (eiEff / (g.E0Mean * h * 0.5)) / gammaM,
            bendingMinorNmm: 0.0,
            kc, areaLong,
            shearN: kmod * g.FRvk * areaLong / gammaM,
            b, h, lambdaRel, km: f.Km);
    }

    // The shared receipt tail: compression, full-face bearing, and the §6.1.8 rectangular torsion (f_v,d from the
    // LONGITUDINAL f_v,k — torsional shear flows grain-parallel; W_tor the Roark α·h·b² the steel SolidShape J
    // shares; k_shape = min(1 + 0.15·h/b, 2.0)).
    static TimberCapacity Receipt(TimberGrade g, double kmod, double gammaM, double bendingNmm, double bendingMinorNmm, double kc, double areaMm2, double shearN, double wMm, double dMm, double lambdaRel, double km) {
        double bTor = Math.Min(wMm, dMm), hTor = Math.Max(wMm, dMm);
        double alpha = 1.0 / 3.0 - 0.21 * (bTor / hTor) * (1.0 - Math.Pow(bTor / hTor, 4.0) / 12.0);
        double fvd = kmod * g.Fvk / gammaM;
        return new TimberCapacity(
            BendingNmm: bendingNmm,
            BendingMinorNmm: bendingMinorNmm,
            CompressionN: kc * kmod * g.Fc0k * areaMm2 / gammaM,
            ShearN: shearN,
            BearingPerpN: kmod * g.Fc90k * wMm * dMm / gammaM,
            TorsionalNmm: Math.Min(1.0 + 0.15 * hTor / bTor, 2.0) * fvd * alpha * hTor * bTor * bTor,
            RelativeSlenderness: lambdaRel,
            Km: km,
            Kmod: kmod);
    }

    // §6.3.2 column buckling over the E005 stability fractile and the per-form β_c imperfection; λ_rel ≤ 0.3 is
    // the stocky no-reduction branch.
    static (double Kc, double LambdaRel) BucklingKc(TimberForm f, TimberGrade g, double lambda) {
        double sigmaCrit = lambda > 0.0 ? Math.PI * Math.PI * g.E005 / (lambda * lambda) : double.PositiveInfinity;
        double lambdaRel = double.IsPositiveInfinity(sigmaCrit) ? 0.0 : Math.Sqrt(g.Fc0k / sigmaCrit);
        double k = 0.5 * (1.0 + f.BetaC * (lambdaRel - 0.3) + lambdaRel * lambdaRel);
        return (lambdaRel <= 0.3 ? 1.0 : 1.0 / (k + Math.Sqrt(Math.Max(0.0, k * k - lambdaRel * lambdaRel))), lambdaRel);
    }

    static double Slenderness(double lengthMm, double radiusMm) =>
        lengthMm > 0.0 && radiusMm > 0.0 ? lengthMm / radiusMm : 0.0;

    // EN 1995-1-1 Annex B gamma method over role-tagged, possibly NON-UNIFORM plies (N·mm²): only the "0" plies
    // carry bending; each off-centre longitudinal ply's Steiner term is reduced by γ_i = 1/(1 + π²·E0·t_i·t̄_i/
    // (ℓ²·G_R)) with t̄_i the ADJACENT transverse ply thickness toward the panel middle (Eq B.5 slip K = G_R·b/t̄
    // — the width cancels; no z² rides the denominator). Span-zero → γ→0 (no composite); span-∞ → γ→1 (rigid);
    // a NON-POSITIVE span reads the SAME γ→0 no-composite lower bound — the prior ∞ mapping silently credited the
    // rigid-glued UPPER bound to a degenerate input. Centroids come from cumulative offsets, so a mixed
    // 30/34/…/30 build integrates exactly.
    public static double EffectiveStiffness(Seq<Ply> plies, TimberGrade grade, double widthMm, double referenceSpanMm) {
        double depth = plies.Sum(static p => p.ThicknessMm.Value), half = depth * 0.5, e0 = grade.E0Mean;
        double span2 = referenceSpanMm > 0.0 ? referenceSpanMm * referenceSpanMm : 0.0;
        var placed = plies.Fold(
            (Offset: 0.0, Rows: Seq<(double T, double Z, string Role, int Index)>()),
            (acc, p) => (acc.Offset + p.ThicknessMm.Value,
                acc.Rows.Add((p.ThicknessMm.Value, acc.Offset + p.ThicknessMm.Value * 0.5 - half, p.Role, acc.Rows.Count)))).Rows;
        return placed.Filter(static row => row.Role == "0").Sum(row => {
            double own = e0 * widthMm * row.T * row.T * row.T / 12.0;
            int crossAt = row.Z <= 0.0 ? row.Index + 1 : row.Index - 1;   // the adjacent transverse ply TOWARD the panel middle
            double tCross = placed
                .Filter(other => other.Index == crossAt && other.Role == "90")
                .Map(static other => other.T).HeadOrNone().IfNone(0.0);
            double gamma = tCross > 0.0
                ? 1.0 / (1.0 + Math.PI * Math.PI * e0 * row.T * tCross / (span2 * grade.GRollMean))
                : 1.0;
            return own + gamma * e0 * widthMm * row.T * row.Z * row.Z;
        });
    }

    // EN 1995-1-2 reduced cross-section: d_ef = β_n·t + k₀·7 mm removed per exposed FACE, β_n = form.BetaN(grade)
    // (species-resolved), k₀ the §4.2.2 min(t/20, 1) zero-strength-layer ramp. Faces accrue bottom -> both sides ->
    // top: 1 chars the depth once, 2 adds one width face, 3 the classic beam (both sides + bottom), 4 the fully
    // exposed column — the deleted mapping charred sides+1 faces for 1..3 and charred at t = 0. TOTAL — clamps to
    // ≥1 mm; the residual feeds a fire Capacity that owns its own admission.
    public static ResidualSection ResidualSection(TimberForm form, TimberGrade grade, SectionProfile profile, double exposureMinutes, int exposedSides) {
        double t = Math.Max(0.0, exposureMinutes);
        double charMm = form.BetaN(grade) * t + 7.0 * Math.Min(t / 20.0, 1.0);
        (double w, double d) = (profile.GrossRectangleMm.WidthMm.Value, profile.GrossRectangleMm.DepthMm.Value);
        double residW = Math.Max(1.0, w - Math.Min(2, Math.Max(0, exposedSides - 1)) * charMm);
        double residD = Math.Max(1.0, d - ((exposedSides >= 1 ? 1 : 0) + (exposedSides >= 4 ? 1 : 0)) * charMm);
        return new ResidualSection(PositiveMagnitude.Create(residW), PositiveMagnitude.Create(residD), charMm);
    }
}
```

## [04]-[RESEARCH]

- [TIMBER_GRADE_DATA]: REALIZED — the `TimberGrade` frozen row table carries the FULL EN 14080/338/14374 characteristic strength-AND-stiffness vector verbatim (PUBLISHED: `E_90,g,mean = 300` for every glulam class including the combined gl24c/gl28c — the MEAN modulus, never the 250 fractile; C24 `f_t,0,k = 14.5`; D40 the dense-hardwood vector — `f_c,90,k = 8.3` the hardwood bearing band, `E_0,05 = 10900` the `0.84·E_0` hardwood fractile, `E_90 = 860`/`G = 810`, `ρ_k = 550` — the prior softwood-band `f_c,90,k = 2.9`/`E_0,05 = 9400` row was a mixed-edition mis-transcription) plus the `Hardwood` species column the EN 1995-1-2 charring rate discriminates on. `VividOrange.Materials` ships NO timber factory (`[FLOOR_SCOPE_GATE]` — EN factories are concrete/steel/rebar only), so the row table is the AUTHORED registered-class owner under `SEED_ROW_LAW`; R1 (a future admitted timber-table producer) would re-class the columns `VENDOR` with zero shape change. A new class is one row; seed rows reference the `TimberGrades` statics directly, so an unknown grade is unrepresentable.
- [ORTHOTROPIC_STIFFNESS_LAW]: REALIZED on the grade row — `TimberGrade.OrthotropicLaw` produces the verified `LinearElasticOrthotropicMaterial(MaterialType.Timber, Pressure Ex, Sx, Ey, Sy, Ez, Sz)` (`E0,mean` parallel X, `E90,mean` perpendicular Y/Z, per-axis strengths — the INDEPENDENT-`G` material whose `G_mean ≈ E0/16` no Poisson derivation reaches), and `TimberGrade.ToProperties` lowers the SAME scalars seam-neutrally through `MaterialPropertySet.OfOrthotropic(density, e1Parallel, e2Perpendicular, shearModulus, strength1Parallel, strength2Perpendicular, thermalExpansion, key)` onto the seam `Orthotropic` structural case the projected `Material` node carries and `Rasm.Compute` reads. Both moved off the deleted `TimberSection` payload — they read ONLY grade scalars, so the grade row is their one true owner.
- [LAYERED_GEOMETRY_AND_COMPOSITION]: REALIZED — the cross-lamination is the `SectionProfile.Layered` arm itself: per-ply thickness and the `"0"`/`"90"` `Ply.Role` are the load-bearing data (non-uniform manufacturer builds sum exactly to the overall — the 7-ply 230 is `30/34/34/34/34/34/30`, a build the deleted uniform layup could not spell), the gamma-method and the IFC layer names read the SAME rows, and the member-vs-panel IFC wire shape (`IfcMaterialProfileSet` vs `IfcMaterialLayerSet`) is the `Projection/component#COMPOSITION_SELECTION` read of the `Sectioned` pin and the `Layered` arm — `ToLayerSet`/`ToUnit`/`MaterialShapeKind` deleted, `Component` never reaches the Projection-homed `CompositionAuthor`. CLT rows pin `Sectioned: false` (the frozen `graph.SectionOf` None identity; `SectionSolver` faults a mis-flagged `Layered`), member rows `true`.
- [EC5_DESIGN_CAPACITY]: REALIZED — `TimberDesign.Capacity` is ONE projection over the RESOLVED `ComputedSection` (member) or the ply stack (panel), modality discriminated by input shape. The per-form EC5 factors are `TimberForm` DATA columns collapsing the deleted enumerated ternaries AND correcting their mis-transcriptions: the glulam `k_h` cap is 1.1 (§3.3, not the flat 1.3), LVL takes the §3.4 law (300 mm reference, declared `s = 0.12`, cap 1.2, `k_cr = 1.0`), `β_c = 0.1` for every engineered lamination including LVL (§6.3.2), and CLT/PSL carry no `k_h` clause (`kh ≡ 1.0`). The member arm carries the §6.3.3 `k_crit` lateral-torsional band and buckles over the receipt's derived `GoverningRadiusMm` (never a re-derived `min(Rx, Ry)`); the panel arm buckles over the gamma-reduced `i_ef` (never the gross radius), gates the CLT-in-SC3 bar by READING `ServiceClass.KdefFor`'s `None` row (the one permission law, never a re-spelled `== Sc3`), and the Annex B `γ_i = 1/(1 + π²·E0·t_i·t̄_cross/(ℓ²·G_R))` drops the non-normative `z²` the deleted kernel carried, a non-positive span reading the conservative no-composite `γ→0` bound. The receipt carries BOTH bending axes: the member `BendingMinorNmm = k_h(w)·k_mod·f_m,k·S_y/γ_M` (the `TimberForm.Kh` law reused over the WIDTH, no `k_crit` — no minor-axis LTB) and the per-form §6.1.6(2) `Km` column (0.7 rectangular solid/glulam/LVL, 1.0 CLT/PSL — the per-form-column collapse pattern `kh`/`kcr`/`betaC` already ride; a flat 0.7 const is unconservative for PSL/CLT) the capacity km-swapped biaxial pair reads; the PANEL `BendingMinorNmm` is research-GATED at 0.0 (in-plane CLT bending resolves over the parallel-layer net section per the Swedish CLT Handbook §3.2.3, but the verification-strength side — `f_m` vs the `f_t`/`f_c` edge model, `k_sys` — is unsettled and ETA-dependent pending EN 1995-1-1:2025), so an in-plane panel `Mz` demand governs loud through the capacity `GuardedRatio` law rather than passing silent. The `TimberCapacity` receipt is FROZEN — `SectionCapacity.Lift(TimberCapacity)` lifts `BendingNmm/BendingMinorNmm/CompressionN/ShearN/BearingPerpN/TorsionalNmm/RelativeSlenderness/Km/Kmod` WHOLE onto `SectionCapacity.TimberEc5`, so a timber and a steel column check through the SAME `Check(demand)` fold and `T_Ed/T_Rd` folds against a real §6.1.8 resistance; the consumer-less `FlexuralKnm`/`CompressionKn` convenience props duplicating the lift conversion are the DELETED form.
- [SERVICE_CREEP_AXIS]: `ServiceClass` carries the EN 1995-1-1 Table 3.2 `k_def` as FORM-JOINT data — the solid/glulam/LVL column and the EN 16351 CLT column, with `KdefFor(form)` returning `None` for the barred CLT-in-SC3 combination (the same exclusion the `Capacity` panel arm faults on). A serviceability consumer reads the creep factor off the policy row, never a flat service-only constant that smears the CLT deviation.
