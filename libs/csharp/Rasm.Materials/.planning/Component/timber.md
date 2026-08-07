# [MATERIALS_TIMBER]

THE TIMBER SEED FAMILY GROUNDED IN THE EN STRENGTH-CLASS TABLES. A sawn/glulam/LVL/PSL member and a cross-laminated panel are each one `ComponentRow` minted by the ONE generator `TimberSeed.Rows -> Component.Of` over the `ComponentFamily.Timber` policy row (`ComponentClass.Primary`, `DetailLane.None`, admits a `SectionProfile.Rectangle` or a `SectionProfile.Layered` containing only `PlyRole.Longitudinal`/`PlyRole.Transverse`, cross-nominal the section depth) — never a `GlulamBeam`/`CltPanel` type, never a hand-keyed strength literal, and never a bespoke `TimberSection` payload. A profiled MEMBER is `SectionProfile.Rectangle` and solvable; a CLT PANEL is `SectionProfile.Layered`, which carries no section integral and therefore no section-map entry. `TimberSeed.Resolve` joins either resolved `ComponentId` back to the typed `TimberRow`, preserving `TimberForm` and `TimberGrade` for `TimberDesign.Capacity` without widening `Component` or adding a detail bag. `TimberDesign` owns the EN 1995-1-1 capacity and Annex B stiffness operations plus the EN 1995-1-2 member and CLT fire modalities; `TimberCapacity` is the frozen receipt the `capacity#SECTION_CAPACITY` `SectionCapacity.Lift(CapacityReceipt)` `CapacityReceipt.Timber` case consumes.

## [01]-[INDEX]

- [02]-[TIMBER_FAMILY]: `TimberForm`, `TimberGrade`, `ServiceClass`, `LoadDuration`, the authored `TimberRow` table, and `TimberSeed.Rows`/`Resolve`.
- [03]-[TIMBER_CAPACITY]: the `TimberDesign.Capacity` EN 1995-1-1 design-resistance projection (one entry, member-vs-panel discriminated by the admitted form/profile correspondence), the corrected Annex B gamma-method `EffectiveStiffness` over non-uniform role-tagged plies, the EN 1995-1-2 `ResidualSection` reduced cross-section and `ResidualStack` CLT step-charring ply modality, and the frozen `TimberCapacity` receipt the `capacity#SECTION_CAPACITY` rail lifts.

## [02]-[TIMBER_FAMILY]

