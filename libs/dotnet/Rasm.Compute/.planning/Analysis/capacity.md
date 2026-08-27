# [COMPUTE_CAPACITY]

Rasm.Compute member-altitude design check and the response-spectrum route that feeds it. It reads the `Analysis/frame` `MemberResponse` signed envelope, checks each member through the `(DesignCode, LimitState)` capacity table — one key set shared with the section-altitude `Rasm.Materials` `DesignBasis` roster — and returns the governing utilization as one `AssessmentResult` fact stream. The seismic overload folds the condensed modal pencil against a `DesignSpectrum` row under a typed per-axis participation floor and checks the combined demands through the SAME table.

Applicability is TWO scoped questions the shape keeps apart: `LimitState.Applies` answers the CODE-family one and cell presence the code-scoped one, so an unserved `(code, state)` pair reports `NotApplicable` and contributes no ratio. A run that resolved no ratio at all governs `None`, which the spine bands `NotApplicable` — never a `0.0` reading `Satisfied` on a member no check ever ran.

## [01]-[INDEX]

- [02]-[DESIGN_CHECK]: `StructuralAnalysis.Run` checks each member through the `(DesignCode, LimitState)` capacity table and folds one governing-utilization fact stream.
- [03]-[SEISMIC_ROUTE]: `Run` over `AssessmentRequest.Seismic` folds the sparse modal response against a `DesignSpectrum` row under a typed per-axis participation floor.

## [02]-[DESIGN_CHECK]

