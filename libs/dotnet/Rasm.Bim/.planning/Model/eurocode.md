# [EUROCODE_ALGEBRA]

`Rasm.Bim` owns the EN 1990 action-classification and combination algebra the structural reader resolves its factor and combination rows under: `AnnexRegime` the national design regime this layer alone bridges a project nation onto, `EurocodePolicy` the regime VALUE a composition elects, `EurocodeAction` the psi mint per action nature, `ActionRow` the two-tier classification the `Model/structural#STRUCTURAL_PROJECTION` load leg stamps, and `Eurocode` the fold that lowers an `IfcStructuralLoadGroup` onto the package-generated combination roster. The `VividOrange.Countries` nation taxonomy and the `NationalAnnex` dispatch key share no compiled member and meet at this design layer, so `AnnexRegime` holds the correspondence keyed on the ISO alpha-2 code, carrying the SAF design code as its own row key. Every gamma, psi, and combination expression is the `VividOrange.*` train's own — this owner elects and composes, and a second clause algebra beside the package that ships Annex A1 diverges on the first national deviation.

The reader supplies the entity reach and the ONE magnitude admission: components enter as the `Model/structural#STRUCTURAL_PROJECTION` `LoadFamily` row's own projection, cross that page's `Admit`, and re-enter it as factored design actions under the IDENTITY unit regime. Faults rail `Model/faults#FAULT_BAND` `BimFault` through `BimFault.Refused` with `BimReason.Capability`, the ONE seam where the package's uncovered-annex throws cross.

## [01]-[INDEX]

- [02]-[EUROCODE_ALGEBRA]: `AnnexRegime` the ISO-keyed national design-regime roster (nation, `NationalAnnex`, SAF design code as the key), `EurocodePolicy` over it and its `CombinationSet`/`SnowAltitude`/`PermanentSense` axes, `EurocodeAction` and `ActionRow` with the `CaseSources` table, `SweepKind`/`PsiRow`/`GammaRow`, and `Eurocode` — the two-tier derivation, factor stamp, combination composition, and typed-carrier lowering.

## [02]-[EUROCODE_ALGEBRA]