- Owner: `TimberForm` the product-form policy row (sawn/glulam/clt/lvl/psl); `TimberGrade` the frozen characteristic-vector row table plus the `TimberGrades` roster; `ServiceClass`/`LoadDuration` the EC5 modification-factor axes; `TimberRow` the authored section table; `TimberSeed` the `Rows` fold and typed `Resolve` join.
- Cases: form {sawn (EN 338 solid), glulam (EN 14080 lamellae), clt (EN 16351 / APA PRG 320 cross-laminated), lvl (EN 14374 veneer), psl (parallel-strand, ETA-governed)} × grade {gl24h/gl28h/gl30h/gl32h homogeneous + gl24c/gl28c combined glulam, c16/c24/c30 sawn softwood, d40 sawn hardwood, lvl48p} × service {sc1/sc2/sc3} × duration {permanent/long/medium/short/instantaneous} — a section is one `TimberRow` over one form/grade plus its lamination build; a member is `SectionProfile.Rectangle`, a CLT panel `SectionProfile.Layered`, never a section subtype.
- Entry: `TimberSeed.Rows(Context) : Fin<Seq<ComponentRow>>` traverses the typed table, builds each railed profile, and constructs `Component.Of`; one malformed row aborts the catalogue, and section-map membership derives from the built profile's own topology rather than from the form's cross-ply flag read a second time. `TimberSeed.Resolve(Component, Op) : Fin<TimberRow>` performs one typed lookup by the resolved component's `ComponentId` and faults if a foreign or unregistered timber component reaches design.
- Packages: Rasm.Numerics (project — `PositiveMagnitude` every length column), Rasm.Domain (project — `Context`/`Op`), Rasm.Element (project — `MaterialId`, `MaterialPropertySet` the `ToProperties` lowering mints), VividOrange.Materials (`LinearElasticOrthotropicMaterial`/`MaterialType.Timber` the along/across-grain law; `.api/api-vividorange-materials.md` — the EN factories are concrete/steel/rebar only, probe-confirmed, so NO timber factory exists and the `TimberGrade` rows are the AUTHORED registered-class owner), UnitsNet (`Pressure.FromMegapascals` coercing the grade scalars at the orthotropic edge; `libs/csharp/.api/api-unitsnet.md`), Rasm.Materials.Component (project — the parent `component#COMPONENT_OWNER` `Component`/`ComponentRow`/`ComponentFamily`/`ComponentFault`/`SectionProfile`/`Ply`/`IfcBinding`/`ComputedSection`/`ComponentStandard`/`ComponentAuthority`/`Coring`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new section is one `TimberRow` (its lamination build a `Seq<double>` column); a new strength class one `TimberGrade` row in the `TimberGrades` roster; a new product form one `TimberForm` row carrying its cross-ply flag, charring base, `γ_M`, and `k_h`/`k_cr`/`β_c`/`k_m` columns; a new service/duration point one `ServiceClass`/`LoadDuration` row — never a per-member type, never a hand-keyed strength literal, never a re-minted EC5 factor beside the form row.
- Boundary: `TimberGrade` is the `SEED_ROW_LAW` `AUTHORED` owner of the published EN strength, stiffness, density, and species columns; `Hardwood` selects the EN 1995-1-2 charring band, while `OrthotropicLaw` and `ToProperties` lower the same grade scalars through `LinearElasticOrthotropicMaterial` and `MaterialPropertySet.OfOrthotropic`. `SectionProfile.Layered` carries each physical ply thickness plus the bounded `PlyRole.Longitudinal`/`PlyRole.Transverse` discriminant, and `ComponentFamily.Timber.Admits` rejects every other known `PlyRole`. the `ComponentFamily.Timber.Ifc` concrete leaf leaves beam/column/brace occurrence refinement outside Materials, `DetailLane.None` forbids a duplicate bag, and `TimberSeed.Resolve` restores the authored form/grade axes by the same `ComponentId` minted during seeding.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;                     // FrozenDictionary — the ComponentId-keyed TimberRow join
using System.Collections.Immutable;                  // ImmutableArray (the frozen TimberGrades roster)
using LanguageExt;                                   // Fin, Seq, Option, Traverse
using Rasm.Numerics;                                  // PositiveMagnitude — the kernel >0 finite magnitude atom (Rasm.Numerics, NOT Rasm.Domain)
using Rasm.Domain;                                   // Context, Op, AcceptValidated
using Rasm.Element.Composition;                                  // MaterialId, MaterialPropertySet (the seam Orthotropic lowering target), PropertyBag
using Rasm.Element.Properties;
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
// glulam/LVL ρ_k ≥ 480/PSL 0.70), the EN 1995-1-1 Table 2.3 material partial factor γ_M (solid 1.30, glulam/CLT/PSL
// 1.25, LVL 1.20 — the design-strength divisor, never flat; the CLT figure follows the national annexes and
// European technical assessments that govern the product, none of which grant it the LVL value), and the per-form EC5 capacity columns that
// COLLAPSE the deleted enumerated ternaries: the §3.2/§3.3/§3.4 depth size effect (reference depth, exponent, cap
// — solid 150/0.2/1.3, glulam 600/0.1/1.1 the CAP the flat-1.3 form over-credited, LVL 300 mm at the EN 14374
// declared s = 0.12 capped 1.2; CLT/PSL carry no clause, kh ≡ 1.0), the §6.1.7 shear crack factor k_cr (0.67
// solid AND glulam only — other wood-based products 1.0, the flat-0.67 form under-checked LVL/PSL), the
// §6.3.2 straightness imperfection β_c (solid 0.2; every engineered lamination 0.1), and the §6.1.6(2)
// stress-redistribution k_m (0.7 rectangular solid/glulam/LVL, 1.0 other wood-based products — CLT/PSL; the
// biaxial-interaction weight the capacity fold reads, a per-form column, never a flat constant that is
// unconservative for PSL/CLT). A new engineered product (box beam, curved glulam) is one row, never a per-member type.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TimberForm {
    public static readonly TimberForm Sawn   = new("sawn",   crossPly: false, betaNSoftwoodMmPerMin: 0.80, gammaM: 1.30, khRefDepthMm: 150.0, khExponent: 0.20, khCap: 1.3, kcrCrack: 0.67, betaC: 0.2, km: 0.7);
    public static readonly TimberForm Glulam = new("glulam", crossPly: false, betaNSoftwoodMmPerMin: 0.70, gammaM: 1.25, khRefDepthMm: 600.0, khExponent: 0.10, khCap: 1.1, kcrCrack: 0.67, betaC: 0.1, km: 0.7);
    public static readonly TimberForm Clt    = new("clt",    crossPly: true,  betaNSoftwoodMmPerMin: 0.80, gammaM: 1.25, khRefDepthMm: 0.0,   khExponent: 0.0,  khCap: 1.0, kcrCrack: 1.00, betaC: 0.1, km: 1.0);
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
// governor), E0Mean/E005 axial moduli (E005 the 5% stability fractile — ≈0.67·E0Mean across the softwood classes,
// 0.84·E0Mean on an EN 338 hardwood D-class), E90Mean perpendicular, gMean/gRollMean shear moduli, densityK
// characteristic density (self-weight + embedment input). Hardwood is the EN 338 SPECIES
// class (D-grades) the EN 1995-1-2 charring rate discriminates on. VividOrange.Materials ships NO timber factory
// (.api/api-vividorange-materials.md), so this table IS the registered timber-class owner.
public readonly record struct TimberGrade(
    string Key, double Fmk, double Ft0k, double Fc0k, double Fc90k, double Fvk, Option<double> FRvk,
    double E0Mean, double E005, double E90Mean, double GMean, double DensityK, double K90Base,
    Option<double> FmEdgeK = default) {

    // ROLLING SHEAR MODULUS as a DERIVATION, never a stored column: EN 14080 publishes Gr,mean = 0.10 * Gg,mean and the
    // German annex restates the ratio, so a stored value could only ever agree with G or contradict it. A row whose
    // product publishes no rolling-shear STRENGTH still has a modulus by this ratio — the strength is what is absent.
    public double GRollMean => GMean * RollingShearRatio;
    const double RollingShearRatio = 0.10;

    public MaterialId Substance => MaterialId.Of($"wood.{Key}");

    // EDGEWISE bending strength — the in-plane column the cross-laminated net-section arm reads, absent on every row
    // whose product standard publishes flatwise strength alone. It is deliberately NOT defaulted to Fmk: a lamella's
    // flat bending strength is measured across the layup's own thickness and an in-plane layup develops a different
    // one, so borrowing the flat value would price a lintel off a test that never loaded a panel that way.

    // The EN 1995-1-1 §8.5.1.1 embedment k90 INTERCEPT, per material family: 1.35 softwood, 1.30 LVL, 0.90
    // hardwood, each carried to the load-to-grain angle as k90 = base + 0.015·d by the fastener family's own
    // Johansen fold. It replaces a bare `Hardwood` boolean because the species axis is a NUMBER two clauses read —
    // the charring band and the embedment reduction — and a boolean can only serve the first, forcing the second to
    // re-derive a coefficient from a flag. Hardwood is now what the number SAYS rather than a second stored fact.
    public bool Hardwood => K90Base < 1.0;

    // The along/across-grain directional-stiffness law the structural solver reads — the verified 7-arg ctor
    // (MaterialType, Ex, Sx, Ey, Sy, Ez, Sz), modulus then strength per axis, over MaterialType.Timber — the one
    // live timber member, the per-product SolidTimber/GluedLaminatedTimber spellings closed. The INDEPENDENT-G orthotropic material
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
// The EN 1995-1-1 Table 2.3 partial factors this family does not carry on a form row: a CONNECTION is verified at
// its own γ_M regardless of which product the members are, so the factor belongs to the joint rather than to either
// side's form. The fastener family's dowel-type fold divides by it.
public static class TimberPartialFactor {
    public const double Connection = 1.30;
}

public static class TimberGrades {
    // EN 338:2016 Table 1 — the twelve softwood classes. Rolling shear is ABSENT on every sawn row because
    // EN 338 publishes none: a solid sawn section has no cross layer to roll, and the value the roster
    // previously carried was an engineered-product figure standing in for a column the standard leaves empty.
    //                                                    fMk    ft0k   fc0k  fc90k  fvk   E0Mean   E005    E90   gMean  rhoK  k90
    public static readonly TimberGrade C14    = new("c14",    14,   7.2,   16,    2,    3, None,   7000,   4700,  230,  440,  290, 1.35);
    public static readonly TimberGrade C16    = new("c16",    16,   8.5,   17,  2.2,  3.2, None,   8000,   5400,  270,  500,  310, 1.35);
    public static readonly TimberGrade C18    = new("c18",    18,    10,   18,  2.2,  3.4, None,   9000,   6000,  300,  560,  320, 1.35);
    public static readonly TimberGrade C20    = new("c20",    20,  11.5,   19,  2.3,  3.6, None,   9500,   6400,  320,  590,  330, 1.35);
    public static readonly TimberGrade C22    = new("c22",    22,    13,   20,  2.4,  3.8, None,  10000,   6700,  330,  630,  340, 1.35);
    public static readonly TimberGrade C24    = new("c24",    24,  14.5,   21,  2.5,    4, None,  11000,   7400,  370,  690,  350, 1.35);
    public static readonly TimberGrade C27    = new("c27",    27,  16.5,   22,  2.5,    4, None,  11500,   7700,  380,  720,  360, 1.35);
    public static readonly TimberGrade C30    = new("c30",    30,    19,   24,  2.7,    4, None,  12000,   8000,  400,  750,  380, 1.35);
    public static readonly TimberGrade C35    = new("c35",    35,  22.5,   25,  2.7,    4, None,  13000,   8700,  430,  810,  390, 1.35);
    public static readonly TimberGrade C40    = new("c40",    40,    26,   27,  2.8,    4, None,  14000,   9400,  470,  880,  400, 1.35);
    public static readonly TimberGrade C45    = new("c45",    45,    30,   29,  2.9,    4, None,  15000,  10100,  500,  940,  410, 1.35);
    public static readonly TimberGrade C50    = new("c50",    50,  33.5,   30,    3,    4, None,  16000,  10700,  530, 1000,  430, 1.35);

    // EN 338:2016 Table 3 — the fourteen hardwood classes. The fc,90,k jump at D60 is the EN 384 rule's own
    // second branch taking over where rho_k reaches 700, and the fv,k plateau from D65 is its fm,k > 60 branch;
    // neither is a transcription artefact. k90 is 0.90 across the D classes, which is what makes Hardwood true.
    public static readonly TimberGrade D18    = new("d18",    18,    11,   18,  4.8,  3.5, None,   9500,   8000,  630,  590,  475, 0.90);
    public static readonly TimberGrade D24    = new("d24",    24,    14,   21,  4.9,  3.7, None,  10000,   8400,  670,  630,  485, 0.90);
    public static readonly TimberGrade D27    = new("d27",    27,    16,   22,  5.1,  3.8, None,  10500,   8800,  700,  660,  510, 0.90);
    public static readonly TimberGrade D30    = new("d30",    30,    18,   24,  5.3,  3.9, None,  11000,   9200,  730,  690,  530, 0.90);
    public static readonly TimberGrade D35    = new("d35",    35,    21,   25,  5.4,  4.1, None,  12000,  10100,  800,  750,  540, 0.90);
    public static readonly TimberGrade D40    = new("d40",    40,    24,   27,  5.5,  4.2, None,  13000,  10900,  870,  810,  550, 0.90);
    public static readonly TimberGrade D45    = new("d45",    45,    27,   29,  5.8,  4.4, None,  13500,  11300,  900,  840,  580, 0.90);
    public static readonly TimberGrade D50    = new("d50",    50,    30,   30,  6.2,  4.5, None,  14000,  11800,  930,  880,  620, 0.90);
    public static readonly TimberGrade D55    = new("d55",    55,    33,   32,  6.6,  4.7, None,  15500,  13000, 1030,  970,  660, 0.90);
    public static readonly TimberGrade D60    = new("d60",    60,    36,   33, 10.5,  4.8, None,  17000,  14300, 1130, 1060,  700, 0.90);
    public static readonly TimberGrade D65    = new("d65",    65,    39,   35, 11.3,    5, None,  18500,  15500, 1230, 1160,  750, 0.90);
    public static readonly TimberGrade D70    = new("d70",    70,    42,   36,   12,    5, None,  20000,  16800, 1330, 1250,  800, 0.90);
    public static readonly TimberGrade D75    = new("d75",    75,    45,   37, 12.8,    5, None,  22000,  18500, 1470, 1380,  850, 0.90);
    public static readonly TimberGrade D80    = new("d80",    80,    48,   38, 13.5,    5, None,  24000,  20200, 1600, 1500,  900, 0.90);

    // EN 14080:2013 Tables 4 and 5 — the seven COMBINED and seven HOMOGENEOUS glulam classes. Every class shares
    // ft,90,g,k 0.5, fc,90,g,k 2.5, fv,g,k 3.5, fr,g,k 1.2, E90 300, and G 650, so those ride the mint below
    // rather than repeating down fourteen rows. Rolling shear IS published here: 1.2 strength over the 65
    // modulus the GRollMean derivation reads.
    public static readonly TimberGrade Gl20c  = Glulam("gl20c",    20,    15, 18.5,  10400,   8600,  355);
    public static readonly TimberGrade Gl22c  = Glulam("gl22c",    22,    16,   20,  10400,   8600,  355);
    public static readonly TimberGrade Gl24c  = Glulam("gl24c",    24,    17, 21.5,  11000,   9100,  365);
    public static readonly TimberGrade Gl26c  = Glulam("gl26c",    26,    19, 23.5,  12000,  10000,  385);
    public static readonly TimberGrade Gl28c  = Glulam("gl28c",    28,  19.5,   24,  12500,  10400,  390);
    public static readonly TimberGrade Gl30c  = Glulam("gl30c",    30,  19.5, 24.5,  13000,  10800,  390);
    public static readonly TimberGrade Gl32c  = Glulam("gl32c",    32,  19.5, 24.5,  13500,  11200,  400);
    public static readonly TimberGrade Gl20h  = Glulam("gl20h",    20,    16,   20,   8400,   7000,  340);
    public static readonly TimberGrade Gl22h  = Glulam("gl22h",    22,  17.6,   22,  10500,   8800,  370);
    public static readonly TimberGrade Gl24h  = Glulam("gl24h",    24,  19.2,   24,  11500,   9600,  385);
    public static readonly TimberGrade Gl26h  = Glulam("gl26h",    26,  20.8,   26,  12100,  10100,  405);
    public static readonly TimberGrade Gl28h  = Glulam("gl28h",    28,  22.3,   28,  12600,  10500,  425);
    public static readonly TimberGrade Gl30h  = Glulam("gl30h",    30,    24,   30,  13600,  11300,  430);
    public static readonly TimberGrade Gl32h  = Glulam("gl32h",    32,  25.6,   32,  14200,  11800,  440);

    // EN 14374 laminated veneer lumber — a declared-value product, not an EN 338 class, so its columns come off
    // the declaration rather than the strength-class table and its k90 is the LVL intercept.
    public static readonly TimberGrade Lvl48p = new("lvl48p", 48.0, 36.0, 40.0, 6.0, 4.6, Some(2.3), 13_800, 11_700, 430, 760, 510, 1.30);

    // The glulam mint: the six columns EN 14080 holds constant across all fourteen classes stated ONCE.
    static TimberGrade Glulam(string key, double fmk, double ft0k, double fc0k, double e0Mean, double e005, double rhoK) =>
        new(key, fmk, ft0k, fc0k, Fc90GlulamMpa, FvGlulamMpa, Some(FrGlulamMpa), e0Mean, e005, E90GlulamMpa, GGlulamMpa, rhoK, 1.35);

    const double Fc90GlulamMpa = 2.5;
    const double FvGlulamMpa = 3.5;
    const double FrGlulamMpa = 1.2;
    const double E90GlulamMpa = 300.0;
    const double GGlulamMpa = 650.0;

    public static readonly ImmutableArray<TimberGrade> Rows = [
        C14, C16, C18, C20, C22, C24, C27, C30,
        C35, C40, C45, C50, D18, D24, D27, D30,
        D35, D40, D45, D50, D55, D60, D65, D70,
        D75, D80, Gl20c, Gl22c, Gl24c, Gl26c, Gl28c, Gl30c,
        Gl32c, Gl20h, Gl22h, Gl24h, Gl26h, Gl28h, Gl30h, Gl32h,
        Lvl48p];
}

// One authored timber section: form and grade as TYPED refs (an unknown key is unrepresentable — no string
// re-parse, no TryGet rail), the gross breadth/depth, and the LAMINATION BUILD as an explicit per-ply thickness
// sequence (PUBLISHED product data on the form's OWN lamination axis: sawn one full-depth piece, glulam its EN 14080
// 45 mm lamellae across the DEPTH, edgewise P-grade LVL its 3 mm veneers across the WIDTH, CLT its exact manufacturer
// ply build summing to the overall depth — a mixed 30/34/…/30 build is one row the deleted uniform-thickness layup
// cannot represent). Cross-ply forms alternate 0°/90° outer-longitudinal.
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

    static readonly FrozenDictionary<ComponentId, TimberRow> Table =
        TimberRows.ToFrozenDictionary(static row => ComponentId.Create(row.Designation), static row => row);

    public static Fin<TimberRow> Resolve(Component component, Op key) =>
        Table.TryGetValue(component.Designation, out TimberRow row)
            ? Fin.Succ(row)
            : ComponentFault.Family(key, $"<timber-row-unregistered:{component.Designation.Value}>");

    // A member is the Rectangle gross; a cross-ply form is the Layered stack — each ply the bounded
    // PlyRole.Longitudinal/Transverse row (the gamma-method discriminant AND the IfcMaterialLayer orientation datum)
    // outer-longitudinal, with the grade-keyed wood MaterialId. Both routes go through the arm's railed Of (the
    // PositiveMagnitude lift + the laminate-build gate live there).
    static Fin<SectionProfile> ProfileOf(TimberRow row, Op key) =>
        row.Form.CrossPly
            ? row.BuildMm.Map((mm, index) => (Mm: mm, Role: (index & 1) == 0 ? PlyRole.Longitudinal : PlyRole.Transverse))
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
                ComponentFamily.Timber.Ifc,
                Coring.None, En,
                substanceId: row.Grade.Substance, appearanceId: row.Grade.Substance,
                detail: Option<PropertyBag>.None, key)
            .Map(item => new ComponentRow(item, Provenance.Published)));
    }

    // The Context parameter is the ComponentFamily.Rows delegate contract; the timber seed reads no context column.
    public static Fin<Seq<ComponentRow>> Rows(Context context) =>
        TimberRows.Traverse(RowOf).As();

    // The ComponentFamily.Timber CAPACITY producer: the typed Resolve restores the authored form/grade axes the
    // Component itself does not carry, and TimberDesign.Capacity's own (profile, section, cross-ply) correspondence
    // gate decides member-vs-panel — so a mis-flagged row faults at the door rather than pricing the wrong modality.
    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        from row in Resolve(component, key)
        from capacity in TimberDesign.Capacity(
            row.Form, row.Grade, component.Profile, section,
            placement.Service, placement.Duration, placement.EffectiveLengthMm, key)
        select SectionCapacity.Lift(new CapacityReceipt.Timber(component.Designation, capacity));
}
```

## [03]-[TIMBER_CAPACITY]

- Owner: `TimberDesign` the EN 1995-1-1 design-code operations owner — `Capacity` the ONE design-resistance projection (member-vs-panel discriminated by the ADMITTED form/profile correspondence), `EffectiveStiffness` the EN 1995-1-1 Annex B gamma-method kernel over role-tagged plies, `ResidualSection` the EN 1995-1-2 reduced cross-section, `ResidualStack` the EN 1995-1-2 CLT step-charring ply modality; `TimberCapacity` the receipt (FROZEN — the `capacity#SECTION_CAPACITY` `SectionCapacity.Lift(CapacityReceipt)` `CapacityReceipt.Timber` arm lifts it WHOLE onto `SectionCapacity.TimberMember`).
- Cases: one `TimberCapacity` across all forms — design bending `M_Rd,y` (member: `k_crit·k_h·k_mod·f_m,k·W_x/γ_M` over the RESOLVED `ComputedSection.SxMm3` with the §6.3.3 lateral-torsional `k_crit` band; panel: the gamma-method `W_eff = (EI)_eff/(E0·h/2)`), minor-axis bending `M_Rd,z` (member: `k_h(w)·k_mod·f_m,k·S_y/γ_M` over the resolved `SyMm3` — `k_h` over the WIDTH, no `k_crit`; panel: the NET-SECTION in-plane arm over the longitudinal plies alone, `k_mod·f_m,edge,k·t_net·h²/6/γ_M`, answering 0.0 where the form declares no edgewise strength so an in-plane `Mz` demand governs loud through the capacity `GuardedRatio` law), compression `N_Rd` (§6.3.2 `k_c`-reduced over `E005`; panel over the longitudinal net area and the effective radius `i_ef = √((EI)_eff/(E0·A_0))`), shear `V_Rd` (member `k_cr`-cracked longitudinal; panel rolling-shear `f_R,v,k`), perpendicular bearing `R_90,Rd` per unit bearing length, §6.1.8 torsion `T_Rd = k_shape·f_v,d·W_tor`, the governing `λ_rel`, the §6.1.6(2) per-form `k_m` weight, and the applied `k_mod` — a capacity is a derived projection over the resolved section or the ply stack, never a per-form check surface.
- Entry: `TimberDesign.Capacity(..., double effectiveLengthMm, Op key) : Fin<TimberCapacity>` admits a finite positive length before the member/panel dispatch. `TimberDesign.ResidualSection(..., double exposureMinutes, int exposedSides, Op key) : Fin<ResidualSection>` admits a rectangular member, non-negative finite exposure, and one through four exposed faces; complete burn-through faults. `TimberDesign.ResidualStack(..., Op key) : Fin<Seq<Ply>>` admits only a cross-ply form, then folds the CLT char front through the ply stack and faults on invalid exposure or full burn-through.
- Packages: Rasm.Numerics (project — `PositiveMagnitude`), Rasm.Domain (project — `Op`), Rasm.Materials.Component (project — `ComputedSection`/`SectionProfile`/`Ply`/`ComponentFault`), Thinktecture.Runtime.Extensions, LanguageExt.Core; the EN 1995 rules HAND-ROLLED (no .NET EC5 package exists — the SAME hand-roll the steel AISC and RC EC2 checks take), every factor a `TimberForm` column or a `TimberGrade` row read.
- Growth: a new design check is one `TimberCapacity` column plus its arm (a notched-beam `k_v`, a load-sharing `k_sys`); a new fire route one `ResidualSection` parameter (a protected member's `t_ch` delay); a new form's factor set is its `TimberForm` row columns — never a per-form capacity surface, never a re-minted characteristic where the grade row carries it.
- Boundary: every design resistance derives from `f_k·k_mod/γ_M`; `TimberForm` owns `k_h`, `k_cr`, `β_c`, `γ_M`, and `k_m`, and `LoadDuration.KmodFor(service)` owns the duration/service joint. Members read the resolved `ComputedSection`, §6.3.3 lateral-torsional stability, weak-axis buckling, shear, per-unit-length bearing, and torsion; panels read the longitudinal plies for Annex B `(EI)_eff`, gamma-reduced buckling, rolling shear, and out-of-plane bending, the in-plane minor axis the net-section arm over those same longitudinal plies. `ResidualSection` and `ResidualStack` preserve the same geometry currencies under EN 1995-1-2 charring, and the `SectionCapacity.Lift(CapacityReceipt)` `CapacityReceipt.Timber` arm carries the frozen receipt onto the unified demand rail.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// The EN 1995-1-1 design-resistance receipt — FROZEN: the capacity#SECTION_CAPACITY SectionCapacity.Lift(CapacityReceipt)
// CapacityReceipt.Timber arm lifts these nine columns WHOLE onto SectionCapacity.TimberMember. SI N·mm / N.
public readonly record struct TimberCapacity(
    double BendingNmm,           // M_Rd,y = k_crit·k_h·k_mod·f_m,k·W / γ_M (W = Sx member with the §6.3.3 k_crit band; W_eff panel)
    double BendingMinorNmm,      // M_Rd,z member: k_h(w)·k_mod·f_m,k·Sy / γ_M — no k_crit (no minor-axis LTB); panel the net-section in-plane arm
    double CompressionN,         // N_Rd = k_c·k_mod·f_c0,k·A / γ_M (§6.3.2 buckling-reduced)
    double ShearN,               // V_Rd = k_mod·f_v·k_cr·A_shear / γ_M (rolling-shear for a CLT panel)
    double BearingPerpNPerMm,    // R_90,Rd PER MM of bearing length = k_mod·f_c90,k·b / γ_M — the length is support DETAILING, so the section states resistance per unit length and capacity#SECTION_CAPACITY attaches MemberCheckRequirement.TimberBearingLength
    double TorsionalNmm,         // T_Rd = k_shape·f_v,d·W_tor (§6.1.8)
    double RelativeSlenderness,  // λ_rel,c = √(f_c0,k / σ_c,crit) — the §6.3.2 stability input
    double Km,                   // the §6.1.6(2) per-form stress-redistribution weight the biaxial fold swaps
    double Kmod);                // the joint service×duration factor the resistances scaled by

// The EN 1995-1-2 reduced cross-section after d_ef = β_n·t + k₀·d₀ per exposed FACE (β_n species-resolved; d₀ = 7 mm
// with the §4.2.2 k₀ = min(t/20, 1) ramp — zero exposure loses zero section); a fire Capacity re-runs over the
// residual rectangle at kmod = gammaM = 1.0; complete burn-through is a capacity fault.
public readonly record struct ResidualSection(PositiveMagnitude ResidualWidthMm, PositiveMagnitude ResidualDepthMm, double CharDepthMm);

// --- [OPERATIONS] --------------------------------------------------------------------------
// The EN 1995-1-1 design projection. HAND-ROLLED (no .NET EC5 package); every factor is a TimberForm column or a
// TimberGrade row read; the member section is the RESOLVED SectionSolver receipt, never a local solve.
public static class TimberDesign {
    // ONE entry, modality by input shape — and the form/profile CORRESPONDENCE is admitted, never caller discipline:
    // the joint pattern over (profile, section, form.CrossPly) makes (Rectangle, Some cs, non-cross-ply) the ONLY
    // route to Member and (Layered, None, cross-ply) the ONLY route to Panel; a cross-ply form on a Rectangle, a
    // sawn form on a Layered stack, or a non-timber arm faults ComponentFault.Family at the door.
    public static Fin<TimberCapacity> Capacity(
        TimberForm form, TimberGrade grade, SectionProfile profile, Option<ComputedSection> section,
        ServiceClass service, LoadDuration duration, double effectiveLengthMm, Op key) =>
        from length in guard(double.IsFinite(effectiveLengthMm) && effectiveLengthMm > 0.0,
            ComponentFault.Capacity(key, $"<timber-effective-length-rejected:{effectiveLengthMm:R}>"))
        from capacity in (profile, section.Case, form.CrossPly) switch {
            (SectionProfile.Rectangle r, ComputedSection cs, false) => Fin.Succ(Member(form, grade, r, cs, service, duration, effectiveLengthMm)),
            (SectionProfile.Rectangle, null, false)                 => Fin.Fail<TimberCapacity>(ComponentFault.Section(key, "<timber-member-section-unresolved>")),
            (SectionProfile.Layered, null, true) when service.KdefFor(form).IsNone   // the ONE permission law — the EN 16351 CLT-in-SC3 bar is the policy row's None, never a re-spelled == Sc3
                                                                    => Fin.Fail<TimberCapacity>(ComponentFault.Capacity(key, "<clt-not-permitted-service-class-3>")),
            (SectionProfile.Layered l, null, true)                  => Fin.Succ(Panel(form, grade, l, service, duration, effectiveLengthMm)),
            (SectionProfile.Layered, ComputedSection, true)         => Fin.Fail<TimberCapacity>(ComponentFault.Section(key, "<timber-panel-carries-no-section>")),
            _                                                       => Fin.Fail<TimberCapacity>(ComponentFault.Family(key, $"<timber-form-profile-mismatch:{form.Key}:{profile.Case}>")),
        }
        select capacity;

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
    // gross rectangle), the rolling-shear f_R,v,k screens shear over the longitudinal area. The in-plane (minor-axis)
    // bending resistance is the net-section in-plane arm, zero only where the form declares no edgewise strength, so an in-plane
    // Mz demand governs loud through the capacity GuardedRatio law rather than passing on a provisional net-section W.
    static TimberCapacity Panel(TimberForm f, TimberGrade g, SectionProfile.Layered l, ServiceClass sc, LoadDuration ld, double lengthMm) {
        double kmod = ld.KmodFor(sc), gammaM = f.GammaM;
        double b = l.WidthMm.Value, h = l.OverallMm.Value;
        double longitudinalThickness = l.Plies.Filter(static p => p.Role == PlyRole.Longitudinal).Sum(static p => p.ThicknessMm.Value);
        double areaLong = b * longitudinalThickness;
        double eiEff = EffectiveStiffness(l.Plies, g, b, lengthMm);
        double iEff = areaLong > 0.0 ? Math.Sqrt(eiEff / (g.E0Mean * areaLong)) : 0.0;
        (double kc, double lambdaRel) = BucklingKc(f, g, Slenderness(lengthMm, iEff));
        return Receipt(g, kmod, gammaM,
            bendingNmm: kmod * g.Fmk * (eiEff / (g.E0Mean * h * 0.5)) / gammaM,
            bendingMinorNmm: InPlaneBending(g, l, kmod, gammaM, longitudinalThickness),
            kc, areaLong,
            // Panel shear is ROLLING shear through the cross layers, so it reads the rolling-shear strength and
            // answers zero where the grade's product standard publishes none — a sawn class has no cross layer and
            // therefore no rolling-shear column, and borrowing the grain-parallel f_v,k there would price a failure
            // mode the section cannot exhibit. Zero governs loud through the capacity GuardedRatio.
            shearN: kmod * g.FRvk.IfNone(0.0) * areaLong / gammaM,
            b, h, lambdaRel, km: f.Km);
    }

    // IN-PLANE (minor-axis) bending of a cross-laminated panel — a plate loaded in its own plane, the lintel and deep-beam
    // case. The model is NET SECTION: only the layers whose fibres run parallel to the span carry the in-plane bending
    // stress, the cross layers contributing no net-section modulus at all, so the resistance rides W_net = t_net·h²/6
    // over the summed longitudinal thickness rather than the gross panel. That is a different section from the
    // out-of-plane arm's composite EI — a cross layer is a shear coupler out of plane and a hole in plane — which is
    // why this cannot be the major-axis expression with a swapped modulus.
    // The STRENGTH is a declared column and it is typed-absent by default: the edgewise bending strength a CLT layup
    // develops in plane is not the lamella's own flat f_m,k, and the value is contested across the in-plane
    // literature, so a panel whose form publishes no edgewise strength answers ZERO here and an in-plane Mz demand
    // governs loud through GuardedRatio rather than passing against a borrowed number. A form that does publish one
    // prices in plane immediately, with no other edit.
    static double InPlaneBending(TimberGrade g, SectionProfile.Layered l, double kmod, double gammaM, double netThicknessMm) =>
        g.FmEdgeK.Match(
            Some: edge => kmod * edge * netThicknessMm * l.OverallMm.Value * l.OverallMm.Value / 6.0 / gammaM,
            None: static () => 0.0);

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
            BearingPerpNPerMm: kmod * g.Fc90k * wMm / gammaM,   // width alone: multiplying by the DEPTH fabricated a bearing area no support detail declared
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

    // EN 1995-1-1 Annex B gamma method over role-tagged, possibly NON-UNIFORM plies (N·mm²): only the Longitudinal
    // plies carry bending; each off-centre longitudinal ply's Steiner term is reduced by γ_i = 1/(1 + π²·E0·t_i·t̄_i/
    // (ℓ²·G_R)) with t̄_i the ADJACENT transverse ply thickness toward the panel middle (Eq B.5 slip K = G_R·b/t̄
    // — the width cancels; no z² rides the denominator). Span-zero → γ→0 (no composite); span-∞ → γ→1 (rigid);
    // a NON-POSITIVE span reads the SAME γ→0 no-composite lower bound — the prior ∞ mapping silently credited the
    // rigid-glued UPPER bound to a degenerate input. Centroids come from cumulative offsets, so a mixed
    // 30/34/…/30 build integrates exactly.
    public static double EffectiveStiffness(Seq<Ply> plies, TimberGrade grade, double widthMm, double referenceSpanMm) {
        double depth = plies.Sum(static p => p.ThicknessMm.Value), half = depth * 0.5, e0 = grade.E0Mean;
        double span2 = referenceSpanMm > 0.0 ? referenceSpanMm * referenceSpanMm : 0.0;
        Seq<(double T, double Z, PlyRole Role, int Index)> placed = plies.Fold(
            (Offset: 0.0, Rows: Seq<(double T, double Z, PlyRole Role, int Index)>()),
            (acc, p) => (acc.Offset + p.ThicknessMm.Value,
                acc.Rows.Add((p.ThicknessMm.Value, acc.Offset + p.ThicknessMm.Value * 0.5 - half, p.Role, acc.Rows.Count)))).Rows;
        return placed.Filter(static row => row.Role == PlyRole.Longitudinal).Sum(row => {
            double own = e0 * widthMm * row.T * row.T * row.T / 12.0;
            int crossAt = row.Z <= 0.0 ? row.Index + 1 : row.Index - 1;   // the adjacent transverse ply TOWARD the panel middle
            double tCross = placed
                .Filter(other => other.Index == crossAt && other.Role == PlyRole.Transverse)
                .Map(static other => other.T).Head.IfNone(0.0);
            double gamma = tCross > 0.0
                ? 1.0 / (1.0 + Math.PI * Math.PI * e0 * row.T * tCross / (span2 * grade.GRollMean))
                : 1.0;
            return own + gamma * e0 * widthMm * row.T * row.Z * row.Z;
        });
    }

    // EN 1995-1-2 reduced cross-section: d_ef = β_n·t + k₀·7 mm removed per exposed FACE, β_n = form.BetaN(grade)
    // (species-resolved), k₀ the §4.2.2 min(t/20, 1) zero-strength-layer ramp. Faces accrue bottom -> both sides ->
    // top: 1 chars the depth once, 2 adds one width face, 3 the classic beam (both sides + bottom), 4 the fully
    // exposed column. Invalid exposure and complete burn-through rail instead of manufacturing a residual section.
    public static Fin<ResidualSection> ResidualSection(TimberForm form, TimberGrade grade, SectionProfile profile, double exposureMinutes, int exposedSides, Op key) {
        if (!double.IsFinite(exposureMinutes) || exposureMinutes < 0.0 || exposedSides is < 1 or > 4 || profile is not SectionProfile.Rectangle) {
            return ComponentFault.Capacity(key, $"<timber-fire-input-rejected:{exposureMinutes:R}:{exposedSides}:{profile.Case}>");
        }
        double t = exposureMinutes;
        double charMm = form.BetaN(grade) * t + 7.0 * Math.Min(t / 20.0, 1.0);
        (double w, double d) = (profile.GrossRectangleMm.WidthMm.Value, profile.GrossRectangleMm.DepthMm.Value);
        double residW = w - Math.Min(2, exposedSides - 1) * charMm;
        double residD = d - (1 + (exposedSides == 4 ? 1 : 0)) * charMm;
        return residW > 0.0 && residD > 0.0
            ? Fin.Succ(new ResidualSection(PositiveMagnitude.Create(residW), PositiveMagnitude.Create(residD), charMm))
            : ComponentFault.Capacity(key, $"<timber-member-burn-through:{exposureMinutes:R}min>");
    }

    // EN 1995-1-2 CLT STEP charring — the Layered fire modality beside the rectangle route: the char front marches
    // from the EXPOSED first ply at β_n within the first lamella; each bond-line FALL-OFF (the charred lamella
    // delaminates and its insulating char layer is lost) doubles the rate to 2·β_n for the next lamella's first
    // 25 mm, then β_n resumes; the §4.2.2 k₀·7 mm zero-strength layer rides the final front. The residual stack
    // re-enters EffectiveStiffness/the panel Capacity at k_mod = γ_M = 1.0 — the surviving lamella set is stated
    // ply by ply, never smeared over a gross rectangle. Full burn-through rails loud.
    public static Fin<Seq<Ply>> ResidualStack(TimberForm form, TimberGrade grade, Seq<Ply> plies, double exposureMinutes, Op key) {
        if (!double.IsFinite(exposureMinutes) || exposureMinutes < 0.0 || plies.IsEmpty) {
            return ComponentFault.Capacity(key, $"<clt-fire-input-rejected:{exposureMinutes:R}>");
        }
        // The form/profile correspondence gate: STEP charring with bond-line fall-off is the cross-ply lamella
        // modality — a non-cross-ply form routes ResidualSection, never a smeared stack march.
        if (!form.CrossPly) {
            return ComponentFault.Capacity(key, $"<clt-fire-form-not-cross-ply:{form.Key}>");
        }
        double beta = form.BetaN(grade);
        (double Beta, double Minutes, double Depth, bool FallOff, bool Done) front = plies.Fold(
            (Beta: beta, Minutes: exposureMinutes, Depth: 0.0, FallOff: false, Done: false),
            static (acc, ply) => {
                if (acc.Done) { return acc; }
                double thickness = ply.ThicknessMm.Value;
                double fast = acc.FallOff ? Math.Min(25.0, thickness) : 0.0;
                double burn = fast / (2.0 * acc.Beta) + (thickness - fast) / acc.Beta;
                return acc.Minutes < burn
                    ? (acc.Beta, 0.0, acc.Depth + (acc.Minutes <= fast / (2.0 * acc.Beta) ? acc.Minutes * 2.0 * acc.Beta : fast + (acc.Minutes - fast / (2.0 * acc.Beta)) * acc.Beta), acc.FallOff, true)
                    : (acc.Beta, acc.Minutes - burn, acc.Depth + thickness, true, false);
            });
        Seq<Ply> residual = Trim(plies, front.Depth + 7.0 * Math.Min(exposureMinutes / 20.0, 1.0));
        return residual.IsEmpty
            ? Fin.Fail<Seq<Ply>>(ComponentFault.Capacity(key, $"<clt-burn-through:{exposureMinutes:R}min>"))
            : Fin.Succ(residual);
    }

    // Consumes the effective char depth from the exposed end of the stack: fully charred plies drop, the partial
    // ply survives trimmed (its role and material intact), the remainder passes through untouched.
    static Seq<Ply> Trim(Seq<Ply> plies, double depthMm) =>
        plies.Fold((Cut: depthMm, Remainder: Seq<Ply>()), static (acc, p) =>
            acc.Cut >= p.ThicknessMm.Value ? (acc.Cut - p.ThicknessMm.Value, acc.Remainder)
            : acc.Cut > 0.0 ? (0.0, acc.Remainder.Add(new Ply(p.Material, PositiveMagnitude.Create(p.ThicknessMm.Value - acc.Cut), p.Role)))
            : (0.0, acc.Remainder.Add(p))).Remainder;
}
```

## [04]-[RESEARCH]

(none)
