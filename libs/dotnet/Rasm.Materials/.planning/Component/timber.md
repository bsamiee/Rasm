# [MATERIALS_TIMBER]

THE TIMBER SEED FAMILY GROUNDED IN THE EN STRENGTH-CLASS TABLES. A sawn/glulam/LVL/PSL member and a cross-laminated panel are each one `ComponentRow` the ONE `component#COMPONENT_SEED` generator mints from `TimberSeed.Roster` under `TimberSeed.Law` over the `ComponentFamily.Timber` policy row (`ComponentClass.Primary`, `DetailLane.None`, admits a `SectionProfile.Rectangle` or a `SectionProfile.Layered` containing only `PlyRole.Longitudinal`/`PlyRole.Transverse`, cross-nominal the section depth) — never a `GlulamBeam`/`CltPanel` type, never a hand-keyed strength literal, and never a bespoke `TimberSection` payload. A profiled MEMBER is `SectionProfile.Rectangle` and solvable; a CLT PANEL is `SectionProfile.Layered`, which carries no section integral and therefore no section-map entry. `TimberSeed.Resolve` joins either resolved `ComponentId` back to the typed `TimberRow`, preserving the product form, the registered grade, and the declared edgewise strength for `TimberDesign.Capacity` without widening `Component` or adding a detail bag. The strength class is a `component#MATERIAL_GRADE` `MaterialGrade` row over the `GradeProperties.Timber` arm whose PHYSICS — the rolling-shear modulus, the species read, the orthotropic law, and the contract lowering — is co-located here with the family that owns it. `TimberDesign` owns the EN 1995-1-1 capacity and Annex B stiffness operations with the EN 1995-1-2 member and CLT fire modalities; `TimberCapacity` is the frozen resistance set the `capacity#SECTION_CAPACITY` `SectionCapacity.Lift(lift, key)` `CapacityLift.Timber`/`TimberFire` cases consume.

## [01]-[INDEX]

- [02]-[TIMBER_FAMILY]: `TimberForm`, `ServiceClass`, `LoadDuration`, the `GradeProperties.Timber` physics members, the authored `TimberRow` table with its declared edgewise column, and `TimberSeed.Roster`/`Law`/`Resolve`/`Capacity`.
- [03]-[TIMBER_CAPACITY]: the `TimberDesign.Capacity` EN 1995-1-1 design-resistance projection (one entry, member-vs-panel discriminated by the admitted form/profile correspondence), the `DesignState` ambient/accidental pair and the `TimberDesign.Fire` verdict over it, the Annex B gamma-method `EffectiveStiffness` over non-uniform role-tagged plies, the EN 1995-1-2 `ResidualSection` and `ResidualStack` charring modalities, and the frozen `TimberCapacity` the `capacity#SECTION_CAPACITY` pipeline lifts.

## [02]-[TIMBER_FAMILY]