- Owner: `AnnexRegime` the `[SmartEnum<string>]` national design-regime roster — one row per `NationalAnnex` carrying the ISO 3166-1 alpha-2 the project nation is admitted by and, as the row's own KEY, the SAF `ExcelNationalCode` member the XLSX boundary writes, with `Of(ICountry)` and `Of(NationalAnnex)` the two shape-discriminated reads over its nested `FrozenDictionary` indexes; `EurocodePolicy` the Eurocode regime as ONE value — the `AnnexRegime` row every psi lookup keys through, the `IDesignSituation` the composition elected, the `CombinationSet` ULS axis, the seismic importance factor, the imposed-load category, the snow altitude band, and the permanent-action sense; `EurocodeAction` the `[SmartEnum<string>]` binding each action nature to the ONE `ENLoadCaseFactory` mint that answers both its psi set and its combination payload; `ActionRow` the resolved classification carrying the consumer-neutral case token beside the EN 1990 `ActionClass` nature; `CombinationSet` the ULS-set row carrying its own roster assembly; `PsiRow`/`GammaRow` the factor rows each carrying its own reader off the case and the situation; `Eurocode` the fold.
- Law: `IDesignSituation` IS the package's own partial-factor policy contract and the ONLY public reach to the EN 1990 Table A1.2 gammas — every `ITableA1_2` implementation is INTERNAL to `VividOrange.Cases` and no public member yields one, so an `ITableA1_2` column on the policy names a value no caller constructs and strands the whole factor composition it fronts. The elected situation therefore carries the WHOLE set, and a two-factor slice stranding the leading-action and 6.10a-b reduction factors on every consumer is the deleted form.
- Exemption: `CreateEquSetA` (both overloads) carries an unconditional `item2[1]`/`list[2]` tail after its leading-variable loop — `ArgumentOutOfRangeException` below two variable cases, a silent overwrite of the third combination above — so no input shape survives it intact; `CreateAccidental` is index-sound but takes the leading AEd `IVariableCase` as a REQUIRED argument this fold never holds, because the IFC action vocabulary classifies no accidental design action and the verb dereferences `accidentalCase.Name` unconditionally. The EQU and accidental rosters therefore CONSTRUCT the package's own combination records through `Sweep` under the elected situation — construction only, every gamma and psi still factoring inside `GetFactoredLoads` — and a fence re-electing either verb on the inputs this fold holds is the refuted form.
- Entry: `Eurocode.RowOf(activity, policy)` walks the activity's load-group assignment and resolves its `ActionRow` two-tier; `Eurocode.Factors(row, policy, scale, key)` stamps the psi triple beside the situation's whole partial-factor set; `Eurocode.Combination(group, policy, scale, key)` lowers a `LOAD_COMBINATION_GROUP` onto the combination `Definition` expressions beside the factored design actions they produce, in ONE combination order both rows share.
- Auto: the `ActionRow` derivation is TWO-TIER — the specific `CaseSources` row over `IfcActionSourceTypeEnum`, else the group's `ActionType` nature with `PERMANENT_G` the dead permanent action and every other nature the imposed variable one — so a prestress, shrinkage, or settlement group factors permanent instead of silently mis-casing variable. An action with no mint carries no case and therefore no psi, the honest reading for a source the code does not classify, and the imposed mint is category-keyed so a psi row is ABSENT rather than defaulted onto whichever Category A-H the reader picked.
- Receipt: the factor rows land as dimensionless `Measure` values beside `Coefficient` on the load-group or activity bag, so one consumer read covers every factor; the combination roster lands as the paired `Combinations` and `FactoredActions` lists, so a consumer reads the k-th definition against the actions it produced.
- Packages: VividOrange.Cases, VividOrange.Countries, VividOrange.Loads, VividOrange.Standards.Eurocode, GeometryGymIFC_Core, UnitsNet, Rasm.Element, Rasm, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: a new Eurocode action is one `EurocodeAction` row carrying its factory mint; a new action-source classification is one `CaseSources` row, the nature tier already totalizing the residue; a new ULS set is one `CombinationSet` row carrying its own assembly, never a branch inside `Elect`; a new national deviation is an `AnnexRegime` row carrying its nation code, its `NationalAnnex`, and its SAF key together, never a per-country arm and never a second annex-to-code table; a new hand-assembled roster is one `SweepKind` row; a new partial factor the contract publishes is one `GammaRow` carrying its own reader.
- Boundary: the policy is REQUIRED at the reader's entry so every caller states its regime — a defaulted parameter letting every landed caller elect nothing left the whole `VividOrange.Loads`/`Cases` composition unreachable — and an ABSENT policy emits no factor row at all rather than a fabricated `NationalAnnex.RecommendedValues` factor; the nation-to-annex bridge is `AnnexRegime.Of(ICountry)` over the ISO alpha-2 `CountryCode` and a hand country-code switch, a `Country`-to-`NationalAnnex` map minted beside this roster, and a match on `ICountry.Name` are each the deleted form — the name equality the catalogues describe the bridge by misses `BosniaAndHerzegovinia` and `NorthMacedonia` and needs a space strip on every multiword nation, so it seeded the roster and never resolves it — while a nation with no annex lands `AnnexRegime.Recommended` rather than faulting, because `NationalAnnex.RecommendedValues` IS the regime a non-CEN nation designs under; the SAF design code is the `AnnexRegime` row's own KEY and a second annex-to-code correspondence at the wire boundary is the deleted form, `NationalAnnexUtility.GetAbbreviation` being both internal and unsound as a stem (`Ткп`/`БДС`/`ΕΛΟΤ` are non-Latin, `ILNAS` is not `LU`, and Singapore and Sweden collide on `SS`); the typed carrier is the payload the algebra folds `ILoad.Factor(Ratio)` across, so a hand-multiplied partial or combination factor beside it is the deleted form; carrier components mint FROM already-coerced SI magnitudes and read back through the typed SI accessors, so the lane reaches neither `ToUnit(UnitSystem.SI)` — which throws for every quantity whose SI unit-info walk is empty — nor `QuantityTypeConverter`, whose culture-formatted abbreviation wire is incommensurable with the seam's canonical byte run; the factored actions re-admit under `UnitScheme.Si` because the ingest already coerced them, and threading the model's regime there prices every design action by its length factor a second time; the psi mint and the combination mint are the TWO throwing seams and both cross as `BimFault.Refused` with `BimReason.Capability` through `BimFault.Refused` with `BimReason.Capability`, never propagating into the fold; EN 1998 spectra and behaviour factors have no producer in the package train and stay authored upstream of the load roster the seismic mint receives.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Domain;
using Rasm.Element.Properties;
using Thinktecture;
using UnitsNet;
using VividOrange.Countries;                          // ICountry — the ISO 3166-1 nation the AnnexRegime roster admits on
using VividOrange.Loads;
using VividOrange.Loads.Cases;
using VividOrange.Loads.Combinations;
using VividOrange.Standards.Eurocode;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Model;

