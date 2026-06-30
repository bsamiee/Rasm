# [MATERIALS_TIMBER]

THE TIMBER PROFILEFAMILY GROUNDED IN THE EN STRENGTH-CLASS TABLES, ORTHOTROPIC ALONG/ACROSS GRAIN, AND CLT-AS-LAYER-SET. The timber cross-section vocabulary — the sawn/glulam/CLT/LVL/PSL product-form discriminant, the EN 14080 (glulam) / EN 338 (sawn) / EN 14374 (LVL) / APA PRG 320 (CLT) strength-class with its FULL characteristic strength-AND-stiffness vector (bending `f_m,k`, tension-AND-compression parallel `f_t0k`/`f_c0k`, compression perpendicular `f_c90k`, shear `f_v,k`, the mean-AND-5%-fractile axial moduli `E0,mean`/`E0,05`, the perpendicular `E90,mean`, the shear-AND-rolling-shear moduli `G_mean`/`G_R,mean`, the characteristic density `ρ_k`), the cross-ply `PlyLayup` (per-ply thickness + 0°/90° orientation), the `ServiceClass`/`LoadDuration` EC5 `k_mod` axis, and the EN 1995-1-2 charring rate — is a realized cross-section vocabulary one `profile#PROFILE_OWNER` `Profile` carries in the `ProfileFamily.Timber` case. A glulam beam is a `Profile` row, never a `GlulamBeam` type; a CLT panel is a `Profile` whose composition is the seam `Construction/assembly#MATERIAL_COMPOSITION` `LayerSet` (the alternating longitudinal/transverse plies), never a `CltPanel` type: the section dimensions, the layup, the EN characteristic vector, the EC5 design capacity, and the product form are timber-`Profile` columns, and the `TimberSection` projection feeds the same `Construction/layout#ASSEMBLY_FOLD` `Resolve` fold the steel and masonry families drive — a sawn/glulam member extrudes through one `Profile` over the `RunPath`, a CLT panel resolves to a layer set, never a per-family layout. The timber vocabulary grows by data — a new section is one `TimberRow` catalogue row, a new product form one `TimberForm` case, a new strength class one `TimberGrade` row — never a per-member type, never a hand-keyed strength literal. The page composes `profile#PROFILE_OWNER` for the `Profile`/`ProfileUnit`/`ProfileStandard`/`ComputedSection` shape and the shared `ParametricSection.Rectangle` solver, `VividOrange.Materials` `LinearElasticOrthotropicMaterial` for the along/across-grain directional-stiffness law the seam `MaterialPropertySet.Mechanical` orthotropic consumer reads, `Construction/assembly#MATERIAL_COMPOSITION` `CompositionAuthor.LayerSet` for the CLT cross-ply layer set, and the `Rasm` kernel `PositiveMagnitude` for every length column; cmu/steel/glazing land their own sibling vocabularies on their own pages.

## [01]-[INDEX]

- [01]-[TIMBER_FAMILY]: the `TimberForm` sawn/glulam/CLT/LVL/PSL discriminant (carrying the IFC material-shape kind + the cross-ply flag), the `TimberGrade` EN 14080/338/14374 characteristic strength-AND-stiffness row, the `ServiceClass`/`LoadDuration` EC5 `k_mod` axis, the `PlyLayup` cross-ply representation, the `TimberSection` section record, the `TimberSection.Section`/`OrthotropicLaw`/`ToLayerSet`/`ToUnit` projections, and the `ProfileCatalogue.BuildTimberRows` row table.
- [02]-[TIMBER_CAPACITY]: the EN 1995-1-1 `TimberCapacity` LRFD-style design receipt (bending with `k_h` size effect / `k_mod` / `γ_M`, compression-with-buckling `k_c`, shear with `k_cr` / CLT rolling-shear, perpendicular-to-grain bearing) and the EN 1995-1-2 reduced-cross-section charring residual, both derived projections over the `TimberSection` columns that lift into the `Profiles/capacity#SECTION_CAPACITY` unified utilisation rail.

## [02]-[TIMBER_FAMILY]

- Owner: the timber unit vocabulary (`TimberForm` the product-form discriminant carrying the IFC material-shape kind and cross-ply flag, `TimberGrade` the EN characteristic strength-AND-stiffness row, `ServiceClass`/`LoadDuration` the EC5 `k_mod` axis, `PlyLayup` the cross-ply per-ply representation, `TimberSection` the section record); `ProfileCatalogue.BuildTimberRows` the registered-row seed `profile#PROFILE_OWNER` composes; the `TimberSection.ToUnit` projection bridging a section to the canonical `ProfileUnit`, the `TimberSection.ToLayerSet` CLT layer-set bridge, the `TimberSection.OrthotropicLaw` directional-stiffness law.
- Cases: form {sawn (EN 338 solid structural lumber), glulam (EN 14080 glue-laminated lamellae), clt (APA PRG 320 / EN 16351 cross-laminated plies), lvl (EN 14374 laminated veneer lumber), psl (parallel-strand lumber)} — the timber product-form set, each carrying its `MaterialShapeKind` (a profiled member vs a layered panel) and its cross-ply flag; grade {gl24h, gl28h, gl30h, gl32h homogeneous + gl24c/gl28c combined glulam, c16, c24, c30, d40 sawn, lvl48p LVL} — the EN strength classes carrying the full characteristic vector; service {sc1 dry-internal, sc2 sheltered, sc3 exposed} and duration {permanent, longterm, mediumterm, shortterm, instantaneous} — the EC5 `k_mod` modification-factor axis; a section is a `TimberSection` row over one `TimberForm`/`TimberGrade`/`PlyLayup`, never a section subtype.
- Entry: `public Fin<ComputedSection> Section(Op key)` on `TimberSection` — the elastic section properties through the shared `profile#PROFILE_OWNER` `ParametricSection.Rectangle` solver (strong-AND-weak-axis modulus/inertia/radius), the design seam's gross section the same owner steel/cmu compute through, for a CLT panel the EFFECTIVE composite section the gamma-method `EffectiveStiffness` refines OVER this gross base from the `PlyLayup`; `public LinearElasticOrthotropicMaterial OrthotropicLaw()` the `[BoundaryAdapter]` along/across-grain directional-stiffness law (`E0,mean` parallel = X, `E90,mean` perpendicular = Y/Z) the seam `MaterialPropertySet.Mechanical` orthotropic case reads, the timber INDEPENDENT-`G` material the seam's isotropic `G = E/(2(1+ν))` derivation cannot model; `public Fin<MaterialComposition> ToLayerSet(Op key)` the CLT cross-ply seam `LayerSet` bridge (the alternating longitudinal/transverse plies a `Construction/assembly#MATERIAL_COMPOSITION` `CompositionAuthor.LayerSet`); `public Fin<ProfileUnit> ToUnit(Context context, Op key)` the section→`ProfileUnit` projection so a sawn/glulam member flows through the SAME `Construction/layout#ASSEMBLY_FOLD` `Resolve` fold; `public int LamellaCount()` the ply/lamella count the layup seam reads; the `[03]-[TIMBER_CAPACITY]` `TimberDesign.Capacity(this, service, duration, effectiveLengthMm, key)` the EC5 design-resistance receipt the `Profiles/capacity#SECTION_CAPACITY` rail lifts.
- Packages: Rasm (project — `PositiveMagnitude` for every fractional-millimeter section column), VividOrange.Materials (`LinearElasticOrthotropicMaterial(MaterialType, Pressure Ex, Pressure Sx, Pressure Ey, Pressure Sy, Pressure Ez, Pressure Sz)` the orthotropic directional-stiffness law the seam orthotropic mechanical case reads; `.api/api-vividorange-materials.md`), VividOrange.Sections.SectionProperties (via the shared `profile#PROFILE_OWNER` `ParametricSection.Rectangle` bridge; `.api/api-vividorange-sections-sectionproperties.md`), UnitsNet (`Pressure` for the orthotropic-law moduli/strengths coerced at the edge; `.api/api-unitsnet.md`), Rasm.Element (project — `MaterialComposition`/`MaterialLayer`/`MaterialId` via the `Construction/assembly#MATERIAL_COMPOSITION` author for the CLT layer set), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`).
- Growth: the timber vocabulary grows by data — a new section is one `TimberRow` catalogue row keyed by its product designation, a new product form one `TimberForm` case carrying its `MaterialShapeKind` + cross-ply flag, a new strength class one `TimberGrade` row carrying its EN characteristic vector, a new service/duration combination one `ServiceClass`/`LoadDuration` row — never a per-member type, never a per-family `Profile` variant, never a hand-keyed strength literal. A cmu/glazing family lands its own vocabulary on its own page the way timber carries `TimberForm`/`TimberSection`.
- Boundary: the timber vocabulary is a realized `ProfileFamily` — a per-member class AND a hand-keyed strength-property literal are the deleted forms; `TimberSection` composes the `Rasm` kernel `PositiveMagnitude` (double-backed `> 0` finite) for every length column so the section never re-mints a length primitive, the EN/APA standard lamella thickness (38–45 mm glulam, 20–40 mm CLT ply) and the sawn nominal-to-actual (38×89 mm) admitting as fractional millimeters; `TimberGrade` carries the FULL EN 14080/338/14374 characteristic strength-AND-stiffness vector as DATA (the `Fmk` bending, the `Ft0k`/`Fc0k` axial, the `Fc90k` perpendicular-bearing, the `Fvk` shear, the `E0Mean`/`E005` axial moduli, the `E90Mean` perpendicular, the `GMean`/`GRollMean` shear-and-rolling-shear, the `DensityK`), never raw doubles and never a single `E0Mean` slice — the EC5 `Capacity` and the orthotropic law read the registered class DATA the way the steel family reads its `SteelGrade`/`EnSteelFactory` yield, the timber-grade DATA grounded in the EN tables because `VividOrange.Materials` ships NO timber factory (its EN factories are concrete/steel/rebar only, `.api/api-vividorange-materials.md` `[FLOOR_SCOPE_GATE]`), so the `TimberGrade` `[SmartEnum]`-as-DATA is the registered-class owner the way `SteelGrade`'s AISC `None` bands carry their spec-nominal yield; `TimberSection.OrthotropicLaw` is the ONE bridge to the structural directional-stiffness law — a `VividOrange.Materials` `LinearElasticOrthotropicMaterial` carrying `E0,mean` parallel-to-grain (X) and `E90,mean` perpendicular (Y/Z) plus the per-axis strengths, the INDEPENDENT-`G` orthotropic material the seam `MaterialPropertySet.Mechanical` names timber as the consumer of (the seam's isotropic `G = E/(2(1+ν))` cannot model a material whose `G_mean ≈ E0/16`), so a timber member's directional stiffness is the registered class DATA the analysis seam reads, never an isotropic approximation; `TimberSection.Section` is the gross elastic section through the SAME `ParametricSection.Rectangle` Green's-theorem solver steel/cmu run, the per-family `W·D²/6`/`W·D³/12` literal the deleted form, and a CLT/glulam panel's cross-ply EFFECTIVE composite stiffness is the gamma-method `EffectiveStiffness` refinement OVER that gross base read from the `PlyLayup` (the alternating longitudinal/transverse plies and the rolling-shear modulus driving the shear-flexible composite `(EI)_eff`), never a parallel section owner; `TimberSection.ToLayerSet` is the ONE bridge from a CLT/LVL layered panel to the seam `Construction/assembly#MATERIAL_COMPOSITION` `LayerSet` (the alternating-orientation plies, each `MaterialLayer` carrying its `Dimension` thickness and a `MaterialId`, so a CLT panel round-trips to IFC 4.3 as an `IfcMaterialLayerSet` the SAME way glazing's IGU does — the cross-laminated panel is genuinely a layered material, never a single rectangle profile), while a sawn/glulam/LVL/PSL MEMBER stays a `ProfileSet` extrusion (`assembly#MATERIAL_COMPOSITION` `ProfileSet`) along the `RunPath` whose profile is the `IfcRectangleProfileDef` rectangle; `TimberForm.MaterialShapeKind` is the discriminant the `ToLayerSet`-vs-`ProfileSet` selection reads (a panel layers, a member profiles), so a timber element's composition matches its product geometry rather than forcing every form through one rectangle; `TimberSection.ToUnit` is the ONE bridge from the section vocabulary to the canonical `ProfileUnit` the `Resolve` fold consumes for the profiled members — a sawn/glulam run is the single-orientation station-stepped fold so a beam/column is a `ProfileSet` extrusion along the `RunPath`, never a masonry-style multi-unit course; `ProfileCatalogue.BuildTimberRows` seeds the `profile#PROFILE_OWNER` `ProfileCatalogue.Rows` table with the rows keyed `timber.<designation>`, the realized cross-section grounded in the published EN 14080 (glulam) / EN 338 (sawn) / EN 14374 (LVL) / APA PRG 320 (CLT) section-and-strength tables.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;                                   // PositiveMagnitude, Op, Context (the kernel value-object + op-key + admission context)
using Rasm.Element;                                  // MaterialId, MaterialComposition (the CLT LayerSet target), MaterialPropertySet (the seam Orthotropic structural case)
using Rasm.Materials.Construction;                   // CompositionAuthor (the seam-LayerSet author the CLT bridge delegates to)
using Rasm.Materials.Profiles;                       // Profile/ProfileUnit/ProfileStandard/ProfileId/ProfileFault/ProfileFamily/ComputedSection/ParametricSection (the parent PROFILE_OWNER)
using Rasm.Materials.Profiles.Masonry;               // Coring (the masonry#PROFILE_FAMILY void-class this timber family passes Coring.None)
using Thinktecture;
using VividOrange.Materials;                         // LinearElasticOrthotropicMaterial (the along/across-grain directional-stiffness law)
using UnitsNet;                                      // Pressure (the orthotropic-law moduli/strengths, coerced at the edge)
using static LanguageExt.Prelude;