- Owner: `TimberForm` the product-form policy row (sawn/glulam/clt/lvl/psl); the `GradeProperties.Timber` partial members the EN clause-bound columns mean; `ServiceClass`/`LoadDuration` the EC5 modification-factor axes; `TimberRow` the authored section table carrying the product's DECLARED edgewise strength; `TimberSeed` the roster, the seed law, the typed `Resolve` join, and the capacity producer.
- Cases: form {sawn (EN 338 solid), glulam (EN 14080 lamellae), clt (EN 16351 / APA PRG 320 cross-laminated), lvl (EN 14374 veneer), psl (parallel-strand, ETA-governed)} × grade {the forty-one `ComponentFamily.Timber` `MaterialGrade` rows — twelve EN 338 softwood classes, fourteen hardwood, fourteen EN 14080 glulam, one EN 14374 LVL declaration} × service {sc1/sc2/sc3} × duration {permanent/long/medium/short/instantaneous} — a section is one `TimberRow` over one form/grade and its lamination build; a member is `SectionProfile.Rectangle`, a CLT panel `SectionProfile.Layered`, never a section subtype.
- Entry: `ComponentSeed.Rows(context, TimberSeed.Roster, TimberSeed.Law)` — this page states the roster and the policy, never the fold. `TimberSeed.Resolve(Component, Op) : Fin<TimberRow>` performs one typed lookup by the resolved component's `ComponentId` and faults if a foreign or unregistered timber component reaches design; section-map membership derives from the built profile's own topology rather than from the form's cross-ply flag read a second time.
- Packages: Rasm.Numerics (project — `PositiveMagnitude` every length column), Rasm.Domain (project — `Context`/`Op`), Rasm.Element (project — `MaterialId`, `EvidenceGrade`, `MaterialPropertySet` the `ToProperties` lowering mints), VividOrange.Materials (`LinearElasticOrthotropicMaterial`/`MaterialType.Timber` the along/across-grain law; `.api/api-vividorange-materials.md` — the EN factories are concrete/steel/rebar only, probe-confirmed, so NO timber factory exists and the `MaterialGrade` timber rows are the AUTHORED registered-class owner), UnitsNet (`Pressure.FromMegapascals` coercing the grade scalars at the orthotropic edge; `libs/dotnet/.api/api-unitsnet.md`), Rasm.Materials.Component (project — the parent `component#COMPONENT_OWNER` `Component`/`ComponentRow`/`ComponentFamily`/`ComponentFault`/`SectionProfile`/`Ply`/`IfcBinding`/`ComputedSection`/`ComponentStandard`/`ComponentAuthority`/`Coring`/`SeedJoin`, `component#MATERIAL_GRADE` `MaterialGrade`/`GradeProperties`, `component#COMPONENT_SEED` `SeedLaw`/`ComponentSeed`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new section is one `TimberRow` (its lamination build a `Seq<double>` column, its edgewise strength the declaration its own ETA prints); a new strength class one `MaterialGrade` timber row on `component#MATERIAL_GRADE`; a new product form one `TimberForm` row carrying its cross-ply flag, charring base, `γ_M`, and `k_h`/`k_cr`/`β_c`/`k_m` columns; a new service/duration point one `ServiceClass`/`LoadDuration` row — never a per-member type, never a hand-keyed strength literal, never a re-minted EC5 factor beside the form row.
- Boundary: the timber arm's columns and identity are `component#MATERIAL_GRADE`'s and its PHYSICS is this page's — `GRollMean` derives the EN 14080 rolling-shear modulus off `GMean`, `Hardwood` reads what the `K90Base` intercept says, and `OrthotropicLaw`/`ToProperties` lower the same scalars through `LinearElasticOrthotropicMaterial` and `MaterialPropertySet.OfOrthotropic`. The EDGEWISE bending strength is a PRODUCT fact, so it rides `TimberRow` where its ETA declares it and never the grade row it is not a column of: a lamella's flatwise `f_m,k` is measured across the layup's own thickness and an in-plane layup develops a different one, so the two never stand in for each other and a layup whose assessment prints no edgewise value answers absence. `SectionProfile.Layered` carries each physical ply thickness under the bounded `PlyRole.Longitudinal`/`PlyRole.Transverse` discriminant, and `ComponentFamily.Timber.Admits` rejects every other known `PlyRole`; the `ComponentFamily.Timber.Ifc` concrete leaf leaves beam/column/brace occurrence refinement outside Materials, `DetailLane.None` forbids a duplicate bag, and `TimberSeed.Resolve` restores the authored form/grade axes by the same `ComponentId` minted during seeding.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Numerics;
using Rasm.Domain;
using Rasm.Element.Composition;
using Rasm.Element.Properties;
using Thinktecture;
using UnitsNet;
using VividOrange.Materials;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Component;

// --- [TYPES] ---------------------------------------------------------------------------
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

    public double BetaN(GradeProperties.Timber grade) =>
        !grade.Hardwood ? BetaNSoftwoodMmPerMin
            : grade.DensityK >= 450.0 ? 0.55
            : grade.DensityK <= 290.0 ? 0.70
            : 0.70 - 0.15 * (grade.DensityK - 290.0) / 160.0;

    public double Kh(double depthMm) =>
        KhRefDepthMm > 0.0 && depthMm < KhRefDepthMm
            ? Math.Min(Math.Pow(KhRefDepthMm / depthMm, KhExponent), KhCap)
            : 1.0;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ServiceClass {
    public static readonly ServiceClass Sc1 = new("sc1", kdefSolid: 0.60, kdefCrossLam: Some(0.80));
    public static readonly ServiceClass Sc2 = new("sc2", kdefSolid: 0.80, kdefCrossLam: Some(1.00));
    public static readonly ServiceClass Sc3 = new("sc3", kdefSolid: 2.00, kdefCrossLam: Option<double>.None);
    public double KdefSolid { get; }
    public Option<double> KdefCrossLam { get; }

    public Option<double> KdefFor(TimberForm form) => form.CrossPly ? KdefCrossLam : Some(KdefSolid);
}

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

public partial record GradeProperties {
    public sealed partial record Timber {
        public double GRollMean => GMean * RollingShearRatio;
        const double RollingShearRatio = 0.10;

        public bool Hardwood => K90Base < 1.0;

        public LinearElasticOrthotropicMaterial OrthotropicLaw() =>
            new(MaterialType.Timber,
                Pressure.FromMegapascals(E0Mean),  Pressure.FromMegapascals(Fc0k),
                Pressure.FromMegapascals(E90Mean), Pressure.FromMegapascals(Fc90k),
                Pressure.FromMegapascals(E90Mean), Pressure.FromMegapascals(Fc90k));

        public Fin<Seq<MaterialPropertySet>> ToProperties(Op key) =>
            MaterialPropertySet.OfOrthotropic(DensityK, E0Mean, Some(E005), E90Mean, GMean, Fc0k, Fc90k, 5.0e-6, key)
                .Map(static orthotropic => Seq(orthotropic));
    }
}

public sealed partial class MaterialGrade {
    public Option<GradeProperties.Timber> TimberArm => Columns is GradeProperties.Timber arm ? Some(arm) : None;
}

public static class TimberPartialFactor {
    public const double Connection = 1.30;
}

public readonly record struct TimberRow(
    string Designation, TimberForm Form, MaterialGrade Grade, double WMm, double DMm, Seq<double> BuildMm,
    Option<double> FmEdgeKMpa = default);

// --- [POLICIES] ------------------------------------------------------------------------
public static class TimberSeed {
    public static readonly Seq<TimberRow> Roster = Seq(
        new TimberRow("timber.sawn-c16-38x89",       TimberForm.Sawn,   MaterialGrade.C16,    38.0,  89.0,  Seq(89.0)),
        new TimberRow("timber.sawn-c24-38x140",      TimberForm.Sawn,   MaterialGrade.C24,    38.0,  140.0, Seq(140.0)),
        new TimberRow("timber.sawn-c24-38x184",      TimberForm.Sawn,   MaterialGrade.C24,    38.0,  184.0, Seq(184.0)),
        new TimberRow("timber.sawn-c30-63x175",      TimberForm.Sawn,   MaterialGrade.C30,    63.0,  175.0, Seq(175.0)),
        new TimberRow("timber.sawn-d40-75x225",      TimberForm.Sawn,   MaterialGrade.D40,    75.0,  225.0, Seq(225.0)),
        new TimberRow("timber.glulam-gl24h-90x225",  TimberForm.Glulam, MaterialGrade.Gl24h,  90.0,  225.0, toSeq(Enumerable.Repeat(45.0, 5))),
        new TimberRow("timber.glulam-gl28h-90x270",  TimberForm.Glulam, MaterialGrade.Gl28h,  90.0,  270.0, toSeq(Enumerable.Repeat(45.0, 6))),
        new TimberRow("timber.glulam-gl30h-115x360", TimberForm.Glulam, MaterialGrade.Gl30h,  115.0, 360.0, toSeq(Enumerable.Repeat(45.0, 8))),
        new TimberRow("timber.glulam-gl32h-115x405", TimberForm.Glulam, MaterialGrade.Gl32h,  115.0, 405.0, toSeq(Enumerable.Repeat(45.0, 9))),
        new TimberRow("timber.glulam-gl28c-140x630", TimberForm.Glulam, MaterialGrade.Gl28c,  140.0, 630.0, toSeq(Enumerable.Repeat(45.0, 14))),
        new TimberRow("timber.lvl-lvl48p-75x300",    TimberForm.Lvl,    MaterialGrade.Lvl48p, 75.0,  300.0, toSeq(Enumerable.Repeat(3.0, 25))),
        new TimberRow("timber.clt-c24-3ply-90",      TimberForm.Clt,    MaterialGrade.C24,    1250.0, 90.0,  Seq(30.0, 30.0, 30.0), Some(CltEdgewiseC24Mpa)),
        new TimberRow("timber.clt-c24-5ply-150",     TimberForm.Clt,    MaterialGrade.C24,    1250.0, 150.0, Seq(30.0, 30.0, 30.0, 30.0, 30.0), Some(CltEdgewiseC24Mpa)),
        new TimberRow("timber.clt-c24-7ply-230",     TimberForm.Clt,    MaterialGrade.C24,    1250.0, 230.0, Seq(30.0, 34.0, 34.0, 34.0, 34.0, 34.0, 30.0), Some(CltEdgewiseC24Mpa)));

    const double CltEdgewiseC24Mpa = 24.0;

    public static readonly SeedLaw<TimberRow> Law = SeedLaw<TimberRow>.Of(
        family: ComponentFamily.Timber,
        designation: static row => row.Designation,
        coherence: Coherence,
        profile: ProfileOf,
        substance: static row => row.Grade.Substance,
        source: static _ => EvidenceGrade.Catalogue,
        standard: static row => new ComponentStandard(row.Grade.Authority.Region, StandardJointThicknessMm: 0.0, row.Grade.Authority),
        detail: Option<Func<TimberRow, SectionProfile, Op, Fin<PropertyBag>>>.None);

    static Validation<Error, Unit> Coherence(TimberRow row, Op key) =>
        (guard(row.Grade.Family == ComponentFamily.Timber,
             new ComponentFault.GradeFamilyMismatch(key, row.Grade, ComponentFamily.Timber)).ToValidation(),
         guard(row.Grade.TimberArm.IsSome,
             new ComponentFault.GradeBodyMissing(key, row.Grade, ComponentFamily.Timber)).ToValidation(),
         guard(row.Form.CrossPly
                 || Math.Abs(row.BuildMm.Sum() - row.DMm) <= BuildClosureMm
                 || Math.Abs(row.BuildMm.Sum() - row.WMm) <= BuildClosureMm,
             new KernelFault.InvalidValue(nameof(row.BuildMm), "a build closing on the form's declared axis", Some(key))).ToValidation(),
         guard(row.FmEdgeKMpa.ForAll(static edge => double.IsFinite(edge) && edge > 0.0) && (row.Form.CrossPly || row.FmEdgeKMpa.IsNone),
             new KernelFault.InvalidValue(nameof(row.FmEdgeKMpa), "edgewise strength only for cross-ply forms", Some(key))).ToValidation())
            .Apply(static (_, _, _, _) => unit).As();

    const double BuildClosureMm = 0.5;

    static Fin<SectionProfile> ProfileOf(TimberRow row, Op key) =>
        row.Form.CrossPly
            ? row.BuildMm.Map((mm, index) => (Mm: mm, Role: (index & 1) == 0 ? PlyRole.Longitudinal : PlyRole.Transverse))
                .Traverse(ply => key.AcceptValidated<PositiveMagnitude>(candidate: ply.Mm)
                    .Map(t => new Ply(row.Grade.Substance, t, ply.Role))).As()
                .Bind(plies => SectionProfile.Layered.Of(plies, overallMm: row.DMm, widthMm: row.WMm, key))
            : SectionProfile.Rectangle.Of(row.WMm, row.DMm, key);

    static readonly Lazy<Fin<FrozenDictionary<ComponentId, TimberRow>>> Table =
        SeedJoin.Of(Roster, static row => row.Designation);

    public static Fin<TimberRow> Resolve(Component component, Op key) =>
        SeedJoin.Resolve(Table, component.Designation, key);

    public static Fin<SectionCapacity> Capacity(Component component, Option<ComputedSection> section, CapacityPlacement placement, Op key) =>
        from row in Resolve(component, key)
        from lift in placement.FireExposure.Match(
            Some: exposure => TimberDesign
                .Fire(row, component.Profile, exposure.Value, FullyExposedFaces, placement.EffectiveLengthMm, key)
                .Map(capacity => CapacityLift.Fire(component.Designation, new FireState.Timber(capacity))),
            None: () =>
                from state in DesignState.Ambient(row.Form, placement.Service, placement.Duration, key)
                from capacity in TimberDesign.Capacity(row, component.Profile, section, state, placement.EffectiveLengthMm, key)
                select (CapacityLift)new CapacityLift.Timber(component.Designation, capacity))
        from capacity in SectionCapacity.Lift(lift, key)
        select capacity;

    const int FullyExposedFaces = 4;
}
```

## [03]-[TIMBER_CAPACITY]

- Owner: `TimberDesign` the EN 1995-1-1 design-code operations owner — `Capacity` the ONE design-resistance projection (member-vs-panel discriminated by the ADMITTED form/profile correspondence), `Fire` the EN 1995-1-2 accidental verdict over the charred geometry, `EffectiveStiffness` the EN 1995-1-1 Annex B gamma-method kernel over role-tagged plies, `ResidualSection` the reduced cross-section, `ResidualStack` the CLT step-charring ply modality; `DesignState` the (k_mod, γ_M) pair that makes the ambient and accidental situations two VALUES of one fold and carries the service-class permission law at its own mint; `TimberCapacity` the resistance set (FROZEN — the `capacity#SECTION_CAPACITY` `SectionCapacity.Lift(lift, key)` `Timber`/`TimberFire` arms lift it WHOLE onto `SectionCapacity.TimberMember`).
- Cases: one `TimberCapacity` across all forms — design bending `M_Rd,y` (member: `k_crit·k_h·k_mod·f_m,k·W_x/γ_M` over the RESOLVED `ComputedSection.SxMm3` with the §6.3.3 lateral-torsional `k_crit` band; panel: the gamma-method `W_eff = (EI)_eff/(E0·h/2)`), minor-axis bending `M_Rd,z` (member: `k_h(w)·k_mod·f_m,k·S_y/γ_M` over the resolved `SyMm3` — `k_h` over the WIDTH, no `k_crit`; panel: the NET-SECTION in-plane arm over the longitudinal plies alone, `k_mod·f_m,edge,k·t_net·h²/6/γ_M` over the product row's declared edgewise strength, answering 0.0 where the layup declares none so an in-plane `Mz` demand governs loud through the capacity `GuardedRatio` law), compression `N_Rd` (§6.3.2 `k_c`-reduced over `E005`; panel over the longitudinal net area and the effective radius `i_ef = √((EI)_eff/(E0·A_0))`), shear `V_Rd` (member `k_cr`-cracked longitudinal; panel rolling-shear `f_R,v,k`), perpendicular bearing `R_90,Rd` per unit bearing length, §6.1.8 torsion `T_Rd = k_shape·f_v,d·W_tor`, the governing `λ_rel`, the §6.1.6(2) per-form `k_m` weight, and the applied `k_mod` — a capacity is a derived projection over the resolved section or the ply stack, never a per-form check surface.
- Entry: `TimberDesign.Capacity(TimberRow row, SectionProfile, Option<ComputedSection>, DesignState, double effectiveLengthMm, Op key) : Fin<TimberCapacity>` takes the registered PRODUCT ROW rather than a loose (form, grade) pair — the row carries both together with the declared edgewise strength the panel arm needs, so no caller can hand the projection a form and a grade that were never seeded together — and admits a finite positive length before the member/panel dispatch. `TimberDesign.Fire(row, profile, exposureMinutes, exposedSides, effectiveLengthMm, key)` is the EN 1995-1-2 ACCIDENTAL verdict over that same fold at `DesignState.Fire`, charring the geometry first and pricing the residual unmodified — the `capacity#SECTION_CAPACITY` `CapacityLift.TimberFire` producer. `TimberDesign.ResidualSection(..., double exposureMinutes, int exposedSides, Op key) : Fin<ResidualSection>` admits a rectangular member, non-negative finite exposure, and one through four exposed faces; complete burn-through faults. `TimberDesign.ResidualStack(..., Op key) : Fin<Seq<Ply>>` admits only a cross-ply form, then folds the CLT char front through the ply stack and faults on invalid exposure or full burn-through.
- Packages: Rasm.Numerics (project — `PositiveMagnitude`), Rasm.Domain (project — `Op`), Rasm.Materials.Component (project — `ComputedSection`/`SectionProfile`/`Ply`/`ComponentFault`, `component#MATERIAL_GRADE` `GradeProperties.Timber`), Thinktecture.Runtime.Extensions, LanguageExt.Core; the EN 1995 rules HAND-ROLLED (no .NET EC5 package exists — the SAME hand-roll the steel AISC and RC EC2 checks take), every factor a `TimberForm` column, a `GradeProperties.Timber` column, or the product row's own declaration.
- Growth: a new design check is one `TimberCapacity` column with its arm (a notched-beam `k_v`, a load-sharing `k_sys`); a new fire route one `ResidualSection` parameter (a protected member's `t_ch` delay); a new form's factor set is its `TimberForm` row columns — never a per-form capacity surface, never a re-minted characteristic where the grade row carries it.
- Boundary: every design resistance derives from `f_k·k_mod/γ_M`; `TimberForm` owns `k_h`, `k_cr`, `β_c`, `γ_M`, and `k_m`, `GradeProperties.Timber` owns the characteristic vector, the product ROW owns the edgewise declaration, and `LoadDuration.KmodFor(service)` owns the duration/service joint. Members read the resolved `ComputedSection`, §6.3.3 lateral-torsional stability, weak-axis buckling, shear, per-unit-length bearing, and torsion; panels read the longitudinal plies for Annex B `(EI)_eff`, gamma-reduced buckling, rolling shear, and out-of-plane bending, the in-plane minor axis the net-section arm over those same longitudinal plies. `ResidualSection` and `ResidualStack` preserve the same geometry currencies under EN 1995-1-2 charring, and the `SectionCapacity.Lift(lift, key)` `Timber`/`TimberFire` arms carry the frozen set onto the unified demand pipeline.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct TimberCapacity(
    double BendingNmm,
    double BendingMinorNmm,
    double CompressionN,
    double ShearN,
    double BearingPerpNPerMm,
    double TorsionalNmm,
    double RelativeSlenderness,
    double Km,
    double Kmod);

public readonly record struct ResidualSection(PositiveMagnitude ResidualWidthMm, PositiveMagnitude ResidualDepthMm, double CharDepthMm);

public readonly record struct DesignState(double Kmod, double GammaM) {
    public static Fin<DesignState> Ambient(TimberForm form, ServiceClass service, LoadDuration duration, Op key) =>
        service.KdefFor(form)
            .ToFin(new ComponentFault.ServiceClassUnsupported(key, form, service))
            .Map(_ => new DesignState(duration.KmodFor(service), form.GammaM));

    public static readonly DesignState Fire = new(1.0, 1.0);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TimberDesign {
    public static Fin<TimberCapacity> Capacity(
        TimberRow row, SectionProfile profile, Option<ComputedSection> section,
        DesignState state, double effectiveLengthMm, Op key) =>
        from length in guard(double.IsFinite(effectiveLengthMm) && effectiveLengthMm > 0.0,
            new KernelFault.OutOfRange(nameof(effectiveLengthMm), effectiveLengthMm, "finite and positive", Some(key)))
        from grade in row.Grade.TimberArm.ToFin(new ComponentFault.GradeBodyMissing(key, row.Grade, ComponentFamily.Timber))
        from capacity in (profile, section.Case, row.Form.CrossPly) switch {
            (SectionProfile.Rectangle r, ComputedSection cs, false) => Fin.Succ(Member(row.Form, grade, r, cs, state, effectiveLengthMm)),
            (SectionProfile.Rectangle, null, false)                 => Fin.Fail<TimberCapacity>(new ComponentFault.SectionIncoherent(key, typeof(SectionProfile.Rectangle))),
            (SectionProfile.Layered l, null, true)                  => Fin.Succ(Panel(row, grade, l, state, effectiveLengthMm)),
            (SectionProfile.Layered, ComputedSection, true)         => Fin.Fail<TimberCapacity>(new ComponentFault.SectionIncoherent(key, typeof(SectionProfile.Layered))),
            _                                                       => Fin.Fail<TimberCapacity>(new ComponentFault.ProfileMismatch(key, ComponentFamily.Timber, profile.GetType())),
        }
        select capacity;

    public static Fin<TimberCapacity> Fire(TimberRow row, SectionProfile profile, double exposureMinutes, int exposedSides, double effectiveLengthMm, Op key) =>
        from grade in row.Grade.TimberArm.ToFin(new ComponentFault.GradeBodyMissing(key, row.Grade, ComponentFamily.Timber))
        from charred in profile switch {
            SectionProfile.Layered layered =>
                from plies in ResidualStack(row.Form, grade, layered.Plies, exposureMinutes, key)
                from stack in SectionProfile.Layered.Of(plies, overallMm: plies.Sum(static p => p.ThicknessMm.Value), widthMm: layered.WidthMm.Value, key)
                select (Profile: stack, Section: Option<ComputedSection>.None),
            _ =>
                from residual in ResidualSection(row.Form, grade, profile, exposureMinutes, exposedSides, key)
                from rectangle in SectionProfile.Rectangle.Of(residual.ResidualWidthMm.Value, residual.ResidualDepthMm.Value, key)
                from solved in SectionSolver.Solve(rectangle, key)
                select (Profile: rectangle, Section: Some(solved)),
        }
        from capacity in Capacity(row, charred.Profile, charred.Section, DesignState.Fire, effectiveLengthMm, key)
        select capacity;

    static TimberCapacity Member(TimberForm f, GradeProperties.Timber g, SectionProfile.Rectangle r, ComputedSection cs, DesignState state, double lengthMm) {
        double kmod = state.Kmod, gammaM = state.GammaM;
        double w = r.WidthMm.Value, d = r.DepthMm.Value, area = cs.AreaMm2.Value;
        (double kc, double lambdaRel) = BucklingKc(f, g, Slenderness(lengthMm, cs.GoverningRadiusMm));
        double sigmaMCrit = lengthMm > 0.0 ? 0.78 * w * w * g.E005 / (d * lengthMm) : double.PositiveInfinity;
        double lambdaRelM = double.IsPositiveInfinity(sigmaMCrit) ? 0.0 : Math.Sqrt(g.Fmk / sigmaMCrit);
        double kcrit = lambdaRelM <= 0.75 ? 1.0 : lambdaRelM <= 1.4 ? 1.56 - 0.75 * lambdaRelM : 1.0 / (lambdaRelM * lambdaRelM);
        return Resistances(g, kmod, gammaM,
            bendingNmm: kcrit * f.Kh(d) * kmod * g.Fmk * cs.SxMm3.Value / gammaM,
            bendingMinorNmm: f.Kh(w) * kmod * g.Fmk * cs.SyMm3.Value / gammaM,
            kc, area,
            shearN: kmod * g.Fvk * f.KcrCrack * area * 2.0 / 3.0 / gammaM,
            w, d, lambdaRel, km: f.Km);
    }

    static TimberCapacity Panel(TimberRow row, GradeProperties.Timber g, SectionProfile.Layered l, DesignState state, double lengthMm) {
        TimberForm f = row.Form;
        double kmod = state.Kmod, gammaM = state.GammaM;
        double b = l.WidthMm.Value, h = l.OverallMm.Value;
        double longitudinalThickness = l.Plies.Filter(static p => p.Role == PlyRole.Longitudinal).Sum(static p => p.ThicknessMm.Value);
        double areaLong = b * longitudinalThickness;
        double eiEff = EffectiveStiffness(l.Plies, g, b, lengthMm);
        double iEff = areaLong > 0.0 ? Math.Sqrt(eiEff / (g.E0Mean * areaLong)) : 0.0;
        (double kc, double lambdaRel) = BucklingKc(f, g, Slenderness(lengthMm, iEff));
        return Resistances(g, kmod, gammaM,
            bendingNmm: kmod * g.Fmk * (eiEff / (g.E0Mean * h * 0.5)) / gammaM,
            bendingMinorNmm: InPlaneBending(row, l, kmod, gammaM, longitudinalThickness),
            kc, areaLong,
            shearN: kmod * g.FRvk.IfNone(0.0) * areaLong / gammaM,
            b, h, lambdaRel, km: f.Km);
    }

    static double InPlaneBending(TimberRow row, SectionProfile.Layered l, double kmod, double gammaM, double netThicknessMm) =>
        row.FmEdgeKMpa.Match(
            Some: edge => kmod * edge * netThicknessMm * l.OverallMm.Value * l.OverallMm.Value / 6.0 / gammaM,
            None: static () => 0.0);

    static TimberCapacity Resistances(GradeProperties.Timber g, double kmod, double gammaM, double bendingNmm, double bendingMinorNmm, double kc, double areaMm2, double shearN, double wMm, double dMm, double lambdaRel, double km) {
        double bTor = Math.Min(wMm, dMm), hTor = Math.Max(wMm, dMm);
        double alpha = 1.0 / 3.0 - 0.21 * (bTor / hTor) * (1.0 - Math.Pow(bTor / hTor, 4.0) / 12.0);
        double fvd = kmod * g.Fvk / gammaM;
        return new TimberCapacity(
            BendingNmm: bendingNmm,
            BendingMinorNmm: bendingMinorNmm,
            CompressionN: kc * kmod * g.Fc0k * areaMm2 / gammaM,
            ShearN: shearN,
            BearingPerpNPerMm: kmod * g.Fc90k * wMm / gammaM,
            TorsionalNmm: Math.Min(1.0 + 0.15 * hTor / bTor, 2.0) * fvd * alpha * hTor * bTor * bTor,
            RelativeSlenderness: lambdaRel,
            Km: km,
            Kmod: kmod);
    }

    static (double Kc, double LambdaRel) BucklingKc(TimberForm f, GradeProperties.Timber g, double lambda) {
        double sigmaCrit = lambda > 0.0 ? Math.PI * Math.PI * g.E005 / (lambda * lambda) : double.PositiveInfinity;
        double lambdaRel = double.IsPositiveInfinity(sigmaCrit) ? 0.0 : Math.Sqrt(g.Fc0k / sigmaCrit);
        double k = 0.5 * (1.0 + f.BetaC * (lambdaRel - 0.3) + lambdaRel * lambdaRel);
        return (lambdaRel <= 0.3 ? 1.0 : 1.0 / (k + Math.Sqrt(Math.Max(0.0, k * k - lambdaRel * lambdaRel))), lambdaRel);
    }

    static double Slenderness(double lengthMm, double radiusMm) =>
        lengthMm > 0.0 && radiusMm > 0.0 ? lengthMm / radiusMm : 0.0;

    public static double EffectiveStiffness(Seq<Ply> plies, GradeProperties.Timber grade, double widthMm, double referenceSpanMm) {
        double depth = plies.Sum(static p => p.ThicknessMm.Value), half = depth * 0.5, e0 = grade.E0Mean;
        double span2 = referenceSpanMm > 0.0 ? referenceSpanMm * referenceSpanMm : 0.0;
        Seq<(double T, double Z, PlyRole Role, int Index)> placed = plies.Fold(
            (Offset: 0.0, Rows: Seq<(double T, double Z, PlyRole Role, int Index)>()),
            (acc, p) => (acc.Offset + p.ThicknessMm.Value,
                acc.Rows.Add((p.ThicknessMm.Value, acc.Offset + p.ThicknessMm.Value * 0.5 - half, p.Role, acc.Rows.Count)))).Rows;
        return placed.Filter(static row => row.Role == PlyRole.Longitudinal).Sum(row => {
            double own = e0 * widthMm * row.T * row.T * row.T / 12.0;
            int crossAt = row.Z <= 0.0 ? row.Index + 1 : row.Index - 1;
            double tCross = placed
                .Filter(other => other.Index == crossAt && other.Role == PlyRole.Transverse)
                .Map(static other => other.T).Head.IfNone(0.0);
            double gamma = tCross > 0.0
                ? 1.0 / (1.0 + Math.PI * Math.PI * e0 * row.T * tCross / (span2 * grade.GRollMean))
                : 1.0;
            return own + gamma * e0 * widthMm * row.T * row.Z * row.Z;
        });
    }

    public static Fin<ResidualSection> ResidualSection(TimberForm form, GradeProperties.Timber grade, SectionProfile profile, double exposureMinutes, int exposedSides, Op key) {
        if (!double.IsFinite(exposureMinutes) || exposureMinutes < 0.0) {
            return new KernelFault.OutOfRange(nameof(exposureMinutes), exposureMinutes, "finite and non-negative", Some(key));
        }
        if (exposedSides is < 1 or > 4) {
            return new KernelFault.OutOfRange(nameof(exposedSides), exposedSides, "inside [1, 4]", Some(key));
        }
        if (profile is not SectionProfile.Rectangle) {
            return new ComponentFault.ProfileMismatch(key, ComponentFamily.Timber, profile.GetType());
        }
        double t = exposureMinutes;
        double charMm = form.BetaN(grade) * t + 7.0 * Math.Min(t / 20.0, 1.0);
        (double w, double d) = (profile.GrossRectangleMm.WidthMm.Value, profile.GrossRectangleMm.DepthMm.Value);
        double residW = w - Math.Min(2, exposedSides - 1) * charMm;
        double residD = d - (1 + (exposedSides == 4 ? 1 : 0)) * charMm;
        return residW > 0.0 && residD > 0.0
            ? Fin.Succ(new ResidualSection(PositiveMagnitude.Create(residW), PositiveMagnitude.Create(residD), charMm))
            : new ComponentFault.FireResistanceExhausted(key, ComponentFamily.Timber, exposureMinutes);
    }

    public static Fin<Seq<Ply>> ResidualStack(TimberForm form, GradeProperties.Timber grade, Seq<Ply> plies, double exposureMinutes, Op key) {
        if (!double.IsFinite(exposureMinutes) || exposureMinutes < 0.0) {
            return new KernelFault.OutOfRange(nameof(exposureMinutes), exposureMinutes, "finite and non-negative", Some(key));
        }
        if (plies.IsEmpty) {
            return new KernelFault.InvalidValue(nameof(plies), "a non-empty layered section", Some(key));
        }
        if (!form.CrossPly) {
            return new KernelFault.InvalidValue(nameof(form), "a cross-ply form for layered fire resistance", Some(key));
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
            ? Fin.Fail<Seq<Ply>>(new ComponentFault.FireResistanceExhausted(key, ComponentFamily.Timber, exposureMinutes))
            : Fin.Succ(residual);
    }

    static Seq<Ply> Trim(Seq<Ply> plies, double depthMm) =>
        plies.Fold((Cut: depthMm, Remainder: Seq<Ply>()), static (acc, p) =>
            acc.Cut >= p.ThicknessMm.Value ? (acc.Cut - p.ThicknessMm.Value, acc.Remainder)
            : acc.Cut > 0.0 ? (0.0, acc.Remainder.Add(new Ply(p.Material, PositiveMagnitude.Create(p.ThicknessMm.Value - acc.Cut), p.Role)))
            : (0.0, acc.Remainder.Add(p))).Remainder;
}
```

## [04]-[RESEARCH]

(none)