// --- [TYPES] ------------------------------------------------------------------------------
// The national design regime as ONE row per Eurocode national annex: the ISO 3166-1 alpha-2 the project nation
// is admitted by, the NationalAnnex every psi and gamma table keys on, and — as the row's own KEY — the SAF
// ExcelNationalCode member the XLSX boundary writes, so the interchange correspondence DERIVES exactly as
// CombinationSet's key derives ExcelLoadCaseCombinationStandard instead of standing a second table.
// The bridge is the ISO CODE, not the name the catalogues describe it by. ICountry publishes Name and
// CountryCode and nothing else, and name equality misses two of the thirty-six nations: the package spells the
// annex BosniaAndHerzegovinia against the nation "Bosnia and Herzegovina", and NorthMacedonia against
// "Macedonia", while every multiword nation ("Czech Republic", "United Kingdom") needs a space strip before the
// compare. The alpha-2 code is unique across all 249 nations and carries neither divergence, so name equality is
// what SEEDED this roster at design time and never what resolves it at run time.
// The SAF column is DECLARED for the same reason deriving it is unsound: NationalAnnexUtility.GetAbbreviation is
// internal to VividOrange.Standards, and its values do not stem the SAF token even where reachable — Belarus
// reads "Ткп" against EC_TKP_EN, Bulgaria "БДС" against EC_BDS_EN, Greece "ΕΛΟΤ" against EC_ELOT_EN, Luxembourg
// "ILNAS" against EC_LU_EN, and Singapore and Sweden read "SS" and "BFS" where SAF splits EC_SS_EN_SG from
// EC_SS_EN_SE. NAMED LOSS: four ExcelNationalCode members — EC_MS_EN (Malaysia), IBC, NBR, SIA_26x — name design
// regimes the closed NationalAnnex enum never declares, so they are unreachable from this axis and a model
// checked to one of them carries no annex and elects nothing at the SAF boundary. WITNESS: every one of the
// thirty-seven annexes holds exactly one EC_* member, RecommendedValues included, so the annex axis is TOTAL
// onto the SAF one and the loss is entirely on the SAF side.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AnnexRegime {
    // The non-CEN fallback, the ONE row belonging to no nation: a nation with no national annex designs to the
    // EN recommended values, which is exactly what SAF's own EC_Standard_EN names. A country miss lands HERE.
    public static readonly AnnexRegime Recommended = new("EC_Standard_EN", "",   NationalAnnex.RecommendedValues);
    public static readonly AnnexRegime Austria     = new("EC_ONORM_EN",    "AT", NationalAnnex.Austria);
    public static readonly AnnexRegime Belarus     = new("EC_TKP_EN",      "BY", NationalAnnex.Belarus);
    public static readonly AnnexRegime Belgium     = new("EC_NBN_EN",      "BE", NationalAnnex.Belgium);
    public static readonly AnnexRegime Bosnia      = new("EC_BAS_EN",      "BA", NationalAnnex.BosniaAndHerzegovinia);
    public static readonly AnnexRegime Bulgaria    = new("EC_BDS_EN",      "BG", NationalAnnex.Bulgaria);
    public static readonly AnnexRegime Cyprus      = new("EC_CYS_EN",      "CY", NationalAnnex.Cyprus);
    public static readonly AnnexRegime Czechia     = new("EC_CSN_EN",      "CZ", NationalAnnex.CzechRepublic);
    public static readonly AnnexRegime Denmark     = new("EC_DS_EN",       "DK", NationalAnnex.Denmark);
    public static readonly AnnexRegime Estonia     = new("EC_EVS_EN",      "EE", NationalAnnex.Estonia);
    public static readonly AnnexRegime Finland     = new("EC_SFS_EN",      "FI", NationalAnnex.Finland);
    public static readonly AnnexRegime France      = new("EC_NF_EN",       "FR", NationalAnnex.France);
    public static readonly AnnexRegime Germany     = new("EC_DIN_EN",      "DE", NationalAnnex.Germany);
    public static readonly AnnexRegime Greece      = new("EC_ELOT_EN",     "GR", NationalAnnex.Greece);
    public static readonly AnnexRegime Hungary     = new("EC_MSZ_EN",      "HU", NationalAnnex.Hungary);
    public static readonly AnnexRegime Iceland     = new("EC_IST_EN",      "IS", NationalAnnex.Iceland);
    public static readonly AnnexRegime Ireland     = new("EC_IS_EN",       "IE", NationalAnnex.Ireland);
    public static readonly AnnexRegime Italy       = new("EC_UNI_EN",      "IT", NationalAnnex.Italy);
    public static readonly AnnexRegime Latvia      = new("EC_LVS_EN",      "LV", NationalAnnex.Latvia);
    public static readonly AnnexRegime Lithuania   = new("EC_LST_EN",      "LT", NationalAnnex.Lithuania);
    public static readonly AnnexRegime Luxembourg  = new("EC_LU_EN",       "LU", NationalAnnex.Luxembourg);
    public static readonly AnnexRegime Malta       = new("EC_MSA_EN",      "MT", NationalAnnex.Malta);
    public static readonly AnnexRegime Netherlands = new("EC_NEN_EN",      "NL", NationalAnnex.Netherlands);
    public static readonly AnnexRegime Macedonia   = new("EC_MKC_EN",      "MK", NationalAnnex.NorthMacedonia);
    public static readonly AnnexRegime Norway      = new("EC_NS_EN",       "NO", NationalAnnex.Norway);
    public static readonly AnnexRegime Poland      = new("EC_PN_EN",       "PL", NationalAnnex.Poland);
    public static readonly AnnexRegime Portugal    = new("EC_NP_EN",       "PT", NationalAnnex.Portugal);
    public static readonly AnnexRegime Romania     = new("EC_SR_EN",       "RO", NationalAnnex.Romania);
    public static readonly AnnexRegime Serbia      = new("EC_SRPS_EN",     "RS", NationalAnnex.Serbia);
    public static readonly AnnexRegime Singapore   = new("EC_SS_EN_SG",    "SG", NationalAnnex.Singapore);
    public static readonly AnnexRegime Slovakia    = new("EC_STN_EN",      "SK", NationalAnnex.Slovakia);
    public static readonly AnnexRegime Slovenia    = new("EC_SIST_EN",     "SI", NationalAnnex.Slovenia);
    public static readonly AnnexRegime Spain       = new("EC_UNE_EN",      "ES", NationalAnnex.Spain);
    public static readonly AnnexRegime Sweden      = new("EC_SS_EN_SE",    "SE", NationalAnnex.Sweden);
    public static readonly AnnexRegime Switzerland = new("EC_SN_EN",       "CH", NationalAnnex.Switzerland);
    public static readonly AnnexRegime Turkey      = new("EC_TS_EN",       "TR", NationalAnnex.Turkey);
    public static readonly AnnexRegime Kingdom     = new("EC_BS_EN",       "GB", NationalAnnex.UnitedKingdom);

    public string Iso { get; }
    public NationalAnnex Annex { get; }

    // The project nation admitted onto its Eurocode regime — the ONE bridge the design layer owes, and the sole
    // reach this branch takes into the nation taxonomy beside IGovernance.Country. A nation off the roster (212
    // of the 249 ISO members declare no national annex) lands Recommended, the documented EN fallback, never a
    // fault: designing to the recommended values IS the answer for a non-CEN nation, and a Fin here would make
    // every project outside the CEN membership state a recovery for a regime the code already defines.
    public static AnnexRegime Of(ICountry country) =>
        Lookup.ByIso.TryGetValue(country.CountryCode, out AnnexRegime? row) && row is { } hit ? hit : Recommended;

    // The same read off an already-elected annex, so a boundary holding only the policy reaches the design code
    // without carrying the nation; an out-of-domain enum cast lands Recommended exactly as an unrostered nation
    // does, because the SAF cell for an unnameable regime is the EN one, never a throw inside a wire fold.
    public static AnnexRegime Of(NationalAnnex annex) =>
        Lookup.ByAnnex.TryGetValue(annex, out AnnexRegime? row) && row is { } hit ? hit : Recommended;

    // Both indexes derive from the ONE roster, so a new annex row lands both reads with no hand-kept mirror. They
    // ride a NESTED holder because a static field initializer in this partial races the generated Items roster's
    // own initializer across the source-generated partial, and a null roster there would freeze both maps empty.
    private static class Lookup {
        internal static readonly FrozenDictionary<string, AnnexRegime> ByIso = Items
            .Where(static row => row.Iso.Length > 0)
            .ToFrozenDictionary(static row => row.Iso, StringComparer.Ordinal);

        internal static readonly FrozenDictionary<NationalAnnex, AnnexRegime> ByAnnex =
            Items.ToFrozenDictionary(static row => row.Annex);
    }
}