// Each family page is its OWN Rasm.Materials.Profiles.<Family> sub-namespace so the six sibling `ProfileCatalogue` static
// classes are distinct types (a shared Rasm.Materials.Profiles would be a CS0101 collision with profile.md's own
// `ProfileCatalogue`); profile#PROFILE_OWNER stays the parent Rasm.Materials.Profiles and folds
// Timber.ProfileCatalogue.BuildTimberRows / Timber.ProfileCatalogue.TimberSections by the sub-namespace-qualified name.
namespace Rasm.Materials.Profiles.Timber;

// --- [TYPES] -------------------------------------------------------------------------------
// The IFC material-shape kind a timber product crosses the wire as: a profiled MEMBER is an IfcMaterialProfileSet
// (the rectangle profile extruded along the run), a layered PANEL is an IfcMaterialLayerSet (the cross-ply stack).
// CLT is genuinely a layered panel, never a single rectangle profile — the cross-lamination IS the material buildup.
[SmartEnum<string>]
public sealed partial class MaterialShapeKind {
    public static readonly MaterialShapeKind ProfiledMember = new("profiled-member");   // IfcMaterialProfileSet
    public static readonly MaterialShapeKind LayeredPanel   = new("layered-panel");     // IfcMaterialLayerSet
}

// The timber product form: each carries its IfcProfileDef rectangle subtype (the profiled members and the CLT
// ply rectangle), its IFC material-SHAPE kind (member profiles vs panel layers), the cross-ply flag, and the
// EN 1995-1-2 Table 3.1 SOFTWOOD notional charring rate β_n (the per-side reduced-cross-section input — glulam/LVL/PSL
// 0.70 vs solid sawn / CLT panel 0.80, the form base BetaN(ρ_k) then reduces for a dense hardwood). A new engineered
// product (curved/tapered glulam, box/I-beam) is one TimberForm case, never a per-member type.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TimberForm {
    public static readonly TimberForm Sawn   = new("sawn",   ifcSubtype: "IfcRectangleProfileDef", shape: MaterialShapeKind.ProfiledMember, crossPly: false, betaNSoftwoodMmPerMin: 0.80);
    public static readonly TimberForm Glulam = new("glulam", ifcSubtype: "IfcRectangleProfileDef", shape: MaterialShapeKind.ProfiledMember, crossPly: false, betaNSoftwoodMmPerMin: 0.70);
    public static readonly TimberForm Clt    = new("clt",    ifcSubtype: "IfcRectangleProfileDef", shape: MaterialShapeKind.LayeredPanel,   crossPly: true,  betaNSoftwoodMmPerMin: 0.80);
    public static readonly TimberForm Lvl    = new("lvl",    ifcSubtype: "IfcRectangleProfileDef", shape: MaterialShapeKind.ProfiledMember, crossPly: false, betaNSoftwoodMmPerMin: 0.70);
    public static readonly TimberForm Psl    = new("psl",    ifcSubtype: "IfcRectangleProfileDef", shape: MaterialShapeKind.ProfiledMember, crossPly: false, betaNSoftwoodMmPerMin: 0.70);
    public string IfcSubtype { get; }
    public MaterialShapeKind Shape { get; }
    public bool CrossPly { get; }
    // The EN 1995-1-2 Table 3.1 SOFTWOOD notional design charring rate β_n (mm/min, the rate INCLUDING corner rounding
    // the reduced-cross-section method applies per exposed side): solid softwood sawn 0.80, glulam ρ_k ≥ 290 / LVL ρ_k ≥
    // 480 / PSL 0.70, CLT panel 0.80. This is the SOFTWOOD base — a hardwood grade (EN 338 D-class) charrs SLOWER, so the
    // density-adjusted rate is BetaN(densityK) reading the grade ρ_k (Beta0CharMmPerMin was the prior flat 0.65/0.70 fill
    // that conflated β₀ one-dimensional with β_n notional and gave a D40 hardwood the same rate as a softwood).
    public double BetaNSoftwoodMmPerMin { get; }
    public bool IsPanel => Shape == MaterialShapeKind.LayeredPanel;

    // The EN 1995-1-2 Table 3.1 / §3.4.2 density-adjusted notional charring rate: a dense hardwood (ρ_k ≥ 450 kg/m³,
    // an EN 338 D-class) charrs at 0.55 mm/min, linearly interpolated up to the softwood β_n at ρ_k = 290; a softwood
    // (ρ_k < 450) keeps its form β_n. So a D40 (ρ_k 700) gets 0.55, a C24 softwood (ρ_k 350) the form's 0.80, never one
    // flat rate — the charring vocabulary is the (form, density) function EN 1995-1-2 tabulates, not a column constant.
    public double BetaN(double densityK) =>
        densityK >= 450.0
            ? 0.55
            : densityK <= 290.0
                ? BetaNSoftwoodMmPerMin
                : BetaNSoftwoodMmPerMin - (BetaNSoftwoodMmPerMin - 0.55) * (densityK - 290.0) / (450.0 - 290.0);
}