- Owner: `MaterialFamily` the constitutive family and `FamilyEvidence` the closed evidence the contract actually supports for it; `SafetyFormat` the resistance-format row carrying the ASD/LRFD/limit-state arithmetic EVERY cell folds through; `SectionModulus` the plastic/elastic modulus-pair row; `ResistanceFactors` the per-code φ column set; `DesignCode` `[SmartEnum<string>]` the standard rows carrying the `MaterialFamily`, the `SafetyFormat`, the partial factor, the φ set, and the interaction delegate; `LimitState` `[SmartEnum<string>]` the check rows carrying the optional demand selector and the `Applies(DesignCode)` predicate; `CapacityContext` the section+strength+geometry+code bundle every capacity reads; the `Capacities` `(DesignCode, LimitState)` frozen table of REAL delegates keyed on the typed pair; `MemberCapacity` the four sense-aware interaction operands and `MemberCheck` the per-check carrier; `CheckFacts` the one fact-and-governing projection both routes fold; `StructuralAnalysis.Run` the governing-utilization entry, overloaded on the request case.
- Cases: `DesignCode` rows `aisc360`/`en1993`/`en1993-1-4`/`en1994`/`en1992`/`nds`/`en1995`/`aci318`/`tms402`/`en1996`/`aisi-s100`/`en1999`/`sdpws` — every hand-rolled structural family carries BOTH its US and its Eurocode row, aluminium the EN-only `en1999` row until a US aluminium pack proves, and `sdpws` the cell-less wire-mirror row; the key SET is the `Rasm.Materials` `Component/capacity#SECTION_CAPACITY` `DesignBasis` MEMBER roster spelled identically and PROVEN identical by `DesignCode.Probe`, so a section-altitude verdict and this member-altitude one name one jurisdiction and a rename on either side fails at composition rather than unrouting silently. `LimitState` rows `axial-tension`/`axial-compression`/`flexure-major`/`flexure-minor`/`shear-major`/`shear-minor`/`combined`/`deflection` (shear split per axis so the major-axis demand checks against `AvY` and the minor against `AvZ`, never one shear area for both) — the capacity is a `(code, state)` cell, each cell the GOVERNING formula for THAT code's material model; lateral-torsional buckling is FOLDED into the flexure-major capacity (one capacity, never a duplicate state).
- Entry: `public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Structural request, GeometrySource geometry, AssessmentSink sink, IClock clock)` — `Project` reads the idealization off `FrameInputs`, `Solve` recovers the signed `MemberResponse` extremes, `Check` folds each member through every applicable `LimitState`, and `CheckFacts` yields the fact stream with the OPTIONAL governing ratio the spine derives its verdict from. `AssessmentRequest.Seismic` dispatches the `[03]` chain as the SIBLING OVERLOAD through the spine's own case `Switch`, never an `Option` gate inside this arm.
- Auto: the column capacity reads the `EffectiveLengthFactor × UnbracedLength / RadiusOfGyrationMinor` slenderness (AISC `Fcr`, EN `χ`); the flexure-major capacity reads `Lb` against `Lp` and the elastic LTB moment (EN `χLT`); the deflection check reads `MemberResponse.MaxDeflection` against `StructuralPolicy.DeflectionLimitRatio × span`; the combined axial+flexure interaction folds each signed corner per the `DesignCode.Interaction` delegate; every cell states its NOMINAL resistance and the row's own `SafetyFormat` applies the format — φ for LRFD, `γM` for limit-state, neither for the ASD rows whose reference values are already allowable — so the φ/`γM` spread no longer rides in each cell body and the format column has thirteen readers.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum]`/`[Union]`/`[UseDelegateFromConstructor]`), LanguageExt.Core (`Fin`/`Option`/`Seq`/`TraverseM`), PureHDF (`H5File`, `H5Dataset<T>` — the shared demands/modal artifact), the `Runtime/archive` chunk-cursor route (`HdfArchive.Begin`, `ChunkGrid.Seat`, `HdfWriter.Open`, `ChunkCursor<T>.Write` — the deferred modal-basis write, its ordinal held by the cursor), Generator.Equals (`[Equatable]`+`[PrecisionEquality]`+`[OrderedEquality]` — the variant diff and the correlation latent-trap repair), CommunityToolkit.HighPerformance (`Span2D<double>`/`MemoryOwner<double>` — the archive block and the correlation matrix), Rasm (kernel — `EpsilonPolicy.BandUlp`), the `Analysis/frame` `DivisorBand` guarded quotient every cell and kernel divides through, `Solver/contract` (`SolveLane`, `LanePolicy.CanonicalModal`, `SolveRoute.Condensed`, `CondensationPolicy`, `CondensationEvidence`, `ModalParticipation`, `SolveResult`), Rasm.Element (project — `SectionProperties`, `MaterialPropertySet`, `NodeId`, `BlobKey`), NodaTime (`Instant`), BCL inbox (`FrozenDictionary`, `FrozenSet`, `TensorPrimitives`).
- Growth: a new design code is one `DesignCode` row with its `(code, state)` cells and its key in the shared basis roster; a new limit state one `LimitState` row with its column of cells; a new material family one `MaterialFamily` row with its codes' cells and its band membership on `FamilyEvidence` — the check fold re-reads the table, never a new check method per code and never a parallel verdict family beside `MemberCheck`.
- Boundary: the DESIGN-BASIS VOCABULARY is shared with `Rasm.Materials` `Component/capacity#SECTION_CAPACITY` as one KEY SET carried by two typed rows, because the branch strata forbid a reference in either direction and the `[WIRE]: SectionCapacity` contract carries portable scalars keyed by section. `DesignCode.Probe` proves the bijection at composition — the `AssessmentRoute.Probe` precedent applied to the second cross-registry correspondence no type system holds — so the thirteen keys can no longer drift apart in silence, and the eight section-carve bases (glazing, connection, anchorage, fatigue) stay outside the member roster by construction. Altitudes stay split, each carrying what its own inputs support: the section-altitude owner holds the published strength tables a geometric contract cannot carry — the RC rebar interaction, the AISI effective width the `SteelDesign` cold-formed arm derives, the EN 1994 composite couple, the EN 1996 `f_xk`/`f_vk0` rows — and this page holds the slenderness, unbraced-length, and deflection facts a cross-section cannot decide. `MemberCapacity` is this page's own member-altitude interaction operand carrier and is NOT the contract's `SectionCapacity` union; both spellings are live on their own sides of the boundary and the altitude word is what keeps them apart. The design codes are hand-rolled (no .NET package owns the AISC, Eurocode, NDS, ACI, TMS, or AISI design rules), realized as a data table of capacity delegates — the canonical `POLICY_VALUES`/`DERIVED_LOGIC` collapse, never a switch ladder and never one family's formulas applied to every material. The authoritative family is `DesignCode.Family`: `Classify` returns EVIDENCE — the contract's own realized `Orthotropic` declaration, or the constitutive modulus BAND naming every family that band cannot separate — and the code decides, so aluminium inside the concrete band and masonry inside the timber band both resolve their own cells instead of failing a mismatch the band's imprecision invented.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MaterialFamily {
    public static readonly MaterialFamily Steel           = new("steel");
    public static readonly MaterialFamily Concrete        = new("concrete");
    public static readonly MaterialFamily Timber          = new("timber");
    public static readonly MaterialFamily Masonry         = new("masonry");
    public static readonly MaterialFamily ColdFormedSteel = new("cold-formed-steel");
    public static readonly MaterialFamily Aluminum        = new("aluminum");

    public static FamilyEvidence Classify(MaterialPropertySet.Mechanical m, Option<MaterialPropertySet.Orthotropic> directional) =>
        directional.IsSome ? new FamilyEvidence.Declared(Timber)
        : m.YoungsModulus.Si > 150e9 ? Band(m, Steel, ColdFormedSteel)
        : m.YoungsModulus.Si > 20e9 ? Band(m, Concrete, Aluminum)
        : m.YoungsModulus.Si > 5e9 ? Band(m, Timber, Masonry)
        : Band(m, Masonry);

    static FamilyEvidence Band(MaterialPropertySet.Mechanical m, params ReadOnlySpan<MaterialFamily> candidates) =>
        new FamilyEvidence.Band(m.YoungsModulus.Si, Seq(candidates));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FamilyEvidence {
    private FamilyEvidence() { }
    public sealed record Declared(MaterialFamily Family) : FamilyEvidence;
    public sealed record Band(double YoungsModulusSi, Seq<MaterialFamily> Candidates) : FamilyEvidence;

    public bool Admits(MaterialFamily family) => Switch(
        declared: declared => declared.Family == family,
        band: band => band.Candidates.Contains(family));

    public string Witness => Switch(
        declared: static declared => declared.Family.Key,
        band: static band => $"band:{band.YoungsModulusSi:R}");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SafetyFormat {
    public static readonly SafetyFormat Asd        = new("asd",         static (nominal, _, _) => nominal);
    public static readonly SafetyFormat Lrfd       = new("lrfd",        static (nominal, phi, _) => phi * nominal);
    public static readonly SafetyFormat LimitState = new("limit-state", static (nominal, _, gammaM) => nominal.Over(gammaM));

    [UseDelegateFromConstructor]
    public partial double Apply(double nominal, double phi, double gammaM);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SectionModulus {
    public static readonly SectionModulus Plastic = new("plastic", static s => s.Wply.Si, static s => s.Wplz.Si);
    public static readonly SectionModulus Elastic = new("elastic", static s => s.Wely.Si, static s => s.Welz.Si);

    [UseDelegateFromConstructor]
    public partial double Major(SectionProperties section);

    [UseDelegateFromConstructor]
    public partial double Minor(SectionProperties section);
}

public readonly record struct ResistanceFactors(double Tension, double Compression, double Flexure, double Shear) {
    public static readonly ResistanceFactors Unity = new(1.0, 1.0, 1.0, 1.0);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DesignCode {
    public static readonly DesignCode Aisc360  = new("aisc360",   MaterialFamily.Steel,           SafetyFormat.Lrfd,       1.00, new ResistanceFactors(0.90, 0.90, 0.90, 1.00), AiscH11);
    public static readonly DesignCode En1993   = new("en1993",    MaterialFamily.Steel,           SafetyFormat.LimitState, 1.00, ResistanceFactors.Unity, En1993Interaction);
    public static readonly DesignCode En1994   = new("en1994",    MaterialFamily.Steel,           SafetyFormat.LimitState, 1.00, ResistanceFactors.Unity, En1993Interaction);
    public static readonly DesignCode En1993Stainless = new("en1993-1-4", MaterialFamily.Steel,   SafetyFormat.LimitState, 1.10, ResistanceFactors.Unity, En1993Interaction);
    public static readonly DesignCode En1992   = new("en1992",    MaterialFamily.Concrete,        SafetyFormat.LimitState, 1.50, ResistanceFactors.Unity, LinearInteraction);
    public static readonly DesignCode Nds      = new("nds",       MaterialFamily.Timber,          SafetyFormat.Asd,        1.00, ResistanceFactors.Unity, NdsInteraction);
    public static readonly DesignCode En1995   = new("en1995",    MaterialFamily.Timber,          SafetyFormat.LimitState, 1.25, ResistanceFactors.Unity, En1995Interaction);
    public static readonly DesignCode Aci318   = new("aci318",    MaterialFamily.Concrete,        SafetyFormat.Lrfd,       1.00, new ResistanceFactors(0.90, 0.65, 0.90, 0.75), LinearInteraction);
    public static readonly DesignCode Tms402   = new("tms402",    MaterialFamily.Masonry,         SafetyFormat.Lrfd,       1.00, new ResistanceFactors(0.90, 0.90, 0.90, 0.80), LinearInteraction);
    public static readonly DesignCode En1996   = new("en1996",    MaterialFamily.Masonry,         SafetyFormat.LimitState, 2.00, ResistanceFactors.Unity, LinearInteraction);
    public static readonly DesignCode AisiS100 = new("aisi-s100", MaterialFamily.ColdFormedSteel, SafetyFormat.Lrfd,       1.00, new ResistanceFactors(0.90, 0.85, 0.90, 0.95), AiscH11);
    public static readonly DesignCode En1999   = new("en1999",    MaterialFamily.Aluminum,        SafetyFormat.LimitState, 1.10, ResistanceFactors.Unity, Ec9Interaction);
    public static readonly DesignCode Sdpws    = new("sdpws",     MaterialFamily.Timber,          SafetyFormat.Asd,        1.00, ResistanceFactors.Unity, LinearInteraction);

    public MaterialFamily Family { get; }
    public SafetyFormat Format { get; }
    public double GammaM { get; }
    public ResistanceFactors Phi { get; }

    public double Resist(double nominal, double phi) => Format.Apply(nominal, phi, GammaM);

    [UseDelegateFromConstructor]
    public partial double Interaction(SectionDemand demand, MemberCapacity capacity, CapacityContext context);

    public static Fin<DesignCode> For(AssessmentRoute route) =>
        TryGet(route.Key, out DesignCode code)
            ? Fin.Succ(code)
            : Fin.Fail<DesignCode>(new ComputeFault.AssessmentInputMissing(AssessmentInputReason.RouteUnrouted, route.Key));

    static readonly FrozenSet<string> BasisKeys = FrozenSet.ToFrozenSet(
        ["aisc360", "aisi-s100", "en1992", "en1993", "en1993-1-4", "en1994", "en1995", "en1996", "en1999",
         "tms402", "sdpws", "nds", "aci318"],
        StringComparer.Ordinal);

    public static Fin<Unit> Probe() {
        Seq<string> unbased = toSeq(Items).Map(static code => code.Key).Filter(static key => !BasisKeys.Contains());
        Seq<string> unrouted = toSeq(BasisKeys).Filter(static key => !TryGet(out _));
        return unbased.IsEmpty && unrouted.IsEmpty
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ComputeFault.AssessmentInputMissing(
                AssessmentInputReason.RouteUnrouted, string.Join(',', unbased + unrouted)));
    }

    static double AiscH11(SectionDemand d, MemberCapacity c, CapacityContext _) {
        double axial = c.AxialRatio(d.N);
        double bending = Math.Abs(d.My).Over(c.FlexureMajor) + Math.Abs(d.Mz).Over(c.FlexureMinor);
        return axial >= 0.2 ? axial + (8.0 / 9.0 * bending) : (axial / 2.0) + bending;
    }

    static double En1993Interaction(SectionDemand d, MemberCapacity c, CapacityContext _) =>
        c.AxialRatio(d.N) + Math.Abs(d.My).Over(c.FlexureMajor) + Math.Abs(d.Mz).Over(c.FlexureMinor);

    static double En1995Interaction(SectionDemand d, MemberCapacity c, CapacityContext _) {
        double axial = c.AxialRatio(d.N);
        return (axial * axial) + Math.Abs(d.My).Over(c.FlexureMajor) + (0.7 * Math.Abs(d.Mz).Over(c.FlexureMinor));
    }

    static double NdsInteraction(SectionDemand d, MemberCapacity c, CapacityContext x) {
        double bendMajor = Math.Abs(d.My).Over(c.FlexureMajor), bendMinor = Math.Abs(d.Mz).Over(c.FlexureMinor);
        double axial = c.AxialRatio(d.N);
        if (d.N >= 0.0) { return axial + bendMajor + bendMinor; }
        double e = x.Strength.YoungsModulus.Si, le = x.EffectiveLengthFactor * x.UnbracedLength, p = -d.N;
        double slender1 = le.Over(x.Section.Depth.Si), slender2 = le.Over(x.Section.Width.Si);
        double pe1 = (0.822 * e).Over(slender1 * slender1) * x.Section.Area.Si;
        double pe2 = (0.822 * e).Over(slender2 * slender2) * x.Section.Area.Si;
        double rb2 = (x.UnbracedLength * x.Section.Depth.Si).Over(x.Section.Width.Si * x.Section.Width.Si);
        double me = (1.20 * e).Over(rb2) * x.Section.Wely.Si;
        return (axial * axial)
            + bendMajor.Over(1.0 - p.Over(pe1))
            + bendMinor.Over(1.0 - p.Over(pe2) - Math.Pow(Math.Abs(d.My).Over(me), 2.0));
    }

    static double Ec9Interaction(SectionDemand d, MemberCapacity c, CapacityContext _) =>
        Math.Pow(c.AxialRatio(d.N), 0.8)
        + Math.Pow(Math.Abs(d.My).Over(c.FlexureMajor), 0.8)
        + Math.Pow(Math.Abs(d.Mz).Over(c.FlexureMinor), 0.8);

    static double LinearInteraction(SectionDemand d, MemberCapacity c, CapacityContext _) =>
        c.AxialRatio(d.N) + Math.Abs(d.My).Over(c.FlexureMajor);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class LimitState {
    public static readonly LimitState AxialTension     = new("axial-tension",     static r => Some(Math.Max(r.Max.N, 0.0)),  static c => c.Family != MaterialFamily.Concrete && c.Family != MaterialFamily.Masonry);
    public static readonly LimitState AxialCompression = new("axial-compression", static r => Some(Math.Max(-r.Min.N, 0.0)), static _ => true);
    public static readonly LimitState FlexureMajor     = new("flexure-major",     static r => Some(r.Span(static d => d.My)), static _ => true);
    public static readonly LimitState FlexureMinor     = new("flexure-minor",     static r => Some(r.Span(static d => d.Mz)), static c => c.Family == MaterialFamily.Steel || c.Family == MaterialFamily.ColdFormedSteel || c.Family == MaterialFamily.Aluminum);
    public static readonly LimitState ShearMajor       = new("shear-major",       static r => Some(r.Span(static d => d.Vy)), static _ => true);
    public static readonly LimitState ShearMinor       = new("shear-minor",       static r => Some(r.Span(static d => d.Vz)), static _ => true);
    public static readonly LimitState Combined         = new("combined",          static _ => None,                          static c => c.Family == MaterialFamily.Steel || c.Family == MaterialFamily.ColdFormedSteel || c.Family == MaterialFamily.Timber || c.Family == MaterialFamily.Aluminum);
    public static readonly LimitState Deflection       = new("deflection",        static _ => None,                          static _ => true);

    [UseDelegateFromConstructor]
    public partial Option<double> Demand(MemberResponse response);

    [UseDelegateFromConstructor]
    public partial bool Applies(DesignCode code);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct RcShearLink(double AswSi, double FywdSi, double VrdMaxSi);

public readonly record struct BucklingCurve(double Alpha, double LambdaZero);

public readonly record struct CapacityContext(
    SectionProperties Section, MaterialPropertySet.Mechanical Strength, Option<MaterialPropertySet.Orthotropic> Directional,
    DesignCode Code, double UnbracedLength, double EffectiveLengthFactor, PositiveMagnitude CotTheta,
    Option<RcShearLink> ShearLink = default, Option<PositiveMagnitude> StirrupSpacing = default,
    Option<BucklingCurve> Buckling = default) {
    public static CapacityContext Of(StructuralMember m, DesignCode code, StructuralPolicy policy) =>
        new(m.Section, m.Strength, m.Directional, code, m.Length, m.EffectiveLengthFactor, policy.CotTheta,
            m.ShearLink, policy.StirrupSpacing, m.Buckling);

    public double Slenderness => EffectiveLengthFactor * UnbracedLength.Over(Section.RadiusOfGyrationMinor.Si);

    public double ShearModulusSi => Directional.Map(static o => o.ShearModulus.Si).IfNone(() => Strength.ShearModulus.Si);
}

public readonly record struct MemberCapacity(double AxialTension, double AxialCompression, double FlexureMajor, double FlexureMinor) {
    public double AxialRatio(double n) => n >= 0.0 ? n.Over(AxialTension) : (-n).Over(AxialCompression);
}

[Equatable]
public readonly partial record struct MemberCheck(
    NodeId Member, LimitState State, Option<double> Demand, Option<double> Capacity, Option<double> Utilization) {
    public AssessmentVerdict Verdict => AssessmentVerdict.FromRatio(Utilization);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class StructuralAnalysis {
    static readonly FrozenDictionary<(DesignCode Code, LimitState State), Func<CapacityContext, double>> Capacities = Seed();

    static FrozenDictionary<(DesignCode, LimitState), Func<CapacityContext, double>> Seed() =>
        (SteelCells(DesignCode.En1993, new BucklingCurve(0.34, 0.20), new BucklingCurve(0.49, 0.20), SectionModulus.Plastic)
         + SteelCells(DesignCode.En1994, new BucklingCurve(0.34, 0.20), new BucklingCurve(0.49, 0.20), SectionModulus.Plastic)
         + SteelCells(DesignCode.En1993Stainless, new BucklingCurve(0.76, 0.20), new BucklingCurve(0.76, 0.40), SectionModulus.Plastic)
         + AiscCells(DesignCode.Aisc360, SectionModulus.Plastic, AiscMn, static c => Math.Min(
             c.Strength.YieldStrength.Si * c.Section.Wplz.Si, 1.6 * c.Strength.YieldStrength.Si * c.Section.Welz.Si))
         + AiscCells(DesignCode.AisiS100, SectionModulus.Elastic,
             static c => c.Strength.YieldStrength.Si * c.Section.Wely.Si,
             static c => c.Strength.YieldStrength.Si * c.Section.Welz.Si)
         + TimberCells(DesignCode.Nds, NdsCp, NdsCl, crackFactor: 1.0)
         + TimberCells(DesignCode.En1995, Ec5Kc, Ec5Kcrit, crackFactor: 0.67)
         + ConcreteCells()
         + MasonryCells()
         + AluminumCells())
        .ToFrozenDictionary(static row => row.Key, static row => row.Rule);

    // --- [CELL_GENERATORS] -------------------------------------------------------------
    static Seq<((DesignCode Code, LimitState State) Key, Func<CapacityContext, double> Rule)> SteelCells(
        DesignCode code, BucklingCurve flexural, BucklingCurve lateral, SectionModulus modulus) =>
        Cells(code,
            tension:     static c => c.Strength.YieldStrength.Si * c.Section.Area.Si,
            compression: c => EnChi(EnLambdaBar(c), flexural) * c.Section.Area.Si * c.Strength.YieldStrength.Si,
            flexureMajor: c => EnChiLt(c, lateral, modulus.Major(c.Section)) * modulus.Major(c.Section) * c.Strength.YieldStrength.Si,
            flexureMinor: c => modulus.Minor(c.Section) * c.Strength.YieldStrength.Si,
            shearMajor:  static c => c.Section.AvY.Si * c.Strength.YieldStrength.Si / Math.Sqrt(3.0),
            shearMinor:  static c => c.Section.AvZ.Si * c.Strength.YieldStrength.Si / Math.Sqrt(3.0));

    static Seq<((DesignCode Code, LimitState State) Key, Func<CapacityContext, double> Rule)> AiscCells(
        DesignCode code, SectionModulus modulus, Func<CapacityContext, double> flexureMajor, Func<CapacityContext, double> flexureMinor) =>
        Cells(code,
            tension:     static c => c.Strength.YieldStrength.Si * c.Section.Area.Si,
            compression: static c => AiscFcr(c) * c.Section.Area.Si,
            flexureMajor: flexureMajor,
            flexureMinor: flexureMinor,
            shearMajor:  static c => 0.60 * c.Strength.YieldStrength.Si * c.Section.AvY.Si,
            shearMinor:  static c => 0.60 * c.Strength.YieldStrength.Si * c.Section.AvZ.Si);

    static Seq<((DesignCode Code, LimitState State) Key, Func<CapacityContext, double> Rule)> TimberCells(
        DesignCode code, Func<CapacityContext, double> stability, Func<CapacityContext, double> lateral, double crackFactor) =>
        Cells(code,
            tension:     static c => c.Strength.YieldStrength.Si * c.Section.Area.Si,
            compression: c => stability(c) * c.Strength.YieldStrength.Si * c.Section.Area.Si,
            flexureMajor: c => lateral(c) * c.Strength.YieldStrength.Si * c.Section.Wely.Si,
            flexureMinor: static c => c.Strength.YieldStrength.Si * c.Section.Welz.Si,
            shearMajor:  c => crackFactor * (2.0 / 3.0) * c.Strength.YieldStrength.Si * c.Section.Area.Si,
            shearMinor:  c => crackFactor * (2.0 / 3.0) * c.Strength.YieldStrength.Si * c.Section.Area.Si);

    static Seq<((DesignCode Code, LimitState State) Key, Func<CapacityContext, double> Rule)> ConcreteCells() =>
        Seq(Cell(DesignCode.Aci318, LimitState.AxialCompression, static c => 0.85 * c.Strength.UltimateStrength.Si * c.Section.Area.Si, DesignCode.Aci318.Phi.Compression),
            Cell(DesignCode.Aci318, LimitState.FlexureMajor, static c => ConcreteFr(c) * c.Section.Wely.Si, DesignCode.Aci318.Phi.Flexure),
            Cell(DesignCode.Aci318, LimitState.ShearMajor, static c => PlainShear(c, c.Section.AvY.Si), DesignCode.Aci318.Phi.Shear),
            Cell(DesignCode.Aci318, LimitState.ShearMinor, static c => PlainShear(c, c.Section.AvZ.Si), DesignCode.Aci318.Phi.Shear),
            Cell(DesignCode.En1992, LimitState.AxialCompression, static c => 0.85 * c.Strength.UltimateStrength.Si * c.Section.Area.Si),
            Cell(DesignCode.En1992, LimitState.FlexureMajor, static c => ConcreteFr(c) * c.Section.Wely.Si),
            Cell(DesignCode.En1992, LimitState.ShearMajor, static c => Truss(c, c.Section.Depth.Si, c.Section.AvY.Si)),
            Cell(DesignCode.En1992, LimitState.ShearMinor, static c => Truss(c, c.Section.Width.Si, c.Section.AvZ.Si)));

    static Seq<((DesignCode Code, LimitState State) Key, Func<CapacityContext, double> Rule)> MasonryCells() =>
        Seq(Cell(DesignCode.Tms402, LimitState.AxialCompression,
                static c => 0.80 * 0.80 * c.Strength.UltimateStrength.Si * TmsSlender(c) * c.Section.Area.Si,
                DesignCode.Tms402.Phi.Compression),
            Cell(DesignCode.En1996, LimitState.AxialCompression, static c => EnMasonryPhi(c) * c.Strength.UltimateStrength.Si * c.Section.Area.Si));

    static Seq<((DesignCode Code, LimitState State) Key, Func<CapacityContext, double> Rule)> AluminumCells() =>
        Seq(Cell(DesignCode.En1999, LimitState.AxialTension, static c => c.Strength.YieldStrength.Si * c.Section.Area.Si),
            Cell(DesignCode.En1999, LimitState.AxialCompression, static c => c.Buckling.Match(
                Some: curve => EnChi(EnLambdaBar(c), curve) * c.Section.Area.Si * c.Strength.YieldStrength.Si,
                None: static () => 0.0)),
            Cell(DesignCode.En1999, LimitState.FlexureMajor, static c =>
                EnChiLt(c, new BucklingCurve(0.20, 0.40), c.Section.Wely.Si) * c.Section.Wely.Si * c.Strength.YieldStrength.Si),
            Cell(DesignCode.En1999, LimitState.FlexureMinor, static c => c.Section.Welz.Si * c.Strength.YieldStrength.Si),
            Cell(DesignCode.En1999, LimitState.ShearMajor, static c => c.Section.AvY.Si * c.Strength.YieldStrength.Si / Math.Sqrt(3.0)),
            Cell(DesignCode.En1999, LimitState.ShearMinor, static c => c.Section.AvZ.Si * c.Strength.YieldStrength.Si / Math.Sqrt(3.0)));

    static Seq<((DesignCode Code, LimitState State) Key, Func<CapacityContext, double> Rule)> Cells(
        DesignCode code,
        Func<CapacityContext, double> tension, Func<CapacityContext, double> compression,
        Func<CapacityContext, double> flexureMajor, Func<CapacityContext, double> flexureMinor,
        Func<CapacityContext, double> shearMajor, Func<CapacityContext, double> shearMinor) =>
        Seq(Cell(code, LimitState.AxialTension, tension, code.Phi.Tension),
            Cell(code, LimitState.AxialCompression, compression, code.Phi.Compression),
            Cell(code, LimitState.FlexureMajor, flexureMajor, code.Phi.Flexure),
            Cell(code, LimitState.FlexureMinor, flexureMinor, code.Phi.Flexure),
            Cell(code, LimitState.ShearMajor, shearMajor, code.Phi.Shear),
            Cell(code, LimitState.ShearMinor, shearMinor, code.Phi.Shear));

    static ((DesignCode Code, LimitState State) Key, Func<CapacityContext, double> Rule) Cell(
        DesignCode code, LimitState state, Func<CapacityContext, double> nominal, double phi = 1.0) =>
        ((code, state), c => code.Resist(nominal(c), phi));

    static Option<double> Capacity(DesignCode code, LimitState state, CapacityContext ctx) =>
        Capacities.TryGetValue((code, state), out Func<CapacityContext, double> rule) ? Some(rule(ctx)) : None;

    // --- [STRENGTH_KERNELS] ------------------------------------------------------------
    static double AiscFcr(CapacityContext c) {
        double fy = c.Strength.YieldStrength.Si;
        double fe = (Math.PI * Math.PI * c.Strength.YoungsModulus.Si).Over(c.Slenderness * c.Slenderness);
        return fy.Over(fe) <= 2.25 ? Math.Pow(0.658, fy.Over(fe)) * fy : 0.877 * fe;
    }

    static double AiscMn(CapacityContext c) {
        double fy = c.Strength.YieldStrength.Si, e = c.Strength.YoungsModulus.Si;
        double ry = c.Section.RadiusOfGyrationMinor.Si, lb = c.UnbracedLength;
        double mp = fy * c.Section.Wply.Si;
        double lp = 1.76 * ry * Math.Sqrt(e.Over(fy)), lr = Math.PI * ry * Math.Sqrt(e.Over(0.7 * fy));
        double mr = 0.7 * fy * c.Section.Wely.Si;
        return lb <= lp ? mp
            : lb <= lr ? Math.Min(mp, mp - ((mp - mr) * (lb - lp).Over(lr - lp)))
            : Math.Min(mp, (Math.PI * Math.PI * e).Over(lb.Over(ry) * lb.Over(ry)) * c.Section.Wely.Si);
    }

    static double EnLambdaBar(CapacityContext c) {
        double i = c.Section.RadiusOfGyrationMinor.Si, l = c.EffectiveLengthFactor * c.UnbracedLength;
        double ncr = (Math.PI * Math.PI * c.Strength.YoungsModulus.Si * c.Section.Area.Si * i * i).Over(l * l);
        return Math.Sqrt((c.Section.Area.Si * c.Strength.YieldStrength.Si).Over(ncr));
    }

    static double EnChi(double lambdaBar, BucklingCurve curve) {
        double phi = 0.5 * (1.0 + (curve.Alpha * (lambdaBar - curve.LambdaZero)) + (lambdaBar * lambdaBar));
        return Math.Min(1.0, 1.0.Over(phi + Math.Sqrt(Math.Max((phi * phi) - (lambdaBar * lambdaBar), EpsilonPolicy.BandUlp))));
    }

    static double EnChiLt(CapacityContext c, BucklingCurve curve, double wSi) {
        double mcr = Math.PI.Over(c.UnbracedLength)
            * Math.Sqrt(Math.Max(c.Strength.YoungsModulus.Si * c.Section.Izz.Si * c.Strength.ShearModulus.Si * c.Section.J.Si, 0.0));
        return EnChi(Math.Sqrt((wSi * c.Strength.YieldStrength.Si).Over(mcr)), curve);
    }

    static double NdsCp(CapacityContext c) {
        double fcStar = c.Strength.YieldStrength.Si;
        double slender = (c.EffectiveLengthFactor * c.UnbracedLength).Over(c.Section.LeastDimension.Si);
        double ratio = (0.822 * c.Strength.YoungsModulus.Si).Over(slender * slender).Over(fcStar), term = (1.0 + ratio) / 1.6;
        return term - Math.Sqrt(Math.Max((term * term) - (ratio / 0.8), 0.0));
    }

    static double NdsCl(CapacityContext c) {
        double fbStar = c.Strength.YieldStrength.Si;
        double rb2 = (c.UnbracedLength * c.Section.Depth.Si).Over(c.Section.Width.Si * c.Section.Width.Si);
        double ratio = (1.20 * c.Strength.YoungsModulus.Si).Over(rb2).Over(fbStar), term = (1.0 + ratio) / 1.9;
        return term - Math.Sqrt(Math.Max((term * term) - (ratio / 0.95), 0.0));
    }

    static Option<double> Ec5E005(CapacityContext c) => c.Directional.Bind(static o => o.E005).Map(static e => e.Si);

    static double Ec5Kc(CapacityContext c) => Ec5E005(c).Match(
        Some: e005 => {
            double slender = c.Slenderness, fc0 = c.Strength.YieldStrength.Si;
            double sigmaCrit = (Math.PI * Math.PI * e005).Over(slender * slender);
            double lambdaRel = Math.Sqrt(fc0.Over(sigmaCrit));
            double k = 0.5 * (1.0 + (0.2 * (lambdaRel - 0.3)) + (lambdaRel * lambdaRel));
            return lambdaRel <= 0.3 ? 1.0 : 1.0.Over(k + Math.Sqrt(Math.Max((k * k) - (lambdaRel * lambdaRel), 0.0)));
        },
        None: static () => 0.0);

    static double Ec5Kcrit(CapacityContext c) => Ec5E005(c).Match(
        Some: e005 => {
            double mcr = (Math.PI * Math.Sqrt(Math.Max(e005 * c.Section.Izz.Si * c.ShearModulusSi * c.Section.J.Si, 0.0)))
                .Over(c.UnbracedLength * c.Section.Wely.Si);
            double lambdaRelM = Math.Sqrt(c.Strength.YieldStrength.Si.Over(mcr));
            return lambdaRelM <= 0.75 ? 1.0 : lambdaRelM <= 1.4 ? 1.56 - (0.75 * lambdaRelM) : 1.0.Over(lambdaRelM * lambdaRelM);
        },
        None: static () => 0.0);

    static double TmsSlender(CapacityContext c) =>
        Math.Max(0.0, 1.0 - Math.Pow(c.UnbracedLength.Over(140.0 * c.Section.RadiusOfGyrationMinor.Si), 2.0));

    static double EnMasonryPhi(CapacityContext c) =>
        Math.Max(0.0, 1.0 - (2.0 / (450.0 * Math.Sqrt(12.0)) * c.UnbracedLength.Over(c.Section.RadiusOfGyrationMinor.Si)));

    static double ConcreteFr(CapacityContext c) =>
        0.62 * Math.Sqrt(Math.Max(c.Strength.UltimateStrength.Si / 1e6, 0.0)) * 1e6;

    static double PlainShear(CapacityContext c, double shearAreaSi) =>
        0.17 * Math.Sqrt(Math.Max(c.Strength.UltimateStrength.Si / 1e6, 0.0)) * 1e6 * shearAreaSi;

    static double Truss(CapacityContext c, double depthSi, double shearAreaSi) =>
        (from link in c.ShearLink from spacing in c.StirrupSpacing select (link, spacing)).Match(
            Some: pair => Math.Min(
                pair.link.AswSi.Over(pair.spacing.Value) * 0.9 * depthSi * pair.link.FywdSi * c.CotTheta.Value,
                pair.link.VrdMaxSi),
            None: () => PlainShear(c, shearAreaSi));

    // --- [GOVERNING] -------------------------------------------------------------------

    public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Structural request, GeometrySource geometry, AssessmentSink sink, IClock clock) =>
        from code   in DesignCode.For(request.Route)
        from model  in Project(graph, FrameInputs.Of(request), geometry)
        from _      in Validate(model, code)
        from resp   in Solve(model, clock)
        from bytes  in Artifact(resp, model.Joint, None)
        from blob   in sink.Store(bytes)
        let checks   = model.Members.Bind(m => Check(m, resp[m.Id], code, model.Policy))
        from folded in CheckFacts(checks)
        from result in AssessmentResult.Of(
            request.Route, folded.Facts, folded.Governing, clock.GetCurrentInstant(), StaticKey, resultArtifact: Some(blob))
        select result;

    static Fin<(Seq<AssessmentFact> Facts, Option<double> Governing)> CheckFacts(Seq<MemberCheck> checks) {
        Seq<(MemberCheck Check, double Utilization)> resolved =
            toSeq(checks.Choose(static c => c.Utilization.Map(u => (Check: c, Utilization: u))).OrderByDescending(static row => row.Utilization));
        Option<(MemberCheck Check, double Utilization)> govern = resolved.Head;
        Seq<AssessmentFact> inapplicable = checks.Filter(static c => c.Utilization.IsNone)
            .Map(static c => AssessmentFact.Text($"{c.Member.Value}/{c.State.Key}", AssessmentVerdict.NotApplicable.Key));
        return from ratios in resolved.TraverseM(static row => AssessmentFact.Ratio($"{row.Check.Member.Value}/{row.Check.State.Key}", row.Utilization)).As()
               from governing in govern.Traverse(static g => AssessmentFact.Ratio(MaxUtilizationFact, g.Utilization)).As()
               select (ratios + inapplicable + governing.ToSeq() + govern.Map(static g => Seq(
                            AssessmentFact.Reference(GoverningMemberFact, g.Check.Member),
                            AssessmentFact.Text(GoverningStateFact, g.Check.State.Key))).IfNone(Seq<AssessmentFact>()),
                       govern.Map(static g => g.Utilization));
    }

    const string MaxUtilizationFact   = "max-utilization";
    const string GoverningMemberFact  = "governing-member";
    const string GoverningStateFact   = "governing-limit-state";

    static Fin<Unit> Validate(FrameModel model, DesignCode code) =>
        model.Members
            .Choose(m => MaterialFamily.Classify(m.Strength, m.Directional) is FamilyEvidence evidence && !evidence.Admits(code.Family)
                ? Some((Member: m.Id, Evidence: evidence))
                : None)
            .Head
            .Match(Some: row => Fin.Fail<Unit>(new ComputeFault.AssessmentInputMissing(
                       AssessmentInputReason.AssessmentUnusable, $"{row.Member.Value}:{row.Evidence.Witness}!={code.Family.Key}")),
                   None: () => Fin.Succ(unit));

    static Seq<MemberCheck> Check(StructuralMember member, MemberResponse response, DesignCode code, StructuralPolicy policy) {
        CapacityContext ctx = CapacityContext.Of(member, code, policy);
        Option<MemberCapacity> caps =
            from tension in Capacity(code, LimitState.AxialTension, ctx)
            from compression in Capacity(code, LimitState.AxialCompression, ctx)
            from flexureMajor in Capacity(code, LimitState.FlexureMajor, ctx)
            from flexureMinor in Capacity(code, LimitState.FlexureMinor, ctx)
            select new MemberCapacity(tension, compression, flexureMajor, flexureMinor);
        return toSeq(LimitState.Items).Filter(state => state.Applies(code)).Map(state => {
            Option<double> capacity = Capacity(code, state, ctx);
            Option<double> demand = Admit.Demand(response);
            Option<double> util =
                state == LimitState.Combined ? caps.Map(operands => Math.Max(
                    code.Interaction(response.TensionCorner, operands, ctx), code.Interaction(response.CompressionCorner, operands, ctx)))
                : state == LimitState.Deflection ? Some(response.MaxDeflection.Over(policy.DeflectionLimitRatio.Value * member.Length))
                : from value in capacity from d in demand select d.Over(value);
            return new MemberCheck(member.Id, state, demand, capacity, util);
        });
    }

    static Fin<ReadOnlyMemory<byte>> Artifact(
        FrozenDictionary<NodeId, MemberResponse> responses, Tolerance quantum,
        Option<(ReadOnlyMemory<double> Shapes, int Modes, int Dofs, Seq<double> Periods)> modal) {
        double Q(double value) => Math.Round(value / quantum.Value) * quantum.Value;
        Seq<(NodeId Id, MemberResponse Response)> ordered =
            toSeq(responses.OrderBy(static row => row.Key.ToValue(), StringComparer.Ordinal)).Map(static row => (Id: row.Key, Response: row.Value));
        double[,] demands = new double[ordered.Count, MemberResponse.Columns];
        Span2D<double> block = demands.AsSpan2D();
        for (int row = 0; row < ordered.Count; row++) {
            MemberResponse response = ordered[row].Response;
            Span<double> line = block.GetRowSpan(row);
            response.Min.WriteRow(line, Q);
            response.Max.WriteRow(line[SectionDemand.Columns..], Q);
            line[^1] = Q(response.MaxDeflection);
        }

        H5DatasetCreation creation = HdfArchivePolicy.Interchange.Creation();
        H5File graph = new() { ["demands"] = new H5Dataset<double[,]>(demands, chunks: [1u, (uint)MemberResponse.Columns], datasetCreation: creation) };
        graph.Attributes["members"] = ordered.Map(static row => row.Id.ToValue()).ToArray();
        Option<(H5Dataset<double[]> Slot, ChunkGrid Grid)> modeSlot = modal.Map(m => {
            ulong[] fileDims = [(ulong)m.Modes, (ulong)m.Dofs];
            uint[] chunkShape = [1u, (uint)m.Dofs];
            H5Dataset<double[]> slot = new(fileDims: fileDims, chunks: chunkShape, datasetCreation: creation);
            graph["modes"] = slot;
            graph["periods"] = new H5Dataset<double[]>(m.Periods.ToArray(), chunks: [(uint)Math.Max(1, m.Periods.Count)], datasetCreation: creation);
            return (Slot: slot, Grid: ChunkGrid.Seat(fileDims, chunkShape));
        });
        using MemoryStream staged = new();
        Fin<Unit> written;
        using (HdfWriter writer = HdfArchive.Begin(graph, staged, HdfArchivePolicy.Interchange)) {
            written = (from m in modal from seat in modeSlot select (m, seat)).Match(
                Some: pair => {
                    ChunkCursor<double> cursor = writer.Open(pair.seat.Slot, pair.seat.Grid);
                    return toSeq(Enumerable.Range(0, pair.m.Modes))
                        .TraverseM(mode => cursor.Write(pair.m.Shapes.Span.Slice(mode * pair.m.Dofs, pair.m.Dofs).ToArray()))
                        .As().Map(static _ => unit);
                },
                None: static () => Fin.Succ(unit));
        }

        return written.Map(_ => (ReadOnlyMemory<byte>)staged.ToArray());
    }
}
```

## [03]-[SEISMIC_ROUTE]

- Owner: `DesignSpectrum` `[SmartEnum<string>]` the code design-spectrum rows — EN 1998-1 Type 1, EN 1998-1 Type 2, ASCE 7 — each row carrying its piecewise pseudo-acceleration ordinate as a delegate over the `SpectrumPolicy` parameters AND its own `GroundShape` table, NEVER a hardcoded curve; `GroundShape` the per-ground-type `(S, T_B, T_C, T_D)` row; `SpectrumPolicy` the site/ground-motion/behaviour/damping parameter record; `ExcitationAxis` `[SmartEnum<string>]` the direction the request excites, each row carrying its own projection off the per-axis `ModalParticipation`; `ModalCombination` `[SmartEnum<string>]` the modal-combination axis (`srss` · `cqc`) and `ModalCorrelation` the once-per-solve cross-modal matrix both rows fold through; `SeismicSpec` the request payload carrying the spectrum row, its policy, the excitation axis, the combination row, the retained-mode count, the condensation policy, the participation floor, and the CAPACITY `DesignCode` the member checks run under; `Run` the seismic overload folding the condensed modal pencil.
- Entry: `public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Seismic request, GeometrySource geometry, AssessmentSink sink, IClock clock)` — `FrameLowering.Lower` builds the same mesh the static route uses and lowers it as `PhysicsKind.FeaModal` so the lane routes its eigen arm; `SolveLane.Solve` under `LanePolicy.CanonicalModal` over `SolveRoute.Condensed` condenses the frame's inertia-free rotational rows out of the pencil and recovers full-length `(φ, λ)` with the per-axis `ModalParticipation` factors off the owned lumped-mass field; the effective-mass floor gates TYPED AND PER AXIS — `Σ Γ_d² / TotalMass_d` for the axis `spec.Direction` names, an achieved fraction below `spec.ParticipationFloor` a `(Solve, Numeric)` `AnalysisFailed` naming the axis and the fraction, never a silent truncation; the per-mode spectral demand scales by that same axis's `Γ_d` and reads `Sa(T_i)` off the `DesignSpectrum` row; the modal responses combine through ONE `ModalCorrelation` built for the solve; and the combined demands check through the SAME `(DesignCode, LimitState)` capacity table under `spec.Capacity`. The achieved participation, the excitation axis, the combination key, and all FOUR reduction-evidence columns ride the fact stream, and `sink.Store` archives the modal basis on the SAME artifact shape the static route writes.
- Auto: the retained-mode count and the reduction cap are `SeismicSpec` columns the route hands the `Condensed` route payload, so the reduction budget is REQUEST data the content key folds rather than a lane default a caller cannot see; the correlation matrix is built once per solve and read as a `Span2D<double>` whose per-row combine is one `TensorPrimitives.Dot`, so the three flat `rho[i*modes+j]` walks and the nested double loop become one strided reduction; the per-mode displacement field leases ONE pooled buffer reused across modes rather than allocating per mode.
- Packages: `Solver/contract` (`SolveLane`, `LanePolicy.CanonicalModal`, `SolveRoute.Condensed`, `CondensationPolicy`, `CondensationEvidence`, `ModalParticipation`, `SolveResult`), `Analysis/frame` (`FrameLowering`, `StationRecovery`, `MemberResponse`), PureHDF, Generator.Equals (`[Equatable]`+`[OrderedEquality]`), CommunityToolkit.HighPerformance (`Span2D<double>`, `MemoryOwner<double>`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox (`FrozenDictionary`, `ImmutableArray`, `TensorPrimitives`) — zero new packages beyond the folder roster.
- Growth: a new code spectrum is one `DesignSpectrum` row carrying its ordinate delegate and its ground-type table; a new combination rule one `ModalCombination` row minting its own `ModalCorrelation`; a ground-type refinement one row in the spectrum's own table; a new excitation direction one `ExcitationAxis` row — a `SeismicAnalyzer` sibling runner is the rejected form, because this is a structural ROUTE over the existing spine.
- Boundary: the building-scale modal route is the condensed route — the reduction's necessity, its exactness over a lumped-mass frame, and the refuted eigensolver substrates are settled law at `RULINGS.md` `[02]`. Spectrum rows are POLICY DATA and a hardcoded curve, a per-code method ladder, or a spectrum baked into the runner is the deleted form; two rows differing only in their KEY are one row — EN 1998-1 Type 1 and Type 2 share one piecewise ordinate and differ in the Table 3.2/3.3 ground-type shapes alone, so the shape table is the row's own data and an unresolvable ground type fails at `Admit` before the demand fold. CQC is the closely-spaced default because SRSS under-combines correlated modes — the choice is a ROW the result records — and its cross-modal correlation depends only on the mode frequencies and the damping ratio, so it is built ONCE per solve. Participation gating runs PER EXCITATION AXIS because a total summed across axes reads healthy while the direction the request excites is unrepresented, and because a torsional mode carries no translational `Γ_d`; the shortfall is a typed `(Solve, Numeric)` fault, deterministic, cached as a Failed node under the lifecycle-spine law. Per the `RULINGS.md` `[02]` seismic action/capacity split, the capacity `DesignCode` rides the request.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
public readonly record struct GroundShape(double S, double Tb, double Tc, double Td) {
    public static readonly GroundShape Mapped = new(1.0, 0.0, 0.0, 0.0);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ExcitationAxis {
    public static readonly ExcitationAxis X = new("x", static p => p.X);
    public static readonly ExcitationAxis Y = new("y", static p => p.Y);
    public static readonly ExcitationAxis Z = new("z", static p => p.Z);

    [UseDelegateFromConstructor]
    public partial double Of(ModalParticipation participation);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DesignSpectrum {
    public static readonly DesignSpectrum En1998Type1 = new("en1998-type1", Eurocode, Type1Ground);
    public static readonly DesignSpectrum En1998Type2 = new("en1998-type2", Eurocode, Type2Ground);
    public static readonly DesignSpectrum Asce7       = new("asce7",        Asce,     FrozenDictionary<string, GroundShape>.Empty);

    public FrozenDictionary<string, GroundShape> Ground { get; }

    [UseDelegateFromConstructor]
    public partial double Sa(SpectrumPolicy policy, GroundShape ground, double period);

    public Fin<GroundShape> Admit(SpectrumPolicy policy) =>
        Ground.IsEmpty ? Fin.Succ(GroundShape.Mapped)
        : Ground.TryGetValue(policy.SiteClass, out GroundShape shape) ? Fin.Succ(shape)
        : Fin.Fail<GroundShape>(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input,
            $"<seismic-ground-type-unresolved:{Key}:{policy.SiteClass}>"));

    static readonly FrozenDictionary<string, GroundShape> Type1Ground = Shapes(
        ("a", 1.00, 0.15, 0.40, 2.0), ("b", 1.20, 0.15, 0.50, 2.0), ("c", 1.15, 0.20, 0.60, 2.0),
        ("d", 1.35, 0.20, 0.80, 2.0), ("e", 1.40, 0.15, 0.50, 2.0));

    static readonly FrozenDictionary<string, GroundShape> Type2Ground = Shapes(
        ("a", 1.00, 0.05, 0.25, 1.2), ("b", 1.35, 0.05, 0.25, 1.2), ("c", 1.50, 0.10, 0.25, 1.2),
        ("d", 1.80, 0.10, 0.30, 1.2), ("e", 1.60, 0.05, 0.25, 1.2));

    static FrozenDictionary<string, GroundShape> Shapes(params ReadOnlySpan<(string Ground, double S, double Tb, double Tc, double Td)> rows) =>
        Seq(rows).ToFrozenDictionary(static row => row.Ground, static row => new GroundShape(row.S, row.Tb, row.Tc, row.Td), StringComparer.Ordinal);

    static double Eurocode(SpectrumPolicy p, GroundShape g, double t) {
        double eta = Math.Max(Math.Sqrt(10.0.Over(5.0 + (100.0 * p.DampingRatio.Value))), 0.55);
        double ag = p.Pga.Value * g.S;
        return t <= g.Tb ? ag * (1.0 + (t.Over(g.Tb) * ((eta * 2.5).Over(p.Behavior.Value) - 1.0)))
            : t <= g.Tc ? ag * (eta * 2.5).Over(p.Behavior.Value)
            : t <= g.Td ? ag * (eta * 2.5).Over(p.Behavior.Value) * g.Tc.Over(t)
            : ag * (eta * 2.5).Over(p.Behavior.Value) * (g.Tc * g.Td).Over(t * t);
    }

    static double Asce(SpectrumPolicy p, GroundShape _, double t) =>
        t < 0.2 * p.T1.Value ? p.Sds.Value * (0.4 + (3.0 * t.Over(p.T1.Value))).Over(p.Behavior.Value)
        : t <= p.T1.Value ? p.Sds.Value.Over(p.Behavior.Value)
        : t <= p.TLong.Value ? p.Sd1.Value.Over(t * p.Behavior.Value)
        : p.Sd1.Value * p.TLong.Value.Over(t * t * p.Behavior.Value);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ModalCombination {
    public static readonly ModalCombination Srss = new("srss", static (_, _, modes) => ModalCorrelation.Identity(modes));
    public static readonly ModalCombination Cqc  = new("cqc",  static (omega, xi, _) => ModalCorrelation.Of(omega, xi));

    [UseDelegateFromConstructor]
    public partial ModalCorrelation Correlate(Seq<double> omega, double dampingRatio, int modes);
}

[Equatable]
public readonly partial record struct ModalCorrelation([property: OrderedEquality] ImmutableArray<double> Rho, int Modes) {
    public static ModalCorrelation Of(Seq<double> omega, double xi) {
        int modes = omega.Count;
        double[] rho = new double[modes * modes];
        Span2D<double> matrix = rho.AsSpan().AsSpan2D(modes, modes);
        for (int i = 0; i < modes; i++) {
            for (int j = 0; j < modes; j++) {
                double ratio = omega[j].Over(omega[i]);
                matrix[i, j] = (8.0 * xi * xi * (1.0 + ratio) * Math.Pow(ratio, 1.5))
                    .Over(Math.Pow(1.0 - (ratio * ratio), 2.0) + (4.0 * xi * xi * ratio * Math.Pow(1.0 + ratio, 2.0)));
            }
        }
        return new ModalCorrelation([.. rho], modes);
    }

    public static ModalCorrelation Identity(int modes) {
        double[] rho = new double[modes * modes];
        Span2D<double> matrix = rho.AsSpan().AsSpan2D(modes, modes);
        for (int i = 0; i < modes; i++) { matrix[i, i] = 1.0; }
        return new ModalCorrelation([.. rho], modes);
    }

    public double Combine(Seq<double> modal) {
        ReadOnlySpan2D<double> matrix = Rho.AsSpan().AsSpan2D(Modes, Modes);
        using SpanOwner<double> scratch = SpanOwner<double>.Allocate(Modes);
        Span<double> values = scratch.Span;
        for (int i = 0; i < Modes; i++) { values[i] = modal[i]; }
        double sum = 0.0;
        for (int i = 0; i < Modes; i++) { sum += values[i] * TensorPrimitives.Dot(matrix.GetRowSpan(i), values); }
        return Math.Sqrt(Math.Max(sum, 0.0));
    }
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SpectrumPolicy(
    string SiteClass, PositiveMagnitude Pga, PositiveMagnitude Sds, PositiveMagnitude Sd1,
    PositiveMagnitude T1, PositiveMagnitude TLong, PositiveMagnitude Behavior, UnitInterval DampingRatio);

public sealed record SeismicSpec(
    DesignSpectrum Spectrum, SpectrumPolicy Policy, ExcitationAxis Direction, ModalCombination Combination,
    DesignCode Capacity, Dimension Modes, CondensationPolicy Reduction, UnitInterval ParticipationFloor);

public static partial class StructuralAnalysis {
    const string RetainedFact     = "modal-retained-dofs";
    const string CondensedFact    = "modal-condensed-dofs";
    const string ReductionFact    = "modal-reduction-residual";
    const string ConditioningFact = "modal-pencil-conditioning";
    const string ExcitationFact   = "modal-excitation-axis";

    public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Seismic request, GeometrySource geometry, AssessmentSink sink, IClock clock) =>
        from ground    in request.Spec.Spectrum.Admit(request.Spec.Policy)
        from model     in Project(graph, FrameInputs.Of(request), geometry)
        from _         in Validate(model, request.Spec.Capacity)
        from lowered   in FrameLowering.Lower(model, clock)
        from problem   in lowered.Problem(PhysicsKind.FeaModal, LoadCombinationSpec.SeismicUnit)
        from modal     in SolveLane.Solve(problem, lowered.Mesh, LanePolicy.CanonicalModal,
                              new SolveRoute.Condensed(request.Spec.Modes, request.Spec.Reduction), clock)
        from gate      in Participation(modal, request.Spec)
        from reduction in modal.Condensation.ToFin(new ComputeFault.AnalysisFailed(
                              SolvePhase.Solve, FailureKind.Numeric, "<modal-reduction-evidence-absent>"))
        let periods = modal.EigenValues.Map(static values => toSeq(values.ToArray()).Map(static w2 => (2.0 * Math.PI).Over(Math.Sqrt(Math.Max(w2, EpsilonPolicy.BandUlp))))).IfNone(Seq<double>())
        from demands   in SpectralDemands(model, lowered, modal, request.Spec, ground, periods)
        from bytes     in Artifact(demands, model.Joint,
                              Some((modal.Field, periods.Count, periods.Count > 0 ? modal.Field.Length / periods.Count : 0, periods)))
        from blob      in sink.Store(bytes)
        let checks = model.Members.Bind(m => Check(m, demands[m.Id], request.Spec.Capacity, model.Policy))
        from folded    in CheckFacts(checks)
        from evidence  in AssessmentFact.Rows(
                              AssessmentFact.Ratio(Analysis.ParticipationFact, gate),
                              AssessmentFact.Ratio(RetainedFact, reduction.Retained),
                              AssessmentFact.Ratio(CondensedFact, reduction.Condensed),
                              AssessmentFact.Ratio(ReductionFact, reduction.Residual),
                              AssessmentFact.Ratio(ConditioningFact, reduction.Conditioning))
        from result    in AssessmentResult.Of(
                              request.Route,
                              folded.Facts + evidence + Seq(
                                  AssessmentFact.Text(Analysis.CombinationFact, request.Spec.Combination.Key),
                                  AssessmentFact.Text(ExcitationFact, request.Spec.Direction.Key)),
                              folded.Governing, clock.GetCurrentInstant(), SeismicKey, resultArtifact: Some(blob))
        select result;

    static Fin<double> Participation(SolveResult modal, SeismicSpec spec) =>
        modal.Participation
            .Bind(gammas => modal.TotalMass.Map(total => {
                double excited = toSeq(gammas.ToArray()).Sum(row => spec.Direction.Of(row) * spec.Direction.Of(row));
                return excited.Over(spec.Direction.Of(total));
            }))
            .ToFin(new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Numeric, "<modal-mass-shortfall:participation-stream-absent>"))
            .Bind(fraction => fraction >= spec.ParticipationFloor.Value
                ? Fin.Succ(fraction)
                : Fin.Fail<double>(new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Numeric,
                    $"<modal-mass-shortfall:axis={spec.Direction.Key}:achieved={fraction:0.000}:floor={spec.ParticipationFloor.Value:0.00}>")));

    static Fin<FrozenDictionary<NodeId, MemberResponse>> SpectralDemands(
        FrameModel model, FrameLowered lowered, SolveResult modal, SeismicSpec spec, GroundShape ground, Seq<double> periods) {
        ReadOnlyMemory<double> shapes = modal.Field;
        Seq<double> omegaSq = modal.EigenValues.Map(static v => toSeq(v.ToArray())).IfNone(Seq<double>());
        Seq<ModalParticipation> gammas = modal.Participation.Map(static v => toSeq(v.ToArray())).IfNone(Seq<ModalParticipation>());
        Seq<double> omega = omegaSq.Map(static w2 => Math.Sqrt(Math.Max(w2, EpsilonPolicy.BandUlp)));
        ModalCorrelation rho = spec.Combination.Correlate(omega, spec.Policy.DampingRatio.Value, omegaSq.Count);
        int dofs = omegaSq.Count > 0 ? shapes.Length / omegaSq.Count : shapes.Length;
        using MemoryOwner<double> scratch = MemoryOwner<double>.Allocate(dofs);
        return toSeq(Enumerable.Range(0, omegaSq.Count)).TraverseM(mode => {
            double scale = spec.Direction.Of(gammas[mode]) * spec.Spectrum.Sa(spec.Policy, ground, periods[mode]).Over(omegaSq[mode]);
            shapes.Span.Slice(mode * dofs, dofs).CopyTo(scratch.Span);
            TensorPrimitives.Multiply(scratch.Span, scale, scratch.Span);
            return StationRecovery.Demands(model, lowered, LoadCombinationSpec.SeismicUnit, scratch.Memory);
        }).As().Map(perMode => model.Members.Map(member => {
            Seq<MemberResponse> rows = perMode.Map(demands =>
                demands.Find(row => row.Id == member.Id).Map(static row => row.Response).IfNone(MemberResponse.Zero));
            double Combined(Func<SectionDemand, double> component) => rho.Combine(rows.Map(row => row.Span(component)));
            SectionDemand magnitude = new(
                Combined(static d => d.N), Combined(static d => d.Vy), Combined(static d => d.Vz),
                Combined(static d => d.My), Combined(static d => d.Mz), Combined(static d => d.T));
            return (member.Id, Response: new MemberResponse(-magnitude, magnitude, rho.Combine(rows.Map(static row => row.MaxDeflection))));
        }).ToFrozenDictionary(static row => row.Id, static row => row.Response));
    }
}
```