// The EN 1990 Annex A1 ULS combination-set election, each row carrying its OWN roster assembly. The situation
// CLASS cannot carry it — the package's DesignSituationClass ladder is persistent/transient/accidental/seismic
// and names no equilibrium or geotechnical member — so the set is its own policy axis, and the key IS the SAF
// ExcelLoadCaseCombinationStandard token so the interchange correspondence derives instead of being tabulated.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CombinationSet {
    public static readonly CombinationSet SetA = new("EnUlsSetA", static (cases, policy) =>
        Eurocode.Sweep(cases, policy, SweepKind.Equilibrium));
    public static readonly CombinationSet SetB = new("EnUlsSetB", static (cases, policy) =>
        ENCombinationFactory.CreateStrGeoSetB(Eurocode.All(cases), policy.Annex, use6_10aAnd6_10b: true,
            Eurocode.ComboPrefix, 1).Cast<ILoadCombination>().ToList());
    public static readonly CombinationSet SetC = new("EnUlsSetC", static (cases, policy) =>
        ENCombinationFactory.CreateStrGeoSetC(Eurocode.All(cases), policy.Annex,
            Eurocode.ComboPrefix, 1).Cast<ILoadCombination>().ToList());

    [UseDelegateFromConstructor]
    public partial IList<ILoadCombination> Assemble(IList<(ActionRow Row, ILoadCase Case)> cases, EurocodePolicy policy);
}

// The snow-altitude discriminant as a BAND: EN 1991-1-3 keys its psi set on a threshold, so the next threshold
// lands as a row. The bool column is the package verb's own argument shape and lives at that boundary alone.
[SmartEnum<string>]
public sealed partial class SnowAltitude {
    public static readonly SnowAltitude Below1000m = new("below-1000m", above: false);
    public static readonly SnowAltitude Above1000m = new("above-1000m", above: true);

    public bool Above { get; }
}

// Which sense the permanent actions take in a hand-assembled sweep: EN 1990 factors an unfavourable permanent
// at gammaG,sup and a favourable one at gammaG,inf, and the IFC action vocabulary classifies neither. The
// composition that knows whether the check is a strength or an uplift check states it, exactly as it states
// the annex. NAMED LOSS: EN 1990 permits a PER-CASE election and this policy elects one sense per model; a
// per-case election lands as one ActionRow column the moment a source vocabulary carries the fact.
[SmartEnum<string>]
public sealed partial class PermanentSense {
    public static readonly PermanentSense Unfavourable = new("unfavourable", favours: false);
    public static readonly PermanentSense Favourable = new("favourable", favours: true);

    public bool Favours { get; }
}

// Which EN 1990 Annex A1.1 action a source classifies as, and the ONE case mint answering both its psi factors
// and its combination payload. Each Create* verb takes the case's own load roster, so ONE mint serves the
// factor read (an empty roster) and the combination fold (the activities' typed carriers): the case arrives
// with its psi set off the package's Table A1.1 singletons AND its loads in one call, never a construction
// followed by a mutation pass, and this owner never tabulates a psi beside the package that owns the table.
[SmartEnum<string>]
public sealed partial class EurocodeAction {
    public static readonly EurocodeAction Imposed = new("imposed", static (policy, loads) =>
        policy.Imposed.Map(category => (IVariableCase)ENLoadCaseFactory.CreateImposed(loads, category, policy.Annex)));
    // ENLoadCaseFactory ships NO seismic verb (and no fatigue surface anywhere in the train), so the seismic
    // CASE hand-mints the package's own VariableCase: the zero psi triple is the Annex A1.1 statement that an
    // AEd never rides as an accompanying action — the table carries no psi column for it — while the
    // COMBINATION side is the LIVE ENCombinationFactory.CreateSeismic Eq 6.12a/b roster Elect composes.
    public static readonly EurocodeAction Seismic = new("seismic", static (policy, loads) =>
        Some((IVariableCase)new VariableCase {
            Name = "seismic", Loads = loads,
            CombinationFactor = Ratio.FromDecimalFractions(0),
            FrequentFactor = Ratio.FromDecimalFractions(0),
            QuasiPermanentFactor = Ratio.FromDecimalFractions(0),
        }));
    public static readonly EurocodeAction Snow = new("snow", static (policy, loads) =>
        Some((IVariableCase)ENLoadCaseFactory.CreateSnow(loads, policy.Annex, policy.Snow.Above)));
    public static readonly EurocodeAction Thermal = new("thermal", static (policy, loads) =>
        Some((IVariableCase)ENLoadCaseFactory.CreateThermal(loads, policy.Annex)));
    public static readonly EurocodeAction Wind = new("wind", static (policy, loads) =>
        Some((IVariableCase)ENLoadCaseFactory.CreateWind(loads, policy.Annex)));