// The EN 14080 (glulam) / EN 338 (structural sawn) / EN 14374 (LVL) strength class as registered DATA — the FULL
// characteristic strength-AND-stiffness vector the EC5 design check and the orthotropic stiffness law read, never a
// single E0Mean slice and never a hand-keyed literal. VividOrange.Materials ships NO timber factory (EN factories are
// concrete/steel/rebar only, api-vividorange-materials.md [FLOOR_SCOPE_GATE]), so this SmartEnum IS the registered
// timber-class owner the way SteelGrade's AISC None bands carry their spec yield. fMk bending, ft0k tension-∥, fc0k
// compression-∥, fc90k compression-⊥ (the perpendicular-to-grain bearing strength a CLT support reaction reads), fvk
// shear, fRvk rolling shear (the CLT transverse-ply shear governing the gamma-method), E0Mean/E005 axial moduli (E005
// the 5% fractile for stability), E90Mean perpendicular modulus (the orthotropic Y/Z), gMean/gRollMean shear-and-
// rolling-shear moduli, densityK the characteristic density (the self-weight + connection embedment-strength input).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TimberGrade {
    //                                              fMk    ft0k  fc0k  fc90k fvk   fRvk  E0Mean  E005    E90Mean gMean gRollMean rhoK
    public static readonly TimberGrade Gl24h = new("gl24h", 24.0, 19.2, 24.0, 2.5, 3.5, 1.2,  11_500, 9_600,  300,  650,  65,  385);
    public static readonly TimberGrade Gl28h = new("gl28h", 28.0, 22.3, 28.0, 2.5, 3.5, 1.2,  12_600, 10_500, 300,  650,  65,  425);
    public static readonly TimberGrade Gl30h = new("gl30h", 30.0, 24.0, 30.0, 2.5, 3.5, 1.2,  13_600, 11_300, 300,  650,  65,  430);
    public static readonly TimberGrade Gl32h = new("gl32h", 32.0, 25.6, 32.0, 2.5, 3.5, 1.2,  14_200, 11_800, 300,  650,  65,  440);
    public static readonly TimberGrade Gl24c = new("gl24c", 24.0, 17.0, 21.5, 2.5, 3.5, 1.2,  11_000, 9_100,  250,  650,  65,  365);
    public static readonly TimberGrade Gl28c = new("gl28c", 28.0, 19.5, 24.0, 2.5, 3.5, 1.2,  12_500, 10_400, 250,  650,  65,  390);
    public static readonly TimberGrade C16   = new("c16",   16.0,  8.5, 17.0, 2.2, 3.2, 1.0,   8_000, 5_400,  270,  500,  50,  310);
    public static readonly TimberGrade C24   = new("c24",   24.0, 14.0, 21.0, 2.5, 4.0, 1.2,  11_000, 7_400,  370,  690,  69,  350);
    public static readonly TimberGrade C30   = new("c30",   30.0, 18.0, 23.0, 2.7, 4.0, 1.2,  12_000, 8_000,  400,  750,  75,  380);
    public static readonly TimberGrade D40   = new("d40",   40.0, 24.0, 26.0, 2.9, 4.0, 1.2,  11_000, 9_400,  750,  690,  69,  700);
    public static readonly TimberGrade Lvl48p = new("lvl48p", 48.0, 36.0, 40.0, 6.0, 4.6, 2.3, 13_800, 11_700, 430, 760, 76, 510);
    public double Fmk { get; } public double Ft0k { get; } public double Fc0k { get; } public double Fc90k { get; }
    public double Fvk { get; } public double FRvk { get; }
    public double E0Mean { get; } public double E005 { get; } public double E90Mean { get; }
    public double GMean { get; } public double GRollMean { get; }
    public double DensityK { get; }
}

// The EN 1995-1-1 service class — the in-service moisture environment driving k_mod AND k_def (creep). SC1 heated
// internal (≤12% moisture), SC2 sheltered/unheated (≤20%), SC3 fully exposed (>20%). A timber member's design
// strength is its characteristic strength × k_mod, and k_mod is the joint of service class AND load duration — so the
// service class is a first-class design input, never an implicit dry assumption. KdefCreep is the long-term
// deformation factor a serviceability check reads. Solid/glulam/CLT share these k_mod rows (EN 1995-1-1 Table 3.1).
[SmartEnum<string>]
public sealed partial class ServiceClass {
    public static readonly ServiceClass Sc1 = new("sc1", kdefCreep: 0.60);
    public static readonly ServiceClass Sc2 = new("sc2", kdefCreep: 0.80);
    public static readonly ServiceClass Sc3 = new("sc3", kdefCreep: 2.00);
    public double KdefCreep { get; }
}

// The EN 1995-1-1 Table 3.1 load-duration class — the k_mod modification factor for solid timber/glulam/LVL by the
// duration of the governing action, jointly with the ServiceClass. A 3-state IsCompact-style flag cannot model the
// duration axis: a permanent dead-load check and a short-term wind check use different k_mod, so each duration carries
// its per-service-class k_mod triple (sc1, sc2, sc3). KmodFor reads the column the ServiceClass selects.
[SmartEnum<string>]
public sealed partial class LoadDuration {
    public static readonly LoadDuration Permanent     = new("permanent",     kmodSc1: 0.60, kmodSc2: 0.60, kmodSc3: 0.50);
    public static readonly LoadDuration LongTerm      = new("long-term",     kmodSc1: 0.70, kmodSc2: 0.70, kmodSc3: 0.55);
    public static readonly LoadDuration MediumTerm    = new("medium-term",   kmodSc1: 0.80, kmodSc2: 0.80, kmodSc3: 0.65);
    public static readonly LoadDuration ShortTerm     = new("short-term",    kmodSc1: 0.90, kmodSc2: 0.90, kmodSc3: 0.70);
    public static readonly LoadDuration Instantaneous = new("instantaneous", kmodSc1: 1.10, kmodSc2: 1.10, kmodSc3: 0.90);
    public double KmodSc1 { get; } public double KmodSc2 { get; } public double KmodSc3 { get; }
    public double KmodFor(ServiceClass service) =>
        service == ServiceClass.Sc1 ? KmodSc1 : service == ServiceClass.Sc2 ? KmodSc2 : KmodSc3;
}

// --- [MODELS] ------------------------------------------------------------------------------
// The cross-ply layup of a CLT/LVL panel: the per-ply thickness and orientation sequence (0° longitudinal,
// 90° transverse), the entire point of cross-lamination the prior model asserted but never carried. The gamma-method
// composite stiffness reads which plies are longitudinal (carrying axial/bending) vs transverse (carrying rolling
// shear), and the ToLayerSet bridge tags each ply's IfcMaterialLayer with its orientation. A homogeneous glulam/sawn
// member is the single 0° ply; a 3/5/7-ply CLT is the alternating sequence. PlyAngles holds one entry per ply.
public readonly record struct PlyLayup(PositiveMagnitude PlyThicknessMm, Seq<int> PlyAngles) {
    public int PlyCount => Math.Max(1, PlyAngles.Count);
    public bool IsCrossLaminated => PlyAngles.Exists(static a => a == 90);
    // The longitudinal plies (0°) carry the in-plane axial/bending action; the transverse (90°) carry rolling shear.
    public Seq<int> LongitudinalIndices => toSeq(Enumerable.Range(0, PlyCount)).Filter(i => PlyAngles[i] == 0);
    public double LongitudinalThicknessMm => LongitudinalIndices.Count * PlyThicknessMm.Value;
    public static PlyLayup Homogeneous(PositiveMagnitude depthMm) => new(depthMm, Seq1(0));
    // A balanced symmetric CLT layup: outer plies longitudinal, alternating 0/90 toward the core (3/5/7-ply).
    public static PlyLayup CrossLaminated(PositiveMagnitude plyMm, int plies) =>
        new(plyMm, toSeq(Enumerable.Range(0, Math.Max(1, plies))).Map(static i => (i & 1) == 0 ? 0 : 90));
}

public readonly record struct TimberSection(
    TimberForm Form,
    TimberGrade Grade,
    PositiveMagnitude WidthMm,
    PositiveMagnitude DepthMm,
    PlyLayup Layup) {

    public int LamellaCount() => Layup.PlyCount;

    // The section properties through the ONE profile#PROFILE_OWNER ParametricSection solver over the rectangle, never
    // a hand-rolled W·D²/6 / W·D³/12 literal: it carries strong-AND-weak-axis modulus/inertia/radius the literal lacked
    // and the SAME owner steel/cmu compute through. A CLT panel's cross-ply EFFECTIVE bending stiffness is the
    // gamma-method EffectiveStiffness(span) refinement OVER this gross-rectangle base (TIMBER_LAYUP_GEOMETRY).
    public Fin<ComputedSection> Section(Op key) =>
        ParametricSection.Rectangle(WidthMm.Value, DepthMm.Value, key);

    // The along/across-grain directional-stiffness law the seam MaterialPropertySet.Mechanical orthotropic consumer
    // reads (api-vividorange-materials.md): E0,mean parallel-to-grain (X), E90,mean perpendicular (Y/Z), with the
    // per-axis characteristic strengths. This is the INDEPENDENT-G orthotropic material the seam's isotropic
    // G = E/(2(1+ν)) derivation cannot model — timber's G_mean ≈ E0/16 is an independent datum, not derivable from a
    // Poisson ratio, so the Rasm.Materials projector lowers THIS through MaterialPropertySet.OfOrthotropic into the
    // REALIZED seam material#MATERIAL_PROPERTY Orthotropic case (E1Parallel=E0Mean, E2Perpendicular=E90Mean, ShearModulus=GMean,
    // Strength1Parallel=Fc0k, Strength2Perpendicular=Fc90k — Discipline.Structural, the case TYPE discriminating it from the
    // isotropic Mechanical), no longer a deferred future case. Pressure is the VividOrange UnitsNet floor.
    [BoundaryAdapter]
    public LinearElasticOrthotropicMaterial OrthotropicLaw() =>
        new(MaterialType.Timber,
            Pressure.FromMegapascals(Grade.E0Mean),  Pressure.FromMegapascals(Grade.Fc0k),    // X: parallel-to-grain
            Pressure.FromMegapascals(Grade.E90Mean), Pressure.FromMegapascals(Grade.Fc90k),   // Y: perpendicular
            Pressure.FromMegapascals(Grade.E90Mean), Pressure.FromMegapascals(Grade.Fc90k));  // Z: perpendicular (radial≈tangential band)

    // The gamma-method effective bending stiffness (EN 1995-1-1 Annex B mechanically-jointed beam, the CLT analogy):
    // only the LONGITUDINAL plies carry bending, coupled through the transverse plies' ROLLING-shear modulus G_R, and a
    // cross-laminated panel is SHEAR-FLEXIBLE — the Steiner (parallel-axis) coupling of each off-centre longitudinal ply
    // is reduced by its connection-efficiency factor γ_i < 1, so (EI)_eff sits BETWEEN the no-composite Σ E·I_own (γ=0)
    // and the rigid-glued Σ(E·I_own + E·A·z²) (γ=1). The γ factor is SPAN-DEPENDENT (a stiffer/longer panel couples
    // more efficiently), so this is a METHOD over the reference span ℓ, NOT a parameterless property — a parameterless
    // (EI)_eff that reads neither G_R nor ℓ is the rigid-glued upper bound mislabeled "gamma-method" (the prior defect:
    // the body summed own+Steiner with γ≡1 and never touched GRollMean). γ_i = 1 / (1 + π²·E·A_i·z_i² / (ℓ²·K_i)) with
    // z_i the ply centroid-to-neutral-axis lever (the same z the Steiner term uses, the symmetric-CLT case) and the
    // interleaved transverse ply's rolling-shear slip stiffness K_i = G_R,mean·b / t_cross (EN Annex B Eq B.5). For a
    // homogeneous (single-0°-ply) member the layup is one full-depth ply and (EI)_eff == the gross E0·I at every span.
    public double EffectiveStiffness(double referenceSpanMm) {
        double widthMm = WidthMm.Value, plyMm = Layup.PlyThicknessMm.Value, e0 = Grade.E0Mean;
        if (!Layup.IsCrossLaminated) { return e0 * widthMm * DepthMm.Value * DepthMm.Value * DepthMm.Value / 12.0; }
        double depthMm = Layup.PlyCount * plyMm, halfDepth = depthMm / 2.0;
        // The transverse-ply rolling-shear slip modulus per unit length (EN 1995-1-1 Annex B Eq B.5): G_R,mean·b / t_cross,
        // the t_cross the interleaved 90° ply thickness (one ply here); span-zero → γ→0 (no composite), span-∞ → γ→1 (rigid).
        double slipK = Layup.PlyThicknessMm.Value > 0.0 ? Grade.GRollMean * widthMm / plyMm : double.PositiveInfinity;
        double span2 = referenceSpanMm > 0.0 ? referenceSpanMm * referenceSpanMm : double.PositiveInfinity;
        // Σ over the longitudinal plies of E·I_own + γ_i·E·A_i·z² — the own-bending term is always full, the Steiner
        // coupling reduced by the per-ply γ_i (the centroidal ply has z≈0 so its γ is immaterial). z is the ply centroid
        // from the panel mid; A_i = b·t the ply area; s_i the lever the same z (the distance the slip resists).
        return Layup.LongitudinalIndices.Sum(i => {
            double zMm = (i + 0.5) * plyMm - halfDepth;
            double areaMm2 = widthMm * plyMm;
            double gammaI = 1.0 / (1.0 + Math.PI * Math.PI * e0 * areaMm2 * zMm * zMm / (span2 * slipK));
            double own = e0 * widthMm * plyMm * plyMm * plyMm / 12.0;
            double steiner = gammaI * e0 * areaMm2 * zMm * zMm;
            return own + steiner;
        });
    }

    // The CLT/LVL layered panel lowers to the seam MaterialComposition.LayerSet (the alternating-orientation plies,
    // IfcMaterialLayerSet) via the Construction/assembly CompositionAuthor — the SAME bridge glazing's IGU uses, because
    // a cross-laminated panel is genuinely a layered material, never one rectangle profile. Each ply tags its
    // MaterialId (the grade-keyed timber appearance row) and its orientation in the layer name (the 0°/90° the
    // gamma-method and the IFC layer-set read). A profiled member (sawn/glulam/LVL/PSL) is NOT a layer set — it is the
    // ProfileSet the assembly author wraps from the ProfileRef; this bridge fires only for a LayeredPanel form.
    public Fin<MaterialComposition> ToLayerSet(Op key) =>
        Form.IsPanel
            ? CompositionAuthor.LayerSet(
                toSeq(Enumerable.Range(0, Layup.PlyCount))
                    .Map(i => (MaterialId.Of($"wood.{Grade.Key}"), Layup.PlyThicknessMm.Value, $"ply-{i}-{Layup.PlyAngles[i]}deg")),
                key)
            : Fin.Fail<MaterialComposition>(ProfileFault.Family(key, $"<timber-form-not-layered-panel:{Form.Key}>"));

    // The seam MaterialPropertySet set a timber material carries — the ORTHOTROPIC structural case lowered from the
    // registered TimberGrade DATA (E0Mean parallel / E90Mean perpendicular / GMean independent shear / Fc0k∥ / Fc90k⊥
    // strengths / DensityK / ~5e-6 grain expansion), the SAME Grade scalars OrthotropicLaw() wraps for the VividOrange
    // structural solver — but lowered to the seam-NEUTRAL MaterialPropertySet.Orthotropic (the seam references no
    // VividOrange, so this passes RAW MPa doubles to the seam OfOrthotropic; OrthotropicLaw() stays the [BoundaryAdapter]
    // the Profiles/capacity#SECTION_CAPACITY / Rasm.Compute structural route reads, NOT the seam carrier). This is the
    // timber sibling of glazing#GLAZING_FAMILY GlazingSection.ToProperties: the family OWNS the section→seam-property
    // lowering, the Projection/material#MATERIAL_PROJECTOR composes it onto the Material node. The orthotropic G is the
    // INDEPENDENT datum the seam Mechanical isotropic G = E/(2(1+ν)) cannot model — the gap the seam Orthotropic case
    // (material#MATERIAL_COMPOSITION) realizes for timber. The thermal/acoustic/fire disciplines a timber material also
    // carries lower from the Properties/properties#MATERIAL_PROPERTY_CATALOGUE row (the EN ISO 10456 λ, the datasheet
    // SRI/absorption, the EN 13501 reaction) the projector composes alongside — this surface owns ONLY the structural
    // stiffness the grade data uniquely sources, never the catalogue's physical rows.
    public Fin<Seq<MaterialPropertySet>> ToProperties(Op key) =>
        MaterialPropertySet.OfOrthotropic(Grade.DensityK, Grade.E0Mean, Grade.E90Mean, Grade.GMean, Grade.Fc0k, Grade.Fc90k, 5.0e-6, key)
            .Map(static orthotropic => Seq1(orthotropic));

    // The unit-segment projection a profiled member flows through the Resolve fold on — WidthMm the section breadth,
    // DepthMm the section depth, LengthMm a unit-segment placeholder the RunPath length overrides (the SAME shape
    // steel.ToUnit uses — the run owns the length, the section owns the breadth/depth), CourseHeightMm the depth so a
    // single-orientation timber run steps once per RunPath segment, never a masonry-style multi-unit course.
    public Fin<ProfileUnit> ToUnit(Context context, Op key) =>
        ProfileUnit.Of(WidthMm.Value, DepthMm.Value, DepthMm.Value, DepthMm.Value, context, key);
}