    [UseDelegateFromConstructor]
    public partial Option<IVariableCase> Mint(EurocodePolicy policy, IList<ILoad> loads);
}

// Which combination record a hand-assembled sweep constructs, as a ROW: the flag that selected two bodies IS
// the row, so the accidental election of the frequent factor on the main accompanying variable (EN 1990 6.11b)
// travels with the arm that makes it rather than riding a boolean parameter beside both.
[SmartEnum<string>]
internal sealed partial class SweepKind {
    internal static readonly SweepKind Equilibrium = new("equilibrium", static (situation, main, rest) =>
        (LoadCombination)new EquilibriumCombination {
            DesignSituation = situation, LeadingVariableCases = main, AccompanyingVariableCases = rest,
        });
    internal static readonly SweepKind Accidental = new("accidental", static (situation, main, rest) =>
        (LoadCombination)new AccidentalCombination {
            DesignSituation = situation, MainAccompanyingVariableCases = main, OtherAccompanyingVariableCases = rest,
            UseFrequentCombinationFactorForMainAccompanying = true,
        });

    [UseDelegateFromConstructor]
    internal partial LoadCombination Assemble(IDesignSituation situation, IList<IVariableCase> main, IList<IVariableCase> rest);
}

// The three combination factors, each row carrying its OWN reader off the minted case: the retired positional
// zip against a hand-ordered triple paired Psi1 with the quasi-permanent factor on the first reorder.
[SmartEnum<string>]
internal sealed partial class PsiRow {
    internal static readonly PsiRow Psi0 = new("Psi0", static variable => variable.CombinationFactor);
    internal static readonly PsiRow Psi1 = new("Psi1", static variable => variable.FrequentFactor);
    internal static readonly PsiRow Psi2 = new("Psi2", static variable => variable.QuasiPermanentFactor);

    [UseDelegateFromConstructor]
    internal partial Ratio Read(IVariableCase variable);

    internal PropertyName Name => PropertyCategory.Seam.Row(Key);
}

// The elected situation's gamma columns, read as DECLARED decimal fractions so no Ratio unwrap sits between
// the policy and the row. GammaQ1 reads Option-valued because the EN leading-action sweep varies it per
// combination and the contract publishes it nullable — the absence is a COLUMN, not a special-cased tail.
[SmartEnum<string>]
internal sealed partial class GammaRow {
    internal static readonly GammaRow GammaSup = new("GammaSup",
        static situation => Some(situation.UnfavourablePermanentActionsPartialFactor));
    internal static readonly GammaRow GammaInf = new("GammaInf",
        static situation => Some(situation.FavourablePermanentActionsPartialFactor));
    internal static readonly GammaRow GammaQ1 = new("GammaQ1",
        static situation => Optional(situation.LeadingActionPartialFactor));
    internal static readonly GammaRow GammaQi = new("GammaQi",
        static situation => Some(situation.OtherAccompanyingVariableActionsPartialFactor));
    internal static readonly GammaRow Xi = new("Xi", static situation => Some(situation.ReductionFactor));

    [UseDelegateFromConstructor]
    internal partial Option<double> Read(IDesignSituation situation);

    internal PropertyName Name => PropertyCategory.Seam.Row(Key);
}

// --- [MODELS] -----------------------------------------------------------------------------
// The regime column is the AnnexRegime ROW, not the bare NationalAnnex it used to carry: the annex the package's
// tables key on, the ISO code the nation was admitted by, and the SAF design code the workbook writes are ONE
// fact and travel as one value, so the Exchange/saf#SAF_EXCHANGE lowering reaches the design code off the policy
// it already holds instead of a second election beside it. Annex derives, so every package call keeps reading
// policy.Annex and the row stays the single authority. A ForCountry factory forwarding six unchanged arguments
// is the entrypoint sibling the polymorphism law deletes — the nation is admitted at AnnexRegime.Of(country) and
// the existing construction path composes it in one hop.
public readonly record struct EurocodePolicy(
    AnnexRegime Regime, IDesignSituation Situation, CombinationSet Set, Ratio Importance,
    Option<ImposedLoadCategory> Imposed, SnowAltitude Snow, PermanentSense Sense) {
    public NationalAnnex Annex => Regime.Annex;
}

// One resolved action row: the consumer-neutral case token, the EN 1990 nature the combination algebra factors
// under, the imposed category a Category A-H action carries, and the psi mint. It replaces the bare token map
// that stranded every consumer re-deriving the nature it already knew and left the code factors unreachable.
internal readonly record struct ActionRow(
    string Case, ActionClass Class, Option<ImposedLoadCategory> Imposed, Option<EurocodeAction> Action);

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class Eurocode {
    internal const string ComboPrefix = "CO";

    private static readonly PropertyName Situation = PropertyCategory.Seam.Row("DesignSituation");
    private static readonly PropertyName Combinations = PropertyCategory.Seam.Row("Combinations");
    private static readonly PropertyName FactoredActions = PropertyCategory.Seam.Row("FactoredActions");

    // The EN 1990 design-situation vocabulary derives from the package's own enum, so the ladder and its
    // PersistentAndTransient pairing stay the package's to widen.
    private static readonly Seq<string> SituationKinds = toSeq(Enum.GetNames<DesignSituationClass>());

    // --- [CLASSIFICATION]

    // The SPECIFIC tier of the two-tier load-CASE derivation: the named permanent, climatic, and seismic
    // sources resolve to a ROW carrying the consumer's closed token, the EN 1990 nature the combination algebra
    // factors under, the category where the action is category-keyed, and the psi mint. The permanent-nature
    // sources land dead + Permanent so they factor as permanent actions; a source with no row falls to the
    // NATURE tier, so the residue (temperature, fire, impact, wave, brakes) is the imposed variable action
    // rather than a silently mis-cased permanent, and USERDEFINED/NOTDEFINED classify identically.
    private static readonly Map<IfcActionSourceTypeEnum, ActionRow> CaseSources = toMap(Seq(
        (IfcActionSourceTypeEnum.DEAD_LOAD_G,         new ActionRow("dead",    ActionClass.Permanent,  None, None)),
        (IfcActionSourceTypeEnum.COMPLETION_G1,       new ActionRow("dead",    ActionClass.Permanent,  None, None)),
        (IfcActionSourceTypeEnum.PRESTRESSING_P,      new ActionRow("dead",    ActionClass.Permanent,  None, None)),
        (IfcActionSourceTypeEnum.SETTLEMENT_U,        new ActionRow("dead",    ActionClass.Permanent,  None, None)),
        (IfcActionSourceTypeEnum.SHRINKAGE,           new ActionRow("dead",    ActionClass.Permanent,  None, None)),
        (IfcActionSourceTypeEnum.CREEP,               new ActionRow("dead",    ActionClass.Permanent,  None, None)),
        (IfcActionSourceTypeEnum.SYSTEM_IMPERFECTION, new ActionRow("dead",    ActionClass.Permanent,  None, None)),
        (IfcActionSourceTypeEnum.LACK_OF_FIT,         new ActionRow("dead",    ActionClass.Permanent,  None, None)),
        (IfcActionSourceTypeEnum.SNOW_S,              new ActionRow("snow",    ActionClass.Variable,   None, Some(EurocodeAction.Snow))),
        (IfcActionSourceTypeEnum.WIND_W,              new ActionRow("wind",    ActionClass.Variable,   None, Some(EurocodeAction.Wind))),
        (IfcActionSourceTypeEnum.EARTHQUAKE_E,        new ActionRow("seismic", ActionClass.Accidental, None, Some(EurocodeAction.Seismic)))));

    // The two-tier row walked off the activity's load-group assignment. An ungrouped activity takes the same
    // variable nature row, the unfactored case the consumer defaults.
    internal static ActionRow RowOf(IfcStructuralActivity activity, Option<EurocodePolicy> eurocode) =>
        activity.HasAssignments.AsIterable()
            .Choose(static a => a is IfcRelAssignsToGroup { RelatingGroup: IfcStructuralLoadGroup g } ? Some(g) : None)
            .ToSeq().Head
            .Map(g => Classify(g, eurocode))
            .IfNone(() => Nature(IfcActionTypeEnum.VARIABLE_Q, eurocode));

    internal static ActionRow Classify(IfcStructuralLoadGroup group, Option<EurocodePolicy> eurocode) =>
        CaseSources.Find(group.ActionSource).IfNone(() => Nature(group.ActionType, eurocode));

    // The NATURE tier: a permanent group is the dead permanent action carrying no psi, and every other nature
    // the IMPOSED variable action — which is where the project's own category lands, because EN 1990 keys the
    // imposed psi set by category and the IFC source vocabulary carries no category of its own.
    private static ActionRow Nature(IfcActionTypeEnum nature, Option<EurocodePolicy> eurocode) =>
        nature == IfcActionTypeEnum.PERMANENT_G
            ? new ActionRow("dead", ActionClass.Permanent, None, None)
            : new ActionRow("live", ActionClass.Variable, eurocode.Bind(static policy => policy.Imposed),
                eurocode.Bind(static policy => policy.Imposed).Map(static _ => EurocodeAction.Imposed));

    // --- [FACTORS]

    // The factor read mints the case over an EMPTY roster: the psi set is the package's own Table A1.1 column
    // and carries no dependence on a payload, so one mint serves both this read and the combination fold rather
    // than a second factor-only entry beside it. Only the psi mint reaches a table — the Annex A1.1 singletons
    // throw for an uncovered annex and the standards kernel throws MissingNationalAnnexException — while the
    // partial factors are settled property reads on an admitted policy value and cannot fault.
    internal static Fin<Map<PropertyName, PropertyValue>> Factors(
        ActionRow row, Option<EurocodePolicy> eurocode, UnitScheme scale, Op key) =>
        eurocode.Match(
            None: static () => Fin.Succ(Map<PropertyName, PropertyValue>()),
            Some: policy => key.Catch(() => row.Action.Bind(action => action.Mint(policy, [])).Match(
                    Some: variable => PsiRow.Items.ToSeq().Map(psi => (psi.Name, psi.Read(variable).DecimalFractions)),
                    None: static () => Seq<(PropertyName, double)>())
                + Partials(policy.Situation))
                .Bind(rows => StructuralProjection.Measures(
                    rows.Map(static factor => (factor.Item1, StructuralProjection.Anonymous, factor.Item2)), scale, key))
                .Map(factors => factors.Add(Situation,
                    StructuralProjection.Enumerated(policy.Situation.Class.ToString(), SituationKinds))));

    private static Seq<(PropertyName, double)> Partials(IDesignSituation situation) =>
        GammaRow.Items.ToSeq().Map(row => row.Read(situation).Map(gamma => (row.Name, gamma))).Somes();

    // --- [COMBINATION]

    // The combination roster a LOAD_COMBINATION_GROUP lowers: the grouped load cases mint as the package's OWN
    // typed cases carrying their activities' typed carriers, the set is elected off the policy, and each
    // combination publishes its generated Definition — the 6.10 / 6.10a-b expression with its gammas and psis
    // already resolved — beside the factored design actions GetFactoredLoads yields, in ONE shared order.
    internal static Fin<Map<PropertyName, PropertyValue>> Combination(
        IfcStructuralLoadGroup group, EurocodePolicy policy, UnitScheme scale, Op key) =>
        toSeq(group.IsGroupedBy)
            .Bind(static rel => toSeq(rel.RelatedObjects).Choose(static o => o is IfcStructuralLoadGroup g ? Some(g) : None))
            .TraverseM(member => CaseOf(member, policy, scale, key))
            .As()
            .Bind(cases => key.Catch(() => Elect(cases.Somes().ToList(), policy)))
            .Bind(combinations => toSeq(combinations)
                .Bind(static c => toSeq(c.GetFactoredLoads()).Bind(Components))
                .TraverseM(row => StructuralProjection.Admit(row.Measure, row.Si, UnitScheme.Si, key))
                .As()
                .Map(values => Map(
                    (Combinations, (PropertyValue)new PropertyValue.List(
                        toSeq(combinations).Map(static c => (PropertyValue)new PropertyValue.Text(c.Definition)))),
                    (FactoredActions, (PropertyValue)new PropertyValue.List(
                        values.Map(static value => (PropertyValue)new PropertyValue.Measure(value)))))));

    // The situation CLASS decides whether the ULS-set axis is read at all: seismic and accidental name their
    // own roster, and every persistent or transient situation reads the policy's set row.
    private static IList<ILoadCombination> Elect(IList<(ActionRow Row, ILoadCase Case)> cases, EurocodePolicy policy) =>
        policy.Situation.Class switch {
            DesignSituationClass.Seismic => ENCombinationFactory.CreateSeismic(
                cases.Where(static pair => pair.Row.Case == "seismic").Select(static pair => pair.Case).OfType<IVariableCase>().ToList(),
                policy.Importance,
                cases.Where(static pair => pair.Row.Case != "seismic").Select(static pair => pair.Case).ToList(),
                policy.Annex, ComboPrefix, 1).Cast<ILoadCombination>().ToList(),
            DesignSituationClass.Accidental => Sweep(cases, policy, SweepKind.Accidental),
            _ => policy.Set.Assemble(cases, policy),
        };

    internal static IList<ILoadCase> All(IList<(ActionRow Row, ILoadCase Case)> cases) =>
        cases.Select(static pair => pair.Case).ToList();

    // The hand-assembled roster: one combination per leading variable action (one bare combination when no
    // variable exists), every permanent case seated through the package's own SetPermanentCases under the
    // policy's declared sense, the remaining variables accompanying. The row supplies the record; this fold
    // supplies the sweep, so the two arms share one leading-action partition.
    internal static IList<ILoadCombination> Sweep(
        IList<(ActionRow Row, ILoadCase Case)> cases, EurocodePolicy policy, SweepKind kind) {
        List<IPermanentCase> permanents = cases.Select(static pair => pair.Case).OfType<IPermanentCase>().ToList();
        List<bool> favours = permanents.ConvertAll(_ => policy.Sense.Favours);
        List<IVariableCase> variables = cases.Select(static pair => pair.Case).OfType<IVariableCase>().ToList();
        IEnumerable<(IList<IVariableCase> Main, IList<IVariableCase> Rest)> sweeps = variables.Count == 0
            ? [(new List<IVariableCase>(), new List<IVariableCase>())]
            : variables.Select(leader => (
                (IList<IVariableCase>)new List<IVariableCase> { leader },
                (IList<IVariableCase>)variables.Where(other => !ReferenceEquals(other, leader)).ToList()));
        return sweeps.Select(split => {
            LoadCombination combination = kind.Assemble(policy.Situation, split.Main, split.Rest);
            combination.SetPermanentCases(permanents, favours);
            return (ILoadCombination)combination;
        }).ToList();
    }

    // --- [CARRIER]

    // One grouped load group lowered to the package's own typed case BESIDE its resolved ActionRow — the pair
    // is what lets Elect partition the seismic-row cases from their accompaniment without a name probe. The row
    // resolves through the SAME two-tier fold RowOf takes (a nature-only read here left an EARTHQUAKE_E group
    // unclassified and the whole seismic chain unreachable); a permanent nature mints the bare PermanentCase, a
    // variable one mints through the row's OWN mint — the loads-taking call, so the case arrives with its psi
    // set AND its payload at once. A variable group whose action the code does not classify carries no mint and
    // therefore no case, so it never enters the combination under a fabricated psi set.
    private static Fin<Option<(ActionRow Row, ILoadCase Case)>> CaseOf(
        IfcStructuralLoadGroup group, EurocodePolicy policy, UnitScheme scale, Op key) =>
        toSeq(group.IsGroupedBy)
            .Bind(static rel => toSeq(rel.RelatedObjects).Choose(static o => o is IfcStructuralActivity a ? Some(a) : None))
            .TraverseM(activity => Optional(activity.AppliedLoad).Match(
                Some: load => Carrier(load, Application(activity.GlobalOrLocal), scale, key),
                None: static () => Fin.Succ(Seq<ILoad>())))
            .As()
            .Map(carried => {
                List<ILoad> loads = carried.Bind(static seq => seq).ToList();
                ActionRow row = Classify(group, Some(policy));
                return group.ActionType == IfcActionTypeEnum.PERMANENT_G
                    ? Some((Row: row, Case: (ILoadCase)new PermanentCase { Name = group.Name ?? row.Case, Loads = loads }))
                    : row.Action.Bind(action => action.Mint(policy, loads)).Map(variable => (Row: row, Case: (ILoadCase)variable));
            });

    // The typed carrier one GG load lowers to, dispatched on the SAME closed LoadFamily roster the reader's
    // component projection keys on, so a new load family breaks HERE at compile time instead of falling into a
    // catch-all that would factor it as nothing. A single force yields TWO carriers — the point force and its
    // point moment — because the two are distinct ILoad shapes the combination algebra factors separately. A
    // temperature action, a prescribed displacement, and a trapezoid have no ILoad shape in the taxonomy: they
    // carry their rows and take no part in the fold rather than lowering onto a nearest-fit carrier.
    private static Fin<Seq<ILoad>> Carrier(IfcStructuralLoad load, LoadApplication application, UnitScheme scale, Op key) =>
        LoadFamily.Of(load).Match(
            None: static () => Fin.Succ(Seq<ILoad>()),
            Some: family => StructuralProjection.Measures(family.Vectors(load), scale, key).Map(si => family.Switch<Seq<ILoad>>(
                singleForce: () => Seq<ILoad>(
                    new PointForce(
                        Force.FromNewtons(StructuralProjection.Si(si, StructuralRows.Force["X"])),
                        Force.FromNewtons(StructuralProjection.Si(si, StructuralRows.Force["Y"])),
                        Force.FromNewtons(StructuralProjection.Si(si, StructuralRows.Force["Z"]))),
                    new PointMoment(
                        Torque.FromNewtonMeters(StructuralProjection.Si(si, StructuralRows.Moment["X"])),
                        Torque.FromNewtonMeters(StructuralProjection.Si(si, StructuralRows.Moment["Y"])),
                        Torque.FromNewtonMeters(StructuralProjection.Si(si, StructuralRows.Moment["Z"])))),
                linearForce: () => Seq<ILoad>(new LineForce(
                    ForcePerLength.FromNewtonsPerMeter(StructuralProjection.Si(si, StructuralRows.Force["X"])),
                    ForcePerLength.FromNewtonsPerMeter(StructuralProjection.Si(si, StructuralRows.Force["Y"])),
                    ForcePerLength.FromNewtonsPerMeter(StructuralProjection.Si(si, StructuralRows.Force["Z"])), application)),
                planarForce: () => Seq<ILoad>(new AreaForce(
                    Pressure.FromPascals(StructuralProjection.Si(si, StructuralRows.PlanarForce["X"])),
                    Pressure.FromPascals(StructuralProjection.Si(si, StructuralRows.PlanarForce["Y"])),
                    Pressure.FromPascals(StructuralProjection.Si(si, StructuralRows.PlanarForce["Z"])), application)),
                temperature: static () => Seq<ILoad>(),
                displacement: static () => Seq<ILoad>(),
                configuration: static () => Seq<ILoad>())));

    // The measure identity each ILoad component re-admits under: the carrier holds UnitsNet quantities, so the
    // SI magnitude reads off the quantity's own SI accessor and the row signs the IFC measure type that
    // quantity corresponds to — the factored action re-enters the seam on the SAME frozen MeasureDimensions
    // rows the ingest used, never as a bare double and never under a registry quantity name.
    private static Seq<(Option<string> Measure, double Si)> Components(ILoad load) => load switch {
        IPointForce f => Triple(StructuralProjection.Named<IfcForceMeasure>(), f.X.Newtons, f.Y.Newtons, f.Z.Newtons),
        IPointMoment m => Triple(StructuralProjection.Named<IfcTorqueMeasure>(), m.Xx.NewtonMeters, m.Yy.NewtonMeters, m.Zz.NewtonMeters),
        ILineForce l => Triple(StructuralProjection.Named<IfcLinearForceMeasure>(), l.X.NewtonsPerMeter, l.Y.NewtonsPerMeter, l.Z.NewtonsPerMeter),
        IAreaForce a => Triple(StructuralProjection.Named<IfcPlanarForceMeasure>(), a.X.Pascals, a.Y.Pascals, a.Z.Pascals),
        // A gravity carrier's components are direction ratios the schema names no measure for, so the triple
        // signs nothing and admits dimensionless rather than forging a round-trip identity.
        IGravity g => Triple(StructuralProjection.Anonymous, g.X.DecimalFractions, g.Y.DecimalFractions, g.Z.DecimalFractions),
        _ => Seq<(Option<string>, double)>(),
    };

    private static Seq<(Option<string> Measure, double Si)> Triple(Option<string> measure, double x, double y, double z) =>
        Seq((measure, x), (measure, y), (measure, z));

    // The projection frame an ILineForce/IAreaForce resolves against, off the activity's own IFC declaration —
    // the same GlobalOrLocal fact the reader's bag stamps as a row, read once rather than defaulted per carrier.
    private static LoadApplication Application(IfcGlobalOrLocalEnum declared) =>
        declared == IfcGlobalOrLocalEnum.LOCAL_COORDS ? LoadApplication.Local : LoadApplication.Global;
}
```

## [03]-[RESEARCH]

(none)