public readonly record struct TimberRow(string Designation, string Form, string Grade, double WMm, double DMm, double PlyMm, int Plies);

public sealed record TimberShape(ProfileId Id, TimberSection Section, ProfileStandard Standard);

// --- [TABLES] ------------------------------------------------------------------------------
public static class ProfileCatalogue {
    // The bounded standards body is ProfileAuthority.En (the profile#PROFILE_OWNER [SmartEnum] row, region "eu"), NOT a
    // free authority string — the EN 14080/338/14374 + APA PRG 320 spec span is the family's documented coverage, never
    // a ProfileStandard column. StandardJointThicknessMm is 0 (a sawn/glulam/CLT member has no mortar joint).
    static readonly ProfileStandard En14080 = new("eu", StandardJointThicknessMm: 0.0, Authority: ProfileAuthority.En);

    static readonly Seq<TimberRow> TimberRows = Seq(
        new TimberRow("timber.sawn-c16-38x89",       "sawn",   "c16",    38.0,  89.0,  89.0,  1),
        new TimberRow("timber.sawn-c24-38x140",      "sawn",   "c24",    38.0,  140.0, 140.0, 1),
        new TimberRow("timber.sawn-c24-38x184",      "sawn",   "c24",    38.0,  184.0, 184.0, 1),
        new TimberRow("timber.sawn-c30-63x175",      "sawn",   "c30",    63.0,  175.0, 175.0, 1),
        new TimberRow("timber.sawn-d40-75x225",      "sawn",   "d40",    75.0,  225.0, 225.0, 1),
        new TimberRow("timber.glulam-gl24h-90x225",  "glulam", "gl24h",  90.0,  225.0, 45.0,  5),
        new TimberRow("timber.glulam-gl28h-90x270",  "glulam", "gl28h",  90.0,  270.0, 45.0,  6),
        new TimberRow("timber.glulam-gl30h-115x360", "glulam", "gl30h",  115.0, 360.0, 45.0,  8),
        new TimberRow("timber.glulam-gl32h-115x405", "glulam", "gl32h",  115.0, 405.0, 45.0,  9),
        new TimberRow("timber.glulam-gl28c-140x630", "glulam", "gl28c",  140.0, 630.0, 45.0,  14),
        new TimberRow("timber.lvl-lvl48p-75x300",    "lvl",    "lvl48p", 75.0,  300.0, 3.0,   100),
        new TimberRow("timber.clt-c24-3ply-90",      "clt",    "c24",    1250.0,  90.0, 30.0, 3),
        new TimberRow("timber.clt-c24-5ply-150",     "clt",    "c24",    1250.0, 150.0, 30.0, 5),
        new TimberRow("timber.clt-c24-7ply-230",     "clt",    "c24",    1250.0, 230.0, 33.0, 7));

    static Fin<TimberShape> TimberOf(TimberRow r, Context context, Op key) =>
        from w in key.AcceptValidated<PositiveMagnitude>(candidate: r.WMm)
        from d in key.AcceptValidated<PositiveMagnitude>(candidate: r.DMm)
        from ply in key.AcceptValidated<PositiveMagnitude>(candidate: r.PlyMm)
        from form in TimberForm.TryGet(r.Form, out TimberForm? f) ? Fin.Succ(f!) : Fin.Fail<TimberForm>(ProfileFault.Family(key, $"<unknown-timber-form:{r.Form}>"))
        from grade in TimberGrade.TryGet(r.Grade, out TimberGrade? g) ? Fin.Succ(g!) : Fin.Fail<TimberGrade>(ProfileFault.Family(key, $"<unknown-timber-grade:{r.Grade}>"))
        let layup = form.CrossPly ? PlyLayup.CrossLaminated(ply, r.Plies) : PlyLayup.Homogeneous(d)
        select new TimberShape(ProfileId.Of(r.Designation), new TimberSection(form, grade, w, d, layup), En14080);

    // Every realized timber section resolved to its TimberShape ONCE — the seed BuildTimberRows and TimberSections both
    // derive from (DERIVED_LOGIC, one source), the SAME shape steel#STEEL_FAMILY Shapes takes. A malformed row drops
    // through Choose rather than seeding a partial.
    static readonly Seq<TimberShape> Shapes =
        TimberRows.Choose(row => TimberOf(row, default, default).ToOption());

    public static FrozenDictionary<ProfileId, Profile> BuildTimberRows(Context context) =>
        Shapes
            .Choose(shape => shape.Section.ToUnit(context, default).ToOption()
                .Map(unit => (shape.Id, Profile: new Profile(ProfileFamily.Timber, unit, Coring.None, shape.Standard, MaterialId.Of($"wood.{shape.Section.Grade.Key}")))))
            .ToFrozenDictionary(static r => r.Id, static r => r.Profile, ComparerAccessors.StringOrdinal.EqualityComparer);

    // The ProfileId → ComputedSection map the profile#PROFILE_OWNER ProfileCatalogue.Sections folds and the
    // [03]-[PROFILE_RESOLUTION] ProfileResolution.Build caches by ProfileRef (the realized sibling to steel#STEEL_FAMILY
    // SteelSections / cmu#CMU_FAMILY CmuSections). The gross elastic section runs ONCE here through the shared
    // profile#PROFILE_OWNER ParametricSection.Rectangle solver — a section whose integral fails drops through Choose (a
    // build-time ProfileFault.Section surfaced in THIS page, never a swallowed gap the resolver mis-reports). A profiled
    // MEMBER (sawn/glulam/LVL/PSL) carries its rectangle section; a layered CLT PANEL is FILTERED out and joins
    // Option<ComputedSection>.None at Build (a panel is an IfcMaterialLayerSet, never a profiled section), its
    // gamma-method EffectiveStiffness(span) the CLT deflection check reads off the TimberSection directly, not this map.
    public static FrozenDictionary<ProfileId, ComputedSection> TimberSections(Context context) =>
        Shapes
            .Filter(static shape => !shape.Section.Form.IsPanel)
            .Choose(shape => shape.Section.Section(default).ToOption().Map(section => (shape.Id, Section: section)))
            .ToFrozenDictionary(static r => r.Id, static r => r.Section, ComparerAccessors.StringOrdinal.EqualityComparer);
}
```

## [03]-[TIMBER_CAPACITY]

- Owner: `TimberCapacity` the EN 1995-1-1 design-resistance receipt (bending / compression-with-buckling / shear / perpendicular-bearing) plus the EN 1995-1-2 reduced-cross-section charring residual; `TimberDesign` the static design-code operations owner (the EC5 math, the SAME static-helper shape the steel family's `PlasticModulus` takes), `TimberDesign.Capacity` the derived projection that folds the `TimberGrade` characteristic vector through the `k_mod`/`γ_M`/`k_h`/`k_c` factors over a `TimberSection`, `TimberDesign.ResidualSection` the fire reduced-cross-section.
- Cases: one `TimberCapacity` receipt across all forms — the design bending moment `M_Rd`, the design compression `N_Rd` (column-buckling-reduced through `k_c`), the design shear `V_Rd` (with the EN 1995-1-1 §6.1.7 crack factor `k_cr` for solid/glulam, the CLT rolling-shear governing for a panel), the perpendicular-to-grain bearing `R_90,Rd`, the EN 1995-1-1 §6.1.8 design torsional resistance `T_Rd = k_shape·f_v,d·W_tor`, and the governing stability slenderness `λ_rel`; a capacity is a derived projection over the section columns, never a per-form check surface.
- Entry: `public static Fin<TimberCapacity> Capacity(TimberSection s, ServiceClass service, LoadDuration duration, double effectiveLengthMm, Op key)` on `TimberDesign` — the ONE EC5 design projection: it reads the `k_mod = LoadDuration.KmodFor(service)`, applies the material partial factor `γ_M` per form (1.25 solid/LVL/glulam/CLT — EN 1995-1-1 Table 2.3), the size-effect `k_h` for bending of small glulam/solid depths, the buckling reduction `k_c` from the relative slenderness `λ_rel = √(f_c0k / σ_c,crit)` over the gross/effective section, and emits the design resistances over the computed `ComputedSection` columns; `public static ResidualSection ResidualSection(TimberSection s, double exposureMinutes, int exposedSides)` the EN 1995-1-2 reduced cross-section (the design char depth `d_char,n = β_n·t + d0` with the 7 mm zero-strength layer, `β_n` the density-adjusted `TimberForm.BetaN(ρ_k)` notional rate — a dense D-class hardwood charrs slower than a softwood — the residual breadth/depth a fire `Capacity` re-reads, TOTAL — the char arithmetic clamps to a ≥1 mm residual); a design check reads `M_Ed / capacity.BendingNmm` as the utilisation, the SAME shape the `Profiles/capacity#SECTION_CAPACITY` rail lifts a `SteelLrfd` receipt through.
- Packages: Rasm (project — `PositiveMagnitude`), Thinktecture.Runtime.Extensions, LanguageExt.Core; the `ComputedSection` (the gross elastic section) comes from `[02]`'s `TimberSection.Section`, the span-dependent `EffectiveStiffness(span)` from the `PlyLayup` gamma-method (the per-ply `γ_i` connection-efficiency factor reading `GRollMean` and the reference span) — no VividOrange member beyond the orthotropic law, the EN 1995 design rules HAND-ROLLED (no .NET EC5 package, the `survey-gaps-materials-deep` admission found none).
- Growth: a new design check is one column on `TimberCapacity` (a notched-beam shear reduction `k_v`, a lateral-torsional-buckling `k_crit` for a deep glulam beam, a combined-bending-axial interaction); a new fire route is one `ResidualSection` parameter (a charring-rate adjustment for a protected member's `t_ch` delay); a new form's design rule is one `Capacity` arm reading the `TimberForm` discriminant — never a per-form capacity surface, never a re-minted characteristic strength where `TimberGrade` carries it.
- Boundary: `TimberCapacity` is the EC5 design receipt — a hand-keyed allowable-stress literal is the deleted form; the design strength is ALWAYS the characteristic strength × `k_mod` ÷ `γ_M` (EN 1995-1-1 §2.4.1), so `k_mod` (the joint of `ServiceClass` AND `LoadDuration`) is a first-class input the `Capacity` reads from the `LoadDuration.KmodFor(service)` table, never an implicit dry-permanent assumption; the bending resistance reads ONE effective section modulus `W_eff = (EI)_eff / (E0·c)` (`c` the gross half-depth) over the gamma-method span-dependent `EffectiveStiffness(effectiveLengthMm)` for EVERY form — a homogeneous sawn/glulam member's `(EI)_eff` IS `E0·I_gross` at every span so `W_eff` degrades EXACTLY to the gross `ComputedSection.SxMm3`, while a shear-flexible CLT panel's `(EI)_eff` is the per-ply `γ_i`-reduced Steiner sum (the `γ_i` reading `GRollMean` and the span) sitting BETWEEN the no-composite and rigid-glued bounds, governing through the SAME expression (the per-form gross-Sx-vs-effective branch is the deleted illusory form, AND the prior parameterless `(EI)_eff` that summed own+Steiner with `γ≡1` and read neither `G_R` nor the span — the rigid-glued upper bound mislabeled "gamma-method" — is the doubly-deleted illusory body), so a cross-laminated panel's shear-flexible composite stiffness governs its deflection the way neither the gross-rectangle assumption nor the rigid-glued sum could; the compression resistance applies the EN 1995-1-1 §6.3.2 column-buckling factor `k_c` from the relative slenderness `λ_rel` over the `effectiveLengthMm` and the `E005` 5%-fractile modulus (the stability modulus, never the mean `E0Mean`), so a slender column's buckling governs; the shear resistance applies the `k_cr` crack factor for a solid/glulam member and the rolling-shear `FRvk`/`GRollMean` for a CLT panel (the transverse plies' rolling shear, the CLT-governing failure mode the gross shear cannot model); the perpendicular-to-grain bearing `R_90,Rd` reads `Fc90k` (a support reaction crushing a glulam beam across the grain, a check a parallel-only model drops); the `ResidualSection` is the EN 1995-1-2 reduced-cross-section method (the design char depth removes a `β_n·t + 7 mm` zero-strength layer from each exposed side, `β_n` the density-adjusted `TimberForm.BetaN(grade ρ_k)` notional rate — a dense EN 338 D-class hardwood charrs at 0.55 mm/min, a softwood at the form's 0.70/0.80, never one flat rate — a fire `Capacity(...)` then re-evaluated over the residual breadth/depth at `k_mod = 1.0` and `γ_M = 1.0`), the fire-design route the `Composition/material#MATERIAL_COMPOSITION` `SectionProperties.HeatedPerimeter` carries for steel but timber owns through charring; the `TimberCapacity` lifts into the `Profiles/capacity#SECTION_CAPACITY` unified rail the way the steel `DesignCapacity` does through the REALIZED `SectionCapacityResolver.TimberEc5(TimberCapacity)` factory (which reads the `TorsionalNmm` column off the receipt — no redundant lift parameter) onto the realized `SectionCapacity.TimberEc5` case (no longer a future case, the cross-file ripple now closed), so a timber column and a steel column are checked through the SAME `Check(demand)` utilisation fold differing only in the capacity case — the design-code computation HAND-ROLLED here, the unified verdict the capacity owner's; the EN 1995-1-1 §6.1.8 torsional resistance `T_Rd = k_shape·f_v,d·W_tor` is REALIZED as the `TimberCapacity.TorsionalNmm` column (`f_v,d = k_mod·f_v,k/γ_M` the design longitudinal shear strength, `k_shape = min(1 + 0.15·h/b, 2.0)` the §6.1.8 rectangular factor, `W_tor = α·h·b²` the Roark rectangular torsion modulus over `b = min(w,h)`/`h = max(w,h)` — the SAME α(b/h) the steel `SolidShape` `J` uses), which the `SectionCapacityResolver.TimberEc5` lift reads directly onto `SectionCapacity.TimberEc5.TorsionalKnm`, so a torsion-loaded glulam member checks against a real resistance instead of an inert 0 and `demand.TorsionKnm` is a CONSUMED action.

```csharp signature
// --- [MODELS] ------------------------------------------------------------------------------
// The EN 1995-1-1 design-resistance receipt — the design bending/compression/shear/perpendicular-bearing resistances
// (each characteristic × k_mod ÷ γ_M, with the k_h size-effect, k_c buckling, k_cr crack reductions applied), plus the
// governing relative slenderness. A design check folds the applied action against it (M_Ed/BendingNmm ≤ 1), the SAME
// shape the Profiles/capacity#SECTION_CAPACITY SteelLrfd receipt lifts into the one utilisation rail. SI N·mm / N.
public readonly record struct TimberCapacity(
    double BendingNmm,        // M_Rd = k_h·k_mod·f_m,k·W / γ_M
    double CompressionN,      // N_Rd = k_c·k_mod·f_c0,k·A / γ_M (column-buckling-reduced)
    double ShearN,            // V_Rd = k_mod·f_v,k·k_cr·A_shear / γ_M (rolling-shear for a CLT panel)
    double BearingPerpN,      // R_90,Rd = k_mod·f_c90,k·A_bearing / γ_M
    double TorsionalNmm,      // T_Rd = k_shape·f_v,d·W_tor (EN 1995-1-1 §6.1.8; f_v,d = k_mod·f_v,k/γ_M, k_shape = min(1+0.15·h/b, 2))
    double RelativeSlenderness,  // λ_rel,c = √(f_c0,k / σ_c,crit) — the EN 1995-1-1 §6.3.2 stability input
    double Kmod) {            // the joint service×duration modification factor the resistances scaled by
    public double FlexuralKnm => BendingNmm * 1e-6;
    public double CompressionKn => CompressionN * 1e-3;
}

// The EN 1995-1-2 reduced cross-section — the residual breadth/depth after the design char depth d_char,n = β_n·t + d0
// (β_n the density-adjusted TimberForm.BetaN notional rate, d0 = 7 mm zero-strength layer) is removed from each exposed
// side; a fire-design Capacity(...) re-evaluates over this residual at k_mod = γ_M = 1.0. A 3-side-exposed beam loses
// char from both faces + the soffit, a 4-side column from all.
public readonly record struct ResidualSection(PositiveMagnitude ResidualWidthMm, PositiveMagnitude ResidualDepthMm, double CharDepthMm);

// --- [OPERATIONS] --------------------------------------------------------------------------
// The EN 1995-1-1 design-resistance projection over a TimberSection. HAND-ROLLED (no .NET EC5 package): the design
// strength is characteristic × k_mod ÷ γ_M, the bending carries the k_h depth-size effect, the compression the §6.3.2
// k_c buckling reduction over the E005 5%-fractile stability modulus, the shear the k_cr crack factor (solid/glulam)
// or the CLT rolling-shear, the bearing the perpendicular f_c90,k. Every input is the registered TimberGrade DATA, the
// computed ComputedSection columns, or the gamma-method EffectiveStiffness — never a hand-keyed allowable stress.
public static class TimberDesign {
    const double GammaM = 1.25;   // EN 1995-1-1 Table 2.3 — solid/glulam/LVL/CLT
    const double Kh0 = 1.0;       // the k_h reference (no size gain above the reference depth); the §6.3.2 imperfection factor βc is per-form below

    public static Fin<TimberCapacity> Capacity(TimberSection s, ServiceClass service, LoadDuration duration, double effectiveLengthMm, Op key) =>
        s.Section(key).Map(cs => {
            double kmod = duration.KmodFor(service);
            double depthMm = s.DepthMm.Value, widthMm = s.WidthMm.Value, areaMm2 = cs.AreaMm2.Value;
            // k_h size effect: solid ≤150 mm / glulam ≤600 mm reference depth gain capped at 1.3 (EN 1995-1-1 §3.2/§3.3).
            double refDepth = s.Form == TimberForm.Glulam ? 600.0 : 150.0;
            double kh = depthMm < refDepth ? Math.Min(Math.Pow(refDepth / depthMm, s.Form == TimberForm.Glulam ? 0.1 : 0.2), 1.3) : Kh0;
            // k_c column buckling over the E005 5%-fractile modulus (the stability modulus, NOT the mean E0).
            double radiusMm = Math.Min(cs.RxMm.Value, cs.RyMm.Value);
            double lambda = effectiveLengthMm > 0.0 && radiusMm > 0.0 ? effectiveLengthMm / radiusMm : 0.0;
            double sigmaCrit = lambda > 0.0 ? Math.PI * Math.PI * s.Grade.E005 / (lambda * lambda) : double.PositiveInfinity;
            double lambdaRel = sigmaCrit > 0.0 ? Math.Sqrt(s.Grade.Fc0k / sigmaCrit) : 0.0;
            double betaC = s.Form == TimberForm.Glulam || s.Form.CrossPly ? 0.1 : 0.2;
            double kFac = 0.5 * (1.0 + betaC * (lambdaRel - 0.3) + lambdaRel * lambdaRel);
            double kc = lambdaRel <= 0.3 ? 1.0 : 1.0 / (kFac + Math.Sqrt(Math.Max(0.0, kFac * kFac - lambdaRel * lambdaRel)));
            // Shear: the §6.1.7 crack factor k_cr for solid/glulam; a CLT panel's transverse plies govern by rolling shear.
            (double fv, double kcr, double aShear) = s.Form.CrossPly
                ? (s.Grade.FRvk, 1.0, widthMm * s.Layup.LongitudinalThicknessMm)
                : (s.Grade.Fvk, 0.67, areaMm2 * 2.0 / 3.0);
            // The bending section modulus is the gamma-method EFFECTIVE modulus W_eff = (EI)_eff / (E0·c) (c = h/2), the
            // (EI)_eff the SPAN-DEPENDENT EffectiveStiffness over effectiveLengthMm (the γ-method reference span — a CLT
            // panel's connection efficiency rises with span), so a shear-flexible CLT panel's reduced (EI)_eff governs its
            // flexure; a homogeneous sawn/glulam member's (EI)_eff IS E0·I_gross at every span, so W_eff degrades EXACTLY
            // to the gross cs.SxMm3 — one expression, both forms. A zero/unset effectiveLengthMm drives γ→0 (the no-
            // composite lower bound), the conservative panel modulus when no span is supplied. E0Mean and the half-depth
            // are both > 0, so the division is total. (The prior parameterless EffectiveStiffnessNmm2 was the illusory
            // arm: it summed own+Steiner with γ≡1 — the rigid-glued upper bound — and read neither G_R nor the span.)
            double wEffMm3 = s.EffectiveStiffness(effectiveLengthMm) / (s.Grade.E0Mean * depthMm * 0.5);
            // EN 1995-1-1 §6.1.8 torsion: T_Rd = k_shape·f_v,d·W_tor. f_v,d is the SOLID/glulam design shear strength
            // (k_mod·f_v,k/γ_M — the §6.1.8 torsional shear reads the f_v,k longitudinal strength, never the CLT rolling
            // shear, since the torsional shear flows in the grain-parallel planes), k_shape the §6.1.8 rectangular cross-
            // section factor min(1 + 0.15·h/b, 2.0), and W_tor the Roark rectangular St-Venant torsion modulus
            // α·h·b² over b = min(w,h)/h = max(w,h) — the SAME α(b/h) shape-factor the steel SolidShape J uses, grounding
            // W_tor in the section geometry not a literal. A panel form's torsion is engineering-immaterial (a plate
            // resists torsion through plate action, not St-Venant), so a CLT panel's T_Rd rides the same rectangular form
            // over its module — the unified rail folds demand.TorsionKnm against it the way the steel arm folds φTn.
            double bTor = Math.Min(widthMm, depthMm), hTor = Math.Max(widthMm, depthMm);
            double alphaTor = 1.0 / 3.0 - 0.21 * (bTor / hTor) * (1.0 - Math.Pow(bTor / hTor, 4.0) / 12.0);   // Roark rectangular torsion-modulus coefficient
            double wTorMm3 = alphaTor * hTor * bTor * bTor;
            double kShape = Math.Min(1.0 + 0.15 * hTor / Math.Max(bTor, double.Epsilon), 2.0);                // §6.1.8 rectangular k_shape, capped 2.0
            double fvd = kmod * s.Grade.Fvk / GammaM;                                                         // design shear strength (longitudinal f_v,k, NOT FRvk)
            return new TimberCapacity(
                BendingNmm: kh * kmod * s.Grade.Fmk * wEffMm3 / GammaM,
                CompressionN: kc * kmod * s.Grade.Fc0k * areaMm2 / GammaM,
                ShearN: kmod * fv * kcr * aShear / GammaM,
                BearingPerpN: kmod * s.Grade.Fc90k * widthMm * depthMm / GammaM,
                TorsionalNmm: kShape * fvd * wTorMm3,
                RelativeSlenderness: lambdaRel,
                Kmod: kmod);
        });

    // EN 1995-1-2 reduced cross-section: remove d_char,n = β_n·t + 7 mm from each exposed side (β_n the notional charring
    // rate INCLUDING corner rounding), then a fire Capacity re-runs over the residual at k_mod = 1. β_n is the density-
    // adjusted TimberForm.BetaN(grade ρ_k) — a dense D-class hardwood charrs at 0.55, a softwood at the form's 0.70/0.80
    // — NOT a flat per-form constant (the prior Beta0CharMmPerMin gave a D40 hardwood a softwood rate). TOTAL — the char
    // arithmetic clamps to a ≥1 mm residual (no Fin rail, no Op key; the residual feeds a fire Capacity(...) that owns
    // its own ComputedSection admission), and a fully-charred section yields the 1 mm floor.
    public static ResidualSection ResidualSection(TimberSection s, double exposureMinutes, int exposedSides) {
        double charDepthMm = s.Form.BetaN(s.Grade.DensityK) * Math.Max(0.0, exposureMinutes) + 7.0;
        double residWidth = Math.Max(1.0, s.WidthMm.Value - (exposedSides >= 2 ? 2.0 : 1.0) * charDepthMm);
        double residDepth = Math.Max(1.0, s.DepthMm.Value - (exposedSides >= 3 ? 2.0 : exposedSides >= 1 ? 1.0 : 0.0) * charDepthMm);
        return new ResidualSection(PositiveMagnitude.Create(residWidth), PositiveMagnitude.Create(residDepth), charDepthMm);
    }
}
```

## [04]-[RESEARCH]

- [TIMBER_ROW_TRANSCRIPTION]: REALIZED — EN 14080 carries the glulam strength classes (GL24h/GL28h/GL30h/GL32h homogeneous, the GL24c/GL28c combined variants) with the standard 40–45 mm lamella thickness, EN 338 the C16/C24/C30 structural-sawn and D40 hardwood grades, EN 14374 the LVL classes, and APA PRG 320 / EN 16351 the CLT layup grades with the 3/5/7-ply cross-laminated lamellae at 20–40 mm; the catalogue carries the C16/C24/C30/D40 sawn sizes (38×89 through 75×225), the GL24h-GL32h homogeneous and GL28c combined glulam sections, the LVL48P veneer section, and the 3/5/7-ply CLT panels, the standard structural set keyed `timber.<designation>`, the remaining EN strength classes and the curved/tapered/box glulam engineered products one further `TimberRow` data addition, each one row, never a new type. Each `TimberGrade` carries the FULL EN characteristic strength-AND-stiffness vector (the bending/tension/compression-parallel-and-perpendicular/shear/rolling-shear strengths, the mean-and-5%-fractile axial moduli, the perpendicular modulus, the shear-and-rolling-shear moduli, the characteristic density) as registered DATA, so the EC5 `Capacity` and the orthotropic law read the class DATA, never a hand-keyed literal — `VividOrange.Materials` ships NO timber factory (its EN factories are concrete/steel/rebar only, `.api/api-vividorange-materials.md` `[FLOOR_SCOPE_GATE]`), so the `TimberGrade` `[SmartEnum]`-as-DATA is the registered-class owner the SAME way `SteelGrade`'s AISC `None` bands carry their spec yield. The CLT panel width is the full panel module the layup repeats, the sawn/glulam width the rectangular section breadth, the lamella thickness/count carrying the layup the gamma-method composite stiffness reads.
- [ORTHOTROPIC_STIFFNESS_LAW]: REALIZED — timber is an ORTHOTROPIC material (its grain-parallel modulus `E0,mean ≈ 11–14 GPa` is ~30× its perpendicular `E90,mean ≈ 300–750 MPa`, and its shear modulus `G_mean ≈ E0/16` is an INDEPENDENT datum a Poisson ratio cannot derive), so `TimberSection.OrthotropicLaw` produces a `VividOrange.Materials` `LinearElasticOrthotropicMaterial` carrying `E0,mean` parallel (X) and `E90,mean` perpendicular (Y/Z) plus the per-axis characteristic strengths (VERIFIED `LinearElasticOrthotropicMaterial(MaterialType, Pressure Ex, Pressure Sx, Pressure Ey, Pressure Sy, Pressure Ez, Pressure Sz)`, the `.api/api-vividorange-materials.md` "timber/composite directional-stiffness law"). The seam `Composition/material#MATERIAL_PROPERTY` carries the REALIZED `MaterialPropertySet.Orthotropic` case (`Density`/`E1Parallel`/`E2Perpendicular`/`ShearModulus`/`Strength1Parallel`/`Strength2Perpendicular`/`ThermalExpansionPerK`, `Discipline.Structural`) — the directional-stiffness sibling of the isotropic `Mechanical` whose `G = E/(2(1+ν))` derivation cannot model timber's independent `G_mean ≈ E0/16` — so the `Rasm.Materials` `MaterialProjector` lowers this `LinearElasticOrthotropicMaterial` through `MaterialPropertySet.OfOrthotropic(densityK, E0Mean, E90Mean, GMean, Fc0k, Fc90k, …)` onto that case, the directional stiffness the `Rasm.Compute` structural route reads off the seam graph (`props.Property<MaterialPropertySet.Orthotropic>()`) rather than an isotropic approximation. Ripple counterpart: `Rasm.Element` `Composition/material` `[MATERIAL_PROPERTY]` (the realized seam `Orthotropic` case timber's `LinearElasticOrthotropicMaterial` lowers into) and `Rasm.Compute` (the orthotropic structural route).
- [IFCPROFILEDEF_TIMBER_ALIGNMENT]: REALIZED — a timber product crosses the IFC wire as its `MaterialShapeKind`: a profiled MEMBER (sawn/glulam/LVL/PSL) is an `IfcMaterialProfileSet` whose profile is the `IfcRectangleProfileDef` rectangle (the `WidthMm`/`DepthMm` breadth/depth) the seam `Construction/assembly#MATERIAL_COMPOSITION` `MaterialComposition.ProfileSet(ProfileRef)` carries, the curved/tapered glulam a `SweptArea` extrusion the host materializes from the rectangle along the `RunPath`; a layered PANEL (CLT) is an `IfcMaterialLayerSet` the `TimberSection.ToLayerSet` bridge resolves (the alternating-orientation plies, each `IfcMaterialLayer` carrying its `LayerThickness` and the 0°/90° orientation in the layer name) — the cross-laminated panel is genuinely a layered material the SAME way glazing's IGU is (`glazing#IFCPROFILEDEF_GLAZING_ALIGNMENT`), so CLT is the `IfcMaterialLayerSet` the layer-set assignment owner already models, NEVER a single rectangle profile (the prior "CLT as `IfcRectangleProfileDef`" was the contradiction the rebuild cures — a profile set has no layers, yet the prior prose claimed CLT round-trips "as an `IfcMaterialProfileSet` whose ... layers"). The `TimberForm.Shape` discriminant is the `ToLayerSet`-vs-`ProfileSet` selection. The per-member `IfcMaterialProfileSetUsage` cardinal-point/orientation is the `[C7]` `ProfileSetUsage` the seam `Associate` edge carries (the `CompositionAuthor.UsageOf` derivation), authored onto IFC at the `Rasm.Bim` boundary; a CLT panel's `IfcMaterialLayerSetUsage` direction/offset rides the same edge. Ripple counterpart: `Rasm.Element` `Composition/material` `[MATERIAL_COMPOSITION]` (the `LayerSet`/`ProfileSet` cases).
- [TIMBER_LAYUP_GEOMETRY]: REALIZED — the cross-ply layup is the realized `PlyLayup` (the per-ply thickness + the 0°/90° orientation sequence), the entire point of cross-lamination the prior model asserted in prose but never carried. A homogeneous sawn/glulam member is the single full-depth 0° ply and its `EffectiveStiffness(span)` IS the gross `E0·I` the `ParametricSection.Rectangle` solver computes at every span; a 3/5/7-ply CLT panel's cross-ply EFFECTIVE bending stiffness is the EN 1995-1-1 Annex B gamma-method refinement OVER that gross base — only the LONGITUDINAL (0°) plies carry bending, coupled through the TRANSVERSE (90°) plies' ROLLING-shear modulus `G_R,mean`, so each off-centre longitudinal ply's Steiner coupling is reduced by its connection-efficiency factor `γ_i = 1/(1 + π²·E·A_i·z_i²/(ℓ²·K_i))` (the interleaved transverse ply's rolling-shear slip stiffness `K_i = G_R,mean·b/t_cross`, EN Annex B Eq B.5) and the panel is genuinely shear-flexible — `(EI)_eff` sits BETWEEN the no-composite `ΣE·I_own` (`γ→0`) and the rigid-glued `Σ(E·I_own + E·A·z²)` (`γ→1`), span-dependent because `γ` rises with span. The prior body summed own+Steiner with `γ≡1` and read neither `G_R` nor the span — the rigid-glued upper bound mislabeled "gamma-method," the illusory thread this realizes; `GRollMean` is now LOAD-BEARING in `(EI)_eff` (not only the `FRvk`/`GRollMean` shear arm), the CLT-governing rolling shear the gross rectangle cannot model and the `TimberDesign.Capacity` shear arm reads. The per-ply orientation and the rolling-shear modulus are realized `TimberSection`/`TimberGrade` columns, never a parallel section owner. The CLT panel crosses the IFC wire as the `IfcMaterialLayerSet` (`TimberSection.ToLayerSet`), the layup the layer rows, NOT a rectangle profile.
- [EC5_DESIGN_CAPACITY]: REALIZED — the `[02]` timber family was a thin section-property slice (a `Section` projection + a `ToUnit` bridge) with NO design capacity, while the steel family carries a full `DesignCapacity` LRFD receipt; the `[03]-[TIMBER_CAPACITY]` `TimberDesign.Capacity` closes the gap with the EN 1995-1-1 design-resistance projection — the design strength characteristic × `k_mod` ÷ `γ_M` (the `k_mod` the joint of `ServiceClass` AND `LoadDuration`, the single most important timber design factor the prior model omitted entirely), the bending with the `k_h` depth-size effect, the compression with the §6.3.2 `k_c` column-buckling reduction over the `E005` 5%-fractile stability modulus, the shear with the `k_cr` crack factor (solid/glulam) or the CLT rolling-shear, the perpendicular-to-grain bearing `R_90,Rd` reading `Fc90k`. Every input is the registered `TimberGrade` DATA, the computed `ComputedSection` columns, or the span-dependent gamma-method `EffectiveStiffness(span)` (the per-ply `γ_i` connection-efficiency factor reading `GRollMean`) — never a hand-keyed allowable stress; the EN 1995-1-2 `ResidualSection` reduced-cross-section method (the `β_n·t + 7 mm` design char depth, `β_n` the DENSITY-ADJUSTED `TimberForm.BetaN(grade ρ_k)` notional rate — a dense EN 338 D-class hardwood charrs at 0.55 mm/min, a softwood glulam/LVL/PSL at 0.70 and a solid sawn / CLT panel at 0.80, the EN 1995-1-2 Table 3.1 (form, density) function the prior flat `Beta0CharMmPerMin` 0.65/0.70 column collapsed and mis-rated for a hardwood) gives timber its fire route the way steel's `HeatedPerimeter` gives the EN 1993-1-2 section factor. The `TimberCapacity` lifts into the `Profiles/capacity#SECTION_CAPACITY` unified utilisation rail through the REALIZED `SectionCapacityResolver.TimberEc5(TimberCapacity)` factory onto the realized `SectionCapacity.TimberEc5(BendingKnm, CompressionKn, ShearKn, BearingPerpKn, TorsionalKnm, RelativeSlenderness, Kmod)` case the SAME way the steel `DesignCapacity` lifts as `SteelLrfd` — no longer a future case — so a timber column and a steel column are checked through ONE `Check(demand)` fold (the EN 1995 `max(M_Ed/M_Rd, N_Ed/N_Rd, V_Ed/V_Rd, T_Ed/T_Rd)`) differing only in the capacity case — the EN 1995 design rules HAND-ROLLED (no .NET EC5 package, the `survey-gaps-materials-deep` admission found none, the SAME hand-roll the steel AISC/EN and the RC ACI/EN checks take). The EN 1995-1-1 §6.1.8 design torsional resistance `T_Rd = k_shape·f_v,d·W_tor` is REALIZED as the `TimberCapacity.TorsionalNmm` column the `TimberEc5` lift threads onto `SectionCapacity.TimberEc5.TorsionalKnm`, so the `T_Ed/T_Rd` arm checks against a real resistance rather than the inert 0 default. Ripple counterpart: `Profiles/capacity` `[SECTION_CAPACITY]` (the realized `TimberEc5` case + `SectionCapacityResolver.TimberEc5` lift the timber `TimberCapacity` feeds, the `TorsionalNmm` column the lift reads onto `TorsionalKnm`) and `Rasm.Compute` (the timber structural route reading the design resistances off the seam).
